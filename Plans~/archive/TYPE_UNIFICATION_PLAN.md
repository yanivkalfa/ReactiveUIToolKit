# Type Unification Plan
## ReactiveUITK — React-aligned public type system

**Goal:** Replace every raw `Func<>`, `Action<>`, `EventCallback<>`, `Delegate`, and
`Hooks.MutableRef<T>` in the public API with named, self-documenting types that follow
the React / DefinitelyTyped convention:
- One generic base per concern (e.g. `UIEventHandler<E>`)
- Named concrete delegates per **event category**, not per prop
- Predictable derivation: if you see prop name `onChange` you know the type is
  `ChangeEventHandler<T>` without looking it up

The new types live in **`Shared/Core/ReactiveTypes.cs`** (already written ✅) and are
automatically in scope in every `.uitkx` file because the emitter always injects
`using ReactiveUITK.Core;`.

---

## Phase Status

| Phase | Description | Status |
|-------|-------------|--------|
| 0 | Write `ReactiveTypes.cs` — all new types | ✅ Done |
| 1 | `SyntheticEvents.cs` — rename old classes, keep as `[Obsolete]` | ✅ Done |
| 2 | `Hooks.cs` — rename `MutableRef<T>` → `Ref<T>` | ✅ Done |
| 3 | `BaseProps.cs` — swap all 22 event prop types, add `OnInput` | ✅ Done |
| 4 | All other `Shared/Props/Typed/*.cs` files | ✅ Done |
| 5 | Promote `SortedColumnDef` + `ColumnLayoutState` to Core | ✅ Done |
| 6 | `PropsApplier.cs` — update `SyntheticEvent.Create()` call site | ✅ Done |
| 7 | SourceGenerator — `PropsResolver.cs` + `UitkxDiagnostics.cs` | ✅ Done |
| 8 | Sample `.uitkx` files — update usage | ✅ Done |
| 9 | Sample C# files — update usage | ✅ Done |
| 10 | Run full test suite — verify 98/98 pass | ✅ Done — 98/98 passed |
| 11 | Delete `SyntheticEvents.cs` (old file, after all [Obsolete] are gone) | ✅ Done |
| 12 | Game project — update usage | ✅ Done |

---

## Complete Type Origin Table

Every row shows: old type → new type, which props / files used it, and why it changed.

### Ref

| Old Type | New Type | Property `.` | Used In | Reason |
|---|---|---|---|---|
| `Hooks.MutableRef<T>` | `Ref<T>` | `.Value` → `.Current` | `Hooks.UseRef()`, all `.uitkx` ref props, SourceGenerator | React 19 `ref.current` parity; de-nest from Hooks static class |

> `Ref<T>.Value` is kept as `[Obsolete]` alias pointing to `.Current` so the
> game and any existing C# code still compiles with a warning during migration.

---

### Event data classes

| Old Class | New Class | Hierarchy | Covers Props | New Fields vs Old |
|---|---|---|---|---|
| `SyntheticEvent` | `ReactiveEvent` | base | all events | Same fields |
| `SyntheticPointerEvent` | `ReactivePointerEvent` | `: ReactiveEvent` | onClick, onPointer* | Same fields |
| `SyntheticWheelEvent` | `ReactiveWheelEvent` | `: ReactivePointerEvent` | onWheel, onScroll | Same fields |
| `SyntheticKeyboardEvent` | `ReactiveKeyboardEvent` | `: ReactiveEvent` | onKeyDown, onKeyUp | Same fields |
| *(none)* | `ReactiveFocusEvent` | `: ReactiveEvent` | onFocus, onBlur, onFocusIn, onFocusOut | + `RelatedTarget` |
| *(none)* | `ReactiveDragEvent` | `: ReactiveEvent` | onDragEnter/Leave/Updated/Perform/Exited | No extra fields (drag events have no extra UIElements data) |
| *(none)* | `ReactiveGeometryEvent` | `: ReactiveEvent` | onGeometryChanged | + `OldRect`, `NewRect` |
| *(none)* | `ReactivePanelEvent` | `: ReactiveEvent` | onAttachToPanel, onDetachFromPanel | + `Panel` |
| `SyntheticChangeEvent` | **removed** | — | — | PropsApplier used this internally to extract `newValue`/`previousValue`; replace with direct `ChangeEvent<T>` in `ChangeEventHandler<T>` |

