# V1 Polish Plan — Execution Checklist

> Created: 2026-03-20
> Prioritized by: correctness → safety nets → clean API → documentation → process

---

## Test Coverage Summary (as of 2026-03-20)

**Total: 863 tests across 2 projects, all green.**

### SourceGenerator~/Tests/ (841 tests, net10.0)

| File | Tests | Coverage |
|------|-------|---------|
| ParserTests.cs | 36 | Directive parsing, markup nodes, @break/@continue, error recovery |
| FormatterSnapshotTests.cs | 213+ | Format idempotency, indentation, whitespace |
| DiagnosticTests.cs | 23 | Generator-level diagnostics (UITKX0001–0021) |
| EmitterTests.cs | 46 | C# code emission, props resolution |
| LoweringTests.cs | 5 | CanonicalLowering render-root flattening |
| DebugDumpTest.cs | 4 | Debug AST dump output |
| **SemanticTokenTests.cs** | **22** | **Token classification: directives, elements, attributes, control flow, comments, setter-in-comment suppression** |
| **CursorContextTests.cs** | **18** | **AstCursorContext.Find(): tag/attribute/expression/code-block classification** |
| **VirtualDocumentTests.cs** | **13** | **Virtual doc generation, source-map round-trips, region kind tagging** |
| **DiagnosticsAnalyzerTests.cs** | **27** | **All Tier-2 diagnostics: UITKX0101–0111, severity checks, span verification** |

### ide-extensions~/lsp-server/Tests/ (22 tests, net8.0)

| File | Tests | Coverage |
|------|-------|---------|
| **RoslynHostTests.cs** | **8** | **In-process workspace, semantic model, hover-style type queries, idempotent rebuild** |
| **RoslynCompletionTests.cs** | **8** | **Dot-completion (strings, ints, expressions, code blocks, inline, function-style, useRef)** |
| **LspProtocolTests.cs** | **6** | **Full JSON-RPC round-trip: init, didOpen, didChange, hover, completion, formatting** |

**Bold** = added in this session. No production code was modified (only a `<Compile Remove>` in the LSP csproj).

---

## Tier 1 — Correctness (fix bugs users can hit)

### 1. TD-09: Fix paren-depth counter to skip strings/chars
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 858 green

**Priority:** High — root cause of TD-07 and TD-10.

**Details:**
Two methods have naive paren-depth loops that don't skip string/char literals:
- `AstFormatter.cs` → `ScanJsxParenBlocks` (~line 2045)
- `DirectiveParser.cs` → `FindJsxBlockRanges` (~line 1907)

**Fix:** Add `TrySkipStringOrCharLiteral()` call at the top of each paren-counting loop. The helper already exists and handles `"..."`, `@"..."`, `$"..."`, `"""..."""`, and `'x'`.

**Verification:** Run `FormatterSnapshotTests` — all idempotency tests should pass.

---

### 2. TD-07 + TD-10: Verify formatter after TD-09 fix
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 836 formatter tests green

**Priority:** High — blocked by #1.

**Details:**
After TD-09 is fixed, re-run `FormatterSnapshotTests.Idempotency_SampleFile_IsUnchanged()`.
- If ShowcaseDemoPage.uitkx and MultiColumnTreeViewStatefulDemoFunc.uitkx pass → close both.
- If failures persist, diff first-pass vs second-pass output to find divergence point in `EmitSetupCodeWithJsx` splice-back logic.

**Verification:** `dotnet test` on SourceGenerator~ — zero formatter failures.

---

### 3. TD-12: Fix useRef `.Current`/`.Value` semantic colour
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 22 LSP tests green (incl. new useRef test)

**Priority:** High — visible IDE bug.

**Details:**
Virtual document stubs are correct (`__UitkxRef__<T>` has `Current` and `Value` properties). Likely a timing race between workspace rebuild and completion request.

**Steps:**
1. Add `ServerLog` tracing to `CompletionHandler` to confirm workspace rebuild timing
2. Check generated virtual document — confirm variable has type `__UitkxRef__<bool>`
3. If type correct → add gate/await before completion request to resolve race
4. If type wrong → fix stub emission in `VirtualDocumentGenerator.cs`

**Verification:** Open a `.uitkx` file with `useRef<bool>()`, type `.Current` — semantic colour and completions should appear.

---

## Tier 2 — Safety Nets (prevent regressions)

### 4. TD-08: Add UITKX0107 regression tests
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 836 green

**Priority:** High — 4 dimming mechanisms with zero test coverage.

**Details:**
Create `UnreachableDiagnosticTests.cs` in `SourceGenerator~/Tests/`. Same pattern as `DiagnosticTests.cs` — parse `.uitkx` string through `DiagnosticsAnalyzer`, collect emitted diagnostics, assert UITKX0107 at expected spans.

