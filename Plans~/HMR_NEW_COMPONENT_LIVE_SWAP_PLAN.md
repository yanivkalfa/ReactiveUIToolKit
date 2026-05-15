# HMR — Support live-swap of components created during a session

**Status:** ✅ COMPLETED — shipped in 0.5.17 (2026-05-15)
**Author:** Investigation 2026-05-15
**Target version:** 0.5.17

---

## Completion summary

All four sections implemented in a single pass:

- ✅ §4.1 `UitkxHmrComponentTrampolineSwapper.cs` — `FindProjectComponentType` replaced by `FindAllSwapTargetTypes` (returns project + prior `hmr_*` types). `SwapAll` iterates targets; `NotifyMatchingFibers` accepts a HashSet with single-type fast path; per-fiber signature comparison.
- ✅ §4.2 `UitkxHmrAssetPostprocessor.cs` — forwards `deletedAssets` and `movedFromAssetPaths` via new `EnqueueAssetDeletion`.
- ✅ §4.3 `UitkxHmrFileWatcher.cs` — added `OnUitkxDeleted` event + `EnqueueAssetDeletion(string)` that fires synchronously (no debounce, deletions are cleanup) and clears any pending change for the same path.
- ✅ §4.4 `UitkxHmrController.cs` — `OnUitkxFileDeleted` handler subscribed in Start/Stop; compile-failure branch checks `File.Exists` before enqueueing retry (belt-and-braces against deletions outside the Project window).
- ✅ §6 README updated.
- ✅ §10 version 0.5.17, CHANGELOG.md + DISCORD_CHANGELOG.md (1942 chars, ASCII-only).

Compile-clean across all touched files.

---

## 1. Workflow we want to support

User has HMR running. Without stopping it, they:

1. Create folder `Assets/UI/Pages/.../PlayerHud/components/StatsPanel/`.
2. Copy `PlayerHud.uitkx` + `PlayerHud.style.uitkx` into it.
3. Rename them to `StatsPanel.uitkx` / `StatsPanel.style.uitkx`.
4. Edit body: `component PlayerHud {` → `component StatsPanel {`, `module PlayerHud {` → `module StatsPanel {`.
5. Save. (Errors during steps 2–4 are acceptable — the duplicate `module PlayerHud` legitimately collides.)
6. Add `<StatsPanel />` to parent `PlayerHud.uitkx`. Save.
7. Edit `StatsPanel.uitkx` further. Save. **Must hot-swap visibly without a domain reload.**
8. Continue editing. Every save must continue to swap.

Today, step 7 silently no-ops: no log, no swap, the rendered tree stays on the first version. Step 8 also silent. Steps 2–4 produce a `FileNotFoundException` cascade for `…\StatsPanel\PlayerHud.uitkx` after the rename.

---

## 2. Why it currently breaks (root cause, with citations)

### 2.1 Trampoline-swap requires a project-loaded type

