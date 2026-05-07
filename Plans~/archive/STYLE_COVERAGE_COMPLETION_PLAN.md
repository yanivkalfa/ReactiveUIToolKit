# Style Coverage Completion — Implementation Plan

> **Status:** Proposed · 2026-05-01
> **Predecessor:** [TECH_DEBT_STYLE_COVERAGE.md](./TECH_DEBT_STYLE_COVERAGE.md)
> **Scope:** Add the 13 missing `IStyle` properties (Unity 6.2 + 6.3) end-to-end
> across the typed `Style`, the dictionary fallback, the source generator
> hoisting whitelist, the HMR mirror, and the IDE schema, **without regressing
> the OPT-V2-2 static-style hoisting optimization or the bitmask diff path**.
> Add a coverage test so the next Unity release surfaces drift automatically.

---

## 1. Goals & non-goals

### Goals

1. Every `UnityEngine.UIElements.IStyle` property is callable from `.uitkx` via
   **both** declaration paths:
    - **Setter form**: `new Style { UnitySliceLeft = 8, … }`
    - **Tuple form**: `new Style { (StyleKeys.UnitySliceLeft, 8), … }`
2. Both forms reach `e.style.unitySliceLeft = 8` at runtime — no silent drops.
3. **No allocation regression.** Static-literal styles still hoist to
   `private static readonly Style __sty_N` (OPT-V2-2 Phase A); dynamic styles
   still rent from `Style.__Rent()` pool with bitmask-diff (Phase B).
4. **No coverage drift in the future.** A CI test reflects over `IStyle` and
   asserts setter / resetter / `StyleKeys` parity. Surfaces missing wires the
   moment Unity adds a property.
5. Schema coverage so VS Code / Rider / VS2022 IntelliSense lists the new keys
   and their enum values.

### Non-goals

- Implementing `cursor` (separate UX design — `Cursor` struct authoring helper).
- Adding `-unity-background-scale-mode` (deprecated by Unity in favor of
  `backgroundPositionX/Y/Size/Repeat`).
- Adding CSS-shorthand parsers (`background-position`, `transition`, …).
- Touching `IResolvedStyle` / `ICustomStyle` / `IStyleSheet` paths.

---

## 2. Properties to add

13 total. Grouped by value-type so the wiring template can be reused.

### Group A — `int` (slice edges)
| IStyle property | StyleKeys const | Style setter | Bit |
|---|---|---|---|
| `unitySliceLeft` | `UnitySliceLeft` | `int UnitySliceLeft` | `BIT_UNITY_SLICE_LEFT` |
| `unitySliceRight` | `UnitySliceRight` | `int UnitySliceRight` | `BIT_UNITY_SLICE_RIGHT` |
| `unitySliceTop` | `UnitySliceTop` | `int UnitySliceTop` | `BIT_UNITY_SLICE_TOP` |
| `unitySliceBottom` | `UnitySliceBottom` | `int UnitySliceBottom` | `BIT_UNITY_SLICE_BOTTOM` |

### Group B — `float`
| IStyle property | StyleKeys const | Style setter | Bit |
|---|---|---|---|
| `unitySliceScale` | `UnitySliceScale` | `float UnitySliceScale` | `BIT_UNITY_SLICE_SCALE` |
| `unityParagraphSpacing` | `UnityParagraphSpacing` | `float UnityParagraphSpacing` | `BIT_UNITY_PARAGRAPH_SPACING` |

### Group C — Unity enum
| IStyle property | StyleKeys const | Enum type | CssHelpers shortcuts |
|---|---|---|---|
| `unitySliceType` | `UnitySliceType` | `SliceType` | `SliceFill`, `SliceTile` |
| `unityOverflowClipBox` | `UnityOverflowClipBox` | `OverflowClipBox` | `ClipPaddingBox`, `ClipContentBox` |
| `unityTextGenerator` | `UnityTextGenerator` | `TextGeneratorType` | `TextGenStandard`, `TextGenAdvanced` |
| `unityEditorTextRenderingMode` | `UnityEditorTextRenderingMode` | `EditorTextRenderingMode` | `EditorTextSDF`, `EditorTextBitmap` (gate `#if UNITY_EDITOR`) |

### Group D — `Length`
| IStyle property | StyleKeys const | Style setter | Bit |
|---|---|---|---|
| `wordSpacing` | `WordSpacing` | `StyleLength WordSpacing` | `BIT_WORD_SPACING` |

