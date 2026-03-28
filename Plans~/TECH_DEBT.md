# Tech Debt

## ~~Inspector "Go to Script" broken for `.uitkx`-generated classes~~ ✅ FIXED

Fixed — `TryOpenViaVisualStudioComIntegration()` now correctly verifies
process success before consuming the event.

---

## ~~Unused variables in `.uitkx` not highlighted by analyzer~~ ✅ FIXED

Fixed — UITKX0112 diagnostic implemented using `SemanticModel.AnalyzeDataFlow()`
on the virtual document's `__uitkx_render()` method. Catches all unused locals
including object/collection initialisers (`new Style { … }`) that Roslyn's CS0219
misses. CS0219 suppressed at compilation level; UITKX0112 is the single source of
truth. Filters: scaffold variables (`__uitkx_*`), discard convention (`_` prefix),
and only flags variables in `FunctionSetup`/`CodeBlock` source-map regions (avoids
false positives on lambda params in expression checks).

---

## ~~LSP diagnostics not updated when companion `.cs` file changes~~ ✅ FIXED

Fixed — TextSyncHandler now registers for `**/*.cs` in addition to `**/*.uitkx`.
When a companion `.cs` file is edited, the handler sets a companion overlay and
re-publishes diagnostics for all `.uitkx` files in the same directory automatically.

---

## ~~Rename Symbol (F2) not working in `.uitkx` files~~ ✅ FIXED

Fixed — bidirectional rename between `.uitkx` ↔ `.cs` companion files works.

---

## ~~Type mismatch in companion `.cs` not surfaced in `.uitkx`~~ ✅ FIXED

Fixed — typed `Style` properties (`Width = 100f`, `FlexDirection = Row`) provide
compile-time type safety. Roslyn catches type mismatches through the strongly-typed
property setters. The old tuple-based `(StyleKeys.X, value)` syntax boxes to `object`
and can't be type-checked, but the typed form is now the recommended path.

---

## ~~Go to Definition (F12) not working for companion symbols in `.uitkx`~~ ✅ FIXED

Fixed — Roslyn-based symbol resolution added. Works for same-file vars,
companion `.cs` symbols, and multi-line attribute values.

---

## Button missing sub-element slot for styling inner Label

**Symptom:** Cannot style the text color (or other properties) of a Button's
internal Label/TextElement via UITKX props. Setting `(StyleKeys.Color, ...)`
on the Button style is overridden by Unity's default Button USS.

**Context:** The library already has a slot pattern (`Dictionary<string, object>`)
for sub-elements on TextField (`label`, `input`, `textElement`), ProgressBar
(`titleElement`, `progress`), Toggle (`label`, `input`, `checkmark`), and
Slider (`input`, `track`). Button is simply missing this.

**Required changes:**
1. `Shared/Props/Typed/ButtonProps.cs` — add
   `public Dictionary<string, object> TextElement { get; set; }`
2. `Shared/Elements/ButtonElementAdapter.cs` — add `ApplySlot`/`DiffSlot`
   targeting `button.Q<TextElement>()` (USS class `unity-text-element`)
3. `ide-extensions~/grammar/uitkx-schema.json` — add `textElement` attribute
   to the Button element definition

**Usage would be:**
```xml
<Button text="Menu"
        style={btnStyle}
        textElement={new Dictionary<string, object> { { "style", labelStyle } }} />
```

**Priority:** High — common need, no clean workaround.

---

## Find All References (textDocument/references) not implemented

**Symptom:** Shift+F12 / "Find All References" does nothing in `.uitkx` files.
The LSP server declares no `referencesProvider` capability.

**Context:** Go to Definition and Rename Symbol both work and share the same
Roslyn-based symbol resolution pipeline (per-file AdhocWorkspace, SourceMap
coordinate mapping, cross-workspace symbol matching via `SymbolSignature`).
Find All References would reuse the same infrastructure.

**Implementation approach:**
1. `ReferencesHandler.cs` — resolve symbol at cursor via `AstCursorContext`,
   then call `SymbolFinder.FindReferencesAsync()` on each per-file workspace.
   Map results back to `.uitkx` coordinates via SourceMap.
2. `CapabilityPatchStream.cs` — add `referencesProvider: true`.
3. `Program.cs` — register the handler.

**Template:** `RenameHandler.cs` already iterates all workspaces matching by
`SymbolSignature` — the same loop applies but returns locations instead of edits.

**Priority:** Medium — important IDE feature, straightforward given existing infra.

---

## ~~No declarative USS stylesheet loading from `.uitkx`~~ ✅ DONE

Implemented — `@uss` directive fully working. DirectiveParser parses `@uss "./path.uss"`,
source generator emits `__uitkx_ussKeys` static array, PropsApplier applies sheets
to elements. HMR file watcher monitors `.uss` changes and triggers recompilation
of dependent `.uitkx` components via reverse dependency map.

