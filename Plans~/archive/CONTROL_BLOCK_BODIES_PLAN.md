# Control Block Bodies: `return (...)` in `@for` / `@if` / `@while` / `@foreach` / `@switch`

## Problem

Control block bodies only support markup + expressions. Users cannot write C#
statements inside them. This causes two real bugs:

1. **Closure capture** — lambdas in `@for` close over the single loop variable,
   not a per-iteration copy. All handlers fire with the loop's exit value.
2. **No local computation** — cannot declare variables, call methods, or build
   intermediate values inside control block bodies.

## Proposed Solution

Reuse the component body model: **everything before `return (...)` is C#,
everything inside `return (...)` is markup.**

```uitkx
@for (int row = 0; row < 3; row++) {
  var r = row;
  var label = grid[r, 0];
  PointerEventHandler click = _ => handleColumnClick(r);

  return (
    <VisualElement onClick={click}>
      <Label text={label} />
    </VisualElement>
  );
}
```

### Why This Works

- `var r = row;` is a **per-iteration local** — the C# `for` block creates a
  new scope each iteration, so `r` captures correctly
- The parser already knows how to split C# from markup via `TryFindTopLevelReturn`
- Users already understand this from the component-level `return (...)`
- **Single-root enforced** — every control block must return exactly one node

### Single-Root Enforcement

`return (...)` enforces a single root element (like React). Multiple
siblings require an explicit `<Fragment>`:

```uitkx
@if (showBoth) {
  return (
    <Fragment>
      <Label text="A" />
      <Label text="B" />
    </Fragment>
  );
}
```

This is a **breaking change** — all existing control block bodies must be
updated to use `return (...)`. A migration codemod is provided (Phase 0).

---

## Architecture: What Exists Today

### Component-Level Split (DirectiveParser)

```
component Foo {
  var x = 42;              ← FunctionSetupCode (raw C# string)
  var style = new Style{};

  return (                 ← TryFindTopLevelReturn detects this
    <Label text={x} />    ← UitkxParser parses this as markup
  );
}
```

`TryFindTopLevelReturn` scans for `return (` at brace/paren/bracket depth 0,
skipping strings and comments. Returns positions of the keyword, open paren,
close paren, and statement end. Proven, battle-tested algorithm.

**Result:** `DirectiveSet.FunctionSetupCode` stores raw C# text.
`ParseResult.RootNodes` stores the parsed markup.

### Control Block Bodies (UitkxParser)

```
@for (int i = 0; i < 3; i++) {
  <Label text={i} />      ← ParseContent() handles this
  var x = i;               ← Becomes TextNode("var x = i;") — BUG
}
```

`ParseFor()` calls `ParseContent(stopAtBrace: true)` which dispatches by
character: `<` → element, `@` → directive, `{` → expression, else → text.
C# statements fall through to the text case.

### Emitters

| Feature | CSharpEmitter (SG) | HmrCSharpEmitter (HMR) |
|---------|-------------------|----------------------|
| ForNode body | Dual mode: expression or statement (if `@break`/`@continue`) | Always expression (IIFE) |
| Setup code | Emits setup code before return | Not applicable |
| IfNode | Ternary chain: `cond ? Fragment(...) : ...` | IIFE with return |
| ForeachNode | LINQ `.Select()` | IIFE with `.Select()` |
| SwitchNode | Switch expression | IIFE with switch statement |

### VirtualDocumentGenerator (LSP)

Emits control blocks as proper C# `for`/`if`/`while` blocks with scoped
variables. Body nodes become expression-type-check statements inside the block.
Setup code for components is emitted via `EmitFunctionStyleSetupSegmented`.

### Grammar (TextMate)

`code-block-body` pattern already includes `#expression-content` which tokenizes
C# keywords, operators, identifiers. Control block bodies use this pattern.
C# statements would get basic highlighting automatically.

### Completions (LSP)

Context detection routes to Roslyn for C# regions. Currently determines C#
context via source-map (`offsetIsInCSharpRegion`) or `CursorKind`. New C#
regions in control block setup code need source-map entries.

### Formatter

Formats control block bodies via `FormatNodeList(body)`. Only handles AST nodes
(elements, expressions, nested control flows). Raw C# code would need verbatim
pass-through.

---

## Implementation Plan

### Phase 1: Parser — Split Control Block Bodies at `return (...)`

**Files:** `UitkxParser.cs`, `AstNode.cs`

#### 1a. AST Changes

Add `SetupCode` fields to control block nodes:

