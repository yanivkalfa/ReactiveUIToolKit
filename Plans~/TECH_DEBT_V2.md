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

---

## 15. JSX `&&` short-circuit splice — extend beyond `{expr}` markup positions

**Status:** Open — partial implementation shipped in v0.5.5 covers the
React idiom `<Box>{cond && <Tag/>}</Box>` (markup `{expr}` and
`attr={expr}` positions). The same syntax in two other expression
positions is **not yet supported** and will produce CS0019 today.

**What's covered (v0.5.5):**
- Child expression: `<Box>{cond && <Tag/>}</Box>`
- Attribute value: `<Wrapper header={cond && <Label/>}/>`
- Both desugared at emit time to
  `((cond) ? V.Tag(...) : (global::ReactiveUITK.Core.VirtualNode?)null)`,
  reusing the already-tested Phase 1 ternary path.

**What's NOT covered:**

1. **Setup-code `&&` JSX** — bare expressions in component setup blocks:
   ```jsx
   component MyComp {
     var node = cond && <Image texture={icon}/>;   // ← CS0019 today
     return (<Box>{node}</Box>);
   }
   ```
   Workaround: rewrite as ternary explicitly:
   `var node = cond ? <Image texture={icon}/> : null;`

2. **Directive-body `&&` JSX** — bare expressions inside `@if`/`@for`/
   `@foreach`/`@while`/`@switch` body code:
   ```jsx
   @foreach (var item in items) {
     var maybeIcon = item.IsActive && <Image texture={item.Icon}/>;
     return (<Row>{maybeIcon}</Row>);
   }
   ```
   Same workaround — rewrite as ternary inline.

**Why deferred:**
- The v0.5.5 desugar lives in `CSharpEmitter.SpliceExpressionMarkup`
  (the single emit-time entry point for `{expr}` / `attr={expr}`).
  Setup code and directive bodies route through different splicers
  (`SpliceSetupCodeMarkup` and `SpliceBodyCodeMarkup`) which currently
  **do not** desugar — they only stitch already-emitted JSX back into
  the surrounding C# verbatim.
- The LHS walker (`FindLhsStartForLogicalAnd`) is already implemented
  as a static helper and would slot into both splicers cleanly.
- The IDE virtual document has parallel scanners
  (`EmitMappedExpressionStrippingJsx` covered in v0.5.5; the larger
  setup-code scanner around `VirtualDocumentGenerator.cs:1970+` is not).
- Risk: the setup-code splicer is older, hand-rolled, and has subtle
  interactions with pool-rent statement hoisting. Touching it for a
  feature most users never hit is not justified by current usage data.

**Trigger to revisit:**
Multiple bug reports of the form "`&&` works inside `{...}` but not in
my `var x = cond && <Tag/>` — why?" If we get more than two such
reports, the consistency argument starts to outweigh the cost.

**Files involved (when this is picked up):**
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `SpliceSetupCodeMarkup`
  and `SpliceBodyCodeMarkup` need the same desugar branch added
