# Unity Version Compatibility — Process & Implementation Plan

> **Purpose:** Systematic tracking, discovery, and implementation of Unity UI Toolkit  
> API changes across versions. Covers new features, deprecations, and removals.  
> **Floor version:** Unity 6.2 (6000.2)  
> **Strategy:** Option F — hard floor, `#if` guards for forward features  

---

## Status Key

| Symbol | Meaning |
|--------|---------|
| `[ ]` | Not started |
| `[~]` | In progress |
| `[x]` | Completed |
| `[t]` | Tested (verified in CI or manually) |

---

## Table of Contents

1. [Discovery Process — How to Find What Changed](#1-discovery-process)
2. [Classification — What Types of Changes Exist](#2-classification)
3. [Version Coverage Matrix — Tracking What We Support](#3-version-coverage-matrix)
4. [Implementation Checklist — Per Change Type](#4-implementation-checklist)
5. [Schema Version-Awareness — LSP Integration](#5-schema-version-awareness)
6. [Deprecation Handling — When Unity Removes Things](#6-deprecation-handling)
7. [Automated API Diff Script](#7-automated-api-diff-script)
8. [Codebase Surface Inventory](#8-codebase-surface-inventory)
9. [Rollout Tasks — Infrastructure to Build](#9-rollout-tasks)

---

## 1. Discovery Process

When a new Unity version is released, check these sources in order.
Each source serves a different purpose — none is sufficient alone.

### 1.1 Primary Sources (check every release)

#### A. "What's New" page — high-level feature overview
- **URL pattern:** `https://docs.unity3d.com/{VERSION}/Documentation/Manual/WhatsNewUnity{MAJOR}{MINOR}.html`
- **Examples:**
  - 6.3: `https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html`
  - 6.4: `https://docs.unity3d.com/6000.4/Documentation/Manual/WhatsNewUnity64.html`
- **What to check:** Scroll to **"UI Toolkit"** section
- **Catches:** New elements, new USS properties, new major features (filters, shader graph, etc.)
- **Misses:** May not mention every individual IStyle property addition; won't list types/enums

#### B. "Upgrade Guide" page — breaking changes & deprecations
- **URL pattern:** `https://docs.unity3d.com/{VERSION}/Documentation/Manual/UpgradeGuideUnity{MAJOR}{MINOR}.html`
- **Examples:**
  - 6.3: `https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html`
  - 6.4: `https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity64.html`
- **What to check:** Scroll to **"UI Toolkit"** section
- **Catches:** Removed APIs, renamed types, changed signatures, deprecations, USS parser changes
- **Misses:** New additions (only covers breaking changes)

#### C. IStyle API Reference — exhaustive property-level diff
- **URL pattern:** `https://docs.unity3d.com/{VERSION}/Documentation/ScriptReference/UIElements.IStyle.html`
- **What to do:** Compare property list between previous version and new version
- **Catches:** Every single IStyle property addition, removal, or type change
- **Misses:** Non-style additions (new elements, new events, new methods)

#### D. USS Supported Properties page — style property reference
- **URL pattern:** `https://docs.unity3d.com/{VERSION}/Documentation/Manual/UIE-USS-SupportedProperties.html`
- **What to check:** Full table of supported USS properties and their types
- **Catches:** New CSS property names, value types, value ranges
- **Misses:** C# API details (struct constructors, enum members)

#### E. UIElements namespace — structs, enums, classes, interfaces
- **URL pattern:** `https://docs.unity3d.com/{VERSION}/Documentation/ScriptReference/30_search.html?q=UIElements`
- **Better — check these sub-pages directly:**
  - **Classes:** `…/ScriptReference/UIElements.{ClassName}.html`
  - **Structs:** (same pattern — FilterFunction, Ratio, etc.)
  - **Enums:** (same pattern — FilterFunctionType, TextAutoSizeMode, etc.)
  - **Interfaces:** (same pattern — IStyle, IResolvedStyle, etc.)
- **What to do:** Compare struct/enum/class lists between versions
- **Catches:** New types we might want to expose (FilterFunction, MaterialDefinition, etc.)
- **Misses:** Subtle signature changes within existing types

### 1.2 Secondary Sources (check as needed)

#### F. Unity Release Notes (detailed changelog)
- **URL pattern:** `https://unity.com/releases/editor/whats-new/{VERSION}`
- **Example:** `https://unity.com/releases/editor/whats-new/6000.3.0`
- **Note:** Extremely granular — hundreds of entries. Filter for "UIElements" or "UI Toolkit"
- **Catches:** Bug fixes that change behavior, minor API additions not in "What's New"
- **Format:** Dynamic page — may need to expand sections manually

#### G. Unity Discussions / Blog Posts
- **URL:** `https://discussions.unity.com/` — search for "UI Toolkit" + version
- **Example thread:** Feature highlights often posted by Unity team
- **Catches:** Context, rationale, upcoming plans, community feedback on changes
- **Misses:** Not authoritative — may be incomplete or speculative

#### H. Unity Issue Tracker
- **URL:** `https://issuetracker.unity3d.com/`
- **When to check:** If a property we use starts behaving differently after upgrade
- **Catches:** Known bugs, regressions, planned fixes

### 1.3 What to Look For in Each Source

| What | Where to find it | How to check |
|------|------------------|-------------|
| New IStyle property | C (API ref), D (USS props) | Compare property lists |
| New VisualElement subclass | A (What's New), E (namespace browse) | Search for new class names |
| New enum value on existing enum | E (enum ref pages) | Compare enum member lists |
| New struct type | E (structs list) | Compare structs list |
| New event type | E (classes ending in "Event") | Compare event class list |
| Deprecated API | B (Upgrade Guide) | Read UI Toolkit section |
| Removed API | B (Upgrade Guide) | Read UI Toolkit section |
| Changed type signature | B (Upgrade Guide), C (API ref) | Compare property types |
| New element attribute | E (class ref) | Check UXML attributes section |
| USS parser behavior change | B (Upgrade Guide) | Read UI Toolkit section |

---

## 2. Classification

Every change Unity makes falls into ONE of these categories.
Each has a different impact and implementation path.

### 2.1 Additions

| Sub-type | Example | Impact Level |
|----------|---------|-------------|
| **New IStyle property** | `filter`, `aspectRatio` | Medium — 6 files |
| **New element/component** | (none in 6.3/6.4, but e.g. hypothetical `CalendarPicker`) | High — 9+ files + new Props class |
| **New enum value on existing property** | e.g. `Position.Sticky` | Low — 2-3 files |
| **New struct type** | `FilterFunction`, `Ratio` | Medium — CssHelpers + PropsApplier |
| **New event type** | (hypothetical `PointerPressEvent`) | Low — BaseProps + schema |
| **New element attribute** | Image `source` UXML attr | Low — Props class + schema |

### 2.2 Deprecations

| Sub-type | Example | Impact Level |
|----------|---------|-------------|
| **Property deprecated** | (hypothetical: `unityBackgroundImageTintColor` deprecated in favor of `filter: tint()`) | Medium — keep both, warn |
| **Element deprecated** | (hypothetical: `IMGUIContainer` deprecated) | Low — keep working, add warning |
| **Enum value deprecated** | `AccessibilityNode.selected` → `.invoked` | Low — update shortcuts |
| **Type renamed** | (historical: `Experimental.UIElements` → `UIElements`) | High — full migration |

### 2.3 Removals

| Sub-type | Example | Impact Level |
|----------|---------|-------------|
| **Property removed** | (hasn't happened for IStyle yet) | Critical — `#if !VERSION` guard |
| **Type removed** | (historical: various Experimental APIs) | Critical — `#if` guard |
| **Element removed** | (hasn't happened yet) | Critical — `#if` guard |

### 2.4 Behavior Changes (no API change)

| Sub-type | Example | Impact Level |
|----------|---------|-------------|
| **USS parser stricter** | Unity 6.3 USS parser upgrade | None — runtime code unaffected |
| **Default value change** | (hypothetical: `flexDirection` default changes) | None — we set explicit values |
| **Rendering behavior** | (hypothetical: border rendering change) | None — visual only |

---

## 3. Version Coverage Matrix

This is the **source of truth** for what we track vs what we support.
Updated whenever a new Unity version is analyzed.

### 3.1 IStyle Properties

Status: `✅` = supported, `⬜` = not yet implemented, `🚫` = not applicable, `⚠️` = deprecated

| Property | Unity Min | Status | Guard | Notes |
|----------|-----------|--------|-------|-------|
| alignContent | 6000.0 | ✅ | — | floor |
| alignItems | 6000.0 | ✅ | — | floor |
| alignSelf | 6000.0 | ✅ | — | floor |
| aspectRatio | 6000.3 | ⬜ | `UNITY_6000_3_OR_NEWER` | New in 6.3. Assembly-verified. |
| backgroundColor | 6000.0 | ✅ | — | floor |
| backgroundImage | 6000.0 | ✅ | — | floor |
| backgroundPositionX | 6000.0 | ✅ | — | floor |
| backgroundPositionY | 6000.0 | ✅ | — | floor |
| backgroundRepeat | 6000.0 | ✅ | — | floor |
| backgroundSize | 6000.0 | ✅ | — | floor |
| borderBottomColor | 6000.0 | ✅ | — | floor |
| borderBottomLeftRadius | 6000.0 | ✅ | — | floor |
| borderBottomRightRadius | 6000.0 | ✅ | — | floor |
| borderBottomWidth | 6000.0 | ✅ | — | floor |
| borderLeftColor | 6000.0 | ✅ | — | floor |
| borderLeftWidth | 6000.0 | ✅ | — | floor |
| borderRightColor | 6000.0 | ✅ | — | floor |
| borderRightWidth | 6000.0 | ✅ | — | floor |
| borderTopColor | 6000.0 | ✅ | — | floor |
| borderTopLeftRadius | 6000.0 | ✅ | — | floor |
| borderTopRightRadius | 6000.0 | ✅ | — | floor |
| borderTopWidth | 6000.0 | ✅ | — | floor |
| bottom | 6000.0 | ✅ | — | floor |
| color | 6000.0 | ✅ | — | floor |
| cursor | 6000.0 | ⬜ | — | No-op stub (Cursor struct needs Texture2D) |
| display | 6000.0 | ✅ | — | floor |
| filter | 6000.3 | ⬜ | `UNITY_6000_3_OR_NEWER` | New in 6.3. Assembly-verified. TODO stub exists. |
| flexBasis | 6000.0 | ✅ | — | floor |
| flexDirection | 6000.0 | ✅ | — | floor |
| flexGrow | 6000.0 | ✅ | — | floor |
| flexShrink | 6000.0 | ✅ | — | floor |
| flexWrap | 6000.0 | ✅ | — | floor |
| fontSize | 6000.0 | ✅ | — | floor |
| height | 6000.0 | ✅ | — | floor |
| justifyContent | 6000.0 | ✅ | — | floor |
| left | 6000.0 | ✅ | — | floor |
| letterSpacing | 6000.0 | ✅ | — | floor |
| marginBottom | 6000.0 | ✅ | — | floor |
| marginLeft | 6000.0 | ✅ | — | floor |
| marginRight | 6000.0 | ✅ | — | floor |
| marginTop | 6000.0 | ✅ | — | floor |
| maxHeight | 6000.0 | ✅ | — | floor |
| maxWidth | 6000.0 | ✅ | — | floor |
| minHeight | 6000.0 | ✅ | — | floor |
| minWidth | 6000.0 | ✅ | — | floor |
| opacity | 6000.0 | ✅ | — | floor |
| overflow | 6000.0 | ✅ | — | floor |
| paddingBottom | 6000.0 | ✅ | — | floor |
| paddingLeft | 6000.0 | ✅ | — | floor |
| paddingRight | 6000.0 | ✅ | — | floor |
| paddingTop | 6000.0 | ✅ | — | floor |
| position | 6000.0 | ✅ | — | floor |
| right | 6000.0 | ✅ | — | floor |
| rotate | 6000.0 | ✅ | — | floor |
| scale | 6000.0 | ✅ | — | floor |
| textOverflow | 6000.0 | ✅ | — | floor |
| textShadow | 6000.0 | ✅ | — | floor |
| top | 6000.0 | ✅ | — | floor |
| transformOrigin | 6000.0 | ✅ | — | floor |
| transitionDelay | 6000.0 | ⬜ | — | No-op stub (StyleList diffing issue) |
| transitionDuration | 6000.0 | ⬜ | — | No-op stub (StyleList diffing issue) |
| transitionProperty | 6000.0 | ⬜ | — | No-op stub (StyleList diffing issue) |
| transitionTimingFunction | 6000.0 | ⬜ | — | No-op stub (StyleList diffing issue) |
| translate | 6000.0 | ✅ | — | floor |
| unityBackgroundImageTintColor | 6000.0 | ✅ | — | floor |
| ~~unityEditorTextRenderingMode~~ | — | 🚫 | — | NOT in IStyle. Was listed based on docs; assembly diff confirmed absent through 6000.3. |
| unityFont | 6000.0 | ✅ | — | floor |
| unityFontDefinition | 6000.0 | ✅ | — | floor |
| unityFontStyleAndWeight | 6000.0 | ✅ | — | floor |
| unityMaterial | 6000.3 | ⬜ | `UNITY_6000_3_OR_NEWER` | New in 6.3. Assembly-verified. UI Shader Graph. |
| unityOverflowClipBox | 6000.0 | ✅ | — | floor |
| unityParagraphSpacing | 6000.0 | ✅ | — | floor |
| unitySliceBottom | 6000.0 | ✅ | — | floor |
| unitySliceLeft | 6000.0 | ✅ | — | floor |
| unitySliceRight | 6000.0 | ✅ | — | floor |
| unitySliceScale | 6000.0 | ✅ | — | floor |
| unitySliceTop | 6000.0 | ✅ | — | floor |
| unitySliceType | 6000.0 | ✅ | — | floor |
| unityTextAlign | 6000.0 | ✅ | — | floor |
| unityTextAutoSize | 6000.2 | ⬜ | — | Floor property. Assembly diff: added in 6000.2 (not 6.3 as originally documented). |
| ~~unityTextGenerator~~ | — | 🚫 | — | NOT in IStyle. Was listed based on docs; assembly diff confirmed absent through 6000.3. |
| unityTextOutlineColor | 6000.0 | ✅ | — | floor |
| unityTextOutlineWidth | 6000.0 | ✅ | — | floor |
| unityTextOverflowPosition | 6000.0 | ✅ | — | floor |
| visibility | 6000.0 | ✅ | — | floor |
| whiteSpace | 6000.0 | ✅ | — | floor |
| width | 6000.0 | ✅ | — | floor |
| wordSpacing | 6000.0 | ✅ | — | floor |

**Last audited:** 6000.4

### 3.2 Elements — Unity VisualElements

Status: `✅` = full support (Props + V.cs + Registry + Schema), `⬜` = not implemented

#### Runtime Elements (no guard needed)

| Element | Unity Min | Status | Notes |
|---------|-----------|--------|-------|
| BoundsField | 6000.0 | ✅ | |
| BoundsIntField | 6000.0 | ✅ | |
| Box | 6000.0 | ✅ | |
| Button | 6000.0 | ✅ | |
| DoubleField | 6000.0 | ✅ | |
| DropdownField | 6000.0 | ✅ | |
| EnumField | 6000.0 | ✅ | |
| FloatField | 6000.0 | ✅ | |
| Foldout | 6000.0 | ✅ | |
| GroupBox | 6000.0 | ✅ | |
| Hash128Field | 6000.0 | ✅ | |
| Image | 6000.0 | ✅ | |
| IMGUIContainer | 6000.0 | ✅ | |
| IntegerField | 6000.0 | ✅ | |
| Label | 6000.0 | ✅ | |
| ListView | 6000.0 | ✅ | |
| LongField | 6000.0 | ✅ | |
| MinMaxSlider | 6000.0 | ✅ | |
| MultiColumnListView | 6000.0 | ✅ | |
| MultiColumnTreeView | 6000.0 | ✅ | |
| ProgressBar | 6000.0 | ✅ | |
| RadioButton | 6000.0 | ✅ | |
| RadioButtonGroup | 6000.0 | ✅ | |
| RectField | 6000.0 | ✅ | |
| RectIntField | 6000.0 | ✅ | |
| RepeatButton | 6000.0 | ✅ | |
| ScrollView | 6000.0 | ✅ | |
| Scroller | 6000.0 | ✅ | |
| Slider | 6000.0 | ✅ | |
| SliderInt | 6000.0 | ✅ | |
| Tab | 6000.0 | ✅ | |
| TabView | 6000.0 | ✅ | |
| TemplateContainer | 6000.0 | ✅ | |
| TextElement | 6000.0 | ✅ | |
| TextField | 6000.0 | ✅ | |
| Toggle | 6000.0 | ✅ | |
| ToggleButtonGroup | 6000.0 | ✅ | |
| TreeView | 6000.0 | ✅ | |
| UnsignedIntegerField | 6000.0 | ✅ | |
| UnsignedLongField | 6000.0 | ✅ | |
| Vector2Field | 6000.0 | ✅ | |
| Vector2IntField | 6000.0 | ✅ | |
| Vector3Field | 6000.0 | ✅ | |
| Vector3IntField | 6000.0 | ✅ | |
| Vector4Field | 6000.0 | ✅ | |
| VisualElement | 6000.0 | ✅ | |

**Count: 46 runtime elements**

#### Editor-Only Elements (behind `UNITY_EDITOR`)

| Element | Unity Min | Status | Notes |
|---------|-----------|--------|-------|
| ColorField | 6000.0 | ✅ | |
| EnumFlagsField | 6000.0 | ✅ | |
| HelpBox | 6000.0 | ✅ | |
| InspectorElement | 6000.0 | ✅ | |
| ObjectField | 6000.0 | ✅ | |
| PropertyField | 6000.0 | ✅ | |
| Toolbar | 6000.0 | ✅ | |
| ToolbarBreadcrumbs | 6000.0 | ✅ | |
| ToolbarButton | 6000.0 | ✅ | |
| ToolbarMenu | 6000.0 | ✅ | |
| ToolbarPopupSearchField | 6000.0 | ✅ | |
| ToolbarSearchField | 6000.0 | ✅ | |
| ToolbarSpacer | 6000.0 | ✅ | |
| ToolbarToggle | 6000.0 | ✅ | |
| TwoPaneSplitView | 6000.0 | ✅ | |

**Count: 15 editor-only elements**

#### Framework Virtual Elements (not Unity types — no version guard needed)

| Element | Type | Notes |
|---------|------|-------|
| Animate | Framework | Animation wrapper |
| ErrorBoundary | Framework | Error boundary |
| Fragment | Framework | Grouping node |
| Func | Framework | Functional component |
| Host | Framework | Mount point |
| Link | Framework | Router link |
| Memo | Framework | Memoized component |
| Portal | Framework | Render-to-target |
| Route | Framework | Router route |
| Router | Framework | Router root |
| Suspense | Framework | Async loading |
| Text | Framework | Text shorthand |
| VisualElementSafe | Framework | Safe fallback |

**Count: 13 framework virtuals**

**Total: 61 Unity elements + 13 framework = 74 registered entries**

**Last audited:** 6000.4

### 3.3 Deprecation Tracker

| API | Deprecated In | Removed In | Status | Notes |
|-----|--------------|------------|--------|-------|
| (none affecting UIElements yet) | — | — | — | — |

**Last audited:** 6000.4

### 3.4 Version Audit Log

| Version | Audited Date | Audited By | IStyle Diff | Elements Diff | Breaking Changes |
|---------|-------------|------------|-------------|---------------|-----------------|
| 6000.0 | 2026-03-23 | Assembly diff | Baseline reference point | Baseline | — |
| 6000.1 | 2026-03-23 | Assembly diff | No IStyle changes vs 6000.0 | No new elements | — |
| 6000.2 | 2026-03-23 | Assembly diff | +1 IStyle (unityTextAutoSize); +6 enums (LibraryVisibility, PanelInputRedirection, Pivot, PivotReferenceSize, TextAutoSizeMode, WorldSpaceSizeMode); +5 structs | No new elements | Floor version — baseline |
| 6000.3 | 2026-03-23 | Assembly diff | +3 IStyle (aspectRatio, filter, unityMaterial); +6 enums (AddressMode, DropdownMenuSizeMode, FilterFunctionType, FilterParameterType, GradientType, TextureSlotCount); +12 structs; UsageHints +2 members | No new elements | USS parser upgrade. Note: original docs-based audit claimed +6 IStyle — assembly shows only +3 new in 6.3. unityTextGenerator and unityEditorTextRenderingMode do not exist on IStyle. |
| 6000.4 | 2026-03-23 | AI-assisted | No changes vs 6.3 | No new elements | No UI Toolkit breaking changes |

---

## 4. Implementation Checklist — Per Change Type

### 4.1 Adding a NEW IStyle Property

When a new IStyle property (e.g. `backdropFilter`) is discovered:

```
[ ] 1. PropsApplier.cs — Add setter in styleSetters dictionary
       - Wrap in #if UNITY_{VERSION}_OR_NEWER / #else no-op
       - Include type conversion logic (ConvertTo*, if/is chains)
[ ] 2. PropsApplier.cs — Add resetter in styleResetters dictionary
       - Wrap in same #if guard
       - Reset pattern: e.style.{prop} = StyleKeyword.Null
[ ] 3. Style.cs — Add typed set-only property
       - Wrap in #if (no #else — compile-time signal)
       - Pattern: public {StyleType} {Name} { set => this["{key}"] = value; }
[ ] 4. StyleKeys.cs — Add const string
       - Can be ungated (strings are harmless) or gated for consistency
[ ] 5. CssHelpers.cs — Add shortcut methods/properties (if enum or struct)
       - Wrap in #if
       - E.g. Blur(float), Grayscale(float), etc.
[ ] 6. uitkx-schema.json — Add entry to styleKeyValues (if enum-like)
       - Add "sinceUnity" annotation (when LSP version awareness is implemented)
[ ] 7. Version Coverage Matrix — Update this file
       - Add row to IStyle Properties table
       - Mark status as ✅
[ ] 8. TECH_DEBT.md — Update relevant entries (if any)
```

### 4.2 Adding a NEW Element

When a new VisualElement subclass (e.g. `CalendarPicker`) is discovered:

```
[ ] 1.  Create XyzProps.cs in Shared/Props/Typed/
        - Extend BaseProps
        - Add element-specific properties
        - Wrap entire file in #if guard
[ ] 2.  V.cs — Add factory method(s)
        - public static VirtualNode Xyz(XyzProps props, ...)
        - Wrap in #if guard
[ ] 3.  ElementRegistryProvider.cs — Add RegisterIfAllowed() call
        - Wrap in #if guard (or #if UNITY_EDITOR if editor-only)
[ ] 4.  PropsResolver.cs (SourceGenerator~) — Add fallback map entry
        - Wrap in #if guard
[ ] 5.  HmrCSharpEmitter.cs (Editor/HMR) — Add tag map entry
        - Wrap in #if guard
[ ] 6.  uitkx-schema.json — Add element entry
        - Include propsType, attributes, acceptsChildren
        - Add "sinceUnity" annotation
[ ] 7.  PropsApplier.cs — Add element-specific property setters (if any)
        - Only for props that map to non-universal APIs
[ ] 8.  Version Coverage Matrix — Update this file
        - Add row to Elements table
        - Mark status as ✅
```

### 4.3 Adding a NEW Enum Value to an Existing Property

When a new value is added to an existing enum (e.g. `Position.Sticky`):

```
[ ] 1. PropsApplier.cs — Update conversion function to handle new value
       - May need #if guard if the new enum member doesn't exist on older Unity
[ ] 2. CssHelpers.cs — Add shortcut property
       - Wrap in #if guard
[ ] 3. uitkx-schema.json — Add value to styleKeyValues array
       - Add "sinceUnity" annotation
[ ] 4. Version Coverage Matrix — Note in IStyle Properties table
```

### 4.4 Handling a DEPRECATION

When Unity deprecates an API we use:

```
[ ] 1. Deprecation Tracker — Add entry to §3.3 table
       - Record: what, deprecated in, scheduled removal version
[ ] 2. PropsApplier.cs — Keep existing setter (it still works)
       - Add comment: // Deprecated in 6.X, see {replacement}
       - If replacement exists: add new setter (behind #if)
       - Both old and new should work — no breaking change
[ ] 3. Style.cs — Keep existing typed property
       - Add [Obsolete] attribute with message, behind #if:
         #if UNITY_{DEPRECATION_VERSION}_OR_NEWER
         [System.Obsolete("Use {NewProp} instead. Deprecated in Unity {version}.")]
         #endif
         public {Type} {OldProp} { set => this["{key}"] = value; }
[ ] 4. CssHelpers.cs — Same [Obsolete] pattern
[ ] 5. uitkx-schema.json — Add "deprecated" and "replacedBy" annotations
[ ] 6. Version Coverage Matrix — Update status to ⚠️
```

### 4.5 Handling a REMOVAL

When Unity removes an API we currently use:

```
[ ] 1. PropsApplier.cs — Wrap setter in #if !UNITY_{REMOVAL_VERSION}_OR_NEWER
       - This removes it for users on the removal version
       - Keeps it for users on older versions (down to floor)
[ ] 2. Style.cs — Wrap typed property in same guard
[ ] 3. CssHelpers.cs — Wrap shortcuts in same guard
[ ] 4. StyleKeys.cs — Wrap constant in same guard
[ ] 5. uitkx-schema.json — Remove entry OR add "removedIn" annotation
[ ] 6. Version Coverage Matrix — Update status to 🚫 with note
[ ] 7. Consider bumping floor version if removal is widespread
```

---

## 5. Schema Version-Awareness — LSP Integration

### 5.1 Current State
- LSP reads `ProjectSettings/ProjectVersion.txt` via `ReferenceAssemblyLocator`
- Parses `m_EditorVersion: 6000.3.0f1` to find Unity install path
- Version string is extracted but **not exposed or used for filtering**
- Schema has no version annotations

### 5.2 Target State
- Schema entries carry optional `sinceUnity` / `deprecatedIn` / `removedIn` fields
- `ReferenceAssemblyLocator` exposes detected version as a public property
- CompletionHandler filters/deprioritizes items above user's version
- DiagnosticsPublisher warns when using features requiring newer Unity

### 5.3 Implementation Tasks

```
[ ] A. Extend schema model (SchemaLoader.cs)
       - Add optional fields to ElementInfo: sinceUnity, deprecatedIn, removedIn
       - Add optional fields to StyleKeyValues entries (requires value→object change)
       - Add optional sinceUnity to AttributeInfo
[ ] B. Expose detected Unity version (ReferenceAssemblyLocator.cs)
       - Make version string a public property
       - Parse into comparable numeric form (e.g. 6000.3 → 60003)
       - Make accessible from CompletionHandler and DiagnosticsPublisher
[ ] C. Filter completions (CompletionHandler.cs)
       - If sinceUnity > detectedVersion: add ⚠️ prefix to label
       - Sort these items lower in the list
       - Add detail text: "Requires Unity {version}+"
[ ] D. Warn in diagnostics (DiagnosticsPublisher.cs)
       - New diagnostic: UITKX_VERSION — "'{name}' requires Unity {version}+"
       - Severity: Warning (not error — the no-op fallback still works)
[ ] E. Add version annotations to uitkx-schema.json
       - All current entries: no annotation (means "since floor")
       - New entries: add sinceUnity
       - Deprecated entries: add deprecatedIn + replacedBy
```

---

## 6. Deprecation Handling

### 6.1 Unity's Deprecation Pattern

Based on research across 6.0–6.4 upgrade guides:

1. **Deprecation cycle:** Unity typically marks APIs `[Obsolete]` for 1-2 major versions before removal
2. **API Updater:** Unity has an automatic API updater that can rename methods/types during project upgrade
3. **Breaking changes page:** Always lists removals under "Upgrade to Unity X.Y"
4. **Typical timeline:** Deprecated in 6.X → Warning in 6.X → Removed in 6.X+2 or 6.X+3

### 6.2 Our Response Strategy

| Unity Action | Our Response | Timeline |
|-------------|-------------|----------|
| API deprecated (still compiles) | Add `[Obsolete]` to our wrapper, add replacement if one exists | Same release |
| API removed (won't compile) | Wrap in `#if !UNITY_{VERSION}_OR_NEWER` | Same release |
| API renamed | Support both via `#if` / `#else` | Same release |
| Type moved to different namespace | Support both via `using` alias + `#if` | Same release |
| Default behavior changed | Document only (no code change needed) | Same release |

### 6.3 Floor Version Bump Policy

We may bump the floor version when:
- A critical API is removed in a new version AND the old version is 2+ releases behind
- The `#if` gymnastics become too complex to maintain
- LTS status changes (e.g. old floor loses LTS support)

**Process for floor bump:**
1. Update `package.json` → `"unity": "6000.X"`
2. Remove all `#if UNITY_6000_X_OR_NEWER` guards (they're always true now)
3. Remove any `#if !UNITY_6000_X_OR_NEWER` blocks (dead code)
4. Update Version Coverage Matrix baselines
5. Announce in changelog / release notes

---

## 7. Automated API Diff Script

### 7.1 Purpose
A script that compares Unity UIElements API between two versions and outputs
a structured diff of additions, removals, and changes.

### 7.2 Approach
- Fetch IStyle API doc pages for version A and version B
- Parse property lists from HTML
- Diff and output: added, removed, type-changed
- Cross-reference against Version Coverage Matrix to flag gaps

### 7.3 Implementation Tasks

```
[ ] A. Create scripts/unity-api-diff.py (or .csx / .ps1)
       Input: --from 6000.2 --to 6000.3
       Output: JSON or markdown diff report
[ ] B. IStyle property scraper
       - Fetch: docs.unity3d.com/{VERSION}/Documentation/ScriptReference/UIElements.IStyle.html
       - Parse: extract all property names and types from the Properties table
       - Output: { "propertyName": "typeName", ... }
[ ] C. Diff engine
       - Compare two property maps
       - Classify: added, removed, type-changed
       - Output structured report
[ ] D. Gap analyzer
       - Load Version Coverage Matrix (parse markdown table)
       - Cross-reference with diff output
       - Flag: "property X was added in 6000.3 but is not in our matrix"
[ ] E. Element scraper (stretch goal)
       - Fetch namespace page for UIElements classes
       - Extract class names, check which extend VisualElement
       - Diff between versions
```

### 7.4 Output Format

```json
{
  "from": "6000.2",
  "to": "6000.3",
  "istyle": {
    "added": [
      { "name": "aspectRatio", "type": "StyleRatio" },
      { "name": "filter", "type": "StyleList<FilterFunction>" }
    ],
    "removed": [],
    "changed": []
  },
  "elements": {
    "added": [],
    "removed": []
  },
  "gaps": [
    { "property": "aspectRatio", "addedIn": "6000.3", "inMatrix": false }
  ]
}
```

---

## 8. Codebase Surface Inventory

### 8.1 Per New IStyle Property — 6 files to touch

| # | File | Section | What to add |
|---|------|---------|-------------|
| 1 | `Shared/Props/PropsApplier.cs` | styleSetters (line ~41) | Setter with type conversion |
| 2 | `Shared/Props/PropsApplier.cs` | styleResetters (line ~558) | Resetter (= StyleKeyword.Null) |
| 3 | `Shared/Props/Typed/Style.cs` | Properties (line ~36) | Typed set-only property |
| 4 | `Shared/Props/Typed/StyleKeys.cs` | Constants (line ~5) | public const string |
| 5 | `Shared/Props/Typed/CssHelpers.cs` | Shortcuts (line ~16) | Static methods/properties |
| 6 | `ide-extensions~/grammar/uitkx-schema.json` | styleKeyValues (line ~1685) | Enum values array |

### 8.2 Per New Element — 9+ files to touch

| # | File | What to add |
|---|------|-------------|
| 1 | `Shared/Props/Typed/XyzProps.cs` | **New file** — props class |
| 2 | `Shared/Core/V.cs` | Factory method(s) |
| 3 | `Shared/Elements/ElementRegistryProvider.cs` | RegisterIfAllowed() |
| 4 | `Shared/Props/PropsApplier.cs` | Element-specific property setters (if any) |
| 5 | `SourceGenerator~/Emitter/PropsResolver.cs` | Fallback map entry |
| 6 | `Editor/HMR/HmrCSharpEmitter.cs` | Tag map entry |
| 7 | `ide-extensions~/grammar/uitkx-schema.json` | Element definition |
| 8 | `Version Coverage Matrix` | Elements table row |
| 9 | (optional) `Shared/Props/Typed/BaseProps.cs` | Only if new universal attributes |

### 8.3 Current Counts (as of 2026-03-23)

| Surface | Count |
|---------|-------|
| styleSetters entries | 80 |
| styleResetters entries | 55 |
| No-op style stubs | 8 keys |
| Style.cs typed properties | 71 |
| StyleKeys.cs constants | 71 |
| CssHelpers shortcuts | 67 |
| Schema elements | 33 |
| Schema styleKeyValues groups | 24+ |
| Schema universalAttributes | 41 |
| ElementRegistry entries | 61 (46 runtime + 15 editor) |
| PropsResolver fallback entries | 31 |
| HmrCSharpEmitter tag entries | 31 |
| V.cs factory methods | 66 |
| *Props.cs files | 57 |
| Framework virtual elements | 13 |

---

## 9. Rollout Tasks — Infrastructure to Build

These are the scaffolding tasks to establish the versioning system.
They do NOT include implementing support for any specific Unity version.

### Phase 1: Documentation & Tracking (this file)

```
[x] 1. Create this plan document (Plans~/VERSIONING_PROCESS.md)
[x] 2. Populate IStyle Properties matrix from current IStyle 6.2 reference
[x] 3. Populate Elements matrix from current ElementRegistryProvider
[x] 4. Record 6.3 and 6.4 audit results in Version Audit Log
[ ] 5. Update TECH_DEBT.md to reference this plan
```

### Phase 2: Schema Version Annotations

```
[ ] 6.  Extend SchemaLoader.cs model types — add sinceUnity, deprecatedIn, removedIn
[ ] 7.  Add sinceUnity annotations to uitkx-schema.json for 6.3 additions
[ ] 8.  Expose Unity version from ReferenceAssemblyLocator.cs
[ ] 9.  Wire version into CompletionHandler for deprioritization
[ ] 10. Wire version into DiagnosticsPublisher for warnings
[ ] 11. Add LSP tests for version-aware completions
```

### Phase 3: Automated Diff Script

```
[ ] 12. Create scripts/unity-api-diff/ tool
[ ] 13. Implement IStyle property scraper
[ ] 14. Implement diff engine
[ ] 15. Implement gap analyzer (reads this matrix)
[ ] 16. Test against known 6.2→6.3 diff (should find the 6 new properties)
```

### Phase 4: First Version Implementation (6.3 support)

```
[ ] 17. Implement #if guards for all 6 new IStyle properties
[ ] 18. Add CssHelpers for FilterFunction types
[ ] 19. Update uitkx-schema.json with new styleKeyValues
[ ] 20. Update Version Coverage Matrix status ⬜ → ✅
[ ] 21. Test on Unity 6.2 (no-ops, no errors)
[ ] 22. Test on Unity 6.3 (properties work)
```

---

## Appendix A: Unity Version to Define Symbol Mapping

| Unity Version | Marketing Name | Define Symbol |
|---------------|---------------|---------------|
| 6000.0 | Unity 6.0 | `UNITY_6000_0_OR_NEWER` |
| 6000.1 | Unity 6.1 | `UNITY_6000_1_OR_NEWER` |
| 6000.2 | Unity 6.2 | `UNITY_6000_2_OR_NEWER` |
| 6000.3 | Unity 6.3 LTS | `UNITY_6000_3_OR_NEWER` |
| 6000.4 | Unity 6.4 | `UNITY_6000_4_OR_NEWER` |

These symbols are **auto-defined by Unity** — no asmdef `versionDefines` needed.
On Unity 6.3, all of `UNITY_6000_0_OR_NEWER` through `UNITY_6000_3_OR_NEWER` are defined.

## Appendix B: Documentation URL Templates

```
What's New:      https://docs.unity3d.com/{V}/Documentation/Manual/WhatsNewUnity{MajMin}.html
Upgrade Guide:   https://docs.unity3d.com/{V}/Documentation/Manual/UpgradeGuideUnity{MajMin}.html
IStyle API:      https://docs.unity3d.com/{V}/Documentation/ScriptReference/UIElements.IStyle.html
USS Properties:  https://docs.unity3d.com/{V}/Documentation/Manual/UIE-USS-SupportedProperties.html
Release Notes:   https://unity.com/releases/editor/whats-new/{V}
UIElements Ref:  https://docs.unity3d.com/{V}/Documentation/ScriptReference/UIElements.{TypeName}.html

Where {V} = 6000.3, {MajMin} = 63
```
