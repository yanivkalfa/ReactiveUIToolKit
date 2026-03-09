using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.SemanticTokens;
using UitkxLanguageServer.Roslyn;

namespace UitkxLanguageServer;

/// <summary>
/// Handles <c>textDocument/semanticTokens/full</c> (and delta / range) for
/// <c>.uitkx</c> files.  Token production is delegated to
/// <see cref="SemanticTokensProvider"/> in the language-lib.
/// </summary>
public sealed class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticTokensProvider _provider = new SemanticTokensProvider();
    private readonly RoslynSemanticTokensProvider _roslynProvider = new RoslynSemanticTokensProvider();
    private readonly RoslynHost _roslynHost;

    public SemanticTokensHandler(DocumentStore store, RoslynHost roslynHost)
    {
        _store      = store;
        _roslynHost = roslynHost;
    }

    // ── Registration ─────────────────────────────────────────────────────────

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new SemanticTokensRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }),
            Full   = true,
            Legend = new SemanticTokensLegend
            {
                TokenTypes = new Container<SemanticTokenType>(
                    System.Linq.Enumerable.Select(
                        SemanticTokenTypes.All,
                        t => new SemanticTokenType(t))),
                TokenModifiers = new Container<SemanticTokenModifier>(
                    System.Linq.Enumerable.Select(
                        SemanticTokenModifiers.All,
                        m => new SemanticTokenModifier(m))),
            },
        };
    }

    // ── SemanticTokensHandlerBase contract ───────────────────────────────────

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(
        ITextDocumentIdentifierParams @params,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
    }

    protected override async Task Tokenize(
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        var docUri = identifier.TextDocument.Uri;

        if (!_store.TryGet(docUri, out var text) || string.IsNullOrEmpty(text))
            return;

        string localPath = TryGetLocalPath(docUri) ?? string.Empty;

        var parseDiags  = new List<ParseDiagnostic>();
        var directives  = DirectiveParser.Parse(text, localPath, parseDiags);
        var parsedNodes = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var nodes       = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, localPath);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        // ── UITKX structural tokens ───────────────────────────────────────────
        var uitkxTokens = _provider.GetTokens(parseResult, text);

        // Build a lookup of positions already covered by UITKX tokens so that
        // Roslyn tokens at the same location are suppressed (overlap policy).
        var usedPositions = new HashSet<(int Line, int Col)>(uitkxTokens.Length);
        foreach (var t in uitkxTokens)
            usedPositions.Add((t.Line, t.Column));

        // ── Roslyn C# semantic tokens (async, best-effort) ────────────────────
        SemanticTokenData[] roslynTokens = Array.Empty<SemanticTokenData>();
        try
        {
            if (!string.IsNullOrEmpty(localPath))
            {
                var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
                var virtualDoc = _roslynHost.GetVirtualDocument(localPath);
                if (roslynDoc != null && virtualDoc != null)
                {
                    roslynTokens = await _roslynProvider
                        .GetTokensAsync(roslynDoc, virtualDoc.Map, text, usedPositions, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            ServerLog.Log($"[SemanticTokens] Roslyn token error: {ex.Message}");
        }

        // ── Merge and push to builder ─────────────────────────────────────────
        // Interleave UITKX and Roslyn tokens maintaining sort order.
        var all = MergeTokens(uitkxTokens, roslynTokens);

        foreach (var t in all)
        {
            if (t.Modifiers.Length == 0)
                builder.Push(t.Line, t.Column, t.Length, t.TokenType);
            else
                builder.Push(t.Line, t.Column, t.Length, t.TokenType, t.Modifiers);
        }
    }

    // ── Merge helper ──────────────────────────────────────────────────────────

    /// <summary>
    /// Merges two token arrays (each pre-sorted by line/col) into a single
    /// sort-ordered array using a two-pointer merge.  No deduplication is
    /// performed here — callers should already have excluded conflicting
    /// Roslyn tokens via <paramref name="existingPositions"/>.
    /// </summary>
    private static SemanticTokenData[] MergeTokens(
        SemanticTokenData[] primary,
        SemanticTokenData[] secondary)
    {
        if (secondary.Length == 0)
            return primary;
        if (primary.Length == 0)
            return secondary;

        var result = new SemanticTokenData[primary.Length + secondary.Length];
        int i = 0, j = 0, k = 0;

        while (i < primary.Length && j < secondary.Length)
        {
            var a = primary[i];
            var b = secondary[j];
            int cmp = a.Line != b.Line ? a.Line.CompareTo(b.Line) : a.Column.CompareTo(b.Column);
            if (cmp <= 0) { result[k++] = a; i++; }
            else          { result[k++] = b; j++; }
        }

        while (i < primary.Length)   result[k++] = primary[i++];
        while (j < secondary.Length) result[k++] = secondary[j++];

        return result;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string? TryGetLocalPath(DocumentUri uri)
    {
        try
        {
            return new System.Uri(uri.ToString()).LocalPath;
        }
        catch
        {
            return null;
        }
    }
}
