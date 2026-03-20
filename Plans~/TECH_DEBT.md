# Tech Debt — Unified Tracker

> Consolidated from `tech-debt.md`, `TECH_DEBT_COMPLETION_CONTEXT.md`, and
> `TECH_DEBT_DIAGNOSTICS_AND_FORMATTER.md` on 2026-03-19.

---

## ~~Source Generator — CRITICAL~~

### ~~TD-01: Early `return` in component body is ignored~~ — ✅ COMPLETED

The source generator always uses the **last** top-level markup block as the Render method's return expression, ignoring any earlier `return` statements. All three cases below silently compile but don't work:

1. `return null;` — should render nothing, but renders the last markup block
2. `return (<></>);` — empty fragment, same problem
3. `return (<Box><Label text="aasd" /></Box>);` — early return with markup, same problem

**Root cause**: The parser/emitter separates top-level markup nodes from code blocks. All `return (markup)` statements inside code blocks are treated as `ReturnMarkup` nodes, but the final top-level markup always becomes the generated `return` expression.

**Affected files**:
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `BuildSource()` always emits last markup as return
- `ide-extensions~/language-lib/Parser/UitkxParser.cs` — how returns are parsed vs top-level markup

---

## ~~IDE Extensions — CRITICAL~~

### ~~TD-02: Go-To-Definition navigates to `dist~/` instead of source files~~ — ✅ COMPLETED

Ctrl+clicking a component reference navigates to the copy inside `dist~/Samples~/` instead of the actual source file under `Samples/UITKX/Components/`.

**Expected**: Go-To-Definition should resolve to the source `.uitkx` file, never to files inside `dist~/` or other tilde-suffixed folders.

**Affected files**:
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — likely indexes `dist~/` files alongside source files
- `ide-extensions~/vscode/` and `ide-extensions~/visual-studio/` — definition providers

---

## LSP Server — Completion Context

### ~~TD-03: `@code` completion leaks into non-header contexts~~ — ✅ COMPLETED
**Priority: Medium**

`@code` completion is properly gated to directive header context via `inDirectiveHeader` checks in `CompletionHandler.cs`. Post-filter also strips `@code` items outside the header zone.

### ~~TD-04: Hover shows 30+ inherited props (noise)~~ — ✅ COMPLETED
**Priority: Low**

Hovering over `<Button>` shows `Text` from `ButtonProps` plus all 30+ inherited properties from `BaseProps`.

**Current mitigation**: `HoverHandler` now renders only the element's own props and shows a count (e.g. `+ 34 inherited from BaseProps`). Completions still use the full resolved list.

---

## Runtime — Dead Code

### TD-05: Dead `memoize` / `memoCompare` fields
**Priority: Low**

**Update 2026-03-20:** Thorough investigation confirms these ARE dead code. `VNode.Memoize` and `VNode.TypedMemoCompare` are written in constructors and copy-constructors but **never read** by the reconciler. The actual bailout in `FiberFunctionComponent.cs` uses `IProps.Equals()` and `ReferenceEquals` — it never checks `Memoize` or `TypedMemoCompare`. `FiberNode` doesn't even store these properties.

**Fix**: Remove `Memoize` and `TypedMemoCompare` from `VNode`, remove `memoize`/`memoCompare` constructor parameters, and remove the corresponding parameters on `V.Func(...)`. Search for `memoize: true` in Samples first to clean up call sites.

### TD-06: `SyntheticEventDemoFunc` uses `extraProps` unnecessarily
**Priority: Low**

`SyntheticEventDemoFunc.uitkx` passes pointer/wheel handlers via the `extraProps` escape hatch. These events are already typed on `BaseProps` — they should be plain JSX attributes. The `extraProps` mechanism itself remains in the framework for genuinely untyped passthrough — this is only about this one sample file using it when it doesn't need to.

**Fix**: Replace `extraProps` block with direct JSX attributes:
```uitkx
<VisualElement
    onPointerDown={(Action<SyntheticPointerEvent>)(e => UpdateLog("PointerDown", e))}
    ...
>
```

---

## Diagnostics & Formatter

### TD-07: Formatter squiggly errors in complex files
**Priority: High** — **Root cause: TD-09**

**Files affected**: `ListViewStatefulDemoFunc.uitkx`, `UitkxCounterFunc.uitkx`, and likely others.

The formatter produces incorrect output when components get misaligned. The JSX placeholder splice-back mechanism (`EmitSetupCodeWithJsx`) can misalign surrounding C# code, cascading errors. Root cause is TD-09 — the naive paren-depth counter in `ScanJsxParenBlocks` miscounts when strings contain `(` or `)`, causing placeholder boundaries to land at wrong positions.

**Fix**: Fix TD-09 first (add string-skipping to paren counters), then verify these files format correctly. If issues persist after TD-09, debug by diffing first-pass vs second-pass formatter output.

### TD-08: Zero regression tests for UITKX0107 (unreachable-after-return)
**Priority: High**

We spent v1.0.251–254 fixing unreachable-after-return dimming through 4 separate mechanisms but there are **zero automated tests** for any of it. Any future change could silently break dimming.

**Approach**: Add `UnreachableDiagnosticTests.cs` in `SourceGenerator~/Tests/`. Same pattern as existing `DiagnosticTests.cs` — parse a `.uitkx` string through `DiagnosticsAnalyzer`, collect emitted diagnostics, assert UITKX0107 at expected spans.

