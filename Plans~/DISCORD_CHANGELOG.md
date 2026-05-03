## [0.4.17] - 2026-05-03

### Two converging bugs around `.style.uitkx` / `.hooks.uitkx` — both fixed

A consumer hit `[UITKX] Asset not found in registry: "../Resources/background-01.png"` from `AppRoot.style.uitkx`. Investigation surfaced two independent latent bugs that both manifest in module/style/hook files.

**Bug 1 — HMR `ArgumentException` on every save.** `UitkxHmrCompiler.InvokeWithDefaults` had two `params object[]` overloads. C# overload resolution preferred `string → object target` over `string → params object[]`, so calls bound to the *wrong* overload — the first `string` arg silently became the (ignored) target receiver and every following arg shifted left by one position. A `List<ParseDiagnostic>` then landed in `DirectiveParser.Parse`'s `filePath` slot:

> `ArgumentException: Object of type 'List<ParseDiagnostic>' cannot be converted to type 'System.String'.`

The two overloads were collapsed into a single canonical `(MethodInfo method, object target, params object[] args)` where `target` is **mandatory** — making the entire bug class structurally impossible to recur. All 11 call sites updated.

**Bug 2 — `Asset<T>(...)` literals never rewritten in `module` / `hook` bodies.** The runtime `UitkxAssetRegistry` is keyed by **resolved** Unity asset paths (`Assets/Resources/background-01.png`); the compile-time emitter is supposed to rewrite `Asset<T>("./relative")` → that key. The rewrite covered component setup code, JSX attributes, and `@if`/`@foreach`/`@switch` bodies — but **not** `module {}` / `hook {}` bodies. So `BackgroundImage = Asset<Texture2D>("../Resources/bg.png")` shipped as the raw relative literal while the registry sync wrote the resolved key independently. The two halves disagreed → runtime miss.

`ResolveAssetPaths` was promoted from a private instance method on `EmitContext` to an `internal static` shared by all three source-gen emitters (`CSharpEmitter`, `HookEmitter`, `ModuleEmitter`); HMR's parallel `HmrCSharpEmitter.ResolveAssetPaths` had its visibility promoted so `HmrHookEmitter` can route module + hook bodies through it. Source-gen and HMR now produce literal-identical asset strings.

**Why these are related.** `.style.uitkx` and `.hooks.uitkx` are the convergence point — Bug 1 prevented HMR from compiling them at all, Bug 2 meant even when source-gen ran cleanly, the registry lookup still missed. Both bugs needed fixing for `Asset<T>` inside `module` / `hook` blocks to work end-to-end.

**Tests.** 4 new regression tests in `EmitterTests` (module + relative `./`, module + `../`, module + absolute, hook + relative). They run on every push, every PR, and before every package publish via the existing GitHub Actions workflows — the bug class cannot ship again. **1064/1064 SG** passing.

VS Code **1.1.9 → 1.1.10** · VS 2022 **1.1.9 → 1.1.10** ride the same release.

---

## [0.4.16] - 2026-05-03

### HMR — fix `TargetParameterCountException` + production-grade hardening

A reflection signature drift between the editor-only HMR compiler and `ReactiveUITK.Language.dll` (`UitkxParser.Parse` gained an optional `lineOffset` in 0.4.7) was firing `TargetParameterCountException` on every `.uitkx` save during play mode, swallowed into a silent warning + infinite retry storm.

**Layer 1 — fix.** Both `_uitkxParse.Invoke` sites now pass the trailing `lineOffset = 0`. Hot reload of components, hooks, and modules works again.

**Layer 2 — `InvokeWithDefaults` helper.** All six reflective calls into the language library now route through a helper that pads short argument arrays with each parameter's compile-time `DefaultValue`. One-time `Debug.LogWarning` per `MethodInfo` surfaces silent API drift the next time it happens, instead of failing.

**Layer 3 — infrastructure-error classifier + self-disable.** `HmrCompileResult.IsInfrastructureError` is now set when the inner exception is `TargetParameterCountException | MissingMethodException | MissingFieldException | TypeLoadException | ReflectionTypeLoadException | BadImageFormatException`. The controller emits one `Debug.LogError` with actionable text and calls `Stop()` (the only safe disable path — unhooks events, stops the watcher, unlocks the assembly-reload suppressor, restores `runInBackground`, clears retry queues). User-authored compile errors (CS0103, CS1xxx, syntax) keep the existing warn + retry cascade.

