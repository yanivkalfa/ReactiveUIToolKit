# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

## [0.4.15] - 2026-05-03

### Fixed

- **Source generator (CS8323):** the no-props `V.Func` emit branch produced
  `V.Func(Type.Render, key: "k", child)` when a parameterless user component
  wrapped element children (e.g. `<MenuPage><HomePage/></MenuPage>`). The
  named `key:` argument landed at call slot 2 while its natural slot is 3,
  triggering CS8323 ("Named argument used out-of-position but is followed by
  an unnamed argument"). Emit now inserts a positional `null` for the IProps
  `props` slot — `V.Func(Type.Render, null, key: "k", child)` — mirroring the
  shape already used by the typed-props branch. Zero runtime / IL change
  (`null` flows through `?? EmptyProps.Instance` exactly as the implicit
  default did). Patch applied to both the cold-build emitter and the HMR
  emitter so hot-reload behaves identically. Regression test added
  ([NoPropsFuncWithChildrenRegressionTest.cs](SourceGenerator~/Tests/NoPropsFuncWithChildrenRegressionTest.cs))
  recompiles the generated source against a real-shape `V.Func` stub and
  asserts no CS8323.

## [0.4.14] - 2026-05-03

### Router — React-Router-v6 parity for layout routes, ranking, and DX hooks

This release closes the structural gap between the UITKX router and React Router v6.
Existing apps continue to work unchanged — every change is additive — but new apps
can now compose layout routes with `<Outlet/>`, rely on deterministic
ranking via `<Routes>`, and use the same DX hooks RR users expect.

#### New components

- **`<Outlet/>`** — render-slot for nested routes. A parent `<Route element=...>`
  with child `<Route>`s now publishes the matched child into context; the
  descendant `<Outlet/>` renders it. Optional `context` prop is exposed to
  descendants via `RouterHooks.UseOutletContext<T>()`.
- **`<Routes>`** — first-match-wins selector. Walks child `<Route>` declarations,
  ranks them with a port of RR's `rankRouteBranches` / `computeScore` (constants
  unchanged: `staticSegmentValue=10`, `dynamicSegmentValue=3`, `splatPenalty=-2`,
  `indexRouteValue=2`, `emptySegmentValue=1`), and renders only the highest-ranked
  match. Replaces ad-hoc "two routes both matched" foot-guns.
- **`<NavLink>`** — built-in navigation link with active styling (`activeStyle`,
  `end`, `caseSensitive`). Activation rules mirror RR exactly, including the
  `to="/"` special case.
- **`<Navigate to>`** — declarative redirect. Defaults to `replace=true` so
  redirects don't grow history. Useful for `<Route path="/" element={<Navigate to="/welcome"/>}/>`.

#### `<Route>` upgrades

- `index="true"` — index routes match the parent pattern exactly (no extra segment).
  Setting both `index` and `path` now throws an actionable
  `InvalidOperationException`.
- `caseSensitive="true"` — opt-in to case-sensitive segment matching for that
  Route only (default remains case-insensitive for back-compat).
- **Layout routes** — when both `element=...` and child `<Route>`s are present,
  `<Route>` now acts as a layout: it ranks the children, publishes the matched
  child to the descendant `<Outlet/>`, and renders its element wrapper. When no
  nested `<Route>`s are present, behavior is byte-identical to today.

#### `<Router>` upgrades

- `basename="/app"` — URL prefix the router treats as the application root.
  Locations are stripped of the prefix on the way in and re-attached on the
  way out (push/replace).
- Nested `<Router>` is now a hard error
  (`InvalidOperationException("UITKX <Router> cannot be nested ...")`)
  instead of silently shadowing context — mirrors RR's `invariant(!useInRouterContext())`.

#### New hooks (`RouterHooks`)

- `UseOutletContext<T>()` — typed accessor for the value passed via
  `<Outlet context=...>`.
- `UseMatches()` — ordered chain of `RouteMatch` from root → current route
  (breadcrumbs / debug overlays / analytics).
- `UseResolvedPath(string to)` — pure path resolver against the current
  navigation base.
- `UseSearchParams()` — `(IReadOnlyDictionary<string,string> Query, Action<…,bool> Set)`
  tuple. The setter preserves the path component and replaces only the query.
