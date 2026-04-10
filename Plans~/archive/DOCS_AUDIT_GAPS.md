# Documentation Audit — Library vs Docs Gap Analysis

**Audit date:** 2026-04-05
**Auditor:** Automated comprehensive cross-reference of Runtime API surface,
Source Generator, LSP server, UITKX schema, and documentation website.

---

## Methodology

1. Read every public API in `Runtime/`, `Shared/`, `Editor/`
2. Read every diagnostic code in SG, LSP language-lib, and parser
3. Read the UITKX schema (`uitkx-schema.json`)
4. Read every documentation page in `ReactiveUIToolKitDocs~/src/`
5. Cross-referenced all findings to identify gaps

---

## SECTION 1 — Missing Documentation Pages

### 1.1 Events & Input Handling Guide (HIGH PRIORITY)

**What exists in the library:**
- 31 event handlers on `BaseProps` (all elements)
- 7 event data classes: `ReactiveEvent`, `ReactivePointerEvent`, `ReactiveWheelEvent`,
  `ReactiveKeyboardEvent`, `ReactiveFocusEvent`, `ReactiveDragEvent` (editor-only),
  `ReactiveGeometryEvent`, `ReactivePanelEvent`
- 12 delegate types: `PointerEventHandler`, `WheelEventHandler`, `KeyboardEventHandler`,
  `FocusEventHandler`, `DragEventHandler`, `GeometryChangedEventHandler`,
  `PanelLifecycleEventHandler`, `ChangeEventHandler<T>`, `InputEventHandler`,
  `RowRenderer`, `ItemFactory`, `ItemBinder`, `MenuBuilderHandler`, `ErrorEventHandler`

**What's documented:** Only `onClick` shown in Button page example. No comprehensive
event reference exists anywhere.

**Gap:** Need a dedicated "Events & Input Handling" page covering:
- Complete event handler table (all 31 events with delegate types)
- `ReactivePointerEvent` properties (Position, DeltaPosition, Button, ClickCount,
  modifier keys, Pressure, Radius, etc.)
- `ReactiveWheelEvent.Delta` (x=horiz, y=vert, z=depth)
- `ReactiveKeyboardEvent` properties (KeyCode, Character, modifier keys)
- `ReactiveFocusEvent.RelatedTarget`
- `ReactiveGeometryEvent` (OldRect, NewRect)
- `ReactivePanelEvent` (onAttachToPanel / onDetachFromPanel)
- `StopPropagation()` and `PreventDefault()` methods
- `ChangeEvent<T>` polymorphism (bool for Toggle, float for Slider, string for TextField, etc.)
- Editor-only drag events
- Event bubbling behavior
- Code examples for each event category

### 1.2 Hooks Deep-Dive Guide (HIGH PRIORITY)

**What exists in the library:**
- 23 hooks in `Hooks.cs`
- Complex state patterns (UseReducer, functional updaters)
- Dependency array semantics for UseEffect, UseMemo, UseCallback
- Context API (UseContext, ProvideContext) with provider shadowing
- Ref system (UseRef<T>, element ref)
- Stable function helpers (UseStableFunc, UseStableAction, UseStableCallback)
- Animation hooks (UseAnimate, UseTweenFloat)
- Signal integration (UseSignal with optional selector)
- SafeArea hook

**What's documented:** API page lists hook names. HMR page mentions state preservation.
FAQ explains "why hooks at top level". No deep guide exists.

**Gap:** Need a dedicated "Hooks Guide" page covering:

| Hook | Status in Docs | What's Missing |
|------|---------------|----------------|
| `UseState<T>(initial)` | Mentioned in examples | Functional updater pattern `set(prev => next)`, `StateUpdate<T>` union type |
| `UseReducer<TState,TAction>(reducer, initial)` | Name only in API | Full explanation, reducer pattern, dispatch, when to use vs UseState |
| `UseEffect(factory, deps)` | Mentioned briefly | Cleanup function, dependency array rules, empty deps = mount-only, no deps = every render |
| `UseLayoutEffect(factory, deps)` | Not mentioned | When to use vs UseEffect (synchronous before paint) |
| `UseMemo<T>(factory, deps)` | Name only | Memoization semantics, dependency comparison, avoid expensive recomputation |
| `UseCallback<T>(callback, deps)` | Not mentioned | Stable callback identity, when to use |
| `UseRef<T>(initial)` | Mentioned in HMR | Mutable `.Current`, persists across renders, no re-render on change |
| `UseContext<T>(key)` | Name only | Full context API, provider/consumer pattern, invalidation |
| `ProvideContext<T>(key, value)` | Not mentioned | Provider semantics, shadowing, nested providers |
| `UseDeferredValue<T>(value, deps)` | Not mentioned | Deferred rendering, low-priority updates |
| `UseImperativeHandle<T>(factory, deps)` | Not mentioned | Expose imperative API via ref |
| `UseStableFunc<T>` | Not mentioned | Prevent closure re-creation, event handler optimization |
| `UseStableAction<T>` | Not mentioned | Same as above for Action delegates |
| `UseStableCallback` | Not mentioned | Same as above for parameterless callbacks |
| `UseAnimate(tracks, autoplay, deps)` | Brief mention | AnimateTrack structure, easing presets, lifecycle |
| `UseTweenFloat(from,to,dur,ease,delay,onUpdate,onComplete,deps)` | Brief mention | Full signature, use cases |
| `UseSafeArea(tolerance)` | Documented | ✅ OK |
| `UseSignal<T>(signal)` | Documented | ✅ OK |
| `UseSignal<T,TSlice>(signal, selector)` | Documented | ✅ OK |

### 1.3 Portal Component Page (HIGH PRIORITY)

**What exists in the library:**
- `V.Portal(target, fallback, children)` — renders children into a different
  VisualElement target outside the component hierarchy
- `PortalContextKeys` — predefined slot keys (ModalRoot, TooltipRoot, OverlayRoot)
- Schema supports `<Portal target={...}>` with `target` and `fallback` attributes
- Sample components demonstrate portal usage

**What's documented:** Nothing. Zero mention in docs.

**Gap:** Need a "Portal" page covering:
- What portals are and when to use them
- `target` prop (VisualElement reference)
- `fallback` prop (shown when target is null)
- `PortalContextKeys` for well-known slot names
- Modal/tooltip/overlay patterns
- Code example

### 1.4 Suspense Component Page (HIGH PRIORITY)

**What exists in the library:**
- `V.Suspense(ready, fallback, children)` — shows fallback while `isReady()` returns false
  or while `pendingTask` is incomplete
- Schema supports `<Suspense isReady={...} fallback={...} pendingTask={...}>`
- Two modes: `Func<bool> isReady` or `Task pendingTask`

**What's documented:** Nothing. Zero mention in docs.

**Gap:** Need a "Suspense" page covering:
- What Suspense is and when to use it
- `isReady` callback mode
- `pendingTask` async mode
- `fallback` prop (VirtualNode shown while loading)
- Loading state patterns
- Code example

### 1.5 Context API Guide (MEDIUM PRIORITY)

**What exists in the library:**
- `Hooks.UseContext<T>(key)` — consume context value by string key
- `Hooks.ProvideContext<T>(key, value)` — provide context value for subtree
- Nested provider shadowing (inner provider overrides outer)
- `HostContext.SetContextValue()` / `ResolveContext()` internals
- Context dependency tracking and selective invalidation

