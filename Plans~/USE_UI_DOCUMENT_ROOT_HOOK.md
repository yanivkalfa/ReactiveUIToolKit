# `UseUiDocumentRoot` Hook — Implementation Plan

## Summary

Add a `UseUiDocumentRoot` hook that bridges a Unity `UIDocument` panel into UITKX's
reactive render pipeline, returning the panel's `rootVisualElement` as a state value
that re-renders the calling component whenever the panel attaches, detaches, or its
source asset is swapped. Primary use case: targeting a `<Portal>` at a UIDocument
that is **not** the one driving `RootRenderer`, including UIDocuments whose root is
not yet built at boot time.

## Motivation

A component (e.g. a router page) wants to render some of its subtree into a
different UIDocument panel than the one hosting the main app. The naive approach:

```csharp
RootRenderer.Render(
    component,
    env: ctx => ctx.Environment[MenuStageSlotKey] = worldMenuUIDocument.rootVisualElement);
```

Doesn't work, for three independent reasons:

1. **`env` runs once at host setup, before the panel is built.**
   `UIDocument.rootVisualElement` is null until UIDocument's own `OnEnable` fires,
   so the dictionary entry is `null` and stays null.
2. **`Environment` is a plain dictionary.** Mutating it does not trigger any
   re-render. Components that read `ctx.Environment[Key]` snapshot the value at
   render time and never re-subscribe.
3. **Panels can detach and re-attach at runtime** (UIDocument disable/enable,
   `visualTreeAsset` swap). Even if (1) and (2) were solved, there is no signal
   path from `panelChanged` into the reconciler.

Workarounds today:

- **Defer the `Render()` call** until the slot UIDocument is enabled, then pass
  the resolved VE through `env`. Bootstrap-level only; can't be expressed inside
  components; doesn't react to subsequent detach/re-attach.
- **Pass the `VisualElement` as a prop** from the bootstrap MonoBehaviour. Same
  staleness problem unless the bootstrap manually re-renders on `panelChanged`.

Both push reactivity out of the component layer, where it belongs, and into
imperative bootstrap code.

## Design

### Hook

```csharp
// Shared/Core/Hooks.UiDocument.cs (new partial of Hooks)

public static partial class Hooks
{
    /// <summary>
    /// Subscribes to <paramref name="doc"/>'s panel lifecycle and returns its
    /// current <c>rootVisualElement</c> as reactive state. The calling component
    /// re-renders whenever the panel attaches, detaches, or swaps its source asset.
    /// Returns <c>null</c> while the panel is detached.
    /// </summary>
    public static VisualElement UseUiDocumentRoot(UIDocument doc)
    {
        var (root, setRoot) = UseState(doc != null ? doc.rootVisualElement : null);

        UseEffect(() =>
        {
            if (doc == null) return null;

            void Sync()
            {
                var current = doc.rootVisualElement;
                // Reference dedupe — early return only when the ref is identical to
                // what we last committed. We *always* propagate genuine changes,
                // even if they arrive every frame (see "Editor selection storm").
                if (ReferenceEquals(current, root)) return;
                setRoot(current);
            }

            doc.panelChanged += Sync;
            Sync(); // catch attach that happened between render and effect

            return () => doc.panelChanged -= Sync;
        }, doc); // re-subscribe if the UIDocument reference itself changes

        return root;
    }

    /// <summary>
    /// Convenience overload: resolves the <see cref="UIDocument"/> from
    /// <see cref="HostContext.Environment"/> by key, then delegates to
    /// <see cref="UseUiDocumentRoot(UIDocument)"/>.
    /// </summary>
    public static VisualElement UseUiDocumentRoot(string contextKey)
        => UseUiDocumentRoot(UseContext<UIDocument>(contextKey));
}
```

Why this works where `env` does not:

- `UseState` is the reactivity bridge. `setRoot(doc.rootVisualElement)` from the
  `panelChanged` callback schedules a re-render, exactly the missing signal.
- `UseEffect` cleanup unsubscribes on unmount or when `doc` changes — consistent
  with the leak-free lifecycle established for hooks (see CHANGELOG `FiberRenderer.Clear`
  cleanup fix).
- Eager `Sync()` inside the effect closes the race where the panel attached
  between the initial `UseState` snapshot and effect execution.
- Storing the **stable `UIDocument` reference** in `Environment` (vs. the
  unstable `rootVisualElement`) sidesteps reason (2) above: descendants subscribe
  via the hook, not via dictionary reads.

### Call-site contract: gate on null

`<Portal target={null}>` does **not** skip rendering. Verified in
[Shared/Core/Fiber/FiberFactory.cs](../Shared/Core/Fiber/FiberFactory.cs) and
[Shared/Core/Fiber/FiberReconciler.cs](../Shared/Core/Fiber/FiberReconciler.cs):
the Portal fiber gets `HostElement = null`, and the commit walk reparents
children to the nearest non-null ancestor — i.e. children appear inside the
caller's local subtree, not at the portal target. To avoid that flash, the call
site gates:

```razor
component MenuPage {
  var target = UseUiDocumentRoot("menu.uiDocument");

  return (
    <VisualElement style={Root}>
      {target != null
        ? <Portal target={target}>{chrome}</Portal>
        : null}
    </VisualElement>
  );
}
```

This matches the existing pattern in
[Samples/Components/UitkxTestFileDoNotTouch/UitkxTestFileDoNotTouch.uitkx](../Samples/Components/UitkxTestFileDoNotTouch/UitkxTestFileDoNotTouch.uitkx)
and
[Samples/Components/PortalEventScopeDemoFunc/PortalEventScopeDemoFunc.uitkx](../Samples/Components/PortalEventScopeDemoFunc/PortalEventScopeDemoFunc.uitkx).

### Lifecycle table

