# UITKX Optimization Plan V2

> **Status:** Active — accepting new items
> **Started:** April 26, 2026
> **Predecessor:** `Plans~/archive/OPTIMIZATION_PLAN.md` (closed April 25, 2026)
> **Scope:** Core library (`Shared/`), Runtime (`Runtime/`), Source Generator (`SourceGenerator~/`)

This plan contains optimization items discovered after the V1 optimization effort
concluded. Each item lists status, motivation, design, expected impact, risk, and
validation plan.

---

## Item Index

| ID | Item | Status | Estimated Impact |
|----|------|--------|------------------|
| OPT-V2-1 | VNode Children List Pooling | Proposed | 2,000–4,000 list allocs/frame |
| OPT-V2-2 | Style Sparse Storage | Proposed | ~5–10× per-element memory reduction; faster diff |
| OPT-V2-3 | (Deferred) Recover render-phase pool returns via commit-phase walk | Deferred | ~96 KB/sec GC savings (showcases) — only worthwhile if OPT-V2-2 is NOT done first |

---

## OPT-V2-1: VNode Children List Pooling

### Status: Proposed

### Summary

Pool the `List<VirtualNode>` children lists used inside VNodes rather than pooling
VNode objects themselves. This recovers alloc savings from the reverted OPT-18 without
the dangling-reference bugs that made VNode object pooling unsafe.

### Background

OPT-18 pooled VNode objects directly — renting from a static pool and returning them
after fiber extraction. This saved ~6,200 allocs/frame but was **fundamentally unsafe**:
VNode references appear not just in `_children` but also inside opaque `IProps` (e.g.
`Route.Element`, any slot-like pattern). When a pooled VNode was reset and re-rented,
any external reference became a dangling pointer to corrupted data, causing:

