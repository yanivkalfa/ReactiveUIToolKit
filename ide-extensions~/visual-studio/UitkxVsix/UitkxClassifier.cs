using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

[Export(typeof(IClassifierProvider))]
[ContentType("text")]
internal sealed class UitkxClassifierProvider : IClassifierProvider
{
    [Import]
    internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; } = null!;

    public IClassifier? GetClassifier(ITextBuffer textBuffer)
    {
        if (!ShouldClassifyBuffer(textBuffer))
        {
            return null;
        }

        return textBuffer.Properties.GetOrCreateSingletonProperty(
            () => new UitkxClassifier(ClassificationTypeRegistryService)
        );
    }

    private static bool ShouldClassifyBuffer(ITextBuffer textBuffer)
    {
        if (string.Equals(textBuffer.ContentType.TypeName, "uitkx", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
        {
            var filePath = document.FilePath;
            return filePath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}

internal sealed class UitkxClassifier : IClassifier
{
    private readonly IClassificationType _keyword;
    private readonly IClassificationType _string;
    private readonly IClassificationType _number;
    private readonly IClassificationType _identifier;
    private readonly IClassificationType _operator;
    private readonly IClassificationType _method;
    private readonly IClassificationType _type;
    private readonly IClassificationType _comment;
    private readonly IClassificationType _tagName;
    private readonly IClassificationType _attributeName;
    private readonly IClassificationType _directiveName;
    private readonly IClassificationType _controlDirectiveName;
    private readonly IClassificationType _tagDelimiter;
    private readonly IClassificationType _directivePunctuation;

    private readonly HashSet<string> _controlDirectives = new(StringComparer.Ordinal)
    {
        "if", "else", "for", "foreach", "while", "switch", "case", "default", "break", "continue"
    };

    private readonly HashSet<string> _typedDirectives = new(StringComparer.Ordinal)
    {
        "namespace", "component", "using", "props"
    };

    private readonly HashSet<string> _keywords = new(StringComparer.Ordinal)
    {
        "true", "false", "null", "new", "typeof", "nameof", "this", "var", "int", "string", "bool",
        "float", "double", "decimal", "long", "uint", "ulong", "byte", "char", "object", "void", "return",
        "await", "async", "if", "else", "for", "foreach", "while", "do", "break", "continue", "throw",
        "catch", "finally", "using", "static", "readonly", "const", "private", "public", "protected", "internal",
        "class", "interface", "enum", "struct", "record", "is", "as", "in", "out", "ref"
    };

    private ITextSnapshot? _cachedSnapshot;
    private List<ClassificationSpan> _cachedSpans = new();

#pragma warning disable CS0067
    public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
#pragma warning restore CS0067

    public UitkxClassifier(
        IClassificationTypeRegistryService classificationTypeRegistryService
    )
    {
        _keyword = classificationTypeRegistryService.GetClassificationType("keyword")
            ?? classificationTypeRegistryService.GetClassificationType("text")!;
        _string = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.String)
            ?? classificationTypeRegistryService.GetClassificationType("string")
            ?? _keyword;
        _number = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.Number)
            ?? classificationTypeRegistryService.GetClassificationType("number")
            ?? _keyword;
        _identifier = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.Identifier)
            ?? classificationTypeRegistryService.GetClassificationType("identifier")
            ?? classificationTypeRegistryService.GetClassificationType("text")!;
        _operator = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.Operator)
            ?? classificationTypeRegistryService.GetClassificationType("operator")
            ?? _keyword;
        _method = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.Function)
            ?? classificationTypeRegistryService.GetClassificationType("method name")
            ?? _identifier;
        _type = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.TypeName)
            ?? classificationTypeRegistryService.GetClassificationType("class name")
            ?? _method;
        _comment = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.Comment)
            ?? classificationTypeRegistryService.GetClassificationType("comment")
            ?? _identifier;
        _tagName = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.TagName)
            ?? classificationTypeRegistryService.GetClassificationType("xml literal name")
            ?? _type;
        _attributeName = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.AttributeName)
            ?? classificationTypeRegistryService.GetClassificationType("xml literal attribute name")
            ?? _keyword;
        _directiveName = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.DirectiveKeyword)
            ?? classificationTypeRegistryService.GetClassificationType("preprocessor keyword")
            ?? _keyword;
        _controlDirectiveName = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.ControlKeyword)
            ?? classificationTypeRegistryService.GetClassificationType("preprocessor keyword")
            ?? _directiveName;
        _tagDelimiter = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.TagDelimiter)
            ?? classificationTypeRegistryService.GetClassificationType("xml literal delimiter")
            ?? _operator;
        _directivePunctuation = classificationTypeRegistryService.GetClassificationType(UitkxClassificationNames.DirectivePunctuation)
            ?? _operator;
    }

    public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
    {
        EnsureSnapshotClassified(span.Snapshot);

        var results = new List<ClassificationSpan>();
        foreach (var classificationSpan in _cachedSpans)
        {
            if (classificationSpan.Span.IntersectsWith(span))
            {
                results.Add(classificationSpan);
            }
        }

        return results;
    }

    private void EnsureSnapshotClassified(ITextSnapshot snapshot)
    {
        if (ReferenceEquals(snapshot, _cachedSnapshot))
        {
            return;
        }

        _cachedSnapshot = snapshot;
        _cachedSpans = ClassifyAll(snapshot);
    }

    private List<ClassificationSpan> ClassifyAll(ITextSnapshot snapshot)
    {
        var text = snapshot.GetText();
        var spans = new List<ClassificationSpan>();

        var index = 0;
        while (index < text.Length)
        {
            if (TryClassifyLineComment(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (TryClassifyDirective(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (TryClassifyMarkupTag(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (TryClassifyBraceExpression(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (TryClassifyInterpolatedString(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (TryClassifyNormalString(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (char.IsDigit(text[index]))
            {
                var start = index;
                index = ParseNumber(text, index);
                AddSpan(snapshot, spans, start, index - start, _number);
                continue;
            }

            if (IsIdentifierStart(text[index]))
            {
                var start = index;
                index = ParseIdentifier(text, index);
                var token = text.Substring(start, index - start);

                if (_keywords.Contains(token))
                {
                    AddSpan(snapshot, spans, start, token.Length, _keyword);
                }
                else
                {
                    var probe = index;
                    while (probe < text.Length && char.IsWhiteSpace(text[probe]))
                    {
                        probe++;
                    }

                    if (probe < text.Length && text[probe] == '(')
                    {
                        AddSpan(snapshot, spans, start, token.Length, _method);
                    }
                    else
                    {
                        AddSpan(snapshot, spans, start, token.Length, _identifier);
                    }
                }

                continue;
            }

            if (IsOperatorChar(text[index]))
            {
                AddSpan(snapshot, spans, index, 1, _operator);
            }

            index++;
        }

        return spans;
    }

    private bool TryClassifyLineComment(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (index + 1 >= text.Length || text[index] != '/' || text[index + 1] != '/')
        {
            return false;
        }

        var start = index;
        index += 2;
        while (index < text.Length && text[index] != '\r' && text[index] != '\n')
        {
            index++;
        }

        AddSpan(snapshot, spans, start, index - start, _comment);
        return true;
    }

    private bool TryClassifyDirective(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (text[index] != '@')
        {
            return false;
        }

        if (index + 1 >= text.Length || !IsIdentifierStart(text[index + 1]))
        {
            return false;
        }

        AddSpan(snapshot, spans, index, 1, _directivePunctuation);
        var directiveStart = index + 1;
        var directiveEnd = ParseIdentifier(text, directiveStart);
        var directiveToken = text.Substring(directiveStart, directiveEnd - directiveStart);
        var directiveType = _controlDirectives.Contains(directiveToken) ? _controlDirectiveName : _directiveName;
        AddSpan(snapshot, spans, directiveStart, directiveEnd - directiveStart, directiveType);
        index = directiveEnd;

        while (index < text.Length)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index++;
                continue;
            }

            if (text[index] == '.')
            {
                AddSpan(snapshot, spans, index, 1, _operator);
                index++;
                continue;
            }

            if (text[index] == '(')
            {
                ClassifyParenthesizedExpression(snapshot, text, ref index, spans);
                continue;
            }

            if (text[index] == '\r' || text[index] == '\n' || text[index] == '{' || text[index] == '<')
            {
                break;
            }

            if (TryClassifyInterpolatedString(snapshot, text, ref index, spans)
                || TryClassifyNormalString(snapshot, text, ref index, spans)
                || TryClassifyBraceExpression(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (IsIdentifierStart(text[index]))
            {
                var tokenStart = index;
                index = ParseIdentifier(text, index);
                var tokenType = _typedDirectives.Contains(directiveToken) ? _type : _identifier;
                AddSpan(snapshot, spans, tokenStart, index - tokenStart, tokenType);
                continue;
            }

            break;
        }

        return true;
    }

    private bool TryClassifyMarkupTag(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (text[index] != '<')
        {
            return false;
        }

        if (index + 1 >= text.Length)
        {
            return false;
        }

        var probe = index + 1;
        if (probe < text.Length && text[probe] == '/')
        {
            probe++;
        }

        if (probe >= text.Length || !IsMarkupNameStart(text[probe]))
        {
            return false;
        }

        AddSpan(snapshot, spans, index, 1, _tagDelimiter);
        index++;

        if (index < text.Length && text[index] == '/')
        {
            AddSpan(snapshot, spans, index, 1, _tagDelimiter);
            index++;
        }

        var tagNameStart = index;
        index = ParseMarkupName(text, index);
        AddSpan(snapshot, spans, tagNameStart, index - tagNameStart, _tagName);

        while (index < text.Length)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                index++;
                continue;
            }

            if (TryClassifyLineComment(snapshot, text, ref index, spans)
                || TryClassifyInterpolatedString(snapshot, text, ref index, spans)
                || TryClassifyNormalString(snapshot, text, ref index, spans)
                || TryClassifyBraceExpression(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '>')
            {
                AddSpan(snapshot, spans, index, 1, _tagDelimiter);
                AddSpan(snapshot, spans, index + 1, 1, _tagDelimiter);
                index += 2;
                return true;
            }

            if (text[index] == '>')
            {
                AddSpan(snapshot, spans, index, 1, _tagDelimiter);
                index++;
                return true;
            }

            if (text[index] == '=')
            {
                AddSpan(snapshot, spans, index, 1, _operator);
                index++;
                continue;
            }

            if (IsMarkupNameStart(text[index]))
            {
                var attrStart = index;
                index = ParseMarkupName(text, index);
                AddSpan(snapshot, spans, attrStart, index - attrStart, _attributeName);
                continue;
            }

            if (IsOperatorChar(text[index]))
            {
                AddSpan(snapshot, spans, index, 1, _operator);
            }

            index++;
        }

        return true;
    }

    private bool TryClassifyBraceExpression(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (text[index] != '{')
        {
            return false;
        }

        if (!ShouldTreatBraceAsExpression(text, index))
        {
            return false;
        }

        AddSpan(snapshot, spans, index, 1, _operator);
        index++;

        var exprStart = index;
        var depth = 1;

        while (index < text.Length && depth > 0)
        {
            if (TryClassifyInterpolatedString(snapshot, text, ref index, spans)
                || TryClassifyNormalString(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (text[index] == '{')
            {
                depth++;
                index++;
                continue;
            }

            if (text[index] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    if (index > exprStart)
                    {
                        ClassifyExpressionSegment(snapshot, text, exprStart, index - exprStart, spans);
                    }

                    AddSpan(snapshot, spans, index, 1, _operator);
                    index++;
                    return true;
                }

                index++;
                continue;
            }

            index++;
        }

        if (exprStart < text.Length)
        {
            ClassifyExpressionSegment(snapshot, text, exprStart, text.Length - exprStart, spans);
        }

        return true;
    }

    private void ClassifyParenthesizedExpression(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        var parenStart = index;
        AddSpan(snapshot, spans, parenStart, 1, _operator);
        index++;

        var exprStart = index;
        var depth = 1;
        while (index < text.Length && depth > 0)
        {
            if (TryClassifyInterpolatedString(snapshot, text, ref index, spans)
                || TryClassifyNormalString(snapshot, text, ref index, spans))
            {
                continue;
            }

            if (text[index] == '(')
            {
                depth++;
                index++;
                continue;
            }

            if (text[index] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    if (index > exprStart)
                    {
                        ClassifyExpressionSegment(snapshot, text, exprStart, index - exprStart, spans);
                    }

                    AddSpan(snapshot, spans, index, 1, _operator);
                    index++;
                    return;
                }

                index++;
                continue;
            }

            index++;
        }

        if (exprStart < text.Length)
        {
            ClassifyExpressionSegment(snapshot, text, exprStart, text.Length - exprStart, spans);
        }
    }

    private bool TryClassifyInterpolatedString(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (index + 1 >= text.Length || text[index] != '$' || text[index + 1] != '"')
        {
            return false;
        }

        AddSpan(snapshot, spans, index, 2, _string);
        index += 2;
        var segmentStart = index;

        while (index < text.Length)
        {
            if (text[index] == '\\')
            {
                index += 2;
                continue;
            }

            if (text[index] == '"')
            {
                if (index > segmentStart)
                {
                    AddSpan(snapshot, spans, segmentStart, index - segmentStart, _string);
                }

                AddSpan(snapshot, spans, index, 1, _string);
                index++;
                return true;
            }

            if (text[index] == '{')
            {
                if (index > segmentStart)
                {
                    AddSpan(snapshot, spans, segmentStart, index - segmentStart, _string);
                }

                AddSpan(snapshot, spans, index, 1, _operator);
                index++;

                var holeStart = index;
                var depth = 1;

                while (index < text.Length && depth > 0)
                {
                    if (text[index] == '\\')
                    {
                        index += 2;
                        continue;
                    }

                    if (text[index] == '{')
                    {
                        depth++;
                        index++;
                        continue;
                    }

                    if (text[index] == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            if (index > holeStart)
                            {
                                ClassifyExpressionSegment(snapshot, text, holeStart, index - holeStart, spans);
                            }

                            AddSpan(snapshot, spans, index, 1, _operator);
                            index++;
                            segmentStart = index;
                            break;
                        }
                    }

                    index++;
                }

                continue;
            }

            index++;
        }

        if (segmentStart < text.Length)
        {
            AddSpan(snapshot, spans, segmentStart, text.Length - segmentStart, _string);
        }

        return true;
    }

    private bool TryClassifyNormalString(
        ITextSnapshot snapshot,
        string text,
        ref int index,
        List<ClassificationSpan> spans)
    {
        if (text[index] != '"')
        {
            return false;
        }

        var start = index;
        index++;
        while (index < text.Length)
        {
            if (text[index] == '\\')
            {
                index += 2;
                continue;
            }

            if (text[index] == '"')
            {
                index++;
                break;
            }

            index++;
        }

        AddSpan(snapshot, spans, start, index - start, _string);
        return true;
    }

    private void ClassifyExpressionSegment(
        ITextSnapshot snapshot,
        string text,
        int start,
        int length,
        List<ClassificationSpan> spans)
    {
        var end = start + length;
        var i = start;

        while (i < end)
        {
            if (i < text.Length && char.IsDigit(text[i]))
            {
                var numStart = i;
                i = ParseNumber(text, i);
                AddSpan(snapshot, spans, numStart, i - numStart, _number);
                continue;
            }

            if (i < text.Length && IsIdentifierStart(text[i]))
            {
                var identStart = i;
                i = ParseIdentifier(text, i);
                var token = text.Substring(identStart, i - identStart);

                if (_keywords.Contains(token))
                {
                    AddSpan(snapshot, spans, identStart, token.Length, _keyword);
                }
                else
                {
                    var probe = i;
                    while (probe < end && char.IsWhiteSpace(text[probe]))
                    {
                        probe++;
                    }

                    if (probe < end && text[probe] == '(')
                    {
                        AddSpan(snapshot, spans, identStart, token.Length, _method);
                    }
                    else
                    {
                        AddSpan(snapshot, spans, identStart, token.Length, _identifier);
                    }
                }

                continue;
            }

            if (i < text.Length && IsOperatorChar(text[i]))
            {
                AddSpan(snapshot, spans, i, 1, _operator);
            }

            i++;
        }
    }

    private static int ParseNumber(string text, int index)
    {
        var i = index;

        while (i < text.Length && char.IsDigit(text[i]))
        {
            i++;
        }

        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && char.IsDigit(text[i]))
            {
                i++;
            }
        }

        while (i < text.Length && char.IsLetter(text[i]))
        {
            i++;
        }

        return i;
    }

    private static int ParseIdentifier(string text, int index)
    {
        var i = index;
        while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
        {
            i++;
        }

        return i;
    }

    private static int ParseMarkupName(string text, int index)
    {
        var i = index;
        while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_' || text[i] == '-' || text[i] == ':' || text[i] == '.'))
        {
            i++;
        }

        return i;
    }

    private static bool IsIdentifierStart(char c)
        => char.IsLetter(c) || c == '_';

    private static bool IsMarkupNameStart(char c)
        => char.IsLetter(c) || c == '_';

    private static bool IsOperatorChar(char c)
        => c is '+' or '-' or '*' or '/' or '%' or '&' or '|' or '^' or '~' or '!' or '<' or '>' or '=' or '?' or ':' or '{' or '}' or '(' or ')';

    private static bool ShouldTreatBraceAsExpression(string text, int braceIndex)
    {
        var prev = braceIndex - 1;
        while (prev >= 0 && char.IsWhiteSpace(text[prev]))
        {
            prev--;
        }

        if (prev < 0)
        {
            return false;
        }

        return text[prev] is '=' or '(' or ',' or '?' or ':';
    }

    private static void AddSpan(
        ITextSnapshot snapshot,
        ICollection<ClassificationSpan> spans,
        int start,
        int length,
        IClassificationType classificationType)
    {
        if (length <= 0 || start < 0 || start + length > snapshot.Length)
        {
            return;
        }

        spans.Add(new ClassificationSpan(new SnapshotSpan(snapshot, start, length), classificationType));
    }
}