```csharp
public sealed record ForNode(
    string ForExpression,
    ImmutableArray<AstNode> Body,
    int SourceLine, string SourceFile
) : AstNode(SourceLine, SourceFile)
{
    // NEW: Raw C# code before return(), null if body is pure markup
    public string? SetupCode { get; init; }
    public int SetupCodeOffset { get; init; }   // absolute offset in source
    public int SetupCodeLine { get; init; }      // 1-based line number
}
```

Same for `WhileNode`, `ForeachNode`, `SwitchCase`, and `IfBranch`.

#### 1b. Parser Changes

In each `ParseXxx()` method (ParseFor, ParseIf, ParseWhile, ParseForeach,
ParseSwitch), after consuming the opening `{`:

1. **Save scanner position** (`bodyStart`)
2. **Call `TryFindTopLevelReturn`** on the text from `bodyStart` to the matching `}`
3. **If found:**
   - Extract `setupCode = source[bodyStart..returnStart]` (may be empty)
   - Advance scanner to inside `return (...)` parens
   - Call `ParseContent(stopAtBrace: false)` on just the markup inside parens
   - Advance scanner past `);` and `}`
   - Store `SetupCode` on the node (null if empty)
4. **If not found:**
   - Emit parse error: "Control block body must contain `return (...)`"
   - Attempt error recovery: parse body as markup to avoid cascading errors

**Key reuse:** `TryFindTopLevelReturn` already exists in DirectiveParser. Extract
it to a shared utility method (or keep in DirectiveParser and call from UitkxParser).

**Nesting:** Recursive by nature — a `@for` inside a `return (...)` inside
another `@for` works because `ParseContent` calls `ParseFor` which calls
`TryFindTopLevelReturn` which calls `ParseContent` again.

#### 1c. Validation

- `return (...)` is **mandatory** — missing `return` is a parse error
- `return (...)` must be the **last statement** (nothing after `;` except
  whitespace before `}`)
- Multiple root elements inside `return (...)` without `<Fragment>` is an error

### Phase 2: Source Generator Emitter

**Files:** `CSharpEmitter.cs`

**All** control blocks now use statement mode (setup code + single expression).
The old expression-mode and dual-mode branching (`ContainsLoopFlow`) can be
removed entirely.

```csharp
// Emit: for (expr) {
_sb.Append($"for ({fn.ForExpression}) {{ ");

// Emit setup code verbatim with hook aliases applied
if (fn.SetupCode != null)
{
    var code = ApplyHookAliases(fn.SetupCode);
    code = ResolveAssetPaths(code);
    _sb.Append(code);
}

// Emit body markup as single expression
_sb.Append("__r.Add(");
EmitBodyExpr(fn.Body);  // Body always has exactly 1 root node
_sb.Append("); }");
```

**Simplification:** Remove `ContainsLoopFlow()` check and the old expression-mode
path. All control blocks use statement mode unconditionally.

**`#line` directives:** Emit `#line N` before setup code and before the markup
expression to preserve source-map accuracy.

### Phase 3: HMR Emitter

**Files:** `HmrCSharpEmitter.cs`

Same changes as Phase 2. This is critical — HMR currently can't handle
statements in control blocks at all, causing `CS0103` errors.

The HMR emitter uses IIFE patterns. With setup code:

```csharp
// Before: for (expr) { __items.Add(EmitBodyAsFragment(body)); }
// After:
for (expr) {
    setupCode;
    __items.Add(EmitBodyAsFragment(body));
}
```

The IIFE wrapper may need adjustment to accommodate statement-mode blocks.

### Phase 4: VirtualDocumentGenerator (LSP Roslyn)

**Files:** `VirtualDocumentGenerator.cs`

In `EmitNodeExpressionScoped()` for each control block type, emit setup code
inside the block scope before processing body nodes:

```csharp
case ForNode fo:
    b.Scaffold($"{indent}for ({fo.ForExpression}) {{\n");
    if (fo.SetupCode != null)
    {
        // Emit as Mapped region for Roslyn analysis + diagnostics
        b.Mapped(
            fo.SetupCode,
            fo.SetupCodeOffset,
            SourceRegionKind.CodeBlock,  // or new: ControlBlockSetup
            fo.SetupCodeLine
        );
        b.Scaffold("\n");
    }
    EmitNodeExpressionsScoped(fo.Body, ...);
    b.Scaffold($"{indent}}}\n");
    break;
```

