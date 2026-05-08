using System.Collections.Immutable;

namespace ReactiveUITK.Language.Nodes
{
    // ── Base ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Base record for every node produced by the UITKX markup parser.
    /// Every node carries the 1-based line number in the .uitkx source file and
    /// the file path — both are used by the emitter to write <c>#line</c> directives
    /// so that compiler errors and debugger breakpoints point at the .uitkx file,
    /// not the generated C#.
    ///
    /// SourceColumn, EndLine, EndColumn default to 0 until the tokenizer is
    /// extended to emit full position data (Phase 3).
    /// </summary>
    public abstract record AstNode(int SourceLine, string SourceFile)
    {
        /// <summary>0-based column of the opening token. 0 until column tracking is wired (Phase 3).</summary>
        public int SourceColumn { get; init; } = 0;

        /// <summary>1-based end line (inclusive). 0 = treat as SourceLine.</summary>
        public int EndLine { get; init; } = 0;

        /// <summary>0-based exclusive end column. 0 until column tracking is wired (Phase 3).</summary>
        public int EndColumn { get; init; } = 0;
    }

    // ── Leaf nodes ────────────────────────────────────────────────────────────

    /// <summary>Raw text content between elements, e.g. "Hello World".</summary>
    public sealed record TextNode(string Content, int SourceLine, string SourceFile)
        : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// Inline C# expression rendered as a child virtual node: <c>{expr}</c>.
    /// The expression is embedded verbatim in the generated C#.
    /// </summary>
    public sealed record ExpressionNode(string Expression, int SourceLine, string SourceFile)
        : AstNode(SourceLine, SourceFile)
    {
        /// <summary>
        /// Absolute character offset in the .uitkx source where the trimmed
        /// expression content begins (i.e. after the opening <c>{</c> and any
        /// leading whitespace). 0 when not tracked.
        /// </summary>
        public int ExpressionOffset { get; init; } = 0;

        /// <summary>
        /// Length of the trimmed expression string in the source.
        /// 0 when not tracked.
        /// </summary>
        public int ExpressionLength { get; init; } = 0;
    }

    /// <summary>
    /// A comment in markup: <c>// line comment</c> or <c>/* block comment */</c>.
    /// Not emitted to C#; preserved by the formatter.
    /// </summary>
    public sealed record CommentNode(string Content, int SourceLine, string SourceFile, bool IsBlock = false)
        : AstNode(SourceLine, SourceFile);

    // ── Attribute value discriminated-union ──────────────────────────────────

    /// <summary>Base record for the three possible forms of an attribute value.</summary>
    public abstract record AttributeValue;

    /// <summary><c>attr="Hello World"</c> — a static string literal.</summary>
    public sealed record StringLiteralValue(string Value) : AttributeValue;

    /// <summary><c>attr={someExpr}</c> — an arbitrary C# expression.</summary>
    public sealed record CSharpExpressionValue(
        string Expression,
        /// <summary>
        /// Absolute character offset in the .uitkx source where the trimmed
        /// expression content begins (after the opening <c>{</c> and leading
        /// whitespace). 0 when not tracked.
        /// </summary>
        int ExpressionOffset = 0
    ) : AttributeValue;

    /// <summary>
    /// <c>disabled</c> — boolean shorthand: attribute present → <c>true</c>,
    /// absent → <c>false</c>.
    /// </summary>
    public sealed record BooleanShorthandValue : AttributeValue;

    /// <summary>
    /// <c>attr={&lt;Tag /&gt;}</c> — an inline JSX element used as an attribute
    /// value. The contained <see cref="ElementNode"/> is emitted as a
    /// <c>VirtualNode</c> expression in the generated C#.
    /// </summary>
    public sealed record JsxExpressionValue(ElementNode? Element) : AttributeValue;

    // ── Attribute ─────────────────────────────────────────────────────────────

    /// <summary>A single attribute on an element, e.g. <c>text="Hi"</c>.</summary>
    public sealed record AttributeNode(string Name, AttributeValue Value, int SourceLine)
    {
        /// <summary>0-based column of the first character of the attribute name. 0 when not tracked.</summary>
        public int SourceColumn { get; init; } = 0;

