using System;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace UitkxLanguageServer.Roslyn;

/// <summary>
/// Runs once the LSP Initialize handshake is complete.
/// Reads <c>rootUri</c> / <c>rootPath</c> from the negotiated params and
/// forwards the workspace root to <see cref="RoslynHost"/> so it can locate
/// Unity's <c>Library/ScriptAssemblies</c> folder and prime its reference cache.
/// </summary>
internal sealed class RoslynHostStartup : IOnLanguageServerStarted
{
    private readonly RoslynHost _host;
    private readonly WorkspaceIndex _index;

    public RoslynHostStartup(RoslynHost host, WorkspaceIndex index)
    {
        _host = host;
        _index = index;
    }

    public Task OnStarted(ILanguageServer server, CancellationToken cancellationToken)
    {
        try
        {
            // OmniSharp exposes the negotiated initialize params here.
            // rootUri is the modern form; rootPath is the legacy string fallback.
            var initParams = server.ClientSettings;

            string? root = null;

            if (initParams?.RootUri is { } rootUri)
            {
                try
                {
                    // RootUri comes in as a URI string (file:///…)
                    var u = new Uri(rootUri.ToString());
                    if (u.IsFile)
                        root = u.LocalPath;
                }
                catch { /* fall through to rootPath */ }
            }

#pragma warning disable CS0618 // RootPath is deprecated but still commonly sent by editors
            if (root is null && initParams?.RootPath is { Length: > 0 } rp)
                root = rp;
#pragma warning restore CS0618

            _host.SetWorkspaceRoot(root);

            ServerLog.Log(
                root is null
                    ? "[RoslynHostStartup] No workspace root received — Roslyn reference discovery limited to BCL."
                    : $"[RoslynHostStartup] Workspace root set to: {root}");

            // Fallback: if WorkspaceIndex.OnStarted didn't manage to scan
            // (e.g. VS2022 sends null rootUri/rootPath for custom content types),
            // trigger the scan now using the root we resolved here.
            if (root is not null)
                _index.EnsureScanned(root);
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[RoslynHostStartup] Error reading workspace root: {ex.Message}");
            _host.SetWorkspaceRoot(null);
        }

        return Task.CompletedTask;
    }
}
