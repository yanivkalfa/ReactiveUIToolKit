using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUITK.SourceGenerator.Nodes;

namespace ReactiveUITK.SourceGenerator.Parser
{
    /// <summary>
    /// Recursive descent parser for UITKX markup.
    ///
    /// Takes the portion of the .uitkx source after the <see cref="DirectiveParser"/>
    /// has consumed all top-level directives and produces a flat list of
    /// <see cref="AstNode"/> instances representing the root of the file.
    ///
    /// Supported grammar:
    /// <code>
    ///   Content      = (Element | IfBlock | ForeachBlock | SwitchBlock |
    ///                   CodeBlock | InlineExpr | Text | HtmlComment)*
    ///   Element      = lt TagName Attribute* ('/>' | '>' Content lt '/' TagName '>')
    ///   Attribute    = Name '=' ('"' Value '"' | '{' CSharpExpr '}') | Name
    ///   IfBlock      = '@if' Paren BraceContent ('@else' ('if' Paren)? BraceContent)*
    ///   BraceContent = '{' Content '}'
    ///   ForeachBlock = '@foreach' '(' Decl 'in' Expr ')' BraceContent
    ///   SwitchBlock  = '@switch' Paren '{' ('@case' Expr ':' | '@default' ':') Content* '}'
    ///   CodeBlock    = '@code' '{' CSharpCode '}'
    ///   InlineExpr   = '@(' CSharpExpr ')'
    /// </code>
    ///
    /// Error recovery: on parse errors the parser emits a diagnostic and skips
    /// to the next recoverable position rather than aborting the whole file.
    /// </summary>
    internal sealed class UitkxParser
    {
        private readonly string _source;
        private readonly string _filePath;

        // Non-readonly so LookAheadIsElse-based @else processing can re-create the
        // scanner after controlled lookahead (though current impl avoids it).
        private MarkupTokenizer _scanner;
        private readonly List<Diagnostic> _diagnostics;

        // ── Construction ──────────────────────────────────────────────────────

