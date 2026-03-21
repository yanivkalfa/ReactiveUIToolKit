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
        public string? Format(string code, int indentSize = 4)
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

                // Format using the caller's indent size (default Roslyn uses 4).
                var fmtOptions = _workspace.Options
                    .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, indentSize)
                    .WithChangedOption(FormattingOptions.UseTabs,          LanguageNames.CSharp, false);
                var formattedRoot = Formatter.Format(root, _workspace, fmtOptions);
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

        // ── ICSharpFormatterDelegate.FormatStatements ─────────────────────────

        /// <inheritdoc/>
        public string? FormatStatements(string code, int indentSize = 4)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            try
            {
                // Wrap in a class + method body so Roslyn can parse statement-level
                // code (local variable declarations, calls, control flow, etc.).
                const string prefix = "class __UitkxFmt__ {\nvoid __render__() {\n";
                const string suffix = "\n}\n}";
                string wrapped = prefix + code + suffix;

                var tree = CSharpSyntaxTree.ParseText(wrapped, s_parseOptions);

                // Bail out on syntax errors so we don't produce garbled output.
                bool hasSyntaxError = false;
                foreach (var diag in tree.GetDiagnostics())
                {
                    if (diag.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                    {
                        hasSyntaxError = true;
                        break;
                    }
                }
                if (hasSyntaxError)
                    return null;

                var root      = tree.GetRoot();
                // Format using the caller's indent size.
                var fmtOptions = _workspace.Options
                    .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, indentSize)
                    .WithChangedOption(FormattingOptions.UseTabs,          LanguageNames.CSharp, false);
                var formattedRoot = Formatter.Format(root, _workspace, fmtOptions);
                string formatted  = formattedRoot.ToFullString();

                // Extract the method body: find "void __render__()" then its opening '{'.
                const string methodSig = "void __render__()";
                int methodIdx = formatted.IndexOf(methodSig, StringComparison.Ordinal);
                if (methodIdx < 0) return null;

                int braceOpen = formatted.IndexOf('{', methodIdx + methodSig.Length);
                if (braceOpen < 0) return null;

                // Find the matching closing brace.
                int depth = 1, pos = braceOpen + 1;
                while (pos < formatted.Length && depth > 0)
                {
                    if      (formatted[pos] == '{') depth++;
                    else if (formatted[pos] == '}') { depth--; if (depth == 0) break; }
                    pos++;
                }

                string body = formatted
                    .Substring(braceOpen + 1, pos - braceOpen - 1)
                    .Replace("\r\n", "\n")
                    .TrimStart('\n');

                // De-indent: strip the consistent leading whitespace baseline so the
                // AstFormatter can re-add the correct indent level for the host scope.
                var lines = body.Split('\n');
                int baseIndent = int.MaxValue;
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int lead = 0;
                    while (lead < line.Length && line[lead] == ' ') lead++;
                    if (lead < baseIndent) baseIndent = lead;
                }
                if (baseIndent == int.MaxValue) baseIndent = 0;

                var sb = new System.Text.StringBuilder();
                for (int li = 0; li < lines.Length; li++)
                {
                    string line = lines[li];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        sb.Append('\n');
                        continue;
                    }
                    string stripped = baseIndent > 0 && line.Length > baseIndent
                        ? line.Substring(baseIndent)
                        : line.TrimStart(' ');
                    sb.Append(stripped);
                    if (li < lines.Length - 1) sb.Append('\n');
                }

                return sb.ToString().TrimEnd('\r', '\n', ' ', '\t');
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynCSharpFormatter] FormatStatements error: {ex.Message}");
                return null;
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose() => _workspace.Dispose();
    }
}
