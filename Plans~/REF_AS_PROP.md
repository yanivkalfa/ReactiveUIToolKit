# Ref-as-Prop — Implementation Plan

> **React 19 model applied to ReactiveUITK.**
> `V.ForwardRef` is removed entirely. `ref` becomes a plain component prop.
> No deprecation warnings, no compatibility shim — hard removal.

---

## 1. Motivation & Design Philosophy

In React 18 and below, `forwardRef` was a wrapper that extracted the `ref` out of the
special-cased "implicit props" mechanism and passed it as a second argument to the render
function. React 19 removed it entirely — `ref` is just a prop. The component author
declares it, the consumer passes it. No ceremony.

ReactiveUITK adopts the same model:

- `V.ForwardRef` and `V.ForwardRef<TProps>` are **deleted** from `V.cs`.
- The runtime infrastructure that already exists (`Hooks.MutableRef<T>`, `RefUtility`,
  `BaseProps.Ref`, `PropsApplier` ref handling) stays completely unchanged —
  it is not ForwardRef-specific.
- A component that needs to expose a ref simply declares a param of type
  `Hooks.MutableRef<T>?`; the consumer passes it like any other prop.

---

## 2. The C# Keyword Problem & Naming Convention

In React 19 (TypeScript), the component parameter can literally be named `ref` because
TypeScript allows it. In C#, `ref` is a reserved keyword and cannot be used as:

- A local variable name: `var ref = props.Ref;` → **compiler error**
- A property name: `public MutableRef<T> ref { get; set; }` → **compiler error**
- An expression in markup: `<TextField ref={ref} />` → **compiler error**

### Chosen approach: free-named param + `ref={}` consumer sugar (Phase 3)

**Component author** names their ref param using any valid C# identifier.
Convention (and the doc example) is `componentRef`:

```uitkx
component FancyInput(Hooks.MutableRef<TextField>? componentRef = null, string label = "") {
    return <TextField ref={componentRef} label={label} />;
}
```

The generated Props class has `public Hooks.MutableRef<TextField>? ComponentRef { get; set; }`,
which is clean C#. The Render body local variable is `componentRef`, also clean C#.

**Consumer** uses `ref={x}` as shorthand — no need to know the internal param name:

```uitkx
var inputRef = useRef<TextField?>(null);
<FancyInput ref={inputRef} label="Email" />
```

The emitter routes `ref={x}` on a user-component tag to the component's designated
ref-param property (see Phase 3 for how the emitter discovers which param that is).

**Until Phase 3 lands**, consumers use the raw prop name:
```uitkx
<FancyInput componentRef={inputRef} label="Email" />
```
This already works today — zero generator changes needed for Phase 2.

---

## 3. Layers Affected

