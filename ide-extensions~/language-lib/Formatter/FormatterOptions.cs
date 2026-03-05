using System;
using System.Text.RegularExpressions;

namespace ReactiveUITK.Language.Formatter
{
    /// <summary>
    /// All formatting options understood by <see cref="AstFormatter"/>.
    /// Defaults match the <c>uitkx.config.json</c> schema spec.
    /// </summary>
    public sealed record FormatterOptions
    {
        // ── Output layout ────────────────────────────────────────────────────

        /// <summary>Soft column limit. Long attribute lists wrap when a tag would exceed this.</summary>
        public int PrintWidth { get; init; } = 100;

        /// <summary>Number of spaces (or tab-width equivalent) per indent level.</summary>
        public int IndentSize { get; init; } = 4;

        /// <summary>When true, indent with hard tabs; otherwise use spaces.</summary>
        public bool UseTabIndent { get; init; } = false;

        // ── Attribute formatting ─────────────────────────────────────────────

        /// <summary>
        /// When true, force every attribute onto its own line even if they fit
        /// within <see cref="PrintWidth"/>.
        /// </summary>
        public bool SingleAttributePerLine { get; init; } = false;

        /// <summary>
        /// When false (default), the closing <c>&gt;</c> of a wrapped opening tag
        /// goes on its own line at the element's indent level.
        /// </summary>
        public bool BracketSameLine { get; init; } = false;

        /// <summary>
        /// Applies specifically to self-closing tags (<c>/&gt;</c>).
        /// When true (default), <c>/&gt;</c> stays on the last attribute line.
        /// </summary>
        public bool ClosingBracketSameLine { get; init; } = true;

        /// <summary>Emit <c>&lt;Foo /&gt;</c> (space before slash). Default true.</summary>
        public bool InsertSpaceBeforeSelfClose { get; init; } = true;

        // ── Blank line handling ──────────────────────────────────────────────

        /// <summary>
        /// When true, emit one blank line between root-level sibling nodes.
        /// Full per-blank-line preservation requires Phase 3 EndLine tracking.
        /// </summary>
        public bool PreserveBlankLines { get; init; } = true;

        /// <summary>
        /// Maximum consecutive blank lines to emit between siblings (when
        /// <see cref="PreserveBlankLines"/> is true).
        /// </summary>
        public int MaxConsecutiveBlankLines { get; init; } = 1;

        // ── Misc ─────────────────────────────────────────────────────────────

        /// <summary>Reserved for future prop-spread trailing comma support.</summary>
        public bool TrailingComma { get; init; } = false;

        // ── Singleton default ────────────────────────────────────────────────

        /// <summary>Immutable default options instance.</summary>
        public static FormatterOptions Default { get; } = new FormatterOptions();

        // ── JSON deserialisation ─────────────────────────────────────────────

        /// <summary>
        /// Parse the <c>"formatter"</c> section from a <c>uitkx.config.json</c> string.
        /// Unknown keys are silently ignored.  Returns <see cref="Default"/> on any
        /// parse failure.
        /// </summary>
        public static FormatterOptions FromJson(string json)
        {
            try
            {
                // Find the "formatter" { ... } section.
                var fmtBody = ExtractSection(json, "formatter");
                if (fmtBody is null)
                    return Default;

                var opts = Default;

                // Integers
                opts = opts with { PrintWidth              = ReadInt(fmtBody, "printWidth",              opts.PrintWidth) };
                opts = opts with { IndentSize              = ReadInt(fmtBody, "indentSize",              opts.IndentSize) };
                opts = opts with { MaxConsecutiveBlankLines = ReadInt(fmtBody, "maxConsecutiveBlankLines", opts.MaxConsecutiveBlankLines) };

                // Booleans
                opts = opts with { UseTabIndent              = ReadBoolOrString(fmtBody, "indentStyle", "tab") ?? opts.UseTabIndent };
                opts = opts with { TrailingComma             = ReadBool(fmtBody, "trailingComma",             opts.TrailingComma) };
                opts = opts with { BracketSameLine           = ReadBool(fmtBody, "bracketSameLine",           opts.BracketSameLine) };
                opts = opts with { SingleAttributePerLine    = ReadBool(fmtBody, "singleAttributePerLine",    opts.SingleAttributePerLine) };
                opts = opts with { ClosingBracketSameLine    = ReadBool(fmtBody, "closingBracketSameLine",    opts.ClosingBracketSameLine) };
                opts = opts with { PreserveBlankLines        = ReadBool(fmtBody, "preserveBlankLines",        opts.PreserveBlankLines) };
                opts = opts with { InsertSpaceBeforeSelfClose = ReadBool(fmtBody, "insertSpaceBeforeSelfClose", opts.InsertSpaceBeforeSelfClose) };

                return opts;
            }
            catch
            {
                return Default;
            }
        }

        // ── JSON helper methods ───────────────────────────────────────────────

        /// <summary>
        /// Extracts the raw JSON object body (without outer braces) for the
        /// given top-level key.  Does not handle nesting > 2 levels.
        /// </summary>
        private static string? ExtractSection(string json, string key)
        {
            // Match  "key"  :  {  ... }  at depth 1.
            var pattern = "\"" + Regex.Escape(key) + @"""\s*:\s*\{";
            var m = Regex.Match(json, pattern);
            if (!m.Success)
                return null;

            var start = m.Index + m.Length;
            var depth = 1;
            var i = start;
            while (i < json.Length && depth > 0)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}') depth--;
                if (depth > 0) i++;
                else break;
            }
            return depth == 0 ? json.Substring(start, i - start) : null;
        }

        private static int ReadInt(string body, string key, int fallback)
        {
            var m = Regex.Match(body, "\"" + Regex.Escape(key) + @"""\s*:\s*(\d+)");
            return m.Success && int.TryParse(m.Groups[1].Value, out var v) ? v : fallback;
        }

        private static bool ReadBool(string body, string key, bool fallback)
        {
            var m = Regex.Match(body, "\"" + Regex.Escape(key) + @"""\s*:\s*(true|false)");
            if (!m.Success) return fallback;
            return m.Groups[1].Value == "true";
        }

        /// <summary>
        /// Reads a string value and returns true when it equals <paramref name="trueValue"/>,
        /// false when it is some other known value, or null when the key is absent.
        /// Used for <c>"indentStyle": "tab" | "space"</c>.
        /// </summary>
        private static bool? ReadBoolOrString(string body, string key, string trueValue)
        {
            var m = Regex.Match(body, "\"" + Regex.Escape(key) + @"""\s*:\s*""([^""]*)""");
            if (!m.Success) return null;
            return string.Equals(m.Groups[1].Value, trueValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
