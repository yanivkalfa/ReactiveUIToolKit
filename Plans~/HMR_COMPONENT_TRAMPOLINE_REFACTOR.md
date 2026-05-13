# HMR — Component Render Trampoline Refactor

Status: **Implemented** (May 13, 2026)
Branch target: `cleanup_and_upgrades` (or follow-up branch)
Owner: AI / Yaniv

---

## 1. Executive Summary

The HMR pipeline currently has **three coupled defects** with one shared root cause:

1. **Editor freeze on rapid Ctrl+S edits** to `.uitkx` files. Reproducible in 1–2 saves without the closure wrapper.
2. **Second-save-ignored regression** introduced by the closure-wrapper workaround (the wrapper hides the original component's `DeclaringType.Name`, so `UitkxHmrDelegateSwapper.IsMatch` fails to identify the component on the next swap).
3. **Navigation-revert bug**: editing a component, navigating to another page, then navigating back shows the **old code**. Architecturally unfixable while parent-component IL bakes direct method-group references.

**Single root cause**: function-component `Render` methods are emitted as **direct static methods** with no HMR indirection. There is no per-component "swap point" — so the only swap path is to walk every fiber and rewrite `fiber.TypedRender`. That walk:
- mutates delegate identity, breaking `ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)` short-circuits in `FiberFunctionComponent.CanReuseFiber`;
- never reaches fibers that don't yet exist (the navigation-revert case);
- runs concurrently with reconciliation (the freeze case).

**Single fix**: emit a per-component **`__hmr_Render` trampoline** that mirrors the existing hook/module pattern. The public `Render` method becomes a thin shim that consults a static delegate field; HMR swaps that **field**, not per-fiber state. Hooks and modules already do this — components have been the inconsistent case.

This unifies the three defects under one solution, restores stable delegate identity (so reconciler short-circuits work again and we can delete the cross-assembly `IsCompatibleType` HMR fallback), and **improves HMR swap latency by an estimated 5–10×** (O(fibers) → O(changed types)).

---

## 2. Problem Statement (Detailed)

### 2.1 Current component emission

`SourceGenerator~/Emitter/CSharpEmitter.cs` emits each component as:

```csharp
public static class MyComponent
{
    public static VirtualNode Render(IProps props, IReadOnlyList<VirtualNode> children)
    {
        // user body — direct
    }
}
```

Parent components reference this via `V.Func<TProps>(MyComponent.Render, props)` ([Shared/Core/V.cs#L485-L500](../Shared/Core/V.cs#L485)). The C# compiler bakes a **method-group delegate** (`Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>` pointing at `MyComponent.Render`) into the parent's IL **at the parent's compile time**.

Note: `[HookSignature]` is **already** emitted on component classes today ([SourceGenerator~/Emitter/CSharpEmitter.cs#L181](../SourceGenerator~/Emitter/CSharpEmitter.cs#L181)) and `HasHookSignatureChanged` already exists ([Editor/HMR/UitkxHmrDelegateSwapper.cs#L407-L415](../Editor/HMR/UitkxHmrDelegateSwapper.cs#L407)). The refactor adds the **trampoline shape** to components; it does not introduce the signature attribute or the diff function — those are reused as-is.

### 2.2 What HMR has to do today

When the user saves `MyComponent.uitkx`, `UitkxHmrCompiler` produces a fresh assembly containing `hmr_MyComponent_N.MyComponent.Render`. To make the rest of the (untouched) graph use it, `UitkxHmrDelegateSwapper.WalkAndSwap` does:

1. `EditorRootRendererUtility.GetAllRenderers()` → enumerate every active root renderer.
2. DFS-walk every `FiberNode` in every tree.
3. For each fiber whose `TypedRender.Method.DeclaringType` matches by `[UitkxElement]` name, set `fiber.TypedRender = newDelegate` and call `OnStateUpdated(fiber)`.

This is O(fibers) per save and races with the active reconciler.

### 2.3 Why this breaks (3 symptoms, 1 cause)

**(a) Freeze.** After the swap, on the next render pass, `FiberFunctionComponent.CanReuseFiber` runs (Shared/Core/Fiber/FiberFunctionComponent.cs#L282-L314):
```csharp
if (ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)) return true;
```
Because the swapper set `fiber.TypedRender = newDelegate` and the parent's vnode also got rewritten to the same `newDelegate` reference (or the same delegate is created for both), the short-circuit returns `true` → fiber reuse → stale hook state on the new body → infinite re-render in `WorkLoop`. Heavy diagnostic I/O widens the timing window enough to mask it (Heisenbug).

**(b) Second-save ignored (wrapper regression).** The closure-wrapper workaround wraps the new render in a lambda. `IsMatch` looks at `delegate.Method.DeclaringType.Name` to decide which fiber's render belongs to which `[UitkxElement]`; for a closure that resolves to the compiler-generated `<>c__DisplayClass…` and **never matches the user component name**, so the next save's swap finds zero fibers to update.

**(c) Navigation revert.** Parent IL contains a direct `ldftn MyComponent::Render` baked at parent compile time. When the user navigates away and the destination page is re-rendered fresh, its **parent's IL** still issues the old method-group delegate — the swapper has no way to retro-edit the parent's IL, only the live fiber instances it can find.

### 2.4 Why hooks and modules don't have this problem

`HookEmitter.cs` (L95–L183) and `ModuleBodyRewriter.cs` (L326–L380) already emit a trampoline:

```csharp
#if UNITY_EDITOR
private static Func<…> __hmr_MyHook;
#endif

public static TResult MyHook(...)
{
#if UNITY_EDITOR
    var hmr = __hmr_MyHook;
    if (hmr != null && ReactiveUITK.Core.HmrState.IsActive) return hmr(...);
#endif
    return __MyHook_body(...);
}

private static TResult __MyHook_body(...) { /* user body */ }
```

Hot-swapping a hook is then a **single `FieldInfo.SetValue`** on `__hmr_MyHook`. No fiber walk, no identity churn, and parent IL keeps calling `MyHook(...)` which transparently dispatches to the new body. This is exactly what we need for components.

---

## 3. Solution

### 3.1 Component emission shape (target)

```csharp
[UitkxElement("MyComponent")]
public static partial class MyComponent
{
#if UNITY_EDITOR
    private static System.Func<
        ReactiveUITK.Core.IProps,
        System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.VirtualNode>,
        ReactiveUITK.Core.VirtualNode> __hmr_Render;
#endif

    public static ReactiveUITK.Core.VirtualNode Render(
        ReactiveUITK.Core.IProps props,
        System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.VirtualNode> children)
    {
#if UNITY_EDITOR
        var hmr = __hmr_Render;
        if (hmr != null && ReactiveUITK.Core.HmrState.IsActive) return hmr(props, children);
#endif
        return __Render_body(props, children);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private static ReactiveUITK.Core.VirtualNode __Render_body(
        ReactiveUITK.Core.IProps props,
        System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.VirtualNode> children)
    {
        // user body (typed-props local, hooks, V.Func calls, return)
    }
}
```

Key invariants:
- **Public signature unchanged** — `MyComponent.Render` remains the canonical method group, `V.Func(MyComponent.Render, …)` keeps working.
- Trampoline + field guarded by `#if UNITY_EDITOR` → **zero player-build overhead** (matches existing hook/module pattern, see [`HookEmitter.cs#L126`](../SourceGenerator~/Emitter/HookEmitter.cs#L126), [`ModuleBodyRewriter.cs#L362`](../SourceGenerator~/Emitter/ModuleBodyRewriter.cs#L362)).
- `__Render_body` is always emitted (referenced by trampoline) → preserved by IL2CPP linker without any `[Preserve]` / link.xml entry.
- `[EditorBrowsable(Never)]` on body suppresses it from IDE completion and Go-to-Def lists.

### 3.2 HMR swap path (target)

Replace `UitkxHmrDelegateSwapper.WalkAndSwap` with a new `UitkxHmrComponentTrampolineSwapper` modelled directly on `UitkxHmrModuleMethodSwapper`:

1. Per changed component type in the freshly compiled assembly:
   - Resolve old type via `[UitkxElement]` name lookup in the live `Assembly-CSharp` domain.
   - Build a new delegate over the new type's `Render`.
   - `oldType.GetField("__hmr_Render", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, newDelegate)`.
2. Decide remount policy via existing signature machinery:
   - `HasHookSignatureChanged(oldType, newType)` (already exists, checks `[HookSignature]`).
   - **Same signature** (body-only edit): swap field, done. Hooks preserved. React Fast Refresh "compatible edit".
   - **Different signature**: swap field **and** walk live fibers of that component type calling `FullResetComponentState(fiber)` — preserves React FR "incompatible edit" semantics. Walk is bounded by component-type filter (much smaller than today's universal walk).

Result: O(changed types) reflection writes vs today's O(all fibers) per-fiber rewrite. Estimated **5–10 ms → < 1 ms** on a 1000-fiber tree.

### 3.3 Reconciler simplification

With stable `Render` identity restored:

- `FiberFunctionComponent.CanReuseFiber` `ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)` short-circuit ([Shared/Core/Fiber/FiberFunctionComponent.cs#L282-L314](../Shared/Core/Fiber/FiberFunctionComponent.cs#L282)) becomes correct again — both sides point to the same enclosing `MyComponent.Render` method, regardless of how many HMR cycles have occurred. The reference equality holds (not just method equality) because Roslyn — the workspace targets `<LangVersion>latest</LangVersion>` for the SG project and Roslyn 4.0+ for Unity assemblies — caches static method-group delegate conversions in compiler-generated static fields per call site. With the trampoline, the underlying method (`Render`) is stable across HMR cycles → the cached delegate slot is stable → `ReferenceEquals` fires.
- Cross-assembly HMR fallback in `IsCompatibleType` ([Shared/Core/Fiber/FiberFunctionComponent.cs#L300-L314](../Shared/Core/Fiber/FiberFunctionComponent.cs#L300) + duplicate at [Shared/Core/Fiber/FiberChildReconciliation.cs#L283-L298](../Shared/Core/Fiber/FiberChildReconciliation.cs#L283)) becomes dead code — delete both.
- Crash-rollback path at FiberReconciler.cs L681-L720 remains correct: `fiber.Alternate.TypedRender = oldDelegate` still rolls back the live cached delegate (and trampoline field stays whatever it was).

### 3.4 React Fast Refresh parity

This refactor finally maps cleanly onto React's model:

| React FR concept                       | UITKX equivalent (after refactor)                                  |
|----------------------------------------|---------------------------------------------------------------------|
| Module-level component identity        | `MyComponent.Render` static method (stable across HMR)              |
| HMR boundary                           | `__hmr_Render` field swap                                           |
| Compatible edit (body-only)            | Same `[HookSignature]` → field swap only, hooks preserved           |
| Incompatible edit (hook order changed) | Different signature → field swap + `FullResetComponentState` walk   |
| Force remount                          | Manual `FullResetComponentState` (already exists)                   |

---

## 4. Impact Analysis (Verified)

### 4.1 Runtime / Player builds — **Zero cost**

- `__hmr_Render` field and trampoline branch are inside `#if UNITY_EDITOR`. In players the `Render` method body collapses to `return __Render_body(props, children);` — JIT/IL2CPP inlines it.
- `HmrState.IsActive` itself is `#if UNITY_EDITOR` ([Shared/HmrState.cs#L11](../Shared/HmrState.cs#L11)) — no symbol exists in players to reference. Confirms zero player overhead.
- IL2CPP code growth estimate: < 5% per component (one extra private method with an unconditional tail call → typically inlined). No `link.xml` / `[Preserve]` changes needed.

### 4.2 Editor render cost — **Negligible (~5–10 ns) + steady-state win**

- Per render: `ldsfld __hmr_Render` + null check + `ldsfld HmrState.IsActive` + branch. JIT inlines thin trampolines.
- **Net win**: stable delegate identity restores `ReferenceEquals` cache hits in `CanReuseFiber`, which today miss after every HMR swap. Likely a perf improvement in the edit-heavy editor session.

### 4.3 HMR latency — **Estimated 5–10× faster**

- Today: O(all fibers across all renderers) DFS + per-fiber delegate write + `OnStateUpdated`. ~5–10 ms on a 1000-fiber tree.
- Proposed: O(changed component types) reflection writes. < 1 ms typical.

### 4.4 IDE / LSP — **Visible but managed**

- LSP runs the same `CSharpEmitter` ([ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](../ide-extensions~/lsp-server/Roslyn/RoslynHost.cs)) → trampoline shape appears in virtual generated documents.
- Go-to-Definition on `MyComponent.Render` lands on the trampoline (one extra hop to the body). `[EditorBrowsable(Never)]` on `__Render_body` keeps it out of completions and most navigators.
- `GeneratedPreview~/Test.uitkx.g.cs` will show the new shape — acceptable; it's already a debugging aid.
- Recommendation: add a one-paragraph note in `ide-extensions~/docs/` explaining the trampoline pattern (mirror what's already documented for hooks).

### 4.5 Stack traces — **+1 frame, no test impact**

- Exceptions in user body show `__Render_body` then `Render`. No tests assert on stack-trace text.
- `#line` directive on `__Render_body` ensures the source line in the user `.uitkx` file is reported correctly.

### 4.6 Reflection / API surface — **No breaking changes**

- Public `Render(IProps, IReadOnlyList<VirtualNode>)` signature unchanged.
- All existing `V.Func(MyComponent.Render, …)` and `MyComponent.Render(…)` call sites compile and run unchanged.
- No production code uses `GetMethod("Render")` reflection.

### 4.7 Tests — **Low maintenance**

- `EmitterTests.cs`: content-based assertions, not full snapshots. Add positive assertions for trampoline + body presence under `#if UNITY_EDITOR`.
- `HmrEmitterParityContractTests.cs`: must be updated to expect the trampoline on **both** SG and HMR emitters (parity preserved).
- `FormatterSnapshotTests.cs`: low risk; verify `#if UNITY_EDITOR` round-trips.
- New tests:
  - "Save same file twice" — second save still hot-swaps.
  - "Navigate away → edit → navigate back" — destination renders new code.
  - "Edit body only (same `[HookSignature]`)" — hook state preserved.
  - "Edit hook order/signature" — `FullResetComponentState` runs for matching fibers, state resets.

---

## 5. File-by-File Change List

### 5.1 Source Generator (emit trampoline on SG side)

- **[`SourceGenerator~/Emitter/CSharpEmitter.cs`](../SourceGenerator~/Emitter/CSharpEmitter.cs)**
  - Around the component class generator (Render at L265, V.Func emission at L1220 / L1304):
    - Emit `#if UNITY_EDITOR private static Func<…> __hmr_Render; #endif`.
    - Rename what is currently the public `Render` body to `__Render_body` and add `[EditorBrowsable(Never)]`.
    - Emit a new public `Render` trampoline that checks `__hmr_Render` + `HmrState.IsActive`.
    - Keep all attributes (`[UitkxElement]`, etc.) on the **class** (not the body method).
  - Reference implementation to copy: [`HookEmitter.cs#L95-L183`](../SourceGenerator~/Emitter/HookEmitter.cs#L95).

- **[`SourceGenerator~/UitkxPipeline.cs`](../SourceGenerator~/UitkxPipeline.cs)** — file classification (L57–L101): no logic change; only confirm component branch reaches the new emission code.

### 5.2 HMR Emitter (must mirror SG exactly)

- **[`Editor/HMR/HmrCSharpEmitter.cs`](../Editor/HMR/HmrCSharpEmitter.cs)** L168–L280 (component emission path): replicate the trampoline shape so newly compiled HMR types **also** expose `__hmr_Render` and `__Render_body` (consistency required by `HmrEmitterParityContractTests`). For HMR, the swapper writes the **old** assembly's `__hmr_Render` to invoke the **new** assembly's `Render` — but the new assembly emitting the trampoline keeps shape parity for chained HMR cycles.

### 5.3 New swapper (replaces fiber walk)

- **New file `Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs`** modelled on [`UitkxHmrModuleMethodSwapper.cs`](../Editor/HMR/UitkxHmrModuleMethodSwapper.cs).
  - Inputs: old `Type` (live), new `Type` (from compiled HMR asm), `[UitkxElement]` name.
  - Builds `Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>` over `newType.GetMethod("Render")`.
  - `oldType.GetField("__hmr_Render", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, newDelegate)`.
  - Calls `HasHookSignatureChanged(oldType, newType)`:
    - false → done.
    - true → enumerate fibers of this component type and call `FullResetComponentState` (existing API).

- **Delete or strip `Editor/HMR/UitkxHmrDelegateSwapper.cs`** (`WalkAndSwap`, `ExtractRenderDelegate`, `IsMatch`). Keep only any utility used by other code (verify call graph). Specifically remove the closure-wrapper code introduced as a workaround.

### 5.4 Reconciler simplification (delete fallbacks)

- **[`Shared/Core/Fiber/FiberFunctionComponent.cs`](../Shared/Core/Fiber/FiberFunctionComponent.cs)** L300–L314: delete the `HmrPreviousRender` cross-assembly compatibility fallback in `IsCompatibleType`.
- **[`Shared/Core/Fiber/FiberChildReconciliation.cs`](../Shared/Core/Fiber/FiberChildReconciliation.cs)** L283–L298: delete duplicate.
- **`HmrPreviousRender` field on `FiberNode`** and its propagation in `CloneForReuse`: keep for now (still used by reconciler crash-rollback path at FiberReconciler.cs L681–L720). Re-evaluate after refactor stabilizes.

### 5.5 Strip diagnostic instrumentation (separate cleanup commit)

See section 7.

---

## 6. Implementation Order

Each step compiles and ships independently; HMR remains functional throughout (until step 4).

1. **SG + HMR emission (atomic)** — add the trampoline to **both** `CSharpEmitter` and `HmrCSharpEmitter` in the **same commit**, together with the corresponding `HmrEmitterParityContractTests` update. Splitting these across commits would leave parity tests red on the intermediate commit. Public `Render` signature unchanged → all consumers compile. HMR still uses the old fiber-walk swapper at this point and keeps working.
2. *(folded into step 1)*
3. **New swapper** — add `UitkxHmrComponentTrampolineSwapper`. Wire into `UitkxHmrController` *alongside* the old swapper, gated by a feature flag for one cycle of testing.
4. **Switch over** — flip default to new swapper, remove old `UitkxHmrDelegateSwapper` + closure wrapper.
5. **Reconciler cleanup** — delete `IsCompatibleType` HMR fallbacks.
6. **Tests** — new regression tests for the four scenarios in §4.7.
7. **Diagnostic cleanup commit** — see §7.

---

## 7. Workspace Cleanup (Pre-Plan Hygiene)

These are leftover instrumentation from the freeze investigation. Revert before any refactor work.

- Revert (drop unstaged changes; preserve already-staged versions):
  - `Editor/EditorRenderScheduler.cs`
  - `Editor/HMR/UitkxHmrCompiler.cs`
  - `Editor/HMR/UitkxHmrController.cs`
  - `Editor/HMR/UitkxHmrDelegateSwapper.cs`
  - `Editor/HMR/UitkxHmrFileWatcher.cs`
  - `Shared/Core/Fiber/FiberReconciler.cs`
- Delete (untracked):
  - `Editor/HMR/HmrFreezeLog.cs`
  - `Editor/HMR/HmrFreezeLog.cs.meta`

---

## 8. Risks & Open Questions

- **Risk: SG emission bug ships to all users.** Mitigation: feature-flag the trampoline emission for one prerelease; keep generated diff small and snapshotted in `EmitterTests`.
- **Risk: `[HookSignature]` coverage incomplete.** If a component edits hooks but signatures don't capture the change, the trampoline swap preserves stale state. Mitigation: audit existing `[HookSignature]` emission; add tests; in doubt, prefer remount.
- **Open: derived components / inheritance.** Components are static — no inheritance. Confirmed safe.
- **Open: components with generic Render signatures.** Today none exist (only typed via props). If introduced, need to extend trampoline emitter; defer.
- **Open: rider-extension parity.** `ide-extensions~/rider/` shares LSP. No additional work needed.

---

## 9. Definition of Done

- [x] All `EmitterTests`, `HmrEmitterParityContractTests`, `FormatterSnapshotTests` green (1222 SG tests passing).
- [x] New trampoline regression tests green (`Sg_FunctionComponent_GeneratesRenderTrampoline`, `Sg_FunctionComponent_BodyContainsHooksAndSetup_TrampolineStaysThin`).
- [ ] Manual repro: 30 rapid Ctrl+S saves of `Test.uitkx` with no freeze.
- [ ] Manual repro: edit → navigate away → navigate back shows new code.
- [x] `UitkxHmrDelegateSwapper.WalkAndSwap` and the closure-wrapper code are deleted; component swap moved to `UitkxHmrComponentTrampolineSwapper`.
- [x] `IsCompatibleType` HMR fallbacks deleted from both fiber files (`FiberFunctionComponent.cs`, `FiberChildReconciliation.cs`).
- [ ] HMR latency benchmark (if added) shows < 1 ms swap on 1000-fiber tree.
- [ ] Player build size delta < 5%.
