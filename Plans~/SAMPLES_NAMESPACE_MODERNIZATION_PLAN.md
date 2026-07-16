# Samples Namespace Modernization — Execution Plan

**Status: READY TO EXECUTE** (written 2026-07-16; inventory verified against the working tree).
**Branch:** `fix/import-diagnostic-anchors` (same branch, same PR).
**Goal:** apply the same treatment JustStayOn received to `Samples/`: a `uitkx.config.json`
with `namespacePrefix`, **zero `@namespace` directives**, C# `using` lines updated to the new
path-derived namespaces, formatted, and compile-verified to zero errors.

Every decision is pre-made below. Do not improvise; where the plan says DELETE, delete; where
it says a table drives a rename, follow the table. All verification gates are HARD — do not
commit with any gate red.

---

## 0. Context you need (read once)

- Since 0.8.x, a file with no `@namespace` gets a **path-derived** namespace:
  `prefix + "." + <folder segments below the derivation anchor>`. The anchor here is the
  directory of `Samples/ReactiveUITK.Examples.asmdef` (i.e. `Samples/`).
- The prefix comes from `uitkx.config.json`'s `"namespacePrefix"` (nearest-config-wins,
  walking up from each file).
- **Folder casing is preserved VERBATIM** — `components` (lowercase) stays `components` in the
  namespace. Do NOT PascalCase anything.
- File imports (`import { X } from "./path"`) resolve by path, not namespace — they are
  unaffected by namespace changes and (since 0.8.1/0.8.2) fully self-sufficient: they carry
  attribute validation, C#-body type aliases, HMR scope. A namespace-import
  (`import "@Some.Ns"`) is only needed for C# namespaces NOT auto-injected.
- Companion files (`X.style.uitkx`, `X.hooks.uitkx`, `X.types.uitkx` beside `X.uitkx`) merge
  as partial classes — they MUST share the component's namespace. Same folder ⇒ same derived
  namespace, so with zero stamps this is automatic. NEVER stamp one file of a companion set.

## 1. Inventory (verified 2026-07-16 — trust it, don't re-derive)

**163 `.uitkx` files, 12 stamp roots** (counts = files):

| Current `@namespace` | Files | Located under |
|---|---|---|
| `ReactiveUITK.Samples.UITKXComponents` | 73 | `Samples/Components/**` (many folders — FLAT stamp) |
| `Samples.DoomGame` | 13 | `Samples/Components/DoomGame/` |
| `PrettyUi.App.Pages` | 13 | `Samples/UIs/PrettyUi/UI/Pages/**` |
| `PrettyUi.App.Components` | 13 | `Samples/UIs/PrettyUi/UI/Components/**` |
| `Samples.MarioGame` | 12 | `Samples/Components/MarioGame/**` |
| `Samples.GalagaGame` | 12 | `Samples/Components/GalagaGame/**` |
| `Samples.SnakeGame` | 9 | `Samples/Components/SnakeGame/**` |
| `ReactiveUITK.Samples.UITKXShared` | 7 | `Samples/Shared/` |
| `PrettyUi.App` | 4 | `Samples/UIs/PrettyUi/UI/` (root files) |
| `Samples.TicTacToe` | 3 | `Samples/Components/TicTacToe/` |
| `Samples.StressTest` | 3 | `Samples/Components/StressTest/` |
| `ReactiveUITK.Samples.PortalsPlayground` | 1 | `Samples/Components/PortalsPlayground/` |

**C# consumers (~50 files, 50 `using` lines):** all `Samples/Showcase/Editor/EditorUitkx*.cs`
(one demo window per sample), `Samples/Showcase/Both/**`, `Samples/Showcase/Runtime/**`,
`Samples/Components/DoomGame/DoomGameRuntimeBootstrap.cs`,
`Samples/Components/PortalsPlayground/PortalsPlaygroundBootstrap.cs`,
`Samples/UIs/PrettyUi/PrettyUiBootstrap.cs`, and — **cross-asmdef** —
`Diagnostics/Benchmark/BenchEditorHost.cs` + `Diagnostics/Benchmark/BenchmarkSetup.cs`.

**`.uitkx` files containing OLD-namespace references inside their code** (FQNs like
`global::PrettyUi.App…` and/or `import "@PrettyUi.App…"` lines) — each needs step 4:
```
Samples/Components/ShowcaseDemoPage/components/ShowcaseListTabsSection/ShowcaseListTabsSection.uitkx
Samples/Components/ShowcaseDemoPage/components/ShowcaseTreeTabsSection/ShowcaseTreeTabsSection.uitkx
Samples/Components/TabTreeDemoFunc/TabTreeDemoFunc.uitkx
Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx
Samples/UIs/PrettyUi/UI/AppRoot.uitkx
Samples/UIs/PrettyUi/UI/Components/AppButton/AppButton.style.uitkx
Samples/UIs/PrettyUi/UI/Components/AppButton/AppButton.uitkx
Samples/UIs/PrettyUi/UI/Components/ContentPanel/ContentPanel.style.uitkx
Samples/UIs/PrettyUi/UI/Components/NewGameButton/NewGameButton.style.uitkx
Samples/UIs/PrettyUi/UI/Components/PageShell/PageShell.style.uitkx
```
(This list came from `grep -rlE 'global::(Samples|PrettyUi|ReactiveUITK\.Samples)|import "@(Samples|PrettyUi|ReactiveUITK\.Samples)' Samples --include="*.uitkx"` —
re-run it during step 4 to catch anything the truncated listing missed.)

