# Samples

## Directory Structure

### Components/

Declarative `.uitkx` components demonstrating hooks, fiber behavior,
and runtime features. Each component has a `.uitkx` file, with optional
companion `.hooks.uitkx` (hook declarations) and `.style.uitkx`
(module declarations) files.

Includes 25+ demos: counters, context, effects, portals, routing,
signals, synthetic events, keyed diffing, and more.

### Shared/

Shared data models, utility functions, and reusable `.uitkx` components
used across multiple demos (list views, tree views, animations).

### Showcase/

Multi-page demo application hosting all samples together.
Contains EditorWindow entries for in-editor preview.

- `Editor/` — Individual demo windows (one per component)
- `Both/` — Showcase All aggregated demo page

### Shared/ (Common Utilities)

Reusable demo components used across categories:
animations, shared layouts, navigation bars.