### Group E — Compound struct
| IStyle property | StyleKeys const | Style setter | Bit | Notes |
|---|---|---|---|---|
| `textShadow` | `TextShadow` | `TextShadow TextShadow` | `BIT_TEXT_SHADOW` | Add `"TextShadow"` to `s_literalCtorTypes` so `new TextShadow(…)` literal is hoistable. |

### Group F — Reference-typed asset
| IStyle property | StyleKeys const | Style setter | Bit | Notes |
|---|---|---|---|---|
| `unityFontDefinition` | `UnityFontDefinition` | `FontDefinition UnityFontDefinition` | `BIT_UNITY_FONT_DEFINITION` | Reference-typed → reset to `default` in `__Rent()`. Not hoistable (asset-loaded). |

---

## 3. The 17 wiring layers (per property)

For each new property, touch **all** of these. Items A–H are in `Style.cs`,
J–L in the appliers, M–Q in periphery.

| # | Layer | File | Mechanical change |
|---|---|---|---|
| A | StyleKeys constant | [`Shared/Props/Typed/StyleKeys.cs`](../Shared/Props/Typed/StyleKeys.cs) | `public const string UnitySliceLeft = "unitySliceLeft";` |
| B | Bit index | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"Bit index constants" | `internal const int BIT_UNITY_SLICE_LEFT = 79;` (next free in `_setBits1`) |
| C | Backing field | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"Typed backing fields" | `internal int _unitySliceLeft;` |
| D | Public typed setter | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"Typed property setters" | Standard pattern, sets bit in `_setBits0` (bits 0-63) or `_setBits1` (bits ≥ 64). |
| E | `SetByKey` case | [`Style.cs`](../Shared/Props/Typed/Style.cs) `SetByKey(string,object)` | `case "unitySliceLeft": UnitySliceLeft = PropsApplier.ConvertToInt(value); break;` |
| F | `GetByKey` case | [`Style.cs`](../Shared/Props/Typed/Style.cs) `GetByKey(string)` | Mirror with `HasBit(...)`. |
| G | `FieldEquals` case | [`Style.cs`](../Shared/Props/Typed/Style.cs) `FieldEquals(int, Style)` | `case BIT_UNITY_SLICE_LEFT: return _unitySliceLeft == other._unitySliceLeft;` |
| H | Pool-rent reset | [`Style.cs`](../Shared/Props/Typed/Style.cs) `__Rent()` | Only for reference-typed fields (FontDefinition / TextShadow if it has heap data). Value-typed fields don't need explicit reset — `_setBits1 = 0` clears the bit and the stale value is invisible. |
| I | Apply switch (typed) | [`TypedPropsApplier.cs`](../Shared/Props/TypedPropsApplier.cs) `ApplyTypedStyleField` | `case Style.BIT_UNITY_SLICE_LEFT: el.style.unitySliceLeft = style._unitySliceLeft; break;` |
| J | Reset switch (typed) | [`TypedPropsApplier.cs`](../Shared/Props/TypedPropsApplier.cs) `ResetTypedStyleField` | `case Style.BIT_UNITY_SLICE_LEFT: el.style.unitySliceLeft = StyleKeyword.Null; break;` |
| K | Setter dispatch (dict) | [`PropsApplier.cs`](../Shared/Props/PropsApplier.cs) static-ctor `styleSetters[…]` | `styleSetters["unitySliceLeft"] = (e, v) => e.style.unitySliceLeft = ConvertToInt(v);` |
| L | Resetter dispatch (dict) | [`PropsApplier.cs`](../Shared/Props/PropsApplier.cs) static-ctor `styleResetters[…]` | Already present for slice/clip-box/paragraph (items 1-8). Add for items 9-13. |
| M | CssHelpers shortcut | [`CssHelpers.cs`](../Shared/Props/Typed/CssHelpers.cs) | Enum shortcuts only — see §2 Group C. |
| N | Hoisting whitelist | [`SourceGenerator~/Emitter/CSharpEmitter.cs`](../SourceGenerator~/Emitter/CSharpEmitter.cs) `s_literalCtorTypes` | Add `"TextShadow"` only (Group E). Everything else already accepted. |
| O | HMR mirror whitelist | [`Editor/HMR/HmrCSharpEmitter.cs`](../Editor/HMR/HmrCSharpEmitter.cs) `s_literalCtorTypes` (mirror) | Same change as N — keep mirrors in lock-step. |
| P | Schema entries | [`ide-extensions~/grammar/uitkx-schema.json`](../ide-extensions~/grammar/uitkx-schema.json) `styleKeyValues` | Add enum-value lists for `unitySliceType`, `unityOverflowClipBox`, `unityTextGenerator`, `unityEditorTextRenderingMode`. |
| Q | Schema version annotation | same `styleVersions` | `"unityEditorTextRenderingMode": { "sinceUnity": "6000.2", "context": "editor" }` etc. |

