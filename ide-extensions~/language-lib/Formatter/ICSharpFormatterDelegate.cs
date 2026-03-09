namespace ReactiveUITK.Language.Formatter
{
    /// <summary>
    /// Pluggable delegate that formats a raw C# code string.
    ///
    /// <para>Implemented by <c>RoslynCSharpFormatter</c> in the LSP server, which
    /// uses Roslyn's <c>Formatter.FormatAsync</c> to produce idiomatic C# output.
    /// A null reference (the default when no implementation is registered) means
    /// the <see cref="AstFormatter"/> falls back to its own indentation-only
    /// C# formatting.</para>
    ///
    /// <para><b>language-lib boundary:</b> This interface lives in
    /// <c>language-lib</c> so that <see cref="AstFormatter"/> can reference it
    /// without taking a dependency on Roslyn NuGet packages.</para>
    /// </summary>
    public interface ICSharpFormatterDelegate
    {
        /// <summary>
        /// Formats a C# code fragment and returns the result.
        /// </summary>
        /// <param name="code">
        /// Raw C# code — the verbatim contents of an <c>@code { … }</c> block,
        /// not including the <c>@code {</c> / <c>}</c> delimiters.
        /// May contain leading/trailing whitespace.
        /// </param>
        /// <returns>
        /// The formatted code, or <c>null</c> / empty string to signal that the
        /// formatter could not process the input (the caller should fall back to
        /// its own formatting logic in that case).
        /// </returns>
        string? Format(string code);
    }
}
