## [0.5.11] - 2026-05-13

### LSP Go-To-Definition - resolves module and hook symbols across directories

**Cross-directory peer discovery.** Jumping to `Theme.SidebarWidth` from `Sidebar.style.uitkx` used to return nothing when `Theme.uitkx` lived in a different folder. `RoslynHost.FindPeerUitkxFiles` only enumerated same-directory `.uitkx` files, so Roslyn never saw the declaring document and had no symbol to resolve to.

`WorkspaceIndex` now tracks a workspace-wide set of `.uitkx` files that contain top-level `module` or `hook` declarations (matched by a `Multiline` regex `^(?:module\s+\w+\s*\{|hook\s+\w+\s*[<\(])`). The set is updated incrementally on every `IndexUitkxFile` and `Refresh` call under a brief write lock, exposed via `GetModuleAndHookFiles()`, and appended to the per-document peer list. Roslyn then loads these files as workspace documents alongside same-directory peers, and Go-To-Definition resolves the cross-directory symbol naturally.

The three downstream consumers - `EnrichWithPeerHookUsings`, `AddPeerUitkxDocuments`, `AddPeerUitkxDocumentsToSolution` - already filter peers by `HookDeclarations` / `ModuleDeclarations` not being default, so the wider candidate set is cost-free for files that declare neither. Module/hook files are typically single-digit per workspace.

Tooling-only release. No runtime, editor, source-generator, or shared changes; SG suite untouched. Pure extension fix with a library version bump for symmetry.

**Tests.** 62/62 LSP server tests passing.

VS Code **1.2.6 -> 1.2.7** | VS 2022 **1.2.6 -> 1.2.7**.

---

## [0.5.10] - 2026-05-13

### Component HMR rewritten as a static trampoline - no more stale renders on rapid saves

**Trampoline refactor.** Every `component` now compiles to a `static` triplet: an `internal static` delegate field `__hmr_Render` (initialized to `__Render_body`) and a `Render` entry that branches on `HmrState.IsActive`. HMR-side and project-side emit are byte-identical at the call site.

```csharp
component Counter(int initial = 0) {
    var (count, setCount) = useState(initial);
    return (<Button text={count.ToString()} />);
}
```

The new `UitkxHmrComponentTrampolineSwapper` swaps via one O(1) `FieldInfo.SetValue` per type, then notifies only fibers whose `TypedRender.Method.DeclaringType` matches. The legacy per-fiber closure-rollback path (`HmrPreviousRender` field, ~36 lines in `FiberReconciler`, two `IsCompatibleType` source-path fallbacks) is gone. A type-level rollback registry bridges via `HmrState.TryRollbackComponent` so a bad swap reverts cleanly. Roslyn's per-call-site method-group cache keeps `ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)` stable.

User-visible: 30+ Ctrl+S no longer leaks stale renders, navigate-away-then-back shows the new code, and incompatible hook-signature edits reset state in-place (React Fast Refresh) - no domain reload.

**Fix - rude-edit reloads Play-mode-safe.** `RequestScriptReload` in Play mode produced partial reloads that broke `MonoBehaviour` refs. Deferred to the next `EnteredEditMode`.

**Fix - HMR Stop no longer stalls 30-40s.** Dropped `CleanBuildCache` from the change-watcher trigger; Roslyn's `AdditionalText` cache picks up edits incrementally.

**Fix - LSP false "unused local" on hook pairs.** `AnalyzeUnusedLocals` treats `dataFlow.Captured` as reads, so `var (count, setCount) = useState(0);` consumed inside lambdas stops warning.

**Tests.** 1222/1222 SG passing (+2 trampoline parity tests).

VS Code **1.2.5 -> 1.2.6** | VS 2022 **1.2.5 -> 1.2.6**.

---

## [0.5.9] - 2026-05-12

### Module `static readonly` fields now hot-reload - no domain reload, no stale refs

**B28 closed.** Editing a module-scope `Style` or `Color` field initializer (e.g. `PaddingTop = 4` -> `16` in `Sidebar.style.uitkx`) used to report a successful HMR cycle but leave the UI on the cold value until you exited Play mode. Already-mounted components picked it up; newly-mounted ones kept reading the old ref.

Byte-level IL diagnostics confirmed: the Mono JIT inlines `ldsfld <static readonly>` into native code at first call-site emission. `FieldInfo.SetValue` updates the slot, but already-JIT'd methods keep reading the inlined cold ref.

```csharp
module Sidebar {
    public static readonly Style Wrapper = new Style {
        PaddingTop = 4,   // edit + save - visible next render
    };
}
```

Permanent IL fix (IL2CPP and Mono AOT player builds too - Editor and Player IL stay identical). The SG strips `readonly` from every top-level `static readonly` field inside a `module { ... }` body and decorates the rewrite with `[global::ReactiveUITK.UitkxHmrSwap]`. HMR mirrors the rewrite in `HmrHookEmitter.EmitModules`. Generator-managed module statics (`__sty_N` style hoists, `__uitkx_ussKeys`) get the same treatment. The hook-cache `static readonly ConcurrentDictionary` is intentionally left as `initonly` (ref is immutable; only contents are HMR-replaced) and the swapper still finds it via `IsInitOnly`.

New analyzer `UITKX0210` (Warning) flags writes to `[UitkxHmrSwap]` fields from non-cctor code - HMR will overwrite any external write on the next save.

