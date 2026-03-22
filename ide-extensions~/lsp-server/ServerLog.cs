namespace UitkxLanguageServer;

/// <summary>
/// Shared file logger for diagnosing LSP traffic in VS2022.
/// Log path: %TEMP%\uitkx-server.log
/// Uses FileShare.ReadWrite so multiple server processes can coexist
/// during VS extension restarts without "file in use" errors.
/// </summary>
internal static class ServerLog
{
    internal static readonly string Path = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(),
        "uitkx-server.log"
    );

    private static readonly object s_lock = new();

    internal static void Log(string msg)
    {
        lock (s_lock)
        {
            try
            {
                using var fs = new System.IO.FileStream(
                    Path,
                    System.IO.FileMode.Append,
                    System.IO.FileAccess.Write,
                    System.IO.FileShare.ReadWrite);
                using var sw = new System.IO.StreamWriter(fs);
                sw.Write($"[{DateTime.UtcNow:O}] {msg}\n");
            }
            catch { /* Swallow — logging must never kill the server */ }
        }
    }
}