- `UsePrompt(bool when, string message = null)` — convenience over `UseBlocker`.
- `UseNavigate(NavigateOptions options)` — overload returning a path-only
  navigator pre-bound to `Replace`/`State`. Old `UseNavigate(bool replace = false)`
  remains for back-compat.

#### Ranker

- New internal `RouteRanker` (port of RR `flattenRoutes` + `rankRouteBranches` +
  `computeScore`) shared by `<Routes>` and the layout-route flow on `<Route>`.
  Higher-score routes win; ties break by declaration order.

#### Source generator + HMR

- `Router/Route/Link` alias map de-duplicated. Single source of truth lives at
  `Shared/Core/Router/RouterTagAliases.cs` and is linked into the source generator
  via `<Compile Include>`. Adding a new router primitive now touches **one**
  dictionary entry instead of two. New entries: `Outlet → OutletFunc`,
  `Routes → RoutesFunc`, `NavLink → NavLinkFunc`, `Navigate → NavigateFunc`.

#### IDE schema

- `ide-extensions~/grammar/uitkx-schema.json` updated with full attribute
  metadata for the new components. VS Code, Rider, and Visual Studio extensions
  inherit autocompletion and inline documentation automatically.

#### Tests

- 6 new emission tests in `SourceGenerator~/Tests/EmitterTests.cs` lock the
  alias map to its expected codegen for every router primitive.
- 1063 total source-generator tests (was 1057); same 2 pre-existing snapshot
  failures (PortalsPlayground.uitkx) unrelated to this change.

#### Internals

- `RouteMatcher.Match` now accepts an optional `caseSensitive` parameter
  (overload preserves the old default-case-insensitive behavior).
- `RouterPath` gained `BuildQuery`, `StripBasename`, `WithBasename` helpers.
- `RouterContextKeys` gained `OutletElement`, `OutletContext`, and `MatchChain`.
- `RouterState` gained `Basename` plus a constructor parameter (default `"/"`).
- `RouteFuncProps` gained `Index` and `CaseSensitive`. `RouterFuncProps` gained
  `Basename`. All defaults preserve current behavior.

#### Backward compatibility

Every change is additive. Existing samples (`RouterDemoFunc`, `MainMenuRouterDemoFunc`,
and downstream user apps) compile and render byte-identically. The only
behavioral change is:
- Nested `<Router>` now throws (previously silently shadowed).
- `<Route index>` with a path now throws (previously silently ignored both).

Both throws replace **silently broken** behavior with **loudly broken** behavior
and catch real bugs at startup instead of in production.

#### Deferred (intentional)

- **Optional segments (`:lang?`)** — Phase 3.7 in
  `Plans~/ROUTER_GAP_CLOSURE_PLAN.md`. Requires porting `explodeOptionalSegments`
  and reworking the ranker's stability ordering; safe to add later as it's purely
  additive in `RouteMatcher`/`RouteRanker`.
- **Static analyzer for ambiguous sibling `<Route>` patterns** — Phase 4.2.
  Best implemented as an AST pass in
  `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs` once user
  reports validate the noise/signal ratio. Until then, wrap competing routes in
  `<Routes>` to get deterministic first-match-wins behavior.

See `Plans~/ROUTER_GAP_CLOSURE_PLAN.md` and `Plans~/ROUTER_REACT_ROUTER_COMPARISON.md`
for the full design analysis.

## [0.4.13] - 2026-05-02

### IStyle coverage — 13 missing properties wired end-to-end

Closes the long-standing gap between `UnityEngine.UIElements.IStyle` (Unity
6.2 floor: 84 properties) and the UITKX style pipeline. All 13 properties
listed below are now first-class typed setters with full bitmask diffing,
SetByKey/GetByKey support, pool reset, source-generator literal hoisting,
HMR mirror, and IDE schema entries. A new xUnit coverage test
(`IStyleCoverageTests`, 7 facts) locks parity in CI so future Unity
versions cannot land an unwired property.

#### New typed `Style` properties

