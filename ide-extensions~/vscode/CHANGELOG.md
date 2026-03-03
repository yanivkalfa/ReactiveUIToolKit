# Changelog

## [1.0.12]
- Add Marketplace icon (`images/logo.png`) and wire manifest `icon` field so extension branding renders correctly.

## [1.0.11]
- Re-publish VS Code extension after IntelliSense/LSP stabilization so Marketplace package reflects current fixed behavior.

## [1.0.10]
- Fix formatter indentation for directive blocks nested under tags (`@if/@for/@case/@code`) and align closing `}` with opening directive level.
- Add `@case` expression tokenization before `{` and classify switch-arm labels (`count`, `0`, `1`, `_`) plus `=>` operator.

## [1.0.9]
- Fix control-flow grammar for `@if/@for/@foreach/@while/@switch` so variables and operators inside parentheses are tokenized/classified.
- Fix brace-based indentation for directive blocks so `@if (...) {` and `@for (...) {` indent correctly on Enter.

## [1.0.8]
- Fix onEnterRules: use plain string regex and correct string indentAction enum values

## [1.0.7]
- Fix onEnterRules: use integer indentAction values and { pattern } regex form; add editor.autoIndent=full default

## [1.0.6]
- Replace indentationRules with explicit onEnterRules for reliable Enter-key auto-indent after open tags

## [1.0.5]
- Fix on-Enter auto-indent: remove `<>` from bracket pairs (was confusing VS Code indent engine), improve increaseIndentPattern

## [1.0.4]
- Added rollForward LatestMajor to runtimeconfig so the server runs on .NET 8, 9, or 10+

## [1.0.3]
- Bundle all dependencies with esbuild so `vscode-languageclient` is available at runtime (fixes missing completions/hover after install)

## [1.0.2]
- Added explicit `activationEvents` to fix extension not activating when installed from Marketplace

## [1.0.1]

### Changed
- LSP server now targets .NET 8 (was .NET 10) — compatible with .NET 8, 9, and 10 runtimes

## [1.0.0]

### Added
- Syntax highlighting for `.uitkx` files (directives, control flow, element tags, attributes, embedded C#)
- Tag completions triggered by `<`
- Attribute completions for all built-in elements
- Directive and control-flow completions triggered by `@`
- Hover documentation (element descriptions, attribute types)
- Language configuration: bracket matching, auto-close, folding
- Bundled LSP server (`UitkxLanguageServer.dll`)
