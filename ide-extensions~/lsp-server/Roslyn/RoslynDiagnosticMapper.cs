using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Roslyn;
using RoslynDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Converts Roslyn <see cref="Diagnostic"/> objects to UITKX-native
    /// <see cref="ParseDiagnostic"/> values using a <see cref="SourceMap"/> and
    /// Roslyn's own <c>#line</c>-resolved mapped-line-span.
    ///
    /// <para><b>Position resolution order:</b>
    /// <list type="number">
    ///   <item>Source-map lookup (character-precise, uses <see cref="SourceMapEntry"/>).
    ///   Preferred because it matches both line <em>and</em> column exactly.</item>
    ///   <item><c>GetMappedLineSpan()</c> — Roslyn applies <c>#line</c> directives
    ///   to rewrite the location.  Yields the correct .uitkx line and column even
    ///   without the source map.  Used as fallback when the diagnostic span starts
    ///   outside every mapped region (e.g. in generated scaffold).</item>
    ///   <item>Skip — diagnostics whose mapped file does not match the .uitkx path
    ///   are silently dropped (they originate from the scaffold itself).</item>
    /// </list></para>
    ///
    /// <para><b>Suppression table</b> (built-in, configurable in future):
    /// CS0246, CS8019, CS1591, CS0649, CS0414, CS8618, CS0169 are always suppressed
    /// because they fire on the generated scaffold regardless of the user's code.
    /// These are also disabled at the compilation level in <see cref="RoslynHost"/>
    /// but the mapper provides a second filter for robustness.</para>
    /// </summary>
    public sealed class RoslynDiagnosticMapper
    {
        // ── Suppressed IDs ────────────────────────────────────────────────────

        /// <summary>
        /// Diagnostic IDs that are suppressed globally for UITKX virtual documents.
        /// All of these are false-positives caused by the generated scaffold.
        /// </summary>
        private static readonly HashSet<string> s_suppressedIds = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            // CS0246 — type/namespace not found.
            // Previously suppressed globally; now emitted only around specific scaffold
            // lines via #pragma warning disable CS0246 so that real user type-not-found
            // errors (e.g. misspelled type names) surface as diagnostics.
            // "CS0246",
            // CS0162 — Unreachable code detected.
            // NOT suppressed: UITKX0107 handles root-scope returns with full-range
            // dimming. CS0162 covers nested scopes (lambdas, local functions)
            // where our T2 analysis doesn't reach. PushTier3 drops CS0162 inside
            // UITKX0107 ranges to avoid duplicates.
            // "CS0162",
            "CS8019", // Unnecessary using directive
            "CS1591", // Missing XML comment
            "CS0649", // Field '…' is never assigned to
            "CS0414", // The field '…' is assigned but its value is never used
            "CS8618", // Non-nullable field '…' must contain a non-null value
            "CS0169", // The field '…' is never used
            "CS8625", // Cannot convert null literal to non-nullable reference type (scaffold default!)
            // CS0219 — variable assigned but never used.
            // Suppressed at compilation level; replaced by UITKX0112 which uses
            // AnalyzeDataFlow and also catches non-constant initialisers.
            // "CS0219",
            "CS8974", // Converting method group to non-delegate type 'object' (false-positive from virtual doc)
            // CS1660 no longer needed — useState setter is now modeled as __StateSetter__<T>(Func<T,T>)
            // so lambda bodies get full type inference. Direct-value calls produce CS1503 instead,
            // which is filtered below by message inspection (not global suppression).
            "CS1977", // Cannot use lambda as argument to dynamically dispatched operation — block-body
            // lambda params are typed as `dynamic` in the scaffold; nested lambdas passed to
            // methods on those params (e.g. dm.AppendAction("X", _ => ...)) trigger this error.
            // Unity's source generator knows the real types and never hits this.
            "CS0436", // Type conflicts with imported type — companion .cs files in the same directory
            // shadow types in Assembly-CSharp.dll. Intentional; safety net for #line-mapped spans.
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a sequence of Roslyn diagnostics and their pre-resolved source-map
        /// entries to <see cref="ParseDiagnostic"/> objects that the LSP publisher can
        /// push to the client.
        ///
        /// <paramref name="diagnosticsWithMap"/> comes directly from
        /// <see cref="RoslynHost.GetLatestDiagnostics"/>.
        /// </summary>
        /// <param name="diagnosticsWithMap">Roslyn diagnostics paired with their source-map entry (or null).</param>
        /// <param name="uitkxFilePath">Absolute path to the .uitkx file (used to validate mapped locations).</param>
        /// <returns>
        /// Filtered list of <see cref="ParseDiagnostic"/> objects.  Scaffold noise is
        /// removed; only diagnostics that map into user-authored C# regions are returned.
        /// </returns>
        public IReadOnlyList<ParseDiagnostic> Map(
            IReadOnlyList<(Diagnostic Diagnostic, SourceMapEntry? MapEntry)> diagnosticsWithMap,
            string uitkxFilePath,
            string? uitkxSource = null
        )
        {
            var result = new List<ParseDiagnostic>(diagnosticsWithMap.Count);

            foreach (var (diag, mapEntry) in diagnosticsWithMap)
            {
                // Drop hidden diagnostics
                if (diag.Severity == RoslynDiagnosticSeverity.Hidden)
                    continue;

                // Drop suppressed IDs
                if (s_suppressedIds.Contains(diag.Id))
                    continue;

                // CS1503: suppress when caused by state-setter direct-value calls
                // (e.g. setCount(5) — passes int to __StateSetter__<int>(Func<int,int>)).
                // Real CS1503 errors (wrong argument types) don't mention 'Func<'.
                if (diag.Id == "CS1503")
                {
                    var msg = diag.GetMessage();
                    if (msg.Contains("Func<"))
                        continue;
                }

                // ── Resolve position ─────────────────────────────────────────

                int uitkxLine = 0;
                int uitkxCol = 0;
                int uitkxEndLine = 0;
                int uitkxEndCol = 0;

                if (mapEntry != null)
                {
                    // Source-map route: character-precise column from offsets.
                    var span = diag.Location.SourceSpan;
                    int uitkxStartOffset =
                        mapEntry.UitkxStart + Math.Max(0, span.Start - mapEntry.VirtualStart);
                    int uitkxEndOffset =
                        mapEntry.UitkxStart
                        + Math.Min(
                            span.End - mapEntry.VirtualStart,
                            mapEntry.UitkxEnd - mapEntry.UitkxStart
                        );

                    if (uitkxSource != null)
                    {
                        (uitkxLine, uitkxCol) = OffsetToLineCol(uitkxSource, uitkxStartOffset);
                        (uitkxEndLine, uitkxEndCol) = OffsetToLineCol(uitkxSource, uitkxEndOffset);
                    }
                    else
                    {
                        // Fallback: line from entry, column unknown
                        uitkxLine = mapEntry.UitkxLine;
                        uitkxCol = 0;
                        uitkxEndLine = uitkxLine;
                        uitkxEndCol = 0;
                    }
                }
                else
                {
                    // #line directive route via Roslyn's mapped span
                    var loc = diag.Location;
                    var mappedSpan = loc.GetMappedLineSpan();

                    // If the mapped path doesn't point to our uitkx file, skip.
                    // (Diagnostics in scaffolded code not covered by #line still
                    //  report the virtual .g.cs path.)
                    if (!IsUitkxPath(mappedSpan.Path, uitkxFilePath))
                        continue;

                    // mappedLineSpan uses 0-based lines; ParseDiagnostic uses 1-based
                    uitkxLine = mappedSpan.IsValid ? mappedSpan.Span.Start.Line + 1 : 0;
                    uitkxCol = mappedSpan.IsValid ? mappedSpan.Span.Start.Character : 0;
                    uitkxEndLine = mappedSpan.IsValid ? mappedSpan.Span.End.Line + 1 : uitkxLine;
                    uitkxEndCol = mappedSpan.IsValid ? mappedSpan.Span.End.Character : uitkxCol;
                }

                if (uitkxLine <= 0)
                    continue; // position unknown — skip rather than report at line 0

                result.Add(
                    new ParseDiagnostic
                    {
                        Code = diag.Id,
                        Severity = ToParseServerity(diag.Severity),
                        Message = diag.GetMessage(),
                        SourceLine = uitkxLine,
                        SourceColumn = uitkxCol,
                        EndLine = uitkxEndLine,
                        EndColumn = uitkxEndCol,
                    }
                );
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="mappedPath"/> refers to the same
        /// .uitkx file as <paramref name="uitkxFilePath"/>, using case-insensitive
        /// comparison on Windows and case-sensitive on Unix.
        /// An empty or null <paramref name="mappedPath"/> is treated as a match so
        /// that diagnostics without a <c>#line</c> context are accepted.
        /// </summary>
        private static bool IsUitkxPath(string? mappedPath, string uitkxFilePath)
        {
            if (string.IsNullOrEmpty(mappedPath))
                return true; // no #line info — assume it belongs to this file

            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            // Normalise separators before comparing
            string norm1 = mappedPath.Replace('\\', '/');
            string norm2 = uitkxFilePath.Replace('\\', '/');

            return string.Equals(norm1, norm2, comparison);
        }

        private static ParseSeverity ToParseServerity(RoslynDiagnosticSeverity s) =>
            s switch
            {
                RoslynDiagnosticSeverity.Error => ParseSeverity.Error,
                RoslynDiagnosticSeverity.Warning => ParseSeverity.Warning,
                RoslynDiagnosticSeverity.Info => ParseSeverity.Information,
                _ => ParseSeverity.Hint,
            };

        /// <summary>
        /// Converts a 0-based character offset in <paramref name="source"/> to a
        /// 1-based line number and 0-based column number, matching the coordinate
        /// system used by <see cref="ParseDiagnostic"/>.
        /// </summary>
        private static (int Line, int Col) OffsetToLineCol(string source, int offset)
        {
            offset = Math.Max(0, Math.Min(offset, source.Length));
            int line = 1;
            int lineStart = 0;
            for (int i = 0; i < offset; i++)
            {
                if (source[i] == '\n')
                {
                    line++;
                    lineStart = i + 1;
                }
            }
            return (line, offset - lineStart);
        }
    }
}