The old `Synthetic*` classes will be kept in a **renamed** `SyntheticEvents.cs` (or
left in place) with `[Obsolete]` attributes pointing users to the new names. The file
can be deleted once Phase 12 (game) is complete.

---

### Event-handler delegates

These are all in `ReactiveUITK.Core` (auto-imported in `.uitkx`).

| New Delegate | Parameters | Covers Props | Origin — unified from |
|---|---|---|---|
| `UIEventHandler<E>` | `(E e) where E : ReactiveEvent` | *(library extensibility)* | New — generic base, React parity `EventHandler<E>` |
| `PointerEventHandler` | `(ReactivePointerEvent e)` | onClick, onPointerDown/Up/Move/Enter/Leave | `Action` (onClick) + `EventCallback<Pointer*Event>` (5 props) |
| `WheelEventHandler` | `(ReactiveWheelEvent e)` | onWheel, onScroll | `EventCallback<WheelEvent>` (2 props) |
| `KeyboardEventHandler` | `(ReactiveKeyboardEvent e)` | onKeyDown, onKeyUp | `EventCallback<KeyDownEvent>`, `EventCallback<KeyUpEvent>` |
| `FocusEventHandler` | `(ReactiveFocusEvent e)` | onFocus, onBlur, onFocusIn, onFocusOut | `EventCallback<FocusEvent>`, `EventCallback<BlurEvent>`, `EventCallback<FocusInEvent>`, `EventCallback<FocusOutEvent>` |
| `DragEventHandler` | `(ReactiveDragEvent e)` | onDragEnter, onDragLeave, onDragUpdated, onDragPerform, onDragExited | `EventCallback<DragEnterEvent>`, `EventCallback<DragLeaveEvent>`, etc. |
| `GeometryChangedEventHandler` | `(ReactiveGeometryEvent e)` | onGeometryChanged | `EventCallback<GeometryChangedEvent>` |
| `PanelLifecycleEventHandler` | `(ReactivePanelEvent e)` | onAttachToPanel, onDetachFromPanel | `EventCallback<AttachToPanelEvent>`, `EventCallback<DetachFromPanelEvent>` |
| `ChangeEventHandler<T>` | `(ChangeEvent<T> e)` | onChange on all typed inputs | `Action<ChangeEvent<T>>` (15+ props files) |
| `InputEventHandler` | `(string newValue)` | onInput | **Missing from BaseProps** — existed only in PropsApplier line 1877 |

---

### Render-function delegates

| New Delegate | Signature | Covers Props | Origin — unified from |
|---|---|---|---|
| `RowRenderer` | `(int index, object item) → VirtualNode` | `Row` (ListView, TreeView), `Cell` (MultiColumn ColumnDef) | `Func<int, object, VirtualNode>` in 4 props files — **unified** since signature is identical |
| `ContentRenderer` | `() → VirtualNode` | `Content` (TabDef) | `Func<VirtualNode>` in TabViewProps.TabDef |
| `ItemFactory` | `() → VisualElement` | `MakeItem` (ListView) | `Func<VisualElement>` in ListViewProps |
| `ItemBinder` | `(VisualElement, int)` | `BindItem`, `UnbindItem` (ListView) | `Action<VisualElement, int>` in ListViewProps |

---

### Widget-event delegates

