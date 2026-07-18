# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

ReactiveUIToolKit brings a React-like component model (function components, hooks, a virtual node tree, typed props, a Fiber reconciler) to Unity UI Toolkit — all in C# on top of UI Toolkit. The repo is more than a Unity package: it's a monorepo that also contains a Roslyn source generator for the `.uitkx` markup language, a shared language library, a cross-editor LSP server, four IDE extensions, and a docs website. Full docs: http://reactiveuitoolkit.info/

## The `~` folder convention (read first)

Unity's Asset Database ignores any folder whose name ends in `~`. This is deliberate and load-bearing: everything that is *tooling* rather than *shipped runtime code* lives under a `~` folder so Unity never tries to compile it. When editing, know which world a file is in — the same code is often **linked** into multiple projects (see below), so a change in `Shared/` can affect the generator, the LSP server, and the Unity runtime at once.

| Path | Unity sees it? | Build world |
|---|---|---|
| `Runtime/`, `Shared/`, `Editor/`, `Samples/`, `Diagnostics/`, `CICD/` | yes (asmdefs) | Unity C# |
| `Analyzers/` | yes (committed generator DLLs) | consumed by Unity as an analyzer |
| `SourceGenerator~/` | no | Roslyn generator, `netstandard2.0` (tests: `net10.0`) |
| `ide-extensions~/language-lib/` | no | shared language lib, `netstandard2.0` |
| `ide-extensions~/lsp-server/` | no | LSP server, `net8.0` |
| `ide-extensions~/{vscode,visual-studio,rider}/` | no | IDE extensions (Node/VSIX) |
| `ReactiveUIToolKitDocs~/` | no | Vite + React docs site |

## Architecture

### Runtime rendering pipeline (the Unity side)

The render path is React-shaped: **`V.*` factory calls → `VirtualNode` tree → `FiberReconciler` → `IElementAdapter`s → Unity `VisualElement`s.**

- **`Shared/` is the core** and is host-agnostic. It contains the whole reactive engine: `V.cs` (element/component factories), `VNode.cs` (pooled virtual nodes), `Hooks.cs` + `HookRegistry.cs`, the `Core/Fiber/` reconciler (`FiberReconciler`, `FiberNode`, `FiberRoot`, child reconciliation, commit/effect phases, time-slicing at ~2 ms), `Core/Signals/`, `Core/Router/` (React-Router-style routing), `Core/Refresh/` (Fast Refresh families), `Elements/` (one `*ElementAdapter` per UI Toolkit control, resolved through `ElementRegistry`), and `Props/` (the typed `Style` system + appliers).
- **`Runtime/` is a thin MonoBehaviour adapter** — `RootRenderer` (the `MonoBehaviour` host that mounts a fiber tree onto a `UIDocument`/`VisualElement`) and `RenderScheduler`. Keep host-specific Unity glue here; keep engine logic in `Shared/`.
- **`Editor/` is Unity Editor integration** — HMR (`Editor/HMR/`), the `.uitkx` change watcher/generator trigger, console navigation, csproj post-processing. HMR recompiles `.uitkx` in-editor and hot-swaps delegates/modules without a domain reload (~50–200 ms).

