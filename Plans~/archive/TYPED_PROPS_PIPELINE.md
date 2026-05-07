# Typed Props Pipeline — Implementation Plan

> **Date:** April 24, 2026
> **Objective:** Eliminate `Dictionary<string, object>` from the hot path for ALL built-in host elements.
> Dict path retained ONLY for `V.Host()` and `VisualElementSafe` (inherently dict-based APIs).
> **Estimated gain:** +5-10 FPS in stress test (3000 boxes), eliminates ~6000 dict allocs/frame.
>
> **Key decisions (confirmed):**
> - `ShallowEquals` is **required** on every `BaseProps` subclass — no dict-fallback safety net.
> - `ITypedElementAdapter` is **required** on every adapter — no opt-in, full conversion.
> - Correctness is guarded by **tests**, not runtime fallback.
> - **Style typing** (eliminating boxing of ~80 style properties) is a **follow-up step** after this pipeline works.
>   Style remains `Dictionary<string, object>` for now; typed Style would eliminate an additional ~15,000 boxing allocs/frame.

---

## Table of Contents

1. [Current Flow (dict-based)](#current-flow)
2. [Proposed Flow (typed)](#proposed-flow)
3. [Implementation Steps](#implementation-steps)
4. [System-by-System Impact](#system-impact)
5. [What Breaks](#what-breaks)
6. [What Does NOT Break](#what-does-not-break)
7. [Risk Mitigations](#risk-mitigations)
8. [Testing Strategy](#testing-strategy)
9. [File-by-File Change List](#file-changes)
10. [Follow-up: Typed Style](#typed-style)

---

## <a name="current-flow"></a>1. Current Flow (dict-based)

Every host element follows this pipeline on every render:

```
Source gen emits:     V.Label(new LabelProps { Text = "hi", Style = new Style { Left = 5f } }, key: "k")
                                                    ↓
V.Label():           props.ToDictionary()           → new Dict { "text": "hi", "style": { "left": 5f } }
                                                    ↓
new VirtualNode(..., properties: dict)              VNode.Properties = dict
                                                    ↓
FiberFactory.CreateNew / CloneForReuse:             fiber.PendingProps = ExtractProps(vnode) → vnode.Properties
                                                    ↓
CompleteWork:         AreHostPropsEqual(             fiber.PendingProps (dict), fiber.Props (dict))
                     → iterates every key+value     ↓ if not equal
                     fiber.EffectTag |= Update
                                                    ↓
CommitUpdate:        _hostConfig.ApplyProperties(   element, type, fiber.Props, fiber.PendingProps)
                     → adapter.ApplyPropertiesDiff( element, prev dict, next dict)
                     → PropsApplier.ApplyDiff(      element, prev dict, next dict)
                     → iterates both dicts + both style sub-dicts
                                                    ↓
CommitPropsAndClearFlags:  fiber.Props = fiber.PendingProps  (for ALL fibers)
```

**Allocations per host element per render:** BaseProps (1) + Style dict (1) + ToDictionary dict (1) = 3 objects.
**For 3000 stress test boxes:** 9000 allocations/frame.

### Where `fiber.Props` / `fiber.PendingProps` are read (exhaustive):

| Location | File:Line | What it does |
|----------|-----------|-------------|
| `CompleteWork` | FiberReconciler.cs:528 | `AreHostPropsEqual(fiber.PendingProps, fiber.Props)` |
| `CommitUpdate` | FiberReconciler.cs:981-984 | `_hostConfig.ApplyProperties(..., fiber.Props, fiber.PendingProps)` then `fiber.Props = fiber.PendingProps` |
| `CommitUpdate` (logging) | FiberReconciler.cs:956-965 | Reads `fiber.Props["text"]` and `fiber.PendingProps["text"]` for debug logging |
| `CommitPropsAndClearFlags` | FiberReconciler.cs:1378 | `fiber.Props = fiber.PendingProps` |
| `CommitPlacement` | FiberReconciler.cs:827-843 | `_hostConfig.ApplyProperties(..., null, fiber.PendingProps)` then `fiber.Props = fiber.PendingProps` |
| `CloneForReuse` | FiberFactory.cs:117-118 | `clone.Props = current.Props; clone.PendingProps = ExtractProps(newVNode)` |
| `CreateWorkInProgress` | FiberReconciler.cs:578-581 | `workInProgress.Props = current.Props; workInProgress.PendingProps = ExtractProps(vnode)` |
| `CreateNew` | FiberFactory.cs:26 | `PendingProps = ExtractProps(vnode)` |
| `FiberFunctionComponent` | FiberFunctionComponent.cs:90 | `wipFiber.Props = wipFiber.PendingProps` (for function component children) |

### Where `ExtractProps(vnode)` is defined (4 copies, all identical):

| File | Line |
|------|------|
| FiberFactory.cs | 210 |
| FiberReconciler.cs | 1275 |
| FiberChildReconciliation.cs | 479 |
| FiberFunctionComponent.cs | 359 |

All read `vnode.Properties ?? VirtualNode.EmptyProps`.

---

## <a name="proposed-flow"></a>2. Proposed Flow (typed)

```
Source gen emits:     V.Label(new LabelProps { Text = "hi", Style = new Style { Left = 5f } }, key: "k")
                                                    ↓
V.Label():           stores LabelProps directly     → NO ToDictionary call
                     new VirtualNode(...,
                       properties: EmptyProps,      ← empty dict (for backward compat / fallback)
                       hostProps: labelProps)        ← NEW: typed BaseProps stored alongside
                                                    ↓
FiberFactory:        fiber.PendingHostProps = vnode.HostProps
                     fiber.PendingProps = vnode.Properties (empty or fallback)
                                                    ↓
CompleteWork:        BaseProps.ShallowEquals(fiber.PendingHostProps, fiber.HostProps)
                       → typed field comparison (no dict iteration)
                       → EVERY built-in element takes this path (ShallowEquals is mandatory)
                                                    ↓ if not equal
CommitUpdate:        adapter.ApplyTypedDiff(element, fiber.HostProps, fiber.PendingHostProps)
                       → TypedPropsApplier.ApplyDiff for base fields
                       → adapter handles element-specific fields
                       → direct field-by-field comparison + style field comparison
                                                    ↓
CommitPropsAndClearFlags:  fiber.HostProps = fiber.PendingHostProps

     NOTE: Dict path (AreHostPropsEqual / PropsApplier.ApplyDiff) is ONLY used
     by V.Host() and VisualElementSafe — these are inherently dict-based APIs.
```

**Allocations per typed host element per render:** BaseProps (1) + Style (1) = 2 objects.
**Savings:** 1 dict allocation eliminated per element per render = 3000 dicts/frame saved.
**CPU savings:** No dict iteration in equality check or diff. Direct typed field comparison.

---

## <a name="implementation-steps"></a>3. Implementation Steps

### Step 1: Add `HostProps` field to `VirtualNode` and `FiberNode`

**Files:** `VNode.cs`, `FiberNode.cs`

```csharp
// VNode.cs — add field + constructor param:
public BaseProps HostProps { get; }

// Constructor: add hostProps parameter, store it
public VirtualNode(..., BaseProps hostProps = null)
{
    HostProps = hostProps;
    // ...
}

// Template copy constructor: copy HostProps from template
```

```csharp
// FiberNode.cs — add two fields:
/// <summary>Committed typed host props from last render.</summary>
public BaseProps HostProps;

/// <summary>Pending typed host props for next render.</summary>
public BaseProps PendingHostProps;
```

**Risk:** None. Additive fields. Existing code never reads them (they're null).

### Step 2: Store typed props in `V.*` factory methods

**Files:** `V.cs` (every host element method)

Change every `V.*` method from:
```csharp
public static VirtualNode Label(LabelProps props, string key = null)
{
    IReadOnlyDictionary<string, object> map = props?.ToDictionary();
    return new VirtualNode(..., properties: map ?? EmptyProps(), ...);
}
```

To:
```csharp
public static VirtualNode Label(LabelProps props, string key = null)
{
    return new VirtualNode(..., properties: EmptyProps(), ..., hostProps: props);
}
```

**Key change:** `ToDictionary()` is no longer called. The dict path gets `EmptyProps()` (zero
allocations). The typed BaseProps is stored on `HostProps`.

**VisualElementSafe:** Remains dict-based (its first param is `object`, not a typed props class).
No change needed.

**Host (V.Host):** Remains dict-based — it applies props to the root `VisualElement` via
`VNodeHostRenderer`, which uses `PropsApplier.Apply/ApplyDiff` on the dict. Converting `V.Host`
is a separate optimization that doesn't affect the hot path.

**Risk:** If any external code depends on `vnode.Properties` being populated for host elements,
it would now see an empty dict. See "What Breaks" section.

### Step 3: Propagate `HostProps` through the Fiber pipeline

**Files:** `FiberFactory.cs`, `FiberReconciler.cs`, `FiberChildReconciliation.cs`, `FiberFunctionComponent.cs`

3a. **`ExtractProps`** — all 4 copies: add extraction of `HostProps`:
```csharp
// Add companion method or inline:
private static BaseProps ExtractHostProps(VirtualNode vnode)
{
    if (vnode == null) return null;
    return vnode.HostProps;  // may be null (dict-path elements, text nodes, etc.)
}
```

3b. **`FiberFactory.CreateNew`** — set `PendingHostProps`:
```csharp
// In CreateNew, after setting PendingProps:
fiber.PendingHostProps = vnode?.HostProps;
```

3c. **`FiberFactory.CloneForReuse`** — copy typed host props:
```csharp
// After: clone.Props = current.Props;
clone.HostProps = current.HostProps;
// After: clone.PendingProps = ...;
clone.PendingHostProps = newVNode?.HostProps ?? current.PendingHostProps;
```

3d. **`FiberReconciler.CreateWorkInProgress`** — copy typed host props:
```csharp
workInProgress.HostProps = current.HostProps;
workInProgress.PendingHostProps = vnode?.HostProps ?? current.PendingHostProps;
```

3e. **`FiberFunctionComponent`** — function components don't have host props,
but the `wipFiber.Props = wipFiber.PendingProps` line (line 90) should be mirrored:
```csharp
wipFiber.HostProps = wipFiber.PendingHostProps;
```

**Risk:** Low. If `PendingHostProps` is null (dict-path element), the typed path is skipped
everywhere. The null check is the gating mechanism.

### Step 4: Add `BaseProps.ShallowEquals` for equality check

**Files:** `BaseProps.cs`

```csharp
/// <summary>
/// Field-by-field equality check for host element bailout.
/// Returns true if all set properties are equal between this and other.
/// </summary>
public virtual bool ShallowEquals(BaseProps other)
{
    if (other == null) return false;
    if (ReferenceEquals(this, other)) return true;
    if (GetType() != other.GetType()) return false;

    // Compare each field that BaseProps defines:
    if (Name != other.Name) return false;
    if (ClassName != other.ClassName) return false;
    if (!ReferenceEquals(Style, other.Style) && !StyleEquals(Style, other.Style)) return false;
    if (!ReferenceEquals(Ref, other.Ref)) return false;
    if (Visible != other.Visible) return false;
    if (Enabled != other.Enabled) return false;
    if (Tooltip != other.Tooltip) return false;
    // ... all event handlers: reference equality only
    if (OnClick != other.OnClick) return false;
    // ... (same for all 30+ handler fields)
    if (!ReferenceEquals(ExtraProps, other.ExtraProps)) return false;
    return true;
}

private static bool StyleEquals(Style a, Style b)
{
    if (a == null && b == null) return true;
    if (a == null || b == null) return false;
    if (a.Count != b.Count) return false;
    foreach (var kv in a)
    {
        if (!b.TryGetValue(kv.Key, out var bv)) return false;
        if (!object.Equals(kv.Value, bv)) return false;
    }
    return true;
}
```

**Subclass overrides:** Each typed props class (LabelProps, ButtonProps, etc.) should override
`ShallowEquals` to also compare their own fields. Example for LabelProps:
```csharp
public override bool ShallowEquals(BaseProps other)
{
    if (!base.ShallowEquals(other)) return false;
    if (other is not LabelProps lp) return false;
    if (Text != lp.Text) return false;
    if (EnableRichText != lp.EnableRichText) return false;
    return true;
}
```

**Note:** There are ~40 concrete props classes. Each **MUST** have a `ShallowEquals` override.
This is mechanical but tedious. Could be code-generated or done manually.

**There is no dict-fallback safety net.** If a field is missed in `ShallowEquals`, changes
to that field will NOT trigger an update — a silent rendering bug. This is mitigated
by **mandatory unit tests per props class** that verify every field is compared.

**No default fallback:** The base `ShallowEquals` compares all `BaseProps` fields. Each
subclass override calls `base.ShallowEquals(other)` then compares its own fields.
If a subclass forgets to override, only its element-specific fields are uncompared
(base fields are still checked). Tests catch this.

### Step 5: Modify `CompleteWork` to use typed equality

**Files:** `FiberReconciler.cs`

```csharp
private void CompleteWork(FiberNode fiber)
{
    switch (fiber.Tag)
    {
        case FiberTag.HostComponent:
            if (fiber.HostElement == null && fiber.ElementType != "root")
            {
                fiber.HostElement = _hostConfig.CreateElement(fiber.ElementType);
                fiber.EffectTag |= EffectFlags.Placement;
            }
            else if (fiber.PendingHostProps != null)
            {
                // Typed path: all built-in elements take this branch
                if (!fiber.PendingHostProps.ShallowEquals(fiber.HostProps))
                {
                    fiber.EffectTag |= EffectFlags.Update;
                }
            }
            else if (!AreHostPropsEqual(fiber.PendingProps, fiber.Props))
            {
                // Dict path: only V.Host() and VisualElementSafe reach here
                fiber.EffectTag |= EffectFlags.Update;
            }
            break;
    }
    // ...
}
```

### Step 6: Add `TypedPropsApplier` for typed diff+apply

**Files:** New file `Shared/Props/TypedPropsApplier.cs`

This is the **core of the typed pipeline**. Instead of iterating dictionary entries
and looking up string keys, it compares typed fields directly and calls the
corresponding VisualElement style/property setters.

```csharp
public static class TypedPropsApplier
{
    /// <summary>
    /// Apply only the changed properties from prev to next on the given element.
    /// </summary>
    public static void ApplyDiff(VisualElement element, BaseProps prev, BaseProps next)
    {
        prev ??= s_emptyBaseProps;
        next ??= s_emptyBaseProps;

        // --- Name ---
        if (prev.Name != next.Name)
            element.name = next.Name ?? string.Empty;

        // --- ClassName ---
        if (prev.ClassName != next.ClassName)
            ApplyClassNameDiff(element, prev.ClassName, next.ClassName);

        // --- Visible ---
        if (prev.Visible != next.Visible)
            element.visible = next.Visible ?? true;

        // --- Enabled ---
        if (prev.Enabled != next.Enabled)
            element.SetEnabled(next.Enabled ?? true);

        // --- Style ---
        if (!ReferenceEquals(prev.Style, next.Style))
            ApplyStyleDiff(element, prev.Style, next.Style);

        // --- Ref ---
        if (!ReferenceEquals(prev.Ref, next.Ref))
            RefUtility.Assign(next.Ref, element);

        // --- Event handlers ---
        DiffHandler(element, "onClick", prev.OnClick, next.OnClick);
        DiffHandler(element, "onPointerDown", prev.OnPointerDown, next.OnPointerDown);
        // ... (all 30+ event handler fields)

        // --- ExtraProps fallback ---
        if (!ReferenceEquals(prev.ExtraProps, next.ExtraProps))
            ApplyExtraPropsDiff(element, prev.ExtraProps, next.ExtraProps);
    }

    /// <summary>
    /// Apply all properties from props (no previous state).
    /// Used on initial placement.
    /// </summary>
    public static void ApplyFull(VisualElement element, BaseProps props)
    {
        if (props == null) return;
        if (props.Name != null) element.name = props.Name;
        if (props.ClassName != null) ApplyClassName(element, props.ClassName);
        if (props.Visible.HasValue) element.visible = props.Visible.Value;
        if (props.Enabled.HasValue) element.SetEnabled(props.Enabled.Value);
        if (props.Style != null) ApplyStyleFull(element, props.Style);
        if (props.Ref != null) RefUtility.Assign(props.Ref, element);
        if (props.OnClick != null) RegisterHandler(element, "onClick", props.OnClick);
        // ... all handlers
        if (props.ExtraProps != null) ApplyExtraProps(element, props.ExtraProps);
    }

    private static void ApplyStyleDiff(VisualElement el, Style prev, Style next)
    {
        // Both are Dictionary<string, object> internally.
        // Use the existing PropsApplier style-diff logic but operating on the
        // Style dict directly, avoiding the outer props dict iteration.
        var prevMap = (IDictionary<string, object>)prev ?? PropsApplier.EmptyMutableProps;
        var nextMap = (IDictionary<string, object>)next ?? PropsApplier.EmptyMutableProps;
        PropsApplier.ApplyStyleDiff(el, prevMap, nextMap);
    }
}
```

**Design decision:** `TypedPropsApplier.ApplyDiff` handles the base properties that ALL elements
share (name, className, style, visibility, event handlers). Element-specific properties
(e.g., `Button.text`, `Label.enableRichText`, `Slider.value`) need per-adapter typed handling.

**Two approaches for element-specific props:**

**Option A — Keep adapters with typed overload (recommended):**
Each adapter gains a `void ApplyTypedDiff(VisualElement, BaseProps prev, BaseProps next)` method
that calls `TypedPropsApplier.ApplyDiff(element, prev, next)` for base properties, then handles
its own typed fields:

```csharp
// In ButtonElementAdapter:
public void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
{
    TypedPropsApplier.ApplyDiff(element, prev, next);  // base fields
    if (prev is ButtonProps bp && next is ButtonProps bn)
    {
        if (bp.Text != bn.Text) ((Button)element).text = bn.Text ?? string.Empty;
    }
}
```

**Option B — TypedPropsApplier handles everything via virtual dispatch:**
`BaseProps` gains a virtual `DiffAndApply(VisualElement, BaseProps prev)` method.
Each props subclass overrides it. Fewer files changed but tighter coupling.

**Recommended: Option A.** It keeps the adapter pattern consistent with existing architecture.

### Step 7: Wire typed path into `CommitUpdate` and `CommitPlacement`

**Files:** `FiberReconciler.cs`, `FiberHostConfig.cs`

7a. **`CommitUpdate`:**
```csharp
private void CommitUpdate(FiberNode fiber)
{
    if (fiber.HostElement == null) return;

    if (fiber.PendingHostProps != null)
    {
        // Typed path — all built-in elements
        _hostConfig.ApplyTypedProperties(
            fiber.HostElement,
            fiber.ElementType,
            fiber.HostProps,        // prev (committed)
            fiber.PendingHostProps  // next (pending)
        );
        fiber.HostProps = fiber.PendingHostProps;
    }
    else
    {
        // Dict path — only V.Host() and VisualElementSafe reach here
        _hostConfig.ApplyProperties(
            fiber.HostElement,
            fiber.ElementType,
            fiber.Props,
            fiber.PendingProps
        );
    }

    fiber.Props = fiber.PendingProps;
}
```

7b. **`CommitPlacement`** — similar: if `PendingHostProps != null`, use typed initial apply.

7c. **`FiberHostConfig`:**
```csharp
public void ApplyTypedProperties(
    VisualElement element,
    string elementType,
    BaseProps oldProps,
    BaseProps newProps)
{
    // All built-in adapters implement ITypedElementAdapter.
    // Only V.Host() / VisualElementSafe use the dict path (they never reach here).
    var adapter = (ITypedElementAdapter)_registry.Resolve(elementType);
    if (oldProps != null)
        adapter.ApplyTypedDiff(element, oldProps, newProps);
    else
        adapter.ApplyTypedFull(element, newProps);
}
```

7d. **New interface `ITypedElementAdapter`:**
```csharp
public interface ITypedElementAdapter
{
    void ApplyTypedFull(VisualElement element, BaseProps props);
    void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next);
}
```

Adapters that opt in implement this alongside `IElementAdapter`. The `BaseElementAdapter`
base class can provide a default implementation that calls `TypedPropsApplier`.

### Step 8: Update `CommitPropsAndClearFlags`

**Files:** `FiberReconciler.cs`

Add `fiber.HostProps = fiber.PendingHostProps;` alongside the existing `fiber.Props = fiber.PendingProps;`:
```csharp
fiber.Props = fiber.PendingProps;
fiber.HostProps = fiber.PendingHostProps;
```

### Step 9: CommitUpdate logging (text extraction)

**Files:** `FiberReconciler.cs:956-965`

The debug logging reads `fiber.Props["text"]` / `fiber.PendingProps["text"]`. For the typed path:
```csharp
// Replace dict lookup with typed props check:
if (fiber.PendingHostProps is LabelProps lp) newText = lp.Text;
else if (fiber.PendingProps?.TryGetValue("text", out var nv) == true && nv is string ns) newText = ns;
```

Or simpler: only log from `PendingHostProps` when available, else fall back to dict.

---

## <a name="system-impact"></a>4. System-by-System Impact

### HMR (Hot Module Replacement)
**Impact: NONE**

HMR operates at the delegate level — it swaps `fiber.TypedRender` and triggers re-render.
It never reads `fiber.Props`, `fiber.PendingProps`, `vnode.Properties`, or calls `PropsApplier`.
The new `HostProps` / `PendingHostProps` fields are invisible to HMR.

Files verified (all clean):
- `Editor/HMR/UitkxHmrDelegateSwapper.cs` — walks fibers but only reads `TypedRender`, `ComponentState`
- `Editor/HMR/HmrCSharpEmitter.cs` — emits C# source code, doesn't touch runtime props
- `Editor/HMR/UitkxHmrController.cs` — orchestration only
- All other HMR files — no props pipeline access

### Source Generator
**Impact: NONE (for the typed pipeline change itself)**

The source generator emits code like:
```csharp
V.Label(new LabelProps { Text = "hi" }, key: "k")
```

This call pattern is **unchanged**. The `V.Label()` method signature stays the same.
Only its internal implementation changes (stops calling `ToDictionary()`).

The source generator does NOT need to emit different code for the typed pipeline to work.
The generated code already constructs typed `BaseProps` subclass instances — we just
stop throwing away the type information inside `V.*()`.

**PropsResolver note:** `<VisualElement>` is currently hardcoded as `BuiltinDictionary` in the
fallback map (PropsResolver.cs:836). The Roslyn-based scanner already classifies it as
`BuiltinTyped` when the compilation has `V.VisualElement(VisualElementProps)`. So the fallback
map should be updated to match: `["visualelement"] = Typed("VisualElement", "VisualElementProps", children: true)`.
This is a consistency fix, not required for the typed pipeline.

### IDE Extensions (LSP + VS Code)
**Impact: NONE**

- **LSP server:** Works on `.uitkx` source text and Roslyn type metadata. Never references
  `ToDictionary`, `Properties`, `PropsApplier`, or any runtime dict. All references to
  `BaseProps` are for scanning property names for autocompletion.
- **VS Code extension:** TypeScript code. No C# runtime knowledge.
- **Grammar/syntax highlighting:** Pure TextMate/regex. No semantic awareness.

### VNodeHostRenderer (non-Fiber V.Host path)
**Impact: NEEDS UPDATE (small)**

`VNodeHostRenderer.ApplyHostProps()` reads `vnode.Properties` for the `V.Host()` root element.
Since `V.Host()` remains dict-based (it's a rare root-level call), this continues to work.

If `V.Host()` were converted to typed (not recommended initially), `VNodeHostRenderer` would
need to read `vnode.HostProps` instead.

### PropsApplier
**Impact: NOT CHANGED — kept as-is**

`PropsApplier.Apply` and `PropsApplier.ApplyDiff` continue to exist and handle:
- Dict-path elements (VisualElementSafe, custom adapters)
- The `ExtraProps` escape hatch
- Style diff logic (reused by `TypedPropsApplier.ApplyStyleDiff`)

`PropsApplier` is NOT modified. `TypedPropsApplier` is a NEW separate class that handles
the typed path. For style diffing, `TypedPropsApplier` delegates to `PropsApplier`'s
existing style diff internals (which operate on `IDictionary<string, object>` — and Style
IS a `Dictionary<string, object>`).

### Element Adapters (57 adapters)
**Impact: MANDATORY — ALL ADAPTERS CONVERTED**

Every adapter implements `ITypedElementAdapter`. No half-measures.

The `BaseElementAdapter` base class provides the default `ITypedElementAdapter` implementation
that calls `TypedPropsApplier.ApplyDiff` for all base fields (name, className, style, visible,
enabled, ref, event handlers). Each concrete adapter overrides to handle element-specific fields.

**Each adapter conversion is ~10-30 lines:** Call `base.ApplyTypedDiff` for base props,
then compare+apply element-specific typed fields (e.g., `Button.text`, `Label.enableRichText`).

---

## <a name="what-breaks"></a>5. What Breaks

### 5.1. `vnode.Properties` becomes empty for typed elements

**Affected code:** Anything that reads `vnode.Properties` for a built-in host element
after the change.

**Known readers:**
- `ExtractProps(vnode)` (4 copies) — returns `vnode.Properties`. This is used to set
  `fiber.PendingProps`. For typed elements, this will return `EmptyProps`. That's fine
  because `CompleteWork` and `CommitUpdate` check `PendingHostProps != null` first.
- `VNodeHostRenderer.ApplyHostProps()` — reads `vnode.Properties` for `V.Host()` roots.
  `V.Host()` stays dict-based, so this is unaffected.

**Risk:** If user code reads `vnode.Properties` directly (e.g., for debugging or custom
rendering), they'd see an empty dict for typed elements. This is unlikely — VirtualNode
is mostly an internal type.

### 5.2. `fiber.Props` / `fiber.PendingProps` become empty for typed elements

**Affected code:**
- `CommitUpdate` logging (line 956-965) — reads `fiber.Props["text"]`. Fix: check
  `PendingHostProps` first.
- `AreHostPropsEqual` — called in `CompleteWork`. For typed elements, `CompleteWork` takes
  the typed path first, so the dict path is never reached.
- `CommitPropsAndClearFlags` — does `fiber.Props = fiber.PendingProps`. Still works (copies
  empty dict). Also needs `fiber.HostProps = fiber.PendingHostProps`.

**Risk:** Low. The dict fields are still maintained (set to EmptyProps). They just contain
no useful data for typed elements. Nothing reads them for typed elements.

### 5.3. `BaseProps` subclasses need `ShallowEquals` overrides

If `ShallowEquals` is not overridden on a subclass, the base `ShallowEquals` won't compare
element-specific fields (e.g., `LabelProps.Text`). This means changes to those fields
would not trigger `EffectFlags.Update`, and the element wouldn't update visually.

**Mitigation:** The `ApplyTypedDiff` in the adapter would still apply the diff
(since the fiber IS marked for update when `ShallowEquals` returns false at the base
level for any changed base field). But if ONLY a subclass-specific field changed
(e.g., only `Text` changed, everything else identical), `ShallowEquals` would return
true, the fiber would NOT be marked for update, and the text would not change.

**This is a correctness risk.** Every props subclass MUST override `ShallowEquals`
to compare its own fields. Missing an override = silent rendering bugs.

**Mitigation: Mandatory unit tests.** Each props class gets a test that:
1. Creates two identical instances, asserts `ShallowEquals` returns true.
2. Changes each field one at a time, asserts `ShallowEquals` returns false.

This catches missing fields at test time, not at runtime. No dict-fallback safety net.
The base `ShallowEquals` does NOT call `ToDictionary()` — it only compares `BaseProps`
fields. Subclass overrides call `base.ShallowEquals(other)` then compare their own fields.

### 5.4. Tests that assert on generated code shape

**Affected:** `EmitterTests.cs` — some tests assert on strings like `"V.Box("`.
None currently assert on `V.VisualElement` dict code. If `<VisualElement>` is
reclassified in the fallback map, one test might need updating.

---

## <a name="what-does-not-break"></a>6. What Does NOT Break

| System | Why |
|--------|-----|
| **HMR** | Operates on TypedRender delegates, not props |
| **Source Generator** | Emitted code pattern unchanged (`V.Label(new LabelProps { ... })`) |
| **IDE Extensions** | Work on source text / Roslyn metadata, not runtime |
| **Grammar / syntax highlighting** | Pure regex/TextMate |
| **Existing dict-path elements** | VisualElementSafe, custom adapters, V.Host — unchanged |
| **ExtraProps escape hatch** | Still works: TypedPropsApplier handles ExtraProps field |
| **Context system** | Uses `ProvidedContext` dict, separate from props |
| **Hooks / Effects** | Use ComponentState, not props dicts |
| **Router / Link / Animate** | Function components using IProps typed path, not host props |
| **ErrorBoundary / Suspense** | Use dedicated VNode fields, not Properties dict |
| **User-written C# calling V.VisualElement()** | Method signature unchanged; `null` still resolves correctly |

---

## <a name="risk-mitigations"></a>7. Risk Mitigations

| Risk | Mitigation |
|------|-----------|
| Missing `ShallowEquals` override → silent render bug | Mandatory unit tests per props class verify every field is compared. No runtime fallback. |
| `vnode.Properties` empty for typed elements | `V.Host()` and VisualElementSafe stay dict-based. VirtualNode is internal. |
| Adapter missing `ITypedElementAdapter` | N/A — ALL adapters implement it. `BaseElementAdapter` provides default. |
| Style fields are set-only (no getters) | `Style` IS a `Dictionary<string, object>`. Read back via dict indexer: `style["left"]`. Or: `TypedPropsApplier.ApplyStyleDiff` delegates to `PropsApplier`'s style diff which already works on `IDictionary<string, object>`. No Style getters needed. |
| Event handler registration/unregistration in typed path | `TypedPropsApplier.DiffHandler()` reuses `PropsApplier`'s event registration internals via `NodeMetadata`. The event plumbing doesn't change — only the way we detect which handlers changed. |
| 40 props classes need `ShallowEquals` | All done at once. Mechanical but each is ~5-15 lines. Unit tests verify completeness. |
| Thread safety | Same as current — all rendering is single-threaded (Unity main thread). `[ThreadStatic]` not needed. |

---

## <a name="testing-strategy"></a>8. Testing Strategy

### Unit tests

1. **ShallowEquals correctness** — per props class: verify that changing each field
   makes `ShallowEquals` return false, and identical values return true.
2. **TypedPropsApplier.ApplyDiff** — verify each field diff applies the correct change
   to a mock VisualElement.
3. **TypedPropsApplier.ApplyFull** — verify initial apply sets all fields.
4. **V.Host / VisualElementSafe** — verify these dict-path elements still work correctly.
5. **Mixed tree** — tree with typed elements and V.Host root renders correctly.

### Integration tests

6. **Stress test** — run existing `StressTest.uitkx` with typed pipeline, verify visual
   correctness and measure FPS improvement.
7. **HMR test** — edit a `.uitkx` file, verify HMR still works (delegate swap unaffected).
8. **All samples** — run each sample app, verify no visual regressions.

### Regression guard

9. **ShallowEquals completeness** — for each props class, a test mutates every
   field individually and asserts `ShallowEquals` detects the change.

---

## <a name="file-changes"></a>9. File-by-File Change List

### Core changes (required)

| File | Change | Effort |
|------|--------|--------|
| `Shared/Core/VNode.cs` | Add `HostProps` field + constructor param | Small |
| `Shared/Core/Fiber/FiberNode.cs` | Add `HostProps`, `PendingHostProps` fields | Tiny |
| `Shared/Core/V.cs` | Stop calling `ToDictionary()`, pass `hostProps` to VNode | Medium (40+ methods, mechanical) |
| `Shared/Core/Fiber/FiberFactory.cs` | Copy `HostProps`/`PendingHostProps` in Create/Clone | Small |
| `Shared/Core/Fiber/FiberReconciler.cs` | Typed path in `CompleteWork`, `CommitUpdate`, `CommitPlacement`, `CommitPropsAndClearFlags` | Medium |
| `Shared/Core/Fiber/FiberHostConfig.cs` | Add `ApplyTypedProperties` dispatch | Small |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Copy `HostProps` in inline fiber creation | Small |
| `Shared/Core/Fiber/FiberFunctionComponent.cs` | `wipFiber.HostProps = wipFiber.PendingHostProps` | Tiny |
| `Shared/Props/Typed/BaseProps.cs` | Add `ShallowEquals` virtual method | Medium |
| `Shared/Props/TypedPropsApplier.cs` | **NEW FILE** — typed diff + apply logic | Large |
| `Shared/Elements/IElementAdapter.cs` | Add `ITypedElementAdapter` interface (separate or extension) | Small |
| `Shared/Elements/BaseElementAdapter.cs` | Default `ITypedElementAdapter` implementation | Small |

### Adapter conversions (mandatory, all at once)

| File | Change | Effort |
|------|--------|--------|
| `Shared/Elements/BaseElementAdapter.cs` | Default `ITypedElementAdapter` implementation (base fields) | Medium |
| All 57 concrete adapters | Override `ApplyTypedDiff`/`ApplyTypedFull` for element-specific fields | Small each, medium total |

### Props class `ShallowEquals` overrides (mandatory, all at once)

| File | Change | Effort |
|------|--------|--------|
| All ~40 concrete props classes | Override `ShallowEquals` to compare element-specific fields | Small each, medium total |
| + Unit tests per props class | Verify every field is compared | Small each, medium total |

### NOT changed

| File | Why |
|------|-----|
| `Editor/HMR/*` | Doesn't touch props pipeline |
| `ide-extensions~/*` | Doesn't touch runtime |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Emitted code pattern unchanged |
| `Shared/Props/PropsApplier.cs` | Kept as-is for dict fallback path |
| `Shared/Core/VNodeHostRenderer.cs` | Uses V.Host() which stays dict-based |
| `Shared/Props/Typed/Style.cs` | Still a `Dictionary<string, object>` — typed Style is a follow-up (see §10) |

---

## Implementation Order

1. **VNode.cs + FiberNode.cs** — add fields (no behavior change, everything null)
2. **V.cs** — stop calling `ToDictionary()`, store `hostProps` on VNode
3. **FiberFactory.cs + FiberChildReconciliation.cs + FiberFunctionComponent.cs** — propagate typed host props through fiber pipeline
4. **BaseProps.cs** — add `ShallowEquals` (no fallback — compares BaseProps fields only)
5. **All ~40 props subclasses** — add `ShallowEquals` overrides (compare element-specific fields)
6. **FiberReconciler.cs** — add typed path in `CompleteWork` (equality check)
7. **TypedPropsApplier.cs** — implement typed diff + apply
8. **ITypedElementAdapter + BaseElementAdapter** — interface + default implementation (handles base fields)
9. **All 57 adapters** — implement `ITypedElementAdapter` overrides for element-specific fields
10. **FiberHostConfig.cs + FiberReconciler.cs CommitUpdate/CommitPlacement** — wire typed apply path
11. **Run tests** — verify correctness
12. **Run stress test** — measure FPS improvement

---

## <a name="typed-style"></a>10. Follow-up: Typed Style

**Current state:** `Style` inherits `Dictionary<string, object>`. Every style property setter
does `this["left"] = value` where `value` is a Unity struct (`StyleLength`, `StyleColor`, etc.).
This means **every style value is boxed** — a heap allocation per property per frame.

For 3000 boxes with ~5 style props each: **~15,000 boxing allocations/frame.**

**Proposed:** Replace `Style` with a struct-backed typed class:
- ~80 typed backing fields (no dictionary, no boxing)
- A `ulong` bitmask (or two) to track which properties are set
- Typed getters + setters
- A typed `DiffAndApply(VisualElement, Style prev, Style next)` method that only compares set bits

**This is independent of the typed props pipeline** and should be done as a separate step after
the pipeline is working. The two optimizations stack: typed pipeline eliminates dict allocs,
typed Style eliminates boxing allocs.

**Estimated additional gain:** Eliminates ~15,000 boxing allocs/frame in the stress test.
Combined with the typed pipeline, total allocation reduction: ~24,000 objects/frame → ~3,000
(just the BaseProps instances, which could later be pooled).

---

## 11. Follow-up: Incremental GC Tuning

**Problem:** Even after eliminating dict and boxing allocations, the remaining per-frame
allocations (BaseProps instances, user closures, list copies in hooks) still trigger
Gen0 GC collections. In Unity, the default Mono GC does stop-the-world collections that
cause visible frame spikes ("GC stutters").

**Unity's Incremental GC:**
- Available since Unity 2019.1, enabled via **Project Settings → Player → Configuration → Use Incremental GC**
- Spreads GC work across multiple frames instead of one long pause
- Does NOT reduce total GC time — just amortizes it
- Controlled via `GarbageCollector.GCMode` (`.Enabled`, `.Disabled`, `.Manual`)
- `GarbageCollector.incrementalTimeSliceNanoseconds` tunes per-frame budget (default: 3ms)

**What to document / recommend:**
1. Verify incremental GC is enabled in the stress test project
2. Recommend incremental GC in UITKX documentation as a best practice
3. Consider exposing a `FiberConfig.GCBudgetNs` knob that tunes the incremental slice
4. Consider `GC.TryStartNoGCRegion()` around the commit phase (risky, needs measurement)

**This is independent of code changes** — it's a project-level setting + documentation.
Should be addressed after Typed Style is done and the remaining allocation profile is measured.
