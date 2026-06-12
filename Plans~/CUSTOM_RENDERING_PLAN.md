# Custom Rendering Support — `OnGenerateVisualContent` + `RedrawKey`

> **Goal:** Add first-class, declarative support for Unity's custom-rendering
> API on **every** built-in element:
>
> ```jsx
> <VisualElement
>     style={Canvas}
>     OnGenerateVisualContent={ctx => {
>         var p = ctx.painter2D;          // Painter2D vector API
>         p.BeginPath(); p.MoveTo(a); p.LineTo(b); p.Stroke();
>         // or low-level: var mwd = ctx.Allocate(vCount, iCount, tex); mwd.SetAllVertices(...);
>     }}
>     RedrawKey={frame}              // optional: force a repaint without a callback change
> />
> ```
>
> **Status:** Plan only — nothing implemented. Every file path / line anchor
> below was verified against the live code on 2026-06-12 (library 0.6.2).
>
> **Scope decision (locked):** typed `BaseProps` prop, NOT a hook, NOT a new
> element. Reuses the typed props pipeline (no boxing, no reflection, no
> dictionary lookup at paint time). `Painter2D` AND `MeshGenerationContext.Allocate`
> are both covered because the callback receives the full `MeshGenerationContext`.

---

## 0. Unity API ground truth (verified, 6000.3 docs)

- `public Action<MeshGenerationContext> VisualElement.generateVisualContent;`
  — a **multicast delegate field** (`+=` / `-=`), **NOT** an `EventBase`/`RegisterCallback` event.
- Re-fires **only** when `VisualElement.MarkDirtyRepaint()` is called (or on first paint / size change).
- Inside the callback the element must be treated **read-only** (mutating it can cause missed updates).
- `MeshGenerationContext` exposes `.painter2D` (Painter2D), `.Allocate(vtx, idx, texture?)` → `MeshWriteData`,
  `.DrawText`, `.DrawVectorImage`, `.visualElement`. All in `UnityEngine.UIElements` (UnityEngine.UIElementsModule) = **runtime, player-safe**.
- => **No `#if UNITY_EDITOR` gating** for this feature.

