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

    public SemanticTokensHandler(DocumentStore store)
    {
        _store = store;
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

    protected override Task Tokenize(
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        var docUri = identifier.TextDocument.Uri;

        if (!_store.TryGet(docUri, out var text) || string.IsNullOrEmpty(text))
            return Task.CompletedTask;

        string localPath = TryGetLocalPath(docUri) ?? string.Empty;

        var parseDiags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, localPath, parseDiags);
        var parsedNodes = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, localPath);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        var tokens = _provider.GetTokens(parseResult, text);

        foreach (var t in tokens)
        {
            if (t.Modifiers.Length == 0)
            {
                builder.Push(t.Line, t.Column, t.Length, t.TokenType);
            }
            else
            {
                builder.Push(t.Line, t.Column, t.Length, t.TokenType, t.Modifiers);
            }
        }

        return Task.CompletedTask;
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
