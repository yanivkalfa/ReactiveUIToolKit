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

        /// <summary>
        /// Like <see cref="FromBrace"/> but also returns the absolute character offset inside
        /// <paramref name="source"/> where the trimmed expression content begins.
        /// Useful for building source maps after the expression is extracted.
        /// </summary>
        public static (string Expression, int AfterClose, int ContentOffset) FromBraceWithOffset(
            string source, int openBracePos)
            => FromBraceWithOffset(source, openBracePos, out _);

        /// <summary>
        /// Like <see cref="FromBraceWithOffset(string, int)"/> but also reports whether the
        /// closing <c>}</c> was actually found (U-28) — <paramref name="found"/> is
        /// <c>false</c> for a truncated/malformed file where the brace never closes, letting
        /// the caller emit an "unclosed '{' expression" diagnostic instead of silently
        /// treating end-of-file as the closing brace.
        /// </summary>
        public static (string Expression, int AfterClose, int ContentOffset) FromBraceWithOffset(
            string source, int openBracePos, out bool found)
        {
            int start = openBracePos + 1;
            int closePos = CSharpLexFacts.FindMatchingClose(source, start, source.Length, '{', '}', out found);
            int rawLen = closePos - start;
            string raw = rawLen > 0 ? source.Substring(start, rawLen) : string.Empty;
            int leading = CountLeadingWhitespace(raw);
            string expr = raw.Trim();
            int after = closePos < source.Length ? closePos + 1 : closePos;
            return (expr, after, start + leading);
        }

        /// <summary>
        /// Like <see cref="FromParen"/> but also returns the absolute character offset inside
        /// <paramref name="source"/> where the trimmed expression content begins.
        /// </summary>
        public static (string Expression, int AfterClose, int ContentOffset) FromParenWithOffset(
            string source, int openParenPos)
        {
            int start = openParenPos + 1;
            int closePos = FindMatchingClose(source, start, '(', ')');
            int rawLen = closePos - start;
            string raw = rawLen > 0 ? source.Substring(start, rawLen) : string.Empty;
            int leading = CountLeadingWhitespace(raw);
            string expr = raw.Trim();
            int after = closePos < source.Length ? closePos + 1 : closePos;
            return (expr, after, start + leading);
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
        ) => CSharpLexFacts.FindMatchingClose(source, start, openChar, closeChar);
        // ── Offset helpers ───────────────────────────────────────────────────

        /// <summary>Returns the number of leading whitespace characters (space, tab, CR, LF).</summary>
        private static int CountLeadingWhitespace(string s)
        {
            int i = 0;
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n'))
                i++;
            return i;
        }
    }
}
