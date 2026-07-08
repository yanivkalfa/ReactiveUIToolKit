using System;
using System.Collections.Immutable;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.IntelliSense
{
    // ── CursorKind ────────────────────────────────────────────────────────────

    /// <summary>
    /// The semantic category of the cursor position inside a .uitkx document.
    /// Mirrors <c>DocumentContext.CompletionKind</c> but derived from the real AST.
    /// </summary>
    public enum CursorKind
    {
        /// <summary>Not in a position that supports IntelliSense.</summary>
        None,

        /// <summary>Cursor is after <c>@</c> in the directive block (before any markup).</summary>
        DirectiveName,

        /// <summary>Cursor is after <c>@</c> inside the markup body (control-flow keywords).</summary>
        ControlFlowName,

        /// <summary>Cursor is typing an element tag name after <c>&lt;</c>.</summary>
        TagName,

        /// <summary>Cursor is inside an open tag, after the tag name (typing an attribute name).</summary>
        AttributeName,

        /// <summary>Cursor is inside an attribute value: <c>attr="…"</c> or <c>attr={…}</c>.</summary>
        AttributeValue,

        /// <summary>
        /// Cursor is inside an inline <c>{expr}</c> expression — C# IntelliSense applies.
        /// The <see cref="CursorContext.Prefix"/> and <see cref="CursorContext.Word"/>
        /// fields hold the partial identifier under the cursor.
        /// </summary>
        CSharpExpression,

        /// <summary>
        /// Cursor is inside an <c>@code { … }</c> block (or a function-style
        /// component body) — C# IntelliSense applies.
        /// </summary>
        CSharpCodeBlock,
    }

    // ── CursorContext ─────────────────────────────────────────────────────────

    /// <summary>
    /// The result of <see cref="AstCursorContext.Find"/>.
    /// Provides the semantic context and partial tokens for completions,
    /// hover, and go-to-definition.
    /// </summary>
    public sealed class CursorContext
    {
        /// <summary>Singleton returned when the cursor is in a non-meaningful position.</summary>
        public static readonly CursorContext Empty = new CursorContext();

        /// <summary>What kind of token the cursor is on.</summary>
        public CursorKind Kind { get; init; } = CursorKind.None;

        /// <summary>
        /// For <see cref="CursorKind.TagName"/>: the tag name at the cursor.<br/>
        /// For <see cref="CursorKind.AttributeName"/> / <see cref="CursorKind.AttributeValue"/>:
        /// the enclosing element's tag name (from the AST).
        /// </summary>
        public string? TagName { get; init; }

        /// <summary>
        /// For <see cref="CursorKind.AttributeValue"/>: the name of the attribute
        /// whose value the cursor is editing.
        /// </summary>
        public string? AttributeName { get; init; }

        /// <summary>
        /// The partial identifier <em>before</em> the cursor position — used as
        /// the filter prefix for completion lists.
        /// </summary>
        public string Prefix { get; init; } = "";

        /// <summary>
        /// The full identifier word <em>under</em> the cursor (prefix + characters
        /// after the cursor up to a word boundary).  Use this for hover and
        /// go-to-definition lookups.
        /// </summary>
        public string Word { get; init; } = "";
    }

    // ── AstCursorContext ──────────────────────────────────────────────────────

    /// <summary>
    /// Determines the semantic context of the cursor in a .uitkx document by
    /// combining AST structural knowledge (which element / attribute the cursor
    /// is associated with) with a single-line text scan (exact sub-position
    /// within a tag).
    ///
    /// <para>
    /// Column-level positioning within a tag/attribute is resolved by scanning
    /// the raw line text rather than reading node span data — a deliberate
    /// simplification, not a parser limitation (<see cref="AstNode.SourceColumn"/>
    /// is populated for elements/attributes, see <c>SemanticTokensProvider</c>).
    /// </para>
    /// <para>
    /// U-32: block "2a" of <see cref="Find"/> only finds an inline expression's
    /// opening '{' when it lives on the SAME line as the cursor, so a cursor on a
    /// continuation line of a multi-line child/attribute expression (a wrapped
    /// ternary split across lines) used to fall through to plain tag/AST
    /// classification instead of <see cref="CursorKind.CSharpExpression"/>. A first
    /// attempt fixed this with a bounded backward TEXT scan counting unclosed '{'
    /// — that was wrong: a plain brace-depth count can't distinguish an inline
    /// expression's '{' from an enclosing STRUCTURAL one (the component body, an
    /// <c>@if</c>/<c>@foreach</c> block, …), so it misclassified cursors near the
    /// top of ordinary files (10 tests failed) and was reverted. Block "2b" instead
    /// walks the ACTUAL parsed <c>ExpressionNode</c>/attribute
    /// <c>CSharpExpressionValue</c> spans (each already carries an absolute
    /// <c>ExpressionOffset</c> + length from the parser) and checks whether the
    /// cursor's line falls inside one — exact, not heuristic, and structural braces
    /// never produce an <c>ExpressionNode</c> in the first place so they can't be
    /// mismatched for one.
    /// </para>
    /// </summary>
    public static class AstCursorContext
    {
        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Returns the cursor context for IntelliSense at the given position.
        /// </summary>
        /// <param name="parseResult">The fully-parsed document.</param>
        /// <param name="text">The raw source text of the same document.</param>
        /// <param name="line1">1-based cursor line (LSP <c>Position.Line + 1</c>).</param>
        /// <param name="col0">0-based cursor column (LSP <c>Position.Character</c>).</param>
        public static CursorContext Find(ParseResult parseResult, string text, int line1, int col0)
        {
            string lineText = GetLine(text, line1);
            if (col0 > lineText.Length)
                col0 = lineText.Length;

            string prefix = ExtractIdentifierBefore(lineText, col0);
            string word = prefix + ExtractIdentifierAfter(lineText, col0);
            int prefixStart = col0 - prefix.Length;

            // ── 1. Control-flow '@' anywhere in the markup body ────────────────
            if (prefixStart > 0 && lineText[prefixStart - 1] == '@')
                return new CursorContext
                {
                    Kind = CursorKind.ControlFlowName,
                    Prefix = prefix,
                    Word = word,
                };
            // ―― 2a. Inline expression {word} ―――――――――――――――――――――――――――――
            // When the cursor is inside a child- or attribute-position {expr},
            // return a context with the word so go-to-definition and completion
            // can scan for its declaration in the surrounding scope.
            if (!string.IsNullOrEmpty(word))
            {
                int searchStart = System.Math.Max(0, prefixStart - 1);
                int braceOpen = lineText.LastIndexOf('{', searchStart);
                if (braceOpen < 0 && prefixStart >= 1)
                    braceOpen = lineText.LastIndexOf('{', prefixStart - 1);
                if (braceOpen >= 0)
                {
                    // Distinguish child-expression `{expr}` from attribute-value
                    // `attr={expr}`: if the last non-whitespace char before '{'
                    // is '=', this is an attribute value — leave classification
                    // to ClassifyTagPosition (block 4) which returns AttributeValue.
                    int prev = braceOpen - 1;
                    while (prev >= 0 && (lineText[prev] == ' ' || lineText[prev] == '\t'))
                        prev--;
                    if (prev >= 0 && lineText[prev] == '=')
                    {
                        // Skip this short-circuit; fall through to AST/position scan.
                    }
                    else
                    {
                    // Verify cursor is between '{' and the matching '}'
                    int inner = braceOpen + 1;
                    int depth = 1;
                    int j = inner;
                    while (j < lineText.Length && depth > 0)
                    {
                        if (lineText[j] == '{')
                            depth++;
                        else if (lineText[j] == '}')
                        {
                            depth--;
                            if (depth == 0)
                                break;
                        }
                        j++;
                    }
                    if (col0 >= inner && col0 <= j)
                        return new CursorContext
                        {
                            Kind   = CursorKind.CSharpExpression,
                            Prefix = prefix,
                            Word   = word,
                        };
                    }
                }

                // ── 2b. Multi-line inline expression (U-32) ─────────────────────
                // Block 2a only finds the opening '{' when it lives on THIS line.
                // A cursor on a continuation line of a multi-line child expression
                // (e.g. a wrapped ternary split across lines) has no '{' of its
                // own, so the block above never fires. Unlike a text-based brace
                // scan (which can't tell an expression's '{' from an enclosing
                // component/@if body's '{' and was reverted after breaking 10
                // tests — see the class doc), this walks the ACTUAL parsed
                // ExpressionNode/attribute spans (SourceLine + ExpressionOffset/
                // ExpressionLength, already tracked by the parser) and checks
                // whether line1 falls inside one. No heuristic: either a real
                // expression node's span contains this line, or it doesn't.
                if (braceOpen < 0
                    && FindMultiLineExpressionSpan(parseResult.RootNodes, text, line1, out bool isAttrValueSpan)
                    && !isAttrValueSpan)
                {
                    return new CursorContext
                    {
                        Kind   = CursorKind.CSharpExpression,
                        Prefix = prefix,
                        Word   = word,
                    };
                }
            }
            // ── 3. AST walk: find the element / attribute that owns this line ──
            string? astTagName = null;
            string? astAttrName = null;
            FindAstContext(parseResult.RootNodes, line1, ref astTagName, ref astAttrName);

            // ── 3a. Function-style setup code ─────────────────────────────────
            // Lines at or after FunctionSetupStartLine are C# setup code unless
            // the AST places the cursor inside a markup element's open-tag.
            // This covers `allowNextRef.Current`, `StyleKeys.Color`, etc.
            // Skip this short-circuit when the cursor is inside a setup-code
            // JSX block — let ClassifyTagPosition handle tag/attribute detection.
            if (parseResult.Directives.IsFunctionStyle
                && parseResult.Directives.FunctionSetupStartLine > 0
                && line1 >= parseResult.Directives.FunctionSetupStartLine
                && astTagName == null
                && !IsInsideSetupCodeMarkup(parseResult, text, line1, col0))
            {
                return new CursorContext
                {
                    Kind   = CursorKind.CSharpCodeBlock,
                    Prefix = prefix,
                    Word   = word,
                };
            }

            // ── 4. Single-line position scan ───────────────────────────────────
            return ClassifyTagPosition(lineText, col0, prefix, word, astTagName, astAttrName);
        }

        // ── Line text helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the cursor (1-based line, 0-based col) falls
        /// inside one of the setup-code JSX block ranges.
        /// </summary>
        private static bool IsInsideSetupCodeMarkup(
            ParseResult parseResult, string text, int line1, int col0)
        {
            if (parseResult.Directives.SetupCodeMarkupRanges.IsDefaultOrEmpty
                && parseResult.Directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
                return false;
            int offset = OffsetFromLineCol(text, line1, col0);
            if (!parseResult.Directives.SetupCodeMarkupRanges.IsDefaultOrEmpty)
                foreach (var (start, end, _) in parseResult.Directives.SetupCodeMarkupRanges)
                    if (offset >= start && offset < end) return true;
            if (!parseResult.Directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
                foreach (var (start, end, _) in parseResult.Directives.SetupCodeBareJsxRanges)
                    if (offset >= start && offset < end) return true;
            return false;
        }

        /// <summary>Converts 1-based line + 0-based column to absolute character offset.</summary>
        private static int OffsetFromLineCol(string text, int line1, int col0)
        {
            int pos = 0;
            for (int l = 1; l < line1 && pos < text.Length; l++)
            {
                int nl = text.IndexOf('\n', pos);
                if (nl < 0) { pos = text.Length; break; }
                pos = nl + 1;
            }
            return pos + col0;
        }

        /// <summary>Returns the raw text of the 1-based <paramref name="line1"/> (CR/LF stripped).</summary>
        private static string GetLine(string text, int line1)
        {
            int pos = 0;
            for (int l = 1; l < line1 && pos < text.Length; l++)
            {
                int nl = text.IndexOf('\n', pos);
                if (nl < 0)
                {
                    pos = text.Length;
                    break;
                }
                pos = nl + 1;
            }
            int end = text.IndexOf('\n', pos);
            if (end < 0)
                end = text.Length;
            string raw = text.Substring(pos, end - pos);
            if (raw.Length > 0 && raw[raw.Length - 1] == '\r')
                raw = raw.Substring(0, raw.Length - 1);
            return raw;
        }

        private static string ExtractIdentifierBefore(string line, int col)
        {
            int end = col,
                start = end;
            while (start > 0 && IsIdentChar(line[start - 1]))
                start--;
            return line.Substring(start, end - start);
        }

        private static string ExtractIdentifierAfter(string line, int col)
        {
            int start = col,
                end = start;
            while (end < line.Length && IsIdentChar(line[end]))
                end++;
            return line.Substring(start, end - start);
        }

        private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        // ── Multi-line expression span lookup (U-32) ──────────────────────────

        /// <summary>
        /// 1-based line number containing <paramref name="offset"/>, computed from the
        /// start of <paramref name="text"/> — independent of any node's own SourceLine,
        /// so it's exact even when a node's tracked expression content starts on a LATER
        /// line than the node's own SourceLine (e.g. the opening '{' alone on its line,
        /// with the expression content beginning on the next line).
        /// </summary>
        private static int LineOfOffset(string text, int offset)
        {
            int line = 1;
            int limit = Math.Min(offset, text.Length);
            for (int i = 0; i < limit; i++)
                if (text[i] == '\n')
                    line++;
            return line;
        }

        /// <summary>
        /// True when <paramref name="line1"/> falls on a CONTINUATION line (strictly
        /// after the opening line) of a real parsed expression span — either a child
        /// <see cref="ExpressionNode"/> or an <see cref="AttributeNode"/> whose value is
        /// a <see cref="CSharpExpressionValue"/>. <paramref name="isAttributeValue"/>
        /// distinguishes the two so the caller can mirror block 2a's exclusion (an
        /// attribute value is classified elsewhere, not as a bare child expression).
        /// Uses the parser-tracked absolute offset + length of the expression, so this
        /// is exact — no brace-counting guesswork about which '{' owns which span.
        /// </summary>
        private static bool FindMultiLineExpressionSpan(
            ImmutableArray<AstNode> nodes, string text, int line1, out bool isAttributeValue)
        {
            foreach (var node in nodes)
            {
                if (TryFindExpressionSpanInNode(node, text, line1, out isAttributeValue))
                    return true;
            }
            isAttributeValue = false;
            return false;
        }

        private static bool TryFindExpressionSpanInNode(
            AstNode node, string text, int line1, out bool isAttributeValue)
        {
            isAttributeValue = false;
            switch (node)
            {
                case ExpressionNode en:
                    if (en.ExpressionLength > 0)
                    {
                        int endLine = LineOfOffset(text, en.ExpressionOffset + en.ExpressionLength);
                        if (line1 > en.SourceLine && line1 <= endLine)
                            return true;
                    }
                    return false;

                case ElementNode el:
                    foreach (var attr in el.Attributes)
                    {
                        if (attr.Value is CSharpExpressionValue cse && cse.ExpressionOffset > 0
                            && cse.Expression.Length > 0)
                        {
                            int endLine = LineOfOffset(text, cse.ExpressionOffset + cse.Expression.Length);
                            if (line1 > attr.SourceLine && line1 <= endLine)
                            {
                                isAttributeValue = true;
                                return true;
                            }
                        }
                    }
                    foreach (var child in el.Children)
                        if (TryFindExpressionSpanInNode(child, text, line1, out isAttributeValue))
                            return true;
                    return false;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        foreach (var n in branch.Payload.Body)
                            if (TryFindExpressionSpanInNode(n, text, line1, out isAttributeValue))
                                return true;
                    return false;

                case ForeachNode fe:
                    foreach (var n in fe.Payload.Body)
                        if (TryFindExpressionSpanInNode(n, text, line1, out isAttributeValue))
                            return true;
                    return false;

                case ForNode fo:
                    foreach (var n in fo.Payload.Body)
                        if (TryFindExpressionSpanInNode(n, text, line1, out isAttributeValue))
                            return true;
                    return false;

                case WhileNode wh:
                    foreach (var n in wh.Payload.Body)
                        if (TryFindExpressionSpanInNode(n, text, line1, out isAttributeValue))
                            return true;
                    return false;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                        foreach (var n in c.Payload.Body)
                            if (TryFindExpressionSpanInNode(n, text, line1, out isAttributeValue))
                                return true;
                    return false;

                default:
                    return false;
            }
        }

        // ── AST structural context ────────────────────────────────────────────

        /// <summary>
        /// Walks the AST to find the <see cref="ElementNode"/> whose open-tag or
        /// attributes land on <paramref name="line1"/>.
        /// On success, sets <paramref name="tagName"/> and optionally
        /// <paramref name="attrName"/> (when an attribute on that line was found).
        /// </summary>
        private static void FindAstContext(
            ImmutableArray<AstNode> nodes,
            int line1,
            ref string? tagName,
            ref string? attrName
        )
        {
            foreach (var node in nodes)
            {
                WalkNode(node, line1, ref tagName, ref attrName);
                if (tagName != null)
                    return;
            }
        }

        private static void WalkNode(
            AstNode node,
            int line1,
            ref string? tagName,
            ref string? attrName
        )
        {
            switch (node)
            {
                case ElementNode el:
                {
                    // Check attributes first: an attribute on this line means the
                    // cursor is inside this element's open-tag.
                    foreach (var attr in el.Attributes)
                    {
                        if (attr.SourceLine == line1)
                        {
                            tagName = el.TagName;
                            attrName = attr.Name;
                            return;
                        }
                    }
                    // Open-tag line itself.
                    if (el.SourceLine == line1)
                    {
                        tagName = el.TagName;
                        return;
                    }
                    // Recurse into children.
                    foreach (var child in el.Children)
                    {
                        WalkNode(child, line1, ref tagName, ref attrName);
                        if (tagName != null)
                            return;
                    }
                    break;
                }
                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        WalkBody(branch.Payload.Body, line1, ref tagName, ref attrName);
                    break;
                case ForeachNode fe:
                    WalkBody(fe.Payload.Body, line1, ref tagName, ref attrName);
                    break;
                case ForNode fo:
                    WalkBody(fo.Payload.Body, line1, ref tagName, ref attrName);
                    break;
                case WhileNode wh:
                    WalkBody(wh.Payload.Body, line1, ref tagName, ref attrName);
                    break;
                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                        WalkBody(c.Payload.Body, line1, ref tagName, ref attrName);
                    break;
            }
        }

        private static void WalkBody(
            ImmutableArray<AstNode> body,
            int line1,
            ref string? tagName,
            ref string? attrName
        )
        {
            foreach (var node in body)
            {
                WalkNode(node, line1, ref tagName, ref attrName);
                if (tagName != null)
                    return;
            }
        }

        // ── Single-line tag classification ────────────────────────────────────

        private static CursorContext ClassifyTagPosition(
            string line,
            int col0,
            string prefix,
            string word,
            string? astTagName,
            string? astAttrName
        )
        {
            // Find the last unclosed '<' before col0 on this line.
            int tagOpen = FindLastOpenAngle(line, col0);

            if (tagOpen < 0)
            {
                // No open '<' found via text scan.
                // If the AST found a multi-line open-tag attribute on this line, use it.
                if (astTagName != null && astAttrName != null)
                {
                    // Detect whether the cursor is inside a {…} attribute value
                    // on this line rather than on the attribute name itself.
                    if (IsInsideBracedValue(line, col0))
                    {
                        string valPrefix = ExtractIdentifierBefore(line, col0);
                        string valWord = valPrefix + ExtractIdentifierAfter(line, col0);
                        return new CursorContext
                        {
                            Kind = CursorKind.AttributeValue,
                            TagName = astTagName,
                            AttributeName = astAttrName,
                            Prefix = valPrefix,
                            Word = valWord,
                        };
                    }

                    return new CursorContext
                    {
                        Kind = CursorKind.AttributeName,
                        TagName = astTagName,
                        AttributeName = astAttrName,
                        Prefix = prefix,
                        Word = word,
                    };
                }
                // No tag context but there is a word under the cursor (e.g. inside @code
                // raw C# text, or between elements) — return it so hover/nav can still work.
                if (!string.IsNullOrEmpty(word))
                    return new CursorContext
                    {
                        Kind = CursorKind.None,
                        Prefix = prefix,
                        Word = word,
                    };
                return CursorContext.Empty;
            }

            // Walk the tag region from (tagOpen + 1) to col0 with a state machine.
            int i = tagOpen + 1;

            // Skip optional '/' for closing tags – those don't need completions.
            bool isClosingTag = i < line.Length && line[i] == '/';
            if (isClosingTag)
                i++;

            // ── Tag name phase ─────────────────────────────────────────────────
            int tagNameStart = i;
            while (i < line.Length && IsIdentChar(line[i]))
                i++;
            string textTagName = line.Substring(tagNameStart, i - tagNameStart);

            // Cursor is in the tag name span → TagName completion.
            if (col0 >= tagNameStart && col0 <= i)
            {
                if (isClosingTag)
                    return new CursorContext
                    {
                        Kind = CursorKind.None,
                        Word = !string.IsNullOrEmpty(textTagName) ? textTagName : word,
                    };
                string resolvedTag =
                    !string.IsNullOrEmpty(astTagName) ? astTagName!
                    : !string.IsNullOrEmpty(textTagName) ? textTagName
                    : "";
                return new CursorContext
                {
                    Kind = CursorKind.TagName,
                    TagName = resolvedTag,
                    Prefix = prefix,
                    Word = word,
                };
            }

            if (isClosingTag)
                return CursorContext.Empty;

            // Prefer the local text-scanned tag name when available (important
            // for same-line nested tags like <Box><Label .../></Box> where AST
            // line-only context can resolve to the outer element).
            string effectiveTag =
                !string.IsNullOrEmpty(textTagName) ? textTagName
                : !string.IsNullOrEmpty(astTagName) ? astTagName!
                : "";

            // ── Attribute list phase ───────────────────────────────────────────
            while (i <= col0 && i < line.Length)
            {
                // Skip whitespace.
                while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
                    i++;
                if (i > col0 || i >= line.Length)
                    break;

                char ch = line[i];

                // Self-closing '/>' or closing '>'.
                if (ch == '>' || ch == '/')
                    break;

                // Attribute name token.
                if (IsIdentChar(ch))
                {
                    int attrStart = i;
                    while (i < line.Length && IsIdentChar(line[i]))
                        i++;
                    string attrTok = line.Substring(attrStart, i - attrStart);

                    // Cursor is within this attribute name span.
                    if (col0 > attrStart && col0 <= i)
                        return new CursorContext
                        {
                            Kind = CursorKind.AttributeName,
                            TagName = effectiveTag ?? "",
                            AttributeName = attrTok,
                            Prefix = prefix,
                            Word = word,
                        };

                    // Skip optional whitespace before '='.
                    while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
                        i++;

                    if (i < line.Length && line[i] == '=')
                    {
                        i++; // consume '='

                        // Skip whitespace after '='.
                        while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
                            i++;

                        if (i < line.Length && (line[i] == '"' || line[i] == '{'))
                        {
                            char open = line[i];
                            char close = open == '"' ? '"' : '}';
                            i++; // consume opening delimiter
                            int valueStart = i;

                            // Scan to closing delimiter (respecting {/} nesting for expressions).
                            int depth = 1;
                            while (i < line.Length)
                            {
                                if (open == '{')
                                {
                                    if (line[i] == '{')
                                        depth++;
                                    else if (line[i] == '}')
                                    {
                                        depth--;
                                        if (depth == 0)
                                            break;
                                    }
                                }
                                else
                                {
                                    if (line[i] == '"')
                                        break;
                                }
                                i++;
                            }
                            int valueEnd = i; // exclusive

                            // Cursor is inside this value span.
                            if (col0 >= valueStart && col0 <= valueEnd)
                            {
                                string valPrefix = ExtractIdentifierBefore(line, col0);
                                string valWord = valPrefix + ExtractIdentifierAfter(line, col0);
                                return new CursorContext
                                {
                                    Kind = CursorKind.AttributeValue,
                                    TagName = effectiveTag,
                                    AttributeName = attrTok,
                                    Prefix = valPrefix,
                                    Word = valWord,
                                };
                            }

                            if (i < line.Length)
                                i++; // consume closing delimiter
                        }
                    }
                }
                else
                {
                    i++; // unrecognised char — advance
                }
            }

            // Cursor ended up past all scanned attribute tokens → typing a new attribute name.
            return new CursorContext
            {
                Kind = CursorKind.AttributeName,
                TagName = effectiveTag,
                Prefix = prefix,
                Word = word,
            };
        }

        // ── Angle-bracket scanner ─────────────────────────────────────────────

        /// <summary>
        /// Returns the character index of the last unclosed <c>&lt;</c> before
        /// <paramref name="col0"/>, or <c>-1</c> if none found.
        /// </summary>
        private static int FindLastOpenAngle(string line, int col0)
        {
            int result = -1;
            int depth = 0;
            int limit = Math.Min(col0, line.Length);
            for (int i = 0; i < limit; i++)
            {
                if (line[i] == '<')
                {
                    result = i;
                    depth++;
                }
                else if (line[i] == '>')
                {
                    depth--;
                    if (depth <= 0)
                    {
                        result = -1;
                        depth = 0;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the cursor at <paramref name="col0"/> is inside
        /// a <c>{…}</c> expression value on this line (e.g. <c>text={param}</c>
        /// on a multi-line open-tag continuation line).
        /// </summary>
        private static bool IsInsideBracedValue(string line, int col0)
        {
            int depth = 0;
            int limit = Math.Min(col0, line.Length);
            for (int i = 0; i < limit; i++)
            {
                if (line[i] == '{') depth++;
                else if (line[i] == '}') depth--;
            }
            return depth > 0;
        }
    }
}
