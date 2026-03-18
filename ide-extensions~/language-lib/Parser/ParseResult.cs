using System.Collections.Immutable;

namespace ReactiveUITK.Language.Parser
{
    // ── Typed-props function parameter ───────────────────────────────────────

    /// <summary>
    /// A single typed parameter declared inside the function-style component header:
    /// <c>component Name(int X = 0, string Label = "")</c>.
    ///
    /// <list type="bullet">
    ///   <item><description><see cref="Type"/> — verbatim C# type (may include generics, e.g. <c>List&lt;string&gt;</c>).</description></item>
    ///   <item><description><see cref="Name"/> — identifier used both as local variable in the body and (PascalCase) as the property name in the generated props class.</description></item>
    ///   <item><description><see cref="DefaultValue"/> — verbatim default expression, or <c>null</c> if omitted (maps to <c>default</c> in the generated class).</description></item>
    /// </list>
    /// </summary>
    public sealed record FunctionParam(string Type, string Name, string? DefaultValue);

    // ── Directive data ────────────────────────────────────────────────────────

    /// <summary>
    /// The validated set of top-level <c>@directives</c> found at the top of a
    /// .uitkx file.
    /// </summary>
    public sealed record DirectiveSet(
        /// <summary><c>@namespace</c> — required. C# namespace for the generated class.</summary>
        string? Namespace,
        /// <summary><c>@component</c> — required. The generated class name.</summary>
        string? ComponentName,
        /// <summary><c>@props</c> — optional. Fully-qualified props type name.</summary>
        string? PropsTypeName,
        /// <summary><c>@key</c> — optional. Default VirtualNode key string.</summary>
        string? DefaultKey,
        /// <summary>All <c>@using</c> namespace values, in declaration order.</summary>
        ImmutableArray<string> Usings,
        /// <summary>
        /// All <c>@inject Type Name</c> declarations, in declaration order.
        /// Each entry carries the fully-qualified type string and the field name.
        /// </summary>
        ImmutableArray<(string Type, string Name)> Injects,
        /// <summary>
        /// 1-based line number of the first non-directive line (i.e. where the
        /// markup begins). Used by the parser to set its initial line counter.
        /// </summary>
        int MarkupStartLine,
        /// <summary>
        /// Character index into the source string where the markup begins.
        /// Passed to the tokenizer/parser so they start at the correct position.
        /// </summary>
        int MarkupStartIndex,
        /// <summary>
        /// Optional exclusive end index for markup parsing in function-style files.
        /// <c>-1</c> means parse until EOF (legacy directive-based form).
        /// </summary>
        int MarkupEndIndex = -1,
        /// <summary>
        /// True when source uses the function-style component form:
        /// <c>component Name { ... return (...) ... }</c>.
        /// </summary>
        bool IsFunctionStyle = false,
        /// <summary>
        /// True when an <c>@namespace X.Y</c> directive was explicitly written in
        /// the source file.  False when the namespace was inferred from a companion
        /// <c>.cs</c> file (or is the hard-coded fallback namespace).
        /// The formatter uses this to decide whether to re-emit the directive.
        /// </summary>
        bool HasExplicitNamespace = false,
        /// <summary>
        /// Setup C# statements extracted from function-style body (all top-level
        /// statements except the <c>return (...)</c> statement). Injected as a
        /// synthetic <c>@code</c> block before markup emission.
        /// </summary>
        string? FunctionSetupCode = null,
        /// <summary>
        /// 1-based line where function-style setup code begins inside
        /// <c>component Name { ... }</c>. <c>-1</c> when unavailable.
        /// </summary>
        int FunctionSetupStartLine = -1,
        /// <summary>
        /// Absolute character offset in the .uitkx source of the first character
        /// of the trimmed <see cref="FunctionSetupCode"/>.
        /// <c>-1</c> when not tracked (fallback: line-based approximation used).
        /// </summary>
        int FunctionSetupStartOffset = -1,
        /// <summary>
        /// Parameters declared in the function-style component header:
        /// <c>component Name(int X = 0, string Label = "")</c>.
        ///
        /// When non-empty the source generator auto-derives a companion props class
        /// named <c>{ComponentName}Props</c> and exposes each parameter as a local
        /// variable in the Render method body.
        ///
        /// Default: <c>default</c> (empty / not used).
        /// </summary>
        ImmutableArray<FunctionParam> FunctionParams = default,
        /// <summary>
        /// 1-based source line of the <c>component Name {</c> or <c>@component</c>
        /// declaration. Used to attach UITKX0103 (filename mismatch) at the right
        /// location. <c>-1</c> when not tracked.
        /// </summary>
        int ComponentDeclarationLine = -1,
        /// <summary>
        /// 0-based column of the first character of the component NAME (not the
        /// <c>component</c> keyword itself). Used to aim the UITKX0103 squiggle at
        /// the name token. <c>-1</c> when not tracked.
        /// </summary>
        int ComponentNameColumn = -1,
        /// <summary>
        /// 1-based line of the <c>;</c> ending the <c>return (...);</c> statement
        /// in function-style components. <c>-1</c> when not tracked or not
        /// function-style.
        /// </summary>
        int FunctionReturnEndLine = -1,
        /// <summary>
        /// 1-based line of the closing <c>}</c> of the function-style component
        /// body. <c>-1</c> when not tracked.
        /// </summary>
        int FunctionBodyEndLine = -1,
        /// <summary>
        /// Absolute (start, end, line) ranges in the original .uitkx source for each
        /// JSX paren block embedded inside function-style setup code, e.g.
        /// <c>var x = (&lt;Box&gt;...&lt;/Box&gt;)</c>.
        /// <para>
        /// <c>start</c> = char index just inside the opening <c>(</c>;<br/>
        /// <c>end</c>   = exclusive index at the closing <c>)</c>;<br/>
        /// <c>line</c>  = 1-based source line of <c>start</c>.
        /// </para>
        /// Default: empty / not used.
        /// </summary>
        ImmutableArray<(int Start, int End, int Line)> SetupCodeMarkupRanges = default,
        /// <summary>
        /// Absolute (start, end, line) ranges in the original .uitkx source for
        /// bare JSX elements in function-style setup code that are NOT
        /// paren-wrapped — e.g. <c>return &lt;Tag/&gt;</c>,
        /// <c>cond ? &lt;A/&gt; : &lt;B/&gt;</c>, <c>var x = &lt;Tag/&gt;</c>.
        /// Used by the virtual-document generator for expression type-checks
        /// but NOT by the formatter (which only handles paren-wrapped blocks).
        /// </summary>
        ImmutableArray<(int Start, int End, int Line)> SetupCodeBareJsxRanges = default,
        /// <summary>
        /// Position inside the trimmed <see cref="FunctionSetupCode"/> where the
        /// gap left by the removed <c>return (…);</c> statement begins.
        /// Characters at or beyond this offset correspond to source positions
        /// shifted by <see cref="FunctionSetupGapLength"/>.
        /// <c>-1</c> when there is no gap (no return was removed).
        /// </summary>
        int FunctionSetupGapOffset = -1,
        /// <summary>
        /// Number of source characters occupied by the removed <c>return (…);</c>
        /// statement.  Added to the base offset for source-map entries whose
        /// position in <see cref="FunctionSetupCode"/> is at or past
        /// <see cref="FunctionSetupGapOffset"/>.
        /// </summary>
        int FunctionSetupGapLength = 0
    );

    // ── Full parse result ─────────────────────────────────────────────────────

    /// <summary>
    /// The complete result of parsing a .uitkx file: directives, AST, and any
    /// diagnostics produced during tokenization and parsing.
    /// All types are Roslyn-free; suitable for use in both the source generator
    /// and the LSP server.
    /// </summary>
    public sealed record ParseResult(
        DirectiveSet Directives,
        ImmutableArray<ReactiveUITK.Language.Nodes.AstNode> RootNodes,
        ImmutableArray<ParseDiagnostic> Diagnostics
    );
}
