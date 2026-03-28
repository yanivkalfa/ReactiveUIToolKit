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
    ) => LspHelpers.ResolveSymbol(doc, virtualOffset, ct);

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

    // ── Helpers (delegated to LspHelpers) ──────────────────────────────────

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

    private static int CountOccurrences(string text, string word)
        => LspHelpers.CountOccurrences(text, word);

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
        => LspHelpers.TryGetText(_store, uri, localPath, out text);

    private static ParseResult ParseText(string text, string filePath)
        => LspHelpers.ParseText(text, filePath);

    private static string? UriToPath(DocumentUri uri)
        => LspHelpers.UriToPath(uri);

    private static int ToOffset(string text, Position position)
        => LspHelpers.ToOffset(text, position);

    private static Position OffsetToPosition(string text, int offset)
        => LspHelpers.OffsetToPosition(text, offset);

    private static LspRange OffsetRangeToLspRange(string text, int start, int end)
        => LspHelpers.OffsetRangeToLspRange(text, start, end);

    private static (string Word, int Start, int End) GetWordAtOffset(string text, int offset)
        => LspHelpers.GetWordAtOffset(text, offset);

    private static bool IsIdentChar(char c) => LspHelpers.IsIdentChar(c);

    private static bool IsOnTagName(string text, int offset, string word)
        => LspHelpers.IsOnTagName(text, offset, word);

    private static bool IsValidIdentifier(string name)
        => LspHelpers.IsValidIdentifier(name);

    private static bool IsInsideTildeFolder(string filePath)
        => LspHelpers.IsInsideTildeFolder(filePath);
}
