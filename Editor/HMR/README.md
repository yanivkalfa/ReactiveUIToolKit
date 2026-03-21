# UITKX Hot Module Replacement (HMR)

Hot Module Replacement lets you edit `.uitkx` files and see changes instantly in the Unity
Editor — without domain reload, without losing component state.

> **Full documentation:** see the [HMR page on the docs site](/tooling/hmr) (`ReactiveUIToolKitDocs~/` → Tooling → Hot Module Replacement).

## Quick Start

1. Open **ReactiveUITK → HMR Mode** from the Unity menu bar
2. Click **Start HMR**
3. Edit and save any `.uitkx` file
4. The component updates in-place — hook state (counters, refs, effects) is preserved

## How It Works

When HMR is active:

- **Assembly reloads are locked** — no domain reload occurs on file saves
- A `FileSystemWatcher` detects `.uitkx` changes under `Assets/`
- The file is parsed and emitted to C# using `ReactiveUITK.Language.dll`
- C# is compiled in-process via Roslyn (`Microsoft.CodeAnalysis.CSharp` 4.3.1), with
  automatic fallback to external `csc.dll` if Roslyn DLLs aren't available
- The compiled assembly is loaded via `Assembly.Load(byte[])`
- The new `Render` delegate is swapped into all active `RootRenderer` instances
- A re-render is triggered — hooks run against preserved state

Total time: typically **25–100 ms** compile + emit from save to visual update
(first compile per session is ~1–1.5s due to Roslyn JIT warmup).

## Lifecycle

| Event | Behavior |
|---|---|
| Start HMR | Assembly reload locked, file watcher started |
| Stop HMR | Assembly reload unlocked, pending changes compile normally |
| Enter/Exit Play Mode | Auto-stops HMR (configurable) |
| Build (Player) | Auto-stops HMR |
| Editor quit | Auto-stops HMR |

While HMR is active, **all compilation is deferred** — not just `.uitkx` changes. Any `.cs`
edits accumulate and compile in one batch when HMR is stopped.

## Keyboard Shortcuts

Shortcuts are **not bound by default** — you configure them in the HMR window to avoid
conflicting with your existing keybindings.

Available actions:

| Action | Description |
|---|---|
| **Toggle HMR** | Start or stop the HMR session |
| **Open/Close Window** | Show or hide the HMR window |

To set a shortcut:

1. Open the HMR window (**ReactiveUITK → HMR Mode**)
2. Expand **Keyboard Shortcuts**
3. Click the button next to an action (shows "Not set" by default)
4. Press your desired key combination (e.g. `Ctrl + Alt + H`)
5. The shortcut is saved immediately and persists across sessions

Requirements:
- At least one modifier key (Ctrl, Alt, or Shift)
- One regular key
- Click **×** to clear a binding

## Window UI

The HMR window shows:

- **Start/Stop** button with status indicator
- **Stats**: swap count, error count, last component name and timing
- **Timing breakdown**: Parse, Emit, Compile, and Swap durations per step
- **Settings**: auto-stop on play mode, swap notifications
- **Keyboard Shortcuts**: configurable bindings
- **Recent Errors**: last 10 compilation errors (scrollable, copyable)

## Companion Files

Companion `.cs` files are **optional**. The source generator produces a complete class from
the `.uitkx` file alone — no boilerplate needed. When you do add `.cs` files in the same
directory, HMR automatically includes them (excluding `.g.cs`) in the compilation:

- Style helpers (e.g. `MyComponent.styles.cs`)
- Type / prop definitions (e.g. `MyComponent.types.cs`)
- Shared utilities (e.g. `MyComponent.utils.cs`)

Companion `.cs` file changes also trigger HMR — saving a `.styles.cs` or `.utils.cs` file
automatically detects the associated `.uitkx` in the same directory, recompiles everything,
and swaps the result in-place.

## New Component Support

HMR can compile and load **new** `.uitkx` files that don't exist in any pre-compiled assembly:

- When a parent component references an unknown child, CS0103 errors are caught
- HMR scans the project for matching `.uitkx` files and compiles them first
- The parent is automatically retried after the dependency resolves
- Cross-component references are managed via an assembly registry

## Hook State Preservation

HMR preserves all hook state across swaps:

- `useState` — current values retained
- `useRef` — ref objects preserved
- `useEffect` — cleanup runs, effect re-runs with new closure
- `useMemo` / `useCallback` — recomputed with new function body
- `useContext` — context values preserved

If the **number or order of hooks changes** between edits, HMR detects the mismatch,
resets state for that component, and logs a warning:
```
[HMR] Hook mismatch in MyComponent, state was reset
```

## Limitations

| Limitation | Details |
|---|---|
| Old assemblies stay in memory | Mono cannot unload assemblies. ~10-30 KB per swap, cleared on domain reload. |
| All compilation deferred | Non-UITKX `.cs` changes don't take effect until HMR stops. UX warning shown. |
| Static field changes ignored | Statics live on the old assembly's type. |
| Cross-assembly props | Props are read via reflection to handle type mismatches across assemblies. |
| First compile is slow | ~1–1.5s on first HMR compile per session (Roslyn JIT warmup). Subsequent compiles are 25–100ms. |
| Requires NuGet cache | In-process Roslyn loads DLLs from `~/.nuget/packages/`. Falls back to external `csc.dll` if unavailable. |

## Troubleshooting

**HMR doesn't start**
- Check the Console for initialization errors
- Ensure `ReactiveUITK.Language.dll` exists in the `Analyzers/` folder
- Verify Unity's Roslyn compiler is present at `{EditorPath}/Data/DotNetSdkRoslyn/csc.dll`
- Check for `[HMR] In-process Roslyn compiler loaded successfully` in Console
- If Roslyn fails to load, check that `~/.nuget/packages/microsoft.codeanalysis.csharp/4.3.1/` exists

**Changes don't appear**
- Confirm the file is saved (not just modified)
- Check the HMR window for compilation errors
- Verify the file is under `Assets/` (the watched directory)

**State is lost after edit**
- Hook order/count may have changed — this triggers an automatic state reset
- Check Console for "[HMR] Hook mismatch" messages

**Props/callbacks are null after swap**
- This should be fixed with reflection-based prop reading
- If it persists, check that prop names match between parent and child components

## Files

| File | Purpose |
|---|---|
| `UitkxHmrWindow.cs` | Editor window UI |
| `UitkxHmrController.cs` | Orchestrates HMR lifecycle |
| `UitkxHmrFileWatcher.cs` | FileSystemWatcher with debounce |
| `UitkxHmrCompiler.cs` | Parse → emit → compile → load (in-process Roslyn + external fallback) |
| `HmrCSharpEmitter.cs` | AST to C# code emission |
| `UitkxHmrDelegateSwapper.cs` | Fiber tree walk and delegate swap |
| `UitkxHmrKeybinds.cs` | Customizable keyboard shortcuts |
| `AssemblyReloadSuppressor.cs` | Assembly reload lock management |
