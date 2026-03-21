using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;
using UitkxLanguageServer.Roslyn;

namespace UitkxLanguageServer;

public sealed class HoverHandler : IHoverHandler
{
    private readonly UitkxSchema _schema;
    private readonly DocumentStore _store;
    private readonly WorkspaceIndex _index;
    private readonly RoslynHost _roslynHost;

    public HoverHandler(
        UitkxSchema schema,
        DocumentStore store,
        WorkspaceIndex index,
        RoslynHost roslynHost
    )
    {
        _schema = schema;
        _store = store;
        _index = index;
        _roslynHost = roslynHost;
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

        // 0. Roslyn type-info hover — runs first for C# expression positions
        //    because Roslyn gives the most accurate type information.
        if (!string.IsNullOrEmpty(localPath))
        {
            try
            {
                _roslynHost
                    .EnsureReadyAsync(localPath, text, parseResult, cancellationToken)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (OperationCanceledException) { throw; }
            catch { /* workspace not ready — fall through to non-Roslyn hover */ }

            var vdoc = _roslynHost.GetVirtualDocument(localPath);
            if (vdoc != null && vdoc.Map.ToVirtualOffset(offset).HasValue)
            {
                var roslynHover = TryGetRoslynHover(localPath, offset, vdoc, cancellationToken);
                if (roslynHover != null)
                    return Task.FromResult<Hover?>(roslynHover);
            }
        }
        // 1. Is it a known workspace element?
        var elementInfo = _index.TryGetElementInfo(word);
        if (elementInfo is not null)
        {
            var ownProps = elementInfo.OwnProps;
            var propList =
                ownProps.Count == 0
                    ? "_None_"
                    : string.Join(
                        "\n",
                        ownProps.Select(p =>
                            string.IsNullOrEmpty(p.XmlDoc)
                                ? $"- `{p.Name}`: `{p.Type}`"
                                : $"- `{p.Name}`: `{p.Type}` — {p.XmlDoc}"
                        )
                    );
            var inheritedCount = elementInfo.Props.Count - ownProps.Count;
            var moreProps =
                inheritedCount > 0 && elementInfo.BaseElement is not null
                    ? $"\n\n*+ {inheritedCount} inherited from {elementInfo.BaseElement}Props*"
                    : "";
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
            var elementAttrs = element.Attributes;
            var universalAttrs = _schema.Root.UniversalAttributes;
            var attrList =
                elementAttrs.Count == 0
                    ? "_None_"
                    : string.Join(
                        "\n",
                        elementAttrs.Select(a =>
                            string.IsNullOrEmpty(a.Description)
                                ? $"- `{a.Name}`: `{a.Type}`"
                                : $"- `{a.Name}`: `{a.Type}` — {a.Description}"
                        )
                    );
            if (universalAttrs.Count > 0)
                attrList +=
                    "\n\n**Common attributes**\n"
                    + string.Join(
                        "\n",
                        universalAttrs.Select(a =>
                            string.IsNullOrEmpty(a.Description)
                                ? $"- `{a.Name}`: `{a.Type}`"
                                : $"- `{a.Name}`: `{a.Type}` — {a.Description}"
                        )
                    );
            var moreAttrs = "";
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
        if (TryGetHookSetterHover(text, offset, word, out var setterMd))
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

    private Hover? TryGetRoslynHover(
        string localPath,
        int uitkxOffset,
        ReactiveUITK.Language.Roslyn.VirtualDocument vdoc,
        CancellationToken ct
    )
    {
        try
        {
            var virtualResult = vdoc.Map.ToVirtualOffset(uitkxOffset);
            if (!virtualResult.HasValue)
                return null;

            int virtualOffset = virtualResult.Value.VirtualOffset;

            var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
            if (roslynDoc == null)
                return null;

            // Use synchronous GetAwaiter().GetResult() — HoverHandler.Handle is synchronous.
            // This is safe: we run on a thread-pool thread, not the LSP message pump,
            // and the Roslyn documents are already computed by EnsureReadyAsync.
#pragma warning disable VSTHRD002
            var syntaxRoot = roslynDoc.GetSyntaxRootAsync(ct).GetAwaiter().GetResult();
            var semanticModel = roslynDoc.GetSemanticModelAsync(ct).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (syntaxRoot == null || semanticModel == null)
                return null;

            // Find the syntax token at the cursor position.
            int pos = virtualOffset > 0 ? virtualOffset - 1 : 0;
            var token = syntaxRoot.FindToken(pos);
            if (token.Parent == null)
                return null;

            // Try to get type information for the node under the cursor.
            var typeInfo = semanticModel.GetTypeInfo(token.Parent, ct);
            var symbolInfo = semanticModel.GetSymbolInfo(token.Parent, ct);

            var sym =
                symbolInfo.Symbol
                ?? (symbolInfo.CandidateSymbols.Length > 0 ? symbolInfo.CandidateSymbols[0] : null);

            ITypeSymbol? type = typeInfo.Type ?? typeInfo.ConvertedType;

            if (sym == null && type == null)
                return null;

            // Build the hover markdown from symbol / type information.
            string md;
            if (sym != null)
            {
                var display = SanitizeInternalTypes(sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                var kind = sym.Kind.ToString().ToLowerInvariant();
                md = $"**({kind})** `{display}`";

                // Append type if it's different from the display string.
                if (type != null)
                {
                    var typeDisplay = SanitizeInternalTypes(type.ToDisplayString(
                        SymbolDisplayFormat.MinimallyQualifiedFormat
                    ));
                    if (!display.Contains(typeDisplay))
                        md = $"**({kind})** `{sym.Name}` : `{typeDisplay}`";
                }

                // Append XML doc summary if present.
                var xml = sym.GetDocumentationCommentXml(cancellationToken: ct);
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    var summary = ExtractXmlSummary(xml);
                    if (!string.IsNullOrWhiteSpace(summary))
                        md += $"\n\n{summary}";
                }
            }
            else
            {
                var typeDisplay = SanitizeInternalTypes(type!.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat
                ));
                md = $"`{typeDisplay}`";
            }

            ServerLog.Log(
                $"[HoverHandler] Roslyn hover: {md.Substring(0, Math.Min(80, md.Length))}"
            );

            return new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent { Kind = MarkupKind.Markdown, Value = md }
                ),
            };
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[HoverHandler] Roslyn hover error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Replaces internal virtual-document stub type names with user-friendly equivalents.
    /// </summary>
    private static string SanitizeInternalTypes(string display)
    {
        display = Regex.Replace(display, @"__UitkxRef__<(.+?)>", "Ref<$1>");
        display = Regex.Replace(display, @"__StateSetter__<(.+?)>", "Action<Func<$1, $1>>");
        display = display.Replace("__UitkxRef__", "Ref");
        display = display.Replace("__StateSetter__", "StateSetter");
        return display;
    }

    private static string ExtractXmlSummary(string xml)
    {
        var m = Regex.Match(xml, @"<summary>(.*?)</summary>", RegexOptions.Singleline);
        if (!m.Success)
            return string.Empty;
        return Regex.Replace(m.Groups[1].Value.Trim(), @"\s+", " ");
    }

    private static bool TryGetHookSetterHover(
        string text,
        int offset,
        string hoveredWord,
        out string markdown
    )
    {
        markdown = string.Empty;
        if (string.IsNullOrWhiteSpace(hoveredWord))
            return false;

        var matches = s_hookTupleRegex.Matches(text);
        foreach (Match match in matches)
        {
            var setterGroup = match.Groups["setter"];
            var hookGroup = match.Groups["hook"];
            if (!setterGroup.Success || !hookGroup.Success)
                continue;

            if (!setterGroup.Value.Equals(hoveredWord, StringComparison.Ordinal))
                continue;

            int setterStart = setterGroup.Index;
            int setterEnd = setterStart + setterGroup.Length;
            bool offsetOnDeclaration = offset >= setterStart && offset <= setterEnd;
            if (!offsetOnDeclaration)
            {
                // Allow hover on any usage of the setter identifier in the file,
                // not only on its tuple declaration.
                int wordStart = offset;
                while (
                    wordStart > 0
                    && (char.IsLetterOrDigit(text[wordStart - 1]) || text[wordStart - 1] == '_')
                )
                    wordStart--;
                int wordEnd = offset;
                while (
                    wordEnd < text.Length
                    && (char.IsLetterOrDigit(text[wordEnd]) || text[wordEnd] == '_')
                )
                    wordEnd++;

                if (wordEnd <= wordStart)
                    continue;

                string wordAtOffset = text.Substring(wordStart, wordEnd - wordStart);
                if (!wordAtOffset.Equals(hoveredWord, StringComparison.Ordinal))
                    continue;
            }

            if (
                !offsetOnDeclaration
                && !hoveredWord.Equals(setterGroup.Value, StringComparison.Ordinal)
            )
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
