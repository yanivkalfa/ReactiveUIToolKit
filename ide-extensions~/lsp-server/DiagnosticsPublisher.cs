using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Lowering;
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
    private readonly ILanguageServerFacade  _server;
    private readonly UitkxSchema           _schema;
    private readonly WorkspaceIndex        _index;
    private readonly DocumentStore         _documentStore;
    private readonly DiagnosticsAnalyzer   _analyzer     = new DiagnosticsAnalyzer();
    private readonly RoslynDiagnosticMapper _roslynMapper = new RoslynDiagnosticMapper();

    // Per-URI snapshot of the last T1+T2 diagnostics pushed.
    // Key = local file path (normalised), Value = diagnostic list.
    private readonly ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>> _lastT1T2 =
        new ConcurrentDictionary<string, IReadOnlyList<ParseDiagnostic>>(StringComparer.Ordinal);

    public DiagnosticsPublisher(ILanguageServerFacade server, UitkxSchema schema, WorkspaceIndex index, DocumentStore documentStore)
    {
        _server        = server;
        _schema        = schema;
        _index         = index;
        _documentStore = documentStore;

        // When the background workspace scan finishes, re-validate every open .uitkx
        // document so that components indexed after initial open are no longer flagged
        // as unknown elements.
        _index.ScanCompleted += () =>
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
                    ServerLog.Log($"[Diagnostics] ScanCompleted re-publish error: {ex.Message}");
                }
            }
        };
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
        var parseDiags  = new List<ParseDiagnostic>();
        var directives  = DirectiveParser.Parse(text, localPath, parseDiags);
        var parsedNodes = UitkxParser.Parse(text, localPath, directives, parseDiags);

        // Also validate UITKX markup embedded inside setup-code JSX blocks,
        // e.g. `var x = (<Box> @if (broken) { ... } </Box>)`.
        // These blocks are replaced by (object)null! in the Roslyn virtual doc,
        // so Roslyn never sees them — we must check them here at T1/T2 level.
        if (!directives.SetupCodeMarkupRanges.IsDefaultOrEmpty)
        {
            foreach (var (jsxStart, jsxEnd, jsxLine) in directives.SetupCodeMarkupRanges)
            {
                var jsxDirectives = directives with
                {
                    MarkupStartIndex = jsxStart,
                    MarkupEndIndex   = jsxEnd,
                    MarkupStartLine  = jsxLine,
                };
                UitkxParser.Parse(text, localPath, jsxDirectives, parseDiags);
            }
        }

        var nodes       = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, localPath);

        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        // â”€â”€ T2 structural analysis â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Seed projectElements with the component declared in this file so that
        // self-referencing components (e.g. recursive <DeepNode />) and sibling
        // components not yet reached by the async scan are never falsely flagged.
        var projectElements = BuildProjectElements(directives.ComponentName);
        var knownAttributes = BuildKnownAttributes(projectElements);
        var t2Diags = _analyzer.Analyze(parseResult, localPath, projectElements, knownAttributes);

        // â”€â”€ Combine T1 + T2 and push immediately â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var t1t2 = parseResult.Diagnostics.Concat(t2Diags).ToList();
        if (!string.IsNullOrEmpty(localPath))
            _lastT1T2[localPath] = t1t2;

        PushToClient(uri, t1t2);

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
        IReadOnlyList<(Microsoft.CodeAnalysis.Diagnostic Diagnostic, SourceMapEntry? MapEntry)>
            roslynDiags,
        string? uitkxSource = null)
    {
        try
        {
            // Map Roslyn diagnostics â†’ ParseDiagnostic
            var t3 = _roslynMapper.Map(roslynDiags, uitkxFilePath, uitkxSource);

            // Retrieve the last T1+T2 snapshot for this file (may be missing
            // if Publish hasn't run yet â€” that's fine, an empty list is safe).
            _lastT1T2.TryGetValue(uitkxFilePath, out var t1t2);

            var combined = ((IEnumerable<ParseDiagnostic>)(t1t2 ?? Array.Empty<ParseDiagnostic>()))
                .Concat(t3)
                .ToList();

            DocumentUri uri = DocumentUri.File(uitkxFilePath);
            PushToClient(uri, combined);

            ServerLog.Log(
                $"[Diagnostics] T3 push '{System.IO.Path.GetFileName(uitkxFilePath)}': "
                + $"{t3.Count} Roslyn diagnostic(s), {combined.Count} total.");
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
                Uri         = uri,
                Diagnostics = new Container<Diagnostic>(lspDiags),
            });
    }

    private static Diagnostic ToLsp(ParseDiagnostic d)
    {
        // SourceLine is 1-based; LSP is 0-based.
        int startLine = Math.Max(0, d.SourceLine - 1);
        int startChar = Math.Max(0, d.SourceColumn);

        // EndLine/EndColumn are 0 when not tracked â†’ fall back to same position.
        int endLine = d.EndLine > 0 ? Math.Max(0, d.EndLine - 1) : startLine;
        // Ensure end is always strictly past start so VS Code renders a visible squiggle.
        int endChar =
            d.EndColumn > 0 ? Math.Max(d.EndColumn, startChar + 1)
            : startChar + 1;

        return new Diagnostic
        {
            Range = new LspRange(
                new Position(startLine, startChar),
                new Position(endLine,   endChar)),
            Severity = ToLspSeverity(d.Severity),
            Code     = (DiagnosticCode)d.Code,
            Source   = "uitkx",
            Message  = d.Message,
            Tags     = d.Code == DiagnosticCodes.UnreachableAfterReturn
                ? new Container<DiagnosticTag>(DiagnosticTag.Unnecessary)
                : null,
        };
    }

    private static LspDiagnosticSeverity ToLspSeverity(ParseSeverity s) =>
        s switch
        {
            ParseSeverity.Error       => LspDiagnosticSeverity.Error,
            ParseSeverity.Warning     => LspDiagnosticSeverity.Warning,
            ParseSeverity.Information => LspDiagnosticSeverity.Information,
            ParseSeverity.Hint        => LspDiagnosticSeverity.Hint,
            _                         => LspDiagnosticSeverity.Information,
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
        HashSet<string> projectElements)
    {
        var result = new Dictionary<string, IReadOnlyCollection<string>>(
            StringComparer.OrdinalIgnoreCase);

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
            var attrs = props
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var ua in _schema.Root.UniversalAttributes)
                attrs.Add(ua.Name);
            result[tagName] = attrs;
        }

        return result;
    }
}
