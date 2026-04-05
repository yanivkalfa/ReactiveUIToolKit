# Rename Symbol (F2) — Complete Solution Plan

**Status: IMPLEMENTED** — See `RenameHandler.cs` in `lsp-server/`.

## Design Philosophy: All or Nothing

Partial rename (only the current file) is **worse than no rename** — it
silently leaves stale references in other files, creating confusion and
broken builds. This plan targets a **complete project-wide rename** from
day one: every `.uitkx` file, every companion `.cs` file, every component
tag — or the rename is refused.

---

## How Other Languages Do It

### C# (Roslyn / OmniSharp)
- Uses **`Renamer.RenameSymbolAsync(solution, symbol, newName, options)`**
- Single API call that finds ALL references across the entire `Solution`,
  handles declarations + usages + overrides + interface implementations +
  `nameof()` + XML doc comments + conflict resolution.
- Returns a new `Solution` — server diffs old vs new to produce edits.
- Works because Roslyn has **one unified workspace** with all projects.

### TypeScript (tsserver)
- Uses **`findRenameLocations()`** — returns location list, server builds edits.
- Works because TS has **one `Program` per tsconfig** with all files loaded.

### Our Challenge
We have **per-file isolated `AdhocWorkspace` instances** (by design — for
parallel editing performance). `Renamer.RenameSymbolAsync()` and
`SymbolFinder.FindReferencesAsync()` only work within a single workspace.
Roslyn symbols are **NOT identity-comparable across compilations** —
`SymbolEqualityComparer` fails across workspaces.

### Our Approach: Hybrid
- **Within each workspace**: use Roslyn's `Renamer.RenameSymbolAsync()` for
  accurate, conflict-aware rename (the gold standard).
- **Across workspaces**: match symbols by **fully-qualified signature**
  (namespace + type + member + parameter types), then run `Renamer` in
  each matching workspace independently.
- **For component tags**: use UITKX AST + `WorkspaceIndex` (not Roslyn).

---

## Architecture Overview

```
  User presses F2 on symbol
         │
    ┌────▼────┐
    │ Prepare  │  Map .uitkx offset → virtual C# → resolve ISymbol
    │ Rename   │  Validate: renameable? not metadata? not keyword?
    └────┬────┘
         │
    ┌────▼─────────────────────────────────────────────────┐
    │                    Rename Handler                     │
    │                                                       │
    │  1. Extract SymbolSignature from resolved ISymbol      │
    │                                                       │
    │  2. Classify symbol:                                   │
    │     ├─ LOCAL (var, parameter) → scan origin file only  │
    │     ├─ COMPANION (field/method in .cs) → scan all      │
    │     │   workspaces sharing that .cs + all project files │
    │     ├─ COMPONENT CLASS → C# rename + tag rename        │
    │     └─ EXTERNAL (metadata) → refuse rename             │
    │                                                       │
    │  3. For each file in scope:                            │
    │     ├─ Open files: use existing RoslynHost workspace   │
    │     └─ Disk files: read → parse → VDG → temp workspace │
    │                                                       │
    │  4. Per workspace: Renamer.RenameSymbolAsync()          │
    │     Map virtual edits → .uitkx edits via SourceMap     │
    │     Map companion edits → real .cs URIs directly        │
    │                                                       │
    │  5. Component tags: AST scan via WorkspaceIndex         │
    │                                                       │
    │  6. Deduplicate + emit single atomic WorkspaceEdit     │
    └──────────────────────────────────────────────────────┘
```

---

## Key Technical Decisions

### Why NOT a Unified Workspace

A single `AdhocWorkspace` containing all files was investigated and rejected:

| Problem | Impact |
|---------|--------|
| Per-file editing parallelism lost | All files bottleneck on single gate |
| 5–15 second startup (parse all files) | Blocks first completion/diagnostic |
| One file edit recompiles EVERYTHING | 10–50x slower diagnostics |
| 5–10x memory increase | ~200 MB → ~1–2 GB |
| Duplicate `(namespace, componentName)` across files | Roslyn merges partial classes incorrectly |

**Decision:** Keep per-file isolation. Pay the cross-workspace iteration
cost only during rename (~2–3 seconds for full project scan).

### Cross-Workspace Symbol Matching

Roslyn's `SymbolEqualityComparer` fails across compilations. We use:

