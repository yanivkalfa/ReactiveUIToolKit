namespace ReactiveUITK.Language.Diagnostics
{
    /// <summary>
    /// Diagnostic code constants used by the UITKX language library.
    ///
    /// ID ranges:
    ///   UITKX0101–0108   T2 — Structural (directive + schema checks); language-lib
    ///   UITKX0300–0305   T1 — Parser syntax errors; emitted by UitkxParser /
    ///                         DirectiveParser into ParseResult.Diagnostics
    /// </summary>
    public static class DiagnosticCodes
    {
        // ── T2 — Structural diagnostics (this analyzer) ──────────────────────

        /// <summary>File contains no <c>@namespace</c> directive.</summary>
        public const string MissingNamespace = "UITKX0101";

        /// <summary>File contains no <c>@component</c> directive.</summary>
        public const string MissingComponent = "UITKX0102";

        /// <summary><c>@component Foo</c> but the file is named <c>Bar.uitkx</c>.</summary>
        public const string FilenameMismatch = "UITKX0103";

        /// <summary>Two sibling elements share the same literal <c>key="…"</c> value.</summary>
        public const string DuplicateKey = "UITKX0104";

        /// <summary>
        /// PascalCase element name is not in the workspace element index.
        /// Only reported when the index is available.
        /// </summary>
        public const string UnknownElement = "UITKX0105";

        /// <summary>
        /// An element inside a <c>@foreach</c> body has no <c>key</c> attribute.
        /// Severity: Warning.
        /// </summary>
        public const string MissingKey = "UITKX0106";

        /// <summary>
        /// Statement appears after an unconditional top-level <c>return</c> in an
        /// <c>@code</c> block and is unreachable.
        /// Severity: Hint (with Unnecessary tag in LSP layer).
        /// </summary>
        public const string UnreachableAfterReturn = "UITKX0107";

        /// <summary>
        /// The component's markup has more than one root render node.
        /// A component must return a single root element; all siblings must be
        /// wrapped in a container element.
        /// Severity: Error.
        /// </summary>
        public const string MultipleRenderRoots = "UITKX0108";

        /// <summary>
        /// An attribute on an element is not part of that element's known prop set.
        /// Only reported when the element's attribute list is available.
        /// Severity: Warning.
        /// </summary>
        public const string UnknownAttribute = "UITKX0109";

        // ── T1 — Parser codes (emitted by UitkxParser / DirectiveParser) ─────
        // Listed here for cross-reference only; not produced by DiagnosticsAnalyzer.

        /// <summary>Unexpected token while parsing. Emitted by UitkxParser.</summary>
        public const string UnexpectedToken = "UITKX0300";

        /// <summary>Unclosed element tag. Emitted by UitkxParser.</summary>
        public const string UnclosedTag = "UITKX0301";

        /// <summary>Mismatched closing tag. Emitted by UitkxParser.</summary>
        public const string MismatchedTag = "UITKX0302";

        /// <summary>Unknown <c>@directive</c> keyword. Emitted by UitkxParser.</summary>
        public const string UnknownDirective = "UITKX0305";
    }
}
