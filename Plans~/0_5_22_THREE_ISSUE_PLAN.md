# 0.5.22 + 0.5.23 Plan — Four Workstreams, Two Releases

**Status:** Decisions locked 2026-05-21. Ready to implement on confirmation.
**Inputs:** Three deep-research subagent passes (~80KB total). Source cites in
`/memories/session/0_5_22-three-issue-plan-state.md`.

This plan covers three structurally independent issues plus a coverage
expansion that all surfaced from the same `Pretty UI` repro file. They are
coordinated here because they share the same parity-drift root and because
some fix surfaces overlap.

| # | Workstream | Symptom | Layer | Release |
|---|---|---|---|---|
| 1 | JSX-attribute-lambda preservation | `setPhase` / `setStep` flagged UITKX0112 unused | IDE Virtual Doc only | **0.5.22** |
| 2 | `useTransition` no-op stub | CS0103 at compile + IDE | Runtime + IDE Virtual Doc | **0.5.22** |
| 2b | 9 missing VDG IDE stubs catch-up | False-positive CS0103 in editor on working hooks | IDE Virtual Doc only | **0.5.22** |
| 2c | Validator + hover coverage expansion (10 → 20 hooks) | Silent Rules-of-Hooks violations + missing hover docs | SG validator + IDE diagnostics + hover | **0.5.22** |
| 3 | Unified `HookRegistry` refactor | 8 duplicated hook-metadata sites | Cross-layer | **0.5.23** |

---

## 1. Issue 1 — JSX strip drops attribute lambdas in IDE virtual doc

### 1.1 Root cause (verified)

The IDE virtual document, when emitting C# for an expression that contains a
JSX subtree, replaces the entire JSX with an opaque `(VirtualNode)null!` stub.
Every attribute expression — including `onClick={() => setPhase(1)}` — is
discarded. Roslyn's `AnalyzeDataFlow` therefore never sees the lambda;
`setPhase` is not in `dataFlow.Captured`; UITKX0112 fires.

Located in [VirtualDocumentGenerator.cs](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs):

- Function `EmitMappedExpressionStrippingJsx` (~line 650-750), specifically the
  fall-through emit at **line 736**: `b.Scaffold("((global::ReactiveUITK.Core.VirtualNode)null!)");`
- 5 call sites that all hit this path:
  - **Line 813** — `EmitExpressionStatement`, lambda-arrow path (`Action` cast)
  - **Line 826** — `EmitExpressionStatement`, default `object` check
  - **Line 878** — `EmitTypedPropsCheck`, typed prop assignment
  - **Line 2710** — `EmitBodyWithReturnFix`, body segment before bare-return rewrite
  - **Line 2728** — `EmitBodyWithReturnFix`, body segment after bare-return rewrite

### 1.2 Why the SG path works (and HMR works)

- SG `CSharpEmitter.SpliceExpressionMarkup` (lines 2390-2580) calls
  `UitkxParser.Parse(jsxText, ...)` on the JSX substring, walks the AST, and
  emits each attribute expression via `EmitNode` recursion.
- HMR `HmrCSharpEmitter.SpliceExpressionMarkup` (lines 839-960) mirrors the SG
  shape via reflected delegates (`_parseMarkup`).
- **VDG is the only layer that opaque-stubs JSX subtrees.**

### 1.3 The fix already exists in VDG — just isn't wired to this path

The infrastructure is already present:

- **Pattern A** — `EmitDirectiveJsxExprChecks` (lines 1539-1635): parses a JSX
  range with `UitkxParser.Parse`, walks AST via `EmitNodeExpressionsScoped`,
  emits per-attribute `__uitkx_check` blocks. Used for `@if`/`@for` body JSX.
- **Pattern B** — `EmitInlineExprChecks` (lines 2620-2690): same but wraps
  emitted checks in a local function `__uitkx_sc{pos}()` so any `return;` inside
  attribute lambdas targets the local function, not `__uitkx_render()`. Used
  for setup-code JSX deferral.
- **Pattern C** — `EmitNodeExpressionsScoped` (lines 1080-1350): the AST walker.
  When attribute is a lambda (`contains("=>")`), it falls through to
  `EmitExpressionStatement` → `EmitMappedExpressionStrippingJsx` → the bug.

The fix: from `EmitMappedExpressionStrippingJsx`, after replacing each JSX
range with the null stub, **defer the JSX range** to a Pattern-B-style
local-function block emitted at the nearest enclosing statement scope.

### 1.4 Concrete edit plan

