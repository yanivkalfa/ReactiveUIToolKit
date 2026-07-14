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

`.uitkx` is JSX-like markup that a Roslyn source generator compiles to a C# partial class at build time (zero runtime overhead). The pipeline (`SourceGenerator~/UitkxPipeline.cs`) is four stages: **DirectiveParser** (`@namespace`/`@component`/`@using`/`@props`) → **UitkxParser** (recursive-descent markup → AST) → **PropsResolver** (tag names → `V.*` call patterns) → **CSharpEmitter**. Diagnostics use `UITKX####` codes.

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