---

## Autocomplete inserts closing tag and breaks JSX syntax

**Symptom:** When typing `<VisualElement` and then pressing `s` to filter
to `VisualElementSafe`, selecting the completion inserts
`<VisualElementSafe></VisualElementSafe>` — adding an unwanted closing tag
that breaks the existing JSX structure.

**Expected:** Autocomplete should replace only the tag name, not insert a
full open+close tag pair. If the cursor is already inside an opening tag
(e.g. `<VisualElement| style={...}>`), completion should only replace the
element name.

**Files to investigate:**
- VS Code extension completion provider (`ide-extensions~/vscode/`)
- LSP server `textDocument/completion` handler — check `insertTextFormat`
  and `textEdit` range to ensure it replaces only the tag name, not the
  surrounding structure

**Priority:** Medium — disrupts typing flow and requires manual cleanup.

---

## `VisualElementSafe` safe area mismatch in Unity Simulator

**Symptom:** In Play Mode with the Unity Device Simulator, the safe area
rendered by `<VisualElementSafe>` does not match the safe area overlay shown
by the simulator. The safe area is correct when building to a real device.

**Hypothesis:** `SafeAreaUtility` may be reading `Screen.safeArea` which
returns different values in the simulator vs the simulator's visual overlay.
The simulator might report the full screen rect while its overlay draws the
actual device cutouts.

**Files to investigate:**
- `Shared/Core/Util/SafeAreaUtility.cs` — how `Screen.safeArea` is read
- Whether `SimulatorUtilities` or `DeviceSimulator` APIs provide a more
  accurate safe-area rect during simulation
- Unity docs on `Screen.safeArea` behaviour in Device Simulator

**Priority:** Low — only affects simulator preview, correct on device.

---

## ~~Add UI Toolkit Debugger shortcut to ReactiveUITK menu~~ ✅ FIXED

Fixed — `FiberMenu.cs` now has `[MenuItem("ReactiveUITK/UI Toolkit Debugger")]`
that calls `EditorApplication.ExecuteMenuItem("Window/UI Toolkit/Debugger")`.

---

## ~~Remove classic mode from VirtualDocumentGenerator~~ ✅ DONE

Completed — removed all classic-mode code paths across 11+ production files:
`EmitClassicBody`, `EmitExpressionWrapper`, `EmitCodeBlock` (VDG),
`FormatDirectives` (formatter), `CollectDirectiveTokens` (semantic tokens),
`DirectiveItems`/`BuildDirectiveSnippet` (completions), classic parsing loop
(DirectiveParser), UITKX0101/0102 diagnostics, and classic guards in the
SourceGenerator emitter/pipeline. All tests converted to function-style
(820 SG + 59 LSP passing).

---

## Rename Symbol (F2) renames same-named lambda params across unrelated lambdas

**Symptom:** In a pattern like:

```uitkx
setCount(v => v + 1);
setCount(v => v + 1);
setCount(v => v + 1);
```

Renaming one `v` with F2 renames all three, even though they are independent
lambda parameters in separate closures.

**Cause:** The virtual C# document places all lambdas inside the same
`Render()` method body. Roslyn's rename engine sees three lambda parameters
with the same name at the same scope level and renames them all together.
This is standard Roslyn behavior — Blazor/Razor has the same limitation.

**Workaround:** Use distinct parameter names (`prev => prev + 1`) or accept
the batch rename.

**Status:** Known limitation — not planned to fix. Scoping rename edits per-
lambda in `RenameHandler` would risk breaking legitimate multi-line renames
(e.g. a variable used across several lines).

---

## ~~LSP virtual document lacks prop type checking (systematic type erasure)~~ ✅ FIXED

Fixed — `EmitTypedPropsCheck` in VirtualDocumentGenerator emits typed assignment
checks (`{ int? __uitkx_check = (expr); }`) for every attribute with a known type
from `uitkx-schema.json` or `WorkspaceIndex` (user components). Covers built-in
elements and user-defined components. The nullable `?` suffix prevents false errors
on nullable C# properties (e.g. `int?` for `selectedIndex`).

---

## Style properties not yet supported: transitions, cursor, filter

**Symptom:** Setting `transitionProperty`, `transitionDuration`, `transitionDelay`,
`transitionTimingFunction`, `cursor`, `transition`, or `filter` in a `Style`
dictionary has no effect — PropsApplier silently ignores the value.

**Root cause per property:**

- **`transition`** — CSS shorthand only. No `IStyle.transition` property exists in
  Unity 6.2; it decomposes into the four `transition*` sub-properties.