**Known limitation.** Static auto-properties (`public static Style Root { get; } = ...`) lower to a private `static readonly` backing field the SG cannot rewrite. Prefer fields for HMR-able module values. Auto-property promotion is on the roadmap.

**Tests.** 1218/1218 SG passing (20 new: stripper, analyzer, end-to-end).

VS Code **1.2.4 -> 1.2.5** | VS 2022 **1.2.4 -> 1.2.5**.

---

## [0.5.8] - 2026-05-11

### `[OnOpenAsset]` migrated to `EntityId` callbacks on 6.3+ - no obsolete API touched

**Console navigation, warning-free on 6.3 / 6.4.** Unity 6.3 deprecated every `int<->EntityId` conversion on `EntityId` - including the implicit cast operator itself, which warns *"EntityId will not be representable by an int in the future. This casting operator will be removed in a future version."* A naive `(EntityId)instanceId` cast still emits `CS0618`.

A reflection probe of `UnityEngine.CoreModule.dll` on 6000.3 / 6000.4 showed the proper migration: `OnOpenAssetAttribute` accepts both the legacy `(int, int, int)` callback shape and a new `(EntityId, int, int)` shape on 6.3+. Let Unity hand us the `EntityId` directly - no conversion at all.

```csharp
#if UNITY_6000_3_OR_NEWER
    [OnOpenAsset]
    private static bool OnOpenAssetCompat(EntityId entityId, int line, int col)
        => HandleOnOpenAsset(entityId, line, col);

    private static bool HandleOnOpenAsset(EntityId id, int line, int col)
    {
        var path = AssetDatabase.GetAssetPath(id);
        if (string.IsNullOrEmpty(path))
            path = AssetDatabase.GetAssetPath(EditorUtility.EntityIdToObject(id));
        return ResolveAndDispatch(path, line, col);
    }
#else
    // int-typed callbacks retained for 6000.2 where EntityId doesn't exist
#endif
```

The four `[OnOpenAsset]` callbacks in `UitkxConsoleNavigation.cs` split by `#if UNITY_6000_3_OR_NEWER`. Common path-resolution and external-editor-launch logic moved into a shared `ResolveAndDispatch`. Zero obsolete APIs touched on 6.3+.

**Fix - `DoomTextures` sample CS8618.** Six lazy-init fields suffixed `= null!`; all getters route through `EnsureBuilt()`, no behaviour change.

**Docs.** Web API page picked up a note explaining why storm-time interactivity is non-restorable framework-side (per-panel event state is panel-owned).

VS Code **1.2.3 -> 1.2.4** | VS 2022 **1.2.3 -> 1.2.4**.

---

## [0.5.7] - 2026-05-11

### `<Portal>` survives Unity 6.3 panel rebuilds - completes the 0.5.6 defense

**Bug class 2 closed.** 0.5.6 fixed the main `RootRenderer` against Unity 6.3's silent `rootVisualElement` rebuilds, but world-space portals still vanished when their `UIDocument` was clicked in the Hierarchy. Logs showed the world root swapped repeatedly with `childCount=0`: `Hooks.UseUiDocumentRoot` correctly re-fired the consumer with the new root, but the Fiber commit phase had no path to physically move existing portal children from the old target to the new one. New mounts went to the new target; deletions cleared from the old; **stable** children stayed parented to the dead target.

**Fix lives in the commit phase**, mirroring 0.5.6's `RetargetContainer` at portal granularity:

- New `EffectFlags.PortalRetarget` (bit 6), set in `CompleteWork` when a `HostPortal` fiber's `PortalTarget` differs from its alternate's. One `ReferenceEquals` per portal per render - zero cost when stable.
- New `CommitWork` branch runs `CommitPortalRetarget`: bounded DFS descending only through non-host wrappers (`Fragment`, `FunctionComponent`, `ErrorBoundary`, `Suspense`) to the first host descendant on each branch. Reparenting one VisualElement carries its subtree along - no per-VE recursion. Nested portals skipped.
- `parent.Add(child)` removes from the old parent first, so retarget is safe even when Unity has already disposed the old target.

Combined with `UseUiDocumentRoot`, world-space portals now survive the rebuild storm: hook re-fires ? consumer renders `<Portal target={newRoot}>` ? reconciler reparents the subtree, all in one commit.

**Fix - lowercase `useUiDocumentRoot` alias.** 0.5.6 only registered the hook in the signature regex; SG/HMR rewrite tables and IDE Roslyn stubs were missed, so `useUiDocumentRoot(...)` in `.uitkx` produced `CS0103` except via `Hooks.UseUiDocumentRoot(...)`. All four sites synced.

VS Code **1.2.2 ? 1.2.3** | VS 2022 **1.2.2 ? 1.2.3**.

---

## [0.5.6] - 2026-05-10

### Unity 6.3 panel-rebuild defense - UIs no longer disappear on Inspector interaction

**Confirmed Unity 6.3 regression.** A standalone probe (`PanelLifecycleProbe`) showed `UIDocument.rootVisualElement` is silently recreated on every `InspectorWindow` redraw - selection changes, hovers, field focus. 6.2 fires zero events on the same project; 6.3 fires `DetachFromPanelEvent` ? fresh `VisualElement` instance, call stack ending at `UnityEditor.InspectorWindow.RedrawFromNative`. Reported to Unity. Distinct from UUM-47682 (closed "By Design" for UI Builder Live Reload only) - the documented workaround there ("create UI in `OnEnable`") doesn't apply because the rebuild fires repeatedly *after* `OnEnable`.