- `Editor/HMR/HmrCSharpEmitter.cs` — mirror sites
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` —
  the setup-code scanner around line 1970 needs an `&&` branch added
  parallel to the existing `?` / `:` / `=>` branches
- Reuse the existing `FindLhsStartForLogicalAnd` helper from v0.5.5

**Diagnostic hook (already in place):**
v0.5.5 emits `UITKX0026` when the LHS walker fails. The same diagnostic
ID can be reused for setup/body splicers when they're extended.

---

## 16. Unity 6.3 obsolete API — `AssetDatabase.GetAssetPath(int)` / `EditorUtility.InstanceIDToObject(int)`

**Status:** ✅ COMPLETED (v0.5.7 follow-up).

Unity 6.3 deprecated the `int`-overload `AssetDatabase.GetAssetPath(int)`
and `EditorUtility.InstanceIDToObject(int)` in favour of new overloads
taking the `UnityEngine.EntityId` struct. Two call sites in
`Editor/UitkxConsoleNavigation.cs` (`HandleOnOpenAsset`) surfaced
`CS0618` warnings on 6.3+.

**Resolution (final).** A reflection probe of `UnityEngine.CoreModule.dll`
on 6000.3.8f1 and 6000.4.6f1 revealed two important constraints:

1. **Every `int ↔ EntityId` conversion on `EntityId` is `[Obsolete]`** —
   the `op_Implicit(int)` cast itself carries a deprecation message:
   *"EntityId will not be representable by an int in the future. This
   casting operator will be removed in a future version."*
   So a plain `(EntityId)instanceId` cast still warns.
2. **`OnOpenAssetAttribute` accepts EntityId-typed callbacks on 6.3+** —
   the attribute's internal signature templates include
   `Signature(EntityId)`, `SignatureLine(EntityId, int)` and
   `SignatureLineColumn(EntityId, int, int)`, alongside the legacy
   `int`-typed templates.

The clean migration is therefore not "convert `int` to `EntityId` at the
call site" but "have `[OnOpenAsset]` hand us an `EntityId` directly".
The four callbacks are split by version pragma:

```csharp
#if UNITY_6000_3_OR_NEWER
    [OnOpenAsset]
    private static bool OnOpenAssetCompat(EntityId entityId, int line, int column)
        => HandleOnOpenAsset(entityId, line, column);
    // …
    private static bool HandleOnOpenAsset(EntityId entityId, int line, int column)
    {
        string assetPath = AssetDatabase.GetAssetPath(entityId);
        if (string.IsNullOrEmpty(assetPath))
        {
            var obj = EditorUtility.EntityIdToObject(entityId);
            if (obj != null) assetPath = AssetDatabase.GetAssetPath(obj);
        }
        // …
    }
#else
    [OnOpenAsset]
    private static bool OnOpenAssetCompat(int instanceId, int line, int column)
        => HandleOnOpenAsset(instanceId, line, column);
    // …existing int-path retained verbatim for 6000.2…
