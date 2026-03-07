# Refactor: Typed `IProps` Records — Implementation Plan

**Branch:** `refactory_types_in_core_library`  
**Status:** Planning — not yet started  
**Scope:** Framework core (Shared/), V.cs, built-in Props types, and the UITKX source generator

---

## 1. Goals and Motivation

### Current State

Every function component in the framework exchanges props via
`Dictionary<string, object>` (or `IReadOnlyDictionary<string, object>`).

```csharp
// caller site (hand-written or generator output)
V.Func(MyComponent.Render, new Dictionary<string, object>
{
    { "name", currentName },
    { "color", Color.red },
});

// component Render method
public static VirtualNode Render(Dictionary<string,object> rawProps, ...)
{
    var name  = rawProps?["name"]  as string ?? "";
    var color = rawProps != null && rawProps.TryGetValue("color", out var c)
                ? (Color)c : Color.white;
    ...
}
```

**Problems:**

| # | Problem |
|---|---------|
| 1 | **No compile-time safety.** Typos in key strings are silent bugs. |
| 2 | **Boxing on every value type.** `Color`, `float`, `int`, `bool` are boxed into `object` on every render cycle. |
| 3 | **Heap allocation per render.** A new `Dictionary` is allocated at every call site on every re-render, plus `CloneProps` creates another `ReadOnlyDictionary` wrapper inside `VirtualNode`'s constructor. |
| 4 | **`ArePropsEqual` is O(n) per key** and does per-value `object.Equals` (with unboxing). Records generate structural equality for free. |
| 5 | **Generator relies on a `"__typed"` sentinel hack.** The `@props` directive already generates a typed record, but the `Render` method still receives a `Dictionary` and has to fish out `__rawProps["__typed"]` to unwrap it — defeating the point. |
| 6 | **Poor IDE experience.** No autocomplete on reading props, no rename-symbol support, no type hierarchy. |

### Goals After Refactor

- Every function component's `Render` method accepts `IProps` and casts to its specific record type at the top.
- `V.Func` forwards an `IProps` object directly — no dictionary wrapper anywhere in the hot path.
- `ArePropsEqual` is a single `props1.Equals(props2)` call.
- The source generator emits `new MyComponentProps { Name = name }` at call sites and `(MyComponentProps)rawProps` at the cast site.
- Built-in elements (`Button`, `Label`, etc.) whose props classes already exist in `ReactiveUITK.Props.Typed` adopt `IProps` directly (no change at call sites since `V.Button(new ButtonProps{...})` already works).
- The old `Dictionary<string,object>` path through `V.Func` stays alive during migration but is deprecated.

---

## 2. Chosen Architecture

> **Single `IProps` marker interface + plain classes** (NOT generic `VirtualNode<TProps>`)

### Rationale

