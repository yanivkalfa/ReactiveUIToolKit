# Unity 6.3+ `unityMaterial` / `aspectRatio` — alias, helper, and HMR-emitter parity fix

**Status:** ⏳ PLANNED
**Author:** Investigation 2026-05-16
**Target version:** library `0.5.19`, IDE `1.2.10` (VS Code + VS 2022)

---

## 1. What triggered this

While walking a Pretty Ui consumer through wiring a UI Shader Graph material into a typed `Style.UnityMaterial`, we hit two errors:

```
Assets\UI\Pages\GamePage\components\PlayerHud\components\HealthBar\HealthBar.style.uitkx(34,27):
  error CS0246: The type or namespace name 'StyleMaterialDefinition' could not be found
Assets\UI\Pages\GamePage\components\PlayerHud\components\HealthBar\HealthBar.style.uitkx(34,55):
  error CS0246: The type or namespace name 'MaterialDefinition' could not be found
```

`Style.UnityMaterial` was added in the Unity 6.3 batch (commit-trail "6000.3 (impl) — 2026-03-24") and the typed property is gated `#if UNITY_6000_3_OR_NEWER`. The **typed property compiled fine** because `Shared/Props/Typed/Style.cs` already lives inside `using UnityEngine.UIElements;`. What broke is the **user-facing emitted `.uitkx` code**: none of the five emitters that produce per-component .cs files include `StyleMaterialDefinition` / `MaterialDefinition` in their alias list, so any user expression that names those types directly fails to resolve.

That investigation surfaced three deeper issues we want to ship in one coherent fix.

---

## 2. Findings

### Finding 1 — `.uitkx` emitters are missing aliases for the Unity 6.3 wrapper types

Library convention (rationale at [`SourceGenerator~/Emitter/CSharpEmitter.cs#L156-L158`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L156)):

> `using static StyleKeys` imports string constants (e.g. `FlexDirection = "flexDirection"`) that collide with identically-named enums/structs from `UnityEngine.UIElements`. We cannot import UIElements wholesale. Instead, targeted aliases import only the non-conflicting types that CssHelpers returns and users may reference.

Every emitter has a per-type alias block like:

```csharp
L("using EasingFunction = UnityEngine.UIElements.EasingFunction;");
L("using BackgroundRepeat = UnityEngine.UIElements.BackgroundRepeat;");
L("using TransformOrigin  = UnityEngine.UIElements.TransformOrigin;");
L("using Length           = UnityEngine.UIElements.Length;");
L("using StyleKeyword     = UnityEngine.UIElements.StyleKeyword;");
// ... 13 entries total
```

The Unity 6.3 batch (`StyleRatio` / `Ratio`, `StyleList<FilterFunction>` / `FilterFunction`, `StyleMaterialDefinition` / `MaterialDefinition`) was added to the typed Style — but **never added to any emitter's alias block**. Users cannot construct any of those wrapper types by their short name in `.uitkx` source.

Affected sites (5 emitters):