- **`filter`** — No `IStyle.filter` property exists in Unity 6.2 (docs return 404).
- **`transitionProperty`** (`StyleList<StylePropertyName>`),
  **`transitionDuration`** (`StyleList<TimeValue>`),
  **`transitionDelay`** (`StyleList<TimeValue>`),
  **`transitionTimingFunction`** (`StyleList<EasingFunction>`) — All use `StyleList<T>`,
  a list-based type. PropsApplier diffs values via `ReferenceEquals` then `.Equals()`;
  `List<T>` lacks value equality, so transitions would re-apply every render,
  potentially resetting in-flight animations. Needs a dedicated diffing strategy
  or a helper struct (e.g. `Transition(property, duration, easing, delay)`) that
  sets all four at once.
- **`cursor`** (`StyleCursor` wrapping `Cursor` struct) — The `Cursor` struct only
  takes a `Texture2D` + `Vector2` hotspot. Unity has no built-in cursor constants
  (pointer, crosshair, etc.) like CSS, making it of limited practical value.

**Possible fix for transitions:** Introduce a `Transition` helper struct:
```csharp
new Style {
    Transitions = new[] {
        new Transition("opacity", 0.3f, EasingMode.EaseInOut),
        new Transition("width",   0.5f, EasingMode.Linear, delay: 0.1f),
    }
}
```
PropsApplier would compare via a custom equality check and set all four
`IStyle.transition*` properties together.

**Files:**
- `Shared/Props/PropsApplier.cs` — stubs at lines ~508-517
- `Shared/Props/Typed/Style.cs` — no typed properties for these yet

**Priority:** Medium — transitions are useful for polish but not blocking.

---

## ~~Unity version compatibility tracking and enforcement~~ ✅ IMPLEMENTED

**Status:** ✅ Infrastructure implemented — see `Plans~/VERSIONING_PROCESS.md` for the
full process, coverage matrices, and implementation checklists.

**What was built (scaffolding):**
- `UnityVersion` value type with parsing, comparison, and display (`lsp-server/UnityVersion.cs`)
- Schema model extended with `sinceUnity`, `deprecatedIn`, `removedIn` fields on
  `ElementInfo`, `AttributeInfo`, and a new `styleVersions` section with `VersionInfo`
- `ReferenceAssemblyLocator` now detects and exposes `DetectedVersion` from `ProjectVersion.txt`
- `CompletionHandler` annotates completion items requiring newer Unity (⚠️ prefix, sorted lower)
- `DiagnosticsPublisher` emits `UITKX0200` warnings for elements requiring newer Unity
- `DiagnosticCodes.VersionMismatch` = `"UITKX0200"` added to language-lib
- Version-awareness tests in `Tests/VersionTests.cs`
- Full version coverage matrix (IStyle properties + elements) in the process doc

**What remains (per-version work):**
- Add `sinceUnity` annotations to schema entries for 6.3+ features
- Implement `#if UNITY_6000_3_OR_NEWER` guards in PropsApplier, Style, CssHelpers, StyleKeys
- Build automated API diff script (`scripts/unity-api-diff/`)
- CI version matrix testing

---

## ~~Centralized changelog for IDE extensions~~ ✅ IMPLEMENTED

Implemented — `ide-extensions~/changelog.json` is the single source of truth.
`scripts/changelog.mjs` provides `add`, `extract`, `extract-overview`, and
`import` commands. CI pipeline (`publish.yml`) generates per-IDE changelogs
before packaging. Per-IDE files (`CHANGELOG.md`, `overview.md`) are gitignored.
AI instructions in `.github/instructions/changelog.instructions.md`.

**Problem:** We ship multiple IDE extensions (VS Code, Visual Studio 2022,
eventually Rider) that share the same language-lib and lsp-server core. Each
extension currently has no formal changelog, and changes are scattered across
git commits with no user-facing release notes.

**Impact:** Users have no way to know what changed between extension updates.
Extension marketplaces (VS Code Marketplace, VS Gallery) expect changelogs.
Internal contributors don't have a single source of truth for what shipped when.

**Requirements:**

1. **Single source of truth** — One `CHANGELOG.md` (or structured format like
   `changelogs/` directory with per-version files) that all extensions reference.

2. **Per-component tagging** — Each changelog entry should tag which layer it
   affects: `[language-lib]`, `[lsp-server]`, `[vscode]`, `[vs2022]`, `[rider]`,
   `[grammar]`. Extensions filter entries relevant to them when generating their
   marketplace changelog.

3. **Automation options:**
   - **Conventional commits** — enforce `feat(lsp):`, `fix(vscode):` prefixes,
     then auto-generate changelog from git log
   - **Keep a Changelog** format — manually curated but following a standard
   - **Hybrid** — auto-generated draft from commits, manually curated before release

4. **Extension-specific output** — Build scripts that extract relevant entries
   for each extension's marketplace page:
   - VS Code: `ide-extensions~/vscode/CHANGELOG.md`
   - VS2022: `ide-extensions~/visual-studio/CHANGELOG.md`
   - Rider: `ide-extensions~/rider/CHANGELOG.md`

