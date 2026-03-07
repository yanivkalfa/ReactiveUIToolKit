using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace UitkxLanguageServer;

/// <summary>
/// Minimal text-document sync handler: tracks open / change / close.
/// Full incremental sync is not required — we always use the full document text.
/// </summary>
public sealed class TextSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly DocumentStore _store;
    private readonly DiagnosticsPublisher _diagnostics;

    public TextSyncHandler(DocumentStore store, DiagnosticsPublisher diagnostics)
    {
        _store = store;
        _diagnostics = diagnostics;
    }

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) =>
        new TextDocumentAttributes(uri, "uitkx");

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new TextDocumentSyncRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }
            ),
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false },
        };

    public override Task<Unit> Handle(
        DidOpenTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        ServerLog.Log(
            $"didOpen: {request.TextDocument.Uri}  lang={request.TextDocument.LanguageId}  len={request.TextDocument.Text?.Length}"
        );
        _store.Set(request.TextDocument.Uri, request.TextDocument.Text);
        _diagnostics.Publish(request.TextDocument.Uri, request.TextDocument.Text ?? string.Empty);
        return Unit.Task;
    }

    public override Task<Unit> Handle(
        DidChangeTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        var text = request.ContentChanges.LastOrDefault()?.Text ?? "";
        ServerLog.Log($"didChange: {request.TextDocument.Uri}  len={text.Length}");
        _store.Set(request.TextDocument.Uri, text);
        _diagnostics.Publish(request.TextDocument.Uri, text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(
        DidSaveTextDocumentParams request,
        CancellationToken cancellationToken
    ) => Unit.Task;

    public override Task<Unit> Handle(
        DidCloseTextDocumentParams request,
        CancellationToken cancellationToken
    )
    {
        _store.Remove(request.TextDocument.Uri);
        return Unit.Task;
    }
}
