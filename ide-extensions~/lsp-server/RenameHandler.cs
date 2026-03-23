using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using UitkxLanguageServer.Roslyn;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// Handles <c>textDocument/prepareRename</c> and <c>textDocument/rename</c>
/// for <c>.uitkx</c> files.
///
/// <para><b>Two rename paths:</b></para>
/// <list type="bullet">
///   <item><b>C# path</b> — cursor is inside a source-mapped C# region
///   (code block, inline expression).  Roslyn's <c>Renamer.RenameSymbolAsync</c>
///   renames the symbol inside the per-file AdhocWorkspace then diffs are mapped
///   back to <c>.uitkx</c> coordinates via the <see cref="SourceMap"/>.</item>
///   <item><b>Component-name path</b> — cursor is on a tag name
///   (<c>&lt;Counter /&gt;</c>) or a <c>component Counter { … }</c> declaration.
///   A workspace-wide text scan replaces all tag references and the declaration.</item>
/// </list>
///
/// When a C# rename targets the component class itself (the <c>partial class</c>
/// generated from the <c>component</c> directive), both paths execute: Roslyn
/// renames code-level references and the text scan renames tags + declaration.
/// </summary>
public sealed class RenameHandler : IRenameHandler, IPrepareRenameHandler
{
    private readonly DocumentStore _store;
    private readonly WorkspaceIndex _index;
    private readonly RoslynHost _roslynHost;

    public RenameHandler(DocumentStore store, WorkspaceIndex index, RoslynHost roslynHost)
    {
        _store = store;
        _index = index;
        _roslynHost = roslynHost;
    }

    // ── Registration ──────────────────────────────────────────────────────────

