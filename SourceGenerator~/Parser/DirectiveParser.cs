using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace ReactiveUITK.SourceGenerator.Parser
{
    /// <summary>
    /// Extracts the top-level <c>@directive value</c> lines from the beginning of a
    /// .uitkx source file and validates that the required directives are present.
    ///
    /// The directive block consists of consecutive lines of the form:
    /// <code>
    ///   @namespace  MyGame.UI
    ///   @component  PlayerHUD
    ///   @using      MyGame.Models
    ///   @using      System.Collections.Generic
    ///   @props      PlayerHUDProps
    ///   @key        "hud-root"
    /// </code>
    ///
    /// The block ends at the first line that is not blank and does not start with
    /// one of the recognised top-level directive keywords.  Any <c>@</c> word that
    /// is a markup/control-flow keyword (<c>@if</c>, <c>@foreach</c>, etc.) is
    /// treated as the start of markup and therefore ends the directive block.
    ///
    /// The returned <see cref="DirectiveSet"/> also carries the character index and
    /// 1-based line number where the markup begins, which the parser reads from.
    /// </summary>
    internal static class DirectiveParser
    {
        private static readonly HashSet<string> s_topLevelKeywords = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            "namespace",
            "component",
            "using",
            "props",
            "key",
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Parses all top-level directives from <paramref name="source"/> and
        /// appends any validation diagnostics to <paramref name="diagnosticBag"/>.
        /// </summary>
        public static DirectiveSet Parse(
            string source,
            string filePath,
            List<Diagnostic> diagnosticBag
        )
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string? ns = null;
            string? component = null;
            string? props = null;
            string? key = null;
            var usings = new List<string>();
            int nsLine = -1;
            int componentLine = -1;

            int i = 0;
            int line = 1;
            int markupStart = 0;
            int markupLine = 1;

            while (i < source.Length)
            {
                // ── Skip leading whitespace on the line (spaces/tabs only) ────
                int lineStart = i;
                while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                    i++;

                // ── Skip blank lines ─────────────────────────────────────────
                if (i < source.Length && IsNewline(source[i]))
                {
                    ConsumeNewline(source, ref i, ref line);
                    continue;
                }

                if (i >= source.Length)
                {
                    markupStart = i;
                    markupLine = line;
                    break;
                }

                // ── Must start with '@' to be a directive ────────────────────
                if (source[i] != '@')
                {
                    markupStart = lineStart;
                    markupLine = line;
                    break;
                }

                i++; // consume '@'

                // Read the keyword after '@'
                int keywordStart = i;
                while (i < source.Length && char.IsLetter(source[i]))
                    i++;
                string keyword = source
                    .Substring(keywordStart, i - keywordStart)
                    .ToLowerInvariant();

                // If this is NOT a recognised top-level directive keyword, treat
                // this line (starting at '@') as the beginning of the markup.
                if (!s_topLevelKeywords.Contains(keyword))
                {
                    markupStart = lineStart;
                    markupLine = line;
                    break;
                }

                // ── Skip whitespace between keyword and value ─────────────────
                while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                    i++;

                // ── Read the value until end of line ──────────────────────────
                int valueStart = i;
                while (i < source.Length && !IsNewline(source[i]))
                    i++;
                string value = source.Substring(valueStart, i - valueStart).Trim();

                // ── Consume the newline ───────────────────────────────────────
                ConsumeNewline(source, ref i, ref line);

                // ── Store ─────────────────────────────────────────────────────
                switch (keyword)
                {
                    case "namespace":
                        ns = value;
                        nsLine = line;
                        break;
                    case "component":
                        component = value;
                        componentLine = line;
                        break;
                    case "props":
                        props = value;
                        break;
                    case "key":
                        key = value.Trim('"');
                        break;
                    case "using":
                        if (!string.IsNullOrEmpty(value))
                            usings.Add(value);
                        break;
                }
            }

            // If we consumed the entire file without finding markup
            if (i >= source.Length && markupStart == 0 && markupLine == 1)
            {
                markupStart = source.Length;
                markupLine = line;
            }

            // ── Validate required directives ──────────────────────────────────
            var fileLoc = Location.Create(filePath, default, default);
            string shortName = Path.GetFileName(filePath);

            if (ns == null)
                diagnosticBag.Add(
                    Diagnostic.Create(
                        UitkxDiagnostics.MissingRequiredDirective,
                        fileLoc,
                        shortName,
                        "namespace"
                    )
                );

            if (component == null)
                diagnosticBag.Add(
                    Diagnostic.Create(
                        UitkxDiagnostics.MissingRequiredDirective,
                        fileLoc,
                        shortName,
                        "component"
                    )
                );

            if (
                component != null
                && fileName != null
                && !string.Equals(component, fileName, StringComparison.Ordinal)
            )
            {
                diagnosticBag.Add(
                    Diagnostic.Create(
                        UitkxDiagnostics.ComponentNameMismatch,
                        fileLoc,
                        component,
                        fileName
                    )
                );
            }

            // UITKX0012 — @namespace must be declared before @component
            if (ns != null && component != null && nsLine > componentLine)
            {
                diagnosticBag.Add(
                    Diagnostic.Create(UitkxDiagnostics.DirectiveOrderError, fileLoc, shortName)
                );
            }

            return new DirectiveSet(
                Namespace: ns,
                ComponentName: component,
                PropsTypeName: props,
                DefaultKey: key,
                Usings: usings.ToImmutableArray(),
                MarkupStartLine: markupLine,
                MarkupStartIndex: markupStart
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool IsNewline(char c) => c == '\r' || c == '\n';

        private static void ConsumeNewline(string source, ref int i, ref int line)
        {
            if (i >= source.Length)
                return;
            if (source[i] == '\r')
                i++;
            if (i < source.Length && source[i] == '\n')
                i++;
            line++;
        }
    }
}
