using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly HashSet<string> s_topLevelKeywords =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "namespace", "component", "using", "props", "key",
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
            HashSet<string>? knownElements = null)
        {
            var tokens = new List<SemanticTokenData>(64);
            int[] lineStarts = BuildLineStarts(source);

            // 1. Directive block (every line before markup starts)
            CollectDirectiveTokens(parseResult.Directives, source, lineStarts, tokens);

            // 2. AST markup nodes
            foreach (var node in parseResult.RootNodes)
                CollectNodeTokens(node, source, lineStarts, tokens, knownElements);

            return tokens
                .OrderBy(t => t.Line)
                .ThenBy(t => t.Column)
                .ToArray();
        }

        // ── Directive block ───────────────────────────────────────────────────

        private static void CollectDirectiveTokens(
            DirectiveSet directives,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens)
        {
            // Lines 1..(MarkupStartLine-1) are directive lines (1-based).
            int markupLine1 = directives.MarkupStartLine;

            for (int line1 = 1; line1 < markupLine1; line1++)
            {
                int lineStart = lineStarts[line1 - 1];
                int lineEnd   = line1 < lineStarts.Length ? lineStarts[line1] : source.Length;
                int len       = lineEnd - lineStart;
                if (len <= 0) continue;

                // Build line text (stripped of trailing newline chars)
                string lineText = source.Substring(lineStart, len).TrimEnd('\r', '\n');

                // Find '@' on this line
                int atCol = lineText.IndexOf('@');
                if (atCol < 0) continue;

                // Read identifier after '@'
                int kwStart = atCol + 1;
                int kwEnd   = kwStart;
                while (kwEnd < lineText.Length && char.IsLetter(lineText[kwEnd]))
                    kwEnd++;

                string keyword = lineText.Substring(kwStart, kwEnd - kwStart);
                if (!s_topLevelKeywords.Contains(keyword)) continue;

                // Emit '@keyword' token
                int fullKwLen = kwEnd - atCol; // includes the '@'
                EmitToken(tokens, line1 - 1, atCol, fullKwLen,
                          SemanticTokenTypes.Directive, s_noMods);

                // Find and emit directive value
                int valStart = kwEnd;
                while (valStart < lineText.Length &&
                       (lineText[valStart] == ' ' || lineText[valStart] == '\t'))
                    valStart++;

                // Trim trailing whitespace
                int valEnd = lineText.Length;
                while (valEnd > valStart &&
                       (lineText[valEnd - 1] == ' ' || lineText[valEnd - 1] == '\t'))
                    valEnd--;

                if (valEnd > valStart)
                    EmitToken(tokens, line1 - 1, valStart, valEnd - valStart,
                              SemanticTokenTypes.DirectiveName, s_noMods);
            }
        }

        // ── AST node walk ─────────────────────────────────────────────────────

        private static void CollectNodeTokens(
            AstNode node,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements)
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
                    break;

                case ExpressionNode ex:
                    EmitKeyword(tokens, source, lineStarts, ex.SourceLine, "@(");
                    break;

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
            HashSet<string>? knownElements)
        {
            // Open-tag name column (search for '<TagName', not '</TagName')
            int openNameCol = FindOpenTagName(source, lineStarts, el.SourceLine, el.TagName);
            if (openNameCol >= 0)
                EmitToken(tokens, el.SourceLine - 1, openNameCol, el.TagName.Length,
                          SemanticTokenTypes.Element, s_noMods);

            // Attribute names (always emit — attributes on unknown elements are still valid)
            foreach (var attr in el.Attributes)
            {
                int attrCol = FindOnLine(source, lineStarts, attr.SourceLine, attr.Name);
                if (attrCol >= 0)
                    EmitToken(tokens, attr.SourceLine - 1, attrCol, attr.Name.Length,
                              SemanticTokenTypes.Attribute, s_noMods);
            }

            // Recurse into children
            foreach (var child in el.Children)
                CollectNodeTokens(child, source, lineStarts, tokens, knownElements);

            // Close-tag name (block elements only — self-closing have CloseTagLine == 0)
            if (el.CloseTagLine > 0)
            {
                int closeNameCol = FindCloseTagName(source, lineStarts, el.CloseTagLine, el.TagName);
                if (closeNameCol >= 0)
                    EmitToken(tokens, el.CloseTagLine - 1, closeNameCol, el.TagName.Length,
                              SemanticTokenTypes.Element, s_noMods);
            }
        }

        // ── @if / @else if / @else ────────────────────────────────────────────

        private static void CollectIfTokens(
            IfNode ifNode,
            string source,
            int[] lineStarts,
            List<SemanticTokenData> tokens,
            HashSet<string>? knownElements)
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
                            int ifCol = FindOnLine(source, lineStarts, branch.SourceLine,
                                                   "if", elseCol + "@else".Length);
                            if (ifCol >= 0)
                                EmitToken(tokens, branch.SourceLine - 1, ifCol, 2,
                                          SemanticTokenTypes.Directive, s_noMods);
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
            HashSet<string>? knownElements)
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

        // ── Position helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the 0-based column of the element tag name inside its
        /// open tag (<c>&lt;TagName</c>) on the given 1-based source line.
        /// Close tags (<c>&lt;/TagName&gt;</c>) are excluded.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindOpenTagName(
            string source, int[] lineStarts, int line1, string tagName)
        {
            // Search for '<TagName'. Close tags start with '</' so they won't
            // match because the char after '<' would be '/' not the tag name.
            int col = FindOnLine(source, lineStarts, line1, "<" + tagName);
            if (col < 0) return -1;
            return col + 1; // +1 to skip the '<' character
        }

        /// <summary>
        /// Returns the 0-based column of the tag name inside a close tag
        /// (<c>&lt;/TagName&gt;</c>) on the given 1-based source line.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindCloseTagName(
            string source, int[] lineStarts, int line1, string tagName)
        {
            int col = FindOnLine(source, lineStarts, line1, "</" + tagName);
            if (col < 0) return -1;
            return col + 2; // +2 to skip '</'
        }

        /// <summary>
        /// Returns the 0-based column of the first occurrence of
        /// <paramref name="search"/> on the given 1-based source line,
        /// starting at character offset <paramref name="startCol"/>.
        /// Returns -1 if not found.
        /// </summary>
        private static int FindOnLine(
            string source, int[] lineStarts, int line1, string search, int startCol = 0)
        {
            if (line1 < 1 || line1 > lineStarts.Length) return -1;

            int lineStart = lineStarts[line1 - 1];
            int lineEnd   = line1 < lineStarts.Length ? lineStarts[line1] : source.Length;
            int from      = lineStart + startCol;
            if (from >= lineEnd || from >= source.Length) return -1;

            int searchLen = lineEnd - from;
            if (searchLen <= 0) return -1;

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
            string keyword)
        {
            int col = FindOnLine(source, lineStarts, line1, keyword);
            if (col >= 0)
                EmitToken(tokens, line1 - 1, col, keyword.Length,
                          SemanticTokenTypes.Directive, s_noMods);
        }

        private static void EmitToken(
            List<SemanticTokenData> tokens,
            int line0,
            int col0,
            int length,
            string tokenType,
            string[] modifiers)
        {
            if (length <= 0) return;
            tokens.Add(new SemanticTokenData
            {
                Line      = line0,
                Column    = col0,
                Length    = length,
                TokenType = tokenType,
                Modifiers = modifiers,
            });
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