        /// <summary>0-based column of the character after the last character of the attribute name. 0 when not tracked.</summary>
        public int NameEndColumn { get; init; } = 0;
    }

    // ── Element node ─────────────────────────────────────────────────────────

    /// <summary>
    /// An XML-like element: either self-closing <c>&lt;Tag /&gt;</c> or block
    /// <c>&lt;Tag&gt;...children...&lt;/Tag&gt;</c>.
    ///
    /// Lowercase tag names map to built-in elements; PascalCase tag names map
    /// to function components (resolved in Phase 3).
    /// </summary>
    public sealed record ElementNode(
        string TagName,
        ImmutableArray<AttributeNode> Attributes,
        ImmutableArray<AstNode> Children,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile)
    {
        /// <summary>
        /// 1-based source line of the closing tag (e.g. the line containing
        /// <c>&lt;/Box&gt;</c>).  0 for self-closing elements that have no
        /// closing tag.
        /// </summary>
        public int CloseTagLine { get; init; } = 0;
    }

    // ── Control flow ──────────────────────────────────────────────────────────

    /// <summary>
    /// One branch of an <c>@if</c>/<c>@else if</c>/<c>@else</c> chain.
    /// <see cref="Condition"/> is <c>null</c> for the <c>@else</c> branch.
    /// </summary>
    public sealed record IfBranch(string? Condition, int SourceLine)
    {
        /// <summary>
        /// Absolute character offset in the .uitkx source where the trimmed condition
        /// expression begins (after the opening <c>(</c>). 0 when not tracked (e.g. <c>@else</c>).
        /// </summary>
        public int ConditionOffset { get; init; } = 0;

        /// <summary>Complete body code (C# with return statements at any depth).</summary>
        public string? BodyCode { get; init; }

        /// <summary>Absolute character offset in source where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeOffset { get; init; }

        /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeLine { get; init; }

        /// <summary>Paren-wrapped JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>Bare JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>
        /// Parsed JSX elements extracted from <see cref="BodyMarkupRanges"/> and
        /// <see cref="BodyBareJsxRanges"/> for IDE features (semantic tokens,
        /// IntelliSense, diagnostics). Emitters use <see cref="BodyCode"/> instead.
        /// </summary>
        public ImmutableArray<AstNode> Body { get; init; }
            = ImmutableArray<AstNode>.Empty;
    }