**What's documented:** Mentioned in FAQ as "context helpers". No standalone page.

**Gap:** Need a "Context API" page covering:
- Provider/consumer pattern
- String-keyed context values
- Type-safe generics
- Nested provider shadowing
- When to use vs Signals vs props drilling
- Code example with theme or auth context

### 1.6 Fragment Syntax (`<>...</>`) (MEDIUM PRIORITY)

**What exists in the library:**
- Parser supports `<>...</>` fragment syntax (empty tag name maps to `V.Fragment`)
- `V.Fragment(key, children)` — invisible wrapper, no DOM element
- Used for returning multiple root elements from control blocks

**What's documented:** Not mentioned anywhere in Language Reference or elsewhere.

**Gap:** Add Fragment syntax to:
- Language Reference "Expressions & Values" section
- Control flow examples showing Fragment for multiple root elements

---

## SECTION 2 — Missing Diagnostic Codes in Docs

### 2.1 UITKX0024 — Control block body missing return (NEW)

**Severity:** Error
**Message:** "Control block body (@if/@for/etc.) must contain 'return (...);'."
**Emitted by:** UitkxParser.cs (ParseControlBlockBody, ParseSwitch)
**Status:** NOT in docs. Needs to be added to Diagnostics page.

### 2.2 UITKX0022 — Asset file not found

**Severity:** Warning
**Message:** "Asset file 'X' not found at resolved path 'Y'."
**Status:** Documented on Assets page. ✅ OK but NOT on Diagnostics page.
Gap: Add to Diagnostics page table.

### 2.3 UITKX0023 — Asset type mismatch

**Severity:** Warning
**Message:** "Asset at 'X' is of type Y, but Asset<Z> was requested."
**Status:** Documented on Assets page. ✅ OK but NOT on Diagnostics page.
Gap: Add to Diagnostics page table.

### 2.4 UITKX0112 — Unused variable in setup code

**Severity:** Hint
**Emitted by:** DiagnosticCodes.cs in language-lib
**Status:** NOT in docs. Needs to be added.

### 2.5 UITKX0120, UITKX0121 — LSP asset validation

**Severity:** Warning
**Emitted by:** DiagnosticsAnalyzer.cs
**Status:** Documented on Assets page. NOT on Diagnostics page.
Gap: Add to Diagnostics page table.

### 2.6 UITKX0200 — Attribute version mismatch

**Severity:** Warning
**Message:** Attribute requires newer Unity version or was removed.
**Emitted by:** DiagnosticsAnalyzer.cs
**Status:** NOT on Diagnostics page. Add it.

### 2.7 UITKX2100–2106 — Function-style component validation (CRITICAL)

These are emitted by DirectiveParser for function-style component issues:

| Code | Severity | Message | Status |
|------|----------|---------|--------|
| UITKX2100 | Error | Non-PascalCase component name | ❌ NOT IN DOCS |
| UITKX2101 | Error | Missing return statement in function-style component | ❌ NOT IN DOCS |
| UITKX2102 | Error | Malformed return (return must return UITKX markup) | ❌ NOT IN DOCS |
| UITKX2104 | Error | Mixed directive/function-style syntax | ❌ NOT IN DOCS |
| UITKX2105 | Error | Invalid function-style declaration | ❌ NOT IN DOCS |
| UITKX2106 | Error | Missing parameter name | ❌ NOT IN DOCS |

---

## SECTION 3 — Outdated Documentation Content

### 3.1 Language Reference — Control Flow Examples

**Issue:** The `CONTROL_FLOW_EXAMPLE` string shows bare markup inside control blocks:
```tsx
@if (isLoggedIn) {
    <Label text="Welcome back!" />
}
```

**Required update:** All examples must show `return (...)` syntax:
```tsx
@if (isLoggedIn) {
    return (<Label text="Welcome back!" />);
}
```

### 3.2 Language Reference — @break / @continue

**Issue:** `@break` and `@continue` are listed as valid control flow directives
in the Markup Control Flow table. These were removed from the grammar and schema.

**Required update:** Remove `@break` and `@continue` rows, or update to note they
are only valid inside `@for` and `@while` loop bodies (which is already in the
Rules & Gotchas section — but the table still lists them as general directives).

### 3.3 Diagnostics Page — UITKX0305 Valid Directives List

**Issue:** The "How to fix" text for UITKX0305 lists `@code` as a valid directive:
> "Valid directives: @if, @else, @for, @foreach, @while, @switch, @case, @default, @break, @continue, @code."

**Required update:** Remove `@code` from the list. It's no longer a valid directive.

### 3.4 Language Reference — Missing `return (...)` in Control Block Docs

**Issue:** The control flow section doesn't mention that each control block body
must wrap its markup in `return (...)`. The function-style section mentions setup
code + return, but control blocks are shown with bare markup.

**Required update:** Add a note and update the example in the "Markup Control Flow"
section to show:
- Each block body requires `return (...);`
- Setup code (var declarations, computations) goes before `return`
- Example showing setup code + return

### 3.5 Diagnostics Page — UITKX0306 Description

**Issue:** The "How to fix" text says "@(expr) in setup code" and references
`@code` blocks:
> "Inline expressions @(...) are only valid inside markup, not in @code blocks."

**Required update:** Change to "not in setup code" (remove `@code` reference).

---

## SECTION 4 — Undocumented Runtime APIs

### 4.1 PropTypes / PropTypeValidator (PUBLIC API — ZERO DOCS)

**What exists:**
```csharp
static class PropTypes
{
    PropTypeDefinition String(name, required)
    PropTypeDefinition Number(name, required)
    PropTypeDefinition Boolean(name, required)
    PropTypeDefinition Enum(name, allowedValues, required)
    PropTypeDefinition InstanceOf<T>(name, required)
    PropTypeDefinition Custom(name, validator, description, required)
}

static class PropTypeValidator
{
    bool Enabled { get; set; }
    void Validate(componentName, props, definitions)
}
```

**Status:** Completely hidden from users. A React-like prop validation system
exists in the public API but has zero documentation, zero mention, zero examples.

**Recommendation:** Add to API Reference page. Consider whether this is a
supported public API or an internal utility. If public, needs a docs section.

### 4.2 HostContext / Environment Dictionary

**What exists:**
- `HostContext.SetContextValue(key, value)` — sets context for component tree
- `HostContext.ResolveContext(key)` — resolves context value
- `HostContext.Environment` — dictionary for scheduling, portal targets, etc.
- Used by `RootRenderer.Initialize(root, env => { ... })`

**Status:** Not documented. The environment setup pattern via RootRenderer.Initialize
callback is not shown anywhere.

### 4.3 SnapshotAssert — Testing Utility

**What exists:**
```csharp
static class SnapshotAssert
{
    Result Compare(VirtualNode expected, VirtualNode actual)
    void AssertEqual(VirtualNode expected, VirtualNode actual)
}
```

**Status:** Undocumented. Used in SG tests. If users should write component tests,
this needs documentation. If internal-only, consider making it internal.

### 4.4 IScheduler.Priority

**What exists:**
```csharp
interface IScheduler
{
    enum Priority { High, Normal, Low, Idle }
    void Enqueue(Action, Priority)
    void BeginBatch() / EndBatch()
    void PumpNow()
}
```