- **9-slice (6 props):** `UnitySliceLeft`, `UnitySliceRight`, `UnitySliceTop`,
  `UnitySliceBottom` (each `StyleInt`), `UnitySliceScale` (`StyleFloat`),
  `UnitySliceType` (`SliceType` — `Sliced` / `Tiled`).
- **Clipping:** `UnityOverflowClipBox` (`OverflowClipBox` —
  `PaddingBox` / `ContentBox`).
- **Text spacing:** `UnityParagraphSpacing` (`StyleLength`),
  `WordSpacing` (`StyleLength`).
- **Text shadow:** `TextShadow` (`TextShadow` struct — offset, blur, color).
- **Advanced font:** `UnityFontDefinition` (`FontDefinition` — wraps a
  legacy `Font` or a TextCore `FontAsset`).
- **Text generator:** `UnityTextGenerator` (`TextGeneratorType` —
  `Standard` / `Advanced`).
- **Editor text rendering:** `UnityEditorTextRenderingMode`
  (`EditorTextRenderingMode` — `SDF` / `Bitmap`; editor-only behaviour).

#### New `CssHelpers` shortcuts

- `SliceFill`, `SliceTile` (SliceType)
- `ClipPaddingBox`, `ClipContentBox` (OverflowClipBox)
- `TextGenStandard`, `TextGenAdvanced` (TextGeneratorType)
- `EditorTextSDF`, `EditorTextBitmap` (EditorTextRenderingMode)
- `Shadow(dx, dy, blur, color)` → `TextShadow`
- `FontDef(font)` → `FontDefinition`

#### Fix — 19 pre-existing missing `styleResetters`

While auditing setter/resetter parity, surfaced 19 `IStyle` properties
that had a `styleSetters` entry but no matching `styleResetters` entry
(silently leaked previous values when removed from a style block):
`alignContent`, `alignItems`, `alignSelf`, `backgroundPositionX`,
`backgroundPositionY`, `backgroundRepeat`, `backgroundSize`,
`flexDirection`, `flexWrap`, `fontFamily`, `fontSize`, `justifyContent`,
`position`, `rotate`, `scale`, `textAlign`, `transformOrigin`,
`translate`, `unityFontStyle`. All now reset to `StyleKeyword.Null`.

#### Internals

- `Style` bit budget extended from 79 to 92 (`_setBits1` bits 15–27;
  total 128 still in budget).
- `Style.__Rent()` pool reset now clears `_textShadow` and
  `_unityFontDefinition` (reference-bearing structs).
- Source-generator hoisting whitelist (`s_literalCtorTypes` in
  `CSharpEmitter` and HMR mirror) now accepts `TextShadow` and
  `FontDefinition` literal initializers — all-literal `Style` blocks
  with `Css.Shadow(...)` or `Css.FontDef(...)` get lifted to a
  `private static readonly Style __sty_N` and reused across renders.
- IDE schema (`uitkx-schema.json`) gained 4 enum value lists:
  `unitySliceType`, `unityOverflowClipBox`, `unityTextGenerator`,
  `unityEditorTextRenderingMode`.

#### Documentation

- Styling page property catalog: 13 new property cards across the Text,
  Enum Styles, Background, and Assets categories.
- Styling page enum-shortcuts table: 4 new rows (SliceType,
  OverflowClipBox, TextGeneratorType, EditorTextRenderingMode).
- Styling page compound-helpers table: 2 new rows (TextShadow,
  FontDefinition).
- CssHelpers Reference page: 6 new helper groups.
- Search index extended with the new property and helper names.

## [0.4.12] - 2026-05-01

### Doom demo — Phase 9 sector-engine release

This release is a non-library update: no UITKX runtime / source-generator /
IDE changes. Everything below is the `Samples/Components/DoomGame/` demo,
promoted from a flat 2.5D raycaster to a full sector-portal engine with
stacked floors, a key-chain progression, a minimap, and a polished status
bar. Pulled in to demonstrate that UITKX can host a real interactive game
on top of the typed-props / hoisted-style render pipeline shipped in 0.4.10
/ 0.4.11.

#### Renderer

- **Sector / portal raycaster (Phase 1–3).** Tile map is converted to a
  `MapData` of sectors + linedefs at level start; rendering walks portals
  via a per-ray cliprange (Plan C `winTop`/`winBot` screen-Y window) instead
  of the old single-cell DDA. Variable floor / ceiling heights, upper /
  lower wall segments, and sky cells render correctly.
