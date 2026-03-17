# Docs Dual-Track Plan

## Goal

Split the documentation into two clean tracks:

- `UITKX` docs for the primary authoring model
- `C#` docs for the low-level/manual authoring model

Neither track should depend on constantly explaining the other.

## Product Positioning

- `UITKX` is the default and primary documentation experience
- `C#` remains documented as a supported alternate mode
- landing pages, onboarding, and most discovery should prioritize `UITKX`

## Structure

### 1. UITKX Track

Route family:

- `/uitkx/...`

Content focus:

- function-style `.uitkx` components
- intrinsic tags
- custom component composition
- hooks usage in UITKX
- router/signals examples in UITKX
- authoring rules and pitfalls

Docs tone:

- first-class path
- beginner-friendly
- no legacy/manual C# examples mixed into the same pages

### 2. C# Track

Route family:

- `/csharp/...`

Content focus:

- `V.*` authoring
- `VirtualNode`
- direct props classes
- manual `Render(...)` functions
- lower-level runtime composition

Docs tone:

- advanced/alternate path
- internally complete
- no UITKX cross-teaching inside core examples

## Content Rules

- do not mix UITKX and C# examples inside the same page unless the page is explicitly comparative
- each track should have its own getting started, concepts, and examples
- component examples should exist in both tracks only when both tracks add real value
- default search, nav emphasis, and homepage links should point users toward the UITKX track first

## Suggested Rollout

### Phase 1: Information Architecture

- define route split
- define navigation split
- identify shared pages vs track-specific pages

### Phase 2: UITKX First

- rewrite the current main docs path to UITKX-first content
- migrate examples and prose to function-style UITKX
- remove mixed old C# framing from the default path

### Phase 3: C# Track Extraction

- preserve or restore the useful low-level C# examples
- move them into a dedicated C# route tree
- ensure the C# track is complete for users who prefer direct runtime authoring

### Phase 4: Consistency Pass

- align terminology across both tracks
- fix search keywords, side nav labels, and cross-linking
- verify the default docs experience never accidentally falls back to old C# examples

## Open Questions

- whether some pages should stay shared between both tracks, such as roadmap or known issues
- whether component pages should exist twice for every wrapped control, or only when the C# version adds distinct value
- whether the homepage should visually brand the C# track as “advanced” or simply “alternate”

## Success Criteria

- a new user lands in UITKX docs and never has to mentally filter out old C# authoring patterns
- a low-level user can intentionally choose the C# track and get a coherent C#-first experience
- examples, wording, and navigation stop mixing the two models
