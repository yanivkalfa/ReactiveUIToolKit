# UITKX V1 Release Plan (Consolidated)

> **Single source of truth for everything still outstanding before the v1.0
> release.** This file consolidates every open item from the plans listed under
> "Superseded plans" below. Those source plans have been moved to `archive/`;
> their *open* items live here, their *shipped* items are recorded in
> [CHANGELOG.md](../CHANGELOG.md).
>
> **Current library version:** 0.6.2 (2026-06-05).
> **Created:** 2026-06-05 by consolidating 9 active plans + 7 shipped plans.

## Superseded plans (folded into this file, now in `archive/`)

Consolidated (had open items):
`UITKX_ROAD_MAP_V1_PRODUCTION_READY.md`, `TECH_DEBT_V2.md`,
`TECH_DEBT_SAMPLES_AND_RUNTIME.md`, `HMR_AUDIT.md`, `GRAMMAR_AUDIT.md`,
`COHERENCY_FIXES.md`, `OPTIMIZATIONS.md`, `ISSUE_14_FIX_PLAN.md`,
`PRETTY_UI_HMR_BUGS.md`.

Already fully shipped (archived, no carryover):
`0_5_22_THREE_ISSUE_PLAN.md`, `CAPTURE_EVENTS_PLAN.md`,
`UITKX_LSP_REF_STALENESS_FIX_PLAN.md`, `PHASE1_PHASE2_JSX_UNIFY.md`,
`UNIVERSAL_ATTRS_USER_COMPONENTS.md`, `HMR_DETERMINISM_REPORT.md`.

Kept as living reference (NOT archived):
`MIGRATION_GUIDE.md`, `VERSIONING_PROCESS.md`,
`UITKX_ARCHITECTURE_LANGUAGE_SERVER.md`, `LATENCY_TARGETS.md`,
`ROUTER_REACT_ROUTER_COMPARISON.md` (needs a refresh - see H-2),
`repository-atlas.md`.

## Status legend

- `[ ]` not started
- `[~]` in progress / partially done
- `[x]` done (verified)
- **Blocker** = gates the v1 tag. **Polish** = should-fix for quality.
  **Debt** = can ship v1 with it open.

---

# Part 1 - Release process & product (the real v1 blockers)

These are **not engineering** - they are scope, release-ops, and support
process. They are the bulk of what stands between "feature-complete" and
"v1 shipped." None are written; all are blockers for a credible v1.

## A. Scope lock & product definition  (Blocker)

- [ ] **A-1** Freeze the v1 in-scope feature set; explicitly mark out-of-scope.
- [ ] **A-2** Lock the MVP language surface (syntax/features) for v1.
- [ ] **A-3** Publish a v1 feature matrix (supported / partial / planned).
- [ ] **A-4** One-line product positioning + target personas + top-3 outcomes.
- [ ] **A-5** Align messaging across README, docs site, extension listing, samples.
- [ ] **A-6** Name a release owner, a backup, and a bug-triage owner.
- [ ] **A-7** Define severity policy (P0-P3) and late-scope-change decision policy.

## G. Release operations  (Blocker)

- [ ] **G-1** Cut an RC branch and freeze feature merges.
- [ ] **G-2** Run the full regression suite and record results.
- [ ] **G-3** Execute the release runbook end-to-end as a dry run.
- [ ] **G-4** Verify the upgrade path from the previous version.
- [ ] **G-5** Build all artifacts from a clean environment, publish, then
      verify a local install + real-world smoke test post-publish.
- [ ] **G-6** Define mandatory green checks before a release tag; block on
      failing tests or open P0/P1.
- [ ] **G-7** Post-release: monitor channels for 72h, triage by SLA, publish
      hotfix criteria, capture a postmortem.

## H. Support, community, docs gaps  (Blocker for launch, not for code-freeze)

- [ ] **H-1** Announcement content (what/why/limitations) + migration messaging.
- [ ] **H-2** Define official support channels + expected response times.
- [ ] **H-3** Incident-response guide (extension/server failures).
- [ ] **H-4** Maintainer onboarding guide + contribution guide (standards, tests, PRs).
- [ ] **H-5** Issue templates (bug, feature, diagnostics, docs) + repro checklist.
      *(Note: `.github/ISSUE_TEMPLATE/feature_request.yml` exists - audit coverage.)*
- [ ] **H-6** Two missing how-to guides: "component composition patterns" and
      "props/state-style patterns."
- [ ] **H-7** Define adoption/quality/DX metrics to watch in the first month.

## Remaining roadmap technical gaps (smaller, from Phases B-F)

- [ ] **RT-1** Validate clean-clone -> first successful build (Phase D). *(Blocker - never done end-to-end.)*
- [ ] **RT-2** Validate samples on at least one macOS setup (Phase D). *(Deferred - no macOS env. Decide: defer officially or borrow a Mac.)*
- [ ] **RT-3** Document per-editor feature-degradation + known limitations (Phase C). *(Docs.)*
- [ ] **RT-4** Third-party license inventory + attribution notices (Phase F compliance). *(Note: `THIRDPARTY.md` exists - verify completeness.)*
- [ ] **RT-5** Define distribution channels, SemVer + compatibility policy, deprecation/upgrade policy (Phase F).
- [ ] **RT-6** (Optional) Evaluate a runtime-only package variant (no IDE tooling) to cut install footprint.