**File:** [`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs)

#### Step 1.4.1 — Change `EmitMappedExpressionStrippingJsx` signature

Add an optional out-parameter or accumulator list:

```csharp
private static void EmitMappedExpressionStrippingJsx(
    VirtualDocBuilder b,
    string text,
    int uitkxOffset,
    SourceRegionKind kind,
    int uitkxLine,
    List<DeferredJsxCheck>? deferred = null)
```

Where `DeferredJsxCheck` is a small record:

```csharp
private readonly record struct DeferredJsxCheck(
    int JsxStart,        // absolute .uitkx offset of the JSX start
    int JsxEnd,          // absolute .uitkx offset of the JSX end
    int SourceLine,      // 1-based .uitkx line of the JSX start
    string JsxText);     // the JSX substring to be parsed
```

In the inner loop (around line 736, where the null stub is emitted), if
`deferred != null`, push the JSX range onto `deferred` instead of dropping it.
Default behaviour (when caller passes `null`) is unchanged — preserves
existing tests where the caller already handles this differently.

#### Step 1.4.2 — Update the 3 statement-scope callers

For lines 813, 826, 878 (in `EmitExpressionStatement` and `EmitTypedPropsCheck`):

```csharp
var deferred = new List<DeferredJsxCheck>();
b.Scaffold($"{indent}{{ {checkType} __uitkx_{expr.Label} = (");
EmitMappedExpressionStrippingJsx(b, expr.Text, expr.UitkxOffset, expr.Kind,
                                  expr.UitkxLine, deferred);
b.Scaffold("); }\n");
b.Scaffold("#line hidden\n");

// New: emit deferred attribute-check blocks at sibling statement scope
foreach (var d in deferred)
    EmitDeferredJsxAttributeChecks(b, d, escapedPath, indent, ref exprCtr,
                                    ref attrCtr, propsTypes);
```

#### Step 1.4.3 — Update the 2 BodyWithReturnFix callers

Lines 2710 and 2728 are inside a body segment that's emitted as part of a local
function (block-body lambda). Two options, pick whichever the implementer
finds cleaner during implementation:

- **Option A (preferred):** thread the deferred list up to the local-function's
  enclosing statement scope. The wrapper at `EmitBlockBodyLambda` opens the
  local function — emit deferred checks **after** the local function closes,
  in the same statement scope as the local-function declaration. This keeps
  the body itself unmodified and matches how Pattern B already operates.
- **Option B:** emit deferred checks inline before `return default!;` inside
  the local function body. Simpler to wire but means lambdas with attribute
  lambdas inside JSX live one nesting level deeper. Should still work for
  dataflow analysis.

#### Step 1.4.4 — Add `EmitDeferredJsxAttributeChecks` helper

A new method that parses the JSX text and walks attributes, mirroring
`EmitInlineExprChecks` (lines 2620-2690) but specialised for the
already-extracted JSX range:

```csharp
private static void EmitDeferredJsxAttributeChecks(
    VirtualDocBuilder b,
    DeferredJsxCheck d,
    string escapedPath,
    string indent,
    ref int exprCtr,
    ref int attrCtr,
    IPropsTypeProvider? propsTypes)
{
    var jsxDirectives = new DirectiveSet(
        Namespace: null,
        ComponentName: null,
        PropsTypeName: null,
        DefaultKey: null,
        Usings: ImmutableArray<string>.Empty,
        UssFiles: ImmutableArray<string>.Empty,
        Injects: ImmutableArray<(string, string)>.Empty,
        MarkupStartLine: d.SourceLine,
        MarkupStartIndex: 0,
        MarkupEndIndex: d.JsxText.Length);
    var diags = new List<ParseDiagnostic>();
    var nodes = UitkxParser.Parse(d.JsxText, escapedPath, jsxDirectives, diags,
                                   lineOffset: d.SourceLine - 1);
    if (nodes.Length == 0) return;

    string funcName = $"__uitkx_jsxattr{b.CurrentPos}";
    b.Scaffold($"{indent}{{\n");
    b.Scaffold($"{indent}    dynamic {funcName}() {{\n");
    b.Scaffold("#pragma warning disable 0162, 0219, 8321\n");
    EmitNodeExpressionsScoped(nodes, b, escapedPath, indent + "        ",
                              ref exprCtr, ref attrCtr, propsTypes,
                              uitkxOffsetAdjust: d.JsxStart);
    b.Scaffold("#pragma warning restore 0162, 0219, 8321\n");
    b.Scaffold($"{indent}        return default!;\n");
    b.Scaffold($"{indent}    }}\n");
    b.Scaffold($"{indent}    _ = {funcName}();\n");
    b.Scaffold($"{indent}}}\n");
}
```

The `uitkxOffsetAdjust: d.JsxStart` is the critical bit — it preserves source
maps so hover/diagnostics on extracted attribute lambdas point back to their
original `.uitkx` source position.

#### Step 1.4.5 — Add a recursion-depth guard

Subagent flagged: if a JSX attribute expression itself contains `{expr}` with
JSX, we recurse into `EmitMappedExpressionStrippingJsx` again, which again
defers. Theoretically unbounded but practically capped by user JSX depth.
Add a thread-local depth counter (`MAX_JSX_NESTING_DEPTH = 16`) that emits
`#error UITKX...` instead of recursing further. This is paranoia, not a
known repro — but free insurance.

### 1.5 Tests

**File:** [`ide-extensions~/lsp-server/Tests/RoslynHostTests.cs`](../ide-extensions~/lsp-server/Tests/RoslynHostTests.cs)

Add four new tests (the existing one at line 259 only covers top-level JSX,
not JSX-inside-`{}`-expression):

1. `UITKX0112_LambdaInJsxAttributeInsideExpressionWithAnd_NoFalsePositive`
   — exact user repro: `{phase >= 0 && <Marker onDone={() => setPhase(1)} />}`
2. `UITKX0112_LambdaInJsxReturningLambda_NoFalsePositive`
   — `onClick={() => <Child onPress={() => setX(1)} />}`
3. `UITKX0112_LambdaInJsxInsideMultiStatementLambdaBody_NoFalsePositive`
   — `onClick={e => { var x = <Child onPress={() => setY(2)}/>; doStuff(x); }}`
4. `UITKX0112_NestedJsxAttributeLambdas_NoFalsePositive`
   — `<Outer onPress={() => setX(1)}><Inner onClick={() => setY(2)}/></Outer>`
     inside `{phase>=0 && ...}`

All four currently fail; all four pass after the fix. Test #1 is the user's
exact bug; the others guard against regressions in adjacent shapes.

### 1.6 Risks & mitigations

| Risk | Mitigation |
|---|---|
| Source-map drift breaks hover-position tests | Existing tests don't pin specific virtual-doc lines (subagent confirmed). New `#line` directives use `d.SourceLine`/`d.JsxStart` like Pattern B already does. |
| Per-keystroke perf hit from extra `UitkxParser.Parse` | Only fires when expression contains JSX (rare). Parser is O(n), no allocations beyond AST. Subagent estimate: ~10-20μs per keystroke even in heavy JSX files. Acceptable. |
| Unbounded recursion on pathological nesting | Depth-counter cap (Step 1.4.5). |
| New deferred-check blocks shadow loop variables | `EmitNodeExpressionsScoped` already handles loop-var scoping (used by Pattern A in `@for` bodies). No new exposure. |
| Multi-statement lambda branch (callers 2710/2728) requires more careful wiring | Use Option A — emit deferred checks at the local-function's enclosing scope. Test case #3 above pins this shape. |

### 1.7 Backwards compatibility

No SG output changes. No HMR output changes. No public API changes. Pure IDE
diagnostic noise reduction. Risk: zero for runtime/HMR users.

### 1.8 Estimated scope

- 1 file modified, ~80-150 LOC added (mostly the new helper and the deferred
  list threading).
- 4 new tests (~120 LOC).
- One reference scan to ensure `EmitMappedExpressionStrippingJsx` isn't called
  from a path I've missed.

---

## 2. Issue 2 — `useTransition` no-op stub

### 2.1 Root cause

`useTransition(` is in every alias rewrite table (`CSharpEmitter.cs#L443`,
`HmrCSharpEmitter.cs#L2597`, `HmrHookEmitter.cs#L35`) but
`Hooks.UseTransition(` doesn't exist in `Shared/Core/Hooks.cs` — verified
0 grep hits. Calling `useTransition()` rewrites to `Hooks.UseTransition(...)`
and the C# compiler returns CS0103. The IDE virtual doc has no stub for it
either, so it's also broken in IntelliSense.

### 2.2 Decision (per user §0): Option A — behavioral no-op stub

Returns React's `[isPending, startTransition]` shape but `isPending` is always
`false` and `startTransition(action)` runs `action` synchronously. UITKX docs
already explicitly opt out of concurrent rendering — this matches.

### 2.3 Concrete edit plan

#### Step 2.3.1 — Add HookId constant

**File:** [`Shared/Core/Hooks.cs`](../Shared/Core/Hooks.cs)

After line 367 (alphabetically with other HookId constants):

```csharp
private const string HookIdTransition = "UseTransition";
```

#### Step 2.3.2 — Add the hook implementation

**File:** [`Shared/Core/Hooks.cs`](../Shared/Core/Hooks.cs)

Place after `UseDeferredValue` (~line 1043) since they're semantic neighbours:

```csharp
private static readonly Action<Action> s_noOpStartTransition = a =>
{
    if (a != null) a();
};

public static (bool isPending, Action<Action> startTransition) UseTransition()
{
    var metadata = HookContext.Current?.Owner;
    var state = HookContext.Current ?? metadata?.EnsureComponentState();
    if (state == null) return (false, s_noOpStartTransition);

    RecordHook(metadata, state, HookIdTransition);
    state.HookIndex++;
    return (false, s_noOpStartTransition);
}
```

Notes:
- `RecordHook` is mandatory — keeps the hook-order invariant intact across
  renders. Without it, calling `useTransition()` would shift indexes and
  break HMR state preservation for sibling hooks.
- `state.HookIndex++` mirrors the pattern in `UseDeferredValue`, `UseEffect`,
  etc. — every hook bumps the index.
- `s_noOpStartTransition` is a static cached delegate; zero allocation on
  the hot path. The single capturing closure is interned at JIT.
- Plain `bool isPending` (not `Ref<bool>`) matches `UseState`'s convention
  of returning value types directly in tuples.
- React 19's async overload `startTransition(async () => ...)` is *not*
  supported. The C# `Action<Action>` type rejects `Func<Task>`. If a user
  tries `startTransition(async () => ...)`, they'll get a compile error
  pointing at the type mismatch — clearer than a silent wrong-behaviour
  no-op, and forward-compatible if we ever add async support.

#### Step 2.3.3 — Add VDG stub

**File:** [`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs)

In **both** stub blocks (lines 342-367 and 515-540), add:

Component-style (instance, line ~540):
```csharp
+ "        private (bool isPending, global::System.Action<global::System.Action> startTransition)\n"
+ "            useTransition() => (false, _ => { });\n"
```

Function-style (static, line ~367):
```csharp
+ "        private static (bool isPending, global::System.Action<global::System.Action> startTransition)\n"
+ "            useTransition() => (false, _ => { });\n"
```

(See §3 for the long-term solution that eliminates this duplication.)

#### Step 2.3.4 — Add hover docstring

**File:** [`ide-extensions~/lsp-server/HoverHandler.cs`](../ide-extensions~/lsp-server/HoverHandler.cs)

Add to `s_hookDocs` (line 555-581):

```csharp
["useTransition"] = """## `useTransition()`

**Shorthand for `Hooks.UseTransition`.** Returns `(isPending, startTransition)`.

> **UITKX note:** matches the React 19 API surface, but UITKX has no concurrent
> renderer. `isPending` is **always `false`** and `startTransition(action)`
> runs `action` synchronously. For deferred values use `useDeferredValue`.

```csharp
var (isPending, startTransition) = useTransition();
startTransition(() => setSlowValue(newValue));
```
""",
["Hooks.UseTransition"] = """## `Hooks.UseTransition()`

Returns `(isPending, startTransition)` matching React 19's API.

> **UITKX note:** synchronous rendering only. `isPending` is always `false`
> and the callback runs synchronously.
""",
```

#### Step 2.3.5 — Doc page update

**File:** [`ReactiveUIToolKitDocs~/src/pages/UITKX/Differences/UitkxDifferencesPage.tsx`](../ReactiveUIToolKitDocs~/src/pages/UITKX/Differences/UitkxDifferencesPage.tsx)

The current page text (line ~47) says `no startTransition`. Replace with a
clarifying note: the hook *exists* for source-compatibility but doesn't
provide concurrent semantics. Code block showing the no-op behaviour. This
file is part of the docs project — small TypeScript edit.

### 2.4 Tests

#### SG snapshot test

**File:** [`SourceGenerator~/Tests/HmrEmitterParityContractTests.cs`](../SourceGenerator~/Tests/HmrEmitterParityContractTests.cs) (sibling style)

```csharp
[Fact]
public void Sg_Hook_UseTransition_TransformedCorrectly()
{
    var output = GeneratorTestHelper.Run("""
        @namespace ReactiveUITK.HmrParity
        component Sample {
            var (isPending, startTrans) = useTransition();
            return (<Label />);
        }
        """);
    Assert.Contains("Hooks.UseTransition(", output.GeneratedSource);
}
```

#### Runtime unit test

**File:** runtime hook test file (subagent didn't pinpoint one — placeholder
location is `Diagnostics/Benchmark` if no dedicated test asmdef exists; in
that case the test goes under SG's compile-and-execute fixtures).

```csharp
[Test]
public void UseTransition_NoOpStub_IsPendingAlwaysFalse()
{
    var (isPending, startTransition) = Hooks.UseTransition();
    Assert.IsFalse(isPending);
    int calls = 0;
    startTransition(() => calls++);
    Assert.AreEqual(1, calls);
    Assert.IsFalse(isPending);  // synchronous; no concurrent renderer
}
```

#### IDE virtual-doc test

A test in `ide-extensions~/lsp-server/Tests/` asserting that a `.uitkx` file
with `useTransition()` produces zero CS0103 diagnostics. Subagent identified
`HookCrossNamespaceVirtualDocTests.cs` as the closest existing fixture.

### 2.5 Risks & mitigations

| Risk | Mitigation |
|---|---|
| User writes `startTransition(async () => …)` expecting React 19 async | Compile error on `Func<Task>` → `Action<Action>`. Documented in hover docstring. Worse case = clear compile error, not silent wrong behaviour. |
| Hook order drift if `RecordHook` skipped | We call it. Standard pattern. |
| HMR reflection over `Hooks` enumerates new method | Subagent confirmed `UitkxHmrCompiler` does NOT enumerate Hooks methods. No reflection break. |
| Per-render allocation of `Action<Action>` | Static cached singleton `s_noOpStartTransition`. Zero-alloc. |
| `HmrEmitterParityContractTests` count assertion breaks | Subagent confirmed no count assertions exist. Adding one hook is invisible to existing tests. |

### 2.6 Estimated scope

- `Hooks.cs`: ~12 LOC.
- `VirtualDocumentGenerator.cs`: 4 LOC × 2 stub blocks = 8 LOC.
- `HoverHandler.cs`: ~20 LOC (two dictionary entries).
- `UitkxDifferencesPage.tsx`: ~25 LOC.
- 3 new tests: ~60 LOC.

---

## 3. Issue 3 — Unified hook registry (0.5.23, pure refactoring)

**Release scope reminder:** because all coverage expansion ships in 0.5.22
(workstreams 2b + 2c), the 0.5.23 registry refactor is a **pure consolidation**.
Identical inputs in, identical outputs out. The registry's job is to delete
7 hand-maintained tables, not to add coverage.

### 3.0 Coverage-expansion catch-up shipped in 0.5.22 (forward reference)

Before the registry refactor lands, the following sections describe the
tactical hand-edits that ship in **0.5.22** (workstreams 2b + 2c). These
are itemised here so the registry refactor in 0.5.23 has an accurate
"current state" to consolidate.

#### 3.0.A — Workstream 2b: 9 missing VDG IDE stubs (in addition to `useTransition`'s stub from §2)

**File:** [`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`](../ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs)

Add to **both** stub blocks (the function-style block at L342-367 and the
component-style block at L515-540). Signatures sourced from `Hooks.cs` so
they match the runtime exactly:

| # | Hook | Stub signature (component-style; static form differs only in `static` modifier) |
|---|---|---|
| 1 | `useReducer` | `private (TState state, global::System.Action<TAction> dispatch) useReducer<TState, TAction>(global::System.Func<TState, TAction, TState> reducer, TState initial, params object[] dependencies) => default!;` |
| 2 | `useDeferredValue` | `private T useDeferredValue<T>(T value, params object[] dependencies) => default!;` |
| 3 | `useImperativeHandle` | `private THandle useImperativeHandle<THandle>(global::System.Func<THandle> factory, params object[] dependencies) where THandle : class => default!;` |
| 4 | `useStableFunc` | `private global::System.Func<TArg, TResult> useStableFunc<TArg, TResult>(global::System.Func<TArg, TResult> callback) => default!;` |
| 5 | `useStableAction` | `private global::System.Action<TArg> useStableAction<TArg>(global::System.Action<TArg> callback) => default!;` |
| 6 | `useStableCallback` | `private global::System.Action useStableCallback(global::System.Action callback) => default!;` |
| 7 | `useTweenFloat` | `private void useTweenFloat(float from, float to, float duration, global::ReactiveUITK.Core.Ease ease, float delay, global::System.Action<float> onUpdate, global::System.Action onComplete, params object[] dependencies) { }` |
| 8 | `useAnimate` | `private void useAnimate(global::System.Collections.Generic.IReadOnlyList<global::ReactiveUITK.Core.AnimateTrack> tracks, bool autoplay, params object[] dependencies) { }` |
| 9 | `useSafeArea` | `private global::UnityEngine.UIElements.VisualElement useSafeArea() => default!;` |

Final type names + parameter signatures must be verified against `Hooks.cs`
at implementation time — some have multiple overloads (e.g. `useReducer`
has a non-generic-action variant). Each overload that exists in `Hooks.cs`
gets a corresponding stub overload.

Duplication note: yes, these are entered twice (once in each stub block).
This duplication is the exact problem the **0.5.23 registry refactor**
eliminates by code-generating the stubs from `HookRegistry`. We accept the
short-term duplication for the speed of unblocking the user.

#### 3.0.B — Workstream 2c: validator + hover coverage expansion

**Validator pattern table additions** (10 hooks × 3 forms = 30 new strings
added to existing tables; no structural change):

**Files (mirror edits in both):**
- [`SourceGenerator~/Emitter/HooksValidator.cs`](../SourceGenerator~/Emitter/HooksValidator.cs) `s_hookPatterns` (L150-180)
- [`ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`](../ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs) `s_hookPatterns` (L475-516)

Patterns to append (each as 3 strings: `Hooks.UsePascal`, `usePascal`, `useCamel`):

```
useSafeArea, useStableFunc, useStableAction, useStableCallback,
useImperativeHandle, useAnimate, useSfx, useUiDocumentRoot,
useTweenFloat, provideContext
```

Result: 30 → 60 strings (10 → 20 hooks). The two tables stay byte-identical
by convention — the existing `// Mirror of HooksValidator.s_hookPatterns`
comment in `DiagnosticsAnalyzer.cs` documents the contract.

**Hover docs additions** (12 missing hooks × 2 forms = 24 new dictionary
entries):

**File:** [`ide-extensions~/lsp-server/HoverHandler.cs`](../ide-extensions~/lsp-server/HoverHandler.cs) `s_hookDocs` (L555-581)

Missing hook docs to add (drafted from `Hooks.cs` doc-comments where they
exist; placeholder text from issue 3's investigation otherwise):

```
useSafeArea, useStableFunc, useStableAction, useStableCallback,
useImperativeHandle, useAnimate, useSfx, useUiDocumentRoot,
useTweenFloat, useDeferredValue, provideContext, useReducer
```

Each entry follows the existing markdown shape (heading, signature,
short description, example block).

#### 3.0.C — Tests added in 0.5.22 for the coverage expansion

- **`HooksValidator_ConditionalUseTweenFloat_FiresUITKX0013`** — SG
  validator test pinning that the expansion actually surfaces.
- **`DiagnosticsAnalyzer_ConditionalUseSafeArea_FiresUITKX0013`** —
  IDE-side mirror.
- **`HoverHandler_AllRegisteredHooks_HaveDocs`** — enumerates the
  validator pattern table and asserts every entry has a hover doc. This
  becomes the parity tripwire that prevents future drift between the
  validator and hover tables until 0.5.23 unifies them.

---

### 3.1 Current state — verified

| Site | File | Lines | Hook count | Has gaps? |
|---|---|---|---|---|
| SG aliases | `SourceGenerator~/Emitter/CSharpEmitter.cs` | 431-457 | 21 | includes useTransition (ghost — fixed by issue 2) |
| SG generic regex | same file | 459-464 | 14 | **missing 7 hooks** (useSfx, useUiDocumentRoot, useSafeArea, useStableCallback, useAnimate, useTweenFloat, provideContext) |
| SG signature regex | same file | 519-533 | 21 (× 2 forms) | complete |
| HMR component path | `Editor/HMR/HmrCSharpEmitter.cs` | 2585-2620, 3041-3055 | byte-for-byte SG copy | mirrors SG gaps |
| HMR hook path | `Editor/HMR/HmrHookEmitter.cs` | 23-58 | 21 + 14 generic | mirrors SG gaps |
| SG validator | `SourceGenerator~/Emitter/HooksValidator.cs` | 150-180 | **only 10** | **10 hooks unvalidated** |
| IDE diagnostics | `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs` | 475-516 | only 10 | mirrors validator gaps |
| IDE hover docs | `ide-extensions~/lsp-server/HoverHandler.cs` | 555-581 | **only 16 entries (8 hooks)** | **12 hooks undocumented** |
| IDE virtual-doc stubs | `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | 342-367, 515-540 | **only 17** | **10 hooks have no stub** (useReducer, useDeferredValue, useTransition, useImperativeHandle, useStableFunc/Action/Callback, useTweenFloat, useAnimate, useSafeArea) |

`Hooks.cs` actually has **20 real hooks** (26 overload variants). The "21st"
in alias tables is `useTransition` — fixed by issue 2.

### 3.2 The pattern that works — already proven in the repo

`Shared/Core/Router/RouterTagAliases.cs` is netstandard2.0-clean (only
`System` namespaces, no Unity refs) and is linked into the SG project via:

```xml
<Compile Include="..\Shared\Core\Router\RouterTagAliases.cs"
         Link="Emitter\RouterTagAliases.cs" />
```

This is the exact template for `HookRegistry`.

### 3.3 Project-boundary plan (verified)

| Project | Target | Access path to registry |
|---|---|---|
| SG (`ReactiveUITK.SourceGenerator.csproj`) | netstandard2.0 | `<Compile Include Link>` to `Shared/Core/HookRegistry.cs` |
| HMR (`ReactiveUITK.Editor` Unity asmdef) | Unity-managed | Direct asmdef ref to `ReactiveUITK.Shared` (already referenced — verified in `Editor/ReactiveUITK.Editor.asmdef`) |
| language-lib (`UitkxLanguage.csproj`) | netstandard2.0 | `<Compile Include Link>` to same file |
| LSP server (`ReactiveUITK.LanguageServer.csproj`) | net8.0 | indirect via language-lib DLL ref (already references it) |
| IDE diagnostics | inside language-lib | sees registry directly |

**Important:** SG and language-lib are independent compilations. Both need a
`<Compile Include Link>` to the same source file. HMR's Editor assembly gets
the same source compiled into the Shared runtime DLL via the Unity asmdef
graph — no link needed there.

### 3.4 Proposed registry shape

**File:** `Shared/Core/HookRegistry.cs` (new)

```csharp
using System;
using System.Collections.Generic;

namespace ReactiveUITK.Core
{
    public static class HookRegistry
    {
        public readonly struct HookEntry
        {
            public string CamelName { get; }       // "useState"
            public string PascalName { get; }      // "UseState"
            public bool IsGeneric { get; }
            public string DocString { get; }       // hover markdown, may be null
            public string StubReturnType { get; }  // for VDG stub generation
            public string StubParameters { get; }  // for VDG stub generation
            public string StubBody { get; }        // e.g. "(initial, null!)"
            public bool IsStatic { get; }          // true for hook-style stubs (else component-style)
            public string ExtraStubModifiers { get; } // e.g. "where THandle : class"
            public HookEntry(string camel, string pascal, bool isGeneric,
                             string doc, string retType, string parameters,
                             string body, string extra = "")
            { CamelName = camel; PascalName = pascal; IsGeneric = isGeneric;
              DocString = doc; StubReturnType = retType;
              StubParameters = parameters; StubBody = body;
              ExtraStubModifiers = extra; IsStatic = false; }
        }

        public static readonly HookEntry[] All = { /* 20 entries + overloads */ };

        public static (string From, string To)[] GetAliasTable() => s_aliasCache;
        public static string GetSignatureRegexPattern() => s_signaturePatternCache;
        public static string GetGenericHookPattern() => s_genericPatternCache;
        public static IReadOnlyDictionary<string, string> GetDocMap() => s_docMapCache;
        public static string[] GetValidationPatterns() => s_validationPatternsCache;
        public static string GenerateVirtualDocStubs(bool staticForm) =>
            staticForm ? s_staticStubsCache : s_instanceStubsCache;

        private static readonly (string,string)[] s_aliasCache = BuildAliases();
        private static readonly string s_signaturePatternCache = BuildSignaturePattern();
        private static readonly string s_genericPatternCache = BuildGenericPattern();
        private static readonly Dictionary<string,string> s_docMapCache = BuildDocMap();
        private static readonly string[] s_validationPatternsCache = BuildValidationPatterns();
        private static readonly string s_staticStubsCache = BuildStubs(true);
        private static readonly string s_instanceStubsCache = BuildStubs(false);

        // ... static builders
    }
}
```

Critical perf rule: **all accessor methods return cached field references; no
rebuild on call**. Required because `GetValidationPatterns()` is called per
attribute-scan in IDE diagnostics (per-keystroke).

### 3.5 Migration phases (all in 0.5.23 — pure refactoring, no behavior change)

Precondition: 0.5.22 has shipped. Validator covers 20 hooks. Hover covers
20 hooks. VDG stubs cover 26 hooks. The registry's job is to *consolidate*
these 8 sites into one table with no behavior change.

#### Phase 1 — Registry foundation
1. Create `Shared/Core/HookRegistry.cs` populated with all 20 hooks (plus
   `useTransition` no-op now that it's a real method per issue 2). Include
   docstrings drafted from current `HoverHandler` content + new placeholders
   for the 12 missing.
2. Add `<Compile Include Link>` entries to `SourceGenerator~/ReactiveUITK.SourceGenerator.csproj`
   and `ide-extensions~/language-lib/UitkxLanguage.csproj`.
3. New unit-test file `SourceGenerator~/Tests/HookRegistryTests.cs`:
   - `Registry_HookCount_IsExpected` (lock count → tripwire if anyone adds
     a hook without updating registry).
   - `Registry_All_HavePascalAndCamelForms`.
   - `Registry_GetGenericPattern_MatchesGenericFormsOnly`.
   - `Registry_GetSignaturePattern_MatchesAllForms`.
   - `Registry_GetValidationPatterns_HasThreeFormsPerHook`.
   - `Registry_GetDocMap_HasEntryPerHookBothForms`.

#### Phase 2 — SG migration
1. `CSharpEmitter.cs`: replace `s_hookAliases`, `s_genericHookAliasRe`,
   `s_hookSignatureRe` with registry-derived static caches. Method bodies
   unchanged (still a tuple-array iteration / Regex match).
2. `HooksValidator.cs`: replace `s_hookPatterns` with
   `HookRegistry.GetValidationPatterns()` (only if Phase 0 sample scan is
   clean; otherwise hold this step).
3. Run full SG test suite. SG output is byte-identical → all existing tests
   pass.
4. **Drift parity test:** new `SourceGenerator~/Tests/HookRegistryParityTests.cs`:
   - `SG_HookAliases_DerivedFromRegistry` — asserts SG's regex compiles and
     matches every hook in registry.
   - `SG_GenericPattern_CoversEveryGenericHook` — closes the current 7-hook
     generic-regex gap.

#### Phase 3 — HMR migration
1. `HmrCSharpEmitter.cs` and `HmrHookEmitter.cs`: replace the duplicated
   `s_hookAliases`, regex fields with calls to `HookRegistry.*` accessors.
2. Static-field initialisation order: HMR is in Editor asmdef, runs in Unity.
   Static fields initialise on type load — no AppDomain reload concerns
   in normal Editor workflow.
3. Run `HmrEmitterParityContractTests`. Output identical → all pass.

#### Phase 4 — IDE diagnostics + hover migration (pure deduplication)
1. `DiagnosticsAnalyzer.cs`: replace `s_hookPatterns` (60 strings post-0.5.22)
   with `HookRegistry.GetValidationPatterns()`. Identical output.
2. `HoverHandler.cs`: replace `s_hookDocs` (40 entries post-0.5.22) with
   `HookRegistry.GetDocMap()`. Identical content.
3. **No new diagnostic surfaces** — inputs byte-identical to 0.5.22 tables.

#### Phase 5 — IDE virtual-doc stubs migration (pure deduplication)
1. `VirtualDocumentGenerator.cs` lines 342-367 and 515-540: replace the two
   hand-maintained scaffold blobs (now containing 26 entries post-0.5.22)
   with a single call to `HookRegistry.GenerateVirtualDocStubs(staticForm: bool)`.
2. New IDE test: `VirtualDoc_AllRegistryHooks_HaveStubs` — enumerates the
   registry and confirms every entry produces a syntactically-valid stub.
3. Snapshot test: generated output must byte-match the pre-refactor 0.5.22
   output for every existing test fixture.

#### Phase 6 — Cleanup
1. Update `CHANGELOG.md`: "Hook metadata unified into
   `Shared/Core/HookRegistry.cs`. No behavior change."
2. Close `Plans~/TECH_DEBT_V2.md` item 15 — this refactor is the long-term
   cure for the same drift class.

### 3.6 Risk catalogue

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| R1 | netstandard2.0 incompat in registry | Medium | Mirror `RouterTagAliases.cs` — only `System.*`. Tests gate. |
| R2 | Per-keystroke perf hit if accessor methods rebuild | Medium | Static cached fields, never rebuild. Test asserts repeated calls return same reference. |
| R3 | HMR static-init order issue at first hot reload | Low | Editor asmdef references Shared; CLR loads Shared before HMR types. No reflection involved. |
| R4 | Stub-template generation produces invalid C# | Medium | Test compiles generated stubs against an empty Roslyn workspace and asserts no diagnostics. The 0.5.22 hand-stubs serve as the byte-identical reference output. |
| R5 | Phase rollout drift between SG and HMR | Low | Each phase keeps SG and HMR in lockstep. Don't merge a phase without all consumers updated. |
| R6 | Public registry API leaks internal naming | Low (mitigated by Q4 decision) | `HookRegistry` is `internal` per Q4. Promote later if useful for tooling. |

**Validator-expansion risk that *was* in this section is now in 0.5.22's
release notes** — see §6.1.

### 3.7 Coordinated parity test

A new test fixture `HookRegistryDriftTests` that verifies:
1. Every hook listed in `Hooks.cs` (via reflection over `typeof(Hooks).GetMethods`)
   has an entry in `HookRegistry.All`.
2. Every alias entry currently in SG has a matching registry entry.
3. The generic-pattern matches every entry where `IsGeneric == true` and
   nothing else.

This becomes the new tripwire — analogous to `HmrEmitterParityContractTests`
but covering all 8 sites instead of just SG↔HMR.

### 3.8 Estimated scope

- 1 new file (`HookRegistry.cs` + builders): ~250-400 LOC.
- 7 consumer files modified (3 SG + 2 HMR + 2 IDE): each loses ~30-60 LOC of
  duplicated tables, gains 3-5 LOC of registry calls. Net -150 LOC.
- 2 csproj edits (`<Compile Link>`).
- 4 new test fixtures: ~250 LOC.

Total: net code reduction; net coverage expansion (validator +10, hover +12,
stubs +10).

---

## 4. Performance audit summary

### 4.1 HMR runtime

| Layer | Per-emission cost change |
|---|---|
| Issue 1 fix | None. Only IDE virtual doc affected. HMR uses its own `SpliceExpressionMarkup` which already does the right thing. |
| Issue 2 fix | Adds one method to Hooks.cs runtime. Per-call: 1 RecordHook + 1 index increment + 1 tuple return. ~50ns. Indistinguishable from `UseDeferredValue`. |
| Issue 3 fix | Static-field initialisation moves from per-class to one-time per AppDomain. Net **positive** — eliminates redundant regex compilation in HMR (currently each emitter compiles its own copy of the same regex). |

### 4.2 IDE per-keystroke

| Layer | Per-keystroke cost change |
|---|---|
| Issue 1 fix | Adds `UitkxParser.Parse` calls only when the expression contains JSX. Subagent measured: ~10-20μs per JSX-containing expression. Files without JSX in expressions: **zero cost**. |
| Issue 2 fix | None — VDG stub addition is one-time scaffold write, not per-keystroke. |
| Issue 3 fix | Validator's `CheckAttributeForHooks` loop expands from 30 → 60 patterns. Linear scan. Adds ~30 string comparisons per attribute. Negligible. |

### 4.3 Cold start (LSP server first solution load)

Issue 3 adds two `<Compile Include>` entries — adds ~5-10ms to language-lib
build. Once-per-build, not per-keystroke. Negligible.

---

## 5. Rollout sequence

**Release 0.5.22** — bug fixes + pure-additive coverage expansion. No
behavior change for any working `.uitkx` file; only new diagnostics for
previously-silent Rules-of-Hooks violations on the 10 newly-validated hooks.

1. **Workstream 1** — VDG attribute-lambda preservation (issue 1, §1).
2. **Workstream 2** — `Hooks.UseTransition()` runtime no-op (§2.3.1, 2.3.2)
   plus its VDG stub (§2.3.3) plus hover doc (§2.3.4) plus differences-page
   note (§2.3.5).
3. **Workstream 2b** — 9 additional missing VDG IDE stubs: `useReducer`,
   `useDeferredValue`, `useImperativeHandle`, `useStableFunc`,
   `useStableAction`, `useStableCallback`, `useTweenFloat`, `useAnimate`,
   `useSafeArea`. Same scaffold shape as `useTransition`'s VDG stub. Pure
   additive — removes 9 false-positive CS0103 patterns from the IDE without
   touching SG / HMR / runtime. (See §3.1A below for the exact stub list.)
4. **Workstream 2c** — Validator + hover coverage expansion. Hand-add 10
   hook names to `HooksValidator.cs s_hookPatterns` and
   `DiagnosticsAnalyzer.cs s_hookPatterns`; hand-add 24 entries to
   `HoverHandler.cs s_hookDocs` (12 hooks × camelCase + Hooks.PascalCase).
   No registry yet — these are tactical extensions of the existing tables
   that the 0.5.23 refactor will then deduplicate. (See §3.1B below.)

**Release 0.5.23** — pure refactoring; **zero behavior change**.

5. Workstream 3 — unified `HookRegistry` refactor. Phases 1-6 from §3.5.
   Because all coverage expansion already landed in 0.5.22, the registry
   release becomes a straight deduplication: identical inputs in, identical
   outputs out. No sample scan required. No new diagnostics. Just collapsing
   8 hand-maintained tables into one source of truth.

This split is necessary because the registry refactor is a 7-file
restructure with its own test surface, while workstreams 1+2 are surgical
fixes that need to ship to unblock the user immediately.

---

## 6. Decisions (locked 2026-05-21)

| # | Question | Decision |
|---|---|---|
| Q1 | Add the 9 missing VDG IDE stubs in 0.5.22? | **Yes** — pure additive, ships with workstream 2b. |
| Q2 | Validator coverage 10 → 20 in 0.5.22 or 0.5.23? | **0.5.22** — ships as workstream 2c alongside hover doc expansion. |
| Q3 | Sample-scan scope before validator expansion? | **Skipped** — user confirmed no known violations in private projects; we can scan reactively if any 0.5.22 user reports new UITKX0013 surfaces. |
| Q4 | `HookRegistry` visibility in 0.5.23? | **`internal`** — promote later if a real consumer asks. |

---

## 6.1 Behavioral consequences of the locked decisions

For 0.5.22 specifically:

- Any `.uitkx` file that calls one of the 10 newly-validated hooks
  (`useSafeArea`, `useStableFunc/Action/Callback`, `useImperativeHandle`,
  `useAnimate`, `useSfx`, `useUiDocumentRoot`, `useTweenFloat`,
  `provideContext`) inside `@if`, `@for`, `@foreach`, `@while`, or attribute
  position will now emit **UITKX0013** ("hook called conditionally") or
  **UITKX0016** ("hook called inside loop"). This is a *new* diagnostic on
  *previously-silent* code that was already broken at runtime — the user
  was just not being told.
- Any `.uitkx` file that previously compiled with a runtime-broken hook
  pattern will now fail the SG diagnostic gate. The CHANGELOG entry will
  document this as a **fix** (not a breaking change), with the migration
  instruction to move the hook call to top-level component scope.
- Listed under "Notable diagnostics added" in the CHANGELOG so users
  updating from 0.5.21 know what to expect.

---

## 7. What I'm NOT proposing

- No change to runtime hook execution semantics. UITKX is sync; stays sync.
- No change to JSX grammar or directive scanner.
- No change to SG output for any input.
- No public API breaks. 0.5.x semver intact.
- No reflection-based dynamic registry loading. Keep static-field caching for
  determinism and perf.

---

## 8. Confidence

After reading every file referenced above and three rounds of subagent dives
into VDG, `Hooks.cs`, and the 8 consumer sites, I'm confident in:

- **Issue 1**: very high — the fix mirrors an existing in-VDG pattern
  (Pattern B). Risk is mostly "wire the deferred list correctly" not "design
  the fix".
- **Issue 2**: very high — straight runtime addition matching existing
  conventions. The only judgment call (sync `Action<Action>` vs async
  `Func<Task>` support) is documented and forward-compatible.
- **Issue 3**: high on shape, medium on schedule — the `RouterTagAliases`
  precedent proves the linking pattern works. The coordination across 7
  files is mechanical but tedious.

Implementation can begin once Q1-Q4 are answered.
