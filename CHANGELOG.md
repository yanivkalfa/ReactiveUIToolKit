# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

## [0.3.0] - 2026-04-05

### Breaking
- **Control block bodies require `return (...)`** — all `@if`, `@for`, `@foreach`, `@while`, and `@switch` `@case`/`@default` bodies must now wrap their markup in `return (...)`. This enables C# setup code before the return statement (var declarations, lambdas, local computation). Existing control blocks with bare markup must be migrated.
- **CssHelpers renamed all shortcuts** — every member now has a consistent prefix for autocomplete discoverability (e.g. `Row` → `FlexRow`, `Column` → `FlexColumn`, `JustifyCenter` → `JustifyCenter`, `SpaceBetween` → `JustifySpaceBetween`, `AlignCenter` → `AlignCenter`, `Stretch` → `AlignStretch`, `Auto` → `StyleAuto`, `None` → `StyleNone`, `Initial` → `StyleInitial`, `WrapOn` → `WrapOn`, `NoWrap` → `WrapOff`, `WrapRev` → `WrapReverse`, `Relative` → `PosRelative`, `Absolute` → `PosAbsolute`, `Flex` → `DisplayFlex`, `DisplayNone` → `DisplayNone`, `Visible` → `VisVisible`, `Hidden` → `VisHidden`, `OverflowVisible` → `OverflowVisible`, `OverflowHidden` → `OverflowHidden`, `Normal` → `WsNormal`, `Nowrap` → `WsNowrap`, `Clip` → `TextClip`, `Ellipsis` → `TextEllipsis`, `Bold` → `FontBold`, `Italic` → `FontItalic`, `BoldItalic` → `FontBoldItalic`, `FontNormal` → `FontNormal`, `White`/`Black`/etc. → `ColorWhite`/`ColorBlack`/etc., `Transparent` → `ColorTransparent`, `OverflowStart` → `TextOverflowStart`, `OverflowMiddle` → `TextOverflowMiddle`, `OverflowEnd` → `TextOverflowEnd`)

### Added
- **Control block setup code** — `@if`, `@for`, `@foreach`, `@while`, `@switch` bodies can now contain C# statements (variable declarations, method calls, lambda captures) before `return (...)`, mirroring the component-level setup code pattern
- **Switch fallthrough** — adjacent `@case` labels with no body share the same branch (emits stacked `case X: case Y:` in statement mode, `X or Y =>` in expression mode)
- **UITKX0024 diagnostic** — parser emits an error when a control block body is missing `return (...);`
- **Compound struct factories** — `CssHelpers` now provides factory methods and presets for all compound struct style types:
  - Background: `BgRepeat(x, y)`, `BgRepeatNone`, `BgRepeatBoth`, `BgRepeatX`, `BgRepeatY`, `BgRepeatSpace`, `BgRepeatRound`; `BgPos(keyword)`, `BgPos(keyword, offset)`, `BgPosCenter`, `BgPosTop`, `BgPosBottom`, `BgPosLeft`, `BgPosRight`; `BgSize(x, y)`, `BgSizeCover`, `BgSizeContain`
  - Transforms: `Origin(x, y)`, `OriginCenter`, `Xlate(x, y)`
  - Easing: `Easing(mode)`, `EaseDefault`, `EaseLinear`, `EaseIn`, `EaseOut`, `EaseInOut`, + sine/cubic/circ/elastic/back/bounce variants (24 presets total)
- **TextAutoSizeMode** — full support for `unityTextAutoSize` across every layer: `StyleKeys`, `Style`, `CssHelpers` (`AutoSizeNone`, `AutoSizeBestFit`), `PropsApplier` (typed + string), schema, LSP completions
- **PropsApplier string parsing** — compound style properties (`backgroundRepeat`, `backgroundPositionX/Y`, `backgroundSize`, `transitionTimingFunction`) now accept CSS string values in the untyped API
- **LSP style value completions** — `backgroundRepeat`, `backgroundPositionX/Y`, `backgroundSize`, `transitionTimingFunction` now auto-complete CSS keyword values in `.uitkx` files
- **`JustifySpaceEvenly`** — added missing `Justify.SpaceEvenly` shortcut
- **`WhiteSpace.Pre`/`PreWrap`** — added `WsPre` and `WsPreWrap` shortcuts

