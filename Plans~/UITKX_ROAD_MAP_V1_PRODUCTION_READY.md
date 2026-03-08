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
- [ ] Ensure parser/generator output is deterministic across machines.
- [ ] Validate generated C# compiles in clean Unity project import.
- [ ] Validate line mapping and error localization back to `.uitkx`.
- [ ] Validate no hidden runtime dependencies beyond intended toolkit.

### Diagnostics Quality
- [ ] Confirm Tier-1/Tier-2 diagnostics are complete for V1 scope.
- [ ] Ensure every diagnostic has actionable message text.
- [ ] Ensure severity levels and codes are documented.
- [ ] Ensure diagnostics are stable under incomplete typing/editing.

### Performance Baselines
- [ ] Capture baseline parse/format/diagnostics timings.
- [ ] Capture baseline source-generation timings by file size buckets.
- [ ] Set acceptable latency targets for edit-time diagnostics.
- [ ] Add regression guardrails for performance-sensitive paths.

---

## Phase C — Tooling and Extension Readiness

### VS Code Extension
- [ ] Verify format-on-save behavior for all major syntax patterns.
- [ ] Verify completions/hover/go-to-definition quality and accuracy.
- [ ] Verify diagnostics publish/cancel/debounce behavior.
- [ ] Verify extension start-up and crash recovery behavior.
- [ ] Each extension surface (commands, settings, snippets) has a clear user-visible description in the extension manifest and README.

### LSP Server Packaging
- [ ] Ensure server binaries are rebuilt for every extension release.
- [ ] Verify packaged server version matches extension version.
- [ ] Verify server logs are sufficient for troubleshooting.
- [ ] Verify fallback UX when server initialization fails.

### Editor Compatibility
- [ ] Validate support matrix (VSCode / VS2022 / Rider) is accurate.
- [ ] Validate TextMate grammar fallback behavior where required.
- [ ] Validate feature degradation path per editor host.
- [ ] Document known editor-specific limitations.

---

## Phase D — Testing and Quality Gates

### Automated Testing
- [ ] Ensure parser tests cover all supported syntax forms.
- [ ] Ensure formatter tests cover stable style invariants.
- [ ] Ensure diagnostics tests cover positive and negative cases.
- [ ] Ensure source-generation tests validate expected emitted C#.

### End-to-End Validation
- [ ] Add E2E sample projects proving full UITKX -> Unity compile path.
- [ ] Validate from clean clone to first successful build.
- [ ] Validate common migration scenarios from handwritten UITK C#.
- [ ] Validate sample projects on at least one Windows and one macOS setup.
- [ ] Convert existing ReactiveUIToolKit demo / sample projects to UITKX syntax to serve as live end-to-end test coverage.

### Release Gates
- [ ] Define mandatory green checks before any release tag.
- [ ] Block release on failing tests or unresolved P0/P1 issues.
- [ ] Require changelog entry and version bump consistency.
- [ ] Require smoke-test signoff before publish.

---

## Phase E — Documentation and Developer Experience

### Core Documentation
- [ ] Publish quick start (install, first component, run, troubleshoot).
- [ ] Publish language reference for directives, markup, control flow.
- [ ] Publish configuration reference (`uitkx.config.json`).
- [ ] Publish diagnostics catalog (code, message, resolution guidance).

### How-To Guides
- [ ] Guide: add UITKX to existing Unity project.
- [ ] Guide: component composition patterns.
- [ ] Guide: props/state-style patterns in UITKX.
- [ ] Guide: debugging generated output and mapped errors.

### Operational Docs
- [ ] Internal release runbook (build/package/publish/install verification).
- [ ] Incident response guide (extension/server failures).
- [ ] Maintainer onboarding guide (architecture + local dev flow).
- [ ] Contribution guide (coding standards, tests, PR requirements).

---

## Phase F — Packaging, Distribution, and Compliance

### Packaging
- [ ] Verify Unity package layout and metadata integrity.
- [ ] Verify extension package includes all required binaries/assets.
- [ ] Verify sample assets import cleanly.
- [ ] Verify package size and unnecessary artifacts are minimized.
- [ ] Evaluate splitting into leaner package variants (e.g. runtime-only, no IDE tooling) to reduce install footprint for users who do not need the editor extension.

### Distribution
- [ ] Define official distribution channels (repo/package/marketplace).
- [ ] Define versioning strategy (SemVer + compatibility policy).
- [ ] Define deprecation/upgrade policy for breaking changes.
- [ ] Define supported Unity/editor version windows.

### Compliance and Legal Hygiene
- [ ] Verify third-party dependency license inventory.
- [ ] Verify attribution and notice requirements are documented.
- [ ] Verify no secrets/tokens remain in repo/docs/scripts.
- [ ] Verify marketplace/release descriptions are policy compliant.

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
- [ ] HMR / hot-reload architecture.
- [ ] Single-process compile UX redesign.
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
