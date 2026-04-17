# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

## [0.4.7] - 2026-04-17

### Added
- **Children slot re-render detection** — components receiving `@(__children)` now correctly re-render when their children change, using reference-equality comparison on the children list

### Fixed
- **Directive body scoping** — `@if`, `@foreach`, `@for`, `@while`, and `@switch` bodies now emit as C# local functions, preventing variable scoping leaks and early-return issues between branches
- **UITKX0009 coverage** — "loop element missing key" diagnostic now fires for `@for` and `@while` loops, not just `@foreach`
- **Setup code JSX validation** — source generator validates JSX placement inside directive body setup code
- **Hook alias runtime wrappers** — source generator emits correct wrapper methods for hook aliases
- **Source map accuracy** — improved diagnostic line mapping for UITKX0014, UITKX0013, and CS0219
- **HMR directive body support** — HMR emitter updated to match source generator's directive-body-as-function approach, including JSX splicing inside directive bodies

## [0.4.6] - 2026-04-13

### Added
- **Procedurally generated Mario levels** — `LevelGenerator` produces 35-screen levels with 6 screen types (Flat, Pit, Pipes, Staircase, Floating, Final/Flagpole). Difficulty scales with progression. Smart block cluster placement avoids pipe/ground overlap and guarantees mushrooms in question blocks.
- **Camera scrolling** — one-way horizontal camera follows Mario, clamped at level edges. Player cannot walk left past the camera (classic Mario behavior). Frustum culling skips rendering and collision for off-screen tiles.
- **Pipe tiles** — 2-wide green solid pipe obstacles with varying heights (2–4 tiles)
- **Flagpole win condition** — final screen has a staircase, flagpole, and castle. Touching the flagpole triggers "YOU WIN!" overlay with final score.
- **Damage shield** — Big Mario hit by enemy shrinks instead of dying, with 3-second invincibility grace period. Mario blinks (opacity toggle) during invincibility.
- **Coin blocks** — multi-hit blocks that give 50 points per hit (up to 5 hits)
- **Block bump animation** — blocks nudge upward briefly when hit from below
- **Mushroom power-up** — collecting a mushroom makes Mario grow (96px tall, 48px wide) for 10 seconds
- **Ducking slide** — ducking on the ground applies friction-based deceleration instead of instant stop, creating a slide effect
- **Multi-row block clusters** — 30% of generated block clusters have a second row 3 tiles above the first

### Fixed
- **HMR hook trampoline + using-static injection** — companion `.hooks.uitkx` files created during HMR sessions now emit public trampoline methods and inject `using static` into the component source
- **Brick destruction** — bricks now break when hit from below and disappear from the level
- **Mushroom physics** — mushrooms slide horizontally, fall with gravity, and bounce off walls
- **Jump height** — increased `JUMP_VEL` from -500 to -620 so Mario can clear gaps and reach blocks
- **Ducking mid-air** — ducking now works in the air (not grounded-only) and correctly reduces collision box
- **Duck position snapping** — transitioning between duck/stand adjusts Y position to keep feet in place, preventing underground clipping and forward teleporting
- **Side-hit brick breaking removed** — bricks only break from head-hits underneath, not side collisions. Center-of-head check prevents angled corner-clip breaks.
- **Mushroom Big flag ordering** — Big/BigTimer now applied after Items loop so mushroom collection actually persists to player state
- **Game start grounding** — player initial Y slightly overlaps ground so `grounded=true` on first frame (enables jumping immediately)
- **Restart keyboard focus** — clicking "Try Again" re-focuses the game board so keyboard input works immediately

## [0.4.5] - 2026-04-12

### Fixed
- **HMR hook companion trampoline** — companion `.hooks.uitkx` files discovered during HMR now emit public trampoline methods (e.g. `useXxx()`) in addition to the private body, and inject `using static Ns.XxxHooks;` into the component source. Previously only the private `__useXxx_body` was emitted, causing `CS0103` when a hook file was created during an HMR session.

## [0.4.4] - 2026-04-12

### Fixed
- **HMR companion `.uitkx` discovery** — HMR now discovers and compiles companion `.uitkx` files (`.style.uitkx`, `.hooks.uitkx`, `.utils.uitkx`) alongside the parent component, so module/hook members are available in the compilation unit. Previously only companion `.cs` files were included, causing `CS0103` errors for module-defined symbols like style constants.
- **HMR companion change redirection** — saving a companion `.uitkx` file now triggers recompilation of the parent component file, ensuring changes to styles/hooks/utils are immediately hot-reloaded.

## [0.4.3] - 2026-04-12

### Fixed
- **`onInput` event handler dispatch** — `onInput` handlers with `Action<string>` signature now correctly receive the field's text (`InputEvent.newData`) instead of `null`. Added `Action<InputEvent>` fast-path dispatch to avoid `DynamicInvoke` fallback.

