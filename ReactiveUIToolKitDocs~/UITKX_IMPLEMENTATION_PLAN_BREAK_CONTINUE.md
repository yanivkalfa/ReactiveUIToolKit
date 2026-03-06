# UITKX Implementation Plan - Break/Continue Loop Flow

## Goal
Enable `break` and `continue` inside UITKX loop constructs (`@for`, `@while`) with clear parser diagnostics and correct C# emission.

## Phase 1 - Language Shape + Rules
- [ ] Finalize syntax rules for `break;` and `continue;` in UITKX blocks.
- [ ] Define semantic constraints (only valid inside loop bodies).
- [ ] Define diagnostic IDs/messages for invalid usage.
- [ ] Add examples for valid/invalid forms in docs.

## Phase 2 - Parser + AST
- [ ] Add AST nodes for `BreakStatement` and `ContinueStatement`.
- [ ] Update parser statement dispatch to recognize both keywords.
- [ ] Preserve source spans for precise diagnostics.
- [ ] Ensure parser recovery for malformed loop-flow statements.

## Phase 3 - Diagnostics
- [ ] Add structural validation for loop-context checks.
- [ ] Report diagnostics when `break`/`continue` appear outside loops.
- [ ] Add diagnostics tests for nested/mixed block scenarios.
- [ ] Validate no false positives in valid nested loops.

## Phase 4 - C# Emitter
- [ ] Emit direct C# `break;` and `continue;` when context is valid.
- [ ] Keep emitter output deterministic and formatting-stable.
- [ ] Ensure generated code compiles in representative loop samples.

## Phase 5 - Tooling + Regression Coverage
- [ ] Add formatter expectations for loop-flow statements.
- [ ] Add end-to-end tests (UITKX input -> generated C# output).
- [ ] Add LSP diagnostics snapshot tests for editor experience.
- [ ] Run full extension + generator test suites.

## Phase 6 - Release + Documentation
- [ ] Update main implementation plan status and summary.
- [ ] Publish syntax examples in community docs.
- [ ] Perform extension release flow (version bump, build, publish, local install).
