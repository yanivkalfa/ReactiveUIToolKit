using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using ReactiveUITK.Language.Formatter;
using CSharpParseOptions = Microsoft.CodeAnalysis.CSharp.CSharpParseOptions;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Implements <see cref="ICSharpFormatterDelegate"/> using Roslyn's
    /// <see cref="Formatter.Format"/> to produce idiomatic C# output for the
    /// contents of <c>@code { … }</c> blocks in .uitkx files.
    ///
    /// <para>The formatter wraps the raw code fragment in a temporary class
    /// scaffold so Roslyn can parse it as a valid compilation unit, applies the
    /// standard C# formatter, then strips the scaffold back out to return only
    /// the reformatted code body.</para>
    ///
    /// <para>Formatting is synchronous and typically takes &lt; 5 ms for an
    /// average <c>@code</c> block.  The underlying <see cref="AdhocWorkspace"/>
    /// is created once and reused to avoid repeated allocation.</para>
    /// </summary>
    public sealed class RoslynCSharpFormatter : ICSharpFormatterDelegate, IDisposable
    {
        // Workspace is needed only to supply default options to Formatter.Format.
        // A single shared instance is safe because formatting is read-only.
        private readonly AdhocWorkspace _workspace = new AdhocWorkspace();

        private static readonly CSharpParseOptions s_parseOptions =
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

        // ── ICSharpFormatterDelegate ──────────────────────────────────────────

        /// <inheritdoc/>
        public string? Format(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            try
            {
                // Wrap in a class so the parser accepts arbitrary member and
                // statement-level code.  Note: member-level code (fields, methods)
                // is the standard content of @code blocks so this always parses.
                const string prefix = "class __UitkxFmt__ {\n";
                const string suffix = "\n}";
                string wrapped = prefix + code + suffix;

                var tree = CSharpSyntaxTree.ParseText(wrapped, s_parseOptions);
                var root  = tree.GetRoot();

                // Format the full tree using Roslyn's default options.
                var formattedRoot = Formatter.Format(root, _workspace);
                string formattedWrapped = formattedRoot.ToFullString();

                // Strip the scaffold wrapper: everything between { on line 1 and
                // the last closing }.
                int contentStart = formattedWrapped.IndexOf('\n');
                int contentEnd   = formattedWrapped.LastIndexOf('}');

                if (contentStart < 0 || contentEnd <= contentStart)
                    return null;

                string body = formattedWrapped.Substring(contentStart + 1, contentEnd - contentStart - 1);

                // Normalise trailing whitespace, preserve at most one blank line
                // at the end (AstFormatter adds its own newline).
                return body.TrimEnd('\r', '\n', ' ', '\t');
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynCSharpFormatter] Format error: {ex.Message}");
                return null;
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose() => _workspace.Dispose();
    }
}
