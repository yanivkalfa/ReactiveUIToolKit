using System.IO;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Formatter;

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

        // Resolve the file's directory so ConfigLoader can walk upward and find
        // uitkx.config.json.  Falls back to default options if the URI is not a
        // local file path.
        var localPath = GetLocalPath(request.TextDocument.Uri);
        var fileDir   = localPath is not null ? Path.GetDirectoryName(localPath) : null;

        var opts      = ConfigLoader.LoadFormatterOptions(fileDir);
        var formatter = new AstFormatter(opts);

        var formatted = formatter.Format(text, localPath ?? string.Empty);

        if (formatted == text)
            return Task.FromResult<TextEditContainer?>(null);

        // Replace the entire document with the formatted version.
        // Use the original text to compute the end position so the range is exact.
        var lines    = text.Split('\n');
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

    /// <summary>
    /// Convert a <see cref="OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri"/>
    /// to a local file-system path, or <c>null</c> if it is not a file URI.
    /// </summary>
    private static string? GetLocalPath(
        OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri uri)
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
