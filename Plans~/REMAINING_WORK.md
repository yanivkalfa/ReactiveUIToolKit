# Remaining Work — consolidated backlog

> Single source of truth for every outstanding item extracted from the archived plans
> (see `Plans~/archive/`). Assembled 2026-07-15 by verifying each plan against the code —
> item status is what the CODE shows, not what the plans claimed. When an item ships,
> delete its row here. Sources: `FINAL_AUDIT_UITKX_OPTIMIZATIONS` (perf audit),
> `V1_RELEASE_PLAN` (v1 tracker), `ASSET_STORE_PUBLISHING_PLAN`, `UITKX_PARITY_CLEANUP_PLAN`,
> `CUSTOM_RENDERING_PLAN`, `ROUTER_REACT_ROUTER_COMPARISON`, `VERSIONING_PROCESS` (still live),
> plus doc-staleness items found during triage.
>
> Living references that stay in `Plans~/` (not plans, no items unless listed below):
> `DISCORD_CHANGELOG.md` (operational log), `family-corpus.hash` (**load-bearing** — CI drift
> gate + `ImportCorpusManifestTests` resolve the repo root via it), `LATENCY_TARGETS.md`,
> `MIGRATION_GUIDE.md`, `UITKX_ARCHITECTURE_LANGUAGE_SERVER.md`, `VERSIONING_PROCESS.md`,
> `codebase-index.json` / `repository-atlas.md` (generated references; stale, refresh-worthy).

---

## 1. Correctness / Bugs

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| U4 | Multi-root counting mismatch: `DiagnosticsAnalyzer` and `StructureValidator` count render roots with two separate implementations that can disagree — extract one shared root counter | two implementations remain (language-lib `DiagnosticsAnalyzer` vs SG `StructureValidator`) | PARITY U4 |
| P-2 | Format-on-save silently no-ops in some VS Code sessions — needs a live repro before any change (investigation item) | no repro recorded | V1 P-2 |

## 2. HMR

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| H-5 | Per-swap non-collectible assembly leak: add optional `UITKX_HMR_AutoReloadAfterSwaps` threshold + surface `SessionMemoryDeltaMB` in the HMR window | no such pref anywhere in `Editor/HMR/` | AUDIT-OPT H-5 |
| O-03 | `GC.Collect(2, Optimized)` per HMR compile — measure a 10-save burst; gate it if it stutters | `Editor/HMR/UitkxHmrCompiler.cs` (~2498, ~2783) unconditional | AUDIT-OPT O-03 |
| O-04 / D-HMR-B8 | Shared-`.uss` edit fans out N separate Roslyn compiles (one per dependent) — batch into one union compile (defer until a real complaint; one-per-tick drain already landed) | no dependent batching in `UitkxHmrController` | AUDIT-OPT O-04, V1 D-HMR-B8 |
| D-HMR-B5 | Generic method overloads silently skip HMR swap (no overload-signature carrier) | no `[HmrOverloadSignature]`-style mechanism found | V1 D-HMR-B5 |
| D-HMR-B9 | `TryResolveMissingDependencies` lacks a visited-set/cycle guard (the visited-set in the import fan-out path is a different mechanism) | `Editor/HMR/UitkxHmrController.cs` (~1156–1181) | V1 D-HMR-B9 |
| D-HMR-B10 | `AssemblyReloadSuppressor` deferred refresh can fire after re-lock | not addressed | V1 D-HMR-B10 |
| D-OPT-1 | HMR dependency index over-links copy-rename near-clones (deferred pending telemetry) | deferred | V1 D-OPT-1 |
| HMR-MC | Multi-component files hot-swap only their FIRST component (HMR reads the singular `ComponentName`; `ComponentDeclarations` is never read in `Editor/HMR/`) — documented as a Known Issue; full support = per-declaration compile/swap | `Editor/HMR/UitkxHmrCompiler.cs` `Compile()` singular read | triage 2026-07-15 |
| HMR-FSW | If the member-file silence recurs WITH the 2026-07-18 trail in place (save produces neither an `[HMR] Save:` line nor, with Verbose watcher trace on, an `[HMR][trace] FSW` line), the drop is OS-level FSW non-delivery (Mono 8 KB buffer / AV hook) — next step is a bounded mtime-sweep fallback over the known `.uitkx` set in the watcher pump (the AssetPostprocessor net cannot help mid-session: `DisallowAutoRefresh` starves it). Trigger to revisit: one field report with the trail present | `Editor/HMR/UitkxHmrFileWatcher.cs` pump; fix wave `fix/hmr-field-wave` | field triage 2026-07-18 |