- Generics on `VirtualNode` would force `FiberNode` to also become generic, making the fiber tree heterogeneous and requiring casts at every reconciliation step — no net win.
- On IL2CPP (Unity's AOT compiler), reference-type generics share a single code path; making `VirtualNode<ButtonProps>` generic produces the same machine code as a cast.
- A cast `(TProps)rawProps` has negligible cost (~2-3 ns, branch-predictable), and appears only once per component `Render` invocation.

### `IProps` Design

```csharp
namespace ReactiveUITK.Core
{
    /// <summary>Marker interface — all typed component prop classes implement this.</summary>
    public interface IProps { }

    /// <summary>Sentinel for components that declare no props.</summary>
    public sealed class EmptyProps : IProps
    {
        public static readonly EmptyProps Instance = new EmptyProps();
        private EmptyProps() {}
    }
}
```

All typed prop classes are **plain C# classes** with public settable properties (or C# 9 positional records where the project targets netstandard 2.1+). The framework does not mandate records — any class with a sensible value-equality implementation works, but records are the recommended authoring pattern.

---

## 3. Affected Files — Summary Table

| Layer | File | Nature of Change |
|-------|------|-----------------|
| Core | `Shared/Core/IProps.cs` | **New file** — `IProps` + `EmptyProps` |
| Core | `Shared/Core/VNode.cs` | `Properties` field type, `FunctionRender` delegate signature, `CloneProps` removal |
| Core | `Shared/Core/Fiber/FiberNode.cs` | `Props`, `PendingProps`, `Render` field types |
| Core | `Shared/Core/Fiber/FiberFactory.cs` | `ExtractTypedProps` helper, usage in `CreateNew` and `CloneForReuse` |
| Core | `Shared/Core/Fiber/FiberReconciler.cs` | `ExtractProps` return type, special-case props for `Suspense`/`Text`, `AreHostPropsEqual` left as-is |
| Core | `Shared/Core/Fiber/FiberFunctionComponent.cs` | `ArePropsEqual` signature + body, render invocation cast |
| Core | `Shared/Core/V.cs` | New `V.Func<TProps>` overload; existing overload deprecated, kept for migration |
| Props | `Shared/Props/Typed/*.cs` (all built-in prop classes) | Add `: IProps` to every class |
| Generator | `SourceGenerator~/Emitter/CSharpEmitter.cs` | `Render` signature, props-binding pattern, `EmitFuncComponent` output |
| Generator | `SourceGenerator~/Emitter/PropsResolver.cs` | Detect typed-props availability for PascalCase tags |
| Samples | All hand-written `Render(Dictionary<string,object> …)` methods | Migrate one-by-one to `Render(IProps rawProps, …)` + top-of-method cast |

---

## 4. Step-by-Step Implementation

### Phase 0 — Create `IProps` marker (prerequisite)

**File:** `Shared/Core/IProps.cs` *(new)*

```csharp
// Shared/Core/IProps.cs
namespace ReactiveUITK.Core
{
    public interface IProps { }

    public sealed class EmptyProps : IProps
    {
        public static readonly EmptyProps Instance = new EmptyProps();
        private EmptyProps() { }

        // Records/classes that have no props should return Instance.
        public override bool Equals(object obj) => obj is EmptyProps;
        public override int GetHashCode() => 0;
    }
}
```

No other changes yet. Compile and verify — zero downstream breakage at this point.

---

### Phase 1 — `VirtualNode` (`Shared/Core/VNode.cs`)

#### 1a. Replace `Properties` field

**Before:**
```csharp
public IReadOnlyDictionary<string, object> Properties { get; }
```

**After:**
```csharp
public IProps TypedProps { get; }

// Keep the old name as a computed shim for the transition period —
// remove once every consumer is migrated.
// [Obsolete("Use TypedProps")]
// public IReadOnlyDictionary<string,object> Properties => ...
```

> **Decision point:** The built-in element path (`V.Button`, `V.Label`, etc.) currently converts its typed props to an `IReadOnlyDictionary` inside `V.Button` and stores that dictionary in `VirtualNode.Properties`. After this refactor the built-in element path *also* stores the native `ButtonProps : IProps` directly. The dict-based host-element reconciliation (`AreHostPropsEqual`, style binding, USS class application) must be updated separately — see Phase 5 note.

#### 1b. Change `FunctionRender` delegate type

**Before:**
```csharp
public Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> FunctionRender { get; }
```

**After:**
```csharp
public Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> FunctionRender { get; }
```

#### 1c. Update constructor

```csharp
// Old parameter: IReadOnlyDictionary<string,object> properties
// New parameter: IProps typedProps

// Remove:
//   this.Properties = CloneProps(properties);
// Add:
//   this.TypedProps = typedProps ?? EmptyProps.Instance;
```

The `CloneProps` method (currently a defensive copy via `new ReadOnlyDictionary<>(dict)`) is no longer needed because `IProps` instances are immutable records/classes; **delete it**.

#### 1d. Update implicit operator (if present)

The implicit operator from render delegate matches the new `FunctionRender` type — update its source type accordingly.

---

### Phase 2 — `FiberNode` (`Shared/Core/Fiber/FiberNode.cs`)

Three field types change:

```csharp
// Before
public IReadOnlyDictionary<string, object> Props         { get; set; }
public IReadOnlyDictionary<string, object> PendingProps  { get; set; }
public Func<Dictionary<string, object>,
            IReadOnlyList<VirtualNode>,
            VirtualNode>                   Render        { get; set; }

// After
public IProps Props         { get; set; }
public IProps PendingProps  { get; set; }
public Func<IProps,
            IReadOnlyList<VirtualNode>,
            VirtualNode>   Render        { get; set; }
```

The `ReadsContext` flag, `HasPendingStateUpdate`, `MemoizeProps` and everything else on `FiberNode` are unchanged.

---

### Phase 3 — `FiberFactory` (`Shared/Core/Fiber/FiberFactory.cs`)

#### 3a. Replace `ExtractProps`

Current signature:
```csharp
private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
```

New signature:
```csharp
private static IProps ExtractTypedProps(VirtualNode vnode) =>
    vnode?.TypedProps ?? EmptyProps.Instance;
```

The special-cases for `Suspense` and `Text` that live in `FiberReconciler.ExtractProps` (see Phase 4) are handled there; `FiberFactory.ExtractTypedProps` is used only for function-component fibers and host-element fibers, both of which now carry an `IProps`.

> **Text node special case** — `V.Text` stores content in `VirtualNode.TextContent`, not in props. No props object needed. The fiber for a text node gets `EmptyProps.Instance`.

#### 3b. Update `CreateNew` and `CloneForReuse`

Replace:
```csharp
fiber.PendingProps = ExtractProps(vnode);
```
With:
```csharp
fiber.PendingProps = ExtractTypedProps(vnode);
```

---

### Phase 4 — `FiberReconciler` (`Shared/Core/Fiber/FiberReconciler.cs`)

#### 4a. `ExtractProps` return type

```csharp
// Before
private IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)

// After
private IProps ExtractProps(VirtualNode vnode)
```

Body: return `vnode.TypedProps ?? EmptyProps.Instance`.

#### 4b. Suspense and Text special-case props

Currently `ExtractProps` manually constructs a dict for Suspense:
```csharp
// Current (approximate)
if (vnode.Type == VirtualNodeType.Suspense)
    return new Dictionary<string,object> { ["fallback"] = vnode.FallbackRender };
```

After the refactor, introduce lightweight typed-props classes:

```csharp
// New — in Shared/Core/SuspenseProps.cs or alongside Suspense
internal sealed class SuspenseProps : IProps
{
    public Func<VirtualNode> Fallback { get; init; }
    // Structural equality is fine with a simple manual override or positional record
}

internal sealed class TextProps : IProps
{
    public string Content { get; init; }
    public override bool Equals(object obj) => obj is TextProps t && t.Content == Content;
    public override int GetHashCode() => Content?.GetHashCode() ?? 0;
}
```

Replace the manual dict construction with `new SuspenseProps { Fallback = vnode.FallbackRender }`.

#### 4c. `AreHostPropsEqual`

This method (around line 1359) compares `IReadOnlyDictionary` props for host elements (VisualElement, Button etc.) — **it does NOT need to change in this phase**. Host-element props are still stored as a dict keyed on CSS-style property names and processed by the style-binding layer.

> **Future work:** Host-element props can be migrated to a typed representation separately — that is a larger change involving the style-binding code and is explicitly out of scope here.

---

### Phase 5 — `FiberFunctionComponent` (`Shared/Core/Fiber/FiberFunctionComponent.cs`)

#### 5a. `ArePropsEqual`

**Before:**
```csharp
private static bool ArePropsEqual(
    IReadOnlyDictionary<string,object> props1,
    IReadOnlyDictionary<string,object> props2)
{
    if (ReferenceEquals(props1, props2)) return true;
    if (props1 == null || props2 == null) return false;
    if (props1.Count != props2.Count) return false;
    foreach (var kv in props1)
    {
        if (!props2.TryGetValue(kv.Key, out var v2)) return false;
        if (!object.Equals(kv.Value, v2)) return false;
    }
    return true;
}
```

**After:**
```csharp
private static bool ArePropsEqual(IProps props1, IProps props2)
{
    if (ReferenceEquals(props1, props2)) return true;
    if (props1 == null || props2 == null) return false;
    return props1.Equals(props2);
    // Record-generated Equals does structural comparison automatically.
    // For hand-written classes, the developer is responsible for overriding Equals.
}
```

This is the largest single performance improvement of the entire refactor: the per-render O(n) dict walk with boxing becomes a single virtual dispatch (plus the record's field comparisons, which the compiler emits as a direct struct-copy comparison for primitive fields).

#### 5b. Render invocation

**Before:**
```csharp
wipFiber.Render(wipFiber.PendingProps as Dictionary<string, object>, children)
```

**After:**
```csharp
wipFiber.Render(wipFiber.PendingProps, children)
```

The cast is no longer needed — `PendingProps` is already `IProps`, and the delegate accepts `IProps`.

---

### Phase 6 — `V.cs` (`Shared/Core/V.cs`)

#### 6a. New typed overload

Add alongside the existing `V.Func`:

```csharp
/// <summary>
/// Typed-props overload. Preferred over the dictionary-based overload.
/// TProps must implement IProps.
/// </summary>
public static VirtualNode Func<TProps>(
    Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
    TProps props,
    string key = null,
    params VirtualNode[] children
) where TProps : class, IProps
{
    return new VirtualNode(
        VirtualNodeType.FunctionComponent,
        elementTypeName: null,
        functionRender: renderFunction,
        textContent: null,
        key: key,
        typedProps: props ?? EmptyProps.Instance,   // ← new field, no dict
        children: children ?? EmptyChildren()
    );
}
```

#### 6b. Deprecate the old overload (do NOT delete yet)

```csharp
[Obsolete("Prefer V.Func<TProps> with a typed IProps record. " +
          "The dictionary-based overload will be removed in a future release.")]
public static VirtualNode Func(
    Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
    IReadOnlyDictionary<string, object> functionProps = null,
    ...
)
```

Add an adapter so old callers still compile during migration:

```csharp
// Inside the old Func overload body:
// Wrap the dict-based delegate to satisfy the new IProps-based FunctionRender type.
Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> adapter =
    (rawProps, ch) =>
    {
        var dict = rawProps is DictProps dp ? dp.Inner : null;
        return renderFunction(dict, ch);
    };
```

Introduce a thin wrapper:
```csharp
// Internal adapter — do not expose publicly.
internal sealed class DictProps : IProps
{
    public readonly IReadOnlyDictionary<string, object> Inner;
    public DictProps(IReadOnlyDictionary<string, object> d) => Inner = d;
    public override bool Equals(object obj)
        => obj is DictProps other && DictionaryEquals(Inner, other.Inner);
    // reuse existing ArePropsEqual logic here
}
```

This adapter means **zero breakage** for all existing callers of `V.Func` with a dictionary during migration.

---

### Phase 7 — Built-in Props Types (`Shared/Props/Typed/*.cs`)

Every existing class in `ReactiveUITK.Props.Typed` that is passed to a built-in element factory method needs to add `: IProps`.

The set includes (exhaustive list must be confirmed by directory scan):
`ButtonProps`, `LabelProps`, `TextFieldProps`, `ToggleProps`, `SliderProps`, `ScrollViewProps`, `VisualElementProps`, `ImageProps`, `ProgressBarProps`, `RadioButtonProps`, `EnumFieldProps`, `TabProps`, `TabViewProps`, `ListViewProps`, `TreeViewProps`, `DropdownFieldProps`, `MinMaxSliderProps`, `BoundsFieldProps`, `Vector2FieldProps`, `Vector3FieldProps`, `Vector4FieldProps`, `ColorFieldProps`, `ObjectFieldProps`, `CurveFieldProps`, `GradientFieldProps`, `MaskFieldProps`, `LayerFieldProps`, `LayerMaskFieldProps`, `TagFieldProps`, `RectFieldProps`, `IntegerFieldProps`, `LongFieldProps`, `FloatFieldProps`, `DoubleFieldProps`, `UnsignedIntegerFieldProps`, `UnsignedLongFieldProps`.

All of these already have a `ToDictionary()` method used by `V.Button`, `V.Label`, etc. For now, that bridge stays — Phase 7 only adds the `IProps` annotation.

**Important:** `PropsResolver.GetPublicPropertyNames` already resolves types from `ReactiveUITK.Props.Typed` via Roslyn — no change needed there.

---

### Phase 8 — Source Generator (`SourceGenerator~/`)

This is the most complex phase. Changes span two files.

#### 8a. `CSharpEmitter.cs` — `Render` method signature

**Before (emitted):**
```csharp
public static VirtualNode Render(
    Dictionary<string, object> __rawProps,
    IReadOnlyList<VirtualNode> __children)
{
    var props = __rawProps != null
        && __rawProps.TryGetValue("__typed", out var __tp)
        && __tp is MyProps __p
        ? __p : new MyProps();
```

**After (emitted):**
```csharp
public static VirtualNode Render(
    global::ReactiveUITK.Core.IProps __rawProps,
    IReadOnlyList<VirtualNode> __children)
{
    var props = (__rawProps as MyProps) ?? new MyProps();
```

Remove the `"__typed"` sentinel key path entirely — it was a workaround for the old signature.

**Change in `BuildSource()`:**
```csharp
// Before
L($"{I2}    Dictionary<string, object> __rawProps,");

// After
L($"{I2}    global::ReactiveUITK.Core.IProps __rawProps,");
```

**Change in props binding:**
```csharp
// Before (with @props directive)
L($"{I3}var props = __rawProps != null");
L($"{I3}    && __rawProps.TryGetValue(\"__typed\", out var __tp)");
L($"{I3}    && __tp is {propsType} __p");
L($"{I3}    ? __p : new {propsType}();");

// After
L($"{I3}var props = (__rawProps as {propsType}) ?? new {propsType}();");
```

#### 8b. `CSharpEmitter.cs` — `EmitFuncComponent`

Currently emits `V.Func(TypeName.Render, new Dictionary<string,object>{ ... })`.

With typed props available, the generator should inspect (via `PropsResolver`) whether the target component has a companion `{TypeName}Props` class. If yes, emit the typed overload; if no, fall back to dict.

**Case A — typed props exist (new behavior):**

```csharp
// Emitted:
V.Func(TypeName.Render, new TypeNameProps { Attr1 = val1, Attr2 = val2 }, key: keyExpr, ...)
```

**Case B — no typed props (backward-compat, existing behavior):**

```csharp
// Emitted (unchanged):
V.Func(TypeName.Render, new Dictionary<string,object> { {"attr1", val1} }, key: keyExpr, ...)
```

The attribute-name-to-property-name mapping in Case A uses the same `ToPropName` helper already used for `EmitBuiltinTyped` (kebab-case → PascalCase).

#### 8c. `PropsResolver.cs` — Typed-props detection for PascalCase tags

Add a method:
```csharp
/// <summary>
/// Returns the props type name for a function component if one exists,
/// e.g. "MyButtonProps" for the component "MyButton".
/// Returns null if no companion props class is found.
/// </summary>
public string? TryGetFuncComponentPropsTypeName(
    string componentTypeName,
    ImmutableArray<string> usingNamespaces)
{
    // Convention: props class is {ComponentName}Props
    string candidate = $"{componentTypeName}Props";
    foreach (var ns in usingNamespaces.Prepend(""))
    {
        string fqn = string.IsNullOrEmpty(ns) ? candidate : $"{ns}.{candidate}";
        var sym = _compilation.GetTypeByMetadataName(fqn);
        if (sym != null) return sym.Name; // return simple name for the emitter
    }
    return null;
}
```

Update `Resolve()` to populate `TagResolution.FuncPropsTypeName` with the result of this lookup.

#### 8d. `EmitFuncComponent` — full replacement

```csharp
private void EmitFuncComponent(
    TagResolution res,
    ImmutableArray<AttributeNode> attrs,
    string keyExpr,
    ImmutableArray<AstNode> children)
{
    string typeName = res.FuncTypeName!;
    string? propsTypeName = res.FuncPropsTypeName; // null if not resolved

    bool hasAttrs = attrs.Any(a => !IsKey(a.Name));

    if (propsTypeName != null)
    {
        // ── Typed path ────────────────────────────────────────────────────
        _sb.Append($"V.Func({typeName}.Render, new {propsTypeName}");
        _sb.Append(" {");
        bool first = true;
        foreach (var attr in attrs)
        {
            if (IsKey(attr.Name)) continue;
            if (!first) _sb.Append(", ");
            first = false;
            _sb.Append($" {ToPropName(attr.Name)} = {AttrVal(attr.Value)}");
        }
        _sb.Append(" }");
    }
    else
    {
        // ── Dict fallback (backward compat / unresolved type) ─────────────
        if (hasAttrs)
        {
            _sb.Append($"V.Func({typeName}.Render, new Dictionary<string, object>");
            _sb.Append(" {");
            bool first = true;
            foreach (var attr in attrs)
            {
                if (IsKey(attr.Name)) continue;
                if (!first) _sb.Append(", ");
                first = false;
                _sb.Append($" {{ \"{attr.Name}\", {AttrVal(attr.Value)} }}");
            }
            _sb.Append(" }");
        }
        else
        {
            _sb.Append($"V.Func({typeName}.Render, (Dictionary<string, object>)null");
        }
    }

    _sb.Append($", key: {keyExpr}");
    if (!children.IsEmpty)
    {
        _sb.Append(", __C(");
        EmitChildArgs(children);
        _sb.Append(")");
    }
    _sb.Append(")");
}
```

#### 8e. `TagResolution` record — add `FuncPropsTypeName`

```csharp
// In PropsResolver.cs
public record TagResolution(
    TagResolutionKind Kind,
    string MethodName,
    string? PropsTypeName,
    bool AcceptsChildren,
    string? FuncTypeName = null,
    string? FuncPropsTypeName = null   // ← new
);
```

---

### Phase 9 — Migrate Hand-Written Components (Samples + Game Project)

Each component that currently has a `Render(Dictionary<string,object> rawProps, ...)` method must be migrated.

**Pattern:**

1. Create `MyComponentProps.cs` (or use the `.uitkx`'s `@props` directive to auto-generate it):
   ```csharp
   public sealed class MyComponentProps : IProps
   {
       public string Name    { get; init; }
       public Color  Color   { get; init; } = Color.white;
       public bool   Visible { get; init; } = true;
   }
   ```

2. Update `Render`:
   ```csharp
   // Before
   public static VirtualNode Render(Dictionary<string,object> rawProps, ...)
   {
       var name  = rawProps?["name"] as string ?? "";
       var color = ...;
   
   // After
   public static VirtualNode Render(IProps rawProps, ...)
   {
       var props  = (MyComponentProps)rawProps;
       var name   = props.Name;
       var color  = props.Color;
   ```

3. Update all call sites that use `new Dictionary<string,object>{ ... }` to `new MyComponentProps{ ... }`.

For components driven by `.uitkx` (the majority), steps 2 and 3 are handled automatically by the updated generator once Phase 8 is complete — no manual work required.

---

## 5. Backward Compatibility Strategy

The migration is designed to be **additive and non-breaking** at each phase boundary:

| Phase | Old callers still compile? | Notes |
|-------|--------------------------|-------|
| 0 — `IProps.cs` created | ✅ | Purely additive |
| 1 — `VirtualNode` changed | ⚠️ | `Properties` field renamed; provide `[Obsolete]` alias or keep both for one cycle |
| 2 — `FiberNode` changed | ✅ | Internal only — no public API |
| 3–5 — Factory/Reconciler/FiberFuncComp changed | ✅ | Internal only |
| 6 — `V.Func` overloads | ✅ | Old overload kept; `DictProps` adapter bridges old render delegates |
| 7 — Built-in `*Props` add `: IProps` | ✅ | Purely additive; no breaking changes |
| 8 — Generator emits typed path | ✅ | Dict fallback kept for unresolved types |
| 9 — Migrate hand-written Render methods | ✅ per component | Each component migrated independently; old dict overload of `V.Func` keeps them working |

**Removal timeline:**
- After all Samples are migrated: mark `V.Func(dict)` `[Obsolete]`.
- After game project is migrated: delete `DictProps`, `V.Func(dict)`, old `VirtualNode.Properties` alias.

---

## 6. Props Authoring Conventions

Once the refactor is complete, components should follow this pattern:

### `.uitkx` style (recommended — generator handles everything)

```uitkx
@namespace MyGame.UI
@component MyButton
@props MyButtonProps

<button text=@props.Label on-click=@props.OnClick />
```

The generator will:
- Emit `Render(IProps __rawProps, ...)` with `var props = (__rawProps as MyButtonProps) ?? new MyButtonProps();`
- At call sites, emit `V.Func(MyButton.Render, new MyButtonProps { Label = ..., OnClick = ... })`

### Hand-written style

```csharp
public sealed class MyButtonProps : IProps
{
    public string          Label    { get; init; } = "";
    public Action          OnClick  { get; init; }

    // Equality — required for correct bailout behavior.
    // Use a C# record (in netstandard 2.1+) to get this for free,
    // or implement manually:
    public override bool Equals(object obj)
        => obj is MyButtonProps o
           && Label   == o.Label
           && OnClick == o.OnClick;

    public override int GetHashCode()
        => HashCode.Combine(Label, OnClick);
}

public partial class MyButton
{
    public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
    {
        var props = (MyButtonProps)rawProps;
        // ...
    }
}
```

### Components with no props

```csharp
public partial class MySpinner
{
    // No companion props class needed.
    public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
    {
        // rawProps will be EmptyProps.Instance — ignore it.
        ...
    }
}
```

At the call site:
```csharp
V.Func(MySpinner.Render, EmptyProps.Instance, key: "spinner")
// or via .uitkx:
// <MySpinner />
// generator emits: V.Func(MySpinner.Render, new MySpinnerProps {})
// (with an empty props class), or V.Func(MySpinner.Render, EmptyProps.Instance)
// when no @props directive is present
```

---

## 7. Known Limitations and Edge Cases

### 7.1 `useContext<T>()` sugar still broken

`ApplyHookAliases` in `CSharpEmitter.cs` is a string-replace on `"useContext("`. Using `useContext<Color>(...)` in `@code` blocks silently fails because `<Color>` sits between `useContext` and `(`. This is **unrelated to the IProps refactor** but should be fixed at the same time:

```csharp
// Current — misses generic form
("useContext(", "Hooks.UseContext("),

// Fix — regex replace (requires minimal refactor of ApplyHookAliases to use Regex):
@"useContext\s*(<[^>]+>)?\s*\("  →  "Hooks.UseContext$1("
```

### 7.2 `memoize` / `memoCompare` parameters on `V.Func`

The new typed `V.Func<TProps>` overload should carry over `memoize` and `memoCompare` parameters from the existing overload. With records providing structural equality, `memoCompare` becomes unnecessary in most cases — but preserving it maintains API parity.

### 7.3 `ForwardRef` and `V.ForwardRef`

`V.ForwardRef` uses the same delegate pattern as `V.Func` with an extra `object ref` parameter. It must be updated in the same pass as Phase 6.

```csharp
// Before
Func<Dictionary<string,object>, object, IReadOnlyList<VirtualNode>, VirtualNode>

// After
Func<IProps, object, IReadOnlyList<VirtualNode>, VirtualNode>
```

### 7.4 IL2CPP and `is`/`as` casts

IL2CPP handles interface casts efficiently for reference types. The `(MyButtonProps)rawProps` cast compiles to a single `isinst`/`castclass` pair. No code-generation issues expected.

### 7.5 Unity serialization

Props **must not** be serialized to the Unity Inspector or JSON. They are ephemeral, per-render objects. Ensure no `[Serializable]` attribute is ever added to an `IProps` implementing class.

---

## 8. Test Plan

After each phase, verify:

| Test | How |
|------|-----|
| Existing Samples compile with zero errors | Run `Build UITKX Extension` task; open Unity and let it recompile |
| Running Samples produce identical visual output | Manual test in Play mode |
| Bailout still works | Check `ContextBailoutDemoFunc` sample — middle layer must not re-render when root state changes but context is unchanged |
| `ArePropsEqual` returns `true` for equal records | Unit test: create two `MyProps { Name = "a" }` instances, verify `ArePropsEqual` returns `true` |
| `ArePropsEqual` returns `false` for `Color` field change | Unit test with `Color.red` vs `Color.blue` |
| No per-frame allocations on idle UI | Unity Profiler → GC Alloc: 0 during steady-state (no state changes) |
| Generator emits typed path for resolved types | Inspect `GeneratedPreview~/Test.uitkx.g.cs` after a `.uitkx` change |
| Generator falls back to dict for unresolved types | Create a `.uitkx` that references an unknown component; confirm dict path is emitted |

---

## 9. Migration Order

Execute phases in this order to maintain a working build throughout:

```
Phase 0  →  Phase 7  →  Phase 1  →  Phase 2  →  Phase 3  →  Phase 4
(IProps)    (tag +=IProps) (VNode) (FiberNode) (Factory) (Reconciler)

    →  Phase 5  →  Phase 6  →  Phase 8 (generator)  →  Phase 9 (samples)
   (FiberFuncComp) (V.Func)
```

> Phase 7 (add `: IProps` to existing prop classes) comes before Phase 1 so that `ButtonProps : IProps` is true before `VirtualNode.TypedProps` starts referencing it — avoiding a transient "ButtonProps does not implement IProps" error.

---

## 10. Files NOT Changing

The following files are explicitly **out of scope** and require no modification:

- `Shared/Core/Fiber/FiberChildReconciliation.cs` — reconciles by key/index, no direct dict access
- `Shared/Core/Fiber/FiberScheduler.cs` / `RenderScheduler.cs` — scheduling is prop-type-agnostic
- `Shared/Core/Hooks/*.cs` — `UseState`, `UseEffect`, `UseContext`, `UseRef` work on internal state, not props
- `Shared/Core/Context/*.cs` — context values are independent of the props type system
- `Editor/` — editor integration layer, reads VirtualNode type/key but not props
- All `.uitkx` template files — templates are re-generated by the updated generator; authoring syntax is unchanged
- All `.asmdef` files — no assembly-structure changes

---

*Document last updated: 2025 — branch `refactory_types_in_core_library`*