- Router demo infinite loop (Route's `element` prop pointing to a recycled VNode)
- Game demos broken (corrupted VNode types during reconciliation)
- Style corruption (wrong BaseProps applied to wrong elements)

OPT-18 was fully reverted. VNodes are now regular GC'd heap objects.

### Proposed Approach

#### Core Insight

The expensive allocations are the `List<VirtualNode>` objects, not the VNode objects
themselves. VNodes are small gen0 garbage (~48 bytes each). But `List<VirtualNode>`
allocates a backing array (typically 4–8 elements) plus the list wrapper. In a tree
with many parent elements, these lists dominate alloc volume.

#### Design

1. **Pool `List<VirtualNode>` instances** in a static `Stack<List<VirtualNode>>`.
2. **Rent** in `VirtualNode.__C(...)` (the children-array helper called by generated code).
3. **Return** after the reconciler extracts children into fiber child pointers
   (in `FiberFactory.CreateNew` / `CloneForReuse` / `FiberChildReconciliation.CreateFiber`).
4. On return, call `list.Clear()` (resets count, keeps backing array for reuse).

#### Why This Is Safe

- No external code holds references to the children **list** object — it's an internal
  transport container consumed immediately by the reconciler.
- The VNode objects inside the list remain alive (not pooled), so any reference to them
  (in IProps, closures, etc.) stays valid.
- `ReferenceEquals` checks on VNode objects still work (no recycling).
- The list is only used during the render phase (between `__C()` and fiber extraction).
  After extraction, the list is empty and returned to pool.

### Implementation Steps

1. Add pool infrastructure to VirtualNode:
   ```csharp
   private static readonly Stack<List<VirtualNode>> s_childListPool = new(256);

   internal static List<VirtualNode> __RentChildList(int capacity)
   {
       if (s_childListPool.Count > 0)
           return s_childListPool.Pop(); // already Clear()'d on return
       return new List<VirtualNode>(capacity);
   }

   internal static void __ReturnChildList(List<VirtualNode> list)
   {
       if (list == null) return;
       list.Clear();
       if (s_childListPool.Count < 1024)
           s_childListPool.Push(list);
   }
   ```

2. Update `VirtualNode.__C(...)` to rent from pool instead of `new List<VirtualNode>`.

3. Update `FiberFactory.CreateNew`, `CloneForReuse`, and
   `FiberChildReconciliation.CreateFiber` to return the children list to pool after
   extracting child info into fibers.

4. Ensure `CommitPropsAndClearFlags` does NOT return the list (fiber.Children stays alive
   for re-render — only the reconciler consumes and returns lists from fresh VNodes).

### Expected Impact

- **Alloc savings:** Proportional to the number of parent elements rendered per frame.
  Likely 2,000–4,000 list allocs/frame eliminated.
- **No correctness risk:** Lists are internal transport; VNode objects are not recycled.
- **Minimal code change:** ~3 files (VNode.cs, FiberFactory.cs, FiberChildReconciliation.cs).

### Risks / Considerations

- Must verify that no code path holds a reference to the children list after fiber
  extraction. If any path reads `fiber.Children` after the list was returned, it would
  see an empty list. The current code only reads `fiber.Children` during
  `UpdateHostComponent` / `UpdateFragment` / `UpdatePortal` (before extraction).
- Fragment fibers read `fiber.Children` in `UpdateFragment` — these must NOT have their
  children list returned until after the fragment is processed.
- Function component `Children` (slot content) must NOT be returned — these are
  preserved across render cycles for the bailout comparison.

### Validation Plan

1. Run all 1029 source gen tests.
2. Manual regression: Router demo, Snake game, Galaga, Mario, ShowcaseAll, Context demo.
3. Profiler: compare alloc counts before/after with stress test.

---

## OPT-V2-2: Style Sparse Storage

### Status: Proposed

### Summary

Replace the dense `Style` struct (80+ nullable fields, ~600 bytes per instance) with
a sparse representation that only stores set properties. Most styles set 5–15
properties — current layout wastes ~80% of the memory.

### Background

`Style` is the dominant per-element memory footprint in UITKX. Every host element
that opts into typed styling rents a pooled `Style` from `Style.__Rent()`. The struct
has fields for every USS property the framework supports (`StyleLength` for sizes,
`StyleColor` for colors, `StyleEnum<T>` for enums, etc.) — most are `default` (unset)
at any given time. The bitmask `_setBits0` / `_setBits1` already tracks which fields
are set, but the dense field layout means even a one-property style allocates the
full ~600 bytes.

The Style hot path is also impacted:

- `ShallowEquals` compares all fields conditionally on the bitmask, but cache lines
  are wasted because adjacent fields are usually unset.
- `ApplyTypedDiff` walks every potential property — sparse storage lets diff iterate
  only the bits that are set.

### Proposed Approach (B1 — Inline arrays + bitmask)

Two parallel arrays (or one tagged-object array), indexed via popcount over the
existing `_setBits0` / `_setBits1` bitmask:

```csharp
// Per-instance storage replaces the 80+ field declarations:
private object[] _refValues;                // for ref-typed style props (Background, Font, etc.)
private StyleLength[] _lengthValues;        // for StyleLength-typed props
// + small inline buffer for the most common 4–6 props (avoids array alloc for
//   trivial styles like { width, height, backgroundColor })
```

Index lookup:
```csharp
private static int IndexFromBit(ulong bits0, ulong bits1, int bitIndex)
{
    // popcount of all set bits below bitIndex
    if (bitIndex < 64)
        return PopCount(bits0 & ((1UL << bitIndex) - 1));
    return PopCount(bits0) + PopCount(bits1 & ((1UL << (bitIndex - 64)) - 1));
}
```

Setters/getters update the bit, then read/write the corresponding slot at the
computed index. Most operations remain O(1) because PopCount is a single hardware
instruction (`Popcnt.X64.PopCount` available since .NET Core 3.0; on Mono fall back
to a software popcount table or `BitOperations.PopCount`).

#### Alternatives Considered

- **B2 — Dictionary-backed:** `Dictionary<int, object>` keyed by bit index. Too much
  per-instance overhead (~48 bytes minimum + boxing structs).
- **B3 — Generated codegen per common-shape:** Source generator emits `StyleSet1<T1>`,
  `StyleSet2<T1,T2>` etc. for the most common combos. Most efficient but very high
  implementation cost. Defer indefinitely.

### Expected Impact

| Metric | Before | After (B1, typical 5-prop style) |
|--------|--------|-----------------------------------|
| Per-`Style` size | ~600 bytes | ~80–120 bytes |
| Empty `Style` size | ~600 bytes | ~40 bytes |
| `ShallowEquals` ops | ~80 conditional comparisons | popcount + N value comparisons (N = bits set) |
| `ApplyTypedDiff` walk | All properties scanned | Only set bits scanned |

For a typical app with ~150 host elements averaging ~10 set props each:
- Memory: ~90 KB before → ~15–18 KB after. **~5–6× reduction.**
- Diff CPU: proportional reduction in the hot path.

### Risks / Considerations

- `Style` is a large API surface — every property accessor needs to be regenerated by
  the source generator (`Style.cs` is partially generated; the typed setters live in
  `BaseProps`-derived classes via `_setBits` / `_setX(value)` patterns).
- `StyleLength` and similar value-type props can't go in `object[]` without boxing.
  Either a separate `StyleLength[]` array (proposed) or a tagged-union struct.
- The bitmask infrastructure exists; this change keeps the bitmask as the source of
  truth and changes only the value storage.
- The pool path in `Style.__Rent()` resets the value arrays (or just clears the
  bitmask if values are read lazily through the bitmask check).
- `ApplyTypedDiff` in `TypedPropsApplier.cs` needs to be updated to iterate set bits
  (e.g. via `BitOperations.TrailingZeroCount` over `_setBits0 ^ otherBits0`).

### Implementation Steps (high level)

1. Profile baseline with `BenchEditorHost.cs` — capture alloc counts and
   `ApplyTypedDiff` CPU time for the showcase.
2. Add the storage arrays + small-inline buffer to `Style`.
3. Update the source generator that emits `Style` accessors and the `_setX(value)` /
   `SetByKey` methods to write through the new storage layout.
4. Update `Style.__Rent()` / `__ResetFields()` to clear the new storage.
5. Update `ShallowEquals` and `ApplyTypedDiff` to walk via popcount-iteration over
   the bitmask instead of conditional field-by-field.
6. Run full test suite + every showcase demo, including stress tests.
7. Re-profile, confirm wins, document the new baseline.

### Validation Plan

1. All 1029 source gen tests pass.
2. All showcase demos visually identical to current behavior.
3. `BenchEditorHost.cs` reports lower alloc count and lower `ApplyTypedDiff` time.
4. Memory profiler shows reduced per-Style heap size.

### Effort Estimate

2–4 days. The accessor regeneration in the source generator is the bulk of the work.
The reconciler and pool are not touched.

---

## OPT-V2-3 (Deferred): Recover Render-Phase Pool Returns via Commit-Phase Walk

### Status: Deferred (only revisit if OPT-V2-2 is NOT done)

### Background — The "Lighter OPT-18" Fix We Just Shipped

On April 26, 2026, we discovered that `FiberReconciler.CompleteWork`'s
"props equal but different instance" branch was returning `PendingHostProps` to the
pool **during the render phase**. The source `VirtualNode` (`vnode._hostProps`) still
held a live reference to that BaseProps. If the render was interrupted (passive
effect or `setState` during render) and restarted, the same VNode reference was
re-encountered and the same `BaseProps` was rescheduled — a double-return — and
worse, if it had already been re-rented elsewhere, two fibers ended up sharing one
mutable instance. This caused the "disco" symptom (cross-wired styles between
unrelated elements every render tick).

This was the same class of bug as the original OPT-18 — render-phase pool returns
where external references still exist — just at the `BaseProps`/`Style` level
instead of the `VirtualNode` level. We reverted the unsafe branch.

#### Production Fix Shipped

- Removed the pool returns from `CompleteWork`'s branch 3 (props equal, different
  instance). The fiber still aliases `PendingHostProps = HostProps` so the commit is
  a no-op. The unused rented `BaseProps` becomes garbage when its owning function
  component eventually re-renders.