## 3. Performance

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| U-16 | LSP workspace scan walks `Library/`/`Temp/`/`obj/` with no exclusions (only `~` dirs skipped) — minutes-long initial scan on big projects | `ide-extensions~/lsp-server/WorkspaceIndex.cs` (~616–645) `EnumerateFiles(root, "*.cs", AllDirectories)` | AUDIT-OPT U-16 |
| U-17.2 | `CheckAssetPaths` does `File.Exists` per `Asset<>`/`@uss` occurrence per keystroke, no TTL cache; regex also matches inside comments | `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs` (~1334) | AUDIT-OPT U-17.2 |
| U-21 | Source generator is effectively non-incremental: reprocesses every `.uitkx` on any compilation change; per-file triple parse not merged; no `TrackIncrementalGeneratorSteps` cacheability test | `SourceGenerator~/UitkxGenerator.cs` single `RegisterSourceOutput` over combined providers | AUDIT-OPT U-21 |
| C-01 | Text vnodes allocate a `Dictionary<string,object>` per reconcile visit (largest steady-state alloc in the diff path) | `Shared/Core/Fiber/FiberChildReconciliation.cs` (~495) | AUDIT-OPT C-01 |
| O-01 | Scanner micro-costs: zero-alloc keyword compare (`TryReadKeywordAt` allocates a Substring per probe); thread line-starts/JSX ranges once per parse (partially done via `CSharpLexFacts.BuildLineStarts`) | `ReturnFinder.cs` (~255), `DirectiveParser.cs` (~2213) | AUDIT-OPT O-01 |
| O-05 | Formatter allocation profile — span-based line walking (opportunistic; its correctness gates all landed) | `AstFormatter` split/substring-heavy | AUDIT-OPT O-05 |

## 4. Diagnostics

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| U1 | Delete/retire never-emitted diagnostic constants + descriptors and add a reflection test enforcing "every registered code has an emission site" (0005/0012/0101/0102/0110 partially retired already — 0005/0103 SG descriptors deleted, 0101/0102/0103 marked RETIRED; finish the sweep + the guard test) | `DiagnosticCodes.cs` still lists codes with no emitter | PARITY U1 |
| U2 | Severity reconciliation per code across IDE/compile surfaces (0104, 0305, 0105/0109/0121) with a table-driven severity test (0106 already aligned by audit U-12) | no table-driven severity test | PARITY U2 |
| U3 | Diagnostics dropped at the Unity bridge: `Location.None` bridging semantics, `#error` strips the code prefix, discarded `jsxDiags`, unused 0025/0026 descriptors (partially improved by SurfaceLocationlessDiagnostics — audit the remainder) | `SourceGenerator~/UitkxPipeline.cs` | PARITY U3 |
| U5 | Silent tolerances to decide + enforce: duplicate attributes, expression-valued duplicate keys, `Key=`/`key` casing, camelCase `useEffect` variants, stray `>` | not addressed | PARITY U5 |

## 5. Testing

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| CR-T | Custom-rendering test matrix T1–T10 (SG emit shape, pool reset, diff/remove, `RedrawKey` `_hasEvents` gating, HMR parity marker) never landed as named tests | `grep OnGenerateVisualContent SourceGenerator~/Tests` → no matches | CUSTOM_RENDERING |
| RT-1 | Clean-clone → first-build validation (was flagged Blocker; never done — a CI job or a recorded manual run) | no CI job or record | V1 RT-1 |
| RT-2 | macOS sample validation (deferred — make the deferral an explicit recorded decision) | no macOS environment | V1 RT-2 |
| D-LAT | Automated latency-regression CI gate for `LATENCY_TARGETS.md` thresholds | no such CI job | V1 D-LAT |

## 6. Tooling / IDE

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| P-1a..e | TextMate layer-1 gaps: fragments `<>`, nested generics, verbatim/interpolated/raw strings, char literals, `?.` / `when` | `ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json` — no fragment/verbatim-string rules | V1 P-1 |
| LSP-P1 | Roslyn-formatter pass for `@code`/setup blocks (self-listed polish) | ARCHITECTURE doc §polish | LSP-ARCH |
| LSP-P2 | Semantic-token cross-IDE portability audit (self-listed polish) | ARCHITECTURE doc §polish | LSP-ARCH |
| LSP-P3 | TmLanguage minimal-fallback cleanup (overlaps P-1) | ARCHITECTURE doc §polish | LSP-ARCH |
| VP-7 | Add `sinceUnity` annotations to `uitkx-schema.json` for 6.3 additions | `VERSIONING_PROCESS.md` unchecked | VERSIONING VP-7 |
| VP-15 | Gap analyzer for the version-diff script (reads the compat matrix) | `VERSIONING_PROCESS.md` unchecked | VERSIONING VP-15 |
| Store-3 | OpenUPM registration (worth doing regardless of Asset Store) | no OpenUPM config | ASSET_STORE |

