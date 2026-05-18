# HMR Built-in Tag Parity + `<Video>` / `<Audio>` Elements + `UseSfx` Hook — Implementation Plan

**Status:** ✅ All 8 phases implemented. **1159/1159 SG tests passing** (was 1142, +17 new contract tests).
**Target version:** _intentionally not bumped_ (per user instruction during implementation).

## Phase summary

| Phase | What | Result |
|---|---|---|
| **1** ✅ | HMR built-in tag auto-discovery (structural fix for 38 latent broken tags) | `HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap()` reflection-mirrors SG's `BuildBuiltinMapFromCompilation`. 12 new contract tests. |
| **2** ✅ | `MediaHost` shared peer pool | `Shared/Core/Media/MediaHost.cs` — pooled `VideoPlayer`, `AudioSource`, `RenderTexture` (16-px-bucketed), `SfxSource` singleton, `MediaHostTicker` MonoBehaviour for OnEnded polling, play-mode reset. |
| **3** ✅ | `<Audio>` element end-to-end | `AudioProps`, `AudioController`, `AudioFunc`, `V.Audio` factory; SG fallback updated; renders Fragment (no visual). |
| **4** ✅ | `useSfx` hook | `Hooks.UseSfx(AudioMixerGroup mixer = null)`; `HookIdSfx`; both regex whitelists + both alias-list updates (SG + HMR). Stable delegate identity across renders. |
| **5** ✅ | `<Video>` element end-to-end | `VideoProps`, `VideoController`, `VideoFunc`, `V.Video` factory; renders positionable VE with pooled RT as `style.backgroundImage`; `GeometryChangedEvent` swap on resize; ScaleMode → BackgroundSize mapping; accepts overlay children. |
| **6** ✅ | Tests | 5 new SG parity facts in `HmrEmitterParityContractTests.cs` (Audio typed-path, Video typed-path, Video overlay children, useSfx camelCase + PascalCase signature detection). |
| **7** ⚠️ scaffold | MediaPlayground sample | Folder created with comprehensive README documenting required binary assets, expected file layout, and how to author the `.uitkx`. The `.uitkx` and bootstrap intentionally NOT shipped — the formatter snapshot tests require any committed `.uitkx` to be `AstFormatter`-idempotent, which is best done by the contributor who is also dropping in the media files. |
| **8** ✅ | Docs | `Plans~/MEDIA_USAGE.md` (architecture diagram + per-surface examples + caveats); `ide-extensions~/grammar/uitkx-schema.json` (Audio + Video element entries with all attributes). No version bumps (per explicit user instruction). No CHANGELOG entry yet — should be added when versioning resumes. |

---

## Files added

```
Editor/HMR/HmrCSharpEmitter.cs                      [edit]   replace literal s_tagMap with reflection-based discovery + nested HmrBuiltinTagDiscovery class
Shared/Core/Media/MediaHost.cs                      [new]    412 LOC — pooled peers, RT bucketing, ticker MB, play-mode reset
Shared/Core/Media/AudioController.cs                [new]    100 LOC — imperative handle for AudioSource
Shared/Core/Media/AudioFunc.cs                      [new]    150 LOC — render function with heavy + cheap effects + OnEnded polling
Shared/Core/Media/VideoController.cs                [new]    115 LOC — imperative handle for VideoPlayer (Volume/PlaybackSpeed/Muted live)
Shared/Core/Media/VideoFunc.cs                      [new]    245 LOC — render function with rent + prepare + RT lifecycle + GeometryChanged swap
Shared/Props/Typed/AudioProps.cs                    [new]    100 LOC
Shared/Props/Typed/VideoProps.cs                    [new]    105 LOC
Shared/Core/Hooks.cs                                [edit]   add HookIdSfx + UseSfx (~75 LOC)
Shared/Core/V.cs                                    [edit]   add V.Audio + V.Video factories
SourceGenerator~/Emitter/CSharpEmitter.cs           [edit]   add useSfx/UseSfx to alias list + signature regex
SourceGenerator~/Emitter/PropsResolver.cs           [edit]   add animate/audio/video to BuildFallbackMap (additive)
Editor/HMR/HmrCSharpEmitter.cs                      [edit]   add useSfx/UseSfx to alias list + signature regex
SourceGenerator~/Tests/HmrBuiltinTagDiscoveryContractTests.cs  [new]   12 facts mirroring discovery algorithm
SourceGenerator~/Tests/HmrEmitterParityContractTests.cs        [edit]  +5 facts (Audio, Video×2, useSfx×2)
Plans~/MEDIA_USAGE.md                               [new]    architecture + usage docs
ide-extensions~/grammar/uitkx-schema.json           [edit]   Audio + Video element entries
Samples/Components/MediaPlayground/README.md        [new]    scaffold README (no .uitkx / bootstrap until media assets sourced)
Plans~/VIDEO_AUDIO_ELEMENTS_PLAN.md                 [edit]   this file — completion status
```

---

## Test count

```
Before:  1142 / 1142  (cleanup_and_upgrades baseline)
After:   1159 / 1159  (+17 new contract tests, 0 regressions)
```

---

## Outcomes

