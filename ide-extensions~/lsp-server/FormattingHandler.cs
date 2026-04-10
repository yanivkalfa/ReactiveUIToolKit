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

        // ── Minimal block diff (Ruff / Prettier style) ──
        // Normalize line endings to LF before diffing so line comparison
        // matches the formatter's normalized output.
        var normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var origLines = normalizedText.Split('\n');
        var fmtLines  = formatted.Split('\n');

        // Scan forward: find first line that differs.
        int firstDiff = 0;
        int minLen = Math.Min(origLines.Length, fmtLines.Length);
        while (firstDiff < minLen && origLines[firstDiff] == fmtLines[firstDiff])
            firstDiff++;

        // Scan backward: find last line that differs.
        // origEnd / fmtEnd are inclusive indices of the last differing line.
        int origEnd = origLines.Length - 1;
        int fmtEnd  = fmtLines.Length - 1;
        while (origEnd > firstDiff && fmtEnd > firstDiff
               && origLines[origEnd] == fmtLines[fmtEnd])
        {
            origEnd--;
            fmtEnd--;
        }

        // If all lines match, nothing to do (shouldn't happen — we already
        // checked formatted == text above, but guard anyway).
        if (firstDiff > origEnd && firstDiff > fmtEnd)
            return Task.FromResult<TextEditContainer?>(null);

        // Build the replacement text from fmtLines[firstDiff..fmtEnd].
        var newText = string.Join("\n", fmtLines, firstDiff, fmtEnd - firstDiff + 1);

        // The single edit replaces origLines[firstDiff..origEnd] with the new text.
        var edit = new TextEdit
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(firstDiff, 0),
                new Position(origEnd, origLines[origEnd].Length)),
            NewText = newText,
        };

        ServerLog.Log($"[Formatting] block diff: firstDiff={firstDiff} origEnd={origEnd} fmtEnd={fmtEnd} (1 edit instead of per-line)");

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
