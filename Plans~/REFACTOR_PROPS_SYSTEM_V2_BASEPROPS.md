# Refactor: `BaseProps` Inheritance — Eradicate All Dict-Based `V.*` APIs

**Branch:** `refactory_types_in_core_library`  
**Status:** 🔲 NOT STARTED  
**Scope:** `Shared/Props/Typed/`, `Shared/Core/V.cs`, `Shared/Elements/`, call sites across `Samples/`, `Runtime/`, `Editor/`, and the UITKX source generator

---

## 1. Goals and Motivation

### Current State

`V.VisualElement` exposes an `IReadOnlyDictionary<string, object>` as its primary overload:

```csharp
// V.cs line ~28
public static VirtualNode VisualElement(
    IReadOnlyDictionary<string, object> properties,
    string key = null,
    params VirtualNode[] children)
```

`PropsResolver` (Strategy B — Roslyn compile-time scan) classifies the first overload it finds for each tag. Its rule:

| First-param type | Classification |
|-----------------|----------------|
| ends in `"Props"` | `BuiltinTyped` ✅ |
| is `IReadOnlyDictionary` | `BuiltinDictionary` ⚠️ |
| anything else | skipped |

Because the primary `V.VisualElement` overload hits the `IReadOnlyDictionary` rule, the tag `<VisualElement>` is classified as `BuiltinDictionary` in every generated UITKX file. It **can never be used as a typed JSX tag** — it falls back to the old dict-based code path with string keys and boxed values.

**Audit of all ~100 `V.VisualElement(...)` call sites confirms:** every single one passes only a `{"style": ...}` entry. The dict was premature "open contract" generalization that was never needed and is now actively harmful.

Additionally, every existing typed `Props` class duplicates the same 3–6 base fields:

```csharp
// in BoxProps, ButtonProps, LabelProps, TextFieldProps, ScrollViewProps, ...
public string Name { get; set; }
public string ClassName { get; set; }
public Style? Style { get; set; }
public Action<VisualElement> Ref { get; set; }
public PickingMode? PickingMode { get; set; }
public bool? Focusable { get; set; }
public int? TabIndex { get; set; }
```

These are duplicated by copy-paste into **every** Props class, with no shared inheritance. `PropsApplier` already handles all of these keys uniformly — the only thing missing is the type hierarchy on the C# side.

### Goals After Refactor

- `BaseProps` abstract class captures every property that `PropsApplier` handles uniformly across all element types.
- All existing typed Props classes (`BoxProps`, `ButtonProps`, `LabelProps`, ...) inherit from `BaseProps` and remove their redundant duplicate fields.
- New `VisualElementProps : BaseProps` (empty subclass, typed identity only) replaces the dict overload as `V.VisualElement`'s primary parameter.
- `V.VisualElement` primary overload accepts `VisualElementProps` → `PropsResolver` classifies it as `BuiltinTyped` → `<VisualElement>` is a first-class typed JSX tag.
- Zero `IReadOnlyDictionary<string, object>` in any public `V.*` API surface.
- No behaviour change at runtime — `PropsApplier.Apply` already handles every field present in `BaseProps`.
- All ~100 `V.VisualElement(new Dictionary<...>{{"style", s}})` call sites become `V.VisualElement(new VisualElementProps { Style = s })` (or `<VisualElement style={s}>` in UITKX).

---

## 2. `BaseProps` Design

Source: Unity 6.2 `VisualElement` + `Focusable` documentation, cross-referenced against `PropsApplier` (existing keys) and what is settable on every element.

### Properties currently handled by `PropsApplier` (already implemented)