| New Delegate | Parameters | Covers Props | Origin — unified from |
|---|---|---|---|
| `TreeExpansionEventHandler` | `(TreeViewExpansionChangedArgs)` | `ItemExpandedChanged` (TreeViewProps) | `Delegate` (runtime-checked) → `Action<TreeViewExpansionChangedArgs>` |
| `TabIndexEventHandler` | `(int index)` | `SelectedIndexChanged` (TabViewProps) | `Delegate` → `Action<int>` or `StateSetter<int>` |
| `TabChangeEventHandler` | `(Tab tab)` | `ActiveTabChanged` single-arg (TabViewProps) | `Delegate` → `Action<Tab>` |
| `TabChangedEventHandler` | `(Tab previous, Tab next)` | `ActiveTabChanged` two-arg (TabViewProps) | `Delegate` → `Action<Tab, Tab>` |
| `ColumnSortEventHandler` | `(List<SortedColumnDef>)` | `ColumnSortingChanged` (MultiColumn*) | `Delegate` → `Action<List<SortedColumnDef>>` |
| `ColumnLayoutEventHandler` | `(ColumnLayoutState)` | `ColumnLayoutChanged` (MultiColumn*) | `Delegate` → `Action<ColumnLayoutState>` |

> ✅ **Decision confirmed:** `SortedColumnDef` and `ColumnLayoutState` are promoted
> to `ReactiveUITK.Core` in Phase 5. Keeping them in `Props/Typed` would create an
> inverted dependency (Core delegates referencing a higher-level layer). Confirmed correct.
>
> `ColumnSortEventHandler` and `ColumnLayoutEventHandler` use `SortedColumnDef`
> and `ColumnLayoutState` which are currently nested in `MultiColumnListViewProps`.
> **Phase 5** promotes these two classes to top-level in `ReactiveUITK.Core` so that
> the delegates in `ReactiveTypes.cs` can reference them strongly.

---

### Misc delegates

| New Delegate | Parameters | Covers Props | Origin |
|---|---|---|---|
| `MenuBuilderHandler` | `(DropdownMenu)` | `PopulateMenu` (ToolbarMenuProps) | `Action<DropdownMenu>` |
| `ErrorEventHandler` | `(Exception)` | `OnError` (ErrorBoundaryProps) | `Action<Exception>` |

> `OnGUI` (IMGUIContainerProps) stays as `Action` — it is a pure IMGUI call with
> no event data. Aliasing it adds no information.
> `OnItem` (ToolbarBreadcrumbsProps) stays as `Action<int>` — the index alone is
> insufficient abstraction surface; it matches its `TabIndexEventHandler` sibling
> in semantics but is in a different component family.

---

## Phase Details

---

### Phase 0 — `Shared/Core/ReactiveTypes.cs` ✅ DONE

**File created:** `Shared/Core/ReactiveTypes.cs`

Contains everything in the Origin Table above. Self-documenting with XML doc comments,
`// Origin:` annotations on every type.

Pending fix in this file (after Phase 5):
- Replace `ColumnSortEventHandler(IList<object>)` with `ColumnSortEventHandler(List<SortedColumnDef>)`
- Replace `ColumnLayoutEventHandler(object)` with `ColumnLayoutEventHandler(ColumnLayoutState)`

---

### Phase 1 — `Shared/Core/SyntheticEvents.cs` ✅ DONE

**What to do:**
- Add `[Obsolete]` on `SyntheticEvent`, `SyntheticPointerEvent`, `SyntheticWheelEvent`,
  `SyntheticKeyboardEvent`, `SyntheticChangeEvent` pointing to the new `Reactive*` names
- Update `SyntheticEvent.Create()` to delegate to `ReactiveEvent.Create()` (one-liner)
- Keep the file intact — do NOT delete until Phase 12 is complete

**Example:**
```csharp
[Obsolete("Use ReactiveEvent instead.")]
public class SyntheticEvent : ReactiveEvent
{
    public SyntheticEvent(EventBase evt) : base(evt) { }
}
```

---

### Phase 2 — `Shared/Core/Hooks.cs`  (line 26) ✅ DONE

