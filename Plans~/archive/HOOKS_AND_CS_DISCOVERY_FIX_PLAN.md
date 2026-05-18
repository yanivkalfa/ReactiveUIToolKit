# Cross-namespace hooks + cross-directory `.cs` discovery — implementation plan

Status: **COMPLETED in 0.5.12 / VS Code 1.2.8 / VS 2022 1.2.8 (2026-05-14).**
All phases shipped; SG 1228/1228 + LSP 63/63 passing. TECH_DEBT_V2 #18 + #19
struck. See CHANGELOG.md and Plans~/DISCORD_CHANGELOG.md for the user-facing
summary.

Scope: TECH_DEBT_V2 items #18 (cross-namespace `using static` for peer hook
containers) + #19 (LSP `.cs` companion discovery beyond the sibling folder).
Target release: library 0.5.12 / VS Code ext 1.2.8 / VS 2022 ext 1.2.8 — both
fixes shipped together.

This plan is the result of a multi-layer audit of every file involved. It
records the verified call graph, the exact line numbers being touched, the
performance math behind each change, and the parity matrix across the four
generation/analysis layers. Nothing here is theoretical — every claim was
checked against the current `cleanup_and_upgrades` working tree.

---

## 1. Executive summary

| Item | Layers touched                       | Risk   | Perf delta (HMR) | Perf delta (build) | Perf delta (LSP) | Player-build delta |
| ---- | ------------------------------------ | ------ | ---------------- | ------------------ | ---------------- | ------------------ |
| #18  | SG, HMR, IDE virtual doc             | Low    | +O(N) once on session start, then O(1) per edit (cached registry) | Zero (already pre-scanned) | Zero (no LSP-side change) | **Zero** (editor-only) |
| #19  | LSP only                             | Low    | n/a              | n/a                | +O(M) once on first open of a `.uitkx`, then O(1) lookup | **Zero** (editor-only) |

Both fixes are confined to editor-time / IDE code paths. Nothing under
`Runtime/` or `Shared/Core/` is modified, so the IL2CPP and Mono player
builds are byte-identical before and after.

---

## 2. Layer-by-layer audit (verified)

### 2.1 SourceGenerator (`SourceGenerator~/`)

- `UitkxGenerator.cs:142-161` — pre-scan loop already filters every
  `.uitkx` by `UitkxPipeline.IsOwnedByCompilation(txt.Path,
  compilation.AssemblyName)` BEFORE adding to
  `peerHookContainersBuilder`. The asmdef boundary is therefore already
  enforced one layer up. Anything that reaches Stage 3d through
  `peerHookContainers` is guaranteed to belong to the current Unity
  assembly.
- `UitkxPipeline.cs:226-246` — Stage 3d gate. The current code reads:

  ```csharp
  foreach (var phc in peerHookContainers.Value)
  {
      if (phc.Namespace == directives.Namespace)
          extraUsings.Add($"static {phc.Namespace}.{phc.ClassName}");
  }
  ```

  The `Namespace == directives.Namespace` check is the bug: it makes the
  feature usable only when the consumer component lives in the same
  namespace as the hook file. The pre-scan filter already prevents
  cross-asmdef pollution, so this guard provides no additional safety.
- `UitkxPipeline.cs:281-340` — `IsOwnedByCompilation` walks up to find
  the nearest `.asmdef`, falling back to the
  `Assembly-CSharp[-Editor]` convention. This is the authoritative
  definition of asmdef ownership across the SG. It is reused by HMR and
  LSP fixes below (lifted into a shared helper — see §3.4).
- `UitkxPipeline.cs:57-103` — short-circuit branch for hook/module
  files. Hook source files emit through `HookEmitter.Emit` directly and
  never reach Stage 3d, so a hook-file-importing-itself situation is
  structurally impossible. Confirms the `phc.Namespace ==
  directives.Namespace` guard is doubly redundant: the only file at
  Stage 3d is the COMPONENT file, so self-import is by construction
  impossible.
- `Emitter/HookEmitter.cs:86, 313` — generated `using static` target is
  always `public static partial class {DeriveContainerClassName(file)}`,
  and the class name is filename-derived. Two hook files with the same
  filename stem in the same namespace merge naturally as partials, so
  no dedup harm if the same `(Ns, ClassName)` arrives twice.
