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

**Status:** Open — MEDIUM priority

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

**Status:** Open — MEDIUM priority

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

**Status:** Open — HIGH priority

**Problem:** Companion `.cs` files (e.g. `MyComponent.style.cs`,
`MyComponent.util.cs`) sitting alongside a `.uitkx` file get **no IDE
features** from the UITKX extension:

- **No IntelliSense** — no completions for UITKX types, StyleKeys, CssHelpers
- **No syntax coloring** — no UITKX-aware highlighting (e.g. StyleKeys usage)
- **No formatting** — no UITKX-aware formatting suggestions
- **No hover** — generic C# hover only, no component-aware context
- **No scaffolding** — no "Create Companion File" command

**Note:** The LSP server internally has infrastructure for companion files
(RoslynHost loads them into the per-file workspace, DefinitionHandler and
ReferencesHandler can resolve across files, TextSyncHandler detects `.cs`
edits). But this infrastructure only benefits the `.uitkx` side — the
companion `.cs` files themselves don't receive any UITKX language features.

**What's needed:**

1. **IntelliSense in companion files** — when editing a `.style.cs`, offer
   completions for `StyleKeys.*`, `CssHelpers.*`, component-specific types.
   May need the LSP to claim `**/*.cs` in specific directories (or files
   matching `*.style.cs` / `*.util.cs` patterns).

2. **Semantic coloring** — highlight StyleKeys constants, CssHelpers values,
   component references in companion files with UITKX colors.

3. **Cross-file navigation** — Go to Definition from companion → `.uitkx`
   (currently works `.uitkx` → companion but not reverse).

4. **Scaffolding command** — "New Companion File" in VS Code command palette
   that creates a `.style.cs` / `.util.cs` with the correct `partial class`
   boilerplate matching the component's namespace and class name.

5. **Documentation** — document the companion file conventions and patterns.

**Challenge:** Claiming `**/*.cs` for the UITKX LSP would conflict with
OmniSharp / C# Dev Kit, which already owns `.cs` files. Need to either:
- Use a separate language ID for companion files (e.g. `uitkx-companion`)
- Provide features through a different mechanism (VS Code extension commands,
  custom views, not LSP)
- Coordinate with C# language server via middleware

**Research findings — recommended approach: Hybrid Middleware + Direct Registration**

Custom language ID (`uitkx-cs`) is **not viable** — VS Code resolves one
language ID per file, so claiming `*.style.cs` would break C# Dev Kit.
Blazer's pattern doesn't apply (`.razor.cs` is handled as regular C# by
C# Dev Kit, Blazor server only handles `.razor`).

The viable approach has 3 tiers:

**Tier 1 — Middleware augmentation (existing pattern):**
The extension already has LSP middleware that intercepts completion responses
(extension.ts lines 379–410 for `@` prefix stripping). Extend this to detect
companion files and inject UITKX-specific items:
```typescript
middleware: {
  provideCompletionItem(document, position, context, token, next) {
    const result = await next(document, position, context, token);
    if (document.fileName.match(/\.(style|util)\.cs$/)) {
      return augmentWithUITKXCompletions(result, document, position);
    }
    return result;
  }
}
```
Same pattern for hover, diagnostics. Zero conflict with C# Dev Kit —
it provides base C# features, middleware adds UITKX-specific extras.

**Tier 2 — Direct VS Code API registration:**
For features that don't need C# compilation context:
```typescript
vscode.languages.registerCompletionItemProvider(
  { scheme: 'file', pattern: '**/*.style.cs' },
  myStyleCompletionProvider
);
```
VS Code supports multiple providers per language — no conflict.
Good for: color previews, StyleKeys constants, CssHelpers shortcuts.

**Tier 3 — Code Actions for complex operations:**
"Generate Props class from markup", "Create style template", etc.
User-initiated (light bulb), no always-on overhead.

**LSP server changes needed:**
- The server already scans `.cs` files and registers for `**/*.cs` changes
- Extend `CompletionHandler` to return UITKX-aware results when the
  request comes from a companion file
- The server already knows the component context from `RoslynHost`

**Effort:** Medium-large for Tier 1 (middleware + LSP handler extension).
Small for Tier 2/3 (VS Code API only). VS2022 needs equivalent middleware.

---
