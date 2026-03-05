using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUITK.Language.Nodes;

namespace ReactiveUITK.Language.Parser
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
    public sealed class UitkxParser
    {
        private readonly string _source;
        private readonly string _filePath;

        // Non-readonly so LookAheadIsElse-based @else processing can re-create the
        // scanner after controlled lookahead (though current impl avoids it).
        private MarkupTokenizer _scanner;
        private readonly List<ParseDiagnostic> _diagnostics;

        // ── Construction ──────────────────────────────────────────────────────

        private UitkxParser(
            string source,
            string filePath,
            int startPos,
            int startLine,
            List<ParseDiagnostic> diagnostics
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
            List<ParseDiagnostic> diagnostics
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
                        ErrMismatchedTag(closing, stopTag ?? "?", _scanner.Line)
                    );
                    _scanner.Advance();
                    continue;
                }

                // ── HTML comment <!-- ... --> ───────────────────────────────
                if (_scanner.TrySkipHtmlComment())
                    continue;

                // ── JSX comment {/* ... */} ──────────────────────────────────
                if (c == '{')
                {
                    int commentLine1 = _scanner.Line;
                    if (_scanner.TrySkipJsxComment(out string jsxContent))
                    {
                        nodes.Add(new JsxCommentNode(jsxContent, commentLine1, _filePath));
                        continue;
                    }
                }

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
                        case "for":
                            var forNode = ParseFor(atLine);
                            if (forNode != null)
                                nodes.Add(forNode);
                            break;
                        case "while":
                            var whileNode = ParseWhile(atLine);
                            if (whileNode != null)
                                nodes.Add(whileNode);
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
                                ErrUnexpectedToken("@else", atLine, "@if block")
                            );
                            SkipToEndOfLine();
                            break;
                        case "case":
                        case "default":
                            _diagnostics.Add(
                                ErrUnexpectedToken("@" + keyword, atLine, "@switch block")
                            );
                            SkipToEndOfLine();
                            break;
                        default:
                            _diagnostics.Add(
                                ErrUnknownDirective(keyword, atLine)
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
            int openCol  = ColAtPos(_scanner.Pos); // 0-based column of the '<'
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
                            ErrUnclosedTag("<>", openLine)
                        );
                }
                else
                {
                    _diagnostics.Add(
                        ErrUnclosedTag("<>", openLine)
                    );
                }

                return new ElementNode(
                    string.Empty,
                    ImmutableArray<AttributeNode>.Empty,
                    fragmentChildren.ToImmutableArray(),
                    openLine,
                    _filePath
                ) { SourceColumn = openCol };
            }

            string tagName = _scanner.ReadTagName();
            if (string.IsNullOrEmpty(tagName))
            {
                _diagnostics.Add(
                    ErrUnexpectedToken("<", openLine, "tag name after '<'")
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
                ) { SourceColumn = openCol };
            }

            // Open-block: >
            if (!_scanner.TryConsume('>'))
            {
                _diagnostics.Add(
                    ErrMissingTagClose(tagName, openLine, openCol)
                );
                // Best-effort: continue without consuming
            }

            var children = ParseContent(stopTag: tagName, stopAtBrace: false, stopAtCase: false);

            int closeTagLine = 0;

            if (_scanner.IsEof)
            {
                _diagnostics.Add(
                    ErrUnclosedTag(tagName, openLine)
                );
            }
            else
            {
                string closing = PeekClosingTagName();
                if (string.Equals(closing, tagName, StringComparison.Ordinal))
                {
                    closeTagLine = _scanner.Line;
                    ConsumeClosingTag();
                }
                else
                {
                    _diagnostics.Add(
                        ErrMismatchedTag(closing, tagName, openLine)
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
            ) { SourceColumn = openCol, CloseTagLine = closeTagLine };
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

                // A '@' or '<' at this level means a control-flow directive or child
                // element — the opening '>' was never written.  Stop here so the caller
                // continues parsing normally rather than consuming the rest of the file
                // as attribute content.
                if (_scanner.Current == '@' || _scanner.Current == '<')
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
                            ErrUnexpectedToken(got, attrLine, "'\"' or '{' for attribute value")
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

        // ── @for ──────────────────────────────────────────────────────────────

        private ForNode? ParseFor(int startLine)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @for", startLine);
                return null;
            }

            string forExpr = _scanner.ReadParenExpression();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @for (...)", startLine);
                return null;
            }

            _scanner.Advance(); // consume '{'
            var body = ParseContent(null, stopAtBrace: true, stopAtCase: false);
            _scanner.TryConsume('}');

            return new ForNode(forExpr, body.ToImmutableArray(), startLine, _filePath);
        }

        // ── @while ────────────────────────────────────────────────────────────

        private WhileNode? ParseWhile(int startLine)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @while", startLine);
                return null;
            }

            string condition = _scanner.ReadParenExpression();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @while (...)", startLine);
                return null;
            }

            _scanner.Advance(); // consume '{'
            var body = ParseContent(null, stopAtBrace: true, stopAtCase: false);
            _scanner.TryConsume('}');

            return new WhileNode(condition, body.ToImmutableArray(), startLine, _filePath);
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
                        ErrUnexpectedToken("@" + keyword, caseLine, "@case or @default")
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
            int bracePos       = _scanner.Pos;
            int codeBodyStart  = bracePos + 1;
            var (code, afterClose) = ExpressionExtractor.FromBrace(_source, bracePos);
            int codeBodyEnd    = afterClose > 0 ? afterClose - 1 : bracePos + 1;
            AdvanceScannerTo(afterClose);

            // Compute how many leading chars were trimmed so offsets map correctly
            int rawLen       = codeBodyEnd - codeBodyStart;
            string rawBody   = rawLen > 0 ? _source.Substring(codeBodyStart, rawLen) : string.Empty;
            int leadingChars = rawBody.Length - rawBody.TrimStart().Length;

            var returnMarkups = ScanForReturnMarkup(codeBodyStart, codeBodyEnd, leadingChars, startLine);

            return new CodeBlockNode(code, startLine, _filePath)
            {
                ReturnMarkups = returnMarkups,
            };
        }

        /// <summary>
        /// Scans the code body for embedded markup expressions in two forms:
        /// <list type="bullet">
        ///   <item><c>return &lt;Tag .../&gt;</c> — explicit return of a VirtualNode.</item>
        ///   <item><c>= &lt;Tag .../&gt;</c> — assignment of a VirtualNode to a variable
        ///     (e.g. <c>var x = &lt;Label text="hi"/&gt;;</c>).</item>
        /// </list>
        /// Each found element is parsed into a <see cref="ReturnMarkupNode"/> using a
        /// temporary sub-parser so the current parser state is not disturbed.
        /// </summary>
        private ImmutableArray<ReturnMarkupNode> ScanForReturnMarkup(
            int codeBodyStart, int codeBodyEnd, int leadingChars, int blockStartLine)
        {
            var result = ImmutableArray.CreateBuilder<ReturnMarkupNode>();
            int i = codeBodyStart;

            while (i < codeBodyEnd)
            {
                int markupAt   = -1; // index in _source where '<Tag' starts, or -1
                int parenStart = -1; // index in _source of an enclosing '(', if present

                // ── Pattern A: return <Tag  (also tolerates return (<Tag>) ) ───
                if (i < codeBodyEnd - 6 && _source[i] == 'r')
                {
                    const string kw = "return";
                    bool kwMatch = true;
                    for (int k = 0; k < kw.Length && kwMatch; k++)
                        if (i + k >= _source.Length || _source[i + k] != kw[k]) kwMatch = false;

                    if (kwMatch)
                    {
                        bool boundBefore = i == 0 ||
                            !(char.IsLetterOrDigit(_source[i - 1]) || _source[i - 1] == '_');
                        int afterKw = i + kw.Length;
                        bool boundAfter = afterKw >= _source.Length ||
                            !(char.IsLetterOrDigit(_source[afterKw]) || _source[afterKw] == '_');

                        if (boundBefore && boundAfter)
                        {
                            int j = afterKw;
                            while (j < codeBodyEnd &&
                                   (_source[j] == ' ' || _source[j] == '\t' ||
                                    _source[j] == '\r' || _source[j] == '\n'))
                                j++;

                            // Tolerate return (<Tag>)
                            if (j < codeBodyEnd && _source[j] == '(')
                            {
                                parenStart = j;
                                j++;
                                while (j < codeBodyEnd &&
                                       (_source[j] == ' ' || _source[j] == '\t' ||
                                        _source[j] == '\r' || _source[j] == '\n'))
                                    j++;
                            }

                            if (j < codeBodyEnd && _source[j] == '<' &&
                                j + 1 < codeBodyEnd && char.IsLetter(_source[j + 1]))
                                markupAt = j;
                            else
                                parenStart = -1; // paren was not followed by JSX — reset
                        }
                    }
                }

                // ── Pattern B: = <Tag  (single-equals assignment) ──────────────
                // Excluded: ==, =>, !=, <=, >=
                if (markupAt < 0 && _source[i] == '=')
                {
                    bool isAssign = true;
                    // Exclude == and =>
                    if (i + 1 < _source.Length &&
                        (_source[i + 1] == '=' || _source[i + 1] == '>'))
                        isAssign = false;
                    // Exclude !=, <=, >=
                    if (isAssign && i > 0 &&
                        (_source[i - 1] == '!' || _source[i - 1] == '<' || _source[i - 1] == '>'))
                        isAssign = false;

                    if (isAssign)
                    {
                        int j = i + 1;
                        while (j < codeBodyEnd &&
                               (_source[j] == ' ' || _source[j] == '\t' ||
                                _source[j] == '\r' || _source[j] == '\n'))
                            j++;

                        // Tolerate an optional '(' directly before the '<'
                        // e.g.  = (<Box> or = (\n<Box>
                        if (j < codeBodyEnd && _source[j] == '(')
                        {
                            parenStart = j;
                            j++;
                            while (j < codeBodyEnd &&
                                   (_source[j] == ' ' || _source[j] == '\t' ||
                                    _source[j] == '\r' || _source[j] == '\n'))
                                j++;
                        }

                        if (j < codeBodyEnd && _source[j] == '<' &&
                            j + 1 < codeBodyEnd && char.IsLetter(_source[j + 1]))
                            markupAt = j;
                        else
                            parenStart = -1; // paren not followed by JSX — reset
                    }
                }

                // ── Parse element if either pattern matched ─────────────────────
                if (markupAt >= 0)
                {
                    int elementStartInSource = markupAt;
                    int elementLine = LineAtPos(elementStartInSource);
                    var (element, endPos) = ParseSingleElement(
                        _source, _filePath, elementStartInSource, elementLine, _diagnostics);

                    if (element != null)
                    {
                        // When a '(' wraps the element, expand the span to cover
                        // '(' … ')' so CSharpEmitter replaces the whole paren group.
                        int spanStart = parenStart >= 0 ? parenStart : elementStartInSource;
                        int spanEnd   = endPos;
                        if (parenStart >= 0)
                        {
                            int k = endPos;
                            while (k < codeBodyEnd &&
                                   (_source[k] == ' ' || _source[k] == '\t' ||
                                    _source[k] == '\r' || _source[k] == '\n'))
                                k++;
                            if (k < codeBodyEnd && _source[k] == ')')
                                spanEnd = k + 1;
                        }

                        int startOffset = spanStart - codeBodyStart - leadingChars;
                        int endOffset   = spanEnd   - codeBodyStart - leadingChars;
                        int elementCol  = ColAtPos(elementStartInSource);
                        result.Add(new ReturnMarkupNode(
                            element with { SourceColumn = elementCol },
                            startOffset, endOffset, elementLine, _filePath));
                        i = spanEnd; // advance past entire span (including ')')
                        continue;
                    }
                }

                i++;
            }

            return result.ToImmutable();
        }

        /// <summary>
        /// Parses a single element from an arbitrary position in <paramref name="source"/>
        /// by creating a temporary sub-parser.  Does NOT mutate the current parser's state.
        /// </summary>
        internal static (ElementNode? Element, int EndPos) ParseSingleElement(
            string source, string filePath, int startPos, int startLine,
            List<ParseDiagnostic> diagnostics)
        {
            var parser = new UitkxParser(source, filePath, startPos, startLine, diagnostics);
            var element = parser.ParseElement();
            return (element, parser._scanner.Pos);
        }

        /// <summary>Returns the 1-based line number for the given character offset in <c>_source</c>.</summary>
        private int LineAtPos(int pos)
        {
            int line = 1;
            for (int i = 0; i < pos && i < _source.Length; i++)
                if (_source[i] == '\n') line++;
            return line;
        }

        /// <summary>Returns the 0-based column for the given character offset in <c>_source</c>.</summary>
        private int ColAtPos(int pos)
        {
            int col = 0;
            for (int i = pos - 1; i >= 0 && _source[i] != '\n'; i--)
                col++;
            return col;
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
            _diagnostics.Add(ErrUnexpectedToken(got, line, expected));
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

        // ── ParseDiagnostic factory helpers ───────────────────────────────────

        private ParseDiagnostic ErrUnexpectedToken(string got, int line, string expected) =>
            new ParseDiagnostic
            {
                Code = "UITKX0300",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                Message =
                    $"Unexpected '{got}' at line {line} in '{_filePath}'. Expected {expected}.",
            };

        private ParseDiagnostic ErrMissingTagClose(string tagName, int line, int openCol) =>
            new ParseDiagnostic
            {
                Code        = "UITKX0303",
                Severity    = ParseSeverity.Error,
                SourceLine  = line,
                SourceColumn = openCol,           // 0-based column of '<'
                EndLine     = line,
                EndColumn   = openCol + 1 + tagName.Length, // covers '<TagName'
                Message     = $"Missing '>' or '/>' after tag '<{tagName}>' at line {line}.",
            };

        private ParseDiagnostic ErrUnclosedTag(string tagName, int line) =>
            new ParseDiagnostic
            {
                Code = "UITKX0301",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                Message =
                    $"Tag '<{tagName}>' opened at line {line} in '{_filePath}' was never closed",
            };

        private ParseDiagnostic ErrMismatchedTag(string got, string expected, int line) =>
            new ParseDiagnostic
            {
                Code = "UITKX0302",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                Message =
                    $"Found '</{got}>' but expected '</{expected}>' (opened at line {line}) in '{_filePath}'",
            };

        private ParseDiagnostic ErrUnknownDirective(string keyword, int line) =>
            new ParseDiagnostic
            {
                Code = "UITKX0305",
                Severity = ParseSeverity.Warning,
                SourceLine = line,
                Message =
                    $"Unknown markup directive '@{keyword}' at line {line} in '{_filePath}'. "
                    + "Valid directives are: if, else, foreach, switch, case, default, code.",
            };
    }
}
