# UITKX Migration Guide

This guide shows how to convert a hand-written C# Func component into a `.uitkx`
file and how to integrate typed props with reactive data sources.

---

## Why migrate?

| Hand-written C# | `.uitkx` |
|---|---|
| Verbose nested `V.Box(...)` calls | Readable XML-like markup |
| No IDE markup highlighting | Full syntax colouring + semantic tokens |
| No element-level completions | IntelliSense for element names and attributes |
| Diagnostics only at Roslyn compile time | Structural diagnostics before the C# build |
| Breakpoints land in generated glue code | `#line` directives map breakpoints to `.uitkx` |

---

## Quick-start: 5-minute migration

### Before — hand-written C# Func component

```csharp
// SimpleCounterFunc.cs
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SimpleCounterFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children)
        {
            var (count, setCount) = Hooks.UseState(0);
            return V.VisualElement(
                null,
                null,
                V.Text($"Count: {count}"),
                V.Button(new ButtonProps { Text = "+", OnClick = () => setCount(count + 1) })
            );
        }
    }
}
```

### After — `.uitkx` file

```uitkx
component SimpleCounter {
    var (count, setCount) = Hooks.UseState(0);

    return (
        <VisualElement>
            <Text text={$"Count: {count}"}/>
            <Button text="+" onClick={() => setCount(count + 1)}/>
        </VisualElement>
    );
}
```

### After — companion `.cs` file (minimal)

```csharp
// SimpleCounter.cs — generated partial class is wired here
namespace ReactiveUITK.Samples.FunctionalComponents
{
    public partial class SimpleCounter { }
}
```

The source generator emits the `Render()` method into `SimpleCounter` automatically.
You call it the same way:

```csharp
V.Func(SimpleCounter.Render, props)
```

---

## Step-by-step migration

### Step 1 — Identify the component

Find the `public static VirtualNode Render(...)` method you want to migrate. Confirm:
- It lives in a known namespace.
- The class name is the component name you want to keep (or pick a new one).

### Step 2 — Create the `.uitkx` file

Create `<ComponentName>.uitkx` next to the existing `.cs` file.

Use function-style component syntax:

```uitkx
component <ClassName> {
    // setup C#
    return (
        <VisualElement />
    );
}
```

Namespace is taken from the companion partial `.cs` file.
`@namespace` / `@component` directives are only needed for legacy directive-header files.

### Step 3 — Move hook calls to function setup section

Everything that was inside `Render()` before the `return` statement goes into
the setup section before `return (...)`:

```uitkx
component <ClassName> {
    var (count, setCount) = Hooks.UseState(0);
    var (mode,  setMode)  = Hooks.UseState("normal");

    return (
        <VisualElement />
    );
}
```

All hook calls, local variable declarations, and helper lambdas belong here.

### Step 4 — Translate the return value to markup

Replace every `V.XXX(new XXXProps { ... }, ...)` call with the equivalent tag.

| C# | UITKX |
|---|---|
| `V.Box(new BoxProps { }, ...)` | `<Box> ... </Box>` |
| `V.Label(new LabelProps { Text = "Hi" })` | `<Label text="Hi"/>` |
| `V.Button(new ButtonProps { Text = "+", OnClick = cb })` | `<Button text="+" onClick={cb}/>` |
| `V.Text("hello")` | `<Text text="hello"/>` |
| `V.Func(MyComp.Render, props)` | `<MyComp data={props}/>` (see typed props section) |

String literals can be unquoted plain values. C# expressions are wrapped in `{ }`:

```uitkx
<Label text="static text"/>
<Label text={$"dynamic: {count}"}/>
```

### Step 5 — Replace the old `.cs` file with a companion stub

Delete (or rename to `Legacy/`) the old static class.
Replace it with an empty `partial` class:

```csharp
namespace <your.namespace>
{
    public partial class <ClassName> { }
}
```

Unity will regenerate the `Render()` method from the `.uitkx` source on the next
compile.

### Step 6 — Verify

1. Save both files.
2. Unity recompiles — no errors should appear.
3. The component renders identically to the original.

---

## Directive reference

### `@if` / `@else if` / `@else`

```uitkx
@if (health <= 0) {
    <Label text="Game Over"/>
} @else if (health < 20) {
    <Label text="Low health!"/>
} @else {
    <Label text={$"HP: {health}"}/>
}
```

All branches are always wrapped in a `V.Fragment` by the emitter, keeping the
type at each child position stable across re-renders.

### `@switch` / `@case` / `@default`

```uitkx
@switch (mode) {
    @case "fireball":
        <Label text="Fireball active"/>
    @case "shield":
        <Label text="Shield active"/>
    @default:
        <Label text="No ability"/>
}
```

