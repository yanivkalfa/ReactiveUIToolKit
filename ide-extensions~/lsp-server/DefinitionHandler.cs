using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// Handles <c>textDocument/definition</c> for <c>.uitkx</c> files.
///
/// Supported navigation targets:
/// <list type="bullet">
///   <item>Element tag name → jump to the corresponding <c>*Props.cs</c> class declaration.</item>
///   <item>Attribute name → jump to the property declaration inside <c>*Props.cs</c>.</item>
/// </list>
/// </summary>
public sealed class DefinitionHandler : IDefinitionHandler
{
    private readonly DocumentStore  _store;
    private readonly WorkspaceIndex _index;

    public DefinitionHandler(DocumentStore store, WorkspaceIndex index)
    {
        _store = store;
        _index = index;
    }

    public DefinitionRegistrationOptions GetRegistrationOptions(
        DefinitionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new DefinitionRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }),
        };

    public Task<LocationOrLocationLinks?> Handle(
        DefinitionParams request,
        CancellationToken cancellationToken)
    {
        ServerLog.Log(
            $"definition request: {request.TextDocument.Uri}  "
            + $"pos={request.Position.Line}:{request.Position.Character}");

        // Extract local path once; reused for disk-read fallback and AST parse.
        string localPath;
        try { localPath = new Uri(request.TextDocument.Uri.ToString()).LocalPath; }
        catch { localPath = string.Empty; }

        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
        {
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                text = File.ReadAllText(localPath);
                _store.Set(request.TextDocument.Uri, text);
            }
            else
            {
                return Task.FromResult<LocationOrLocationLinks?>(null);
            }
        }

        // Parse the document with the language-lib AST pipeline so go-to-definition
        // targets are derived from the real syntax tree instead of text scanning.

        var parseDiags  = new List<ReactiveUITK.Language.ParseDiagnostic>();
        var directives  = DirectiveParser.Parse(text, localPath, parseDiags);
        var nodes       = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        int line1 = (int)request.Position.Line + 1;
        int col0  = (int)request.Position.Character;
        var ctx = AstCursorContext.Find(parseResult, text, line1, col0);

        string word       = ctx.Word;
        string? tagContext = ctx.Kind == CursorKind.AttributeName ? ctx.TagName : null;

        if (string.IsNullOrEmpty(word))
            return Task.FromResult<LocationOrLocationLinks?>(null);

        // ── Case 1: cursor is on an element tag name ─────────────────────────
        var elementInfo = _index.TryGetElementInfo(word);
        if (elementInfo is not null && File.Exists(elementInfo.FilePath))
        {
            ServerLog.Log($"definition: '{word}' → {elementInfo.FilePath}:{elementInfo.FileLine}");
            return Task.FromResult<LocationOrLocationLinks?>(MakeLocation(elementInfo.FilePath, elementInfo.FileLine));
        }

        // ── Case 2: cursor is on an attribute name inside a known element tag ─
        if (tagContext is not null)
        {
            var parentInfo = _index.TryGetElementInfo(tagContext)
                          ?? _index.TryGetElementInfo(
                                 char.ToUpperInvariant(tagContext[0]) + tagContext[1..]);

            if (parentInfo is not null && File.Exists(parentInfo.FilePath))
            {
                var prop = parentInfo.Props.Find(p =>
                    p.Name.Equals(word, StringComparison.OrdinalIgnoreCase));

                if (prop is not null)
                {
                    ServerLog.Log(
                        $"definition: prop '{word}' on '{tagContext}' → "
                        + $"{parentInfo.FilePath}:{prop.Line}");
                    return Task.FromResult<LocationOrLocationLinks?>(MakeLocation(parentInfo.FilePath, prop.Line));
                }

                // Jump to the class declaration even if the exact prop wasn't parsed.
                ServerLog.Log(
                    $"definition: attr '{word}' on '{tagContext}' → class at "
                    + $"{parentInfo.FilePath}:{parentInfo.FileLine}");
                return Task.FromResult<LocationOrLocationLinks?>(MakeLocation(parentInfo.FilePath, parentInfo.FileLine));
            }
        }

        // ―― Case 3: variable declared in a @code block ――――――――――――――――――――――――――
        // Handles @(identifier) — scan the document for a @code-block variable
        // declaration matching the word (e.g. "var component = ..." or "Type word =").
        var (codeVarLine, codeVarCol) = FindCodeVarDeclaration(text, word);
        if (codeVarLine > 0)
        {
            ServerLog.Log($"definition: '{word}' \u2192 @code var declaration at line {codeVarLine} col {codeVarCol}");
            return Task.FromResult<LocationOrLocationLinks?>(MakeLocation(localPath, codeVarLine, codeVarCol));
        }

        ServerLog.Log($"definition: no target found for '{word}'");
        return Task.FromResult<LocationOrLocationLinks?>(null);
    }

    /// <summary>
    /// Scans the document source for a line that declares a variable named
    /// <paramref name="varName"/> inside a <c>@code { }</c> block.
    /// Matches patterns like <c>var word = ...</c>, <c>var (word, ...) = ...</c>,
    /// and <c>TypeName word = ...</c>.
    /// Returns (1-based line, 0-based character), or (0, 0) if not found.
    /// </summary>
    private static (int Line, int Col) FindCodeVarDeclaration(string text, string varName)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(varName))
            return (0, 0);

        bool insideCode = false;
        int lineNum = 0;

        foreach (var rawLine in text.Split('\n'))
        {
            lineNum++;
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.Trim();

            if (!insideCode)
            {
                if (trimmed.StartsWith("@code", StringComparison.Ordinal)) insideCode = true;
                continue;
            }

            if (trimmed == "}") { insideCode = false; continue; }

            int col = -1;
            var escaped = System.Text.RegularExpressions.Regex.Escape(varName);

            // var word = …  or  var word;
            var m = System.Text.RegularExpressions.Regex.Match(
                line, $@"\bvar\s+({escaped})\b");
            if (m.Success) col = m.Groups[1].Index;

            // var (word, …) = …  tuple deconstruction
            if (col < 0 && trimmed.StartsWith("var (", StringComparison.Ordinal))
            {
                m = System.Text.RegularExpressions.Regex.Match(line, $@"\b({escaped})\b");
                if (m.Success) col = m.Groups[1].Index;
            }

            // TypeName word = …
            if (col < 0)
            {
                m = System.Text.RegularExpressions.Regex.Match(
                    line, $@"\b\w+\s+({escaped})\s*[=;]");
                if (m.Success) col = m.Groups[1].Index;
            }

            if (col >= 0) return (lineNum, col);
        }
        return (0, 0);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static LocationOrLocationLinks MakeLocation(string filePath, int oneBasedLine, int zeroBasedCol = 0)
    {
        var targetLine = Math.Max(0, oneBasedLine - 1);
        var targetCol  = Math.Max(0, zeroBasedCol);
        var location   = new Location
        {
            Uri   = DocumentUri.FromFileSystemPath(filePath),
            Range = new LspRange(
                new Position(targetLine, targetCol),
                new Position(targetLine, targetCol)),
        };
        return new LocationOrLocationLinks(new LocationOrLocationLink(location));
    }

    private static int ToOffset(string text, Position position)
    {
        var line   = (int)position.Line;
        var column = (int)position.Character;
        var offset = 0;
        for (var i = 0; i < line && offset < text.Length; i++)
        {
            var nl = text.IndexOf('\n', offset);
            offset = nl < 0 ? text.Length : nl + 1;
        }
        return Math.Min(offset + column, text.Length);
    }
}
