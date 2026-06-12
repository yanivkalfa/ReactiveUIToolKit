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

## 20. HMR does not cascade to module consumers (cross-file stale values)

**Status:** Open — **Priority: CRITICAL** (next-up).

**Bug.** Editing a value in a module file (e.g. `Theme.Accent` in
`Theme.uitkx`) recompiles the module successfully under HMR, but every
consumer that captured that value at cctor time keeps rendering the
stale value until a full domain reload. The `[HMR]` log reports success
for the edited file; visual output does not change.

**Repro (PrettyUi, 2026-05-17).**
- `Theme.uitkx` declares `public static readonly Color Accent = ...`.
- `StatsPanel.style.uitkx` declares `public static readonly Style Container = new Style { BorderColor = Theme.Accent, ... }`.
- `StatsPanel.uitkx` consumes `Container`.
- Edit `Theme.Accent`, save. HMR logs `[HMR] Swapped Theme`. `StatsPanel`
  continues to render the old border colour.
- Domain reload fixes it.

**Three converging root causes.**

1. **No `_moduleDependents` reverse-dep map.** `UitkxHmrController`
   maintains `_ussDependents` for USS → UITKX cascade ([Editor/HMR/UitkxHmrController.cs](../Editor/HMR/UitkxHmrController.cs#L59))
   but nothing equivalent for module → consumer. When `Theme.uitkx`
   recompiles, the controller has no way to find `StatsPanel.style.uitkx`
   (let alone its transitive consumers) and enqueue them for the same
   HMR batch.
2. **Consumer cctor captured values by value.** Even if the consumer is
   recompiled, the 0.5.9 `[UitkxHmrSwap]` rewrite re-runs the field
   initializer per HMR cycle, which re-reads `Theme.Accent` and updates
   the captured value — so this part works *once the consumer is in the
   swap set*. Today it isn't.
3. **`const` is inlined at compile time.** Module `const float`
   (`Theme.NavHeight`, `Theme.Radius`, `Theme.BorderWidth`, ...) is
   baked into every consumer's IL at SG/Roslyn compile time. No HMR
   mechanism can update these — the consumer's IL would have to be
   rewritten. Recompiling the consumer via (1) does pick the new
   constant up, so cascading is the fix.

**Fix approach.**

- **A. Reverse-dep map.** Build `_moduleDependents` mirroring
  `_ussDependents` shape. Source: scan each `.uitkx` for
  (i) bare references to top-level `module <Name>` symbols
  (`<Name>.<Member>` token sequences), and (ii) the SG-injected
  `using static <Module>;` lines from the same-asmdef cross-file pass
  (0.5.12). Update incrementally per watcher event; rebuild on `Start`.
- **B. Cascade enqueue.** On change to file `F.uitkx` that declares a
  module, enqueue `F` + every transitive dependent in topological order
  into the same HMR batch. The 0.5.9 swap loop already re-runs cctors
  per `[UitkxHmrSwap]` field, so consumers pick up the new captured
  values automatically once recompiled. The 0.5.10 trampoline picks
  up new component bodies the same way.
- **C. (Optional, additive) Const → static readonly promotion.** Either
  rewrite `const` to `static readonly` inside `module { … }` at SG
  emit time, or ship a new analyzer (`UITKX0211`, info or warning)
  recommending the change. Pure C# `const` will never participate in
  HMR by language semantics; this closes the gap for the
  most-frequently-edited primitive values (paddings, sizes, weights).

**Why deferred.** Touches `UitkxHmrController`, the file watcher's
event payload (needs path → declaring-modules-in-file index), and a
new dep-scanner. Sizeable change with non-trivial test surface
(transitive cascade, cycle detection, asmdef-scoping, USS-style
coalescing inside the existing debounce window). Workaround for now:
exit Play mode after editing `Theme.uitkx` / any module a tree of
consumers reads at cctor time.

**Files (estimated).**
- `Editor/HMR/UitkxHmrController.cs` — new `_moduleDependents` field,
  topological enqueue in the swap pipeline, mirror existing
  `_ussDependents` lifecycle hooks (Start/Stop/Refresh).
- `Editor/HMR/UitkxHmrFileWatcher.cs` — surface module-declaring-file
  set so the controller can map a changed file to its declared module
  names.
- New `Editor/HMR/ModuleDependencyScanner.cs` — text-scan `.uitkx`
  files for `<Module>.<Member>` references gated by known module names
  from the workspace (or the SG's cross-file pass).
- `SourceGenerator~/Emitter/ModuleEmitter.cs` (optional, for C) — emit
  `static readonly` instead of `const` for primitive module values
  marked for HMR, or analyzer-only.
- Tests: dep-scanner unit tests, controller cascade tests (single
  module, transitive chain, cycle, no-dependents no-op).

**Surfaced by:** PrettyUi `Theme.uitkx` → `StatsPanel.style.uitkx` →
`StatsPanel.uitkx` cascade (2026-05-17). Diagnosed in chat after the
0.5.19 release.

---

## 21. LSP element index is one-to-one — folder copy+rename evicts the original

**Status:** Open — **Priority: CRITICAL** (next-up).

**Bug.** `WorkspaceIndex._elementInfo` is a `Dictionary<string, ElementInfo>`
keyed by element name with a single `FilePath` per entry. When two
`.uitkx` files declare the same `component <Name>` (e.g. a folder copy
during refactoring) the second indexed file silently overwrites the
first's entry. A subsequent rename of the copy then evicts the shared
name entirely, breaking IntelliSense for every consumer of the
original — until the user restarts the window.

**Repro (PrettyUi, 2026-05-17).**
1. Copy `Assets/UI/Pages/GamePage/components/PlayerHud/components/StatsPanel/components/Ammunition/`
   to `Ammunition Copy/`. Both folders now contain `Ammunition.uitkx`
   with `component Ammunition { ... }`.
2. The LSP `WatchedFilesHandler.Refresh(copyPath)` indexes the copy →
   `CommitElement("Ammunition", copyPath)` overwrites
   `_elementInfo["Ammunition"].FilePath` to point at the copy.
3. Edit the copy to `component Stats { ... }`, rename folder to `Stats/`.
   `IndexUitkxFile` calls `EvictElementsFromFile(copyPath)`:
   - Iterates `_elementsByFile[copyPath]` = `{ "Ammunition" }`.
   - Checks `_elementInfo["Ammunition"].FilePath == copyPath` → **true**
     (because of step 2).
   - Removes `"Ammunition"` from `_elements` and `_elementInfo` entirely.
4. Re-scan adds `"Stats"`. The original `Ammunition/Ammunition.uitkx`
   is never re-indexed during this flow, so `_elementInfo["Ammunition"]`
   stays missing.
5. `StatsPanel.uitkx`'s analyzer queries `WorkspaceIndex.HasElement("Ammunition")`
   → false → emits `UITKX0105` red squiggle on `<Ammunition>`.
6. `Ctrl+Shift+P → Reload Window` triggers a full rescan; the original
   file is re-discovered; the squiggle clears.

**Compounding factor (VS Code watcher quirk on Windows).**
`workspace/didChangeWatchedFiles` for `**/*.uitkx` does not reliably
emit per-file Delete events for files inside a folder being renamed
(VS Code tracks the rename as one event for the directory). Even if
logic were "re-scan workspace on delete of original," the delete event
we'd want never arrives. Defensive code must not rely on a Delete-of-
the-original signal.

**Fix approach.**

- **A. Multi-valued index (proper fix).** Change `_elementInfo` from
  `Dictionary<string, ElementInfo>` to
  `Dictionary<string, Dictionary<string, ElementInfo>>` (keyed by
  element name, then by file path), or to a `List<ElementInfo>` per
  name. Update sites:
  - `CommitElement` adds the entry keyed by `filePath` (idempotent).
  - `EvictElementsFromFile` removes only the file-specific entry per
    name, never the whole name unless its bucket goes empty.
  - `HasElement(name)` returns true if any entry remains.
  - `GetElementInfo(name)` picks one (first by path, deterministic
    order) or aggregates props from all entries (caller's choice).
  - Optional: expose `GetAllElementInfo(name)` for diagnostics that
    want to surface ambiguity.
  ~50–80 lines, immune to copies, ambiguous-name workflows, and
  partial deletes.
- **B. Mitigation (cheap, recommended as belt-and-braces).** When
  `EvictElementsFromFile` would remove the last entry for a name,
  scan the workspace (or query `_elementsByFile` reverse map) for any
  other `.uitkx` declaring it and re-attach. Hides the symptom even
  with the multi-valued index in place, e.g. if a future change
  introduces a regression.
- **C. UX warning.** Emit a workspace diagnostic (new `UITKX0107` or
  similar) when two files declare the same `component <Name>` in the
  same asmdef. Useful even after A — duplicate component declarations
  are almost always a copy-paste mistake.

**Recommendation: Option A** + a one-line `UITKX0107` warning for the
ambiguity surfaced by A. The duplicate declaration is itself a
user-visible problem; the goal of A is to keep the IDE honest about
which files declare what, not to silently pick one.

**Risk: Low.** All `_elementInfo` lookups are wrapped by
`WorkspaceIndex` accessor methods (no external Dictionary leaks). The
change is purely additive to those accessors; existing call sites get
either a deterministic pick or an aggregate, both backwards
compatible.

**Files:**
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — change
  `_elementInfo` shape, update `CommitElement`,
  `EvictElementsFromFile`, `HasElement`, any `GetElementInfo` /
  `Iterator` paths.
- `ide-extensions~/lsp-server/Tests/` — add regression test:
  1. Index file A declaring `Foo` and file B declaring `Foo`.
  2. Refresh file B (edit removes `Foo`).
  3. Assert `HasElement("Foo")` is still `true` (covered by A).
- Optionally a new diagnostic for the ambiguity (Option C).

**Surfaced by:** PrettyUi folder copy
`StatsPanel/components/Ammunition/` → `Ammunition Copy/` → rename to
`Stats/`, in `StatsPanel.uitkx` consumers (2026-05-17). Workaround:
`Ctrl+Shift+P → Reload Window`. Disrupts the copy-rename refactor
workflow that's natural for adding sibling components.

---

## 22. HMR cannot resolve new sibling `.cs` types or cascading signature changes

**Status:** Open — **Priority: CRITICAL** (next-up).

**Bug class.** HMR compiles each changed `.uitkx` file in isolation
against the *already-loaded* project assembly. While HMR is active it
holds the assembly-reload lock (the whole point — Play-mode state must
survive saves), so Unity cannot recompile the project DLL. This makes
two adjacent edit patterns unrecoverable without stopping HMR:

1. **New sibling `.cs` type referenced by a `.uitkx`.** Author adds
   `Assets/.../GamePage/PlayerStats.cs` declaring a new `record
   PlayerStats(...)` and saves `GamePage.uitkx` referencing it. HMR
   compiles `GamePage.uitkx` against the stale project DLL which
   doesn't contain `PlayerStats` → `CS0246: The type or namespace name
   'PlayerStats' could not be found`. Every subsequent save loops on
   the same error. Stopping HMR + letting Unity recompile is the only
   path forward.
2. **Cascading signature changes across multiple `.uitkx` files.**
   Author refactors a tree: parent passes a new prop, child declares
   it, grandchild forwards it. Each file is saved in turn. HMR
   compiles each one against the stale project DLL which still has the
   *old* shape of all the other components in the tree — so the parent
   fails with `CS0117: 'ChildProps' does not contain a definition for
   'newProp'` even after the child file is saved, because the child's
   project-DLL `ChildProps` class is still the cold one. The new
   child compiled to an HMR DLL, but the parent's compile references
   the project-DLL `Child` type, not the HMR DLL.

**Why this lands as a real bug not a known limitation.** The 0.5.10
"trampoline" architecture made HMR almost-fully-transparent for body
edits. Users now expect *any* edit cycle to be HMR-resolvable. The
two patterns above are both natural — adding a sibling helper type is
trivial in C# normally; refactoring a parent+child pair is a one-minute
task in React. Hitting "CS0246 on a file you just created" or "CS0117
on a prop you just added" reads as broken tooling, not as a documented
edge case. Discord support burden is high because the error text
points at the user's code, not at HMR's design.

**Repro (PrettyUi, 2026-05-17).**
1. HMR active, Play-mode running.
2. Create `Assets/UI/Pages/GamePage/PlayerStats.cs` declaring `record
   PlayerStats(...)`.
3. Save `GamePage.uitkx` referencing `useState(new PlayerStats(...))`.
4. Save 4 child `.uitkx` files (`PlayerHud`, `StatsPanel`, `TopRow`,
   `BottomRow`) each with new typed props.
5. Save 8 leaf `.uitkx` files now passing those props.
6. Observe: every HMR compile of GamePage / PlayerHud / StatsPanel
   fails with mixed `CS0246` (PlayerStats) and `CS0426`
   (`StatsPanelProps` doesn't exist) and `CS0117`
   (`StaminaBarProps does not contain Stamina`) — because the project
   DLL is frozen at the pre-edit shape.
7. Stop HMR → Unity recompiles → all errors clear → restart HMR works.

**Root causes (overlapping).**

- **(a) HMR's compile input is a single `.uitkx` plus the stale
  project DLL.** It cannot see new sibling `.cs` files because they're
  not in the project DLL yet and HMR has no `.cs` discovery pass.
- **(b) Cross-`.uitkx` signature changes need a transitive recompile
  of every consumer.** HMR compiles per-file; it has no dep graph
  walker that, on a saved child, queues every parent that imports it.
  Even if it did, the parent's compile would still bind against the
  project DLL's child type, not the HMR DLL's — the HMR DLL would
  need to be referenced from every dependent compile.
- **(c) The trampoline architecture targets body-only edits.** Adding
  a new prop is a "structural" edit that changes the generated
  `ChildProps` class shape; the trampoline only swaps `__hmr_Render`
  field values, not whole prop-class layouts.

**Fix approaches (sketched, none cheap).**

- **A. New-`.cs`-file pickup.** When HMR compiles a `.uitkx`, also
  enumerate sibling `.cs` files under the same asmdef that aren't
  already in the project DLL (file `Last Write Time` newer than the
  loaded assembly's compile time, OR not present by type-name lookup).
  Include them as additional `SyntaxTree`s in the HMR Roslyn compile.
  ~80–120 lines; reuses `AsmdefResolver` already shipped in 0.5.12.
  Closes case (1).
- **B. Transitive recompile queue.** Build a `.uitkx` dep graph at
  HMR `Start` (parse imports + JSX tag references). On every save,
  enqueue all transitive consumers in topological order. The first
  changed file's HMR DLL becomes a reference for the next file's
  compile; the next file's compile sees the new prop shapes through
  the HMR DLL, not the project DLL. The compile-against-stale-project-DLL
  problem dissolves because each compile in the queue picks up the
  prior compile's HMR DLLs explicitly. ~200–300 lines; requires
  changes to `UitkxHmrCompiler.Compile` to accept extra `MetadataReference`s
  and to `UitkxHmrController.ProcessFileChange` to drive the queue.
  Closes case (2).
- **C. UX fallback — "HMR can't handle this, want to recompile?"**
  When an HMR compile fails with `CS0246` / `CS0117` / `CS0426`
  patterns characteristic of stale-project-DLL bindings, surface a
  one-click "Stop HMR + recompile + restart HMR" action. Already
  doable manually but the user has to know to do it. Cheap (~30 lines)
  but doesn't actually fix the underlying limitation. Belt-and-braces.

**Recommendation: A + B together.** A is a near-trivial parser
extension and closes the most common offender (new helper types).
B is the structural fix that makes refactors work; without it,
every multi-file structural change still requires an HMR stop.
C is worth adding as a UX layer even after A+B, for the residual
cases B can't catch (e.g. edits that change asmdef references).

**Risk.** B is the heavy item. Building a dep graph means parsing
imports + JSX tag references for every `.uitkx` on `Start`; cache
invalidation has to track imports added/removed by every save. The
2026-05-12 `HookContainerRegistry` (item closed in 0.5.12) is a good
template — it already does async background indexing with watcher
invalidation.

**Files (A — new `.cs` pickup):**
- `Editor/HMR/UitkxHmrCompiler.cs` — add `EnumerateNewSiblingCsFiles`
  pass; call from `Compile` before invoking Roslyn.
- `Editor/HMR/AsmdefResolver.cs` — already exposes the asmdef
  boundary; reuse to scope the enumeration.

**Files (B — transitive recompile):**
- `Editor/HMR/UitkxDepGraph.cs` (new) — parse imports + JSX
  references, maintain `{file → consumers}` map.
- `Editor/HMR/UitkxHmrController.cs` — on `ProcessFileChange`, enqueue
  topo-sorted consumers; on `Start`, seed graph; on watcher events,
  invalidate per-file.
- `Editor/HMR/UitkxHmrCompiler.cs` — accept additional
  `MetadataReference[]` for prior HMR DLLs in the queue.

**Files (C — UX fallback):**
- `Editor/HMR/UitkxHmrController.cs` — `ProcessFileChange` failure
  branch checks for `CS0246|CS0117|CS0426` + queues > 1, emits an
  EditorWindow action.

**Surfaced by:** PrettyUi `PlayerHud` numeric extraction refactor
(2026-05-17) — added `PlayerStats.cs` + signature changes to 12
`.uitkx` files in one go. Every HMR compile failed; Stop+Unity
recompile+Start was the only recovery.

---


---

## 99. Dual-arg V.Func editor overload (collapse Family-only resolution path)

**Status:** Not started — captured for future cleanup

**Summary:** Today the editor branch of the SG-emitted `V.Func` call passes
only the `Family` handle; the body delegate is resolved indirectly via
`Family.Current`. This indirection caused two distinct production-blocking
bugs in 0.6.0:

1. `[ModuleInitializer]` on the component class triggered Mono's
   `BeforeFieldInit` divergence (cctor fired on `call` from
   `<Module>::.cctor`), running user `static readonly` asset
   initializers before `UitkxAssetRegistry` was populated → cold-open
   crash. Worked around by emitting a separate `{Component}__UitkxRefresh`
   companion class.

2. Hand-written children (`ReactiveUITK.Router.Route`, `Router`,
   `Outlet`, `Routes` — not `.uitkx`) have no companion → no Register
   call → `Family.Current` returned `PlaceholderRender` and the very
   first playmode render threw. Worked around by emitting a fallback
   factory `GetFamily("X", () => global::X.Render)` from every parent.

**Cleaner long-term shape:** make the editor overload accept BOTH the Family
AND a direct render delegate, e.g.

`csharp
V.Func<P>(Family family, Func<IProps, IReadOnlyList<VNode>, VNode> render,
         P props, string key = null, params VNode[] children)
`

Body resolution becomes `family.Current ?? render` (with HMR-published
`Family.Current` taking precedence after the first recompile). This makes
the editor path almost shape-identical to the player path; eliminates the
fallback-factory machinery in `GetFamily`; and the companion class can be
kept (HMR-only need) or collapsed depending on whether we can prove the
Mono cctor case is unreachable when the Register call lives elsewhere.

**Not done now because:**
- Current shape is shipping (1245/1245 tests green, two clean Unity
  validations: cold-open + playmode).
- Refactor doesn't simplify the HMR DLL recompile path (those still need
  Register to swap `Family.Current`).
- The fragility surface shrinks but does not disappear; companion class
  may still be needed for HMR DLLs.

**Files to touch (estimated):**
- `Shared/Core/V.cs` — add `render` parameter to both editor Family
  overloads; switch `_typedFunctionRender = family?.Current ?? render`.
- `Shared/Core/Refresh/RefreshRuntime.cs` — remove fallback-factory
  `GetFamily` overload + `Family.TrySetFallbackFactory` if unused.
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — drop the `() =>
  {Type}.Render` arg from `GetFamily` emission; add `{Type}.Render`
  as second arg to `V.Func` editor branch.
- `Editor/HMR/HmrCSharpEmitter.cs` — mirror.
- `SourceGenerator~/Tests/EmitterTests.cs` — update fallback-factory
  assertions to the new shape.

**Surfaced by:** PrettyUi 0.6.0 cold-open crash (Asset registry race) +
playmode placeholder exception (Router family unresolved) — 2026-05-21.

---

## 23. HMR `HookSignature` regex misses composed hooks (silent state corruption on add/remove)

**Status:** Not started — **CRITICAL, next**

**Summary:** `s_hookSignatureRe` in
`SourceGenerator~/Emitter/CSharpEmitter.cs` (mirrored in
`Editor/HMR/HmrCSharpEmitter.cs` after the 0.5.23 comment-scrubber fix)
matches a hard-coded allow-list of hook names: `useState`, `useEffect`,
`useReducer`, `useContext`, `useRef`, `useMemo`, `useCallback`,
`useLayoutEffect`, `useSignal`, `useAnimate`, etc. **Any hook the user
calls whose name is not on that list is invisible to the signature.**

PrettyUi's GamePage runtime logs (2026-05-22) showed 11 entries in
`state.FunctionEffects` while `HookSignature` reported only 7 entries —
the gap is `UseNavigate` (RouterHooks), `useUiDocumentSlot`
(PrettyUi.UIHooks), and other higher-level hooks that compose internally
on `useEffect`. The regex was authored before custom-hook composition
became common and never updated.

**Concrete failure mode (latent, not yet user-reported but
reproducible):**

1. Component body contains `var slot = useUiDocumentSlot("PlayerHud");`
   between two `useState` calls.
2. User comments out the `useUiDocumentSlot` line. Saves.
3. SG emits new `HookSignature` — **identical** to the old one (regex
   ignored both before and after). `sigChanged == false`.
4. `PerformRefresh` does NOT call `FullResetComponentState`.
5. The new IL re-walks hooks: `useState` (slot 0) → `useState` (slot 1).
   But the *runtime* still has 3+ slots from before, and the slot
   indices for the internal effects/state from `useUiDocumentSlot` are
   now read as `useState` slots. Type-cast crash inside `Hooks.UseState`,
   or worse — wrong cached value read with no crash.

The current "soft" symptom is a `HookOrderMismatch` warning from
`RecordHook`'s priming check; the hard symptom is undefined behavior
once the user's reset-mismatch state leaks across hook types.

**Why it didn't bite the timer-comment-out bug (0.5.23):** that edit
toggled a plain `useEffect`, which **is** on the regex allow-list, so
the signature did change correctly. The user's pain point (full scene
reset on every hook toggle) is a separate concern — that's React-Refresh's
documented "incompatible signature → fresh mount" semantics
(`HMR_FAST_REFRESH_PLAN.md` §10 PR-D), not a bug. **This item is about
the cases where the regex is silently wrong.**

**Fix shape — do it the React way:**

React Fast Refresh doesn't use a static allow-list. It computes the
signature from the actual hook calls discovered at compile time:

```
// React's signature for a component that calls useState, useEffect,
// useMyCustomHook (which internally uses useReducer):
//   "useState{[]}useEffect{[]}useMyCustomHook{[customHookSignature]}"
// where customHookSignature is recursively computed from the custom hook
```

Translate to UITKX:

1. **SG-time analysis** (`SourceGenerator~/Emitter`): instead of a
   name-list regex on the raw body text, walk the Roslyn syntax tree
   for the component body and:
   - Collect every method-invocation expression whose target type
     resolves to a method annotated `[Hook]` (or whose containing type
     ends in `Hooks` — flag whichever is more reliable in the codebase),
     OR whose name begins with `use`/`Use`.
   - For each such call, emit its fully-qualified-name into the
     signature.
   - For dependency lists, follow react-refresh's convention: emit
     `{}` for an empty deps array, `{*}` for a non-literal array, or the
     literal expression text for inline literals.
2. **Custom-hook transitivity** (already in PR-D plan, `HMR_FAST_REFRESH_PLAN.md`
   L820): SG emits a `customHookFamilies` array alongside `HookSignature`;
   `RefreshRuntime.HaveEqualSignatures` walks transitively so editing a
   shared custom hook remounts every consumer.
3. **HMR parity**: mirror in `Editor/HMR/HmrCSharpEmitter.cs` — same
   syntax-tree walker, same FQN emission. Cold and HMR signatures must
   agree byte-for-byte or every first HMR cycle after a Unity recompile
   misfires.
4. **Migration**: existing components carry the old short signatures.
   First-ever recompile after this lands will produce a different
   signature for every component → one cycle of forced remount on every
   open scene. Document in the changelog.

**Tests required:**

- SG snapshot: component with `useState`, `useEffect`, custom
  `useThing()`, `Hooks.useState(...)` (fully-qualified form) — assert
  all four appear in `HookSignature`.
- SG snapshot: component with `useUiDocumentSlot("x")` — assert the
  call appears in signature.
- SG snapshot: comment out one hook with `//`, recompile — signature
  differs from uncommented version.
- HMR end-to-end: comment out `useUiDocumentSlot` line in a live
  component, observe `sigChanged=true` log, observe
  `FullResetComponentState` runs. (Should be added to the Editor
  test in `HMR_FAST_REFRESH_PLAN.md` §12 validation contract.)

**Surfaced by:** PrettyUi 0.5.23 investigation, GamePage HMR log analysis
— `effects=11 hookStates=15` while `HookSignature` reported 7 entries.
2026-05-22.

**Files to touch (estimated):**
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — replace `ExtractHookSignature`
  regex with Roslyn syntax-tree walker. Keep `ScrubCommentsAndStrings` as a
  helper for the legacy code path until removed.
- `Editor/HMR/HmrCSharpEmitter.cs` — mirror.
- `SourceGenerator~/Emitter/HookEmitter.cs` — passes through to
  `EmitContext.ExtractHookSignature`; verify behavior unchanged.
- `SourceGenerator~/Tests/EmitterTests.cs` + new snapshots under
  `SourceGenerator~/Tests/Snapshots/`.

---