### `@foreach`

```uitkx
@foreach (var item in inventory) {
    <Box key={item.Id}>
        <Label text={item.Name}/>
    </Box>
}
```

> **Always supply a `key` attribute** on the direct children of `@foreach`.
> The reconciler uses it to match items across list changes. Omitting it
> triggers diagnostic `UITKX0009`.

### `@for`

```uitkx
@for (int i = 0; i < count; i++) {
    @if (i % 2 == 0) {
        <Label text={$"Even: {i}"}/>
    } @else {
        <Label text={$"Odd: {i}"}/>
    }
}
```

---

## Typed props + `PropsHelper.Bind`

For components that receive external data (not just internal hooks state), declare
a props class and pass it under the `"data"` key.

### Props class

```csharp
// PlayerHUDProps.cs
public sealed class PlayerHUDProps
{
    public int    Health        { get; set; } = 100;
    public int    MaxHealth     { get; set; } = 100;
    public string ActiveAbility { get; set; } = "";
    public IReadOnlyList<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();

    public static readonly PlayerHUDProps Default = new PlayerHUDProps();
}
```

### UITKX component

Extract the typed props in `@code`:

```uitkx
@component PlayerHUD

@code {
    var hud = (__rawProps.TryGetValue("data", out var __d) ? __d : null)
              as PlayerHUDProps ?? PlayerHUDProps.Default;
}

<Box>
    <Label text={$"HP: {hud.Health} / {hud.MaxHealth}"}/>
    ...
</Box>
```

### Presenter / MonoBehaviour

```csharp
using ReactiveUITK.Props;

// Create props once
var props = new PlayerHUDProps { Health = 100, MaxHealth = 100 };

// Bind a Signal<int> — re-renders when the signal fires
_disposables.Add(PropsHelper.Bind(
    propsInstance : props,
    selector      : (PlayerHUDProps p) => p.Health,
    signal        : _healthSignal,
    onChanged     : RequestRender));

// Bind an INotifyPropertyChanged source
_disposables.Add(PropsHelper.Bind(
    selector       : (PlayerHUDProps p) => p.Health,
    source         : _viewModel,
    sourceProperty : (PlayerViewModel vm) => vm.Health,
    onChange       : (name, value) => { props.Health = (int)value; RequestRender(); }));

// Render
var node = V.Func(PlayerHUD.Render,
    new Dictionary<string, object> { { "data", props } });
```

---

## Common pitfalls

### 1. Missing `partial` keyword

The companion `.cs` file **must** use `partial class`, not `static class`. The
generator emits into the same partial class.

```csharp
// WRONG — static class cannot be partial
public static class MyComp { }

// CORRECT
public partial class MyComp { }
```

### 2. Namespace mismatch

The `@namespace` directive must exactly match the namespace in the companion `.cs`
file. A mismatch produces `CS0101: The namespace already contains a definition for 'MyComp'`.

### 3. Forgetting `key` on `@foreach` children

```uitkx
@foreach (var item in list) {
    <Box>                    ← UITKX0009 warning — missing key
        ...
    </Box>
}

@foreach (var item in list) {
    <Box key={item.Id}>      ← Correct
        ...
    </Box>
}
```

### 4. Expressions that need braces

Attribute values that are C# expressions must be wrapped in `{ }`:

```uitkx
<Label text="literal"/>           ← plain string, no braces needed
<Label text={$"count: {count}"}/>  ← C# expression, braces required
<Button onClick={() => setX(1)}/>  ← lambdas also need braces
```

### 5. `@code` scope

Variables declared in `@code` are local to the generated `Render()` method.
Do not declare instance fields there. For state shared across renders, use
`Hooks.UseState` or `Hooks.UseRef`.

### 6. `__rawProps` vs typed props

Inside a UITKX component the raw dictionary is available as `__rawProps`.
Access it in `@code` to extract typed props, as shown in the typed-props section above.
Avoid accessing `__rawProps` directly inside markup expressions — extract into
a local variable in `@code` first.

---

## Full example: counter migration

See [UitkxCounterFunc.uitkx](../Samples/Components/UitkxCounterFunc.uitkx) and
its companion [UitkxCounterFunc.cs](../Samples/Components/UitkxCounterFunc.cs).

The original hand-written equivalent is
[SimpleCounterFunc.cs](../Samples/Components/SimpleCounterFunc.cs).

For a more complete example that covers all directives and typed props, see
[PlayerHUD.uitkx](../Samples/Components/PlayerHUD.uitkx) and
[PlayerHUDProps.cs](../Samples/Components/PlayerHUDProps.cs).
