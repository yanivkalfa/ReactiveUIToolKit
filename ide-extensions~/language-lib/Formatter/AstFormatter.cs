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
        private string _source   = "";
        private string _filePath = "";

        public AstFormatter(FormatterOptions opts, ICSharpFormatterDelegate? csharpFormatter = null)
        {
            _opts            = opts;
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
            _source   = source;
            _filePath = filePath;

            var diags = new List<ParseDiagnostic>();
            var directives = DirectiveParser.Parse(source, filePath, diags);
            var nodes = UitkxParser.Parse(source, filePath, directives, diags);

            // Return source unchanged when there are parse errors — except
            // UITKX2103 (multiple top-level returns) which is structural but
            // the file still parses successfully with the first return extracted.
            foreach (var d in diags)
                if (d.Severity == ParseSeverity.Error && d.Code != "UITKX2103")
                    return source;

            // When there are multiple top-level returns (UITKX2103), re-parse
            // using the LAST return so the formatter formats the real render
            // markup instead of an early-exit return.
            if (directives.IsFunctionStyle && diags.Any(d => d.Code == "UITKX2103"))
            {
                var fmtDiags = new List<ParseDiagnostic>();
                directives = DirectiveParser.Parse(source, filePath, fmtDiags, useLastReturn: true);
                nodes = UitkxParser.Parse(source, filePath, directives, fmtDiags);
            }

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

            if (hasPreamble)
                _sb.Append('\n');

            string paramList = "";
            if (!directives.FunctionParams.IsDefaultOrEmpty)
            {
                var parts = directives.FunctionParams.Select(p =>
                    p.DefaultValue != null
                        ? $"{p.Type} {p.Name} = {p.DefaultValue}"
                        : $"{p.Type} {p.Name}");
                paramList = $"({string.Join(", ", parts)})";
            }

            Ln($"component {componentName}{paramList} {{");
            _indent++;

            var fullSetupCode = directives.FunctionSetupCode?.Trim();
            string? beforeReturnCode = fullSetupCode;
            string? afterReturnCode = null;

            // When setup code is a concatenation of code-before-return +
            // code-after-return (gap left by the removed return statement),
            // split it so the return stays in its original position and
            // formatting is idempotent.
            if (fullSetupCode != null
                && directives.FunctionSetupGapOffset >= 0
                && directives.FunctionSetupGapOffset < fullSetupCode.Length)
            {
                beforeReturnCode = fullSetupCode.Substring(0, directives.FunctionSetupGapOffset).TrimEnd();
                afterReturnCode = fullSetupCode.Substring(directives.FunctionSetupGapOffset).TrimStart();
                if (string.IsNullOrWhiteSpace(beforeReturnCode)) beforeReturnCode = null;
                if (string.IsNullOrWhiteSpace(afterReturnCode)) afterReturnCode = null;
            }

            if (!string.IsNullOrWhiteSpace(beforeReturnCode))
            {
                string tabExp = new string(' ', _opts.IndentSize);
                string codeToFormat = beforeReturnCode!;

                // Check for JSX paren blocks in the before-return portion only.
                bool hasJsxInSetup = false;
                if (!directives.SetupCodeMarkupRanges.IsDefaultOrEmpty)
                {
                    int gapSrcOffset = directives.FunctionSetupStartOffset >= 0
                        ? directives.FunctionSetupStartOffset + directives.FunctionSetupGapOffset
                        : int.MaxValue;
                    foreach (var (s, _, _) in directives.SetupCodeMarkupRanges)
                    {
                        if (s < gapSrcOffset) { hasJsxInSetup = true; break; }
                    }
                }

                if (hasJsxInSetup)
                {
                    EmitSetupCodeWithJsx(codeToFormat, directives, tabExp);
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
                        while (lc < stripped.Length && stripped[lc] == '}') lc++;
                        d = System.Math.Max(0, d - lc);
                    }

                    if (d == 0 && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string lStripped = stripped;
                        bool isContinuation = lStripped.Length > 0 &&
                            (lStripped[0] == '?' || lStripped[0] == ':' || lStripped[0] == '.');
                        bool isComment = lStripped.StartsWith("//") || lStripped.StartsWith("/*");
                        if (!isContinuation && !isComment)
                        {
                            var exp = lines[i].Replace("\t", tabExp);
                            int lead = exp.Length - exp.TrimStart().Length;
                            if (lead < baseSpaces) baseSpaces = lead;
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
            if (baseSpaces == int.MaxValue) baseSpaces = 0;

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
                        int anchor = lastStatementInputIndent >= 0 ? lastStatementInputIndent : baseSpaces;
                        rel = System.Math.Max(0, leadL - anchor);
                    }
                    string relPrefix = rel > 0 ? new string(' ', rel) : string.Empty;

                    if (li == lastMeaningful && suppressLastNewline)
                        _sb.Append(IndentStr() + relPrefix + stripped);
                    else
                        Ln(relPrefix + stripped);
                    emittedTotal = indentSpaces + rel;

                    prevWasStatementStarter = IsStatementStarter(stripped) || (firstLineStripped && li == 0);
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
                        stripped.StartsWith("case ", System.StringComparison.Ordinal) ||
                        stripped.StartsWith("default:", System.StringComparison.Ordinal);
                    if (isCaseLabel) extra = 0;          // label itself at blockTarget
                    if (stripped[0] == ')') extra = 0;   // closing paren of outer call

                    // Check for continuation lines (ternary arms, method chains).
                    bool isBlockContinuation = stripped[0] == '?' || stripped[0] == ':' || stripped[0] == '.';
                    // Exclude case/default labels that start with the keyword, not ':'
                    if (isCaseLabel) isBlockContinuation = false;

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
                    string blockPrefix = prefixSpaces > 0 ? new string(' ', prefixSpaces) : string.Empty;

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
                        while (ci < stripped.Length && stripped[ci] == ' ') ci++;
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

                // Push for a trailing '{' — next lines should be one indentSize deeper.
                string tail = stripped.TrimEnd();
                if (tail.Length > 0 && tail[tail.Length - 1] == '{')
                {
                    blockStack.Push(emittedTotal + _opts.IndentSize);
                    caseExtraStack.Push(caseExtra);
                    blockAnchorStack.Push(lastBlockAnchor);
                    isLambdaStack.Push(tail.Contains("=>"));
                    caseExtra = 0;
                    lastBlockAnchor = -1;
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
                        while (lc < stripped.Length && stripped[lc] == '}') lc++;
                        d = System.Math.Max(0, d - lc);
                    }

                    // Measure depth-0 line for baseSpaces before potential push.
                    // Skip continuation-style lines (ternary arms, method chains)
                    // so they don't collapse the base indent calculation.
                    if (d == 0 && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        string lStripped = stripped;
                        bool isContinuation = lStripped.Length > 0 &&
                            (lStripped[0] == '?' || lStripped[0] == ':' || lStripped[0] == '.');
                        // Comments are not statements — they must not pull baseSpaces
                        // down and prevent over-indented statement lines from being
                        // corrected (e.g. when CSharpier has added 4-space indent to
                        // setup-code lines that sit next to 2-space comment headers).
                        bool isComment = lStripped.StartsWith("//") || lStripped.StartsWith("/*");
                        if (!isContinuation && !isComment)
                        {
                            var exp = lines[i].Replace("\t", tabExp);
                            int lead = exp.Length - exp.TrimStart().Length;
                            if (lead < baseSpaces) baseSpaces = lead;
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
            if (baseSpaces == int.MaxValue) baseSpaces = indentSpaces;

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
                    if (blockStack.Count > 0) blockStack.Pop();

                // Compute the indentation for this line and emit it.
                int emittedTotal; // total column position of the emitted line content
                if (blockStack.Count == 0)
                {
                    // Depth-0: statement-opening lines are always at indentSpaces (rel=0).
                    // Continuation/closure tokens anchor to the most-recent statement's
                    // INPUT indent so relative offsets survive mixed-corruption files
                    // (e.g. some lines at 2sp, others at 4sp or 12sp due to CSharpier).
                    var expL  = raw.Replace("\t", tabExp);
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
                        int anchor = lastStatementInputIndent >= 0 ? lastStatementInputIndent : baseSpaces;
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
                    string blockPrefix = prefixSpaces > 0 ? new string(' ', prefixSpaces) : string.Empty;
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
            if (s.Length == 0) return false;

            // Ternary arms ('?', ':') and method-chain continuations ('.') are
            // never statement openers regardless of their ending character.
            char first = s[0];
            if (first == '?' || first == ':' || first == '.') return false;

            // ── keyword prefix → always a statement opener ───────────────────
            foreach (var kw in s_statementKeywords)
                if (s.StartsWith(kw, System.StringComparison.Ordinal)) return true;

            // ── trailing character heuristics ─────────────────────────────────
            string t = s.TrimEnd();
            if (t.Length == 0) return false;
            char last = t[t.Length - 1];

            // Lines ending with ';' are complete expression statements.
            if (last == ';') return true;

            // Lines ending with '{' that have content before the brace open a block
            // as part of a new statement (method, if-body, lambda inline, etc.).
            // A bare '{' alone (Allman-style continuation) is deliberately excluded.
            if (last == '{' && t.Length > 1) return true;

            // Lines containing a standalone ' = ' are assignments/declarations
            // (e.g. "MyType name = value =>").  The ' = ' pattern naturally
            // excludes compound operators (+=, -=, ==, !=, >=, <=, ??=, etc.)
            // because they don't produce space-equals-space.
            if (t.IndexOf(" = ", System.StringComparison.Ordinal) >= 0) return true;

            return false;
        }

        private static readonly string[] s_statementKeywords =
        {
            "var ", "void ",
            "if (", "if(", "else ", "else{", "else if",
            "foreach ", "foreach(", "for ", "for(",
            "while ", "while(", "do ", "do{",
            "switch ", "switch(",
            "return ", "throw ",
            "break;", "break ", "continue;", "continue ",
            "bool ", "int ", "uint ", "long ", "ulong ",
            "float ", "double ", "decimal ", "char ",
            "string ", "string?", "object ", "object?",
            "byte ", "sbyte ", "short ", "ushort ",
            "using ", "using(",
            "try ", "try{", "catch ", "catch(", "finally ", "finally{",
            "static ", "readonly ", "const ",
            "public ", "private ", "protected ", "internal ",
            "abstract ", "override ", "virtual ", "sealed ", "partial ",
            "async ", "await ",
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
            if (line.IndexOf("  ", System.StringComparison.Ordinal) < 0
                && line.IndexOf("( ", System.StringComparison.Ordinal) < 0
                && line.IndexOf(" )", System.StringComparison.Ordinal) < 0)
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
                                sb.Append(line[++i]);   // doubled-quote escape
                            else
                                inStr = false;
                        }
                    }
                    else
                    {
                        if (c == '\\' && i + 1 < line.Length)
                            sb.Append(line[++i]);       // backslash escape
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
                    if (pendingSp) { sb.Append(' '); pendingSp = false; }
                    sb.Append(line, i, line.Length - i);
                    break;
                }

                // String opener.
                if (c == '"')
                {
                    if (pendingSp) { sb.Append(' '); pendingSp = false; }
                    sb.Append(c);
                    inStr = true;
                    verbatim = (i > 0 && line[i - 1] == '@') ||
                               (i > 1 && line[i - 1] == '$' && line[i - 2] == '@') ||
                               (i > 1 && line[i - 1] == '@' && line[i - 2] == '$');
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
            if (count <= 0 || string.IsNullOrEmpty(s)) return string.Empty;
            var sb = new System.Text.StringBuilder(s.Length * count);
            for (int i = 0; i < count; i++) sb.Append(s);
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

        private void FormatCodeBlock(CodeBlockNode cb)
        {
            Ln("@code {");
            _indent++;
            string tabExp = new string(' ', _opts.IndentSize);

            if (cb.ReturnMarkups.IsEmpty)
            {
                // No embedded JSX — the entire body is pure C#.
                // Offer it to the pluggable Roslyn formatter first; fall back to
                // the simple indentation-only formatter on failure or no delegate.
                string codeToFormat = cb.Code ?? string.Empty;
                if (_csharpFormatter != null)
                {
                    try
                    {
                        string? formatted = _csharpFormatter.Format(codeToFormat, _opts.IndentSize);
                        if (!string.IsNullOrEmpty(formatted))
                            codeToFormat = formatted;
                    }
                    catch
                    {
                        // Ignore delegate errors and proceed with built-in formatting.
                    }
                }

                EmitCSharpLines(
                    codeToFormat,
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

                        _sb.Append(oneLineTail);
                        _sb.Append('\n');
                    }
                    else
                    {
                        string firstTailLine = tail.Substring(0, nlPos).TrimEnd();
                        string rest = tail.Substring(nlPos + 1);

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
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var exp = lines[i].Replace("\t", tabExp);
                int lead = exp.Length - exp.TrimStart().Length;
                if (lead < baseSpaces) baseSpaces = lead;
            }
            if (baseSpaces == int.MaxValue) baseSpaces = 0;

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
                    int lead   = expLine.Length - expLine.TrimStart().Length;
                    int rel    = System.Math.Max(0, lead - baseSpaces);
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
            string setupCode,
            DirectiveSet directives,
            string tabExp)
        {
            var blocks = ScanJsxParenBlocks(setupCode);
            if (blocks.Count == 0)
            {
                // Unexpected — fall back to simple re-indent.
                EmitCSharpLines(setupCode, tabExp, firstLineStripped: true, suppressLastNewline: false);
                _sb.Append('\n');
                return;
            }

            int pos      = 0;
            int blockIdx = 0;
            bool firstSeg = true;

            foreach (var (jS, jE) in blocks)
            {
                // ── C# segment before the opening '(' ────────────────────────
                if (jS > pos)
                {
                    string seg = setupCode.Substring(pos, jS - pos);
                    if (!string.IsNullOrWhiteSpace(seg))
                    {
                        EmitCSharpLines(seg, tabExp,
                            firstLineStripped: firstSeg,
                            suppressLastNewline: true);
                    }
                }

                // ── Opening '(' — preserve space separation ───────────────────
                // If the segment ended with '(' (e.g. container.Add() case) emit
                // '(\n' directly; otherwise add a separating space first.
                string priorTrimmed = jS > 0 ? setupCode.Substring(0, jS).TrimEnd() : "";
                bool priorEndsParen = priorTrimmed.Length > 0
                    && priorTrimmed[priorTrimmed.Length - 1] == '(';
                _sb.Append(priorEndsParen ? "(\n" : " (\n");

                // ── JSX block content ─────────────────────────────────────────
                bool jsxEmitted = false;
                if (blockIdx < directives.SetupCodeMarkupRanges.Length
                    && !string.IsNullOrEmpty(_source))
                {
                    var (rangeStart, rangeEnd, rangeLine) = directives.SetupCodeMarkupRanges[blockIdx];
                    try
                    {
                        var jsxDiags      = new List<ParseDiagnostic>();
                        var jsxDirectives = directives with
                        {
                            MarkupStartIndex = rangeStart,
                            MarkupEndIndex   = rangeEnd,
                            MarkupStartLine  = rangeLine,
                        };
                        var jsxNodes = UitkxParser.Parse(_source, _filePath, jsxDirectives, jsxDiags);
                        bool hasErrors = false;
                        foreach (var d in jsxDiags)
                            if (d.Severity == ParseSeverity.Error) { hasErrors = true; break; }

                        if (!hasErrors && jsxNodes.Length > 0)
                        {
                            _indent++;
                            FormatNodeList(jsxNodes, topLevel: false);
                            _indent--;
                            jsxEmitted = true;
                        }
                    }
                    catch { /* best-effort — fall through to raw emit */ }
                }

                if (!jsxEmitted)
                {
                    // Fallback: re-indent the raw JSX text.
                    string jsxContent = setupCode.Substring(jS + 1, jE - jS - 2);
                    _indent++;
                    EmitCSharpLines(jsxContent, tabExp,
                        firstLineStripped: false, suppressLastNewline: false);
                    _indent--;
                }

                // ── Closing ')' with trailing punctuation ─────────────────────
                // Peek past whitespace at what comes immediately after ')'.
                int afterPos = jE;
                while (afterPos < setupCode.Length
                       && (setupCode[afterPos] == ' ' || setupCode[afterPos] == '\t'))
                    afterPos++;

                if (afterPos < setupCode.Length && setupCode[afterPos] == ';')
                {
                    Ln(");");
                    pos = afterPos + 1;
                    // Consume one trailing newline so the next segment's
                    // firstLineStripped logic works correctly.
                    if (pos < setupCode.Length && setupCode[pos] == '\r') pos++;
                    if (pos < setupCode.Length && setupCode[pos] == '\n') pos++;
                }
                else if (afterPos < setupCode.Length && setupCode[afterPos] == ',')
                {
                    Ln("),");
                    pos = afterPos + 1;
                    if (pos < setupCode.Length && setupCode[pos] == '\r') pos++;
                    if (pos < setupCode.Length && setupCode[pos] == '\n') pos++;
                }
                else
                {
                    Ln(")");
                    pos = jE;
                }

                blockIdx++;
                firstSeg = false;
            }

            // ── Remaining C# after the last JSX block ─────────────────────────
            if (pos < setupCode.Length)
            {
                string remaining = setupCode.Substring(pos);
                if (!string.IsNullOrWhiteSpace(remaining))
                    EmitCSharpLines(remaining, tabExp,
                        firstLineStripped: false, suppressLastNewline: false);
            }
            _sb.Append('\n');
        }

        /// <summary>
        /// Scans <paramref name="code"/> for JSX paren-blocks: a <c>'('</c> whose
        /// first non-whitespace content is <c>'&lt;'</c>.  Returns a list of
        /// <c>(ParenStart, ParenEnd)</c> pairs where <c>code[ParenStart]='('</c>
        /// and <c>code[ParenEnd-1]=')'</c>.
        /// </summary>
        private static List<(int ParenStart, int ParenEnd)> ScanJsxParenBlocks(string code)
        {
            var result = new List<(int, int)>();
            int i = 0;
            while (i < code.Length)
            {
                if (code[i] != '(') { i++; continue; }

                int peek = i + 1;
                while (peek < code.Length
                       && (code[peek] == ' '  || code[peek] == '\t'
                        || code[peek] == '\r' || code[peek] == '\n'))
                    peek++;

                if (peek >= code.Length || code[peek] != '<') { i++; continue; }

                int depth = 1, j = i + 1;
                while (j < code.Length && depth > 0)
                {
                    if      (code[j] == '(') depth++;
                    else if (code[j] == ')') depth--;
                    j++;
                }

                if (depth == 0)
                {
                    result.Add((i, j)); // i = '(' position, j = right after ')'
                    i = j;
                }
                else { i++; }
            }
            return result;
        }
    }
}
