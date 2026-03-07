# UITKX Implementation Plan - Break/Continue Loop Flow

## Goal
Enable `break` and `continue` inside UITKX loop constructs (`@for`, `@while`) with clear parser diagnostics and correct C# emission.

## Phase 1 - Language Shape + Rules
- [x] Finalize syntax rules for `break;` and `continue;` in UITKX blocks.
- [x] Define semantic constraints (only valid inside loop bodies).
- [x] Define diagnostic IDs/messages for invalid usage.
- [x] Add examples for valid/invalid forms in docs.

## Phase 2 - Parser + AST
- [x] Add AST nodes for `BreakStatement` and `ContinueStatement`.
- [x] Update parser statement dispatch to recognize both keywords.
- [x] Preserve source spans for precise diagnostics.
- [x] Ensure parser recovery for malformed loop-flow statements.

## Phase 3 - Diagnostics
- [x] Add structural validation for loop-context checks.
- [x] Report diagnostics when `break`/`continue` appear outside loops.
- [x] Add diagnostics tests for nested/mixed block scenarios.
- [x] Validate no false positives in valid nested loops.

## Phase 4 - C# Emitter
- [x] Emit direct C# `break;` and `continue;` when context is valid.
- [x] Keep emitter output deterministic and formatting-stable.
- [x] Ensure generated code compiles in representative loop samples.

## Phase 5 - Tooling + Regression Coverage
- [x] Add formatter expectations for loop-flow statements.
- [x] Add end-to-end tests (UITKX input -> generated C# output).
- [x] Add LSP diagnostics snapshot tests for editor experience.
- [x] Run full extension + generator test suites.

## Phase 6 - Release + Documentation
- [x] Update main implementation plan status and summary.
- [x] Publish syntax examples in community docs.
- [x] Perform extension release flow (version bump, build, publish, local install).
