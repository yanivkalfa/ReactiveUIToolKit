# B28 — HMR fails to refresh `static readonly` module fields

Status: **In Progress** (May 12, 2026)
Branch: `cleanup_and_upgrades`
Owner: AI / Yaniv

---

## 1. Symptom

User edits a module-scope style:

```uitkx
module Sidebar {
    public static readonly Style Wrapper = new Style {
        PaddingTop = 4,        // user changes this to 16
        BackgroundColor = ...
    };
}
```

After HMR save:
- `HomePage` (re-rendered after save) shows `PaddingTop = 16` ✓
- Navigate to `SettingsPage` (renders fresh) — shows `PaddingTop = 4` ✗

Forcing a full domain reload (exit Play mode) fixes it.

## 2. Root cause (confirmed via byte-level IL diagnostics)

**Mono JIT inlines the object reference for `ldsfld <static readonly ref field>` into machine code.**

- `Assembly-CSharp.dll`'s `Sidebar.Wrapper` field slot is updated correctly by `UitkxHmrModuleStaticSwapper.FieldInfo.SetValue(null, newStyle)`.
- The `Sidebar.Wrapper` slot in memory contains the new `Style` instance (#-630071728, PaddingTop=16).
- BUT methods JIT-compiled BEFORE the swap (e.g. `MenuPage.Render` invoked by SettingsPage) hold an INLINED reference to the OLD `Style` instance (#-740022704, PaddingTop=4). They never re-read the slot.
- Methods JIT-compiled AFTER the swap (e.g. `hmr_Sidebar_1` dll's render method) read the new slot value.
- `Assembly-wide scan` confirms no static slot in `Assembly-CSharp` holds the stale instance — it lives only in pre-JIT'd native code.

This is a well-known Mono/CoreCLR JIT optimization: `initonly` flag in IL gives the JIT license to treat the field as a constant after the type initializer runs.

## 3. Fix strategy — Option 1 (chosen)

**Drop the `readonly` (`initonly` IL flag) from every generator-emitted module-scope static that HMR needs to refresh.** Without `initonly`, the JIT cannot legally inline; every `ldsfld` reads the slot; `FieldInfo.SetValue` writes are immediately visible.

### Why not alternatives

- **Option 2 — mutate Style in-place preserving identity**: breaks the `SameInstance` reference-equality fast-paths used by `Style.SameInstance`, `BaseProps.SameInstance`, `TypedPropsApplier.DiffStyle`, `FiberReconciler` short-circuits, `PropsApplier`. Each would become a correctness bug rather than an optimization. Cross-cutting refactor required, with weaker HMR coverage. Rejected as a band-aid.
- **Option 3 — force re-JIT**: not achievable from managed Mono. Rejected.

### Identity & fast-path safety (verified)

After the fix:
- Within a single assembly load, the slot returns the same ref → `SameInstance` still hits → no perf regression in steady state.
- After an HMR swap, the slot returns a fresh ref → `SameInstance` correctly returns false → DiffStyle walks → user sees updated style. **Exactly the intended HMR behavior.**

Audited consumers:
- [`Style.SameInstance`](../Shared/Props/Typed/Style.cs#L350)
- [`BaseProps.SameInstance`](../Shared/Props/Typed/BaseProps.cs#L924)
- [`TypedPropsApplier.DiffStyle`](../Shared/Props/TypedPropsApplier.cs#L153)
- [`FiberReconciler` ReferenceEquals usage](../Shared/Core/Fiber/FiberReconciler.cs#L600)
- [`VNode.EmptyPropsInstance` check](../Shared/Core/VNode.cs#L277)
- [`PropsApplier`](../Shared/Props/PropsApplier.cs#L1334)

All ask "is this the same reference?" — none ask "is this declared readonly?". **No regression.**

### Player-build cost

Permanent IL change applies in IL2CPP and Mono AOT Player builds too. Cost per access: one extra static-slot load (~1 ns, L1-cached, single `mov`). Sidebar with ~50 buttons: ~50 ns/frame = 0.0003 % of 60 fps budget. Below noise. We deliberately keep Editor and Player IL identical so HMR remains a faithful Player preview.

## 4. Discriminator attribute

Naked `readonly` removal is dangerous: it would make every generator-emitted module field externally writable with no documentation of intent. We discriminate via attribute:

```csharp
// Shared/Core/UitkxHmrSwapAttribute.cs (new file)
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class UitkxHmrSwapAttribute : Attribute { }
```

The generator emits `[UitkxHmrSwap]` on every stripped field. The swapper's predicate becomes:

```csharp
bool eligible = HasAttribute<UitkxHmrSwapAttribute>(f) || f.IsInitOnly;
```

`IsInitOnly` is retained because the hook `_cache` field ([`HookEmitter.cs#L130`](../SourceGenerator~/Emitter/HookEmitter.cs#L130)) is intentionally `static readonly` (the `ConcurrentDictionary` reference is semantically immutable; only its contents are HMR-replaced). That site does NOT suffer JIT inlining (the ref is never replaced), so leave it as is. The swapper's existing logic for it stays valid.

This is NOT a backward-compat concession to old DLLs (per user requirement, we don't carry compat code). It serves a live semantic distinction.

## 5. Emission inventory

| # | Site | Before | After |
|---|------|--------|-------|
| 1 | [`CSharpEmitter` hoisted `__sty_N`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L3172) | `private static readonly Style __sty_N = …` | `[global::ReactiveUITK.UitkxHmrSwap] private static Style __sty_N = …` |
| 2 | [`HmrCSharpEmitter` hoisted `__sty_N`](../Editor/HMR/HmrCSharpEmitter.cs#L2160) | identical | identical |
| 3 | [`CSharpEmitter` `__uitkx_ussKeys`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L208) | `internal static readonly string[] __uitkx_ussKeys = …` | `[global::ReactiveUITK.UitkxHmrSwap] internal static string[] __uitkx_ussKeys = …` |
| 4 | [`HmrCSharpEmitter` `__uitkx_ussKeys`](../Editor/HMR/HmrCSharpEmitter.cs#L250) | identical | identical |
| 5 | [`ModuleBodyRewriter` user fields](../SourceGenerator~/Emitter/ModuleBodyRewriter.cs#L173) (currently `ToFullString()` verbatim) | `public static readonly Style Wrapper = …` (user code) | `[global::ReactiveUITK.UitkxHmrSwap] public static Style Wrapper = …` |
| 6 | [`HmrHookEmitter.EmitModules`](../Editor/HMR/HmrHookEmitter.cs#L213) (verbatim string append) | same user code | same rewrite |

**Sites NOT touched** (intentionally retained `readonly`):
- Hook `_cache` field (`__hmr_<name>_cache`): contents-mutated `ConcurrentDictionary`, ref never replaced → no JIT-inlining hazard. Stays `static readonly`.
- Static-method delegate trampoline fields (`__hmr_<name>`): already `static` (no `readonly`), already mutable. Untouched.
- Generic-hook `MethodInfo` field (`__hmr_<name>`): already `static` (no `readonly`). Untouched.
- All `static readonly` private regex / lookup tables inside the generator's OWN source (`s_hookAliases`, `s_componentTagAliases`, `s_literalCtorTypes`, etc.). These never see HMR — they're inside the analyzer DLL. Untouched.

## 6. Implementation files

### New
- `Shared/Core/UitkxHmrSwapAttribute.cs` — attribute definition.
- `SourceGenerator~/Emitter/StaticReadonlyStripper.cs` — Roslyn helper that walks a parsed body and rewrites top-level `static readonly` fields. Single source of truth for SG + HMR.
- `SourceGenerator~/Analyzers/UitkxHmrSwapWriteAnalyzer.cs` — `DiagnosticAnalyzer` that flags external writes to `[UitkxHmrSwap]` fields.
- `SourceGenerator~/Tests/StaticReadonlyStripperTests.cs` — unit tests for the rewriter.
- `SourceGenerator~/Tests/HmrSwapWriteAnalyzerTests.cs` — analyzer tests.

### Modified
- `SourceGenerator~/Emitter/ModuleBodyRewriter.cs` — extend `EmitMember` to rewrite `FieldDeclarationSyntax`.
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — change two emit strings (hoisted style, uss keys).
- `Editor/HMR/HmrCSharpEmitter.cs` — mirror the two emit-string changes.
- `Editor/HMR/HmrHookEmitter.cs` — replace verbatim body append in `EmitModules` with Roslyn-based field rewrite (mirrors `ModuleBodyRewriter` rewrite logic).
- `Editor/HMR/UitkxHmrModuleStaticSwapper.cs` — predicate accepts `[UitkxHmrSwap] OR IsInitOnly`; comment block updated to reflect the new mechanism.
- `SourceGenerator~/Diagnostics/UitkxDiagnostics.cs` — add `UITKX0210` descriptor.
- `SourceGenerator~/Tests/EmitterTests.cs` — update existing 3 `StyleHoist_*` test assertions.

## 7. Edge cases handled

- **`const` fields**: `FieldInfo.IsLiteral == true` and no `readonly` keyword. Untouched.
- **Mutable `static int counter`**: no `readonly`. Untouched. User intent preserved.
- **`public static readonly int A = 1, B = 2;`**: 1 `FieldDeclarationSyntax`, 2 `VariableDeclarator`s. Strip `readonly` from declaration; attribute on declaration (covers all variables).
- **Attributes / XML doc on field**: preserved by working at token level (`WithModifiers(...)`) and prepending to `AttributeLists`.
- **Generic field types**: Roslyn parses; reconstructed correctly.
- **Nested types inside module body**: NOT recursed into. Documented limitation. The `module Foo { class Inner { static readonly … } }` pattern is not supported for HMR re-init — users put HMR-relevant statics at module top level.
- **Static auto-properties (`public static Style X { get; } = …`)**: compiler-generated backing field `<X>k__BackingField` is `initonly`. NOT rewritten. **Documented**: use fields, not properties, for HMR-swappable module statics.
- **Field initializer with side-effects (subscribe / register)**: pre-existing limitation. HMR cctor re-runs producing duplicates. Out of scope for B28; documented in `UitkxHmrModuleStaticSwapper.cs` header.

## 8. Analyzer rule `UITKX0210`

```
UITKX0210 — Write to [UitkxHmrSwap] field outside type initializer.
Severity: Warning.
Message: "Field '{0}' is generator-managed for HMR re-initialization. Writing
to it from non-cctor code defeats hot-reload and is almost certainly a bug.
If this is intentional, suppress with `#pragma warning disable UITKX0210`."
```

Implementation: `RegisterOperationAction` on `SimpleAssignment`, `CompoundAssignment`, `Increment`, `Decrement`. Allow when containing symbol is `MethodKind.StaticConstructor`.

## 9. Documentation

- `Plans~/PRETTY_UI_HMR_BUGS.md` — mark B28 / Issue 13 resolved.
- `ReactiveUIToolKitDocs~/` module page — note that module statics are conceptually immutable; flagged by `UITKX0210`; use fields not properties.
- `UitkxHmrModuleStaticSwapper.cs` file-header doc-comment — rewrite to describe new mechanism (non-readonly + attribute) instead of "FieldInfo.SetValue bypasses readonly".

## 10. Tests to add / update

Add to `SourceGenerator~/Tests/`:
- `StaticReadonlyStripperTests`: bare field, multi-declarator, generic type, with attributes, with XML doc, const untouched, mutable untouched, nested type untouched, parse failure → verbatim fallback.
- `EmitterTests` — new: `Module_StaticReadonlyField_StripsAndAttributes`, `Module_MutableField_Untouched`, `Module_Const_Untouched`, `HoistedStyle_HasUitkxHmrSwapAttribute`, `UssKeys_HasUitkxHmrSwapAttribute`.
- `HmrSwapWriteAnalyzerTests`: write outside cctor flagged, write inside cctor allowed, reflection-style write not flagged (and not detected, by design).

Update `EmitterTests.cs` lines 1441, 1470, 1519: change `"private static readonly global::ReactiveUITK.Props.Typed.Style __sty_0"` → `"private static global::ReactiveUITK.Props.Typed.Style __sty_0"` AND assert presence of `[global::ReactiveUITK.UitkxHmrSwap]` attribute.

## 11. Manual repro test (post-fix)

1. Edit `Sidebar.uitkx` `Wrapper.PaddingTop` from current value to a new value.
2. Save.
3. Confirm `HomePage` (already mounted) shows the new value.
4. Navigate to `SettingsPage` (Sidebar re-mounts fresh).
5. Confirm `Sidebar` shows the NEW value (not the stale pre-edit one).

## 12. Out of scope

- Companion `.cs` partial-class user fields. They are not transcribed by the generator; HMR cannot touch them. Document this — users put HMR statics in `module` blocks, not in companion files.
- Side-effecting initializers (subscribe / event registration). Pre-existing limitation, not introduced by this fix.
- Static auto-properties. Documented; users should prefer fields.