```csharp
// SymbolSignature: unambiguous cross-compilation identity
record SymbolSignature(
    string Namespace,        // "Game.UI"
    string TypeName,         // "HomePanel"
    string MemberName,       // "GoldAccent"  (empty for type rename)
    string[] ParameterTypes  // ["int", "string"]  (for method overloads)
);
```

In each foreign workspace: walk syntax tree → `GetSymbolInfo()` →
extract signature → compare. This is semantically correct (not
text-matching) and handles overloads, generics, and nested types.

### Performance Budget

| Step | Per File | 67 Files (batched, 4 threads) |
|------|----------|-------------------------------|
| Disk read | 1–5 ms | — |
| Parse + VDG | 4–15 ms | — |
| Companion discovery | 5–20 ms | — |
| Workspace + metadata refs | 50–200 ms | — |
| Semantic model + Renamer | 100–500 ms | — |
| **Total per file** | **~500 ms avg** | — |
| **Full project scan** | — | **~2–3 seconds** |

Acceptable for an F2 rename operation. LSP `workDoneProgress` shows a
progress bar in the IDE during the scan.

---

## HMR (Hot Module Reload) Interaction

**Critical finding:** HMR uses a `FileSystemWatcher` on `.uitkx` and
companion `.cs` files. Rename's `WorkspaceEdit` writes files sequentially
(not atomically), which creates a dangerous race condition.

### The Problem

```
WorkspaceEdit writes File A
  └─ FileSystemWatcher fires → HMR compiles with partial state
     └─ File B not yet written → compilation error / stale code
WorkspaceEdit writes File B
  └─ FileSystemWatcher fires again → second HMR cycle
```

Additionally, HMR matches components by **class name** in two places:

1. **`IsMatch()` in `UitkxHmrDelegateSwapper.cs`** — the delegate swap:
   ```csharp
   declaringType.Name == componentName
   ```
2. **`CanReuseFiber()` in `FiberChildReconciliation.cs`** — HMR fallback
   for fiber reuse during reconciliation:
   ```csharp
   fiberType.Name == vnodeType.Name
   ```

Renaming a component class breaks both — `IsMatch()` returns false,
old delegates are never swapped, component appears frozen.

### The Fix: Match by File Path, Not Class Name

Class names change during rename, but the **file path is the stable
component identity**. The `.uitkx` file path is already embedded in
generated code via `#line` directives.

**Solution:** Add a `[UitkxSource("path/to/Component.uitkx")]` attribute
to generated component classes. HMR uses this as a fallback when
class-name matching fails.

#### How It Works

```
User renames Button → CustomButton via F2:

1. LSP returns WorkspaceEdit (changes class name in all files)
2. IDE applies edits to disk
3. HMR FileSystemWatcher fires (50ms debounce)
4. HMR compiles Button.uitkx → class CustomButton { ... }
5. SwapAll("CustomButton"):
   - Walks fiber tree → fiber has TypedRender.DeclaringType.Name = "Button"
   - "Button" == "CustomButton"? NO
   - FALLBACK: Check [UitkxSource] attribute on old fiber's type
   - Old type has [UitkxSource("Button.uitkx")]
   - New type compiled from "Button.uitkx"
   - FILE PATH MATCH ✅
6. Fiber swapped → state preserved → re-render triggered
```

#### Changes Required

| File | Change | Effort |
|------|--------|--------|
| `Runtime/` or `Shared/` | Define `UitkxSourceAttribute` (5-line attribute class) | Tiny |
| `SourceGenerator~/` | Emit `[UitkxSource("path")]` on generated component class | Small |
| `Editor/HMR/UitkxHmrDelegateSwapper.cs` | Add file-path fallback in `IsMatch()` + pass file path to `SwapAll()` | Small (~15 lines) |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add file-path fallback in HMR section of `CanReuseFiber()` | Small (~10 lines) |

#### Implementation Detail

**1. Define attribute:**
```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class UitkxSourceAttribute : Attribute
{
    public string FilePath { get; }
    public UitkxSourceAttribute(string filePath) => FilePath = filePath;
}
```

**2. Source generator emits it:**
```csharp
[UitkxSource("Assets/Samples/Button.uitkx")]
partial class Button { ... }
```

