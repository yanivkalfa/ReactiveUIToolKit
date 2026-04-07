## [0.3.2] - 2026-04-07

### Breaking
- **Comment syntax normalized** — `{/* */}` JSX comments replaced with standard `//` (line) and `/* */` (block) in markup. Same syntax everywhere — setup code and JSX.

### Added
- **UITKX0025 for var assignments** — `var x = (<A/><B/>)` now flagged as single-root violation in IDE
- **Block comments in markup** — `/* */` supported in JSX markup for multi-line comments

### Fixed
- **`@(expr)` type enforcement** — inline `@(expr)` now type-checked as `VirtualNode` in IDE diagnostics. Non-VirtualNode expressions (e.g. `VirtualNode[]`) show errors early.
- **Formatter block diff** — single block TextEdit instead of per-line diffs, eliminates corruption on blank-line variations
- **Formatter idempotency** — bare-return formatting matches canonical form on first pass
- **Formatter preserves empty containers** — `<Box></Box>` no longer collapsed to `<Box />`
- **HMR dangling comma** — fixed pre-existing bug in `EmitChildArgs` when comment nodes appear between children

### IDE
- **VS Code** — removed custom `toggleBlockComment` command. `Ctrl+/` → `//`, `Shift+Alt+A` → `/* */`
- **VS2022** — simplified comment handler, always uses `//` line comments

### Extensions
VS Code **1.0.306** · VS2022 **1.0.82**