There is no public event for this rebuild, so polling is the only defense. The fix is additive - both old and new APIs remain valid.

**`RootRenderer.Initialize(UIDocument)` overload (new).** Subscribes to a per-frame poll. On swap, migrates the fiber tree to the new root via `RetargetHost` + `RetargetContainer`: host props re-applied, children re-added, all hook state and subscriptions preserved.

```csharp
rootRenderer.Initialize(uiDocument.rootVisualElement); // legacy, still valid
rootRenderer.Initialize(uiDocument);                   // 6.3-resilient
```

**`UseUiDocumentRoot` hook (new).** Stable `VisualElement` reference tracking the current root, with `ReferenceEquals` short-circuit.

```csharp
var root = Hooks.UseUiDocumentRoot(myUIDocument);
```

**Reparent-resilient adapters.** Video, MultiColumnListView, MultiColumnTreeView, TabView selection trackers route through new `PanelDetachGuard.Wire(ve, teardown)`, which defers teardown one frame via `MainThreadTimer.OneFrameLater` and cancels it on re-attach.

**Cost.** ~10 ns/frame per managed root. Zero hot-path allocations. O(N) retarget runs only on real rebuilds (Editor-only). HMR compatible.

VS Code **1.2.1 ? 1.2.2** | VS 2022 **1.2.1 ? 1.2.2**.

---

## [0.5.5] - 2026-05-09

The React idiom `{cond && <Tag/>}` now splices cleanly in markup expression positions. C# `&&` is bool-only - `bool && VirtualNode` is `CS0019` - so the splicer rewrites the expression at emit time to a ternary `((cond) ? V.Tag(...) : (VirtualNode?)null)`, reusing the already-tested Phase 1 ternary path. The `null` fallback is dropped at render time by `__C(params object[])` which filters nulls. No runtime change.

```jsx
// simple bool
<Box>{flag && <Label text="hi"/>}</Box>

// null check (the repro)
<Box>{icon != null && <Image texture={icon}/>}</Box>

// nested - LHS walker respects precedence
<Box>{a ? b : c && <Tag/>}</Box>     // LHS = c
<Box>{a || b && <Tag/>}</Box>        // LHS = b
```

A new shared `FindLhsStartForLogicalAnd` walker finds where the LHS of `&&` begins: forward pass, per-paren-depth boundary tracking, lexer-aware string/comment skipping, recognises `?`, `:`, `??`, `||`, `,`, `;` as boundaries at the same depth as the `&&`. Mirrored across SG, HMR (via reflection), and the IDE virtual document so the editor no longer flags CS0019.

Degenerate input (`{ && <X/>}`) emits a single `UITKX0026` diagnostic instead of cascading into raw-JSX compile errors. Setup-code and directive-body `&&` JSX (`var node = cond && <Tag/>;`) remain unsupported - tracked in TECH_DEBT_V2 item 15, workaround is the explicit ternary.

**Fix - `using UnityEngine;` is now injected into generated C#.** Six SG and HMR emit sites covered the namespace, partial-class, and function-component-overload positions. The IDE virtual document already had `UnityEngine` in scope, so code touching `Texture2D`, `Color`, `Vector2`, `Mathf`, etc. compiled green in the editor but red at build / HMR. Both pipelines now see the same surface area.

**Tests.** 8 new in `JsxInExpressionTests`, 3 in `UnityEngineImportTests`. **1198/1198 SG** passing. VS Code **1.2.0 ? 1.2.1** | VS 2022 **1.2.0 ? 1.2.1**.

---

## [0.5.4] - 2026-05-08

### Strict attribute validation on user components - fixed silent CS0117 cascade

Anything that isn't a declared parameter on a user component (or the universal `key` / `ref`) is now rejected at the IDE and build layers. Forwarding `style`, `name`, `className`, `onClick`, `extraProps`, etc. through a user component used to silently compile to `Style = x` against the generated `AppButtonProps` and explode at C# build time as `CS0117` with no pointer back to the `.uitkx` file. Now you get a proper `UITKX0109` error in the IDE and at build time with an actionable hint listing the available parameters.

```jsx
// before - silent slip-through, then CS0117
<AppButton text="Save" style={btnStyle}/>

// now - UITKX0109: Unknown attribute 'style' on 'AppButton'.
//                  Available on 'AppButton': text, onClick.
```

**Root cause.** The schema lumped `key`/`ref` together with the 58 `BaseProps` members under one `universalAttributes` list, so every tag appeared to accept the full intrinsic surface. The list is now split into two:

- `structuralAttributes` - `key`, `ref` - apply everywhere (`key` lives on `VirtualNode`; `ref` is routed `forwardRef`-style to the unique `Hooks.MutableRef<T>` parameter).
- `intrinsicElementAttributes` - the 58 `BaseProps` members - only valid on built-in `V.*` tags backing a `VisualElement`.

Built-ins are unchanged: `<Button style={...} extraProps={...}/>` still works.

**Migration.** Forwarding `style` through a user component? Declare it explicitly on the component:

```jsx
component AppButton(string text = "", IStyle? style = null) {
    return (<Button text={text} style={style}/>);
}
```

**Parity.** Editor (LSP) and build (source generator) now share the same attribute map - no more red-in-editor / yellow-at-build asymmetry. The bad attribute is skipped in the emitted C# so `UITKX0109` doesn't cascade into `CS0117`/`CS0246`.

