# Tech Debt #20 / #21 / #22 — Resolution Plan

**Status:** Final — production-grade design, ready to implement. All Rank-5 go/no-go questions answered (see §5.2.1).
**Scope:** TECH_DEBT_V2.md items #20, #21, #22 (all marked CRITICAL / next-up).
**Author note:** This document supersedes the inline “Fix approach”
sketches in TECH_DEBT_V2.md for these three items. It is the result of
a manual code re-read across the SourceGenerator, HMR, LSP, language-lib,
and IDE-client layers and may be implemented incrementally without
rewriting any of the other layers.

---

## 0. Executive summary

The three bugs share **one underlying gap**: the toolkit has no
**workspace-wide, asmdef-aware, incrementally-maintained dependency
graph** for `.uitkx` files. The current symptoms are:

| Bug  | What the user sees                                                                       | Layer that breaks   |
| ---- | ---------------------------------------------------------------------------------------- | ------------------- |
| #20  | Edit a `module` value → consumer keeps rendering the old value until full reload         | HMR (cctor capture) |
| #21  | Duplicate `component <Name>` → IntelliSense randomly evicts one; rename breaks both      | LSP (index shape)   |
| #22A | Create a new `.cs` helper, reference it from `.uitkx` → HMR loops on CS0246              | HMR (no .cs scan)   |
| #22B | Refactor parent/child/grand-child props together → HMR loops on CS0117 across the tree   | HMR (no dep graph)  |

This plan delivers them as **five ranked, incrementally-shippable
deliverables** (Ranks 1–5). Each rank is a self-contained PR with its
own tests and changelog entry. Ranks share a small piece of new
infrastructure (`UitkxFileDependencyIndex`) that closes the gap above
once and is reused everywhere it’s needed afterwards.

### Rank order & bug mapping

| Rank | Title                                            | Closes | Risk    | Status of design |
| ---- | ------------------------------------------------ | ------ | ------- | ---------------- |
| 1    | LSP: multi-valued element index + UITKX0107      | #21    | Low     | Confirmed        |
| 2    | HMR: new `.cs` pickup via workspace scan         | #22A   | Low     | Confirmed        |
| 3    | HMR: module reverse-dep cascade (`_moduleDependents` + `UitkxFileDependencyIndex`) | #20 | Medium | Confirmed |
| 4    | HMR: transitive recompile queue (real queue + topological cascade)                 | #22B (partial) + foundation for Rank 5 | Medium | Confirmed |
| 5    | HMR: cross-`.uitkx` signature cascade (HMR-DLL ref injection + extern-alias guard) | #22B (full)  | **HIGH** — committed (default on, no SCC cap, loud-fallback) | Designed, see §5 risk matrix |

Ranks 1, 2, 3 are independent and can ship in any order.
Rank 4 depends on Rank 3 (reuses the dep index and the new compile queue).
Rank 5 depends on Rank 4 (cannot inject HMR-DLL refs without the queue).

A sixth, optional sub-fix (analyzer `UITKX0211` — const in modules) is
listed in §6 and can ship with Rank 3.

---

## 1. Shared infrastructure — `UitkxFileDependencyIndex`

A workspace-wide, asmdef-aware dependency index for `.uitkx` files,
shared by Ranks 3 / 4 / 5. Pattern is **deliberately identical** to
`HookContainerRegistry` ([Editor/HMR/HookContainerRegistry.cs](../Editor/HMR/HookContainerRegistry.cs))
so reviewers don’t have to learn a new lifecycle:

- Static class.
- Seeded asynchronously on HMR `Start()` via `Task.Run`; a
  `ManualResetEventSlim` lets the first HMR compile block briefly
  (≤ 100 ms) for completeness.
- Invalidated per-file from `UitkxHmrFileWatcher.OnUitkxChanged`.
- Reset on HMR `Stop()`.
- Thread-safe via `ReaderWriterLockSlim`.
- Skips tilde folders via the same `IsInsideTildeFolder` rule.
- Uses `AsmdefResolver.OwningAsmdefName(path)` so all queries are
  asmdef-scoped (mirrors the SG’s `IsOwnedByCompilation`).

### State

```csharp
private struct FileNode
{
    public string Asmdef;                   // e.g. "Assembly-CSharp"
    public string DeclaredNamespace;         // from @namespace
    public string ComponentName;             // null for module/hook files
    public HashSet<string> DeclaredModules;  // top-level `module X` names
    public HashSet<string> ReferencedModules;// FQN-resolved `X.Member` reads
    public HashSet<string> ReferencedComponents; // JSX `<Foo>` tag names
}

// path → node
private static readonly Dictionary<string, FileNode> s_byPath = …;
// reverse: module name → set of paths that reference it
private static readonly Dictionary<string, HashSet<string>> s_moduleReverse = …;
// reverse: component name → set of paths that consume it (JSX)
private static readonly Dictionary<string, HashSet<string>> s_componentReverse = …;
```

### Parsing strategy

Cheap text scan in `TryIndexFile`. **Do not** require Roslyn or the
SG’s parser to run here — Seed must be fast across ~1000 .uitkx files
in large projects.

