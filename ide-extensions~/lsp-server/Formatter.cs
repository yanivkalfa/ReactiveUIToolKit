using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UitkxLanguageServer;

/// <summary>
/// Formats a .uitkx document.
///
/// Architecture (Prettier-style):
///   Pass 1 â€” Tokenize: character-level scanner yields one logical token per
///             string, regardless of how the author wrote the source.
///             e.g.  "}<Label/>"  â†’  "}"  then  "<Label/>"
///                   multi-line attributes  â†’  one joined tag line
///   Pass 2 â€” Print: stack-based indenter consumes the token stream.
///             Every { pushes a block-kind; every } pops it so indentation
///             is always exact.
///
/// This two-pass approach means edge cases like }<Tag/>, multiline attrs,
/// extra whitespace, etc. are all normalised before indentation is applied.
/// </summary>
public static class UitkxFormatter
{
    private const string Indent = "    ";

    // â”€â”€ Pass 2 patterns (used only by the printer) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private static readonly Regex s_atLine = new(@"^@", RegexOptions.Compiled);
    private static readonly Regex s_blockOpen = new(@"\{\s*$", RegexOptions.Compiled);
    private static readonly Regex s_elseConn = new(@"^\}\s*@else\b", RegexOptions.Compiled);
    private static readonly Regex s_caseLabel = new(
        @"^@(case\b|default(\s*:)?\s*$)",
        RegexOptions.Compiled
    );
    private static readonly Regex s_switchOpen = new(@"^@switch\b", RegexOptions.Compiled);
    private static readonly Regex s_codeLine = new(@"^@code\b", RegexOptions.Compiled);

    private static readonly Regex s_tagPattern = new(
        @"^(<\/?[A-Za-z][A-Za-z0-9]*)(.*?)(\s*\/?>)\s*$",
        RegexOptions.Singleline | RegexOptions.Compiled
    );
    private static readonly Regex s_attrPattern = new(
        @"([A-Za-z][A-Za-z0-9\-_]*)(?:=""([^""]*)""|=(\{(?:[^{}]|\{[^{}]*\})*\})|(?=[\/\s>]))",
        RegexOptions.Compiled
    );