| Layer | File(s) | Change |
|---|---|---|
| **Runtime** | `Shared/Core/V.cs` | Delete `V.ForwardRef` and `V.ForwardRef<TProps>` |
| **Demo (C#)** | `Samples/Components/RefForwardingDemoFunc/RefForwardingDemoFunc.cs` | Rewrite: no 3-arg render, ref passed as normal prop |
| **Demo (UITKX)** | `Samples/UITKX/Components/RefForwardingDemoFunc/RefForwardingDemoFunc.uitkx` | Update cross-component sub-component use |
| **Generator — emitter** | `SourceGenerator~/Emitter/CSharpEmitter.cs` | Phase 3: route `ref={x}` → ref-param prop |
| **Generator — resolver** | `SourceGenerator~/Emitter/PropsResolver.cs` | Phase 3: expose ref-param prop name per component |
| **Language lib — parser** | `ide-extensions~/language-lib/Parser/DirectiveParser.cs` | Phase 3 (optional): validate only one ref-type param per component |
| **Tests** | `SourceGenerator~/Tests/EmitterTests.cs` | Phase 3: new test cases |

---

## 4. Phase 1 — Delete `V.ForwardRef` from the Runtime

### 4.1 `Shared/Core/V.cs`

Delete both overloads (lines ~1140–1218 in current source):

```csharp
// DELETE — V.ForwardRef<TProps>(...)
public static VirtualNode ForwardRef<TProps>(
    Func<Core.IProps, object, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
    ...

// DELETE — V.ForwardRef(...)
public static VirtualNode ForwardRef(
    Func<Core.IProps, object, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
    ...
```

Also delete their XML doc comment blocks that immediately precede them.

**Nothing else in the runtime changes.** The `Fragment` method that follows is untouched.

### 4.2 What does NOT change

| Symbol | Why it stays |
|---|---|
| `Hooks.MutableRef<T>` | How you *create* a ref container — orthogonal to forwarding |
| `Hooks.UseRef<T>()` | Hook that allocates the MutableRef — orthogonal |
| `RefUtility.Assign` | How the runtime assigns a resolved VisualElement to a ref |
| `NodeMetadata.AttachedRef` | How built-in elements store their ref at runtime |
| `BaseProps.Ref` | How built-in elements accept `ref=` from the typed-props side |
| `PropsApplier` ref handling | The "ref" property path in Apply/ApplyDiff — untouched |

---

## 5. Phase 2 — Rewrite `RefForwardingDemoFunc.cs`

The existing C# demo uses `V.ForwardRef`, a private nested `ForwardedChild` class with a
3-arg `RenderWithForwardedRef(IProps, object, IReadOnlyList<VirtualNode>)` signature, and
`forwardedRef` being an untyped `object` that is cast at runtime.

### 5.1 New C# demo design

Replace with two normal components. The parent holds both refs. The "child" component
accepts the `TextField` ref as a plain typed prop named `InputRef`.

```csharp
// Child component — ref is just a typed prop
private static class RefChild
{
    public sealed class Props : IProps
    {
        public Hooks.MutableRef<TextField> InputRef { get; set; }
        public Hooks.MutableRef<Label>     LabelRef { get; set; }
        public Action                      OnSnapshot { get; set; }

        public override bool Equals(object obj) { ... }
        public override int GetHashCode() { ... }
    }

    // 2-arg Render — exactly like every other component
    public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> _)
    {
        var p = rawProps as Props ?? new Props();
        string currentValue = p.InputRef?.Value?.value ?? "Hello ReactiveUITK refs!";
        return V.VisualElement(
            ...,
            V.Label(new LabelProps { Text = $"Child sees: {currentValue}", Ref = p.LabelRef }, "child-label"),
            V.TextField(new TextFieldProps { Value = currentValue, Ref = p.InputRef }, "child-input"),
            V.Button(new ButtonProps
            {
                Text = "Focus (child)",
                OnClick = () => { p.InputRef?.Value?.Focus(); p.OnSnapshot?.Invoke(); }
            }, "child-focus")
        );
    }
}
```

Parent calls:
```csharp
VirtualNode child = V.Func<RefChild.Props>(
    RefChild.Render,
    new RefChild.Props { InputRef = inputRef, LabelRef = labelRef, OnSnapshot = UpdateSnapshot },
    key: "ref-child"
);
```

### 5.2 Updated UITKX demo

The existing `RefForwardingDemoFunc.uitkx` currently demonstrates `useRef` within a single
component only. Rewrite it to also show the cross-component pattern using a sub-component
file `RefChild/RefChild.uitkx`:

```uitkx
// RefChild.uitkx
component RefChild(
    Hooks.MutableRef<UnityEngine.UIElements.TextField>? inputRef = null,
    Hooks.MutableRef<UnityEngine.UIElements.Label>?    labelRef  = null,
    Action? onSnapshot = null
) {
    var currentValue = inputRef?.Value?.value ?? "Hello ReactiveUITK refs!";
    return (
        <VisualElement>
            <Label text={$"Child sees: {currentValue}"} ref={labelRef} />
            <TextField value={currentValue} ref={inputRef} />
            <Button text="Focus (child)" onClick={() => { inputRef?.Value?.Focus(); onSnapshot?.Invoke(); }} />
        </VisualElement>
    );
}
```

```uitkx
// RefForwardingDemoFunc.uitkx — parent
component RefForwardingDemoFunc {
    var inputRef = useRef<UnityEngine.UIElements.TextField?>(null);
    var labelRef  = useRef<UnityEngine.UIElements.Label?>(null);
    var (snapshot, setSnapshot) = useState("Click 'Read refs' to inspect.");

    void UpdateSnapshot() { ... }

    return (
        <VisualElement>
            <Label text="useRef + ref-as-prop demo" />
            <Button text="Read refs" onClick={UpdateSnapshot} />
            <Button text="Focus input" onClick={() => inputRef?.Value?.Focus()} />
            <RefChild inputRef={inputRef} labelRef={labelRef} onSnapshot={UpdateSnapshot} />
            <Label text={snapshot} />
        </VisualElement>
    );
}
```

Note: The consumer uses the raw prop names (`inputRef=`, `labelRef=`) until Phase 3's
`ref={x}` sugar lands. This is still vastly cleaner than the old `V.ForwardRef` ceremony.

---

## 6. Phase 3 — Generator Support for `ref={x}` Consumer Sugar

> Pre-requisite: Phases 1 + 2 complete.

### 6.1 Goal

Allow the consumer to write `ref={x}` on any user-component tag that has exactly one
`Hooks.MutableRef<T>?` parameter declared. The emitter routes it to that component's
Props property without the consumer knowing the internal param name.

```uitkx
// Author declares — param named anything
component FancyInput(Hooks.MutableRef<TextField>? componentRef = null) { ... }

// Consumer writes — always 'ref=', regardless of author's param name
<FancyInput ref={inputRef} />
```

Emits:
```csharp
V.Func<FancyInputProps>(FancyInput.Render, new FancyInputProps { ComponentRef = inputRef }, key: null)
```

### 6.2 Discovering the ref param name

The emitter needs to know, for each user-component tag, which (if any) Props property is
the "ref param". Two sources of truth:

**A. Peer UITKX components** (most common case):
When the emitter processes `<FancyInput ref={x} />` and `FancyInput` is a peer `.uitkx`
component, the `FunctionParams` from its `DirectiveSet` are already in memory.
The generator can scan them for the first param whose type matches the MutableRef pattern.

**B. External C# components** (less common):
When the component is a C# class (not UITKX), the emitter uses Roslyn to inspect the
Props class for a property whose type is `Hooks.MutableRef<T>` (generic check via
`INamedTypeSymbol.ConstructedFrom`).

### 6.3 MutableRef type detection pattern

A `FunctionParam.Type` string is a MutableRef param if:
```
typeName starts with "Hooks.MutableRef<" OR
typeName starts with "ReactiveUITK.Core.Hooks.MutableRef<"
```
(after stripping trailing `?`).

For Roslyn symbols: check `type.OriginalDefinition.ToDisplayString() == "ReactiveUITK.Core.Hooks.MutableRef<T>"`.

### 6.4 Changes needed in the generator

#### `PropsResolver.cs`
Add a method `TryGetRefParamPropName(string componentTypeName) → string?` that:
1. Checks `_peerPropsComponentTypeNames` set — if component is a peer UITKX component,
   reconstructs its param list from `DirectiveSet` and scans for a MutableRef-typed param.
   Returns `ToPropName(param.Name)`.
2. Otherwise checks the Roslyn compilation for `ComponentNameProps.{PropName}` where the
   property type is `Hooks.MutableRef<Any>`.
3. Returns `null` if no ref param found (then `ref={}` on that tag is a diagnostic error).

To make step 1 work, the generator pass needs to share peer `DirectiveSet` data (not just
the component names) across the multi-file compilation. The existing
`peerPropsComponentTypeNames` `ImmutableHashSet<string>` pipeline would be extended with a
`ImmutableDictionary<string, DirectiveSet> peerDirectiveSets` parameter.

#### `CSharpEmitter.cs`
In `EmitFuncComponent`, before iterating attributes:
- Check if `ref` attribute is present in `attrs`.
- If yes: call `_resolver.TryGetRefParamPropName(res.FuncTypeName)`.
  - Found → emit `{RefParamPropName} = {AttrVal(refAttr.Value)}` in the Props initializer,
    skip the `ref` attribute in the normal loop.
  - Not found → emit UITKX0015 diagnostic: `ref={...} used on '{TagName}' which has no
    Hooks.MutableRef<T>? parameter.`
- `IsKey` already skips `key`. Add a parallel `IsRef(string name)` guard for the loop.

#### `UitkxGenerator.cs`
Extend the pre-scan loop (lines ~149-158) to also collect a
`ImmutableDictionary<string, DirectiveSet>` (keyed by component name) from peer `.uitkx`
files, to feed into `UitkxPipeline.Run` and then down to `PropsResolver`.

### 6.5 New diagnostic

| Code | Severity | Condition |
|---|---|---|
| `UITKX0015` | Error | `ref={...}` on a component tag that has no `Hooks.MutableRef<T>?` param |

### 6.6 Consumer side: multiple ref params

If a component declares **multiple** `Hooks.MutableRef<T>?` params, `ref={}` is ambiguous.
In this case, `ref={}` is not allowed — the consumer must use explicit prop names.
Diagnostic UITKX0016: `ref={...} is ambiguous: '{TagName}' declares {N} MutableRef parameters. Use explicit prop names instead.`

---

## 7. Test Plan

### Phase 1 tests
- `V_ForwardRef_DoesNotExist` — verify the method is gone (compilation test / reflection test)
- Existing ForwardRef demo tests updated to use ref-as-prop pattern

### Phase 2 tests
- `RefChild_ReceivesTypedRef_AndAssignsToElement` — functional test that the ref is
  populated when the child component is rendered with the ref prop set

### Phase 3 tests

**Emitter tests (in `EmitterTests.cs`)**:
```
UserComponent_WithRefAttr_RoutesToMutableRefParam
// <FancyInput ref={inputRef} /> where FancyInputProps has MutableRef<TextField>? ComponentRef
// Expected: V.Func<FancyInputProps>(..., new FancyInputProps { ComponentRef = inputRef })

UserComponent_NoRefParam_RefAttrEmitsDiagnostic
// <Label ref={x} /> (user component, not built-in) — UITKX0015 expected

UserComponent_MultipleRefParams_RefAttrEmitsDiagnostic
// Component with two MutableRef params — UITKX0016 expected

UserComponent_RefAttrAndOtherProps_CombineCorrectly
// <FancyInput ref={inputRef} label="Email" /> → ComponentRef = inputRef, Label = "Email"
```

---

## 8. Comparison: Old vs New

### Old (V.ForwardRef — removed)
```csharp
// Consumer — C#
VirtualNode child = V.ForwardRef<ForwardedChild.Props>(
    ForwardedChild.RenderWithForwardedRef,
    new ForwardedChild.Props { LabelRef = labelRef },
    forwardedRef: inputRef,
    key: "child"
);

// Component — must use untyped 3-arg signature
public static VirtualNode RenderWithForwardedRef(
    IProps rawProps, object forwardedRef, IReadOnlyList<VirtualNode> _)
{
    var typedRef = forwardedRef as Hooks.MutableRef<TextField>; // runtime cast, unsafe
    ...
}
```

### New (ref-as-prop — Phase 2, C# style)
```csharp
// Consumer — C#
VirtualNode child = V.Func<RefChild.Props>(
    RefChild.Render,
    new RefChild.Props { InputRef = inputRef, LabelRef = labelRef },
    key: "child"
);

// Component — standard 2-arg Render, typed Props
public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> _)
{
    var p = rawProps as Props ?? new Props();
    // p.InputRef is already typed as Hooks.MutableRef<TextField> — no cast needed
    ...
}
```

### New (ref-as-prop — Phase 3, UITKX style)
```uitkx
// Component definition
component FancyInput(Hooks.MutableRef<TextField>? componentRef = null, string label = "") {
    return <TextField ref={componentRef} label={label} />;
}

// Consumer — ref= sugar
<FancyInput ref={inputRef} label="Email" />
```

---

## 9. File Checklist

### Phase 1 (delete V.ForwardRef)
- [ ] `Shared/Core/V.cs` — delete `ForwardRef<TProps>` + `ForwardRef` + their XML doc comments

### Phase 2 (rewrite demos)
- [ ] `Samples/Components/RefForwardingDemoFunc/RefForwardingDemoFunc.cs` — full rewrite
- [ ] Create `Samples/UITKX/Components/RefForwardingDemoFunc/components/RefChild/RefChild.uitkx`
- [ ] Update `Samples/UITKX/Components/RefForwardingDemoFunc/RefForwardingDemoFunc.uitkx`

### Phase 3 (generator changes)
- [ ] `ide-extensions~/language-lib/Parser/ParseResult.cs` — no change needed (FunctionParam already sufficient)
- [ ] `SourceGenerator~/UitkxGenerator.cs` — collect peer DirectiveSets in pre-scan loop
- [ ] `SourceGenerator~/UitkxPipeline.cs` — thread `peerDirectiveSets` parameter through
- [ ] `SourceGenerator~/Emitter/PropsResolver.cs` — add `TryGetRefParamPropName`
- [ ] `SourceGenerator~/Emitter/CSharpEmitter.cs` — route `ref=` in EmitFuncComponent
- [ ] `SourceGenerator~/Tests/EmitterTests.cs` — four new tests
- [ ] Update `Plans~/UITKX_IMPLEMENTATION_PLAN.md` P11 status → IN PROGRESS → DONE

---

## 10. Sequence

```
Phase 1: V.ForwardRef deletion  ← do first, everything else builds on clean baseline
    ↓
Phase 2: Demo rewrite           ← validate the approach works end-to-end with plain props
    ↓
Phase 3: ref= sugar in UITKX    ← generator work, adds consumer ergonomics
```

Phase 1 is self-contained and can be merged independently.
Phase 2 depends on Phase 1 (no ForwardRef to call).
Phase 3 depends on Phase 2 (demonstrates the pattern the generator codifies).
