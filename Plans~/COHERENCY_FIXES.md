# COHERENCY_FIXES — Audit Follow-up Action Plan

> **Status:** Partially executed (see Execution Record below)
> **Created:** April 27, 2026
> **Origin:** Cleanup pass after the April 26 disco-bug + ErrorBoundary-loop fixes.
>   See chat history for the original audit findings.

This plan catalogs the six issues surfaced by the post-fix coherency audit,
the result of deeper research into each, and the recommended approach. The
items are ordered from highest to lowest user-facing impact.

Throughout: file references use workspace-relative paths.

---

## Execution Record (April 27, 2026 pass)

| # | Status | Notes |
|---|---|---|
| 1 | ✅ **DONE** | 7 source-gen IDs renumbered; tests + pipeline comment + migration guide updated; CHANGELOG entry added under `[Unreleased]`. SourceGen tests: 111/111 pass. LSP-server tests: 57/57 pass. |
| 2A | ⚠️ **DEFERRED — original diagnosis was wrong** | After re-reading [AstFormatter.cs:55-91](ide-extensions~/language-lib/Formatter/AstFormatter.cs#L55-L91): the formatter is **already designed to fail-safe** — it explicitly returns `source` unchanged on parse errors with the documented contract "the formatter can never corrupt a file." Roslyn-pass exceptions are also already caught at lines 2057/2094/2360 with `/* best-effort */`. So the "FormattingHandler swallows exceptions silently" framing was incorrect. The user's specific scenario (cannot format file with `__UitkxRef__` semantic error) needs **live reproduction** to determine which of: (a) parse-error short-circuit kicks in, (b) `formatted == text` no-op (file already canonical), (c) some other code path. Cannot safely modify without that repro. **Recommendation:** open a dedicated investigation issue and capture the exact LSP Trace output during a failed save before changing anything. |
| 2B | ⚠️ **DEFERRED — TD-S1, multi-day workstream** | Touches the IDE virtual-document shape that intellisense, hover, completion, classification, and analyzer all depend on. Already documented as a known issue in [TECH_DEBT_SAMPLES_AND_RUNTIME.md](Plans~/TECH_DEBT_SAMPLES_AND_RUNTIME.md#L62-L77). Not low-risk. |
| 2C | ✅ **DONE** | Unused `int beamH` deleted from [GameScreen.uitkx:102](Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx). MarioGame HUD audit not yet run — see follow-up below. |
| 3 | ✅ **DONE** | `ContextProviderId` field removed from [FiberNode.cs](Shared/Core/Fiber/FiberNode.cs) and the one copy in [FiberFactory.cs](Shared/Core/Fiber/FiberFactory.cs). All tests pass. |
| 4 | ⚠️ **PARTIAL — only the truly safe sub-fix applied** | Applied: `old.Cancel()` → `await old.CancelAsync()` at [RoslynHost.cs:778](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs). **Deferred:** the rest of the async refactor. Reasoning: [HoverHandler.cs:316](ide-extensions~/lsp-server/HoverHandler.cs#L316) explicitly comments *"Use synchronous GetAwaiter().GetResult() — HoverHandler.Handle is synchronous"* — this is a deliberate design decision, not an oversight. The LSP server runs as a console EXE with no `SynchronizationContext`, so there is no actual deadlock risk; the warnings are pure future-proofing. Converting `MapRoslynLocation`, `CollectRoslynReferences`, `GetLatestDiagnostics`, and the 3 hover sites to async is a real refactor that touches public signatures and ~10 test files, with the only concrete benefit being silencing pure-hygiene warnings. Not appropriate for a "do the safe items" pass. |
| 5 | ✅ **DONE** | `<NoWarn>$(NoWarn);RS2008</NoWarn>` added to [UitkxLanguageServer.csproj](ide-extensions~/lsp-server/UitkxLanguageServer.csproj). Build verified clean (0 warnings). |
| 6 | ✅ **DONE** | `chatHistory` / `globalState.keys()` debug block removed from [extension.ts](ide-extensions~/vscode/src/extension.ts). Server-resolution logs and `[Formatting]` traces preserved (genuine diagnostics). |

### Outstanding follow-ups for a future pass

1. **MarioGame HUD prop-mismatch audit** — verify whether MarioGame's `<HUD .../>` call site passes only the `score` and `lives` props its signature declares, or whether it also passes extras that the source generator silently drops.
2. **#2A — investigate user's actual failed-save scenario** — capture the LSP Trace output ("UITKX LSP Trace" channel in VS Code) the moment a save no-ops on a broken file. That output will identify exactly which branch the formatter hits.
3. **#2B — TD-S1 virtual-doc Ref<T> unification** — schedule as a dedicated workstream with an IDE-wide regression test pass.
4. **#4 — full LSP async refactor** — only worth doing if the LSP server is ever embedded into an in-process VS host (where a SyncContext would create real deadlock risk). Otherwise the existing pragma + design comment in HoverHandler is correct.

---

## At-a-Glance

| # | Item | Impact | Risk | Effort |
|---|------|--------|------|--------|
| 1 | Diagnostic-ID duplication (SourceGen ↔ Analyzer) | High — user-facing | Low (mechanical, one-shot breaking change for v1.x) | 2–3 h |
| 2 | Format-on-save silently no-ops on parse-error files (+ TD-S1 IDE false positive + sample dead code) | High — user can't recover indent drift; blocks save UX | Medium (LSP handler + virtual-doc shape) | 4–8 h |
| 3 | Dead field `FiberNode.ContextProviderId` | Low — 4 bytes/fiber + reader confusion | Trivial | 5 min |
| 4 | LSP server async/sync warnings (5 of them) | Low — dormant deadlock surface | Medium (async refactor) | 1–2 h |
| 5 | RS2008 analyzer-release-tracking warning | Trivial — build noise | Trivial | 5 min |
| 6 | Stale `chatHistory` debug logging in `extension.ts` | Trivial — Output channel spam | Trivial | 5 min |

**Recommended low-risk batch (≈30 min, near-zero risk):** 6 → 3 → 5 → 1 → 2C (sample dead code).
**Higher-priority follow-up:** 2A (the silent format-on-save no-op — user-blocking).
**Medium-effort follow-up:** 2B (TD-S1 virtual-doc Ref<T>), 4.

---

## Item 1 — Diagnostic-ID Duplication

### The actual situation (corrected after research)

There are TWO distinct diagnostic registries:

- **Source generator** ([SourceGenerator~/Diagnostics/UitkxDiagnostics.cs](SourceGenerator~/Diagnostics/UitkxDiagnostics.cs)) — `Microsoft.CodeAnalysis.DiagnosticDescriptor` instances reported through the Roslyn `SourceProductionContext`. These appear in **Unity Console** at compile time. ID range used: `UITKX0001`–`UITKX0023`, `UITKX0300`–`UITKX0305`, `UITKX2100`–`UITKX2104` (function-style directive errors).
- **Live analyzer** ([ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs](ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs)) — `ParseDiagnostic` records emitted by `DiagnosticsAnalyzer` and shipped via the LSP `publishDiagnostics`. These appear in the **VS Code Problems pane**. ID range used: `UITKX0013`–`UITKX0016` (intentionally shared with source-gen for hooks), `UITKX0101`–`UITKX0112`, `UITKX0120`–`UITKX0121`, `UITKX0200`, `UITKX0300`–`UITKX0306`.

The convention is documented in [DiagnosticCodes.cs](ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs#L5-L15). Hooks (`UITKX0013`–`UITKX0016`) are intentionally shared. Everything else is split.

### Concept overlaps that have two different IDs

| Concept | Source-gen ID | Analyzer ID | Note |
|---|---|---|---|
| Filename ≠ component name | `UITKX0006` | `UITKX0103` | Same warning text |
| Unknown attribute on element | `UITKX0002` | `UITKX0109` | Same warning text |
| Element inside loop missing `key` | `UITKX0009` | `UITKX0106` | Same warning text |
| Duplicate sibling key | `UITKX0010` | `UITKX0104` | Same warning text |
| Multiple root elements | `UITKX0017` | `UITKX0108` | Same error text |
| Asset path not found | `UITKX0022` | `UITKX0120` | Same error text |
| Asset type mismatch | `UITKX0023` | `UITKX0121` | Same warning text |

### Items that ONLY one side reports (do not unify)

- **Source-gen-only:**
  - `UITKX0001` — unknown built-in element (lowercase). Analyzer's `UITKX0105` checks **PascalCase** custom-component names.
  - `UITKX0005` — missing required `@namespace`/`@component`. Analyzer splits this into `UITKX0101` + `UITKX0102`.
  - `UITKX0008` — unknown function component (PascalCase, semantic). Analyzer's `UITKX0105` is the syntactic equivalent.
  - `UITKX0012` — directive ordering (`@namespace` after `@component`).
  - `UITKX0018` — `UseEffect` missing dep array.
  - `UITKX0019` — loop variable used as `key`.
  - `UITKX0020`/`UITKX0021` — `ref={}` routing.
- **Analyzer-only:**
  - `UITKX0107` — unreachable after `return`.
  - `UITKX0110` — unreachable after `@break`/`@continue`.
  - `UITKX0111` — unused parameter.
  - `UITKX0112` — unused variable (Roslyn data-flow).
  - `UITKX0200` — Unity version compatibility.

### Recommended approach

**Renumber the 7 source-generator descriptors** to use the analyzer's `01xx` codes for the overlapping concepts. Specifically, in
[SourceGenerator~/Diagnostics/UitkxDiagnostics.cs](SourceGenerator~/Diagnostics/UitkxDiagnostics.cs):

| Field | Old ID | New ID |
|---|---|---|
| `ComponentNameMismatch` | `UITKX0006` | `UITKX0103` |
| `UnknownAttribute` | `UITKX0002` | `UITKX0109` |
| `ForeachMissingKey` | `UITKX0009` | `UITKX0106` |
| `DuplicateSiblingKey` | `UITKX0010` | `UITKX0104` |
| `MultipleRootElements` | `UITKX0017` | `UITKX0108` |
| `AssetFileNotFound` | `UITKX0022` | `UITKX0120` |
| `AssetTypeMismatch` | `UITKX0023` | `UITKX0121` |

The `MissingRequiredDirective` (`UITKX0005`) stays — it's a one-shot composite that the analyzer splits into two. Worth leaving as-is to avoid a no-net-improvement breaking rename.

### Files touched

- [SourceGenerator~/Diagnostics/UitkxDiagnostics.cs](SourceGenerator~/Diagnostics/UitkxDiagnostics.cs) — change the `id:` strings, update the XML doc comment IDs, and update the `ID ranges` summary at the top.
- [SourceGenerator~/Tests/](SourceGenerator~/Tests/) — any test that asserts `d.Id == "UITKX0017"` etc.; spot-checked `EmitterTests.cs` and `ParserTests.cs` — neither asserts these specific IDs, but `DiagnosticsAnalyzerTests.cs` already uses `DiagnosticCodes.MultipleRenderRoots` which is the analyzer ID, so most tests are insulated. Need a grep across `Tests/` for any `"UITKX000[269]"`, `"UITKX0010"`, `"UITKX0017"`, `"UITKX002[23]"`.
- [CHANGELOG.md](CHANGELOG.md) — entry under Breaking Changes describing the 7 renames so users with `// uitkx-disable UITKX0017` style suppressions know to migrate.
- Documentation: search `ide-extensions~/docs/` and `ReactiveUIToolKitDocs~/` for hard-coded references to the old IDs.

### Hidden costs / risks

- **Breaking change for any user who has rule-suppression comments referencing the old IDs.** Concrete impact: probably zero (these IDs were just shipped, suppressions are rare). Mitigated by clear CHANGELOG.
- **No runtime behavior change** — same diagnostic, same location, same severity, same message. The number on the side is the only thing that changes.
- **Tests:** existing `DiagnosticsAnalyzerTests` already use the analyzer codes, so they're insulated. SourceGenerator pipeline tests that check raw `Diagnostic.Id` values need updating — easy grep-and-replace.

### Validation

1. Run all 1029 source-gen tests; replace any failures by updating the asserted ID string.
2. Build the LSP server.
3. Manual: trigger one diagnostic of each renumbered type in a sample `.uitkx` and confirm Unity Console + VS Code Problems both show the new code.

---

## Item 2 — Format-on-Save Silently Disabled When File Has Errors (+ sample defects)

> **Correction note:** The earlier "`&&` / `||` continuation predicate" diagnosis
> was a hallucination on my part — built on a stale `2026-03-12` test trx and an
> inverted reading of the `Idempotency_SampleFile_IsUnchanged` failure (the failing
> sample was `DeferredEffectDemoFunc`, not `GameScreen.uitkx`, and the diff was a
> 2-vs-4 space root-body indent, not `&&`). User correctly pointed this out.
> This section reflects the actual chain of failures observed in
> [GameScreen.uitkx](Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx).

### The actual chain (three layered problems)

#### 2A — Format-on-save silently no-ops on parse-error files (the real blocker)

In [ide-extensions~/lsp-server/FormattingHandler.cs](ide-extensions~/lsp-server/FormattingHandler.cs#L60-L70):

```csharp
try {
    formatted = formatter.Format(text, localPath ?? string.Empty);
} catch (Exception ex) {
    ServerLog.Log($"[Formatting] Format error for '{localPath}': {ex.Message}…");
    return Task.FromResult<TextEditContainer?>(null);   // ← swallows + returns null
}
```

When `AstFormatter.Format` throws (e.g. parser cannot recover from a syntactic
problem caused by a typing-in-progress edit, OR a Roslyn-side issue in
`RoslynCSharpFormatter`), the handler returns no edits. VS Code's
format-on-save sees "no edits" and saves the file un-reformatted. **There is no
user-facing notification.** This is what the user is actually hitting: any file
with an unresolved error becomes un-format-on-save-able, which means simple
indentation drift accumulates and can't be recovered without manually editing.

**Note:** the IDE-only `Ref<T>` / `__UitkxRef__<T>` mismatch (TD-S1, see 2B) is a
*semantic* error reported by the analyzer — it does NOT cause `AstFormatter` to
throw, because the AST formatter is purely syntactic. So the 2A failure mode
is specifically about **parse-time** failures or the Roslyn-pass throwing.
Worth verifying which one is blocking the user's specific save. Two
hypotheses to confirm by reproducing:

- **Hypothesis H1:** `AstFormatter.Format` itself is throwing on a partially-typed
  edit. Fix: catch narrower (`ParseException` etc.), apply lossless fallback
  (return whitespace-only edits OR return `null` but with a `window/showMessage`
  user notification so they don't silently lose formatting).
- **Hypothesis H2:** `RoslynCSharpFormatter.Format` (the C# setup-code pass) is
  throwing because Roslyn cannot syntactically parse the broken setup region.
  Fix: in the Roslyn formatter, on parse-error, return the input unchanged
  rather than throwing — leaving the AST formatter's whitespace pass to still
  apply.

#### 2B — Pre-existing IDE false positive (TD-S1) — affects GalagaGame too

[Plans~/TECH_DEBT_SAMPLES_AND_RUNTIME.md](Plans~/TECH_DEBT_SAMPLES_AND_RUNTIME.md#L62-L77)
already documents this:

> Argument 1: cannot convert from `__UitkxRef__<VisualElement?>` to `Ref<VisualElement?>`

The virtual-document generator
[VirtualDocumentGenerator.cs:282](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs#L282)
emits a private `__UitkxRef__<T>` class for IDE intellisense, but hooks declare
parameters as `ReactiveUITK.Core.Ref<T>`. The compile-time source generator
emits the correct `Ref<T>`, so there's no real bug — but the analyzer pass over
the virtual document sees two distinct types and produces a false positive.
The user encounters it on:

- [GameScreen.uitkx:13](Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx#L13) — `useGalagaGame(boardRef)`
- The pre-existing TD-S1 example in MarioGame's GameScreen.

**Fix scope:** unify the virtual-document `Ref<T>` shape with
`ReactiveUITK.Core.Ref<T>` so the analyzer sees one type. Either:

- Emit `Ref<T>` (the real type, with a `using ReactiveUITK.Core;` in the
  virtual doc) instead of the local `__UitkxRef__<T>` class, OR
- Make `__UitkxRef__<T>` an alias / inherit from the real `Ref<T>` so it's
  assignable.

**Risk:** medium — touches the virtual-doc shape that intellisense and a lot
of analyzer paths depend on. Should be done in its own PR with thorough IDE
smoke testing across every hook and `useRef` shape (object, value-type,
nullable, etc.).

This is already documented as TD-S1 with a "low priority, IDE-only" tag.
Re-classify to **medium-high** because it ladders into 2A: the user can't
format-on-save the file until either 2A or 2B is fixed.

#### 2C — Real sample-quality defects in `GameScreen.uitkx`

Confirmed:

- [Line 102](Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx#L102):
  `int beamH = beamFrame.H * SPRITE_SCALE;` — declared, never used. Dead code.
  Either delete or use (`beamH` is presumably intended in
  `MakeSpriteStyle` somewhere). Nice candidate for a `UITKX0112` analyzer hit
  if the analyzer's unused-variable pass were enabled on `.uitkx` setup code.
- The user mentioned `<HUD spriteSheet={…} score={…} lives={…} wave={…} />`
  potentially mismatching the HUD signature. **Verified false alarm for
  GalagaGame:** [GalagaGame's HUD.uitkx:6](Samples/Components/GalagaGame/components/HUD/HUD.uitkx#L6)
  is `component HUD(Texture2D spriteSheet, int score, int lives, int wave)`
  — all four props match. Confusion came from MarioGame's HUD which has
  the smaller signature `component HUD(int score, int lives)`. **Action:
  audit MarioGame's `<HUD .../>` call site** for an actual prop mismatch.

### Recommended sequencing

1. **2A (real blocker):** Reproduce the user's broken save first. Determine H1
   vs H2 by inserting `Console.Error.WriteLine` / `ServerLog.Log` at strategic
   points. Then fix the throwing path to return either: (a) the input unchanged
   (graceful no-op) WITH a `window/showMessage` warning, or (b) a partial
   formatting result. **Do not change behavior silently.**
2. **2B (TD-S1):** Now that 2A unblocks formatting, fix the Roslyn virtual-doc
   `Ref<T>` mismatch so the IDE error disappears. Bumps TD-S1 to "fixed".
3. **2C (sample cleanup):** Delete the unused `beamH`. Audit MarioGame's
   `<HUD />` call. Spot-check other samples for similar drift while we're in
   the area.

### Hidden costs / risks

- **2A:** Wrapping the broader catch in a smaller one risks letting a real
  exception escape and crash the LSP request handler. Mitigation: keep the
  catch-all but route to a graceful fallback path; log + notify, don't throw.
- **2B:** Changing the virtual-doc shape can break completion / hover / quick-info
  in subtle ways. Need to run the full lsp-server test suite in
  [ide-extensions~/lsp-server/Tests/](ide-extensions~/lsp-server/Tests/) and
  manually smoke-test every hook variant (`useRef`, `useState`, custom hooks).
- **2C:** None — pure dead-code deletion + sample audit.

### Validation

1. **2A:** Reproduce user's stuck save by introducing a typo, then save.
   Confirm: (a) the formatter no longer silently no-ops; (b) a notification or
   trace appears; (c) at minimum, whitespace-only formatting still applies even
   when Roslyn pass fails.
2. **2B:** TD-S1 squiggle disappears in both GalagaGame's and MarioGame's
   GameScreen. Hover/completion still works on `boardRef.Current`. Build and
   runtime unaffected.
3. **2C:** Build clean (no IDE0059 / unused-variable warning). MarioGame's HUD
   call matches its signature.

### Files touched

- 2A: [ide-extensions~/lsp-server/FormattingHandler.cs](ide-extensions~/lsp-server/FormattingHandler.cs),
  possibly [ide-extensions~/language-lib/Formatter/AstFormatter.cs](ide-extensions~/language-lib/Formatter/AstFormatter.cs)
  and [ide-extensions~/language-lib/Formatter/RoslynCSharpFormatter.cs](ide-extensions~/language-lib/Formatter/RoslynCSharpFormatter.cs).
- 2B: [ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs)
  (the two `__UitkxRef__<T>` emissions at lines 282, 333, 353, 500).
- 2C: [Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx](Samples/Components/GalagaGame/components/GameScreen/GameScreen.uitkx)
  (delete `beamH`); audit [Samples/Components/MarioGame/components/](Samples/Components/MarioGame/components/) HUD usage.

---

## Item 3 — Dead Field `FiberNode.ContextProviderId`

### Confirmed dead

Workspace-wide grep:
- Declaration: [Shared/Core/Fiber/FiberNode.cs](Shared/Core/Fiber/FiberNode.cs#L87)
- One copy in [Shared/Core/Fiber/FiberFactory.cs](Shared/Core/Fiber/FiberFactory.cs#L165)
- **Zero reads. Zero writes other than the copy.** Always 0.
- Only other matches are in archived plan docs.

### Fix

Delete the field declaration and the one copy line. That's it.

### Risk

Trivial. C# compiler catches any reference; we just verified there are none.

### Validation

Build succeeds, all 1027 tests pass.

---

## Item 4 — LSP Server Threading Warnings

### Inventory

5 `VSTHRD002` / `VSTHRD103` warnings:

- [ide-extensions~/lsp-server/ReferencesHandler.cs](ide-extensions~/lsp-server/ReferencesHandler.cs#L311) — `mainDoc.GetSyntaxTreeAsync().GetAwaiter().GetResult()` inside `MapRoslynLocation`.
- [ide-extensions~/lsp-server/ReferencesHandler.cs](ide-extensions~/lsp-server/ReferencesHandler.cs#L350) — same idiom in the peer-VDoc loop.
- [ide-extensions~/lsp-server/ReferencesHandler.cs](ide-extensions~/lsp-server/ReferencesHandler.cs#L391) — same idiom in the companion-`.cs` loop.
- Inside `CollectRoslynReferences` (already wrapped in `#pragma warning disable VSTHRD002`):
  `SymbolFinder.FindReferencesAsync(...).GetAwaiter().GetResult()`.
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L357) — `VSTHRD002` (sync wait on a task).
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L778) — `VSTHRD103` (`.Cancel()` instead of `await CancelAsync()`).
- [ide-extensions~/lsp-server/HoverHandler.cs](ide-extensions~/lsp-server/HoverHandler.cs#L115) — `VSTHRD103` (`.GetResult()`).

### Practical impact today

- The LSP server is a console app with no `SynchronizationContext`. So the classic "deadlock by sync-waiting on a task that needs the captured context to complete" scenario cannot occur.
- The VSTHRD warnings are a **future-proofing** concern: if anyone ever runs this inside a sync-context host (e.g. embedding into Visual Studio in-proc), the same code would deadlock.
- Performance impact: each `GetSyntaxTreeAsync().GetResult()` blocks the LSP request thread for the duration of tree retrieval. With Roslyn's caching this is usually fast, but on cold starts a Find-All-References can pause the server for hundreds of ms.

### Recommended approach

Convert the four hot paths to true async:

1. **`MapRoslynLocation` → `MapRoslynLocationAsync`** (returns `Task<LspLocation?>`). The three `GetSyntaxTreeAsync().GetAwaiter().GetResult()` calls become `await` with `.ConfigureAwait(false)`. Caller `CollectRoslynReferences` is already inside an async-friendly path.
2. **`CollectRoslynReferences` → `CollectRoslynReferencesAsync`**. Drop the `#pragma warning disable VSTHRD002` and `await SymbolFinder.FindReferencesAsync(...).ConfigureAwait(false)`.
3. **`RoslynHost.cs:357`** — investigate the specific call; likely a one-line conversion.
4. **`RoslynHost.cs:778`** — `_cts.Cancel()` → `await _cts.CancelAsync().ConfigureAwait(false)`.
5. **`HoverHandler.cs:115`** — convert `.GetAwaiter().GetResult()` to `await ... .ConfigureAwait(false)`.

### Hidden costs / risks

- Async-correctness pitfalls:
  - Forgetting `.ConfigureAwait(false)` is harmless here (no sync-context) but should still be added for hygiene.
  - Forgetting to thread `CancellationToken` — the existing `ct` parameter is already passed everywhere; just keep the chain intact.
  - Exception unwrapping — `.GetAwaiter().GetResult()` propagates the original exception; `.Result` wraps in `AggregateException`. Since the codebase uses `.GetAwaiter().GetResult()`, switching to `await` is exception-equivalent. No callers catch `AggregateException`.
- The handler signatures must remain `Task<T>`-returning (LSP requirement). Already are.
- Async chains are contagious — `MapRoslynLocation` going async means its caller `CollectRoslynReferences` goes async, which means its caller `FindReferencesInCsAsync` (already async) just adds an `await`. Net: ~3 method signatures change.

### Files touched

- [ide-extensions~/lsp-server/ReferencesHandler.cs](ide-extensions~/lsp-server/ReferencesHandler.cs) — 2 method signatures + ~6 `await`s.
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs) — 2 spot fixes.
- [ide-extensions~/lsp-server/HoverHandler.cs](ide-extensions~/lsp-server/HoverHandler.cs) — 1 spot fix.

### Validation

1. `dotnet build` of [ide-extensions~/lsp-server/](ide-extensions~/lsp-server/) shows 0 `VSTHRD002` / `VSTHRD103` warnings.
2. lsp-server tests in [ide-extensions~/lsp-server/Tests/](ide-extensions~/lsp-server/Tests/) pass.
3. Manual: in VS Code, trigger Find-All-References on a symbol used in two `.uitkx` files and confirm both locations are returned.
4. Manual: trigger Hover repeatedly on a symbol to confirm no hangs.

---

## Item 5 — RS2008 Analyzer Release Tracking

### Confirmed harmless

[ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L143) creates a `DiagnosticDescriptor` (`UITKX0112`) at runtime to attach to LSP `publishDiagnostics`. It is **not** consumed by any Roslyn analyzer publication pipeline — the lsp-server is an EXE, not an analyzer NuGet package.

`RS2008` is meant for analyzer assemblies that ship with `AnalyzerReleases.{Shipped,Unshipped}.md` tracking. For an EXE, those files are meaningless ceremony.

### Fix

Add `<NoWarn>RS2008</NoWarn>` to [ide-extensions~/lsp-server/UitkxLanguageServer.csproj](ide-extensions~/lsp-server/UitkxLanguageServer.csproj) `PropertyGroup`. One line.

### Risk

Trivial. We're suppressing a warning that's targeting the wrong project type.

### Validation

`dotnet build` of [ide-extensions~/lsp-server/](ide-extensions~/lsp-server/) — confirm no `RS2008`.

---

## Item 6 — Stale Debug Logging in `extension.ts`

### What's actually there (more nuanced than the audit suggested)

Three categories of logging in [ide-extensions~/vscode/src/extension.ts](ide-extensions~/vscode/src/extension.ts):

- **Stale scaffolding (delete):** lines 52–56 log `globalStorageUri` + `globalState.chatHistory` + `globalState.keys()`. The "chatHistory" key has nothing to do with UITKX — leftover from an unrelated experiment. Always-on, runs on every activation.
- **Useful diagnostic (keep):** lines 67–99 log server-path resolution and command. Genuinely needed when users have setup problems.
- **Verbose formatting trace (gate):** lines 183–204 — `[Formatting]` logs that fire on every format-on-save. Useful for the "DocumentFormattingEditProvider not registering" workaround that's documented in the surrounding comment, but not for end users.

### Recommended fix

- **Delete** the chatHistory block (lines 52–56) outright.
- **Keep** the server-resolution logs as-is.
- **Optionally gate** the `[Formatting]` logs behind the existing `uitkx.trace.server` setting (`if (config.get<string>('trace.server') === 'verbose') output.appendLine(...)`). Low-priority polish.

### Risk

Trivial. `extension.ts` is rebuilt by `npm run build` and bundled into the VSIX; no other code depends on these log lines.

### Validation

Reload the extension; confirm the Output channel no longer mentions `chatHistory`.

---

## Sequencing & Sign-off

### Recommended low-risk batch (≈30 min, near-zero risk, ship before v1.0)

1. **#6** — Delete chatHistory logging. (5 min)
2. **#3** — Delete `ContextProviderId`. (5 min)
3. **#5** — Suppress `RS2008`. (5 min)
4. **#1** — Renumber 7 SourceGen diagnostic IDs. (~2 h, mostly mechanical)

### Higher-risk follow-ups (do later or in a separate PR)

5. **#2A** — Format-on-save silent no-op. Reproduce first, then fix. **User-blocking.** (~2 h)
6. **#2B** — Virtual-doc `Ref<T>` unification (TD-S1). Separate PR. (~3 h)
7. **#2C** — Delete unused `beamH`; audit MarioGame HUD. (~30 min)
8. **#4** — LSP async refactor. Independent of everything else. (~2 h)

### Cross-cutting validation (run after EACH item)

- `dotnet test` on [SourceGenerator~/Tests/](SourceGenerator~/Tests/) — must keep passing 1027 of 1029 (the 2 failing are item #2; resolved when #2 lands).
- `dotnet build` on [ide-extensions~/lsp-server/](ide-extensions~/lsp-server/) — must succeed; warning count strictly non-increasing per item.
- Unity Editor smoke test: launch one of the showcase windows; confirm no new errors.

### What the audit got wrong / refined here

- The audit's "diagnostic IDs are just inconsistent" framing missed that hooks IDs are **already shared on purpose** and the rest is an intentional split. The actual user-visible problem is much narrower than the original bullet suggested — only 7 IDs need to be unified.
- **Item #2 was completely re-scoped after user feedback.** Earlier iteration of this plan claimed a `&&` / `||` continuation bug in `AstFormatter.cs` was the cause of GameScreen.uitkx idempotency test failures. That was wrong on two counts: (a) the failing sample in the available trx was `DeferredEffectDemoFunc`, not GameScreen, and the diff was a root-body 2-vs-4 space indent; (b) the user's actual blocker is the *behavior* of format-on-save when a file has errors — it silently no-ops because `FormattingHandler` catches all formatter exceptions and returns null. Item #2 is now framed around that real chain (2A formatter exception swallow + 2B TD-S1 virtual-doc mismatch + 2C sample dead code).
- The LSP threading warnings are dormant in practice (no sync-context in the host process) — the upgrade is for hygiene + cold-start latency, not to fix an active deadlock.
- The `extension.ts` logging is a mix of "delete" and "keep" — original audit recommendation to "delete the 6 lines" was too broad.