- `@namespace Foo.Bar;` → single-line regex.
- `module FooBar {` and `component FooBar(` → existing regex from
  `WorkspaceIndex.IndexUitkxFile` ([ide-extensions~/lsp-server/WorkspaceIndex.cs](../ide-extensions~/lsp-server/WorkspaceIndex.cs#L518)).
  Reuse via a shared helper in `Shared/` or copy with the same parity-
  test discipline that AsmdefResolverParityTests already uses.
- Module member references — gated regex: enumerate **known** module
  names (built from the same index) and find token-bounded matches of
  `\bName\.\w+`. Avoids false positives on string literals via a
  pre-strip pass (same trick `s_uitkxModuleOrHookPattern` uses).
- JSX tag references — token-scan `<TagName` where `TagName` begins
  with an uppercase letter (function components). Lowercase tags are
  built-ins.

### Cycle handling

`module Theme` referencing `Util.X` while `module Util` references
`Theme.Y` is legal and currently works (cold build does both cctors
once). The cascade walker (Rank 3) must therefore detect cycles and
break them by stopping at any node already in the current batch (SCC
collapse, not Tarjan — we just need termination). Test coverage
required.

### Why a new file rather than extending `HookContainerRegistry`

`HookContainerRegistry` is single-purpose (asmdef → hook FQN list) and
its API has already been ratified by the cross-file pass in SG’s
`UitkxPipeline.cs` L237. Mixing module dependency state into it would
muddle two unrelated lifecycles. The new file co-locates under
`Editor/HMR/`, with the same `#if UNITY_EDITOR` guard.

---

## 2. Rank 1 — LSP: multi-valued element index + UITKX0107

**Bug closed:** #21 in full.
**Risk:** Low.
**Estimated touch surface:** ~120 lines added + 1 new test fixture.

### Root cause (confirmed)

`WorkspaceIndex._elementInfo` is `Dictionary<string, ElementInfo>`
([ide-extensions~/lsp-server/WorkspaceIndex.cs](../ide-extensions~/lsp-server/WorkspaceIndex.cs#L60)).
`CommitElement` ([L715](../ide-extensions~/lsp-server/WorkspaceIndex.cs#L715))
unconditionally assigns `_elementInfo[elementName] = info`, so the
second `.uitkx` declaring the same name wins. `EvictElementsFromFile`
([L760](../ide-extensions~/lsp-server/WorkspaceIndex.cs#L760)) then
removes the entry on the winner’s next edit because its `FilePath`
check matches, deleting the shared name globally even though the
original file still declares it on disk.

### Design

**Change the shape:**

```csharp
// Before:
private readonly Dictionary<string, ElementInfo> _elementInfo = new(StringComparer.Ordinal);

// After:
// element name → file path → info  (file path is the inner key for
// deterministic ordering when there are duplicates).
private readonly Dictionary<string, SortedDictionary<string, ElementInfo>>
    _elementInfo = new(StringComparer.Ordinal);
```

`SortedDictionary` for the inner map gives stable “first by file path”
selection when callers want a single answer.

**Update the accessor API to be explicit about multiplicity:**

```csharp
// Backwards-compatible — returns the first deterministic match.
public ElementInfo? TryGetElementInfo(string name);

// New — for diagnostics and Rename.
public IReadOnlyList<ElementInfo> GetAllElementInfo(string name);

// Optional asmdef-aware overload for future callers (no churn in
// existing handlers; they can opt in incrementally).
public ElementInfo? TryGetElementInfo(string name, string asmdef);
```

**Update `EvictElementsFromFile`** to remove only the per-file inner
entry and only drop the outer name when the inner dict becomes empty.

**Add diagnostic `UITKX0107` — Duplicate component declaration.**
Published from `DiagnosticsPublisher` when any element name has > 1
file *in the same asmdef*. Severity: Warning. Asmdef-scoped because
duplicate names across asmdefs are legal (separate compilations).

### Call-site audit

All readers go through `WorkspaceIndex` methods today (verified via
grep — 19 hits, all internal). Touch sites:

- `DefinitionHandler.cs` L106, L135 — call `TryGetElementInfo`, no
  change. Optionally call `GetAllElementInfo` when the user invokes
  go-to-definition on a duplicated name to surface a quick-pick
  (future polish, not in this rank).
- `HoverHandler.cs` L129 — no change.
- `RenameHandler.cs` L152, L360 — **must** call `GetAllElementInfo`
  so renaming `Card` updates *every* file that declares it. Today it
  would silently skip the non-winning files.
- `ReferencesHandler.cs` L144 — no change (already iterates all
  references regardless of declaration count).
- `PropsTypeAdapter.cs` L48 — no change. Picks the deterministic
  first; behavior matches the SG’s “first wins in file order” rule
  for ambiguous props resolution (compile-time semantics are the
  authoritative spec).

### Tests

New file `ide-extensions~/lsp-server/Tests/DuplicateElementIndexTests.cs`:

1. **Folder-copy regression** (verbatim PrettyUi repro from
   TECH_DEBT_V2.md §21):
   1. Index file A: `component Ammunition { … }`.
   2. Index file B (copy of A).
   3. Edit B → `component Stats { … }` and call `Refresh(B)`.
   4. Assert `HasElement("Ammunition") == true` (file A still wins).
   5. Assert `HasElement("Stats") == true`.
2. **Duplicate diagnostic UITKX0107** — fires once per
   (name, asmdef) pair, suppressed across asmdefs.
3. **Eviction correctness** — delete file A; assert `Ammunition`
   gone if no other declarant; still present if file C also declares
   it.
4. **Rename touches all declarants** — `RenameHandler` integration
   test (mock LSP client) verifying `GetAllElementInfo` flows
   through to a multi-file `WorkspaceEdit`.

### Risk matrix

| Threat                                             | Mitigation                                                                |
| -------------------------------------------------- | ------------------------------------------------------------------------- |
| Hidden caller dereferences raw `_elementInfo`      | Confirmed via workspace grep — only WorkspaceIndex itself touches it.     |
| `ResolveProps` (depth ≤ 5 base chain) needs the multi-valued shape | `ResolveProps` walks by name; calls `TryGetElementInfo` recursively. Update to use deterministic first match. Behavior identical when no duplicates. |
| `_elementsByFile` reverse map still owns the truth | Already independent; no change needed.                                    |
| Diagnostic spam in big projects                    | UITKX0107 fires at most once per (name, asmdef) per publish cycle.        |

### Alternatives considered & rejected

- **Tagging element names with the file path inside the key** —
  breaks every consumer’s `name` lookup.
- **First-write-wins lock (refuse to overwrite)** — leaves the
  user confused why their newly added second file gets no IntelliSense.
- **Re-scan workspace on every Evict** — current “mitigation B”
  in TECH_DEBT_V2.md §21. Still ships as belt-and-braces *inside*
  `EvictElementsFromFile` only when the inner dict becomes empty:
  cheap and protects against future regressions.

---

## 3. Rank 2 — HMR: new `.cs` pickup

**Bug closed:** #22 Failure Mode 1.
**Risk:** Low.
**Estimated touch surface:** ~80 lines added + 1 new test fixture.

### Root cause (confirmed)

`UitkxHmrCompiler.BuildReferenceList` ([Editor/HMR/UitkxHmrCompiler.cs](../Editor/HMR/UitkxHmrCompiler.cs#L730))
walks `AppDomain.CurrentDomain.GetAssemblies()` only. New `.cs` files
not yet in any loaded DLL are invisible. `UitkxHmrFileWatcher`
([Editor/HMR/UitkxHmrFileWatcher.cs](../Editor/HMR/UitkxHmrFileWatcher.cs#L135-L142))
maps `.cs` events to `.uitkx` only when the `.cs` file lives in the
**same directory** as a `.uitkx` (`FindAssociatedUitkx`). Both gaps
must be closed.

### Design

**A. Watcher enhancement — pick up `.cs` files anywhere in the asmdef.**

Replace the same-directory filter in `FindAssociatedUitkx` with:

1. If the `.cs` file lives in the same directory as a `.uitkx`,
   keep current behaviour (cheapest, dominant case).
2. Otherwise, resolve the `.cs` file’s owning asmdef via
   `AsmdefResolver.OwningAsmdefName(csPath)`. Mark **every** open
   `.uitkx` belonging to that asmdef as needing recompile? — **No,
   that’s too aggressive.** Instead:
3. Compute the `.cs` file’s declared types via the same cheap regex
   as `WorkspaceIndex.s_classPattern`. Look up reverse-references via
   `UitkxFileDependencyIndex.s_componentReverse` + a new
   `s_typeReverse` populated by the .uitkx scan (token search for
   capitalised identifiers in setup-code blocks). Enqueue only the
   .uitkx files that actually reference the new type’s names.

Practical fallback when reverse lookup misses: also bump a “new .cs
generation counter” the next HMR compile reads; if non-zero,
`UitkxHmrCompiler.Compile` enumerates *all* asmdef-scoped `.cs`
files (cheap, ≤ 500 files in PrettyUi) and includes any whose
`LastWriteTimeUtc > _projectDllCompileTime`. This guarantees a
correct (possibly slower) first save, then the reverse-ref index
takes over for subsequent saves.

**B. Compiler enhancement — include new .cs as additional SyntaxTrees.**

In `UitkxHmrCompiler.Compile`, before calling `InProcessCompile`:

```csharp
// New: enumerate asmdef-scoped .cs files not in the project DLL
var extraCs = NewCsFileDiscovery.FindForAsmdef(asmdef,
    _projectDllCompileTime, knownTypeNames: existingTypesInProjectDll);
sources = MergeWithCompanions(sources, extraCs);
```

The discovery class lives in `Editor/HMR/NewCsFileDiscovery.cs`,
uses `AsmdefResolver`, and is asynchronously refreshed alongside
`UitkxFileDependencyIndex.Seed`.

**Type-name dedupe (critical).** If a `.cs` file declares a type that
already exists in the project DLL, including it as a fresh
SyntaxTree causes `CS0101` (duplicate type). Discovery must **only**
include `.cs` files whose top-level types are **not** present in the
project DLL, checked via Roslyn `compilation.GetTypeByMetadataName`.
That check must run inside the HMR compilation pipeline (after the
Roslyn `Compilation` is constructed), not in the watcher.

### Tests

New file `Editor/HMR/Tests/NewCsFileDiscoveryTests.cs`:

1. **Same-folder helper** — add `PlayerStats.cs` next to
   `GamePage.uitkx`; save GamePage; assert HMR succeeds.
2. **Cross-folder helper** — add `Utils/StatsHelpers.cs` in same
   asmdef but different directory; save GamePage referencing it;
   assert HMR succeeds without manual second save.
3. **Asmdef boundary** — `.cs` in a different asmdef must **not**
   be picked up (would violate Unity’s assembly partitioning).
4. **Already-loaded type** — type already in project DLL must not be
   redeclared (no CS0101).
5. **Watcher path coverage** — verify both
   FileSystemWatcher and AssetPostprocessor paths route through the
   new logic.

### Risk matrix

| Threat                                                | Mitigation                                                    |
| ----------------------------------------------------- | ------------------------------------------------------------- |
| `CS0101` from accidental redeclaration                | Roslyn type-name check before adding SyntaxTree.              |
| HMR compile slowdown on save                          | Discovery cached + asmdef-scoped; warm path ≤ 5 ms expected.  |
| `.cs` file under a tilde folder picked up             | Reuse `IsInsideTildeFolder` everywhere.                       |
| Race between asset import and our pickup              | Editor pump runs after AssetPostprocessor — already ordered.  |

### Alternatives considered

- **Forcing Unity to recompile first** — defeats the purpose of HMR
  (would release the assembly-reload lock).
- **Treating every save as a full reload trigger** — same.
- **Manual user opt-in via UI button** — bad UX; the user shouldn’t
  have to know about the discovery.

---

## 4. Rank 3 — HMR: module reverse-dep cascade

**Bug closed:** #20 in full (subject to §6 const-in-module follow-up).
**Risk:** Medium.
**Estimated touch surface:** ~250 lines (controller + dep index + tests).

### Root cause (confirmed)

`UitkxHmrModuleStaticSwapper.SwapModuleStatics` already copies
`[UitkxHmrSwap]` field *values* across the HMR boundary
([UitkxHmrModuleStaticSwapper.cs](../Editor/HMR/UitkxHmrModuleStaticSwapper.cs#L194))
**but only for fields whose initializer expression runs in the HMR
assembly’s cctor**. The PrettyUi repro proves a transitive case:

```
Theme.uitkx:        public static readonly Color Accent = ...;
StatsPanel.style.uitkx:
                    public static readonly Style Container =
                        new Style { BorderColor = Theme.Accent, ... };
```

Recompiling `Theme.uitkx`:
1. Builds an HMR `Theme` type with the new `Accent`.
2. `SwapModuleStatics` copies HMR-`Theme.Accent` → project-`Theme.Accent`. ✓
3. **But** `StatsPanel.Container` was captured at cold-build cctor
   time, reading the *original* `Theme.Accent`. Nothing re-runs
   `StatsPanel.Container`’s initializer because `StatsPanel.style.uitkx`
   was not recompiled. ✗

Required fix: cascade `Theme.uitkx`’s recompile to every transitive
consumer file, in dep-graph topological order, so each consumer’s
cctor re-runs against the freshly-updated `Theme.Accent` and updates
its own derived fields.

### Design

Three parts:

**A. Use `UitkxFileDependencyIndex` (from §1)** to populate
`s_moduleReverse` at Seed time and incrementally maintain it from
the watcher.

**B. Cascade walker in `UitkxHmrController`** — replaces the current
direct call to `ProcessFileChange(uitkxPath)`:

```csharp
var batch = new List<string>();
TopoCollectTransitive(uitkxPath, batch); // includes self last
foreach (var f in batch)
    ProcessFileChange(f);
```

`TopoCollectTransitive` walks `s_moduleReverse` and
`s_componentReverse` (Rank 4 reuses this), breaking on already-in-
batch nodes (cycle break). Files outside the changed file’s asmdef
are excluded (cross-asmdef cascade is not legal in Unity anyway —
each asmdef is a separate compilation unit).

**C. Compile queue (real queue, not single slot).** Today
`_compilationQueued`/`_queuedPath` ([UitkxHmrController.cs](../Editor/HMR/UitkxHmrController.cs#L41-L42))
is a 1-slot replace-on-write design that drops in-flight saves. The
cascade now produces N-file batches that must execute in order, so:

```csharp
private readonly Queue<string> _compileQueue = new();
private bool _compilationInProgress;
```

Enqueue from the watcher (deduped per-tick via the existing debounce
buffer) and from `TopoCollectTransitive`. Drain on every editor
update tick. Preserves the “one in-flight compile at a time” design
(HMR Roslyn isn’t thread-safe across compiles in our setup).

### Side-effect concerns

The “re-run cctor” model means **every** module cctor side-effect
(event subscriptions, registry pushes) re-runs each cascade tick.
This is already a known limitation of the `[UitkxHmrSwap]` mechanism
(documented in `UitkxHmrModuleStaticSwapper.cs` L132-L137); cascade
amplifies the radius. **Mitigation:** add a static `s_warned`
sentinel + console hint when the swapper detects that a module type’s
cctor would run > 1× in a single batch (i.e. its file is in the batch
multiple times via cycles) — guides the user toward refactoring
side-effects out of module bodies.

### Tests

New file `Editor/HMR/Tests/ModuleCascadeTests.cs`:

1. **PrettyUi repro** — Theme + StatsPanel.style + StatsPanel.
   Save Theme; assert all three recompile in order; assert
   StatsPanel.Container.BorderColor reads the new Theme.Accent.
2. **Three-deep chain** — A → B → C; save A; all three recompile.
3. **Cycle break** — A ↔ B; save A; both recompile exactly once.
4. **Cross-asmdef boundary** — A in asmdef X consumed by B in
   asmdef Y; save A; only X-side files recompile; B is unchanged
   (Unity would recompile Y on its own next reload).
5. **Single-slot queue regression** — fire 5 saves within debounce
   window; assert all are coalesced; fire 5 saves across compile
   gaps; assert all 5 compile (no drops).

### Risk matrix

| Threat                                                          | Mitigation                                                          |
| --------------------------------------------------------------- | ------------------------------------------------------------------- |
| Dep-graph false positives (regex matches in string literals)    | Strip strings/comments before regex; reuse SG’s `s_uitkxModuleOrHookPattern` style. |
| Side-effect re-run on cascade (subscription duplication)        | New console hint + documented in HMR docs; not a regression — already exists for single-file recompile. |
| Cascade balloons to whole project (worst case Theme reference)  | Asmdef boundary + tilde filter caps blast radius; if still too wide, add per-file cooldown (200 ms) for noisy modules. |
| Seed cost on large projects                                     | Async background seed (≤ 100 ms wait on first compile); HookContainerRegistry proved this model viable. |
| Queue starvation if a single compile hangs                      | Reuse existing infrastructure failure self-disable (`_loggedInfrastructureFailure`). |

### Alternatives considered

- **Force re-run of consumer cctors via reflection without
  recompile** — would also re-fire side-effects, *and* leaves the
  generated IL of the consumer untouched, so any new SG behavior
  (e.g. a fix landing in 0.6) wouldn’t take effect.
- **Walk only direct consumers, not transitive** — would leave
  3-deep chains broken (verified in PrettyUi repros).
- **Emit a “stale” warning instead of fixing** — bad UX; HMR
  promise is invisible updates.

---

## 5. Ranks 4 & 5 — HMR: transitive recompile + cross-`.uitkx` signature cascade

**Bugs closed:** #22 Failure Mode 2 (full).
**Risk:** Medium (Rank 4) → **HIGH** (Rank 5).

### 5.1 Rank 4 — Transitive recompile queue

This is the **structural foundation** for Rank 5 and a meaningful
partial fix for #22B on its own.

Today, on save of a child whose prop signature changed, only the
child is recompiled into an HMR DLL. The parent is never enqueued
unless the user manually saves it.

After Rank 3 lands, `UitkxFileDependencyIndex.s_componentReverse`
already tracks JSX consumers. Rank 4 reuses the topological walker:

- Save of `<Child>` ⇒ enqueue Child + every transitive parent that
  uses `<Child>` in its JSX.
- Each compile in the queue references the prior HMR DLLs explicitly
  (this requires Rank 5 to fully work — see below).

**What Rank 4 fixes on its own:** when the user’s edit is **body-only
inside Child** (no prop-signature change), the parent recompile picks
up Child via the project DLL’s unchanged Child type, **plus** the
HMR-DLL-of-Child’s new render delegate via the existing trampoline
swap. Parent rendering is immediately correct; no `CS0117`.

**What Rank 4 does NOT fix:** when Child’s prop **signature** changes,
parent recompile tries to emit `new ChildProps { newProp = … }`. The
parent compiles against the project DLL’s old `ChildProps` (which
has no `newProp`), so it fails with `CS0117`. That requires Rank 5.

#### Files

- `Editor/HMR/UitkxHmrController.cs` — extend the queue and cascade
  walker from Rank 3 to also pull from `s_componentReverse`.
- No new file needed; all infrastructure already in place after Rank 3.

#### Tests

- **Body-only cascade** — edit Child’s render body; assert parent
  HMR compile succeeds and re-renders.
- **Drop regression** — single-slot queue would lose mid-flight
  cascade members; assert all members compile.

### 5.2 Rank 5 — Cross-`.uitkx` signature cascade

This is the bug the user explicitly asked about: *“fixes #22 Failure
Mode 2 (cascading prop signatures).”* It is the **highest-risk** item
in this plan and the only one whose design includes alternatives that
materially change the HMR architecture.

#### The wall (confirmed)

`UitkxHmrCompiler.BuildCrossRefs` ([UitkxHmrCompiler.cs](../Editor/HMR/UitkxHmrCompiler.cs#L1322))
filters HMR-DLL refs to **components not present in the project DLL**
(`_genuinelyNewComponents`). The reason is `CS0433` — if both the
project DLL and an HMR DLL define `ChildProps`, the compiler can’t
pick one and errors out. Suppressing `CS0436` (already done at
[L1489](../Editor/HMR/UitkxHmrCompiler.cs#L1489)) covers only the
warning form, not the hard-error form.

Therefore: to make the parent see the *new* `ChildProps` shape we
must either (a) make sure the project-DLL `ChildProps` is **not in
scope** during the parent’s HMR compile, or (b) use `extern alias`
to disambiguate, or (c) avoid emitting `ChildProps` into the HMR DLL
in the first place (impractical — the SG always emits it).

#### Three implementable strategies

**Strategy A — Side-by-side namespace.** When SG emits to HMR, mangle
the namespace: `Foo.Bar` → `__HMR.Foo.Bar`. The consumer HMR compile
adds `using Foo = __HMR.Foo;` (or per-name `using ChildProps =
__HMR.Foo.Bar.ChildProps;`). No `CS0433` because the types live in
different namespaces. Reversible — `__HMR.` namespace prefix is purely
HMR-internal.

  - **Pros:** No language-feature dependency; readable; easy to debug
    via `Debug.Log(typeof(ChildProps).Namespace)`.
  - **Cons:** Every generated `.g.cs` SG emit path needs a knob;
    cross-references inside HMR DLL (HMR-Foo references HMR-Bar)
    must follow the same mangling. Touches every emitter file.

**Strategy B — `extern alias`.** Tag the HMR-DLL reference with
`extern alias HMR;` in the consumer’s HMR compile. Source generators
already understand `extern` so the SG can emit `extern alias HMR;
using ChildProps = HMR::Foo.Bar.ChildProps;`. The CLR has supported
this since C# 2.0.

  - **Pros:** Standard CLR mechanism, well-documented, no namespace
    mangling.
  - **Cons:** Roslyn’s `MetadataReference` API requires per-reference
    aliases passed at `CompilationOptions` time, *not* in source.
    `UitkxHmrCompiler` already uses reflection to talk to Roslyn —
    the alias plumbing adds another reflected surface. Plus: every
    parent in the cascade needs the alias added to its emit, which
    means the **HMR emitter** (not the SG) must rewrite the parent’s
    code per-compile. Significant emitter complexity.

**Strategy C — Per-asmdef HMR DLL union.** Instead of one HMR DLL per
.uitkx, collect every changed .uitkx in the asmdef into a single HMR
DLL. The parent and child are emitted into the same compilation, so
their `ChildProps` is unique within the assembly — no `CS0433` because
the project DLL’s `ChildProps` is shadowed by the HMR DLL’s
(`CS0436` warning, suppressed) and the loaded copy is the HMR one.

  - **Pros:** Architecturally cleanest. The CLR loads types from
    whichever assembly the consumer was *compiled against*; if we
    compile parent against HMR-DLL-union, parent sees HMR-ChildProps
    universally. No alias gymnastics.
  - **Cons:** Forces all changed files in the asmdef into one
    compilation per HMR tick — defeats per-file isolation. Compile
    times grow O(batch-size). Failure mode: one bad file in the
    batch breaks the whole tick.
  - **Mitigation:** only union files in the same dep-graph SCC,
    keep independent saves in independent HMR DLLs. Most edits are
    leaf-only; only refactors with cross-file signature changes
    incur the union.

#### Recommendation

**Strategy C (per-asmdef union of SCC).** Reasoning:

1. The other two strategies require either touching every SG emit
   path or adding new reflected Roslyn surfaces. Strategy C is
   surgical: it changes how `UitkxHmrCompiler.Compile` aggregates
   its input set, nothing else.
2. The dep-graph from Rank 3 already gives us the SCC for free.
3. Failure isolation is acceptable: if the user is doing a
   coordinated cross-file refactor, they expect coordinated failure
   reporting.
4. Strategy A is a viable fallback if Strategy C reveals unforeseen
   issues — kept as Plan B in the risk matrix below.

#### 5.2.1 Decisions (committed 2026-05-18)

All three Rank-5 design questions were answered by the maintainer.
Design sections below reflect these decisions; no further sign-off
required before implementation.

1. **Justification — Ship Rank 5 with default-on behaviour.** Real
   confirmation: maintainer hit the cascading-prop scenario within
   ~30 minutes of normal PrettyUi work. Coordinated multi-file
   refactors are a routine, not-edge-case workflow. Rank 5 is **not**
   gated behind an EditorPref opt-in — it ships enabled.
   *(Implication for Phase 5 below: the EditorPref-gated rollout in
   the original draft is dropped. See §8 for the updated phasing.)*

2. **Failure budget — Prevent first, fall back loudly.** Engineering
   priority is to make Rank 5 *never* silently mis-bind types. The
   compiler path must include defensive guards (assembly identity
   checks, type-name uniqueness asserts inside the union compile,
   post-compile sanity checks comparing `Type.Assembly` for each
   IProps emit). **When any guard trips, abort the union compile,
   surface the same `CS0117`/`CS0246`/`CS0433` error the user sees
   today, and recommend Stop + Restart.** Loud regression is
   preferred over silent wrong-IL.
   *Concrete guards added to the implementation checklist (§10):*
   - Pre-compile: every type in the union batch must have a unique
     `(Namespace, Name)` pair across all input trees.
   - Post-compile: assert every emitted IProps type’s
     `Assembly == hmrAssembly` for all types referenced by the
     parent’s render call sites.
   - On assertion failure: bail to the existing per-file compile
     path, let the standard error surface; log
     `[HMR] union-compile sanity check failed — falling back`.

3. **Compile-time budget — No SCC cap; correctness over latency.**
   Maintainer accepts arbitrarily large coordinated batches. The
   Strategy-A namespace-mangling spill path is **dropped from the
   design**: there is no SCC size at which we fall back to a
   different strategy. Reasoning: the cascade only fires when the
   user *intentionally* saves many files; the resulting compile time
   is bounded by what the user just asked for. A 30-file refactor
   producing a ~1.5 s HMR tick is acceptable; a 100-file refactor
   producing a ~5 s tick is acceptable. Optimisation passes can
   come later (incremental Roslyn compilation reuse — `TryBuildIncremental`
   in `UitkxHmrCompiler.cs` L1357 already exists for the per-component
   case and can be extended to unions in a follow-up).
   *Telemetry to add:* log the SCC size and union compile-time of
   every union compile at `[HMR] union: N files, M ms`. If user
   reports surface unusable latencies, the data tells us where to
   focus.

### 5.3 Tests (Rank 4 + 5 combined)

New file `Editor/HMR/Tests/SignatureCascadeTests.cs`:

1. **Body-only parent cascade** (Rank 4 only) — body edit propagates.
2. **Prop addition cascade** (Rank 5) — add prop on Child, save
   Child, save Parent passing the new prop; assert single HMR tick
   reconciles both.
3. **Prop deletion cascade** (Rank 5) — delete prop on Child, save
   Parent removing the now-illegal attribute; assert single HMR tick
   reconciles both. (Order-dependent — Parent save must come after
   Child save in the user’s flow; cover both orderings.)
4. **Three-deep refactor** — Grandparent → Parent → Child all
   change props together.
5. **SCC isolation regression** — independent leaf save during an
   in-flight cascade compile doesn’t get merged into the SCC.
6. **CS0433 absence** — verify no `error CS0433` ever surfaces from
   the compile output for any of the above.

### 5.4 Risk matrix (Rank 5)

| Threat                                                              | Mitigation                                                                          |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Per-SCC union compile fails on one bad file → loses N working files | Already current behaviour for body-cascade today (parent fails, child loses re-render too); documented; on failure, fall back to per-file path so each file emits its own error in isolation (per §5.2.1 decision 2). |
| `CS0436` floods the user’s console when union shadows project DLL   | Already suppressed via `-nowarn:0436` at [L1489](../Editor/HMR/UitkxHmrCompiler.cs#L1489); verify the in-process Roslyn path applies the same suppression. |
| Mono JIT caches `Type` references that now resolve to HMR DLL types | Trampoline + delegate swap already handle this for render fns; verify prop classes (rare path — accessed via `IProps` boxed reference, no JIT inline). |
| Compile-time growth on big SCCs                                     | No cap (per §5.2.1 decision 3). Telemetry `[HMR] union: N files, M ms` logged for every union compile; optimisation comes later via extending `TryBuildIncremental` to unions. |
| Side-by-side prop class identity confuses Equals/GetHashCode        | Generated `Equals` uses `obj is FooProps other` — identity check is per-assembly. Cross-assembly references already break this today; Rank 5 *fixes* it by ensuring all consumers see the same HMR-DLL FooProps. |
| Silent type mis-binding (the catastrophic-regression scenario)      | Pre-compile name-uniqueness check + post-compile `Assembly == hmrAssembly` assertion (per §5.2.1 decision 2). On any failure: bail to per-file path, let standard errors surface, log fallback. |

---

## 6. Optional sub-fix — `UITKX0211` (const in modules)

**Bug closed:** TECH_DEBT_V2.md §20 root cause (c) — module `const`
values are inlined into every consumer’s IL at compile time and
cannot be HMR-cascaded.

**Confirmed:** `SourceGenerator~/Emitter/StaticReadonlyStripper.cs`
[L60 `IsStripCandidate`](../SourceGenerator~/Emitter/StaticReadonlyStripper.cs#L60)
explicitly excludes `const` — proving the gap is intentional and the
right fix is an analyzer, not a stripper change (stripping `const` →
`static readonly` would break code that uses module constants as
attribute arguments or case labels).

**Design.** New analyzer file
`SourceGenerator~/Analyzers/UitkxModuleConstAnalyzer.cs`.

- ID: `UITKX0211`.
- Severity: **Warning** (committed 2026-05-18). The HMR-invisibility
  this analyzer warns about is a real foot-gun — Info severity is
  too easy to ignore in busy Problems panels, and the code fix
  (replace `const` with `static readonly`) is a one-keystroke
  remediation, so the warning is actionable rather than nagging.
- Trigger: top-level `const` inside `module <Name> { … }` body.
- Message: *“`const` values in module bodies are inlined at compile
  time and will not update under HMR. Use `static readonly` for HMR-
  cascading values.”*
- Code fix: replace `const` keyword with `static readonly`.

Tests: extend `SourceGenerator~/Tests/StaticReadonlyStripperTests.cs`
with two cases (const should trigger, static readonly should not).

This ships with Rank 3 or as a standalone follow-up.

---

## 7. Cross-cutting concerns

### 7.1 Asmdef-boundary discipline

Every rank above uses `AsmdefResolver.OwningAsmdefName(path)`. The
parity test (`SourceGenerator~/Tests/AsmdefResolverParityTests.cs`)
must be extended to cover any new path normalisation logic added by
this plan. **Do not** introduce a third asmdef walker.

### 7.2 Tilde-folder discipline

`Plans~`, `Samples~`, `GeneratedPreview~`, `SourceGenerator~`, etc.
must never be indexed. Every new file scanner above must reuse the
canonical helper (`UitkxHmrFileWatcher`-style path-segment split).
A shared helper in `Shared/UitkxPathPolicy.cs` (new file) is worth
extracting if more than two scanners are added.

### 7.3 Editor-only guard

Everything under `Editor/HMR/` must be `#if UNITY_EDITOR`-gated.
Verified in current files; new files must follow.

### 7.4 Logging

Use `Debug.Log("[HMR] …")` for cascade announcements (matches
existing style). For high-frequency events (per-file enqueue) use the
existing `VerboseWatcherTrace` EditorPref so default console stays
clean.

### 7.5 Test infrastructure

The HMR test layer historically uses `EmitterTests` /
`HmrEmitterParityContractTests` for SG↔HMR parity. Cascade tests
need a different harness — they need a live `UitkxHmrController` +
file-system manipulation. Recommend:

- New test class `Editor/HMR/Tests/CascadeTestHarness.cs` that:
  1. Creates a temp `Assets/_CascadeTests/` directory.
  2. Writes seed `.uitkx` files.
  3. Starts the controller against that directory only (subclass
     `UitkxHmrController` with a settable `_watchRoot`).
  4. Asserts compile outcomes from the controller’s state.
- Tear-down deletes the temp directory.

This harness covers Ranks 3, 4, 5. Rank 1 + 2 use the existing LSP /
SG test infrastructure.

### 7.6 Documentation

Each rank ships with:

- Changelog entry in `ide-extensions~/changelog.json` (LSP-side, only
  Rank 1) and `CHANGELOG.md` (everything HMR-side).
- Discord changelog entry per `Plans~/DISCORD_CHANGELOG.md`
  conventions.
- TECH_DEBT_V2.md: move the corresponding section to “Closed” with
  a one-line reference to the PR / version.

---

## 8. Phasing & release strategy

| Phase | Ships in | Contents                                       | User-visible impact                                             |
| ----- | -------- | ---------------------------------------------- | --------------------------------------------------------------- |
| P1    | 0.5.x    | Rank 1 + UITKX0107                             | LSP: folder copy/rename no longer breaks IntelliSense.          |
| P2    | 0.5.x    | Rank 2                                         | HMR: new sibling/asmdef-scoped `.cs` types just work.           |
| P3    | 0.5.x    | Rank 3 + UITKX0211                             | HMR: module value edits propagate to all consumers.             |
| P4    | 0.6.0    | Rank 4                                         | HMR: body-only edits cascade through component tree.            |
| P5    | 0.6.0    | Rank 5 (default ON, with sanity-check fallback)| HMR: prop signature refactors no longer require Stop+Restart.   |

Phase 5 ships **default-on** (per §5.2.1 decision 1). The previously
planned `UITKX_HMR_SignatureCascade` opt-in EditorPref is dropped.
The safety net is the union-compile sanity-check fallback (§5.2.1
decision 2): if the pre/post-compile guards detect anything off,
the pipeline bails to the per-file path and the user sees the same
error they’d see today. No silent regressions possible.

P4 + P5 may ship in the same minor release since Rank 5 depends on
Rank 4’s queue/cascade infrastructure and there’s no user benefit to
splitting them.

---

## 9. Out of scope (for this plan)

- **Static-method swap in modules** (the “What this does NOT cover”
  list in `UitkxHmrModuleStaticSwapper.cs` L122). Tracked in TECH_DEBT
  separately.
- **Player-runtime HMR** — HMR is Editor-only; this plan keeps it so.
- **LSP rename across asmdefs when both declare the same name** —
  Rank 1 picks deterministic first-by-path. Cross-asmdef rename is
  a separate UX problem.
- **Removing the assembly-reload lock** during HMR — would render
  this entire plan moot but conflicts with Play-mode state
  preservation. Not on the roadmap.

---

## 10. Implementation order checklist (for the eventual PRs)

```
[ ] Rank 1: WorkspaceIndex multi-valued + UITKX0107 + tests
[ ] UITKX0211: const-in-module analyzer (Warning severity) + code fix + tests
[ ] Rank 2: NewCsFileDiscovery + watcher patch + tests
[ ] Shared: UitkxFileDependencyIndex (lands with Rank 3 PR)
[ ] Rank 3: module reverse-dep cascade + real compile queue + tests
[ ] Rank 4: component reverse-dep cascade (reuses Rank 3 infra) + tests
[ ] Rank 5: per-SCC union compile (default on, no SCC cap) + tests
        [ ] Pre-compile guard: (Namespace, Name) uniqueness check across union
        [ ] Post-compile guard: every IProps Type.Assembly == hmrAssembly
        [ ] Fallback path: on guard failure → per-file compile + log
            `[HMR] union-compile sanity check failed — falling back`
        [ ] Telemetry: `[HMR] union: N files, M ms` per union compile
        [ ] Verify `-nowarn:0436` suppression on the in-process Roslyn path
[ ] Docs: CHANGELOG, DISCORD_CHANGELOG, TECH_DEBT_V2 closure entries
[ ] Verify with PrettyUi reproductions (#20, #21, #22A, #22B)
```

---

**End of plan.**
