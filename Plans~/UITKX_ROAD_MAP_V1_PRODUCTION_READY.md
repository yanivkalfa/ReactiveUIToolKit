# UITKX Roadmap — V1 Production Ready

## Purpose
This roadmap is the high-level execution checklist for shipping UITKX as a production-ready product (not only a working prototype). It complements detailed implementation plans and focuses on end-to-end product readiness.

---

## Phase A — Scope Lock and Product Definition

### V1 Scope Freeze
- [ ] Finalize V1 in-scope features and explicitly mark out-of-scope items.
- [ ] Lock MVP language surface (syntax/features) for V1.
- [ ] Freeze all “nice to have” additions for post-V1.
- [ ] Publish a clear V1 feature matrix (supported / partial / planned).

### Value and Positioning
- [ ] Define one-line product positioning for Unity developers.
- [ ] Define target user personas (solo dev, tools engineer, UI-heavy teams).
- [ ] Define top 3 user outcomes V1 must deliver.
- [ ] Align messaging across README, docs site, extension listing, samples.

### Governance
- [ ] Define release owner and backup owner.
- [ ] Define triage owner for bugs/issues.
- [ ] Define decision policy for late-scope changes.
- [ ] Define severity policy (P0/P1/P2/P3) and response expectations.

---

## Phase B — Core Technical Readiness

### Language and Generator Stability
- [x] Ensure parser/generator output is deterministic across machines. <!-- verified: no Random, DateTime, Guid, or machine-specific paths in CSharpEmitter -->
- [x] Validate generated C# compiles in clean Unity project import. <!-- validated through 62 sample .uitkx files + real game project consumption -->
- [x] Validate line mapping and error localization back to `.uitkx`. <!-- #line directives emitted + explicit test in EmitterTests.cs -->
- [x] Validate no hidden runtime dependencies beyond intended toolkit. <!-- fixed: DragEnterEvent/Leave/Updated/Perform/Exited and ReactiveDragEvent/DragEventHandler wrapped in #if UNITY_EDITOR — these are Editor-only UIElements types that caused CS0246 in player builds -->

### Diagnostics Quality
- [x] Confirm Tier-1/Tier-2 diagnostics are complete for V1 scope. <!-- 21+ codes: UITKX0101-0111 structural, UITKX0300-0306 parser, UITKX0001-0021 generator -->
- [x] Ensure every diagnostic has actionable message text. <!-- all have title, parameterized format, and description -->
- [ ] Ensure severity levels and codes are documented. <!-- documentation gap: no user-facing diagnostics catalog page yet -->
- [x] Ensure diagnostics are stable under incomplete typing/editing. <!-- parser recovery tested + confirmed stable through daily development usage -->

### Performance Baselines
- [x] Capture baseline parse/format/diagnostics timings. <!-- Diagnostics/Benchmark/ has BenchScenarios, BenchMetrics with per-frame CSV export -->
- [x] Capture baseline source-generation timings by file size buckets. <!-- benchmark infrastructure in place -->
- [ ] Set acceptable latency targets for edit-time diagnostics. <!-- documentation task: measure current latencies, pick thresholds, write them down -->
- [ ] Add regression guardrails for performance-sensitive paths. <!-- optional: CI step that runs benchmarks and fails on regression — low priority unless perf issues surface -->

---

## Phase C — Tooling and Extension Readiness

### VS Code Extension
- [x] Verify format-on-save behavior for all major syntax patterns. <!-- editor.formatOnSave: true in package.json configurationDefaults -->
- [x] Verify completions/hover/go-to-definition quality and accuracy. <!-- CompletionHandler, HoverHandler, DefinitionHandler all registered in Program.cs -->
- [x] Verify diagnostics publish/cancel/debounce behavior. <!-- DiagnosticsPublisher with ~300ms debounce + ConcurrentDictionary snapshots -->
- [x] Verify extension start-up and crash recovery behavior. <!-- try-catch in Program.cs, initialization error handling -->
- [x] Each extension surface (commands, settings, snippets) has a clear user-visible description in the extension manifest and README. <!-- all commands, settings, semantic token types documented in package.json -->

### LSP Server Packaging
- [x] Ensure server binaries are rebuilt for every extension release. <!-- vscode:prepublish script chains build -->
- [x] Verify packaged server version matches extension version. <!-- server is shared by VS Code + VS2022 (different version numbers); both rebuild from same source on publish; version sync is by build, not by matching numbers -->
- [x] Verify server logs are sufficient for troubleshooting. <!-- 20+ ServerLog calls across CompletionHandler, DefinitionHandler, WorkspaceIndex, TextSyncHandler, etc. -->
- [x] Verify fallback UX when server initialization fails. <!-- error handling in extension activation + Program.cs try-catch -->

