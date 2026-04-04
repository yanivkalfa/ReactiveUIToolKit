using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Walks the AST produced by ReactiveUITK.Language.dll (accessed via reflection)
    /// and emits compilable C# source code equivalent to CSharpEmitter from the
    /// source generator — but without requiring Roslyn types.
    /// </summary>
    internal static class HmrCSharpEmitter
    {
        // ── Hardcoded tag → resolution map (mirrors PropsResolver.BuildFallbackMap) ─
        private static readonly Dictionary<string, TagRes> s_tagMap = new Dictionary<
            string,
            TagRes
        >(StringComparer.OrdinalIgnoreCase)
        {
            // Typed elements (no children)
            ["label"] = TagRes.Typed("Label", "LabelProps"),
            ["button"] = TagRes.Typed("Button", "ButtonProps"),
            ["textfield"] = TagRes.Typed("TextField", "TextFieldProps"),
            ["toggle"] = TagRes.Typed("Toggle", "ToggleProps"),
            ["slider"] = TagRes.Typed("Slider", "SliderProps"),
            ["sliderint"] = TagRes.Typed("SliderInt", "SliderIntProps"),
            ["image"] = TagRes.Typed("Image", "ImageProps"),
            ["listview"] = TagRes.Typed("ListView", "ListViewProps"),
            ["treeview"] = TagRes.Typed("TreeView", "TreeViewProps"),
            ["helpbox"] = TagRes.Typed("HelpBox", "HelpBoxProps"),
            ["progressbar"] = TagRes.Typed("ProgressBar", "ProgressBarProps"),
            ["tab"] = TagRes.Typed("Tab", "TabProps"),
            ["tabview"] = TagRes.Typed("TabView", "TabViewProps"),
            ["textelement"] = TagRes.Typed("TextElement", "TextElementProps"),
            ["radiobutton"] = TagRes.Typed("RadioButton", "RadioButtonProps"),
            ["radiobuttongroup"] = TagRes.Typed("RadioButtonGroup", "RadioButtonGroupProps"),
            ["dropdownfield"] = TagRes.Typed("DropdownField", "DropdownFieldProps"),
            ["enumfield"] = TagRes.Typed("EnumField", "EnumFieldProps"),
            ["integerfield"] = TagRes.Typed("IntegerField", "IntegerFieldProps"),
            ["floatfield"] = TagRes.Typed("FloatField", "FloatFieldProps"),
            // Typed containers (accept children)
            ["box"] = TagRes.TypedC("Box", "BoxProps"),
            ["scrollview"] = TagRes.TypedC("ScrollView", "ScrollViewProps"),
            ["foldout"] = TagRes.TypedC("Foldout", "FoldoutProps"),
            ["groupbox"] = TagRes.TypedC("GroupBox", "GroupBoxProps"),
            ["errorboundary"] = TagRes.TypedC("ErrorBoundary", "ErrorBoundaryProps"),
            // VisualElement is typed with children
            ["visualelement"] = TagRes.TypedC("VisualElement", "VisualElementProps"),
            // Dictionary-based (accept children)
            ["visualelementsafe"] = new TagRes(TagKind.Dict, "VisualElementSafe", null),
            // Text
            ["text"] = new TagRes(TagKind.Text, "Text", null),
            // Fragment
            ["fragment"] = new TagRes(TagKind.Fragment, "Fragment", null),
            // Suspense, Portal
            ["suspense"] = new TagRes(TagKind.Suspense, "Suspense", null),
            ["portal"] = new TagRes(TagKind.Portal, "Portal", null),
        };

        private static readonly Dictionary<string, string> s_componentAliases = new Dictionary<
            string,
            string
        >(StringComparer.Ordinal)
        {
            ["Router"] = "RouterFunc",
            ["Route"] = "RouteFunc",
            ["Link"] = "LinkFunc",
        };

        private const string QVNode = "global::ReactiveUITK.Core.VirtualNode";

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Parses a JSX markup fragment and returns the AST nodes (opaque IList).
        /// Signature: (jsxText, filePath, startLine) → IList of AST nodes.
        /// </summary>
        internal delegate IList MarkupParseFunc(string jsxText, string filePath, int startLine);

        /// <summary>
        /// Emit C# from the AST and directives produced by the Language parser.
        /// All parameters are opaque objects from the dynamically loaded Language.dll.
        /// </summary>
        public static string Emit(
            object directives,
            object rootNodes,
            string filePath,
            MarkupParseFunc parseMarkup = null)
        {
            var ctx = new EmitCtx(directives, filePath, parseMarkup);
            ctx.EmitFile(rootNodes);
            return ctx.ToString();
        }

        // ── Emit context (stateful per invocation) ────────────────────────────

        private sealed class EmitCtx
        {
            private readonly StringBuilder _sb = new StringBuilder(4096);
            private readonly object _directives;
            private readonly string _filePath;
            private readonly string _displayName;
            private readonly string _linePath;
            private readonly string _ns;
            private readonly string _componentName;
            private readonly string _propsTypeName;
            private readonly bool _isFunctionStyle;
            private readonly IList _usings;
            private readonly IList _ussFiles;
            private readonly IList _functionParams;
            private readonly IList _injects;
            private readonly MarkupParseFunc _parseMarkup;
            private bool _isRootElement = true;

            public EmitCtx(object directives, string filePath, MarkupParseFunc parseMarkup)
            {
                _directives = directives;
                _filePath = filePath;
                _parseMarkup = parseMarkup;
                _displayName = Path.GetFileName(filePath);
                _linePath = filePath.Replace("\\", "/");
                _ns = GP<string>(directives, "Namespace") ?? "UITKX.Generated";
                _componentName = GP<string>(directives, "ComponentName") ?? "Unknown";
                _propsTypeName = GP<string>(directives, "PropsTypeName");
                _isFunctionStyle = GP<bool>(directives, "IsFunctionStyle");
                _usings = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(directives, "Usings"));
                _ussFiles = UitkxHmrCompiler.GetItems(
                    UitkxHmrCompiler.GetProp(directives, "UssFiles")
                );
                _functionParams = UitkxHmrCompiler.GetItems(
                    UitkxHmrCompiler.GetProp(directives, "FunctionParams")
                );
                _injects = UitkxHmrCompiler.GetItems(
                    UitkxHmrCompiler.GetProp(directives, "Injects")
                );
            }

            public override string ToString() => _sb.ToString();

            public void EmitFile(object rootNodes)
            {
                var nodes = UitkxHmrCompiler.GetItems(rootNodes);

                var markupNodes = new List<object>();
                foreach (var n in nodes)
                {
                    string typeName = n.GetType().Name;
                    if (typeName == "JsxCommentNode")
                        continue;
                    else
                        markupNodes.Add(n);
                }

                // Header
                L("// <auto-generated — do not edit — HMR />");
                L("#pragma warning disable CS0105,CS8600,CS8601,CS8602,CS8603,CS8604");
                L("");
                L("using System;");
                L("using System.Collections.Generic;");
                L("using System.Linq;");
                L("using ReactiveUITK;");
                L("using ReactiveUITK.Core;");
                L("using ReactiveUITK.Core.Animation;");
                L("using ReactiveUITK.Props.Typed;");
                L("using static ReactiveUITK.Props.Typed.StyleKeys;");
                L("using static ReactiveUITK.Props.Typed.CssHelpers;");
                L("using static ReactiveUITK.AssetHelpers;");
                L("using UColor = UnityEngine.Color;");
                foreach (var u in _usings)
                    L($"using {u};");
                L("using Color = UnityEngine.Color;");
                L("");

                // Namespace + class
                L($"namespace {_ns}");
                L("{");
                L(
                    $"    [global::ReactiveUITK.UitkxSource(@\"{_filePath.Replace("\"", "\"\"")}\")]"
                );
                L($"    [global::ReactiveUITK.UitkxElement(\"{_componentName}\")]");
                L($"    public partial class {_componentName}");
                L("    {");

                // Helper method
                EmitHelper();

                // @inject fields
                if (_injects.Count > 0)
                {
                    L("        // @inject fields");
                    foreach (var inj in _injects)
                    {
                        string injType =
                            GP<string>(inj, "Type")
                            ?? (string)inj.GetType().GetField("Item1")?.GetValue(inj)
                            ?? "object";
                        string injName =
                            GP<string>(inj, "Name")
                            ?? (string)inj.GetType().GetField("Item2")?.GetValue(inj)
                            ?? "_inject";
                        L($"        public static {injType} {injName};");
                    }
                    L("");
                }

                // @uss stylesheet keys
                if (_ussFiles.Count > 0)
                {
                    _sb.Append(
                        "        internal static readonly string[] __uitkx_ussKeys = new string[] { "
                    );
                    for (int i = 0; i < _ussFiles.Count; i++)
                    {
                        if (i > 0)
                            _sb.Append(", ");
                        string rawPath = _ussFiles[i]?.ToString() ?? "";
                        string resolved;
                        if (rawPath.StartsWith("./") || rawPath.StartsWith("../"))
                            resolved = ResolveRelativePath(rawPath, _filePath);
                        else
                            resolved = rawPath;
                        _sb.Append($"\"{resolved}\"");
                    }
                    L(" };");
                    L("");
                }

                // Render method
                L($"        public static {QVNode} Render(");
                L($"            global::ReactiveUITK.Core.IProps __rawProps,");
                L($"            IReadOnlyList<{QVNode}> __children)");
                L("        {");

                // Props binding — HMR uses reflection to read props from ANY
                // assembly's props type, avoiding cross-assembly type mismatch.
                if (_isFunctionStyle && _functionParams.Count > 0)
                {
                    foreach (var fp in _functionParams)
                    {
                        string fpName =
                            GP<string>(fp, "Name")
                            ?? (string)fp.GetType().GetField("Name")?.GetValue(fp)
                            ?? "p";
                        string fpType =
                            GP<string>(fp, "TypeAnnotation")
                            ?? GP<string>(fp, "Type")
                            ?? (string)fp.GetType().GetField("TypeAnnotation")?.GetValue(fp)
                            ?? "object";
                        string propName = char.ToUpper(fpName[0]) + fpName.Substring(1);
                        L(
                            $"            var {fpName} = __HmrProp<{fpType}>(__rawProps, \"{propName}\", default({fpType}));"
                        );
                    }
                    L("");
                }
                else if (_propsTypeName != null)
                {
                    L(
                        $"            var props = (__rawProps as {_propsTypeName}) ?? new {_propsTypeName}();"
                    );
                    L("");
                }

                // Function-style setup code
                string funcSetup = GP<string>(_directives, "FunctionSetupCode");
                if (!string.IsNullOrWhiteSpace(funcSetup))
                {
                    int setupLine = GP<int>(_directives, "FunctionSetupStartLine");
                    if (setupLine <= 0)
                        setupLine = GP<int>(_directives, "ComponentDeclarationLine");
                    L($"#line {setupLine} \"{_linePath}\"");
                    funcSetup = SpliceSetupCodeMarkup(funcSetup);
                    funcSetup = ApplyHookAliases(funcSetup);
                    if (funcSetup.Contains("Asset<") || funcSetup.Contains("Ast<"))
                        funcSetup = ResolveAssetPaths(funcSetup, _filePath);
                    _sb.Append(funcSetup);
                    if (!funcSetup.EndsWith("\n"))
                        _sb.AppendLine();
                }

                // Return expression
                if (markupNodes.Count == 0)
                {
                    L($"            return ({QVNode})null;");
                }
                else if (markupNodes.Count == 1)
                {
                    int srcLine = GP<int>(markupNodes[0], "SourceLine");
                    L($"#line {srcLine} \"{_linePath}\"");
                    _sb.Append("            return ");
                    EmitNode(markupNodes[0]);
                    _sb.AppendLine(";");
                }
                else
                {
                    int srcLine = GP<int>(markupNodes[0], "SourceLine");
                    L($"#line {srcLine} \"{_linePath}\"");
                    _sb.Append("            return V.Fragment(key: null, __C(");
                    for (int i = 0; i < markupNodes.Count; i++)
                    {
                        if (i > 0)
                            _sb.Append(", ");
                        EmitNode(markupNodes[i]);
                    }
                    _sb.AppendLine("));");
                }

                L("        }"); // close Render

                // Function-style auto-generated props class
                if (_isFunctionStyle && _functionParams.Count > 0)
                    EmitFunctionPropsClass();

                L("    }"); // close class
                L("}"); // close namespace
            }

            // ── Setup-code JSX splice ─────────────────────────────────────

            /// <summary>
            /// Replaces embedded JSX markup spans inside <paramref name="setupCode"/>
            /// with their emitted C# VirtualNode call equivalents.
            /// </summary>
            private string SpliceSetupCodeMarkup(string setupCode)
            {
                if (_parseMarkup == null)
                    return setupCode;

                // Read markup/bare ranges via reflection (ImmutableArray<(int,int,int)>).
                var markupRanges = GetTupleRanges(_directives, "SetupCodeMarkupRanges");
                var bareRanges = GetTupleRanges(_directives, "SetupCodeBareJsxRanges");

                if (markupRanges.Count == 0 && bareRanges.Count == 0)
                    return setupCode;

                var allRanges = new List<(int Start, int End, int Line)>(markupRanges.Count + bareRanges.Count);
                allRanges.AddRange(markupRanges);
                allRanges.AddRange(bareRanges);
                allRanges.Sort((a, b) => a.Start.CompareTo(b.Start));

                int fseOffset = GP<int>(_directives, "FunctionSetupStartOffset");
                int gapOffset = GP<int>(_directives, "FunctionSetupGapOffset");
                int gapLength = GP<int>(_directives, "FunctionSetupGapLength");

                var spliced = new StringBuilder(setupCode.Length);
                int prev = 0;

                foreach (var (absStart, absEnd, line) in allRanges)
                {
                    int relStart = AbsToSetupOffset(absStart, fseOffset, gapOffset, gapLength);
                    int relEnd = AbsToSetupOffset(absEnd, fseOffset, gapOffset, gapLength);

                    if (relStart < 0) relStart = 0;
                    if (relEnd < 0) relEnd = 0;
                    if (relStart > setupCode.Length) relStart = setupCode.Length;
                    if (relEnd > setupCode.Length) relEnd = setupCode.Length;
                    if (relEnd <= relStart) continue;

                    if (relStart > prev)
                        spliced.Append(setupCode, prev, relStart - prev);

                    string jsxText = setupCode.Substring(relStart, relEnd - relStart);
                    IList nodes = _parseMarkup(jsxText, _filePath, line);

                    if (nodes != null && nodes.Count > 0)
                    {
                        int savedLen = _sb.Length;
                        if (nodes.Count == 1)
                            EmitNode(nodes[0]);
                        else
                        {
                            _sb.Append("V.Fragment(key: null, __C(");
                            for (int i = 0; i < nodes.Count; i++)
                            {
                                if (i > 0) _sb.Append(", ");
                                EmitNode(nodes[i]);
                            }
                            _sb.Append("))");
                        }
                        string emittedCs = _sb.ToString(savedLen, _sb.Length - savedLen);
                        _sb.Length = savedLen;
                        spliced.Append(emittedCs.Trim());
                    }
                    else
                    {
                        spliced.Append(jsxText);
                    }

                    prev = relEnd;
                }

                if (prev < setupCode.Length)
                    spliced.Append(setupCode, prev, setupCode.Length - prev);

                return spliced.ToString();
            }

            private static int AbsToSetupOffset(int absPos, int fseOffset, int gapOffset, int gapLength)
            {
                int relOffset = absPos - fseOffset;
                if (gapOffset >= 0 && relOffset >= gapOffset)
                    relOffset -= gapLength;
                return relOffset;
            }

            /// <summary>
            /// Reads an ImmutableArray&lt;(int, int, int)&gt; property via reflection,
            /// returning the tuples as a plain list.
            /// </summary>
            private static List<(int Start, int End, int Line)> GetTupleRanges(object obj, string propName)
            {
                var result = new List<(int, int, int)>();
                object raw = UitkxHmrCompiler.GetProp(obj, propName);
                if (raw == null) return result;

                // ImmutableArray<T> implements IEnumerable
                if (raw is IEnumerable enumerable)
                {
                    foreach (object tuple in enumerable)
                    {
                        // ValueTuple<int,int,int> fields: Item1, Item2, Item3
                        var t = tuple.GetType();
                        int s = (int)(t.GetField("Item1")?.GetValue(tuple) ?? 0);
                        int e = (int)(t.GetField("Item2")?.GetValue(tuple) ?? 0);
                        int l = (int)(t.GetField("Item3")?.GetValue(tuple) ?? 0);
                        result.Add((s, e, l));
                    }
                }
                return result;
            }

            // ── Node emission ─────────────────────────────────────────────

            private void EmitNode(object node)
            {
                if (node == null)
                {
                    _sb.Append("null");
                    return;
                }
                string typeName = node.GetType().Name;

                switch (typeName)
                {
                    case "ElementNode":
                        EmitElement(node);
                        break;
                    case "TextNode":
                        EmitText(node);
                        break;
                    case "ExpressionNode":
                        EmitExpression(node);
                        break;
                    case "IfNode":
                        EmitIf(node);
                        break;
                    case "ForeachNode":
                        EmitForeach(node);
                        break;
                    case "ForNode":
                        EmitFor(node);
                        break;
                    case "WhileNode":
                        EmitWhile(node);
                        break;
                    case "SwitchNode":
                        EmitSwitch(node);
                        break;
                    case "JsxCommentNode":
                        break; // skip
                    default:
                        _sb.Append($"({QVNode})null /* unsupported: {typeName} */");
                        break;
                }
            }

            private void EmitElement(object el)
            {
                bool injectUss = _isRootElement && _ussFiles.Count > 0;
                _isRootElement = false;

                string tagName = GP<string>(el, "TagName") ?? "";
                var attrs = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(el, "Attributes"));
                var children = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(el, "Children"));
                string keyExpr = ExtractKey(attrs);

                // Empty tag = fragment shorthand
                if (string.IsNullOrEmpty(tagName))
                {
                    EmitFragment(keyExpr, children);
                    return;
                }

                // Try built-in resolution
                if (s_tagMap.TryGetValue(tagName, out var res))
                {
                    switch (res.Kind)
                    {
                        case TagKind.Fragment:
                            EmitFragment(keyExpr, children);
                            break;
                        case TagKind.Text:
                            EmitBuiltinText(attrs, keyExpr);
                            break;
                        case TagKind.Typed:
                            EmitTyped(res, attrs, keyExpr, children, injectUss);
                            break;
                        case TagKind.TypedC:
                            EmitTyped(res, attrs, keyExpr, children, injectUss);
                            break;
                        case TagKind.Dict:
                            EmitDict(res, attrs, keyExpr, children, injectUss);
                            break;
                        case TagKind.Suspense:
                            EmitSuspense(attrs, keyExpr, children);
                            break;
                        case TagKind.Portal:
                            EmitPortal(attrs, keyExpr, children);
                            break;
                    }
                    return;
                }

                // PascalCase → function component
                if (char.IsUpper(tagName[0]))
                {
                    string lookupName = s_componentAliases.TryGetValue(tagName, out var aliased)
                        ? aliased
                        : tagName;
                    EmitFuncComponent(lookupName, attrs, keyExpr, children);
                    return;
                }

                // Unknown tag → null with comment
                _sb.Append($"({QVNode})null /* unknown: {tagName} */");
            }

            private void EmitText(object node)
            {
                string content = GP<string>(node, "Content") ?? "";
                _sb.Append($"V.Text({Escape(content)})");
            }

            private void EmitExpression(object node)
            {
                string expr = GP<string>(node, "Expression") ?? "";
                // Inline expression → embed as text via string interpolation
                _sb.Append($"V.Text(({expr})?.ToString() ?? \"\")");
            }

            // ── Element emission variants ──────────────────────────────────

            private void EmitFragment(string keyExpr, IList children)
            {
                _sb.Append($"V.Fragment(key: {keyExpr}");
                if (children.Count > 0)
                {
                    _sb.Append(", __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            private void EmitBuiltinText(IList attrs, string keyExpr)
            {
                string textVal = GetAttrExpr(attrs, "text") ?? "\"\"";
                _sb.Append($"V.Text({textVal}, key: {keyExpr})");
            }

            private void EmitTyped(
                TagRes res,
                IList attrs,
                string keyExpr,
                IList children,
                bool injectUssKeys = false
            )
            {
                var filteredAttrs = FilterAttrs(attrs, "key");

                _sb.Append($"V.{res.MethodName}(new {res.PropsType} {{ ");
                bool first = true;
                foreach (var attr in filteredAttrs)
                {
                    string name = GP<string>(attr, "Name") ?? "";
                    string val = AttrToExpr(attr);
                    string propName = ToPascal(name);
                    if (!first)
                        _sb.Append(", ");
                    _sb.Append($"{propName} = {val}");
                    first = false;
                }
                if (injectUssKeys)
                {
                    if (!first)
                        _sb.Append(", ");
                    _sb.Append(
                        "ExtraProps = new Dictionary<string, object> { { \"__ussKeys\", __uitkx_ussKeys } }"
                    );
                }
                _sb.Append($" }}, key: {keyExpr}");

                if (res.Kind == TagKind.TypedC && children.Count > 0)
                {
                    _sb.Append(", __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            private void EmitDict(
                TagRes res,
                IList attrs,
                string keyExpr,
                IList children,
                bool injectUssKeys = false
            )
            {
                var filteredAttrs = FilterAttrs(attrs, "key");

                bool hasNonKeyAttrs = injectUssKeys;
                if (!hasNonKeyAttrs)
                {
                    foreach (var a in filteredAttrs)
                    {
                        hasNonKeyAttrs = true;
                        break;
                    }
                }

                _sb.Append($"V.{res.MethodName}(");
                if (hasNonKeyAttrs)
                {
                    _sb.Append("new Dictionary<string,object> { ");
                    bool first = true;
                    foreach (var attr in filteredAttrs)
                    {
                        string name = GP<string>(attr, "Name") ?? "";
                        string val = AttrToExpr(attr);
                        if (!first)
                            _sb.Append(", ");
                        _sb.Append($"[\"{name}\"] = {val}");
                        first = false;
                    }
                    if (injectUssKeys)
                    {
                        if (!first)
                            _sb.Append(", ");
                        _sb.Append("[\"__ussKeys\"] = __uitkx_ussKeys");
                    }
                    _sb.Append(" }");
                }
                else
                {
                    _sb.Append("null");
                }
                _sb.Append($", key: {keyExpr}");

                if (children.Count > 0)
                {
                    _sb.Append(", __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            private void EmitFuncComponent(
                string typeName,
                IList attrs,
                string keyExpr,
                IList children
            )
            {
                var filteredAttrs = FilterAttrs(attrs, "key");
                filteredAttrs = FilterAttrs(filteredAttrs, "ref");

                // Check if there are props → use V.Func<Props>(Type.Render, new Props { })
                // Otherwise → use V.Func(Type.Render, key: ...)
                if (filteredAttrs.Count > 0)
                {
                    // Find the actual props type via runtime reflection
                    string propsTypeName = FindPropsType(typeName);
                    _sb.Append($"V.Func<{propsTypeName}>({typeName}.Render, ");
                    _sb.Append($"new {propsTypeName} {{ ");
                    bool first = true;
                    foreach (var attr in filteredAttrs)
                    {
                        string name = GP<string>(attr, "Name") ?? "";
                        string val = AttrToExpr(attr);
                        string propName = ToPascal(name);
                        if (!first)
                            _sb.Append(", ");
                        _sb.Append($"{propName} = {val}");
                        first = false;
                    }
                    _sb.Append($" }}, key: {keyExpr}");
                }
                else
                {
                    _sb.Append($"V.Func({typeName}.Render, key: {keyExpr}");
                }

                if (children.Count > 0)
                {
                    _sb.Append(", children: __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            /// <summary>
            /// Find the props type for a function component by searching loaded assemblies.
            /// Tries: TypeName.TypeNameProps (uitkx convention), TypeName.Props (manual convention).
            /// </summary>
            private static string FindPropsType(string typeName)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.IsDynamic)
                        continue;
                    try
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            if (type.Name != typeName)
                                continue;
                            // Look for nested types that implement IProps
                            foreach (var nested in type.GetNestedTypes())
                            {
                                if (nested.GetInterface("ReactiveUITK.Core.IProps") != null)
                                    return $"{typeName}.{nested.Name}";
                            }
                        }
                    }
                    catch
                    { /* ReflectionTypeLoadException — skip */
                    }
                }
                // Fallback: assume uitkx convention
                return $"{typeName}.{typeName}Props";
            }

            private void EmitSuspense(IList attrs, string keyExpr, IList children)
            {
                string isReady = GetAttrExpr(attrs, "isReady") ?? "true";
                string fallback = GetAttrExpr(attrs, "fallback") ?? "null";
                _sb.Append($"V.Suspense({isReady}, {fallback}, key: {keyExpr}");
                if (children.Count > 0)
                {
                    _sb.Append(", __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            private void EmitPortal(IList attrs, string keyExpr, IList children)
            {
                string target = GetAttrExpr(attrs, "target") ?? "null";
                _sb.Append($"V.Portal({target}, key: {keyExpr}");
                if (children.Count > 0)
                {
                    _sb.Append(", __C(");
                    EmitChildArgs(children);
                    _sb.Append(")");
                }
                _sb.Append(")");
            }

            // ── Control flow ──────────────────────────────────────────────

            private void EmitIf(object node)
            {
                var branches = UitkxHmrCompiler.GetItems(
                    UitkxHmrCompiler.GetProp(node, "Branches")
                );

                // if/else-if/else chain wrapped in an IIFE returning VirtualNode
                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                for (int i = 0; i < branches.Count; i++)
                {
                    var branch = branches[i];
                    string condition = GP<string>(branch, "Condition");
                    var body = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(branch, "Body"));

                    if (condition != null)
                    {
                        _sb.Append(
                            i == 0 ? $"if ({condition}) {{ " : $" else if ({condition}) {{ "
                        );
                    }
                    else
                    {
                        _sb.Append(" else { ");
                    }

                    string setupCode = GP<string>(branch, "SetupCode");
                    if (setupCode != null)
                    {
                        setupCode = ApplyHookAliases(setupCode);
                        if (setupCode.Contains("Asset<") || setupCode.Contains("Ast<"))
                            setupCode = ResolveAssetPaths(setupCode, _filePath);
                        _sb.Append(setupCode);
                        _sb.Append(" ");
                    }
                    _sb.Append("return ");
                    EmitBodyAsFragment(body);
                    _sb.Append("; }");
                }
                _sb.Append($" return ({QVNode})null; }}))()");
                ;
            }

            private void EmitForeach(object node)
            {
                string iterDecl = GP<string>(node, "IteratorDeclaration") ?? "var item";
                string collExpr = GP<string>(node, "CollectionExpression") ?? "new object[0]";
                var body = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(node, "Body"));
                string setupCode = GP<string>(node, "SetupCode");

                // Extract just the variable name from "var item" or "int x" style declarations
                string iterVar = iterDecl.Trim().Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } parts
                    ? parts[parts.Length - 1]
                    : "__item";

                if (setupCode != null)
                {
                    // IIFE with statement mode for setup code
                    _sb.Append("((Func<" + QVNode + ">)(() => { ");
                    _sb.Append($"var __items = new List<{QVNode}>(); ");
                    _sb.Append($"foreach ({iterDecl} in {collExpr}) {{ ");
                    setupCode = ApplyHookAliases(setupCode);
                    if (setupCode.Contains("Asset<") || setupCode.Contains("Ast<"))
                        setupCode = ResolveAssetPaths(setupCode, _filePath);
                    _sb.Append(setupCode);
                    _sb.Append(" __items.Add(");
                    EmitBodyAsFragment(body);
                    _sb.Append("); } ");
                    _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()" );
                }
                else
                {
                    _sb.Append($"V.Fragment(key: null, __C(");
                    _sb.Append($"({collExpr}).Select({iterVar} => {{ return (object)(");
                    if (body.Count == 1)
                        EmitNode(body[0]);
                    else
                        EmitBodyAsFragment(body);
                    _sb.Append("); }).ToArray()");
                    _sb.Append("))");
                }
            }

            private void EmitFor(object node)
            {
                string forExpr = GP<string>(node, "ForExpression") ?? "";
                var body = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(node, "Body"));
                string setupCode = GP<string>(node, "SetupCode");

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"var __items = new List<{QVNode}>(); ");
                _sb.Append($"for ({forExpr}) {{ ");
                if (setupCode != null)
                {
                    setupCode = ApplyHookAliases(setupCode);
                    if (setupCode.Contains("Asset<") || setupCode.Contains("Ast<"))
                        setupCode = ResolveAssetPaths(setupCode, _filePath);
                    _sb.Append(setupCode);
                    _sb.Append(" ");
                }
                _sb.Append("__items.Add(");
                EmitBodyAsFragment(body);
                _sb.Append("); } ");
                _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()");
            }

            private void EmitWhile(object node)
            {
                string cond = GP<string>(node, "Condition") ?? "false";
                var body = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(node, "Body"));
                string setupCode = GP<string>(node, "SetupCode");

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"var __items = new List<{QVNode}>(); ");
                _sb.Append($"while ({cond}) {{ ");
                if (setupCode != null)
                {
                    setupCode = ApplyHookAliases(setupCode);
                    if (setupCode.Contains("Asset<") || setupCode.Contains("Ast<"))
                        setupCode = ResolveAssetPaths(setupCode, _filePath);
                    _sb.Append(setupCode);
                    _sb.Append(" ");
                }
                _sb.Append("__items.Add(");
                EmitBodyAsFragment(body);
                _sb.Append("); } ");
                _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()");
            }

            private void EmitSwitch(object node)
            {
                string switchExpr = GP<string>(node, "SwitchExpression") ?? "null";
                var cases = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(node, "Cases"));

                // Pre-compute body fragments for fallthrough merging
                var caseData = new List<(string val, string setupCode, string bodyFrag)>();
                foreach (var c in cases)
                {
                    string val = GP<string>(c, "ValueExpression");
                    var body = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(c, "Body"));
                    string setupCode = GP<string>(c, "SetupCode");
                    int mark = _sb.Length;
                    EmitBodyAsFragment(body);
                    string frag = _sb.ToString(mark, _sb.Length - mark);
                    _sb.Length = mark;
                    caseData.Add((val, setupCode, frag));
                }

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"switch ({switchExpr}) {{ ");
                int i = 0;
                while (i < caseData.Count)
                {
                    var (val, setupCode, bodyFrag) = caseData[i];
                    // Merge consecutive no-setup, non-default cases with identical bodies
                    int groupEnd = i + 1;
                    if (setupCode == null && val != null)
                    {
                        while (groupEnd < caseData.Count
                            && caseData[groupEnd].setupCode == null
                            && caseData[groupEnd].val != null
                            && caseData[groupEnd].bodyFrag == bodyFrag)
                            groupEnd++;
                    }
                    for (int j = i; j < groupEnd; j++)
                    {
                        if (caseData[j].val != null)
                            _sb.Append($"case {caseData[j].val}: ");
                        else
                            _sb.Append("default: ");
                    }
                    if (setupCode != null)
                    {
                        setupCode = ApplyHookAliases(setupCode);
                        if (setupCode.Contains("Asset<") || setupCode.Contains("Ast<"))
                            setupCode = ResolveAssetPaths(setupCode, _filePath);
                        _sb.Append(setupCode);
                        _sb.Append(" ");
                    }
                    _sb.Append("return ");
                    _sb.Append(bodyFrag);
                    _sb.Append("; ");
                    i = groupEnd;
                }
                _sb.Append($"}} return ({QVNode})null; }}))()");
            }

            // ── Helpers ───────────────────────────────────────────────────

            private void EmitBodyAsFragment(IList body)
            {
                if (body.Count == 0)
                {
                    _sb.Append($"({QVNode})null");
                    return;
                }
                if (body.Count == 1)
                {
                    EmitNode(body[0]);
                    return;
                }
                _sb.Append("V.Fragment(key: null, __C(");
                EmitChildArgs(body);
                _sb.Append("))");
            }

            private void EmitChildArgs(IList children)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (i > 0)
                        _sb.Append(", ");
                    EmitNode(children[i]);
                }
            }

            private void EmitHelper()
            {
                L($"        private static {QVNode}[] __C(params object[] items)");
                L("        {");
                L(
                    $"            var result = new System.Collections.Generic.List<{QVNode}>(items.Length);"
                );
                L("            for (int i = 0; i < items.Length; i++)");
                L("            {");
                L("                var item = items[i];");
                L("                if (item == null) continue;");
                L($"                if (item is {QVNode} v) {{ result.Add(v); continue; }}");
                L(
                    $"                if (item is {QVNode}[] arr) {{ result.AddRange(arr); continue; }}"
                );
                L("                if (item is System.Collections.IEnumerable seq)");
                L(
                    $"                    foreach (var child in seq) if (child is {QVNode} cv) result.Add(cv);"
                );
                L("            }");
                L("            return result.ToArray();");
                L("        }");
                L("");

                // Cross-assembly reflection helper for reading props
                L(
                    "        private static T __HmrProp<T>(global::ReactiveUITK.Core.IProps props, string name, T fallback)"
                );
                L("        {");
                L("            if (props == null) return fallback;");
                L("            var p = props.GetType().GetProperty(name);");
                L("            if (p == null) return fallback;");
                L("            try { return (T)p.GetValue(props); } catch { return fallback; }");
                L("        }");
                L("");
            }

            private void EmitFunctionPropsClass()
            {
                L("");
                L(
                    $"        public sealed class {_componentName}Props : global::ReactiveUITK.Core.IProps"
                );
                L("        {");
                foreach (var fp in _functionParams)
                {
                    string fpName =
                        GP<string>(fp, "Name")
                        ?? (string)fp.GetType().GetField("Name")?.GetValue(fp)
                        ?? "p";
                    string fpType =
                        GP<string>(fp, "TypeAnnotation")
                        ?? GP<string>(fp, "Type")
                        ?? (string)fp.GetType().GetField("TypeAnnotation")?.GetValue(fp)
                        ?? "object";
                    string propName = char.ToUpper(fpName[0]) + fpName.Substring(1);
                    L($"            public {fpType} {propName} {{ get; set; }}");
                }
                L("");
                L("            public override bool Equals(object obj)");
                L("            {");
                L($"                if (!(obj is {_componentName}Props o)) return false;");
                var paramNames = new List<string>();
                foreach (var fp in _functionParams)
                {
                    string fpName =
                        GP<string>(fp, "Name")
                        ?? (string)fp.GetType().GetField("Name")?.GetValue(fp)
                        ?? "p";
                    paramNames.Add(char.ToUpper(fpName[0]) + fpName.Substring(1));
                }
                if (paramNames.Count == 0)
                    L("                return true;");
                else
                {
                    var checks = paramNames.Select(n => $"Equals({n}, o.{n})").ToArray();
                    L($"                return {string.Join(" && ", checks)};");
                }
                L("            }");
                L("");
                L("            public override int GetHashCode()");
                L("            {");
                L("                unchecked");
                L("                {");
                L("                    int hash = 17;");
                foreach (var fp in _functionParams)
                {
                    string fpName =
                        GP<string>(fp, "Name")
                        ?? (string)fp.GetType().GetField("Name")?.GetValue(fp)
                        ?? "p";
                    string propName = char.ToUpper(fpName[0]) + fpName.Substring(1);
                    L(
                        $"                    hash = hash * 31 + global::System.Collections.Generic.EqualityComparer<object>.Default.GetHashCode({propName});"
                    );
                }
                L("                    return hash;");
                L("                }");
                L("            }");
                L("        }");
            }

            // ── Attribute helpers ─────────────────────────────────────────

            private string ExtractKey(IList attrs)
            {
                string keyExpr = GetAttrExpr(attrs, "key");
                return keyExpr ?? "null";
            }

            private string GetAttrExpr(IList attrs, string name)
            {
                foreach (var attr in attrs)
                {
                    string attrName = GP<string>(attr, "Name");
                    if (string.Equals(attrName, name, StringComparison.OrdinalIgnoreCase))
                        return AttrToExpr(attr);
                }
                return null;
            }

            private string AttrToExpr(object attr)
            {
                var val = UitkxHmrCompiler.GetProp(attr, "Value");
                if (val == null)
                    return "true";

                string valType = val.GetType().Name;
                switch (valType)
                {
                    case "StringLiteralValue":
                        return "\""
                            + (GP<string>(val, "Value") ?? "")
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"")
                            + "\"";
                    case "CSharpExpressionValue":
                        // Apply setter-lambda sugar (matches real emitter's AttrVal)
                        string expr = GP<string>(val, "Expression") ?? "null";
                        expr = s_setterLambdaRe.Replace(expr, "$1.Set(");
                        // Resolve relative asset paths
                        if (expr.Contains("Asset<") || expr.Contains("Ast<"))
                            expr = ResolveAssetPaths(expr, _filePath);
                        return expr;
                    case "BooleanShorthandValue":
                        return "true";
                    case "JsxExpressionValue":
                        // Element as attribute value — rare
                        var innerEl = UitkxHmrCompiler.GetProp(val, "Element");
                        if (innerEl != null)
                        {
                            var sb = new StringBuilder();
                            // This is unusual — just emit null for now
                            return "null /* jsx attr value */";
                        }
                        return "null";
                    default:
                        return "null";
                }
            }

            private static IList FilterAttrs(IList attrs, string excludeName)
            {
                var result = new List<object>();
                foreach (var attr in attrs)
                {
                    string name = GP<string>(attr, "Name");
                    if (!string.Equals(name, excludeName, StringComparison.OrdinalIgnoreCase))
                        result.Add(attr);
                }
                return result;
            }

            private static string ToPascal(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return name;
                // Common pattern: onClick → OnClick, text → Text
                return char.ToUpper(name[0]) + name.Substring(1);
            }

            // ── Output helpers ────────────────────────────────────────────

            private void L(string line) => _sb.AppendLine(line);

            private static T GP<T>(object obj, string name)
            {
                if (obj == null)
                    return default;
                var prop = obj.GetType()
                    .GetProperty(
                        name,
                        System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.Instance
                    );
                if (prop == null)
                    return default;
                try
                {
                    return (T)prop.GetValue(obj);
                }
                catch
                {
                    return default;
                }
            }

            private static string Escape(string s)
            {
                return "\""
                    + s.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                    + "\"";
            }
        }

        // ── Tag resolution types ──────────────────────────────────────────────

        // ── Hook alias substitution (mirrors CSharpEmitter.ApplyHookAliases) ──

        private static readonly (string From, string To)[] s_hookAliases =
        {
            ("useState(", "Hooks.UseState("),
            ("useEffect(", "Hooks.UseEffect("),
            ("useLayoutEffect(", "Hooks.UseLayoutEffect("),
            ("useRef(", "Hooks.UseRef("),
            ("useCallback(", "Hooks.UseCallback("),
            ("useMemo(", "Hooks.UseMemo("),
            ("useContext(", "Hooks.UseContext("),
            ("useReducer(", "Hooks.UseReducer("),
            ("useSignal(", "Hooks.UseSignal("),
            ("useDeferredValue(", "Hooks.UseDeferredValue("),
            ("useTransition(", "Hooks.UseTransition("),
            ("provideContext(", "Hooks.ProvideContext("),
        };

        // Matches generic hook calls: useRef<VisualElement?>(, useState<int>( etc.
        private static readonly Regex s_genericHookAliasRe = new Regex(
            @"\b(useState|useEffect|useLayoutEffect|useRef|useCallback|useMemo|useContext|useReducer|useSignal|useDeferredValue|useTransition)(<(?:[^<>]|<(?:[^<>]|<[^<>]*>)*>)*>)\s*\(",
            RegexOptions.Compiled
        );

        // Matches setter lambda calls: setFoo(v => ...) → setFoo.Set(v => ...)
        private static readonly Regex s_setterLambdaRe = new Regex(
            @"\b(set[A-Z][a-zA-Z0-9_]*)\(\s*(?=[a-zA-Z_][a-zA-Z0-9_]*\s*=>|\([^)]*\)\s*=>)",
            RegexOptions.Compiled
        );

        // Matches Asset<T>("path") or Ast<T>("path") with any string-literal path.
        private static readonly Regex s_assetCallRe = new Regex(
            @"(?:Asset|Ast)\s*<\s*\w+\s*>\s*\(\s*""([^""]+)""\s*\)",
            RegexOptions.Compiled
        );

        private static string ApplyHookAliases(string code)
        {
            // Setter lambda sugar: setFoo(v => v+1) → setFoo.Set(v => v+1)
            code = s_setterLambdaRe.Replace(code, "$1.Set(");

            if (
                code.IndexOf("use", StringComparison.Ordinal) < 0
                && code.IndexOf("provideContext", StringComparison.Ordinal) < 0
            )
                return code;

            // Generic hook calls: useRef<T>( → Hooks.UseRef<T>(
            code = s_genericHookAliasRe.Replace(
                code,
                m =>
                {
                    string hookName = m.Groups[1].Value;
                    string typeArgs = m.Groups[2].Value;
                    string pascalName = char.ToUpper(hookName[3]) + hookName.Substring(4);
                    return $"Hooks.Use{pascalName}{typeArgs}(";
                }
            );

            // Simple non-generic aliases
            foreach (var (from, to) in s_hookAliases)
                code = code.Replace(from, to);

            return code;
        }

        // ── Asset path resolution (mirrors CSharpEmitter) ─────────────────────

        private static string ResolveAssetPaths(string expression, string filePath)
        {
            return s_assetCallRe.Replace(
                expression,
                match =>
                {
                    string rawPath = match.Groups[1].Value;
                    if (!rawPath.StartsWith("./") && !rawPath.StartsWith("../"))
                        return match.Value;
                    string resolved = ResolveRelativePath(rawPath, filePath);
                    return match.Value.Replace($"\"{rawPath}\"", $"\"{resolved}\"");
                }
            );
        }

        private static string ResolveRelativePath(string relativePath, string filePath)
        {
            string uitkxDir = GetUitkxAssetDir(filePath);
            string combined = uitkxDir + "/" + relativePath;
            var parts = combined.Replace('\\', '/').Split('/');
            var stack = new List<string>();
            foreach (var p in parts)
            {
                if (p == "." || p == "")
                    continue;
                if (p == ".." && stack.Count > 0)
                    stack.RemoveAt(stack.Count - 1);
                else if (p != "..")
                    stack.Add(p);
            }
            return string.Join("/", stack);
        }

        private static string GetUitkxAssetDir(string filePath)
        {
            string normalized = filePath.Replace('\\', '/');
            int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            if (assetsIdx >= 0)
            {
                string assetPath = normalized.Substring(assetsIdx + 1);
                int lastSlash = assetPath.LastIndexOf('/');
                return lastSlash >= 0 ? assetPath.Substring(0, lastSlash) : "Assets";
            }
            string dir = Path.GetDirectoryName(filePath)?.Replace('\\', '/') ?? "";
            return dir;
        }

        private enum TagKind
        {
            Typed,
            TypedC,
            Dict,
            Text,
            Fragment,
            Suspense,
            Portal,
        }

        private readonly struct TagRes
        {
            public readonly TagKind Kind;
            public readonly string MethodName;
            public readonly string PropsType;

            public TagRes(TagKind kind, string method, string props)
            {
                Kind = kind;
                MethodName = method;
                PropsType = props;
            }

            public static TagRes Typed(string method, string props) =>
                new TagRes(TagKind.Typed, method, props);

            public static TagRes TypedC(string method, string props) =>
                new TagRes(TagKind.TypedC, method, props);
        }
    }
}