- **ExtraFloor stacked slabs (Phase 9).** Sectors can carry any number of
  `ExtraFloor` slabs; the column rasterizer emits front-side and back-side
  TOP / BOTTOM / SIDE planes per slab and tightens `winTop` / `winBot` per
  ray so taller slabs further along the ray stay visible. Fixes the
  long-standing “staircase upper treads disappear behind the lower one”
  bug — used by Level 6’s 7-step interior staircase.
- **Z-aware collision (Phase 7).** `MapDef.BlocksMovementZ(footZ, headZ,
  STEP_HEIGHT)` replaces the binary `BlocksMovement` for slab-aware step-up,
  jump, and crouch. Player is anchored to the current sector floor unless
  airborne.

#### Gameplay

- **6 hand-built levels** (`Level1`..`Level6`) in `DoomMaps.uitkx` covering
  Hangar, Toxin Refinery, Containment Area, Outpost, Phobos Anomaly, and
  the boss-only finale.
- **Level 1 progression rebuild.** Hub now gates side wings behind colored
  doors: pick up the yellow key in the hub center → east wing (red key) →
  west wing (blue key + shotgun) → north boss room (Baron + Cacodemon).
  Walls flank every door so they can’t be sidestepped.
- **Boss-gated exits.** New `LevelStart.BossExitGated` flag plus
  `GameLogic.AnyBossAlive(ref st)` blocks the level-end trigger until every
  Baron / Cacodemon is dead, with a “Kill the boss first.” HUD message on
  attempt.
- **Walkable exit pads.** New `MapBuilder.ExitPad(x, y)` creates an
  `Exit`-kind cell with no wall texture and a deep-blue floor (`F_BLUE`),
  so the back of the boss room reads as a clear visual end-zone instead of
  the legacy “EXIT” sign block.
- **Blue-brick back wall** (`W_BRICK_BLUE`) paints the wall behind the
  Level 1 exit pads to reinforce the end-zone signal.

#### UI

- **Status bar rewrite** (`DoomHUD.uitkx`). 8-panel `FlexGrow`-ratio layout
  (AMMO / HEALTH / ARMS / FACE / ARMOR / KEYS / BREAKDOWN / INFO) that
  fills the full 800×90 viewport-bottom region. Per-panel title labels
  with consistent vertical spacing and `WhiteSpace.NoWrap`. ARMS button
  group renders 7 weapons in 3 columns with centered justification.
- **Live minimap** (`DoomMinimap.uitkx`). Top-right overlay, auto-scales to
  fit the largest map dimension into 160px. Renders walls, color-keyed
  doors, the exit pad, the player (yellow dot + heading indicator), and
  every live mobj (red enemies, cyan pickups, key-color keys).
- **Boss / pickup balance.** Baron HP 800 → 200, Cacodemon HP 400 → 120 so
  the Level 1 boss can be cleared with a few shotgun blasts.

## [0.4.11] - 2026-04-28

### Performance

- **OPT-V2-1 — JSX children fast-path.** Source generator now emits child
  arguments directly into `params VirtualNode[]` instead of allocating a
  transient `__C(...)` wrapper array when the children list is statically
  simple (no spreads, no conditional fragments, no `@foreach`/`@for`/`@while`
  collectors). Eliminates one allocation per element on the hot render path.
- **OPT-V2-2 — Static-style hoisting.** Source generator now hoists
  `style={new Style{...}}` literals to class-level `static readonly Style`
  fields whenever every initializer value is a compile-time constant. Handles
  both setter form (`Width = 5f`) and tuple form (`(StyleKeys.Width, 5f)`).
  Whitelist covers literals, named-static dotted refs (`StyleKeys.X`,
  `Color.red`, `Position.Absolute`), and `new T(literal-args)` for
  `Color`/`Color32`/`Vector*`/`Length`/`TimeValue`/`Rect`/`Quaternion`. The
  reconciler's existing `SameInstance` check makes the diff walk a no-op when
  the same hoisted instance is supplied across renders. Falls back to the
  existing pool-rent path for any non-literal value (state-derived, captures,
  method calls, instance-member access on locals).

