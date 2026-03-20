using System;
using System.IO;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Formatter;
using UitkxLanguageServer.Roslyn;

namespace UitkxLanguageServer;

public sealed class FormattingHandler : IDocumentFormattingHandler
{
    private readonly DocumentStore _store;

    // Singleton Roslyn-backed C# formatter: reused across all formatting requests
    // to avoid repeated AdhocWorkspace allocation.
    private readonly RoslynCSharpFormatter _csharpFormatter = new RoslynCSharpFormatter();

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
        var fileDir = localPath is not null ? Path.GetDirectoryName(localPath) : null;

        // Editor settings serve as the base; config file overrides only
        // the properties it explicitly sets.
        var editorOpts = FormatterOptions.Default with
        {
            IndentSize = (int)request.Options.TabSize,
            UseTabIndent = !request.Options.InsertSpaces,
        };

        var opts = ConfigLoader.LoadFormatterOptions(fileDir, editorOpts);

        ServerLog.Log($"[Formatting] file='{localPath}' editorTabSize={request.Options.TabSize} resolved IndentSize={opts.IndentSize} UseTab={opts.UseTabIndent}");

        var formatter = new AstFormatter(opts, _csharpFormatter);

        string formatted;
        try
        {
            formatted = formatter.Format(text, localPath ?? string.Empty);
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[Formatting] Format error for '{localPath}': {ex.Message}\n{ex.StackTrace}");
            return Task.FromResult<TextEditContainer?>(null);
        }

        if (formatted == text)
        {
            ServerLog.Log($"[Formatting] no-op for '{localPath}' (output == input)");
            return Task.FromResult<TextEditContainer?>(null);
        }

        ServerLog.Log($"[Formatting] applying edit for '{localPath}' (input={text.Length} chars, output={formatted.Length} chars)");

        // Compute a minimal text edit covering only the changed line range.
        // This preserves cursor position far better than replacing the entire document.
        var origLines = text.Split('\n');
        var fmtLines  = formatted.Split('\n');

        // Find first differing line.
        int prefixLen = 0;
        int minLen = Math.Min(origLines.Length, fmtLines.Length);
        while (prefixLen < minLen && origLines[prefixLen] == fmtLines[prefixLen])
            prefixLen++;

        // Find last differing line (from the end).
        int suffixLen = 0;
        while (suffixLen < minLen - prefixLen
            && origLines[origLines.Length - 1 - suffixLen] == fmtLines[fmtLines.Length - 1 - suffixLen])
            suffixLen++;

        int origEnd = origLines.Length - suffixLen;
        int fmtEnd  = fmtLines.Length  - suffixLen;

        // Build the replacement text from the changed lines.
        var changedLines = new string[fmtEnd - prefixLen];
        Array.Copy(fmtLines, prefixLen, changedLines, 0, changedLines.Length);
        var newText = string.Join("\n", changedLines);

        // Compute end column of the last original line being replaced.
        int endLine = origEnd - 1;
        int endChar = endLine >= 0 && endLine < origLines.Length ? origLines[endLine].Length : 0;

        // If the changed range doesn't border the end of the file, we need
        // to include the trailing newline separators in the replacement.
        if (suffixLen > 0 && changedLines.Length > 0)
            newText += "\n";
        if (suffixLen > 0 && changedLines.Length == 0 && prefixLen < origEnd)
            newText = "";

        var edit = new TextEdit
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(prefixLen, 0),
                new Position(endLine, endChar)
            ),
            NewText = newText,
        };

        return Task.FromResult<TextEditContainer?>(new TextEditContainer(edit));
    }

    /// <summary>
    /// Convert a <see cref="OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri"/>
    /// to a local file-system path, or <c>null</c> if it is not a file URI.
    /// </summary>
    private static string? GetLocalPath(
        OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri uri
    )
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
