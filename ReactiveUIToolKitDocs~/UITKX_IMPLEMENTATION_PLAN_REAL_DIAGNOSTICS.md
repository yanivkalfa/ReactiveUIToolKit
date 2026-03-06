# UITKX Implementation Plan - Real Diagnostics

## Goal
Upgrade UITKX diagnostics from parser/structural checks to robust semantic and type-aware diagnostics aligned with generated C# and Unity compilation behavior.

## Phase 1 - Diagnostic Scope Definition
- [ ] Define MVP semantic diagnostics (symbols, types, unresolved references, invalid member access).
- [ ] Define severity strategy (error/warning/info) and default categories.
- [ ] Map diagnostic codes to user-facing messages and quick-fix intent.
- [ ] Identify explicit non-goals for first release (to keep scope controlled).

## Phase 2 - Semantic Model Foundation
- [ ] Build/bind symbol tables for UITKX scopes (component, parameters, locals, loops).
- [ ] Resolve identifiers and type contexts across markup/code boundaries.
- [ ] Track inferred expression types where available.
- [ ] Add incremental invalidation strategy for changed files.

## Phase 3 - Roslyn-Backed Validation
- [ ] Integrate generated C# semantic checks using Roslyn compilation APIs.
- [ ] Map Roslyn diagnostics back to UITKX source spans.
- [ ] Add filtering/deduplication to avoid duplicate parser/semantic reports.
- [ ] Ensure deterministic results in partial/incomplete editor states.

## Phase 4 - LSP Diagnostics Pipeline
- [ ] Extend LSP diagnostics publisher for semantic tiers.
- [ ] Add stable tier ordering and source tagging (parser/structural/semantic/roslyn).
- [ ] Implement debounce/cancellation for responsive editing.
- [ ] Validate multi-file workspace diagnostics consistency.

## Phase 5 - Quality + Regression Suite
- [ ] Add unit tests for symbol/type binding edge cases.
- [ ] Add golden diagnostics tests for common authoring mistakes.
- [ ] Add performance benchmarks on medium and large UITKX sets.
- [ ] Verify no regression in existing Tier-1/Tier-2 diagnostics.

## Phase 6 - Documentation + Release
- [ ] Document diagnostic catalog and troubleshooting guidance.
- [ ] Update implementation plan status and release notes.
- [ ] Execute extension release flow (version bump, build, publish, local install).
- [ ] Gather community feedback and schedule post-MVP diagnostic expansions.
