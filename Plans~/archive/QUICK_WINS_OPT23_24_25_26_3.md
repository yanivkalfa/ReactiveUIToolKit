# Quick Wins Batch: OPT-23 + OPT-24 + OPT-25 + OPT-26 + OPT-3

**Status:** ✅ COMPLETE (OPT-23B + OPT-24 + OPT-25 + OPT-26 implemented April 25, 2026) / OPT-23A dropped / OPT-3 deferred  
**Date:** April 25, 2026  
**Current FPS:** 37 (after OPT-22)  
**Pure UITK baseline:** 47.7 FPS  
**Gap:** ~10.7 FPS  
**Combined expected gain:** ~1–2ms per frame (~1–3 FPS)  
**Combined effort:** Small–Medium (each is 5–30 lines of change)  
**Combined risk:** Low  

---

## Table of Contents

1. [OPT-23: Eliminate Double Style Diff](#opt-23-eliminate-double-style-diff)
2. [OPT-25: Pool MapRemainingChildren Dict + pendingPassiveEffects List](#opt-25-pool-mapremainingchildren-dict--pendingpassiveeffects-list)
3. [OPT-26: Merge CommitDeletions Into CommitPropsAndClearFlags](#opt-26-merge-commitdeletions-into-commitpropsandclearflags)
4. [OPT-24: Host-Only CloneForReuse Fast Path](#opt-24-host-only-cloneforreuse-fast-path)
5. [OPT-3: __C() Arity Overloads](#opt-3-__c-arity-overloads)
6. [Impact Analysis](#impact-analysis)
7. [Risk Assessment](#risk-assessment)
8. [Implementation Order](#implementation-order)

---

## OPT-23: Eliminate Double Style Diff

### Problem

Style comparison runs **twice** for every changed element:

**Pass 1 — `CompleteWork` (reconciliation phase):**
```csharp
// FiberReconciler.cs line ~532
else if (
    fiber.PendingHostProps != null
        ? !fiber.PendingHostProps.ShallowEquals(fiber.HostProps)  // ← calls ShallowEquals
        : !AreHostPropsEqual(fiber.PendingProps, fiber.Props)
)
```

Inside `ShallowEquals` (BaseProps.cs line ~584):
```csharp
if (!Style.SameInstance(Style, other.Style) && !StyleEquals(Style, other.Style))
    return false;
```

`StyleEquals` calls `Style.TypedEquals` which iterates all set bits comparing each field via the big `FieldEquals` switch. For a box with 7 style properties (`Position, Left, Top, Width, Height, BackgroundColor, BorderRadius`), this does:
- `SameInstance` check (2 null checks + ReferenceEquals + generation compare)
- If different instance: `TypedEquals` → compare `_setBits0`/`_setBits1` → iterate 7 bits → 7 `FieldEquals` switch calls

**Pass 2 — `CommitUpdate` → `ApplyTypedDiff` → `DiffStyle` (commit phase):**
```csharp
// TypedPropsApplier.cs line ~157
if (!Style.SameInstance(prev.Style, next.Style))
    DiffStyle(element, prev.Style, next.Style);
```

Inside `DiffStyle`:
```csharp
public static void DiffStyle(VisualElement element, Style prev, Style next)
{
    if (Style.SameInstance(prev, next)) return;  // redundant check
    ulong prevBits0 = prev?._setBits0 ?? 0UL;   // read bitmasks again
    ulong nextBits0 = next?._setBits0 ?? 0UL;
    // ... compute removed bits, apply changed bits ...
}
```

**The waste:** For 3,000 boxes in the stress test, ShallowEquals compares style fields to determine "did anything change?" — then DiffStyle reads the same bitmasks and fields again to determine "what changed?". The first pass is pure decision-making; the second pass does the actual work. The first pass could be eliminated if we could mark the element as needing an update more cheaply.

### What ShallowEquals Actually Decides

`ShallowEquals` returns `false` (meaning "needs update") when **any** BaseProps field differs. For the stress test, the only fields that change per frame are `Style.Left` and `Style.Top` — so ShallowEquals:

1. Compares 13 non-style BaseProps fields (Name, ClassName, Ref, ContentContainer, Visible, Enabled, Tooltip, ViewDataKey, PickingMode, Focusable, TabIndex, DelegatesFocus, LanguageDirection) — all match
2. Does `Style.SameInstance` — returns false (different pooled instance)
3. Does `StyleEquals` → `TypedEquals` → iterates 7 set bits → finds `Left` differs → returns false
4. **Short-circuits** — doesn't check events or ExtraProps

So for a changed box, ShallowEquals does: 13 field comparisons + SameInstance(~4 ops) + TypedEquals(bitmask compare + ~1-3 FieldEquals calls until mismatch). Average ~20 operations.

Then DiffStyle does: SameInstance(~4 ops) + bitmask math (~8 ops) + iterate 7 set bits comparing each + apply 2 changed fields to the VisualElement.

**The redundancy:** The bitmask reads and field comparisons in TypedEquals overlap with DiffStyle's work. DiffStyle must still run (it applies changes), but TypedEquals's full equality check is wasted — we only needed to know "is style different?" which can be answered much cheaper.

### Fix: Use Bitmask-Only Quick-Reject in ShallowEquals

Replace the full `StyleEquals(Style, other.Style)` call in `ShallowEquals` with a cheaper bitmask-only comparison:

```csharp
// In BaseProps.ShallowEquals, replace:
//   if (!Style.SameInstance(Style, other.Style) && !StyleEquals(Style, other.Style))
// With:
if (!Style.SameInstance(Style, other.Style) && !Style.QuickEquals(Style, other.Style))
    return false;
```

Where `QuickEquals` is:
```csharp
// In Style.cs:
internal static bool QuickEquals(Style a, Style b)
{
    if (a == null && b == null) return true;
    if (a == null || b == null) return false;
    // Different set bits → definitely different
    return a._setBits0 == b._setBits0 && a._setBits1 == b._setBits1;
}
```

**Wait — this is wrong.** `QuickEquals` returning `true` (same bitmasks) does NOT mean the styles are equal — the values could differ. Example: `Left = 100` vs `Left = 200` both have the same `BIT_LEFT` set. ShallowEquals would return `true` (no update), silently dropping the style change.

### Fix (Revised): No separate quick-reject — eliminate the ShallowEquals style check entirely

The real insight is: **ShallowEquals doesn't need to compare styles at all for the purpose of Update marking**. Here's why:

- If styles are `SameInstance` → no style change → ShallowEquals should NOT return false on this account
- If styles are NOT `SameInstance` → there MIGHT be a style change → mark Update and let DiffStyle figure it out

The current code does the expensive `TypedEquals` to avoid marking unchanged elements for Update when only the Style instance differs (due to pooling creating different instances with identical values). But this optimization is rarely triggered — in the stress test, Left/Top change every frame, so TypedEquals always returns false anyway.

**The fix: Replace `!StyleEquals(Style, other.Style)` with `true` (always consider different Style instances as "changed"):**

```csharp
// In BaseProps.ShallowEquals, replace:
if (!Style.SameInstance(Style, other.Style) && !StyleEquals(Style, other.Style))
    return false;
// With:
if (!Style.SameInstance(Style, other.Style))
    return false;
```

This means: if the Style instance is different (not SameInstance), always return false from ShallowEquals → always mark Update → always run DiffStyle in commit phase. DiffStyle handles unchanged values gracefully (it compares each field and only applies if different).

**But there's a catch:** Elements with static styles shared across frames (`static readonly Style btnStyle = new Style { ... }`) use the SAME Style instance. `SameInstance` returns true → ShallowEquals skips style → correct. No regression.

For pooled styles (the stress test case), `SameInstance` returns false (different instance). Currently, `TypedEquals` catches the case where values are identical despite different instances. By removing `TypedEquals`, we'd mark these elements for Update even when style values haven't changed. In the stress test, this doesn't matter (all 3,000 boxes have changed Left/Top). But in a real app with mostly static UIs, this could cause unnecessary DiffStyle calls on unchanged elements.

**Trade-off analysis for stress test:**
- 3,000 boxes × ALL change style → TypedEquals always returns false (wasted work) → removing it saves ~20 ops × 3,000 = ~60,000 ops/frame → **WIN**
- Real app: ~50 elements, ~45 unchanged → 45 unnecessary DiffStyle calls → DiffStyle fast-paths via bitmask (same bits, same values → iterate 7 bits, all match) → ~315 extra ops → **NEGLIGIBLE**

### Alternative: Keep TypedEquals but skip event comparison in ShallowEquals

Actually, re-examining ShallowEquals — we already skip events via `_hasEvents` in ApplyDiff. But ShallowEquals still checks ALL 43 event fields individually:

```csharp
if (OnClick != other.OnClick) return false;
if (OnClickCapture != other.OnClickCapture) return false;
// ... 41 more ...
```

We could add the same `_hasEvents` optimization to ShallowEquals:

```csharp
// Replace 43 individual event comparisons with:
if (_hasEvents || other._hasEvents)
{
    if (OnClick != other.OnClick) return false;
    // ... rest of events ...
}
```

**Combined savings:**
- Remove TypedEquals from ShallowEquals: saves ~10-20 ops per changed element
- Add `_hasEvents` guard in ShallowEquals: saves ~43 comparisons per element (same as OPT-22 did for ApplyDiff)

### Recommended Fix

**Part A: Remove StyleEquals from ShallowEquals** — replace with just `SameInstance` check. If different instance, assume different. DiffStyle handles the actual comparison during commit.

**Part B: Add _hasEvents guard in ShallowEquals** — wrap the 43 event comparisons with `if (_hasEvents || other._hasEvents)`.

### Files Changed

| File | Change |
|------|--------|
| `Shared/Props/Typed/BaseProps.cs` | Remove `StyleEquals` call in `ShallowEquals`, add `_hasEvents` guard around event comparisons |

### Ecosystem Impact

- **Source Generator:** None — doesn't touch ShallowEquals
- **HMR:** None — HMR triggers re-render → CompleteWork calls ShallowEquals → faster now
- **IDE Extensions:** None — ShallowEquals is runtime-only
- **Unity:** None — purely internal comparison logic
- **Pool lifecycle:** Removing StyleEquals means pooled styles with identical values will trigger DiffStyle (which handles them correctly via field-by-field comparison). No correctness issue.
- **BaseProps subclasses:** All ~50 subclasses override ShallowEquals and call `base.ShallowEquals()`. The base change propagates automatically.

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Static shared Style instances | None | `SameInstance` returns true → ShallowEquals still skips → correct |
| Pooled styles with identical values | **None** (minor perf) | ShallowEquals returns false → element marked Update → DiffStyle runs → finds no changes → applies nothing. Functionally correct, slightly more work than before for these specific elements. |
| Elements with no style | None | Both styles null → `SameInstance(null, null)` returns true → skip. |
| Elements with events (no _hasEvents guard) | None | Same behavior as before — just the comparison runs 43 times. The guard is additive. |

### Expected Gain

**Part A (remove StyleEquals):** ~0.2-0.3ms — eliminates TypedEquals for 3,000 elements per frame  
**Part B (_hasEvents in ShallowEquals):** ~0.1-0.2ms — skips 43 delegate comparisons × 3,000 elements  
**Combined:** ~0.3-0.5ms

---

## OPT-25: Pool MapRemainingChildren Dict + pendingPassiveEffects List

### Problem 1: MapRemainingChildren Dict

`MapRemainingChildren` allocates a new `Dictionary<string, FiberNode>` every frame:

```csharp
// FiberChildReconciliation.cs line ~466
private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
{
    var map = new Dictionary<string, FiberNode>();  // ← allocation
    // ... populate with 3,000 entries ...
    return map;
}
```

For 3,000 keyed boxes, the dictionary's internal arrays total ~100-200 KB per frame. This is the **single largest remaining per-frame allocation**.

**Note:** This is the same optimization as the failed OPT-4. The previous attempt used `[ThreadStatic]` and caused test failures. The issue was likely subtle reentrancy or test isolation problems. This time we'll use a simpler approach: an instance-level field on `FiberReconciler` (not static) to avoid cross-test contamination.

### Problem 2: pendingPassiveEffects List

`CommitRoot` allocates a new `List<FiberNode>()` on every commit:

```csharp
// FiberReconciler.cs line ~659
_pendingPassiveEffects = new List<FiberNode>();
```

In the stress test, this list typically has 0 entries (no passive effects) but is allocated anyway. In a real app, it might have 5-20 entries.

### Fix 1: Pool MapRemainingChildren Dict

Use a `[ThreadStatic]` dictionary with `Clear()` — same approach as OPT-4's plan but with extra care:

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

**Reentrancy safety (re-verified):**
The work loop is iterative: `PerformUnitOfWork(fiber)` → `BeginWork` → `ReconcileChildren` → `MapRemainingChildren` → returns dict → the `for` loop inside `ReconcileChildrenWithKeys` walks ALL newChildren using this dict → deletes orphans → method returns → next `PerformUnitOfWork` call is on `fiber.Child`. By that time, the parent's keyed reconciliation is complete. No overlap.

**Why OPT-4 failed previously:**
Unknown — the conversation summary says "caused test failures and subtle bugs" but doesn't specify which tests. The implementation described in the OPT-4 plan is mechanically correct. Possible causes:
1. An implementation bug (typo, wrong variable name)
2. Test setup that creates multiple reconcilers sharing state via `[ThreadStatic]`
3. The dict was modified during enumeration (but `DeleteChild` doesn't touch the dict)

**Mitigation:** Implement carefully and run tests one-by-one to identify any failure.

### Fix 2: Pool pendingPassiveEffects List

Replace `new List<FiberNode>()` with a reusable list:

```csharp
// Field on FiberReconciler:
private List<FiberNode> _passiveEffectsPool; // reused across commits

// In CommitRoot, replace:
//   _pendingPassiveEffects = new List<FiberNode>();
// With:
_passiveEffectsPool ??= new List<FiberNode>(8);
_passiveEffectsPool.Clear();
_pendingPassiveEffects = _passiveEffectsPool;
```

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add `[ThreadStatic]` field, rewrite `MapRemainingChildren` |
| `Shared/Core/Fiber/FiberReconciler.cs` | Add `_passiveEffectsPool` field, reuse in `CommitRoot` |

### Ecosystem Impact

- **Source Generator:** None
- **HMR:** None — reconciliation and commit are transparent to HMR
- **IDE Extensions:** None
- **Unity:** None
- **Tests:** Must verify no test relies on dict identity or list identity

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Dict pool (same as OPT-4 analysis) | **Low** | See full reentrancy analysis in OPT-4 plan. The work loop is iterative, not recursive. |
| passiveEffectsPool: list reuse across commits | None | `Clear()` at start of each commit. The list is consumed synchronously (either flushed immediately or captured in a closure for scheduler). In the scheduler case, the closure captures `toFlush` which is reassigned from `_pendingPassiveEffects` BEFORE we null it — the closure holds the reference, and `_passiveEffectsPool` is cleared on next commit. |
| **Wait — scheduler closure captures the pooled list:** | **MEDIUM** | If scheduler is present: `var toFlush = _pendingPassiveEffects;` then `_pendingPassiveEffects = null;`. The closure references `toFlush`. If the next commit fires before the batched effect runs, `_passiveEffectsPool.Clear()` would clear the list while the closure still references it. **This is a bug.** |

**Fix for scheduler race:** Don't pool the list when scheduler is present. Only pool in sync mode:

```csharp
if (_scheduler != null)
{
    // Async mode: allocate fresh list because the closure captures it
    _pendingPassiveEffects = new List<FiberNode>(8);
}
else
{
    // Sync mode: reuse pooled list (effects run immediately, no race)
    _passiveEffectsPool ??= new List<FiberNode>(8);
    _passiveEffectsPool.Clear();
    _pendingPassiveEffects = _passiveEffectsPool;
}
```

Actually, the stress test runs with a scheduler (RenderScheduler). So the pool only helps in sync/test mode. **This makes the list pool effectively useless for the stress test.** The allocation is small (~32 bytes + 64 bytes for initial capacity 8) and happens once per frame. Not worth the complexity.

**Decision: DROP the pendingPassiveEffects pool. Keep only the MapRemainingChildren dict pool.**

### Expected Gain

- **Dict pool:** Eliminates ~100-200 KB/frame allocation → ~0.2-0.5ms (allocator pressure + GC amortized cost)
- **List pool:** Dropped — negligible benefit, scheduler race risk
- **Combined:** ~0.2-0.5ms

---

## OPT-26: Merge CommitDeletions Into CommitPropsAndClearFlags

### Problem

`CommitRoot` performs **two full tree walks** sequentially:

```csharp
// Walk 1: CommitDeletions — O(N) recursive walk looking for fiber.Deletions != null
CommitDeletions(_root.WorkInProgress);

// ... effect list processing (NOT a tree walk — iterates linked list) ...

// Walk 2: CommitPropsAndClearFlags — O(N) recursive walk
CommitPropsAndClearFlags(_root.Current);
```

For 3,010 fibers, that's ~6,020 node visits per frame. In steady state (no deletions), CommitDeletions visits all 3,010 nodes and finds nothing.

### Ordering Constraint Analysis

The ordering in CommitRoot is:
1. `CommitDeletions(WIP)` — removes deleted elements from the DOM
2. Effect list walk: `CommitWork(effect)` — processes Placement/Update/Deletion effects
3. Swap: `_root.Current = finishedWork`
4. `CommitPropsAndClearFlags(Current)` — commits props, clears flags, updates ComponentState

**Can deletions be moved to step 4?**

Deletions are processed in two places:
- `CommitDeletions` — walks the tree to find parents with `fiber.Deletions != null`, then calls `CommitDeletion(childFiber)` for each entry
- `CommitWork(effect)` — if a fiber has `EffectFlags.Deletion`, calls `CommitDeletion(effect)` directly

Wait — `CommitDeletions` processes deletions stored on **parent** fibers (parent.Deletions list). These are children that were removed during reconciliation. `CommitWork`'s deletion handling processes fibers that are themselves being deleted (EffectFlags.Deletion on the fiber itself).

**The key question:** Does `CommitDeletions` need to run BEFORE `CommitWork`?

Yes: deleted elements must be removed from the DOM BEFORE new elements are placed at the same position. If `CommitPlacement` runs before `CommitDeletion`, the new element might be inserted before the old one, leading to incorrect ordering.

**Can we merge into CommitPropsAndClearFlags instead?**

`CommitPropsAndClearFlags` runs AFTER `CommitWork` (after the swap). By then, all Placements and Updates are done. Moving deletion processing here would be too late — deleted elements would remain in the DOM during Placement.

**Alternative: Just skip the walk when there are no deletions (boolean flag)**

```csharp
// Field on FiberReconciler:
private bool _hasDeletions;

// Set in DeleteChild (via FiberChildReconciliation):
// Need a way to signal from static DeleteChild to the reconciler instance.
// Options:
// A) [ThreadStatic] flag — simple, same approach as OPT-11's "boolean flag alternative"
// B) Check on WIP root: if _root.WorkInProgress.Deletions == null && no child has deletions...
//    — can't know without walking

// Option A:
[ThreadStatic]
internal static bool s_hasDeletions;
```

In `DeleteChild`:
```csharp
private static void DeleteChild(FiberNode parentFiber, FiberNode childFiber)
{
    if (parentFiber.Deletions == null)
        parentFiber.Deletions = new List<FiberNode>();
    
    childFiber.EffectTag |= EffectFlags.Deletion;
    parentFiber.Deletions.Add(childFiber);
    s_hasDeletions = true;  // ← signal to CommitRoot
}
```

In `CommitRoot`:
```csharp
// Replace:
CommitDeletions(_root.WorkInProgress);
// With:
if (FiberChildReconciliation.s_hasDeletions)
{
    CommitDeletions(_root.WorkInProgress);
    FiberChildReconciliation.s_hasDeletions = false;
}
```

**Steady state (no deletions):** `s_hasDeletions` is false → skip entire O(N) walk → **WIN**  
**With deletions:** Walk runs as before → no regression

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add `s_hasDeletions` flag, set in `DeleteChild` |
| `Shared/Core/Fiber/FiberReconciler.cs` | Guard `CommitDeletions` call with `s_hasDeletions` check + clear |

### Ecosystem Impact

- **Source Generator:** None
- **HMR:** None — HMR triggers re-render which may or may not have deletions. The flag handles both cases.
- **IDE Extensions:** None
- **Unity:** None
- **Multiple reconcilers:** `[ThreadStatic]` is per-thread. Reconcilers run sequentially. Each `CommitRoot` clears the flag. Safe.

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Steady state (no deletions) | None | Flag is false, walk skipped. Correct. |
| Elements added/removed | None | `DeleteChild` sets flag → walk runs → processes deletions → flag cleared. |
| HMR full reset (all components re-rendered) | None | If HMR causes deletions, `DeleteChild` fires → flag set → walk runs. |
| Flag not cleared on exception | **Low** | If `CommitRoot` throws after checking the flag but before clearing it, the flag stays true → next commit runs the walk unnecessarily. Harmless — just wastes one O(N) walk. |
| `DeleteRemainingChildren` batch deletes | None | Calls `DeleteChild` repeatedly → flag set on first call, stays true. |

### Expected Gain

- **CPU:** Eliminates O(3,010) recursive tree walk in steady state
- **Estimated:** ~0.2-0.3ms

---

## OPT-24: Host-Only CloneForReuse Fast Path

### Problem

`CloneForReuse` copies ~30 fields for EVERY fiber, regardless of type. For host elements (HostComponent), many fields are always null/default:

```csharp
// FiberFactory.cs — CloneForReuse copies ALL fields:
clone.TypedRender = current.TypedRender;           // always null for HostComponent
clone.TypedProps = current.TypedProps;               // always null for HostComponent
clone.TypedPendingProps = ...;                       // always null for HostComponent
clone.ComponentState = current.ComponentState;       // always null for HostComponent
clone.ContextFrame = current.ContextFrame;           // usually default for HostComponent
clone.ContextProviderId = current.ContextProviderId; // always 0 for HostComponent
clone.ProvidedContext = current.ProvidedContext;      // always null for HostComponent
clone.ErrorBoundaryActive = current.ErrorBoundaryActive;         // always false
clone.ErrorBoundaryShowingFallback = current.ErrorBoundaryShowingFallback; // always false
clone.ErrorBoundaryLastException = current.ErrorBoundaryLastException;     // always null
clone.ErrorBoundaryResetKey = current.ErrorBoundaryResetKey;               // always null
clone.ErrorBoundaryFallback = ...;                   // always null
clone.ErrorBoundaryHandler = ...;                    // always null
clone.ErrorBoundaryChildren = ...;                   // always null
clone.ReadsContext = current.ReadsContext;            // always false for HostComponent
#if UNITY_EDITOR
clone.HmrPreviousRender = current.HmrPreviousRender; // always null for HostComponent
#endif
```

That's **15 wasted field copies** per host fiber (17 in editor mode). With 3,000 host fibers:  
3,000 × 15 = **45,000 unnecessary field writes per frame**.

### Fiber Type Distribution (Stress Test)

| Fiber Tag | Count | Needs All Fields? |
|-----------|-------|-------------------|
| HostComponent | ~3,003 | NO — no TypedRender, no ComponentState, no ErrorBoundary, no Context |
| FunctionComponent | ~4 | YES — has TypedRender, ComponentState, possibly Context |
| Fragment | ~3 | NO — no TypedRender, no HostElement, no ComponentState |
| ErrorBoundary | 0 | YES — needs all ErrorBoundary fields |
| HostPortal | 0 | Partial — needs PortalTarget |

### Fix: Branch on fiber.Tag

```csharp
public static FiberNode CloneForReuse(FiberNode current, VirtualNode newVNode)
{
    if (current == null) return null;

    var clone = current.Alternate ?? new FiberNode();

    // === Common fields (all fiber types) ===
    clone.Tag = current.Tag;
    clone.ElementType = current.ElementType;
    clone.Key = current.Key;
    clone.Props = current.Props;
    clone.PendingProps = newVNode != null ? ExtractProps(newVNode) : current.PendingProps;
    clone.Children = newVNode != null ? newVNode.Children : current.Children;
    clone.HostElement = current.HostElement;
    clone.Index = current.Index;
    clone.HasPendingStateUpdate = current.HasPendingStateUpdate;
    clone.SubtreeHasUpdates = current.SubtreeHasUpdates;

    // === Host-specific (HostComponent) ===
    clone.HostProps = current.HostProps;
    clone.PendingHostProps = newVNode?.HostProps ?? current.PendingHostProps;

    // === Function/ErrorBoundary/Portal-specific ===
    if (current.Tag != FiberTag.HostComponent)
    {
        clone.TypedRender = current.TypedRender;
        clone.TypedProps = current.TypedProps;
        clone.TypedPendingProps = newVNode?.TypedProps ?? current.TypedPendingProps;
        clone.ComponentState = current.ComponentState;
        clone.ContextFrame = current.ContextFrame;
        clone.ContextProviderId = current.ContextProviderId;
        clone.ProvidedContext = current.ProvidedContext;
        clone.ReadsContext = current.ReadsContext;
        clone.PortalTarget = current.PortalTarget;

        clone.ErrorBoundaryActive = current.ErrorBoundaryActive;
        clone.ErrorBoundaryShowingFallback = current.ErrorBoundaryShowingFallback;
        clone.ErrorBoundaryLastException = current.ErrorBoundaryLastException;
        clone.ErrorBoundaryResetKey = current.ErrorBoundaryResetKey;
        clone.ErrorBoundaryFallback = newVNode?.ErrorFallback ?? current.ErrorBoundaryFallback;
        clone.ErrorBoundaryHandler = newVNode?.ErrorHandler ?? current.ErrorBoundaryHandler;
        clone.ErrorBoundaryChildren = newVNode?.Children ?? current.ErrorBoundaryChildren;

#if UNITY_EDITOR
        clone.HmrPreviousRender = current.HmrPreviousRender;
#endif

        if (newVNode?.NodeType == VirtualNodeType.Suspense)
            clone.TypedPendingProps = FiberIntrinsicComponents.CreateSuspenseProps(newVNode);
    }
    else
    {
        // Host elements: these fields are always null/default — skip the write
        // when reusing an alternate that was also a host (common case).
        // Only clear them when the alternate was a different type (extremely rare).
        if (clone.Tag != FiberTag.HostComponent)
        {
            clone.TypedRender = null;
            clone.TypedProps = null;
            clone.TypedPendingProps = null;
            clone.ComponentState = null;
            clone.ContextFrame = default;
            clone.ContextProviderId = 0;
            clone.ProvidedContext = null;
            clone.ReadsContext = false;
            clone.PortalTarget = null;
            clone.ErrorBoundaryActive = false;
            clone.ErrorBoundaryShowingFallback = false;
            clone.ErrorBoundaryLastException = null;
            clone.ErrorBoundaryResetKey = null;
            clone.ErrorBoundaryFallback = null;
            clone.ErrorBoundaryHandler = null;
            clone.ErrorBoundaryChildren = null;
#if UNITY_EDITOR
            clone.HmrPreviousRender = null;
#endif
        }
    }

    // === Reset tree structure ===
    clone.Child = null;
    clone.Sibling = null;
    clone.Parent = null;
    clone.EffectTag = EffectFlags.None;
    clone.NextEffect = null;
    clone.Deletions = null;
    clone.LayoutEffects = null;
    clone.PassiveEffects = null;

    // === Alternate chain ===
    clone.Alternate = current;
    current.Alternate = clone;

    VirtualNode.__ScheduleReturn(newVNode);
    return clone;
}
```

### Files Changed

| File | Change |
|------|--------|
| `Shared/Core/Fiber/FiberFactory.cs` | Branch `CloneForReuse` on `current.Tag` |

### Ecosystem Impact

- **Source Generator:** None — doesn't touch FiberFactory
- **HMR:** `HmrPreviousRender` is only set on FunctionComponent fibers. The branch correctly skips it for HostComponent. Safe.
- **IDE Extensions:** None
- **Unity:** None
- **Context providers:** Context fields (`ContextFrame`, `ContextProviderId`, `ProvidedContext`) are only relevant for FunctionComponent and ErrorBoundary. Host elements don't provide/read context. Safe.
- **Bailout path:** `CloneChildrenForBailout` calls `CloneForReuse(current, null)` — the null VNode path works the same for both branches.

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| Host element with all default fields | None | The fast path skips writing them. If the alternate was also a HostComponent, these fields are already null/default. |
| Fiber type changes (HostComponent → FunctionComponent) | **Extremely rare** | Can only happen if a key maps to a different element type between renders. The reconciler normally deletes+creates in this case, not clone. But if it somehow happens, the `clone.Tag != FiberTag.HostComponent` guard clears the stale fields. |
| New FiberNode fields added in future | **Low** | Developer must remember to add the new field to both branches. Mitigate with a comment. |
| `CreateNew` also sets fields | None | `CreateNew` creates from scratch — unaffected. |

### Expected Gain

- **CPU:** Eliminates ~45,000 field writes per frame (15 fields × 3,000 host fibers)
- **Estimated:** ~0.1-0.3ms (field writes are cheap on their own, but Mono's memory write barriers add overhead)

---

## OPT-3: __C() Arity Overloads

### Problem

Every parent element with children calls `__C(...)`, which:
1. Allocates `params object[]` at the call site (compiler boxes VirtualNode references)
2. Creates `new List<VirtualNode>()` inside `__C`
3. Iterates all items, type-checking each with `is VirtualNode` / `is IEnumerable<VirtualNode>`
4. Calls `list.ToArray()` — allocates final `VirtualNode[]`

For the stress test root element with 3,000 children, this is the `@foreach` path which uses `IEnumerable<VirtualNode>`. But for typical static parents (1-3 children), all 4 steps are waste.

### Current `__C` Implementation

```csharp
private static VirtualNode[] __C(params object[] items)
{
    var list = new List<VirtualNode>();       // alloc 1
    foreach (var __ci in items)              // alloc: params object[] at call site
    {
        if (__ci is VirtualNode __vn) { if (__vn != null) list.Add(__vn); }
        else if (__ci is IEnumerable<VirtualNode> __seq)
            foreach (var __sn in __seq) { if (__sn != null) list.Add(__sn); }
    }
    return list.ToArray();                   // alloc 2
}
```

### Call Sites in Stress Test

The stress test component has approximately:
- 1 root `<VisualElement>` with 4-5 static children → `__C(child1, child2, child3, child4, child5)`
- A few containers with 1-3 children → `__C(child1)`, `__C(child1, child2)`
- 1 `@foreach` producing `IEnumerable<VirtualNode>` → `__C(foreachResult)` (single arg, but type is enumerable)

### Fix: Add Typed Overloads

Add overloads for 1, 2, and 3 children. These avoid the `params object[]` allocation, the `List<VirtualNode>`, and the `ToArray()`:

```csharp
// 1-child overload (also handles @foreach returning VirtualNode[])
private static VirtualNode[] __C(object item)
{
    if (item is VirtualNode vn)
        return vn != null ? new[] { vn } : System.Array.Empty<VirtualNode>();
    if (item is IEnumerable<VirtualNode> seq)
    {
        var list = new List<VirtualNode>();
        foreach (var sn in seq) { if (sn != null) list.Add(sn); }
        return list.ToArray();
    }
    return System.Array.Empty<VirtualNode>();
}

// 2-child overload
private static VirtualNode[] __C(object a, object b)
{
    // Fast path: both are VirtualNode (90%+ of cases)
    if (a is VirtualNode va && b is VirtualNode vb)
    {
        if (va != null && vb != null) return new[] { va, vb };
        if (va != null) return new[] { va };
        if (vb != null) return new[] { vb };
        return System.Array.Empty<VirtualNode>();
    }
    // Fallback: one or both are sequences
    var list = new List<VirtualNode>();
    __AddItem(list, a);
    __AddItem(list, b);
    return list.Count > 0 ? list.ToArray() : System.Array.Empty<VirtualNode>();
}

// 3-child overload (similar pattern)
private static VirtualNode[] __C(object a, object b, object c)
{
    if (a is VirtualNode va && b is VirtualNode vb && c is VirtualNode vc)
    {
        // ... null-filter and build array ...
    }
    // Fallback
    var list = new List<VirtualNode>();
    __AddItem(list, a);
    __AddItem(list, b);
    __AddItem(list, c);
    return list.Count > 0 ? list.ToArray() : System.Array.Empty<VirtualNode>();
}

private static void __AddItem(List<VirtualNode> list, object item)
{
    if (item is VirtualNode vn) { if (vn != null) list.Add(vn); }
    else if (item is IEnumerable<VirtualNode> seq)
        foreach (var sn in seq) { if (sn != null) list.Add(sn); }
}
```

### Source Generator Changes

The source gen's `EmitHelperMethod()` must emit the new overloads alongside the existing `params` version. The call sites don't change — C# overload resolution automatically picks the best match.

### HMR Emitter Changes

The HMR emitter (`HmrCSharpEmitter.cs`) also emits `__C`. It must emit the same overloads.

### Files Changed

| File | Change |
|------|--------|
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Emit additional __C overloads in `EmitHelperMethod()` |
| `Editor/HMR/HmrCSharpEmitter.cs` | Emit same __C overloads |

### Ecosystem Impact

- **Source Generator:** Modified — emits new overloads. No behavioral change for call sites (overload resolution is automatic).
- **HMR:** Modified — must emit matching overloads or the HMR-generated code won't compile.
- **IDE Extensions (LSP/VirtualDoc):** The language-lib generates VirtualDocuments for `.uitkx` files. If it emits `__C` calls, it must have matching overloads. **Need to verify.**
- **Unity:** None — purely generated code
- **Tests:** Source gen snapshot tests will need updating (new helper method signature). The 1027 passing tests should still pass — the overloads are additive.

### LSP VirtualDoc Check

The LSP's `VirtualDocumentGenerator` generates in-memory C# for intellisense. It likely emits `__C` calls. If the VirtualDoc includes the `__C` helper, it needs the overloads too. If it only imports/references the component class (which has __C as a private method), it's fine.

Let me check: does the VirtualDoc generator emit its own `__C` method or reference the component's?

Based on prior research: The LSP VirtualDoc generates a standalone C# file for each `.uitkx`. It must include its own `__C` helper since the real generated class doesn't exist yet during editing. So **YES**, the VirtualDoc generator must also emit the overloads.

### Files Changed (Revised)

| File | Change |
|------|--------|
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Emit __C overloads in `EmitHelperMethod()` |
| `Editor/HMR/HmrCSharpEmitter.cs` | Emit same __C overloads |
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Emit same __C overloads (if it emits its own `__C`) |

### What Could Break

| Scenario | Risk | Analysis |
|----------|------|----------|
| @foreach with 1 result sequence | None | `__C(foreachResult)` matches the 1-arg overload. The `IEnumerable<VirtualNode>` branch handles it. |
| @if without @else (null VirtualNode) | None | Null is `object`, matches `__C(object)`. The `is VirtualNode` check handles null correctly. |
| Mixed children (VirtualNode + IEnumerable) | None | Falls through to `List<VirtualNode>` path in the overload. |
| 4+ children | None | Falls through to `params object[]` version. No regression. |
| HMR without overloads | **HIGH** | HMR-compiled code calls `__C(a, b)` which would match `params` version if no 2-arg overload exists. Functionally correct but no perf gain. Must keep HMR in sync. |
| LSP without overloads | **MEDIUM** | Intellisense might show type errors if the VirtualDoc has __C calls that don't match. Must keep in sync. |

### Expected Gain

For the stress test specifically, `__C` is called ~6 times per frame (a few static parent elements). The savings are:
- 6 × (avoid `params object[]` alloc + avoid `List<VirtualNode>` alloc + avoid `ToArray()`) = ~18 allocations eliminated
- Total bytes: ~500 bytes per frame (negligible)
- CPU: ~0.01ms

**For real apps:** Many more static parent elements (50-200), so savings scale linearly. Still small in absolute terms.

**Verdict:** LOW impact for stress test. Moderate for real apps. Low risk. Worth doing as cleanup but not a priority.

---

## Impact Analysis

### Combined Savings (Stress Test: 3,000 boxes, all moving every frame)

| Optimization | Type | Savings/Frame | Confidence |
|-------------|------|---------------|------------|
| **OPT-23A**: Remove StyleEquals from ShallowEquals | CPU | ~0.2-0.3ms | HIGH |
| **OPT-23B**: _hasEvents guard in ShallowEquals | CPU | ~0.1-0.2ms | HIGH |
| **OPT-25**: Pool MapRemainingChildren dict | Alloc + CPU | ~0.2-0.5ms | MEDIUM |
| **OPT-26**: Skip CommitDeletions walk (boolean flag) | CPU | ~0.2-0.3ms | HIGH |
| **OPT-24**: Host-only CloneForReuse | CPU | ~0.1-0.3ms | LOW |
| **OPT-3**: __C() arity overloads | Alloc | ~0.01ms | LOW |
| **TOTAL** | | **~0.8-1.6ms** | |

At 37 FPS (27ms/frame), saving ~1-1.5ms would yield **~38-39 FPS**.

### Combined Allocation Savings

| Optimization | Allocs Eliminated | Bytes Saved/Frame |
|-------------|-------------------|-------------------|
| OPT-23 | 0 (CPU only) | 0 |
| OPT-25 | 1 Dictionary (3,000 entries) | ~100-200 KB |
| OPT-26 | 0 (CPU only) | 0 |
| OPT-24 | 0 (CPU only) | 0 |
| OPT-3 | ~18 small objects | ~500 bytes |
| **Total** | **~19 objects** | **~100-200 KB** |

---

## Risk Assessment

### What These Changes DO NOT Touch

- ❌ Any public API
- ❌ VirtualNode / Style / BaseProps pool lifecycle
- ❌ PropsApplier (except removing redundant comparison in OPT-23)
- ❌ Element adapters
- ❌ Rider / Visual Studio extensions

### What These Changes DO Touch

| File | Changed By | Risk |
|------|-----------|------|
| `Shared/Props/Typed/BaseProps.cs` | OPT-23 | Low — simplifying ShallowEquals |
| `Shared/Core/Fiber/FiberFactory.cs` | OPT-24 | Low — branching on Tag |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | OPT-25, OPT-26 | **Medium** — dict pool (failed before), deletion flag |
| `Shared/Core/Fiber/FiberReconciler.cs` | OPT-26 | Low — guard around CommitDeletions |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | OPT-3 | Low — additive overloads |
| `Editor/HMR/HmrCSharpEmitter.cs` | OPT-3 | Low — additive overloads |
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | OPT-3 | Low — additive overloads (if needed) |

### Highest Risk Item

**OPT-25 (dict pool)** — this is a retry of OPT-4 which failed previously. The plan is mechanically sound but the previous attempt had undiscovered issues. **Must implement carefully and test thoroughly.**

---

## Implementation Order

Ordered by impact/risk ratio:

| Order | Item | Why |
|-------|------|-----|
| 1 | **OPT-23** (both parts) | Highest savings (~0.3-0.5ms), lowest risk (simplifies existing code), no external file changes |
| 2 | **OPT-26** (deletion skip flag) | High savings (~0.2-0.3ms), very simple (1 flag + 1 guard), no external file changes |
| 3 | **OPT-24** (host CloneForReuse) | Medium savings (~0.1-0.3ms), medium complexity (branching), no external file changes |
| 4 | **OPT-25** (dict pool) | High savings (~0.2-0.5ms) but **previously failed** — do last, test carefully |
| 5 | **OPT-3** (__C overloads) | Lowest impact for stress test, requires source gen + HMR + LSP changes — most files touched for least gain |

**Recommendation:** Implement 1-4 first. OPT-3 can be deferred to a future session.

---

## Appendix: Code References

### BaseProps.cs — Key Locations

| Method/Field | Line | Touched By |
|--------|------|------------|
| `ShallowEquals` | ~570 | OPT-23 |
| `StyleEquals` | ~724 | OPT-23 (removed call) |
| `_hasEvents` | ~55 | OPT-23 (guard in ShallowEquals) |

### FiberFactory.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `CloneForReuse` | ~105 | OPT-24 |
| `CloneChildrenForBailout` | ~195 | (calls CloneForReuse) |

### FiberReconciler.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `CompleteWork` | ~516 | OPT-23 (calls ShallowEquals) |
| `CommitRoot` | ~645 | OPT-26 (CommitDeletions guard) |
| `CommitDeletions` | ~770 | OPT-26 (guarded) |

### FiberChildReconciliation.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `MapRemainingChildren` | ~466 | OPT-25 |
| `DeleteChild` | ~441 | OPT-26 (sets flag) |

### CSharpEmitter.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `EmitHelperMethod` | ~585 | OPT-3 |

### HmrCSharpEmitter.cs — Key Locations

| Method | Line | Touched By |
|--------|------|------------|
| `__C` emit | ~1199 | OPT-3 |