**3. `IsMatch()` fallback:**
```csharp
private static bool IsMatch(FiberNode fiber, string componentName,
                             string? uitkxFilePath)
{
    if (fiber.TypedRender == null) return false;
    var declaringType = fiber.TypedRender.Method.DeclaringType;
    if (declaringType == null) return false;

    // Primary: class name match (existing behavior, zero cost)
    if (declaringType.Name == componentName) return true;

    // Secondary: [UitkxElement] attribute (existing behavior)
    var attr = declaringType.GetCustomAttribute<UitkxElementAttribute>();
    if (attr?.ComponentName == componentName) return true;

    // Tertiary: file path match (handles renames)
    if (uitkxFilePath != null)
    {
        var srcAttr = declaringType.GetCustomAttribute<UitkxSourceAttribute>();
        if (srcAttr?.FilePath != null
            && string.Equals(
                Path.GetFullPath(srcAttr.FilePath),
                Path.GetFullPath(uitkxFilePath),
                StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}
```

#### Why This Is the Right Fix

- **No LSP ↔ HMR coordination needed** — purely runtime, no custom
  notifications, no pause/resume protocol
- **Works for ALL renames** — not just LSP-triggered; manual file renames
  in Explorer, external refactoring tools, etc.
- **Zero performance cost in happy path** — class name match hits first
  (existing behavior). File path fallback only runs when name doesn't match.
- **No temp files, no JSON mapping** — the attribute is baked into the
  generated assembly
- **Backwards compatible** — old assemblies without the attribute simply
  fall through to `return false` (existing behavior)

### Safe vs. Unsafe Scenarios (After Fix)

| Scenario | HMR Risk | Notes |
|----------|----------|-------|
| Rename local variable | **None** | No file boundary crossed, no class name change |
| Rename field in companion `.cs` | **None** | HMR recompiles, class name unchanged, name match works |
| Rename symbol across multiple files | **None** | HMR 50ms debounce fires after all writes; normal recompile |
| Rename component class name | **None** (with fix) | File-path fallback matches correctly |
| Rename component class name (without fix) | **High** | Component frozen — old behavior |
| HMR is stopped/inactive | **None** | All renames safe when HMR is off |

### Multi-File Write Race (The 50ms Window)

HMR uses a `FileSystemWatcher` with 50ms debounce. When rename writes
3 files sequentially, each write resets the debounce timer. After the
last write, HMR waits 50ms then compiles.

**Is 50ms enough?** Yes — IDE applies WorkspaceEdit edits within ~10ms
total (in-process buffer modifications + disk flush). The 50ms debounce
fires well after all files are written. This was verified by examining
the event timeline:

```
T=0ms    IDE applies File A → FSW event → debounce starts (50ms)
T=2ms    IDE applies File B → FSW event → debounce RESETS (50ms from now)
T=4ms    IDE applies File C → FSW event → debounce RESETS (50ms from now)
T=54ms   Debounce fires → HMR reads all 3 files from disk → all updated ✅
```

**No partial-state compilation risk** — HMR always sees the
fully-renamed state.

---

## Implementation — Complete Rename Handler

**Status:** `not-started`  
**Started:** —  
**Completed:** —  
**Tested:** —

### Scope (All-or-Nothing)

When the user presses F2, ALL of the following happen in one atomic
`WorkspaceEdit` — or none of them:

1. Rename in the **originating `.uitkx`** file (inline expressions, code blocks)
2. Rename in **companion `.cs`** files (declaration + usages)
3. Rename in **all other open `.uitkx`** files that reference the symbol
4. Rename in **all unopened `.uitkx`** files on disk that reference the symbol
5. If the symbol is a component class: rename **all `<Tag>` references** in
   `.uitkx` ASTs across the project + rename the `*Props` class

### Files to Create / Modify

| File | Change |
|------|--------|
| `lsp-server/RenameHandler.cs` | **New** — ~400–500 LOC, full implementation |
| `lsp-server/Program.cs` | +1 line: `.WithHandler<RenameHandler>()` |
| `lsp-server/CapabilityPatchStream.cs` | +1 line: `"renameProvider": true` |
| `lsp-server/Roslyn/RoslynHost.cs` | +1 method: `GetAllTrackedPaths()` — returns open file paths |
| `language-lib/WorkspaceIndex.cs` | +1 method: `GetAllUITKXPaths()` — returns all discovered `.uitkx` paths |

### Step-by-Step Implementation

#### 1. `PrepareRename` (validation gate)

