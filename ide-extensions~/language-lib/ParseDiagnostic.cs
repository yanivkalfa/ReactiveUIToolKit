namespace ReactiveUITK.Language
{
    /// <summary>
    /// Severity levels for a <see cref="ParseDiagnostic"/>.
    /// Intentionally named ParseSeverity (not DiagnosticSeverity) to avoid
    /// naming collisions with Microsoft.CodeAnalysis.DiagnosticSeverity in
    /// projects that reference both libraries.
    /// </summary>
    public enum ParseSeverity
    {
        Error,
        Warning,
        Information,
        Hint,
    }

    /// <summary>
    /// A Roslyn-free diagnostic produced by the UITKX parser or analyzer.
    /// All positions use the same coordinate system as AstNode:
    /// SourceLine = 1-based, SourceColumn / EndColumn = 0-based.
    /// </summary>
    public sealed class ParseDiagnostic
    {
        /// <summary>Diagnostic code, e.g. "UITKX0300".</summary>
        public string Code { get; init; } = "";

        /// <summary>Human-readable message (already formatted, not a message template).</summary>
        public string Message { get; init; } = "";

        /// <summary>Severity of this diagnostic.</summary>
        public ParseSeverity Severity { get; init; }

        /// <summary>1-based source line where the diagnostic originates.</summary>
        public int SourceLine { get; init; }

        /// <summary>0-based column of the opening token. 0 until column tracking is wired.</summary>
        public int SourceColumn { get; init; }

        /// <summary>1-based end line (inclusive). 0 = treat as SourceLine.</summary>
        public int EndLine { get; init; }

        /// <summary>0-based exclusive end column. 0 until column tracking is wired.</summary>
        public int EndColumn { get; init; }
    }
}