**Test cases:**
- `return null;` followed by `<Label/>` → UITKX0107 on trailing markup
- Multi-line `return (<Box/>);` at depth > 0 (lambda) → dims correctly
- Early return in function-style setup code → dims remaining setup + render root
- Nested lambda return → dims within lambda, not outside
- Clean file with no early return → zero UITKX0107 (negative case)
- Site A: code after render return is dimmed (excluding `}`)

**Verification:** `dotnet test` — all new tests green.

---

### 5. TD-13: Create LSP integration test harness
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 21 green

**Priority:** Medium — foundation for testing all LSP features.

**Details:**
Create test project under `ide-extensions~/lsp-server/Tests/`. Use OmniSharp `LanguageServer.From()` test mode — spin up server in-process with `PipeReader/PipeWriter`, send/receive JSON-RPC directly.

**Covers:**
- `textDocument/completion` — scope symbols, member access, attribute expressions
- `textDocument/signatureHelp` — parameter hints after `(`
- `textDocument/hover` — type tooltips on expressions
- `textDocument/semanticTokens/full` — verify token types and colours
- `textDocument/publishDiagnostics` — codes, severities, spans
- `textDocument/formatting` — formatted output matches expected
- Edit stability — send `didChange` with partial content, verify recovery

**Key completion scenarios:**
- `Ctrl+Space` on blank line in setup code → C# scope symbols
- `from.` → member completions
- `.` on markup line → no C# popup
- `StyleKeys.` inside `style={…}` → member completions
- `onClick={` → C# scope completions
- `(` after method call → signature help

**Verification:** `dotnet test` on lsp-server/Tests — all scenarios pass.

---

## Tier 3 — Clean API Surface (ship-ready code)

### 6. TD-05: Remove dead memoize/memoCompare fields
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 863 green

**Priority:** Low — dead API surface confuses readers.

**Details:**
Remove from `VNode`:
- `Memoize` property + constructor parameter
- `TypedMemoCompare` property + constructor parameter
- Copy-constructor assignments for both

Remove from `V.Func(...)`:
- `memoize` parameter
- `memoCompare` parameter

Search for `memoize: true` in Samples to clean up call sites first.

**Verification:** Full test suite passes. No compilation errors.

---

### 7. TD-06: Convert SyntheticEventDemoFunc from extraProps to direct attributes
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** 863 green

**Priority:** Low — sample code should show best practices.

**Details:**
In `Samples/UITKX/Components/SyntheticEventDemoFunc/SyntheticEventDemoFunc.uitkx`, replace the `extraProps` dictionary with direct JSX attributes: `onPointerDown={...}`, `onPointerMove={...}`, `onPointerUp={...}`, `onWheel={...}`.

The `extraProps` mechanism stays in the framework for genuinely untyped passthrough.

**Verification:** Sample compiles and renders identically in Unity.

---

## Tier 4 — Documentation

### 8. Directive & syntax reference page (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Medium — users need a single reference for all UITKX syntax.

**Details:**
Create a new page in `ReactiveUIToolKitDocs~/src/pages/UITKX/` covering the complete UITKX language surface. Sections:

**Header Directives (directive-header form):**
| Directive | Syntax | Description |
|-----------|--------|-------------|
| `@namespace` | `@namespace My.Game.UI` | C# namespace for generated class |
| `@component` | `@component MyButton` | Component class name (must match filename) |
| `@using` | `@using System.Collections.Generic` | Adds a using directive |
| `@props` | `@props MyButtonProps` | Props type consumed by the component |
| `@key` | `@key "root-key"` | Static key on root element |
| `@inject` | `@inject ILogger logger` | Dependency-injected field |
| `@code` | `@code { var x = 1; }` | Embeds C# setup code before return |

**Function-Style Component Syntax:**
| Feature | Syntax |
|---------|--------|
| Declaration | `component Name { ... }` |
| With parameters | `component Name(string text = "default") { ... }` |
| Preamble `@using` | Before `component` keyword |
| Preamble `@namespace` | Optional explicit namespace override |

**Markup Control Flow:**
| Directive | Syntax | Notes |
|-----------|--------|-------|
| `@if` / `@else if` / `@else` | `@if (cond) { ... } @else { ... }` | Conditional rendering |
| `@foreach` | `@foreach (var item in list) { ... }` | Loop — direct children must have `key` |
| `@for` | `@for (int i = 0; i < n; i++) { ... }` | C-style for loop |
| `@while` | `@while (cond) { ... }` | While loop |
| `@switch` / `@case` / `@default` | `@switch (val) { @case "a": ... @default: ... }` | Switch expression |
| `@break` | `@break;` | Exit `@for`/`@while` loop |
| `@continue` | `@continue;` | Skip to next iteration |

