# Tech Debt: Diagnostics & Formatter (v1.0.254)

## 1. Formatter Squiggly Errors

**Files affected**: `ListViewStatefulDemoFunc.uitkx`, `UitkxCounterFunc.uitkx`, and likely others.

**Symptom**: When a component (or the file itself) gets misaligned, the formatter produces
incorrect output, causing red squiggly error lines where there shouldn't be any. Fixing
one component can cause errors to appear in others — "fix one leak and the water goes
somewhere else."

**Root cause**: The `AstFormatter` processes setup code containing embedded JSX (like
`return <Label .../>` or `? <Label .../> : <Comp />` inside lambdas). When indentation
gets off, the JSX placeholder splice-back mechanism (`EmitSetupCodeWithJsx`) can misalign
the surrounding C# code, cascading errors downstream.

**What's needed**:
- Fix affected files to canonical format manually
- Write regression tests (inline `[Fact]` methods in `FormatterSnapshotTests.cs`) that
  pin the exact formatted output
- Then investigate + fix the formatter's JSX-in-setup-code handling

---

## 2. UITKX0107 Regression Tests

**Symptom**: We spent v1.0.251–254 fixing unreachable-after-return dimming through 4
separate mechanisms (CheckUnreachableAfterReturn, Site B sibling dimming, Site A
post-render, render-return wrapper), but there are **zero automated tests** for any of it.

**Risk**: Any future change to the parser, lowering, or diagnostics could silently break
dimming again.

**What's needed**: Create a new test class (e.g. `UnreachableDiagnosticTests.cs`) with
tests for:
- Single-line `return expr;` at depth 0 and depth > 0
- Multi-line `return (\n<Box/>\n);` at depth > 0 (lambda)
- Early return in function-style setup code → dims remaining setup + render root
- Nested lambda return → dims within lambda, not outside
- No false positives on clean files (no early return)
- Site A: code after render return is dimmed (excluding `}`)
- Each test: parse → lower → `DiagnosticsAnalyzer.Analyze()` → assert `UITKX0107`
  diagnostic with correct `SourceLine`/`EndLine`

**Architecture reference (v1.0.254)**:
- **CheckUnreachableAfterReturn** (Site C/D unified): scans CodeBlockNode.Code line-by-line
  with brace-depth + paren-depth tracking. Handles single-line `return expr;` and multi-line
  `return (\n<JSX/>\n);`. Uses `isFunctionStyle` for correct line offset (no +1 for synthetic
  @code blocks).
- **Site B** (WalkNodeList sibling check): `HasTopLevelReturn(cb)` → dims all subsequent AST
  siblings. Now fires for function-style (decoupled from `skipReturnCheck`).
- **Site A** (function-style post-render): dims from `FunctionReturnEndLine+1` to
  `FunctionBodyEndLine-1` (excludes closing `}`).
- **Render-return wrapper**: dims the `return (` line wrapping the render root when setup code
  has early return (uses `d.MarkupStartLine`).
- CS0162 suppressed globally in `RoslynDiagnosticMapper.s_suppressedIds` — UITKX0107 handles
  all dimming.
- CS0162 merging code + `FindScopeEnd` removed from DiagnosticsPublisher.
- `isFunctionStyle` vs `skipReturnCheck` are separate params through WalkNodeList → WalkNode →
  CheckUnreachableAfterReturn.

---

## 3. Paren-Depth Counter Doesn't Skip Strings/Chars

**Files**: `VirtualDocumentGenerator.cs` (~line 1121, `EmitFunctionStyleSetupSegmented`),
`DirectiveParser.cs` (`FindJsxBlockRanges`)

**Symptom**: If C# code contains `(` or `)` inside a string literal like `"hello (world)"`
or a char literal `'('`, the paren-depth counter miscounts, potentially causing the JSX
scanner to start/stop at wrong positions.

**Risk**: Low — string literals containing unbalanced parens in setup code near JSX
expressions are rare. But it's technically wrong.

**What's needed**: Add string/char literal skipping in the paren-counting loops (similar to
the `/* */` and `//` comment skipping already added in v1.0.217).

---

## 4. Four Pre-Existing Formatter Idempotency Failures

**Failing files**:
- `Components/ShowcaseDemoPage/components/ShowcaseDemoPage.uitkx` (both plain + Roslyn)
- `Shared/MultiColumnTreeViewStatefulDemoFunc.uitkx` (both plain + Roslyn)

**Symptom**: `Format(content) != content` — the formatter changes these files even though
they're supposed to be canonical. This means format-on-save would cause a "save loop".

**Root cause**: Likely complex nested JSX-in-C# patterns (ternaries, lambdas, deeply nested
setup code) where the formatter's indent logic doesn't converge. Possibly related to issue #1.

**What's needed**: Investigate what the formatter changes in each file, fix the formatter or
fix the files to canonical form, and verify `Assert.Equal(content, Format(content))` passes.
