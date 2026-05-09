# Changelog

## [1.2.1] - 2026-05-09
- Feature: JSX `&&` short-circuit now splices cleanly in markup expression positions. The React idiom `{cond && <Tag/>}` is impossible to emit verbatim because C# `&&` is bool-only and `bool && VirtualNode` is CS0019. The splicer now detects a trailing `&&` operator at the end of the prefix preceding any JSX literal in `{expr}` or `attr={expr}` positions and rewrites the expression to a ternary form `((cond) ? V.Tag(...) : (global::ReactiveUITK.Core.VirtualNode?)null)` reusing the already-tested Phase 1 ternary path. The `null` fallback is dropped at render time by `__C(params object[])` which filters nulls. New shared `DirectiveParser.FindLhsStartForLogicalAnd` precedence-aware forward walker locates where the LHS of the `&&` begins inside the surrounding expression — single forward pass over `expr[prev..ampStart]` with per-paren-depth boundary tracking, lexer-aware string/comment skipping, and recognition of `?`, `:`, `??`, `||`, `,`, `;` as boundary tokens at the same paren depth as the `&&`. Mirrored across all four code layers: shared scanner adds the `&&` trigger in `FindBareJsxRanges` and the LHS walker; SG `CSharpEmitter.SpliceExpressionMarkup` and HMR `HmrCSharpEmitter.SpliceExpressionMarkup` emit the ternary desugar (HMR via a third reflection delegate `FindLhsStartFunc` plumbed through `UitkxHmrCompiler`); IDE `VirtualDocumentGenerator.EmitMappedExpressionStrippingJsx` rewrites the same shape to a typed-null ternary placeholder so Roslyn doesn't show a permanent CS0019 squiggle on the `&&` line. Walker degenerate-input failure (e.g. `{ && <X/>}`) emits `#error UITKX0026` with an actionable message instead of cascading into raw-JSX compile errors. Setup-code and directive-body `&&` JSX positions remain unsupported and tracked in TECH_DEBT_V2 item 15. 8 new regression tests in `JsxInExpressionTests` cover simple bool, null comparison, parenthesised LHS, method-call LHS, nested-in-`?:`, nested-in-`||`, bitwise-`&` non-trigger, and the UITKX0026 diagnostic path. 1198/1198 SG tests passing; LSP server build clean.
- Fix: source generator and HMR emitter now inject `using UnityEngine;` into the generated component compilation unit. Six emit sites (three in SG `CSharpEmitter`, three in HMR `HmrCSharpEmitter`) covered the namespace block, the partial-class body, and the function-component overload. The IDE virtual document already pulled `UnityEngine` into scope via its Roslyn workspace, so user code referencing types like `Texture2D`, `Color`, `Vector2`, `Mathf` etc. without an explicit `@using UnityEngine` directive compiled green in the editor but red at build/HMR time. Now both pipelines see the same surface area and the editor-vs-build asymmetry on `UnityEngine.*` symbols is gone. 3 regression tests added covering the namespace-scope, class-scope, and function-component-overload positions.

## [1.2.0] - 2026-05-08
- Breaking: User components now reject any attribute that isn't a declared parameter (or `key`/`ref`). Previously the schema treated all 60 BaseProps members — `style`, `name`, `className`, `onClick`, `extraProps`, `enabledInHierarchy`, etc. — as universal across every tag, so a typo or stale attribute on a user component (`<AppButton style={x}/>` when `AppButton` doesn't declare a `style` parameter) silently produced `Style = x` against the generated `AppButtonProps` class and exploded at C# compile time as CS0117 with no useful pointer back to the .uitkx source. The schema is now split into `structuralAttributes` (just `key` and `ref` — apply everywhere because `key` is a VirtualNode reconciliation slot and `ref` is routed to the unique `Hooks.MutableRef<T>` parameter via `forwardRef`-style semantics) and `intrinsicElementAttributes` (the 58 BaseProps members — only valid on built-in V.* tags that actually back a `VisualElement`). User components no longer inherit the intrinsic set: every attribute you pass must either be in the function-style `(...)` parameter list or be `key`/`ref`. Unknown attributes raise UITKX0109 at Error severity (was Warning) with an actionable hint — `did you mean 'X'?` for close matches, otherwise `Available on '<Comp>': a, b, c. Add a parameter to the component or remove the attribute.` The unknown attribute is also skipped in the generated C# so a single UITKX0109 doesn't cascade into CS0117/CS0246 against the synthesized props class. Editor and build-time error paths (LSP DiagnosticsPublisher.BuildKnownAttributes, source-generator EmitFuncComponent) now share the same element-class-aware attribute map, eliminating the previous editor-shows-red but build-shows-yellow asymmetry. Migration: if you were forwarding `style`/`name`/`className`/etc. through a user component, declare them as explicit parameters (`component AppButton(IStyle? style = null) { ... }`) and forward them yourself in the body. Built-ins are unchanged — `<Button style={...} extraProps={...}/>` still works exactly as before. PropsResolver.GetPublicPropertyNamesByQualifiedName now also has a same-pass peer fallback so cross-file user-component validation works on a clean build before the generated `*Props` symbol exists in compiled metadata. 9 new regression tests added (5 analyzer-level, 4 SG end-to-end) — 1187/1187 SG tests passing. HoverHandler now distinguishes structural-vs-intrinsic in the "Common attributes" section for built-in tags.

