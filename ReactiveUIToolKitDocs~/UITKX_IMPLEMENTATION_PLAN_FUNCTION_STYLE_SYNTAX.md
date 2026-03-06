# UITKX Implementation Plan - Function-Style Syntax

## Goal
Introduce React-like function-style component authoring in UITKX while preserving deterministic generation into existing C# component structures.

## Phase 1 - Syntax Contract
- [ ] Define function-style declaration syntax and allowed modifiers.
- [ ] Define parameters/props mapping and defaults behavior.
- [ ] Define return/content rules (single root, fragments, conditional return).
- [ ] Freeze initial MVP scope (exclude advanced features not required for first release).

## Phase 2 - Parsing + AST Model
- [ ] Extend grammar/parser to parse function-style component declarations.
- [ ] Add AST nodes for function declarations and returns.
- [ ] Capture body sections (markup, code, expressions) in normalized form.
- [ ] Add parser recovery paths for malformed function signatures and returns.

## Phase 3 - Lowering to Existing IR
- [ ] Design lowering pass from function-style AST into current component IR.
- [ ] Map function parameters to generated props/state-compatible structures.
- [ ] Normalize control-flow/markup blocks before emission.
- [ ] Ensure lowered IR remains backward-compatible with existing emitter paths.

## Phase 4 - C# Emission
- [ ] Emit stable C# partial classes/methods from lowered function-style input.
- [ ] Ensure generated names/symbols avoid collisions.
- [ ] Validate emitted code with generic and nested component usage.
- [ ] Confirm Unity compile compatibility on representative sample scenes.

## Phase 5 - Diagnostics + LSP UX
- [ ] Add syntax/structural diagnostics specific to function-style rules.
- [ ] Add hover/completion updates for new syntax entry points.
- [ ] Add formatter behavior for function signatures and returns.
- [ ] Add golden diagnostics snapshots for editor scenarios.

## Phase 6 - Testing + Rollout
- [ ] Add unit tests for parser, lowering, and emitter.
- [ ] Add end-to-end tests from UITKX source to generated C#.
- [ ] Update docs with side-by-side classic vs function-style examples.
- [ ] Execute extension release flow (version bump, build, publish, local install).