**Status:** Not documented. Users who write custom renderers or advanced scheduling
would need this.

---

## SECTION 5 — Incomplete Documentation Coverage

### 5.1 CssHelpers — No Reference Table

**Library has 164+ public members.** Docs mention CssHelpers as "shortcut values"
with a few examples. No searchable reference table of all members by category.

**Categories missing complete listings:**
- Length units: `Pct()`, `Px()`, `Em()`, `Vh()`
- Flex: `FlexRow`, `FlexColumn`, `FlexRowReverse`, `FlexColumnReverse`, `JustifyCenter`,
  `JustifyStart`, `JustifyEnd`, `JustifySpaceBetween`, `JustifySpaceAround`,
  `JustifySpaceEvenly`, `AlignStart`, `AlignEnd`, `AlignCenter`, `AlignStretch`,
  `AlignBaseline`
- Colors: `ColorWhite`, `ColorBlack`, `ColorRed`, `ColorGreen`, `ColorBlue`,
  `ColorYellow`, `ColorCyan`, `ColorMagenta`, `ColorGray`, `ColorClear`,
  `ColorTransparent`, `Hex()`, `Rgba()`
- Display: `DisplayFlex`, `DisplayNone`, `VisVisible`, `VisHidden`
- Position: `PosAbsolute`, `PosRelative`
- Overflow: `OverflowVisible`, `OverflowHidden`
- Text: `FontBold`, `FontItalic`, `FontBoldItalic`, `FontNormal`, `TextClip`,
  `TextEllipsis`, `TextOverflowStart`, `TextOverflowMiddle`, `TextOverflowEnd`
- WhiteSpace: `WsNormal`, `WsNowrap`, `WsPre`, `WsPreWrap`
- Wrap: `WrapOn`, `WrapOff`, `WrapReverse`
- Keywords: `StyleAuto`, `StyleNone`, `StyleInitial`
- Background: `BgRepeat()`, `BgRepeatNone`, `BgRepeatBoth`, `BgRepeatX/Y`,
  `BgPos()`, `BgPosCenter`, `BgPosTop/Bottom/Left/Right`,
  `BgSize()`, `BgSizeCover`, `BgSizeContain`
- Transform: `Origin()`, `OriginCenter`, `Xlate()`
- Easing: `Easing()`, `EaseDefault`, `EaseLinear`, `EaseIn`, `EaseOut`, `EaseInOut`,
  + 18 sine/cubic/circ/elastic/back/bounce variants
- Enum shortcuts: `PickPosition`, `PickIgnore`, `SelectNone`, `SelectSingle`,
  `SelectMultiple`, `ScrollerAuto`, `ScrollerVisible`, `ScrollerHidden`,
  `DirInherit`, `DirLTR`, `DirRTL`, `SliderHorizontal`, `SliderVertical`,
  `ScrollVertical`, `ScrollHorizontal`, `ScrollBoth`, `ScaleStretch`, `ScaleFit`,
  `ScaleCrop`, `OrientHorizontal`, `OrientVertical`, `SortNone`, `SortDefault`,
  `SortCustom`, `AutoSizeNone`, `AutoSizeBestFit`
- Filters (Unity 6.3+): `FilterBlur()`, `FilterGrayscale()`, `FilterBrightness()`,
  `FilterContrast()`, `FilterSaturate()`, `FilterSepia()`, `FilterHueRotate()`,
  `FilterInvert()`, `FilterOpacity()`, `FilterDropShadow()`

**Recommendation:** The Styling page already has a searchable catalog for StyleKeys
properties. Extend it to include a CssHelpers reference table with all 164+ members.
Or generate the table from the source file.

### 5.2 Style Class — Property Reference

**Library has 70+ typed set-only properties** (Width, Height, FlexGrow, Color,
BackgroundColor, etc.) plus initializer tuple syntax.

**What's documented:** Styling page shows the tuple initializer pattern with
examples. No complete property reference.

**Recommendation:** The Styling page already provides searchable StyleKeys. The
Style typed properties mirror StyleKeys, so this gap is lower priority. Consider
adding a note that Style has typed setters matching StyleKeys.

### 5.3 AnimateTrack — No API Detail

**What exists:**
```csharp
sealed class AnimateTrack
{
    string Property
    object From, To
    float Duration, Delay
    Ease Ease = EaseInOutCubic
    int Repeat
    bool Loop, Yoyo
    float TimeScale = 1f
    Action<float> OnUpdate
    Action OnComplete
}
```

Plus 24 `Ease` enum values.

**What's documented:** API page says "AnimateTrack — helpers for creating animation tracks."
No property reference, no easing reference, no examples.

**Gap:** Need AnimateTrack property documentation — at minimum in the Animate
component page or a dedicated Animation guide.

### 5.4 Ease Enum — Complete Values

**What exists:** 17 values (Linear, EaseInSine, EaseOutSine, EaseInOutSine,
EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic,
EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInExpo, EaseOutExpo, EaseInOutExpo,
EaseInBack, EaseOutBack, EaseInOutBack).

**Status:** Not documented anywhere.

### 5.5 BaseProps Common Properties — No Reference

**What exists:** 16 common properties on ALL elements:
- `name`, `className`, `style`, `ref`, `contentContainer`
- `visible`, `enabled`
- `pickingMode`, `focusable`, `tabIndex`, `delegatesFocus`
- `tooltip`, `viewDataKey`
- `languageDirection`
- `extraProps`

**What's documented:** Not listed anywhere as a group. Individual component pages
show per-element props but not the common base.

**Recommendation:** Add a "Common Props" section to the Components Overview page
or create a BaseProps reference.

---

## SECTION 6 — Component Pages vs Schema Discrepancies

### 6.1 Schema Elements Not in Component Reference

| Schema Element | Has Component Page? | Notes |
|---------------|-------------------|-------|
| Router | ❌ No | Documented on Router tooling page instead |
| Route | ❌ No | Documented on Router tooling page instead |
| Link | ❌ No | Documented on Router tooling page instead |
| Portal | ❌ No | **Not documented anywhere** |
| Suspense | ❌ No | **Not documented anywhere** |
| Fragment | ❌ No | **Not documented anywhere** |
| Spacer | ❌ No | Simple spacer element, might not need a page |
| RequestAnimationFrame | ❌ No | Animation scheduling, might not need a page |
| Host | ❌ No | Internal use, might not need a page |

### 6.2 PropertyInspector — Editor-Only

**Present in:** Component pages, schema
**Gap:** Not clearly marked as editor-only in docs. Could confuse runtime users.

---

## SECTION 7 — Website UX / Structural Issues

### 7.1 No "Guides" Section

The docs have:
- Introduction → Getting Started → Styling → Assets → Components → Concepts
- Tooling: Router, Signals, HMR
- Reference: Language, Diagnostics, Config, Debug, API

**Missing:** A "Guides" section for intermediate/advanced topics:
- Hooks Guide (UseReducer, UseEffect patterns, Context API)
- Events & Input Handling
- Animation Guide
- Portal / Suspense patterns
- Testing components
- Performance optimization
- Integration with existing UI Toolkit code

### 7.2 Roadmap Page Empty