**Tests.** 9 new regression tests covering both legal `key`/`ref` propagation and rejection of all 58 `BaseProps` members on user components. **1187/1187 SG** passing.

VS Code **1.1.15 ? 1.2.0** | VS 2022 **1.1.15 ? 1.2.0** ride the same release.

---

## [0.5.3] - 2026-05-08

### Hard cut of `@(expr)` markup syntax - `{expr}` is the only form

The only embed form for arbitrary C# expressions inside markup is now `{expr}` - same as JSX / Babel / React. The `@` prefix survives only as the directive marker (`@if`, `@else`, `@for`, `@foreach`, `@while`, `@switch`, `@case`, `@default`, `@using`, `@namespace`, `@component`, `@props`, `@key`, `@inject`, `@uss`).

```jsx
// before
<Box>@(items.Count)</Box>
<Tag attr=@(value) />

// now
<Box>{items.Count}</Box>
<Tag attr={value} />
```

Files containing legacy `@(expr)` raise hard parse error `UITKX0306`. Migration is mechanical: every `@(` ? `{`, matching `)` ? `}`. Unification touches the parser, formatter, analyzer, IntelliSense, VDG, HMR, source generator, TextMate grammar, all 12 shipped samples, and the tests.

**Fix - pool-rent decls no longer hide inside line comments.** The naive backward-scan picking the splice point for `var __p_N = __Rent<TProps>()` stopped at the first `;` or `}` - including `}` inside `// see {catBadge}` comments. The compiler ate the decls as comment text ? downstream `CS0103`. All four sites (SG x2, HMR x2) replaced with a shared `FindLastTopLevelStatementBoundary` lexer-aware scanner that skips comments, strings (regular / verbatim / interpolated with brace-depth in holes), and char literals. Pre-Phase 2 the comment had `@(catBadge)` so the bug was masked - unification exposed it.

**Fix - TextMate grammar was completely dead.** The JSON file picked up a UTF-8 BOM during the edit pass. `vscode-textmate` rejects BOM ? grammar fails to load ? every scoped token fell back to plain text (keywords, properties, operators rendering white in module bodies). Both grammar copies are now BOM-free; 37 rules load.

**Tests.** **1178/1178 SG** passing. HMR ? SG parity green.

VS Code **1.1.14 ? 1.1.15** | VS 2022 **1.1.14 ? 1.1.15** ride the same release.

---

## [0.5.2] - 2026-05-08

### JSX literals in any C# expression position - Phase 1 splicer

JSX literals now splice cleanly wherever a C# expression is allowed, matching React / Babel semantics. Patterns that previously emitted raw JSX and tripped the C# compiler now compile to `V.Tag(...)` calls:

```jsx
// ternary branches
<Box>{cond ? <A/> : <B/>}</Box>

// null-coalescing
<Box>{fallback ?? <Default/>}</Box>

// JSX in attribute expressions
<Box icon={active ? <Check/> : <X/>}/>

// JSX inside lambda bodies
attr={items.Select(x => <Item key={x.Id}/>)}
```

The scanner powering this (`FindBareJsxRanges` + `FindJsxBlockRanges`) was already proven on component preambles and directive bodies - it is now wired into the two remaining emit sites (`EmitExpressionNode` and the `CSharpExpressionValue` branch of attribute emission) and mirrored in HMR and the IDE virtual-document generator.

**No runtime change** | same `V.Tag(...)` factory + `__Rent<TProps>()` pooled shape. The whole splice is emit-time only and short-circuits when an expression contains no JSX.

**Fix - `Texture2D ? iconName = null` no longer drops the `?` on save.** Whitespace before `?` in a nullable component param made the tokenizer leave it unconsumed; format-on-save then re-emitted the parameter as non-nullable. Same pathology as the recent `@else` blank-line bug - formatter re-emit is lossy when the parser drops tokens. The tokenizer now peeks past whitespace, canonicalises the captured type, and emits a clean `Texture2D? name` regardless of input spacing.

**HMR + IDE parity.** HMR mirrors the splice end-to-end. The virtual-document generator strips embedded JSX to typed `(VirtualNode)null!` stubs so the IDE no longer shows phantom Roslyn errors on files that compile cleanly under SG.

**Tests.** 14 new (10 SG `JsxInExpressionTests`, 1 HMR parity tripwire, 3 VDG). **1178/1178** passing.

VS Code **1.1.13 ? 1.1.14** | VS 2022 **1.1.13 ? 1.1.14** ride the same release.

---

## [0.5.1] - 2026-05-07

### Generic `static` methods inside `module { - }` - fixed

Consumer (JustStayOn) upgraded 0.4.15 ? 0.5.0 and hit `CS0119: 'TProps' is a type, which is not valid in the given context` on every generic method inside `module Dialogs { Register<TProps,TResult>(...), Open<TProps,TResult>(...), - }`, plus `CS8625: Cannot convert null literal to non-nullable reference type` under `<Nullable>enable</Nullable>`. Both bugs sat dormant in `ModuleBodyRewriter` since 0.4.19 (the release that introduced `module { }` HMR). Non-generic methods worked fine - generic ones never ran through a real compiler in CI.

**Bug 1 - CS0119.** `AppendTypeArgs` emitted bare type-parameter names into the synthesized `MethodInfo.MakeGenericMethod(...)` call ? `MakeGenericMethod(TProps, TResult)`. `MakeGenericMethod` takes `params Type[]`. Fix: wrap each name in `typeof(...)` ? `MakeGenericMethod(typeof(TProps), typeof(TResult))`.

