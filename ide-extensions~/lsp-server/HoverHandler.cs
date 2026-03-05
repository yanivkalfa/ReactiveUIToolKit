using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;

namespace UitkxLanguageServer;

public sealed class HoverHandler : IHoverHandler
{
    private readonly UitkxSchema _schema;
    private readonly DocumentStore _store;
    private readonly WorkspaceIndex _index;

    public HoverHandler(UitkxSchema schema, DocumentStore store, WorkspaceIndex index)
    {
        _schema = schema;
        _store = store;
        _index = index;
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
        try
        {
            localPath = new System.Uri(request.TextDocument.Uri.ToString()).LocalPath;
        }
        catch
        {
            localPath = string.Empty;
        }

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

        var parseDiags = new List<ReactiveUITK.Language.ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, localPath, parseDiags);
        var nodes = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags)
        );

        int line1 = (int)request.Position.Line + 1;
        int col0 = (int)request.Position.Character;
        var ctx = AstCursorContext.Find(parseResult, text, line1, col0);

        // For hover: `word` is what the cursor is on; `tagContext` is the
        // enclosing element (non-null only when cursor is on an attribute name).
        string word = ctx.Word;
        string? tagContext = ctx.Kind == CursorKind.AttributeName ? ctx.TagName : null;
        int offset = ToOffset(text, request.Position);

        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult<Hover?>(null);

        // 1. Is it a known workspace element?
        var elementInfo = _index.TryGetElementInfo(word);
        if (elementInfo is not null)
        {
            var props = elementInfo.Props;
            var propList =
                props.Count == 0
                    ? "_None_"
                    : string.Join(
                        "\n",
                        props
                            .Take(8)
                            .Select(p =>
                                string.IsNullOrEmpty(p.XmlDoc)
                                    ? $"- `{p.Name}`: `{p.Type}`"
                                    : $"- `{p.Name}`: `{p.Type}` — {p.XmlDoc}"
                            )
                    );
            var moreProps = props.Count > 8 ? $"\n- _+{props.Count - 8} more..._" : "";
            var md = $"## `<{word}>` \u2014 {word}Props\n\n" + $"**Props**\n{propList}{moreProps}";
            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                    ),
                }
            );
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
                return Task.FromResult<Hover?>(
                    new Hover
                    {
                        Contents = new MarkedStringsOrMarkupContent(
                            new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                        ),
                    }
                );
            }

            // Fall back to schema attr
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

        // 5. Is it a hook setter variable from tuple destructuring?
        if (TryGetHookSetterHover(text, offset, out var setterMd))
            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = setterMd }
                    ),
                }
            );

        // 6. Is it a hook name (camelCase shorthand or qualified Hooks.Use*)?
        string hookLookup = word;
        // Also match if the word is e.g. "UseState" (PascalCase bare name)
        if (!s_hookDocs.ContainsKey(hookLookup) && char.IsUpper(hookLookup[0]))
            hookLookup = "Hooks." + hookLookup;
        if (s_hookDocs.TryGetValue(hookLookup, out var hookMd))
            return Task.FromResult<Hover?>(
                new Hover
                {
                    Contents = new MarkedStringsOrMarkupContent(
                        new MarkupContent { Kind = MarkupKind.Markdown, Value = hookMd }
                    ),
                }
            );

        return Task.FromResult<Hover?>(null);
    }

    private static bool TryGetHookSetterHover(string text, int offset, out string markdown)
    {
        markdown = string.Empty;

        var matches = s_hookTupleRegex.Matches(text);
        foreach (Match match in matches)
        {
            var setterGroup = match.Groups["setter"];
            var hookGroup = match.Groups["hook"];
            if (!setterGroup.Success || !hookGroup.Success)
                continue;

            int setterStart = setterGroup.Index;
            int setterEnd = setterStart + setterGroup.Length;
            if (offset < setterStart || offset > setterEnd)
                continue;

            string hookName = hookGroup.Value;
            string shortHookName = hookName.StartsWith("Hooks.", StringComparison.Ordinal)
                ? hookName.Substring("Hooks.".Length)
                : hookName;
            string setterName = setterGroup.Value;

            markdown =
                $"## `{setterName}`\n\n"
                + $"Setter returned by `{shortHookName}`. Calling it schedules a re-render with the new value.\n\n"
                + "```csharp\n"
                + $"{setterName}(nextValue);\n"
                + "```";
            return true;
        }

        return false;
    }

    // ── Hook documentation ──────────────────────────────────────────────────

    private static readonly Dictionary<string, string> s_hookDocs = new Dictionary<string, string>(
        StringComparer.Ordinal
    )
    {
        ["useState"] =
            "## `useState<T>(initialValue)`\n\n**Shorthand for `Hooks.UseState`.** Returns a `(value, setter)` tuple.  \nCall `setter(newValue)` to schedule a re-render with the new state.\n\n```csharp\nvar (count, setCount) = useState(0);\n```",
        ["Hooks.UseState"] =
            "## `Hooks.UseState<T>(initialValue)`\n\nReturns a `(value, setter)` tuple.  \nCall `setter(newValue)` to schedule a re-render with the new state.\n\n```csharp\nvar (count, setCount) = Hooks.UseState(0);\n```",
        ["useEffect"] =
            "## `useEffect(action, deps?)`\n\n**Shorthand for `Hooks.UseEffect`.** Runs `action` after each render.  \nPass a `deps` array to run only when those values change.\n\n```csharp\nuseEffect(() => { /* side-effect */ }, new object[] { count });\n```",
        ["Hooks.UseEffect"] =
            "## `Hooks.UseEffect(action, deps?)`\n\nRuns `action` after each render.  \nPass a `deps` array to run only when those values change.\n\n```csharp\nHooks.UseEffect(() => { /* side-effect */ }, new object[] { count });\n```",
        ["useRef"] =
            "## `useRef<T>(initialValue?)`\n\n**Shorthand for `Hooks.UseRef`.** Returns a mutable ref object whose `.Current` persists across re-renders without causing a re-render on write.",
        ["Hooks.UseRef"] =
            "## `Hooks.UseRef<T>(initialValue?)`\n\nReturns a mutable ref object whose `.Current` persists across re-renders without causing a re-render on write.",
        ["useMemo"] =
            "## `useMemo<T>(factory, deps)`\n\n**Shorthand for `Hooks.UseMemo`.** Returns a memoised value. Re-computes `factory()` only when `deps` change.",
        ["Hooks.UseMemo"] =
            "## `Hooks.UseMemo<T>(factory, deps)`\n\nReturns a memoised value. Re-computes `factory()` only when `deps` change.",
        ["useCallback"] =
            "## `useCallback(fn, deps)`\n\n**Shorthand for `Hooks.UseCallback`.** Returns a memoised delegate. Re-creates `fn` only when `deps` change.",
        ["Hooks.UseCallback"] =
            "## `Hooks.UseCallback(fn, deps)`\n\nReturns a memoised delegate. Re-creates `fn` only when `deps` change.",
        ["useSignal"] =
            "## `useSignal<T>(initialValue)`\n\n**Shorthand for `Hooks.UseSignal`.** Like `useState` but backed by a reactive signal — updates propagate without a full re-render.",
        ["Hooks.UseSignal"] =
            "## `Hooks.UseSignal<T>(initialValue)`\n\nLike `UseState` but backed by a reactive signal — updates propagate without a full re-render.",
        ["useContext"] =
            "## `useContext<T>()`\n\n**Shorthand for `Hooks.UseContext`.** Reads the nearest context value of type `T` provided by a parent component.",
        ["Hooks.UseContext"] =
            "## `Hooks.UseContext<T>()`\n\nReads the nearest context value of type `T` provided by a parent component.",
        ["useReducer"] =
            "## `useReducer<TState, TAction>(reducer, initialState)`\n\n**Shorthand for `Hooks.UseReducer`.** Returns `(state, dispatch)`. Calls `reducer(state, action)` on each `dispatch(action)`.",
        ["Hooks.UseReducer"] =
            "## `Hooks.UseReducer<TState, TAction>(reducer, initialState)`\n\nReturns `(state, dispatch)`. Calls `reducer(state, action)` on each `dispatch(action)`.",
    };

    private static readonly Regex s_hookTupleRegex = new Regex(
        @"\bvar\s*\(\s*\w+\s*,\s*(?<setter>\w+)\s*\)\s*=\s*(?<hook>(?:Hooks\.)?[Uu]se[A-Za-z]+)\s*[<(]",
        RegexOptions.Compiled
    );

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