---

## 4. Phasing

### Phase 1 — int / float / Length (8 properties)

The cheapest, highest-coverage win. Adds slicing + paragraph-spacing + word-spacing.

- Properties: Group A (4) + Group B (2) + Group D (1) + the `int` part of Group C
  (`unitySliceType` enum is paired with the slice values in user docs).
- All values are JIT-trivial (int/float/Length/enum) and fit in the existing
  bitmask layout (next free bit = 79; we have plenty of room in `_setBits1`).
- No hoisting-whitelist change.
- Closes the `unity-slice-*` user-visible gap that triggered this audit.

**Subtasks (executed in this order so the build keeps passing after each step):**

1. `StyleKeys.cs` — add 8 constants.
2. `Style.cs` — bit indices + backing fields + typed setters + `SetByKey`/`GetByKey`/`FieldEquals` cases. The five switches in `Style.cs` are independent; add cases to each in one PR.
3. `CssHelpers.cs` — `SliceFill`, `SliceTile`, `ClipPaddingBox`, `ClipContentBox`.
4. `TypedPropsApplier.cs` — add `case Style.BIT_…` to **both** `ApplyTypedStyleField` and `ResetTypedStyleField`.
5. `PropsApplier.cs` — add `styleSetters[…]` entries for items 9 (`wordSpacing`) only; items 1-8 already have resetters and now get setters.
6. `uitkx-schema.json` — add `styleKeyValues` entries for `unitySliceType`, `unityOverflowClipBox`.
7. Run `dotnet test SourceGenerator~/Tests` — all snapshot tests must still pass.
8. Build VS Code extension; smoke-test IntelliSense for `(StyleKeys.UnitySlice` completion.

### Phase 2 — Group E (`textShadow`)

Slightly heavier because it needs whitelist support to remain hoistable.

