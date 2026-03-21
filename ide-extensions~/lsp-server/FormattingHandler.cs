using System;
using System.Collections.Generic;
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

        // Emit per-line TextEdits to preserve cursor position.
        // Normalize \r\n to \n before splitting so line comparison matches the
        // formatter's normalized output.
        var normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var origLines = normalizedText.Split('\n');
        var fmtLines  = formatted.Split('\n');

        var edits = new List<TextEdit>();

        int minLen = Math.Min(origLines.Length, fmtLines.Length);

        bool linesRemoved = origLines.Length > fmtLines.Length;

        // Replace changed lines that exist in both original and formatted.
        // When lines were removed, the last per-line edit must extend through
        // the end of the original document to absorb the deleted lines;
        // otherwise the line-deletion edit would overlap with the last
        // replacement and VS Code silently drops the entire batch.
        for (int i = 0; i < minLen; i++)
        {
            if (origLines[i] != fmtLines[i])
            {
                // Skip edits that only strip trailing whitespace from blank lines.
                // This preserves cursor indentation on the line the user is editing.
                if (origLines[i].TrimEnd().Length == 0 && fmtLines[i].TrimEnd().Length == 0)
                    continue;

                bool isLastCommonLine = linesRemoved && i == minLen - 1;
                int endLine = isLastCommonLine ? origLines.Length - 1 : i;
                int endCol  = isLastCommonLine ? origLines[origLines.Length - 1].Length : origLines[i].Length;
                string newText = isLastCommonLine
                    ? fmtLines[i]  // replacement absorbs trailing deleted lines
                    : fmtLines[i];

                edits.Add(new TextEdit
                {
                    Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                        new Position(i, 0),
                        new Position(endLine, endCol)),
                    NewText = newText,
                });

                if (isLastCommonLine)
                {
                    linesRemoved = false; // absorbed — don't emit a separate delete
                }
            }
        }

        // Handle line count differences.
        if (fmtLines.Length > origLines.Length)
        {
            // Formatter added lines — insert after the last original line.
            var extraLines = new string[fmtLines.Length - origLines.Length];
            Array.Copy(fmtLines, origLines.Length, extraLines, 0, extraLines.Length);
            edits.Add(new TextEdit
            {
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    new Position(origLines.Length - 1, origLines[origLines.Length - 1].Length),
                    new Position(origLines.Length - 1, origLines[origLines.Length - 1].Length)),
                NewText = "\n" + string.Join("\n", extraLines),
            });
        }
        else if (linesRemoved)
        {
            // Lines were removed but no per-line edit in the common range
            // absorbed them (all common lines were identical or skipped).
            // Emit a standalone delete for the trailing original lines.
            int lastFmt = fmtLines.Length - 1;
            int lastOrig = origLines.Length - 1;
            edits.Add(new TextEdit
            {
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    new Position(lastFmt, origLines[lastFmt].Length),
                    new Position(lastOrig, origLines[lastOrig].Length)),
                NewText = "",
            });
        }

        if (edits.Count == 0)
            return Task.FromResult<TextEditContainer?>(null);

        return Task.FromResult<TextEditContainer?>(new TextEditContainer(edits));
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
