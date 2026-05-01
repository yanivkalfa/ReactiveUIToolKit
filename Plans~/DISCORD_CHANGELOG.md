## [0.4.12] - 2026-05-01

### Doom demo — Phase 9 sector-engine release
No runtime / source-generator / IDE changes this cycle. The `Samples/Components/DoomGame/` demo went from a flat raycaster to a real sector-portal engine — stacked floors, key-chain progression, minimap, and a full status bar.

### Renderer
- **Sector / portal raycaster (Phase 1–3)** — tile map compiles to `MapData` (sectors + linedefs); rendering walks portals via per-ray cliprange (`winTop`/`winBot` screen-Y window). Variable floor/ceiling heights, upper/lower wall segs, and sky cells render correctly.
- **ExtraFloor stacked slabs (Phase 9)** — sectors carry any number of slabs; column rasterizer emits front + back TOP/BOTTOM/SIDE planes per slab and tightens the cliprange so taller slabs further along the ray stay visible. Fixes the staircase upper-treads-vanish bug; powers Level 6’s 7-step interior staircase.
- **Z-aware collision (Phase 7)** — `BlocksMovementZ(footZ, headZ, STEP_HEIGHT)` replaces binary `BlocksMovement` for slab-aware step-up, jump, and crouch.

### Gameplay
- **6 hand-built levels** — Hangar, Toxin Refinery, Containment Area, Outpost, Phobos Anomaly, and a boss-only finale.
- **Level 1 progression rebuild** — hub gates side wings behind colored doors: yellow key in hub → east wing (red key) → west wing (blue key + shotgun) → north boss room (Baron + Cacodemon). Walls flank every door so they can’t be sidestepped.
- **Boss-gated exits** — new `LevelStart.BossExitGated` + `GameLogic.AnyBossAlive` blocks level-end until every Baron / Cacodemon is dead, with a HUD message on attempt.
- **Walkable exit pads** — new `MapBuilder.ExitPad(x, y)` creates an Exit-kind cell with no wall texture and a deep-blue floor (`F_BLUE`); back wall painted with new `W_BRICK_BLUE` so the end-zone reads visually.
- **Boss balance** — Baron HP 800 → 200, Cacodemon 400 → 120; the Level 1 boss now drops in a few shotgun blasts.

### UI
- **Status bar rewrite** — 8-panel `FlexGrow`-ratio layout (AMMO / HEALTH / ARMS / FACE / ARMOR / KEYS / BREAKDOWN / INFO) filling the full 800×90 region; consistent title spacing, `WhiteSpace.NoWrap`, ARMS as a 3-column 7-weapon button grid.
- **Live minimap** — top-right overlay, auto-scales to fit any map into 160px. Walls, color-keyed doors, exit pad, player (yellow + heading), and every live mobj (red enemies, cyan pickups, key-color keys).

---

## [0.4.11] - 2026-04-28

### Performance
- **JSX children fast-path (OPT-V2-1)** — source generator emits children directly into `params VirtualNode[]` instead of allocating a transient `__C(...)` wrapper when the children list is statically simple. One allocation per element gone.
- **Static-style hoisting (OPT-V2-2)** — `style={new Style{...}}` literals with all-constant initializers are hoisted to class-level `static readonly Style` fields. The reconciler's `SameInstance` check makes the diff walk a no-op when the same instance is reused across renders. Falls back to the pool-rent path for any non-literal value.

### Extensions
VS Code **1.1.5 → 1.1.6** · VS2022 **1.1.5 → 1.1.6**
- Source-generator parity — both LSPs ship the new hoisting + children fast-path emit.
- **Cross-document `Ref<T>` unification** — `useRef<T>()` returns canonical `global::ReactiveUITK.Core.Ref<T>` in virtual docs, killing false `CS1503` when passing refs to peer hooks.
- **Polyfill stubs load correctly** in workspaces where Unity hasn't compiled yet.
- **Formatter idempotency for `&&` / `||` chains** inside nested blocks — no more indent drift on save.

---

## [0.4.10] - 2026-04-27

### Performance
- **Major reconciler & props pipeline rewrite** — UITKX went from ~1.7× overhead vs. native UIToolkit (28 FPS at the 3000-box stress benchmark) to ~78% of native (36–38 FPS).
- **Typed Props + Style pipelines** — eliminated ~27,000 dictionary/boxing allocations/frame; flat backing-field structs replace `Dictionary<string, object>`.
- **Object pooling + IIFE closure elimination** — removed ~6,000 `Style`/`BaseProps` allocations and ~3,000 loop-body closures per frame.

### Added
- **Doom-style game demo** (`Samples/Components/DoomGame/`). Window: `ReactiveUITK/Demos/Doom Game`.
- **Typed Props for editor field types** — `BoundsField`, `ColorField`, `DropdownField`, `EnumField`, `MinMaxSlider`, `ObjectField`, `Slider(Int)`, `Tab(View)`, `Toggle(ButtonGroup)`, `Vector2/3/4Field`, and more.

### Changed
- **SG diagnostic IDs unified with live analyzer** — seven codes renumbered so the same issue surfaces with the same ID in both Unity Console and IDE Problems pane (`UITKX0006/0002/0009/0010/0017/0022/0023 → 0103/0109/0106/0104/0108/0120/0121`).
- **Initial `CreateRoot` render is now synchronous** — no empty-frame flicker between `Clear()` and the first commit.

### Fixed
- **Cross-wired Style / BaseProps "disco" bug** — pooled instance could be returned twice in one flush window and re-rented to two fibers. Fixed via idempotent `_isPendingReturn` guard.
- **`<ErrorBoundary>` stuck on fallback after `resetKey` change**, **`<Portal target={x}>` ignored target changes** — bailout clones now refresh the relevant fields from the new VNode.
- **VNode pooling reverted** — VNode refs can live inside opaque `IProps` payloads (e.g. `Route.Element`); pool returns produced cross-wired trees.

---

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
