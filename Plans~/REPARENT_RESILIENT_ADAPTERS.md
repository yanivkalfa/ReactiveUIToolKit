# Reparent-Resilient Adapters & Hooks — Implementation Plan

## Summary

When `RootRenderer` reparents its mounted subtree to a new host root (Class 1
fix in [ROOT_RENDERER_HOST_REATTACH.md](ROOT_RENDERER_HOST_REATTACH.md)), or
when `<Portal target>` swaps to a new VE (Class 2 fix in
[USE_UI_DOCUMENT_ROOT_HOOK.md](USE_UI_DOCUMENT_ROOT_HOOK.md)), Unity dispatches
`DetachFromPanelEvent` and `AttachToPanelEvent` across the moved subtree.
Several pieces of our framework currently treat that event pair as
"resource has gone away forever," tearing down or stopping work that should
have continued. This plan inventories the affected pieces and adds a uniform
**defer-teardown** pattern so transient detach/attach (within ≤1 frame) does
not tear anything down.

Without this, the pair (Class 1 + Class 2) is **technically** correct but
**practically** unacceptable: clicking a UIDocument GameObject in playmode
makes videos restart every frame, animations freeze, and trackers thrash.

## Scope of impact (audit results)

### Confirmed safe (no change needed)

| Subsystem | Why safe |
|---|---|
| `<Audio>` Func-Component ([Shared/Core/Media/AudioFunc.cs](../Shared/Core/Media/AudioFunc.cs)) | Lifecycle is `UseEffect` — fires on fiber mount/unmount + deps change, **not** on panel attach/detach. AudioSource keeps playing across reparent. |
| All Hooks (`UseState`, `UseEffect`, `UseLayoutEffect`, `UseRef`, `UseSignal`, `UseSfx`, `UseContext`, `UseReducer`, `UseImperativeHandle`) | Tied to fiber lifecycle, not panel. |
| Signal subscriptions ([SignalsRuntime](../Shared/Core/Signals)) | Disposed only via `UseEffect` cleanup or fiber unmount. |
| Cell pools in `<ListView>` / `<MultiColumnListView>` / `<TreeView>` / `<MultiColumnTreeView>` | Cell mounts are children of the view; reparenting the view moves them too. Cell `VNodeHostRenderer._container` references the cell mount, which keeps identity. |
| User-code `onClick`, `onPointerDown`, all non-panel UI events | Registered on stable VEs; events fire normally regardless of panel parent. |
| Component instance state, props, refs | Fiber tree is unmodified; we only swap the host parent pointer chain. |

### Broken by reparent (the focus of this plan)

#### 1. `<Video>` adapter — full teardown on detach