**Current status:** "will be documented in future."
**Recommendation:** Link to the existing Plans~ files or provide a summary.

---

## SECTION 8 — Summary Priority Matrix

### P0 — Critical (Must fix before v0.3.0 release docs)

1. **Update Language Reference control flow examples** — add `return (...)` to all
   control block examples (currently shows bare markup)
2. **Add UITKX0024 to Diagnostics page** — new error code for control blocks
3. **Remove `@code` from UITKX0305 fix text** — outdated reference
4. **Add UITKX2100–2106 to Diagnostics page** — function-style component errors
   completely missing
5. **Add UITKX0022, UITKX0023, UITKX0112, UITKX0120, UITKX0121, UITKX0200
   to Diagnostics page** — asset and version codes only on Assets page

### P1 — High Priority (New doc pages needed)

6. **Events & Input Handling page** — 31 events, 7 event types, 12 delegates
7. **Hooks Deep-Dive page** — 23 hooks with patterns, deps, lifecycle
8. **Portal component page** — render children into external target
9. **Suspense component page** — loading state with fallback
10. **Fragment documentation** — `<>...</>` syntax in Language Reference

### P2 — Medium Priority (Expand existing docs)

11. **Context API guide** — UseContext/ProvideContext patterns
12. **CssHelpers reference table** — all 164+ public members
13. **AnimateTrack + Ease reference** — animation API detail
14. **BaseProps common properties** — shared props on all elements
15. **PropTypes documentation** — decide if public API, then document

### P3 — Low Priority (Nice to have)

16. **HostContext/Environment docs** — advanced RootRenderer setup
17. **SnapshotAssert testing guide** — component testing utilities
18. **IScheduler priority docs** — custom scheduling
19. **Roadmap page content** — link to Plans~ or fill in
20. **ColumnSignature docs** — MultiColumn internal API

---
---

# SECOND PASS — Additional Findings

**Date:** 2026-04-05
**Method:** Read every docs page (.tsx), every sample, V.cs, RootRenderer.cs,
Signal.cs, RouterHooks.cs, propsDocs.ts. Compared exact content against library.

---

## SECTION 9 — Language Reference Page — Specific Issues

### 9.1 Control flow example uses bare markup (CONFIRMED P0)

The `CONTROL_FLOW_EXAMPLE` constant shows:
```tsx
@if (isLoggedIn) {
    <Label text="Welcome back!" />
}
```

This is the **old syntax**. Since v0.3.0, all control block bodies require
`return (...)`:
```tsx
@if (isLoggedIn) {
    return (<Label text="Welcome back!" />);
}
```

The `@foreach` and `@switch` examples in the same block are also bare markup.
ALL must be updated.

### 9.2 @break and @continue listed as valid directives

The Language Reference table includes:
- `@break` — "Exit a @for or @while loop"
- `@continue` — "Skip to the next iteration"

The Rules & Gotchas section says:
> "@break / @continue are only valid inside @for and @while."

**Problem:** The parser rejects `@break` and `@continue` entirely (UITKX0305).
These were removed as part of the control block bodies rework. The table and
gotchas text are both stale.

### 9.3 Setup code before return() — not explained

The function-style example shows `var (count, setCount) = useState(0);` before
`return (...)`, but there's no explanation of what "setup code" is. Users don't
know that:
- Any C# code before `return` is setup code
- Setup code runs every render
- Local variables, method calls, conditional logic all allowed
- This also works inside control block bodies (e.g., `@if`)

### 9.4 Fragment `<>...</>` — not mentioned

Fragment syntax is entirely absent from the Language Reference. No mention of:
- `<>...</>` as shorthand for `V.Fragment`
- Multiple root elements via Fragment
- Fragment being an invisible wrapper (no DOM element)

### 9.5 Switch expressions not documented

The Language Reference only shows `@switch / @case / @default` (statement form).
C# switch expressions inside `@(...)` inline expressions are not mentioned:
```tsx
@(status switch {
    "ok" => <Label text="OK" />,
    _ => <Label text="Unknown" />
})
```

---

## SECTION 10 — Diagnostics Page — Specific Issues

### 10.1 Complete list of missing diagnostic codes

The Diagnostics page covers UITKX0001–0021 (SG) and UITKX0101–0111 (LSP)
and UITKX0300–0306 (parser). These are all **missing**:

| Code | Category | Description |
|------|----------|-------------|
| UITKX0022 | SG | Asset file not found |
| UITKX0023 | SG | Asset type mismatch |
| UITKX0024 | SG | Control block body missing return() |
| UITKX0112 | LSP | Unused variable in setup code |
| UITKX0120 | LSP | Asset path validation |
| UITKX0121 | LSP | Asset path validation |
| UITKX0200 | LSP | Attribute version mismatch |
| UITKX2100 | LSP | Non-PascalCase component name |
| UITKX2101 | LSP | Missing return in function-style component |
| UITKX2102 | LSP | Malformed return statement |
| UITKX2104 | LSP | Mixed directive/function-style syntax |
| UITKX2105 | LSP | Invalid function-style declaration |
| UITKX2106 | LSP | Missing parameter name |

### 10.2 UITKX0305 fix text lists @code as valid

Exact current text:
> "Valid directives: @if, @else, @for, @foreach, @while, @switch, @case,
> @default, @break, @continue, @code."

Must remove: `@break`, `@continue`, `@code`.

### 10.3 UITKX0306 fix text references @code blocks

Exact current text:
> "Inline expressions @(...) are only valid inside markup, not in @code blocks."

Must change to: "not in setup code."

---

## SECTION 11 — API Reference Page — Missing Items

### 11.1 V.Memo not listed (LOW PRIORITY)

`V.Memo(render, props, key, children)` — memoized component that skips
re-render when props haven't changed. Exists in V.cs, not on API page.

**NOTE:** The framework memos by design, so V.Memo is not user-facing.
Don't prioritize documenting this.

### 11.2 V.Host not listed — needs bootstrap guide (HIGH PRIORITY)

`V.Host(hostProps, key, children)` — creates a subtree mount point with its
own VisualElement root. Exists in V.cs, not on API page.

**ACTION:** Write a complete "Bootstrapping" guide covering:
- `V.Func(Component.Render)` — what it does, typed vs untyped overloads
- `V.Host(props, key, children)` — when and why to use it
- `RootRenderer.Initialize()` + `RootRenderer.Render()` flow
- Environment callback pattern (`env => ctx.Environment[...] = ...`)
- Companion .cs file relationship to generated .uitkx.g.cs
- EditorWindow vs MonoBehaviour bootstrap examples
- `EditorRootRendererUtility` for editor-only usage

### 11.3 Missing hooks from API hook list

The API page lists 18 hooks. Missing from the list:
- `UseLayoutEffect` — synchronous layout effect
- `UseCallback` — stable callback identity
- `UseRef` — mutable ref container
- `UseContext` — consume context value
- `ProvideContext` — provide context for subtree
- `UseDeferredValue` — deferred value
- `UseImperativeHandle` — imperative ref handle
- `UseStableFunc` — stable function reference
- `UseStableAction` — stable action reference
- `UseStableCallback` — stable parameterless callback
- `UseRouter` — router state access
- `UseRouteMatch` — route match object
- `UseNavigationBase` — navigation base path

### 11.4 V.Func not explained properly

