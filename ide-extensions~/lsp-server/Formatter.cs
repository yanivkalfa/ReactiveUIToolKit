using System.Text;
using System.Text.RegularExpressions;

namespace UitkxLanguageServer;

/// <summary>
/// Formats a .uitkx document: consistent indentation, one attribute per line
/// when there are multiple, normalised quoting.
/// </summary>
public static class UitkxFormatter
{
    private const string Indent = "    "; // 4 spaces — matches C# convention
    private static readonly Regex s_directiveLine = new(@"^@", RegexOptions.Compiled);
    private static readonly Regex s_directiveBlockOpen = new(@"^@.*\{\s*$", RegexOptions.Compiled);

    public static string Format(string source)
    {
        var rawLines = source.ReplaceLineEndings("\n").Split('\n');
        // Step 1: join attributes that were broken across lines into one tag per line.
        // Step 2: split any line that contains multiple tags (e.g. <Box><Label/></Box>).
        var lines  = JoinMultiLineTags(rawLines).SelectMany(SplitTagsOnLine);
        var result = new StringBuilder();
        var depth = 0;
        var directiveDepth = 0;
        var codeDepth = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            // ── Blank lines pass through ──────────────────────────────────
            if (string.IsNullOrWhiteSpace(line))
            {
                result.AppendLine();
                continue;
            }

            var trimmed = line.TrimStart();

            // ── Close brace for directive/code blocks ─────────────────────
            if (trimmed == "}")
            {
                if (codeDepth > 0)
                {
                    codeDepth--;
                    result.AppendLine(Pad(depth + directiveDepth + codeDepth) + trimmed);
                }
                else if (directiveDepth > 0)
                {
                    directiveDepth--;
                    result.AppendLine(Pad(depth + directiveDepth) + trimmed);
                }
                else
                {
                    result.AppendLine(Pad(depth) + trimmed);
                }
                continue;
            }

            // ── Directives & control flow (@namespace, @if, etc.) ─────────
            if (s_directiveLine.IsMatch(trimmed))
            {
                result.AppendLine(Pad(depth + directiveDepth) + trimmed);

                if (s_directiveBlockOpen.IsMatch(trimmed))
                    directiveDepth++;

                continue;
            }

            // ── Closing tag </foo> → dedent before printing ───────────────
            if (trimmed.StartsWith("</"))
            {
                depth = Math.Max(0, depth - 1);
                result.AppendLine(Pad(depth + directiveDepth) + trimmed);
                continue;
            }

            // ── Self-closing or opening tag ───────────────────────────────
            if (trimmed.StartsWith("<") && !trimmed.StartsWith("<!--"))
            {
                var formatted = FormatTag(trimmed, depth + directiveDepth);
                result.AppendLine(formatted);

                // Increase depth after an opening tag (not self-closing, not </…>)
                if (!trimmed.EndsWith("/>") && !trimmed.StartsWith("</"))
                    depth++;
                continue;
            }

            // ── HTML comments ─────────────────────────────────────────────
            if (trimmed.StartsWith("<!--"))
            {
                result.AppendLine(Pad(depth + directiveDepth) + trimmed);
                continue;
            }

            // ── C# style non-markup lines inside directive/code blocks ─────
            if (trimmed.StartsWith("}"))
                codeDepth = Math.Max(0, codeDepth - 1);

            result.AppendLine(Pad(depth + directiveDepth + codeDepth) + trimmed);

            if (directiveDepth > 0)
                codeDepth += CountCodeBlockOpeners(trimmed);
        }

