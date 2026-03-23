# Changelog

## [1.0.63] - 2026-03-22
- Fix: ActivateAsync now kills any previous orphaned server process before launching a new one. VS 2022 calls ActivateAsync again after receiving -32002 "Server Not Initialized" errors during the OmniSharp startup window, leaving zombie server processes that contend for resources.
- Fix: server log file contention when multiple LSP server processes run concurrently. ServerLog now uses FileShare.ReadWrite to prevent cascading failures in RoslynHost and DLL watcher setup.

## [1.0.62] - 2026-03-22
- Fix: debounce IndexChanged → diagnostics revalidation (500ms coalescing). Previously, each individual .cs file change triggered a full re-publish of diagnostics for all open .uitkx files, causing 70+ publishDiagnostics notifications during Unity recompilation bursts — freezing VS 2022's UI thread. Now coalesced into a single revalidation per burst.

## [1.0.61] - 2026-03-22
- Fix: LSP server build cross-contamination between VS Code and VS 2022 (shared obj/ directory). Added --runtime win-x64 to VS 2022 publish pipeline and obj/ cleanup before each build.

## [1.0.58] - 2026-03-21
- Initial tracked version.
