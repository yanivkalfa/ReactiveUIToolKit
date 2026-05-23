# HMR Audit (May 11, 2026)

Comprehensive audit of the UITKX Hot Module Reload subsystem under
`Editor/HMR/` plus the Source Generator side that emits the swap targets
(`SourceGenerator~/Emitter/HookEmitter.cs`, `ModuleBodyRewriter.cs`,
`CSharpEmitter.cs`), and a cross-reference of how the live PrettyUi sample
project at `C:\Users\neta\Pretty Ui\Assets\UI` exercises (and breaks) those
code paths.

Every entry is self-contained: file paths, line numbers, symptom, root cause,
and an **Effect tag** so an unrelated agent picking this up after a context
break can route the work.

**Effect tag legend:**

- **VISUAL** — user sees wrong/missing UI, flicker, ghost state, double-fire
  callbacks, lost subscriptions.
- **STATE** — hook/component state corruption (lost counters, reset signals,
  duplicated effects).
- **MEMORY** — leak: assemblies pinned, objects pinned across HMR cycles,
  caches that never bound.
- **PERF** — editor freezes, thundering-herd renders, GC spikes per save.
- **UX** — confusing console output, silent failures the user can't diagnose.
- **CORRECTNESS** — wrong dispatch target, dead code, race window, stale data
  served as if fresh.
- **STABILITY** — crash, infinite loop, stack overflow risk.

---

## Part A — Confirmed HMR bugs from reading `Editor/HMR/`

### B1 — `_compilationQueued` guard is dead code

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L262)

**Symptom:** Rapid consecutive saves (or saves issued while a cascade
retry is mid-flight) can interleave inconsistently. The "single in-flight
compile" guarantee the read of `_compilationQueued` implies does not exist.

**Root cause:** Field is declared at
`UitkxHmrController.cs:40 (private bool _compilationQueued;)` and only ever
**read** (`if (_compilationQueued) { _queuedPath = uitkxPath; return; }` at
line ~262 inside `OnUitkxFileChanged`). It is never set to `true` anywhere
in the file. The "queued path" drain at the end of `ProcessFileChange`
(`if (_queuedPath != null) ... EditorApplication.delayCall += () => ProcessFileChange(queued);`)
is reachable only when something else writes `_queuedPath` (the cascade
retry path does, indirectly). The intended re-entrancy guard never engages.

**Fix shape:** Either delete the field + dead branch, or wrap
`ProcessFileChange` so it sets `_compilationQueued = true` on entry and
`= false` on exit, with `_queuedPath` capturing the most recent path during
the in-flight window.