| Transition | Behavior with gate |
|---|---|
| `target` swaps between two non-null VEs (asset swap) | `<Portal>` stays mounted; reconciler reparents children via the existing target-diff fix; **state preserved** |
| `target` goes `null` (panel detach) | `chrome` unmounts; effects run cleanup; subtree state lost |
| `target` returns from `null` (re-attach) | `chrome` mounts fresh |

State that must survive detach/re-attach lives **above** the gate, in
`MenuPage` or higher (e.g. the router store). Standard React conditional-render
pattern.

If detach/re-attach later turns out to be high-frequency and warm-keeping the
subtree matters, the right fix is a reconciler-level "hidden Portal" mode that
keeps the fiber alive without committing children — that's a separate plan, not
a hook concern.

## Wiring `UIDocument` into `Environment`

Bootstrap registers the stable handle once:

```csharp
public class MenuBootstrap : MonoBehaviour
{
    [SerializeField] private RootRenderer rootRenderer;
    [SerializeField] private UIDocument hostUIDocument;
    [SerializeField] private UIDocument worldMenuUIDocument;

    private void Start()
    {
        rootRenderer.Initialize(hostUIDocument.rootVisualElement);
        rootRenderer.Render(
            V.Func(MenuPage.Render),
            env: ctx => ctx.Environment[MenuContextKeys.UiDocument] = worldMenuUIDocument);
    }
}

public static class MenuContextKeys
{
    public const string UiDocument = "menu.uiDocument";
}
```

This is **safe** even though we said `Environment` is non-reactive — the
`UIDocument` reference is stable for the lifetime of the host. Reactivity comes
from `panelChanged` inside the hook, not from Environment mutation.

## Files to add / change

- **New:** `Shared/Core/Hooks.UiDocument.cs` — the hook (two overloads).
- **No changes** to `Portal`, the reconciler, `HostContext`, or `env` plumbing.
- **Optional follow-ups:**
  - Doc page under `ReactiveUIToolKitDocs~/src/pages/UITKX/` mirroring other hook pages.
  - Sample: `Samples/Components/UiDocumentSlot/` showing a router page portaling into a separate UIDocument that toggles at runtime.
  - Source-generator awareness: none required; hook is plain Hooks API.

## Open questions / explicit non-goals

- **Not solving:** main-panel detach (the host UIDocument driving `RootRenderer`
  itself going away). That is a separate `RootRenderer` reparenting concern,
  tracked in [ROOT_RENDERER_HOST_REATTACH.md](ROOT_RENDERER_HOST_REATTACH.md).
- **Not solving:** preserving Portal subtree state across full detach. Documented
  trade-off above; deferred to a future "hidden Portal" feature if needed.
- **`panelChanged` coverage:** fires on attach/detach and source-asset swap.
  If a future Unity API path mutates `rootVisualElement` without firing it,
  add a `schedule.Execute` follow-up sync inside the effect.

## Side-effects on the moved subtree

When the `<Portal target>` swaps from one VE to another, Unity dispatches
`DetachFromPanelEvent` + `AttachToPanelEvent` on every descendant in the
portaled subtree. By default this tears down panel-scoped resources:
`<Video>` resets to frame 0; `Hooks.UseAnimate` / `UseTweenFloat` / `Animator`
freeze; List/Tree/Tab trackers detach. Hooks, signals, `<Audio>`, and
component state are unaffected (they're tied to fiber lifecycle, not panel
events).

These side-effects are addressed in
[REPARENT_RESILIENT_ADAPTERS.md](REPARENT_RESILIENT_ADAPTERS.md) via a
defer-teardown pattern. **Until that plan lands, callers of this hook should
avoid putting `<Video>` or animations directly under a `<Portal>` whose
target may change.** Static portal targets (set once at boot) are safe.

## Known Unity quirks

### Editor selection storm

Selecting a `UIDocument` GameObject in the Hierarchy **during playmode** causes
Unity to rebuild `rootVisualElement` every single frame. Verified with a minimal
repro (empty scene, single `UIDocument` GameObject + a probe `MonoBehaviour` that
logs `rootVisualElement.GetHashCode()` per frame). The id changes every frame
while selected, stops the moment you click elsewhere.

This is:
- **Editor-only** — does not occur in built players.
- **Selection-only** — does not occur when no UIDocument GameObject is selected.
- **Playmode-only** — does not occur in edit mode.
- **Not introduced by us** — reproduces in a clean scene with zero ReactiveUITK code.

The hook reacts every frame in this case (`setRoot` with the new VE → re-render →
`<Portal>` reparent). This is **intentional**:

> The contract is: whenever the root reference changes, we reparent. The
> reference dedupe is a cheap-path optimization for the common single-event case
> (undo, asset swap, enable/disable). It is **never** a throttle that could
> cause the developer to lose their UI mid-inspection.

In the storm, every frame's new VE is genuinely different, so dedupe doesn't
suppress anything — and that's correct. The UI must remain visible while the
developer inspects the GameObject. CPU cost is acceptable because this only
happens while the developer is actively staring at the inspector.

## Acceptance criteria

1. `UseUiDocumentRoot(UIDocument)` and `UseUiDocumentRoot(string)` compile and
   live in the `Hooks` partial.
2. Hook returns the current root, `null` when detached, and triggers a re-render
   on every `panelChanged`.
3. `UseEffect` cleanup unsubscribes on unmount and on `doc` dep change — no leak
   in the editor mount/unmount cycle (verify via existing `FiberRenderer.Clear`
   leak test patterns).
4. End-to-end sample: a router page using
   `var target = UseUiDocumentRoot("menu.uiDocument")` + gated `<Portal>` mounts
   correctly when the slot UIDocument's GameObject is enabled after first render,
   and unmounts cleanly when disabled.