**What to do:**
- Mark `MutableRef<T>` as `[Obsolete("Use Ref<T> instead.")]`
- Change `UseRef<T>()` return type from `MutableRef<T>` to `Ref<T>`
- Update all internal `new MutableRef<T>` allocations in `UseRef` body to `new Ref<T>`
- Update cast `(MutableRef<T>)state.HookStates[...]` to `(Ref<T>)...`

**Lines affected:** 26, 1093, 1099, 1105, 1107

**Note:** `MutableRef<T>` stays as a type in the file (marked obsolete) so game code
still compiles. `Ref<T>.Value` obsolete alias ensures `inputRef?.Value?.value` patterns
compile with a deprecation warning.

---

### Phase 3 — `Shared/Props/Typed/BaseProps.cs` ✅ DONE

**What to do:** Replace all 22 event properties + add the missing `OnInput`.

| Property | Old Type | New Type |
|---|---|---|
| `OnClick` | `Action` | `PointerEventHandler` |
| `OnPointerDown` | `EventCallback<PointerDownEvent>` | `PointerEventHandler` |
| `OnPointerUp` | `EventCallback<PointerUpEvent>` | `PointerEventHandler` |
| `OnPointerMove` | `EventCallback<PointerMoveEvent>` | `PointerEventHandler` |
| `OnPointerEnter` | `EventCallback<PointerEnterEvent>` | `PointerEventHandler` |
| `OnPointerLeave` | `EventCallback<PointerLeaveEvent>` | `PointerEventHandler` |
| `OnWheel` | `EventCallback<WheelEvent>` | `WheelEventHandler` |
| `OnScroll` | `EventCallback<WheelEvent>` | `WheelEventHandler` |
| `OnFocus` | `EventCallback<FocusEvent>` | `FocusEventHandler` |
| `OnBlur` | `EventCallback<BlurEvent>` | `FocusEventHandler` |
| `OnFocusIn` | `EventCallback<FocusInEvent>` | `FocusEventHandler` |
| `OnFocusOut` | `EventCallback<FocusOutEvent>` | `FocusEventHandler` |
| `OnKeyDown` | `EventCallback<KeyDownEvent>` | `KeyboardEventHandler` |
| `OnKeyUp` | `EventCallback<KeyUpEvent>` | `KeyboardEventHandler` |
| *(missing)* | — | `InputEventHandler OnInput` — **ADD** |
| `OnDragEnter` | `EventCallback<DragEnterEvent>` | `DragEventHandler` |
| `OnDragLeave` | `EventCallback<DragLeaveEvent>` | `DragEventHandler` |
| `OnDragUpdated` | `EventCallback<DragUpdatedEvent>` | `DragEventHandler` |
| `OnDragPerform` | `EventCallback<DragPerformEvent>` | `DragEventHandler` |
| `OnDragExited` | `EventCallback<DragExitedEvent>` | `DragEventHandler` |
| `OnGeometryChanged` | `EventCallback<GeometryChangedEvent>` | `GeometryChangedEventHandler` |
| `OnAttachToPanel` | `EventCallback<AttachToPanelEvent>` | `PanelLifecycleEventHandler` |
| `OnDetachFromPanel` | `EventCallback<DetachFromPanelEvent>` | `PanelLifecycleEventHandler` |

`ToDictionary()` in BaseProps marshals these props into the `Dictionary<string, object>`
passed to PropsApplier. Since the new types are still delegates, PropsApplier continues
to work without changes at the marshalling layer.

---

### Phase 4 — All other `Shared/Props/Typed/*.cs` files ✅ DONE

**Files and specific changes:**

