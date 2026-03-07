using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace UitkxLanguageServer;

/// <summary>
/// Handles <c>workspace/didChangeWatchedFiles</c> notifications so that the
/// <see cref="WorkspaceIndex"/> (which tracks <c>*Props.cs</c> files for
/// IntelliSense) stays up-to-date when files are added, removed, or modified
/// without the user explicitly re-opening a <c>.uitkx</c> document.
///
/// The registration options tell the client to watch all <c>*Props.cs</c>
/// files in the workspace; any change notification calls
/// <see cref="WorkspaceIndex.Refresh"/> for the affected path.
/// </summary>
public sealed class WatchedFilesHandler : IDidChangeWatchedFilesHandler
{
    private readonly WorkspaceIndex _index;

    public WatchedFilesHandler(WorkspaceIndex index) => _index = index;

    // ── Registration ──────────────────────────────────────────────────────────

    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(
        DidChangeWatchedFilesCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new DidChangeWatchedFilesRegistrationOptions
        {
            // Ask the client to watch every *Props.cs file in the workspace so
            // we hear about create/change/delete without polling.
            Watchers = new Container<OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher>(
                new OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher
                {
                    GlobPattern = "**/*Props.cs",
                    Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete,
                }
            ),
        };

    // ── Handler ───────────────────────────────────────────────────────────────

    public Task<Unit> Handle(
        DidChangeWatchedFilesParams request,
        CancellationToken cancellationToken
    )
    {
        if (request?.Changes is null)
            return Unit.Task;

        foreach (var change in request.Changes)
        {
            try
            {
                var localPath = GetLocalPath(change.Uri);
                if (localPath is not null)
                {
                    ServerLog.Log($"WatchedFilesHandler: {change.Type} → {localPath}");
                    _index.Refresh(localPath);
                }
            }
            catch (Exception ex)
            {
                ServerLog.Log($"WatchedFilesHandler error for {change.Uri}: {ex.Message}");
            }
        }

        return Unit.Task;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string? GetLocalPath(DocumentUri uri)
    {
        try
        {
            var sysUri = new System.Uri(uri.ToString());
            return sysUri.IsFile ? sysUri.LocalPath : null;
        }
        catch
        {
            return null;
        }
    }
}
