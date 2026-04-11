# Capture Event Props — Implementation Plan

## Summary

Add `onXxxCapture` props to support TrickleDown (capture phase) event registration, matching React's naming convention and Unity UI Toolkit's native two-phase dispatch model.

## Motivation

Unity UI Toolkit dispatches every event through two phases:
1. **TrickleDown** (capture) — root → target
2. **BubbleUp** (bubble) — target → root

Currently UITKX only registers event handlers in BubbleUp phase. Users who need capture-phase handling (e.g., intercepting keyboard events before children consume them) must use raw `RegisterCallback<T>(handler, TrickleDown.TrickleDown)` via `useEffect` — bypassing the declarative prop system entirely.

## Design

### Naming Convention

Follow React's established pattern: `onXxxCapture` for capture-phase handlers.

```xml
<VisualElement onKeyDown={handler} />          <!-- bubble (existing) -->
<VisualElement onKeyDownCapture={handler} />   <!-- capture (new) -->
```

Same delegate type for both. No new delegate types or event data classes needed.

### Capture Props to Add

| Capture Prop | Delegate Type | UIElements Event |
|---|---|---|
| `onClickCapture` | `PointerEventHandler` | `ClickEvent` |
| `onPointerDownCapture` | `PointerEventHandler` | `PointerDownEvent` |
| `onPointerUpCapture` | `PointerEventHandler` | `PointerUpEvent` |
| `onPointerMoveCapture` | `PointerEventHandler` | `PointerMoveEvent` |
| `onPointerEnterCapture` | `PointerEventHandler` | `PointerEnterEvent` |
| `onPointerLeaveCapture` | `PointerEventHandler` | `PointerLeaveEvent` |
| `onWheelCapture` | `WheelEventHandler` | `WheelEvent` |
| `onScrollCapture` | `WheelEventHandler` | `WheelEvent` |
| `onKeyDownCapture` | `KeyboardEventHandler` | `KeyDownEvent` |
| `onKeyUpCapture` | `KeyboardEventHandler` | `KeyUpEvent` |
| `onFocusCapture` | `FocusEventHandler` | `FocusEvent` |
| `onBlurCapture` | `FocusEventHandler` | `BlurEvent` |
| `onFocusInCapture` | `FocusEventHandler` | `FocusInEvent` |
| `onFocusOutCapture` | `FocusEventHandler` | `FocusOutEvent` |
| `onDragEnterCapture` | `DragEventHandler` | `DragEnterEvent` |
| `onDragLeaveCapture` | `DragEventHandler` | `DragLeaveEvent` |
| `onDragUpdatedCapture` | `DragEventHandler` | `DragUpdatedEvent` |
| `onDragPerformCapture` | `DragEventHandler` | `DragPerformEvent` |
| `onDragExitedCapture` | `DragEventHandler` | `DragExitedEvent` |

Lifecycle events (`onGeometryChanged`, `onAttachToPanel`, `onDetachFromPanel`) do NOT get capture variants — they are target-only events that don't propagate.

## Implementation

### Architecture: Generic Helper Refactor

Replace the ~400-line if/else chains in `ApplyEvent`/`RemoveEvent` with generic helpers. Keep if/else dispatch (no dictionary) for zero-allocation, compile-time type safety.

#### Registration Helper

```csharp
private static void RegisterEvent<T>(
    VisualElement element, NodeMetadata meta,
    string propName, string newSig, bool capture
) where T : EventBase<T>, new()
{
    if (!meta.EventHandlers.ContainsKey(propName))
    {
        EventCallback<T> w = e => InvokeEvent(meta, propName, e);
        element.RegisterCallback(w, capture
            ? TrickleDown.TrickleDown
            : TrickleDown.NoTrickleDown);
        meta.EventHandlers[propName] = w;
    }
    meta.EventHandlerSignatures[propName] = newSig;
}
```

#### Removal Helper

```csharp
private static void UnregisterEvent<T>(
    VisualElement element, NodeMetadata meta, string propName
) where T : EventBase<T>, new()
{
    if (meta.EventHandlers.TryGetValue(propName, out var handler)
        && handler is EventCallback<T> cb)
    {
        bool capture = propName.EndsWith("Capture");
        element.UnregisterCallback(cb, capture
            ? TrickleDown.TrickleDown
            : TrickleDown.NoTrickleDown);
        meta.EventHandlers.Remove(propName);
    }
}
```

