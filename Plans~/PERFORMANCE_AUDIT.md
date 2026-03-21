# ReactiveUITK Performance Audit & Optimization Recommendations

> **Date:** March 2026  
> **Scope:** Read-only analysis — no code changes. All findings are safe to implement without affecting functionality.  
> **Methodology:** Full source audit of every hot-path file in the core runtime, Unity-specific host layer, and scheduling system. Competitors researched: ReactUnity, React (Facebook).

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [ReactiveUITK vs ReactUnity vs React — Architectural Comparison](#2-reactiveuitk-vs-reactunity-vs-react--architectural-comparison)
3. [Performance Findings — Tiered by Impact](#3-performance-findings--tiered-by-impact)
   - [P0: Critical — Immediate Wins](#p0-critical--immediate-wins)
   - [P1: High — Significant Gains](#p1-high--significant-gains)
   - [P2: Medium — Measurable Improvements](#p2-medium--measurable-improvements)
   - [P3: Low — Polish](#p3-low--polish)
4. [Unity-Specific Concerns](#4-unity-specific-concerns)
5. [What's Already Well Done](#5-whats-already-well-done)
6. [Recommended Implementation Order](#6-recommended-implementation-order)
7. [Benchmarking Strategy](#7-benchmarking-strategy)

---

## 1. Architecture Overview

ReactiveUITK is a **React Fiber-style reconciler** implemented entirely in C#, targeting Unity UI Toolkit (UITK). The data flow is:

```
User Code (V.* factories / UITKX source gen)
    → VirtualNode tree (immutable virtual DOM)
    → FiberReconciler (two-tree diff: current ↔ work-in-progress)
    → FiberNode linked-list tree (with hooks, state, effects, context)
    → FiberHostConfig → ElementRegistry → IElementAdapter → VisualElement tree
    → PropsApplier (style/event application to Unity VisualElements)
```

**Key subsystems:**

| Subsystem | Files | Hot Path? |
|-----------|-------|-----------|
| **VNode creation** | `V.cs`, `VNode.cs`, `IProps.cs` | Every render |
| **Fiber reconciler** | `FiberReconciler.cs`, `FiberChildReconciliation.cs`, `FiberFactory.cs` | Every render |
| **Function components** | `FiberFunctionComponent.cs`, `FunctionComponentState` (in `NodeMetadata.cs`) | Every component render |
| **Hooks** | `Hooks.cs` (~1900 lines) | Every hook call per render |
| **Props diffing** | `PropsApplier.cs` (~2700 lines), `BaseElementAdapter.cs` | Every commit |
| **Event dispatch** | `PropsApplier.InvokeHandler` | Every user interaction |
| **Scheduling** | `RenderScheduler.cs`, `EditorRenderScheduler.cs` | Every frame |
| **Signals** | `Signal.cs`, `SignalsRuntime.cs` | Every signal change |
| **Animation** | `Animator.cs`, `Easing.cs` | Every 16ms tick per animation |

---

## 2. ReactiveUITK vs ReactUnity vs React — Architectural Comparison

### How ReactUnity Works

[ReactUnity](https://reactunity.github.io/) embeds an actual JavaScript engine inside Unity:

1. **JavaScript runtime**: Uses one of three JS engines — **Jint** (pure C# interpreter, slow), **ClearScript** (V8 wrapper, fast but desktop-only), or **QuickJS** (lightweight C engine, used for WebGL). The engine selection is via `JavascriptEngineType` enum.

2. **React runs in JS**: The actual React library (`react`, `react-reconciler`, `@reactunity/renderer`) runs inside the JS engine. The reconciler is Facebook's `react-reconciler` package — the same one React DOM and React Native use.

3. **Bridge layer**: `ScriptContext.cs` creates DOM shims (`document`, `window`, `setTimeout`, `requestAnimationFrame`, `console`, `localStorage`, `WebSocket`, `XMLHttpRequest`, `URL`, `fetch`, `TextEncoder`) to make the JS environment behave like a browser. Every DOM operation crosses the JS↔C# interop boundary.

4. **Host config**: The `@reactunity/renderer` npm package implements `react-reconciler`'s HostConfig interface in TypeScript. Each `createInstance`, `appendChild`, `removeChild`, `commitUpdate` call goes through the JS→C# bridge to invoke methods on Unity's VisualElement (UIToolkit backend) or UGUI components.

5. **Development workflow**: Requires Node.js during development (`npm run start` → Webpack Dev Server with Fast Refresh). Production builds bundle JS into static files.

6. **Layout**: Uses **Yoga** (Facebook's Flexbox engine) compiled as native lib + managed C# wrapper, mirroring React Native's approach. This is a separate layout engine from UITK's built-in Yoga-based layout.

### Performance Comparison: ReactiveUITK vs ReactUnity

| Dimension | ReactiveUITK | ReactUnity | Winner |
|-----------|-------------|------------|--------|
| **Reconciler location** | C# (native to Unity runtime) | JavaScript (inside embedded engine) | **ReactiveUITK** — zero interop overhead |
| **Interop cost per DOM op** | Zero — direct VisualElement calls | JS→C# bridge per `createElement`, `appendChild`, `commitUpdate`, etc. Each call marshals types across boundary | **ReactiveUITK** — 10-100x less overhead per op |
| **Memory model** | Single GC (Mono/IL2CPP) | Dual GC (JS engine GC + C# GC), plus marshaling objects between heaps | **ReactiveUITK** — simpler, less memory churn |
| **Startup time** | Near-zero (C# compiles with project) | Must boot JS engine + parse/compile React bundle + execute initialization | **ReactiveUITK** — no JS engine boot |
| **Bundle size** | Zero extra (C# assembly) | React + ReactDOM-equivalent + polyfills + user code JS bundle | **ReactiveUITK** — no JS payload |
| **Hot reload** | HMR via UITKX source gen + editor watcher | Webpack Fast Refresh (native React HMR) | **ReactUnity** — more mature JS HMR ecosystem |
| **Reconciler maturity** | Custom implementation (faithful port but newer) | Facebook's `react-reconciler` (battle-tested at scale) | **ReactUnity** — uses official React code |
| **React compatibility** | API-compatible hooks/patterns, not actual React | Actual React library + full npm ecosystem | **ReactUnity** — real React, real npm packages |
| **IL2CPP / AOT** | Fully compatible (C# only) | Requires JS engine compatible with target platform; QuickJS for WebGL, ClearScript excluded | **ReactiveUITK** — universal platform support |
| **CSS support** | Inline styles via style dictionaries, USS classes | Full CSS subset parser + Yoga layout + computed styles | **ReactUnity** — richer CSS |
| **Typing** | C# strong typing + source-gen props | TypeScript (optional) | ~Tie |
| **Layout engine** | UITK's built-in Yoga (no extra engine) | Bundled Yoga native lib (redundant on UITK backend) | **ReactiveUITK** — leverages UITK's native layout |

### The JS Bridge Tax — Quantified

Every ReactUnity operation that modifies the Unity UI tree must:

1. **Serialize** the call in JS (function name + arguments)
2. **Marshal** through the engine's interop layer (Jint: C# reflection on JS objects; ClearScript: V8→COM→.NET; QuickJS: P/Invoke + manual marshaling)
3. **Execute** the C# method on the Unity side
4. **Return** result back through the same bridge

For a typical re-render of 50 elements with 5 props each:
- ReactUnity: ~250 bridge calls (50 commitUpdate × 5 property sets), each with serialization overhead
- ReactiveUITK: ~250 direct C# method calls — effectively free by comparison

The bridge cost is **especially** impactful for:
- **Frequent updates**: Animations, dragging, scrolling — anything that changes many props per frame
- **Large lists**: ListView with 100+ rows binding/unbinding as user scrolls
- **Complex UIs**: Deep component trees with many elements that all update

### How React (Web) Handles Performance

React's reconciler (used by both React DOM and ReactUnity) has these key performance properties:

| Aspect | React (Web) | ReactiveUITK | Parity? |
|--------|-------------|-------------|---------|
| **Fiber double-buffer** | `cloneFiber` reuses alternate lazily | `CreateWorkInProgress` reuses alternate at root, but child fibers always `new FiberNode` | ❌ Partial — React reuses alternates deeper |
| **Work loop time-slicing** | `shouldYield()` checks via `requestIdleCallback` / `MessageChannel`, 5ms slices | `ProcessWorkUntilDeadline` with `Stopwatch`, 2ms budget | ✅ Similar — ReactiveUITK is even more conservative |
| **Bailout optimization** | `pendingProps === memoizedProps` (reference equality) + `shouldComponentUpdate` / `React.memo` | `ReferenceEquals(tp, cp) \|\| tp.Equals(cp)` for typed props | ✅ Equivalent — ReactiveUITK also supports structural equality |
| **Effect list (linked)** | `NextEffect` linked list, only dirty fibers | Same `NextEffect` linked list pattern | ✅ Same |
| **hooks as linked list** | `memoizedState.next` linked list on fiber | `HookStates` as `List<object>` (array, not linked list) | ⚠️ ReactiveUITK uses array indexing (good for access speed, bad for boxing) |
| **Object reuse** | Fibers reused via `alternate`, arrays via `push`/`pop` | New `FiberNode` per child reconciliation, no pooling | ❌ React reuses more aggressively |
| **Lanes model** | Priority lanes (31-bit mask) with entanglement | Simple `Priority` enum (High/Normal/Low/Idle) | ❌ React has finer scheduling |
| **Auto-memoization** | React Compiler (React Forget) auto-inserts `useMemo`/`useCallback` | No compiler — manual hooks only | ❌ React is ahead here |
| **Commit tree walks** | Single pass merges flags/props/effects | 3 separate full-tree walks in `CommitRoot` | ❌ ReactiveUITK does 3x the work |
| **State batching** | Automatic in React 18 (microtask queue) | `BeginBatch`/`EndBatch` pattern + deferred queue during commit | ✅ Similar mechanism |
| **Context propagation** | Depth-first scan, stops at consumers | `PropagateContextChange` — same algorithm | ✅ Same |
| **Suspense** | Promise-based, concurrent features | `FiberSuspenseSuspendException` control-flow | ✅ Same concept |

**Key React optimizations ReactiveUITK is missing:**

1. **Fiber alternate reuse for children**: React's `cloneFiber` checks `fiber.alternate` before allocating. ReactiveUITK only reuses the root WIP alternate but creates new FiberNode instances for every child during reconciliation.

2. **Single-pass commit**: React merges prop commits, flag clearing, and reference updates into one tree walk. ReactiveUITK does three (`CommitDeletions` + `UpdateComponentStateReferences` + `CommitPropsAndClearFlags`).

3. **No `List<object>` boxing**: React stores hook state as a linked list of typed nodes (each hook's state is a distinct JS object with no boxing). ReactiveUITK's `List<object>` boxes every value-type hook state.

---

## 3. Performance Findings — Tiered by Impact

### P0: Critical — Immediate Wins

These are the highest-impact optimizations that would produce measurable improvements in real-world UI rendering, especially for complex game UIs.

---

#### P0-1: `DynamicInvoke` in Event Handler Dispatch

**File:** `Shared/Props/PropsApplier.cs` (lines ~2307–2440)

**Problem:** `InvokeHandler()` uses `Delegate.DynamicInvoke()` as its primary dispatch path for all event handlers that aren't plain `Action`. `DynamicInvoke` is catastrophically slow — typically **50-100x slower** than a direct typed delegate invocation because it:

- Calls `MethodInfo.GetParameters()` (reflection) to inspect the delegate signature
- Allocates `object[]` for arguments
- Boxes all value-type arguments
- Uses runtime type checking for parameter assignment

Every `onClick`, `onChange`, `onPointerMove`, `onInput` callback goes through this path unless the user uses exactly `Action` (zero parameters).

**Additionally:** Before the DynamicInvoke, the code always calls `ReactiveEvent.Create(evt)` which allocates a synthetic event object for **every** event dispatch, even if the handler doesn't use it. And for `ChangeEvent<T>`, it does `evtType.GetProperty("newValue")?.GetValue(evtObj)` — full reflection on every change event.

**Impact:** This is the single most impactful performance issue for interactive UIs. A user dragging a slider fires `onPointerMove` 60+ times per second per element — each triggering DynamicInvoke + ReactiveEvent allocation + reflection.

**Fix:** Replace DynamicInvoke with typed dispatch. Pre-check the delegate type at registration time (or on first call) and store a typed wrapper:

```csharp
// Instead of DynamicInvoke, generate typed paths:
if (del is Action<ClickEvent> typedClick) { typedClick(evt as ClickEvent); return; }
if (del is Action<ChangeEvent<string>> typedStr) { typedStr(evt as ChangeEvent<string>); return; }
if (del is Action<string> strHandler) { strHandler(newValue as string); return; }
// ... cover the 10-15 common signatures, DynamicInvoke only as final fallback
```

For `ReactiveEvent.Create`: defer allocation until the handler actually requests the synthetic event (lazy creation).

---

#### P0-2: VNode Triple-Clone Allocation Pattern

**Files:** `Shared/Core/V.cs`, `Shared/Core/VNode.cs`

**Problem:** Every element factory call (`V.Button(props, children)`, `V.VisualElement(props, children)`, etc.) triggers a cascade of allocations:

| Step | Allocation | Object Created |
|------|-----------|---------------|
| 1. `props.ToDictionary()` | `Dictionary<string, object>` | Props as mutable dict |
| 2. `CloneStyleDictionary()` | `Dictionary<string, object>` | Style sub-dict clone (if "style" key exists) |
| 3. `VNode.CloneProps()` | `Dictionary<string, object>` + `ReadOnlyDictionary<>` wrapper | Defensive immutable copy |
| 4. `VNode.CloneChildren()` | `VirtualNode[]` + `ReadOnlyCollection<>` wrapper | Defensive immutable copy |
| 5. `params VirtualNode[]` | Compiler-generated array at call site | Every call with children |

**Total per element per render:** 3 dictionary allocations, 2 array allocations, 2 wrapper objects = **7 heap objects**.

For a UI tree of 200 elements, that's **~1,400 heap objects** just for the VNode layer — before any fiber work begins. This is the dominant source of GC pressure per render cycle.

**Comparison:** React (web) creates plain JS objects for elements (`{type, props, children}`) with zero defensive cloning. Props are passed by reference. React trusts the immutability contract.

**Fix options (progressively more impactful):**

1. **Trust immutability**: Remove `CloneProps`/`CloneChildren` from VNode constructor. Props returned by `ToDictionary()` should be treated as frozen. This alone eliminates 2 dictionary + 1 array + 2 wrapper allocations per element.

2. **Skip style cloning**: `CloneStyleDictionary` exists to prevent mutation of the original style. If callers never mutate after passing, this can be removed.

3. **Typed props path**: For UITKX-generated components, props are already strongly-typed `IProps` structs with source-gen equality. Skip the `Dictionary<string, object>` entirely for these — pass `IProps` directly through to the fiber.

4. **Params avoidance**: For common arities (0, 1, 2, 3 children), provide overloads instead of `params`:
   ```csharp
   public static VirtualNode Button(ButtonProps props) => ...;
   public static VirtualNode Button(ButtonProps props, VirtualNode child) => ...;
   public static VirtualNode Button(ButtonProps props, VirtualNode c1, VirtualNode c2) => ...;
   ```

---

#### P0-3: FiberNode — Large Class, No Object Pooling

**Files:** `Shared/Core/Fiber/FiberFactory.cs`, `Shared/Core/Fiber/FiberChildReconciliation.cs`, `Shared/Core/Fiber/FiberNode.cs`

**Problem:** `FiberNode` is a class with **30+ public fields** (Tag, ElementType, TypedRender, ComponentState, HostElement, Alternate, Parent, Child, Sibling, Props, PendingProps, TypedProps, TypedPendingProps, EffectTag, NextEffect, Children, Key, LayoutEffects, PassiveEffects, Deletions, ProvidedContext, ReadsContext, HasPendingStateUpdate, SubtreeHasUpdates, etc.).

Every `CreateFiber()`, `CloneFiber()`, and `CloneForReuse()` call allocates a **new** `FiberNode` instance. During reconciliation of a 200-element tree, this creates ~200 new FiberNode objects per render pass — each a sizable heap allocation (~300+ bytes estimated given field count and reference types).

**Comparison:** React reuses fibers via the `alternate` pointer. When React needs a work-in-progress fiber for a child, it first checks `current.alternate` — if it exists from a previous render, React reuses it (just resets its fields). Only on first mount does it allocate fresh. ReactiveUITK's `CreateWorkInProgress` does this for the root, but `CreateFiber`/`CloneFiber` in child reconciliation always allocate new.

**Fix:** Implement a `FiberNode` object pool:

```csharp
static class FiberNodePool
{
    private static readonly Stack<FiberNode> pool = new(128);
    
    public static FiberNode Rent()
    {
        return pool.Count > 0 ? pool.Pop() : new FiberNode();
    }
    
    public static void Return(FiberNode node)
    {
        node.Reset(); // Clear all fields
        pool.Push(node);
    }
}
```

Return nodes to the pool during `CommitDeletion` and when the old current tree's fibers are no longer needed. This is the single highest-impact structural optimization for GC pressure.

---

#### P0-4: Hook State Boxing (`List<object>`)

**File:** `Shared/Core/Hooks.cs` (pervasive)

**Problem:** All hook state is stored in `List<object>`:

```csharp
// In FunctionComponentState:
public List<object> HookStates;

// Every UseState<int> call:
state.HookStates.Add(initial);  // boxes int → object
var current = (int)state.HookStates[hookIdx]; // unboxes object → int
```

Every `UseState<int>()`, `UseState<bool>()`, `UseState<float>()`, or any value-type state causes:
- **Boxing** on every write (state update)
- **Unboxing** on every read (every render that calls the hook)

For a component with 5 value-type hooks rendering 60 times per second, that's 300 box operations + 300 unbox operations per second — just for one component.

**Comparison:** React stores hook state as a JavaScript linked list — JS has no boxing concept (all values are objects or primitives stored directly). The `List<object>` is a C#-specific penalty.

**Fix options:**

1. **Immediate**: Use `List<HookSlot>` where `HookSlot` is a union-like struct:
   ```csharp
   [StructLayout(LayoutKind.Explicit)]
   struct HookSlot
   {
       [FieldOffset(0)] public HookSlotKind Kind;
       [FieldOffset(4)] public int IntValue;
       [FieldOffset(4)] public float FloatValue;
       [FieldOffset(4)] public bool BoolValue;
       [FieldOffset(8)] public object ObjectValue; // for reference types
   }
   ```

2. **Longer-term**: Per-component typed hook arrays generated by the UITKX source generator, eliminating boxing entirely for known hook signatures.

---

#### P0-5: `previousStyles` Static Dictionary — Memory Leak

**File:** `Shared/Props/PropsApplier.cs` (line ~16)

**Problem:**
```csharp
private static readonly Dictionary<VisualElement, Dictionary<string, object>> previousStyles = new();
```

This static dictionary maps every `VisualElement` that ever had styles applied to its previous style values. Since `VisualElement` is a reference type held by strong reference, **destroyed elements remain rooted in memory indefinitely**. Elements are added at line ~1190 but only removed at line ~2690 (`CleanUpElement`).

Any scenario with dynamic element creation/destruction — navigation between pages, list view scrolling, modals opening/closing — causes unbounded memory growth.

**Fix:** Either:
- Use `ConditionalWeakTable<VisualElement, Dictionary<string, object>>` (allows GC to collect entries when the VisualElement is collected)
- Ensure `CleanUpElement` is called reliably for every element that goes through `ApplyStyle`

---

### P1: High — Significant Gains

---

#### P1-1: Commit Phase — 3 Full-Tree Walks

**File:** `Shared/Core/Fiber/FiberReconciler.cs` (lines ~607–680)

**Problem:** Every `CommitRoot()` performs three **separate** full tree traversals:

1. `CommitDeletions(root)` — walks entire tree looking for `Deletions` lists
2. `UpdateComponentStateReferences(root)` — walks entire tree updating `ComponentState.Fiber` 
3. `CommitPropsAndClearFlags(root)` — walks entire tree committing props and clearing flags

Plus the effect-list walk (which is efficient — only dirty fibers). For a 500-node tree, that's 1,500+ node visits per commit, where React does it in one pass.

**Fix:** Merge all three into a single recursive walk:

```csharp
private void CommitTreeWalk(FiberNode fiber)
{
    if (fiber == null) return;
    
    // 1. Process deletions
    if (fiber.Deletions != null) { /* commit deletions */ }
    
    // 2. Update component state reference
    if (fiber.ComponentState != null) fiber.ComponentState.Fiber = fiber;
    
    // 3. Commit props and clear flags
    fiber.Props = fiber.PendingProps;
    if (fiber.TypedPendingProps != null) fiber.TypedProps = fiber.TypedPendingProps;
    fiber.SubtreeHasUpdates = false;
    
    // Recurse
    var child = fiber.Child;
    while (child != null) { CommitTreeWalk(child); child = child.Sibling; }
}
```

---

#### P1-2: `OnStateUpdated` Closure Per Render

**File:** `Shared/Core/Fiber/Components/FiberFunctionComponent.cs` (line ~57)

**Problem:**
```csharp
componentState.OnStateUpdated = () => reconciler.ScheduleUpdateOnFiber(componentState.Fiber, null);
```

This creates a **new closure** (capturing `reconciler` and `componentState`) on **every render** of every function component. Since `componentState` is stable across renders and `reconciler` is constant for the lifetime of the root, this delegate never needs re-creation.

**Fix:** Cache the delegate on `FunctionComponentState`:

```csharp
if (componentState.OnStateUpdated == null)
{
    componentState.OnStateUpdated = () => reconciler.ScheduleUpdateOnFiber(componentState.Fiber, null);
}
```

Since `componentState.Fiber` is updated by reference (via `UpdateComponentStateReferences`), the captured closure remains valid.

---

#### P1-3: Signal.Set() Array Allocation

**File:** `Shared/Core/Signals/Signal.cs` (line ~88)

**Problem:**
```csharp
snapshot = listeners.ToArray(); // allocates new T[] every Set()
```

Every signal value change with active subscribers allocates a new array for snapshot-style iteration. For signals driving animations or real-time data (e.g., health bars, score counters), this fires many times per second.

**Fix:** Use `ArrayPool<T>`:
```csharp
var count = listeners.Count;
var snapshot = ArrayPool<Action<T>>.Shared.Rent(count);
listeners.CopyTo(snapshot, 0);
// ... iterate snapshot[0..count] ...
ArrayPool<Action<T>>.Shared.Return(snapshot, clearArray: true);
```

---

#### P1-4: Unconditional Debug Logging in PropsApplier

**File:** `Shared/Props/PropsApplier.cs` (lines ~2005–2012, ~2238–2244)

**Problem:**
```csharp
Debug.Log(
    "[RemoveEvent] begin eventPropName=" + eventPropName
    + ", element=" + element.name
    + ", parent=" + (element.parent != null ? element.parent.name : "<null>")
    + ", handlerType=" + handler.GetType().Name);
```

This `Debug.Log` with string concatenation runs on **every event unregistration** regardless of build config. `Debug.Log` in Unity is expensive: it formats the call stack, writes to the console, and writes to the editor log file. The string concatenation allocates multiple temporary strings.

**Fix:** Guard with `#if UNITY_EDITOR && REACTIVE_UITK_VERBOSE` or behind `FiberConfig.EnableFiberLogging`.

---

#### P1-5: `className` Handling — HashSet + Split Allocation Per Diff

**File:** `Shared/Props/PropsApplier.cs` (lines ~1005–1050)

**Problem:** On every diff pass where CSS classes change:
```csharp
var oldSet = new HashSet<string>(
    (oldClasses ?? string.Empty).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
var newSet = new HashSet<string>(
    (newClasses ?? string.Empty).Split(new[] { ' ', '\t', '\n', '\r' }, ...));
```

This allocates:
- `new char[]` for the separator array (should be `static readonly`)
- Two `string[]` from `Split`
- Two `HashSet<string>` objects

For the common case of a single class name changing, this is massive overkill.

**Fix:** 
- Cache the separator array as `static readonly`
- For the common case (no whitespace in class strings), use `string.Equals` directly
- Only construct HashSets when both old and new contain spaces

---

#### P1-6: Reflection in ListView `DeriveRowKey` Per Row Bind

**File:** `Shared/Elements/ListViewElementAdapter.cs` (lines ~295–325)

**Problem:**
```csharp
var t = item.GetType();
var f = t.GetField("Id", BindingFlags.Instance | BindingFlags.Public);
var p = t.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
```

This runs `GetField()` and `GetProperty()` on **every `bindItem` callback** — every list row that scrolls into view. For a list with 50 visible rows scrolling at 60fps, that's potentially hundreds of reflection calls per frame.

**Fix:** Cache the `FieldInfo`/`PropertyInfo` per `Type` in a `static Dictionary<Type, MemberInfo>`.

---

#### P1-7: O(N) Pool Scan in ListView `unbindItem`

**File:** `Shared/Elements/ListViewElementAdapter.cs` (lines ~155–168)

**Problem:**
```csharp
listView.unbindItem = (ve, i) => {
    foreach (var kv in parts.Pool) // iterates ENTIRE pool
    {
        var mount = kv.Value.mount;
        if (mount != null && mount.parent == ve)
            mount.RemoveFromHierarchy();
    }
};
```

This iterates the **entire pool** to find one matching mount — O(N) per row unbind. With 1000 items and 50 visible rows, this scans up to 50,000 entries during a single scroll gesture.

**Fix:** Maintain a reverse index `Dictionary<VisualElement, string>` mapping parent→pool key for O(1) lookup.

---

### P2: Medium — Measurable Improvements

---

#### P2-1: `TryDiffProp` Empty Dictionary Allocation

**File:** `Runtime/Core/Adapters/BaseElementAdapter.cs` (lines ~68–69) or `Shared/Elements/BaseElementAdapter.cs`

**Problem:**
```csharp
previous ??= new Dictionary<string, object>();
next ??= new Dictionary<string, object>();
```

Creates up to 2 empty dictionaries **per property diff per element per render** when either previous or next props are null.

**Fix:** Use static singletons:
```csharp
private static readonly IReadOnlyDictionary<string, object> EmptyProps = 
    new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
```

---

#### P2-2: `MapRemainingChildren` Dictionary + `int.ToString()` Per Unkeyed Child

**File:** `Shared/Core/Fiber/FiberChildReconciliation.cs` (lines ~309, ~316)

**Problem:** Keyed reconciliation allocates a `new Dictionary<string, FiberNode>()` per pass, and `index.ToString()` allocates a string for every unkeyed child in the keyed path.

For a list of 50 items without explicit keys, that's 50 `int.ToString()` string allocations + 1 dictionary per reconciliation.

**Fix:** Pool the dictionary (clear + reuse across renders), and use a pre-cached string array for small indices (`"0"`, `"1"`, ..., `"99"`).

---

#### P2-3: `UseSignal` Boxing Wrappers Per Render

**File:** `Shared/Core/Hooks.cs` (lines ~1530–1533)

**Problem:**
```csharp
new Func<object, object>(raw => selector((T)raw))
new BoxedEqualityComparer<TSlice>(...)
```

Creates 2 delegate/object allocations per `UseSignal` call per render, even for identity selectors.

**Fix:** Cache these per `(signalKey, hookIndex)` on the component state. For the common identity selector, use a static singleton.

---

#### P2-4: `UseReducer` Dispatch Closure Per Render

**File:** `Shared/Core/Hooks.cs` (lines ~893–910)

**Problem:** Unlike `UseState` (which has the efficient `StateSetterHandle<T>` struct with delegate caching), `UseReducer` creates a new `Dispatch` delegate on every render.

**Fix:** Apply the same `StateSetterHandle`-style caching pattern to `UseReducer`'s dispatch function.

---

#### P2-5: `HasKeys()` Double-Traversal

**File:** `Shared/Core/Fiber/FiberChildReconciliation.cs` (lines ~325–332)

**Problem:** `HasKeys()` iterates all children to check if any have keys — O(n). Then the actual reconciliation iterates all children again — another O(n). This is 2x the work.

**Fix:** Merge into a single pass: start with the index-based path, switch to keyed path on first encountered key.

---

#### P2-6: Unconditional Reflection for Debug Names

**Files:** `FiberFunctionComponent.cs` (~L50), `FiberFactory.cs` (~L167), `FiberReconciler.cs` (~L1361)

**Problem:**
```csharp
var name = fiber.TypedRender?.Method.DeclaringType?.Name ?? "Unknown";
```

Computed on every render/clone/commit even when the result is never used (assigned to a local and discarded, or only used in guarded log statements).

**Fix:** Remove unconditional computation. Guard with `FiberConfig.EnableFiberLogging` or lazily compute only when needed.

---

#### P2-7: `Canonicalize` / `ToCamelCase` String Allocations

**File:** `Shared/Props/PropsApplier.cs` (lines ~853–873)

**Problem:** `Canonicalize(styleKey)` is called for every style property application. For USS-style keys with hyphens (e.g., `flex-direction`, `border-radius`), `ToCamelCase()` allocates via `string.Split()`, `char.ToUpperInvariant()`, `string.Join()`, and `Substring()`.

**Fix:** Build a static `Dictionary<string, string>` cache of canonicalized keys. Most codebases use a fixed set of ~60 style properties.

---

#### P2-8: Animator Boxing in `Lerp`

**File:** `Shared/Core/Animation/Animator.cs` (lines ~232–241)

**Problem:**
```csharp
private static object Lerp(object from, object to, float t)
{
    if (from is float ff && to is float tf)
        return Mathf.Lerp(ff, tf, t);  // boxes float result
    if (from is Color fc && to is Color tc)
        return Color.Lerp(fc, tc, t);  // boxes Color result
}
```

The `float` and `Color` return values are **boxed** on every animation tick (every 16ms per active animation). The `Apply()` method then unboxes them.

**Fix:** Generic or typed paths:
```csharp
private static float LerpFloat(float from, float to, float t) => Mathf.Lerp(from, to, t);
private static Color LerpColor(Color from, Color to, float t) => Color.Lerp(from, to, t);
```

---

### P3: Low — Polish

| Issue | File | Description | Fix |
|-------|------|-------------|-----|
| Effect deps `Clone()` per dirty effect | `FiberFunctionComponent.cs` | `(object[])effect.deps?.Clone()` allocates per effect | Use `ArrayPool` or compare in-place |
| `ExtractProps` empty dict fallback | `FiberChildReconciliation.cs`, `FiberFunctionComponent.cs` | `new Dictionary<string, object>()` for null/Suspense fibers | Static readonly empty dict singleton |
| `PendingStateUpdate.From<T>` bridge lambda | `Hooks.cs` (~L100-106) | Closure allocation for updater-style state updates | Cache per updater type |
| `ContextDependencies` appends per render | `Hooks.cs` (~L1508) | `new ContextDependency(...)` per `UseContext` call per render | Reuse list entries by index |
| `SubscribeRaw` wrapper lambda | `Signal.cs` (~L72) | `Action<T> wrapper = v => listener(v)` per subscription | Cache per listener |
| `ComputeHandlerSignature` string concat | `PropsApplier.cs` (~L1582) | `owner + "::" + name` per event registration | Guard behind diagnostics flag |
| `ScrollViewAdapter.ToLowerInvariant()` | `ScrollViewElementAdapter.cs` (~L72) | String allocation per mode prop | Use `StringComparison.OrdinalIgnoreCase` |
| `TextField` reflection for `isPasswordField` | `TextFieldElementAdapter.cs` (~L215) | `GetProperty` per diff with password prop | Cache in static field |
| EditorRenderScheduler no dedup | `EditorRenderScheduler.cs` | Same action can be enqueued multiple times | Add `HashSet<Action>` tracking (as runtime scheduler has) |
| `RenderScheduler.EndBatch` lambda closures | `RenderScheduler.cs` (~L56, L108) | `() => Enqueue(action, priority)` + `.ToArray()` per batch | Store `(Action, Priority)` tuples instead of closures |
| `foreach` on `IReadOnlyDictionary` (PropsEqual) | `ShallowCompare.cs` | May box enumerator through interface | Cast to `Dictionary<>` for struct enumerator when possible |
| `ConditionalWeakTable` factory lambda | `Animator.cs` (~L104) | `_ => new Dictionary<>()` allocates closure per call even if key exists | Cache delegate in static field |
| Duplicate `CanReuseFiber` method | `FiberFunctionComponent.cs` vs `FiberChildReconciliation.cs` | Maintenance risk, not perf | Extract to shared static method |

---

## 4. Unity-Specific Concerns

### 4.1 Mono vs IL2CPP GC Characteristics

ReactiveUITK must perform well under both **Mono** (editor, development) and **IL2CPP** (builds, production):

- **Mono's Boehm GC**: Non-generational, stop-the-world. Every allocation contributes directly to GC pause time. The allocations in P0-2 (VNode cloning) and P0-3 (FiberNode creation) are particularly harmful here because they create many short-lived objects.

- **IL2CPP's Boehm GC**: Same characteristics but potentially worse because IL2CPP's Boehm is conservative (can retain memory longer due to false positives).

- **Unity 6+ incremental GC**: Mitigates pause spikes but doesn't eliminate the throughput cost of allocation. Reducing allocation volume is still the primary lever.

**Guidance:** Every eliminated allocation directly reduces frame-time variance. Object pooling (P0-3) and allocation elimination (P0-2) are the most impactful Unity-specific optimizations.

### 4.2 `VisualElement.schedule.Execute().Every(16)` for Animations

The animation system uses UITK's built-in scheduling, which creates a `ScheduledItem` per animation. This is correct but results in Unity managing the tick timing rather than the reconciler. Consider batching animations into a single `LateUpdate` tick for better control and to avoid per-animation scheduling overhead.

### 4.3 USS Classes vs Inline Styles

The current architecture applies styles as inline values (`element.style.width = ...`). While this matches React's model, UITK is optimized for **USS class-based styling** where `AddToClassList`/`RemoveFromClassList` triggers efficient batch-style recalculation. Heavy inline style usage (e.g., 20 properties per element) may be slower than equivalent USS class toggling for static styles.

### 4.4 `RegisterCallback` / `UnregisterCallback` Pattern

**Good news:** The current event system correctly registers wrapper delegates **once** and swaps the target handler in a dictionary (`meta.EventHandlerTargets`). This avoids the expensive register/unregister-per-render pattern. This is a mature, well-designed approach.

### 4.5 VisualElement Hierarchy Operations

`AppendChild`, `InsertBefore`, and `RemoveChild` are UITK operations that trigger layout recalculation. The current commit phase correctly batches these (all within a single `CommitRoot` call). However, UITK does not have React DOM's `requestAnimationFrame`-based batching — changes take effect immediately on the retained mode tree.

---

## 5. What's Already Well Done

ReactiveUITK has several strong engineering choices:

| Pattern | Where | Why It's Good |
|---------|-------|---------------|
| **Empty singletons** in VNode | `VNode.cs` L8-12 | `EmptyPropsInstance`, `EmptyChildrenInstance` — zero-alloc empty cases |
| **`StateSetterHandle<T>` struct** | `Hooks.cs` | Avoids closure allocation — delegates cached per `(slot, kind)` |
| **Time-sliced work loop** | `FiberReconciler.cs` | 2ms budget prevents frame drops, respects Unity's frame timing |
| **`CustomSampler` profiling** | `FiberReconciler.cs` | Proper Unity Profiler integration (shows up in Profiler timeline) |
| **Bailout optimization** | `FiberFunctionComponent.cs` | Skips re-render when props/context/state unchanged |
| **Typed `IProps` path** | `IProps.cs`, source gen | Source-gen structural equality — O(field-count) comparison without dict iteration |
| **Reference equality fast-path** | `ShallowCompare.cs`, `AreHostPropsEqual` | `ReferenceEquals` check before field-by-field comparison |
| **Effect linked-list** | `FiberReconciler.cs` | `NextEffect` chain — only dirty fibers in commit, no list allocation |
| **Event wrapper reuse** | `PropsApplier.cs` | Register once, swap target — avoids register/unregister churn |
| **Deferred update queue** | `FiberReconciler.cs` | Updates during commit don't corrupt WIP tree |
| **`[ThreadStatic]` hook context** | `Hooks.cs` | Zero-alloc per-render thread-local context |
| **HMR-aware fiber identity** | `FiberChildReconciliation.cs` | `CanReuseFiber` compares `DeclaringType.Name` for cross-assembly HMR |
| **Signal equality gate** | `Signal.cs` | `Set` no-ops for same value — prevents unnecessary subscriber notifications |
| **Priority scheduling** | `RenderScheduler.cs` | 4-tier priority with frame budget, intelligent queue cancellation |
| **State batching** | `RenderScheduler.cs` | `BeginBatch`/`EndBatch` prevents interleaving during multi-setState |

---

## 6. Recommended Implementation Order

Ordered by **impact ÷ effort**:

| # | Item | Impact | Effort | Est. Alloc Reduction |
|---|------|--------|--------|---------------------|
| 1 | P0-5: Fix `previousStyles` memory leak | Critical | Small | Eliminates unbounded leak |
| 2 | P1-4: Guard unconditional `Debug.Log` | High | Tiny | Eliminates string alloc per event unregister |
| 3 | P2-1: Static empty dict for `TryDiffProp` | Medium | Tiny | ~2 dicts per element per commit |
| 4 | P2-6: Guard unconditional reflection for names | Medium | Tiny | ~1 reflection call per component per render |
| 5 | P1-2: Cache `OnStateUpdated` delegate | High | Small | ~1 closure per component per render |
| 6 | P1-5: Optimize `className` diffing | High | Small | ~4 objects per className change |
| 7 | P0-1: Typed event dispatch (replace DynamicInvoke) | Critical | Medium | Eliminates reflection + boxing per event |
| 8 | P1-1: Merge 3 commit-phase tree walks into 1 | High | Medium | 2×(N-node) fewer traversals per commit |
| 9 | P0-2: Eliminate VNode defensive cloning | Critical | Medium | ~5 objects per element per render |
| 10 | P0-3: FiberNode object pooling | Critical | Medium | ~1 large object per fiber per reconciliation |
| 11 | P0-4: Typed hook storage (eliminate boxing) | Critical | Large | ~2 box/unbox per value-type hook per render |
| 12 | P1-3: ArrayPool for Signal.Set() snapshot | High | Small | ~1 array per signal update |
| 13 | P1-6: Cache reflection in ListView | High | Small | ~2 reflection calls per row bind |
| 14 | P1-7: Reverse index for ListView pool | High | Medium | O(N)→O(1) per row unbind |

---

## 7. Benchmarking Strategy

To validate these optimizations, establish these benchmarks **before** any changes:

### Micro-benchmarks (Unity Test Runner + `Stopwatch`)
1. **VNode creation throughput**: Create 1000 VNodes with 5 props each, measure time + GC alloc bytes
2. **Fiber reconciliation**: Render → re-render 100-element tree with 10% changes, measure per-frame alloc
3. **Event dispatch latency**: Fire 1000 `ClickEvent` dispatches, measure median latency
4. **Hook state read/write**: 1000 `UseState<int>` read-write cycles, measure boxing overhead
5. **Signal notification**: Set signal with 50 subscribers, measure alloc per `Set()`

### Integration benchmarks (Unity Profiler)
1. **Stress test UI**: 500-element tree with animations, scrolling list, frequent state updates
2. **Measure**: GC alloc per frame, render phase ms, commit phase ms, total frame time
3. **Capture**: Unity Profiler timeline showing `CustomSampler` markers for render/commit phases

### A/B comparison
Use `GC.GetTotalMemory(true)` before and after each optimization to quantify per-change impact.

---

*This document is a point-in-time analysis. Re-benchmark after each optimization to validate impact.*