**Other facts:** one asmdef only (`Samples/ReactiveUITK.Examples.asmdef`, name
`ReactiveUITK.Examples`, no `rootNamespace`). **Zero `~/` usage** in samples — so the config
must NOT set `"root"`. The corpus gate (`SamplesCorpusGateTests`) copies `*.*` recursively, so
the new `uitkx.config.json` is picked up automatically — no gate change needed.

## 2. The one decision (already made)

**`namespacePrefix = "ReactiveUITK.Samples"`.** Derived namespaces become:

| File | New namespace |
|---|---|
| `Samples/Components/StressTest/StressTest.uitkx` | `ReactiveUITK.Samples.Components.StressTest` |
| `Samples/Components/DoomGame/*.uitkx` | `ReactiveUITK.Samples.Components.DoomGame` |
| `Samples/Components/GalagaGame/components/GameScreen/*.uitkx` | `ReactiveUITK.Samples.Components.GalagaGame.components.GameScreen` (note lowercase `components`) |
| `Samples/Shared/*.uitkx` | `ReactiveUITK.Samples.Shared` |
| `Samples/UIs/PrettyUi/UI/AppRoot.uitkx` | `ReactiveUITK.Samples.UIs.PrettyUi.UI` |
| `Samples/UIs/PrettyUi/UI/Pages/HomePage/*.uitkx` | `ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.HomePage` |

General rule for any file: `ReactiveUITK.Samples` + the folder path below `Samples/`, one
dotted segment per folder, casing verbatim, file name excluded.

## 3. Execution steps

### Step 0 — Baseline
```bash
cd c:/Yanivs/GameDev/UnityComponents/Assets/ReactiveUIToolKit
git status --short   # expect clean apart from Errors (user scratch — NEVER commit it)
dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj -v q --nologo | tail -2   # expect 1542 green
dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj -v q --nologo | tail -2  # expect 118 green
```

### Step 1 — Config
Create `Samples/uitkx.config.json` with EXACTLY:
```json
{
  "namespacePrefix": "ReactiveUITK.Samples"
}
```
Do NOT add a `"root"` key (it would change `~/` resolution semantics; samples use none and
the default must stay `Assets`).

### Step 2 — Clear every stamp
Write this to a scratch file and run with node (do NOT inline it in bash — backslash mangling):
```js
// clear-stamps.js — run from the package root
const fs = require('fs'), path = require('path');
let removed = 0;
(function walk(d) {
  for (const e of fs.readdirSync(d, { withFileTypes: true })) {
    const p = path.join(d, e.name);
    if (e.isDirectory()) walk(p);
    else if (e.name.endsWith('.uitkx')) {
      const src = fs.readFileSync(p, 'utf8');
      const nl = src.includes('\r\n') ? '\r\n' : '\n';
      const out = src.split(/\r?\n/).filter(l => {
        if (/^@namespace /.test(l)) { removed++; return false; }
        return true;
      }).join(nl);
      if (out !== src) fs.writeFileSync(p, out);
    }
  }
})('Samples');
console.log('stamps removed:', removed); // expect 163 (includes the fixture's 4)
```
Then **revert the protected fixture** (it must stay byte-identical, stamps and all):
```bash
git checkout -- "Samples/Components/UitkxTestFileDoNotTouch/"
```
Net result: 159 stamps removed, the fixture's 4 restored. NEVER modernize that folder.

### Step 3 — Fix intra-uitkx old-namespace references (APPENDIX C — apply VERBATIM)
The full research is done: there are **no `global::` FQNs anywhere** — every stale reference
is a namespace-import line. Appendix C lists every DELETE (24 lines across 23 files) and every
replacement ADD (exactly 3 — the `SidebarItem` file imports). Apply it verbatim; place each ADD
at the top of that file's import block (the step-5 formatter normalizes ordering).
If (and only if) step 6 then surfaces a CS0246/CS0103 in a `.uitkx`-mapped line, the missing
name is a peer export referenced ambiently: add
`import { Name } from "<relative path to its declaring file, extensionless>"` — find the
declaring file in Appendix A.

### Step 4 — Update the C# `using` lines (APPENDIX B — apply VERBATIM)
The full research is done: Appendix B lists, for every one of the ~50 consumer `.cs` files,
the exact REMOVE line(s) and the exact ADD line(s) (computed from what each file actually
references). Apply it verbatim. Appendix A is the reference map (every sample export → its
new namespace) if anything unexpected turns up.

