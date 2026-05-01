# Unity Version Compatibility — Option F Implementation Plan

> **Strategy:** Hard floor at Unity 6.2, `#if UNITY_6000_3_OR_NEWER` guards for
> forward features from 6.3+. Users on 6.2 keep all current features; users on
> 6.3+ get bonus features automatically.

---

## Current State

- `package.json` floor: `"unity": "6000.2"`
- All 8 `.asmdef` files: empty `versionDefines` and `defineConstraints`
- Zero `#if UNITY_6000_*` guards in any code
- PropsApplier has no-op stubs for: `filter`, `cursor`, `transition`, `transition*`
- Style.cs: 72+ typed properties, all targeting 6.2 unconditionally

---

## What Unity 6.3 Added (relevant to us)

### New IStyle Properties (6 total)

| Property | C# Type | USS Name | Notes |
|----------|---------|----------|-------|
| `aspectRatio` | `StyleRatio` | `aspect-ratio` | Layout — width-to-height ratio |
| `filter` | `StyleList<FilterFunction>` | `filter` | We had a TODO stub! |
| `unityMaterial` | `StyleMaterialDefinition` | `-unity-material` | UI Shader Graph support |
| `unityTextAutoSize` | `StyleTextAutoSize` | `-unity-text-auto-size` | Already referenced in PropsApplier as no-op |
| `unityTextGenerator` | `StyleEnum<...>` | `-unity-text-generator` | Standard vs Advanced text |
| `unityEditorTextRenderingMode` | `StyleEnum<...>` | `-unity-editor-text-rendering-mode` | Editor only |

### New Types/Structs

| Type | Purpose |
|------|---------|
| `FilterFunction` | Struct: type (FilterFunctionType) + parameters |
| `FilterFunctionType` | Enum: Custom, Tint, Opacity, Invert, Grayscale, Sepia, Blur, Contrast, HueRotate |
| `FilterParameter` / `FilterParameterDeclaration` | Filter parameter data |
| `FilterPassContext` / `PostProcessingPass` / `PostProcessingMargins` | Custom filter infrastructure |
| `Ratio` / `StyleRatio` | Aspect-ratio value struct |
| `MaterialDefinition` / `StyleMaterialDefinition` | Material assignment struct |
| `TextAutoSize` / `StyleTextAutoSize` | Text auto-size data |
| `TextAutoSizeMode` | Enum for auto-size modes |
| `EditorTextRenderingMode` | Enum for editor text rendering |
| `FillGradient` / `GradientType` | Gradient fill types |

### Non-Style Features

| Feature | Impact on us |
|---------|-------------|
| Vector Graphics (SVG) core module | No direct impact (rendering feature) |
| Image `source` UXML attribute | Could add to schema if we support Image |
| UI Shader Graph | Consumed via `unityMaterial` style |
| Pseudo-state API | Could expose in hooks/events later |
| `TextElement.PostProcessTextVertices` | Potential hook for text animation |
| UI Test Framework | Testing utility, no product code impact |

## What Unity 6.4 Added (UI Toolkit)

- Default theme in Project Settings — editor workflow only
- Drag-and-drop UXML/USS into UI Builder — editor workflow only
- **Zero new IStyle properties, elements, or types**

So right now, Option F means exactly **one gate**: `#if UNITY_6000_3_OR_NEWER`.

---

## How Our Pipeline Works (and where version gates fit)

```
.uitkx file
    │
    ▼
┌─────────────────────────────────────────────┐
│  PARSER  (language-lib)                     │
│  Tokenize → AST (ElementNode, TextNode...)  │
│  No Unity API — pure text parsing           │
│  ✅ Version-immune                          │
└──────────────┬──────────────────────────────┘
               │ AST
    ┌──────────┴──────────┐
    ▼                     ▼
┌────────────┐    ┌──────────────┐
│ SOURCE GEN │    │  HMR EMITTER │
│ (compile)  │    │  (runtime)   │
└─────┬──────┘    └──────┬───────┘
      │                  │
      │  Both emit the SAME pattern:
      │  V.Label(new LabelProps {
      │    style = new Dictionary<string,object> { ["filter"] = value }
      │  })
      │
      │  They emit STRING KEYS, not IStyle calls.
      │  ✅ Version-immune
      │
      ▼
┌─────────────────────────────────────────────┐
│  RUNTIME — PropsApplier.cs                  │
│  Receives Dictionary<string, object>        │
│  Looks up styleSetters["filter"]            │
│  Calls e.style.filter = ...                 │
│  ⚠️ THIS IS WHERE IStyle IS TOUCHED        │
│  🔴 VERSION GATE GOES HERE                  │
└─────────────────────────────────────────────┘
```

