// ════════════════════════════════════════════════════════════════════════════
//  HmrStaticReadonlyStripper — HMR-side mirror of the SG's
//  StaticReadonlyStripper. Rewrites `static readonly` field declarations in
//  a `module { … }` body to plain `static` fields decorated with
//  `[global::ReactiveUITK.UitkxHmrSwap]`.
//
//  Why a hand-written scanner (and not Roslyn):
//    The Editor assembly does NOT directly reference Microsoft.CodeAnalysis —
//    Roslyn is loaded by UitkxHmrCompiler via reflection from the in-process
//    runtime, and only consumed indirectly. Reaching into Roslyn from this
//    code path would require plumbing every Roslyn API call through reflection
//    — a 200-line detour for a 20-line scan. The SG-side path already uses
//    real Roslyn (StaticReadonlyStripper) and produces canonical output;
//    this scanner only needs to handle the well-bounded user-authored module
//    body that the SG-side already accepts.
//
//  Strategy:
//    Single linear pass. Track depth (brace/paren/bracket) and whether we
//    are inside a string/char/comment. At depth==0 only, identify the start
//    of each statement and look forward through modifier keywords. If the
//    sequence of modifier tokens contains BOTH `static` AND `readonly` and
//    NOT `const`, rewrite the statement: drop the `readonly` token and
//    prepend `[global::ReactiveUITK.UitkxHmrSwap] `.
//
//  Edge-case policy (matches SG-side stripper):
//    • const            → untouched
//    • mutable static   → untouched
//    • static readonly  → strip + attribute
//    • nested types     → declarations inside nested `{ … }` blocks are
//                         depth > 0 and therefore skipped
//    • static auto-props → property has `{` after the identifier; field
//                         lookahead returns false in that case → untouched
//    • Comments / strings / verbatim strings / interpolated strings →
//                         skipped opaquely.
// ════════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using System.Text;

namespace ReactiveUITK.EditorSupport.HMR
{
    internal static class HmrStaticReadonlyStripper
    {
        public const string AttributePrefix = "[global::ReactiveUITK.UitkxHmrSwap] ";

        // C# field-modifier keywords. Any token in this set encountered at
        // statement start (depth 0) is considered part of the modifier prefix.
        private static readonly HashSet<string> s_modifierKeywords = new HashSet<string>
        {
            "public", "private", "protected", "internal",
            "static", "readonly", "volatile", "new", "unsafe",
            "const", "extern", "required", "file", "abstract", "virtual",
            "override", "sealed", "partial",
        };

