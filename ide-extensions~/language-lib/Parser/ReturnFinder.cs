using System;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Shared utilities for finding <c>return (...)</c> statements and matching
    /// braces at the top level of a C# code region.  Used by both
    /// <see cref="DirectiveParser"/> (component-level return) and
    /// <see cref="UitkxParser"/> (control-block-level return).
    /// </summary>
    internal static class ReturnFinder
    {
        /// <summary>
        /// Scans <paramref name="source"/> between <paramref name="start"/> and
        /// <paramref name="endExclusive"/> for a top-level <c>return (...); </c>
        /// statement (outside nested braces / parens / brackets, skipping strings
        /// and comments).
        /// </summary>
        /// <returns><c>true</c> if found; positions are written to the out params.</returns>
        public static bool TryFindTopLevelReturn(
            string source,
            int start,
            int endExclusive,
            out int returnStart,
            out int openParen,
            out int closeParen,
            out int stmtEndExclusive,
            bool useLastReturn = true
        )
        {
            returnStart = -1;
            openParen = -1;
            closeParen = -1;
            stmtEndExclusive = -1;

            int i = start;
            int braceDepth = 0;
            int parenDepth = 0;
            int bracketDepth = 0;

            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                char c = source[i];

                if (c == '{') { braceDepth++; i++; continue; }
                if (c == '}') { if (braceDepth > 0) braceDepth--; i++; continue; }
                if (c == '(') { parenDepth++; i++; continue; }
                if (c == ')') { if (parenDepth > 0) parenDepth--; i++; continue; }
                if (c == '[') { bracketDepth++; i++; continue; }
                if (c == ']') { if (bracketDepth > 0) bracketDepth--; i++; continue; }

                if (braceDepth == 0 && parenDepth == 0 && bracketDepth == 0)
                {
                    if (TryReadKeywordAt(source, i, "return"))
                    {
                        int candidateStart = i;
                        int j = i + "return".Length;
                        SkipWhitespace(source, ref j);

                        if (j < endExclusive && source[j] == '(')
                        {
                            int candidateOpenParen = j;
                            if (TryReadBalancedParen(source, candidateOpenParen, endExclusive, out int closeParenExclusive))
                            {
                                int candidateCloseParen = closeParenExclusive - 1;
                                j = closeParenExclusive;
                                SkipWhitespace(source, ref j);
                                if (j < endExclusive && source[j] == ';')
                                {
                                    returnStart = candidateStart;
                                    openParen = candidateOpenParen;
                                    closeParen = candidateCloseParen;
                                    stmtEndExclusive = j + 1;

                                    if (!useLastReturn)
                                        return true;

                                    i = stmtEndExclusive;
                                    continue;
                                }
                            }
                        }

                        if (!useLastReturn)
                            return false;
                    }
                }

                i++;
            }

            return returnStart >= 0;
        }

        /// <summary>
        /// Given a <c>{</c> at <paramref name="openBracePos"/>, find the matching
        /// <c>}</c> while respecting nested braces, strings, and comments.
        /// Returns the position of the matching <c>}</c>, or <c>-1</c> if not found.
        /// </summary>
        public static int FindMatchingBrace(string source, int openBracePos, int endExclusive)
        {
            if (openBracePos < 0 || openBracePos >= source.Length || source[openBracePos] != '{')
                return -1;

            int i = openBracePos + 1;
            int depth = 1;

            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                char c = source[i];
                if (c == '{')
                {
                    depth++;
                    i++;
                    continue;
                }
                if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                    i++;
                    continue;
                }
                i++;
            }

            return -1;
        }

        /// <summary>
        /// Returns the 1-based line number of the character at <paramref name="pos"/>.
        /// </summary>
        public static int LineAtPos(string source, int pos)
        {
            int line = 1;
            for (int i = 0; i < pos && i < source.Length; i++)
                if (source[i] == '\n')
                    line++;
            return line;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Read a balanced <c>(...)</c> span, skipping strings, comments, and nested
        /// parens.  Returns <c>true</c> and sets <paramref name="closeExclusive"/>
        /// to the position just past the closing <c>)</c>.
        /// </summary>
        public static bool TryReadBalancedParen(
            string source,
            int openParenPos,
            int endExclusive,
            out int closeExclusive
        )
        {
            closeExclusive = -1;
            if (openParenPos < 0 || openParenPos >= source.Length || source[openParenPos] != '(')
                return false;

            int i = openParenPos + 1;
            int depth = 1;
            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                if (source[i] == '(')
                {
                    depth++;
                    i++;
                    continue;
                }

                if (source[i] == ')')
                {
                    depth--;
                    i++;
                    if (depth == 0)
                    {
                        closeExclusive = i;
                        return true;
                    }
                    continue;
                }

                i++;
            }

            return false;
        }

        private static bool TryReadKeywordAt(string source, int i, string keyword)
        {
            if (i < 0 || i + keyword.Length > source.Length)
                return false;

            if (!string.Equals(source.Substring(i, keyword.Length), keyword, StringComparison.Ordinal))
                return false;

            char prev = i > 0 ? source[i - 1] : '\0';
            char next = i + keyword.Length < source.Length ? source[i + keyword.Length] : '\0';
            bool prevOk = prev == '\0' || !(char.IsLetterOrDigit(prev) || prev == '_');
            bool nextOk = next == '\0' || !(char.IsLetterOrDigit(next) || next == '_');
            return prevOk && nextOk;
        }

        private static void SkipWhitespace(string source, ref int i)
        {
            while (i < source.Length && char.IsWhiteSpace(source[i]))
                i++;
        }

        /// <summary>
        /// Skips a non-code span (string literal, comment, char literal) starting
        /// at position <paramref name="i"/>.  Returns <c>true</c> if something was
        /// skipped (and <paramref name="i"/> was advanced), <c>false</c> otherwise.
        /// </summary>
        public static bool TrySkipNonCodeSpan(string source, ref int i, int limit)
        {
            if (i >= limit)
                return false;

            // Line comment //
            if (source[i] == '/' && i + 1 < limit)
            {
                if (source[i + 1] == '/')
                {
                    i += 2;
                    while (i < limit && source[i] != '\n')
                        i++;
                    return true;
                }
                // Block comment /* */
                if (source[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < limit && !(source[i] == '*' && source[i + 1] == '/'))
                        i++;
                    i = i + 1 < limit ? i + 2 : limit;
                    return true;
                }
            }

            // Char literal
            if (source[i] == '\'')
            {
                i++;
                while (i < limit)
                {
                    if (source[i] == '\\') { i += 2; continue; }
                    if (source[i] == '\'') { i++; break; }
                    i++;
                }
                return true;
            }

            // String literals: "", @"", $"", $@"", @$""
            int quotePos = -1;
            bool verbatim = false;

            if (source[i] == '"')
            {
                quotePos = i;
            }
            else if ((source[i] == '@' || source[i] == '$') && i + 1 < limit && source[i + 1] == '"')
            {
                quotePos = i + 1;
                verbatim = source[i] == '@';
            }
            else if (
                (source[i] == '@' || source[i] == '$')
                && i + 2 < limit
                && (source[i + 1] == '@' || source[i + 1] == '$')
                && source[i + 2] == '"'
            )
            {
                quotePos = i + 2;
                verbatim = source[i] == '@' || source[i + 1] == '@';
            }

            if (quotePos >= 0)
            {
                i = quotePos + 1;
                while (i < limit)
                {
                    if (verbatim)
                    {
                        if (source[i] == '"')
                        {
                            if (i + 1 < limit && source[i + 1] == '"')
                            {
                                i += 2;
                                continue;
                            }
                            i++;
                            break;
                        }
                        i++;
                        continue;
                    }

                    if (source[i] == '\\') { i += 2; continue; }
                    if (source[i] == '"') { i++; break; }
                    i++;
                }

                return true;
            }

            return false;
        }
    }
}