## [0.2.45] - 2026-03-29

### Added
- **CssHelpers auto-import** — `using static CssHelpers` is now auto-injected by the source generator and HMR emitter, no `@using` directive needed in `.uitkx` files
- **CssHelpers enum shortcuts** — full zero-exception coverage of all UIElements enums used in typed props: `PickPosition`/`PickIgnore` (PickingMode), `SelectNone`/`SelectSingle`/`SelectMultiple` (SelectionType), `ScrollerAuto`/`ScrollerVisible`/`ScrollerHidden` (ScrollerVisibility), `DirInherit`/`DirLTR`/`DirRTL` (LanguageDirection), `SliderHorizontal`/`SliderVertical`, `ScrollVertical`/`ScrollHorizontal`/`ScrollBoth`, `ScaleStretch`/`ScaleFit`/`ScaleCrop`, `OrientHorizontal`/`OrientVertical`, `SortNone`/`SortDefault`/`SortCustom`
- **LSP enum value completions** — attribute value completions now suggest CssHelpers shortcuts for enum-typed and string-enum props

### Fixed
- **ScrollView adapter** — `VerticalAndHorizontal` mode now accepted via string `"verticalandhorizontal"` or `"both"`
- **TwoPaneSplitView adapter** — orientation string comparison is now case-insensitive

### Improved
- **Plan status audit** — updated USS_LOADING_PLAN (15% → 95% complete), ASSET_REGISTRY_PLAN (D2 status), and V1 Road Map (checked 6 items previously marked incomplete that are covered by existing docs site pages)
- **Sample cleanup** — removed redundant `@using static StyleKeys` (17 files), `@using static CssHelpers` (1 file), and `@using UnityEngine.UIElements` (4 files) from sample `.uitkx` files; replaced `SelectionType.None`/`ColumnSortingMode.Custom` with CssHelpers shortcuts

## [0.2.44] - 2026-03-29

### Fixed
- **Formatter empty-element regression** — `<Box></Box>` no longer expands to multi-line; empty elements with explicit close tags stay on one line
- **LSP attribute version filtering** — completion items for attributes requiring a newer Unity version now show ⚠️ warning and sort lower; removed attributes are hidden entirely
- **LSP attribute version diagnostics** — UITKX0200 warnings for attributes with `sinceUnity` or `removedIn` mismatches against the detected Unity version

### Improved
- **Docs Unity links** — component reference pages now show an inline "Unity docs" link next to the title, pointing to the versioned Unity manual page
- **Documentation updates** — updated architecture docs reflecting completed Roslyn integration; updated versioning process docs; documented `apply-diff-to-schema.mjs` automation script

## [0.2.43] - 2026-03-29

### Fixed
- **Formatter preserves empty elements** — `<Box></Box>` no longer collapsed to `<Box />` by the formatter; explicit close tags are preserved
- **Tag completion** — autocomplete no longer inserts closing tag for elements accepting children; inserts tag name + trailing space instead

## [0.2.42] - 2026-03-28

### Added
- **Find All References** (Shift+F12) — resolves symbol via `SymbolFinder.FindReferencesAsync()` across all per-file workspaces; results mapped back to `.uitkx` via SourceMap
- **JSX-style fallback** — improved fallback for JSX-style syntax in completions

### Fixed
- **VS2022 native LSP routing** — removed 3 custom GoToDefinition handlers; VS2022 now routes through `CodeRemoteContentTypeName`

## [0.2.41] - 2026-03-28

### Improved
- **HMR background reload** — HMR now sets `Application.runInBackground = true` while active, so file-save hot-reloads trigger immediately even when VS Code (or another editor) has focus. Original setting restored on stop.

## [0.2.40] - 2026-03-28

### Added
- **Transition style support** — `transitionDelay`, `transitionDuration`, `transitionProperty`, and `transitionTimingFunction` setters/resetters in PropsApplier, typed properties in Style, and StyleKeys constants