## [0.4.10] - 2026-04-27

### Performance

- **Major reconciler & props pipeline optimization pass.** Brought UITKX from
  ~1.7× overhead vs. native UIToolkit (28 FPS / 47 FPS at the 3000-box stress
  benchmark) up to ~78% of native (36–38 FPS). Real apps with partial updates
  will be much closer to native still. Notable items:
  - **Typed Props Pipeline** — eliminated ~6,000 dictionary allocations/frame
    on the props plumbing path (component → reconciler → element adapter).
  - **Typed Style Pipeline** — eliminated ~21,000 boxing + dictionary
    allocations/frame; styles now flow through a flat backing-field struct
    instead of `Dictionary<string, object>`.
  - **Style & BaseProps object pooling (OPT-16)** — removed ~6,000 object
    allocations/frame; pool runs at ~99% hit rate at steady state.
  - **`@foreach` / `@for` / `@while` IIFE closure elimination (OPT-10)** —
    `return` inside loop bodies rewritten to `__r.Add(...); continue;` so each
    iteration no longer allocates a delegate closure (~3,000 closures/frame
    eliminated). Also fixes a pre-existing `break`/`continue` semantics bug in
    `@for`/`@while` bodies.
  - **Event handler diff fast-path (OPT-22)** — `_hasEvents` flag on `BaseProps`
    skips ~43 `DiffEvent` calls per element when neither the previous nor next
    props carry any handler. ~+2 FPS at 3000 boxes.
  - **Quick-wins batch (OPT-4/5/7/11/23/24/25/26)** — small per-element wins
    across BaseProps equality, fragment fast-paths, fiber bailout, deletion
    tracking, and adapter dispatch.

### Added

- **Doom-style game demo sample** (`Samples/Components/DoomGame/`) — full
  demo built in UITKX: types, maps, game loop, hooks, styles, and a
  `DoomGameScreen` / `DoomHUD` / `DoomMainMenu` component split. Editor
  window: `ReactiveUITK/Demos/Doom Game`.
- **Pure UI Toolkit comparison harness** — `PureUIToolkitStressTestBootstrap`
  + editor window for measuring native UIToolkit alongside the UITKX stress
  test under identical conditions.
- **`ScrollView` `contentContainer` typed-path styling** — `contentContainer`
  prop now applies on both `ApplyTypedFull` and `ApplyTypedDiff` paths
  (previously only the untyped slot path applied it).
- **Typed Props for editor field types** — `BoundsField`, `BoundsIntField`,
  `ColorField`, `DoubleField`, `DropdownField`, `EnumField`, `EnumFlagsField`,
  `FloatField`, `Foldout`, `GroupBox`, `Hash128Field`, `HelpBox`, `Image`,
  `IntegerField`, `LongField`, `MinMaxSlider`, `MultiColumnListView`,
  `MultiColumnTreeView`, `ObjectField`, `ProgressBar`, `PropertyInspector`,
  `RadioButton`/`RadioButtonGroup`, `RectField`/`RectIntField`,
  `RepeatButton`, `Scroller`, `Slider`/`SliderInt`, `Tab`/`TabView`,
  `TemplateContainer`, `TextElement`, `TextField`, `ToggleButtonGroup`,
  `Toggle`, `Toolbar`, `TreeView`, `UnsignedIntegerField`/`UnsignedLongField`,
  `Vector2Field`/`Vector2IntField`/`Vector3Field`/`Vector3IntField`/`Vector4Field`,
  `IMGUIContainer`. All wired through `TypedPropsApplier` with full diff
  support.

### Changed

- **Source generator diagnostic IDs unified with live analyzer.** Seven
  diagnostics now use the analyzer's canonical IDs so the same logical issue
  surfaces with the same code in both the Unity Console (source generator) and
  the VS Code Problems pane (live analyzer):

  | Concept | Old (source-gen) | New (aligned) |
  |---|---|---|
  | `@component` name ≠ filename | `UITKX0006` | `UITKX0103` |
  | Unknown attribute on element | `UITKX0002` | `UITKX0109` |
  | Element inside loop missing `key` | `UITKX0009` | `UITKX0106` |
  | Duplicate sibling key | `UITKX0010` | `UITKX0104` |
  | Multiple root elements | `UITKX0017` | `UITKX0108` |
  | Asset path not found | `UITKX0022` | `UITKX0120` |
  | Asset type mismatch | `UITKX0023` | `UITKX0121` |

  Diagnostic text and severity are unchanged. Migrate any explicit ID
  references (e.g. CI grep rules) to the new codes.