**Source map:** Setup code must be a `Mapped` region so that:
- Roslyn provides completions (variables, methods, types)
- Diagnostics (unused variables, type errors) map back to `.uitkx` lines
- Go-to-definition works for symbols defined in setup code

### Phase 5: Formatter

**Files:** `AstFormatter.cs`

All control blocks now always emit `return (...)`. No branching needed:

```csharp
private void FormatFor(ForNode node)
{
    Ln($"@for ({node.ForExpression}) {{");
    _indent++;
    if (node.SetupCode != null)
    {
        EmitIndentedRawCode(node.SetupCode);
        Ln("");  // blank line between setup and return
    }
    Ln("return (");
    _indent++;
    FormatNodeList(node.Body, topLevel: false);
    _indent--;
    Ln(");");
    _indent--;
    Ln("}");
}
```

**`EmitIndentedRawCode`:** New helper that splits raw C# on newlines and emits
each line with the current indent level. Does not reformat the C# — preserves
user formatting (same as component setup code).

### Phase 6: Grammar / Colorizer

**Files:** `uitkx.tmLanguage.json`

Minimal changes needed. The `code-block-body` scope already includes
`#expression-content` which tokenizes C# keywords and operators. Since control
block bodies already use this scope, C# statements will get basic highlighting
(keywords like `var`, `int`, `new`, operators, string literals, etc.).

**Optional enhancement:** Add a `return` keyword match inside control block
bodies to highlight it with `keyword.control.flow.uitkx` scope, matching how
the component-level `return` is highlighted.

### Phase 7: Completions

**Files:** `CompletionHandler.cs`

Setup code regions will be emitted as `Mapped` regions in the virtual document
(Phase 4), so Roslyn completions will work automatically via the existing
`offsetIsInCSharpRegion` check.

**One adjustment needed:** `IsLikelyEmbeddedMarkupAtOffset` currently backtracks
~25 lines to determine context. Lines containing `var x = i;` inside a control
block body should NOT be detected as embedded markup. This should work naturally
since they don't start with `<` or `@if`/`@for`/etc.

### Phase 8: Diagnostics

**Files:** `DiagnosticsAnalyzer.cs`

#### New diagnostics:
- **UITKX0XXX**: "Missing `return (...)` in control block body" — parse error
  when no `return (...)` is found at depth 0
- **UITKX0XXX**: "Code after `return (...)` in control block body" — if there
  are non-whitespace characters between `);` and `}` in a control block body
- **UITKX0XXX**: "Multiple root elements in `return (...)`" — if the markup
  inside `return` has multiple root elements (suggest `<Fragment>`)

#### Existing diagnostics:
- **UITKX0009** (key in loops): Still applies to the single element inside
  `return (...)` — no change needed, the Body array still holds the parsed
  markup nodes

### Phase 9: Tests

#### Source Generator Tests (EmitterTests.cs)
- `@for` with setup code + return → correct C# output
- `@if`/`@else` each with setup code → correct ternary/statement output
- `@foreach` with setup code → correct LINQ output
- Nested: `@for` with return containing `@if` with return
- Missing return → parse error
- Setup code with hook aliases (`useState`) → aliases applied
- Setup code with asset paths → resolved
- Closure capture: `var r = row;` in setup → `r` is per-iteration local

#### LSP Tests
- Virtual document emits setup code in correct scope
- Roslyn completions work inside setup code region
- Source map accuracy: diagnostic in setup code maps to correct .uitkx line
- Go-to-definition for variable declared in setup code

#### Formatter Tests
- `@for` with setup code → proper indentation
- `@if`/`@else` with setup code → else-if chaining preserved
- Nested control blocks with setup code → correct nesting

#### HMR Tests (if applicable)
- `@for` with setup code compiles via HMR
- No `CS0103` errors for variables in setup code

---

## Rollout

1. **Parser + AST** — `return (...)` required, parse error if missing
2. **Remove `@code {}`** — delete CodeBlockNode, ReturnMarkupNode, and all handling
3. **SG Emitter + tests** — simplified, statement mode only
4. **HMR Emitter** — same simplification
5. **VDG + completions** — LSP provides full IntelliSense in setup code
6. **Formatter** — always emits `return (...)`
7. **Grammar** — `return` keyword highlighting, remove `@code` rules
8. **Diagnostics** — error reporting for misuse
9. **Documentation** — update Styling/UITKX docs pages

Each phase is independently testable and shippable.

---

## Edge Cases

### Nested Control Blocks with Setup Code

