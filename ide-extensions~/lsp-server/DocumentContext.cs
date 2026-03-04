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

        /// <summary>Cursor in an attribute value — offer value suggestions based on attribute type.</summary>
        AttributeValue,
    }

    public sealed class Context
    {
        public CompletionKind Kind { get; init; }

        /// <summary>Tag name (lower-cased) when Kind == AttributeName, or empty string.</summary>
        public string TagName { get; init; } = "";

        /// <summary>Attribute name when Kind == AttributeValue, or empty string.</summary>
        public string AttributeName { get; init; } = "";

        /// <summary>The prefix already typed (used to pre-filter the list).</summary>
        public string Prefix { get; init; } = "";
    }

    private static readonly Regex s_tagNameRegex = new(
        @"^([A-Za-z][A-Za-z0-9]*)",
        RegexOptions.Compiled
    );

    private static readonly Regex s_attributeValueRegex = new(
        @"([A-Za-z][A-Za-z0-9\-_]*)\s*=\s*(?:(""[^""]*)|('[^']*)|(\{[^}]*))$",
        RegexOptions.Compiled
    );

    private static readonly Regex s_identifierTailRegex = new(
        @"([A-Za-z][A-Za-z0-9\-_]*)$",
        RegexOptions.Compiled
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

        // --- @-triggered completion: directive vs control-flow
        // Directives (@namespace, @component, etc.) appear only before any markup.
        // Control-flow (@if, @foreach, etc.) appear inside the markup body.
        // Heuristic: if any line before the cursor starts with '<' (a markup tag),
        // we are past the directive header and should offer control-flow.
        var atIdx2 = currentLine.LastIndexOf('@');
        if (atIdx2 >= 0 && currentLine.IndexOf(' ', atIdx2) < 0)
        {
            var atPrefix = currentLine[(atIdx2 + 1)..];
            bool isPastMarkup = before.Split('\n').Any(l => l.TrimStart().StartsWith('<'));
            return new Context
            {
                Kind = isPastMarkup ? CompletionKind.ControlFlowName : CompletionKind.DirectiveName,
                Prefix = atPrefix,
            };
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

        // --- Inside open tag attributes/value: <button text="|  or  <button |
        var ltOpen = before.LastIndexOf('<');
        var gtOpen = before.LastIndexOf('>');
        if (ltOpen >= 0 && ltOpen > gtOpen)
        {
            var openTagContent = before[(ltOpen + 1)..];
            if (openTagContent.StartsWith("/"))
            {
                return new Context { Kind = CompletionKind.None };
            }

            var tagMatch = s_tagNameRegex.Match(openTagContent);
            if (!tagMatch.Success)
            {
                return new Context { Kind = CompletionKind.None };
            }

            var tagName = tagMatch.Groups[1].Value;
            var afterTagName = openTagContent[tagMatch.Length..];

            var valueMatch = s_attributeValueRegex.Match(afterTagName);
            if (valueMatch.Success)
            {
                var attrName = valueMatch.Groups[1].Value;
                var rawPrefix =
                    valueMatch.Groups[2].Success ? valueMatch.Groups[2].Value
                    : valueMatch.Groups[3].Success ? valueMatch.Groups[3].Value
                    : valueMatch.Groups[4].Value;

                var prefix = rawPrefix.Length > 0 ? rawPrefix[1..] : "";

                return new Context
                {
                    Kind = CompletionKind.AttributeValue,
                    TagName = tagName,
                    AttributeName = attrName,
                    Prefix = prefix,
                };
            }

            var attrPrefix = s_identifierTailRegex.Match(afterTagName).Value;
            return new Context
            {
                Kind = CompletionKind.AttributeName,
                TagName = tagName,
                Prefix = attrPrefix,
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
