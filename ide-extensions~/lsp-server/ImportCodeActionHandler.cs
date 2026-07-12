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
            if (!TryBuildAddImportEdit(text, d.Message, out int line, out string insertText))
                continue;

            var edit = new TextEdit
            {
                Range = new LspRange(new Position(line, 0), new Position(line, 0)),
                NewText = insertText,
            };
            var wsEdit = new WorkspaceEdit
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    [request.TextDocument.Uri] = new[] { edit },
                },
            };
            actions.Add(new CommandOrCodeAction(new CodeAction
            {
                Title = insertText.Trim(),
                Kind = CodeActionKind.QuickFix,
                Diagnostics = new Container<Diagnostic>(d),
                Edit = wsEdit,
            }));
        }

        return Task.FromResult(new CommandOrCodeActionContainer(actions));
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