### Fixed
- **Tag completion** — autocomplete no longer inserts a closing tag snippet when editing an existing tag name (e.g., replacing `VisualElement` with `Box` inside `<VisualElement style={...}>`)

## [0.2.39] - 2026-03-28

### Fixed
- **HMR CS0433** — companion file discovery now filters by component prefix, preventing duplicate type errors when multiple `.uitkx` files share a directory
- **HMR memory leak** — controller and compiler reused across start/stop cycles, eliminating ~200MB Roslyn re-init per cycle
- **HMR per-cycle growth** — eliminated `ms.ToArray()` byte[] copy (direct `ms.CopyTo(fs)`), cached USS dependency map across cycles, switched to normal `AssetDatabase.Refresh()`

### Added
- **HMR memory tracking** — HMR window shows live RAM (working set via Win32 P/Invoke), delta since window open, and delta since session start; refreshes every 2 seconds

### Improved
- **HMR compilation** — incremental Roslyn compilation cache, cross-reference MetadataReference cache, `Assembly.LoadFrom()` instead of `Assembly.Load(byte[])`, `GC.Collect(2)` after each compilation
- **HMR window** — Repaint only on state change (swap count, error count, active toggle) instead of every frame

## [0.2.38] - 2026-03-28

### Improved
- **Documentation site** — Asset\<T\> page: replaced plain-text sections (Texture Import, Diagnostics, Supported Types, Registry) with rich MUI tables, colored Chips, and Alerts
- **Documentation site** — Diagnostics page: all diagnostic codes and severities rendered as colored MUI Chips (red=Error, orange=Warning, blue=Hint)
- **Documentation site** — Fixed Image (`texture=`) and HelpBox (`messageType=`) prop names in component examples
- **Documentation site** — Component props displayed as table with collapsible BaseProps accordion
- **Documentation site** — Added Asset\<T\> docs page with 8 sections (basic usage, relative paths, shorthand, inline, @uss, auto-import, diagnostics, supported types, registry)
- **Documentation site** — Added @uss section to Styling guide (basic usage, .uss file, multiple sheets, combining USS+Style, HMR info)

## [0.2.37] - 2026-03-28

### Added
- **@uss directive** — attach USS stylesheets to components via `@uss "./path.uss"`, parsed at compile time with `__uitkx_ussKeys` static array
- **@uss SG diagnostics** — UITKX0022 (file not found) and UITKX0023 (type mismatch) validate @uss paths at compile time
- **@uss HMR** — `.uss` file changes trigger hot-reload of dependent `.uitkx` components; USS→UITKX dependency tracking
- **@uss formatter** — `@uss` directives preserved on save (formatter preamble emission)
- **@uss syntax highlighting** — `@uss` keyword colored as directive, path colored as string

## [0.2.36] - 2026-03-28

### Added
- **Asset Registry** — `UitkxAssetRegistry` ScriptableObject with `Asset<T>()`/`Ast<T>()` helpers for loading assets from `.uitkx` files
- **Editor asset sync** — `UitkxAssetRegistrySync` auto-populates the registry on `.uitkx` save and domain reload
- **HMR asset injection** — Hot reload injects asset cache entries; on-demand `ImportAsset` for files copied during HMR
- **Type-aware auto-import** — `Asset<Sprite>("./img.png")` auto-configures `TextureImporter` to Sprite mode; `Asset<Texture2D>()` ensures Default import
- **UITKX0022** (Source Generator) — Error when `Asset<T>()`/`Ast<T>()` references a file that doesn't exist on disk
- **UITKX0023** (Source Generator) — Error when `Asset<T>()` type parameter is incompatible with file extension (e.g. `Asset<AudioClip>("./bg.png")`)

### Changed
- `Style.TextColor` renamed to `Style.Color` to match `StyleKeys` and Unity `IStyle` naming
- Classic directive mode removed — function-style only

## [0.2.35] - 2026-03-27

### Added
- Centralized changelog system for IDE extensions (`ide-extensions~/changelog.json`)

### Removed
- Classic mode code paths (~835 lines across 15+ files)
