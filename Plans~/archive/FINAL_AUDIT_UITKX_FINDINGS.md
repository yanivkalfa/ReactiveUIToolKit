> **ARCHIVED 2026-07-15** — Every finding (U-01..U-43, H-01..H-06, C-03/C-05, LSP smalls) verified FIXED in code with tagged comments + tests as of 2026-07-15. The perf sibling (FINAL_AUDIT_UITKX_OPTIMIZATIONS) retains the open items — see REMAINING_WORK.md.

# ReactiveUIToolKit — Final Audit: FINDINGS & BUGS (v5)

**VERIFICATION PASS (v5, 2026-07-06):** a consolidated probe verifier re-ran every previously-confirmed finding against the current tree — **14/14 still reproduce**: U-01, U-02, U-03, U-04, U-05, U-07, U-08, U-09, U-10 (both comment + `_useState` variants), U-12 (0106 severity == Error in the analyzer), U-36 (+ its non-idempotency), and **U-20's `$@"` lexing drift is UPGRADED from latent to CONFIRMED** with an exact repro: `ExpressionExtractor.FromBrace` on `{ $@"{a("}")}" }` extracts the truncated `$@"{a("` (the quoted `}` inside the interpolation hole closes the outer brace early — add this string to the U-20 regression tests). Grep-verified code-read claims: U-37 (zero `RenameFile`/`DocumentChanges` occurrences in RenameHandler), U-39 (the `__StateSetter__` marker exists in the scaffold — `Shared/Core/HookRegistry.cs:429` — so the tightened filter works), H-01 (`diagList` truly never read after the parses in `Compile`'s body), H-03 (all four resolution behaviors re-read at their anchors). The v5 sweep of remaining unread surface (Definition/References handlers, CanonicalLowering, module/hook emitters, StaticReadonlyStripper) found ONE new item (U-42) and one cleanup note (see section 6).

**Date:** 2026-07-06 (v4 = v3 content split into two documents; this file = correctness findings/bugs with fix recipes. Performance/optimization items live in `FINAL_AUDIT_UITKX_OPTIMIZATIONS.md` — IDs U-16, U-17, U-21, H-05, C-01, C-02, C-04, C-06 moved there unchanged.)
**Auditor:** Claude, three read-only research passes. Working-tree changes made during the audit (already committed on `cleanup_and_upgrades`): deleted stray `Cargo.toml` and `tscn_stable.html` (the 1.45 MB saved Godot-docs page that was ~20% "HTML" in GitHub language stats; drop its `pathsToOmitFromStore` entry in `config.json` when merging master, where that list lives).

**Scope:** language-lib (parser, formatter, diagnostics, IntelliSense, semantic tokens, VDG/SourceMap), LSP server, SourceGenerator, IDE extensions, Editor/HMR, Shared core (correctness-relevant parts).
**Method:** full-file reads plus an **empirical probe suite** (scratch console app referencing `ide-extensions~/language-lib/UitkxLanguage.csproj`; probe IDs P1–P16 = pass 1, V1–V5 = pass 2). Every finding marked **CONFIRMED** was reproduced against the working tree on 2026-07-06; everything else is code-reading analysis with the failure scenario stated.

**Audited and found CLEAN (do not re-audit):** `CSharpEmitter` string escaping (`EscStr`), `Hooks.DepsChanged` semantics, `Runtime/Core/RootRenderer` (UUM-127851 editor-poll gates intact, compiled out of players), `RenderScheduler`, `HooksValidator` structure (U-10 twin — its fix covers it), Hover/Definition/SignatureHelp handlers, CompletionHandler's asset-path directory enumeration (user-triggered cadence), HMR swapper design (readonly-strip strategy, Family-handle indirection).

---

## HOW TO USE THIS DOCUMENT (read first, executor)

- Findings are `U-##` (tooling) and `H-##` (HMR). Each has: severity, exact anchors, a failure scenario, and a **FIX RECIPE** — numbered steps naming the file, method, and change. Where a recipe says "add a test", add it in the named test file BEFORE the change (red → green).
- **Never patch a symptom.** Every recipe targets the root cause; if the recipe fights the code, stop and re-read the finding's "why".
- After ANY change under `ide-extensions~/language-lib/` or `SourceGenerator~/`: run BOTH test suites (`dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj` and `dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj`), then rebuild the committed analyzer DLLs with `scripts/build-generator.ps1` and commit them WITH the source change.
- The HMR emitter (`Editor/HMR/HmrCSharpEmitter.cs`) mirrors the SG emitter (`SourceGenerator~/Emitter/CSharpEmitter.cs`); contract tests (`SourceGenerator~/Tests/Hmr*ContractTests.cs`) guard the pairing. When a recipe touches one emitter it names the mirror step — do not skip it.
- Versioning: patch bump per batch; CHANGELOG.md entry per batch (VERSIONING.md). Do not commit/push without the user's go.
- **Branch note:** the audit ran on `cleanup_and_upgrades` (a496377 + audit commits), which is ~65 commits behind origin/master (master has the Asset-Store CI campaign — workflows/CICD/config, not tooling code). Merge/rebase master before fixing; all anchors are in files the store campaign did not touch.