Note on ambiguous names: `GameLogic`, `GameScreen`, `HUD`, `MainMenu` exist in several game
folders. No consumer `.cs` references them directly (verified), so Appendix B contains no
ambiguity — but if you end up adding an import for one of them in step 3's error loop, pick
the declaring file inside the SAME game folder as the referencing file.

### Step 5 — Format
```bash
dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- Samples --format
git checkout -- "Samples/Components/UitkxTestFileDoNotTouch/"   # protect the fixture AGAIN
```
(The formatter also canonicalizes preamble order. It must run LAST among uitkx edits or
`FormatterSnapshotTests` idempotency fails.)

### Step 6 — Verify (ALL gates hard)
```bash
# 1. SG suite (includes the corpus gate + formatter idempotency over the samples)
dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj -v q --nologo | tail -2
# 2. LSP suite
dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj -v q --nologo | tail -2
# 3. Unity assemblies (run from c:/Yanivs/GameDev/UnityComponents — csprojs verified to exist):
dotnet build ReactiveUITK.Examples.csproj -v q --nologo                     # 0 errors
dotnet build ReactiveUITK.Editor.csproj -v q --nologo                       # 0 errors
dotnet build ReactiveUITK.Diagnostics.Benchmark.Editor.csproj -v q --nologo # 0 errors (BenchEditorHost/BenchmarkSetup)
dotnet build ReactiveUITK.Diagnostics.csproj -v q --nologo                  # 0 errors
# 4. Nothing left behind:
grep -rc "^@namespace" Samples --include="*.uitkx" | grep -v ":0" | grep -v DoNotTouch  # empty
grep -rlE "global::(Samples\.|PrettyUi\.)" Samples --include="*.uitkx"                  # empty
grep -rlE "using (Samples\.|PrettyUi\.|ReactiveUITK\.Samples\.UITKX)" Samples Diagnostics --include="*.cs"  # empty
```
Iterate steps 3–4 on any compile error until all gates pass. Typical error signatures and
their meaning (from the JustStayOn run of this same migration):
- `CS0246 <Type> not found` in a `.cs` → missed `using` update (step 4).
- `CS0246/CS0234` in a `.uitkx`-mapped line → stale FQN or deleted-but-needed namespace
  import (step 3) — prefer adding a FILE import of the name.
- `CS0103 'Styles' does not exist` → a companion pair got split across namespaces; check that
  NEITHER file of the pair carries a stamp (both must derive, same folder).
- `UITKX0109 "declares no parameters"` storms → you're running against DLLs older than the
  toolkit source; rebuild committed DLLs first (`scripts/build-generator.ps1`) — but do NOT
  commit DLLs unless language-lib/SG source changed (it should NOT in this task).

### Step 7 — Changelog + commit
Append one bullet to the **unpublished 0.8.2** entry in `CHANGELOG.md` under a `### Changed`
heading (create it after the `### Fixed` block if absent):
```
- **Samples modernized to zero `@namespace`.** `Samples/uitkx.config.json` now carries
  `namespacePrefix: "ReactiveUITK.Samples"`; all sample namespaces are path-derived
  (`ReactiveUITK.Samples.<Folders>`), demo bootstraps/windows updated accordingly. The
  UitkxTestFileDoNotTouch fixture intentionally keeps the legacy form.
```
Commit on `fix/import-diagnostic-anchors` (message below), **no Co-Authored-By trailer, do
not push unless the user asks**:
```
chore(samples): zero @namespace — namespacePrefix config + path-derived namespaces

Samples/uitkx.config.json (namespacePrefix: ReactiveUITK.Samples); removed all
163 stamps (DoNotTouch fixture excluded); intra-uitkx old-namespace FQNs and
dead namespace-imports resolved via file imports; ~50 C# using lines updated
(incl. Diagnostics/Benchmark cross-asmdef consumers); formatted.

Gates: SG suite green, LSP suite green, Examples/Editor/Diagnostics csprojs
compile 0 errors, zero legacy-namespace references remain.
```

## 4. Hard guardrails (violating any of these = stop and ask)

1. **NEVER touch `Samples/Components/UitkxTestFileDoNotTouch/`** — byte-identical fixture.
   Revert it after every bulk tool run.
2. **NEVER move/edit `family-corpus.hash`** (CI drift gate + test root discovery).
3. **NEVER PascalCase folder segments** — namespaces take folder casing verbatim.
4. **Do NOT set `"root"`** in the samples config.
5. **Do NOT stamp one file of a companion set** (in this task: stamp nothing at all).
6. **Do NOT commit** the `Errors` file, `*.meta` churn you didn't cause, or Analyzer DLLs
   (this task changes no generator source).
7. Write node scripts to files before running (inline `node -e` mangles backslashes in bash).
8. Pre-existing warnings that are NOT regressions: UITKX0113 duplicate `GameScreen`
   (Galaga/Mario/Snake/Doom), the DoomGame `GameScreen` codemod ambiguity note, CS0414 in
   unrelated game code.

