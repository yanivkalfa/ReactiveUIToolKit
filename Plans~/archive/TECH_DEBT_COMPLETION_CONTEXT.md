# Tech Debt - Completion Context Leakage (`@code`)

## Summary
`@code` completion has repeatedly leaked into non-header contexts due to multiple overlapping completion paths (cursor-kind routing, context stack inference, and schema-driven item sources).

## Current Mitigation
A final post-filter in the LSP completion handler now removes `@code` from results whenever the cursor is not in the strict header zone.

## Why This Is Tech Debt
- The behavior is currently protected by a safety filter rather than a single canonical context classifier.
- Context decisions are split across several helpers, making regressions likely when one path changes.

## Follow-up (when prioritized)
- Consolidate all completion context gating into one authoritative function.
- Add explicit completion snapshot tests for: header, after `@code`, first markup lines, and embedded markup inside `@code`.
- Remove the safety filter after canonical gating + tests are stable.

## Current Deferred Behavior
- `@code` suggestion currently still appears at the header boundary/start area by design.
- We still need a stricter rule that hides `@code` specifically in the transition zone
	between regular code content and first markup when context classification is ambiguous.

---

# Tech Debt - Dead `memoize` / `memoCompare` Fields

## Summary
`VirtualNode.Memoize` and `VirtualNode.TypedMemoCompare` are stored but never read by the reconciler or fiber component machinery. The `memoize` and `memoCompare` parameters on `V.Func(...)` / `V.Func<TProps>(...)` are therefore no-ops.

## How It Works Today
Every function component already bails out unconditionally when:
- no pending state update, AND
- `IProps.Equals(pendingProps)` is true, AND
- no context change

This means all components are effectively memo'd by default through `IProps.Equals`. The `memoize` flag adds nothing on top of this.

## Affected Code (to remove)
- `VirtualNode.Memoize` property and all constructor assignments
- `VirtualNode.TypedMemoCompare` property and all constructor assignments
- `memoize` / `memoCompare` parameters on all `V.Func` overloads in `V.cs`
- `VirtualNode` copy-constructor line: `Memoize = template.Memoize` / `TypedMemoCompare = template.TypedMemoCompare`

## Why This Is Tech Debt
- The fields are populated through the entire call chain (`V.Func` -> `VirtualNode` -> `FiberNode`) but silently ignored at the point of use.
- Keeping them creates false confidence that `memoize: true` does something.
- Removing them simplifies the API surface and eliminates dead constructor parameters.

## Follow-up (when prioritized)
- Search all call sites for `memoize: true` in Samples and game projects before deleting.
- Remove all `memoize` / `memoCompare` parameters and fields.
- If a custom-comparator escape hatch is ever needed, design it as a real named feature, not a silent no-op parameter.

---

# Tech Debt - `SyntheticEventDemoFunc` Uses `extraProps` Unnecessarily

## Summary
`SyntheticEventDemoFunc.uitkx` passes `onPointerDown`, `onPointerMove`, `onPointerUp`, and `onWheel`
handlers via the `extraProps` escape hatch with full-namespace casts:

```csharp
extraProps={new System.Collections.Generic.Dictionary<string, object> {
    { "onPointerDown", (System.Action<SyntheticPointerEvent>)(e => UpdateLog("PointerDown", e)) },
    { "onPointerMove", (System.Action<SyntheticPointerEvent>)(e => UpdateLog("PointerMove", e)) },
    { "onPointerUp",   (System.Action<SyntheticPointerEvent>)(e => UpdateLog("PointerUp", e)) },
    { "onWheel",       (System.Action<SyntheticWheelEvent>)(e => UpdateLog("Wheel", e)) }
}}
```

This is incorrect. All four events (`onPointerDown`, `onPointerMove`, `onPointerUp`, `onWheel`) are
**already hardcoded in `PropsApplier.ApplyEvent`** and typed on `BaseProps`. They can and should be
passed as plain JSX attributes � no `extraProps` escape hatch required.

Additional redundancy: `System.Collections.Generic.` and `System.` prefixes are unnecessary �
the emitter always injects `using System;` and `using System.Collections.Generic;` into generated code.

## Why This Is Tech Debt
- The demo was written defensively before the `Action<SyntheticPointerEvent>` handler path was
  confirmed to work through direct typed props, so `extraProps` was used as a workaround.
- Leaving it in place gives the false impression that `extraProps` is needed for synthetic event
  handlers, which it is not � it is purely an escape hatch for non-standard / custom prop keys.
- The excess `System.` namespace prefixes make the code look more verbose than necessary.

## Correct Fix (when prioritized)
Replace the `extraProps` block in `SyntheticEventDemoFunc.uitkx` with direct JSX attributes:

```uitkx
<VisualElement
    onPointerDown={(Action<SyntheticPointerEvent>)(e => UpdateLog("PointerDown", e))}
    onPointerMove={(Action<SyntheticPointerEvent>)(e => UpdateLog("PointerMove", e))}
    onPointerUp={(Action<SyntheticPointerEvent>)(e => UpdateLog("PointerUp", e))}
    onWheel={(Action<SyntheticWheelEvent>)(e => UpdateLog("Wheel", e))}
    ...
>
```

Once `SyntheticPointerHandler` / `SyntheticWheelHandler` shorthand delegates are defined in
`ReactiveUITK.Core`, the casts simplify further to:

```uitkx
    onPointerDown={(SyntheticPointerHandler)(e => UpdateLog("PointerDown", e))}
```