The Getting Started page shows `V.Func(HelloWorld.Render)` as the mount pattern
but the API page doesn't explain `V.Func` — what it does, its typed vs untyped
overloads, or when to use it.

---

## SECTION 12 — Router Page — Gaps

### 12.1 Route pattern syntax incomplete

The Router page shows `:id` parameter and `*` wildcard but doesn't document:
- Optional parameters
- Regex constraints (if any)
- How parameters are validated/typed
- Nested route path resolution rules in detail

### 12.2 Missing hooks (partially)

The first pass said UseRouter, UseRouteMatch, UseNavigationBase were missing.
Second pass confirms the Router page **does document UseRouter()** but:
- `UseRouteMatch()` — NOT documented
- `UseNavigationBase()` — NOT documented

### 12.3 Link vs RouterNavLink confusion

V.cs has `V.Link(to, label, replace, style, key, state)` but the Router page
shows `<RouterNavLink>` in examples. The relationship between Link and
RouterNavLink is not explained. Are they the same? Different components?

---

## SECTION 13 — Signals Page — Gaps

### 13.1 Custom equality comparer not documented

`UseSignal<T, TSlice>(signal, selector, comparer)` accepts a custom
`IEqualityComparer<T>` but the docs don't mention this parameter or when
you'd use it.

### 13.2 API naming inconsistency in examples

The Signals page component example uses `SignalFactory.Get<int>(...)` while
the runtime example uses `Signals.Get<int>(...)`. If these are different APIs
they need explanation. If they're aliases, one should be canonical.

### 13.3 Thread safety not mentioned

`Signal<T>` has lock-based thread safety (visible in source). This is a
selling point worth documenting — signals can be safely read/written from
any thread.

---

## SECTION 14 — Samples With No Corresponding Docs

24 samples exist in `Samples/Components/`. Most demonstrate patterns that have
**no dedicated documentation page**:

| Sample | Feature | Docs Coverage |
|--------|---------|---------------|
| ContextDemoFunc | UseContext, ProvideContext | ❌ No page |
| ContextBailoutDemoFunc | Context performance patterns | ❌ No page |
| RefForwardingDemoFunc | UseRef, ref forwarding | ❌ No page |
| KeyedDiffLisDemoFunc | key prop in lists | ❌ No guide |
| EventBatchingDemoFunc | Event batching behavior | ❌ No page |
| ExceptionFlowDemoFunc | Error handling, ErrorBoundary patterns | ❌ Only component page exists |
| HookStateQueueDemoFunc | State update queueing/batching | ❌ No page |
| EffectCleanupOrderDemoFunc | Effect cleanup ordering | ❌ No page |
| DeferredEffectDemoFunc | UseDeferredValue or deferred effects | ❌ No page |
| FlushSyncDemoFunc | Synchronous render flush | ❌ No page |
| PropTypesDemoFunc | PropTypes validation | ❌ No page |
| RenderDepthGuardDemoFunc | Render depth limits | ❌ No page |
| SyntheticEventDemoFunc | Synthetic events | ❌ No page |
| PortalEventScopeDemoFunc | Portal + event scoping | ❌ No page |

These samples represent features that are implemented and testable but invisible
to users reading the docs.

---

## SECTION 15 — Getting Started Page — Issues

### 15.1 Mount pattern uses V.Func without explanation

Bootstrap example:
```csharp
rootRenderer.Render(V.Func(HelloWorld.Render));
```

`V.Func` is not explained anywhere on the Getting Started page or the API page.
A user seeing this for the first time wouldn't understand what `V.Func` does or
why it wraps the component's `Render` method.

### 15.2 No companion .cs file pattern shown

The Getting Started page shows the .uitkx file but the relationship between
a .uitkx file and its generated `.uitkx.g.cs` file isn't explained. Users
need to know:
- The SG generates `ComponentName.Render(IProps, IReadOnlyList<VirtualNode>)`
- This method is what you pass to `V.Func()`
- Companion .cs files can extend the generated partial class

### 15.3 RootRenderer creation pattern

The bootstrap creates `RootRenderer` via `AddComponent<RootRenderer>()` on a
new GameObject. But `RootRenderer.Instance` is also referenced in other docs.
The relationship between instance creation and the static `Instance` property
isn't clear.

---

## SECTION 16 — Styling Page — Gaps

### 16.1 className attribute not demonstrated

The Styling page shows inline `style={...}` and @uss directives but doesn't
have a concrete example of:
```tsx
<VisualElement className="my-card">
```
and how it interacts with USS selectors.

### 16.2 Style merging / composition not documented

How do you combine two Style objects? Can you spread styles? Is there a
`Style.Merge()` or similar? Users from CSS-in-JS backgrounds will ask this.

### 16.3 Conditional styling patterns

No example of conditional styles:
```tsx
style={new Style { BackgroundColor = isActive ? ColorGreen : ColorGray }}
```

---

## SECTION 17 — FAQ Issues

### 17.1 .NET version claim should be verified

FAQ states `.NET 8` is required for the LSP server. If the LSP targets a
different version, this should be updated.

---

## SECTION 18 — Component Pages — Systemic Issues

### 18.1 65 component pages exist but no overview

There are 65 individual component pages but no single page that:
- Lists ALL components by category (input, display, container, editor-only)
- Shows which are editor-only vs runtime
- Explains the V.ComponentName(props, key, children) pattern
- Explains the Props class inheritance (BaseProps → TypedProps)

### 18.2 Editor-only components not marked

`TwoPaneSplitView`, `PropertyField`, `InspectorElement`, `ObjectField`,
`ToolbarButton`, `ToolbarToggle`, `ToolbarMenu`, `ToolbarBreadcrumbs`,
`ToolbarPopupSearchField`, `ToolbarSearchField`, `ToolbarSpacer`, `Toolbar`,
`PropertyInspector` — all require `UNITY_EDITOR`. Not consistently marked as
editor-only in the docs.

### 18.3 Common BaseProps not shown per component

Each component page shows its specific props but doesn't reference the 16
common BaseProps (name, className, style, ref, visible, enabled, etc.). A user
on the Button page doesn't learn they can use `ref={...}` or `visible={false}`.

---

## Updated Priority Matrix

### P0 — Critical (v0.3.0 release blockers)

1. ~~Language Reference: update control flow examples to use `return ()`~~ ✅ DONE
2. ~~Language Reference: remove @break/@continue from directives table~~ ✅ DONE
3. ~~Diagnostics: add UITKX0024~~ ✅ DONE
4. ~~Diagnostics: remove @code, @break, @continue from UITKX0305 fix text~~ ✅ DONE
5. ~~Diagnostics: remove @code from UITKX0306 fix text~~ ✅ DONE
6. ~~Diagnostics: add UITKX2100–2106~~ ✅ DONE
7. ~~Diagnostics: add UITKX0022, 0023, 0112, 0120, 0121, 0200~~ ✅ DONE

### P1 — High (New pages / major gaps)

8. ~~Events & Input Handling guide~~ ✅ DONE
9. ~~Hooks deep-dive page~~ ✅ DONE
10. ~~Portal component page~~ ✅ DONE
11. ~~Suspense component page~~ ✅ DONE
12. ~~Fragment docs in Language Reference~~ ✅ DONE
13. ~~**Bootstrapping guide** (V.Func, V.Host, RootRenderer)~~ ✅ DONE
14. ~~Missing hooks in API Reference list~~ ✅ DONE
15. ~~Component lifecycle page~~ ✅ DONE (added to Concepts page)
16. ~~Context API guide~~ ✅ DONE