## 5. Acceptance checklist

- [ ] `Samples/uitkx.config.json` exists with exactly the one key.
- [ ] Zero `@namespace` in `Samples/**/*.uitkx` except the DoNotTouch fixture.
- [ ] Zero `global::Samples.* / global::PrettyUi.*` and zero dead `import "@<old-ns>"` lines.
- [ ] Zero `using` of the 12 legacy roots anywhere in `Samples/` + `Diagnostics/`.
- [ ] SG suite green; LSP suite green.
- [ ] `ReactiveUITK.Examples` / `ReactiveUITK.Editor` / `ReactiveUITK.Diagnostics` csprojs: 0 errors.
- [ ] DoNotTouch fixture byte-identical (`git diff --stat` shows no changes there).
- [ ] CHANGELOG 0.8.2 bullet added; single commit on the branch; not pushed.

---

# Appendices — pre-computed research (2026-07-16, verified against the working tree)

Generated by exhaustive scan (all 163 sample .uitkx + all .cs under Samples/, Diagnostics/, CICD/).
Appendix A = the name→namespace reference map. Appendix B = exact per-file C# using edits.
Appendix C (from the supplementary scan) = exact intra-uitkx DELETE/ADD lines.

## Appendix A — component/module/hook name -> NEW namespace (derived)

