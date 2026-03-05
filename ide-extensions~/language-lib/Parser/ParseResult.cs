using System.Collections.Immutable;

namespace ReactiveUITK.Language.Parser
{
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
        int MarkupStartIndex
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