### Probe harness (rebuild to verify fixes)
`probe/probe.csproj` OUTSIDE the repo:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><ProjectReference Include="<repo>/ide-extensions~/language-lib/UitkxLanguage.csproj" /></ItemGroup>
</Project>
```
`Program.cs` calls `DirectiveParser.Parse` + `UitkxParser.Parse` + `new AstFormatter(FormatterOptions.Default).Format(src, "Probe.uitkx")` on inline strings. Every CONFIRMED finding quotes its repro input — paste, run red, fix, run green.

---

## 0. Executive summary

1. **Five confirmed formatter data-loss/corruption bugs** (U-01…U-04, U-36). Formatter fixes are the top priority — format-on-save is the default workflow.
2. **Six drifted mini-lexers** (U-20) are the highest-leverage refactor; U-04/U-07/U-36 and the `$@"` drift are copy-drift casualties.
3. **HMR:** the compile path **ignores parse diagnostics** (H-01 — a save with a syntax error hot-swaps code emitted from an error-recovered AST; the SG gates correctly), a synchronous whole-queue drain freezes the editor on shared-USS edits (H-02), and asset paths resolve with four different semantics (H-03).
4. Grammar asymmetries confirmed: Allman `@else` (U-05), `&&`-JSX setup gap (U-06, four splice sites incl. HMR), literal `@` in text (U-08), hook-rules false positives (U-10, comments + `_useState`, probes V3/V4).
5. **Refactoring tools:** F2-rename never renames the declaring file → instant UITKX0103 error state (U-37); hook renames miss cross-directory call sites (U-38); the diagnostic mapper over-suppresses CS1503 and hides real type errors in `Func<`-taking APIs (U-39).

---

## 1. P0 — Formatter data loss / corruption (ALL CONFIRMED by probe)

### U-01 — Leading comments before `component` silently deleted  *(CONFIRMED, P1)*
- **Anchors:** `ide-extensions~/language-lib/Formatter/AstFormatter.cs` → `FormatFunctionStyleComponent()` (rebuilds output solely from `DirectiveSet` + AST); `Parser/DirectiveParser.cs` → `SkipLeadingFunctionStyleTrivia()` (consumes comments, records nothing).
- **Repro:** `"// My license header\ncomponent Foo {\n    return (\n        <Label text=\"hi\" />\n    );\n}\n"` → format → header gone, zero diagnostics. Same for comments between `@namespace`/`@using`/`@uss` lines and HTML comments in the preamble.
- **FIX RECIPE:**
  1. In `ParseResult.cs`, add to `DirectiveSet` an init-only member `public ImmutableArray<(string Text, bool IsBlock, int Line)> LeadingTrivia { get; init; }` defaulting to empty (init property, NOT a positional parameter — no call-site changes).
  2. In `DirectiveParser`, give `SkipLeadingFunctionStyleTrivia` an overload that appends each consumed `//`, `/* */`, `<!-- -->` comment (raw text incl. delimiters + a following-blank-line flag) to a list when the caller passes one. The preamble loop in `TryParseFunctionStyle` passes the list; `LooksLikeFunctionStyleComponent` passes null. Attach the list to every `DirectiveSet` built by `TryParseFunctionStyle` (all 4 return paths).
  3. In `AstFormatter.FormatFunctionStyleComponent` (and `FormatHookModuleFile`), re-emit `directives.LeadingTrivia` verbatim before the preamble, preserving blank-line separators.
  4. Trivia BETWEEN preamble directives: acceptable normalization is emit-all-trivia-first then directives; document in CHANGELOG.
  5. Tests (`SourceGenerator~/Tests/FormatterSnapshotTests.cs`): `//` header round-trips; `/* block */` header; comment between two `@using` lines survives; idempotency each.

### U-02 — `{expr}`/comment children deleted from inline-attribute JSX  *(CONFIRMED, P4)*
- **Anchors:** `AstFormatter.cs:756-777` `SerializeJsxInlineCore` — handles ONLY `ElementNode` and `TextNode` children.
- **Repro:** `<Box header={<Box><Label text="t" />{1 + 1}</Box>} />` → format → `{1 + 1}` deleted.
- **FIX RECIPE:**
  1. Extend the child loop: `case ExpressionNode en: sb.Append('{').Append(en.Expression).Append('}'); break;` and `case CommentNode cn: sb.Append($"/* {cn.Content.Trim()} */"); break;` (line comments become block form inside single-line serialization).
  2. Control-flow nodes inside inline-attr JSX cannot serialize single-line: detect first; if present, make `Format()` return the ORIGINAL full source (never drop). (The `AttributeNode` carries no raw span today, so whole-file fallback is the correct minimal guarantee.)
  3. Tests: expr child round-trips; comment child round-trips; `@if` inside inline-attr JSX leaves the file unchanged.

