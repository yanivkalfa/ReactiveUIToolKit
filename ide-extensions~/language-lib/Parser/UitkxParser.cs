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
    ///                   InlineExpr | Text | HtmlComment)*
    ///   Element      = lt TagName Attribute* ('/>' | '>' Content lt '/' TagName '>')
    ///   Attribute    = Name '=' ('"' Value '"' | '{' CSharpExpr '}') | Name
    ///   IfBlock      = '@if' Paren BraceContent ('@else' ('if' Paren)? BraceContent)*
    ///   BraceContent = '{' Content '}'
    ///   ForeachBlock = '@foreach' '(' Decl 'in' Expr ')' BraceContent
    ///   SwitchBlock  = '@switch' Paren '{' ('@case' Expr ':' | '@default' ':') Content* '}'
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
        private int _stopPosExclusive;

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
            int stopPosExclusive,
            List<ParseDiagnostic> diagnostics
        )
        {
            _source = source;
            _filePath = filePath;
            _stopPosExclusive = stopPosExclusive;
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
                directives.MarkupEndIndex,
                diagnostics
            );

            return parser
                .ParseContent(stopTag: null, stopAtBrace: false, stopAtCase: false)
                .ToImmutableArray();
        }

        // ── Control block body parsing ───────────────────────────────────────

        /// <summary>
        /// Parsed result of a control block body that may contain
        /// <c>setup-code; return (...markup...);</c>.
        /// </summary>
        private readonly struct ControlBlockBody
        {
            public readonly List<AstNode> Nodes;
            public readonly string? SetupCode;
            public readonly int SetupCodeOffset;
            public readonly int SetupCodeLine;

            public ControlBlockBody(List<AstNode> nodes, string? setupCode, int setupCodeOffset, int setupCodeLine)
            {
                Nodes = nodes;
                SetupCode = setupCode;
                SetupCodeOffset = setupCodeOffset;
                SetupCodeLine = setupCodeLine;
            }
        }

        /// <summary>
        /// Parses a control block body: the text between <c>{</c> (already consumed)
        /// and its matching <c>}</c>.  Looks for <c>return (...)</c> to split
        /// setup code from markup.  If no <c>return</c> is found, emits a diagnostic
        /// and falls back to parsing the body as pure markup (error recovery).
        /// </summary>
        private ControlBlockBody ParseControlBlockBody(
            int openBracePos,
            bool stopAtCase,
            string blockName
        )
        {
            int bodyStart = _scanner.Pos;

            // Find the matching '}' using the raw source string.
            int closeBrace = ReturnFinder.FindMatchingBrace(_source, openBracePos, _source.Length);
            if (closeBrace < 0)
            {
                // Unbalanced — fallback to old behaviour
                var fallback = ParseContent(null, stopAtBrace: true, stopAtCase: stopAtCase);
                _scanner.TryConsume('}');
                return new ControlBlockBody(fallback, null, 0, 0);
            }

            int bodyEnd = closeBrace; // exclusive: position of '}'

            // Try to find return (...); at depth 0 within the body.
            if (ReturnFinder.TryFindTopLevelReturn(
                    _source, bodyStart, bodyEnd,
                    out int returnStart,
                    out int returnOpenParen,
                    out int returnCloseParen,
                    out int returnStmtEnd,
                    useLastReturn: false))
            {
                // Extract setup code (everything before 'return')
                string? setupCode = null;
                int setupCodeOffset = 0;
                int setupCodeLine = 0;
                string rawSetup = _source.Substring(bodyStart, returnStart - bodyStart).Trim();
                if (rawSetup.Length > 0)
                {
                    setupCode = rawSetup;
                    // Find the first non-whitespace character for accurate offset
                    int firstNonWs = bodyStart;
                    while (firstNonWs < returnStart && char.IsWhiteSpace(_source[firstNonWs]))
                        firstNonWs++;
                    setupCodeOffset = firstNonWs;
                    setupCodeLine = ReturnFinder.LineAtPos(_source, firstNonWs);
                }

                // Position scanner inside the return parens and parse markup
                _scanner.AdvanceTo(returnOpenParen + 1); // past '('

                int savedStop = _stopPosExclusive;
                _stopPosExclusive = returnCloseParen;
                var body = ParseContent(null, stopAtBrace: false, stopAtCase: false);
                _stopPosExclusive = savedStop;

                // Advance scanner past ');' and '}'
                _scanner.AdvanceTo(closeBrace);
                _scanner.TryConsume('}');

                return new ControlBlockBody(body, setupCode, setupCodeOffset, setupCodeLine);
            }
            else
            {
                // No return() found — emit diagnostic and fall back to parsing as markup
                _diagnostics.Add(new ParseDiagnostic
                {
                    Code = "UITKX0024",
                    Severity = ParseSeverity.Error,
                    SourceLine = ReturnFinder.LineAtPos(_source, bodyStart),
                    Message = $"Control block body ({blockName}) must contain 'return (...);'.",
                });

                // Error recovery: parse body as pure markup, same as before
                var fallback = ParseContent(null, stopAtBrace: true, stopAtCase: stopAtCase);
                _scanner.TryConsume('}');
                return new ControlBlockBody(fallback, null, 0, 0);
            }
        }

        // ── Content loop ──────────────────────────────────────────────────────

        /// <summary>
        /// Parses zero or more nodes until a stop condition is met.
        /// </summary>
        /// <param name="stopTag">Stop (without consuming) at matching closing tag.</param>
        /// <param name="stopAtBrace">Stop (without consuming) at bare '}'.</param>
        /// <param name="stopAtCase">Stop (without consuming) at '@case' or '@default'.</param>
        private List<AstNode> ParseContent(
            string? stopTag,
            bool stopAtBrace,
            bool stopAtCase
        )
        {
            var nodes = new List<AstNode>();

            while (!_scanner.IsEof)
            {
                if (_stopPosExclusive >= 0 && _scanner.Pos >= _stopPosExclusive)
                    break;

                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsEof)
                    break;

                if (_stopPosExclusive >= 0 && _scanner.Pos >= _stopPosExclusive)
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
                    if (kw == "case" || kw == "default" || kw == "break")
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
                    _diagnostics.Add(ErrMismatchedTag(closing, stopTag ?? "?", _scanner.Line));
                    _scanner.Advance();
                    continue;
                }

                // ── HTML comment <!-- ... --> ───────────────────────────────
                if (_scanner.TrySkipHtmlComment())
                    continue;

                // ── JSX comment {/* ... */} or child expression {expr} ────────
                if (c == '{')
                {
                    int commentLine1 = _scanner.Line;
                    if (_scanner.TrySkipJsxComment(out string jsxContent))
                    {
                        nodes.Add(new JsxCommentNode(jsxContent, commentLine1, _filePath));
                        continue;
                    }

                    // ── Child expression {expr} ─────────────────────────────
                    int exprLine = _scanner.Line;
                    int exprCol = ColAtPos(_scanner.Pos);
                    var (expr, exprOffset) = _scanner.ReadBraceExpressionWithOffset();
                    if (!string.IsNullOrEmpty(expr))
                    {
                        nodes.Add(
                            new ExpressionNode(expr, exprLine, _filePath)
                            {
                                ExpressionOffset = exprOffset,
                                ExpressionLength = expr.Length,
                                SourceColumn = exprCol,
                                EndColumn = exprCol + 1,
                            }
                        );
                    }
                    continue;
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
                    int atCol = ColAtPos(_scanner.Pos); // 0-based column of '@'
                    _scanner.Advance(); // consume '@'

                    if (!_scanner.IsEof && _scanner.Current == '(')
                    {
                        var (inlineExpr, exprOffset) = _scanner.ReadParenExpressionWithOffset();
                        nodes.Add(
                            new ExpressionNode(inlineExpr, atLine, _filePath)
                            {
                                ExpressionOffset = exprOffset,
                                ExpressionLength = inlineExpr.Length,
                                SourceColumn = atCol,
                                EndColumn = atCol + 1, // '@' itself
                            }
                        );
                        continue;
                    }

                    string keyword = _scanner.ReadIdentifier();

                    switch (keyword)
                    {
                        case "if":
                            var ifNode = ParseIf(atLine, atCol);
                            if (ifNode != null)
                                nodes.Add(ifNode);
                            break;
                        case "foreach":
                            var feNode = ParseForeach(atLine, atCol);
                            if (feNode != null)
                                nodes.Add(feNode);
                            break;
                        case "for":
                            var forNode = ParseFor(atLine, atCol);
                            if (forNode != null)
                                nodes.Add(forNode);
                            break;
                        case "while":
                            var whileNode = ParseWhile(atLine, atCol);
                            if (whileNode != null)
                                nodes.Add(whileNode);
                            break;
                        case "switch":
                            var swNode = ParseSwitch(atLine, atCol);
                            if (swNode != null)
                                nodes.Add(swNode);
                            break;
                        case "break":
                            _diagnostics.Add(
                                ErrUnexpectedToken(
                                    "@break",
                                    atLine,
                                    "@for or @while loop block"
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        case "continue":
                            _diagnostics.Add(
                                ErrUnexpectedToken(
                                    "@continue",
                                    atLine,
                                    "@for or @while loop block"
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        case "code":
                            _diagnostics.Add(
                                ErrUnexpectedToken(
                                    "@code",
                                    atLine,
                                    "use setup code before return() instead"
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        case "else":
                            _diagnostics.Add(
                                ErrUnexpectedToken("@else", atLine, "@if block", atCol, atCol + 5)
                            );
                            SkipToEndOfLine();
                            break;
                        case "case":
                        case "default":
                            _diagnostics.Add(
                                ErrUnexpectedToken(
                                    "@" + keyword,
                                    atLine,
                                    "@switch block",
                                    atCol,
                                    atCol + 1 + keyword.Length
                                )
                            );
                            SkipToEndOfLine();
                            break;
                        default:
                            _diagnostics.Add(ErrUnknownDirective(keyword, atLine, atCol));
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
            int openCol = ColAtPos(_scanner.Pos); // 0-based column of the '<'
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
                        _diagnostics.Add(ErrUnclosedTag("<>", openLine));
                }
                else
                {
                    _diagnostics.Add(ErrUnclosedTag("<>", openLine));
                }

                return new ElementNode(
                    string.Empty,
                    ImmutableArray<AttributeNode>.Empty,
                    fragmentChildren.ToImmutableArray(),
                    openLine,
                    _filePath
                )
                {
                    SourceColumn = openCol,
                };
            }

            string tagName = _scanner.ReadTagName();
            if (string.IsNullOrEmpty(tagName))
            {
                _diagnostics.Add(ErrUnexpectedToken("<", openLine, "tag name after '<'"));
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
                )
                {
                    SourceColumn = openCol,
                };
            }

            // Open-block: >
            if (!_scanner.TryConsume('>'))
            {
                _diagnostics.Add(ErrMissingTagClose(tagName, openLine, openCol));
                // Best-effort: continue without consuming
            }

            var children = ParseContent(
                stopTag: tagName,
                stopAtBrace: false,
                stopAtCase: false
            );

            int closeTagLine = 0;

            if (_scanner.IsEof)
            {
                _diagnostics.Add(ErrUnclosedTag(tagName, openLine, openCol));
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
                    _diagnostics.Add(ErrMismatchedTag(closing, tagName, openLine));
                    SkipToTagEnd();
                }
            }

            return new ElementNode(
                tagName,
                attributes,
                children.ToImmutableArray(),
                openLine,
                _filePath
            )
            {
                SourceColumn = openCol,
                CloseTagLine = closeTagLine,
            };
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

                // JSX block comment inside an attribute list: {/* … */}
                // Skip the entire comment so its contents are never interpreted as attributes.
                if (_scanner.IsAt("{/*"))
                {
                    _scanner.TryConsume("{/*");
                    while (!_scanner.IsEof && !_scanner.IsAt("*/}"))
                        _scanner.Advance();
                    _scanner.TryConsume("*/}");
                    continue;
                }

                // A '@' or '<' at this level means a control-flow directive or child
                // element — the opening '>' was never written.  Stop here so the caller
                // continues parsing normally rather than consuming the rest of the file
                // as attribute content.
                if (_scanner.Current == '@' || _scanner.Current == '<')
                    break;

                int attrLine = _scanner.Line;
                int attrCol = ColAtPos(_scanner.Pos); // 0-based column of attribute name start
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
                        attrs.Add(
                            new AttributeNode(name, new StringLiteralValue(lit), attrLine)
                            {
                                SourceColumn = attrCol,
                                NameEndColumn = attrCol + name.Length,
                            }
                        );
                    }
                    else if (!_scanner.IsEof && _scanner.Current == '{')
                    {
                        // Check if brace content is an inline JSX element: attr={<Tag />}
                        if (IsJsxInBraces())
                        {
                            _scanner.Advance(); // consume '{'
                            _scanner.SkipWhitespaceAndNewlines();
                            var element = ParseElement();
                            _scanner.SkipWhitespaceAndNewlines();
                            if (!_scanner.TryConsume('}'))
                                _diagnostics.Add(
                                    ErrUnexpectedToken(
                                        _scanner.IsEof ? "EOF" : _scanner.Current.ToString(),
                                        _scanner.Line,
                                        "'}' after inline JSX attribute value"
                                    )
                                );
                            attrs.Add(
                                new AttributeNode(name, new JsxExpressionValue(element), attrLine)
                                {
                                    SourceColumn = attrCol,
                                    NameEndColumn = attrCol + name.Length,
                                }
                            );
                        }
                        else
                        {
                            var (expr, exprOffset) = _scanner.ReadBraceExpressionWithOffset();
                            attrs.Add(
                                new AttributeNode(
                                    name,
                                    new CSharpExpressionValue(expr, exprOffset),
                                    attrLine
                                )
                                {
                                    SourceColumn = attrCol,
                                    NameEndColumn = attrCol + name.Length,
                                }
                            );
                        }
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
                    attrs.Add(
                        new AttributeNode(name, new BooleanShorthandValue(), attrLine)
                        {
                            SourceColumn = attrCol,
                            NameEndColumn = attrCol + name.Length,
                        }
                    );
                }
            }

            return attrs.ToImmutableArray();
        }

        // ── @if ───────────────────────────────────────────────────────────────

        private IfNode? ParseIf(int startLine, int startCol)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @if", startLine);
                return null;
            }

            var (cond, condOffset) = _scanner.ReadParenExpressionWithOffset();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @if (...)", startLine);
                return null;
            }

            int firstLine = _scanner.Line;
            int firstBrace = _scanner.Pos;
            _scanner.Advance(); // consume '{'
            var firstResult = ParseControlBlockBody(firstBrace, stopAtCase: false, "@if");
            var branches = new List<IfBranch>
            {
                new IfBranch(cond, firstResult.Nodes.ToImmutableArray(), firstLine)
                {
                    ConditionOffset = condOffset,
                    SetupCode = firstResult.SetupCode,
                    SetupCodeOffset = firstResult.SetupCodeOffset,
                    SetupCodeLine = firstResult.SetupCodeLine,
                },
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
                    var (elseCond, elseCondOffset) = _scanner.ReadParenExpressionWithOffset();
                    _scanner.SkipWhitespaceAndNewlines();
                    if (!PeekAt('{'))
                    {
                        EmitExpected("'{' after @else if (...)", elseLine);
                        break;
                    }

                    int elseIfBrace = _scanner.Pos;
                    _scanner.Advance(); // consume '{'
                    var elseIfResult = ParseControlBlockBody(elseIfBrace, stopAtCase: false, "@else if");
                    branches.Add(
                        new IfBranch(elseCond, elseIfResult.Nodes.ToImmutableArray(), elseLine)
                        {
                            ConditionOffset = elseCondOffset,
                            SetupCode = elseIfResult.SetupCode,
                            SetupCodeOffset = elseIfResult.SetupCodeOffset,
                            SetupCodeLine = elseIfResult.SetupCodeLine,
                        }
                    );
                }
                else
                {
                    if (!PeekAt('{'))
                    {
                        EmitExpected("'{' after @else", elseLine);
                        break;
                    }
                    int elseBrace = _scanner.Pos;
                    _scanner.Advance(); // consume '{'
                    var elseResult = ParseControlBlockBody(elseBrace, stopAtCase: false, "@else");
                    branches.Add(new IfBranch(null, elseResult.Nodes.ToImmutableArray(), elseLine)
                    {
                        SetupCode = elseResult.SetupCode,
                        SetupCodeOffset = elseResult.SetupCodeOffset,
                        SetupCodeLine = elseResult.SetupCodeLine,
                    });
                    break; // @else terminates the chain
                }
            }

            return new IfNode(branches.ToImmutableArray(), startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 3, // @if
            };
        }

        // ── @for ──────────────────────────────────────────────────────────────

        private ForNode? ParseFor(int startLine, int startCol)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @for", startLine);
                return null;
            }

            var (forExpr, forExprOffset) = _scanner.ReadParenExpressionWithOffset();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @for (...)", startLine);
                return null;
            }

            int openBrace = _scanner.Pos;
            _scanner.Advance(); // consume '{'
            var result = ParseControlBlockBody(openBrace, stopAtCase: false, "@for");

            return new ForNode(forExpr, result.Nodes.ToImmutableArray(), startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 4, // @for
                ForExpressionOffset = forExprOffset,
                SetupCode = result.SetupCode,
                SetupCodeOffset = result.SetupCodeOffset,
                SetupCodeLine = result.SetupCodeLine,
            };
        }

        // ── @while ────────────────────────────────────────────────────────────

        private WhileNode? ParseWhile(int startLine, int startCol)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @while", startLine);
                return null;
            }

            var (condition, conditionOffset) = _scanner.ReadParenExpressionWithOffset();
            _scanner.SkipWhitespaceAndNewlines();

            if (!PeekAt('{'))
            {
                EmitExpected("'{' after @while (...)", startLine);
                return null;
            }

            int openBrace = _scanner.Pos;
            _scanner.Advance(); // consume '{'
            var result = ParseControlBlockBody(openBrace, stopAtCase: false, "@while");

            return new WhileNode(condition, result.Nodes.ToImmutableArray(), startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 6, // @while
                ConditionOffset = conditionOffset,
                SetupCode = result.SetupCode,
                SetupCodeOffset = result.SetupCodeOffset,
                SetupCodeLine = result.SetupCodeLine,
            };
        }

        // ── @foreach ──────────────────────────────────────────────────────────

        private ForeachNode? ParseForeach(int startLine, int startCol)
        {
            _scanner.SkipInlineWhitespace();

            if (!PeekAt('('))
            {
                EmitExpected("'(' after @foreach", startLine);
                return null;
            }

            var (foreachExpr, foreachExprOffset) = _scanner.ReadParenExpressionWithOffset();
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

            int openBrace = _scanner.Pos;
            _scanner.Advance(); // consume '{'
            var result = ParseControlBlockBody(openBrace, stopAtCase: false, "@foreach");

            return new ForeachNode(
                iteratorDecl,
                collectionExpr,
                result.Nodes.ToImmutableArray(),
                startLine,
                _filePath
            )
            {
                SourceColumn = startCol,
                EndColumn = startCol + 8, // @foreach
                ForeachExpression = foreachExpr,
                ForeachExpressionOffset = foreachExprOffset,
                SetupCode = result.SetupCode,
                SetupCodeOffset = result.SetupCodeOffset,
                SetupCodeLine = result.SetupCodeLine,
            };
        }

        // ── @switch ───────────────────────────────────────────────────────────

        private SwitchNode? ParseSwitch(int startLine, int startCol)
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

            int switchOpenBrace = _scanner.Pos;
            _scanner.Advance(); // consume '{'

            // Find the matching '}' for the entire switch block
            int switchCloseBrace = ReturnFinder.FindMatchingBrace(_source, switchOpenBrace, _source.Length);

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
                int caseCol = ColAtPos(_scanner.Pos);
                _scanner.Advance(); // consume '@'
                string keyword = _scanner.ReadIdentifier();

                if (keyword == "case" || keyword == "default")
                {
                    string? caseVal = null;
                    if (keyword == "case")
                    {
                        _scanner.SkipInlineWhitespace();
                        int vStart = _scanner.Pos;
                        while (
                            !_scanner.IsEof
                            && _scanner.Current != ':'
                            && _scanner.Current != '\r'
                            && _scanner.Current != '\n'
                        )
                            _scanner.Advance();
                        caseVal = _source.Substring(vStart, _scanner.Pos - vStart).Trim();
                    }
                    _scanner.TryConsume(':');

                    int caseBodyStart = _scanner.Pos;

                    // Find the end of this case body — the next @case/@default at depth 0
                    // or the switch closing brace.
                    int caseBodyEnd = switchCloseBrace >= 0 ? switchCloseBrace : _source.Length;
                    int searchPos = caseBodyStart;
                    int braceDepth = 0;
                    while (searchPos < caseBodyEnd)
                    {
                        if (ReturnFinder.TrySkipNonCodeSpan(_source, ref searchPos, caseBodyEnd))
                            continue;
                        char ch = _source[searchPos];
                        if (ch == '{') { braceDepth++; searchPos++; continue; }
                        if (ch == '}') { if (braceDepth > 0) { braceDepth--; searchPos++; continue; } break; }
                        if (braceDepth == 0 && ch == '@')
                        {
                            // Check for @case or @default or @break
                            int peek = searchPos + 1;
                            while (peek < caseBodyEnd && char.IsLetter(_source[peek])) peek++;
                            string kw = _source.Substring(searchPos + 1, peek - searchPos - 1);
                            if (kw == "case" || kw == "default") break;
                            if (kw == "break") break; // @break is a case terminator
                        }
                        searchPos++;
                    }

                    // Now try to find return() in [caseBodyStart, searchPos)
                    if (ReturnFinder.TryFindTopLevelReturn(
                            _source, caseBodyStart, searchPos,
                            out int retStart, out int retOpen, out int retClose, out int retEnd,
                            useLastReturn: false))
                    {
                        string? setupCode = null;
                        int setupCodeOffset = 0;
                        int setupCodeLine = 0;
                        string rawSetup = _source.Substring(caseBodyStart, retStart - caseBodyStart).Trim();
                        if (rawSetup.Length > 0)
                        {
                            setupCode = rawSetup;
                            int firstNonWs = caseBodyStart;
                            while (firstNonWs < retStart && char.IsWhiteSpace(_source[firstNonWs]))
                                firstNonWs++;
                            setupCodeOffset = firstNonWs;
                            setupCodeLine = ReturnFinder.LineAtPos(_source, firstNonWs);
                        }

                        _scanner.AdvanceTo(retOpen + 1);
                        int savedStop = _stopPosExclusive;
                        _stopPosExclusive = retClose;
                        var caseBody = ParseContent(null, stopAtBrace: false, stopAtCase: false);
                        _stopPosExclusive = savedStop;

                        _scanner.AdvanceTo(retEnd); // past ");
                        TryConsumeSwitchBreak();
                        cases.Add(new SwitchCase(caseVal, caseBody.ToImmutableArray(), caseLine)
                        {
                            SetupCode = setupCode,
                            SetupCodeOffset = setupCodeOffset,
                            SetupCodeLine = setupCodeLine,
                        });
                    }
                    else
                    {
                        _diagnostics.Add(new ParseDiagnostic
                        {
                            Code = "UITKX0024",
                            Severity = ParseSeverity.Error,
                            SourceLine = ReturnFinder.LineAtPos(_source, caseBodyStart),
                            Message = $"Switch case body must contain 'return (...);'.",
                        });

                        // Error recovery: parse as pure markup
                        var fallbackBody = ParseContent(null, stopAtBrace: true, stopAtCase: true);
                        TryConsumeSwitchBreak();
                        cases.Add(new SwitchCase(caseVal, fallbackBody.ToImmutableArray(), caseLine));
                    }
                }
                else
                {
                    _diagnostics.Add(
                        ErrUnexpectedToken(
                            "@" + keyword,
                            caseLine,
                            "@case or @default",
                            caseCol,
                            caseCol + 1 + keyword.Length
                        )
                    );
                    SkipToEndOfLine();
                }
            }

            _scanner.TryConsume('}');

            return new SwitchNode(switchExpr, cases.ToImmutableArray(), startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 7, // @switch
            };
        }

        // ── Single element parsing (used by VDG and semantic tokens) ──────

        /// <summary>
        /// Parses a single element from an arbitrary position in <paramref name="source"/>
        /// by creating a temporary sub-parser.  Does NOT mutate the current parser's state.
        /// </summary>
        internal static (ElementNode? Element, int EndPos) ParseSingleElement(
            string source,
            string filePath,
            int startPos,
            int startLine,
            List<ParseDiagnostic> diagnostics
        )
        {
            var parser = new UitkxParser(
                source,
                filePath,
                startPos,
                startLine,
                stopPosExclusive: -1,
                diagnostics
            );
            var element = parser.ParseElement();
            return (element, parser._scanner.Pos);
        }

        /// <summary>Returns the 1-based line number for the given character offset in <c>_source</c>.</summary>
        private int LineAtPos(int pos)
        {
            int line = 1;
            for (int i = 0; i < pos && i < _source.Length; i++)
                if (_source[i] == '\n')
                    line++;
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
        /// While positioned at <c>{</c>, peeks ahead to check whether the brace
        /// content starts with a JSX element (e.g. <c>element={&lt;Dashboard /&gt;}</c>).
        /// Returns <c>true</c> when the first non-whitespace character after <c>{</c>
        /// is <c>&lt;</c> followed by a letter (tag name) or <c>&gt;</c> (fragment).
        /// </summary>
        private bool IsJsxInBraces()
        {
            int i = _scanner.Pos + 1; // past '{'
            while (i < _source.Length && char.IsWhiteSpace(_source[i]))
                i++;
            if (i >= _source.Length || _source[i] != '<')
                return false;
            i++;
            if (i >= _source.Length)
                return false;
            char afterLt = _source[i];
            return char.IsLetter(afterLt) || afterLt == '>';
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

        private void ConsumeOptionalDirectiveTerminator()
        {
            _scanner.SkipInlineWhitespace();
            _scanner.TryConsume(';');
        }

        private void TryConsumeSwitchBreak()
        {
            _scanner.SkipWhitespaceAndNewlines();
            if (_scanner.IsEof || _scanner.Current != '@')
                return;

            string? kw = PeekDirectiveKeyword();
            if (!string.Equals(kw, "break", StringComparison.Ordinal))
                return;

            _scanner.Advance();
            _scanner.ReadIdentifier();
            ConsumeOptionalDirectiveTerminator();
        }

        // ── ParseDiagnostic factory helpers ───────────────────────────────────

        private ParseDiagnostic ErrUnexpectedToken(
            string got,
            int line,
            string expected,
            int col = 0,
            int endCol = 0
        ) =>
            new ParseDiagnostic
            {
                Code = "UITKX0300",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                SourceColumn = col,
                EndLine = line,
                EndColumn = endCol > 0 ? endCol : col,
                Message =
                    $"Unexpected '{got}' at line {line} in '{_filePath}'. Expected {expected}.",
            };

        private ParseDiagnostic ErrMissingTagClose(string tagName, int line, int openCol) =>
            new ParseDiagnostic
            {
                Code = "UITKX0303",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                SourceColumn = openCol, // 0-based column of '<'
                EndLine = line,
                EndColumn = openCol + 1 + tagName.Length, // covers '<TagName'
                Message = $"Missing '>' or '/>' after tag '<{tagName}>' at line {line}.",
            };

        private ParseDiagnostic ErrUnclosedTag(string tagName, int line, int openCol = 0) =>
            new ParseDiagnostic
            {
                Code = "UITKX0301",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                SourceColumn = openCol,
                EndLine = line,
                EndColumn = openCol > 0 ? openCol + 1 + tagName.Length : 0,
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

        private ParseDiagnostic ErrUnknownDirective(string keyword, int line, int col = 0) =>
            new ParseDiagnostic
            {
                Code = "UITKX0305",
                Severity = ParseSeverity.Error,
                SourceLine = line,
                SourceColumn = col,
                EndLine = line,
                EndColumn = col > 0 ? col + 1 + keyword.Length : 0,
                Message =
                    $"Unknown markup directive '@{keyword}' at line {line} in '{_filePath}'. "
                    + "Valid directives are: if, else, foreach, for, while, switch, case, default, break, continue, code.",
            };
    }
}
