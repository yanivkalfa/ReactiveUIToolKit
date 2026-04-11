# Capture Event Props — Implementation Plan

## Summary

Add `onXxxCapture` props to support TrickleDown (capture phase) event registration, matching React's naming convention and Unity UI Toolkit's native two-phase dispatch model. Concurrently refactor `ApplyEvent`/`RemoveEvent` with generic helpers, fix pre-existing onChange registration gaps, and remove dead diagnostic counters.

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
| `onChangeCapture` | element-polymorphic | `ChangeEvent<T>` |
| `onInputCapture` | `InputEventHandler` | `InputEvent` |
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

Returns `bool` so onChange can short-circuit across multiple `ChangeEvent<T>` types:

```csharp
private static bool UnregisterEvent<T>(
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
        return true;
    }
    return false;
}
```

#### Resulting Call Sites

Standard events collapse to one-liners:
```csharp
// Bubble
if (eventPropName == "onKeyDown")        { RegisterEvent<KeyDownEvent>(element, meta, eventPropName, newSig, false); return; }
// Capture
if (eventPropName == "onKeyDownCapture") { RegisterEvent<KeyDownEvent>(element, meta, eventPropName, newSig, true); return; }
```

#### onChange / onInput — Also Use the Helper

The helper is universal. onChange polymorphism stays at the **call site**, not in the helper:

```csharp
// Registration
if (eventPropName == "onChange" || eventPropName == "onChangeCapture")
{
    bool capture = eventPropName == "onChangeCapture";
    if (element is Toggle or RadioButton or Foldout)
        RegisterEvent<ChangeEvent<bool>>(element, meta, eventPropName, newSig, capture);
    else if (element is SliderInt or RadioButtonGroup)
        RegisterEvent<ChangeEvent<int>>(element, meta, eventPropName, newSig, capture);
    else if (element is Slider)
        RegisterEvent<ChangeEvent<float>>(element, meta, eventPropName, newSig, capture);
    else
        RegisterEvent<ChangeEvent<string>>(element, meta, eventPropName, newSig, capture);
    return;
}

// onInput — trivially
if (eventPropName == "onInput" || eventPropName == "onInputCapture")
{
    RegisterEvent<InputEvent>(element, meta, eventPropName, newSig, eventPropName == "onInputCapture");
    return;
}
```

Removal uses short-circuit `||`:
```csharp
if (eventPropName == "onChange" || eventPropName == "onChangeCapture")
{
    UnregisterEvent<ChangeEvent<bool>>(element, meta, eventPropName)
    || UnregisterEvent<ChangeEvent<int>>(element, meta, eventPropName)
    || UnregisterEvent<ChangeEvent<float>>(element, meta, eventPropName)
    || UnregisterEvent<ChangeEvent<string>>(element, meta, eventPropName);
    return;
}

if (eventPropName == "onInput" || eventPropName == "onInputCapture")
{
    UnregisterEvent<InputEvent>(element, meta, eventPropName);
    return;
}
```

### Performance Characteristics

- **Hot path (InvokeHandler)**: UNTOUCHED. No changes whatsoever.
- **Registration/Removal**: Generic methods are monomorphized by JIT — identical native code to handwritten. Zero boxing (all types are reference types — `EventCallback<T>` is a delegate, `Delegate` storage is a reference upcast). Zero reflection. One delegate allocation per registration (same as current).
- **Prop diffing**: No changes. `ApplySingle` routes `propertyName.StartsWith("on")` + `is Delegate` to `ApplyEvent` (line ~1341).
- **UnregisterEvent `bool` return**: One extra `isinst` check per miss in the onChange short-circuit chain (at most 4 checks, one succeeds). Negligible vs. dictionary lookups already in the path.

### Removal Correctness

Unity's `UnregisterCallback` requires the same `useTrickleDown` parameter as was used during `RegisterCallback`. The capture flag is derived from the prop name (`propName.EndsWith("Capture")`), so it always matches. No extra metadata storage needed.

### Pre-Existing Issues Fixed by This Refactor

#### 1. onChange Registration Gap

**Problem**: `ApplyEvent` only handles 6 element types for onChange (Toggle, SliderInt, RadioButton, RadioButtonGroup, Slider, Foldout → fallback string). Elements like `DoubleField`, `LongField`, `ColorField`, `EnumField`, `ObjectField` fall through to the `ChangeEvent<string>` default — but Unity fires `ChangeEvent<double>`, `ChangeEvent<Color>`, etc., which never matches the registered wrapper. The handler never fires.