#endif
```

The version-independent path resolution and dispatch was extracted into
`ResolveAndDispatch(string assetPath, int line, int column)` so neither
branch duplicates the resolver / external-editor-launch logic. No
obsolete API is touched on 6.3+; the int-typed callbacks remain only on
6.2 where `EntityId` does not exist.

**Files:** `Editor/UitkxConsoleNavigation.cs` (`HandleOnOpenAsset`,
`ResolveAndDispatch`).

---

## 17. `DoomTextures.uitkx` non-nullable Texture2DArray fields trigger CS8618

**Status:** ✅ COMPLETED (v0.5.7 follow-up).

Six fields on `DoomTextures` (`_walls`, `_floors`, `_sprites`, `_sky`,
`_faces`, `_weapons`) were declared as non-nullable `Texture2D` /
`Texture2D[]` but populated lazily by `EnsureBuilt()` after first read.
The compiler flagged each with `CS8618`.

Fixed by suffixing each field with `= null!`, idiomatic for
"framework-initialized later" state. Every public getter routes through
`EnsureBuilt()` so consumers never observe the null state — zero
behaviour change.

**Files:** `Samples/Components/DoomGame/DoomTextures.uitkx`.

---

## 18. SG `using static` injection only fires when peer-hook namespace equals consumer namespace

**Status:** **RESOLVED in 0.5.12 / VS Code 1.2.8 / VS 2022 1.2.8 (2026-05-14).**
Fixed across SG, LSP, and HMR in lockstep. SG `UitkxPipeline` Stage 3d drops
the strict-namespace check (asmdef ownership is enforced one layer up by
`UitkxGenerator`'s pre-scan via `IsOwnedByCompilation`). LSP
`RoslynHost.EnrichWithPeerHookUsings` mirrors the SG fix and adds an
asmdef gate via the new `AsmdefResolver`. HMR `EmitCompanionUitkxSources`
adds a registry-backed second pass for cross-directory hook FQNs. New
tests: `HookUsingStaticCrossNamespaceTest`, `HookUsingStaticAsmdefBoundaryTest`,
`HookCrossNamespaceVirtualDocTests` (LSP), `AsmdefResolverParityTests`.

**Status:** Open — **Priority: HIGH**.

**Bug.** `UitkxPipeline.cs` Stage 3d injects `using static Ns.HookContainer`
into a component's generated source **only if** the hook file's
`@namespace` is byte-identical to the component file's `@namespace`. As a
result, lowercase hook aliases (`useFoo()`) silently fail to resolve when
the hook lives in a namespace that's "close but not equal" — e.g. parent
or sibling under a shared root.

**Repro.**
- `Assets/UI/Hooks/UseUiDocumentSlot.hooks.uitkx` declares `@namespace PrettyUi.UIHooks`.
- `Assets/UI/Pages/MenuPage/MenuPage.uitkx` declares `@namespace PrettyUi.App.Pages`.
- `MenuPage` calls `useUiDocumentSlot(...)`.
- Result: `CS0103: The name 'useUiDocumentSlot' does not exist in the current context`
  in the Unity Console, **even though** both files compile into the same
  `Assembly-CSharp.dll`. Workaround today is `@using static PrettyUi.UIHooks.UseUiDocumentSlotHooks`
  in every consumer — error-prone, easy to forget, and inconsistent with
  the same-namespace case which works automatically.

**Why it's strict equality today.** Comment in
`SourceGenerator~/UitkxPipeline.cs` ~L231 explains the original intent:
"avoid polluting unrelated assemblies." That concern is already handled
upstream — `UitkxGenerator.cs` L142-L156 pre-filters
`peerHookContainersBuilder` by `IsOwnedByCompilation(...)`, so the
`peerHookContainers` list is already scoped to the **owning compilation**
(asmdef boundary). The additional namespace-equality check is therefore
redundant for cross-asmdef isolation and only serves to break
ergonomics within an assembly.

**Fix approach (per-compilation injection).**

1. **`SourceGenerator~/UitkxPipeline.cs` Stage 3d** — drop the
   `phc.Namespace == directives.Namespace` guard. Inject
   `using static <Ns>.<ClassName>` for **every** entry in
   `peerHookContainers`. Deduplicate via a `HashSet<string>`
   (`StringComparer.Ordinal`) so `directives.Usings` is not duplicated
   when two hook files share a `<Ns>.<ClassName>` (which is legal —
   `HookEmitter.cs` L86 emits `partial static class`, so adjacent hook
   files with the same container merge naturally).
2. **Self-import guard.** A hook file should not `using static` itself.
   Skip when `phc.Namespace == directives.Namespace &&
   phc.ClassName == HookEmitter.DeriveContainerClassName(filePath)`.
   For pure component files (no own hook container) this guard is a no-op.
3. **`Editor/HMR/HmrCSharpEmitter.cs`** — mirror the same logic so
   HMR-recompiled types use the identical `using` list. The HMR emitter
   uses its own peer scan (search for the matching `phc.Namespace ==`
   block); apply the same dedup + self-skip pattern.
4. **Tests.** Add to `SourceGenerator~/Tests/`:
   - Hook in `NsA.UIHooks` + component in `NsB.Pages` → component
     compiles with auto-injected `using static NsA.UIHooks.MyHookHooks`.
   - Two hook files defining `useFoo` in different namespaces → consumer
     gets a clean `CS0121: ambiguous call` (not a generator crash).
   - Hook file in same namespace as consumer (existing case) → no
     duplicate `using static` in the emitted source.
5. **HMR parity test** in `HmrEmitterParityContractTests.cs` confirming
   the SG and HMR emitters produce identical using-static blocks.

**Risk: Low.** Pre-scan is already per-compilation. No new fields, no
API changes, no new analyzers. Worst-case behaviour change is a clean
`CS0121` if two hooks share a name across namespaces in the same
assembly — that's the *correct* outcome (the dev needs to qualify),
and currently same-namespace hooks already produce the same error.

**Tradeoff vs. status quo.**
| Concern | Today | Proposed |
|---|---|---|
| Cross-folder hook in same assembly | Manual `@using static` per consumer | Auto-injected |
| Cross-asmdef hook | Doesn't work either way (different assembly entirely) | Same |
| Per-file usings count | ~1-3 hook containers max | Same in practice; one dedup `HashSet` allocation per emit |
| Compile-time ambiguity if two hooks share a name | Local — only same-namespace | Global within assembly (correct) |
| HMR emitter parity | Must match | Same — emit logic is identical |

**Files:**
- `SourceGenerator~/UitkxPipeline.cs` (Stage 3d ~L231-L246)
- `SourceGenerator~/PeerHookContainerInfo.cs` (no change; record already
  carries `Namespace` + `ClassName`)
- `SourceGenerator~/Emitter/HookEmitter.cs` (`DeriveContainerClassName`
  already public-internal; reuse for self-skip)
- `Editor/HMR/HmrCSharpEmitter.cs` (matching peer-hook block)
- `SourceGenerator~/Tests/EmitterTests.cs` + `HmrEmitterParityContractTests.cs`

**Surfaced by:** PrettyUi cross-namespace `useUiDocumentSlot` consumer
(2026-05-14). User feedback: "incosistant and sami guess work".

---

## 19. LSP Roslyn workspace ignores `.cs` files outside the `.uitkx` directory

**Status:** **RESOLVED in 0.5.12 / VS Code 1.2.8 / VS 2022 1.2.8 (2026-05-14).**
`WorkspaceIndex` now tracks every indexed `.cs` file (`_allCsFiles` /
`GetAllCsFiles()`) and `RoslynHost.FindCompanionFiles` unions same-folder
`.cs` with workspace-wide `.cs` filtered to the consumer's asmdef via the
new `AsmdefResolver` (mirrored verbatim under `Editor/HMR/` and the LSP).

**Status:** Open — **Priority: HIGH**.

**Bug.** When a `.uitkx` file (component, hook, or module) references a
type defined in a `.cs` file that lives in a different directory, the
LSP shows a false-positive `CS0103` / "does not contain a definition"
squiggle in VS Code, **even though the Unity compile succeeds** (Unity
feeds the entire `Assembly-CSharp` source set to `csc`).

**Repro.**
- `Assets/UI/Hooks/UseUiDocumentSlot.hooks.uitkx` references
  `UIDocumentSlot.SlotChanged` (a static event).
- `UIDocumentSlot.cs` originally lived at `Assets/Scripts/UIDocumentSlot.cs`.
- LSP squiggle: `'UIDocumentSlot' does not contain a definition for 'SlotChanged'`.
- Workaround today: move the `.cs` file into the same directory as the
  consuming `.uitkx`. This is fragile (encourages bad project structure)
  and breaks for users who organise scripts under a single `Assets/Scripts/`
  root.

**Why this happens.** `RoslynHost.cs` builds an `AdhocWorkspace` per opened
`.uitkx` and adds three categories of documents:
1. The `.uitkx` itself (as a virtual `.cs` document).
2. Same-directory `.uitkx` peers (recently extended in v0.5.11 to also
   include workspace-wide module/hook `.uitkx` files via
   `WorkspaceIndex.GetModuleAndHookFiles()`).
3. **Same-directory `.cs` companion files only** — see
   `RoslynHost.AddPeerCsFiles` / `AddPeerCsFilesToSolution`.

There is no equivalent of `WorkspaceIndex.GetModuleAndHookFiles()` for
arbitrary `.cs` files, so cross-directory C# types are invisible to the
language server.

**Fix approach (workspace-wide `.cs` index).**

Two viable strategies — pick one based on perf measurement:

**Option A — eager full-workspace index (simple, may bloat memory).**
1. Extend `WorkspaceIndex` with a new tracked set:
   `private readonly HashSet<string> _allCsFiles = new(StringComparer.OrdinalIgnoreCase);`
2. Populate during `IndexFile(filePath)` for every `.cs` file (already
   walked for `*Props.cs` extraction) — no extra IO.
3. Expose `IReadOnlyList<string> GetAllCsFiles()` under read-lock.
4. `RoslynHost.AddPeerCsFiles` (and `…ToSolution` variant) appends every
   workspace `.cs` after the same-directory peers, deduped by absolute
   path. Each becomes a real Roslyn `Document` so semantic resolution
   works for any symbol in the assembly.
5. **Asmdef boundary.** To match Unity's compilation model, optionally
   filter the appended `.cs` files by the `.uitkx`'s asmdef ownership
   (`UitkxPipeline.IsOwnedByCompilation` already exists in the SG; lift
   into a shared helper in `language-lib` or `lsp-server`). Without this
   filter, IDE resolution is *more permissive* than Unity (might
   suggest a type that's actually unreachable at runtime), but never
   *less permissive* — so the squiggle goes away in all real cases.

**Option B — lazy, demand-driven `.cs` lookup (heavier code, lighter memory).**
1. On semantic-failure (e.g. unresolved identifier in
   `DefinitionHandler` / `CompletionHandler`), perform a workspace
   text-scan for `class <Name>` / `public static class <Name>` matching
   the unresolved symbol, then add the matching `.cs` file as a peer
   document and re-resolve.
2. Avoids loading `Assets/Plugins/**/*.cs`, `Library/**`, etc.
3. Significantly more complex — adds an unresolved-symbol pump and
   cache invalidation.

**Recommendation: Option A with asmdef filter.** Memory cost is bounded
by the number of `.cs` files in the project (typically < 10k for game
code), Roslyn's incremental compile handles re-parses efficiently, and
the impl is a 30-50 line change matching the v0.5.11 module/hook fix
pattern.

**Risk: Medium.** More documents in the workspace means more memory
held by the language server process and more work for Roslyn on first
open per `.uitkx` (one-time cost, cached per session). Mitigations:
- Cap with a soft warning at e.g. 10000 files (log once, continue).
- Lazy-add: only add a `.cs` file as a Roslyn document on first
  semantic request that would otherwise fail. Avoids paying the cost
  for `.uitkx` files that don't need any cross-directory C# resolution.
- Use `DocumentInfo.Create(...)` with `LoadTextLazily(...)` so file IO
  is deferred until Roslyn actually parses a given document.

**Why this matters.** With v0.5.11 we made cross-directory module/hook
`.uitkx` symbols resolve; the `.cs` side is the symmetric missing
piece. Together they make consumer projects organise files by domain
(`Assets/UI/...`, `Assets/Game/...`, `Assets/Scripts/...`) without
fighting the LSP. Without this fix, every cross-directory C# reference
shows a squiggle that the user has learned to ignore — eroding trust
in real diagnostics.

**Files:**
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` (new `_allCsFiles`
  set + `GetAllCsFiles()`; populate in existing `IndexFile` path; clear
  in `Refresh` deletion branch)
- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`
  (`AddPeerCsFiles` + `AddPeerCsFilesToSolution`: append workspace `.cs`
  files after same-directory peers, deduped, optionally asmdef-filtered)
- `ide-extensions~/lsp-server/Tests/` — add cross-directory C# resolution test

**Surfaced by:** PrettyUi `UIDocumentSlot.cs` in `Assets/Scripts/`
referenced from hook in `Assets/UI/Hooks/` (2026-05-14). Workaround:
moved `.cs` file next to the hook. Not sustainable as a long-term answer.

---