---

# Part 2 - Engineering polish (user-visible IDE/UX)

The only items here a *user* would actually feel. Worth doing for a quality v1.

## P-1  TextMate grammar layer-1 gaps  (Polish)
**Source:** `GRAMMAR_AUDIT.md`. **Files:**
`ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json` (+ `ide-extensions~/grammar/` copy).
Semantic tokens (layer 3) mask these *after* the LSP connects, so the symptom
is only un-colored constructs on the first paint / before LSP attaches.

- [ ] **P-1a** Fragments `<>...</>` have no grammar rule (entirely uncolored). *(CRITICAL of the set - common syntax.)*
- [ ] **P-1b** Nested generics `Helper<List<int>>()` break function-call highlighting.
- [ ] **P-1c** Verbatim / interpolated / raw strings (`@"..."`, `$@"..."`, `"""..."""`) not matched; `""` escape can close a string early.
- [ ] **P-1d** Char literals `'x'` unstyled.
- [ ] **P-1e** Null-conditional `?.` / `?[` and the `when` keyword not recognized. *(Lowest priority - semantic tokens cover.)*

## P-2  Format-on-save silent no-op - INVESTIGATE (not yet a confirmed bug)  (Polish)
**Source:** `COHERENCY_FIXES.md` item 2A. The original "swallows exceptions"
diagnosis was **wrong**: `AstFormatter.Format` is already fail-safe (returns
source unchanged on parse error; Roslyn passes are caught best-effort). The
real user scenario (a save that no-ops on a broken file) needs a **live repro**
to identify which branch is hit (parse-error short-circuit vs. already-canonical
no-op vs. other).
- [ ] **P-2** Capture "UITKX LSP Trace" output during a failed save, identify
      the branch, then decide if any change is warranted. Files:
      `ide-extensions~/lsp-server/FormattingHandler.cs`,
      `ide-extensions~/language-lib/Formatter/AstFormatter.cs`.

## P-3  Cross-file diagnostic staleness - verify VS2022  (Polish)
**Source:** `TECH_DEBT_V2.md` TD-12. Fix is **implemented** for the LSP
(editor-buffer peer reads + T3 revalidation wired); only VS2022 confirmation
is outstanding.
- [x] **P-3** Re-test tab-switch staleness in the VS2022 extension; close if green. **Done (2026-06-19)** - fix shipped (LSP editor-buffer peer reads + T3 revalidation); VS2022 1.1.0.

---

# Part 3 - Tech debt (v1 can ship with these open)

## HMR audit follow-ups  (Debt)
**Source:** `HMR_AUDIT.md`. B1/B4/B7 were resolved by the 0.6.0 Family redesign.
Remaining:
- [x] **D-HMR-B2** Verify `[HookSignature]` is emitted for empty-signature hooks (quick check vs. 0.6.0 hook redesign). **Done (2026-06-19)** - correct by design: the attribute is intentionally omitted for empty signatures, but the runtime still registers the empty signature via unconditional `RegisterHook`; parity tests pass.
- [ ] **D-HMR-B5** Generic method overloads silently skip swap (no overload-signature carrier on the `MethodInfo` field). Needs `[HmrOverloadSignature]` or name-encoding.
- [ ] **D-HMR-B8** USS cascade rewrites each dependent separately; batch into one asset + selective re-render.
- [ ] **D-HMR-B9** `TryResolveMissingDependencies` has no visited-set / cycle guard; add depth + cycle detection.
- [ ] **D-HMR-B10** `AssemblyReloadSuppressor` deferred refresh can fire after a re-lock; make the flow synchronous / gated.
- [x] **D-HMR-B3** Module-static cross-copy pins assemblies (per-save Mono leak). **Documented limitation, not fixable** - user mitigation is periodic full reload. Keep documented; no action.

## Optimization  (Debt)
- [ ] **D-OPT-1** HMR dependency index over-links copy-rename near-clones; one
      stray save triggers an extra ~80-300ms Roslyn compile on a sibling. P3
      hygiene, no user-facing symptom since 0.5.21. Deferred pending telemetry.
      *(This is also `ISSUE_14_FIX_PLAN.md` patch 14.2.)* Files:
      `Editor/HMR/UitkxHmrController.cs`, dependency-index builder.

## Design questions to resolve before locking the public API  (Debt)
- [ ] **D-DESIGN-TD11** Hook ownership model: keep namespace-scoped static
      container (`FooHooks.useX()`), or add `hook ComponentName useX()` to bind
      a hook to a component partial. Decide before API finalization.
