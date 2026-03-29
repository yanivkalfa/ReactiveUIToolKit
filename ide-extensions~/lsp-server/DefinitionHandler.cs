using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using UitkxLanguageServer.Roslyn;
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
    private readonly RoslynHost     _roslynHost;

    public DefinitionHandler(DocumentStore store, WorkspaceIndex index, RoslynHost roslynHost)
    {
        _store = store;
        _index = index;
        _roslynHost = roslynHost;
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

        var result = HandleCore(request, text, localPath, cancellationToken);
        return Task.FromResult(result);
    }

    private LocationOrLocationLinks? HandleCore(
        DefinitionParams request, string text, string localPath, CancellationToken ct)
    {

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
            return null;

        // ── Case 1: cursor is on an element tag name ─────────────────────────
        var elementInfo = _index.TryGetElementInfo(word);
        if (elementInfo is not null && File.Exists(elementInfo.FilePath))
        {
            // When the definition is on the same line in the same file (i.e. the
            // user clicked the component's own declaration), return the exact
            // cursor position.  VS Code detects "definition == current location"
            // and automatically shows Find All References instead — matching the
            // JSX/TSX behaviour.
            bool isSameLocation =
                string.Equals(
                    Path.GetFullPath(elementInfo.FilePath),
                    Path.GetFullPath(localPath),
                    StringComparison.OrdinalIgnoreCase)
                && elementInfo.FileLine == line1;

            if (isSameLocation)
            {
                ServerLog.Log(
                    $"definition: '{word}' is own declaration – returning cursor pos for references fallback");
                return MakeLocation(localPath, line1, col0);
            }

            ServerLog.Log($"definition: '{word}' → {elementInfo.FilePath}:{elementInfo.FileLine}");
            return MakeLocation(elementInfo.FilePath, elementInfo.FileLine);
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
                    return MakeLocation(parentInfo.FilePath, prop.Line);
                }

                // Jump to the class declaration even if the exact prop wasn't parsed.
                ServerLog.Log(
                    $"definition: attr '{word}' on '{tagContext}' → class at "
                    + $"{parentInfo.FilePath}:{parentInfo.FileLine}");
                return MakeLocation(parentInfo.FilePath, parentInfo.FileLine);
            }
        }

        // ―― Case 3: variable declared in a @code block ――――――――――――――――――――――――――
        // Handles @(identifier) — scan the document for a @code-block variable
        // declaration matching the word (e.g. "var component = ..." or "Type word =").
        var (codeVarLine, codeVarCol) = FindCodeVarDeclaration(text, word);
        if (codeVarLine > 0)
        {
            ServerLog.Log($"definition: '{word}' \u2192 @code var declaration at line {codeVarLine} col {codeVarCol}");
            return MakeLocation(localPath, codeVarLine, codeVarCol);
        }

        if (directives.IsFunctionStyle)
        {
            var setupStart = directives.FunctionSetupStartLine > 0
                ? directives.FunctionSetupStartLine
                : 1;
            var setupEnd = Math.Max(setupStart, directives.MarkupStartLine - 1);

            var (setupVarLine, setupVarCol) = FindVarDeclarationInLineRange(
                text,
                word,
                setupStart,
                setupEnd
            );
            if (setupVarLine > 0)
            {
                ServerLog.Log($"definition: '{word}' \u2192 function-style setup var at line {setupVarLine} col {setupVarCol}");
                return MakeLocation(localPath, setupVarLine, setupVarCol);
            }
        }

        // ── Case 5: Roslyn symbol resolution ─────────────────────────────────
        // Covers symbols defined in companion .cs files and declarations that
        // the text-regex heuristic above cannot find.
        var roslynResult = TryResolveViaRoslyn(localPath, text, parseResult, request.Position, word, ct);
        if (roslynResult != null)
            return roslynResult;

        ServerLog.Log($"definition: no target found for '{word}'");
        return null;
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

    private static (int Line, int Col) FindVarDeclarationInLineRange(
        string text,
        string varName,
        int startLine1,
        int endLine1
    )
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(varName))
            return (0, 0);

        var lines = text.Split('\n');
        int start = Math.Max(1, startLine1);
        int end = Math.Min(lines.Length, Math.Max(start, endLine1));

        for (int lineNum = start; lineNum <= end; lineNum++)
        {
            var line = lines[lineNum - 1].TrimEnd('\r');
            int col = -1;
            var escaped = System.Text.RegularExpressions.Regex.Escape(varName);

            var m = System.Text.RegularExpressions.Regex.Match(line, $@"\bvar\s+({escaped})\b");
            if (m.Success) col = m.Groups[1].Index;

            if (col < 0 && line.Contains("var (", StringComparison.Ordinal))
            {
                m = System.Text.RegularExpressions.Regex.Match(line, $@"\b({escaped})\b");
                if (m.Success) col = m.Groups[1].Index;
            }

            if (col < 0)
            {
                m = System.Text.RegularExpressions.Regex.Match(
                    line,
                    $@"\b\w+\s+({escaped})\s*[=;]"
                );
                if (m.Success) col = m.Groups[1].Index;
            }

            if (col >= 0)
                return (lineNum, col);
        }

        return (0, 0);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static LocationOrLocationLinks MakeLocation(string filePath, int oneBasedLine, int zeroBasedCol = 0)
    {
        var targetLine = Math.Max(0, oneBasedLine - 1);
        var targetCol  = Math.Max(0, zeroBasedCol);
        var location   = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Location
        {
            Uri   = DocumentUri.FromFileSystemPath(filePath),
            Range = new LspRange(
                new Position(targetLine, targetCol),
                new Position(targetLine, targetCol)),
        };
        return new LocationOrLocationLinks(new LocationOrLocationLink(location));
    }

    private static int ToOffset(string text, Position position)
        => LspHelpers.ToOffset(text, position);

    // ── Roslyn-based symbol resolution ──────────────────────────────────────

    /// <summary>
    /// Maps the cursor position through the SourceMap into the Roslyn virtual
    /// document, resolves the symbol, and returns the definition location.
    /// Handles both same-file definitions (mapped back via SourceMap) and
    /// companion .cs file definitions.
    /// </summary>
    private LocationOrLocationLinks? TryResolveViaRoslyn(
        string localPath,
        string text,
        ParseResult parseResult,
        Position position,
        string word,
        CancellationToken ct)
    {
        var vdoc = _roslynHost.GetVirtualDocument(localPath);
        if (vdoc == null)
            return null;

        var offset = ToOffset(text, position);
        var virtualResult = vdoc.Map.ToVirtualOffset(offset);
        if (!virtualResult.HasValue)
            return null;

        var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
        if (roslynDoc == null)
            return null;

        ISymbol? symbol;
        try
        {
#pragma warning disable VSTHRD002
            var syntaxRoot = roslynDoc.GetSyntaxRootAsync(ct).GetAwaiter().GetResult();
            var semanticModel = roslynDoc.GetSemanticModelAsync(ct).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (syntaxRoot == null || semanticModel == null)
                return null;

            int pos = virtualResult.Value.VirtualOffset;
            var token = syntaxRoot.FindToken(pos > 0 ? pos - 1 : 0);
            if (token.Parent == null)
                return null;

            // Try declared symbol first (cursor on a declaration)
            symbol = semanticModel.GetDeclaredSymbol(token.Parent, ct);
            if (symbol == null)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(token.Parent, ct);
                symbol = symbolInfo.Symbol
                    ?? (symbolInfo.CandidateSymbols.Length > 0 ? symbolInfo.CandidateSymbols[0] : null);
            }
        }
        catch (Exception ex)
        {
            ServerLog.Log($"definition: Roslyn resolve error: {ex.Message}");
            return null;
        }

        if (symbol == null)
            return null;

        // Find the definition location from the symbol's declaring syntax references.
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            var defDoc = roslynDoc.Project.Solution.GetDocument(syntaxRef.SyntaxTree);
            if (defDoc == null)
                continue;

            var span = syntaxRef.Span;

            if (defDoc.Id == roslynDoc.Id)
            {
                // Definition is in the main virtual document → map back to .uitkx
                var uitkxResult = vdoc.Map.ToUitkxOffset(span.Start);
                if (uitkxResult.HasValue)
                {
                    ServerLog.Log($"definition: Roslyn '{word}' → {localPath} (via SourceMap)");
                    return MakeLocationFromOffset(localPath, text, uitkxResult.Value.UitkxOffset);
                }
            }
            else
            {
                // Definition is in a companion .cs document
                var dir = Path.GetDirectoryName(localPath);
                if (dir == null) continue;

                var companionPath = Path.Combine(dir, defDoc.Name);
                if (!File.Exists(companionPath)) continue;

#pragma warning disable VSTHRD002
                var defText = defDoc.GetTextAsync(ct).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                var linePos = defText.Lines.GetLinePosition(span.Start);

                ServerLog.Log(
                    $"definition: Roslyn '{word}' → {companionPath}:{linePos.Line + 1}:{linePos.Character}");
                return MakeLocation(companionPath, linePos.Line + 1, linePos.Character);
            }
        }

        ServerLog.Log($"definition: Roslyn found '{symbol.Kind}:{symbol.Name}' but no source location");
        return null;
    }

    private static LocationOrLocationLinks MakeLocationFromOffset(
        string filePath, string text, int offset)
    {
        int line = 0, col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n') { line++; col = 0; }
            else if (text[i] != '\r') { col++; }
        }
        return MakeLocation(filePath, line + 1, col);
    }
}
