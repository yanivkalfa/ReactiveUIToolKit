# Tech Debt v2

Consolidated tech debt tracker. Includes carryover items from the original
`TECH_DEBT.md` and new items identified during v0.3.1 work.

---

## 1. Button missing sub-element slot for styling inner Label

**Status:** Skipped

---

## 2. `VisualElementSafe` safe area mismatch in Unity Simulator

**Status:** Skipped

---

## 3. Rename Symbol renames same-named lambda params across unrelated lambdas

**Status:** Skipped

---

## 4. Comment toggle (Ctrl+/) misdetects pure C# as markup

**Status:** ✅ COMPLETED

**Bug:** Selecting pure C# code inside a component body and pressing `Ctrl+/`
wraps it in `{/* */}` instead of inserting `//` on each line.

**Repro (VS Code):**
1. Open `UitkxTestFileDoNotTouch.uitkx`
2. Select the following pure-C# block:
```csharp
MenuBuilderHandler buildMenu = dm =>
  {
    dm.AppendAction("Reset", _ => Reset());
    dm.AppendAction("Set 10", _ => setCount(10));
    dm.AppendAction("Set 100", _ => setCount(100));
    dm.AppendAction("Shuffle", _ => ShuffleOptions());
  };
```
3. Press `Ctrl+/`
4. **Actual:** entire block wrapped in `{/* */}`
5. **Expected:** each line prefixed with `//`

**Root cause (likely):** `isInsideCodeBlock()` in `extension.ts` tracks brace
depth from document start. The function body `component Foo {` opens one brace
level, but the heuristic may not recognize the cursor as being inside "code"
vs "markup" when the component mixes both. The `looksLikeMarkupSelection()`
check may be falling through to the block-comment path.

**Files to investigate:**
- `ide-extensions~/vscode/src/extension.ts` — `isInsideCodeBlock()`,
  `looksLikeMarkupSelection()`, `uitkx.toggleBlockComment` handler
- `ide-extensions~/visual-studio/UitkxVsix/UitkxCommentHandler.cs` — same logic

**Root cause:** `isInsideCodeBlock()` (extension.ts lines 42–177) only recognizes
explicit `@code { }` blocks (Blazor-style). It scans from document start looking
for the `@code` keyword followed by `{` to set `inCode = true`. In function-style
components (`component Foo { ... return (...); }`) there is no `@code` keyword —
`inCode` stays `false` for the entire file.

When the handler runs (lines 490–523):
```
const inCode = isInsideCodeBlock(document, effectiveRange.start);  // false!
const isMarkupSelection = looksLikeMarkupSelection(selectedText);
if (inCode && !isMarkupSelection) → lineComment   // never reached
else → jsxComment                                  // always taken
```

**Fix approach:** Rewrite `isInsideCodeBlock()` to understand function-style
components. After detecting `component Name(...) {`, track brace depth and
consider everything BEFORE `return (` as C# code. The return finder logic
already exists in the language-lib (`TryFindTopLevelReturn`) — the extension
needs an equivalent heuristic:
1. Find `component` keyword + opening `{`
2. Track brace depth (skip strings/comments)
3. Find `return (` at depth 1
4. Everything between component `{` and `return (` is C#
5. Everything inside `return (...)` follows the existing markup/code detection
6. Control block setup code (between `@if {` and `return (`) is also C#

**Effort:** Medium — the character scanning is already there, just needs the
function-style component awareness added alongside the existing `@code` path.
VS2022 handler needs the same fix.

---

## 5. Formatter non-idempotent on bare `return <Element>` inside control blocks

**Status:** ✅ COMPLETED

**Bug:** When a bare `return <Element>` (no parens) is used inside a control
block, the formatter takes **two passes** to reach the canonical form. The first
format produces incorrect indentation; a second format-on-save stabilizes.

