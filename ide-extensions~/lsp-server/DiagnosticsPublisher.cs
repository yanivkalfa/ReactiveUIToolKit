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

    private void RevalidateOpenDocuments()
    {
        foreach (var (uriString, text) in _documentStore.GetAll())
        {
            if (!uriString.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                var docUri = DocumentUri.From(uriString);
                Publish(docUri, text, roslynHost: null);
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
                var jsxNodes = UitkxParser.Parse(text, localPath, jsxDirectives, parseDiags);
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
        var t2Diags = _analyzer.Analyze(parseResult, localPath, projectElements, knownAttributes);

        // NOTE: T2 element/attribute checks for setup JSX are already covered
        // by the main Analyze path — CanonicalLowering hoists setup code into a
        // CodeBlockNode whose ReturnMarkups contain the JSX elements, and
        // WalkNode walks into those ReturnMarkups.  Running AnalyzeNodes on the
        // separately-parsed setupJsxNodes would produce duplicate diagnostics.

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

        var t1t2 = filteredT1.Concat(t2Diags).ToList();
        if (!string.IsNullOrEmpty(localPath))
            _lastT1T2[localPath] = t1t2;

        // Carry forward the last T3 diagnostics so the error list doesn't
        // flash empty during the 300ms debounce gap before Roslyn rebuilds.
        IEnumerable<ParseDiagnostic> combined = t1t2;
        if (
            !string.IsNullOrEmpty(localPath)
            && _lastT3.TryGetValue(localPath, out var cachedT3)
            && cachedT3.Count > 0
        )
        {
            combined = t1t2.Concat(cachedT3);
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

            ServerLog.Log(
                $"[Diagnostics] T3 push '{System.IO.Path.GetFileName(uitkxFilePath)}': "
                    + $"{t3.Count} Roslyn diagnostic(s), {combined.Count} total."
            );
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[Diagnostics] PushTier3 error: {ex.Message}");
        }
    }

    // â”€â”€ Private helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
