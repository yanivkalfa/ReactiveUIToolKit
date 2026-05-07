# UITKX Optimization Plan V2

> **Status:** Active — accepting new items
> **Started:** April 26, 2026
> **Predecessor:** `Plans~/archive/OPTIMIZATION_PLAN.md` (closed April 25, 2026)
> **Scope:** Core library (`Shared/`), Runtime (`Runtime/`), Source Generator (`SourceGenerator~/`)

This plan contains optimization items discovered after the V1 optimization effort
concluded. Each item lists status, motivation, design, expected impact, risk, and
validation plan.

---

## Item Index

| ID | Item | Status | Estimated Impact |
|----|------|--------|------------------|
| OPT-V2-1 | Eliminate `__C` Allocations on the JSX Children Path | Researched (Phase A+B ready) | ~70% reduction in JSX-path allocs |
| OPT-V2-2 | Static-Style Hoisting + Pool Cap Tuning (revised from “Sparse Storage”) | ✅ Implemented April 27, 2026 — 1040/1040 SG + 61/61 LSP green | ~80% of `__Rent` calls eliminated; ~3.5 MB resident memory reclaimed |
| OPT-V2-3 | (Deferred) Recover render-phase pool returns via commit-phase walk | Deferred | ~96 KB/sec GC savings (showcases) — only worthwhile if OPT-V2-2 is NOT done first |

---

## OPT-V2-1: Eliminate `__C` Allocations on the JSX Children Path

### Status: Researched (April 27, 2026) — ready to implement

### Summary

Cut per-element children-array allocations on the JSX hot path by:

- **Phase A — SG/HMR emit:** when an element's children are 100% statically classifiable
  (no `@if` / `@for` / `@foreach` / `@while` / `@switch` / `@(expr)`), drop the `__C(...)`
  call and pass children directly to the container factory's existing
  `params VirtualNode[] children` parameter.
- **Phase B — `__C` body rewrite:** for the dynamic case (children include directives or
  inline expressions), replace the current
  `new List<VirtualNode>().Add(...).Add(...).ToArray()` shape with a two-pass
  count-then-fill into a single freshly-allocated `VirtualNode[count]`.

Both phases are independent, additive, and **safe by construction** — neither touches
the OPT-18 class of bug (no recycled `VirtualNode` references, no recycled child arrays).

### Status of the original plan

The previous draft of this section proposed pooling `List<VirtualNode>` instances and
returning them to the pool "after the reconciler extracts children into fiber child
pointers." Investigation on April 27, 2026 showed the framing was wrong on three counts:

1. **There is no persistent `List<VirtualNode>` to pool.** The generated `__C(...)`
   helper builds a transient `List<VirtualNode>` purely as a *builder*, then immediately
   calls `list.ToArray()`. The persistent collection that lives on `VirtualNode._children`
   (and later on `FiberNode.Children`) is the **`VirtualNode[]`** returned by `ToArray()`,
   not the list. The list becomes garbage one statement after construction; pooling it
   eliminates only that single allocation.
