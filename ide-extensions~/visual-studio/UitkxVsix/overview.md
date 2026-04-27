# UITKX

Language support for `.uitkx` ReactiveUIToolKit component templates in Visual Studio.

## Features

- Syntax highlighting for directives, control flow, tags, attributes, and embedded C# expressions
- Basic editor tooling through the bundled language server
- Formatting support via the UITKX formatter

## Repository

https://github.com/ReactiveUITK/ReactiveUIToolKit

## Changelog

### [1.1.6] - 2026-04-28
- Perf: source generator now hoists `style={new Style{...}}` literals to class-level static fields, eliminating per-render Style allocations and diff-walks for static styles. Handles both setter form (`Width = 5f`) and tuple form (`(StyleKeys.Width, 5f)`). Whitelist-based; falls back to existing pool-rent path for any non-literal value (state/captures/method calls).
- Perf: source generator emits child arguments directly into `params VirtualNode[]` instead of allocating a transient `__C(...)` array when the JSX children list is statically simple (no spreads, no conditional fragments). Cuts per-frame allocations on the children path.

### [1.1.5] - 2026-04-27
- Fix: cross-document Ref<T> unification — useRef<T>() now returns canonical global::ReactiveUITK.Core.Ref<T> in virtual docs, eliminating false CS1503 when passing refs to peer hooks (e.g. useGalagaGame(boardRef))
- Fix: polyfill stubs (Ref<T>, ReactiveEvent, etc.) now load correctly in workspaces where Unity has not yet compiled — detection scoped to runtime assembly names (ReactiveUITK.Shared/Runtime/Core), no longer suppressed by the LSP-internal ReactiveUITK.Language.dll
- Fix: formatter idempotency for multi-line paren-continuation (e.g. && / || chains) inside nested blocks — no more indent drift on save in files like the Galaga GameScreen sample
- Fix: source-generator diagnostic IDs unified with live analyzer (UITKX0006/0002/0009/0010/0017/0022/0023 → UITKX0103/0109/0106/0104/0108/0120/0121) so the same logical issue surfaces with the same code in both Unity Console and VS Code Problems pane
- Fix: RS2008 'analyzer release tracking' build warning suppressed in the LSP server csproj (warning targets analyzer NuGet packages, not the server EXE)

### [1.1.4] - 2026-04-18
- Fix: UITKX0105 false positives eliminated — unknown-element diagnostics suppressed until workspace scan completes
- Fix: @(__children) no longer produces CS0103 in function-style components

### [1.1.3] - 2026-04-17
- Fix: VDG inline expression checks no longer cause false code dimming past directive boundaries
- Fix: formatter handles deeply nested structures correctly (22 idempotency regressions resolved)
- Fix: improved diagnostic line mapping for UITKX0014, UITKX0013, and CS0219
- Fix: workspace scan now discovers components when VS2022 provides no rootUri (fixes UITKX0105 false positives, go-to-definition, and F2 rename)

### [1.1.2] - 2026-04-11
- Feature: capture-phase event handlers (onClickCapture, onKeyDownCapture, onChangeCapture, etc.) — every bubble event now has a Capture suffix variant that fires during trickle-down
- Feature: refactored ApplyEvent/RemoveEvent with generic RegisterEvent<T>/UnregisterEvent<T> helpers — zero-overhead, no behavioral change
- Fix: removed dead diagnostic counters (totalStyleSets, totalStyleResets, totalEventsRegistered, totalEventsRemoved) and unused GetStyleMetrics()
- Feature: IDE autocompletion and IntelliSense for all capture-phase event props

### [1.1.1] - 2026-04-11
- Fix: hook/module files with arbitrary middle segments (e.g. Counter.hook.uitkx, Counter.custom.uitkx) no longer produce invalid C# class names — container class derivation now truncates at the first dot instead of relying on a suffix whitelist

### [1.1.0] - 2026-04-10
- Feature: full IDE support for hook companion files (.hooks.uitkx) — parsing, diagnostics, hover, completions, coloring, formatting
- Feature: full IDE support for module companion files (.style.uitkx, .utils.uitkx) — parsing, diagnostics, hover, completions, coloring, formatting
- Feature: cross-file peer resolution reads editor buffers instead of disk — no stale diagnostics when editing companion files
- Feature: hover shows type info for local variable, parameter, and field declarations
- Feature: hover shows delegate invoke signature (e.g. void Action(int value)) instead of raw type name
- Fix: CS1662 lambda cascade errors suppressed when caused by state-setter type mismatches
- Fix: removed 7 hot-path log calls that fired on every keystroke, rebuild, or hover
- Feature: hook and module keywords colored as keywords in the classifier
- Feature: hover tooltip correctly renders (kind) prefix for Roslyn-style hover format

### [1.0.85] - 2026-04-09
- Feature: delegate-typed variables (useState setters, callbacks) now colored as functions in both VS Code and VS2022
- Fix: generic function calls like useState<string>() no longer lose function coloring
- Feature: custom notification channel (CustomMessageTarget) for server-to-client classification overrides

### [1.0.84] - 2026-04-08
- Fix: unreachable code after return now correctly dims entire scope without squiggly lines
- Fix: stale gray no longer persists after removing return statement — unreachable diagnostics stripped from T3 carry-forward cache
- Fix: unreachable code now renders fully gray — custom high-priority classification overrides competing VS2022 LSP syntax colors