### U-03 — Multi-line verbatim string interiors re-indented (string VALUE corrupted)  *(CONFIRMED, P5)*
- **Anchors:** `AstFormatter.cs` `EmitSetupCodeNormalized` (l.1162+) and `EmitCSharpLines` (l.792+) — line-by-line `raw.Trim()` + re-indent + `CollapseIntraLineSpaces` with no cross-line string state; the `{content`-line splitter (l.803-849) and mid-line brace counting (l.1095-1103) are also string-blind.
- **Repro:** setup `var s = @"line1\n    }\n  indented content\n";` → interior lines shift +2 spaces on first format (value changed; stable afterwards — plain idempotency tests never catch it).
- **FIX RECIPE:**
  1. Add to the U-20 shared lexer: `public static bool[] ComputeMultilineStringLineMask(string code)` — per line, whether the line STARTS inside an open `@"`/`$@"`/`@$"` literal (single forward scan recording line starts).
  2. In `EmitCSharpLines` AND `EmitSetupCodeNormalized`: masked lines emit **byte-verbatim** (no Trim/tab-expansion/collapse/brace-count/indent bookkeeping).
  3. Exclude masked lines from `baseSpaces` and from the `{content`-split preprocessor.
  4. Tests: P5 input round-trips byte-identical; a `}`-leading line inside a verbatim string doesn't pop the block stack; `$@"` variant.

### U-04 — `@case global::…` silently corrupts the file on format  *(CONFIRMED, P9)*
- **Anchors:** `Parser/UitkxParser.cs` `ParseSwitch` case-value scan (l.1183-1192) stops at the FIRST `:`; the formatter re-emits the mangled AST.
- **Repro:** `@case global::System.StringComparison.Ordinal:` → `@case global:` + garbage body. Zero diagnostics.
- **FIX RECIPE:**
  1. In the case-value scan, consume `::` as content (at `:`, peek next; if also `:`, advance twice and continue; else stop).
  2. Run `Hmr*ContractTests` (emitters consume `SwitchCase.ValueExpression` — no change expected, verify).
  3. Tests: parser test that `global::Ns.Enum.Val` parses as one value; formatter round-trip; emitter test that `case global::Ns.Enum.Val:` compiles.

### U-36 — Setup-code JSX splice-index desync REPLACES one block's markup with another's  *(CONFIRMED, V1)*
- **Anchors:** `AstFormatter.cs` `EmitSetupCodeWithJsx` (l.2128+) walks blocks from its own `ScanJsxParenBlocks` (paren+`<` only) while indexing `directives.SetupCodeMarkupRanges` from `DirectiveParser.FindJsxBlockRanges` — a DIFFERENT detector that also records `(@if …)` directive-paren blocks and `=> <Tag` bare-arrow ranges. Any block only one detector sees shifts `origMarkupIdx`.
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
  Output: `var y = ( @if (cond) { … } );` — **`y`'s real markup DELETED, `x`'s `@if` content spliced in its place.** Non-idempotent (V5).
- **FIX RECIPE (no index-arithmetic band-aids):**
  1. In `EmitSetupCodeWithJsx`, stop re-scanning with `ScanJsxParenBlocks`; iterate `SetupCodeMarkupRanges` + `SetupCodeBareJsxRanges` directly (absolute offsets), converting to setup-relative offsets exactly as `CSharpEmitter.AbsToSetupOffset` does (copy that helper into the formatter or the shared lexer).
  2. Classify each range from the source text (char before Start == `(`? first non-ws == `@`?). Directive-paren blocks (`(@if …)`) are left VERBATIM (treated like single-line blocks: skip).
  3. `ScanJsxParenBlocks` remains for `EmitDirectiveBodySetupCode`/`EmitBodySetupCodeWithJsx` (normalized strings, no ranges available; blocks self-contained) — add a comment explaining why the detector split is allowed THERE only.
  4. Tests: the V1 repro round-trips with both blocks intact + idempotent; `=> <Tag/>` lambda before a paren block; two paren blocks; commented-out JSX before a real block (V2 input stays green).

---

## 2. P1 — Grammar / diagnostics correctness