        private UitkxParser(
            string source,
            string filePath,
            int startPos,
            int startLine,
            List<Diagnostic> diagnostics
        )
        {
            _source = source;
            _filePath = filePath;
            _scanner = new MarkupTokenizer(source, startPos, startLine);
            _diagnostics = diagnostics;
        }

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Parses the markup section (everything at or after
        /// <see cref="DirectiveSet.MarkupStartIndex"/>) and returns root AST nodes.
        /// </summary>
        public static ImmutableArray<AstNode> Parse(
            string source,
            string filePath,
            DirectiveSet directives,
            List<Diagnostic> diagnostics
        )
        {
            var parser = new UitkxParser(
                source,
                filePath,
                directives.MarkupStartIndex,
                directives.MarkupStartLine,
                diagnostics
            );

            return parser
                .ParseContent(stopTag: null, stopAtBrace: false, stopAtCase: false)
                .ToImmutableArray();
        }

        // ── Content loop ──────────────────────────────────────────────────────

        /// <summary>
        /// Parses zero or more nodes until a stop condition is met.
        /// </summary>
        /// <param name="stopTag">Stop (without consuming) at matching closing tag.</param>
        /// <param name="stopAtBrace">Stop (without consuming) at bare '}'.</param>
        /// <param name="stopAtCase">Stop (without consuming) at '@case' or '@default'.</param>
        private List<AstNode> ParseContent(string? stopTag, bool stopAtBrace, bool stopAtCase)
        {
            var nodes = new List<AstNode>();

            while (!_scanner.IsEof)
            {
                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsEof)
                    break;

                int positionBefore = _scanner.Pos;
                char c = _scanner.Current;

                // ── Stop: bare '}' ──────────────────────────────────────────
                if (stopAtBrace && c == '}')
                    break;

                // ── Stop: @case / @default inside @switch content ───────────
                if (stopAtCase && c == '@')
                {
                    string? kw = PeekDirectiveKeyword();
                    if (kw == "case" || kw == "default")
                        break;
                }

                // ── Stop: matching closing tag ──────────────────────────────
                if (c == '<' && PeekChar(1) == '/')
                {
                    string closing = PeekClosingTagName();
                    if (
                        stopTag == null
                        || string.Equals(closing, stopTag, StringComparison.Ordinal)
                    )
                        break;
                    // Closing tag does not match expected — emit error and skip '<'
                    _diagnostics.Add(
                        MakeError(
                            UitkxDiagnostics.MismatchedClosingTag,
                            closing,
                            stopTag ?? "?",
                            _scanner.Line,
                            _filePath
                        )
                    );
                    _scanner.Advance();
                    continue;
                }

                // ── HTML comment <!-- ... --> ───────────────────────────────
                if (_scanner.TrySkipHtmlComment())
                    continue;

                // ── Opening element <Tag ────────────────────────────────────
                if (c == '<' && PeekChar(1) != '/')
                {
                    var elem = ParseElement();
                    if (elem != null)
                        nodes.Add(elem);
                    continue;
                }

                // ── Directive / control flow / inline @(expr) ───────────────
                if (c == '@')
                {
                    int atLine = _scanner.Line;
                    _scanner.Advance(); // consume '@'

                    if (!_scanner.IsEof && _scanner.Current == '(')
                    {
                        string inlineExpr = _scanner.ReadParenExpression();
                        nodes.Add(new ExpressionNode(inlineExpr, atLine, _filePath));
                        continue;
                    }

                    string keyword = _scanner.ReadIdentifier();

                    switch (keyword)
                    {
                        case "if":
                            var ifNode = ParseIf(atLine);
                            if (ifNode != null)
                                nodes.Add(ifNode);
                            break;
                        case "foreach":
                            var feNode = ParseForeach(atLine);
                            if (feNode != null)
                                nodes.Add(feNode);
                            break;
                        case "switch":
                            var swNode = ParseSwitch(atLine);
                            if (swNode != null)
                                nodes.Add(swNode);
                            break;
                        case "code":
                            var cbNode = ParseCodeBlock(atLine);
                            if (cbNode != null)
                                nodes.Add(cbNode);
                            break;
                        case "else":
                            _diagnostics.Add(
                                MakeError(
                                    UitkxDiagnostics.UnexpectedToken,
                                    "@else",
                                    atLine,
                                    _filePath,
                                    "@if block"
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        case "case":
                        case "default":
                            _diagnostics.Add(
                                MakeError(
                                    UitkxDiagnostics.UnexpectedToken,
                                    "@" + keyword,
                                    atLine,
                                    _filePath,
                                    "@switch block"
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        default:
                            _diagnostics.Add(
                                MakeError(
                                    UitkxDiagnostics.UnknownDirective,
                                    keyword,
                                    atLine,
                                    _filePath
                                )
                            );
                            SkipToEndOfLine();
                            break;
                    }

                    continue;
                }

                // ── Text content ────────────────────────────────────────────
                if (c == '>')
                {
                    _scanner.Advance(); // stray '>'
                    continue;
                }

                string? text = _scanner.ReadTextContent();
                if (text != null)
                    nodes.Add(new TextNode(text, _scanner.Line, _filePath));

                // Anti-infinite-loop: if nothing advanced the scanner, skip one char
                if (_scanner.Pos == positionBefore)
                    _scanner.Advance();
            }

            return nodes;
        }

        // ── Element ───────────────────────────────────────────────────────────

        private ElementNode? ParseElement()
        {
            int openLine = _scanner.Line;
            _scanner.Advance(); // consume '<'
            _scanner.SkipInlineWhitespace();

            // <></> short-hand fragment — empty tag name maps to V.Fragment in the emitter.
            if (_scanner.TryConsume('>'))
            {
                var fragmentChildren = ParseContent(
                    stopTag: string.Empty,
                    stopAtBrace: false,
                    stopAtCase: false
                );

                // Consume </>
                if (!_scanner.IsEof)
                {
                    if (PeekClosingTagName() == string.Empty)
                        ConsumeClosingTag();
                    else
                        _diagnostics.Add(
                            MakeError(UitkxDiagnostics.UnclosedTag, "<>", openLine, _filePath)
                        );
                }
                else
                {
                    _diagnostics.Add(
                        MakeError(UitkxDiagnostics.UnclosedTag, "<>", openLine, _filePath)
                    );
                }

                return new ElementNode(
                    string.Empty,
                    ImmutableArray<AttributeNode>.Empty,
                    fragmentChildren.ToImmutableArray(),
                    openLine,
                    _filePath
                );
            }

            string tagName = _scanner.ReadTagName();
            if (string.IsNullOrEmpty(tagName))
            {
                _diagnostics.Add(
                    MakeError(
                        UitkxDiagnostics.UnexpectedToken,
                        "<",
                        openLine,
                        _filePath,
                        "tag name after '<'"
                    )
                );
                SkipToTagEnd();
                return null;
            }

            var attributes = ParseAttributes();
            _scanner.SkipWhitespaceAndNewlines();

            // Self-closing: />
            if (_scanner.TryConsume("/>"))
            {
                return new ElementNode(
                    tagName,
                    attributes,
                    ImmutableArray<AstNode>.Empty,
                    openLine,
                    _filePath
                );
            }

            // Open-block: >
            if (!_scanner.TryConsume('>'))
            {
                string got = _scanner.IsEof ? "EOF" : _scanner.Current.ToString();
                _diagnostics.Add(
                    MakeError(
                        UitkxDiagnostics.UnexpectedToken,
                        got,
                        openLine,
                        _filePath,
                        "'>' or '/>'"
                    )
                );
                // Best-effort: continue without consuming
            }

            var children = ParseContent(stopTag: tagName, stopAtBrace: false, stopAtCase: false);

            if (_scanner.IsEof)
            {
                _diagnostics.Add(
                    MakeError(UitkxDiagnostics.UnclosedTag, tagName, openLine, _filePath)
                );
            }
            else
            {
                string closing = PeekClosingTagName();
                if (string.Equals(closing, tagName, StringComparison.Ordinal))
                    ConsumeClosingTag();
                else
                {
                    _diagnostics.Add(
                        MakeError(
                            UitkxDiagnostics.MismatchedClosingTag,
                            closing,
                            tagName,
                            openLine,
                            _filePath
                        )
                    );
                    SkipToTagEnd();
                }
            }

            return new ElementNode(
                tagName,
                attributes,
                children.ToImmutableArray(),
                openLine,
                _filePath
            );
        }

        // ── Attributes ────────────────────────────────────────────────────────

        private ImmutableArray<AttributeNode> ParseAttributes()
        {
            var attrs = new List<AttributeNode>();

            while (!_scanner.IsEof)
            {
                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsAt("/>") || _scanner.Current == '>' || _scanner.IsEof)
                    break;

                int attrLine = _scanner.Line;
                string name = _scanner.ReadAttrName();

                if (string.IsNullOrEmpty(name))
                {
                    _scanner.Advance();
                    continue;
                }

                _scanner.SkipInlineWhitespace();

                if (_scanner.TryConsume('='))
                {
                    _scanner.SkipInlineWhitespace();

                    if (!_scanner.IsEof && _scanner.Current == '"')
                    {
                        string lit = _scanner.ReadStringLiteral();
                        attrs.Add(new AttributeNode(name, new StringLiteralValue(lit), attrLine));
                    }
                    else if (!_scanner.IsEof && _scanner.Current == '{')
                    {
                        string expr = _scanner.ReadBraceExpression();
                        attrs.Add(
                            new AttributeNode(name, new CSharpExpressionValue(expr), attrLine)
                        );
                    }
                    else
                    {
                        string got = _scanner.IsEof ? "EOF" : _scanner.Current.ToString();
                        _diagnostics.Add(
                            MakeError(
                                UitkxDiagnostics.UnexpectedToken,
                                got,
                                attrLine,
                                _filePath,
                                "'\"' or '{' for attribute value"
                            )
                        );
                    }
                }
                else
                {
                    // Boolean shorthand attribute
                    attrs.Add(new AttributeNode(name, new BooleanShorthandValue(), attrLine));
                }
            }

            return attrs.ToImmutableArray();
        }

        // ── @if ───────────────────────────────────────────────────────────────

        private IfNode? ParseIf(int startLine)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @if", startLine);
                return null;
            }

            string cond = _scanner.ReadParenExpression();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @if (...)", startLine);
                return null;
            }

            int firstLine = _scanner.Line;
            _scanner.Advance(); // consume '{'
            var firstBody = ParseContent(null, stopAtBrace: true, stopAtCase: false);
            _scanner.TryConsume('}');
            var branches = new List<IfBranch>
            {
                new IfBranch(cond, firstBody.ToImmutableArray(), firstLine),
            };

            // Chain @else if / @else
            while (true)
            {
                _scanner.SkipWhitespaceAndNewlines();
                if (!LookAheadIsElse(out bool isElseIf))
                    break;

                // Consume '@else'
                _scanner.TryConsume('@');
                _scanner.ReadIdentifier(); // "else"
                _scanner.SkipInlineWhitespace();
                int elseLine = _scanner.Line;

                if (isElseIf)
                {
                    _scanner.ReadIdentifier(); // "if"
                    _scanner.SkipInlineWhitespace();

                    if (!PeekAt('('))
                    {
                        EmitExpected("'(' after @else if", elseLine);
                        break;
                    }
                    string elseCond = _scanner.ReadParenExpression();
                    _scanner.SkipWhitespaceAndNewlines();
                    if (!PeekAt('{'))
                    {
                        EmitExpected("'{' after @else if (...)", elseLine);
                        break;
                    }

                    _scanner.Advance(); // consume '{'
                    var elseIfBody = ParseContent(null, stopAtBrace: true, stopAtCase: false);
                    _scanner.TryConsume('}');
                    branches.Add(new IfBranch(elseCond, elseIfBody.ToImmutableArray(), elseLine));
                }
                else
                {
                    if (!PeekAt('{'))
                    {
                        EmitExpected("'{' after @else", elseLine);
                        break;
                    }
                    _scanner.Advance(); // consume '{'
                    var elseBody = ParseContent(null, stopAtBrace: true, stopAtCase: false);
                    _scanner.TryConsume('}');
                    branches.Add(new IfBranch(null, elseBody.ToImmutableArray(), elseLine));
                    break; // @else terminates the chain
                }
            }

            return new IfNode(branches.ToImmutableArray(), startLine, _filePath);
        }