### P2 — Medium (Expand existing pages)

17. ~~Language Reference: document setup code pattern~~ ✅ DONE (added to Rules & Gotchas)
18. ~~Language Reference: document switch expressions~~ ✅ DONE
19. ~~Router: UseRouteMatch, UseNavigationBase, Link vs RouterNavLink~~ ✅ DONE
20. ~~Signals: custom comparer, thread safety, naming consistency~~ ✅ DONE
21. ~~Getting Started: explain V.Func, companion .cs, RootRenderer~~ ✅ DONE
22. ~~Styling: className, style merging, conditional patterns~~ ✅ DONE
23. ~~CssHelpers reference table~~ ✅ DONE
24. ~~AnimateTrack + Ease reference~~ ✅ DONE
25. ~~BaseProps common properties~~ ✅ DONE (added to Concepts page)
26. ~~Component overview page (categorized, editor-only markers)~~ ✅ DONE
27. ~~Ref usage tutorial~~ ✅ DONE
28. ~~Key usage guide~~ ✅ DONE

### P3 — Low (Nice to have)

29. ~~PropTypes documentation — §4.1~~ ✅ DONE (Advanced API Reference page)
30. ~~HostContext/Environment setup — §4.2~~ ✅ DONE (Advanced API Reference page)
31. ~~SnapshotAssert testing guide — §4.3~~ ✅ DONE (Advanced API Reference page)
32. ~~IScheduler priority docs — §4.4~~ ✅ DONE (Advanced API Reference page)
33. ~~FAQ: verify .NET version claim~~ ✅ DONE (confirmed net8.0 in .csproj)
34. ~~Roadmap page content~~ ✅ DONE
35. ~~Error handling patterns guide — §14 (ExceptionFlowDemoFunc)~~ ✅ DONE (Advanced API Reference page)
36. ~~State batching / FlushSync guide — §14 (FlushSyncDemoFunc)~~ ✅ DONE (Advanced API Reference page)
37. ~~Render depth guard docs — §14 (RenderDepthGuardDemoFunc)~~ ✅ DONE (Advanced API Reference + Known Issues)

---
---

# THIRD PASS — Additional Findings

**Date:** 2026-04-05
**Method:** Read Hooks.cs (full API), HMR page, Concepts page, ErrorBoundary,
Animate, Known Issues, StateSetterExtensions, EditorRootRendererUtility,
ElementRegistry, schema vs props cross-check, CHANGELOG cross-reference.

---

## SECTION 19 — Hooks.cs Complete API (Exact Signatures)

The first two passes listed hook names. This section provides the exact public
API for the hooks reference page.

### 19.1 State & Refs

```csharp
(T value, StateSetter<T> set) UseState<T>(T initial = default)
(TState state, Action<TAction> dispatch) UseReducer<TState, TAction>(
    Func<TState, TAction, TState> reducer, TState initialState)
Ref<T> UseRef<T>(T initial = default)
VisualElement UseRef()  // element ref overload
```

### 19.2 Effects & Lifecycle

```csharp
void UseEffect(Func<Action> effectFactory, params object[] dependencies)
void UseLayoutEffect(Func<Action> effectFactory, params object[] dependencies)
```

### 19.3 Memoization & Optimization

```csharp
T UseMemo<T>(Func<T> factory, params object[] dependencies)
Func<T> UseCallback<T>(Func<T> callback, params object[] dependencies)
    // NOTE: returns Func<T>, NOT Action — docs must clarify this
T UseDeferredValue<T>(T value, params object[] dependencies)
```

### 19.4 Advanced Patterns

```csharp
THandle UseImperativeHandle<THandle>(Func<THandle> factory,
    params object[] dependencies) where THandle : class
Func<T> UseStableFunc<T>(Func<T> function)
Action<T> UseStableAction<T>(Action<T> action)
Action UseStableCallback(Action callback)
```

### 19.5 Context

```csharp
T UseContext<T>(string key)
    // WARNING: stateless — does NOT consume hook slot
void ProvideContext<T>(string key, T value)
void ProvideContext(string key, object value)  // untyped overload
```

### 19.6 Signal Integration

```csharp
T UseSignal<T>(Signal<T> signal)
TSlice UseSignal<T, TSlice>(Signal<T> signal, Func<T, TSlice> selector,
    IEqualityComparer<TSlice> comparer = null)
T UseSignal<T>(string key, T initialValue = default)
TSlice UseSignal<T, TSlice>(string key, Func<T, TSlice> selector,
    IEqualityComparer<TSlice> comparer = null, T initialValue = default)
```

### 19.7 Animation

```csharp
void UseAnimate(IReadOnlyList<AnimateTrack> tracks,
    bool autoplay = true, params object[] dependencies)
void UseTweenFloat(float from, float to, float duration,
    Ease ease, float delay, Action<float> onUpdate,
    Action onComplete, params object[] dependencies)
```

### 19.8 Utilities

```csharp
SafeAreaInsets UseSafeArea(float tolerance = 0.5f)
void SuspendUntil(Task task)  // throws FiberSuspenseSuspendException
void FlushSync(Action action)
void FlushSync()
void BatchedUpdates(Action action)
```

### 19.9 Configuration Properties

```csharp
static bool EnableHookValidation { get; set; }
static bool EnableStrictDiagnostics { get; set; }
static bool EnableHookAutoRealign { get; set; }
```

**DOC GAP:** None of these configuration properties are documented. Users don't
know they can enable/disable hook validation, strict diagnostics, or auto-realign.

---

## SECTION 20 — HMR Page Gaps

### 20.1 Hook behavior in HMR not fully documented

The HMR page covers UseState, UseRef, UseEffect, UseMemo/UseCallback, UseContext
preservation. Missing:
- UseImperativeHandle behavior during HMR
- UseStableFunc/Action/Callback behavior during HMR
- UseTweenFloat/UseAnimate behavior during HMR
- UseDeferredValue behavior during HMR

### 20.2 UseCallback return type not mentioned

The HMR page implies UseCallback behaves like React's useCallback. In this
library, `UseCallback<T>` returns `Func<T>` not `Action`. This is a subtle but
important difference from React that should be called out.

### 20.3 UseContext is stateless — important HMR implication

`UseContext<T>` does NOT consume a hook slot. This means it can be called
conditionally without breaking hook order. The HMR page says "context values
preserved" but doesn't explain this unique characteristic.

---

## SECTION 21 — Concepts/Architecture Documentation Missing

### 21.1 No rendering pipeline explanation

The docs never explain the full pipeline:
1. User writes `.uitkx` markup
2. Source Generator compiles to C# (`Component.Render(IProps, children)`)
3. `V.Func(Component.Render)` wraps it as a VirtualNode
4. Reconciler diffs old vs new VirtualNode tree
5. Reconciler patches the real VisualElement tree

This is fundamental to understanding debug output, performance, and error
messages. Should be on the Concepts page.

### 21.2 No component lifecycle diagram

