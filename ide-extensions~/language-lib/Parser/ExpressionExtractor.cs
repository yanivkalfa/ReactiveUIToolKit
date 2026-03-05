using System;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Brace- and paren-balanced C# expression extractor.
    ///
    /// Given a source string and the position of an opening delimiter (<c>{</c> or
    /// <c>(</c>), extracts everything up to the matching closing delimiter,
    /// respecting:
    ///   • Regular string literals  <c>"..."</c>   (with <c>\"</c> escapes)
    ///   • Verbatim strings         <c>@"..."</c>  (with <c>""</c> escapes)
    ///   • Interpolated strings     <c>$"..."</c>  (with nested <c>{expr}</c> handling)
    ///   • Char literals            <c>'x'</c> / <c>'\n'</c>
    ///   • Line comments            <c>// ...</c>
    ///   • Block comments           <c>/* ... */</c>
    ///   • Nested braces / parens   (depth-tracked)
    ///
    /// The extractor never modifies the source — it only computes positions.
    /// </summary>
    public static class ExpressionExtractor
    {
        // ── Public entry points ───────────────────────────────────────────────

        /// <summary>
        /// Extracts a brace-delimited expression. The caller must ensure
        /// <c>source[openBracePos] == '{'</c>.
        /// </summary>
        /// <returns>
        /// The expression content without the surrounding braces, and the
        /// character index <em>after</em> the closing <c>}</c>.
        /// </returns>
        public static (string Expression, int AfterClose) FromBrace(string source, int openBracePos)
        {
            int start = openBracePos + 1;
            int closePos = FindMatchingClose(source, start, '{', '}');
            string expr = source.Substring(start, closePos - start).Trim();
            int after = closePos < source.Length ? closePos + 1 : closePos;
            return (expr, after);
        }

        /// <summary>
        /// Extracts a paren-delimited expression. The caller must ensure
        /// <c>source[openParenPos] == '('</c>.
        /// </summary>
        /// <returns>
        /// The expression content without the surrounding parens, and the
        /// character index <em>after</em> the closing <c>)</c>.
        /// </returns>
        public static (string Expression, int AfterClose) FromParen(string source, int openParenPos)
        {
            int start = openParenPos + 1;
            int closePos = FindMatchingClose(source, start, '(', ')');
            string expr = source.Substring(start, closePos - start).Trim();
            int after = closePos < source.Length ? closePos + 1 : closePos;
            return (expr, after);
        }

        // ── Core matching logic ───────────────────────────────────────────────

        /// <summary>
        /// Scans from <paramref name="start"/>, tracking open/close delimiter
        /// depth and skipping over string literals and comments.
        /// </summary>
        /// <returns>
        /// The position OF the matching <paramref name="closeChar"/>, or
        /// <c>source.Length</c> if unmatched (malformed input).
        /// </returns>
        internal static int FindMatchingClose(
            string source,
            int start,
            char openChar,
            char closeChar
        )
        {
            int depth = 1;
            int i = start;

            while (i < source.Length && depth > 0)
            {
                char c = source[i];

                // ── String / char literals and comments ──────────────────────

                if (c == '@' && i + 1 < source.Length)
                {
                    char next = source[i + 1];
                    if (next == '"')
                    {
                        // Verbatim string @"..."
                        i = SkipVerbatimString(source, i + 1);
                        continue;
                    }
                    if (next == '$' && i + 2 < source.Length && source[i + 2] == '"')
                    {
                        // @$"..." or $@"..." — same as interpolated verbatim
                        i = SkipInterpolatedVerbatimString(source, i);
                        continue;
                    }
                }

                if (c == '$' && i + 1 < source.Length && source[i + 1] == '"')
                {
                    // Interpolated string $"..."
                    i = SkipInterpolatedString(source, i);
                    continue;
                }

                if (c == '"')
                {
                    i = SkipRegularString(source, i);
                    continue;
                }

                if (c == '\'')
                {
                    i = SkipCharLiteral(source, i);
                    continue;
                }

                if (c == '/' && i + 1 < source.Length)
                {
                    if (source[i + 1] == '/')
                    {
                        // Line comment
                        i += 2;
                        while (i < source.Length && source[i] != '\n')
                            i++;
                        continue;
                    }
                    if (source[i + 1] == '*')
                    {
                        // Block comment
                        i += 2;
                        while (i + 1 < source.Length && !(source[i] == '*' && source[i + 1] == '/'))
                            i++;
                        if (i + 1 < source.Length)
                            i += 2;
                        continue;
                    }
                }

                // ── Depth tracking ────────────────────────────────────────────

                if (c == openChar)
                {
                    depth++;
                    i++;
                    continue;
                }
                if (c == closeChar)
                {
                    depth--;
                    if (depth == 0)
                        return i;
                    i++;
                    continue;
                }

                i++;
            }

            return source.Length; // unclosed
        }

        // ── String-skipping helpers ───────────────────────────────────────────

        // Returns index AFTER the closing '"'
        private static int SkipRegularString(string source, int openQuote)
        {
            int i = openQuote + 1;
            while (i < source.Length)
            {
                if (source[i] == '\\')
                {
                    i += 2;
                    continue;
                } // escaped char
                if (source[i] == '"')
                {
                    return i + 1;
                }
                i++;
            }
            return i;
        }

        // openQuote points at '"' (position after '@')
        // Returns index AFTER the closing '"'
        private static int SkipVerbatimString(string source, int openQuote)
        {
            int i = openQuote + 1;
            while (i < source.Length)
            {
                if (source[i] == '"')
                {
                    // "" inside verbatim string is an escaped quote, not end
                    if (i + 1 < source.Length && source[i + 1] == '"')
                    {
                        i += 2;
                        continue;
                    }
                    return i + 1;
                }
                i++;
            }
            return i;
        }

        // dollarPos points at '$' of $"..."
        // Returns index AFTER the closing '"'
        private static int SkipInterpolatedString(string source, int dollarPos)
        {
            int i = dollarPos + 2; // skip $"
            while (i < source.Length)
            {
                char c = source[i];
                if (c == '\\')
                {
                    i += 2;
                    continue;
                }
                if (c == '"')
                {
                    return i + 1;
                }
                if (c == '{')
                {
                    // {{ is an escaped brace, not an interpolation hole
                    if (i + 1 < source.Length && source[i + 1] == '{')
                    {
                        i += 2;
                        continue;
                    }
                    // Start of an interpolation hole — skip until matching '}'
                    int closePos = FindMatchingClose(source, i + 1, '{', '}');
                    i = closePos < source.Length ? closePos + 1 : closePos;
                    continue;
                }
                i++;
            }
            return i;
        }

        // For @$"..." or $@"..." (interpolated verbatim)
        // atPos points at '@' or '$'
        private static int SkipInterpolatedVerbatimString(string source, int atPos)
        {
            // Skip the two-character prefix (@$ or $@) plus the opening "
            int i = atPos + 3;
            while (i < source.Length)
            {
                char c = source[i];
                if (c == '"')
                {
                    // "" is an escaped quote inside verbatim; but in interpolated
                    // verbatim it's still an end unless doubled
                    if (i + 1 < source.Length && source[i + 1] == '"')
                    {
                        i += 2;
                        continue;
                    }
                    return i + 1;
                }
                if (c == '{')
                {
                    if (i + 1 < source.Length && source[i + 1] == '{')
                    {
                        i += 2;
                        continue;
                    }
                    int closePos = FindMatchingClose(source, i + 1, '{', '}');
                    i = closePos < source.Length ? closePos + 1 : closePos;
                    continue;
                }
                i++;
            }
            return i;
        }

        // pos points at '
        // Returns index AFTER the closing '
        private static int SkipCharLiteral(string source, int pos)
        {
            int i = pos + 1;
            if (i < source.Length && source[i] == '\\')
                i++; // escape prefix
            if (i < source.Length)
                i++; // the char itself
            if (i < source.Length && source[i] == '\'')
                i++; // closing '
            return i;
        }
    }
}