## Related Design Inconsistency (separate concern - do not conflate)
`BaseProps.OnPointerDown` etc. are typed as `EventCallback<PointerDownEvent>` (raw UIToolkit struct),
while `InvokeHandler` in `PropsApplier` **also** accepts `Action<SyntheticPointerEvent>` for the same
prop keys via the dynamic dispatch fallback. These two paths are not unified:
- Setting via the typed `BaseProps.OnPointerDown` property => you receive a raw `PointerDownEvent`
- Setting it as a direct `Action<SyntheticPointerEvent>` JSX prop => you receive the richer synthetic wrapper

This inconsistency should be resolved if a single-path event model is ever prioritised.


---

# Tech Debt - Burst AOT Error: `Failed to resolve assembly: Assembly-CSharp-Editor`

## Summary
Unity's Burst compiler logs `Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: 'Assembly-CSharp-Editor'` on every domain reload / asset import. The error appears in the Unity console even outside Play mode.

## Root Cause
Burst performs an AOT assembly scan at editor-time to discover `[BurstCompile]` entry points. It walks a static list of assembly directories, but `Assembly-CSharp-Editor.dll` is dynamically compiled into `Library/ScriptAssemblies` � a directory not in Burst's hardcoded search path � so resolution fails with an exception.

ReactiveUITK has no `[BurstCompile]` methods, so there is no functional impact. However the log noise is distracting and counts as a red error in the console.

## Why This Is Tech Debt
- The error is a false positive � no Burst functionality is broken.
- Red console errors create noise that masks real issues and erodes confidence in build health.
- The fix requires either a Project Settings change or an assembly attribute audit.

## Follow-up (when prioritized)
- Audit all `.asmdef` files in `ReactiveUITK` and any game project for any `[BurstCompile]` assembly-level attributes that should be removed.
- In **Edit > Project Settings > Burst AOT Settings** (Unity 6), add `Assembly-CSharp-Editor` to the assembly exclusion list or restrict the scan to an explicit allowlist.
- Add a note to the package README / setup guide so new project integrators can apply the setting post-installation.

---

# Tech Debt - Hover Shows 30+ Inherited Props (Noise)

## Summary
After adding base-class prop inheritance to `WorkspaceIndex`, hovering over a built-in element
like `<Button>` now shows `Text` from `ButtonProps` plus all 30+ inherited properties from
`BaseProps` (Name, ClassName, Style, OnClick, OnPointerDown, OnPointerUp, etc.). This is
technically correct but produces an overwhelming tooltip that buries the element-specific props.

## Current Behavior
`WorkspaceIndex.ResolveProps` merges all ancestor props into a flat list. `HoverHandler` now
renders only the element's **own props** and shows a count of inherited props (e.g.
`+ 34 inherited from BaseProps`). Completions still use the full resolved list.

## Desired Behavior (future)
- Show a curated subset of common inherited props (Name, ClassName, Style, OnClick, Visible)
  below the own props, with remaining inherited collapsed.
- Use a `[Prop(PropCategory.X)]` attribute system on BaseProps to let property authors control
  categorization (Common, Event, Layout, Lifecycle, Advanced).
- Group inherited props by category with one-line summaries per group.
- Same improvement should apply to completion item detail/documentation sort ordering.

## Follow-up (when prioritized)
- Implement `PropAttribute` + `PropCategory` enum in `Shared/Props/`.
- Annotate `BaseProps` properties with categories.
- Teach `WorkspaceIndex.IndexFile` scanner to parse `[Prop(...)]` attributes.
- Add `Category` field to `PropInfo`.
- Update `HoverHandler` to render grouped display.
- Update `CompletionHandler` to use category for sort-group ordering.

---

# Tech Debt - Markup Parse Errors Emit Cascading "Component Not Found" Error

## Summary
When a parse error occurs inside a JSX paren block in setup code (e.g. missing `;`), Unity shows
**two errors** instead of one: the actual parse error plus a cascading `CS0103: The name
'ComponentName' does not exist in the current context` from a downstream `.cs` file that
references the component.

## Reproduction

Missing `;` after a JSX paren block produces two errors:
```csharp
var inlineNode = (
    <VisualElement>
      <Button text="-5" onClick={_ => setCount(count - 5)} />
      <Button text="+5" onClick={_ => setCount(count + 5)} />
    </VisualElement>
  )   // <-- missing semicolon
```
1. `#error: '; expected'` — from the generated `.g.cs` error stub
2. `CS0103: The name 'UitkxCounterFunc' does not exist` — from a consumer `.cs` file

This does NOT happen with pure C# errors (e.g. missing `;` after a lambda block `};`), because
those still produce a valid `.g.cs` with a compilable class — only the body has the error.

## Root Cause
When the source generator encounters parse errors, it emits an error-stub `.g.cs` with `#error`
directives instead of a real class. This means the component's partial class is never defined,
so any downstream `.cs` file referencing it gets `CS0103`.

## Possible Fix
Emit a minimal compilable skeleton (namespace + partial class + empty `Render`) alongside the
`#error` directives, so the component type still exists for downstream references. The `#error`
still surfaces the parse error, but `CS0103` is avoided.

## Follow-up (when prioritized)
- Modify `UitkxPipeline` error path to emit a skeleton class wrapping the `#error` directives.
- Ensure the skeleton has the correct namespace, class name, and `Render` signature so
  downstream references compile (even though the body is empty/throws).