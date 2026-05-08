using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        // ── Built-in tag → resolution map ───────────────────────────────────
        //
        // Auto-discovered at type-init via reflection over
        // <c>typeof(global::ReactiveUITK.V).GetMethods()</c>. Mirrors the
        // source generator's <c>PropsResolver.BuildBuiltinMapFromCompilation</c>
        // (Roslyn-based auto-scan) one-for-one — same classification rules,
        // same key casing — so adding a new <c>V.Foo(FooProps, …)</c> factory
        // is automatically picked up by both emitters with no further edits.
        //
        // Historically this was a 33-entry hand-maintained literal that
        // silently drifted from <c>V.cs</c> every release (38 typed factories
        // were missing as of 0.4.19, e.g. <c>&lt;Animate&gt;</c>,
        // <c>&lt;Toolbar&gt;</c>, every editor field, etc.). The reflection
        // discovery closes the entire bug class structurally.
        private static readonly Dictionary<string, TagRes> s_tagMap =
            HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap();

        // Single source of truth lives in
        // Shared/Core/Router/RouterTagAliases.cs.  Both the source generator
        // and this HMR emitter consume the same dictionary so that markup tag
        // resolution stays in lock-step across cold-build and hot-reload.
        private static readonly IReadOnlyDictionary<string, string> s_componentAliases =
            global::ReactiveUITK.Router.RouterTagAliases.Map;

        private const string QVNode = "global::ReactiveUITK.Core.VirtualNode";

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Parses a JSX markup fragment and returns the AST nodes (opaque IList).
        /// Signature: (jsxText, filePath, startLine) → IList of AST nodes.
        /// </summary>
        internal delegate IList MarkupParseFunc(string jsxText, string filePath, int startLine);

        /// <summary>
        /// Reflective access to <c>DirectiveParser.FindJsxBlockRanges</c> and
        /// <c>DirectiveParser.FindBareJsxRanges</c>. Each call returns an
        /// <c>IEnumerable</c> of <c>ValueTuple&lt;int, int, int&gt;</c>
        /// (Start, End, Line). Used by Phase 1's expression splice to detect
        /// JSX literals embedded inside arbitrary C# expressions at HMR time.
        /// </summary>
        internal delegate IEnumerable FindJsxRangesFunc(string source, int rangeStart, int rangeEnd);

        /// <summary>
        /// Emit C# from the AST and directives produced by the Language parser.
        /// All parameters are opaque objects from the dynamically loaded Language.dll.
        /// </summary>
        public static string Emit(
            object directives,
            object rootNodes,
            string filePath,
            MarkupParseFunc parseMarkup = null,
            FindJsxRangesFunc findJsxBlockRanges = null,
            FindJsxRangesFunc findBareJsxRanges = null
        )
        {
            var ctx = new EmitCtx(
                directives,
                filePath,
                parseMarkup,
                findJsxBlockRanges,
                findBareJsxRanges
            );
            ctx.EmitFile(rootNodes);
            return ctx.ToString();
        }

        // ── Emit context (stateful per invocation) ────────────────────────────

        private sealed class EmitCtx
        {
            private readonly StringBuilder _sb = new StringBuilder(4096);
            private StringBuilder _rentBuffer = new StringBuilder();
            private int _poolVarId;

            // OPT-V2-2 Phase A: hoisted static-style fields collected during the
            // render-body walk. Flushed into the partial class body just before its
            // closing brace. The hoisted instances are created with `new Style { ... }`
            // (generation == 0) so __ScheduleReturn skips them and DiffStyle bails on
            // SameInstance across renders. Mirrors the SG behaviour 1:1.
            private readonly StringBuilder _hoistedStyleFields = new StringBuilder();
            private int _hoistCounter;
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
            private readonly FindJsxRangesFunc _findJsxBlockRanges;
            private readonly FindJsxRangesFunc _findBareJsxRanges;
            private bool _isRootElement = true;

            public EmitCtx(
                object directives,
                string filePath,
                MarkupParseFunc parseMarkup,
                FindJsxRangesFunc findJsxBlockRanges = null,
                FindJsxRangesFunc findBareJsxRanges = null
            )
            {
                _directives = directives;
                _filePath = filePath;
                _parseMarkup = parseMarkup;
                _findJsxBlockRanges = findJsxBlockRanges;
                _findBareJsxRanges = findBareJsxRanges;
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
                    if (typeName == "CommentNode")
                        continue;
                    else
                        markupNodes.Add(n);
                }

                // Header
                L("// <auto-generated — do not edit — HMR />");
                L(
                    "#pragma warning disable CS0105,CS0162,CS8600,CS8601,CS8602,CS8603,CS8604,CS8625"
                );
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

                // Emit hook signature for proactive HMR state-reset detection
                string funcSetupForSig = GP<string>(_directives, "FunctionSetupCode");
                string hookSig = ExtractHookSignature(funcSetupForSig);
                if (hookSig.Length > 0)
                    L($"    [global::ReactiveUITK.HookSignature(\"{hookSig}\")]");

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

                    var savedRent = _rentBuffer;
                    _rentBuffer = new StringBuilder();

                    int mark = _sb.Length;
                    EmitNode(markupNodes[0]);
                    string expr = _sb.ToString(mark, _sb.Length - mark);
                    _sb.Length = mark;

                    // Emit pool-rent statements before return
                    if (_rentBuffer.Length > 0)
                    {
                        L($"#line hidden");
                        _sb.Append("            ");
                        _sb.AppendLine(_rentBuffer.ToString());
                        L($"#line default");
                    }
                    _rentBuffer = savedRent;

                    L($"#line {srcLine} \"{_linePath}\"");
                    _sb.Append("            return ");
                    _sb.Append(expr);
                    _sb.AppendLine(";");
                }
                else
                {
                    L(
                        $"#error UITKX0025: Component return must have a single root element. Wrap multiple elements in a container like <VisualElement>."
                    );
                    L($"            return ({QVNode})null;");
                }

                L("        }"); // close Render

                // Function-style auto-generated props class
                if (_isFunctionStyle && _functionParams.Count > 0)
                    EmitFunctionPropsClass();

                // OPT-V2-2 Phase A: emit any hoisted static Style fields collected
                // during the render-body walk.
                if (_hoistedStyleFields.Length > 0)
                {
                    L("");
                    L("        // ── Hoisted static styles (OPT-V2-2) ──");
                    _sb.Append(_hoistedStyleFields.ToString());
                }

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

                var allRanges = new List<(int Start, int End, int Line)>(
                    markupRanges.Count + bareRanges.Count
                );
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

                    if (relStart < 0)
                        relStart = 0;
                    if (relEnd < 0)
                        relEnd = 0;
                    if (relStart > setupCode.Length)
                        relStart = setupCode.Length;
                    if (relEnd > setupCode.Length)
                        relEnd = setupCode.Length;
                    if (relEnd <= relStart)
                        continue;

                    if (relStart > prev)
                        spliced.Append(setupCode, prev, relStart - prev);

                    string jsxText = setupCode.Substring(relStart, relEnd - relStart);
                    IList nodes = _parseMarkup(jsxText, _filePath, line);

                    if (nodes != null && nodes.Count > 0)
                    {
                        // Save rent buffer — inline JSX may produce rent statements
                        var savedRent = _rentBuffer;
                        _rentBuffer = new StringBuilder();

                        int savedLen = _sb.Length;
                        if (nodes.Count == 1)
                            EmitNode(nodes[0]);
                        else
                        {
                            _sb.Append(
                                "#error UITKX0025: Inline JSX expression must have a single root element."
                            );
                        }
                        string emittedCs = _sb.ToString(savedLen, _sb.Length - savedLen);
                        _sb.Length = savedLen;

                        // Insert rent statements at the last statement boundary
                        string inlineRent = _rentBuffer.ToString();
                        _rentBuffer = savedRent;
                        if (inlineRent.Length > 0)
                        {
                            int insertPos = 0;
                            for (int si = spliced.Length - 1; si >= 0; si--)
                            {
                                char ch = spliced[si];
                                if (ch == ';' || ch == '}')
                                {
                                    insertPos = si + 1;
                                    break;
                                }
                            }
                            spliced.Insert(insertPos, inlineRent);
                        }
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

            private static int AbsToSetupOffset(
                int absPos,
                int fseOffset,
                int gapOffset,
                int gapLength
            )
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
            private static List<(int Start, int End, int Line)> GetTupleRanges(
                object obj,
                string propName
            )
            {
                var result = new List<(int, int, int)>();
                object raw = UitkxHmrCompiler.GetProp(obj, propName);
                if (raw == null)
                    return result;

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

            /// <summary>
            /// Replaces embedded JSX markup spans inside directive body code
            /// with their emitted C# VirtualNode call equivalents.
            /// No gap handling (body is contiguous).
            /// </summary>
            private string SpliceBodyCodeMarkup(
                string bodyCode,
                int bodyCodeOffset,
                List<(int Start, int End, int Line)> markupRanges,
                List<(int Start, int End, int Line)> bareRanges
            )
            {
                if (_parseMarkup == null)
                    return bodyCode;

                if (markupRanges.Count == 0 && bareRanges.Count == 0)
                    return bodyCode;

                var allRanges = new List<(int Start, int End, int Line)>(
                    markupRanges.Count + bareRanges.Count
                );
                allRanges.AddRange(markupRanges);
                allRanges.AddRange(bareRanges);
                allRanges.Sort((a, b) => a.Start.CompareTo(b.Start));

                var spliced = new StringBuilder(bodyCode.Length);
                int prev = 0;

                foreach (var (absStart, absEnd, line) in allRanges)
                {
                    int relStart = absStart - bodyCodeOffset;
                    int relEnd = absEnd - bodyCodeOffset;

                    if (relStart < 0)
                        relStart = 0;
                    if (relEnd < 0)
                        relEnd = 0;
                    if (relStart > bodyCode.Length)
                        relStart = bodyCode.Length;
                    if (relEnd > bodyCode.Length)
                        relEnd = bodyCode.Length;
                    if (relEnd <= relStart)
                        continue;

                    if (relStart > prev)
                        spliced.Append(bodyCode, prev, relStart - prev);

                    string jsxText = bodyCode.Substring(relStart, relEnd - relStart);
                    IList nodes = _parseMarkup(jsxText, _filePath, line);

                    if (nodes != null && nodes.Count > 0)
                    {
                        // Save rent buffer — inline JSX may produce rent statements
                        var savedRent = _rentBuffer;
                        _rentBuffer = new StringBuilder();

                        int savedLen = _sb.Length;
                        if (nodes.Count == 1)
                        {
                            object node = nodes[0];
                            string nodeType = node.GetType().Name;
                            if (
                                nodeType == "ForeachNode"
                                || nodeType == "ForNode"
                                || nodeType == "WhileNode"
                            )
                                _sb.Append(
                                    "#error UITKX: @foreach/@for/@while produces a list and cannot appear directly where a single element is expected. Wrap it in a container element."
                                );
                            else
                                EmitNode(node);
                        }
                        else
                            _sb.Append(
                                "#error UITKX0025: Inline JSX expression must have a single root element."
                            );
                        string emittedCs = _sb.ToString(savedLen, _sb.Length - savedLen);
                        _sb.Length = savedLen;

                        // Inline rent statements before the expression at the splice point
                        string inlineRent = _rentBuffer.ToString();
                        _rentBuffer = savedRent;
                        if (inlineRent.Length > 0)
                        {
                            // Find the last statement boundary in the accumulated text.
                            // Rent statements must appear as standalone statements, not
                            // after 'return' or 'yield return'. Insert right after the
                            // last ';' or '}', or at position 0 if none found.
                            int insertPos = 0;
                            for (int si = spliced.Length - 1; si >= 0; si--)
                            {
                                char ch = spliced[si];
                                if (ch == ';' || ch == '}')
                                {
                                    insertPos = si + 1;
                                    break;
                                }
                            }
                            spliced.Insert(insertPos, inlineRent);
                        }
                        spliced.Append(emittedCs.Trim());
                    }
                    else
                    {
                        spliced.Append(jsxText);
                    }

                    prev = relEnd;
                }

                if (prev < bodyCode.Length)
                    spliced.Append(bodyCode, prev, bodyCode.Length - prev);

                return spliced.ToString();
            }

            // ── Inline-expression JSX splice (Phase 1) ────────────────────

            /// <summary>
            /// HMR mirror of <c>CSharpEmitter.SpliceExpressionMarkup</c>.
            /// Splices JSX literals embedded inside an arbitrary C# expression
            /// (used for <c>{ expr }</c> child positions and <c>attr={ expr }</c>
            /// attribute values). Each detected JSX literal is replaced by its
            /// emitted <c>V.Tag(...)</c> equivalent; pool-rent statements flow
            /// into the shared <see cref="_rentBuffer"/> so the surrounding
            /// emit context hoists them above the parent expression.
            ///
            /// <para>Returns <paramref name="expr"/> unchanged when no JSX is
            /// detected (the common case — scanner cost is O(n) on the
            /// expression text and runs only when the expression actually
            /// contains JSX).</para>
            ///
            /// <para>Falls back to a no-op if the scanner reflection delegates
            /// were not provided (older HMR plumbing). The expression is then
            /// emitted as-is, matching pre-Phase-1 behavior.</para>
            /// </summary>
            private string SpliceExpressionMarkup(string expr, int sourceLine)
            {
                if (string.IsNullOrEmpty(expr))
                    return expr;
                if (_findJsxBlockRanges == null || _findBareJsxRanges == null || _parseMarkup == null)
                    return expr;

                var markupRanges = TuplesFromEnumerable(_findJsxBlockRanges(expr, 0, expr.Length));
                var bareRanges = TuplesFromEnumerable(_findBareJsxRanges(expr, 0, expr.Length));

                if (markupRanges.Count == 0 && bareRanges.Count == 0)
                    return expr;

                var allRanges = new List<(int Start, int End, int Line)>(
                    markupRanges.Count + bareRanges.Count
                );
                allRanges.AddRange(markupRanges);
                allRanges.AddRange(bareRanges);
                allRanges.Sort((a, b) => a.Start.CompareTo(b.Start));

                var spliced = new StringBuilder(expr.Length);
                int prev = 0;

                foreach (var (start, end, line) in allRanges)
                {
                    int s = start;
                    int e = end;
                    if (s < 0)
                        s = 0;
                    if (e < 0)
                        e = 0;
                    if (s > expr.Length)
                        s = expr.Length;
                    if (e > expr.Length)
                        e = expr.Length;
                    if (e <= s)
                        continue;
                    if (s < prev)
                        continue;

                    if (s > prev)
                        spliced.Append(expr, prev, s - prev);

                    string jsxText = expr.Substring(s, e - s);
                    int absLine = sourceLine + (line - 1);

                    IList nodes = _parseMarkup(jsxText, _filePath, absLine);

                    if (nodes != null && nodes.Count > 0)
                    {
                        // Rent flows to the SHARED _rentBuffer so the parent emit
                        // context hoists it above the surrounding expression.
                        int savedLen = _sb.Length;
                        if (nodes.Count == 1)
                        {
                            string nodeType = nodes[0].GetType().Name;
                            if (
                                nodeType == "ForeachNode"
                                || nodeType == "ForNode"
                                || nodeType == "WhileNode"
                            )
                                _sb.Append(
                                    "#error UITKX: @foreach/@for/@while produces a list and cannot appear directly in an expression position. Wrap it in a container element."
                                );
                            else
                                EmitNode(nodes[0]);
                        }
                        else
                        {
                            _sb.Append(
                                "#error UITKX0025: Inline JSX expression must have a single root element."
                            );
                        }
                        string emittedCs = _sb.ToString(savedLen, _sb.Length - savedLen);
                        _sb.Length = savedLen;

                        spliced.Append(emittedCs.Trim());
                    }
                    else
                    {
                        spliced.Append(jsxText);
                    }

                    prev = e;
                }

                if (prev < expr.Length)
                    spliced.Append(expr, prev, expr.Length - prev);

                return spliced.ToString();
            }

            /// <summary>
            /// Converts an enumerable of <c>ValueTuple&lt;int,int,int&gt;</c>
            /// (returned by the language-lib scanners via reflection) into a
            /// concrete tuple list that <see cref="SpliceExpressionMarkup"/>
            /// can consume directly.
            /// </summary>
            private static List<(int Start, int End, int Line)> TuplesFromEnumerable(IEnumerable raw)
            {
                var result = new List<(int, int, int)>();
                if (raw == null)
                    return result;
                foreach (object tuple in raw)
                {
                    var t = tuple.GetType();
                    int s = (int)(t.GetField("Item1")?.GetValue(tuple) ?? 0);
                    int e = (int)(t.GetField("Item2")?.GetValue(tuple) ?? 0);
                    int l = (int)(t.GetField("Item3")?.GetValue(tuple) ?? 0);
                    result.Add((s, e, l));
                }
                return result;
            }

            /// <summary>
            /// Processes directive body code through the full transformation pipeline:
            /// splice JSX → hook aliases → asset paths.
            /// </summary>
            private string TransformBodyCode(object directiveNode)
            {
                string bodyCode = GP<string>(directiveNode, "BodyCode");
                if (bodyCode == null)
                    return "";

                int bodyCodeOffset = GP<int>(directiveNode, "BodyCodeOffset");
                var markupRanges = GetTupleRanges(directiveNode, "BodyMarkupRanges");
                var bareRanges = GetTupleRanges(directiveNode, "BodyBareJsxRanges");

                var code = SpliceBodyCodeMarkup(bodyCode, bodyCodeOffset, markupRanges, bareRanges);
                code = ApplyHookAliases(code);
                if (code.Contains("Asset<") || code.Contains("Ast<"))
                    code = ResolveAssetPaths(code, _filePath);
                return code;
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
                    case "CommentNode":
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
                int sourceLine = GP<int>(node, "SourceLine");
                // @(expr) / {expr} — passed as-is into __C which handles VirtualNode,
                // VirtualNode[], and IEnumerable<VirtualNode> (e.g. @(__children)).
                //
                // Phase 1: any JSX literals embedded inside the expression
                // (e.g. {cond ? <A/> : <B/>}) are spliced to V.Tag(...) calls.
                string spliced = SpliceExpressionMarkup(expr, sourceLine);
                _sb.Append($"({spliced})");
            }

            // ── Element emission variants ──────────────────────────────────

            private void EmitFragment(string keyExpr, IList children)
            {
                _sb.Append($"V.Fragment(key: {keyExpr}");
                if (children.Count > 0)
                {
                    if (TryClassifySimpleChildren(children))
                    {
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
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

                // ErrorBoundaryProps extends IProps (not BaseProps) — cannot be pooled
                bool skipPooling = res.PropsType == "ErrorBoundaryProps";

                if (skipPooling)
                {
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
                }
                else
                {
                    // ── Pool-rent emission ──
                    int pId = _poolVarId++;
                    string propsVar = $"__p_{pId}";
                    _rentBuffer.Append(
                        $"var {propsVar} = global::ReactiveUITK.Props.Typed.BaseProps.__Rent<{res.PropsType}>(); "
                    );

                    string styleVarName = null;
                    foreach (var attr in filteredAttrs)
                    {
                        string name = GP<string>(attr, "Name") ?? "";
                        if (string.Equals(ToPascal(name), "Style", StringComparison.Ordinal))
                        {
                            string val = AttrToExpr(attr);

                            // ── OPT-V2-2 Phase A: try to hoist all-literal styles ──
                            if (TryHoistStaticStyle(val, out string hoistedName))
                            {
                                styleVarName = hoistedName;
                            }
                            else if (TryExtractNewStyleInit(val, out string body))
                            {
                                int sId = _poolVarId++;
                                styleVarName = $"__s_{sId}";
                                _rentBuffer.Append(
                                    $"var {styleVarName} = global::ReactiveUITK.Props.Typed.Style.__Rent(); "
                                );
                                var inits = SplitTopLevelCommas(body);
                                foreach (var init in inits)
                                {
                                    string trimmed = init.Trim();
                                    if (trimmed.Length > 0)
                                        _rentBuffer.Append($"{styleVarName}.{trimmed}; ");
                                }
                            }
                            break;
                        }
                    }

                    foreach (var attr in filteredAttrs)
                    {
                        string name = GP<string>(attr, "Name") ?? "";
                        string propName = ToPascal(name);
                        string val;
                        if (
                            string.Equals(propName, "Style", StringComparison.Ordinal)
                            && styleVarName != null
                        )
                            val = styleVarName;
                        else
                            val = AttrToExpr(attr);
                        _rentBuffer.Append($"{propsVar}.{propName} = {val}; ");
                    }
                    if (injectUssKeys)
                    {
                        _rentBuffer.Append(
                            $"{propsVar}.ExtraProps = new Dictionary<string, object> {{ {{ \"__ussKeys\", __uitkx_ussKeys }} }}; "
                        );
                    }

                    _sb.Append($"V.{res.MethodName}({propsVar}, key: {keyExpr}");
                } // end else (!skipPooling)

                if (res.Kind == TagKind.TypedC && children.Count > 0)
                {
                    if (TryClassifySimpleChildren(children))
                    {
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
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
                    if (TryClassifySimpleChildren(children))
                    {
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
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
                // Extract ref={x} BEFORE filtering so we can route it into the
                // synthesized props object even when ref is the *only* attribute.
                // SG mirrors this with PropsResolver.TryGetRefParamPropName at
                // CSharpEmitter.cs:1162 — when omitted, ref={...} silently
                // no-ops because the resolver-routed assignment never fires.
                string refExpr = GetAttrExpr(attrs, "ref");
                var filteredAttrs = FilterAttrs(attrs, "key");
                filteredAttrs = FilterAttrs(filteredAttrs, "ref");

                // Use the typed (props) path when there is at least one writable
                // prop OR a ref to route. The no-props path (`V.Func(R, null, key)`)
                // would discard a lone ref={x} attribute — restoring the very bug
                // Issue 5 was filed against.
                if (filteredAttrs.Count > 0 || refExpr != null)
                {
                    // Find props type AND scan it for the Ref<T>/MutableRef<T>
                    // slot in a single reflection pass.
                    var (propsTypeName, refSlotName) = FindPropsTypeAndRefSlot(typeName);
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

                    // Route ref={x} into the resolved Ref<T>/MutableRef<T> slot.
                    // No cast — the user's local is already typed by UseRef<T>(),
                    // matching SG's emit shape (CSharpEmitter.cs:1175).
                    if (refExpr != null && refSlotName != null)
                    {
                        if (!first)
                            _sb.Append(", ");
                        _sb.Append($"{refSlotName} = {refExpr}");
                        first = false;
                    }
                    else if (refExpr != null)
                    {
                        // ref={x} on a component whose props expose no Ref<T> slot
                        // is an authoring error. SG raises UITKX0020 here; HMR
                        // logs a warning so the developer sees the divergence.
                        UnityEngine.Debug.LogWarning(
                            $"[HMR] {_displayName}: <{typeName} ref={{...}}> has no "
                                + "matching Ref<T>/MutableRef<T> property on its Props type. "
                                + "The ref will be ignored at runtime."
                        );
                    }

                    _sb.Append($" }}, key: {keyExpr}");
                }
                else
                {
                    // Explicit positional null in the IProps `props` slot — see
                    // CSharpEmitter.cs (cold-build twin) for the CS8323 rationale.
                    _sb.Append($"V.Func({typeName}.Render, null, key: {keyExpr}");
                }

                if (children.Count > 0)
                {
                    if (TryClassifySimpleChildren(children))
                    {
                        // Phase A bypass: V.Func<P>(R, props, key: k, child1, child2, ...)
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", children: __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
                }
                _sb.Append(")");
            }

            /// <summary>
            /// Find the props type for a function component by searching loaded assemblies.
            /// Tries: TypeName.TypeNameProps (uitkx convention), TypeName.Props (manual convention).
            /// </summary>
            /// <summary>
            /// Resolves the props type for a function-style component, mirroring the
            /// source generator's <c>PropsResolver.TryGetFuncComponentPropsTypeName</c>
            /// fallback chain. Looks for, in order:
            /// <list type="number">
            ///   <item>A sibling top-level <c>{typeName}Props</c> class in the same
            ///     namespace as the component (e.g. <c>RouterFunc</c> +
            ///     <c>RouterFuncProps</c> in <c>ReactiveUITK.Router</c>).</item>
            ///   <item>A nested <c>{typeName}.{typeName}Props</c> implementing
            ///     <c>IProps</c> (the convention emitted by the source generator
            ///     for UITKX function components compiled into referenced assemblies).</item>
            ///   <item>Any nested type implementing <c>IProps</c> (legacy fallback).</item>
            /// </list>
            /// Returns a fully-qualified name when (1) matches so emitted code remains
            /// valid even when the consumer's <c>@using</c> directives don't cover the
            /// props' namespace; otherwise returns the bare nested form.
            /// Falls back to <c>{typeName}.{typeName}Props</c> only when nothing is
            /// found — preserves prior behaviour for unknown components.
            /// </summary>
            private static string FindPropsType(string typeName)
            {
                var (name, _) = FindPropsTypeAndRefSlot(typeName);
                return name;
            }

            /// <summary>
            /// Combined props-type + ref-slot resolution. Performs the same
            /// reflection scan as <see cref="FindPropsType"/> but additionally
            /// inspects the resolved Props type for a <c>Ref&lt;T&gt;</c> /
            /// <c>Hooks.MutableRef&lt;T&gt;</c> property so HMR can route
            /// <c>ref={x}</c> attributes into the synthesized props initializer
            /// — mirroring SG's <c>PropsResolver.TryGetRefParamPropName</c>.
            /// Returns a tuple of (formatted-name-for-emit, ref-slot-property-name)
            /// where <c>RefSlotName</c> is <c>null</c> when no ref slot exists
            /// (ambiguity is resolved by taking the first declared ref-typed
            /// property — same precedence rule as SG's first-match behaviour).
            /// </summary>
            private static (string PropsTypeName, string RefSlotName) FindPropsTypeAndRefSlot(
                string typeName
            )
            {
                string resolvedName = null;
                Type resolvedPropsType = null;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.IsDynamic)
                        continue;
                    Type[] types;
                    try
                    {
                        types = asm.GetTypes();
                    }
                    catch
                    {
                        continue; /* ReflectionTypeLoadException — skip */
                    }

                    foreach (var type in types)
                    {
                        if (type.Name != typeName)
                            continue;

                        // Step 1 — sibling top-level {typeName}Props in the same namespace
                        // (the modern UITKX function-component pattern, e.g. RouterFunc /
                        // RouterFuncProps both declared at namespace scope).
                        string siblingName = typeName + "Props";
                        string siblingFullName = string.IsNullOrEmpty(type.Namespace)
                            ? siblingName
                            : type.Namespace + "." + siblingName;
                        var sibling = asm.GetType(siblingFullName, throwOnError: false);
                        if (
                            sibling != null
                            && sibling.GetInterface("ReactiveUITK.Core.IProps") != null
                        )
                        {
                            resolvedName = "global::" + siblingFullName;
                            resolvedPropsType = sibling;
                            goto resolved;
                        }

                        // Step 2 — nested {typeName}.{typeName}Props (the form generated
                        // by the source generator for compiled UITKX components).
                        var nestedNamed = type.GetNestedType(siblingName);
                        if (
                            nestedNamed != null
                            && nestedNamed.GetInterface("ReactiveUITK.Core.IProps") != null
                        )
                        {
                            resolvedName = $"{typeName}.{siblingName}";
                            resolvedPropsType = nestedNamed;
                            goto resolved;
                        }

                        // Step 3 — any nested IProps (legacy fallback).
                        foreach (var nested in type.GetNestedTypes())
                        {
                            if (nested.GetInterface("ReactiveUITK.Core.IProps") != null)
                            {
                                resolvedName = $"{typeName}.{nested.Name}";
                                resolvedPropsType = nested;
                                goto resolved;
                            }
                        }
                    }
                }

                resolved:
                if (resolvedName == null)
                {
                    // Last-resort fallback: assume the {Type}.{Type}Props convention so
                    // emitted code at least produces a clear CS error pointing at a
                    // recognizable name if the type is genuinely missing.
                    return ($"{typeName}.{typeName}Props", null);
                }

                string refSlot =
                    resolvedPropsType != null ? FindRefSlotName(resolvedPropsType) : null;
                return (resolvedName, refSlot);
            }

            /// <summary>
            /// Scans <paramref name="propsType"/>'s public instance properties for
            /// the first one whose declared type is a generic instantiation of
            /// <see cref="global::ReactiveUITK.Core.Ref{T}"/> or the deprecated
            /// <c>Hooks.MutableRef&lt;T&gt;</c>. Returns the property name (the
            /// slot HMR should assign <c>ref={x}</c> into) or <c>null</c> when
            /// the props type has no such slot. Mirrors
            /// <c>PropsResolver.GetMutableRefPropertyNames</c>'s first-match
            /// behaviour at the reflection level.
            /// </summary>
            private static string FindRefSlotName(Type propsType)
            {
                foreach (
                    var prop in propsType.GetProperties(
                        System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.Instance
                    )
                )
                {
                    var pt = prop.PropertyType;
                    if (!pt.IsGenericType)
                        continue;
                    var def = pt.GetGenericTypeDefinition();
                    if (def == typeof(global::ReactiveUITK.Core.Ref<>))
                        return prop.Name;
                    // [Obsolete] backward-compat shape used in older user code.
                    // Suppress CS0618: this is *exactly* the call site that has
                    // to know about the legacy type so HMR can keep routing
                    // ref={x} into pre-existing user components.
#pragma warning disable CS0618
                    if (def == typeof(global::ReactiveUITK.Core.Hooks.MutableRef<>))
                        return prop.Name;
#pragma warning restore CS0618
                }
                return null;
            }

            private void EmitSuspense(IList attrs, string keyExpr, IList children)
            {
                string isReady = GetAttrExpr(attrs, "isReady") ?? "true";
                string fallback = GetAttrExpr(attrs, "fallback") ?? "null";
                _sb.Append($"V.Suspense({isReady}, {fallback}, key: {keyExpr}");
                if (children.Count > 0)
                {
                    if (TryClassifySimpleChildren(children))
                    {
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
                }
                _sb.Append(")");
            }

            private void EmitPortal(IList attrs, string keyExpr, IList children)
            {
                string target = GetAttrExpr(attrs, "target") ?? "null";
                _sb.Append($"V.Portal({target}, key: {keyExpr}");
                if (children.Count > 0)
                {
                    if (TryClassifySimpleChildren(children))
                    {
                        _sb.Append(", ");
                        EmitChildArgs(children);
                    }
                    else
                    {
                        _sb.Append(", __C(");
                        EmitChildArgs(children);
                        _sb.Append(")");
                    }
                }
                _sb.Append(")");
            }

            // ── Control flow ──────────────────────────────────────────────

            private void EmitIf(object node)
            {
                var branches = UitkxHmrCompiler.GetItems(
                    UitkxHmrCompiler.GetProp(node, "Branches")
                );

                // IIFE — body code contains return statements at arbitrary depth
                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                for (int i = 0; i < branches.Count; i++)
                {
                    var branch = branches[i];
                    string condition = GP<string>(branch, "Condition");

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

                    var savedRent = _rentBuffer;
                    _rentBuffer = new StringBuilder();
                    string bodyCode = TransformBodyCode(branch);
                    string rentStmts = _rentBuffer.ToString();
                    _rentBuffer = savedRent;

                    if (!string.IsNullOrEmpty(bodyCode))
                    {
                        _sb.Append(bodyCode);
                        _sb.Append(" ");
                    }
                    if (rentStmts.Length > 0)
                        _sb.Append(rentStmts);
                    _sb.Append("}");
                }
                _sb.Append($" return ({QVNode})null; }}))()");
            }

            private void EmitForeach(object node)
            {
                string iterDecl = GP<string>(node, "IteratorDeclaration") ?? "var item";
                string collExpr = GP<string>(node, "CollectionExpression") ?? "new object[0]";

                var savedRent = _rentBuffer;
                _rentBuffer = new StringBuilder();
                string bodyCode = TransformBodyCode(node);
                string rentStmts = _rentBuffer.ToString();
                _rentBuffer = savedRent;

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"var __items = new List<{QVNode}>(); ");
                _sb.Append($"foreach ({iterDecl} in {collExpr}) {{ ");
                if (!string.IsNullOrEmpty(bodyCode) || rentStmts.Length > 0)
                {
                    string inlined = RewriteReturnsForInline(bodyCode, "__items");
                    _sb.Append($"{rentStmts}{inlined} ");
                }
                _sb.Append("} ");
                _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()");
            }

            private void EmitFor(object node)
            {
                string forExpr = GP<string>(node, "ForExpression") ?? "";

                var savedRent = _rentBuffer;
                _rentBuffer = new StringBuilder();
                string bodyCode = TransformBodyCode(node);
                string rentStmts = _rentBuffer.ToString();
                _rentBuffer = savedRent;

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"var __items = new List<{QVNode}>(); ");
                _sb.Append($"for ({forExpr}) {{ ");
                if (!string.IsNullOrEmpty(bodyCode) || rentStmts.Length > 0)
                {
                    string inlined = RewriteReturnsForInline(bodyCode, "__items");
                    _sb.Append($"{rentStmts}{inlined} ");
                }
                _sb.Append("} ");
                _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()");
            }

            private void EmitWhile(object node)
            {
                string cond = GP<string>(node, "Condition") ?? "false";

                var savedRent = _rentBuffer;
                _rentBuffer = new StringBuilder();
                string bodyCode = TransformBodyCode(node);
                string rentStmts = _rentBuffer.ToString();
                _rentBuffer = savedRent;

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"var __items = new List<{QVNode}>(); ");
                _sb.Append($"while ({cond}) {{ ");
                if (!string.IsNullOrEmpty(bodyCode) || rentStmts.Length > 0)
                {
                    string inlined = RewriteReturnsForInline(bodyCode, "__items");
                    _sb.Append($"{rentStmts}{inlined} ");
                }
                _sb.Append("} ");
                _sb.Append("return V.Fragment(key: null, __items.ToArray()); }))()");
            }

            private void EmitSwitch(object node)
            {
                string switchExpr = GP<string>(node, "SwitchExpression") ?? "null";
                var cases = UitkxHmrCompiler.GetItems(UitkxHmrCompiler.GetProp(node, "Cases"));

                _sb.Append("((Func<" + QVNode + ">)(() => { ");
                _sb.Append($"switch ({switchExpr}) {{ ");
                foreach (var c in cases)
                {
                    string val = GP<string>(c, "ValueExpression");
                    _sb.Append(val != null ? $"case {val}: " : "default: ");

                    var savedRent = _rentBuffer;
                    _rentBuffer = new StringBuilder();
                    string bodyCode = TransformBodyCode(c);
                    string rentStmts = _rentBuffer.ToString();
                    _rentBuffer = savedRent;

                    if (!string.IsNullOrEmpty(bodyCode) || rentStmts.Length > 0)
                    {
                        _sb.Append("{ ");
                        _sb.Append(bodyCode);
                        if (rentStmts.Length > 0)
                            _sb.Append(" ").Append(rentStmts);
                        _sb.Append(" } ");
                    }
                }
                _sb.Append($"}} return ({QVNode})null; }}))()");
            }

            // ── Helpers ───────────────────────────────────────────────────

            private void EmitChildArgs(IList children)
            {
                // UITKX0104 — warn on duplicate static string keys among siblings.
                // Mirrors SG's CSharpEmitter.CheckDuplicateKeys (line 1631) so HMR
                // surfaces the same authoring mistake the cold build catches.
                CheckDuplicateKeys(children);

                bool first = true;
                for (int i = 0; i < children.Count; i++)
                {
                    // Comments emit nothing — skip to avoid dangling commas
                    if (children[i].GetType().Name == "CommentNode")
                        continue;

                    if (!first)
                        _sb.Append(", ");
                    first = false;
                    EmitNode(children[i]);
                }
            }

            /// <summary>
            /// HMR mirror of SG's <c>CSharpEmitter.CheckDuplicateKeys</c>: walk
            /// sibling element children, collect literal-string <c>key={...}</c>
            /// values, and emit a one-line <c>Debug.LogWarning</c> on the first
            /// duplicate. SG severity is Warning (UITKX0104), not Error, so HMR
            /// must not <c>#error</c> here — that would over-fail relative to the
            /// cold build. Non-literal key expressions are skipped (we cannot
            /// statically resolve them).
            /// </summary>
            private void CheckDuplicateKeys(IList children)
            {
                HashSet<string> seen = null;
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child.GetType().Name != "ElementNode")
                        continue;
                    var attrs = UitkxHmrCompiler.GetItems(
                        UitkxHmrCompiler.GetProp(child, "Attributes")
                    );
                    foreach (var attr in attrs)
                    {
                        string name = GP<string>(attr, "Name");
                        if (!string.Equals(name, "key", StringComparison.OrdinalIgnoreCase))
                            continue;
                        var val = UitkxHmrCompiler.GetProp(attr, "Value");
                        if (val == null || val.GetType().Name != "StringLiteralValue")
                            continue;
                        string keyVal = GP<string>(val, "Value") ?? "";
                        seen ??= new HashSet<string>(StringComparer.Ordinal);
                        if (!seen.Add(keyVal))
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[HMR] UITKX0104: Duplicate key '{keyVal}' found "
                                    + $"among sibling elements in '{_displayName}'."
                            );
                        }
                    }
                }
            }

            // ============================================================
            //  Phase A children classifier (mirrors CSharpEmitter)
            // ============================================================
            //
            // Returns true when *every* non-comment child is statically guaranteed
            // to produce exactly one non-null VirtualNode (only ElementNode and
            // TextNode), AND there is at least one such child.  Caller may then
            // bypass `__C(...)` and pass children directly into the container's
            // `params VirtualNode[] children` parameter.  Must be kept in lock-step
            // with CSharpEmitter.TryClassifySimpleChildren so HMR'd components have
            // the same alloc shape as their source-gen'd counterparts.
            private static bool TryClassifySimpleChildren(IList children)
            {
                int effectiveCount = 0;
                for (int i = 0; i < children.Count; i++)
                {
                    string n = children[i].GetType().Name;
                    if (n == "CommentNode")
                        continue;
                    if (n == "ElementNode" || n == "TextNode")
                    {
                        effectiveCount++;
                        continue;
                    }
                    return false;
                }
                return effectiveCount > 0;
            }

            private void EmitHelper()
            {
                // Two-pass count-then-fill into a single fresh VirtualNode[count].
                // Mirrors CSharpEmitter.EmitHelperMethod's Phase B body.
                //
                //   * skips null VNodes (from @if without @else)
                //   * fast path for VirtualNode[] (the common case after Phase A)
                //   * fast path for IReadOnlyList<VirtualNode> (slot pass-through:
                //     @(__children) where __children has compile-time type
                //     IReadOnlyList<VirtualNode>)
                //   * fallback to non-generic IEnumerable for anything else
                L($"        private static {QVNode}[] __C(params object[] items)");
                L("        {");
                L("            // Pass 1: count valid VNodes.");
                L("            int __count = 0;");
                L("            for (int __i = 0; __i < items.Length; __i++)");
                L("            {");
                L("                var __ci = items[__i];");
                L("                if (__ci == null) continue;");
                L(
                    $"                if (__ci is {QVNode} __vn) {{ if (__vn != null) __count++; continue; }}"
                );
                L(
                    $"                if (__ci is {QVNode}[] __arr) {{ for (int __j = 0; __j < __arr.Length; __j++) if (__arr[__j] != null) __count++; continue; }}"
                );
                L(
                    $"                if (__ci is global::System.Collections.Generic.IReadOnlyList<{QVNode}> __ros) {{ int __rn = __ros.Count; for (int __j = 0; __j < __rn; __j++) if (__ros[__j] != null) __count++; continue; }}"
                );
                L("                if (__ci is System.Collections.IEnumerable __seq)");
                L(
                    $"                    foreach (var __sn in __seq) if (__sn is {QVNode} __cv && __cv != null) __count++;"
                );
                L("            }");
                L($"            if (__count == 0) return global::System.Array.Empty<{QVNode}>();");
                L("");
                L("            // Pass 2: fill into pre-sized array.");
                L($"            var __result = new {QVNode}[__count];");
                L("            int __k = 0;");
                L("            for (int __i = 0; __i < items.Length; __i++)");
                L("            {");
                L("                var __ci = items[__i];");
                L("                if (__ci == null) continue;");
                L(
                    $"                if (__ci is {QVNode} __vn) {{ if (__vn != null) __result[__k++] = __vn; continue; }}"
                );
                L(
                    $"                if (__ci is {QVNode}[] __arr) {{ for (int __j = 0; __j < __arr.Length; __j++) {{ var __sn = __arr[__j]; if (__sn != null) __result[__k++] = __sn; }} continue; }}"
                );
                L(
                    $"                if (__ci is global::System.Collections.Generic.IReadOnlyList<{QVNode}> __ros) {{ int __rn = __ros.Count; for (int __j = 0; __j < __rn; __j++) {{ var __sn = __ros[__j]; if (__sn != null) __result[__k++] = __sn; }} continue; }}"
                );
                L("                if (__ci is System.Collections.IEnumerable __seq)");
                L(
                    $"                    foreach (var __sn in __seq) if (__sn is {QVNode} __cv && __cv != null) __result[__k++] = __cv;"
                );
                L("            }");
                L("            return __result;");
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

            private static bool TryExtractNewStyleInit(string expr, out string body)
            {
                body = null;
                if (expr == null)
                    return false;
                int idx = expr.IndexOf("new Style {", StringComparison.Ordinal);
                if (idx < 0)
                    idx = expr.IndexOf("new Style{", StringComparison.Ordinal);
                if (idx < 0)
                    return false;
                int braceStart = expr.IndexOf('{', idx);
                if (braceStart < 0)
                    return false;
                int depth = 1;
                int i = braceStart + 1;
                while (i < expr.Length && depth > 0)
                {
                    if (expr[i] == '{')
                        depth++;
                    else if (expr[i] == '}')
                        depth--;
                    i++;
                }
                if (depth != 0)
                    return false;
                body = expr.Substring(braceStart + 1, i - braceStart - 2).Trim();
                if (body.Length > 0 && body[0] == '(')
                    return false; // tuple, not initializer
                return true;
            }

            private static List<string> SplitTopLevelCommas(string text)
            {
                var parts = new List<string>();
                int start = 0;
                int depth = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '(' || c == '{' || c == '[')
                        depth++;
                    else if (c == ')' || c == '}' || c == ']')
                        depth--;
                    else if (c == ',' && depth == 0)
                    {
                        parts.Add(text.Substring(start, i - start));
                        start = i + 1;
                    }
                }
                if (start < text.Length)
                    parts.Add(text.Substring(start));
                return parts;
            }

            // ── OPT-V2-2 Phase A: static-style hoisting (mirror of CSharpEmitter) ──

            private static readonly HashSet<string> s_literalCtorTypes = new HashSet<string>(
                StringComparer.Ordinal
            )
            {
                "Color",
                "Color32",
                "Vector2",
                "Vector3",
                "Vector4",
                "Vector2Int",
                "Vector3Int",
                "Length",
                "TimeValue",
                "Rect",
                "Quaternion",
                // OPT-V2-2 — newly hoistable struct ctors used by 9-slice / text-shadow / advanced-font helpers
                "TextShadow",
                "FontDefinition",
            };

            /// <summary>
            /// Returns <c>true</c> iff <paramref name="val"/> is `new Style { ... }`
            /// with all-literal initializers. On success, allocates a static field
            /// name in the enclosing partial class and appends its declaration.
            /// </summary>
            private bool TryHoistStaticStyle(string val, out string hoistName)
            {
                hoistName = null;
                if (string.IsNullOrEmpty(val))
                    return false;

                string trimmed = val.Trim();
                int idx;
                if (trimmed.StartsWith("new Style{", StringComparison.Ordinal))
                    idx = "new Style".Length;
                else if (trimmed.StartsWith("new Style {", StringComparison.Ordinal))
                    idx = "new Style ".Length;
                else
                    return false;

                int braceOpen = trimmed.IndexOf('{', idx);
                if (braceOpen < 0)
                    return false;

                int depth = 0;
                int braceClose = -1;
                for (int i = braceOpen; i < trimmed.Length; i++)
                {
                    char c = trimmed[i];
                    if (c == '{')
                        depth++;
                    else if (c == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            braceClose = i;
                            break;
                        }
                    }
                }
                if (braceClose < 0)
                    return false;

                string after = trimmed.Substring(braceClose + 1).Trim();
                if (after.Length > 0)
                    return false;

                string body = trimmed.Substring(braceOpen + 1, braceClose - braceOpen - 1).Trim();
                if (body.Length == 0)
                    return false;

                if (!IsHoistableInitializerBody(body))
                    return false;

                int hid = _hoistCounter++;
                hoistName = $"__sty_{hid}";
                _hoistedStyleFields.Append("        ");
                _hoistedStyleFields.Append(
                    "private static readonly global::ReactiveUITK.Props.Typed.Style "
                );
                _hoistedStyleFields.Append(hoistName);
                _hoistedStyleFields.Append(" = new global::ReactiveUITK.Props.Typed.Style { ");
                _hoistedStyleFields.Append(body);
                _hoistedStyleFields.AppendLine(" };");
                return true;
            }

            private static bool IsHoistableInitializerBody(string body)
            {
                var parts = SplitTopLevelCommas(body);
                foreach (var raw in parts)
                {
                    string part = raw.Trim();
                    if (part.Length == 0)
                        continue;

                    if (part[0] == '(')
                    {
                        if (part[part.Length - 1] != ')')
                            return false;
                        string inner = part.Substring(1, part.Length - 2);
                        var tupleArgs = SplitTopLevelCommas(inner);
                        if (tupleArgs.Count != 2)
                            return false;
                        if (!IsLiteralExpression(tupleArgs[0].Trim()))
                            return false;
                        if (!IsLiteralExpression(tupleArgs[1].Trim()))
                            return false;
                    }
                    else
                    {
                        int eq = FindTopLevelEquals(part);
                        if (eq < 0)
                            return false;
                        string lhs = part.Substring(0, eq).Trim();
                        string rhs = part.Substring(eq + 1).Trim();
                        if (!IsSimpleIdentifier(lhs))
                            return false;
                        if (!IsLiteralExpression(rhs))
                            return false;
                    }
                }
                return true;
            }

            private static int FindTopLevelEquals(string s)
            {
                int depth = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '(' || c == '{' || c == '[')
                        depth++;
                    else if (c == ')' || c == '}' || c == ']')
                        depth--;
                    else if (c == '=' && depth == 0)
                    {
                        if (i + 1 < s.Length && s[i + 1] == '=')
                        {
                            i++;
                            continue;
                        }
                        if (i > 0 && (s[i - 1] == '!' || s[i - 1] == '<' || s[i - 1] == '>'))
                            continue;
                        if (i + 1 < s.Length && s[i + 1] == '>')
                        {
                            i++;
                            continue;
                        }
                        return i;
                    }
                }
                return -1;
            }

            private static bool IsLiteralExpression(string expr)
            {
                if (string.IsNullOrEmpty(expr))
                    return false;
                expr = expr.Trim();
                if (expr.Length == 0)
                    return false;

                if (expr == "null" || expr == "true" || expr == "false")
                    return true;

                if (expr[0] == '"' && expr[expr.Length - 1] == '"' && expr.Length >= 2)
                {
                    for (int i = 1; i < expr.Length - 1; i++)
                    {
                        if (expr[i] == '"' && expr[i - 1] != '\\')
                            return false;
                    }
                    return true;
                }
                if (
                    expr.Length >= 3
                    && expr[0] == '@'
                    && expr[1] == '"'
                    && expr[expr.Length - 1] == '"'
                )
                {
                    for (int i = 2; i < expr.Length - 1; i++)
                        if (expr[i] == '"')
                            return false;
                    return true;
                }
                if (expr[0] == '\'' && expr[expr.Length - 1] == '\'')
                    return true;

                if (IsNumericLiteral(expr))
                    return true;
                if (IsHexLiteral(expr))
                    return true;
                if (IsDottedReference(expr))
                    return true;
                if (TryParseLiteralCtor(expr))
                    return true;

                return false;
            }

            private static bool IsNumericLiteral(string s)
            {
                int i = 0;
                if (s[i] == '-' || s[i] == '+')
                    i++;
                if (i >= s.Length)
                    return false;
                bool seenDigit = false;
                bool seenDot = false;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.'))
                {
                    if (s[i] == '.')
                    {
                        if (seenDot)
                            return false;
                        seenDot = true;
                    }
                    else
                    {
                        seenDigit = true;
                    }
                    i++;
                }
                if (!seenDigit)
                    return false;
                while (i < s.Length)
                {
                    char c = s[i];
                    if (
                        c == 'f'
                        || c == 'F'
                        || c == 'd'
                        || c == 'D'
                        || c == 'm'
                        || c == 'M'
                        || c == 'u'
                        || c == 'U'
                        || c == 'l'
                        || c == 'L'
                    )
                        i++;
                    else
                        return false;
                }
                return true;
            }

            private static bool IsHexLiteral(string s)
            {
                int i = 0;
                if (s[i] == '-' || s[i] == '+')
                    i++;
                if (i + 2 > s.Length)
                    return false;
                if (s[i] != '0' || (s[i + 1] != 'x' && s[i + 1] != 'X'))
                    return false;
                i += 2;
                bool seenDigit = false;
                while (i < s.Length && IsHexDigit(s[i]))
                {
                    seenDigit = true;
                    i++;
                }
                if (!seenDigit)
                    return false;
                while (i < s.Length)
                {
                    char c = s[i];
                    if (c == 'u' || c == 'U' || c == 'l' || c == 'L')
                        i++;
                    else
                        return false;
                }
                return true;
            }

            private static bool IsHexDigit(char c) =>
                (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

            private static bool IsDottedReference(string s)
            {
                if (s.IndexOf('.') < 0)
                    return false;
                if (s.IndexOf('(') >= 0 || s.IndexOf('[') >= 0)
                    return false;
                var parts = s.Split('.');
                // First segment must start uppercase (type/enum/static-class convention).
                // Rejects instance-member access on locals (e.g. box.size, areaSize.x).
                if (parts.Length == 0 || parts[0].Length == 0 || !char.IsUpper(parts[0][0]))
                    return false;
                foreach (var p in parts)
                {
                    if (!IsSimpleIdentifier(p))
                        return false;
                }
                return true;
            }

            private static bool IsSimpleIdentifier(string s)
            {
                if (string.IsNullOrEmpty(s))
                    return false;
                if (!(char.IsLetter(s[0]) || s[0] == '_'))
                    return false;
                for (int i = 1; i < s.Length; i++)
                {
                    if (!(char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                        return false;
                }
                return true;
            }

            private static bool TryParseLiteralCtor(string s)
            {
                if (!s.StartsWith("new ", StringComparison.Ordinal))
                    return false;
                int parenOpen = s.IndexOf('(');
                if (parenOpen < 0)
                    return false;
                if (s[s.Length - 1] != ')')
                    return false;
                string typeName = s.Substring(4, parenOpen - 4).Trim();
                if (!s_literalCtorTypes.Contains(typeName))
                    return false;
                string args = s.Substring(parenOpen + 1, s.Length - parenOpen - 2);
                if (args.Trim().Length == 0)
                    return true;
                var parts = SplitTopLevelCommas(args);
                foreach (var p in parts)
                {
                    if (!IsLiteralExpression(p.Trim()))
                        return false;
                }
                return true;
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
                        // Phase 1: splice JSX literals embedded inside the expression
                        // (e.g. attr={cond ? <A/> : <B/>} or attr={x => <Item/>}).
                        // For the common case (no JSX) the helper returns the input
                        // unchanged after a single O(n) scan.
                        int attrLine = GP<int>(attr, "SourceLine");
                        expr = SpliceExpressionMarkup(expr, attrLine);
                        expr = s_setterLambdaRe.Replace(expr, "$1.Set(");
                        // Resolve relative asset paths
                        if (expr.Contains("Asset<") || expr.Contains("Ast<"))
                            expr = ResolveAssetPaths(expr, _filePath);
                        return expr;
                    case "BooleanShorthandValue":
                        return "true";
                    case "JsxExpressionValue":
                        // Element as attribute value — e.g. <Route element={<HomePage/>}/>.
                        // Mirrors SG's CSharpEmitter.EmitJsxToString: recursively emit the
                        // nested element into _sb, capture the appended span, then truncate
                        // _sb back so the captured text becomes our return value. Side
                        // effects on _rentBuffer / _hoistedStyleFields are intentionally
                        // shared with the parent emit so any pool-rents or hoisted styles
                        // produced by the nested JSX still land in the parent's pre-return
                        // block (and the parent's hoisted-fields section, respectively).
                        var innerEl = UitkxHmrCompiler.GetProp(val, "Element");
                        if (innerEl == null)
                            return $"({QVNode})null";
                        int startLen = _sb.Length;
                        EmitNode(innerEl);
                        string captured = _sb.ToString(startLen, _sb.Length - startLen);
                        _sb.Length = startLen;
                        return captured;
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
            ("useSfx(", "Hooks.UseSfx("),
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

        /// <summary>
        /// Rewrites top-level <c>return EXPR;</c> statements in transformed body code
        /// so the code can be inlined directly inside a loop body instead of an IIFE lambda.
        /// <list type="bullet">
        ///   <item><c>return null;</c> becomes <c>continue;</c></item>
        ///   <item><c>return EXPR;</c> becomes <c>listVar.Add(EXPR); continue;</c></item>
        /// </list>
        /// Returns inside nested lambda bodies (<c>=&gt; { ... }</c>) are left untouched.
        /// </summary>
        private static string RewriteReturnsForInline(string code, string listVar)
        {
            if (string.IsNullOrEmpty(code))
                return code;

            var result = new StringBuilder(code.Length + 64);
            int len = code.Length;
            int lambdaDepth = 0;

            var braceIsLambda = new Stack<bool>();

            int i = 0;
            while (i < len)
            {
                char c = code[i];

                // Skip string literals
                if (c == '"')
                {
                    if (i > 0 && code[i - 1] == '@')
                    {
                        result.Append(c);
                        i++;
                        while (i < len)
                        {
                            result.Append(code[i]);
                            if (code[i] == '"')
                            {
                                if (i + 1 < len && code[i + 1] == '"')
                                {
                                    result.Append(code[i + 1]);
                                    i += 2;
                                }
                                else
                                {
                                    i++;
                                    break;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                        continue;
                    }
                    result.Append(c);
                    i++;
                    while (i < len)
                    {
                        result.Append(code[i]);
                        if (code[i] == '\\')
                        {
                            i++;
                            if (i < len)
                            {
                                result.Append(code[i]);
                                i++;
                            }
                        }
                        else if (code[i] == '"')
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    continue;
                }

                if (c == '\'')
                {
                    result.Append(c);
                    i++;
                    while (i < len)
                    {
                        result.Append(code[i]);
                        if (code[i] == '\\')
                        {
                            i++;
                            if (i < len)
                            {
                                result.Append(code[i]);
                                i++;
                            }
                        }
                        else if (code[i] == '\'')
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    continue;
                }

                if (c == '/' && i + 1 < len && code[i + 1] == '/')
                {
                    while (i < len && code[i] != '\n')
                    {
                        result.Append(code[i]);
                        i++;
                    }
                    continue;
                }

                if (c == '/' && i + 1 < len && code[i + 1] == '*')
                {
                    result.Append(code[i]);
                    result.Append(code[i + 1]);
                    i += 2;
                    while (i < len)
                    {
                        if (code[i] == '*' && i + 1 < len && code[i + 1] == '/')
                        {
                            result.Append(code[i]);
                            result.Append(code[i + 1]);
                            i += 2;
                            break;
                        }
                        result.Append(code[i]);
                        i++;
                    }
                    continue;
                }

                // Track => { for lambda body detection
                if (c == '=' && i + 1 < len && code[i + 1] == '>')
                {
                    result.Append('=');
                    result.Append('>');
                    i += 2;
                    while (
                        i < len
                        && (code[i] == ' ' || code[i] == '\t' || code[i] == '\r' || code[i] == '\n')
                    )
                    {
                        result.Append(code[i]);
                        i++;
                    }
                    if (i < len && code[i] == '{')
                    {
                        braceIsLambda.Push(true);
                        lambdaDepth++;
                        result.Append('{');
                        i++;
                    }
                    continue;
                }

                if (c == '{')
                {
                    braceIsLambda.Push(false);
                    result.Append(c);
                    i++;
                    continue;
                }
                if (c == '}')
                {
                    if (braceIsLambda.Count > 0 && braceIsLambda.Pop())
                        lambdaDepth--;
                    result.Append(c);
                    i++;
                    continue;
                }

                // Detect 'return' keyword at lambda depth 0
                if (
                    lambdaDepth == 0
                    && c == 'r'
                    && i + 5 < len
                    && code[i + 1] == 'e'
                    && code[i + 2] == 't'
                    && code[i + 3] == 'u'
                    && code[i + 4] == 'r'
                    && code[i + 5] == 'n'
                    && (i + 6 >= len || !char.IsLetterOrDigit(code[i + 6]) && code[i + 6] != '_')
                )
                {
                    if (i > 0 && (char.IsLetterOrDigit(code[i - 1]) || code[i - 1] == '_'))
                    {
                        result.Append(c);
                        i++;
                        continue;
                    }

                    int afterReturn = i + 6;
                    int exprStart = afterReturn;
                    while (
                        exprStart < len
                        && (
                            code[exprStart] == ' '
                            || code[exprStart] == '\t'
                            || code[exprStart] == '\r'
                            || code[exprStart] == '\n'
                        )
                    )
                        exprStart++;

                    // Case 1: return null;
                    if (
                        exprStart + 4 <= len
                        && code[exprStart] == 'n'
                        && code[exprStart + 1] == 'u'
                        && code[exprStart + 2] == 'l'
                        && code[exprStart + 3] == 'l'
                    )
                    {
                        int afterNull = exprStart + 4;
                        while (
                            afterNull < len
                            && (
                                code[afterNull] == ' '
                                || code[afterNull] == '\t'
                                || code[afterNull] == '\r'
                                || code[afterNull] == '\n'
                            )
                        )
                            afterNull++;
                        if (afterNull < len && code[afterNull] == ';')
                        {
                            result.Append("continue;");
                            i = afterNull + 1;
                            continue;
                        }
                    }

                    // Case 2/3: return EXPR; or return (EXPR);
                    int semi = FindStatementEnd(code, exprStart);
                    if (semi >= 0)
                    {
                        string expr = code.Substring(exprStart, semi - exprStart).Trim();
                        if (expr.Length >= 2 && expr[0] == '(')
                        {
                            int matchClose = FindMatchingClose(expr, 0, '(', ')');
                            if (matchClose == expr.Length - 1)
                                expr = expr.Substring(1, expr.Length - 2).Trim();
                        }
                        result.Append(listVar).Append(".Add(").Append(expr).Append("); continue;");
                        i = semi + 1;
                        continue;
                    }
                }

                result.Append(c);
                i++;
            }

            return result.ToString();
        }

        private static int FindStatementEnd(string code, int start)
        {
            int depth = 0;
            int i = start;
            int len = code.Length;
            while (i < len)
            {
                char c = code[i];
                if (c == '"')
                {
                    if (i > 0 && code[i - 1] == '@')
                    {
                        i++;
                        while (i < len)
                        {
                            if (code[i] == '"')
                            {
                                if (i + 1 < len && code[i + 1] == '"')
                                {
                                    i += 2;
                                }
                                else
                                {
                                    i++;
                                    break;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                        continue;
                    }
                    i++;
                    while (i < len)
                    {
                        if (code[i] == '\\')
                        {
                            i += 2;
                        }
                        else if (code[i] == '"')
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    continue;
                }
                if (c == '\'')
                {
                    i++;
                    while (i < len)
                    {
                        if (code[i] == '\\')
                        {
                            i += 2;
                        }
                        else if (code[i] == '\'')
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    continue;
                }
                if (c == '(' || c == '{' || c == '[')
                {
                    depth++;
                    i++;
                    continue;
                }
                if (c == ')' || c == '}' || c == ']')
                {
                    depth--;
                    i++;
                    continue;
                }
                if (c == ';' && depth == 0)
                    return i;
                i++;
            }
            return -1;
        }

        private static int FindMatchingClose(string s, int start, char open, char close)
        {
            int depth = 0;
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == open)
                    depth++;
                else if (s[i] == close)
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
            return -1;
        }

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

        // ── Hook signature extraction ──────────────────────────────────────────

        /// <summary>
        /// Matches any hook call in setup code — both user-written camelCase
        /// (useState, useEffect) and fully-qualified PascalCase (Hooks.UseState).
        /// Captures the hook name (without "Hooks." prefix) in group 1.
        /// </summary>
        private static readonly Regex s_hookSignatureRe = new Regex(
            @"(?:Hooks\.)?\b(useState|useEffect|useLayoutEffect|useRef|useCallback|useMemo|useContext|useReducer|useSignal|useDeferredValue|useTransition|useSafeArea|useStableFunc|useStableAction|useStableCallback|useImperativeHandle|useAnimate|useTweenFloat|useSfx|provideContext|UseState|UseEffect|UseLayoutEffect|UseRef|UseCallback|UseMemo|UseContext|UseReducer|UseSignal|UseDeferredValue|UseTransition|UseSafeArea|UseStableFunc|UseStableAction|UseStableCallback|UseImperativeHandle|UseAnimate|UseTweenFloat|UseSfx|ProvideContext)(?:<[^>]*>)?\s*\(",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Scans raw setup code for hook call patterns and returns
        /// a comma-separated ordered signature string (e.g. "UseState,UseEffect,UseMemo").
        /// Returns empty string if no hooks are found.
        /// </summary>
        internal static string ExtractHookSignature(string setupCode)
        {
            if (string.IsNullOrWhiteSpace(setupCode))
                return string.Empty;

            var matches = s_hookSignatureRe.Matches(setupCode);
            if (matches.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < matches.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(NormalizeHookName(matches[i].Groups[1].Value));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Normalizes a hook name to its canonical PascalCase form
        /// matching the runtime hook ID constants in Hooks.cs.
        /// </summary>
        private static string NormalizeHookName(string name)
        {
            // Already PascalCase
            if (char.IsUpper(name[0]))
                return name;

            // camelCase → PascalCase: useState → UseState, provideContext → ProvideContext
            if (name.StartsWith("use"))
                return "Use" + char.ToUpper(name[3]) + name.Substring(4);
            if (name.StartsWith("provide"))
                return "Provide" + char.ToUpper(name[7]) + name.Substring(8);

            return name;
        }

        // ── Asset path resolution (mirrors CSharpEmitter) ─────────────────────

        internal static string ResolveAssetPaths(string expression, string filePath)
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

        // ── Auto-discovery of V.* factory methods (mirrors SG's
        // PropsResolver.BuildBuiltinMapFromCompilation) ─────────────────────
        //
        // Walks every public static <c>VirtualNode</c>-returning method on
        // <c>global::ReactiveUITK.V</c> and classifies each by its first
        // parameter's type:
        //
        //   • first param ends in "Props"  → Typed (or TypedC if last param is
        //                                    <c>params VirtualNode[]</c>)
        //   • first param implements
        //     <c>IDictionary</c>/<c>IReadOnlyDictionary&lt;,&gt;</c>          → Dict
        //   • method name is "Fragment"                                       → Fragment
        //   • method name is "Text" + first param string                      → Text
        //   • everything else (Func/Memo/Portal/Suspense/Router/etc.)         → SKIP
        //                                                                       (manual
        //                                                                       overrides
        //                                                                       below
        //                                                                       handle
        //                                                                       the few
        //                                                                       markup-relevant
        //                                                                       skips)
        //
        // Generic methods (<c>V.Func&lt;T&gt;</c>) are skipped — they're
        // resolved via the FuncComponent path, not the typed path. Inherited
        // methods from <c>System.Object</c> are excluded via
        // <c>BindingFlags.DeclaredOnly</c> (and would be filtered by the
        // VirtualNode return-type check anyway).
        //
        // The discovery algorithm is also mirrored verbatim by
        // <c>HmrBuiltinTagDiscoveryContractTests</c> in the SG test project
        // (which cannot load this assembly because it depends on
        // <c>UnityEditor</c>). If the algorithm here changes, the mirror in
        // the test must change in lockstep.
        private static class HmrBuiltinTagDiscovery
        {
            public static Dictionary<string, TagRes> BuildAutoDiscoveredTagMap()
            {
                var map = new Dictionary<string, TagRes>(StringComparer.OrdinalIgnoreCase);
                var vType = typeof(global::ReactiveUITK.V);
                var vNodeType = typeof(global::ReactiveUITK.Core.VirtualNode);
                var vNodeArrayType = typeof(global::ReactiveUITK.Core.VirtualNode[]);
                var paramArrayAttr = typeof(ParamArrayAttribute);

                foreach (
                    var m in vType.GetMethods(
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly
                    )
                )
                {
                    if (m.IsGenericMethodDefinition)
                        continue;
                    if (m.ReturnType != vNodeType)
                        continue;
                    var ps = m.GetParameters();
                    if (ps.Length == 0)
                        continue;

                    var firstType = ps[0].ParameterType;
                    bool acceptsChildren =
                        ps[ps.Length - 1].IsDefined(paramArrayAttr, false)
                        && ps[ps.Length - 1].ParameterType == vNodeArrayType;

                    // V.Fragment(string key = null, params VirtualNode[] children)
                    if (m.Name == "Fragment")
                    {
                        map["fragment"] = new TagRes(TagKind.Fragment, "Fragment", null);
                        continue;
                    }

                    // V.Text(string text, string key = null)
                    if (m.Name == "Text" && firstType == typeof(string))
                    {
                        map["text"] = new TagRes(TagKind.Text, "Text", null);
                        continue;
                    }

                    // Typed: first param is a *Props class.
                    if (firstType.Name.EndsWith("Props", StringComparison.Ordinal))
                    {
                        var kind = acceptsChildren ? TagKind.TypedC : TagKind.Typed;
                        var key = m.Name.ToLowerInvariant();
                        // If overloads collide, prefer the no-children variant
                        // (Typed) — same precedence as the legacy literal map.
                        if (!map.TryGetValue(key, out var _existing) || kind == TagKind.Typed)
                        {
                            map[key] = new TagRes(kind, m.Name, firstType.Name);
                        }
                        continue;
                    }

                    // Dictionary-based (e.g. anything that takes IDictionary).
                    if (
                        typeof(System.Collections.IDictionary).IsAssignableFrom(firstType)
                        || (
                            firstType.IsGenericType
                            && firstType.GetGenericTypeDefinition()
                                == typeof(IReadOnlyDictionary<,>)
                        )
                    )
                    {
                        map[m.Name.ToLowerInvariant()] = new TagRes(TagKind.Dict, m.Name, null);
                        continue;
                    }

                    // All other shapes (Func/Memo/Portal/Suspense/Router/etc.)
                    // are skipped here — they're either routed through the
                    // FuncComponent / component-alias paths, or handled by the
                    // explicit manual overrides below.
                }

                // ── Manual overrides ────────────────────────────────────────
                // Tags whose V.* factory has a non-*Props first parameter but
                // whose markup tag must still resolve through the typed-path
                // emitter.

                // V.Suspense(Func<bool>|Task, …) → first param is Func/Task.
                map["suspense"] = new TagRes(TagKind.Suspense, "Suspense", null);
                // V.Portal(VisualElement target, …) → first param is VE.
                map["portal"] = new TagRes(TagKind.Portal, "Portal", null);
                // V.VisualElementSafe(object props, …) → dict-shape, first
                // param is `object` (not detectable as IDictionary at type
                // level since the runtime cast happens inside V).
                map["visualelementsafe"] = new TagRes(TagKind.Dict, "VisualElementSafe", null);

                return map;
            }
        }
    }
}
