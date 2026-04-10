# CssHelpers Full Coverage — Eliminate `@using UnityEngine.UIElements`

**Goal:** Make `@using UnityEngine.UIElements` never necessary in `.uitkx` files by:
1. Covering all UIElements enum values used in typed props with CssHelpers shortcuts
2. Auto-importing `using static CssHelpers` in the source generator
3. Adding LSP completions for enum-typed attributes
4. Updating samples to use shortcuts instead of qualified enum names

---

## Problem

When a user adds `@using UnityEngine.UIElements` to a `.uitkx` file, UIElements enum
members (e.g. `FlexDirection.Column`) enter scope and clash with CssHelpers shortcuts
(e.g. `CssHelpers.Column`), causing ambiguity errors. The only safe solution is to ensure
users never need that import.

---

## Current State

### Already implemented (this session)
- [x] PickingMode: `PickPosition`, `PickIgnore`
- [x] SelectionType: `SelectNone`, `SelectSingle`, `SelectMultiple`
- [x] ScrollerVisibility: `ScrollerAuto`, `ScrollerVisible`, `ScrollerHidden`
- [x] LanguageDirection: `DirInherit`, `DirLTR`, `DirRTL`

### Not yet covered
- [x] SliderDirection (string-typed prop: `Direction`)
- [x] ScrollViewMode (string-typed prop: `Mode`)
- [x] ScaleMode (string-typed prop in ImageProps)
- [x] TwoPaneSplitViewOrientation (Editor-only, string-typed prop)
- [x] ColumnSortingMode (reflection-based `object`-typed prop — adapter accepts strings via `Enum.Parse`)

### Auto-imports
- `using static StyleKeys` — already auto-imported by source generator
- `using static CssHelpers` — auto-imported by source generator + HMR emitter

---

## Implementation Phases

### Phase 1 — Complete CssHelpers enum shortcuts

**File:** `Shared/Props/Typed/CssHelpers.cs`

Add shortcuts for the remaining enums. Note: SliderDirection, ScrollViewMode, and
TwoPaneSplitViewOrientation are **string-typed** in the Props classes — the element
adapters convert strings to enums internally. Users already write `direction="vertical"`.
CssHelpers shortcuts for these would be string constants, not enum values.

```csharp
// ── Slider direction (string-based) ─────────────────────────────
public static string SliderHorizontal => "horizontal";
public static string SliderVertical   => "vertical";

// ── ScrollView mode (string-based) ─────────────────────────────
public static string ScrollVertical   => "vertical";
public static string ScrollHorizontal => "horizontal";

// ── Image scale mode (string-based) ────────────────────────────
public static string ScaleStretch => "stretchfill";
public static string ScaleFit     => "scaletofit";
public static string ScaleCrop    => "scalefill";

// ── TwoPaneSplitView orientation (string-based, Editor-only) ───
public static string OrientHorizontal => "horizontal";
public static string OrientVertical   => "vertical";

// ── Column sorting mode (string-based, parsed via Enum.Parse) ──
public static string SortNone    => "None";
public static string SortDefault => "Default";
public static string SortCustom  => "Custom";
```

**Zero exceptions policy:** Every UIElements enum used in any prop or element adapter
has a CssHelpers shortcut. Users never need `@using UnityEngine.UIElements`.

**String-based props** (SliderDirection, ScrollViewMode, TwoPaneSplitViewOrientation,
ScaleMode) accept string values that the element adapters convert internally.
CssHelpers provides named constants for discoverability and LSP completions.

**ColumnSortingMode** is `object`-typed and handled via `Enum.Parse(enumType, s, true)`
in the adapters. String values `"None"`, `"Default"`, `"Custom"` are parsed
case-insensitively. CssHelpers provides string constants so users write
`sortingMode={SortCustom}` instead of `sortingMode={ColumnSortingMode.Custom}`.

**Checklist:**
```
[x] Add string-based shortcuts for SliderDirection, ScrollViewMode, ScaleMode
[x] Add string-based shortcuts for TwoPaneSplitViewOrientation
[x] Add string-based shortcuts for ColumnSortingMode
[x] Verify no naming conflicts with existing CssHelpers/StyleKeys members
[x] Run tests (832 SG tests)
```

---

### Phase 2 — Auto-import `using static CssHelpers` in source generator