5. **Version synchronization** — All extensions sharing the same lsp-server
   should publish the same LSP version number alongside their own extension
   version.

**Files to create/modify:**
- `ide-extensions~/CHANGELOG.md` — centralized changelog (new)
- `ide-extensions~/vscode/package.json` — reference changelog
- `ide-extensions~/visual-studio/` — reference changelog
- `scripts/` — changelog generation script (new)

**Priority:** Medium — needed before public marketplace release.

---

## ~~Package-level CHANGELOG.md~~ ✅ DONE

Implemented — `CHANGELOG.md` exists at the package root.

---

## ~~Documentation versioning strategy~~ ✅ IMPLEMENTED

Implemented — `versionManifest.ts` drives version-aware docs with a version
selector dropdown, filtered sidebar, and `sinceUnity` annotations. Docs are
built from `ReactiveUIToolKitDocs~/` with version data injected at build time.

---

## Per-component / per-style Unity docs deep-links with version badge

**Problem:** Component documentation pages and style property tables don't link
to the corresponding Unity documentation page for that specific element or USS
property, versioned to match the user's selected Unity version.

**Desired behavior:**
- Each component page (both C# and UITKX tracks) shows a version badge
  (e.g. "Unity 6.2") next to the component name, linking to the Unity manual
  page for that element at the correct docs version.
- The style property type table on the Styling page shows per-property version
  badges when a property was added after the floor version (e.g. `aspectRatio`
  shows "6.3+" badge, linking to the Unity IStyle docs for 6.3).
- Links use the version selected in the docs site version dropdown, so selecting
  "6.3" points all Unity doc links to `docs.unity3d.com/6000.3/`.

**Example:**
```
Button  [Unity 6.2] → https://docs.unity3d.com/6000.2/.../UIE-uxml-element-Button.html
aspectRatio  [6.3+] → https://docs.unity3d.com/6000.3/.../UIElements.IStyle.html
```

**Files to modify:**
- `ReactiveUIToolKitDocs~/src/versionManifest.ts` — version data already tracks this
- `ReactiveUIToolKitDocs~/src/components/UnityDocsSection/UnityDocsSection.tsx` — use selected version
- `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/StylingPage.tsx` — add per-property badges
- Individual component pages — add inline version badge + link

**Priority:** Low — nice-to-have polish, not blocking.

---

## ~~Autocomplete overwrites existing attribute value binding~~ ✅ FIXED

Fixed — `AttributeItems` now receives a `hasExistingBinding` flag computed by
`HasExistingBinding()`, which scans past the remaining identifier chars and
whitespace after the cursor. When `=` is found, completion items emit only the
attribute name (plain text) instead of `name={$1}` / `name="$1"` snippets.
The existing `={value}` binding is preserved by the LSP client.

Fixed — `AttributeItems` now receives a `hasExistingBinding` flag computed by
`HasExistingBinding()`, which scans past the remaining identifier chars and
whitespace after the cursor. When `=` is found, completion items emit only the
attribute name (plain text) instead of `name={$1}` / `name="$1"` snippets.
The existing `={value}` binding is preserved by the LSP client.

**Symptom:** When editing an attribute value like `sprite={bg}` — double-clicking
`sprite` to select it, then typing to trigger autocomplete — selecting a completion
item (e.g. `texture`) produces broken output:

```uitkx
<!-- Before: cursor inside attribute name -->
<Image sprite={bg} />

<!-- After selecting "texture" from autocomplete -->
<Image texture={}={bg} />
```

The completion inserts `texture={}` as a full attribute snippet instead of replacing
only the attribute name. The existing `={bg}` binding is left in place, producing
invalid syntax with two `=` signs.

**Expected:** Autocomplete should replace only the attribute name, preserving the
existing `={value}` binding:

```uitkx
<!-- Expected result -->
<Image texture={bg} />
```

**Root cause (likely):** The completion item's `textEdit` range covers only the
selected word, and the `insertText`/`insertTextFormat` includes `=$1{}` or `={}`.
When there's already an `={...}` after the attribute name, the completion should
detect the existing binding and only replace the name portion.

**Files to investigate:**
- `ide-extensions~/lsp-server/CompletionHandler.cs` — attribute name completions
  should check if the cursor is followed by `={` and adjust the insert text
- `ide-extensions~/language-lib/` — completion item building

---

## ~~HMR Window: memory usage tracking~~ ✅ DONE

Implemented — HMR window shows live RAM (working set via Win32 P/Invoke), delta
since window open, and delta since HMR session start. Refreshes every 2 seconds
via `EditorApplication.update` timer. Non-Windows falls back to Unity Profiler APIs.
