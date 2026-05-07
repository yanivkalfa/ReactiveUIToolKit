# UITKX Router vs React Router DOM — Deep Comparison & Gap Analysis

> Source basis
> - **UITKX router**: every file under `Shared/Core/Router/` (9 source files, ~700 LOC total)
>   read in full: `RouterComponents.cs`, `RouteMatcher.cs`, `RouterHooks.cs`, `RouterPath.cs`,
>   `MemoryHistory.cs`, `RouterState.cs`, `RouterLocation.cs`, `RouteMatch.cs`,
>   `IRouterHistory.cs`, `RouterContextKeys.cs`.
> - **React Router**: v7 declarative + data modes, source files
>   `packages/react-router/lib/components.tsx`, `lib/hooks.tsx`, `lib/router/utils.ts`
>   (≈ 6,250 LOC combined), plus the public docs for `Routes`, `Outlet`, `createBrowserRouter`,
>   declarative routing, and data routing.
> - **UITKX impact surface searched**: `SourceGenerator~/Emitter/PropsResolver.cs`,
>   `SourceGenerator~/Emitter/CSharpEmitter.cs`, `Editor/HMR/HmrCSharpEmitter.cs`,
>   `ide-extensions~/grammar/uitkx-schema.json`, `ide-extensions~/vscode/src/extension.ts`.

This document is the verdict. Recommendations are at the bottom.

---

## 1. Capability Matrix

Legend: ✅ supported · ⚠ partial / workaround · ❌ missing

| # | Capability | RR (v6/v7) | UITKX | Notes |
|---|---|---|---|---|
| 1 | Single `<Router>` per tree | ✅ | ✅ | Both invariant: nesting routers is forbidden |
| 2 | `<Route path>` with relative segments | ✅ | ✅ | UITKX `RouterPath.Combine(parent, child)` mirrors RR |
| 3 | Param segments (`:id`) merged from parent | ✅ | ✅ | `RouteMatcher.MergeParameters` |
| 4 | `<Route exact>` / leaf-only match | ✅ (auto via end) | ✅ (`exact` prop) | Different mental model; see §2.A |
| 5 | Splat / wildcard (`/*`, `*`) | ✅ | ✅ | Both consume "rest" |
| 6 | **`<Outlet />` for layout routes** | ✅ | ❌ | **Critical gap — see §3** |
| 7 | **`<Routes>` first-match-wins switch** | ✅ | ❌ | **Critical gap — see §3** |
| 8 | **Layout routes (`<Route element={X}>` w/ no path)** | ✅ | ❌ | Tied to Outlet |
| 9 | **Index routes (`index`)** | ✅ | ❌ | Tied to Outlet |
| 10 | **Pathless prefix routes** | ✅ | ❌ | Tied to Routes |
| 11 | **Route ranking / specificity scoring** | ✅ | ❌ | All siblings match independently |
| 12 | **Optional segments (`:lang?`)** | ✅ | ❌ | Single `?` suffix unsupported |
| 13 | Case-sensitive matching | ✅ (per route) | ❌ | UITKX is always case-insensitive |
| 14 | `useNavigate(to, { replace, state, relative })` | ✅ | ⚠ | UITKX has `replace` flag at hook construction, no per-call options bag, no `relative: "path"` mode |
| 15 | `useLocation()`, `useParams()`, `useMatches()` | ✅ | ⚠ | UITKX has `UseLocation/UseParams/UseRouteMatch`; **no `useMatches()` flat list** |
| 16 | `useResolvedPath` / `useHref` | ✅ | ❌ | Not exposed |
| 17 | `<Link>` / `<NavLink>` with `isActive` | ✅ | ⚠ | UITKX has `LinkFunc` (rendered as Button), **no NavLink active styling helper** |
| 18 | `useSearchParams()` setter | ✅ | ❌ | UITKX `UseQuery` is read-only |
| 19 | History blockers / `useBlocker` | ✅ | ✅ | `RouterHooks.UseBlocker` + `MemoryHistory.RegisterBlocker` |
| 20 | `usePrompt` / `unstable_usePrompt` | ✅ | ❌ | Convenience over blocker |
| 21 | Loaders / actions / `useLoaderData` | ✅ (data mode) | ❌ | Out of scope for UI-toolkit, but worth noting |
| 22 | `<Form>` / `useFetcher` / `useSubmit` | ✅ (data mode) | ❌ | Same — server/data concept |
| 23 | `errorElement` / `<ErrorBoundary>` per route | ✅ (data mode) | ❌ | Could be added cheaply (§5) |
| 24 | `useNavigation()` pending state | ✅ (data mode) | ❌ | Async-only relevance |
| 25 | `lazy` / route code-splitting | ✅ | ❌ | Not really applicable; Unity assemblies aren't lazy |
| 26 | `patchRoutesOnNavigation` ("Fog of War") | ✅ | ❌ | Same |
| 27 | Scroll restoration | ✅ (`<ScrollRestoration>`) | ❌ | Could add a `<ScrollRestore>` for UITK ScrollViews |
| 28 | `viewTransition` / `flushSync` | ✅ DOM only | ❌ | DOM-only; n/a |
| 29 | `useOutletContext` | ✅ | ❌ | Tied to Outlet |
| 30 | Browser/Hash/Memory history strategies | ✅ × 3 | ⚠ × 1 | UITKX has Memory only (sufficient for Unity) |
| 31 | Programmatic `<Navigate />` component | ✅ | ⚠ | Achievable via `Hooks.UseEffect(() => navigate(...))`, no first-class component |
| 32 | `basename` prop on `<Router>` | ✅ | ❌ | Trivial to add |
| 33 | Per-route `id` / `useRouteLoaderData(id)` | ✅ | ❌ | Tied to data router |
| 34 | `defer()` / `<Await>` / `useAsyncValue` | ✅ | ❌ | Promise-based, n/a in current Hook model |
| 35 | RSC (`unstable_*` server components) | ✅ | ❌ | DOM/server only |

