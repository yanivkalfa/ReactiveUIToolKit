# OPT-18: VirtualNode Object Pooling — Research & Implementation Plan

> **Date:** April 25, 2026
> **Status:** ✅ IMPLEMENTED
> **Objective:** Eliminate ~3,200 VirtualNode allocations per frame (~538 KB/frame) via pooling
> **Prerequisite:** OPT-16 (Style & BaseProps pooling — done)
> **Current:** 35 FPS, ~6,200 remaining allocs/frame, ~960 KB GC pressure/frame
> **Target:** ~3,000 fewer allocs/frame → combined with OPT-10 would reach near-zero allocs

---

## Table of Contents

1. [Problem Analysis](#problem)
2. [VirtualNode Anatomy](#anatomy)
3. [Complete Lifecycle Analysis](#lifecycle)
4. [All Persistent VNode References](#persistent)
5. [Critical Hazard: Children Array Aliasing](#children-hazard)
6. [Critical Hazard: ErrorBoundary Long-Lived VNodes](#errorboundary-hazard)
7. [Critical Hazard: Suspense VNode Storage](#suspense-hazard)
8. [Ecosystem Impact Analysis](#ecosystem)
9. [Approach Evaluation](#approaches)
10. [Chosen Approach: Pool with Decoupled Children](#chosen)
11. [Implementation Steps](#steps)
12. [What Breaks / What Doesn't](#breaks)
13. [Testing Strategy](#testing)

---

## <a name="problem"></a>1. Problem Analysis

### Per-frame VNode allocations (3,000 boxes stress test):

Every `V.*()` factory call creates `new VirtualNode(...)`. VirtualNode is a `sealed class` with
17 properties (6 value-type, 11 reference-type).

| Source | Count/frame | Notes |
|--------|------------|-------|
| Host elements (`V.VisualElement`, `V.Label`, etc.) | ~3,000 | One per box in `@foreach` |
| Fragment nodes (`V.Fragment`) | ~2-5 | `@foreach` outer wrapper + root |
| Function component returns | ~5-10 | StressTest component + parents |
| Text nodes (`V.Text`) | ~0-10 | If labels are used |
| **Total** | **~3,010-3,025** | |

### Object size (64-bit Mono):

```
Object header:           16 bytes
VirtualNodeType (enum):   4 bytes  (+ 4 padding)
string ElementTypeName:   8 bytes
VisualElement PortalTarget: 8 bytes
VirtualNode Fallback:     8 bytes
Func<bool> SuspenseReady: 8 bytes
Task SuspenseReadyTask:   8 bytes
string TextContent:       8 bytes
string Key:               8 bytes
IReadOnlyDictionary Properties: 8 bytes
IReadOnlyList<VNode> Children:  8 bytes
VirtualNode ErrorFallback:      8 bytes
ErrorEventHandler ErrorHandler: 8 bytes
string ErrorResetToken:         8 bytes
IReadOnlyList<PropTypeDef> PropTypes: 8 bytes
Func<IProps,IReadOnlyList<VNode>,VNode> TypedFunctionRender: 8 bytes
IProps TypedProps:        8 bytes
BaseProps HostProps:      8 bytes
─────────────────────────────────
Total:                   ~168 bytes
```

**Per-frame cost:** ~3,015 × 168 = **~506 KB/frame** = ~17.7 MB/second at 35 FPS

---

## <a name="anatomy"></a>2. VirtualNode Anatomy

### Current structure (all read-only auto-properties):

```csharp
public sealed class VirtualNode
{
    // All 17 properties are { get; } — set only in constructor
    public VirtualNodeType NodeType { get; }
    public string ElementTypeName { get; }
    public VisualElement PortalTarget { get; }
    public VirtualNode Fallback { get; }
    public Func<bool> SuspenseReady { get; }
    public Task SuspenseReadyTask { get; }
    public string TextContent { get; }
    public string Key { get; }
    public IReadOnlyDictionary<string, object> Properties { get; }
    public IReadOnlyList<VirtualNode> Children { get; }
    public VirtualNode ErrorFallback { get; }
    public ErrorEventHandler ErrorHandler { get; }
    public string ErrorResetToken { get; }
    public IReadOnlyList<PropTypeDefinition> PropTypes { get; }
    public Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> TypedFunctionRender { get; }
    public IProps TypedProps { get; }
    public BaseProps HostProps { get; }
}
```

### Constructors:
1. **Primary constructor** (17 parameters) — used by all `V.*()` factories
2. **Template constructor** `(VirtualNode template, propTypes)` — used only by `WithPropTypesImmutable()`

### Key internal members:
- `EmptyProps` / `EmptyChildren` — static singletons, shared across all VNodes
- `CloneProps()` / `CloneChildren()` — currently unused (defensive copies removed)
- `ClonePropTypes()` — copies propTypes array; returns `EmptyPropTypesInstance` when null
- `WithPropTypesImmutable()` — creates a copy with different propTypes; returns `this` when propTypes is empty

---

## <a name="lifecycle"></a>3. Complete Lifecycle Analysis

### VNode flow through the system:

```
USER RENDER (.uitkx / V.* factories)
  │
  ▼
V.VisualElement(props, key, children) → new VirtualNode(...)
  │
  ▼
Component.Render() returns VirtualNode
  │
  ▼
FiberFunctionComponent.RenderFunctionComponent()
  ├─ wipFiber.LastRenderedVNode = childVNode   ← STORED (function component)
  └─ ReconcileSingleChild(wipFiber, current, childVNode)
       │
       ▼
FiberChildReconciliation.ReconcileChildren(fiber, fiber.Children)
  │   Reads: vnode.Key, vnode.NodeType, vnode.ElementTypeName,
  │          vnode.TypedFunctionRender, vnode.HostProps, vnode.Children,
  │          vnode.Properties, vnode.TextContent, vnode.PortalTarget,
  │          vnode.ErrorResetToken, vnode.TypedProps
  │   Stores: fiber.Children = vnode.Children  ← ALIASED
  │           fiber.LastRenderedVNode = vnode   ← STORED (ErrorBoundary only)
  │
  ▼
CompleteWork — reads fiber.PendingHostProps (NOT VNode)
  │
  ▼
CommitRoot
  ├─ CommitDeletions
  ├─ CommitWork (effects)
  ├─ _root.Current = finishedWork (WIP→Current swap)
  ├─ CommitPropsAndClearFlags
  │    └─ fiber.HostProps = fiber.PendingHostProps (props commit, no VNode access)
  ├─ Style.__FlushReturns() / BaseProps.__FlushReturns()
  ├─ Passive effects
  └─ Clear WIP
```

### When VNodes become garbage:

**Normal host elements (the hot path — 3,000/frame):**
1. Frame N: `V.VisualElement(props, key)` creates VNode_A
2. VNode_A.Children → stored as `fiber.Children` (aliased)
3. VNode_A.HostProps → stored as `fiber.PendingHostProps` (already pooled separately)
4. VNode_A itself is NOT stored on the fiber (no LastRenderedVNode for host elements)
5. After reconciliation reads VNode_A's fields, VNode_A is **immediately unreferenced**
6. **VNode_A becomes garbage during the same frame it was created**

**Function components:**
1. Component.Render() returns VNode_B
2. `wipFiber.LastRenderedVNode = VNode_B`
3. Next render: `wipFiber.LastRenderedVNode = VNode_C` → VNode_B becomes garbage
4. **VNode_B lives for exactly one render cycle on that component**

**ErrorBoundary nodes:**
1. `<ErrorBoundary>` creates VNode_EB with fallback/handler/children
2. `fiber.LastRenderedVNode = VNode_EB` (in FiberFactory.CreateNew)
3. VNode_EB is accessed in `UpdateErrorBoundary()` every render to read:
   - `ErrorResetToken` — compared with fiber's cached value
   - `ErrorFallback` — used as fallback children when active
   - `ErrorHandler` — called when error occurs
   - `Children` — used as normal children when not showing fallback
4. `CloneForReuse`: `clone.LastRenderedVNode = newVNode ?? current.LastRenderedVNode`
5. **VNode_EB is replaced each render** (newVNode is always non-null for re-rendered boundaries)

### Key insight for pooling:

**For the hot path (host elements — ~3,000/frame), VNodes are transient within a single reconciliation phase.** They are created by `V.*()`, their fields are read during `ReconcileChildren`/`CreateFiber`/`CloneForReuse`, and then they're unreferenced. The fiber stores extracted values (Key, ElementType, HostProps, Children, etc.) — NOT the VNode itself.

The only persistent VNode references are:
- `fiber.LastRenderedVNode` (function components + error boundaries)
- `fiber.Children` (aliases the VNode's Children array, not the VNode itself)
- `_root.RootVNode` (bookkeeping only — never read back)

---

## <a name="persistent"></a>4. All Persistent VNode References

| Location | Field Type | Written When | Read When | Pooling Risk |
|----------|-----------|-------------|-----------|-------------|
| `FiberNode.LastRenderedVNode` | `VirtualNode` | After function component render; ErrorBoundary creation/clone | `UpdateErrorBoundary` reads ErrorResetToken, ErrorFallback, ErrorHandler, Children | **HIGH** — VNode must stay valid between renders |
| `FiberNode.Children` | `IReadOnlyList<VirtualNode>` | `= vnode.Children` during fiber creation/cloning | `ReconcileChildren`, `UpdatePortal`, `UpdateErrorBoundary`, bailout comparison | **MEDIUM** — array reference aliased, not the VNode |
| `FiberRoot.RootVNode` | `VirtualNode` | `ScheduleUpdateOnFiber` when new vnode provided | **NEVER READ** | **NONE** — pure bookkeeping |
| `SuspenseProps.Node` | `VirtualNode` | `CreateSuspenseProps` during fiber creation/clone | `SuspenseRender` reads SuspenseReady, Fallback, Children, SuspenseReadyTask | **HIGH** — VNode stored in IProps, accessed during render |
| `RouteFuncProps.Element` | `VirtualNode` | User creates `new RouteFuncProps { Element = vnode }` | `RouteFunc.Render` returns it as render output | **LOW** — VNode passes through, created fresh each render |
| `SuspenseRenderState.LastRenderedChildren` | `IReadOnlyList<VirtualNode>` | **DEAD** — never written in fiber path | **DEAD** — never read | **NONE** |

---

## <a name="children-hazard"></a>5. Critical Hazard: Children Array Aliasing

### The problem:

When a VNode is created by `V.VisualElement(props, key, child1, child2)`, the `params VirtualNode[]` array becomes `VNode.Children`. Then `FiberFactory` does:

```csharp
fiber.Children = vnode.Children;  // Same array reference
```

If we pool the parent VNode and clear its `Children` field, the fiber's `Children` still points to the original array — which is fine. The **array is NOT owned by the VNode**; it's owned by the caller (the `params` allocation or `__C()` helper). The VNode just holds a reference.

### But there's a subtlety:

For `EmptyChildren()` (the static singleton `EmptyChildrenInstance`), many VNodes share the same `IReadOnlyList<VirtualNode>`. Clearing a pooled VNode's Children to `null` or to `EmptyChildrenInstance` is safe because the fiber already captured its own reference.

### Conclusion: **NOT a hazard for pooling.**

The Children array is independently allocated. The VNode holds a reference to it but does NOT own it. The fiber copies the reference. Clearing the VNode's field after the fiber has captured it is safe.

---

## <a name="errorboundary-hazard"></a>6. Critical Hazard: ErrorBoundary Long-Lived VNodes

### The problem:

ErrorBoundary fibers store `LastRenderedVNode` and **re-read its fields across multiple renders**:

```csharp
// UpdateErrorBoundary — called every render cycle for EB fibers
var boundaryNode = fiber.LastRenderedVNode;
// Reads: boundaryNode.ErrorResetToken, boundaryNode.ErrorFallback,
//        boundaryNode.ErrorHandler, boundaryNode.Children
```

If this VNode were pooled and recycled, the fiber would read stale/corrupted data from a reused object.

### How often does this occur?

ErrorBoundary is relatively rare:
- Stress test: 0 error boundaries (typically)
- Typical app: 1-5 error boundaries
- Each creates exactly 1 VNode stored as `LastRenderedVNode`

### Solution:

**Do NOT pool ErrorBoundary VNodes.** Check `vnode.NodeType == VirtualNodeType.ErrorBoundary` and skip pooling. This affects at most 1-5 VNodes per app — negligible allocation cost.

### Alternative: Extract ErrorBoundary data into fiber fields

Instead of keeping the VNode alive, extract the needed fields into `FiberNode` at creation time:
```csharp
fiber.ErrorFallback = vnode.ErrorFallback;        // already have ErrorBoundaryResetKey
fiber.ErrorHandler = vnode.ErrorHandler;           // new field
fiber.ErrorBoundaryChildren = vnode.Children;      // alias to Children
```

Then `UpdateErrorBoundary` reads fiber fields instead of VNode fields. This would allow the ErrorBoundary VNode to be pooled. **Recommended approach** — cleaner architecture regardless of pooling.

---

## <a name="suspense-hazard"></a>7. Critical Hazard: Suspense VNode Storage

### The problem:

`SuspenseProps.Node` stores the entire VNode and reads it during `SuspenseRender`:

```csharp
var suspenseNode = (rawProps as SuspenseProps)?.Node;
// Reads: suspenseNode.SuspenseReady, suspenseNode.Fallback,
//        suspenseNode.Children, suspenseNode.SuspenseReadyTask
```

### Lifecycle:

`CreateSuspenseProps(vnode)` is called in:
1. `FiberFactory.CreateNew` — initial creation
2. `FiberFactory.CloneForReuse` — re-render (creates NEW SuspenseProps with current VNode)
3. `FiberChildReconciliation.CreateFiber` — inline creation

On re-render, `CloneForReuse` passes the **new VNode** (from the current render), so the SuspenseProps.Node is always the **freshly created VNode**. The old SuspenseProps/VNode becomes garbage after the WIP swap.

### Wait — is the VNode alive across frames?

YES, but only for ONE render cycle. `SuspenseProps` is stored as `fiber.TypedPendingProps`, and after commit, `fiber.TypedProps = fiber.TypedPendingProps`. The old `TypedProps` (containing the old SuspenseProps.Node) becomes garbage.

The VNode inside SuspenseProps follows the same lifecycle as function component `LastRenderedVNode` — it lives for exactly one render cycle, then gets replaced.

### Solution:

**Same as ErrorBoundary: extract needed fields into the SuspenseProps or fiber fields.**

Or simpler: SuspenseProps already stores the VNode. Since Suspense VNodes are rare (typically 0-5 per app), we can skip pooling them. Check `vnode.NodeType == VirtualNodeType.Suspense` and skip.

### Recommended: Skip pooling for Suspense VNodes (0-5 per app, negligible).

---

## <a name="ecosystem"></a>8. Ecosystem Impact Analysis

### Source Generator (`SourceGenerator~/`)
- **Impact:** NONE. The source gen emits `V.*()` factory calls. It never touches VNode internals. The factories would internally use the pool — the emitted code stays identical.
- **No changes needed.**

### HMR Emitter (`Editor/HMR/HmrCSharpEmitter.cs`)
- **Impact:** NONE. Same as source gen — emits `V.*()` calls as text. The pool is internal to `V.cs`.
- **No changes needed.**

### HMR Delegate Swapper (`Editor/HMR/UitkxHmrDelegateSwapper.cs`)
- **Impact:** NONE. Walks fibers and swaps `TypedRender` delegates. Never reads VNode fields. Uses VirtualNode only in the render delegate's generic type signature.
- **No changes needed.**

### IDE Extensions (`ide-extensions~/`)
- **Impact:** NONE. All VNode references are string literals in code generation (LSP virtual document scaffolding, grammar schemas). No runtime VNode instances.
- **No changes needed.**

### Unity Runtime (`Runtime/`)
- **Impact:** Minimal. `RenderScheduler`, `EditorRenderScheduler` don't touch VNodes. They schedule work callbacks.
- **No changes needed.**

### User-facing API
- **VNode is NOT part of the documented public API.**
- Users create VNodes implicitly via JSX in `.uitkx` files.
- `V.*()` factory methods are public — their signatures DON'T change.
- **No user-visible changes.**

### Diagnostics/Benchmark
- **Impact:** NONE. Benchmark code creates VNodes via `V.*()` factories (transient).
- **No changes needed.**

### Router (`Shared/Core/Router/`)
- `RouteFuncProps.Element` holds a VNode — it's created fresh each render and passes through.
- The element VNode follows normal lifecycle: created in user render, consumed by Route.Render, unreferenced after.
- **No special handling needed.** The VNode is fresh (not from pool), and even if it were, Route.Render reads it immediately and returns it — no cross-frame storage.

### Test code
- All tests create VNodes via `V.*()` factories. No identity/equality assertions on VNodes.
- **No test changes needed** (assuming tests work without relying on `new` vs pool semantics).

---

## <a name="approaches"></a>9. Approach Evaluation

### Approach A: Generation-stamped pool (mirrors OPT-16)

Convert readonly auto-properties to backing fields. Add `_generation`, `__Rent()`, `__ScheduleReturn()`, `__FlushReturns()`.

**Pros:** Proven pattern. Same infrastructure.
**Cons:** VNode has 17 properties to convert. All public read-only. Changing to `{ get; internal set; }` expands writability within the assembly.

### Approach B: Pool only in V.cs factories (no generation stamp)

The key insight: **VNodes don't need generation stamps** because nobody compares VNode identity. Unlike BaseProps (which uses `SameInstance()` for equality checking), VNodes are NEVER compared by reference equality anywhere in the codebase.

`V.*()` factories can rent from pool, set fields, and return. No generation tracking needed. The pool just needs to ensure VNodes are fully reset before reuse.

**Pros:** Simpler than OPT-16. No generation field needed.
**Cons:** No safety net — a stale VNode reference would silently read wrong data. But per our lifecycle analysis, VNodes for host elements are consumed within one reconciliation phase.

### Approach C: Pool with selective exclusion

Pool all common VNode types (Element, Text, Fragment, FunctionComponent) but skip Suspense and ErrorBoundary. These rare types keep `new VirtualNode()`.

**Pros:** Avoids the two hazardous persistent-reference cases entirely.
**Cons:** Two code paths in `V.*()` factories.

### Approach D: Extract persistent data from VNodes into fibers

Before pooling, refactor the code so NO VNode reference persists past its reconciliation phase:
1. `fiber.LastRenderedVNode` for ErrorBoundary → extract to fiber fields
2. `SuspenseProps.Node` → extract to dedicated SuspenseProps fields
3. `fiber.LastRenderedVNode` for function components → only used for bailout, NOT for reading VNode fields. Can be nulled after reconciliation.
4. `_root.RootVNode` → never read, can be removed or kept (no impact)

After this refactor, ALL VNodes are truly ephemeral. Then pooling is straightforward.

**Pros:** Cleanest architecture. Eliminates persistent VNode references entirely.
**Cons:** More refactoring upfront, but each change is simple and testable.

### Recommendation: **Approach D + A (Extract first, then pool)**

1. First: Refactor persistent references out of VNode into fiber fields
2. Then: Apply generation-stamped pool (proven OPT-16 pattern)
3. Keep generation stamps as safety net despite them being theoretically unnecessary

---

## <a name="chosen"></a>10. Chosen Approach: Extract + Pool

### Phase 1: Decouple ErrorBoundary from VNode

**Goal:** `UpdateErrorBoundary` reads fiber fields instead of `fiber.LastRenderedVNode`.

Add to `FiberNode`:
```csharp
public VirtualNode ErrorBoundaryFallback;    // was: LastRenderedVNode.ErrorFallback
public ErrorEventHandler ErrorBoundaryHandler; // was: LastRenderedVNode.ErrorHandler
// ErrorBoundaryResetKey already exists on FiberNode
// Children already exists on FiberNode
```

Update `FiberFactory.CreateNew` (ErrorBoundary case):
```csharp
fiber.ErrorBoundaryFallback = vnode.ErrorFallback;
fiber.ErrorBoundaryHandler = vnode.ErrorHandler;
// fiber.ErrorBoundaryResetKey = vnode.ErrorResetToken;  // already done
// fiber.Children = vnode.Children;                      // already done
```

Update `FiberFactory.CloneForReuse`:
```csharp
clone.ErrorBoundaryFallback = newVNode?.ErrorFallback ?? current.ErrorBoundaryFallback;
clone.ErrorBoundaryHandler = newVNode?.ErrorHandler ?? current.ErrorBoundaryHandler;
```

Update `UpdateErrorBoundary` to read from fiber instead of VNode.
Update `TryActivateErrorBoundary` to read from fiber instead of VNode.

### Phase 2: Decouple Suspense from VNode

**Goal:** `SuspenseRender` reads SuspenseProps fields instead of `SuspenseProps.Node`.

Expand `SuspenseProps`:
```csharp
internal sealed class SuspenseProps : IProps
{
    // Extracted fields — no longer stores VNode
    public Func<bool> SuspenseReady { get; set; }
    public Task SuspenseReadyTask { get; set; }
    public VirtualNode Fallback { get; set; }
    public IReadOnlyList<VirtualNode> Children { get; set; }
}
```

Update `CreateSuspenseProps`:
```csharp
internal static SuspenseProps CreateSuspenseProps(VirtualNode vnode)
{
    return new SuspenseProps
    {
        SuspenseReady = vnode.SuspenseReady,
        SuspenseReadyTask = vnode.SuspenseReadyTask,
        Fallback = vnode.Fallback,
        Children = vnode.Children
    };
}
```

Update `SuspenseRender` to read from SuspenseProps fields instead of `suspenseNode.*`.

**Note:** `Fallback` is a VirtualNode. If the fallback is a fresh VNode from user render, it follows normal transient lifecycle. If it's cached (unlikely — no user code caches VNodes), it would still work because the reconciler only reads its fields during reconciliation.

### Phase 3: Clean up LastRenderedVNode

After Phase 1-2:
- `LastRenderedVNode` is still used for **function components** (`FiberFunctionComponent.cs` L164: `wipFiber.LastRenderedVNode = childVNode`)
- It's ONLY read in `UpdateErrorBoundary` (which now uses fiber fields) and in error handling (`FindNearestErrorBoundary`)
- For function components, `LastRenderedVNode` is set but never read for field access — it's only used as a signal for "has this component rendered" type checks.

**Action:** Verify all reads. If `LastRenderedVNode` is only read in ErrorBoundary paths (now refactored), we can remove the function component assignment or keep it harmlessly.

**Result after Phase 1-3:** No VNode instance persists across render cycles. All VNode data is extracted into fiber fields or SuspenseProps during reconciliation.

### Phase 4: Add pool infrastructure to VirtualNode

Mirrors OPT-16 pattern:

```csharp
public sealed class VirtualNode
{
    // Pool infrastructure
    internal uint _generation;
    private const int PoolCapacity = 4096;
    private static readonly Stack<VirtualNode> s_pool = new Stack<VirtualNode>(256);
    private static readonly List<VirtualNode> s_pendingReturn = new List<VirtualNode>(2048);
    private static uint s_nextGeneration = 1;

    // Convert all auto-properties to backing fields
    internal VirtualNodeType _nodeType;
    internal string _elementTypeName;
    // ... all 17 fields ...

    public VirtualNodeType NodeType => _nodeType;
    public string ElementTypeName => _elementTypeName;
    // ... all 17 getters ...

    internal static VirtualNode __Rent()
    {
        VirtualNode v;
        if (s_pool.Count > 0)
        {
            v = s_pool.Pop();
            v.__Reset();
        }
        else
        {
            v = new VirtualNode();
        }
        v._generation = s_nextGeneration++;
        return v;
    }

    private void __Reset()
    {
        // Clear all reference-type fields to prevent stale references
        _elementTypeName = null;
        _portalTarget = null;
        _fallback = null;
        _suspenseReady = null;
        _suspenseReadyTask = null;
        _textContent = null;
        _key = null;
        _properties = EmptyPropsInstance;
        _children = EmptyChildrenInstance;
        _errorFallback = null;
        _errorHandler = null;
        _errorResetToken = null;
        _propTypes = EmptyPropTypesInstance;
        _typedFunctionRender = null;
        _typedProps = null;
        _hostProps = null;
        // _nodeType stays (overwritten by factory)
    }

    internal static void __ScheduleReturn(VirtualNode v)
    {
        if (v == null || v._generation == 0) return; // user-created
        s_pendingReturn.Add(v);
    }

    internal static void __FlushReturns()
    {
        for (int i = 0; i < s_pendingReturn.Count; i++)
        {
            if (s_pool.Count < PoolCapacity)
                s_pool.Push(s_pendingReturn[i]);
        }
        s_pendingReturn.Clear();
    }
}
```

### Phase 5: Update V.cs factories to use pool

Every `V.*()` factory changes from:
```csharp
return new VirtualNode(nodeType, elementTypeName, textContent, key, properties, children, ...);
```
To:
```csharp
var v = VirtualNode.__Rent();
v._nodeType = VirtualNodeType.Element;
v._elementTypeName = "VisualElement";
v._key = key;
v._properties = EmptyProps();
v._children = children ?? EmptyChildren();
v._hostProps = props;
return v;
```

**~50 factory methods** need this change. All follow the same pattern.

### Phase 6: Return VNodes to pool in CommitRoot

After `CommitPropsAndClearFlags` and `Style.__FlushReturns()` / `BaseProps.__FlushReturns()`:
```csharp
VirtualNode.__FlushReturns();
```

But when do we schedule returns? The key question:

**Host element VNodes** — consumed during reconciliation, unreferenced after. We could schedule return right after `ReconcileChildren` processes them. But that's scattered across many call sites.

**Simpler approach:** Don't explicitly return VNodes. Instead, use a **frame-based arena pattern**:
- Rent from pool during render phase
- At end of CommitRoot, return ALL rented VNodes at once (tracked via a frame-local list)
- This matches VNode lifecycle perfectly — all VNodes from current render are consumed during reconciliation and can be returned after commit

**Implementation:** `V.*()` factories add the rented VNode to a frame-local list. `CommitRoot` flushes the entire list to the pool.

Actually, let's use the same pending-return pattern as OPT-16. The return points are:

1. **`FiberFactory.CloneForReuse`** — the `newVNode` parameter has been consumed (its fields extracted to the fiber). Schedule return.
2. **`FiberFactory.CreateNew`** — the `vnode` parameter has been consumed. Schedule return.
3. **`FiberChildReconciliation.CreateFiber`** — same pattern.
4. **`FiberReconciler.CreateWorkInProgress`** — the `vnode` parameter has been consumed.
5. **`CommitRoot`** — flush all pending returns to pool.

Wait — there's a problem. `CloneForReuse(current, newVNode)` extracts fields from `newVNode`, but `newVNode.Children` is **aliased** into `clone.Children`. If we return `newVNode` to pool and reset its Children field, the fiber's Children is unaffected (it holds its own reference to the array). But if the pooled VNode is re-rented and assigned new Children, the fiber still has the old array reference. So this is safe.

BUT: `LastRenderedVNode` is set to `newVNode` in `CloneForReuse`. After Phase 1-3, this reference is either unused or no longer read for field access. If we've properly extracted all data, we can set `LastRenderedVNode = null` for non-ErrorBoundary fibers (or remove the assignment entirely).

### Phase 7: Handle remaining edge cases

1. **`_root.RootVNode = vnode`** — never read. Can keep (harmless) or remove.
2. **`WithPropTypesImmutable`** — creates a template copy. Extremely rare (only for `@propTypes` directive). Skip pooling for these — use `new VirtualNode()` with `_generation = 0`.
3. **`V.Host()`** — uses `props.ToDictionary()`. Rare. Pool or skip.
4. **`V.VisualElementSafe()`** — legacy dict-based path. Rare. Pool or skip.
5. **`WrapChildrenAsFragment`** in `FiberIntrinsicComponents` — creates Fragment VNode. Pool it.
6. **`WrapHostChildren`** in `VNodeHostRenderer` — creates Fragment VNode via `V.Fragment()`. Pool it.
7. **`AnimateFunc.Render`** — creates `V.VisualElement(...)`. Pool it (goes through factory).

---

## <a name="steps"></a>11. Implementation Steps

### Step 1: ✅ Extract ErrorBoundary data from VNode into FiberNode

**Files:** `FiberNode.cs`, `FiberFactory.cs`, `FiberChildReconciliation.cs`, `FiberReconciler.cs`

Add 2 fields to FiberNode. Update 5 methods that read `LastRenderedVNode` for ErrorBoundary data. Remove `LastRenderedVNode = vnode` for ErrorBoundary in `CreateNew` and `CreateFiber`.

### Step 2: ✅ Extract Suspense data from VNode into SuspenseProps

**Files:** `FiberIntrinsicComponents.cs`

Expand SuspenseProps with 4 extracted fields. Update `CreateSuspenseProps`. Update `SuspenseRender` to read from SuspenseProps instead of `.Node`. Remove `Node` property.

### Step 3: ✅ Audit and minimize LastRenderedVNode usage

**Files:** `FiberFactory.cs`, `FiberFunctionComponent.cs`, `FiberReconciler.cs`, `FiberNode.cs`

After Steps 1-2, `LastRenderedVNode` was confirmed fully dead — written by function components but NEVER READ by anyone. Removed the field entirely from `FiberNode`, removed all writes from `FiberFactory.CloneForReuse` and `FiberFunctionComponent.RenderFunctionComponent`.

### Step 4: ✅ Convert VNode properties to backing fields + pool infrastructure

**File:** `VNode.cs`

- Convert all 17 auto-properties to `internal` backing fields + `public` getters
- Add `_generation`, `__Rent()`, `__Reset()`, `__ScheduleReturn()`, `__FlushReturns()`
- Keep public constructor for backward compat (sets `_generation = 0`)

### Step 5: ✅ Update V.cs factories to use pool

**File:** `V.cs`

- All ~50 factory methods converted to `VirtualNode.__Rent()` + field assignment via `RentElement()` / `RentElementWithChildren()` helpers
- Skipped pooling for `V.Host()` and `V.VisualElementSafe()` (dict-based legacy, use public constructor with generation=0)

### Step 6: ✅ Add return-to-pool in reconciliation

**Files:** `FiberFactory.cs`, `FiberChildReconciliation.cs`, `FiberReconciler.cs`

- `FiberFactory.CreateNew`: `VirtualNode.__ScheduleReturn(vnode)` after all fields extracted
- `FiberFactory.CloneForReuse`: `VirtualNode.__ScheduleReturn(newVNode)` after all fields extracted
- `FiberChildReconciliation.CreateFiber`: `VirtualNode.__ScheduleReturn(vnode)` after all fields extracted
- `CommitRoot`: `VirtualNode.__FlushReturns()` after `Style.__FlushReturns()` and `BaseProps.__FlushReturns()`

### Step 7: ✅ Verify compilation

- Zero compile errors across all assemblies
- All pre-existing warnings unchanged
- No new errors introduced

---

## <a name="breaks"></a>12. What Breaks / What Doesn't

### What does NOT break:

| System | Why Safe |
|--------|---------|
| Source generator | Emits `V.*()` calls — internal pool is transparent |
| HMR emitter | Same — emits `V.*()` calls |
| HMR delegate swapper | Reads `TypedRender` (delegate), not VNode fields |
| IDE extensions | All references are string literals / schemas |
| User `.uitkx` code | Creates VNodes via JSX → `V.*()` factories |
| Router | `RouteFuncProps.Element` passes through, consumed same frame |
| Diagnostics/Benchmarks | Creates VNodes via `V.*()`, transient |
| ShallowEquals / PropsApplier | Operates on BaseProps/Style, not VNodes |

### What could break (and mitigations):

| Risk | Likelihood | Mitigation |
|------|-----------|-----------|
| Stale VNode read from pooled object | Low (after Phase 1-3 refactor) | Generation stamps; extract all persistent refs first |
| ErrorBoundary fallback not showing | Medium | Phase 1 extracts EB data to fiber fields; test explicitly |
| Suspense fallback broken | Medium | Phase 2 extracts Suspense data to SuspenseProps; test explicitly |
| User code caching VNode in useMemo | Very low (0 instances found) | Generation stamp would catch; VNode reads return defaults |
| `WithPropTypesImmutable` double-pool | Low | Use `new VirtualNode()` with `_generation = 0` for copies |
| `params VirtualNode[]` array pooled | None | Arrays are NOT pooled, only VNode objects |
| Public constructor backward compat | None | Constructor still works, sets `_generation = 0` |

---

## <a name="testing"></a>13. Testing Strategy

### Automated checks:
1. Compile all assemblies — zero errors
2. Run source gen tests (`SourceGenerator~/Tests/`)

### Manual checks:
1. **Stress test** — 3000 boxes rendering at 35+ FPS, visual correctness
2. **Showcase demos** — all demo pages render correctly
3. **ErrorBoundary** — trigger an error in a component, verify fallback appears, verify reset works
4. **Suspense** — verify loading state shows fallback, completion shows content
5. **Router** — navigate between routes, verify correct content rendering
6. **HMR** — modify a `.uitkx` file, verify hot reload applies changes
7. **Profiler** — verify reduced GC allocation count (expected: ~3,000 fewer allocs/frame)

### Optional diagnostic (like OPT-16):
Add temporary pool hit/miss counters to verify pool behavior. Remove after verification.

---

## Expected Gains

| Metric | Before | After |
|--------|--------|-------|
| VNode allocs/frame | ~3,015 | ~5-15 (Suspense/EB/legacy only) |
| VNode bytes/frame | ~506 KB | ~1 KB |
| Total allocs/frame | ~6,200 | ~3,200 |
| Total GC pressure/frame | ~960 KB | ~460 KB |

Combined with OPT-10 (IIFE elimination, ~3,000 closure allocs) → total allocs/frame would drop to ~200 (arrays, delegate scaffolding). At that point, GC pressure is near-zero and the 9-11 second spikes should disappear.

---

## File-by-File Change List

| File | Change | Risk |
|------|--------|------|
| `Shared/Core/VNode.cs` | Convert properties to backing fields, add pool infrastructure | Medium |
| `Shared/Core/V.cs` | All ~50 factories use `__Rent()` + field assignment | Low |
| `Shared/Core/Fiber/FiberNode.cs` | Add `ErrorBoundaryFallback`, `ErrorBoundaryHandler` fields | Low |
| `Shared/Core/Fiber/FiberFactory.cs` | Extract EB data to fiber; schedule VNode return | Medium |
| `Shared/Core/Fiber/FiberChildReconciliation.cs` | Extract EB data to fiber; schedule VNode return | Medium |
| `Shared/Core/Fiber/FiberReconciler.cs` | `UpdateErrorBoundary` reads fiber fields; `CommitRoot` flushes pool | Medium |
| `Shared/Core/Fiber/FiberIntrinsicComponents.cs` | Expand SuspenseProps; remove `.Node`; update `SuspenseRender` | Medium |
| `Shared/Core/Fiber/FiberFunctionComponent.cs` | Minimal — may remove `LastRenderedVNode` assignment | Low |
| `Shared/Core/Fiber/FiberRoot.cs` | Optional — remove unused `RootVNode` | None |
| `Shared/Core/VNodeHostRenderer.cs` | No change — `V.Fragment()` goes through pooled factory | None |
| `Editor/HMR/*` | No changes | None |
| `SourceGenerator~/` | No changes | None |
| `ide-extensions~/` | No changes | None |