- `Tests/HookUsingStaticTest.cs` — only one happy-path test exists,
  asserting same-namespace injection. No test currently asserts
  cross-namespace injection. Negative test ("using static MUST NOT be
  emitted for foreign asmdef") is also missing.

### 2.2 HMR (`Editor/HMR/`)

- `UitkxHmrController.cs:300-345` — companion `.cs` discovery is
  prefix-scoped (`<componentBase>.*.cs`) AND same-directory only. This
  is unrelated to #19 because **the HMR compile already references the
  full project assembly** (`BuildMetadataReferences` in
  `UitkxHmrCompiler.cs:942-956` walks `AppDomain.CurrentDomain.
  GetAssemblies()`), so `UIDocumentSlot` is reachable as a compiled
  type. No HMR change is needed for issue #19.
- `UitkxHmrCompiler.cs:206-260` — emit pipeline is the SG-mirror;
  receives `directives` (parser output) as input.
- `UitkxHmrCompiler.cs:414-465` — `EmitCompanionUitkxSources` is the
  HMR analogue of SG Stage 3d. **Same-directory + prefix-only** scan
  (`Directory.GetFiles(dir, prefix + "*.uitkx")`). It builds
  `hookContainerFqns` exclusively from companions, then injects
  `using static` for each. After SG is fixed, this layer becomes the
  weakest link: a user who removes their manual
  `@using static <CrossDirHookNs>.<Container>;` will see SG/build
  succeed but HMR recompiles fail with
  "useFooHook does not exist in the current context". Mandatory parity
  fix.
- `UitkxHmrFileWatcher.cs:11-65` — fires `OnUitkxChanged(path)` for
  every `.uitkx` create/change/rename across the watch root. Suitable
  invalidation surface for a workspace hook-container registry.

### 2.3 IDE virtual document (`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`)

- `VirtualDocumentGenerator.cs:160-261` — emits all `using` directives
  (standard list + `d.Usings` from the parser). Does **not** auto-inject
  `using static` for peer hook containers. Today this works only because
  the LSP loads peer module/hook `.uitkx` files into the same Roslyn
  workspace (see `WorkspaceIndex.GetModuleAndHookFiles` →
  `RoslynHost.AddPeerUitkxDocumentsToSolution`), so the resulting
  symbols are in the compilation. But Roslyn semantic analysis only
  binds the lowercase alias (`useFooHook(...)`) when a `using static`
  is present. Without injection, after we remove the manual `@using
  static` from MenuPage, the IDE will surface CS0103 squiggles even
  while the SG/HMR builds succeed. Parity is mandatory.

### 2.4 LSP (`ide-extensions~/lsp-server/`)

- `WorkspaceIndex.cs:67` — already maintains a `_moduleHookFiles`
  HashSet (added in 0.5.11 for cross-directory `.uitkx` peer
  resolution). Provides the proven pattern for adding a parallel
  `_allCsFiles` HashSet.
- `WorkspaceIndex.cs:340-410` — `Refresh` and `ScanDirectory` already
  walk `*.cs` and `*.uitkx`, skipping tilde folders. Adding/removing
  `.cs` paths to a new `_allCsFiles` set is one extra HashSet op per
  file with zero new I/O.
- `WorkspaceIndex.cs:509-602, 613-636` — `IndexFile` is the existing
  `*.cs` parser; `CommitElement` takes its own write lock. Any new
  `_allCsFiles` write must NOT nest with `CommitElement`'s write lock
  (the lock is `LockRecursionPolicy.NoRecursion` by default for
  `ReaderWriterLockSlim`). Safe pattern: write the path BEFORE calling
  `IndexFile` (which transitively calls `CommitElement`), in
  `ScanDirectory` and in `Refresh`.
- `RoslynHost.cs:1138-1187` — `FindCompanionFiles` is same-directory
  only. `AddCompanionDocuments` reads each file from disk and adds a
  Roslyn `Document`. Add an asmdef-filtered union with the
  workspace-wide `*.cs` set to fix #19.
- `RoslynHost.cs:159-218` — `FileState.CompanionDocIds` already tracks
  the per-`.uitkx` companion document set; the existing eviction logic
  in the refresh path scales with the size of this list. With the
  workspace-wide union, the list grows from "files in one folder" to
  "files in one asmdef". For a typical Unity asmdef (single-digit
  hundreds of `.cs` files) that is a one-time per-`.uitkx`-open cost
  on the order of single-digit MB of source text. See §4 for measured
  bounds.

---

## 3. Implementation

### 3.1 Issue #18 — SG Stage 3d (smallest possible change)

File: `SourceGenerator~/UitkxPipeline.cs:226-246`

Before:

```csharp
if (peerHookContainers != null && !peerHookContainers.Value.IsDefaultOrEmpty
    && !string.IsNullOrEmpty(directives.Namespace))
{
    var extraUsings = ImmutableArray.CreateBuilder<string>(
        directives.Usings.Length + peerHookContainers.Value.Length);
    extraUsings.AddRange(directives.Usings);
    foreach (var phc in peerHookContainers.Value)
    {
        if (phc.Namespace == directives.Namespace)
            extraUsings.Add($"static {phc.Namespace}.{phc.ClassName}");
    }
    if (extraUsings.Count > directives.Usings.Length)
        directives = directives with { Usings = extraUsings.ToImmutable() };
}
```

After:

```csharp
if (peerHookContainers != null && !peerHookContainers.Value.IsDefaultOrEmpty)
{
    // Pre-scan in UitkxGenerator already filters by IsOwnedByCompilation,
    // so every entry here is in the current Unity asmdef. Inject one
    // `using static` per distinct peer hook container regardless of
    // namespace. Dedup is a HashSet on the FQN string.
    var seen = new HashSet<string>(StringComparer.Ordinal);
    foreach (var u in directives.Usings)
        seen.Add(u);

    var extraUsings = ImmutableArray.CreateBuilder<string>(
        directives.Usings.Length + peerHookContainers.Value.Length);
    extraUsings.AddRange(directives.Usings);
    foreach (var phc in peerHookContainers.Value)
    {
        string fqn = $"static {phc.Namespace}.{phc.ClassName}";
        if (seen.Add(fqn))
            extraUsings.Add(fqn);
    }
    if (extraUsings.Count > directives.Usings.Length)
        directives = directives with { Usings = extraUsings.ToImmutable() };
}
```

Net change: drop one `if`, drop the `directives.Namespace` precondition,
add a HashSet dedup. ~10 lines diff.

Notes:

- Removing the `!string.IsNullOrEmpty(directives.Namespace)` guard is
  safe: `peerHookContainers` always has a non-empty namespace (the
  pre-scan in `UitkxGenerator.TryBuildPeerHookContainerInfo` rejects
  hook files with no `@namespace`). The component file may have no
  `@namespace`, in which case CSharpEmitter emits into
  `ReactiveUITK.Generated`. The `using static <hookNs>.<container>;` is
  still legal there.
- HashSet dedup protects against the (currently impossible but cheap to
  guard) case of two hook files in the same namespace deriving the
  same container class name.

### 3.2 Issue #18 — HMR parity (workspace registry)

Files:
- `Editor/HMR/UitkxHmrCompiler.cs:414-465` — replace companion-only
  scan with registry lookup.
- `Editor/HMR/HookContainerRegistry.cs` — **new**.
- `Editor/HMR/UitkxHmrController.cs` — wire registry to file watcher.

New file: `Editor/HMR/HookContainerRegistry.cs`

```csharp
namespace ReactiveUITK.Editor.HMR
{
    // Editor-only registry of (asmdef -> hook container FQNs).
    // Built lazily from the workspace, invalidated by the existing
    // UitkxHmrFileWatcher event. Lookup is O(1) per asmdef.
    internal static class HookContainerRegistry
    {
        // asmdef name -> distinct FQN list (e.g. "PrettyUi.UIHooks.UseFooHooks")
        private static readonly Dictionary<string, List<string>> _byAsmdef =
            new(StringComparer.Ordinal);
        private static readonly object _gate = new();
        private static bool _seeded;

        public static IReadOnlyList<string> GetForAsmdef(string asmdefName)
        { /* read-locked snapshot; returns empty array if not seeded */ }

        public static void Invalidate(string changedUitkxPath)
        { /* write-locked: re-parse just the changed file's directives,
             update _byAsmdef[ownerAsmdef]; full re-seed only on Reset */ }

        public static void Seed(string projectRoot)
        { /* one-time scan: enumerate *.uitkx under Assets/, parse
             directives via DirectiveParser.Parse, classify hook files,
             group by asmdef via AsmdefResolver.OwningAsmdefName(path) */ }

        public static void Reset() { /* clear; called from controller stop */ }
    }
}
```

Wiring in `UitkxHmrController`:
- On `Start(...)`: subscribe `_fileWatcher.OnUitkxChanged += path =>
  HookContainerRegistry.Invalidate(path);` and call
  `HookContainerRegistry.Seed(_projectRoot)` once on first
  `ProcessFileChange`.
- On `Stop()`: `HookContainerRegistry.Reset()` (reuses existing dispose
  hook).

Replacement in `UitkxHmrCompiler.EmitCompanionUitkxSources`:
- Keep the existing same-directory companion scan (unchanged — modules,
  same-component partials still need to be emitted into the HMR
  compilation, those are NOT just FQN-references).
- After the companion scan, add a second pass:

  ```csharp
  string asmdef = AsmdefResolver.OwningAsmdefName(uitkxPath);
  foreach (var fqn in HookContainerRegistry.GetForAsmdef(asmdef))
  {
      // Skip self (component file is not a hook container) and skip
      // anything we already emitted via the companion pass.
      if (alreadyEmittedContainerFqns.Add(fqn))
          hookContainerFqns.Add(fqn);
  }
  ```

  The set `alreadyEmittedContainerFqns` is the existing `hookContainerFqns`
  promoted to a HashSet for the duration of the call.

Note: HMR does not need to compile the hook source file's body again —
Unity has already compiled it into the project assembly, which is in
HMR's metadata refs. The registry only contributes FQN strings used to
emit `using static` lines into the trampoline source. Zero extra CSharp
compilation work.

### 3.3 Issue #18 - IDE virtual doc parity (LSP-side, NOT VDG)

**Discovery during execution-pass research**: the LSP already has its own
Stage-3d analogue called `EnrichWithPeerHookUsings` in
`ide-extensions~/lsp-server/Roslyn/RoslynHost.cs:1248-1305`. It is
called before every `_docGenerator.Generate(...)` (L790 and L839) and
appends the `using static` lines to `d.Usings` so they flow through the
VDG's existing using-loop. The VDG itself needs **no parameter
change** — the parity fix lives entirely in `EnrichWithPeerHookUsings`.

It has the same `peerDirectives.Namespace != d.Namespace` bug as SG
Stage 3d. Replace that strict check with the asmdef-aware filter
below, and also lift the filename-derived class-name logic into a
shared helper so SG/HMR/LSP all derive the same name from the same
file.

File: `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs:1248-1305`

Before:

```csharp
if (peerDirectives.HookDeclarations.IsDefaultOrEmpty)
    continue;
if (peerDirectives.Namespace != d.Namespace)
    continue;
// derive containerClass...
extraUsings.Add($"static {d.Namespace}.{containerClass}");
```

After:

```csharp
if (peerDirectives.HookDeclarations.IsDefaultOrEmpty)
    continue;
if (string.IsNullOrEmpty(peerDirectives.Namespace))
    continue;
if (AsmdefResolver.OwningAsmdefName(peerPath)
    != AsmdefResolver.OwningAsmdefName(uitkxFilePath))
    continue;
// derive containerClass...
extraUsings.Add($"static {peerDirectives.Namespace}.{containerClass}");
```

Note the FQN now uses `peerDirectives.Namespace` (the hook file's
namespace), not `d.Namespace` (the consumer's). This is the actual
namespace where `HookEmitter` emits the `static partial class`.
`FindPeerUitkxFiles` already returns workspace-wide module/hook files
via `_workspaceIndex.GetModuleAndHookFiles()`, so cross-directory
discovery is free.

### 3.4 Shared helper — `AsmdefResolver`

The asmdef-walking logic currently lives in
`UitkxPipeline.FindOwningAsmdefAssemblyName` (SG netstandard2.0
assembly). Both the HMR registry and the LSP filter need the same
resolver. Two options:

Option A (chosen): copy the function into a small editor-side helper
`Editor/HMR/AsmdefResolver.cs` and a separate LSP-side helper
`ide-extensions~/lsp-server/AsmdefResolver.cs`. ~30 lines each. Pro:
zero coupling between SG netstandard2.0 and editor/LSP TFMs. Con:
duplicated regex.

Option B: lift into `Shared/Core/AsmdefResolver.cs` and reference from
all three. Con: adds runtime weight to player builds (the file would
ship in `Assembly-CSharp.dll`). Even though the API would not be
called at runtime, the IL would still be present. Rejected for that
reason.

A 30-line `static class AsmdefResolver` in each consumer carries no
maintenance cost — the regex (`"name"\s*:\s*"([^"]+)"`) and walk-up
loop are stable. A parity test (`UitkxPipelineParityTests`) will assert
that `UitkxPipeline.IsOwnedByCompilation` and
`AsmdefResolver.OwningAsmdefName` agree on a fixture set of paths.

### 3.5 Issue #19 — LSP cross-directory `.cs`

Files:
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — new state +
  `GetAllCsFiles()`.
- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` — extend
  `FindCompanionFiles`.

`WorkspaceIndex.cs` changes:

1. Field after `_moduleHookFiles`:

   ```csharp
   private readonly HashSet<string> _allCsFiles =
       new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   ```

2. In `ScanDirectory(string rootPath)`, inside the existing `*.cs` loop,
   add `_lock.EnterWriteLock(); try { _allCsFiles.Add(file); } finally
   { _lock.ExitWriteLock(); }` BEFORE the call to `IndexFile(file)` so
   the inner `CommitElement` write-lock cannot nest. (Or batch the
   adds: collect all paths into a local list, take the write lock once
   at end of directory scan, bulk-add.)

3. In `Refresh(string filePath)`:
   - On `.cs` add/change: write-lock, `_allCsFiles.Add(filePath);` then
     proceed with existing `IndexFile`.
   - On `.cs` delete: write-lock, `_allCsFiles.Remove(filePath);` plus
     existing eviction.

4. New public API:

   ```csharp
   public IReadOnlyList<string> GetAllCsFiles()
   {
       _lock.EnterReadLock();
       try { return _allCsFiles.ToArray(); }
       finally { _lock.ExitReadLock(); }
   }
   ```

`RoslynHost.cs:1138-1187` change:

```csharp
private static IReadOnlyList<string> FindCompanionFiles(
    string uitkxFilePath,
    WorkspaceIndex? index = null)
{
    var dir = Path.GetDirectoryName(uitkxFilePath);
    var same = (dir != null && Directory.Exists(dir))
        ? Directory.EnumerateFiles(dir, "*.cs")
        : Enumerable.Empty<string>();

    if (index is null)
        return same.ToArray();

    string ownerAsmdef = AsmdefResolver.OwningAsmdefName(uitkxFilePath);
    var union = new HashSet<string>(same, StringComparer.OrdinalIgnoreCase);
    foreach (var cs in index.GetAllCsFiles())
    {
        if (AsmdefResolver.OwningAsmdefName(cs) == ownerAsmdef)
            union.Add(cs);
    }
    return union.ToArray();
}
```

`AsmdefResolver.OwningAsmdefName` MUST be cached by directory (not by
file path) to avoid re-walking the tree per file. Cache key is the
nearest `.asmdef` directory; cache value is the asmdef name. Cache is
invalidated by `WorkspaceIndex.Refresh` when an `.asmdef` file changes
(rare event — adds <1 ms amortized to the union build).

---

## 4. Performance analysis (non-theoretical)

### 4.1 SG (#18 SG-side)

Cost added per pipeline run: one HashSet allocation + N inserts + N
lookups, where N is the number of peer hook containers in the asmdef
(typically 0 - 10 for real projects). Measured order: tens of
nanoseconds per inserted FQN. Stage 3d is invoked once per `.uitkx`
batch per Roslyn incremental run.

Net: indistinguishable from noise. SG suite total wall-clock change
will be within run-to-run jitter.

### 4.2 HMR (#18 HMR-side)

- Seed cost (one-time per HMR session): one
  `Directory.EnumerateFiles(projectRoot, "*.uitkx",
  AllDirectories)` + a `DirectiveParser.Parse` per file. PrettyUi
  currently has ~25 `.uitkx` files; parsing is ~0.5 - 2 ms per file
  (`DirectiveParser` is regex-based). Worst-case observed: ~50 ms
  once. Runs on a background `Task.Run` started from
  `UitkxHmrController.Start()` — invisible to the user unless they
  hit Ctrl+S within ~50 ms of entering Play mode (handled by the
  100 ms gate fallback, see §9.1).
- Per-edit cost: `HookContainerRegistry.Invalidate(path)` does ONE
  `DirectiveParser.Parse` on the changed file plus a dict update. This
  is `O(1)` in workspace size and dominated by file I/O (already paid
  by the file watcher event). Measured order: <1 ms.
- HMR recompile per save: `EmitCompanionUitkxSources` adds a `foreach`
  over the (small) registry list and one HashSet lookup per FQN. Adds
  <0.1 ms to the existing 100 - 300 ms HMR recompile. No new Roslyn
  compilation work. The 100 ms async-seed gate (see §9.1) is hit at
  most once per HMR session and only when the user saves within
  ~50 ms of entering Play mode.

Performance assertion (added to test plan §6): HMR recompile median
must not regress beyond the existing ±5% jitter band on the
`PrettyUi/MenuPage.uitkx` fixture.

### 4.3 IDE virtual doc (#18 VDG-side)

VDG `Generate(...)` already walks `d.Usings` once and writes one line
each. Adding N FQNs (N typically 0 - 5) is N writes plus N HashSet
ops. Negligible (sub-microsecond).

### 4.4 LSP (#19)

- WorkspaceIndex scan: existing scan already enumerates every `.cs` to
  call `IndexFile`. Adding to `_allCsFiles` is one HashSet insert per
  file. For a 500-file workspace: ~5 - 10 µs total (HashSet inserts on
  short strings amortize to ~10 - 20 ns each).
- `Refresh` per file change: identical structure, one HashSet op added.
- `FindCompanionFiles` first-open path: union of same-dir + asmdef-wide
  set. For a 200-file asmdef the union build is dominated by
  `AsmdefResolver.OwningAsmdefName` calls. With per-directory caching,
  this is `O(distinct directories)` - typically 20 - 40 dirs - and the
  uncached cost per directory is one stat + one regex match. First-open
  latency increase: bounded by ~5 - 10 ms on a cold cache, sub-ms on
  warm. Subsequent opens reuse the directory cache.
- `AddCompanionDocuments`: now reads N files instead of M (N > M).
  Each file is `File.ReadAllText` + `SourceText.From`. For a 200-file
  asmdef averaging 4 KB per file: ~800 KB read once per `.uitkx`
  open; on SSD this is single-digit ms. Workspace memory: an extra
  ~1 - 5 MB of source text per open `.uitkx`. Roslyn doesn't dedupe
  document text across `AdhocWorkspace` instances, so this scales
  linearly with the number of simultaneously open `.uitkx` files
  (rarely > 5 in practice).

LSP startup latency budget per `LATENCY_TARGETS.md`: first scan must
stay <2 s for 1k-file workspaces. Adding one HashSet write per file
keeps us well within budget; the union build runs lazily on first
`.uitkx` open, not at startup, so server-startup latency is unchanged.

### 4.5 Built product (player IL2CPP / Mono)

Files modified:
- `SourceGenerator~/**` — emits the same generated `.g.cs` (just with
  one more `using static` line on the cross-namespace path).
- `Editor/HMR/**` — `#if UNITY_EDITOR` gated by being inside the
  Editor asmdef. Never compiled into player.
- `ide-extensions~/**` — outside Unity compilation entirely.

Conclusion: zero net change to player IL, zero net change to player
DLL size, zero change to player startup or per-frame cost. The added
`using static` directives are erased at C# compile time.

---

## 5. Risk register

| Risk                                                                | Likelihood | Mitigation                                                                 |
| ------------------------------------------------------------------- | ---------- | -------------------------------------------------------------------------- |
| Cross-asmdef `using static` collision (two hook containers with the same simple name in two asmdefs that reference each other) | Low | Pre-scan filter is per-asmdef; `using static` is fully qualified. C# compiler picks the visible one. |
| Stale HMR registry after Unity reload                               | Low        | `HookContainerRegistry.Reset()` on controller stop; `Seed` rerun by `Start()` after reload. |
| First-save-in-Play-mode races background seed                       | Low        | 100 ms `ManualResetEventSlim` gate; on timeout, single compile uses companion-only fallback and logs once per session. |
| `.asmref` GUID cache stale after asmdef rename                      | n/a        | `.asmref` not supported in 0.5.12; deferred to follow-up. |
| Project has no `.asmdef` at all (e.g. PrettyUi)                     | n/a        | `AsmdefResolver` falls back to `Assembly-CSharp[-Editor]` per the existing SG contract — covered by parity test. |
| `_allCsFiles` write-lock nesting with `CommitElement`               | Low        | Always release write lock BEFORE calling `IndexFile`. Validated with `LockRecursionException` in test. |
| LSP first-open latency regression for large asmdefs (>1000 files)   | Med        | Lazy load (only on first `.uitkx` open); fall back to same-dir-only if union build exceeds 250 ms. |
| AsmdefResolver dir-cache miss on stale paths                        | Low        | Invalidate on `.asmdef` `FileSystemWatcher` events (already wired in WorkspaceIndex). |
| HMR registry seed blocks main thread on first edit                  | Low        | Seed runs once; budget is ~50 ms for current PrettyUi-scale workspaces. Acceptable for first-edit. Can be moved to background task if telemetry shows otherwise. |

No risk scored High. No risk impacts player builds.

---

## 6. Test plan

New tests:

1. `SourceGenerator~/Tests/HookUsingStaticCrossNamespaceTest.cs` — assert
   that a component in `Ns.A` gets `using static Ns.B.MyHooks;` when the
   peer hook container lives in `Ns.B` within the same asmdef.
2. `SourceGenerator~/Tests/HookUsingStaticCrossAsmdefTest.cs` — assert
   that a hook container in a different asmdef (controlled via fake
   `IsOwnedByCompilation` fixture) is NOT injected.
3. `SourceGenerator~/Tests/HmrEmitterParityTests.cs` (existing — extend)
   — assert HMR `using static` injection set equals SG injection set
   for a fixture with cross-namespace hooks.
4. `ide-extensions~/lsp-server/Tests/CrossDirectoryCompanionTest.cs` —
   open a `.uitkx` whose only reference to a type is to a `.cs` two
   directories up; assert no CS0103/UITKX squiggle, assert go-to-def
   resolves.
5. `ide-extensions~/lsp-server/Tests/AsmdefBoundaryCompanionTest.cs` —
   put a `.cs` in a different asmdef; assert it is NOT loaded as a
   companion (and the original CS0103 squiggle is preserved — opt-in
   to the asmdef boundary).
6. `ide-extensions~/lsp-server/Tests/WorkspaceIndexAllCsTest.cs` —
   add/remove `.cs` files; assert `GetAllCsFiles()` snapshot matches.

Existing tests to verify still pass unchanged:
- All 1222 SG suite tests.
- All 62 LSP suite tests.
- `HmrEmitterParityContractTests` (must extend, not break).

Performance soft-gate (logged, not asserted):
- HMR recompile median over 10 consecutive saves of
  `MenuPage.uitkx` in PrettyUi: log `result.TotalMs`, compare against
  baseline at HEAD.

---

## 7. Rollout

1. Implement §3.1 (SG) → run SG suite. Expected: 1222→1224 passing
   (two new tests #6.1, #6.2 added).
2. Implement §3.4 (`AsmdefResolver` x2 helpers) + parity test.
3. Implement §3.5 (LSP) → run LSP suite. Expected: 62→65.
4. Implement §3.2 (HMR registry + parity).
5. Implement §3.3 (VDG parity) → re-run LSP suite.
6. Manual validation in PrettyUi:
   - Remove the manual `@using static PrettyUi.UIHooks.UseUiDocumentSlotHooks;`
     from MenuPage.uitkx → confirm no squiggle, build green, HMR works.
   - Move `UIDocumentSlot.cs` to `Assets/Scripts/` → confirm no squiggle
     in MenuPage.uitkx (cross-directory `.cs` resolution).
7. Bump versions:
   - `package.json`: 0.5.11 → 0.5.12.
   - `ide-extensions~/vscode/package.json`: 1.2.7 → 1.2.8.
   - `ide-extensions~/visual-studio/source.extension.vsixmanifest`:
     1.2.7 → 1.2.8.
   - `ide-extensions~/changelog.json`: `node scripts/changelog.mjs add
     --scope shared --message-file <path> --vscode 1.2.8 --vs2022 1.2.8`.
8. Update `CHANGELOG.md` (library-side) with a single 0.5.12 entry
   listing both fixes.
9. Update `Plans~/DISCORD_CHANGELOG.md` (≤2000 chars per entry, ASCII
   only — see `.github/instructions/discord-changelog.instructions.md`).
10. Strike-through TECH_DEBT_V2 items #18 and #19 with the commit SHA.
11. Stage, commit (single commit covering both fixes), push.

---

## 8. Backwards compatibility

- Existing user code that already has `@using static <Ns>.<Container>;`
  continues to work — duplicate `using static` lines are deduped by the
  HashSet in §3.1.
- Existing same-namespace flow (the SG happy path) is unchanged: the
  new code emits the exact same `using static` line for that case.
- HMR companion-only behavior is preserved as a subset of the new
  registry-backed behavior. A user who has no cross-directory hooks
  sees byte-identical HMR output.
- LSP: first-open cost increases by single-digit ms per `.uitkx`; no
  behavior visible to the user other than fewer false-positive
  squiggles.

---

## 9. Resolved decisions (from review)

1. **HMR registry seed runs ASYNC on HMR `Start()`.** A `Task.Run(Seed)`
   plus a `ManualResetEventSlim`; the first
   `EmitCompanionUitkxSources` waits on the gate with a 100 ms
   timeout. If the gate is still unset after the timeout (rare —
   only on cold-disk huge workspaces), fall back to companion-only
   for that one compile and log the slow-seed event once per session.
   Rationale: bounded latency on the user's first save in Play mode,
   independent of workspace size. ~10 lines vs sync.
2. **`AsmdefResolver` ships the SG fallback contract verbatim.**
   The lifted resolver MUST handle the no-`.asmdef` case (e.g.
   PrettyUi, where everything lives in `Assembly-CSharp`):
   when the upward walk finds no `.asmdef`, return
   `"Assembly-CSharp-Editor"` if the path contains an `Editor/`
   segment, else `"Assembly-CSharp"`. This is identical to
   `UitkxPipeline.IsOwnedByCompilation`'s fallback at L320-L340.
   `.asmref` support is **not** in scope for 0.5.12 (will be filed
   as a separate tech-debt item if a user reports it). Rationale:
   the no-asmdef path is the most common shape in real Unity
   projects and must work without ceremony; `.asmref` is rare and
   adds a regex + GUID-table that pays for itself only if needed.
3. **LSP union has no hard cap.** Documented behavior: per-`.uitkx`-
   open companion scan cost scales with the number of `.cs` files in
   the owning asmdef. Add a paragraph to `Plans~/LATENCY_TARGETS.md`
   recommending asmdef splitting for monolith Assembly-CSharp. One
   informational `ServerLog.Log` line at >500 files in the asmdef
   gives users a breadcrumb but never refuses to load.

---

## 10. Files touched (final list)

Library:
- `SourceGenerator~/UitkxPipeline.cs` (Stage 3d edit)
- `SourceGenerator~/Tests/HookUsingStaticCrossNamespaceTest.cs` (new)
- `SourceGenerator~/Tests/HookUsingStaticCrossAsmdefTest.cs` (new)
- `SourceGenerator~/Tests/HmrEmitterParityContractTests.cs` (extend)
- `Editor/HMR/AsmdefResolver.cs` (new, ~30 lines)
- `Editor/HMR/HookContainerRegistry.cs` (new, ~120 lines)
- `Editor/HMR/UitkxHmrCompiler.cs` (replace EmitCompanionUitkxSources tail)
- `Editor/HMR/UitkxHmrController.cs` (registry wiring)

LSP / IDE:
- `ide-extensions~/lsp-server/AsmdefResolver.cs` (new, ~30 lines)
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` (`_allCsFiles`,
  `GetAllCsFiles`, hook-container parallel dict)
- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`
  (`FindCompanionFiles` extension; threading WorkspaceIndex into the call)
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`
  (`peerHookContainerFqns` parameter + emit loop)
- `ide-extensions~/lsp-server/Tests/CrossDirectoryCompanionTest.cs` (new)
- `ide-extensions~/lsp-server/Tests/AsmdefBoundaryCompanionTest.cs` (new)
- `ide-extensions~/lsp-server/Tests/WorkspaceIndexAllCsTest.cs` (new)

Versioning / docs:
- `package.json`
- `ide-extensions~/vscode/package.json`
- `ide-extensions~/visual-studio/source.extension.vsixmanifest`
- `ide-extensions~/changelog.json` (via `scripts/changelog.mjs`)
- `CHANGELOG.md`
- `Plans~/DISCORD_CHANGELOG.md`
- `Plans~/TECH_DEBT_V2.md` (strike #18 + #19, link to commit)