## [1.1.15] - 2026-05-08
- Breaking: the `@(expr)` markup-embed syntax has been removed. The canonical and only embed form for arbitrary C# expressions inside markup is now `{expr}` (matching JSX/Babel). The `@` prefix survives only as the directive marker (`@if`, `@else`, `@for`, `@foreach`, `@while`, `@switch`, `@case`, `@default`, `@using`, `@namespace`, `@component`, `@props`, `@key`, `@inject`, `@uss`). Authors who still have legacy `@(expr)` in markup will see a hard parser error UITKX0306 (`@(expr) is no longer supported — use {expr}`). Migration is mechanical: replace every `@(` with `{` and every matching `)` with `}`. The unification removes one of two competing embed forms across the parser, formatter, analyzer, IntelliSense, virtual-document generator, HMR emitter, source generator, TextMate grammar, and all 12 shipped sample files. xmldoc and inline comments updated end-to-end.
- Fix: TextMate grammar failed to load entirely because the JSON file had a UTF-8 BOM (`EF BB BF`) prepended during the Phase 2 edit pass. `vscode-textmate`'s `parseRawGrammar()` rejects the BOM with `Unexpected token "", "{ "$sc..." is not valid JSON`, killing the grammar and forcing every TextMate-scoped token to fall back to plain text. Symptoms: keywords (`public`, `static`, `readonly`, `var`), property names (`FlexGrow`, `BackgroundImage`, `Width`), operators (`=`, `,`, `;`), and braces appeared white inside module/hook bodies and component setup blocks. LSP semantic tokens (markup tags, AST-classified types) still rendered, masking the breadth of the regression. Both grammar copies (`ide-extensions~/grammar/uitkx.tmLanguage.json` and the deployed `ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json`) are now BOM-free UTF-8 and parse cleanly (37 repository rules loaded). Mojibake'd em dashes in comment fields restored to `—`.
- Fix: source generator and HMR emitter no longer hide pool-rent declarations inside line comments. The naive backward-scan that picked the insertion point for `var __p_N = __Rent<TProps>();` statements stopped at the first `;` or `}` it saw — including `}` characters that lived inside `// see {catBadge}`-style line comments. The result was C# emitted as `// ... {catBadge}var __p_12 = __Rent<...>();` (compiler reads it as part of the comment), then `__p_12` referenced below tripped CS0103 (`The name '__p_12' does not exist in the current context`). Replaced all four sites (two in `CSharpEmitter`, two in `HmrCSharpEmitter`) with a single shared `FindLastTopLevelStatementBoundary` lexer-aware forward scanner that correctly skips line comments, block comments, regular and verbatim and interpolated strings (with `{{`/`}}` escape handling and brace-depth tracking inside interpolation holes), and char literals. Pre-Phase-2 the bug was masked by `@(catBadge)` in the comment text (no `}` to trip on); Phase 2 unification of `@(...)` → `{...}` exposed the latent flaw. 1178/1178 SG tests still pass.
- Fix: `LooksLikeMarkupRoot` (function-style component setup-vs-return discriminator) now accepts a bare `{` opener. With Phase 2, the leading character of an inline markup expression in a return position is `{` (formerly `(` for `@(...)`); the discriminator continued to recognise only the legacy openers and misclassified the entire return body as setup code, producing CS-level cascade errors. Expanded acceptance set covers `(`, `<`, and `{`.
- Fix: `AstCursorContext` IntelliSense block-2a (post-`{` cursor classification) now correctly recognises `{` as the opening of a markup-embed expression rather than as an unrelated brace, so completions inside `<Tag attr={cursor}/>` and `<Box>{cursor}</Box>` resolve under the right scope (C# expression, not markup attribute key).

## [1.1.14] - 2026-05-08
- Feature: JSX literals are now allowed in any C# expression position — matching React/Babel semantics. Patterns that previously emitted raw JSX and tripped Roslyn now splice cleanly to V.Tag(...) factory calls: ternary branches ({cond ? <A/> : <B/>}), null-coalescing ({fallback ?? <Default/>}), JSX in attribute expressions (icon={active ? <Check/> : <X/>}), JSX inside lambda bodies (items.Select(x => <Item key={x.Id}/>)), and the same patterns through @(...) child syntax. The scanner driving this (DirectiveParser.FindBareJsxRanges + FindJsxBlockRanges) was already proven on component preamble and directive bodies; it is now wired into the two remaining emit sites in CSharpEmitter (EmitExpressionNode and the CSharpExpressionValue branch of AttrVal) plus mirrored in HmrCSharpEmitter for hot reload and in VirtualDocumentGenerator so the IDE no longer shows phantom Roslyn errors on files that compile cleanly. Embedded JSX is replaced with a typed-null stub in the virtual document while surrounding C# stays source-mapped for completions and squiggles. Zero runtime impact (same V.Tag/__Rent shape); compile-time cost is a single O(n) scanner pass per expression and returns unchanged when no JSX is present. Backed by 14 new tests across SG, HMR parity, and VDG layers — 1178/1178 passing.
- Fix: nullable component-parameter types written with whitespace before the question mark (e.g. 'Texture2D ? iconName = null', 'Action ? onClick = null') no longer have the '?' silently dropped on save. DirectiveParser.TryReadTypeName required the '?' to immediately follow the type name with no whitespace; with whitespace, the trailing '?' was left unconsumed and the formatter re-emitted the parameter as non-nullable. Same root pathology as the recent @else blank-line bug — formatter re-emit-from-AST is lossy when the parser drops tokens. Fix peeks past whitespace, consumes the '?', and canonicalises the captured type as '<base>?' so the formatter re-emits a clean 'Texture2D? name' regardless of input spacing. Locked down by a new FormatterSnapshotTests case covering Action, Texture2D, and List<int> nullable params with varied whitespace, asserting both idempotency across three format passes and canonical re-emit.

## [1.1.13] - 2026-05-07
- Fix: format-on-save no longer adds a phantom blank line on every pass between '} @else {' (or any directive opener) and a bare-C# body line. AstFormatter.FormatDirectiveBody fallback path passed the raw body code starting with a newline to EmitSetupCodeNormalized, which emitted it as a blank line on top of the newline already written by the directive opener. The parsed-body path already trimmed leading newlines for the same reason; the fallback (no-JSX body) is now symmetric. New regression test asserts three consecutive format passes produce identical output and that no blank line appears between the opener brace and the first body line.

## [1.1.12] - 2026-05-06
- Feature: New <Video> and <Audio> elements plus useSfx() hook for one-shot SFX. <Video> wraps a pooled VideoPlayer + RenderTexture under a shared MediaHost peer (HideAndDontSave GameObject), drives an Image sink via VideoPlayer.frameReady, and exposes a VideoController ref for imperative Play/Pause/Seek. <Audio> renders no visible content and rents an AudioSource from the same pool with declarative Clip/Loop/Volume/Autoplay props plus an AudioController ref. useSfx() returns a stable Action<AudioClip,float> that plays one-shots on MediaHost.Instance.SfxSource without per-render allocations. New MediaPlayground demo (editor window + runtime bootstrap) exercises every surface end-to-end. IDE virtual-document generator now scaffolds useSfx() in both function-style component and hook-document templates so Roslyn no longer reports CS0103 in the editor.
- Fix: FiberRenderer.Clear() now runs effect cleanups before dropping the tree. The previous implementation cleared the container and nulled the root in one shot, never invoking the depth-first CommitDeletion path that fires UseEffect cleanup callbacks and disposes signals. Any UseEffect-owned resource (audio sources, timers, signal subscriptions, RenderTextures, etc.) leaked across editor-window close/reopen cycles. New FiberReconciler.UnmountRoot() walks _root.Current.Child and runs CommitDeletion on each, then nulls the root; FiberRenderer.Clear() calls it before the container clear. The leak only became visible with the new <Audio> element (background music kept playing forever after closing the demo window) but the root cause affected every Func-Component using UseEffect cleanup.

## [1.1.11] - 2026-05-03
- Fix: HMR no longer fails compilation with CS0426: The type name 'RouterFuncProps' does not exist in the type 'RouterFunc' (or any function-component whose Props class is declared as a sibling at namespace scope rather than nested). HMR's FindPropsType previously only walked nested types and shipped {Type}.{Type}Props unconditionally — diverging from the source generator's PropsResolver.TryGetFuncComponentPropsTypeName which already handled all three resolution paths. The HMR resolver now mirrors source-gen: (1) sibling top-level {Type}Props in the same namespace (the RouterFunc/RouterFuncProps shape), (2) nested {Type}.{Type}Props, (3) any nested IProps (legacy ValuesBarFunc.Props), with a final convention fallback. Locked down by 6 new regression tests (1 SG parity + 5 in-memory Roslyn contract tests mirroring the HMR algorithm). 1070/1070 SG passing.

## [1.1.10] - 2026-05-03
- Fix: HMR no longer fails with `ArgumentException: 'List<ParseDiagnostic>' cannot be converted to 'System.String'` on every `.uitkx` save. The compiler's reflective `InvokeWithDefaults` had two competing `params object[]` overloads; C# overload resolution silently bound `string` arguments into the receiver slot of the wrong overload, shifting every subsequent argument by one position and dropping a `List<ParseDiagnostic>` into `Parse(...)`'s `filePath` slot. The two overloads were collapsed into a single canonical `(MethodInfo method, object target, params object[] args)` signature so the bug class is structurally impossible to recur. Hot reload of components, hooks, and module/style files works again.
- Fix: `Asset<T>("./x.png")` and `Asset<T>("../x.png")` now resolve correctly inside `module { ... }` and `hook { ... }` bodies (e.g. `AppRoot.style.uitkx`). Both the source generator's `ModuleEmitter`/`HookEmitter` and HMR's `HmrHookEmitter` previously emitted module/hook bodies verbatim, leaving relative path literals unrewritten — the runtime `UitkxAssetRegistry.Get<T>` then missed because the registry indexes by resolved Unity asset paths (e.g. `Assets/Resources/background-01.png`). The path-rewrite pipeline that already covered component setup code, JSX attributes, and directive bodies now also runs on module/hook bodies in both source-gen and HMR. Backed by 4 new regression tests in `EmitterTests` (1064/1064 SG passing).

## [1.1.9] - 2026-05-03
- Fix: `Asset<T>(...)` and `Ast<T>(...)` no longer report CS0103 (`The name Asset does not exist in the current context`) inside component setup blocks, hook bodies, and module/style initializers (e.g. `AppRoot.style.uitkx`). The IDE virtual document now injects `using static ReactiveUITK.AssetHelpers;` so Roslyn binds the helpers exactly the way the runtime emitter does.

## [1.1.8] - 2026-05-03
- Feature: schema entries for the new router primitives — <Outlet/>, <Routes>, <NavLink>, <Navigate> — with full attribute metadata. Autocomplete and inline docs work in VS Code, Rider, and Visual Studio without per-IDE changes (single uitkx-schema.json source).
- Feature: <Router> schema gained 'basename' attribute; <Route> schema gained 'index' and 'caseSensitive' attributes. Layout-route description on <Route> now mentions Element + child Routes co-existence.

## [1.1.7] - 2026-05-01
- Feature: autocomplete for IStyle 9-slice + clip-box + text-generator + editor-text-rendering enums (unitySliceType, unityOverflowClipBox, unityTextGenerator, unityEditorTextRenderingMode)

## [1.1.6] - 2026-04-28
- Perf: source generator now hoists `style={new Style{...}}` literals to class-level static fields, eliminating per-render Style allocations and diff-walks for static styles. Handles both setter form (`Width = 5f`) and tuple form (`(StyleKeys.Width, 5f)`). Whitelist-based; falls back to existing pool-rent path for any non-literal value (state/captures/method calls).
- Perf: source generator emits child arguments directly into `params VirtualNode[]` instead of allocating a transient `__C(...)` array when the JSX children list is statically simple (no spreads, no conditional fragments). Cuts per-frame allocations on the children path.

## [1.1.5] - 2026-04-27
- Fix: cross-document Ref<T> unification — useRef<T>() now returns canonical global::ReactiveUITK.Core.Ref<T> in virtual docs, eliminating false CS1503 when passing refs to peer hooks (e.g. useGalagaGame(boardRef))
- Fix: polyfill stubs (Ref<T>, ReactiveEvent, etc.) now load correctly in workspaces where Unity has not yet compiled — detection scoped to runtime assembly names (ReactiveUITK.Shared/Runtime/Core), no longer suppressed by the LSP-internal ReactiveUITK.Language.dll
- Fix: formatter idempotency for multi-line paren-continuation (e.g. && / || chains) inside nested blocks — no more indent drift on save in files like the Galaga GameScreen sample
- Fix: source-generator diagnostic IDs unified with live analyzer (UITKX0006/0002/0009/0010/0017/0022/0023 → UITKX0103/0109/0106/0104/0108/0120/0121) so the same logical issue surfaces with the same code in both Unity Console and VS Code Problems pane
- Fix: RS2008 'analyzer release tracking' build warning suppressed in the LSP server csproj (warning targets analyzer NuGet packages, not the server EXE)
- Fix: removed stray chatHistory / globalState.keys() activation logging — leftover scaffolding from an unrelated experiment

## [1.1.4] - 2026-04-18
- Fix: UITKX0105 false positives eliminated — unknown-element diagnostics suppressed until workspace scan completes
- Fix: @(__children) no longer produces CS0103 in function-style components

## [1.1.3] - 2026-04-17
- Fix: VDG inline expression checks no longer cause false code dimming past directive boundaries
- Fix: formatter handles deeply nested structures correctly (22 idempotency regressions resolved)
- Fix: improved diagnostic line mapping for UITKX0014, UITKX0013, and CS0219

## [1.1.2] - 2026-04-11
- Feature: capture-phase event handlers (onClickCapture, onKeyDownCapture, onChangeCapture, etc.) — every bubble event now has a Capture suffix variant that fires during trickle-down
- Feature: refactored ApplyEvent/RemoveEvent with generic RegisterEvent<T>/UnregisterEvent<T> helpers — zero-overhead, no behavioral change
- Fix: removed dead diagnostic counters (totalStyleSets, totalStyleResets, totalEventsRegistered, totalEventsRemoved) and unused GetStyleMetrics()
- Feature: IDE autocompletion and IntelliSense for all capture-phase event props

## [1.1.1] - 2026-04-11
- Fix: hook/module files with arbitrary middle segments (e.g. Counter.hook.uitkx, Counter.custom.uitkx) no longer produce invalid C# class names — container class derivation now truncates at the first dot instead of relying on a suffix whitelist

## [1.1.0] - 2026-04-10
- Feature: full IDE support for hook companion files (.hooks.uitkx) — parsing, diagnostics, hover, completions, coloring, formatting
- Feature: full IDE support for module companion files (.style.uitkx, .utils.uitkx) — parsing, diagnostics, hover, completions, coloring, formatting
- Feature: cross-file peer resolution reads editor buffers instead of disk — no stale diagnostics when editing companion files
- Feature: hover shows type info for local variable, parameter, and field declarations
- Feature: hover shows delegate invoke signature (e.g. void Action(int value)) instead of raw type name
- Fix: CS1662 lambda cascade errors suppressed when caused by state-setter type mismatches
- Fix: removed 7 hot-path log calls that fired on every keystroke, rebuild, or hover

## [1.0.308] - 2026-04-09
- Feature: delegate-typed variables (useState setters, callbacks) now colored as functions in both VS Code and VS2022
- Fix: generic function calls like useState<string>() no longer lose function coloring

## [1.0.307] - 2026-04-08
- Fix: unreachable code after return now correctly dims entire scope without squiggly lines
- Fix: stale gray no longer persists after removing return statement — unreachable diagnostics stripped from T3 carry-forward cache

## [1.0.306] - 2026-04-07
- Breaking: comment syntax changed — {/* */} replaced with // (line) and /* */ (block) in markup
- Fix: @(expr) inline expressions now type-checked as VirtualNode in IDE diagnostics
- Fix: UITKX0025 single-root validation now covers var x = (<A/><B/>)
- Fix: formatter uses block diff (single TextEdit) — eliminates corruption on blank-line variations
- Fix: formatter bare-return idempotency — first format pass now matches canonical form
- Fix: formatter no longer collapses empty container elements to self-closing
- Fix: semantic tokens for // and /* */ comments in markup
- Fix: removed custom toggleBlockComment command — Ctrl+/ uses //, Shift+Alt+A uses /* */

## [1.0.305] - 2026-04-06
- Fix: bare JSX returns now accepted in control blocks
- Fix: formatter no longer collapses empty container elements to self-closing
- Fix: formatter bare-return idempotency (first format pass now matches canonical form)
- Fix: comment toggle now defaults to line comments in code sections

## [1.0.304] - 2026-04-05
- Feature: @uss and Asset<T>("...") path completion — filesystem-aware autocomplete inside quoted paths
- Feature: @uss added to preamble autocomplete and uitkx-schema.json directive list
- Feature: Rules of Hooks diagnostics (UITKX0013–0016) now reported in the IDE with accurate squiggle positions

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
