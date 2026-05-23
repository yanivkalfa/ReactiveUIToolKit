# HMR Determinism Report (May 21, 2026)

## Purpose

Answer the user's question: *"Why does HMR behave differently depending on
project size? Make it deterministic — same input, same output, every time,
regardless of how many samples or files exist."*

This report is the result of a full read-through of every file in
[Editor/HMR/](../Editor/HMR/) plus the reconciler-side integration points in
[Shared/Core/Fiber/](../Shared/Core/Fiber/) and the cross-check with the
captured live log at `C:\Users\neta\Pretty Ui\Assets\consoleLogFile.txt`.

It is not a patch. It is a diagnosis + three concrete redesign options
ranked by cost, risk, and how thoroughly they eliminate non-determinism.

---

## TL;DR

**HMR is non-deterministic today for four independent reasons.** The
project-size sensitivity the user observed (delete the `Samples/` folder →
duplication bug starts reproducing on every save) is the most visible
symptom of cause #1, but #2-#4 each cause their own variant of "looks
different every time".

| # | Cause | One-line summary |
|---|---|---|
| 1 | **Rank 4 cascade reintroduces cross-DLL type identity drift** | `CollectTransitiveDependents(includeComponents: true)` pulls every parent into the recompile, which moves the parent's own type identity into a new HMR DLL, which makes its grandparent see "different component" and tear the subtree down. This is the bug from the captured log. |
| 2 | **BFS visit order over reverse maps is `Dictionary`/`HashSet` enumeration order** | Mono enumeration order depends on hash-bucket layout, which depends on what else is in the dictionary, which depends on how many sample files were indexed. Same edit → different batch order → different "originator-last" choice → different swap semantics. |
| 3 | **Async seed window** | `HookContainerRegistry.Seed` and `UitkxFileDependencyIndex.Seed` both run on `Task.Run` after `Start()`. The first 1-2 saves after `Start` may run against an empty / partial index, taking the per-file fast path instead of the cascade path. Bigger workspace = longer seed = bigger non-determinism window. |
| 4 | **Module-static cross-copy timing depends on cctor ordering** | When the HMR DLL loads, the CLR runs *its* cctors before `ModuleStaticSwapper` copies values into the project type. Any module cctor that observes another module's static at load time reads a different value depending on the alphabetical order Mono chose for cctor invocation, which depends on the metadata-token layout produced by the union compile, which depends on the file set in the batch. |

The bug visible in the captured log
(`[HMR-DIAG] IsCompatibleType=false ... fiber.declaring='GamePage,Assembly-CSharp' vnode.declaring='GamePage,hmr_batch_InteractionDialog_4_12'`)
is the direct consequence of #1 colliding with the
[HMR_COMPONENT_TRAMPOLINE_REFACTOR](archive/HMR_COMPONENT_TRAMPOLINE_REFACTOR.md)'s
explicit design assumption that **the parent never recompiles**.

---

## 1. Why the user's repro is project-size-sensitive

The captured log shows the cascade producing:

```
[HMR] union: 4 files, 976 ms
[HMR] Compiled InteractiveDrinkWater — no active instances to refresh.
[HMR] Swapped GamePage (1 instance(s)).
[HMR] GamePage updated (240ms, 1 instance(s)) — Module statics re-init: 5 | Module methods re-init: 1
[HMR] Swapped AppRoot (1 instance(s)).
```

— meaning the cascade walker pulled `InteractionDialog`, `InteractiveDrinkWater`,
`GamePage`, `AppRoot` all into a single union DLL `hmr_batch_InteractionDialog_4_12`.
Trampoline swaps fire on AppRoot's, GamePage's, etc. trampolines — these
succeed. But because GamePage's *own type* moved to `hmr_batch_*_12`, and
`AppRoot`'s render body (also now in `hmr_batch_*_12`) emits a vnode whose
`TypedFunctionRender.Method.DeclaringType` is `GamePage@hmr_batch_*_12`,
while the existing GamePage fiber's `TypedRender.Method.DeclaringType` is
still `GamePage@Assembly-CSharp` —

