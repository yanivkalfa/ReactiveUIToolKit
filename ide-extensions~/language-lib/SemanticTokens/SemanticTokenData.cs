namespace ReactiveUITK.Language.SemanticTokens
{
    /// <summary>
    /// A single semantic token ready for LSP integer-delta encoding.
    /// All positions follow the LSP convention: 0-based line and character offset.
    /// </summary>
    public readonly struct SemanticTokenData
    {
        /// <summary>0-based line number.</summary>
        public int Line { get; init; }

        /// <summary>0-based character offset on the line.</summary>
        public int Column { get; init; }

        /// <summary>Character length of the token.</summary>
        public int Length { get; init; }

        /// <summary>
        /// Token type string (one of the constants in <see cref="SemanticTokenTypes"/>).
        /// </summary>
        public string TokenType { get; init; }

        /// <summary>
        /// Active modifiers (constants from <see cref="SemanticTokenModifiers"/>).
        /// May be an empty array – never null.
        /// </summary>
        public string[] Modifiers { get; init; }
    }
}