        // ── @foreach ──────────────────────────────────────────────────────────

        private ForeachNode? ParseForeach(int startLine)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @foreach", startLine);
                return null;
            }

            string foreachExpr = _scanner.ReadParenExpression();
            _scanner.SkipWhitespaceAndNewlines();

            // Split "var item in collection" on first standalone " in "
            string iteratorDecl = foreachExpr;
            string collectionExpr = string.Empty;
            int inIdx = foreachExpr.IndexOf(" in ", StringComparison.Ordinal);
            if (inIdx >= 0)
            {
                iteratorDecl = foreachExpr.Substring(0, inIdx).Trim();
                collectionExpr = foreachExpr.Substring(inIdx + 4).Trim();
            }

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @foreach (...)", startLine);
                return null;
            }

            _scanner.Advance(); // consume '{'
            var body = ParseContent(null, stopAtBrace: true, stopAtCase: false);
            _scanner.TryConsume('}');

            return new ForeachNode(
                iteratorDecl,
                collectionExpr,
                body.ToImmutableArray(),
                startLine,
                _filePath
            );
        }

        // ── @switch ───────────────────────────────────────────────────────────

        private SwitchNode? ParseSwitch(int startLine)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @switch", startLine);
                return null;
            }

            string switchExpr = _scanner.ReadParenExpression();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @switch (...)", startLine);
                return null;
            }

            _scanner.Advance(); // consume '{'

            var cases = new List<SwitchCase>();

            while (!_scanner.IsEof && _scanner.Current != '}')
            {
                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsEof || _scanner.Current == '}')
                    break;

                if (_scanner.Current != '@')
                {
                    _scanner.Advance(); // skip noise
                    continue;
                }

                int caseLine = _scanner.Line;
                _scanner.Advance(); // consume '@'
                string keyword = _scanner.ReadIdentifier();

                if (keyword == "case")
                {
                    _scanner.SkipInlineWhitespace();
                    // Read case value up to ':'
                    int vStart = _scanner.Pos;
                    while (
                        !_scanner.IsEof
                        && _scanner.Current != ':'
                        && _scanner.Current != '\r'
                        && _scanner.Current != '\n'
                    )
                        _scanner.Advance();
                    string caseVal = _source.Substring(vStart, _scanner.Pos - vStart).Trim();
                    _scanner.TryConsume(':');
                    var caseBody = ParseContent(null, stopAtBrace: true, stopAtCase: true);
                    cases.Add(new SwitchCase(caseVal, caseBody.ToImmutableArray(), caseLine));
                }
                else if (keyword == "default")
                {
                    _scanner.TryConsume(':');
                    var defaultBody = ParseContent(null, stopAtBrace: true, stopAtCase: true);
                    cases.Add(new SwitchCase(null, defaultBody.ToImmutableArray(), caseLine));
                }
                else
                {
                    _diagnostics.Add(
                        MakeError(
                            UitkxDiagnostics.UnexpectedToken,
                            "@" + keyword,
                            caseLine,
                            _filePath,
                            "@case or @default"
                        )
                    );
                    SkipToEndOfLine();
                }
            }

            _scanner.TryConsume('}');

            return new SwitchNode(switchExpr, cases.ToImmutableArray(), startLine, _filePath);
        }

        // ── @code ─────────────────────────────────────────────────────────────

        private CodeBlockNode? ParseCodeBlock(int startLine)
        {
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @code", startLine);
                return null;
            }

            // Use ExpressionExtractor so nested braces in C# are handled correctly
            var (code, afterClose) = ExpressionExtractor.FromBrace(_source, _scanner.Pos);
            AdvanceScannerTo(afterClose);

            return new CodeBlockNode(code, startLine, _filePath);
        }

        // ── Peek / advance helpers ────────────────────────────────────────────

        /// <summary>
        /// Returns the character at <c>_scanner.Pos + offset</c> without advancing,
        /// or <c>'\0'</c> if out of range.
        /// </summary>
        private char PeekChar(int offset = 1)
        {
            int i = _scanner.Pos + offset;
            return i < _source.Length ? _source[i] : '\0';
        }

        /// <summary>
        /// Without consuming, returns <c>true</c> if the current character
        /// (after skipping inline whitespace in the raw source) is <paramref name="c"/>.
        /// </summary>
        private bool PeekAt(char c)
        {
            int i = _scanner.Pos;
            while (i < _source.Length && (_source[i] == ' ' || _source[i] == '\t'))
                i++;
            return i < _source.Length && _source[i] == c;
        }

        /// <summary>
        /// Returns the directive keyword immediately after a leading '@' at the
        /// current scanner position — without consuming anything.
        /// Returns <c>null</c> if the current char is not '@'.
        /// </summary>
        private string? PeekDirectiveKeyword()
        {
            int pos = _scanner.Pos;
            if (pos >= _source.Length || _source[pos] != '@')
                return null;
            int i = pos + 1;
            if (i >= _source.Length || !char.IsLetter(_source[i]))
                return null;
            int start = i;
            while (i < _source.Length && char.IsLetter(_source[i]))
                i++;
            return _source.Substring(start, i - start);
        }

        /// <summary>
        /// Returns the tag name from the closing tag '&lt;/TagName&gt;' at the
        /// current scanner position without consuming any characters.
        /// </summary>
        private string PeekClosingTagName()
        {
            int i = _scanner.Pos;
            if (i >= _source.Length || _source[i] != '<')
                return string.Empty;
            i++;
            if (i >= _source.Length || _source[i] != '/')
                return string.Empty;
            i++;
            while (i < _source.Length && (_source[i] == ' ' || _source[i] == '\t'))
                i++;
            int start = i;
            while (
                i < _source.Length
                && (char.IsLetterOrDigit(_source[i]) || _source[i] == '_' || _source[i] == '-')
            )
                i++;
            return _source.Substring(start, i - start);
        }

        /// <summary>
        /// Consumes '&lt;/TagName&gt;', assuming the scanner is at the '&lt;'.
        /// </summary>
        private void ConsumeClosingTag()
        {
            _scanner.TryConsume('<');
            _scanner.TryConsume('/');
            _scanner.SkipInlineWhitespace();
            _scanner.ReadTagName();
            _scanner.SkipInlineWhitespace();
            _scanner.TryConsume('>');
        }

        /// <summary>
        /// Pure look-ahead: does the raw source at the current scanner position
        /// contain '@else' or '@else if'?  Sets <paramref name="isElseIf"/> accordingly.
        /// Never advances the scanner.
        /// </summary>
        private bool LookAheadIsElse(out bool isElseIf)
        {
            isElseIf = false;

            int i = _scanner.Pos;
            const string ElseToken = "@else";
            if (i + ElseToken.Length > _source.Length)
                return false;

            for (int k = 0; k < ElseToken.Length; k++)
                if (_source[i + k] != ElseToken[k])
                    return false;

            int afterElse = i + ElseToken.Length;

            // '@else' must not be part of a longer identifier
            if (
                afterElse < _source.Length
                && (char.IsLetterOrDigit(_source[afterElse]) || _source[afterElse] == '_')
            )
                return false;

            // Skip inline whitespace after '@else'
            int j = afterElse;
            while (j < _source.Length && (_source[j] == ' ' || _source[j] == '\t'))
                j++;

            if (j < _source.Length && _source[j] == '{')
            {
                isElseIf = false;
                return true;
            }

            if (
                j + 2 <= _source.Length
                && _source[j] == 'i'
                && _source[j + 1] == 'f'
                && (
                    j + 2 >= _source.Length
                    || (!char.IsLetterOrDigit(_source[j + 2]) && _source[j + 2] != '_')
                )
            )
            {
                isElseIf = true;
                return true;
            }

            return false;
        }

        private void EmitExpected(string expected, int line)
        {
            string got = _scanner.IsEof ? "end of file" : _scanner.Current.ToString();
            _diagnostics.Add(
                MakeError(UitkxDiagnostics.UnexpectedToken, got, line, _filePath, expected)
            );
        }

        private void SkipToTagEnd()
        {
            while (!_scanner.IsEof && _scanner.Current != '>')
                _scanner.Advance();
            if (!_scanner.IsEof)
                _scanner.Advance(); // consume '>'
        }

        private void SkipToEndOfLine()
        {
            while (!_scanner.IsEof && _scanner.Current != '\r' && _scanner.Current != '\n')
                _scanner.Advance();
        }

        private void AdvanceScannerTo(int targetPos)
        {
            while (_scanner.Pos < targetPos && !_scanner.IsEof)
                _scanner.Advance();
        }

        private Diagnostic MakeError(DiagnosticDescriptor desc, params object[] args)
        {
            var loc = Location.Create(_filePath, default, default);
            return Diagnostic.Create(desc, loc, args);
        }
    }
}
