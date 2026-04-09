using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using ReactiveUITK.Language.Roslyn;
using ReactiveUITK.Language.SemanticTokens;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Classifies C# regions inside a .uitkx file using Roslyn's full semantic
    /// classifier (<see cref="Classifier.GetClassifiedSpansAsync"/>), then maps
    /// the results back to .uitkx source coordinates via <see cref="SourceMap"/>.
    ///
    /// The returned <see cref="SemanticTokenData"/> records are merged with the
    /// UITKX-markup tokens produced by <see cref="SemanticTokensProvider"/> to
    /// provide complete IDE highlighting: markup structure from the UITKX provider,
    /// C# type/method/variable semantics from this class.
    ///
    /// <para><b>Overlap policy:</b> When a Roslyn token occupies the same source
    /// position as a UITKX token, the UITKX token is kept and the Roslyn token is
    /// discarded.  This preserves accurate UITKX-category highlighting (e.g.
    /// <c>uitkxDirective</c> on <c>@if</c>) while Roslyn fills everything else.</para>
    /// </summary>
    public sealed class RoslynSemanticTokensProvider
    {
        // ── Roslyn classification string → UITKX/LSP token type ──────────────

        // Uses OrdinalIgnoreCase for robustness across Roslyn versions that may
        // capitalise or hyphenate the strings slightly differently.
        private static readonly Dictionary<string, string> s_typeMap = new Dictionary<
            string,
            string
        >(StringComparer.OrdinalIgnoreCase)
        {
            // Keywords
            ["keyword"] = SemanticTokenTypes.Keyword,
            ["keyword - control"] = SemanticTokenTypes.Keyword,
            ["preprocessor keyword"] = SemanticTokenTypes.Keyword,
            // Types
            ["class name"] = SemanticTokenTypes.Type,
            ["struct name"] = SemanticTokenTypes.Type,
            ["record class name"] = SemanticTokenTypes.Type,
            ["record struct name"] = SemanticTokenTypes.Type,
            ["interface name"] = SemanticTokenTypes.Type,
            ["enum name"] = SemanticTokenTypes.Type,
            ["delegate name"] = SemanticTokenTypes.Type,
            ["type parameter name"] = SemanticTokenTypes.Type,
            ["namespace name"] = SemanticTokenTypes.Type,
            // Methods / functions
            ["method name"] = SemanticTokenTypes.Function,
            ["extension method name"] = SemanticTokenTypes.Function,
            // Properties / fields / members
            // Note: "property name" is intentionally omitted — the TmLanguage grammar
            // provides good property coloring and Roslyn semantic tokens would override
            // it with a less distinguishable color in most themes.
            ["field name"] = SemanticTokenTypes.Variable,
            ["enum member name"] = SemanticTokenTypes.Variable,
            ["event name"] = SemanticTokenTypes.Variable,
            ["constant name"] = SemanticTokenTypes.Variable,
            ["parameter name"] = SemanticTokenTypes.Variable,
            ["local name"] = SemanticTokenTypes.Variable,
            ["label name"] = SemanticTokenTypes.Variable,
            // Literals
            ["string"] = SemanticTokenTypes.String,
            ["verbatim string"] = SemanticTokenTypes.String,
            ["string - verbatim"] = SemanticTokenTypes.String,
            ["interpolated string text"] = SemanticTokenTypes.String,
            ["number"] = SemanticTokenTypes.Number,
            // Comments (including XML doc)
            ["comment"] = SemanticTokenTypes.Comment,
            ["xml doc comment - text"] = SemanticTokenTypes.Comment,
            ["xml doc comment - delimiter"] = SemanticTokenTypes.Comment,
            ["xml doc comment - attribute name"] = SemanticTokenTypes.Comment,
            ["xml doc comment - attribute quotes"] = SemanticTokenTypes.Comment,
            ["xml doc comment - attribute value"] = SemanticTokenTypes.Comment,
            ["xml doc comment - name"] = SemanticTokenTypes.Comment,
            ["xml doc comment - processing instruction"] = SemanticTokenTypes.Comment,
            ["xml doc comment - entity reference"] = SemanticTokenTypes.Comment,
            ["xml doc comment - comment"] = SemanticTokenTypes.Comment,
        };

        private static readonly string[] s_noMods = Array.Empty<string>();

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Asynchronously classifies all C# spans in the Roslyn virtual document
        /// and maps them back to .uitkx source coordinates.
        /// </summary>
        /// <param name="document">
        /// Roslyn <see cref="Document"/> from the <see cref="AdhocWorkspace"/>.
        /// </param>
        /// <param name="map">
        /// Source map for this file — defines which virtual-doc ranges correspond
        /// to .uitkx source ranges.
        /// </param>
        /// <param name="uitkxSource">
        /// Raw .uitkx source text, used to convert character offsets to line/column.
        /// </param>
        /// <param name="existingPositions">
        /// Set of (line, col) pairs already covered by UITKX tokens.  Any Roslyn
        /// token starting at one of these positions is suppressed (see overlap policy).
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Array of <see cref="SemanticTokenData"/> in 0-based .uitkx coordinates,
        /// sorted by line then column.
        /// </returns>
        public async Task<SemanticTokenData[]> GetTokensAsync(
            Document document,
            SourceMap map,
            string uitkxSource,
            HashSet<(int Line, int Col)>? existingPositions,
            CancellationToken ct = default
        )
        {
            if (map == null || map.Entries.IsDefaultOrEmpty)
                return Array.Empty<SemanticTokenData>();

            // Classify across the union of all source-mapped regions.
            var entries = map.Entries;
            int totalStart = entries[0].VirtualStart;
            int totalEnd = entries[entries.Length - 1].VirtualEnd;
            var querySpan = TextSpan.FromBounds(totalStart, totalEnd);

            IEnumerable<ClassifiedSpan> classified;
            try
            {
                classified = await Classifier
                    .GetClassifiedSpansAsync(document, querySpan, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ServerLog.Log(
                    $"[RoslynSemanticTokens] Classifier.GetClassifiedSpansAsync error: {ex.Message}"
                );
                return Array.Empty<SemanticTokenData>();
            }

            // Pre-fetch syntax root and semantic model for delegate detection.
            // Both are already cached by Roslyn (~1ms each on subsequent calls).
            SyntaxNode? syntaxRoot = null;
            SemanticModel? semanticModel = null;
            try
            {
                syntaxRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
                semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            { /* graceful degradation — delegate detection skipped */
            }

            // Pre-build a line-starts lookup so offset → (line, col) is fast.
            var lineStarts = BuildLineStarts(uitkxSource);

            var results = new List<SemanticTokenData>(64);

            foreach (var cs in classified)
            {
                ct.ThrowIfCancellationRequested();

                // Map classification type to an LSP token type string.
                if (!s_typeMap.TryGetValue(cs.ClassificationType, out var tokenType))
                    continue;

                // Override locals/parameters whose type is a delegate to Function.
                // This covers all delegate-typed variables generically:
                //   var (count, setCount) = useState(0)  → setCount is StateSetter<int> → Function
                //   Action callback = MyMethod           → callback is Action → Function
                if (
                    syntaxRoot != null
                    && semanticModel != null
                    && (
                        cs.ClassificationType == "local name"
                        || cs.ClassificationType == "parameter name"
                    )
                )
                {
                    try
                    {
                        var token = syntaxRoot.FindToken(cs.TextSpan.Start);
                        if (token.Parent != null)
                        {
                            var typeInfo = semanticModel.GetTypeInfo(token.Parent, ct);
                            var type = typeInfo.Type ?? typeInfo.ConvertedType;
                            if (type?.TypeKind == TypeKind.Delegate)
                                tokenType = SemanticTokenTypes.Function;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch
                    { /* graceful fallback to original classification */
                    }
                }

                // Only include spans that trace back to user-authored .uitkx C# code.
                var mapped = map.ToUitkxOffset(cs.TextSpan.Start);
                if (!mapped.HasValue)
                    continue;

                int uitkxStart = mapped.Value.UitkxOffset;
                var entry = mapped.Value.Entry;

                // The token length is the Roslyn span length, clamped to the entry bound
                // (defensive — should always fit within the entry).
                int tokenLength = cs.TextSpan.Length;
                int maxLen = entry.UitkxEnd - uitkxStart;
                if (tokenLength > maxLen)
                    tokenLength = maxLen;
                if (tokenLength <= 0)
                    continue;

                // Convert to 0-based line/column in the .uitkx file.
                var (line0, col0) = OffsetToLineCol(uitkxStart, lineStarts);

                // Suppress tokens that collide with an existing UITKX token start.
                if (existingPositions != null && existingPositions.Contains((line0, col0)))
                    continue;

                results.Add(
                    new SemanticTokenData
                    {
                        Line = line0,
                        Column = col0,
                        Length = tokenLength,
                        TokenType = tokenType,
                        Modifiers = s_noMods,
                    }
                );
            }

            // Stable sort: line ascending, then column ascending.
            results.Sort(
                static (a, b) =>
                {
                    int cmp = a.Line.CompareTo(b.Line);
                    return cmp != 0 ? cmp : a.Column.CompareTo(b.Column);
                }
            );

            return results.ToArray();
        }

        /// <summary>
        /// Returns classification overrides for locals/parameters whose Roslyn type
        /// is a delegate.  These overrides tell the VS2022 classifier to reclassify
        /// the span from <c>identifier</c> to <c>method</c> (function colour).
        /// </summary>
        /// <remarks>
        /// This is the Phase B counterpart of the inline delegate detection in
        /// <see cref="GetTokensAsync"/>: VSCode gets the override via semantic
        /// tokens, but VS2022 cannot consume semantic tokens — it receives these
        /// overrides via a custom <c>uitkx/classificationOverrides</c> notification.
        /// </remarks>
        public async Task<ClassificationOverride[]> GetDelegateOverridesAsync(
            Document document,
            SourceMap map,
            string uitkxSource,
            CancellationToken ct = default
        )
        {
            if (map == null || map.Entries.IsDefaultOrEmpty)
                return Array.Empty<ClassificationOverride>();

            var entries = map.Entries;
            int totalStart = entries[0].VirtualStart;
            int totalEnd = entries[entries.Length - 1].VirtualEnd;
            var querySpan = TextSpan.FromBounds(totalStart, totalEnd);

            IEnumerable<ClassifiedSpan> classified;
            try
            {
                classified = await Classifier
                    .GetClassifiedSpansAsync(document, querySpan, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch { return Array.Empty<ClassificationOverride>(); }

            SyntaxNode? syntaxRoot = null;
            SemanticModel? semanticModel = null;
            try
            {
                syntaxRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
                semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch { return Array.Empty<ClassificationOverride>(); }

            if (syntaxRoot == null || semanticModel == null)
                return Array.Empty<ClassificationOverride>();

            var lineStarts = BuildLineStarts(uitkxSource);
            var results = new List<ClassificationOverride>();

            foreach (var cs in classified)
            {
                ct.ThrowIfCancellationRequested();

                if (cs.ClassificationType != "local name" && cs.ClassificationType != "parameter name")
                    continue;

                try
                {
                    var token = syntaxRoot.FindToken(cs.TextSpan.Start);
                    if (token.Parent == null) continue;

                    // Try expression-based type info first (works for usages like
                    // `setGameStarted(true)`).
                    var typeInfo = semanticModel.GetTypeInfo(token.Parent, ct);
                    var type = typeInfo.Type ?? typeInfo.ConvertedType;

                    // For tuple declarations like `var (x, setX) = useState(...)`,
                    // GetTypeInfo fails because the parent is a SingleVariableDesignationSyntax
                    // (not an expression).  Fall back to GetDeclaredSymbol → symbol.Type.
                    if (type?.TypeKind != TypeKind.Delegate)
                    {
                        var declared = semanticModel.GetDeclaredSymbol(token.Parent, ct);
                        if (declared is ILocalSymbol local)
                            type = local.Type;
                        else if (declared is IParameterSymbol param)
                            type = param.Type;
                    }

                    if (type?.TypeKind != TypeKind.Delegate) continue;

                    var mapped = map.ToUitkxOffset(cs.TextSpan.Start);
                    if (!mapped.HasValue) continue;

                    int uitkxStart = mapped.Value.UitkxOffset;
                    int tokenLength = cs.TextSpan.Length;
                    int maxLen = mapped.Value.Entry.UitkxEnd - uitkxStart;
                    if (tokenLength > maxLen) tokenLength = maxLen;
                    if (tokenLength <= 0) continue;

                    var (line0, col0) = OffsetToLineCol(uitkxStart, lineStarts);
                    results.Add(new ClassificationOverride
                    {
                        Line = line0,
                        Column = col0,
                        Length = tokenLength,
                        OverrideType = "method",
                    });
                }
                catch (OperationCanceledException) { throw; }
                catch { /* skip single token */ }
            }

            return results.ToArray();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns an array where <c>lineStarts[i]</c> is the character offset of
        /// the first character on 0-based line <c>i</c> within <paramref name="text"/>.
        /// </summary>
        private static int[] BuildLineStarts(string text)
        {
            var starts = new List<int>(text.Length / 40 + 4) { 0 };
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '\n')
                    starts.Add(i + 1);
            return starts.ToArray();
        }

        /// <summary>
        /// Converts a 0-based character <paramref name="offset"/> to a
        /// (0-based line, 0-based col) pair using the pre-built
        /// <paramref name="lineStarts"/> table.
        /// </summary>
        private static (int Line, int Col) OffsetToLineCol(int offset, int[] lineStarts)
        {
            // Binary search for the last line start ≤ offset.
            int lo = 0,
                hi = lineStarts.Length - 1;
            while (lo < hi)
            {
                int mid = lo + (hi - lo + 1) / 2;
                if (lineStarts[mid] <= offset)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            return (lo, offset - lineStarts[lo]);
        }
    }
}
