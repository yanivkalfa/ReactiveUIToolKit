using System.Collections.Immutable;
using System.IO;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;

namespace UitkxLanguageServer;

public sealed class HoverHandler : IHoverHandler
{
    private readonly UitkxSchema   _schema;
    private readonly DocumentStore _store;
    private readonly WorkspaceIndex _index;

    public HoverHandler(UitkxSchema schema, DocumentStore store, WorkspaceIndex index)
    {
        _schema = schema;
        _store  = store;
        _index  = index;
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

        // Extract local path once; reused for disk-read fallback and AST parse.
        string localPath;
        try { localPath = new System.Uri(request.TextDocument.Uri.ToString()).LocalPath; }
        catch { localPath = string.Empty; }

        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
        {
            // VS2022 may have sent textDocument/didOpen before the server was ready.
            // Fall back to reading the file from disk.
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
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

        // Parse the document with the language-lib AST pipeline so hover is
        // derived from the real syntax tree instead of text scanning.

        var parseDiags  = new List<ReactiveUITK.Language.ParseDiagnostic>();
        var directives  = DirectiveParser.Parse(text, localPath, parseDiags);
        var nodes       = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        int line1 = (int)request.Position.Line + 1;
        int col0  = (int)request.Position.Character;
        var ctx = AstCursorContext.Find(parseResult, text, line1, col0);

        // For hover: `word` is what the cursor is on; `tagContext` is the
        // enclosing element (non-null only when cursor is on an attribute name).
        string word       = ctx.Word;
        string? tagContext = ctx.Kind == CursorKind.AttributeName ? ctx.TagName : null;

        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult<Hover?>(null);

        // 1. Is it a known workspace element?
        var elementInfo = _index.TryGetElementInfo(word);
        if (elementInfo is not null)
        {
            var props    = elementInfo.Props;
            var propList = props.Count == 0
                ? "_None_"
                : string.Join("\n", props.Take(8).Select(p =>
                    string.IsNullOrEmpty(p.XmlDoc)
                        ? $"- `{p.Name}`: `{p.Type}`"
                        : $"- `{p.Name}`: `{p.Type}` — {p.XmlDoc}"));
            var moreProps = props.Count > 8 ? $"\n- _+{props.Count - 8} more..._" : "";
            var md = $"## `<{word}>` \u2014 {word}Props\n\n"
                   + $"**Props**\n{propList}{moreProps}";
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent { Kind = MarkupKind.Markdown, Value = md }),
            });
        }

        // 2. Fallback to schema built-in element
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

        // 3. Is it an attribute name on a known tag?
        if (tagContext is not null)
        {
            // Prefer workspace prop info
            var workspaceProp = _index
                .GetProps(tagContext)
                .FirstOrDefault(p => p.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (workspaceProp is not null)
            {
                var doc = string.IsNullOrEmpty(workspaceProp.XmlDoc)
                    ? ""
                    : $"\n\n{workspaceProp.XmlDoc}";
                var md = $"## `{workspaceProp.Name}` : `{workspaceProp.Type}`{doc}";
                return Task.FromResult<Hover?>(new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = md }),
                });
            }

            // Fall back to schema attr
            var attr = _schema
                .GetAttributesForElement(tagContext)
                .FirstOrDefault(a => a.Name.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (attr is not null)
            {
                var md = $"## `{attr.Name}` : `{attr.Type}`\n\n{attr.Description}";
                return Task.FromResult<Hover?>(new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = md }),
                });
            }
        }

        // 4. Is it a directive?
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