### Why this is NOT the existing event path
Existing event props (`OnClick` etc.) are typed as **Reactive wrapper** delegates —
`public delegate void PointerEventHandler(ReactivePointerEvent e)`
([Shared/Core/ReactiveTypes.cs](../Shared/Core/ReactiveTypes.cs#L427)) — and are applied via
`PropsApplier.RegisterEvent<T>` where `T : EventBase<T>` ([Shared/Props/PropsApplier.cs](../Shared/Props/PropsApplier.cs#L2089)).
`generateVisualContent` is neither a Reactive wrapper nor an `EventBase`, so it
**cannot and must not** go through `RegisterEvent`/`ApplyEvent`. We assign it
directly. This is *simpler and faster* than the event path: no `EventCallback<T>`
allocation, no `EventHandlerTargets` dictionary, no `InvokeHandler` type-switch.

---

## 1. The two design decisions (locked)

### 1a. Delegate type = raw `System.Action<MeshGenerationContext>`
Matches Unity's field exactly, so the user's `ctx` is the real `MeshGenerationContext`
(needed for `ctx.painter2D` / `ctx.Allocate`). Consistent with `VideoProps` using
raw `Action` for `OnPrepared`/`OnEnded` ([Shared/Props/Typed/VideoProps.cs](../Shared/Props/Typed/VideoProps.cs#L66)).
No new Reactive wrapper type. (Optional nicety: a named alias
`public delegate void VisualContentHandler(MeshGenerationContext mgc);` in ReactiveTypes.cs
for prettier hover — **deferred**, adds a delegate-conversion wrinkle for little gain.)

### 1b. Repaint trigger = reference-change OR `RedrawKey` change
`generateVisualContent` only re-fires on `MarkDirtyRepaint()`. The apply path calls
`element.MarkDirtyRepaint()` when **either**:
- the draw delegate reference differs from the previous render, **or**
- `RedrawKey` (typed `int`, default `0`) differs from the previous render.

Behaviour matrix:
| draw callback | RedrawKey | repaints when |
|---|---|---|
| fresh closure each render (default) | unset (0) | every render of the owner |
| `useStableCallback(draw, deps)` | unset (0) | only when `deps` change |
| `useStableCallback(draw, [])` | bump it | only when `RedrawKey` changes |

`RedrawKey` is `int` (NOT `object`) to avoid boxing on every render. `useStableCallback`
already ships (0.5.21) and is the efficiency lever — no new memoization API needed.

**Honest limitation (document, don't hide):** `RedrawKey` is a prop, so changing it
requires the component to re-render. It does NOT enable per-frame repaint *without*
re-rendering. For an external 60fps tween/ticker, the supported path is the `ref`
escape hatch + imperative `MarkDirtyRepaint`, or pairing with the existing
`AnimationTicker` ([Shared/Core/Animation/AnimationTicker.cs](../Shared/Core/Animation/AnimationTicker.cs)).

---

## 2. Files-to-touch summary (verified)

| Layer | File | Change | Auto? |
|---|---|---|---|
| Runtime props | [Shared/Props/Typed/BaseProps.cs](../Shared/Props/Typed/BaseProps.cs) | 2 fields + 2 props + reset + ShallowEquals | — |
| Runtime metadata | [Shared/Core/NodeMetadata.cs](../Shared/Core/NodeMetadata.cs) | 2 fields (trampoline + latest) | — |
| Runtime apply | [Shared/Props/PropsApplier.cs](../Shared/Props/PropsApplier.cs) | `ApplyGvc` + `RemoveGvc` helpers; route key in `ApplySingle`/`RemoveProp` BEFORE the `on*` trap | — |
| Runtime typed apply | [Shared/Props/TypedPropsApplier.cs](../Shared/Props/TypedPropsApplier.cs) | `ApplyFull` add `ApplyGvcIfSet`; `ApplyDiff` add `DiffGvc` (inside `_hasEvents` block) | — |
| IDE schema | [ide-extensions~/grammar/uitkx-schema.json](../ide-extensions~/grammar/uitkx-schema.json) | 2 entries in `intrinsicElementAttributes` | — |
| IDE VDG | [ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs) | 1 entry in `s_eventCallbackParamTypes` | — |
| Source generator | — | **nothing** | ✅ reflection + PascalCase |
| HMR emitters | — | **nothing** | ✅ reflection + ToPascal |
| Per-element props (ButtonProps, …) | — | **nothing** | ✅ inherit from BaseProps |
| Completion / Hover / DiagnosticsAnalyzer / grammar tmLanguage | — | **nothing** | ✅ schema-driven |

---

## 3. Runtime implementation (the real work)

### 3.1 `BaseProps` — [Shared/Props/Typed/BaseProps.cs](../Shared/Props/Typed/BaseProps.cs)

**(a) Backing fields** — add beside the lifecycle backing fields (after `_onDetachFromPanel`, ~L120):
```csharp
private Action<UnityEngine.UIElements.MeshGenerationContext> _onGenerateVisualContent;
// RedrawKey is a plain value; default 0 means "unset / never forces a repaint".
```
Add to the existing identity/value region (near `TabIndex`, ~L44):
```csharp
public int RedrawKey { get; set; }
```
> `using System;` is already present (L1). `MeshGenerationContext` is fully-qualified
> to avoid adding a `using UnityEngine.UIElements;` ambiguity (the file already has it
> via L4 `using UnityEngine.UIElements;` — confirm and use short name `MeshGenerationContext` if so).

**(b) Public property** — beside the event props, sets `_hasEvents = true` so it rides the
existing `_hasEvents` fast-path gate (the draw delegate is conceptually an event):
```csharp
public Action<UnityEngine.UIElements.MeshGenerationContext> OnGenerateVisualContent
{
    get => _onGenerateVisualContent;
    set
    {
        _onGenerateVisualContent = value;
        if (value != null)
            _hasEvents = true;
    }
}
```

**(c) `__ResetBase()`** ([L757](../Shared/Props/Typed/BaseProps.cs#L757)) — **CRITICAL** pool-safety. Add before `ExtraProps = null;` (L821):
```csharp
_onGenerateVisualContent = null;
RedrawKey = 0;
```
> Missing this = a recycled pooled props instance carries a stale draw delegate or
> stale key → ghost drawing on an unrelated element. This is the #1 pooling trap.

**(d) `ShallowEquals(BaseProps other)`** ([L577](../Shared/Props/Typed/BaseProps.cs#L577)) — host-bailout equality.
- Add `RedrawKey` compare in the **value** section (it's not an event), near the `LanguageDirection` check (~L621):
  ```csharp
  if (RedrawKey != other.RedrawKey)
      return false;
  ```
- Add the draw-delegate compare **inside** the `if (_hasEvents || other._hasEvents)` block
  (with the other lifecycle events, ~L723):
  ```csharp
  if (OnGenerateVisualContent != other.OnGenerateVisualContent)
      return false;
  ```
> Why both: `ShallowEquals` drives bailout — if it wrongly returns `true` when only the
> draw delegate or key changed, the element would skip the update and never repaint.

**(e) `ToDictionary()` — DO NOT add `OnGenerateVisualContent`.** See risk R1 (§6). The
untyped dictionary path would route an `on*`/`On*` delegate into `ApplyEvent` and silently
no-op. `RedrawKey` may be added to `ToDictionary` safely (it's an int, handled by `ApplySingle`'s
generic setter) but is only needed if the untyped/Host path must support forced redraw — defer
unless required.

**Per-element props (ButtonProps, BoxProps, …): NO CHANGES.** Verified
([ButtonProps.cs](../Shared/Props/Typed/ButtonProps.cs)): each overrides `__ResetFields`
(own fields only), `ShallowEquals` (calls `base.ShallowEquals` first), `ToDictionary`
(calls `base`), `__ReturnToPool`. The pool's `Rent()` calls `__ResetBase()` **and**
`__ResetFields()` ([L836-L844](../Shared/Props/Typed/BaseProps.cs#L836)). So a `BaseProps`-level
field + base reset + base equality propagates to **all** ~50 subclasses automatically.

### 3.2 `NodeMetadata` — [Shared/Core/NodeMetadata.cs](../Shared/Core/NodeMetadata.cs#L9)

Add two dedicated fields (do **not** reuse `EventHandlers`/`EventHandlerTargets` — their
removal logic expects `EventCallback<T>`):
```csharp
// Custom visual content (generateVisualContent). Trampoline is registered once
// on the element; LatestGvc holds the current user delegate, swapped each render.
public Action<UnityEngine.UIElements.MeshGenerationContext> GvcTrampoline;
public Action<UnityEngine.UIElements.MeshGenerationContext> LatestGvc;
```
> `NodeMetadata` is `internal sealed`; created lazily on first ref/event/gvc apply via
> `element.userData`. It survives re-renders and the panel-rebuild `RetargetContainer`
> (element identity preserved); GC'd with the element on unmount. (`using System;` already present, L3.)

### 3.3 `PropsApplier` — [Shared/Props/PropsApplier.cs](../Shared/Props/PropsApplier.cs)

**(a) The stable-trampoline apply helper** (new, model on `RegisterEvent` ~L2089 and
VideoElementAdapter's `frameReady += / -=` ~L302):
```csharp
internal const string GvcKey = "generateVisualContent";

private static void ApplyGvc(
    VisualElement element,
    Action<MeshGenerationContext> next,
    Action<MeshGenerationContext> prev,
    int prevKey,
    int nextKey)
{
    var meta = element.userData as Core.NodeMetadata;
    if (meta == null) { meta = new Core.NodeMetadata(); element.userData = meta; }

    // Register the trampoline exactly once; it always reads the latest delegate.
    if (meta.GvcTrampoline == null)
    {
        meta.GvcTrampoline = mgc =>
        {
            var d = meta.LatestGvc;
            if (d == null) return;
            try { d(mgc); }
            catch (Exception ex) { UnityEngine.Debug.LogError(ex); } // one bad draw must not kill the panel
        };
        element.generateVisualContent += meta.GvcTrampoline;
    }

    meta.LatestGvc = next;

    // Repaint when the delegate identity changed OR RedrawKey changed.
    if (!ReferenceEquals(prev, next) || prevKey != nextKey)
        element.MarkDirtyRepaint();
}

private static void RemoveGvc(VisualElement element)
{
    var meta = element.userData as Core.NodeMetadata;
    if (meta == null) return;
    meta.LatestGvc = null;
    if (meta.GvcTrampoline != null)
    {
        element.generateVisualContent -= meta.GvcTrampoline;
        meta.GvcTrampoline = null;
    }
    element.MarkDirtyRepaint(); // clear the old drawing
}
```
> Trampoline pattern mirrors the event system's "register once, swap latest" indirection
> ([RegisterEvent](../Shared/Props/PropsApplier.cs#L2096) + [InvokeEvent](../Shared/Props/PropsApplier.cs#L2616)).
> `try/catch` mirrors [AnimationTicker.Pump](../Shared/Core/Animation/AnimationTicker.cs#L56).

**(b) Untyped-path guard (DEFENSIVE, optional).** The typed path (§3.4) calls `ApplyGvc`
**directly** and never goes through `ApplySingle`'s string dispatch, so this guard is NOT
required for normal usage. It only matters if a user manually puts a draw delegate in
`ExtraProps`. The `on*` trap is at
`if (propertyName.StartsWith("on") && propertyValue is Delegate d)`
(verified [L1581](../Shared/Props/PropsApplier.cs#L1581), the LAST branch of `ApplySingle`,
signature `ApplySingle(element, oldValue, propertyName, propertyValue)` [L1351](../Shared/Props/PropsApplier.cs#L1351)).
Note `"generateVisualContent"` does **not** start with `"on"`, so it would fall through
harmlessly; but the markup name `"onGenerateVisualContent"` **does**. If we add the guard,
put it **above** the trap and match both spellings:
```csharp
if ((propertyName == GvcKey || propertyName == "onGenerateVisualContent")
    && propertyValue is Action<MeshGenerationContext> gvc)
{
    ApplyGvc(element, gvc, oldValue as Action<MeshGenerationContext>, 0, 0);
    return;
}
```
And in `RemoveProp` (signature `RemoveProp(element, propertyName, oldValue)` [L1588](../Shared/Props/PropsApplier.cs#L1588))
an early `if (propertyName == GvcKey || propertyName == "onGenerateVisualContent") { RemoveGvc(element); return; }`.
> Recommended but low-priority: the primary protection is simply keeping the prop out of
> `ToDictionary` (§3.1e) so the untyped path never sees it from typed props.

### 3.4 `TypedPropsApplier` — [Shared/Props/TypedPropsApplier.cs](../Shared/Props/TypedPropsApplier.cs)

This is the hot path for built-in elements (adapters call `ApplyTypedFull`/`ApplyTypedDiff`).

**(a) `ApplyFull`** ([L24](../Shared/Props/TypedPropsApplier.cs#L24)) — after the lifecycle `ApplyEventIfSet` calls (~L135), before `ExtraProps`:
```csharp
// Custom visual content (registers trampoline; repaints once on initial set)
if (props.OnGenerateVisualContent != null)
    PropsApplier.ApplyGvcFull(element, props.OnGenerateVisualContent); // thin wrapper: ApplyGvc(el, cb, null, 0, props.RedrawKey)
```
> Add a tiny `internal static ApplyGvcFull(element, cb)` wrapper in PropsApplier, OR call
> `ApplyGvc` directly with `prev=null, prevKey=0`. Initial mount always repaints (correct).
> NOTE: if `OnGenerateVisualContent == null` but `RedrawKey != 0` on first mount, no draw
> exists yet — nothing to repaint; safe to skip.

**(b) `ApplyDiff`** ([L156](../Shared/Props/TypedPropsApplier.cs#L156)) — inside the
`if (prev._hasEvents || next._hasEvents)` block ([L201](../Shared/Props/TypedPropsApplier.cs#L201)),
beside the lifecycle `DiffEvent`s (~L320). Because `RedrawKey` participates, handle it together:
```csharp
DiffGvc(element, prev.OnGenerateVisualContent, next.OnGenerateVisualContent, prev.RedrawKey, next.RedrawKey);
```
New private helper (model on [DiffEvent](../Shared/Props/TypedPropsApplier.cs#L1197)):
```csharp
private static void DiffGvc(
    VisualElement element,
    Action<MeshGenerationContext> prev,
    Action<MeshGenerationContext> next,
    int prevKey, int nextKey)
{
    if (next != null)
        PropsApplier.ApplyGvc(element, next, prev, prevKey, nextKey); // make ApplyGvc internal
    else if (prev != null)
        PropsApplier.RemoveGvc(element);
    // (next==null && prev==null) => nothing
}
```
> **GOTCHA (R2):** `RedrawKey` is gated behind `_hasEvents`. If a user sets `RedrawKey`
> but never sets `OnGenerateVisualContent`, `_hasEvents` stays false and `DiffGvc` is skipped.
> That's correct — with no draw callback there is nothing to repaint. But if a user sets the
> draw callback once via a stable ref (so `_hasEvents=true` on both prev/next) and then only
> bumps `RedrawKey`, both sides have `_hasEvents=true` (the setter is monotonic and the value
> is carried), so the block runs and `DiffGvc` sees the key change. ✅ Verified safe because
> `OnGenerateVisualContent` setter set `_hasEvents=true` and pooled reset clears it only on
> a fresh rent. Confirm with a test (T7).

**Add `using UnityEngine.UIElements;`** is already present in both files (BaseProps L4, TypedPropsApplier L4, PropsApplier uses it). Confirm before editing.

---

## 4. IDE implementation (2 edits, both in `ide-extensions~`)

### 4.1 Schema — [ide-extensions~/grammar/uitkx-schema.json](../ide-extensions~/grammar/uitkx-schema.json)
There is exactly **one** schema file with `intrinsicElementAttributes` ([L16](../ide-extensions~/grammar/uitkx-schema.json#L16));
it is embedded as a resource into the LSP server (consumed by VS Code, VS2022, Rider alike).
Add two entries to that array (alongside `onDetachFromPanel`, ~L196):
```json
{
  "name": "onGenerateVisualContent",
  "type": "Action<MeshGenerationContext>",
  "description": "Custom rendering callback. Receives a MeshGenerationContext; use ctx.painter2D for vector drawing or ctx.Allocate for raw meshes. Re-runs on MarkDirtyRepaint (driven by callback identity change or RedrawKey)."
},
{
  "name": "redrawKey",
  "type": "int",
  "description": "Bump to force a repaint of OnGenerateVisualContent without changing the callback (use with a stable callback)."
}
```
> This is what makes the attribute valid on **all** elements (kills the false **UITKX0109**
> in the LSP analyzer, which builds `knownAttributes` from the schema's
> `intrinsicElementAttributes`), and feeds autocomplete + hover automatically
> (CompletionHandler / HoverHandler read the schema). Casing is normalized
> case-insensitively, so `OnGenerateVisualContent` and `onGenerateVisualContent` both resolve.
> **SG build-time UITKX0109 is already satisfied by reflection** over `BaseProps` — no SG edit.

### 4.2 Virtual document generator — [VirtualDocumentGenerator.cs](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs#L1718)
Add one entry to `s_eventCallbackParamTypes` (the map that types attribute-lambda params; [L1718](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs#L1718)):
```csharp
["onGenerateVisualContent"] = "global::UnityEngine.UIElements.MeshGenerationContext",
```
> This makes `ctx` in `OnGenerateVisualContent={ctx => ...}` type as `MeshGenerationContext`
> so `ctx.painter2D` / `ctx.Allocate` autocomplete and type-check. The map key is the
> **camelCase** sanitised attribute name. `MeshGenerationContext` resolves because any project
> using `<VisualElement>` references `UnityEngine.UIElementsModule`; the emit also wraps with
> `#pragma warning disable CS0246` defensively. `redrawKey` needs **no** VDG entry (it's a
> value attribute, not a lambda).

**No grammar (`uitkx.tmLanguage.json`) change** — attribute coloring is generic.
**No CompletionHandler / HoverHandler / DiagnosticsAnalyzer change** — all schema-driven.

---

## 5. Source generator + HMR (verified: ZERO changes)

- **SG attribute→prop**: pure PascalCase via `ToPropName` ([CSharpEmitter.cs L3424](../SourceGenerator~/Emitter/CSharpEmitter.cs#L3424)).
  `onGenerateVisualContent` → `OnGenerateVisualContent`, `redrawKey` → `RedrawKey`.
- **SG UITKX0109**: dynamic reflection over the props type's inheritance chain
  ([PropsResolver.CollectPropertyNames L414](../SourceGenerator~/Emitter/PropsResolver.cs#L414) walks into `BaseProps`).
  New `BaseProps` members are auto-known. Only `key`/`ref` are special.
- **SG lambda values**: multi-line `ctx => {...}` handled by `SpliceExpressionMarkup`
  ([L2767](../SourceGenerator~/Emitter/CSharpEmitter.cs#L2767)) with correct `#line` mapping
  ([L2847](../SourceGenerator~/Emitter/CSharpEmitter.cs#L2847)); JSX-in-lambda splicing won't interfere.
- **HMR**: same `ToPascal` path ([HmrCSharpEmitter L2674](../Editor/HMR/HmrCSharpEmitter.cs#L2674)),
  reflection-based tag discovery ([L33](../Editor/HMR/HmrCSharpEmitter.cs#L33)). Draw-body edits
  hot-reload: recompile → `RefreshRuntime.PerformRefresh()` re-renders → new closure flows as a
  prop → `DiffGvc` re-applies (delegate identity changes because the method's DeclaringType moves
  to the new HMR DLL → `ReferenceEquals(prev,next)` is false → repaint fires). HMR infra is
  `#if UNITY_EDITOR`; the prop itself is unconditional → player-safe.

---

## 6. Risks & gotchas (each with mitigation)

- **R1 — untyped `on*` delegate trap (LOW / defensive).** `ApplySingle` routes any
  `propertyName.StartsWith("on") && value is Delegate` to `ApplyEvent`
  ([L1581](../Shared/Props/PropsApplier.cs#L1581)), which only knows `EventBase` events →
  a draw delegate there **silently no-ops**. **This is NOT hit by normal usage:** the typed
  path calls `ApplyGvc` directly (no string dispatch), and we keep the prop out of
  `ToDictionary` (§3.1e), so the untyped path never receives it. It is reachable ONLY if a
  user hand-adds `onGenerateVisualContent` to `ExtraProps`. **Mitigation:** the optional
  defensive guard in §3.3b (matching both `"generateVisualContent"` and
  `"onGenerateVisualContent"`), placed above the trap.
- **R2 — `_hasEvents` gating of `RedrawKey` (MED).** `DiffGvc` lives inside the `_hasEvents`
  block. Safe because setting `OnGenerateVisualContent` sets `_hasEvents=true`; a stable-callback
  user keeps `_hasEvents=true` across renders so key-only changes still diff. Cover with test T7.
- **R3 — pool stale state (HIGH).** Must null `_onGenerateVisualContent` and zero `RedrawKey`
  in `__ResetBase()` (§3.1c). Verified `Rent()` calls `__ResetBase()` only on reuse
  ([L836](../Shared/Props/Typed/BaseProps.cs#L836)). Cover with test T5.
- **R4 — trampoline leak on element reuse (LOW).** The trampoline is stored in `NodeMetadata`
  and registered once; on the element's panel rebuild (`RetargetContainer`) the element identity
  + `userData` survive, so we do NOT re-register (guarded by `GvcTrampoline == null`). On unmount
  the element is dropped and GC'd with its metadata; no manual `-=` needed, but `RemoveGvc` handles
  the explicit null-transition. Cover with test T6.
- **R5 — drawing while detached.** `MarkDirtyRepaint` on a detached element is a Unity no-op;
  harmless. The callback only fires when Unity paints an attached panel.
- **R6 — `MeshGenerationContext` resolvable in IDE.** Requires the project to reference
  `UnityEngine.UIElementsModule` (true for any UI Toolkit project). VDG `#pragma` softens failure.
- **R7 — read-only-element rule.** Document that mutating the element inside the callback is
  unsupported (Unity contract). Not enforced in code (can't cheaply), but called out in docs + the
  schema description.
- **R8 — multicast clobber.** Use `+=`/`-=` with the stored trampoline (never `=`), so we never
  clobber a foreign subscriber. Framework elements have no other subscriber, but `+=` is the
  correct Unity idiom regardless.

---

## 7. Test plan (SourceGenerator~/Tests unless noted)

- **T1 (SG emit):** `<VisualElement OnGenerateVisualContent={ctx => {...}} RedrawKey={n}/>` emits
  `__p.OnGenerateVisualContent = ctx => {...}` and `__p.RedrawKey = n` — no UITKX0109.
- **T2 (SG multi-line lambda):** multi-statement `ctx => { ... }` compiles + `#line` maps to `.uitkx`.
- **T3 (SG on user component):** `OnGenerateVisualContent` on a *user component* without that
  parameter still raises UITKX0109 (unchanged behaviour — it's only intrinsic on built-ins).
- **T4 (runtime apply):** initial mount registers exactly one `generateVisualContent` subscriber +
  one `MarkDirtyRepaint`. (Editor playmode test, or a thin unit around `ApplyGvc`.)
- **T5 (pool reset):** rent → set draw + RedrawKey → return → re-rent → both are default
  (`__ResetBase` cleared them).
- **T6 (diff/remove):** changing the delegate ref repaints; setting it null `-=` the trampoline +
  repaints; stable ref + unchanged key does NOT repaint; stable ref + changed key DOES repaint.
- **T7 (RedrawKey + _hasEvents):** stable callback (so `_hasEvents` true), bump only `RedrawKey`
  → `DiffGvc` runs and repaints.
- **T8 (HMR parity):** existing `HmrEmitterParityContractTests` / `HmrBuiltinTagDiscoveryContractTests`
  stay green (no new attribute allowlist). Add a marker test that HMR emits the prop assignment
  identically to SG.
- **T9 (IDE VDG):** virtual doc for `OnGenerateVisualContent={ctx => ...}` declares
  `MeshGenerationContext ctx` → add an `RoslynCompletionTests`/VDG test asserting `ctx.painter2D`
  resolves and no false diagnostic on the attribute.
- **T10 (player build):** a build-shaped compile (no `UNITY_EDITOR`) of a component using the prop
  compiles and the prop applies (no editor-only dependency).

---

## 8. Phased checklist (suggested order)

1. **Runtime core** — BaseProps (fields/prop/reset/equals) + NodeMetadata + PropsApplier
   (`ApplyGvc`/`RemoveGvc` + untyped guard) + TypedPropsApplier (`ApplyGvcIfSet`/`DiffGvc`).
   `get_errors` clean. This alone makes `Initialize`-mounted trees draw.
2. **Runtime tests** T4–T7, T10. Verify in a scratch `.uitkx` in editor playmode (Painter2D line + an `Allocate` quad).
3. **IDE** — schema 2 entries + VDG 1 entry. Rebuild LSP; confirm autocomplete on the attribute and on `ctx.painter2D`, no UITKX0109. (Uses the rebuild-ide-extensions skill.)
4. **SG/HMR tests** T1–T3, T8 (should pass with no SG/HMR code change; if any fail, that's a real surprise to investigate).
5. **Sample + docs** — a `CustomDrawDemo.uitkx` (Painter2D chart + a `useStableCallback` + `RedrawKey` animation example); docs page mirroring the Video page. Read the changelog instruction files before any version/changelog edits (user owns versioning).
6. **V1 plan** — add a one-line entry under a new "Custom rendering" feature in
   [V1_RELEASE_PLAN.md](V1_RELEASE_PLAN.md) once shipped (do not bump versions here).

---

## 9. Explicitly out of scope (for this change)
- `useVisualContent` hook (the typed prop supersedes it; could ship later as sugar over the same field).
- A `<Canvas>` element (a Video-style adapter) — optional ergonomic wrapper, post-v1.
- Detached/offscreen `Painter2D` + `SaveToVectorImage` workflows.
- Per-frame repaint without re-render (use ref + AnimationTicker; documented in §1b).