| Name | New namespace(s) |
|---|---|
| AnimationsDemoPage | ReactiveUITK.Samples.Shared |
| AppButton | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.AppButton |
| AppRoot | ReactiveUITK.Samples.UIs.PrettyUi.UI |
| Block | ReactiveUITK.Samples.Components.MarioGame.components.Block |
| ContentPanel | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.ContentPanel |
| ContextBailoutDemoFunc | ReactiveUITK.Samples.Components.ContextBailoutDemoFunc |
| ContextConsumer | ReactiveUITK.Samples.Components.ContextDemoFunc |
| ContextDemoFunc | ReactiveUITK.Samples.Components.ContextDemoFunc |
| CustomDrawDemoFunc | ReactiveUITK.Samples.Components.CustomDrawDemoFunc |
| DeepNestedSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| DeepNode | ReactiveUITK.Samples.Components.RenderDepthGuardDemoFunc |
| DeferredEffectDemoFunc | ReactiveUITK.Samples.Components.DeferredEffectDemoFunc |
| DirectiveSuccessDemo | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| DoomFace | ReactiveUITK.Samples.Components.DoomGame |
| DoomGame | ReactiveUITK.Samples.Components.DoomGame |
| DoomGameScreen | ReactiveUITK.Samples.Components.DoomGame |
| DoomGameScreenLogic | ReactiveUITK.Samples.Components.DoomGame |
| DoomHUD | ReactiveUITK.Samples.Components.DoomGame |
| DoomMainMenu | ReactiveUITK.Samples.Components.DoomGame |
| DoomMaps | ReactiveUITK.Samples.Components.DoomGame |
| DoomMinimap | ReactiveUITK.Samples.Components.DoomGame |
| DoomTextures | ReactiveUITK.Samples.Components.DoomGame |
| DoomTypes | ReactiveUITK.Samples.Components.DoomGame |
| EditorControlsDemoFunc | ReactiveUITK.Samples.Components.EditorControlsDemoFunc |
| EffectCleanupOrderDemoFunc | ReactiveUITK.Samples.Components.EffectCleanupOrderDemoFunc |
| EffectPanel | ReactiveUITK.Samples.Components.EffectCleanupOrderDemoFunc |
| Enemy | ReactiveUITK.Samples.Components.MarioGame.components.Enemy |
| EventBatchingDemoFunc | ReactiveUITK.Samples.Components.EventBatchingDemoFunc |
| ExceptionFlowDemoFunc | ReactiveUITK.Samples.Components.ExceptionFlowDemoFunc |
| FlushSyncDemoFunc | ReactiveUITK.Samples.Components.FlushSyncDemoFunc |
| ForSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| ForeachSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| GalagaGame | ReactiveUITK.Samples.Components.GalagaGame |
| GalagaTypes | ReactiveUITK.Samples.Components.GalagaGame |
| GameLogic | ReactiveUITK.Samples.Components.DoomGame AND ReactiveUITK.Samples.Components.GalagaGame |
| GamePage | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.GamePage |
| GameScreen | ReactiveUITK.Samples.Components.GalagaGame.components.GameScreen AND ReactiveUITK.Samples.Components.MarioGame.components.GameScreen AND ReactiveUITK.Samples.Components.SnakeGame.components.GameScreen |
| HUD | ReactiveUITK.Samples.Components.GalagaGame.components.HUD AND ReactiveUITK.Samples.Components.MarioGame.components.HUD |
| HelloWorldFunc | ReactiveUITK.Samples.Components.HelloWorldFunc |
| HomePage | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.HomePage |
| HomePageSidebar | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.HomePage |
| HookStateQueueDemoFunc | ReactiveUITK.Samples.Components.HookStateQueueDemoFunc |
| IfSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| IntroCounterFunc | ReactiveUITK.Samples.Shared |
| KeyedDiffLisDemoFunc | ReactiveUITK.Samples.Components.KeyedDiffLisDemoFunc |
| LatestFeaturesDemoFunc | ReactiveUITK.Samples.Components.LatestFeaturesDemoFunc |
| ListViewStatefulDemoFunc | ReactiveUITK.Samples.Shared |
| MainMenu | ReactiveUITK.Samples.Components.GalagaGame.components.MainMenu AND ReactiveUITK.Samples.Components.MarioGame.components.MainMenu |
| MainMenuLayout | ReactiveUITK.Samples.Components.MainMenuRouterDemoFunc |
| MainMenuNavigationRow | ReactiveUITK.Samples.Components.MainMenuRouterDemoFunc |
| MainMenuRouterDemoFunc | ReactiveUITK.Samples.Components.MainMenuRouterDemoFunc |
| MainMenuSidebar | ReactiveUITK.Samples.Components.MainMenuRouterDemoFunc |
| Mario | ReactiveUITK.Samples.Components.MarioGame.components.Mario |
| MarioGame | ReactiveUITK.Samples.Components.MarioGame |
| MediaPlaygroundDemoPage | ReactiveUITK.Samples.Shared |
| Menu | ReactiveUITK.Samples.Components.SnakeGame.components.Menu |
| MenuPage | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.MenuPage |
| MiddleLayer | ReactiveUITK.Samples.Components.ContextBailoutDemoFunc |
| MultiColumnListViewStatefulDemoFunc | ReactiveUITK.Samples.Shared |
| MultiColumnTreeViewStatefulDemoFunc | ReactiveUITK.Samples.Shared |
| Mushroom | ReactiveUITK.Samples.Components.MarioGame.components.Mushroom |
| NestedSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| NewGameButton | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.NewGameButton |
| NewsPage | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.NewsPage |
| NewsPageSidebar | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.NewsPage |
| PageShell | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.PageShell |
| PortalEventScopeDemoFunc | ReactiveUITK.Samples.Components.PortalEventScopeDemoFunc |
| PortalsPlayground | ReactiveUITK.Samples.Components.PortalsPlayground |
| ProgressBarDemoFunc | ReactiveUITK.Samples.Components.ProgressBarDemoFunc |
| PropTypesDemoFunc | ReactiveUITK.Samples.Components.PropTypesDemoFunc |
| Raycast | ReactiveUITK.Samples.Components.DoomGame |
| RefChild | ReactiveUITK.Samples.Components.RefForwardingDemoFunc.RefChild |
| RefForwardingDemoFunc | ReactiveUITK.Samples.Components.RefForwardingDemoFunc |
| RenderDepthGuardDemoFunc | ReactiveUITK.Samples.Components.RenderDepthGuardDemoFunc |
| RouterDemoFunc | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterHistoryPanel | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterLocationBanner | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterNavLink | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterNavigatePanel | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterNavigationGuardPanel | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterQuickAccessPanel | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterSettingsPanel | ReactiveUITK.Samples.Components.RouterDemoFunc |
| RouterUserDetails | ReactiveUITK.Samples.Components.RouterDemoFunc |
| SectionBox | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| SectionHeading | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| SectionNote | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| SettingsPage | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.SettingsPage |
| SettingsPageSidebar | ReactiveUITK.Samples.UIs.PrettyUi.UI.Pages.SettingsPage |
| SetupCodeSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| ShowcaseDemoPage | ReactiveUITK.Samples.Components.ShowcaseDemoPage |
| ShowcaseExtrasPanel | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseExtrasPanel |
| ShowcaseFieldsPanel | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseFieldsPanel |
| ShowcaseListTabsSection | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseListTabsSection |
| ShowcaseNewComponentsPanel | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseNewComponentsPanel |
| ShowcaseTopBar | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseTopBar |
| ShowcaseTreeTabsSection | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ShowcaseTreeTabsSection |
| Sidebar | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.Sidebar |
| SidebarItem | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.Sidebar |
| SignalCounterDemoFunc | ReactiveUITK.Samples.Components.SignalCounterDemoFunc |
| SimpleCounterFunc | ReactiveUITK.Samples.Components.SimpleCounterFunc |
| SimpleTextFieldFunc | ReactiveUITK.Samples.Components.SimpleTextFieldFunc |
| SimpleUseEffectFunc | ReactiveUITK.Samples.Components.SimpleUseEffectFunc |
| SnakeGame | ReactiveUITK.Samples.Components.SnakeGame |
| SpriteAtlas | ReactiveUITK.Samples.Components.GalagaGame |
| StatusBadge | ReactiveUITK.Samples.Components.PropTypesDemoFunc |
| StressTest | ReactiveUITK.Samples.Components.StressTest |
| StyleExtensions | ReactiveUITK.Samples.UIs.PrettyUi.UI |
| StyledAssetDemoFunc | ReactiveUITK.Samples.Components.StyledAssetDemoFunc |
| SwitchSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| SyntheticEventDemoFunc | ReactiveUITK.Samples.Components.SyntheticEventDemoFunc |
| TabTreeDemoFunc | ReactiveUITK.Samples.Components.TabTreeDemoFunc |
| Theme | ReactiveUITK.Samples.UIs.PrettyUi.UI |
| ThemeConsumer | ReactiveUITK.Samples.Components.ContextBailoutDemoFunc |
| TicTacToe | ReactiveUITK.Samples.Components.TicTacToe |
| ToggleButtonGroupDemoFunc | ReactiveUITK.Samples.Components.ToggleButtonGroupDemoFunc |
| TopNav | ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.TopNav |
| TreeViewStatefulDemoFunc | ReactiveUITK.Samples.Shared |
| UitkxCounterFunc | ReactiveUITK.Samples.Components.UitkxCounterFunc |
| UnstableChild | ReactiveUITK.Samples.Components.ExceptionFlowDemoFunc |
| ValueItem | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ValuesBar.components.ValueItem |
| ValuesBar | ReactiveUITK.Samples.Components.ShowcaseDemoPage.components.ValuesBar |
| WelcomeScreen | ReactiveUITK.Samples.Components.SnakeGame.components.WelcomeScreen |
| WhileSection | ReactiveUITK.Samples.Components.DirectiveSuccessDemo |
| useDoomGame | ReactiveUITK.Samples.Components.DoomGame |
| useGalagaGame | ReactiveUITK.Samples.Components.GalagaGame.components.GameScreen |
| useMarioGame | ReactiveUITK.Samples.Components.MarioGame.components.GameScreen |
| useSnakeGame | ReactiveUITK.Samples.Components.SnakeGame.components.GameScreen |
| useStressTestLoop | ReactiveUITK.Samples.Components.StressTest |
| useTestHomeState | ReactiveUITK.Samples.Components.TicTacToe |