- **Initial `CreateRoot` render is now synchronous.** The first render +
  commit phase runs to completion before `CreateRoot` returns, so the host
  container never appears empty for one frame between `Clear()` and the
  first commit. Mirrors React 18's `createRoot().render()` behaviour:
  initial mount is always synchronous; time-slicing is reserved for
  subsequent state-driven updates. Passive effects are still scheduled
  asynchronously.

### Removed

- **Dead `FiberNode.ContextProviderId` field.** The field had no production
  reads — it was only assigned in `CloneForReuse` and ignored by every
  consumer. Removing it slightly reduces the per-fiber memory footprint.
- **`VirtualNode` object pooling fully reverted.** VNode references can
  appear inside opaque `IProps` payloads (e.g. `Route.Element` and any
  slot-like prop), so pool returns produced dangling pointers and
  cross-wired component trees. VNodes are now plain GC heap objects.

### Fixed

- **Cross-wired Style / BaseProps "disco" bug.** A pooled `Style` or
  `BaseProps` instance could be scheduled for return twice in the same
  flush window — once during render-phase bailout and again from the
  commit-phase update — causing it to be pushed into the pool twice and
  then re-rented to two different fibers, which then mutated each other's
  styles. Fixed by adding an idempotent `_isPendingReturn` guard on both
  pools and by removing the render-phase pool-return entirely (the leak
  is bounded — the unused instance is collected when the owning component
  re-renders).
- **`<ErrorBoundary>` stuck on its fallback after `resetKey` change.**
  `CloneFromCurrent` was copying `ErrorBoundaryResetKey` from the previous
  fiber, so the clone always equalled the current and the change was never
  observed. The reset key is now refreshed from the new VNode and marked
  consumed against the alternate inside `UpdateErrorBoundary`.
- **`<Portal target={x}>` ignored target-prop changes.** When a portal's
  `target` prop pointed at a new container between renders, the bailout
  clone kept the previous `PortalTarget` / `HostElement`. Both now refresh
  from the new VNode.
- **Universal deletion tracking in `BeginWork`.** Function components
  (`ReconcileSingleChild`, null-return deletion) and fragments call into
  the reconciliation path directly, bypassing the wrapper that set
  `_hasDeletions`. Tracking now lives at the single universal `BeginWork`
  exit, covering every code path.
- **Diagnostic-test IDs realigned with renumbered codes.** The
  `UITKX0009_ForeachMissingKey` / `UITKX0010_DuplicateSiblingKey` /
  `UITKX0010_NotFiredForUniqueKeys` source-generator tests asserted the
  pre-renumber IDs and silently failed; now assert the canonical
  `UITKX0106` / `UITKX0104` codes.
- **Stray VS Code extension activation logging.** The extension previously
  logged `chatHistory` / `globalState.keys()` on every activation — leftover
  scaffolding from an unrelated experiment. Removed.
- **`RS2008` build warning in the language server.** Suppressed the
  "enable analyzer release tracking" warning, which targets analyzer NuGet
  packages, not the LSP server EXE.
- **Galaga sample dead code.** Removed unused `int beamH` local in
  `GameScreen.uitkx`.

## [0.4.9] - 2026-04-18

### Added
- **Galaga game demo** — full arcade-style Galaga game sample built entirely in UITKX. Features sprite-sheet rendering, entry wave formations with configurable delays, dive attacks with enemy shooting, tractor beam capture/release mechanics, dual-ship mode, multi-wave progression, and game-over/restart flow

## [0.4.8] - 2026-04-18

### Added
- **HMR delegate rollback guard** — if a hot-reloaded delegate crashes during render, the reconciler automatically rolls back to the previous working version, resets hook/effect state, and retries before falling through to the ErrorBoundary

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
