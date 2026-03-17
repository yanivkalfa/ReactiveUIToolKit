# ReactiveUIToolKit

ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit, with function components, hooks, a virtual node tree, and a typed props model that all run entirely in C# on top of UI Toolkit.

👉 Full documentation, guides, and examples: http://reactiveuitoolkit.info/

---

## UITKX — JSX-style markup for ReactiveUIToolKit

UITKX lets you write components as `.uitkx` files with XML-like markup instead
of nested `V.Box(...)` C# calls. A Roslyn source generator compiles each
`.uitkx` file into a C# partial class at build time — zero runtime overhead.

### Quick start (under 10 minutes)

**1. Install the VSCode extension** (optional but recommended)

Open the `ide-extensions~/vscode/` folder and run `npm install && npm run build`,
or install the pre-built `.vsix` from the latest release. The extension provides
syntax highlighting, IntelliSense, and auto-format for `.uitkx` files.

**2. Create a `.uitkx` file**

Add `MyComponent.uitkx` to your project (anywhere Unity picks up scripts):

```uitkx
component MyComponent {
    var (count, setCount) = Hooks.UseState(0);

    return (
        <Box>
            <Label text={$"Count: {count}"}/>
            <Button text="+" onClick={() => setCount(count + 1)}/>
        </Box>
    );
}
```

**3. Add a companion `.cs` file**

```csharp
// MyComponent.cs
namespace MyGame.UI
{
    public partial class MyComponent { }
}
```

The source generator emits `Render()` into the partial class automatically on
the next Unity compile.

For function-style `.uitkx` files, `@namespace` is not required in markup.
Namespace is inferred from the companion partial `.cs` file.

**4. Use the component**

```csharp
var node = V.Func(MyComponent.Render, props);
```

That's it — no reflection, no codegen at runtime.

---

## Samples

| File | What it demonstrates |
|---|---|
| [`Samples/Components/UitkxCounterFunc.uitkx`](Samples/Components/UitkxCounterFunc.uitkx) | `@switch`, `@if/@else`, `@for`, nested conditionals |
| [`Samples/Components/PlayerHUD.uitkx`](Samples/Components/PlayerHUD.uitkx) | `@foreach` with `key`, `@switch`, `@if/@else`, typed props |
| [`Samples/Components/SimpleCounterFunc.cs`](Samples/Components/SimpleCounterFunc.cs) | Equivalent hand-written C# (migration starting point) |

---

## Migrating existing components

See the **[Migration Guide](ReactiveUIToolKitDocs~/MIGRATION_GUIDE.md)** for a
step-by-step walkthrough, directive reference, typed-props integration pattern,
and common pitfalls.