```uitkx
@for (int row = 0; row < 3; row++) {
  var styles = row == 0 ? headerStyle : rowStyle;

  return (
    <VisualElement style={styles}>
      @for (int col = 0; col < 3; col++) {
        var c = col;
        var cell = grid[row, c];

        return (
          <Label text={cell} key={$"cell-{row}-{c}"} />
        );
      }
    </VisualElement>
  );
}
```

Works naturally: outer `TryFindTopLevelReturn` finds the outer `return (` at
depth 0. Inner `@for` is parsed as markup by `ParseContent`. When the inner
`@for` body is parsed, `TryFindTopLevelReturn` runs again and finds the inner
`return (` at depth 0 within that block.

### @if / @else with Setup Code

```uitkx
@if (isEditing) {
  var handler = (PointerEventHandler)(_ => save());

  return (
    <Button text="Save" onClick={handler} />
  );
} @else {
  return (
    <Label text="Read only" />
  );
}
```

Each branch is parsed independently by `ParseIf()`. Every branch must have its
own `return (...)`.

### @switch with Setup Code

```uitkx
@switch (mode) {
  @case "edit":
    var handler = buildHandler();
    return (<Button onClick={handler} />);
  @case "view":
    return (<Label text="View" />);
  @default:
    return (<Label text="Unknown" />);
}
```

Each `SwitchCase` body is parsed independently. Same split logic applies.

### @foreach (No Closure Issue)

```uitkx
@foreach (var item in items) {
  var processed = transform(item);

  return (
    <Label text={processed.Name} key={item.Id} />
  );
}
```

`@foreach` already creates per-iteration scope in C#, so closures work without
setup code. But setup code is still useful for pre-computation.

### @code Blocks — Removed

`@code { }` is unused in any `.uitkx` file (confirmed by codebase search).
The function-style component body (setup code before `return`) already provides
full C# support at the component level. With control block bodies now also
supporting C# via setup regions, `@code { }` has no remaining use case.

**Remove entirely:**
- `UitkxParser.ParseCodeBlock()` — remove method
- `ParseContent()` — remove the `case "code":` branch
- `CodeBlockNode` / `ReturnMarkupNode` — remove AST types
- `CSharpEmitter.EmitCodeBlockContent()` — remove method
- `HmrCSharpEmitter` — remove `@code` handling
- `VirtualDocumentGenerator` — remove `CodeBlockNode` emission
- `AstFormatter` — remove `@code` formatting
- `DiagnosticsAnalyzer` — remove `CodeBlockNode` walking
- `uitkx.tmLanguage.json` — remove `@code` grammar rules
- `CompletionHandler` — remove `@code` from control flow completions
- `uitkx-schema.json` — remove `@code` from schema if present

**Files affected:** ~10 files, net reduction in code.

### Empty Return

```uitkx
@if (hidden) {
  return ();
}
```

Should produce `null` (no element rendered). Same as an empty body today.

---

## Summary

| Aspect | Impact |
|--------|--------|
| Migration | Update existing samples + test snapshots manually |
| `@code` removal | Delete ~10 files worth of dead code (CodeBlockNode, ReturnMarkupNode, etc.) |
| Parser | Reuse `TryFindTopLevelReturn` — ~60 lines (no fallback path) |
| AST | Add ~5 fields across existing node types |
| SG Emitter | ~30 lines per control block type, remove dual-mode branching |
| HMR Emitter | ~30 lines per control block type |
| VDG | ~15 lines per control block type |
| Formatter | ~30 lines (always emit `return`, no branching) |
| Grammar | ~5 lines (return keyword scope) |
| Completions | ~0 lines (works via source map automatically) |
| Diagnostics | ~30 lines (3 new diagnostics) |
| Tests | ~20 new test cases, update existing snapshots |
| **Total** | ~250-350 lines of production code (simpler than dual-mode) |
| **Breaking** | Yes — all control block bodies require `return (...)` |

---

## Remaining Work (Post-Implementation Audit)

All original phases 1–9 are implemented and green (SG 833/833, LSP 59/59). The following items were identified during a comprehensive audit and require attention before the feature is release-ready.

### Item 1 — HMR `EmitIf`: Missing SetupCode Emission (Bug)

**Severity:** Must-fix  
**File:** `Editor/HMR/HmrCSharpEmitter.cs` — `EmitIf()` (~line 674)

The HMR `EmitIf` method never reads `SetupCode` from `IfBranch` nodes. The SG emitter correctly emits setup code before each branch's `return`, but the HMR emitter skips it entirely.