**Repro:**
1. Write this inside a component setup code:
```uitkx
var inlineNode = (
    <Portal target={portalTarget}>
      @if (mounted) {
        return <Button
            text="Portal Button (click me)"
            onClick={_ => AppendLog("Portal button clicked")}
            style={new Style { (StyleKeys.MarginTop, 6f) }}
          />
      } @else {
        return (
          <Label text="Portal unmounted." />
        );
      }
    </Portal>
  );
```
2. Save (format-on-save) → **first pass produces broken indentation:**
```uitkx
var inlineNode = (

    <Portal target={portalTarget}>
    @if (mounted) {
      return (<Button
      text="Portal Button (click me)"
      onClick={_ => AppendLog("Portal button clicked")}
      style={new Style { (StyleKeys.MarginTop, 6f) }}
      />);
    } @else {
      return (
      <Label text="Portal unmounted." />
      );
    }
    </Portal>
  );
```
3. Save again → **second pass produces the correct form:**
```uitkx
var inlineNode = (
    <Portal target={portalTarget}>
      @if (mounted) {
        return (
          <Button
            text="Portal Button (click me)"
            onClick={_ => AppendLog("Portal button clicked")}
            style={new Style { (StyleKeys.MarginTop, 6f) }}
          />
        );
      } @else {
        return (
          <Label text="Portal unmounted." />
        );
      }
    </Portal>
  );
```

**Root cause (likely):** `NormalizeBareJsx()` injects synthetic parens around
`return <Button .../>` but the injected positions interact poorly with the
indentation engine on the first pass. The synthetic paren tracking
(`insertedOpenParenPositions`) may not account for the nesting depth of
control-block bodies.

**Impact:** Not data-corrupting — double-save always converges. But annoying
for users and breaks formatter idempotency tests.

**Files to investigate:**
- `ide-extensions~/language-lib/Formatter/AstFormatter.cs` — `NormalizeBareJsx()`,
  `FormatFunctionStyleComponent()`, indent tracking for control-block returns
- Formatter snapshot tests — add a bare-return-in-control-block case

**Root cause:** The formatter pipeline for bare `return <Element>` has an
indentation mismatch between the synthetic paren injection and the placeholder
replacement:

1. `NormalizeBareJsx()` (line 1948) injects `(` right after `return`, producing
   `return (<Button .../>)` — paren and element on the same line.

2. `EmitSetupCodeWithJsx()` (line 1544) replaces the JSX with a placeholder and
   calls `EmitCSharpLines()` to format the surrounding C# (including control
   blocks). This re-indents the placeholder line based on brace depth.

3. Placeholder replacement (lines 1632–1655) extracts the formatted line's
   indentation to compute `jsxIndent`. But the indentation was set by
   `EmitCSharpLines` relative to the control block nesting, while the JSX
   content's original position context is lost. Result: wrong indent level
   on first pass.

4. On the **second pass**, the input already has `return (` on its own line
   (from first-pass output). `NormalizeBareJsx` doesn't fire (not bare anymore).
   The standard parenthesized-return path handles it correctly.

**Fix approach:** When `NormalizeBareJsx` injects synthetic parens for a bare
`return <Tag`, it should inject them as a **line break** (`return (\n<Tag`)
rather than inline (`return (<Tag`). This way the first-pass output matches
what the second pass would see, achieving idempotency.

Alternatively, the placeholder replacement logic (lines 1632–1655) should
always emit `return (` on its own line and compute `jsxIndent` from the
control-block nesting depth rather than from the formatted line's indentation.

**Effort:** Small-medium — the fix is localized to `NormalizeBareJsx()` or
the placeholder replacement block. Need a new formatter snapshot test with
bare returns inside `@if`/`@for` control blocks.

---

## 6. Companion file IDE support (IntelliSense, coloring, formatting)

**Status:** ✅ COMPLETED

Companion files are now `.uitkx` files using `hook` and `module` keywords
instead of `.cs` files. Both VS Code and VS2022 provide full IDE support:
parsing, diagnostics, hover, completions, coloring, and formatting —
all handled by the shared LSP server with no special middleware needed.

---

## 7. Formatter destroys hook/module files on save

**Status:** ✅ COMPLETED

**Bug:** When a `.uitkx` file containing `hook` or `module` declarations is
saved (or formatted), the `AstFormatter` rewrites the entire file content to
`component Component { return (); }` — complete data loss without undo.

**Root cause:** `AstFormatter.Format()` checks `directives.IsFunctionStyle`
(which is `true` for hook/module files) and routes to
`FormatFunctionStyleComponent()`. That method doesn't know about hooks/modules
— it falls back to `componentName = "Component"`, emits an empty scaffold,
and discards all original content.

**Fix:** At the top of `AstFormatter.Format()`, check
`directives.HookDeclarations` / `directives.ModuleDeclarations` — if either is
populated, return early (no formatting) or implement a dedicated hook/module
formatter.

**Files:**
- `ide-extensions~/language-lib/Formatter/AstFormatter.cs` (lines 70-76)

---

## 8. TextMate grammar coloring broken for hook/module files

**Status:** ✅ COMPLETED