---

## 2. Where UITKX matches RR

### 2.A. Parent → child path composition
`RouterComponents.cs:109` does `RouterPath.Combine(parentMatch.Pattern, path)` — this is the
exact behavior of RR's `joinPaths([parentPath, meta.relativePath])` in `flattenRoutes`.
A child `<Route path="welcome">` inside a matched `<Route path="/home/*">` resolves to
`/home/welcome`. ✅

### 2.B. Param merging
`RouteMatcher.MergeParameters` walks `parent.Parameters` then overrides with current — RR
does `Object.assign({}, parentParams, match.params)` in `useRoutes`. Same semantics.

### 2.C. History / blockers
`MemoryHistory` is API-equivalent to RR's `createMemoryHistory`: push/replace/go, listen,
and a blocker chain that vetoes transitions. RR additionally has `Action`
(`PUSH/POP/REPLACE`) tracking — UITKX does not expose nav action.

### 2.D. Single-router invariant
RR throws if you nest `<Router>`. UITKX doesn't *throw*, but
`RouterContextKeys.RouterState` resolves nearest-up so a nested router silently shadows
the outer (worse — silent wrong behavior). Should be hardened.

---

## 3. Critical gaps (the Outlet story)

### 3.A. Why `<Outlet />` exists in RR

In RR, `useRoutes` (called by `<Routes>`) calls `matchRoutes(routes, location)` which:

1. **Flattens** the route tree into "branches" (each branch is one root→leaf path).
2. **Scores** each branch with `computeScore(path, isIndex)` so static segments outrank
   dynamic, longer outranks shorter, indexes outrank empty.
3. **Sorts** descending and **picks the first branch whose path matches** the location.
4. Returns the **chain of matches** from root to leaf.

