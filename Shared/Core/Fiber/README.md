# React Fiber Reconciler for ReactiveUITK

This is a complete React Fiber-style reconciler implementation for ReactiveUITK, enabling true headless components and React-like behavior.

## Features

✅ **True Headless Components** - Function components render without wrapper elements  
✅ **React Fiber Architecture** - Double buffering, work loop, effects system  
✅ **Full Reconciliation** - Keyed and index-based child diffing  
✅ **Hooks Support** - useState, useEffect, useLayoutEffect  
✅ **Fragments** - Render multiple children without wrapper  
✅ **Effects** - Layout effects (synchronous) and passive effects (async)  
✅ **ElementAdapter Integration** - Works with existing element system  

## How to Use

### Enable Fiber Reconciler

```csharp
using ReactiveUITK.Core.Fiber;

// In your initialization code:
FiberConfig.UseFiberReconciler = true;
```

Or use the Unity menu: **ReactiveUITK → Use Fiber Reconciler**

### Example: Headless Function Component

```csharp
// This renders just a Button - no wrapper element!
var counter = V.Func("Counter", (props, children) => 
{
    var (count, setCount) = Hooks.UseState(0);
    
    return V.Button(
        $"Count: {count}", 
        onClick: () => setCount(count + 1)
    );
});
```

### Example: Fragment

```csharp
// Render multiple children without wrapper
var items = V.Fragment(
    V.Label("Item 1"),
    V.Label("Item 2"),
    V.Label("Item 3")
);
```

## Architecture

### Core Files

- **FiberNode.cs** - Fiber node structure (React Fiber node)
- **FiberRoot.cs** - Root container with double buffering
- **FiberReconciler.cs** - Work loop, BeginWork, CompleteWork, Commit
- **FiberChildReconciliation.cs** - Diffing algorithm (keyed & index-based)
- **FiberFunctionComponent.cs** - Function component rendering & hooks
- **FiberHostConfig.cs** - Platform operations (ElementAdapter integration)
- **FiberFragment.cs** - Fragment support
- **FiberRenderer.cs** - Public API
- **FiberConfig.cs** - Configuration and toggles

### Reconciliation Flow

```
1. RENDER PHASE (Interruptible - not yet)
   ├─ BeginWork: Process fiber, reconcile children
   ├─ CompleteWork: Create/update elements, collect effects
   └─ Build effect list

2. COMMIT PHASE (Synchronous)
   ├─ Process deletions
   ├─ Process placements (insert elements)
   ├─ Process updates (apply props)
   ├─ Run layout effects (useLayoutEffect)
   └─ Schedule passive effects (useEffect)

3. SWAP TREES
   └─ current ↔ work-in-progress
```

### Key Differences from Legacy Reconciler

| Feature | Legacy Reconciler | Fiber Reconciler |
|---------|------------------|------------------|
| Function Components | Wrapper element | No wrapper (headless) |
| Tree Structure | Single tree | Double buffering (current + WIP) |
| Reconciliation | Direct DOM manipulation | Effect list → batch commit |
| Component State | NodeMetadata.userData | FiberNode.ComponentState |
| Updates | Immediate | Collected, then committed |

## Testing

### Run Tests

Unity Menu: **ReactiveUITK → Run Fiber Tests**

Or programmatically:

```csharp
using ReactiveUITK.Core.Fiber;

var container = new VisualElement();
FiberTest.RunBasicTest(container);
FiberTest.RunCounterTest(container);
```

### Toggle Reconcilers

```csharp
// Enable Fiber
FiberConfig.UseFiberReconciler = true;

// Disable (use legacy)
FiberConfig.UseFiberReconciler = false;

// Enable logging
FiberConfig.EnableFiberLogging = true;
FiberConfig.ShowReconcilerInfo = true;
```

## Implementation Status

### ✅ Completed (Phases 1-4)

- [x] Core infrastructure
- [x] Work loop (BeginWork, CompleteWork)
- [x] Child reconciliation (keyed & index-based)
- [x] Element operations (via ElementAdapters)
- [x] Function components
- [x] Hooks integration
- [x] Effects (layout & passive)
- [x] Fragments
- [x] Feature toggle

### 🚧 Future Enhancements

- [ ] Time-slicing (interruptible rendering)
- [ ] Suspense
- [ ] Portals
- [ ] Error boundaries
- [ ] Context API improvements
- [ ] Concurrent mode
- [ ] Profiler integration

## Performance

The Fiber reconciler uses:
- **Double buffering** - Prevents partial updates
- **Effect batching** - Minimizes DOM operations
- **Work units** - Prepared for time-slicing
- **Profiling markers** - Unity Profiler integration

## Migration Guide

### For Existing Code

No changes needed! Just enable Fiber:

```csharp
FiberConfig.UseFiberReconciler = true;
```

All existing `V.*` components work identically.

### For New Code

Take advantage of headless components:

```csharp
// OLD: Creates wrapper element
V.Func("MyComp", () => V.Button("Click"));
// Result: <FunctionComponentContainer><Button /></FunctionComponentContainer>

// NEW (Fiber): No wrapper
V.Func("MyComp", () => V.Button("Click"));  
// Result: <Button />
```

## Debugging

Enable detailed logging:

```csharp
FiberConfig.EnableFiberLogging = true;
FiberConfig.ShowReconcilerInfo = true;
```

Check Unity Console for:
- `[VNodeHostRenderer]` - Which reconciler is being used
- `[FiberTest]` - Test results
- `[Fiber.*]` - Fiber-specific logs

## Credits

Based on:
- [React Fiber Architecture](https://github.com/acdlite/react-fiber-architecture)
- [React Reconciliation](https://react.dev/learn/reconciliation)
- [Didact - Build Your Own React](https://pomb.us/build-your-own-react/)

## License

Part of ReactiveUITK. See main project license.