```
[HMR-DIAG] IsCompatibleType=false (FunctionComponent):
  fiber.declaring='PrettyUi.App.Pages.GamePage, Assembly-CSharp'
  vnode.declaring='PrettyUi.App.Pages.GamePage, hmr_batch_InteractionDialog_4_12'
```

`CanReuseFiber` returns false → entire GamePage subtree torn down → all
`useEffect(LoadSceneAsync(...))` mount effects re-fire → scene additively
re-loads → duplicate. The 0.5.21 `allowFullStateReset` gate is dead in this
codepath because it only protects `UitkxHmrComponentTrampolineSwapper.FullResetComponentState`
— the reconciler's `CommitDeletion → effect.cleanup → fresh mount` path is
upstream and unconditional.

### Why samples masked it

With samples present (~80 extra `.uitkx` files compiling into
`ReactiveUITK.Samples.dll`):

- The dependency-index seed (background `Task.Run`) takes longer to
  complete.
- The reverse maps (`s_componentReverse`, `s_moduleReverse`) carry hundreds
  of extra entries; `HashSet.Add` returns affect insertion order; the BFS
  pulls more files into the batch; the union compile sometimes **fails**
  on a stale sample reference (e.g. the captured first-attempt
  `[HMR] Compilation failed for TextOne: 'Container' does not exist`),
  which aborts the swap pipeline before the reconciler sees the bad vnode
  → bug never manifests on those saves.
- When the union compile happens to succeed but the cascade ordering
  produces a batch that *doesn't* include GamePage (e.g. because
  `HomePage` happened to be enumerated first and the BFS path different),
  GamePage stays in `Assembly-CSharp` and the trampoline pattern works
  as designed.

This is the classic "Heisenbug from a non-deterministic substrate".
**Removing samples doesn't fix the bug — it removes the noise that was
masking it.**

---

## 2. Four independent sources of non-determinism

### Cause 1 — Rank 4 cascade vs. trampoline's "parent IL is stable" assumption