| PropsApplier key | C# Type | `VisualElement` API |
|-----------------|---------|--------------------|
| `"style"` | `Style` | `element.style` merge via `StyleProperties.Apply` |
| `"name"` | `string` | `element.name` |
| `"className"` | `string` | `element.AddToClassList` |
| `"ref"` | `Action<VisualElement>` | callback after mount |
| `"contentContainer"` | `VisualElement` | `element.contentContainer` override |
| `"pickingMode"` | `PickingMode` | `element.pickingMode` |
| `"focusable"` | `bool` | `element.focusable` (from `Focusable`) |
| `"tabIndex"` | `int` | `element.tabIndex` (from `Focusable`) |
| `"onClick"` | `Action` | `Clickable` manipulator |
| `"onPointerDown"` | `EventCallback<PointerDownEvent>` | `RegisterCallback` |
| `"onPointerUp"` | `EventCallback<PointerUpEvent>` | `RegisterCallback` |
| `"onPointerMove"` | `EventCallback<PointerMoveEvent>` | `RegisterCallback` |
| `"onPointerEnter"` | `EventCallback<PointerEnterEvent>` | `RegisterCallback` |
| `"onPointerLeave"` | `EventCallback<PointerLeaveEvent>` | `RegisterCallback` |
| `"onWheel"` | `EventCallback<WheelEvent>` | `RegisterCallback` |
| `"onFocus"` | `EventCallback<FocusEvent>` | `RegisterCallback` |
| `"onBlur"` | `EventCallback<BlurEvent>` | `RegisterCallback` |
| `"onKeyDown"` | `EventCallback<KeyDownEvent>` | `RegisterCallback` |
| `"onKeyUp"` | `EventCallback<KeyUpEvent>` | `RegisterCallback` |
| `"onDragEnter"` | `EventCallback<DragEnterEvent>` | `RegisterCallback` |
| `"onDragLeave"` | `EventCallback<DragLeaveEvent>` | `RegisterCallback` |
| `"onScroll"` | `EventCallback<WheelEvent>` | `RegisterCallback` |

### New properties to add to `PropsApplier` as part of this refactor

Confirmed present on **every** `VisualElement` subclass in Unity 6.2 docs — writable, universally useful:

| Property | C# Type | `VisualElement` API | Notes |
|---------|---------|--------------------|---------|
| `"visible"` | `bool` | `element.visible` | Show/hide without removing from layout |
| `"enabled"` | `bool` | `element.SetEnabled(bool)` | Disables event receipt; grays out UI |
| `"tooltip"` | `string` | `element.tooltip` | Editor-only hover tooltip |
| `"viewDataKey"` | `string` | `element.viewDataKey` | UI Toolkit persistence key |
| `"delegatesFocus"` | `bool` | `element.delegatesFocus` | From `Focusable` — delegates focus to first child |
| `"languageDirection"` | `LanguageDirection` | `element.languageDirection` | RTL/LTR; propagates to children |

Proposed `BaseProps.cs`:

```csharp
namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Base class for all built-in element props.
    /// Covers every property that VisualElement exposes as a writable field
    /// and that PropsApplier handles uniformly across all element types.
    /// Source: Unity 6.2 VisualElement + Focusable API docs.
    /// </summary>
    public abstract class BaseProps : IProps
    {
        // --- Identity / structure ---
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style? Style { get; set; }
        public Action<VisualElement> Ref { get; set; }
        public VisualElement ContentContainer { get; set; }

        // --- Visibility / enabled state ---
        // visible  → element.visible (hides rendering but keeps layout space)
        // enabled  → element.SetEnabled(bool) (disables event processing, triggers :disabled USS)
        public bool? Visible { get; set; }
        public bool? Enabled { get; set; }

        // --- Tooltip / persistence ---
        public string Tooltip { get; set; }      // editor-only hover label
        public string ViewDataKey { get; set; }  // UI Toolkit scroll/expand persistence

        // --- Focus / interaction (from Focusable) ---
        public PickingMode? PickingMode { get; set; }
        public bool? Focusable { get; set; }
        public int? TabIndex { get; set; }
        public bool? DelegatesFocus { get; set; } // delegates focus to first focusable child

        // --- Locale / direction ---
        public LanguageDirection? LanguageDirection { get; set; } // RTL/LTR, propagates to children

        // --- Pointer / input events ---
        public Action OnClick { get; set; }
        public EventCallback<PointerDownEvent> OnPointerDown { get; set; }
        public EventCallback<PointerUpEvent> OnPointerUp { get; set; }
        public EventCallback<PointerMoveEvent> OnPointerMove { get; set; }
        public EventCallback<PointerEnterEvent> OnPointerEnter { get; set; }
        public EventCallback<PointerLeaveEvent> OnPointerLeave { get; set; }
        public EventCallback<WheelEvent> OnWheel { get; set; }

        // --- Drag-and-drop events ---
        public EventCallback<DragEnterEvent> OnDragEnter { get; set; }
        public EventCallback<DragLeaveEvent> OnDragLeave { get; set; }
        public EventCallback<DragUpdatedEvent> OnDragUpdated { get; set; }
        public EventCallback<DragPerformEvent> OnDragPerform { get; set; }
        public EventCallback<DragExitedEvent> OnDragExited { get; set; }

        // --- Focus events ---
        public EventCallback<FocusEvent> OnFocus { get; set; }
        public EventCallback<BlurEvent> OnBlur { get; set; }
        public EventCallback<FocusInEvent> OnFocusIn { get; set; }
        public EventCallback<FocusOutEvent> OnFocusOut { get; set; }

        // --- Keyboard events ---
        public EventCallback<KeyDownEvent> OnKeyDown { get; set; }
        public EventCallback<KeyUpEvent> OnKeyUp { get; set; }

        // --- Layout / lifecycle events ---
        public EventCallback<GeometryChangedEvent> OnGeometryChanged { get; set; }
        public EventCallback<AttachToPanelEvent> OnAttachToPanel { get; set; }
        public EventCallback<DetachFromPanelEvent> OnDetachFromPanel { get; set; }
    }
}
```

