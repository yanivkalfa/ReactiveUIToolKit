# UITKX Optimization Plan

> **Status:** ✅ COMPLETE — optimization effort concluded April 25, 2026
> **Date:** April 2026 (verified April 25)
> **Scope:** Core library (`Shared/`), Runtime (`Runtime/`), Source Generator (`SourceGenerator~/`), IDE extensions (`ide-extensions~/`)
> **Baseline:** Pure UITK 47.7 FPS vs UITKX 28.3 FPS at 3000 boxes/60s runtime — 1.7x overhead
> **Final:** 36-38 FPS (~78% of native UITK) after all optimizations (April 25, 2026)
> **Conclusion:** Remaining ~10 FPS gap is the irreducible cost of the reconciler abstraction when 100% of elements change every frame. Real apps with partial updates will be much closer to native.

---

## Completed Items (April 25, 2026)

| Item | Result | Details |
|------|--------|---------|
| **OPT-1 Layer 2**: Typed Props Pipeline | ✅ **Done** | Eliminated ~6,000 dict allocs/frame. See `Plans~/archive/TYPED_PROPS_PIPELINE.md` |
| **OPT-1 Layer 1**: Typed host props equality | ✅ **Done** | Subsumed by Layer 2 |
| **OPT-6**: AreHostPropsEqual ref fast-path | ✅ **Done** | Free with typed pipeline |
| **Typed Style Pipeline** | ✅ **Done** | Eliminated ~21,000 boxing+dict allocs/frame. See `Plans~/archive/TYPED_STYLE_PIPELINE.md` |
| **OPT-16**: Style & BaseProps Object Pooling | ✅ **Done** | Eliminated ~6,000 object allocs/frame. Pool at 99% hit rate steady state. See `Plans~/STYLE_BASEPROPS_POOLING.md` |
| **OPT-18**: VirtualNode Object Pooling | ✅ **Done** | Eliminated ~3,200 VNode allocs/frame (~538 KB/frame). Generation-stamped pool. See `Plans~/VNODE_POOLING.md` |
| **OPT-10**: IIFE Closure Elimination | ✅ **Done** | Eliminated ~3,000 per-iteration delegate closures/frame. `return` → `__r.Add(); continue;` rewriting. Also fixed `break`/`continue` bug in `@for`/`@while`. See `Plans~/FOREACH_IIFE_ELIMINATION.md` |
| **OPT-22**: Event Handler Skip in ApplyDiff | ✅ **Done** | Added `_hasEvents` bool to BaseProps — skips 43 `DiffEvent()` calls per element when neither prev nor next has any event handler. +2 FPS (35→37). |
| **OPT-4/5/7/11**: Quick Wins Batch | ❌ **Failed & Reverted** | Attempted all four, caused test failures and subtle bugs. Reverted. See `Plans~/QUICK_WINS_OPT4_5_7_11.md` |
| **Performance Audit** | ✅ **Archived** | See `Plans~/archive/PERFORMANCE_AUDIT.md` |

---

## Verification Pass (April 24, 2026)

Every item below was traced through the actual code as if implementing it.
Items marked **VERIFIED** are feasible. Items marked **INVALIDATED** have fundamental
problems that prevent the described approach from working. Items marked
**ALREADY DONE** are already implemented in the current codebase.

---

## Status of Previous PERFORMANCE_AUDIT Findings

Before listing new items, here's what's already been fixed since the March 2026 audit:

| Audit Item | Status | Evidence |
|-----------|--------|----------|
| P0-3: FiberNode pooling | ✅ **Partially fixed** | `CloneForReuse` now reuses `current.Alternate ?? new FiberNode()` — stable trees don't allocate after first render |
| P0-5: `previousStyles` memory leak | ✅ **Fixed** | `NotifyElementRemoved()` called in `CommitDeletion` |
| P1-1: 3 commit-phase tree walks | ✅ **Fixed** | `CommitPropsAndClearFlags` merged from 3 walks into 1 |
| P1-2: `OnStateUpdated` closure per render | ✅ **Fixed** | Uses `??=` pattern — only creates delegate once |
| P2-2: `int.ToString()` per unkeyed child | ✅ **Fixed** | `s_indexStrings` pre-computed cache for indices 0-255 |
| P0-2: VNode defensive cloning | ✅ **Partially fixed** | `CloneProps`/`CloneChildren` removed from VNode constructor (trust immutability comment) |
| Signal snapshot allocation | ✅ **Fixed** | `cachedSnapshot` field added — only reallocates on listener list change |

**Still unfixed from audit:** P0-4 (hook state boxing), P1-6 (ListView reflection), P1-7 (ListView pool scan), P2-3 through P2-8, all P3 items.

**Resolved since audit:** P0-1 (DynamicInvoke) — typed fast paths already cover all common event signatures (30+ `is Action<T>` checks before DynamicInvoke fallback in `PropsApplier.InvokeHandler`). P1-5 (className HashSet) — already uses static `s_oldClassSet` / `s_newClassSet` plus single-token fast path.

---

## New Optimization Items (Post-Audit Discovery)

### OPT-1: Typed Props Pipeline — Bypass Dictionary Allocation (Core + Source Gen)

**Category:** Allocation | **Priority:** P0 | **Impact:** Critical — the single biggest optimization opportunity
**Verification:** INVALIDATED (Phase 1 caching approach) / VERIFIED (typed pipeline approach)

**Problem:**
Every host element (`<VisualElement>`, `<Button>`, `<Label>`, etc.) creates a `new BaseProps` subclass per render, which is immediately converted to a dictionary via `ToDictionary()`. For a `@foreach` over 3000 boxes, this produces **6000 dictionary allocations per frame** (3000 props dicts + 3000 Style dicts), plus the 3000 `BaseProps` instances and 3000 `Style` instances themselves.

**Hot path trace (current — 4 allocations per element per render):**
```
1. new VisualElementProps { Style = new Style { Left = x } }  // alloc: BaseProps + Style dict
2. V.VisualElement() → props.ToDictionary()                    // alloc: Dict<string,object>
3. new VirtualNode(..., properties: dict)                      // alloc: VNode (reused via fiber alternate)
4. FiberFactory.CloneForReuse() → fiber.PendingProps = dict
5. CompleteWork → AreHostPropsEqual(PendingProps, Props)        // iterates full dict
6. CommitUpdate → PropsApplier.ApplyDiff(old, new)             // iterates both dicts + both style sub-dicts
7. fiber.Props = fiber.PendingProps                            // old dict → garbage
```

**Why the original "Phase 1 caching" approach was INVALIDATED:**

The original plan proposed caching the dictionary on the `BaseProps` instance with a generation
counter. This fundamentally cannot work because:

1. **Fresh instance every render:** The source generator emits `new VisualElementProps { ... }`
   in every render call. The props instance is brand new — there's no "second call" to
   `ToDictionary()` on the same instance to benefit from caching.

2. **Different values per element:** In a `@foreach`, each box has different `Left`, `Top`,
   `BackgroundColor` values. Even if the instance were somehow reused, the cache would
   never hit because every box is different.

3. **Shared reference corruption:** If we cached the dict and returned the same reference,
   the committed `fiber.Props` would point to the same object as `fiber.PendingProps`. On
   the next render, `PropsApplier.ApplyDiff(previous, next)` would compare an object to
   itself and skip all updates — silently breaking rendering.

**Fix — Typed Props Pipeline (the approach that actually works):**

Instead of caching dictionaries, **eliminate dictionaries from the hot path entirely**.
The infrastructure is already partially in place:

- `BaseProps` already implements `IProps`
- `VirtualNode` already has `TypedProps` (used for function components)
- `FiberNode` already has `TypedProps` / `TypedPendingProps` (used for function components)

The change: **use the same typed path for host elements**, bypassing `ToDictionary()`.

**Architecture overview:**

```
CURRENT (dict-based host props):
  V.VisualElement(new VisualElementProps { Style = ... })
    → props.ToDictionary()        ← ALLOCATES DICT
    → VNode.Properties = dict
    → fiber.PendingProps = dict
    → AreHostPropsEqual(dict, dict)  ← ITERATES DICT
    → PropsApplier.ApplyDiff(dict, dict)  ← ITERATES BOTH DICTS + STYLE SUB-DICTS

PROPOSED (typed host props):
  V.VisualElement(new VisualElementProps { Style = ... })
    → VNode.HostProps = props     ← STORES TYPED OBJECT DIRECTLY (no dict)
    → fiber.PendingHostProps = props
    → props.DiffAndApply(oldProps, element)  ← COMPARES TYPED FIELDS, APPLIES ONLY CHANGES
```

