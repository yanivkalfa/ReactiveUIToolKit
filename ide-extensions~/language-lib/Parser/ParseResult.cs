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
    public sealed record FunctionParam(string Type, string Name, string? DefaultValue)
    {
        /// <summary>1-based source line where the parameter name appears. 0 when not tracked.</summary>
        public int SourceLine { get; init; } = 0;

        /// <summary>0-based column of the first character of the parameter name. -1 when not tracked.</summary>
        public int NameColumn { get; init; } = -1;
    }

    // ── Hook declaration ─────────────────────────────────────────────────────

    /// <summary>
    /// A single <c>hook</c> declaration parsed from a .uitkx file:
    /// <c>hook useCounter(int initial = 0) -&gt; (int, Action) { ... }</c>.
    /// </summary>
    public sealed record HookDeclaration(
        /// <summary>Hook name (camelCase), e.g. <c>useCounter</c>.</summary>
        string Name,
        /// <summary>Generic type parameters including angle brackets, e.g. <c>&lt;T&gt;</c>, or <c>null</c> if non-generic.</summary>
        string? GenericParams,
        /// <summary>Typed parameters declared in the hook header.</summary>
        ImmutableArray<FunctionParam> Params,
        /// <summary>Return type text after <c>-&gt;</c>, e.g. <c>(int, Action)</c>, or <c>null</c> for void hooks.</summary>
        string? ReturnType,
        /// <summary>Raw C# body text between the braces.</summary>
        string Body,
        /// <summary>1-based line of the <c>hook</c> keyword.</summary>
        int DeclarationLine,
        /// <summary>1-based line of the first body statement.</summary>
        int BodyStartLine,
        /// <summary>Absolute char offset in source just after the opening <c>{</c>.</summary>
        int BodyStartOffset,
        /// <summary>Absolute char offset in source just before the closing <c>}</c>.</summary>
        int BodyEndOffset
    )
    {
        /// <summary>
        /// True when the declaration is prefixed with <c>export</c> (import/export grammar,
        /// leg 3). Non-exported hooks are file-private / strict-invisible cross-file.
        /// </summary>
        public bool IsExported { get; init; } = false;
    };

    // ── Module declaration ───────────────────────────────────────────────────

    /// <summary>
    /// A single <c>module</c> declaration parsed from a .uitkx file:
    /// <c>module Counter { ... }</c>.
    /// </summary>
    public sealed record ModuleDeclaration(
        /// <summary>Module name (PascalCase), e.g. <c>Counter</c>.</summary>
        string Name,
        /// <summary>Raw C# body text between the braces.</summary>
        string Body,
        /// <summary>1-based line of the <c>module</c> keyword.</summary>
        int DeclarationLine,
        /// <summary>1-based line of the first body statement.</summary>
        int BodyStartLine,
        /// <summary>Absolute char offset in source just after the opening <c>{</c>.</summary>
        int BodyStartOffset,
        /// <summary>Absolute char offset in source just before the closing <c>}</c>.</summary>
        int BodyEndOffset
    )
    {
        /// <summary>
        /// True when the declaration is prefixed with <c>export</c> (import/export grammar,
        /// leg 3). Non-exported modules are file-private / strict-invisible cross-file.
        /// </summary>
        public bool IsExported { get; init; } = false;
    };

    // ── Import declaration ───────────────────────────────────────────────────

    /// <summary>
    /// A single preamble <c>import { A, B } from "specifier"</c> line (import/export grammar,
    /// leg 3). Named imports only; static, string-literal specifier; preamble-only.
    /// </summary>
    public sealed record ImportDeclaration(
        /// <summary>The imported names, in source order (e.g. <c>["StatusChip"]</c>).</summary>
        ImmutableArray<string> Names,
        /// <summary>The raw specifier string (e.g. <c>"./StatusChip"</c>, <c>"~/Shared/Types"</c>). Extensionless — <c>.uitkx</c> implied.</summary>
        string Specifier,
        /// <summary>1-based line of the <c>import</c> keyword.</summary>
        int Line,
        /// <summary>0-based column of the <c>import</c> keyword.</summary>
        int Column,
        /// <summary>0-based column of each imported name, parallel to <see cref="Names"/>. For unused-import / not-exported squiggles.</summary>
        ImmutableArray<int> NameColumns
    );

    // ── Component declaration (per-decl, mixed-decl v1) ──────────────────────

    /// <summary>
    /// A single <c>component</c> declaration parsed from a .uitkx file (mixed-decl v1, leg 3):
    /// a file may declare multiple components/hooks/modules in any order. Carries every field
    /// that was historically singular on <see cref="DirectiveSet"/> so multi-component files
    /// emit one partial class per component.
    /// </summary>
    public sealed record ComponentDeclaration(
        /// <summary>Component (class) name.</summary>
        string Name,
        /// <summary>True when prefixed with <c>export</c> (→ <c>public</c>; else <c>internal</c>).</summary>
        bool IsExported,
        /// <summary><c>@props</c> — optional fully-qualified props type name.</summary>
        string? PropsTypeName,
        /// <summary><c>@key</c> — optional default VirtualNode key.</summary>
        string? DefaultKey,
        /// <summary>Typed parameters from the component header.</summary>
        ImmutableArray<FunctionParam> FunctionParams,
        /// <summary>Setup C# statements (all top-level statements except the <c>return (...)</c>).</summary>
        string? FunctionSetupCode,
        /// <summary>1-based line where setup code begins. -1 when unavailable.</summary>
        int FunctionSetupStartLine,
        /// <summary>Absolute char offset of the first char of trimmed setup code. -1 when not tracked.</summary>
        int FunctionSetupStartOffset,
        /// <summary>1-based line where this component's markup begins.</summary>
        int MarkupStartLine,
        /// <summary>Absolute char index where this component's markup begins.</summary>
        int MarkupStartIndex,
        /// <summary>Exclusive end index for this component's markup. -1 = to EOF.</summary>
        int MarkupEndIndex,
        /// <summary>1-based line of the <c>component Name {</c> declaration.</summary>
        int DeclarationLine,
        /// <summary>0-based column of the component NAME token.</summary>
        int NameColumn,
        /// <summary>1-based line of the <c>;</c> ending <c>return (...);</c>. -1 when not tracked.</summary>
        int ReturnEndLine,
        /// <summary>1-based line of the closing <c>}</c> of the component body. -1 when not tracked.</summary>
        int BodyEndLine
    )
    {
        /// <summary>Paren-wrapped JSX ranges in this component's setup code (start, end, line).</summary>
        public ImmutableArray<(int Start, int End, int Line)> SetupCodeMarkupRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;
        /// <summary>Bare (non-paren-wrapped) JSX ranges in this component's setup code.</summary>
        public ImmutableArray<(int Start, int End, int Line)> SetupCodeBareJsxRanges { get; init; }
            = ImmutableArray<(int Start, int End, int Line)>.Empty;
        /// <summary>Offset in setup code where the removed <c>return (…);</c> gap begins. -1 when none.</summary>
        public int FunctionSetupGapOffset { get; init; } = -1;
        /// <summary>Length of the removed <c>return (…);</c> statement.</summary>
        public int FunctionSetupGapLength { get; init; } = 0;
    };

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
        /// <summary>All <c>@uss "path"</c> stylesheet paths, in declaration order.</summary>
        ImmutableArray<string> UssFiles,
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
        int FunctionSetupGapLength = 0,
        /// <summary>
        /// Hook declarations parsed from this file. Empty/default for component or directive-style files.
        /// </summary>
        ImmutableArray<HookDeclaration> HookDeclarations = default,
        /// <summary>
        /// Module declarations parsed from this file. Empty/default for component or directive-style files.
        /// </summary>
        ImmutableArray<ModuleDeclaration> ModuleDeclarations = default
    )
    {
        /// <summary>
        /// Comments (<c>//</c>, <c>/* */</c>, <c>&lt;!-- --&gt;</c>) consumed while skipping
        /// leading trivia before the preamble (<c>@namespace</c>/<c>@using</c>/<c>@uss</c>/
        /// <c>component</c>), in source order, each with its raw text (including delimiters)
        /// and 1-based line. The formatter re-emits these verbatim so a license header or
        /// other leading comment is not silently dropped on format (see
        /// FINAL_AUDIT_UITKX_FINDINGS.md, finding U-01). Empty when not tracked (e.g. the
        /// lightweight <c>LooksLikeFunctionStyleComponent</c> lookahead probe).
        /// </summary>
        public ImmutableArray<(string Text, bool IsBlock, int Line)> LeadingTrivia { get; init; }
            = ImmutableArray<(string Text, bool IsBlock, int Line)>.Empty;

        /// <summary>
        /// Preamble <c>import { … } from "…"</c> declarations, in source order (import/export
        /// grammar, leg 3). Empty when the file has no imports. See <see cref="ImportDeclaration"/>.
        /// </summary>
        public ImmutableArray<ImportDeclaration> Imports { get; init; }
            = ImmutableArray<ImportDeclaration>.Empty;

        /// <summary>
        /// All <c>component</c> declarations in this file, in source order (mixed-decl v1, leg 3).
        /// Supersedes the singular <see cref="ComponentName"/>/setup/markup fields, which remain during
        /// the migration and are kept in sync for the first component. Empty for hook/module-only or
        /// directive-style files.
        /// </summary>
        public ImmutableArray<ComponentDeclaration> ComponentDeclarations { get; init; }
            = ImmutableArray<ComponentDeclaration>.Empty;
    };

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