    private enum BlockKind
    {
        Directive,
        Switch,
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  PUBLIC API
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public static string Format(string source)
    {
        var tokens = Tokenize(source.ReplaceLineEndings("\n"));
        return Print(tokens);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  PASS 1 â€” TOKENIZER
    //  Character-level scanner.  Yields one logical token per string:
    //    ""              â†’ blank line
    //    "@if (x) {"     â†’ directive block opener
    //    "} @else {"     â†’ else-continuation (treated as one token)
    //    "}"             â†’ standalone close-brace
    //    "@case \"x\":"  â†’ case label
    //    "<Label/>"      â†’ self-closing tag (attrs collapsed to one line)
    //    "<Box>"         â†’ opening tag
    //    "</Box>"        â†’ closing tag
    //    "<!-- ... -->"  â†’ comment
    //    "@code {" +     â†’ code-block token (inner C# stored verbatim)
    //      inner lines
    //    "@namespace F"  â†’ plain directive line
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private static List<string> Tokenize(string src)
    {
        var tokens = new List<string>();
        var i = 0;
        var len = src.Length;

        while (i < len)
        {
            // â”€â”€ Horizontal whitespace: skip â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (src[i] is ' ' or '\t')
            {
                i++;
                continue;
            }

            // â”€â”€ Newline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (src[i] == '\n')
            {
                i++;
                // A blank line = \n then (optional spaces/tabs) then \n
                var j = i;
                while (j < len && src[j] is ' ' or '\t')
                    j++;
                if (j < len && src[j] == '\n')
                {
                    tokens.Add(""); // blank line marker
                    i = j; // the outer \n check will advance past it
                }
                continue;
            }

            // â”€â”€ } â€” close brace (may be "} @else ...") â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (src[i] == '}')
            {
                i++; // consume '}'
                // Skip spaces/tabs on the same line
                while (i < len && src[i] is ' ' or '\t')
                    i++;
                // If followed by @else, read the whole continuation as one token
                if (i < len && src[i] == '@')
                {
                    var peek = ReadIdent(src, i + 1, len);
                    if (peek == "else")
                    {
                        var line = "} " + ReadToEol(src, ref i).TrimStart();
                        tokens.Add(line);
                        continue;
                    }
                }
                // Plain }  â€” do NOT advance past EOL; more tokens may follow on the
                // same source line (e.g. "}<Label/>")
                tokens.Add("}");
                continue;
            }

            // â”€â”€ @ â€” directive â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (src[i] == '@')
            {
                var ident = ReadIdent(src, i + 1, len);

                if (ident == "code")
                {
                    // Read @code as an opaque block so its C# braces never
                    // confuse the directive-depth counter.
                    ReadCodeToken(src, ref i, len, tokens);
                    continue;
                }

                var line = ReadToEol(src, ref i).Trim();
                tokens.Add(line);
                continue;
            }

            // â”€â”€ < â€” tag or comment â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (src[i] == '<')
            {
                var tag = ReadTag(src, ref i, len);
                if (!string.IsNullOrWhiteSpace(tag))
                    tokens.Add(NormaliseTag(tag));
                continue;
            }

            // â”€â”€ Anything else: read to EOL (shouldn't normally occur) â”€â”€â”€â”€â”€â”€
            var stray = ReadToEol(src, ref i).Trim();
            if (stray.Length > 0)
                tokens.Add(stray);
        }

        return tokens;
    }

    // â”€â”€ Tokenizer helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// Reads from <paramref name="pos"/> to the end of the current line,
    /// advances <paramref name="pos"/> past the newline (if any) and returns
    /// the text (without the newline).
    private static string ReadToEol(string src, ref int pos)
    {
        var start = pos;
        while (pos < src.Length && src[pos] != '\n')
            pos++;
        var text = src[start..pos];
        if (pos < src.Length)
            pos++; // consume \n
        return text;
    }

    /// Returns the identifier starting at <paramref name="start"/> (does not advance pos).
    private static string ReadIdent(string src, int start, int len)
    {
        var end = start;
        while (end < len && (char.IsLetterOrDigit(src[end]) || src[end] == '_'))
            end++;
        return src[start..end];
    }

    /// Reads a complete XML tag (from '<' up to and including the matching '>'),
    /// joining continuation lines.  Handles {expr} nesting so '>' inside an
    /// expression doesn't terminate the tag early.
    private static string ReadTag(string src, ref int pos, int len)
    {
        var sb = new StringBuilder();
        var depth = 0; // { } brace depth inside attribute expressions
        var inStr = false;

        while (pos < len)
        {
            var c = src[pos];

            // Collapse newlines inside a multi-line tag to a space
            if (c == '\n')
            {
                sb.Append(' ');
                pos++;
                continue;
            }

            if (c == '"' && depth == 0)
            {
                inStr = !inStr;
                sb.Append(c);
                pos++;
                continue;
            }
            if (inStr)
            {
                sb.Append(c);
                pos++;
                continue;
            }

            if (c == '{')
                depth++;
            if (c == '}')
                depth--;

            sb.Append(c);
            pos++;

            if (depth == 0 && c == '>')
                break; // tag complete
        }

        return sb.ToString().Trim();
    }

    /// Reads the entire @code { ... } block.
    /// Emits:  "@code {"  followed by each re-indented C# line, then "}".
    private static void ReadCodeToken(string src, ref int pos, int len, List<string> tokens)
    {
        // pos is at '@', read to end of the @code ... { line
        var header = ReadToEol(src, ref pos).Trim(); // e.g. "@code {" or "@code\n"

        if (!header.EndsWith("{"))
        {
            // @code without a brace â€” just a plain directive line
            tokens.Add(header);
            return;
        }

        tokens.Add(header); // "@code {"

        // Read C# body until the matching closing }
        var braceDepth = 1;
        var lineStart = pos;

        while (pos < len && braceDepth > 0)
        {
            if (src[pos] == '\n')
            {
                var codeLine = src[lineStart..pos].TrimEnd();
                if (!string.IsNullOrWhiteSpace(codeLine))
                    tokens.Add("\x01" + codeLine.Trim()); // \x01 prefix = "code body line"
                pos++;
                lineStart = pos;
                continue;
            }

            // Track brace depth (ignoring strings â€” good enough for @code)
            if (src[pos] == '"')
            {
                pos++;
                while (pos < len && src[pos] != '"')
                    pos++;
            }
            else if (src[pos] == '{')
                braceDepth++;
            else if (src[pos] == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    // Flush any partial line before the closing }
                    var partial = src[lineStart..pos].Trim();
                    if (partial.Length > 0)
                        tokens.Add("\x01" + partial);
                    tokens.Add("}"); // the closing } of @code
                    pos++;
                    return;
                }
            }
            pos++;
        }
    }