### Key Insight

Our architecture is **already version-friendly by design**. The parser, source
generator, and HMR emitter work with **string keys** and **dictionary-based
props**. They never call `IStyle` directly. The only place that touches the
actual Unity API is `PropsApplier.cs` at runtime.

---

## Mapping: What Changes Where

### Files that need `#if UNITY_6000_3_OR_NEWER` guards

| File | What changes |
|------|-------------|
| `Shared/Props/PropsApplier.cs` | Convert `filter` no-op → real setter; add `aspectRatio`, `unityMaterial`, `unityTextAutoSize`, `unityTextGenerator`, `unityEditorTextRenderingMode` setters + resetters |
| `Shared/Props/Typed/Style.cs` | Add typed properties: `Filter`, `AspectRatio`, `UnityMaterial`, `UnityTextAutoSize`, etc. |
| `Shared/Props/Typed/CssHelpers.cs` | Add FilterFunctionType shortcuts: `Blur()`, `Grayscale()`, etc. |

### Files that need updates (no guards — just data)

| File | What changes |
|------|-------------|
| `ide-extensions~/grammar/uitkx-schema.json` | Add new styleKeyValues entries, optionally with `"sinceUnity": "6000.3"` |
| `Plans~/TECH_DEBT.md` | Update filter entry, version compatibility entry |

### Files that DON'T need changes (passthrough architecture)

| File | Why |
|------|-----|
| `Editor/HMR/HmrCSharpEmitter.cs` | Emits generic code, passes style dict through |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Emits generic code, passes style dict through |
| `Shared/Core/V.cs` | Factory methods — no style properties involved |
| `Shared/Elements/ElementRegistryProvider.cs` | No new 6.3 elements (only styles) |
| `Shared/Props/Typed/BaseProps.cs` | Universal, no style keys |
| `ide-extensions~/lsp-server/*` | Reads schema, no Unity API references |
| `ide-extensions~/language-lib/*` | Parser, no Unity API references |
| `ide-extensions~/vscode/*` | Activates LSP, no Unity API references |
| All 8 `.asmdef` files | `UNITY_6000_3_OR_NEWER` is auto-defined by Unity |

### Exception: New Elements

