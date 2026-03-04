using System.IO;
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
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }
            ),
        };

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        ServerLog.Log(
            $"hover request: {request.TextDocument.Uri}  pos={request.Position.Line}:{request.Position.Character}"
        );

        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
        {
            // VS2022 may have sent textDocument/didOpen before the server was ready.
            // Fall back to reading the file from disk.
            string? localPath = null;
            try
            {
                localPath = new System.Uri(request.TextDocument.Uri.ToString()).LocalPath;
            }
            catch { }

            if (localPath != null && File.Exists(localPath))
            {
                text = File.ReadAllText(localPath);
                _store.Set(request.TextDocument.Uri, text);
                ServerLog.Log($"hover: loaded from disk ({text.Length} chars)");
            }
            else
            {
                ServerLog.Log($"hover: store miss + disk miss — returning null");
                return Task.FromResult<Hover?>(null);
            }
        }

        var offset = ToOffset(text, request.Position);
        var (word, tagContext) = DocumentContext.WordAt(text, offset);

        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult<Hover?>(null);

        // 1. Is it an element tag?
        var element = _schema.TryGetElement(word);
        if (element is not null)
        {
            var attrs = element.Attributes.Take(8).ToArray();
            var attrList =
                attrs.Length == 0
                    ? "_None_"
                    : string.Join("\n", attrs.Select(a => $"- `{a.Name}`: `{a.Type}`"));
            var moreAttrs =
                element.Attributes.Count > attrs.Length
                    ? $"\n- _+{element.Attributes.Count - attrs.Length} more..._"
                    : "";
            var md =
                $"## `<{word}>` — {element.PropsType}\n\n"
                + $"{element.Description}\n\n"
                + $"**Accepts children:** {(element.AcceptsChildren ? "yes" : "no")}\n\n"
                + $"**Attributes**\n{attrList}{moreAttrs}";
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
            var md =
                $"## `@{directive.Name}`\n\n{directive.Description}\n\n"
                + "```uitkx\n"
                + DirectiveExample(directive.Name)
                + "\n```";
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

    private static string DirectiveExample(string name) =>
        name switch
        {
            "namespace" => "@namespace My.Game.Ui",
            "component" => "@component InventoryPanel",
            "using" => "@using UnityEngine",
            "props" => "@props InventoryPanelProps",
            "if" => "@if (show)\n{\n    <Label text=\"Visible\" />\n}",
            "else" => "@else\n{\n    <Label text=\"Hidden\" />\n}",
            "foreach" => "@foreach (var item in items)\n{\n    <Label text={item.Name} />\n}",
            "switch" =>
                "@switch (mode)\n{\n    @case 0 => <Label text=\"A\" />\n    @default => <Label text=\"B\" />\n}",
            "case" => "@case 0 => <Label text=\"State\" />",
            "default" => "@default => <Label text=\"Fallback\" />",
            "code" => "@code\n{\n    var enabled = true;\n}",
            _ => "@" + name,
        };
}
