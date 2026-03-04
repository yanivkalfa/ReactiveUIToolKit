using System.IO;
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
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }
            ),
            TriggerCharacters = new Container<string>("<", "@", " ", "\n", "{"),
            ResolveProvider = false,
        };

    private static void Log(string msg) => ServerLog.Log(msg);

    public Task<CompletionList> Handle(
        CompletionParams request,
        CancellationToken cancellationToken
    )
    {
        Log(
            $"completion request: {request.TextDocument.Uri}  pos={request.Position.Line}:{request.Position.Character}"
        );

        if (!_store.TryGet(request.TextDocument.Uri, out var text))
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
                Log($"completion: loaded from disk ({text.Length} chars)");
            }
            else
            {
                Log($"completion: store miss + disk miss — returning empty");
                return Task.FromResult(new CompletionList());
            }
        }

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
            DocumentContext.CompletionKind.AttributeValue => AttributeValueItems(
                context.TagName,
                context.AttributeName,
                context.Prefix
            ),
            _ => Enumerable.Empty<CompletionItem>(),
        };

        var list = items.ToList();
        Log($"completion: kind={context.Kind} prefix='{context.Prefix}' → {list.Count} items");
        return Task.FromResult(new CompletionList(list));
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
                InsertText = BuildDirectiveSnippet(d.Name),
                InsertTextFormat = InsertTextFormat.Snippet,
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
                InsertText = BuildControlFlowSnippet(d.Name),
                InsertTextFormat = InsertTextFormat.Snippet,
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
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
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

    private IEnumerable<CompletionItem> AttributeValueItems(
        string tagName,
        string attributeName,
        string prefix
    )
    {
        var attr = _schema
            .GetAttributesForElement(tagName)
            .FirstOrDefault(a => a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        if (attr is null)
            return Enumerable.Empty<CompletionItem>();

        var type = attr.Type.ToLowerInvariant();

        if (type is "bool" or "boolean")
        {
            return new[]
            {
                ValueItem("true", "bool", "true"),
                ValueItem("false", "bool", "false"),
            }.Where(i => i.Label.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        if (type is "int" or "long" or "short" or "byte" or "uint" or "ulong")
        {
            return new[] { ValueItem("0", attr.Type, "0") };
        }

        if (type is "float" or "double" or "decimal")
        {
            return new[] { ValueItem("0.0", attr.Type, "0.0") };
        }

        if (type == "action")
        {
            return new[]
            {
                new CompletionItem
                {
                    Label = "() => { }",
                    Kind = CompletionItemKind.Function,
                    InsertText = "() => { $0 }",
                    InsertTextFormat = InsertTextFormat.Snippet,
                    Detail = attr.Type,
                    Documentation = new MarkupContent
                    {
                        Kind = MarkupKind.Markdown,
                        Value = "Inline callback expression.",
                    },
                },
            };
        }

        if (type is "object" or "style")
        {
            return new[] { ValueItem("{ }", attr.Type, "{ $0 }", true) };
        }

        return Enumerable.Empty<CompletionItem>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string BuildTagSnippet(string tagName, UitkxSchema.ElementInfo info) =>
        info.AcceptsChildren ? $"{tagName}>$0</{tagName}>" : $"{tagName} $1 />";

    private static string BuildDirectiveSnippet(string name) =>
        name switch
        {
            "namespace" => "@namespace $1",
            "component" => "@component $1",
            "using" => "@using $1",
            "props" => "@props $1",
            "key" => "@key $1",
            "code" => "@code\n{\n\t$0\n}",
            _ => "@" + name + " $1",
        };

    private static string BuildControlFlowSnippet(string name) =>
        name switch
        {
            "if" => "@if ($1)\n{\n\t$0\n}",
            "else" => "@else\n{\n\t$0\n}",
            "foreach" => "@foreach (var item in $1)\n{\n\t$0\n}",
            "switch" => "@switch ($1)\n{\n\t@case $2 => $0\n}",
            "case" => "@case $1 => $0",
            "default" => "@default => $0",
            "code" => "@code\n{\n\t$0\n}",
            "for" => "@for (int $1 = 0; $1 < $2; $1++)\n{\n\t$0\n}",
            "while" => "@while ($1)\n{\n\t$0\n}",
            _ => "@" + name,
        };

    private static CompletionItem ValueItem(
        string label,
        string detail,
        string insertText,
        bool snippet = false
    ) =>
        new()
        {
            Label = label,
            Kind = CompletionItemKind.Value,
            InsertText = insertText,
            InsertTextFormat = snippet ? InsertTextFormat.Snippet : InsertTextFormat.PlainText,
            Detail = detail,
        };

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