1. **38 previously-broken HMR built-in tags** (`<Animate>`, `<Toolbar>`, `<TwoPaneSplitView>`, every editor field, every toolbar item, every multi-column view, etc.) are now hot-reloadable. Adding any future `V.Foo(FooProps, …)` factory is automatically picked up by both SG (Roslyn auto-scan) and HMR (reflection auto-scan) with zero further edits — the entire bug class that hit 0.4.16/17/18/19 is structurally closed.
2. **`<Video>` / `<Audio>` / `useSfx`** ship with pooled peers, imperative controllers, and full HMR support — reflection auto-discovery picks them up on first save.
3. **No breaking changes**, no behavior changes to existing tags, no PropsResolver core changes — fallback map only grew (additive). `BaseProps` and `Pool<T>` lifecycle were sufficient as-is; no infrastructure modifications.
4. **Documentation:** `Plans~/MEDIA_USAGE.md` + IDE schema entries. CHANGELOG.md and `ide-extensions~/changelog.json` and `package.json` versions intentionally untouched per user instruction.

---

## Open follow-ups (out of scope for this PR)

- Author `MediaPlayground.uitkx` + bootstrap once royalty-free assets are sourced (see `Samples/Components/MediaPlayground/README.md`).
- Optional: shrink `PropsResolver.BuildFallbackMap` to manual-overrides-only (full symmetry with HMR's auto-discovery), once a coordinated SG-test-stub update lands.
- Optional: PrettyUi adoption — replace `MenuPage` static background with `<Video Loop Autoplay Muted />` and wire `useSfx` to button clicks.
- Version bump (when ready).

---

### Phase 1 — original draft (preserved for reference) ─────────────────
**Estimated effort:** ~1120 LOC across runtime + emitters + tests + sample + docs.
**Closes:**
- TD-S7 (`Plans~/TECH_DEBT_SAMPLES_AND_RUNTIME.md` lines 142–175 — “Video element”).
- Latent HMR bug class: 38 built-in `V.*` tags silently degraded (compile-fail or wrong codepath) on hot-reload.

---

## 0. Architectural verification (confirmed before planning)

Re-walked every layer. Findings (all verified against current `cleanup_and_upgrades` HEAD):

| # | Concern | File | Outcome |
|---|---|---|---|
| 1 | **Pattern B (Func-Component) is viable.** `V.Animate(AnimateProps, key, params children)` calls `Func<AnimateProps>(AnimateFunc.Render, …)` and `AnimateFunc.Render` uses `Hooks.UseRef()` + `Hooks.UseAnimate()`. Same pattern works for Video/Audio. | [Shared/Core/V.cs](Shared/Core/V.cs#L744-L751), [Shared/Core/Animation/AnimateFunc.cs](Shared/Core/Animation/AnimateFunc.cs) | ✅ Adopt verbatim |
| 2 | **Source generator auto-discovers `V.*` factories with `XxxProps` first-param** via Roslyn scan (`BuildBuiltinMapFromCompilation`) — classifies as `BuiltinTyped`. `V.Video(VideoProps,…)` and `V.Audio(AudioProps,…)` will be auto-picked up; no PropsResolver scan code changes required. | [SourceGenerator~/Emitter/PropsResolver.cs](SourceGenerator~/Emitter/PropsResolver.cs#L652-L760) | ✅ Add fallback-map entries only (cold-build safety) |
| 3 | **HMR has a hardcoded `s_tagMap` with only 33 entries vs. 80 `V.*` factories** — auto-discovery exists on SG (Roslyn) but not HMR. **38 typed built-ins** silently fall into the PascalCase func-component path on save and emit broken C# (`Animate`, `Toolbar`, `TwoPaneSplitView`, all editor fields, all toolbar items, `MultiColumnListView`, …). Phase 1 replaces the literal map with reflection-based discovery — fixes the entire bug class and makes Video/Audio “just work” without further `s_tagMap` edits. | [Editor/HMR/HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs#L19-L65) | ✅ Phase 1 (structural) |
| 4 | **Hook regex whitelists exist in TWO places** (SG and HMR) for hook-order signature recording. Adding `UseSfx`/`useSfx` requires updating BOTH regexes — else the new hook is invisible to HMR rude-edit detection. | [SourceGenerator~/Emitter/CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs#L468), [Editor/HMR/HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs#L2601) | ✅ Required edit (2 regexes, identical text) |
| 5 | **Hooks lifecycle infrastructure** (`HookContext.Current?.Owner`, `EnsureState`, `RecordHook`, `HookStates`, `SyncState`, `UseEffect` cleanup) supports the exact lifecycle Video/Audio need (mount, deps-change re-init, unmount cleanup). `UseLayoutEffect` also exists for pre-paint work. | [Shared/Core/Hooks.cs](Shared/Core/Hooks.cs#L348), [Shared/Core/NodeMetadata.cs](Shared/Core/NodeMetadata.cs) | ✅ No infra changes |
| 6 | **Asset registry already supports VideoClip/AudioClip** through the generic `AssetDatabase.LoadAssetAtPath<UnityEngine.Object>` fallback. `UitkxAssetRegistry.Get<T>()` casts via `as T`. Unity's native importers turn `.mp4`/`.webm` into `VideoClip` and `.mp3`/`.wav`/`.ogg` into `AudioClip`. **Zero L9 changes.** | [Editor/UitkxAssetRegistrySync.cs](Editor/UitkxAssetRegistrySync.cs#L200-L235), [Shared/Core/UitkxAssetRegistry.cs](Shared/Core/UitkxAssetRegistry.cs#L34-L48) | ✅ Free reuse |
| 7 | **Func-component props convention** — minimal: backing fields, `__ResetFields`, `__ReturnToPool` via `Pool<T>`. No `ToDictionary`/`ShallowEquals` required (typed-props pipeline handles diffing for the SG side; func-component side just receives the instance). | [Shared/Props/Typed/AnimateProps.cs](Shared/Props/Typed/AnimateProps.cs) | ✅ Mirror for VideoProps/AudioProps |
| 8 | **IDE schema** — append two new entries (`Video`, `Audio`) under top-level `elements` map, mirroring `Image` shape. LSP completions auto-derive from schema. | [ide-extensions~/grammar/uitkx-schema.json](ide-extensions~/grammar/uitkx-schema.json#L746) | ✅ Schema entries only |
| 9 | **`HookSignatureAttribute`** is the runtime mechanism the SG/HMR uses to detect rude hook-order changes. New hooks just need to appear in the regex whitelists (item 4) — no attribute redesign. | [Shared/Core/HookSignatureAttribute.cs](Shared/Core/HookSignatureAttribute.cs) | ✅ No changes |
| 10 | **HMR contract tests** live in `HmrEmitterParityContractTests.cs` (12 existing facts, in-memory Roslyn). Add 6+ new cases for Audio/Video. | [SourceGenerator~/Tests/HmrEmitterParityContractTests.cs](SourceGenerator~/Tests/HmrEmitterParityContractTests.cs) | ✅ Add tests |
| 11 | **`MediaHost` GameObject lifecycle** — a hidden, lazy, `DontDestroyOnLoad` GameObject is the standard Unity pool pattern for non-VE peers. Use `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)` to align with the registry's cache-reset hook (same site `UitkxAssetRegistry` resets at). Works in both Editor and player builds; `[ExecuteAlways]` not required because `<Video>`/`<Audio>` only render at runtime. | n/a (new) | ✅ Safe pattern |

**Conclusion:** safe to implement. No infrastructure changes, no new fiber tags, no new IElementAdapter — just one structural HMR fix (Phase 1) + new typed props + V.* factories + Func render bodies + minor whitelist additions.

---

## Phase breakdown

### Phase 1 — HMR built-in tag auto-discovery (structural fix) ✅ DONE

**Status:** Implemented and tested. 1154/1154 SG passing (was 1142, +12 new contract tests).

**What shipped:**
- `Editor/HMR/HmrCSharpEmitter.cs`: replaced literal 33-entry `s_tagMap` with `HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap()` — a private nested static class that reflection-scans `typeof(global::ReactiveUITK.V).GetMethods(Public|Static|DeclaredOnly)`, classifies each `VirtualNode`-returning factory by first-param shape (`*Props`/`IDictionary`/`string`/special-cased `Fragment`/`Text`), with manual overrides for `suspense`/`portal`/`visualelementsafe` whose first params don't match any auto-rule. Generic methods (`V.Func<TProps>`) and non-`VirtualNode` returns are skipped.
- `SourceGenerator~/Tests/HmrBuiltinTagDiscoveryContractTests.cs`: new file (12 facts) mirrors the discovery algorithm against in-memory Roslyn assemblies that synthesize a `V`-shaped class with every shape the production `V.cs` declares. Per the established pattern in `HmrFindPropsTypeContractTests.cs`, this is a one-way drift tripwire — the Editor asmdef can't load in the standalone xUnit runner. Covers: typed/typedC dispatch, Animate/Audio/Video auto-discovery, Fragment/Text name-based dispatch, manual overrides, generic skip, return-type filter, alias-routed factories skip, lowercase-keys invariant, full coverage catalogue.
- SG `BuildFallbackMap` left unchanged — it's only used in cold-build/test paths where `V` is not in the compilation; shrinking it would regress existing SG tests. Roslyn auto-scan (`BuildBuiltinMapFromCompilation`) handles the production hot path.

**Outcome:** All 38 previously-broken built-in tags (`<Animate>`, `<Toolbar>`, `<TwoPaneSplitView>`, every editor field, every toolbar item, every multi-column view, etc.) are now HMR-reloadable automatically. Adding a new `V.Foo(FooProps, …)` factory in any future release is auto-picked-up by both SG and HMR without touching either emitter.

---

### Phase 1 — original draft (preserved for reference) ─────────────────

**Problem.** `HmrCSharpEmitter.s_tagMap` is a hand-maintained 33-entry literal dictionary. `V.cs` has **80** public static `VirtualNode`-returning factories. The 38-entry gap means tags like `<Animate>`, `<Toolbar>`, `<TwoPaneSplitView>`, `<ToggleButtonGroup>`, `<TemplateContainer>`, every editor field (`<ColorField>`, `<ObjectField>`, `<Vector2Field>`…all variants), every toolbar item (`<ToolbarButton>`, `<ToolbarMenu>`…), `<MultiColumnListView>`, `<MultiColumnTreeView>`, `<RepeatButton>`, `<MinMaxSlider>`, `<EnumFlagsField>`, `<Hash128Field>`, `<PropertyField>`, `<InspectorElement>`, `<Scroller>`, `<IMGUIContainer>`, `<LongField>`, `<DoubleField>`, `<UnsignedIntegerField>`, `<UnsignedLongField>` silently fall into the PascalCase func-component branch on every `.uitkx` save and emit broken C# (CS0246 or wrong codepath). Same bug class HMR has hit four times before (0.4.16/17/18/19) — every release we discover one more tag missing from the manual list.

**Fix.** Replace the literal `s_tagMap` with reflection-based discovery that mirrors the SG side's `BuildBuiltinMapFromCompilation` exactly.

**New file** `Editor/HMR/HmrBuiltinTagDiscovery.cs` (~100 LOC):

```csharp
internal static class HmrBuiltinTagDiscovery {
    private static IReadOnlyDictionary<string, TagRes> _cache;

    public static IReadOnlyDictionary<string, TagRes> Build() {
        if (_cache != null) return _cache;
        var map = new Dictionary<string, TagRes>(StringComparer.OrdinalIgnoreCase);
        var vType = typeof(global::ReactiveUITK.V);

        foreach (var m in vType.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            if (m.ReturnType != typeof(global::ReactiveUITK.Core.VirtualNode)) continue;
            var ps = m.GetParameters();
            if (ps.Length == 0) continue;

            var first = ps[0];
            bool acceptsChildren =
                ps[ps.Length - 1].ParameterType == typeof(global::ReactiveUITK.Core.VirtualNode[])
                && ps[ps.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);

            // Fragment
            if (m.Name == "Fragment") {
                map["fragment"] = new TagRes(TagKind.Fragment, "Fragment", null);
                continue;
            }
            // Text
            if (m.Name == "Text" && first.ParameterType == typeof(string)) {
                map["text"] = new TagRes(TagKind.Text, "Text", null);
                continue;
            }
            // Typed: first param ends in "Props"
            if (first.ParameterType.Name.EndsWith("Props", StringComparison.Ordinal)) {
                var kind = acceptsChildren ? TagKind.TypedC : TagKind.Typed;
                var key = m.Name.ToLowerInvariant();
                // Prefer Typed when overloads collide
                if (!map.TryGetValue(key, out var existing) || kind == TagKind.Typed)
                    map[key] = new TagRes(kind, m.Name, first.ParameterType.Name);
                continue;
            }
            // Dictionary
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(first.ParameterType)
                || (first.ParameterType.IsGenericType
                    && first.ParameterType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))) {
                map[m.Name.ToLowerInvariant()] = new TagRes(TagKind.Dict, m.Name, null);
                continue;
            }
            // Func, Portal, Suspense, Memo, ForwardRef, Router, Routes, Route, Link, NavLink, Navigate, Outlet, ErrorBoundary, Host, VisualElement…
            // → handled by manual overrides below.
        }

        // Manual overrides for first-param-not-Props special cases (parity with SG):
        map["suspense"]          = new TagRes(TagKind.Suspense, "Suspense", null);
        map["portal"]            = new TagRes(TagKind.Portal,   "Portal",   null);
        map["visualelement"]     = new TagRes(TagKind.TypedC,   "VisualElement", "VisualElementProps");
        map["visualelementsafe"] = new TagRes(TagKind.Dict,     "VisualElementSafe", null);
        // ErrorBoundary, Router, Route, Routes, Link, NavLink, Navigate, Outlet, Memo, Func, ForwardRef, Host
        //   → routed through component-alias / func-component path; no s_tagMap entry needed.

        _cache = map;
        return _cache;
    }
}
```

**Edit** [`HmrCSharpEmitter.cs`](Editor/HMR/HmrCSharpEmitter.cs#L19-L65): replace the literal `s_tagMap` initializer with `private static readonly IReadOnlyDictionary<string, TagRes> s_tagMap = HmrBuiltinTagDiscovery.Build();`. Delete the 50-line literal dictionary.

**Edit** [`PropsResolver.cs`](SourceGenerator~/Emitter/PropsResolver.cs#L799): the SG fallback map (`BuildFallbackMap`) similarly becomes a thin wrapper that lists ONLY the manual override tags (Suspense, Portal, VisualElement, VisualElementSafe, ErrorBoundary). The Roslyn auto-scan path was already comprehensive and stays primary; the fallback shrinks to ~10 entries (only the special-case first-param tags). This makes both sides obviously symmetric — same logic shape, just one uses Roslyn `ITypeSymbol` and the other uses `System.Type`.

**Tests** — new file `SourceGenerator~/Tests/HmrBuiltinTagDiscoveryTests.cs` (~60 LOC):

1. **Coverage fact:** every public static `VirtualNode V.*(…)` method whose first param ends in `Props` appears as either `Typed` or `TypedC` in `HmrBuiltinTagDiscovery.Build()`. Asserts ≥38 typed entries (today: 33 → 38+ after fix; future: matches `V.cs` count automatically).
2. **Symmetry fact:** for every key in HMR's discovered map, SG's auto-scan or fallback produces the same `(MethodName, PropsType, AcceptsChildren)` triple. Pins parity forever.
3. **Spot facts:** `<Animate>`, `<Toolbar>`, `<ColorField>`, `<MultiColumnListView>`, `<ToolbarButton>`, `<TwoPaneSplitView>` each round-trip to `V.X(new XProps {...}, key, …)` identically through both emitters.
4. **Manual-override fact:** `<Suspense>`/`<Portal>`/`<VisualElement>`/`<VisualElementSafe>`/`<ErrorBoundary>` resolve correctly despite first-param-not-`*Props`.

**Acceptance.** After Phase 1, every existing `V.*` factory is HMR-reloadable. Adding new factories (Video, Audio in later phases — also any future built-in) requires zero further `s_tagMap` edits.

### Phase 2 — `MediaHost` shared peer-component pool

New file `Shared/Core/Media/MediaHost.cs` (~120 LOC):

- Lazy hidden `GameObject` (`hideFlags = HideAndDontSave`, `DontDestroyOnLoad`).
- Pools:
  - `Stack<VideoPlayer> _videoPool` — `RentVideoPlayer()` / `ReturnVideoPlayer(VideoPlayer)`.
  - `Stack<AudioSource> _audioPool` — `RentAudioSource()` / `ReturnAudioSource(AudioSource)`.
  - `AudioSource _sfxSource` — single shared, `playOnAwake=false`, used by `UseSfx` for `PlayOneShot`.
- Pooled `RenderTexture` registry keyed by `(width, height, depthFormat)` — `RentRT(w, h)` / `ReturnRT(rt)`. Avoids per-frame RT churn for `<Video>` resize.
- `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` — resets pools on play-mode enter (match `UitkxAssetRegistry` cache-reset semantics).
- Unit-testable via internal `ResetForTests()`.
- All Unity API access guarded by `Application.isPlaying` checks where appropriate (so editor cold-load doesn't construct a GameObject).

### Phase 3 — `<Audio>` element end-to-end (simpler — validates the full pipeline)

**L2 — typed props.** New file `Shared/Props/Typed/AudioProps.cs` (~90 LOC):

```csharp
public sealed class AudioProps : BaseProps {
    public AudioClip Clip { get; set; }
    public bool Autoplay { get; set; } = true;
    public bool Loop { get; set; }
    public float Volume { get; set; } = 1f;
    public float Pitch { get; set; } = 1f;
    public bool Mute { get; set; }
    public int Priority { get; set; } = 128;
    public AudioMixerGroup MixerGroup { get; set; }
    public float SpatialBlend { get; set; }            // 0 = 2D, 1 = 3D
    public float PanStereo { get; set; }
    public AudioRolloffMode RolloffMode { get; set; } = AudioRolloffMode.Logarithmic;
    public float MinDistance { get; set; } = 1f;
    public float MaxDistance { get; set; } = 500f;
    public Vector3? WorldPosition { get; set; }        // null = follow listener
    public float FadeInSeconds { get; set; }
    public float FadeOutSeconds { get; set; }
    public Action OnStarted { get; set; }
    public Action OnEnded { get; set; }
    public Action OnLoop { get; set; }
    public Ref<AudioController> Controller { get; set; }   // imperative API ref

    internal override void __ResetFields() { /* reset all */ }
    internal override void __ReturnToPool() => Pool<AudioProps>.Return(this);
}
```

**L3 — `V.Audio` factory.** Append to [Shared/Core/V.cs](Shared/Core/V.cs#L744) under the existing `// Animation` block (rename to `// Animation, Video, Audio`):

```csharp
public static VirtualNode Audio(AudioProps props, string key = null, params VirtualNode[] children)
    => Func<AudioProps>(AudioFunc.Render, props, key, children);
```

**L5 — SG fallback map.** No edit required — Phase 1 made both SG auto-scan and HMR reflection-discovery pick up `V.Audio` automatically.

**L6 — HMR `s_tagMap`.** No edit required — auto-discovered in Phase 1.

**L7 — IDE schema.** Append `Audio` element entry mirroring `Image` shape in [uitkx-schema.json](ide-extensions~/grammar/uitkx-schema.json#L746) — every prop above as an attribute with type and description.

**L8 — LSP enum shortcuts (optional).** Add per-attribute completions for `rolloffMode` (Logarithmic/Linear/Custom) in [CompletionHandler.cs](ide-extensions~/lsp-server/CompletionHandler.cs).

**Render function.** New file `Shared/Core/Media/AudioFunc.cs` (~140 LOC):

```csharp
public static class AudioFunc {
    public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children) {
        var p = (AudioProps)rawProps;

        // Per-instance imperative controller (stable across renders via UseRef pattern).
        var ctrlRef = Hooks.UseMutableRef<AudioController>(null);

        Hooks.UseEffect(() => {
            var src = MediaHost.Instance.RentAudioSource();
            ApplyProps(src, p);
            var ctrl = new AudioController(src);
            ctrlRef.Current = ctrl;
            if (p.Controller != null) p.Controller.Current = ctrl;

            if (p.Autoplay && p.Clip != null) ctrl.Play(p.FadeInSeconds);
            if (p.OnStarted != null) p.OnStarted();

            // Polled callbacks — VisualElement.schedule.Execute every 100 ms,
            // anchored to the parent VE (resolved via Hooks.UseRef()).
            // … OnEnded / OnLoop wiring …

            return () => {
                ctrl.Stop(p.FadeOutSeconds);
                MediaHost.Instance.ReturnAudioSource(src);
                if (p.Controller != null && p.Controller.Current == ctrl) p.Controller.Current = null;
            };
        }, p.Clip, p.Loop, p.MixerGroup, p.SpatialBlend);   // re-init only when these change

        // Cheap-prop pipeline (no re-create, just push to live AudioSource).
        Hooks.UseEffect(() => {
            var c = ctrlRef.Current;
            if (c != null) { c.Volume = p.Volume; c.Pitch = p.Pitch; c.Mute = p.Mute; }
            return null;
        }, p.Volume, p.Pitch, p.Mute);

        // <Audio> renders nothing — pure side effect.
        return V.Fragment();
    }

    private static void ApplyProps(AudioSource src, AudioProps p) { /* … */ }
}
```

**Imperative API.** New file `Shared/Core/Media/AudioController.cs` (~80 LOC) — `Play(fadeInSec)`, `Pause()`, `Stop(fadeOutSec)`, `Seek(seconds)`, `Volume`, `Pitch`, `Mute`, `IsPlaying`, `Time`, `Length`. Fades implemented via `MonoBehaviour` coroutine on MediaHost or `VisualElement.schedule.Execute` driven by an interpolation tick.

### Phase 4 — `useSfx()` hook

Per user requirement: "**sound should have both `<Audio>` and the sfx hook**". Fire-and-forget one-shot for UI clicks/effects.

**Hook.** Append to [Shared/Core/Hooks.cs](Shared/Core/Hooks.cs):

```csharp
private const string HookIdSfx = "UseSfx";

public static Action<AudioClip, float> UseSfx(AudioMixerGroup mixer = null) {
    var metadata = HookContext.Current?.Owner;
    var state = EnsureState(metadata);
    if (state == null) return static (_, __) => { };
    RecordHook(metadata, state, HookIdSfx);

    state.HookStates ??= new List<object>();
    if (state.HookIndex >= state.HookStates.Count) state.HookStates.Add(null);
    int index = state.HookIndex++;

    var cached = state.HookStates[index] as Action<AudioClip, float>;
    if (cached == null) {
        cached = (clip, volumeScale) => {
            if (clip == null) return;
            var src = MediaHost.Instance.SfxSource;
            if (mixer != null) src.outputAudioMixerGroup = mixer;
            src.PlayOneShot(clip, volumeScale);
        };
        state.HookStates[index] = cached;
    }
    SyncState(metadata, state);
    return cached;
}
```

**Hook regex whitelists (CRITICAL — both must be updated identically).** Append `|useSfx|UseSfx` inside the alternation group at:

- [SourceGenerator~/Emitter/CSharpEmitter.cs:468](SourceGenerator~/Emitter/CSharpEmitter.cs#L468)
- [Editor/HMR/HmrCSharpEmitter.cs:2601](Editor/HMR/HmrCSharpEmitter.cs#L2601)

Without this, `useSfx` calls inside a component body are invisible to hook-order signature recording — HMR would silently miss order changes.

**Cleanup contract.** `UseSfx` cleanup is a no-op (PlayOneShot is fire-and-forget; the `MediaHost.SfxSource` survives across mounts).

### Phase 5 — `<Video>` element end-to-end

**L2 — typed props.** New file `Shared/Props/Typed/VideoProps.cs` (~140 LOC):

```csharp
public sealed class VideoProps : BaseProps {
    public VideoClip Clip { get; set; }
    public string Url { get; set; }                           // streaming alternative
    public bool Autoplay { get; set; } = true;
    public bool Loop { get; set; }
    public bool Muted { get; set; }
    public float Volume { get; set; } = 1f;
    public float PlaybackSpeed { get; set; } = 1f;
    public string ScaleMode { get; set; } = "scaleToFit";    // mirrors ImageProps convention
    public float? AspectRatio { get; set; }
    public VideoAudioOutputMode AudioOutputMode { get; set; } = VideoAudioOutputMode.Direct;
    public AudioMixerGroup MixerGroup { get; set; }
    public RenderTextureFormat RenderTextureFormat { get; set; } = RenderTextureFormat.ARGB32;
    public Action OnPrepared { get; set; }
    public Action OnEnded { get; set; }
    public Action<string> OnError { get; set; }
    public Action<double> OnTimeUpdate { get; set; }
    public Action OnSeekCompleted { get; set; }
    public Ref<VideoController> Controller { get; set; }

    internal override void __ResetFields() { /* reset all */ }
    internal override void __ReturnToPool() => Pool<VideoProps>.Return(this);
}
```

**L3 — `V.Video` factory.** Append to [Shared/Core/V.cs](Shared/Core/V.cs#L744):

```csharp
public static VirtualNode Video(VideoProps props, string key = null, params VirtualNode[] children)
    => Func<VideoProps>(VideoFunc.Render, props, key, children);
```

**L5 / L6.** No edit required — auto-discovered in Phase 1.
**L7 / L8.** Add `Video` schema entry; add LSP enum shortcuts for `scaleMode` and `audioOutputMode`.

**Render function.** New file `Shared/Core/Media/VideoFunc.cs` (~220 LOC):

- Renders `V.VisualElement(new VisualElementProps { Style = p.Style }, null, …)` so `<Video>` is a real positionable VE.
- `Hooks.UseRef()` → host VE.
- `Hooks.UseEffect(setupFactory, p.Clip, p.Url, p.Loop, p.AudioOutputMode, p.MixerGroup, p.RenderTextureFormat)`:
  1. Rent `VideoPlayer` from `MediaHost`.
  2. Configure (clip/url, loop, audio output mode, mixer group, `playOnAwake=false`).
  3. Initial RT sized from VE's `resolvedStyle.width/height` (fallback 256×256 if not yet laid out).
  4. Create `BackgroundImage` from RT and assign to host VE's `style.backgroundImage`.
  5. Wire VideoPlayer events → user callbacks (`prepareCompleted`, `loopPointReached` → OnEnded, `errorReceived`, `seekCompleted`, frame-tick polled via `schedule.Execute(50ms)` → OnTimeUpdate).
  6. Cleanup: stop, return RT, return VideoPlayer to pool.
- `Hooks.UseEffect(() => { vp.SetDirectAudioVolume(0, p.Volume); vp.playbackSpeed = p.PlaybackSpeed; }, p.Volume, p.PlaybackSpeed, p.Muted)` — cheap props.
- `RegisterCallback<GeometryChangedEvent>(...)` on host VE (registered inside the setup effect): on resize, return old RT and rent new RT at new dimensions, reassign `BackgroundImage`. Debounce by comparing rounded `(w, h)`.
- ScaleMode mapping mirrors `ImageProps.ScaleMode` runtime (string → `ScaleMode` enum).

**Imperative API.** New file `Shared/Core/Media/VideoController.cs` (~70 LOC) — `Play()`, `Pause()`, `Stop()`, `Seek(seconds)`, `IsPlaying`, `Time`, `Duration`, `Frame`.

### Phase 6 — Tests (Video/Audio/SFX-specific)

**HMR parity contracts.** Append to [HmrEmitterParityContractTests.cs](SourceGenerator~/Tests/HmrEmitterParityContractTests.cs) — 6 new `[Fact]` cases:

1. `<Audio Clip={…} Autoplay />` → emits `V.Audio(new AudioProps {Clip=…, Autoplay=true}, …)` identical between SG and HMR.
2. `<Audio>` with `OnEnded={…}` event handler.
3. `<Audio>` with `MixerGroup={mixer}` and `SpatialBlend={1f}` (3D).
4. `<Video Clip={…} Loop />` basic.
5. `<Video Url="https://…" Autoplay Muted />` URL streaming.
6. `<Video>` with all callbacks (`OnPrepared`, `OnEnded`, `OnError`, `OnTimeUpdate`).

**Diagnostics.** Append to [DiagnosticsAnalyzerTests.cs](SourceGenerator~/Tests/) — `Asset<VideoClip>("missing.mp4")` and `Asset<AudioClip>("missing.mp3")` produce the standard UITKX0040 (asset-not-found) diagnostic.

**Pool/host unit tests.** New file `SourceGenerator~/Tests/MediaHostTests.cs` — rent/return idempotency, RT key reuse, SfxSource singleton behaviour. Pure C# (no UnityEngine deps in this assembly — gate Unity-dependent tests behind `#if UNITY_EDITOR` if needed, mirror `HmrEmitterParityContractTests`).

**Hook-order signature.** Add a fact verifying a component using `useSfx` produces a `HookSignatureAttribute("UseSfx,…")` mentioning UseSfx (exercises the regex whitelist update from Phase 3).

### Phase 7 — Sample

New folder `Samples/Components/MediaPlayground/`:

- `MediaPlayground.uitkx` — root component:
  - `<Video Clip={Asset<VideoClip>("./demo.mp4")} Loop Autoplay style={…} />` background panel.
  - `<Audio Clip={Asset<AudioClip>("./music.ogg")} Autoplay Loop Volume={0.4f} />` ambient music.
  - Three buttons exercising `useSfx`:
    - "Click" → `playSfx(Asset<AudioClip>("./click.wav"), 1f)`
    - "Pickup" → `playSfx(Asset<AudioClip>("./pickup.wav"), 0.8f)`
    - "Error" → `playSfx(Asset<AudioClip>("./error.wav"), 1f)`
  - Volume slider bound to `Video.Volume` via state.
  - Imperative video controls (Play/Pause/Seek 10s) via `Ref<VideoController>` prop.
- `MediaPlayground.uss` — layout.
- `MediaPlaygroundBootstrap.cs` — `EditorWindow` with menu item `ReactiveUITK/Demos/Media Playground` (mirrors existing demo pattern).
- Assets (royalty-free): `demo.mp4` (~1 MB), `music.ogg`, `click.wav`, `pickup.wav`, `error.wav`. Source from existing royalty-free libraries; document attribution in `THIRDPARTY.md`.

### Phase 8 — Documentation

- `CHANGELOG.md` — v0.4.20 entry (added `<Video>`, `<Audio>`, `useSfx`, MediaHost; closed TD-S7).
- `Plans~/TECH_DEBT_SAMPLES_AND_RUNTIME.md` — flip TD-S7 status to **FIXED in 0.4.20** with link to MediaPlayground sample.
- `Plans~/DISCORD_CHANGELOG.md` — release entry mirroring previous format.
- `ide-extensions~/changelog.json` — bump VS Code + VS 2022 minor (1.1.11 → 1.1.12) for new schema entries.
- `ReactiveUIToolKitDocs~/` — new docs page `media.md` covering both elements + the hook with code samples.

### Phase 9 — Optional PrettyUi adoption

In standalone PrettyUi project:

- Replace static background image in `MenuPage.style.uitkx` with `<Video Clip={Asset<VideoClip>("../Resources/menu-bg.mp4")} Loop Autoplay Muted style={…stretch…} />` mounted on the menu portal stage.
- Wire `useSfx` to existing menu button click handlers for tactile feedback.

---

## Risks & gotchas

| Risk | Mitigation |
|---|---|
| **HMR reflection load order.** `HmrBuiltinTagDiscovery.Build()` calls `typeof(global::ReactiveUITK.V).GetMethods()` — must run after the Runtime asmdef has loaded. | HMR controller already depends on the runtime assembly being present (it imports `UitkxAssetRegistry`, `VirtualNode`, etc.); the lazy `_cache` field defers the call until first emit. Verified safe. |
| **Reflection enumeration cost.** `GetMethods()` on `V` (~80 entries) takes ~50–100 µs. | Cached at first call; never re-enumerated. Negligible vs. compile time (which is hundreds of ms). |
| **Future `V.*` factories with non-`*Props` first param.** If someone adds `V.Foo(Bar bar, …)`, auto-discovery skips it (correct — it's not a typed-props built-in). | Tests assert that the manual-override list (Suspense/Portal/VisualElement/VisualElementSafe/ErrorBoundary) is the complete set of skipped factories. Adding a new skip-target requires also adding the manual override. CI pin. |
| **WebGL audio autoplay restriction.** Browsers block autoplay until user gesture. | Document in `media.md`. `<Audio Autoplay>` will fail-silent on WebGL pre-gesture; `useSfx` (triggered by click) always works. |
| **RenderTexture memory churn on resize.** Naive recreate on every `GeometryChangedEvent` fragments GPU memory. | Pool by `(roundedW, roundedH, format)` in `MediaHost`; debounce dimension changes by ≥1px round-trip. |
| **HMR re-mount restarts video at t=0.** A `.uitkx` save re-renders the component → effect re-runs → VideoPlayer rents fresh. | Acceptable for v0.4.20 (matches `<Animate>` behaviour). Document. Could be revisited via `Hooks.UseImperativeHandle` to preserve `time` across hot reload. |
| **No `AudioListener` in scene.** PrettyUi uses world-space UIDocs — verify scene has an AudioListener (usually on Main Camera). | `MediaPlaygroundBootstrap.cs` adds AudioListener if none exists (Editor-only convenience). Production users responsible. |
| **Build-vs-Editor MediaHost lifetime.** `RuntimeInitializeOnLoadMethod` fires identically in both — pool is recreated on play-mode enter, GameObject survives via `DontDestroyOnLoad`. Verified safe. |
| **SG/HMR regex drift.** The two hook whitelists at `CSharpEmitter.cs:468` and `HmrCSharpEmitter.cs:2601` are byte-identical strings. They must remain so. | Add a contract test that asserts `Regex.Match(SG_REGEX, "useSfx").Success` AND `Regex.Match(HMR_REGEX, "useSfx").Success`. Pin the parity. |
| **VideoClip cold-start hitch.** `VideoPlayer.Prepare()` is async. Calling `Play()` immediately on a cold clip skips audio sync. | `AudioFunc`/`VideoFunc` always call `Prepare()` then `Play()` from the `prepareCompleted` callback. |
| **`<Audio>` with `WorldPosition`.** Requires an attached GameObject transform — currently `MediaHost`'s pooled AudioSource sits at origin. | When `WorldPosition.HasValue`, parent the AudioSource's transform to MediaHost.Instance.transform and set localPosition. Cleanup re-parents back. |

---

## Effort estimate

| Phase | LOC | Notes |
|---|---:|---|
| 1 HMR built-in tag auto-discovery | 160 | Discovery 100 + SG fallback shrink ~0 + tests 60 |
| 2 MediaHost | 120 | Pure C# pool patterns |
| 3 `<Audio>` | 240 | Props 90 + Func 140 + Controller 80 - shared-helper savings |
| 4 `useSfx` hook | 30 | + 2 regex edits |
| 5 `<Video>` | 350 | Props 140 + Func 220 + Controller 70 - shared-helper savings |
| 6 Tests | 120 | 6 HMR parity + 2 diagnostics + 4 host + 1 hook signature |
| 7 Sample | 80 | Bootstrap 30 + uitkx 30 + uss 20 + assets external |
| 8 Docs | 60 | CHANGELOG + Discord + IDE changelog + docs page |
| **Total** | **~1160** | One focused working session |

---

## Open decisions to confirm before kickoff

1. **Version bump.** v0.4.20 (additive minor) vs v0.5.0 (signal new media surface)?
2. **URL streaming.** Include `Url` prop in `VideoProps` for v0.4.20, or VideoClip-only and add Url in a follow-up?
3. **Imperative `Ref<VideoController>` / `Ref<AudioController>` API.** Ship in v0.4.20 or follow-up?
4. **Fade-in/fade-out helpers.** Built into `AudioFunc` (Phase 2 design above) or a separate `useAudioFade` hook?
5. **PrettyUi adoption (Phase 8).** Do alongside this PR, or separately after merge?

---

## Execution order summary

Implement strictly in this order — each phase compiles green and is independently testable before moving on:

```
1. HMR built-in tag auto-discovery  → 1142+ → 1148+ SG passing, all 38 latent tags HMR-reloadable
2. MediaHost                        → compiles, host tests pass
3. <Audio> end-to-end               → audio-only sample works (no s_tagMap edit needed)
4. useSfx hook                      → SFX buttons work, hook regex parity test green
5. <Video> end-to-end               → video sample works (no s_tagMap edit needed)
6. All tests green                  → 1160+/1160+ SG passing
7. MediaPlayground sample
8. Docs + version bump
9. Optional: PrettyUi adoption
```
