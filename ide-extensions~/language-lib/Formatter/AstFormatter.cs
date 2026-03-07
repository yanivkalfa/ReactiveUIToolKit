using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent;
        private static readonly string[] NoSemicolonLeadingKeywords =
        {
            "if",
            "for",
            "foreach",
            "while",
            "switch",
            "else",
            "do",
            "try",
            "catch",
            "finally",
            "using",
            "lock",
            "fixed",
            "namespace",
            "class",
            "struct",
            "interface",
            "record",
            "enum",
        };

        public AstFormatter(FormatterOptions opts)
        {
            _opts = opts;
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

            var diags = new List<ParseDiagnostic>();
            var directives = DirectiveParser.Parse(source, filePath, diags);
            var nodes = UitkxParser.Parse(source, filePath, directives, diags);

            // Return source unchanged when there are parse errors.
            foreach (var d in diags)
                if (d.Severity == ParseSeverity.Error)
                    return source;

            if (directives.IsFunctionStyle)
            {
                FormatFunctionStyleComponent(directives, nodes);
            }
            else
            {
                FormatDirectives(directives);
                FormatNodeList(nodes, topLevel: true);
            }

            // Trim any trailing whitespace/newlines and add exactly one trailing \n.
            var result = _sb.ToString().TrimEnd('\r', '\n', ' ', '\t');
            return result + "\n";
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DIRECTIVES
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatDirectives(DirectiveSet d)
        {
            bool any = false;

            if (d.Namespace != null)
            {
                Ln($"@namespace {d.Namespace}");
                any = true;
            }
            foreach (var u in d.Usings)
            {
                Ln($"@using {u}");
                any = true;
            }
            if (d.ComponentName != null)
            {
                Ln($"@component {d.ComponentName}");
                any = true;
            }
            if (d.PropsTypeName != null)
            {
                Ln($"@props {d.PropsTypeName}");
                any = true;
            }
            if (d.DefaultKey != null)
            {
                Ln($"@key \"{d.DefaultKey}\"");
                any = true;
            }
            foreach (var inj in d.Injects)
            {
                Ln($"@inject {inj.Type} {inj.Name}");
                any = true;
            }

            // Blank separator between the directive block and the markup.
            if (any)
                _sb.Append('\n');
        }

        private void FormatFunctionStyleComponent(DirectiveSet directives, ImmutableArray<AstNode> nodes)
        {
            var componentName = string.IsNullOrWhiteSpace(directives.ComponentName)
                ? "Component"
                : directives.ComponentName;

            Ln($"component {componentName} {{");
            _indent++;

            var setupCode = directives.FunctionSetupCode?.Trim();
            if (!string.IsNullOrWhiteSpace(setupCode))
            {
                var normalizedSetupCode = setupCode!;
                string tabExp = new string(' ', _opts.IndentSize);
                EmitCSharpLines(
                    normalizedSetupCode,
                    tabExp,
                    firstLineStripped: false,
                    suppressLastNewline: false
                );
                _sb.Append('\n');
            }

            Ln("return (");
            _indent++;
            FormatNodeList(nodes, topLevel: false);
            _indent--;
            Ln(");");

            _indent--;
            Ln("}");
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
                case BreakNode:
                    Ln("@break;");
                    break;
                case ContinueNode:
                    Ln("@continue;");
                    break;
                case CodeBlockNode cb:
                    FormatCodeBlock(cb);
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

                case JsxCommentNode jc:
                    Ln($"{{/* {jc.Content.Trim()} */}}");
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ELEMENT
        // ═══════════════════════════════════════════════════════════════════════

        private void FormatElement(ElementNode el)
        {
            bool selfClose = el.Children.IsEmpty;
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
                _indent++;
                FormatNodeList(el.Children, topLevel: false);
                _indent--;
                Ln($"</{el.TagName}>");
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
                    case BooleanShorthandValue:
                    default:
                        list.Add(a.Name);
                        break;
                }
            }
            return list;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  @code BLOCK
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

            // baseSpaces: minimum leading spaces across all non-blank lines that
            // are not the ExpressionExtractor-stripped line[0].
            int baseSpaces = int.MaxValue;
            for (int li = firstLineStripped ? 1 : 0; li < lines.Length; li++)
            {
                if (string.IsNullOrWhiteSpace(lines[li]))
                    continue;
                var exp = lines[li].Replace("\t", tabExp);
                int lead = exp.Length - exp.TrimStart().Length;
                if (lead < baseSpaces)
                    baseSpaces = lead;
            }
            if (baseSpaces == int.MaxValue)
                baseSpaces = 0;

            // Find last non-blank line index (for suppressLastNewline).
            int lastMeaningful = lines.Length - 1;
            while (lastMeaningful >= 0 && string.IsNullOrWhiteSpace(lines[lastMeaningful]))
                lastMeaningful--;

            for (int li = 0; li < lines.Length; li++)
            {
                if (li > lastMeaningful)
                    break; // skip trailing blank lines

                var stripped = lines[li].TrimEnd();
                if (string.IsNullOrWhiteSpace(stripped))
                {
                    _sb.Append('\n');
                    continue;
                }

                var expL = stripped.Replace("\t", tabExp);
                int leadL = expL.Length - expL.TrimStart().Length;
                int rel =
                    (firstLineStripped && li == 0) ? 0 : System.Math.Max(0, leadL - baseSpaces);

                string relPrefix = rel > 0 ? new string(' ', rel) : string.Empty;
                string content = stripped.TrimStart();

                if (_opts.InsertMissingSemicolonsOnFormat)
                {
                    var nextMeaningful = FindNextMeaningfulLine(lines, li + 1, lastMeaningful);
                    content = TryInsertMissingSemicolon(content, nextMeaningful);
                }

                if (li == lastMeaningful && suppressLastNewline)
                    _sb.Append(IndentStr() + relPrefix + content);
                else
                    Ln(relPrefix + content);
            }
        }

        private static string? FindNextMeaningfulLine(string[] lines, int start, int lastMeaningful)
        {
            for (int i = start; i <= lastMeaningful; i++)
            {
                var candidate = lines[i].Trim();
                if (candidate.Length > 0)
                    return candidate;
            }

            return null;
        }

        private static string TryInsertMissingSemicolon(string content, string? nextMeaningfulLine)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            var trimmed = content.Trim();
            if (!CanEndStatement(trimmed))
                return content;

            if (LooksLikeControlOrDeclarationHeader(trimmed))
                return content;

            if (nextMeaningfulLine is not null)
            {
                if (nextMeaningfulLine.StartsWith("{"))
                    return content;

                if (
                    nextMeaningfulLine.StartsWith(".")
                    || nextMeaningfulLine.StartsWith("?")
                    || nextMeaningfulLine.StartsWith(":")
                    || nextMeaningfulLine.StartsWith("&&")
                    || nextMeaningfulLine.StartsWith("||")
                    || nextMeaningfulLine.StartsWith("+")
                    || nextMeaningfulLine.StartsWith("-")
                    || nextMeaningfulLine.StartsWith("*")
                    || nextMeaningfulLine.StartsWith("/")
                    || nextMeaningfulLine.StartsWith("%")
                )
                    return content;
            }

            return content + ";";
        }

        private static bool CanEndStatement(string trimmed)
        {
            if (
                trimmed.EndsWith(";")
                || trimmed.EndsWith("{")
                || trimmed.EndsWith("}")
                || trimmed.EndsWith(":")
                || trimmed.EndsWith(",")
                || trimmed.EndsWith("=>")
            )
                return false;

            if (
                trimmed.StartsWith("#")
                || trimmed.StartsWith("//")
                || trimmed.StartsWith("/*")
                || trimmed.StartsWith("*")
                || trimmed.StartsWith("[")
            )
                return false;

            if (trimmed.Contains("//") || trimmed.Contains("/*"))
                return false;

            var last = trimmed[trimmed.Length - 1];
            return char.IsLetterOrDigit(last) || last == ')' || last == ']' || last == '"' || last == '\'';
        }

        private static bool LooksLikeControlOrDeclarationHeader(string trimmed)
        {
            foreach (var keyword in NoSemicolonLeadingKeywords)
            {
                if (trimmed == keyword || trimmed.StartsWith(keyword + " ") || trimmed.StartsWith(keyword + "("))
                    return true;
            }

            return false;
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

        private void FormatCodeBlock(CodeBlockNode cb)
        {
            Ln("@code {");
            _indent++;
            string tabExp = new string(' ', _opts.IndentSize);

            if (cb.ReturnMarkups.IsEmpty)
            {
                // No embedded JSX — emit C# lines with relative-indent only.
                EmitCSharpLines(
                    cb.Code,
                    tabExp,
                    firstLineStripped: true,
                    suppressLastNewline: false
                );
            }
            else
            {
                int pos = 0;
                bool firstSegment = true;

                foreach (var rm in cb.ReturnMarkups) // sorted by StartOffsetInCodeBlock
                {
                    int jsxStart = rm.StartOffsetInCodeBlock;

                    // ── C# segment before this JSX element ──────────────────────────
                    if (jsxStart > pos)
                    {
                        string seg = cb.Code.Substring(pos, jsxStart - pos);

                        if (firstSegment)
                        {
                            // The very first segment: ExpressionExtractor stripped
                            // leading whitespace from line[0].
                            EmitCSharpLines(
                                seg,
                                tabExp,
                                firstLineStripped: true,
                                suppressLastNewline: true
                            );
                            firstSegment = false;
                        }
                        else
                        {
                            // Subsequent segment: the first char(s) up to the first \n
                            // are the inline suffix of the previous JSX tag (e.g. ";").
                            // Append them directly (no indent prefix) then process rest.
                            int nlPos = seg.IndexOf('\n');
                            if (nlPos < 0)
                            {
                                // Entirely on one line (e.g. "; ") — append inline.
                                _sb.Append(seg.TrimEnd());
                            }
                            else
                            {
                                _sb.Append(seg.Substring(0, nlPos)); // e.g. ";"
                                _sb.Append('\n');
                                string rest = seg.Substring(nlPos + 1);
                                if (!string.IsNullOrWhiteSpace(rest))
                                    EmitCSharpLines(
                                        rest,
                                        tabExp,
                                        firstLineStripped: false,
                                        suppressLastNewline: true
                                    );
                            }
                        }
                    }
                    else
                    {
                        firstSegment = false;
                    }

                    // ── JSX element ───────────────────────────────────────
                    // Always separate '=' from '<' with a space. No () wrapping —
                    // the CSharpEmitter requires the bare  = <Tag ...>  form.
                    if (rm.Element.Children.IsEmpty)
                    {
                        // Self-closing: space + inline tag, no parens.
                        _sb.Append(' ');
                        AppendElementInline(rm.Element, selfClose: true);
                    }
                    else
                    {
                        // Multi-line: wrap in ().
                        // Result: = (\n    <Tag>\n        children\n    </Tag>\n)
                        // The C# tail (e.g. ";") is appended inline after ')' by the
                        // tail-handling code below, giving the idiomatic "    );" line.
                        _sb.Append(" (\n");
                        _indent++;
                        _sb.Append(IndentStr());
                        AppendElementInline(rm.Element, selfClose: false);
                        _sb.Append('\n');
                        _indent++;
                        FormatNodeList(rm.Element.Children, topLevel: false);
                        _indent--;
                        _sb.Append(IndentStr() + $"</{rm.Element.TagName}>\n");
                        _indent--;
                        _sb.Append(IndentStr() + ')');

                        if (_opts.InsertMissingSemicolonsOnFormat)
                        {
                            var following = cb.Code.Substring(rm.EndOffsetInCodeBlock);
                            int nl = following.IndexOf('\n');
                            string sameLineSuffix = nl >= 0 ? following.Substring(0, nl) : following;

                            // If there is inline suffix on the same source line (e.g. ";", ":", ")")
                            // we let the normal segment/tail handling print it verbatim.
                            if (string.IsNullOrWhiteSpace(sameLineSuffix))
                            {
                                string? nextMeaningful = null;
                                if (nl >= 0 && nl + 1 < following.Length)
                                {
                                    var rest = following.Substring(nl + 1);
                                    var restLines = rest.Split('\n');
                                    int lastMeaningful = restLines.Length - 1;
                                    while (
                                        lastMeaningful >= 0
                                        && string.IsNullOrWhiteSpace(restLines[lastMeaningful])
                                    )
                                        lastMeaningful--;

                                    if (lastMeaningful >= 0)
                                        nextMeaningful = FindNextMeaningfulLine(
                                            restLines,
                                            0,
                                            lastMeaningful
                                        );
                                }

                                var fixedParen = TryInsertMissingSemicolon(
                                    ")",
                                    nextMeaningful
                                );
                                if (fixedParen.EndsWith(";"))
                                    _sb.Append(';');
                            }
                        }
                    }

                    pos = rm.EndOffsetInCodeBlock;
                }

                // ── Remaining C# text after all JSX regions ─────────────────────
                if (pos < cb.Code.Length)
                {
                    string tail = cb.Code.Substring(pos);
                    int nlPos = tail.IndexOf('\n');
                    if (nlPos < 0)
                    {
                        // Just a one-line suffix like ";"
                        string oneLineTail = tail.TrimEnd();
                        if (_opts.InsertMissingSemicolonsOnFormat)
                            oneLineTail = TryInsertMissingSemicolon(oneLineTail, nextMeaningfulLine: null);

                        _sb.Append(oneLineTail);
                        _sb.Append('\n');
                    }
                    else
                    {
                        string firstTailLine = tail.Substring(0, nlPos).TrimEnd();
                        string rest = tail.Substring(nlPos + 1);

                        if (_opts.InsertMissingSemicolonsOnFormat)
                        {
                            var restLines = rest.Split('\n');
                            int lastMeaningful = restLines.Length - 1;
                            while (lastMeaningful >= 0 && string.IsNullOrWhiteSpace(restLines[lastMeaningful]))
                                lastMeaningful--;

                            var nextMeaningful = lastMeaningful >= 0
                                ? FindNextMeaningfulLine(restLines, 0, lastMeaningful)
                                : null;

                            firstTailLine = TryInsertMissingSemicolon(firstTailLine, nextMeaningful);
                        }

                        _sb.Append(firstTailLine); // inline suffix, e.g. ";" or ");"
                        _sb.Append('\n');

                        if (!string.IsNullOrWhiteSpace(rest))
                            EmitCSharpLines(
                                rest,
                                tabExp,
                                firstLineStripped: false,
                                suppressLastNewline: false
                            );
                    }
                }
            }

            if (_sb.Length > 0 && _sb[_sb.Length - 1] != '\n')
                _sb.Append('\n');

            _indent--;
            Ln("}");
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
                FormatNodeList(branch.Body, topLevel: false);
                _indent--;
                Ln("}");
            }
        }

        private void FormatForeach(ForeachNode node)
        {
            Ln($"@foreach ({node.IteratorDeclaration} in {node.CollectionExpression}) {{");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
            _indent--;
            Ln("}");
        }

        private void FormatFor(ForNode node)
        {
            Ln($"@for ({node.ForExpression}) {{");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
            _indent--;
            Ln("}");
        }

        private void FormatWhile(WhileNode node)
        {
            Ln($"@while ({node.Condition}) {{");
            _indent++;
            FormatNodeList(node.Body, topLevel: false);
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
                FormatNodeList(sc.Body, topLevel: false);
                _indent--;
            }

            _indent--;
            Ln("}");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  OUTPUT HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>Append an indented line terminating with a single LF.</summary>
        private void Ln(string text)
        {
            if (_indent > 0)
                _sb.Append(IndentStr());
            _sb.Append(text);
            _sb.Append('\n');
        }

        private string IndentStr()
        {
            if (_indent <= 0)
                return string.Empty;
            return _opts.UseTabIndent
                ? new string('\t', _indent)
                : new string(' ', _indent * _opts.IndentSize);
        }
    }
}
