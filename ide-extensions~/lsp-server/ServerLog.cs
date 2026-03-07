namespace UitkxLanguageServer;

/// <summary>
/// Shared file logger for diagnosing LSP traffic in VS2022.
/// Log path: %TEMP%\uitkx-server.log
/// </summary>
internal static class ServerLog
{
    internal static readonly string Path = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(),
        "uitkx-server.log"
    );

    internal static void Log(string msg) =>
        System.IO.File.AppendAllText(Path, $"[{DateTime.UtcNow:O}] {msg}\n");
}
