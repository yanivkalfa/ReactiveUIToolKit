# UITKX Hot Module Replacement (HMR) — Implementation Plan

**Status:** � IMPLEMENTED (pending testing)  
**Priority:** High — core DX improvement  
**Depends on:** Fiber architecture (complete), Source generator pipeline (complete)

---

## 1. Problem Statement

Editing a `.uitkx` file currently requires **two sequential wait steps** before the result is visible:

1. **Source generator** — Roslyn reads the `.uitkx` via `AdditionalText`, source generator emits `.g.cs` (~1-2 s)
2. **Domain reload** — Unity recompiles all assemblies, destroys and rebuilds the AppDomain (~3-10 s)

Domain reload kills **all** runtime state: every Fiber tree, every hook, every open window.
The user must then manually re-navigate to the screen they were iterating on.

**Goal:** Reduce the edit → preview cycle to **< 500 ms** with **full state preservation** — no domain reload, no lost hook state, no window rebuilds.

---

## 2. Approach — In-Process Roslyn Compilation + Delegate Swap

### Core Idea

Instead of relying on Unity's compilation pipeline, an **editor-side HMR system** will:

1. Detect `.uitkx` (and companion `.cs`) file saves via `FileSystemWatcher`
2. Run the UITKX source generator pipeline **in-process** (parser → emitter)
3. Compile the emitted C# to a byte-array DLL using **Roslyn CSharpCompilation** in-process
4. Load the DLL via `Assembly.Load(byte[])`
5. Extract the new `Render` delegate from the loaded type
6. **Swap** `FiberNode.TypedRender` on every matching fiber in every active tree
7. **Trigger re-render** via `FiberReconciler.ScheduleUpdateOnFiber()` — hooks and state are preserved

### Why This Works

- **State lives on the FiberNode, not the render function.** `FiberNode.ComponentState` contains
  all hook data (`HookStates: List<object>`, effects, refs, memo caches). Swapping the render
  delegate and re-rendering produces new VNodes from the same hook state — identical to what
  happens on a normal `setState` re-render.

- **Assembly.Load is additive.** The old assembly stays loaded (Mono cannot unload) but the
  reference to the old `MethodInfo` is simply replaced. Memory cost ~10-30 KB per swap,
  fully reclaimed on next domain reload. Acceptable for development.

- **The source generator pipeline is Roslyn-free.** `DirectiveParser`, `UitkxParser`,
  `CanonicalLowering`, and `CSharpEmitter` are all pure C# classes in
  `ReactiveUITK.Language.dll` (netstandard2.0). Only `PropsResolver` touches Roslyn, and it has a
  `BuildFallbackMap()` with hardcoded entries for all 25+ built-in elements — no real Roslyn
  Compilation needed for tag resolution.

---

## 3. User-Facing Design — HMR Mode

### 3.1 Editor Window

A lightweight editor window accessible via **UITKX → HMR Mode** menu item.

```
┌──────────────────────────────────┐
│  UITKX Hot Reload                │
│                                  │
│  [ ● Start HMR ]                │  ← Green play button
│  Status: Idle                    │
│                                  │
│  Watched: Assets/**/*.uitkx      │
│  Swaps:   0                      │
│  Errors:  0                      │
│                                  │
│  ☑ Auto-stop on Play Mode        │
│  ☑ Show swap notifications       │
└──────────────────────────────────┘
```

When HMR is active:

```
┌──────────────────────────────────┐
│  UITKX Hot Reload                │
│                                  │
│  [ ■ Stop HMR ]                 │  ← Red stop button
│  Status: ● ACTIVE                │
│                                  │
│  Watched: Assets/**/*.uitkx      │
│  Swaps:   7                      │
│  Errors:  0                      │
│                                  │
│  Last: SimpleCounter (42ms)      │
│                                  │
│  ☑ Auto-stop on Play Mode        │
│  ☑ Show swap notifications       │
│                                  │
│  ⚠ Assembly reload locked.       │
│    Stop HMR to compile normally. │
└──────────────────────────────────┘
```

### 3.2 Lifecycle

| Event                        | Action                                                   |
|------------------------------|----------------------------------------------------------|
| **Start pressed**            | Lock assemblies, disable auto-refresh, start watcher     |
| **Stop pressed**             | Unlock assemblies, allow auto-refresh, stop watcher, force `AssetDatabase.Refresh()` |
| **Editor quit**              | Auto-stop (cleanup in `OnDisable` / `OnDestroy`)         |
| **Enter Play Mode**          | Auto-stop (via `EditorApplication.playModeStateChanged`)  |
| **Exit Play Mode**           | Auto-stop                                                |
| **Pre-build**                | Auto-stop (via `BuildPlayerProcessor.OnPreprocessBuild`)  |
| **File change while active** | Trigger HMR pipeline                                     |

### 3.3 Assembly Reload Suppression

When HMR is active, **all** compilation is suppressed — not just `.uitkx`-related changes:

