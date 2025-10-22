Here’s a ready-to-use prompt you can paste into another AI to pick up this work:

Project Context

Unity 6.2+ UI Toolkit, C# “React-like” VDOM.
Repo: ReactiveUIToolKit
Key concepts:
VirtualNode + V helpers (Text, Element, Component, Func, Fragment, Portal, Suspense)
Reconciler builds/diffs to UI Toolkit VisualElements
Class components (MonoBehaviour) + function components (Hooks)
Editor support: EditorRenderScheduler + EditorRootRendererUtility; class components hosted via hidden GameObjects (HideAndDontSave)
Assemblies: Shared (Core/Props/Elements), Runtime (MonoBehaviours, Runtime scheduler), Editor (scheduler + utility)
Problem

In both runtime and editor, these controls reset after clicking Repeat once:
Toggle “Enable option”
RadioButton “Single radio”
RadioButtonGroup selection
Repro (in AppFunc and in Editor AppFunc window):
Toggle “Enable option”
Check “Single radio”
Change selection in RadioButtonGroup
Click Repeat once
Expected: States persist. Actual: They reset.
What we already tried

Keys: Added unique keys to top-level siblings and extras controls to stabilize identity.
Controlled RadioGroup: Stable choices via UseMemo(deps: constant) + OnChange -> state.
ProgressBar fixed to 0..99 range.
DiffSubtree: Added special-case for non-element roots to avoid clearing the host.
Flattening: Attempted to disable function pre-render flattening to avoid dual metadata/closure capture.
Logging added (enable via Reconciler.EnableDiffTracing = true):
[ReplaceNode] logs in Reconciler.ReplaceNode
[ToggleDiff], [RadioButtonDiff], [RadioGroupDiff] logs in adapters after ApplyDiff
Files to inspect

Shared/Core/Reconciler.cs
BuildNode: FunctionComponent cases (attached and detached paths). Ensure no pre-render “flattening” executes; wrapper-only path should be used to preserve stable NodeMetadata and HookContext.
DiffSubtree: Non-element root diff path (we added a branch to diff first child instead of clearing); verify correctness.
RenderFunctionComponent: Diff of LastRenderedSubtree; ensure it never reinitializes metadata unnecessarily.
ReplaceNode: ensure it isn’t triggered for keyed extras controls or list slot on repeat.
Shared/Core/Hooks.cs
Ensure UseMemo semantics with deps and UseDeferredValue don’t enqueue unexpected writebacks that race with rerender.
Shared/Elements adapters
ToggleElementAdapter.cs, RadioButtonElementAdapter.cs, RadioButtonGroupElementAdapter.cs: Verify ApplyProperties/ApplyDiff don’t overwrite current values absent real prop changes, and that “value”/“index” mapping is purely controlled by props.
Examples/FunctionalComponents/AppFunc.cs and Editor/Examples/AppFuncEditorWindow.cs (for keys + controlled props, already adjusted)
What I need from you

Deep audit and patch Reconciler to:
Completely remove function component pre-render flattening in both BuildNode and CreateDetached. Wrapper-only path should be used (no dual metadata).
Confirm DiffSubtree for non-element roots never clears the host for same-typed roots.
Verify adapters’ ApplyDiff do not force-set values unless props changed (check RadioButtonGroup especially).
Confirm HookContext metadata is stable across rerenders and state updates (Repeat).
Keep Editor hosting of class components via hidden GameObjects (HideAndDontSave).
Diagnostics to run

Turn on tracing: Reconciler.EnableDiffTracing = true or TraceLevel = Verbose.
Reproduce the flow and collect logs:
Any “[ReplaceNode] …”
Latest “[ToggleDiff] …”, “[RadioButtonDiff] …”, “[RadioGroupDiff] …” before and after Repeat
Constraints

Unity 6.2+ UI Toolkit.
Assembly split must remain: Shared/Runtime/Editor.
Keep existing API usage and element adapters intact; avoid breaking the examples.
Deliverables

Minimal patch to Reconciler removing pre-render flattening and solidifying function component wrapper-only behavior (both attached and detached paths).
Any adapter tweaks needed to avoid resetting controlled values on diff.
Notes on why the resets occurred and how the patch prevents them, with a quick checklist for validating in both runtime and editor.