**Implementation layers (can be done incrementally):**

**Layer 1 — Store typed props alongside dict, skip diff for unchanged elements (medium effort):**
- `V.VisualElement()` stores the `BaseProps` on VNode (new `HostProps` field) in addition to the dict
- `FiberNode` gets `HostProps` / `PendingHostProps` fields (type `BaseProps`)
- `FiberFactory.CloneForReuse()` copies `HostProps` and `PendingHostProps`
- `CompleteWork` first compares typed host props via a new `BaseProps.ShallowEquals(BaseProps)` method.
  If equal → skip marking as Update (no dict creation, no diff). If not equal → fall back to
  existing dict path.
- **Requires:** Adding `ShallowEquals()` to `BaseProps` (compares each typed field). Currently
  `BaseProps` has **no `Equals` override** — it uses reference equality, which always fails
  since each render creates a fresh instance.
- **Impact:** Saves dict creation + diff for elements whose props haven't changed
  (static containers wrapping animated children). For the stress test, this helps the ~6 static
  parent containers, not the 3000 boxes.
- **Note:** This is a LOW-IMPACT quick win for the stress test specifically, but a big win for
  typical apps where most elements are stable between renders.

**Layer 2 — Typed diff for changed elements, eliminate dict entirely (large effort, big win):**
- `V.VisualElement()` does NOT call `props.ToDictionary()` at all
- `VNode.Properties` becomes optional/empty for typed elements
- `fiber.PendingProps` (the dict) is no longer set for typed elements
- `BaseProps` gains a `DiffAndApply(BaseProps previous, VisualElement target)` method that:
  1. Compares each typed field against the previous props
  2. For changed fields, calls the corresponding PropsApplier setter directly
  3. For the `Style` sub-object, compares individual style fields (Left, Top, etc.)
  4. Skips unchanged fields entirely
- `FiberHostConfig.ApplyProperties` gains a typed overload: `ApplyTypedDiff(VisualElement, BaseProps old, BaseProps new)`
- `CommitUpdate` calls the typed path when `PendingHostProps != null`, falling back to dict path for
  custom elements without typed props
- **Eliminates:** 3000 `ToDictionary()` dict allocs + 3000 Style dict allocs = 6000 allocs/frame
- **Also eliminates:** Dict iteration overhead in both `AreHostPropsEqual` and `PropsApplier.ApplyDiff`
  (hash lookups, string key comparisons, boxing of Nullable values)
- **Concrete example for stress test:** Each box has `Style = { Position, Left, Top, Width, Height,
  BackgroundColor, BorderRadius }`. Only `Left` and `Top` change per frame. The typed diff would do
  7 typed field comparisons (float == float, Color == Color), apply 2 style changes, and skip 5.
  The dict path does: allocate dict with 1 entry, allocate Style dict with 7 entries, iterate
  old dict (1 entry), iterate new dict (1 entry), iterate old style dict (7 entries), iterate
  new style dict (7 entries) — each with string hashing and `object.Equals` boxing.

**Layer 3 — Source-gen-assisted `DiffAndApply` (optional, high effort):**
- Instead of hand-writing `DiffAndApply` on `BaseProps` with a giant switch/if chain,
  have the UITKX source generator emit optimized diff methods per component.
- The generator already knows exactly which properties each element uses (from the `.uitkx` markup).
- It could emit: `static void DiffBox(VisualElement el, VisualElementProps prev, VisualElementProps next) { if (prev.Style?.Left != next.Style?.Left) el.style.left = ...; }`
- This would be maximally efficient — only checking the properties that the element actually sets.

**Files involved:**
- `Shared/Core/V.cs` — all `V.*()` factory methods: add `hostProps` param to VNode constructor
- `Shared/Core/VNode.cs` — add `HostProps` field (type `BaseProps`)
- `Shared/Core/Fiber/FiberNode.cs` — add `HostProps` / `PendingHostProps` fields
- `Shared/Core/Fiber/FiberFactory.cs` — copy typed host props in `CloneForReuse`
- `Shared/Core/Fiber/FiberReconciler.cs` — `CompleteWork` and `CommitUpdate` typed paths
- `Shared/Core/Fiber/FiberHostConfig.cs` — new typed `ApplyTypedDiff` method
- `Shared/Props/Typed/BaseProps.cs` — add `ShallowEquals()` and `DiffAndApply()` methods
- `Shared/Props/Typed/Style.cs` — typed getters for diff (currently set-only)
- `Shared/Elements/IElementAdapter.cs` — new `ApplyTypedDiff` overload (Layer 2)
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — stop emitting `.ToDictionary()` (Layer 2)

**Ecosystem impact:**
- Layer 1: Internal only. No API changes. All existing code works unchanged.
- Layer 2: Adds overload to `IElementAdapter`. Custom adapters inherit default implementation
  from `BaseElementAdapter` (which falls back to `ToDictionary()` + dict path). No breaking
  changes — existing dict path remains as fallback.
- Layer 3: Source gen only. No runtime API changes.
- IDE extensions: No impact.

**Risk:**
- Layer 1: Low. Additive, with dict path as fallback.
- Layer 2: Medium. Must ensure every `PropsApplier` setter is reachable from the typed diff path.
  The typed `DiffAndApply` must handle all the same edge cases as the dict `ApplyDiff`
  (event registration/unregistration, ref assignment, className class-list manipulation, etc.).
  Missing a case would cause a silent rendering bug.
- Style typed getters: `Style` currently extends `Dictionary<string, object>` with set-only
  typed properties. Adding getters (to read back for diff) requires reading from the dict,
  which works but adds complexity.

---

### OPT-2: Style Object Allocation — Addressed by OPT-1 Typed Pipeline

**Verification:** INVALIDATED as standalone item — subsumed by OPT-1 Layer 2

**Problem:**
Every element with inline styles allocates `new Style { ... }` (which IS `Dictionary<string, object>`)
on every render. 3000 Style dicts per frame in the stress test.

**Why standalone fixes don't work:**
- **Instance caching:** Generated code emits `new Style { ... }` inline — fresh instance every render.
  No persistent instance to cache on.
- **Mutation-based reuse:** Reusing a Style instance by mutating fields conflicts with immutability
  contracts. The committed `fiber.Props["style"]` would be the same dict reference, so the next
  diff would see no changes.
- **Struct conversion:** `Style` extends `Dictionary<string, object>`. Changing to a struct would
  be a massive breaking change across the entire framework.

**Resolution:** OPT-1 Layer 2 (typed pipeline) eliminates Style dicts entirely by diffing
Style fields directly instead of iterating dictionary entries. The `Style` object is still allocated
but is never converted to a dictionary — the typed diff reads its typed properties directly
and compares against the previous Style object's typed properties.

---

### OPT-3: `__C()` Helper Allocates List + Array Per Parent Per Render (Source Gen)

**Category:** Allocation | **Priority:** P2 | **Impact:** Low for stress test, Medium for general
**Verification:** VERIFIED — valid but minimal impact for @foreach-heavy UIs

**Problem:**
The `__C()` helper method (emitted into every component) creates a `List<VirtualNode>` and then calls `.ToArray()` on every invocation:

```csharp
private static VirtualNode[] __C(params object[] items)
{
    var list = new List<VirtualNode>();  // allocation 1
    foreach (var __ci in items)
    {
        if (__ci is VirtualNode __vn) { if (__vn != null) list.Add(__vn); }
        else if (__ci is IEnumerable<VirtualNode> __seq)
            foreach (var __sn in __seq) { if (__sn != null) list.Add(__sn); }
    }
    return list.ToArray();  // allocation 2
}
```

Every element with children calls `__C(...)`. For the stress test root `<VisualElement>` that has 3000 children, `__C()` allocates a `List<VirtualNode>` that grows to 3000, then creates a `VirtualNode[3000]` array copy.

Plus the `params object[]` at the call site allocates another array.

**Files involved:**
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `EmitHelperMethod()` (line ~519)

