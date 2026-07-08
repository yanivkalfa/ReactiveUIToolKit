using System;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Canonical C# lexical facts shared by every consumer that needs to skip over string/char
    /// literals and comments while scanning raw setup/body code: <see cref="ExpressionExtractor"/>,
    /// <see cref="ReturnFinder"/>, <see cref="DirectiveParser"/>, <see cref="AstFormatter"/>, and
    /// (via the SourceGenerator/LSP projects) <c>CSharpEmitter</c> / <c>DiagnosticsPublisher</c>.
    ///
    /// This single implementation replaces six independently-drifted copies (see
    /// FINAL_AUDIT_UITKX_FINDINGS.md, finding U-20). The string/char-literal core is the
    /// "most complete" of the six — it is the only one of the originals that correctly recurses
    /// into interpolation holes (so a quoted <c>"}"</c> or a nested string/comment inside a
    /// <c>{...}</c> hole does not prematurely terminate the outer string), and the only one that
    /// recognized both the <c>@$"</c> and <c>$@"</c> interpolated-verbatim orderings symmetrically.
    /// </summary>
    public static class CSharpLexFacts
    {
        /// <summary>
        /// Skips a line comment, block comment, char literal, or string literal (regular,
        /// verbatim, interpolated, or interpolated-verbatim in either <c>@$"</c>/<c>$@"</c>
        /// ordering) starting at <paramref name="i"/>. Returns <c>true</c> and advances
        /// <paramref name="i"/> past the skipped span if one was found; returns <c>false</c>
        /// (leaving <paramref name="i"/> untouched) otherwise.
        /// </summary>
        public static bool TrySkipNonCode(string source, ref int i, int limit)
        {
            if (i >= limit)
                return false;

            char c0 = source[i];

            if (c0 == '/' && i + 1 < limit)
            {
                if (source[i + 1] == '/')
                {
                    i += 2;
                    while (i < limit && source[i] != '\n')
                        i++;
                    return true;
                }

                if (source[i + 1] == '*')
                {
                    int close = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = (close >= 0 && close + 2 <= limit) ? close + 2 : limit;
                    return true;
                }
            }

            return TrySkipStringOrCharLiteral(source, limit, ref i);
        }

        /// <summary>
        /// Skips a char literal <c>'x'</c> or a string literal (<c>""</c>, <c>@""</c>,
        /// <c>$""</c>, <c>$@""</c>, <c>@$""</c>) starting at <paramref name="i"/>, recursing into
        /// interpolation holes so nested strings/char-literals/comments inside a <c>{...}</c>
        /// hole are consumed as code rather than terminating the outer string early. Returns
        /// <c>false</c> (leaving <paramref name="i"/> untouched) if position <paramref name="i"/>
        /// is not the start of a char/string literal.
        /// </summary>
        public static bool TrySkipStringOrCharLiteral(string source, int rangeEnd, ref int i)
        {
            if (i >= rangeEnd)
                return false;
            char c0 = source[i];

            // ── Char literal '...' ─────────────────────────────────────────
            if (c0 == '\'')
            {
                int j = i + 1;
                while (j < rangeEnd)
                {
                    if (source[j] == '\\') { j += 2; continue; }
                    if (source[j] == '\'') { i = j + 1; return true; }
                    j++;
                }
                i = rangeEnd;
                return true;
            }

            // ── Detect string kind ─────────────────────────────────────────
            bool isVerbatim = false;
            bool isInterpolated = false;
            int quotePos = -1;

            if (c0 == '"')
            {
                quotePos = i;
            }
            else if (c0 == '$' && i + 1 < rangeEnd)
            {
                if (source[i + 1] == '"')
                {
                    isInterpolated = true;
                    quotePos = i + 1;
                }
                else if (source[i + 1] == '@' && i + 2 < rangeEnd && source[i + 2] == '"')
                {
                    // $@"..." (dollar-at ordering)
                    isInterpolated = true;
                    isVerbatim = true;
                    quotePos = i + 2;
                }
            }
            else if (c0 == '@' && i + 1 < rangeEnd)
            {
                if (source[i + 1] == '"')
                {
                    isVerbatim = true;
                    quotePos = i + 1;
                }
                else if (source[i + 1] == '$' && i + 2 < rangeEnd && source[i + 2] == '"')
                {
                    // @$"..." (at-dollar ordering)
                    isInterpolated = true;
                    isVerbatim = true;
                    quotePos = i + 2;
                }
            }

            if (quotePos < 0)
                return false;

            // ── Scan to end of string ──────────────────────────────────────
            int k = quotePos + 1;
            int braceDepth = 0;

            while (k < rangeEnd)
            {
                char ch = source[k];

                // Inside an interpolation hole — track braces, skip nested strings/comments.
                if (isInterpolated && braceDepth > 0)
                {
                    if (ch == '{') { braceDepth++; k++; continue; }
                    if (ch == '}') { braceDepth--; k++; continue; }
                    if (ch == '"' || ch == '\'' || ch == '$' || ch == '@')
                    {
                        if (TrySkipStringOrCharLiteral(source, rangeEnd, ref k))
                            continue;
                    }
                    if (ch == '/' && k + 1 < rangeEnd)
                    {
                        if (source[k + 1] == '/') { while (k < rangeEnd && source[k] != '\n') k++; continue; }
                        if (source[k + 1] == '*')
                        {
                            int ce = source.IndexOf("*/", k + 2, StringComparison.Ordinal);
                            k = ce >= 0 ? ce + 2 : rangeEnd;
                            continue;
                        }
                    }
                    k++;
                    continue;
                }

                // Inside string text (braceDepth == 0)
                if (isVerbatim)
                {
                    if (ch == '"')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '"')
                        { k += 2; continue; } // escaped ""
                        i = k + 1; return true; // end of string
                    }
                    if (isInterpolated && ch == '{')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '{')
                        { k += 2; continue; } // escaped {{
                        braceDepth++;
                    }
                    if (isInterpolated && ch == '}')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '}')
                        { k += 2; continue; } // escaped }}
                    }
                    k++;
                    continue;
                }

                // Regular or interpolated non-verbatim
                if (ch == '\\') { k += 2; continue; }
                if (ch == '"') { i = k + 1; return true; }
                if (isInterpolated && ch == '{')
                {
                    if (k + 1 < rangeEnd && source[k + 1] == '{')
                    { k += 2; continue; } // escaped {{
                    braceDepth++;
                }
                if (isInterpolated && ch == '}')
                {
                    if (k + 1 < rangeEnd && source[k + 1] == '}')
                    { k += 2; continue; } // escaped }}
                }
                k++;
            }

            // Unterminated — advance to end
            i = rangeEnd;
            return true;
        }

        /// <summary>
        /// Scans from <paramref name="start"/> (the position just after an opening
        /// <paramref name="openChar"/>) tracking nested <paramref name="openChar"/>/
        /// <paramref name="closeChar"/> depth while skipping string/char literals and comments
        /// via <see cref="TrySkipNonCode"/>. Returns the position OF the matching close
        /// character, or <paramref name="limit"/> if unmatched, with <paramref name="found"/>
        /// reporting which happened.
        /// </summary>
        public static int FindMatchingClose(string source, int start, int limit, char openChar, char closeChar, out bool found)
        {
            int depth = 1;
            int i = start;

            while (i < limit)
            {
                if (TrySkipNonCode(source, ref i, limit))
                    continue;

                char c = source[i];

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
                    {
                        found = true;
                        return i;
                    }
                    i++;
                    continue;
                }

                i++;
            }

            found = false;
            return limit;
        }

        /// <summary>Overload scanning to <c>source.Length</c>, discarding the found/unclosed flag.</summary>
        public static int FindMatchingClose(string source, int start, char openChar, char closeChar)
            => FindMatchingClose(source, start, source.Length, openChar, closeChar, out _);

        /// <summary>Returns the 0-based start offset of every line (index 0 = offset 0).</summary>
        public static int[] BuildLineStarts(string source)
        {
            int lineCount = 1;
            for (int i = 0; i < source.Length; i++)
                if (source[i] == '\n')
                    lineCount++;

            var starts = new int[lineCount];
            int line = 1;
            starts[0] = 0;
            for (int i = 0; i < source.Length; i++)
                if (source[i] == '\n' && line < lineCount)
                    starts[line++] = i + 1;

            return starts;
        }

        /// <summary>Binary-searches <paramref name="lineStarts"/> for the 0-based line index containing <paramref name="offset"/>.</summary>
        public static int LineIndexOf(int[] lineStarts, int offset)
        {
            int lo = 0, hi = lineStarts.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (lineStarts[mid] <= offset) lo = mid; else hi = mid - 1;
            }
            return lo;
        }

        /// <summary>Converts an absolute offset to a 1-based (line, column) pair.</summary>
        public static (int Line, int Col) OffsetToLineCol(int[] lineStarts, int offset)
        {
            int line = LineIndexOf(lineStarts, offset);
            int col = offset - lineStarts[line];
            return (line + 1, col + 1);
        }

        /// <summary>
        /// Finds the next occurrence of a hook-call pattern (e.g. <c>"useState("</c>) in
        /// <paramref name="code"/>, starting from <paramref name="start"/> and stopping at
        /// <paramref name="limit"/>, skipping over comments and string/char literals via
        /// <see cref="TrySkipNonCode"/>, and rejecting a match whose preceding character is
        /// part of a longer identifier (<c>_useState(</c>) or a member-access dot
        /// (<c>obj.useState(</c>) — both are false positives for a raw <c>IndexOf</c> scan.
        /// Returns -1 if no real match is found (see FINAL_AUDIT_UITKX_FINDINGS.md, U-10).
        /// </summary>
        public static int FindHookCall(string code, string pattern, int start, int limit)
        {
            int i = start;
            while (i < limit)
            {
                if (TrySkipNonCode(code, ref i, limit))
                    continue;

                if (i + pattern.Length <= limit
                    && string.CompareOrdinal(code, i, pattern, 0, pattern.Length) == 0)
                {
                    char prev = i > 0 ? code[i - 1] : '\0';
                    bool isBoundary = !(char.IsLetterOrDigit(prev) || prev == '_' || prev == '.');
                    if (isBoundary)
                        return i;
                }

                i++;
            }
            return -1;
        }

        /// <summary>
        /// Per line (index 0 = line 1, aligned with <see cref="BuildLineStarts"/>), whether that
        /// line's first character lies inside a verbatim/interpolated-verbatim string literal
        /// (<c>@"..."</c>, <c>@$"..."</c>, <c>$@"..."</c>) opened on an earlier line. Non-verbatim
        /// strings cannot contain a literal newline in valid C#, so only verbatim openings matter.
        /// Used by the formatter (U-03) to avoid re-indenting/trimming lines that are really
        /// inside a multi-line string VALUE.
        /// </summary>
        public static bool[] ComputeMultilineStringLineMask(string code)
        {
            int[] lineStarts = BuildLineStarts(code);
            var mask = new bool[lineStarts.Length];
            int i = 0;

            while (i < code.Length)
            {
                char c = code[i];
                bool isVerbatimStart =
                    (c == '@' && i + 1 < code.Length && code[i + 1] == '"')
                    || (c == '@' && i + 2 < code.Length && code[i + 1] == '$' && code[i + 2] == '"')
                    || (c == '$' && i + 2 < code.Length && code[i + 1] == '@' && code[i + 2] == '"');

                int start = i;
                if (TrySkipNonCode(code, ref i, code.Length))
                {
                    if (isVerbatimStart)
                    {
                        int startLine = LineIndexOf(lineStarts, start);
                        int endLine = LineIndexOf(lineStarts, i > start ? i - 1 : start);
                        for (int ln = startLine + 1; ln <= endLine && ln < mask.Length; ln++)
                            mask[ln] = true;
                    }
                    continue;
                }

                i++;
            }

            return mask;
        }

        /// <summary>
        /// Per line (index 0 = line 1, aligned with <see cref="BuildLineStarts"/>), whether
        /// that line lies ENTIRELY inside a <c>/* ... */</c> block comment opened on an
        /// earlier line and closed on a later line — i.e. the line has no code before the
        /// comment opens or after it closes, so it is pure comment content. The comment's
        /// own opening line (which may have real code before <c>/*</c>) and closing line
        /// (which may have real code after <c>*/</c>) are deliberately left unmasked, since
        /// masking them would hide real brace/paren characters from the caller's bookkeeping.
        /// Used by the formatter (U-07) so commented-out code (e.g. old removed JSX kept for
        /// reference) is re-emitted byte-verbatim instead of being re-indented as if live code.
        /// </summary>
        public static bool[] ComputeMultilineBlockCommentLineMask(string code)
        {
            int[] lineStarts = BuildLineStarts(code);
            var mask = new bool[lineStarts.Length];
            int i = 0;

            while (i < code.Length)
            {
                bool isBlockCommentStart = code[i] == '/' && i + 1 < code.Length && code[i + 1] == '*';

                int start = i;
                if (TrySkipNonCode(code, ref i, code.Length))
                {
                    if (isBlockCommentStart)
                    {
                        int startLine = LineIndexOf(lineStarts, start);
                        int endLine = LineIndexOf(lineStarts, i > start ? i - 1 : start);
                        // Strictly interior lines only — the open/close boundary lines may
                        // carry real code and must stay subject to normal bookkeeping.
                        for (int ln = startLine + 1; ln < endLine && ln < mask.Length; ln++)
                            mask[ln] = true;
                    }
                    continue;
                }

                i++;
            }

            return mask;
        }
    }
}