**Test cases needed**:
- Single-line `return expr;` followed by markup → UITKX0107 on trailing markup
- Multi-line `return (\n<Box/>\n);` at depth > 0 (lambda) → dims correctly
- Early return in function-style setup code → dims remaining setup + render root
- Nested lambda return → dims within lambda, not outside
- No false positives on clean files (negative case)
- Site A: code after render return is dimmed (excluding `}`)

**Architecture reference (v1.0.254)**:
- **CheckUnreachableAfterReturn** (Site C/D unified): brace-depth + paren-depth tracking
- **Site B** (WalkNodeList sibling check): `HasTopLevelReturn(cb)` → dims subsequent siblings
- **Site A** (function-style post-render): dims from `FunctionReturnEndLine+1` to `FunctionBodyEndLine-1`
- **Render-return wrapper**: dims `return (` line wrapping render root
- CS0162 suppressed globally — UITKX0107 handles all dimming

### TD-09: Paren-depth counter doesn't skip strings/chars
**Priority: Medium** (upgraded — root cause of TD-07 and TD-10)

**Files**:
- `AstFormatter.cs` → `ScanJsxParenBlocks` (~line 2045)
- `DirectiveParser.cs` → `FindJsxBlockRanges` (~line 1907)

Both have naive paren-depth loops that don't skip string/char literals. Other methods in the same files (e.g. `FindJsxElementEnd`) DO properly skip strings using `TrySkipStringOrCharLiteral()` — the omission is inconsistent.

**Example that breaks**: `var node = (<Box onToggle={e => UpdateLog("(test)")} />);` — the `(` inside the string miscounts paren depth, causing the JSX block boundary to land at the wrong position.

**Fix**: Add `TrySkipStringOrCharLiteral()` call at the top of each paren-counting loop:
```csharp
while (j < code.Length && depth > 0)
{
    if (TrySkipStringOrCharLiteral(code, ref j)) continue; // ← ADD
    if (code[j] == '(') depth++;
    else if (code[j] == ')') depth--;
    j++;
}
```
The helper already exists and handles `"..."`, `@"..."`, `$"..."`, `"""..."""`, and `'x'`.

**Fixes TD-07 and likely TD-10.**

### TD-10: Four pre-existing formatter idempotency failures
**Priority: Medium** — **Likely caused by TD-09**

**Failing files**:
- `Components/ShowcaseDemoPage/components/ShowcaseDemoPage.uitkx` (both plain + Roslyn)
- `Shared/MultiColumnTreeViewStatefulDemoFunc.uitkx` (both plain + Roslyn)

`Format(content) != content` — the formatter changes these files even though they should be canonical. The paren-depth bug (TD-09) causes JSX block boundaries to differ between passes, producing different output each time.

**Fix**: Fix TD-09 first, then re-run `FormatterSnapshotTests.Idempotency_SampleFile_IsUnchanged()`. If failures persist, diff first-pass vs second-pass output character by character to find where they diverge.

---

## Environment — Noise

### TD-11: Burst AOT error: `Failed to resolve assembly: Assembly-CSharp-Editor`
**Priority: Low** — documentation only

Burst logs `Mono.Cecil.AssemblyResolutionException` on every domain reload. ReactiveUITK has no `[BurstCompile]` methods — purely a false positive. Red console noise.

**Fix**: Add to docs troubleshooting section: "If you see `Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor` in the console, this is a Burst false positive. Go to Edit > Project Settings > Burst AOT Settings and exclude `Assembly-CSharp-Editor`."

---

## IDE Extensions — IntelliSense (carried from archived plans)

### TD-12: `.Current` / `.Value` — no semantic colour on `useRef` variables
**Priority: Medium**
**Carried from:** intellisense-bugs-v2.md (N-1)

Inside `RouterHooks.UseBlocker(…)`, the variable `allowNextRef` (declared by `useRef<bool>()`) shows no semantic colouring for `.Current` / `.Value` members. After v1.0.148 the issue persists.

**Investigation 2026-03-20:** Virtual document stubs are correct — `__UitkxRef__<T>` class has `Current` and `Value` properties, and `useRef<T>()` returns it. Likely a timing race or Roslyn type resolution edge case, not a stub emission bug.

**Fix plan:**
1. Add `ServerLog` tracing to `CompletionHandler` to confirm workspace rebuild completes before `GetCompletionsAsync`
2. Check generated virtual document — confirm `allowNextRef` has type `__UitkxRef__<bool>`
3. If type is correct, likely a race condition — add a gate/await before completion request

### TD-13: Zero IntelliSense integration test coverage
**Priority: Medium**
**Carried from:** intellisense-plan.md (T-10)

No automated tests exist for LSP completion / signature-help scenarios.

**Approach**: Create test project under `ide-extensions~/lsp-server/Tests/`. Use OmniSharp's `LanguageServer.From()` test mode — spin up server in-process with `PipeReader/PipeWriter`, send/receive JSON-RPC messages directly. Each test ~20 lines. Same harness also covers:
- `textDocument/semanticTokens/full` → verify token types/colors
- `textDocument/publishDiagnostics` → verify codes, severities, spans
- `textDocument/formatting` → verify formatted output
- Diagnostics stability under editing (send `didChange` with partial content, verify recovery)

**Key completion scenarios**:
- `Ctrl+Space` on blank line in setup code → C# scope symbols
- `from.` in setup code → member completions
- `.` on markup line → no C# popup
- `StyleKeys.` inside `style={…}` → member completions
- `onClick={` → C# scope completions
- `(` after method call → signature help
- Hover over expression in `style={expr}` → type tooltip