```
Input:  textDocument URI + cursor position
Output: range + placeholder text  OR  error

Flow:
  1. Get VirtualDocument for the URI
  2. Map .uitkx offset → virtual C# offset (SourceMap.ToVirtualOffset)
  3. If offset not in C# region → check if on a component tag name
     → if tag: return tag span
     → else: return error "Not a renameable symbol"
  4. Get syntax root + semantic model from RoslynHost
  5. FindToken(virtualOffset) → walk to meaningful parent node
  6. GetSymbolInfo / GetDeclaredSymbol → resolve ISymbol
  7. If symbol is null → error
  8. If all locations are metadata → error "Cannot rename external symbol"
  9. Map token span back to .uitkx coordinates → return range + name
```

#### 2. `Rename` (compute all edits)

```
Input:  textDocument URI + cursor position + newName
Output: WorkspaceEdit with ALL edits across ALL files

Flow:
  1. Resolve ISymbol (same as PrepareRename step 1–6)
  2. Extract SymbolSignature for cross-workspace matching
  3. Classify symbol scope:

     LOCAL (symbol defined in virtual doc, not a type member):
       → Run Renamer.RenameSymbolAsync in origin workspace only
       → Map virtual edits → .uitkx edits via SourceMap
       → Done (no cross-file work needed)

     COMPANION (symbol defined in a .cs companion file):
       → Run Renamer in origin workspace (gets .uitkx + companion edits)
       → Enumerate all open workspaces (RoslynHost.GetAllTrackedPaths)
       → For each that shares the companion .cs:
           - Match symbol by SymbolSignature in their semantic model
           - Run Renamer in their workspace
           - Map virtual edits → .uitkx edits
       → Enumerate all project .uitkx files (WorkspaceIndex.GetAllUITKXPaths)
       → For each unopened file:
           - Read from disk → Parse → VDG → temp workspace
           - Match symbol by SymbolSignature
           - If found: run Renamer, map edits, dispose workspace
       → Deduplicate companion .cs edits (same file appears in multiple workspaces)

     COMPONENT CLASS (symbol is a class deriving from component base):
       → All of COMPANION flow above (for C# references)
       → PLUS: scan ALL .uitkx files for <OldName> / <OldName /> tags
       → Use WorkspaceIndex to find which files use this element name
       → Parse each file's AST, find tag name spans, emit text edits
       → If *Props.cs exists: include Props class rename

     EXTERNAL (from referenced DLL):
       → Return error "Cannot rename external symbol"

  4. Deduplicate all edits by (URI, startOffset, endOffset)
  5. Return single WorkspaceEdit using documentChanges (ordered, transactional)
```

#### 3. Cross-Workspace Symbol Matching (detail)

```
Given: SymbolSignature from origin workspace

For each foreign workspace:
  1. Acquire FileState.Gate semaphore (prevent concurrent rebuild)
  2. Get semantic model + syntax root
  3. Walk all IdentifierNameSyntax nodes in the syntax tree
  4. For each: call semanticModel.GetSymbolInfo(node)
  5. Extract SymbolSignature from resolved symbol
  6. If signatures match → this node references the target symbol
  7. Use Renamer.RenameSymbolAsync with the foreign workspace's own
     ISymbol (resolved in step 4) — this avoids cross-compilation issues
  8. Release gate
```

**Key insight:** We don't pass the origin ISymbol to foreign workspaces.
We find the *equivalent* symbol in each foreign workspace by signature
match, then call `Renamer` with that workspace's own symbol. This
sidesteps the cross-compilation identity problem entirely.

#### 4. Unopened File Processing

```
For each .uitkx path from WorkspaceIndex not in DocumentStore:
  1. Read source from disk
  2. Parse (DirectiveParser + UitkxParser)
  3. Generate VirtualDocument + SourceMap via VDG
  4. Create temporary AdhocWorkspace:
     - Reuse cached MetadataReference[] (already shared via ReferenceAssemblyLocator)
     - Add virtual doc as Document
     - Discover + add companion .cs files from same directory
  5. Get semantic model
  6. Walk syntax → signature match → if found: Renamer in temp workspace
  7. Map edits back to .uitkx via SourceMap
  8. Dispose temp workspace immediately
  9. Process in batches of 10, parallel (4 threads)
```

---

## Risk Assessment