**Bug:** Syntax coloring inside `hook` and `module` bodies is broken. Words
are fragmented into multiple colors, and the coloring doesn't make sense.
The `hook` and `module` keywords themselves may not color correctly either.

**Likely cause:** The grammar patterns for `hook-declaration` and
`module-declaration` use `match` (single-line) which captures the declaration
line, but the body content (everything inside `{ ... }`) falls through to
top-level patterns that expect UITKX markup, not pure C# code. Hook/module
bodies need to be scoped as embedded C# using a `begin`/`end` pattern that
properly delegates to `#expression-content` or the C# grammar.

**Files:**
- `ide-extensions~/grammar/uitkx.tmLanguage.json`
- `ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json`

---

## 9. IntelliSense errors in component consuming custom hooks

**Status:** ✅ COMPLETED — resolved by peer workspace infrastructure

**Bug:** In `TestHome.uitkx`, calling `useTestHomeState()` produces:
- `The name 'useTestHomeState' does not exist in the current context`
- `Cannot infer the type of implicitly-typed deconstruction variable 'gameStarted'`
- `The name 'DetailsStyles' does not exist in the current context`

**Note:** The peer `.uitkx` document infrastructure added during v1.0.308
(`AddPeerUitkxDocuments`, `AddPeerUitkxDocumentsToSolution`,
`EnrichWithPeerHookUsings`) may have resolved this. The LSP now loads
hook/module `.uitkx` files as peer documents in the Roslyn workspace,
and auto-injects `using static` directives for peer hook container classes.
Needs re-testing to confirm whether these errors still reproduce.

**Likely causes (if still present):**
1. The test project (`Tic-tac-toe`) has an **older version** of the source
   generator DLL that doesn't know about `hook`/`module` keywords. The new
   DLL was only deployed to the ReactiveUIToolKit workspace's `Analyzers/`
   folder — not to the consumer project's local package cache.
2. The VDG (VirtualDocumentGenerator) builds each `.uitkx` file in
   isolation. Component files don't automatically see types generated by
   sibling hook/module `.uitkx` files. The Roslyn AdhocWorkspace needs all
   generated sources to resolve cross-file references.
3. Module `TestHome` generates `partial class TestHome` — but only if the
   source generator runs. Without it, the styles are invisible.

**Fix path:**
- Ensure the consumer project gets the updated source generator DLL
- Verify VDG/RoslynHost includes all `.uitkx` generated sources in the
  workspace compilation (not just the current file)
- May need to add hook container classes to the virtual document's scope

