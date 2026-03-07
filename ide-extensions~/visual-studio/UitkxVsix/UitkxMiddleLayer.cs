using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/// <summary>
/// Intercepts every LSP message between VS2022 and the server.
/// Logs all method names and the full initialize response to %TEMP%\uitkx-middleware.log.
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

    public bool CanHandle(string methodName) => true; // intercept everything

    public async Task HandleNotificationAsync(
        string methodName,
        JToken methodParam,
        Func<JToken, Task> sendNotification
    )
    {
        Log($"NOTIFY  → {methodName}");
        await sendNotification(methodParam);
    }

    public async Task<JToken?> HandleRequestAsync(
        string methodName,
        JToken methodParam,
        Func<JToken, Task<JToken?>> sendRequest
    )
    {
        Log($"REQUEST → {methodName}");
        var result = await sendRequest(methodParam);

        // Log the full initialize result so we can see advertised capabilities
        if (methodName == "initialize" && result != null)
            Log($"InitializeResult (capabilities): {result}");
        else
            Log(
                $"RESPONSE← {methodName}: {result?.ToString(Newtonsoft.Json.Formatting.None)?.Substring(0, Math.Min(300, result?.ToString()?.Length ?? 0))}"
            );

        return result;
    }
}