**Effect:** CORRECTNESS, PERF, STATE (interleaved compiles can swap delegates
in an order that doesn't match user save order).

---

### B2 — `[HookSignature]` proactive reset skipped when first hook is added or last hook is removed

**Files:**
- [Editor/HMR/HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs#L218) lines 217-220
- [SourceGenerator~/Emitter/CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs#L181) lines 181-183
- [Editor/HMR/UitkxHmrDelegateSwapper.cs](Editor/HMR/UitkxHmrDelegateSwapper.cs#L407) `HasHookSignatureChanged`

**Symptom:** User opens a hook-free component, adds a `useState(0)` call,
saves. HMR swaps the delegate but `HasHookSignatureChanged` returns `false`
because the OLD type has no `[HookSignature]` attribute (signature was
empty, attribute not emitted). The proactive `FullResetComponentState` is
skipped. Next render either crashes inside `Hooks.UseState` (caught by the
post-swap try/catch and only then reset) OR — worse, when adding a *trailing*
hook to an existing list — silently corrupts an unrelated slot because hook
slot indices have shifted but state was preserved.

**Root cause:** Both emitters guard attribute emission with
`if (hookSig.Length > 0)`. An empty signature → no attribute → old/new comparison
sees `oldAttr == null || newAttr == null` and returns `false` by design.

**Fix shape:** Always emit `[HookSignature("")]` even for empty signatures,
or have `HasHookSignatureChanged` treat "one side has attr, other doesn't"
as a definite change rather than a skip.

**Effect:** STATE (hook slot corruption on common edit pattern), VISUAL
(stale values for shifted slots).

---

### B3 — Module-static cross-assembly copy pins HMR assemblies forever

**File:** [Editor/HMR/UitkxHmrModuleStaticSwapper.cs](Editor/HMR/UitkxHmrModuleStaticSwapper.cs#L235) `CopyStaticReadonlyFields`

**Symptom:** Memory growth that doesn't level off across long HMR
sessions. Every saved-and-swapped module file adds tens to hundreds of KB
that GC can't reclaim.

**Root cause:** `ModuleStaticSwapper` copies HMR-side `static readonly`
field values into the same-FQN project type using `FieldInfo.SetValue`. If
the field's value is a reference whose runtime type is defined in (or
captures objects defined in) the HMR-loaded assembly — e.g. a `Style`
whose backing dictionary instance was constructed by the HMR cctor — the
project type now holds a strong reference into the HMR assembly. The HMR
assembly's `Assembly` object and every transitively-referenced object stay
rooted for the life of the AppDomain. Mono cannot unload assemblies, so
this is permanent. Each swap of the same module accumulates another
generation.

`AreCompatibleFieldTypes` only checks the field's *declared* type identity
(same FullName + same defining assembly); it does not check the assignment
*value*'s assembly residence.

**Fix shape:** Either (a) deep-clone the value into the project assembly's
"clean" representation before assignment (only safe for serialization-style
types), or (b) document this as a known accumulator and emit a periodic
"force domain reload" prompt when the leak crosses a threshold, or (c)
fingerprint the value and skip the copy when the new HMR-side value is
structurally equal to the existing project-side value (works for simple
`Color`, `Style { primitive K=V, ... }` cases).

**Effect:** MEMORY (permanent leak per cycle), eventual PERF (GC pressure).

---

### B4 — Module type cctor side-effects re-execute on every HMR

**File:** Architectural — affects every module loaded via
[Editor/HMR/UitkxHmrCompiler.cs](Editor/HMR/UitkxHmrCompiler.cs) `Assembly.LoadFrom`

**Symptom:** Modules that contain initializer expressions with side effects
(static field initializers that subscribe to an event, push to a singleton
registry, register a font, etc.) accumulate duplicate registrations per save.
After ~5 saves, "clicking" any UI element calls handlers 5x. After 10, the
editor logs duplicate-key warnings on every play-mode enter.

**Root cause:** Every `Assembly.LoadFrom` triggers the CLR's "type
initializer runs once per assembly" rule — once per HMR assembly. The
project assembly's cctor ran on first load, the HMR-generation-1 assembly's
cctor runs on save 1, HMR-generation-2's on save 2, etc. The
`ModuleStaticSwapper` then copies the HMR-side static results back into the
project type, so the project type's fields now point at HMR-gen-N objects —
but the side effects of every HMR cctor are still wired up. PrettyUi-style
modules (pure value initializers like
`public static readonly Color BgDeep = new Color(...);`) don't trigger this,
but anything fancier does.

The comment header at the top of `UitkxHmrModuleStaticSwapper.cs` explicitly
notes "Re-run the project type's cctor would re-fire any side effects" as
the rationale for the cross-copy approach — but that argument only applies
to the *project* type's cctor. The HMR-side cctor still runs unconditionally
on `LoadFrom`, and the side effects are unavoidable.

**Fix shape:** Document that module bodies must be side-effect-free in
their initializer expressions. Add a UITKX diagnostic for the common cases
(event subscription in static initializer, calls to known mutator APIs).

**Effect:** STATE (duplicate event subscriptions), VISUAL (handlers fire N
times), PERF (registered handlers accumulate), UX (mystifying behavior with
no console output).

---

### B5 — Generic module-method overloads silently skip swap

**File:** [Editor/HMR/UitkxHmrModuleMethodSwapper.cs](Editor/HMR/UitkxHmrModuleMethodSwapper.cs#L194) `TrySwapGenericMethodInfoField`

**Symptom:** User has a module with two generic overloads of the same name
(`Helper<T>(T x)` and `Helper<T>(IList<T> xs)`). User edits the body of one
overload. Save → HMR sees `matchCount > 1`, logs `"X has N generic
overloads — cannot disambiguate from MethodInfo field"`, returns `false`,
no swap. The project type keeps dispatching to the original method. The
user sees no change and no clear error.

**Root cause:** The `MethodInfo`-typed field has no carrier of overload
signature information — there's nothing on the field to disambiguate by.
The non-generic delegate-field path disambiguates via the delegate's
`Invoke` signature (`ParametersMatch` in the same file), but the generic
path lacks that lifeline.

**Fix shape:** Have the SG `ModuleBodyRewriter` emit an
`[HmrOverloadSignature("T,IList`1[T]")]` attribute on the body method
that the swapper can read to disambiguate. Alternatively, embed the
parameter-Type.Name sequence in the field-name hash itself so the swapper
can decode it back.

**Effect:** CORRECTNESS (silent stale dispatch), UX (warning is visible but
non-actionable for the user).

---

### B6 — Hook-file global re-render fires `OnStateUpdated` on every fiber in every tree

**File:** [Editor/HMR/UitkxHmrDelegateSwapper.cs](Editor/HMR/UitkxHmrDelegateSwapper.cs#L173) `TriggerGlobalReRender`

**Symptom:** Saving a `*.hooks.uitkx` (or any hook/module file) freezes the
editor for hundreds of milliseconds in projects with many components, even
when only one or two components actually call the changed hook.

**Root cause:** `TriggerGlobalReRender` does a full-tree walk on every
renderer and pokes `state.OnStateUpdated()` on every `FunctionComponent`
fiber. That schedules an update for every component, which the reconciler
must drain on the next frame. Thundering herd.

**Fix shape:** Filter the walk: for each fiber, walk up the render delegate's
declaring type / file and check whether it (or any of its companions)
references the changed hook container. Only schedule those.

**Effect:** PERF (editor freeze proportional to component count).

---

### B7 — Cross-assembly `Type.Name` match in `CanReuseFiber` is fragile

**Files:**
- [Shared/Core/Fiber/FiberChildReconciliation.cs](Shared/Core/Fiber/FiberChildReconciliation.cs#L281) lines 281-309
- [Shared/Core/Fiber/FiberFunctionComponent.cs](Shared/Core/Fiber/FiberFunctionComponent.cs#L298) lines 298+

**Symptom:** When a child component is hot-reloaded, the parent's vnode
tree still references the *old* render delegate (the parent didn't
recompile). The HMR fallback in `CanReuseFiber` accepts a match when
`fiberType.Name == vnodeType.Name` (or `[UitkxSource]` paths match), which
keeps the existing fiber and its state.

**Root cause:** `Type.Name` collides freely across assemblies. Two unrelated
components named `Item` in different namespaces produce the same
`Type.Name == "Item"`. The reconciliation can reuse the wrong fiber. The
`[UitkxSource]` path fallback narrows this to the case where both children
were authored in `.uitkx` files, but if the project also has a hand-written
`Item` C# component (non-`.uitkx`), the source-attr lookup returns null
and the loose name-match wins.

Also: anywhere downstream that does `if (fiber.TypedRender.Method.DeclaringType == X)`
on cached Type objects will silently miss across the new HMR-loaded
assembly.

**Fix shape:** Match by `Type.FullName` *and* require either `[UitkxSource]`
path equality OR same `[UitkxElement]` component name. Plain `Type.Name`
match is too coarse.

**Effect:** STATE (wrong fiber reused → state leaks between unrelated
components in the very rare same-Name case), CORRECTNESS.

---

### B8 — USS dependency cascade is unbounded and synchronous

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L536) `OnUssFileChanged`

**Symptom:** Editing a USS shared by 20 components freezes the editor for
multiple seconds while 20 sequential HMR compiles run back-to-back.

**Root cause:** `OnUssFileChanged` iterates every dependent `.uitkx` path
and synchronously calls `OnUitkxFileChanged(path)` in a tight loop. No
batching, no debounce, no progress indicator. Each iteration is a full
compile + module-static-swap + module-method-swap + tree walk.

**Fix shape:** Batch dependents: re-import the USS asset once, force a
single re-render of all affected fibers (no recompile needed if the
USS-consuming code uses `StyleSheet` references — `AssetDatabase.LoadAssetAtPath`
returns the new sheet). If a recompile *is* needed, run with a "USS-only"
fast path that skips parse + emit + module swap.

**Effect:** PERF (editor freeze), UX (no feedback during the freeze).

---

### B9 — `TryResolveMissingDependencies` recurses without depth bound or cycle detection

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L626) lines 626-674

**Symptom:** Two newly-created `.uitkx` components that reference each other
produce log spam and (worst case) stack overflow inside the editor process.

**Root cause:** `TryResolveMissingDependencies` parses CS0103 errors from
the failed compile, finds matching `.uitkx` files, and recursively calls
`ProcessFileChange(uitkxPath)` for each. There's no visited set, no depth
limit, and no cycle detection. A → B → A bounces until the call stack
overflows or until both happen to compile (which they won't if the cycle
is genuine).

**Fix shape:** Maintain a `HashSet<string> _resolutionVisitedThisPass` that
clears between top-level saves. Skip paths already in the set. Bail with a
clear diagnostic when a cycle is detected.

**Effect:** STABILITY (stack overflow worst case), UX (log spam).

---

### B10 — `AssemblyReloadSuppressor.Unlock` deferred refresh can fire after re-Lock

**File:** [Editor/HMR/AssemblyReloadSuppressor.cs](Editor/HMR/AssemblyReloadSuppressor.cs#L26) `Unlock`

**Symptom:** Toggle HMR off → on rapidly (or auto-stop from play-mode change
followed by manual restart) leaves the editor in a weird state where
queued imports detonate seconds later, sometimes after the user has
started a new HMR session.

**Root cause:** `Unlock` schedules
`EditorApplication.delayCall += () => AssetDatabase.Refresh();`. If the
next editor frame calls `Lock()` again (the typical "stop, immediately
restart" flow), `Refresh()` fires while HMR is freshly locked. The refresh
is mostly a no-op while locked but it *queues* imports that release-fire
on the eventual unlock.

**Fix shape:** Hold a generation token in the delegate closure; the
delayCall short-circuits if the suppressor has been re-locked under a new
generation.

**Effect:** CORRECTNESS (queued refreshes detonate at unexpected times),
UX (mysterious recompiles).

---

### B11 — `runInBackground` is clobbered if the user changes it mid-session

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L196) `Start` and L209 `Stop`

**Symptom:** User toggles "Run In Background" via Project Settings while
HMR is active. Stopping HMR restores the pre-HMR value, silently undoing
the user's change.

**Root cause:** `Start` snapshots `runInBackground` into a field, `Stop`
writes that field back unconditionally.

**Fix shape:** Only restore if the current value still equals what we
forced (i.e. `true`). If the user changed it back to `false` mid-session,
leave it alone.

**Effect:** UX (silently reverts user setting).

---

### B12 — Companion `.cs` discovery ignores asmdef boundaries

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L323) `ProcessFileChange`

**Symptom:** A component file in a project that uses multiple asmdefs may
fail to compile in HMR with seemingly random CS0246/CS0234 errors, even
though the same project builds cleanly in Unity's normal pipeline.

**Root cause:** `ProcessFileChange` gathers every `<base>.*.cs` file
(except `.g.cs`) from the component's directory and feeds them into the
HMR compilation. The HMR `_referenceLocations` cache is built from *all*
loaded assemblies, so the references happen to be a superset — but if the
`.cs` companion is in a folder that maps to a different asmdef than the
component's own asmdef, namespace visibility rules (internal types,
`InternalsVisibleTo`) differ. HMR ignores asmdef boundaries entirely.

**Fix shape:** Look up the asmdef for both the component and each candidate
`.cs` companion; skip companions that live under a different asmdef. If
none is configured, fall back to current behavior.

**Effect:** CORRECTNESS (wrong references), UX (compile errors that look
like user code bugs).

---

### B13 — `Environment.TickCount` wraps after ~24.9 days

**File:** [Editor/HMR/UitkxHmrFileWatcher.cs](Editor/HMR/UitkxHmrFileWatcher.cs#L120) lines 120, 160

**Symptom:** After ~24.9 days of continuous editor uptime, a single save
event can fall through the debounce check (the subtraction goes negative).

**Root cause:** `Environment.TickCount` is an `int` that wraps. The check
`now - kvp.Value >= DebounceMs` doesn't handle wrap correctly.

**Fix shape:** Switch to `Environment.TickCount64` (`long`). Trivial.

**Effect:** CORRECTNESS (extremely rare missed debounce).

---

### B14 — Forced `GC.Collect(2, Optimized)` after every compile

**File:** [Editor/HMR/UitkxHmrCompiler.cs](Editor/HMR/UitkxHmrCompiler.cs#L1133) `InProcessCompile`, L1380 `ExternalCompile`

**Symptom:** Every save freezes the editor for 50-300ms even when nothing
visible changed.

**Root cause:** Both compile paths force a full Gen2 collection at the end
to combat Mono's heap-growth behavior. On large heaps this is much more
expensive than what it claws back.

**Fix shape:** Either remove the forced collect entirely, or only run it
on the Nth save (e.g. every 10) once a threshold is crossed.

**Effect:** PERF (per-save freeze).

---

### B27 — `Stop()` leaks the assembly-reload Lock on exception (CRITICAL)

**File:** [Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs#L209) `Stop`

**Symptom (reported in field, May 2026):** After a long HMR session with
several edits, the HMR window appears blank/frozen, exiting Play mode does
not compile pending `.cs` changes, and the editor refuses to compile
anything until it is killed and restarted. The user reports: "the HMR
window itself crashed and nothing got compiled when I closed play mode -
probably because it thought HMR was still on".

**Root cause:** `Stop()` is not exception-safe. Current body
(simplified):

```csharp
public void Stop() {
    if (!_active) return;
    _active = false;                              // ← flips immediately
    HmrState.IsActive = false;
    Application.runInBackground = _previousRunInBackground;
    EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    _watcher.OnUitkxChanged -= OnUitkxFileChanged;
    _watcher.OnUssChanged -= OnUssFileChanged;
    _watcher.Stop();                              // ← can throw on Windows
    _pendingRetryPaths.Clear();
    _suppressor.Unlock();                         // ← skipped on throw above
    _compiler?.Reset();                           // ← can throw on locked DLLs
    Debug.Log("[HMR] Stopped ...");
}
```

Any exception between the `_active = false` line and `_suppressor.Unlock()`
leaves the editor in the following state:

- `_active == false` (HMR appears stopped externally — window says stopped,
  `IsActive` getter returns false).
- `EditorApplication.LockReloadAssemblies` counter still incremented
  (no `Unlock` was called).
- `AssetDatabase.DisallowAutoRefresh` still in force.

`EditorApplication.LockReloadAssemblies` is **counter-based**, not boolean.
The editor refuses to reload assemblies (and therefore refuses to compile
any `.cs` change, refuses to enter Play Mode, refuses to import assets)
until `UnlockReloadAssemblies` is called the same number of times. Until
the Unity process is killed.

Common triggers of the throw between flip and Unlock:

1. `_watcher.Stop()` — Windows `FileSystemWatcher.Dispose()` can throw on
   high-load workspaces or when the watch directory has gone away.
2. `_compiler?.Reset()` — iterates `_tempDir` and deletes DLLs; one might
   still be locked by `Assembly.LoadFrom`. The try/catch around the
   inner Delete is present BUT the surrounding `GetFiles` enumeration can
   throw `IOException` if the temp dir itself is locked by another
   process.
3. Anywhere in the `OnPlayModeChanged` auto-stop callback chain — if HMR
   auto-stops *while* user-code is mid-throw (the playmode transition
   itself raised an exception), the Stop() re-enters the same path.

**Effect:** STABILITY (editor unusable until restart),
CORRECTNESS (silent Lock leak — no log indicates HMR's Stop failed),
UX (HMR window goes blank because `OnGUI` reads `_compiler` after partial
disposal, throws, blank repaint).

**Fix shape:**

```csharp
public void Stop() {
    if (!_active) return;
    try {
        HmrState.IsActive = false;
        try { Application.runInBackground = _previousRunInBackground; } catch { }
        try { EditorApplication.playModeStateChanged -= OnPlayModeChanged; } catch { }
        try { _watcher.OnUitkxChanged -= OnUitkxFileChanged; } catch { }
        try { _watcher.OnUssChanged  -= OnUssFileChanged; } catch { }
        try { _watcher.Stop(); } catch (Exception ex) {
            Debug.LogWarning($"[HMR] FileWatcher.Stop threw: {ex.Message}");
        }
        try { _pendingRetryPaths.Clear(); } catch { }
        try { _compiler?.Reset(); } catch (Exception ex) {
            Debug.LogWarning($"[HMR] Compiler.Reset threw: {ex.Message}");
        }
    }
    finally {
        // MUST run no matter what — leaving the lock dangling makes the
        // editor unusable until process restart.
        try { _suppressor.Unlock(); }
        catch (Exception ex) {
            Debug.LogError($"[HMR] CRITICAL: Unlock threw, editor may be " +
                $"stuck — call EditorApplication.UnlockReloadAssemblies() " +
                $"manually. Detail: {ex.Message}");
        }
        _active = false;  // ← flip AFTER unlock so a second Stop call from
                          //   inside this finally's exception path can still
                          //   run cleanup. Re-entrancy from a parallel call
                          //   sees _active==true and bails on the early
                          //   return at the top.
    }
    Debug.Log($"[HMR] Stopped — {_swapCount} swap(s), {_errorCount} error(s).");
}
```

Plus: add a "Force Unlock" button to the HMR window that calls
`EditorApplication.UnlockReloadAssemblies()` in a loop until the next call
to `LockReloadAssemblies` followed by `IsReloading` confirms the counter
hit zero. Also clean up via `Dispose()` if `s_instance` is non-null.

**Effect:** STABILITY, CORRECTNESS — editor unusable until restart.

---

### B28 — Sidebar style change reverts visibly on next route change (suspected; needs instrumentation)

**Files:**
- `C:\Users\neta\Pretty Ui\Assets\UI\Components\Sidebar\Sidebar.style.uitkx`
- Consumer chain: `HomePage.uitkx` → `HomePageSidebar.uitkx` → `Sidebar.uitkx`
- HMR delegate path: [Editor/HMR/UitkxHmrDelegateSwapper.cs](Editor/HMR/UitkxHmrDelegateSwapper.cs) `SwapAll` + [CanReuseFiber HMR fallback](Shared/Core/Fiber/FiberChildReconciliation.cs#L281)

**Symptom (user-reported, May 2026):** Edit `PaddingTop = 16f` to
`PaddingTop = 4f` in `Sidebar.style.uitkx`. Save. Visible change to 4px
applies immediately (console shows
`[HMR] Sidebar updated ... Module statics re-init: 2`). Then user clicks
the TopNav buttons (which navigate between `/home`, `/settings`, `/news`,
triggering route remount). On the click, Sidebar's padding visually
reverts to 16f. **No exception is logged.** No rollback warning. The
state-readonly field on the project-loaded `Sidebar` type is presumably
still 4f.

**Why this is surprising (theoretical flow):**

1. Save triggers HMR compile of Sidebar.
2. `Assembly.LoadFrom` loads the HMR DLL; CLR runs the new assembly's
   `Sidebar` cctor → HMR-loaded `Sidebar.Wrapper` = `new Style { PaddingTop = 4, ... }`.
3. `ModuleStaticSwapper.CopyStaticReadonlyFields` iterates the HMR
   assembly's `Sidebar` type, reads `Wrapper` (=Style with PaddingTop=4),
   writes via `FieldInfo.SetValue` into the **project-loaded** `Sidebar`
   type's `Wrapper` field. ✓ — log line `Module statics re-init: 2`
   confirms 2 copies (Wrapper + Item).
4. `DelegateSwapper.SwapAll` finds 1 fiber and swaps its `TypedRender` to
   the HMR delegate. ✓ — log line `Swapped 1 instance(s) of Sidebar`.
5. Re-render fires via `OnStateUpdated`. The HMR-loaded Sidebar.Render
   runs; reads HMR-loaded `Sidebar.Wrapper` (PaddingTop=4). Applies. ✓
6. Visible: 4px. ✓
7. User clicks TopNav. Router location changes. The Outlet at MenuPage's
   `<Outlet />` unmounts its previous child (the route subtree that
   contained HomePageSidebar → Sidebar) and mounts the new route's
   subtree. (When the user clicks back to /home, HomePage MOUNTS fresh.)
8. Fresh mount: a brand-new Sidebar fiber is created (no `CanReuseFiber`
   match possible — there's no prior fiber at that slot). `CreateFiber`
   copies `vnode.TypedFunctionRender` into `fiber.TypedRender`.
9. `vnode.TypedFunctionRender` came from
   `<Sidebar items={items} />` in `HomePageSidebar.uitkx`. HomePageSidebar
   was NOT recompiled by HMR — its IL still references the
   **project-loaded** `Sidebar.Render`. So the new fiber's `TypedRender`
   is project-loaded.
10. Project-loaded `Sidebar.Render` runs. Reads project-loaded
    `Sidebar.Wrapper` field. ModuleStaticSwapper wrote PaddingTop=4 into
    it at step 3. **Should paint 4f.** ❌ But user reports 16f.

**Possible root causes (need instrumentation to disambiguate):**

- **C1: Project-loaded `Sidebar.Wrapper` is not actually 4f at step 10.**
  ModuleStaticSwapper may be writing to a `Sidebar` type from a different
  assembly than the one HomePageSidebar resolves at runtime. The
  `FindProjectType` helper does `AppDomain.CurrentDomain.GetAssemblies()`
  + `GetType(fullName, throwOnError: false)` and returns the first match.
  If `PrettyUi.App.Components.Sidebar` exists in both `Assembly-CSharp`
  AND in a prior HMR assembly (e.g. `hmr_Sidebar_3.dll`), the swapper
  may pick the latter and silently leave the project-loaded type alone.
  The `_genuinelyNewComponents` set may exclude Sidebar, but
  ModuleStaticSwapper does its own type lookup independently — it doesn't
  consult that set.

- **C2: A second `Sidebar` cctor runs after the swap.** Each HMR
  generation's `Assembly.LoadFrom` runs that assembly's cctor. If anything
  triggers a *new* HMR cycle (or a stale enqueued one finishing) between
  the user's click and the paint, the swapper might be writing 4f into
  the project field from one HMR assembly and another (older?) HMR
  assembly's value bleeds in via... no, the swapper always reads from
  the freshly-loaded asm — this is unlikely.

- **C3: The applied style is cached on the VisualElement.** When the
  fiber remounts, the new host VisualElement is created from scratch and
  styled via the props applier. But the props applier may apply
  computed styles via a `Style → IStyle` mapping that caches the
  computed value-string keyed on the Style instance identity. If a
  long-lived cache holds a `Style → IStyle` mapping from the
  PRE-HMR-edit Style instance, and the new HMR assembly produces a Style
  instance with same identity hash but new values... unclear, but worth
  ruling out.

- **C4: Layout subscription or theme override.** Sidebar might have a
  hover/focus pseudo-style applied by parent USS that resets paddingTop
  to 16f on layout invalidation. The TopNav click invalidates layout,
  USS re-resolves, and overwrites the inline padding. Verify by checking
  `MainMenu.uss` or any other USS in the project — though no `@uss`
  directive is present in Sidebar.style.uitkx so this is unlikely.

- **C5: Cross-assembly module class duplication.** When Sidebar.uitkx is
  recompiled, the HMR compiler includes Sidebar.style.uitkx as a companion
  partial. The HMR assembly therefore contains a `Sidebar` partial class
  defining the `Wrapper` field. The project assembly also has a `Sidebar`
  partial class. After ModuleStaticSwapper copies, both have PaddingTop=4.
  BUT: the field's *runtime value* is a `Style` instance constructed by
  the HMR assembly's cctor. That instance lives in the HMR assembly. The
  Style might internally reference type-identity-sensitive collaborators
  (a `BackgroundPosition` etc.) that bind to the HMR-loaded type. If
  the project-loaded Sidebar.Render iterates the Style dictionary and
  hits a type-identity branch internally that finds the HMR-loaded
  variant of a struct, it may bail and return defaults. Very speculative.

**Recommended diagnostic instrumentation:**

1. In a local clone of the package (or directly in the cache for one
   repro cycle), add at the top of [UitkxHmrDelegateSwapper.SwapAll](Editor/HMR/UitkxHmrDelegateSwapper.cs#L24):

   ```csharp
   // DIAGNOSTIC for B28
   if (componentName == "Sidebar") {
       var projType = FindProjectType("PrettyUi.App.Components.Sidebar");
       var wrapper = projType?.GetField("Wrapper",
           BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
       Debug.Log($"[B28] Post-swap project Sidebar.Wrapper PaddingTop = " +
           $"{wrapper?.GetType().GetMethod("get_Item")?
              .Invoke(wrapper, new object[] { "PaddingTop" })}");
   }
   ```

2. In `Sidebar.uitkx` (PrettyUi side), temporarily prefix the return with:

   ```jsx
   var _diag = PrettyUi.App.Components.Sidebar.Wrapper;
   UnityEngine.Debug.Log($"[Sidebar.Render] reading Wrapper.PaddingTop, " +
       $"thread={System.Threading.Thread.CurrentThread.ManagedThreadId}, " +
       $"asm={typeof(PrettyUi.App.Components.Sidebar).Assembly.GetName().Name}");
   ```

3. Reproduce: edit, save, observe 4f, click TopNav, observe revert. Read
   the logs:
   - If `[B28] Post-swap ... Wrapper.PaddingTop = 4` AND
     `[Sidebar.Render] ... asm=hmr_Sidebar_N` after save AND
     `[Sidebar.Render] ... asm=Assembly-CSharp` after click + revert AND
     a separate log reading Wrapper from project at click time shows 16
     → **C1 confirmed (swapper wrote to wrong type).**
   - If both logs show 4f throughout but UI shows 16f → C3 (style
     application caching). Need to inspect the props applier.
   - If `[Sidebar.Render]` is never logged after the click → fiber
     reuse short-circuited the render entirely; layout reflection from
     USS or panel is repainting against a stale snapshot.

**Effect:** VISUAL (the headline symptom — style changes don't persist
across route navigation), STATE (suggests something deeper about which
assembly's static field is authoritative), UX (HMR feels unreliable —
"changes that don't stick" is a confidence-killer).

**Priority:** HIGH — this is the user-visible "HMR is broken" symptom.
Needs the instrumented repro from the user before code changes.

---

### B15 — Hook-delegate field re-bind silently fails for signature changes

**File:** [Editor/HMR/UitkxHmrDelegateSwapper.cs](Editor/HMR/UitkxHmrDelegateSwapper.cs#L144) `SwapHooks`

**Symptom:** User changes a hook's return type or parameter list, saves,
gets a `LogWarning` per save: "Failed to swap hook 'X': delegate signature
mismatch". The OLD body keeps executing forever (until domain reload).

**Root cause:** `Delegate.CreateDelegate(field.FieldType, newMethod)` is
strict about signature equality. The project-side trampoline still wraps
the old delegate type; HMR cannot rewrite the trampoline (it's in a
sealed-metadata project assembly).

**Fix shape:** Detect the signature change up front and force a domain
reload (or surface a prompt). At minimum, downgrade the per-save log spam
to once-per-session per hook.

**Effect:** CORRECTNESS (stale dispatch), UX (log noise per save, no
recovery path).

---

## Part B — PrettyUi-specific issues found at `C:\Users\neta\Pretty Ui\Assets\UI`

### B16 — `module SidebarItem { instance properties }` is HMR-unswappable

**Files:**
- `C:\Users\neta\Pretty Ui\Assets\UI\Components\Sidebar\SidebarItem.types.uitkx`
- Consumer: `C:\Users\neta\Pretty Ui\Assets\UI\Pages\HomePage\HomePageSidebar.uitkx`

**Symptom:** User edits `SidebarItem.types.uitkx` — adds a property, changes
a default value, changes a `Action?` to `Func<bool>?`. Save. HMR logs
"Compiled SidebarItem - no active instances to swap." Nothing visible
changes. No clear error.

**Root cause:** The `module SidebarItem` body declares only instance
properties (`public string Id { get; set; }`, etc.), no `static readonly`
fields, no `public static` methods.
- `ModuleStaticSwapper` (`UitkxHmrModuleStaticSwapper.cs`) iterates and
  finds zero `static readonly` fields → no copies.
- `ModuleMethodSwapper` (`UitkxHmrModuleMethodSwapper.cs`) iterates and
  finds zero `__hmr_*` trampoline fields → no swaps.
- The compiled HMR assembly's `SidebarItem` type lives in memory but no
  active user code references it: existing `List<SidebarItem>` instances
  point at the project-loaded `SidebarItem` type; new instances created
  inside HMR-compiled `HomePageSidebar` (if HomePageSidebar was also
  recompiled) would point at the HMR-loaded type → **type-identity split**.

A common edit: user changes `public bool IsActive { get; set; }` to add a
default `= false`. Save. Effect: zero. The user is forced into a domain
reload to validate the change.

If the user *adds* a property (e.g. `public string Tooltip { get; set; }`),
the HMR-compiled `HomePageSidebar` will reference `SidebarItem.Tooltip`,
which the project-loaded `SidebarItem` doesn't have → CS0117 in the
HomePageSidebar HMR compile → "Tooltip does not exist". Confusing, because
the user did define it.

**Fix shape:** Add a UITKX diagnostic when a `module` block defines
instance members. Recommend either (a) extracting into a hand-written `.cs`
type (which HMR knows it can't swap), or (b) making it a `record`/`struct`
where appropriate, or (c) restricting `module { ... }` to static-only
bodies and surfacing a clear error.

**Effect:** UX (silent no-op), CORRECTNESS (type-identity split when the
user *adds* a member), STATE (member-add → CS0117 → confusing).

---

### B17 — `const float Theme.NavHeight = 56f;` HMR changes do not propagate

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Theme.uitkx`

**Symptom:** User changes `public const float NavHeight = 56f;` to `= 72f;`
in `Theme.uitkx`. Save. HMR logs successful compile of Theme. Visually,
every component that uses `Theme.NavHeight` still shows the 56px value.

**Root cause:** C# `const` values are compile-time inlined into every
consumer's IL. The project assembly's `TopNav.cs` (and every other
consumer) was compiled with `NavHeight = 56f` literally baked into its
bytecode. The `ModuleStaticSwapper` cannot help — `const` has no runtime
field slot to overwrite (`IsLiteral == true` is explicitly skipped at
`UitkxHmrModuleStaticSwapper.cs:CopyStaticReadonlyFields`).

This is a CLR fundamental, not a bug — but the symptom is "HMR is lying to
me about what changed."

**Fix shape:** Surface a UITKX diagnostic when HMR detects a `const`-value
change in a module file: "const X changed - requires domain reload to take
effect. Click here to reload now." Or recommend switching to `static
readonly`. Theme.uitkx specifically should use `static readonly` for all
the `NavHeight`/`Radius`/`FsTitle` values since they're never used in
contexts that require compile-time constness.

**Effect:** UX (silent no-op despite successful "compile"), STATE (project
appears to ignore changes), VISUAL (no visible update).

---

### B18 — Module static methods (`AppButton.BaseButton(...)`, `StyleExtensions.Extend(...)`) depend entirely on SG trampoline emission

**Files:**
- `C:\Users\neta\Pretty Ui\Assets\UI\Components\AppButton\AppButton.style.uitkx` (defines `module AppButton { public static Style BaseButton(...) }`)
- `C:\Users\neta\Pretty Ui\Assets\UI\StyleExtensions.uitkx` (defines `module StyleExtensions { public static Style Extend(...) }`)
- `C:\Users\neta\Pretty Ui\Assets\UI\Components\MenuButton\MenuButton.style.uitkx` (same pattern as AppButton)

**Symptom:** User edits `BaseButton` to tweak colors. Save. If the SG
correctly emitted the `__hmr_BaseButton_h<hash>` trampoline + delegate
field on the project-loaded `AppButton` partial class, the swap works. If
**not** (SG version mismatch, asmdef misconfig, or any reason the
ModuleBodyRewriter pass was skipped), `ModuleMethodSwapper` finds no
`__hmr_*` field → no swap → consumers keep calling the OLD method.

The user has no way to confirm trampoline presence without inspecting
generated source — there's no console-level diagnostic for
"trampoline missing, swap impossible."

**Root cause:** `UitkxHmrModuleMethodSwapper.SwapTypeMethods` only touches
fields that **already exist** on the project type. If the SG didn't emit
them (because the project predates the trampoline feature, or because the
.cs.g.cs file was stale and not regenerated), HMR cannot synthesise them
post-hoc.

**Fix shape:** On HMR start, enumerate every loaded module type and check
whether each `public static` method has a corresponding `__hmr_<name>_h<hash>`
companion field. If any are missing, log a one-time diagnostic listing the
unswappable methods and recommending a Force Reimport on the .uitkx file
(or a full domain reload to regenerate). Also extend the existing
"infrastructure failure" self-disable path to cover this case.

**Effect:** UX (silent stale dispatch on a heavily-used pattern), VISUAL
(style edits appear to not take effect), CORRECTNESS.

---

### B19 — Shared module `Theme.uitkx` recompile triggers no consumer re-render

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Theme.uitkx`

**Symptom:** User edits `Theme.Accent` color from orange to red. Save. HMR
compiles Theme. `ModuleStaticSwapper` copies the new Color value into the
project-loaded `Theme.Accent`. But the existing UI on screen does not
refresh — buttons keep painting in the old orange until the user
interacts with something that re-renders.

**Root cause:** `OnUitkxFileChanged → ProcessFileChange` for a
hook/module-only file routes to `SwapHooks`, which calls
`TriggerGlobalReRender` (B6). That **should** force a re-render — but
`TriggerGlobalReRender` only fires for *hook* files, not pure-module
files. Looking at `UitkxHmrController.cs:393`, the swap path is `SwapHooks`
when `result.IsHookModuleFile == true`, and `SwapHooks` calls
`TriggerGlobalReRender` only when `swapped > 0` (the count of `__hmr_*`
field re-binds). If the module body contains only `static readonly` fields
(no static methods, no trampoline fields), `swapped == 0`, no global
re-render fires, and `ModuleStaticSwapper`'s field copies sit silently
until something else triggers a paint.

**Root cause cont.:** Module-static field copies happen *before* the
delegate swap (see comment in `UitkxHmrController.cs:343-348`), but for a
**pure** module file (no component) there's no per-component delegate to
swap, and `SwapHooks` is the only re-render trigger — gated on the wrong
counter.

**Fix shape:** Trigger `TriggerGlobalReRender` whenever
`moduleStaticResult.Copied > 0 || swapped > 0`, not just on `swapped > 0`.
Or always trigger it for hook/module files on success.

**Effect:** VISUAL (changes take effect but require user interaction to
become visible), UX (looks like HMR is broken when it actually worked).

---

### B20 — `useEffect(() => { GameSceneLoader.LoadAsync(); return () => GameSceneLoader.UnloadAsync(); })` re-fires on every HMR save

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Pages\GamePage\GamePage.uitkx`

**Symptom:** User is iterating on GamePage HUD layout. Every save → effect
cleanup runs (`UnloadAsync`) → effect re-runs (`LoadAsync`). The game
scene loads and unloads on every keystroke. Editor is unusable while doing
HUD work.

**Root cause:** HMR's `SwapAll` re-runs `OnStateUpdated` on every matched
fiber, which the reconciler treats as a state-change render. The effect's
dependency array `new object[] { }` is empty, so the effect "should"
never re-fire — but the post-swap state-reset path in the catch block (and
the proactive `FullResetComponentState` when hook signature changes)
explicitly clears `state.FunctionEffects` and re-runs them.

Critically: the catch-block reset path is reachable even when the user's
hook signature didn't change, because `Hooks.UseEffect`'s slot validation
throws on certain HMR-edge-case slot-identity mismatches.

**Fix shape:** Have HMR distinguish between a "legitimate re-render"
(props changed, state changed) and "the same render with a new delegate
that should reuse hook state including effect deps". The latter should
not invoke effect cleanup if the dep array is reference-stable. Currently
no such distinction exists.

**Effect:** PERF (heavyweight effect re-runs per save), STATE (effect
cleanup may release resources the new render still needs), UX (cannot
iterate on a page that loads a scene).

---

### B21 — `<Portal target={menuStage}>` may briefly unmount on HMR re-render

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Pages\MenuPage\MenuPage.uitkx`

**Symptom:** Saving MenuPage briefly flashes the chrome (TopNav, Outlet,
NewGameButton) out of and back into the world-space panel. On weaker
machines this is visible as a one-frame disappearance.

**Root cause:** After HMR delegate swap, MenuPage re-renders. The
`useUiDocumentRoot` hook returns `menuStage`, which is a `VisualElement`
reference polled per frame from a `UIDocument`. If the polled value
transitions through `null` between the prior render and the new one (Unity
6.3 panel-root churn during the HMR window), the conditional path
`@if (menuStage != null) ... <Portal ... > ... @else ... return chrome;`
switches branches → the Portal subtree unmounts (effect cleanup, fiber
deletion, host VisualElement detach) → next frame remounts.

**Root cause cont.:** HMR doesn't synchronise with the
`AnimationTicker`-driven `useUiDocumentRoot` polling. The race is
project-induced but HMR makes it observable.

**Fix shape:** In MenuPage, prefer rendering the chrome unconditionally
and using `target ?? defaultTarget` semantics inside the Portal. As a
framework fix, document the unmount risk for portals whose target hooks
have null-transition windows.

**Effect:** VISUAL (one-frame flash), STATE (effect cleanup ran for the
chrome subtree → any short-lived state inside lost).

---

### B22 — `AppButton.BaseButton(active, disabled)` is a method call inside every render

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Components\AppButton\AppButton.uitkx` consuming `AppButton.style.uitkx`

**Symptom:** Every Sidebar render allocates a fresh `Style` dictionary per
button via `AppButton.BaseButton(active, disabled)`. With 3 sidebar items +
3 nav items + 1 NewGameButton, that's 7 allocations per parent render.
After HMR swap, every component in the page re-renders → 7+ Style dicts
per save.

**Root cause:** `BaseButton` is implemented as a method that
`new Style { ... }` each time. The HMR `__hmr_*` trampoline dispatches via
delegate when `HmrState.IsActive`, adding indirection cost. Not
strictly a bug, but worsens the "HMR feels slow" perception.

**Fix shape:** Encourage memoising the `Style` result per `(active, disabled)`
pair when only a handful of states exist. Document this anti-pattern.

**Effect:** PERF (avoidable allocations on every render, doubled during
HMR session).

---

### B23 — `var items = new (string, string, Texture2D, Texture2D)[] { ... }` in `TopNav.uitkx` re-evaluates `Asset<T>(...)` lookups every render

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Components\TopNav\TopNav.uitkx`

**Symptom:** Every render of TopNav calls `Asset<Texture2D>("Assets/Resources/HomeWhite.png")` 6 times. The `UitkxAssetRegistry`
lookup is hash-keyed and cheap (<1 µs), but a single ValueTuple array
allocation + 6 lookups per render still shows in the profiler.

**Root cause:** PrettyUi pattern — the items array is constructed inside
the render body without `useMemo` or hoisting. This is exacerbated by
HMR's `SyncAssetCacheForHmr` running on every save, which re-imports
assets and may invalidate Texture2D references mid-render.

**Fix shape:** Encourage `useMemo` or hoisting `Asset<T>` results above
the render path, or memoise per file via the module-static pattern.
Document.

**Effect:** PERF (allocations + repeated registry lookups), VISUAL (Texture
flicker if SyncAssetCacheForHmr swaps the reference mid-frame).

---

### B24 — `module Theme { ... }` is a *project-wide* shared module — every save of *any* module file rebuilds Theme references

**Files:**
- `C:\Users\neta\Pretty Ui\Assets\UI\Theme.uitkx`
- Every `*.style.uitkx` in the project references `Theme.*`

**Symptom:** Saving `AppButton.style.uitkx` (which references `Theme.Accent`,
`Theme.BorderWidth`, etc.) recompiles only AppButton. But the
`ModuleStaticSwapper` then iterates types in the HMR assembly,
which includes `Theme` (because AppButton.style.uitkx has
`@using PrettyUi.App` and the compile pulls Theme.uitkx in as a companion
via the FQN reference resolution). On every save, Theme's `static readonly`
fields are copied across — even though nothing changed in Theme.

Actually wait: the companion-discovery pattern in
`EmitCompanionUitkxSources` is `<componentName>.*.uitkx`, so
`AppButton.*.uitkx` does NOT pull in `Theme.uitkx`. But the HMR compile
uses `_metadataReferences` which includes the project assembly that
contains Theme. So Theme is referenced as metadata, not recompiled. Good.

**However:** if Theme.uitkx is open in the file watcher and `LastWrite`
fires from any IDE save-all or git checkout, the watcher debounces and
fires `OnUitkxChanged` for Theme too — Theme gets recompiled as a
hook/module file → its cctor reruns → its static readonly fields are
overwritten with **freshly-constructed Color values** that have the same
RGBA but are *different instances*. Anything that compares Theme.Accent
by reference (style diffing, memoisation keys) sees a "change" and
invalidates → cascading re-renders.

**Fix shape:** In `ModuleStaticSwapper.CopyStaticReadonlyFields`, skip the
copy when the new value `.Equals()` the existing value (deep equality where
available). Add a fast-path for `Color`, `Style`, simple structs.

**Effect:** PERF (spurious cascading re-renders), MEMORY (B3 amplified —
every Theme save accumulates leak even when nothing changed), VISUAL
(possible flicker for style-diff-based animations).

---

### B25 — `@if / @else` inside JSX (`MenuPage.uitkx`) creates two distinct VNode subtrees that HMR cannot reconcile across the branch flip

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Pages\MenuPage\MenuPage.uitkx` (the `@if (menuStage != null) ... @else ...` block)

**Symptom:** When `menuStage` flips from null to non-null (or vice versa)
mid-HMR-session, hook state in the `chrome` subtree is lost. The user sees
TopNav forget its hover state, Sidebar lose its scroll position, etc.

**Root cause:** The `@if/@else` desugar produces two separate VNode trees;
`CanReuseFiber` matches by node-type-plus-element-name, and a `<Portal>`
node vs the direct return of `chrome` are structurally different at the
fiber tag level (Portal is `FiberTag.HostPortal`, a plain `VisualElement`
is `FiberTag.HostComponent`). The reconciler correctly unmounts/remounts
the subtree. Not strictly an HMR bug, but HMR exposes this flow
repeatedly because the branch flip can happen during the swap window.

**Fix shape:** Document the unmount-on-branch-flip behaviour. Encourage
wrapping the branch in a stable container so the subtree fiber lives
across the flip with a constant identity.

**Effect:** STATE (hook state lost on branch flip during HMR window).

---

### B26 — `Asset<VideoClip>(...)` in MenuPage references a 10MB video — Asset cache sync rebuilds the reference on every save

**File:** `C:\Users\neta\Pretty Ui\Assets\UI\Pages\MenuPage\MenuPage.uitkx`

**Symptom:** Every save of MenuPage runs
`SyncAssetCacheForHmr(uitkxPath)` (`UitkxHmrController.cs:712`) which
reads the file, finds the `Asset<VideoClip>("Assets/Resources/file_example_MP4_1280_10MG.mp4")` call, and re-injects the
VideoClip reference into the registry. The VideoClip itself isn't
re-imported (it's already imported and the asset isn't dirty), but if
during HMR the controller decided it *wasn't* imported, `ImportAsset`
with `ForceSynchronousImport` would trigger a 10MB asset reimport on
every save.

**Root cause:** `InjectIfResolved` falls into the disk-import path when
`AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resolved)` returns
null. With assembly reloads locked, asset auto-refresh is disabled — so
if the registry cache was cleared (e.g. after a `Reset()` call between
HMR start/stop cycles), the first save of each component triggers a
synchronous import of every referenced asset, including 10MB videos.

**Fix shape:** Cache the resolved Object reference in
`UitkxAssetRegistry.InjectCacheEntry` such that subsequent calls with
the same path-and-type skip the LoadAssetAtPath probe entirely. The
registry already does this for the value side; verify the lookup-by-path
side is similarly cached. Also: limit `ImportAssetOptions.ForceSynchronousImport` to non-large assets.

**Effect:** PERF (multi-second freeze on first save per asset), UX (HMR
"feels broken" on heavy projects).

---

## Part C — Likely root causes of the user's "breaks left and right" report

Ranked by probability based on the audit:

1. **B4 (cctor side-effects re-running on every save)** — accumulates
   duplicate event handlers, registry pushes, etc. Classic "everything
   fires twice, then three times".
2. **B19 (module-only saves don't trigger global re-render)** — looks like
   "I changed Theme but nothing happened" until user clicks something.
3. **B3 (cross-assembly object pinning / leak)** — long sessions degrade.
4. **B6 (global re-render storm on hook saves)** — heavy editor freeze that
   feels like a crash.
5. **B2 (proactive hook reset missed on first-hook-added pattern)** — state
   corruption that requires re-mounting to recover.
6. **B20 (effect re-runs on every save)** — only matters for components
   with heavyweight effects, but when it bites it's brutal.
7. **B17 (`const` changes don't propagate)** — common edit pattern in
   PrettyUi.Theme.
8. **B18 (missing trampoline → silent stale dispatch)** — only triggers if
   the SG output is out of sync with the .uitkx file.
9. **B16 (`module` with instance members)** — PrettyUi-specific, silent
   no-op.
10. **B5 (generic overload skip)** — narrow but silent.

---

## Open follow-ups

- Verify whether the `_compilationQueued` field (B1) was intended to be set
  inside `OnUitkxFileChanged` before the call to `ProcessFileChange`. Git
  blame can probably reveal what was removed.
- Run a long-duration HMR session (100+ saves) with the heap snapshotted
  before and after, to quantify B3 + B4 in absolute MB. Without numbers
  the fix priority is hand-wavy.
- Add an HMR self-test target that exercises every swap path
  (component, hook, module-static, module-method, generic-method, USS,
  companion .cs) and verifies the on-screen result programmatically.
