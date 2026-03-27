# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

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