    public RenameRegistrationOptions GetRegistrationOptions(
        RenameCapability capability,
        ClientCapabilities clientCapabilities
    ) =>
        new RenameRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" },
                new TextDocumentFilter { Pattern = "**/*.cs" }
            ),
            PrepareProvider = true,
        };

    // ── textDocument/prepareRename ────────────────────────────────────────────

    public async Task<RangeOrPlaceholderRange?> Handle(
        PrepareRenameParams request,
        CancellationToken ct
    )
    {
        var localPath = UriToPath(request.TextDocument.Uri);
        if (localPath == null)
            return null;

        if (!TryGetText(request.TextDocument.Uri, localPath, out var text))
            return null;

        // ── .cs companion file path ──────────────────────────────────────
        if (localPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return await PrepareRenameCsAsync(localPath, text, request.Position, ct).ConfigureAwait(false);

        var parseResult = ParseText(text, localPath);
        var offset = ToOffset(text, request.Position);

        // Ensure workspace is up-to-date for Roslyn queries
        await _roslynHost
            .EnsureReadyAsync(localPath, text, parseResult, ct)
            .ConfigureAwait(false);

        // ── C# path: cursor in a source-mapped C# region ─────────────────
        var vdoc = _roslynHost.GetVirtualDocument(localPath);
        if (vdoc != null)
        {
            var virtualResult = vdoc.Map.ToVirtualOffset(offset);
            ServerLog.Log($"[Rename] PrepareRename: offset={offset}, virtualOffset={virtualResult?.VirtualOffset.ToString() ?? "NULL"}, kind={virtualResult?.Entry.Kind.ToString() ?? "N/A"}");
            if (virtualResult.HasValue)
            {
                var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
                if (roslynDoc != null)
                {
                    var symbol = ResolveSymbol(roslynDoc, virtualResult.Value.VirtualOffset, ct);
                    ServerLog.Log($"[Rename] PrepareRename: symbol={symbol?.Name ?? "NULL"}, kind={symbol?.Kind.ToString() ?? "N/A"}, renameable={symbol != null && IsRenameable(symbol)}");
                    if (symbol != null && IsRenameable(symbol))
                    {
                        var (word, wordStart, wordEnd) = GetWordAtOffset(text, offset);
                        if (!string.IsNullOrEmpty(word))
                        {
                            ServerLog.Log(
                                $"[Rename] PrepareRename (C#): '{word}' ({symbol.Kind})"
                            );
                            return new RangeOrPlaceholderRange(
                                new PlaceholderRange
                                {
                                    Range = OffsetRangeToLspRange(text, wordStart, wordEnd),
                                    Placeholder = word,
                                }
                            );
                        }
                    }
                }
            }
        }

        // ── Component-name path: tag name or component declaration ────────
        var (cWord, cStart, cEnd) = GetWordAtOffset(text, offset);
        if (!string.IsNullOrEmpty(cWord))
        {
            // Component declaration: `component FooBar { … }`
            if (
                parseResult.Directives.IsFunctionStyle
                && cWord == parseResult.Directives.ComponentName
            )
            {
                ServerLog.Log($"[Rename] PrepareRename (declaration): '{cWord}'");
                return new RangeOrPlaceholderRange(
                    new PlaceholderRange
                    {
                        Range = OffsetRangeToLspRange(text, cStart, cEnd),
                        Placeholder = cWord,
                    }
                );
            }

            // Tag reference: <FooBar … /> — the element must be a UITKX component
            if (IsOnTagName(text, offset, cWord))
            {
                var elementInfo = _index.TryGetElementInfo(cWord);
                if (
                    elementInfo != null
                    && elementInfo.FilePath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase)
                )
                {
                    ServerLog.Log($"[Rename] PrepareRename (tag): '{cWord}'");
                    return new RangeOrPlaceholderRange(
                        new PlaceholderRange
                        {
                            Range = OffsetRangeToLspRange(text, cStart, cEnd),
                            Placeholder = cWord,
                        }
                    );
                }
            }
        }

        return null;
    }

    // ── textDocument/rename ───────────────────────────────────────────────────

    public async Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken ct)
    {
        var localPath = UriToPath(request.TextDocument.Uri);
        if (localPath == null)
            return null;

        if (!TryGetText(request.TextDocument.Uri, localPath, out var text))
            return null;

        var newName = request.NewName;
        if (string.IsNullOrWhiteSpace(newName) || !IsValidIdentifier(newName))
            return null;

        // ── .cs companion file path ──────────────────────────────────────
        if (localPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return await RenameCsAsync(localPath, text, newName, request.Position, ct).ConfigureAwait(false);

        var parseResult = ParseText(text, localPath);
        var offset = ToOffset(text, request.Position);

        await _roslynHost
            .EnsureReadyAsync(localPath, text, parseResult, ct)
            .ConfigureAwait(false);

        var changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();

        // ── C# path ──────────────────────────────────────────────────────
        var vdoc = _roslynHost.GetVirtualDocument(localPath);
        if (vdoc != null)
        {
            var virtualResult = vdoc.Map.ToVirtualOffset(offset);
            if (virtualResult.HasValue)
            {
                var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
                if (roslynDoc != null)
                {
                    var symbol = ResolveSymbol(
                        roslynDoc,
                        virtualResult.Value.VirtualOffset,
                        ct
                    );
                    if (symbol != null && IsRenameable(symbol))
                    {
                        ServerLog.Log(
                            $"[Rename] Rename (C#): '{symbol.Name}' → '{newName}' ({symbol.Kind})"
                        );

                        try
                        {
                        // Run Roslyn Renamer
                        var oldSolution = roslynDoc.Project.Solution;
                        // Use a dedicated timeout token — the LSP request token
                        // can be cancelled by a concurrent didChange notification,
                        // which races with Renamer's internal formatting pass.
                        using var renameCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                        var newSolution = await Renamer
                            .RenameSymbolAsync(
                                oldSolution,
                                symbol,
                                new SymbolRenameOptions(),
                                newName,
                                renameCts.Token
                            )
                            .ConfigureAwait(false);

                        CollectRoslynEdits(
                            oldSolution,
                            newSolution,
                            roslynDoc.Id,
                            localPath,
                            text,
                            vdoc.Map,
                            symbol.Name,
                            newName,
                            changes,
                            ct
                        );

                        // Store post-rename .cs text so the upcoming workspace
                        // rebuild reads it instead of stale disk content.
                        SnapshotCompanionOverlay(
                            oldSolution, newSolution, roslynDoc.Id, localPath);
                        }
                        catch (Exception ex)
                        {
                            ServerLog.Log($"[Rename] ERROR: {ex}");
                            return null;
                        }

                        // If the symbol is the component class, also do
                        // cross-file tag + declaration rename.
                        if (symbol is INamedTypeSymbol namedType)
                        {
                            if (
                                parseResult.Directives.IsFunctionStyle
                                && namedType.Name == parseResult.Directives.ComponentName
                            )
                            {
                                ServerLog.Log(
                                    $"[Rename] Component class rename: '{namedType.Name}' → '{newName}'"
                                );
                                CollectComponentRenameEdits(
                                    namedType.Name,
                                    newName,
                                    changes
                                );
                            }
                        }

                        // Log what we're returning
                        int totalEdits = 0;
                        foreach (var kvp in changes)
                        {
                            int count = 0;
                            foreach (var _ in kvp.Value) count++;
                            totalEdits += count;
                        }
                        ServerLog.Log($"[Rename] C# path: {changes.Count} file(s), {totalEdits} edit(s)");

                        return new WorkspaceEdit { Changes = changes };
                    }
                }
            }
        }

        // ── Component-name path ──────────────────────────────────────────
        var (word, _, _) = GetWordAtOffset(text, offset);
        if (!string.IsNullOrEmpty(word))
        {
            bool isDeclaration =
                parseResult.Directives.IsFunctionStyle
                && word == parseResult.Directives.ComponentName;

            bool isTagRef = false;
            if (!isDeclaration && IsOnTagName(text, offset, word))
            {
                var elementInfo = _index.TryGetElementInfo(word);
                isTagRef =
                    elementInfo != null
                    && elementInfo.FilePath.EndsWith(
                        ".uitkx",
                        StringComparison.OrdinalIgnoreCase
                    );
            }

            if (isDeclaration || isTagRef)
            {
                ServerLog.Log($"[Rename] Component rename: '{word}' → '{newName}'");
                CollectComponentRenameEdits(word, newName, changes);
                return new WorkspaceEdit { Changes = changes };
            }
        }

        return null;
    }

    // ── .cs companion rename ──────────────────────────────────────────────────

    private async Task<RangeOrPlaceholderRange?> PrepareRenameCsAsync(
        string csPath, string text, Position position, CancellationToken ct)
    {
        // Refresh the companion doc text so Roslyn sees the latest content
        // (e.g. after a rename from the .uitkx side applied edits here).
        var refreshedDoc = _roslynHost.RefreshCompanionDocument(csPath, text);
        if (refreshedDoc == null)
        {
            ServerLog.Log($"[Rename] PrepareRename (.cs): no companion workspace for {Path.GetFileName(csPath)}");
            return null;
        }

        var offset = ToOffset(text, position);
        var symbol = ResolveSymbol(refreshedDoc, offset, ct);
        if (symbol == null || !IsRenameable(symbol))
            return null;

        var (word, wordStart, wordEnd) = GetWordAtOffset(text, offset);
        if (string.IsNullOrEmpty(word))
            return null;

        ServerLog.Log($"[Rename] PrepareRename (.cs): '{word}' ({symbol.Kind}) in {Path.GetFileName(csPath)}");
        return new RangeOrPlaceholderRange(
            new PlaceholderRange
            {
                Range = OffsetRangeToLspRange(text, wordStart, wordEnd),
                Placeholder = word,
            }
        );
    }

    private async Task<WorkspaceEdit?> RenameCsAsync(
        string csPath, string text, string newName, Position position, CancellationToken ct)
    {
        // Refresh the companion doc text so Roslyn sees the latest content.
        var refreshedDoc = _roslynHost.RefreshCompanionDocument(csPath, text);
        if (refreshedDoc == null)
        {
            ServerLog.Log($"[Rename] Rename (.cs): no companion workspace for {Path.GetFileName(csPath)}");
            return null;
        }

        // Re-lookup the full companion info (uitkxPath, mainDocId, vdoc)
        var result = _roslynHost.FindCompanionDocument(csPath);
        if (result == null)
            return null;

        var (companionDoc, uitkxPath, mainDocId, vdoc) = result.Value;
        var offset = ToOffset(text, position);
        var symbol = ResolveSymbol(companionDoc, offset, ct);
        if (symbol == null || !IsRenameable(symbol))
            return null;

        ServerLog.Log($"[Rename] Rename (.cs): '{symbol.Name}' → '{newName}' ({symbol.Kind}) in {Path.GetFileName(csPath)}");

        var changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
        try
        {
            var oldSolution = companionDoc.Project.Solution;
            using var renameCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var newSolution = await Renamer
                .RenameSymbolAsync(
                    oldSolution,
                    symbol,
                    new SymbolRenameOptions(),
                    newName,
                    renameCts.Token
                )
                .ConfigureAwait(false);

            // Read the .uitkx text for SourceMap mapping
            string? uitkxText = null;
            var uitkxUri = DocumentUri.FromFileSystemPath(uitkxPath);
            if (!_store.TryGet(uitkxUri, out uitkxText!) || uitkxText == null)
            {
                if (File.Exists(uitkxPath))
                    uitkxText = File.ReadAllText(uitkxPath);
            }

            if (uitkxText != null && vdoc != null)
            {
                CollectRoslynEdits(
                    oldSolution, newSolution, mainDocId,
                    uitkxPath, uitkxText, vdoc.Map,
                    symbol.Name, newName, changes, ct);
            }

            // Store post-rename .cs text so the upcoming workspace
            // rebuild reads it instead of stale disk content.
            SnapshotCompanionOverlay(
                oldSolution, newSolution, mainDocId, uitkxPath);
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[Rename] ERROR (.cs): {ex}");
            return null;
        }

        int totalEdits = 0;
        foreach (var kvp in changes)
        {
            int count = 0;
            foreach (var _ in kvp.Value) count++;
            totalEdits += count;
        }
        ServerLog.Log($"[Rename] .cs path: {changes.Count} file(s), {totalEdits} edit(s)");

        return totalEdits > 0 ? new WorkspaceEdit { Changes = changes } : null;
    }

    // ── Roslyn edit collection ────────────────────────────────────────────────

    /// <summary>
    /// Diffs the old and new Roslyn <see cref="Solution"/> and collects text
    /// edits.  Main-document changes are mapped via the <see cref="SourceMap"/>
    /// to <c>.uitkx</c> coordinates.  Companion-document changes target the
    /// real <c>.cs</c> file.
    /// </summary>
    private static void CollectRoslynEdits(
        Solution oldSolution,
        Solution newSolution,
        DocumentId mainDocId,
        string uitkxFilePath,
        string uitkxText,
        SourceMap map,
        string oldName,
        string newName,
        Dictionary<DocumentUri, IEnumerable<TextEdit>> changes,
        CancellationToken ct
    )
    {
        var solutionChanges = newSolution.GetChanges(oldSolution);
        int projectCount = 0;
        foreach (var projectChanges in solutionChanges.GetProjectChanges())
        {
            projectCount++;
            var changedDocs = projectChanges.GetChangedDocuments();
            foreach (var docId in changedDocs)
            {
                var oldDoc = oldSolution.GetDocument(docId);
                var newDoc = newSolution.GetDocument(docId);
                if (oldDoc == null || newDoc == null)
                    continue;

#pragma warning disable VSTHRD002
                var oldText = oldDoc.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();
                var newText = newDoc.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                var textChanges = newText.GetTextChanges(oldText);

                if (docId == mainDocId)
                {
                    // ── Main virtual document → map back to .uitkx ───────
                    // Roslyn's Renamer often returns a single whole-document
                    // replacement rather than surgical per-token edits.
                    // We can't map scaffold-region offsets back to .uitkx,
                    // so instead we scan each SourceMap entry for occurrences
                    // of the old symbol name that became the new name in the
                    // Renamer output, and emit per-occurrence edits.
                    var uri = DocumentUri.FromFileSystemPath(uitkxFilePath);
                    var oldStr = oldText.ToString();
                    var newStr = newText.ToString();

                    // Walk entries in order.  Track cumulative offset shift
                    // caused by earlier renames (name length may change).
                    int cumulativeShift = 0;

                    foreach (var entry in map.Entries)
                    {
                        int oldLen = entry.VirtualEnd - entry.VirtualStart;
                        if (entry.VirtualStart >= oldStr.Length)
                            continue;

                        var oldSlice = oldStr.Substring(
                            entry.VirtualStart,
                            Math.Min(oldLen, oldStr.Length - entry.VirtualStart));

                        int newStart = entry.VirtualStart + cumulativeShift;
                        if (newStart >= newStr.Length)
                            continue;

                        int newLen = oldLen; // start with same length estimate
                        // Compute actual new-region length by counting how many
                        // renames happened inside this region.
                        int occurrences = CountOccurrences(oldSlice, oldName);
                        int lengthDelta = occurrences * (newName.Length - oldName.Length);
                        newLen = oldLen + lengthDelta;

                        if (newStart + newLen > newStr.Length)
                            newLen = newStr.Length - newStart;

                        var newSlice = newStr.Substring(newStart, newLen);

                        if (oldSlice == newSlice)
                        {
                            cumulativeShift += lengthDelta;
                            continue;
                        }

                        // Emit one edit per occurrence within this mapped region.
                        // Walk the old slice finding occurrences as whole-word matches.
                        int searchPos = 0;
                        while (searchPos < oldSlice.Length)
                        {
                            int idx = oldSlice.IndexOf(oldName, searchPos, StringComparison.Ordinal);
                            if (idx < 0) break;

                            // Whole-word check
                            bool leftOk = idx == 0 || !char.IsLetterOrDigit(oldSlice[idx - 1]) && oldSlice[idx - 1] != '_';
                            int endIdx = idx + oldName.Length;
                            bool rightOk = endIdx >= oldSlice.Length || !char.IsLetterOrDigit(oldSlice[endIdx]) && oldSlice[endIdx] != '_';

                            if (leftOk && rightOk)
                            {
                                int uitkxOffset = entry.UitkxStart + idx;
                                AddEdit(
                                    changes,
                                    uri,
                                    new TextEdit
                                    {
                                        Range = OffsetRangeToLspRange(
                                            uitkxText,
                                            uitkxOffset,
                                            uitkxOffset + oldName.Length),
                                        NewText = newName,
                                    }
                                );
                            }

                            searchPos = idx + oldName.Length;
                        }

                        cumulativeShift += lengthDelta;
                    }
                }
                else
                {
                    // ── Companion .cs document → edits target the real file ──
                    var dir = Path.GetDirectoryName(uitkxFilePath);
                    if (dir == null)
                        continue;

                    var companionPath = Path.Combine(dir, oldDoc.Name);
                    if (!File.Exists(companionPath))
                        continue;

                    var uri = DocumentUri.FromFileSystemPath(companionPath);
                    foreach (var change in textChanges)
                    {
                        var startLinePos = oldText.Lines.GetLinePosition(change.Span.Start);
                        var endLinePos = oldText.Lines.GetLinePosition(
                            change.Span.Start + change.Span.Length
                        );

                        AddEdit(
                            changes,
                            uri,
                            new TextEdit
                            {
                                Range = new LspRange(
                                    new Position(startLinePos.Line, startLinePos.Character),
                                    new Position(endLinePos.Line, endLinePos.Character)
                                ),
                                NewText = change.NewText!,
                            }
                        );
                    }
                }
            }
        }
        if (projectCount == 0)
            ServerLog.Log("[Rename] CollectRoslynEdits: NO project changes found in solution diff");
    }

    /// <summary>
    /// Scans the entire workspace for <c>component</c> declarations and tag
    /// references matching <paramref name="oldName"/> and generates edits to
    /// replace them with <paramref name="newName"/>.
    /// </summary>
    private void CollectComponentRenameEdits(
        string oldName,
        string newName,
        Dictionary<DocumentUri, IEnumerable<TextEdit>> changes
    )
    {
        var workspaceRoot = _roslynHost.WorkspaceRoot;
        if (string.IsNullOrEmpty(workspaceRoot))
            return;

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(workspaceRoot, "*.uitkx", SearchOption.AllDirectories);
        }
        catch
        {
            return;
        }

        foreach (var uitkxFile in files)
        {
            if (IsInsideTildeFolder(uitkxFile))
                continue;

            string fileText;
            var fileUri = DocumentUri.FromFileSystemPath(uitkxFile);
            if (!_store.TryGet(fileUri, out fileText!))
            {
                try
                {
                    fileText = File.ReadAllText(uitkxFile);
                }
                catch
                {
                    continue;
                }
            }

            RenameComponentDeclarationInFile(uitkxFile, fileText, oldName, newName, changes);
            RenameTagUsages(uitkxFile, fileText, oldName, newName, changes);
        }
    }

    private static void RenameComponentDeclarationInFile(
        string filePath,
        string text,
        string oldName,
        string newName,
        Dictionary<DocumentUri, IEnumerable<TextEdit>> changes
    )
    {
        // Match `component OldName` or `@component OldName` at line start
        var pattern = new Regex(
            $@"(?:^|\n)\s*(?:@component|component)\s+({Regex.Escape(oldName)})\b",
            RegexOptions.CultureInvariant
        );
        var match = pattern.Match(text);
        if (!match.Success)
            return;

        var group = match.Groups[1];
        AddEdit(
            changes,
            DocumentUri.FromFileSystemPath(filePath),
            new TextEdit
            {
                Range = OffsetRangeToLspRange(text, group.Index, group.Index + group.Length),
                NewText = newName,
            }
        );
    }

    private static void RenameTagUsages(
        string filePath,
        string text,
        string oldName,
        string newName,
        Dictionary<DocumentUri, IEnumerable<TextEdit>> changes
    )
    {
        // Match opening, closing, and self-closing tag names:
        //   <OldName …>  </OldName>  <OldName />
        var tagPattern = new Regex(
            $@"<(/?)({Regex.Escape(oldName)})(?=[\s/>])",
            RegexOptions.CultureInvariant
        );

        var uri = DocumentUri.FromFileSystemPath(filePath);
        foreach (Match m in tagPattern.Matches(text))
        {
            var nameGroup = m.Groups[2];
            AddEdit(
                changes,
                uri,
                new TextEdit
                {
                    Range = OffsetRangeToLspRange(
                        text,
                        nameGroup.Index,
                        nameGroup.Index + nameGroup.Length
                    ),
                    NewText = newName,
                }
            );
        }
    }

    // ── Symbol resolution ─────────────────────────────────────────────────────

    private static ISymbol? ResolveSymbol(
        Document doc,
        int virtualOffset,
        CancellationToken ct
    )
    {
        try
        {
#pragma warning disable VSTHRD002
            var syntaxRoot = doc.GetSyntaxRootAsync(ct).GetAwaiter().GetResult();
            var semanticModel = doc.GetSemanticModelAsync(ct).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (syntaxRoot == null || semanticModel == null)
                return null;

            int pos = virtualOffset > 0 ? virtualOffset - 1 : 0;
            var token = syntaxRoot.FindToken(pos);
            if (token.Parent == null)
                return null;

            // Try declared symbol first (cursor on a declaration)
            var declared = semanticModel.GetDeclaredSymbol(token.Parent, ct);
            if (declared != null)
                return declared;

            // Then try referenced symbol (cursor on a usage)
            var symbolInfo = semanticModel.GetSymbolInfo(token.Parent, ct);
            return symbolInfo.Symbol
                ?? (symbolInfo.CandidateSymbols.Length > 0 ? symbolInfo.CandidateSymbols[0] : null);
        }
        catch (Exception ex)
        {
            ServerLog.Log($"[Rename] ResolveSymbol error: {ex.Message}");
            return null;
        }
    }

    private static bool IsRenameable(ISymbol symbol)
    {
        // Metadata-only symbols (BCL types, Unity API, etc.) cannot be renamed
        if (symbol.Locations.Length > 0 && symbol.Locations[0].IsInMetadata)
            return false;

        // Compiler-generated or scaffold symbols (__ prefix)
        if (symbol.IsImplicitlyDeclared)
            return false;
        if (symbol.Name.StartsWith("__", StringComparison.Ordinal))
            return false;

        // Built-in property names on the generated props class
        if (symbol.Name == "Key" || symbol.Name == "Ref")
        {
            if (symbol.ContainingType?.Name?.EndsWith("Props") == true)
                return false;
        }

        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int CountOccurrences(string text, string word)
    {
        int count = 0;
        int idx = 0;
        while ((idx = text.IndexOf(word, idx, StringComparison.Ordinal)) >= 0)
        {
            bool leftOk = idx == 0 || !char.IsLetterOrDigit(text[idx - 1]) && text[idx - 1] != '_';
            int end = idx + word.Length;
            bool rightOk = end >= text.Length || !char.IsLetterOrDigit(text[end]) && text[end] != '_';
            if (leftOk && rightOk) count++;
            idx = end;
        }
        return count;
    }

    private static void AddEdit(
        Dictionary<DocumentUri, IEnumerable<TextEdit>> changes,
        DocumentUri uri,
        TextEdit edit
    )
    {
        if (!changes.TryGetValue(uri, out var existing))
        {
            changes[uri] = new List<TextEdit> { edit };
        }
        else
        {
            ((List<TextEdit>)existing).Add(edit);
        }
    }

    private bool TryGetText(DocumentUri uri, string localPath, out string text)
    {
        if (_store.TryGet(uri, out text!) && text != null)
            return true;

        if (File.Exists(localPath))
        {
            text = File.ReadAllText(localPath);
            _store.Set(uri, text);
            return true;
        }

        text = "";
        return false;
    }

    /// <summary>
    /// After a successful rename, snapshot the post-rename text of every
    /// changed companion .cs document into the RoslynHost overlay so the
    /// next workspace rebuild doesn't read stale content from disk.
    /// </summary>
    private void SnapshotCompanionOverlay(
        Solution oldSolution,
        Solution newSolution,
        DocumentId mainDocId,
        string uitkxFilePath)
    {
        var dir = Path.GetDirectoryName(uitkxFilePath);
        if (dir == null) return;

        foreach (var projectChanges in newSolution.GetChanges(oldSolution).GetProjectChanges())
        {
            foreach (var docId in projectChanges.GetChangedDocuments())
            {
                if (docId == mainDocId) continue;

                var newDoc = newSolution.GetDocument(docId);
                if (newDoc == null) continue;

                var companionPath = Path.Combine(dir, newDoc.Name);
#pragma warning disable VSTHRD002
                var newText = newDoc.GetTextAsync(CancellationToken.None)
                    .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                _roslynHost.SetCompanionOverlay(companionPath, newText.ToString());
            }
        }
    }

    private static ParseResult ParseText(string text, string filePath)
    {
        var parseDiags = new List<ReactiveUITK.Language.ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, filePath, parseDiags);
        var nodes = UitkxParser.Parse(text, filePath, directives, parseDiags);
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(parseDiags));
    }

    private static string? UriToPath(DocumentUri uri)
    {
        try
        {
            return new Uri(uri.ToString()).LocalPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a 0-based LSP <see cref="Position"/> (line, character) to a
    /// 0-based character offset in <paramref name="text"/>.
    /// </summary>
    private static int ToOffset(string text, Position position)
    {
        int line = (int)position.Line;
        int column = (int)position.Character;
        int offset = 0;
        for (int i = 0; i < line && offset < text.Length; i++)
        {
            var nl = text.IndexOf('\n', offset);
            offset = nl < 0 ? text.Length : nl + 1;
        }
        return Math.Min(offset + column, text.Length);
    }

    /// <summary>
    /// Converts a 0-based character offset in <paramref name="text"/> to a
    /// 0-based LSP <see cref="Position"/>.
    /// </summary>
    private static Position OffsetToPosition(string text, int offset)
    {
        int line = 0,
            col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                col = 0;
            }
            else if (text[i] != '\r')
            {
                col++;
            }
        }
        return new Position(line, col);
    }

    private static LspRange OffsetRangeToLspRange(string text, int start, int end)
    {
        return new LspRange(OffsetToPosition(text, start), OffsetToPosition(text, end));
    }

    private static (string Word, int Start, int End) GetWordAtOffset(string text, int offset)
    {
        if (offset < 0 || offset >= text.Length)
            return ("", offset, offset);

        // If the cursor char is not an ident char, try one position back
        // (cursor is often positioned right after the identifier).
        if (!IsIdentChar(text[offset]) && offset > 0 && IsIdentChar(text[offset - 1]))
            offset--;

        if (!IsIdentChar(text[offset]))
            return ("", offset, offset);

        int start = offset;
        while (start > 0 && IsIdentChar(text[start - 1]))
            start--;

        int end = offset;
        while (end < text.Length && IsIdentChar(text[end]))
            end++;

        return (text.Substring(start, end - start), start, end);
    }

    private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// Returns <c>true</c> when the word at <paramref name="offset"/> is
    /// preceded by <c>&lt;</c> or <c>&lt;/</c>, i.e. it is a tag name.
    /// </summary>
    private static bool IsOnTagName(string text, int offset, string word)
    {
        var (_, wordStart, _) = GetWordAtOffset(text, offset);
        int i = wordStart - 1;
        // Skip whitespace between '<' and the tag name (uncommon but valid)
        while (i >= 0 && text[i] == ' ')
            i--;
        if (i >= 0 && text[i] == '<')
            return true;
        if (i >= 1 && text[i] == '/' && text[i - 1] == '<')
            return true;
        return false;
    }

    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
        for (int i = 1; i < name.Length; i++)
        {
            if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                return false;
        }
        return true;
    }

    private static bool IsInsideTildeFolder(string filePath)
    {
        var segments = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var seg in segments)
        {
            if (seg.Length > 1 && seg[seg.Length - 1] == '~')
                return true;
        }
        return false;
    }
}
