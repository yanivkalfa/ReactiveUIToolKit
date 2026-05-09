# Changelog

All notable changes to the ReactiveUIToolKit Unity package are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/).

For IDE extension changelogs (VS Code, Visual Studio 2022), see
`ide-extensions~/changelog.json` — the single source of truth for extension releases.

## [0.5.5] - 2026-05-09

### Added

- **JSX `&&` short-circuit splice in markup expression positions.** The React
  idiom `{cond && <Tag/>}` is impossible to emit verbatim because C# `&&` is
  bool-only and `bool && VirtualNode` is **CS0019**. The splicer now detects
  a trailing `&&` operator at the end of the prefix preceding any JSX literal
  in `{expr}` or `attr={expr}` positions and rewrites the expression to
  ternary form

  ```csharp
  ((cond) ? V.Tag(...) : (global::ReactiveUITK.Core.VirtualNode?)null)
  ```

  reusing the already-tested Phase 1 ternary path. The `null` fallback is
  dropped at render time by `__C(params object[])` which filters nulls — no
  runtime change required.

  A new shared precedence-aware walker
  `DirectiveParser.FindLhsStartForLogicalAnd` locates where the LHS of the
  `&&` begins inside the surrounding expression: single forward pass with
  per-paren-depth boundary tracking, lexer-aware string/comment skipping,
  and recognition of `?`, `:`, `??`, `||`, `,`, `;` as boundary tokens at
  the same paren depth as the `&&`. Examples:

  ```jsx
  // simple bool
  <Box>{flag && <Label text="hi"/>}</Box>

  // null check (the user-reported repro)
  <Box>{icon != null && <Image texture={icon}/>}</Box>

  // parenthesised LHS preserved
  <Box>{(x.Count > 0) && <Label text="non-empty"/>}</Box>

  // method-call LHS preserved (parens balanced)
  <Box>{IsActive(item) && <Label text="on"/>}</Box>

  // nested in ternary — LHS walker stops at `:` boundary
  <Box>{(a ? b : c && <Label text="x"/>)}</Box>   // LHS = c

  // nested in `||` — LHS walker stops at `||` boundary
  <Box>{a || b && <Label text="x"/>}</Box>        // LHS = b

  // bitwise `&` is NOT mistaken for logical
  <Box>{((a & b) > 0 ? <Label text="on"/> : <Label text="off"/>)}</Box>
  ```

  Mirrored across all four code layers: shared scanner adds the `&&` trigger
  in `FindBareJsxRanges` and the LHS walker; SG `CSharpEmitter` and HMR
  `HmrCSharpEmitter` emit the ternary desugar (HMR via a new reflection
  delegate `FindLhsStartFunc` plumbed through `UitkxHmrCompiler`); the IDE
  `VirtualDocumentGenerator` rewrites the same shape to a typed-null ternary
  placeholder so Roslyn does not show a permanent CS0019 squiggle on the
  `&&` line.

  When the LHS walker fails on degenerate input (e.g. `{ && <X/>}`) the
  splicer emits a single `#error UITKX0026: Could not desugar \`&&\` JSX
  expression. Use \`cond ? <Tag/> : null\` instead.` directive instead of
  cascading into raw-JSX compile errors.

  Setup-code and directive-body `&&` JSX positions (e.g. `var node = cond &&
  <Tag/>;` inside a component setup block) remain unsupported and are
  tracked in `Plans~/TECH_DEBT_V2.md` item 15. The workaround is identical
  to before: rewrite as an explicit ternary `var node = cond ? <Tag/> :
  null;`.

### Fixed

- **Source generator and HMR emitter now inject `using UnityEngine;` into
  the generated component compilation unit.** Six emit sites covered the
  namespace block, the partial-class body, and the function-component
  overload across both pipelines (three in SG `CSharpEmitter`, three in HMR
  `HmrCSharpEmitter`). The IDE virtual document already pulled
  `UnityEngine` into scope via its Roslyn workspace, so user code
  referencing types like `Texture2D`, `Color`, `Vector2`, `Mathf`, etc.
  without an explicit `@using UnityEngine` directive compiled green in the
  editor but red at build/HMR time. Both pipelines now see the same surface
  area and the editor-vs-build asymmetry on `UnityEngine.*` symbols is
  gone.

### Tests

- 8 new regression tests in `JsxInExpressionTests` cover the `&&` desugar:
  simple bool, null comparison, parenthesised LHS, method-call LHS,
  nested-in-`?:`, nested-in-`||`, bitwise-`&` non-trigger, and the
  UITKX0026 diagnostic path.
- 3 new tests in `UnityEngineImportTests` cover the namespace-scope,
  class-scope, and function-component-overload `using UnityEngine;`
  injection sites.
- **1198/1198 SG** passing. LSP server build clean.

## [0.5.4] - 2026-05-08

### Changed

