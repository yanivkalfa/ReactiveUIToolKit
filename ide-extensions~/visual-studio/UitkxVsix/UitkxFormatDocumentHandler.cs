using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/// <summary>
/// Handles "Format Document" (Ctrl+K, Ctrl+D) for .uitkx files by forwarding
/// the request to the LSP server via InternalRpc.
/// </summary>
[Export(typeof(ICommandHandler))]
[ContentType("uitkx")]
[Name("UitkxFormatDocumentHandler")]
internal sealed class UitkxFormatDocumentHandler : ICommandHandler<FormatDocumentCommandArgs>
{
    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-format.log"
    );

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public string DisplayName => "UITKX Format Document";

    public CommandState GetCommandState(FormatDocumentCommandArgs args) =>
        CommandState.Available;

    public bool ExecuteCommand(FormatDocumentCommandArgs args, CommandExecutionContext executionContext)
    {
        Log("FormatDocument command fired");

        var textView = args.TextView;
        var buffer = textView.TextBuffer;
        if (!buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
        {
            Log("No ITextDocument on buffer");
            return false;
        }

        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
        {
            Log("InternalRpc is null — LSP not connected");
            return false;
        }

        var token = executionContext.OperationContext.UserCancellationToken;

        try
        {
            // Save caret position and viewport so formatting doesn't scroll to top.
            var caretLine = textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;
            var caretCol = textView.Caret.Position.BufferPosition.Position
                         - textView.Caret.Position.BufferPosition.GetContainingLine().Start.Position;
            var firstVisibleLine = textView.TextViewLines?.FirstVisibleLine?.Start
                .GetContainingLine()?.LineNumber ?? 0;

            // Fetch edits from the LSP server on a background thread.
            var editsTask = Task.Run(() => FetchEditsAsync(rpc, buffer, doc.FilePath, token));
            var edits = editsTask.GetAwaiter().GetResult();

            if (edits != null && edits.Count > 0)
            {
                // Apply edits on the current (UI) thread.
                var snapshot = buffer.CurrentSnapshot;
                using var edit = buffer.CreateEdit();
                foreach (var (startPos, endPos, newText) in edits)
                {
                    if (startPos >= 0 && endPos >= startPos && endPos <= snapshot.Length)
                        edit.Replace(Span.FromBounds(startPos, endPos), newText);
                }
                edit.Apply();
                Log($"formatting: applied {edits.Count} edit(s) on UI thread");

                // Restore caret and scroll position.
                var newSnapshot = buffer.CurrentSnapshot;
                if (caretLine < newSnapshot.LineCount)
                {
                    var line = newSnapshot.GetLineFromLineNumber(caretLine);
                    var newCol = Math.Min(caretCol, line.Length);
                    var newPos = new SnapshotPoint(newSnapshot, line.Start.Position + newCol);
                    textView.Caret.MoveTo(newPos);
                }
                if (firstVisibleLine < newSnapshot.LineCount)
                {
                    var topLine = newSnapshot.GetLineFromLineNumber(firstVisibleLine);
                    textView.DisplayTextLineContainingBufferPosition(
                        topLine.Start, 0.0,
                        ViewRelativePosition.Top);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log("FormatDocument cancelled");
        }
        catch (Exception ex)
        {
            Log($"FormatDocument error: {ex.Message}");
        }

        return true; // handled
    }

    /// <summary>
    /// Fetches formatting edits from the LSP server on a background thread.
    /// Returns a list of (startPos, endPos, newText) tuples ready to apply.
    /// </summary>
    private static async Task<List<(int startPos, int endPos, string newText)>?> FetchEditsAsync(
        StreamJsonRpc.JsonRpc rpc,
        ITextBuffer buffer,
        string filePath,
        CancellationToken token
    )
    {
        var uri = new Uri(filePath).AbsoluteUri;
        var snapshot = buffer.CurrentSnapshot;
        var currentText = snapshot.GetText();

        // Sync buffer text to server (VS2022 doesn't send didChange for custom content types).
        await rpc.NotifyWithParameterObjectAsync(
                "textDocument/didChange",
                new
                {
                    textDocument = new { uri, version = 1 },
                    contentChanges = new[] { new { text = currentText } },
                }
            )
            .ConfigureAwait(false);

        Log($"Calling textDocument/formatting for {uri}");

        var result = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                "textDocument/formatting",
                new
                {
                    textDocument = new { uri },
                    options = new { tabSize = 4, insertSpaces = true },
                },
                token
            )
            .ConfigureAwait(false);

        if (result == null || result.Type == JTokenType.Null)
        {
            Log("formatting: null result (no changes)");
            return null;
        }

        var jsonEdits = result as JArray;
        if (jsonEdits == null || jsonEdits.Count == 0)
        {
            Log("formatting: empty edits array");
            return null;
        }

        Log($"formatting: received {jsonEdits.Count} edit(s)");

        // Convert LSP TextEdits to buffer positions while still on the background thread.
        // Use the same snapshot we sent to the server.
        var edits = new List<(int, int, string)>();
        foreach (var te in jsonEdits)
        {
            var range = te["range"];
            var start = range?["start"];
            var end = range?["end"];
            var newText = te["newText"]?.ToString() ?? "";

            var startLine = start?["line"]?.Value<int>() ?? 0;
            var startChar = start?["character"]?.Value<int>() ?? 0;
            var endLine = end?["line"]?.Value<int>() ?? 0;
            var endChar = end?["character"]?.Value<int>() ?? 0;

            var startPos = GetPosition(snapshot, startLine, startChar);
            var endPos = GetPosition(snapshot, endLine, endChar);

            if (startPos >= 0 && endPos >= startPos)
                edits.Add((startPos, endPos, newText));
        }

        return edits;
    }

    private static int GetPosition(ITextSnapshot snapshot, int line, int character)
    {
        if (line < 0 || line >= snapshot.LineCount)
        {
            // Past end of document — clamp to end.
            if (line >= snapshot.LineCount)
                return snapshot.Length;
            return -1;
        }
        var snapshotLine = snapshot.GetLineFromLineNumber(line);
        var pos = snapshotLine.Start.Position + character;
        return Math.Min(pos, snapshotLine.End.Position);
    }
}