VS Code **1.1.8 → 1.1.9** · VS2022 **1.1.8 → 1.1.9** — IDE virtual-document generator now injects `using static ReactiveUITK.AssetHelpers;` so `Asset<T>("...")` and `Ast<T>("...")` no longer report CS0103 in component setup blocks, hook bodies, and module/style initializers (e.g. `AppRoot.style.uitkx`).

**1060/1060 SG** passing. Source generator, runtime, build, and IDE extension surfaces are otherwise untouched.

---

## [0.4.14] - 2026-05-03

### Router — React-Router-v6 parity (additive, no breaking changes)

New primitives: **`<Outlet/>`**, **`<Routes>`** (ranked first-match-wins), **`<NavLink>`** (active styling), **`<Navigate to>`** (declarative redirect).

`<Route>` gains `index`, `caseSensitive`, and layout-route composition (`element` + child `<Route>`s feed `<Outlet/>`). `<Router>` gains `basename`; nested `<Router>` is now a hard error.

New `RouterHooks`: `UseOutletContext<T>`, `UseMatches`, `UseResolvedPath`, `UseSearchParams`, `UsePrompt`, `UseNavigate(NavigateOptions)`. Old signatures preserved.

Internals: shared `RouteRanker` (port of RR's `rankRouteBranches`/`computeScore`); single-source-of-truth tag-alias map shared by source-gen + HMR.

**1063/1063 SG** passing (6 new emission tests). VS Code **1.1.7 → 1.1.8** · VS2022 **1.1.7 → 1.1.8** ship schema entries for the new tags/attributes. Docs site router page rewritten to cover the new surface.

---

## [0.4.13] - 2026-05-02

### Style coverage — 13 missing IStyle properties wired end-to-end
Closes the long-standing UITKX vs `UnityEngine.UIElements.IStyle` wiring gap. Every IStyle property is now reachable via the typed `Style` API, the tuple `(StyleKeys.X, value)` form, and the IDE autocomplete schema.

### New typed properties (Unity 6.2 floor)
- **9-slice** — `UnitySliceLeft/Right/Top/Bottom`, `UnitySliceScale`, `UnitySliceType` (`Sliced`/`Tiled`).
- **Clipping** — `UnityOverflowClipBox` (`PaddingBox`/`ContentBox`).
- **Text** — `WordSpacing`, `UnityParagraphSpacing`, `TextShadow`, `UnityTextGenerator` (`Standard`/`Advanced`), `UnityEditorTextRenderingMode` (`SDF`/`Bitmap`, editor-only).
- **Fonts** — `UnityFontDefinition` (legacy `Font` or TextCore `FontAsset`).

New `CssHelpers`: `SliceFill`/`SliceTile`, `ClipPaddingBox`/`ClipContentBox`, `TextGenStandard`/`TextGenAdvanced`, `EditorTextSDF`/`EditorTextBitmap`, `Shadow(dx,dy,blur,color)`, `FontDef(font)`.

### Fix — 19 missing `styleResetters` (silent leak)
Setter/resetter audit found 19 `IStyle` properties with a setter but no resetter — removing them from a `style={}` block silently leaked the previous value. Now all reset to `StyleKeyword.Null` (`alignContent/Items/Self`, `backgroundPositionX/Y`, `backgroundRepeat/Size`, `flexDirection/Wrap`, `fontFamily/Size`, `justifyContent`, `position`, `rotate`, `scale`, `textAlign`, `transformOrigin`, `translate`, `unityFontStyle`).

### Tests
New `IStyleCoverageTests` (7 facts) regex-asserts every IStyle property is wired through every layer. Future Unity versions can no longer add an unwired property without a red CI. **1051/1051 SG · 61/61 LSP** passing.

### Extensions
VS Code **1.1.6 → 1.1.7** · VS2022 **1.1.6 → 1.1.7** — autocomplete for the 4 new enum-valued IStyle properties via embedded `uitkx-schema.json`.

---

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