**Fix:** After the `if`/`else if`/`else` brace opening and before `_sb.Append("return ")`, add:
```csharp
string setupCode = GP<string>(branch, "SetupCode");
if (setupCode != null)
{
    setupCode = ApplyHookAliases(setupCode);
    _sb.Append(setupCode);
    _sb.Append(" ");
}
```

### Item 2 — Remove `@code` / `CodeBlockNode` / `ReturnMarkupNode` (Dead Code)

**Severity:** Cleanup (required before release)  
**Scope:** ~15 files, ~500+ lines to delete

The old `@code { }` block mechanism is fully superseded by inline setup code in control block bodies. All related infrastructure is dead code:

| Construct | Files |
|-----------|-------|
| `CodeBlockNode` record | `AstNode.cs` |
| `ReturnMarkupNode` record | `AstNode.cs` |
| `ParseCodeBlock()` method | `UitkxParser.cs` |
| `ScanForReturnMarkup()` method | `UitkxParser.cs` (~290 lines) |
| `case "code":` directive handling | `UitkxParser.cs` |
| `EmitCodeBlockContent()` | `CSharpEmitter.cs` |
| `EmitCodeBlock()` + collection | `HmrCSharpEmitter.cs` |
| `CodeBlockNode` cases | `AstFormatter.cs`, `VirtualDocumentGenerator.cs`, `DiagnosticsAnalyzer.cs`, `SemanticTokensProvider.cs`, `AstCursorContext.cs`, `HooksValidator.cs`, `StructureValidator.cs`, `CanonicalLowering.cs`, `DiagnosticsPublisher.cs` |
| `@code` snippet | `CompletionHandler.cs` |
| `code-block` grammar rules | `uitkx.tmLanguage.json` |
| `@code` schema entries | `uitkx-schema.json` |
| Related tests | `ParserTests.cs`, `LoweringTests.cs`, `EmitterTests.cs` |

### Item 3 — Remove `@break` / `@continue` / `BreakNode` / `ContinueNode` (Dead Code)

**Severity:** Cleanup (required before release)  
**Scope:** ~10 files, ~200 lines to delete

Users now write plain C# `break;` / `continue;` inside setup code before `return()`. The dedicated `@break`/`@continue` directives and AST nodes are dead code:

| Construct | Files |
|-----------|-------|
| `BreakNode` record | `AstNode.cs` |
| `ContinueNode` record | `AstNode.cs` |
| `@break`/`@continue` parsing + `loopDepth` guard | `UitkxParser.cs` (~50 lines) |
| `case BreakNode:` / `case ContinueNode:` | `CSharpEmitter.cs`, `AstFormatter.cs`, `SemanticTokensProvider.cs`, `DiagnosticsAnalyzer.cs` |

### Item 4 — Remove `ContainsLoopFlow` + Dual-Mode Emitter Paths (Dead Code)

**Severity:** Cleanup (required before release)  
**Scope:** `CSharpEmitter.cs`, ~120 lines to delete

With `@break`/`@continue` gone, the dual-mode branching in `EmitForNode` and `EmitWhileNode` is dead. These methods currently check `ContainsLoopFlow()` to decide between expression-bodied emission and statement-bodied emission with `EmitLoopBodyStatements`. Only the expression-bodied path survives.

| Construct | Location |
|-----------|----------|
| `ContainsLoopFlow()` overloads | `CSharpEmitter.cs` (~37 lines) |
| `EmitLoopBodyStatements()` | `CSharpEmitter.cs` (~30 lines) |
| `EmitLoopBodyStatement()` | `CSharpEmitter.cs` |
| `EmitLoopIfStatement()` | `CSharpEmitter.cs` |
| `if (ContainsLoopFlow(...))` checks | `EmitForNode`, `EmitWhileNode` |

### Item 5 — Remove `loopDepth` Parameter Plumbing (Dead Code)

**Severity:** Cleanup (required before release)  
**Scope:** `UitkxParser.cs`, ~39 usages

The `loopDepth` parameter threaded through `ParseContent`, `ParseControlBlockBody`, and all `Parse*` methods is only used to validate `@break`/`@continue` placement. With those directives removed, `loopDepth` becomes dead plumbing.

**Fix:** Remove the `int loopDepth` parameter from `ParseContent()`, `ParseControlBlockBody()`, and all calling sites (~11 method signatures, ~39 call sites).