Ambiguous names (declared in >1 folder): GameLogic, GameScreen, HUD, MainMenu

## Appendix B — exact per-file C# using edits

### Diagnostics/Benchmark/BenchEditorHost.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ShowcaseDemoPage;
- (referenced sample names: ShowcaseDemoPage)

### Diagnostics/Benchmark/BenchmarkSetup.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ShowcaseDemoPage;
- (referenced sample names: ShowcaseDemoPage)

### Samples/Components/DoomGame/DoomGameRuntimeBootstrap.cs
- REMOVE: using Samples.DoomGame;
- ADD: using ReactiveUITK.Samples.Components.DoomGame;
- (referenced sample names: DoomGame)

### Samples/Components/PortalsPlayground/PortalsPlaygroundBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.PortalsPlayground;
- ADD: using ReactiveUITK.Samples.Components.PortalsPlayground;
- (referenced sample names: PortalsPlayground)

### Samples/Showcase/Both/EditorUitkxShowcaseAllDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ShowcaseDemoPage;
- (referenced sample names: ShowcaseDemoPage)

### Samples/Showcase/Both/RuntimeAppExample/RuntimeAppExampleUIBootstrap.cs
- REMOVE: using Samples.StressTest;
- ADD: using ReactiveUITK.Samples.Components.StressTest;
- (referenced sample names: StressTest)

### Samples/Showcase/Editor/EditorUitkxAnimationsDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXShared;
- ADD: using ReactiveUITK.Samples.Shared;
- (referenced sample names: AnimationsDemoPage)

### Samples/Showcase/Editor/EditorUitkxContextBailoutDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ContextBailoutDemoFunc;
- (referenced sample names: ContextBailoutDemoFunc)

### Samples/Showcase/Editor/EditorUitkxContextDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ContextDemoFunc;
- (referenced sample names: ContextDemoFunc)

### Samples/Showcase/Editor/EditorUitkxCounterDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.UitkxCounterFunc;
- (referenced sample names: UitkxCounterFunc)

### Samples/Showcase/Editor/EditorUitkxCustomDrawDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.CustomDrawDemoFunc;
- (referenced sample names: CustomDrawDemoFunc)

### Samples/Showcase/Editor/EditorUitkxDeferredEffectDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.DeferredEffectDemoFunc;
- (referenced sample names: DeferredEffectDemoFunc)

### Samples/Showcase/Editor/EditorUitkxDiabloMenuDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SnakeGame.components.Menu;
- (referenced sample names: Menu)

### Samples/Showcase/Editor/EditorUitkxDirectiveSuccessDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.DirectiveSuccessDemo;
- (referenced sample names: DirectiveSuccessDemo)

### Samples/Showcase/Editor/EditorUitkxDoomGameDemoWindow.cs
- REMOVE: using Samples.DoomGame;
- ADD: using ReactiveUITK.Samples.Components.DoomGame;
- (referenced sample names: DoomGame)