**Fix — Common arity overloads:**
For 0, 1, 2, 3 children (which covers ~90% of real UI elements), emit overloads:
```csharp
private static VirtualNode[] __C(VirtualNode c1) => c1 != null ? new[] { c1 } : Array.Empty<VirtualNode>();
private static VirtualNode[] __C(VirtualNode c1, VirtualNode c2) => ...;
```

For the `@foreach` path (variable-length), keep the existing `params` version but use `ArrayPool` or a thread-static buffer:
```csharp
private static VirtualNode[] __C(params object[] items)
{
    // Use span/stackalloc for small counts, ArrayPool for large
}
```

**Ecosystem impact:** Source gen only. No runtime API changes. No IDE impact. Fully backward compatible — just different generated code.

**Risk:** None. Pure optimization of generated code internals.

---

### OPT-4: `MapRemainingChildren` HashMap Per Keyed Reconciliation (Core)

**Category:** Allocation | **Priority:** P1 | **Impact:** High for keyed lists
**Verification:** VERIFIED — [ThreadStatic] pooling is safe; work loop processes one fiber at a time, no reentrancy

**Problem:**
Every keyed child reconciliation (`ReconcileChildrenWithKeys`) creates `new Dictionary<string, FiberNode>()` and populates it:

```csharp
private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
{
    var map = new Dictionary<string, FiberNode>();  // allocation
    // ... walk old children, add to map ...
    return map;
}
```

For the stress test's 3000 keyed boxes, this creates a `Dictionary<string, FiberNode>` with 3000 entries every frame. The dictionary itself is ~200KB (hashing overhead for 3000 entries).

**Files involved:**
- `Shared/Core/Fiber/FiberChildReconciliation.cs` — `MapRemainingChildren()` (line ~470)

**Fix:**
Pool the dictionary. Use a static/thread-static dictionary that gets `Clear()`ed and reused:

```csharp
[ThreadStatic]
private static Dictionary<string, FiberNode> s_childMap;

private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
{
    s_childMap ??= new Dictionary<string, FiberNode>(64);
    s_childMap.Clear();
    // ... populate ...
    return s_childMap;
}
```

**Ecosystem impact:** Internal change. No public API affected. No IDE impact.

**Risk:** Low. The map is only used within `ReconcileChildrenWithKeys` scope, never stored long-term. Thread-safety is guaranteed by `[ThreadStatic]`.

---

### OPT-5: `ScheduleUpdateOnFiber` Runs Redundantly Per setState Call (Core)

**Category:** CPU | **Priority:** P1 | **Impact:** Medium-High
**Verification:** PARTIALLY VERIFIED — parent walk early-out is unsafe; WIP/metrics guard is valid

**Problem:**
When a component calls multiple `setState` in a single callback (e.g., `setBoxes(...)`, `setAvgFps(...)`, `setElapsed(...)`, `setTotalFrames(...)`), each call triggers `ScheduleUpdateOnFiber` which:

1. Resets `_workUnitCount`, `_sliceCount`, `_yieldCount`, `_effectsCommitted` — metrics lost
2. Restarts `_renderStopwatch` — timing wrong
3. Walks the entire parent chain marking `SubtreeHasUpdates = true` — redundant after first call
4. Creates/reuses WIP root — redundant after first call
5. Sets `_nextUnitOfWork` — overwritten by next call

**Why the parent walk early-out is unsafe:**
The original plan proposed `if (rootCurrent.Parent.SubtreeHasUpdates) break;` to skip the rest
of the parent walk. However, the walk serves **dual purposes**:
1. Marking `SubtreeHasUpdates` flags (can be skipped if already set)
2. **Validating the root fiber** — checking for `EffectFlags.Deletion` on ancestors and verifying
   the root matches `_root.Current` or `_root.WorkInProgress`

Breaking early would skip deletion checks and root validation, potentially allowing updates to be
scheduled on deleted or detached fibers.

**Files involved:**
- `Shared/Core/Fiber/FiberReconciler.cs` — `ScheduleUpdateOnFiber()` (line ~118)
- `Shared/Core/Hooks.cs` — `StateSetterHandle.EnqueuePendingUpdate()` calls `state.OnStateUpdated.Invoke()`

**Fix (safe subset):**
Guard metrics reset and WIP re-creation — keep the full parent walk:
```csharp
// Only reset metrics on first schedule per cycle
if (_workInProgressRoot == null)
{
    _workUnitCount = 0;
    _sliceCount = 0;
    _yieldCount = 0;
    _effectsCommitted = 0;
    _renderStopwatch.Restart();
}

// ... full parent walk (still needed for validation) ...

// Only create WIP on first schedule per cycle
if (_root.WorkInProgress == null)
{
    _workInProgressRoot = CreateWorkInProgress(rootCurrent, vnode);
    _root.WorkInProgress = _workInProgressRoot;
    _nextUnitOfWork = _workInProgressRoot;
}
else
{
    _workInProgressRoot = _root.WorkInProgress;
    // Just mark fiber's pending update, WIP already exists
}
```

**Ecosystem impact:** Internal. No public API changes.

**Risk:** Low. The optimization is conservative — worst case, it behaves as before.

---

### OPT-6: `AreHostPropsEqual` Iterates Full Dict For Unchanged Elements (Core)

**Category:** CPU | **Priority:** P2 | **Impact:** Medium

**Problem:**
In `CompleteWork`, every host fiber runs:
```csharp
else if (!AreHostPropsEqual(fiber.PendingProps, fiber.Props))
{
    fiber.EffectTag |= EffectFlags.Update;
}
```

`AreHostPropsEqual` iterates all entries in the props dictionary. For unchanged elements (e.g., static containers that wrap animated children), this is wasted work.

If OPT-1 Phase 1 is implemented (cached `ToDictionary`), `PendingProps` and `Props` would be the **same reference** for unchanged elements, and `AreHostPropsEqual` would return `true` immediately via its `if (props1 == props2) return true;` fast path.

**Files involved:**
- `Shared/Core/Fiber/FiberReconciler.cs` — `AreHostPropsEqual()` (line ~1330), `CompleteWork()` (line ~525)

**Fix:** Already has reference-equality fast path. This becomes automatically optimized by OPT-1.

**Ecosystem impact:** None.

**Risk:** None.

---

### OPT-7: Slice Delegate Deduplication Broken in BOTH Schedulers (Core + Runtime)

**Category:** CPU | **Priority:** P1 | **Impact:** High — prevents duplicate render passes
**Verification:** VERIFIED (but the fix is different from originally proposed)

**Problem:**
`ScheduleRootWork()` creates a **local function** `Slice()` that captures the `priority` parameter:
```csharp
private void ScheduleRootWork(IScheduler.Priority priority)
{
    void Slice()                         // ← closure over `priority`
    {
        ProcessWorkUntilDeadline();
        if (_nextUnitOfWork != null)
            ScheduleRootWork(priority);  // ← captured
    }
    _scheduler.Enqueue(Slice, priority);
}
```

Each call creates a **new delegate instance**. The runtime `RenderScheduler` has `HashSet<Action>`
dedup (`normalPriorityTracker.Add(action)`), but since every delegate is a different instance,
`Add()` always returns `true` — the dedup is silently broken.

The editor `EditorRenderScheduler` has no dedup at all (raw `Queue<Action>`).

Combined with OPT-5 (multiple `setState` calls each triggering `ScheduleUpdateOnFiber` →
`ScheduleRootWork`), **4 setState calls produce 4 separate Slice delegates**, all enqueued,
all executing `ProcessWorkUntilDeadline()`. In practice this may not cause 4 full renders
(the first Slice processes all work, subsequent ones find `_nextUnitOfWork == null` and
no-op), but it still:
- Enqueues 4 actions into the scheduler queue
- Calls `ProcessWorkUntilDeadline()` 4 times (with 3 no-op calls)
- Adds scheduler overhead and queue management cost

**Files involved:**
- `Shared/Core/Fiber/FiberReconciler.cs` — `ScheduleRootWork()` (line ~305)
- `Runtime/Core/RenderScheduler.cs` — `Enqueue()` with `HashSet` tracking (line ~60)
- `Editor/EditorRenderScheduler.cs` — `Enqueue()` without tracking (line ~36)

