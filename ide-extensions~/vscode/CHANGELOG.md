# Changelog

## [1.0.303] - 2026-04-04
- Feature: control block bodies — @if, @for, @foreach, @while, @switch cases now support setup code before return() (var declarations, lambdas, local computation)
- Feature: switch fallthrough — adjacent @case labels with no body share the same branch
- Feature: Rules of Hooks diagnostics (UITKX0013–0016) now reported in the IDE — hooks called inside @if, @foreach, @for, @while, @switch, or event-handler attributes are flagged as errors
- Fix: @foreach with setup code generated invalid C# (double-brace bug in IIFE emitter)
- Fix: ResolveAssetPaths now applied inside all control block bodies (SG + HMR emitters)
- Fix: removed dead @code, @break, @continue grammar rules and schema entries

## [1.0.302] - 2026-04-02
- Breaking: CssHelpers renamed all shortcuts with consistent prefixes for autocomplete discoverability
- Feature: compound struct factory helpers in CssHelpers (BgRepeat, BgPos, BgSize, Origin, Xlate, Easing + 24 easing presets)
- Feature: TextAutoSizeMode support across all layers (StyleKeys, Style, CssHelpers, PropsApplier, schema, LSP)
- Feature: LSP style value completions for backgroundRepeat, backgroundPosition, backgroundSize, transitionTimingFunction

## [1.0.301] - 2026-04-01
- Feature: LSP enum value completions for CssHelpers shortcuts (PickingMode, SelectionType, ScrollerVisibility, LanguageDirection, SliderDirection, ScrollViewMode, ScaleMode, TwoPaneSplitViewOrientation, ColumnSortingMode)

## [1.0.301] - 2026-03-29
- Fix: updated plan documents to reflect actual implementation status

## [1.0.300] - 2026-03-29
- Fix: formatter no longer expands empty open+close elements to multi-line
- Feature: LSP attribute-level version filtering in completions and UITKX0200 diagnostics
- Docs: component reference pages now show inline Unity docs link next to title

## [1.0.299] - 2026-03-28
- Fix: formatter no longer collapses empty open+close tags to self-closing (<Box></Box> is preserved)
- Fix: tag completion no longer inserts closing tag for elements that accept children

## [1.0.298] - 2026-03-28
- Feature: Find All References (Shift+F12) for C# symbols and component tag names across .uitkx files
- Feature: F12 / Ctrl+Click on component declaration shows references (JSX-style)

## [1.0.297] - 2026-03-28
-  Fix: tag completion no longer inserts closing tag snippet when editing an existing tag name

## [1.0.296] - 2026-03-28
- Fix: autocomplete no longer inserts duplicate ={} when attribute already has a value binding

## [1.0.295] - 2026-03-27
- Feature: @uss directive with formatter, syntax highlighting, and UITKX0022/0023 SG diagnostics
- Feature: HMR support for @uss — .uss file changes trigger hot-reload of dependent components

## [1.0.294] - 2026-03-28
- Feature: UITKX0121 — LSP error when Asset<T> type is incompatible with file extension
- Feature: Extension-to-type validation map covering 20+ file types (images, audio, fonts, materials, USS)

## [1.0.293] - 2026-03-27
- Feature: UITKX0120 — LSP error squiggle when Asset/Ast path references a file that does not exist
- Feature: Asset<T>/Ast<T> stub support in LSP virtual document for IntelliSense

## [1.0.292] - 2026-03-27
- Rename Style.TextColor to Style.Color to match StyleKeys and Unity IStyle naming
- Remove classic directive mode — function-style only
- Centralized changelog system: single changelog.json source of truth, CI generates per-IDE changelogs
