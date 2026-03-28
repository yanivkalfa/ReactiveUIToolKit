# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

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