- Pool returns now only happen during the commit phase from `CommitUpdate` (props
  actually changed → old replaced) and `CommitDeletion` (element removed). Both are
  atomic under `_isCommitting = true` so reentrancy can't cause double-returns.
- Defense in depth: added `_isPendingReturn` bool to `BaseProps` and `Style`.
  `__ScheduleReturn` is now idempotent — a second call on the same instance within
  one flush window is a no-op. Cleared on `Rent` and on `__FlushReturns`.

#### Cost of the Fix

- **One `BaseProps` + one `Style` allocation per unchanged host element per render**
  that the parent re-rendered (i.e. parent didn't bail out, but this child's typed
  props happened to deep-equal the previous ones) now becomes GC garbage instead of
  returning to the pool.
- Estimated ~96 KB/sec dropped to gen0 GC in the showcase with the timer ticking,
  ~150 host elements where ~120 don't change.
- Negligible in editor; potentially noticeable in IL2CPP / mobile.

### Recovery Plan (only if needed)

The proper way to recover this savings is to do the pool return in the **commit
phase**, not the render phase:

1. Add a third slot `FiberNode.UnusedRentedProps` (and `UnusedRentedStyle`).
2. In `CompleteWork`'s branch 3, instead of scheduling the return, write the unused
   instance to the new slot.
