# Tech Debt — IStyle Coverage Gaps

> **Status:** Open · audit captured 2026-05-01
> **Scope:** Inline-style support across `Shared/Props/**`, `SourceGenerator~/`,
> `Editor/HMR/**`, and `ide-extensions~/`.
> **Companion plan:** [STYLE_COVERAGE_COMPLETION_PLAN.md](./STYLE_COVERAGE_COMPLETION_PLAN.md)

---

## TL;DR

UITKX claims to cover Unity's inline `IStyle` surface, but a cross-check against the
authoritative Unity Scripting API for both
[Unity 6.2 IStyle](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/UIElements.IStyle.html)
(85 properties) and
[Unity 6.3 IStyle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.IStyle.html)
(88 properties) shows that **13 properties are silently unsupported** and **1 is an
intentional stub**. All 13 unsupported properties accept `(StyleKeys.X, value)` /
`Style { X = value }` user input but never reach `e.style.*` at runtime — they no-op.

The miss survived development because:
1. Setter dispatch was hand-written sample-by-sample, while resetter dispatch was
   built from a systematic sweep of `IStyle`.  The two dictionaries drifted.
2. No coverage test asserts setter ⊇ resetter or setter ⊇ `IStyle.GetProperties()`.
3. Missing `StyleKeys.*` constants make the affected properties uncallable from
   `.uitkx`, so no user bug ever surfaced as a "doesn't work" report — the API just
   doesn't exist from the user's point of view.

---

## Authoritative IStyle property surface

Pulled directly from Unity Scripting API. Total surface = **88 properties** when
6.2 ∪ 6.3 is taken (6.2 = 85, 6.3 adds `aspectRatio`, `filter`, `unityMaterial`).

### Supported (73 wired, 1 intentional no-op = 74 of 88)

`alignContent`, `alignItems`, `alignSelf`, `aspectRatio` (6.3), `backgroundColor`,
`backgroundImage`, `backgroundPositionX`, `backgroundPositionY`, `backgroundRepeat`,
`backgroundSize`, `borderBottomColor`, `borderBottomLeftRadius`, `borderBottomRightRadius`,
`borderBottomWidth`, `borderLeftColor`, `borderLeftWidth`, `borderRightColor`,
`borderRightWidth`, `borderTopColor`, `borderTopLeftRadius`, `borderTopRightRadius`,
`borderTopWidth`, `bottom`, `color`, `cursor` *(intentional stub — `// TODO: StyleCursor wraps Cursor struct`)*,
`display`, `filter` (6.3), `flexBasis`, `flexDirection`, `flexGrow`, `flexShrink`,
`flexWrap`, `fontSize`, `height`, `justifyContent`, `left`, `letterSpacing`,
`marginBottom`, `marginLeft`, `marginRight`, `marginTop`, `maxHeight`, `maxWidth`,
`minHeight`, `minWidth`, `opacity`, `overflow`, `paddingBottom`, `paddingLeft`,
`paddingRight`, `paddingTop`, `position`, `right`, `rotate`, `scale`, `textOverflow`,
`top`, `transformOrigin`, `transitionDelay`, `transitionDuration`, `transitionProperty`,
`transitionTimingFunction`, `translate`, `unityBackgroundImageTintColor`, `unityFont`,
`unityFontStyleAndWeight`, `unityMaterial` (6.3), `unityTextAlign`, `unityTextAutoSize`,
`unityTextOutlineColor`, `unityTextOutlineWidth`, `unityTextOverflowPosition`,
`visibility`, `whiteSpace`, `width`.

### Missing (13 + 1 intentional) — full breakdown