> **Note on `enabled` vs `enabledSelf`:** `element.enabledSelf` is read-only (returns current state). Setting enabled state requires calling `element.SetEnabled(bool)`. `PropsApplier` must call `SetEnabled` for the `"enabled"` key, not assign a property directly.

> **Note on casing:** `PropsApplier` uses camelCase string keys internally (`"onClick"`, `"onPointerDown"`, etc.). The Props property names use PascalCase (`OnClick`, `OnPointerDown`, etc.) — `VisualElementAdapter` and `PropsApplier` map between these two conventions.

---

## 3. `VisualElementProps` Design

```csharp
namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Typed props for a plain VisualElement.
    /// Inherits all base properties from BaseProps — no element-specific extras.
    /// This class exists solely to provide a typed identity for PropsResolver classification.
    /// </summary>
    public sealed class VisualElementProps : BaseProps { }
}
```

---

## 4. Affected Files — Summary Table

| Layer | File | Nature of Change |
|-------|------|------------------|
| Props | `Shared/Props/Typed/BaseProps.cs` | **New file** — shared base for all element props (Unity 6.2 full API coverage) |
| Props | `Shared/Props/Typed/VisualElementProps.cs` | **New file** — empty typed identity class |
| Props | `Shared/Props/Typed/BoxProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/ButtonProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/LabelProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/TextFieldProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/ToggleProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/ScrollViewProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/SliderProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/RepeatButtonProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | `Shared/Props/Typed/AnimateProps.cs` | Inherit `BaseProps`; remove duplicated fields |
| Props | *(all other `*Props.cs`)* | Inherit `BaseProps`; remove duplicated fields |
| Core | `Shared/Core/V.cs` | Replace primary `VisualElement` overload; make dict overload `private` |
| Core | `Shared/Core/V.cs` | Migrate `VisualElementSafe` internal usage off the dict overload |
| Core | `Shared/Core/V.cs` | Remove the `Style` convenience overload (subsumed by `VisualElementProps`) |
| Elements | `Shared/Elements/VisualElementAdapter.cs` | No change expected — already routes through `PropsApplier` |
| Generator | `SourceGenerator~/Emitter/PropsResolver.cs` | No change needed — resolver auto-classifies `VisualElementProps` as `BuiltinTyped` |
| Call sites | ~100 files across `Samples/`, `Editor/`, `Runtime/` | Replace `V.VisualElement(new Dictionary...)` with `V.VisualElement(new VisualElementProps {...})` |
| UITKX | UITKX files currently forced to use `<Box>` as workaround | Can now use `<VisualElement>` with typed props |

---

## 5. Step-by-Step Implementation

### Phase 1 — Create `BaseProps.cs` and `VisualElementProps.cs`

**Status:** 🔲 NOT STARTED

- [ ] Create `Shared/Props/Typed/BaseProps.cs` with all common fields (see §2 — Unity 6.2 validated full list).
- [ ] Create `Shared/Props/Typed/VisualElementProps.cs` (empty, inherits `BaseProps`).
- [ ] Compile. Expect zero errors — no existing code references these types yet.

---

### Phase 2 — Migrate all existing Props classes to inherit `BaseProps`

**Status:** 🔲 NOT STARTED

For each Props class:
1. Add `: BaseProps` to the class declaration.
2. Delete every field that is already declared in `BaseProps`: `Name`, `ClassName`, `Style`, `Ref`, `ContentContainer`, `Visible`, `Enabled`, `Tooltip`, `ViewDataKey`, `PickingMode`, `Focusable`, `TabIndex`, `DelegatesFocus`, `LanguageDirection`, `OnClick`, `OnPointerDown`, `OnPointerUp`, `OnPointerMove`, `OnPointerEnter`, `OnPointerLeave`, `OnWheel`, `OnDragEnter`, `OnDragLeave`, `OnDragUpdated`, `OnDragPerform`, `OnDragExited`, `OnFocus`, `OnBlur`, `OnFocusIn`, `OnFocusOut`, `OnKeyDown`, `OnKeyUp`, `OnGeometryChanged`, `OnAttachToPanel`, `OnDetachFromPanel`.
3. Compile. Expect zero errors for each migrated file (properties are still reachable via inheritance).

Files:
- [ ] `BoxProps.cs`
- [ ] `ButtonProps.cs`
- [ ] `LabelProps.cs`
- [ ] `TextFieldProps.cs`
- [ ] `ToggleProps.cs`
- [ ] `ScrollViewProps.cs`
- [ ] `SliderProps.cs`
- [ ] `RepeatButtonProps.cs`
- [ ] `AnimateProps.cs`
- [ ] *(enumerate all remaining `*Props.cs` files and add them here)*

> After all files are migrated: grep for the removed field names across `Shared/Props/Typed/` to confirm no stragglers.

---

### Phase 3 — Add `V.VisualElement(VisualElementProps, ...)` overload; privatize dict overload

**Status:** 🔲 NOT STARTED

**File:** `Shared/Core/V.cs`

#### 3a. Add new public typed overload

```csharp
public static VirtualNode VisualElement(
    VisualElementProps props,
    string key = null,
    params VirtualNode[] children)
{
    return VisualElement(PropsApplier.ToDict(props), key, children);
    // OR: forward directly to the internal element factory without going through dict
}
```

> **Preferred approach (no-dict path):** If `VisualElementAdapter` accepts `IProps` directly (or the `VirtualNode` factory accepts `IProps`), route through that. Keeps consistency with the IProps refactor. If the VE adapter still uses `PropsApplier.Apply(element, dict)` internally, a `PropsApplier.ToDict` helper (or direct dispatch to an overload that accepts `BaseProps`) avoids any heap allocation difference vs the old dict path.

#### 3b. Make the dict overload private

```csharp
// BEFORE (public):
public static VirtualNode VisualElement(
    IReadOnlyDictionary<string, object> properties, ...)