Editor-only defenses (e.g. `RootRenderer`'s `UIDocument` host-rebuild polling for Unity 6.3 `UUM-127851`) are gated behind `#if UNITY_EDITOR` so they compile out of player builds. Preserve those gates.

### UITKX language pipeline (the tooling side)

`.uitkx` is JSX-like markup that a Roslyn source generator compiles to a C# partial class at build time (zero runtime overhead). The pipeline (`SourceGenerator~/UitkxPipeline.cs`) is four stages: **DirectiveParser** (preamble + declarations) → **UitkxParser** (recursive-descent markup → AST) → **PropsResolver** (tag names → `V.*` call patterns) → **CSharpEmitter**/**ExportsEmitter**. Diagnostics use `UITKX####` codes.

Since 0.9.0 (ES-modules redesign): a file IS a module. Plain typed `export` declarations (`export VirtualNode Name(...) {…}`, `export (ret) useX(...) {…}`, `export Style x = …;`) replace the deprecated `component`/`hook`/`module` wrapper keywords (UITKX2320 window; classification is signature-driven). Namespaces are FILE-keyed (folder segments + file stem) for new-syntax files; values/utils/hooks emit onto a per-file `public static partial class __Exports`; the full ES import surface is live (`import { a as b }`, `import * as X` + dotted tags, default imports, `export { … }` lists). Companion partial-merging is deprecated (UITKX2107). Emitted usings for new-mode units go INSIDE the namespace block, `global::`-qualified (file-level aliases are shadowed by sibling file-stem namespaces). The codemod is `UitkxMigrateImports --es-modules`.

The actual parser/lexer/AST/lowering/formatter/IntelliSense lives in **`ide-extensions~/language-lib/`** (`ReactiveUITK.Language`), *not* in the generator. Both the generator and the LSP server reference it, so parsing behaves identically in a Unity build and in every editor. `HookRegistry.cs` is the single source of truth for hook metadata and is **`<Compile Include>`-linked** from `Shared/Core/` into the language lib — the generator, analyzer, LSP hover, and Unity runtime all read the same table. There are explicit *parity/contract* tests (`Hmr*ContractTests`, `AsmdefResolverParityTests`) guarding that the HMR emitter and generator emitter stay in lockstep; if you change one emitter, expect to update the other.

### Committed generator DLLs

`Analyzers/ReactiveUITK.SourceGenerator.dll` and `ReactiveUITK.Language.dll` are **checked into the repo** because Unity loads them as analyzers. After changing anything in `SourceGenerator~/` or `language-lib/`, rebuild and re-commit them with `scripts/build-generator.ps1` — the csproj builds to `bin/` and a `PublishGeneratorToAnalyzers` post-build target copies the DLLs into `Analyzers/` (direct-write was abandoned: Unity locks the loaded analyzer, MSB3027). CI (`test.yml`) has an advisory check that warns if the committed DLL drifts from a fresh Release build; `publish.yml` rebuilds fresh on release regardless.

## Common commands

```bash
# Rebuild the source generator + language lib into Analyzers/ (do this after editing either)
scripts/build-generator.ps1              # Release; -Debug for Debug

# Tests — the two suites CI runs (net10.0 and net8.0 respectively)
dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj
dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj

# Run a single test
dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj --filter "FullyQualifiedName~ParserTests"

# LSP server (published into each extension's server/ folder)
dotnet publish ide-extensions~/lsp-server -c Release --self-contained false -o ide-extensions~/vscode/server

# VS Code extension
cd ide-extensions~/vscode && npm ci && npm run build      # prebuild copies the shared TextMate grammar

# VS2022 extension
ide-extensions~/visual-studio/build-local.ps1

# Docs site
cd ReactiveUIToolKitDocs~ && npm run dev                  # or: npm run build
```

## Conventions

- **Versioning is SemVer, patch by default.** Bump only the last digit unless a change is genuinely additive (minor) or breaking (major); see `VERSIONING.md`. IDE extensions version independently from the Unity package (`package.json`, currently `0.6.x`). Deprecate for one minor with `[Obsolete(...)]` before removing. `CHANGELOG.md` is the source of truth for every version and is generated/assisted by `scripts/changelog.mjs`.
- **Typed styles:** `Style` is a set-only typed dictionary mapping every UI Toolkit inline style; values are compile-time checked. `CssHelpers` (`Pct()`, `Px()`, `FlexRow`, `Rgba()`, `Hex()`, …) is auto-imported in `.uitkx` files; in `.cs` add `using static ReactiveUITK.Props.Typed.CssHelpers;`. The old `(StyleKeys.Key, value)` tuple form is a still-supported escape hatch.
- `VirtualNode` is pooled (`__Rent`); don't hold references across renders.
- Git author is the user's alone — do not add a `Co-Authored-By` trailer, and don't stage/commit/push unless explicitly asked.

## Skills (`.claude/skills/`)

Project skills live in `.claude/skills/` (this repo dropped the Copilot `.github/instructions|prompts|skills` conventions — everything migrated 2026-07-16). Use them; don't improvise their subject matter:

- **`rebuild-ide-extensions`** — THE local dev loop: rebuild LSP server + extension for the owner to F5-test. Never publish to a marketplace to test anything; releases are `.github/workflows/publish.yml`, owner-triggered.
- **`changelog`** — the centralized `changelog.json` system: add/extract/extract-overview/`verify`; generated marketplace pages (README.md, overview.md) are never hand-edited — edit templates and regenerate.
- **`discord-changelog`** — style + hard 2000-char cap for `Plans~/DISCORD_CHANGELOG.md` entries.
- **`add-unity-version`** — full runbook for supporting a new Unity release (API diff → classify → implement → schema/LSP → docs → record-keeping).

## Coding standards (repo policy — applies to all code edits in this package, not to consumer projects or Markdown/commit prose)

**Style (code files):**
- No emojis in source, generated code, logs, or diagnostic text. No non-ASCII in identifiers or shipped strings — only `—` and `→` in human-facing text, and only when they materially aid readability.
- No `//` comments unless strictly necessary (non-obvious intent in a complex algorithm, spec/ticket citation, deliberate-workaround marker). Never: restating code, section banners, author tags, commented-out code (delete it).
- No XML doc comments (`///`) unless the symbol is public consumer-facing API.
- Match the surrounding file's style; don't reformat unrelated lines.

**Research before editing** — for any change beyond a trivial one-liner, answer these in the reply *before* the edit (search the codebase, don't guess):
- **Blast radius:** what call sites depend on this symbol/format/contract?
- **Parity:** the codebase has FOUR parallel emission/analysis layers that must stay in sync — SG (`SourceGenerator~/Emitter/*`), HMR emitters (`Editor/HMR/Hmr*Emitter.cs`), IDE virtual doc (`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`), shared parser (`ide-extensions~/language-lib/Parser/*`). A fix in one usually lands in all; `HmrEmitterParityContractTests` catches SG↔HMR drift but NOT the virtual doc.
- **HMR safety:** does it break `UitkxHmrCompiler`'s reflection plumbing? New shared parser entry points typically need a new reflection delegate.
- **Source-map safety:** does it shift generated-line offsets? (cursor-context/hover/diagnostic-mapping tests pin lines).
- **Performance:** hot paths = parser scanners (per-keystroke), `FindBareJsxRanges`, splicers (per `{expr}`), formatter (every save).
- **Test coverage:** does an existing test pin current behaviour — and does it confirm the bug or the fix?
- **Backwards compat:** do existing user `.uitkx` files still compile/render identically? A Warning→Error severity bump is breaking.

**Root cause, not patch:**
- State the root cause in one line before fixing. Fix at the layer where the bug lives, not the call site that surfaces it.
- No `try { } catch { }` around "sometimes throws"; no `#pragma warning disable` without a false-positive justification comment.
- Never comment out failing tests — fix the test or the code; genuinely out-of-scope failures get an entry in `Plans~/REMAINING_WORK.md` with what/why/trigger-to-revisit.
- Workarounds that don't reach root cause this iteration also get a `Plans~/REMAINING_WORK.md` entry, referenced from the changelog.
- When ambiguous between quick patch and deep fix, surface the trade-off in one sentence and let the owner choose; prefer the deep fix when the patch would leave parity drift.
