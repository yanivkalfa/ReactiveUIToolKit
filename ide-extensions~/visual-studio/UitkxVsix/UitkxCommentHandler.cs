using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

/// <summary>
/// Handles Ctrl+K,Ctrl+C (comment) and Ctrl+K,Ctrl+U (uncomment) for .uitkx files.
/// Always uses // line comments regardless of context (code or markup).
/// </summary>
[Export(typeof(ICommandHandler))]
[ContentType("uitkx")]
[Name("UitkxCommentHandler")]
[Order(Before = "default")]
internal sealed class UitkxCommentHandler
    : ICommandHandler<CommentSelectionCommandArgs>,
      ICommandHandler<UncommentSelectionCommandArgs>
{
    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-comment.log"
    );

    private static void Log(string msg)
    {
        try { File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n"); }
        catch { }
    }

    public string DisplayName => "UITKX Comment/Uncomment";

    // ── Comment Selection (Ctrl+K, Ctrl+C) ──────────────────────────────────

    public CommandState GetCommandState(CommentSelectionCommandArgs args) =>
        CommandState.Available;

    public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext ctx)
    {
        Log("CommentSelection command fired");
        return CommentOrUncomment(args.TextView, isComment: true);
    }

    // ── Uncomment Selection (Ctrl+K, Ctrl+U) ────────────────────────────────

    public CommandState GetCommandState(UncommentSelectionCommandArgs args) =>
        CommandState.Available;

    public bool ExecuteCommand(UncommentSelectionCommandArgs args, CommandExecutionContext ctx)
    {
        Log("UncommentSelection command fired");
        return CommentOrUncomment(args.TextView, isComment: false);
    }

    // ── Core logic ──────────────────────────────────────────────────────────

    private bool CommentOrUncomment(ITextView textView, bool isComment)
    {
        var snapshot = textView.TextBuffer.CurrentSnapshot;
        var selection = textView.Selection;
        var caretPos = textView.Caret.Position.BufferPosition;

        // Determine selection range.
        int startPos, endPos;
        if (selection.IsEmpty)
        {
            var line = snapshot.GetLineFromPosition(caretPos.Position);
            startPos = line.Start.Position;
            endPos = line.End.Position;
        }
        else
        {
            startPos = selection.Start.Position.Position;
            endPos = selection.End.Position.Position;
        }

        // Always use line comments regardless of context.
        Log($"selection={startPos}-{endPos}");

        return isComment
            ? CommentLines(textView, snapshot, startPos, endPos)
            : UncommentLines(textView, snapshot, startPos, endPos);
    }

    // ── Line comment: // ─────────────────────────────────────────────────────

    private bool CommentLines(ITextView textView, ITextSnapshot snapshot, int startPos, int endPos)
    {
        var startLine = snapshot.GetLineNumberFromPosition(startPos);
        var endLine = snapshot.GetLineNumberFromPosition(endPos);

        // If selection ends at column 0 of a line, don't include that line.
        if (endPos > startPos)
        {
            var endLineObj = snapshot.GetLineFromLineNumber(endLine);
            if (endPos == endLineObj.Start.Position && endLine > startLine)
                endLine--;
        }

        using var edit = textView.TextBuffer.CreateEdit();
        for (int i = startLine; i <= endLine; i++)
        {
            var line = snapshot.GetLineFromLineNumber(i);
            var lineText = line.GetText();
            if (string.IsNullOrWhiteSpace(lineText))
                continue;

            var indent = lineText.Length - lineText.TrimStart().Length;
            edit.Insert(line.Start.Position + indent, "// ");
        }
        edit.Apply();
        Log($"Commented lines {startLine}-{endLine}");
        return true;
    }

    private bool UncommentLines(ITextView textView, ITextSnapshot snapshot, int startPos, int endPos)
    {
        var startLine = snapshot.GetLineNumberFromPosition(startPos);
        var endLine = snapshot.GetLineNumberFromPosition(endPos);

        if (endPos > startPos)
        {
            var endLineObj = snapshot.GetLineFromLineNumber(endLine);
            if (endPos == endLineObj.Start.Position && endLine > startLine)
                endLine--;
        }

        using var edit = textView.TextBuffer.CreateEdit();
        var commentRegex = new Regex(@"^(\s*)// ?");
        for (int i = startLine; i <= endLine; i++)
        {
            var line = snapshot.GetLineFromLineNumber(i);
            var lineText = line.GetText();
            var match = commentRegex.Match(lineText);
            if (match.Success)
            {
                var newText = lineText.Substring(0, match.Groups[1].Length) +
                              lineText.Substring(match.Length);
                edit.Replace(line.Start.Position, line.Length, newText);
            }
        }
        edit.Apply();
        Log($"Uncommented lines {startLine}-{endLine}");
        return true;
    }

}