`InvokeHandler` has fast-path dispatch for 8 `ChangeEvent<T>` types (string, bool, int, float, double, long, Enum, Object), but registration only covers 4 of those types. The double/long/Enum/Object paths in InvokeHandler are effectively dead code.

**Fix**: Add missing element types to the registration dispatch. The generic helper makes this trivial — just add branches for the missing elements. The corresponding removal works automatically via `UnregisterEvent<T>` short-circuit.

#### 2. onClick Verbose Logging

**Problem**: Lines ~1905-1921 have a `Debug.Log` behind `DiagnosticsConfig.TraceLevel.Verbose` — only on onClick, not on any other event. Leftover from initial development.

**Fix**: Remove. The generic helper does not include any logging. If verbose tracing is needed later, it can be added uniformly in the helper.

#### 3. Dead Diagnostic Counters

**Problem**: `totalEventsRegistered`, `totalEventsRemoved`, `totalStyleSets`, `totalStyleResets` (lines 24-27) and `GetStyleMetrics()` (line ~2983) are completely unused. `GetStyleMetrics` is `public static` but has zero callers — not from benchmarks, diagnostics, editor windows, tests, or any external code. `totalEventsRegistered++` only appears on onClick (not on any other event) — an oversight from the initial commit where onClick was written first as an expanded block and all other events were added as compressed one-liners without the counter.

**Fix**: Remove all 4 counters, the `GetStyleMetrics()` method, and all increment statements. They add noise and make the refactor harder.

## Files to Change

### Runtime (core library)

| File | Change | Scope |
|---|---|---|
| `Shared/Core/ReactiveTypes.cs` | **None** | Same delegate types |
| `Shared/Core/NodeMetadata.cs` | **None** | Same dictionary storage |
| `Shared/Props/Typed/BaseProps.cs` | Add ~21 `OnXxxCapture` properties + `ToDictionary()` entries | Additive |
| `Shared/Props/PropsApplier.cs` | Refactor `ApplyEvent`/`RemoveEvent` with generic helpers, add capture branches, fix onChange gaps, remove dead counters/logging | Major refactor |

### IDE Extensions

| File | Change | Scope |
|---|---|---|
| `ide-extensions~/grammar/uitkx-schema.json` | Add ~21 capture prop entries to `universalAttributes` | Additive |
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Add ~21 entries to `s_eventCallbackParamTypes` | Additive |
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
| **Prop routing** | None | `ApplySingle` line ~1341: `propertyName.StartsWith("on") && propertyValue is Delegate` — matches capture props automatically |
| **HMR** | None | `EventHandlerTargets` lookup by prop name string — works for any prop name. Re-render reapplies handlers with correct phase. |
| **Source Generator** | None | `ToPropName()` uppercases first char: `onKeyDownCapture` → `OnKeyDownCapture`. No event-specific logic. |
| **Parser/Formatter** | None | Attribute parsing is format-agnostic. `onKeyDownCapture={handler}` parses identically to `onKeyDown={handler}`. |
| **InvokeHandler** | None | Dispatches on `del is Action<T>` — the delegate type determines dispatch, not the prop name. Capture handlers use the same delegate types as bubble. |
| **VS Code Extension** | Schema only | Reads `uitkx-schema.json` |
| **VS2022 Extension** | Schema only | Reads same schema |
| **Rider Extension** | Schema only | Reads same schema |

## Breaking Changes

**None.** All existing `onXxx` props work identically (bubble phase, default behavior). `onXxxCapture` is purely additive.

## Testing

- Run all existing tests after refactor — must pass with zero regressions
- Add formatter snapshot tests for `onKeyDownCapture` in UITKX markup
- Verify both bubble and capture handlers fire in correct order on same element
- Verify removal correctness (unregister with matching TrickleDown phase)

## Execution Order

1. Remove dead counters (`totalEventsRegistered/Removed`, `totalStyleSets/Resets`, `GetStyleMetrics()`) and onClick verbose logging
2. Add generic `RegisterEvent<T>` and `UnregisterEvent<T>` helpers to `PropsApplier.cs`
3. Refactor existing bubble events to use helpers (behavior-preserving), including onChange/onInput
4. Fix onChange registration gaps (add missing element types: DoubleField, ColorField, etc.)
5. Run all tests — must pass with zero regressions
6. Add capture event branches (one-liner per event type)
7. Add `BaseProps` capture properties + `ToDictionary()` entries
8. Add `onChangeCapture`/`onInputCapture` to per-element typed Props files
9. Add IDE schema + VDG entries
10. Update docs
11. Update snake game sample to use `onKeyDownCapture` prop
