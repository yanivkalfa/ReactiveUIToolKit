using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.SemanticTokens
{
    /// <summary>
    /// Walks a <see cref="ParseResult"/> and the matching source text to produce
    /// <see cref="SemanticTokenData"/> records that a language server can encode
    /// as LSP <c>textDocument/semanticTokens/full</c> data.
    ///
    /// Token categories emitted:
    /// <list type="bullet">
    ///   <item>Directive-block keywords (<c>@namespace</c>, <c>@component</c>, …) and their values</item>
    ///   <item>Element open-tag names (<c>Box</c>, <c>Label</c>, custom components)</item>
    ///   <item>Attribute names (<c>text</c>, <c>key</c>, …)</item>
    ///   <item>Control-flow keywords (<c>@if</c>, <c>@else</c>, <c>@foreach</c>, <c>@switch</c>, …)</item>
    ///   <item>Inline expression delimiters (<c>@(</c>)</item>
    /// </list>
    ///
    /// Column positions are resolved by scanning the raw source text because
    /// <see cref="AstNode.SourceColumn"/> defaults to 0 in the current parser
    /// version.
    /// </summary>
    public sealed class SemanticTokensProvider
    {
        // ── Top-level directive keywords ──────────────────────────────────────

        private static readonly HashSet<string> s_topLevelKeywords = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            "namespace",
            "component",
            "using",
            "props",
            "key",
        };

        private static readonly string[] s_noMods = Array.Empty<string>();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all semantic tokens for <paramref name="parseResult"/>,
        /// resolved to absolute 0-based line/column positions via
        /// <paramref name="source"/>.  The returned array is sorted by line,
        /// then column.
        /// </summary>
        /// <param name="parseResult">Fully-parsed UITKX document.</param>
        /// <param name="source">The raw source text of the same document.</param>
        /// <param name="knownElements">
        /// Set of component names that exist in the workspace (suffix "Props" stripped).
        /// When <c>null</c> (e.g. the index is not yet ready) every element receives a
        /// token so the file is never un-highlighted.  When non-null, PascalCase element
        /// names that are NOT in the set are skipped — the LSP diagnostic squiggle
        /// becomes their sole visual indicator, avoiding a double-underline effect.
        /// </param>
        public SemanticTokenData[] GetTokens(
            ParseResult parseResult,
            string source,
            HashSet<string>? knownElements = null
        )
        {
            var tokens = new List<SemanticTokenData>(64);
            int[] lineStarts = BuildLineStarts(source);

            // 1. Directive block (every line before markup starts)
            CollectDirectiveTokens(parseResult.Directives, source, lineStarts, tokens);

            // 2. AST markup nodes
            foreach (var node in parseResult.RootNodes)
                CollectNodeTokens(node, source, lineStarts, tokens, knownElements);

            // 3. Hook setter variables — whole-source scan (covers all @code blocks)
            CollectHookSetterTokens(source, lineStarts, tokens);

            return tokens.OrderBy(t => t.Line).ThenBy(t => t.Column).ToArray();
        }

        // ── Directive block ───────────────────────────────────────────────────

        private static void CollectDirectiveTokens(
            DirectiveSet directives,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            // Lines 1..(MarkupStartLine-1) are directive lines (1-based).
            int markupLine1 = directives.MarkupStartLine;

            for (int line1 = 1; line1 < markupLine1; line1++)
            {
                int lineStart = lineStarts[line1 - 1];
                int lineEnd = line1 < lineStarts.Length ? lineStarts[line1] : source.Length;
                int len = lineEnd - lineStart;
                if (len <= 0)
                    continue;

                // Build line text (stripped of trailing newline chars)
                string lineText = source.Substring(lineStart, len).TrimEnd('\r', '\n');

                // Find '@' on this line
                int atCol = lineText.IndexOf('@');
                if (atCol < 0)
                    continue;

                // Read identifier after '@'
                int kwStart = atCol + 1;
                int kwEnd = kwStart;
                while (kwEnd < lineText.Length && char.IsLetter(lineText[kwEnd]))
                    kwEnd++;

                string keyword = lineText.Substring(kwStart, kwEnd - kwStart);
                if (!s_topLevelKeywords.Contains(keyword))
                    continue;

                // Emit '@keyword' token
                int fullKwLen = kwEnd - atCol; // includes the '@'
                EmitToken(
                    tokens,
                    line1 - 1,
                    atCol,
                    fullKwLen,
                    SemanticTokenTypes.Directive,
                    s_noMods
                );

                // Find and emit directive value
                int valStart = kwEnd;
                while (
                    valStart < lineText.Length
                    && (lineText[valStart] == ' ' || lineText[valStart] == '\t')
                )
                    valStart++;

                // Trim trailing whitespace
                int valEnd = lineText.Length;
                while (
                    valEnd > valStart
                    && (lineText[valEnd - 1] == ' ' || lineText[valEnd - 1] == '\t')
                )
                    valEnd--;

                if (valEnd > valStart)
                    EmitToken(
                        tokens,
                        line1 - 1,
                        valStart,
                        valEnd - valStart,
                        SemanticTokenTypes.DirectiveName,
                        s_noMods
                    );
            }
        }

        // ── AST node walk ─────────────────────────────────────────────────────

        private static void CollectNodeTokens(
            AstNode node,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            switch (node)
            {
                case ElementNode el:
                    CollectElementTokens(el, source, lineStarts, tokens, knownElements);
                    break;

                case IfNode ifN:
                    CollectIfTokens(ifN, source, lineStarts, tokens, knownElements);
                    break;

                case ForeachNode fe:
                    EmitKeyword(tokens, source, lineStarts, fe.SourceLine, "@foreach");
                    foreach (var child in fe.Body)
                        CollectNodeTokens(child, source, lineStarts, tokens, knownElements);
                    break;

                case ForNode fo:
                    EmitKeyword(tokens, source, lineStarts, fo.SourceLine, "@for");
                    foreach (var child in fo.Body)
                        CollectNodeTokens(child, source, lineStarts, tokens, knownElements);
                    break;

                case WhileNode wh:
                    EmitKeyword(tokens, source, lineStarts, wh.SourceLine, "@while");
                    foreach (var child in wh.Body)
                        CollectNodeTokens(child, source, lineStarts, tokens, knownElements);
                    break;

                case SwitchNode sw:
                    CollectSwitchTokens(sw, source, lineStarts, tokens, knownElements);
                    break;

                case CodeBlockNode cb:
                    EmitKeyword(tokens, source, lineStarts, cb.SourceLine, "@code");
                    // Walk elements embedded inside @code (var x = <Tag ...>)
                    foreach (var rm in cb.ReturnMarkups)
                        CollectElementTokens(rm.Element, source, lineStarts, tokens, knownElements);
                    // Emit semantic tokens for all C# content inside @code
                    CollectCodeBlockBodyTokens(cb, source, lineStarts, tokens);
                    break;

                case ExpressionNode ex:
                    EmitKeyword(tokens, source, lineStarts, ex.SourceLine, "@(");
                    break;

                case JsxCommentNode jc:
                {
                    EmitJsxCommentTokens(jc, source, lineStarts, tokens);
                    break;
                }

                case TextNode _:
                    break; // plain text — no semantic tokens
            }
        }

        // ── Element ───────────────────────────────────────────────────────────

        private static void CollectElementTokens(
            ElementNode el,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            // Open-tag name column (search for '<TagName', not '</TagName')
            int openNameCol = FindOpenTagName(source, lineStarts, el.SourceLine, el.TagName);
            if (openNameCol >= 0)
                EmitToken(
                    tokens,
                    el.SourceLine - 1,
                    openNameCol,
                    el.TagName.Length,
                    SemanticTokenTypes.Element,
                    s_noMods
                );

            // Attribute names (always emit — attributes on unknown elements are still valid)
            foreach (var attr in el.Attributes)
            {
                int attrCol = FindOnLine(source, lineStarts, attr.SourceLine, attr.Name);
                if (attrCol >= 0)
                    EmitToken(
                        tokens,
                        attr.SourceLine - 1,
                        attrCol,
                        attr.Name.Length,
                        SemanticTokenTypes.Attribute,
                        s_noMods
                    );
            }

            // Recurse into children
            foreach (var child in el.Children)
                CollectNodeTokens(child, source, lineStarts, tokens, knownElements);

            // Close-tag name (block elements only — self-closing have CloseTagLine == 0)
            if (el.CloseTagLine > 0)
            {
                int closeNameCol = FindCloseTagName(
                    source,
                    lineStarts,
                    el.CloseTagLine,
                    el.TagName
                );
                if (closeNameCol >= 0)
                    EmitToken(
                        tokens,
                        el.CloseTagLine - 1,
                        closeNameCol,
                        el.TagName.Length,
                        SemanticTokenTypes.Element,
                        s_noMods
                    );
            }
        }

        // ── @if / @else if / @else ────────────────────────────────────────────

        private static void CollectIfTokens(
            IfNode ifNode,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            bool isFirst = true;
            foreach (var branch in ifNode.Branches)
            {
                if (isFirst)
                {
                    EmitKeyword(tokens, source, lineStarts, branch.SourceLine, "@if");
                    isFirst = false;
                }
                else
                {
                    // '@else if (cond)' or '@else'  – emit '@else' in both cases
                    EmitKeyword(tokens, source, lineStarts, branch.SourceLine, "@else");
                    // If there is a condition, also emit 'if' keyword that follows '@else '
                    if (branch.Condition != null)
                    {
                        int elseCol = FindOnLine(source, lineStarts, branch.SourceLine, "@else");
                        if (elseCol >= 0)
                        {
                            // '@else' is 5 chars; skip it plus 1 space → look for 'if'
                            int ifCol = FindOnLine(
                                source,
                                lineStarts,
                                branch.SourceLine,
                                "if",
                                elseCol + "@else".Length
                            );
                            if (ifCol >= 0)
                                EmitToken(
                                    tokens,
                                    branch.SourceLine - 1,
                                    ifCol,
                                    2,
                                    SemanticTokenTypes.Directive,
                                    s_noMods
                                );
                        }
                    }
                }

                foreach (var child in branch.Body)
                    CollectNodeTokens(child, source, lineStarts, tokens, knownElements);
            }
        }

        // ── @switch / @case / @default ────────────────────────────────────────

        private static void CollectSwitchTokens(
            SwitchNode sw,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            EmitKeyword(tokens, source, lineStarts, sw.SourceLine, "@switch");

            foreach (var c in sw.Cases)
            {
                string kw = c.ValueExpression != null ? "@case" : "@default";
                EmitKeyword(tokens, source, lineStarts, c.SourceLine, kw);

                foreach (var child in c.Body)
                    CollectNodeTokens(child, source, lineStarts, tokens, knownElements);
            }
        }

        // ── @code body C# tokenizer ─────────────────────────────────────────

        /// <summary>
        /// Composite tokeniser for C# source inside <c>@code { }</c> blocks.
        /// Groups (tried left-to-right, so earlier groups win):
        ///   str  — string / interpolated string / verbatim string literal
        ///   num  — numeric literal
        ///   kw   — C# keyword or built-in type alias
        ///   func — identifier immediately followed by (
        ///   type — PascalCase identifier (class / type name)
        ///   var  — camelCase / underscore identifier
        /// </summary>
        private static readonly Regex s_codeBodyTokenRegex = new Regex(
            @"(?<str>\$?@?""(?:[^""\\]|\\.)*"")"
                + @"|(?<num>\b\d+(?:\.\d+)?[fFdDmMuUlL]*\b)"
                + @"|(?<kw>\b(?:var|int|string|bool|float|double|decimal|long|uint|ulong|byte|char|object|void"
                + @"|return|await|async|if|else|for|foreach|while|do|break|continue|in|new|this"
                + @"|null|true|false|typeof|nameof|is|as|out|ref|readonly|const|static|using"
                + @"|throw|catch|finally|params|class|interface|struct|enum|record"
                + @"|private|public|protected|internal|abstract|override|virtual|sealed)\b)"
                + @"|(?<func>[a-zA-Z_][A-Za-z0-9_]*)(?=\s*\()"
                + @"|(?<type>\b[A-Z][A-Za-z0-9]*\b)"
                + @"|(?<var>\b[a-z_][A-Za-z0-9_]*\b)",
            RegexOptions.Compiled
        );

        // ── Hook setter coloring ────────────────────────────────────────────

        // Matches: var (anyState, <setter>) = useState(  or  Hooks.UseState(
        private static readonly Regex s_hookTupleRegex = new Regex(
            @"\bvar\s*\(\s*\w+\s*,\s*(?<setter>\w+)\s*\)\s*=\s*(?:Hooks\.)?[Uu]se[A-Za-z]+\s*[<(]",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Scans the entire source for hook tuple destructuring and emits
        /// <see cref="SemanticTokenTypes.Function"/> tokens for setter variables.
        /// Called once as a post-pass so it covers hooks at any nesting level.
        /// </summary>
        private static void CollectHookSetterTokens(
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            var matches = s_hookTupleRegex.Matches(source);
            foreach (Match m in matches)
            {
                var setter = m.Groups["setter"];
                if (!setter.Success)
                    continue;
                int offset = setter.Index;
                int lineIdx = Array.BinarySearch(lineStarts, offset);
                if (lineIdx < 0)
                    lineIdx = (~lineIdx) - 1;
                if (lineIdx < 0)
                    lineIdx = 0;
                int col = offset - lineStarts[lineIdx];
                EmitToken(
                    tokens,
                    lineIdx,
                    col,
                    setter.Length,
                    SemanticTokenTypes.Function,
                    s_noMods
                );
            }
        }

        // ── @code body scanner ────────────────────────────────────────────────

        private static void EmitJsxCommentTokens(
            JsxCommentNode jc,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            int col = FindOnLine(source, lineStarts, jc.SourceLine, "{/*");
            if (col < 0)
                return;

            int startOffset = lineStarts[jc.SourceLine - 1] + col;
            int closeAt = source.IndexOf("*/}", startOffset, StringComparison.Ordinal);
            if (closeAt < 0)
            {
                int fallbackLen = "{/*".Length + jc.Content.Length + "*/}".Length;
                EmitToken(
                    tokens,
                    jc.SourceLine - 1,
                    col,
                    fallbackLen,
                    SemanticTokenTypes.Comment,
                    s_noMods
                );
                return;
            }

            int endExclusive = closeAt + 3;
            int startLineIdx = Array.BinarySearch(lineStarts, startOffset);
            if (startLineIdx < 0)
                startLineIdx = (~startLineIdx) - 1;
            if (startLineIdx < 0)
                startLineIdx = 0;

            for (int li = startLineIdx; li < lineStarts.Length; li++)
            {
                int lineStart = lineStarts[li];
                if (lineStart >= endExclusive)
                    break;

                int lineEnd = li + 1 < lineStarts.Length ? lineStarts[li + 1] : source.Length;
                int segStart = Math.Max(lineStart, startOffset);
                int segEnd = Math.Min(lineEnd, endExclusive);
                while (
                    segEnd > segStart && (source[segEnd - 1] == '\r' || source[segEnd - 1] == '\n')
                )
                    segEnd--;

                if (segEnd > segStart)
                {
                    EmitToken(
                        tokens,
                        li,
                        segStart - lineStart,
                        segEnd - segStart,
                        SemanticTokenTypes.Comment,
                        s_noMods
                    );
                }
            }
        }

        private static void CollectCodeBlockBodyTokens(
            CodeBlockNode cb,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            // 1. Locate the opening { of @code in source (same line or the next)
            int searchLineStart = lineStarts[cb.SourceLine - 1];
            int searchEnd =
                (cb.SourceLine + 1 < lineStarts.Length)
                    ? lineStarts[cb.SourceLine + 1]
                    : source.Length;
            int openBrace = source.IndexOf(
                '{',
                searchLineStart,
                Math.Min(searchEnd, source.Length) - searchLineStart
            );
            if (openBrace < 0)
                return;

            int bodyStart = openBrace + 1;

            // 2. Find matching closing } by brace depth
            int depth = 1;
            int bodyEnd = bodyStart;
            for (int i = bodyStart; i < source.Length && depth > 0; i++)
            {
                if (source[i] == '{')
                    depth++;
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        bodyEnd = i;
                        break;
                    }
                }
            }
            if (bodyEnd <= bodyStart)
                return;

            // 3. Lines that contain embedded markup — handled by element walker, skip them
            var markupLines = new HashSet<int>();
            foreach (var rm in cb.ReturnMarkups)
            {
                int startLine = rm.Element.SourceLine;
                int endLine =
                    rm.Element.CloseTagLine > 0 ? rm.Element.CloseTagLine : rm.Element.SourceLine;
                if (endLine < startLine)
                    endLine = startLine;

                for (int line = startLine; line <= endLine; line++)
                    markupLines.Add(line);
            }

            // 3b. Lines inside JSX comment spans {/* ... */} in @code should not be
            // tokenized as C#; they are handled by grammar + comment semantic tokens.
            var jsxCommentLines = new HashSet<int>();
            int scan = bodyStart;
            while (scan < bodyEnd)
            {
                int jsxStart = source.IndexOf(
                    "{/*",
                    scan,
                    bodyEnd - scan,
                    StringComparison.Ordinal
                );
                if (jsxStart < 0)
                    break;

                int jsxClose = source.IndexOf(
                    "*/}",
                    jsxStart + 3,
                    bodyEnd - (jsxStart + 3),
                    StringComparison.Ordinal
                );
                int jsxEndExclusive = jsxClose >= 0 ? jsxClose + 3 : bodyEnd;

                int startLineIdx2 = Array.BinarySearch(lineStarts, jsxStart);
                if (startLineIdx2 < 0)
                    startLineIdx2 = (~startLineIdx2) - 1;
                if (startLineIdx2 < 0)
                    startLineIdx2 = 0;

                int endLineIdx2 = Array.BinarySearch(
                    lineStarts,
                    Math.Max(jsxStart, jsxEndExclusive - 1)
                );
                if (endLineIdx2 < 0)
                    endLineIdx2 = (~endLineIdx2) - 1;
                if (endLineIdx2 < 0)
                    endLineIdx2 = 0;

                for (int li2 = startLineIdx2; li2 <= endLineIdx2; li2++)
                    jsxCommentLines.Add(li2 + 1);

                scan = Math.Max(scan + 1, jsxEndExclusive);
            }

            // 4. Determine the first source line that overlaps bodyStart
            int startLineIdx = Array.BinarySearch(lineStarts, bodyStart);
            if (startLineIdx < 0)
                startLineIdx = (~startLineIdx) - 1;
            if (startLineIdx < 0)
                startLineIdx = 0;

            // 5. Walk each line inside the body
            for (int li = startLineIdx; li < lineStarts.Length; li++)
            {
                int lineStart = lineStarts[li];
                if (lineStart >= bodyEnd)
                    break;

                int lineEnd = (li + 1 < lineStarts.Length) ? lineStarts[li + 1] : source.Length;

                // Clip to body bounds
                int segStart = Math.Max(lineStart, bodyStart);
                int segEnd = Math.Min(lineEnd, bodyEnd);
                if (segStart >= segEnd)
                    continue;

                int line1 = li + 1;
                if (markupLines.Contains(line1))
                    continue;
                if (jsxCommentLines.Contains(line1))
                    continue;

                string seg = source.Substring(segStart, segEnd - segStart).TrimEnd('\r', '\n');
                int colBase = segStart - lineStart;

                // Strip // line comment — emit Comment token for it
                int slashIdx = seg.IndexOf("//", StringComparison.Ordinal);
                if (slashIdx >= 0)
                {
                    EmitToken(
                        tokens,
                        li,
                        colBase + slashIdx,
                        seg.Length - slashIdx,
                        SemanticTokenTypes.Comment,
                        s_noMods
                    );
                    seg = seg.Substring(0, slashIdx);
                }

                // Emit C# tokens
                foreach (Match m in s_codeBodyTokenRegex.Matches(seg))
                {
                    if (!m.Success)
                        continue;
                    string ttType;
                    if (m.Groups["str"].Success)
                        ttType = SemanticTokenTypes.String;
                    else if (m.Groups["num"].Success)
                        ttType = SemanticTokenTypes.Number;
                    else if (m.Groups["kw"].Success)
                        ttType = SemanticTokenTypes.Keyword;
                    else if (m.Groups["func"].Success)
                        ttType = SemanticTokenTypes.Function;
                    else if (m.Groups["type"].Success)
                        ttType = SemanticTokenTypes.Type;
                    else if (m.Groups["var"].Success)
                        ttType = SemanticTokenTypes.Variable;
                    else
                        continue;
                    EmitToken(tokens, li, colBase + m.Index, m.Length, ttType, s_noMods);
                }
            }
        }

        // ── Position helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the 0-based column of the element tag name inside its
        /// open tag (<c>&lt;TagName</c>) on the given 1-based source line.
        /// Close tags (<c>&lt;/TagName&gt;</c>) are excluded.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindOpenTagName(
            string source,
            int[] lineStarts,
            int line1,
            string tagName
        )
        {
            // Search for '<TagName'. Close tags start with '</' so they won't
            // match because the char after '<' would be '/' not the tag name.
            int col = FindOnLine(source, lineStarts, line1, "<" + tagName);
            if (col < 0)
                return -1;
            return col + 1; // +1 to skip the '<' character
        }

        /// <summary>
        /// Returns the 0-based column of the tag name inside a close tag
        /// (<c>&lt;/TagName&gt;</c>) on the given 1-based source line.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindCloseTagName(
            string source,
            int[] lineStarts,
            int line1,
            string tagName
        )
        {
            int col = FindOnLine(source, lineStarts, line1, "</" + tagName);
            if (col < 0)
                return -1;
            return col + 2; // +2 to skip '</'
        }

        /// <summary>
        /// Returns the 0-based column of the first occurrence of
        /// <paramref name="search"/> on the given 1-based source line,
        /// starting at character offset <paramref name="startCol"/>.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindOnLine(
            string source,
            int[] lineStarts,
            int line1,
            string search,
            int startCol = 0
        )
        {
            if (line1 < 1 || line1 > lineStarts.Length)
                return -1;

            int lineStart = lineStarts[line1 - 1];
            int lineEnd = line1 < lineStarts.Length ? lineStarts[line1] : source.Length;
            int from = lineStart + startCol;
            if (from >= lineEnd || from >= source.Length)
                return -1;

            int searchLen = lineEnd - from;
            if (searchLen <= 0)
                return -1;

            int idx = source.IndexOf(search, from, searchLen, StringComparison.Ordinal);
            return idx >= 0 ? idx - lineStart : -1;
        }

        /// <summary>
        /// Searches for <paramref name="keyword"/> on <paramref name="line1"/>
        /// and emits a <see cref="SemanticTokenTypes.Directive"/> token.
        /// </summary>
        private static void EmitKeyword(
            List<SemanticTokenData> tokens,
            string source,
            int[] lineStarts,
            int line1,
            string keyword
        )
        {
            int col = FindOnLine(source, lineStarts, line1, keyword);
            if (col >= 0)
                EmitToken(
                    tokens,
                    line1 - 1,
                    col,
                    keyword.Length,
                    SemanticTokenTypes.Directive,
                    s_noMods
                );
        }

        private static void EmitToken(
            List<SemanticTokenData> tokens,
            int line0,
            int col0,
            int length,
            string tokenType,
            string[] modifiers
        )
        {
            if (length <= 0)
                return;
            tokens.Add(
                new SemanticTokenData
                {
                    Line = line0,
                    Column = col0,
                    Length = length,
                    TokenType = tokenType,
                    Modifiers = modifiers,
                }
            );
        }

        // ── Source scanning utility ───────────────────────────────────────────

        /// <summary>
        /// Builds a lookup array where <c>lineStarts[i]</c> is the character
        /// index in <paramref name="source"/> where 0-based line <c>i</c> begins.
        /// </summary>
        private static int[] BuildLineStarts(string source)
        {
            var starts = new List<int>(64) { 0 };
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\n')
                {
                    starts.Add(i + 1);
                }
                else if (source[i] == '\r')
                {
                    // Handle both \r and \r\n
                    if (i + 1 < source.Length && source[i + 1] == '\n')
                        i++;
                    starts.Add(i + 1);
                }
            }
            return starts.ToArray();
        }
    }
}