[Shared/Elements/VideoElementAdapter.cs](../Shared/Elements/VideoElementAdapter.cs#L154):

```csharp
private void OnDetach(DetachFromPanelEvent _)
{
    _attached = false;
    _cachedOwnerWindow = null;
    Teardown();   // ← returns VideoPlayer + RT to pool, stops playback,
                  //   detaches controller, nulls everything
}
```

`Teardown()` returns the pooled `VideoPlayer` and `RenderTexture` to
`MediaHost`, stops playback, detaches `VideoController`, removes editor pump.
A subsequent `OnAttach` calls `Setup()` which rents fresh peers and calls
`Prepare()` (async — first frame is 100–300 ms away).

**Visible cost on a single host-swap:** video restarts from frame 0; black
RT for hundreds of milliseconds; controller ref briefly null.

**Visible cost on per-frame storm:** video never produces a frame at all
(`Prepare` never completes before next teardown); pure pool churn; possible
audible audio stutter via `VideoPlayer`'s direct audio output.

#### 2. `Animator` & `UseTweenFloat` — silent stop on detach

[Shared/Core/Animation/Animator.cs](../Shared/Core/Animation/Animator.cs#L117-L125):

```csharp
handle.item = ve.schedule.Execute(() =>
{
    if (ve.panel == null)
    {
        handle.Stop();   // ← terminates the schedule item; never resumed
        return;
    }
    ...
});
```

[Shared/Core/Hooks.cs](../Shared/Core/Hooks.cs#L1420-L1430):

```csharp
item = target.schedule.Execute(() =>
{
    if (target.panel == null)
    {
        item?.Pause();   // ← pauses; no on-attach hook resumes it
        item = null;
        return;
    }
    ...
});
```

`IVisualElementScheduledItem` is **panel-bound** by design — when the panel
goes away, the item stops ticking. Both call sites detect that and quietly
stop, with no path to resume on re-attach. Reparent therefore freezes any
running animation/tween permanently.

**Visible cost:** any in-flight animation or tween dies on first reparent.
Affects loading spinners, transitions, ripple effects, slides, anything
using `Hooks.UseAnimate`, `UseTweenFloat`, or `Animator`.

#### 3. List/Tree/Tab trackers — detach on every panel leave

`MultiColumnListView` ([adapter L1205](../Shared/Elements/MultiColumnListViewElementAdapter.cs#L1205)),
`MultiColumnTreeView` ([adapter L1214](../Shared/Elements/MultiColumnTreeViewElementAdapter.cs#L1214)),
`TabView` ([tracker L229](../Shared/Elements/Trackers/TabViewSelectionTracker.cs#L229))
each register a `DetachFromPanelEvent` handler that detaches their state
trackers (Adjustment / Sort / Layout / Scroll / Selection).

**Visible cost:** trackers detach on reparent. They re-attach on the next
props application via `EnsureXTracker(...)` — but only the next time props
flow. Sort/scroll position **may be lost**, and any in-progress drag
operation (column resize, header sort) is interrupted.

**Severity:** medium. Mostly self-healing on next render, but loses transient
state. Not visually catastrophic for typical lists.

#### 4. User-declared panel handlers (declarative props)

[PropsApplier.cs L2432, L2437](../Shared/Props/PropsApplier.cs#L2432) wires
`onAttachToPanel` / `onDetachFromPanel` props through the standard event
plumbing. If consumer code declares either, it fires spuriously on every
reparent.

**Visible cost:** consumer-defined. Documented as known consequence; cannot
fix without changing event semantics.

## Design

### Pattern: defer transient teardown

Pseudocode shape, applied to each affected subsystem:

```
on DetachFromPanelEvent:
    schedule a "still detached?" check for next frame  (panel-independent timer)
on AttachToPanelEvent:
    cancel any pending teardown
on the deferred check (executes only if not cancelled):
    if (panel is still null) actually-tear-down()
    else: no-op  (we re-attached in time)
```

This collapses a same-frame or next-frame detach+attach pair into a no-op,
while still cleaning up genuine unmounts (where re-attach never comes).

### The "panel-independent timer" problem

`ve.schedule.Execute(...)` cannot be used from the deferred check because
`ve` has `panel == null` at that moment, so `schedule` is dead. We need a
panel-independent timer:

| Source | Editor | Runtime |
|---|---|---|
| `EditorApplication.delayCall` | ✅ next editor tick | ❌ unavailable |
| `MediaHost.Instance.SubscribeTick` | ✅ already polled (`MediaHostTicker`) | ✅ same |
| `RenderScheduler.Instance` / `EditorRenderScheduler.Instance` | ✅ via existing scheduler | ✅ via existing scheduler |
| Static `MainThreadDispatcher` (new) | ✅ | ✅ |

Use **`MediaHost.SubscribeTick`** for the Video adapter (already required at
runtime; already wired in editor via `MediaHostTicker`). For
`Animator` / `UseTweenFloat`, use the existing `RenderScheduler` they already
depend on. Either way, we don't introduce a new singleton.

### Helper: `PanelDetachGuard`

Centralize the pattern so adapters don't reinvent it:

```csharp
// Shared/Core/PanelDetachGuard.cs (new)
public sealed class PanelDetachGuard
{
    private readonly VisualElement _ve;
    private readonly Action _doTeardown;
    private Action _cancelPending;

    public PanelDetachGuard(VisualElement ve, Action teardown)
    {
        _ve = ve;
        _doTeardown = teardown;
        ve.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        ve.RegisterCallback<AttachToPanelEvent>(OnAttach);
    }

    private void OnDetach(DetachFromPanelEvent _)
    {
        _cancelPending?.Invoke();
        // Schedule a check on the next main-thread tick via a panel-
        // independent timer. If we're still detached, run teardown.
        _cancelPending = MainThreadTimer.AfterOneFrame(() =>
        {
            _cancelPending = null;
            if (_ve.panel == null) _doTeardown();
        });
    }

    private void OnAttach(AttachToPanelEvent _)
    {
        _cancelPending?.Invoke();
        _cancelPending = null;
        // Caller's adapter is responsible for "re-setup if torn down"
        // (Video already does this in its OnAttach).
    }
}
```

`MainThreadTimer` is a thin facade over the appropriate timer source for
the current Unity context.

### Per-subsystem changes

#### Video adapter

Replace the immediate `Teardown()` in `OnDetach` with the guard:

```csharp
public VideoVisualElement()
{
    ...
    new PanelDetachGuard(this, deferredTeardown: () =>
    {
        _attached = false;
#if UNITY_EDITOR
        _cachedOwnerWindow = null;
#endif
        Teardown();
    });
    RegisterCallback<AttachToPanelEvent>(OnAttach);  // unchanged: re-Setup if torn down
}
```

`OnAttach` keeps its existing `if (_props != null) Setup();` — `Setup` no-ops
when `_vp != null` (already running), so a same-frame re-attach simply
preserves the live player.

#### Animator & UseTweenFloat

These require a more invasive change because the schedule item itself is
panel-bound. Two options:

**A. Reschedule on re-attach.** On detach, snapshot animation state (current
value, elapsed time, easing position). On re-attach, recreate the
`schedule.Execute` item from snapshot. Adds bookkeeping per running tween.

**B. Move to panel-independent scheduling.** Use `RenderScheduler.Instance`'s
tick (which is already a global, panel-independent main-thread pump for the
Fiber commit phase). Animations register a per-tick callback; advance state
based on `Time.realtimeSinceStartupAsDouble`; only call `target.style.X = …`
when `target.panel != null` (so a transient detach just suppresses the visual
update for one frame, doesn't kill the animation).

Recommend **(B)** — fewer moving parts, less per-tween state, fixes the issue
permanently for both reparent and any other "panel becomes null" scenario
(e.g. user toggles `display: none` ancestor in some Unity versions).

#### Trackers (`MultiColumnListView`, `MultiColumnTreeView`, `TabView`)

Wrap the existing detach handler with the same defer-teardown helper. Detach
work is comparatively cheap, but the cancellation on re-attach prevents
state thrash:

```csharp
view.RegisterCallback<DetachFromPanelEvent>(_ =>
{
    PendingTrackerDetach.Schedule(view, () =>
    {
        parts.AdjustmentTracker.Detach(view, parts);
        parts.SortTracker.Detach(view, parts);
        parts.LayoutTracker.Detach(view, parts);
        parts.ScrollTracker.Detach(view, parts);
    });
});
view.RegisterCallback<AttachToPanelEvent>(_ => PendingTrackerDetach.Cancel(view));
```

#### `onAttachToPanel` / `onDetachFromPanel` user events

Do **not** suppress these — they're consumer-declared, and consumers may
legitimately want to know about transient panel changes. Document the
behavior in the prop's API docs:

> Fires whenever the element's panel reference changes, including transient
> reparents triggered by host-panel rebuilds. Use `evt.originPanel` /
> `evt.destinationPanel` to disambiguate.

## Non-goals

- **Suppressing Unity's panel events.** No public API; would require
  reflection into UIElements internals. Out of scope.
- **Generalizing to a "panel-aware effect" hook.** Tempting (`UsePanelEffect`
  with `onTransientDetach: false`), but premature abstraction. Wait for the
  third call site before extracting.
- **Fixing animations launched outside the framework.** Consumer-launched
  `ve.schedule.Execute` items are still vulnerable; document.

## Performance impact

| Path | Cost without defer-teardown | Cost with defer-teardown |
|---|---|---|
| Single host-swap (undo, asset swap, enable/enable) | 1× full Video teardown+setup (≈100–300 ms black RT); animations dead | Zero — same-frame re-attach cancels deferred teardown |
| Per-frame storm (editor selection in playmode) | Continuous teardown+setup; never produces a frame | One deferred-teardown scheduled per frame, all cancelled by next frame's re-attach. Net cost: 1 extra `MediaHost.SubscribeTick` callback registration per frame per affected adapter. Cheap. |
| Genuine unmount (component returns null, parent removes child) | Immediate teardown | Same teardown, delayed by 1 frame. Acceptable; no leak (cleanup still runs). |
| Memory | None (handlers are cheap closures) | Tiny per affected element: one `Action` ref held while pending |

The defer-teardown adds at most 1 frame of delay to genuine unmounts. For
panel-bound resources (`VideoPlayer`, etc.) this is invisible because the
element is already gone from the panel; the resource is just held one extra
frame in the pool.

## Files to add / change

| File | Change |
|---|---|
| `Shared/Core/PanelDetachGuard.cs` | **New.** Reusable detach-defer helper. |
| `Shared/Core/MainThreadTimer.cs` | **New.** Panel-independent 1-frame timer (editor: `EditorApplication.delayCall`; runtime: `MediaHost.Instance.SubscribeTick` + flag). |
| `Shared/Elements/VideoElementAdapter.cs` | Replace immediate `OnDetach` teardown with `PanelDetachGuard`. |
| `Shared/Core/Animation/Animator.cs` | Migrate to `RenderScheduler`-driven ticking; suppress style writes when `panel == null` instead of stopping. |
| `Shared/Core/Hooks.cs` (`UseTweenFloat`) | Same migration. |
| `Shared/Elements/MultiColumnListViewElementAdapter.cs` | Wrap `EnsureDetachHook` body in defer-teardown. |
| `Shared/Elements/MultiColumnTreeViewElementAdapter.cs` | Same. |
| `Shared/Elements/Trackers/TabViewSelectionTracker.cs` | Same. |
| `Plans~/USE_UI_DOCUMENT_ROOT_HOOK.md` | Cross-reference this plan in "Risks". |
| `Plans~/ROOT_RENDERER_HOST_REATTACH.md` | Cross-reference this plan in "Risks"; update Class 1 fix to clarify it depends on this plan to be production-quality. |

No source-generator changes. No reconciler changes. No HMR changes.

## Acceptance criteria

1. **Single-shot host swap (Class 1):** `<Video>` keeps playing across the
   swap; the visible RT does not flicker; playback position is preserved
   (within ≤1 frame).
2. **Single-shot portal target swap (Class 2):** same as (1) for a `<Video>`
   inside a `<Portal target>`.
3. **Animations survive reparent:** `Hooks.UseAnimate` / `UseTweenFloat` /
   `Animator` continue advancing across a reparent.
4. **Editor selection storm:** repeated reparents do not tear down `<Video>`
   even once — verified by counting `MediaHost.RentVideoPlayer` calls during
   a 60-frame storm (expect 1, observe 1).
5. **Genuine unmount:** removing a `<Video>` from the tree (component
   returns null) still releases its `VideoPlayer` and `RenderTexture` within
   ≤1 frame. Verified via existing `FiberRenderer.Clear` leak tests.
6. **Trackers preserve sort/scroll across reparent:** verified by sorting
   a column, then triggering a host-panel rebuild, and confirming the sort
   indicator and scroll offset are preserved.
7. **No allocations on the steady-state hot path** (no reparent occurring):
   the new helpers do not allocate per-frame.

## Open questions

- **Animator state snapshot — needed?** If we go with option B
  (panel-independent scheduling), no snapshot is needed because the
  animation state (start time, current value) lives in the Animator's
  closure, not the schedule item. Confirmed by reading the Animator code:
  state is captured in the lambda, so as long as we keep ticking the lambda
  (and gate style writes), we're done.
- **What about Unity ≥ 2024 panel-rebuild behavior?** UIToolkit may evolve;
  the editor selection storm could go away or change shape. Plan as-is is
  forward-compatible because it doesn't depend on storm semantics — only on
  the general "transient detach/attach" pattern.
- **`<Video>` audio across reparent:** if `<Video>`'s audio is via
  `VideoPlayer.SetDirectAudioVolume` (not via `MediaHost.AudioSource`), the
  audio path is also panel-scoped (via the player). Defer-teardown preserves
  it the same way. Verified by code reading; double-check with manual repro.

## Related plans

- [USE_UI_DOCUMENT_ROOT_HOOK.md](USE_UI_DOCUMENT_ROOT_HOOK.md) — Class 2 hook.
- [ROOT_RENDERER_HOST_REATTACH.md](ROOT_RENDERER_HOST_REATTACH.md) — Class 1 framework fix.
- [VIDEO_AUDIO_ELEMENTS_PLAN.md](VIDEO_AUDIO_ELEMENTS_PLAN.md) — Video/Audio adapter background.