```csharp
EditorApplication.LockReloadAssemblies();   // Counter-based, must be balanced
AssetDatabase.DisallowAutoRefresh();         // Counter-based, must be balanced
```

This is necessary because `UitkxChangeWatcher` (a sealed `AssetPostprocessor`) automatically calls
`CompilationPipeline.RequestScriptCompilation()` on every `.uitkx` change. We cannot disable
the watcher, but by locking assembly reloads, the compilation request is deferred until
HMR is stopped.

On stop:

```csharp
AssetDatabase.AllowAutoRefresh();
EditorApplication.UnlockReloadAssemblies();
AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);  // Catch up on all changes
```

---

## 4. Architecture

### 4.1 Component Diagram

```
                       ┌─────────────────────────────┐
                       │     UitkxHmrWindow           │
                       │     (EditorWindow)            │
                       │  Start/Stop button, status    │
                       └────────┬────────────────┬─────┘
                                │                │
                     ┌──────────▼──────────┐     │
                     │  UitkxHmrController  │     │
                     │  (orchestrator)      │◄────┘
                     └──┬────┬────┬────┬───┘
                        │    │    │    │
           ┌────────────▼┐   │    │    └───────────────┐
           │ FileWatcher  │   │    │                    │
           │ (FSWatcher)  │   │    │                    ▼
           └──────┬───────┘   │    │         ┌─────────────────┐
                  │           │    │         │ AssemblyReload   │
                  │    ┌──────▼────▼──┐     │ Suppressor       │
                  │    │  HmrCompiler  │     └─────────────────┘
                  │    │              │
                  │    │  1. Parse    │
                  │    │  2. Emit     │
                  │    │  3. Compile  │
                  │    │  4. Load     │
                  │    └──────┬───────┘
                  │           │
                  │    ┌──────▼───────────┐
                  │    │  DelegateSwapper  │
                  │    │                  │
                  │    │  Walk all trees  │
                  │    │  Swap TypedRender│
                  │    │  Re-render fibers│
                  │    └──────────────────┘
                  │
      ┌───────────▼────────────┐
      │ .uitkx / .cs save      │
      └────────────────────────┘
```

### 4.2 Class Responsibilities

| Class                       | Assembly            | Responsibility |
|-----------------------------|---------------------|----------------|
| `UitkxHmrWindow`           | ReactiveUITK.Editor | Editor window UI, Start/Stop button, status display |
| `UitkxHmrController`       | ReactiveUITK.Editor | Orchestrates the full HMR lifecycle |
| `UitkxHmrFileWatcher`      | ReactiveUITK.Editor | `FileSystemWatcher` on project folder, debounce, filter |
| `UitkxHmrCompiler`         | ReactiveUITK.Editor | Runs parser → emitter → Roslyn compile → Assembly.Load |
| `UitkxHmrDelegateSwapper`  | ReactiveUITK.Editor | Walks all Fiber trees, swaps delegates, triggers re-render |
| `AssemblyReloadSuppressor` | ReactiveUITK.Editor | Manages LockReloadAssemblies / DisallowAutoRefresh lifecycle |

### 4.3 New Files

All in `Editor/HMR/`:

```
Editor/
  HMR/
    UitkxHmrWindow.cs
    UitkxHmrController.cs
    UitkxHmrFileWatcher.cs
    UitkxHmrCompiler.cs
    UitkxHmrDelegateSwapper.cs
    AssemblyReloadSuppressor.cs
```

---

## 5. Detailed Design

### 5.1 File Watcher (`UitkxHmrFileWatcher`)

```csharp
// Uses System.IO.FileSystemWatcher (available in Unity Editor on all platforms)
// Watches: Assets/ recursively
// Filters: *.uitkx AND *.cs (companion files)
// Debounce: 100ms — coalesce rapid multi-file saves from IDE "Save All"
// Thread: FileSystemWatcher fires on a threadpool thread → marshal to main thread
//         via EditorApplication.delayCall

public class UitkxHmrFileWatcher : IDisposable
{
    public event Action<string> OnFileChanged;  // Fires on main thread, debounced

    public void Start(string projectRoot);
    public void Stop();
    public void Dispose();
}
```