`_renderMatches` then folds the matches *right-to-left* (deepest first). Each fold step
sets a `RouteContext` whose `outlet` is the result of the deeper fold. `<Outlet />`
simply reads `useContext(RouteContext).outlet` and renders it.

Net effect:
```jsx
<Route path="/" element={<App/>}>            // App renders <Outlet/>
  <Route path="dashboard" element={<Dash/>}>  // Dash renders <Outlet/>
    <Route path="settings" element={<Settings/>}/>
  </Route>
</Route>
```
At `/dashboard/settings` you render `<App><Dash><Settings/></Dash></App>` from a *single
declarative tree*. No repetition, no parent-rule "match everything" hack.

### 3.B. Why UITKX cannot do this today

`RouteFunc.Render` (`RouterComponents.cs:88-160`):

- Does not enumerate sibling/child Routes.
- Each `<Route>` resolves its own match against `router.Location.Path` independently.
- If `Element` is set, returns `Element` and **discards children**
  (`RouterComponents.cs:147-156`). So `<Route element={<Layout/>}><Route .../></Route>`
  silently drops the inner routes.
- If `Element` is null, returns children as Fragment. Inner `<Route>`s then run their
  own (relative) match. That's how UITKX gets the appearance of nested routes — only the
  inner Route is "real"; the outer route is just a wrapper and *its own element-vs-children
  is mutually exclusive*.

There is no flatten/score/select pipeline. There is no shared "matched chain" context
that an `<Outlet/>` could read from. All siblings match independently with no ranking,
so a parent `path="/"` non-exact route would also match `/game` (because `RouteMatcher`
returns success when `matchSegmentCount = 0`) and double-render any layout chrome.

### 3.C. The user-visible cost (concrete from `Pretty Ui`)

```jsx
<Router initialPath="/home/welcome">
  <Route path="/game" exact={true} element={<GamePage/>}/>
  <Route path="/home/*">     <MenuPage><HomePage/></MenuPage>     </Route>
  <Route path="/settings/*"> <MenuPage><SettingsPage/></MenuPage> </Route>
  <Route path="/news/*">     <MenuPage><NewsPage/></MenuPage>     </Route>
</Router>
```

- Layout (`<MenuPage/>`) is duplicated N times, one per top-level menu section.
- Adding a section is two edits (add Route + add Page) in two files instead of one.
- Cannot have an `index` route ("default child of /home"); requires either a redirect-on-mount
  effect or wildcard-fallback hackery.
- Cannot have pathless-prefix grouping (`<Route path="settings"><sub-routes/></Route>`
  *without* Settings owning a wrapper).
- Sibling Routes are non-exclusive: it is *very* easy to write two routes that both match
  the same URL and both render. RR's `<Routes>` makes this impossible by construction.

---

## 4. Performance impact of adding the missing primitives

Per render cycle on a route change, today's UITKX does:

```
N * RouteMatcher.Match()   // N = number of <Route> siblings under <Router>
```

`RouteMatcher.Match` is O(P) on path segment count, allocates a `string[]` for splits and
an optional `Dictionary<string,string>` only if params exist. UseMemo gates re-runs.

Adding **flatten + rank + first-match-wins** (the RR pipeline):

| Phase | Cost (UITKX context) | Frequency |
|---|---|---|
| Flatten route tree | O(routes) once per tree shape | Per-render of `<Routes>`, but route tree shape is stable → easily memoized to **once** |
| Score branches | O(branches × segments) | Same — memoized with tree |
| Sort branches | O(b log b) | Same |
| Match selected branch | O(matched-depth × segments) | Per location change |
| Fold matches into `<Outlet/>` chain | O(matched-depth) component renders | Per location change |

For a *typical* UITKX app (≤ 50 routes, depth ≤ 4):
- Flatten + score + sort: a few dozen string ops, allocates ~50 small objects, ~0.1ms.
- Memoized to once per route-tree change → effectively free after warm-up.
- Per navigation: O(d) matcher calls instead of O(N) sibling scans → **strictly faster**
  than current code once N > d (which is almost always true for nested apps).