- [ ] **D-DESIGN-TD14** Synthetic event dispatcher for cross-panel portal
      bubbling (React-DOM-style). Opt-in `<Portal synthetic>`. Userland
      callback/context bus works today. Large, post-v1 unless demanded.
- [ ] **D-TD15** `{cond && <Jsx/>}` in **setup code / directive bodies** still
      needs the explicit-ternary workaround (works in `{expr}` / `attr={expr}`
      since 0.5.5). Shared `FindLhsStartForLogicalAnd` walker would extend it.

## Low-priority test/quality  (Debt)
- [x] **D-TDS6** GalagaGame `GameScreen.uitkx` formatter idempotency: 2nd pass
      differs from 1st (2 snapshot tests fail). Runtime unaffected. Same family
      as the (fixed) bare-return idempotency bug. Files:
      `ide-extensions~/language-lib/Formatter/AstFormatter.cs`.
      **Done (2026-06-19)** - idempotency snapshot suite (incl. GalagaGame) passes; dedicated regression test added.
- [ ] **D-LAT** No automated latency-regression gate for the targets in
      `LATENCY_TARGETS.md` (<200ms T1/T2 diags, <500ms completion, etc.).
      Add CI sampling post-v1.

---

# Part 4 - Housekeeping

- [x] **H-K1** MarioGame `<HUD>` prop-mismatch audit (carryover from
      `COHERENCY_FIXES.md`): confirm the call site passes only declared props.
      **Done (2026-06-19)** - `HUD(int score, int lives)`; the only call site passes exactly `score` + `lives`.
- [x] **H-K2** Refresh or retire `ROUTER_REACT_ROUTER_COMPARISON.md` - it lists
      `<Outlet>`/`<Routes>`/`<NavLink>`/`<Navigate>` as missing, but all four
      shipped in 0.4.14 (`Shared/Core/V.cs`). Doc is stale.
      **Done (2026-06-19)** - added a status banner (verified 14/18 gaps shipped); 4 remaining tracked there (optional segments, errorElement, nav-action tracking, useNavigate relative).

---

# Already resolved - verified in code during this consolidation (2026-06-05)

These were carried as "open" in the source plans but are in fact **done**.
Recorded here so they are not re-opened.

- [x] **TD-S1 / COHERENCY-2B** - "cannot convert `__UitkxRef__<T>` to `Ref<T>`"
      IDE false positive. **Fixed.** `VirtualDocumentGenerator.cs` (L291-295,
      339, 505) now resolves `useRef<T>()` and ref params to the canonical
      `global::ReactiveUITK.Core.Ref<T>` (real DLL or polyfill, same FQN), so a
      `component -> peer hook(Ref<T>)` call binds to one nominal type.
      `__UitkxRef__` survives only as HoverHandler display-normalization and a
      test asserting it does NOT appear.
- [x] **TD-S7** - "No first-class `<Video>` element." **Shipped in 0.5.0**
      (`<Video>`, `<Audio>`, `useSfx()`, `MediaHost`, `VideoElementAdapter`).
- [x] **ISSUE_14 (14.1, 14.4)** - HMR cascade re-fires mount effects /
      wrong-namespace spam. **Shipped in 0.5.21.** 14.2 -> D-OPT-1; 14.3 dropped
      (no repro).
- [x] **HMR_DETERMINISM_REPORT** - project-size-sensitive HMR behavior. Root
      cause **eliminated by the 0.6.0 Family redesign** (no cascade batch
      ordering). Historical; run the §6 acceptance test (save x20, identical
      console) once to close formally.
- [x] **PRETTY_UI_HMR_BUGS** - point-in-time HMR audit (pre-0.5.21). Findings
      addressed across 0.5.21-0.6.0. Historical.

---

# Recommended order

**Track 1 - close the code story (fast, do first):**
1. **P-3** verify TD-12 in VS2022 (fix already in; just confirm).
2. **P-1a / P-1b** grammar fragments + nested generics (the two a user notices).
3. **P-2** reproduce the format-on-save no-op; only patch if a real branch is found.
4. **D-HMR-B2** quick verification; **D-TDS6** formatter idempotency if cheap.

**Track 2 - process & product (the actual v1 gate, do in parallel):**
5. **A-1..A-3** scope freeze + feature matrix (unblocks messaging + everything else).
6. **A-4..A-7** positioning, owners, severity policy.
7. **RT-1** clean-clone -> build validation; **G-6** define release gates.
8. **G-1..G-5** RC branch + runbook dry-run + clean-room publish rehearsal.
9. **H-1..H-5** announcement, support channels, incident/onboarding/contribution docs, issue templates.
10. **G-7 / H-7** post-release monitoring + metrics.

**Track 3 - housekeeping (anytime):**
11. **H-K2** refresh the stale router doc; **H-K1** MarioGame HUD audit; remaining grammar `P-1c..e`.

**Defer past v1 (debt):** D-HMR-B5/B8/B9/B10, D-OPT-1, D-DESIGN-TD11/TD14, D-TD15, D-LAT, RT-2 (macOS), RT-6 (package split).
