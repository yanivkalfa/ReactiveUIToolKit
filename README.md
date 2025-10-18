# ReactiveUIToolKit (Preview)

Lightweight React-inspired component model for Unity UI Toolkit.
Supports class components (`ReactiveComponent`) and function components (via `V.Func`).
Provides hooks (state, reducer, memo, effect, layoutEffect, context, deferred value, imperative handle), portals, suspense, snapshots, diff metrics, and a nested style prop model.

## 1. Import / Folder Placement
Copy the `Assets/ReactiveUIToolKit` folder into your Unity project `Assets` directory.
Requires Unity with UI Toolkit (2021.2+ recommended). Target framework: .NET 4.x (Project settings -> Api Compatibility Level: .NET 4.x).

## 2. Setup Root Renderer
Create an empty GameObject in your scene (e.g. `ReactiveUIRoot`). Add the `RootRenderer` component.
You need access to a UI Toolkit `VisualElement` root (usually from a `UIDocument`). Example bootstrap script:

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core; // for RootRenderer
using ReactiveUITK.Examples.ClassComponents; // or your own components

public class ReactiveUIBootstrap : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument; // assign in Inspector

    private void Awake()
    {
        if (uiDocument == null)
        {
            Debug.LogError("Assign UIDocument reference");
            return;
        }
        // Ensure RootRenderer exists
        var rootRenderer = FindObjectOfType<RootRenderer>();
        if (rootRenderer == null)
        {
            rootRenderer = new GameObject("ReactiveUIRoot").AddComponent<RootRenderer>();
        }
        // Initialize root (panel's rootVisualElement)
        rootRenderer.Initialize(uiDocument.rootVisualElement);

        // Render a class component
        rootRenderer.Render<CounterComponent>();

        // OR render a function component:
        // var vnode = V.Func(CounterFunc.Render);
        // (Wrap it in a simple component if you want re-render triggers via state).
    }
}
```

## 3. Authoring Class Components
Create a C# script inheriting `ReactiveComponent`.
Override `Render()` and return a `VirtualNode` tree built with the `V` helpers.

```csharp
using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK; // base component
using ReactiveUITK.Core; // V helpers

