using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace UitkxLanguageServer;

public sealed class HoverHandler : IHoverHandler
{
    private readonly UitkxSchema _schema;
    private readonly DocumentStore _store;

    public HoverHandler(UitkxSchema schema, DocumentStore store)
    {
        _schema = schema;
        _store = store;
    }

    public HoverRegistrationOptions GetRegistrationOptions(
        HoverCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new HoverRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("uitkx"),
        };

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
            return Task.FromResult<Hover?>(null);

        var offset = ToOffset(text, request.Position);
        var (word, tagContext) = DocumentContext.WordAt(text, offset);

        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult<Hover?>(null);

        // 1. Is it an element tag?
        var element = _schema.TryGetElement(word);
        if (element is not null)
        {
            var attrs = string.Join(", ", element.Attributes.Select(a => $"`{a.Name}`"));
            var md =
                $"## `<{word}>` — {element.PropsType}\n\n"
                + $"{element.Description}\n\n"
                + $"**Accepts children:** {(element.AcceptsChildren ? "yes" : "no")}\n\n"
                + $"**Attributes:** {attrs}";
            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                    ),
                }
            );
        }

        // 2. Is it an attribute name on a known tag?
        if (tagContext is not null)
        {
            var attr = _schema
                .GetAttributesForElement(tagContext)
                .FirstOrDefault(a => a.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (attr is not null)
            {
                var md = $"## `{attr.Name}` : `{attr.Type}`\n\n{attr.Description}";
                return Task.FromResult<Hover?>(
                    new Hover
                    {
                        Contents = new MarkedStringsOrMarkupContent(
                            new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                        ),
                    }
                );
            }
        }

        // 3. Is it a directive?
        var directive = _schema
            .Root.Directives.Concat(_schema.Root.ControlFlow)
            .FirstOrDefault(d =>
                d.Name.Equals(word.TrimStart('@'), StringComparison.OrdinalIgnoreCase)
            );
        if (directive is not null)
        {
            var md = $"## `@{directive.Name}`\n\n{directive.Description}";
            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                    ),
                }
            );
        }

        return Task.FromResult<Hover?>(null);
    }

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