    /// Collapses redundant whitespace inside a tag string (produced by joining
    /// multi-line attributes) and re-formats the attribute list onto one line.
    private static string NormaliseTag(string tag)
    {
        // Collapse internal whitespace runs (outside expressions) to single space
        var sb = new StringBuilder();
        var depth = 0;
        var inStr = false;
        var prev = ' ';

        foreach (var c in tag)
        {
            if (c == '"' && depth == 0)
            {
                inStr = !inStr;
                sb.Append(c);
                prev = c;
                continue;
            }
            if (inStr)
            {
                sb.Append(c);
                prev = c;
                continue;
            }
            if (c == '{')
                depth++;
            if (c == '}')
                depth--;
            if (depth == 0 && (c == ' ' || c == '\t' || c == '\n'))
            {
                if (prev != ' ')
                    sb.Append(' ');
                prev = ' ';
            }
            else
            {
                sb.Append(c);
                prev = c;
            }
        }

        return sb.ToString().Trim();
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  PASS 2 â€” PRINTER
    //  Stack-based indenter.  Each token gets exactly one correct indent level.
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private static string Print(List<string> tokens)
    {
        var result = new StringBuilder();
        var xmlDepth = 0;
        var dirDepth = 0;
        var inCase = false;
        var blockStack = new Stack<BlockKind>();

        foreach (var token in tokens)
        {
            // â”€â”€ Blank line â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (token.Length == 0)
            {
                result.AppendLine();
                continue;
            }

            var t = token;

            // â”€â”€ @code body passthrough (prefixed with \x01) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (t[0] == '\x01')
            {
                result.AppendLine(Pad(xmlDepth + 1) + t[1..]);
                continue;
            }

            // â”€â”€ @code opener â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (s_codeLine.IsMatch(t))
            {
                result.AppendLine(Pad(xmlDepth) + t);
                // @code body lines and closing } are separate tokens already;
                // nothing to push onto blockStack â€” @code's } is handled below
                // as a plain } that finds an empty stack and stays at xmlDepth.
                continue;
            }

            // â”€â”€ } @else / } @else if (single source token) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (s_elseConn.IsMatch(t))
            {
                if (blockStack.Count > 0)
                {
                    var kind = blockStack.Pop();
                    dirDepth--;
                    if (kind == BlockKind.Switch)
                        inCase = false;
                }
                result.AppendLine(Pad(xmlDepth + dirDepth + (inCase ? 1 : 0)) + t);
                if (s_blockOpen.IsMatch(t))
                {
                    blockStack.Push(BlockKind.Directive);
                    dirDepth++;
                }
                continue;
            }

            // â”€â”€ Standalone } â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (t == "}")
            {
                if (blockStack.Count > 0)
                {
                    var kind = blockStack.Pop();
                    dirDepth--;
                    if (kind == BlockKind.Switch)
                        inCase = false;
                }
                result.AppendLine(Pad(xmlDepth + dirDepth + (inCase ? 1 : 0)) + "}");
                continue;
            }

            // â”€â”€ @case / @default â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (s_caseLabel.IsMatch(t))
            {
                result.AppendLine(Pad(xmlDepth + dirDepth) + t);
                inCase = true;
                continue;
            }

            // â”€â”€ @ directives â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (s_atLine.IsMatch(t))
            {
                result.AppendLine(Pad(xmlDepth + dirDepth + (inCase ? 1 : 0)) + t);
                if (s_blockOpen.IsMatch(t))
                {
                    blockStack.Push(
                        s_switchOpen.IsMatch(t) ? BlockKind.Switch : BlockKind.Directive
                    );
                    dirDepth++;
                }
                continue;
            }

            // â”€â”€ Closing XML tag â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (t.StartsWith("</"))
            {
                xmlDepth = Math.Max(0, xmlDepth - 1);
                result.AppendLine(Pad(xmlDepth + dirDepth + (inCase ? 1 : 0)) + t);
                continue;
            }

            // â”€â”€ XML tag (<Foo/> or <Foo>) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (t.StartsWith("<") && !t.StartsWith("<!--"))
            {
                var total = xmlDepth + dirDepth + (inCase ? 1 : 0);
                result.AppendLine(FormatTag(t, total));
                if (!t.TrimEnd().EndsWith("/>"))
                    xmlDepth++;
                continue;
            }

            // â”€â”€ HTML comments and anything else â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            result.AppendLine(Pad(xmlDepth + dirDepth + (inCase ? 1 : 0)) + t);
        }

        return result.ToString().TrimEnd('\r', '\n') + "\n";
    }

    // â”€â”€ Tag formatter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static string FormatTag(string tag, int depth)
    {
        var m = s_tagPattern.Match(tag);
        if (!m.Success)
            return Pad(depth) + tag;

        var open = m.Groups[1].Value;
        var attrs = m.Groups[2].Value.Trim();
        var close = m.Groups[3].Value.TrimStart();

        var attrList = new List<string>();
        foreach (Match am in s_attrPattern.Matches(attrs))
        {
            var name = am.Groups[1].Value;
            if (am.Groups[3].Length > 0)
                attrList.Add($"{name}={am.Groups[3].Value}");
            else if (am.Groups[2].Length > 0)
                attrList.Add($"{name}=\"{am.Groups[2].Value}\"");
            else
                attrList.Add(name);
        }

        if (attrList.Count == 0)
            return Pad(depth) + open + close;

        return Pad(depth) + open + " " + string.Join(" ", attrList) + close;
    }

    private static string Pad(int n) => string.Concat(Enumerable.Repeat(Indent, Math.Max(0, n)));
}
