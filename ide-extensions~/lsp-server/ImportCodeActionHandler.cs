using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// <c>textDocument/codeAction</c> for the import/export grammar (leg 3): an "add import" quick-fix
/// for <c>UITKX2305</c>. The 2305 message already carries the exact <c>import {{ X }} from "./rel"</c>
/// line to add, so the fix parses it from the message and inserts it at the top of the preamble.
/// </summary>
public sealed class ImportCodeActionHandler : ICodeActionHandler
{
    private readonly DocumentStore _store;

    public ImportCodeActionHandler(DocumentStore store) => _store = store;

    public CodeActionRegistrationOptions GetRegistrationOptions(
        CodeActionCapability capability, ClientCapabilities clientCapabilities) =>
        new CodeActionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }),
            CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix),
        };

    public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
    {
        var actions = new List<CommandOrCodeAction>();
        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
            return Task.FromResult(new CommandOrCodeActionContainer(actions));

        foreach (var d in request.Context.Diagnostics)
        {
            // UITKX2305 — "add import { X } from …" quick-fix.
            if (TryBuildAddImportEdit(text, d.Message, out int line, out string insertText))
            {
                actions.Add(MakeEditAction(
                    insertText.Trim(),
                    request.TextDocument.Uri,
                    new TextEdit
                    {
                        Range = new LspRange(new Position(line, 0), new Position(line, 0)),
                        NewText = insertText,
                    },
                    d));
                continue;
            }

            // UITKX2317 — "remove redundant using" quick-fix (delete the whole line).
            if (DiagnosticIs(d, "UITKX2317"))
            {
                int redundantLine = d.Range.Start.Line;
                actions.Add(MakeEditAction(
                    "Remove redundant using",
                    request.TextDocument.Uri,
                    new TextEdit
                    {
                        Range = new LspRange(
                            new Position(redundantLine, 0), new Position(redundantLine + 1, 0)),
                        NewText = string.Empty,
                    },
                    d));
            }
        }

        // Cursor-position refactor (not diagnostic-gated): on a `@using X` / `using X;` line,
        // offer "Convert to import \"@X\"" — the migration nudge toward the unified spelling.
        int cursorLine = request.Range.Start.Line;
        if (TryBuildConvertUsingToImport(GetLine(text, cursorLine), out string converted))
        {
            actions.Add(MakeEditAction(
                $"Convert to import \"@…\"",
                request.TextDocument.Uri,
                new TextEdit
                {
                    Range = new LspRange(
                        new Position(cursorLine, 0), new Position(cursorLine, GetLine(text, cursorLine).Length)),
                    NewText = converted,
                },
                diagnostic: null,
                kind: CodeActionKind.RefactorRewrite));
        }

        return Task.FromResult(new CommandOrCodeActionContainer(actions));
    }

    private static bool DiagnosticIs(Diagnostic d, string code) =>
        d.Code.HasValue && d.Code.Value.IsString && d.Code.Value.String == code;

    private static CommandOrCodeAction MakeEditAction(
        string title, DocumentUri uri, TextEdit edit, Diagnostic? diagnostic,
        CodeActionKind kind = default)
    {
        var wsEdit = new WorkspaceEdit
        {
            Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
            {
                [uri] = new[] { edit },
            },
        };
        return new CommandOrCodeAction(new CodeAction
        {
            Title = title,
            Kind = kind == default ? CodeActionKind.QuickFix : kind,
            Diagnostics = diagnostic != null ? new Container<Diagnostic>(diagnostic) : null,
            Edit = wsEdit,
        });
    }

    /// <summary>The 0-based <paramref name="line"/> of <paramref name="text"/> (no terminator), or "".</summary>
    public static string GetLine(string text, int line)
    {
        if (line < 0) return string.Empty;
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        return line < lines.Length ? lines[line] : string.Empty;
    }

    /// <summary>
    /// Converts a single <c>@using X</c> / <c>using X;</c> line to <c>import "@X"</c>, preserving the
    /// full payload (namespace / <c>static T</c> / <c>Alias = T</c>) and leading indentation. Returns
    /// false when the line is not a plain using directive. Pure + side-effect-free for unit testing.
    /// </summary>
    public static bool TryBuildConvertUsingToImport(string lineText, out string replacement)
    {
        replacement = string.Empty;
        if (string.IsNullOrEmpty(lineText)) return false;

        int i = 0;
        while (i < lineText.Length && (lineText[i] == ' ' || lineText[i] == '\t')) i++;
        string indent = lineText.Substring(0, i);
        string rest = lineText.Substring(i);

        if (rest.StartsWith("@using ", System.StringComparison.Ordinal))
            rest = rest.Substring("@using ".Length);
        else if (rest.StartsWith("using ", System.StringComparison.Ordinal))
            rest = rest.Substring("using ".Length);
        else
            return false;

        string payload = rest.TrimEnd().TrimEnd(';').Trim();
        if (payload.Length == 0) return false;

        replacement = $"{indent}import \"@{payload}\"";
        return true;
    }

    /// <summary>
    /// Extract the <c>import {{ … }} from "…"</c> line named by a 2305 message and compute where to
    /// insert it (0-based line at the top of the preamble, after leading comment/blank lines). Pure +
    /// side-effect-free so it is directly unit-testable. Returns <c>false</c> when the message carries
    /// no import line or that import already exists in the file.
    /// </summary>
    public static bool TryBuildAddImportEdit(string text, string diagMessage, out int insertLine, out string insertText)
    {
        insertLine = 0;
        insertText = string.Empty;

        var m = Regex.Match(diagMessage, "import \\{ [^}]* \\} from \"[^\"]*\"");
        if (!m.Success)
            return false;
        if (text.Contains(m.Value))
            return false; // already imported — nothing to add

        string nl = text.Contains("\r\n") ? "\r\n" : "\n";
        insertText = m.Value + nl;
        insertLine = FirstNonTriviaLine(text);
        return true;
    }

    /// <summary>0-based first line that isn't blank or a comment (imports go at the top of the preamble).</summary>
    public static int FirstNonTriviaLine(string text)
    {
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        bool inBlock = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string t = lines[i].Trim();
            if (inBlock)
            {
                int end = t.IndexOf("*/", System.StringComparison.Ordinal);
                if (end < 0) continue;
                t = t.Substring(end + 2).Trim();
                inBlock = false;
                if (t.Length == 0) continue;
            }
            if (t.Length == 0) continue;
            if (t.StartsWith("//", System.StringComparison.Ordinal)) continue;
            if (t.StartsWith("/*", System.StringComparison.Ordinal))
            {
                if (t.IndexOf("*/", System.StringComparison.Ordinal) < 0) inBlock = true;
                continue;
            }
            return i;
        }
        return lines.Length > 0 ? 0 : 0;
    }
}