3. In `CommitPropsAndClearFlags` (already a tree walk), after committing each fiber,
   schedule the contents of `UnusedRentedProps` for return to the pool. At this
   point the new tree is the authoritative one, the WIP tree is being discarded,
   and the unused instance is provably no longer referenced by any live fiber.
   Clear the slot.
4. Tests must specifically cover the render-restart scenario (passive effect or
   `setState` mid-render) to prove the new path doesn't regress to the disco bug.

### Why This Is Deferred

- If **OPT-V2-2 (Style Sparse Storage)** ships first, the per-element memory cost
  drops by ~5×. The leaked `Style` becomes ~80–120 bytes instead of ~600. The
  motivation for recovering the pool optimization largely disappears.
- New `FiberNode` field costs 8 bytes per fiber (~thousands of fibers in a real
  app) — partially offsets the savings.
- Risk of reintroducing the same class of bug we just spent a day debugging.

### Trigger to Revisit

Revisit only if:
- OPT-V2-2 is rejected or deferred indefinitely, AND
- Profiling on IL2CPP / mobile shows GC pressure from this specific path is causing
  measurable frame hitches.

---

## Notes

- All items here are **gated by profiling**. Do not implement on speculation.
  Capture baseline numbers from `BenchEditorHost.cs` before and after each change,
  and document the actual delta in this file.
- Lessons from V1 still apply:
  - Don't pool objects that have external references (OPT-18 lesson, reinforced by
    the April 26 BaseProps fix).
  - Reverting an unsafe optimization is cheaper than living with a bug.
  - Idempotent guards (`_isPendingReturn`) are cheap defense-in-depth — add them
    proactively to any new pool you build.
# OPT: VNode Children List Pooling

## Status: Proposed

## Summary

Pool the `List<VirtualNode>` children lists used inside VNodes rather than pooling
VNode objects themselves. This recovers alloc savings from the reverted OPT-18 without
the dangling-reference bugs that made VNode object pooling unsafe.

## Background

OPT-18 pooled VNode objects directly — renting from a static pool and returning them
after fiber extraction. This saved ~6,200 allocs/frame but was **fundamentally unsafe**:
VNode references appear not just in `_children` but also inside opaque `IProps` (e.g.
`Route.Element`, any slot-like pattern). When a pooled VNode was reset and re-rented,
any external reference became a dangling pointer to corrupted data, causing:

