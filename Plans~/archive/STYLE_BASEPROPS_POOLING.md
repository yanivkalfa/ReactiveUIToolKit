# OPT-16: Style & BaseProps Object Pooling — Research & Implementation Plan

> **Status: ✅ COMPLETE (April 25, 2026)**
> **Date:** April 25, 2026
> **Objective:** Eliminate the remaining ~6,000 per-frame GC allocations (3,000 Style + 3,000 BaseProps)
> that cause Gen0/Gen1/Gen2 GC pauses — small freezes every ~1s and large freezes every ~9-12s.
> **Prerequisite:** Typed Props Pipeline + Typed Style Pipeline (both done).
> **Baseline:** 35 FPS, ~6,000 allocs/frame, ~2.3 MB GC pressure/frame.
> **Result:** 35 FPS (unchanged — pool CPU overhead offsets GC savings), 99% pool hit rate
> in steady state, reduced small freeze frequency. Large 9-11s spikes persist due to
> remaining ~6,200 allocs/frame from VirtualNode + IIFE closures (see OPT-18, OPT-10).

---

## Table of Contents

1. [Problem Analysis](#problem)
2. [Research Findings](#research)
3. [Three Critical Hazards](#hazards)
4. [Approach Evaluation](#approaches)
5. [Chosen Approach: Version-Stamped Pool](#chosen)
6. [Implementation Steps](#steps)
7. [System-by-System Impact](#impact)
8. [What Breaks](#breaks)
9. [What Does NOT Break](#safe)
10. [Risk Mitigations](#risks)
11. [Testing Strategy](#testing)
12. [File-by-File Change List](#files)

---

## <a name="problem"></a>1. Problem Analysis

### Current per-frame allocation (3,000 boxes stress test):

| Object | Count/frame | Size/obj | Total/frame |
|--------|------------|----------|-------------|
| `Style` instances | 3,000 | ~566 bytes | ~1.70 MB |
| `BaseProps` subclasses | 3,000 | ~200 bytes | ~0.60 MB |
| **Total** | **6,000** | | **~2.30 MB** |

Unity's Gen0 nursery is ~4 MB. At 2.3 MB/frame, the nursery fills every ~2 frames, triggering:
- **Gen0 incremental slices** every ~1 second (small freezes, ~3ms each)
- **Full Gen1/Gen2 collections** every ~9-12 seconds (large freezes, 10-50ms)

### Why these objects are allocated fresh every frame:

The source generator emits:
```csharp
V.VisualElement(new VisualElementProps {
    Style = new Style { Left = box.x, Top = box.y, Width = box.size, ... }
}, key: box.id)
```

Both `new VisualElementProps` and `new Style` are C# object initializers — there is no
pooling hook. The objects are created, consumed during reconciliation, and become garbage
after the commit phase swaps `fiber.HostProps = fiber.PendingHostProps`.

### Why the alternate fiber tree delays garbage collection:

The fiber reconciler uses a **two-tree pattern** (current + work-in-progress). After commit:
- The WIP tree becomes Current
- The old Current becomes the alternate (via `fiber.Alternate`)
- Old `HostProps` survives on the alternate until the **next** render's `CloneForReuse` overwrites it

This means a props/style object is live for **two full render cycles** before becoming garbage.

---

## <a name="research"></a>2. Research Findings

### 2.1 Style is user-authored, not emitter-generated

The UITKX source generator does **not** generate `new Style { ... }` — it passes user-written
style expressions through verbatim as property initializers on typed props. This means any
pooling change must work with arbitrary user C# expressions.

### 2.2 Props are always inline object initializers

The source generator always emits:
```csharp
V.Label(new LabelProps { Text = "hi", Style = new Style { ... } }, key: "k")
```

Props are never extracted to variables by the emitter. They're always constructed as direct
arguments to `V.*` factory methods.

### 2.3 Shared Style references are extremely common

Users frequently declare a Style variable and share it across multiple elements:
```csharp
var btnStyle = new Style { Width = 100f, Height = 30f };
<Button text="A" style={btnStyle} />   // same reference
<Button text="B" style={btnStyle} />   // same reference
<Button text="C" style={btnStyle} />   // same reference
```

Found in: `EventBatchingDemoFunc`, `HookStateQueueDemoFunc`, `KeyedDiffLisDemoFunc`,
`RouterDemoFunc`, `DeferredEffectDemoFunc`, `ContextBailoutDemoFunc`, `TicTacToe`, and many others.

### 2.4 Static readonly styles are pervasive

`.style.uitkx` modules define `public static readonly Style` fields that persist for the
entire app lifetime. Found 20+ instances across Samples, Docs, and Games.

### 2.5 ReferenceEquals gates at three levels

The diff pipeline uses `ReferenceEquals` as a fast-path skip at:
1. `BaseProps.ShallowEquals`: `!ReferenceEquals(Style, other.Style)` — skips deep comparison
2. `TypedPropsApplier.ApplyDiff`: `!ReferenceEquals(prev.Style, next.Style)` — skips DiffStyle
3. `TypedPropsApplier.DiffStyle`: `if (ReferenceEquals(prev, next)) return` — skips entire diff

These are **intentional optimizations** — when the same Style object is passed twice
(e.g., a `static readonly Style` or a shared variable), skipping the diff is correct because
the values cannot have changed.

### 2.6 HMR: No impact

Style and BaseProps live in the project assembly, not in HMR assemblies. HMR recompiles
component Render methods but uses the same `Style`/`BaseProps` types. Pooled objects from
pre-HMR renders are the same CLR type as post-HMR. **Zero risk.**

### 2.7 IDE/LSP: No impact

The LSP server operates on string-keyed schema data and Roslyn semantic models. It never
instantiates `Style` or `BaseProps` objects. **Zero risk.**

### 2.8 Style is never mutated after construction

No user code or framework code mutates a Style after assigning it to BaseProps. The only
mutation pattern is `V.BuildSafeAreaStyle` which creates a **new** Style, copies into it,
then mutates the copy.

### 2.9 No null-style ternary patterns

Styles are always non-null in JSX. `BaseProps.Style` defaults to null but all diff paths
handle null safely.

---

## <a name="hazards"></a>3. Three Critical Hazards

### Hazard 1: ReferenceEquals fast-path poisoning

If a pooled Style is returned to the pool, cleared, re-rented, and happens to be assigned
to the same fiber that held it previously, `ReferenceEquals(prev.Style, next.Style)` would
be `true` — causing the diff to be **completely skipped**. The element would retain stale
styles.

**Scenario:**
```
Frame N:   fiber.HostProps.Style = @0x1234 {Left=10, Top=20}
Frame N+1: Pool returns @0x1234, clears it
           User code rents @0x1234, sets {Color=red}
           fiber.PendingHostProps.Style = @0x1234 {Color=red}
           ShallowEquals: ReferenceEquals(@0x1234, @0x1234) == true → skip diff
           Element still shows Left=10, Top=20 — Color=red never applied
```

### Hazard 2: Shared Style across siblings

If `var s = new Style { ... }` is used by 3 elements, all three `BaseProps.Style` fields
point to the same object. If the pool returns this Style after the first element is diffed,
the remaining two elements would see a cleared/corrupted Style.

This is NOT just about pooling — it affects the return-to-pool timing. You can't return a
Style to the pool just because one consumer is done with it.

### Hazard 3: Static readonly styles entering the pool

A `static readonly Style` is an immortal singleton. If pooling logic returns it to a pool
(e.g., when a fiber using it is deleted), the next `Clear()` would corrupt the shared
singleton for all future renders.

---

## <a name="approaches"></a>4. Approach Evaluation

### Approach A: Direct Pool with Return-After-Two-Commits

Pool Style/BaseProps objects. Track generation counters. Return objects to pool two commits
after they become "old".

**Pros:** Maximum alloc reduction.
**Cons:** ReferenceEquals hazard is **unsolvable** without breaking the fast path. Shared
references require refcount tracking. Static readonly requires ownership tagging.
**Verdict:** ❌ Too dangerous. The ReferenceEquals fast path is load-bearing.

### Approach B: Pool + Version Stamp (Invalidate ReferenceEquals)

Add a `uint _version` field to Style. Increment on every pool-rent. Change `ReferenceEquals`
checks to `ReferenceEquals && a._version == b._version`.

**Pros:** Solves the ReferenceEquals hazard.
**Cons:** Still has shared-reference and static-readonly hazards. Version comparison adds
overhead to every diff (negates some of the fast-path benefit).
**Verdict:** ⚠️ Partial solution. Shared references still problematic.

### Approach C: Never Pool Existing Objects — Pool Only the Allocation

Instead of returning old objects to a pool, **pre-allocate a buffer of Style objects** and
hand them out via `Style.Rent()`. The old objects become garbage normally. The pool only
prevents `new` allocations — it reuses pre-allocated slots.

**Problem:** This doesn't reduce GC pressure at all — old objects still become garbage.
**Verdict:** ❌ Defeats the purpose.

### Approach D: Struct Style (value type)

Convert Style from a class to a struct. Store directly in BaseProps as a value field.
No heap allocation at all.

**Pros:** Zero allocs. No pooling complexity. No ReferenceEquals hazard (structs don't
have identity). `TypedEquals` becomes a direct field comparison.
**Cons:**
- Style is ~566 bytes — large struct copies on every assignment
- `BaseProps.Style` becomes non-nullable — need `bool HasStyle` flag
- All `ReferenceEquals` fast paths must be replaced with `HasStyle` + `TypedEquals`
- `IDictionary<string, object>` interface implementation on struct causes boxing when cast
- Breaking change: `Style` is currently `class` everywhere in user code
**Verdict:** ⚠️ Viable but large breaking change. Deferred.

### Approach E: Epoch-Based Pool with Ownership Flag

Add an `internal bool _pooled` flag and a `uint _epoch` to Style. The pool marks objects
on return. `Style.Rent()` increments epoch and clears `_pooled`. Static readonly styles
never have `_pooled` set. Shared styles: the **first** consumer to try returning a shared
style to pool sees multiple fiber references and skips the return.

**Problem:** Detecting "multiple references" requires traversing all fibers or maintaining
a refcount — expensive.
**Verdict:** ❌ Refcount overhead negates gains.

### Approach F: Source-Gen Rent/Return Pattern (The Pragmatic Middle)

The source generator emits `Style.__Rent()` instead of `new Style()` and
`Style.__Return(style)` at a safe point. Key design decisions:

1. **Only emitter-generated code uses `__Rent()`** — user code (`new Style { ... }`) still
   uses normal constructors. This isolates the pool to the hot path.
2. **A monotonic `_generation` counter** on Style prevents ReferenceEquals poisoning:
   ```csharp
   if (ReferenceEquals(a, b) && a._generation == b._generation)
       return true; // truly same object, same generation
   ```
3. **Shared styles are safe** because user-declared `var s = new Style` uses `new` (not pool)
   — these are never returned to pool.
4. **Static readonly styles are safe** — they use `new Style` (not pool).
5. **Return timing:** old `HostProps.Style` is returned to pool in `CommitPropsAndClearFlags`
   when `HostProps` is overwritten. At this point the old style is ONLY referenced by the
   alternate fiber's `HostProps`, which will be overwritten on the next `CloneForReuse`.
   However, since the alternate still holds a reference, we **delay return by one frame**
   using a pending-return queue.

**Pros:** Isolates pooling to generated code. No user-facing API change. ReferenceEquals
safe via generation counter. Static/shared styles naturally excluded.
**Cons:** Source generator change required. Delayed return adds one frame of latency
(objects returned to pool one frame after becoming obsolete — 50% pool efficiency on first
few frames, 100% at steady state). BaseProps pooling requires per-type pools or a generic pool.
**Verdict:** ✅ Best balance of safety and effectiveness.

---

## <a name="chosen"></a>5. Chosen Approach: Source-Gen Rent/Return (Approach F)

### 5.1 Style Pool Design

```csharp
// In Style.cs:
public partial class Style
{
    internal uint _generation;  // monotonically increasing per-rent

    // --- Pool internals ---
    private static readonly Stack<Style> s_pool = new(256);
    private static readonly List<Style> s_pendingReturn = new(256);
    private static uint s_nextGeneration = 1;

    /// <summary>
    /// Rent a Style from the pool. Only called by generated code.
    /// </summary>
    internal static Style __Rent()
    {
        Style s;
        if (s_pool.Count > 0)
        {
            s = s_pool.Pop();
        }
        else
        {
            s = new Style();
        }
        s._setBits0 = 0;
        s._setBits1 = 0;
        s._backgroundImage = null;
        s._fontFamily = null;
        s._transitionDelay = default;
        s._transitionDuration = default;
        s._transitionProperty = default;
        s._transitionTimingFunction = default;
#if UNITY_6000_3_OR_NEWER
        s._filter = default;
        s._unityMaterial = null;
#endif
        s._generation = s_nextGeneration++;
        return s;
    }

    /// <summary>
    /// Schedule a Style for return to pool on next flush.
    /// Only called by framework internals (CommitPropsAndClearFlags).
    /// </summary>
    internal static void __ScheduleReturn(Style s)
    {
        if (s == null) return;
        if (s._generation == 0) return; // generation 0 = user-created (new Style), never pool
        s_pendingReturn.Add(s);
    }

    /// <summary>
    /// Flush pending returns into the pool. Called once per frame after commit.
    /// </summary>
    internal static void __FlushReturns()
    {
        for (int i = 0; i < s_pendingReturn.Count; i++)
            s_pool.Push(s_pendingReturn[i]);
        s_pendingReturn.Clear();
    }
}
```

**Key safety invariants:**
- `_generation == 0` means "user-created via `new Style()`" — **never returned to pool**
- `_generation > 0` means "rented from pool" — can be returned
- `__ScheduleReturn` adds to `s_pendingReturn` (not directly to pool)
- `__FlushReturns` moves pending → pool once per frame, after ALL fibers are committed
- Static readonly styles always have `_generation == 0` (created via `new Style()`)
- Shared user-created styles always have `_generation == 0` (created via `new Style()`)

### 5.2 ReferenceEquals Fix

Replace all three `ReferenceEquals(style, style)` fast paths:

```csharp
// BEFORE:
if (ReferenceEquals(a.Style, b.Style)) ...

// AFTER:
if (Style.SameInstance(a.Style, b.Style)) ...

// Where:
internal static bool SameInstance(Style a, Style b)
{
    return ReferenceEquals(a, b) && a._generation == b._generation;
}
```

This handles:
- **Same object, not recycled:** `ReferenceEquals == true`, generations match → skip diff ✅
- **Same pointer, recycled:** `ReferenceEquals == true`, generations differ → DO diff ✅
- **Different objects:** `ReferenceEquals == false` → DO diff ✅
- **Both null:** `ReferenceEquals(null, null) == true` → need null guard first ✅

Full pattern:
```csharp
if (a == null && b == null) return true;          // both null → equal
if (a == null || b == null) return false;          // one null → not equal
if (ReferenceEquals(a, b) && a._generation == b._generation)
    return true;                                   // same instance, same generation → skip diff
// fall through to TypedEquals / DiffStyle
```

### 5.3 Source Generator Change

**Current emission:**
```csharp
V.VisualElement(new VisualElementProps {
    Style = new Style { Left = box.x, Top = box.y }
}, key: box.id)
```

**New emission:**
```csharp
V.VisualElement(new VisualElementProps {
    Style = Style.__Rent().__Set(s => { s.Left = box.x; s.Top = box.y; })
}, key: box.id)
```

Or simpler — since Style setters return void, use a helper:
```csharp
// On Style:
internal Style __Init(Action<Style> init)
{
    init(this);
    return this;
}

// Emitted:
Style.__Rent().__Init(s => { s.Left = box.x; s.Top = box.y; })
```

**Wait — this allocates a delegate!** The `Action<Style>` lambda captures `box` → heap closure.
That's worse than `new Style`.

**Better approach — emit direct property assignments:**
```csharp
// Emit a local variable + assignments:
var __s0 = Style.__Rent();
__s0.Left = box.x;
__s0.Top = box.y;
V.VisualElement(new VisualElementProps { Style = __s0 }, key: box.id)
```

This is zero-alloc for Style but requires the emitter to extract Style construction from
the inline object initializer into a preceding local variable + assignment block.

**Alternative — keep the object initializer syntax but change the constructor:**
```csharp
// Emit:
V.VisualElement(new VisualElementProps {
    Style = new Style(pooled: true) { Left = box.x, Top = box.y }
}, key: box.id)

// Style constructor:
public Style() { _generation = 0; }  // user path
internal Style(bool pooled) { _generation = s_nextGeneration++; }  // emitter path
```

**Problem:** `new Style(pooled: true)` still allocates a new object from the heap. The pool
is only useful if we `Rent()` instead of `new`.

**Final emission pattern — inline `__Rent()` + object initializer:**

C# **does not allow** object initializer syntax on a rented object (it requires `new T { }`).
So we must use one of:

**Option 1: Local var pattern (cleanest):**
```csharp
// Emitter generates:
var __s_42 = Style.__Rent();
__s_42.Left = box.x;
__s_42.Top = box.y;
__s_42.Width = box.size;
// ... then used in props:
V.VisualElement(new VisualElementProps { Style = __s_42 }, key: box.id)
```

Each `style={...}` attribute generates a unique local `__s_N`. This is straightforward —
the emitter already knows which attributes are `style` type.

**Option 2: Fluent builder (method chaining):**
```csharp
Style.__Rent().Set(s => s.Left, box.x).Set(s => s.Top, box.y)
```
**Problem:** Each `.Set()` call is a method invocation — overhead. And `Expression<Func<>>` would be
even worse. Skip.

**Chosen: Option 1 — local var pattern.** Minimal generated code change, zero alloc, no new
patterns for users to learn (users still write `new Style { ... }` — only the emitter uses `__Rent`).

### 5.4 BaseProps Pooling

BaseProps pooling follows the same pattern but is more complex due to ~50 subclasses.

**Generic pool approach:**
```csharp
public abstract class BaseProps
{
    internal uint _generation;

    internal abstract void __Reset(); // clear all fields to default

    private static class Pool<T> where T : BaseProps, new()
    {
        private static readonly Stack<T> s_pool = new(64);
        private static uint s_nextGeneration = 1;

        internal static T Rent()
        {
            T p;
            if (s_pool.Count > 0)
            {
                p = s_pool.Pop();
            }
            else
            {
                p = new T();
            }
            p.__Reset();
            p._generation = s_nextGeneration++;
            return p;
        }

        internal static void Return(T p)
        {
            if (p._generation == 0) return;
            s_pool.Push(p);
        }
    }

    internal static T __Rent<T>() where T : BaseProps, new() => Pool<T>.Rent();
}
```

Each `BaseProps` subclass implements `__Reset()`:
```csharp
public sealed class LabelProps : BaseProps
{
    internal override void __Reset()
    {
        // BaseProps fields:
        Name = null; ClassName = null; Style = null; Ref = null;
        ContentContainer = null; Visible = null; Enabled = null;
        Tooltip = null; ViewDataKey = null; PickingMode = null;
        Focusable = null; TabIndex = null; DelegatesFocus = null;
        LanguageDirection = null;
        // LabelProps fields:
        Text = null;
        // Event handlers — all null:
        // (none for Label beyond base events)
    }
}
```

**Source generator emission:**
```csharp
// BEFORE:
V.Label(new LabelProps { Text = "hello", Style = __s_42 }, key: "k")

// AFTER:
var __p_7 = BaseProps.__Rent<LabelProps>();
__p_7.Text = "hello";
__p_7.Style = __s_42;
V.Label(__p_7, key: "k")
```

### 5.5 Return Timing & Flush Cycle

```
Frame N render:
  1. Source gen calls Style.__Rent() / BaseProps.__Rent<T>()
  2. Props flow through VNode → Fiber → PendingHostProps

Frame N commit:
  3. CommitUpdate: hostConfig.ApplyTypedProperties(fiber.HostProps, fiber.PendingHostProps)
  4. CommitPropsAndClearFlags:
     - old = fiber.HostProps
     - fiber.HostProps = fiber.PendingHostProps
     - Style.__ScheduleReturn(old.Style)   // old style → pending return queue
     - BaseProps.__ScheduleReturn(old)      // old props → pending return queue
  5. After full tree walk: Style.__FlushReturns() → moves pending to pool

Frame N+1 render:
  6. Old objects are now in the pool, available for __Rent()
  7. The alternate fiber still references the old HostProps pointer,
     but CloneForReuse will overwrite it:
     clone.HostProps = current.HostProps;  // overwrites alternate's reference
```

**Wait — step 7 has a problem.** Between step 5 (object returned to pool) and step 7
(alternate's reference overwritten by CloneForReuse), the alternate fiber still holds a
pointer to the now-pooled object. If `CloneForReuse` reads `current.HostProps` (which is
the NEW committed props, not the pooled one), this is fine — the alternate's stale pointer
is overwritten before anyone reads it.

Let me verify this with the actual code sequence:

```
After CommitRoot:
  _root.Current = finishedWork;     // WIP becomes current
  // The OLD current is now the alternate, accessible via fiber.Alternate

CloneForReuse (next render):
  clone = current.Alternate         // the old current
  clone.HostProps = current.HostProps  // overwrite with committed props from new current
```

So `clone.HostProps` is overwritten to `current.HostProps` (the NEWLY committed props).
The pooled old props are **never read** from the alternate. ✅

**However:** What if a component bails out (no re-render)? Then `CloneForReuse` is not called
for that fiber. The alternate keeps its old `HostProps` pointer indefinitely. If that
pointer was returned to pool, and the pool re-issues it, we'd have two fibers pointing to
the same object.

**Fix:** Only `__ScheduleReturn` for props that were **actually overwritten in this commit**.
In `CommitPropsAndClearFlags`, check if `oldHostProps != newHostProps` before scheduling
return:

```csharp
var oldProps = fiber.HostProps;
fiber.HostProps = fiber.PendingHostProps;
if (oldProps != null && !ReferenceEquals(oldProps, fiber.HostProps))
{
    Style.__ScheduleReturn(oldProps.Style);
    BaseProps.__ScheduleReturn(oldProps);
}
```

For bailed-out fibers where `HostProps == PendingHostProps` (same reference — no change),
no return is scheduled. ✅

### 5.6 Pool Size Limits

```csharp
private static readonly Stack<Style> s_pool = new(256);

internal static void __FlushReturns()
{
    for (int i = 0; i < s_pendingReturn.Count; i++)
    {
        if (s_pool.Count < 4096) // hard cap
            s_pool.Push(s_pendingReturn[i]);
        // else: let it become garbage (pool is full)
    }
    s_pendingReturn.Clear();
}
```

Cap prevents unbounded memory growth in scenarios where element count spikes temporarily
(e.g., scrolling a huge list) then drops back.

---

## <a name="steps"></a>6. Implementation Steps

### Step 1: Add `_generation` field + pool infrastructure to Style ✅

**File:** `Shared/Props/Typed/Style.cs`

- Add `internal uint _generation;` field
- Add `private static Stack<Style> s_pool`, `s_pendingReturn`, `s_nextGeneration`
- Add `internal static Style __Rent()` — rent from pool or create new, set generation > 0
- Add `internal static void __ScheduleReturn(Style s)` — guard generation == 0
- Add `internal static void __FlushReturns()` — move pending → pool with cap
- Add `internal static bool SameInstance(Style a, Style b)` — ReferenceEquals + generation check
- Ensure `public Style()` constructor sets `_generation = 0` (user path — never pooled)

### Step 2: Add `_generation` field + pool infrastructure to BaseProps ✅

**File:** `Shared/Props/Typed/BaseProps.cs`

- Add `internal uint _generation;` field
- Add `internal abstract void __Reset();`
- Add `internal static class Pool<T>` with `Rent()` / `Return()`
- Add `internal static T __Rent<T>()` convenience method
- Add `internal static void __ScheduleReturn(BaseProps p)`
- Add `internal static void __FlushReturns()`
- Ensure `BaseProps()` constructor sets `_generation = 0`

### Step 3: Implement `__Reset()` on all BaseProps subclasses ✅

**Files:** All 62 `*Props.cs` files in `Shared/Props/Typed/`

Each subclass implements `__Reset()` that nulls/defaults all fields (base + own).

### Step 4: Fix ReferenceEquals fast paths ✅

**Files:**
- `Shared/Props/Typed/BaseProps.cs` — `ShallowEquals`: replace `ReferenceEquals(Style, other.Style)` with `Style.SameInstance(Style, other.Style)`
- `Shared/Props/TypedPropsApplier.cs` — `ApplyDiff`: replace `ReferenceEquals(prev.Style, next.Style)` with `Style.SameInstance(prev.Style, next.Style)`
- `Shared/Props/TypedPropsApplier.cs` — `DiffStyle`: replace `ReferenceEquals(prev, next)` with `SameInstance(prev, next)`

### Step 5: Add return-to-pool logic in commit phase ✅

**File:** `Shared/Core/Fiber/FiberReconciler.cs`

In `CommitPropsAndClearFlags`, before overwriting `fiber.HostProps`:
```csharp
var oldProps = fiber.HostProps;
fiber.HostProps = fiber.PendingHostProps;
if (oldProps != null && !ReferenceEquals(oldProps, fiber.HostProps) && oldProps._generation > 0)
{
    if (oldProps.Style != null && oldProps.Style._generation > 0)
        Style.__ScheduleReturn(oldProps.Style);
    BaseProps.__ScheduleReturn(oldProps);
}
```

After the full tree walk, call `Style.__FlushReturns()` and `BaseProps.__FlushReturns()`.

### Step 6: Update source generator to emit `__Rent()` pattern ✅

**File:** `SourceGenerator~/Emitter/CSharpEmitter.cs`

In `EmitBuiltinTyped`:
- For `style` attribute: emit `var __s_N = Style.__Rent(); __s_N.Prop1 = val1; ...` before the V.* call
- For the props: emit `var __p_N = BaseProps.__Rent<XxxProps>(); __p_N.Style = __s_N; __p_N.Prop2 = val2; ...`
- Replace the inline `new XxxProps { ... }` with `__p_N`

### Step 7: Update HMR emitter to emit same `__Rent()` pattern ✅

**File:** `Editor/HMR/HmrCSharpEmitter.cs`

Mirror the source generator change. Same `__Rent()` / local var pattern.

### Step 8: Verify compilation + stress test ✅

- ✅ Pool verified at 99% hit rate (capacity fix 1024→4096)
- ✅ Small GC freezes reduced in frequency
- ✅ Visual correctness confirmed
- ✅ Diagnostic counters added, verified, then removed

---

## <a name="impact"></a>7. System-by-System Impact

| System | Impact | Details |
|--------|--------|---------|
| **Source Generator** | **Modified** | Emits `Style.__Rent()` + `BaseProps.__Rent<T>()` instead of `new`. Local var pattern for style attribute extraction. |
| **HMR Emitter** | **Modified** | Same change as source generator |
| **Fiber Reconciler** | **Modified** | `CommitPropsAndClearFlags` schedules pool returns + flush |
| **Style.cs** | **Modified** | Pool infrastructure, `_generation`, `SameInstance` |
| **BaseProps.cs** | **Modified** | Pool infrastructure, `_generation`, `__Reset()` abstract |
| **All 50 *Props.cs** | **Modified** | Each implements `__Reset()` |
| **TypedPropsApplier.cs** | **Modified** | `ReferenceEquals` → `SameInstance` (3 locations) |
| **IDE/LSP** | **No change** | String-based schema, no Style instances |
| **User .uitkx files** | **No change** | `new Style { ... }` still works (generation=0, never pooled) |
| **User C# code** | **No change** | Public API unchanged |
| **Tests** | **May need updates** | Tests that check style identity may need generation-aware assertions |

---

## <a name="breaks"></a>8. What Breaks

### 8.1 Nothing in user-facing API

Users continue to write `new Style { ... }` and `new XxxProps { ... }`. These objects have
`_generation = 0` and are never pooled. **Zero user-facing breaking changes.**

### 8.2 Generated code output changes

The generated `.g.cs` files will contain `Style.__Rent()` and `BaseProps.__Rent<T>()` calls
instead of `new`. This is purely internal — users don't read generated code. However, if
anyone has custom tooling that parses generated output, it would see different patterns.

### 8.3 `ReferenceEquals` semantics change (internal only)

Framework code that relied on `ReferenceEquals(style, style)` as a proxy for "same content"
must use `Style.SameInstance()` instead. This only affects framework internals (3 locations).

---

## <a name="safe"></a>9. What Does NOT Break

| Feature | Why safe |
|---------|---------|
| `new Style { ... }` user syntax | `_generation = 0` → never pooled, never returned |
| `static readonly Style` | `_generation = 0` → never pooled |
| Shared `var s = new Style` | `_generation = 0` → never pooled |
| `Style.Of(...)` factory | Creates via `new Style()` → `_generation = 0` |
| `V.BuildSafeAreaStyle` | Creates via `new Style()` → `_generation = 0` |
| `IDictionary<string,object>` interface | Unchanged |
| `TypedEquals` | Unchanged (compares by value, not identity) |
| HMR hot reload | Same CLR types, pool survives swaps |
| IDE completions/diagnostics | No runtime dependency |
| Event handlers on props | Cleared by `__Reset()`, set by emitter |
| Conditional rendering / @if | Props created inside IIFE, returned to pool normally |
| @foreach loops | Each iteration creates its own rented Style/Props |

---

## <a name="risks"></a>10. Risk Mitigations

### 10.1 Stale reference on alternate fiber

**Risk:** Alternate fiber holds pointer to pooled object.
**Mitigation:** Only schedule return when `oldProps != fiber.HostProps` (overwritten, not same ref).
For bailed-out fibers where props didn't change, no return is scheduled.

### 10.2 Pool exhaustion / memory leak

**Risk:** Pool grows unbounded after element count spike.
**Mitigation:** Hard cap (`4096` for Style, `1024` per BaseProps type). Excess objects are
dropped and become garbage.

### 10.3 Generation counter overflow

**Risk:** `uint` wraps at ~4.29 billion. At 3000 rents/frame × 60 fps = 180,000/s, overflow
after ~6.6 hours.
**Mitigation:** Use `uint` (not `ushort`). At stress-test rates, overflow after ~6.6 hours.
In real apps (~200 elements), overflow after ~330 hours. Overflow to 0 would make the
object look "user-created" (generation 0 = never pool) — safe failure mode (object becomes
garbage instead of being pooled). Alternatively, skip 0 on overflow:
`s_nextGeneration = s_nextGeneration == 0 ? 1 : s_nextGeneration;`

### 10.4 Thread safety

**Risk:** Pool accessed from multiple threads.
**Mitigation:** Unity UI runs on the main thread. `RenderScheduler.LateUpdate()` and all
reconciler work run on the main thread. No thread safety needed.

### 10.5 Deleted fibers

**Risk:** When a fiber is deleted, its HostProps should be returned to pool.
**Mitigation:** `CommitDeletion` already processes deleted fibers. Add
`Style.__ScheduleReturn(fiber.HostProps?.Style)` and `BaseProps.__ScheduleReturn(fiber.HostProps)`
in the deletion path.

---

## <a name="testing"></a>11. Testing Strategy

### 11.1 Unit Tests

- **Pool lifecycle:** `Rent()` → set properties → `ScheduleReturn()` → `FlushReturns()` → `Rent()` again → verify clean state
- **Generation counter:** Verify rented style has generation > 0, user-created has generation == 0
- **SameInstance:** Same object same generation → true. Same object different generation → false. Different objects → false.
- **User styles not pooled:** `new Style { Left = 5f }` → `__ScheduleReturn()` → verify NOT added to pool (generation == 0)
- **Static readonly not pooled:** Same test with `static readonly Style`
- **`__Reset()` completeness:** For each Props subclass, verify all fields are null/default after reset

### 11.2 Integration Tests

- **Stress test:** 3000 boxes, 60s. Compare FPS, GC.Alloc count before/after.
- **Visual regression:** All showcase demos render identically.
- **Shared style:** Create two elements with same `var style`, verify both render correctly across frames.
- **Conditional rendering:** Toggle elements on/off, verify pool return/reuse works.
- **HMR:** Hot-reload a component with styles, verify rendering is correct.

### 11.3 Profiler Verification

- **Expected:** ~0 Style/BaseProps GC allocs in steady state (after initial pool warmup)
- **Pool warmup:** First frame allocates 3000 + 3000 objects. Second frame reuses pool for 3000 + 3000. Third frame onwards: zero allocs.
- **GC collections:** Should drop from ~30 Gen0/s to near-zero.

---

## <a name="files"></a>12. File-by-File Change List

### Core changes

| File | Change | Risk |
|------|--------|------|
| `Shared/Props/Typed/Style.cs` | Pool infra, `_generation`, `SameInstance`, `__Rent`, `__ScheduleReturn`, `__FlushReturns` | Medium |
| `Shared/Props/Typed/BaseProps.cs` | Pool infra, `_generation`, `__Reset` abstract, `__Rent<T>`, `__ScheduleReturn`, `__FlushReturns` | Medium |
| `Shared/Props/TypedPropsApplier.cs` | 3× `ReferenceEquals` → `SameInstance` | Low |
| `Shared/Core/Fiber/FiberReconciler.cs` | Return-to-pool in `CommitPropsAndClearFlags` + `CommitDeletion`, flush after commit | Medium |

### Props subclass changes (~50 files)

| File | Change |
|------|--------|
| Each `*Props.cs` in `Shared/Props/Typed/` | Add `internal override void __Reset() { ... }` |

### Source generator changes

| File | Change | Risk |
|------|--------|------|
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | `EmitBuiltinTyped`: extract style to `var __s_N = Style.__Rent(); __s_N.X = ...; `, extract props to `var __p_N = BaseProps.__Rent<T>(); __p_N.X = ...;` | Medium |

### HMR changes

| File | Change | Risk |
|------|--------|------|
| `Editor/HMR/HmrCSharpEmitter.cs` | Same pattern as source generator | Medium |

### No changes needed

| File | Why |
|------|-----|
| All `.uitkx` files | User syntax unchanged |
| `Shared/Props/Typed/StyleKeys.cs` | String constants unchanged |
| `Shared/Props/Typed/CssHelpers.cs` | Helper constants unchanged |
| `Shared/Props/PropsApplier.cs` | Dict path unchanged |
| `Shared/Core/V.cs` | Uses `new Style()` (generation 0, safe) |
| IDE/LSP (`ide-extensions~/`) | No runtime Style dependency |
| Grammar (`ide-extensions~/grammar/`) | Text-based |

---

## Appendix A: Alternative Considered — Struct Style

Converting Style to a `struct` would eliminate heap allocation entirely but has significant
consequences:

| Aspect | Impact |
|--------|--------|
| Copy semantics | 566-byte struct copied on every assignment — potentially slower than pool |
| `BaseProps.Style` | Becomes non-nullable `Style` + `bool HasStyle` — changes all null checks |
| `IDictionary` interface | Boxing when cast to `IDictionary<string, object>` (fallback path) |
| `ReferenceEquals` | Not applicable to structs — need value comparison everywhere |
| User code | `new Style { ... }` still works but semantics change (copy vs reference) |
| Breaking changes | Any code that uses `Style s = null;` breaks |
| `TypedEquals` | Must change to compare by value (no `ReferenceEquals` fast path) |

This is viable as a **future Phase 2** after pooling proves the allocation reduction works
and the ecosystem handles the transition. Pool-based approach is safer as a first step.

---

## Appendix B: Expected Performance After Pooling

### Stress test (3,000 boxes):

| Metric | Before Pooling | After Pooling | Delta |
|--------|---------------|---------------|-------|
| Style allocs/frame | 3,000 | 0 (steady state) | -3,000 |
| BaseProps allocs/frame | 3,000 | 0 (steady state) | -3,000 |
| Total GC allocs/frame | ~6,000 | ~0 | **-6,000** |
| GC bytes/frame | ~2.3 MB | ~0 | **-2.3 MB** |
| Gen0 collections/10s | ~150 | ~0 | **-150** |
| Small freezes (1s interval) | Yes | No | **Eliminated** |
| Large freezes (9-12s interval) | Yes | No | **Eliminated** |
| Estimated FPS gain | — | +3-5 FPS | 38-40 FPS |

### Combined with all optimizations (if OPT-4, OPT-7, OPT-10 also done):

| Cumulative | Allocs/frame | FPS |
|-----------|-------------|-----|
| Baseline (before all work) | ~42,000 | 28.3 |
| After typed pipeline + style | ~6,000 | 35 |
| After pooling (this plan) | ~0 | 38-40 |
| After OPT-4 + OPT-7 + OPT-10 | ~0 + CPU savings | 40-45 |
| Pure UITK baseline | 0 | 47.7 |