| # | IStyle property | Resetter? | StyleKeys const? | `Style.*` setter? | `Style.SetByKey`? | Diagnosis |
|---|---|---|---|---|---|---|
| 1 | `unitySliceLeft` | ✅ ([PropsApplier.cs:958](../Shared/Props/PropsApplier.cs#L958)) | ❌ | ❌ | ❌ | No 9-slice sample existed; setter dispatch was hand-curated. |
| 2 | `unitySliceRight` | ✅ ([L966](../Shared/Props/PropsApplier.cs#L966)) | ❌ | ❌ | ❌ | Same. |
| 3 | `unitySliceTop` | ✅ ([L950](../Shared/Props/PropsApplier.cs#L950)) | ❌ | ❌ | ❌ | Same. |
| 4 | `unitySliceBottom` | ✅ ([L942](../Shared/Props/PropsApplier.cs#L942)) | ❌ | ❌ | ❌ | Same. |
| 5 | `unitySliceScale` | ✅ ([L974](../Shared/Props/PropsApplier.cs#L974)) | ❌ | ❌ | ❌ | Same. |
| 6 | `unitySliceType` | ✅ ([L982](../Shared/Props/PropsApplier.cs#L982)) | ❌ | ❌ | ❌ | Needs `SliceType` enum mapping; no sample. |
| 7 | `unityOverflowClipBox` | ✅ ([L926](../Shared/Props/PropsApplier.cs#L926)) | ❌ | ❌ | ❌ | No content-box-clipping use case in samples. |
| 8 | `unityParagraphSpacing` | ✅ ([L934](../Shared/Props/PropsApplier.cs#L934)) | ❌ | ❌ | ❌ | No multi-paragraph rich-text sample. |
| 9 | `wordSpacing` | ❌ | ❌ | ❌ | ❌ | **Pure oversight.** Trivial mirror of `letterSpacing`; nobody asked. |
| 10 | `textShadow` | ❌ | ❌ | ❌ | ❌ | Multi-component value (`x/y/blur/color`) needs custom converter. |
| 11 | `unityFontDefinition` | ❌ | ❌ | ❌ | ❌ | Samples only use legacy `Font` via `fontFamily` → `unityFont`; SDF/TMP path skipped. |
| 12 | `unityTextGenerator` | ❌ | ❌ | ❌ | ❌ | Standard / Advanced text generator switch — relevant for advanced text only. |
| 13 | `unityEditorTextRenderingMode` | ❌ | ❌ | ❌ | ❌ | Editor-UI only; runtime samples never needed it. |
| — | `cursor` *(intentional)* | — | ❌ | ❌ | ❌ | Marked `// TODO: StyleCursor wraps Cursor struct (Texture2D + hotspot)` at [PropsApplier.cs:666](../Shared/Props/PropsApplier.cs#L666). |

Items 1–8 have a `styleResetters[…]` entry — proving the resetter sweep was systematic
but the setter dispatch was sample-driven. Items 9–13 are missing in **both** dictionaries.

---

## How the styles reach `e.style.*` today

There are two convergent paths into `IStyle`:

```
┌─ .uitkx markup ──────────┐
│  style={new Style { … }} │
└────────────┬─────────────┘
             ▼
┌─ Source generator ───────────────────────────────────────┐
│  • OPT-V2-2 Phase A: TryHoistStaticStyle                 │
│      → emits `private static readonly Style __sty_N = …` │
│  • Else: TryExtractNewStyleInit                          │
│      → emits `var __s_X = Style.__Rent(); __s_X.Width =` │
└────────────┬─────────────────────────────────────────────┘
             ▼
┌─ Runtime ────────────────────────────────────────────────┐
│  Props.Style is set on a BaseProps subclass              │
│  (rented from BaseProps pool).                           │
└────────────┬─────────────────────────────────────────────┘
             ▼
┌─ Reconcile / commit ─────────────────────────────────────┐
│  TypedPropsApplier.ApplyDiff(el, prev, next)             │
│    └─ DiffStyle(el, prev.Style, next.Style)              │
│         └─ bitmask diff → ApplyTypedStyleField(bit)      │
│              switch(bit) case BIT_WIDTH: el.style.width  │
│         └─ removed bits → ResetTypedStyleField(bit)      │
│              switch(bit) case BIT_WIDTH: el.style.width = StyleKeyword.Null  │
└──────────────────────────────────────────────────────────┘
```

There is **also** a legacy dictionary path used by:
- Slot props (`Label`, `Input`, `VisualInput`) which remain dictionary-typed,
- `Style.SetByKey(string, object)` which the tuple form `(StyleKeys.X, val)`
  routes through (it just maps to the typed setter).

This dictionary path goes through `PropsApplier.ApplyStyle` →
`styleSetters[key](el, value)` and on remove
`styleResetters[key](el)`. **This is the path the 13 missing properties never hit.**

## Where each missing property must be added

For full parity (typed + dict + IDE + HMR + Generator) **every** new style needs entries in:

| Layer | File | What changes |
|---|---|---|
| **A. String constant** | [`Shared/Props/Typed/StyleKeys.cs`](../Shared/Props/Typed/StyleKeys.cs) | Add `public const string UnitySliceLeft = "unitySliceLeft";` etc. |
| **B. Bit index** | [`Shared/Props/Typed/Style.cs`](../Shared/Props/Typed/Style.cs) §"Bit index constants" | Add `internal const int BIT_UNITY_SLICE_LEFT = N;` (keep ordered, tracking `_setBits1`). |
| **C. Backing field** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"Typed backing fields" | Add `internal int _unitySliceLeft;` (or `StyleInt`/`StyleFloat`/etc.). |
| **D. Public typed setter** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"Typed property setters" | `public int UnitySliceLeft { get => _unitySliceLeft; set { _unitySliceLeft = value; _setBits1 \|= (1UL << (BIT_UNITY_SLICE_LEFT - 64)); } }` |
| **E. SetByKey case** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"SetByKey" | `case "unitySliceLeft": UnitySliceLeft = PropsApplier.ConvertToInt(value); break;` |
| **F. GetByKey case** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"GetByKey" | Mirror with `HasBit(BIT_UNITY_SLICE_LEFT) ? (object)_unitySliceLeft : null`. |
| **G. FieldEquals case** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §"FieldEquals" | `case BIT_UNITY_SLICE_LEFT: return _unitySliceLeft == other._unitySliceLeft;` |
| **H. CopyTo / Clear case** | [`Style.cs`](../Shared/Props/Typed/Style.cs) §two more switches | One line each. |
| **I. Pool reset** | [`Style.cs`](../Shared/Props/Typed/Style.cs) `__Rent()` | If reference-typed (e.g. `StyleFontDefinition`, `TextShadow`) reset to `default`. |
| **J. Apply/Reset (typed)** | [`TypedPropsApplier.cs`](../Shared/Props/TypedPropsApplier.cs) §"ApplyTypedStyleField" + §"ResetTypedStyleField" | `case Style.BIT_UNITY_SLICE_LEFT: el.style.unitySliceLeft = style._unitySliceLeft; break;` + reset via `StyleKeyword.Null`. |
| **K. Setter dispatch (dict)** | [`PropsApplier.cs`](../Shared/Props/PropsApplier.cs) `styleSetters[…]` | `styleSetters["unitySliceLeft"] = (e, v) => e.style.unitySliceLeft = ConvertToInt(v);` |
| **L. Resetter dispatch (dict)** | [`PropsApplier.cs`](../Shared/Props/PropsApplier.cs) `styleResetters[…]` | Already present for items 1–8; add for 9–13. |
| **M. CssHelpers shortcut** | [`CssHelpers.cs`](../Shared/Props/Typed/CssHelpers.cs) | Enum types only — e.g. `public static SliceType SliceFill => SliceType.Sliced;` |
| **N. Hoisting whitelist** | [`SourceGenerator~/Emitter/CSharpEmitter.cs`](../SourceGenerator~/Emitter/CSharpEmitter.cs) `s_literalCtorTypes` | If the value type has a literal ctor (e.g. `TextShadow`), add it here so OPT-V2-2 hoisting still applies. |
| **O. HMR mirror** | [`Editor/HMR/HmrCSharpEmitter.cs`](../Editor/HMR/HmrCSharpEmitter.cs) | Already mirrors hoisting and `using static StyleKeys`; **no change needed** unless N changes. |
| **P. IDE schema** | [`ide-extensions~/grammar/uitkx-schema.json`](../ide-extensions~/grammar/uitkx-schema.json) `styleKeyValues` | For enum-typed properties (`unitySliceType`, `unityOverflowClipBox`, …) add `"unitySliceType": ["sliced","tiled"]` so VS Code completion lists values. |
| **Q. Coverage test** | new `SourceGenerator~/Tests/PropsApplierCoverageTests.cs` | Reflect over `typeof(IStyle).GetProperties()` and assert every property name is a key in BOTH `styleSetters` and `styleResetters`. Also assert it has a `StyleKeys.*` constant. |

Steps J + K are the *only* lines that actually move the runtime needle; A–H and N–P
keep the typed/IDE story consistent.

---

## Why the optimization story stays intact after fixing this

OPT-V2-2 (static-style hoisting) keys off of literal expressions inside
`new Style { … }`. The current classifier ([`CSharpEmitter.cs:2548`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L2548))
accepts:
- Named static expressions (`StyleKeys.Width`, `CssHelpers.AlignCenter`)
- Number / string / bool literals
- `new T(literal-args…)` for whitelisted types in `s_literalCtorTypes`.

Adding new `StyleKeys.UnitySliceLeft` does **not** disturb the classifier — its
use as the LHS of a tuple, or as the RHS in `Width = StyleKeys.SomeStatic`, is
already a "named static" leaf in `IsLiteralExpression`. Two of the new properties
have non-trivial value types and need a one-line addition to the whitelist:

| New key | Value type | Whitelist needed? |
|---|---|---|
| `unitySliceLeft/Right/Top/Bottom` | `int` | No — `int` literals are accepted. |
| `unitySliceScale` | `float` | No. |
| `unitySliceType` | `enum SliceType { Sliced, Tiled }` | No — qualified enum members are accepted. |
| `unityOverflowClipBox` | `enum OverflowClipBox` | No. |
| `unityParagraphSpacing` | `float` | No. |
| `wordSpacing` | `Length` | No — `Length` already in `s_literalCtorTypes`. |
| `textShadow` | `TextShadow` (struct, ctor takes `Vector2/float/Color`) | **Yes — add `"TextShadow"` to whitelist.** |
| `unityFontDefinition` | `FontDefinition` | No — must come from `Resources.Load`/`AssetHelpers`, not hoistable; falls back to pool path. |
| `unityTextGenerator` | enum | No. |
| `unityEditorTextRenderingMode` | enum | No. |

So the optimization pass's allocation savings are preserved for static literal
declarations of every new property except `unityFontDefinition` (which is
inherently asset-loaded). For non-literal styles the existing `Style.__Rent()`
pool path still applies — the bitmask diff just gets two more bits in the
already-allocated `_setBits1`.

---

## Process gap to close

The single test that would have caught all 13 misses on day one:

```csharp
[Fact]
public void IStyleCoverage_AllPropertiesHaveSetterAndResetter()
{
    var iStyleProps = typeof(UnityEngine.UIElements.IStyle)
        .GetProperties()
        .Select(p => p.Name)
        .ToHashSet();
    var setters = PropsApplier.GetStyleSetterKeys();      // expose for testing
    var resetters = PropsApplier.GetStyleResetterKeys();
    var styleKeys = typeof(StyleKeys)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (string)f.GetValue(null))
        .ToHashSet();

    var missingSetters = iStyleProps.Except(setters).ToList();
    var missingResetters = iStyleProps.Except(resetters).ToList();
    var missingKeys = iStyleProps.Except(styleKeys).ToList();

    Assert.Empty(missingSetters);
    Assert.Empty(missingResetters);
    Assert.Empty(missingKeys);
}
```

Add a `[Theory]` variant gated on `#if UNITY_6000_3_OR_NEWER` so 6.3 deltas are
covered automatically when CI bumps the floor.

This locks the door against the next Unity-version drift.

---

## Out of scope

- **`cursor`**: deliberate stub. Adding it requires authoring a `Cursor` struct
  helper (`Texture2D` + `Vector2 hotspot`) which is a separate UX design question.
- **`-unity-background-scale-mode`**: USS-side only; Unity itself moved to
  `backgroundPositionX/Y/Size/Repeat` (already wired). Defensible omission.
- **CSS shorthands** (`background-position`, `transition`, `-unity-text-outline`):
  intentional no-ops — the long-form components are wired and the formatter
  doesn't emit shorthand.
- **`all: initial`**: accepted as a USS keyword but `IStyle` has no setter for it.
  Map to `IStyle.Clear()` if ever requested.
