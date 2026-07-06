# ReactiveUIToolKit — Final Audit: OPTIMIZATIONS & PERFORMANCE (v4 split)

**Date:** 2026-07-06. Companion to `FINAL_AUDIT_UITKX_FINDINGS.md` (correctness bugs live there). This file holds every performance/memory/scalability item from the audit — core runtime AND tooling. IDs are stable across both docs (U-16, U-17, U-21, H-05, C-01, C-02, C-04, C-06 moved here unchanged; O-## items are perf observations that were embedded inside findings text in earlier revisions, now given their own IDs).

**Ground rules for the executor:** measure before and after (numbers in the PR description); never trade correctness for speed; all changes here are behavior-preserving — if a test changes, you broke something. Rebuild committed analyzer DLLs after language-lib changes (`scripts/build-generator.ps1`).

**Core verdict up front:** `Shared/` is already strongly optimized — pooled `VNode.__Rent` with generation stamps, `Style` as a double-ulong bitmask with typed fields, thread-static reconciliation maps (OPT-25), bailout cloning, ~2 ms time-sliced work loop, profiler samplers. LINQ is confined to cold paths (`RouteRanker`, tree-view adapters, one `FiberFactory` use). The items below are the residue, not a rework.

---

## 1. Core runtime (`Shared/`)

### C-01 — Text vnodes allocate a Dictionary per reconcile visit  *(largest remaining steady-state alloc in the diff path)*
- **Anchors:** `Shared/Core/Fiber/FiberChildReconciliation.cs:494-498` `ExtractProps` — `case VirtualNodeType.Text: return new Dictionary<string, object> { { "text", … } };` — runs on EVERY reconcile visit of every text node.
- **RECIPE:** in the text-fiber update path, compare `vnode.TextContent` against the fiber's stored text directly and skip props extraction for text nodes entirely; where a props map is structurally required, use a pooled single-entry map keyed by generation (mirror the `VNode.__Rent` pattern). Verify with the Profiler samplers (`Fiber.RenderPhase`) on a text-heavy screen: GC alloc per re-render should drop to ~0 for unchanged text.

### C-02 — `ExtractProps` duplicated ×3 (drift risk + triple maintenance)
- **Anchors:** `FiberChildReconciliation.cs:482`, `FiberFactory.cs:287`, `FiberFunctionComponent.cs:361`.
- **RECIPE:** consolidate into one internal helper (do WITH C-01 so the text-path special case lands once). Pure refactor; suites must stay green untouched.

### C-04 — `FiberReconciler.MetricsEmitted` static event can pin torn-down hosts (editor)
- **Anchors:** `Shared/Core/Fiber/FiberReconciler.cs:76`.
- **RECIPE:** clear subscribers on `UnmountRoot`, or subscribe weakly from Diagnostics; add an editor-side `[InitializeOnLoadMethod]` reset for domain reloads. Editor-memory hygiene, not a player concern.

### C-06 — Keep LINQ out of the hot paths (guard, not a change)
- Current state is clean (`ReconcileChildren`/`CommitWork` LINQ-free). Add a reviewer note (or an analyzer rule if cheap) that `Shared/Core/Fiber/**` stays LINQ-free.

---

## 2. LSP server — keystroke-path and startup costs

### U-16 — Workspace scan walks `Library/`+`Temp/`+`obj/` with no exclusions
- **Anchors:** `ide-extensions~/lsp-server/WorkspaceIndex.cs:517-560` `ScanDirectory` — `Directory.EnumerateFiles(root, "*.cs", AllDirectories)` + `File.ReadAllText`+regex per file; only `~`-dirs skipped (`IsInsideTildeFolder`).
- **Cost:** workspace root = Unity project root (the normal case) → the scan reads every `.cs` under `Library/PackageCache` (easily 10-50k files), delaying `HasCompletedInitialScan` (which gates UITKX0105/0109) by minutes on big projects and bloating `_allCsFiles` (feeds `RoslynHost.FindCompanionFiles`).
- **RECIPE:** `s_excludedDirs = {"Library","Temp","Logs","obj","bin","node_modules",".git",".vs"}` + an `IsExcluded(path)` segment check beside `IsInsideTildeFolder`; replace `AllDirectories` with a manual recursive walk that PRUNES excluded dirs (enumeration itself must not descend). Apply in both scan loops and `WatchedFilesHandler`. Log scan duration before/after. **Note:** `RenameHandler.CollectComponentRenameEdits` (findings U-38/U-40) reuses this helper — build it first if the rename batch runs earlier.

### U-17 — Keystroke-path disk I/O (three sites)
1. `DirectiveParser.InferFunctionStyleNamespace` (`DirectiveParser.cs:1043-1065`) — `File.Exists`+`ReadAllText` of the companion `.cs` on EVERY parse when the file has no `@namespace` (per keystroke via `DiagnosticsPublisher.Publish`, plus every SG run).
   **RECIPE:** `ConcurrentDictionary<string,(DateTime mtime,string ns)>` keyed by companion path; consult `File.GetLastWriteTimeUtc` (one stat on hit, no read). Clear when > 512 entries. (Fuller fix — host-supplied namespace via DocumentStore — later; don't block.)
2. `DiagnosticsAnalyzer.CheckAssetPaths` (`DiagnosticsAnalyzer.cs:1314-1404`) — `File.Exists` per `Asset<>`/`@uss` occurrence per keystroke; regex also matches inside comments.
   **RECIPE:** existence cache keyed by resolved path with ~2 s TTL; invalidate via `WatchedFilesHandler`; run the regex only over code spans (reuse the findings-doc U-10 span helper).
3. `ConfigLoader` re-reads `uitkx.config.json` per format request — leave (user-triggered).

### O-01 — Language-lib scanner micro-costs (parse runs per keystroke)
- `ReturnFinder.TryReadKeywordAt` allocates a `Substring` per depth-0 scan position (`ReturnFinder.cs:250-263`); same pattern in `DirectiveParser.TryReadKeywordAt`. **RECIPE:** compare `source[i] != keyword[0]` first, then `string.CompareOrdinal(source, i, keyword, 0, keyword.Length)` — zero alloc. Lands naturally inside the U-20 lexer consolidation.
- `UitkxParser.ColAtPos`/`ReturnFinder.LineAtPos` are O(line)/O(n) per call, called per node/range; `DirectiveParser` re-runs `FindJsxBlockRanges`+`FindBareJsxRanges`+`LineAtPos` over the same body ranges multiple times per parse. **RECIPE:** `CSharpLexFacts.BuildLineStarts` once per document + binary-search lookups; compute JSX ranges once per body and thread them. Target: parse cost linear in file size; verify by timing `DirectiveParser.Parse+UitkxParser.Parse` on a 2k-line file before/after (probe harness).
- `VirtualDocumentGenerator` re-runs the range finders per expression — same fix via threading the shared ranges.

### O-02 — `DocumentStore.TryGetByPath` linear scan
- `DocumentStore.cs:38-59` walks all open docs with try/catch per entry. Fine at IDE scale (tens of docs); note only — no action unless profiling says otherwise.

---

## 3. Source generator

### U-21 — Incremental generator is effectively non-incremental
- **Anchors:** `SourceGenerator~/UitkxGenerator.cs:144-260` — single `RegisterSourceOutput` over `(projectRoot × allUitkxTexts × CompilationProvider)` reprocesses ALL files on every compilation change (every keystroke in IDE scenarios); the peer prescan parses each file TWICE (`TryBuildPeerComponentInfo` + `TryBuildPeerHookContainerInfo` both call `DirectiveParser.Parse`), then `UitkxPipeline.Run` parses a third time.
- **RECIPE (staged; stage 1 alone is most of the win):**
  1. **Merge the triple parse:** one `TryBuildPeerInfo(source, path, out both)` with a single `DirectiveParser.Parse`; pass the parsed `DirectiveSet` into `UitkxPipeline.Run` via a new optional parameter (keep the old path when null). 3 parses → 1.
  2. **True incrementality:** per-file transform nodes — `AdditionalTextsProvider.Where(uitkx).Select(ParseToPeerInfoAndSource)` (cacheable per file), `.Collect()` only for the peer array; combine each file node with a REDUCED compilation projection (audit `PropsResolver`'s actual `Compilation` usage first; if it needs live symbol lookups, keep `CompilationProvider` only on the emit node — parse stays cached).
  3. Verify with `GeneratorDriver` + `TrackIncrementalGeneratorSteps` in a new cacheability test.
- **Also:** `InferFunctionStyleNamespace` file I/O inside the generator (see U-17.1) breaks incremental purity — the cache fixes both.

---

## 4. HMR

### H-05 — Every swap leaks one non-collectible assembly (inherent to Mono)
- **Anchors:** `UitkxHmrCompiler.InProcessCompile` `Assembly.LoadFrom` (l.2333); no collectible ALCs on Unity's Mono. Memory telemetry exists (`SessionMemoryDeltaMB`).
- **RECIPE (optional):** EditorPrefs threshold `UITKX_HMR_AutoReloadAfterSwaps` (default 0 = off) triggering the existing `RequestDomainReloadSafe` after N swaps; surface `SessionMemoryDeltaMB` prominently in `UitkxHmrWindow` with a "Reload now" button. Document in AUTOMATION.md.

### O-03 — `GC.Collect(2, Optimized)` per HMR compile
- **Anchors:** `UitkxHmrCompiler.cs:2352`. Deliberate (Mono heap growth) but a forced gen-2 per save adds pause time to every swap.
- **RECIPE:** measure a 10-save burst with/without; if it stutters, gate to every Nth compile or to `SessionMemoryDeltaMB` growth. Keep the comment either way.

### O-04 — USS fan-out compile cost
- After the findings-doc H-02 fix (one compile per editor tick), a `.uss` shared by N components still costs N Roslyn compiles total. If that's ever felt: batch dependents into ONE compilation (the union-compile machinery `BuildComponentArtifacts` still exists for exactly this shape). Defer until a real project complains.

---

## 5. Formatter / misc tooling

### O-05 — Formatter allocation profile
- `AstFormatter` splits/substrings heavily (per-emission `code.Split('\n')`, `Substring` in loops). Format is user-triggered (save), so this is LOW priority — do nothing until U-01…U-04/U-36 land; then, if large-file format feels slow, convert the split loops to span-based line walking inside the already-touched methods.

### O-06 — `SemanticTokensProvider` line-search
- `FindOnLine`/`FindOpenTagName` do per-token string searches; switching to the parser-provided `SourceColumn` (findings U-31) removes most of the searching as a side effect — no separate work needed here.

---

## 6. Suggested order & measurement

1. **O-01 + U-17.1** land inside/alongside the U-20 lexer consolidation (findings doc) — same files, one measurement pass: time `Publish()` on a 2k-line document before/after (target: >5× on the parse-path, no I/O on the hot path).
2. **U-16** (scan pruning) — measure cold-start `HasCompletedInitialScan` time on a real project with Library present.
3. **U-21 stage 1** (triple parse → 1); stage 2 when an IDE-latency complaint justifies it.
4. **C-01/C-02** (one PR) — profiler capture on a text-heavy screen.
5. **H-05/O-03/O-04** — only with measurements; these trade complexity for headroom.
6. C-04, O-02, O-05, O-06 — opportunistic.