- **Breaking: User components now reject any attribute that isn't a declared
  parameter (or `key`/`ref`).** Previously the schema treated all 60 BaseProps
  members — `style`, `name`, `className`, `onClick`, `extraProps`,
  `enabledInHierarchy`, etc. — as universal across every tag, so a typo or
  stale attribute on a user component (`<AppButton style={x}/>` when
  `AppButton` doesn't declare a `style` parameter) silently produced
  `Style = x` against the generated `AppButtonProps` class and exploded at C#
  compile time as **CS0117** with no useful pointer back to the `.uitkx`
  source. The schema is now split into two semantic groups:

  - **`structuralAttributes`** — just `key` and `ref`. These apply everywhere
    because `key` is a VirtualNode reconciliation slot (lives on the node, not
    on Props) and `ref` is routed to the unique `Hooks.MutableRef<T>`
    parameter on the target component via `forwardRef`-style semantics.
  - **`intrinsicElementAttributes`** — the 58 BaseProps members. These only
    apply to built-in `V.*` tags that actually back a `VisualElement`. User
    components do **not** inherit them.

  Unknown attributes on user components now raise **UITKX0109** at **Error**
  severity (was Warning) with an actionable hint — `did you mean 'X'?` for
  close matches, otherwise
  `Available on '<Comp>': a, b, c. Add a parameter to the component or remove
  the attribute.` The bad attribute is also **skipped in the generated C#**
  so a single UITKX0109 doesn't cascade into CS0117/CS0246 against the
  synthesized props class.

  **Migration:** if you were forwarding `style`/`name`/`className`/etc.
  through a user component, declare them as explicit parameters and forward
  them yourself in the body — e.g.
  `component AppButton(IStyle? style = null) { return (<Button style={style}/>); }`.
  Built-ins are unchanged: `<Button style={...} extraProps={...}/>` still
  works exactly as before.

### Fixed

- **Editor and build-time diagnostics paths now share the same
  element-class-aware attribute map.** The LSP analyzer
  (`DiagnosticsPublisher.BuildKnownAttributes`) and the source generator
  (`CSharpEmitter.EmitFuncComponent`) previously diverged: the LSP raised
  UITKX0109 (Error) for the user-component path while the source generator
  raised nothing, leaving the IDE red but the build only yellow (or worse,
  silent until the C# compiler exploded with CS0117). Both now query the
  same split schema and produce identical diagnostics.
- **`PropsResolver.GetPublicPropertyNamesByQualifiedName` gained a same-pass
  peer fallback** so cross-file user-component attribute validation works on
  a clean build — before the generated `*Props` symbol exists as compiled
  metadata, the resolver now consults `PeerComponentInfo.FunctionParams`
  collected during the same generator pass.

### Tests

- 9 new regression tests (5 analyzer-level in `DiagnosticsAnalyzerTests`,
  4 source-generator end-to-end in `EmitterTests`) covering: style rejected
  on user component, `key`/`ref` always exempt, `extraProps` rejected on
  user component, declared attributes pass through cleanly, no-params
  components reject every non-structural attribute, and built-ins remain
  unaffected. **1187 / 1187 SG tests passing.**

## [0.5.3] - 2026-05-08

### Changed

- **Breaking: `@(expr)` markup-embed syntax has been removed.** The canonical
  and only embed form for arbitrary C# expressions inside markup is now
  `{expr}` — matching JSX/Babel and React. The `@` prefix continues to mark
  directives only: `@if`, `@else`, `@for`, `@foreach`, `@while`, `@switch`,
  `@case`, `@default`, `@using`, `@namespace`, `@component`, `@props`, `@key`,
  `@inject`, `@uss`. Files containing legacy `@(expr)` in markup now raise a
  hard parse error **UITKX0306** (`@(expr) is no longer supported — use
  {expr}`). Migration is mechanical: every `@(` becomes `{` and the matching
  `)` becomes `}`. The unification removes one of two competing embed forms
  end-to-end across the parser, formatter, analyzer, IntelliSense cursor
  context, virtual-document generator, HMR emitter, source generator,
  TextMate grammar, all 12 shipped sample files, and the test suite (every
  fixture inverted; 3 new UITKX0306 diagnostic tests added).

### Fixed

- **Source generator: pool-rent declarations no longer end up inside line
  comments.** The naive backward-scan that picked the splice point for
  `var __p_N = __Rent<TProps>();` statements stopped at the first `;` or `}`
  it encountered — including `}` characters living inside `// see {catBadge}`
  line comments. The compiler then read the rent statements as part of the
  comment text, leaving `__p_N` references downstream tripping CS0103
  (`The name '__p_12' does not exist in the current context`). Replaced all
  four sites (two in `CSharpEmitter`, two in `HmrCSharpEmitter` for HMR
  parity) with a shared `FindLastTopLevelStatementBoundary` lexer-aware
  forward scanner. The scanner correctly skips `//` line comments,
  `/* */` block comments, regular `"..."`, interpolated `$"..."` (with
  `{{`/`}}` escape and brace-depth tracking inside interpolation holes),
  verbatim `@"..."`, dollar-verbatim `$@"..."`, and `'...'` char literals —
  only `;` or `}` outside any of these counts as a statement boundary.
  Pre-Phase-2 the bug was masked because the comment text contained
  `@(catBadge)` (no `}` to trip on); the Phase 2 unification of `@(...)` →
  `{...}` exposed the latent flaw.
- **Function-style component discriminator now accepts a bare `{` opener.**
  `LooksLikeMarkupRoot` (used to distinguish setup code from a return-value
  markup expression) recognised `(` and `<` only, missing the new `{`-opened
  embed form introduced by Phase 2. Files using a top-level `{expr}` return
  were misclassified as setup code, producing CS-cascade errors. Acceptance
  set expanded to `(`, `<`, and `{`.
- **`AstCursorContext` block-2a recognises `{` as a markup-embed opener.**
  IntelliSense post-`{` cursor classification was anchored on the legacy
  `@(` opener; with Phase 2 the cursor inside `<Tag attr={cursor}/>` and
  `<Box>{cursor}</Box>` now resolves under the correct C#-expression scope
  rather than as an unrelated brace context.

### Tests

- 1178/1178 source-generator tests passing after the Phase 2 cut and the
  splice-helper rewrite.
- All 12 sample `.uitkx` files converted in-place with byte-safe UTF-8
  preservation (no encoding regressions).
- HMR↔SG parity contract tests still green (verifying both emitters share
  the same splice semantics).

## [0.5.2] - 2026-05-08

### Added

- **JSX literals are now allowed in any C# expression position** — matching
  React/Babel semantics. Previously the source generator only recognised JSX
  in three places: top-level markup, component preamble (`var x = <Tag/>;`
  before `return`), and directive bodies (inside `@if`/`@foreach`/etc.). JSX
  inside an inline expression — ternary branches, lambda bodies, attribute
  expressions, `?? <Tag/>`, child `{...}` or `@(...)` — was emitted verbatim
  and rejected by Roslyn. The existing scanner
  (`DirectiveParser.FindBareJsxRanges` + `FindJsxBlockRanges`) is now wired
  into the two remaining emit sites (`EmitExpressionNode` and the
  `CSharpExpressionValue` branch of attribute emission), so all six positions
  splice JSX uniformly. New `SpliceExpressionMarkup` helper in
  `CSharpEmitter.cs` mirrors `SpliceBodyCodeMarkup` 1:1; pool-rent statements
  flow into the shared `_rentBuffer` so the parent emit context hoists them
  above the surrounding expression. Patterns now supported:
  - `<Box>{cond ? <A/> : <B/>}</Box>` — ternary with JSX branches
  - `<Box>{fallback ?? <Default/>}</Box>` — null-coalescing with JSX
  - `<Box icon={active ? <Check/> : <X/>}/>` — JSX in attribute ternary
  - `attr={items.Select(x => <Item key={x.Id}/>)}` — JSX in lambda body
  - `var renderItem = i => <Label text={i}/>;` in preamble (already worked,
    now also works through attribute lambda flows)

  No runtime change — the emitter still produces the same `V.Tag(...)` factory
  calls and pooled `__Rent<TProps>()` shape; the splice runs purely at emit
  time. Compile-time cost is one O(n) scanner pass per expression; for
  expressions without embedded JSX (the common case) the helper returns the
  input unchanged.

### Fixed

- **`Texture2D ? iconName = null` (whitespace before `?`) is no longer
  silently dropped on save.** The `DirectiveParser.TryReadTypeName` tokenizer
  required `?` to immediately follow the type name with no intervening
  whitespace; with whitespace, the trailing `?` was left unconsumed and the
  formatter re-emitted the type without nullability — turning a nullable
  parameter into a non-nullable one across format-on-save cycles. Same
  pathology as the 0.5.x `@else` blank-line bug — formatter re-emit-from-AST
  is lossy when the parser drops tokens. Fix: `TryReadTypeName` now peeks
  past whitespace, consumes a trailing `?`, and canonicalises the captured
  type name as `<base>?` so the formatter re-emits a clean `Texture2D? name`
  regardless of the user's spacing. New regression test
  `ComponentParam_NullableType_WhitespaceBeforeQuestionMark_Preserved` in
  `FormatterSnapshotTests` covers `Action ? onClick`, `Texture2D ? iconName`,
  and `List<int>  ?  items`, asserting both idempotency across three format
  passes and canonical re-emit.

### IDE / HMR parity

- **HMR `HmrCSharpEmitter` mirrors `SpliceExpressionMarkup`** end-to-end.
  Reflection delegates for the two scanner methods are piped through
  `UitkxHmrCompiler` (graceful fallback if an older `Language.dll` lacks the
  newly-public APIs). Hot-reload of components using JSX-in-expression now
  produces identical C# to the source generator.
- **Virtual document generator (IDE Roslyn analysis)** now strips embedded
  JSX literals to typed-`(VirtualNode)null!` stubs when wrapping expressions
  for type-checking. Without this update, files using the new patterns would
  compile cleanly under the source generator but show phantom Roslyn errors
  in the editor on the JSX literals. Surrounding C# stays source-mapped so
  completions and squiggles still work outside the JSX.

### Tests

- 14 new tests, 1178/1178 passing (1164 baseline + 10 SG `JsxInExpressionTests`
  + 1 HMR parity tripwire + 3 VDG `VirtualDocumentTests`). Coverage matrix
  includes ternary-with-jsx (both branches, single branch, `@(...)` form),
  attribute-with-jsx, lambda-with-jsx, generic-LT-not-confused-with-jsx,
  string-with-tag-like-text-not-spliced, null-coalesce-with-jsx,
  no-op-fast-path, and `JsxExpressionValue` non-regression.

### Notes

- `DirectiveParser.FindBareJsxRanges` and `FindJsxBlockRanges` widened from
  `internal` to `public` so SG, HMR, and VDG share the single proven scanner
  implementation. Binary-additive change; no breaking impact on consumers.
- Phase 2 (soft-deprecate `@(expr)` in JSX child position in favour of
  `{expr}` to match React semantics) is planned as a separate follow-up;
  both syntaxes continue to work and emit identical AST in this release.

## [0.5.1] - 2026-05-07

### Fixed

- **Generic `static` methods inside `module { … }` blocks now compile.** The
  HMR trampoline rewriter (`SourceGenerator~/Emitter/ModuleBodyRewriter.cs`),
  introduced in 0.4.19, emitted two pieces of invalid C# on the generic-method
  branch — the bug was inert until a consumer authored a generic method inside
  a `module { … }` body.
  - **CS0119** (`'TProps' is a type, which is not valid in the given context`)
    — `AppendTypeArgs` emitted bare type-parameter names into the synthesized
    `MethodInfo.MakeGenericMethod(...)` call, e.g.
    `MakeGenericMethod(TProps, TResult)`. `MakeGenericMethod` takes
    `params Type[]`, so each name must be wrapped in `typeof(...)`. Fix:
    `AppendTypeArgs` now emits `typeof(TProps), typeof(TResult)`.
  - **CS8625** (`Cannot convert null literal to non-nullable reference type`)
    — the synthesized `MethodInfo` HMR field was emitted as
    `static MethodInfo __hmr_<name>_h<sig> = null;`. The field MUST start
    `null` (the trampoline checks `!= null` to fall through to the body method
    until `UitkxHmrModuleMethodSwapper` fills it via reflection), but consumer
    projects with `<Nullable>enable</Nullable>` or
    `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` failed compilation.
    Fix: emit `= null!;` — runtime value identical, warning suppressed.
- **Non-generic module methods, player builds, and the HMR swapper are
  unaffected** — both fixes are purely emit-side, behind `#if UNITY_EDITOR`,
  and `null!` is a compile-time null-forgiving annotation only.

### Tests

- New regression test
  `Sg_ModuleGenericMethod_GeneratedCodeCompiles_NoCS0119_NoCS8625` in
  `HmrEmitterParityContractTests` actually **compiles** the generated module
  output through Roslyn (with `UNITY_EDITOR` defined and nullable enabled) and
  asserts neither CS0119 nor CS8625 is raised. The pre-existing
  `Sg_ModuleGenericMethod_UsesMethodInfoCache` test only did substring checks
  and could not detect either bug. **1162/1162 SG** passing.

### Notes

- Runtime-only release. IDE extensions (VS Code, VS 2022) unchanged — the
  rewriter pass is SG-only and never runs in the LSP.

## [0.5.0] - 2026-05-06

### Added

- **`<Video>` element** (Pattern A — element adapter). Wraps a pooled
  `VideoPlayer` + `RenderTexture` rented from the new `MediaHost` peer pool and
  feeds the decoded RT into a UI Toolkit `Image` sink via
  `Image.image = renderTexture`. Repaints are driven by
  `VideoPlayer.frameReady` (no polling). An editor-only
  `EditorApplication.QueuePlayerLoopUpdate()` pump advances the player when
  Unity isn't ticking. Declarative props: `Clip`, `Loop`, `Autoplay`, `Muted`,
  `ScaleMode`, `Volume`. Imperative `VideoController` ref:
  `Play`/`Pause`/`Seek`/`StepForward`.
- **`<Audio>` element** (Pattern B — Func-Component). Renders no visible
  content; rents an `AudioSource` from `MediaHost` via `UseEffect` and returns
  it on unmount. Props: `Clip`, `Loop`, `Autoplay`, `Volume`, `Pitch`,
  `SpatialBlend`, optional `AudioMixerGroup`. Imperative `AudioController` ref.
- **`useSfx()` hook** — returns a stable `Action<AudioClip, float>` that calls
  `MediaHost.Instance.SfxSource.PlayOneShot(clip, volumeScale)`. Zero per-call
  allocation, identical delegate reference across renders so it composes
  cleanly inside `UseEffect` dependency lists. Optional `AudioMixerGroup`
  parameter is captured at hook-call time.
- **`MediaHost` peer pool** — `HideAndDontSave` GameObject hosting all
  `VideoPlayer` and `AudioSource` instances plus a stable `SfxSource`. Pool
  rent/return is reference-counted; `RenderTexture`s pooled by
  `(width, height, depth)` tuple. Survives domain reloads via lazy
  resurrection.
- **MediaPlayground demo** — `Samples/Shared/MediaPlaygroundDemoPage.uitkx`
  exercises every media surface end-to-end. Editor window at
  `ReactiveUITK > Demos > Media Playground`; runtime bootstrap
  (`MediaPlaygroundRuntimeBootstrap.cs`) for play-mode testing.

### Fixed

- **`FiberRenderer.Clear()` now runs effect cleanups before dropping the
  tree.** The previous implementation cleared the container and nulled the
  root in a single call, never invoking the depth-first `CommitDeletion` path
  that fires `UseEffect` cleanup callbacks and disposes signals. Any
  `UseEffect`-owned resource (audio sources, timers, signal subscriptions,
  `RenderTexture`s, animation handles) leaked across editor-window close /
  reopen cycles. New `FiberReconciler.UnmountRoot()` walks
  `_root.Current.Child` and runs `CommitDeletion` on each child before the
  root is nulled. `EditorRootRendererUtility.Unmount()` now calls
  `EditorRenderScheduler.Instance.PumpNow()` after `Unmount()` so cleanups
  drain synchronously before the editor window closes. The leak only became
  visible with `<Audio>` (background music kept playing forever) but the same
  code path affected every Func-Component using `UseEffect` cleanup.
- **IDE — `useSfx()` no longer reports `CS0103` in `.uitkx` files.** The LSP
  scaffolds private hook stubs into a virtual document so Roslyn can
  type-check setup code; `useSfx` had been added to the source generator and
  HMR alias regexes when it shipped but was never added to the LSP's stub
  list. Stubs added to both function-style component and hook-document
  scaffolds in `VirtualDocumentGenerator`.

### IDE extensions

- VS Code **1.1.11 → 1.1.12**
- Visual Studio 2022 **1.1.11 → 1.1.12**

---

## [0.4.19] - 2026-05-04

Full HMR support for `module { … }` declarations. The contract for what is and
is not preserved across a hot-reload cycle is now explicit and matches the
conventions used by React Fast Refresh and .NET Hot Reload.

### HMR contract for `module { … }` bodies

| Member kind | Behaviour on save |
|---|---|
| `public const X` | Re-baked into the call sites at compile time; new value visible after the next HMR swap (constants are folded by the C# compiler, so existing already-loaded code keeps the old value until that code is itself re-emitted). |
| `public static readonly X` | **Re-initialised every HMR cycle** — the new initializer expression runs in the HMR-compiled assembly and the result is copied into the project type via `UitkxHmrModuleStaticSwapper`. |
| `public static X` (mutable) | **Preserved** — runtime value carries across HMR cycles. Matches React Fast Refresh, .NET Hot Reload, and JS HMR. Use cases: lazy caches (`_textures`, `_built`), session counters, accumulated state. To reset, exit Play mode (or enable the opt-in auto-reload setting). |
| `public static T Foo(…)` (method) | **Hot-swapped via per-method delegate trampolines** — supports `ref`/`out`/`in`/`params`, default values, generics, and overloads. Behind `#if UNITY_EDITOR`, zero overhead in player builds. |
| Newly-added `static readonly` field | **CLR rude edit** — the project type's metadata is sealed by the runtime and cannot grow new fields. A warning is logged once per session per field; opt into `UITKX_HMR_AutoReloadOnRudeEdit` for an automatic domain reload. |
| Newly-added method | The new method exists only on the HMR-compiled type. Calls from already-loaded (non-HMR'd) code throw `MissingMethodException`. Same workaround as new fields. |
| Instance methods, properties, operators, nested-type members | Emitted verbatim — not hot-reloaded. Edit them and trigger a full domain reload to see changes. |

### Added — HMR for module statics & methods

- **Module `static readonly` field re-init.** New `UitkxHmrModuleStaticSwapper` copies static-readonly field values from the freshly HMR-compiled assembly into matching project types. Fixes the case where editing a `Style`/`Color` module field initializer reported a successful HMR cycle but the rendered UI kept showing the cold-build value until you exited Play mode.
- **Module `static` method hot-swap.** New source-generator pass (`ModuleBodyRewriter`) rewrites every top-level `public static` method inside a `module { … }` body into a trampoline triplet: a public surface method that bounces through an `__hmr_<name>_h<sig>` delegate field to a private `__<name>_body_h<sig>` body method (all `#if UNITY_EDITOR`-gated). After each HMR compile, the new `UitkxHmrModuleMethodSwapper` rebinds every delegate field to the freshly compiled method via `Delegate.CreateDelegate`. Custom delegate types support `ref`/`out`/`in`/`params` (previously impossible with framework `Func<>`/`Action<>`); FNV-1a 32-bit signature hash disambiguates overloads; generic methods use a `MethodInfo` + `ConcurrentDictionary<Type, Delegate>` cache pattern. Trampolines preserve the original method's visibility so `private static` methods using `private` nested types stay valid (no CS0050/CS0051/CS0052/CS0058/CS0059).
- **Rude-edit detection.** When you add a new `static readonly` field to a module mid-session, the CLR can't grow the project type's metadata — `UitkxHmrModuleStaticSwapper` now detects the mismatch and logs a once-per-session warning naming each affected field, the runtime constraint, and the available remediations.
- **Opt-in auto-reload.** New `UitkxHmrController.AutoReloadOnRudeEdit` setting (EditorPref `UITKX_HMR_AutoReloadOnRudeEdit`, default `false`). When enabled and a rude edit lands, schedules `EditorUtility.RequestScriptReload()` via `EditorApplication.delayCall` so the new field/method materialises everywhere with one extra round-trip.
- **`UITKX0150` Info diagnostic.** Emitted when the source generator cannot Roslyn-parse a module body for trampoline rewriting; falls back to verbatim emission so the module still compiles (only per-method HMR for that module is unavailable).

### Fixed — 12 HMR ↔ source-generator parity bugs in `HmrCSharpEmitter`

The HMR pipeline emits C# from a hand-written transpiler that must match the
Roslyn-based source generator's output for any given `.uitkx` input. A round
of cross-checking surfaced 12 long-standing divergences:

- `ref={x}` on function components is now resolved to the props' `Ref<T>`/`MutableRef<T>` slot via the new `FindPropsTypeAndRefSlot` + `FindRefSlotName` helpers, instead of being treated as a literal `Ref` prop assignment (which silently dropped the binding).
- JSX-as-attribute-value (e.g. React-Router `element={<X/>}`) now emits a real nested element via the `JsxExpressionValue` `_sb`-capture path instead of collapsing to `null`.
- Sibling duplicate `key={…}` warnings are now raised at HMR-compile time via `CheckDuplicateKeys` from `EmitChildArgs`, matching SG's `UITKX0104`.
- Sibling top-level Props classes (`RouterFunc` / `RouterFuncProps` at namespace scope) resolve correctly — three resolution paths now mirror the SG's `PropsResolver.TryGetFuncComponentPropsTypeName`.
- `HmrCSharpEmitter.FindPropsType` no longer over-eagerly returns `{Type}.{Type}Props` and now walks all three legitimate Props shapes (sibling top-level, nested same-name, nested differently-named).
- Function-component invocations correctly use `new …Props { … }` (not `BaseProps.__Rent`) — function-component Props derive from `IProps`, not `BaseProps`, and cannot be pooled.
- `Asset<T>("./x")` / `Ast<T>("../x")` relative paths are resolved to absolute Unity-registry keys before HMR emit, so HMR-compiled and SG-compiled code produce identical literal strings (parity with `UitkxAssetRegistry`).
- `UitkxHmrCompiler` adds a silent-drift list for 4 reflection-bound Roslyn methods, a deterministic `PickAllOptionalTailOverload` helper (overload picking is no longer order-sensitive across Roslyn versions), and an explicit `lineOffset:0` on `_uitkxParse`.
- `CheckIfGenuinelyNew` uses fully-qualified type names so two unrelated modules with the same short name no longer fight over the swap slot.
- `CompileHookModuleFile` correctly dispatches `HmrHookEmitter.EmitModules` so module bodies emitted by HMR compile end-to-end (exposed by Bug 1 from 0.4.17).

### Changed

- `UitkxHmrController.ProcessFileChange` extends its success log with `| Module statics re-init: N` and `| Module methods re-init: K` so the editor console makes it obvious which kind of HMR work happened on each save.
- `UitkxHmrModuleStaticSwapper.SwapModuleStatics` returns a richer `ModuleStaticSwapResult { Copied, AddedFieldsDetected }` instead of a bare `int`.

### Tests

- 12 SG ↔ HMR emitter parity contract tests in `HmrEmitterParityContractTests` (5 from the parity-bugs round + 7 for the new module-method trampoline shape: trampoline-triplet shape, `ref` parameter custom-delegate, distinct overload hashes, generic-method `MethodInfo`-cache, non-method members emitted verbatim, instance-method untouched, default-parameter behaviour). 1142/1142 tests passing.

## [0.4.18] - 2026-05-03

### Fixed — HMR `CS0426` on function components with sibling top-level Props

A consumer hit `[HMR] Compilation failed for AppRoot... CS0426: The type name
'RouterFuncProps' does not exist in the type 'RouterFunc'` immediately after
shipping 0.4.17. Root cause was a long-standing convention divergence between
the source generator and the HMR compiler that only surfaced once HMR could
actually compile module/style/hook files end-to-end (Bugs 1 & 2 from 0.4.17).

#### The bug

Function-component Props classes are emitted in three legitimate shapes:

1. **Sibling top-level** — `RouterFunc` and `RouterFuncProps` both at namespace
   scope, neither nested. Used by `ReactiveUITK.Router`.
2. **Nested same-name** — `CompFunc.CompFuncProps` (the source generator's own
   default emission shape).
3. **Nested differently-named** — `ValuesBarFunc.Props` (legacy hand-written
   pattern still in use).

The source generator's `PropsResolver.TryGetFuncComponentPropsTypeName` already
walked all three. HMR's `HmrCSharpEmitter.FindPropsType` only walked nested
types and shipped `{Type}.{Type}Props` unconditionally — so any component using
shape (1) compiled fine through source-gen but failed CS0426 through HMR.

#### The fix

`FindPropsType` now mirrors `PropsResolver` lookup order verbatim:

1. Sibling top-level `{typeName}Props` in same namespace as the located
   component type → returns `"global::" + siblingFullName` (typed Props).
2. Nested `{typeName}.{typeName}Props` implementing `IProps` → returns
   `"{typeName}.{siblingName}"`.
3. Any nested `IProps` (legacy fallback) → returns `"{typeName}.{nested.Name}"`.
4. Convention fallback string (preserves prior behavior for genuinely missing
   types so the resulting CS error points at a recognizable location).

#### Tests

Two complementary layers, both running on every push, every PR, and before
every package publish via the existing GitHub Actions workflows:

- **SG-side parity test** (`FuncComponent_WithSiblingTopLevelPropsClass_EmitsTypedVFunc`)
  — drives the generator with the real `RouterFunc` / `RouterFuncProps` shape
  and asserts it emits `V.Func<global::Ns.RouterFuncProps>` rather than the
  broken nested form. Pins the contract HMR mirrors.
- **HMR algorithm contract tests** (`HmrFindPropsTypeContractTests`) — five
  cases exercising the algorithm against in-memory Roslyn-compiled assemblies
  (sibling / nested-named / nested-legacy / sibling-wins-priority / negative
  fallback). Mirrors `FindPropsType` verbatim because the Editor assembly
  (`UnityEditor` deps) cannot be loaded by the standalone .NET test runner.

**1070/1070 SG** passing.

VS Code **1.1.10 → 1.1.11** · VS 2022 **1.1.10 → 1.1.11** ride the same release.

---

## [0.4.17] - 2026-05-03

### Fixed — HMR overload-resolution bug + asset-path rewrite gap in `module` / `hook` bodies

Two related production-grade fixes converging on `.style.uitkx` / `.hooks.uitkx`
files. Both were silent until they met in a real consumer project (the
`AppRoot.style.uitkx` / `Asset<Texture2D>("../Resources/background-01.png")`
case), so this release also adds CI coverage so neither can recur.

#### Bug 1 — HMR `ArgumentException` on every `.uitkx` save

`UitkxHmrCompiler.InvokeWithDefaults` had two `params object[]` overloads:

- `InvokeWithDefaults(MethodInfo, object target, params object[])` (instance/static aware)
- `InvokeWithDefaults(MethodInfo, params object[])` (static-only, with API-drift padding)

C# overload resolution prefers `string → object target` over `string →
params object[]`, so calls like `InvokeWithDefaults(_directiveParse, source,
uitkxPath, diagList, true)` silently bound to the **first** overload —
`source` became the (ignored) target receiver and every subsequent argument
shifted left by one position, dropping a `List<ParseDiagnostic>` into
`DirectiveParser.Parse(string source, string filePath, ...)`'s `filePath`
slot. Result on every `.uitkx` save during play mode:

```
ArgumentException: Object of type
'System.Collections.Generic.List`1[ReactiveUITK.Language.Parser.ParseDiagnostic]'
cannot be converted to type 'System.String'.
```

The two overloads were collapsed into a single canonical signature
`InvokeWithDefaults(MethodInfo method, object target, params object[] args)`
where `target` is **mandatory** (not defaulted) — this makes the entire
class of "string arg accidentally captured as receiver" bug structurally
impossible to recur. All eleven call sites updated to pass an explicit
`null` (static methods) or the actual receiver (instance methods like
`Compilation.Emit(stream)`).

#### Bug 2 — `Asset<T>("./x")` / `Asset<T>("../x")` not rewritten in `module` / `hook` bodies

The runtime `UitkxAssetRegistry` is a flat dictionary keyed by **resolved**
Unity asset paths (e.g. `Assets/Resources/background-01.png`). The compile-
time emitters are responsible for rewriting every `Asset<T>("./relative")`
literal in the generated C# from the relative form to that resolved key,
so that runtime `Get<T>(string key)` finds the entry.

That rewrite (`ResolveAssetPaths`) was applied to component setup code,
JSX attribute expressions, and directive (`@if` / `@foreach` / `@switch`)
bodies — but **not** to `module { ... }` or `hook { ... }` bodies. So:

```uitkx
module AppRoot {
  public static readonly Style Root = new Style {
    BackgroundImage = Asset<Texture2D>("../Resources/background-01.png"),
  };
}
```

…shipped the literal `"../Resources/background-01.png"` to runtime, while
the editor-side `UitkxAssetRegistrySync` (which scans the same source
independently) wrote the entry under the resolved key
`Assets/Resources/background-01.png`. The two halves no longer agreed,
so `Asset<T>("…")` returned `null` with a warning:

```
[UITKX] Asset not found in registry: "../Resources/background-01.png"
```

Both emitter pipelines were widened to apply `ResolveAssetPaths` to
module/hook bodies:

- **Source generator** — `ModuleEmitter.Emit` and `HookEmitter.EmitSingleHook`
  now call the same shared `EmitContext.ResolveAssetPaths` that powers
  setup code and JSX attributes. The helper was promoted from a private
  instance method to an `internal static` so all three emitters share a
  single implementation (no semantic drift).
- **HMR** — `HmrHookEmitter.EmitModules` and `HmrHookEmitter.EmitSingleHookBody`
  now route bodies through `HmrCSharpEmitter.ResolveAssetPaths` (visibility
  promoted from `private` to `internal`). HMR-recompiled assemblies now
  produce literal-identical asset strings to source-generated ones.

#### Why these two are related

`.style.uitkx` and `.hooks.uitkx` files are the convergence point. Bug 1
prevented HMR from ever compiling those files (Parse blew up). Bug 2 meant
that even when source-gen ran cleanly, the registry lookup still missed
because the literal stayed unrewritten. Both bugs needed to be fixed for
`Asset<T>` inside `module` / `hook` blocks to work end-to-end.

#### Tests

Four new regression tests in `SourceGenerator~/Tests/EmitterTests.cs`:

- `Module_AssetCall_RelativePath_IsRewritten` — `./bg.png` → `Assets/UI/bg.png`
- `Module_AssetCall_DotDotPath_IsRewritten` — the exact failing case
  (`../Resources/bg.png` → `Assets/Resources/bg.png`)
- `Module_AssetCall_AbsolutePath_Unchanged` — negative test, no double-prefix
- `Hook_AssetCall_RelativePath_IsRewritten` — parity for `HookEmitter`

These run on every push, every PR, and before every package publish via
`.github/workflows/test.yml` and `.github/workflows/publish.yml`, so the
bug class cannot ship again. **1064/1064 SG** passing.

#### Files touched

- `Editor/HMR/UitkxHmrCompiler.cs` — overload collapse + 11 call-site updates
- `Editor/HMR/HmrCSharpEmitter.cs` — `ResolveAssetPaths` visibility
- `Editor/HMR/HmrHookEmitter.cs` — apply asset-path rewrite to hook + module bodies
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `ResolveAssetPaths` (and helpers
  `ResolveRelativePath` / `GetUitkxAssetDir` / `GetProjectRoot`) promoted to
  pure statics taking `(filePath, diagnostics)` parameters
- `SourceGenerator~/Emitter/HookEmitter.cs` — wire asset-path rewrite after
  hook-alias substitution
- `SourceGenerator~/Emitter/ModuleEmitter.cs` — wire asset-path rewrite for
  every module body
- `SourceGenerator~/Tests/EmitterTests.cs` — 4 new regression tests

VS Code **1.1.9 → 1.1.10** · VS 2022 **1.1.9 → 1.1.10** ride the same release.

## [0.4.16] - 2026-05-03

### Fixed — HMR `TargetParameterCountException` + production-grade hardening

A reflection signature drift between the editor-only HMR compiler and the
loaded `ReactiveUITK.Language.dll` (`UitkxParser.Parse` gained an optional
`lineOffset` parameter in 0.4.7) caused `TargetParameterCountException` to
fire on every `.uitkx` save during play mode, swallowed silently into a
`Debug.LogWarning` and an infinite retry storm. This release fixes the
immediate symptom and adds two layers of defense so the same class of
plumbing failure cannot recur silently.

#### Layer 1 — immediate fix

`UitkxHmrCompiler` now passes the trailing `lineOffset = 0` argument to
both `_uitkxParse.Invoke` sites in `Compile()` and the `parseMarkup`
delegate. Hot reload of components, hooks, and modules works again
during play mode.

#### Layer 2 — defensive `InvokeWithDefaults` helper

All six reflective invocations into the language library
(`DirectiveParser.Parse`, `UitkxParser.Parse`, `CanonicalLowering.LowerToRenderRoots`)
now route through a new `InvokeWithDefaults(MethodInfo, params object[])`
helper that pads short argument arrays with each parameter's compile-time
`DefaultValue`. When padding is actually triggered, a one-time
`Debug.LogWarning` per `MethodInfo` surfaces silent API drift the next
time it happens — instead of failing, HMR keeps working with sensible
defaults and tells you to update the call site.

#### Layer 3 — infrastructure-error classifier + self-disable

`HmrCompileResult` gained a `bool IsInfrastructureError` flag. The
compiler's catch blocks classify the inner exception type
(`TargetParameterCountException | MissingMethodException |
MissingFieldException | TypeLoadException | ReflectionTypeLoadException |
BadImageFormatException`) and set the flag. `UitkxHmrController` checks
the flag before its existing CS0103 retry cascade: on the first
infrastructure failure it emits a single `Debug.LogError` with
actionable text, then calls `Stop()` (the only safe disable path —
unhooks events, stops the file watcher, unlocks the assembly-reload
suppressor, restores `Application.runInBackground`, clears retry
queues). The user can re-`Start` from the HMR window after rebuilding
the language library; a `_loggedInfrastructureFailure` gate is reset on
`Start()` so future sessions get a fresh shot.

User-authored compile errors (`CS0103`, `CS1xxx`, syntax errors) are
still returned as strings on `result.Error` and follow the existing
warn + retry cascade — only true infrastructure plumbing failures
self-disable.

Files changed: [Editor/HMR/UitkxHmrCompiler.cs](Editor/HMR/UitkxHmrCompiler.cs),
[Editor/HMR/UitkxHmrController.cs](Editor/HMR/UitkxHmrController.cs).
Source generator, runtime, build, and IDE extension surfaces are
untouched. All 1060 source-generator tests pass.

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
