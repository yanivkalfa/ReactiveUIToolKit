# Embedded Roslyn Implementation Plan

**Status:** ✅ COMPLETE  
**Branch:** `feature/embedded-roslyn`  
**Depends on:** The LSP server and language-lib as described in `UITKX_ARCHITECTURE_LANGUAGE_SERVER.md`  
**Unlocks:** P9 (Tier-3 diagnostics), Item 2 (accurate C# coloring), Item 3 (C# formatting), Item 4 (C# completions)

---

## 1. Goals

| Goal | Description |
|------|-------------|
| G1 — Tier-3 diagnostics | Roslyn compilation errors inside `@code` / `@(expr)` appear as LSP squiggles with correct source positions in the `.uitkx` file (UITKX0200+ codes). |
| G2 — C# semantic tokens | The `SemanticTokensProvider` becomes the sole coloring authority for C# regions, replacing all regex heuristics. Accurate inside every IDE. |
| G3 — C# completions | `CompletionHandler` delegates to Roslyn `CompletionService` for positions inside C# regions. |
| G4 — C# hover | `HoverHandler` delegates to Roslyn `SemanticModel.GetSymbolInfo` for positions inside C# regions. |
| G5 — C# formatting | `AstFormatter` delegates to Roslyn `Formatter` for `@code` block content. |

**Non-goals:**
- Full OmniSharp-equivalent goto-definition across multiple projects (deferred — our `DefinitionHandler` already handles go-to-def for UITKX components via `WorkspaceIndex`).
- Hot-reload / HMR (separate roadmap item).

---

## 2. Approach Overview

### The Virtual Document Pattern

For every open `.uitkx` file the system maintains a **virtual C# document** — a complete, valid
C# source file synthesized from the `ParseResult`.  Roslyn services operate on this virtual
document; results are mapped back to `.uitkx` positions via a **source map**.

```
.uitkx source              VirtualDocument.cs (in-memory only)
─────────────              ────────────────────────────────────
@namespace MyGame.UI   →   namespace MyGame.UI {
@using UnityEngine     →       using UnityEngine;
@props PlayerHUDProps       partial class PlayerHUDFunc {
                                 private PlayerHUDProps props = default!;

@(props.Score)         →        static object __expr_6() { return props.Score; }

@code {                →        // --- @code begin (line 10) ---
  string GetLabel()    →        string GetLabel()
    => ...             →            => ...
}                      →        // --- @code end ---
                           }
                       }
```

A `SourceMap` records precise character-offset correspondences between the `.uitkx` file and
the virtual document so every Roslyn result (diagnostic span, classified span, completion
position) can be translated back to a `.uitkx` line/column.

### Two-zone split

| Zone | Virtual document representation |
|------|---------------------------------|
| `@code { ... }` body | Inserted verbatim into the class body at the same indentation; source map is a straight character-offset shift |
| `@(expr)` / `attr={expr}` | Each expression is wrapped in a `static object __expr_N()` method; only the expression text is mapped |

Control-flow expressions (`if (cond)`, `foreach (var x in coll)`) are not individually wrapped —
they are implicitly covered by the `@code` compilation context.

---

## 3. Package Changes

### 3.1 `lsp-server/UitkxLanguageServer.csproj`

Add the following `PackageReference` entries:

```xml
<!-- Roslyn compiler + semantic model (syntax, binding, diagnostics) -->
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.0.0" />

<!-- Workspace APIs: AdhocWorkspace, CompletionService, Classifier, Formatter -->
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.0.0" />
```

**Version rationale:**
- 5.0.0 is the latest stable (November 2024), targets both `net8.0` and `netstandard2.0`.
- The LSP server already targets `net8.0`; no TFM change needed.
- No conflict with Unity's internal Roslyn (4.3.1) because the LSP server is a separate process.

### 3.2 `language-lib/UitkxLanguage.csproj`

No changes.  The `language-lib` stays `netstandard2.0` with zero Roslyn references.
The virtual document generator (Phase 1) is pure C# with no Roslyn dependency.

---

## 4. New File Structure

```
ide-extensions~/
├── language-lib/
│   └── Roslyn/                           (new directory — no Roslyn NuGet dep)
│       ├── CSharpRegion.cs               Data: kind, uitkx span, content
│       ├── VirtualDocument.cs            Data: generated C# text + SourceMap + scaffolding info
│       ├── SourceMap.cs                  Span translation uitkx ↔ virtual doc
│       └── VirtualDocumentGenerator.cs  Pure transform: ParseResult → VirtualDocument
│
└── lsp-server/
    └── Roslyn/                           (new directory — Roslyn dep lives here)
        ├── RoslynHost.cs                 AdhocWorkspace lifecycle, async get-document
        ├── ReferenceAssemblyLocator.cs   Discovers assembly paths from workspace
        └── RoslynDiagnosticMapper.cs     Roslyn Diagnostic → ParseDiagnostic (Tier 3)
```

Modified existing files:

```
lsp-server/
├── Program.cs                            Register RoslynHost as singleton
├── TextSyncHandler.cs                    Notify RoslynHost on open/change/close
├── DiagnosticsPublisher.cs              Add Tier-3 Roslyn diagnostics
├── SemanticTokensHandler.cs             Pass RoslynHost to provider
└── CompletionHandler.cs                 Delegate C# positions to RoslynHost

language-lib/
├── SemanticTokens/SemanticTokensProvider.cs   Add C# region token emission via callback
└── IntelliSense/AstCursorContext.cs            Add CursorKind.CSharpExpression + CursorKind.CSharpCodeBlock
```

---

## 5. Phase 0 — Baseline: Complete Column Tracking (Pre-req)

**Status:** 🔲 NOT STARTED  
**Files:** `language-lib/Parser/MarkupTokenizer.cs`, `language-lib/Nodes/AstNode.cs`  
**Blocking:** All subsequent phases depend on accurate column data for SourceMap generation.

Today `AstNode.SourceColumn` defaults to 0 ("until column tracking is wired — Phase 3").
The source map requires byte-level offsets, not just line numbers.

### 5.1 What to do

- [ ] `MarkupTokenizer.cs`: emit `SourceColumn` (0-based) on every token the tokenizer
  produces.  The tokenizer already tracks line number; tracking column is a parallel counter
  reset on every newline.
- [ ] `AstNode.cs`: the `SourceColumn` property already exists but is always 0.  Wire it
  through every node constructor call site in `UitkxParser.cs`.
- [ ] `AstNode.EndLine` / `AstNode.EndColumn`: populate for `ElementNode` (close-tag line
  already tracked as `CloseTagLine`; add `CloseTagColumn`), `CodeBlockNode`, and
  `ExpressionNode`.
- [ ] Add `SourceOffset` (absolute character index in the source string) to `AstNode` alongside
  `SourceLine` / `SourceColumn`.  The source map uses offsets, not line+col pairs, because
  offset arithmetic is O(1) and avoids re-scanning.

### 5.2 Compatibility

The `SourceOffset` field defaults to 0 and is populated only when the tokenizer is fully wired.
All existing code that reads `SourceLine` is unchanged.  The source map generator gracefully
falls back to line-based offset computation when `SourceOffset == 0`.

---

## 6. Phase 1 — Virtual Document Generator + Source Map

**Status:** 🔲 NOT STARTED  
**Files:** `language-lib/Roslyn/` (all new)  
**No external dependencies.**

### 6.1 `CSharpRegion.cs`

```csharp
namespace ReactiveUITK.Language.Roslyn
{
    public enum CSharpRegionKind
    {
        CodeBlock,          // @code { ... } body
        InlineExpression,   // @(expr)
        AttributeExpression // attr={expr}
    }

    /// <summary>
    /// A contiguous region of C# text extracted from a .uitkx source file.
    /// </summary>
    public sealed record CSharpRegion(
        CSharpRegionKind Kind,
        /// <summary>Absolute character offset in the .uitkx source of the first C# character.</summary>
        int UitkxStartOffset,
        /// <summary>Exclusive end offset in the .uitkx source.</summary>
        int UitkxEndOffset,
        /// <summary>1-based line of UitkxStartOffset in the source.</summary>
        int UitkxStartLine,
        /// <summary>The C# text (same as source.Substring(UitkxStartOffset, length)).</summary>
        string Text,
        /// <summary>
        /// For InlineExpression / AttributeExpression: depth of surrounding element tree
        /// (used for props field injection scoping in the virtual doc).
        /// </summary>
        int NestingDepth = 0
    );
}
```

### 6.2 `SourceMap.cs`

```csharp
namespace ReactiveUITK.Language.Roslyn
{
    /// <summary>
    /// Maps character positions between a .uitkx source and its virtual C# document.
    /// </summary>
    public sealed class SourceMap
    {
        private readonly IReadOnlyList<SourceMapEntry> _entries;

        public SourceMap(IReadOnlyList<SourceMapEntry> entries) => _entries = entries;

        /// <summary>
        /// Translates a character offset in the virtual document to a .uitkx source offset.
        /// Returns -1 if the virtual offset does not correspond to a mapped region.
        /// </summary>
        public int ToUitkxOffset(int virtualOffset);

        /// <summary>
        /// Translates a .uitkx source offset to a virtual document offset.
        /// Returns -1 if the uitkx offset is not inside a mapped region.
        /// </summary>
        public int ToVirtualOffset(int uitkxOffset);

        /// <summary>
        /// Returns true when <paramref name="virtualOffset"/> falls inside a mapped region.
        /// </summary>
        public bool IsInMappedRegion(int virtualOffset);

        /// <summary>All source map entries (for debug/test use).</summary>
        public IReadOnlyList<SourceMapEntry> Entries => _entries;
    }

    /// <summary>
    /// One contiguous mapping span.
    /// For a CodeBlock region the virtual run is the verbatim @code body.
    /// For an expression region the virtual run is the expression only (inside the wrapper method).
    /// </summary>
    public sealed record SourceMapEntry(
        int VirtualStart,    // inclusive, offset in virtual doc
        int VirtualEnd,      // exclusive
        int UitkxStart,      // inclusive, offset in .uitkx source
        int UitkxEnd,        // exclusive
        CSharpRegionKind Kind
    );
}
```

### 6.3 `VirtualDocument.cs`

```csharp
namespace ReactiveUITK.Language.Roslyn
{
    public sealed class VirtualDocument
    {
        /// <summary>The complete generated C# source text fed to Roslyn.</summary>
        public string Text { get; init; } = "";

        /// <summary>Span translation between virtual text and .uitkx source.</summary>
        public SourceMap SourceMap { get; init; } = new SourceMap(Array.Empty<SourceMapEntry>());

        /// <summary>All C# regions extracted from the .uitkx file.</summary>
        public IReadOnlyList<CSharpRegion> Regions { get; init; } = Array.Empty<CSharpRegion>();

        /// <summary>
        /// The virtual character offset at which the @code body begins in <see cref="Text"/>.
        /// Used by the formatter to splice Roslyn-formatted code back.
        /// -1 if the file has no @code block.
        /// </summary>
        public int CodeBlockVirtualStart { get; init; } = -1;

        /// <summary>Exclusive end of the @code body in <see cref="Text"/>.</summary>
        public int CodeBlockVirtualEnd { get; init; } = -1;
    }
}
```

### 6.4 `VirtualDocumentGenerator.cs`

This is the most critical file in Phase 1.  It must produce a virtual C# document that:
1. Gives Roslyn the correct namespace, class name, using directives, and props type — matching
   the context the source generator would produce.
2. Embeds `@code` content verbatim at class body level (source map is a direct offset shift).
3. Wraps each `@(expr)` / `attr={expr}` expression in a method stub, preserving the expression
   verbatim (source map covers only the expression span).
4. Injects a `props` field of the component's props type so expressions can reference `props.*`
   and Roslyn resolves the type.
5. Injects `using` directives from `@using` declarations.

**Error invariant:** If `ParseResult` has parse errors the generator still produces a best-effort
virtual document (blank body or recovered partial body).  Roslyn operates on the recovered text;
its own parse errors are separate from the UITKX parse errors.

```csharp
namespace ReactiveUITK.Language.Roslyn
{
    public static class VirtualDocumentGenerator
    {
        // Standard set injected into every virtual document.
        private static readonly string[] s_standardUsings = new[]
        {
            "System",
            "System.Collections.Generic",
            "ReactiveUITK.Core",
            "UnityEngine",
            "UnityEngine.UIElements",
        };

        /// <summary>
        /// Generates the virtual C# document and source map for <paramref name="parseResult"/>.
        /// <paramref name="uitkxSource"/> is the original .uitkx source text.
        /// <paramref name="filePath"/> is used only in the generated #line directives.
        /// </summary>
        public static VirtualDocument Generate(
            ParseResult parseResult,
            string uitkxSource,
            string filePath = "");
    }
}
```

**Generated document structure:**

```csharp
// <auto-generated: UITKX Roslyn virtual document — do not edit>
// Source: {filePath}
#pragma warning disable CS0169, CS0649, CS8618, CS0414

using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;
// + @using directives from the .uitkx file

namespace {namespace}
{
    partial class {ComponentName}Func
    {
        // === PROPS INJECTION ===
        // Gives expressions access to props.* members.
        private {PropsType ?? "object"} props = default!;

        // === INLINE EXPRESSIONS ===
        // Each @(expr) and attr={expr} is wrapped so Roslyn analyses it in context.
        // Source map entry: virtual offset of `expr` → uitkx offset of `expr`.
        private static object __expr_1() { return {expression_1}; }
        private static object __expr_2() { return {expression_2}; }
        // ... one per expression in the file ...

        // === @code BODY ===
        // Inserted verbatim. Source map entry: virtual offset → uitkx @code body offset.
        // #line directives are injected at the start so Roslyn diagnostics report
        // the correct .uitkx file/line even without the source map translation.
        #line {codeBlock.SourceLine} "{filePath}"
        {codeBlock.Code}
        #line default
    }
}
```

**Implementation steps:**

- [ ] Create `language-lib/Roslyn/` directory.
- [ ] Implement `CSharpRegion.cs`, `SourceMapEntry.cs`, `SourceMap.cs`, `VirtualDocument.cs`.
- [ ] Implement `VirtualDocumentGenerator.Generate`:
  - Walk `parseResult.Directives` for namespace, component name, props type, usings.
  - Walk `parseResult.RootNodes` recursively:
    - `ExpressionNode` → add `CSharpRegion(InlineExpression, ...)` + emit wrapper method.
    - `ElementNode.Attributes` with `CSharpExpressionValue` → add `CSharpRegion(AttributeExpression, ...)` + emit wrapper method.
    - `CodeBlockNode` → add `CSharpRegion(CodeBlock, ...)` + emit verbatim with `#line` directive.
    - All other node types → recurse children.
  - Build `SourceMap` from accumulated `SourceMapEntry` records.
  - Return `VirtualDocument`.
- [ ] Unit tests (`language-lib` or a test project):
  - Given a minimal function-style component, assert the virtual doc contains the correct namespace and the expression wrapper.
  - Given a component with `@code`, assert code block content appears verbatim.
  - Assert `SourceMap.ToUitkxOffset(SourceMap.ToVirtualOffset(n)) == n` for every expression character.
  - Assert the virtual document parses without Roslyn syntax errors (even if incomplete).

---

## 7. Phase 2 — Roslyn Host

**Status:** 🔲 NOT STARTED  
**Files:** `lsp-server/Roslyn/RoslynHost.cs`, `ReferenceAssemblyLocator.cs`  
**Dependency:** Phase 0 (column tracking) + Phase 1 (VirtualDocument) + NuGet additions

### 7.1 `ReferenceAssemblyLocator.cs`

Resolves which assemblies to reference in the Roslyn compilation.  This is critical for type
resolution — without it Roslyn sees `PlayerHUDProps` as an unknown type and emits spurious errors.

**Resolution strategy (ordered, first-wins per category):**

| Priority | Source | Description |
|----------|--------|-------------|
| 1 | `{workspaceRoot}/Library/ScriptAssemblies/*.dll` | Unity-compiled project assemblies |
| 2 | `{workspaceRoot}/Library/ScriptAssemblies/UnityEngine.dll` | Core Unity assemblies |
| 3 | Unity installation `{unityEditorPath}/Data/Managed/UnityEngine/*.dll` | UI Toolkit + other modules |
| 4 | `.NET BCL` via `AppContext.BaseDirectory` | `System.*`, `mscorlib` |
| 5 | `{workspaceRoot}/**/*.csproj` → `<Reference Include=...>` | MSBuild explicit references |
| Fallback | `typeof(object).Assembly.Location` et al. | Minimal BCL — always available |

```csharp
public sealed class ReferenceAssemblyLocator
{
    /// <summary>
    /// Called once at server start (after WorkspaceIndex.OnStarted).
    /// Scans the workspace root and caches the assembly set.
    /// Expensive — run on background thread.
    /// </summary>
    public Task<IReadOnlyList<MetadataReference>> LocateAsync(
        string workspaceRoot,
        CancellationToken ct = default);

    /// <summary>Cached result from the last successful LocateAsync call.</summary>
    public IReadOnlyList<MetadataReference> CachedReferences { get; }
}
```

**Important notes:**
- `MetadataReference.CreateFromFile` has a small memory cost per assembly (metadata only — no
  IL is loaded).  Cache the list; do not re-create on every request.
- Watch `Library/ScriptAssemblies/` for changes (Unity recompile) via `WatchedFilesHandler` and
  call `LocateAsync` again, then update all open Roslyn workspaces.
- If no Unity assemblies are found, fall back to BCL-only.  Roslyn will still provide completions
  for standard library types; Unity types will be unknown (show as errors until Unity compiles).

### 7.2 `RoslynHost.cs`

One `AdhocWorkspace` per open `.uitkx` file.  The workspace contains exactly one project and
one document.

```csharp
public sealed class RoslynHost : IDisposable
{
    // ── Construction / DI ─────────────────────────────────────────────────────

    public RoslynHost(ReferenceAssemblyLocator locator);

    // ── Lifecycle (called by TextSyncHandler) ─────────────────────────────────

    /// <summary>
    /// Creates or updates the Roslyn workspace for the given .uitkx file.
    /// Parses the source through the UITKX language-lib, generates the virtual doc,
    /// and updates the AdhocWorkspace.  Idempotent and thread-safe.
    /// </summary>
    public Task UpdateAsync(DocumentUri uri, string uitkxSource, CancellationToken ct = default);

    /// <summary>Removes the workspace entry when the file is closed.</summary>
    public void Remove(DocumentUri uri);

    // ── Feature queries ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Roslyn Document for the given .uitkx URI, or null if not yet indexed.
    /// </summary>
    public Task<Document?> GetDocumentAsync(DocumentUri uri, CancellationToken ct = default);

    /// <summary>
    /// Returns classified text spans for the given virtual document character range.
    /// Each span is returned with its ClassificationTypeNames constant.
    /// </summary>
    public Task<IReadOnlyList<(TextSpan VirtualSpan, string Classification)>> GetClassifiedSpansAsync(
        DocumentUri uri,
        TextSpan virtualSpan,
        CancellationToken ct = default);

    /// <summary>
    /// Translates a .uitkx (line, col) to a virtual document position, then returns
    /// Roslyn completions at that position.
    /// Returns an empty list if the position is not in a mapped C# region.
    /// </summary>
    public Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
        DocumentUri uri,
        int uitkxLine,
        int uitkxCol,
        CancellationToken ct = default);

    /// <summary>
    /// Returns quick-info (hover) for a .uitkx position inside a C# region.
    /// </summary>
    public Task<string?> GetHoverAsync(
        DocumentUri uri,
        int uitkxLine,
        int uitkxCol,
        CancellationToken ct = default);

    /// <summary>
    /// Returns Roslyn-formatted text for the @code block body of the given file.
    /// Returns null if the file has no @code block or Roslyn is not ready.
    /// </summary>
    public Task<string?> FormatCodeBlockAsync(
        DocumentUri uri,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all Roslyn diagnostics for the given .uitkx file, with positions
    /// already translated to .uitkx source coordinates.
    /// </summary>
    public Task<IReadOnlyList<ParseDiagnostic>> GetDiagnosticsAsync(
        DocumentUri uri,
        CancellationToken ct = default);

    public void Dispose();
}
```

**Internal implementation notes:**

1. **One workspace per file** rather than one workspace for all files.  This is simpler and
   avoids cross-file interference.  The cost is one `AdhocWorkspace` per open file (~1 MB RAM
   each); acceptable for typical editor usage (≤20 open files).

2. **Workspace update is async and debounced.** `TextSyncHandler.DidChange` fires on every
   keystroke.  Debounce by 300 ms (cancel + restart timer) before calling `UpdateAsync`.  This
   avoids spinning up Roslyn on every character.

3. **The `AdhocWorkspace` update pattern:**
   ```csharp
   var currentSolution = workspace.CurrentSolution;
   var newSolution = currentSolution.WithDocumentText(documentId, SourceText.From(virtualDoc.Text));
   workspace.TryApplyChanges(newSolution);
   ```
   Roslyn reuses the incremental parse when the text changes, making repeated edits cheap.

4. **Thread safety:** `_documentMap` (uri → workspace + ids) is guarded by a
   `SemaphoreSlim(1,1)` for writes; reads are snapshot-safe because `workspace.CurrentSolution`
   is immutable.

5. **`#line` directives in the virtual document** (from Phase 1) cause Roslyn to report
   diagnostic file/line in the `.uitkx` coordinates already.  The `RoslynDiagnosticMapper` can
   use those directly for `Diagnostic.Location.GetMappedLineSpan()` rather than going through
   the source map for diagnostic translation.  The source map is still needed for completions
   and hover (LSP sends `.uitkx` positions that must be translated to virtual positions before
   querying Roslyn).

### 7.3 Registration in `Program.cs`

```csharp
.WithServices(services =>
{
    // existing:
    services.AddSingleton<UitkxSchema>();
    services.AddSingleton<DocumentStore>();
    services.AddSingleton<WorkspaceIndex>();
    services.AddSingleton<DiagnosticsPublisher>();

    // new:
    services.AddSingleton<ReferenceAssemblyLocator>();
    services.AddSingleton<RoslynHost>();
})
```

`ReferenceAssemblyLocator.LocateAsync` is called in `WorkspaceIndex.OnStarted` so both scans
happen in parallel on server start.

### 7.4 Wiring to `TextSyncHandler`

```csharp
// didOpen
_roslynHost.UpdateAsync(uri, text, ct).FireAndForget();

// didChange (debounced 300 ms)
_debounce.Restart(() => _roslynHost.UpdateAsync(uri, text, ct));

// didClose
_roslynHost.Remove(uri);
```

---

## 8. Phase 3 — Tier-3 Diagnostics

**Status:** 🔲 NOT STARTED  
**Files:** `lsp-server/Roslyn/RoslynDiagnosticMapper.cs`, `lsp-server/DiagnosticsPublisher.cs`  
**Dependency:** Phase 2  
**Completes:** P9 in `UITKX_IMPLEMENTATION_PLAN.md`

### 8.1 `RoslynDiagnosticMapper.cs`

```csharp
public static class RoslynDiagnosticMapper
{
    /// <summary>
    /// Converts a subset of Roslyn Diagnostic objects to ParseDiagnostic records
    /// suitable for the LSP publishDiagnostics notification.
    ///
    /// Filters:
    ///   - Hidden and Info severity: discarded (too noisy for an embedded context).
    ///   - Warning: emitted as ParseSeverity.Warning.
    ///   - Error: emitted as ParseSeverity.Error.
    ///   - Diagnostics outside mapped regions: discarded (they are in the scaffolding, not user code).
    ///   - Well-known false-positive codes suppressed in embedded context: see s_suppressedIds.
    /// </summary>
    public static IReadOnlyList<ParseDiagnostic> Map(
        IReadOnlyList<Diagnostic> roslynDiags,
        SourceMap sourceMap,
        string uitkxSource);
}
```

**Suppressed diagnostic IDs** (`s_suppressedIds`):

| Code | Reason |
|------|--------|
| `CS0246` | Type not found — expected when Unity assemblies are not yet compiled |
| `CS8019` | Unnecessary using — always present in virtual doc header |
| `CS1591` | XML doc comment missing — irrelevant in .uitkx context |
| `CS0649` | Field never assigned — props field injected as stub |
| `CS0414` | Field assigned but never read — same |
| `CS8618` | Non-nullable field not initialized — constructor not present in class fragment |

The suppressed list is configurable via the `uitkx.config.json` file
(`diagnostics.suppressedRoslynIds`).

**Diagnostic code range:**
- UITKX0201–0299: Roslyn error mapping (compilation errors)
- UITKX0301–0399: Roslyn warning mapping (style / analysis warnings)

### 8.2 `DiagnosticsPublisher` update

```csharp
public void Publish(DocumentUri uri, string text)
{
    // ... Tier 1 + 2 (existing) ...

    // Tier 3: Roslyn diagnostics (fire-and-forget; results pushed separately)
    _ = PublishRoslynDiagnosticsAsync(uri, text, existingT1T2);
}

private async Task PublishRoslynDiagnosticsAsync(DocumentUri uri, string text, Diagnostic[] t1t2)
{
    var t3 = await _roslynHost.GetDiagnosticsAsync(uri);
    var all = t1t2.Concat(t3.Select(d => ToLsp(d))).ToArray();
    _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams { Uri = uri, Diagnostics = new Container<Diagnostic>(all) });
}
```

Tier-3 diagnostics are published asynchronously (after the Roslyn workspace updates) because
they are slower than Tier-1/2.  The client receives two `publishDiagnostics` notifications: a
fast T1+T2 push on every keystroke, and a slower T1+T2+T3 push once Roslyn finishes.

---

## 9. Phase 4 — Semantic Tokens: C# Region Coverage

**Status:** 🔲 NOT STARTED  
**Files:** `language-lib/SemanticTokens/SemanticTokensProvider.cs`, `lsp-server/SemanticTokensHandler.cs`  
**Dependency:** Phase 2  
**Completes:** Item 2 (IDE-agnostic accurate coloring)

### 9.1 API addition to `SemanticTokensProvider`

```csharp
public SemanticTokenData[] GetTokens(
    ParseResult parseResult,
    string source,
    HashSet<string>? knownElements,
    // NEW:
    Func<IReadOnlyList<CSharpRegion>, IReadOnlyList<(int UitkxOffset, int Length, string TokenType)>>?
        csharpClassifier = null
)
```

When `csharpClassifier` is non-null it is called once with the full list of C# regions.  The
callback returns classified spans in uitkx-source coordinates.  The provider converts these to
line/column tokens and merges them with the UITKX structure tokens.

The callback is provided by `SemanticTokensHandler` using `RoslynHost.GetClassifiedSpansAsync`.

### 9.2 C# classification → LSP token type mapping

| Roslyn `ClassificationTypeNames` | LSP `SemanticTokenTypes` custom id | Standard fallback |
|----------------------------------|------------------------------------|-------------------|
| `className` / `structName` / `interfaceName` | `type` | `type` |
| `keyword` | `keyword` | `keyword` |
| `stringLiteral` | `string` | `string` |
| `numericLiteral` | `number` | `number` |
| `localName` / `parameterName` | `variable` | `variable` |
| `methodName` / `extensionMethodName` | `function` | `function` |
| `comment` | `comment` | `comment` |
| `propertyName` | `uitkxAttribute` (re-used) | `property` |
| `fieldName` | `variable` | `variable` |
| `operator` | (none — not emitted) | — |
| `punctuation` | (none — not emitted) | — |

Operators and punctuation are intentionally not emitted.  The TmLanguage grammar handles those
as fallback.  This avoids visual noise and reduces the number of tokens sent over the wire.

### 9.3 TmLanguage cleanup (simultaneous with Phase 4)

Once semantic tokens cover all C# regions, the TmLanguage regex patterns for C# expressions and
identifiers inside `@(expr)` and `@code` can be simplified to avoid conflict:

- Remove `expression-content` sub-patterns that colour identifiers and operators.
- Keep only the delimiter patterns (`@(`, `)`, `{`, `}`) as anchors for the grammar.
- Keep the `code-block-body` patterns for embedded markup (tags inside `@code`).

This ensures the "pre-connection flash" shows reasonable colours without fighting the LSP.
The TmLanguage diff is small (≈40 lines removed or changed).

---

## 10. Phase 5 — IntelliSense: C# Completions + Hover

**Status:** 🔲 NOT STARTED  
**Files:** `lsp-server/CompletionHandler.cs`, `lsp-server/HoverHandler.cs`  
**Dependency:** Phase 2  
**Completes:** Item 4 (IntelliSense for C# regions)

### 10.1 `CursorKind` additions

Add to `AstCursorContext.CursorKind`:

```csharp
/// <summary>Cursor is inside a @code { } body (class-member scope).</summary>
CSharpCodeBlock,

/// <summary>Cursor is inside @(expr) or attr={expr} (expression scope).</summary>
CSharpExpression,
```

`AstCursorContext.Find` is already called in `CompletionHandler` before the switch dispatch.  It
must be extended to detect these two new kinds by checking whether the cursor offset falls within
a `CodeBlockNode` or inside a `CSharpExpressionValue` attribute.

### 10.2 `CompletionHandler` routing update

```csharp
var items = ctx.Kind switch
{
    // NEW: C# regions → delegate to Roslyn
    CursorKind.CSharpCodeBlock    => await _roslynHost.GetCompletionsAsync(...),
    CursorKind.CSharpExpression   => await _roslynHost.GetCompletionsAsync(...),

    // existing UITKX completions unchanged:
    CursorKind.DirectiveName      => ...,
    CursorKind.TagName            => ...,
    CursorKind.AttributeName      => ...,
    CursorKind.AttributeValue     => ...,
    _                             => Enumerable.Empty<CompletionItem>(),
};
```

`RoslynHost.GetCompletionsAsync` translates the `.uitkx` cursor offset to the virtual document
offset via `SourceMap.ToVirtualOffset`, invokes Roslyn's `CompletionService.GetCompletionsAsync`,
converts `CompletionItem` results to LSP `CompletionItem` records, and returns.

**CompletionItem conversion notes:**
- Roslyn `CompletionItem.DisplayText` → LSP `Label`
- Roslyn `CompletionItem.Tags` → approximate LSP `CompletionItemKind` mapping
  (Method → Method, Property → Property, Field → Field, Class → Class, Keyword → Keyword, …)
- Roslyn `CompletionItem.InlineDescription` → LSP `Detail`
- `CompletionItem.GetDescriptionAsync()` (async) → LSP `Documentation` — only called during
  `completionItem/resolve` (deferred, not on initial list).

### 10.3 `HoverHandler` update

```csharp
if (ctx.Kind is CursorKind.CSharpCodeBlock or CursorKind.CSharpExpression)
{
    var hover = await _roslynHost.GetHoverAsync(uri, line1, col0, ct);
    if (hover != null)
        return new Hover { Contents = new MarkedStringsOrMarkupContent(new MarkupContent { Kind = MarkupKind.Markdown, Value = hover }) };
}
```

`RoslynHost.GetHoverAsync` calls `SemanticModel.GetSymbolInfo(node)` at the mapped virtual
position.  The symbol's XML documentation is extracted and formatted as Markdown.

---

## 11. Phase 6 — C# Formatting

**Status:** 🔲 NOT STARTED  
**Files:** `language-lib/Formatter/AstFormatter.cs`  
**Dependency:** Phase 2  
**Completes:** Item 3 (C# formatting inside .uitkx)

The `AstFormatter` is called from `FormattingHandler`.  It currently passes `@code` bodies
through with only indent adjustment.

### 11.1 Addition to `AstFormatter.Format`

```csharp
// Parameter addition (optional, null = no Roslyn)
public string Format(
    string source,
    string filePath = "",
    Func<string, Task<string?>>? roslynCodeFormatter = null)
```

When `roslynCodeFormatter` is non-null and the file has a `CodeBlockNode`, the formatter:
1. Extracts the raw `@code` body text.
2. Calls `roslynCodeFormatter(rawCodeBody)` → gets formatted text.
3. Replaces the raw body with the formatted text in its output.
4. Adjusts indentation to match the surrounding UITKX indentation level.

`FormattingHandler` provides the formatter by closing over `_roslynHost.FormatCodeBlockAsync(uri)`.

---

## 12. Phase 7 — Reference Assembly Hot-Reload

**Status:** 🔲 NOT STARTED  
**Files:** `lsp-server/WatchedFilesHandler.cs`, `lsp-server/Roslyn/ReferenceAssemblyLocator.cs`  
**Dependency:** Phase 2

Unity recompiles assemblies in `Library/ScriptAssemblies/` whenever project scripts change.
When this happens:

1. `WatchedFilesHandler` (already watches `*.cs` and `*.uitkx`) is extended to also watch
   `Library/ScriptAssemblies/*.dll`.
2. On `*.dll` change: call `ReferenceAssemblyLocator.LocateAsync` again.
3. On locator update: broadcast to all open `RoslynHost` workspaces via
   `RoslynHost.RebuildAllAsync(newReferences)`.
4. Each open workspace disposes its old workspace and creates a new one with fresh references.
5. Triggers a new `GetDiagnosticsAsync` → `DiagnosticsPublisher.Publish` cycle for all open files.

This ensures that after Unity compiles, Roslyn sees the latest `Assembly-CSharp.dll` with all
current type definitions, and errors like "type not found" resolve automatically.

---

## 13. Phase 8 — Integration Tests

**Status:** 🔲 NOT STARTED  
**New project:** `ide-extensions~/language-roslyn-tests/` (xUnit, net8.0)

### 13.1 Virtual document tests

- [ ] Given a well-formed function-style `.uitkx` component, assert virtual doc:
  - Contains the correct namespace and class name.
  - Contains the `using` directives from `@using`.
  - Contains `static object __expr_N()` wrappers for all `@(expr)` / `attr={expr}`.
  - Contains `@code` body verbatim.
- [ ] Source map round-trip: `ToVirtualOffset(ToUitkxOffset(v)) == v` for all v in a mapped region.
- [ ] Source map rejection: `ToUitkxOffset(v)` returns -1 for v in scaffold sections.
- [ ] Generated virtual doc parses without Roslyn syntax errors.

### 13.2 Roslyn host tests

- [ ] `UpdateAsync` → `GetDocumentAsync` returns a valid `Document`.
- [ ] `GetDiagnosticsAsync` for a code block with `undefined_variable` contains exactly one
  CS0246-or-similar pointing at the correct .uitkx line.
- [ ] `GetDiagnosticsAsync` for a clean code block returns no errors.
- [ ] `GetCompletionsAsync` inside `@code` body returns at least `string`, `int`, `bool` keywords.
- [ ] `GetClassifiedSpansAsync` for a code block with `string x = ""` returns a string token
  and a keyword token with correct uitkx offsets.
- [ ] `FormatCodeBlockAsync` for unformatted code returns correctly formatted code.
- [ ] `Remove` after `UpdateAsync` does not throw.

### 13.3 Diagnostics publisher tests

- [ ] When Roslyn reports an error in `@code`, `DiagnosticsPublisher.Publish` serialises it as
  a Tier-3 `ParseDiagnostic` with `Code` starting with `UITKX02`.
- [ ] Well-known false-positive `CS0649` does not appear in the published list.
- [ ] Position in published diagnostic matches the original `.uitkx` line.

---

## 14. Phase 9 — Rider + VS2022 Validation

**Status:** 🔲 NOT STARTED  
**No code changes — testing + optional polish only.**

Because the LSP server is IDE-agnostic, all features from Phases 3–6 work in any IDE that
connects an LSP client.  This phase validates that the output is correct in each IDE.

- [ ] Rider: open a `.uitkx` file, confirm Tier-3 squiggles appear, completions trigger inside
  `@code`.
- [ ] VS2022: same.  The VSIX extension already wires the LSP client
  (`ide-extensions~/visual-studio/`).
- [ ] Fix any IDE-specific LSP quirks (e.g. VS2022 has partial support for `semanticTokens/full`;
  fallback to range requests may be needed).

---

## 15. Summary Table

| Phase | Description | Files | Dependency | Status |
|-------|-------------|-------|------------|--------|
| 0 | Column tracking | `MarkupTokenizer`, `AstNode` | — | 🔲 |
| 1 | Virtual doc + SourceMap | `language-lib/Roslyn/` (all new) | Phase 0 | 🔲 |
| 2 | Roslyn host | `lsp-server/Roslyn/` (all new), `Program.cs`, `TextSyncHandler.cs` | Phase 1 + NuGet | 🔲 |
| 3 | Tier-3 diagnostics | `RoslynDiagnosticMapper.cs`, `DiagnosticsPublisher.cs` | Phase 2 | 🔲 |
| 4 | C# semantic tokens | `SemanticTokensProvider.cs`, TmLanguage cleanup | Phase 2 | 🔲 |
| 5 | C# completions + hover | `CompletionHandler.cs`, `HoverHandler.cs`, `AstCursorContext.cs` | Phase 2 | 🔲 |
| 6 | C# formatting | `AstFormatter.cs` | Phase 2 | 🔲 |
| 7 | Reference hot-reload | `WatchedFilesHandler.cs`, `ReferenceAssemblyLocator.cs` | Phase 2 | 🔲 |
| 8 | Integration tests | `language-roslyn-tests/` (new project) | Phases 1–6 | 🔲 |
| 9 | Rider + VS2022 validation | — testing — | Phase 8 | 🔲 |

**Version gates:**
- Phases 0–3 → v1.2.0 (Tier-3 diagnostics, P9 complete)
- Phases 4–6 → v1.3.0 (full IDE-agnostic language services)
- Phase 7 → v1.3.1 (hot-reload polish)

---

## 16. Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Roslyn `CompletionService` is not publicly available in `CSharp.Workspaces` 5.0 | Low | High | Verify at Phase 2 kick-off; fallback to direct `SemanticModel.LookupSymbols` for completions |
| Unity assembly DLL discovery fails on all platforms | Medium | Medium | Tier-0 fallback: BCL-only compilation still gives completions for standard types |
| AdhocWorkspace per file causes OOM with many open files | Low | Medium | Add a cap (16 open workspaces); LRU-evict least-recently-used on overflow |
| `#line` directives in virtual doc cause Roslyn to report `.uitkx` paths that the client cannot open | Low | Low | Strip `#line` directives from the virtual doc and rely solely on SourceMap for position translation |
| Debounce too short → Roslyn spins on large files | Medium | Low | Make debounce configurable via `uitkx.config.json` (`roslyn.updateDebounceMs`, default 300) |
| Virtual doc scaffold causes spurious errors that leak through the suppression list | Medium | Low | Extend `s_suppressedIds`; add `#pragma warning disable` on all scaffold lines |

---

*Last updated:* 2026-03-09 — initial creation.  
*See also:* `UITKX_ARCHITECTURE_LANGUAGE_SERVER.md`, `UITKX_IMPLEMENTATION_PLAN.md` (P9).