**Bug 2 - CS8625.** Generic-branch `MethodInfo` HMR field emitted as `= null;`. The field MUST start null - the trampoline checks `!= null` to fall through to the body method until `UitkxHmrModuleMethodSwapper` rebinds it via reflection. Fix: emit `= null!;`. Runtime value identical, nullable warning gone, swapper untouched.

**Why this only hit now.** `module { }` HMR landed in 0.4.19 with substring-only test coverage. Both broken outputs still contained the markers the existing test asserted on (`MakeGenericMethod`, `__hmr_<name>_h-`). Real consumer code with generic module methods was the first end-to-end compile of that code path.

**Test.** New `Sg_ModuleGenericMethod_GeneratedCodeCompiles_NoCS0119_NoCS8625` actually compiles the SG output through Roslyn (`UNITY_EDITOR` defined, nullable enabled) and asserts neither CS0119 nor CS8625 is raised. Bug class structurally impossible to recur silently. **1162/1162 SG** passing.

Runtime-only release - IDE extensions unchanged.

---

## [0.5.0] - 2026-05-06

### Media - `<Video>`, `<Audio>`, `useSfx()` + a fiber-unmount fix

Two declarative elements and one hook bring video/audio into UITKX, all rendered through one shared host.

**`<Video>`.** Pooled `VideoPlayer` + `RenderTexture` from a new `MediaHost`, fed into a UI Toolkit `Image` via `image = renderTexture` (`style.backgroundImage` caches stale samples). `frameReady` drives `MarkDirtyRepaint()`; an editor-only `QueuePlayerLoopUpdate()` pump advances the player when Unity isn't ticking. Props: `Clip`/`Loop`/`Autoplay`/`Muted`/`ScaleMode`/`Volume` + `VideoController` ref.

**`<Audio>` (Func-Component).** No visible content; rents an `AudioSource` via `UseEffect`, returns it on unmount. Same shape minus visuals + `Pitch`/`SpatialBlend`/`AudioMixerGroup`. `AudioController` ref.

**`useSfx()`.** Stable, allocation-free `Action<AudioClip, float>` backed by `MediaHost.Instance.SfxSource.PlayOneShot`. Same delegate ref across renders.

**`MediaHost`.** `HideAndDontSave` GameObject; reference-counted player/source pools; RT pool keyed by `(w, h, depth)`. Survives domain reloads.

**Fix - `FiberRenderer.Clear()` runs effect cleanups.** The audio leak surfaced it: closing the demo window left music playing forever. The stub cleared the container and nulled the root without invoking `CommitDeletion` - every `UseEffect` cleanup (timers, signal subs, RTs, audio) leaked on every editor mount/unmount. New `FiberReconciler.UnmountRoot()` runs depth-first `CommitDeletion`; `EditorRootRendererUtility.Unmount()` drains via `EditorRenderScheduler.PumpNow()`.

**IDE - `useSfx()` recognized in the editor.** Added to the LSP virtual-document hook-stub list (it was in SG/HMR alias regexes but missing here, so the editor reported `CS0103`).

**Demo.** `MediaPlaygroundDemoPage.uitkx` covers everything. Editor: `ReactiveUITK > Demos > Media Playground`.

VS Code **1.1.11 ? 1.1.12** | VS 2022 **1.1.11 ? 1.1.12** ride along.

---

## [0.4.19] - 2026-05-04

### Full HMR support for `module { - }` declarations

`module { - }` is now hot-reloadable end-to-end. Edit a `Style` field, a `static readonly Color`, or a 200-line `GameLogic.Tick(ref GameState st, -)` and the change takes effect on the next call - no Play-mode exit, no domain reload.

**Per-method hot-swap.** New SG pass (`ModuleBodyRewriter`) rewrites every top-level `static` method into a public trampoline bouncing through an `__hmr_<name>_h<sig>` delegate field to a private body method. `UitkxHmrModuleMethodSwapper` rebinds each field via `Delegate.CreateDelegate` after every HMR compile. Custom delegate types preserve `ref`/`out`/`in`/`params`; FNV-1a signature hash disambiguates overloads; generics use a `MethodInfo` + `ConcurrentDictionary` cache. `#if UNITY_EDITOR`-gated - zero player-build overhead. Trampoline visibility tracks the original, so `private static` methods using `private` nested types stay valid.

**Documented contract** per member kind: const ? re-baked at compile; `static readonly` ? re-initialised each cycle; mutable static ? preserved; static method ? hot-swapped; instance / nested / new field-or-method ? verbatim or rude-edit.

**Rude-edit detection.** Adding a new `static readonly` mid-session can't grow the project type's metadata - the CLR seals it at load. Instead of a silent `MissingFieldException` later, `UitkxHmrModuleStaticSwapper` logs a once-per-session warning. New `UitkxHmrController.AutoReloadOnRudeEdit` EditorPref (default `false`) automates the reload.

**12 HMR ? SG parity bugs fixed** in `HmrCSharpEmitter`: sibling-Props, JSX-as-attribute-value, duplicate-`key={}`, `ref={x}` on Func components, `Asset<T>(-)` in module bodies, deterministic Roslyn overload picking, FQN type comparison. New `UITKX0150` surfaces module-body parse failures with verbatim fallback. **1142/1142 SG** passing.

IDE extensions unchanged - runtime-only release.

---