Memory: the matched-chain array is tiny (depth ≤ 4 typically). One extra context entry per
nesting level. Negligible.

**Verdict**: Adding `<Outlet/>` + `<Routes>` is **a net performance win** on any
non-trivial route table, because today every sibling Route runs its own matcher every
render, while RR-style runs the matcher once over the (memoized) flattened branch list
and short-circuits on first hit.

The only price is a few new context provides per matched depth. Hooks already
provide/consume contexts efficiently in the Fiber runtime.

---

## 5. Cross-layer impact analysis

The UITKX router primitives flow through these subsystems. For each, here's what an
Outlet/Routes addition would touch.

### 5.A. Source generator (`SourceGenerator~/`)

Affected file: **`Emitter/PropsResolver.cs`**, lines 53–60.

```csharp
private static readonly Dictionary<string, string> s_componentTagAliases = new(...) {
    ["Router"] = "RouterFunc",
    ["Route"]  = "RouteFunc",
    ["Link"]   = "LinkFunc",
    // NEW:
    ["Outlet"] = "OutletFunc",
    ["Routes"] = "RoutesFunc",
    ["NavLink"] = "NavLinkFunc",   // for §6
    ["Navigate"] = "NavigateFunc", // declarative redirect
};
```

That's the ONLY change to the generator pipeline. New components are emitted exactly like
`Router`/`Route`/`Link` — no parser, AST, or analyzer changes. **No new diagnostics
codes** required. The Roslyn analyzers already handle "unknown tag" gracefully.

Impact: **trivial**, ~5 LOC.

### 5.B. HMR (`Editor/HMR/HmrCSharpEmitter.cs`)

Same alias dictionary is duplicated at line 67–71 (this is a known DRY violation worth
filing as a separate cleanup). Same one-liner additions.

HMR-specific concerns:
- **Outlet uses context to publish/consume the matched-chain entry.** Because Hooks
  contexts are keyed by string (`RouterContextKeys.*`), HMR re-mount will see fresh
  contexts each render — same as today. Nothing to special-case.
- HMR's `UitkxHmrCompiler.cs` re-routes hook delegates; Outlet has no special hook
  signature, so it inherits the standard delegate-swap behavior. No HMR risk.

Impact: **trivial**, ~5 LOC + retest the hot-reload golden samples that touch the router.

### 5.C. IDE extensions (`ide-extensions~/`)

Affected file: **`grammar/uitkx-schema.json`**, after the `Route` entry (~line 1700).

Add:
```json
"Outlet": {
  "propsType": "OutletFuncProps",
  "description": "Renders the matching child route of the surrounding Route layout.",
  "acceptsChildren": false,
  "attributes": []
},
"Routes": {
  "propsType": "RoutesFuncProps",
  "description": "Renders the best-match Route from its children (first-match-wins ranked).",
  "acceptsChildren": true,
  "attributes": [
    { "name": "location", "type": "RouterLocation",
      "description": "Override the location used for matching." }
  ]
},
"NavLink": { ... }
```

This drives:
- VS Code completion (`extension.ts` already routes via LSP using the schema — no code
  change in `extension.ts`, only data).
- Hover docs.
- Roslyn-side IntelliSense via the schema-mirroring test in `FormatterSnapshotTests.cs`
  (the `<Router path=...` regression guard at line 9436+ would automatically also cover
  `<Outlet>` via the same generic test machinery; no new tests *required* but worth
  adding 1-2 sanity tests for `<Routes>` since it has children).
- Rider / Visual Studio extensions (`ide-extensions~/rider/`, `visual-studio/`) — both
  read the same `uitkx-schema.json`, so they pick up the new components for free.