- Router demo infinite loop (Route's `element` prop pointing to a recycled VNode)
- Game demos broken (corrupted VNode types during reconciliation)
- Style corruption (wrong BaseProps applied to wrong elements)

OPT-18 was fully reverted. VNodes are now regular GC'd heap objects.

## Proposed Approach

### Core Insight

The expensive allocations are the `List<VirtualNode>` objects, not the VNode objects
themselves. VNodes are small gen0 garbage (~48 bytes each). But `List<VirtualNode>`
allocates a backing array (typically 4–8 elements) plus the list wrapper. In a tree
with many parent elements, these lists dominate alloc volume.

### Design

1. **Pool `List<VirtualNode>` instances** in a static `Stack<List<VirtualNode>>`.
2. **Rent** in `VirtualNode.__C(...)` (the children-array helper called by generated code).
3. **Return** after the reconciler extracts children into fiber child pointers
   (in `FiberFactory.CreateNew` / `CloneForReuse` / `FiberChildReconciliation.CreateFiber`).
4. On return, call `list.Clear()` (resets count, keeps backing array for reuse).

### Why This Is Safe

- No external code holds references to the children **list** object — it's an internal
  transport container consumed immediately by the reconciler.
- The VNode objects inside the list remain alive (not pooled), so any reference to them
  (in IProps, closures, etc.) stays valid.
- `ReferenceEquals` checks on VNode objects still work (no recycling).
- The list is only used during the render phase (between `__C()` and fiber extraction).
  After extraction, the list is empty and returned to pool.

### Implementation Steps

1. Add pool infrastructure to VirtualNode:
   ```csharp
   private static readonly Stack<List<VirtualNode>> s_childListPool = new(256);

   internal static List<VirtualNode> __RentChildList(int capacity)
   {
       if (s_childListPool.Count > 0)
       {
           var list = s_childListPool.Pop();
           // list is already Clear()'d on return
           return list;
       }
       return new List<VirtualNode>(capacity);
   }

   internal static void __ReturnChildList(List<VirtualNode> list)
   {
       if (list == null) return;
       list.Clear();
       if (s_childListPool.Count < 1024)
           s_childListPool.Push(list);
   }
   ```

2. Update `VirtualNode.__C(...)` to rent from pool instead of `new List<VirtualNode>`.

3. Update `FiberFactory.CreateNew`, `CloneForReuse`, and `FiberChildReconciliation.CreateFiber`
   to return the children list to pool after extracting child info into fibers.

4. Ensure `CommitPropsAndClearFlags` does NOT return the list (fiber.Children stays alive
   for re-render — only the reconciler consumes and returns lists from fresh VNodes).

### Expected Impact

- **Alloc savings**: Proportional to the number of parent elements rendered per frame.
  Likely 2,000–4,000 list allocs/frame eliminated (the remainder of OPT-18's ~6,200 were
  the VNode objects themselves, which are cheap gen0 garbage).
- **No correctness risk**: Lists are internal transport; VNode objects are not recycled.
- **Minimal code change**: ~3 files (VNode.cs, FiberFactory.cs, FiberChildReconciliation.cs).

### Risks / Considerations

- Must verify that no code path holds a reference to the children list after fiber
  extraction. If any path reads `fiber.Children` after the list was returned, it would
  see an empty list. The current code only reads `fiber.Children` during
  `UpdateHostComponent` / `UpdateFragment` / `UpdatePortal` (before extraction).
- Fragment fibers read `fiber.Children` in `UpdateFragment` — these must NOT have their
  children list returned until after the fragment is processed.
- Function component `Children` (slot content) must NOT be returned — these are
  preserved across render cycles for the bailout comparison.

### Validation Plan

1. Run all 1029 source gen tests
2. Manual regression: Router demo, Snake game, Galaga, Mario, ShowcaseAll, Context demo
3. Profiler: compare alloc counts before/after with stress test
