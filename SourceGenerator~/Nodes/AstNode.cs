using System.Collections.Immutable;

namespace ReactiveUITK.SourceGenerator.Nodes
{
    // ── Base ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Base record for every node produced by the UITKX markup parser.
    /// Every node carries the 1-based line number in the .uitkx source file and
    /// the file path — both are used by the emitter to write <c>#line</c> directives
    /// so that compiler errors and debugger breakpoints point at the .uitkx file,
    /// not the generated C#.
    /// </summary>
    public abstract record AstNode(int SourceLine, string SourceFile);

    // ── Leaf nodes ────────────────────────────────────────────────────────────

    /// <summary>Raw text content between elements, e.g. "Hello World".</summary>
    public sealed record TextNode(string Content, int SourceLine, string SourceFile)
        : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// Inline C# expression rendered as a child virtual node: <c>@(expr)</c>.
    /// The expression is embedded verbatim in the generated C#.
    /// </summary>
    public sealed record ExpressionNode(string Expression, int SourceLine, string SourceFile)
        : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// The <c>@code { ... }</c> block at the bottom of a .uitkx file.
    /// Its content is inserted verbatim inside the generated partial class.
    /// </summary>
    public sealed record CodeBlockNode(string Code, int SourceLine, string SourceFile)
        : AstNode(SourceLine, SourceFile);

    // ── Attribute value discriminated-union ──────────────────────────────────

    /// <summary>Base record for the three possible forms of an attribute value.</summary>
    public abstract record AttributeValue;

    /// <summary><c>attr="Hello World"</c> — a static string literal.</summary>
    public sealed record StringLiteralValue(string Value) : AttributeValue;

    /// <summary><c>attr={someExpr}</c> — an arbitrary C# expression.</summary>
    public sealed record CSharpExpressionValue(string Expression) : AttributeValue;

    /// <summary>
    /// <c>disabled</c> — boolean shorthand: attribute present → <c>true</c>,
    /// absent → <c>false</c>.
    /// </summary>
    public sealed record BooleanShorthandValue : AttributeValue;

    // ── Attribute ─────────────────────────────────────────────────────────────

    /// <summary>A single attribute on an element, e.g. <c>text="Hi"</c>.</summary>
    public sealed record AttributeNode(string Name, AttributeValue Value, int SourceLine);

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
    ) : AstNode(SourceLine, SourceFile);

    // ── Control flow ──────────────────────────────────────────────────────────

    /// <summary>
    /// One branch of an <c>@if</c>/<c>@else if</c>/<c>@else</c> chain.
    /// <see cref="Condition"/> is <c>null</c> for the <c>@else</c> branch.
    /// </summary>
    public sealed record IfBranch(string? Condition, ImmutableArray<AstNode> Body, int SourceLine);

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
        ImmutableArray<AstNode> Body,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// A <c>@for (init; condition; increment) { ... }</c> block.
    /// <see cref="ForExpression"/> is the raw content between the parentheses.
    /// </summary>
    public sealed record ForNode(
        string ForExpression,
        ImmutableArray<AstNode> Body,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);

    /// <summary>
    /// A <c>@while (condition) { ... }</c> block.
    /// </summary>
    public sealed record WhileNode(
        string Condition,
        ImmutableArray<AstNode> Body,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);

    /// <summary>One <c>@case</c> or <c>@default</c> branch inside a switch block.</summary>
    public sealed record SwitchCase(
        /// <summary>The case expression, or <c>null</c> for <c>@default</c>.</summary>
        string? ValueExpression,
        ImmutableArray<AstNode> Body,
        int SourceLine
    );

    /// <summary>An <c>@switch (expr) { @case ... }</c> block.</summary>
    public sealed record SwitchNode(
        string SwitchExpression,
        ImmutableArray<SwitchCase> Cases,
        int SourceLine,
        string SourceFile
    ) : AstNode(SourceLine, SourceFile);
}