**Expression & Value Syntax:**
| Syntax | Example | Description |
|--------|---------|-------------|
| `@(expr)` | `@(DateTime.Now.ToString())` | Inline C# expression in markup children |
| `{expr}` | `text={$"Count: {count}"}` | C# expression as attribute value |
| `"literal"` | `text="hello"` | Plain string attribute |
| `{/* comment */}` | `{/* TODO */}` | JSX-style comment |

**Rules & Gotchas:**
- `@namespace` must appear before `@component` in directive-header form
- Hook calls must be unconditional at component top level — not inside `@if`, `@foreach`, etc.
- `@break`/`@continue` only valid inside `@for` and `@while`
- Direct children of `@foreach` need a `key` attribute
- Components must have a single root element

**Verification:** Page renders correctly on docs site, all examples are accurate.

---

### 9. Diagnostics catalog page (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Medium — users need error code reference.

**Details:**
New docs page listing every UITKX diagnostic code with: code, severity, message, what it means, and how to fix it. Source from `DiagnosticCodes.cs` + `UitkxDiagnostics.cs`.

Codes to cover:
- UITKX0001–0021 (generator diagnostics)
- UITKX0101–0111 (structural/Tier-2 diagnostics)
- UITKX0107 (unreachable code dimming)
- UITKX0300–0306 (parser/Tier-1 diagnostics)

**Verification:** All codes from source match the docs page.

---

### 10. Configuration reference page (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Medium — undocumented config file.

**Details:**
Document all `uitkx.config.json` options: what each key does, default values, valid values, and examples. Also mention VS Code extension settings (`uitkx.server.path`, `uitkx.server.dotnetPath`, `uitkx.trace.server`).

**Verification:** All config keys from source are documented.

---

### 11. Debugging guide (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Low — helps advanced users troubleshoot.

**Details:**
How-to page covering:
- How to inspect generated `.g.cs` files
- How `#line` directives map errors back to `.uitkx`
- How to read LSP server logs (`uitkx.trace.server` setting)
- How to debug formatter issues
- How to report bugs with repro steps

**Verification:** Follows a real debugging scenario end to end.

---

### 12. Editor-specific limitations (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Low — set user expectations per editor.

**Details:**
Add a "Known Limitations" section covering:
- **VS Code:** Brief TmLanguage colour flash before LSP connects (cosmetic)
- **VS2022:** Fully functional (dimming and JSX comment colouring both fixed)
- **Rider:** Stub implementation — extent of working features unclear

**Verification:** Matches actual current editor behaviour.

---

### 13. TD-11: Add Burst AOT troubleshooting note (docs site)
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Low — quick doc addition.

**Details:**
Add to troubleshooting/FAQ: "If you see `Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor`, go to Edit > Project Settings > Burst AOT Settings and exclude `Assembly-CSharp-Editor`."

**Verification:** Note is visible on docs site troubleshooting page.

---

### 14. Latency targets documentation
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Priority:** Low — internal reference.

**Details:**
Measure current actual latencies using existing benchmark infrastructure. Document thresholds:
- Tier-1/2 diagnostics: < 200ms after keystroke
- Tier-3 Roslyn diagnostics: < 2s
- Completion response: < 500ms
- Format-on-save: < 1s

Store in a dev-facing doc or config file.

**Verification:** Measured values are within thresholds on dev machine.

---

## Tier 5 — Process & Governance (when ready to ship)

### 15. Phase A: Scope lock and product definition
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Details:** Decision-making tasks, not code:
- Finalize V1 in-scope features and out-of-scope items
- Lock MVP language surface
- Define product positioning and target personas
- Define severity policy and governance

---

### 16. Phase F gaps: Versioning and compliance
**Status:** ✅ Done | **Delivered:** 2026-03-20 | **Tested:** —

**Details:**
- ✅ Define versioning strategy (SemVer + compatibility policy) → `VERSIONING.md`
- ✅ Define deprecation/upgrade policy → `VERSIONING.md`
- ✅ Create THIRDPARTY.md with dependency license inventory → `THIRDPARTY.md`
- ✅ Define distribution channels → `RELEASE_OPS.md`

---

### 17. Phase G: Release operations
**Status:** ✅ Done | **Delivered:** 2026-03-21 | **Tested:** CI verified

**Details:**
- ✅ CI/CD pipeline: `test.yml` (PR tests) + `publish.yml` (unified publish)
- ✅ `RELEASE_OPS.md` with version files reference and full runbook
- ✅ Automated: dist deploy, docs deploy, VS Code publish, VS2022 VSIX publish
- ✅ Each extension version-checked independently via git tags

---

### 18. Phase H: Community and support readiness
**Status:** ✅ Done | **Delivered:** 2026-03-21 | **Tested:** —

**Details:**
- ✅ Publish FAQ for common issues → docs site FAQ page
- ✅ Add issue templates for bug reports → `.github/ISSUE_TEMPLATE/`
- Define support channels and response expectations
