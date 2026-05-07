# OPT-10: Eliminate Per-Iteration IIFE Closures in `@foreach`/`@for`/`@while`

> **Date:** April 25, 2026
> **Status:** ✅ COMPLETE
> **Objective:** Eliminate ~3,000 per-iteration delegate closures per frame in `@foreach` loops
> **Prerequisites:** OPT-16 (Style/BaseProps pooling ✅), OPT-18 (VNode pooling ✅)
> **Current:** ~35 FPS, ~3,000 remaining allocs/frame (almost entirely IIFE closures)
> **Target:** Near-zero per-frame allocs → GC pressure eliminated → FPS improvement

---

## Table of Contents

1. [Problem Statement](#problem)
2. [Current Codegen Architecture](#architecture)
3. [Ecosystem Impact Analysis](#ecosystem)
4. [Body Code Classification](#classification)
5. [The Fix: Inline Body Code, Remove Inner IIFE](#fix)
6. [Edge Cases & `_rentBuffer` Interaction](#edge-cases)
7. [break/continue Bug Fix (Bonus)](#break-continue)
8. [What Does NOT Change](#no-change)
9. [Implementation Steps](#steps)
10. [Test Plan](#testing)
11. [Risk Assessment](#risks)

---

## <a name="problem"></a>1. Problem Statement

Every `@foreach`, `@for`, and `@while` directive wraps each loop iteration body in
an IIFE (Immediately Invoked Function Expression) — a `Func<VirtualNode>` delegate:

```csharp
// Current codegen for: @foreach (var box in boxes) { return (<VisualElement .../>); }
((System.Func<VirtualNode[]>)(() => {              // outer IIFE (1 alloc)
    var __r = new List<VirtualNode>();              // 1 List alloc
    foreach (var box in boxes) {
        __r.Add(((System.Func<VirtualNode>)(() => { // inner IIFE — 1 alloc PER ITERATION
            return V.VisualElement(...);
        }))());
    }
    return __r.ToArray();                           // 1 array copy
}))()
```

For 3,000 boxes: **3,000 delegate closures + 1 List + 1 Array.Copy = ~3,002 allocations**
per frame, just for loop scaffolding. The VNode/Style/BaseProps objects are now pooled
(OPT-16/18), but these closures remain.

---

## <a name="architecture"></a>2. Current Codegen Architecture

### 2.1 Three Emitters

The UITKX ecosystem has **three independent emitters** that all produce the same IIFE pattern.
All three must be updated:

| Emitter | File | Description |
|---------|------|-------------|
| **Source Generator** | `SourceGenerator~/Emitter/CSharpEmitter.cs` | Roslyn incremental generator (build-time). Produces `.g.cs` files. |
| **HMR Emitter** | `Editor/HMR/HmrCSharpEmitter.cs` | Runtime hot-reload emitter. Uses reflection to access the shared AST (objects are `object`, properties accessed via `GP<T>()`). Produces in-memory C# for on-the-fly compilation. |
| **LSP Virtual Doc** | `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | IDE-side emitter for IntelliSense. Produces a virtual C# document for Roslyn diagnostics. **Does NOT use IIFEs** — emits natural `foreach` blocks. No changes needed. |

### 2.2 Key Methods Per Emitter

| Method | Source Gen (`CSharpEmitter.cs`) | HMR (`HmrCSharpEmitter.cs`) |
|--------|--------------------------------|------------------------------|
| foreach | `EmitForeachNode()` L1360 | `EmitForeach()` L1082 |
| for | `EmitForNode()` L1312 | `EmitFor()` L1107 |
| while | `EmitWhileNode()` L1336 | `EmitWhile()` L1130 |
| if | `EmitIfNode()` L1264 | `EmitIf()` L1040 |
| switch | `EmitSwitchNode()` L1384 | `EmitSwitch()` L1153 |

### 2.3 The Inner IIFE Pattern

All three loop emitters (`foreach`, `for`, `while`) share identical structure:

```csharp
// Outer IIFE: wraps the entire loop to produce VirtualNode[]
((System.Func<VirtualNode[]>)(() => {
    var __r = new List<VirtualNode>();
    foreach (ITER in COLL) {
        // Inner IIFE: wraps each iteration body to produce single VirtualNode
        __r.Add(((System.Func<VirtualNode>)(() => {
            TRANSFORMED_BODY_CODE     // ← user code with JSX spliced in
            RENT_STATEMENTS           // ← pool-rent stmts from _rentBuffer
            return (VirtualNode)null;  // ← fallback if user code doesn't return
        }))());
    }
    return __r.ToArray();
}))()
```

The inner IIFE exists because:
1. Body code may contain `return (...)` statements that need to yield a single VNode
2. Body code may have setup variables before the return
3. The `return` inside the lambda maps to "produce the iteration value"

### 2.4 The `_rentBuffer` Mechanism

When emitting a JSX element like `<VisualElement style={new Style { ... }} />`, the
emitter generates **pool-rent statements** that must execute before the element
expression:

```csharp
// _rentBuffer accumulates these:
var __p_0 = BaseProps.__Rent<VisualElementProps>();
var __s_0 = Style.__Rent();
__s_0.Left = box.x;
__p_0.Style = __s_0;

// _sb accumulates the expression:
V.VisualElement(__p_0, key: box.id)
```

Inside an inner IIFE, the rent statements are prepended inside the lambda body:
```csharp
__r.Add(((Func<VNode>)(() => {
    var __p_0 = BaseProps.__Rent<VisualElementProps>();  // rent stmt
    var __s_0 = Style.__Rent();                          // rent stmt
    __s_0.Left = box.x;                                  // rent stmt
    __p_0.Style = __s_0;                                 // rent stmt
    return V.VisualElement(__p_0, key: box.id);          // expression
}))());
```

When we remove the inner IIFE, these rent statements must be placed **directly in
the loop body** before the element expression. This is straightforward because the
body code + rent statements + element expression all move into the loop body together.

### 2.5 `TransformBodyCode` Pipeline

`BodyCode` is the raw text between `{` and `}` of the directive body. It goes through:
1. `SpliceBodyCodeMarkup` — replaces JSX ranges (detected by parser) with emitted C# 
2. `ApplyHookAliases` — rewrites `useState` → `Hooks.UseState` etc.
3. `ResolveAssetPaths` — resolves `Asset<T>` paths

The output is valid C# with `return V.SomeElement(...)` statements at arbitrary depth.
This pipeline is unchanged by OPT-10 — we only change how the output is placed.

### 2.6 HMR Emitter Differences

The HMR emitter mirrors the source gen but with key differences:
- Uses `object` + reflection (`GP<T>(node, "FieldName")`) instead of typed AST records
- **Wraps foreach/for/while result in `V.Fragment()`** instead of returning `VirtualNode[]`:
  ```csharp
  return V.Fragment(key: null, __items.ToArray());
  ```
  This is because HMR emits inline expressions, not `params VirtualNode[]` arrays.
- The same inner IIFE pattern is used per iteration

---

## <a name="ecosystem"></a>3. Ecosystem Impact Analysis

### 3.1 Parser — NO CHANGES

| Component | Impact |
|-----------|--------|
| `UitkxParser.cs` (shared parser) | None. The parser produces the AST; it doesn't know about IIFEs. |
| `ForeachNode`, `ForNode`, `WhileNode` records | None. Fields unchanged. |
| `ParseControlBlockBody()` | None. Still produces `BodyCode`, `BodyMarkupRanges`, etc. |
| `ReturnFinder` | None. Still used by parser/IDE for finding `return (...)`. |

### 3.2 Diagnostics — NO CHANGES

| Analyzer | Impact |
|----------|--------|
| `HooksValidator` (UITKX0014: hook in loop) | None. Scans `BodyCode` text for hook calls — doesn't depend on emit pattern. |
| `StructureValidator` (useEffect missing deps) | None. Scans `BodyCode` text. |
| `DiagnosticsAnalyzer` (UITKX0106: missing key) | None. Inspects `Body` AST nodes, not emitted code. |
| Source gen diagnostics (UITKX0009, UITKX0010, etc.) | None. Operate on AST, not emitted code. |

### 3.3 Formatter — NO CHANGES

| Component | Impact |
|-----------|--------|
| `AstFormatter.FormatForeach()` | None. Formats the `.uitkx` source — has nothing to do with emitted C#. |
| `AstFormatter.FormatFor()` | None. |
| `AstFormatter.FormatWhile()` | None. |

### 3.4 Grammar / TextMate — NO CHANGES

The grammar tokenizes `.uitkx` syntax (`@foreach`, `@for`, `@while` keywords). It has
nothing to do with the emitted C# output.

### 3.5 IDE / LSP Virtual Document — NO CHANGES

`VirtualDocumentGenerator` already emits **natural C# loops** (no IIFEs) for IntelliSense:
```csharp
foreach (var x in items) {
    // body code emitted directly
}
```
This is correct and unchanged.

### 3.6 Source Generator — CHANGES REQUIRED

`CSharpEmitter.EmitForeachNode()`, `EmitForNode()`, `EmitWhileNode()` must be rewritten
to eliminate the inner IIFE.

### 3.7 HMR Emitter — CHANGES REQUIRED

`HmrCSharpEmitter.EmitForeach()`, `EmitFor()`, `EmitWhile()` must be rewritten
with the same pattern.

### 3.8 Syntax — NO CHANGES

The `.uitkx` syntax for `@foreach`, `@for`, `@while` is completely unchanged. Users
write exactly the same code. The generated C# output is different but semantically
equivalent.

### 3.9 Breaking Changes — NONE

- User `.uitkx` code: unchanged
- Generated API surface: unchanged (V.*, __C, etc.)
- Runtime behavior: identical (same VNodes produced)
- IDE experience: unchanged (diagnostics, formatting, IntelliSense)
- HMR hot-reload: unchanged (same emitter pattern, just different codegen)

---

## <a name="classification"></a>4. Body Code Classification

From the inventory of all 43 occurrences across the codebase:

### 4.1 Body Patterns Found

| Pattern | Count | Example |
|---------|-------|---------|
| **Single-expression** — `return (<Element/>);` only | 16 | StressTest boxes |
| **Multi-statement** — setup vars + `return (...)` | 22 | DoomGame strips |
| **Early return null** — `if (cond) return null;` then `return (...)` | 5 | GalagaGame enemies |
| **Nested @if/@switch/@foreach** inside return JSX | 8 | NestedSection grid |
| **break/continue** — `if (cond) { continue; }` | 2 | SetupCodeSection, ForSection |

### 4.2 Key Insight: ALL Patterns Work Without Inner IIFE

The inner IIFE's only purpose is to turn `return V.Element(...)` into a value expression.
But we don't need that — we can replace `return (EXPR)` with `__r.Add(EXPR)` and
`return null` with `continue` in the transformed body code:

| User writes | Current codegen (inner IIFE) | New codegen (inlined) |
|-------------|-----------------------------|-----------------------|
| `return (<Element/>);` | `return V.Element(...)` inside lambda | `__r[__i++] = V.Element(...);` |
| `return null;` | `return null` inside lambda (adds null, filtered) | `continue;` or skip |
| `var x = 1; return (<E/>);` | `var x = 1; return V.E(...)` inside lambda | `var x = 1; __r[__i++] = V.E(...);` |
| `if (cond) return null; return (<E/>);` | works inside lambda | `if (cond) { continue; } __r[__i++] = V.E(...);` |
| `if (cond) { break; }` | **BROKEN** inside lambda (C# error) | Works naturally — `break` is in the loop |
| `if (cond) { continue; }` | **BROKEN** inside lambda (C# error) | Works naturally — `continue` is in the loop |

### 4.3 The `return` Rewrite

The transformation is a text replacement in `TransformBodyCode`'s output:
- `return (EXPR);` → `{ RENT_STMTS __r[__i++] = EXPR; continue; }`
  (or `__r.Add(EXPR); continue;` for `List`-based approach)
- `return null;` → `continue;`
- The final fallback `return (VirtualNode)null;` at the end of the old IIFE → removed

Wait — this is simpler than text replacement. The body code after `TransformBodyCode`
has already had JSX spliced in. The `return (...)` statements now contain C# expressions
like `return V.VisualElement(...)`. We need to:

1. Replace `return (<C# expression>);` with `__r.Add(<C# expression>); continue;`
2. Replace `return null;` with `continue;`

But this is fragile text manipulation. A better approach:

**Don't rewrite returns. Keep the inner function, but make it a `static local function` 
instead of a lambda to avoid the closure allocation.**

Wait — that still allocates a delegate. The issue is the delegate allocation per iteration.

**Actually, the simplest and most robust approach:** Just inline the body code directly
into the loop body, and replace `return X;` with `{ __r.Add(X); continue; }`. The body
code is already valid C# after `TransformBodyCode`.

### 4.4 Simpler Alternative: `TransformBodyCode` Already Handles Everything

Looking at the generated code structure more carefully:

```csharp
__r.Add(((Func<VNode>)(() => { 
    BODY_CODE    // contains: var x = 1; return V.Element(...)
    RENT_STMTS   // contains: var __p_0 = BaseProps.__Rent<...>(); ...
    return (VNode)null;  // fallback
}))());
```

The `BODY_CODE` already has `return V.Element(...)` which, inside the IIFE lambda,
returns from the lambda. Without the IIFE, we need `return` to mean "this is the
element to add to the list, then move to next iteration."

**The cleanest approach is a text transformation on `BODY_CODE`:**

```
return EXPR;  →  __r.Add(EXPR); continue;
return null;  →  continue;
```

This handles ALL cases including multi-statement bodies, early returns, setup variables,
and nested directives (which are already spliced into the body code by `SpliceBodyCodeMarkup`).

---

## <a name="fix"></a>5. The Fix: Inline Body Code, Remove Inner IIFE

### 5.1 New Codegen Pattern — Source Generator

**Before (current):**
```csharp
((System.Func<VirtualNode[]>)(() => {
    var __r = new System.Collections.Generic.List<VirtualNode>();
    foreach (var box in boxes) {
        __r.Add(((System.Func<VirtualNode>)(() => {
            // body code with JSX spliced in
            var __p_0 = BaseProps.__Rent<VisualElementProps>();
            var __s_0 = Style.__Rent(); __s_0.Left = box.x; ...
            __p_0.Style = __s_0;
            return V.VisualElement(__p_0, key: box.id);
            return (VirtualNode)null;
        }))());
    }
    return __r.ToArray();
}))()
```

**After (optimized):**
```csharp
((System.Func<VirtualNode[]>)(() => {
    var __r = new System.Collections.Generic.List<VirtualNode>();
    foreach (var box in boxes) {
        // body code inlined directly — no inner IIFE
        var __p_0 = BaseProps.__Rent<VisualElementProps>();
        var __s_0 = Style.__Rent(); __s_0.Left = box.x; ...
        __p_0.Style = __s_0;
        __r.Add(V.VisualElement(__p_0, key: box.id)); continue;
    }
    return __r.ToArray();
}))()
```

**Allocations eliminated:** N delegate closures per loop → 0

**Allocations remaining:** 1 outer IIFE delegate + 1 List + 1 ToArray. These are
O(1) per loop, not O(N). For 3,000 boxes: 3,002 → 3 allocations.

### 5.2 The `return` Rewriter

A helper function that transforms the body code output:

```csharp
/// <summary>
/// Rewrites 'return EXPR;' → '__r.Add(EXPR); continue;'
/// and 'return null;' → 'continue;'
/// in the transformed body code to inline it directly in the loop body.
/// </summary>
private static string RewriteReturnsForInline(string bodyCode, string listVar)
```

This operates on the **post-`TransformBodyCode`** output (already valid C#). It:
1. Finds `return null;` → replaces with `continue;`
2. Finds `return EXPR;` → replaces with `{listVar}.Add(EXPR); continue;`

The `continue;` ensures the loop body ends after adding the element (equivalent to
the lambda returning from the IIFE).

**Implementation approach:** Use regex or manual scanning to find `return` at statement
positions (not inside string literals, not inside nested lambdas). This is the same
level of text-rewriting complexity as existing `ApplyHookAliases`.

### 5.3 New Codegen Pattern — HMR Emitter

Same transformation. The HMR emitter wraps the loop result in `V.Fragment()` instead of
returning an array, but the inner IIFE elimination is identical.

**Before:**
```csharp
((Func<VNode>)(() => {
    var __items = new List<VNode>();
    foreach (ITER in COLL) {
        __items.Add(((Func<VNode>)(() => { BODY_CODE RENT return (VNode)null; }))());
    }
    return V.Fragment(key: null, __items.ToArray());
}))()
```

**After:**
```csharp
((Func<VNode>)(() => {
    var __items = new List<VNode>();
    foreach (ITER in COLL) {
        REWRITTEN_BODY_CODE
        RENT_STMTS
    }
    return V.Fragment(key: null, __items.ToArray());
}))()
```

---

## <a name="edge-cases"></a>6. Edge Cases & `_rentBuffer` Interaction

### 6.1 Rent Statements in Loop Bodies

Rent statements (`var __p_N = BaseProps.__Rent<T>(); ...`) are generated by
`EmitBuiltinTyped` when it encounters a JSX element with props. Inside the inner
IIFE, they're accumulated in `_rentBuffer` and prepended in the lambda body.

**Without the inner IIFE**, rent statements must be placed directly in the loop body.
The existing save/restore `_rentBuffer` pattern already handles this:

```csharp
var savedRent = _rentBuffer;
_rentBuffer = new StringBuilder();
var code = TransformBodyCode(...);     // may emit rent stmts for nested JSX
string rentStmts = _rentBuffer.ToString();
_rentBuffer = savedRent;
```

The `rentStmts` string contains complete C# declaration statements. We emit them
**before** the rewritten body code in the loop body:
```csharp
foreach (var box in boxes) {
    RENT_STMTS    // var __p_0 = ...; var __s_0 = ...; etc.
    REWRITTEN_BODY_CODE  // var x = 1; __r.Add(V.Element(...)); continue;
}
```

Wait — there's a subtlety. The rent statements are generated during `TransformBodyCode`
→ `SpliceBodyCodeMarkup` → `EmitNode` for inline JSX. They reference variables used
in the element expression. The placement order is: rent stmts → then the expression
that uses them.

Inside `SpliceBodyCodeMarkup`, rent statements are already inserted at the correct
position within the body code (before the expression that uses them). So when we get
back the transformed `code` and `rentStmts`, they're already interleaved correctly.

Actually, re-reading the code: `SpliceBodyCodeMarkup` handles **inline JSX within
body code** (e.g., `var catBadge = (<VisualElement .../>`). But the main return
expression's rent statements are in the top-level `_rentBuffer` and are separate.

So the placement is:
1. `code` — body code with inline JSX rent stmts already spliced in
2. `rentStmts` — rent stmts from the final return expression's JSX elements

Both go directly into the loop body, in order: `{rentStmts}{code}` — same as current
IIFE body.

Wait, looking at the current pattern more carefully:
```csharp
$"__r.Add(((System.Func<{QVNode}>)(() => {{ {code} {rentStmts}return ({QVNode})null; }}))());"
```

The order is: `code` first, then `rentStmts`, then fallback return. This means the
rent statements come AFTER the body code? That seems wrong for scoping...

Actually no — the rent statements from the final return JSX are hoisted by
`SpliceBodyCodeMarkup`'s `insertPos` logic. When `SpliceBodyCodeMarkup` processes a
JSX range inside body code, it finds the last `;` or `}` before the JSX position
and inserts the rent statements there. So rent statements for inline JSX are already
placed inside `code` at the right position.

The top-level `rentStmts` after `code` handle any remaining rent statements that
weren't part of inline JSX. In practice, this is rare — most JSX in body code is
inline (processed by `SpliceBodyCodeMarkup`).

For the inlined pattern, we simply emit: `{code} {rentStmts}` directly in the loop
body, with the `return` rewritten to `__r.Add()`.

### 6.2 Nested Directives (e.g., `@if` inside `@foreach`)

When `@foreach` body contains `@if` or `@switch`, these are nested inside the body's
JSX. They go through `SpliceBodyCodeMarkup` → `EmitNode` → `EmitIfNode`/etc., which
produce their own IIFEs. These nested IIFEs are independent — they produce a single
`VirtualNode` expression value. They work fine without the outer per-iteration IIFE.

Example: `@foreach (var item in items) { return (<Box> @if (cond) {...} </Box>); }`
The `@if` inside `<Box>` becomes part of `__C(...)` children, which is fine.

### 6.3 `return null;` (Skip Iteration)

Several patterns use `return null;` or `if (cond) return null;` to skip producing
an element for certain iterations. In the current IIFE, this returns `null` from
the lambda, which gets added to `__r`, and `__C` filters out nulls.

Without the IIFE, `return null;` → `continue;` (skip adding anything to `__r`).
This is more efficient — no null added, no filtering needed.

### 6.4 Variable Name Collisions (`__r`, `__i`)

The variables `__r` (list/array) and optionally `__i` (index) are introduced by the
emitter. They use the double-underscore convention to avoid collisions with user code.
User variables in `.uitkx` never start with `__` (convention-enforced by the parser).
This is the same as existing `__p_N`, `__s_N`, `__ci`, `__vn` etc.

### 6.5 Empty BodyCode

If `BodyCode` is null, no iteration body is emitted — the loop body is empty.
Current code already handles this (`if (forn.BodyCode != null) { ... }`).
This edge case is unchanged.

---

## <a name="break-continue"></a>7. `break`/`continue` Bug Fix (Bonus)

### Current Bug

In the current codegen, user-written `break;` and `continue;` inside `@for`/`@while`
bodies are placed inside the inner IIFE lambda. This is a **C# compilation error** —
`break`/`continue` cannot cross lambda boundaries.

Test `ForDirective_WithLoopFlow_EmitsBreakAndContinueStatements` only checks that the
text appears in the generated output, not that the generated code compiles successfully.

### Fix

By removing the inner IIFE, `break;` and `continue;` statements naturally target
the enclosing `for`/`while`/`foreach` loop — exactly what the user intends.

### Note on `return` Rewriting & `break`/`continue`

When we rewrite `return EXPR;` → `__r.Add(EXPR); continue;`, we must ensure that
user-written `continue;` is not rewritten. The rewriter should only target `return`
statements, not `continue`/`break`.

User-written `continue;` means "skip this iteration, don't produce an element" — 
which is naturally correct when inlined in the loop body (skip the `__r.Add` and
go to the next iteration).

User-written `break;` means "stop the loop entirely" — also naturally correct
when inlined.

---

## <a name="no-change"></a>8. What Does NOT Change

| Component | Why No Change |
|-----------|---------------|
| **Parser** (`UitkxParser.cs`) | Produces AST from `.uitkx` source. Doesn't know about emitted C#. |
| **Grammar** (TextMate, VS Code) | Tokenizes `.uitkx` keywords. Unrelated to C# output. |
| **Formatter** (`AstFormatter.cs`) | Formats `.uitkx` source layout. Unrelated to C# output. |
| **Diagnostics** (`DiagnosticsAnalyzer`, `HooksValidator`, `StructureValidator`) | Operate on AST or raw body code text, not emitted C#. |
| **LSP Virtual Doc** (`VirtualDocumentGenerator.cs`) | Already emits natural loops without IIFEs. |
| **Runtime** (`V.cs`, `VNode.cs`, `Fiber*.cs`, `Hooks.cs`, etc.) | No changes. The emitted C# calls the same APIs. |
| **User `.uitkx` syntax** | Completely unchanged. No new syntax, no deprecated syntax. |
| **`@if` and `@switch` IIFEs** | These are 1 alloc per occurrence (not per iteration). Low impact. Can be optimized separately later if needed. Not part of OPT-10. |
| **Outer IIFE** | The outer `((Func<VNode[]>)(() => { ... }))()` wrapping the entire loop stays. It's 1 alloc per loop, not per iteration. Removing it would require changes to how loop results are consumed as children (`__C()`). Not worth the complexity. |
| **`@if`/`@switch` IIFEs** | 1 alloc per occurrence, not O(N). Not the performance bottleneck. Future optimization (OPT-10b) could convert to ternary expressions but risk is higher and reward is lower. |

---

## <a name="steps"></a>9. Implementation Steps

### Step 1: Implement `RewriteReturnsForInline` helper

**File:** `SourceGenerator~/Emitter/CSharpEmitter.cs`

Add a static helper method that rewrites return statements in the transformed body code:
- `return null;` → `continue;`
- `return EXPR;` → `__r.Add(EXPR); continue;`

Must handle:
- Multiple return statements (guard clauses: `if (x) return null; return (<E/>);`)
- Return expressions spanning multiple lines (unlikely after `TransformBodyCode` but handle anyway)
- NOT rewriting returns inside nested lambdas (`() => { return ...; }`) — these are user lambdas

Implementation: walk the code character by character, track brace/paren depth and
lambda context. When a top-level `return` is found, classify it (null vs expression)
and rewrite.

### Step 2: Rewrite `EmitForeachNode`

**File:** `SourceGenerator~/Emitter/CSharpEmitter.cs`

```csharp
private void EmitForeachNode(ForeachNode forn)
{
    _sb.Append($"((System.Func<{QVNode}[]>)(() => {{ ");
    _sb.Append($"var __r = new System.Collections.Generic.List<{QVNode}>(); ");
    _sb.Append($"foreach ({forn.IteratorDeclaration} in {forn.CollectionExpression}) {{ ");
    if (forn.BodyCode != null)
    {
        var savedRent = _rentBuffer;
        _rentBuffer = new StringBuilder();
        var code = TransformBodyCode(
            forn.BodyCode, forn.BodyCodeOffset,
            forn.BodyMarkupRanges, forn.BodyBareJsxRanges
        );
        string rentStmts = _rentBuffer.ToString();
        _rentBuffer = savedRent;

        // Inline body code directly — no inner IIFE
        string inlined = RewriteReturnsForInline(code, "__r");
        _sb.Append($"{rentStmts}{inlined} ");
    }
    _sb.Append("} return __r.ToArray(); }))()");
}
```

### Step 3: Rewrite `EmitForNode` and `EmitWhileNode`

**File:** `SourceGenerator~/Emitter/CSharpEmitter.cs`

Same pattern as Step 2 — replace inner IIFE with inlined body code.

### Step 4: Rewrite HMR `EmitForeach`, `EmitFor`, `EmitWhile`

**File:** `Editor/HMR/HmrCSharpEmitter.cs`

Same transformation. The HMR emitter uses `__items` instead of `__r` and wraps in
`V.Fragment()`, but the inner IIFE elimination is identical.

### Step 5: Rebuild Source Generator DLL

Build the `SourceGenerator~` project to produce the updated `ReactiveUITK.SourceGenerator.dll`.
Copy to `Analyzers/` so Unity picks it up.

### Step 6: Update Tests

**File:** `SourceGenerator~/Tests/EmitterTests.cs`

- `ForeachDirective_WithSetupCode_GeneratesIIFE`: Update assertion — the inner IIFE
  `Func<VirtualNode>` should no longer appear
- `ForeachDirective_WithSetupCode_InRealComponent_GeneratesValidCSharp`: Update the
  `"} return __r.ToArray(); }))()"`  assertion — brace count changes
- `ForDirective_WithLoopFlow_EmitsBreakAndContinueStatements`: Now should verify that
  break/continue appear in the loop body (NOT inside a lambda)
- `WhileDirective_WithLoopFlow_EmitsContinueStatement`: Same

### Step 7: Verify all samples compile

Trigger source gen rebuild in Unity. Verify all samples compile and render correctly.
Pay special attention to:
- StressTest (3000 boxes, single-expression foreach)
- DoomGameScreen (multi-statement foreach with nested foreach)
- GalagaGame (early return null patterns)
- DeepNestedSection (deeply nested foreach/for/if/switch)
- SetupCodeSection (break/continue in for loop — should now actually work!)

---

## <a name="testing"></a>10. Test Plan

### 10.1 Automated Tests (Source Generator)

| Test | Verify |
|------|--------|
| Simple foreach | No inner `Func<VirtualNode>` in output. `__r.Add(V.Label(...))` pattern present. |
| Foreach with setup code | Setup vars appear in loop body, not in lambda. |
| For with break/continue | `break;` and `continue;` appear in loop body, NOT inside any lambda. |
| While with continue | Same. |
| Foreach with return null | `continue;` appears instead of inner IIFE returning null. |
| Nested foreach inside foreach | Both levels inline correctly. |
| Foreach with nested @if in JSX | @if IIFE is inside element children, not conflicting. |

### 10.2 Manual Tests (Unity Runtime)

| Test | Verify |
|------|--------|
| StressTest 3000 boxes | Renders correctly. FPS ≥ 35. **Near-zero per-frame allocs** in Profiler. |
| DoomGameScreen | All strips, sprites, minimap render correctly. |
| GalagaGame | Enemies with dead/alive guards render correctly. |
| Showcase demos | DirectiveSuccessDemo all tabs render correctly. |
| HMR hot-reload | Edit a component with @foreach → hot-reload updates correctly. |
| ForSection break/continue | Now should actually work (was previously broken). |

### 10.3 Allocation Verification

After OPT-10, per-frame allocs in the StressTest should be:
- **VirtualNode**: ~0 (pooled, OPT-18)
- **Style**: ~0 (pooled, OPT-16)
- **BaseProps**: ~0 (pooled, OPT-16)
- **IIFE closures**: **~0** (eliminated, OPT-10)
- **List + Array** from outer IIFE: ~2 per loop occurrence (O(1), negligible)
- **__C() List + Array**: present but O(component-count), not O(element-count)
- **params object[]** from __C: present but O(component-count)

Total: near-zero steady-state allocs → GC should barely trigger.

---

## <a name="risks"></a>11. Risk Assessment

| Risk | Severity | Likelihood | Mitigation |
|------|----------|-----------|------------|
| `RewriteReturnsForInline` misparses a return | High | Low | Robust parser with lambda-depth tracking. Comprehensive tests. Fallback to current IIFE if parsing fails. |
| Rent statement ordering breaks | Medium | Very Low | Order is preserved from existing pipeline. Already works inside IIFE. |
| Variable name collision with `__r` | Low | Very Low | Double-underscore convention. No user code uses `__r`. |
| HMR emitter diverges from source gen | Medium | Low | Both emitters follow same template. Update both in same step. |
| Nested @foreach inside @foreach both use `__r` | High | Medium | Each nested loop is inside its own outer IIFE scope, so inner `__r` shadows outer. If scoping breaks, use unique names like `__r_0`, `__r_1`. |
| `return` inside user-written lambda in body code | High | Low | The rewriter must track lambda depth and only rewrite top-level returns. User lambdas like `items.Select(x => { return x; })` must not be touched. |

### 11.1 Nested Loop Variable Shadowing

When `@foreach` is nested inside another `@foreach`, both emit `var __r = new List<>()`.
Currently, each is inside its own outer IIFE scope, so the inner `__r` shadows the outer.

With the inner IIFE removed, both loops are still inside separate outer IIFEs (each
`@foreach` produces its own `((Func<VNode[]>)(() => { var __r = ...; foreach ... }))()`.
The inner foreach's outer IIFE is nested inside the outer foreach's body, so `__r`
shadowing still works correctly.

Verify this with the `NestedSection.uitkx` test case.

---

## Summary

| Aspect | Impact |
|--------|--------|
| Files changed | 2 (CSharpEmitter.cs, HmrCSharpEmitter.cs) |
| Runtime files changed | 0 |
| User syntax changed | None |
| Breaking changes | None |
| Diagnostics changed | None |
| Formatter changed | None |
| Grammar changed | None |
| LSP/IDE changed | None |
| Tests updated | ~4-6 tests in EmitterTests.cs |
| Allocs eliminated | ~3,000/frame (per-iteration delegates) |
| Bug fixed | `break`/`continue` in @for/@while now works |
| Source gen DLL rebuild | Required |