**Files:**
- `SourceGenerator~/Emitter/HookEmitter.cs`
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`

---

## 10. Lambda return type mismatch in hook body

**Status:** ✅ COMPLETED (v1.0.308 — netstandard.dll ref fix + CS1662 cascade suppression)

**Bug:** In `TestHome.hooks.uitkx`, the line:
```
Func<int, int, PointerEventHandler> handleColumnClick = (row, col) => _ => {
```
produces: `Cannot convert lambda expression to intended delegate type because
some of the return types in the block are not implicitly convertible to the
delegate return type`.

**Likely cause:** The VDG hook stubs don't include `PointerEventHandler`
in the virtual document's usings or type scope. Without the proper type
resolution, the nested lambda's return type can't be inferred. May also be
a genuine C# type inference limitation with nested lambdas requiring explicit
parameter types.

**Files:**
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`

---

## 11. Hook ownership model — no implicit component binding

**Status:** Open — DESIGN QUESTION

**Context:** Hooks are generated into a **static container class** derived
from the filename (e.g. `TestHome.hooks.uitkx` → `TestHomeHooks` class).
They are **not** automatically scoped to a specific component — any component
in the same namespace can call them.

**Current model:**
- `hook useX()` in `Foo.hooks.uitkx` → `public static class FooHooks`
- Component calls `FooHooks.useX()` (or just `useX()` if hook aliases
  are extended to cover custom hooks)
- Hooks and components are connected by **namespace**, not by filename

**Open question:** Should we support `hook ComponentName useX()` syntax
to generate the hook as a method on the component's partial class instead?
This would make `useX()` callable without qualification inside the component.
Trade-off: less reusable, more coupled.

**Decision needed before:** documentation / public API finalization.

---

## 12. Cross-file diagnostic staleness on tab switch

**Status:** IN PROGRESS — fix implemented, pending VS2022 verification

**Bug:** When two `.uitkx` files depend on each other (e.g. file B uses a
hook/type from file A), editing file A and then switching to file B shows
stale diagnostics. The errors/fixes don't refresh until the user makes an
edit (even a space) in file B.

**Root cause:** Two issues:
1. Peer `.uitkx` content was read from **disk** (`File.ReadAllText`) instead
   of the in-memory editor buffer. Unsaved edits in an open peer were invisible.
2. `RevalidateOpenDocuments` passed `roslynHost: null`, skipping T3 Roslyn
   rebuilds — only T1+T2 diagnostics were pushed.

**Fix (implemented):**
- Added `TryGetByPath()` to `DocumentStore` for path-based editor buffer lookup
- Added `ReadPeerSource()` to `RoslynHost` — prefers editor content over disk
- Both `AddPeerUitkxDocuments` and `AddPeerUitkxDocumentsToSolution` now use
  `ReadPeerSource()` instead of `File.ReadAllText()`
- `DiagnosticsPublisher.SetRoslynHost()` wired at startup so
  `RevalidateOpenDocuments` triggers full T3 Roslyn rebuilds
- Removed 7 hot-path spam logs from the server

**Remaining:** Verify behavior in VS2022 extension.

**Files changed:**
- `ide-extensions~/lsp-server/DocumentStore.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`
- `ide-extensions~/lsp-server/DiagnosticsPublisher.cs`
- `ide-extensions~/lsp-server/Program.cs`

---

## 13. Hover missing for variable declarations and delegate types

**Status:** ✅ COMPLETED

**Bug:** Hover tooltips are absent for variable declarations and show
unhelpful text for delegate types. Autocomplete works correctly in all
file types.

**Concrete test results (TicTacToe sample):**

In `TicTacToe.uitkx`:
- `useTestHomeState()` hover ✅ works correctly
- Destructured variables (`gameStarted`, `playerTurn`, `grid`, etc.) on
  the LEFT side of `var (...) = useTestHomeState()` — **no hover** ❌

In `TicTacToe.hooks.uitkx`:
- Local variables like `handleStartGame`, `handleRestartGame`,
  `handleColumnClick`, `haveAWinner` — **no hover** ❌
- `PointerEventHandler` type name — shows `(namedtype) PointerEventHandler`
  instead of the delegate signature ❌

In `TicTacToe.style.uitkx`:
- Style names (`containerStyle`, `ButtonStyles`) — no hover. Expected —
  these are module fields, not yet instrumented.

**Root cause:** `HoverHandler.TryGetRoslynHover()` uses
`semanticModel.GetSymbolInfo()` and `GetTypeInfo()` to resolve symbols.
These APIs return nothing for **declarations** (variable declarations,
parameters, tuple designations) — only for **references/expressions**.
The correct API for declarations is `semanticModel.GetDeclaredSymbol()`,
which is **never called**.

Affected declaration types:
- `SingleVariableDesignationSyntax` (tuple destructuring: `var (a, b) = ...`)
- `VariableDeclaratorSyntax` (locals: `Type name = ...`)
- `ParameterSyntax` (lambda params: `x => ...`)

Additionally, for delegate types, `sym.Kind.ToString()` returns the raw
enum name `"namedtype"` instead of `"delegate"`. The hover doesn't show
the delegate's invoke signature (`void PointerEventHandler(PointerEventBase evt)`).

**Fix approach:**
1. Add `semanticModel.GetDeclaredSymbol(token.Parent)` fallback before
   the null check — covers all variable declarations and parameters
2. For `INamedTypeSymbol` with `TypeKind == Delegate`, display the
   `DelegateInvokeMethod` signature and show kind as `"delegate"`
3. Map other `TypeKind` values: `class`, `struct`, `interface`, `enum`

**Effort:** Small — all changes in `HoverHandler.TryGetRoslynHover()`

**Files:**
- `ide-extensions~/lsp-server/HoverHandler.cs` — `TryGetRoslynHover()`

---

## 14. Synthetic event dispatcher for cross-panel portal bubbling

**Status:** Open — DESIGN / FUTURE

**Context:** `V.Portal(target, ...)` already supports cross-panel targets
end-to-end. One `Render(...)` call can fan out into N Unity panels living
on N GameObjects (e.g. World Space HUD on the player rig + World Space
popup on a bottle), all sharing one fiber tree, hooks, state, context,
error boundaries, and suspense.

**Gap vs. React DOM:** React DOM portals propagate events through the
**React component tree** via a synthetic event system (single root-level
listener, custom dispatch walking React parents — including jumping out
of a portal back up to the portal's parent component).

This library registers handlers via `element.RegisterCallback<T>(...)`
directly with Unity's per-panel `BaseVisualElementPanel` dispatcher
(`Shared/Props/PropsApplier.cs` ~L2095). So bubbling is panel-local:
a click on `<Button>` inside `<Portal target={panelB}>` does **not**
surface to a `<VisualElement onClickCapture={...}>` ancestor in panel A.

**Userland workaround (works today, recommended for now):** pass callback
props or use a context-provided event bus. Strictly more flexible than
DOM bubbling — any descendant can subscribe, not just lineal ancestors.
State setters already propagate cross-panel because they're plain
delegates closing over fiber state.

**Proposed implementation (Level 2/3 from the design discussion):**

1. **Per-panel root listener.** When a portal mounts children into a
   foreign panel, install one capture listener on that panel's root for
   each event type the fiber tree subscribes to. Reference-count by
   event type so unmounting the last subscriber releases the listener.

2. **Fiber-walk dispatch.** When the root listener fires, locate the
   target VE's owning fiber (back-pointer from `VisualElement.userData`
   → `NodeMetadata` → `FiberNode`), then walk **up the fiber tree**
   (not the VE tree). Invoke registered handlers in capture order on
   the way down, bubble order on the way up. Honor a synthetic
   `stopPropagation` flag distinct from Unity's native one.

3. **Registration change in `PropsApplier`.** Stop calling
   `element.RegisterCallback` for fibered handlers; store them in
   `NodeMetadata.EventHandlers` only and let the synthetic dispatcher
   route. Keep the native path as an opt-out for handlers that need raw
   Unity dispatch (FocusEvent, IMGUI integration, perf-critical paths).

4. **Recommended ship vehicle: Level 3 (hybrid, opt-in per portal).**
   Add a `synthetic` attribute on `<Portal>`:
   ```jsx
   <Portal target={popupSlot} synthetic>   {/* React-style bubbling */}
     <Button onClick={...} />
   </Portal>
   ```
   Default stays Unity-native (zero overhead, predictable). Synthetic
   flag opts the subtree into the new dispatcher. Cheapest way to give
   users React parity *when they ask for it* without forcing the cost
   on everyone.

**Files to touch:**
- `Shared/Props/PropsApplier.cs` — split native vs. synthetic
  registration paths (`RegisterEvent<T>`, `ApplyEvent`, `RemoveEvent`)
- `Shared/Core/Fiber/FiberHostConfig.cs` — after `AppendChild` to a
  portal target, ensure the target's panel has a `SyntheticDispatcher`
  attached
- `Shared/Core/Fiber/FiberNode.cs` — add `Fiber` back-pointer field on
  `NodeMetadata` (or equivalent VE → fiber lookup)
- `Shared/Core/V.cs` + `Shared/Core/VNode.cs` — add `synthetic` flag
  to `V.Portal(...)` factory
- New: `Shared/Core/SyntheticDispatcher.cs` — per-panel root listener,
  per-event-type fiber-walk, capture/bubble ordering, synthetic
  `stopPropagation`
- `SourceGenerator~/Emitter/CSharpEmitter.cs` + `Editor/HMR/HmrCSharpEmitter.cs`
  — emit the `synthetic` attribute through `EmitPortal`
- `ide-extensions~/grammar/uitkx-schema.json` — add `synthetic` to
  Portal's known attributes for autocomplete
- New tests under `SourceGenerator~/Tests/` and a runtime fiber test
  validating cross-panel bubble + capture ordering and `stopPropagation`

**Effort:** Medium-large — ~600–1000 lines, careful test coverage,
real perf consideration (event delegation pattern à la React; aggressive
event-args pooling).

**Priority:** Low for now. Userland callback-prop / event-bus pattern
is documented and sufficient for current use cases (HUD + World Space
popups + connector lines). Re-evaluate once we have real game code
hitting this — if missing bubbling becomes a daily papercut rather
than a once-a-year curiosity, ship Level 3.

**Related:**
- `Shared/Core/PortalContextKeys.cs` — named-slot pattern that already
  enables the multi-panel architecture
- `Samples/Components/PortalEventScopeDemoFunc/PortalEventScopeDemoFunc.uitkx`
  — sample explicitly verifying current panel-local event scoping
  (would need updating once synthetic dispatch lands)
- `Editor/EditorRootRendererUtility.cs` + `Runtime/Core/RootRenderer.cs`
  — `HostContext.Environment` seeding entry points
