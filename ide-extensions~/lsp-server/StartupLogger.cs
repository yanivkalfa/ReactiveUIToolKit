using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace UitkxLanguageServer;

/// <summary>
/// Logs the full InitializeResult.Capabilities that OmniSharp advertises to VS2022,
/// so we can confirm whether completionProvider / hoverProvider appear in the static caps.
/// </summary>
internal sealed class StartupLogger : IOnLanguageServerStarted
{
    public Task OnStarted(ILanguageServer server, CancellationToken cancellationToken)
    {
        try
        {
            var caps = server.ServerSettings?.Capabilities;
            if (caps is null)
            {
                ServerLog.Log("StartupLogger: ServerSettings.Capabilities is null");
                return Task.CompletedTask;
            }

            var json = JsonConvert.SerializeObject(caps, Formatting.Indented);
            ServerLog.Log(
                $"=== InitializeResult.Capabilities ===\n{json}\n=== END Capabilities ==="
            );
        }
        catch (Exception ex)
        {
            ServerLog.Log($"StartupLogger error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
