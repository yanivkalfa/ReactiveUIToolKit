using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.Roslyn
{
    // ── Collected expression ──────────────────────────────────────────────────

    /// <summary>
    /// A single C# expression (inline or attribute) gathered during the AST walk,
    /// ready to be written into the virtual document as a type-checking wrapper.
    /// </summary>
    internal sealed class CollectedExpression
    {
        /// <summary>Trimmed C# expression text.</summary>
        public string Text { get; init; } = "";

        /// <summary>Absolute char offset in the .uitkx source where <see cref="Text"/> starts.</summary>
        public int UitkxOffset { get; init; }

        /// <summary>1-based line in the .uitkx source.</summary>
        public int UitkxLine { get; init; }

        /// <summary>Label appended to the generated method/variable name (e.g. "attr_0_text").</summary>
        public string Label { get; init; } = "";

        public SourceRegionKind Kind { get; init; }
    }

    // ── Virtual document ──────────────────────────────────────────────────────

    /// <summary>
    /// The generated virtual C# document for a single .uitkx file, together with
    /// the <see cref="SourceMap"/> that maps positions back to the .uitkx source.
    /// </summary>
    public sealed class VirtualDocument
    {
        /// <summary>
        /// Full text of the virtual C# file, encoded with LF-only line endings.
        /// Feed this to <c>AdhocWorkspace.AddDocument</c> to get a Roslyn document.
        /// </summary>
        public string Text { get; }

        /// <summary>Bidirectional position map between virtual C# and .uitkx source.</summary>
        public SourceMap Map { get; }

        /// <summary>The .uitkx source path this document was generated from.</summary>
        public string UitkxFilePath { get; }

        internal VirtualDocument(string text, SourceMap map, string uitkxFilePath)
        {
            Text          = text;
            Map           = map;
            UitkxFilePath = uitkxFilePath;
        }
    }

    // ── Builder (internal) ────────────────────────────────────────────────────

    /// <summary>
    /// Stateful builder used during document generation.  Tracks the current
    /// byte offset in the output <see cref="StringBuilder"/> so mapped spans can be
    /// recorded precisely.
    /// </summary>
    internal sealed class VirtualDocBuilder
    {
        private readonly StringBuilder _sb    = new StringBuilder(4096);
        private readonly List<SourceMapEntry> _entries = new List<SourceMapEntry>();
        private int _virtualPos;

        // ── Scaffold text (no source-map entry) ──────────────────────────────

        /// <summary>Appends generated-scaffold text that does not map to any .uitkx position.</summary>
        public void Scaffold(string text)
        {
            _sb.Append(text);
            _virtualPos += text.Length;
        }

        // ── Mapped region ────────────────────────────────────────────────────

        /// <summary>
        /// Appends text that was copied verbatim from the .uitkx source, recording
        /// the bidirectional span so the source map can translate positions.
        /// </summary>
        public void Mapped(string text, int uitkxStart, SourceRegionKind kind, int uitkxLine)
        {
            if (text.Length == 0)
                return;

            int vStart = _virtualPos;
            _sb.Append(text);
            _virtualPos += text.Length;

            _entries.Add(new SourceMapEntry(
                VirtualStart: vStart,
                VirtualEnd:   _virtualPos,
                UitkxStart:   uitkxStart,
                UitkxEnd:     uitkxStart + text.Length,
                Kind:         kind,
                UitkxLine:    uitkxLine));
        }

        // ── Output ───────────────────────────────────────────────────────────

        public VirtualDocument Build(string uitkxFilePath) =>
            new VirtualDocument(
                text:          _sb.ToString(),
                map:           new SourceMap(_entries.ToImmutableArray()),
                uitkxFilePath: uitkxFilePath);

        public int CurrentPos => _virtualPos;
    }

    // ── Generator ────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates an in-memory C# source file (the "virtual document") from a
    /// parsed UITKX file.
    ///
    /// <para><b>Purpose:</b> The virtual document is fed to a Roslyn
    /// <c>AdhocWorkspace</c> so the language server can answer type-related
    /// questions about the C# regions embedded inside the .uitkx file:
    /// diagnostics, completions, hover, and semantic token classification.</para>
    ///
    /// <para><b>Virtual document structure — classic directive-based style:</b></para>
    /// <code>
    /// // &lt;auto-generated&gt;
    /// #line hidden
    /// #pragma warning disable ...
    /// using System; using ReactiveUITK.Core; ... // standard + @usings
    ///
    /// namespace {Namespace} {
    ///     partial class {Component}Func {
    /// #line hidden
    ///         private {PropsType} props = default!;
    ///         private {InjectType} {injectName} = default!;
    ///
    ///         // One wrapper method per @(expr) / attr={expr}:
    /// #line {uitkxLine} "{uitkxFilePath}"
    ///         private object __uitkx_{label}() { return ({expression}); }
    /// #line hidden
    ///
    ///         // @code block verbatim (class-body context):
    /// #line {codeStartLine} "{uitkxFilePath}"
    ///         {code body}
    /// #line default
    ///     }
    /// }
    /// </code>
    ///
    /// <para><b>Virtual document structure — function-style:</b>
    /// All expressions plus the setup code are placed inside a
    /// <c>__uitkx_render()</c> method so local variables declared in setup code
    /// are visible to expressions that reference them.</para>
    ///
    /// <para><b>Source map:</b> Every C# expression region records a
    /// <see cref="SourceMapEntry"/> so the calling code can convert positions in
    /// either direction between the virtual document and the .uitkx source.</para>
    ///
    /// <para><b>Language-lib constraint:</b> This class has NO dependency on Roslyn
    /// NuGet packages.  It lives in <c>language-lib</c> which targets
    /// <c>netstandard2.0</c> and must stay Roslyn-free so it can be referenced by
    /// the Unity source-generator project.</para>
    /// </summary>
    public sealed class VirtualDocumentGenerator
    {
        // ── Standard using directives ─────────────────────────────────────────

        /// <summary>
        /// Usings always injected into every virtual document.
        /// These cover the types most commonly found in .uitkx @code blocks and
        /// expressions, preventing false CS0246 errors.
        /// The list is intentionally conservative — only assemblies that the
        /// Unity project always contains.
        /// </summary>
        private static readonly string[] s_standardUsings =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "UnityEngine",
            // UnityEngine.UIElements intentionally omitted: it exports a FlexDirection enum
            // that conflicts with the FlexDirection string constant from
            // `using static ReactiveUITK.Props.Typed.StyleKeys` (CS0104).
            "ReactiveUITK.Core",
            "ReactiveUITK.Core.Animation",
            "ReactiveUITK.Props.Typed",
        };

        /// <summary>
        /// Extra lines appended after the normal using directives.
        /// These are not simple namespace usings (static / alias forms).
        /// </summary>
        private static readonly string[] s_extraUsingLines =
        {
            "using static ReactiveUITK.Props.Typed.StyleKeys;",
            "using UColor = UnityEngine.Color;",
            // `using static StyleKeys` imports a `Color` string constant (TextColor prop key).
            // UnityEngine.UIElementsModule.dll also defines UnityEngine.Color → CS0104.
            // An explicit alias always wins over both, so declare it last.
            "using Color = UnityEngine.Color;",
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the virtual C# document for <paramref name="parseResult"/> and
        /// returns a <see cref="VirtualDocument"/> that includes both the text and
        /// the bidirectional source map.
        /// </summary>
        /// <param name="parseResult">
        /// The fully-parsed UITKX document (directives + AST + diagnostics).
        /// </param>
        /// <param name="source">
        /// The raw .uitkx source text.  Must be the same string that was used to
        /// produce <paramref name="parseResult"/> so source offsets are valid.
        /// </param>
        /// <param name="uitkxFilePath">
        /// Absolute path to the .uitkx file, written into <c>#line</c> directives.
        /// </param>
        public VirtualDocument Generate(ParseResult parseResult, string source, string uitkxFilePath)
        {
            var b = new VirtualDocBuilder();
            var d = parseResult.Directives;

            // ── File header ──────────────────────────────────────────────────
            b.Scaffold("// <auto-generated: UITKX Roslyn virtual document>\n");
            b.Scaffold($"// Source: {EscapeForComment(uitkxFilePath)}\n");
            b.Scaffold("// DO NOT EDIT — regenerated on every document change.\n");
            b.Scaffold("#line hidden\n");
            b.Scaffold(
                "#pragma warning disable CS0169, CS0414, CS8618, CS8019, CS1591, CS0649, CS0246, CS0411, CS1660, CS1026, CS1513\n\n"
            );

            // ── Using directives ─────────────────────────────────────────────
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var u in s_standardUsings)
            {
                if (seen.Add(u))
                    b.Scaffold($"using {u};\n");
            }
            foreach (var u in d.Usings)
            {
                string trimmed = u.Trim();
                if (!string.IsNullOrEmpty(trimmed) && seen.Add(trimmed))
                    b.Scaffold($"using {trimmed};\n");
            }
            // Static / alias usings that cannot go through the simple "using X;" path
            foreach (var line in s_extraUsingLines)
                b.Scaffold(line + "\n");
            b.Scaffold("\n");

            // ── Namespace + class header ─────────────────────────────────────
            string ns        = !string.IsNullOrEmpty(d.Namespace) ? d.Namespace! : "ReactiveUITK.Generated";
            string className = (!string.IsNullOrEmpty(d.ComponentName) ? d.ComponentName! : "Component") + "Func";
            string escapedPath = EscapePathForLineDirective(uitkxFilePath);

            b.Scaffold($"namespace {ns}\n{{\n");
            b.Scaffold($"    partial class {className}\n    {{\n");
            b.Scaffold("#line hidden\n");

            if (d.IsFunctionStyle)
                EmitFunctionStyleBody(b, parseResult, source, escapedPath);
            else
                EmitClassicBody(b, parseResult, source, escapedPath);

            // ── Close class + namespace ──────────────────────────────────────
            b.Scaffold("    }\n}\n");

            return b.Build(uitkxFilePath);
        }

        // ── Classic directive-based component ─────────────────────────────────

        private static void EmitClassicBody(
            VirtualDocBuilder b,
            ParseResult parseResult,
            string source,
            string escapedPath)
        {
            var d = parseResult.Directives;

            // Props field
            string propsType = !string.IsNullOrEmpty(d.PropsTypeName) ? d.PropsTypeName! : "object";
            b.Scaffold($"        private {propsType} props = default!;\n");

            // @inject fields
            foreach (var (injectType, injectName) in d.Injects)
                b.Scaffold($"        private {injectType} {injectName} = default!;\n");

            b.Scaffold("\n");

            // Collect all C# expressions in markup, numbered for unique method names
            var expressions = new List<CollectedExpression>();
            CollectExpressions(parseResult.RootNodes, expressions);

            // Expression wrapper methods (instance — so props / inject fields are in scope)
            if (expressions.Count > 0)
            {
                b.Scaffold(
                    "        // ── C# expression type-checking wrappers ───────────────────────\n"
                );
                b.Scaffold("        // These methods are NEVER called; they exist only for Roslyn.\n\n");

                foreach (var expr in expressions)
                    EmitExpressionWrapper(b, expr, escapedPath, indent: "        ");
            }

            // @code block (class-body context)
            EmitCodeBlock(b, parseResult.RootNodes, source, escapedPath);
        }

        // ── Function-style component ──────────────────────────────────────────

        private static void EmitFunctionStyleBody(
            VirtualDocBuilder b,
            ParseResult parseResult,
            string source,
            string escapedPath)
        {
            var d = parseResult.Directives;

            // Typed parameters become private fields so the render method can access them.
            if (!d.FunctionParams.IsDefaultOrEmpty)
            {
                foreach (var p in d.FunctionParams)
                    b.Scaffold($"        private {p.Type} {p.Name};\n");
                b.Scaffold("\n");
            }

            // ── Hook shorthand stubs ──────────────────────────────────────────
            // The authoring shorthands useState / useMemo / useEffect etc. are NOT
            // rewritten before being fed to Roslyn, so we scaffold private methods
            // with the correct return types so Roslyn can type-check the setup code
            // without CS0103 / CS8130 / CS1026 errors.
            b.Scaffold(
                "\n" +
                "        // ── Roslyn-only hook stubs (never called at runtime) ──────────────\n" +
                "#pragma warning disable CS8603, CS8625, CS1998\n" +
                // Custom delegate: T __StateUpdater__<T>(T prev) matches BOTH usage patterns:\n                //   setX(newValue)          → setX called with T, return T discarded ✓\n                //   setX(prev => { ... })   → lambda T→T matches delegate T(T) exactly ✓\n                "        private delegate T __StateUpdater__<T>(T prev);\n" +
                "        private (T value, __StateUpdater__<T> set)\n" +
                "            useState<T>(T initial = default) => (initial, null!);\n" +
                "        private T useMemo<T>(global::System.Func<T> factory, params object[] deps)\n" +
                "            => factory != null ? factory() : default!;\n" +
                "        private void useEffect(\n" +
                "            global::System.Func<global::System.Action> effectFactory,\n" +
                "            params object[] deps) { }\n" +
                "        private global::ReactiveUITK.Ref<T> useRef<T>(T initial = default) => new();\n" +
                "        private global::UnityEngine.UIElements.VisualElement useRef() => null!;\n" +
                "        private global::System.Func<T> useCallback<T>(\n" +
                "            global::System.Func<T> callback, params object[] deps) => callback!;\n" +
                "        private T useSignal<T>(object signal) => default!;\n" +
                "        private T useSignal<T>(string key, T initialValue = default) => initialValue;\n" +
                "        private T useContext<T>(string key) => default!;\n" +
                "        private void provideContext<T>(string key, T value) { }\n" +
                "        private void provideContext(string key, object value) { }\n" +
                "        private void useLayoutEffect(\n" +
                "            global::System.Func<global::System.Action> effectFactory,\n" +
                "            params object[] deps) { }\n" +
                "#pragma warning restore CS8603, CS8625, CS1998\n\n");

            // Collect markup nodes — skip the setup CodeBlockNode whose JSX paren
            // blocks are already replaced with (object)null! placeholders by
            // EmitFunctionStyleSetupSegmented.  Emitting attribute expressions from those
            // replaced JSX nodes (e.g. onClick={_ => ...}) produces spurious CS0411 errors
            // because the lambda has no delegate-type context in the generated code.
            var markupOnlyNodes = ImmutableArray.CreateBuilder<AstNode>(parseResult.RootNodes.Length);
            foreach (var n in parseResult.RootNodes)
                if (n is not CodeBlockNode)
                    markupOnlyNodes.Add(n);

            // All code goes inside a single render method so local vars from setup
            // code are visible to expressions.
            // Return type is `object` (not void) so that JSX paren blocks replaced with
            // `return (object)null!` in conditional branches (e.g. `if (...) return (...)`)
            // are valid C# — a void method can't return a value.
            b.Scaffold("        private object __uitkx_render()\n        {\n");

            // Setup code — emitted in segments so that JSX paren blocks
            // (e.g. `var x = (<Box>...</Box>)`) are replaced with a valid C#
            // placeholder and never seen by Roslyn as markup.
            if (!string.IsNullOrEmpty(d.FunctionSetupCode) && d.FunctionSetupStartLine > 0)
            {
                EmitFunctionStyleSetupSegmented(
                    b,
                    d.FunctionSetupCode!,
                    uitkxSetupStartOffset: OffsetOfLine(source, d.FunctionSetupStartLine),
                    uitkxSetupStartLine:   d.FunctionSetupStartLine,
                    escapedPath:           escapedPath);
            }

            // Expression checks — emitted in-scope so that loop variables declared
            // in @for / @foreach / @while headers are visible inside the body.
            b.Scaffold(
                "            // ── Expression type checks ─────────────────────────────────\n"
            );
            int __exprCtr = 0; int __attrCtr = 0;
            EmitNodeExpressionsScoped(
                markupOnlyNodes.ToImmutable(), b, escapedPath,
                indent: "            ", ref __exprCtr, ref __attrCtr);

            // Ensure all code paths return — components whose setup code only has
            // conditional `return (object)null!` branches need a fallback.
            b.Scaffold("            return default!;\n");
            b.Scaffold("        }\n"); // close render method
        }

        // ── Expression wrappers / statements ─────────────────────────────────

        /// <summary>
        /// Emits a private instance method wrapping a single expression.
        /// Used in classic (class-body) context.
        /// </summary>
        private static void EmitExpressionWrapper(
            VirtualDocBuilder b,
            CollectedExpression expr,
            string escapedPath,
            string indent)
        {
            // #line directive maps Roslyn errors back to the uitkx line
            b.Scaffold($"#line {expr.UitkxLine} \"{escapedPath}\"\n");

            // Scaffold: method signature up to the opening paren
            string methodName = $"__uitkx_{expr.Label}";
            b.Scaffold($"{indent}private object {methodName}() {{ return (");

            // Mapped region: the expression text itself (1-to-1 source map)
            b.Mapped(expr.Text, expr.UitkxOffset, expr.Kind, expr.UitkxLine);

            // Scaffold: close method
            b.Scaffold("); }\n");
            b.Scaffold("#line hidden\n\n");
        }

        /// <summary>
        /// Emits a block-statement expression check.
        /// Used inside a render method (function-style context).
        /// </summary>
        private static void EmitExpressionStatement(
            VirtualDocBuilder b,
            CollectedExpression expr,
            string escapedPath,
            string indent)
        {
            b.Scaffold($"#line {expr.UitkxLine} \"{escapedPath}\"\n");

            // Lambdas (`expr =>` / `_ =>` / `(x, y) =>`) cannot be stored as
            // `object` — Roslyn emits CS1660 and a secondary "delegate type could
            // not be inferred" diagnostic.  Cast to Action / Action<dynamic> so:
            //  • Zero-arg lambdas `() => ...` → Action (no type param, no arg mismatch)
            //  • One-arg lambdas `e => ...`   → Action<dynamic> so e.newValue etc. compile
            //  • The lambda body is still type-checked against the surrounding scope
            if (expr.Text.Contains("=>"))
            {
                // Block-body lambdas: `dm => { dm.AppendAction(..., _ => ...) }`
                // Casting to Action<dynamic> makes the param `dynamic`, then passing
                // a nested lambda to a dynamic method call triggers CS1977 — an error
                // that cannot be pragma-suppressed.  Skip emitting these entirely so
                // no false squiggles appear; the body is not type-checked (acceptable).
                int arrowIdx = expr.Text.IndexOf("=>");
                bool isBlockBody = arrowIdx >= 0 &&
                    expr.Text.Substring(arrowIdx + 2).TrimStart().StartsWith("{");
                if (isBlockBody)
                {
                    b.Scaffold($"{indent}// (block-body lambda skipped — CS1977 not suppressable)\n");
                }
                else
                {
                    // () => ... is zero-arg; everything else is treated as single-arg dynamic
                    bool isZeroArg = expr.Text.TrimStart().StartsWith("()");
                    string castType = isZeroArg
                        ? "global::System.Action"
                        : "global::System.Action<dynamic>";
                    b.Scaffold($"{indent}{{ (({castType})(");
                    b.Mapped(expr.Text, expr.UitkxOffset, expr.Kind, expr.UitkxLine);
                    b.Scaffold(")); }\n");
                }
            }
            else
            {
                // CS0428 (method group → object) is a warning (orange), not an error — leave it visible.
                b.Scaffold($"{indent}{{ object __uitkx_{expr.Label} = (");
                b.Mapped(expr.Text, expr.UitkxOffset, expr.Kind, expr.UitkxLine);
                b.Scaffold("); }\n");
            }

            b.Scaffold("#line hidden\n");
        }

        /// <summary>
        /// Finds the one <see cref="CodeBlockNode"/> at the root level and emits its
        /// content verbatim inside a <c>#line</c> pair so Roslyn maps errors correctly.
        /// </summary>
        private static void EmitCodeBlock(
            VirtualDocBuilder b,
            ImmutableArray<AstNode> rootNodes,
            string source,
            string escapedPath)
        {
            foreach (var node in rootNodes)
            {
                if (node is not CodeBlockNode cb)
                    continue;

                if (string.IsNullOrEmpty(cb.Code))
                    break;

                b.Scaffold(
                    "\n        // ── @code block (class-body context) ─────────────────────────\n"
                );

                // +1 because @code { lives on SourceLine, code content starts on the line after
                int codeLine = cb.SourceLine + 1;
                b.Scaffold($"#line {codeLine} \"{escapedPath}\"\n");

                // The Code text is inserted verbatim; if CodeContentOffset is tracked
                // use that for precise character mapping, otherwise fall back to a
                // line-only approximation.
                int uitkxStart = cb.CodeContentOffset > 0
                    ? cb.CodeContentOffset
                    : OffsetOfLine(source, codeLine);

                b.Mapped(cb.Code, uitkxStart, SourceRegionKind.CodeBlock, codeLine);
                b.Scaffold("\n");
                b.Scaffold("#line default\n");
                break; // only one @code block per file
            }
        }

        // ── AST expression collector ──────────────────────────────────────────

        /// <summary>
        /// Walks the entire AST and collects every C# expression that needs a
        /// type-checking wrapper in the virtual document:
        /// <list type="bullet">
        ///   <item><c>@(expr)</c> — <see cref="ExpressionNode"/></item>
        ///   <item><c>attr={expr}</c> — <see cref="AttributeNode"/> with <see cref="CSharpExpressionValue"/></item>
        /// </list>
        /// Numbers each expression to produce unique method/variable names.
        /// </summary>
        private static void CollectExpressions(
            ImmutableArray<AstNode> nodes,
            List<CollectedExpression> output)
        {
            int exprCounter = 0;
            int attrCounter = 0;
            CollectFromNodeList(nodes, output, ref exprCounter, ref attrCounter);
        }

        private static void CollectFromNodeList(
            ImmutableArray<AstNode>  nodes,
            List<CollectedExpression> output,
            ref int exprCounter,
            ref int attrCounter)
        {
            foreach (var node in nodes)
                CollectFromNode(node, output, ref exprCounter, ref attrCounter);
        }

        private static void CollectFromNode(
            AstNode                  node,
            List<CollectedExpression> output,
            ref int exprCounter,
            ref int attrCounter)
        {
            switch (node)
            {
                case ExpressionNode en:
                    if (!string.IsNullOrEmpty(en.Expression) && en.ExpressionOffset > 0)
                        output.Add(new CollectedExpression
                        {
                            Text        = en.Expression,
                            UitkxOffset = en.ExpressionOffset,
                            UitkxLine   = en.SourceLine,
                            Label       = $"expr_{exprCounter++}",
                            Kind        = SourceRegionKind.InlineExpression,
                        });
                    else if (!string.IsNullOrEmpty(en.Expression))
                        output.Add(new CollectedExpression
                        {
                            Text        = en.Expression,
                            UitkxOffset = 0,
                            UitkxLine   = en.SourceLine,
                            Label       = $"expr_{exprCounter++}",
                            Kind        = SourceRegionKind.InlineExpression,
                        });
                    break;

                case ElementNode el:
                    // Attribute expressions on this element
                    foreach (var attr in el.Attributes)
                    {
                        if (attr.Value is CSharpExpressionValue cev
                            && !string.IsNullOrEmpty(cev.Expression))
                        {
                            output.Add(new CollectedExpression
                            {
                                Text        = cev.Expression,
                                UitkxOffset = cev.ExpressionOffset,
                                UitkxLine   = attr.SourceLine,
                                Label       = $"attr_{attrCounter++}_{SanitizeLabel(attr.Name)}",
                                Kind        = SourceRegionKind.AttributeExpression,
                            });
                        }
                    }
                    // Recurse into children
                    CollectFromNodeList(el.Children, output, ref exprCounter, ref attrCounter);
                    break;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        CollectFromNodeList(branch.Body, output, ref exprCounter, ref attrCounter);
                    break;

                case ForeachNode fe:
                    CollectFromNodeList(fe.Body, output, ref exprCounter, ref attrCounter);
                    break;

                case ForNode fo:
                    CollectFromNodeList(fo.Body, output, ref exprCounter, ref attrCounter);
                    break;

                case WhileNode wh:
                    CollectFromNodeList(wh.Body, output, ref exprCounter, ref attrCounter);
                    break;

                case SwitchNode sw:
                    foreach (var sc in sw.Cases)
                        CollectFromNodeList(sc.Body, output, ref exprCounter, ref attrCounter);
                    break;

                case CodeBlockNode cb:
                    // Return-markup elements inside @code can also have attribute expressions
                    foreach (var rm in cb.ReturnMarkups)
                        CollectFromNode(rm.Element, output, ref exprCounter, ref attrCounter);
                    break;
            }
        }

        // ── Scoped expression emitter ─────────────────────────────────────────

        /// <summary>
        /// Recursively walks <paramref name="nodes"/> and emits expression type-check
        /// statements, wrapping <c>@for</c> / <c>@foreach</c> / <c>@while</c> bodies
        /// in proper C# block scopes so that loop variables declared in headers are
        /// visible for attribute expressions inside the body.
        /// </summary>
        private static void EmitNodeExpressionsScoped(
            ImmutableArray<AstNode> nodes,
            VirtualDocBuilder b,
            string escapedPath,
            string indent,
            ref int exprCtr,
            ref int attrCtr)
        {
            foreach (var node in nodes)
                EmitNodeExpressionScoped(node, b, escapedPath, indent, ref exprCtr, ref attrCtr);
        }

        private static void EmitNodeExpressionScoped(
            AstNode node,
            VirtualDocBuilder b,
            string escapedPath,
            string indent,
            ref int exprCtr,
            ref int attrCtr)
        {
            switch (node)
            {
                case ExpressionNode en:
                {
                    if (!string.IsNullOrEmpty(en.Expression))
                    {
                        var expr = new CollectedExpression
                        {
                            Text        = en.Expression,
                            UitkxOffset = en.ExpressionOffset,
                            UitkxLine   = en.SourceLine,
                            Label       = $"expr_{exprCtr++}",
                            Kind        = SourceRegionKind.InlineExpression,
                        };
                        EmitExpressionStatement(b, expr, escapedPath, indent);
                    }
                    break;
                }

                case ElementNode el:
                    foreach (var attr in el.Attributes)
                    {
                        if (attr.Value is CSharpExpressionValue cev
                            && !string.IsNullOrEmpty(cev.Expression))
                        {
                            var expr = new CollectedExpression
                            {
                                Text        = cev.Expression,
                                UitkxOffset = cev.ExpressionOffset,
                                UitkxLine   = attr.SourceLine,
                                Label       = $"attr_{attrCtr++}_{SanitizeLabel(attr.Name)}",
                                Kind        = SourceRegionKind.AttributeExpression,
                            };
                            EmitExpressionStatement(b, expr, escapedPath, indent);
                        }
                    }
                    EmitNodeExpressionsScoped(el.Children, b, escapedPath, indent, ref exprCtr, ref attrCtr);
                    break;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        EmitNodeExpressionsScoped(branch.Body, b, escapedPath, indent, ref exprCtr, ref attrCtr);
                    break;

                case ForeachNode fe:
                    b.Scaffold($"{indent}foreach ({fe.IteratorDeclaration} in {fe.CollectionExpression}) {{\n");
                    EmitNodeExpressionsScoped(fe.Body, b, escapedPath, indent + "    ", ref exprCtr, ref attrCtr);
                    b.Scaffold($"{indent}}}\n");
                    break;

                case ForNode fo:
                    b.Scaffold($"{indent}for ({fo.ForExpression}) {{\n");
                    EmitNodeExpressionsScoped(fo.Body, b, escapedPath, indent + "    ", ref exprCtr, ref attrCtr);
                    b.Scaffold($"{indent}}}\n");
                    break;

                case WhileNode wh:
                    b.Scaffold($"{indent}while ({wh.Condition}) {{\n");
                    EmitNodeExpressionsScoped(wh.Body, b, escapedPath, indent + "    ", ref exprCtr, ref attrCtr);
                    b.Scaffold($"{indent}}}\n");
                    break;

                case SwitchNode sw:
                    foreach (var sc in sw.Cases)
                        EmitNodeExpressionsScoped(sc.Body, b, escapedPath, indent, ref exprCtr, ref attrCtr);
                    break;

                case CodeBlockNode cb:
                    foreach (var rm in cb.ReturnMarkups)
                        EmitNodeExpressionScoped(rm.Element, b, escapedPath, indent, ref exprCtr, ref attrCtr);
                    break;
            }
        }

        // ── Utility helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the 0-based character offset of the start of <paramref name="line1"/>
        /// (1-based) within <paramref name="source"/>.  Returns 0 when out of range.
        /// </summary>
        private static int OffsetOfLine(string source, int line1)
        {
            if (line1 <= 1)
                return 0;

            int current = 1;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\n')
                {
                    current++;
                    if (current == line1)
                        return i + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Strips characters from an attribute name that are not valid in a C#
        /// method name suffix (e.g. hyphens in <c>data-id</c>).
        /// </summary>
        private static string SanitizeLabel(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (var c in name)
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            return sb.ToString();
        }

        /// <summary>
        /// Escapes backslashes in a file path for use inside a C# string literal
        /// (used in <c>#line N "path"</c> directives).
        /// </summary>
        private static string EscapePathForLineDirective(string path) =>
            path.Replace("\\", "\\\\");

        /// <summary>Strips newlines from a string for safe inclusion in a // comment.</summary>
        private static string EscapeForComment(string text) =>
            text.Replace('\r', ' ').Replace('\n', ' ');

        // ── JSX-stripping setup-code emitter ──────────────────────────────────

        /// <summary>
        /// Emits the function-style setup code in segments, replacing any
        /// parenthesised JSX blocks — i.e. <c>(<Tag ...>...</Tag>)</c> or
        /// <c>(<Tag .../> )</c> — with the scaffold placeholder <c>(null!)</c>.
        ///
        /// <para>JSX paren blocks produce invalid C# when placed verbatim inside
        /// the generated <c>__uitkx_render()</c> method, causing cascading Roslyn
        /// errors.  By replacing them with <c>(null!)</c> and padding with the
        /// same number of newlines, both Roslyn's <c>#line</c> tracking and the
        /// internal source map stay accurate for all surrounding C# code.</para>
        /// </summary>
        private static void EmitFunctionStyleSetupSegmented(
            VirtualDocBuilder b,
            string setupCode,
            int uitkxSetupStartOffset,
            int uitkxSetupStartLine,
            string escapedPath)
        {
            int segStart   = 0;
            int currentLine = uitkxSetupStartLine;
            int i           = 0;

            while (i < setupCode.Length)
            {
                // Only look for `(` that immediately precedes JSX
                if (setupCode[i] != '(')
                {
                    i++;
                    continue;
                }

                // Peek past whitespace to see if the first non-ws char is '<'
                int peek = i + 1;
                while (peek < setupCode.Length &&
                       (setupCode[peek] == ' '  || setupCode[peek] == '\t' ||
                        setupCode[peek] == '\r' || setupCode[peek] == '\n'))
                    peek++;

                if (peek >= setupCode.Length || setupCode[peek] != '<')
                {
                    i++;
                    continue;
                }

                // ── Found a JSX paren block starting at i ──────────────────

                // 1. Emit the C# segment that precedes this block.
                if (i > segStart)
                {
                    string seg     = setupCode.Substring(segStart, i - segStart);
                    int    segLine = currentLine;
                    b.Scaffold($"#line {segLine} \"{escapedPath}\"\n");
                    b.Mapped(seg,
                             uitkxSetupStartOffset + segStart,
                             SourceRegionKind.FunctionSetup,
                             segLine);
                    b.Scaffold("\n#line hidden\n");
                    // Advance currentLine by the newlines inside the segment.
                    currentLine += CountNewlines(seg);
                }

                // 2. Find the matching close paren (depth-balanced, ignores {}).
                int depth = 1;
                int j     = i + 1;
                while (j < setupCode.Length && depth > 0)
                {
                    if      (setupCode[j] == '(') depth++;
                    else if (setupCode[j] == ')') depth--;
                    j++;
                }
                // setupCode[i..j) is the complete `(<JSX>...)` block.

                // 3. Scaffold a valid C# placeholder with the same newline count
                //    so Roslyn's #line tracking stays in sync.
                //    Use (object)null! so `var x = (object)null!` compiles (CS0815
                //    fires when the type cannot be inferred from a bare null literal).
                int jsxNewlines = CountNewlines(setupCode, i, j);
                b.Scaffold("(object)null!");
                for (int k = 0; k < jsxNewlines; k++)
                    b.Scaffold("\n");

                currentLine += jsxNewlines;
                segStart     = j;
                i            = j;
            }

            // 4. Emit any trailing C# segment after the last JSX block.
            if (segStart < setupCode.Length)
            {
                string seg     = setupCode.Substring(segStart);
                int    segLine = currentLine;
                b.Scaffold($"#line {segLine} \"{escapedPath}\"\n");
                b.Mapped(seg,
                         uitkxSetupStartOffset + segStart,
                         SourceRegionKind.FunctionSetup,
                         segLine);
                b.Scaffold("\n#line hidden\n");
            }

            b.Scaffold("\n");
        }

        /// <summary>Counts '\n' characters in <paramref name="s"/>.</summary>
        private static int CountNewlines(string s)
        {
            int count = 0;
            foreach (char c in s)
                if (c == '\n') count++;
            return count;
        }

        /// <summary>Counts '\n' characters in <paramref name="s"/> between
        /// <paramref name="start"/> (inclusive) and <paramref name="end"/> (exclusive).</summary>
        private static int CountNewlines(string s, int start, int end)
        {
            int count = 0;
            for (int i = start; i < end && i < s.Length; i++)
                if (s[i] == '\n') count++;
            return count;
        }
    }
}
