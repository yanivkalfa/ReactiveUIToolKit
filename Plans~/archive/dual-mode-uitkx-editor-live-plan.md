# Dual-Mode UITKX Plan

## Goal

Move UITKX toward a dual-mode architecture:

- editor mode: live `.uitkx` updates without relying on C# recompilation for every markup edit
- build mode: keep generated C# output for production builds

This is aimed at better editor feedback now and a realistic path toward future HMR.

## High-Level Direction

The current UITKX flow is compile-time first:

- `.uitkx` change
- force Unity script compilation
- Roslyn source generator emits partial class code
- Unity reloads assemblies and re-renders

The dual-mode target is:

- editor-live path:
  - parse `.uitkx`
  - lower it into a runtime/editor renderable representation
  - re-render an already-mounted tree in place
  - preserve state when component identity and hook shape remain compatible
- build path:
  - keep the existing parser/lowering/emitter pipeline
  - emit C# for shipping builds

## Architecture Shape

### 1. Shared front-end

Keep one shared UITKX front-end:

- directive parsing
- markup parsing
- canonical lowering
- diagnostics

This front-end should feed both editor-live mode and build-time codegen.

### 2. Editor-live backend

Add an editor backend that:

- watches `.uitkx` changes
- reparses only the changed file
- resolves tags/components through editor/runtime registries instead of Roslyn-only compilation state
- produces a renderable node/factory representation
- asks mounted renderers to refresh in place

This backend should avoid script recompilation for pure UITKX markup edits.

### 3. Build backend

Keep the current C# emitter backend for builds:

- generate deterministic `.g.cs` or keep Roslyn generation
- preserve the zero-runtime-overhead production path

## Identity and State

True hot reload behavior depends on preserving component identity across edits.

The likely rule set is:

- state is preserved when component identity is stable
- hook state is preserved only when hook order/signature remains compatible
- incompatible edits trigger a controlled remount for that component subtree

The identity key should be stable and editor-owned, for example:

- `assembly + file path + component name`

not method/delegate identity from compiled C#.

## Suggested Phases

### Phase 1: Live Preview Infrastructure

- add a UITKX editor session manager
- track open/active UITKX roots
- re-render editor-mounted trees from file changes
- accept remount-only behavior first

### Phase 2: Stable Component Identity

- introduce non-Roslyn component identity keys
- map editor-updated components back onto existing mounted trees

### Phase 3: State Preservation Rules

- preserve hook state when compatible
- remount on hook mismatch
- surface clear diagnostics when preservation is unsafe

### Phase 4: Build/Editor Split Hardening

- formalize editor-live backend vs build backend
- keep one shared parser/lowering pipeline
- reduce source-generator responsibilities to build/production concerns

## Expected Benefits

- faster editor feedback for `.uitkx` edits
- cleaner path to HMR-like behavior
- less dependence on Unity additional-file/source-generator edge cases
- easier debugging of live markup changes

## Main Risks

- component resolution without Roslyn compilation context
- preserving state safely across markup and setup-code edits
- keeping editor-live behavior semantically aligned with build output
- avoiding drift between the live backend and the codegen backend

## Success Criteria

- editing a `.uitkx` file in the editor updates mounted UI without a full script recompile for markup-only changes
- stateful components preserve state across safe edits
- unsafe edits remount predictably instead of corrupting state
- builds still produce the same generated C# behavior as today
