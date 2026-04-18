# UITKX Tech Debt — IDE Extension & Source Generator

> Status legend: 🔴 Open  🟡 In Progress  🟢 Fixed

---

## Disease Taxonomy

After deep research, the 7 reported issues collapse into **4 structural diseases**,
not 7 independent bugs. Fixes below treat the disease, not the symptoms.

---

## DISEASE 1 — VDG embeds statement blocks inside `return (expr)` → Issues 1 & 4

**Status:** 🔴 Open

### What breaks
- **Issue 1**: `component DeepNestedSection {` — red squiggle under `e`, error
  "The name 'd' does not exist in the current context". No Unity error.
- **Issue 4**: `");` — line 211, "Invalid token ')' in class, record, struct, or
  interface member declaration". No Unity error.

### Root cause
`EmitDirectiveBodyCode` in `VirtualDocumentGenerator.cs` processes directive
body code by emitting C# segments and JSX placeholders in order:

```
seg (includes "return (")  →  (VirtualNode)null!  →  EmitDirectiveJsxExprChecks  →  tail (")\n")
```

`EmitDirectiveJsxExprChecks` calls `EmitNodeExpressionsScoped`, which emits
**statement-level** blocks — `for (int d) { }`, `if (d % 2 == 0) { }`,
`{ object __attr = (expr); }` — between the placeholder and the closing `)`.

The resulting C# is:
```csharp
return (
    (VirtualNode)null!      // placeholder
    for (int d = ...) { }  // ← STATEMENT inside return (expr) — INVALID C#
    if (...) { }           // ← STATEMENT inside return (expr) — INVALID C#
);                          // tail
```

This is syntactically invalid. Roslyn's error recovery desynchronises:
- After encountering the `for { }` statement inside `return (...)`, the parser
  exits the method scope.
- The `);` tail with its `#line N "file.uitkx"` directive then appears at **class
  member level** → CS1519 "Invalid token ')' in class, record, struct, or
  interface member declaration".
- The `for (int d)` block is now outside its original `for` scope so Roslyn
  reports `d` as unknown, mapped via `#line` to wherever the ForNode header is
  (happens to land on line 4, `component DeepNestedSection {`).

`if { }` blocks inside `return (...)` apparently survive because CS1026 / CS1513
are globally suppressed in the virtual doc. `for { }` triggers CS1519, which is
NOT suppressed.

### Disease: statement-vs-expression mismatch in body code emission

Every directive body that contains `return (<JSX>)` produces this broken pattern.
It "works" for `if`/`else` branch (CS1026/1513 suppressed) but fails for `for`
loop (CS1519 not suppressed).

### Cure
In `EmitDirectiveBodyCode`, **collect** all `EmitDirectiveJsxExprChecks` results
and **emit them after the tail**, not inline between placeholder and tail.
Move the expr-checks accumulation to a deferred list (same pattern as
`pendingExprChecks` in `EmitFunctionStyleSetupSegmented`).

```
seg  →  (VirtualNode)null!   (all JSX ranges)
tail (includes "); return (null!); ...")
THEN: EmitDirectiveJsxExprChecks for each range (after all tail text)
```

Variables remain in scope because the checks are still inside the containing
directive's `foreach`/`for`/`if` C# block. The `return (null!);` makes subsequent
checks unreachable code, suppressed by `#pragma warning disable 0162` already in
place.

**Files:** `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`

---

## DISEASE 2 — Local functions in setup code are invisible to validators & tokenizer → Issues 2, 5, 6 (partial), 7

**Status:** 🔴 Open

### What breaks
- **Issue 2**: `<Button>` inside `@foreach` in `VirtualNode PillBar(...)` local
  function — no key, no UITKX0009 error. No Unity error.
- **Issue 5**: `onClick={_ => { Hooks.UseState(0); }}` inside `PillBar`'s
  `@foreach` — no UITKX0016 error. Same pattern in main markup fires correctly.