public sealed class MyPanel : ReactiveComponent
{
    private int clicks;
    protected override VirtualNode Render()
    {
        var buttonProps = new Dictionary<string, object>
        {
            {"style", new Dictionary<string, object>{{"width",140f},{"height",30f},{"marginTop",6f}}},
            {"onClick", (System.Action)(() => SetState(ref clicks, clicks + 1)) }
        };
        return V.VisualElement(
            new Dictionary<string, object>{{"style", new Dictionary<string, object>{{"padding",10f},{"backgroundColor", Color.gray}}}},
            null,
            V.Text($"Clicks: {clicks}"),
            V.VisualElement(buttonProps, null, V.Text("Add"))
        );
    }
}
```
Attach your component script to a GameObject is NOT required; the reconciler creates an internal host `VisualElement` when rendered by `RootRenderer`. Rendering happens via `rootRenderer.Render<MyPanel>();`.

## 4. Function Components
Use `V.Func(rendererFunc, props?, key?, memo?)`.
`rendererFunc` signature: `VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)`.
Inside function components use hooks from `ReactiveUITK.Core.Hooks` (e.g. `Hooks.UseState`, `Hooks.UseMemo`). Example:

```csharp
public static class HelloFunc
{
    public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
    {
        var (count, setCount) = Hooks.UseState(0);
        var btn = new Dictionary<string, object> {
            {"style", new Dictionary<string, object>{{"width",120f},{"height",28f},{"marginTop",8f}}},
            {"onClick", (System.Action)(() => setCount(count + 1))}
        };
        return V.VisualElement(new Dictionary<string, object>{{"style", new Dictionary<string, object>{{"padding",8f}}}}, null,
            V.Text("Hello (Func) count=" + count),
            V.VisualElement(btn, null, V.Text("Increment"))
        );
    }
}
```
To mount a function component, wrap it in a root `V.VisualElement` or create a small bootstrap class component that returns `V.Func(HelloFunc.Render)`.

## 5. Styles
Styles use a single nested dictionary prop with key `"style"`:
```csharp
new Dictionary<string, object> {
  {"style", new Dictionary<string, object> {
      {"width",160f},
      {"height",30f},
      {"backgroundColor", Color.black},
      {"marginTop",8f}
  }}
}
```
Supported style keys (Unity 6.2+): width, height, opacity, flexGrow, flexShrink, flexDirection, justifyContent, alignItems, alignSelf, alignContent, fontSize, unityFontStyle, textAlign, position, left, top, right, bottom, flexWrap, flexBasis, minWidth, minHeight, maxWidth, maxHeight, display, visibility, overflow, whiteSpace, backgroundImage, backgroundImageTint (tint color), borderWidth, borderColor, borderLeftWidth/Right/Top/Bottom, borderLeftColor/Right/Top/Bottom, color, backgroundColor, rotate (degrees), scale (float or [x,y]), translate (float x or [x,y]), fontFamily, borderRadius, borderTopLeftRadius/TopRight/BottomLeft/BottomRight, margin, padding, marginLeft/Right/Top/Bottom, paddingLeft/Right/Top/Bottom, letterSpacing, textOverflow, unityTextOutlineColor, unityTextOutlineWidth, unityTextOverflowPosition, wordSpacing, unityOverflowClipBox, unityParagraphSpacing, unitySliceBottom/Top/Left/Right/Scale/Type.

Removed legacy/preview-only keys: rotateDeg, scaleX, scaleY, translateX, translateY, unityBackgroundScaleMode (deprecated in Unity 6.2+; use background-size in USS for stylesheet-based scaling).

Unity Version Support: This package targets Unity 6.2+ (UI Toolkit). Earlier versions may not expose some style fields or may emit obsolete warnings for deprecated ones.

## 6. Events
Use `onClick`, `onPointerDown`, `onPointerUp`, `onPointerMove`, `onPointerEnter`, `onPointerLeave`, `onWheel`, `onFocus`, `onBlur`, `onKeyDown`, `onKeyUp`, `onChange`, `onInput`, `onDragEnter`, `onDragLeave`, `onScroll`.
Delegates may be `Action` or handlers with one parameter (UI Toolkit event).

## 7. Hooks (Function Components)
- State: `Hooks.UseState<T>(initial)`
- Reducer: `Hooks.UseReducer<TState,TAction>(reducer, initial)`
- Memo: `Hooks.UseMemo(() => value, deps...)`
- Callback: `Hooks.UseCallback(fn, deps...)`
- Stable closure identity: `Hooks.UseStableCallback(Action)` / `Hooks.UseStableAction<T>(Action<T>)`
- Layout effects: `Hooks.UseLayoutEffect(() => cleanup?, deps...)` (runs synchronously after render diff)
- Passive effects: `Hooks.UseEffect(() => cleanup?, deps...)` (batched)
- Context: `Hooks.UseContext<T>(key)`
- Deferred value: `Hooks.UseDeferredValue(value, deps...)`
- Imperative handle: `Hooks.UseImperativeHandle(() => handleObj, deps...)`

## 8. Context
Class components: `ProvideContext("themeColor", Color.cyan);` and consumers call `ConsumeContext<Color>("themeColor")`.
Function components: `Hooks.UseContext<Color>("themeColor")`.

## 9. Portals & Suspense
- Portal: `V.Portal(targetElement, key, childNodes...)` renders children into another `VisualElement` while keeping a placeholder in parent ordering.
- Suspense: `V.Suspense(() => readyBool, fallbackNode, key, children...)`.

## 10. For Testing
Use `SnapshotAssert.AssertEqual(expectedVNode, actualVNode)` to compare virtual trees.
Metrics: `Reconciler.GetMetrics()` / `PropsApplier.GetStyleMetrics()` for performance insight.

## 11. Updating State
Class: `SetState(() => { /* mutate fields */ });` or `SetState(ref field, newValue);` / functional updater.
Function: capture setters from `Hooks.UseState` / dispatch from `Hooks.UseReducer`.

## 12. Unmount
Call `RootRenderer.Unmount()` or destroy the `RootRenderer` GameObject to clean up.

## 13. Notes
- Styles are diffed & reset automatically when removed.
- Always replace the style dictionary (new instance) instead of mutating in place for memoization.
- This is an early preview; API may change.

## 14. Minimal Flow Summary
1. Add `UIDocument` to scene.
2. Add / create `RootRenderer` GameObject.
3. Initialize with `rootRenderer.Initialize(uidoc.rootVisualElement)`.
4. Call `rootRenderer.Render<YourComponent>();`.
5. Your component's `Render()` builds a `VirtualNode` tree using `V.*` helpers (e.g. `V.VisualElement`, `V.Text`, `V.Func`).

Enjoy building reactive UI in Unity.
