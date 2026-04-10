# Samples

## Directory Structure

### Components/ (Legacy C# Style)

Function-style components using the C# `V.*` API directly.
These demonstrate hooks, fiber behavior, and runtime features
without UITKX markup. Kept as reference for the C# authoring track.

Includes 25+ demos: counters, context, effects, portals, routing,
signals, synthetic events, keyed diffing, and more.

### UITKX/ (Modern UITKX Style)

Same demos rewritten in `.uitkx` declarative markup.
Each component has a `.uitkx` file, with optional companion
`.hooks.uitkx` and `.style.uitkx` files for hooks and styling.
This is the recommended authoring style.

### Showcase/ (Host Application)

Multi-page demo application hosting all samples together.
Contains Bootstrap classes and EditorWindow entries.
Run in Editor or Runtime mode.

- `Editor/` — EditorWindow host for in-editor preview
- `Runtime/` — MonoBehaviour host for play mode
- `Both/` — Shared demo pages (animations, controls, tree views)

### Shared/ (Common Utilities)

Reusable demo components used across categories:
animations, shared layouts, navigation bars.