### Added
- **Editor demo windows** — added `ReactiveUITK/Demos/Stress Test`, `Snake Game`, and `Tic Tac Toe` menu items for launching sample games in editor windows
- **Stress Test sample** — moved stress test to its own `Samples/Components/StressTest/` folder with configurable box count via UI input

## [0.4.0] - 2026-04-10

### Added
- **Hook companion files** (`.hooks.uitkx`) — extract reusable hooks into dedicated companion files using the `hook` keyword with `-> ReturnType` syntax. Hooks are parsed, validated, and code-generated alongside the parent component.
- **Module companion files** (`.style.uitkx`, `.utils.uitkx`) — extract styles, constants, and utilities into companion files using the `module` keyword. Generates partial class members on the parent component.
- **`@namespace` directive** — components, hooks, and modules declare their namespace via `@namespace` instead of requiring a companion `.cs` partial class.
- **Cross-file peer resolution** — LSP server and source generator resolve hooks and modules from sibling `.uitkx` files, providing full IntelliSense, diagnostics, and navigation across companion files.

### Fixed
- **Cross-file diagnostic staleness** — peer `.uitkx` content now read from editor buffers (not disk) during Roslyn rebuilds, eliminating stale diagnostics when editing companion files
- **Hover for declarations** — hover now shows type info for local variables, parameters, and fields via `GetDeclaredSymbol` fallback
- **Hover for delegate types** — delegate-typed symbols show invoke signature (e.g. `void Action(int value)`) instead of raw enum name
- **CS1662 lambda cascade** — suppressed cascading lambda conversion errors caused by state-setter type mismatches
- **Log spam cleanup** — removed 7 hot-path log calls that fired on every keystroke, rebuild, or hover

### Changed
- **Documentation rewritten** — all docs updated to reflect hook/module `.uitkx` companion approach; no more `.cs` companion file references

## [0.3.3] - 2026-04-07

### Fixed
- **VS2022 CI build** — pipeline now correctly packages LSP server binaries in VSIX; clean marketplace installs no longer fail with "no launch strategy succeeded"

### Added
- **HMR hook signature detection** — both emitters now emit `[HookSignature]` attribute with ordered hook call list. `UitkxHmrDelegateSwapper` compares old/new signatures before render and proactively resets all component state on mismatch, preventing silent hook corruption.

### Fixed
- **HMR state reset now comprehensive** — `FullResetComponentState` runs effect cleanups, disposes signal subscriptions, and clears hook states, queued updates, setter caches, context dependencies (previously only `HookStates` was cleared)
- **Hook order validation activated** — `HookOrderPrimed` now set to `true` after first render, enabling the previously dead runtime hook-order validation code path
- **Formatter snapshot tests stabilised** — Replace target updated to match current sample file content, fixing 32 spurious CI failures

## [0.3.2] - 2026-04-07

### Breaking
- **Comment syntax changed** — `{/* */}` JSX comments replaced with standard `//` (line) and `/* */` (block) comments in markup. Existing `{/* */}` comments in JSX return blocks must be converted.

### Added
- **UITKX0025 for variable assignments** — `var x = (<A/><B/>)` now correctly flagged as single-root violation in IDE diagnostics
- **Block comments in markup** — `/* */` now supported in JSX markup for multi-line comments

### Fixed
- **`@(expr)` type enforcement** — VDG now emits `VirtualNode` (not `object`) for inline `@(expr)`, matching the SG's cast. IDE shows errors for non-VirtualNode expressions early.
- **Formatter block diff** — formatter now uses a single block TextEdit instead of per-line diffs, eliminating corruption on files with blank-line variations
- **Formatter idempotency** — bare-return formatting now matches canonical form on first pass
- **Formatter preserves empty containers** — `<Box></Box>` no longer collapsed to self-closing by the formatter
- **HMR comment node handling** — fixed pre-existing dangling comma bug in `EmitChildArgs` when comment nodes appear between children

## [0.3.1] - 2026-04-05

### Added
- **Rules of Hooks validation in SG** — `HooksValidator` now scans SetupCode in all control blocks (`@if`, `@foreach`, `@for`, `@while`, `@switch`) for hook calls (UITKX0013–0016)
- **UseEffect missing-deps in SetupCode** — `StructureValidator` now scans control-block SetupCode for `UseEffect` without dependency arrays (UITKX0018)
- **StyledAssetDemoFunc sample** — new sample component demonstrating `@uss` directive with className-based USS styling

### Fixed
- **`@foreach` emitter double-brace bug** — `EmitForeachNode` produced invalid C# when SetupCode was present (`}}` in plain string instead of `}` in the IIFE closing)

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

### Docs
- **Documentation audit complete** — all 67 identified gaps now addressed: expanded guides for hooks, context, events, refs, keys, HMR, styling, advanced API, known issues, and more
- **CodeBlock syntax highlighting** — fixed non-functional C# highlighting in docs site by switching all code blocks to JSX (prism-react-renderer compatible)
- **`onChange` event documented** — added `ChangeEventHandler<T>` / `ChangeEvent<T>` to the Events page reference table

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