    /// <summary>
    /// An <c>@if</c> control-flow block, holding one or more <see cref="IfBranch"/>
    /// instances (if / else-if / else).
    /// </summary>
    public sealed record IfNode(
        ImmutableArray<IfBranch> Branches,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// An <c>@foreach (var item in collection) { ... }</c> block.
    /// <see cref="IteratorDeclaration"/> is the part before <c>in</c>
    /// (e.g. <c>var item</c>); <see cref="CollectionExpression"/> is the part
    /// after (e.g. <c>props.Items</c>).
    /// </summary>
    public sealed record ForeachNode(
        string IteratorDeclaration,
        string CollectionExpression,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile)
    {
        /// <summary>
        /// Verbatim content between the parentheses, e.g. <c>"var item in props.Items"</c>.
        /// Stored alongside the split fields to enable column-accurate source mapping.
        /// </summary>
        public string ForeachExpression { get; init; } = string.Empty;

        /// <summary>
        /// Absolute character offset in the .uitkx source where <see cref="ForeachExpression"/>
        /// begins (after the opening <c>(</c>). 0 when not tracked.
        /// </summary>
        public int ForeachExpressionOffset { get; init; } = 0;

        /// <summary>Complete body code (C# with return statements at any depth).</summary>
        public string? BodyCode { get; init; }

        /// <summary>Absolute character offset in source where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeOffset { get; init; }

        /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeLine { get; init; }

        /// <summary>Paren-wrapped JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>Bare JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>
        /// Parsed JSX elements extracted from <see cref="BodyMarkupRanges"/> and
        /// <see cref="BodyBareJsxRanges"/> for IDE features (semantic tokens,
        /// IntelliSense, diagnostics). Emitters use <see cref="BodyCode"/> instead.
        /// </summary>
        public ImmutableArray<AstNode> Body { get; init; }
            = ImmutableArray<AstNode>.Empty;
    }

    /// <summary>
    /// A <c>@for (init; condition; increment) { ... }</c> block.
    /// <see cref="ForExpression"/> is the raw content between the parentheses.
    /// </summary>
    public sealed record ForNode(
        string ForExpression,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile)
    {
        /// <summary>
        /// Absolute character offset in the .uitkx source where <see cref="ForExpression"/>
        /// begins (after the opening <c>(</c>). 0 when not tracked.
        /// </summary>
        public int ForExpressionOffset { get; init; } = 0;

        /// <summary>Complete body code (C# with return statements at any depth).</summary>
        public string? BodyCode { get; init; }

        /// <summary>Absolute character offset in source where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeOffset { get; init; }

        /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeLine { get; init; }

        /// <summary>Paren-wrapped JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>Bare JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>
        /// Parsed JSX elements extracted from <see cref="BodyMarkupRanges"/> and
        /// <see cref="BodyBareJsxRanges"/> for IDE features (semantic tokens,
        /// IntelliSense, diagnostics). Emitters use <see cref="BodyCode"/> instead.
        /// </summary>
        public ImmutableArray<AstNode> Body { get; init; }
            = ImmutableArray<AstNode>.Empty;
    }

    /// <summary>
    /// A <c>@while (condition) { ... }</c> block.
    /// </summary>
    public sealed record WhileNode(
        string Condition,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile)
    {
        /// <summary>
        /// Absolute character offset in the .uitkx source where <see cref="Condition"/>
        /// begins (after the opening <c>(</c>). 0 when not tracked.
        /// </summary>
        public int ConditionOffset { get; init; } = 0;

        /// <summary>Complete body code (C# with return statements at any depth).</summary>
        public string? BodyCode { get; init; }

        /// <summary>Absolute character offset in source where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeOffset { get; init; }

        /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeLine { get; init; }

        /// <summary>Paren-wrapped JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>Bare JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>
        /// Parsed JSX elements extracted from <see cref="BodyMarkupRanges"/> and
        /// <see cref="BodyBareJsxRanges"/> for IDE features (semantic tokens,
        /// IntelliSense, diagnostics). Emitters use <see cref="BodyCode"/> instead.
        /// </summary>
        public ImmutableArray<AstNode> Body { get; init; }
            = ImmutableArray<AstNode>.Empty;
    }

    /// <summary>One <c>@case</c> or <c>@default</c> branch inside a switch block.</summary>
    public sealed record SwitchCase(
        /// <summary>The case expression, or <c>null</c> for <c>@default</c>.</summary>
        string? ValueExpression,
        int SourceLine
    )
    {
        /// <summary>Complete body code (C# with return statements at any depth).</summary>
        public string? BodyCode { get; init; }

        /// <summary>Absolute character offset in source where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeOffset { get; init; }

        /// <summary>1-based line number where <see cref="BodyCode"/> begins.</summary>
        public int BodyCodeLine { get; init; }

        /// <summary>Paren-wrapped JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>Bare JSX ranges within <see cref="BodyCode"/>.</summary>
        public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;

        /// <summary>
        /// Parsed JSX elements extracted from <see cref="BodyMarkupRanges"/> and
        /// <see cref="BodyBareJsxRanges"/> for IDE features (semantic tokens,
        /// IntelliSense, diagnostics). Emitters use <see cref="BodyCode"/> instead.
        /// </summary>
        public ImmutableArray<AstNode> Body { get; init; }
            = ImmutableArray<AstNode>.Empty;
    };

    /// <summary>An <c>@switch (expr) { @case ... }</c> block.</summary>
    public sealed record SwitchNode(
        string SwitchExpression,
        ImmutableArray<SwitchCase> Cases,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);
}
