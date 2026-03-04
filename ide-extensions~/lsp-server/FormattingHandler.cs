using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace UitkxLanguageServer;

public sealed class FormattingHandler : IDocumentFormattingHandler
{
    private readonly DocumentStore _store;

    public FormattingHandler(DocumentStore store) => _store = store;

    public DocumentFormattingRegistrationOptions GetRegistrationOptions(
        DocumentFormattingCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new DocumentFormattingRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }
            ),
        };

    public Task<TextEditContainer?> Handle(
        DocumentFormattingParams request,
        CancellationToken cancellationToken
    )
    {
        if (!_store.TryGet(request.TextDocument.Uri, out var text))
            return Task.FromResult<TextEditContainer?>(null);

        var formatted = UitkxFormatter.Format(text);

        if (formatted == text)
            return Task.FromResult<TextEditContainer?>(null);

        // Replace the entire document with the formatted version
        var lines = text.Split('\n');
        var lastLine = lines.Length - 1;
        var lastChar = lines[lastLine].Length;

        var edit = new TextEdit
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(0, 0),
                new Position(lastLine, lastChar)
            ),
            NewText = formatted,
        };

        return Task.FromResult<TextEditContainer?>(new TextEditContainer(edit));
    }
}