### Samples/Showcase/Editor/EditorUitkxEditorControlsDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.EditorControlsDemoFunc;
- (referenced sample names: EditorControlsDemoFunc)

### Samples/Showcase/Editor/EditorUitkxEffectCleanupOrderDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.EffectCleanupOrderDemoFunc;
- (referenced sample names: EffectCleanupOrderDemoFunc)

### Samples/Showcase/Editor/EditorUitkxEventBatchingDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.EventBatchingDemoFunc;
- (referenced sample names: EventBatchingDemoFunc)

### Samples/Showcase/Editor/EditorUitkxExceptionFlowDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ExceptionFlowDemoFunc;
- (referenced sample names: ExceptionFlowDemoFunc)

### Samples/Showcase/Editor/EditorUitkxFlushSyncDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.FlushSyncDemoFunc;
- (referenced sample names: FlushSyncDemoFunc)

### Samples/Showcase/Editor/EditorUitkxGalagaGameDemoWindow.cs
- REMOVE: using Samples.GalagaGame;
- ADD: using ReactiveUITK.Samples.Components.GalagaGame;
- (referenced sample names: GalagaGame)

### Samples/Showcase/Editor/EditorUitkxHelloWorldDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.HelloWorldFunc;
- (referenced sample names: HelloWorldFunc)

### Samples/Showcase/Editor/EditorUitkxHookQueueDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.HookStateQueueDemoFunc;
- (referenced sample names: HookStateQueueDemoFunc)

### Samples/Showcase/Editor/EditorUitkxKeyedDiffLisDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.KeyedDiffLisDemoFunc;
- (referenced sample names: KeyedDiffLisDemoFunc)

### Samples/Showcase/Editor/EditorUitkxLatestFeaturesDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.LatestFeaturesDemoFunc;
- (referenced sample names: LatestFeaturesDemoFunc)

### Samples/Showcase/Editor/EditorUitkxMainMenuRouterDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.MainMenuRouterDemoFunc;  using ReactiveUITK.Samples.Components.SnakeGame.components.Menu;
- (referenced sample names: MainMenuRouterDemoFunc, Menu)

### Samples/Showcase/Editor/EditorUitkxMarioGameDemoWindow.cs
- REMOVE: using Samples.MarioGame;
- ADD: using ReactiveUITK.Samples.Components.MarioGame;  using ReactiveUITK.Samples.Components.MarioGame.components.Mario;
- (referenced sample names: Mario, MarioGame)

### Samples/Showcase/Editor/EditorUitkxMediaPlaygroundDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXShared;
- ADD: using ReactiveUITK.Samples.Shared;
- (referenced sample names: MediaPlaygroundDemoPage)

### Samples/Showcase/Editor/EditorUitkxPortalDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.PortalEventScopeDemoFunc;
- (referenced sample names: PortalEventScopeDemoFunc)

### Samples/Showcase/Editor/EditorUitkxProgressBarDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ProgressBarDemoFunc;
- (referenced sample names: ProgressBarDemoFunc)

### Samples/Showcase/Editor/EditorUitkxPropTypesDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.PropTypesDemoFunc;
- (referenced sample names: PropTypesDemoFunc)

### Samples/Showcase/Editor/EditorUitkxRefForwardingDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.RefForwardingDemoFunc;
- (referenced sample names: RefForwardingDemoFunc)

### Samples/Showcase/Editor/EditorUitkxRenderDepthGuardDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.RenderDepthGuardDemoFunc;
- (referenced sample names: RenderDepthGuardDemoFunc)

### Samples/Showcase/Editor/EditorUitkxRouterDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.RouterDemoFunc;
- (referenced sample names: RouterDemoFunc)

### Samples/Showcase/Editor/EditorUitkxSignalCounterDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SignalCounterDemoFunc;
- (referenced sample names: SignalCounterDemoFunc)

### Samples/Showcase/Editor/EditorUitkxSimpleCounterDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleCounterFunc;
- (referenced sample names: SimpleCounterFunc)

### Samples/Showcase/Editor/EditorUitkxSimpleTextFieldDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleTextFieldFunc;
- (referenced sample names: SimpleTextFieldFunc)

### Samples/Showcase/Editor/EditorUitkxSimpleUseEffectDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleUseEffectFunc;
- (referenced sample names: SimpleUseEffectFunc)

### Samples/Showcase/Editor/EditorUitkxSnakeGameDemoWindow.cs
- REMOVE: using Samples.SnakeGame;
- ADD: using ReactiveUITK.Samples.Components.SnakeGame;
- (referenced sample names: SnakeGame)

### Samples/Showcase/Editor/EditorUitkxStressTestDemoWindow.cs
- REMOVE: using Samples.StressTest;
- ADD: using ReactiveUITK.Samples.Components.StressTest;
- (referenced sample names: StressTest)

### Samples/Showcase/Editor/EditorUitkxSyntheticEventDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SyntheticEventDemoFunc;
- (referenced sample names: SyntheticEventDemoFunc)