- Grammar (`ide-extensions~/grammar/`) — TextMate/tree-sitter scopes use the same name
  list; verify the well-known-component highlighting picks up `Outlet`/`Routes`/`NavLink`.

Impact: **low**, schema additions + 1 grammar list update. Zero TypeScript code changes.

### 5.D. Diagnostics & analyzers

A new analyzer warning would be valuable but not required:
- **UITKX-Rxxx**: `<Outlet/>` used outside any `<Route>` ancestor — runtime no-op.
- **UITKX-Rxxx**: Two sibling `<Route>` patterns can match the same path in `<Router>`
  (encourage migration to `<Routes>`).

These are net new analyzer rules, opt-in. Not blockers.

### 5.E. Runtime (Hooks/Fiber)

No Hooks API changes needed. `Outlet` is implemented purely on top of
`Hooks.UseContext` + `Hooks.ProvideContext` (existing primitives). `Routes` is implemented
on top of `Hooks.UseMemo` (existing).

Impact: **zero on the Fiber/Hook runtime.**

### 5.F. Tests

- New unit tests on `Shared/Core/Router/`: ranking, optional segment, splat at
  multiple depths, layout-route + index-route, `useOutletContext`.
- Existing snapshot tests in `FormatterSnapshotTests.cs` cover Route well; add Outlet
  snapshot pairs.
- HMR golden samples — add one Outlet sample.

Impact: **medium-low** (this is the only place that requires real authoring work).

### 5.G. Docs / changelog

`ide-extensions~/changelog.json` entry per the project's changelog instructions.
`CHANGELOG.md` + `Plans~/MIGRATION_GUIDE.md` notes for the new components.
README sections for the router.

---

## 6. Other RR primitives worth considering (independent of Outlet)

These are decoupled from the Outlet decision. Each one ranked by value-vs-cost:

| Primitive | Value | Cost | Recommendation |
|---|---|---|---|
| `<NavLink>` with `isActive` | High (almost every menu wants it) | ~30 LOC | **Add** alongside Outlet/Routes |
| `<Navigate to>` declarative redirect | High (replaces the redirect-effect boilerplate in current `Pretty Ui`) | ~20 LOC | **Add** |
| Optional segments (`:lang?`) | Medium | ~50 LOC in `RouteMatcher` (need to "explode" optional segments like RR) | **Add later** |
| Case-sensitive flag | Low | ~10 LOC | Add when asked |
| `basename` on `<Router>` | Low (Unity has no URL bar) | ~15 LOC | Add when asked |
| `useSearchParams` setter | Low | ~30 LOC; need `Replace`/`Push` integration | Add when needed |
| `useNavigate(to, {replace,state,relative})` options bag | Medium-high (closer to RR DX) | ~20 LOC | **Add** as optional second arg, keep current signature |
| `useMatches()` (flat chain) | Medium (debug/breadcrumbs) | needs Outlet first | After Outlet |
| `errorElement` per route | Medium | ~80 LOC + Fiber error-boundary integration check | Defer |
| Loaders / actions / fetchers | Out of scope — assumes a request/response model that doesn't exist in Unity UI | Large | **Skip** |

---

## 7. Recommendation

### 7.A. Do this (in one PR)

1. Add `<Outlet/>` component (`Shared/Core/Router/RouterComponents.cs`):
   - New `OutletFunc.Render` reads `RouterContextKeys.OutletElement` from context, returns
     it (or `null`).
   - New `OutletFuncProps` (no required attributes; optional `Context` for
     `useOutletContext` parity).
   - New context key `RouterContextKeys.OutletElement`.

2. Modify `RouteFunc.Render` to **publish** the matched child's element into
   `RouterContextKeys.OutletElement` *before* rendering its own `Element`. The matched
   child is computed by:
   - Inspecting `children` for child `<Route>` VirtualNodes (already a `IReadOnlyList`).
   - Running `RouteMatcher.Match` against each child's `Path` (combined with our pattern).
   - Picking the **first matching** (or the *best-ranked* if we add scoring).
   - Allow `Element` AND `children` to coexist when children are all `<Route>`s.

