using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

// ── F12 command handler ─────────────────────────────────────────────────────

/// <summary>
/// Handles F12 / Go To Definition for .uitkx files by calling
/// textDocument/definition on the LSP server via InternalRpc.
/// </summary>
[Export(typeof(ICommandHandler))]
[ContentType("uitkx")]
[Name("UitkxGoToDefinitionHandler")]
[Order(Before = "default")]
internal sealed class UitkxGoToDefinitionHandler : ICommandHandler<GoToDefinitionCommandArgs>
{
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public string DisplayName => "UITKX Go To Definition";

    public CommandState GetCommandState(GoToDefinitionCommandArgs args) => CommandState.Available;

    public bool ExecuteCommand(
        GoToDefinitionCommandArgs args,
        CommandExecutionContext executionContext
    )
    {
        Log("GoToDefinition command fired");

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
            var snapshot = buffer.CurrentSnapshot;
            var caretPos = textView.Caret.Position.BufferPosition;
            var lineNo = snapshot.GetLineNumberFromPosition(caretPos.Position);
            var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
            var charNo = caretPos.Position - lineStart;
            var uri = new Uri(doc.FilePath).AbsoluteUri;

            // Link user cancellation with a 10s timeout to prevent infinite hang on cold start.
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                await GoToDefinitionCoreAsync(
                    rpc,
                    uri,
                    snapshot.GetText(),
                    lineNo,
                    charNo,
                    cts.Token
                )
            );
        }
        catch (OperationCanceledException)
        {
            Log("GoToDefinition cancelled");
        }
        catch (Exception ex)
        {
            Log($"GoToDefinition error: {ex.Message}");
        }

        return true;
    }

    /// <summary>Shared helper: calls textDocument/definition and navigates.</summary>
    internal static async Task<bool> GoToDefinitionCoreAsync(
        StreamJsonRpc.JsonRpc rpc,
        string fileUri,
        string currentText,
        int line,
        int character,
        CancellationToken token
    )
    {
        Log($"Calling textDocument/definition: {fileUri} {line}:{character}");

        // Sync buffer to server.
        await rpc.NotifyWithParameterObjectAsync(
                "textDocument/didChange",
                new
                {
                    textDocument = new { uri = fileUri, version = 1 },
                    contentChanges = new[] { new { text = currentText } },
                }
            )
            .ConfigureAwait(false);

        var result = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                "textDocument/definition",
                new { textDocument = new { uri = fileUri }, position = new { line, character } },
                token
            )
            .ConfigureAwait(false);

        if (result == null || result.Type == JTokenType.Null)
        {
            Log("definition: null result");
            return false;
        }

        Log($"definition result: {result.ToString(Newtonsoft.Json.Formatting.None)}");

        JToken? location = null;
        if (result is JArray arr && arr.Count > 0)
            location = arr[0];
        else if (result is JObject)
            location = result;

        if (location == null)
            return false;

        var targetUri = location["targetUri"]?.ToString() ?? location["uri"]?.ToString();
        var targetRange = location["targetRange"] ?? location["range"];

        if (string.IsNullOrEmpty(targetUri) || targetRange == null)
            return false;

        var targetLine = targetRange["start"]?["line"]?.Value<int>() ?? 0;
        var targetChar = targetRange["start"]?["character"]?.Value<int>() ?? 0;

        string targetPath;
        try
        {
            targetPath = new Uri(targetUri!).LocalPath;
        }
        catch
        {
            return false;
        }

        Log($"Navigating to {targetPath}:{targetLine}:{targetChar}");

        await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(
            token
        );

        var dte = (EnvDTE.DTE)
            Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
        if (dte == null)
            return false;

        var window = dte.ItemOperations.OpenFile(targetPath);
        var sel = window?.Document?.Selection as EnvDTE.TextSelection;
        sel?.MoveToLineAndOffset(targetLine + 1, targetChar + 1);
        sel?.MoveToLineAndOffset(targetLine + 1, targetChar + 1);

        Log("definition: navigation completed");
        return true;
    }
}

// ── Ctrl+Click navigable symbol source ──────────────────────────────────────