**Fix — Cache the Slice delegate (eliminates the closure):**
```csharp
// In FiberReconciler fields:
private Action _cachedSlice;
private IScheduler.Priority _scheduledPriority;
private bool _sliceScheduled;

private void ScheduleRootWork(IScheduler.Priority priority)
{
    if (_scheduler == null) { WorkLoop(); return; }

    if (_sliceScheduled) return;  // Already enqueued — skip

    _scheduledPriority = priority;
    _cachedSlice ??= CachedSlice;  // Create delegate once, reuse forever
    _sliceScheduled = true;
    _scheduler.Enqueue(_cachedSlice, priority);
}

private void CachedSlice()
{
    _sliceScheduled = false;
    ProcessWorkUntilDeadline();
    if (_nextUnitOfWork != null)
        ScheduleRootWork(_scheduledPriority);
}
```

This makes the delegate a stable reference, so the runtime scheduler's existing HashSet dedup
works correctly. It also adds a `_sliceScheduled` flag as a second line of defense.

**Ecosystem impact:** Internal to `FiberReconciler`. No API changes. Both schedulers benefit.

**Risk:** Very low. The `_sliceScheduled` flag prevents double-enqueue. The cached delegate
uses instance fields instead of captured locals.

---

### ~~OPT-8: `DynamicInvoke` in Event Dispatch~~ — ALREADY DONE

**Verification:** ALREADY DONE

`PropsApplier.InvokeHandler()` already has 30+ typed fast paths (`if (del is Action<ClickEvent>)`,
`if (del is Action<PointerDownEvent>)`, `if (del is Action<string>)`, `if (del is Action<ReactiveEvent>)`,
etc.) before the `DynamicInvoke` fallback (see `PropsApplier.cs` lines ~2140-2230). All common
event handler signatures are covered. `DynamicInvoke` only fires for truly exotic delegate types
that don't match any of the 30+ checked signatures.

**No further action needed.** If profiling shows specific delegate types hitting the fallback,
individual fast paths can be added.

---

### OPT-9: Hook State Boxing — `List<object>` (Core — from P0-4)

**Category:** Allocation + CPU | **Priority:** P2 | **Impact:** High for many-component apps, LOW for stress test
**Verification:** VERIFIED but stress-test impact is minimal

**Problem (unchanged from audit):**
All hook state is stored in `List<object>`. Every `useState<int>()`, `useState<float>()`, `useState<bool>()` causes boxing on write and unboxing on read.

**Stress test reality check:** The stress test has **one component** (`StressTest`) with a few hooks.
The 3000 boxes are data in a `List<BoxData>`, NOT individual components. So hook boxing causes
~10-20 box/unbox operations per frame — negligible compared to the ~12,000 dictionary allocations.

For real-world apps (200 components × 5 value-type hooks × 60fps), this causes 60,000
box/unbox operations per second, which IS significant.

**Files involved:**
- `Shared/Core/NodeMetadata.cs` — `FunctionComponentState.HookStates` (line ~111)
- `Shared/Core/Hooks.cs` — every `Use*` hook reads/writes `HookStates[index]`

**Fix — Phase 1 (targeted, medium effort):**
Replace `List<object>` with `List<HookSlot>` where `HookSlot` is a discriminated union:

```csharp
internal struct HookSlot
{
    internal enum Kind : byte { Null, Int, Float, Bool, Object }
    internal Kind SlotKind;
    internal int IntValue;
    internal float FloatValue;
    internal bool BoolValue;
    internal object ObjectValue;

    internal void Set<T>(T value)
    {
        if (typeof(T) == typeof(int)) { IntValue = (int)(object)value; SlotKind = Kind.Int; }
        else if (typeof(T) == typeof(float)) { FloatValue = (float)(object)value; SlotKind = Kind.Float; }
        else if (typeof(T) == typeof(bool)) { BoolValue = (bool)(object)value; SlotKind = Kind.Bool; }
        else { ObjectValue = value; SlotKind = Kind.Object; }
    }
}
```

The JIT will optimize the `typeof(T) == typeof(int)` checks to constant-true/false, making this zero-cost.

**Fix — Phase 2 (source gen, high effort):**
Have the UITKX source generator emit a typed hook-state struct per component:
```csharp
// Generated for a component with useState<int>, useState<string>, useState<bool>:
internal struct StressTest_HookState
{
    public int State0;
    public string State1;
    public bool State2;
}
```

This eliminates boxing entirely and makes hook state cache-line-friendly.

**Ecosystem impact:**
- Phase 1: Internal to `FunctionComponentState`. No public API change. All hooks still work via `UseState<T>`.
- Phase 2: Requires source gen changes and a new `FunctionComponentState` variant. Much larger scope.
- IDE extensions: No impact.
- `List<object>` → `List<HookSlot>` changes the internal type but the hook API is unchanged.

**Risk:**
- Phase 1: Medium. Need to handle ALL hook types (memo, callback, ref, reducer, etc.) — not just useState. Each stores different shaped data in the slot.
- Phase 2: High complexity. Tight coupling between source gen and runtime.

---

### OPT-10: IIFE Closure Elimination (Source Gen + HMR)

**Category:** Allocation + CPU | **Priority:** P0 | **Impact:** High for list UIs — highest-impact quick win
**Verification:** VERIFIED

**Status: ✅ DONE (April 25, 2026)**
Implemented `RewriteReturnsForInline` helper that rewrites `return EXPR;` → `__r.Add(EXPR); continue;`
and `return null;` → `continue;`, allowing loop body code to be inlined directly without inner IIFE.
Character-by-character scanner with lambda depth tracking ensures nested lambdas are preserved.
Applied to `@foreach`, `@for`, and `@while` in both source gen and HMR emitters.
Also fixed a latent bug where `break`/`continue` in `@for`/`@while` bodies failed (were inside lambda).
See `Plans~/FOREACH_IIFE_ELIMINATION.md` for full design.

**Problem:**
The `EmitForeachNode` method wraps every `@foreach` in an IIFE (Immediately Invoked Function Expression) pattern:

```csharp
((System.Func<VirtualNode[]>)(() => {
    var __r = new List<VirtualNode>();           // allocation 1
    foreach (var box in boxes) {
        __r.Add(((System.Func<VirtualNode>)(() => {  // allocation 2: delegate per iteration
            return V.VisualElement(...);
        }))());
    }
    return __r.ToArray();                       // allocation 3
}))()
```

For 3000 boxes: 1 List, 3000 delegate allocations (IIFE per iteration), 1 Array copy = ~3002 allocations just for the loop scaffolding.

**Files involved:**
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `EmitForeachNode()` (line ~1230)

**Fix:**
For the common case of `@foreach` with a single child element (no body code), emit a direct `.Select().ToArray()` or a pre-sized array:

```csharp
// Instead of IIFE wrapping:
boxes.Select(box => V.VisualElement(...)).ToArray()

// Or even better — pre-sized array:
((System.Func<VirtualNode[]>)(() => {
    var __src = boxes;
    var __r = new VirtualNode[__src.Count];
    int __i = 0;
    foreach (var box in __src)
        __r[__i++] = V.VisualElement(...);
    return __r;
}))()
```

The IIFE per iteration is only needed when the `@foreach` body contains multiple statements or complex control flow. For single-expression bodies (the majority), it can be eliminated.

**Ecosystem impact:** Source gen only. No runtime changes. No IDE impact.

**Risk:** Low. Need to correctly detect single-expression vs multi-statement bodies in the emitter. Fallback to current IIFE for complex cases.

---

### OPT-11: `CommitDeletions` Full-Tree Recursive Walk (Core)

**Category:** CPU | **Priority:** P2 | **Impact:** Medium
**Verification:** VERIFIED

**Problem:**
`CommitDeletions` walks the **entire** fiber tree recursively looking for fibers with `Deletions` lists:

```csharp
private void CommitDeletions(FiberNode fiber)
{
    if (fiber == null) return;
    if (fiber.Deletions != null) { /* process */ }
    var child = fiber.Child;
    while (child != null) { CommitDeletions(child); child = child.Sibling; }
}
```

In steady state (no elements added/removed), this visits all 3000+ fibers and finds zero deletions. This is separate from the `CommitPropsAndClearFlags` walk (which was already merged from 3 walks to 1).

**Files involved:**
- `Shared/Core/Fiber/FiberReconciler.cs` — `CommitDeletions()` (line ~780), `CommitRoot()` (line ~660)

**Fix:**
Track whether any deletions were scheduled during reconciliation. Only run `CommitDeletions` if the flag is set.

