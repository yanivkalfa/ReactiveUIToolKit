# Quick Wins Batch: OPT-4 + OPT-5 + OPT-7 + OPT-11

**Status:** ❌ FAILED & REVERTED  
**Date:** April 25, 2026  
**Context:** After OPT-16 (Style/BaseProps pooling), OPT-18 (VNode pooling), and OPT-10 (IIFE closure elimination), per-frame allocations are near-zero and GC stutter is eliminated. FPS is stable at ~35. The remaining ~12 FPS gap to pure UITK (47.7) is CPU time. These four quick wins target low-hanging CPU waste and the last significant per-frame allocation.

**Result:** All four were attempted, caused test failures and subtle bugs. All changes were reverted.

**Combined expected gain:** +2.5–5 FPS  
**Combined effort:** Small (all four are < 20 lines of change each)  
**Combined risk:** Very Low (all purely internal, no public API changes, no source gen changes, no IDE/HMR changes)

---

## Table of Contents

1. [OPT-7: Cache Slice Delegate](#opt-7-cache-slice-delegate)
2. [OPT-4: Pool MapRemainingChildren Dict](#opt-4-pool-mapremainingchildren-dict)
3. [OPT-5: ScheduleUpdateOnFiber WIP/Metrics Guard](#opt-5-scheduleupdateonfiber-wipmetrics-guard)
4. [OPT-11: Skip CommitDeletions Tree Walk](#opt-11-skip-commitdeletions-tree-walk)
5. [Impact Analysis](#impact-analysis)
6. [Risk Assessment](#risk-assessment)
7. [Implementation Order](#implementation-order)

---

## OPT-7: Cache Slice Delegate

### Problem

`ScheduleRootWork()` creates a **new local function `Slice()`** every time it's called:

```csharp
// Current code — Shared/Core/Fiber/FiberReconciler.cs line ~305
private void ScheduleRootWork(IScheduler.Priority priority)
{
    if (_scheduler == null) { WorkLoop(); return; }

    void Slice()                         // ← new delegate instance every call
    {
        ProcessWorkUntilDeadline();
        if (_nextUnitOfWork != null)
            ScheduleRootWork(priority);  // ← captures `priority`
    }

    _scheduler.Enqueue(Slice, priority);
}
```

**Three problems:**

1. **Delegate allocation**: Every call creates a new `Action` delegate wrapping the local function. The local function captures `priority`, so the compiler generates a closure object. With 4 `setState` calls per frame (stress test: `setBoxes`, `setAvgFps`, `setElapsed`, `setTotalFrames`), that's 4 closure allocations per frame.

2. **Broken dedup in `RenderScheduler`**: The runtime scheduler uses `HashSet<Action>` to deduplicate enqueued actions. But since every `Slice` is a **different delegate instance**, `tracker.Add(action)` always returns `true` — dedup is silently broken. All 4 calls get enqueued.

3. **No dedup in `EditorRenderScheduler`**: The editor scheduler has zero dedup — raw `Queue<Action>`. Every call is enqueued unconditionally.

**What actually happens per frame (stress test):**
- `useStressTest` hook calls 4 `setState` functions
- Each triggers `ScheduleUpdateOnFiber` → `ScheduleRootWork(Normal)`
- 4 separate `Slice` delegates enqueued into the scheduler
- Scheduler executes all 4:
  - Slice 1: `ProcessWorkUntilDeadline()` → does all the real work
  - Slice 2: `ProcessWorkUntilDeadline()` → immediate return (`_nextUnitOfWork == null`)
  - Slice 3: same no-op
  - Slice 4: same no-op
- Net: 3 wasted scheduler queue operations + 3 wasted method calls + 4 closure allocs

### Fix

Cache the delegate as a field. Add a `_sliceScheduled` flag to prevent redundant enqueue:

```csharp
// New fields on FiberReconciler:
private Action _cachedSlice;
private IScheduler.Priority _scheduledPriority;
private bool _sliceScheduled;

private void ScheduleRootWork(IScheduler.Priority priority)
{
    if (_scheduler == null)
    {
        WorkLoop();
        return;
    }

    if (_sliceScheduled)
        return;   // Already enqueued — skip

    _scheduledPriority = priority;
    _cachedSlice ??= CachedSlice;   // Create delegate ONCE, reuse forever
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

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberReconciler.cs` | Add 3 fields, rewrite `ScheduleRootWork`, add `CachedSlice` |

### Reentrancy / Concurrency Analysis

- `_sliceScheduled` is set `true` before `Enqueue`, cleared at the top of `CachedSlice`.
- Between set and clear, any additional `ScheduleRootWork` calls (from deferred updates, effects, etc.) are no-ops — the already-enqueued `CachedSlice` will process all pending work.
- After `CachedSlice` runs, if `_nextUnitOfWork != null`, it re-enqueues itself (time slicing).
- **Thread safety**: UITKX runs single-threaded (Unity main thread). No concurrent access.
- **Deferred updates during commit**: `_isCommitting` blocks `ScheduleUpdateOnFiber` → deferred updates are replayed after commit. At that point, `_sliceScheduled` is `false` (cleared by `CachedSlice`), so the re-enqueue works correctly.

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Multiple `setState` in one callback | None | First call enqueues, subsequent skip. The enqueued Slice does all work. |
| Time-sliced render (Slice reschedules itself) | None | `_sliceScheduled` is cleared at top of `CachedSlice`, then re-set if rescheduling. |
| Deferred updates during commit | None | `_sliceScheduled` is already `false` when deferred updates replay. |
| Two reconcilers on same thread | None | `_cachedSlice` and `_sliceScheduled` are instance fields, not static. |
| HMR delegate swap during render | None | HMR goes through `ScheduleUpdateOnFiber`. If `_sliceScheduled` is true, the update flags are still set — the pending `CachedSlice` will process them. |
| Priority escalation (Low → Normal → High) | **Low** | If first call is `Normal` and later call is `High`, the `High` priority is lost. In practice, `ScheduleRootWork` is only called with `Normal`. If needed, add `if (priority > _scheduledPriority) { /* re-enqueue at higher priority */ }`. |

### HMR Impact

None. HMR uses the same `ScheduleUpdateOnFiber` → `ScheduleRootWork` path. The cached delegate is an instance method on the same `FiberReconciler`, so HMR assembly swaps don't affect it (the reconciler instance persists across HMR).

### IDE Extension / Source Gen Impact

None. This is purely a runtime reconciler change.

### Expected Gain

- **Allocation**: Eliminates 4 closure allocs/frame (minor — ~128 bytes)
- **CPU**: Eliminates 3 no-op `ProcessWorkUntilDeadline` calls + 3 scheduler queue operations/frame
- **Estimated FPS**: +0.5–1 FPS

---

## OPT-4: Pool MapRemainingChildren Dict

### Problem

`ReconcileChildrenWithKeys` calls `MapRemainingChildren` which allocates a **new `Dictionary<string, FiberNode>`** every call:

```csharp
// Current code — Shared/Core/Fiber/FiberChildReconciliation.cs line ~464
private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
{
    var map = new Dictionary<string, FiberNode>();   // ← allocation
    var child = firstChild;
    int index = 0;

    while (child != null)
    {
        var key = child.Key ?? IndexToString(index);
        map[key] = child;
        child = child.Sibling;
        index++;
    }

    return map;
}
```

In the stress test, this dict has **3,000 entries** (one per keyed box). A `Dictionary<string, FiberNode>` with 3,000 entries:
- Internal `entries[]` array: 3,000 × 28 bytes = ~84 KB
- Internal `buckets[]` array: ~4,096 × 4 bytes = ~16 KB
- Total: **~100–200 KB per frame** (varies by load factor)

This is the **single largest remaining per-frame allocation** after OPT-16/18/10.

### Fix

Use a `[ThreadStatic]` cached dictionary. `Clear()` resets entries without deallocating the backing arrays:

```csharp
[ThreadStatic]
private static Dictionary<string, FiberNode> s_childMap;

private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
{
    var map = s_childMap ??= new Dictionary<string, FiberNode>(64);
    map.Clear();

    var child = firstChild;
    int index = 0;

    while (child != null)
    {
        var key = child.Key ?? IndexToString(index);
        map[key] = child;
        child = child.Sibling;
        index++;
    }

    return map;
}
```

**Why initial capacity 64, not 3,000?**  
The dictionary self-resizes on first use to fit the actual child count, and then `Clear()` preserves that capacity for subsequent frames. Starting at 64 avoids over-allocating for small child lists. After the first frame with 3,000 children, it stays at 4,096 capacity.

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add `[ThreadStatic]` field, rewrite `MapRemainingChildren` (3 lines changed) |

### Reentrancy Analysis

**Can `MapRemainingChildren` be called reentrantly (nested)?**

The reconciler's work loop is **iterative**, not recursive:
1. `PerformUnitOfWork(fiber)` → `BeginWork(fiber)` → may call `ReconcileChildren` → may call `MapRemainingChildren`
2. Returns `fiber.Child` as next unit of work
3. Next iteration: `PerformUnitOfWork(fiber.Child)` → `BeginWork(fiber.Child)` → ...

These are **sequential** calls. By the time fiber.Child's `ReconcileChildren` runs, the parent's `ReconcileChildrenWithKeys` has already finished — the dict is no longer in use. No reentrancy.

**Can two separate `ReconcileChildrenWithKeys` calls overlap?**

No. The `foreach` loop inside `ReconcileChildrenWithKeys` runs to completion (walks all `newChildren`) before returning. It calls `UpdateSlot` / `CreateFiber` which are pure factory calls — they never trigger another `ReconcileChildren`.

**`[ThreadStatic]` safety?**

Unity runs all UI on the main thread. Even if multiple reconcilers exist (e.g., editor + play mode), they execute sequentially within the same frame. `[ThreadStatic]` ensures the dict is per-thread, not per-reconciler, which is correct — only one reconciler runs at a time.

### Remaining-children enumeration

After the `for` loop, `ReconcileChildrenWithKeys` iterates `existingChildren.Values` to delete orphans:

```csharp
foreach (var oldFiber in existingChildren.Values)
{
    DeleteChild(wipFiber, oldFiber);
}
```

With the `[ThreadStatic]` approach, `existingChildren` IS `s_childMap`. This `foreach` loop runs before `MapRemainingChildren` returns to the caller (it's still inside `ReconcileChildrenWithKeys`). So the static dict is still valid during this enumeration. ✅

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Large child list (3,000+) | None | Dict auto-resizes on first use, stays resized via `Clear()`. |
| Small child list after large | None | `Clear()` keeps capacity. Slight over-allocation (~100 KB retained in steady state), acceptable. |
| No keyed children (index-based reconciliation) | None | `ReconcileChildrenByIndex` is a separate path — doesn't call `MapRemainingChildren`. |
| Multiple reconcilers on same thread | None | Sequential execution. Dict is cleared at start of each call. |
| `DeleteChild` during orphan cleanup | None | `DeleteChild` only adds to `parentFiber.Deletions` — doesn't touch the dict. |

### HMR Impact

None. HMR triggers a re-render which flows through the same `ReconcileChildren` path. The dict pool is transparent to HMR.

### IDE Extension / Source Gen Impact

None. Purely runtime reconciler internal.

### Expected Gain

- **Allocation**: Eliminates ~100–200 KB/frame dictionary allocation (the last major alloc)
- **CPU**: Saves dict constructor + capacity resize cost. `Clear()` is O(N) on entries but avoids allocator pressure.
- **Estimated FPS**: +1–2 FPS

---

## OPT-5: ScheduleUpdateOnFiber WIP/Metrics Guard

### Problem

When a component calls multiple `setState` in one callback, each triggers `ScheduleUpdateOnFiber`. In the stress test, the `useStressTest` hook calls **4 setters** (`setBoxes`, `setAvgFps`, `setElapsed`, `setTotalFrames`) per frame. Each call:

1. **Resets metrics** (`_workUnitCount = 0`, `_sliceCount = 0`, etc.) — overwrites valid data
2. **Restarts stopwatch** (`_renderStopwatch.Restart()`) — timing from wrong origin
3. **Walks full parent chain** (fiber → root) marking `SubtreeHasUpdates` — redundant after 1st call
4. **Recreates / resets WIP root** — redundant WIP creation + `ExtractProps` + array alloc

```csharp
// Current code (abbreviated) — line ~130
public void ScheduleUpdateOnFiber(FiberNode fiber, VirtualNode vnode, bool scheduleWork = true)
{
    // Reset metrics for this render       ← PROBLEM: wiped on every call
    _workUnitCount = 0;
    _sliceCount = 0;
    _yieldCount = 0;
    _effectsCommitted = 0;
    _renderStopwatch.Restart();

    // ... full parent walk ...            ← PROBLEM: redundant after 1st call

    // Create work-in-progress root        ← PROBLEM: WIP already exists after 1st call
    _workInProgressRoot = CreateWorkInProgress(rootCurrent, vnode);
    _root.WorkInProgress = _workInProgressRoot;
    _nextUnitOfWork = _workInProgressRoot;

    // Schedule work                       ← PROBLEM: already scheduled after 1st call
    ScheduleRootWork(IScheduler.Priority.Normal);
}
```

### Fix

Guard the metrics reset and WIP creation. The parent walk MUST remain (it validates deletion flags and root identity — see plan notes).

```csharp
public void ScheduleUpdateOnFiber(FiberNode fiber, VirtualNode vnode, bool scheduleWork = true)
{
    if (_root == null) return;

    // Only reset metrics on first schedule per render cycle
    if (_workInProgressRoot == null)
    {
        _workUnitCount = 0;
        _sliceCount = 0;
        _yieldCount = 0;
        _effectsCommitted = 0;
        _renderStopwatch.Restart();
    }

    if (vnode != null) { _root.RootVNode = vnode; }

    // Mark the target fiber as having an update
    if (fiber != null) { fiber.HasPendingStateUpdate = true; }

    // Parent walk — KEEP AS-IS (validates deletion flags + root identity)
    FiberNode rootCurrent = fiber;
    bool isDeleted = false;
    while (rootCurrent != null) { /* ... unchanged ... */ }
    if (isDeleted) return;
    // ... root validation ... (unchanged)
    if (_isCommitting) { _deferredUpdates.Enqueue((fiber, vnode)); return; }

    // Only create WIP on first schedule per render cycle
    if (rootCurrent == _root.WorkInProgress)
    {
        // WIP already exists — just ensure pending update flags are set (already done above)
        _workInProgressRoot = rootCurrent;
    }
    else if (_root.WorkInProgress != null)
    {
        // WIP was created by a previous setState in this batch — reuse it
        _workInProgressRoot = _root.WorkInProgress;
    }
    else
    {
        // First setState in this cycle — create WIP
        _workInProgressRoot = CreateWorkInProgress(rootCurrent, vnode);
        _root.WorkInProgress = _workInProgressRoot;
        _nextUnitOfWork = _workInProgressRoot;
    }

    if (scheduleWork) { /* ... unchanged ... */ }
}
```

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberReconciler.cs` | Guard metrics reset + WIP creation in `ScheduleUpdateOnFiber` |

### What the Guard Preserves

The parent walk is NOT optimized (no early-out). It still:
- Checks every ancestor for `EffectFlags.Deletion` → rejects updates on deleted fibers
- Validates root matches `_root.Current` / `_root.WorkInProgress` / alternate
- Marks `SubtreeHasUpdates` on all ancestors (redundant but harmless — O(depth) not O(N))

The walk is O(tree depth), not O(tree size). For typical UIs (depth ≤ 20), it's ~20 comparisons per `setState` call — negligible.

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| 4 setState calls in one callback | None | 1st creates WIP + resets metrics. 2nd–4th skip both. All 4 mark `HasPendingStateUpdate`. |
| setState with new root vnode | None | `_root.RootVNode = vnode` still runs on every call. Only metrics/WIP are guarded. |
| Deferred updates during commit | None | `_isCommitting` check fires before WIP guard. Deferred replay happens after commit when `_workInProgressRoot` is null, so WIP is correctly created. |
| `scheduleWork: false` (deferred updates) | None | WIP guard works the same regardless of `scheduleWork`. |
| HMR-triggered re-render | None | HMR calls `ScheduleUpdateOnFiber` with a vnode. If WIP exists (unlikely during HMR), it reuses it. If not, creates fresh. |
| Concurrent fiber updates on different fibers | None | Each `setState` marks its own fiber's `HasPendingStateUpdate`. The WIP root is shared — it doesn't need to be recreated for each fiber. |

### HMR Impact

None. HMR goes through the same path. The WIP guard is transparent.

### IDE Extension / Source Gen Impact

None.

### Expected Gain

- **Allocation**: Eliminates 3 redundant `CreateWorkInProgress` calls/frame (each allocates `new[] { vnode }`)
- **CPU**: Saves 3 × (`ExtractProps` + `CreateWorkInProgress` + `ScheduleRootWork`) per frame
- **Estimated FPS**: +0.5–1 FPS
- **Bonus**: Metrics are now accurate (not reset 4 times — only once at start of render cycle)

---

## OPT-11: Skip CommitDeletions Tree Walk

### Problem

`CommitDeletions` recursively walks the **entire WIP fiber tree** looking for fibers with non-null `Deletions` lists:

```csharp
// Current code — line ~770
private void CommitDeletions(FiberNode fiber)
{
    if (fiber == null) return;

    if (fiber.Deletions != null)
    {
        foreach (var deletion in fiber.Deletions)
            CommitDeletion(deletion);
        fiber.Deletions = null;
    }

    var child = fiber.Child;
    while (child != null)
    {
        CommitDeletions(child);       // ← recursive walk
        child = child.Sibling;
    }
}
```

For the stress test (3,000 box fibers + ~10 container fibers ≈ 3,010 fibers), this walks **all 3,010 fibers** every frame. In steady state (no elements being added or removed), `fiber.Deletions` is `null` on every single fiber — the entire walk is a no-op.

Each recursion step: null check + Deletions null check + child loop + sibling walk. At 3,010 fibers, that's ~9,000+ comparisons per frame for nothing.

### Fix: Track Deletion Parents During Reconciliation

Instead of walking the tree, collect deletion-bearing parents during reconciliation and process only those:

**Step 1: Add a `[ThreadStatic]` collection to `FiberChildReconciliation`:**

```csharp
// In FiberChildReconciliation.cs:
[ThreadStatic]
private static List<FiberNode> s_fibersWithDeletions;

internal static List<FiberNode> DeletionParents
    => s_fibersWithDeletions;

internal static void ClearDeletionTracking()
{
    s_fibersWithDeletions?.Clear();
}
```

**Step 2: Record in `DeleteChild`:**

```csharp
private static void DeleteChild(FiberNode parentFiber, FiberNode childFiber)
{
    if (parentFiber.Deletions == null)
    {
        parentFiber.Deletions = new List<FiberNode>();
        // Track this parent for CommitRoot
        (s_fibersWithDeletions ??= new List<FiberNode>(4)).Add(parentFiber);
    }

    childFiber.EffectTag |= EffectFlags.Deletion;
    parentFiber.Deletions.Add(childFiber);
}
```

**Step 3: Replace `CommitDeletions` in `CommitRoot`:**

```csharp
// In CommitRoot, replace:
//   CommitDeletions(_root.WorkInProgress);
// With:
var deletionParents = FiberChildReconciliation.DeletionParents;
if (deletionParents != null && deletionParents.Count > 0)
{
    for (int i = 0; i < deletionParents.Count; i++)
    {
        var parent = deletionParents[i];
        if (parent.Deletions != null)
        {
            for (int j = 0; j < parent.Deletions.Count; j++)
            {
                CommitDeletion(parent.Deletions[j]);
            }
            parent.Deletions = null;
        }
    }
}
FiberChildReconciliation.ClearDeletionTracking();
```

### Complexity Comparison

| Scenario | Current (tree walk) | Proposed (tracked list) |
|----------|-------------------|----------------------|
| Steady state (no deletions) | O(N) — walks all 3,010 fibers | O(1) — list is empty, skip |
| 1 element removed | O(N) — walks all fibers, finds 1 | O(1) — list has 1 entry |
| K elements removed from K parents | O(N) — walks all fibers, finds K | O(K) — list has K entries |
| Full teardown (all removed) | O(N) — walks tree | O(P) — P = number of parents with deletions |

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add `s_fibersWithDeletions` field, `DeletionParents` property, `ClearDeletionTracking()`, modify `DeleteChild` |
| `Shared/Core/Fiber/FiberReconciler.cs` | Replace `CommitDeletions(...)` call in `CommitRoot` with tracked-list approach |

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| No deletions (steady state) | None | List is null or empty. `ClearDeletionTracking` is a no-op. |
| Deletions during time-sliced render | None | `DeleteChild` is called during BeginWork (reconciliation), which happens before CommitRoot. All deletions are tracked before commit. |
| Multiple reconcilers on same thread | **Low** | `s_fibersWithDeletions` is `[ThreadStatic]`, shared between reconcilers. But reconcilers run sequentially — each `CommitRoot` drains and clears the list before the next reconciler runs. If a reconciler's render phase is interleaved with another's commit (impossible in current design), this would be a problem. |
| `DeleteRemainingChildren` batch deletions | None | Calls `DeleteChild` for each child → each parent's first deletion adds it to the list. Subsequent deletions for the same parent just add to `parent.Deletions`, no duplicate list entries (because the `Deletions == null` check is the gate). |
| HMR full component reset | None | HMR-triggered re-renders go through normal reconciliation → `DeleteChild` → tracked. |
| Old `CommitDeletions` method | None | Remove or keep as dead code. The method is only called from `CommitRoot`. |

**Important subtlety**: `DeleteChild` only adds to `s_fibersWithDeletions` when `parentFiber.Deletions == null` (first deletion for that parent). This means each parent appears in the list **at most once**, even if it has multiple deleted children. ✅

### Alternative Considered: Boolean Flag

A simpler approach: track `_hasAnyDeletions` flag and skip the tree walk when false. This handles the steady-state case but still walks the full tree when ANY deletion exists. The tracked-list approach is O(K) in all cases, making it strictly better.

### HMR Impact

None. Deletions during HMR go through the same `DeleteChild` path.

### IDE Extension / Source Gen Impact

None. Purely runtime reconciler internal.

### Expected Gain

- **CPU**: Eliminates O(N) tree walk (~3,010 nodes) in steady state
- **Estimated FPS**: +0.5–1 FPS

---

## Impact Analysis

### Combined Allocation Savings

| Optimization | Allocs Eliminated | Bytes Saved/Frame |
|-------------|-------------------|-------------------|
| OPT-7 | 4 closure objects | ~128 bytes |
| OPT-4 | 1 Dictionary (3,000 entries) | ~100–200 KB |
| OPT-5 | 3 WIP creations + 3 VNode[] arrays | ~300–500 bytes |
| OPT-11 | 0 (CPU only) | 0 |
| **Total** | **~8 objects** | **~100–200 KB** |

After these four, the only remaining per-frame allocations are:
- `@foreach` scaffolding: 1 `List<VirtualNode>` + 1 `ToArray()` per `@foreach` directive (~2–4 allocs)
- `__C()` arrays: params array + List + ToArray for non-overloaded arities (~6–300 allocs)
- `Deletions` list: `new List<FiberNode>()` when elements are actually deleted (0 in steady state)
- `_pendingPassiveEffects`: `new List<FiberNode>()` each commit — could be pooled too

### Combined CPU Savings

| Optimization | CPU Work Eliminated |
|-------------|-------------------|
| OPT-7 | 3 no-op `ProcessWorkUntilDeadline` calls + 3 scheduler enqueue/dequeue/remove ops |
| OPT-4 | Dictionary constructor + capacity resize (avoided by `Clear()` + reuse) |
| OPT-5 | 3 × (`CreateWorkInProgress` + `ExtractProps` + `ScheduleRootWork`) |
| OPT-11 | O(3,010) recursive tree walk with ~9,000 comparisons |
| **Total** | ~10,000+ avoided operations per frame |

### Combined FPS Estimate

| Scenario | Estimated Gain |
|----------|---------------|
| Conservative | +2.5 FPS |
| Optimistic | +5 FPS |
| Expected (stress test, 3,000 boxes) | +3–4 FPS |

This would bring the stress test from ~35 FPS to ~38–39 FPS.

---

## Risk Assessment

### What These Changes DO NOT Touch

- ❌ Source generator (`CSharpEmitter.cs`)
- ❌ HMR emitter (`HmrCSharpEmitter.cs`)
- ❌ IDE extensions (LSP, VSCode, Rider, Visual Studio)
- ❌ Any public API
- ❌ VirtualNode / Style / BaseProps (already pooled)
- ❌ PropsApplier / Element adapters
- ❌ Generated component code

### What These Changes DO Touch

| File | Changes | Risk |
|------|---------|------|
| `Shared/Core/Fiber/FiberReconciler.cs` | OPT-7 + OPT-5 + OPT-11 | Low — all internal methods, no signature changes |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | OPT-4 + OPT-11 | Low — static class, internal methods only |

### Regression Vectors

1. **Rendering correctness**: OPT-4 (dict pool) and OPT-11 (deletion tracking) touch reconciliation, the most sensitive part of the framework. However:
   - OPT-4 changes only the dict's lifecycle, not the algorithm.
   - OPT-11 changes HOW deletions are found, not HOW they're processed.
   - Both are purely mechanical refactors.

2. **Timing/ordering**: OPT-7 (Slice caching) and OPT-5 (WIP guard) affect render scheduling order. However:
   - OPT-7's `_sliceScheduled` flag only prevents redundant enqueues — never drops a needed one.
   - OPT-5's WIP guard only skips re-creation — never misses a fiber's `HasPendingStateUpdate`.

3. **Metrics accuracy**: OPT-5 actually FIXES a bug where metrics were reset 4x per frame, producing wrong timing data.

### Testing Strategy

All four changes can be verified with existing tests:
- Source gen unit tests (1023/1029 pass — 6 pre-existing formatter failures)
- Manual stress test in Unity (FPS counter + visual inspection)
- HMR hot-reload test (modify component → verify live update)

No new tests required, but we could add:
- A stress test asserting `_sliceCount == 1` per frame (OPT-7 correctness)
- A reconciliation test verifying dict reuse via allocation counter

---

## Implementation Order

All four are independent and can be implemented in any order. Recommended order by impact/risk ratio:

| Order | Item | Why First |
|-------|------|-----------|
| 1 | **OPT-7** | Highest confidence fix — caching a delegate is trivially safe. Also fixes the broken dedup bug. |
| 2 | **OPT-4** | Highest allocation impact (~200 KB/frame). Simple `[ThreadStatic]` + `Clear()`. |
| 3 | **OPT-5** | Fixes metrics accuracy bug as a bonus. Guard logic is straightforward. |
| 4 | **OPT-11** | Most moving parts (two files, new tracking list). But still simple. |

All four can be done in a single implementation session and tested together.

---

## Appendix: Code References

### FiberReconciler.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `ScheduleUpdateOnFiber` | ~130 | OPT-5 |
| `ScheduleRootWork` | ~305 | OPT-7 |
| `ProcessWorkUntilDeadline` | ~330 | (called by OPT-7's CachedSlice) |
| `CommitRoot` | ~650 | OPT-11 |
| `CommitDeletions` | ~770 | OPT-11 (replaced) |

### FiberChildReconciliation.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `MapRemainingChildren` | ~464 | OPT-4 |
| `DeleteChild` | ~441 | OPT-11 |
| `ReconcileChildrenWithKeys` | ~162 | (uses OPT-4's pooled dict) |
