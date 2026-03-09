using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Parser;
using CompletionItem     = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem;
using CompletionItemKind = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItemKind;
using RoslynSymbolKind   = Microsoft.CodeAnalysis.SymbolKind;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Provides C# symbol-based completions for .uitkx files by delegating to
    /// Roslyn's <see cref="Recommender.GetRecommendedSymbolsAtPositionAsync"/>.
    ///
    /// <para>This uses the semantic model to enumerate all symbols accessible at
    /// the cursor position in the virtual C# document â€” local variables, fields,
    /// properties, methods, types, etc. â€” and converts them to LSP
    /// <see cref="CompletionItem"/> objects.</para>
    ///
    /// <para>A static set of C# keywords is merged with the symbol completions
    /// and deduplicated, ensuring the list is immediately useful even when the
    /// Roslyn workspace is freshly built.</para>
    ///
    /// <para><b>Workspace readiness:</b> If the document has not been built yet
    /// (e.g. within the first 300 ms after file open), the provider calls
    /// <see cref="RoslynHost.EnsureReadyAsync"/> to trigger an immediate
    /// one-shot build before querying symbols.</para>
    /// </summary>
    public sealed class RoslynCompletionProvider
    {
        private readonly RoslynHost _host;

        // Minimal C# keyword set that Roslyn's Recommender does not return.
        private static readonly string[] s_csharpKeywords =
        {
            "var", "new", "null", "true", "false", "this", "base",
            "return", "if", "else", "for", "foreach", "while", "do",
            "switch", "case", "break", "continue", "default",
            "try", "catch", "finally", "throw",
            "using", "await", "async", "static", "readonly",
            "public", "private", "protected", "internal",
            "override", "virtual", "abstract", "sealed",
            "in", "out", "ref", "params",
            "string", "int", "bool", "float", "double", "long",
            "byte", "char", "object", "void", "decimal", "uint",
            "short", "ushort", "ulong", "sbyte",
        };

        // Static keyword completion items (computed once at startup).
        private static readonly IReadOnlyList<CompletionItem> s_keywordItems =
            BuildKeywordItems();

        public RoslynCompletionProvider(RoslynHost host)
        {
            _host = host;
        }

        // â”€â”€ Public entry point â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Returns LSP completion items for the given .uitkx cursor position.
        /// </summary>
        /// <param name="uitkxFilePath">Absolute local path to the .uitkx file.</param>
        /// <param name="uitkxSource">Full current source text.</param>
        /// <param name="parseResult">Already-parsed document.</param>
        /// <param name="uitkxOffset">0-based cursor offset in the .uitkx source.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
            string            uitkxFilePath,
            string            uitkxSource,
            ParseResult       parseResult,
            int               uitkxOffset,
            CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(uitkxFilePath))
                return Array.Empty<CompletionItem>();

            try
            {
                // Ensure workspace is ready (immediate build if not yet loaded).
                await _host.EnsureReadyAsync(uitkxFilePath, uitkxSource, parseResult, ct)
                    .ConfigureAwait(false);

                var virtualDoc = _host.GetVirtualDocument(uitkxFilePath);
                if (virtualDoc == null)
                    return s_keywordItems; // workspace not ready; fall back to keywords

                // Map .uitkx offset â†’ virtual document offset.
                var virtualResult = virtualDoc.Map.ToVirtualOffset(uitkxOffset);
                if (!virtualResult.HasValue)
                    return s_keywordItems; // cursor not in a C# region

                int virtualOffset = virtualResult.Value.VirtualOffset;

                var roslynDoc = _host.GetRoslynDocument(uitkxFilePath);
                if (roslynDoc == null)
                    return s_keywordItems;

                // Ask Roslyn for all symbols accessible at this position.
                // Recommender uses the SemanticModel to determine scope-appropriate
                // completions (locals, fields, type members, accessible types, â€¦).
                var semanticModel = await roslynDoc.GetSemanticModelAsync(ct).ConfigureAwait(false);
                if (semanticModel == null)
                    return s_keywordItems;

                var workspace = roslynDoc.Project.Solution.Workspace;
#pragma warning disable CS0618 // The Document overload needs RecommendationOptions not yet in our reference set
                var symbols = await Recommender
                    .GetRecommendedSymbolsAtPositionAsync(
                        semanticModel,
                        virtualOffset,
                        workspace,
                        cancellationToken: ct)
                    .ConfigureAwait(false);
#pragma warning restore CS0618

                // Build deduplicated list: symbols first (most relevant), then keywords.
                var seen  = new HashSet<string>(StringComparer.Ordinal);
                var items = new List<CompletionItem>(128);

                foreach (var sym in symbols)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!seen.Add(sym.Name))
                        continue;
                    items.Add(SymbolToLspItem(sym));
                }

                // Append C# keywords that aren't already provided as symbols.
                foreach (var kwItem in s_keywordItems)
                    if (seen.Add(kwItem.Label!))
                        items.Add(kwItem);

                ServerLog.Log(
                    $"[RoslynCompletion] {items.Count} items at virtual offset {virtualOffset}");
                return items;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynCompletion] Error: {ex.Message}");
                return s_keywordItems;
            }
        }

        // â”€â”€ Conversion helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static CompletionItem SymbolToLspItem(ISymbol sym)
        {
            return new CompletionItem
            {
                Label      = sym.Name,
                Kind       = SymbolKindToCompletionKind(sym.Kind),
                Detail     = sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                InsertText = sym.Name,
                SortText   = sym.Name,
                FilterText = sym.Name,
            };
        }

        private static CompletionItemKind SymbolKindToCompletionKind(RoslynSymbolKind kind) =>
            kind switch
            {
                RoslynSymbolKind.Local          => CompletionItemKind.Variable,
                RoslynSymbolKind.Parameter      => CompletionItemKind.Variable,
                RoslynSymbolKind.Field          => CompletionItemKind.Field,
                RoslynSymbolKind.Property       => CompletionItemKind.Property,
                RoslynSymbolKind.Method         => CompletionItemKind.Method,
                RoslynSymbolKind.NamedType      => CompletionItemKind.Class,
                RoslynSymbolKind.Namespace      => CompletionItemKind.Module,
                RoslynSymbolKind.Event          => CompletionItemKind.Event,
                RoslynSymbolKind.TypeParameter  => CompletionItemKind.TypeParameter,
                _                               => CompletionItemKind.Text,
            };

        private static IReadOnlyList<CompletionItem> BuildKeywordItems()
        {
            var items = new List<CompletionItem>(s_csharpKeywords.Length);
            foreach (var kw in s_csharpKeywords)
            {
                items.Add(new CompletionItem
                {
                    Label      = kw,
                    Kind       = CompletionItemKind.Keyword,
                    InsertText = kw,
                    SortText   = "~" + kw, // sort after symbol suggestions
                    FilterText = kw,
                });
            }
            return items;
        }
    }
}

