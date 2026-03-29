# ReactiveUIToolKit

ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit, with function components, hooks, a virtual node tree, and a typed props model that all run entirely in C# on top of UI Toolkit.

👉 Full documentation, guides, and examples: http://reactiveuitoolkit.info/

---

## Architecture

### Directory Map

| Directory | Description |
|---|---|
| `Runtime/` | Thin MonoBehaviour adapter — `RootRenderer`, `RenderScheduler` |
| `Shared/` | Core reactive library — `V`, `VNode`, Hooks, Fiber reconciler, Elements, Props |
| `Editor/` | Unity Editor integration — HMR, change watcher, console navigation |
| `Analyzers/` | Published Roslyn analyzer / source generator DLLs |
| `Samples/` | Demo components: legacy C# (`Components/`), UITKX (`UITKX/`), showcase app (`Showcase/`) |
| `SourceGenerator~/` | Source generator source code + tests |
| `ide-extensions~/` | IDE extension projects (VS Code, VS2022, Rider, shared LSP server) |
| `Plans~/` | Design documents and implementation plans |
| `ReactiveUIToolKitDocs~/` | Documentation website (Vite + React) |
| `scripts/` | Build and publish automation |

### Key Architectural Decisions

- **Shared is the core** — `Runtime/` is a thin adapter that hosts the reconciler inside Unity
- **Source generator** transpiles `.uitkx` → C# partial classes at build time (zero runtime overhead)
- **LSP server** (`language-lib` + `lsp-server`) provides IDE features across all editors
- **HMR** compiles `.uitkx` changes in-editor without domain reload (50–200 ms cycle)
- `~` suffix hides IDE / dev folders from Unity's Asset Database

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

See [`Samples/README.md`](Samples/README.md) for a full breakdown of each sample category.

---

## Typed Style System

`Style` is a typed dictionary class with **set-only properties** that map to every
Unity UI Toolkit inline style. Values are compile-time checked — passing a `float`
where a `Color` is expected is a build error.

```csharp
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.CssHelpers;

var cardStyle = new Style {
    Width = Pct(100),
    Height = Px(200),
    BackgroundColor = Rgba(0.1f, 0.1f, 0.15f, 0.9f),
    FlexDirection = Column,
    JustifyContent = SpaceBetween,
    AlignItems = AlignCenter,
    Padding = 16f,
    BorderRadius = 8f,
};
```

| Category | Type | Properties |
|---|---|---|
| Layout & spacing | `StyleLength` | Width, Height, Margin*, Padding*, FlexBasis, BorderRadius*, FontSize, LetterSpacing |
| Flex & opacity | `StyleFloat` | FlexGrow, FlexShrink, Opacity, BorderWidth* |
| Colors | `Color` | Color, BackgroundColor, BorderColor*, UnityTextOutlineColor (9 total) |
| Enums | Unity enums | FlexDirection, JustifyContent, AlignItems, Position, Display, Visibility, etc. (15 total) |
| Background | Structs | BackgroundRepeat, BackgroundPositionX/Y, BackgroundSize |
| Transforms | `float` / struct | Rotate, Scale, Translate, TransformOrigin |
| Assets | `Texture2D` / `Font` | BackgroundImage, FontFamily |

**`CssHelpers`** (import via `using static`) provides shortcuts: `Pct()`, `Px()`,
`Auto`, `None`, `Row`, `Column`, `JustifyCenter`, `AlignCenter`, `Hex("#FF0000")`,
`Rgba(255, 0, 0)`, color presets (`White`, `Black`, `Red`, …), and all enum values.

The old tuple syntax `(StyleKeys.Key, value)` remains available as an escape hatch.

👉 Full guide: [Styling documentation](http://reactiveuitoolkit.info/styling)

---

## Development Setup

### Prerequisites

- Unity 6000.2+
- .NET 8+ SDK
- Node.js 18+ (for VS Code extension and docs site)
- Visual Studio 2022 (for VS2022 extension development)

### Building Components

**Source Generator**
```
dotnet build SourceGenerator~/ReactiveUITK.SourceGenerator.csproj
scripts/build-generator.ps1   # builds + copies DLL to Analyzers/
```

**IDE Extensions**
```bash
# VS Code
cd ide-extensions~/vscode && npm run build

# VS2022
ide-extensions~/visual-studio/build-local.ps1

# LSP Server (shared)
dotnet publish ide-extensions~/lsp-server -c Release
```

**Documentation Site**
```bash
cd ReactiveUIToolKitDocs~ && npm run dev
```

### Running Tests

```
dotnet test SourceGenerator~/Tests
```

