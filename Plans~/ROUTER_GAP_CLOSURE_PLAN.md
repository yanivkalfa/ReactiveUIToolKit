# UITKX Router — Gap-Closing Plan vs React Router

> **Status (this revision):** Phases 0 → 4 implemented in a single PR. Phase 3.7
> (optional `:lang?` segments) and Phase 4.2 (static analyzer for ambiguous
> sibling Routes) are intentionally deferred — see the **Deferred** section at
> the bottom for the production-grade rationale.
>
> Companion to `Plans~/ROUTER_REACT_ROUTER_COMPARISON.md` (capability matrix and
> first-principles analysis). This document is an **execution plan**. Items are sorted
> by **gain ÷ risk** (highest first). Every task lists concrete files, LOC budget, the
> exact RR source it ports from, and the test/HMR/IDE/source-gen impact.

> Definitions used to score:
> - **Gain** = user-visible DX win + how often the gap is hit in real apps (1–5).
> - **Risk** = blast radius across runtime + source-gen + HMR + IDE + tests (1–5).
> - **Score** = `gain × (6 − risk)` (higher is better).

> Verified facts (re-confirmed before writing this plan):
> - `Hooks.ProvideContext` / `Hooks.UseContext` are the existing primitives (`Shared/Core/Hooks.cs:1488,1567`).
>   Keyed by string. No runtime change needed for any new context-using component.
> - `Samples/Components/RouterDemoFunc/RouterNavLink.uitkx` already exists as a
>   user-space NavLink. We will promote a first-class one to the library and keep the
>   sample as a "how to write your own" reference.
> - Source-gen alias map is in `SourceGenerator~/Emitter/PropsResolver.cs:53-60` and
>   duplicated in `Editor/HMR/HmrCSharpEmitter.cs:67-71` (DRY violation; called out in §X).
> - IDE schema is `ide-extensions~/grammar/uitkx-schema.json`. Rider + VS extensions
>   read the same file → schema additions cover all IDEs.
> - Fiber runtime: zero changes required for any item below.

---

## Phased Roadmap (Top-down by Gain/Risk)

### Phase 0 — Foundation cleanup (ship first; unlocks everything else)

| # | Task | Gain | Risk | Score | Status |
|---|---|---|---|---|---|
| 0.1 | De-dup the alias map between `PropsResolver` and `HmrCSharpEmitter` | 2 | 1 | 10 | ✅ Done |
| 0.2 | Make nested `<Router>` an explicit error (today silently shadows) | 2 | 1 | 10 | ✅ Done |

**0.1 — Alias map de-dup**
- Files: `SourceGenerator~/Emitter/PropsResolver.cs:53-60`, `Editor/HMR/HmrCSharpEmitter.cs:67-71`
- Action: extract `s_componentTagAliases` into a single `Shared/Core/RouterTagAliases.cs`
  static readonly map. Both consumers reference it.
- Why first: every subsequent item adds entries to this map. Doing this once means each
  later task touches **one** file instead of two.
- Tests: existing snapshot tests in `FormatterSnapshotTests.cs:9436+` keep passing.
- Risk: 1 (mechanical refactor, no behavior change).

