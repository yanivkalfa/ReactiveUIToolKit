# Directive Body as Function — Implementation Plan

## Goal

Treat `@if/@foreach/@for/@while/@switch` bodies identically to function-style
component bodies: **a C# function body that can `return (<JSX/>)` or
`return null;` at any depth** (`if/else`, `switch`, nested blocks, etc.).

Rules unchanged:
- Hooks are forbidden inside directives (UITKX0013–0015 still enforced)
- Iterators (`@foreach`/`@for`) require `key` on returned JSX elements
- Single-root-element rule still enforced per branch / iteration
- `@(variableName)` works inside returned JSX, exactly like function-style

```
@foreach (var item in items) {
    if (item.IsSpecial) {
        var label = item.Name.ToUpper();
        return (
            <Box key={item.Id}>
                <Label text={label} />
                <Label text={@(label)} />
            </Box>
        );
    } else {
        return null;
    }
}
```

---

## Current Architecture (What We Have)

**Parser** (`UitkxParser.ParseControlBlockBody`, line 130):
- Calls `ReturnFinder.TryFindTopLevelReturn()` with `useLastReturn: false`
- Only finds `return(...)` at `braceDepth==0 && parenDepth==0 && bracketDepth==0`
- Splits body into `SetupCode` (string, C# before return) + `Body` (ImmutableArray<AstNode>, parsed JSX inside return parens)
- `return null;` not matched (ReturnFinder checks for `(` then `<` after return)
- Returns at depth > 0 invisible → UITKX0024 error

**AST nodes** (`AstNode.cs`, line 137+):
- `IfBranch(Condition, Body, SourceLine)` + `SetupCode/SetupCodeOffset/SetupCodeLine`
- `ForeachNode(IteratorDecl, CollectionExpr, Body, ...)` + `SetupCode/...`
- `ForNode(ForExpression, Body, ...)` + `SetupCode/...`
- `WhileNode(Condition, Body, ...)` + `SetupCode/...`
- `SwitchCase(ValueExpression, Body, SourceLine)` + `SetupCode/...`

**SG Emitter** (`CSharpEmitter.cs`, line 1098+):
- Two paths per directive: "with setup" (IIFE) and "without" (expression-mode)
- `SetupCode` processed: `ApplyHookAliases()` → `ResolveAssetPaths()` → append
- `Body` emitted via `EmitBodyAsFragment(body)` / `EmitBodyExpr(body)` → `EmitNode()`
- NO `SpliceSetupCodeMarkup()` — JSX inside setup code not supported

**VDG** (`VirtualDocumentGenerator.EmitNodeExpressionsScoped`, line 960+):
- Emits `SetupCode` as one `b.Mapped()` call (simple, no JSX replacement)
- Recursively emits `Body` nodes via `EmitNodeExpressionsScoped(body)`

**HMR Emitter** (`HmrCSharpEmitter.cs`, line 772+):
- Same split as SG: `SetupCode` + `Body`
- No `SpliceSetupCodeMarkup()` for directives
- No `#line` directives for directive body setup code

**Formatter** (`AstFormatter.cs`, line 1618+):
- Directive setup code: `EmitSetupCodeLines()` (line 1762) — verbatim line-by-line, no normalization
- Then emits `return (` + `FormatNodeList(body)` + `);` separately

---

## Target Architecture (What We Need)

### Core Principle

Stop splitting directive bodies into `SetupCode` + `Body`. Instead, treat the
**entire body** as a raw C# string — exactly like `DirectiveSet.FunctionSetupCode`
for function-style components. The body string contains `return (<JSX/>);` and
`return null;` at arbitrary depths.

The emitters (SG, HMR) wrap the entire body in an IIFE and emit it as C#. JSX
spans inside the body are spliced using `SpliceSetupCodeMarkup()` — the same
function already used for component setup code.

---

## Staged Plan

> **Status legend**: ✅ = Complete, 🔄 = In progress, ⬜ = Not started

### Stage 1: Parser + AST (Foundation) ✅

**Files**: `AstNode.cs`, `UitkxParser.cs`, `ReturnFinder.cs`, `ParseResult.cs`

#### 1A. Update ReturnFinder — `return null;` support

File: `ide-extensions~/language-lib/Parser/ReturnFinder.cs`, line 56–116

After the `return <Tag` bare-JSX check (line 95), add:
```csharp
// return null;
if (TryReadKeywordAt(source, j, "null"))
{
    int k = j + 4;
    SkipWhitespace(source, ref k);
    if (k < endExclusive && source[k] == ';')
    {
        returnStart = candidateStart;
        openParen = -1;           // sentinel: null return
        closeParen = -1;
        stmtEndExclusive = k + 1;
        if (!useLastReturn) return true;
        i = stmtEndExclusive;
        continue;
    }
}
```

#### 1B. New AST data shape — `BodyCode` replaces `SetupCode` + `Body`

File: `ide-extensions~/language-lib/Nodes/AstNode.cs`

Replace the existing `SetupCode` + `Body` split with a single `BodyCode` string
on each directive node. Remove `SetupCode`, `SetupCodeOffset`, `SetupCodeLine`,
and change `Body` from `ImmutableArray<AstNode>` to unused/empty.

New properties:

```csharp
// On IfBranch, ForeachNode, ForNode, WhileNode, SwitchCase:

/// <summary>
/// Complete body code (C# with return statements at any depth).
/// When non-null, replaces SetupCode + Body for full-function-body mode.
/// </summary>
public string? BodyCode { get; init; }

/// <summary>Absolute char offset in source where BodyCode begins.</summary>
public int BodyCodeOffset { get; init; }

/// <summary>1-based line number where BodyCode begins.</summary>
public int BodyCodeLine { get; init; }

/// <summary>
/// Paren-wrapped JSX ranges within BodyCode.
/// Same format as DirectiveSet.SetupCodeMarkupRanges.
/// </summary>
public ImmutableArray<(int Start, int End, int Line)> BodyMarkupRanges { get; init; }

/// <summary>
/// Bare JSX ranges (return <Tag/>, ? <Tag/>, etc.) within BodyCode.
/// Same format as DirectiveSet.SetupCodeBareJsxRanges.
/// </summary>
public ImmutableArray<(int Start, int End, int Line)> BodyBareJsxRanges { get; init; }
```

#### 1C. Rework `ParseControlBlockBody`

File: `ide-extensions~/language-lib/Parser/UitkxParser.cs`, line 130–215

New behavior:
1. `FindMatchingBrace` → get `closeBrace` (same as before)
2. Extract entire body as raw string: `bodyCode = _source[bodyStart..closeBrace]`
3. Call `DirectiveParser.FindJsxBlockRanges()` on the body range to find embedded JSX
4. Call `DirectiveParser.FindBareJsxRanges()` on the body range
5. **NO call to `TryFindTopLevelReturn`** — don't split; keep body intact
6. Return new `ControlBlockBody` with `BodyCode`, `BodyCodeOffset`, `BodyCodeLine`,
   `BodyMarkupRanges`, `BodyBareJsxRanges`
7. Remove UITKX0024 diagnostic (no longer applicable)

Need to make `FindJsxBlockRanges` and `FindBareJsxRanges` accessible from
`UitkxParser` (currently private on `DirectiveParser`). Options:
- Move to `ReturnFinder` (shared utilities), or
- Make `internal static` on `DirectiveParser`

#### 1D. Update `ParseSwitch` to use new `ParseControlBlockBody`

File: `ide-extensions~/language-lib/Parser/UitkxParser.cs`, line 941–1096

ParseSwitch currently replicates `ParseControlBlockBody` logic manually. After
Stage 1C, it should call the new `ParseControlBlockBody` for each case body
instead of duplicating the return-finding logic.

#### 1E. Populate AST nodes with new fields

Update `ParseIf`, `ParseForeach`, `ParseFor`, `ParseWhile`, `ParseSwitch` to
populate `BodyCode`, `BodyCodeOffset`, `BodyCodeLine`, `BodyMarkupRanges`,
`BodyBareJsxRanges` from the new `ControlBlockBody` return value.

Remove `SetupCode`, `SetupCodeOffset`, `SetupCodeLine` properties from all
directive AST nodes. `Body` is no longer populated — remove it from the record
constructor (or keep as `ImmutableArray<AstNode>.Empty` if needed for record
syntax, but never read).

**Tests**: Add parser tests for:
- `@if` with returns at depth > 0
- `@foreach` with `return null;` in else branch
- `@if/@else` with JSX variables in setup code: `var x = (<Label />);`
- Deep nesting: `@foreach` → C# if → `return (<Box> @if { ... } </Box>)`
- `@switch @case` with C# switch inside

---

### Stage 2: Source Generator Emitter ✅

**File**: `SourceGenerator~/Emitter/CSharpEmitter.cs`

#### 2A. Create shared `SpliceBodyCodeMarkup` method

The existing `SpliceSetupCodeMarkup()` (line 1592) reads ranges from
`_directives.SetupCodeMarkupRanges` and uses `_directives.FunctionSetupStartOffset`
and gap offsets. For directive bodies there is NO gap (the body is intact).

Create a new overload or standalone method:
```csharp
private string SpliceBodyCodeMarkup(
    string bodyCode,
    int bodyCodeOffset,
    ImmutableArray<(int Start, int End, int Line)> markupRanges,
    ImmutableArray<(int Start, int End, int Line)> bareRanges)
```

Same logic as `SpliceSetupCodeMarkup` but:
- `fseOffset = bodyCodeOffset` (body start in source)
- `gapOffset = -1` (no gap)
- `gapLength = 0`

This means `AbsToSetupOffset` simply does `absPos - bodyCodeOffset` with no gap
subtraction. Simpler than the component version.

#### 2B. Rework `EmitIfNode`

Replace the two-path (IIFE vs ternary) with a single IIFE path when
`BodyCode != null`:

```csharp
if (anyBranchHasBodyCode)
{
    sb.Append($"((System.Func<{QVNode}>)(() => {{ ");
    for each branch:
        emit "if (cond) {" or "else if (cond) {" or "else {"
        string code = SpliceBodyCodeMarkup(branch.BodyCode, ...)
        code = ApplyHookAliases(code)  // hooks banned but setter sugar still applies
        code = ResolveAssetPaths(code)
        sb.Append(code)
        sb.Append(" } ")
    sb.Append($"return ({QVNode})null; }}))()");
}
```

Remove the old `SetupCode` + `EmitBodyAsFragment(body)` path entirely.
Also remove the expression-mode paths (ternary for `@if`, `.Select()` for
`@foreach`, switch-expression for `@switch`) — everything is IIFE now.

#### 2C. Rework `EmitForeachNode`

Always IIFE when `BodyCode != null`:
```csharp
sb.Append($"((System.Func<{QVNode}[]>)(() => {{ ");
sb.Append($"var __r = new List<{QVNode}>(); ");
sb.Append($"foreach ({iterDecl} in {collExpr}) {{ ");
string code = SpliceBodyCodeMarkup(forn.BodyCode, ...)
code = ApplyHookAliases(code)
code = ResolveAssetPaths(code)
// Replace "return (" with "__r.Add(" and "return null;" with "__r.Add(null);"?
// NO — the body already has return statements. Wrap the entire body in a
// local function and call it:
sb.Append($"__r.Add(((System.Func<{QVNode}>)(() => {{ {code} return ({QVNode})null; }}))());")
sb.Append(" } return __r.ToArray(); }))()");
```

**Key insight for iterators**: The body code has `return (<JSX/>)` at various
depths returning a single VirtualNode. For `@foreach`/`@for`/`@while`, each
iteration's return value must be added to a list. The cleanest approach is to
wrap the entire body code in a nested IIFE per iteration:

```csharp
foreach (var item in collection)
{
    __r.Add(((Func<VirtualNode>)(() => {
        // <body code with returns at any depth>
        return (VirtualNode)null;
    }))());
}
```

This lets the body `return` statements naturally produce values that get added
to the list.

#### 2D. Rework `EmitForNode`, `EmitWhileNode`

Same pattern as `EmitForeachNode` — nested IIFE per iteration.

#### 2E. Rework `EmitSwitchNode`

IIFE wrapping the switch statement. Each `@case` body code is emitted directly
inside the case block (no nested IIFE needed since `switch` branches don't
iterate):

```csharp
sb.Append($"((System.Func<{QVNode}>)(() => {{ switch ({expr}) {{ ");
foreach case:
    sb.Append($"case {val}: {{ ");
    string code = SpliceBodyCodeMarkup(...)
    sb.Append(code)
    sb.Append(" } ")
sb.Append($"}} return ({QVNode})null; }}))()");
```

#### 2F. UITKX0009 key check update

Currently `EmitForeachNode` checks `forn.Body` (the pre-parsed AST nodes) for
missing `key` attributes. With the new model, the body is raw C#, so the key
check must run on the JSX ranges that were found by `FindJsxBlockRanges`. The
emitter can mini-parse each JSX range and check for keys, or this check can move
to the diagnostics layer entirely.

**Tests**:
- Verify generated C# compiles for all directive types with deep returns
- Verify `return null;` produces `(VirtualNode)null` in output
- Verify iterator nested IIFE produces correct `__r.Add(...)` values
- Verify `SpliceBodyCodeMarkup` replaces `(<JSX/>)` with VirtualNode calls
- Verify `@(expr)` inside returned JSX works correctly
- Snapshot tests for generated code

---

### Stage 3: VDG (Virtual Document Generator) ✅

**File**: `ide-extensions~/language-lib/VirtualDocument/VirtualDocumentGenerator.cs`

#### 3A. New `EmitDirectiveBodyCode` method

For each directive in `EmitNodeExpressionsScoped`, when `BodyCode != null`:

1. Emit `#line` directive for source mapping
2. Call a lighter-weight version of `EmitFunctionStyleSetupSegmented` that:
   - Scans body code for `return (<JSX>)` and `return <Tag>` patterns
   - Replaces JSX with `(VirtualNode)null!` placeholder
   - Emits inline expression checks (`EmitInlineExprChecks`) for JSX attributes
   - Uses `b.Mapped()` for non-JSX C# segments (enables IntelliSense/go-to-def)
3. No gap handling needed (body is contiguous)

**Simplification**: Since there's no gap, we can avoid the full complexity of
`EmitMappedWithGap`. A simpler version works:
```csharp
b.Scaffold($"#line {bodyCodeLine} \"{escapedPath}\"\n");
b.Mapped(segment, bodyCodeOffset + segOffset, SourceRegionKind.CodeBlock, line);
```

#### 3B. Update `EmitNodeExpressionsScoped` for @if/@foreach/@for/@while/@switch

Replace the current pattern:
```csharp
if (branch.SetupCode != null) { b.Mapped(branch.SetupCode, ...); }
EmitNodeExpressionsScoped(branch.Body, ...);
```

With:
```csharp
EmitDirectiveBodyCode(branch.BodyCode, ...);
```

**Tests**:
- Verify IntelliSense works inside directive body code (go-to-definition,
  hover, completions for variables)
- Verify Roslyn diagnostics (type errors, etc.) map back to correct .uitkx lines
- Verify JSX attribute expressions inside returns get type-checked

---

### Stage 4: HMR Emitter ✅

**File**: `Editor/HMR/HmrCSharpEmitter.cs`

#### 4A. Add `SpliceBodyCodeMarkup` to HmrCSharpEmitter

Same approach as Stage 2A. HmrCSharpEmitter already has its own
`SpliceSetupCodeMarkup` (line 318). Create a parallel method for directive body
code, or refactor to share.

#### 4B. Update all 5 directive emit methods

Apply the same IIFE pattern as CSharpEmitter changes (Stage 2B–2E).
Additionally: emit `#line` directives for body code (currently missing for
directive setup code in HMR).

#### 4C. Read new AST fields

HmrCSharpEmitter reads AST via reflection (`GP<T>()` helper). Add reads for
`BodyCode`, `BodyCodeOffset`, `BodyCodeLine`, `BodyMarkupRanges`,
`BodyBareJsxRanges`.

**Tests**:
- Hot-reload a component containing `@foreach` with deep returns
- Verify delegate swap works and new IIFE code executes
- Verify `#line` directives produce correct error locations during HMR

---

### Stage 5: Formatter ✅

**File**: `ide-extensions~/language-lib/Formatter/AstFormatter.cs`

#### 5A. Detect body code mode

In `FormatIf`, `FormatForeach`, `FormatFor`, `FormatWhile`, `FormatSwitch`:

```csharp
// Single path: format body code as function body
EmitDirectiveBodyFormatted(branch.BodyCode);
```

#### 5B. `EmitDirectiveBodyFormatted`

Determine whether body code contains embedded JSX:
- If yes: use `EmitSetupCodeWithJsx`-style placeholder approach
- If no: use `EmitSetupCodeNormalized` (block-stack indentation)

Both methods already exist and are production-tested for function-style
components. The directive version passes the body code through the same pipeline,
with `_indent` set to the directive's nesting level.

**Tests**: Formatter snapshot tests for:
- `@if` with C# if/else containing returns at depth > 0
- `@foreach` with `return null;`
- Mixed JSX and C# in body code
- Idempotency: format twice → same output

---

### Stage 6: Diagnostics ✅

**File**: `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`

#### 6A. Hook scanning on `BodyCode`

Change to scan `BodyCode`:
```csharp
ScanCodeForHooks(branch.BodyCode, ...);
```

The hook scanner searches for `useState`, `useEffect`, etc. in raw strings.
It works unchanged on the new body code string.

#### 6B. Remove UITKX0024

`ParseControlBlockBody` no longer emits UITKX0024 since every body is raw C#.
Remove the diagnostic definition and any references to it.

#### 6C. Single-root validation

For `@if/@switch`: each branch must return either one JSX element (wrapped in
`<Fragment>` or a single container) or `null`. This validation currently runs via
`ValidateSingleRoot` on the pre-parsed `Body` nodes. With body code, validation
shifts to the emitter (C# compiler will enforce single return type) and the JSX
ranges (parser can mini-parse each range and check root count).

For `@foreach/@for/@while`: each iteration must return one element or null. Same
enforcement via the nested IIFE return type.

**Tests**: Verify hook-in-directive diagnostics still fire correctly with the new
body code scanning.

---

### Stage 7: Semantic Tokens ✅ (no changes needed)

**File**: `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs`

No changes expected. Currently relies on Layer 3 (Roslyn via VDG). After Stage 3
ensures correct VDG source mapping, semantic tokens work automatically.

Verify: C# keywords, types, function names inside directive body code get correct
coloring in VS Code.

---

### Stage 8: TextMate Grammar ✅ (no changes needed)

**File**: `ide-extensions~/grammar/uitkx.tmLanguage.json`

No changes needed. The `code-block-body` fallback pattern already provides C#
syntax highlighting inside `{ }` blocks.

---

### Stage 9: Tests ✅

#### 9A. Test results: 931/932 passing

One pre-existing failure: UITKX0009_ForeachMissingKey (diagnostic descriptor
`ForeachMissingKey` was defined but never reported by any validator — not a
regression from this refactor).

Across all test suites:
- Parser tests for deep returns, `return null;`, embedded JSX
- Emitter snapshot tests for generated C#
- VDG tests for source mapping accuracy
- Formatter idempotency tests
- Diagnostic tests (hooks still banned, key still required)
- HMR integration test (if test infra allows)

---

### Stage 10: Documentation ⬜

**Files**: `ReactiveUIToolKitDocs~/`, `ide-extensions~/docs/`

#### 10A. Update control-flow directive docs

Update all documentation pages covering `@if`, `@foreach`, `@for`, `@while`,
`@switch` directives to reflect the new body-as-function model:

- Bodies are C# function bodies that `return (<JSX/>)` or `return null;`
- Returns allowed at any depth (inside C# `if/else`, `switch`, loops, etc.)
- Setup code runs before any return — regular C# statements (variables, LINQ, etc.)
- Hooks remain forbidden inside directive bodies
- Iterators require `key` on returned JSX elements
- Single-root-element rule per branch/iteration
- `@(expr)` works inside returned JSX for inline expressions

#### 10B. Add examples showing deep nesting

Provide practical examples of the new pattern:
- `@foreach` with C# `if/else` returning different JSX or null
- `@if` with setup code computing values before returning JSX
- Nested directives: `@foreach` → `@if` → `@switch`
- Mixed C# control flow and UITKX directives in the same body

#### 10C. Update WebDocs site

If applicable, update the corresponding pages on the web documentation site to
match the updated directive body semantics.

---

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Old `SetupCode` consumers break | Clean break: update all consumers in one pass (Stages 1–6) |
| `SpliceBodyCodeMarkup` misses edge cases | Reuse proven `SpliceSetupCodeMarkup` logic; no gap simplifies it |
| Iterator nested IIFE performance | IIFE is already used throughout; one extra closure per iteration is negligible |
| Source mapping off-by-one in VDG | No gap means simpler offset math; extensive `#line` testing needed |
| Formatter regression | Snapshot test baseline before any changes |

## Files Modified (Complete List)

| File | Stage | Changes |
|------|-------|---------|
| `ide-extensions~/language-lib/Parser/ReturnFinder.cs` | 1A | `return null;` support |
| `ide-extensions~/language-lib/Nodes/AstNode.cs` | 1B | New fields on 5 node types |
| `ide-extensions~/language-lib/Parser/UitkxParser.cs` | 1C–1E | Rework ParseControlBlockBody + all Parse* methods |
| `ide-extensions~/language-lib/Parser/ParseResult.cs` | — | No changes (DirectiveSet unchanged) |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | 2A–2F | SpliceBodyCodeMarkup + rework 5 Emit*Node methods |
| `ide-extensions~/language-lib/VirtualDocument/VirtualDocumentGenerator.cs` | 3A–3B | EmitDirectiveBodyCode + update EmitNodeExpressionsScoped |
| `Editor/HMR/HmrCSharpEmitter.cs` | 4A–4C | SpliceBodyCodeMarkup + rework 5 directive emitters |
| `ide-extensions~/language-lib/Formatter/AstFormatter.cs` | 5A–5B | EmitDirectiveBodyFormatted + update 5 Format* methods |
| `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs` | 6A–6C | Hook scanning on BodyCode, remove UITKX0024 |
| `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs` | 7 | No changes expected |
| `ide-extensions~/grammar/uitkx.tmLanguage.json` | 8 | No changes |
| `SourceGenerator~/Tests/*.cs` | 9A | New + updated tests |
| `ReactiveUIToolKitDocs~/`, `ide-extensions~/docs/` | 10A–10C | Updated directive body docs + examples |
