# Changelog

## [1.0.62] - 2026-03-22
- Fix: debounce IndexChanged → diagnostics revalidation (500ms coalescing). Previously, each individual .cs file change triggered a full re-publish of diagnostics for all open .uitkx files, causing 70+ publishDiagnostics notifications during Unity recompilation bursts — freezing VS 2022's UI thread. Now coalesced into a single revalidation per burst.

## [1.0.61] - 2026-03-22
- Fix: LSP server build cross-contamination between VS Code and VS 2022 (shared obj/ directory). Added --runtime win-x64 to VS 2022 publish pipeline and obj/ cleanup before each build.

## [1.0.58] - 2026-03-21
- Initial tracked version.
