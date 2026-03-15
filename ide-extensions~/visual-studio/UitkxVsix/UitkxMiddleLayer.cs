using System;
using System.IO;
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
            UitkxDiagnosticStore.HandlePublishDiagnostics(methodParam);

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
            Log(
                $"RESPONSE← {methodName}: {result?.ToString(Newtonsoft.Json.Formatting.None)?.Substring(0, Math.Min(300, result?.ToString()?.Length ?? 0))}"
            );

        return result;
    }

    private static bool NeedsBufferSync(string method) =>
        method is "textDocument/definition"
            or "textDocument/formatting"
            or "textDocument/hover"
            or "textDocument/completion";

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
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                .SwitchToMainThreadAsync();

            var rdt = (Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable)
                Microsoft.VisualStudio.Shell.Package.GetGlobalService(
                    typeof(Microsoft.VisualStudio.Shell.Interop.SVsRunningDocumentTable));
            if (rdt == null) return;

            string localPath;
            try { localPath = new Uri(uri).LocalPath; }
            catch { return; }

            if (rdt.FindAndLockDocument(
                    (uint)Microsoft.VisualStudio.Shell.Interop._VSRDTFLAGS.RDT_NoLock,
                    localPath,
                    out _,
                    out _,
                    out var docData,
                    out _) == 0 && docData != IntPtr.Zero)
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
                await rpc.NotifyWithParameterObjectAsync(
                    "textDocument/didChange",
                    new
                    {
                        textDocument = new { uri, version = 1 },
                        contentChanges = new[] { new { text } },
                    });
                Log($"  Synced buffer for {TruncateUri(uri)}");
            }
        }
        catch (Exception ex)
        {
            Log($"  SyncBuffer failed: {ex.Message}");
        }
    }

    private static string TruncateUri(string uri) =>
        uri.Length > 60 ? "..." + uri.Substring(uri.Length - 50) : uri;
}