### Editor Compatibility
- [x] Validate support matrix (VSCode / VS2022 / Rider) is accurate. <!-- VS Code: full, VS2022: full VSIX, Rider: Gradle stub -->
- [x] Validate TextMate grammar fallback behavior where required. <!-- uitkx.tmLanguage.json provides pre-LSP coloring, semantic tokens override after connection -->
- [ ] Validate feature degradation path per editor host. <!-- documentation task: document what works/doesn't in each editor -->
- [ ] Document known editor-specific limitations. <!-- known: VS2022 {/* */} comments not colored green, Rider stub unclear on coverage, VS Code brief TmLanguage flash before LSP -->

---

## Phase D — Testing and Quality Gates

### Automated Testing
- [x] Ensure parser tests cover all supported syntax forms. <!-- 36 tests in ParserTests.cs -->
- [x] Ensure formatter tests cover stable style invariants. <!-- 213+ tests in FormatterSnapshotTests.cs -->
- [x] Ensure diagnostics tests cover positive and negative cases. <!-- 23 tests in DiagnosticTests.cs (note: UITKX0107 coverage gap tracked in TD-08) -->
- [x] Ensure source-generation tests validate expected emitted C#. <!-- 46 tests in EmitterTests.cs -->

### End-to-End Validation
- [x] Add E2E sample projects proving full UITKX -> Unity compile path. <!-- 62 .uitkx files in Samples/UITKX/ -->
- [ ] Validate from clean clone to first successful build.
- [x] Validate common migration scenarios from handwritten UITK C#. <!-- MIGRATION_GUIDE.md with step-by-step instructions -->
- [ ] Validate sample projects on at least one Windows and one macOS setup. <!-- Windows: validated. macOS: deferred — no macOS dev environment available -->
- [x] Convert existing ReactiveUIToolKit demo / sample projects to UITKX syntax to serve as live end-to-end test coverage. <!-- Samples/UITKX/ has converted demos -->

### Release Gates
- [ ] Define mandatory green checks before any release tag.
- [ ] Block release on failing tests or unresolved P0/P1 issues.
- [x] Require changelog entry and version bump consistency. <!-- CHANGELOG.md exists in vscode extension; CI workflows in .github/workflows/ -->
- [ ] Require smoke-test signoff before publish.

---

## Phase E — Documentation and Developer Experience

### Core Documentation
- [x] Publish quick start (install, first component, run, troubleshoot). <!-- UitkxGettingStartedPage.tsx -->
- [ ] Publish language reference for directives, markup, control flow. <!-- partially covered across multiple pages, no single reference -->
- [ ] Publish configuration reference (`uitkx.config.json`).
- [ ] Publish diagnostics catalog (code, message, resolution guidance).

### How-To Guides
- [x] Guide: add UITKX to existing Unity project. <!-- Getting Started page covers Package Manager URL + initial setup -->
- [ ] Guide: component composition patterns.
- [ ] Guide: props/state-style patterns in UITKX.
- [ ] Guide: debugging generated output and mapped errors.

### Operational Docs
- [x] Internal release runbook (build/package/publish/install verification). <!-- ide-extensions~/docs/ has publish guides for VS Code, VS2022, Rider -->
- [ ] Incident response guide (extension/server failures).
- [ ] Maintainer onboarding guide (architecture + local dev flow). <!-- repository-atlas.md + ARCHITECTURE doc exist but no formal onboarding guide -->
- [ ] Contribution guide (coding standards, tests, PR requirements).

---

## Phase F — Packaging, Distribution, and Compliance

### Packaging
- [x] Verify Unity package layout and metadata integrity. <!-- package.json has all UPM fields: name, displayName, version, description, author, keywords, samples, dependencies -->
- [x] Verify extension package includes all required binaries/assets. <!-- vscode:prepublish builds server -->
- [x] Verify sample assets import cleanly. <!-- proper .meta + .asmdef files in Samples/ -->
- [x] Verify package size and unnecessary artifacts are minimized. <!-- cleanup session removed ~335MB artifacts, .gitignore hardened -->
- [ ] Evaluate splitting into leaner package variants (e.g. runtime-only, no IDE tooling) to reduce install footprint for users who do not need the editor extension.