3. Add `<Routes>` component:
   - First-match-wins over its children. Implements the small flatten+rank pipeline (RR's
     `flattenRoutes` + `rankRouteBranches` ported, ~150 LOC).
   - Renders the matched chain as `<Route>` → `<Outlet>` → `<Route>` → ... fold.

4. Add `<NavLink>` and `<Navigate>` (small, independent, high value).

5. Source-gen alias updates (PropsResolver + HmrCSharpEmitter).

6. Schema updates (`uitkx-schema.json`).

7. Tests, changelog, migration note, sample.

### 7.B. Migration is fully additive

Existing `<Router>`/`<Route>`/`<Link>` semantics are **unchanged**. The new
`Element + children-with-Routes` behavior only activates when both are present and
children include `<Route>` descendants. No existing app breaks.

### 7.C. Estimated touch list

- `Shared/Core/Router/RouterComponents.cs` — modify `RouteFunc`, add `OutletFunc`,
  `RoutesFunc`, `NavigateFunc`, `NavLinkFunc` (~250 LOC added)
- `Shared/Core/Router/RouterContextKeys.cs` — add 1 key (~3 LOC)
- `Shared/Core/Router/RouteMatcher.cs` — extract `flattenRoutes`/`rankRouteBranches`
  helpers (~150 LOC)
- `SourceGenerator~/Emitter/PropsResolver.cs` — alias additions (~5 LOC)
- `Editor/HMR/HmrCSharpEmitter.cs` — alias additions (~5 LOC)
- `ide-extensions~/grammar/uitkx-schema.json` — entries (~60 LOC of JSON)
- Tests (~300 LOC)
- Sample under `Samples/Components/RouterOutletDemo/` (~100 LOC)
- `CHANGELOG.md`, `ide-extensions~/changelog.json`, `Plans~/MIGRATION_GUIDE.md`

### 7.D. Risk register

| Risk | Likelihood | Mitigation |
|---|---|---|
| Hook ordering changes when `RouteFunc` newly inspects children | low | Children inspection is plain VirtualNode property reads, no Hooks issued; safe |
| HMR delegate swap on new `OutletFunc` | low | Inherits standard pipeline; covered by existing HMR sample retest |
| `<Routes>` ranking diverges from RR for edge cases (empty path, splat depth) | medium | Port RR's exploded-optional-segment tests verbatim |
| Schema typing for `OutletProps.Context` (object?) confuses IntelliSense | low | Mirror existing `LinkFunc.State` typing |
| Existing `<Route>` consumers that *intentionally* relied on element-vs-children mutual exclusion | very low | Behavior only changes when children include `<Route>` — non-route children still treated as Fragment as today |

---

## 8. Bottom line

UITKX router today is a faithful subset of React Router v6 for **path-relative nested
matching, params, blockers, and memory history**. It is missing the **composition
primitive** (`<Outlet/>`) and the **mutual-exclusion primitive** (`<Routes>`), and the
ergonomic helpers (`<NavLink>`, `<Navigate>`). These four additions:

- Are **strictly additive** (no breaking change).
- Are **a net perf win** because they replace per-sibling matching with rank+first-match.
- Touch **two trivial alias maps** in source-gen/HMR and **one schema file** for IDE.
- Require **zero changes** to Hooks/Fiber runtime.
- Require **no new diagnostic codes** (analyzer warnings are nice-to-have, not required).

Recommended scope: ship Outlet + Routes + NavLink + Navigate together as a single
"router v2" feature (no version bump; minor release).

Out of scope (intentionally): loaders, actions, fetchers, lazy routes, scroll restoration,
view transitions, RSC, browser/hash history. These belong to a different problem space
than a Unity UI router.
