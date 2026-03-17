# UITKX — Companion Files in Roslyn Workspace

**Status:** `[DONE]`

> **Problem:** The embedded Roslyn workspace in the UITKX language server creates
> a single-document `AdhocWorkspace` per open `.uitkx` file. Companion `.cs` files
> (e.g., `.style.cs`, `.util.cs`) that define members in the **same partial class**
> are never loaded. This causes false CS0103 errors ("The name 'Styles' does not
> exist in the current context") whenever the component references those members.
>
> **Additionally:** The virtual document generator appends a `Func` suffix to the
> class name (`partial class TableViewFunc`), but companion files use the real
> component name (`partial class TableView`). These are **different classes** and
> can never merge as partial types — so simply adding companions as source
> documents would not work without also fixing the class name.

---

## Table of Contents

1. [Root Cause Analysis](#1-root-cause-analysis)
2. [Design Constraints](#2-design-constraints)
3. [Chosen Approach — Directory-Level Source Injection](#3-chosen-approach--directory-level-source-injection)
4. [Implementation Plan](#4-implementation-plan)
5. [File Changes Summary](#5-file-changes-summary)
6. [Edge Cases](#6-edge-cases)
7. [Testing](#7-testing)

---

## 1. Root Cause Analysis

### Two Problems, Not One

**Problem 1 — Missing companion files:**
The `AdhocWorkspace` contains only the virtual document. Companion `.cs` files
(`.style.cs`, `.util.cs`, `.types.cs`) that define `Styles`, utility methods, and
nested types in the same partial class are never loaded.

**Problem 2 — Class name mismatch (`Func` suffix):**
`VirtualDocumentGenerator` (line 264) appends `"Func"` to the component name:
```csharp
string className = (d.ComponentName ?? "Component") + "Func";
// component TableView → partial class TableViewFunc
```

But companion files declare `partial class TableView` — the **real** component
name, matching what the Unity source generator emits (`CSharpEmitter.cs` line 140
uses `_directives.ComponentName` directly, no suffix).

Even if companions were loaded as source documents, `partial class TableView` and
`partial class TableViewFunc` are **different types** — Roslyn would never merge
them. The `Func` suffix was originally added to avoid CS0436 conflicts with
`Assembly-CSharp.dll`, but it prevents partial-class merging.

### Companion File Patterns (Real-World Data)

Investigating JustStayOn reveals **two companion patterns**:

**Pattern A — Partial class (same name as component, NO `Func` suffix):**
```
TableView.styles.cs      → public partial class TableView   { static class Styles { ... } }
TableView.types.cs       → public partial class TableView   { class Column { ... } }
TableView.util.cs        → public partial class TableView   { BuildDefaultCell(...) }
NeonProgressBar.style.cs → public partial class NeonProgressBar { private static class Styles { ... } }
```

**Pattern B — Standalone classes (not partial):**
```
JSOAppButton.style.cs    → public static class JSOAppButtonStyles { ... }
JSOAppButton.util.cs     → public static class JSOAppButtonUtils { ... }
Sidebar.style.cs         → public static class SidebarStyles { ... }
SidebarItem.cs           → public sealed class SidebarItem { ... }
```

### Why the `Func` Suffix Breaks Everything

The **real** Unity source generator (`CSharpEmitter.cs` line 140) emits:
```csharp
public partial class TableView     // ← uses ComponentName as-is, NO Func
```

But the **LSP** virtual document generator (line 264) emits:
```csharp
partial class TableViewFunc        // ← appends "Func"
```

This means:
- Companion files declare `partial class TableView`
- Virtual doc declares `partial class TableViewFunc`
- **These are different types — they can never merge as partial classes!**
- Even loading companions as source won't help: `Styles` is a nested type of
  `TableView`, not `TableViewFunc`

### Why Assembly-CSharp.dll Doesn't Help

`Assembly-CSharp.dll` contains the complete `TableView` type (companions merged).
But the virtual document declares `partial class TableViewFunc` which is a
**different type** — it doesn't inherit from or relate to the DLL's `TableView`.
`Styles` lives inside `TableView`, and there's no path from `TableViewFunc` to it.

Even if the virtual doc used the same name (`TableView`), source declarations
**shadow** metadata-reference types entirely. The source `TableView` would be
just the virtual doc alone — incomplete, missing `Styles`. CS0436 warns about this.

The fix requires BOTH:
1. Removing the `Func` suffix (so virtual doc and companions share the class name)
2. Loading companions as source (so Roslyn merges all partial parts)

---

## 2. Design Constraints

| Constraint | Detail |
|------------|--------|
| **`Func` suffix mismatch** | Virtual doc uses `{Name}Func`, companions and Unity source gen use `{Name}`. Must align. |
| **CS0436 conflict** | Once we use the real class name, source `TableView` shadows DLL `TableView`. CS0436 warning — must be suppressed at compilation level. |
| **Performance** | Each `.uitkx` gets its own `AdhocWorkspace`. Loading the entire directory (typically 1–5 files, < 50 KB) adds negligible overhead. |
| **Diagnostic isolation** | `SemanticModel.GetDiagnostics()` returns per-syntax-tree diagnostics. Companion file errors stay on their own documents. But CS0436 can appear on the virtual doc's type declaration — must suppress. |
| **Workspace root** | `_workspaceRoot` is the Unity project root (from LSP `Initialize`). |
| **Existing watchers** | `WatchedFilesHandler` watches `**/*.cs` for `WorkspaceIndex`. `_dllWatcher` watches `Library/ScriptAssemblies/*.dll`. Neither feeds the Roslyn workspace. |

---

## 3. Chosen Approach — Directory-Level Source Injection

### Strategy (Two-Part Fix)

1. **Remove the `Func` suffix** from `VirtualDocumentGenerator` so the virtual
   document class name matches the real source generator and companion files.
2. **Load all `.cs` files from the same directory** as the `.uitkx` file into the
   `AdhocWorkspace` as additional source documents.

### Why This Works

After both changes, the Roslyn project contains:

```
Source documents:
  1. Virtual doc:         partial class TableView { /* scaffold + user code body */ }
  2. TableView.styles.cs: partial class TableView { static class Styles { ... } }
  3. TableView.types.cs:  partial class TableView { class Column { ... } }
  4. TableView.util.cs:   partial class TableView { BuildDefaultCell(...) }

Metadata references:
  - Assembly-CSharp.dll  (also has TableView — shadowed by source, CS0436 suppressed)
  - UnityEngine.dll, System.dll, etc.
```

Roslyn merges all source `partial class TableView` parts → the unified type has:
- The virtual doc's scaffold (state hooks, expression checks)
- The user's code body (references `Styles.Container`, `new Column()`)
- Companion members (`Styles`, `Column`, `BuildDefaultCell`)

Type resolution:
- `Styles.Container` → nested type of merged `TableView` → **resolved** ✅
- `new Column()` → nested type from companion → **resolved** ✅
- `JSOAppButtonUtils.ResolveStyle(...)` → standalone class, present in both
  source and DLL → source wins, CS0436 suppressed → **resolved** ✅
- `UnityEngine.Color` → only in DLL → **resolved** ✅
- Types from other folders → only in `Assembly-CSharp.dll` → **resolved** ✅

### Why All `.cs` in the Directory (Not Just Pattern-Matched)

Some companion files don't follow `{ComponentName}.*.cs` naming. Examples:
- `SidebarItem.cs` — a type used by `Sidebar.uitkx` props
- Standalone static helper classes (`JSOAppButtonStyles`)

Loading **all** `.cs` files from the directory captures everything the component
needs without relying on naming conventions. Typical component directories have
1–5 `.cs` files totaling < 50 KB — negligible cost.

### CS0436 Handling

When source declares `partial class TableView` and `Assembly-CSharp.dll` also
has `TableView`, CS0436 fires. Source always wins.

**Fix**: Suppress CS0436 at the **compilation level** in `RoslynHost.s_compilationOptions`:
```csharp
["CS0436"] = ReportDiagnostic.Suppress
```

This is safe because:
- CS0436 is a warning (not an error)
- It's always caused by our directory injection — not a user mistake
- Suppressing at compilation level prevents it from appearing on any document

---

## 4. Implementation Plan

### Step 1 — Remove `Func` Suffix from Virtual Document

**File:** `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` (line 264)

```diff
- string className = (!string.IsNullOrEmpty(d.ComponentName) ? d.ComponentName! : "Component") + "Func";
+ string className = !string.IsNullOrEmpty(d.ComponentName) ? d.ComponentName! : "Component";
```

This aligns the virtual document class name with:
- The real Unity source generator (`CSharpEmitter.cs` line 140)
- Companion partial-class declarations
- No downstream code depends on the `Func` suffix (verified by grep)

### Step 2 — Suppress CS0436 at Compilation Level

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` (in `s_compilationOptions`)

Add to the `WithSpecificDiagnosticOptions` dictionary:
```csharp
["CS0436"] = ReportDiagnostic.Suppress, // source type shadows imported type (companion injection)
```

Also add to `s_suppressedIds` in `RoslynDiagnosticMapper.cs` as a second-level
safety net (in case a CS0436 leaks through from `#line`-mapped spans):
```csharp
"CS0436", // Type conflicts with imported type (companion vs DLL)
```

### Step 3 — Extend `FileState` to Track Companion Documents

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`

```csharp
private sealed class FileState : IDisposable
{
    // ... existing fields ...
    public DocumentId?      DocumentId;
    public List<DocumentId> CompanionDocIds = new List<DocumentId>();  // NEW
    // ... rest unchanged ...
}
```

### Step 4 — Add Directory-Level Companion Loading to `UpdateWorkspace`

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`

Add a helper method:
```csharp
/// <summary>
/// Returns all .cs files in the same directory as the .uitkx file.
/// These are loaded as additional source documents so that partial-class
/// members (Styles, utils, types) are visible to Roslyn's semantic analysis.
/// </summary>
private static IReadOnlyList<string> FindCompanionFiles(string uitkxFilePath)
{
    var dir = System.IO.Path.GetDirectoryName(uitkxFilePath);
    if (dir == null || !System.IO.Directory.Exists(dir))
        return Array.Empty<string>();

    var result = new List<string>();
    foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.cs"))
        result.Add(file);
    return result;
}
```

**First-open branch** — after `var doc = ws.AddDocument(docInfo);`:
```csharp
var companions = FindCompanionFiles(uitkxFilePath);
state.CompanionDocIds.Clear();
foreach (var companionPath in companions)
{
    try
    {
        var companionText = System.IO.File.ReadAllText(companionPath);
        var companionDocInfo = DocumentInfo.Create(
            id:     DocumentId.CreateNewId(project.Id, debugName: companionPath),
            name:   System.IO.Path.GetFileName(companionPath),
            sourceCodeKind: SourceCodeKind.Regular,
            loader: TextLoader.From(
                TextAndVersion.Create(
                    Microsoft.CodeAnalysis.Text.SourceText.From(companionText),
                    VersionStamp.Create(),
                    companionPath)));
        var companionDoc = ws.AddDocument(companionDocInfo);
        state.CompanionDocIds.Add(companionDoc.Id);
    }
    catch (Exception ex)
    {
        ServerLog.Log($"[RoslynHost] Could not load companion {companionPath}: {ex.Message}");
    }
}
```

**Update branch** — after `WithDocumentText` + `WithProjectMetadataReferences`:
```csharp
// Remove old companions, re-add with fresh content
foreach (var oldId in state.CompanionDocIds)
    newSolution = newSolution.RemoveDocument(oldId);
state.CompanionDocIds.Clear();

var companions = FindCompanionFiles(uitkxFilePath);
foreach (var companionPath in companions)
{
    try
    {
        var companionText = System.IO.File.ReadAllText(companionPath);
        var newDocId = DocumentId.CreateNewId(state.ProjectId!, debugName: companionPath);
        var companionDocInfo = DocumentInfo.Create(
            id:     newDocId,
            name:   System.IO.Path.GetFileName(companionPath),
            sourceCodeKind: SourceCodeKind.Regular,
            loader: TextLoader.From(
                TextAndVersion.Create(
                    Microsoft.CodeAnalysis.Text.SourceText.From(companionText),
                    VersionStamp.Create(),
                    companionPath)));
        newSolution = newSolution.AddDocument(companionDocInfo);
        state.CompanionDocIds.Add(newDocId);
    }
    catch { /* file may have been deleted between discovery and read */ }
}

state.Workspace.TryApplyChanges(newSolution);
```

### Step 5 — Verify Diagnostic Isolation (No Code Change Needed)

`GetLatestDiagnostics` already retrieves diagnostics only from `state.DocumentId`
(the virtual doc). `SemanticModel.GetDiagnostics()` returns diagnostics scoped to
that document's syntax tree. Companion file errors stay on their own documents.

The only cross-document diagnostic is CS0436, which is suppressed at compilation
level (Step 2).

---

## 5. File Changes Summary

| File | Change | Scope |
|------|--------|-------|
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Remove `+ "Func"` from class name (line 264) | 1 line |
| `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` | Add `CS0436` to `s_compilationOptions`; add `CompanionDocIds` to `FileState`; add `FindCompanionFiles`; modify `UpdateWorkspace` to load/refresh companions | ~50 lines |
| `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs` | Add `"CS0436"` to `s_suppressedIds` (safety net) | 1 line |

**Total:** ~52 lines of changes across 3 files.

---

## 6. Edge Cases

| Case | Handling |
|------|----------|
| **No `.cs` files in directory** | `FindCompanionFiles` returns empty list; workspace has only the virtual doc — identical to current behaviour |
| **Companion has syntax errors** | Roslyn compiles partially; broken companion won't crash the workspace. Its diagnostics stay on its own document, not the virtual doc |
| **Companion references unavailable types** | Most types resolve from DLLs. If a companion uses an Editor-only type, that companion's error stays isolated. The virtual doc's `Styles` still resolves if the companion defining it compiles |
| **Non-component `.cs` files in directory** | Loaded as source. Their types shadow DLL equivalents (CS0436 suppressed). Harmless — source version is identical to DLL version |
| **Multiple `.uitkx` files in same directory** | Each gets its own workspace with the same companions. Works correctly — Roslyn handles this |
| **Companion file changes** | Picked up on next `.uitkx` rebuild (debounced). Phase 2 enhancement could add proactive rebuild on `.cs` file change via `WatchedFilesHandler` |
| **Directory has many `.cs` files** | Uncommon for component dirs (typically 1–5 files). Even 20+ files would add negligible compile time |
| **`Assembly-CSharp.dll` not compiled yet** | No DLL → no shadow conflict. Companions provide partial members, other types unresolved (same as today) |
| **Namespace mismatch** | If companion uses different namespace than `@namespace` directive, partial classes won't merge. This is a real bug (same at Unity compile time) — correctly surfaces as CS0103 |

---

## 7. Testing

### Manual Tests

1. **Baseline**: Open `NeonProgressBar.uitkx` → verify `Styles.Container` shows
   CS0103 red squiggle (current behaviour)
2. **After fix**: Same file → CS0103 is **gone**
3. **Standalone helper**: Open `JSOAppButton.uitkx` → verify
   `JSOAppButtonUtils.ResolveStyle(...)` resolves without error
4. **Nested types**: Open `TableView.uitkx` → verify `TableView.Column` resolves
5. **No companions**: Open a `.uitkx` file in an empty directory → identical
   behaviour to current
6. **Companion with errors**: Break syntax in `.style.cs` → verify `.uitkx` editor
   doesn't show companion's errors (only `Styles` reference may fail)
7. **CS0436 suppressed**: Check no "type conflicts with imported type" warnings
   appear in the `.uitkx` editor

### Phase 2: Companion File Change Detection (Optional)

When the user edits a companion `.cs` file, the Roslyn workspace should refresh.
Currently, this happens naturally when the user switches back to the `.uitkx` file
and triggers a rebuild. If users report stale diagnostics, add proactive rebuild:

- `WatchedFilesHandler` already watches `**/*.cs`
- Extend it to call `RoslynHost.OnCompanionFileChanged(path)`
- That method finds which open `.uitkx` workspaces include that directory
  and triggers a debounced rebuild

---

## Appendix: Key Code Locations

| Component | Path | Key Lines |
|-----------|------|-----------|
| Virtual doc generator | `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | L264: `+ "Func"` (to remove) |
| Roslyn workspace manager | `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` | L56–82: `s_compilationOptions`; L88–108: `FileState`; L394–447: `UpdateWorkspace` |
| Diagnostic mapper | `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs` | L43–62: `s_suppressedIds` |
| Reference locator | `ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs` | Unchanged |
| File watcher | `ide-extensions~/lsp-server/WatchedFilesHandler.cs` | Phase 2 only |
| Real source generator | `SourceGenerator~/Emitter/CSharpEmitter.cs` | L140: `partial class {ComponentName}` (no Func) |