**Implementation note:** `DeleteChild()` is `private static` in `FiberChildReconciliation` (also called
from `FiberFunctionComponent`), so it doesn't have access to the `FiberReconciler` instance. Options:
1. Use a `[ThreadStatic]` flag set by `DeleteChild`, checked by `CommitRoot`
2. Maintain a `List<FiberNode> _fibersWithDeletions` — `CommitDeletions` iterates the list instead of walking the tree
3. **Best option:** Merge deletion processing into the existing `CommitPropsAndClearFlags` walk, since
   that already visits every fiber. Check `fiber.Deletions != null` during the walk and process inline.

```csharp
// Option 3 — in CommitPropsAndClearFlags:
private void CommitPropsAndClearFlags(FiberNode fiber)
{
    // ... existing props/flags work ...

    // Process deletions inline (no separate tree walk needed)
    if (fiber.Deletions != null)
    {
        foreach (var deletion in fiber.Deletions)
            CommitDeletion(deletion);
        fiber.Deletions = null;
    }

    // ... recurse to children ...
}
```

**Ordering concern:** Currently `CommitDeletions` runs BEFORE the effect list walk, while
`CommitPropsAndClearFlags` runs AFTER. If merged, deletions would process after effects.
Need to verify this ordering is safe — deletions remove elements from the DOM, which must
happen before new placements at the same position. Since the effect list walk handles placements,
deletions must run first. This means **Option 3 requires reordering CommitRoot**, or we use
Option 1/2 instead.

**Ecosystem impact:** Internal. No API changes.

**Risk:** Very low. Worst case: deletions processed one frame late if flag is missed (would cause visual artifact). Mitigation: always set flag conservatively.

---

### ~~OPT-12: `className` Diffing Creates HashSets Per Change~~ — ALREADY DONE

**Verification:** ALREADY DONE

The code already has all the optimizations described:
- `s_oldClassSet` and `s_newClassSet` — static reusable `HashSet<string>` (no per-diff allocation)
- Single-token fast path: `IndexOfAny(s_classNameSeparators) < 0` — compares and swaps single class
  names directly without touching HashSets
- Multi-class path: populates static sets, diffs only added/removed classes

See `PropsApplier.cs` lines ~1237-1295.

**No further action needed.**

---

### OPT-13: `Canonicalize` String Allocation (Core — from P2-7)

**Category:** Allocation | **Priority:** P3 | **Impact:** Low

**Problem:**
`Canonicalize(styleKey)` is called per style property application. A `s_canonicalizeCache` dictionary already exists and caches results, so this is only an issue for the first render of each style key.

**Status:** Already mostly optimized via `s_canonicalizeCache`. No further action needed unless profiling shows cache misses.

---

### OPT-14: `VirtualNode` Constructor — `ClonePropTypes` (Core)

**Category:** Allocation | **Priority:** P3 | **Impact:** Low

**Problem:**
`VirtualNode` constructor calls `ClonePropTypes(propTypes)` which creates `new PropTypeDefinition[]` + `Array.AsReadOnly()`. PropTypes are used for runtime prop validation. Most elements have null propTypes (returns `EmptyPropTypesInstance`), so this only fires for elements with `@propTypes`.

**Files involved:**
- `Shared/Core/VNode.cs` — `ClonePropTypes()` (line ~155)

**Fix:** Low priority. Could use `ArrayPool` but the impact is minimal since most elements skip this path.

**Ecosystem impact:** None.

**Risk:** None.

---

### OPT-15: `Deletions` List Allocation Per Parent With Deletions (Core)

**Category:** Allocation | **Priority:** P3 | **Impact:** Low

**Problem:**
`DeleteChild` allocates `new List<FiberNode>()` on each parent fiber that has deletions:
```csharp
if (parentFiber.Deletions == null)
    parentFiber.Deletions = new List<FiberNode>();
```

In steady-state rendering (no structural changes), this never fires. Only allocates during mount/unmount transitions.

**Fix:** Could pool or pre-allocate, but the impact is negligible since it only happens during structural changes.

**Ecosystem impact:** None.

**Risk:** None.

---

## Implementation Priority Order

Ordered by **verified impact ÷ effort ÷ risk**. Items invalidated or already done are excluded.

| # | Item | Priority | Effort | Risk | Key Benefit |
|---|------|----------|--------|------|-------------|
| 1 | **OPT-23**: Eliminate double style diff | P1 | Small | Low | Style compared in ShallowEquals then again in DiffStyle — skip redundant pass. ~0.3-0.5ms |
| 2 | **OPT-25**: Dict pool/alternative (MapRemainingChildren + pendingPassiveEffects) | P1 | Small | Low | Pool the 3000-entry dict + List alloc per commit. ~0.2-0.5ms |
| 3 | **OPT-26**: Merge post-commit walks | P2 | Small | Low | CommitDeletions + CommitPropsAndClearFlags are 2 separate full tree walks — merge into 1. ~0.2-0.3ms |
| 4 | **OPT-24**: Host-only CloneForReuse fast path | P2 | Small | Low | Skip ~12-15 irrelevant field copies per host fiber (ErrorBoundary, Context, TypedProps, etc). ~0.1-0.3ms |
| 5 | **OPT-3**: `__C()` arity overloads | P2 | Small | None | Eliminate List+ToArray for 1-3 static children |
| 6 | **OPT-9 Phase 1**: `HookSlot` union struct | P2 | Medium | Medium | Eliminate boxing for int/float/bool state |
| ~~7~~ | ~~**OPT-7**: Cache Slice delegate~~ | ~~P1~~ | ~~Small~~ | ~~None~~ | ~~Failed & reverted~~ |
| ~~8~~ | ~~**OPT-4**: Pool `MapRemainingChildren` dict~~ | ~~P1~~ | ~~Small~~ | ~~None~~ | ~~Failed & reverted~~ |
| ~~9~~ | ~~**OPT-11**: Skip/merge `CommitDeletions` walk~~ | ~~P2~~ | ~~Small~~ | ~~Low~~ | ~~Failed & reverted~~ |
| ~~10~~ | ~~**OPT-5**: `ScheduleUpdateOnFiber` WIP/metrics guard~~ | ~~P1~~ | ~~Small~~ | ~~None~~ | ~~Failed & reverted~~ |

### Items Removed (invalidated, already done, or completed)

| Item | Reason |
|------|--------|
| OPT-1 Phase 1 (cached ToDictionary) | **Invalidated:** new BaseProps instance per render makes instance-level caching useless |
| OPT-1 Layer 1 (typed equality) | **Done:** Subsumed by Layer 2 — typed host props pipeline |
| OPT-1 Layer 2 (typed pipeline) | **Done:** Typed Props Pipeline fully implemented |
| OPT-2 (Style reuse) | **Invalidated:** new Style per render, same problem as OPT-1. Subsumed by OPT-1 Layer 2 |
| OPT-6 (AreHostPropsEqual ref fast-path) | **Done:** Free with typed props pipeline |
| OPT-8 (DynamicInvoke fast paths) | **Already done:** 30+ typed fast paths exist in PropsApplier.InvokeHandler |
| OPT-10 (IIFE closure elimination) | **Done:** Inner per-iteration `Func<VNode>` eliminated. `return` → `__r.Add(); continue;` rewriting. Also fixed `break`/`continue` bug. |
| OPT-12 (className HashSet alloc) | **Already done:** static HashSets + single-class fast path already implemented |
| OPT-16 (Style & BaseProps pooling) | **Done:** 99% hit rate steady state. Reduced GC freeze frequency. See analysis above. |
| OPT-18 (VirtualNode pooling) | **Done:** Generation-stamped pool. Eliminated ~3,200 VNode allocs/frame. |
| OPT-22 (Event handler skip in ApplyDiff) | **Done:** `_hasEvents` bool skips 43 `DiffEvent()` calls. +2 FPS. |
| OPT-23B (_hasEvents guard in ShallowEquals) | **Done:** Same `_hasEvents` guard applied to `ShallowEquals` event comparisons. |
| OPT-24 (Host-only CloneForReuse) | **Done:** Skip ~15 irrelevant field copies for HostComponent fibers. |
| OPT-25 (Pool MapRemainingChildren dict) | **Done:** `[ThreadStatic]` dict with `Clear()` reuse. Supersedes failed OPT-4 attempt. |
| OPT-26 (Skip CommitDeletions walk) | **Done:** `_hasDeletions` boolean flag skips O(N) tree walk in steady state. |
| OPT-23A (Remove StyleEquals from ShallowEquals) | **Dropped:** Would cause pool leaks for unchanged elements with constant inline styles. |
| OPT-3 (__C() arity overloads) | **Deferred:** ~0.01ms gain in stress test; requires 3 emitter changes. Not worth the coordination cost. |
| OPT-4 (Pool MapRemainingChildren dict) | **Failed & Reverted:** First attempt caused test failures. Superseded by OPT-25. |
| OPT-5 (ScheduleUpdateOnFiber guard) | **Failed & Reverted:** Caused subtle bugs. |
| OPT-7 (Cache Slice delegate) | **Failed & Reverted:** Caused test failures. |
| OPT-11 (Skip CommitDeletions walk) | **Failed & Reverted:** Caused subtle bugs. |
| OPT-20 (Reconciliation bail-out) | **Removed:** All elements are rendered always — bail-out not applicable to this workload. |
| OPT-21 (Virtualized @foreach) | **Removed:** All elements are rendered always — virtualization not applicable. |