## [0.4.18] - 2026-05-03

### HMR `CS0426` on function components with sibling top-level Props - fixed

After 0.4.17 unblocked HMR compilation of module/style/hook files, a follow-up surfaced: `[HMR] Compilation failed for AppRoot... CS0426: The type name 'RouterFuncProps' does not exist in the type 'RouterFunc'`. A long-standing convention divergence between SG and HMR that only became reachable once HMR could compile these files end-to-end.

**The bug.** Func-component Props classes ship in three legitimate shapes:

1. **Sibling top-level** | `RouterFunc` + `RouterFuncProps` both at namespace scope, neither nested. Used by `ReactiveUITK.Router`.
2. **Nested same-name** | `CompFunc.CompFuncProps` (the SG's default emission).
3. **Nested differently-named** | `ValuesBarFunc.Props` (legacy hand-written).

SG's `PropsResolver.TryGetFuncComponentPropsTypeName` walked all three. HMR's `FindPropsType` only walked nested types and shipped `{Type}.{Type}Props` unconditionally - shape (1) compiled fine through SG but failed CS0426 through HMR.

**The fix.** `FindPropsType` now mirrors `PropsResolver` verbatim - sibling top-level first, then nested same-name, then any nested `IProps`, then a convention fallback. One canonical answer per component.

**Tests.** Two layers on every push/PR/publish:
- **SG-side parity test** drives the generator with the real `RouterFunc` / `RouterFuncProps` shape and asserts `V.Func<global::Ns.RouterFuncProps>` is emitted.
- **HMR algorithm contract tests** | five Roslyn-in-memory cases (sibling / nested-named / nested-legacy / sibling-wins-priority / negative fallback) mirror `FindPropsType` verbatim, since the Editor assembly's `UnityEditor` deps block direct loading by the standalone test runner.

**1070/1070 SG** passing.

VS Code **1.1.10 ? 1.1.11** | VS 2022 **1.1.10 ? 1.1.11** ride the same release.

---

## [0.4.17] - 2026-05-03

### Two converging bugs around `.style.uitkx` / `.hooks.uitkx` - both fixed

Consumer hit `[UITKX] Asset not found in registry: "../Resources/background-01.png"`. Two independent latent bugs converged in module/style/hook files.

**Bug 1 - HMR `ArgumentException` on every save.** `UitkxHmrCompiler.InvokeWithDefaults` had two `params object[]` overloads; C# resolution preferred `string ? object target`, shifting all args left and landing a `List<ParseDiagnostic>` in `DirectiveParser.Parse`'s `filePath` slot. Collapsed to a single canonical `(MethodInfo, object target, params object[])` with mandatory `target` - bug class structurally impossible to recur. All 11 call sites updated.

**Bug 2 - `Asset<T>(...)` not rewritten in `module` / `hook` bodies.** `UitkxAssetRegistry` is keyed by resolved paths; the emitter rewrite covered setup code + JSX + `@if`/`@foreach`/`@switch` but skipped module/hook bodies, so `Asset<Texture2D>("../Resources/bg.png")` shipped as the raw literal while registry sync wrote the resolved key - runtime miss. `ResolveAssetPaths` promoted to `internal static` shared by `CSharpEmitter`/`HookEmitter`/`ModuleEmitter`; HMR's `HmrCSharpEmitter.ResolveAssetPaths` visibility lifted so `HmrHookEmitter` routes module+hook bodies through it. Source-gen and HMR now emit literal-identical asset strings.

**Tests.** 4 new `EmitterTests` regressions (module `./`, module `../`, module absolute, hook `./`) run on every push/PR/publish. **1064/1064 SG** passing.

VS Code **1.1.9 ? 1.1.10** | VS 2022 **1.1.9 ? 1.1.10** ride the same release.

---

## [0.4.16] - 2026-05-03

### HMR - fix `TargetParameterCountException` + production-grade hardening

A reflection signature drift between the editor-only HMR compiler and `ReactiveUITK.Language.dll` (`UitkxParser.Parse` gained an optional `lineOffset` in 0.4.7) was firing `TargetParameterCountException` on every `.uitkx` save during play mode, swallowed into a silent warning + infinite retry storm.

**Layer 1 - fix.** Both `_uitkxParse.Invoke` sites now pass the trailing `lineOffset = 0`. Hot reload of components, hooks, and modules works again.

**Layer 2 - `InvokeWithDefaults` helper.** All six reflective calls into the language library now route through a helper that pads short argument arrays with each parameter's compile-time `DefaultValue`. One-time `Debug.LogWarning` per `MethodInfo` surfaces silent API drift the next time it happens, instead of failing.

**Layer 3 - infrastructure-error classifier + self-disable.** `HmrCompileResult.IsInfrastructureError` is now set when the inner exception is `TargetParameterCountException | MissingMethodException | MissingFieldException | TypeLoadException | ReflectionTypeLoadException | BadImageFormatException`. The controller emits one `Debug.LogError` with actionable text and calls `Stop()` (the only safe disable path - unhooks events, stops the watcher, unlocks the assembly-reload suppressor, restores `runInBackground`, clears retry queues). User-authored compile errors (CS0103, CS1xxx, syntax) keep the existing warn + retry cascade.

VS Code **1.1.8 ? 1.1.9** | VS2022 **1.1.8 ? 1.1.9** | IDE virtual-document generator now injects `using static ReactiveUITK.AssetHelpers;` so `Asset<T>("...")` and `Ast<T>("...")` no longer report CS0103 in component setup blocks, hook bodies, and module/style initializers (e.g. `AppRoot.style.uitkx`).

**1060/1060 SG** passing. Source generator, runtime, build, and IDE extension surfaces are otherwise untouched.

---

## [0.4.14] - 2026-05-03

### Router - React-Router-v6 parity (additive, no breaking changes)

New primitives: **`<Outlet/>`**, **`<Routes>`** (ranked first-match-wins), **`<NavLink>`** (active styling), **`<Navigate to>`** (declarative redirect).

`<Route>` gains `index`, `caseSensitive`, and layout-route composition (`element` + child `<Route>`s feed `<Outlet/>`). `<Router>` gains `basename`; nested `<Router>` is now a hard error.

New `RouterHooks`: `UseOutletContext<T>`, `UseMatches`, `UseResolvedPath`, `UseSearchParams`, `UsePrompt`, `UseNavigate(NavigateOptions)`. Old signatures preserved.

Internals: shared `RouteRanker` (port of RR's `rankRouteBranches`/`computeScore`); single-source-of-truth tag-alias map shared by source-gen + HMR.

**1063/1063 SG** passing (6 new emission tests). VS Code **1.1.7 ? 1.1.8** | VS2022 **1.1.7 ? 1.1.8** ship schema entries for the new tags/attributes. Docs site router page rewritten to cover the new surface.

---

## [0.4.13] - 2026-05-02

### Style coverage - 13 missing IStyle properties wired end-to-end
Closes the long-standing UITKX vs `UnityEngine.UIElements.IStyle` wiring gap. Every IStyle property is now reachable via the typed `Style` API, the tuple `(StyleKeys.X, value)` form, and the IDE autocomplete schema.

### New typed properties (Unity 6.2 floor)
- **9-slice** | `UnitySliceLeft/Right/Top/Bottom`, `UnitySliceScale`, `UnitySliceType` (`Sliced`/`Tiled`).
- **Clipping** | `UnityOverflowClipBox` (`PaddingBox`/`ContentBox`).
- **Text** | `WordSpacing`, `UnityParagraphSpacing`, `TextShadow`, `UnityTextGenerator` (`Standard`/`Advanced`), `UnityEditorTextRenderingMode` (`SDF`/`Bitmap`, editor-only).
- **Fonts** | `UnityFontDefinition` (legacy `Font` or TextCore `FontAsset`).

New `CssHelpers`: `SliceFill`/`SliceTile`, `ClipPaddingBox`/`ClipContentBox`, `TextGenStandard`/`TextGenAdvanced`, `EditorTextSDF`/`EditorTextBitmap`, `Shadow(dx,dy,blur,color)`, `FontDef(font)`.

### Fix - 19 missing `styleResetters` (silent leak)
Setter/resetter audit found 19 `IStyle` properties with a setter but no resetter - removing them from a `style={}` block silently leaked the previous value. Now all reset to `StyleKeyword.Null` (`alignContent/Items/Self`, `backgroundPositionX/Y`, `backgroundRepeat/Size`, `flexDirection/Wrap`, `fontFamily/Size`, `justifyContent`, `position`, `rotate`, `scale`, `textAlign`, `transformOrigin`, `translate`, `unityFontStyle`).

### Tests
New `IStyleCoverageTests` (7 facts) regex-asserts every IStyle property is wired through every layer. Future Unity versions can no longer add an unwired property without a red CI. **1051/1051 SG - 61/61 LSP** passing.

### Extensions
VS Code **1.1.6 ? 1.1.7** | VS2022 **1.1.6 ? 1.1.7** | autocomplete for the 4 new enum-valued IStyle properties via embedded `uitkx-schema.json`.

---

## [0.4.12] - 2026-05-01

### Doom demo - Phase 9 sector-engine release
No runtime / source-generator / IDE changes this cycle. The `Samples/Components/DoomGame/` demo went from a flat raycaster to a real sector-portal engine - stacked floors, key-chain progression, minimap, and a full status bar.

### Renderer
- **Sector / portal raycaster (Phase 1-3)** | tile map compiles to `MapData` (sectors + linedefs); rendering walks portals via per-ray cliprange (`winTop`/`winBot` screen-Y window). Variable floor/ceiling heights, upper/lower wall segs, and sky cells render correctly.
- **ExtraFloor stacked slabs (Phase 9)** | sectors carry any number of slabs; column rasterizer emits front + back TOP/BOTTOM/SIDE planes per slab and tightens the cliprange so taller slabs further along the ray stay visible. Fixes the staircase upper-treads-vanish bug; powers Level 6-s 7-step interior staircase.
- **Z-aware collision (Phase 7)** | `BlocksMovementZ(footZ, headZ, STEP_HEIGHT)` replaces binary `BlocksMovement` for slab-aware step-up, jump, and crouch.

### Gameplay
- **6 hand-built levels** | Hangar, Toxin Refinery, Containment Area, Outpost, Phobos Anomaly, and a boss-only finale.
- **Level 1 progression rebuild** | hub gates side wings behind colored doors: yellow key in hub ? east wing (red key) ? west wing (blue key + shotgun) ? north boss room (Baron + Cacodemon). Walls flank every door so they can-t be sidestepped.
- **Boss-gated exits** | new `LevelStart.BossExitGated` + `GameLogic.AnyBossAlive` blocks level-end until every Baron / Cacodemon is dead, with a HUD message on attempt.
- **Walkable exit pads** | new `MapBuilder.ExitPad(x, y)` creates an Exit-kind cell with no wall texture and a deep-blue floor (`F_BLUE`); back wall painted with new `W_BRICK_BLUE` so the end-zone reads visually.
- **Boss balance** | Baron HP 800 ? 200, Cacodemon 400 ? 120; the Level 1 boss now drops in a few shotgun blasts.

### UI
- **Status bar rewrite** | 8-panel `FlexGrow`-ratio layout (AMMO / HEALTH / ARMS / FACE / ARMOR / KEYS / BREAKDOWN / INFO) filling the full 800-90 region; consistent title spacing, `WhiteSpace.NoWrap`, ARMS as a 3-column 7-weapon button grid.
- **Live minimap** | top-right overlay, auto-scales to fit any map into 160px. Walls, color-keyed doors, exit pad, player (yellow + heading), and every live mobj (red enemies, cyan pickups, key-color keys).

---

## [0.4.11] - 2026-04-28

### Performance
- **JSX children fast-path (OPT-V2-1)** | source generator emits children directly into `params VirtualNode[]` instead of allocating a transient `__C(...)` wrapper when the children list is statically simple. One allocation per element gone.
- **Static-style hoisting (OPT-V2-2)** | `style={new Style{...}}` literals with all-constant initializers are hoisted to class-level `static readonly Style` fields. The reconciler's `SameInstance` check makes the diff walk a no-op when the same instance is reused across renders. Falls back to the pool-rent path for any non-literal value.

### Extensions
VS Code **1.1.5 ? 1.1.6** | VS2022 **1.1.5 ? 1.1.6**
- Source-generator parity - both LSPs ship the new hoisting + children fast-path emit.
- **Cross-document `Ref<T>` unification** | `useRef<T>()` returns canonical `global::ReactiveUITK.Core.Ref<T>` in virtual docs, killing false `CS1503` when passing refs to peer hooks.
- **Polyfill stubs load correctly** in workspaces where Unity hasn't compiled yet.
- **Formatter idempotency for `&&` / `||` chains** inside nested blocks - no more indent drift on save.

---

## [0.4.10] - 2026-04-27

### Performance
- **Major reconciler & props pipeline rewrite** | UITKX went from ~1.7- overhead vs. native UIToolkit (28 FPS at the 3000-box stress benchmark) to ~78% of native (36-38 FPS).
- **Typed Props + Style pipelines** | eliminated ~27,000 dictionary/boxing allocations/frame; flat backing-field structs replace `Dictionary<string, object>`.
- **Object pooling + IIFE closure elimination** | removed ~6,000 `Style`/`BaseProps` allocations and ~3,000 loop-body closures per frame.

### Added
- **Doom-style game demo** (`Samples/Components/DoomGame/`). Window: `ReactiveUITK/Demos/Doom Game`.
- **Typed Props for editor field types** | `BoundsField`, `ColorField`, `DropdownField`, `EnumField`, `MinMaxSlider`, `ObjectField`, `Slider(Int)`, `Tab(View)`, `Toggle(ButtonGroup)`, `Vector2/3/4Field`, and more.

### Changed
- **SG diagnostic IDs unified with live analyzer** | seven codes renumbered so the same issue surfaces with the same ID in both Unity Console and IDE Problems pane (`UITKX0006/0002/0009/0010/0017/0022/0023 ? 0103/0109/0106/0104/0108/0120/0121`).
- **Initial `CreateRoot` render is now synchronous** | no empty-frame flicker between `Clear()` and the first commit.

### Fixed
- **Cross-wired Style / BaseProps "disco" bug** | pooled instance could be returned twice in one flush window and re-rented to two fibers. Fixed via idempotent `_isPendingReturn` guard.
- **`<ErrorBoundary>` stuck on fallback after `resetKey` change**, **`<Portal target={x}>` ignored target changes** | bailout clones now refresh the relevant fields from the new VNode.
- **VNode pooling reverted** | VNode refs can live inside opaque `IProps` payloads (e.g. `Route.Element`); pool returns produced cross-wired trees.

---

## [0.3.2] - 2026-04-07

### Breaking
- **Comment syntax normalized** | `{/* */}` JSX comments replaced with standard `//` (line) and `/* */` (block) in markup. Same syntax everywhere - setup code and JSX.

### Added
- **UITKX0025 for var assignments** | `var x = (<A/><B/>)` now flagged as single-root violation in IDE
- **Block comments in markup** | `/* */` supported in JSX markup for multi-line comments

### Fixed
- **`@(expr)` type enforcement** | inline `@(expr)` now type-checked as `VirtualNode` in IDE diagnostics. Non-VirtualNode expressions (e.g. `VirtualNode[]`) show errors early.
- **Formatter block diff** | single block TextEdit instead of per-line diffs, eliminates corruption on blank-line variations
- **Formatter idempotency** | bare-return formatting matches canonical form on first pass
- **Formatter preserves empty containers** | `<Box></Box>` no longer collapsed to `<Box />`
- **HMR dangling comma** | fixed pre-existing bug in `EmitChildArgs` when comment nodes appear between children

### IDE
- **VS Code** | removed custom `toggleBlockComment` command. `Ctrl+/` ? `//`, `Shift+Alt+A` ? `/* */`
- **VS2022** | simplified comment handler, always uses `//` line comments

### Extensions
VS Code **1.0.306** | VS2022 **1.0.82**