**Filtering logic:**
- `.uitkx` file changed → HMR compile for that component
- `.cs` file changed → check if it's a companion file (same directory as a `.uitkx` file with matching partial class) → HMR compile for the associated component
- Other `.cs` files → ignore (they'll compile on HMR stop)

### 5.2 Compiler (`UitkxHmrCompiler`)

#### 5.2.1 Parse & Emit (Roslyn-free)

Uses `ReactiveUITK.Language.dll` directly:

```csharp
// Step 1: Read .uitkx source
string uitkxSource = File.ReadAllText(uitkxPath);

// Step 2: Parse directives
var directives = DirectiveParser.Parse(uitkxSource);

// Step 3: Parse AST
var parseResult = UitkxParser.Parse(uitkxSource, directives);

// Step 4: Lower to render roots (optional — may use raw AST)
var lowered = CanonicalLowering.LowerToRenderRoots(parseResult);

// Step 5: Resolve props (use fallback map — no Roslyn needed)
var propsMap = PropsResolver.BuildFallbackMap();
var resolved = PropsResolver.Resolve(lowered, propsMap);

// Step 6: Emit C#
string csharpSource = CSharpEmitter.Emit(resolved, directives);
```

#### 5.2.2 Companion File Inclusion

If companion `.cs` files exist alongside the `.uitkx`:

```
MyComponent.uitkx          → emits MyComponent class with Render method
MyComponent.styles.cs      → partial class MyComponent { ... }
MyComponent.types.cs       → partial class MyComponent { ... }
```

All companion files are included in the Roslyn compilation as additional syntax trees.

#### 5.2.3 Roslyn Compilation

```csharp
// Roslyn packages needed: Microsoft.CodeAnalysis.CSharp (already in SourceGenerator~/)
// At runtime in Unity Editor: use the Roslyn DLLs from Unity's own installation
// or bundle a copy in Editor/HMR/Plugins/

var syntaxTree = CSharpSyntaxTree.ParseText(csharpSource);
var companionTrees = companionFiles.Select(f =>
    CSharpSyntaxTree.ParseText(File.ReadAllText(f)));

var compilation = CSharpCompilation.Create(
    assemblyName: $"UITKX_HMR_{componentName}_{swapCount}",
    syntaxTrees: new[] { syntaxTree }.Concat(companionTrees),
    references: GetMetadataReferences(),
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
);

using var ms = new MemoryStream();
var result = compilation.Emit(ms);

if (!result.Success)
{
    // Report errors to HMR window
    ReportErrors(result.Diagnostics);
    return null;
}

byte[] assemblyBytes = ms.ToArray();
Assembly loadedAssembly = Assembly.Load(assemblyBytes);
```

#### 5.2.4 Metadata References

The compiled code references types from these assemblies:

| Type / Namespace                | Assembly                    | How to get MetadataReference |
|---------------------------------|-----------------------------|------------------------------|
| `ReactiveUITK.V`               | ReactiveUITK.Shared         | `typeof(V).Assembly.Location` |
| `ReactiveUITK.Core.VirtualNode`| ReactiveUITK.Shared         | same as above |
| `ReactiveUITK.Core.IProps`     | ReactiveUITK.Shared         | same as above |
| `ReactiveUITK.Core.Hooks`      | ReactiveUITK.Shared         | same as above |
| `ReactiveUITK.Props.Typed.*`   | ReactiveUITK.Shared         | same as above |
| `UnityEngine.Color`            | UnityEngine.CoreModule      | `typeof(UnityEngine.Color).Assembly.Location` |
| `UnityEngine.UIElements.*`     | UnityEngine.UIElementsModule| `typeof(UnityEngine.UIElements.VisualElement).Assembly.Location` |
| `System.*`                     | mscorlib, System.Core, etc. | `typeof(object).Assembly.Location` |
| `System.Collections.Generic`   | mscorlib                    | already covered |
| `System.Linq`                  | System.Core                 | `typeof(System.Linq.Enumerable).Assembly.Location` |

**Implementation:**

```csharp
private static List<MetadataReference> GetMetadataReferences()
{
    var refs = new List<MetadataReference>();

    // Core .NET
    refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
    refs.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location));

    // Unity
    refs.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.Color).Assembly.Location));
    refs.Add(MetadataReference.CreateFromFile(
        typeof(UnityEngine.UIElements.VisualElement).Assembly.Location));

    // ReactiveUITK.Shared (V, VirtualNode, IProps, Hooks, Props)
    refs.Add(MetadataReference.CreateFromFile(typeof(ReactiveUITK.V).Assembly.Location));

    // ReactiveUITK.Runtime (UitkxElementAttribute, RootRenderer)
    refs.Add(MetadataReference.CreateFromFile(
        typeof(ReactiveUITK.UitkxElementAttribute).Assembly.Location));

    // Add all loaded assemblies that the component might reference
    // (user code assemblies — e.g. game-specific types used in @code blocks)
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (asm.IsDynamic || string.IsNullOrEmpty(asm.Location)) continue;
        try { refs.Add(MetadataReference.CreateFromFile(asm.Location)); }
        catch { /* skip assemblies that can't be read */ }
    }

    return refs;
}
```

> **Note:** Loading all assemblies ensures user types referenced in `@code` blocks resolve
> correctly. The compilation takes ~50-200ms even with many references since we're compiling
> a single small file.

#### 5.2.5 Roslyn DLL Availability

The Roslyn compiler DLLs (`Microsoft.CodeAnalysis.dll`, `Microsoft.CodeAnalysis.CSharp.dll`)
need to be available at runtime in the Unity Editor. Options:

| Option | Pros | Cons |
|--------|------|------|
| **A. Bundle in Editor/HMR/Plugins/** | Self-contained, always works | Adds ~5 MB to package |
| **B. Reference from Unity's installation** | No extra size | Path varies by Unity version, fragile |
| **C. Reference from SourceGenerator~/packages/** | Already downloaded | NuGet cache path varies |

**Recommendation: Option A** — bundle `Microsoft.CodeAnalysis.dll` and
`Microsoft.CodeAnalysis.CSharp.dll` in `Editor/HMR/Plugins~` (hidden from Unity's asset
pipeline via `~` suffix), loaded at runtime via `Assembly.LoadFrom()`.

Alternatively, reference them from the NuGet global cache if present, with Option A as runtime fallback.

### 5.3 Delegate Swapper (`UitkxHmrDelegateSwapper`)

#### 5.3.1 Extract New Render Delegate

```csharp
// The generated class has [UitkxElement("ComponentName")] attribute
// and a static Render(IProps, IReadOnlyList<VirtualNode>) method

Type newType = loadedAssembly.GetTypes()
    .FirstOrDefault(t => t.GetCustomAttribute<UitkxElementAttribute>()
        ?.ComponentName == componentName);

MethodInfo renderMethod = newType.GetMethod("Render",
    BindingFlags.Public | BindingFlags.Static);

var newDelegate = (Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>)
    Delegate.CreateDelegate(
        typeof(Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>),
        renderMethod);
```

#### 5.3.2 Walk All Fiber Trees

Must walk **every active Fiber tree** across both Editor and Runtime renderers:

```csharp
public static void SwapDelegates(string componentName,
    Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate)
{
    int swapCount = 0;

    // 1. Editor renderers — via EditorRootRendererUtility
    foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
    {
        swapCount += WalkAndSwap(renderer.GetFiberRoot(), componentName, newDelegate);
    }

    // 2. Runtime renderer — via RootRenderer.Instance
    if (RootRenderer.Instance != null)
    {
        var root = RootRenderer.Instance.GetFiberRoot();
        if (root != null)
            swapCount += WalkAndSwap(root, componentName, newDelegate);
    }

    Debug.Log($"[HMR] Swapped {swapCount} instance(s) of {componentName}");
}
```

#### 5.3.3 Tree Walking

```csharp
private static int WalkAndSwap(FiberRoot root, string componentName,
    Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate)
{
    int count = 0;
    var fiber = root.Current;
    WalkFiber(fiber, componentName, newDelegate, ref count);
    return count;
}

private static void WalkFiber(FiberNode fiber, string componentName,
    Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate,
    ref int count)
{
    if (fiber == null) return;

    if (fiber.Tag == FiberTag.FunctionComponent && IsMatchingComponent(fiber, componentName))
    {
        // Swap the render delegate
        fiber.TypedRender = newDelegate;

        // Check if hook signature changed → reset state if needed
        ValidateHookCompatibility(fiber);

        // Schedule re-render (same mechanism as setState)
        fiber.ComponentState?.OnStateUpdated?.Invoke();
        count++;
    }

    // Recurse: first child, then sibling
    WalkFiber(fiber.Child, componentName, newDelegate, ref count);
    WalkFiber(fiber.Sibling, componentName, newDelegate, ref count);
}
```

#### 5.3.4 Component Matching

The fiber doesn't store the component name directly. We match by inspecting the
render delegate's declaring type:

```csharp
private static bool IsMatchingComponent(FiberNode fiber, string componentName)
{
    if (fiber.TypedRender == null) return false;

    // Check declaring type name against component name
    var declaringType = fiber.TypedRender.Method.DeclaringType;
    if (declaringType == null) return false;

    // Direct name match (generated class name == component name)
    if (declaringType.Name == componentName) return true;

    // Also check UitkxElement attribute
    var attr = declaringType.GetCustomAttribute<UitkxElementAttribute>();
    return attr?.ComponentName == componentName;
}
```

#### 5.3.5 Hook Compatibility Validation

When the number or order of hooks changes between edits, the existing hook state becomes
invalid. Following React Fast Refresh semantics:

```csharp
private static void ValidateHookCompatibility(FiberNode fiber)
{
    // After swap, do a trial render to count hooks
    // If hook count differs from ComponentState.HookStates.Count → reset state
    //
    // Implementation: Set a "validate" flag, run one render cycle,
    // compare hook call count. If mismatched, clear HookStates and
    // re-mount (lose state for this one component only).
    //
    // Simple v1: Always preserve state. If hooks mismatch, the
    // Hooks infrastructure will throw an index-out-of-range,
    // which we catch and trigger a full re-mount of that fiber.
}
```

**V1 Strategy (simple):** Wrap the re-render in a try-catch. If hooks fail due to
count/order mismatch, log a warning and re-mount that fiber subtree (clearing its state).
This is the same behavior React Fast Refresh uses.

### 5.4 Assembly Reload Suppressor (`AssemblyReloadSuppressor`)

```csharp
public class AssemblyReloadSuppressor : IDisposable
{
    private bool _active;

    public void Lock()
    {
        if (_active) return;
        _active = true;
        EditorApplication.LockReloadAssemblies();
        AssetDatabase.DisallowAutoRefresh();
    }

    public void Unlock()
    {
        if (!_active) return;
        _active = false;
        AssetDatabase.AllowAutoRefresh();
        EditorApplication.UnlockReloadAssemblies();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    public void Dispose() => Unlock();
}
```

### 5.5 Fiber Reuse Fix — CanReuseFiber Across Assemblies

**Problem:** After HMR swap, the `CanReuseFiber` check in `FiberChildReconciliation.cs` (line 264-271)
compares delegates via `fiber.TypedRender.Method == vnode.TypedFunctionRender.Method`.
After a swap, the fiber's `TypedRender` points to the NEW assembly's method, but parent
components' VNodes still reference the OLD assembly's method (they haven't been re-compiled).

**Impact:** This happens when a parent component renders a child that was HMR-swapped. The parent's
VNode tree still has `V.Func<TProps>(OldClass.Render, ...)` but the fiber now has
`NewClass.Render`. The `CanReuseFiber` check fails → fiber is unmounted and remounted →
**state is lost**.

**Solution:** Add a **secondary match** by component name when the method comparison fails:

```csharp
// In CanReuseFiber, after the existing delegate comparison:
case VirtualNodeType.FunctionComponent:
    if (fiber.Tag != FiberTag.FunctionComponent) return false;
    if (fiber.TypedRender == null || vnode.TypedFunctionRender == null) return false;
    if (ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)) return true;
    if (fiber.TypedRender.Method == vnode.TypedFunctionRender.Method
        && fiber.TypedRender.Target == vnode.TypedFunctionRender.Target) return true;

    // HMR fallback: if methods differ only by assembly, treat as same component
    if (UitkxHmrController.IsActive)
    {
        var fiberType = fiber.TypedRender.Method.DeclaringType;
        var vnodeType = vnode.TypedFunctionRender.Method.DeclaringType;
        if (fiberType != null && vnodeType != null
            && fiberType.Name == vnodeType.Name
            && fiber.TypedRender.Method.Name == vnode.TypedFunctionRender.Method.Name)
        {
            return true;
        }
    }
    return false;
```

This check only runs when HMR is active, so there's zero performance impact in production.

---

## 6. Required Codebase Changes

### 6.1 Expose Private Fields (Internal Accessors)

The HMR system needs to walk all active Fiber trees. Several private fields must be exposed
with **internal** accessors (not public — HMR is editor-only code):

| File | Field | Current Access | Change |
|------|-------|----------------|--------|
| `Shared/Core/Fiber/FiberRenderer.cs:11` | `_root` (FiberRoot) | private | Add `internal FiberRoot Root => _root;` |
| `Shared/Core/VNodeHostRenderer.cs:20` | `fiberRenderer` (FiberRenderer) | private readonly | Add `internal FiberRenderer FiberRendererInternal => fiberRenderer;` |
| `Runtime/Core/RootRenderer.cs:15` | `vnodeHostRenderer` (VNodeHostRenderer) | private | Add `internal VNodeHostRenderer VNodeHostRendererInternal => vnodeHostRenderer;` |
| `Editor/EditorRootRendererUtility.cs:15` | `renderersByHost` (Dictionary) | private static readonly | Add `internal static IEnumerable<VNodeHostRenderer> GetAllRenderers() => renderersByHost.Values;` |

### 6.2 InternalsVisibleTo

Since HMR code lives in `ReactiveUITK.Editor` and needs internal access to `ReactiveUITK.Shared`
and `ReactiveUITK.Runtime`:

**Add to `Shared/` assembly:**
```csharp
// Shared/AssemblyInfo.cs (new file)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ReactiveUITK.Editor")]
```

**Add to `Runtime/` assembly:**
```csharp
// Runtime/AssemblyInfo.cs (new file)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ReactiveUITK.Editor")]
```

> **Note:** `FiberNode.ComponentState` is already `internal`, so this access is needed regardless.

### 6.3 CanReuseFiber Modification

In `Shared/Core/Fiber/FiberChildReconciliation.cs` (lines 264-271) and
`Shared/Core/Fiber/FiberFunctionComponent.cs` (same logic if duplicated):

Add HMR-aware fallback match as described in §5.5. Gate behind a static bool
`UitkxHmrController.IsActive` to avoid any runtime cost when HMR is off.

The static bool check uses:
```csharp
// In Shared assembly — a simple hook point:
internal static class HmrState
{
    internal static bool IsActive;
}
```

Set by `UitkxHmrController` in Editor assembly via InternalsVisibleTo.

### 6.4 No Changes to UitkxChangeWatcher

`UitkxChangeWatcher` is a sealed `AssetPostprocessor` that cannot be disabled. It will continue
to fire on every `.uitkx` change, calling `CompilationPipeline.RequestScriptCompilation()`.
However, with `LockReloadAssemblies()` active, Unity will **defer** the compilation until
assemblies are unlocked. This is the intended behavior — all changes accumulate and compile
in one batch when HMR is stopped.

---

## 7. Complete HMR Pipeline Flow

```
User saves MyComponent.uitkx
         │
         ▼
FileSystemWatcher fires (threadpool thread)
         │
         ▼  EditorApplication.delayCall (marshal to main thread)
         │
         ▼  Debounce 100ms
         │
         ▼
Read .uitkx source from disk
         │
         ▼
DirectiveParser.Parse() → directives (@namespace, @component, @using)
         │
         ▼
UitkxParser.Parse() → AST
         │
         ▼
CanonicalLowering.LowerToRenderRoots() → lowered AST
         │
         ▼
PropsResolver.Resolve() (using BuildFallbackMap())
         │
         ▼
CSharpEmitter.Emit() → C# source string
         │
         ▼
Find companion .cs files in same directory
         │
         ▼
CSharpCompilation.Create() with all references
         │
         ▼
compilation.Emit() → byte[]
         │
    ┌────┴────┐
    │ Errors? │
    ├─ Yes ───┤──→ Show in HMR window, log warning, STOP
    │ No      │
    └────┬────┘
         │
         ▼
Assembly.Load(byte[]) → Assembly
         │
         ▼
Find type with [UitkxElement("MyComponent")] attribute
         │
         ▼
Extract static Render method → create delegate
         │
         ▼
Walk ALL active Fiber trees:
  - EditorRootRendererUtility.GetAllRenderers()
  - RootRenderer.Instance
         │
         ▼
For each FiberNode where declaring type name matches:
  fiber.TypedRender = newDelegate
  fiber.ComponentState.OnStateUpdated?.Invoke()
         │
         ▼
Reconciler re-renders affected fibers
Hooks run against preserved HookStates
UI updates instantly (~50-200ms total)
```

---

## 8. Edge Cases & Error Handling

### 8.1 Compilation Errors

If the emitted C# fails to compile:
- Display errors in HMR window with mapped source locations
- Log to console with `.uitkx` file reference for console click navigation
- **Do not swap** — keep the previous working version active
- On next save, retry compilation

### 8.2 Hook Count/Order Changes

If a user changes hooks (adds/removes `useState`, `useEffect`, etc.):
- The existing `HookStates: List<object>` has the wrong size
- Hooks will throw `IndexOutOfRangeException` or return wrong types
- **Catch and remount:** Wrap `OnStateUpdated` invocation in try-catch;
  on failure, clear `ComponentState.HookStates`, set `ComponentState` to a fresh instance,
  and re-mount that fiber subtree
- Log: `"[HMR] Hook signature changed in MyComponent — state was reset"`

### 8.3 Multiple Components in Same File

UITKX supports one component per file. The component name equals the file name
(minus extension). No special handling needed.

### 8.4 New Components (File Created)

If a brand-new `.uitkx` file is created while HMR is active:
- The file watcher detects it
- HMR compiles it successfully
- But **no existing fibers** reference this component yet (no parent renders it)
- Result: DLL is loaded but no swap occurs — the new component becomes available
  when a parent that references it is also HMR-swapped, or on next domain reload

### 8.5 Deleted Components

If a `.uitkx` file is deleted while HMR is active:
- The file watcher detects the deletion
- No action needed — existing fibers continue with the last-loaded delegate
- On HMR stop + domain reload, Unity will report the missing component as expected

### 8.6 Runtime (Play Mode) vs Editor

HMR works in **both** modes:
- **Editor mode:** All editor windows using UITKX components update live
- **Play mode:** Game UI using `RootRenderer.Instance` also updates live

The auto-stop-on-play-mode-change is a **safety feature**, not a limitation. Users can
disable it via the checkbox to keep HMR active across play mode transitions.

### 8.7 Concurrent Saves

FileSystemWatcher may fire multiple times for a single save (write, flush, metadata update).
The 100ms debounce window coalesces these into a single HMR cycle.

If a new save arrives while a compilation is in progress, queue it and process after
the current cycle completes.

---

## 9. Performance Budget

| Step | Expected Time | Notes |
|------|---------------|-------|
| File read | < 1 ms | Small files |
| Parse + Emit | 5-20 ms | Roslyn-free pipeline, pure string operations |
| Roslyn compile | 50-150 ms | Single file, many references loaded once and cached |
| Assembly.Load | 5-10 ms | Small DLL (~10-30 KB) |
| Tree walk + swap | 1-5 ms | Typically < 100 fibers total |
| Re-render | 5-20 ms | Only affected component subtrees |
| **Total** | **~70-200 ms** | Well under the 500 ms target |

**Roslyn reference caching:** `MetadataReference` objects should be created once and cached
for the lifetime of the HMR session. Creating them from file paths is the expensive part;
reusing them across compilations is essentially free.

---

## 10. Memory Impact

| Item | Size | Lifecycle |
|------|------|-----------|
| Loaded HMR assembly | ~10-30 KB each | Cannot be unloaded (Mono). Freed on domain reload. |
| Roslyn compiler infrastructure | ~20-40 MB | Loaded on first compile, stays in memory until domain reload |
| MetadataReference cache | ~5-10 MB | Cached for session, freed on domain reload |
| FileSystemWatcher | Negligible | Disposed on HMR stop |

**Total overhead while HMR is active:** ~30-50 MB (one-time, acceptable for Editor-only dev tool).
Per-swap memory leak: ~10-30 KB (accumulates, cleared on any domain reload).

---

## 11. Implementation Phases

> **Status key:**
> - ⬜ = Not Started
> - ✅ = Completed
> - 🧪 = Tested

### Phase 1 — Core Pipeline (MVP)

**Goal:** End-to-end HMR for a single `.uitkx` component, editor mode only.

| # | Feature | Not Started | Completed | Tested |
|---|---------|:-----------:|:---------:|:------:|
| 1 | Create `Editor/HMR/` folder structure | | ✅ | |
| 2 | Implement `AssemblyReloadSuppressor` | | ✅ | |
| 3 | Implement `UitkxHmrCompiler` (parse → emit → Roslyn compile → Assembly.Load) | | ✅ | |
| 4 | Implement `UitkxHmrDelegateSwapper` (walk trees, swap, re-render) | | ✅ | |
| 5 | Expose private fields (§6.1) — add internal accessors | | ✅ | |
| 6 | Add `InternalsVisibleTo` attributes (§6.2) | | ✅ | |
| 7 | Add `HmrState.IsActive` and `CanReuseFiber` fallback (§6.3) | | ✅ | |
| 8 | Create minimal `UitkxHmrWindow` with Start/Stop button | | ✅ | |
| 9 | Implement basic `UitkxHmrFileWatcher` | | ✅ | |
| 10 | Wire everything through `UitkxHmrController` | | ✅ | |

**Validation:** Open an editor window with a UITKX counter component. Start HMR.
Change the counter's label text in the `.uitkx` file. Verify the label updates
instantly without losing the count state.

### Phase 2 — Robustness

| # | Feature | Not Started | Completed | Tested |
|---|---------|:-----------:|:---------:|:------:|
| 11 | Companion `.cs` file support (include in compilation) | | ✅ | |
| 12 | Hook change detection and state reset (§8.2) | | ✅ | |
| 13 | Error display in HMR window with source-mapped locations | | ✅ | |
| 14 | Console log integration with clickable `.uitkx` references | | ✅ | |
| 15 | Debounce and concurrent save handling | | ✅ | |
| 16 | Auto-stop lifecycle hooks (play mode, build, quit) | | ✅ | |

### Phase 3 — Polish

| # | Feature | Not Started | Completed | Tested |
|---|---------|:-----------:|:---------:|:------:|
| 17 | Play mode support (RootRenderer tree walking) | | ✅ | |
| 18 | Settings persistence (checkbox states via EditorPrefs) | | ✅ | |
| 19 | Status display (swap count, timing, last component name) | | ✅ | |
| 20 | Keyboard shortcut for Start/Stop | | ✅ | |
| 21 | Performance profiling and optimization | | ✅ | |
| 22 | Documentation | | ✅ | |

---

## 12. Roslyn DLL Bundling Strategy

### Where to Get the DLLs

The UITKX Source Generator already depends on `Microsoft.CodeAnalysis.CSharp` v4.3.1.
The DLLs are in the NuGet package cache after building the source generator project.

Required DLLs:
- `Microsoft.CodeAnalysis.dll` (~3.2 MB)
- `Microsoft.CodeAnalysis.CSharp.dll` (~1.8 MB)
- `System.Collections.Immutable.dll` (~500 KB) — already bundled in `Analyzers/`

### Unity Editor Considerations

Unity's Mono runtime supports .NET Standard 2.0 / .NET Framework 4.x assemblies.
Roslyn 4.3.1 targets netstandard2.0. The DLLs should load without issues.

**Loading:** Use `Assembly.LoadFrom()` with explicit path to avoid conflicts with Unity's
own Roslyn usage (Unity uses Roslyn internally for compilation but may use a different version).

**Isolation:** Place in `Editor/HMR/Plugins~/` (the tilde suffix hides the folder from Unity's
asset pipeline). Load explicitly at runtime when HMR starts, not via Unity's auto-loading.

---

## 13. Testing Strategy

| Test | Type | Description | Not Started | Completed | Tested |
|------|------|-------------|:-----------:|:---------:|:------:|
| Parse → Emit roundtrip | Unit | Verify CSharpEmitter output compiles via Roslyn | ⬜ | | |
| MetadataReference resolution | Unit | Verify all required assemblies resolve | ⬜ | | |
| Delegate extraction | Unit | Verify `[UitkxElement]` type discovery and delegate creation | ⬜ | | |
| Tree walking | Integration | Mount a Fiber tree, swap delegate, verify re-render | ⬜ | | |
| State preservation | Integration | Counter component: swap label, verify count preserved | ⬜ | | |
| Hook mismatch recovery | Integration | Add a hook, verify state reset + re-mount | ⬜ | | |
| Assembly reload suppression | Integration | Start HMR, save .cs file, verify no domain reload | ⬜ | | |
| Auto-stop lifecycle | Integration | Enter play mode, verify HMR stops and assemblies unlock | ⬜ | | |
| Companion file inclusion | Integration | Modify companion .cs, verify included in compilation | ⬜ | | |

---

## 14. Known Limitations

| Limitation | Impact | Mitigation |
|------------|--------|------------|
| Cannot unload old assemblies (Mono) | ~10-30 KB leak per swap | Acceptable; cleared on domain reload |
| Roslyn DLLs add ~5 MB to package | Package size increase | Only in Editor folder, stripped from builds |
| All compilation suppressed during HMR | Other code changes don't take effect | Clear UX warning; Stop button |
| New components not immediately usable | Must be referenced by an HMR-swapped parent | Expected; will work after domain reload |
| Global state changes (static fields) in @code not reflected | Statics live on old assembly types | Rare edge case; documented |
| PropsResolver fallback map may miss custom elements | Custom host elements not in hardcoded map | Can extend map or use runtime resolution |

---

## 15. File Manifest

### New Files

| Path | Purpose | Not Started | Completed | Tested |
|------|---------|:-----------:|:---------:|:------:|
| `Editor/HMR/UitkxHmrWindow.cs` | Editor window with Start/Stop UI | | ✅ | |
| `Editor/HMR/UitkxHmrController.cs` | Orchestrator for HMR lifecycle | | ✅ | |
| `Editor/HMR/UitkxHmrFileWatcher.cs` | FileSystemWatcher wrapper with debounce | | ✅ | |
| `Editor/HMR/UitkxHmrCompiler.cs` | Parse → emit → compile → load | | ✅ | |
| `Editor/HMR/HmrCSharpEmitter.cs` | Custom C# emitter for UITKX AST | | ✅ | |
| `Editor/HMR/UitkxHmrDelegateSwapper.cs` | Fiber tree walking, delegate swap, re-render trigger | | ✅ | |
| `Editor/HMR/AssemblyReloadSuppressor.cs` | LockReloadAssemblies/DisallowAutoRefresh lifecycle | | ✅ | |
| `Shared/HmrState.cs` | Static bool `IsActive` (read by reconciler) | | ✅ | |
| `Shared/AssemblyInfo.cs` | `InternalsVisibleTo("ReactiveUITK.Editor")` | | ✅ | |
| `Runtime/AssemblyInfo.cs` | `InternalsVisibleTo("ReactiveUITK.Editor")` | | ✅ | |

### Modified Files

| Path | Change | Not Started | Completed | Tested |
|------|--------|:-----------:|:---------:|:------:|
| `Shared/Core/Fiber/FiberRenderer.cs` | Add `internal FiberRoot Root => _root;` | | ✅ | |
| `Shared/Core/VNodeHostRenderer.cs` | Add `internal FiberRenderer FiberRendererInternal => fiberRenderer;` | | ✅ | |
| `Runtime/Core/RootRenderer.cs` | Add `internal VNodeHostRenderer VNodeHostRendererInternal => vnodeHostRenderer;` | | ✅ | |
| `Editor/EditorRootRendererUtility.cs` | Add `internal static IEnumerable<VNodeHostRenderer> GetAllRenderers()` | | ✅ | |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Add HMR-aware `CanReuseFiber` fallback | | ✅ | |

---

## 16. Open Questions

1. **Roslyn version:** Should we match SourceGenerator's v4.3.1 or use a newer version?
   Newer versions have better perf but may have Mono compatibility issues.

2. **User assemblies in @code:** If a component's `@code` block references game-specific
   types (e.g., `GameManager.Instance`), the HMR compilation needs those assemblies.
   The current plan uses `AppDomain.CurrentDomain.GetAssemblies()` to include everything,
   but this may cause version conflicts. Need to test.

3. **Source generator cache interaction:** When HMR is stopped and domain reload occurs,
   the source generator may regenerate `.g.cs` files that conflict with HMR-compiled
   versions. Since HMR assemblies are in-memory only (no files on disk), there should be
   no conflict — the source generator produces the authoritative `.g.cs` files.

4. **Multiple UITKX packages:** If multiple packages in the project use ReactiveUITK,
   do we need to watch all of them? Likely yes — `FileSystemWatcher` on `Assets/` recursive
   handles this.