### Item 6 — HMR `EmitForeach`: Iterator Variable Bug (Pre-existing)

**Severity:** Bug (pre-existing, not a regression)  
**File:** `Editor/HMR/HmrCSharpEmitter.cs` — `EmitForeach()` (~line 720)

The method reads `IteratorDeclaration` (e.g., `"var item"`) and passes the full string into `.Select(var item => ...)`, which is invalid C# — the lambda parameter must be just the variable name.

**Fix:** Extract the variable name from the declaration:
```csharp
string iterVar = iterDecl.Split(' ')[^1]; // "var item" → "item"
```
Then use `iterVar` in the `.Select()` lambda instead of `iterDecl`.

### Item 7 — HMR: `ApplyHookAliases` Missing on SetupCode (Bug)

**Severity:** Bug  
**File:** `Editor/HMR/HmrCSharpEmitter.cs`

Four methods emit SetupCode without calling `ApplyHookAliases()` first. Hook aliases (e.g., `useState(...)` → `__useState(...)`) in setup code will fail at runtime.

| Method | Affected |
|--------|----------|
| `EmitForeach` | ✅ Needs fix |
| `EmitFor` | ✅ Needs fix |
| `EmitWhile` | ✅ Needs fix |
| `EmitSwitch` | ✅ Needs fix |
| `EmitIf` | Fixed by Item 1 above |

**Fix:** In each method, before appending `setupCode`, add `setupCode = ApplyHookAliases(setupCode);`.

### Item 8 (Optional) — Switch Fallthrough ("Multiple Cases, Same Body")

**Severity:** Enhancement (low priority)  
**Scope:** `CSharpEmitter.cs`, `HmrCSharpEmitter.cs`

Currently each `@case` generates its own `return V.Fragment(...)`, so true C# fallthrough is impossible. However, the common "multiple cases → same body" pattern could be supported by grouping consecutive cases with identical body ASTs and emitting stacked `case` labels:

```csharp
// Before: case "a": return Fragment(...); case "b": return Fragment(...);
// After:  case "a": case "b": return Fragment(...);
```

Constraints: Only merge cases with no SetupCode. Low priority enhancement — can be deferred.

### Item 9 — HooksValidator: Scan SetupCode for Hook Calls

**Severity:** Bug (runtime hook-ordering violation not caught at compile time)  
**Scope:** `HooksValidator.cs`

The HooksValidator only walks AST nodes (the markup `Body` of control blocks). It does not scan `SetupCode` strings. A user could write `var x = UseState(0);` inside a `@for` setup region — this compiles fine but violates hook ordering rules at runtime (hooks must not be called inside loops/conditionals).

**Current state:** Before the control block bodies feature, control blocks had no setup code, so this gap didn't exist. Now that every control block can have arbitrary C# in `SetupCode`, the validator must scan those strings for hook call patterns.

**Fix:** In `HooksValidator`, for each control block node type (`ForNode`, `WhileNode`, `ForeachNode`, `IfNode` branches, `SwitchCase`), scan the `SetupCode` string (when non-null) for the same hook patterns used in the AST walker (the 30 patterns: `Hooks.*`, bare names, camelCase aliases). If a match is found, emit the appropriate `UITKX0015` / `UITKX0016` diagnostic with the correct `HookContext` (Loop, Conditional, Switch).

**Complexity:** Medium — requires string-scanning logic similar to `StructureValidator.ScanUseEffectMissingDeps`, applied to each control block's SetupCode.

### Item 10 — StructureValidator: Scan SetupCode for UseEffect Missing Deps

**Severity:** Enhancement (consistency)  
**Scope:** `StructureValidator.cs`

The `ScanUseEffectMissingDeps` check currently only runs on `DirectiveSet.FunctionSetupCode` (component-level setup). If a user writes `UseEffect(() => { ... })` without a dependency array inside a control block's `SetupCode`, the validator won't flag it.

**Current state:** `UseEffect` inside a control block is already a hooks-in-conditional/loop violation (Item 9 would catch it). However, if Item 9 is not yet implemented, the missing-deps check provides a secondary safety net.

**Fix:** After the component-level scan, iterate over all control block nodes in the AST and call `ScanUseEffectMissingDeps` on each non-null `SetupCode` string.

**Complexity:** Low — reuse the existing `ScanUseEffectMissingDeps` method, just call it for each control block's SetupCode. Depends on Item 9 for full correctness (if hooks in control blocks are outright banned, this check becomes redundant for those regions).