### Correctness Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| SourceMap offset produces wrong .uitkx position | Low | **Critical** — edits corrupt file | Validate every mapped offset falls within a known `SourceMapEntry` region. If any offset fails validation, refuse the entire rename. |
| Cross-workspace signature match has false positive | Very Low | **Critical** — wrong symbol renamed | Signature includes namespace + type + member + parameter types. Verify by checking `symbol.ContainingAssembly` matches (same compilation unit or same DLL identity). |
| Rename misses a reference (incomplete) | Medium | **High** — broken build, stale names | Run a post-rename verification pass: re-parse renamed files, check for diagnostics mentioning old name. If found, warn user. |
| `Renamer.RenameSymbolAsync` introduces qualification edits | Low | Medium — unexpected `Namespace.NewName` insertions | These are correct — Roslyn adds qualifiers to prevent conflicts. Let them through. |
| Symbol resolves to metadata | Medium | Low | `PrepareRename` rejects. Clear error message. |

### Performance Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Full project scan takes >5 seconds | Medium | Medium — feels slow | LSP `workDoneProgress` shows progress bar. Batch + parallel processing. Early exit for locals. |
| Memory spike from 67 temp workspaces | Medium | Medium — GC pressure | Batch of 10, dispose between batches. `MetadataReference[]` already cached/shared. |
| Rename blocks other LSP requests | Medium | Medium — IDE appears frozen | Run rename on background thread. Other handlers continue on their own `FileState.Gate`. |

### HMR Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Partial file writes trigger HMR mid-rename | Low | **Low** — HMR 50ms debounce fires after all IDE writes complete (~4ms total) | No mitigation needed — debounce is sufficient. |
| Component class rename breaks HMR delegate matching | **Eliminated** | — | `[UitkxSource]` file-path fallback in `IsMatch()` handles this. See HMR section above. |
| Companion .cs write triggers unnecessary HMR cycle | Low | Low — extra compile, no data loss | Normal HMR behavior. Single debounced recompile. |

### Architecture Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Exposing RoslynHost internals | Low | Low | Expose `IReadOnlyCollection<string>` only, not `FileState`. |
| VDG standalone invocation has edge cases | Low | Medium | Already proven by `DiagnosticsPublisher`. Same code path. |
| WorkspaceIndex stale (files added/removed since scan) | Low | Low — misses new file | `WorkspaceIndex.Refresh()` before rename scan. Already called on `workspace/didChangeWatchedFiles`. |
| Race: file edited between scan start and edit application | Low | Medium | Use `DocumentStore` version for open files. For disk files, check modification time. |

---

---

## Deep Impact Analysis

### What This Handler Touches (and What It Doesn't)

The rename handler is **additive** — it creates one new file and adds
small additions to existing files. It does NOT modify any existing handler
logic, diagnostics pipeline, or workspace management.

#### Existing APIs — NOT Modified

| API / System | Impact | Why |
|---|---|---|
| `TextSyncHandler` | **None** | Rename handler is a separate handler; doesn't intercept didChange/didOpen/didClose |
| `CompletionHandler` | **None** | Separate handler, separate DI, no shared mutable state |
| `HoverHandler` | **None** | Same — read-only Roslyn queries, no mutex contention |
| `DefinitionHandler` | **None** | Uses WorkspaceIndex only; rename doesn't modify index |
| `SemanticTokensHandler` | **None** | Read-only; consumes VirtualDoc + SourceMap that rename doesn't touch |
| `DiagnosticsPublisher` | **None** | Rename doesn't call Publish(). After rename, normal didChange flow handles diagnostics |
| `FormattingHandler` | **None** | Independent |
| `SignatureHelpHandler` | **None** | Independent |
| `DocumentStore` | **Read-only** | Rename reads via `TryGet()` / `GetAll()`. Never writes. |
| `WorkspaceIndex` | **Read-only** | Rename reads element names. Adds `GetAllUITKXPaths()` (new public method, no existing behavior changed) |
| `SourceMap` | **Read-only** | Rename calls `ToVirtualOffset()` and `ToUitkxOffset()`. No mutations. |

#### Existing APIs — Minor Additions

