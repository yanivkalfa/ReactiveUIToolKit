using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace UitkxLanguageServer;

public sealed class CompletionHandler : ICompletionHandler
{
    private readonly UitkxSchema _schema;
    private readonly DocumentStore _store;

    public CompletionHandler(UitkxSchema schema, DocumentStore store)
    {
        _schema = schema;
        _store = store;
    }

    public CompletionRegistrationOptions GetRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("uitkx"),
            TriggerCharacters = new Container<string>("<", "@", " ", "\n", "{"),
            ResolveProvider = false,
        };

    public Task<CompletionList> Handle(
        CompletionParams request,
        CancellationToken cancellationToken
    )
    {
        if (!_store.TryGet(request.TextDocument.Uri, out var text))
            return Task.FromResult(new CompletionList());

        var offset = ToOffset(text, request.Position);
        var context = DocumentContext.Detect(text, offset);

        var items = context.Kind switch
        {
            DocumentContext.CompletionKind.DirectiveName => DirectiveItems(context.Prefix),
            DocumentContext.CompletionKind.ControlFlowName => ControlFlowItems(context.Prefix),
            DocumentContext.CompletionKind.TagName => TagItems(context.Prefix),
            DocumentContext.CompletionKind.AttributeName => AttributeItems(
                context.TagName,
                context.Prefix
            ),
            _ => Enumerable.Empty<CompletionItem>(),
        };

        return Task.FromResult(new CompletionList(items));
    }

    // ── Completion item builders ─────────────────────────────────────────────

    private IEnumerable<CompletionItem> DirectiveItems(string prefix) =>
        _schema
            .Root.Directives.Where(d =>
                d.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            )
            .Select(d => new CompletionItem
            {
                Label = "@" + d.Name,
                Kind = CompletionItemKind.Keyword,
                InsertText = "@" + d.Name + " ",
                Detail = "UITKX directive",
                Documentation = new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = d.Description,
                },
            });

    private IEnumerable<CompletionItem> ControlFlowItems(string prefix) =>
        _schema
            .Root.ControlFlow.Where(d =>
                d.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            )
            .Select(d => new CompletionItem
            {
                Label = "@" + d.Name,
                Kind = CompletionItemKind.Keyword,
                InsertText = "@" + d.Name,
                Detail = "UITKX control flow",
                Documentation = new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = d.Description,
                },
            });

    private IEnumerable<CompletionItem> TagItems(string prefix) =>
        _schema
            .Root.Elements.Where(kv =>
                kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            )
            .Select(kv => new CompletionItem
            {
                Label = kv.Key,
                Kind = CompletionItemKind.Class,
                InsertText = BuildTagSnippet(kv.Key, kv.Value),
                InsertTextFormat = InsertTextFormat.Snippet,
                Detail = kv.Value.PropsType,
                Documentation = new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = kv.Value.Description,
                },
            });

    private IEnumerable<CompletionItem> AttributeItems(string tagName, string prefix) =>
        _schema
            .GetAttributesForElement(tagName)
            .Where(a => a.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(a => new CompletionItem
            {
                Label = a.Name,
                Kind = CompletionItemKind.Property,
                InsertText = a.Name + "=\"$1\"",
                InsertTextFormat = InsertTextFormat.Snippet,
                Detail = a.Type,
                Documentation = new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = a.Description,
                },
            });

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string BuildTagSnippet(string tagName, UitkxSchema.ElementInfo info) =>
        info.AcceptsChildren ? $"<{tagName}>$0</{tagName}>" : $"<{tagName} $1 />";

    private static int ToOffset(string text, Position position)
    {
        var line = (int)position.Line;
        var column = (int)position.Character;
        var offset = 0;
        for (var i = 0; i < line && offset < text.Length; i++)
        {
            var nl = text.IndexOf('\n', offset);
            offset = nl < 0 ? text.Length : nl + 1;
        }
        return Math.Min(offset + column, text.Length);
    }
}
