using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/// <summary>
/// Async completion source that calls textDocument/completion on the uitkx LSP
/// server directly via the JsonRpc pipe captured in UitkxLanguageClient.
/// </summary>
internal sealed class UitkxCompletionSource : IAsyncCompletionSource
{
    private static readonly string LogPath = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(),
        "uitkx-completion-src.log"
    );

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    // ── IAsyncCompletionSource ───────────────────────────────────────────────

    public CompletionStartData InitializeCompletion(
        CompletionTrigger trigger,
        SnapshotPoint triggerLocation,
        CancellationToken token
    )
    {
        // Walk backwards to find span start: include '@', '<', and identifier chars.
        // This ensures VS *replaces* the typed prefix (e.g. '@', '<Box') rather
        // than inserting after it, which would produce '@@if...' or '<<Box>'.
        var snapshot = triggerLocation.Snapshot;
        var pos = triggerLocation.Position;
        var start = pos;
        while (start > 0)
        {
            var c = snapshot[start - 1];
            // Walk back over '@' and identifier chars only.
            // Do NOT walk back over '<' — tag insertText from the server excludes '<'
            // so the '<' must remain in the buffer (span starts after it).
            if (c == '@' || char.IsLetterOrDigit(c) || c == '_' || c == '-')
                start--;
            else
                break;
        }
        var spanStart = new SnapshotPoint(snapshot, start);
        return new CompletionStartData(
            CompletionParticipation.ProvidesItems,
            new SnapshotSpan(spanStart, triggerLocation)
        );
    }

    public async Task<CompletionContext> GetCompletionContextAsync(
        IAsyncCompletionSession session,
        CompletionTrigger trigger,
        SnapshotPoint triggerLocation,
        SnapshotSpan applicableToSpan,
        CancellationToken token
    )
    {
        Log($"GetCompletionContextAsync called — trigger={trigger.Reason}");

        try
        {
            var rpc = UitkxLanguageClient.InternalRpc;
            if (rpc == null)
            {
                Log("InternalRpc is null — LSP not yet connected");
                return CompletionContext.Empty;
            }

            // Resolve file URI from the text buffer.
            var buffer = triggerLocation.Snapshot.TextBuffer;
            if (!buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            {
                Log("ITextDocument not found on buffer properties");
                return CompletionContext.Empty;
            }

            var uri = new Uri(doc.FilePath).AbsoluteUri;
            var snapshot = triggerLocation.Snapshot;
            var lineNo = snapshot.GetLineNumberFromPosition(triggerLocation.Position);
            var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
            var charNo = triggerLocation.Position - lineStart;

            // VS2022 does not send textDocument/didChange for custom LSP types, so the
            // server's document store is stale. Push the current buffer text before completing.
            var currentText = snapshot.GetText();
            await BufferSyncService.SyncIfChangedAsync(rpc, uri, currentText)
                .ConfigureAwait(false);

            Log($"Calling textDocument/completion: {uri} {lineNo}:{charNo}");

            // Determine trigger kind and character.
            // LSP: 1=Invoked, 2=TriggerCharacter, 3=TriggerForIncompleteCompletions
            int triggerKind = 1;   // default: explicit invocation
            string? triggerCharacter = null;

            if (trigger.Reason == CompletionTriggerReason.Insertion)
            {
                var ch = trigger.Character;
                if (ch == '.' || ch == '<' || ch == '@' || ch == '{' || ch == '(' || ch == ',')
                {
                    triggerKind      = 2; // TriggerCharacter
                    triggerCharacter = ch.ToString();
                }
            }

            // Build context: include triggerCharacter only when triggerKind=2.
            object completionContext = triggerCharacter != null
                ? new { triggerKind, triggerCharacter }
                : (object)new { triggerKind };

            var result = await rpc.InvokeWithParameterObjectAsync<JToken>(
                    "textDocument/completion",
                    new
                    {
                        textDocument = new { uri },
                        position = new { line = lineNo, character = charNo },
                        context  = completionContext,
                    },
                    token
                )
                .ConfigureAwait(false);

            Log(
                $"textDocument/completion response: {result?.ToString()?.Substring(0, Math.Min(200, result?.ToString()?.Length ?? 0))}"
            );

            if (result == null)
                return CompletionContext.Empty;

            // Response is either CompletionItem[] or CompletionList { items: [...] }
            var items = result is JArray arr ? arr : result["items"] as JArray;
            if (items == null || items.Count == 0)
                return CompletionContext.Empty;

            var builder = ImmutableArray.CreateBuilder<CompletionItem>(items.Count);
            foreach (var item in items)
            {
                var label = item["label"]?.ToString() ?? "";
                var rawInsert = item["insertText"]?.ToString() ?? label;
                var insertText = StripSnippetMarkers(rawInsert);
                var sortText = item["sortText"]?.ToString() ?? label;
                var filterText = item["filterText"]?.ToString() ?? label;

                var vsItem = new CompletionItem(
                    displayText: label,
                    source: this,
                    icon: default,
                    filters: ImmutableArray<CompletionFilter>.Empty,
                    suffix: "",
                    insertText: insertText,
                    sortText: sortText,
                    filterText: filterText,
                    attributeIcons: ImmutableArray<Microsoft.VisualStudio.Text.Adornments.ImageElement>.Empty
                );

                vsItem.Properties.AddProperty("lspItem", item.ToString());
                builder.Add(vsItem);
            }

            Log($"Returning {builder.Count} completion items");
            return new CompletionContext(builder.ToImmutable());
        }
        catch (OperationCanceledException)
        {
            return CompletionContext.Empty;
        }
        catch (Exception ex)
        {
            Log($"GetCompletionContextAsync error: {ex}");
            return CompletionContext.Empty;
        }
    }

    // Converts an LSP snippet string to plain text by removing tab-stop markers.
    // e.g. "Box>$0</Box>" → "Box></Box>",  "Button $1 />" → "Button />"
    private static string StripSnippetMarkers(string text)
    {
        // ${N:placeholder}  → placeholder
        text = Regex.Replace(text, @"\$\{\d+:([^}]*)\}", "$1");
        // ${N} and $N  → empty
        text = Regex.Replace(text, @"\$\{?\d+\}?", "");
        // collapse runs of spaces left behind
        text = Regex.Replace(text, @" {2,}", " ").TrimEnd();
        return text;
    }

    public Task<object?> GetDescriptionAsync(
        IAsyncCompletionSession session,
        CompletionItem item,
        CancellationToken token
    )
    {
        if (item.Properties.TryGetProperty<string>("lspItem", out var json))
        {
            try
            {
                var obj = JObject.Parse(json);
                var doc = obj["documentation"]?.ToString();
                var det = obj["detail"]?.ToString();
                var text = doc ?? det ?? item.DisplayText;
                return Task.FromResult<object?>(text);
            }
            catch { }
        }
        return Task.FromResult<object?>(null);
    }
}
