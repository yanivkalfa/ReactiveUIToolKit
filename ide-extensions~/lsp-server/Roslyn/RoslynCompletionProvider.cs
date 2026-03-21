using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Tags;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Parser;
using CompletionItem     = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem;
using CompletionItemKind = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItemKind;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Provides context-aware C# completions for .uitkx files by delegating to
    /// Roslyn's <see cref="CompletionService.GetCompletionsAsync"/>.
    ///
    /// <para>Unlike the deprecated <c>Recommender</c> API (which was a scope dump),
    /// <c>CompletionService</c> understands member access, object initializers,
    /// method arguments, override suggestions, keywords in context, and all other
    /// C# completion scenarios.</para>
    ///
    /// <para><b>Prerequisite:</b> The workspace must be created with
    /// <c>MefHostServices.DefaultHost</c> so that Roslyn MEF-exported completion
    /// providers are loaded. See <see cref="RoslynHost"/>.</para>
    ///
    /// <para><b>Workspace readiness:</b> If the document has not been built yet
    /// (e.g. within the first 300 ms after file open), the provider calls
    /// <see cref="RoslynHost.EnsureReadyAsync"/> to trigger an immediate
    /// one-shot build before querying.</para>
    /// </summary>
    public sealed class RoslynCompletionProvider
    {
        private readonly RoslynHost _host;

        public RoslynCompletionProvider(RoslynHost host)
        {
            _host = host;
        }

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Returns LSP completion items for the given .uitkx cursor position.
        /// </summary>
        /// <param name="uitkxFilePath">Absolute local path to the .uitkx file.</param>
        /// <param name="uitkxSource">Full current source text.</param>
        /// <param name="parseResult">Already-parsed document.</param>
        /// <param name="uitkxOffset">0-based cursor offset in the .uitkx source.</param>
        /// <param name="triggerChar">Optional character that triggered completion (e.g. '.').</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
            string            uitkxFilePath,
            string            uitkxSource,
            ParseResult       parseResult,
            int               uitkxOffset,
            char?             triggerChar = null,
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
                    return Array.Empty<CompletionItem>(); // workspace not ready

                // Map .uitkx offset -> virtual document offset.
                var virtualResult = virtualDoc.Map.ToVirtualOffset(uitkxOffset);
                if (!virtualResult.HasValue)
                    return Array.Empty<CompletionItem>(); // cursor not in a C# region

                int virtualOffset = virtualResult.Value.VirtualOffset;

                var roslynDoc = _host.GetRoslynDocument(uitkxFilePath);
                if (roslynDoc == null)
                    return Array.Empty<CompletionItem>();

                // Get CompletionService — requires MefHostServices.DefaultHost on the workspace.
                var completionService = CompletionService.GetService(roslynDoc);
                if (completionService == null)
                {
                    ServerLog.Log("[RoslynCompletion] CompletionService is null — workspace missing MEF host?");
                    return Array.Empty<CompletionItem>();
                }

                // Build the trigger context from the character that caused the popup.
                var trigger = triggerChar.HasValue
                    ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                    : CompletionTrigger.Invoke;

                // Ask Roslyn for context-aware completions at the virtual offset.
                var completionList = await completionService
                    .GetCompletionsAsync(roslynDoc, virtualOffset, trigger, cancellationToken: ct)
                    .ConfigureAwait(false);

                if (completionList == null || completionList.ItemsList.Count == 0)
                    return Array.Empty<CompletionItem>();

                // Map Roslyn CompletionItem -> LSP CompletionItem.
                var items = new List<CompletionItem>(completionList.ItemsList.Count);
                foreach (var rItem in completionList.ItemsList)
                {
                    ct.ThrowIfCancellationRequested();
                    items.Add(RoslynItemToLspItem(rItem));
                }

                ServerLog.Log(
                    $"[RoslynCompletion] {items.Count} items at virtual offset {virtualOffset}");
                return items;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynCompletion] Error: {ex.Message}");
                return Array.Empty<CompletionItem>();
            }
        }

        // ── Conversion helpers ────────────────────────────────────────────────

        private static CompletionItem RoslynItemToLspItem(
            Microsoft.CodeAnalysis.Completion.CompletionItem rItem)
        {
            // Prefer explicit InsertionText property (set for snippets / override stubs),
            // otherwise fall back to the display text.
            rItem.Properties.TryGetValue("InsertionText", out var insertionText);
            var insertText = !string.IsNullOrEmpty(insertionText)
                ? insertionText
                : rItem.DisplayText;

            // Prefer inline description (grey hint after label), else display suffix.
            var detail = !string.IsNullOrEmpty(rItem.InlineDescription)
                ? rItem.InlineDescription
                : rItem.DisplayTextSuffix;

            return new CompletionItem
            {
                Label      = rItem.DisplayText,
                Kind       = TagsToCompletionKind(rItem.Tags),
                Detail     = detail,
                InsertText = insertText,
                SortText   = rItem.SortText,
                FilterText = rItem.FilterText,
            };
        }

        private static CompletionItemKind TagsToCompletionKind(
            System.Collections.Immutable.ImmutableArray<string> tags)
        {
            // Roslyn uses string tags from WellKnownTags (Microsoft.CodeAnalysis.Tags).
            foreach (var tag in tags)
            {
                switch (tag)
                {
                    case WellKnownTags.Method:          return CompletionItemKind.Method;
                    case WellKnownTags.ExtensionMethod: return CompletionItemKind.Method;
                    case WellKnownTags.Property:        return CompletionItemKind.Property;
                    case WellKnownTags.Field:           return CompletionItemKind.Field;
                    case WellKnownTags.Event:           return CompletionItemKind.Event;
                    case WellKnownTags.Class:           return CompletionItemKind.Class;
                    case WellKnownTags.Structure:       return CompletionItemKind.Struct;
                    case WellKnownTags.Interface:       return CompletionItemKind.Interface;
                    case WellKnownTags.Enum:            return CompletionItemKind.Enum;
                    case WellKnownTags.EnumMember:      return CompletionItemKind.EnumMember;
                    case WellKnownTags.Delegate:        return CompletionItemKind.Class;
                    case WellKnownTags.Namespace:       return CompletionItemKind.Module;
                    case WellKnownTags.Local:           return CompletionItemKind.Variable;
                    case WellKnownTags.Parameter:       return CompletionItemKind.Variable;
                    case WellKnownTags.Keyword:         return CompletionItemKind.Keyword;
                    case WellKnownTags.Snippet:         return CompletionItemKind.Snippet;
                    case WellKnownTags.TypeParameter:   return CompletionItemKind.TypeParameter;
                    case WellKnownTags.Constant:        return CompletionItemKind.Constant;
                }
            }
            return CompletionItemKind.Text;
        }
    }
}
