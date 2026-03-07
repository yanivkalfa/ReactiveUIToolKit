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
        /// Parameters declared in the function-style component header:
        /// <c>component Name(int X = 0, string Label = "")</c>.
        ///
        /// When non-empty the source generator auto-derives a companion props class
        /// named <c>{ComponentName}Props</c> and exposes each parameter as a local
        /// variable in the Render method body.
        ///
        /// Default: <c>default</c> (empty / not used).
        /// </summary>
        ImmutableArray<FunctionParam> FunctionParams = default
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