**File:** `SourceGenerator~/Emitter/CSharpEmitter.cs` (around line 129)

Add `using static ReactiveUITK.Props.Typed.CssHelpers;` to the auto-injected
using block, alongside the existing `using static StyleKeys;`.

**Impact:**
- Users no longer need `@using static ReactiveUITK.Props.Typed.CssHelpers` in
  their `.uitkx` files — it's always available
- Existing files that explicitly add the `@using static` are unaffected (duplicate
  `using static` is a no-op in C#)
- No conflict with StyleKeys (StyleKeys has `string` constants for property names;
  CssHelpers has typed values — zero overlap)
- No conflict with the old tuple syntax `(Width, Column)` — StyleKeys provides
  the key, CssHelpers provides the value

**Checklist:**
```
[x] Add auto-import line in CSharpEmitter.cs
[x] Run tests (832 SG tests — snapshot tests will need updating if the auto-import
    line appears in generated output)
[x] Verify HMR compilation still works (HmrCSharpEmitter may need the same change)
```

---

### Phase 3 — LSP enum attribute value completions

**File:** `ide-extensions~/lsp-server/CompletionHandler.cs`

Currently `AttributeValueItems()` returns completions for `bool`, `int`, `float`,
`action`, `object`, `style` types but returns **empty** for enum-typed attributes.

**Approach:** Add a mapping of attribute names to their CssHelpers shortcut names.
When the cursor is inside an attribute value for a known enum prop, offer the
shortcuts as completion items.

**Mapping (attribute name → CssHelpers shortcuts):**

| Attribute | Shortcuts |
|-----------|-----------|
| `pickingMode` | `PickPosition`, `PickIgnore` |
| `selection` | `SelectNone`, `SelectSingle`, `SelectMultiple` |
| `verticalScrollerVisibility` | `ScrollerAuto`, `ScrollerVisible`, `ScrollerHidden` |
| `horizontalScrollerVisibility` | `ScrollerAuto`, `ScrollerVisible`, `ScrollerHidden` |
| `languageDirection` | `DirInherit`, `DirLTR`, `DirRTL` |
| `direction` (Slider) | `SliderHorizontal`, `SliderVertical` |
| `mode` (ScrollView) | `ScrollVertical`, `ScrollHorizontal` |
| `scaleMode` (Image) | `ScaleStretch`, `ScaleFit`, `ScaleCrop` |
| `orientation` (TwoPaneSplitView) | `OrientHorizontal`, `OrientVertical` |
| `sortingMode` (MultiColumn*) | `SortNone`, `SortDefault`, `SortCustom` |

**Implementation:** In `AttributeValueItems()`, add an `else if` branch that checks
if the attribute type maps to known enum shortcuts. Return `CompletionItem` entries
with `Kind = Value` and the shortcut name as the insert text (wrapped in `{` `}`
if needed by the cursor context).

**Checklist:**
```
[x] Add enum-to-shortcuts mapping dictionary in CompletionHandler
[x] Add branch in AttributeValueItems() for enum completion
[x] Test with manual LSP interaction (VS Code)
[x] Add LSP tests for enum completions
```

---

### Phase 4 — Update samples and add tech debt note

**Files:** `Samples/UITKX/Shared/*.uitkx` (4+ files)

Replace `SelectionType.None` with `SelectNone`, `ColumnSortingMode.Custom` with
`SortCustom`, and remove `@using UnityEngine.UIElements` from **all** samples.

**Also:**
- Remove redundant `@using static ReactiveUITK.Props.Typed.StyleKeys` from all
  sample `.uitkx` files (it's auto-imported)
- Remove redundant `@using static ReactiveUITK.Props.Typed.CssHelpers` after
  Phase 2 makes it auto-imported

**Checklist:**
```
[x] Update TreeViewStatefulDemoFunc.uitkx
[x] Update ListViewStatefulDemoFunc.uitkx
[x] Update MultiColumnListViewStatefulDemoFunc.uitkx
[x] Update MultiColumnTreeViewStatefulDemoFunc.uitkx
[x] Scan for other .uitkx files with @using UnityEngine.UIElements — 7 removed; 6 files retain it (they reference UIElements types like VisualElement/TextField in C# code)
[x] Run tests
```

---

### Phase 5 — Documentation (full inventory)

Update **every** doc that mentions CssHelpers, `@using`, StyleKeys, or enum values.

#### 🔴 High Priority — Styling Docs

| File | Changes |
|------|---------|
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/StylingPage.tsx` | Remove manual `@using static CssHelpers` from Setup section (lines 124–135). Update "CssHelpers reference" section (lines 260–420) with new enum shortcuts. |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/StylingPage.example.ts` | Update `EXAMPLE_IMPORT` (line 33–36) to remove `@using static CssHelpers`. Update `EXAMPLE_CSS_HELPERS` (line 21–32) to show auto-import behavior. |

#### 🟡 Medium Priority — Reference, API, Getting Started, FAQ

| File | Changes |
|------|---------|
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Reference/UitkxReferencePage.tsx` | Add note in `@using` directive table that `using static StyleKeys` and `using static CssHelpers` are auto-imported. |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/API/UitkxAPIPage.tsx` | Add CssHelpers mention alongside StyleKeys documentation. |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/GettingStarted/UitkxGettingStartedPage.tsx` | Mention auto-imported directives in setup section. |
| `ReactiveUIToolKitDocs~/src/pages/FAQ/FAQPage.tsx` | Update troubleshooting section: clarify which `@using` are automatic. Add warning about `@using UnityEngine.UIElements` causing ambiguity. |
| `README.md` (root) | Update "Typed Style System" code example (line 113) — remove manual import or note it's optional. |

#### 🟢 Lower Priority — IDE & Changelog

| File | Changes |
|------|---------|
| `ide-extensions~/vscode/CHANGELOG.md` | Add entry for enum value completions (Phase 3). |
| `CHANGELOG.md` | Add entry for CssHelpers auto-import and new shortcuts. |
| `Plans~/TECH_DEBT.md` | Mark "CssHelpers static imports ambiguous" item as resolved. |

**Checklist:**
```
[x] StylingPage.tsx — remove manual import from Setup, update CssHelpers reference
[x] StylingPage.example.ts — update import examples
[x] UitkxReferencePage.tsx — add auto-import note to @using directive table
[x] UitkxAPIPage.tsx — add CssHelpers alongside StyleKeys
[x] UitkxGettingStartedPage.tsx — mention auto-imported directives
[x] FAQPage.tsx — add ambiguity warning, clarify auto-imports
[x] README.md — update Typed Style System code example
[x] ide-extensions~/vscode/CHANGELOG.md — add entry
[x] CHANGELOG.md — add entry
[x] Plans~/TECH_DEBT.md — mark resolved
```

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Auto-importing CssHelpers breaks existing `.uitkx` files | Very Low | CssHelpers names don't overlap with StyleKeys or any auto-imported type. Duplicate `using static` is a no-op. |
| Snapshot tests fail after adding auto-import | High | Expected — generated output changes. Update snapshots. |
| New CssHelpers names clash with user-defined variables | Low | Prefixed names (`PickIgnore`, `SelectNone`, `DirLTR`) are distinctive enough. Users can always qualify: `CssHelpers.PickIgnore`. |
| ColumnSortingMode string values differ across Unity versions | Very Low | `Enum.Parse` is case-insensitive. Known values (`None`, `Default`, `Custom`) are stable since Unity 2022.2. |
| HMR doesn't pick up auto-import change | Medium | Check `Editor/HMR/UitkxHmrCompiler.cs` for its own using-statement injection. |

---

## Deliverables

| Phase | Scope | Files Changed |
|-------|-------|---------------|
| 1 | CssHelpers string shortcuts (all enums, zero exceptions) | `Shared/Props/Typed/CssHelpers.cs` |
| 2 | Auto-import CssHelpers | `SourceGenerator~/Emitter/CSharpEmitter.cs`, possibly `Editor/HMR/UitkxHmrCSharpEmitter.cs` |
| 3 | LSP enum completions | `ide-extensions~/lsp-server/CompletionHandler.cs` |
| 4 | Sample updates (remove ALL `@using UnityEngine.UIElements`) | `Samples/UITKX/Shared/*.uitkx` (4+ files) |
| 5 | Full documentation update | `StylingPage.tsx`, `StylingPage.example.ts`, `UitkxReferencePage.tsx`, `UitkxAPIPage.tsx`, `UitkxGettingStartedPage.tsx`, `FAQPage.tsx`, `README.md`, changelogs, `TECH_DEBT.md` |