### U-05 — Allman `@else` is a parse error while Allman `@if` works  *(CONFIRMED, P2)*
- **Anchors:** `UitkxParser.cs` `LookAheadIsElse` (l.1453-1501) skips only spaces/tabs after `@else`; consumption path l.915-925.
- **FIX RECIPE:** extend the post-`@else` whitespace skip to `\r`/`\n` in `LookAheadIsElse`; in `ParseIf`'s else branch use `SkipWhitespaceAndNewlines` before `PeekAt('{')` (the `@else if` path already tolerates newlines). Tests: K&R + Allman forms of `@if/@else if/@else`; `@else` + blank line + `{`.

### U-06 — `cond && <Tag/>` desugar missing outside `{expr}` — FOUR fix sites
- **Anchors:** `SourceGenerator~/Emitter/CSharpEmitter.cs` `SpliceSetupCodeMarkup` (l.2334) + `SpliceBodyCodeMarkup` (l.2610) splice `&&`-detected JSX verbatim → `isOn && V.Badge(...)` = CS0019; the desugar lives only in `SpliceExpressionMarkup` (l.2767). Mirrors: `Editor/HMR/HmrCSharpEmitter.cs` `SpliceSetupCodeMarkup` (l.572, same gap) / `SpliceExpressionMarkup` (l.975, has desugar).
- **FIX RECIPE:**
  1. Extract the desugar block from `CSharpEmitter.SpliceExpressionMarkup` (l.2817-2870 + closing append l.2939-2943) into `bool TryEmitLogicalAndDesugar(string text, int prev, int s, int e, StringBuilder spliced, Func<string,string> emitJsx)`.
  2. Call it from `SpliceSetupCodeMarkup` and `SpliceBodyCodeMarkup` before the plain-prefix append.
  3. Mirror 1-2 in `HmrCSharpEmitter` (its `EmitCtx` variants).
  4. Extend `HmrEmitterParityContractTests` with `var x = isOn && <Badge/>;` setup-code case; add an `EmitterTests` case asserting the ternary output.
  5. Also add `=` (not `==`,`<=`,`>=`,`!=`,`=>`) as a boundary in `DirectiveParser.FindLhsStartForLogicalAnd` (l.2508) so `x = cond && <T/>` yields LHS `cond` (absorbed U-23).

### U-07 — `FindJsxBlockRanges` is block-comment-blind → false CS1002 + comment reformatting  *(CONFIRMED, P3b/P3c)*
- **Anchors:** `Parser/DirectiveParser.cs:2154-2245` skips `//` and strings but not `/* */` (sibling `FindBareJsxRanges` l.2299-2319 does).
- **FIX RECIPE:** add before the string-skip in the loop:
  ```csharp
  if (source[i] == '/' && i + 1 < rangeEnd && source[i + 1] == '*')
  { int e2 = source.IndexOf("*/", i + 2, StringComparison.Ordinal); i = (e2 >= 0 && e2 + 2 <= rangeEnd) ? e2 + 2 : rangeEnd; continue; }
  ```
  Tests: `/* old UI: (<Label/>) was removed */` → no CS1002 (P3b); VDG doesn't strip commented-out JSX; formatter leaves comment interiors byte-identical (P3c).

### U-08 — Literal `@` in text content unwritable  *(CONFIRMED, P12)*
- **FIX RECIPE:** in `UitkxParser.ParseContent`'s `@` branch, peek the keyword BEFORE consuming `@`; only enter the directive branch for known keywords (incl. the error-worthy `else/case/default/break/continue/code`); otherwise treat the `@` as text. Keep erroring for `@identifier` followed by `(` or `{` (catches `@foreech (x in y) {` typos) — test that explicitly. Fix `ErrUnknownDirective` message: drop `code` from the valid list, suggest `{"@"}`. Tests: `<Label>contact me @ home</Label>` clean; `@if` unchanged; `@foreech (…) {` still errors.

### U-09 — Duplicate `@namespace` → misleading whole-file UITKX2105  *(CONFIRMED, P13)*
- **FIX RECIPE:** in `TryParseFunctionStyle`'s preamble loop, when `inlineNamespace != null` and another `@namespace` is at the cursor, consume the line, emit "duplicate @namespace directive — only one is allowed" at that line, keep the FIRST, continue. Test: two `@namespace` lines → exactly one diagnostic, correct namespace, component parses.

### U-10 — Hook-rules scanning: comment/string/word-boundary FALSE POSITIVES  *(CONFIRMED, V3 + V4)*
- **Repro:** `// TODO: maybe useState(1) here later` inside an `@if` body → UITKX0013 **Error** (V3). `var z = _useState(1);` → same false error (V4).
- **Anchors:** `language-lib/Diagnostics/DiagnosticsAnalyzer.cs` `ScanCodeForHooks` (l.517), `CheckExpressionForHooks` (l.595), `CheckAttributeHooks` (l.639) — raw `IndexOf(pattern)`. Twin: `SourceGenerator~/Emitter/HooksValidator.cs`.
- **FIX RECIPE:**
  1. One helper (next to `HookRegistry.GetValidationPatterns()` so both consumers share it): `static int FindHookCall(string code, string pattern, int limit)` — moving `IndexOf`; reject when the preceding char is letter/digit/`_`/`.` (the `.` also fixes `obj.useState(`); reject matches inside comments/strings via `ReturnFinder.TrySkipNonCodeSpan` span-walking. **Reference implementation: Godot's `guitkx.gd _find_hook_call` (l.596-609)** — port its semantics.
  2. Replace all three DiagnosticsAnalyzer call sites + HooksValidator's.
  3. Tests (`DiagnosticsAnalyzerTests.cs`): V3 input → none; `_useState(` → none; `"useState("` in a string → none; real `useState(0)` in `@if` body → still errors.

### U-11 — `return someVar;`/unwrapped ternary invisible in control-block bodies
- **FIX RECIPE:** in `UitkxParser.ParseBodyForIde`, when `TryFindTopLevelReturn` fails AND `bodyCode` contains `return `, emit (reuse UITKX2102 text: "'return' in a directive body must use 'return (…)', 'return <Tag/>;' or 'return null;'") at the `return` offset. Document the rule in the language reference. Test: `@if (x) { return items.First(); }` → the diagnostic.

### U-12 — UITKX0106 severity mismatch across surfaces  *(CONFIRMED direction via U-41)*
- SG's `UitkxDiagnostics.ForeachMissingKey` = UITKX0106 **Warning** (`UitkxDiagnostics.cs:170-176`); LSP analyzer's 0106 = **Error** (`DiagnosticsAnalyzer.cs:686`). **FIX:** set the LSP severity to Warning; align tests.

### U-13 — `ErrMismatchedTag` wrong line in message + no columns
- **FIX RECIPE:** explicit `foundLine`/`openLine` params; `ParseContent:457` passes the closing-tag line as found-line (omit open-line from message if unknown); `ParseElement:733` passes both; set `SourceColumn/EndColumn` from the closing tag's span. Do with the section-5 cleanup batch.

---

## 3. P1 — HMR

*Flow: `UitkxHmrFileWatcher` (50 ms debounce) → `UitkxHmrController` FIFO queue → `UitkxHmrCompiler.Compile` (reflection-driven parse via committed language DLL → `HmrCSharpEmitter` → in-process Roslyn → `Assembly.LoadFrom` + `ForceRunModuleInitializers`) → module-static/method swappers → `RefreshRuntime.PerformRefresh`. Design quality is high; the findings:*

### H-01 — **HIGH** — HMR ignores parse diagnostics; emits from an error-recovered AST
- **Anchors:** `UitkxHmrCompiler.Compile` (l.261-475): `diagList` created (l.272), filled by both parses, **never read**. Contrast `SourceGenerator~/UitkxPipeline.cs:82-91 & 194-207` (converts every Error into `#line`/`#error` and returns).
- **Failure:** save a syntactically-broken `.uitkx` during HMR → cryptic csc wall, or the recovered AST emits valid C# that hot-swaps WRONG UI silently.
- **FIX RECIPE:**
  1. After the AST parse (l.316): iterate `diagList` via the existing reflection helpers (`GetItems` + `GetProp`); if any `Severity == Error`, set `result.Error = "[HMR] {file} has N parse error(s):\n" + "  UITKXxxxx Lxx: message"` lines, return.
  2. Same gate in `CompileHookModuleFile` (l.1024) and `BuildComponentArtifacts` (l.507).
  3. Cache the `ParseSeverity.Error` enum value in `CacheReflectionHandles` (l.1351).
  4. Unit-test a pure `FormatParseErrors` helper in the SG suite; manual: HMR on, save `<Box>` unclosed → one readable UITKX0301 line, no swap.

### H-02 — **MED** — whole-queue synchronous drain freezes the editor on fan-out
- **Anchors:** `UitkxHmrController.DrainCompileQueueIfIdle` (l.384-416) `while` loop compiles everything in ONE tick; `OnUssFileChanged` (l.748-774) fans a shared `.uss` to all dependents → N sequential Roslyn compiles in a frame.
- **FIX RECIPE:** dequeue exactly ONE item per invocation; keep the existing `delayCall` tail (l.414-415) to continue. Manual test: edit a `.uss` used by many components — editor stays responsive, swaps land one per tick.

### H-03 — **MED** — asset-path resolution disagrees across FOUR consumers
- | Consumer | Bare `"styles.uss"` treated as | Anchor |
  |---|---|---|
  | `DiagnosticsAnalyzer.ResolveAssetPath` (editor squiggles) | uitkx-dir-relative | `DiagnosticsAnalyzer.cs:1406-1424` |
  | `CSharpEmitter.ResolveAssetPaths` (generated code) | as-is (unresolved) | `CSharpEmitter.cs:3994-4014` |
  | `UitkxHmrController.RegisterUssDependencies` | project-root-relative | `UitkxHmrController.cs:823-830` |
  | `UitkxHmrController.InjectIfResolved` (HMR asset cache) | as-is | `UitkxHmrController.cs:1032-1054` |
- **Failure:** editor shows NO error, build emits an unresolvable path, HMR's USS map misses the file (theme edits stop hot-reloading).
- **FIX RECIPE:** one rule (`Assets/`/`Packages/` absolute; everything else uitkx-dir-relative) implemented ONCE in language-lib (move the analyzer's version to a public `AssetPathUtil`, plus its `GetAssetDir`); consume from all four + `HmrCSharpEmitter`. Tests: emitter emits `"Assets/UI/icon.png"` for a bare path in `Assets/UI/Foo.uitkx`; HMR parity contract. CHANGELOG: bare relatives now resolve (old behavior was broken-at-runtime).

### H-04 — **MED** — fragile synthetic-header markup parsing in HMR
- **Anchors:** `UitkxHmrCompiler.Compile` l.332-355 (+ duplicate in `BuildComponentArtifacts`): `parseMarkup` prepends `"@namespace __Tmp\n@component __Tmp\n"` and relies on DirectiveParser REJECTING it into the fallback `DirectiveSet` (MarkupStartIndex=0) + markup-parser error-skipping of the bogus lines. Three accidental behaviors chained; U-09's nearby work could silently break HMR splicing.
- **FIX RECIPE:** add `UitkxParser.ParseFragment(string jsxText, string filePath, int startLine, List<ParseDiagnostic> diags)` building the minimal `DirectiveSet` internally (the VDG already builds exactly this with `directives with {…}`, `VirtualDocumentGenerator.cs:813-820` — reuse). HMR calls it via one cached `MethodInfo` with a null-fallback to the old path for older DLLs; VDG + `DiagnosticsPublisher.Publish`'s setup-JSX loop switch to it. Rebuild committed DLLs.

### H-06 — **LOW** — stale docs + micro-issues
- `UitkxHmrModuleStaticSwapper.cs` header "does NOT cover static methods in modules" is stale — `UitkxHmrModuleMethodSwapper` exists and is called (`UitkxHmrController.cs:543`). Update.
- `TryResolveMissingDependencies` → `ProcessFileChange` recursion (l.889) technically violates the queue's "one in flight" doc — safe today; comment or route through the queue.
- (GC-per-compile note moved to the optimization doc.)

---

## 4. P2 — LSP server & IDE clients

### U-14 — VS Code client never syncs `.cs` buffers → companion-overlay machinery dead in VS Code
- **Anchors:** `ide-extensions~/vscode/src/extension.ts:107-112` (selector = uitkx only; watcher = `**/*.uitkx` only). Server supports `.cs` (`TextSyncHandler.cs:37-49`).
- **FIX RECIPE:** add `{ scheme:'file', language:'csharp' }` to the selector + a `**/*.cs` watcher (array form). Verify no perf cliff opening a large `.cs` (server fast-paths them). VS2022: `[LanguageClientContentType("uitkx")]` only — add a `csharp` content-type mapping and verify our server never publishes diagnostics for `.cs` (it doesn't). Rebuild + repackage both extensions.

### U-15 — companion overlay consumed-on-read + never cleared on close  *(pair with U-14)*
- **Anchors:** `Roslyn/RoslynHost.cs:1109` `_companionOverlay.TryRemove(...)` in the rebuild; `TextSyncHandler` DidClose only removes `_files` entries.
- **FIX RECIPE:**
  1. Read companions via `_documentStore.TryGetByPath` FIRST in `UpdateWorkspace`'s companion loop (l.1102-1133), then disk; keep `_companionOverlay` only for the rename-flow shim (`RefreshCompanionDocument` l.695-699) or delete it if that path can also use the store.
  2. DidClose on `.cs`: call a new `RoslynHost.ClearCompanionOverlay(path)` and re-publish dependents (same loop as DidChange's `.cs` branch).
  3. Unit-test the resolution helper in `lsp-server/Tests`.

### U-18 — CRLF file → formatted block becomes LF (mixed EOL)
- **Anchors:** `FormattingHandler.cs:79-118`.
- **FIX RECIPE:** `bool crlf = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0;` → join `newText` with `crlf ? "\r\n" : "\n"`. Test with a CRLF doc asserting `\r\n` in the edit.

### U-19 — VS Code tabSize sync walks UP from the workspace root (misses in-project configs)
- **FIX RECIPE:** replace the upward walk in `extension.ts findConfigIndentSize` with `vscode.workspace.findFiles('**/uitkx.config.json', '**/{Library,node_modules,Temp}/**', 1)` (make async). Keep the watcher.

### U-37 — component rename doesn't rename the `.uitkx` file → instant UITKX0103 errors
- **Anchors:** `RenameHandler.cs` `CollectComponentRenameEdits` (l.751) + `RenameComponentDeclarationInFile` (l.795) — TEXT edits only.
- **Failure:** F2 `Counter`→`Timer`: edits apply, `Counter.uitkx` now declares `component Timer` → UITKX0103 Error immediately; SG hint-name/registry stale until manual file rename.
- **FIX RECIPE:**
  1. When the component-name path fires and the declaring file's stem equals the old name, return edits via `WorkspaceEdit.DocumentChanges` and append `RenameFile { OldUri, NewUri: dir/NewName.uitkx }` LAST (TextDocumentEdits first — they reference the old URI).
  2. Gate on `ClientCapabilities.Workspace.WorkspaceEdit.ResourceOperations` containing `rename`; else keep today's behavior + `window/showMessage` telling the user to rename the file.
  3. Do NOT auto-rename companions in v1; message listing companions still referencing the old stem.
  4. VS2022's RenameFile support is spotty — test; keep the fallback.
  5. Test: WorkspaceEdit shape (order, capability gating) in `lsp-server/Tests`.

### U-38 — hook rename only scans the declaring DIRECTORY; hooks are consumed workspace-wide
- **Anchors:** `RenameHandler.CollectHookRenameEdits` (l.869-899): non-recursive same-dir enumeration; consumption is workspace-wide (`EnrichWithPeerHookUsings`, `HookContainerRegistry.Seed`).
- **FIX RECIPE:** enumerate the same set the workspace index knows (reuse `CollectComponentRenameEdits`'s walk + the U-16 exclusions from the optimization doc); rename token-boundary `oldName(` occurrences using the U-10 comment/string-aware matcher. Keep `skipFilePath` dedupe. Test: cross-directory call site renamed; commented `// useCounter()` NOT renamed.

### U-39 — DiagnosticMapper suppresses ANY `CS1503` mentioning `Func<` → hides REAL user errors
- **Anchors:** `Roslyn/RoslynDiagnosticMapper.cs:129-137`; batch-wide CS1662 cascade suppression l.109-117 + 144-145.
- **Failure:** `items.Select(5)` → real CS1503 mentioning `Func<` → dropped; no squiggle while the build fails. One state-setter call in a file can also hide an unrelated real CS1662.
- **FIX RECIPE:**
  1. Tighten to `msg.Contains("__StateSetter__")` (probe once: type `setCount(5)`, log the CS1503 message, confirm the scaffold marker appears).
  2. CS1662: suppress only when its mapped span OVERLAPS a suppressed state-setter CS1503 span (collect spans in the pre-scan, not a boolean).
  3. Mapper unit tests: state-setter CS1503 dropped; LINQ CS1503 kept; CS1662 overlapping vs not.

### U-40 — rename text-scan comment-blind + unfiltered workspace walk (LOW)
- `RenameTagUsages` (l.824) renames `<OldName` inside markup comments/strings; `CollectComponentRenameEdits` (l.764) walks the root with only tilde filtering. RECIPE: apply the U-16 exclusion helper; for comments either restrict matches to code spans (U-10 helper) or explicitly document that commented-out tags rename too (arguably desirable) in the handler doc-comment — DECIDE and write it down.

### U-41 — stale validator docs (LOW)
- `SourceGenerator~/Emitter/StructureValidator.cs:15-19` doc lists "UITKX0009/0017"; actual descriptors are 0106/0108-aligned. Fix the comment (severity part is handled by U-12).

### U-42 — **NEW (v5)** — Definition/References peer reads bypass the DocumentStore (stale-jump)
- **Anchors:** `DefinitionHandler.cs:470` + `:481` and `ReferencesHandler.cs:361` — peer `.uitkx` sources read via `File.ReadAllText(peerPath)` with NO `_store.TryGetByPath` check first (unlike the handlers' own primary-document reads at `DefinitionHandler.cs:63-67`, which correctly prefer the store).
- **Failure:** the target peer file is open with unsaved edits (e.g. you just added a hook above the one you're navigating to) → go-to-definition/references computes the declaration line against the STALE disk copy → the jump lands on the wrong line.
- **FIX RECIPE:** at each of the three sites, `if (_store.TryGetByPath(peerPath, out var live)) peerSource = live; else peerSource = File.ReadAllText(peerPath);` (the helper already exists on `DocumentStore`). Sweep both handlers for any remaining bare `File.ReadAllText` on paths that can be open documents (`ReferencesHandler.cs:440/508` already do it right — use them as the template). Test: unit-test the store-preference helper; manual: edit a peer hook file without saving, F12 from a consumer → correct line.

### LSP smalls (one cleanup commit)
- `DiagnosticsPublisher._lastT1T2/_lastT3`: evict on DidClose (`Forget(path)` from `TextSyncHandler`); key `OrdinalIgnoreCase`.
- `ScheduleDebouncedRevalidation` CTS race → `Interlocked.Exchange`.
- DidClose: push empty diagnostics for the closed uitkx URI.
- `RoslynHost.EnqueueRebuild` timer swap race → small lock on `state`.

---

## 5. P2 — Source generator

### U-20 — Consolidate the six drifting mini-lexers (root-cause refactor — DO FIRST)
- **The six:** `ExpressionExtractor` (own skippers — **`$@"` drift confirmed**: l.120-142 handles `@$"` but lexes `$@"` as NON-interpolated verbatim), `ReturnFinder.TrySkipNonCodeSpan` (correct), `DirectiveParser.TrySkipNonCodeSpan` (private dup) + `TrySkipStringOrCharLiteral` (most complete), `AstFormatter.ScanParens`/`CollapseIntraLineSpaces`/brace counters (string-blind — U-03), `CSharpEmitter.FindLastTopLevelStatementBoundary`, `DiagnosticsPublisher.FindEnclosingScopeEnd`.
- **FIX RECIPE:**
  1. Create `ide-extensions~/language-lib/Parser/CSharpLexFacts.cs`: `TrySkipNonCode(string, ref int, int limit)` (copy `DirectiveParser.TrySkipStringOrCharLiteral` + comment handling — most complete: both `$@`/`@$`, interpolation-hole recursion), `FindMatchingBrace`, `FindMatchingParen`, `int[] BuildLineStarts(string)`, `OffsetToLineCol`, and U-03's `ComputeMultilineStringLineMask`.
  2. Migrate consumers ONE at a time, both test suites between each: (a) ExpressionExtractor (regression test first: `FromBrace` on `{ $@"{x.ToString("D")}" + "(" }` must balance); (b) ReturnFinder; (c) DirectiveParser; (d) AstFormatter (with U-03); (e) CSharpEmitter; (f) DiagnosticsPublisher.
  3. `LexFactsTests.cs` torture inputs: `$@"{a("(")}"`, `@$"…"`, `'A'`, `"\\"`, nested holes, unterminated everything.
  4. Rebuild committed DLLs. Model the test mechanism on the Godot repo's `scanner-cases.json` cross-impl contract.

### U-22 — `UITKX0000` "generator alive" ships as Warning — VERIFY THEN DEMOTE
- **Anchors:** `UitkxGenerator.cs:40-48` (Warning, enabled, per assembly at l.243); no suppression anywhere.
- **RECIPE:** open the store-shell project console after a recompile; if UITKX0000 shows, demote `defaultSeverity` to `Info`; fix the stray "UITKX0001" comment at l.242.

---

## 6. P3 — smaller parser/tooling items (one cleanup batch)

| ID | Anchor | Item + one-line recipe |
|---|---|---|
| U-24 | `UitkxParser.ParseControlBlockBody:166-179` | unbalanced `{` recovery emits no diagnostic in snippet mini-parses → add "'}' closing the directive body" error. |
| U-25 | `UitkxParser.IsJsxInBraces:1378` | `attr={(<Tag/>)}` typed as opaque C# — extend the peek to skip one `(`+ws before checking `<`. |
| U-26 | `MarkupTokenizer.TryConsume(string)/AdvanceTo` | CRLF overshoot latent trap → `Debug.Assert(!s.Contains('\r'))` + doc comment. |
| U-27 | `ExpressionExtractor.SkipCharLiteral:327` | `'A'`/`'\x41'` mis-skip → skip escape then scan to next `'` (bounded). Covered by U-20 tests. |
| U-28 | `ExpressionExtractor.FindMatchingClose` unclosed | return a `found` flag; parser emits "unclosed '{' expression" at the open brace. |
| U-29 | TextNode line = end line | capture `_scanner.Line` BEFORE `ReadTextContent`. |
| U-30 | Dead code (verified unreferenced) | delete `UitkxParser.LineAtPos`(1315), `AdvanceScannerTo`(1523), `SemanticTokensProvider.s_codeBodyTokenRegex`/`IsLikelyEmbeddedMarkupLine`/`IsLikelyMarkupCloserLine`/`OffsetToLine1`, the `ExpressionNode → EmitKeyword("@(")` case. |
| U-31 | SemanticTokensProvider ignores now-populated `SourceColumn`; first-occurrence `FindOnLine` collides on duplicate names per line | use `SourceColumn` when > 0, search fallback; fix stale header comment. |
| U-32 | AstCursorContext single-line | document; optional bounded look-back heuristic (≤5 lines) for an unclosed `{`. |
| U-33 | AstNode body-payload duplication ×5; `SwitchNode` missing `SwitchExpressionOffset` | extract `ControlBlockPayload` record; `ParseSwitch` → `ReadParenExpressionWithOffset`, store offset, wire VDG mapping (grep `ConditionOffset` usage and mirror). |
| U-34 | `WorkspaceIndex.s_propPattern` misses expression-bodied props | extend regex to `=>` bodies. |
| U-35 | ~~Cargo.toml / tscn_stable.html~~ | **RESOLVED + PUSHED** (a909714). Remaining: remove the `tscn_stable.html` entry from master's `config.json` `pathsToOmitFromStore` on merge. |
| C-03 | `FiberFunctionComponent.ScheduleEffect:601` `// TODO: Use proper scheduler` — effects run inline | investigate callers FIRST: legacy-only → delete; reachable during commit → route through `_pendingPassiveEffects`. Reconciler timing — do not change blindly. |
| C-05 | `PropsApplier.cs:669` cursor TODO | document as unsupported in `uitkx-schema.json` so the LSP flags it instead of silently ignoring. |
| U-43 | `language-lib/Lowering/CanonicalLowering.cs` (v5) | the entire lowering stage is a documented NO-OP pass-through ("simply returns the parsed roots unchanged") still called from 3 sites (DiagnosticsPublisher, HMR compiler, pipeline). Delete the stage + call sites, or keep ONLY if a lowering pass is genuinely planned — then say so in the doc comment with a date. Zero risk either way. |

Formatter behavior notes (non-corrupting, document only): 1-blank-line normalization between top-level siblings (EndLine untracked); `CollapseIntraLineSpaces` collapses inside one-line `/* */` interiors; `Ln()` multi-line re-anchoring.

---

## 7. Execution order

1. **U-20 lexer consolidation** (unlocks U-03, U-04, U-07, U-10, U-27, U-28).
2. **Formatter batch:** U-01, U-02, U-03, U-04, U-36 (+ snapshots per recipe; paste-ready repros above).
3. **HMR batch:** H-01, H-02, H-03 (coordinates with emitters), H-04, H-06.
4. **Grammar batch:** U-05, U-06 (4 sites + parity tests), U-08, U-09, U-10, U-11, U-12 — one release-notes entry.
5. **LSP batch:** U-14+U-15 together, U-18, U-19, U-39 (small, high value — do early), U-42 (three-line fix, same theme as U-15), smalls.
5b. **Rename batch:** U-37, U-38, U-40 — same file, one PR.
6. **SG batch:** U-22 (verify/demote). *(U-21 generator incrementality is in the optimization doc.)*
7. **Cleanup sweep:** section 6.

Performance work (scan exclusions U-16, keystroke I/O U-17, generator incrementality U-21, HMR assembly leak H-05, core allocations C-01/C-02/C-04/C-06) lives in **`FINAL_AUDIT_UITKX_OPTIMIZATIONS.md`** with its own order; U-16's exclusion helper is referenced by U-38/U-40 here — build it first if the rename batch precedes the optimization batch.

## 8. Cross-repo note
The Godot repo is the reference implementation for: single-lexer + contract-case testing (U-20), token-boundary hook detection (`guitkx.gd _find_hook_call` — U-10), formatter trivia preservation (U-01/U-02), paren-wrapped `@case` values (U-04 context), HMR ok/error gating invariant (H-01). Its audit lives at `plans/FINAL_AUDIT_GODOT_FINDINGS.md` in that repo.
