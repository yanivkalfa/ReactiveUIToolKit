namespace ReactiveUITK.Language.SemanticTokens
{
    /// <summary>
    /// Describes a single classification override for a token whose type should
    /// be changed from the lexer's default (e.g. <c>identifier</c> → <c>method</c>).
    /// Sent from the LSP server to VS2022 via the custom
    /// <c>uitkx/classificationOverrides</c> notification.
    /// </summary>
    public readonly struct ClassificationOverride
    {
        /// <summary>0-based line number in the .uitkx file.</summary>
        public int Line { get; init; }

        /// <summary>0-based character offset on the line.</summary>
        public int Column { get; init; }

        /// <summary>Character length of the token.</summary>
        public int Length { get; init; }

        /// <summary>
        /// The classification type to apply instead of the lexer's default.
        /// Uses VS classification names: <c>"method"</c>, <c>"keyword"</c>, etc.
        /// </summary>
        public string OverrideType { get; init; }
    }
}