---

## Ecosystem Impact Summary

| Change Area | Source Gen | Core Runtime | Scheduler | IDE Extensions | User Code |
|-------------|-----------|-------------|-----------|----------------|-----------|
| ~~OPT-10~~ | ~~Emit changes~~ | ~~No change~~ | ~~No change~~ | ~~No change~~ | ~~No change~~ | ✅ Done |
| OPT-23 | No change | `TypedPropsApplier` / `FiberReconciler` | No change | No change | No change |
| OPT-24 | No change | `FiberFactory` | No change | No change | No change |
| OPT-25 | No change | `FiberChildReconciliation` / `FiberReconciler` | No change | No change | No change |
| OPT-26 | No change | `FiberReconciler` | No change | No change | No change |
| OPT-3 | `__C()` overloads | No change | No change | No change | No change |
| OPT-9 Phase 1 | No change | `FunctionComponentState` internal | No change | No change | No change |

**Key takeaway:** Items 1-6 are purely internal, zero-risk changes. OPT-1 Layer 1 is also internal.
Only OPT-1 Layer 2 touches public surface (`IElementAdapter` gains an overload with a default
fallback implementation). No breaking changes in any item.

---

## Expected Gains (Revised Conservative Estimates — Post OPT-16 Pooling)

> **Updated April 25, 2026:** OPT-16 (Style & BaseProps pooling) is now done. Pool hit rate
> at 99% steady state. However, **no measurable FPS gain** from pooling alone because
> the remaining ~6,000+ allocations/frame (VirtualNode + IIFE closures + __C + MapChildren dict)
> still drive the same GC pressure pattern. Small freezes are less noticeable, but the
> big 9-11 second spike persists. See analysis below.

### Why OPT-16 Produced No FPS Improvement

The pool works correctly (99% hit rate, verified with diagnostics). But the allocations
it eliminated (~6,000 Style + BaseProps) were only **half** of the total per-frame budget:

| Allocation Source | Count/Frame | Size/Frame | Status |
|-------------------|-------------|------------|--------|
| Style objects | ~3,000 | ~1.70 MB | ✅ **Pooled** (OPT-16) |
| BaseProps subclasses | ~3,000 | ~0.60 MB | ✅ **Pooled** (OPT-16) |
| VirtualNode objects | ~3,000+ | ~500+ KB | ✅ **Pooled** (OPT-18) |
| IIFE delegates (@foreach) | ~3,000 | ~72+ KB | ✅ **Eliminated** (OPT-10) |
| **@foreach scaffolding** (List + ToArray) | **~2-4** | **~100+ KB** | ❌ Not optimized |
| **`__C()` arrays** (params + List + ToArray) | **~6-300** | **~10-50 KB** | ❌ Not optimized |
| **MapRemainingChildren dict** | **1** | **~200 KB** | ❌ Not pooled |
| **Remaining total** | **~10-305** | **~310+ KB** | Near-zero steady-state allocs |

**Result:** Total allocations went from ~12,000/frame to ~6,000/frame. GC pressure halved.
This explains the **reduced frequency of small freezes**. But ~6,000 allocs/frame at ~900 KB
still triggers periodic major GC collections (the **9-11 second big spike**).

**CPU impact:** Pool rent/return overhead (stack pop + field reset + generation stamp + pending
list management + flush) roughly offsets the saved GC overhead for per-frame CPU. Net FPS: ±0.

**The big 9-11 second spike:** Unity's incremental Boehm GC accumulates ~900 KB/frame × 35 fps
= ~31 MB/second of managed garbage. The GC spreads marking across frames, but periodically
performs a more thorough sweep. At 31 MB/s, a major sweep cycle triggers every ~10 seconds,
causing a 10-50ms pause (the user-observed spike).

### Path to Real FPS Gains

The remaining ~6,000 allocs/frame must be attacked. The highest-impact items in order:

1. **OPT-10 + OPT-18** (eliminate ~6,000 allocs): IIFE closures + VirtualNode pooling
2. **OPT-4** (eliminate ~200 KB): MapRemainingChildren dict pool
3. **OPT-3** (eliminate ~300 allocs): __C overloads

With OPT-10 + OPT-18, per-frame allocations are now near-zero in steady state,
which eliminated both the small GC freezes AND the big 9-11 second spikes.

For the stress test (3000 boxes, every frame update):

| Optimization | Allocs Eliminated Per Frame | Estimated FPS Gain |
|-------------|---------------------------|-------------------|
| ~~OPT-10 (@foreach IIFE elimination)~~ | ~~~3000 delegate closures~~ | ✅ Done — smoothing only |
| ~~OPT-18 (VirtualNode pooling)~~ | ~~~3000+ VNode objects~~ | ✅ Done — smoothing only |
| ~~OPT-22 (Event handler skip)~~ | ~~0 (CPU: skip 43 DiffEvent calls)~~ | ✅ Done — +2 FPS (35→37) |
| **OPT-23 (Eliminate double style diff)** | **0 (CPU: skip redundant style comparison)** | **~0.3-0.5ms** |
| **OPT-25 (Dict pool/alternative)** | **1 large dict + 1 List** | **~0.2-0.5ms** |
| **OPT-26 (Merge post-commit walks)** | **0 (CPU: halve tree traversal)** | **~0.2-0.3ms** |
| **OPT-24 (Host-only CloneForReuse)** | **0 (CPU: skip ~45K field copies)** | **~0.1-0.3ms** |
| OPT-3 (__C overloads) | List + Array for ~6 static parents | +0.1-0.5 FPS |
| ~~OPT-7 (Slice delegate caching)~~ | ~~0~~ | ❌ Failed & reverted |
| ~~OPT-4 (pooled child map)~~ | ~~1 large dict~~ | ❌ Failed & reverted |
| ~~OPT-11 (skip CommitDeletions walk)~~ | ~~0~~ | ❌ Failed & reverted |
| ~~OPT-5 (WIP/metrics guard)~~ | ~~0~~ | ❌ Failed & reverted |
| **Subtotal (OPT-23/24/25/26)** | **~1 dict/frame** | **~1-2ms (~1-3 FPS)** |

> **Key insight (post OPT-10+18+22):** Allocation elimination produced **smoothing** (no GC
> stutter) but NOT FPS gains. The remaining ~10 FPS gap to pure UITK is pure CPU time spent
> reconciling and diffing 3,000 elements per frame. OPT-23/24/25/26 target per-element CPU
> overhead reduction.

---

## Post-Typed-Pipeline Items (April 25, 2026)

> **Context:** OPT-1 Layer 2 (Typed Props Pipeline) and Typed Style Pipeline are both implemented.
> Result: 35 FPS (up from 28.3). However, GC stutter pattern persists:
> - Small freezes every ~1s (Gen0 incremental slices — nursery fills from ~6,000 remaining allocs/frame)
> - Large freeze every ~9-12s (full Gen1/Gen2 stop-the-world collection)
>
> Root cause: 3,000 Style objects + 3,000 BaseProps objects per frame still allocated fresh.
> Smoothing: Incremental GC time slice is 3ms (default) — noticeable at 28ms frame budget.

### OPT-16: Style / BaseProps Object Pooling (Root Cause)

**Category:** Allocation | **Priority:** P0 | **Impact:** Critical — eliminates remaining ~6,000 allocs/frame
**Prerequisite:** OPT-1 Layer 2 + Typed Style Pipeline (both done)

