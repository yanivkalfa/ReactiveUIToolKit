namespace ReactiveUITK.Language.SemanticTokens
{
    /// <summary>Semantic token type identifiers used by the UITKX language server.</summary>
    public static class SemanticTokenTypes
    {
        // ── Custom UITKX types ────────────────────────────────────────────────

        /// <summary>Element open/close tag names, e.g. <c>Box</c> in <c>&lt;Box&gt;</c>.</summary>
        public const string Element = "uitkxElement";

        /// <summary>Control-flow keywords: <c>@if</c>, <c>@foreach</c>, <c>@switch</c>, etc.</summary>
        public const string Directive = "uitkxDirective";

        /// <summary>Attribute names on elements, e.g. <c>text</c> in <c>text="Hi"</c>.</summary>
        public const string Attribute = "uitkxAttribute";

        /// <summary>Inline expression delimiters <c>@(</c> … <c>)</c>.</summary>
        public const string Expression = "uitkxExpression";

        /// <summary>
        /// Value text following a top-level directive, e.g. <c>MyGame.UI</c>
        /// after <c>@namespace</c>.
        /// </summary>
        public const string DirectiveName = "uitkxDirectiveName";

        // ── Standard LSP types (reused) ───────────────────────────────────────

        public const string Variable = "variable";
        public const string Keyword  = "keyword";

        /// <summary>Standard LSP function type — used for hook setter variables.</summary>
        public const string Function = "function";

        /// <summary>Standard LSP comment type — used for {/* */} JSX comments.</summary>
        public const string Comment  = "comment";

        /// <summary>Standard LSP type — PascalCase class/type names inside @code.</summary>
        public const string Type     = "type";

        /// <summary>Standard LSP string type — string literals inside @code.</summary>
        public const string String   = "string";

        /// <summary>Standard LSP number type — numeric literals inside @code.</summary>
        public const string Number   = "number";

        // ── Legend array (position = token-type index) ────────────────────────

        /// <summary>
        /// All token types in registration order.
        /// The index of each entry must match the integer identity used in the
        /// LSP <c>SemanticTokens.data</c> encoding.
        /// </summary>
        public static readonly string[] All = new[]
        {
            Element, Directive, Attribute, Expression, DirectiveName, Variable, Keyword, Function, Comment, Type, String, Number,
        };
    }

    /// <summary>Semantic token modifier identifiers used by the UITKX language server.</summary>
    public static class SemanticTokenModifiers
    {
        /// <summary>
        /// Applied to an element whose Props class was found in the workspace index.
        /// Reserved for future use (known-component highlighting).
        /// </summary>
        public const string Declaration = "declaration";

        /// <summary>
        /// All modifiers in registration order.
        /// Bit position <c>i</c> corresponds to modifier <c>All[i]</c> in the bitmask.
        /// </summary>
        public static readonly string[] All = new[]
        {
            Declaration,
        };
    }
}