| API | Change | Risk |
|---|---|---|
| `RoslynHost` | +1 public method `GetAllTrackedPaths()` → returns `IReadOnlyCollection<string>` of tracked .uitkx file paths | **Zero risk** — pure read from `_files.Keys`. No lock needed (ConcurrentDictionary.Keys is snapshot). |
| `RoslynHost` | +1 public method `GetCompanionPath(string uitkxPath, DocumentId docId)` → returns real .cs file path for a companion document | **Zero risk** — adds a `Dictionary<DocumentId, string>` to FileState populated during `AddCompanionDocuments`. |
| `WorkspaceIndex` | +1 public method `GetAllUITKXPaths()` → returns all discovered .uitkx file paths | **Zero risk** — reads existing internal collection. |

### SourceMap: The Critical Constraint

The SourceMap guarantees **equal-length mapping**:
```
VirtualEnd - VirtualStart == UitkxEnd - UitkxStart  (always)
```

This is preserved during rename because:

1. We call `Renamer.RenameSymbolAsync()` on the **old** Solution
2. Renamer returns a **new** Solution with modified text
3. We diff old vs new documents using `oldDoc.GetTextChangesAsync(newDoc)`
4. Each `TextChange` has a `Span` in the **old** virtual document coordinates
5. We map `change.Span.Start` back to .uitkx using the **old** SourceMap
6. The old SourceMap is still valid for the old document — no drift

**The key insight:** We never map positions in the *new* virtual document.
We only use old-document coordinates, which the SourceMap was built for.

The `TextChange.Span.Start` lands within a mapped region (because Renamer
only touches identifier tokens that exist in the original code). We map
that start position to .uitkx, then emit a `TextEdit` replacing from that
position for `oldName.Length` characters with `newName`.

**Edge cases handled:**