No page shows the lifecycle stages:
- Mount: construct VirtualNode → create VisualElement → run effects
- Update: re-render → diff → patch → run cleanup → run new effects
- Unmount: run all cleanups → remove VisualElement

---

## SECTION 22 — ErrorBoundary Gaps

### 22.1 OnError callback undocumented

`ErrorBoundaryProps.OnError` is `ErrorEventHandler` — the delegate signature
and when it fires are not shown on the ErrorBoundary component page.

### 22.2 ResetKey mechanism undocumented

`ErrorBoundaryProps.ResetKey` — presumably when this string changes, the error
state resets and children re-render. This recovery pattern is not documented.

---

## SECTION 23 — Animate/Animation Gaps

### 23.1 UseAnimate hook not documented separately

The Animate component page exists, but `UseAnimate` as a standalone hook
(usable without the `<Animate>` component) is not documented.

### 23.2 UseTweenFloat completely undocumented

Full signature available (§19.7) but zero docs anywhere.

### 23.3 Ease enum values not listed

17 easing functions exist (Linear, EaseInSine, EaseOutSine, EaseInOutSine,
etc.) but no page lists them.

### 23.4 AnimateTrack properties not documented

The AnimateTrack class has: Property, From, To, Duration, Delay, Ease, Repeat,
Loop, Yoyo, TimeScale, OnUpdate, OnComplete. No docs page lists these.

---

## SECTION 24 — EditorRootRendererUtility — Undocumented

### 24.1 No editor bootstrap guide

`EditorRootRendererUtility.Mount(host, root, env)` is the editor equivalent of
`RootRenderer.Initialize()` + `Render()`. Not documented anywhere.

Differs from runtime:
- Uses `EditorRenderScheduler.Instance`
- Sets `isEditor = true` in environment
- Auto-initializes SignalsRuntime and DiagnosticsConfig

### 24.2 Should be part of the Bootstrapping guide (§11.2)

The V.Func/V.Host bootstrap guide should have both runtime and editor sections.

---

## SECTION 25 — Known Issues Page — Stale/Incomplete

### 25.1 Only 2 issues listed

The Known Issues page has:
1. MultiColumnListView scroll jumping
2. Burst AOT Assembly Resolution

Both are marked "will be addressed" with no dates or status updates.

### 25.2 Missing known limitations

Should document:
- Any HMR edge cases or instability
- Maximum component tree depth
- Performance characteristics of large lists
- Thread safety constraints of hooks
- Editor vs runtime behavioral differences

---

## SECTION 26 — CHANGELOG Features Not in Docs

v0.3.0 changelog lists features with **zero documentation** on the docs site:

| Feature | Impact | Doc Action Needed |
|---------|--------|-------------------|
| Control block setup code | Users need to know | Add to Language Reference §9.3 |
| Switch fallthrough (`@case` stacking) | Users need to know | Add to Language Reference |
| CssHelpers renaming (`Row` → `FlexRow`) | **BREAKING** | Migration guide urgently needed |
| TextAutoSizeMode support | New capability | Add to relevant component pages |
| JustifySpaceEvenly | New enum | Add to CssHelpers reference |
| WhiteSpace.Pre/PreWrap | New enum | Add to CssHelpers reference |
| Compound struct factories (BgRepeat, etc.) | New API | Add to Styling page |
| Easing presets (24 values) | New API | Add to Animation guide |

### 26.1 CssHelpers new names must be in docs

No migration guide needed (no existing user base to migrate). The new names
(`FlexRow`, `FlexColumn`, etc.) simply become the library norm. However, the
CssHelpers reference table (§5.1) must list ALL current names correctly.

---

## SECTION 27 — Miscellaneous Findings

### 27.1 StateSetterExtensions — undocumented

Three extension methods exist:
```csharp
T Set<T>(this StateSetter<T>, StateUpdate<T>)
T Set<T>(this StateSetter<T>, Func<T, T>)
Action<T> ToValueAction<T>(this StateSetter<T>)
```
Not documented. `ToValueAction` is useful for binding state setters to onChange
events: `onChange={setName.ToValueAction()}`.

### 27.2 ElementRegistry — undocumented

`ElementRegistry` and `ElementRegistryProvider` manage element adapter
registration. Not user-facing for typical usage, but needed for custom
element adapters or filtered registries.

### 27.3 Hooks configuration properties — undocumented

`Hooks.EnableHookValidation`, `EnableStrictDiagnostics`, `EnableHookAutoRealign`
are all public static properties. Users don't know they exist or what they do.

### 27.4 BaseProps has all common properties including events

Verified: `BaseProps` contains 16 common properties + 19 event handlers
(pointer, wheel, drag, focus, keyboard, input, lifecycle). All element
props classes inherit these. The schema's `onClick`, `enabled`, etc. on
Button are inherited from BaseProps — no schema mismatch exists.

---

## Updated Priority Matrix (Pass 3 additions)

### P0 — Critical (add to existing P0 list)

38. ~~CssHelpers renaming migration guide~~ — NO MIGRATION NEEDED (no user base).
    Just ensure FlexRow, FlexColumn, etc. appear in CssHelpers reference — §5.1, §26

### P1 — High (add to existing P1 list)

39. ~~**Bootstrapping guide** should include EditorRootRendererUtility~~ ✅ DONE
40. ~~**Hooks API reference page** with all exact signatures~~ ✅ DONE
41. ~~**UseCallback returns Func<T> not Action**~~ ✅ DONE
42. ~~UseContext is stateless (no hook slot)~~ — internal implementation detail,
    does not affect users. No documentation needed.
43. ~~Switch fallthrough (`@case` stacking) docs~~ — normal switch behavior, no special docs needed

### P2 — Medium (add to existing P2 list)

44. ~~**ErrorBoundary: OnError + ResetKey**~~ ✅ DONE
45. ~~**UseAnimate standalone hook docs**~~ ✅ DONE
46. ~~**UseTweenFloat docs**~~ ✅ DONE
47. ~~**Ease enum values + AnimateTrack properties**~~ ✅ DONE
48. ~~**Rendering pipeline / component lifecycle explanation**~~ ✅ DONE
49. ~~**StateSetterExtensions docs**~~ ✅ DONE
50. ~~**Hooks configuration properties**~~ ✅ DONE
51. ~~**TextAutoSizeMode, JustifySpaceEvenly, WhiteSpace helpers**~~ ✅ DONE (included in CssHelpers reference)

### P3 — Low (add to existing P3 list)

52. ~~**Known Issues page update** — §25~~ ✅ DONE (expanded to 6 sections)
53. ~~**ElementRegistry docs** — §27.2~~ ✅ DONE (Advanced API Reference page)
54. ~~**HMR: document behavior of all hooks** — §20.1~~ ✅ DONE (expanded State Preservation table)

---
---

# FOURTH PASS (FINAL) — Verified Findings

**Date:** 2026-04-05
**Method:** Read every docs page TSX file, component example files, navigation
structure, Shared/Core/ types, config.json, SourceGenerator discovery, and
cross-verified all claims from passes 1–3.

---

## SECTION 28 — Component Examples Show C# API, Not .uitkx Markup (VERIFIED)

### 28.1 All 65 component pages show low-level C# API

Every component `*.example.ts` file (Button, Label, TextField, Toggle, ListView,
etc.) contains the **imperative C# factory API** (`V.Button()`, `Hooks.UseState()`,
etc.) — NOT `.uitkx` markup syntax.