| File | Lines | Notes |
|---|---|---|
| [`SourceGenerator~/Emitter/CSharpEmitter.cs`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L158) | 158-172 | SG component emit |
| [`SourceGenerator~/Emitter/ModuleEmitter.cs`](../SourceGenerator~/Emitter/ModuleEmitter.cs#L55) | 55-67 | SG module emit |
| [`SourceGenerator~/Emitter/HookEmitter.cs`](../SourceGenerator~/Emitter/HookEmitter.cs#L61) | 61-73 | SG hook emit |
| [`Editor/HMR/HmrHookEmitter.cs`](../Editor/HMR/HmrHookEmitter.cs#L109) | 109-121, 186-198 | HMR hook emit (two blocks) |
| [`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs#L191) | 191-203 | LSP virtual document |

### Finding 2 — `HmrCSharpEmitter` is missing the *entire* alias block (parity gap)

Direct read of [`Editor/HMR/HmrCSharpEmitter.cs#L190-L204`](../Editor/HMR/HmrCSharpEmitter.cs#L190):

```csharp
L("using System;");
L("using System.Collections.Generic;");
L("using System.Linq;");
L("using ReactiveUITK;");
L("using ReactiveUITK.Core;");
L("using ReactiveUITK.Core.Animation;");
L("using ReactiveUITK.Props.Typed;");
L("using UnityEngine;");
L("using static ReactiveUITK.Props.Typed.StyleKeys;");
L("using static ReactiveUITK.Props.Typed.CssHelpers;");
L("using static ReactiveUITK.AssetHelpers;");
L("using UColor = UnityEngine.Color;");
foreach (var u in _usings)
    L($"using {u};");
L("using Color = UnityEngine.Color;");
L("");
```

The 12-alias block (`EasingFunction`, `EasingMode`, `BackgroundRepeat`, `BackgroundPosition`, `BackgroundSize`, `TransformOrigin`, `BackgroundPositionKeyword`, `BackgroundSizeType`, `Repeat`, `Length`, `StyleKeyword`, `TextAutoSizeMode`) that exists in **every other emitter** is **completely absent**. This violates the SG ↔ HMR parity contract that the codebase invests heavily in (see `SourceGenerator~/Tests/HmrEmitterParityContractTests.cs`).

Latent bug — has not blown up because most users go through `CssHelpers.Easing(...)` / `CssHelpers.Pct(...)` / etc., so the wrapper type names rarely appear verbatim in source. But:

- Any HMR-recompiled component body that names `EasingFunction` / `Length` / `TransformOrigin` etc. directly compiles in cold builds (SG path) and **fails on first save under HMR with CS0246**.
- The Filter / AspectRatio / UnityMaterial 6.3 types share the same fate even if Finding 1 is fixed in the SG emitters but not here.

### Finding 3 — `CssHelpers` is missing factories for the 6.3 wrapper types

Library convention: every UIElements wrapper type that a user might want to construct gets a factory in `CssHelpers` — that's the whole reason `using static CssHelpers;` is in every emitter's preamble. Established precedent at [`Shared/Props/Typed/CssHelpers.cs#L302`](../Shared/Props/Typed/CssHelpers.cs#L302):

```csharp
/// <summary>Wrap a legacy <see cref="Font"/> into a <see cref="FontDefinition"/>.</summary>
public static FontDefinition FontDef(Font font) => FontDefinition.FromFont(font);
```

Same shape for `BgRepeat(...)`, `BgPos(...)`, `BgSize(...)`, `Origin(...)`, `Xlate(...)`, `Easing(...)`, `Shadow(...)`, `FilterBlur/Grayscale/Contrast/HueRotate/Invert/Opacity/Sepia/Tint(...)`.

The 6.3 batch added 8 `Filter*` helpers ([CssHelpers.cs#L233-L270](../Shared/Props/Typed/CssHelpers.cs#L233)) but **never added the corresponding `MaterialDef` and `Ratio` factories**.

### Finding 4 — Untyped `unityMaterial` setter does not accept bare `Material`

[`Shared/Props/PropsApplier.cs#L653-L660`](../Shared/Props/PropsApplier.cs#L653):

```csharp
styleSetters["unityMaterial"] = (e, v) =>
{
    if (v is StyleMaterialDefinition smd)
        e.style.unityMaterial = smd;
    else if (v is MaterialDefinition md)
        e.style.unityMaterial = new StyleMaterialDefinition(md);
};
```

So `(StyleKeys.UnityMaterial, Asset<Material>("./x.mat"))` silently no-ops — the registry returns a `Material`, no branch matches, the falls through.

Precedent for accepting multiple shapes is the **AspectRatio** setter (also Unity 6.3 batch), which already accepts `StyleRatio | Ratio | float | int`. We adopt the same multi-shape philosophy for `unityMaterial`: also accept bare `Material`.

---

## 3. Out-of-scope (already correct)

Audited and confirmed correct, no changes needed:

- `Shared/Props/Typed/Style.cs` — typed property + field + bit set + FromString + Reset all guarded.
- `Shared/Props/Typed/StyleKeys.cs` — `UnityMaterial = "unityMaterial"` constant, guarded.
- `Shared/Props/PropsApplier.cs` resetter (line 1080) — guarded.
- `Shared/Props/TypedPropsApplier.cs` — apply (line 716) + reset (line 1058), both guarded.
- `ide-extensions~/grammar/uitkx-schema.json` line 2229 — `unityMaterial` schema entry with `sinceUnity: "6000.3"`.
- `ide-extensions~/lsp-server/Tests/VersionTests.cs` line 196 — schema test passes.
- `ReactiveUIToolKitDocs~/src/versionManifest.ts` lines 69, 176-188 — entry + canonical-form example.
- `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/stylePropertyCatalog.ts` lines 926-933 — catalog entry (will be touched in §6 to introduce the new helper-form example, see "Documentation").

---

## 4. Implementation plan

### 4.1 — Add UIElements aliases for Unity 6.3 types in all 5 emitters

In each of the 5 sites listed in Finding 1, append a guarded block immediately after the existing alias list:

```csharp
#if UNITY_6000_3_OR_NEWER
L("using FilterFunction         = UnityEngine.UIElements.FilterFunction;");
L("using Ratio                  = UnityEngine.UIElements.Ratio;");
L("using StyleRatio             = UnityEngine.UIElements.StyleRatio;");
L("using MaterialDefinition     = UnityEngine.UIElements.MaterialDefinition;");
L("using StyleMaterialDefinition = UnityEngine.UIElements.StyleMaterialDefinition;");
#endif
```

Pre-6.3 builds get nothing (the typed `Style.UnityMaterial` itself is gated, so users on older Unity can't reference these types regardless — the gate is purely defensive against `EditorPreprocessorSymbols` resolution leaking the symbol on multi-target consumers).

**Note on `FilterFunction`:** today, `Filter` is reachable via `CssHelpers.FilterBlur(...)` etc., which return `FilterFunction`. Adding the alias doesn't change runtime behavior; it lets users construct `FilterFunction` instances explicitly if they ever want to.

### 4.2 — Bring `HmrCSharpEmitter` to alias parity with `CSharpEmitter`

After [`HmrCSharpEmitter.cs#L204`](../Editor/HMR/HmrCSharpEmitter.cs#L204) (`L("using Color = UnityEngine.Color;");`), insert the same 12-line alias block that exists in `CSharpEmitter.cs#L159-L172`, **plus** the new 5-line 6.3 block from §4.1:

```csharp
L("using EasingFunction        = UnityEngine.UIElements.EasingFunction;");
L("using EasingMode            = UnityEngine.UIElements.EasingMode;");
L("using BackgroundRepeat      = UnityEngine.UIElements.BackgroundRepeat;");
L("using BackgroundPosition    = UnityEngine.UIElements.BackgroundPosition;");
L("using BackgroundSize        = UnityEngine.UIElements.BackgroundSize;");
L("using TransformOrigin       = UnityEngine.UIElements.TransformOrigin;");
L("using BackgroundPositionKeyword = UnityEngine.UIElements.BackgroundPositionKeyword;");
L("using BackgroundSizeType    = UnityEngine.UIElements.BackgroundSizeType;");
L("using Repeat                = UnityEngine.UIElements.Repeat;");
L("using Length                = UnityEngine.UIElements.Length;");
L("using StyleKeyword          = UnityEngine.UIElements.StyleKeyword;");
L("using TextAutoSizeMode      = UnityEngine.UIElements.TextAutoSizeMode;");
#if UNITY_6000_3_OR_NEWER
L("using FilterFunction         = UnityEngine.UIElements.FilterFunction;");
L("using Ratio                  = UnityEngine.UIElements.Ratio;");
L("using StyleRatio             = UnityEngine.UIElements.StyleRatio;");
L("using MaterialDefinition     = UnityEngine.UIElements.MaterialDefinition;");
L("using StyleMaterialDefinition = UnityEngine.UIElements.StyleMaterialDefinition;");
#endif
```

### 4.3 — Add `MaterialDef` and `Ratio` factories to `CssHelpers`

Append to [`Shared/Props/Typed/CssHelpers.cs`](../Shared/Props/Typed/CssHelpers.cs) inside the existing `#if UNITY_6000_3_OR_NEWER` block (around line 270, immediately after the Filter helpers):

```csharp
/// <summary>
/// Wrap a Unity <see cref="Material"/> into a <see cref="StyleMaterialDefinition"/>
/// suitable for assignment to <c>Style.UnityMaterial</c>. Mirrors the
/// <see cref="FontDef"/> pattern for the Unity 6.3 <c>-unity-material</c> property.
/// </summary>
public static StyleMaterialDefinition MaterialDef(Material material) =>
    new StyleMaterialDefinition(new MaterialDefinition(material));

/// <summary>
/// Wrap a numeric value into a <see cref="StyleRatio"/> suitable for assignment to
/// <c>Style.AspectRatio</c>. Mirrors the construction pattern used by other
/// 6.3 wrapper helpers.
/// </summary>
public static StyleRatio Ratio(float value) => new StyleRatio(new Ratio(value));
```

Naming: `MaterialDef` mirrors the established `FontDef` precedent (both wrap a non-UIElements Unity engine type into a UIElements wrapper struct). `Ratio` is a direct constructor wrapper — the alias from §4.1 brings `Ratio` into scope so the type name doesn't shadow the static helper (C# resolves the static method `Ratio(float)` and the type `Ratio` unambiguously by usage context).

### 4.4 — Accept bare `Material` in untyped applier

In [`Shared/Props/PropsApplier.cs#L653`](../Shared/Props/PropsApplier.cs#L653), append the new branch:

```csharp
styleSetters["unityMaterial"] = (e, v) =>
{
    if (v is StyleMaterialDefinition smd)
        e.style.unityMaterial = smd;
    else if (v is MaterialDefinition md)
        e.style.unityMaterial = new StyleMaterialDefinition(md);
    else if (v is Material mat)                            // NEW
        e.style.unityMaterial = new StyleMaterialDefinition(new MaterialDefinition(mat));
};
```

Mirrors the `aspectRatio` multi-shape acceptance pattern.

The typed setter ([`TypedPropsApplier.cs#L716`](../Shared/Props/TypedPropsApplier.cs#L716)) is unchanged — typed flows go through `Style.UnityMaterial` which is strongly typed to `StyleMaterialDefinition`.

---

## 5. Test coverage

### 5.1 — Emitter alias parity test (catches Finding 2 forever)

Add a new row to [`SourceGenerator~/Tests/HmrEmitterParityContractTests.cs`](../SourceGenerator~/Tests/HmrEmitterParityContractTests.cs):

```csharp
[Fact]
public void HmrCSharpEmitter_emits_same_UIElements_alias_block_as_CSharpEmitter()
{
    // Both emitters must share an identical UIElements alias prefix
    // so that user code reachable in both cold-build and HMR-recompile
    // paths resolves the same set of short names.
    var sgUsings   = ExtractAliasBlock(EmitWith<CSharpEmitter>(SAMPLE_COMPONENT));
    var hmrUsings  = ExtractAliasBlock(EmitWith<HmrCSharpEmitter>(SAMPLE_COMPONENT));
    Assert.Equal(sgUsings, hmrUsings);
}
```

Where `ExtractAliasBlock` selects all `using X = UnityEngine.UIElements.X;` lines and returns them as an ordered list.

### 5.2 — Per-emitter alias presence tests

For each of the 5 emitters, assert the new 6.3 aliases appear in the emitted preamble:

```csharp
Assert.Contains("using StyleMaterialDefinition = UnityEngine.UIElements.StyleMaterialDefinition;", emitted);
Assert.Contains("using MaterialDefinition = UnityEngine.UIElements.MaterialDefinition;", emitted);
Assert.Contains("using StyleRatio = UnityEngine.UIElements.StyleRatio;", emitted);
Assert.Contains("using Ratio = UnityEngine.UIElements.Ratio;", emitted);
Assert.Contains("using FilterFunction = UnityEngine.UIElements.FilterFunction;", emitted);
```

### 5.3 — `CssHelpers.MaterialDef` / `Ratio` unit tests

Likely target file: existing `CssHelpers` test suite (locate during implementation; if none exists, add one beside the typed-Style tests).

```csharp
[Fact]
public void MaterialDef_wraps_material_into_StyleMaterialDefinition()
{
    var mat = new Material(Shader.Find("Sprites/Default"));
    var smd = CssHelpers.MaterialDef(mat);
    Assert.Same(mat, smd.value.material);
}

[Fact]
public void Ratio_wraps_float_into_StyleRatio()
{
    var sr = CssHelpers.Ratio(1.5f);
    Assert.Equal(1.5f, sr.value.value, precision: 5);
}
```

### 5.4 — Untyped applier accepts bare `Material`

Add to the existing PropsApplier test suite:

```csharp
[Fact]
public void UnityMaterialSetter_accepts_bare_Material()
{
    var mat = new Material(Shader.Find("Sprites/Default"));
    var el  = new VisualElement();
    PropsApplier.ApplyStyleProperty(el, "unityMaterial", mat);
    Assert.Equal(mat, el.style.unityMaterial.value.value.material);
}
```

(Adjust the test entrypoint to whatever the existing suite uses — `ApplyStyleProperty` shown for illustration.)

### 5.5 — End-to-end uitkx smoke test

Add a sample `.uitkx` in `SourceGenerator~/Tests/Fixtures/` (or wherever sample fixtures live) that uses all four idioms:

```csharp
public static Style Test() => new Style {
    AspectRatio   = Ratio(1.5f),
    UnityMaterial = MaterialDef(Asset<Material>("./test.mat")),
    Filter        = new StyleList<FilterFunction>(new List<FilterFunction> { FilterBlur(2f) }),
};
```

Assert: SG output compiles. HMR re-emit of the same source compiles. LSP virtual document compiles. All three under Unity 6000.3+ symbol set.

---

## 6. Documentation updates

### 6.1 — Style property catalog (docs site)

Update [`ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/stylePropertyCatalog.ts`](../ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/stylePropertyCatalog.ts):

- `unityMaterial` row (line 926): change `typedExample` from
  ```
  UnityMaterial = new StyleMaterialDefinition(\n    new MaterialDefinition(myMaterial))
  ```
  to
  ```
  UnityMaterial = MaterialDef(myMaterial)
  ```
  Add a note: "Accepts `StyleMaterialDefinition`, `MaterialDefinition`, or a bare `Material` via the untyped path."
- `aspectRatio` row: update typed example to `AspectRatio = Ratio(1.5f)`.

### 6.2 — Version manifest

Update [`ReactiveUIToolKitDocs~/src/versionManifest.ts`](../ReactiveUIToolKitDocs~/src/versionManifest.ts) lines 176-188 — replace the multi-line `new StyleMaterialDefinition(new MaterialDefinition(myMaterial))` example with the new `MaterialDef(myMaterial)` form. Mention both forms (helper preferred, explicit construction also valid).

### 6.3 — CssHelpers reference page

If there is a dedicated CssHelpers docs page (search `ReactiveUIToolKitDocs~/src/pages/` for one), add `MaterialDef` and `Ratio` to the helper inventory under a "Unity 6.3+" section. If no such page exists, file a TODO note in the docs site `README.md` rather than spinning up a new page in this changeset.

### 6.4 — Style guide / sample uitkx

Search `Samples/` and `ReactiveUIToolKitDocs~/` for any `.uitkx` snippet that demonstrates `-unity-material` or `aspect-ratio`. Update each to use the helper form so downstream copy-paste lands on the idiomatic shape.

### 6.5 — Discord changelog

Per `Plans~/DISCORD_CHANGELOG.md` rules (≤2000 chars, ASCII, prepended): one short paragraph announcing IDE 1.2.10 + library 0.5.19 with the new `MaterialDef(...)` / `Ratio(...)` helpers, the bare-`Material` acceptance, and the HMR alias-parity bugfix.

### 6.6 — Root `CHANGELOG.md` (Keep-a-Changelog)

Library changelog entry under `0.5.19`:

```
### Added
- `CssHelpers.MaterialDef(Material)` factory for `Style.UnityMaterial` (Unity 6.3+).
- `CssHelpers.Ratio(float)` factory for `Style.AspectRatio` (Unity 6.3+).
- Untyped `unityMaterial` style setter now accepts bare `Material` in addition to
  `StyleMaterialDefinition` / `MaterialDefinition`, mirroring the multi-shape
  acceptance the `aspectRatio` setter already implements.

### Fixed
- Generated `.uitkx` source now imports `StyleMaterialDefinition`,
  `MaterialDefinition`, `StyleRatio`, `Ratio`, and `FilterFunction` so users on
  Unity 6.3+ can construct those types by their short names without
  fully-qualifying. Fix applied across all 5 emitters (SG component, SG module,
  SG hook, HMR hook x2, LSP virtual document).
- `HmrCSharpEmitter` now emits the same UIElements alias block as `CSharpEmitter`
  (`EasingFunction`, `EasingMode`, `BackgroundRepeat`, `BackgroundPosition`,
  `BackgroundSize`, `TransformOrigin`, `BackgroundPositionKeyword`,
  `BackgroundSizeType`, `Repeat`, `Length`, `StyleKeyword`, `TextAutoSizeMode`).
  Closes a long-standing SG ↔ HMR parity gap that would have surfaced as CS0246
  the moment a user named one of those types verbatim in HMR-recompiled code.
```

### 6.7 — IDE shared changelog

`scripts/changelog.mjs add --scope shared --message-file <path> --vscode 1.2.10 --vs2022 1.2.10` with a message summarizing the LSP virtual-document alias additions (mirrors §6.6 phrased for IDE consumers).

---

## 7. Versioning

| Surface | From | To | Reason |
|---|---|---|---|
| `package.json` (library) | 0.5.18 | **0.5.19** | Additive helpers + applier expansion + emitter bugfix (no API removals, all guarded behind `UNITY_6000_3_OR_NEWER`) |
| `ide-extensions~/vscode/package.json` | 1.2.9 | **1.2.10** | LSP virtual document alias additions |
| `ide-extensions~/visual-studio/UitkxVsix/source.extension.vsixmanifest` | 1.2.9 | **1.2.10** | Bundled LSP server gets the same fix |

---

## 8. Files touched

| Layer | File | Change |
|---|---|---|
| **SG emit** | `SourceGenerator~/Emitter/CSharpEmitter.cs` | Add 5 guarded aliases |
| **SG emit** | `SourceGenerator~/Emitter/ModuleEmitter.cs` | Add 5 guarded aliases |
| **SG emit** | `SourceGenerator~/Emitter/HookEmitter.cs` | Add 5 guarded aliases |
| **HMR emit** | `Editor/HMR/HmrHookEmitter.cs` | Add 5 guarded aliases in BOTH alias blocks |
| **HMR emit** | `Editor/HMR/HmrCSharpEmitter.cs` | Add 12 baseline aliases + 5 guarded aliases (parity fix) |
| **LSP virtual doc** | `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Add 5 guarded aliases |
| **CssHelpers** | `Shared/Props/Typed/CssHelpers.cs` | Add `MaterialDef` + `Ratio` factories (guarded) |
| **Untyped applier** | `Shared/Props/PropsApplier.cs` | Add `else if (v is Material)` branch (guarded) |
| **Tests** | `SourceGenerator~/Tests/HmrEmitterParityContractTests.cs` | Add alias-parity contract test |
| **Tests** | `SourceGenerator~/Tests/EmitterTests.cs` (or per-emitter test files) | Add per-emitter 6.3 alias presence tests |
| **Tests** | `Shared/Tests/CssHelpersTests.cs` (or appropriate target — locate during impl) | Add `MaterialDef` / `Ratio` unit tests |
| **Tests** | PropsApplier tests | Add bare-`Material` acceptance test |
| **Tests** | `SourceGenerator~/Tests/Fixtures/UnityMaterial.uitkx` | New fixture exercising all four idioms end-to-end |
| **Docs** | `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/stylePropertyCatalog.ts` | Update `unityMaterial` + `aspectRatio` typed examples |
| **Docs** | `ReactiveUIToolKitDocs~/src/versionManifest.ts` | Update example to `MaterialDef(...)` form |
| **Docs** | `Samples/` and `ReactiveUIToolKitDocs~/` `.uitkx` snippets that mention `-unity-material` / `aspect-ratio` | Migrate to helper form |
| **Versioning** | `package.json` | 0.5.18 → 0.5.19 |
| **Versioning** | `ide-extensions~/vscode/package.json` | 1.2.9 → 1.2.10 |
| **Versioning** | `ide-extensions~/visual-studio/UitkxVsix/source.extension.vsixmanifest` | 1.2.9 → 1.2.10 |
| **Changelogs** | `CHANGELOG.md` (root) | New 0.5.19 entry |
| **Changelogs** | `ide-extensions~/changelog.json` (via script) | New shared entry, vscode 1.2.10, vs2022 1.2.10 |
| **Changelogs** | `Plans~/DISCORD_CHANGELOG.md` | Prepend short release note |

---

## 9. Acceptance criteria

1. SG cold-build of a `.uitkx` containing `UnityMaterial = MaterialDef(Asset<Material>("./x.mat"))` compiles clean. **Baseline blocked today** by Finding 1.
2. SG cold-build of a `.uitkx` containing `UnityMaterial = new StyleMaterialDefinition(new MaterialDefinition(...))` (the explicit form, e.g. legacy code) compiles clean. **Baseline blocked today** by Finding 1.
3. HMR re-emit of the same `.uitkx` after a user save compiles clean. **Baseline blocked today** by Findings 1 + 2.
4. Untyped flow `(StyleKeys.UnityMaterial, asset)` where `asset` is a bare `Material` applies the material to the element. **Baseline silently no-ops today** (Finding 4).
5. LSP virtual document for the same `.uitkx` resolves all four wrapper types — no red squiggles, hover/go-to-def both function. **Baseline blocked today** by Finding 1 in `VirtualDocumentGenerator`.
6. `HmrEmitterParityContractTests` includes a row that fails if `HmrCSharpEmitter` and `CSharpEmitter` ever drift on the alias block again.
7. All existing tests still pass; new tests pass.
8. Library / IDE changelog entries are present and accurate. Discord entry prepended.
9. Docs catalog and version manifest examples render the new helper form.

---

## 10. Out-of-scope for this changeset (deferred / acknowledged)

- **UI Shader Graph subtarget validation diagnostic** (UITKX0xxx warning when an assigned `Material` uses a shader that isn't on the UI subtarget). Unity already emits its own runtime warning (`Selected material 'X' is not compatible with UITK`) which is sufficient for now. Worth revisiting if user reports show people getting tripped up.
- **`StyleList<FilterFunction>` factory in CssHelpers** (something like `Filters(params FilterFunction[])`). Not required to fix the current issues; could be added in a follow-up DX pass.
- **Auto-injection of `using UnityEngine.UIElements;` instead of per-type aliases.** Tempting but blocked by the deliberate design decision documented at `CSharpEmitter.cs#L156-L158` — the `using static StyleKeys` constants collide. Would require renaming the StyleKeys constants to non-clashing identifiers, which is a far larger change with downstream ripple. Not in scope.
- **HMR sample fixture for the parity contract test.** The new `HmrCSharpEmitter_emits_same_UIElements_alias_block_as_CSharpEmitter` test uses a small inline sample; we are not introducing a full per-emitter golden file in this pass.