**Problem:**
Every render creates `new Style { ... }` and `new VisualElementProps { ... }` per element.
With 3,000 boxes: 3,000 Style (~566 bytes each) + 3,000 BaseProps (~200 bytes each) = ~2.3 MB
of short-lived objects per frame. This fills the Gen0 nursery (~4 MB default) every ~2 frames,
triggering frequent GC collections.

**Approach:**
Pool Style and BaseProps objects between frames. After `CommitPropsAndClearFlags` swaps
`fiber.HostProps = fiber.PendingHostProps`, the old `HostProps` (and its `.Style`) become garbage.
Instead of discarding them, return them to a pool for reuse next frame.

**Implementation:**
1. Add `StylePool` — a simple `Stack<Style>` with a `Rent()` / `Return()` API
2. Add `Clear()` method to Style that resets all bitmasks and fields to default
3. After commit phase, return old `fiber.HostProps.Style` to pool
4. Source generator emits `StylePool.Rent()` instead of `new Style()`
5. Same pattern for BaseProps subclasses (per-type pools)

**Key constraint:** Pooled objects must be fully cleared before reuse — stale bits would cause
incorrect rendering. `Style.Clear()` resets `_setBits0 = _setBits1 = 0` plus all ref-type fields
to null (value-type fields can stay dirty since bitmask guards them).

**Files involved:**
- `Shared/Props/Typed/Style.cs` — add `Clear()`, pool integration
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — emit `StylePool.Rent()` instead of `new Style()`
- `Shared/Core/Fiber/FiberReconciler.cs` — return old props to pool in commit phase

**Risk:** Medium. Stale field bugs if `Clear()` misses a reference field. Mitigate with tests.

**Expected gain:** Eliminates ~6,000 allocs/frame → ~0 per-frame allocs (steady state).
GC collections drop from every ~2 frames to near-zero in steady state.

**Status: ✅ DONE (April 25, 2026)**
Implemented with version-stamped generation approach. Pool at 99% hit rate steady state.
Capacity: 4096 per type. Pending return list flushed once per frame in CommitRoot.
Reduced small GC freezes but did NOT produce FPS gain — remaining ~6,000 allocs/frame
(VirtualNode + IIFE closures) still drive GC pressure. See revised estimates above.

---

### OPT-18: VirtualNode Object Pooling (Source Gen + Core)

**Category:** Allocation | **Priority:** P0 | **Impact:** Critical — eliminates ~3,000+ allocs/frame
**Prerequisite:** OPT-16 (done)

**Status: ✅ DONE (April 25, 2026)**
Implemented with Option D (pool with read-only facade). Generation-stamped pool using
same pattern as OPT-16. Auto-properties converted to backing fields + `__Reset()`.
All ~50 `V.*()` factories use `VirtualNode.__Rent()`. Old VNodes returned to pool in
`CommitRoot` via `__FlushReturns()`. Eliminated ~3,200 VNode allocs/frame (~538 KB/frame).
Combined with OPT-10, brought per-frame allocations from ~6,000 to near-zero.

**Problem:**
Every `V.*()` factory call creates `new VirtualNode(...)`. VirtualNode is a `sealed class` with
18 properties (6 value-type, 12 reference-type). At 64-bit Mono:
- Object header: 16 bytes
- 12 reference fields × 8 bytes = 96 bytes
- 6 value/enum fields: ~24 bytes
- Alignment: ~168 bytes total per instance

With 3,000 boxes in the stress test, each render frame produces ~3,000 VirtualNode allocations
(+ additional VNodes for containers, fragments, function components → ~3,200+ total).

At ~168 bytes × 3,200 = **~538 KB/frame** of VirtualNode garbage. This is now the **single
largest remaining allocation source** after OPT-16 pooling.

**Why it's harder than Style/BaseProps pooling:**
1. VirtualNode has **readonly (get-only) properties** — cannot be mutated after construction
2. VirtualNode instances are consumed by the reconciler and stored on FiberNode.VNode
3. The fiber's alternate tree keeps a reference to the old VNode for one extra frame
4. VirtualNode is used across ALL node types (Element, Text, Fragment, FunctionComponent, etc.)
5. The `V.*()` factories are public API — changing signatures would break user code

**Approach options:**

**Option A — Mutable VirtualNode with pool (mirrors OPT-16 pattern):**
- Convert readonly properties to internal-set or add Reset() method
- Add `VirtualNode.__Rent()` pool with generation stamps (same pattern as BaseProps)
- Source gen emits `VirtualNode.__Rent()` + field assignment instead of `new VirtualNode(...)`
- Return old VNodes to pool in commit phase (after WIP→Current swap)
- **Risk:** High. VirtualNode is referenced everywhere. Mutable VNodes could introduce
  subtle bugs if a reference is held past its pool lifecycle.

**Option B — Struct VirtualNode + arena allocator (zero-GC):**
- Convert VirtualNode from class to struct, stored in a frame-local array/arena
- Pass around indices instead of references
- **Risk:** Very high. Massive refactor — every consumer (FiberNode, reconciler, user code)
  would need to change from `VirtualNode` to `VNodeRef` or index.

**Option C — VirtualNode flyweight with reusable backing store:**
- VirtualNode becomes a thin wrapper around an index into a frame-local array
- The array is reused between frames (cleared + refilled)
- Less invasive than Option B because VirtualNode stays a class but its fields are stored externally
- **Risk:** Medium-High. Indirection overhead, cache-unfriendly.

**Option D — Pool with read-only facade (safest):**
- VirtualNode stays readonly externally
- Internally, add `internal void __Reset(...)` that re-initializes all fields
- Pool uses the same generation-stamp pattern as OPT-16
- `V.*()` factories call `VNodePool.Rent()` + `__Reset(...)` instead of `new VirtualNode(...)`
- Old VNodes returned to pool after commit
- **Risk:** Medium. The `__Reset()` writes to auto-properties (requires backing field access
  or switching to `{ get; internal set; }` or manual backing fields)

**Recommended:** Option D — safest path, proven pattern from OPT-16, no public API changes.

**Files involved:**
- `Shared/Core/VNode.cs` — add backing fields, `__Reset()`, pool infrastructure
- `Shared/Core/V.cs` — all factory methods: use pool instead of `new`
- `Shared/Core/Fiber/FiberReconciler.cs` — return old VNodes to pool in commit phase
- `Shared/Core/VNodeHostRenderer.cs` — ensure WrapHostChildren doesn't leak VNode references

**Key constraints:**
- VNode must be fully reset before reuse (all 18 fields)
- The alternate tree holds old VNodes for one frame — pool return must happen in CommitRoot
  after the WIP→Current swap, not before
- `VNode.WithPropTypesImmutable()` creates a copy — would need pool integration too
- `ClonePropTypes()` allocates arrays — could be pooled or eliminated if propTypes are rare

**Expected gain:** Eliminates ~3,200 allocs/frame × ~168 bytes = ~538 KB/frame.
Combined with OPT-10 (IIFE elimination), would bring per-frame allocs to near-zero.

---

### OPT-19: Incremental GC Time Slice Tuning (Bump Smoothing)

**Category:** GC | **Priority:** P1 | **Impact:** Reduces perceived stutter severity
**Independent of:** All other items — can be done anytime

**Problem:**
Unity's incremental GC defaults to ~3ms time slices. At 35 FPS (28ms frame budget), a 3ms
GC slice consumes ~11% of the frame — noticeable as a micro-stutter. Reducing the slice
spreads GC work across more frames with less per-frame impact.

**Implementation:**
Add a configurable `GCTimeSliceMs` property to `RenderScheduler`:

```csharp
// RenderScheduler.cs
[SerializeField]
private float gcTimeSliceMs = 1.0f;

private void Awake()
{
    // ... existing code ...
    if (gcTimeSliceMs > 0f)
    {
        UnityEngine.Scripting.GarbageCollector.incrementalTimeSliceNanoseconds =
            (ulong)(gcTimeSliceMs * 1_000_000);
    }
}
```

**Values to try:**
- `1.0ms` — good balance for 30+ FPS targets (3.5% of 28ms frame)
- `0.5ms` — aggressive, may delay collections (risk: bigger Gen2 pauses)
- `2.0ms` — conservative, still better than 3ms default

**Files involved:**
- `Runtime/Core/RenderScheduler.cs` — add serialized field + Awake integration