| File | Property | Old Type | New Type |
|---|---|---|---|
| `ColorFieldProps.cs` | `OnChange` | `Action<ChangeEvent<Color>>` | `ChangeEventHandler<Color>` |
| `DoubleFieldProps.cs` | `OnChange` | `Action<ChangeEvent<double>>` | `ChangeEventHandler<double>` |
| `DropdownFieldProps.cs` | `OnChange` | `Action<ChangeEvent<string>>` | `ChangeEventHandler<string>` |
| `FloatFieldProps.cs` | `OnChange` | `Action<ChangeEvent<float>>` | `ChangeEventHandler<float>` |
| `FoldoutProps.cs` | `OnChange` | `Action<ChangeEvent<bool>>` | `ChangeEventHandler<bool>` |
| `IntegerFieldProps.cs` | `OnChange` | `Action<ChangeEvent<int>>` | `ChangeEventHandler<int>` |
| `LongFieldProps.cs` | `OnChange` | `Action<ChangeEvent<long>>` | `ChangeEventHandler<long>` |
| `SliderProps.cs` | `OnChange` | `Action<ChangeEvent<float>>` | `ChangeEventHandler<float>` |
| `SliderIntProps.cs` | `OnChange` | `Action<ChangeEvent<int>>` | `ChangeEventHandler<int>` |
| `ToolbarToggleProps.cs` | `OnChange` | `Action<ChangeEvent<bool>>` | `ChangeEventHandler<bool>` |
| `ToolbarPopupSearchFieldProps.cs` | `OnChange` | `Action<ChangeEvent<string>>` | `ChangeEventHandler<string>` |
| `ToolbarSearchFieldProps.cs` | `OnChange` | `Action<ChangeEvent<string>>` | `ChangeEventHandler<string>` |
| `UnsignedIntegerFieldProps.cs` | `OnChange` | `Action<ChangeEvent<uint>>` | `ChangeEventHandler<uint>` |
| `UnsignedLongFieldProps.cs` | `OnChange` | `Action<ChangeEvent<ulong>>` | `ChangeEventHandler<ulong>` |
| `Vector2FieldProps.cs` | `OnChange` | `Action<ChangeEvent<Vector2>>` | `ChangeEventHandler<Vector2>` |
| `Vector3FieldProps.cs` | `OnChange` | `Action<ChangeEvent<Vector3>>` | `ChangeEventHandler<Vector3>` |
| `Vector4FieldProps.cs` | `OnChange` | `Action<ChangeEvent<Vector4>>` | `ChangeEventHandler<Vector4>` |
| `ToolbarMenuProps.cs` | `PopulateMenu` | `Action<DropdownMenu>` | `MenuBuilderHandler` |
| `ErrorBoundaryProps.cs` | `OnError` | `Action<Exception>` | `ErrorEventHandler` |
| `ListViewProps.cs` | `Row` | `Func<int, object, VirtualNode>` | `RowRenderer` |
| `ListViewProps.cs` | `MakeItem` | `Func<VisualElement>` | `ItemFactory` |
| `ListViewProps.cs` | `BindItem` | `Action<VisualElement, int>` | `ItemBinder` |
| `ListViewProps.cs` | `UnbindItem` | `Action<VisualElement, int>` | `ItemBinder` |
| `TreeViewProps.cs` | `Row` | `Func<int, object, VirtualNode>` | `RowRenderer` |
| `TreeViewProps.cs` | `ItemExpandedChanged` | `Delegate` | `TreeExpansionEventHandler` |
| `TabViewProps.cs` | `SelectedIndexChanged` | `Delegate` | `TabIndexEventHandler` |
| `TabViewProps.cs` | `ActiveTabChanged` | `Delegate` | `TabChangeEventHandler` or `TabChangedEventHandler` |
| `TabViewProps.cs` (TabDef) | `Content` | `Func<VirtualNode>` | `ContentRenderer` |
| `MultiColumnListViewProps.cs` (ColumnDef) | `Cell` | `Func<int, object, VirtualNode>` | `RowRenderer` |
| `MultiColumnListViewProps.cs` | `ColumnSortingChanged` | `Delegate` | `ColumnSortEventHandler` |
| `MultiColumnListViewProps.cs` | `ColumnLayoutChanged` | `Delegate` | `ColumnLayoutEventHandler` |
| `MultiColumnTreeViewProps.cs` (ColumnDef) | `Cell` | `Func<int, object, VirtualNode>` | `RowRenderer` |
| `MultiColumnTreeViewProps.cs` | `ColumnSortingChanged` | `Delegate` | `ColumnSortEventHandler` |
| `MultiColumnTreeViewProps.cs` | `ColumnLayoutChanged` | `Delegate` | `ColumnLayoutEventHandler` |