- **Issue 6 (partial)**: `var s = Hooks.UseState(0);` inside `PillBar`'s
  `@foreach` body — no UITKX0014 error. Same code at level-4 in main markup
  fires correctly.
- **Issue 7**: Colors for `VirtualNode PillBar(...) { ... }` in setup code are
  wrong — directives, JSX tags, attribute names are not semantically highlighted
  at all or have incorrect token types.

### Root cause
Both the Source Generator pipeline and the IDE extension pipeline operate on
**`parseResult.RootNodes`** — the parsed AST nodes from the **markup section**
only (the `return (<JSX>)` part of the component).

```
HooksValidator.Validate(rootNodes, ...)         // SG
StructureValidator.Validate(rootNodes, ...)      // SG
DiagnosticsAnalyzer.WalkNodeList(RootNodes, ...) // IDE
SemanticTokensProvider.GetTokens → CollectNodeTokens(RootNodes, ...) // IDE
```

A local function like `VirtualNode PillBar(string[] opts, string selected) { ... }`
lives in `FunctionSetupCode`. Its content is:
- **Stored** in `DirectiveSet.FunctionSetupCode` (raw C# string).
- **Position-tracked** in `DirectiveSet.SetupCodeMarkupRanges` /
  `SetupCodeBareJsxRanges`.
- **Emitted** by `EmitFunctionStyleSetupSegmented` and `EmitInlineExprChecks`.
- **NEVER PARSED** into `AstNode` objects for walking by validators or tokenizer.

So `PillBar`'s `@foreach` node is invisible to every validator and token emitter.

### Disease: system-wide assumption that "everything worth validating/tokenizing is in RootNodes"

This assumption breaks down as soon as setup code contains local functions with
their own directive bodies.

### Cure
Parse `SetupCodeMarkupRanges` (and `SetupCodeBareJsxRanges`) through
`UitkxParser.Parse` into proper `AstNode` arrays and visit them in:

1. **`HooksValidator`** — walk setup-code nodes with `HookContext.TopLevel`
   (within a local function, the function IS the top level).
2. **`StructureValidator`** — call `WalkForeachForMissingKey` /
   `WalkForeachForIndexKey` on setup-code nodes.
3. **`DiagnosticsAnalyzer`** — `WalkNodeList` the setup-code nodes.
4. **`SemanticTokensProvider`** — `CollectNodeTokens` for each node from
   setup-code ranges (additionally, scan `FunctionSetupCode` for bare directive
   keywords like `@foreach`/`@for`/`@if`/`@(` that may sit outside JSX ranges).

The setup-code nodes should be parsed the SAME WAY as mini-parses in the emitter
— using `UitkxParser.Parse(jsxText, filePath, jsxDirectives, diags, lineOffset: srcLine-1)`.

**Files:**
- `SourceGenerator~/Emitter/HooksValidator.cs`
- `SourceGenerator~/Emitter/StructureValidator.cs`
- `SourceGenerator~/UitkxPipeline.cs`
- `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`
- `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs`

---

## DISEASE 3 — `ScanCodeForHooks` scans the entire BodyCode, including nested bodies → Issue 6

**Status:** 🔴 Open (partially fixed when Disease 2 is fixed)

### What breaks
- **Issue 6 (main part)**: `var s = Hooks.UseState(0);` at line 191 inside the
  level-4 `@foreach` fires **4 identical diagnostics** (once per nesting level),
  all at the same line:
  - UITKX0014 (inside @foreach — level 1)
  - UITKX0014 (inside @foreach — level 2)
  - UITKX0013 (inside @if — level 3)
  - UITKX0014 (inside @foreach — level 4)

### Root cause
Both `HooksValidator.ScanCodeForHooks(forn.BodyCode, ...)` (SG) and
`DiagnosticsAnalyzer.ScanCodeForHooks(fe.BodyCode, ...)` (IDE) scan the **entire
`BodyCode` string** for hook patterns. `BodyCode` is the raw C# between `{` and
`}` of a directive, including **all nested directive bodies verbatim**.

The nesting stack for line 191:
```
Level-1 @foreach body code → contains "Hooks.UseState(0)" deep inside → fires
Level-2 @for body code     → also contains it → fires again
Level-3 @if body code      → also contains it → fires again
Level-4 @foreach body code → IS the direct host → fires (correctly)
```

So the hook fires N times for N nesting levels.

### Disease: BodyCode scan is not aware of nested directive boundaries

### Cure
Limit `ScanCodeForHooks` to only the **"setup preamble"** of BodyCode — the C#
text before the first JSX range. Nested directive bodies are inside JSX ranges;
the hook there will be caught when the tree walk reaches that specific nested node
directly (where it fires exactly once).

Compute the scan limit as:
```
scanEnd = min(all BodyMarkupRanges[i].Start - bodyCodeOffset,
              all BodyBareJsxRanges[i].Start - bodyCodeOffset,
              bodyCode.Length)
```

Modify `ScanCodeForHooks` to accept `int scanEnd = int.MaxValue` and scan only
`code[0..scanEnd]`.

**Files:**
- `SourceGenerator~/Emitter/HooksValidator.cs`
- `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`

---

## DISEASE 4 — `ParseBodyForIde` uses first return, misses JSX when `return null` comes first → Issue 3

**Status:** 🔴 Open

### What breaks
- **Issue 3**: `<Label>` inside level-4 `@foreach` (which starts with
  `if (tag == "z" && d == 0) return null;`) — no key attribute, no UITKX0009
  error. No Unity error.

### Root cause
`ParseBodyForIde(useLastReturn: false)` finds `return null;` as the **first**
depth-0 return in bodies like:
```csharp
if (tag == "z" && d == 0)
    return null;         // ← found first, openParen = -1

var isActive = ...;
return <Label .../>;    // ← the actual JSX return, NEVER reached
```

When `openParen == -1` (null return), `ParseBodyForIde` immediately returns
`ImmutableArray<AstNode>.Empty` for `Body`. The `ForeachNode.Body` is empty.

`WalkForeachForMissingKey(foreachNode.Body, ...)` sees the empty `Body` →
`CheckDirectChildrenForMissingKey` finds no nodes → UITKX0009 not fired.

The same empty `Body` also means `ParseJsxFragments` produces nothing and the
IDE-side `DiagnosticsAnalyzer.WalkNodeList(fe.Body, ...)` sees nothing.

### Disease: First-return-wins strategy breaks multi-return bodies

### Cure
In `UitkxParser.ParseBodyForIde`, when `TryFindTopLevelReturn(useLastReturn:false)`
finds `openParen == -1` (a `return null`), fall through and retry with
`useLastReturn: true` to find the LAST return (which is the actual JSX return):

```csharp
// return null; — try again with last return to find actual JSX
if (openParen == -1)
{
    if (ReturnFinder.TryFindTopLevelReturn(
            bodyCode, 0, bodyCode.Length,
            out _, out int lp, out int lc, out _,
            useLastReturn: true)
        && lp >= 0 && lc > lp)
    {
        // parse the last return's JSX content
        ...
    }
    return ImmutableArray<AstNode>.Empty;
}
```

**Files:** `ide-extensions~/language-lib/Parser/UitkxParser.cs`

---

## Implementation Order

| Priority | Disease | Issues | Effort |
|----------|---------|--------|--------|
| 1 (done?) | 3 | 3 | Small — one method in UitkxParser.cs |
| 2 | 3 (hook scan) | 6 | Small — two files, one helper + param |
| 3 | 1 (VDG) | 1, 4 | Medium — restructure EmitDirectiveBodyCode |
| 4 | 2 (local funcs) | 2, 5, 6-partial, 7 | Large — multi-file pipeline change |