| Scenario | What Happens | Safe? |
|---|---|---|
| Simple rename: `myVar` → `newVar` | Span.Start maps cleanly, old name length matches span | ✅ |
| Renamer adds qualification: `myVar` → `Namespace.NewVar` | Span covers only `myVar`. We emit `TextEdit(range=myVar, newText=Namespace.NewVar)` — IDE handles variable-length replacement | ✅ |
| Renamer touches scaffold (unmapped code) | `ToUitkxOffset()` returns `null` → we skip this change (it's in generated code, not user code) | ✅ |
| Multiple references in same `@code` block | Each gets its own TextChange with its own Span.Start | ✅ |
| Reference in `attr={expr}` inline expression | Span.Start maps to the inline expr region via SourceMap | ✅ |

### HMR Interaction: Fully Solved

#### What Happens During Rename (HMR Active)

```
T=0ms    LSP returns WorkspaceEdit (3 files)
T=1ms    IDE applies edit to File A (open buffer)
           → textDocument/didChange → TextSyncHandler → DiagnosticsPublisher.Publish()
           → T1+T2 diagnostics pushed immediately
           → RoslynHost.EnqueueRebuild(A) → 300ms timer starts
           → IDE writes to disk → FileSystemWatcher fires → HMR debounce (50ms) starts
T=2ms    IDE applies edit to File B
           → same cascade as above
           → HMR debounce timer RESETS (50ms from now)
T=3ms    IDE applies edit to File C
           → same cascade
           → HMR debounce timer RESETS again
T=53ms   HMR debounce fires (50ms after last FSW event)
           → HMR reads all 3 files from disk (all already written ✅)
           → Compiles → generates assembly → swaps delegates
           → Class name changed? → name match fails → file-path fallback matches ✅
T=303ms  RoslynHost timers fire for A, B, C → 3 parallel Roslyn rebuilds
T=600ms  T3 diagnostics pushed for all 3 files → errors clear in editor
```

#### All Scenarios Are HMR-Safe (With Fix)

| Rename Type | HMR Risk | Explanation |
|---|---|---|
| **Local variable** (`var x = ...`) | **None** | Single file, no class name change, name match works |
| **Field in companion .cs** | **None** | Class name unchanged, name match works |
| **Symbol across multiple .uitkx** | **None** | Class names unchanged, normal HMR recompile |
| **Component class rename** | **None** (with `[UitkxSource]` fix) | File-path fallback in `IsMatch()` handles class name change |
| **Component class rename** (without fix) | **High** — component frozen | Old behavior — must not ship rename without the HMR fix |

**The `[UitkxSource]` fix is a prerequisite for shipping rename.** It's
small (~30 lines across 4 files) and benefits HMR independently of rename
(e.g., manual renames in file explorer also work).

### Diagnostics Flash During Rename

When rename modifies multiple open files, the IDE sends `didChange` for
each. This triggers T1+T2 diagnostics immediately and T3 (Roslyn) after
300ms debounce per file.

**User sees:** Brief cascade of diagnostic updates over ~300–600ms as
each file's Roslyn rebuild completes and errors resolve one by one.

**Is this harmful?** No — it's cosmetic only. The errors clear quickly.
Same behavior occurs when the user manually edits multiple files rapidly.

**Can we mitigate?** The `DiagnosticsPublisher` already carries forward
the last T3 diagnostics during the debounce gap, so the error list doesn't
flash empty. It updates incrementally as each file resolves. This is
acceptable behavior — no changes needed.

### Thread Safety

The rename handler follows the same pattern as all other handlers:

1. Acquires no global locks — uses per-file `FileState.Gate` via
   `EnsureReadyAsync()` (which already serializes with `RebuildAsync`)
2. Reads `DocumentStore` via `TryGet()` — concurrent-safe
3. Reads `WorkspaceIndex` via existing public methods — concurrent-safe
4. Creates temporary workspaces for unopened files — fully isolated,
   disposed after use, no shared state
5. Multiple rename requests cannot overlap because the IDE serializes
   rename (user must confirm the dialog before another rename starts)

### VS 2022 Specific: Buffer Sync

The `UitkxMiddleLayer.cs` has a `NeedsBufferSync()` check that ensures
the editor buffer is synced before certain requests. Currently covers:
`definition`, `formatting`, `hover`, `completion`.

**Must add:** `textDocument/rename` and `textDocument/prepareRename` to
ensure the rename handler sees the latest buffer content.

Without this, VS 2022 could send a rename request with a stale buffer,
causing the offset mapping to be wrong.

---

## Summary

| Aspect | Decision |
|--------|----------|
| **Scope** | All-or-nothing: full project rename or refuse |
| **Workspace strategy** | Keep per-file isolation; iterate for rename |
| **Cross-workspace matching** | SymbolSignature (fully-qualified, type-safe) |
| **Within-workspace rename** | `Renamer.RenameSymbolAsync()` (gold standard) |
| **Unopened files** | Disk read → VDG → temp workspace → dispose |
| **Component tags** | UITKX AST scan via WorkspaceIndex |
| **HMR** | Fully solved via `[UitkxSource]` file-path fallback — all rename types safe |
| **Diagnostics flash** | Acceptable — clears within 600ms, same as manual multi-file edit |
| **Existing APIs** | **Zero impact** — rename is purely additive |
| **Performance target** | <3 seconds for full project (67 files, batched) |
| **New files** | 2 (`RenameHandler.cs`, `UitkxSourceAttribute.cs`) |
| **Modified files** | 7 (see table below) |
| **Lines of code** | ~400–500 new (rename) + ~30 new (HMR fix) + ~20 modified |

### Files Affected

| File | Change | Risk |
|------|--------|------|
| `lsp-server/RenameHandler.cs` | **New file** — full rename handler (~400–500 LOC) | N/A — new code |
| `lsp-server/Program.cs` | +1 line: `.WithHandler<RenameHandler>()` | **None** — additive registration |
| `lsp-server/CapabilityPatchStream.cs` | +1 line: `"renameProvider":{...}` in providers string | **None** — additive capability |
| `lsp-server/Roslyn/RoslynHost.cs` | +2 methods: `GetAllTrackedPaths()`, `GetCompanionPath()` | **None** — read-only additions |
| `language-lib/WorkspaceIndex.cs` | +1 method: `GetAllUITKXPaths()` | **None** — read-only addition |
| `visual-studio/UitkxVsix/UitkxMiddleLayer.cs` | +2 entries in `NeedsBufferSync()` | **None** — additive check |
| `Runtime/` or `Shared/` | **New file** `UitkxSourceAttribute.cs` (~5 LOC) | **None** — new attribute class |
| `SourceGenerator~/` | Emit `[UitkxSource("path")]` on generated classes | **Low** — adds attribute to generated output |
| `Editor/HMR/UitkxHmrDelegateSwapper.cs` | Add file-path fallback in `IsMatch()` (~15 lines) | **Low** — fallback only runs when name match fails |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add file-path fallback in HMR `CanReuseFiber()` (~10 lines) | **Low** — `#if UNITY_EDITOR` guarded, fallback only |
| `language-lib/SourceMap.cs` | No changes | — |
