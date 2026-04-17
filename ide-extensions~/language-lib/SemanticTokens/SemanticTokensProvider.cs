using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            // 1. Top-level declaration tokens
            CollectFunctionStyleDeclarationTokens(source, lineStarts, tokens);

            // 2. AST markup nodes
            foreach (var node in parseResult.RootNodes)
                CollectNodeTokens(node, source, lineStarts, tokens, knownElements);

            // 3. Setup code JSX (local functions, variable assignments with JSX)
            CollectSetupCodeJsxTokens(parseResult, source, lineStarts, tokens, knownElements);

            return NormalizeTokenConflicts(tokens)
                .OrderBy(t => t.Line)
                .ThenBy(t => t.Column)
                .ToArray();
        }

        private static IEnumerable<SemanticTokenData> NormalizeTokenConflicts(
            List<SemanticTokenData> tokens
        )
        {
            var merged = new Dictionary<(int Line, int Col, int Len), SemanticTokenData>();

            foreach (var token in tokens)
            {
                var key = (token.Line, token.Column, token.Length);
                if (!merged.TryGetValue(key, out var existing))
                {
                    merged[key] = token;
                    continue;
                }

                bool preferFunction =
                    existing.TokenType == SemanticTokenTypes.Variable
                    && token.TokenType == SemanticTokenTypes.Function;

                bool preferComment =
                    existing.TokenType != SemanticTokenTypes.Comment
                    && token.TokenType == SemanticTokenTypes.Comment;

                if (preferFunction || preferComment)
                    merged[key] = token;
            }

            // Suppress non-comment tokens that are contained within a comment
            // token on the same line (e.g. setCount(...) inside {/* ... */}).
            var commentTokens = merged
                .Values.Where(t => t.TokenType == SemanticTokenTypes.Comment)
                .ToList();

            if (commentTokens.Count > 0)
            {
                var keysToRemove = new List<(int, int, int)>();
                foreach (var kvp in merged)
                {
                    var t = kvp.Value;
                    if (t.TokenType == SemanticTokenTypes.Comment)
                        continue;

                    foreach (var ct in commentTokens)
                    {
                        if (
                            ct.Line == t.Line
                            && t.Column >= ct.Column
                            && t.Column + t.Length <= ct.Column + ct.Length
                        )
                        {
                            keysToRemove.Add(kvp.Key);
                            break;
                        }
                    }
                }

                foreach (var key in keysToRemove)
                    merged.Remove(key);
            }

            return merged.Values;
        }

        // ── Function-style declaration ───────────────────────────────────────

        private static void CollectFunctionStyleDeclarationTokens(
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            int i = 0;
            while (i < source.Length && char.IsWhiteSpace(source[i]))
                i++;

            const string keyword = "component";
            if (
                i + keyword.Length > source.Length
                || !string.Equals(
                    source.Substring(i, keyword.Length),
                    keyword,
                    StringComparison.Ordinal
                )
            )
                return;

            var (line0, col0) = OffsetToLineCol0(lineStarts, i);
            EmitToken(tokens, line0, col0, keyword.Length, SemanticTokenTypes.Directive, s_noMods);

            int j = i + keyword.Length;
            while (j < source.Length && (source[j] == ' ' || source[j] == '\t'))
                j++;

            int nameStart = j;
            while (j < source.Length && (char.IsLetterOrDigit(source[j]) || source[j] == '_'))
                j++;

            if (j > nameStart)
            {
                var (nameLine0, nameCol0) = OffsetToLineCol0(lineStarts, nameStart);
                EmitToken(
                    tokens,
                    nameLine0,
                    nameCol0,
                    j - nameStart,
                    SemanticTokenTypes.DirectiveName,
                    s_noMods
                );
            }
        }

        // ── Setup code JSX tokens ────────────────────────────────────────────

        /// <summary>
        /// Parses JSX ranges embedded in function-style setup code (local
        /// functions, variable assignments) and collects semantic tokens for
        /// the markup inside them.
        /// </summary>
        private static void CollectSetupCodeJsxTokens(
            ParseResult parseResult,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            var d = parseResult.Directives;
            var allRanges = d.SetupCodeMarkupRanges;
            if (!d.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
            {
                allRanges = allRanges.IsDefaultOrEmpty
                    ? d.SetupCodeBareJsxRanges
                    : allRanges.AddRange(d.SetupCodeBareJsxRanges);
            }
            if (allRanges.IsDefaultOrEmpty)
                return;

            var diags = new List<ParseDiagnostic>();
            foreach (var (jsxStart, jsxEnd, jsxLine) in allRanges)
            {
                var jsxDirectives = d with
                {
                    MarkupStartIndex = jsxStart,
                    MarkupEndIndex = jsxEnd,
                    MarkupStartLine = jsxLine,
                };
                diags.Clear();
                var jsxNodes = UitkxParser.Parse(source, "", jsxDirectives, diags);
                foreach (var n in jsxNodes)
                    CollectNodeTokens(n, source, lineStarts, tokens, knownElements);
            }
        }

        // ── AST node walk ─────────────────────────────────────────────────────

        /// <summary>
        /// Emits semantic tokens for <paramref name="node"/>.
        /// </summary>
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
                    CollectBodyNodes(fe.Body, source, lineStarts, tokens, knownElements);
                    break;

                case ForNode fo:
                    EmitKeyword(tokens, source, lineStarts, fo.SourceLine, "@for");
                    CollectBodyNodes(fo.Body, source, lineStarts, tokens, knownElements);
                    break;

                case WhileNode wh:
                    EmitKeyword(tokens, source, lineStarts, wh.SourceLine, "@while");
                    CollectBodyNodes(wh.Body, source, lineStarts, tokens, knownElements);
                    break;

                case SwitchNode sw:
                    CollectSwitchTokens(sw, source, lineStarts, tokens, knownElements);
                    break;

                case ExpressionNode ex:
                    EmitKeyword(tokens, source, lineStarts, ex.SourceLine, "@(");
                    break;

                case CommentNode jc:
                    EmitCommentTokens(jc, source, lineStarts, tokens);
                    break;

                case TextNode _:
                    break; // plain text — no semantic tokens
            }
        }

        // ── Body-list traversal ────────────────────────────────────────────

        private static void CollectBodyNodes(
            IEnumerable<AstNode> body,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements
        )
        {
            foreach (var node in body)
                CollectNodeTokens(node, source, lineStarts, tokens, knownElements);
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
            var mods = s_noMods;

            // Open-tag name column (search for '<TagName', not '</TagName')
            int openNameCol = FindOpenTagName(source, lineStarts, el.SourceLine, el.TagName);
            if (openNameCol >= 0)
                EmitToken(
                    tokens,
                    el.SourceLine - 1,
                    openNameCol,
                    el.TagName.Length,
                    SemanticTokenTypes.Element,
                    mods
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
                        mods
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
                        mods
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

                CollectBodyNodes(branch.Body, source, lineStarts, tokens, knownElements);
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

                CollectBodyNodes(c.Body, source, lineStarts, tokens, knownElements);
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

        // ── @code body scanner ────────────────────────────────────────────────

        private static void EmitCommentTokens(
            CommentNode jc,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens
        )
        {
            // Determine the marker to search for in source based on comment kind
            string marker = jc.IsBlock ? "/*" : "//";
            int col = FindOnLine(source, lineStarts, jc.SourceLine, marker);
            if (col < 0)
                return;

            int startOffset = lineStarts[jc.SourceLine - 1] + col;

            if (!jc.IsBlock)
            {
                // Line comment — single token to end of line
                int lineEnd =
                    jc.SourceLine < lineStarts.Length ? lineStarts[jc.SourceLine] : source.Length;
                // Trim trailing newline chars
                while (
                    lineEnd > startOffset
                    && (source[lineEnd - 1] == '\r' || source[lineEnd - 1] == '\n')
                )
                    lineEnd--;
                int len = lineEnd - startOffset;
                if (len > 0)
                {
                    EmitToken(
                        tokens,
                        jc.SourceLine - 1,
                        col,
                        len,
                        SemanticTokenTypes.Comment,
                        s_noMods
                    );
                }
                return;
            }

            // Block comment — find closing */
            int closeAt = source.IndexOf("*/", startOffset + 2, StringComparison.Ordinal);
            if (closeAt < 0)
            {
                int fallbackLen = "/*".Length + jc.Content.Length + "*/".Length;
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

            int endExclusive = closeAt + 2;
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

        private static int OffsetToLine1(int[] lineStarts, int offset)
        {
            int idx = Array.BinarySearch(lineStarts, offset);
            if (idx < 0)
                idx = (~idx) - 1;
            if (idx < 0)
                idx = 0;
            return idx + 1;
        }

        private static (int Line0, int Col0) OffsetToLineCol0(int[] lineStarts, int offset)
        {
            int idx = Array.BinarySearch(lineStarts, offset);
            if (idx < 0)
                idx = (~idx) - 1;
            if (idx < 0)
                idx = 0;

            int col = offset - lineStarts[idx];
            if (col < 0)
                col = 0;

            return (idx, col);
        }

        private static bool IsLikelyEmbeddedMarkupLine(string segment)
        {
            string trimmed = segment.TrimStart();
            if (trimmed.Length == 0)
                return false;

            if (trimmed.StartsWith("<", StringComparison.Ordinal))
                return true;

            return trimmed.StartsWith("@if", StringComparison.Ordinal)
                || trimmed.StartsWith("@else", StringComparison.Ordinal)
                || trimmed.StartsWith("@for", StringComparison.Ordinal)
                || trimmed.StartsWith("@foreach", StringComparison.Ordinal)
                || trimmed.StartsWith("@while", StringComparison.Ordinal)
                || trimmed.StartsWith("@switch", StringComparison.Ordinal)
                || trimmed.StartsWith("@case", StringComparison.Ordinal)
                || trimmed.StartsWith("@default", StringComparison.Ordinal);
        }

        private static bool IsLikelyMarkupCloserLine(string trimmed)
        {
            if (trimmed.Length == 0)
                return false;

            return trimmed == "}"
                || trimmed == ")"
                || trimmed == ");"
                || trimmed.StartsWith("};", StringComparison.Ordinal)
                || trimmed.StartsWith(");", StringComparison.Ordinal)
                || trimmed.StartsWith("}", StringComparison.Ordinal)
                || trimmed.StartsWith(")", StringComparison.Ordinal);
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