**`ToDictionary()` bodies** — for the `Delegate`-typed props (TabView, TreeView,
MultiColumn), the `ToDictionary()` method today does runtime type-checking with
`is Action<int>`, `is Action<Tab>`, etc. to route into the right key. After the
change, there is still a need to handle `StateSetter<T>` (which users can also
pass). The updated `ToDictionary()` uses the new concrete delegate type directly
and handles `StateSetter<T>` as before.

---

### Phase 5 — Promote `SortedColumnDef` + `ColumnLayoutState` to Core ✅ DONE

Currently both classes are nested inside `MultiColumnListViewProps`. To allow
`ColumnSortEventHandler` and `ColumnLayoutEventHandler` in `ReactiveTypes.cs` to
use them with strong typing they must live in `ReactiveUITK.Core`.

**What to do:**
1. Move `SortedColumnDef` class body to its own file `Shared/Core/ColumnTypes.cs`
   (or inline into `ReactiveTypes.cs`). Keep `SortDirection` in place (UIElements type).
2. Move `ColumnLayoutState` class body to same file.
3. In `MultiColumnListViewProps.cs` and `MultiColumnTreeViewProps.cs`, replace the
   nested class definitions with `using` aliases or remove them (they now come from Core).
4. Update `ReactiveTypes.cs` — replace weak `IList<object>` / `object` parameter types
   in `ColumnSortEventHandler` and `ColumnLayoutEventHandler` with the promoted types.

---

### Phase 6 — `Shared/Props/PropsApplier.cs` ✅ DONE

**One change only:** The factory call that creates synthetic event objects.

Find the call (approximately line 1880 area):
```csharp
var se = SyntheticEvent.Create(evt);
```
Change to:
```csharp
var se = ReactiveEvent.Create(evt);
```

PropsApplier dispatches by inspecting `del.Method.GetParameters()[0].ParameterType`
at runtime, so it already handles any delegate signature without knowing the concrete
type at compile time. No other changes needed here.

---

### Phase 7 — SourceGenerator ✅ DONE

**File 1: `SourceGenerator~/ReactiveUITK.SourceGenerator/PropsResolver.cs`**

Lines 172-176 — `IsMutableRefTypeName()` method:
```csharp
// OLD
return stripped.StartsWith("Hooks.MutableRef<", StringComparison.Ordinal)
    || stripped.StartsWith("ReactiveUITK.Core.Hooks.MutableRef<", StringComparison.Ordinal);

// NEW — support both old name (grace period) and new name
return stripped.StartsWith("Ref<", StringComparison.Ordinal)
    || stripped.StartsWith("ReactiveUITK.Core.Ref<", StringComparison.Ordinal)
    || stripped.StartsWith("Hooks.MutableRef<", StringComparison.Ordinal)           // grace period
    || stripped.StartsWith("ReactiveUITK.Core.Hooks.MutableRef<", StringComparison.Ordinal); // grace period
```

Line 256 — `IsRoslynMutableRefType()`:
```csharp
// OLD
return string.Equals(def.Name, "MutableRef", StringComparison.Ordinal)

// NEW
return string.Equals(def.Name, "Ref", StringComparison.Ordinal)
    || string.Equals(def.Name, "MutableRef", StringComparison.Ordinal); // grace period
```

**File 2: `SourceGenerator~/ReactiveUITK.SourceGenerator/UitkxDiagnostics.cs`**

