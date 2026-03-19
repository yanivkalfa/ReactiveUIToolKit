# Project Cleanup, Splitting & Restructure Plan

> Created: 2026-03-19  
> Status: **PROPOSAL** — awaiting review  
> Scope: Repo hygiene, distribution splitting, structure, documentation

---

## Table of Contents

1. [Current State Assessment](#1-current-state-assessment)
2. [Phase 1 — Immediate Cleanup (Git Bloat Removal)](#2-phase-1--immediate-cleanup-git-bloat-removal)
3. [Phase 2 — .gitignore Hardening](#3-phase-2--gitignore-hardening)
4. [Phase 3 — Splitting IDE Extensions to Separate Repo](#4-phase-3--splitting-ide-extensions-to-separate-repo)
5. [Phase 4 — Plan File Consolidation](#5-phase-4--plan-file-consolidation)
6. [Phase 5 — Stale / Orphan File Cleanup](#6-phase-5--stale--orphan-file-cleanup)
7. [Phase 6 — Distribution Trimming (Leaner Unity Package)](#7-phase-6--distribution-trimming-leaner-unity-package)
8. [Phase 7 — Documentation Improvements](#8-phase-7--documentation-improvements)
9. [Phase 8 — HMR Documentation (Docs Site)](#9-phase-8--hmr-documentation-docs-site)
10. [Phase 9 — Structural Improvements](#10-phase-9--structural-improvements)
11. [Summary: Disk Savings](#11-summary-disk-savings)
12. [Execution Order](#12-execution-order)

---

## 1. Current State Assessment

### Disk Usage Breakdown (excluding node_modules, .git, .vs)

| Directory | Size | Notes |
|---|---|---|
| `ide-extensions~/vscode/` | **364 MB** | 21 old .vsix archives (not tracked but on disk) + server/ |
| `ide-extensions~/visual-studio/` | **215 MB** | 5 old .vsix files (6 tracked!), server/, _inspect_1014/ |
| `ide-extensions~/lsp-server/` | **158 MB** | bin/obj/ build artifacts (not tracked, properly ignored) |
| `SourceGenerator~/` | **46 MB** | Tests/bin/obj/ — **271 files tracked in git!** |
| `ReactiveUIToolKitDocs~/` | 1.5 MB | Clean, dist/ properly ignored |
| `ide-extensions~/language-lib/` | 1.7 MB | Clean, build artifacts ignored |
| `Shared/` | 1.0 MB | Clean — this is the core library |
| `Samples/` | 0.6 MB | Clean |
| `Analyzers/` | 0.6 MB | 3 DLLs needed by Unity + 2 PDBs (unnecessary) |
| `Runtime/` | ~0 MB | Thin adapter (5 files) |
| Everything else | < 1 MB each | scripts, Plans~, Editor, CICD, .github, etc. |

### Git-Tracked Artifacts That Should Not Be Committed

| Category | Count | Size |
|---|---|---|
| `SourceGenerator~/Tests/bin/` + `obj/` | **271 files** | ~45 MB tracked (Roslyn, test platform DLLs, PDBs, resource satellites) |
| `.vsix` files (root + VS2022) | **6 files** | ~28 MB tracked |
| `.pdb` files | **6 files** | ~3 MB tracked |
| `.csx` temp scripts | **4 files** | < 1 MB tracked |
| **Total committed bloat** | **~287 files** | **~76 MB** |

### Git Repo Size

`.git/` directory: **168 MB** — likely heavily inflated by historical commits of binary artifacts.

---

## 2. Phase 1 — Immediate Cleanup (Git Bloat Removal)

**Goal**: Remove tracked binary artifacts from the index. This is the single highest-impact change.

### 2.1 Untrack SourceGenerator Test Build Outputs

These 271 files were accidentally committed and should never be in git:

```bash
# Remove from git index (keeps local files)
git rm -r --cached "SourceGenerator~/Tests/bin/"
git rm -r --cached "SourceGenerator~/Tests/obj/"
```

### 2.2 Untrack Committed .vsix Files

```bash
git rm --cached "UitkxVsix.vsix"
git rm --cached "UitkxVsix.vsix.meta"
git rm --cached "ide-extensions~/visual-studio/UitkxVsix/.vsix"
git rm --cached "ide-extensions~/visual-studio/UitkxVsix/UitkxVsix_1.0.1.vsix"
git rm --cached "ide-extensions~/visual-studio/UitkxVsix/UitkxVsix_live.vsix"
git rm --cached "ide-extensions~/visual-studio/UitkxVsix/UitkxVsix_test_intellisense_1.0.10.vsix"
git rm --cached "ide-extensions~/visual-studio/UitkxVsix/UitkxVsix_upload_1.0.10.vsix"
```

### 2.3 Untrack .pdb Files in Analyzers

PDBs are debug symbols — not needed for Unity to run the analyzers:

```bash
git rm --cached "Analyzers/ReactiveUITK.Language.pdb"
git rm --cached "Analyzers/ReactiveUITK.SourceGenerator.pdb"
```

### 2.4 Untrack Temporary .csx Scripts

These are debugging/scratch scripts:

```bash
git rm --cached "SourceGenerator~/diag_temp.csx"
git rm --cached "ide-extensions~/lsp-server/test_ast.csx"
git rm --cached "ide-extensions~/lsp-server/test_vdoc.csx"
# Keep SourceGenerator~/Tests/FormatTest.csx — it's a test helper
```

### 2.5 Commit Cleanup

```bash
git commit -m "chore: remove tracked build artifacts, .vsix files, PDBs, temp scripts

- Untrack SourceGenerator~/Tests/bin/ and obj/ (271 files, ~45 MB)
- Remove committed .vsix files (6 files, ~28 MB)
- Remove Analyzers/*.pdb (debug symbols, not needed)
- Remove temp .csx scripts from git index"
```

### 2.6 (Optional) Rewrite Git History

After confirming the cleanup is correct, consider running `git filter-repo` or BFG Repo Cleaner to purge the `.vsix` and binary blobs from git history. This would reclaim the 168 MB `.git/` size down to ~20-30 MB.

**⚠️ WARNING**: This rewrites all commit hashes. Only do this if there are no active forks/branches that would diverge.

```bash
# Using BFG Repo Cleaner:
bfg --delete-files "*.vsix" --no-blob-protection
bfg --delete-folders "bin" --no-blob-protection  # careful — only in SourceGenerator~/Tests/
git reflog expire --expire=now --all
git gc --prune=now --aggressive
```

**Estimated savings**: 76 MB from index immediately. Up to ~140 MB from `.git/` with history rewrite.

---

## 3. Phase 2 — .gitignore Hardening

**Goal**: Prevent future accidental commits of build artifacts.

### Current .gitignore Gaps

The `.gitignore` already covers most cases well, but is missing:

```gitignore
# ── Add these entries ──────────────────────────────────────────

# SourceGenerator test outputs (were accidentally committed)
SourceGenerator~/Tests/bin/
SourceGenerator~/Tests/obj/

# All .vsix files anywhere in the repo
**/*.vsix

# Root-level VSIX (was committed)
UitkxVsix.vsix
UitkxVsix.vsix.meta

# PDB files in Analyzers (debug symbols, not needed for distribution)
Analyzers/*.pdb

# Temporary debugging scripts
*.csx
!SourceGenerator~/Tests/FormatTest.csx

# VS2022 inspection output
ide-extensions~/visual-studio/UitkxVsix/_inspect_*/

# Empty publish folder
Analyzers/publish/

# VSCode server build output (already ignored for VS2022)
ide-extensions~/vscode/server/
```

**Note**: `SourceGenerator~/Tests/bin/` and `SourceGenerator~/Tests/obj/` are the critical additions. The current `.gitignore` only has rules for the parent `SourceGenerator~/bin/` and `SourceGenerator~/obj/` — the Tests subdirectory wasn't covered.

---

## 4. Phase 3 — Splitting IDE Extensions to Separate Repo

**Goal**: Users who install the Unity package don't need ~800 MB of IDE tooling source code, build systems, and artifacts cluttering their project.

### Current Problem

The `ide-extensions~/` folder contains:
- **3 IDE extension projects** (VS Code, VS2022, Rider) — each with their own build systems
- **1 LSP server** (.NET 8.0 project with Roslyn dependencies)
- **1 language library** (.NET Standard 2.0)
- **1 grammar** (TextMate JSON + schema)
- **Publishing docs, build scripts, CI workflows**

This is ~740 MB on disk (including build artifacts). Even without build artifacts, the source alone is ~5 MB — completely irrelevant to a Unity game developer using the package.

Unity hides `~` suffixed folders from the Asset Database, so they don't affect compilation. But they still:
- Inflate git clone times
- Bloat the working directory
- Get scanned by file watchers
- Confuse the project structure

### Proposed Split

#### Option A: Separate Git Repository (Recommended)

Create `uitkx-ide-extensions` as a standalone repo containing:

```
uitkx-ide-extensions/
├── language-lib/          ← from ide-extensions~/language-lib/
├── lsp-server/            ← from ide-extensions~/lsp-server/
├── grammar/               ← from ide-extensions~/grammar/
├── vscode/                ← from ide-extensions~/vscode/
├── visual-studio/         ← from ide-extensions~/visual-studio/
├── rider/                 ← from ide-extensions~/rider/
├── docs/                  ← from ide-extensions~/docs/
├── .github/workflows/     ← publish-vscode.yml, publish-vsix.yml, publish-rider.yml
├── scripts/               ← publish-extension.ps1, publish-vsix.ps1
└── README.md
```

**Main repo** keeps only:
```
ReactiveUIToolKit/
├── Runtime/
├── Shared/
├── Editor/
├── Samples/
├── Analyzers/           ← published DLLs (built from ide-extensions repo)
├── SourceGenerator~/    ← source generator (depends on language-lib as NuGet package or DLL)
├── CICD/
├── ...
```

**Benefits**:
- Main repo becomes ~3 MB (excluding .git)
- IDE extensions develop/release independently
- Game developers never see IDE build artifacts
- CI/CD for IDE extensions runs in isolation
- Easier to give IDE-extension-only contributors access

**Challenges**:
- `SourceGenerator~/` depends on `language-lib/` (shares parser). Two options:
  - A) Publish `ReactiveUITK.Language` as a NuGet package from the IDE repo, consume in SourceGenerator
  - B) Keep `language-lib/` as a git submodule in both repos
  - C) Keep `language-lib/` in the main repo and copy the built DLL to Analyzers/ (current approach)
- Need to move 3 CI workflows
- Build scripts need path updates

**Recommended dependency approach**: Option C is simplest — continue the current model where `Analyzers/ReactiveUITK.Language.dll` is the built output. The IDE repo builds it and publishes it. The main repo consumes the DLL. No NuGet intermediary needed.

#### Option B: Git Submodule (Simpler but Less Clean)

Keep `ide-extensions~/` as a git submodule pointing to the separate repo:

```
ReactiveUIToolKit/
├── ide-extensions~/    ← git submodule → uitkx-ide-extensions
├── Runtime/
├── ...
```

**Benefits**: Same repo structure, developers who need IDE tools can init the submodule
**Downsides**: Submodules are notoriously painful (especially on Windows/Unity)

#### Option C: Keep Together, Just Clean Up (Minimum Viable)

If splitting is too disruptive right now:
- Clean all build artifacts (Phase 1-2)
- Add proper `.gitignore` entries
- Delete old .vsix files from disk
- Result: ~5 MB of IDE source code stays in repo (acceptable for now)

### Recommendation

**Start with Option C now** (cleanup), plan Option A for later. The immediate cleanup removes ~76 MB from git and ~700 MB from disk. A full repo split can be done when there's bandwidth for it.

---

## 5. Phase 4 — Plan File Consolidation

**Goal**: Unite the three separate plan directories into one canonical location.

### Current State

Plan/design documents exist in **three** locations:

| Location | Files | Purpose |
|---|---|---|
| `Plans~/` | 18 files | Main plan directory (active) |
| `root/plan/` | 2 files | Tactical plans |
| `root/plans/` | 6 files (inc. JSON index) | Strategic plans + codebase index |

### Proposed Action

1. **Move `root/plan/*.md` → `Plans~/`**
   - `dual-mode-uitkx-editor-live-plan.md` → keep
   - `intellisense-plan.md` → keep

2. **Move `root/plans/*.md` → `Plans~/`**
   - `UITKX_HMR_PLAN.md` → keep (mark as IMPLEMENTED)
   - `docs-dual-track-plan.md` → keep
   - `tech-debt.md` → merge with `TECH_DEBT_COMPLETION_CONTEXT.md`
   - `intellisense-bugs-v2.md` → keep
   - `repository-atlas.md` → keep (useful reference)
   - `codebase-index.json` → move to `root/codebase-index.json` or archive

3. **Delete empty `root/plan/` and `root/plans/` directories** after migration

4. **Add status tags to plan files** — prefix each plan with a status header:
   ```markdown
   > Status: IMPLEMENTED | IN PROGRESS | PLANNED | ARCHIVED
   ```

5. **Archive completed plans** into `Plans~/archive/`:
   - `EMBEDDED_ROSLYN_IMPLEMENTATION_PLAN.md` → IMPLEMENTED, archive
   - `UITKX_IMPLEMENTATION_PLAN.md` → IMPLEMENTED, archive
   - `UITKX_IMPLEMENTATION_PLAN_FUNCTION_STYLE_SYNTAX.md` → IMPLEMENTED, archive
   - `UITKX_IMPLEMENTATION_PLAN_BREAK_CONTINUE.md` → IMPLEMENTED, archive
   - `VSCODE_TOOLING_PLAN.md` → mostly IMPLEMENTED, archive
   - `VS2022_PUBLISH_GUIDE.md` → move to `ide-extensions~/docs/` (it's a guide, not a plan)

---

## 6. Phase 5 — Stale / Orphan File Cleanup

**Goal**: Remove files that serve no current purpose.

| File / Directory | Action | Reason |
|---|---|---|
| `UitkxVsix.vsix` (root) | **Delete** | Old VS2022 build artifact in wrong location |
| `UitkxVsix.vsix.meta` (root) | **Delete** | Companion meta for removed file |
| `Analyzers/publish/` | **Delete** | Empty directory, orphaned |
| `Analyzers/ReactiveUITK.Language.pdb` | **Delete** | Debug symbol, not needed in distribution |
| `Analyzers/ReactiveUITK.SourceGenerator.pdb` | **Delete** | Debug symbol, not needed in distribution |
| `Analyzers/ReactiveUITK.SourceGenerator.deps.json` | **Delete** | Not needed (already in .gitignore-ish config) |
| `RegistryDebug.cs` (root) | **Move** → `Editor/RegistryDebug.cs` | Loose file in root; it's an editor-only debug utility |
| `SourceGenerator~/diag_temp.csx` | **Delete** | Temporary debugging scratch file |
| `SourceGenerator~/Parser/` (empty) | **Delete** | Dead directory (parser is in language-lib) |
| `SourceGenerator~/Nodes/` (empty) | **Delete** | Dead directory (nodes are in language-lib) |
| `ide-extensions~/lsp-server/test_ast.csx` | **Delete** | Temporary debugging script |
| `ide-extensions~/lsp-server/test_vdoc.csx` | **Delete** | Temporary debugging script |
| `ide-extensions~/visual-studio/UitkxVsix/_inspect_1014/` | **Delete** | Old inspection output |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxVsix_*.vsix` | **Delete** | Old versioned build artifacts |
| `ide-extensions~/visual-studio/UitkxVsix/.vsix` | **Delete** | Old build intermediate |
| `ide-extensions~/vscode/uitkx-*.vsix` | **Delete from disk** | Not tracked but cluttering workspace (21 files, ~300 MB) |
| `ide-extensions~/vscode/UitkxVsix.vsix` | **Delete from disk** | Not tracked, wrong location for VS2022 artifact |
| `Diagnostics/Benchmark/` | **Keep** (empty is OK) | May be used for future benchmarks |
| `Diagnostics/Logs/` | **Keep** (empty is OK) | May be used for future logging |
| `_docs_branch/` | **Review** | Purpose unclear — delete if unused |
| `GeneratedPreview~/Test.uitkx.g.cs` | **Keep** | Useful reference for generated output |

### Local-Only Cleanup

These are on disk but NOT tracked — safe to delete locally:

```powershell
# Delete untracked .vsix files (safe — they're build artifacts)
Remove-Item "ide-extensions~/vscode/uitkx-*.vsix" -Force
Remove-Item "ide-extensions~/vscode/UitkxVsix.vsix" -Force
Remove-Item "ide-extensions~/visual-studio/UitkxVsix/UitkxVsix.vsix" -Force

# Delete server build outputs (rebuilt by build scripts)
Remove-Item "ide-extensions~/vscode/server" -Recurse -Force
Remove-Item "ide-extensions~/visual-studio/UitkxVsix/server" -Recurse -Force
```

---

## 7. Phase 6 — Distribution Trimming (Leaner Unity Package)

**Goal**: When users install the Unity package, they should get only what they need.

### What Users Actually Need

For a Unity game that uses ReactiveUIToolKit:

| Directory | Needed? | Notes |
|---|---|---|
| `Runtime/` | **YES** | Core MonoBehaviour adapter |
| `Shared/` | **YES** | Core reactive library (V, Hooks, Fiber, Elements, Props) |
| `Editor/` | **YES** | Editor utilities, HMR, change watcher |
| `Analyzers/` | **YES** | Source generator DLLs (3 DLLs) |
| `Samples/` | Optional | Demo components — could be a separate UPM sample |
| `package.json` | **YES** | Unity package manifest |
| `README.md` | **YES** | Package documentation |
| `config.json` | **YES** | Runtime configuration |

### What Users Do NOT Need

| Directory | Size (source) | Purpose |
|---|---|---|
| `ide-extensions~/` | ~5 MB source | IDE extension development |
| `SourceGenerator~/` | ~1 MB source | Source generator development (the built DLL is in Analyzers/) |
| `Plans~/` | 0.3 MB | Internal plans |
| `ReactiveUIToolKitDocs~/` | 1.5 MB | Documentation website |
| `scripts/` | < 0.1 MB | Build/publish automation |
| `CICD/` | < 0.1 MB | CI/CD utilities |
| `Diagnostics/` | < 0.1 MB | Empty benchmark/log dirs |
| `.github/` | < 0.1 MB | CI workflows |
| `root/` | 0.4 MB | Internal plans |

### Current Protection

Unity already ignores `~`-suffixed folders (`ide-extensions~`, `SourceGenerator~`, `Plans~`, `ReactiveUIToolKitDocs~`, `GeneratedPreview~`) — these don't compile or appear in the Unity asset database.

`config.json` → `pathsToOmitFromDist` already lists CICD, Diagnostics, scripts, etc. for the distribution build.

### Recommended Additional Actions

1. **Make Samples a UPM Sample** (instead of hardcoded):
   - Move `Samples/` content into the UPM `samples` field in `package.json`
   - Users opt-in to import samples via Package Manager → Import button
   - Saves ~0.6 MB per project that doesn't need demos

2. **Add `pathsToOmitFromDist` entries** for anything newly identified:
   ```json
   "pathsToOmitFromDist": [
     // ... existing entries ...
     "root/**",
     "GeneratedPreview~/**",
     ".github/**",
     "publisher-secrets.example.json",
     "publisher-secrets.example.json.meta",
     "ReactiveUIToolKit.code-workspace",
     "ReactiveUIToolKit.code-workspace.meta",
     "RegistryDebug.cs",
     "RegistryDebug.cs.meta"
   ]
   ```

3. **Consider adding a `.npmignore` or `.upmignore`** if publishing to a UPM registry, to exclude non-essential files from the tarball.

---

## 8. Phase 7 — Documentation Improvements

**Goal**: Better project documentation for maintainers and contributors.

### 8.1 Root README.md Enhancement

The current README has a quick start but lacks:
- **Architecture overview** — what each directory is for
- **Development guide** — how to set up for development
- **Build instructions** — for each component (source gen, IDE extensions, docs site)
- **Contributing guide** — context for new contributors

Proposed README structure:
```markdown
# ReactiveUIToolKit

> React-style UI framework for Unity UI Toolkit

## Quick Start
(existing content)

## Architecture

### Directory Map
| Directory | Description |
|---|---|
| `Runtime/` | Thin MonoBehaviour adapter — RootRenderer, RenderScheduler |
| `Shared/` | Core reactive library — V, VNode, Hooks, Fiber reconciler, Elements, Props |
| `Editor/` | Unity Editor integration — HMR, change watcher, console navigation |
| `Analyzers/` | Published Roslyn analyzer/source generator DLLs |
| `Samples/` | Demo components: legacy C# (Components/), UITKX (UITKX/), showcase app (Showcase/) |
| `SourceGenerator~/` | Source generator source code + tests |
| `ide-extensions~/` | IDE extension projects (VS Code, VS2022, Rider, shared LSP server) |
| `Plans~/` | Design documents and implementation plans |
| `ReactiveUIToolKitDocs~/` | Documentation website (Vite + React) |
| `scripts/` | Build and publish automation |

### Key Architectural Decisions
- **Shared is the core**, Runtime is a thin adapter
- **Source generator** transpiles `.uitkx` → C# at build time
- **LSP server** (language-lib + lsp-server) provides IDE features
- **HMR** compiles `.uitkx` in-editor without domain reload
- `~` suffix hides IDE/dev folders from Unity's Asset Database

## Development Setup

### Prerequisites
- Unity 6000.2+
- .NET 8+ SDK
- Node.js 18+ (for VS Code extension and docs site)
- Visual Studio 2022 (for VS2022 extension development)

### Building Components

#### Source Generator
```
dotnet build SourceGenerator~/ReactiveUITK.SourceGenerator.csproj
scripts/build-generator.ps1  # builds + copies DLL to Analyzers/
```

#### IDE Extensions
```
# VS Code
cd ide-extensions~/vscode && npm run build

# VS2022
ide-extensions~/visual-studio/build-local.ps1

# LSP Server (shared)
dotnet publish ide-extensions~/lsp-server -c Release
```

#### Documentation Site
```
cd ReactiveUIToolKitDocs~ && npm run dev
```

### Running Tests
```
dotnet test SourceGenerator~/Tests
```
```

### 8.2 Samples README

Add `Samples/README.md` explaining the three sample categories:

```markdown
# Samples

## Directory Structure

### Components/ (Legacy C# Style)
Function-style components using the C# `V.*` API directly.
These demonstrate hooks, fiber behavior, and runtime features
without UITKX markup. Kept as reference for the C# authoring track.

### UITKX/ (Modern UITKX Style)
Same demos rewritten in `.uitkx` declarative markup.
Each component has a `.uitkx` file + companion `.cs` file.
This is the recommended authoring style.

### Showcase/ (Host Application)
Multi-page demo application hosting all samples together.
Contains Bootstrap classes and EditorWindow entries.
Run in Editor or Runtime mode.

### Shared/ (Common Utilities)
Reusable demo components used across categories:
animations, shared layouts, navigation bars.
```

### 8.3 IDE Extensions README

Improve `ide-extensions~/README.md`:
- Architecture diagram (shared language-lib → LSP server → clients)
- Per-IDE feature matrix
- Build instructions for each IDE
- How to debug

---

## 9. Phase 8 — HMR Documentation (Docs Site)

**Goal**: Add HMR documentation to the Tooling section of `ReactiveUIToolKitDocs~/`.

### Current State

- `Editor/HMR/README.md` — excellent internal README (already complete, ~130 lines)
- `root/plans/UITKX_HMR_PLAN.md` — implementation plan (status: IMPLEMENTED)
- Docs site `Tooling` section — only Router and Signals, **no HMR page**

### Implementation Plan

#### 9.1 Create HMR Documentation Page

Create `ReactiveUIToolKitDocs~/src/pages/Tooling/HMR/HmrPage.tsx`:

**Sections to include** (adapted from `Editor/HMR/README.md`):

1. **Introduction**
   - What is HMR — edit `.uitkx` files and see changes instantly
   - Why it matters — no domain reload, state preserved, 50-200ms cycle

2. **Quick Start**
   - Open menu: ReactiveUITK → HMR Mode
   - Click Start HMR
   - Edit any `.uitkx` file → instant visual update

3. **How It Works** (architecture diagram)
   ```
   FileSystemWatcher (150ms debounce)
       ↓
   Parse .uitkx → Emit C# → Roslyn Compile → Assembly.Load
       ↓
   Delegate Swap → Re-render (hooks run against preserved state)
   ```

4. **State Preservation**
   - useState, useRef, useEffect, useMemo, useCallback all preserved
   - Hook mismatch detection (auto-reset with warning)

5. **Companion Files**
   - Partial class, styles, types automatically included in compilation

6. **HMR Window UI**
   - Start/Stop button, stats, timing breakdown, recent errors
   - Settings: auto-stop on play mode, swap notifications

7. **Keyboard Shortcuts**
   - How to configure via HMR window
   - Requirements (modifier + key)

8. **Lifecycle**
   - Auto-stops: Play Mode, Build, Editor quit
   - While active: all compilation deferred (UX warning shown)

9. **Limitations**
   - Old assemblies stay in memory (~10-30 KB per swap)
   - New components not hot-loaded
   - Static field changes ignored
   - Cross-assembly props via reflection

10. **Troubleshooting**
    - HMR doesn't start
    - Changes don't appear
    - State lost after edit

#### 9.2 Create HMR Style File

Create `ReactiveUIToolKitDocs~/src/pages/Tooling/HMR/HmrPage.style.ts`

#### 9.3 Register in docs.tsx

Add to the `uitkx-tooling` section in `docs.tsx`:

```tsx
import { HmrPage } from './pages/Tooling/HMR/HmrPage'

// In uitkx-tooling section pages array:
{
  id: 'uitkx-hmr-page',
  canonicalId: 'hmr',
  title: 'Hot Module Replacement',
  path: '/tooling/hmr',
  keywords: ['uitkx', 'hmr', 'hot reload', 'live editing', 'instant preview'],
  track: 'uitkx',
  element: () => <HmrPage />,
},
```

#### 9.4 Update Tooling Overview

Update `ToolingOverviewPage.tsx` to mention HMR alongside Router and Signals:

```tsx
<Typography variant="body1">
  Utilities that ship with ReactiveUITK: <code>HMR</code> for instant live editing,{' '}
  <code>Router</code> for navigation, and <code>Signals</code> for shared state.
</Typography>
```

#### 9.5 (Future) IDE Extensions Documentation Page

Consider also adding a `Tooling/IDE/` page documenting:
- Supported editors (VS Code, VS2022, Rider)
- Installation instructions per editor
- Features: syntax highlighting, completions, hover, diagnostics, formatting, go-to-definition
- Troubleshooting

---

## 10. Phase 9 — Structural Improvements

### 10.1 Assembly Definition Naming

Current `.asmdef` files are well-structured. No changes needed.

### 10.2 Samples as UPM Optional Package

In `package.json`, move Samples into the `samples` key:

```json
{
  "samples": [
    {
      "displayName": "UITKX Component Demos",
      "description": "25+ demo components showing UITKX markup features",
      "path": "Samples/UITKX"
    },
    {
      "displayName": "C# Function Component Demos (Legacy)",
      "description": "Legacy C# V.* API demos (pre-UITKX style)",
      "path": "Samples/Components"
    },
    {
      "displayName": "Showcase Application",
      "description": "Multi-page host app demonstrating all features",
      "path": "Samples/Showcase"
    }
  ]
}
```

This lets users optionally import samples via Unity's Package Manager instead of always including them.

### 10.3 Move RegistryDebug.cs

`RegistryDebug.cs` is sitting loose in the package root. Move it to `Editor/`:

```bash
git mv RegistryDebug.cs Editor/RegistryDebug.cs
git mv RegistryDebug.cs.meta Editor/RegistryDebug.cs.meta
```

---

## 11. Summary: Disk Savings

### After Phase 1-2 (Git Cleanup + .gitignore)

| Metric | Before | After |
|---|---|---|
| Git-tracked files | 2028 | ~1750 |
| Tracked binaries | 287 | ~6 (3 Analyzers DLLs + 3 metas) |
| Working dir (excluding node_modules) | ~800 MB | ~720 MB |
| `.git/` size | 168 MB | 168 MB (or ~25 MB with history rewrite) |

### After Phase 5 (Stale File Cleanup)

| Metric | Before | After |
|---|---|---|
| Untracked .vsix on disk | ~363 MB | 0 MB |
| Server build outputs (untracked) | ~78 MB | 0 MB (rebuilt on demand) |
| Working dir (excluding node_modules) | ~720 MB | ~280 MB |

### After Phase 3 Option A (Repo Split)

| Metric | Main Repo | IDE Repo |
|---|---|---|
| Source size | ~3 MB | ~5 MB |
| With build artifacts | ~50 MB | ~200 MB |
| Focus | Unity game developers | IDE extension developers |

---

## 12. Execution Order

| Priority | Phase | Effort | Impact | Risk |
|---|---|---|---|---|
| **P0** | Phase 1 — Git bloat removal | 15 min | **HIGH** (76 MB from git) | Low |
| **P0** | Phase 2 — .gitignore fix | 5 min | **HIGH** (prevents regression) | None |
| **P1** | Phase 5 — Stale file cleanup | 20 min | **HIGH** (~400 MB from disk) | Low |
| **P1** | Phase 4 — Plan consolidation | 15 min | Medium (organization) | None |
| **P2** | Phase 7 — Documentation | 30 min | Medium (maintainability) | None |
| **P2** | Phase 8 — HMR docs page | 45 min | Medium (user-facing) | None |
| **P2** | Phase 6 — Distribution trimming | 15 min | Medium (package size) | Low |
| **P3** | Phase 9 — Structural improvements | 20 min | Low (polish) | Low |
| **P3** | Phase 3 — Repo split | 2-4 hours | **HIGH** (long-term) | Medium |

**Recommended starting point**: Phases 1, 2, and 5 together as one cleanup commit. Maximum impact, minimal risk.