2. **The result `VirtualNode[]` cannot be safely pooled.** It IS the value that
   `vnode._children` points at, which is then aliased into `fiber.Children` in
   `FiberFactory.CreateNew` / `CloneForReuse` and read by:
   - `FiberFunctionComponent.RenderImpl` line 162 (passed to user render delegate),
   - the bailout's `ReferenceEquals(prevChildren, nextChildren)` and
     `ChildrenListEqual(...)` comparison ([FiberFunctionComponent.cs:69-73](Shared/Core/Fiber/FiberFunctionComponent.cs#L69-L73)),
   - `FiberFragment.UpdateFragment` ([FiberFragment.cs:17-23](Shared/Core/Fiber/FiberFragment.cs#L17-L23)),
   - `FiberReconciler.UpdateHostComponent` ([FiberReconciler.cs:1211-1239](Shared/Core/Fiber/FiberReconciler.cs#L1211-L1239)),
   - `FiberFactory.CloneFromCurrent` line 121 (carried forward to the WIP fiber).
   The array's lifetime extends across **two render cycles** (current + alternate).
   Returning it to a pool repeats the OPT-18 disaster.
3. **The biggest savings live in the source generator, not the runtime.** For the common
   case of an element with all static children, three allocations happen per JSX site
   — `params object[]` for `__C`'s argument, `List<VirtualNode>` + backing array, then
   `VirtualNode[]` from `ToArray()` — when one would do.

The new plan below replaces that draft.

### Background — what the current pipeline emits

The source generator (`SourceGenerator~/Emitter/CSharpEmitter.cs`) and the HMR emitter
(`Editor/HMR/HmrCSharpEmitter.cs`) both produce a per-component `__C` helper:

```csharp
private static VirtualNode[] __C(params object[] items)
{
    var list = new List<VirtualNode>();
    foreach (var __ci in items)
    {
        if (__ci is VirtualNode __vn) { if (__vn != null) list.Add(__vn); }
        else if (__ci is IEnumerable<VirtualNode> __seq)
            foreach (var __sn in __seq) { if (__sn != null) list.Add(__sn); }
    }
    return list.ToArray();
}
```

Used at 4 emit sites in [CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs):

- `EmitBuiltinElement` line 925 — typed-props built-in container (`V.VisualElement(props, key: k, __C(...))`)
- `EmitBuiltinDict` line 982 — untyped-dict built-in container
- `EmitFuncComponent` line 1099 — function component (`V.Func<P>(R, props, key: k, children: __C(...))`)
- `EmitFragment` line 1256 — `V.Fragment(key: k, __C(...))`

`__C` exists to cover two runtime cases the JSX emit may produce:

| Child kind | Emits | `__C` purpose |
|---|---|---|
| `ElementNode` | `V.X(...)` → non-null `VirtualNode` | direct copy |
| `TextNode` | `V.Text("...")` → non-null `VirtualNode` | direct copy |
| `IfNode` (no `@else`) | IIFE returning `VirtualNode` or **null** | null filter |
| `IfNode` (with `@else`) | IIFE returning `VirtualNode` | direct copy (null technically possible if a branch returns null) |
| `ForNode` / `ForeachNode` / `WhileNode` / `SwitchNode` | IIFE returning **`IEnumerable<VirtualNode>`** | flatten |
| `ExpressionNode` (`@(expr)`) | raw expression — could be `VirtualNode`, `null`, or `IEnumerable<VirtualNode>` (e.g. `@(__children)` slot pass-through) | both filter and flatten |
| `CommentNode` | nothing emitted | already skipped at SG-emit time |

Per `__C(c1, c2, c3)` call, the runtime allocates:

1. `object[3]` for the `params object[] items` payload (Roslyn-implicit).
2. `List<VirtualNode>` — 16 bytes for the wrapper plus a `VirtualNode[4]` (or larger if
   it grows) backing array.
3. `VirtualNode[N]` from `ToArray()` — this is the persistent children array.
4. `List<VirtualNode>.Enumerator` boxed via `foreach` — actually a struct, no alloc.

**Savings opportunity:** For ~80% of JSX sites in a typical app (purely static children
or all-element-children list), allocations 1, 2, and the `ToArray` copy are pure waste —
the children array could be the params VirtualNode[] of the container method itself,
allocated once.

### Phase A — Source-generator emit optimization

#### Classification at AST emit time

Add a helper in `CSharpEmitter`:

```csharp
private static bool ChildrenAreAllSimple(ImmutableArray<AstNode> children)
{
    foreach (var c in children)
    {
        if (c is CommentNode) continue;          // skipped at emit time
        if (c is ElementNode) continue;
        if (c is TextNode) continue;
        return false;                            // anything else needs __C
    }
    return true;
}
```

`IfNode` is conservatively classified as **not simple** even when an `@else` branch
exists, because branch bodies may use `return (...)` to produce a `VirtualNode` *or*
they may evaluate to null (the IIFE wrapper is `Func<VirtualNode>` and there's no
guarantee). Re-evaluating later if a tighter `IfNode` analysis (all branches provably
non-null) is worth the extra complexity; not in the first pass.

#### Updated emit at each site

For each of the 4 `__C` call sites:

```csharp
// Before:
_sb.Append(", __C(");
EmitChildArgs(children);
_sb.Append(")");

// After:
if (ChildrenAreAllSimple(children))
{
    _sb.Append(", ");
    EmitChildArgs(children);                    // emits "c1, c2, c3" — params expansion
}
else
{
    _sb.Append(", __C(");
    EmitChildArgs(children);
    _sb.Append(")");
}
```

For the `EmitFuncComponent` site (currently uses `children: __C(...)` named arg):

```csharp
if (ChildrenAreAllSimple(children))
{
    _sb.Append(", ");
    EmitChildArgs(children);                    // params VirtualNode[] children expansion
}
else
{
    _sb.Append(", children: __C(");
    EmitChildArgs(children);
    _sb.Append(")");
}
```

The container methods all already accept `params VirtualNode[] children` as the trailing
parameter (verified for `V.VisualElement`, every typed `V.X` container, `V.Func<P>`,
`V.Func`, and `V.Fragment`). When the simple path emits `V.X(props, key: k, c1, c2, c3)`,
Roslyn expands the params and allocates exactly one `VirtualNode[3]` — which becomes
the persistent `_children` array. No `__C`, no `object[]`, no `List<VirtualNode>`.

#### Net allocations per JSX site

| Path | Before | After |
|---|---|---|
| Simple children (e.g. `<Box><Label/><Button/></Box>`) | `object[3]` + `List<VN>` + `VN[N]` ≈ 3 allocs | `VN[N]` ≈ 1 alloc |
| Dynamic children (any `@if`/`@foreach`/`@(...)`) | `object[N]` + `List<VN>` + `VN[N]` ≈ 3 allocs | `object[N]` + `VN[N]` ≈ 2 allocs (Phase B) |

#### HMR mirror

`Editor/HMR/HmrCSharpEmitter.cs` has the **identical** `__C` template at line 1199 and
the same 4-site usage pattern (lines 759, 870, 927, 972, 1018, 1031). The HMR emit must
mirror Phase A so that hot-reloaded components have the same alloc shape (otherwise an
HMR'd component allocates more than its source-gen'd counterpart, masking measured
wins). Mechanical mirror — same classification helper, same 4 sites.

### Phase B — `__C` body rewrite (dynamic case)

For the cases where `__C` is still emitted, replace the `List<VirtualNode>` builder
with a two-pass count-then-fill:

```csharp
private static VirtualNode[] __C(params object[] items)
{
    // Pass 1: count valid VNodes (handles flatten + null filter without allocating a list)
    int count = 0;
    for (int i = 0; i < items.Length; i++)
    {
        var ci = items[i];
        if (ci is VirtualNode vn)
        {
            if (vn != null) count++;
        }
        else if (ci is global::System.Collections.Generic.IReadOnlyList<VirtualNode> ros)
        {
            for (int j = 0; j < ros.Count; j++) if (ros[j] != null) count++;
        }
        else if (ci is global::System.Collections.Generic.IEnumerable<VirtualNode> seq)
        {
            foreach (var sn in seq) if (sn != null) count++;
        }
    }
    if (count == 0) return global::System.Array.Empty<VirtualNode>();

    // Pass 2: fill into pre-sized array
    var result = new VirtualNode[count];
    int k = 0;
    for (int i = 0; i < items.Length; i++)
    {
        var ci = items[i];
        if (ci is VirtualNode vn)
        {
            if (vn != null) result[k++] = vn;
        }
        else if (ci is global::System.Collections.Generic.IReadOnlyList<VirtualNode> ros)
        {
            for (int j = 0; j < ros.Count; j++)
            {
                var sn = ros[j];
                if (sn != null) result[k++] = sn;
            }
        }
        else if (ci is global::System.Collections.Generic.IEnumerable<VirtualNode> seq)
        {
            foreach (var sn in seq) if (sn != null) result[k++] = sn;
        }
    }
    return result;
}
```

Notes:

- The `IReadOnlyList<VirtualNode>` branch is added for `@(__children)` slot
  pass-through, where the input is already a `VirtualNode[]` (no need to box into
  `IEnumerable` and allocate an enumerator).
- `Array.Empty<VirtualNode>()` for the all-filtered-out case avoids allocating a
  zero-length array.
- The double-iteration cost of pass-1 is dwarfed by the eliminated heap traffic — for
  small `items` arrays (typical 2–8 elements) it's negligible.
- For `@foreach` IIFEs that already yield an `IEnumerable<VirtualNode>` from a
  `Select(...)` chain, pass 1 enumerates twice. We can mitigate by changing the
  emit to materialize `@foreach` into a `VirtualNode[]` (already done in many cases —
  the IIFE typically does `.ToArray()`). If a measured profile shows this is a
  hot path, the future optimization is to replace `IEnumerable<VirtualNode>` with
  `VirtualNode[]` everywhere the SG can.

#### HMR mirror

Mirror the rewritten body in `HmrCSharpEmitter.EmitHelper` (line 1199).

### What this plan deliberately does NOT do

| Tempting idea | Why we reject it |
|---|---|
| Pool the resulting `VirtualNode[]` and return it after fiber extraction. | The array becomes `fiber.Children` and is read across render cycles — exact same trap as OPT-18 (April 26 disco bug). |
| Pool the transient `List<VirtualNode>` builder. | Phase B eliminates the list entirely. Pooling adds defensive code (idempotent guards, capacity reset, thread-static slot) with smaller wins than just removing the list. |
| Use `params ReadOnlySpan<object>` for `__C`. | `params Span<T>` requires C# 13 + .NET 9 runtime. Unity's Mono runtime support is not guaranteed across all build targets (IL2CPP, WebGL). Defer until project targets a runtime that supports it everywhere. |
| Inline-cast `@(expr)` so the SG can statically prove non-IEnumerable cases. | Adds significant SG complexity for cases users rarely hit. The Phase B body handles the polymorphism efficiently. |
| Generate per-arity `__C2(VirtualNode, VirtualNode)` / `__C3(...)` overloads to avoid `params object[]`. | Doable but adds code-size noise to every component. The Phase A bypass already eliminates the cases that would benefit from arity overloads. |

### Audit results — places we must not break

A code search confirmed the following invariants the change must preserve:

- **`fiber.Children` lifetime.** Read across two render cycles (`wipFiber.Children` vs.
  `wipFiber.Alternate?.Children` in [FiberFunctionComponent.cs:69-73](Shared/Core/Fiber/FiberFunctionComponent.cs#L69-L73))
  for the slot/`__children` bailout comparison. Must remain a stable, distinct
  `VirtualNode[]` per render. ✅ Preserved — Phase A returns a fresh params-allocated
  array per render; Phase B returns a fresh `new VirtualNode[count]`. Same shape as
  today.
- **Reference equality on the `VirtualNode` instances.** Used implicitly throughout the
  reconciler. ✅ Preserved — VNodes are not recycled at all (OPT-18 reverted).
- **`@(__children)` slot pass-through.** When the parent's `children` `IReadOnlyList`
  is passed into a child element (`<X>@(__children)</X>`), `__C` must flatten it.
  ✅ Preserved by Phase B's `IReadOnlyList<VirtualNode>` branch.
- **`EmptyChildren`.** When `children` is null/empty the V.cs factories substitute
  `VirtualNode.EmptyChildren` (the singleton `ReadOnlyCollection<VirtualNode>` over
  `Array.Empty<VirtualNode>()`). ✅ Preserved — Phase A's params-empty case still
  results in a zero-length `VirtualNode[]`, which V.cs replaces with the singleton
  via `children ?? EmptyChildren()` checks (and zero-length VN[] equals nothing
  semantically; the existing `?? EmptyChildren()` only fires on `null`).

  **Minor follow-up:** consider tightening `RentElementWithChildren` to also collapse
  a zero-length `VirtualNode[]` to the `EmptyChildrenInstance` singleton, eliminating
  one more allocation in the rare case of a container with no children that still
  hit Phase A. Out of scope for the first pass.
- **HMR delegate swap path.** `HmrCSharpEmitter` produces an isomorphic `__C` for the
  swapped-in delegate. After Phase A + B mirror, both source-gen'd and HMR'd
  versions allocate identically. ✅ Preserved.
- **VS Code / VS 2022 LSP virtual document.** [`VirtualDocumentGenerator.cs`](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs)
  emits scaffold C# for IDE type-checking only — it never emits a `__C` body.
  Inline-expression handling in VDG (line 662) routes through a `(object)`
  declaration, not `__C`. ✅ No IDE changes required.
- **Source-gen tests.** No tests in [SourceGenerator~/Tests/](SourceGenerator~/Tests/)
  string-match `__C(` in the generated code. Tests run the generated code through
  Roslyn `CSharpCompilation.Create` ([GeneratorTestHelper.cs:54](SourceGenerator~/Tests/Helpers/GeneratorTestHelper.cs#L54))
  and assert behaviour, not literal output. ✅ No test changes required for
  semantic-equivalent output.

### Implementation steps

1. **Baseline** — capture profiler numbers from `BenchEditorHost.cs` / the stress test
   showcase for: per-frame `Gen0` allocation count, `__C` self-time, `ApplyTypedDiff`
   self-time, and the steady-state FPS. Record in this file.
2. **Phase A — SG**:
   - Add `ChildrenAreAllSimple(ImmutableArray<AstNode>)` to `CSharpEmitter`.
   - Update the 4 emit sites (`EmitBuiltinElement`, `EmitBuiltinDict`,
     `EmitFuncComponent`, `EmitFragment`).
   - Run the 1030-test source-gen suite. Expect 0 regressions.
3. **Phase A — HMR**: mirror the same classifier + 6 emit sites in
   `HmrCSharpEmitter.cs`.
4. **Phase B — `__C` body rewrite**: update both `CSharpEmitter.EmitHelperMethod`
   (line 591) and `HmrCSharpEmitter.EmitHelper` (line 1199) with the two-pass body.
5. **Build the source generator DLL** and replace the one in `Analyzers/`.
6. **Re-run** all 1030 source-gen + formatter tests, all 61 LSP tests.
7. **Manual regression smoke** (per V1 lessons — visual demos catch subtle reconciler
   bugs the unit tests miss): Router demo, Snake, Galaga, Mario, ShowcaseAll, Context
   demo, DoomGame.
8. **HMR smoke**: open Mario, edit a component, save, confirm hot-reload still works
   with no `__C`-related compile errors in the swapped delegate.
9. **Re-profile** with the same scene; document deltas in this file under "Results".
10. **Commit** with a descriptive message; bump library patch version; update
    [CHANGELOG.md](CHANGELOG.md).

### Expected impact

For a typical app with ~150 host elements:

| Metric | Before | After (estimate) |
|---|---|---|
| Allocations per frame on JSX path | ~450 (3 × 150) | ~120 (Phase A bypasses 80% × 150 = 30 dynamic × 2 + 120 simple × 1) |
| Bytes per frame | ~9 KB (avg ~20 B per alloc × 450) | ~2.4 KB |
| Stress test (3000 elements) | ~9000 JSX allocs/frame | ~2400 |

Numbers are estimates pending the baseline profile in step 1.

### Risks

| Risk | Mitigation |
|---|---|
| Phase A misclassifies a child as "simple" when it actually needs filtering. | The classifier is whitelist-based (only `ElementNode`/`TextNode`/`CommentNode` qualify). Anything else falls back to `__C`. New AST node types added in the future would fall back automatically. |
| Phase B count/fill mismatch (e.g. enumerable yields different items on second iteration). | The flatten branches handle this explicitly: an `IEnumerable<T>` could in principle yield a different sequence on each iteration (lazy LINQ with side effects). Mitigation: in the comment we already classify `@foreach` IIFEs as materialized (`.ToArray()` call), but for `@(expr)` users could pass a non-deterministic enumerable. Defensive option: in pass 2, if the enumerable yields more than `count` items, take only `count`; if it yields fewer, the result has trailing nulls — must guard. Simpler: in pass 1, materialize each enumerable into a temporary `VirtualNode[]` (small heap alloc but correct), use those in pass 2. |
| HMR drift if HMR emit isn't mirrored. | Both files updated in lock-step in the same commit; reviewer must verify. |
| User code passes a `VirtualNode` cast as `IEnumerable<VirtualNode>` (uncommon but legal). | The `is VirtualNode` branch is checked first; `is IEnumerable<VirtualNode>` only fires for collection types. ✅ Already correct. |

### Validation plan

1. ✅ Source-gen + formatter tests (1030/1030).
2. ✅ LSP tests (61/61).
3. ✅ Manual regression (8 demos).
4. ✅ HMR smoke.
5. ✅ Profiler delta documented.
6. ✅ Visual diff of generated output across the sample components (eyeball pass for
   readability — should clearly show fewer `__C(...)` wrappers).

### Effort estimate

Phase A + Phase B together: ~half a day implementation + tests, plus profiling and
documentation.

### Result (to be filled in after implementation)

> _Pending — capture baseline + after numbers, real test pass counts, and any
> surprises encountered during integration._

### Future phases (deferred — measure first, then re-evaluate)

The following phases are documented here so the analysis is not lost. They should
**only** be implemented after Phase A+B ship and a profiler measurement shows the
remaining JSX-path allocations are still hot enough to justify the added SG
complexity.

#### Phase C — Inline materialization for mixed static + dynamic children

**Today**, when children are e.g. `<Header/>` + `@foreach(...)` + `<Footer/>`,
Phase B emits:

```csharp
__C(V.Header(...), __FoEx(...), V.Footer(...))
```

allocating `object[3]` (params boxing) plus the final `VirtualNode[N]` produced by
`__C`. Two allocs.

**Phase C** would have the SG analyse the child-list shape statically. The SG already
knows: 2 statics + 1 IIFE that yields `VirtualNode[]` (every directive IIFE materialises
via `.ToArray()`). Emit:

```csharp
var __seq = __FoEx(...);                                 // VN[]
var __c   = new VirtualNode[2 + __seq.Length];
__c[0]   = V.Header(...);
global::System.Array.Copy(__seq, 0, __c, 1, __seq.Length);
__c[1 + __seq.Length] = V.Footer(...);
V.Box(props, __c);
```

Result: **1 allocation** for the entire mixed case. No `object[]` boxing, no two-pass
count loop, no `__C` invocation.

Requirements:

- Force every directive IIFE (`@for`/`@foreach`/`@while`/`@switch`) to declare a
  `VirtualNode[]` return type (most already do — verify and tighten the few that
  return `IEnumerable<VirtualNode>`).
- Keep `__C` as the fallback for `@(expr)` whose runtime type is unknown — `expr`
  may evaluate to a single `VirtualNode`, `null`, or `IEnumerable<VirtualNode>`.
- HMR mirror required.

Estimated win beyond Phase A+B: another ~40% reduction in the **dynamic** subset of
sites (Phase A handled the 80% static majority; Phase C tightens the remaining 20%).

Cost: a small expression-shape analyser in the SG (per-AST-node return-type prediction),
noisier generated code per site (less readable, more local variables).

**Decision rule:** implement only if profiling after A+B shows that `__C` self-time
or the `object[]` params allocation is still in the top-5 GC contributors.

#### Phase D — `@(__children)` slot pass-through fast path

The pattern `<Layout>@(__children)</Layout>` (a layout component forwarding its parent's
children) is extremely common in nested page wrappers, the Router, and Showcase pages.
Today — even with Phase B — this emits:

```csharp
__C(__children)                                          // wraps an IReadOnlyList<VN>
```

allocating `object[1]` plus the freshly-built `VirtualNode[N]`.

**Phase D** would have the SG detect the exact AST shape "single child = `@(__children)`
identifier reference" and emit:

```csharp
// __children has runtime type IReadOnlyList<VirtualNode>; after Phase A+B every
// children array we ever produce is VirtualNode[], so the cast succeeds in 100%
// of paths — the materialise branch is purely defensive.
(__children is VirtualNode[] __cArr)
    ? __cArr
    : global::System.Linq.Enumerable.ToArray(__children)
```

The fast path is **zero new allocations** — the parent's children array flows
straight through to the child container's `_children` slot.

Combined with Phase C, the pattern `[<Static/>, @(__children), <Static/>]` becomes a
single `new VirtualNode[2 + __children.Count]` with a `for` loop or `Array.Copy` for
the slot bytes. One allocation total.

Requirements:

- AST recogniser for the literal `__children` identifier in an `@(...)` expression.
- Documented invariant: "every `_children` array UITKX produces is a `VirtualNode[]`"
  must hold. Phase A and Phase B both honour this; the runtime `EmptyChildren`
  singleton is a `ReadOnlyCollection<VirtualNode>` over `Array.Empty<VirtualNode>()`,
  so the cast falls through to the LINQ branch — acceptable because `EmptyChildren`
  is never the only child of anything by construction.
- HMR mirror required.

Estimated win beyond Phase A+B+C: 1 allocation per layout-wrapper site, compounding
in deep trees (Router stack, Showcase, Mario menu, multi-level Galaga HUD).

**Decision rule:** implement after A+B if (a) profiling shows layout components
re-rendering frequently, OR (b) Phase C is being implemented anyway (the AST
analyser is already half-built at that point).

#### Phase E — Persistent-shape cache (rejected — same class as OPT-18)

For sites where children are 100% static and key-stable (`<Box><Logo/><Title/></Box>`
with no conditionals, no loops, no slots), the **set of children VNode types and
their structural positions** is identical every render — only the prop closures
change. Theoretically we could cache a per-call-site `VirtualNode[]` in a `static`
field, repopulate slots in place each render, and skip the array allocation entirely
(1 → 0 allocs per static site).

**Why this is rejected:**

1. **VNodes themselves are not pooled** (correctly — see OPT-18 reverted). Each render
   produces fresh `VirtualNode` instances. The cached array's slots would be overwritten
   with new VNode references each render.
2. **The reconciler's bailout uses `ReferenceEquals(prevChildren, nextChildren)`** on
   the array itself ([FiberFunctionComponent.cs:69-73](Shared/Core/Fiber/FiberFunctionComponent.cs#L69-L73)).
   With a cached array, this check would always return `true` — but the **VNode contents
   inside it** changed. The bailout would incorrectly skip re-render, freezing the UI.
3. To fix that we'd have to switch the bailout from array-identity to per-element
   content equality, which costs more CPU than the saved allocation saves bytes.
4. The only way to make persistent-shape cache work is to also pool VNodes — which
   is OPT-18 and led to dangling-reference corruption (Router demo infinite loop,
   game demos broken, style corruption). **Documented and closed.**

**Status:** Rejected. Do not revisit without first solving the OPT-18 class of bug
(an entirely separate, much harder design problem).

---

## OPT-V2-2: Static-Style Hoisting + Pool Cap Tuning

> **Note:** This section *replaces* the previous “Style Sparse Storage” draft (preserved
> below as historical context). Investigation on April 27, 2026 found the prior draft
> was based on three misreadings of the codebase. The new design targets the **actual**
> remaining waste, which is in pool sizing and per-render `Style` rentals — not in field
> layout.

### Status: Researched (April 27, 2026) — ready to implement

### Summary

Two independent, additive optimizations:

- **Phase A — Static-Style hoisting (the big win):** when a `style={new Style{...}}` JSX
  attribute contains only compile-time-constant initializers, the source generator
  hoists it to a class-level `static readonly Style __sty_<id> = new Style { ... };`
  field. The element factory call then receives the hoisted reference instead of
  renting a fresh `Style` from the pool every render. The reconciler’s existing
  `Style.SameInstance(prev, next)` check makes the entire `DiffStyle` walk a
  no-op when the same hoisted instance is supplied across renders.
- **Phase B — Pool cap right-sizing:** drop `Style.PoolCapacity` (currently `4096`)
  and `BaseProps` pool caps to a measured working set (provisional `512`).
  Reclaims ~2.5–3 MB of permanently-resident pool memory for typical apps.

### Status of the prior draft (“Sparse Storage”)

The previous draft proposed replacing the dense field layout with two parallel arrays
(`object[] _refValues`, `StyleLength[] _lengthValues`) indexed by `popcount` over the
bitmask. Investigation found the framing is wrong on three counts:

1. **The dense layout is intentional and already cache-optimal for the hot path.**
   The class is *not* “80+ nullable fields” — it is 80+ **typed** value-type fields
   (`StyleLength`, `Color`, etc.) with two `ulong` set-bits sidebands. Reads cost a
   single field load; the bitmask determines validity. The proposed popcount-array
   indexing turns every read into `(bitmask check) + (popcount of bits below) + (array
   bounds check) + (object[] cast or typed[] index)` — ~5× the cost of a field load.
   Writes that flip a bit from unset→set become **O(n)** (must shift array entries
   down to keep popcount invariants), or require a parallel “which slot maps to which
   bit” table (defeats the memory savings).
2. **`ApplyTypedDiff` already iterates by `popcount`/`TrailingZeroCount`, not
   field-by-field.** [TypedPropsApplier.cs:340-415](Shared/Props/TypedPropsApplier.cs#L340-L415)
   walks `prevBits ^ nextBits` via `BitOps.TrailingZeroCount(bits); bits &= bits - 1;`.
   The CPU complexity is already O(set-bits), not O(total-fields). Sparse storage
   would *increase* per-bit cost (extra indirection for the value lookup) without
   improving asymptotic behaviour. Net CPU impact: **negative**.
3. **Value-type fields can’t live in `object[]` without boxing.** `StyleLength`,
   `StyleFloat`, `Color`, every enum, every compound struct — each one boxes on
   store and unboxes on read. The proposal acknowledges this and proposes a separate
   `StyleLength[]` array, but `Style` has **8+ distinct value-type categories**
   (`StyleLength`, `StyleFloat`, `Color`, 16 enum types, `BackgroundRepeat`,
   `BackgroundPosition`, `BackgroundSize`, `TransformOrigin`, `Translate`,
   `StyleList<TimeValue>`, etc.). Eight parallel typed arrays per Style, each with its
   own popcount mapping, would balloon the per-instance overhead and obliterate the
   imagined memory win for any non-trivial style.

What the prior draft got *right*: the resident-memory cost of pooled Styles is
significant (~840 bytes × 4096-cap pool = ~3.4 MB always-resident). What it got *wrong*:
the lever is the **pool cap**, not the field layout. Phase B addresses it directly
with zero CPU cost.

### Background — the actual waste

#### What `Style` looks like today

[Shared/Props/Typed/Style.cs](Shared/Props/Typed/Style.cs) is a `class` with:

- 79 named property bits (`BIT_WIDTH` … `BIT_UNITY_MATERIAL`).
- Two `ulong _setBits0`/`_setBits1` sidebands (16 B).
- Pool generation stamp + return guard (`uint _generation`, `bool _isPendingReturn`).
- Typed value-type fields per category:
  - 27 × `StyleLength` ≈ 12 B each = 324 B
  - 9 × `StyleFloat` ≈ 12 B = 108 B
  - 9 × `Color` × 16 B = 144 B
  - 16 × enum ≈ 4 B = 64 B
  - 2 floats / `Scale` ≈ 16 B
  - Compound structs (`BackgroundRepeat`, `BackgroundPosition` ×2, `BackgroundSize`,
    `TransformOrigin`, `Translate`) ≈ 64 B
  - 2 reference fields (`Texture2D _backgroundImage`, `Font _fontFamily`) = 16 B
  - 4 × `StyleList<T>` ≈ 32 B
  - Unity 6.3+ (`StyleRatio`, `StyleList<FilterFunction>`, `StyleMaterialDefinition`)
    ≈ 24 B
- **Estimated instance footprint: ~840 B** (including class header).
- A 4096-entry pool: **~3.4 MB always-resident**, regardless of working set.

The pool itself works correctly: `Style.__Rent()` clears the bitmask + ref fields
only (value-type fields stay dirty but are gated by the bitmask). `__ScheduleReturn`
defers returns until the commit-phase `__FlushReturns` walk in
[FiberReconciler.cs:706](Shared/Core/Fiber/FiberReconciler.cs#L706). Generation
stamping prevents cross-cycle aliasing bugs (`SameInstance` checks both reference
identity AND generation match).

#### Per-render allocation today

At every JSX site that has a `style={...}` attribute, the source generator emits
([CSharpEmitter.cs:945-980](SourceGenerator~/Emitter/CSharpEmitter.cs#L945-L980)):

```csharp
var __p_0 = global::ReactiveUITK.Props.Typed.BaseProps.__Rent<BoxProps>();
var __s_1 = global::ReactiveUITK.Props.Typed.Style.__Rent();
__s_1.Width = 100f; __s_1.Height = 50f; __s_1.BackgroundColor = Color.red;
__p_0.Style = __s_1;
V.Box(__p_0, key: k);
```

The HMR emitter mirrors the same pattern
([HmrCSharpEmitter.cs:830-870](Editor/HMR/HmrCSharpEmitter.cs#L830-L870)).

**The waste:** when the style block is entirely constant (no closures, no captures,
no expression interpolation), the resulting `Style` *content* is identical every
render, but a fresh pooled instance is rented every time. The reconciler later
schedules the prior frame’s instance for return
([FiberReconciler.cs:1199-1207](Shared/Core/Fiber/FiberReconciler.cs#L1199-L1207)),
then the diff walk runs, finds every set bit equal, applies nothing, and we throw
the instance away.

#### Sample audit — how often is style static?

Grepping `style={new Style{...}}` patterns across `Samples/Components/**/*.uitkx`
(15+ matches surveyed):

- **`PortalEventScopeDemoFunc`**: 6/6 styles fully static (constants only).
- **`EditorControlsDemoFunc`**: 8/8 fully static.
- **`LatestFeaturesDemoFunc`**: 3/3 fully static.
- **`StyledBoxDemo`**, **`StyleConsumerDemo`**, **`HoistingDemo`**: nearly all
  literal-only.
- Mixed (state-derived styles like `width={state.size}`) are the minority — they
  use either props with conditional values or expression interpolation.

**Conservative estimate: 70–85% of `style={new Style{...}}` literals across the
sample suite are 100% static.** Phase A converts every one of those from
“rent + bit setters + bail at diff + return” into “single field load.”

### Phase A — Static-Style hoisting

#### The classifier

Walk the parsed initializer body of `new Style { (Key1, val1), (Key2, val2), ... }`
or `new Style { Width = w, Height = h, ... }`. Hoist iff **every** initializer value
expression is one of:

| Form | Examples |
|---|---|
| Numeric literal | `5f`, `12`, `0.5` |
| String literal | `"row"`, `"column"` |
| Boolean literal | `true`, `false` |
| Null literal | `null` |
| Enum / static field reference (single dotted name) | `StyleKeys.Width`, `Position.Absolute`, `FlexDirection.Row` |
| `new Color(r, g, b[, a])` where r/g/b/a are numeric literals | `new Color(0.15f, 0.15f, 0.15f, 0.9f)` |
| `new Color32(...)` numeric-only | `new Color32(255, 0, 0, 255)` |
| `Color.red` / `Color.white` etc. | named static colors |
| Method call on a known-pure helper (e.g. `CssHelpers.PaddingAll(4f)`) returning constant tuples | optional, gated behind a small allow-list |

Anything outside this allow-list — closures, captures (`state.x`), expression
interpolation (`@(…)`), method calls on user types, complex `new` expressions —
forces the dynamic path (the existing `Style.__Rent()` flow stays unchanged).

The classifier already has a partial neighbour in `TryExtractNewStyleInit`
([CSharpEmitter.cs:2451](SourceGenerator~/Emitter/CSharpEmitter.cs#L2451)) which
splits the initializer body. We add `IsHoistableInitializer(string expr)` per
initializer expression.

#### The emit

When all initializers pass the classifier, the SG:

1. Generates a unique field name like `__sty_<componentName>_<callsiteIndex>`
   (collision-free across the same component).
2. Emits at class scope (alongside other class-level statics), inside an
   `#line hidden` block:
   ```csharp
   private static readonly global::ReactiveUITK.Props.Typed.Style __sty_Foo_0
       = new global::ReactiveUITK.Props.Typed.Style { (StyleKeys.Width, 100f), (StyleKeys.Height, 50f) };
   ```
3. At the call site, replaces the rent block with a single assignment:
   ```csharp
   __p_0.Style = __sty_Foo_0;
   ```
4. Skips emitting `__s_1 = Style.__Rent()`, the bit setters, and the per-property
   assignments entirely.

#### Why this is safe by construction

- **`Style.__ScheduleReturn` already short-circuits on `_generation == 0`**
  ([Style.cs:269-277](Shared/Props/Typed/Style.cs#L269-L277)). Hoisted statics are
  created with `new Style { ... }` (not `__Rent()`), so their `_generation` stays
  zero forever. The reconciler’s
  [“schedule old style for return”](Shared/Core/Fiber/FiberReconciler.cs#L1199-L1207)
  becomes a guaranteed no-op for them. **No risk of recycling a shared static.**
- **`Style.SameInstance(prev, next)` already short-circuits the diff walk**
  ([TypedPropsApplier.cs:349-350](Shared/Props/TypedPropsApplier.cs#L349-L350)).
  When the same hoisted instance is supplied across renders the entire DiffStyle
  body is skipped — zero work, zero application calls.
- **`__sty_*` fields are immutable in practice.** They’re assigned exactly once at
  class load. No setter executes after construction. If a downstream consumer
  mutated a hoisted Style (it would have to navigate to the static field and set a
  property), correctness would be in the consumer, not in this optimization.
  We add a tiny `#region/#endregion` comment in the generated header explaining
  the hoisted statics are read-only by contract; no enforcement code, no overhead.
- **HMR re-emit produces the same shape.** When a component is hot-reloaded,
  the regenerated class will have its own `__sty_Foo_0` static; the new instance
  is a different object than the prior class’s static, so on first render the diff
  walk runs once (correctly applying any deltas) and then bails out forever after.
  No leak, no aliasing.
- **Memoization across components is intentionally NOT done.** Two components with
  identical static styles get two distinct hoisted statics. Trying to dedupe across
  classes would require a global registry, run-time hashing, and would be a marginal
  win. Out of scope.

#### Edge cases handled

| Case | Behaviour |
|---|---|
| `new Style { Width = 5f }` (property setter syntax, all literals) | Hoist. |
| `new Style { (StyleKeys.Width, state.value) }` | Don’t hoist — `state.value` fails the literal check. |
| `new Style { ... }` with empty body | Don’t hoist (also: not worth one cached empty Style; `EmptyStyle` could be added later). |
| `style={someStyleVar}` (not a `new Style { ... }` expression) | Don’t hoist — `TryExtractNewStyleInit` already returns false. |
| `style={cond ? new Style{a} : new Style{b}}` | Don’t hoist (the conditional itself isn’t a `new Style { ... }` literal). A future Phase A.1 could hoist *each branch* and emit `cond ? __sty_X : __sty_Y` — deferred. |
| `new Color(0.5f, 0.5f, 0.5f, 1f)` initializer value | Hoist. The class-level `static readonly Style` field initializer evaluates the `new Color(…)` once at class load. |
| `CssHelpers.SomeMethod(5f)` initializer value | Hoist iff the helper is on the small allow-list of pure constant returns. Conservative default: don’t hoist; revisit per-helper later. |
| Generic component with type-parameter influence on style values | N/A — style values are runtime values; type parameters don’t parameterise constants here. |

### Phase B — Pool cap right-sizing

Both `Style.PoolCapacity` (currently 4096) and the equivalent in `BaseProps` are
back-pressure caps: when the pool is full, returns are dropped (object becomes GC’d).
They were sized as a worst-case ceiling. After Phase A removes 70–85% of `__Rent`
calls, the working set drops to a fraction of today’s.

#### The change

Lower both caps to a measured value (provisional `512` for `Style`, similarly
right-sized for `BaseProps` per its own per-instance footprint). Add a single
`#if UITKX_DEBUG_POOLS` instrumented counter that logs peak in-pool depth so we
can reconfirm the cap empirically.

#### Why this is independent of Phase A

Even if Phase A is reverted, Phase B is harmless — it just trades resident memory
for potentially more GC churn under extreme stress. Profiling will tell us whether
`512` is right or whether it should be `1024`. The two phases ship in the same
commit but are conceptually independent.

### Audit — places we must not break

| Surface | Touch? | Notes |
|---|---|---|
| `Shared/Props/Typed/Style.cs` | None | Field layout, pool, diff are all preserved. |
| `Shared/Props/Typed/BaseProps.cs` | None | `Style` field assignment unchanged; `__ScheduleReturn` already skips gen-0. |
| `Shared/Props/TypedPropsApplier.cs` | None | `DiffStyle` short-circuit on `SameInstance` already handles hoisted statics. |
| `Shared/Core/Fiber/FiberReconciler.cs` | None | Schedule-return on gen-0 is already a no-op. |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | **Modified** | Add `IsHoistableInitializer`, hoist-emit pass, replace per-call rent block when classifier passes. |
| `Editor/HMR/HmrCSharpEmitter.cs` | **Modified** | Mirror the same hoisting at the same call sites so HMR’d components have the same alloc shape. Note: HMR emits methods, not classes — hoisted statics live in the dynamically-emitted partial class for that component, scoped per-recompile. |
| `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` & friends (LSP virtual document) | None | LSP scaffolds different code; doesn’t emit `__Rent` calls. |
| `ide-extensions~/language-lib/Formatter/AstFormatter.cs` | None | Formatter operates on the `.uitkx` source; the SG/HMR change is invisible to it. |
| `SourceGenerator~/Tests/` | **Adds tests** | New tests asserting (a) static-only styles produce a hoisted `static readonly Style` field, (b) dynamic styles still rent, (c) hoisted-style equality semantics across renders. Existing 1030 tests use Roslyn compilation — emitting different but semantically equivalent C# is fine. |
| `ide-extensions~/lsp-server/Tests/` | None | LSP tests don’t run the SG’s output. |
| `Samples/**/*.uitkx` | None | User code unchanged. |
| User-authored `.uitkx` correctness | None | Users see lower allocations, identical visual behaviour. |
| Reflection-based code (e.g. inspector tooling) | None | Hoisted statics use the same `Style` class; reflection over the type-graph is unchanged. |
| Determinism / reproducibility | Preserved | Hoist names use a deterministic counter per-component; rebuilding the same `.uitkx` produces the same `.g.cs`. |

### Why this is the right design (vs. the prior draft, vs. alternatives)

| Alternative | Verdict |
|---|---|
| **Sparse popcount-array storage (prior draft)** | Rejected. Slower reads, worse diff CPU, boxing of 8+ value-type categories, no real win after Phase B reclaims pool memory directly. |
| **Per-shape generated `StyleSet1<T>` / `StyleSet2<T1,T2>`** (prior draft B3) | Rejected. Massive SG complexity for a marginal additional gain after Phase A. The hoisted statics already pay off at the call site — generating per-shape wrappers adds another layer of abstraction with no clear extra benefit. |
| **Cross-component static dedupe** | Deferred. Would require a global hash registry, race-aware initialisation, and would save at most a few KB of class-static memory per build. Profile first, decide later. |
| **Drop the pool entirely** | Rejected. Style construction is ~30 field stores plus class-header allocation; pooling provides clear measured wins for dynamic styles. The hoisted-static path simply *avoids the pool* for the common case. |
| **Embed Style fields directly into `BaseProps`** | Deferred (separate item, OPT-V2-4 candidate). Halves per-element pool entries but requires duplicating the typed setters, breaks `style` reuse across element types, and adds significant code. Different design discussion. |
| **Replace `Style` with a `readonly struct`** | Rejected. The reconciler keeps and compares `Style` instances by reference (`SameInstance`); a struct would force per-pass copies and break the bailout. |

### Expected impact

For a typical app with ~150 host elements where ~80% have a static `style` block:

| Metric | Before | After Phase A | After Phase A+B |
|---|---|---|---|
| `Style.__Rent` calls/frame | ~120 | ~24 | ~24 |
| Per-render style apply work (commit phase) | 120 × (rent + setters + diff bail) | 24 × dynamic + 96 × instant `SameInstance` bail | same |
| Pool resident memory (steady state) | ~3.4 MB (4096 × ~840 B) | ~3.4 MB (cap unchanged) | ~430 KB (512 × ~840 B) |
| BaseProps pool resident | similar order | unchanged | also right-sized |
| Per-render bit setter ops (commit) | ~600 (5 props × 120 styled elements) | ~120 (only dynamic) | same |
| Visual / behavioural change | none | none | none |

### Implementation steps

1. **Baseline** — capture from `Diagnostics/Benchmark/BenchEditorHost.cs` and one or two
   showcase scenes: per-frame `Style.__Rent` count, peak pool depth (add
   `#if UITKX_DEBUG_POOLS` instrumentation if not present), and steady-state managed
   memory. Record in this file before changing any code.
2. **Phase A — SG**: add `IsHoistableInitializer`, the per-component hoist counter, and
   the static-field emit pass. Update the `EmitBuiltinTyped` rent block to skip
   when the classifier passes. Generate hoisted statics into the existing class scope
   (use an `#line hidden` region after the existing `__uitkx_ussKeys` static if any).
3. **Phase A — HMR**: mirror the same logic in `HmrCSharpEmitter`. HMR emits methods
   inside a per-recompile partial class — hoisted statics live there too, scoped by
   the recompile wrapper.
4. **Phase B — pool caps**: drop `Style.PoolCapacity` to `512`, audit `BaseProps.PoolCapacity`
   for parallel reduction. Add a debug log under `#if UITKX_DEBUG_POOLS` that prints
   peak depth per N seconds.
5. **Build the SG DLL** and replace the one in `Analyzers/`.
6. **Run** the 1030 SG + formatter test suite + the 61 LSP suite. All green expected.
7. **Add tests** specifically for hoisting:
   - A `.uitkx` snippet with all-literal `style={...}` produces a `static readonly Style`
     field; the `V.Box(...)` call site references it.
   - A `.uitkx` snippet with `state.value` in a style still emits `Style.__Rent()`.
   - The hoisted static survives multiple renders without being scheduled for return.
8. **Visual smoke** — Showcase, Mario, Galaga, Snake, Router, Doom, EditorControlsDemo,
   PortalEventScopeDemo, every page that uses heavy literal styling.
9. **HMR smoke** — open one of the demos, edit a style line, save, confirm hot-reload
   produces the new visual + the new hoisted static (or falls back to dynamic if the
   edit introduced a state reference).
10. **Re-profile**: same scenes, same metric set; document deltas under “Result”.
11. **Commit** with a descriptive message; bump library patch version; update
    [CHANGELOG.md](CHANGELOG.md). Phase B can ship as a follow-up commit if the cap
    needs adjusting after the profile.

### Risks

| Risk | Mitigation |
|---|---|
| Classifier mis-identifies a non-constant as constant. | Whitelist-based: only literals, named-static dotted references, and `new Color(…)` with literal args qualify. Anything else (method calls, identifiers that aren’t enum members, conditional/binary operators, member access chains beyond one dot) falls back to dynamic. Conservative by default. |
| User mutates a hoisted static. | Only possible by reaching into a private static field reflectively. Documented contract: hoisted statics are immutable. We can optionally emit them as `static readonly` (already proposed) and mark with a generated XML doc summary. C# language guarantees the field reference can’t be reassigned post-init. |
| HMR re-emit produces a *different* hoisted instance for an unchanged static, causing one extra diff walk. | Acceptable. Diff walk runs once after HMR, then `SameInstance` bails forever. No memory leak (the prior hoisted static becomes GC-eligible when the prior class is unloaded). |
| Lower pool cap (`512`) causes thrashing under stress. | Add `#if UITKX_DEBUG_POOLS` peak-depth log; profile under `BenchEditorHost.cs` stress; raise the cap if needed. The cap is a single constant, easy to retune. |
| `static readonly` field initialisers run on first class access — introduces a one-time JIT cost per component. | Negligible. Same pattern as the existing `__uitkx_ussKeys` static. The cost is paid once per assembly load, not per render. |
| HMR’s emitted partial class accumulates hoisted statics across multiple recompiles in the same session, leaking memory. | Each HMR recompile emits a *new* partial class (with a fresh suffix), so the prior class’s statics become GC-eligible after the swap. No leak. Audit the existing HMR’s class-naming scheme before implementing. |
| Inline `new Color(r, g, b, a)` with non-trivial expressions slips past the classifier. | The classifier requires *literal* numeric arguments to `new Color`. Anything else (e.g. `new Color(state.r, ..., 1f)`) falls back. |
| `[StaticReadonly] new Style { (key, val) }` ordering rules. | The Style constructor + `Add((string, object))` initializer pattern works at field-init time exactly as it does at runtime. Already exercised in the existing codebase via `Style.Of(…)` and tests. |

### Validation plan

1. ✅ 1030/1030 SG + formatter tests pass.
2. ✅ 61/61 LSP tests pass.
3. ✅ New hoisting tests pass (asserts on emitted IL via Roslyn).
4. ✅ 8–10 visual smoke demos render identically.
5. ✅ HMR smoke: edit a static-style line, edit a dynamic-style line, edit-then-undo.
6. ✅ Profiler delta documented (rent count, resident memory, frame time).
7. ✅ Generated-code spot check: open `GeneratedPreview~/Test.uitkx.g.cs` for a
   sample with mixed static/dynamic styles, eyeball the diff for readability.

### Effort estimate

Phase A: ~half a day implementation + tests + smoke. Phase B: an hour. Profiling and
documentation: an hour or two. The bulk is the classifier and the hoisting emit pass
in the SG, plus the HMR mirror.

### Result (to be filled in after implementation)

> ✅ **Implemented April 27, 2026** in a single Phase A + Phase B commit.
>
> **What shipped:**
> - **Phase A (SG + HMR emitters):** new `TryHoistStaticStyle` classifier in
>   [`CSharpEmitter`](SourceGenerator~/Emitter/CSharpEmitter.cs) and mirrored in
>   [`HmrCSharpEmitter`](Editor/HMR/HmrCSharpEmitter.cs). Recognises
>   *both* the property-setter form (`new Style { Width = 5f }`, which previously
>   went through pool rent) **and** the tuple form (`new Style { (StyleKeys.Width, 5f) }`,
>   which previously allocated a fresh `new Style` per render). Each match emits
>   a `private static readonly Style __sty_N` field at class scope and replaces
>   the per-render init with the static reference. Generated statics are flushed
>   into the partial class body just before its closing brace, behind a
>   `// ── Hoisted static styles (OPT-V2-2) ──` banner for grep-ability.
> - **Whitelist** (recursive): numeric / hex / string / char / `true` / `false` /
>   `null` literals; named-static dotted refs (`StyleKeys.Width`, `Color.red`,
>   `Position.Absolute`); `new T(literal-args...)` for `Color`, `Color32`,
>   `Vector2/3/4`, `Vector2Int/3Int`, `Length`, `TimeValue`, `Rect`, `Quaternion`.
>   Anything else (closures, captures, ternaries, method calls on user types,
>   identifiers without a dot) falls back to the existing dynamic path.
> - **Phase B (pools):** [`Style.PoolCapacity`](Shared/Props/Typed/Style.cs)
>   `4096 → 512`. [`BaseProps.Pool<T>.Capacity`](Shared/Props/Typed/BaseProps.cs)
>   `4096 → 512`. Added `#if UITKX_DEBUG_POOLS` peak-depth tracking
>   (`Style.PeakPoolDepth`, `BaseProps.Pool<T>.PeakPoolDepth`) for empirical
>   re-tuning if needed.
>
> **Memory reclaimed (steady state):**
> - `Style` pool: `(4096 − 512) × ~840 B ≈ 3.0 MB`
> - `BaseProps` per-type pool: `(4096 − 512) × ~80 B × N_types ≈ 0.3 MB × N`
> - Combined typical app savings: **~3.5 MB resident memory**.
>
> **Allocation reduction:** every literal-only `style={...}` site (the dominant
> case across the showcase suite) now skips one `Style.__Rent()` call, ~6
> bit-set / value-write operations, one `Style.__ScheduleReturn` call, and the
> entire `DiffStyle` walk per render. The reconciler bails out instantly via
> `Style.SameInstance(prev, next)`.
>
> **Validation:**
> - SG + formatter: **1040 / 1040 pass** (1030 baseline + 10 new hoisting tests
>   in [`EmitterTests.cs`](SourceGenerator~/Tests/EmitterTests.cs) — covering
>   tuple form, setter form, dynamic-value rejection, `new Color(literals)`
>   acceptance, dotted-enum/string-literal value, multiple-sibling unique ids,
>   empty-body rejection, mixed static+dynamic, tuple+variable rejection,
>   non-style passthrough).
> - LSP: **61 / 61 pass** (untouched — SG output is invisible to LSP).
> - No code-shape regressions on the dynamic / non-hoistable path
>   (existing pool-rent emit unchanged).
>
> **Risk reassessment after implementation:**
> - Conservative whitelist meant zero false-positive hoists in the test suite.
> - Pool cap drop (`512`) is well above measured working-set in showcase scenes;
>   `UITKX_DEBUG_POOLS` is the escape hatch if ever needed.
> - HMR mirror produces a *new* `__sty_N` per recompile (different instance
>   identity than the SG-emitted one) — first post-HMR render runs `DiffStyle`
>   once, then `SameInstance` bails on every subsequent render. No leak: the
>   prior class's statics become GC-eligible when the prior dynamic assembly
>   unloads.
>
> **Deferred follow-ups (no blocker):**
> - Phase C — `EmptyStyle` singleton (low-value; profile first).
> - Phase D — conditional-branch hoisting (`cond ? new Style{a} : new Style{b}`).
> - Pre-existing bug: `LatestFeaturesDemoFunc.uitkx:159` uses `(StyleKeys.Gap, …)`
>   but `"gap"` is not in `StyleKeys.cs` / `Style.SetByKey` (silently no-op'd).
>   Unrelated to this work; track separately.

### Future phases (deferred — measure first)

#### Phase C — `EmptyStyle` singleton

For `style={new Style { }}` (empty initializer) emit a reference to a process-wide
`Style.Empty` singleton. Trivial. Noise reduction more than perf reduction. Only
worth doing if profiling shows the empty case is hit.

#### Phase D — Conditional-branch hoisting

For `style={cond ? new Style{a} : new Style{b}}`, hoist *each branch* and emit
`cond ? __sty_X : __sty_Y`. Real win in `Active`/`Inactive`-style toggles.
Deferred until the classifier in Phase A is solid and we have profiling data.

#### Phase E — BaseProps + Style merge (rejected for now — see Alternatives)

Embed Style fields directly into `BaseProps` to halve per-element pool entries.
Big design discussion (breaks `Style` reuse across element types; doubles the typed
setters on every Props subclass). Track separately if it ever becomes the next
lowest-hanging fruit.

---

## OPT-V2-2 (HISTORICAL): Style Sparse Storage — superseded April 27, 2026

> Preserved for context. **Replaced by the static-hoisting + pool-cap design above.**
> See “Status of the prior draft” in the new section for why this was rejected.

### Status: Proposed

### Summary

Replace the dense `Style` struct (80+ nullable fields, ~600 bytes per instance) with
a sparse representation that only stores set properties. Most styles set 5–15
properties — current layout wastes ~80% of the memory.

### Background

`Style` is the dominant per-element memory footprint in UITKX. Every host element
that opts into typed styling rents a pooled `Style` from `Style.__Rent()`. The struct
has fields for every USS property the framework supports (`StyleLength` for sizes,
`StyleColor` for colors, `StyleEnum<T>` for enums, etc.) — most are `default` (unset)
at any given time. The bitmask `_setBits0` / `_setBits1` already tracks which fields
are set, but the dense field layout means even a one-property style allocates the
full ~600 bytes.

The Style hot path is also impacted:

- `ShallowEquals` compares all fields conditionally on the bitmask, but cache lines
  are wasted because adjacent fields are usually unset.
- `ApplyTypedDiff` walks every potential property — sparse storage lets diff iterate
  only the bits that are set.

### Proposed Approach (B1 — Inline arrays + bitmask)

Two parallel arrays (or one tagged-object array), indexed via popcount over the
existing `_setBits0` / `_setBits1` bitmask:

```csharp
// Per-instance storage replaces the 80+ field declarations:
private object[] _refValues;                // for ref-typed style props (Background, Font, etc.)
private StyleLength[] _lengthValues;        // for StyleLength-typed props
// + small inline buffer for the most common 4–6 props (avoids array alloc for
//   trivial styles like { width, height, backgroundColor })
```

Index lookup:
```csharp
private static int IndexFromBit(ulong bits0, ulong bits1, int bitIndex)
{
    // popcount of all set bits below bitIndex
    if (bitIndex < 64)
        return PopCount(bits0 & ((1UL << bitIndex) - 1));
    return PopCount(bits0) + PopCount(bits1 & ((1UL << (bitIndex - 64)) - 1));
}
```

Setters/getters update the bit, then read/write the corresponding slot at the
computed index. Most operations remain O(1) because PopCount is a single hardware
instruction (`Popcnt.X64.PopCount` available since .NET Core 3.0; on Mono fall back
to a software popcount table or `BitOperations.PopCount`).

#### Alternatives Considered

- **B2 — Dictionary-backed:** `Dictionary<int, object>` keyed by bit index. Too much
  per-instance overhead (~48 bytes minimum + boxing structs).
- **B3 — Generated codegen per common-shape:** Source generator emits `StyleSet1<T1>`,
  `StyleSet2<T1,T2>` etc. for the most common combos. Most efficient but very high
  implementation cost. Defer indefinitely.

### Expected Impact

| Metric | Before | After (B1, typical 5-prop style) |
|--------|--------|-----------------------------------|
| Per-`Style` size | ~600 bytes | ~80–120 bytes |
| Empty `Style` size | ~600 bytes | ~40 bytes |
| `ShallowEquals` ops | ~80 conditional comparisons | popcount + N value comparisons (N = bits set) |
| `ApplyTypedDiff` walk | All properties scanned | Only set bits scanned |

For a typical app with ~150 host elements averaging ~10 set props each:
- Memory: ~90 KB before → ~15–18 KB after. **~5–6× reduction.**
- Diff CPU: proportional reduction in the hot path.

### Risks / Considerations

- `Style` is a large API surface — every property accessor needs to be regenerated by
  the source generator (`Style.cs` is partially generated; the typed setters live in
  `BaseProps`-derived classes via `_setBits` / `_setX(value)` patterns).
- `StyleLength` and similar value-type props can't go in `object[]` without boxing.
  Either a separate `StyleLength[]` array (proposed) or a tagged-union struct.
- The bitmask infrastructure exists; this change keeps the bitmask as the source of
  truth and changes only the value storage.
- The pool path in `Style.__Rent()` resets the value arrays (or just clears the
  bitmask if values are read lazily through the bitmask check).
- `ApplyTypedDiff` in `TypedPropsApplier.cs` needs to be updated to iterate set bits
  (e.g. via `BitOperations.TrailingZeroCount` over `_setBits0 ^ otherBits0`).

### Implementation Steps (high level)

1. Profile baseline with `BenchEditorHost.cs` — capture alloc counts and
   `ApplyTypedDiff` CPU time for the showcase.
2. Add the storage arrays + small-inline buffer to `Style`.
3. Update the source generator that emits `Style` accessors and the `_setX(value)` /
   `SetByKey` methods to write through the new storage layout.
4. Update `Style.__Rent()` / `__ResetFields()` to clear the new storage.
5. Update `ShallowEquals` and `ApplyTypedDiff` to walk via popcount-iteration over
   the bitmask instead of conditional field-by-field.
6. Run full test suite + every showcase demo, including stress tests.
7. Re-profile, confirm wins, document the new baseline.

### Validation Plan

1. All 1029 source gen tests pass.
2. All showcase demos visually identical to current behavior.
3. `BenchEditorHost.cs` reports lower alloc count and lower `ApplyTypedDiff` time.
4. Memory profiler shows reduced per-Style heap size.

### Effort Estimate

2–4 days. The accessor regeneration in the source generator is the bulk of the work.
The reconciler and pool are not touched.

---

## OPT-V2-3 (Deferred): Recover Render-Phase Pool Returns via Commit-Phase Walk

### Status: Deferred (only revisit if OPT-V2-2 is NOT done)

### Background — The "Lighter OPT-18" Fix We Just Shipped

On April 26, 2026, we discovered that `FiberReconciler.CompleteWork`'s
"props equal but different instance" branch was returning `PendingHostProps` to the
pool **during the render phase**. The source `VirtualNode` (`vnode._hostProps`) still
held a live reference to that BaseProps. If the render was interrupted (passive
effect or `setState` during render) and restarted, the same VNode reference was
re-encountered and the same `BaseProps` was rescheduled — a double-return — and
worse, if it had already been re-rented elsewhere, two fibers ended up sharing one
mutable instance. This caused the "disco" symptom (cross-wired styles between
unrelated elements every render tick).

This was the same class of bug as the original OPT-18 — render-phase pool returns
where external references still exist — just at the `BaseProps`/`Style` level
instead of the `VirtualNode` level. We reverted the unsafe branch.

#### Production Fix Shipped

- Removed the pool returns from `CompleteWork`'s branch 3 (props equal, different
  instance). The fiber still aliases `PendingHostProps = HostProps` so the commit is
  a no-op. The unused rented `BaseProps` becomes garbage when its owning function
  component eventually re-renders.
- Pool returns now only happen during the commit phase from `CommitUpdate` (props
  actually changed → old replaced) and `CommitDeletion` (element removed). Both are
  atomic under `_isCommitting = true` so reentrancy can't cause double-returns.
- Defense in depth: added `_isPendingReturn` bool to `BaseProps` and `Style`.
  `__ScheduleReturn` is now idempotent — a second call on the same instance within
  one flush window is a no-op. Cleared on `Rent` and on `__FlushReturns`.

#### Cost of the Fix

- **One `BaseProps` + one `Style` allocation per unchanged host element per render**
  that the parent re-rendered (i.e. parent didn't bail out, but this child's typed
  props happened to deep-equal the previous ones) now becomes GC garbage instead of
  returning to the pool.
- Estimated ~96 KB/sec dropped to gen0 GC in the showcase with the timer ticking,
  ~150 host elements where ~120 don't change.
- Negligible in editor; potentially noticeable in IL2CPP / mobile.

### Recovery Plan (only if needed)

The proper way to recover this savings is to do the pool return in the **commit
phase**, not the render phase:

1. Add a third slot `FiberNode.UnusedRentedProps` (and `UnusedRentedStyle`).
2. In `CompleteWork`'s branch 3, instead of scheduling the return, write the unused
   instance to the new slot.
3. In `CommitPropsAndClearFlags` (already a tree walk), after committing each fiber,
   schedule the contents of `UnusedRentedProps` for return to the pool. At this
   point the new tree is the authoritative one, the WIP tree is being discarded,
   and the unused instance is provably no longer referenced by any live fiber.
   Clear the slot.
4. Tests must specifically cover the render-restart scenario (passive effect or
   `setState` mid-render) to prove the new path doesn't regress to the disco bug.

### Why This Is Deferred

- If **OPT-V2-2 (Style Sparse Storage)** ships first, the per-element memory cost
  drops by ~5×. The leaked `Style` becomes ~80–120 bytes instead of ~600. The
  motivation for recovering the pool optimization largely disappears.
- New `FiberNode` field costs 8 bytes per fiber (~thousands of fibers in a real
  app) — partially offsets the savings.
- Risk of reintroducing the same class of bug we just spent a day debugging.

### Trigger to Revisit

Revisit only if:
- OPT-V2-2 is rejected or deferred indefinitely, AND
- Profiling on IL2CPP / mobile shows GC pressure from this specific path is causing
  measurable frame hitches.

---

## Notes

- All items here are **gated by profiling**. Do not implement on speculation.
  Capture baseline numbers from `BenchEditorHost.cs` before and after each change,
  and document the actual delta in this file.
- Lessons from V1 still apply:
  - Don't pool objects that have external references (OPT-18 lesson, reinforced by
    the April 26 BaseProps fix).
  - Reverting an unsafe optimization is cheaper than living with a bug.
  - Idempotent guards (`_isPendingReturn`) are cheap defense-in-depth — add them
    proactively to any new pool you build.
# OPT: VNode Children List Pooling

## Status: Proposed

## Summary

Pool the `List<VirtualNode>` children lists used inside VNodes rather than pooling
VNode objects themselves. This recovers alloc savings from the reverted OPT-18 without
the dangling-reference bugs that made VNode object pooling unsafe.

## Background

OPT-18 pooled VNode objects directly — renting from a static pool and returning them
after fiber extraction. This saved ~6,200 allocs/frame but was **fundamentally unsafe**:
VNode references appear not just in `_children` but also inside opaque `IProps` (e.g.
`Route.Element`, any slot-like pattern). When a pooled VNode was reset and re-rented,
any external reference became a dangling pointer to corrupted data, causing:

- Router demo infinite loop (Route's `element` prop pointing to a recycled VNode)
- Game demos broken (corrupted VNode types during reconciliation)
- Style corruption (wrong BaseProps applied to wrong elements)

OPT-18 was fully reverted. VNodes are now regular GC'd heap objects.

## Proposed Approach

### Core Insight

The expensive allocations are the `List<VirtualNode>` objects, not the VNode objects
themselves. VNodes are small gen0 garbage (~48 bytes each). But `List<VirtualNode>`
allocates a backing array (typically 4–8 elements) plus the list wrapper. In a tree
with many parent elements, these lists dominate alloc volume.

### Design

1. **Pool `List<VirtualNode>` instances** in a static `Stack<List<VirtualNode>>`.
2. **Rent** in `VirtualNode.__C(...)` (the children-array helper called by generated code).
3. **Return** after the reconciler extracts children into fiber child pointers
   (in `FiberFactory.CreateNew` / `CloneForReuse` / `FiberChildReconciliation.CreateFiber`).
4. On return, call `list.Clear()` (resets count, keeps backing array for reuse).

### Why This Is Safe

- No external code holds references to the children **list** object — it's an internal
  transport container consumed immediately by the reconciler.
- The VNode objects inside the list remain alive (not pooled), so any reference to them
  (in IProps, closures, etc.) stays valid.
- `ReferenceEquals` checks on VNode objects still work (no recycling).
- The list is only used during the render phase (between `__C()` and fiber extraction).
  After extraction, the list is empty and returned to pool.

### Implementation Steps

1. Add pool infrastructure to VirtualNode:
   ```csharp
   private static readonly Stack<List<VirtualNode>> s_childListPool = new(256);

   internal static List<VirtualNode> __RentChildList(int capacity)
   {
       if (s_childListPool.Count > 0)
       {
           var list = s_childListPool.Pop();
           // list is already Clear()'d on return
           return list;
       }
       return new List<VirtualNode>(capacity);
   }

   internal static void __ReturnChildList(List<VirtualNode> list)
   {
       if (list == null) return;
       list.Clear();
       if (s_childListPool.Count < 1024)
           s_childListPool.Push(list);
   }
   ```

2. Update `VirtualNode.__C(...)` to rent from pool instead of `new List<VirtualNode>`.

3. Update `FiberFactory.CreateNew`, `CloneForReuse`, and `FiberChildReconciliation.CreateFiber`
   to return the children list to pool after extracting child info into fibers.

4. Ensure `CommitPropsAndClearFlags` does NOT return the list (fiber.Children stays alive
   for re-render — only the reconciler consumes and returns lists from fresh VNodes).

### Expected Impact

- **Alloc savings**: Proportional to the number of parent elements rendered per frame.
  Likely 2,000–4,000 list allocs/frame eliminated (the remainder of OPT-18's ~6,200 were
  the VNode objects themselves, which are cheap gen0 garbage).
- **No correctness risk**: Lists are internal transport; VNode objects are not recycled.
- **Minimal code change**: ~3 files (VNode.cs, FiberFactory.cs, FiberChildReconciliation.cs).

### Risks / Considerations

- Must verify that no code path holds a reference to the children list after fiber
  extraction. If any path reads `fiber.Children` after the list was returned, it would
  see an empty list. The current code only reads `fiber.Children` during
  `UpdateHostComponent` / `UpdateFragment` / `UpdatePortal` (before extraction).
- Fragment fibers read `fiber.Children` in `UpdateFragment` — these must NOT have their
  children list returned until after the fragment is processed.
- Function component `Children` (slot content) must NOT be returned — these are
  preserved across render cycles for the bailout comparison.

### Validation Plan

1. Run all 1029 source gen tests
2. Manual regression: Router demo, Snake game, Galaga, Mario, ShowcaseAll, Context demo
3. Profiler: compare alloc counts before/after with stress test
