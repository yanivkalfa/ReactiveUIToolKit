# ReactiveUIToolKit — Final Audit v3 (Tooling, Markup/Syntax, LSP/IntelliSense, Formatter, HMR, Core)

**Date:** 2026-07-06 (v3 — third pass added Rename/Completion/DiagnosticMapper/validators/emitter-escaping/runtime-host coverage; new findings U-37…U-41) · **Auditor:** Claude (read-only research; the ONLY working-tree changes made: deleted stray `Cargo.toml` (was staged, zero bytes) and `tscn_stable.html` (1.45 MB saved copy of the Godot-docs "TSCN file format" page accidentally committed in a496377 — it alone was the ~20% "HTML" in GitHub's language bar; commit the deletion and the language stats normalize).**

**v3 coverage note (what the third pass audited and found CLEAN — do not re-audit):** `CSharpEmitter` string escaping (`EscStr` handles `\ " \n \r \t`), `Hooks.DepsChanged` (Equals semantics correct), `Runtime/Core/RootRenderer` (UUM-127851 editor-poll gates intact, compiled out of players), `RenderScheduler`, `HooksValidator` structure (it is the U-10 twin — fix covers it), `HoverHandler`/`DefinitionHandler`/`SignatureHelpHandler` (no hidden sync I/O beyond the shared read-through-store pattern), CompletionHandler's directory enumeration (user-triggered asset-path completion — acceptable cadence).

**Scope:** language-lib (parser, formatter, diagnostics, IntelliSense, semantic tokens, VDG/SourceMap), LSP server, SourceGenerator, IDE extensions, **Editor/HMR (fully audited in v2)**, Shared core.
**Method:** full-file reads plus an **empirical probe suite** (scratch console app referencing `ide-extensions~/language-lib/UitkxLanguage.csproj`; probe IDs P1–P16 = first pass, V1–V5 = validation pass). Every finding marked **CONFIRMED** reproduced against the working tree on 2026-07-06. Everything else is code-reading analysis with the failure scenario stated.

---

## HOW TO USE THIS DOCUMENT (read first, executor)

- Findings are `U-##` (tooling), `H-##` (HMR), `C-##` (core). Each has: severity, exact anchors, a failure scenario, and a **FIX RECIPE** — numbered steps naming the file, method, and the change. Follow the recipe steps in order; where a recipe says "add a test", add it in the named test file BEFORE making the change (red → green).
- **Never patch a symptom** (e.g. special-casing one probe input). Every recipe targets the root cause; if while fixing you find the recipe fights the code, stop and re-read the finding's "why" paragraph.
- After ANY change under `ide-extensions~/language-lib/` or `SourceGenerator~/`: run `dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj` AND `dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj`, then rebuild the committed analyzer DLLs with `scripts/build-generator.ps1` and commit them together with the source change.
- The HMR emitter (`Editor/HMR/HmrCSharpEmitter.cs`) mirrors the SG emitter (`SourceGenerator~/Emitter/CSharpEmitter.cs`). Contract tests (`SourceGenerator~/Tests/Hmr*ContractTests.cs`) guard the pairing — when a recipe touches one emitter it names the mirror step. Do not skip it.
- Versioning: patch bump per batch; CHANGELOG.md entry per batch (see VERSIONING.md). Do not commit/push without the user's go.

### Probe harness (rebuild it to verify your fixes)
Create `probe/probe.csproj` anywhere OUTSIDE the repo:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><ProjectReference Include="<repo>/ide-extensions~/language-lib/UitkxLanguage.csproj" /></ItemGroup>
</Project>
```
and a `Program.cs` that calls `DirectiveParser.Parse` + `UitkxParser.Parse` + `new AstFormatter(FormatterOptions.Default).Format(src, "Probe.uitkx")` on inline strings. Every CONFIRMED finding below quotes the exact input string that reproduces it — paste it, run, verify red; fix; verify green.

---

## 0. Executive summary (v2)

Engine core: strong, tested; only optimization nits (C-01…C-06). Risk concentrates in tooling:

1. **Five confirmed formatter data-loss/corruption bugs** (U-01…U-04 + **new U-36**, the splice-index desync, confirmed by probe V1 — it *deletes real markup and splices the wrong block in its place*). Formatter fixes are the top priority.
2. **The six drifted mini-lexers** (U-20) remain the highest-leverage refactor; U-04/U-07/U-36 and the `$@"` drift are all copy-drift casualties.
3. **HMR (new in v2):** the compile path **ignores parse diagnostics entirely** (H-01) — a save with a syntax error hot-swaps code emitted from an error-recovered AST; the SG path gates correctly, HMR doesn't. Plus a synchronous whole-queue drain that freezes the editor on shared-USS edits (H-02) and a 4-way asset-path resolution inconsistency (H-03).
4. Grammar asymmetries confirmed: Allman `@else` (U-05), `&&`-JSX setup gap (U-06 — fix surface is FOUR splice sites incl. HMR), literal `@` in text (U-08), hook-rules false positives now CONFIRMED for comments and `_useState` (U-10, probes V3/V4).
5. **Refactoring tools (new in v3):** F2-rename is unsafe in three ways — it never renames the declaring file so every component rename lands in a UITKX0103 error state (U-37), hook renames miss cross-directory call sites (U-38) — and the editor's diagnostic mapper over-suppresses `CS1503`, hiding real type errors in `Func<`-taking APIs (U-39).

---

## 1. P0 — Formatter data loss / corruption (ALL CONFIRMED by probe)

### U-01 — Leading comments before `component` silently deleted  *(CONFIRMED, P1)*
- **Anchors:** `ide-extensions~/language-lib/Formatter/AstFormatter.cs` → `FormatFunctionStyleComponent()` (rebuilds output solely from `DirectiveSet` + AST); `Parser/DirectiveParser.cs` → `SkipLeadingFunctionStyleTrivia()` (consumes comments, records nothing).
- **Repro:** `"// My license header\ncomponent Foo {\n    return (\n        <Label text=\"hi\" />\n    );\n}\n"` → format → header gone, zero diagnostics. Same for comments between `@namespace`/`@using`/`@uss` lines and HTML comments in the preamble.
- **FIX RECIPE:**
  1. In `ParseResult.cs`, add to `DirectiveSet` a new init-only member `public ImmutableArray<(string Text, bool IsBlock, int Line)> LeadingTrivia { get; init; }` defaulting to empty. (DirectiveSet is a record with named ctor args used in ~8 construction sites — add it as an `init` property, NOT a positional parameter, so no call site changes.)
  2. In `DirectiveParser`, change `SkipLeadingFunctionStyleTrivia` to an overload that appends each consumed `//`, `/* */`, and `<!-- -->` comment (raw text incl. delimiters, plus a flag for a following blank line) to a `List<(string,bool,int)>` when the caller passes one. The preamble loop in `TryParseFunctionStyle` passes the list; other callers (`LooksLikeFunctionStyleComponent`) pass null. Attach the list to every `DirectiveSet` built by `TryParseFunctionStyle` (all return paths — there are 4).
  3. In `AstFormatter.FormatFunctionStyleComponent` (and `FormatHookModuleFile`), before emitting the preamble, re-emit `directives.LeadingTrivia` verbatim, one per line, preserving blank-line separators.
  4. Trivia interleaved BETWEEN preamble directives: capture ordering by storing trivia with a "before directive index" tag OR (simpler, acceptable) emit all leading trivia first, then the directives — document the normalization in the CHANGELOG.
  5. Tests (`SourceGenerator~/Tests/FormatterSnapshotTests.cs`): (a) `//` header above component round-trips; (b) `/* block */` header; (c) comment between two `@using` lines survives (possibly reordered per step 4); (d) idempotency for each.

### U-02 — `{expr}`/comment children deleted from inline-attribute JSX  *(CONFIRMED, P4)*
- **Anchors:** `AstFormatter.cs:756-777` `SerializeJsxInlineCore` — handles ONLY `ElementNode` and `TextNode` children.
- **Repro:** `<Box header={<Box><Label text="t" />{1 + 1}</Box>} />` → format → `{1 + 1}` deleted.
- **FIX RECIPE:**
  1. In `SerializeJsxInlineCore`, extend the child loop with: `case ExpressionNode en: sb.Append('{').Append(en.Expression).Append('}'); break;` and `case CommentNode cn: sb.Append(cn.IsBlock ? $"/* {cn.Content.Trim()} */" : $"/* {cn.Content.Trim()} */"); break;` (line comments must become block form inside a single-line serialization).
  2. Control-flow nodes (`IfNode` etc.) inside inline-attr JSX cannot be serialized single-line: detect them first (walk children; if any control-flow node present, **return the original source slice instead of serializing** — add an `out bool failed` or return null and let `BuildAttrStrings` fall back to the raw text from the source span; the `AttributeNode` doesn't carry the raw span today, so simplest correct fallback: make `Format()` return the ORIGINAL full source when serialization hits a control-flow child — never drop).
  3. Tests: expr child round-trips; comment child round-trips; `@if` inside inline-attr JSX leaves file unchanged.

### U-03 — Multi-line verbatim string interiors re-indented (string VALUE corrupted)  *(CONFIRMED, P5)*
- **Anchors:** `AstFormatter.cs` `EmitSetupCodeNormalized` (l.1162+) and `EmitCSharpLines` (l.792+) — both process line-by-line: `raw.Trim()` + re-indent + `CollapseIntraLineSpaces`, with no cross-line string state. Also the `{content`-line splitter (l.803-849) and mid-line brace counting (l.1095-1103) are string-blind.
- **Repro:** setup `var s = @"line1\n    }\n  indented content\n";` → interior lines shift +2 spaces on first format (value changed; stable afterwards, so plain idempotency tests never catch it).
- **FIX RECIPE:**
  1. Add to the (new, see U-20) shared lexer a helper `public static bool[] ComputeMultilineStringLineMask(string code)` returning, per line, whether the line STARTS inside an open `@"`/`$@"`/`@$"` (and future raw ```"""```) literal. Implementation: single forward scan with the existing string-skipping state machine, recording line starts.
  2. In `EmitCSharpLines` AND `EmitSetupCodeNormalized`, compute the mask once per call; for any line whose mask bit is true, emit the line **byte-verbatim** (no Trim, no tab expansion, no collapse, no brace counting, no push/pop) and skip all indent bookkeeping for it.
  3. Also exclude masked lines from `baseSpaces` computation and from the `{content`-split preprocessor.
  4. Tests: the P5 input round-trips byte-identical inside the literal; a `}`-leading line inside a verbatim string does not pop the block stack (assert following lines' indent unchanged); `$@"` variant.

### U-04 — `@case global::…` silently corrupts the file on format  *(CONFIRMED, P9)*
- **Anchors:** root cause `Parser/UitkxParser.cs` `ParseSwitch` case-value scan (l.1183-1192) stops at the FIRST `:`; the formatter then re-emits the mangled AST.
- **Repro:** `@case global::System.StringComparison.Ordinal:` → formats to `@case global:` + garbage body. No diagnostics.
- **FIX RECIPE:**
  1. In the case-value scan loop, replace the `_scanner.Current != ':'` condition with logic that consumes `::` as content: when at `:`, peek the next char; if it is also `:`, advance twice and continue; else stop.
  2. Mirror check in the emitters: grep `SourceGenerator~/Emitter/CSharpEmitter.cs` and `Editor/HMR/HmrCSharpEmitter.cs` for how `SwitchCase.ValueExpression` is consumed — no change should be needed once the parser captures the full value, but run the `Hmr*ContractTests` to confirm.
  3. Tests: parser test (`ParserTests.cs`) that `global::Ns.Enum.Val` parses as one case value; formatter snapshot that it round-trips; emitter test that generated `case global::Ns.Enum.Val:` compiles.

### U-36 — **NEW / CONFIRMED (V1)** — setup-code JSX splice-index desync REPLACES one block's markup with another's
- **Anchors:** `AstFormatter.cs` `EmitSetupCodeWithJsx` (l.2128+). It walks blocks found by its OWN detector `ScanJsxParenBlocks` (paren + `<` only) while indexing into `directives.SetupCodeMarkupRanges`, which came from `DirectiveParser.FindJsxBlockRanges` — a DIFFERENT detector that ALSO records `(@if …)`-style directive-paren blocks and `=> <Tag` bare-arrow ranges. Any block only ONE detector sees shifts `origMarkupIdx` for everything after it.
- **Repro (exact):**
  ```
  component Foo {
      var cond = true;
      var x = (@if (cond) { <Label text="a" /> } @else { <Label text="b" /> });
      var y = (
          <Box>
              <Label text="real" />
          </Box>
      );
      return (
          <Box>{y}</Box>
      );
  }
  ```
  Output: `var y = ( @if (cond) { … } @else { … } );` — **`y`'s real `<Box><Label text="real"/></Box>` is DELETED and `x`'s `@if` content spliced in its place.** Non-idempotent (V5: second pass differs again).
- **Why:** `FindJsxBlockRanges` records ranges `[0]=x's @if block, [1]=y's JSX`; `ScanJsxParenBlocks` sees only y's block, maps it to `origMarkupIdx = 0`, and splices `SetupCodeMarkupRanges[0]` (x's content) into y.
- **FIX RECIPE (do not band-aid the index arithmetic):**
  1. Eliminate the dual-detector design: in `EmitSetupCodeWithJsx`, stop re-scanning with `ScanJsxParenBlocks`. Instead iterate `directives.SetupCodeMarkupRanges` + `SetupCodeBareJsxRanges` directly (they carry absolute source offsets); convert to setup-string-relative offsets using `FunctionSetupStartOffset`/gap fields exactly as `CSharpEmitter.AbsToSetupOffset` does (copy that tiny helper into the formatter or move it to the shared lexer).
  2. For each range decide single-line vs multi-line and paren-wrapped vs directive vs bare using the source text at the range (char before `Start` == `(`? first non-ws char == `@`?). Directive-paren blocks (`(@if …)`) must be left VERBATIM by the formatter (they are re-emitted by `EmitCSharpLines` as-is) — i.e., treat them like single-line blocks: skip.
  3. Delete `ScanJsxParenBlocks` from AstFormatter if then unused (grep first — `EmitDirectiveBodySetupCode`/`EmitBodySetupCodeWithJsx` also call it; those operate on normalized strings where ranges aren't available, and their blocks are self-contained — those call sites may keep it, but add a comment explaining why the two detectors are allowed to differ THERE and not in `EmitSetupCodeWithJsx`).
  4. Tests: the V1 repro round-trips with both `x` and `y` intact + idempotent; a `=> <Tag/>` lambda before a paren block; two paren blocks; commented-out JSX before a real block (V2 input — must stay green).

---

## 2. P1 — Grammar / diagnostics correctness

### U-05 — Allman `@else` is a parse error while Allman `@if` works  *(CONFIRMED, P2)*
- **Anchors:** `UitkxParser.cs` `LookAheadIsElse` (l.1453-1501) skips only spaces/tabs after `@else`; consumption path l.915-925.
- **FIX RECIPE:** in `LookAheadIsElse`, extend the whitespace skip after `@else` (the `j` loop) to include `\r`/`\n`; in `ParseIf`'s else-branch consumption, replace `SkipInlineWhitespace` after reading "else" with `SkipWhitespaceAndNewlines` BEFORE `PeekAt('{')` (the `@else if` path already tolerates newlines). Add parser tests: K&R and Allman forms of `@if/@else if/@else`, plus `@else` followed by a blank line then `{`.

### U-06 — `cond && <Tag/>` desugar missing outside `{expr}` — FOUR fix sites
- **Anchors (fix surface, verified in v2):** `SourceGenerator~/Emitter/CSharpEmitter.cs` → `SpliceSetupCodeMarkup` (l.2334) and `SpliceBodyCodeMarkup` (l.2610) splice `&&`-detected JSX ranges verbatim → generated `isOn && V.Badge(...)` = hard CS0019; the desugar exists only in `SpliceExpressionMarkup` (l.2767). Mirrors: `Editor/HMR/HmrCSharpEmitter.cs` → its `SpliceSetupCodeMarkup` (l.572) has the same gap; its `SpliceExpressionMarkup` (l.975) has the desugar.
- **FIX RECIPE:**
  1. Extract the desugar block from `CSharpEmitter.SpliceExpressionMarkup` (the `TryFindTrailingLogicalAnd`/`FindLhsStartForLogicalAnd`/ternary-wrap logic, l.2817-2870 + the closing append l.2939-2943) into a private helper `bool TryEmitLogicalAndDesugar(string text, int prev, int s, int e, StringBuilder spliced, Func<string,string> emitJsx)` so all splicers share one implementation.
  2. Call it from `SpliceSetupCodeMarkup` and `SpliceBodyCodeMarkup` before the plain-prefix append (both loops have the identical `prev/s/e` shape).
  3. Mirror steps 1-2 in `HmrCSharpEmitter` (its `EmitCtx` variants).
  4. Extend `HmrEmitterParityContractTests` with a `var x = isOn && <Badge/>;` setup-code case asserting both emitters produce the ternary; add an `EmitterTests` case asserting the generated C# contains `? ` + `: (global::ReactiveUITK.Core.VirtualNode?)null`.
  5. While there: `FindLhsStartForLogicalAnd` (DirectiveParser l.2508) — add `=` (not `==`,`<=`,`>=`,`!=`,`=>`) as a boundary token so `x = cond && <T/>` computes LHS `cond`, not `x = cond` (U-23 merged here).

### U-07 — `FindJsxBlockRanges` is block-comment-blind → false CS1002 + comment reformatting  *(CONFIRMED, P3b/P3c)*
- **Anchors:** `Parser/DirectiveParser.cs:2154-2245` skips `//` and strings but not `/* */` (sibling `FindBareJsxRanges` l.2299-2319 does skip them).
- **FIX RECIPE:** add before the string-skip in the `FindJsxBlockRanges` loop:
  ```csharp
  if (source[i] == '/' && i + 1 < rangeEnd && source[i + 1] == '*')
  { int e2 = source.IndexOf("*/", i + 2, StringComparison.Ordinal); i = (e2 >= 0 && e2 + 2 <= rangeEnd) ? e2 + 2 : rangeEnd; continue; }
  ```
  Tests: `/* old UI: (<Label/>) was removed */` produces no CS1002 (P3b input); VDG test that commented-out JSX is not stripped; formatter test that comment interiors are not reformatted (P3c input, byte-identical comment).

### U-08 — Literal `@` in text content unwritable  *(CONFIRMED, P12)*
- **FIX RECIPE:** in `UitkxParser.ParseContent`'s `@` branch, after reading the keyword: if the keyword is NOT one of the known directives AND not `else/case/default/break/continue/code`, do not emit UITKX0305 — instead back up and treat the `@…` run as text (append to a TextNode). Implementation: peek the keyword with `PeekDirectiveKeyword()` BEFORE consuming `@`; only enter the directive branch for known keywords; otherwise fall through to `ReadTextContent` (which must then not stop at THIS `@` — simplest: read one char as text and continue). Also fix `ErrUnknownDirective`'s message to drop `code` from the valid list and suggest `{"@"}`. Tests: `<Label>contact me @ home</Label>` parses clean; `@if` still parses; `@bogus (…)` still errors (now only when followed by `(`? — no: keep erroring for `@identifier` followed by `(` or `{` to catch typos like `@foreech`; add that heuristic to the recipe consciously and test `@foreech (x in y) {` still errors).

### U-09 — Duplicate `@namespace` → misleading whole-file UITKX2105  *(CONFIRMED, P13)*
- **FIX RECIPE:** in `TryParseFunctionStyle`'s preamble loop, when `inlineNamespace != null` and `TryReadFunctionStyleNamespaceDirective` would match again (probe with `IsAt("@namespace")` style check before the guard), consume the line and emit a dedicated error `"duplicate @namespace directive — only one is allowed"` at that line, keep the FIRST namespace, and continue parsing normally. Test: file with two `@namespace` lines yields exactly one diagnostic, correct namespace, and the component still parses.

### U-10 — Hook-rules scanning: comment/string/word-boundary FALSE POSITIVES  *(CONFIRMED, V3 + V4)*
- **Repro:** `// TODO: maybe useState(1) here later` inside an `@if` body → **UITKX0013 Error** (V3). `var z = _useState(1);` → same false error (V4).
- **Anchors:** `language-lib/Diagnostics/DiagnosticsAnalyzer.cs` `ScanCodeForHooks` (l.517), `CheckExpressionForHooks` (l.595), `CheckAttributeHooks` (l.639) — raw `IndexOf(pattern)`.
- **FIX RECIPE:**
  1. Write one helper in DiagnosticsAnalyzer (or the U-20 shared lexer): `static int FindHookCall(string code, string pattern, int limit)` that (a) loops `IndexOf` from a moving start; (b) rejects a match whose preceding char is letter/digit/`_`/`.` (the `.` also fixes `obj.useState(` member-call false positives — mirror Godot's `_find_hook_call` semantics in `guitkx.gd:596-609`, which is the reference implementation); (c) skips matches inside comments/strings by pre-scanning with `ReturnFinder.TrySkipNonCodeSpan` (walk from 0, maintaining "in code" spans; a match not inside a code span is rejected).
  2. Replace all three call sites.
  3. Check the SG twin: `SourceGenerator~/Emitter/HooksValidator.cs` uses the same registry patterns — apply the same helper there (move the helper next to `HookRegistry.GetValidationPatterns()` so both consume it).
  4. Tests (`DiagnosticsAnalyzerTests.cs`): V3 input → no UITKX0013; V4 input (`_useState(`) → none; `"useState("` in a string → none; a REAL `useState(0)` in an `@if` body → still errors.

### U-11 — `return someVar;`/unwrapped ternary invisible in control-block bodies
- **FIX RECIPE:** in `UitkxParser.ParseBodyForIde`, when `TryFindTopLevelReturn` fails AND `bodyCode` contains `return ` (cheap IndexOf), emit a new diagnostic (reuse UITKX2102 text: "'return' in a directive body must use 'return (…)', 'return <Tag/>;' or 'return null;'") anchored at the `return` offset (compute line via the line-index from U-17's cache). Document the rule in the docs site language reference. Test: `@if (x) { return items.First(); }` produces the diagnostic instead of silently-empty IDE features.

### U-12 — UITKX0106 severity mismatch (doc says warning, message says "should", code says Error)
- **FIX RECIPE:** change `DiagnosticsAnalyzer.cs:686` severity to `ParseSeverity.Warning`; verify the SG's equivalent (grep `MissingKey` in `SourceGenerator~/` — `StructureValidator.cs`) and align. Update tests asserting Error.

### U-13 — `ErrMismatchedTag` wrong line in message + no columns
- **FIX RECIPE:** give `ErrMismatchedTag` explicit `foundLine`/`openLine` parameters; at `ParseContent:457` pass `(closingLine: _scanner.Line, openLine: /*unknown → omit from message*/)`; at `ParseElement:733` pass both correctly; set `SourceColumn/EndColumn` from the closing tag's span (`PeekClosingTagName` start/len). Cosmetic; do together with U-29/U-30 cleanup.

---

## 3. P1 — HMR (NEW SECTION — fully audited in v2)

*Context: HMR = `Editor/HMR/`. Flow: `UitkxHmrFileWatcher` (50 ms debounce) → `UitkxHmrController.OnUitkxFileChanged` → FIFO queue → `UitkxHmrCompiler.Compile` (reflection-driven parse via the committed language DLL → `HmrCSharpEmitter` → in-process Roslyn → `Assembly.LoadFrom` + `ForceRunModuleInitializers`) → module-static/method swappers → `RefreshRuntime.PerformRefresh`. Overall design quality is high (Family-handle indirection, readonly-stripping strategy, rude-edit auto-reload). The findings:*

### H-01 — **HIGH** — HMR ignores parse diagnostics; emits from an error-recovered AST
- **Anchors:** `UitkxHmrCompiler.Compile` (l.261-475): `diagList` is created (l.272), filled by `DirectiveParser.Parse` + `UitkxParser.Parse`, and **never read**. Compare `SourceGenerator~/UitkxPipeline.cs:82-91 & 194-207`, which converts every `ParseSeverity.Error` into `#line`/`#error` and returns before emitting.
- **Failure:** save a `.uitkx` with a syntax error during an HMR session → either a cryptic csc error wall (pointing at generated code) or — worse — the error-recovered AST emits *valid* C# that hot-swaps WRONG UI with no indication.
- **FIX RECIPE:**
  1. In `Compile`, immediately after the AST parse (l.316) add: iterate `diagList` via the existing reflection helpers (`GetItems` + `GetProp(diag,"Severity"/"Code"/"Message"/"SourceLine")`); if any has `Severity == Error`, set `result.Error = $"[HMR] {Path.GetFileName(uitkxPath)} has {n} parse error(s):\n" + joined "  UITKXxxxx Lxx: message" lines; `return result;`.
  2. Apply the same gate in `CompileHookModuleFile` (l.1024) and `BuildComponentArtifacts` (l.507) — all three create `diagList`s.
  3. Since severities cross the reflection boundary, cache the `ParseSeverity.Error` enum value once in `CacheReflectionHandles` (l.1351) alongside the other handles rather than comparing strings.
  4. Test: `SourceGenerator~/Tests/UitkxHmrSwapWriteAnalyzerTests.cs` is analyzer-side; HMR compiler tests run in-Unity only — so add the gate behind a small static pure helper `internal static string FormatParseErrors(IEnumerable<(string code,int line,string msg)> errs)` and unit-test THAT in the SG test suite; manual verification: HMR window, save a file with `<Box>` unclosed → single readable UITKX0301 error in console, no swap.

### H-02 — **MED** — whole-queue synchronous drain freezes the editor on fan-out
- **Anchors:** `UitkxHmrController.DrainCompileQueueIfIdle` (l.384-416): `while (_compileQueue.Count > 0) { … ProcessFileChange(next); }` — all queued files compile in ONE editor tick. `OnUssFileChanged` (l.748-774) fans a shared `.uss` out to every dependent `.uitkx`, so editing a theme used by 20 components = 20 sequential Roslyn compiles in a single frame.
- **FIX RECIPE:** change the loop to process exactly ONE item per invocation, then if `_compileQueue.Count > 0` schedule `EditorApplication.delayCall += DrainCompileQueueIfIdle` (the tail scheduling already exists at l.414-415 — keep it; just replace `while` with a single dequeue `if`). Watch out: `_compileInFlight` must stay true only during the single item. Manual test: edit a `.uss` referenced by several components; editor stays responsive while swaps land one per tick.

### H-03 — **MED** — asset-path resolution disagrees across FOUR consumers (cross-cutting with the LSP)
- **The four semantics for a bare path like `@uss "styles.uss"` / `Asset<Texture2D>("icon.png")`:**
  | Consumer | Bare path treated as | Anchor |
  |---|---|---|
  | `DiagnosticsAnalyzer.ResolveAssetPath` (LSP squiggles) | **uitkx-dir-relative** | `DiagnosticsAnalyzer.cs:1406-1424` |
  | `CSharpEmitter.ResolveAssetPaths` (generated code) | **as-is** (unresolved) | `CSharpEmitter.cs:3994-4014` |
  | `UitkxHmrController.RegisterUssDependencies` | **project-root-relative** | `UitkxHmrController.cs:823-830` |
  | `UitkxHmrController.InjectIfResolved` (HMR asset cache) | **as-is** | `UitkxHmrController.cs:1032-1054` |
- **Failure:** the editor shows NO error (analyzer resolves it), the build emits an unresolvable path (runtime asset miss), and HMR's USS dependency map misses the file (theme edits stop hot-reloading).
- **FIX RECIPE:**
  1. Define ONE rule (recommended, matches the analyzer & docs expectations): `Assets/…`/`Packages/…` = project-absolute; everything else (incl. bare names and `./`/`../`) = uitkx-dir-relative.
  2. Implement `public static string ResolveAssetPath(string rawPath, string uitkxAssetDir)` ONCE in language-lib (move the analyzer's version — it already implements this rule — to a public utility class, e.g. `ReactiveUITK.Language.AssetPathUtil`).
  3. Consume it from: `DiagnosticsAnalyzer` (delete its private copy), `CSharpEmitter.ResolveAssetPaths` (compute `uitkxAssetDir` from `filePath` the same way the analyzer's `GetAssetDir` does — move that too), `HmrCSharpEmitter` (mirror), `UitkxHmrController.RegisterUssDependencies` + `InjectIfResolved`.
  4. Tests: emitter test that `Asset<Texture2D>("icon.png")` in `Assets/UI/Foo.uitkx` emits `"Assets/UI/icon.png"`; analyzer test unchanged; parity contract test HMR emitter.
  5. CHANGELOG note: bare relative paths now resolve (previously silently unresolved at runtime) — behavioral fix, patch-acceptable since the old behavior was a broken state.

### H-04 — **MED** — fragile synthetic-header markup parsing in HMR
- **Anchors:** `UitkxHmrCompiler.Compile` l.332-355 (and duplicate in `BuildComponentArtifacts` l.579+): `parseMarkup` builds `"@namespace __Tmp\n@component __Tmp\n" + jsxText` and relies on DirectiveParser REJECTING it into the fallback `DirectiveSet` whose `MarkupStartIndex == 0`, then on the markup parser error-skipping the two bogus `@` lines. Three accidental behaviors in a chain; a change to the DirectiveParser fallback (e.g. U-09's work nearby) can silently break HMR JSX splicing.
- **FIX RECIPE:** add a first-class fragment API to the language lib: `public static ImmutableArray<AstNode> UitkxParser.ParseFragment(string jsxText, string filePath, int startLine, List<ParseDiagnostic> diags)` that constructs a minimal `DirectiveSet` (`MarkupStartIndex: 0, MarkupEndIndex: jsxText.Length, MarkupStartLine: startLine`) internally — the VDG already builds exactly this shape with `directives with {…}` (`VirtualDocumentGenerator.cs:813-820`); reuse. Then: HMR's `parseMarkup` calls it via one cached `MethodInfo` (no synthetic header); VDG and `DiagnosticsPublisher.Publish`'s setup-JSX loop switch to it too. Rebuild the committed DLLs (HMR resolves methods reflectively — `CacheReflectionHandles` must add the new handle with a graceful null-fallback to the old synthetic-header path for older DLLs).

### H-05 — **LOW / inherent** — every swap leaks one non-collectible assembly
- `Assembly.LoadFrom` per compile (l.2333); Mono has no collectible ALCs. Known (memory telemetry exists: `SessionMemoryDeltaMB`). **RECIPE (optional):** add an EditorPrefs-backed threshold (`UITKX_HMR_AutoReloadAfterSwaps`, default 0 = off) that calls the existing `RequestDomainReloadSafe` after N swaps; surface `SessionMemoryDeltaMB` prominently in `UitkxHmrWindow` with a "Reload now" button. Document in AUTOMATION.md.

### H-06 — **LOW** — stale docs + micro-issues
- `UitkxHmrModuleStaticSwapper.cs` header ("What this does NOT cover: static *methods* in modules") is stale — `UitkxHmrModuleMethodSwapper` exists and is called (`UitkxHmrController.cs:543`). Update the header.
- `GC.Collect(2, Optimized)` per compile (l.2352): measure once; if a save-burst stutters, gate it to every Nth compile.
- `TryResolveMissingDependencies` → `ProcessFileChange` recursion (l.889) technically violates the queue's "one in flight" doc — safe today; add a comment or route through the queue.

---

## 4. P2 — LSP server & IDE clients

### U-14 — VS Code client never syncs `.cs` buffers → companion-overlay machinery dead in VS Code
- **Anchors:** `ide-extensions~/vscode/src/extension.ts:107-112` (`documentSelector` = uitkx only; watcher = `**/*.uitkx` only). Server supports `.cs` (`TextSyncHandler.cs:37-49`).
- **FIX RECIPE:** in `clientOptions`: `documentSelector: [{ scheme:'file', language:'uitkx' }, { scheme:'file', language:'csharp' }]` and add a second watcher `createFileSystemWatcher('**/*.cs')` to `synchronize.fileEvents` (array form). Caveat: this makes the client send ALL C# docs — the server's `TextSyncHandler` already fast-paths them (stores text, no diagnostics), verify no perf cliff on large solutions by opening a big `.cs`. VS2022: `UitkxLanguageClient.cs` has `[LanguageClientContentType("uitkx")]` only — add a `csharp` content-type mapping (VS's LSP client attaches per content type; test carefully — VS also runs Roslyn on `.cs`, our server must not publish diagnostics for them, which it doesn't). Rebuild + repackage both extensions.

### U-15 — companion overlay consumed-on-read + never cleared on close  *(pairs with U-14)*
- **Anchors:** `Roslyn/RoslynHost.cs:1109` `_companionOverlay.TryRemove(...)`; `TextSyncHandler` DidClose → `CloseDocument` only removes `_files` entries.
- **FIX RECIPE:**
  1. Replace the overlay mechanism at the read site: in `UpdateWorkspace`'s companion loop (l.1102-1133), read via `_documentStore.TryGetByPath(companionPath, out text)` FIRST (the store has the live buffer once U-14 lands), then disk. Keep `_companionOverlay` only as the rename-flow shim (`RefreshCompanionDocument` l.695-699) or delete it if that path can also use the store.
  2. In `TextSyncHandler` DidClose: if the closed path ends with `.cs`, also call a new `RoslynHost.ClearCompanionOverlay(path)` and re-publish dependents (same loop as DidChange's `.cs` branch) so discarding unsaved edits reverts squiggles.
  3. Test in `lsp-server/Tests`: simulate store-set + two dependent rebuilds → both see buffer text (unit-test the resolution helper; full flow is manual).

### U-16 — workspace scan walks `Library/`+`Temp/`+`obj/` with no exclusions
- **Anchors:** `WorkspaceIndex.cs:517-560`; only `~`-suffixed dirs skipped.
- **FIX RECIPE:** add `private static readonly HashSet<string> s_excludedDirs = new(StringComparer.OrdinalIgnoreCase){"Library","Temp","Logs","obj","bin","node_modules",".git",".vs"};` and a `IsExcluded(path)` segment check next to `IsInsideTildeFolder`; apply in `ScanDirectory` (both loops) and in `WatchedFilesHandler` change events. Replace `Directory.EnumerateFiles(root,…,AllDirectories)` with a manual recursive walk that prunes excluded dirs (AllDirectories can't prune — walking INTO Library still costs minutes). Log scan duration. Test: unit-test `IsExcluded`; manual: open a real Unity project root, log shows Library skipped.

### U-17 — keystroke-path disk I/O (3 sites)
1. `DirectiveParser.InferFunctionStyleNamespace` (`DirectiveParser.cs:1043-1065`) — `File.Exists`+`ReadAllText` of the companion per parse (LSP keystroke AND every SG run).
   **RECIPE:** add `private static readonly ConcurrentDictionary<string,(DateTime mtime,string ns)> s_nsCache;` — key companion path; consult `File.GetLastWriteTimeUtc` (one stat, no read on hit). Bound: clear when > 512 entries. This keeps behavior identical while removing the read. (A fuller fix — host-supplied namespace via the LSP DocumentStore — can come later; don't block on it.)
2. `DiagnosticsAnalyzer.CheckAssetPaths` (`:1314-1404`) — `File.Exists` per `Asset<>`/`@uss` per keystroke, regex also matches comments.
   **RECIPE:** same mtime-less existence cache keyed by resolved path with a 2 s TTL (`(bool exists, long stamp)`); pre-strip comments by running matches only over code spans (reuse the U-10 span helper).
3. `ConfigLoader` per format request — leave (user-triggered).

### U-18 — CRLF file → formatted block becomes LF (mixed EOL)
- **Anchors:** `FormattingHandler.cs:79-118`.
- **FIX RECIPE:** detect dominant EOL: `bool crlf = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0;` then `var newText = string.Join(crlf ? "\r\n" : "\n", fmtLines, firstDiff, fmtEnd - firstDiff + 1);`. One-line change + test with a CRLF document asserting the edit contains `\r\n`.

### U-19 — VS Code tabSize sync walks UP from workspace root (misses in-project configs)
- **FIX RECIPE:** in `extension.ts` `findConfigIndentSize`, replace the upward walk with `const files = await vscode.workspace.findFiles('**/uitkx.config.json', '**/{Library,node_modules,Temp}/**', 1);` (make the function async; call sites already tolerate promise via `syncTabSize` refactor). Keep the watcher.

### U-37 — **NEW (v3)** — component rename doesn't rename the `.uitkx` file → instant UITKX0103 errors
- **Anchors:** `ide-extensions~/lsp-server/RenameHandler.cs` — `CollectComponentRenameEdits` (l.751) + `RenameComponentDeclarationInFile` (l.795) produce TEXT edits only; the declaring file keeps its old name.
- **Failure:** rename `<Counter/>` → `Timer` via F2: every edit applies, then `Counter.uitkx` still declares `component Timer` → **UITKX0103 "does not match filename" Error immediately**, and the SG's hint-name/registry keyed off the file stem goes stale until a manual file rename + Unity refresh.
- **FIX RECIPE:**
  1. In `Handle(RenameParams…)`, when the component-name path fires and the declaring file's stem equals the old component name, return the edits via `WorkspaceEdit.DocumentChanges` (not `Changes`) and append a `new RenameFile { OldUri = …, NewUri = dir/NewName.uitkx }` operation. Order matters: all `TextDocumentEdit`s FIRST (they reference the old URI), the `RenameFile` LAST.
  2. Gate on the client capability `ClientCapabilities.Workspace.WorkspaceEdit.ResourceOperations` containing `rename`; if unsupported, keep today's behavior but log + (optionally) send a `window/showMessage` telling the user to rename the file.
  3. Also rename the companion files that follow the `<Name>.*.uitkx` / `<Name>.*.cs` convention? NO in v1 of this fix — too invasive; add a message listing companions that still reference the old stem.
  4. VS Code applies resource ops natively; VS2022's LSP client support for RenameFile is spotty — test there and keep the fallback path.
  5. Test: `lsp-server/Tests` unit for the WorkspaceEdit shape (documentChanges array order, capability gating).

### U-38 — **NEW (v3)** — hook rename only scans the declaring DIRECTORY; hooks are consumed workspace-wide
- **Anchors:** `RenameHandler.CollectHookRenameEdits` (l.869-899): `Directory.EnumerateFiles(dir, "*.uitkx")` — non-recursive, same-dir only. But hook consumption is workspace-wide via `using static <Ns>.<HookContainer>;` (that's exactly what `RoslynHost.EnrichWithPeerHookUsings` + HMR's `HookContainerRegistry.Seed(assetsPath)` support).
- **Failure:** rename `useCounter` → `useTimer`; a component in another folder calling `useCounter()` keeps the old name → compile break the user discovers later.
- **FIX RECIPE:** scan the same file set the workspace index already knows: iterate `_index` for module/hook-consuming files (or simply reuse `CollectComponentRenameEdits`'s workspace-wide enumeration with the U-16 exclusions), and rename token-boundary occurrences of `oldName(` — reuse the U-10 comment/string-aware `FindHookCall`-style matcher so comments aren't renamed. Keep the `skipFilePath` dedupe. Test: cross-directory call site renamed; commented `// useCounter()` NOT renamed.

### U-39 — **NEW (v3)** — DiagnosticMapper suppresses ANY `CS1503` mentioning `Func<` → hides REAL user errors
- **Anchors:** `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs:129-137` (`msg.Contains("Func<")` → drop), and the batch-wide CS1662 cascade suppression at l.109-117 + 144-145.
- **Failure:** `items.Select(5)` (wrong arg where a `Func<T,R>` is expected) → real CS1503 whose message mentions `Func<` → silently dropped; user sees no squiggle in the editor while the Unity build fails. The CS1662 batch rule can likewise hide an unrelated real lambda error anywhere in the file whenever ONE state-setter direct-value call exists.
- **FIX RECIPE:**
  1. The scaffold's state-setter false positive always names the synthesized delegate — tighten the filter to `msg.Contains("__StateSetter__")` (verify the exact rendered type name in one probe: type `setCount(5)` in a test doc and log the CS1503 message; the scaffold emits `__StateSetter__<T>(Func<T,T>)`, so either marker works — prefer the scaffold-unique one).
  2. For CS1662: only suppress when the diagnostic's mapped span OVERLAPS a span that produced a suppressed state-setter CS1503 (collect those spans in the pre-scan instead of a boolean).
  3. Tests: mapper unit tests with three fabricated diagnostics — state-setter CS1503 (dropped), LINQ CS1503 mentioning `Func<` (kept), CS1662 overlapping vs non-overlapping (dropped/kept).

### U-40 — **NEW (v3, LOW)** — rename text-scan is comment-blind + unfiltered workspace walk
- `RenameTagUsages` (l.824) regex renames `<OldName` occurrences inside markup comments/strings; `CollectComponentRenameEdits` (l.764) enumerates from the workspace root with only tilde filtering (Library/PackageCache walk — same class as U-16). RECIPE: apply the U-16 exclusion helper here too; for comment-blindness, run matches only over code spans using the shared lexer (same helper as U-10 step 1) — or accept and document (text-scan rename renaming commented-out tags is arguably desirable; DECIDE and write it down in the handler doc-comment).

### U-41 — **NEW (v3, LOW)** — stale validator docs + cross-surface severity mismatch CONFIRMED (strengthens U-12)
- `SourceGenerator~/Emitter/StructureValidator.cs:15-19` class doc lists "UITKX0009/0017" but the actual descriptors are aligned to 0106/0108 (`UitkxDiagnostics.ForeachMissingKey` = **UITKX0106, Warning** at `UitkxDiagnostics.cs:170-176`). Meanwhile the LSP analyzer's 0106 is **Error** (`DiagnosticsAnalyzer.cs:686`). Same rule, different severity per surface — the U-12 fix direction is now unambiguous: set the LSP to Warning to match the SG. Also fix the stale doc-comment lines while there.

### LSP smalls (do as one cleanup commit)
- `DiagnosticsPublisher._lastT1T2/_lastT3`: evict on DidClose (add a `Forget(path)` called from `TextSyncHandler`); key with `StringComparer.OrdinalIgnoreCase`.
- `ScheduleDebouncedRevalidation` CTS race: swap to `Interlocked.Exchange` pattern.
- DidClose: push empty diagnostics for the closed uitkx URI so squiggles clear.
- `RoslynHost.EnqueueRebuild` timer race: guard `state.DebounceTimer` swap with a small lock on `state`.

---

## 5. P2 — Source generator

### U-20 — Consolidate the six drifted mini-lexers (root-cause refactor — DO FIRST)
- **The six:** `ExpressionExtractor` (own skippers — **`$@"` drift confirmed**: l.120-142 handles `@$"` but lexes `$@"` as NON-interpolated verbatim), `ReturnFinder.TrySkipNonCodeSpan` (correct), `DirectiveParser.TrySkipNonCodeSpan` (private dup) + `TrySkipStringOrCharLiteral` (most complete), `AstFormatter.ScanParens`/`CollapseIntraLineSpaces`/brace counters (string-blind — U-03), `CSharpEmitter.FindLastTopLevelStatementBoundary`, `DiagnosticsPublisher.FindEnclosingScopeEnd`.
- **FIX RECIPE:**
  1. Create `ide-extensions~/language-lib/Parser/CSharpLexFacts.cs`: `internal static class CSharpLexFacts` with `TrySkipNonCode(string, ref int, int limit)` (copy `DirectiveParser.TrySkipStringOrCharLiteral` + comment handling — it is the most complete: both `$@`/`@$`, interpolation-hole recursion), `FindMatchingBrace`, `FindMatchingParen`, `int[] BuildLineStarts(string)`, `(int line,int col) OffsetToLineCol(int[] starts, int offset)`, and the U-03 `ComputeMultilineStringLineMask`.
  2. Migrate consumers one at a time, running both test suites between each: (a) ExpressionExtractor (fixes `$@"` — add a regression test first: `FromBrace` on `{ $@"{x.ToString("D")}" + "(" }` must balance); (b) ReturnFinder; (c) DirectiveParser; (d) AstFormatter (with U-03); (e) CSharpEmitter; (f) DiagnosticsPublisher.
  3. Add a `LexFactsTests.cs` with the torture inputs: `$@"{a("(")}"`, `@$"…"`, `'A'`, `"\\"`, nested holes, unterminated everything.
  4. Rebuild committed DLLs; the Godot repo's `scanner-cases.json` cross-impl contract idea is the model — port the same JSON-driven cases if time allows.

### U-21 — incremental generator effectively non-incremental
- **Anchors:** `SourceGenerator~/UitkxGenerator.cs:144-260` — single `RegisterSourceOutput` over `(projectRoot × allUitkxTexts × CompilationProvider)` reprocesses everything per compilation change; peer prescan parses every file 2× (`TryBuildPeerComponentInfo` + `TryBuildPeerHookContainerInfo` each call `DirectiveParser.Parse`), then `UitkxPipeline.Run` parses a 3rd time.
- **FIX RECIPE (staged; stage 1 is safe and delivers most of the win):**
  1. **Stage 1 (merge the double parse):** make one `TryBuildPeerInfo(source, path, out PeerComponentInfo?, out PeerHookContainerInfo?)` doing a single `DirectiveParser.Parse`; also pass the parsed `DirectiveSet` into `UitkxPipeline.Run` via a new optional parameter so Run skips its own directive parse (keep the old path when null). 3 parses → 1.
  2. **Stage 2 (true incrementality):** convert to `var parsed = uitkxTexts.SelectMany(...)`-style per-file transforms: `context.AdditionalTextsProvider.Where(uitkx).Select((t,ct) => ParseToPeerInfoAndSource(t))` (cacheable per file), `.Collect()` only for the peer array; combine each file node with the collected peers + a REDUCED compilation projection (`context.CompilationProvider.Select((c,_) => c.AssemblyName)` plus whatever `PropsResolver` actually needs — audit its `Compilation` usage first; if it needs symbol lookups, keep CompilationProvider but ONLY on the per-file emit node, accepting re-emit on compilation change while still caching parse).
  3. Verify with the generator cacheability tests pattern (`GeneratorDriver` + `TrackIncrementalGeneratorSteps` in a new test).

### U-22 — `UITKX0000` "generator alive" ships as Warning — VERIFY THEN DEMOTE
- **Anchors:** `UitkxGenerator.cs:40-48` (Warning, enabled, reported per assembly at l.243). No suppression anywhere in-repo.
- **RECIPE:** (1) open the store-shell project (`C:\Yanivs\GameDev\UnityStoreShell\shell`) console after a script recompile and check for `UITKX0000`; (2) if visible: change `defaultSeverity` to `DiagnosticSeverity.Info` (still visible in verbose logs) — it exists only as a liveness beacon; (3) fix the stray comment "UITKX0001" at l.242.

---

## 6. P3 — smaller parser/tooling items (one cleanup batch)

| ID | Anchor | Item + one-line recipe |
|---|---|---|
| U-24 | `UitkxParser.ParseControlBlockBody:166-179` | Unbalanced `{` recovery emits no diagnostic in snippet mini-parses → add ErrUnexpectedToken("EOF", …, "'}' closing the directive body"). |
| U-25 | `UitkxParser.IsJsxInBraces:1378` | `attr={(<Tag/>)}` typed as opaque C# — extend the peek to skip one `(`+ws before checking `<` (then ParseElement path handles it; keep a fallback test). |
| U-26 | `MarkupTokenizer.TryConsume(string)/AdvanceTo` | CRLF overshoot latent trap → add `Debug.Assert(!s.Contains('\r'))` + doc comment. |
| U-27 | `ExpressionExtractor.SkipCharLiteral:327` | `'A'`/`'\x41'` mis-skip → skip escape then scan to next `'` (bounded 8 chars). Covered by U-20 migration tests. |
| U-28 | `ExpressionExtractor.FindMatchingClose` unclosed → silently swallows rest of file | return a `found` flag up through `ReadBraceExpressionWithOffset`; parser emits UITKX0304-style "unclosed '{' expression" (new code) at the open brace. |
| U-29 | TextNode line = end line | capture `_scanner.Line` BEFORE `ReadTextContent`. |
| U-30 | Dead code (verified unreferenced) | delete: `UitkxParser.LineAtPos`(1315), `AdvanceScannerTo`(1523), `SemanticTokensProvider.s_codeBodyTokenRegex`, `IsLikelyEmbeddedMarkupLine`, `IsLikelyMarkupCloserLine`, `OffsetToLine1`, the `ExpressionNode → EmitKeyword("@(")` case. |
| U-31 | SemanticTokensProvider ignores now-populated `SourceColumn`; first-occurrence `FindOnLine` collides on duplicate names per line | use `el.SourceColumn`/`attr.SourceColumn` when > 0, fall back to search; fix stale header comment. |
| U-32 | AstCursorContext single-line | document limitation; optional: when line has no `{`, walk up to 5 previous lines for an unclosed `{` before giving up (bounded heuristic). |
| U-33 | AstNode body-payload duplication ×5; `SwitchNode` missing `SwitchExpressionOffset` | extract `record ControlBlockPayload(...)`; `ParseSwitch` → `ReadParenExpressionWithOffset` and store the offset; wire into VDG's directive-expression mapping (grep `ConditionOffset` usage in `VirtualDocumentGenerator.cs` and mirror). |
| U-34 | `WorkspaceIndex.s_propPattern` misses expression-bodied props (`public string Text => …`) | extend regex to also match `=>` bodies. |
| U-35 | ~~Cargo.toml staged~~ | **RESOLVED in v2** (unstaged + deleted). `tscn_stable.html` also deleted (was the 20% GitHub HTML). Remove its `pathsToOmitFromStore` entry in `config.json` once the deletion is committed. |

Formatter behavior notes (unchanged from v1, non-corrupting): 1-blank-line normalization between top-level siblings (EndLine untracked); `CollapseIntraLineSpaces` collapses inside one-line `/* */` interiors; `Ln()` multi-line re-anchoring.

---

## 7. Core (`Shared/`) — optimization pass (unchanged from v1 + verified)

| ID | Anchor | Item + recipe |
|---|---|---|
| C-01 | `FiberChildReconciliation.ExtractProps:494` (+2 clones) | Text vnodes allocate a Dictionary per reconcile visit. Recipe: in the text-fiber update path compare `vnode.TextContent` to the fiber's stored text prop directly; if a props-dict is truly needed, cache a per-thread single-entry dict keyed by generation. |
| C-02 | `ExtractProps` duplicated ×3 (`FiberChildReconciliation:482`, `FiberFactory:287`, `FiberFunctionComponent:361`) | consolidate to one internal helper (do together with C-01). |
| C-03 | `FiberFunctionComponent.ScheduleEffect:601` `// TODO: Use proper scheduler` — runs effects inline | investigate callers first: if only reachable from legacy paths, delete; if reachable during commit, route through `_pendingPassiveEffects`. Do NOT change blindly — reconciler timing. |
| C-04 | `FiberReconciler.MetricsEmitted` static event | editor-only leak risk; clear subscribers on `UnmountRoot`/domain reload (`[InitializeOnLoadMethod]` in Diagnostics). |
| C-05 | `PropsApplier.cs:669` cursor TODO | document as unsupported in schema so LSP flags it (add to `uitkx-schema.json` exclusions). |
| C-06 | LINQ confined to cold paths — keep it that way | no action; note for reviewers. |

Runtime/HMR interplay verified in v2: interrupted-render restart machinery exists on both engines (Unity 0.6.4 fix; Godot `reconciler.gd` `_restart` cap-25).

---

## 8. Execution order (v2)

1. **U-20 lexer consolidation** (unlocks/simplifies U-03, U-04, U-07, U-10, U-27, U-28).
2. **Formatter batch:** U-01, U-02, U-03, U-04, **U-36** (+ snapshot tests per recipe; all five have paste-ready repro inputs above).
3. **HMR batch:** H-01 (gate), H-02 (drain), H-03 (asset paths — coordinates with the emitters), H-04 (fragment API), H-06 docs. H-05 optional.
4. **Grammar batch:** U-05, U-06 (4 sites + parity tests), U-08, U-09, U-10, U-11, U-12 — one release-notes entry.
5. **LSP batch:** U-14+U-15 together, U-16, U-17, U-18, U-19, **U-39** (mapper over-suppression — small, high value, do early in this batch), smalls.
5b. **Rename batch:** U-37 (file rename op), U-38 (workspace-wide hook rename), U-40 — these share `RenameHandler.cs`; one PR.
6. **SG batch:** U-21 stage 1 (+ stage 2 if time), U-22 verify/demote.
7. **Cleanup sweep:** section 6 + C-01/C-02; C-03 investigate-first.

## 9. Cross-repo note
The Godot repo (`plans/FINAL_AUDIT_GODOT.md`) is the reference implementation for: single-lexer + `scanner-cases.json` contract testing (model for U-20 step 4), token-boundary hook detection (`guitkx.gd _find_hook_call` — model for U-10), formatter trivia preservation (model for U-01/U-02), paren-wrapped `@case` values (context for U-04). When fixing here, read the Godot counterpart first.
