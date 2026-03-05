using System;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Character-level scanner for UITKX markup source.
    ///
    /// The scanner is a stateful cursor over a <c>string</c> that the
    /// <see cref="UitkxParser"/> calls to read individual lexical constructs:
    /// tag names, attribute names, string literals, brace-delimited C# expressions,
    /// text content, etc.
    ///
    /// It tracks the current 1-based line number so that every AST node created by
    /// the parser carries accurate source location information for <c>#line</c>
    /// emission.
    ///
    /// The scanner does NOT produce a pre-built token list.  Instead the parser
    /// drives scanning one construct at a time based on its current grammatical
    /// context, which allows the same scanner to be used in both "markup mode" and
    /// "C# expression mode" without ambiguity.
    /// </summary>
    public sealed class MarkupTokenizer
    {
        private readonly string _source;
        private int _pos;
        private int _line;

        public MarkupTokenizer(string source, int startPos = 0, int startLine = 1)
        {
            _source = source ?? string.Empty;
            _pos = startPos;
            _line = startLine;
        }

        // ── Position and state ────────────────────────────────────────────────

        public int Pos => _pos;
        public int Line => _line;
        public bool IsEof => _pos >= _source.Length;

        /// <summary>Character at the current position, or <c>'\0'</c> at EOF.</summary>
        public char Current => _pos < _source.Length ? _source[_pos] : '\0';

        /// <summary>Peek ahead by <paramref name="offset"/> characters without advancing.</summary>
        public char Peek(int offset = 1)
        {
            int idx = _pos + offset;
            return idx < _source.Length ? _source[idx] : '\0';
        }

        // ── Advance / consume ─────────────────────────────────────────────────

        /// <summary>Advances the cursor by one character, tracking newlines.</summary>
        public void Advance()
        {
            if (_pos >= _source.Length)
                return;
            if (_source[_pos] == '\n')
                _line++;
            else if (_source[_pos] == '\r')
            {
                _line++;
                // If \r\n, skip both in one call
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '\n')
                    _pos++;
            }
            _pos++;
        }

        /// <summary>
        /// Tries to consume the character <paramref name="c"/> at the current
        /// position. Returns <c>true</c> and advances if matched; returns
        /// <c>false</c> and leaves the position unchanged otherwise.
        /// </summary>
        public bool TryConsume(char c)
        {
            if (_pos >= _source.Length || _source[_pos] != c)
                return false;
            Advance();
            return true;
        }

        /// <summary>
        /// Tries to consume the exact string <paramref name="s"/> starting at the
        /// current position. Returns <c>true</c> and advances past it if matched.
        /// Case-sensitive.
        /// </summary>
        public bool TryConsume(string s)
        {
            if (_pos + s.Length > _source.Length)
                return false;
            if (_source.IndexOf(s, _pos, s.Length, StringComparison.Ordinal) != _pos)
                return false;
            // Must advance char-by-char to track newlines
            for (int k = 0; k < s.Length; k++)
                Advance();
            return true;
        }

        /// <summary>Peek without consuming: is the next char <paramref name="c"/>?</summary>
        public bool IsAt(char c) => Current == c;

        /// <summary>Peek without consuming: does the source start with <paramref name="s"/> at the current position?</summary>
        public bool IsAt(string s)
        {
            if (_pos + s.Length > _source.Length)
                return false;
            return _source.IndexOf(s, _pos, s.Length, StringComparison.Ordinal) == _pos;
        }

        // ── Whitespace ────────────────────────────────────────────────────────

        /// <summary>Skips spaces and tabs (no newlines).</summary>
        public void SkipInlineWhitespace()
        {
            while (!IsEof && (Current == ' ' || Current == '\t'))
                Advance();
        }

        /// <summary>Skips spaces, tabs, and newlines.</summary>
        public void SkipWhitespaceAndNewlines()
        {
            while (
                !IsEof && (Current == ' ' || Current == '\t' || Current == '\r' || Current == '\n')
            )
                Advance();
        }

        // ── Structured reads ──────────────────────────────────────────────────

        /// <summary>
        /// Reads a tag name: a letter or underscore followed by letters, digits,
        /// or underscores.  E.g. <c>Box</c>, <c>label</c>, <c>MyComponent</c>.
        /// Returns an empty string if the current character is not a name-start.
        /// </summary>
        public string ReadTagName()
        {
            if (IsEof || !IsNameStart(Current))
                return string.Empty;
            int start = _pos;
            while (!IsEof && (char.IsLetterOrDigit(Current) || Current == '_'))
                Advance();
            return _source.Substring(start, _pos - start);
        }

        /// <summary>
        /// Reads an attribute name: letter or underscore start, continuing with
        /// letters, digits, underscores, hyphens, or dots.
        /// Hyphens are allowed for names like <c>data-id</c>.
        /// Returns an empty string if not at a name-start character.
        /// </summary>
        public string ReadAttrName()
        {
            if (IsEof || !IsNameStart(Current))
                return string.Empty;
            int start = _pos;
            while (
                !IsEof
                && (
                    char.IsLetterOrDigit(Current)
                    || Current == '_'
                    || Current == '-'
                    || Current == '.'
                )
            )
                Advance();
            return _source.Substring(start, _pos - start);
        }

        /// <summary>
        /// Reads a C# identifier or keyword (letters, digits, underscores; no hyphens).
        /// Used to read directive keywords after <c>@</c>.
        /// </summary>
        public string ReadIdentifier()
        {
            if (IsEof || (!char.IsLetter(Current) && Current != '_'))
                return string.Empty;
            int start = _pos;
            while (!IsEof && (char.IsLetterOrDigit(Current) || Current == '_'))
                Advance();
            return _source.Substring(start, _pos - start);
        }

        /// <summary>
        /// Reads a double-quoted string literal, consuming the surrounding quotes
        /// but returning only the content (no escape processing — the content is
        /// used as-is in generated code or as a literal value).
        /// Assumes <see cref="Current"/> == <c>"</c>.
        /// </summary>
        public string ReadStringLiteral()
        {
            if (IsEof || Current != '"')
                return string.Empty;
            Advance(); // opening "
            int start = _pos;
            while (!IsEof && Current != '"')
            {
                if (Current == '\\')
                    Advance(); // skip escaped char
                Advance();
            }
            int end = _pos;
            if (!IsEof)
                Advance(); // closing "
            return _source.Substring(start, end - start);
        }

        /// <summary>
        /// Reads a brace-delimited C# expression <c>{expr}</c>, advancing past
        /// both braces and returning the expression content (trimmed).
        /// Uses <see cref="ExpressionExtractor"/> for correct brace-depth tracking
        /// that is aware of string literals and comments.
        /// Assumes <see cref="Current"/> == <c>'{'</c>.
        /// </summary>
        public string ReadBraceExpression()
        {
            if (IsEof || Current != '{')
                return string.Empty;
            var (expr, afterClose) = ExpressionExtractor.FromBrace(_source, _pos);
            // Advance to afterClose, tracking newlines by counting them in the span
            AdvanceTo(afterClose);
            return expr;
        }

        /// <summary>
        /// Reads a paren-delimited expression <c>(expr)</c>, advancing past both
        /// parens and returning the expression content (trimmed).
        /// Assumes <see cref="Current"/> == <c>'('</c>.
        /// </summary>
        public string ReadParenExpression()
        {
            if (IsEof || Current != '(')
                return string.Empty;
            var (expr, afterClose) = ExpressionExtractor.FromParen(_source, _pos);
            AdvanceTo(afterClose);
            return expr;
        }

        /// <summary>
        /// Tries to skip an HTML comment <c>&lt;!-- ... --&gt;</c>.
        /// Returns <c>true</c> if an HTML comment was found and consumed.
        /// </summary>
        public bool TrySkipHtmlComment()
        {
            if (!IsAt("<!--"))
                return false;
            // Skip until -->
            while (!IsEof)
            {
                if (IsAt("-->"))
                {
                    TryConsume("-->");
                    return true;
                }
                Advance();
            }
            return true; // unclosed comment — consumed to EOF
        }

        /// <summary>
        /// Tries to skip a JSX-style comment <c>{/* ... */}</c> and returns
        /// the comment body in <paramref name="content"/>.
        /// Returns <c>true</c> if a JSX comment was found and consumed.
        /// Handles multi-line content and unclosed comments (consumed to EOF).
        /// </summary>
        public bool TrySkipJsxComment(out string content)
        {
            content = string.Empty;
            if (!IsAt("{/*"))
                return false;
            TryConsume("{/*");
            var sb = new System.Text.StringBuilder();
            while (!IsEof)
            {
                if (IsAt("*/}"))
                {
                    TryConsume("*/}");
                    content = sb.ToString();
                    return true;
                }
                sb.Append(Current);
                Advance();
            }
            content = sb.ToString(); // unclosed comment — content up to EOF
            return true;
        }

        /// <summary>
        /// Reads text content in markup context: everything up to the next
        /// <c>&lt;</c>, <c>@</c>, or EOF.  Returns <c>null</c> if there is no
        /// non-whitespace text before the next significant character (pure
        /// whitespace between elements is discarded).
        /// </summary>
        public string? ReadTextContent()
        {
            if (IsEof || Current == '<' || Current == '@')
                return null;

            int start = _pos;
            while (!IsEof && Current != '<' && Current != '@')
                Advance();

            string text = _source.Substring(start, _pos - start);
            // Collapse purely-whitespace text between elements
            string trimmed = text.Trim();
            return trimmed.Length == 0 ? null : text;
        }

        /// <summary>
        /// Reads to the end of the current line (does not consume the newline).
        /// </summary>
        public string ReadToEndOfLine()
        {
            int start = _pos;
            while (!IsEof && Current != '\r' && Current != '\n')
                Advance();
            return _source.Substring(start, _pos - start);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private static bool IsNameStart(char c) => char.IsLetter(c) || c == '_';

        /// <summary>
        /// Advances the position to <paramref name="targetPos"/>, counting
        /// newlines along the way so <see cref="Line"/> stays accurate.
        /// </summary>
        private void AdvanceTo(int targetPos)
        {
            while (_pos < targetPos && _pos < _source.Length)
                Advance();
        }
    }
}
