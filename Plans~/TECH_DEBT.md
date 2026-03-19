# Tech Debt — Unified Tracker

> Consolidated from `tech-debt.md`, `TECH_DEBT_COMPLETION_CONTEXT.md`, and
> `TECH_DEBT_DIAGNOSTICS_AND_FORMATTER.md` on 2026-03-19.

---

## Source Generator — CRITICAL

### TD-01: Early `return` in component body is ignored
**Priority: Critical**

The source generator always uses the **last** top-level markup block as the Render method's return expression, ignoring any earlier `return` statements. All three cases below silently compile but don't work:

1. `return null;` — should render nothing, but renders the last markup block
2. `return (<></>);` — empty fragment, same problem
3. `return (<Box><Label text="aasd" /></Box>);` — early return with markup, same problem

**Root cause**: The parser/emitter separates top-level markup nodes from code blocks. All `return (markup)` statements inside code blocks are treated as `ReturnMarkup` nodes, but the final top-level markup always becomes the generated `return` expression.

**Affected files**:
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `BuildSource()` always emits last markup as return
- `ide-extensions~/language-lib/Parser/UitkxParser.cs` — how returns are parsed vs top-level markup

---

## IDE Extensions — CRITICAL

### TD-02: Go-To-Definition navigates to `dist~/` instead of source files
**Priority: Critical**

Ctrl+clicking a component reference navigates to the copy inside `dist~/Samples~/` instead of the actual source file under `Samples/UITKX/Components/`.

**Expected**: Go-To-Definition should resolve to the source `.uitkx` file, never to files inside `dist~/` or other tilde-suffixed folders.

**Affected files**:
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — likely indexes `dist~/` files alongside source files
- `ide-extensions~/vscode/` and `ide-extensions~/visual-studio/` — definition providers

---

## LSP Server — Completion Context

### TD-03: `@code` completion leaks into non-header contexts
**Priority: Medium**

`@code` completion has repeatedly leaked into non-header contexts due to multiple overlapping completion paths (cursor-kind routing, context stack inference, and schema-driven item sources).

**Current mitigation**: A final post-filter in the LSP completion handler removes `@code` from results whenever the cursor is not in the strict header zone.

**Follow-up**:
- Consolidate all completion context gating into one authoritative function
- Add explicit completion snapshot tests for: header, after `@code`, first markup lines, and embedded markup inside `@code`
- Remove the safety filter after canonical gating + tests are stable

### TD-04: Hover shows 30+ inherited props (noise)
**Priority: Low**

Hovering over `<Button>` shows `Text` from `ButtonProps` plus all 30+ inherited properties from `BaseProps`.

**Current mitigation**: `HoverHandler` now renders only the element's own props and shows a count (e.g. `+ 34 inherited from BaseProps`). Completions still use the full resolved list.

---

## Runtime — Dead Code

### TD-05: Dead `memoize` / `memoCompare` fields
**Priority: Medium**

`VirtualNode.Memoize` and `VirtualNode.TypedMemoCompare` are stored but never read by the reconciler. The `memoize` and `memoCompare` parameters on `V.Func(...)` are no-ops — all components already bail out unconditionally via `IProps.Equals`.

**Fix**: Remove all `memoize` / `memoCompare` parameters and fields. Search call sites for `memoize: true` in Samples before deleting.

### TD-06: `SyntheticEventDemoFunc` uses `extraProps` unnecessarily
**Priority: Low**

`SyntheticEventDemoFunc.uitkx` passes pointer/wheel handlers via the `extraProps` escape hatch. These events are already hardcoded in `PropsApplier.ApplyEvent` and typed on `BaseProps` — they should be plain JSX attributes.

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
**Priority: High**

**Files affected**: `ListViewStatefulDemoFunc.uitkx`, `UitkxCounterFunc.uitkx`, and likely others.

The formatter produces incorrect output when components get misaligned. The JSX placeholder splice-back mechanism (`EmitSetupCodeWithJsx`) can misalign surrounding C# code, cascading errors.

**Fix**: Fix affected files to canonical format, write regression tests in `FormatterSnapshotTests.cs`, then fix the formatter's JSX-in-setup-code handling.

### TD-08: Zero regression tests for UITKX0107 (unreachable-after-return)
**Priority: High**

We spent v1.0.251–254 fixing unreachable-after-return dimming through 4 separate mechanisms but there are **zero automated tests** for any of it. Any future change could silently break dimming.

**What's needed**: Create `UnreachableDiagnosticTests.cs` with tests for:
- Single-line `return expr;` at depth 0 and depth > 0
- Multi-line `return (\n<Box/>\n);` at depth > 0 (lambda)
- Early return in function-style setup code → dims remaining setup + render root
- Nested lambda return → dims within lambda, not outside
- No false positives on clean files
- Site A: code after render return is dimmed (excluding `}`)

**Architecture reference (v1.0.254)**:
- **CheckUnreachableAfterReturn** (Site C/D unified): brace-depth + paren-depth tracking
- **Site B** (WalkNodeList sibling check): `HasTopLevelReturn(cb)` → dims subsequent siblings
- **Site A** (function-style post-render): dims from `FunctionReturnEndLine+1` to `FunctionBodyEndLine-1`
- **Render-return wrapper**: dims `return (` line wrapping render root
- CS0162 suppressed globally — UITKX0107 handles all dimming

### TD-09: Paren-depth counter doesn't skip strings/chars
**Priority: Low**

**Files**: `VirtualDocumentGenerator.cs` (~line 1121), `DirectiveParser.cs` (`FindJsxBlockRanges`)

If C# code contains `(` or `)` inside a string literal like `"hello (world)"` or char `'('`, the paren-depth counter miscounts. Risk is low but technically wrong.

**Fix**: Add string/char literal skipping in paren-counting loops (similar to existing comment skipping).

### TD-10: Four pre-existing formatter idempotency failures
**Priority: Medium**

**Failing files**:
- `Components/ShowcaseDemoPage/components/ShowcaseDemoPage.uitkx` (both plain + Roslyn)
- `Shared/MultiColumnTreeViewStatefulDemoFunc.uitkx` (both plain + Roslyn)

`Format(content) != content` — the formatter changes these files even though they should be canonical. Likely related to TD-07.

---

## Environment — Noise

### TD-11: Burst AOT error: `Failed to resolve assembly: Assembly-CSharp-Editor`
**Priority: Low**

Burst logs `Mono.Cecil.AssemblyResolutionException` on every domain reload. ReactiveUITK has no `[BurstCompile]` methods — purely a false positive. Red console noise.

**Fix**: In **Edit > Project Settings > Burst AOT Settings**, add `Assembly-CSharp-Editor` to the exclusion list or restrict the scan to an explicit allowlist.