**0.2 — Hard error on nested `<Router>`**
- File: `Shared/Core/Router/RouterComponents.cs` `RouterFunc.Render`
- Action: at top of `Render`, `if (Hooks.UseContext<RouterState>(RouterContextKeys.RouterState) != null) throw new InvalidOperationException("...")` (matches RR's `invariant(!useInRouterContext())` in `Router`).
- Why now: without it, every "I added Outlet but it doesn't update" bug report will be
  someone who accidentally double-wrapped. Cheap to add, expensive bug to chase later.
- Tests: 1 unit test asserting throw + a positive test that the existing
  `Samples/Components/RouterDemoFunc/RouterDemoFunc.uitkx` still renders.

---

### Phase 1 — Core composition primitives (the big unlock)

| # | Task | Gain | Risk | Score | Status |
|---|---|---|---|---|---|
| 1.1 | `<Outlet />` + layout-route support on `<Route>` | 5 | 2 | 20 | ✅ Done |
| 1.2 | `<NavLink>` first-class with `Active`/`End` styling | 5 | 1 | 25 | ✅ Done |
| 1.3 | `<Navigate to>` declarative redirect | 4 | 1 | 20 | ✅ Done |
| 1.4 | `useNavigate(path, { replace, state, relative })` options bag | 4 | 1 | 20 | ✅ Done |

#### 1.1 — `<Outlet />` + layout routes — **THE keystone**

This is the change the user demanded. Execute first inside Phase 1.

**RR source ported**: `react-router/lib/components.tsx::Outlet`,
`react-router/lib/hooks.tsx::useOutlet`, `_renderMatches` (the right-fold-into-context loop).

**Files / LOC budget**:
- `Shared/Core/Router/RouterContextKeys.cs` (+1 key, ~3 LOC)
  ```csharp
  public const string OutletElement = "__router_outlet_element";
  public const string OutletContext = "__router_outlet_context";
  ```
- `Shared/Core/Router/RouterComponents.cs`
  - Add `OutletFunc.Render` (~25 LOC): reads `OutletElement` from context, returns it
    (or `null` if no match). Optional `OutletContext` prop publishes a value for
    `RouterHooks.UseOutletContext<T>()`.
  - Add `OutletFuncProps` (~6 LOC): `public object Context { get; set; }`.
  - Modify `RouteFunc.Render` (~40 LOC delta):
    - When children include child `<Route>`s **and** `Element` is set, treat as a
      layout route:
      1. Walk `children` looking for `RouteFuncProps`-bearing VirtualNodes.
      2. Resolve each child route's full pattern (`RouterPath.Combine(ourPattern, childPath)`).
      3. First matching child wins (simple ordering — Phase 2 adds ranking).
      4. `Hooks.ProvideContext(OutletElement, matchedChild)` then return `Element`.
    - When `Element` is null, current Fragment behavior is preserved.
- `Shared/Core/Router/RouterHooks.cs` (+1 hook, ~10 LOC)
  ```csharp
  public static T UseOutletContext<T>() =>
      Hooks.UseContext<T>(RouterContextKeys.OutletContext);
  ```

**Source gen**: add `["Outlet"] = "OutletFunc"` to the (now-shared) alias map. ~1 LOC.

**HMR**: zero (hooks are standard `UseContext`/`ProvideContext`).

**IDE schema** (`uitkx-schema.json`):
```json
"Outlet": {
  "propsType": "OutletFuncProps",
  "description": "Renders the matching child route of the surrounding layout Route. ...",
  "acceptsChildren": false,
  "attributes": [
    { "name": "context", "type": "object",
      "description": "Value exposed to descendants via RouterHooks.UseOutletContext<T>()." }
  ]
}
```

**Tests** (`SourceGenerator~/Tests/` + new `Shared/Core/Router/Tests/`):
1. Single-level layout: `<Route path="/" element={<Layout/>}><Route path="a" element={<A/>}/></Route>` at `/a` → renders `<Layout><A/></Layout>` via `<Outlet/>`.
2. Multi-level: 3-deep nest, verify deepest match populates outer outlets.
3. No match: `<Outlet/>` returns null, no warning.
4. Param flow: parent `:id` accessible in child via `UseParams`.
5. `UseOutletContext<T>()` round-trip.
6. Sample: `Samples/Components/RouterOutletDemo/` (1 small uitkx + 1 editor window).

**Risk register specific to 1.1**:
- **Children inspection in RouteFunc must be Hooks-free.** The children walk happens
  before any `Hooks.ProvideContext` call and issues no hooks itself. Safe.
- **VirtualNode → props introspection.** Need to read each child's `IProps` and check
  `is RouteFuncProps`. This is already done by Fiber for prop diffing, so the API exists.
  Verify pattern in `Shared/Core/Fiber/`.
- **Memoization.** Wrap child-match scan in `Hooks.UseMemo` keyed on
  `(children, router.Location.Path, ourPattern)`. Same pattern `RouteFunc` already uses.

**Backward compat**: when `Element` is null OR children contain no `<Route>`s, behavior
is byte-identical to today. No existing app breaks.

---

#### 1.2 — `<NavLink>` first-class

**RR source ported**: `react-router/lib/dom/lib.tsx::NavLink` (the matching logic
specifically: lowercase compare unless `caseSensitive`, `end` flag, special-case `to="/"`).

**Files**:
- `Shared/Core/Router/RouterComponents.cs` add `NavLinkFunc` (~80 LOC):
  - Props: `To`, `Label`, `Replace`, `End` (bool, like RR's `end`), `CaseSensitive`,
    `Style` (base), `ActiveStyle`, `ChildrenFunc` (optional `Func<bool, VirtualNode>` for
    full render-prop parity).
  - Compute `isActive` exactly like RR:
    ```csharp
    string toPath = ResolvePath(To);
    string loc = router.Location.Path;
    if (!CaseSensitive) { toPath = toPath.ToLowerInvariant(); loc = loc.ToLowerInvariant(); }
    bool isActive = End
        ? string.Equals(toPath, loc, StringComparison.Ordinal)
        : loc == toPath || loc.StartsWith(toPath + "/", StringComparison.Ordinal);
    if (toPath == "/") isActive = string.Equals(loc, "/", StringComparison.Ordinal); // RR's "/" special case
    ```
  - Compose: `style = isActive ? StyleExtensions.Extend(Style, ActiveStyle) : Style`
    (in C#-land, just merge dictionaries).
  - Render as a `Button` like `LinkFunc` does today.

**Source gen**: alias `["NavLink"] = "NavLinkFunc"`.

**IDE schema**: full attribute set.

**Tests**:
- 4 isActive-truth-table entries from RR's NavLink JSDoc (the table at the
  `end={true}` doc).
- Case sensitivity test.
- `to="/"` non-active when at `/dashboard`.

**Migration note**: existing `Samples/Components/RouterDemoFunc/RouterNavLink.uitkx`
keeps working (it's a different name in user-space). Add a comment in that sample
pointing to the new built-in `<NavLink>`.

**Risk**: 1. Pure additive component, no router internals touched.

---

#### 1.3 — `<Navigate to>` declarative redirect

**RR source**: `react-router/lib/components.tsx::Navigate`.

**Files**:
- `Shared/Core/Router/RouterComponents.cs` add `NavigateFunc` (~30 LOC):
  ```csharp
  public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children) {
      var p = rawProps as NavigateFuncProps;
      var navigate = RouterHooks.UseNavigate(p?.Replace ?? false);
      Hooks.UseEffect(() => {
          navigate(p.To, p.State);
          return null;
      }, new object[] { p.To, p.Replace });
      return null;
  }
  ```
- Props: `To`, `Replace`, `State`.

**Source gen**: alias `["Navigate"] = "NavigateFunc"`.

**Why useful**: today `Pretty Ui`'s `HomePage` does the redirect-to-default-child via
a hand-rolled `useEffect`. With `<Navigate>`, that becomes
`<Route path="/" exact element={<Navigate to="/welcome" />} />`.

**Risk**: 1. Pure UseEffect wrapper.

---

#### 1.4 — `useNavigate` options bag

**RR source**: `useNavigateUnstable` in `react-router/lib/hooks.tsx`. Signature
`navigate(to, { replace, state, relative })`.

**Files**:
- `Shared/Core/Router/RouterHooks.cs` add overload:
  ```csharp
  public static RouterNavigateOptionsHandler UseNavigate() { ... }
  public delegate bool RouterNavigateOptionsHandler(string path, NavigateOptions opts = default);
  public struct NavigateOptions { public bool Replace; public object State; public string Relative; }
  ```
  Old `UseNavigate(bool replace = false)` stays for backward compat; document new
  signature as preferred.
- `Relative = "path"` mode mirrors RR (resolves `..` against URL segments instead of
  matched-route patterns). Implementable by switching the base used in
  `RouterPath.Combine`.

**Risk**: 1. Additive; old delegate preserved.

---

### Phase 2 — Ranking + first-match-wins switch

| # | Task | Gain | Risk | Score | Status |
|---|---|---|---|---|---|
| 2.1 | Port RR's `flattenRoutes` + `rankRouteBranches` to UITKX | 4 | 3 | 12 | ✅ Done (`RouteRanker.cs`) |
| 2.2 | Add `<Routes>` first-match-wins component | 4 | 2 | 16 | ✅ Done |
| 2.3 | Add **index routes** (`<Route index>` shortcut) | 3 | 2 | 12 | ✅ Done |
| 2.4 | Add **pathless prefix Route** (no element, just children) | 2 | 1 | 10 | ✅ Done (covered by ranker + Outlet flow) |

#### 2.1 — Ranking algorithm port

**RR source**: `lib/router/utils.ts::flattenRoutes`, `rankRouteBranches`,
`computeScore`. RR uses these constants:
- `paramRe = /^:\w+$/`
- `staticSegmentValue = 10`
- `dynamicSegmentValue = 3`
- `splatPenalty = -2`
- `indexRouteValue = 2`
- `emptySegmentValue = 1`
Score = sum over segments + (`isIndex ? indexRouteValue : 0`).

**Files**:
- New `Shared/Core/Router/RouteRanker.cs` (~150 LOC, direct port of the three
  functions). Pure static. No Hooks.
- `Shared/Core/Router/RouteMatcher.cs` consumed only by current single-Route path; no
  change to existing call sites.

**Tests**: copy RR's ranking tests verbatim
(`packages/react-router/__tests__/path-matching-test.tsx`).

**Risk 3 reasoning**: medium because the algorithm is non-obvious and edge cases
(splat scoring, empty segments, index priority) are subtle. Mitigated entirely by
porting the test fixtures.

---

#### 2.2 — `<Routes>` first-match-wins

**RR source**: `react-router/lib/components.tsx::Routes` → `useRoutes` →
`matchRoutes(branches)` → `_renderMatches`.

**Files**:
- `Shared/Core/Router/RouterComponents.cs` add `RoutesFunc` (~80 LOC):
  - On render, walk children VirtualNodes (no Hooks calls), build
    `List<RouteBranch>` via `RouteRanker.FlattenRoutes`, rank, pick first match.
  - Render the matched chain by injecting `OutletElement` context downward (reuse the
    Phase 1.1 plumbing).
- Optional `Location` prop to override the matching location.

**Source gen alias**: `["Routes"] = "RoutesFunc"`.

**IDE schema**: entry with `acceptsChildren: true`.

**Tests**: ambiguity test that today fails — two sibling Routes `path="/"` (non-exact)
and `path="/about"` — under `<Routes>` only one renders; outside `<Routes>` both fire
(today's behavior, regression-pinned).

**Risk 2**: depends on 1.1 (Outlet) and 2.1 (ranker). If those are correct, this is
glue.

---

#### 2.3 — Index routes (`<Route index>`)

**RR source**: `IndexRouteObject`. An `index` route renders into its parent's outlet
at the parent's exact URL.

**Files**:
- `Shared/Core/Router/RouterComponents.cs` add `Index` bool to `RouteFuncProps`. When
  `Index = true`:
  - `Path` must be unset → otherwise diagnostic.
  - Effective pattern = parent's pattern, exact = true.
  - When matched, populates parent's outlet.

**Source gen**: schema attribute + a UITKX-Rxxx analyzer warning if both `Index` and
`Path` are set.

**Risk 2**: needs the Phase 1.1 layout-route mechanism + Phase 2.1 ranker (index gets a
+2 boost).

---

#### 2.4 — Pathless prefix Route

When `<Route path="/foo">` has no element and only Route children, it acts as a
namespace prefix. Already partially works today (children render as Fragment, child
patterns are combined with parent). Phase 2.4 just removes the
"can't combine path-prefix + Outlet" awkwardness by ensuring `<Routes>` matcher treats
pathless+children correctly.

**Risk 1**: covered by ranker tests.

---

### Phase 3 — Higher-DX hooks (independent of Phases 1–2)

| # | Task | Gain | Risk | Score | Status |
|---|---|---|---|---|---|
| 3.1 | `useMatches()` flat chain | 3 | 2 | 12 | ✅ Done |
| 3.2 | `useResolvedPath(to, opts)` | 2 | 1 | 10 | ✅ Done |
| 3.3 | `useSearchParams()` setter (mutating tuple) | 3 | 2 | 12 | ✅ Done |
| 3.4 | `usePrompt(when, message)` convenience over `UseBlocker` | 2 | 1 | 10 | ✅ Done |
| 3.5 | `basename` prop on `<Router>` | 2 | 2 | 8 | ✅ Done |
| 3.6 | `caseSensitive` per-route flag | 2 | 2 | 8 | ✅ Done |
| 3.7 | Optional segments (`:lang?`) | 3 | 4 | 6 | ⏸ Deferred — see bottom |

#### 3.1 — `useMatches()`
- Returns the full ancestor chain as `IReadOnlyList<RouteMatch>`. Required for
  breadcrumbs, debug overlays, dev tools.
- Implementation: walk `RouteContextEntry.Parent` chain (already exists). Return
  reversed list.
- Files: `RouterHooks.cs` (+15 LOC), `RouterContextKeys.cs` no change.
- Risk 2: depends on Outlet's matched-chain context being populated on the way down.

#### 3.2 — `useResolvedPath`
- Pure helper over `RouterPath.Combine`. ~10 LOC.

#### 3.3 — `useSearchParams` setter
- Returns `(IReadOnlyDictionary<string,string>, Action<IDictionary<string,string>>)`.
- Setter calls `router.Navigate("/" + path + "?" + querystring)` or `Replace`.
- Files: `RouterHooks.cs` (+30 LOC), needs a `RouterPath.BuildQuery` helper (+15 LOC).

#### 3.4 — `usePrompt`
- Convenience: `UseBlocker(when)` + a callback that calls `EditorUtility.DisplayDialog`
  (in editor) or a user-supplied confirm callback (in player).
- ~25 LOC.

#### 3.5 — `basename`
- Storage on `RouterState`. Strip from incoming locations, prepend on outgoing
  navigations. `LinkFunc` and `NavLinkFunc` consult it via context.
- Risk 2: needs care to not double-prepend.

#### 3.6 — Per-route `CaseSensitive`
- New bool on `RouteFuncProps`. Threaded into `RouteMatcher.Match` (currently always
  `OrdinalIgnoreCase`).
- Risk 2: need to re-run all router snapshot tests after the matcher signature change.

#### 3.7 — Optional segments (`?` suffix)
- RR uses `explodeOptionalSegments` to expand `:lang?/x` into two patterns
  (`x` and `:lang/x`) at flatten time.
- Risk 4: subtle interaction with ranking (RR has a comment about always keeping
  required-version-first to avoid score inversion). If we ship this, we must port
  RR's exploded ordering tests.

---

### Phase 4 — Diagnostics & analyzer rules (low-risk polish)

| # | Task | Gain | Risk | Score | Status |
|---|---|---|---|---|---|
| 4.1 | `<Outlet/>` outside any `<Router>` | 2 | 1 | 10 | ✅ Done (runtime warning in `OutletFunc.Render`) |
| 4.2 | Ambiguous sibling `<Route>` patterns (encourage `<Routes>`) | 3 | 2 | 12 | ⏸ Deferred — needs language-lib AST analyzer |
| 4.3 | `<Route index>` with `Path` set | 2 | 1 | 10 | ✅ Done (runtime `InvalidOperationException` in `RouteFunc.Render`) |
| 4.4 | nested `<Router>` (pairs with 0.2's runtime check) | 1 | 1 | 5 | ✅ Done (runtime `InvalidOperationException` in `RouterFunc.Render`) |

The runtime checks (4.1, 4.3, 4.4) make every misuse loud and immediate. The static
analyzer rule (4.2) requires walking the .uitkx AST to discover sibling `<Route>`
declarations and is appropriately scoped to a follow-up PR in `ide-extensions~/
language-lib/Diagnostics/DiagnosticsAnalyzer.cs`. Until that lands, applications
that want strict first-match-wins semantics should wrap their routes in `<Routes>`
— which is now the recommended pattern.

---

### Deferred items (intentionally not in this PR)

Two tasks from Phases 3 and 4 were intentionally held back from the current
implementation pass. The rationale below is recorded so the trade-off is auditable
later and so the work isn't accidentally re-litigated.

| Task | Why deferred (production-grade reasoning) |
|---|---|
| **3.7 — Optional segments (`:lang?`)** | Score 6 (lowest in Phase 3). Implementing this requires porting RR's `explodeOptionalSegments`, which expands a single pattern into 2ⁿ patterns at flatten time and forces the ranker to keep "required-version-first" ordering. The interaction with the existing `RouteRanker.Pick` is subtle (RR has explicit comments warning about score inversion) and the surface to test is large. None of the audited consumer apps (RouterDemoFunc, MainMenuRouterDemoFunc, Pretty Ui) currently need it; adding it later is purely additive in `RouteMatcher` + `RouteRanker`. |
| **4.2 — Static analyzer for ambiguous sibling Routes** | The right home for this rule is `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`, which would need a new AST-walking pass that identifies sibling `<Route>` elements at any markup depth and re-uses `RouteRanker.ComputeScore` to detect score collisions. Implementing it well (without false positives across `<Routes>`-wrapped collections, fragments, and `@if`-conditional Routes) is a self-contained ~150 LOC analyzer + tests effort that is best shipped on its own once user reports validate the noise/signal ratio. The runtime semantics it would warn about already work — the new `<Routes>` component (Phase 2.2) gives users a direct, in-library escape hatch. |

---

### Phase 5 — Out-of-scope items (document-and-defer)

These are React Router features that **don't make sense in a Unity UI router**. Listed
so they're not re-asked.

| Feature | Why deferred |
|---|---|
| Loaders / actions / `useLoaderData` | Assumes a request/response model with promises; conflicts with current Hooks scheduler model and adds a parallel data graph. Right answer for Unity is "use your own data layer, expose via context." Re-evaluate only if the team adds an async data primitive to Hooks. |
| `<Form>` / `useFetcher` / `useSubmit` | Same — predicated on form-submit→server roundtrip. |
| `lazy: () => import(...)` | Unity assemblies aren't lazy in the JS sense. AssetBundles are an option but a different abstraction. |
| `patchRoutesOnNavigation` ("Fog of War") | Tied to lazy. Defer with lazy. |
| `errorElement` per route | Tractable (Fiber has error paths) but doubles router complexity. Add if/when there's a concrete user demand. |
| RSC, view transitions, `flushSync` | DOM/Server-only. |
| Browser/Hash history | Unity has no URL bar. MemoryHistory is the right primitive. |

---

## Cumulative Cross-Layer Impact (after Phase 1+2 ship)

| Layer | Net change |
|---|---|
| Runtime (Hooks/Fiber) | **0 LOC**. All new components compose existing primitives. |
| `Shared/Core/Router/` | +~700 LOC (5 new components, 1 ranker, 1 helper hook bag) |
| Source generator | +~10 LOC in the (now-shared) alias map. **Zero parser/AST changes.** |
| HMR | +~3 LOC (alias map already shared after 0.1) |
| IDE schema | +~250 LOC of JSON. **Zero TypeScript changes.** Rider/VS inherit. |
| Diagnostics | +~80 LOC per analyzer rule (4 rules in Phase 4) |
| Tests | +~600 LOC (porting RR's ranking + match tests + ~10 new UITKX tests) |
| Samples | +1 sample (`RouterOutletDemo`) |
| Docs | `CHANGELOG.md`, `ide-extensions~/changelog.json`, `Plans~/MIGRATION_GUIDE.md` |

**No breaking changes** anywhere in the plan. Every existing app, sample, and test
continues to behave identically because the new shapes are activated only when new
props/components are used.

---

## Recommended Release Bundling

- **0.x.0 release "router foundation"**: Phase 0 (cleanup) + Phase 1 (Outlet + NavLink
  + Navigate + useNavigate options).
  - Smallest unit that closes the user's stated complaint and improves DX immediately.
  - Patch-level migration note: "If you wrote your own NavLink, you can keep it or
    migrate to the built-in `<NavLink>`."
- **0.x+1.0 release "router switch + ranking"**: Phase 2 (Routes + ranker + index +
  prefix).
  - Adds composability for large apps. Touches ranking, so deserves its own beat.
- **0.x+2.0 release "router DX bundle"**: Phase 3 hooks (subset; pick the ones with
  active demand).
- **Phase 4 analyzers**: ship piecewise after each runtime feature lands (analyzer
  follows the feature it guards).

## Risk Mitigations Common to All Phases

1. **Snapshot tests as the contract.** `FormatterSnapshotTests.cs` already has
   `Router`-vs-`Route` regression guards. Mirror those for every new component
   (`<Outlet path=…>` should never round-trip to `<Outlets>` or anything weird).
2. **HMR golden samples.** After 1.1, add `Samples/Components/RouterOutletDemo/` and
   include it in HMR golden regen. After 2.2, add a `<Routes>` golden.
3. **No public API renames.** `RouterFunc`, `RouteFunc`, `LinkFunc` keep their names
   and signatures forever. New components add; nothing renames.
4. **Diagnostic rollout** (Phase 4) is opt-in `Info` severity for one release, then
   promoted to `Warning`. Avoids breaking analyzers-as-errors CI configurations.

---

## TL;DR Execution Order (one-line each)

1. **0.1** Share alias map between source-gen and HMR.
2. **0.2** Throw on nested `<Router>`.
3. **1.1** Ship `<Outlet/>` + layout-route Element+children co-existence in `RouteFunc`.
4. **1.2** Ship `<NavLink End CaseSensitive ActiveStyle>`.
5. **1.3** Ship `<Navigate to>`.
6. **1.4** Ship `useNavigate(path, opts)` overload.
7. **2.1** Port RR ranker.
8. **2.2** Ship `<Routes>` first-match-wins.
9. **2.3** `<Route index>` shortcut.
10. **2.4** Pathless prefix sanity.
11. **3.1–3.6** Pick & ship by demand.
12. **3.7** Optional segments (only if asked — highest implementation risk).
13. **Phase 4** Analyzer rules.

Stop after each phase, ship, gather feedback. Every phase is independently shippable
and additive.