#### Resulting Call Sites (one-liners)

```csharp
// Bubble
if (eventPropName == "onKeyDown")        { RegisterEvent<KeyDownEvent>(element, meta, eventPropName, newSig, false); return; }
// Capture
if (eventPropName == "onKeyDownCapture") { RegisterEvent<KeyDownEvent>(element, meta, eventPropName, newSig, true); return; }
```

### Performance Characteristics

- **Hot path (InvokeHandler)**: UNTOUCHED. No changes whatsoever.
- **Registration/Removal**: Generic methods are monomorphized by JIT — identical IL to handwritten code. Zero boxing, zero reflection, zero allocation beyond the wrapper delegate (same as current).
- **Prop diffing**: No changes. `ApplyDiff` detects prop name changes, delegates to `ApplySingle`, which routes `"on*"` prefixed delegates to `ApplyEvent`.

### Special Cases

- **`onChange`**: Polymorphic on element type (Toggle → `ChangeEvent<bool>`, Slider → `ChangeEvent<float>`, etc.). Keep existing if/else pattern — the generic helper doesn't fit polymorphic dispatch. No capture variant needed for change events.
- **`onInput`**: Direct string handler, not a standard UIElements event pattern. Keep existing. No capture variant.
- **`onClick` verbose logging**: Remove. It was leftover debug code only on onClick. If verbose event tracing is needed, add it uniformly in the generic helper.

### Removal Correctness

Unity's `UnregisterCallback` requires the same `useTrickleDown` parameter as was used during `RegisterCallback`. The capture flag is derived from the prop name (`propName.EndsWith("Capture")`), so it always matches. No extra metadata storage needed.

## Files to Change

### Runtime (core library)

| File | Change | Scope |
|---|---|---|
| `Shared/Core/ReactiveTypes.cs` | **None** | Same delegate types |
| `Shared/Core/NodeMetadata.cs` | **None** | Same dictionary storage |
| `Shared/Props/Typed/BaseProps.cs` | Add ~19 `OnXxxCapture` properties + `ToDictionary()` entries | Additive |
| `Shared/Props/PropsApplier.cs` | Refactor `ApplyEvent`/`RemoveEvent` with generic helpers, add capture branches | Major refactor |

### IDE Extensions

| File | Change | Scope |
|---|---|---|
| `ide-extensions~/grammar/uitkx-schema.json` | Add ~19 capture prop entries | Additive |
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Add ~19 entries to `s_eventCallbackParamTypes` | Additive |
| `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` | **None** | Delegates unchanged |

### Source Generator

| File | Change | Scope |
|---|---|---|
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | **None** | `ToPropName("onKeyDownCapture")` → `"OnKeyDownCapture"` works automatically |

### Documentation

| File | Change | Scope |
|---|---|---|
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Events/EventsPage.tsx` | Add capture events section, explain two-phase dispatch | Content |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Events/EventsPage.example.ts` | Add capture event example | Content |

## Subsystem Impact Analysis

| Subsystem | Impact | Why |
|---|---|---|
| HMR | None | Handler swap uses `EventHandlerTargets` lookup by prop name — works for any prop name |
| Source Generator | None | PascalCase conversion handles `onKeyDownCapture` → `OnKeyDownCapture` |
| Prop Diffing | None | `"on*".StartsWith("on")` matches capture props naturally |
| InvokeHandler | None | Dispatches on delegate type, not prop name |
| VS Code Extension | Schema only | Reads `uitkx-schema.json` |
| VS2022 Extension | Schema only | Reads same schema |
| Rider Extension | Schema only | Reads same schema |

## Breaking Changes

**None.** All existing `onXxx` props work identically (bubble phase, default behavior). `onXxxCapture` is purely additive.

## Testing

- Add unit tests for `RegisterEvent<T>` / `UnregisterEvent<T>` helpers
- Add formatter snapshot tests for `onKeyDownCapture` in UITKX markup
- Verify both bubble and capture handlers fire in correct order on same element
- Verify removal correctness (unregister with matching TrickleDown phase)

## Execution Order

1. Add generic helpers to `PropsApplier.cs`
2. Refactor existing bubble events to use helpers (behavior-preserving)
3. Run all 904 tests — must pass with zero regressions
4. Add capture event branches
5. Add `BaseProps` properties
6. Add IDE schema/VDG entries
7. Update docs
8. Update snake game sample to use `onKeyDownCapture` prop