### Samples/Showcase/Editor/EditorUitkxTabTreeDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.TabTreeDemoFunc;
- (referenced sample names: TabTreeDemoFunc)

### Samples/Showcase/Editor/EditorUitkxTicTacToeDemoWindow.cs
- REMOVE: using Samples.TicTacToe;
- ADD: using ReactiveUITK.Samples.Components.TicTacToe;
- (referenced sample names: TicTacToe)

### Samples/Showcase/Editor/EditorUitkxToggleButtonGroupDemoWindow.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.ToggleButtonGroupDemoFunc;
- (referenced sample names: ToggleButtonGroupDemoFunc)

### Samples/Showcase/Runtime/RuntimeHelloWorldDemo/RuntimeHelloWorldDemoBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.HelloWorldFunc;
- (referenced sample names: HelloWorldFunc)

### Samples/Showcase/Runtime/RuntimeLatestFeaturesDemo/RuntimeLatestFeaturesDemoBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.LatestFeaturesDemoFunc;
- (referenced sample names: LatestFeaturesDemoFunc)

### Samples/Showcase/Runtime/RuntimeSimpleCounterDemo/RuntimeSimpleCounterDemoBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleCounterFunc;
- (referenced sample names: SimpleCounterFunc)

### Samples/Showcase/Runtime/RuntimeSimpleTextFieldDemo/RuntimeSimpleTextFieldDemoBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleTextFieldFunc;
- (referenced sample names: SimpleTextFieldFunc)

### Samples/Showcase/Runtime/RuntimeSimpleUseEffectDemo/RuntimeSimpleUseEffectDemoBootstrap.cs
- REMOVE: using ReactiveUITK.Samples.UITKXComponents;
- ADD: using ReactiveUITK.Samples.Components.SimpleUseEffectFunc;
- (referenced sample names: SimpleUseEffectFunc)

### Samples/UIs/PrettyUi/PrettyUiBootstrap.cs
- REMOVE: using PrettyUi.App;
- ADD: using ReactiveUITK.Samples.UIs.PrettyUi.UI;
- (referenced sample names: AppRoot)

## Appendix C — intra-uitkx namespace-import lines: exact edits

### Samples/Components/ShowcaseDemoPage/components/ShowcaseListTabsSection/ShowcaseListTabsSection.uitkx
- DELETE L4: `import "@ReactiveUITK.Samples.UITKXShared"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/Components/ShowcaseDemoPage/components/ShowcaseTreeTabsSection/ShowcaseTreeTabsSection.uitkx
- DELETE L5: `import "@ReactiveUITK.Samples.UITKXShared"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/Components/TabTreeDemoFunc/TabTreeDemoFunc.uitkx
- DELETE L4: `import "@ReactiveUITK.Samples.UITKXShared"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx
- DELETE L2: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/AppRoot.uitkx
- DELETE L7: `import "@PrettyUi.App.Pages"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/AppButton/AppButton.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/AppButton/AppButton.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/ContentPanel/ContentPanel.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/NewGameButton/NewGameButton.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/PageShell/PageShell.style.uitkx
- DELETE L2: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/Sidebar/Sidebar.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Components/TopNav/TopNav.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/GamePage/GamePage.style.uitkx
- DELETE L3: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/GamePage/GamePage.uitkx
- DELETE L3: `import "@PrettyUi"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)
- DELETE L4: `import "@PrettyUi.App.Components"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/HomePage/HomePage.uitkx
- DELETE L5: `import "@PrettyUi.App.Components"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/HomePage/HomePageSidebar.uitkx
- DELETE L3: `import "@PrettyUi.App.Components"`
  - ADD: `import { SidebarItem } from "../../Components/Sidebar/SidebarItem.types"`

### Samples/UIs/PrettyUi/UI/Pages/MenuPage/MenuPage.style.uitkx
- DELETE L2: `import "@PrettyUi.App"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/MenuPage/MenuPage.uitkx
- DELETE L4: `import "@PrettyUi.App.Components"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/NewsPage/NewsPage.uitkx
- DELETE L5: `import "@PrettyUi.App.Components"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/NewsPage/NewsPageSidebar.uitkx
- DELETE L3: `import "@PrettyUi.App.Components"`
  - ADD: `import { SidebarItem } from "../../Components/Sidebar/SidebarItem.types"`

### Samples/UIs/PrettyUi/UI/Pages/SettingsPage/SettingsPage.uitkx
- DELETE L5: `import "@PrettyUi.App.Components"`
  - ADD: nothing (no referenced names from that namespace — pure dead line)

### Samples/UIs/PrettyUi/UI/Pages/SettingsPage/SettingsPageSidebar.uitkx
- DELETE L3: `import "@PrettyUi.App.Components"`
  - ADD: `import { SidebarItem } from "../../Components/Sidebar/SidebarItem.types"`


## Appendix D — fixture

UitkxTestFileDoNotTouch carries 4 @namespace stamps that MUST remain (revert after every bulk tool).
