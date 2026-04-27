# Tech Debt: Sample Apps & Runtime Issues

**Date:** April 25, 2026  
**Context:** Discovered after optimization pass (OPT-22/23B/24/25/26).

---

## Root Cause Analysis — COMPLETED

Two bugs were discovered and fixed:

### Bug 1: OPT-26 `_hasDeletions` flag — FIXED

**Root cause:** The `_hasDeletions` flag (OPT-26) was only set in the `ReconcileChildren` wrapper
method in `FiberReconciler.cs`, but deletions can be recorded by two additional code paths
that bypass that wrapper:

1. **`FiberFunctionComponent.RenderFunctionComponent()`** — When a function component returns
   `null` (e.g. Route with non-matching path) or a different child type (e.g. `@if/@else`
   switching between MainMenu and GameScreen), `DeleteChild()` adds to `wipFiber.Deletions`
   directly, bypassing the wrapper.

2. **`FiberFragment.UpdateFragment()`** — Calls `FiberChildReconciliation.ReconcileChildren()`
   directly instead of through the reconciler's wrapper method.

Because `_hasDeletions` was never set for these paths, `CommitRoot` skipped `CommitDeletions()`,
and old VisualElements remained in the DOM tree.

**Fix:** Added `_hasDeletions` check in `PerformUnitOfWork` after `BeginWork` returns — this is
the single universal point that covers every code path (function components, fragments, host
components, error boundaries, portals). See `FiberReconciler.cs` line ~381.

### Bug 2: OPT-18 `Children = null` clearing — FIXED

**Root cause:** `CommitPropsAndClearFlags` cleared `fiber.Children = null` after commit to
prevent stale VNode pool references. However, when a function component bails out
(`SubtreeHasUpdates` path) via `CloneChildrenForBailout` → `CloneForReuse(current, null)`,
the clone's Children is copied from `current.Children` — which is null. When a cloned
descendant re-renders (e.g. Router with `HasPendingStateUpdate`), it calls
`TypedRender(props, Children ?? EmptyChildren)` with no slot content → blank screen.

**Trace for Router navigation:**
1. `setLocation` → `ScheduleUpdateOnFiber(routerFiber)` → marks ancestors `SubtreeHasUpdates`
2. `CreateWorkInProgress(root, null)` → `Children = current.Children` = null (cleared by OPT-18)
3. Root `UpdateHostComponent` → `Children == null` → `CloneChildrenForBailout`
4. Parent component bails out → clones Router with `Children = null`
5. Router re-renders → `TypedRender(props, null ?? EmptyChildren)` → empty Fragment → blank screen

**Fix:** Instead of clearing `fiber.Children = null`, pin the children VNodes by setting
their `_generation = 0`. This prevents `__FlushReturns` from pooling them, so they remain
valid for the bailout path. When the fiber eventually re-renders and receives new children,
the old pinned VNodes become GC-eligible. `__FlushReturns` was updated to skip VNodes with
`_generation == 0`. See `FiberReconciler.cs CommitPropsAndClearFlags` and `VNode.cs __FlushReturns`.

**Note:** This bug existed since OPT-18 was first introduced but was never caught because
sample apps and games were not tested until after the full optimization pass.

**Affected issues:** TD-S2, TD-S3, TD-S4, TD-S5 (all required BOTH fixes).

---

## Issues

### TD-S1: useRef type mismatch in IDE (MarioGame)

**Status:** Open (pre-existing, low priority)  
**File:** `Samples/Components/MarioGame/components/GameScreen/GameScreen.uitkx`  
**Symptom:** IDE shows error on `useMarioGame(boardRef)`:
```
Argument 1: cannot convert from '__UitkxRef__<VisualElement?>' to 'Ref<VisualElement?>'
```
**Analysis:** The VirtualDocument generator (`VirtualDocumentGenerator.cs`) emits a local
`__UitkxRef__<T>` class for IDE intellisense, but the hook's parameter type is
`ReactiveUITK.Core.Ref<T>`. These are different types in the virtual document's Roslyn workspace.
The source generator emits the correct `Ref<T>` at build time — this is an **IDE-only** false positive.  
**Pre-existing:** Yes  
**Impact:** Red squiggly in IDE only. Compilation and runtime are unaffected.

---

### TD-S2: Router-based navigation broken — FIXED

**Status:** Fixed (Bug 1 + Bug 2)  
**Symptom:** Route changes not reflected in UI — old route children stay, new route is blank.  
**Root cause:** Bug 1 (OPT-26): deletions not committed. Bug 2 (OPT-18): Router's slot children
(Route elements) are null during bailout re-render → empty Fragment.  
**Affected samples:** SnakeGame, MainMenuRouterDemoFunc, any Router-based UI.

---

### TD-S3: GalagaGame doesn't work — FIXED

**Status:** Fixed (Bug 1 + Bug 2)  
**Symptom:** Galaga game broken — screens don't switch.  
**Root cause:** Bug 1: old screen's VisualElements not deleted. Bug 2: after `@if/@else` switch,
parent's slot children are null → new screen renders with empty content.

---

### TD-S4: MarioGame doesn't load assets properly — FIXED

**Status:** Fixed (Bug 1 + Bug 2)  
**Symptom:** Mario game appears to work but assets seem missing.  
**Root cause:** Same as TD-S3 — Bug 1 leaves old elements covering new content, Bug 2 causes
new content to render with missing slot children.
GameScreen. When switching, the old MainMenu's VisualElements were not deleted, covering
the GameScreen. The "missing assets" were actually the game elements being hidden behind
un-deleted old elements.

---

### TD-S5: SnakeGame button clicks don't work — FIXED

**Status:** Fixed (Bug 1 + Bug 2)  
**Symptom:** Clicking "Start New Game" does nothing.  
**Root cause:** Bug 1: old WelcomeScreen not deleted. Bug 2: Router slot children null after
navigation → new route renders blank.

---

### TD-S6: GalagaGame GameScreen.uitkx formatter idempotency failure

**Status:** Open (pre-existing, low priority)  
**File:** `Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx`  
**Symptom:** Formatter's second pass produces different output than the first, causing the
idempotency snapshot test to fail (2 tests affected).  
**Pre-existing:** Yes  
**Impact:** Test failure only. Runtime unaffected.

---

## Summary

| Issue | Status | Cause |
|-------|--------|-------|
| **TD-S2** Router broken | **FIXED** | Bug 1 (OPT-26) + Bug 2 (OPT-18) |
| **TD-S3** Galaga broken | **FIXED** | Bug 1 (OPT-26) + Bug 2 (OPT-18) |
| **TD-S4** Mario assets | **FIXED** | Bug 1 (OPT-26) + Bug 2 (OPT-18) |
| **TD-S5** Snake buttons | **FIXED** | Bug 1 (OPT-26) + Bug 2 (OPT-18) |
| **TD-S1** useRef IDE mismatch | Open | Pre-existing VirtualDoc limitation |
| **TD-S6** Formatter idempotency | Open | Pre-existing formatter bug |
