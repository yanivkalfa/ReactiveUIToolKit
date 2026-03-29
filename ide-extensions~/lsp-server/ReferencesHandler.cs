using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Parser;

using LspLocation = OmniSharp.Extensions.LanguageServer.Protocol.Models.Location;
using ReactiveUITK.Language.Roslyn;
using UitkxLanguageServer.Roslyn;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// Handles <c>textDocument/references</c> (Shift+F12) for <c>.uitkx</c> files.
///
/// <para><b>Two paths (mirrors RenameHandler):</b></para>
/// <list type="bullet">
///   <item><b>C# path</b> — cursor is inside a source-mapped C# region.
///   Resolves the symbol via Roslyn, uses <see cref="SymbolFinder"/> to find
///   all references, then maps locations back to <c>.uitkx</c> coordinates
///   via the <see cref="SourceMap"/>.</item>
///   <item><b>Component-name path</b> — cursor is on a tag name or
///   <c>component</c> declaration.  Workspace-wide text scan for all tag
///   usages and declarations.</item>
/// </list>
/// </summary>
public sealed class ReferencesHandler : IReferencesHandler
{
    private readonly DocumentStore _store;
    private readonly WorkspaceIndex _index;
    private readonly RoslynHost _roslynHost;

    public ReferencesHandler(DocumentStore store, WorkspaceIndex index, RoslynHost roslynHost)
    {
        _store = store;
        _index = index;
        _roslynHost = roslynHost;
    }

