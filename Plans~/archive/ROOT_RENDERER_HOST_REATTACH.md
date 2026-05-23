# `RootRenderer` Host-Panel Re-Attach — Implementation Plan

## Summary

Make `RootRenderer` survive its host `UIDocument`'s panel being rebuilt
(Unity replaces `rootVisualElement` with a new instance on undo, source-asset
swap, GameObject disable/enable, etc.). When the host panel swaps,
`RootRenderer` must **reparent** its already-mounted top-level VEs from the
dead root to the live one, **without** rebuilding the fiber tree — so component
state, `UseEffect` bookkeeping, signal subscriptions, and external resources
(`VideoPlayer`, `AudioSource`, etc.) all survive.

Pairs with [USE_UI_DOCUMENT_ROOT_HOOK.md](USE_UI_DOCUMENT_ROOT_HOOK.md), which
solves the analogous problem for `<Portal target>` (Class 2). This plan covers
**Class 1**: the host UIDocument that drives `RootRenderer` itself.

## Motivation

### Reproduction

`AppBootstrap.cs` (Pretty Ui) records:

```
[BASELINE] main root: id=-273937408
[PANEL SWAP] main root: was=-273937408 now=1193634304 (childCount=0)
```

The id changed; the new root's `childCount` is `0`. Meaning: `RootRenderer`
still has its mounted top-level VE parented to the **old** root, which is no
longer attached to any panel. The user sees the entire UI vanish.

Triggers reproduced:
- Undo/redo on the host UIDocument's GameObject (single one-shot event)
- *Editor selection storm* (per-frame, see "Known Unity quirks" in the
  companion plan) — a degenerate but real DX scenario.

### Why no user-land hook can fix this

For Class 2 (portal target), the component owning the `<Portal>` is hosted
*elsewhere*, so it can re-render and republish a new target via
`UseUiDocumentRoot`. For Class 1, the dying root **is** the host that the
component tree lives under. There is nothing inside the tree to re-render —
the tree itself is parented to a corpse. The fix must live in `RootRenderer`,
which is the only entity that:

1. Holds the original `UIDocument` reference (or could, see "Required API
   change")
2. Owns the cached host `VisualElement` and the fiber container reference
3. Can move the mounted top-level children to a new container without going
   through the reconciler (so no unmount/remount = no state loss)

### Required API change

Today `RootRenderer.Initialize(VisualElement uiRootElement, ...)` takes only
the VE, not the source `UIDocument`. Without the `UIDocument`, we cannot
subscribe to `panelChanged`. Two options:

**A. Add a `UIDocument` overload** (preferred):
```csharp
public void Initialize(UIDocument hostDoc, Action<HostContext> env = null);
```
Internally caches `hostDoc`, subscribes to `panelChanged`, and uses
`hostDoc.rootVisualElement` as the container. The existing
`Initialize(VisualElement, ...)` overload stays for non-UIDocument hosts
(EditorWindow, custom panel, tests).

**B. Add an opt-in extension method**:
```csharp
rootRenderer.Initialize(uiDoc.rootVisualElement);
rootRenderer.AttachToUIDocument(uiDoc); // wires panelChanged → reparent
```

Option **A** is cleaner — it's the path 99% of users want. **B** is reserved
in case we need a separation between "where we render" and "what we listen to."
Recommend A; mention B in follow-ups only if the need surfaces.

## Design

### Reparent operation (the core mechanic)

When `panelChanged` fires:

```csharp
private UIDocument _hostDoc;
private VisualElement _committedHost; // last VE we parented our subtree under

private void OnHostPanelChanged()
{
    if (_hostDoc == null) return;
    var nextHost = _hostDoc.rootVisualElement;

    // Reference dedupe — early return only when the new ref is identical
    // to what we last committed. Always propagate genuine changes, even
    // per-frame ones (editor selection storm, see "Known Unity quirks").
    if (ReferenceEquals(nextHost, _committedHost)) return;

    if (nextHost == null)
    {
        // Panel detached. Don't tear anything down — keep the fiber tree
        // alive in memory; we'll reparent on the next attach. The mounted
        // top-level VE is harmlessly orphaned until then.
        return;
    }

    ReparentMountedTreeTo(nextHost);
    _committedHost = nextHost;
}

private void ReparentMountedTreeTo(VisualElement nextHost)
{
    // Move physical children from old container to new — a single transfer
    // operation, no fiber rebuild, no prop re-application, no effect cycles.
    if (rootElement != null && rootElement != nextHost)
    {
        // VisualElement.Add() implicitly removes from previous parent.
        // Snapshot first because Add mutates rootElement.hierarchy during the loop.
        int n = rootElement.childCount;
        if (n > 0)
        {
            var snapshot = new VisualElement[n];
            for (int i = 0; i < n; i++) snapshot[i] = rootElement[i];
            for (int i = 0; i < n; i++) nextHost.Add(snapshot[i]);
        }
    }

    // Update every cached pointer that references the old container so
    // subsequent reconciler work appends to the right place.
    rootElement = nextHost;
    vnodeHostRenderer?.RetargetHost(nextHost);
}
```

### What `RetargetHost` must update

Reading the current code paths ([VNodeHostRenderer.cs](../Shared/Core/VNodeHostRenderer.cs),
[FiberRenderer.cs](../Shared/Core/Fiber/FiberRenderer.cs),
[FiberReconciler.cs](../Shared/Core/Fiber/FiberReconciler.cs),
[FiberRoot.cs](../Shared/Core/Fiber/FiberRoot.cs)), the host VE is cached in
**five** places for a single mount:

| Layer | Field | Role |
|---|---|---|
| `RootRenderer` | `rootElement` | Returned by `Initialize`, passed to `VNodeHostRenderer` ctor |
| `VNodeHostRenderer` | `hostElement` | Receives host props via `PropsApplier` |
| `FiberRenderer` | `_container` | `Clear()` target on unmount; passed to `CreateRoot` |
| `FiberRoot` | `ContainerElement` | Logical mount point for the fiber tree |
| Root `FiberNode` | `HostElement` | `Tag = HostComponent`, `ElementType = "root"` — what `CommitPlacement` walks up to find as the parent for top-level user fibers |

All five must be updated atomically. New internal API surface:

```csharp
// VNodeHostRenderer
internal void RetargetHost(VisualElement nextHost)
{
    hostElement = nextHost;        // private field becomes settable
    fiberRenderer.RetargetContainer(nextHost);
}

// FiberRenderer
internal void RetargetContainer(VisualElement next)
{
    _container = next;
    if (_root != null)
    {
        _root.ContainerElement = next;
        if (_root.Current != null)
            _root.Current.HostElement = next;   // root fiber
    }
}
```

No reconciler changes. No `WorkLoop` invocation. No effect cleanup. Just
five pointer writes after the children are physically moved.

### Host-prop preservation

`VNodeHostRenderer.lastHostProps` records props applied to the **previous**
host VE via `PropsApplier`. After retargeting, those props are NOT on the new
host. Two options:

1. **Re-apply on retarget** — call `PropsApplier.Apply(nextHost, lastHostProps)`
   right after the pointer swap. Simple, idempotent, correct.
2. **Diff and reset** — call `PropsApplier.ApplyDiff(oldHost, lastHostProps,
   EmptyProps)` to clean up the dead host (it's orphaned anyway, so likely
   moot), then `Apply(nextHost, lastHostProps)`.

Go with **(1)**. The dead host is unreferenced by any panel and will be GC'd;
cleaning its props is wasted work.

### Initial subscribe / unsubscribe

```csharp
// RootRenderer.Initialize(UIDocument, ...)
public void Initialize(UIDocument hostDoc, Action<HostContext> env = null)
{
    EnsureSetup();
    _hostDoc = hostDoc;
    rootElement = hostDoc != null ? hostDoc.rootVisualElement : null;
    _committedHost = rootElement;
    if (hostDoc != null) hostDoc.panelChanged += OnHostPanelChanged;
    env?.Invoke(sharedHostContext);
}

private void OnDestroy()
{
    // (existing logic)
    if (_hostDoc != null) _hostDoc.panelChanged -= OnHostPanelChanged;
    _hostDoc = null;
    Unmount();
}
```

`Unmount()` must also unsubscribe in case it's called separately from
destroy. Add the `-=` there too.

## What this fixes vs. doesn't

**Fixes:**
- Undo/redo on the host UIDocument — visible content survives.
- Source-asset swap on the host UIDocument.
- GameObject disable → enable cycle (children stay alive in memory across the
  null window; reparent on re-attach).
- Editor selection storm — UI stays visible while the developer inspects the
  GameObject. Per-frame reparent is "ugly but correct."

**Does not fix:**
- Component state reset that would be intended by the developer (there isn't
  any — this is purely about the panel rebuilding underneath us).
- Adapter elements that cache their **own** panel/parent references — see
  "Risks" below.

## Risks and verification

### Risk 1: Element adapters & animations broken by panel-event dispatch on reparent

`VisualElement.Add(child)` dispatches `DetachFromPanelEvent` (from the dead
old root) and `AttachToPanelEvent` (on the new live root) for **every
descendant** in the moved subtree. Auditing the codebase reveals the
following are torn down or stopped by this dispatch:

| Subsystem | Effect on reparent |
|---|---|
| `<Video>` adapter | Full `Teardown()` → returns `VideoPlayer`/`RT` to pool, drops controller. Re-`Setup()` on attach calls async `Prepare()` (100–300 ms black RT). |
| `Hooks.UseAnimate` / `UseTweenFloat` / `Animator` | Schedule items detect `panel == null` and **stop without resuming** — animations die permanently. |
| List/Tree/Tab trackers (`MultiColumnListView`, `MultiColumnTreeView`, `TabView`) | Detach handlers fire; trackers re-attach on next props apply but transient drag/sort/scroll state may be lost. |
| User-declared `onAttachToPanel` / `onDetachFromPanel` props | Fire spuriously on every reparent (consumer-visible). |

**Confirmed safe (no change needed):** `<Audio>` (`UseEffect`-based, not panel
event-based), all hooks, signal subscriptions, component state, fiber tree
identity, cell pools (cells move with the view), normal UI events
(`onClick` etc.).

This risk is **not just theoretical for the editor selection storm** — it
also fires once on every legitimate single-shot trigger (undo, asset swap,
disable+enable). A user who undoes a transform change would see videos
restart and animations freeze even in normal workflow.

**Resolution:** addressed by
[REPARENT_RESILIENT_ADAPTERS.md](REPARENT_RESILIENT_ADAPTERS.md), which
introduces a defer-teardown pattern at the affected adapters and migrates
animations to a panel-independent ticker. **The Class 1 fix in this plan
is feature-complete on its own** (UI no longer disappears) **but should ship
together with REPARENT_RESILIENT_ADAPTERS.md to be production-quality.**
Without it, undo on the host UIDocument resets every video and freezes every
animation in the rendered tree.

### Risk 2: Per-frame storm performance

In the editor selection storm, the framework runs:
- 1 panel-event dispatch sweep over the full subtree (UIElements internals)
- 1 host-prop re-application
- 5 pointer writes (the retarget chain)

The panel-event sweep is O(N) over total descendant count. For typical UIs
(hundreds of nodes) this is invisible; for thousands it would become
noticeable. Since the storm is editor-only DX and not a production scenario,
this is acceptable. Add a diagnostic gate (`DiagnosticsConfig.EnableInternalLogs`)
so unexpected production occurrences are visible in logs.

### Risk 3: Inspector order / focus / scroll

`UIDocument` rebuilds may reset focus and scroll positions on the new root
itself. The fiber tree is unaware of these — preserving them would require
a separate snapshot/restore pass. Document as known editor-only limitation;
in builds none of these triggers fire.

### Risk 4: `panelChanged` arriving before our subscribe

If the bootstrap calls `Initialize(hostDoc)` *before* the UIDocument has
built its root once, `hostDoc.rootVisualElement` is null at that moment.
Our subscription is in place; `panelChanged` will fire on first attach and
`OnHostPanelChanged` will reparent (from the empty old root to the new live
one). Correct behavior. Worth a unit-style test.

### Risk 5: Editor host scenarios (`EditorWindow`)

`EditorRootRendererUtility` ([Editor/EditorRootRendererUtility.cs](../Editor/EditorRootRendererUtility.cs))
mounts onto an arbitrary `VisualElement` (typically `EditorWindow.rootVisualElement`).
There is no `UIDocument` in this path; instead, `EditorWindow` may rebuild
its root on docking, layout reload, or assembly reload. This plan does **not**
cover the editor host scenario directly. Follow-up: investigate whether
`EditorWindow.rootVisualElement` swap is observable via a similar event,
and if so, add an analogous `RetargetHost` entry point exposed by
`VNodeHostRenderer`.

## Files to add / change

| File | Change |
|---|---|
| `Runtime/Core/RootRenderer.cs` | Add `Initialize(UIDocument, ...)` overload, `_hostDoc` field, `_committedHost` field, `OnHostPanelChanged`, `ReparentMountedTreeTo`, `OnDestroy`/`Unmount` unsubscribe |
| `Shared/Core/VNodeHostRenderer.cs` | `internal void RetargetHost(VisualElement)`; make `hostElement` mutable internally |
| `Shared/Core/Fiber/FiberRenderer.cs` | `internal void RetargetContainer(VisualElement)` |
| `Editor/EditorRootRendererUtility.cs` | Mirror the new overload for editor host scenarios; for `EditorWindow` callers, panel-rebuild can also occur on docking — wire the same `panelChanged` path on the editor root if the editor host exposes a `UIDocument`. If it does not (custom `EditorWindow.rootVisualElement`), this plan does not cover it (separate concern; add a follow-up). |
| `Plans~/USE_UI_DOCUMENT_ROOT_HOOK.md` | Already cross-references this plan in non-goals. |

No source-generator changes. No `<Portal>` changes. No reconciler changes
beyond the `RetargetContainer` helper hook.

## Acceptance criteria

1. `RootRenderer.Initialize(UIDocument, env)` compiles and is wired in
   `AppBootstrap.cs`-style callers.
2. After the host UIDocument is undone/swapped/disabled+enabled, the
   `childCount` of the new live root reflects the mounted tree (i.e. no
   blank panel).
3. Component instance identity is preserved across the swap — verified via
   a `UseRef`-stable counter that does not reset.
4. `UseEffect` cleanups do NOT run on swap (state is not torn down). Verified
   with a counted side-effect log.
5. Editor selection storm: UI remains visible across all frames while the
   GameObject is selected; deselecting returns to normal idle.
6. No leak in editor mount/unmount cycles (existing `FiberRenderer.Clear`
   leak test still passes; new test ensures `panelChanged` is unsubscribed
   on `OnDestroy`).
7. `<Video>` / `<Audio>` survive a host swap (controller ref binding intact;
   playback may restart from 0 — that's expected given panel-scoped
   adapters).

## Related plans

- [USE_UI_DOCUMENT_ROOT_HOOK.md](USE_UI_DOCUMENT_ROOT_HOOK.md) — Class 2 (portal target panel changes).
- [REPARENT_RESILIENT_ADAPTERS.md](REPARENT_RESILIENT_ADAPTERS.md) — **required companion plan** to make this fix production-quality (defer-teardown for `<Video>`, animations, trackers).
- [VIDEO_AUDIO_ELEMENTS_PLAN.md](VIDEO_AUDIO_ELEMENTS_PLAN.md) — adapter lifecycle context.

## Known Unity quirks

Same editor selection storm note as the companion plan. Selecting a UIDocument
GameObject in playmode rebuilds `rootVisualElement` every frame. The reparent
runs every frame in this case; UI remains visible; CPU is the only cost.
Editor-only, selection-only, playmode-only. Not introduced by us; verified
with a minimal repro (single `UIDocument` GameObject + probe `MonoBehaviour`,
no ReactiveUITK code present).
