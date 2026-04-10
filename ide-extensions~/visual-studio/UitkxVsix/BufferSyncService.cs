using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace UitkxVsix;

/// <summary>
/// Centralised buffer-sync service that deduplicates <c>textDocument/didChange</c>
/// notifications to the LSP server.
///
/// <para>
/// VS2022 does not send <c>didChange</c> automatically for custom content types
/// in Open Folder mode, so multiple components (middleware, hover, completion,
/// rename, formatting, diagnostic tagger) all need to push buffer contents
/// before making LSP requests.  Without deduplication each push triggers a
/// full server re-parse → <c>publishDiagnostics</c> → classification cascade,
/// causing visible flickering.
/// </para>
///
/// <para>
/// This service keeps a per-URI hash of the last text sent.  When a caller
/// asks to sync, the send is skipped if the text hasn't changed since the
/// last successful push.
/// </para>
/// </summary>
internal static class BufferSyncService
{
    // Per-URI hash of the last text successfully sent via didChange.
    private static readonly ConcurrentDictionary<string, int> _lastSentHashes =
        new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-unreachable-trace.log"
    );

    /// <summary>
    /// Sends <c>textDocument/didChange</c> to the server only if <paramref name="text"/>
    /// differs from the last text sent for <paramref name="uri"/>.
    /// </summary>
    /// <returns><c>true</c> if the notification was sent; <c>false</c> if skipped.</returns>
    internal static async Task<bool> SyncIfChangedAsync(JsonRpc rpc, string uri, string text)
    {
        var hash = text.GetHashCode();

        if (_lastSentHashes.TryGetValue(uri, out var lastHash) && lastHash == hash)
            return false;

        // Update hash before sending so concurrent callers skip immediately.
        _lastSentHashes[uri] = hash;

        await rpc.NotifyWithParameterObjectAsync(
            "textDocument/didChange",
            new
            {
                textDocument = new { uri, version = 1 },
                contentChanges = new[] { new { text } },
            }
        ).ConfigureAwait(false);

        try
        {
            File.AppendAllText(LogPath,
                $"[{System.Diagnostics.Stopwatch.GetTimestamp(),16}] [SyncService] Sent didChange for {TruncateUri(uri)} ({text.Length} chars)\n");
        }
        catch { }

        return true;
    }

    /// <summary>
    /// Invalidates the cached hash for a URI, forcing the next sync to send
    /// regardless of content.  Call when the server restarts or reconnects.
    /// </summary>
    internal static void Invalidate(string uri)
    {
        _lastSentHashes.TryRemove(uri, out _);
    }

    /// <summary>
    /// Clears all cached hashes.  Call on server shutdown / restart.
    /// </summary>
    internal static void InvalidateAll()
    {
        _lastSentHashes.Clear();
    }

    private static string TruncateUri(string uri) =>
        uri.Length > 60 ? "..." + uri.Substring(uri.Length - 50) : uri;
}