Example from ButtonPage.example.ts:
```csharp
public static VirtualNode BasicButton(...)
{
    var (count, setCount) = Hooks.UseState(0);
    return V.Button(new ButtonProps { Text = $"Count: {count}" }, ...);
}
```

But users write:
```uitkx
component ButtonExample {
    var (count, setCount) = useState(0);
    return (
        <Button text={$"Count: {count}"} onClick={_ => setCount(count + 1)} />
    );
}
```

**Problem:** The Getting Started page, Companion Files page, and Language Reference
all teach `.uitkx` markup. Then the user clicks a component page and sees a
completely different syntax they've never been taught.

**Recommendation:** Add `.uitkx` markup examples alongside (or instead of) the
C# API examples on component pages. Or add a callout explaining the two syntaxes.

### 28.2 Pages that DO show .uitkx markup correctly

These pages already use correct `.uitkx` markup in their examples:
- Getting Started — `component HelloWorld { ... return (...) }`
- Companion Files — `component PlayerCard(PlayerInfo player) { ... }`
- Router — `component RouterDemo { ... }`
- HMR — various markup examples

The gap is specifically on the **65 individual component reference pages**.

---

## SECTION 29 — Source Generator File Discovery (VERIFIED)

### 29.1 Two discovery mechanisms, neither documented

The SG uses:

1. **Primary:** `AdditionalTexts` — `.uitkx` files injected into `.csproj` by
   `UitkxCsprojPostprocessor` as `<AdditionalFiles>`. Roslyn incremental cache-aware.

2. **Fallback:** Recursive disk scan of `Assets/` for `*.uitkx` when the
   postprocessor hasn't run yet (fresh install).

Neither mechanism is documented in the docs site. `config.json` has no file
discovery settings — it's hardcoded in the generator.

**Recommendation:** Add a brief section to Getting Started or Concepts:
> "The source generator automatically discovers all `.uitkx` files in your
> Assets/ directory. No registration is needed."

### 29.2 One component per file — implied but never stated

The Companion Files page implies it (file = component name) but never
explicitly says "each `.uitkx` file must contain exactly one component."

---

## SECTION 30 — Debugging Page — Verified Critical Gaps

### 30.1 Zero mention of breakpoints anywhere in docs

Checked ALL `.tsx` files in the docs source. The words "breakpoint", "debugger",
"stack trace" appear **zero times** across the entire documentation site.

The Debugging page covers:
- Generated `.g.cs` file locations
- `#line` directives (purpose, not how to use them for debugging)
- LSP server trace logging
- Formatter issues
- Bug reporting

**Missing entirely:**
- Can you set breakpoints in `.uitkx` files? (Answer: no — you debug in `.g.cs`)
- How to set breakpoints in generated code
- How `#line` directives map stack traces back to `.uitkx` source
- Using Unity's UI Toolkit Debugger with UITKX components
- Performance profiling approach

### 30.2 No performance profiling guidance

No page mentions:
- How to measure component render times
- How to identify slow reconciliation
- Using Unity Profiler markers (if any exist)
- Render count diagnostics

---

## SECTION 31 — Navigation Structure (VERIFIED — No Issues)

`docs.tsx` defines 15 sections. All links resolve to existing pages. No dead
links found. No orphan pages found.

The structure is well-organized. **No action needed.**

---

## SECTION 32 — "Different from React" Page (VERIFIED)

This page exists and is well-written. It covers:

1. **State setter model:** `StateSetter<T>` delegates vs React's setState methods.
   Documents `StateSetterExtensions` fluent helpers.

2. **Rendering model:** Synchronous per-Unity-frame reconciliation. No React 18
   concurrent rendering, no `startTransition`, no time-slicing.

**This page is good.** One refinement opportunity: mention `UseCallback` returns
`Func<T>` not `Action` (differs from React's `useCallback` which returns the
same type passed in).

---

## SECTION 33 — Companion Files Page (VERIFIED — Good)

This page exists and is comprehensive:
- Explains companion `.cs` files are optional
- Naming conventions: `*.styles.cs`, `*.types.cs`, `*.utils.cs`, `*.extra.cs`
- Same-directory requirement
- HMR support for companion file changes

**One note:** The page mentions "small helpers better in `@code` blocks" — this
reference to `@code` is stale. `@code` blocks were removed; setup code goes
before `return()` in the component body.

---

## SECTION 34 — VirtualNode Public API (VERIFIED)

`VirtualNode` is a `public sealed class` in `Shared/Core/VNode.cs` with many
public properties (NodeType, ElementTypeName, Properties, Children, etc.).

Users interact with it as the return type of render functions and in custom
render logic. However, for typical `.uitkx` usage, users never reference
`VirtualNode` directly — the SG handles it.

**Recommendation:** Low priority — only document if targeting advanced users
who work with the runtime C# API directly.

---

## SECTION 35 — USS Integration Gaps

### 35.1 className prop not demonstrated with USS

The Styling page shows `style={...}` inline and `@uss "path"` directive, but
no example shows:
```uitkx
@uss "./Card.uss"
component Card {
    return (
        <VisualElement className="card-container">
            <Label className="card-title" text="Hello" />
        </VisualElement>
    );
}
```

This is the most common USS usage pattern and it's not shown.

### 35.2 USS specificity with inline Style not explained

When both USS and inline `style={}` target the same property, which wins?
(In Unity UI Toolkit, inline style wins — matching CSS behavior.) This should
be stated.

### 35.3 Multiple className values

Can you do `className="card-container dark-theme"`? The Unity UI Toolkit
`AddToClassList` accepts individual class names. Is `className` prop
split on spaces? This behavior needs documentation.

---

## SECTION 36 — Companion Files Page — Stale @code Reference

### 36.1 Mentions @code blocks

The Companion Files page says helpers are "better in `@code` blocks" — but
`@code` blocks no longer exist. This text must be updated to say "in setup
code before return()."

---

## Updated Priority Matrix (Pass 4 additions)

### P0 — Critical

55. ~~**Companion Files page: remove @code reference**~~ ✅ DONE

### P1 — High

56. ~~**Component pages: add .uitkx markup examples**~~ ✅ DONE (UitkxComponentReferencePage already renders .uitkx examples via getExample())
57. ~~**Document SG file discovery** in Getting Started~~ ✅ DONE
58. ~~**State "one component per file" rule explicitly**~~ ✅ DONE

### P2 — Medium

59. ~~**Debugging: breakpoint workflow**~~ ✅ DONE
60. ~~**Debugging: stack trace mapping from #line**~~ ✅ DONE
61. ~~**Debugging: performance profiling approach**~~ ✅ DONE (UI Toolkit Debugger mentioned)
62. ~~**USS className prop example**~~ ✅ DONE (already in USS examples)
63. ~~**USS specificity with inline Style**~~ ✅ DONE
64. ~~**Multiple className values behavior**~~ ✅ DONE
65. ~~**Differences from React: UseCallback return type**~~ ✅ DONE

### P3 — Low

66. ~~**VirtualNode API for advanced users** — §34~~ ✅ DONE (Advanced API Reference page)
67. ~~**Unity UI Toolkit Debugger integration**~~ ✅ DONE
