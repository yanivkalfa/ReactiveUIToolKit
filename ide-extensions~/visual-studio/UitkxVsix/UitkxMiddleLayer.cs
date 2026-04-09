using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/// <summary>
/// Intercepts LSP messages between VS2022 and the server.
/// - Routes publishDiagnostics to UitkxDiagnosticStore for squiggles.
/// - Syncs buffer content before requests that need fresh text (VS2022 doesn't
///   send textDocument/didChange for custom content types automatically).
/// </summary>
internal sealed class UitkxMiddleLayer : ILanguageClientMiddleLayer
{
    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-middleware.log"
    );

    private static void Log(string line)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {line}\n");
        }
        catch { }
    }

    public bool CanHandle(string methodName) => true;

    public async Task HandleNotificationAsync(
        string methodName,
        JToken methodParam,
        Func<JToken, Task> sendNotification
    )
    {
        Log($"NOTIFY  → {methodName}");

        if (methodName == "textDocument/publishDiagnostics")
        {
            // Route full diagnostics to our custom taggers (classifier + unreachable tagger).
            UitkxDiagnosticStore.HandlePublishDiagnostics(methodParam);

            // Strip Unnecessary-tagged diagnostics from VS2022's built-in passthrough.
            // With CodeRemoteContentTypeName, VS2022's built-in rendering partially
            // overrides our IClassifier's "excluded code" classification for Hint severity,
            // resulting in keywords keeping their syntax colors instead of full gray.
            // By stripping these, we let our UitkxClassifier + UitkxUnreachableCodeTagger
            // handle ALL fade rendering without interference.
            var filtered = StripUnnecessaryDiagnostics(methodParam);
            await sendNotification(filtered);
            return;
        }

        await sendNotification(methodParam);
    }

    public async Task<JToken?> HandleRequestAsync(
        string methodName,
        JToken methodParam,
        Func<JToken, Task<JToken?>> sendRequest
    )
    {
        Log($"REQUEST → {methodName}");

        // VS2022 doesn't send textDocument/didChange for custom content types.
        // Before forwarding requests that need fresh content, sync the buffer.
        if (NeedsBufferSync(methodName))
        {
            await SyncBufferAsync(methodParam);
        }

        var result = await sendRequest(methodParam);

        if (methodName == "initialize" && result != null)
            Log($"InitializeResult (capabilities): {result}");
        else
        {
            var json = result?.ToString(Newtonsoft.Json.Formatting.None);
            if (json != null && json.Length > 300)
                json = json.Substring(0, 300);
            Log($"RESPONSE← {methodName}: {json}");
        }

        return result;
    }

    private static bool NeedsBufferSync(string method) =>
        method
            is "textDocument/definition"
                or "textDocument/formatting"
                or "textDocument/hover"
                or "textDocument/completion"
                or "textDocument/rename"
                or "textDocument/prepareRename"
                or "textDocument/references";

    /// <summary>
    /// Extracts the URI from the request params, finds the matching VS buffer,
    /// and sends a full-content didChange to the LSP server.
    /// </summary>
    private static async Task SyncBufferAsync(JToken methodParam)
    {
        try
        {
            var uri = methodParam?["textDocument"]?["uri"]?.ToString();
            if (string.IsNullOrEmpty(uri))
                return;

            var rpc = UitkxLanguageClient.InternalRpc;
            if (rpc == null)
                return;

            // Get the current buffer text from VS's running document table.
            string? text = null;
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var rdt = (Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable)
                Microsoft.VisualStudio.Shell.Package.GetGlobalService(
                    typeof(Microsoft.VisualStudio.Shell.Interop.SVsRunningDocumentTable)
                );
            if (rdt == null)
                return;

            string localPath;
            try
            {
                localPath = new Uri(uri).LocalPath;
            }
            catch
            {
                return;
            }

            if (
                rdt.FindAndLockDocument(
                    (uint)Microsoft.VisualStudio.Shell.Interop._VSRDTFLAGS.RDT_NoLock,
                    localPath,
                    out _,
                    out _,
                    out var docData,
                    out _
                ) == 0
                && docData != IntPtr.Zero
            )
            {
                var obj = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(docData);
                if (obj is Microsoft.VisualStudio.Text.ITextBuffer buffer)
                {
                    text = buffer.CurrentSnapshot.GetText();
                }
                else if (obj is Microsoft.VisualStudio.TextManager.Interop.IVsTextLines lines)
                {
                    lines.GetLastLineIndex(out var lastLine, out var lastCol);
                    lines.GetLineText(0, 0, lastLine, lastCol, out text);
                }
            }

            await TaskScheduler.Default;

            if (text != null)
            {
                var sent = await BufferSyncService.SyncIfChangedAsync(rpc, uri, text);
                if (sent)
                    Log($"  Synced buffer for {TruncateUri(uri)}");
                else
                    Log($"  Skipped sync (unchanged) for {TruncateUri(uri)}");
            }
        }
        catch (Exception ex)
        {
            Log($"  SyncBuffer failed: {ex.Message}");
        }
    }

    private static string TruncateUri(string uri) =>
        uri.Length > 60 ? "..." + uri.Substring(uri.Length - 50) : uri;

    /// <summary>
    /// Returns a clone of the publishDiagnostics params with diagnostics that
    /// carry <c>DiagnosticTag.Unnecessary</c> (tag value 1) removed.
    /// This prevents VS2022's built-in rendering from interfering with our
    /// custom "excluded code" classifier/tagger fade.
    /// </summary>
    private static JToken StripUnnecessaryDiagnostics(JToken param)
    {
        var clone = param.DeepClone();
        var diagArray = clone["diagnostics"] as JArray;
        if (diagArray == null || diagArray.Count == 0)
            return clone;

        for (int i = diagArray.Count - 1; i >= 0; i--)
        {
            var tags = diagArray[i]["tags"] as JArray;
            if (tags != null && tags.Any(t => t.Value<int>() == 1))
            {
                diagArray.RemoveAt(i);
            }
        }

        var originalCount = (param["diagnostics"] as JArray)?.Count ?? 0;
        Log(
            $"  Stripped {originalCount - diagArray.Count} Unnecessary diagnostics from VS2022 passthrough"
        );
        return clone;
    }
}
