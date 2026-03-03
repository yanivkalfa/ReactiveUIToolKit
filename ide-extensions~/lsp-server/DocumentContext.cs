using System.Text.RegularExpressions;

namespace UitkxLanguageServer;

/// <summary>
/// Lightweight context scanner: given document text + cursor position,
/// determines what kind of completion is appropriate at that point.
/// </summary>
public static class DocumentContext
{
    public enum CompletionKind
    {
        None,

        /// <summary>Cursor immediately after '@' at line start — offer directive names.</summary>
        DirectiveName,

        /// <summary>Cursor immediately after '@' mid-markup — offer control-flow names.</summary>
        ControlFlowName,

        /// <summary>Cursor right after '&lt;' — offer element tag names.</summary>
        TagName,

        /// <summary>Cursor inside an open tag after the tag name — offer attributes.</summary>
        AttributeName,
    }

    public sealed class Context
    {
        public CompletionKind Kind { get; init; }

        /// <summary>Tag name (lower-cased) when Kind == AttributeName, or empty string.</summary>
        public string TagName { get; init; } = "";

        /// <summary>The prefix already typed (used to pre-filter the list).</summary>
        public string Prefix { get; init; } = "";
    }

    private static readonly Regex s_openTagRegex = new(
        @"<([A-Za-z][A-Za-z0-9]*)\s+(?:[^>]*)$",
        RegexOptions.Compiled | RegexOptions.RightToLeft
    );

    /// <summary>
    /// Analyses the text up to <paramref name="offset"/> (exclusive) and returns the context.
    /// </summary>
    public static Context Detect(string documentText, int offset)
    {
        if (offset <= 0 || offset > documentText.Length)
            return new Context { Kind = CompletionKind.None };

        var before = documentText[..offset];

        // Split into current line for '@' detection
        var lineStart = before.LastIndexOf('\n') + 1;
        var currentLine = before[lineStart..];

        // --- @directive at position 0 on the line
        if (currentLine.StartsWith('@') && !currentLine.Contains(' '))
        {
            var atPrefix = currentLine[1..]; // text after '@'
            // Decide: directive vs control-flow based on whether '@' is the first non-whitespace
            bool isDeclarationLine =
                lineStart == 0
                || string.IsNullOrWhiteSpace(
                    documentText[
                        (before.LastIndexOf('\n', lineStart > 0 ? lineStart - 1 : 0))..lineStart
                    ]
                        .Trim('\n', '\r')
                );
            return new Context
            {
                Kind = isDeclarationLine
                    ? CompletionKind.DirectiveName
                    : CompletionKind.ControlFlowName,
                Prefix = atPrefix,
            };
        }

        // --- After '@' anywhere in markup (@if, @foreach, etc.)
        if (currentLine.Contains('@') && currentLine.IndexOf(' ', currentLine.LastIndexOf('@')) < 0)
        {
            var atIdx = currentLine.LastIndexOf('@');
            var atPrefix = currentLine[(atIdx + 1)..];
            return new Context { Kind = CompletionKind.ControlFlowName, Prefix = atPrefix };
        }

        // --- Tag name  <|
        var afterLastLt = before.TrimEnd();
        if (afterLastLt.EndsWith('<'))
            return new Context { Kind = CompletionKind.TagName, Prefix = "" };

        // --- Tag name still being typed  <lab|
        var ltIdx = before.LastIndexOf('<');
        if (ltIdx >= 0)
        {
            var afterLt = before[(ltIdx + 1)..];
            // No spaces yet → still typing tag name
            if (!afterLt.Contains(' ') && !afterLt.Contains('>') && !afterLt.Contains('/'))
                return new Context { Kind = CompletionKind.TagName, Prefix = afterLt };
        }

        // --- Inside open tag attributes  <button text="|  or  <button |
        var m = s_openTagRegex.Match(before);
        if (m.Success)
        {
            var tagName = m.Groups[1].Value;
            // Extract the attribute-name prefix the user is typing
            var rest = before[(m.Index + m.Length)..];
            var attrPfx = Regex.Match(rest, @"([A-Za-z][A-Za-z0-9\-_]*)$").Value;
            return new Context
            {
                Kind = CompletionKind.AttributeName,
                TagName = tagName,
                Prefix = attrPfx,
            };
        }

        return new Context { Kind = CompletionKind.None };
    }

    /// <summary>
    /// Returns the word (element tag or attribute name) at the given offset.
    /// </summary>
    public static (string word, string? tagContext) WordAt(string documentText, int offset)
    {
        if (offset > documentText.Length)
            offset = documentText.Length;

        // Expand the word boundary around offset
        var start = offset;
        while (start > 0 && IsWordChar(documentText[start - 1]))
            start--;
        var end = offset;
        while (end < documentText.Length && IsWordChar(documentText[end]))
            end++;

        var word = documentText[start..end];

        // Try to find the enclosing tag for attribute hover context
        string? tagCtx = null;
        var ltIdx = documentText.LastIndexOf('<', start > 0 ? start - 1 : 0);
        if (ltIdx >= 0)
        {
            var afterLt = documentText[(ltIdx + 1)..];
            var firstWord = Regex.Match(afterLt, @"^([A-Za-z][A-Za-z0-9]*)");
            if (firstWord.Success)
                tagCtx = firstWord.Groups[1].Value.ToLowerInvariant();
        }

        return (word, tagCtx);
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '-' || c == '_';
}