        public static string Strip(string body)
        {
            if (string.IsNullOrEmpty(body))
                return body ?? string.Empty;

            // Fast-path: nothing to do if the body never says "readonly".
            if (body.IndexOf("readonly", System.StringComparison.Ordinal) < 0)
                return body;

            // Edits applied to the body, recorded as (offset, length, replacement)
            // and applied right-to-left at the end so earlier offsets stay valid.
            var edits = new List<(int Offset, int Length, string Replacement)>();

            int len = body.Length;
            int i = 0;
            int depthBrace = 0;
            int depthParen = 0;
            int depthBracket = 0;

            while (i < len)
            {
                while (i < len && char.IsWhiteSpace(body[i])) i++;
                if (i >= len) break;

                int afterOpaque = TrySkipOpaque(body, i);
                if (afterOpaque > i) { i = afterOpaque; continue; }

                char c = body[i];

                if (c == '{') { depthBrace++; i++; continue; }
                if (c == '}') { if (depthBrace > 0) depthBrace--; i++; continue; }
                if (c == '(') { depthParen++; i++; continue; }
                if (c == ')') { if (depthParen > 0) depthParen--; i++; continue; }
                if (c == '[') { depthBracket++; i++; continue; }
                if (c == ']') { if (depthBracket > 0) depthBracket--; i++; continue; }

                if (depthBrace != 0 || depthParen != 0 || depthBracket != 0)
                {
                    i++;
                    continue;
                }

                int stmtStart = i;
                var modifiers = TryReadModifiers(body, ref i);

                if (modifiers == null || modifiers.Count == 0)
                {
                    AdvanceOneToken(body, ref i);
                    continue;
                }

                bool hasStatic = false;
                bool hasReadonly = false;
                bool hasConst = false;
                (int Start, int End) readonlyTok = (0, 0);
                foreach (var m in modifiers)
                {
                    if (m.Text == "static") hasStatic = true;
                    else if (m.Text == "readonly") { hasReadonly = true; readonlyTok = (m.Start, m.End); }
                    else if (m.Text == "const") hasConst = true;
                }

                if (!hasStatic || !hasReadonly || hasConst)
                    continue;

                if (!IsFieldDeclarationAhead(body, i))
                    continue;

                int readonlyEnd = readonlyTok.End;
                int swallow = readonlyEnd;
                if (swallow < len && body[swallow] == ' ') swallow++;

                edits.Add((readonlyTok.Start, swallow - readonlyTok.Start, ""));
                edits.Add((stmtStart, 0, AttributePrefix));

                AdvanceToStatementEnd(body, ref i);
            }

            if (edits.Count == 0) return body;

            edits.Sort((a, b) => b.Offset.CompareTo(a.Offset));
            var sb = new StringBuilder(body);
            foreach (var e in edits)
            {
                if (e.Length > 0) sb.Remove(e.Offset, e.Length);
                if (!string.IsNullOrEmpty(e.Replacement)) sb.Insert(e.Offset, e.Replacement);
            }
            return sb.ToString();
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static int TrySkipOpaque(string body, int i)
        {
            int len = body.Length;
            if (i >= len) return i;
            char c = body[i];

            if (c == '/' && i + 1 < len)
            {
                if (body[i + 1] == '/')
                {
                    int j = i + 2;
                    while (j < len && body[j] != '\n') j++;
                    return j;
                }
                if (body[i + 1] == '*')
                {
                    int j = i + 2;
                    while (j + 1 < len && !(body[j] == '*' && body[j + 1] == '/')) j++;
                    return System.Math.Min(j + 2, len);
                }
            }
            if (c == '"') return SkipStringLiteral(body, i);
            if (c == '\'') return SkipCharLiteral(body, i);
            if (c == '@' && i + 1 < len && body[i + 1] == '"')
                return SkipVerbatimStringLiteral(body, i + 1);
            if (c == '$' && i + 1 < len && body[i + 1] == '"')
                return SkipStringLiteral(body, i + 1);
            return i;
        }

        private static int SkipStringLiteral(string s, int i)
        {
            int j = i + 1, len = s.Length;
            while (j < len)
            {
                char c = s[j];
                if (c == '\\' && j + 1 < len) { j += 2; continue; }
                if (c == '"') return j + 1;
                j++;
            }
            return len;
        }

        private static int SkipVerbatimStringLiteral(string s, int i)
        {
            int j = i + 1, len = s.Length;
            while (j < len)
            {
                char c = s[j];
                if (c == '"')
                {
                    if (j + 1 < len && s[j + 1] == '"') { j += 2; continue; }
                    return j + 1;
                }
                j++;
            }
            return len;
        }

        private static int SkipCharLiteral(string s, int i)
        {
            int j = i + 1, len = s.Length;
            while (j < len)
            {
                char c = s[j];
                if (c == '\\' && j + 1 < len) { j += 2; continue; }
                if (c == '\'') return j + 1;
                j++;
            }
            return len;
        }

        private static List<(int Start, int End, string Text)> TryReadModifiers(
            string body, ref int i)
        {
            List<(int, int, string)> result = null;
            int len = body.Length;
            while (i < len)
            {
                int probe = i;
                while (probe < len && char.IsWhiteSpace(body[probe])) probe++;
                int afterOpaque = TrySkipOpaque(body, probe);
                if (afterOpaque > probe) { i = afterOpaque; continue; }

                if (!IsIdentStart(body, probe)) { i = probe; break; }

                int end = probe;
                while (end < len && IsIdentChar(body[end])) end++;
                string word = body.Substring(probe, end - probe);
                if (!s_modifierKeywords.Contains(word))
                {
                    i = probe;
                    break;
                }

                if (result == null) result = new List<(int, int, string)>(4);
                result.Add((probe, end, word));
                i = end;
            }
            return result;
        }

        private static bool IsFieldDeclarationAhead(string body, int i)
        {
            int len = body.Length;
            int depthAngle = 0;
            int depthParenLocal = 0;
            int depthBracketLocal = 0;
            bool sawIdentifier = false;

            while (i < len)
            {
                int probe = i;
                while (probe < len && char.IsWhiteSpace(body[probe])) probe++;
                int afterOpaque = TrySkipOpaque(body, probe);
                if (afterOpaque > probe) { i = afterOpaque; continue; }
                i = probe;
                if (i >= len) return false;

                char c = body[i];
                if (c == '<') { depthAngle++; i++; continue; }
                if (c == '>') { if (depthAngle > 0) depthAngle--; i++; continue; }
                if (c == '(') { depthParenLocal++; i++; continue; }
                if (c == ')') { if (depthParenLocal > 0) depthParenLocal--; i++; continue; }
                if (c == '[') { depthBracketLocal++; i++; continue; }
                if (c == ']') { if (depthBracketLocal > 0) depthBracketLocal--; i++; continue; }

                if (depthAngle > 0 || depthParenLocal > 0 || depthBracketLocal > 0)
                {
                    i++;
                    continue;
                }

                if (c == '{') return false;
                if (c == '=' || c == ';' || c == ',') return sawIdentifier;
                if (c == '(') return false;

                if (IsIdentStart(body, i))
                {
                    int e = i;
                    while (e < len && IsIdentChar(body[e])) e++;
                    sawIdentifier = true;
                    i = e;
                    continue;
                }

                i++;
            }
            return false;
        }

        private static void AdvanceOneToken(string body, ref int i)
        {
            int len = body.Length;
            if (i >= len) return;
            if (IsIdentStart(body, i))
            {
                while (i < len && IsIdentChar(body[i])) i++;
                return;
            }
            i++;
        }

        private static void AdvanceToStatementEnd(string body, ref int i)
        {
            int len = body.Length;
            int depthB = 0, depthP = 0, depthBr = 0;
            while (i < len)
            {
                int afterOpaque = TrySkipOpaque(body, i);
                if (afterOpaque > i) { i = afterOpaque; continue; }
                char c = body[i];
                if (c == '{') depthB++;
                else if (c == '}') { if (depthB > 0) depthB--; }
                else if (c == '(') depthP++;
                else if (c == ')') { if (depthP > 0) depthP--; }
                else if (c == '[') depthBr++;
                else if (c == ']') { if (depthBr > 0) depthBr--; }
                else if (c == ';' && depthB == 0 && depthP == 0 && depthBr == 0)
                {
                    i++;
                    return;
                }
                i++;
            }
        }

        private static bool IsIdentStart(string s, int i)
        {
            if (i >= s.Length) return false;
            char c = s[i];
            return c == '_' || char.IsLetter(c);
        }

        private static bool IsIdentChar(char c)
        {
            return c == '_' || char.IsLetterOrDigit(c);
        }
    }
}