        return result.ToString().TrimEnd('\r', '\n') + "\n";
    }

    // ── Tag formatter ────────────────────────────────────────────────────────

    private static readonly Regex s_tagPattern = new(
        @"^(<\/?[A-Za-z][A-Za-z0-9]*)(.*?)(\s*\/?>)\s*$",
        RegexOptions.Singleline);

    private static readonly Regex s_attrPattern = new(
        // Matches:  name="value"  name={expr}  name  (boolean shorthand)
        @"([A-Za-z][A-Za-z0-9\-_]*)(?:=""([^""]*)""|=(\{(?:[^{}]|\{[^{}]*\})*\})|(?=[\/\s>]))",
        RegexOptions.Compiled);

    private static string FormatTag(string tag, int depth)
    {
        var m = s_tagPattern.Match(tag);
        if (!m.Success) return Pad(depth) + tag;

        var open  = m.Groups[1].Value;   // e.g. "<button"
        var attrs = m.Groups[2].Value.Trim();
        var close = m.Groups[3].Value.TrimStart();  // "/>" or ">"

        // Collect attributes
        var attrList = new List<string>();
        foreach (Match am in s_attrPattern.Matches(attrs))
        {
            var name = am.Groups[1].Value;
            if (am.Groups[3].Value.Length > 0)          // expression value
                attrList.Add($"{name}={am.Groups[3].Value}");
            else if (am.Groups[2].Value.Length > 0)     // string value
                attrList.Add($"{name}=\"{am.Groups[2].Value}\"");
            else                                         // boolean shorthand
                attrList.Add(name);
        }

        if (attrList.Count == 0)
            return Pad(depth) + open + close;

        // Single attribute → same line
        if (attrList.Count == 1)
            return Pad(depth) + open + " " + attrList[0] + close;

        // Multiple attributes → one per line
        var sb = new StringBuilder();
        sb.AppendLine(Pad(depth) + open);
        var attrIndent = Pad(depth + 1);
        for (var i = 0; i < attrList.Count; i++)
        {
            if (i < attrList.Count - 1)
                sb.AppendLine(attrIndent + attrList[i]);
            else
                sb.Append(attrIndent + attrList[i] + close);
        }
        return sb.ToString();
    }

    private static string Pad(int depth) => string.Concat(Enumerable.Repeat(Indent, depth));

    private static int CountCodeBlockOpeners(string line)
    {
        var opens = 0;
        var closes = 0;
        var inString = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inString = !inString;
                continue;
            }

            if (inString)
                continue;

            if (c == '{') opens++;
            if (c == '}') closes++;
        }

        return Math.Max(0, opens - closes);
    }

    // ── Multi-line tag joiner ────────────────────────────────────────────────

    /// <summary>
    /// Joins tag fragments that span multiple lines (e.g. attributes on separate lines)
    /// into a single line before the main formatting pass runs.
    /// </summary>
    private static IEnumerable<string> JoinMultiLineTags(string[] lines)
    {
        var result = new List<string>(lines.Length);
        var i      = 0;

        while (i < lines.Length)
        {
            var line    = lines[i].TrimEnd();
            var trimmed = line.TrimStart();

            // Only try to join an opening tag that has no closing > yet
            if (
                trimmed.StartsWith("<")
                && !trimmed.StartsWith("</")
                && !trimmed.StartsWith("<!--")
                && !HasTagClose(trimmed)
            )
            {
                var sb = new StringBuilder(trimmed);
                i++;
                while (i < lines.Length)
                {
                    var next = lines[i].Trim();
                    sb.Append(' ');
                    sb.Append(next);
                    i++;
                    if (HasTagClose(next))
                        break;
                }
                result.Add(sb.ToString());
            }
            else
            {
                result.Add(line);
                i++;
            }
        }

        return result;
    }

    // ── Compound-line splitter ───────────────────────────────────────────────

    /// <summary>
    /// Splits a line that contains multiple tags (e.g. <c>&lt;Box&gt;&lt;Label/&gt;&lt;/Box&gt;</c>)
    /// into one tag per line. Non-markup lines pass through unchanged.
    /// Leading whitespace is stripped — the main formatting pass re-indents everything.
    /// </summary>
    private static IEnumerable<string> SplitTagsOnLine(string line)
    {
        var trimmed = line.TrimStart();

        // Only split markup lines
        if (!trimmed.StartsWith("<") || trimmed.StartsWith("<!--"))
        {
            yield return line;
            yield break;
        }

        var i = 0;
        while (i < trimmed.Length)
        {
            if (trimmed[i] == '<')
            {
                var start  = i;
                var depth  = 0;
                var inStr  = false;
                i++;
                while (i < trimmed.Length)
                {
                    var c = trimmed[i];
                    if (c == '"' && depth == 0) { inStr = !inStr; i++; continue; }
                    if (inStr)                  { i++; continue; }
                    if (c == '{')               { depth++; i++; continue; }
                    if (c == '}')               { depth--; i++; continue; }
                    if (depth == 0 && c == '>')  { i++; break; }
                    i++;
                }
                yield return trimmed[start..i];
            }
            else if (!char.IsWhiteSpace(trimmed[i]))
            {
                // Non-tag text — gather until next '<' or end
                var start = i;
                while (i < trimmed.Length && trimmed[i] != '<')
                    i++;
                yield return trimmed[start..i].Trim();
            }
            else
            {
                i++;
            }
        }
    }

    /// <summary>
    /// Returns true when <paramref name="s"/> contains a tag-closing <c>&gt;</c> that is
    /// not inside a curly-brace expression or a double-quoted string.
    /// </summary>
    private static bool HasTagClose(string s)
    {
        var depth  = 0;
        var inStr  = false;

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '"' && depth == 0) { inStr = !inStr; continue; }
            if (inStr) continue;
            if (c == '{') { depth++; continue; }
            if (c == '}') { depth--; continue; }
            if (depth == 0 && c == '>') return true;
        }

        return false;
    }
}
