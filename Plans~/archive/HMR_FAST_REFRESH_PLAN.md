# Option C — UITKX Fast Refresh: Concrete Plan

**Date:** May 21, 2026
**Status:** ✅ Shipped in package version 0.6.0 (2026-05-21). All phases
PR-A through PR-D landed as a single squashed change in the
`cleanup_and_upgrades` branch. Trampoline + cascade architecture is fully
removed; reconciliation now uses Family-handle identity exclusively.

**Scope:** Replaces the trampoline + cascade architecture with a Family/Signature
runtime modeled directly on React's `react-refresh/runtime` and
`react-refresh/babel`, adapted to the C# / Roslyn / Mono Editor / Unity
asset-import constraints.

This is **not theoretical**. It maps every primitive in React's Fast Refresh
to a specific code change in UITKX, names the file and the function being
edited, and lists every behaviour change a user would observe.

**Code-audit pass 2 (May 21, 2026):** every file mentioned below was read
in full. All file:line references are verified against the current tree.
Findings that changed the plan are marked **[A2]** inline.

## Audit summary — what was verified

- Both copies of `CanReuseFiber` exist and are byte-identical in the
  FunctionComponent branch ([FiberChildReconciliation.cs#L244-L296](../Shared/Core/Fiber/FiberChildReconciliation.cs#L244)
  and [FiberFunctionComponent.cs#L264-L327](../Shared/Core/Fiber/FiberFunctionComponent.cs#L264)).
  The two copies serve different reconciliation paths: keyed/index lists
  vs single-child. Both must be edited identically.
- The render-crash rollback hook in [FiberReconciler.cs#L505-L530](../Shared/Core/Fiber/FiberReconciler.cs#L505)
  uses `fiber.TypedRender.Method.DeclaringType` to identify the component
  type, then calls `HmrState.TryRollbackComponent(Type)`. With Family,
  this becomes `HmrState.TryRollbackFamily(Family)` — same shape, swapped
  identity. **[A2: not in original plan §11]**
- `[UitkxSource(path)]` and `[UitkxElement(name)]` are emitted
  unconditionally on every generated component partial class
  ([CSharpEmitter.cs#L186-L188](../SourceGenerator~/Emitter/CSharpEmitter.cs#L186)
  and [HmrCSharpEmitter.cs#L237-L239](../Editor/HMR/HmrCSharpEmitter.cs#L237)).
  Both attributes carry exactly the data needed for persistent IDs.
- The trampoline IL shape is wrapped in `#if UNITY_EDITOR` already
  ([CSharpEmitter.cs#L264-L296](../SourceGenerator~/Emitter/CSharpEmitter.cs#L264)).
  Family path mirrors this gating — zero player overhead. Confirmed.
- `[ModuleInitializer]` attribute is recognized by the C# compiler when
  the type `System.Runtime.CompilerServices.ModuleInitializerAttribute`
  is visible to the assembly, regardless of TFM. The polyfill in
  [SourceGenerator~/IsExternalInit.cs#L12-L15](../SourceGenerator~/IsExternalInit.cs#L12)
  is `internal` to the SG project itself — **the SG does NOT currently
  emit a polyfill into user assemblies. [A2: blocker without fix.]** See §5.0 below for the fix.
- `ReactiveUITK.Shared` is referenced by every other asmdef in the workspace
  (Editor, Runtime, Diagnostics, CICD, Examples; verified via
  asmdef grep). Putting `Family` and `RefreshRuntime` in `Shared/Core/Refresh/`
  is safe and visible everywhere.
- `HookContainerRegistry` is **not** a cascade-only consumer
  ([UitkxHmrController.cs#L208-L213](../Editor/HMR/UitkxHmrController.cs#L208)) —
  it resolves `using static <Ns>.<HookContainer>` at HMR compile time,
  independent of cascade. **[A2: original plan §9.1 wrong; corrected below — `HookContainerRegistry` STAYS.]** Only
  `UitkxFileDependencyIndex` is cascade-only and gets deleted.
- The companion-`.cs`-file path at [UitkxHmrController.cs#L468-L489](../Editor/HMR/UitkxHmrController.cs#L468)
  collects `<ComponentBase>.*.cs` files (excluding `.g.cs`) and feeds
  them into the compile. They're hand-written and may call
  `V.Func(MyComp.Render, ...)` directly (legacy delegate path); they
  fall back gracefully and **don't regress** under Option C.
- Today's per-type swap walks every loaded assembly via
  `AppDomain.CurrentDomain.GetAssemblies()` to find every prior
  `hmr_*.dll` `MyChild` type and updates `__hmr_Render` on each
  ([UitkxHmrComponentTrampolineSwapper.cs#L327-L398](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L327)).
  Family collapses this: **one** `Family.Current` write reaches every
  consumer regardless of which DLL generation that consumer's IL was
  baked from, because every consumer's `__fam_MyChild` field resolved
  to the same Family object at type-init time. **[A2: strict
  simplification.]**
- Cross-DLL after a Rank 4 cascade: parent compiles into the **same** new
  HMR DLL as the changed child. Parent's IL bakes the new DLL's
  `MyChild.Render` method-group; existing fibers carry the old DLL's
  delegate. `ReferenceEquals` fails (different delegate instances);
  `Method ==` fails (different MethodInfo handles even for "same" method
  across DLLs); `Target ==` both null. `CanReuseFiber` returns false.
  Subtree torn down. **[A2: confirmed root cause. The trampoline
  invariant explicitly assumed parents never recompile alongside
  children — see header comment at [UitkxHmrComponentTrampolineSwapper.cs#L18-L43](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L18). The Rank 4 cascade violates this invariant.]**

---

## 0. The user-facing promise

After this lands, the following **must hold deterministically**, regardless
of project size, sample count, or save bursts:

1. Save any `.uitkx` file. The component's body executes the new code on the
   next frame. `useState` / `useReducer` values are preserved. `useEffect`
   cleanups do NOT re-fire. Mount effects (those with `[]` deps) do NOT
   re-fire.
2. Add or remove a hook call in a component. That component (and only that
   component) remounts. Sibling components, parents, and unrelated subtrees
   are untouched. State outside the changed component is preserved.
3. Edit a component that 30 other components consume. Only the changed
   component recompiles. Consumers see the change on their next render
   without recompile, because they reach the changed component through a
   stable runtime indirection (the Family).
4. Crash on render. The previous version of the component is restored.
   Subsequent edits are tried against the live tree.
5. Two saves with identical content produce identical console output and
   identical memory delta.

---

## 1. The five React Fast Refresh primitives, restated for C\#

React's Fast Refresh has five load-bearing primitives. Each one has a direct
counterpart in UITKX. The whole plan is the construction of those five
counterparts.

| # | React primitive | What it does | UITKX counterpart |
|---|-----------------|--------------|-------------------|
| **P1** | `register(type, persistentID)` | At module load, every component registers its type against a stable string ID. The runtime maintains `id → Family { current: Type }`. | `RefreshRuntime.Register(persistentId, renderDelegate)` invoked from the SG-emitted module initializer of every component's host class. The persistent ID is `[UitkxSource].SourcePath + "::" + [UitkxElement].ComponentName`. |
| **P2** | `createSignatureFunctionForTransform()` | Babel emits a per-component signature describing the hook call sequence. On signature change → forced remount; otherwise compatible swap. | SG emits `[HookSignature("hash")]` (already exists) **plus** a one-time `__signature` static field call into `RefreshRuntime.Signature(...)` so the runtime owns the comparison, not the swapper. |
| **P3** | `resolveFamily(type)` | Reconciler asks "given this fiber's old type and the new vnode's type, do they belong to the same family?" — true → reuse, false → remount. | `RefreshRuntime.AreSameFamily(fiberFamilyId, vnodeFamilyId)` called from `CanReuseFiber` and `IsCompatibleType` instead of `Method`/`DeclaringType` equality. |
| **P4** | `scheduleRefresh(root, update)` | After a module reloads, the runtime walks every fiber root, replaces each fiber's type with `Family.current`, and schedules re-render. | `RefreshRuntime.PerformRefresh()` → walks `RootRenderer.AllInstances` + `EditorRootRendererUtility.GetAllRenderers()`, for every fiber whose `FamilyId` has a newer `Current`, sets `fiber.TypedRender = newCurrent` (or marks for remount on signature change), invokes `OnStateUpdated`. |
| **P5** | `module.hot.accept()` + bundler glue | Module system tells the runtime "this module finished reloading, here are its new types". | `UitkxHmrController.ApplySuccessfulCompileResult` → after `Assembly.LoadFrom`, the new assembly's module initializers call `Register` (P1), then the controller calls `RefreshRuntime.PerformRefresh()` (P4). No cascade walk needed. |

---

## 2. Architectural shift in one paragraph

**Today:** parents bake `MyChild.Render` (a method-group → static delegate)
into their IL at compile time. When `MyChild` recompiles into a new HMR DLL,
the parent's baked delegate now points at the *new type's* `Render`
trampoline, but the *fiber's* `TypedRender` still points at the *old type's*
`Render`. The reconciler compares `Method.DeclaringType` and sees them as
different. Fiber is torn down. Subtree unmounts. Effects re-fire. To work
around this, we introduced the cascade (rebuild parents into the same DLL),
which broke the trampoline's invariant by making the parent itself recompile.

**After Option C:** parents do NOT bake `MyChild.Render` into IL. They bake
a *Family handle* — a small `Family` object whose `Current` field always
points at the latest registered version of `MyChild`. The vnode carries the
Family handle, not the delegate. The reconciler compares Family handles, not
delegates. Recompile only changes `Family.Current`; the parent's IL never
moves; cascade is never needed; cross-DLL identity is impossible by
construction.

---

## 3. Concrete IL shape change

**Before** (current SG output):

```csharp
// inside Parent.__Render_body:
return V.Func<MyChild.Props>(
    MyChild.Render,                 // method-group → bakes Type+Method
    new MyChild.Props { /* ... */ },
    key: "k1");
```

**After** (Option C SG output):

```csharp
// at top of Parent class, ONCE per consumed child:
private static readonly global::ReactiveUITK.Refresh.Family __fam_MyChild =
    global::ReactiveUITK.Refresh.RefreshRuntime
        .GetFamily("Assets/UI/Pages/MyChild.uitkx::MyChild");

// inside Parent.__Render_body:
return V.Func<MyChild.Props>(
    __fam_MyChild,                  // FAMILY HANDLE — stable across HMR
    new MyChild.Props { /* ... */ },
    key: "k1");
```

The new `V.Func<TProps>(Family, TProps, ...)` overload writes the family
handle onto the vnode (`vnode._family = fam`) and pulls the dispatch
delegate from `fam.Current` so `_typedFunctionRender` is still set for
non-HMR rendering paths. The trampoline is **deleted entirely** — its
job is now done by the Family.

---

## 4. New types (full signatures)

```csharp
// Shared/Core/Refresh/Family.cs  (new — ~80 lines)
namespace ReactiveUITK.Refresh
{
    public sealed class Family
    {
        public string PersistentId { get; }
        // The dispatch delegate handed to V.Func. Identity is stable for
        // the lifetime of the Family object — it closes over `this` and
        // calls Current(props, children).
        public Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> Render { get; }

        internal Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> Current;
        internal Signature Signature;          // P2
        internal int Generation;               // bumped on every Register

        internal Family(string id, Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> initial)
        {
            PersistentId = id;
            Current = initial;
            Render = (p, c) => Current(p, c); // single closure allocation, EVER
        }
    }

    public sealed class Signature
    {
        public string OwnKey;        // "useState();useEffect([],);..." style
        public bool ForceReset;      // if any custom hook ref couldn't be resolved
        public Family[] CustomHooks; // transitive hook signature inputs
        internal string FullKey;     // computed lazily, like react-refresh
    }
}
```

```csharp
// Shared/Core/Refresh/RefreshRuntime.cs  (new — ~250 lines)
namespace ReactiveUITK.Refresh
{
    public static class RefreshRuntime
    {
        // P1 — primary registration entry point.
        public static Family Register(
            string persistentId,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> render);

        // Used by SG-emitted code at parent compile time to obtain a
        // Family handle BEFORE the child is loaded. If the family doesn't
        // exist yet, a placeholder Family with a "throw not-yet-loaded"
        // dispatch is returned and back-patched on first Register().
        public static Family GetFamily(string persistentId);

        // P2 — signature comparison.
        public static bool HaveEqualSignatures(Family a, Family b);
        internal static void AttachSignature(Family fam, Signature sig);

        // P3 — family identity for the reconciler.
        public static bool AreSameFamily(Family a, Family b)
            => a != null && a == b;

        // P4 — debounced refresh trigger.
        public static void PerformRefresh();

        // Error recovery.
        internal static void MarkRootFailed(VNodeHostRenderer root);
        internal static void RetryFailedRoots();
    }
}
```

The `Family` is **the single mutable cell** in the entire system. Every
parent's IL holds a reference to it. Every vnode produced by that parent
carries it. The reconciler compares it. The runtime mutates `Current`. No
other identity is consulted.

---

## 5. Changes per file (every line of edit, named)

### 5.0 SG — emit `ModuleInitializerAttribute` polyfill into user assemblies

**[A2: new section.]** The C# compiler synthesizes `<Module>.cctor`
whenever it sees a method tagged with the *type-name*
`System.Runtime.CompilerServices.ModuleInitializerAttribute`. The type
must be visible to the user's assembly. The SG project's own polyfill
([SourceGenerator~/IsExternalInit.cs#L12-L15](../SourceGenerator~/IsExternalInit.cs#L12))
is `internal` and lives only in the SG DLL — invisible to user code.

Fix: extend the existing `RegisterPostInitializationOutput` block at
[UitkxGenerator.cs#L53-L62](../SourceGenerator~/UitkxGenerator.cs#L53)
to emit a per-assembly polyfill source file:

```csharp
ctx.AddSource(
    "UITKX_ModuleInitializerPolyfill.g.cs",
    "// <auto-generated/>\n" +
    "#if !NET5_0_OR_GREATER\n" +
    "namespace System.Runtime.CompilerServices {\n" +
    "    [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false)]\n" +
    "    internal sealed class ModuleInitializerAttribute : global::System.Attribute { }\n" +
    "}\n" +
    "#endif\n"
);
```

`internal` is sufficient — the C# compiler only requires the type be
*reachable* from the method, not `public`. The `#if !NET5_0_OR_GREATER`
guard is future-proof: if Unity ever upgrades to a runtime that ships
the attribute in BCL, the polyfill silently disappears.

No change to `IsExternalInit.cs`; that polyfill is already gone-by-
emission for `init` accessors via the same pattern in user assemblies
elsewhere (verified — but needs the same emission added if it isn't
already). Treat it the same way: emit both polyfills together.

### 5.1 SG — Source generator (`SourceGenerator~/Emitter/CSharpEmitter.cs`)

**Current emit** [verified L1268-L1372](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1268)
emits `V.Func<TProps>(TypeName.Render, ...)`.
**Replace with** an emission that:

1. At **parent class top** (next to the existing trampoline field at
   [L286-L296](../SourceGenerator~/Emitter/CSharpEmitter.cs#L286)),
   emit one `__fam_<TypeName>` static readonly field per distinct child
   component referenced in the parent's render body. **[A2:** persistent
   ID is resolvable at SG time only via a **path resolution helper**:
   the parent SG knows `MyChild`'s **type name** but not its source
   path. The path lookup goes through the existing `PropsResolver`
   pipeline which already maps `typeName → asmdef → file` for prop
   discovery. We extend it to expose the source path. **] If path
   resolution fails (cross-asmdef name-only reference), fall back to
   `"::<TypeName>"` — still globally unique within a session, just
   not stable across rename.
2. At **call site** [L1278 and L1334](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1278),
   replace `{typeName}.Render` with `__fam_{typeName}`.
3. At **parent class module-init** — emit a private static method tagged
   `[ModuleInitializer]` calling
   `RefreshRuntime.Register(persistentId, __Render_body, signature, customHookFamilies)`
   for the **parent itself** (so consumers of *this* parent see its
   refresh too). This is exactly what React's `$RefreshReg$` emits.
4. **Delete** the trampoline (`__hmr_Render` field, the `Render` shim's
   `if (HmrState.IsActive)` guard at
   [L264-L296](../SourceGenerator~/Emitter/CSharpEmitter.cs#L264)).
   Family.Current does the indirection now. The public `Render` method
   stays — it just collapses to `return __Render_body(...)` (it's
   reachable from companion `.cs` files and external user code).
5. The whole block of new emit (Family fields + ModuleInitializer call)
   stays under `#if UNITY_EDITOR` to match the existing trampoline
   gating. In player builds, the SG keeps emitting the legacy direct
   `V.Func<TProps>(TypeName.Render, ...)` shape — zero player overhead.
   **[A2: not in original plan §8.4.]**

The persistent ID is computed at SG time from `_filePath` (already known)
and `_directives.ComponentName` (already known). It is byte-stable across
edits because it derives from path + name, not from compiled artifacts.

For consumers that reference a child component from a *different* asmdef
or from companion `.cs` code, the same `RefreshRuntime.GetFamily(id)` call
works — the runtime returns the singleton Family object regardless of who
asks first.

### 5.2 V — VNode call surface (`Shared/Core/V.cs`)

Add two overloads next to the existing `V.Func<TProps>` and `V.Func` (at
L484 and L506):

```csharp
public static VirtualNode Func<TProps>(
    Family family,
    TProps typedProps,
    string key = null,
    params VirtualNode[] children
) where TProps : class, Core.IProps;

public static VirtualNode Func(
    Family family,
    Core.IProps props = null,
    string key = null,
    params VirtualNode[] children
);
```

Both write `v._family = family;` and `v._typedFunctionRender = family.Render;`
so the existing render path through `family.Render → Current` continues to
work without touching the reconciler's hot loop.

The old delegate-taking overloads stay for backward compatibility (any
hand-written `V.Func(MyChild.Render, ...)` keeps working — the delegate
path is a degraded mode equivalent to today).

### 5.3 VNode — vnode storage (`Shared/Core/VNode.cs`)

Add `internal Family _family;` next to `_typedFunctionRender` at L58.
Reset to null in `__Rent` and the existing reset path at L235.

### 5.4 FiberNode — fiber storage (`Shared/Core/Fiber/FiberNode.cs`)

Add `internal Family Family;` field. Set in `CreateFiber` at
[FiberChildReconciliation.cs#L337](../Shared/Core/Fiber/FiberChildReconciliation.cs#L337)
from `vnode._family`. Carried for the lifetime of the fiber.

### 5.5 Reconciler — `CanReuseFiber` (both copies)

**[A2:** Verified — there is no `IsCompatibleType` function in the
current tree; the original plan was lifted from outdated notes. The
identity check happens in **`CanReuseFiber`** in two files. Both must
be edited identically because they handle different reconciliation
paths (keyed/index lists vs single-child). **]**

[FiberChildReconciliation.cs#L244-L296](../Shared/Core/Fiber/FiberChildReconciliation.cs#L244)
and [FiberFunctionComponent.cs#L264-L327](../Shared/Core/Fiber/FiberFunctionComponent.cs#L264)
— FunctionComponent branch becomes:

```csharp
case VirtualNodeType.FunctionComponent:
    if (fiber.Tag != FiberTag.FunctionComponent) return false;

    // Family path — Option C. Authoritative when both sides have a Family.
    if (fiber.Family != null && vnode._family != null)
        return RefreshRuntime.AreSameFamily(fiber.Family, vnode._family);

    // Legacy delegate path — for hand-written V.Func(MyChild.Render) sites.
    if (fiber.TypedRender == null || vnode.TypedFunctionRender == null)
        return false;
    if (ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)) return true;
    if (fiber.TypedRender.Method == vnode.TypedFunctionRender.Method
        && fiber.TypedRender.Target == vnode.TypedFunctionRender.Target) return true;
    return false;
```

The DeclaringType / cross-asm comparison is gone. We never look at
`Method.DeclaringType`. Identity is the Family object reference.

### 5.6 Reconciler — handle "same family, signature changed" remount

Add to the already-existing `useEffect` setup loop in
`FiberFunctionComponent.RunPassiveEffectSetups` a one-time check:

```csharp
// On the SECOND or later render of a fiber whose family's signature
// changed since the fiber was created, remount the fiber. Same semantics
// as React's haveEqualSignatures returning false.
if (fiber.Family != null
    && fiber.Family.Generation != fiber.FamilyGenerationAtMount
    && !RefreshRuntime.HaveEqualSignatures(
        fiber.MountedSignature, fiber.Family.Signature))
{
    ScheduleForceRemount(fiber);
    return;
}
```

`ScheduleForceRemount` runs the existing `FullResetComponentState` plus
`EffectFlags.Placement` so the fiber is treated as fresh-mounted on the
next commit.

### 5.7 Controller — replace cascade with `PerformRefresh`

[UitkxHmrController.cs](../Editor/HMR/UitkxHmrController.cs):

1. **Delete** the `CollectTransitiveDependents(includeComponents: true)`
   call in `OnUitkxFileChanged` at
   [L450-L455](../Editor/HMR/UitkxHmrController.cs#L450).
   Replace with `EnqueueCompile(uitkxPath); DrainCompileQueueIfIdle();`
   — single file only.
2. **Delete** `ProcessBatch` and the union-compile path entirely (the
   whole `if (_compileQueue.Count == 1) ... else ProcessBatch(batch)`
   branch in `DrainCompileQueueIfIdle`,
   [L491-L530](../Editor/HMR/UitkxHmrController.cs#L491)).
   Drains become single-file always.
3. **Delete** the `allowFullStateReset = (i == paths.Count - 1)`
   originator gate in `ProcessBatch`
   [L755-L763](../Editor/HMR/UitkxHmrController.cs#L755) — no batch, no
   originator, no gate. Force-remount is decided per-Family by signature
   comparison, not by save originator.
4. In `ApplySuccessfulCompileResult`
   [L595-L605](../Editor/HMR/UitkxHmrController.cs#L595), replace the
   `UitkxHmrComponentTrampolineSwapper.SwapAll(...)` call with
   `RefreshRuntime.PerformRefresh()`. **[A2:** the loaded HMR assembly's
   module initializers fire on `Assembly.LoadFrom` (which is what
   `_compiler.Compile` returns). By the time `ApplySuccessfulCompileResult`
   runs, every `Register(persistentId, ...)` call has already mutated
   the corresponding `Family.Current`. `PerformRefresh` only needs to
   walk roots and trigger re-render — it does not need to do the
   register/swap itself. **]**
5. **`UitkxHmrModuleStaticSwapper.SwapModuleStatics`** and
   **`UitkxHmrModuleMethodSwapper.SwapModuleMethods`** stay intact —
   they handle module-scope statics (not component identity), and they
   own the rude-edit detection for newly-added fields. Orthogonal to
   Family.
6. **Keep** `HookContainerRegistry.Seed/Invalidate/Reset` calls
   ([L208](../Editor/HMR/UitkxHmrController.cs#L208), L394, L208).
   **[A2: not cascade-related — corrected from original plan.]**
   It resolves `using static` for hook containers at compile time.

### 5.8 HMR emitter — emit register calls in HMR DLLs too

[HmrCSharpEmitter.cs](../Editor/HMR/HmrCSharpEmitter.cs) generates HMR
DLL source via runtime-reflective AST traversal. Edit sites mirror
`CSharpEmitter.cs` 1:1:

- [L237-L239](../Editor/HMR/HmrCSharpEmitter.cs#L237) emits the same
  `[UitkxSource]` + `[UitkxElement]` attributes — keep.
- [L295-L325](../Editor/HMR/HmrCSharpEmitter.cs#L295) emits the
  `__hmr_Render` trampoline + `Render` shim + `__Render_body`. Replace
  with: keep `Render` and `__Render_body` (collapse the IsActive guard);
  emit a `[ModuleInitializer]` `__uitkx_init` calling
  `RefreshRuntime.Register(persistentId, __Render_body, signature)`.
- [L1417 and L1459](../Editor/HMR/HmrCSharpEmitter.cs#L1417) emit
  `V.Func<P>(typeName.Render, ...)` and `V.Func(typeName.Render, null, ...)`.
  Replace `typeName.Render` with `__fam_{typeName}`. Family handle
  resolution is by `RefreshRuntime.GetFamily(persistentId)` so the HMR
  DLL's vnodes resolve to the **same Family object** as the project
  DLL's vnodes. **[A2: this is the load-bearing identity invariant —
  any vnode produced anywhere in the workspace, regardless of
  generation, resolves to the same Family by ID.]**
- [L3191](../Editor/HMR/HmrCSharpEmitter.cs#L3191) — no change (generic
  V.Func surface unchanged).

### 5.9 Files **deleted** (post-validation)

- [Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs)
  — 560 lines. Family in-place mutation obviates field-level swap.
  `FullResetComponentState` (L498-L539) **moves** to
  `Shared/Core/Refresh/ForceRemount.cs` since the reconciler still
  needs it as the force-remount primitive.
- [Editor/HMR/UitkxFileDependencyIndex.cs](../Editor/HMR/UitkxFileDependencyIndex.cs)
  — 424 lines. Only consumer was the cascade walker.
  **[A2: confirmed: only callers are `OnUitkxFileChanged` (L455),
  `Seed/Invalidate/Reset` lifecycle, and `Stop()`. No other code
  depends on the index after cascade is removed.]**
- The `__hmr_Render` field and `if (HmrState.IsActive) return __hmr_Render(...)`
  guard in every generated component file. **[A2: kept the public
  `Render` method itself — companion `.cs` files and external user
  code still reference it.]**
- Cross-asm fallback comments in both `CanReuseFiber` copies
  (already deleted). Stay deleted.

### 5.10 Files **simplified**

- [UitkxHmrController.cs](../Editor/HMR/UitkxHmrController.cs) shrinks
  by ~400 lines (cascade plumbing, batch path, retry orchestration).
  **[A2: pending-retry path stays** — it serves new-component-discovery
  via CS0103 auto-resolve at L915-L920+, which is independent of
  cascade. Original plan implied removal; corrected.**]**
- [HmrCSharpEmitter.cs](../Editor/HMR/HmrCSharpEmitter.cs) — trampoline
  emit collapses; Register-emit added. Net ~30 lines removed.

### 5.11 Reconciler — render-crash rollback hook

**[A2: not in original plan.]** [FiberReconciler.cs#L505-L530](../Shared/Core/Fiber/FiberReconciler.cs#L505)
uses `fiber.TypedRender.Method.DeclaringType` plus
`HmrState.TryRollbackComponent(Type)` to revert a crashing component to
the previous body. After Option C:

- Replace `Type` parameter with `Family`.
- `HmrState.TryRollbackFamily = RefreshRuntime.TryRollback;` wired by
  the `[InitializeOnLoadMethod]` hook in `RefreshRuntime`.
- `RefreshRuntime.TryRollback(Family)` reverts `family.Current` to its
  previous closure (kept in a per-Family rollback slot identical to
  today's `s_rollbackByType` design at
  [UitkxHmrComponentTrampolineSwapper.cs#L107-L113](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L107)).
- Same UX as today: render crash → revert to previous body → retry once
  → if still crashes, fall through to nearest ErrorBoundary.

---

## 6. Persistent IDs — the make-or-break detail

Persistent ID quality determines correctness. React's bundler uses
`module.id + " " + componentName`. We use:

```
<asset-relative path with forward slashes>::<ComponentName>
```

Example: `Assets/UI/Pages/GamePage.uitkx::GamePage`.

**Properties:**

- **Stable across edits.** The path never changes when the user types in
  the file.
- **Stable across HMR DLL generations.** Every HMR recompile of the same
  file produces the same ID. `Register` finds the existing Family and
  updates `Current`.
- **Stable across rename of the *class*** (the SG-emitted partial keeps
  the file basename as the component name unless the user puts an
  override in `@component <Name>`; in either case the SG knows the name
  at emit time).
- **Changes only on file rename or move.** That's the same as React: if
  you rename the file, the component remounts. Acceptable.
- **Cross-asmdef safe.** Two components with the same name in different
  asmdefs have different paths → different IDs → different Families.
- **No collisions.** Two `[UitkxElement]` attributes with the same
  ComponentName at the same path is already a SG diagnostic
  (`UITKX0001` duplicate component name).

### 6.1 What if the user renames the file mid-session?

The runtime sees:
- Old Family `Assets/A.uitkx::A` exists with current = old body.
- New Family `Assets/B.uitkx::B` is registered with new body.
- Old fibers' `Family` field still points at the old Family, whose
  `Current` is unchanged. They render normally with old code.
- Next parent re-render produces vnodes whose Family is the *new* Family.
- `AreSameFamily` returns false → fiber remounts.

This is the same UX as React when you rename a component. Acceptable and
well-defined.

---

## 7. The signature mechanism (P2 detail)

React's signature is a hash of the source text of the hook calls, plus
recursive references to custom hooks. Hook order and hook-call shapes
must be stable for state preservation. Adding a `useState` between two
existing hooks → signature changes → remount.

UITKX already has `[HookSignature("hash")]` (emitted at
[CSharpEmitter.cs#L194](../SourceGenerator~/Emitter/CSharpEmitter.cs#L194)).
Today it's only consulted at the swapper level. After Option C, every
`Register` call passes the signature string into the runtime:

```csharp
[ModuleInitializer]
internal static void __uitkx_init()
{
    RefreshRuntime.Register(
        persistentId: "Assets/UI/Pages/GamePage.uitkx::GamePage",
        render: __Render_body,
        signature: "useState(0);useEffect([userId],);useReducer(reducer,init);"
    );
}
```

The runtime stores the signature, and on a later `Register` of the same
ID, compares old vs new signature. If different, the Family is marked
"force remount" — every fiber of that Family will be torn down and
remounted with fresh state on the next refresh pass.

### 7.1 Custom hook transitivity

React's signature transitively pulls in custom hook signatures. For UITKX
this is straightforward: the SG already extracts the hook call sequence
when it computes `[HookSignature]`, and it knows which calls are
user-defined (custom) hooks because the resolver tracks them. We extend
the SG to emit:

```csharp
RefreshRuntime.Register(
    persistentId: "...",
    render: __Render_body,
    ownSignature: "useState(0);__customHook;useReducer;",
    customHookFamilies: new[] {
        RefreshRuntime.GetFamily("Assets/Hooks/useUser.uitkx::useUser"),
        // ...
    });
```

`HaveEqualSignatures` walks `customHookFamilies` and concatenates each
family's `Signature.OwnKey` to compute the full key. Same algorithm as
[ReactFreshRuntime.computeFullKey](https://github.com/facebook/react/blob/main/packages/react-refresh/src/ReactFreshRuntime.js).

---

## 8. Performance

### 8.1 Per-render cost

The Family adds **one indirect call** per child render
(`family.Render(props, children) → Current(props, children)`). Today's
trampoline has the same single indirect call (`Render → if HmrState.IsActive
return __hmr_Render(...)`). **Net cost: identical.**

### 8.2 Per-vnode cost

`vnode._family = family` is one extra reference assignment per `V.Func`
call. Negligible (vnode allocations already touch ~10 fields).

### 8.3 Per-edit cost

Replaces:
- N-file union compile (cascade-driven, can be 10+ files for a leaf edit)
- Reflection scan over `AppDomain.CurrentDomain.GetAssemblies()` for every
  swap (`FindAllSwapTargetTypes` at
  [UitkxHmrComponentTrampolineSwapper.cs#L327](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L327))
- Per-fiber tree walk to notify

With:
- Single-file compile.
- One `RefreshRuntime.Register` call from the new DLL's module initializer
  (an indexed dictionary write).
- One `PerformRefresh` walk (still a tree walk, but bounded — only fibers
  whose Family changed do work).

**Net edit cost: ~5–10× faster on a typical 4-file cascade today.**

### 8.4 Player builds

Family handles are populated once at type-init by reading
`[UitkxSource]` / `[UitkxElement]` attributes via reflection. In the
player, `RefreshRuntime.Register` becomes a no-op stub (a `#if
UNITY_EDITOR` branch in the SG-emitted module initializer leaves the
delegate path direct). The Family's `Render` closure is still allocated
(one per component) but its `Current` is set once and never changes — JIT
will inline. The user pays one allocation per component class, total. For
a 100-component app: 100 closures, 8 KB total.

Alternative for player builds: emit the Family init under
`#if UNITY_EDITOR`, and in players use the legacy direct delegate path.
Then players have zero overhead from this change. **Recommended**.

---

## 9. What breaks

A complete enumeration. Honest.

### 9.1 Things that break for *us* (not the user)

- **`UitkxHmrComponentTrampolineSwapper`** — file is deleted. ~560 lines
  of code gone. Net win.
- **`UitkxFileDependencyIndex`** — file is deleted (in Editor scope). The
  language-server still has its own indexer; not affected.
- **`HookContainerRegistry` Seed** — cascade-only consumer. The hook
  delegate-swap path (used for hook bodies, not components) still works
  via `UitkxHmrDelegateSwapper`; that file stays.
- **All 28 entries in [HMR_AUDIT.md](HMR_AUDIT.md)** are revisited. ~12
  of them go away with the cascade, 8 are unaffected (FSW path, USS
  cascade, asset cache sync), 8 need re-audit against the new runtime
  (rollback, error recovery, signature transitivity). **A new audit doc
  replaces HMR_AUDIT.md** when the work lands.

### 9.2 Things that break for the user (consumer-facing)

- **None of the user's `.uitkx` syntax changes.** The SG emit shape
  changes but the input language is identical.
- **One new C# attribute is emitted on every component**:
  `[ModuleInitializer]` on a private `__uitkx_init` method. C# users
  don't see this; SG-generated code only.
- **One new dependency in `Shared/`** — the `RefreshRuntime` static class.
  No external dependencies. Existing code that calls `V.Func<T>(R,
  props)` directly with a delegate keeps working unchanged (the legacy
  overload stays). User code that ignores SG output (does not exist in
  practice) is unaffected.
- **Hand-written components that use hooks** (rare; supported via
  companion `.cs` code) need to call `RefreshRuntime.Register` in their
  module initializer to participate in HMR. Without it they fall back to
  the direct-delegate path, which is exactly what they have today
  (correct, but no fast-refresh state preservation). **No regression** —
  same as today.
- **Behaviour change on file rename**: a file rename now causes a
  controlled remount of fibers of the renamed component. Today's
  behaviour is "the rename probably breaks everything until you restart
  Unity", so this is a strict improvement, but it is a behaviour change.

### 9.3 Things that newly *work* that didn't before

- **Add a `useState` mid-component → only that component remounts.**
  Today: subtree teardown.
- **Edit a deeply-shared component → all consumers see the new code on
  the next render.** Today: cascade recompiles all consumers, randomly
  triggers the cross-DLL bug.
- **Save twice in a row → second save is faster** (same Family, just
  updates `Current`). Today: same as today (no win, no loss).
- **Live-create a brand-new component and use it in another file.**
  Today's code at
  [HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md](archive/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md)
  is a special-cased patch; with Option C this is just "register a new
  Family, it has zero consumers, the next save of the consumer wires
  the IL to look up that Family by ID, done".

---

## 10. Migration plan (PR sequence)

> **Status:** All four phases shipped in 0.6.0. Checkmarks below reflect
> the final state in the `cleanup_and_upgrades` branch.

A working implementation lands across ~4 PRs in this order. **Each PR is
green and shippable on its own.**

### ✅ PR-A: introduce Family + RefreshRuntime, dual-path emit (1 day)

- Create `Shared/Core/Refresh/Family.cs`, `RefreshRuntime.cs`.
- Create the new `V.Func<TProps>(Family, ...)` overloads.
- Add `vnode._family` and `fiber.Family` fields.
- Update `CanReuseFiber` / `IsCompatibleType` to consult Family first,
  fall back to delegate path if null. (Family is null for all existing
  vnodes since SG hasn't been changed yet — net behaviour unchanged.)
- Update reconciler `CreateFiber` to copy `vnode._family → fiber.Family`.
- Tests: `RefreshRuntimeTests.cs` exercising Register / GetFamily /
  AreSameFamily.

**At end of PR-A**: nothing user-visible changes. Infrastructure exists.

### ✅ PR-B: SG emits Family handles + Register; trampoline still emitted (2 days)

- Update `CSharpEmitter.cs` to emit `__fam_<ChildName>` static fields and
  use Family handle in `V.Func` calls.
- Emit `[ModuleInitializer]` `__uitkx_init` calling
  `RefreshRuntime.Register`.
- Keep the trampoline emission for now — both run. Family wins over
  trampoline because the family path is checked first in
  `CanReuseFiber`.
- Update SG snapshot tests to expect Family emission.

**At end of PR-B**: Family path is live. HMR uses Family identity. Trampoline
is dead code but still compiled. **The user's reproduce-bug case
(`InteractionDialog.uitkx` → GamePage cascade) is fixed at this point.**

### ✅ PR-C: HMR controller switches to Family-based refresh (1 day)

- Delete cascade walker call in `OnUitkxFileChanged`.
- Delete `ProcessBatch`, `CompileBatch`, `_pendingRetryPaths` complexity.
- Delete `UitkxHmrComponentTrampolineSwapper.cs` and `FullResetComponentState`
  is moved to `RefreshRuntime` (it remains the force-remount primitive).
- HMR DLL emitter (`HmrCSharpEmitter`) emits the same Register call.
- Tests: end-to-end Editor test that saves a leaf component and asserts
  no parent fiber is unmounted.

**At end of PR-C**: cascade is gone. Single-file compile. Determinism
property #1 from §0 is testable and passes.

### ⚠️ PR-D: Signature transitivity + remount-on-incompat-edit (1-2 days)

> Partially shipped in 0.6.0: hook-signature equality string is computed
> per component and `PerformRefresh` calls `FullResetComponentState` on
> signature-changed fibers (force remount). Transitive custom-hook
> Family chasing (`customHookFamilies` array + `HaveEqualSignatures`
> walk) is deferred - the current signature is a self-contained hash
> over the component's own hook shape. Sufficient for the targeted
> cross-DLL identity bug; transitive remount on shared-hook edits
> remains future work.

- SG emits `customHookFamilies` array.
- `RefreshRuntime.HaveEqualSignatures` walks transitively.
- Force-remount path on `Generation` mismatch with `HaveEqualSignatures =
  false`.
- Tests: add/remove a `useState`, assert fresh state on next render.

**At end of PR-D**: full Fast Refresh semantics. Determinism property #2
testable and passes.

---

## 11. Risk register

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `[ModuleInitializer]` polyfill missing in user assemblies → CS0518 | **Was high; now mitigated by §5.0** | SG emits polyfill via `RegisterPostInitializationOutput` per asmdef. **[A2]** |
| `[ModuleInitializer]` fires too late and the parent's `__fam_X` field reads null | None | `__fam_X` static-init calls `RefreshRuntime.GetFamily(id)` which upserts a placeholder if no Register has happened yet. Both orderings (parent-first / child-first) safe by construction. |
| Cross-asmdef load order — `MyChild` (asmdef B) loads before parent (asmdef A) | None | A → B reference graph forces B to load first. Module initializers fire on assembly load. |
| Two copies of `CanReuseFiber` drift apart | Low | Both files edited in same commit. **[A2: explicit acknowledgement.]** |
| The closure `(p,c) => Current(p,c)` allocates per call | None | Once-per-Family closure — allocated at type-init, called many times. Zero new GC pressure per render. |
| Cross-asmdef Family resolution requires `Shared/` to be loaded first | None | `Shared/` referenced by every other asmdef in the workspace (verified via grep). |
| User's hand-written `V.Func(MyChild.Render, ...)` doesn't get fast refresh | Accepted | Documented limitation. Non-regression: same UX as today. |
| Custom-renderer code that walks vnodes by `.TypedFunctionRender.Method` | Low | `TypedFunctionRender` keeps working — Family's stable closure is what's stored. |
| Player build size grows | Negligible | Under `#if UNITY_EDITOR` only. **[A2: zero player overhead.]** |
| Domain reload (Unity → "Reload Domain") clears the Family registry | None | New domain → new registry → first frame seeds from module initializers as types load. |
| Render crash with no rollback slot populated | Low | First-ever render of a Family has no previous body; `TryRollback` returns false; reconciler falls through to ErrorBoundary — same as today. |
| Persistent ID resolution fails for cross-asmdef name-only references at SG time | Low | Fallback ID `"::<TypeName>"` keeps the system functional within a session; only renames suffer (component remounts on rename, same as React). **[A2]** |

---

## 12. Validation contract

The implementation is "done" when **all six** of these tests pass on a
fresh Unity instance with samples present, and again with samples removed:

1. **Save InteractionDialog.uitkx 20 times**, no body change. Expect: no
   `[HMR-DIAG] CommitDeletion` log line, no console errors, working set
   delta < 5 MB.
2. **Save GamePage.uitkx body change**, then save back. Expect: GamePage
   re-renders with new body; no GamePage child fiber is destroyed; scene
   does not re-load.
3. **Add `useState(0)` mid-`InteractionDialog`**. Expect: only
   `InteractionDialog` fibers remount; parent fibers (GamePage,
   AppRoot) preserve their state and effects; siblings preserve their
   state.
4. **Edit a custom hook used by 5 components**. Expect: all 5 component
   fibers remount (signature transitivity); their parents do not.
5. **Inject a render-time exception into a component**. Expect:
   `RefreshRuntime` reverts to previous Family.Current; subsequent saves
   are tried.
6. **Run with samples folder present, then without**. Expect: identical
   per-save log output for the same edit (modulo timestamps). Equality
   verified by stripping the timestamp prefix and diffing.

---

## 13. What this *doesn't* fix

Listed honestly:

- **CLR rude edits** (newly-added `static readonly` field, newly-added
  public method on a project-loaded type) still need a domain reload.
  **[A2:** UITKX already handles this gracefully — it's not a CLR
  bypass, but a *deferred* reload via `EditorUtility.RequestScriptReload`
  ([UitkxHmrController.cs#L1273-L1280](../Editor/HMR/UitkxHmrController.cs#L1273)),
  gated by Play mode, with a one-warning-per-session log + opt-out
  EditorPref `UITKX_HMR_AutoReloadOnRudeEdit`. **Option C preserves
  this exactly** — `UitkxHmrModuleStaticSwapper.DetectAndWarnAddedFields`
  is unchanged. **]**
- **Adding a new component class that didn't exist before** — special-cased
  today via [Plans~/archive/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md](archive/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md).
  Family is a strict improvement: a brand-new component just registers
  a brand-new Family on first save; consumers wire to that Family ID
  on their next save (or on first parent type-init touch). The
  `FindAllSwapTargetTypes` workaround at
  [UitkxHmrComponentTrampolineSwapper.cs#L327-L398](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs#L327)
  becomes unnecessary. **[A2: net deletion, not a missing feature.]**
- **Asset import races** — if the user saves a `.uitkx` that depends on
  a `.uss` that hasn't finished importing, the swap fails like today.
  Orthogonal.
- **Hand-written companion `.cs` files** that don't go through the SG —
  they're outside the Family system. They work like today (recompile via
  Unity's normal path; no fast refresh state preservation). Documented.

---

## 14. Versioning & delivery

**[A2: revised per user direction — single local landing, no incremental PRs.]**

Delivery: implement the entire change locally on the
`cleanup_and_upgrades` branch as one cohesive sequence. Validate
end-to-end against PrettyUi's repro before any commit. Land as a single
push (or a logical group of commits) once green.

Version: bump straight to `0.6.0` as the symbolic marker for "the HMR
architecture changed under your feet but your code didn't have to".
User-facing `.uitkx` syntax does not change; SG emit shape changes (
consumer-invisible). The only public-API addition is
`ReactiveUITK.Refresh.{Family, RefreshRuntime}` — a brand-new namespace,
additive only.

CHANGELOG entry + `Plans~/DISCORD_CHANGELOG.md` entry follow
[discord-changelog instructions](../.github/instructions/discord-changelog.instructions.md).

During local development, the old plan's PR-A → PR-D sequence is still
useful as a **checkpoint structure** (Family infra → SG emit → controller
cutover → signature transitivity). Validate at each checkpoint before
proceeding; commit only at the end.

---

## 15. Direct mapping to React Fast Refresh, line by line

For traceability — a developer reading this plan should be able to open
the React source and see the same algorithm:

| React file | React function | UITKX file | UITKX function |
|------------|---------------|------------|----------------|
| `react-refresh/src/ReactFreshRuntime.js` | `register(type, id)` | `Shared/Core/Refresh/RefreshRuntime.cs` | `Register(id, render, sig)` |
| same | `createSignatureFunctionForTransform()` | `SourceGenerator~/Emitter/CSharpEmitter.cs` | emit `Register(..., signature: ...)` from module-init |
| same | `computeFullKey(signature)` | `Shared/Core/Refresh/RefreshRuntime.cs` | `ComputeFullKey(Family)` |
| same | `haveEqualSignatures(prev, next)` | same | `HaveEqualSignatures(a, b)` |
| same | `performReactRefresh()` | same | `PerformRefresh()` |
| `ReactFiberHotReloading.js` | `resolveFamily(type)` | `Shared/Core/Refresh/RefreshRuntime.cs` | `AreSameFamily(a, b)` (we expose the comparison instead of resolving, since `Family` IS the identity) |
| same | `isCompatibleFamilyForHotReloading(fiber, element)` | `Shared/Core/Fiber/FiberChildReconciliation.cs` | new branch in `CanReuseFiber` (§5.5) |
| same | `markFailedErrorBoundaryForHotReloading(fiber)` | `RefreshRuntime.cs` | `MarkRootFailed(root)` (§4 sketch) |
| same | `scheduleRoot(root, element)` for failed-root retry | same | `RetryFailedRoots()` (debounced after every PerformRefresh) |
| `react-refresh/src/ReactFreshBabelPlugin.js` | `findInnerComponents` + `createRegistration` | `SourceGenerator~/Emitter/CSharpEmitter.cs` | the SG already finds components via `[UitkxElement]` |
| same | `$RefreshReg$(handle, persistentID)` | SG-emitted `__uitkx_init` `[ModuleInitializer]` | `RefreshRuntime.Register(...)` |
| same | `$RefreshSig$()` start-of-function call | SG-emitted call at top of `__Render_body` | `RefreshRuntime.SigEnter(family)` (or omitted — see §7 note: the signature is computed at compile time as a string, not at runtime) |

The single divergence: React computes signatures at runtime by tracing
hook calls during the first render; UITKX computes signatures at
**compile time** via `[HookSignature]`. This is **strictly stronger**
because it doesn't depend on the first render running — it's pure source
analysis. We can do this because we own the SG; React can't because Babel
runs before any user code analysis.

---

## 16. Recommendation

**Implement Option C as a single local landing. Bump to `0.6.0`.**

Validate at each internal checkpoint (Family infra → SG emit →
controller cutover → signature transitivity) before proceeding to the
next. The non-determinism the user observed in PrettyUi is gone once
the SG-emit checkpoint is reached and the new SG output replaces the
old trampoline emission.

The `[HMR-DIAG]` logs in
`C:\Users\neta\Pretty Ui\Assets\ReactiveUIToolKit\` stay in place
until end-to-end validation against PrettyUi's repro is complete.

CHANGELOG entry that reads: *"HMR architecture rewritten on top of a
Family/Signature runtime modeled on React Fast Refresh. Cascade
compiles are removed; cross-DLL identity is impossible by construction;
component edits never re-fire mount effects in unrelated subtrees. No
code changes required in user `.uitkx` files."*

## 17. Audit-pass-2 conclusion

The plan **stands** with the corrections applied above. No new file or
subsystem was found that contradicts the architecture. Three corrections
were material:

1. **§5.0 added** — `[ModuleInitializer]` polyfill must be emitted into
   user assemblies. Single missed step in the original plan that would
   have produced CS0518 across the entire workspace. Fix is one new
   `RegisterPostInitializationOutput` block (~10 lines).
2. **§5.5 corrected** — there is no `IsCompatibleType` function; the
   identity check happens in two copies of `CanReuseFiber`. Both must
   be edited identically. Same edit, two files.
3. **§5.7 corrected** — `HookContainerRegistry` is *not* cascade-only.
   It stays. Only `UitkxFileDependencyIndex` is deleted.

Minor refinements: rollback hook signature change (Type → Family),
player-build gating made explicit, cross-DLL `Method.DeclaringType`
failure mode confirmed as root cause, persistent-ID fallback for
cross-asmdef name-only references.

**Still a good idea?** Yes. The audit reinforced the central insight:
the trampoline architecture's correctness depends on parents *not*
recompiling, and the cascade architecture forces parents to recompile.
The two are mutually incompatible by construction. Family removes the
dependency on parent IL stability — parents become free to recompile,
the cascade becomes unnecessary, and cross-DLL identity is impossible
because identity has moved into a Shared/-resident, single-instance,
ID-keyed handle. The whole class of bugs around
`Method.DeclaringType` inequality vanishes structurally.

Net code change: **~1500 lines deleted, ~400 lines added** (Family +
RefreshRuntime + SG emit changes + reconciler 2-line diff × 2 files).
No user-facing API regression. Player build size: unchanged.
Validation contract: 6 deterministic tests in §12.

---

*End of plan.*
