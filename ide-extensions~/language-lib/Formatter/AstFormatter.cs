using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.Formatter
{
    /// <summary>
    /// AST-based formatter for <c>.uitkx</c> files.
    ///
    /// Call <see cref="Format"/> with the source text; on any parse error the
    /// original source is returned unchanged so the formatter can never corrupt a
    /// file.  Formatting options come from <see cref="FormatterOptions"/>; load
    /// them from disk with <see cref="ConfigLoader.LoadFormatterOptions"/>.
    ///
    /// Architecture: parse the source into a <see cref="ParseResult"/>, walk the
    /// AST tree, and emit canonical text into a <see cref="StringBuilder"/>.
    /// No regular-expression post-processing is needed because we work from the
    /// structured representation, not from the original character stream.
    /// </summary>
    public sealed class AstFormatter
    {
        private readonly FormatterOptions _opts;
        private readonly ICSharpFormatterDelegate? _csharpFormatter;
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent;

        // Set at the start of each Format() call; used by EmitSetupCodeWithJsx.
        private string _source = "";
        private string _filePath = "";

        public AstFormatter(FormatterOptions opts, ICSharpFormatterDelegate? csharpFormatter = null)
        {
            _opts = opts;
            _csharpFormatter = csharpFormatter;
        }

        public AstFormatter()
            : this(FormatterOptions.Default) { }

        // ═══════════════════════════════════════════════════════════════════════
        //  PUBLIC ENTRY POINT
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Parse <paramref name="source"/>, format it, and return the result.
        /// Returns the original source unchanged if there are any parse errors.
        /// <paramref name="filePath"/> is only used for diagnostic position
        /// reporting and may be empty.
        /// </summary>
        public string Format(string source, string filePath = "")
        {
            _sb.Clear();
            _indent = 0;

            // Normalise to LF-only so all AppendLine helpers stay consistent.
            source = source.Replace("\r\n", "\n").Replace("\r", "\n");
            _source = source;
            _filePath = filePath;

            var diags = new List<ParseDiagnostic>();
            var directives = DirectiveParser.Parse(source, filePath, diags);
            var nodes = UitkxParser.Parse(source, filePath, directives, diags);

            foreach (var d in diags)
                if (d.Severity == ParseSeverity.Error)
                    return source;

            // ── Hook/module files: dedicated path ─────────────────────────────
            // Must be checked BEFORE IsFunctionStyle because the parser sets that
            // flag for hook/module files too.
            if (!directives.HookDeclarations.IsDefaultOrEmpty
                || !directives.ModuleDeclarations.IsDefaultOrEmpty)
            {
                FormatHookModuleFile(source, directives);
            }
            else if (directives.IsFunctionStyle)
            {
                FormatFunctionStyleComponent(directives, nodes);
            }
            else
            {
                FormatNodeList(nodes, topLevel: true);
            }

            // Trim any trailing whitespace/newlines and add exactly one trailing \n.
            var result = _sb.ToString().TrimEnd('\r', '\n', ' ', '\t');
            return result + "\n";
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DIRECTIVES
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatFunctionStyleComponent(
            DirectiveSet directives,
            ImmutableArray<AstNode> nodes
        )
        {
            var componentName = string.IsNullOrWhiteSpace(directives.ComponentName)
                ? "Component"
                : directives.ComponentName;

            // ── Preamble: re-emit @namespace / using lines before the component block ─
            bool hasPreamble = false;

            if (directives.HasExplicitNamespace && !string.IsNullOrWhiteSpace(directives.Namespace))
            {
                Ln($"@namespace {directives.Namespace}");
                hasPreamble = true;
            }

            foreach (var u in directives.Usings)
            {
                Ln($"@using {u}");
                hasPreamble = true;
            }

            if (!directives.UssFiles.IsDefaultOrEmpty)
            {
                foreach (var uss in directives.UssFiles)
                {
                    Ln($"@uss \"{uss}\"");
                    hasPreamble = true;
                }
            }

            if (hasPreamble)
                _sb.Append('\n');

            string paramList = "";
            if (!directives.FunctionParams.IsDefaultOrEmpty)
            {
                var parts = directives.FunctionParams.Select(p =>
                    p.DefaultValue != null
                        ? $"{p.Type} {p.Name} = {p.DefaultValue}"
                        : $"{p.Type} {p.Name}"
                ).ToList();
                paramList = $"({string.Join(", ", parts)})";
            }

            string headerLine = $"component {componentName}{paramList} {{";
            if (paramList.Length > 0 && headerLine.Length > _opts.PrintWidth)
            {
                // Wrap params one-per-line, each indented one level
                var parts = directives.FunctionParams.Select(p =>
                    p.DefaultValue != null
                        ? $"{p.Type} {p.Name} = {p.DefaultValue}"
                        : $"{p.Type} {p.Name}"
                );
                _sb.Append($"component {componentName}(\n");
                _indent++;
                var paramArray = parts.ToArray();
                for (int pi = 0; pi < paramArray.Length; pi++)
                {
                    bool isLast = pi == paramArray.Length - 1;
                    Ln(isLast ? paramArray[pi] : paramArray[pi] + ",");
                }
                _indent--;
                Ln(") {");
            }
            else
            {
                Ln(headerLine);
            }
            _indent++;

            var fullSetupCode = directives.FunctionSetupCode?.Trim();
            string? beforeReturnCode = fullSetupCode;
            string? afterReturnCode = null;

            // When setup code is a concatenation of code-before-return +
            // code-after-return (gap left by the removed return statement),
            // split it so the return stays in its original position and
            // formatting is idempotent.
            if (
                fullSetupCode != null
                && directives.FunctionSetupGapOffset >= 0
                && directives.FunctionSetupGapOffset < fullSetupCode.Length
            )
            {
                beforeReturnCode = fullSetupCode
                    .Substring(0, directives.FunctionSetupGapOffset)
                    .TrimEnd();
                afterReturnCode = fullSetupCode
                    .Substring(directives.FunctionSetupGapOffset)
                    .TrimStart();
                if (string.IsNullOrWhiteSpace(beforeReturnCode))
                    beforeReturnCode = null;
                if (string.IsNullOrWhiteSpace(afterReturnCode))
                    afterReturnCode = null;
            }

            if (!string.IsNullOrWhiteSpace(beforeReturnCode))
            {
                string tabExp = new string(' ', _opts.IndentSize);
                string codeToFormat = NormalizeBareJsx(beforeReturnCode!, out var insertedPositions);

                // Check for JSX paren blocks OR bare JSX in the before-return portion only.
                bool hasJsxInSetup = false;
                int gapSrcOffset =
                    directives.FunctionSetupStartOffset >= 0
                        ? directives.FunctionSetupStartOffset
                            + directives.FunctionSetupGapOffset
                        : int.MaxValue;
                if (!directives.SetupCodeMarkupRanges.IsDefaultOrEmpty)
                {
                    foreach (var (s, _, _) in directives.SetupCodeMarkupRanges)
                    {
                        if (s < gapSrcOffset)
                        {
                            hasJsxInSetup = true;
                            break;
                        }
                    }
                }
                if (!hasJsxInSetup && !directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
                {
                    foreach (var (s, _, _) in directives.SetupCodeBareJsxRanges)
                    {
                        if (s < gapSrcOffset)
                        {
                            hasJsxInSetup = true;
                            break;
                        }
                    }
                }

                if (hasJsxInSetup)
                {
                    EmitSetupCodeWithJsx(codeToFormat, directives, tabExp, insertedPositions);
                }
                else
                {
                    EmitSetupCodeNormalized(codeToFormat, tabExp);
                    _sb.Append('\n');
                }
            }

            Ln("return (");
            _indent++;
            FormatNodeList(nodes, topLevel: false);
            _indent--;
            Ln(");");

            // Emit unreachable code after return (if any) with basic
            // indentation normalization — this preserves the structure
            // of the file without rearranging returns.
            if (!string.IsNullOrWhiteSpace(afterReturnCode))
            {
                string tabExp = new string(' ', _opts.IndentSize);
                _sb.Append('\n');
                EmitSetupCodeNormalized(afterReturnCode!, tabExp);
                _sb.Append('\n');
            }

            _indent--;
            Ln("}");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HOOK / MODULE FILES
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Formats a .uitkx file that contains <c>hook</c> and/or <c>module</c>
        /// declarations (no component markup). Re-emits the preamble directives,
        /// then each declaration header + body with normalized indentation.
        /// </summary>
        private void FormatHookModuleFile(string source, DirectiveSet directives)
        {
            // ── Preamble ──────────────────────────────────────────────────────
            bool hasPreamble = false;

            if (directives.HasExplicitNamespace && !string.IsNullOrWhiteSpace(directives.Namespace))
            {
                Ln($"@namespace {directives.Namespace}");
                hasPreamble = true;
            }

            foreach (var u in directives.Usings)
            {
                Ln($"@using {u}");
                hasPreamble = true;
            }

            if (hasPreamble)
                _sb.Append('\n');

            string tabExp = new string(' ', _opts.IndentSize);

            // ── Hooks ─────────────────────────────────────────────────────────
            if (!directives.HookDeclarations.IsDefaultOrEmpty)
            {
                for (int i = 0; i < directives.HookDeclarations.Length; i++)
                {
                    if (i > 0) _sb.Append('\n');
                    var hook = directives.HookDeclarations[i];
                    EmitHookHeader(hook);
                    _indent++;
                    EmitSetupCodeNormalized(hook.Body.Trim(), tabExp);
                    _indent--;
                    Ln("}");
                }
            }

            // ── Modules ──────────────────────────────────────────────────────
            if (!directives.ModuleDeclarations.IsDefaultOrEmpty)
            {
                for (int i = 0; i < directives.ModuleDeclarations.Length; i++)
                {
                    // Blank line between hooks and first module, or between modules
                    if (i > 0 || !directives.HookDeclarations.IsDefaultOrEmpty)
                        _sb.Append('\n');
                    var mod = directives.ModuleDeclarations[i];
                    Ln($"module {mod.Name} {{");
                    _indent++;
                    EmitSetupCodeNormalized(mod.Body.Trim(), tabExp);
                    _indent--;
                    Ln("}");
                }
            }
        }

        /// <summary>
        /// Emits the hook declaration header line:
        /// <c>hook useName&lt;T&gt;(type param) -> ReturnType {</c>
        /// When the single-line form exceeds <see cref="FormatterOptions.PrintWidth"/>,
        /// wraps parameters and return-type tuple members one-per-line (same
        /// convention as the component header wrapping).
        /// </summary>
        private void EmitHookHeader(HookDeclaration hook)
        {
            // ── Build single-line form to measure ─────────────────────────────
            string singleLine = BuildHookHeaderSingleLine(hook);

            if (singleLine.Length <= _opts.PrintWidth)
            {
                Ln(singleLine);
                return;
            }

            // ── Wrapped form ──────────────────────────────────────────────────
            // hook name<T>(
            //   type param,
            //   type param
            // ) -> (
            //   type member,
            //   type member
            // ) {

            var prefix = new StringBuilder();
            prefix.Append("hook ");
            prefix.Append(hook.Name);
            if (!string.IsNullOrEmpty(hook.GenericParams))
                prefix.Append(hook.GenericParams);

            bool hasParams = !hook.Params.IsDefaultOrEmpty;
            bool hasReturnType = !string.IsNullOrEmpty(hook.ReturnType);
            bool returnIsTuple = hasReturnType && hook.ReturnType!.TrimStart().StartsWith("(");

            if (hasParams)
            {
                _sb.Append(IndentStr()).Append(prefix).Append("(\n");
                _indent++;
                for (int i = 0; i < hook.Params.Length; i++)
                {
                    var p = hook.Params[i];
                    string paramText = p.DefaultValue != null
                        ? $"{p.Type} {p.Name} = {p.DefaultValue}"
                        : $"{p.Type} {p.Name}";
                    bool isLast = i == hook.Params.Length - 1;
                    Ln(isLast ? paramText : paramText + ",");
                }
                _indent--;

                if (hasReturnType && returnIsTuple)
                {
                    // `) -> (`  on its own line, then tuple members
                    Ln(") -> (");
                    EmitWrappedTupleMembers(hook.ReturnType!);
                    Ln(") {");
                }
                else if (hasReturnType)
                {
                    Ln($") -> {hook.ReturnType} {{");
                }
                else
                {
                    Ln(") {");
                }
            }
            else
            {
                // No params but return type is long
                if (hasReturnType && returnIsTuple)
                {
                    _sb.Append(IndentStr()).Append(prefix).Append("() -> (\n");
                    EmitWrappedTupleMembers(hook.ReturnType!);
                    Ln(") {");
                }
                else
                {
                    // Fallback: single-line (shouldn't exceed width without params, but safe)
                    Ln(singleLine);
                }
            }
        }

        /// <summary>
        /// Builds the single-line hook header string for measuring purposes.
        /// </summary>
        private static string BuildHookHeaderSingleLine(HookDeclaration hook)
        {
            var sb = new StringBuilder();
            sb.Append("hook ");
            sb.Append(hook.Name);
            if (!string.IsNullOrEmpty(hook.GenericParams))
                sb.Append(hook.GenericParams);
            sb.Append('(');
            if (!hook.Params.IsDefaultOrEmpty)
            {
                for (int p = 0; p < hook.Params.Length; p++)
                {
                    if (p > 0) sb.Append(", ");
                    var param = hook.Params[p];
                    sb.Append(param.Type).Append(' ').Append(param.Name);
                    if (param.DefaultValue != null)
                        sb.Append(" = ").Append(param.DefaultValue);
                }
            }
            sb.Append(')');
            if (!string.IsNullOrEmpty(hook.ReturnType))
                sb.Append(" -> ").Append(hook.ReturnType);
            sb.Append(" {");
            return sb.ToString();
        }

        /// <summary>
        /// Emits the inner members of a tuple return type, one per line, indented.
        /// Input: <c>(bool foo, string bar, int baz)</c> — outer parens are stripped.
        /// </summary>
        private void EmitWrappedTupleMembers(string tupleType)
        {
            // Strip outer parens
            string inner = tupleType.Trim();
            if (inner.StartsWith("(")) inner = inner.Substring(1);
            if (inner.EndsWith(")")) inner = inner.Substring(0, inner.Length - 1);
            inner = inner.Trim();

            // Split on commas, respecting nested generics/tuples
            var members = SplitRespectingNesting(inner);

            _indent++;
            for (int i = 0; i < members.Count; i++)
            {
                bool isLast = i == members.Count - 1;
                string member = members[i].Trim();
                Ln(isLast ? member : member + ",");
            }
            _indent--;
        }

        /// <summary>
        /// Splits a string on top-level commas, respecting <c>&lt;&gt;</c>,
        /// <c>()</c>, and <c>[]</c> nesting.
        /// </summary>
        private static List<string> SplitRespectingNesting(string text)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<' || c == '(' || c == '[') depth++;
                else if (c == '>' || c == ')' || c == ']') depth--;
                else if (c == ',' && depth == 0)
                {
                    result.Add(text.Substring(start, i - start));
                    start = i + 1;
                }
            }
            if (start < text.Length)
                result.Add(text.Substring(start));
            return result;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  NODE LIST
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatNodeList(ImmutableArray<AstNode> nodes, bool topLevel)
        {
            bool firstVisible = true;

            foreach (var node in nodes)
            {
                // Drop whitespace-only text nodes — they are layout artefacts.
                if (node is TextNode tn && string.IsNullOrWhiteSpace(tn.Content))
                    continue;

                // Blank line(s) between top-level siblings, capped by MaxConsecutiveBlankLines.
                if (!firstVisible && topLevel)
                {
                    // EndLine is not yet tracked (Phase 3), so we always have at most
                    // 1 blank line to insert; honour MaxConsecutiveBlankLines as a cap.
                    int blanks = _opts.PreserveBlankLines
                        ? System.Math.Min(1, _opts.MaxConsecutiveBlankLines)
                        : 0;
                    for (int b = 0; b < blanks; b++)
                        _sb.Append('\n');
                }

                FormatNode(node);
                firstVisible = false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  NODE DISPATCH
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatNode(AstNode node)
        {
            switch (node)
            {
                case ElementNode el:
                    FormatElement(el);
                    break;
                case IfNode ifn:
                    FormatIf(ifn);
                    break;
                case ForeachNode fe:
                    FormatForeach(fe);
                    break;
                case ForNode fn:
                    FormatFor(fn);
                    break;
                case WhileNode wh:
                    FormatWhile(wh);
                    break;
                case SwitchNode sw:
                    FormatSwitch(sw);
                    break;

                case TextNode tn:
                {
                    var t = tn.Content.Trim();
                    if (t.Length > 0)
                        Ln(t);
                    break;
                }

                case ExpressionNode en:
                    Ln($"@({en.Expression})");
                    break;

                case CommentNode jc:
                {
                    string trimmed = jc.Content.Trim();
                    if (jc.IsBlock || trimmed.Contains('\n'))
                    {
                        Ln($"/* {trimmed} */");
                    }
                    else
                    {
                        Ln($"// {trimmed}");
                    }
                    break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ELEMENT
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatElement(ElementNode el)
        {
            bool selfClose = el.Children.IsEmpty && el.CloseTagLine == 0;
            string selfCloseSeq = _opts.InsertSpaceBeforeSelfClose ? " />" : "/>";
            var attrStrings = BuildAttrStrings(el.Attributes);

            if (attrStrings.Count == 0)
            {
                // ── No attributes ─────────────────────────────────────────────
                Ln(selfClose ? $"<{el.TagName}{selfCloseSeq}" : $"<{el.TagName}>");
            }
            else
            {
                // ── Decide wrapping ───────────────────────────────────────────
                var singleLine = selfClose
                    ? $"<{el.TagName} {string.Join(" ", attrStrings)}{selfCloseSeq}"
                    : $"<{el.TagName} {string.Join(" ", attrStrings)}>";

                bool wrap =
                    _opts.SingleAttributePerLine
                    || IndentStr().Length + singleLine.Length > _opts.PrintWidth;

                if (!wrap)
                {
                    Ln(singleLine);
                }
                else
                {
                    EmitWrappedOpenTag(el.TagName, attrStrings, selfClose, selfCloseSeq);
                }
            }

            // ── Children + closing tag ─────────────────────────────────────────
            if (!selfClose)
            {
                if (el.Children.IsEmpty)
                {
                    // Empty element with explicit close tag: keep on same line
                    _sb.Remove(_sb.Length - 1, 1); // strip trailing \n
                    _sb.Append($"</{el.TagName}>\n");
                }
                else
                {
                    _indent++;
                    FormatNodeList(el.Children, topLevel: false);
                    _indent--;
                    Ln($"</{el.TagName}>");
                }
            }
        }

        /// <summary>
        /// Emit the opening tag with each attribute on its own line.
        /// Manages <see cref="_indent"/> internally.
        /// </summary>
        private void EmitWrappedOpenTag(
            string tagName,
            List<string> attrStrings,
            bool selfClose,
            string selfCloseSeq
        )
        {
            // Tag name line: e.g. "    <Box"
            Ln($"<{tagName}");
            _indent++;

            for (int i = 0; i < attrStrings.Count; i++)
            {
                bool isLast = i == attrStrings.Count - 1;
                string attr = attrStrings[i];

                if (!isLast)
                {
                    Ln(attr);
                    continue;
                }

                // ── Last attribute ────────────────────────────────────────────

                if (selfClose)
                {
                    if (_opts.ClosingBracketSameLine)
                    {
                        // <Foo
                        //     lastAttr />
                        Ln($"{attr}{selfCloseSeq}");
                        _indent--;
                    }
                    else
                    {
                        // <Foo
                        //     lastAttr
                        // />
                        Ln(attr);
                        _indent--;
                        Ln(selfCloseSeq.TrimStart());
                    }
                }
                else // opening tag
                {
                    if (_opts.BracketSameLine)
                    {
                        // <Foo
                        //     lastAttr>
                        Ln($"{attr}>");
                        _indent--;
                    }
                    else
                    {
                        // <Foo
                        //     lastAttr
                        // >
                        Ln(attr);
                        _indent--;
                        Ln(">");
                    }
                }
            }
        }

        /// <summary>
        /// Render each <see cref="AttributeNode"/> to its canonical string form.
        /// </summary>
        private static List<string> BuildAttrStrings(ImmutableArray<AttributeNode> attrs)
        {
            var list = new List<string>(attrs.Length);
            foreach (var a in attrs)
            {
                switch (a.Value)
                {
                    case StringLiteralValue s:
                        list.Add($"{a.Name}=\"{s.Value}\"");
                        break;
                    case CSharpExpressionValue e:
                        list.Add($"{a.Name}={{{e.Expression}}}");
                        break;
                    case JsxExpressionValue jsx when jsx.Element != null:
                        list.Add($"{a.Name}={{{SerializeJsxInline(jsx.Element)}}}");
                        break;
                    case BooleanShorthandValue:
                    default:
                        list.Add(a.Name);
                        break;
                }
            }
            return list;
        }

        /// <summary>
        /// Serializes a JSX <see cref="ElementNode"/> into a single-line string
        /// representation suitable for embedding inside a <c>{...}</c> attribute value.
        /// </summary>
        private static string SerializeJsxInline(ElementNode el)
        {
            var sb = new StringBuilder();
            SerializeJsxInlineCore(el, sb);
            return sb.ToString();
        }

        private static void SerializeJsxInlineCore(ElementNode el, StringBuilder sb)
        {
            var attrStrings = BuildAttrStrings(el.Attributes);
            string attrPart = attrStrings.Count > 0 ? " " + string.Join(" ", attrStrings) : "";

            if (el.Children.IsEmpty)
            {
                sb.Append($"<{el.TagName}{attrPart} />");
            }
            else
            {
                sb.Append($"<{el.TagName}{attrPart}>");
                foreach (var child in el.Children)
                {
                    if (child is ElementNode childEl)
                        SerializeJsxInlineCore(childEl, sb);
                    else if (child is TextNode tn && !string.IsNullOrWhiteSpace(tn.Content))
                        sb.Append(tn.Content.Trim());
                }
                sb.Append($"</{el.TagName}>");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  C# SETUP CODE EMISSION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Emit lines of C# code with relative-indent re-anchoring.
        /// <para><paramref name="firstLineStripped"/> – set when line[0] had its
        /// leading whitespace stripped by ExpressionExtractor (it always gets
        /// relSpaces = 0 regardless of its actual content).</para>
        /// <para><paramref name="suppressLastNewline"/> – when true the last
        /// non-blank line is appended without a trailing '\n' so a JSX element
        /// can continue on the same line.</para>
        /// </summary>
        private void EmitCSharpLines(
            string code,
            string tabExp,
            bool firstLineStripped,
            bool suppressLastNewline
        )
        {
            if (string.IsNullOrEmpty(code))
                return;
            var lines = code.Split('\n');

            // ── Pre-process: split `{content` lines ───────────────────────────
            // When a line starts with `{` followed by content (not just `{`
            // alone) and the line has net-positive open braces, the `{` opens
            // a multi-line block while content sits on the same line.  Split
            // into two lines so the formatter can indent the content inside
            // the block correctly.  Skip `{/*` (JSX/C# comment expressions).
            {
                var split = new System.Collections.Generic.List<string>(lines.Length);
                for (int si = 0; si < lines.Length; si++)
                {
                    string raw = lines[si];
                    string t = raw.TrimStart();
                    if (t.Length > 1 && t[0] == '{')
                    {
                        int after = 1;
                        while (after < t.Length && (t[after] == ' ' || t[after] == '\t'))
                            after++;

                        bool hasContent = after < t.Length && t[after] != '}';
                        bool isBlockComment =
                            after + 1 < t.Length && t[after] == '/' && t[after + 1] == '*';

                        if (hasContent && !isBlockComment)
                        {
                            int opens = 0,
                                closes = 0;
                            for (int c = 0; c < t.Length; c++)
                            {
                                if (t[c] == '{')
                                    opens++;
                                else if (t[c] == '}')
                                    closes++;
                            }
                            if (opens > closes)
                            {
                                int leadWs = raw.Length - raw.TrimStart().Length;
                                string wsPrefix = leadWs > 0 ? raw.Substring(0, leadWs) : "";
                                split.Add(wsPrefix + "{");
                                split.Add(wsPrefix + t.Substring(1).TrimStart());
                                continue;
                            }
                        }
                    }
                    split.Add(raw);
                }
                lines = split.ToArray();
            }

            // Find last non-blank line index (for suppressLastNewline).
            int lastMeaningful = lines.Length - 1;
            while (lastMeaningful >= 0 && string.IsNullOrWhiteSpace(lines[lastMeaningful]))
                lastMeaningful--;
            if (lastMeaningful < 0)
                return;

            int indentSpaces = _indent * _opts.IndentSize; // spaces contributed by IndentStr()

            // ── baseSpaces for depth-0 lines ──────────────────────────────────
            // Excludes comments and continuation-style lines (ternary arms,
            // method chains) so that CSharpier-corrupted files where comments
            // sit at 2sp but statements at 4sp are correctly normalised.
            int baseSpaces = int.MaxValue;
            {
                int d = 0;
                int startLine = firstLineStripped ? 1 : 0;
                for (int i = startLine; i <= lastMeaningful; i++)
                {
                    string stripped = lines[i].Trim();
                    if (!string.IsNullOrWhiteSpace(stripped))
                    {
                        int lc = 0;
                        while (lc < stripped.Length && stripped[lc] == '}')
                            lc++;
                        d = System.Math.Max(0, d - lc);
                    }

                    if (d == 0 && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string lStripped = stripped;
                        bool isContinuation =
                            lStripped.Length > 0
                            && (lStripped[0] == '?' || lStripped[0] == ':' || lStripped[0] == '.');
                        bool isComment = lStripped.StartsWith("//") || lStripped.StartsWith("/*");
                        if (!isContinuation && !isComment)
                        {
                            var exp = lines[i].Replace("\t", tabExp);
                            int lead = exp.Length - exp.TrimStart().Length;
                            if (lead < baseSpaces)
                                baseSpaces = lead;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string stripped2 = lines[i].Trim().TrimEnd();
                        if (stripped2.Length > 0 && stripped2[stripped2.Length - 1] == '{')
                            d++;
                    }
                }
            }
            if (baseSpaces == int.MaxValue)
                baseSpaces = 0;

            // ── Emit with stack-based block normalisation ─────────────────────
            // The stack holds the TOTAL column (including IndentStr) at which
            // the content of each open block should appear.  This normalises
            // indentation inside { } blocks (Style initialisers, lambda bodies,
            // etc.) while depth-0 lines use the IsStatementStarter / anchor
            // approach to handle multi-line expressions and continuations.
            //
            // caseExtra: inside a { } block, lines following a `case X:` /
            // `default:` label or a `},` brace-comma continuation get an
            // additional indentSize offset.  This is saved/restored on the
            // caseExtraStack whenever a nested block is entered/exited so
            // that the outer context is preserved.
            var blockStack = new System.Collections.Generic.Stack<int>();
            var caseExtraStack = new System.Collections.Generic.Stack<int>();
            var blockAnchorStack = new System.Collections.Generic.Stack<int>();
            var isLambdaStack = new System.Collections.Generic.Stack<bool>();
            int caseExtra = 0;
            int lastBlockAnchor = -1;
            int lastStatementInputIndent = -1;
            bool prevWasStatementStarter = false;

            for (int li = 0; li <= lastMeaningful; li++)
            {
                string raw = lines[li].TrimEnd();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    _sb.Append('\n');
                    continue;
                }

                string stripped = raw.Trim();

                // Convert intra-line tabs to spaces, then collapse runs of 2+
                // whitespace characters to a single space (outside strings/comments).
                if (stripped.IndexOf('\t') >= 0)
                    stripped = stripped.Replace('\t', ' ');

                stripped = CollapseIntraLineSpaces(stripped);

                // Pop for each leading '}'.
                int leadClose = 0;
                while (leadClose < stripped.Length && stripped[leadClose] == '}')
                    leadClose++;
                bool lastPoppedWasLambda = false;
                for (int p = 0; p < leadClose; p++)
                {
                    if (blockStack.Count > 0)
                    {
                        blockStack.Pop();
                        lastPoppedWasLambda = isLambdaStack.Count > 0 && isLambdaStack.Pop();
                        caseExtra = caseExtraStack.Count > 0 ? caseExtraStack.Pop() : 0;
                        lastBlockAnchor = blockAnchorStack.Count > 0 ? blockAnchorStack.Pop() : -1;
                    }
                }

                var expL = raw.Replace("\t", tabExp);
                int leadL = expL.Length - expL.TrimStart().Length;
                int emittedTotal;

                if (blockStack.Count == 0)
                {
                    // Depth-0: statement-opening lines at indentSpaces (rel=0).
                    // Continuation/closure tokens anchor to the most-recent
                    // statement's INPUT indent so relative offsets survive
                    // mixed-corruption files.
                    int rel;
                    if (firstLineStripped && li == 0)
                    {
                        rel = 0;
                    }
                    else if (IsStatementStarter(stripped))
                    {
                        rel = 0;
                        if (li > 0)
                            lastStatementInputIndent = leadL;
                    }
                    else if (stripped == "{" && prevWasStatementStarter)
                    {
                        // Allman-style block opener after a statement: always at rel=0 (same
                        // indent as the statement that precedes it).  Does NOT apply when
                        // the preceding line is a continuation (e.g. () => new List<int>).
                        rel = 0;
                    }
                    else
                    {
                        int anchor =
                            lastStatementInputIndent >= 0 ? lastStatementInputIndent : baseSpaces;
                        rel = System.Math.Max(0, leadL - anchor);
                    }
                    string relPrefix = rel > 0 ? new string(' ', rel) : string.Empty;

                    if (li == lastMeaningful && suppressLastNewline)
                        _sb.Append(IndentStr() + relPrefix + stripped);
                    else
                        Ln(relPrefix + stripped);
                    emittedTotal = indentSpaces + rel;

                    prevWasStatementStarter =
                        IsStatementStarter(stripped) || (firstLineStripped && li == 0);
                }
                else
                {
                    // Inside a { } block: normalise to the block's expected
                    // indentation so inner content is always consistent.
                    // caseExtra adds one indentSize for lines inside a case
                    // body or after a `},` brace-comma continuation.
                    // Continuation lines (starting with ?, :, .) preserve
                    // their relative offset from the last non-continuation line.
                    int blockTarget = blockStack.Peek();

                    // Determine the extra indent for THIS line.
                    int extra = caseExtra;
                    bool isCaseLabel =
                        stripped.StartsWith("case ", System.StringComparison.Ordinal)
                        || stripped.StartsWith("default:", System.StringComparison.Ordinal);
                    if (isCaseLabel)
                        extra = 0; // label itself at blockTarget
                    if (stripped[0] == ')')
                        extra = 0; // closing paren of outer call

                    // Check for continuation lines (ternary arms, method chains).
                    bool isBlockContinuation =
                        stripped[0] == '?' || stripped[0] == ':' || stripped[0] == '.';
                    // Exclude case/default labels that start with the keyword, not ':'
                    if (isCaseLabel)
                        isBlockContinuation = false;

                    int target;
                    if (isBlockContinuation && lastBlockAnchor >= 0)
                    {
                        int rel = System.Math.Max(0, leadL - lastBlockAnchor);
                        target = blockTarget + extra + rel;
                    }
                    else
                    {
                        target = blockTarget + extra;
                        lastBlockAnchor = leadL;
                    }

                    int prefixSpaces = System.Math.Max(0, target - indentSpaces);
                    string blockPrefix =
                        prefixSpaces > 0 ? new string(' ', prefixSpaces) : string.Empty;

                    if (li == lastMeaningful && suppressLastNewline)
                        _sb.Append(IndentStr() + blockPrefix + stripped);
                    else
                        Ln(blockPrefix + stripped);
                    emittedTotal = target;

                    // Update caseExtra for the NEXT line.
                    if (isCaseLabel)
                    {
                        caseExtra = _opts.IndentSize;
                    }
                    else if (leadClose > 0)
                    {
                        // After a line starting with '}': check for brace-comma
                        // continuation `},` which means call arguments follow,
                        // but only if the closed block was a lambda (opened
                        // with `=> {`).  Object-initialiser `},` should NOT
                        // trigger deeper indent for the next property.
                        int ci = leadClose;
                        while (ci < stripped.Length && stripped[ci] == ' ')
                            ci++;
                        if (ci < stripped.Length && stripped[ci] == ',' && lastPoppedWasLambda)
                            caseExtra = _opts.IndentSize;
                        else
                            caseExtra = 0;
                    }
                    else if (stripped[0] == ')')
                    {
                        caseExtra = 0;
                    }
                    // else: keep caseExtra as-is (continuation lines stay at
                    // the same extra level).
                }

                // Push for net unmatched '{' in the line (after leading '}' chars).
                // Previously only a trailing '{' triggered a push, but mid-line
                // '{' whose matching '}' lands on a later line (e.g. after a
                // JSX placeholder) also needs to be tracked.
                // Similarly, pop for net unmatched '}' (e.g. `*/}` closing a
                // `{/*` comment block) so depth returns to the correct level.
                int midOpens = 0,
                    midCloses = 0;
                for (int ci = leadClose; ci < stripped.Length; ci++)
                {
                    if (stripped[ci] == '{')
                        midOpens++;
                    else if (stripped[ci] == '}')
                        midCloses++;
                }
                int netOpens = midOpens - midCloses;
                if (netOpens > 0)
                {
                    string tail = stripped.TrimEnd();
                    bool trailingBrace = tail.Length > 0 && tail[tail.Length - 1] == '{';
                    bool lineHasArrow = stripped.Contains("=>");
                    for (int p = 0; p < netOpens; p++)
                    {
                        blockStack.Push(emittedTotal + (p + 1) * _opts.IndentSize);
                        caseExtraStack.Push(caseExtra);
                        blockAnchorStack.Push(lastBlockAnchor);
                        // Only the last push can be a lambda — and only when the
                        // line actually ends with '{' (the traditional pattern).
                        bool lambda = (p == netOpens - 1) && trailingBrace && lineHasArrow;
                        isLambdaStack.Push(lambda);
                    }
                    caseExtra = 0;
                    lastBlockAnchor = -1;
                }
                else if (netOpens < 0)
                {
                    // Mid-line net closes (e.g. `*/}` ending a `{/*` block).
                    // Pop after the line is emitted — affects subsequent lines.
                    for (int p = 0; p < -netOpens; p++)
                    {
                        if (blockStack.Count > 0)
                        {
                            blockStack.Pop();
                            if (isLambdaStack.Count > 0)
                                isLambdaStack.Pop();
                            caseExtra = caseExtraStack.Count > 0 ? caseExtraStack.Pop() : 0;
                            lastBlockAnchor =
                                blockAnchorStack.Count > 0 ? blockAnchorStack.Pop() : -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Emits function-style setup code (no embedded JSX) with two rules:
        /// <list type="bullet">
        ///   <item>Lines INSIDE a <c>{ }</c> block use the indentation of the
        ///         line that opened the block plus one <paramref name="tabExp"/>
        ///         unit, normalising any inconsistent per-line indentation (e.g.
        ///         a <c>new Style { }</c> whose entries have mixed leading-space
        ///         counts).</item>
        ///   <item>Lines at depth 0 (not inside any <c>{ }</c> block) preserve
        ///         their relative indentation anchored to <c>baseSpaces</c>, so
        ///         ternary continuations, method chains, and similar multi-line
        ///         expressions are not disturbed.</item>
        /// </list>
        /// Uses a stack to track the expected indentation for each block level,
        /// seeded from the actual emitted position of the line that opened the
        /// block.  This makes the normalisation relative (not absolute), so a
        /// <c>{</c> opened inside a deeply-indented call-argument list produces
        /// correctly-relative children.
        /// </summary>
        private void EmitSetupCodeNormalized(string code, string tabExp)
        {
            if (string.IsNullOrEmpty(code))
                return;

            var lines = code.Split('\n');

            // Find last non-blank line index.
            int lastMeaningful = lines.Length - 1;
            while (lastMeaningful >= 0 && string.IsNullOrWhiteSpace(lines[lastMeaningful]))
                lastMeaningful--;
            if (lastMeaningful < 0)
                return;

            int indentSpaces = _indent * _opts.IndentSize; // spaces contributed by IndentStr()

            // ── baseSpaces for depth-0 lines (excluding line[0]) ──────────────
            // Used to anchor continuation-line relative indentation.
            int baseSpaces = int.MaxValue;
            {
                // We need a quick depth scan to find which lines are depth-0 for
                // the baseSpaces calculation.
                int d = 0;
                for (int i = 1; i <= lastMeaningful; i++)
                {
                    string stripped = lines[i].Trim();
                    if (!string.IsNullOrWhiteSpace(stripped))
                    {
                        int lc = 0;
                        while (lc < stripped.Length && stripped[lc] == '}')
                            lc++;
                        d = System.Math.Max(0, d - lc);
                    }

                    // Measure depth-0 line for baseSpaces before potential push.
                    // Skip continuation-style lines (ternary arms, method chains)
                    // so they don't collapse the base indent calculation.
                    if (d == 0 && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string lStripped = stripped;
                        bool isContinuation =
                            lStripped.Length > 0
                            && (lStripped[0] == '?' || lStripped[0] == ':' || lStripped[0] == '.');
                        // Comments are not statements — they must not pull baseSpaces
                        // down and prevent over-indented statement lines from being
                        // corrected (e.g. when CSharpier has added 4-space indent to
                        // setup-code lines that sit next to 2-space comment headers).
                        bool isComment = lStripped.StartsWith("//") || lStripped.StartsWith("/*");
                        if (!isContinuation && !isComment)
                        {
                            var exp = lines[i].Replace("\t", tabExp);
                            int lead = exp.Length - exp.TrimStart().Length;
                            if (lead < baseSpaces)
                                baseSpaces = lead;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string stripped2 = lines[i].Trim().TrimEnd();
                        if (stripped2.Length > 0 && stripped2[stripped2.Length - 1] == '{')
                            d++;
                    }
                }
            }
            // If only continuation lines appeared at depth-0, fall back to
            // indentSpaces so ternary arms / method chains preserve their
            // relative indent rather than collapsing to the same level.
            if (baseSpaces == int.MaxValue)
                baseSpaces = indentSpaces;

            // ── Emit with stack-based block normalisation ─────────────────────
            // The stack holds the TOTAL column (including IndentStr) at which
            // the content of each open block should appear.
            var blockStack = new System.Collections.Generic.Stack<int>();
            int lastStatementInputIndent = -1; // input lead of most-recent depth-0 statement opener
            bool prevWasStatementStarter = false;

            for (int li = 0; li <= lastMeaningful; li++)
            {
                string raw = lines[li].TrimEnd();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    _sb.Append('\n');
                    continue;
                }

                string stripped = raw.Trim();

                // Convert intra-line tabs to spaces, then collapse runs of 2+
                // whitespace characters to a single space (outside strings/comments).
                if (stripped.IndexOf('\t') >= 0)
                    stripped = stripped.Replace('\t', ' ');

                stripped = CollapseIntraLineSpaces(stripped);

                // Pop for each leading '}'.
                int leadClose = 0;
                while (leadClose < stripped.Length && stripped[leadClose] == '}')
                    leadClose++;
                for (int p = 0; p < leadClose; p++)
                    if (blockStack.Count > 0)
                        blockStack.Pop();

                // Compute the indentation for this line and emit it.
                int emittedTotal; // total column position of the emitted line content
                if (blockStack.Count == 0)
                {
                    // Depth-0: statement-opening lines are always at indentSpaces (rel=0).
                    // Continuation/closure tokens anchor to the most-recent statement's
                    // INPUT indent so relative offsets survive mixed-corruption files
                    // (e.g. some lines at 2sp, others at 4sp or 12sp due to CSharpier).
                    var expL = raw.Replace("\t", tabExp);
                    int leadL = expL.Length - expL.TrimStart().Length;
                    int rel;
                    if (li == 0 || IsStatementStarter(stripped))
                    {
                        rel = 0;
                        // li==0 is Trim()'d by the caller so its leadL is 0 regardless of
                        // the original source indent — not a reliable anchor for subsequent
                        // non-starter lines.  Only record the input indent for li > 0.
                        if (li > 0)
                            lastStatementInputIndent = leadL;
                    }
                    else if (stripped == "{" && prevWasStatementStarter)
                    {
                        // Allman-style block opener after a statement: always at rel=0.
                        rel = 0;
                    }
                    else
                    {
                        int anchor =
                            lastStatementInputIndent >= 0 ? lastStatementInputIndent : baseSpaces;
                        rel = System.Math.Max(0, leadL - anchor);
                    }
                    string relPrefix = rel > 0 ? new string(' ', rel) : string.Empty;
                    Ln(relPrefix + stripped);
                    emittedTotal = indentSpaces + rel;

                    prevWasStatementStarter = IsStatementStarter(stripped) || li == 0;
                }
                else
                {
                    // Inside a block: normalise to the block's expected indentation.
                    int blockTarget = blockStack.Peek();
                    int prefixSpaces = System.Math.Max(0, blockTarget - indentSpaces);
                    string blockPrefix =
                        prefixSpaces > 0 ? new string(' ', prefixSpaces) : string.Empty;
                    Ln(blockPrefix + stripped);
                    emittedTotal = blockTarget;
                }

                // Push for a trailing '{' — next lines should be one tabExp deeper.
                string tail = stripped.TrimEnd();
                if (tail.Length > 0 && tail[tail.Length - 1] == '{')
                    blockStack.Push(emittedTotal + _opts.IndentSize);
            }
        }

        /// <summary>
        /// Returns <c>true</c> when a stripped depth-0 line begins a new C# statement
        /// rather than being a continuation fragment (continuation arg, named arg, bare
        /// block opener, ternary arm, method chain, etc.).
        /// A line is a statement starter when it:
        ///   • begins with a well-known C# keyword (var, void, if, foreach, return, …) or
        ///   • ends with <c>;</c> (expression-statement terminator) or
        ///   • ends with <c>{</c> AND has content before the brace (not just a bare <c>{</c>)
        /// Used in <see cref="EmitSetupCodeNormalized"/> to anchor depth-0 statement lines
        /// at <c>indentSpaces</c> regardless of their input indentation.
        /// </summary>
        private static bool IsStatementStarter(string s)
        {
            if (s.Length == 0)
                return false;

            // Ternary arms ('?', ':') and method-chain continuations ('.') are
            // never statement openers regardless of their ending character.
            char first = s[0];
            if (first == '?' || first == ':' || first == '.')
                return false;

            // ── keyword prefix → always a statement opener ───────────────────
            foreach (var kw in s_statementKeywords)
                if (s.StartsWith(kw, System.StringComparison.Ordinal))
                    return true;

            // ── trailing character heuristics ─────────────────────────────────
            string t = s.TrimEnd();
            if (t.Length == 0)
                return false;
            char last = t[t.Length - 1];

            // Lines ending with ';' are complete expression statements.
            if (last == ';')
                return true;

            // Lines ending with '{' that have content before the brace open a block
            // as part of a new statement (method, if-body, lambda inline, etc.).
            // A bare '{' alone (Allman-style continuation) is deliberately excluded.
            if (last == '{' && t.Length > 1)
                return true;

            // Lines containing a standalone ' = ' are assignments/declarations
            // (e.g. "MyType name = value =>").  The ' = ' pattern naturally
            // excludes compound operators (+=, -=, ==, !=, >=, <=, ??=, etc.)
            // because they don't produce space-equals-space.
            if (t.IndexOf(" = ", System.StringComparison.Ordinal) >= 0)
                return true;

            // Expression-bodied method declarations: e.g.
            //   RowRenderer BuildRowRenderer() =>
            // These end with '=>' and start with a type name (uppercase letter).
            // Lambda continuations like `(index, obj) =>` are excluded because
            // they start with '(' not a letter.
            if (
                t.Length >= 2
                && t[t.Length - 1] == '>'
                && t[t.Length - 2] == '='
                && char.IsUpper(first)
            )
                return true;

            return false;
        }

        private static readonly string[] s_statementKeywords =
        {
            "var ",
            "void ",
            "if (",
            "if(",
            "else ",
            "else{",
            "else if",
            "foreach ",
            "foreach(",
            "for ",
            "for(",
            "while ",
            "while(",
            "do ",
            "do{",
            "switch ",
            "switch(",
            "return ",
            "throw ",
            "break;",
            "break ",
            "continue;",
            "continue ",
            "bool ",
            "int ",
            "uint ",
            "long ",
            "ulong ",
            "float ",
            "double ",
            "decimal ",
            "char ",
            "string ",
            "string?",
            "object ",
            "object?",
            "byte ",
            "sbyte ",
            "short ",
            "ushort ",
            "using ",
            "using(",
            "try ",
            "try{",
            "catch ",
            "catch(",
            "finally ",
            "finally{",
            "static ",
            "readonly ",
            "const ",
            "public ",
            "private ",
            "protected ",
            "internal ",
            "abstract ",
            "override ",
            "virtual ",
            "sealed ",
            "partial ",
            "async ",
            "await ",
        };

        /// <summary>
        /// Normalises intra-line whitespace outside of string literals and
        /// <c>// …</c> line comments:
        /// <list type="bullet">
        ///   <item>Runs of 2+ consecutive spaces → single space.</item>
        ///   <item>Spaces immediately after <c>(</c> are removed.</item>
        ///   <item>Spaces immediately before <c>)</c> are removed.</item>
        /// </list>
        /// Tab characters should be replaced with spaces before calling
        /// this method.
        /// </summary>
        private static string CollapseIntraLineSpaces(string line)
        {
            if (
                line.IndexOf("  ", System.StringComparison.Ordinal) < 0
                && line.IndexOf("( ", System.StringComparison.Ordinal) < 0
                && line.IndexOf(" )", System.StringComparison.Ordinal) < 0
            )
                return line;

            var sb = new System.Text.StringBuilder(line.Length);
            bool inStr = false;
            bool verbatim = false;
            bool pendingSp = false;
            bool afterOpen = false; // just emitted '(' outside strings

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inStr)
                {
                    sb.Append(c);
                    if (verbatim)
                    {
                        if (c == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                                sb.Append(line[++i]); // doubled-quote escape
                            else
                                inStr = false;
                        }
                    }
                    else
                    {
                        if (c == '\\' && i + 1 < line.Length)
                            sb.Append(line[++i]); // backslash escape
                        else if (c == '"')
                            inStr = false;
                    }
                    pendingSp = false;
                    afterOpen = false;
                    continue;
                }

                // Line comment — flush pending space, then rest is literal.
                if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
                {
                    if (pendingSp)
                    {
                        sb.Append(' ');
                        pendingSp = false;
                    }
                    sb.Append(line, i, line.Length - i);
                    break;
                }

                // String opener.
                if (c == '"')
                {
                    if (pendingSp)
                    {
                        sb.Append(' ');
                        pendingSp = false;
                    }
                    sb.Append(c);
                    inStr = true;
                    verbatim =
                        (i > 0 && line[i - 1] == '@')
                        || (i > 1 && line[i - 1] == '$' && line[i - 2] == '@')
                        || (i > 1 && line[i - 1] == '@' && line[i - 2] == '$');
                    afterOpen = false;
                    continue;
                }

                if (c == ' ')
                {
                    // Skip spaces right after '('.
                    if (!afterOpen)
                        pendingSp = true;
                    continue;
                }

                // Non-space character.
                if (c == ')')
                {
                    // Absorb pending spaces before ')'.
                    pendingSp = false;
                }
                else if (pendingSp)
                {
                    sb.Append(' ');
                    pendingSp = false;
                }

                sb.Append(c);
                afterOpen = (c == '(');
            }

            return sb.ToString();
        }

        private static string BuildRepeat(string s, int count)
        {
            if (count <= 0 || string.IsNullOrEmpty(s))
                return string.Empty;
            var sb = new System.Text.StringBuilder(s.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(s);
            return sb.ToString();
        }

        /// <summary>
        /// Append a JSX element tag inline onto the current <see cref="_sb"/> line
        /// (no leading indent prefix, no trailing newline).
        /// </summary>
        private void AppendElementInline(ElementNode el, bool selfClose)
        {
            var attrStrings = BuildAttrStrings(el.Attributes);
            string closing = selfClose ? (_opts.InsertSpaceBeforeSelfClose ? " />" : "/>") : ">";
            if (attrStrings.Count == 0)
                _sb.Append($"<{el.TagName}{closing}");
            else
                _sb.Append($"<{el.TagName} {string.Join(" ", attrStrings)}{closing}");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  CONTROL FLOW
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatIf(IfNode node)
        {
            for (int i = 0; i < node.Branches.Length; i++)
            {
                var branch = node.Branches[i];

                if (i == 0)
                {
                    // First branch: @if (cond) {
                    Ln($"@if ({branch.Condition}) {{");
                }
                else
                {
                    // Remove the '\n' that was appended after the previous closing '}'
                    // so we can emit '}⎵@else...' on the same line.
                    if (_sb.Length > 0 && _sb[_sb.Length - 1] == '\n')
                        _sb.Length--;

                    if (branch.Condition != null)
                        _sb.Append($" @else if ({branch.Condition}) {{\n");
                    else
                        _sb.Append(" @else {\n");
                }

                _indent++;
                if (branch.SetupCode != null)
                    EmitSetupCodeLines(branch.SetupCode);
                Ln("return (");
                _indent++;
                FormatNodeList(branch.Body, topLevel: false);
                _indent--;
                Ln(");");
                _indent--;
                Ln("}");
            }
        }

        private void FormatForeach(ForeachNode node)
        {
            Ln($"@foreach ({node.IteratorDeclaration} in {node.CollectionExpression}) {{");
            _indent++;
            if (node.SetupCode != null)
                EmitSetupCodeLines(node.SetupCode);
            Ln("return (");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
            _indent--;
            Ln(");");
            _indent--;
            Ln("}");
        }

        private void FormatFor(ForNode node)
        {
            Ln($"@for ({node.ForExpression}) {{");
            _indent++;
            if (node.SetupCode != null)
                EmitSetupCodeLines(node.SetupCode);
            Ln("return (");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
            _indent--;
            Ln(");");
            _indent--;
            Ln("}");
        }

        private void FormatWhile(WhileNode node)
        {
            Ln($"@while ({node.Condition}) {{");
            _indent++;
            if (node.SetupCode != null)
                EmitSetupCodeLines(node.SetupCode);
            Ln("return (");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
            _indent--;
            Ln(");");
            _indent--;
            Ln("}");
        }

        private void FormatSwitch(SwitchNode node)
        {
            Ln($"@switch ({node.SwitchExpression}) {{");
            _indent++;

            foreach (var sc in node.Cases)
            {
                // @case / @default label
                if (sc.ValueExpression != null)
                    Ln($"@case {sc.ValueExpression}:");
                else
                    Ln("@default:");

                // Body is indented under the case label
                _indent++;
                if (sc.SetupCode != null)
                    EmitSetupCodeLines(sc.SetupCode);
                Ln("return (");
                _indent++;
                FormatNodeList(sc.Body, topLevel: false);
                _indent--;
                Ln(");");
                _indent--;
            }

            _indent--;
            Ln("}");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  OUTPUT HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private void EmitSetupCodeLines(string setupCode)
        {
            foreach (var line in setupCode.Split('\n'))
            {
                var trimmed = line.TrimEnd('\r');
                if (trimmed.Length > 0)
                    Ln(trimmed);
            }
        }

        /// <summary>
        /// Append an indented line (or block of lines) terminating with a single LF.
        /// When <paramref name="text"/> contains embedded newlines (e.g. a multi-line
        /// attribute expression value), each continuation line is re-anchored to the
        /// current indent level plus its relative indentation from the first line.
        /// </summary>
        private void Ln(string text)
        {
            if (!text.Contains('\n'))
            {
                if (_indent > 0)
                    _sb.Append(IndentStr());
                _sb.Append(text);
                _sb.Append('\n');
                return;
            }

            // Multi-line: re-anchor every continuation line to current indent + relative.
            string tabExp = new string(' ', _opts.IndentSize);
            // Normalise CR/LF and strip any trailing blank lines before splitting.
            var lines = text.TrimEnd('\r', '\n')
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            // Compute the minimum leading-space count of all non-blank continuation lines.
            int baseSpaces = int.MaxValue;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;
                var exp = lines[i].Replace("\t", tabExp);
                int lead = exp.Length - exp.TrimStart().Length;
                if (lead < baseSpaces)
                    baseSpaces = lead;
            }
            if (baseSpaces == int.MaxValue)
                baseSpaces = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (i == 0)
                {
                    // First line — emit with the current indent prefix as-is.
                    if (_indent > 0)
                        _sb.Append(IndentStr());
                    _sb.Append(line.TrimEnd());
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // Blank line — emit as truly empty (just the newline below).
                }
                else
                {
                    // Continuation line — strip the common base indentation and
                    // re-prefix with current indent + remaining relative indent.
                    var expLine = line.Replace("\t", tabExp);
                    int lead = expLine.Length - expLine.TrimStart().Length;
                    int rel = System.Math.Max(0, lead - baseSpaces);
                    if (_indent > 0)
                        _sb.Append(IndentStr());
                    if (rel > 0)
                        _sb.Append(new string(' ', rel));
                    _sb.Append(expLine.TrimStart().TrimEnd());
                }
                _sb.Append('\n');
            }
        }

        private string IndentStr()
        {
            if (_indent <= 0)
                return string.Empty;
            return _opts.UseTabIndent
                ? new string('\t', _indent)
                : new string(' ', _indent * _opts.IndentSize);
        }

        // ── JSX-in-setup formatting ───────────────────────────────────────────

        /// <summary>
        /// Emits function-style setup code that contains embedded JSX paren-blocks
        /// (e.g. <c>var x = (&lt;Box/&gt;);</c>).
        ///
        /// C# segments between JSX blocks are re-indented via
        /// <see cref="EmitCSharpLines"/>.  Each JSX block is parsed and formatted
        /// through <see cref="FormatNodeList"/> so it gets the same canonical
        /// element / attribute layout as markup inside <c>return (…)</c>.
        /// </summary>
        private void EmitSetupCodeWithJsx(
            string setupCode, DirectiveSet directives, string tabExp,
            HashSet<int> insertedPositions)
        {
            // setupCode is already normalised by NormalizeBareJsx; insertedPositions
            // tracks which '(' were inserted so we can distinguish synthetic vs original.

            var blocks = ScanJsxParenBlocks(setupCode);
            if (blocks.Count == 0)
            {
                EmitCSharpLines(
                    setupCode,
                    tabExp,
                    firstLineStripped: true,
                    suppressLastNewline: false
                );
                _sb.Append('\n');
                return;
            }

            // ── Build a csharpWithPlaceholders string ─────────────────────────
            // Replace each multi-line JSX paren-block with a single-line
            // placeholder that preserves the brace context.  EmitCSharpLines
            // formats the entire C# as one pass (correct brace tracking), then
            // we splice the formatted JSX back in place of the placeholders.
            //
            // Single-line paren blocks are left as-is (they don't break brace
            // context and EmitCSharpLines handles them fine).
            const string PLACEHOLDER_PREFIX = "___UITKX_JSX_PLACEHOLDER_";
            var multiLineBlocks = new List<(int OrigMarkupIndex, int Start, int End, bool IsSynthetic)>();

            var csBuilder = new System.Text.StringBuilder(setupCode.Length);
            int pos = 0;
            int origMarkupIdx = 0; // index into directives.SetupCodeMarkupRanges (original blocks only)

            foreach (var (jS, jE) in blocks)
            {
                // A block is synthetic if its '(' was inserted by NormalizeBareJsx.
                // insertedPositions contains output-string positions of inserted '('.
                bool isSynthetic = insertedPositions.Contains(jS);

                // Check if this block spans multiple lines, or is a single-line
                // container element (has `</` closing tag) that should be expanded.
                bool isMultiLine = false;
                bool hasContainerClose = false;
                for (int k = jS; k < jE; k++)
                {
                    if (setupCode[k] == '\n')
                    {
                        isMultiLine = true;
                        break;
                    }
                    if (setupCode[k] == '<' && k + 1 < jE && setupCode[k + 1] == '/')
                        hasContainerClose = true;
                }

                if (!isMultiLine && !hasContainerClose)
                {
                    if (!isSynthetic) origMarkupIdx++;
                    continue; // simple single-line self-closing blocks stay in the C# text
                }

                // Append C# before this block.
                csBuilder.Append(setupCode, pos, jS - pos);

                // Emit a placeholder line: keep the opening `(` and replace
                // the content+closing with a marker that EmitCSharpLines will
                // output as a normal statement.
                csBuilder.Append($"{PLACEHOLDER_PREFIX}{multiLineBlocks.Count}___");

                multiLineBlocks.Add((origMarkupIdx, jS, jE, isSynthetic));

                // Skip past the `)` and its trailing `;` or `,`.
                int afterPos = jE;
                while (
                    afterPos < setupCode.Length
                    && (setupCode[afterPos] == ' ' || setupCode[afterPos] == '\t')
                )
                    afterPos++;

                if (
                    afterPos < setupCode.Length
                    && (setupCode[afterPos] == ';' || setupCode[afterPos] == ',')
                )
                {
                    csBuilder.Append(setupCode[afterPos]); // keep the ; or ,
                    pos = afterPos + 1;
                }
                else
                {
                    pos = jE;
                }

                if (!isSynthetic) origMarkupIdx++;
            }

            // Append remaining C# after the last block.
            if (pos < setupCode.Length)
                csBuilder.Append(setupCode, pos, setupCode.Length - pos);

            // ── Format the entire C# (with placeholders) in one pass ──────────
            int savedSbLen = _sb.Length;
            EmitCSharpLines(
                csBuilder.ToString(),
                tabExp,
                firstLineStripped: true,
                suppressLastNewline: false
            );
            _sb.Append('\n');

            // If no multi-line blocks, we're done.
            if (multiLineBlocks.Count == 0)
                return;

            // ── Extract the formatted C# and splice JSX in ───────────────────
            string formattedCs = _sb.ToString(savedSbLen, _sb.Length - savedSbLen);
            _sb.Length = savedSbLen; // rewind

            var lines = formattedCs.Split('\n');
            int baseIndent = _indent; // component-level indent (typically 1)

            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li];

                // Check if this line contains a placeholder.
                int phIdx = line.IndexOf(PLACEHOLDER_PREFIX, System.StringComparison.Ordinal);
                if (phIdx < 0)
                {
                    _sb.Append(line);
                    if (li < lines.Length - 1)
                        _sb.Append('\n');
                    continue;
                }

                // Extract placeholder index.
                int markerStart = phIdx + PLACEHOLDER_PREFIX.Length;
                int markerEnd = line.IndexOf("___", markerStart, System.StringComparison.Ordinal);
                if (markerEnd < 0)
                {
                    _sb.Append(line);
                    if (li < lines.Length - 1)
                        _sb.Append('\n');
                    continue;
                }

                int placeholderIdx;
                if (
                    !int.TryParse(
                        line.Substring(markerStart, markerEnd - markerStart),
                        out placeholderIdx
                    )
                    || placeholderIdx < 0
                    || placeholderIdx >= multiLineBlocks.Count
                )
                {
                    _sb.Append(line);
                    if (li < lines.Length - 1)
                        _sb.Append('\n');
                    continue;
                }

                var (origBlockIdx, origStart, origEnd, blockIsSynthetic) = multiLineBlocks[placeholderIdx];

                // Everything before the placeholder is the C# prefix (e.g. "    var oneTest = ").
                string prefix = line.Substring(0, phIdx);
                // Trailing punctuation (e.g. ";") after the placeholder marker.
                string suffix = line.Substring(markerEnd + 3).TrimEnd();

                // Measure the indent of this line (in spaces).
                int lineIndentSpaces = 0;
                foreach (char ch in prefix)
                {
                    if (ch == ' ')
                        lineIndentSpaces++;
                    else if (ch == '\t')
                        lineIndentSpaces += _opts.IndentSize;
                    else
                        break;
                }

                // Set _indent to match the C# nesting.
                int jsxIndent = lineIndentSpaces / _opts.IndentSize;

                // Emit the line prefix (e.g. "    var oneTest = ") and opening `(`.
                _sb.Append(prefix);
                _sb.Append("(\n");

                // ── Format JSX content ────────────────────────────────────────
                _indent = jsxIndent + 1;
                bool jsxEmitted = false;

                if (
                    !blockIsSynthetic
                    && origBlockIdx < directives.SetupCodeMarkupRanges.Length
                    && !string.IsNullOrEmpty(_source)
                )
                {
                    var (rangeStart, rangeEnd, rangeLine) = directives.SetupCodeMarkupRanges[
                        origBlockIdx
                    ];
                    try
                    {
                        var jsxDiags = new List<ParseDiagnostic>();
                        var jsxDirectives = directives with
                        {
                            MarkupStartIndex = rangeStart,
                            MarkupEndIndex = rangeEnd,
                            MarkupStartLine = rangeLine,
                        };
                        var jsxNodes = UitkxParser.Parse(
                            _source,
                            _filePath,
                            jsxDirectives,
                            jsxDiags
                        );
                        bool hasErrors = false;
                        foreach (var d in jsxDiags)
                            if (d.Severity == ParseSeverity.Error)
                            {
                                hasErrors = true;
                                break;
                            }

                        if (!hasErrors && jsxNodes.Length > 0)
                        {
                            FormatNodeList(jsxNodes, topLevel: false);
                            jsxEmitted = true;
                        }
                    }
                    catch
                    { /* best-effort */
                    }
                }

                // For synthetic blocks (created by NormalizeBareJsx), try
                // parsing the JSX content directly from the normalised text.
                if (!jsxEmitted && blockIsSynthetic)
                {
                    int contentStart = origStart + 1; // skip '('
                    int contentEnd = origEnd - 1;     // skip ')'
                    int contentLen = contentEnd - contentStart;
                    if (contentLen > 0)
                    {
                        string jsxText = setupCode.Substring(contentStart, contentLen).Trim();
                        try
                        {
                            var synthDiags = new List<ParseDiagnostic>();
                            var synthDirectives = directives with
                            {
                                MarkupStartIndex = 0,
                                MarkupEndIndex = jsxText.Length,
                                MarkupStartLine = 1,
                            };
                            var synthNodes = UitkxParser.Parse(
                                jsxText, _filePath, synthDirectives, synthDiags);
                            bool hasErrors = false;
                            foreach (var d in synthDiags)
                                if (d.Severity == ParseSeverity.Error)
                                { hasErrors = true; break; }

                            if (!hasErrors && synthNodes.Length > 0)
                            {
                                FormatNodeList(synthNodes, topLevel: false);
                                jsxEmitted = true;
                            }
                        }
                        catch { /* best-effort */ }
                    }
                }

                if (!jsxEmitted)
                {
                    // Fallback: raw re-indent.
                    int contentStart = origStart + 1;
                    int contentEnd = origEnd - 1;
                    int contentLen = contentEnd - contentStart;
                    if (contentLen < 0)
                        contentLen = 0;
                    string jsxContent = setupCode.Substring(contentStart, contentLen);
                    EmitCSharpLines(
                        jsxContent,
                        tabExp,
                        firstLineStripped: false,
                        suppressLastNewline: false
                    );
                }

                // ── Closing ')' ───────────────────────────────────────────────
                _indent = jsxIndent;
                Ln(")" + suffix);
                _indent = baseIndent;

                // Don't append \n here — the `Ln` already did.
                // But the for loop expects to append \n, so skip it for this line.
                // Actually, Ln already appended \n. We just need to not double it.
                // The loop would append \n if li < lines.Length - 1, but Ln already did.
                // So we skip the \n append for this line.
                continue;
            }

            _indent = baseIndent;
        }

        /// <summary>
        /// Scans <paramref name="code"/> for JSX paren-blocks: a <c>'('</c> whose
        /// first non-whitespace content is <c>'&lt;'</c>.  Returns a list of
        /// <c>(BlockStart, BlockEnd)</c> tuples where
        /// <c>code[BlockStart]='('</c> and <c>code[BlockEnd-1]=')'</c>.
        /// Bare forms (<c>=&gt; &lt;Tag</c> and <c>= &lt;Tag</c>) should be
        /// pre-normalised via <see cref="NormalizeBareArrows"/> before calling
        /// this method.
        /// </summary>
        private static List<(int Start, int End)> ScanJsxParenBlocks(string code)
        {
            var result = new List<(int, int)>();
            int i = 0;
            while (i < code.Length)
            {
                // Skip // line comments
                if (code[i] == '/' && i + 1 < code.Length && code[i + 1] == '/')
                {
                    while (i < code.Length && code[i] != '\n')
                        i++;
                    continue;
                }

                // Skip string and char literals
                if (DirectiveParser.TrySkipStringOrCharLiteral(code, code.Length, ref i))
                    continue;

                if (code[i] != '(')
                {
                    i++;
                    continue;
                }

                int peek = i + 1;
                while (
                    peek < code.Length
                    && (
                        code[peek] == ' '
                        || code[peek] == '\t'
                        || code[peek] == '\r'
                        || code[peek] == '\n'
                    )
                )
                    peek++;

                if (
                    peek >= code.Length
                    || code[peek] != '<'
                    || peek + 1 >= code.Length
                    || !char.IsLetter(code[peek + 1])
                )
                {
                    i++;
                    continue;
                }

                int depth = 1,
                    j = i + 1;
                while (j < code.Length && depth > 0)
                {
                    if (DirectiveParser.TrySkipStringOrCharLiteral(code, code.Length, ref j))
                        continue;
                    if (code[j] == '(')
                        depth++;
                    else if (code[j] == ')')
                        depth--;
                    j++;
                }

                if (depth == 0)
                {
                    result.Add((i, j)); // i = '(' position, j = right after ')'
                    i = j;
                }
                else
                {
                    i++;
                }
            }
            return result;
        }

        /// <summary>
        /// Pre-processes <paramref name="code"/> to normalise bare JSX after
        /// <c>=&gt;</c> (arrow) or <c>=</c> (assignment) into paren-wrapped form.
        /// <list type="bullet">
        ///   <item><c>=&gt; &lt;Tag .../&gt;</c>  →  <c>=&gt; (&lt;Tag .../&gt;)</c></item>
        ///   <item><c>= &lt;Tag .../&gt;</c>  →  <c>= (&lt;Tag .../&gt;)</c></item>
        /// </list>
        /// Also handles:
        /// <list type="bullet">
        ///   <item><c>return &lt;Tag .../&gt;</c>  →  <c>return (&lt;Tag .../&gt;)</c></item>
        ///   <item><c>? &lt;Tag .../&gt;</c> (ternary)  →  <c>? (&lt;Tag .../&gt;)</c></item>
        ///   <item><c>: &lt;Tag .../&gt;</c> (ternary)  →  <c>: (&lt;Tag .../&gt;)</c></item>
        /// </list>
        /// Already paren-wrapped expressions are left unchanged.  The result
        /// can then be scanned by <see cref="ScanJsxParenBlocks"/>.
        /// <para>
        /// <paramref name="insertedOpenParenPositions"/> receives the original
        /// source positions where <c>(</c> was inserted.  This lets callers
        /// distinguish blocks that already existed in the original source from
        /// those that were synthesised by normalisation.
        /// </para>
        /// </summary>
        private static string NormalizeBareJsx(
            string code,
            out HashSet<int> insertedOpenParenPositions)
        {
            var insertions = new List<(int Position, char Char)>();
            int i = 0;
            while (i < code.Length)
            {
                // Skip // line comments
                if (code[i] == '/' && i + 1 < code.Length && code[i + 1] == '/')
                {
                    while (i < code.Length && code[i] != '\n')
                        i++;
                    continue;
                }

                // Skip string and char literals
                if (i < code.Length && (code[i] == '"' || code[i] == '\'' ||
                    (code[i] == '@' && i + 1 < code.Length && code[i + 1] == '"') ||
                    (code[i] == '$' && i + 1 < code.Length && code[i + 1] == '"') ||
                    (code[i] == '$' && i + 1 < code.Length && code[i + 1] == '@')))
                {
                    int saved = i;
                    if (DirectiveParser.TrySkipStringOrCharLiteral(code, code.Length, ref i))
                        continue;
                    i = saved;
                }

                // ── return <Tag ─────────────────────────────────────────
                if (code[i] == 'r' && i + 5 < code.Length
                    && code.Substring(i, 6) == "return"
                    && (i == 0 || !(char.IsLetterOrDigit(code[i - 1]) || code[i - 1] == '_'))
                    && (i + 6 >= code.Length || !(char.IsLetterOrDigit(code[i + 6]) || code[i + 6] == '_')))
                {
                    int peek = i + 6;
                    while (peek < code.Length &&
                           (code[peek] == ' ' || code[peek] == '\t' ||
                            code[peek] == '\r' || code[peek] == '\n'))
                        peek++;

                    if (peek < code.Length && code[peek] == '<'
                        && peek + 1 < code.Length && char.IsLetter(code[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(code, peek, code.Length);
                        if (jsxEnd > peek)
                        {
                            // Insert '(' then '\n' so the normalised text reads
                            // "return (\n<Tag..." matching the canonical form.
                            // Without the '\n', the first format pass produces
                            // "return (<Tag..." requiring a second pass to
                            // break the line — an idempotency bug.
                            insertions.Add((peek, '('));
                            insertions.Add((peek, '\n'));
                            insertions.Add((jsxEnd, ')'));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ── ? <Tag  (ternary true, but NOT ?. or ??) ────────────
                if (code[i] == '?' && i + 1 < code.Length
                    && code[i + 1] != '.' && code[i + 1] != '?')
                {
                    int peek = i + 1;
                    while (peek < code.Length &&
                           (code[peek] == ' ' || code[peek] == '\t' ||
                            code[peek] == '\r' || code[peek] == '\n'))
                        peek++;

                    if (peek < code.Length && code[peek] == '<'
                        && peek + 1 < code.Length && char.IsLetter(code[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(code, peek, code.Length);
                        if (jsxEnd > peek)
                        {
                            insertions.Add((peek, '('));
                            insertions.Add((jsxEnd, ')'));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ── : <Tag  (ternary false, but NOT ::) ─────────────────
                if (code[i] == ':' && i + 1 < code.Length && code[i + 1] != ':')
                {
                    int peek = i + 1;
                    while (peek < code.Length &&
                           (code[peek] == ' ' || code[peek] == '\t' ||
                            code[peek] == '\r' || code[peek] == '\n'))
                        peek++;

                    if (peek < code.Length && code[peek] == '<'
                        && peek + 1 < code.Length && char.IsLetter(code[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(code, peek, code.Length);
                        if (jsxEnd > peek)
                        {
                            insertions.Add((peek, '('));
                            insertions.Add((jsxEnd, ')'));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                if (code[i] == '=')
                {
                    int peek;
                    bool isArrow = i + 1 < code.Length && code[i + 1] == '>';

                    if (isArrow)
                    {
                        // => <Tag
                        peek = i + 2;
                    }
                    else
                    {
                        // Bare = : skip ==, !=, <=, >=
                        if (i + 1 < code.Length && code[i + 1] == '=')
                        {
                            i++;
                            continue;
                        }
                        if (
                            i > 0
                            && (code[i - 1] == '!' || code[i - 1] == '<' || code[i - 1] == '>')
                        )
                        {
                            i++;
                            continue;
                        }
                        peek = i + 1;
                    }

                    while (
                        peek < code.Length
                        && (
                            code[peek] == ' '
                            || code[peek] == '\t'
                            || code[peek] == '\r'
                            || code[peek] == '\n'
                        )
                    )
                        peek++;

                    if (
                        peek < code.Length
                        && code[peek] == '<'
                        && peek + 1 < code.Length
                        && char.IsLetter(code[peek + 1])
                    )
                    {
                        int jsxEnd = FindJsxElementEnd(code, peek, code.Length);
                        if (jsxEnd > peek)
                        {
                            insertions.Add((peek, '('));
                            insertions.Add((jsxEnd, ')'));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }
                i++;
            }

            if (insertions.Count == 0)
            {
                insertedOpenParenPositions = new HashSet<int>();
                return code;
            }

            // Build the output and compute normalised-string positions of
            // inserted '(' characters so callers can identify synthetic blocks.
            insertedOpenParenPositions = new HashSet<int>();
            var sb = new System.Text.StringBuilder(code.Length + insertions.Count);
            int pos = 0;
            for (int idx = 0; idx < insertions.Count; idx++)
            {
                var (insPos, ch) = insertions[idx];
                sb.Append(code, pos, insPos - pos);
                if (ch == '(')
                    insertedOpenParenPositions.Add(sb.Length);
                sb.Append(ch);
                pos = insPos;
            }
            sb.Append(code, pos, code.Length - pos);
            return sb.ToString();
        }

        /// <summary>Backward-compat overload that discards insertion tracking.</summary>
        private static string NormalizeBareArrows(string code)
        {
            return NormalizeBareJsx(code, out _);
        }

        /// <summary>
        /// Finds the end position (exclusive) of a JSX element starting at
        /// <paramref name="start"/> (which must point to <c>&lt;</c>).
        /// Handles self-closing (<c>/&gt;</c>) and container elements with
        /// nested children.  Skips over string literals and <c>{expr}</c> blocks.
        /// </summary>
        private static int FindJsxElementEnd(string text, int start, int limit)
            => ReturnFinder.FindJsxElementEnd(text, start, limit);
    }
}