1. Same A-L wiring as Phase 1 with type `TextShadow`.
2. **Hoisting whitelist update:** add `"TextShadow"` to `s_literalCtorTypes` in
   [CSharpEmitter.cs](../SourceGenerator~/Emitter/CSharpEmitter.cs#L2548) and the
   mirror [HmrCSharpEmitter.cs](../Editor/HMR/HmrCSharpEmitter.cs).
3. **Helper:** add `CssHelpers.Shadow(Vector2 offset, float blur, Color color)`
   convenience to keep `.uitkx` callsites clean.
4. **Snapshot test:** add a `TextShadow` literal to the UnitsKitchenSink sample
   and verify the generated code uses `__sty_N` (proves hoisting still applies).

### Phase 3 — Group C remaining + Group F (4 properties)

`unityTextGenerator`, `unityEditorTextRenderingMode`, `unityFontDefinition`.

1. `unityTextGenerator` + `unityEditorTextRenderingMode`: enum group, identical to Phase 1.
2. `unityFontDefinition`: reference-typed; **must** reset to `null` in `__Rent()`.
3. Documentation note in `MIGRATION_GUIDE.md`: `unityFontDefinition` takes
   precedence over `unityFont`/`fontFamily` — don't set both.

### Phase 4 — Coverage test (the lock)

The single most valuable deliverable. Add it AFTER Phase 1-3 so it passes green
on day one.

```csharp
// SourceGenerator~/Tests/IStyleCoverageTests.cs
public sealed class IStyleCoverageTests
{
    private static HashSet<string> IStyleProperties =>
        typeof(UnityEngine.UIElements.IStyle)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

    [Fact]
    public void EveryIStylePropertyHasASetter()
    {
        var setters = PropsApplier.__GetStyleSetterKeys();
        var missing = IStyleProperties.Except(setters).ToList();

        // Allow-list intentional stubs.
        missing.RemoveAll(p => p == "cursor");

        Assert.True(
            missing.Count == 0,
            $"styleSetters missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryIStylePropertyHasAResetter()
    {
        var resetters = PropsApplier.__GetStyleResetterKeys();
        var missing = IStyleProperties.Except(resetters).ToList();
        missing.RemoveAll(p => p == "cursor");

        Assert.True(
            missing.Count == 0,
            $"styleResetters missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryIStylePropertyHasAStyleKey()
    {
        var keys = typeof(StyleKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue())
            .ToHashSet();

        var missing = IStyleProperties.Except(keys).ToList();
        missing.RemoveAll(p => p == "cursor");

        Assert.True(
            missing.Count == 0,
            $"StyleKeys missing constants: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryStyleKeyConstantHasASetter()
    {
        // Catches typos: a StyleKeys constant whose value isn't recognized
        // by styleSetters silently no-ops at runtime.
        var setters = PropsApplier.__GetStyleSetterKeys();
        var keys = typeof(StyleKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue())
            .ToHashSet();

        var missing = keys.Except(setters).ToList();
        Assert.True(
            missing.Count == 0,
            $"StyleKeys with no setter: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryStyleSetterMapsToAValidIStyleProperty()
    {
        // Catches typos in the other direction: a styleSetters key that doesn't
        // correspond to an IStyle property name (e.g. "wordspacing" vs "wordSpacing").
        var setters = PropsApplier.__GetStyleSetterKeys();

        // Permitted shorthands that don't map 1:1 to IStyle.
        var shorthands = new HashSet<string> {
            "margin", "padding", "borderWidth", "borderColor", "borderRadius",
            "transition", "backgroundImageTint", "backgroundPosition",
            "fontFamily", "textAlign", "unityFontStyle"
        };

        var orphans = setters.Except(IStyleProperties).Except(shorthands).ToList();
        Assert.True(
            orphans.Count == 0,
            $"styleSetters with no matching IStyle property: {string.Join(", ", orphans)}");
    }
}
```

This requires exposing two test-only accessors on `PropsApplier`:

```csharp
#if UITKX_TEST
internal static IReadOnlyCollection<string> __GetStyleSetterKeys()  => styleSetters.Keys;
internal static IReadOnlyCollection<string> __GetStyleResetterKeys() => styleResetters.Keys;
#endif
```

Use `[InternalsVisibleTo("ReactiveUITK.Tests")]` (already set up for the
SourceGenerator tests).

### Phase 5 — Schema completion

`uitkx-schema.json` already has `styleKeyValues` for the existing enums. Add the
four new enums (Group C). The LSP already projects these into completion; no
TypeScript / extension code change needed. Just JSON.

```json
"unitySliceType":          ["sliced", "tiled"],
"unityOverflowClipBox":    ["padding-box", "content-box"],
"unityTextGenerator":      ["standard", "advanced"],
"unityEditorTextRenderingMode": ["sdf", "bitmap"]
```

### Phase 6 — Sample + docs (lightweight, optional)

1. Add `Samples/Components/SliceShowcase.uitkx` exercising 9-slice with all four
   `UnitySlice*` keys plus `UnitySliceScale` + `UnitySliceType`. Doubles as a
   visual regression test in the Editor host.
2. Update `StylingPage.tsx` in the docs site to list the new properties (already
   auto-generated from `StyleKeys` reflection if the docs build uses it; otherwise
   one-line addition).

---

## 5. Optimization-preserving design notes

### Bitmask layout

`_setBits0` is full (bits 0-63). `_setBits1` currently uses bits 64-78 (15
entries, plenty of room — `ulong` gives 64). The 13 new properties land at bits
79-91, well under the 128-bit budget. **No upgrade to `_setBits2` needed.**

### Pool-rent reset cost

Adding 13 fields to `Style` grows the struct by:
- 4 × `int` = 16 bytes (slice edges)
- 2 × `float` = 8 bytes (slice scale, paragraph spacing)
- 4 × enum (4 bytes each) = 16 bytes
- 1 × `Length` = 8 bytes (`StyleLength` is a discriminated value/keyword)
- 1 × `TextShadow` ≈ 24 bytes
- 1 × `FontDefinition` reference = 8 bytes

≈ 80 bytes per `Style` instance. With pool capacity 4096 that's 320 KB max
heap residency — no concern.

`__Rent()` only needs explicit reset for *reference* fields (clears the GC
root). New reference field: `_unityFontDefinition` → add `s._unityFontDefinition = null;`
in `__Rent()`. The other 12 are value-typed; `_setBits1 = 0` is enough because
nothing reads a field whose bit is unset.

### Diff cost

Each new bit costs **one extra `BitOps.TrailingZeroCount` iteration** *only when
that bit is set in either `prev` or `next`*. Static-style hoisting bypasses
the loop entirely (`SameInstance` returns true for `__sty_N`). For dynamic styles,
adding 13 bits to a typically-1-3-key style adds zero loop iterations.

### Hoisting classifier

`IsLiteralExpression` already accepts:
- `int`/`float`/`bool`/`string` literals (covers Groups A, B)
- Qualified identifiers like `SliceType.Sliced` or `CssHelpers.SliceFill` (covers Group C)
- `new T(literals…)` for whitelisted T (extend with `"TextShadow"` for Group E)

The only category that *can't* hoist is Group F (`unityFontDefinition`) because
it's an asset reference loaded from disk. That's correct — runtime asset loads
must occur per-render. Falls back to the existing `Style.__Rent()` pool path,
which still amortizes the allocation.

### HMR

`HmrCSharpEmitter` is a near-verbatim mirror of the source-generator emitter.
The hoisting whitelist change in Phase 2 is the only bit that needs duplication.
Everything else (typed setters, dispatch dictionaries) lives in `Shared/` which
HMR compiles against, so it just works.

---

## 6. Risk register

| Risk | Likelihood | Mitigation |
|---|---|---|
| Bit-index collision (off-by-one when assigning new bits) | Medium | Added bits go in a single PR; the `IStyleCoverage_…` test catches missing wires. Code review checklist: "next free bit?" |
| Typo in StyleKeys constant value (`"unitySliceLft"`) silently no-ops | Medium | `EveryStyleKeyConstantHasASetter` test catches it. |
| Enum value name drift (Unity renames `SliceType.Sliced` → `SliceType.NineSlice`) | Low | Conditional `#if UNITY_6000_3_OR_NEWER` if needed; `unityEditorTextRenderingMode` may also need an editor-only guard. |
| Snapshot tests break because hoisting now picks up new style literals | High (intended) | Update snapshots; this is the optimization working. |
| Adding `using static SliceType;` cascade conflicts in user code | Low | We add static *methods*, not `using static T` — no namespace pollution. |
| `TextShadow` ctor signature differs across Unity versions | Low | Use `new TextShadow { offset = …, blurRadius = …, color = … }` initializer (struct field syntax) — version-stable. |

---

## 7. Acceptance criteria

- [ ] `dotnet test SourceGenerator~/Tests` passes including the 5 new
      `IStyleCoverageTests`.
- [ ] Sample `SliceShowcase.uitkx` renders correctly in the Editor host with a
      9-slice background.
- [ ] Generated code for `style={new Style { (StyleKeys.UnitySliceLeft, 8), … }}`
      contains a `__sty_N` static field, **not** a per-render `Style.__Rent()`.
- [ ] HMR a `.uitkx` that uses `unitySliceLeft` and confirm the change is
      visible without a domain reload.
- [ ] VS Code completion in `(StyleKeys.|` lists `UnitySliceLeft` etc.
- [ ] VS Code completion in `(StyleKeys.UnitySliceType, "|` lists
      `"sliced"` and `"tiled"`.
- [ ] No allocation regression in the kitchen-sink benchmark (run
      `Diagnostics/Benchmark/BenchEditorHost`, compare GC alloc bytes/frame
      vs `main`).

---

## 8. Effort estimate

| Phase | LoC delta | Complexity | Confidence |
|---|---|---|---|
| 1 (int/float/Length, 8 props) | ~250 | Low (mechanical) | High |
| 2 (textShadow + hoist whitelist) | ~80 | Low-Med (whitelist mirror) | High |
| 3 (text-generator/font-definition + reset hook) | ~120 | Med (`__Rent()` reset) | High |
| 4 (coverage tests) | ~150 | Low | High |
| 5 (schema entries) | ~12 JSON | Trivial | High |
| 6 (sample + docs) | ~80 + docs | Low | High |
| **Total** | **~700 LoC** | **Mostly mechanical** | **High** |

All phases are independently shippable. Phase 4 (coverage test) is the highest-value
deliverable and should land **immediately after Phase 1** so that subsequent
phases get green lights from the test as they ship.

---

## 9. Result (to be filled in after implementation)

> _Placeholder._ Capture: final styleSetters / styleResetters / StyleKeys count,
> snapshot test deltas, kitchen-sink alloc comparison, IDE smoke-test screenshots.