// AFTER (private — keep for VisualElementSafe and AnimateFunc to compile during transition):
private static VirtualNode VisualElement_Dict(
    IReadOnlyDictionary<string, object> properties, ...)
```

Renaming to `VisualElement_Dict` (not overloading) avoids any ambiguity. Update all internal callers.

#### 3c. Remove the `Style` convenience overload

```csharp
// DELETE this — subsumed by new typed overload:
public static VirtualNode VisualElement(Style style, string key = null, params VirtualNode[] children)
// Callers update to: V.VisualElement(new VisualElementProps { Style = style }, ...)
```

- [ ] Add new typed overload.
- [ ] Rename dict overload to private `VisualElement_Dict`.
- [ ] Update `VisualElementSafe` to call `VisualElement_Dict`.
- [ ] Update any other internal `V.VisualElement(new Dictionary...{})` calls within `V.cs` itself.
- [ ] Remove Style convenience overload.
- [ ] Compile. Expect compile errors at every external call site — these are the call sites to fix in Phase 4.

---

### Phase 4 — Migrate ~100 call sites

**Status:** 🔲 NOT STARTED

Use `grep_search` for `V\.VisualElement\(` to enumerate every call site.

Generic migration pattern:

```csharp
// BEFORE
V.VisualElement(new Dictionary<string, object> { { "style", myStyle } }, key)

// AFTER
V.VisualElement(new VisualElementProps { Style = myStyle }, key)
```

If a call site passes only `style` (confirmed for all ~100 sites), the migration is mechanical.

Special cases:
- Any call passing `"name"` → `new VisualElementProps { Name = ..., Style = ... }`
- Any call passing `"ref"` → `new VisualElementProps { Ref = ..., Style = ... }`
- Any call using `V.VisualElement(someStyle, key)` (Style overload) → `new VisualElementProps { Style = someStyle }`

- [ ] Run grep to capture full list of call sites and verify count.
- [ ] Migrate `Samples/` call sites.
- [ ] Migrate `Editor/` call sites.
- [ ] Migrate `Runtime/` call sites.
- [ ] Migrate `Shared/` internal call sites (outside `V.cs`).
- [ ] Compile. Expect zero errors.

---

### Phase 5 — Delete `VisualElement_Dict` and finalize

**Status:** 🔲 NOT STARTED

After Phase 4, the private `VisualElement_Dict` overload should have zero callers.

- [ ] Verify zero remaining references to `VisualElement_Dict` via grep.
- [ ] Migrate `VisualElementSafe` to construct `VisualElementProps` directly (no more dict path).
- [ ] Delete `VisualElement_Dict`.
- [ ] Verify no remaining `IReadOnlyDictionary` anywhere in public `V.*` APIs via grep.
- [ ] Compile + full test run.

---

### Phase 6 — `PropsApplier` additions (new properties from BaseProps)

**Status:** 🔲 NOT STARTED

All properties in `BaseProps` beyond what `PropsApplier` already handles must be wired up. Confirmed from Unity 6.2 docs — all are present on every `VisualElement` subclass:

| BaseProps field | UIToolkit API | `PropsApplier` key | Notes |
|----------------|--------------|--------------------|---------|
| `Visible` | `element.visible` | `"visible"` | Direct bool assignment |
| `Enabled` | `element.SetEnabled(bool)` | `"enabled"` | Must call method, not assign property |
| `Tooltip` | `element.tooltip` | `"tooltip"` | string; editor-only |
| `ViewDataKey` | `element.viewDataKey` | `"viewDataKey"` | string; persistence |
| `DelegatesFocus` | `element.delegatesFocus` | `"delegatesFocus"` | bool; from Focusable |
| `LanguageDirection` | `element.languageDirection` | `"languageDirection"` | LanguageDirection enum |
| `OnFocusIn` | `RegisterCallback<FocusInEvent>` | `"onFocusIn"` | Bubbling focus event |
| `OnFocusOut` | `RegisterCallback<FocusOutEvent>` | `"onFocusOut"` | Bubbling blur event |
| `OnDragUpdated` | `RegisterCallback<DragUpdatedEvent>` | `"onDragUpdated"` | |
| `OnDragPerform` | `RegisterCallback<DragPerformEvent>` | `"onDragPerform"` | |
| `OnDragExited` | `RegisterCallback<DragExitedEvent>` | `"onDragExited"` | |
| `OnGeometryChanged` | `RegisterCallback<GeometryChangedEvent>` | `"onGeometryChanged"` | Layout/size callback |
| `OnAttachToPanel` | `RegisterCallback<AttachToPanelEvent>` | `"onAttachToPanel"` | Mount lifecycle |
| `OnDetachFromPanel` | `RegisterCallback<DetachFromPanelEvent>` | `"onDetachFromPanel"` | Unmount lifecycle |

- [ ] Add `"visible"` key handling to `PropsApplier.Apply` and `ApplyDiff` (`element.visible = (bool)val`).
- [ ] Add `"enabled"` key handling — **must call `element.SetEnabled((bool)val)`**, not assign a property.
- [ ] Add `"tooltip"` and `"viewDataKey"` string assignments.
- [ ] Add `"delegatesFocus"` and `"languageDirection"` assignments.
- [ ] Add `RegisterCallback` for 9 new event keys above.
- [ ] Add corresponding `UnregisterCallback` in `ApplyDiff` for all new event keys.
- [ ] Add handling in `VisualElementAdapter` if needed.
- [ ] Compile + smoke test all new keys.

---

### Phase 7 — Tests + Version Bump

**Status:** 🔲 NOT STARTED

- [ ] Add/update unit tests for `VisualElementProps` construction and `PropsApplier.Apply`.
- [ ] Add/update `PropsResolver` tests confirming `V.VisualElement` now classifies as `BuiltinTyped`.
- [ ] Verify UITKX emit test for `<VisualElement style={...}>` produces correct C# output.
- [ ] Verify `<Box>` behaviour unchanged (regression check for typed layout container).
- [ ] Bump `package.json` version (semver minor — **v1.1.0** — because this widens the public typed API surface).
- [ ] Update `CHANGELOG` / `README` if applicable.

---

### Phase 8 — Migrate `JustStayOn` game UI

**Status:** 🔲 NOT STARTED  
**Prerequisite:** Phase 7 complete — package published at v1.1.0 and all tests green.

**Target:** `C:\Yanivs\GameDev\JustStayOn\Assets\UI`

- [ ] Update the `JustStayOn` project's ReactiveUITK package reference to v1.1.0.
- [ ] Run grep for `V\.VisualElement\(` in `C:\Yanivs\GameDev\JustStayOn\Assets\UI` to enumerate all call sites.
- [ ] Apply same mechanical migration pattern as Phase 4:
  - `V.VisualElement(new Dictionary<string, object> { { "style", s } })` → `V.VisualElement(new VisualElementProps { Style = s })`
  - `V.VisualElement(someStyle, key)` → `V.VisualElement(new VisualElementProps { Style = someStyle }, key)`
- [ ] Replace any `<Box>` workarounds in `.uitkx` files that were intended to be `<VisualElement>`.
- [ ] Compile the game project. Expect zero errors.
- [ ] Smoke test in Play Mode — verify no visual regressions.

---

## 6. Migration Notes

### Why not just add a `VisualElementProps` overload without removing the dict overload?

The dict overload being `public` means `PropsResolver` still sees it first (or sees both and picks one). Keeping it public with the same name as the typed overload creates ambiguity. Making it `private` (renamed to `VisualElement_Dict`) is cleaner and enforces the migration.

### Why `VisualElement` and not just using `Box` everywhere?

`Box` and `VisualElement` are different UIToolkit DOM element types:
- `VisualElement` — the primitive base, no default visual styling
- `Box` — a subclass with default background/border behaviour

They are not semantically interchangeable. Code that currently calls `V.VisualElement` expects a `VisualElement` in the DOM, not a `Box`. A typed `VisualElementProps` gives the correct element type with the correct typed API.

### `V.VisualElement(Style, ...)` convenience overload

The `Style`-only overload (`VisualElement(Style style, ...)`) is removed in Phase 3c. Its only purpose was to avoid the dict syntax for the common case — `new VisualElementProps { Style = s }` serves the same purpose with better discoverability and type-safety.

### Call sites in UITKX files

Any `.uitkx` file currently using `<Box>` as a workaround for the missing typed `<VisualElement>` can switch back:

```uitkx
// Before (workaround):
<Box style={new Style { (FlexDirection, "column") }}>

// After (correct semantic type):
<VisualElement style={new Style { (FlexDirection, "column") }}>
```

Only do this where the original intent was a plain `VisualElement` container. `<Box>` should remain where a `Box` (with its visual defaults) is actually the intended element type.

---

## 7. Summary

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Create `BaseProps.cs` (Unity 6.2 full coverage) + `VisualElementProps.cs` | 🔲 |
| 2 | Migrate all existing Props classes to inherit `BaseProps` | 🔲 |
| 3 | Add typed `V.VisualElement` overload; privatize dict overload | 🔲 |
| 4 | Migrate ~100 `V.VisualElement(dict)` call sites | 🔲 |
| 5 | Delete private dict overload; finalize | 🔲 |
| 6 | Wire 14 new `PropsApplier` keys (`visible`, `enabled`, `tooltip`, `viewDataKey`, lifecycle events, …) | 🔲 |
| 7 | Tests + v1.1.0 version bump | 🔲 |
| 8 | Migrate `JustStayOn` game UI (`C:\Yanivs\GameDev\JustStayOn\Assets\UI`) | 🔲 |
