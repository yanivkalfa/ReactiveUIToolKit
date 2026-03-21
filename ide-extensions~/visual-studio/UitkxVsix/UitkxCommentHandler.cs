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
/// In @code blocks: uses // line comments.
/// In markup: uses {/* */} JSX-style block comments.
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

        var text = snapshot.GetText();
        var selectedText = snapshot.GetText(startPos, endPos - startPos);

        // Determine whether we're in a @code block.
        var inCode = IsInsideCodeBlock(text, startPos);
        var isMarkup = LooksLikeMarkupSelection(selectedText);

        Log($"inCode={inCode}, isMarkup={isMarkup}, selection={startPos}-{endPos}");

        if (inCode && !isMarkup)
        {
            return isComment
                ? CommentLines(textView, snapshot, startPos, endPos)
                : UncommentLines(textView, snapshot, startPos, endPos);
        }
        else
        {
            return isComment
                ? CommentBlock(textView, snapshot, startPos, endPos)
                : UncommentBlock(textView, snapshot, startPos, endPos);
        }
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

    // ── Block comment: {/* */} ───────────────────────────────────────────────

    private bool CommentBlock(ITextView textView, ITextSnapshot snapshot, int startPos, int endPos)
    {
        using var edit = textView.TextBuffer.CreateEdit();
        edit.Insert(startPos, "{/* ");
        edit.Insert(endPos, " */}");
        edit.Apply();
        Log("Block-commented selection");
        return true;
    }

    private bool UncommentBlock(ITextView textView, ITextSnapshot snapshot, int startPos, int endPos)
    {
        var selectedText = snapshot.GetText(startPos, endPos - startPos);
        var trimmed = selectedText.Trim();

        if (trimmed.StartsWith("{/*") && trimmed.EndsWith("*/}"))
        {
            var openIdx = selectedText.IndexOf("{/*", StringComparison.Ordinal);
            var closeIdx = selectedText.LastIndexOf("*/}", StringComparison.Ordinal);
            if (openIdx >= 0 && closeIdx >= openIdx)
            {
                var inner = selectedText.Substring(openIdx + 3, closeIdx - (openIdx + 3));
                if (inner.StartsWith(" ")) inner = inner.Substring(1);
                if (inner.EndsWith(" ")) inner = inner.Substring(0, inner.Length - 1);

                var newText = selectedText.Substring(0, openIdx) + inner + selectedText.Substring(closeIdx + 3);
                using var edit = textView.TextBuffer.CreateEdit();
                edit.Replace(startPos, endPos - startPos, newText);
                edit.Apply();
                Log("Block-uncommented selection");
                return true;
            }
        }

        // Fallback: try line uncomment
        return UncommentLines(textView, snapshot, startPos, endPos);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Detects whether a given offset is inside a @code { } block,
    /// accounting for strings, char literals, and comments.
    /// </summary>
    private static bool IsInsideCodeBlock(string text, int targetOffset)
    {
        bool inCode = false;
        bool awaitingCodeBrace = false;
        int codeBraceDepth = 0;

        bool inLineComment = false;
        bool inBlockComment = false;
        bool inString = false;
        bool inChar = false;
        bool isVerbatimString = false;

        for (int i = 0; i < text.Length && i < targetOffset; i++)
        {
            char ch = text[i];
            char next = i + 1 < text.Length ? text[i + 1] : '\0';

            if (inLineComment)
            {
                if (ch == '\n') inLineComment = false;
                continue;
            }

            if (inBlockComment)
            {
                if (ch == '*' && next == '/')
                {
                    inBlockComment = false;
                    i++;
                }
                continue;
            }

            if (inString)
            {
                if (isVerbatimString)
                {
                    if (ch == '"' && next == '"') { i++; continue; }
                    if (ch == '"') { inString = false; isVerbatimString = false; }
                }
                else
                {
                    if (ch == '\\') { i++; continue; }
                    if (ch == '"') inString = false;
                }
                continue;
            }

            if (inChar)
            {
                if (ch == '\\') { i++; continue; }
                if (ch == '\'') inChar = false;
                continue;
            }

            if (ch == '/' && next == '/') { inLineComment = true; i++; continue; }
            if (ch == '/' && next == '*') { inBlockComment = true; i++; continue; }
            if (ch == '\'') { inChar = true; continue; }

            if (ch == '"')
            {
                inString = true;
                isVerbatimString = false;
                continue;
            }

            if ((ch == '@' || ch == '$') && next == '"')
            {
                inString = true;
                isVerbatimString = ch == '@';
                i++;
                continue;
            }

            if ((ch == '@' || ch == '$') && i + 2 < text.Length &&
                (text[i + 1] == '@' || text[i + 1] == '$') && text[i + 2] == '"')
            {
                inString = true;
                isVerbatimString = ch == '@' || text[i + 1] == '@';
                i += 2;
                continue;
            }

            if (!inCode)
            {
                if (!awaitingCodeBrace && ch == '@' && i + 5 <= text.Length &&
                    text.Substring(i + 1, Math.Min(4, text.Length - i - 1)) == "code")
                {
                    bool prevOk = i == 0 || !IsIdentChar(text[i - 1]);
                    bool afterOk = i + 5 >= text.Length || !IsIdentChar(text[i + 5]);
                    if (prevOk && afterOk)
                    {
                        awaitingCodeBrace = true;
                        i += 4;
                        continue;
                    }
                }

                if (awaitingCodeBrace && ch == '{')
                {
                    inCode = true;
                    codeBraceDepth = 1;
                    awaitingCodeBrace = false;
                }
                continue;
            }

            if (ch == '{') { codeBraceDepth++; continue; }
            if (ch == '}')
            {
                codeBraceDepth--;
                if (codeBraceDepth <= 0)
                {
                    inCode = false;
                    codeBraceDepth = 0;
                }
            }
        }

        return inCode;
    }

    private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// Checks if selected text looks like markup (lines start with tags or JSX comments).
    /// </summary>
    private static bool LooksLikeMarkupSelection(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        bool anyNonEmpty = false;
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            anyNonEmpty = true;
            if (!line.StartsWith("<") && !line.StartsWith("</") &&
                !line.StartsWith("{/*") && !line.StartsWith("*/}"))
                return false;
        }
        return anyNonEmpty;
    }
}
