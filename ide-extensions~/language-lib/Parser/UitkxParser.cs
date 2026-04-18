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
        /// <summary>
        /// 0 in a full-file parse. In a snippet mini-parse (source = substring),
        /// set to <c>absoluteStartLine - 1</c> so that all <see cref="ReturnFinder.LineAtPos"/>
        /// and <see cref="DirectiveParser.FindJsxBlockRanges"/> results are offset to
        /// absolute file line numbers.
        /// </summary>
        private readonly int _lineOffset;

        // ΓöÇΓöÇ Construction ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private UitkxParser(
            string source,
            string filePath,
            int startPos,
            int startLine,
            int stopPosExclusive,
            List<ParseDiagnostic> diagnostics,
            int lineOffset = 0
        )
        {
            _source = source;
            _filePath = filePath;
            _stopPosExclusive = stopPosExclusive;
            _scanner = new MarkupTokenizer(source, startPos, startLine);
            _diagnostics = diagnostics;
            _lineOffset = lineOffset;
        }

        // ΓöÇΓöÇ Public entry point ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Parses the markup section (everything at or after
        /// <see cref="DirectiveSet.MarkupStartIndex"/>) and returns root AST nodes.
        /// </summary>
        public static ImmutableArray<AstNode> Parse(
            string source,
            string filePath,
            DirectiveSet directives,
            List<ParseDiagnostic> diagnostics,
            bool validateSingleRoot = false,
            int lineOffset = 0
        )
        {
            var parser = new UitkxParser(
                source,
                filePath,
                directives.MarkupStartIndex,
                directives.MarkupStartLine,
                directives.MarkupEndIndex,
                diagnostics,
                lineOffset
            );

            var nodes = parser
                .ParseContent(stopTag: null, stopAtBrace: false, stopAtCase: false);

            if (validateSingleRoot)
                parser.ValidateSingleRoot(
                    nodes, "variable assignment",
                    directives.MarkupStartIndex, directives.MarkupEndIndex);

            return nodes.ToImmutableArray();
        }

        // ΓöÇΓöÇ Control block body parsing ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Parsed result of a control block body ΓÇö the entire body is kept as
        /// raw C# code, with JSX ranges identified for later splicing.
        /// </summary>
        private readonly struct ControlBlockBody
        {
            /// <summary>Complete body code (C# with return statements at any depth).</summary>
            public readonly string? BodyCode;

            /// <summary>Absolute char offset in source where <see cref="BodyCode"/> begins.</summary>
            public readonly int BodyCodeOffset;

            /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
            public readonly int BodyCodeLine;

            /// <summary>Paren-wrapped JSX ranges (absolute positions) within source.</summary>
            public readonly ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges;

            /// <summary>Bare JSX ranges (absolute positions) within source.</summary>
            public readonly ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges;

            /// <summary>
            /// Parsed JSX elements from the body ranges, for IDE features
            /// (semantic tokens, IntelliSense, diagnostics).
            /// </summary>
            public readonly ImmutableArray<AstNode> Body;

            public ControlBlockBody(
                string? bodyCode,
                int bodyCodeOffset,
                int bodyCodeLine,
                ImmutableArray<(int Start, int End, int Line)> bodyMarkupRanges,
                ImmutableArray<(int Start, int End, int Line)> bodyBareJsxRanges,
                ImmutableArray<AstNode> body
            )
            {
                BodyCode = bodyCode;
                BodyCodeOffset = bodyCodeOffset;
                BodyCodeLine = bodyCodeLine;
                BodyMarkupRanges = bodyMarkupRanges;
                BodyBareJsxRanges = bodyBareJsxRanges;
                Body = body;
            }
        }

        /// <summary>
        /// Parses a control block body: the text between <c>{</c> (already consumed)
        /// and its matching <c>}</c>.  Extracts the entire body as raw C# code
        /// and identifies embedded JSX ranges for later splicing by the emitters.
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
                // Unbalanced ΓÇö skip to best-effort end
                while (!_scanner.IsEof && _scanner.Current != '}')
                    _scanner.Advance();
                _scanner.TryConsume('}');
                return new ControlBlockBody(
                    null, bodyStart, ReturnFinder.LineAtPos(_source, bodyStart) + _lineOffset,
                    ImmutableArray<(int, int, int)>.Empty,
                    ImmutableArray<(int, int, int)>.Empty,
                    ImmutableArray<AstNode>.Empty
                );
            }

            int bodyEnd = closeBrace; // exclusive: position of '}'

            // Extract the entire body as raw C# code
            string bodyCode = _source.Substring(bodyStart, bodyEnd - bodyStart).TrimEnd();
            int bodyCodeOffset = bodyStart;
            int bodyCodeLine = ReturnFinder.LineAtPos(_source, bodyStart) + _lineOffset;

            // Find paren-wrapped JSX ranges (absolute positions)
            var markupRanges = DirectiveParser.FindJsxBlockRanges(_source, bodyStart, bodyEnd);

            // Find bare JSX ranges (absolute positions)
            var bareJsxRanges = DirectiveParser.FindBareJsxRanges(_source, bodyStart, bodyEnd);

            // When parsing a snippet source (mini-parse from SpliceBodyCodeMarkup),
            // LineAtPos results are relative to the snippet start. Offset them so that
            // all BodyMarkupRanges.Line values are absolute file line numbers.
            if (_lineOffset > 0)
            {
                markupRanges = OffsetLineValues(markupRanges, _lineOffset);
                bareJsxRanges = OffsetLineValues(bareJsxRanges, _lineOffset);
            }

            // Parse body content into AST nodes for IDE features (formatter,
            // semantic tokens, IntelliSense). Uses TryFindTopLevelReturn to locate
            // a depth-0 return statement, then ParseContent between its parens to
            // produce the correct structural AST (including nested directives).
            var bodyNodes = ParseBodyForIde(bodyCode, bodyStart, bodyEnd, bodyCodeLine);

            // Advance scanner past the closing '}'
            _scanner.AdvanceTo(closeBrace);
            _scanner.TryConsume('}');

            return new ControlBlockBody(
                bodyCode, bodyCodeOffset, bodyCodeLine,
                markupRanges, bareJsxRanges, bodyNodes
            );
        }

        /// <summary>
        /// Parses JSX content at the given source ranges into AST nodes.
        /// Used to provide parsed children for IDE features while emitters
        /// use the raw <c>BodyCode</c> string.
        /// </summary>
        private ImmutableArray<AstNode> ParseJsxFragments(
            ImmutableArray<(int Start, int End, int Line)> markupRanges,
            ImmutableArray<(int Start, int End, int Line)> bareJsxRanges
        )
        {
            if (markupRanges.IsDefaultOrEmpty && bareJsxRanges.IsDefaultOrEmpty)
                return ImmutableArray<AstNode>.Empty;

            var result = ImmutableArray.CreateBuilder<AstNode>();

            foreach (var (start, end, line) in markupRanges)
            {
                var parser = new UitkxParser(
                    _source, _filePath, start, line, end, _diagnostics, _lineOffset);
                var nodes = parser.ParseContent(
                    stopTag: null, stopAtBrace: false, stopAtCase: false);
                result.AddRange(nodes);
            }

            foreach (var (start, end, line) in bareJsxRanges)
            {
                var parser = new UitkxParser(
                    _source, _filePath, start, line, end, _diagnostics, _lineOffset);
                var nodes = parser.ParseContent(
                    stopTag: null, stopAtBrace: false, stopAtCase: false);
                result.AddRange(nodes);
            }

            return result.ToImmutable();
        }

        /// <summary>
        /// Produces parsed <see cref="AstNode"/> children for a directive body,
        /// used by the formatter, semantic tokens, and IntelliSense.
        /// <para>
        /// If a depth-0 <c>return (...)</c> is found, the content between the
        /// parens is parsed with <see cref="ParseContent"/> so that nested
        /// directives (like <c>@if</c> inside <c>@foreach</c>) appear as
        /// structural AST nodes rather than flat text.
        /// </para>
        /// <para>
        /// If no return is found but the body contains markup, the entire body
        /// is parsed as content (bare JSX without <c>return</c>).
        /// </para>
        /// </summary>
        private ImmutableArray<AstNode> ParseBodyForIde(
            string bodyCode, int bodyStart, int bodyEnd, int bodyCodeLine)
        {
            // Try to find a depth-0 return statement in the body
            if (ReturnFinder.TryFindTopLevelReturn(
                    bodyCode, 0, bodyCode.Length,
                    out int returnStart, out int openParen,
                    out int closeParen, out _,
                    useLastReturn: false))
            {
                if (openParen >= 0 && closeParen > openParen)
                {
                    // Parse content between the return's parentheses
                    int contentStart = bodyStart + openParen + 1;
                    int contentEnd = bodyStart + closeParen;
                    int contentLine = ReturnFinder.LineAtPos(_source, contentStart) + _lineOffset;
                    var parser = new UitkxParser(
                        _source, _filePath, contentStart, contentLine,
                        contentEnd, _diagnostics, _lineOffset);
                    var nodes = parser.ParseContent(
                        stopTag: null, stopAtBrace: false, stopAtCase: false);
                    return nodes.ToImmutableArray();
                }

                // return null; — first return is null.  The body may still have
                // a subsequent 'return <JSX>' (e.g. guard clauses like
                // 'if (cond) return null;'). Retry with useLastReturn: true to
                // find the actual JSX return so Body is correctly populated for
                // validators (UITKX0009 etc.) and IDE diagnostics.
                if (openParen == -1)
                {
                    if (ReturnFinder.TryFindTopLevelReturn(
                            bodyCode, 0, bodyCode.Length,
                            out _, out int lp, out int lc, out _,
                            useLastReturn: true))
                    {
                        if (lp >= 0 && lc > lp)
                        {
                            // Paren-wrapped: return (<JSX>)
                            int cs = bodyStart + lp + 1;
                            int ce = bodyStart + lc;
                            int cl = ReturnFinder.LineAtPos(_source, cs) + _lineOffset;
                            var p3 = new UitkxParser(_source, _filePath, cs, cl, ce, _diagnostics, _lineOffset);
                            return p3.ParseContent(stopTag: null, stopAtBrace: false, stopAtCase: false).ToImmutableArray();
                        }
                        if (lp >= 0 && lc >= lp)
                        {
                            // Bare JSX: return <Tag/>  (openParen = jsxStart-1, closeParen = jsxEnd)
                            int js = bodyStart + lp + 1;
                            int je = bodyStart + lc;
                            int jl = ReturnFinder.LineAtPos(_source, js) + _lineOffset;
                            var p4 = new UitkxParser(_source, _filePath, js, jl, je, _diagnostics, _lineOffset);
                            return p4.ParseContent(stopTag: null, stopAtBrace: false, stopAtCase: false).ToImmutableArray();
                        }
                    }
                    return ImmutableArray<AstNode>.Empty;
                }

                // return <Tag/> ΓÇö bare JSX return, parse from the JSX start
                int jsxStart = bodyStart + openParen + 1; // synthesized: openParen+1 == jsxStart
                int jsxEnd = bodyStart + closeParen;      // synthesized: closeParen == jsxEnd
                int jsxLine = ReturnFinder.LineAtPos(_source, jsxStart) + _lineOffset;
                {
                    var parser = new UitkxParser(
                        _source, _filePath, jsxStart, jsxLine,
                        jsxEnd, _diagnostics, _lineOffset);
                    var nodes = parser.ParseContent(
                        stopTag: null, stopAtBrace: false, stopAtCase: false);
                    return nodes.ToImmutableArray();
                }
            }

            // No return found ΓÇö try parsing entire body as markup
            // (handles bare <Element/> in body without explicit return)
            if (bodyCode.IndexOf('<') >= 0)
            {
                var parser = new UitkxParser(
                    _source, _filePath, bodyStart, bodyCodeLine,
                    bodyEnd, _diagnostics, _lineOffset);
                var nodes = parser.ParseContent(
                    stopTag: null, stopAtBrace: false, stopAtCase: false);
                return nodes.ToImmutableArray();
            }

            return ImmutableArray<AstNode>.Empty;
        }

        /// <summary>
        /// Emits UITKX0025 if the parsed body contains more than one significant
        /// root node (ElementNode, ExpressionNode, or non-whitespace TextNode).
        /// The diagnostic spans from <paramref name="rangeStart"/> to <paramref name="rangeEnd"/>.
        /// </summary>
        private void ValidateSingleRoot(
            List<AstNode> nodes,
            string blockName,
            int rangeStart,
            int rangeEnd
        )
        {
            int rootCount = 0;
            foreach (var node in nodes)
            {
                bool significant = node switch
                {
                    ElementNode => true,
                    ExpressionNode => true,
                    TextNode t => !string.IsNullOrWhiteSpace(t.Content),
                    _ => false,
                };
                if (!significant)
                    continue;
                rootCount++;
                if (rootCount == 2)
                    break;
            }

            if (rootCount > 1)
            {
                int startLine = ReturnFinder.LineAtPos(_source, rangeStart);
                int startCol = ColAtPos(rangeStart);
                int endLine = ReturnFinder.LineAtPos(_source, rangeEnd);
                int endCol = ColAtPos(rangeEnd);

                _diagnostics.Add(
                    new ParseDiagnostic
                    {
                        Code = "UITKX0025",
                        Severity = ParseSeverity.Error,
                        SourceLine = startLine,
                        SourceColumn = startCol,
                        EndLine = endLine,
                        EndColumn = endCol,
                        Message =
                            $"JSX expression in {blockName} must have a single root element. Wrap multiple elements in a container like <VisualElement>.",
                    }
                );
            }
        }

        // ΓöÇΓöÇ Content loop ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
                if (_stopPosExclusive >= 0 && _scanner.Pos >= _stopPosExclusive)
                    break;

                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsEof)
                    break;

                if (_stopPosExclusive >= 0 && _scanner.Pos >= _stopPosExclusive)
                    break;

                int positionBefore = _scanner.Pos;
                char c = _scanner.Current;

                // ΓöÇΓöÇ Stop: bare '}' ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (stopAtBrace && c == '}')
                    break;

                // ΓöÇΓöÇ Stop: @case / @default inside @switch content ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (stopAtCase && c == '@')
                {
                    string? kw = PeekDirectiveKeyword();
                    if (kw == "case" || kw == "default" || kw == "break")
                        break;
                }

                // ΓöÇΓöÇ Stop: matching closing tag ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (c == '<' && PeekChar(1) == '/')
                {
                    string closing = PeekClosingTagName();
                    if (
                        stopTag == null
                        || string.Equals(closing, stopTag, StringComparison.Ordinal)
                    )
                        break;
                    // Closing tag does not match expected ΓÇö emit error and skip '<'
                    _diagnostics.Add(ErrMismatchedTag(closing, stopTag ?? "?", _scanner.Line));
                    _scanner.Advance();
                    continue;
                }

                // ΓöÇΓöÇ HTML comment <!-- ... --> ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (_scanner.TrySkipHtmlComment())
                    continue;

                // ΓöÇΓöÇ Line comment // ... or block comment /* ... */ ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (c == '/')
                {
                    int commentLine = _scanner.Line;
                    if (_scanner.TrySkipLineComment(out string lineContent))
                    {
                        nodes.Add(new CommentNode(lineContent, commentLine, _filePath, IsBlock: false));
                        continue;
                    }
                    if (_scanner.TrySkipBlockComment(out string blockContent))
                    {
                        nodes.Add(new CommentNode(blockContent, commentLine, _filePath, IsBlock: true));
                        continue;
                    }
                }

                // ΓöÇΓöÇ Child expression {expr} ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (c == '{')
                {
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

                // ΓöÇΓöÇ Opening element <Tag ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (c == '<' && PeekChar(1) != '/')
                {
                    var elem = ParseElement();
                    if (elem != null)
                        nodes.Add(elem);
                    continue;
                }

                // ΓöÇΓöÇ Directive / control flow / inline @(expr) ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
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
                                ErrUnexpectedToken("@break", atLine, "@for or @while loop block")
                            );
                            SkipToEndOfLine();
                            break;
                        case "continue":
                            _diagnostics.Add(
                                ErrUnexpectedToken("@continue", atLine, "@for or @while loop block")
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

                // ΓöÇΓöÇ Text content ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
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

        // ΓöÇΓöÇ Element ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private ElementNode? ParseElement()
        {
            int openLine = _scanner.Line;
            int openCol = ColAtPos(_scanner.Pos); // 0-based column of the '<'
            _scanner.Advance(); // consume '<'
            _scanner.SkipInlineWhitespace();

            // <></> short-hand fragment ΓÇö empty tag name maps to V.Fragment in the emitter.
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

            var children = ParseContent(stopTag: tagName, stopAtBrace: false, stopAtCase: false);

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

        // ΓöÇΓöÇ Attributes ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private ImmutableArray<AttributeNode> ParseAttributes()
        {
            var attrs = new List<AttributeNode>();

            while (!_scanner.IsEof)
            {
                _scanner.SkipWhitespaceAndNewlines();
                if (_scanner.IsAt("/>") || _scanner.Current == '>' || _scanner.IsEof)
                    break;

                // JSX block comment inside an attribute list: {/* ΓÇª */}
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
                // element ΓÇö the opening '>' was never written.  Stop here so the caller
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

        // ΓöÇΓöÇ @if ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
                new IfBranch(cond, firstLine)
                {
                    ConditionOffset = condOffset,
                    BodyCode = firstResult.BodyCode,
                    BodyCodeOffset = firstResult.BodyCodeOffset,
                    BodyCodeLine = firstResult.BodyCodeLine,
                    BodyMarkupRanges = firstResult.BodyMarkupRanges,
                    BodyBareJsxRanges = firstResult.BodyBareJsxRanges,
                    Body = firstResult.Body,
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
                    var elseIfResult = ParseControlBlockBody(
                        elseIfBrace,
                        stopAtCase: false,
                        "@else if"
                    );
                    branches.Add(
                        new IfBranch(elseCond, elseLine)
                        {
                            ConditionOffset = elseCondOffset,
                            BodyCode = elseIfResult.BodyCode,
                            BodyCodeOffset = elseIfResult.BodyCodeOffset,
                            BodyCodeLine = elseIfResult.BodyCodeLine,
                            BodyMarkupRanges = elseIfResult.BodyMarkupRanges,
                            BodyBareJsxRanges = elseIfResult.BodyBareJsxRanges,
                            Body = elseIfResult.Body,
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
                    branches.Add(
                        new IfBranch(null, elseLine)
                        {
                            BodyCode = elseResult.BodyCode,
                            BodyCodeOffset = elseResult.BodyCodeOffset,
                            BodyCodeLine = elseResult.BodyCodeLine,
                            BodyMarkupRanges = elseResult.BodyMarkupRanges,
                            BodyBareJsxRanges = elseResult.BodyBareJsxRanges,
                            Body = elseResult.Body,
                        }
                    );
                    break; // @else terminates the chain
                }
            }

            return new IfNode(branches.ToImmutableArray(), startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 3, // @if
            };
        }

        // ΓöÇΓöÇ @for ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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

            return new ForNode(forExpr, startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 4, // @for
                ForExpressionOffset = forExprOffset,
                BodyCode = result.BodyCode,
                BodyCodeOffset = result.BodyCodeOffset,
                BodyCodeLine = result.BodyCodeLine,
                BodyMarkupRanges = result.BodyMarkupRanges,
                BodyBareJsxRanges = result.BodyBareJsxRanges,
                Body = result.Body,
            };
        }

        // ΓöÇΓöÇ @while ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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

            return new WhileNode(condition, startLine, _filePath)
            {
                SourceColumn = startCol,
                EndColumn = startCol + 6, // @while
                ConditionOffset = conditionOffset,
                BodyCode = result.BodyCode,
                BodyCodeOffset = result.BodyCodeOffset,
                BodyCodeLine = result.BodyCodeLine,
                BodyMarkupRanges = result.BodyMarkupRanges,
                BodyBareJsxRanges = result.BodyBareJsxRanges,
                Body = result.Body,
            };
        }

        // ΓöÇΓöÇ @foreach ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
                startLine,
                _filePath
            )
            {
                SourceColumn = startCol,
                EndColumn = startCol + 8, // @foreach
                ForeachExpression = foreachExpr,
                ForeachExpressionOffset = foreachExprOffset,
                BodyCode = result.BodyCode,
                BodyCodeOffset = result.BodyCodeOffset,
                BodyCodeLine = result.BodyCodeLine,
                BodyMarkupRanges = result.BodyMarkupRanges,
                BodyBareJsxRanges = result.BodyBareJsxRanges,
                Body = result.Body,
            };
        }

        // ΓöÇΓöÇ @switch ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
            int switchCloseBrace = ReturnFinder.FindMatchingBrace(
                _source,
                switchOpenBrace,
                _source.Length
            );

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

                    // Find the end of this case body ΓÇö the next @case/@default at depth 0
                    // or the switch closing brace.
                    int caseBodyStart = _scanner.Pos;
                    int caseBodyEnd = switchCloseBrace >= 0 ? switchCloseBrace : _source.Length;
                    int searchPos = caseBodyStart;
                    int braceDepth = 0;
                    while (searchPos < caseBodyEnd)
                    {
                        if (ReturnFinder.TrySkipNonCodeSpan(_source, ref searchPos, caseBodyEnd))
                            continue;
                        char ch = _source[searchPos];
                        if (ch == '{')
                        {
                            braceDepth++;
                            searchPos++;
                            continue;
                        }
                        if (ch == '}')
                        {
                            if (braceDepth > 0)
                            {
                                braceDepth--;
                                searchPos++;
                                continue;
                            }
                            break;
                        }
                        if (braceDepth == 0 && ch == '@')
                        {
                            // Check for @case or @default or @break
                            int peek = searchPos + 1;
                            while (peek < caseBodyEnd && char.IsLetter(_source[peek]))
                                peek++;
                            string kw = _source.Substring(searchPos + 1, peek - searchPos - 1);
                            if (kw == "case" || kw == "default")
                                break;
                            if (kw == "break")
                                break; // @break is a case terminator
                        }
                        searchPos++;
                    }

                    // Extract the case body as raw C# code
                    string bodyCode = _source.Substring(caseBodyStart, searchPos - caseBodyStart).TrimEnd();
                    int bodyCodeLine = ReturnFinder.LineAtPos(_source, caseBodyStart);

                    // Find JSX ranges within this case body
                    var markupRanges = DirectiveParser.FindJsxBlockRanges(_source, caseBodyStart, searchPos);
                    var bareJsxRanges = DirectiveParser.FindBareJsxRanges(_source, caseBodyStart, searchPos);

                    // Parse JSX ranges into AST nodes for IDE features
                    var bodyNodes = ParseBodyForIde(bodyCode, caseBodyStart, searchPos, bodyCodeLine);

                    _scanner.AdvanceTo(searchPos);
                    TryConsumeSwitchBreak();

                    cases.Add(
                        new SwitchCase(caseVal, caseLine)
                        {
                            BodyCode = bodyCode,
                            BodyCodeOffset = caseBodyStart,
                            BodyCodeLine = bodyCodeLine,
                            BodyMarkupRanges = markupRanges,
                            BodyBareJsxRanges = bareJsxRanges,
                            Body = bodyNodes,
                        }
                    );
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

        // ΓöÇΓöÇ Single element parsing (used by VDG and semantic tokens) ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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

        /// <summary>
        /// Offsets each <c>Line</c> field in <paramref name="ranges"/> by <paramref name="offset"/>.
        /// Used to convert snippet-relative line numbers to absolute file line numbers.
        /// </summary>
        private static ImmutableArray<(int Start, int End, int Line)> OffsetLineValues(
            ImmutableArray<(int Start, int End, int Line)> ranges, int offset)
        {
            if (offset == 0 || ranges.IsDefaultOrEmpty)
                return ranges;
            var builder = ImmutableArray.CreateBuilder<(int, int, int)>(ranges.Length);
            foreach (var (s, e, l) in ranges)
                builder.Add((s, e, l + offset));
            return builder.ToImmutable();
        }

        /// <summary>Returns the 0-based column for the given character offset in <c>_source</c>.</summary>
        private int ColAtPos(int pos)
        {
            int col = 0;
            for (int i = pos - 1; i >= 0 && _source[i] != '\n'; i--)
                col++;
            return col;
        }

        // ΓöÇΓöÇ Peek / advance helpers ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
        /// current scanner position ΓÇö without consuming anything.
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

        // ΓöÇΓöÇ ParseDiagnostic factory helpers ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