The trampoline architecture lives on a `private static` field `__hmr_Render` on the component class — emitted by the source generator at lines verified in [Editor/HMR/HmrCSharpEmitter.cs#L275](Editor/HMR/HmrCSharpEmitter.cs#L275):

```csharp
internal static Func<...> __hmr_Render = __Render_body;

public static VirtualNode Render(IProps __rawProps, IReadOnlyList<VirtualNode> __children)
{
    if (global::ReactiveUITK.Core.HmrState.IsActive)
        return __hmr_Render(__rawProps, __children);
    return __Render_body(__rawProps, __children);
}
```

`SwapAll` writes a fresh delegate into that field on the **already-loaded** type ([Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L155](Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L155)):

```csharp
Type oldType = FindProjectComponentType(componentName, uitkxFilePath);
if (oldType == null)
{
    // Component never compiled into the project — nothing to swap.
    return 0;
}
```

`FindProjectComponentType` ([UitkxHmrComponentTrampolineSwapper.cs#L295](Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L295)) deliberately skips assemblies whose name starts with `hmr_`:

```csharp
if (asm.GetName().Name?.StartsWith("hmr_", StringComparison.Ordinal) == true)
    continue;
```

**Consequence:** for a component that has never gone through a domain reload (so the SG has never produced its `[UitkxElement]`/`[UitkxSource]` types in the project assembly), `oldType == null`. The method returns 0 with no log.

### 2.2 But the trampoline DOES exist on a prior HMR DLL

When the parent compiles with `<StatsPanel />`, the controller's CS0103 path ([UitkxHmrController.cs#L682–728](Editor/HMR/UitkxHmrController.cs#L682-L728)) auto-discovers and compiles `StatsPanel`, registering it in `_compiler._hmrAssemblyPaths["StatsPanel"] = hmr_StatsPanel_1.dll` ([UitkxHmrCompiler.cs#L1168](Editor/HMR/UitkxHmrCompiler.cs#L1168)) and loads it via `Assembly.LoadFrom` ([UitkxHmrCompiler.cs#L1172](Editor/HMR/UitkxHmrCompiler.cs#L1172)).

Then the parent's recompile uses that DLL as a `MetadataReference` in `BuildCrossRefs()` ([UitkxHmrCompiler.cs#L1197–L1230](Editor/HMR/UitkxHmrCompiler.cs#L1197-L1230)). The parent's emitted IL binds to the `StatsPanel` type living in `hmr_StatsPanel_1.dll` — that type has the standard `__hmr_Render` static field, populated to `__Render_body`.

So the runtime call chain on every render after step 6 is:

```
parent's render → V.Func(StatsPanel.Render, ...) [bound to hmr_StatsPanel_1's StatsPanel]
  → StatsPanel.Render checks HmrState.IsActive
  → invokes __hmr_Render (static field on hmr_StatsPanel_1's type)
  → currently still the original __Render_body baked into hmr_StatsPanel_1
```

When the user saves StatsPanel again at step 7, we compile `hmr_StatsPanel_2.dll`, but `SwapAll` looks for the project-loaded `StatsPanel` type, finds none, returns 0. The trampoline on `hmr_StatsPanel_1`'s type is never updated. Render keeps executing the v1 body.

### 2.3 The stale-retry cascade is independent

`_pendingRetryPaths` accepts an entry on every compile error ([UitkxHmrController.cs#L557](Editor/HMR/UitkxHmrController.cs#L557), [#L722](Editor/HMR/UitkxHmrController.cs#L722)) but only removes it on **successful compile of that exact path** ([#L504](Editor/HMR/UitkxHmrController.cs#L504)) or on Stop/Dispose ([#L230](Editor/HMR/UitkxHmrController.cs#L230), [#L264](Editor/HMR/UitkxHmrController.cs#L264)). When the user renames `…\StatsPanel\PlayerHud.uitkx` → `…\StatsPanel\StatsPanel.uitkx`, the entry keyed by the OLD path can never succeed — `RetryPendingCompilations` ([#L766–L773](Editor/HMR/UitkxHmrController.cs#L766-L773)) calls `ProcessFileChange` which calls `_compiler.Compile` which calls `File.ReadAllText` ([UitkxHmrCompiler.cs#L172](Editor/HMR/UitkxHmrCompiler.cs#L172)) — `FileNotFoundException`. Forever.

`UitkxHmrAssetPostprocessor.OnPostprocessAllAssets` currently ignores `deletedAssets` and `movedFromAssetPaths` ([UitkxHmrAssetPostprocessor.cs#L86–L96](Editor/HMR/UitkxHmrAssetPostprocessor.cs#L86-L96)), so nothing tells the controller to evict the stale key.

---

## 3. Why "swap on prior HMR DLL" is safe (architectural verification)

| Concern | Verdict | Evidence |
|---|---|---|
| Trampoline field exists on every `[UitkxElement]` type, including in HMR DLLs | YES | SG emits unconditionally at [HmrCSharpEmitter.cs#L275](Editor/HMR/HmrCSharpEmitter.cs#L275) |
| Field is `static` so `SetValue(null, …)` is correct | YES | `BindingFlags.Static \| BindingFlags.NonPublic`, [UitkxHmrComponentTrampolineSwapper.cs#L86–L88](Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L86-L88) |
| Render gates on `HmrState.IsActive` so post-Stop the swap is harmless | YES | `if (HmrState.IsActive) return __hmr_Render(...)` |
| `HasHookSignatureChanged` works cross-assembly | YES | Reads `HookSignatureAttribute.Signature` (string) on each side via reflection — assembly identity irrelevant ([UitkxHmrComponentTrampolineSwapper.cs#L410–L420](Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L410-L420)) |
| `NotifyMatchingFibers` / `ComponentState` rebind by Type identity | Need to handle multi-version case | See §5.2 below |
| Prior `hmr_*` DLLs stay loaded in AppDomain across recompiles | YES | `Assembly.LoadFrom` + memory-mapping; old file deletion best-effort only ([UitkxHmrCompiler.cs#L1163–L1166](Editor/HMR/UitkxHmrCompiler.cs#L1163-L1166)). Confirmed by exploration: no code ever calls `Unload`/`AssemblyLoadContext` |
| Other swappers (modules, methods, delegates) have the same `hmr_*` skip but for different reasons | DIFFERENT CASE | Module statics/methods live on the *partial class* whose project-side instance does need to exist — these legitimately can't HMR-swap a module that has no project-side counterpart. Out of scope for this plan; see §7 |
| Performance hit | Negligible | One additional pass over `AppDomain.CurrentDomain.GetAssemblies()` (already used) only when the project-side scan returns null. Linear in #loaded asms (~50–200), runs at most once per save |

### 3.1 Multi-version question

If the user saves StatsPanel three times after step 6 we end up with `hmr_StatsPanel_1.dll` (referenced by parent compiled at step 6), `hmr_StatsPanel_2.dll`, `hmr_StatsPanel_3.dll` all loaded. The parent's **method-group binding** at step 6 was to `hmr_StatsPanel_1`'s `StatsPanel.Render`. Subsequent parent edits (if any) re-emit and rebind to whichever DLL was newest in `_hmrAssemblyPaths` at that compile time. Both bindings can coexist in a fiber tree.

→ **Decision:** swap the trampoline on **every prior `hmr_*`** assembly that contains `[UitkxElement(componentName)]`, not only the most recent. O(k) where k = small. Notify fibers matching **any** of those types. This keeps every binding live regardless of which parent compiled when.

---

## 4. Implementation plan

### 4.1 File: `Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs`

**Rename / refactor `FindProjectComponentType` → `FindAllSwapTargetTypes` returning `IReadOnlyList<Type>`.**

Behaviour:

1. Walk `AppDomain.CurrentDomain.GetAssemblies()` once.
2. For each non-dynamic assembly:
   - If the assembly name starts with `hmr_` AND it is the *just-loaded* `hmrAssembly` we are swapping FROM, skip (we'd be swapping the brand-new body onto itself — harmless but pointless).
   - Otherwise, scan its types for `[UitkxElement(componentName)]` (primary) and `[UitkxSource]` matching `uitkxFilePath` (rename fallback), exactly as today.
3. Collect ALL matches (project + prior HMR DLLs) into the returned list.
4. If empty, return empty list (caller logs and returns 0).

`SwapAll` adjustments:

- After resolving `newRender`, get `var oldTypes = FindAllSwapTargetTypes(...)`.
- If `oldTypes.Count == 0`: `Debug.Log($"[HMR] Compiled {componentName} — no live consumer types yet (component is brand-new; subsequent edits will hot-swap once a parent compiles against it).")`, return 0. **This is your point #2.**
- For each `oldType` in `oldTypes`:
  - Locate trampoline field; capture rollback in `s_rollbackByType[oldType]`; write `newRender`. (`s_rollbackByType` is already a `ConcurrentDictionary<Type, Delegate>` so multi-key is fine.)
- Compute `signatureChanged` once against the FIRST `oldType` (all prior versions share the same component, signature attribute either changed or didn't — comparison against the first stable representative is correct).
- Iterate fibers once; in `NotifyMatchingFibers` accept `IReadOnlyList<Type>` instead of `Type` and match if any in the set.

**`TryRollback(Type)` is unchanged** — it operates on whichever specific type the reconciler hands it, which will be one of the types we registered.

### 4.2 File: `Editor/HMR/UitkxHmrAssetPostprocessor.cs`

Wire `deletedAssets` and `movedFromAssetPaths` to a new sink on the watcher.

```csharp
for (int i = 0; i < deletedAssets.Length; i++)
    ForwardDeletion(deletedAssets[i]);
for (int i = 0; i < movedFromAssetPaths.Length; i++)
    ForwardDeletion(movedFromAssetPaths[i]);
```

`ForwardDeletion` mirrors `Forward` but calls `watcher.EnqueueAssetDeletion(abs)`.

### 4.3 File: `Editor/HMR/UitkxHmrFileWatcher.cs`

Add `internal void EnqueueAssetDeletion(string absolutePath)` that forwards to the controller via a new internal call (the watcher already holds a controller ref for `_pendingChanges`). For symmetry I'll add a callback the watcher invokes synchronously (deletions don't need the 50 ms debounce — they're cleanup, not work).

### 4.4 File: `Editor/HMR/UitkxHmrController.cs`

Add `internal void NotifyAssetDeleted(string absolutePath)`:

1. `_pendingRetryPaths.Remove(absolutePath)` — kills the stale-retry cascade. **This is your point #3 (and resolves Phase B's `FileNotFoundException`).**
2. Optional belt-and-braces: scan `_compiler.HmrAssemblyPaths` and remove entries whose value is no longer on disk. Skip for now — adds risk of perturbing parent references; not required for the stated workflow.

Also add to `ProcessFileChange`'s catch path: when `Compile` throws `FileNotFoundException` or `DirectoryNotFoundException` for `uitkxPath`, call `_pendingRetryPaths.Remove(uitkxPath)` and log `"[HMR] Path no longer exists, removing from retry queue: {uitkxPath}"`. **This is your point #1.** Belt-and-braces against the postprocessor missing the deletion event (e.g. user deletes via Explorer instead of Project window, then comes back to Unity later).

### 4.5 No changes needed in

- `UitkxHmrCompiler.cs` — the cross-ref / LoadFrom / accumulation behaviour is exactly what we want.
- `HmrCSharpEmitter.cs` — emitted shape is already correct.
- `HookContainerRegistry.cs` — hook bindings flow through the existing trampoline path; the swap above carries them.
- `UitkxHmrModuleStaticSwapper.cs` / `UitkxHmrModuleMethodSwapper.cs` / `UitkxHmrDelegateSwapper.cs` — see §7.

---

## 5. How points 1–4 from earlier conversation map

| # | Point | Where it lives in this plan |
|---|---|---|
| 1 | `_pendingRetryPaths` GC on FileNotFoundException | §4.4 second paragraph |
| 2 | Visible "no project type" log in `SwapAll` | §4.1 — replaced with a richer message that reflects the new semantics |
| 3 | Postprocessor handles `deletedAssets` / `movedFromAssetPaths` | §4.2 + §4.3 + §4.4 first paragraph |
| 4 | Documentation | §6 — `Editor/HMR/README.md` gets a new short section on "creating components live" |

All four are needed regardless — they're not theoretical. #1 and #3 fix observable bug (FileNotFoundException spam after rename). #2 fixes "feels broken" silence. #4 sets expectations for future maintainers.

---

## 6. Test plan

### 6.1 Manual repro — the original workflow

Exactly the user's 8 steps in §1. Acceptance:

- After step 5: compile errors visible in console (acceptable). NO `FileNotFoundException` cascade.
- After step 6: `[HMR] Compiled StatsPanel — no live consumer types yet…` (when StatsPanel auto-discovered) then `[HMR] Swapped PlayerHud (N instance(s))` (when parent recompiles). UI shows StatsPanel.
- After step 7: `[HMR] Swapped StatsPanel (N instance(s))` and the visible UI updates. **This is the success criterion.**
- After step 8: continued `Swapped StatsPanel` on each save with visible UI update.

### 6.2 Regression check — existing single-component HMR

Edit any existing component (`PlayerHud.uitkx`). Acceptance: behaviour unchanged from 0.5.16 — `Swapped` log, instances re-render. (FindAllSwapTargetTypes returns the project-loaded type as the first entry; any prior HMR types of the same component also get their trampolines updated, which is correct and was already a latent gap.)

### 6.3 Hook signature change in new component

After step 7, add a `useState` to `StatsPanel`. Acceptance: `[HMR] Hook signature changed in StatsPanel — resetting state on all instances.` then UI repaints from fresh state.

### 6.4 Delete the new component

Delete `StatsPanel.uitkx` entirely from Project window. Acceptance: no retry-loop, no log spam. Parent may show a render error (expected — its IL still references `StatsPanel`); next parent edit will surface a clean CS0103 in console.

### 6.5 Performance

In the existing benchmark (`Diagnostics/Benchmark/`), measure swap latency over 100 saves on a small project. Acceptance: ≤ 5 % regression. Expectation: zero measurable change — the new code paths are an extra pass over `GetAssemblies()` only when prior scan returns null, and an extra `oldTypes.Count == 1` loop body in the common case.

---

## 7. Out of scope / explicit non-goals

- **Module-static / module-method swap for brand-new modules.** When a brand-new `module Foo {}` is created live, its static fields don't exist on the project-side partial class (because the SG hasn't run). The module swappers ([UitkxHmrModuleStaticSwapper.cs#L342](Editor/HMR/UitkxHmrModuleStaticSwapper.cs#L342)) skip `hmr_*` assemblies for the same reason `FindProjectComponentType` does. The semantics are different (module statics are class-wide state, not per-instance trampolines) and applying the same fallback there has different correctness implications. **If the user's StatsPanel uses a brand-new `module StatsPanel { static Style Header = … }` defined in the SAME file, those statics will live in `hmr_StatsPanel_N.dll` already and get used by that DLL's own Render — fine.** The cross-assembly module-swap problem only manifests if a brand-new module from one file is consumed by code in a *different* file that's also being live-edited. Punt to a separate plan if the user reports it.
- **Roslyn `'Hud' does not exist` errors during steps 2–4.** Legitimate duplicate-definition during the copy-rename phase. User explicitly said errors during the journey are acceptable.
- **Eviction of stale entries from `_compiler._hmrAssemblyPaths` on file delete.** Not needed for the workflow — those entries remain pointing at still-loaded DLLs that are still valid as MetadataReferences for any future cross-ref. Adding eviction risks breaking parent compiles. Skip.

---

## 8. Risk register

| Risk | Severity | Mitigation |
|---|---|---|
| Multiple prior `hmr_StatsPanel_*` types — fiber may match more than one in `NotifyMatchingFibers` | Low | Idempotent: notifying a fiber twice in the same swap pass is safe (second notify is a no-op state-update). Worst case: one extra render per fiber per swap. |
| `s_rollbackByType` accumulates entries for prior HMR types that may never be invoked | Negligible | Map is per-type, types are bounded by # components in project. Entries overwritten on each swap. Cleared on Stop via existing teardown. |
| `[UitkxElement(componentName)]` collisions across prior HMR DLLs of UNRELATED components (false positive name match) | None | The attribute carries the component name verbatim; if two DLLs both claim `StatsPanel` they are by definition versions of the same component. |
| Stale DLL on disk gets locked and accumulates in `Temp\UitkxHmr\` | Existing behaviour | Already cleaned by `CleanStaleTempFiles()` ([UitkxHmrCompiler.cs#L558–L565](Editor/HMR/UitkxHmrCompiler.cs#L558-L565)) on next session. No change. |
| Postprocessor delete events fire during HMR Stop transition and call into a half-disposed controller | Low | The watcher's existing `s_watchers` snapshot pattern + `lock` covers this; deletion path will use the same lock. |

---

## 9. Rollback strategy

Single feature-flag-free change. If it misbehaves:

1. `git revert` the 0.5.17 commit — restores 0.5.16 behaviour exactly.
2. The new asset-postprocessor methods are additive; reverting them costs nothing.
3. No on-disk state changes; no schema changes; no public API changes (`SwapAll` signature unchanged, `TryRollback` unchanged).

---

## 10. Versioning & changelog

- `package.json` → `0.5.17`
- `CHANGELOG.md` → new "0.5.17" entry under Added/Fixed
- `Plans~/DISCORD_CHANGELOG.md` → ASCII-only narrative entry (≤ 2000 chars), prepended
- `ide-extensions~/changelog.json` → no entry (no ide-extension files touched)

---

## 11. Estimated diff size

- `UitkxHmrComponentTrampolineSwapper.cs`: ~+40 / −10 lines (rename method, list iteration, log)
- `UitkxHmrAssetPostprocessor.cs`: ~+15 lines (delete forwarding)
- `UitkxHmrFileWatcher.cs`: ~+8 lines (`EnqueueAssetDeletion` plumbing)
- `UitkxHmrController.cs`: ~+15 lines (`NotifyAssetDeleted` + catch-block GC)
- `Editor/HMR/README.md`: ~+20 lines (new section)
- Total: ~110 lines added, ~10 removed across 5 files. No new files.