    public ReferenceRegistrationOptions GetRegistrationOptions(
        ReferenceCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new ReferenceRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" },
                new TextDocumentFilter { Pattern = "**/*.cs" }
            ),
        };

    public async Task<LocationContainer?> Handle(ReferenceParams request, CancellationToken ct)
    {
        var localPath = LspHelpers.UriToPath(request.TextDocument.Uri);
        if (localPath == null)
            return null;

        if (!LspHelpers.TryGetText(_store, request.TextDocument.Uri, localPath, out var text))
            return null;

        bool includeDeclaration = request.Context?.IncludeDeclaration ?? true;

        // ── .cs companion file ───────────────────────────────────────────
        if (localPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return await FindReferencesInCsAsync(localPath, text, request.Position, includeDeclaration, ct)
                .ConfigureAwait(false);

        var parseResult = LspHelpers.ParseText(text, localPath);
        var offset = LspHelpers.ToOffset(text, request.Position);

        await _roslynHost
            .EnsureReadyAsync(localPath, text, parseResult, ct)
            .ConfigureAwait(false);

        // ── C# path: cursor in a source-mapped C# region ─────────────────
        var vdoc = _roslynHost.GetVirtualDocument(localPath);
        if (vdoc != null)
        {
            var virtualResult = vdoc.Map.ToVirtualOffset(offset);
            if (virtualResult.HasValue)
            {
                var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
                if (roslynDoc != null)
                {
                    var symbol = LspHelpers.ResolveSymbol(
                        roslynDoc, virtualResult.Value.VirtualOffset, ct);

                    if (symbol != null && LspHelpers.IsUserSymbol(symbol))
                    {
                        ServerLog.Log(
                            $"[References] C# symbol: '{symbol.Name}' ({symbol.Kind})");

                        var locations = CollectRoslynReferences(
                            roslynDoc, symbol, localPath, text, vdoc,
                            includeDeclaration, ct);

                        ServerLog.Log($"[References] C# path: {locations.Count} location(s)");
                        return new LocationContainer(locations);
                    }
                }
            }
        }

        // ── Component-name path: tag name or component declaration ────────
        var (word, _, _) = LspHelpers.GetWordAtOffset(text, offset);
        if (!string.IsNullOrEmpty(word))
        {
            bool isDeclaration =
                parseResult.Directives.IsFunctionStyle
                && word == parseResult.Directives.ComponentName;

            bool isTagRef = false;
            if (!isDeclaration && LspHelpers.IsOnTagName(text, offset, word))
            {
                var elementInfo = _index.TryGetElementInfo(word);
                isTagRef = elementInfo != null
                    && elementInfo.FilePath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase);
            }

            if (isDeclaration || isTagRef)
            {
                ServerLog.Log($"[References] Component: '{word}'");
                var locations = CollectComponentReferences(word, includeDeclaration);
                ServerLog.Log($"[References] Component path: {locations.Count} location(s)");
                return new LocationContainer(locations);
            }
        }

        return null;
    }

    // ── .cs companion file references ─────────────────────────────────────────

    private async Task<LocationContainer?> FindReferencesInCsAsync(
        string csPath, string text, Position position,
        bool includeDeclaration, CancellationToken ct)
    {
        var refreshedDoc = _roslynHost.RefreshCompanionDocument(csPath, text);
        if (refreshedDoc == null)
            return null;

        var result = _roslynHost.FindCompanionDocument(csPath);
        if (result == null)
            return null;

        var (companionDoc, uitkxPath, mainDocId, vdoc) = result.Value;
        var offset = LspHelpers.ToOffset(text, position);
        var symbol = LspHelpers.ResolveSymbol(companionDoc, offset, ct);
        if (symbol == null || !LspHelpers.IsUserSymbol(symbol))
            return null;

        // Read the .uitkx text for SourceMap mapping
        string? uitkxText = null;
        var uitkxUri = DocumentUri.FromFileSystemPath(uitkxPath);
        if (!_store.TryGet(uitkxUri, out uitkxText!) || uitkxText == null)
        {
            if (File.Exists(uitkxPath))
                uitkxText = File.ReadAllText(uitkxPath);
        }

        var roslynDoc = _roslynHost.GetRoslynDocument(uitkxPath);
        if (roslynDoc == null || uitkxText == null || vdoc == null)
            return null;

        var locations = CollectRoslynReferences(
            roslynDoc, symbol, uitkxPath, uitkxText, vdoc,
            includeDeclaration, ct);

        return locations.Count > 0 ? new LocationContainer(locations) : null;
    }

    // ── Roslyn-based reference collection ─────────────────────────────────────

    private List<LspLocation> CollectRoslynReferences(
        Document roslynDoc,
        ISymbol symbol,
        string uitkxFilePath,
        string uitkxText,
        VirtualDocument vdoc,
        bool includeDeclaration,
        CancellationToken ct)
    {
        var locations = new List<LspLocation>();

        try
        {
#pragma warning disable VSTHRD002
            var refs = SymbolFinder
                .FindReferencesAsync(symbol, roslynDoc.Project.Solution, ct)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

            foreach (var refSymbol in refs)
            {
                // Include declaration location(s)
                if (includeDeclaration)
                {
                    foreach (var declLoc in refSymbol.Definition.Locations)
                    {
                        var loc = MapRoslynLocation(
                            declLoc, roslynDoc, uitkxFilePath, uitkxText, vdoc);
                        if (loc != null)
                            locations.Add(loc);
                    }
                }

                // Include all reference locations
                foreach (var refLoc in refSymbol.Locations)
                {
                    var loc = MapRoslynLocation(
                        refLoc.Location, roslynDoc, uitkxFilePath, uitkxText, vdoc);
                    if (loc != null)
                        locations.Add(loc);
                }
            }
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[References] SymbolFinder error: {ex.Message}");
        }

        return locations;
    }

    /// <summary>
    /// Maps a Roslyn <see cref="Microsoft.CodeAnalysis.Location"/> back to an
    /// LSP <see cref="LspLocation"/> — either via SourceMap for the main virtual
    /// document, or directly for companion .cs files.
    /// </summary>
    private LspLocation? MapRoslynLocation(
        Microsoft.CodeAnalysis.Location roslynLoc,
        Document roslynDoc,
        string uitkxFilePath,
        string uitkxText,
        VirtualDocument vdoc)
    {
        if (!roslynLoc.IsInSource)
            return null;

        var tree = roslynLoc.SourceTree;
        if (tree == null)
            return null;

        var span = roslynLoc.SourceSpan;

        // Check if this location is in the main virtual document
        var mainDoc = roslynDoc.Project.Solution.GetDocument(roslynDoc.Id);
        if (mainDoc != null && tree == mainDoc.GetSyntaxTreeAsync().GetAwaiter().GetResult())
        {
            // Map via SourceMap to .uitkx coordinates
            var startResult = vdoc.Map.ToUitkxOffset(span.Start);
            var endResult = vdoc.Map.ToUitkxOffset(span.End);
            if (startResult.HasValue && endResult.HasValue)
            {
                return new LspLocation
                {
                    Uri = DocumentUri.FromFileSystemPath(uitkxFilePath),
                    Range = LspHelpers.OffsetRangeToLspRange(
                        uitkxText,
                        startResult.Value.UitkxOffset,
                        endResult.Value.UitkxOffset),
                };
            }

            // If end doesn't map, try start + symbol name length
            if (startResult.HasValue)
            {
                int nameLen = span.Length;
                return new LspLocation
                {
                    Uri = DocumentUri.FromFileSystemPath(uitkxFilePath),
                    Range = LspHelpers.OffsetRangeToLspRange(
                        uitkxText,
                        startResult.Value.UitkxOffset,
                        startResult.Value.UitkxOffset + nameLen),
                };
            }

            return null;
        }

        // Location is in a companion .cs document
        var dir = Path.GetDirectoryName(uitkxFilePath);
        if (dir == null)
            return null;

        // Find which document this tree belongs to
        foreach (var doc in roslynDoc.Project.Documents)
        {
            var docTree = doc.GetSyntaxTreeAsync().GetAwaiter().GetResult();
            if (docTree == tree)
            {
                var companionPath = Path.Combine(dir, doc.Name);
                if (!File.Exists(companionPath))
                    continue;

                var lineSpan = roslynLoc.GetLineSpan();
                return new LspLocation
                {
                    Uri = DocumentUri.FromFileSystemPath(companionPath),
                    Range = new LspRange(
                        new Position(lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character),
                        new Position(lineSpan.EndLinePosition.Line, lineSpan.EndLinePosition.Character)),
                };
            }
        }

        return null;
    }

    // ── Component/tag name reference collection ───────────────────────────────

    private List<LspLocation> CollectComponentReferences(string componentName, bool includeDeclaration)
    {
        var locations = new List<LspLocation>();
        var workspaceRoot = _roslynHost.WorkspaceRoot;
        if (string.IsNullOrEmpty(workspaceRoot))
            return locations;

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(workspaceRoot, "*.uitkx", SearchOption.AllDirectories);
        }
        catch
        {
            return locations;
        }

        foreach (var uitkxFile in files)
        {
            if (LspHelpers.IsInsideTildeFolder(uitkxFile))
                continue;

            string fileText;
            var fileUri = DocumentUri.FromFileSystemPath(uitkxFile);
            if (!_store.TryGet(fileUri, out fileText!))
            {
                try { fileText = File.ReadAllText(uitkxFile); }
                catch { continue; }
            }

            // Component declaration
            if (includeDeclaration)
            {
                var declPattern = new Regex(
                    $@"(?:^|\n)\s*(?:@component|component)\s+({Regex.Escape(componentName)})\b",
                    RegexOptions.CultureInvariant);
                var declMatch = declPattern.Match(fileText);
                if (declMatch.Success)
                {
                    var group = declMatch.Groups[1];
                    locations.Add(new LspLocation
                    {
                        Uri = fileUri,
                        Range = LspHelpers.OffsetRangeToLspRange(
                            fileText, group.Index, group.Index + group.Length),
                    });
                }
            }

            // Tag usages: <Name …>  </Name>  <Name />
            var tagPattern = new Regex(
                $@"<(/?)({Regex.Escape(componentName)})(?=[\s/>])",
                RegexOptions.CultureInvariant);

            foreach (Match m in tagPattern.Matches(fileText))
            {
                var nameGroup = m.Groups[2];
                locations.Add(new LspLocation
                {
                    Uri = fileUri,
                    Range = LspHelpers.OffsetRangeToLspRange(
                        fileText,
                        nameGroup.Index,
                        nameGroup.Index + nameGroup.Length),
                });
            }
        }

        return locations;
    }
}