**Where:**
- [Editor/HMR/UitkxHmrController.cs#L466-L482](../Editor/HMR/UitkxHmrController.cs#L466) — `OnUitkxFileChanged` calls `CollectTransitiveDependents(includeComponents: true)`.
- [Editor/HMR/UitkxFileDependencyIndex.cs#L193-L268](../Editor/HMR/UitkxFileDependencyIndex.cs#L193) — walker pulls every JSX-consumer of the changed component into the recompile batch.
- [Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L1-L62](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L1) — file header explicitly says `Render` method identity stays stable across cycles.
- [Shared/Core/Fiber/FiberChildReconciliation.cs#L274-L284](../Shared/Core/Fiber/FiberChildReconciliation.cs#L274) — comment says "Cross-assembly HMR fallback was deleted by the per-component `__hmr_Render` trampoline refactor: parent IL continues to issue method-group references to the live project type's stable Render trampoline".

**The contradiction:** The trampoline plan deleted the cross-assembly
fallback because it assumed the parent's IL never changes. Rank 4 cascade
explicitly recompiles the parent. Inside the cascaded parent's new body,
JSX references to siblings (`<GamePage />`) bind to the parent's
own union DLL — so the parent's freshly-emitted IL hands the reconciler
vnodes whose `TypedFunctionRender` lives in `hmr_batch_*_N`, not in
`Assembly-CSharp`. The reconciler sees that as a different type → `CanReuseFiber` false → subtree teardown.

**Why this hits "size sensitivity":** The cascade walker is the only path
that re-emits the parent. Without a cascade trigger, only the originator
recompiles. With samples present, certain unions fail to compile and the
swap aborts before the bad vnode ever reaches the reconciler.

### Cause 2 — BFS visit order is dictionary-enumeration-defined

**Where:**
- [UitkxFileDependencyIndex.cs#L226-L240](../Editor/HMR/UitkxFileDependencyIndex.cs#L226) — `foreach (string referrer in refs)` over a `HashSet<string>`.
- [UitkxFileDependencyIndex.cs#L218](../Editor/HMR/UitkxFileDependencyIndex.cs#L218) — `foreach (string mod in node.DeclaredModules)` over a `HashSet<string>`.
- [UitkxHmrController.cs#L489](../Editor/HMR/UitkxHmrController.cs#L489) — `_enqueued.Add(uitkxPath)` enqueues in walk order.

**The problem:** `HashSet<string>` enumeration order in Mono is "whatever
the internal bucket layout happens to be at this moment". Inserting more
items changes the bucket layout. Same source → different walk order →
different `batch` list → different `i == paths.Count - 1` originator →
different `allowFullStateReset` decisions across runs.

The walker promises "dependents first, originator last" — but among the
dependents themselves, order is undefined and depends on what else has been
indexed. Add samples, indexes change, order changes.

### Cause 3 — Async seed creates a "first-N-saves are different" window

**Where:**
- [UitkxFileDependencyIndex.cs#L122-L146](../Editor/HMR/UitkxFileDependencyIndex.cs#L122) — `Seed` runs on `Task.Run`.
- [UitkxHmrController.cs#L223](../Editor/HMR/UitkxHmrController.cs#L223) — `Seed` is fire-and-forget; no `TryWaitForSeed` gate before serving requests.
- `HookContainerRegistry.Seed` — same pattern.

**The problem:** On `Start`, the seed task scans the whole workspace
(`Directory.EnumerateFiles` recursive). Bigger workspace → longer seed
window. During that window, `CollectTransitiveDependents` walks a
partially-populated index, the cascade is smaller than it should be,
behavior differs from a steady-state save.

If the user happens to save during the seed window, the batch composition
is different from a save 5 seconds later. The user perceives this as
"sometimes my edits work, sometimes they don't".

### Cause 4 — Module cctor ordering depends on metadata token layout

**Where:**
- [UitkxHmrCompiler.cs](../Editor/HMR/UitkxHmrCompiler.cs) — union compile emits all files into one DLL; Roslyn assigns metadata tokens in source-iteration order; the file set varies per save.
- [UitkxHmrModuleStaticSwapper.cs](../Editor/HMR/UitkxHmrModuleStaticSwapper.cs) — copies values AFTER `Assembly.LoadFrom` runs cctors, but if cctor A reads cctor B's static, A sees whatever B emitted in this generation, which can differ from what was just copied into the project type.

**The problem:** Mono runs type cctors lazily, in dependency order, on
first access. For a union DLL containing types A and B where A's cctor
reads `B.Value`, A's cctor triggers B's cctor before A's body sees the
field. Now A's `static readonly` holds a value computed from B's
HMR-generation-N initializer. If B's initializer in the project assembly
was different (because the project assembly was compiled before the user
edited B), the values diverge between project-DLL-A and HMR-DLL-A — and
the static swapper only copies leaf values, not transitively-derived ones.

This is rare in practice but it's a class of bug that produces "this value
is sometimes wrong" symptoms that look random.

---

## 3. What "deterministic HMR" must guarantee

A deterministic HMR session means:

1. **Same source change → same compile batch.** No project-state, no file
   enumeration order, no async seed window affects which files end up in
   the batch. Defined deterministically by the source file + the
   dependency graph.

2. **Same compile batch → same swap targets.** The set of fibers notified,
   the set of types swapped, the `FullResetComponentState` decisions —
   all deterministic functions of `(old assembly N, new assembly N+1)`.

3. **Same swap targets → same reconciler outcome.** `CanReuseFiber` /
   `IsCompatibleType` return the same answer regardless of which DLL
   generation either side lives in, as long as the component's logical
   identity (UITKX name + source-file path) is unchanged.

4. **Effect cleanup runs exactly once per unmount; setup runs exactly once
   per mount.** No "mount effect re-fires on a hot reload" — that's a
   bug, not a feature.

5. **Render is idempotent per save.** Two consecutive saves of the same
   file with identical content produce identical visual + identical
   memory deltas.

React's Fast Refresh achieves (1)-(5) by:
- Tagging every component with a stable signature derived from its source
  (not from runtime identity).
- Storing a global `signatureToCurrentComponent` map so consumers always
  resolve to the *current* version via the stable signature, not via
  bound delegate identity.
- Splitting the swap into two phases: "register new versions" (pure data
  update), then "schedule re-renders" (single tree pass, no per-component
  decisions).
- Refusing to remount on body edits; remounting only on signature change
  (hook count, prop shape).

---

## 4. Three redesign options, ranked

### Option A — **Minimum-change patch + lock cascade to "trampoline-only"**

**Scope:** ~1 day of work. Restores the deleted cross-assembly fallback
in `CanReuseFiber` / `IsCompatibleType`, and gates `allowFullStateReset`
correctly on the reconciler side instead of just the swapper side.

**Edits:**

1. In `FiberChildReconciliation.CanReuseFiber` and
   `FiberFunctionComponent.IsCompatibleType` (FunctionComponent branch),
   after the existing `Method/Target` equality check fails, add an
   `#if UNITY_EDITOR` block that:
   - Resolves `[UitkxElement].ComponentName` on both `Method.DeclaringType`s.
   - Returns `true` when both attributes are present and the component
     names match (non-null, equal).
   - On reuse hit, adopts `fiber.TypedRender = vnode.TypedFunctionRender`
     so subsequent diffs hit the `ReferenceEquals` fast path.

2. In `UitkxFileDependencyIndex.CollectTransitiveDependents`, replace
   `HashSet<string>` with a `SortedSet<string>` (or `List` + sort
   post-walk) so iteration order is reproducible.

3. In `UitkxHmrController.Start`, call
   `UitkxFileDependencyIndex.TryWaitForSeed(2000)` before returning so
   the first save can't beat the seed.

4. Keep the trampoline + Rank 4 cascade as-is.

**What it fixes:**
- Causes 1 (the actual bug from the log) and 2 (BFS ordering) are fully
  resolved.
- Cause 3 (seed window) is narrowed to a hard cap that's almost always
  hit before the first save.
- Cause 4 remains (rare, requires real-world repro to motivate the fix).

**Why I list it first:** It is the smallest possible diff that makes the
user's repro deterministic and resolves the symptom. It accepts the
existing architecture and patches its load-bearing assumption.

**Risk:** The name-based fallback can theoretically reuse the wrong fiber
when the user has two unrelated components with the same `[UitkxElement]`
name in the same render tree. The SG already disallows duplicate component
names per asmdef; cross-asmdef collisions are extremely rare in practice
and produce a visible-immediately wrong-render rather than silent state
corruption.

**This is what I recommend you start with.** The architecture work in
Options B/C can follow once the user's iteration loop is unblocked.

---

### Option B — **Eliminate Rank 4 cascade; use stable-identity registry**

**Scope:** ~3-5 days of work. Removes the need for parent recompile by
introducing a runtime component registry keyed by stable identity.

**Design:**

1. Source generator emits, on every component type:
   ```csharp
   internal static readonly Guid __uitkx_StableId = new Guid("...");
   internal static readonly string __uitkx_SourcePath = "Assets/.../MyComponent.uitkx";
   ```
   The GUID is deterministically derived from the asmdef + source-file
   path (not file contents — so it's stable across edits).

2. Add `UitkxComponentRegistry` that maps `StableId → Type currentVersion`.
   When an HMR DLL loads, its module initializer registers every type's
   `Render` method against its `StableId`, overwriting any prior entry.

3. Parent components reference siblings via a registry-mediated thunk:
   ```csharp
   // BEFORE
   V.Func<TProps>(MyChild.Render, props)
   // AFTER (SG-emitted)
   V.Func<TProps>(UitkxComponentRegistry.GetRender(MyChild_StableId), props)
   ```
   The registry lookup is O(1) and the returned delegate always points at
   the latest version. Parent IL no longer needs to recompile when a
   sibling changes.

4. Rank 4 cascade becomes UNUSED — the only time the parent needs to
   recompile is when its OWN source changed.

5. `CanReuseFiber` keys identity by `StableId`, not by `Method.DeclaringType`.
   Cross-DLL drift is impossible by construction.

**What it fixes:**
- Cause 1 (cascade) — fully eliminated; cascade is no longer needed.
- Cause 2 (BFS order) — irrelevant; no batch ordering matters.
- Cause 3 (seed window) — irrelevant; the registry is populated on
  assembly load, not via async file scan.
- Cause 4 (cctor order) — partially helped: cross-module cctor reads still
  exist, but the registry-mediated render path means subsequent renders
  read fresh values.

**Cost:** Every JSX call-site becomes a registry lookup. With `Dictionary<Guid, Delegate>` and Mono's hash distribution, that's ~20-30 ns per child render — likely below noise floor for a render that already allocates a vnode. Can be benchmarked.

**Risk:** Registry-mediated thunks must be hot-swap-safe themselves. A
stable invariant: registry value is monotonically the latest-loaded
delegate; never "demoted" backwards. Rollback (on render crash) needs
a per-StableId rollback slot, not the current per-Type slot.

---

### Option C — **Full React Fast Refresh port**

**Scope:** ~2-3 weeks. The "completely rewrite if necessary" path the
user mentioned.

**Design:** Mirror React's runtime contract for HMR:

1. **Signature-based identity.** Every component carries a
   `[FastRefreshSignature("hash")]` where the hash is over: prop shape,
   hook call sequence (count + types), and child JSX shape — NOT over
   body code. Body edits don't change the signature.

2. **Compatible-edit guarantee.** Signature unchanged → in-place body
   swap, no fiber teardown, no effect re-fire. Signature changed →
   targeted remount of *just* the changed component, not the subtree.

3. **Module-as-a-single-unit.** The unit of HMR is one .uitkx file, not a
   cascade. Sibling consumers are reached via the registry (Option B),
   not via recompile.

4. **Boundary inference.** React knows that "edits to App.jsx remount the
   whole tree" is bad UX, so it computes a "refresh boundary" — the
   deepest component whose signature didn't change, and remounts only
   from there. UITKX would do the same.

5. **Strict-mode-style invariants.** Single render scheduler, no
   reentrancy, no per-cycle ordering races — by construction, not by
   convention.

6. **Persistent state across remount.** When a remount is unavoidable,
   serialize `useState` / `useRef` values into a side table keyed by
   `(SourcePath, FiberIndex)` and restore on remount — matches React DevTools'
   "preserve state across edits" feature.

**What it fixes:** All four causes plus the entire category of "HMR edit
behaves slightly differently from a fresh mount" issues.

**Cost / risk:** Significant rewrite of the controller + swapper +
reconciler integration. Touches the SG. Migration story for existing
generated code is non-trivial. The user said unlimited tokens, but
practical risk is high: every existing PrettyUi-style consumer pattern
has to be re-validated.

---

## 5. Recommendation

**Ship Option A as the immediate unblock.** It is the smallest possible
change that eliminates the user's project-size-sensitivity and the
duplication symptom — confirmed by the captured log diagnostic chain
(IsCompatibleType=false ⇒ CommitDeletion ⇒ effect re-fire as
`lastDepsNull=True`).

Concurrently, begin Option B as the v0.6 architectural shift. The
registry-mediated component reference is the single highest-leverage
change in the HMR system because it removes the *need* for cascade
entirely, which means the trampoline pattern's assumption about parent IL
stability is true again, and the cross-assembly identity problem becomes
impossible by construction (not patched away).

Option C is the right destination but not the right next step. Defer
until v1.0 or when the user hits Option B's limits.

---

## 6. Acceptance test for "deterministic HMR"

After whichever option ships, the following must hold and should be
automated where possible:

1. **Two consecutive identical saves produce identical console output**
   (modulo timestamps). Run via a soak test that saves the same file 20
   times and diffs every other run.

2. **Adding/removing the `Samples/` folder changes nothing about the
   per-save behavior of a non-sample file.** Use the PrettyUi
   `InteractionDialog.uitkx` repro from May 21 as the canonical test
   case.

3. **Mount-effect re-fire counter is zero across HMR saves.** Add a test
   hook that asserts `useEffect(setup, [])` runs exactly once across a
   sequence of HMR saves that don't change the effect's body.

4. **`CanReuseFiber` returns true 100% of the time for fibers whose
   component name and source path are unchanged**, regardless of which
   DLL their `Method.DeclaringType` lives in.

5. **`HookContainerRegistry.TryWaitForSeed` always returns true within
   the first 100 ms of `Start()` completing.**

6. **No `Heisenbug` reports.** If a user reports "sometimes my edit
   doesn't show up", we have enough determinism that we can either
   reproduce it on the spot or definitively rule out HMR as the cause.

---

## 7. Investigation notes (what I read and where)

- [Editor/HMR/UitkxHmrFileWatcher.cs](../Editor/HMR/UitkxHmrFileWatcher.cs) — debounce + AssetPostprocessor parallel path; deterministic modulo `Environment.TickCount` wrap (B13).
- [Editor/HMR/UitkxHmrAssetPostprocessor.cs](../Editor/HMR/UitkxHmrAssetPostprocessor.cs) — same dedupe key as FSW; safe parallel source.
- [Editor/HMR/UitkxHmrController.cs](../Editor/HMR/UitkxHmrController.cs) — full read; the orchestrator is where the cascade is triggered and where the queue is drained. `_compileInFlight` correctly serializes drains; `DrainCompileQueueIfIdle` re-arms itself via `delayCall` (single source of in-flight). The `OnUitkxFileChanged → CollectTransitiveDependents → EnqueueCompile → DrainCompileQueueIfIdle → ProcessBatch → CompileBatch → ApplySuccessfulCompileResult → SwapAll` chain is single-threaded and well-defined; the non-determinism is in the cascade input set and BFS order.
- [Editor/HMR/UitkxFileDependencyIndex.cs](../Editor/HMR/UitkxFileDependencyIndex.cs) — `HashSet<string>` enumeration order at L226/L240 is the BFS non-determinism; `Seed` async pattern at L122 is the seed-window non-determinism.
- [Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs) — full read; `FindAllSwapTargetTypes` correctly enrolls all prior HMR DLL types; `NotifyMatchingFibers` walks every renderer's root; `FullResetComponentState` is the swapper's reset path (separate from `FiberReconciler.ResetComponentStateForHmrRollback`). The file header explicitly documents the "parent IL is stable" invariant that Rank 4 breaks.
- [Shared/Core/Fiber/FiberChildReconciliation.cs#L264-L290](../Shared/Core/Fiber/FiberChildReconciliation.cs#L264) and [Shared/Core/Fiber/FiberFunctionComponent.cs#L264-L290](../Shared/Core/Fiber/FiberFunctionComponent.cs#L264) — `CanReuseFiber` / `IsCompatibleType` after trampoline-refactor deletion of the cross-asm fallback. This is the line that has to change (or be made irrelevant) to fix the symptom.
- [Plans~/HMR_AUDIT.md](HMR_AUDIT.md) — already documents 28 separate HMR bugs (B1-B28); the cross-asm issue here is closely related to B7 (name-match was "fragile") and B28 (Sidebar style revert, marked HIGH priority and "needs instrumentation"). The instrumentation has now been done (logs in Pretty Ui's clone, May 21).
- [Plans~/archive/HMR_COMPONENT_TRAMPOLINE_REFACTOR.md](archive/HMR_COMPONENT_TRAMPOLINE_REFACTOR.md) — original design rationale; explicit invariant at L156: "parent IL keeps calling `MyComponent.Render` which transparently dispatches to the new body". Rank 4 cascade was added later and broke this invariant.

---

*End of report. The user should pick an option (A / B / C) before any
code changes happen.*