If Unity adds a **new built-in element** (didn't happen in 6.3/6.4), then:
- `ElementRegistryProvider.cs` — add registration behind `#if`
- `V.cs` — add factory method behind `#if`
- Create new `*Props.cs` — behind `#if`
- `PropsResolver.cs` (SourceGen) + `HmrCSharpEmitter.cs` (HMR) fallback maps — behind `#if`
- `uitkx-schema.json` — add element entry

---

## Implementation Patterns (all 4 apply together as layers)

### Pattern 1: PropsApplier — Convert existing no-op to real setter

```csharp
// Before (current 6.2 no-op):
styleSetters["filter"] = (e, v) => { };

// After:
#if UNITY_6000_3_OR_NEWER
styleSetters["filter"] = (e, v) =>
{
    if (v is StyleList<FilterFunction> fl) { e.style.filter = fl; return; }
    if (v is List<FilterFunction> list)   { e.style.filter = list; return; }
};
styleResetters["filter"] = e => e.style.filter = StyleKeyword.Null;
#else
styleSetters["filter"] = (e, v) => { };     // no-op on 6.2
styleResetters["filter"] = e => { };
#endif
```

### Pattern 2: PropsApplier — Add brand new property

```csharp
#if UNITY_6000_3_OR_NEWER
styleSetters["aspectRatio"] = (e, v) =>
{
    if (v is StyleRatio sr)      { e.style.aspectRatio = sr; return; }
    if (v is float f)            { e.style.aspectRatio = new Ratio(f); return; }
    if (v is StyleKeyword kw)    { e.style.aspectRatio = kw; return; }
};
styleResetters["aspectRatio"] = e => e.style.aspectRatio = StyleKeyword.Null;
#endif
```

### Pattern 3: Style.cs — Typed property behind guard

```csharp
#if UNITY_6000_3_OR_NEWER
        // ── Unity 6.3+ properties ──────────────────────────────────
        public StyleRatio                AspectRatio       { set => this["aspectRatio"] = value; }
        public StyleList<FilterFunction> Filter            { set => this["filter"] = value; }
        public StyleMaterialDefinition   UnityMaterial     { set => this["unityMaterial"] = value; }
        public StyleTextAutoSize         UnityTextAutoSize { set => this["unityTextAutoSize"] = value; }
#endif
```

### Pattern 4: CssHelpers — Shortcuts behind guard

```csharp
#if UNITY_6000_3_OR_NEWER
        public static FilterFunction Blur(float sigma)
        {
            var f = new FilterFunction(FilterFunctionType.Blur);
            f.AddParameter(new FilterParameter(sigma));
            return f;
        }
        public static FilterFunction Grayscale(float pct = 1f) { ... }
        // Sepia, Invert, Tint, Opacity, Contrast, HueRotate...
#endif
```

**Pattern relationship:** All 4 are layers in the same stack — not alternatives.

```
User:  new Style { Filter = new[] { Blur(5f) } }
                      │                │
                      ▼                ▼
          Pattern 3 (Style.cs)   Pattern 4 (CssHelpers.cs)
                      │
                      ▼
             Dictionary ["filter"] = List<FilterFunction>
                      │
                      ▼
          Pattern 1/2 (PropsApplier.cs)
                      │
                      ▼
             e.style.filter = value
```

- Patterns 1+2 are **mandatory** (runtime engine)
- Patterns 3+4 are **ergonomic** (users can always use raw dict keys instead)

---

## User Experience by Version

| Unity Version | Experience |
|---------------|------------|
| **6.2** (floor) | Everything works as today. `filter`, `aspectRatio`, etc. are silently ignored (no-op). No compile errors. |
| **6.3** | All new properties light up. `Filter = new[] { Blur(5f) }` works. |
| **6.4** | Same as 6.3 — nothing new was added. |
| **Future 6.5+** | Add new `#if UNITY_6000_5_OR_NEWER` blocks following same patterns. |

---

## New Feature Discovery Process (for future Unity releases)

When Unity releases a new version:

1. **Check What's New** — UI Toolkit section of release notes
2. **Compare IStyle API** — diff 6.N vs 6.N+1 property lists
3. **Check for new elements** — new VisualElement subclasses
4. **Classify each addition:**

| Type | Where it touches us |
|------|-------------------|
| New style property | PropsApplier, Style.cs, CssHelpers, StyleKeys, schema |
| New element/component | ElementRegistryProvider, V.cs, *Props.cs, PropsResolver, HmrCSharpEmitter, schema |
| New enum value on existing property | PropsApplier converter, CssHelpers shortcut, schema |
| New element attribute | *Props.cs, schema |
| New API/method | Case-by-case (hooks, ref exposure) |

5. **Implement behind `#if UNITY_XXXX_Y_OR_NEWER`**
6. **Update `uitkx-schema.json`** with optional `sinceUnity` annotation
7. **Update TECH_DEBT.md** if applicable

---

## Key Design Decisions

1. **`UNITY_6000_3_OR_NEWER` is auto-defined** — No `versionDefines` needed in asmdef.
2. **Both `#if` and `#else`** — PropsApplier keeps no-op stubs in `#else` so string keys exist on all versions (prevents KeyNotFoundException).
3. **Style.cs uses `#if` without `#else`** — Properties don't exist on 6.2. Compile-time signal: "this requires 6.3+".
4. **No runtime reflection** — Pure compile-time gating. Zero overhead.
5. **Schema annotation** — `"sinceUnity": "6000.3"` metadata for IDE warnings.
6. **package.json floor stays at 6000.2** — Never changes for forward features.