[Export(typeof(INavigableSymbolSourceProvider))]
[ContentType("uitkx")]
[Name("UitkxNavigableSymbolSource")]
[Order(Before = "default")]
internal sealed class UitkxNavigableSymbolSourceProvider : INavigableSymbolSourceProvider
{
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    public INavigableSymbolSource? TryCreateNavigableSymbolSource(
        ITextView textView,
        ITextBuffer buffer
    )
    {
        try
        {
            File.AppendAllText(
                LogPath,
                $"[{DateTime.UtcNow:O}] TryCreateNavigableSymbolSource called\n"
            );
        }
        catch { }
        return buffer.Properties.GetOrCreateSingletonProperty(() =>
            new UitkxNavigableSymbolSource(buffer)
        );
    }
}

internal sealed class UitkxNavigableSymbolSource : INavigableSymbolSource
{
    private readonly ITextBuffer _buffer;

    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public UitkxNavigableSymbolSource(ITextBuffer buffer) => _buffer = buffer;

    public async Task<INavigableSymbol?> GetNavigableSymbolAsync(
        SnapshotSpan triggerSpan,
        CancellationToken token
    )
    {
        Log($"GetNavigableSymbolAsync called at {triggerSpan.Start.Position}");

        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
        {
            Log("InternalRpc is null");
            return null;
        }

        if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            return null;

        var snapshot = triggerSpan.Snapshot;
        var pos = triggerSpan.Start;
        var lineNo = snapshot.GetLineNumberFromPosition(pos.Position);
        var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
        var charNo = pos.Position - lineStart;
        var uri = new Uri(doc.FilePath).AbsoluteUri;

        try
        {
            var result = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                    "textDocument/definition",
                    new
                    {
                        textDocument = new { uri },
                        position = new { line = lineNo, character = charNo },
                    },
                    token
                )
                .ConfigureAwait(false);

            if (result == null || result.Type == JTokenType.Null)
            {
                Log("GetNavigableSymbolAsync: no definition");
                return null;
            }

            JToken? location =
                result is JArray arr && arr.Count > 0 ? arr[0]
                : result is JObject ? result
                : null;
            if (location == null)
                return null;

            var targetUri = location["targetUri"]?.ToString() ?? location["uri"]?.ToString();
            if (string.IsNullOrEmpty(targetUri))
                return null;

            Log("GetNavigableSymbolAsync: found definition, returning symbol");

            var wordSpan = GetWordSpan(snapshot, pos);
            return new UitkxNavigableSymbol(wordSpan, uri, lineNo, charNo, _buffer);
        }
        catch (Exception ex)
        {
            Log($"GetNavigableSymbolAsync error: {ex.Message}");
            return null;
        }
    }

    private static SnapshotSpan GetWordSpan(ITextSnapshot snapshot, SnapshotPoint point)
    {
        var pos = point.Position;
        var start = pos;
        var end = pos;
        while (start > 0 && IsWordChar(snapshot[start - 1]))
            start--;
        while (end < snapshot.Length && IsWordChar(snapshot[end]))
            end++;
        return new SnapshotSpan(snapshot, Span.FromBounds(start, Math.Max(end, start + 1)));
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-';

    public void Dispose() { }
}

internal sealed class UitkxNavigableSymbol : INavigableSymbol
{
    private readonly string _uri;
    private readonly ITextBuffer _buffer;
    private readonly int _line;
    private readonly int _character;

    public SnapshotSpan SymbolSpan { get; }

    public UitkxNavigableSymbol(
        SnapshotSpan span,
        string uri,
        int line,
        int character,
        ITextBuffer buffer
    )
    {
        SymbolSpan = span;
        _uri = uri;
        _line = line;
        _character = character;
        _buffer = buffer;
    }

    public IEnumerable<INavigableRelationship> Relationships =>
        new[] { PredefinedNavigableRelationships.Definition };

    public void Navigate(INavigableRelationship relationship)
    {
        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
            return;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                await UitkxGoToDefinitionHandler.GoToDefinitionCoreAsync(
                    rpc,
                    _uri,
                    _buffer.CurrentSnapshot.GetText(),
                    _line,
                    _character,
                    cts.Token
                )
            );
        }
        catch { }
    }
}