### [1.0.84] - 2026-04-08
- Fix: eliminated color flickering/dancing in unreachable code regions caused by redundant classification sources and didChange spam
- Fix: centralized buffer sync via BufferSyncService — deduplicated 6 independent didChange senders to eliminate redundant server re-parses
- Fix: classifier subscriber leak — stale instances no longer accumulate on static diagnostic events

### [1.0.83] - 2026-04-07
- Fix: CI pipeline now correctly packages LSP server binaries in VSIX — clean marketplace installs no longer fail with 'no launch strategy succeeded'

### [1.0.82] - 2026-04-07
- Breaking: comment syntax changed — {/* */} replaced with // (line) and /* */ (block) in markup
- Fix: @(expr) inline expressions now type-checked as VirtualNode in IDE diagnostics
- Fix: UITKX0025 single-root validation now covers var x = (<A/><B/>)
- Fix: formatter uses block diff (single TextEdit) — eliminates corruption on blank-line variations
- Fix: formatter bare-return idempotency — first format pass now matches canonical form
- Fix: formatter no longer collapses empty container elements to self-closing
- Fix: semantic tokens for // and /* */ comments in markup
- Fix: simplified comment handler — always uses // line comments, removed context-dependent {/* */} logic

### [1.0.81] - 2026-04-05
- Feature: @uss and Asset<T>("...") path completion — filesystem-aware autocomplete inside quoted paths
- Feature: @uss added to preamble autocomplete and uitkx-schema.json directive list
- Feature: Rules of Hooks diagnostics (UITKX0013–0016) now reported in the IDE with accurate squiggle positions

### [1.0.80] - 2026-04-04
- Feature: control block bodies — @if, @for, @foreach, @while, @switch cases now support setup code before return() (var declarations, lambdas, local computation)
- Feature: switch fallthrough — adjacent @case labels with no body share the same branch
- Feature: Rules of Hooks diagnostics (UITKX0013–0016) now reported in the IDE — hooks called inside @if, @foreach, @for, @while, @switch, or event-handler attributes are flagged as errors
- Fix: @foreach with setup code generated invalid C# (double-brace bug in IIFE emitter)
- Fix: ResolveAssetPaths now applied inside all control block bodies (SG + HMR emitters)
- Fix: removed dead @code, @break, @continue grammar rules and schema entries

### [1.0.79] - 2026-04-02
- Breaking: CssHelpers renamed all shortcuts with consistent prefixes for autocomplete discoverability
- Feature: compound struct factory helpers in CssHelpers (BgRepeat, BgPos, BgSize, Origin, Xlate, Easing + 24 easing presets)
- Feature: TextAutoSizeMode support across all layers (StyleKeys, Style, CssHelpers, PropsApplier, schema, LSP)
- Feature: LSP style value completions for backgroundRepeat, backgroundPosition, backgroundSize, transitionTimingFunction

### [1.0.78] - 2026-04-01
- Feature: LSP enum value completions for CssHelpers shortcuts (PickingMode, SelectionType, ScrollerVisibility, LanguageDirection, SliderDirection, ScrollViewMode, ScaleMode, TwoPaneSplitViewOrientation, ColumnSortingMode)

### [1.0.78] - 2026-03-29
- Fix: updated plan documents to reflect actual implementation status

### [1.0.77] - 2026-03-29
- Fix: formatter no longer expands empty open+close elements to multi-line
- Feature: LSP attribute-level version filtering in completions and UITKX0200 diagnostics
- Docs: component reference pages now show inline Unity docs link next to title

### [1.0.76] - 2026-03-28
- Fix: formatter no longer collapses empty open+close tags to self-closing (<Box></Box> is preserved)
- Fix: tag completion no longer inserts closing tag for elements that accept children

### [1.0.75] - 2026-03-28
- Feature: native LSP routing via CodeRemoteContentTypeName — removes 3 custom GoToDefinition handler files
- Feature: Find All References (Shift+F12) now works natively through VS2022 LSP framework

### [1.0.74] - 2026-03-28
-  Fix: tag completion no longer inserts closing tag snippet when editing an existing tag name

### [1.0.73] - 2026-03-28
- Fix: autocomplete no longer inserts duplicate ={} when attribute already has a value binding

### [1.0.72] - 2026-03-27
- Feature: @uss directive with formatter, syntax highlighting, and UITKX0022/0023 SG diagnostics
- Feature: HMR support for @uss — .uss file changes trigger hot-reload of dependent components

### [1.0.71] - 2026-03-28
- Feature: UITKX0121 — LSP error when Asset<T> type is incompatible with file extension
- Feature: Extension-to-type validation map covering 20+ file types (images, audio, fonts, materials, USS)

### [1.0.70] - 2026-03-27
- Feature: UITKX0120 — LSP error squiggle when Asset/Ast path references a file that does not exist
- Feature: Asset<T>/Ast<T> stub support in LSP virtual document for IntelliSense

### [1.0.69] - 2026-03-27
- Rename Style.TextColor to Style.Color to match StyleKeys and Unity IStyle naming
- Remove classic directive mode — function-style only
- Centralized changelog system: single changelog.json source of truth, CI generates per-IDE changelogs