Lines 283-285, 298, 303-311: Update all occurrences of `Hooks.MutableRef<T>` in
diagnostic message strings to say `Ref<T>` (primary) with a note about the old name.

**File 3: `SourceGenerator~/ReactiveUITK.SourceGenerator/EmitterTests.cs`** (test file)

All test `.uitkx` strings using `Hooks.MutableRef<object>?` → update to `Ref<object>?`
so tests reflect the new API. The old form still passes (grace period in PropsResolver)
so tests can be updated incrementally.

---

### Phase 8 — Sample `.uitkx` files ✅ DONE

The following sample files contain types that need updating.
Check each one for: `Hooks.MutableRef`, `Action<ChangeEvent<`, `EventCallback<`,
`Func<int, object,`, `Action<TreeViewExpansion`, `Action<MultiColumn`.

| File | Change needed |
|---|---|
| `RefChild.uitkx` | `Hooks.MutableRef<TextField>` → `Ref<TextField>`, `Hooks.MutableRef<Label>` → `Ref<Label>`, `.Value?.value` → `.Current?.value` |
| `RefForwardingDemoFunc.uitkx` | Same ref changes; check `Hooks.UseRef` call sites |
| `ShowcaseExtrasPanel.uitkx` | `Action<ChangeEvent<bool>>` → `ChangeEventHandler<bool>`, `Action<ChangeEvent<int>>` → `ChangeEventHandler<int>` |
| `ShowcaseListTabsSection.uitkx` | `Action<MultiColumnListViewProps.ColumnLayoutState>` → `ColumnLayoutEventHandler`, `Action<int>` for count → leave as-is (not an event prop, it's a user callback) |
| `ShowcaseNewComponentsPanel.uitkx` | `Action<ChangeEvent<bool/float/int/string>>` → `ChangeEventHandler<T>` |
| `ShowcaseTopBar.uitkx` | `Action<ChangeEvent<string>>` → `ChangeEventHandler<string>` |
| `ShowcaseTreeTabsSection.uitkx` | `Action<TreeViewExpansionChangedArgs>` → `TreeExpansionEventHandler`, `Action<MultiColumnTreeViewProps.ColumnLayoutState>` → `ColumnLayoutEventHandler` |
| `SyntheticEventDemoFunc.uitkx` | `Action<SyntheticPointerEvent>` → `PointerEventHandler`, `Action<SyntheticWheelEvent>` → `WheelEventHandler`, `SyntheticEvent evt` → `ReactiveEvent evt` |
| `TabTreeDemoFunc.uitkx` | `Action<TreeViewExpansionChangedArgs>` → `TreeExpansionEventHandler` |

Also update comments in `RefChild.uitkx` and `RefForwardingDemoFunc.uitkx` that
mention "MutableRef" by name.

---

### Phase 9 — Sample C# files ✅ DONE

Run a global search for the following patterns in `Samples/`:
```
Hooks.MutableRef
Action<ChangeEvent<
EventCallback<
Func<int, object, VirtualNode>
Action<TreeViewExpansion
SyntheticPointerEvent
SyntheticWheelEvent
SyntheticKeyboardEvent
SyntheticEvent
```
Update each hit using the mapping from the Origin Table.

Also check any C# demo class that creates props objects directly:
```csharp
// OLD
var props = new ListViewProps { Row = (i, item) => ... };

// NEW — same lambda, type is inferred from RowRenderer delegate
var props = new ListViewProps { Row = (i, item) => ... };
// (no change needed at call site when using lambdas — C# infers the delegate type)
```

---

### Phase 10 — Full test suite ✅ DONE — 98/98 passed

Run all 98 tests. Expected: 98/98 pass.

Focus areas:
- Ref routing tests in `EmitterTests.cs` (UITKX0020, UITKX0021)
- Any test that constructs props objects with old delegate types
- PropsApplier dispatch tests (event handler invocation)

---

### Phase 11 — Delete `SyntheticEvents.cs` ✅ DONE

Once the game (Phase 12) is fully migrated:
- Confirm zero usages of `SyntheticEvent`, `SyntheticPointerEvent`, `SyntheticWheelEvent`,
  `SyntheticKeyboardEvent`, `SyntheticChangeEvent` anywhere in the codebase
- Delete `Shared/Core/SyntheticEvents.cs`
- Remove grace-period branches from `PropsResolver.cs`

---

### Phase 12 — Game project ✅ DONE

> **Game project UI root:** `c:\Yanivs\GameDev\JustStayOn\Assets\UI`

**Changes applied:**

1. **`Button.cs`** — Wrapped `Action resolvedOnClick` into `PointerEventHandler` at `ButtonProps.OnClick` assignment: `_ => resolvedOnClick()`
2. **`DialogHost.cs`** — Changed `(Action)(() => ...)` cast to `_ => ...` on `VisualElementProps.OnClick`
3. **`ActionButtons.util.cs`** — Changed `(Action)(() => { ... })` cast to `_ => { ... }` on `VisualElementProps.OnClick`
4. **`ToggleSettingRow.cs`** — Changed `() => handleSelect(0/1)` to `_ => handleSelect(0/1)` on `ButtonProps.OnClick`
5. **`ConsentDialogPreset.cs`** — Changed `() => context.Resolve(false/true)` to `_ => ...` on `ButtonProps.OnClick`
6. **`HomePage.cs`** — Changed `() => setRunVersion.Set(...)` to `_ => ...` on `ButtonProps.OnClick`
7. **`Sidebar.cs`** — Shortened `ReactiveUITK.Props.Typed.Style` → `Style` (4 occurrences; `using ReactiveUITK.Props.Typed` already present)
8. **`DialogPresets.cs`** — Added `using ReactiveUITK.Props.Typed;` and `using static ReactiveUITK.Props.Typed.StyleKeys;`; shortened all qualified `Style` and `StyleKeys.*` references
9. **`GameOverPageSidebar.cs`** — Added `using ReactiveUITK.Core;`; shortened `ReactiveUITK.Core.VirtualNode` → `VirtualNode`
10. **`MetricDisplay.cs`** — Shortened `ReactiveUITK.Core.Fiber.FiberConfig.EnableFiberLogging` → `Fiber.FiberConfig.EnableFiberLogging`

---

## Implementation notes

### PropsApplier dispatch — no structural changes needed

`PropsApplier.InvokeHandler()` dispatches by inspecting the delegate's parameter
types at runtime:
```csharp
del.Method.GetParameters()[0].ParameterType
```
Because our new delegates (`PointerEventHandler`, `WheelEventHandler`, ...) have
`Reactive*Event` as parameter types, and `ReactiveEvent.Create()` returns those
exact subtypes, the existing dispatch routing continues to work correctly without
any `if/else` changes inside PropsApplier. **Only the `SyntheticEvent.Create()` →
`ReactiveEvent.Create()` call site in Phase 6 is required.**

### `StateSetter<T>` interop — preserved

Multiple `ToDictionary()` implementations check `is Hooks.StateSetter<T> setter`
before checking the concrete delegate type (e.g. `is Action<int>`). The new
concrete delegate types are checked alongside — `StateSetter<T>` still binds
correctly via its existing `.ToValueAction()` path.

### `.uitkx` files — naming convention after this refactor

```uitkx
// Every type is now derivable from the prop name — no lookup required:
component Example(
    Ref<TextField>? inputRef       = null,  // was Hooks.MutableRef<TextField>
    PointerEventHandler? onClick   = null,  // was Action
    KeyboardEventHandler? onKeyDown = null, // was EventCallback<KeyDownEvent>
    ChangeEventHandler<string>? onChange = null, // was Action<ChangeEvent<string>>
    RowRenderer? row               = null,  // was Func<int, object, VirtualNode>
    TreeExpansionEventHandler? onExpanded = null, // was Action<TreeViewExpansionChangedArgs>
)
```