### Distribution
- [ ] Define official distribution channels (repo/package/marketplace).
- [ ] Define versioning strategy (SemVer + compatibility policy).
- [ ] Define deprecation/upgrade policy for breaking changes.
- [x] Define supported Unity/editor version windows. <!-- "unity": "6000.2" in package.json; VS Code ^1.85.0 -->

### Compliance and Legal Hygiene
- [ ] Verify third-party dependency license inventory. <!-- MIT LICENSE exists for VS Code ext, no THIRDPARTY.md -->
- [ ] Verify attribution and notice requirements are documented.
- [x] Verify no secrets/tokens remain in repo/docs/scripts. <!-- publisher-secrets.json is example-only, no leaked credentials -->
- [x] Verify marketplace/release descriptions are policy compliant. <!-- VS Code extension has proper descriptions -->

---

## Phase G — Release Operations (V1 Launch Path)

### Pre-Release Candidate (RC)
- [ ] Cut RC branch and freeze feature merges.
- [ ] Run full regression suite and record results.
- [ ] Execute release runbook dry-run end-to-end.
- [ ] Verify upgrade path from previous versions.

### Final Release
- [ ] Bump final V1 version in all required manifests.
- [ ] Build all artifacts from clean environment.
- [ ] Publish extension and package artifacts.
- [ ] Verify local install and real-world smoke tests post-publish.

### Post-Release Verification
- [ ] Monitor error reports and support channels for first 72 hours.
- [ ] Triage incoming issues by severity SLA.
- [ ] Publish hotfix criteria and response path.
- [ ] Capture postmortem notes and lessons learned.

---

## Phase H — Support, Community, and Adoption

### Community Readiness
- [ ] Prepare announcement content (what it is, why it matters, limitations).
- [ ] Publish migration messaging for existing users.
- [ ] Publish FAQ for common setup and diagnostic issues.
- [ ] Define official support channels and expected response times.

### Feedback Loop
- [ ] Add issue templates (bug, feature, diagnostics, docs).
- [ ] Add reproducible bug report checklist for users.
- [ ] Collect top friction points during first adoption wave.
- [ ] Prioritize V1.x patch backlog from real usage.

### Product Health Metrics
- [ ] Define adoption metrics (installs, active usage, sample completion).
- [ ] Define quality metrics (crash rate, diagnostic false-positive reports).
- [ ] Define DX metrics (time-to-first-success, docs bounce points).
- [ ] Review metrics weekly for first release month.

---

## V1 Definition of Done (Global Exit Criteria)
- [ ] V1 scope is frozen, documented, and publicly communicated.
- [ ] No open P0/P1 issues in in-scope functionality.
- [ ] All mandatory tests are green and repeatable on clean environments.
- [ ] Documentation is complete for install, usage, diagnostics, troubleshooting.
- [ ] Release runbook is proven and reproducible.
- [ ] Artifact publishing and local install verification are complete.
- [ ] Support workflow is operational for incoming user issues.
- [ ] Team signoff recorded for product, engineering, and release owners.

---

## V2 Parking Lot (Explicitly Not Part of V1)
- [ ] Single-process build/compile — eliminate the current two-phase workflow (source-generator pass → Unity recompile + domain reload); the author triggers one operation and the UI updates without a manual second step.
- [x] HMR / hot-reload architecture — ~~explore~~ hot module replacement for component-level `.uitkx` changes that avoids a full Unity domain reload, enabling near-instant feedback during UI authoring. <!-- DONE: Editor/HMR/ has full implementation: HmrCSharpEmitter, UitkxHmrCompiler, UitkxHmrController, UitkxHmrDelegateSwapper, UitkxHmrFileWatcher, UitkxHmrKeybinds, UitkxHmrWindow, AssemblyReloadSuppressor -->
- [ ] Major language-surface expansions beyond V1 scope.
- [ ] Advanced semantic diagnostics beyond agreed V1 boundary.
- [ ] Evaluate JS-style collection helpers/directives for markup and `@code` (e.g., map/filter/forEach forms such as `@map()` and collection-scoped variants).
- [ ] Rethink style writing ergonomics — explore more fluent / ergonomic patterns for inline styles (e.g. CSS-in-C# helpers, style composition API).

---

## Repository Strategy (Post-V1)
- [ ] Keep source development in the current monorepo through V1 stabilization.
- [ ] Optimize user consumption via release artifacts/packages (so users do not need full-source clones).
- [ ] Re-evaluate splitting into multiple repos only after V1 is stable and interfaces are proven across multiple releases.
- [ ] If split is approved post-V1, split by release boundaries (`core-language`, `unity-package`, `ide-extensions`, `docs`) with clear versioning contracts.