## 7. Features / Design decisions

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| VP-27..33 | Unity 6.3 support wave: `#if` guards for 3 new `IStyle` props, `FilterFunction` CssHelpers, schema/styleKeyValues, docs manifest, matrix update, 6.2/6.3 test runs | `VERSIONING_PROCESS.md` Phase 4 unchecked | VERSIONING |
| RR-1 | Router: optional segments (`:lang?`) | re-homed from ROUTER comparison | ROUTER |
| RR-2 | Router: per-route `errorElement` | re-homed | ROUTER |
| RR-3 | Router: navigation-action tracking (POP/PUSH/REPLACE) | re-homed | ROUTER |
| RR-4 | Router: `UseNavigate` `relative:"path"` resolution | re-homed | ROUTER |
| TD11 | Design decision: hook ownership model | open decision | V1 D-DESIGN-TD11 |
| TD14 | Design decision: synthetic event dispatcher for portals | open decision | V1 D-DESIGN-TD14 |
| U7 | Decision: single-quote attribute strings | open decision | PARITY U7 |
| RT-6 | (Optional) runtime-only package variant | not done | V1 RT-6 |
| ~~NSIMP~~ | **DONE (v0.8.0)** — Namespace-import unification: `import "@Ns"`, UITKX2316 (editor error / build warning), UITKX2317 redundant-using Hint, quick-fixes, codemod `--tidy`, formatter round-trip. Deferred follow-ups: semantic unused-using 2317, per-segment namespace completion + hover, 2316 "did you mean" suggestion, bulk samples `--tidy` | `Plans~/IMPORT_NAMESPACE_UNIFICATION_PLAN.md` | user request 2026-07-15 |
| SAMPLES-NS | **READY TO EXECUTE** (hand to any agent) — modernize Samples to zero `@namespace` via `namespacePrefix` config; fully-researched plan with verbatim per-file edit appendices (A: name→ns map, B: exact C# using edits, C: exact uitkx import DELETEs/ADDs) + hard verification gates | `Plans~/SAMPLES_NAMESPACE_MODERNIZATION_PLAN.md` | user request 2026-07-16 |

## 8. Cleanup / Tech-debt

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| C-02 | `ExtractProps` duplicated ×4 (drift risk): `FiberChildReconciliation` (~482), `FiberFactory` (~287), `FiberFunctionComponent` (~348), `FiberReconciler` (~1599) — extract one helper | four copies in `Shared/Core/Fiber/` | AUDIT-OPT C-02 |
| C-04 | `FiberReconciler.MetricsEmitted` static event can pin torn-down hosts in the editor — clear on unmount / domain reload | `Shared/Core/Fiber/FiberReconciler.cs` (~76) bare static event | AUDIT-OPT C-04 |
| C-06 | Add a reviewer note / analyzer rule that `Shared/Core/Fiber/**` stays LINQ-free | no guard exists | AUDIT-OPT C-06 |
| U6 | Docs truthfulness sweep: `{__children}`, bare-boolean attribute shorthand, prop-spread divergence, stale emitter comments | not addressed | PARITY U6 |
| VP-5 | `VERSIONING_PROCESS.md` still says "update TECH_DEBT.md" — retarget the pointer to this file | `VERSIONING_PROCESS.md` (~705) | VERSIONING VP-5 |
| ATLAS | `codebase-index.json` / `repository-atlas.md` are stale (May 3) — regenerate or mark generation date prominently | file dates | triage |

## 9. Docs

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| RT-3 | Per-editor feature-degradation matrix (what works in VS Code vs VS2022 vs Rider vs none) | not found | V1 RT-3 |
| RT-4 | Audit `THIRDPARTY.md` completeness | file exists, unaudited | V1 RT-4 |
| RT-5 | Publish compat/deprecation policy (beyond `VERSIONING.md`) | not published | V1 RT-5 |
| H-1..H-7 | Launch collateral: announcement, support channels, incident guide, onboarding/contribution guide, 2 how-to guides, metrics; residual H-5: diagnostics/docs issue templates (bug/feature/config exist) | not found | V1 H-* |
| A-1..A-7 | Product/process: scope lock, feature matrix, positioning, owners, severity policy | process work, no artifacts | V1 A-* |

## 10. Release / Process

| ID | Item | Evidence / anchor | Source |
|---|---|---|---|
| G-1..G-7 | Release engineering: RC branch, regression record, runbook dry-run, upgrade-path check, clean-env publish, release gates, post-release monitoring | process work, not done | V1 G-* |
| Store-1 | Asset Store: create package draft + first upload from a 6.2 editor + Submit (**paused by owner**; price switched to ~$5 → payout/tax setup required first) | no submission occurred | ASSET_STORE |
| RUNTIME-V | Unity-in-editor runtime verification of the 0.7.x import/export wave: the 5 HMR hook-family-key scenarios + F5 pass over migrated samples + JustStayOn | pending user | triage |