**Risk:** None. Purely a Unity API configuration. Reversible by setting back to default.

**Expected gain:** Reduces small freeze severity from ~3ms to ~1ms. Does NOT fix the
large 9-12s freezes (those are full Gen2 collections — only allocation reduction fixes those).
OPT-16 (pooling) is the real fix; this just smooths whatever remains.

---

### OPT-20: Reconciliation Bail-Out — Skip Unchanged Subtrees (Core)

**Category:** CPU | **Priority:** P0 | **Impact:** Critical — **the remaining FPS bottleneck**
**Prerequisites:** None (independent of allocation optimizations)

**Problem:**
After OPT-16 + OPT-18 + OPT-10, per-frame allocations are near-zero and GC stutter is
eliminated. FPS is stable at ~35 (up from 28.3). But the gap to pure UITK (47.7 FPS) is
still ~12 FPS. The remaining bottleneck is **CPU time spent reconciling unchanged elements**.

In the stress test, every frame:
1. The root component re-renders (produces new VNode tree with 3,000 children)
2. `ReconcileChildrenWithKeys` walks all 3,000 existing fibers and matches against new VNodes
3. `CompleteWork` runs on every fiber: compares props, checks if styles changed
4. `CommitPropsAndClearFlags` walks the tree applying diffs

For boxes whose `Left`/`Top` changed: all work is necessary.
For boxes whose position is unchanged this frame: the entire reconcile→diff→commit pipeline
runs and discovers "nothing changed" — pure waste.

**The React pattern — `shouldComponentUpdate` / `React.memo`:**
React solves this with memoization: if a component's props haven't changed, skip its entire
subtree. UITKX has the infrastructure for this in `FiberFunctionComponent` (the `bailOut` path),
but it only applies to function components, not host elements.

**Approach options:**

**Option A — Host element bail-out (medium effort, high impact for stress test):**
When `CompleteWork` sees a host fiber whose props are reference-equal to the previous frame's
props (same `BaseProps` instance), skip marking it as `Update`. This already partially works
via the typed equality check — but the check still *runs* on every fiber. A cheaper guard:

```csharp
// In CompleteWork for host elements:
if (ReferenceEquals(fiber.PendingHostProps, fiber.HostProps))
{
    // Props instance is identical — skip entirely
    return;
}
```

This requires the source gen to emit the same `BaseProps` instance when props haven't changed.
Currently impossible — `new VisualElementProps { ... }` is always a fresh instance.

**Option B — Keyed child dirty tracking (medium effort, high impact):**
Instead of reconciling ALL 3,000 children every frame, track which keys have changed data.
The `useStressTestLoop` hook knows exactly which boxes moved — it could provide a dirty set.
But this couples the optimization to the specific hook, not a general solution.

**Option C — VNode children equality fast-path (medium effort, general):**
In `ReconcileChildrenWithKeys`, before building the full `MapRemainingChildren` dict,
compare the new VNode children array against the old fiber children by key+props.
If all children are identical (same key order, same props references), skip reconciliation
entirely for the parent.

**Option D — Virtualized list / windowed rendering (large effort, highest FPS gain):**
Only render the visible subset of 3,000 boxes. For a viewport that shows ~100 boxes,
reconcile only those ~100 instead of all 3,000. This is how `ListView` works in UITK.

Implementation: a `<VirtualList>` component or `@foreach` with automatic windowing.
Requires knowing the scroll position and item sizes. Most complex but most impactful —
would bring FPS from 35 to 47+ for large lists.

**Option E — Fiber-level memoization with shallow props comparison (medium effort, general):**
Add a `memo()` wrapper (like React.memo) that wraps function components and skips re-render
if props are shallowly equal. For host elements, the typed `DiffAndApply` already does this
partially — but the VNode creation + reconciler walk still happens. True bail-out must happen
before VNode creation (at the component render level, not the element level).

**Files involved:**
- `Shared/Core/Fiber/FiberReconciler.cs` — `CompleteWork`, `ReconcileChildren`
- `Shared/Core/Fiber/FiberChildReconciliation.cs` — `ReconcileChildrenWithKeys`
- `Shared/Core/Fiber/FiberFunctionComponent.cs` — bail-out path for memo'd components
- Possibly `SourceGenerator~/Emitter/CSharpEmitter.cs` — for virtualized list codegen

**Recommended approach:** Start with Option A + C (fast-path guards in reconciler), then
Option E (memo wrapper), then Option D (virtualized list) for maximum FPS in list-heavy UIs.

**Expected gain:**
- Option A+C: +2-5 FPS (skip reconciling unchanged elements)
- Option E: +3-5 FPS (skip entire unchanged component subtrees)
- Option D: +10-15 FPS for list-heavy UIs (only reconcile visible items)
- Combined: could close the gap to pure UITK (~47 FPS)

**Risk:** Medium. Bail-out correctness is critical — skipping a dirty subtree would cause
stale rendering. Must ensure dirty tracking is conservative (never miss a change).

---

### OPT-21: Virtualized `@foreach` / `<VirtualList>` (Core + Source Gen)

**Category:** CPU | **Priority:** P1 | **Impact:** Very High for large lists
**Prerequisites:** OPT-20 (reconciliation bail-out provides the foundation)

**Problem:**
The stress test renders ALL 3,000 boxes every frame, even though only a subset may be visible
in the viewport. Reconciling and laying out 3,000 VisualElements is the dominant per-frame cost.

**Fix:**
A `<VirtualList>` component (or `@foreach` modifier like `@foreach.virtual`) that:
1. Knows the scroll position and container size
2. Calculates which items are in the visible window (+ buffer above/below)
3. Only creates VNodes for visible items (~50-200 instead of 3,000)
4. Reuses VisualElements via a recycle pool as items scroll in/out

This is analogous to `ListView` in Unity's UI Toolkit, but integrated into the UITKX
reactive model.

**Expected gain:** For 3,000 boxes with ~100 visible: reconcile 100 instead of 3,000.
~30x reduction in per-frame reconciliation work. FPS could reach 47+ (pure UITK parity).

**Risk:** High complexity. Requires scroll event integration, dynamic height estimation,
and careful recycling to avoid visual artifacts.

---

## The Core Bottleneck: Why the Typed Pipeline Matters

The stress test's overhead comes from one fundamental design choice: **host element props are
serialized to `Dictionary<string, object>` on every render.** This causes a per-element-per-frame
allocation waterfall:

```
Source gen emits:
  new VisualElementProps { Style = new Style { Left = x, Top = y, ... } }
                          ↑ alloc 1: BaseProps   ↑ alloc 2: Style (Dict)
  
V.VisualElement() calls:
  props.ToDictionary()
  ↑ alloc 3: Dict<string, object> with ~1 entry ("style" → Style ref)
  
new VirtualNode(..., properties: dict)
  ↑ alloc 4: VNode (but reused via fiber alternate after first render)

Total: 3 fresh allocations per element per frame (after first render)
For 3000 boxes: 9000 allocations per frame = ~540,000 allocations per second at 60fps
```

These allocations are fundamentally unnecessary. The framework already has typed `BaseProps`
objects with typed `Style` sub-objects. It converts them to `Dictionary<string, object>` only
because the reconciler and `PropsApplier` were designed around dictionaries.

The typed pipeline keeps the typed objects all the way through:
```
TYPED (proposed):
  new VisualElementProps { Style = new Style { Left = x, Top = y, ... } }
  ↑ alloc 1: BaseProps   ↑ alloc 2: Style
  → V.VisualElement() stores VisualElementProps directly (NO ToDictionary)
  → fiber.PendingHostProps = visElemProps
  → DiffAndApply compares typed fields: if (old.Style.Left != new.Style.Left) apply(Left)
  
Total: 2 allocations per element per frame (BaseProps + Style)
Dict allocation: ZERO
Dict iteration: ZERO
```

This cuts allocations by 33% and eliminates all dictionary hashing/iteration CPU overhead.
A further optimization (making `Style` a struct or pooling `BaseProps` instances) could
reduce it further, but that's a separate concern.

**Effort estimate:** The typed pipeline touches ~10 files across Core, Props, and Source Gen.
It can be implemented incrementally — Layer 1 (typed equality check) works as a standalone
improvement, and Layer 2 (full typed diff) builds on it. The existing dict path remains as
a fallback for custom elements, so there's no big-bang migration.
