using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using UitkxLanguageServer.Roslyn;
using LspDiagnosticSeverity = OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// Parses a .uitkx document, runs all diagnostic tiers, and pushes the results
/// to the LSP client as a <c>textDocument/publishDiagnostics</c> notification.
///
/// <b>Tier overview:</b>
/// <list type="bullet">
///   <item>Tier 1 (parser syntax) â€” from <see cref="ParseResult.Diagnostics"/>.</item>
///   <item>Tier 2 (structural)    â€” produced by <see cref="DiagnosticsAnalyzer"/>.</item>
///   <item>Tier 3 (Roslyn / C#)  â€” produced asynchronously by <see cref="RoslynHost"/>
///     after the virtual document is compiled.  Pushed via <see cref="PushTier3"/>.</item>
/// </list>
///
/// <b>Push flow:</b>
/// <list type="number">
///   <item><c>textDocument/didOpen|didChange</c> â†’ <see cref="Publish"/> â†’
///     T1+T2 computed synchronously and pushed immediately.</item>
///   <item>In parallel, <see cref="RoslynHost.EnqueueRebuild"/> is queued.
///     ~300 ms later Roslyn compiles and calls <see cref="PushTier3"/>, which
///     merges T1+T2+T3 and re-pushes (replacing the previous notification).</item>
/// </list>
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly ILanguageServerFacade _server;
    private readonly UitkxSchema _schema;
    private readonly WorkspaceIndex _index;
    private readonly DocumentStore _documentStore;
    private readonly DiagnosticsAnalyzer _analyzer = new DiagnosticsAnalyzer();
    private readonly RoslynDiagnosticMapper _roslynMapper = new RoslynDiagnosticMapper();

    // Per-URI snapshot of the last T1+T2 diagnostics pushed.
    // Key = local file path (normalised), Value = diagnostic list.
    private readonly ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>> _lastT1T2 =
        new ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>>(StringComparer.Ordinal);

    // Per-URI snapshot of the last T3 (Roslyn) diagnostics.
    // Carried forward in T1+T2 pushes so the error list never flashes empty
    // during the 300ms debounce gap between edits and Roslyn rebuild.
    private readonly ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>> _lastT3 =
        new ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>>(StringComparer.Ordinal);

    // Debounce timer for IndexChanged → RevalidateOpenDocuments.
    // When many .cs files change in a burst (e.g. Unity recompilation),
    // each fires IndexChanged individually; we coalesce into one revalidation.
    private CancellationTokenSource? _revalidateCts;

    // Lazily resolved via SetRoslynHost so that RevalidateOpenDocuments can
    // trigger full T3 Roslyn rebuilds (not just T1+T2).
    private Roslyn.RoslynHost? _roslynHost;

    public DiagnosticsPublisher(
        ILanguageServerFacade server,
        UitkxSchema schema,
        WorkspaceIndex index,
        DocumentStore documentStore
    )
    {
        _server = server;
        _schema = schema;
        _index = index;
        _documentStore = documentStore;

        // When the background workspace scan finishes, re-validate every open .uitkx
        // document immediately so that components indexed after initial open are no
        // longer flagged as unknown elements.
        _index.ScanCompleted += RevalidateOpenDocuments;

        // When individual files change (didChangeWatchedFiles), debounce the
        // revalidation so a burst of .cs changes produces only one re-publish.
        _index.IndexChanged += ScheduleDebouncedRevalidation;
    }

    /// <summary>
    /// Sets the <see cref="Roslyn.RoslynHost"/> used by
    /// <see cref="RevalidateOpenDocuments"/> so it can trigger full T3
    /// Roslyn rebuilds (not just T1+T2).  Called once after all singletons
    /// have been resolved to avoid a circular-dependency in DI.
    /// </summary>
    public void SetRoslynHost(Roslyn.RoslynHost roslynHost)
    {
        _roslynHost = roslynHost;
    }

    private void RevalidateOpenDocuments()
    {
        foreach (var (uriString, text) in _documentStore.GetAll())
        {
            if (!uriString.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                var docUri = DocumentUri.From(uriString);
                Publish(docUri, text, _roslynHost);
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[Diagnostics] index re-publish error: {ex.Message}");
            }
        }
    }

    private void ScheduleDebouncedRevalidation()
    {
        _revalidateCts?.Cancel();
        var cts = new CancellationTokenSource();
        _revalidateCts = cts;
        _ = Task.Delay(500, cts.Token).ContinueWith(_ =>
        {
            RevalidateOpenDocuments();
        }, cts.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    // â”€â”€ Tier 1 + 2: immediate synchronous push â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Parses <paramref name="text"/>, runs Tier-1 and Tier-2 analysis, and
    /// immediately pushes the resulting diagnostics to the client.
    ///
    /// Also enqueues the Roslyn T3 rebuild on <paramref name="roslynHost"/>
    /// (pass <c>null</c> to skip, e.g. in unit tests).
    /// </summary>
    /// <returns>The <see cref="ParseResult"/> produced during this invocation.</returns>
    public ParseResult Publish(DocumentUri uri, string text, RoslynHost? roslynHost = null)
    {
        string localPath = GetLocalPath(uri) ?? string.Empty;

        // â”€â”€ Parse â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var parseDiags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, localPath, parseDiags);
        var parsedNodes = UitkxParser.Parse(text, localPath, directives, parseDiags);

        // Also validate UITKX markup embedded inside setup-code JSX blocks,
        // e.g. `var x = (<Box> @if (broken) { ... } </Box>)`.
        // These blocks are replaced by (object)null! in the Roslyn virtual doc,
        // so Roslyn never sees them — we must check them here at T1/T2 level.
        var setupJsxNodes = ImmutableArray<AstNode>.Empty;
        var allSetupJsxRanges = directives.SetupCodeMarkupRanges;
        if (!directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
        {
            allSetupJsxRanges = allSetupJsxRanges.IsDefaultOrEmpty
                ? directives.SetupCodeBareJsxRanges
                : allSetupJsxRanges.AddRange(directives.SetupCodeBareJsxRanges);
        }
        if (!allSetupJsxRanges.IsDefaultOrEmpty)
        {
            var setupBuilder = ImmutableArray.CreateBuilder<AstNode>();
            foreach (var (jsxStart, jsxEnd, jsxLine) in allSetupJsxRanges)
            {
                var jsxDirectives = directives with
                {
                    MarkupStartIndex = jsxStart,
                    MarkupEndIndex = jsxEnd,
                    MarkupStartLine = jsxLine,
                };
                var jsxNodes = UitkxParser.Parse(text, localPath, jsxDirectives, parseDiags, validateSingleRoot: true);
                setupBuilder.AddRange(jsxNodes);
            }
            setupJsxNodes = setupBuilder.ToImmutable();
        }

        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, localPath);

        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags)
        );

        // â”€â”€ T2 structural analysis â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Seed projectElements with the component declared in this file so that
        // self-referencing components (e.g. recursive <DeepNode />) and sibling
        // components not yet reached by the async scan are never falsely flagged.
        var projectElements = BuildProjectElements(directives.ComponentName);
        var knownAttributes = BuildKnownAttributes(projectElements);
        var t2Diags = _analyzer.Analyze(parseResult, localPath, projectElements, knownAttributes, text);

        // ── T2v version-compatibility diagnostics ────────────────────────────
        // Check elements and style properties against the detected Unity version.
        // Only produces diagnostics when schema entries carry sinceUnity annotations.
        var versionDiags = CheckVersionCompatibility(
            parsedNodes, roslynHost?.DetectedUnityVersion ?? UnityVersion.Unknown);


        // â”€â”€ Combine T1 + T2 and push immediately â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Suppress T1 parser diagnostics that fall inside unreachable regions
        // (e.g. UITKX2103 'Multiple top-level returns' when the second return
        // is unreachable after the first).
        var unreachableT2 = t2Diags
            .Where(d =>
                d.Code == DiagnosticCodes.UnreachableAfterReturn
                || d.Code == DiagnosticCodes.UnreachableAfterBreakOrContinue
            )
            .ToList();

        IEnumerable<ParseDiagnostic> filteredT1 = parseResult.Diagnostics;
        if (unreachableT2.Count > 0)
        {
            filteredT1 = parseResult.Diagnostics.Where(pd =>
            {
                foreach (var ur in unreachableT2)
                {
                    int urStart = ur.SourceLine;
                    int urEnd = ur.EndLine > 0 ? ur.EndLine : ur.SourceLine;
                    if (pd.SourceLine >= urStart && pd.SourceLine <= urEnd)
                        return false;
                }
                return true;
            });
        }

        var t1t2 = filteredT1.Concat(t2Diags).Concat(versionDiags).ToList();
        if (!string.IsNullOrEmpty(localPath))
            _lastT1T2[localPath] = t1t2;

        // Carry forward the last T3 diagnostics so the error list doesn't
        // flash empty during the 300ms debounce gap before Roslyn rebuilds.
        // BUT strip unreachable diagnostics from the carry-forward: they
        // cause stale gray when the user deletes a return statement.
        IEnumerable<ParseDiagnostic> combined = t1t2;
        if (
            !string.IsNullOrEmpty(localPath)
            && _lastT3.TryGetValue(localPath, out var cachedT3)
            && cachedT3.Count > 0
        )
        {
            var nonUnreachable = cachedT3.Where(d =>
                d.Code != "CS0162"
                && d.Code != DiagnosticCodes.UnreachableAfterReturn
                && d.Code != DiagnosticCodes.UnreachableAfterBreakOrContinue
            );
            combined = t1t2.Concat(nonUnreachable);
        }

        PushToClient(uri, combined);

        // â”€â”€ Kick off T3 Roslyn rebuild in the background â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (roslynHost != null && !string.IsNullOrEmpty(localPath))
            roslynHost.EnqueueRebuild(localPath, text, parseResult, this);

        return parseResult;
    }

    // â”€â”€ Tier 3: async Roslyn push â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Called by <see cref="RoslynHost"/> when the Roslyn compilation is ready.
    /// Re-pushes the combined T1+T2+T3 diagnostics as a single notification,
    /// replacing the earlier T1+T2-only notification for this file.
    /// </summary>
    public void PushTier3(
        string uitkxFilePath,
        IReadOnlyList<(
            Microsoft.CodeAnalysis.Diagnostic Diagnostic,
            SourceMapEntry? MapEntry
        )> roslynDiags,
        string? uitkxSource = null
    )
    {
        try
        {
            // Map Roslyn diagnostics → ParseDiagnostic
            var t3 = _roslynMapper.Map(roslynDiags, uitkxFilePath, uitkxSource);

            // Expand CS0162 (unreachable code) from a single statement to the
            // full unreachable range up to the enclosing closing `}`.
            if (!string.IsNullOrEmpty(uitkxSource))
                t3 = ExpandUnreachableRanges(t3, uitkxSource);

            // Retrieve the last T1+T2 snapshot for this file (may be missing
            // if Publish hasn't run yet — that's fine, an empty list is safe).
            _lastT1T2.TryGetValue(uitkxFilePath, out var t1t2);

            // Suppress Roslyn warnings/errors that fall within unreachable
            // regions (UITKX0107/UITKX0110).  Those diagnostics (CS8321
            // "local function never used", CS0219 "variable never used", etc.)
            // are false‐positives caused by dead code after return — the
            // unreachable hint and fade are sufficient.
            var unreachableRanges = (t1t2 ?? Array.Empty<ParseDiagnostic>())
                .Where(d =>
                    d.Code == DiagnosticCodes.UnreachableAfterReturn
                    || d.Code == DiagnosticCodes.UnreachableAfterBreakOrContinue
                )
                .ToList();

            IEnumerable<ParseDiagnostic> filteredT3 = t3;
            if (unreachableRanges.Count > 0)
            {
                filteredT3 = t3.Where(rd =>
                {
                    // Drop any Roslyn diagnostic whose start line is inside
                    // an unreachable range (including CS0162, to avoid
                    // double-marking with our own UITKX0107).
                    foreach (var ur in unreachableRanges)
                    {
                        int urStart = ur.SourceLine;
                        int urEnd = ur.EndLine > 0 ? ur.EndLine : ur.SourceLine;
                        if (rd.SourceLine >= urStart && rd.SourceLine <= urEnd)
                            return false;
                    }
                    return true;
                });
            }

            var filteredT3List = filteredT3.ToList();

            // Cache the T3 snapshot so it can be carried forward in T1+T2 pushes.
            _lastT3[uitkxFilePath] = filteredT3List;

            var combined = ((IEnumerable<ParseDiagnostic>)(t1t2 ?? Array.Empty<ParseDiagnostic>()))
                .Concat(filteredT3List)
                .ToList();

            DocumentUri uri = DocumentUri.File(uitkxFilePath);
            PushToClient(uri, combined);
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[Diagnostics] PushTier3 error: {ex.Message}");
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Expands CS0162 diagnostics from a single statement to the full
    /// unreachable range within the enclosing scope block.
    /// </summary>
    private static IReadOnlyList<ParseDiagnostic> ExpandUnreachableRanges(
        IReadOnlyList<ParseDiagnostic> diagnostics, string source)
    {
        var result = new List<ParseDiagnostic>(diagnostics.Count);
        var lineOffsets = BuildLineOffsets(source);

        foreach (var d in diagnostics)
        {
            if (d.Code != "CS0162")
            {
                result.Add(d);
                continue;
            }

            int endLine = FindEnclosingScopeEnd(source, lineOffsets, d.SourceLine);
            if (endLine > d.SourceLine)
            {
                // Re-emit as UITKX0107 with Hint severity so it gets identical
                // treatment to our own T2 unreachable diagnostic: dim-only, no
                // squiggly, and proper Unnecessary tag in ToLsp().
                result.Add(new ParseDiagnostic
                {
                    Code = DiagnosticCodes.UnreachableAfterReturn,
                    Severity = ParseSeverity.Hint,
                    Message = "Unreachable code after 'return'.",
                    SourceLine = d.SourceLine,
                    SourceColumn = 0,
                    EndLine = endLine - 1,
                    EndColumn = 9999,
                });
            }
            else
            {
                // Single-statement fallback: still convert to UITKX0107.
                result.Add(new ParseDiagnostic
                {
                    Code = DiagnosticCodes.UnreachableAfterReturn,
                    Severity = ParseSeverity.Hint,
                    Message = "Unreachable code after 'return'.",
                    SourceLine = d.SourceLine,
                    SourceColumn = d.SourceColumn,
                    EndLine = d.EndLine,
                    EndColumn = d.EndColumn,
                });
            }
        }

        return result;
    }

    private static List<int> BuildLineOffsets(string source)
    {
        var offsets = new List<int> { 0 };
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == '\n')
                offsets.Add(i + 1);
        }
        return offsets;
    }

    /// <summary>
    /// From <paramref name="startLine"/> (1-based), scans forward to find the
    /// closing <c>}</c> at brace depth 0, skipping strings, comments, and
    /// nested blocks. Returns the 1-based line of that <c>}</c>.
    /// </summary>
    private static int FindEnclosingScopeEnd(string source, List<int> lineOffsets, int startLine)
    {
        if (startLine < 1 || startLine > lineOffsets.Count)
            return startLine;

        int i = lineOffsets[startLine - 1];
        int braceDepth = 0;
        int currentLine = startLine;

        while (i < source.Length)
        {
            char c = source[i];
            if (c == '\n') { currentLine++; i++; continue; }
            if (c == '\r') { i++; continue; }

            // Skip strings
            if (c == '"')
            {
                i++;
                if (i + 1 < source.Length && source[i] == '"' && source[i + 1] == '"')
                {
                    i += 2;
                    while (i + 2 < source.Length)
                    {
                        if (source[i] == '\n') currentLine++;
                        if (source[i] == '"' && source[i + 1] == '"' && source[i + 2] == '"')
                        { i += 3; break; }
                        i++;
                    }
                    continue;
                }
                while (i < source.Length && source[i] != '"' && source[i] != '\n')
                {
                    if (source[i] == '\\') i++;
                    i++;
                }
                if (i < source.Length && source[i] == '"') i++;
                continue;
            }
            if (c == '\'')
            {
                i++;
                while (i < source.Length && source[i] != '\'' && source[i] != '\n')
                {
                    if (source[i] == '\\') i++;
                    i++;
                }
                if (i < source.Length && source[i] == '\'') i++;
                continue;
            }
            if (c == '$' && i + 1 < source.Length && source[i + 1] == '"')
            {
                i += 2;
                while (i < source.Length && source[i] != '"' && source[i] != '\n')
                {
                    if (source[i] == '\\') i++;
                    i++;
                }
                if (i < source.Length && source[i] == '"') i++;
                continue;
            }

            // Skip comments
            if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n') i++;
                continue;
            }
            if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < source.Length)
                {
                    if (source[i] == '\n') currentLine++;
                    if (source[i] == '*' && source[i + 1] == '/')
                    { i += 2; break; }
                    i++;
                }
                continue;
            }

            if (c == '{') { braceDepth++; i++; continue; }
            if (c == '}')
            {
                if (braceDepth > 0) { braceDepth--; i++; continue; }
                return currentLine;
            }

            i++;
        }

        return startLine;
    }

    private void PushToClient(DocumentUri uri, IEnumerable<ParseDiagnostic> diagnostics)
    {
        var lspDiags = diagnostics.Select(ToLsp).ToArray();
        _server.TextDocument.PublishDiagnostics(
            new PublishDiagnosticsParams
            {
                Uri = uri,
                Diagnostics = new Container<Diagnostic>(lspDiags),
            }
        );
    }

    private static Diagnostic ToLsp(ParseDiagnostic d)
    {
        // SourceLine is 1-based; LSP is 0-based.
        int startLine = Math.Max(0, d.SourceLine - 1);
        int startChar = Math.Max(0, d.SourceColumn);

        // EndLine/EndColumn are 0 when not tracked â†’ fall back to same position.
        int endLine = d.EndLine > 0 ? Math.Max(0, d.EndLine - 1) : startLine;
        // Ensure end is always strictly past start so VS Code renders a visible squiggle.
        int endChar = d.EndColumn > 0 ? Math.Max(d.EndColumn, startChar + 1) : startChar + 1;

        return new Diagnostic
        {
            Range = new LspRange(
                new Position(startLine, startChar),
                new Position(endLine, endChar)
            ),
            Severity = ToLspSeverity(d.Severity),
            Code = (DiagnosticCode)d.Code,
            Source = "uitkx",
            Message = d.Message,
            Tags =
                d.Code == DiagnosticCodes.UnreachableAfterReturn
                || d.Code == DiagnosticCodes.UnreachableAfterBreakOrContinue
                || d.Code == "CS0162" // Roslyn: Unreachable code detected
                || d.Code == DiagnosticCodes.UnusedParameter // UITKX0111
                || d.Code == "CS0219" // Roslyn: unused local variable
                || d.Code == "CS8321" // Roslyn: unused local function
                    ? new Container<DiagnosticTag>(DiagnosticTag.Unnecessary)
                    : null,
        };
    }

    private static LspDiagnosticSeverity ToLspSeverity(ParseSeverity s) =>
        s switch
        {
            ParseSeverity.Error => LspDiagnosticSeverity.Error,
            ParseSeverity.Warning => LspDiagnosticSeverity.Warning,
            ParseSeverity.Information => LspDiagnosticSeverity.Information,
            ParseSeverity.Hint => LspDiagnosticSeverity.Hint,
            _ => LspDiagnosticSeverity.Information,
        };

    private static string? GetLocalPath(DocumentUri uri)
    {
        try
        {
            var sysUri = new Uri(uri.ToString());
            return sysUri.IsFile ? sysUri.LocalPath : null;
        }
        catch
        {
            return null;
        }
    }

    // ── Version-compatibility diagnostics ──────────────────────────────────

    /// <summary>
    /// Walks the parsed element tree and emits <see cref="DiagnosticCodes.VersionMismatch"/>
    /// warnings for any elements (or style properties) whose schema annotations
    /// indicate they require a newer Unity version than the one detected in the project.
    /// Returns an empty list when the user version is unknown or no annotations exist.
    /// </summary>
    private List<ParseDiagnostic> CheckVersionCompatibility(
        ImmutableArray<AstNode> nodes, UnityVersion userVersion)
    {
        var diags = new List<ParseDiagnostic>();
        if (!userVersion.IsKnown)
            return diags;

        foreach (var node in nodes)
            WalkForVersionDiags(node, userVersion, diags);
        return diags;
    }

    private void WalkForVersionDiags(AstNode node, UnityVersion userVersion, List<ParseDiagnostic> diags)
    {
        if (node is ElementNode el)
        {
            // Check element version requirement
            var elMinVersion = _schema.GetElementMinVersion(el.TagName);
            if (elMinVersion.IsKnown && userVersion < elMinVersion)
            {
                diags.Add(new ParseDiagnostic
                {
                    Code = DiagnosticCodes.VersionMismatch,
                    Severity = ParseSeverity.Warning,
                    Message = $"Element '<{el.TagName}>' requires {elMinVersion.ToDisplayString()}+, "
                            + $"but this project targets {userVersion.ToDisplayString()}.",
                    SourceLine = el.SourceLine,
                    SourceColumn = el.SourceColumn + 1, // +1 to point past '<'
                    EndLine = el.SourceLine,
                    EndColumn = el.SourceColumn + 1 + el.TagName.Length,
                });
            }

            // Check attribute version requirements
            foreach (var attr in el.Attributes)
            {
                var schemaAttr = _schema.GetAttributesForElement(el.TagName)
                    .FirstOrDefault(a => a.Name.Equals(attr.Name, StringComparison.OrdinalIgnoreCase));
                if (schemaAttr?.SinceUnity is not null
                    && UnityVersion.TryParse(schemaAttr.SinceUnity, out var attrMinVersion)
                    && userVersion < attrMinVersion)
                {
                    diags.Add(new ParseDiagnostic
                    {
                        Code = DiagnosticCodes.VersionMismatch,
                        Severity = ParseSeverity.Warning,
                        Message = $"Attribute '{attr.Name}' on '<{el.TagName}>' requires {attrMinVersion.ToDisplayString()}+, "
                                + $"but this project targets {userVersion.ToDisplayString()}.",
                        SourceLine = attr.SourceLine,
                        SourceColumn = attr.SourceColumn,
                        EndLine = attr.SourceLine,
                        EndColumn = attr.NameEndColumn > 0 ? attr.NameEndColumn : attr.SourceColumn + attr.Name.Length,
                    });
                }
                if (schemaAttr?.RemovedIn is not null
                    && UnityVersion.TryParse(schemaAttr.RemovedIn, out var removedVersion)
                    && userVersion >= removedVersion)
                {
                    diags.Add(new ParseDiagnostic
                    {
                        Code = DiagnosticCodes.VersionMismatch,
                        Severity = ParseSeverity.Warning,
                        Message = $"Attribute '{attr.Name}' on '<{el.TagName}>' was removed in {removedVersion.ToDisplayString()}.",
                        SourceLine = attr.SourceLine,
                        SourceColumn = attr.SourceColumn,
                        EndLine = attr.SourceLine,
                        EndColumn = attr.NameEndColumn > 0 ? attr.NameEndColumn : attr.SourceColumn + attr.Name.Length,
                    });
                }
            }

            // Recurse into children
            foreach (var child in el.Children)
                WalkForVersionDiags(child, userVersion, diags);
        }
    }

    // ── Project element / attribute helpers ───────────────────────────────────

    /// <summary>
    /// Builds the combined set of known element names from the static schema
    /// (built-in UITKX elements) and the dynamic workspace index (*Props.cs scan).
    /// Used for UITKX0105 unknown-element checks.
    /// </summary>
    private HashSet<string> BuildProjectElements(string? ownComponentName = null)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in _schema.Root.Elements.Keys)
            set.Add(key);
        foreach (var e in _index.KnownElements)
            set.Add(e);
        // Always include the component declared in the file being validated so that
        // recursive self-references and race conditions with the async scan never
        // produce a false-positive UITKX0105 "Unknown element" diagnostic.
        if (!string.IsNullOrEmpty(ownComponentName))
            set.Add(ownComponentName);
        return set;
    }

    /// <summary>
    /// Builds a map of element-name → valid attribute names for the UITKX0109
    /// unknown-attribute check.
    /// Schema elements use the full attribute list from the schema JSON (including
    /// universal attributes).  Workspace elements use their prop names plus
    /// the schema's universal attributes.
    /// </summary>
    private IReadOnlyDictionary<string, IReadOnlyCollection<string>> BuildKnownAttributes(
        HashSet<string> projectElements
    )
    {
        var result = new Dictionary<string, IReadOnlyCollection<string>>(
            StringComparer.OrdinalIgnoreCase
        );

        // Schema elements — use the schema's per-element + universal attributes.
        foreach (var tagName in _schema.Root.Elements.Keys)
        {
            var attrs = _schema
                .GetAttributesForElement(tagName)
                .Select(a => a.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            result[tagName] = attrs;
        }

        // Workspace elements — prop names from *Props.cs + universal attributes.
        foreach (var tagName in _index.KnownElements)
        {
            if (result.ContainsKey(tagName))
                continue; // schema wins if there's a conflict

            var props = _index.GetProps(tagName);
            var attrs = props.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var ua in _schema.Root.UniversalAttributes)
                attrs.Add(ua.Name);
            result[tagName] = attrs;
        }

        return result;
    }
}
