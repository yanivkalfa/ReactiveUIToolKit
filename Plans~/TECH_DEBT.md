# Tech Debt

## Inspector "Go to Script" broken for `.uitkx`-generated classes

**Symptom:** Clicking on a generated script reference in the Unity Inspector
opens Visual Studio but navigates to **no file at all**. The IDE window
appears blank — it should navigate to the `.cs` file that was clicked.

**Root cause:** `TryOpenViaVisualStudioComIntegration()` in
`Editor/UitkxConsoleNavigation.cs` (line ~245) **unconditionally returns
`true`** after calling `Process.Start()` on `COMIntegration.exe`, without
verifying the process actually opened the file. Because `Process.Start()` is
asynchronous and doesn't validate file paths, the callback claims success
immediately, **consuming the event** so Unity's default handler never runs.
If COMIntegration.exe then fails silently (malformed path, crash, async
timing), Visual Studio is open but has no file.

**Exact flow:**
1. User clicks script reference in Inspector for a `.g.cs` asset.
2. `OnOpenAsset(-10000)` fires → `HandleOnOpenAsset()` resolves `.g.cs` →
   `.uitkx` via `#line` directives in `TryResolveUitkxTarget()`.
3. `TryOpenViaConfiguredCodeEditor()` returns false.
4. `TryOpenViaConfiguredEditorExecutable()` detects "devenv" in the editor
   path → calls `TryOpenViaVisualStudioComIntegration()`.
5. `Process.Start(psi)` fires `COMIntegration.exe` asynchronously.
6. Method returns `true` immediately → event consumed → Unity fallback
   (`TryOpenViaUnityDefaultEditor`) is **never attempted**.
7. If COMIntegration.exe fails silently → VS is open, no file shown.

**Fix:** Change `TryOpenViaVisualStudioComIntegration()` to return `false`
instead of `true` (so Unity's default handler still fires as a fallback),
or verify process success before returning `true`:
```csharp
var process = Process.Start(psi);
return process != null && !process.HasExited;
```

**File to fix:**
- `Editor/UitkxConsoleNavigation.cs` — line ~245 in
  `TryOpenViaVisualStudioComIntegration()`

**Priority:** Medium — does not affect runtime, but hurts editor workflow.

---

## Unused variables in `.uitkx` not highlighted by analyzer

**Symptom:** Declaring an unused local variable in a `.uitkx` file (e.g.
`var btnTextStyle = new Style { ... };` in `DiabloMenuDemoFunc.uitkx`) does
not produce a red/grey "unused" diagnostic in the editor.

**Expected:** The Roslyn analyzer or source generator should emit a warning
(e.g. `CS0219` or a custom `UITKX` diagnostic) for unused locals, matching
standard C# IDE behaviour.

**Example:**
```csharp
// DiabloMenuDemoFunc.uitkx — this variable is never referenced
var btnTextStyle = new Style {
    (StyleKeys.TextColor, TextLight),
    (StyleKeys.FontSize, 11f),
};
```

**Files to investigate:**
- `SourceGenerator~/` — check if generated `.g.cs` preserves local declarations
  in a way that Roslyn can detect them as unused
- `Analyzers/ReactiveUITK.Language.dll` — check if a custom analyzer suppresses
  `CS0219` / `IDE0059` for `.uitkx`-originated code
- LSP server (`ide-extensions~/lsp-server/`) — check if unused-variable
  diagnostics are forwarded from the generated C# back to `.uitkx` source positions

**Priority:** Low — cosmetic, but important for developer experience.

---

## LSP diagnostics not updated when companion `.cs` file changes

**Symptom:** Moving a symbol from the `.uitkx` file into a companion `.cs`
partial class does not clear the red error squiggle until the `.uitkx` file
itself is edited (e.g. adding/removing a character).

**Reproduction:**
1. `DiabloMenuDemoFunc.uitkx` has a local:
   ```csharp
   var nameStyle = new Style {
       (StyleKeys.TextColor, GoldAccent),
       (StyleKeys.FontSize, 14f),
   };
   ```
2. Delete it from the `.uitkx` → references turn red (expected).
3. Add `nameStyle` as a static field in `DiabloMenuDemoFunc.styles.cs`
   (the companion partial class).
4. The `.uitkx` still shows red — the error does not resolve until you
   make a dummy edit in the `.uitkx` file.

**Expected:** Saving the companion `.cs` file should trigger the LSP to
re-evaluate diagnostics for the `.uitkx` file automatically.

**Files to investigate:**
- LSP server file-watcher / `workspace/didChangeWatchedFiles` handler
- Whether companion `.cs` changes trigger a re-compilation of the
  virtual `.g.cs` document used for diagnostics

**Priority:** Medium — common workflow friction when refactoring into companions.

---

## Rename Symbol (F2) not working in `.uitkx` files

**Symptom:** Placing the cursor on a local variable in a `.uitkx` file and
pressing F2 (Rename Symbol) does nothing.

**Reproduction:**
1. In `DiabloMenuDemoFunc.uitkx`, place cursor on `el` in:
   ```csharp
   var el = rootRef?.Current;
   if (el == null) {
       return null;
   }
   var h = el.schedule.Execute(() => {
   ```
2. Press F2 — nothing happens, no rename dialog appears.

**Expected:** Rename Symbol should work for local variables, renaming all
occurrences within the component body.

**Files to investigate:**
- LSP server `textDocument/rename` and `textDocument/prepareRename` handlers
- Whether the LSP maps `.uitkx` positions to the generated `.g.cs` correctly
  for the rename provider

**Priority:** Medium — important IDE feature for refactoring.

---

## Type mismatch in companion `.cs` not surfaced in `.uitkx`

**Symptom:** A companion `.cs` file declares a field with the wrong type, but
the `.uitkx` file that uses it does not show a red error squiggle.

**Reproduction:**
1. In `StatusBar.styles.cs`, declare:
   ```csharp
   public static readonly Color barWidth = 160f;   // ← wrong type, should be float
   public static readonly Color barHeight = 10f;    // ← wrong type, should be float
   ```
2. In `StatusBar.uitkx`, reference them:
   ```csharp
   var fillStyle = new Style {
       (StyleKeys.Width, barWidth * percent),   // Color * float — should be an error
       (StyleKeys.Height, barHeight),
       (StyleKeys.BackgroundColor, barColor),
   };
   ```
3. No error is shown in the `.uitkx` file.

**Expected:** The LSP should report a type mismatch (`CS0019` or similar)
since `Color * float` is not valid for a width style value.

**Likely related to:** The companion `.cs` change not triggering re-evaluation
of `.uitkx` diagnostics (see "LSP diagnostics not updated when companion
`.cs` file changes" above).

**Priority:** High — silent type errors can cause runtime bugs.

---

## Go to Definition (F12) not working for companion symbols in `.uitkx`

**Symptom:** Clicking "Go to Definition" (F12) on a symbol defined in a
companion `.cs` partial class does nothing — the editor does not navigate
to the companion file.

**Reproduction:**
1. In `DiabloMenuDemoFunc.uitkx`, place cursor on `RootStyle`, `TopBarStyle`,
   `MenuButtonStyle`, or any symbol defined in `DiabloMenuDemoFunc.styles.cs`.
2. Press F12 (Go to Definition) — nothing happens.

**Expected:** Should navigate to the declaration in the companion `.cs` file.

**Files to investigate:**
- LSP server `textDocument/definition` handler
- Whether the handler resolves symbols from companion `.cs` files or only
  within the generated `.g.cs` virtual document

**Priority:** High — core navigation feature, blocks efficient use of
companion file pattern.

---

## `.meta` files visible in VS Code editor

**Symptom:** Unity `.meta` files appear in the VS Code file explorer and
open tabs. They should be hidden.

**Fix:** Add `*.meta` to `files.exclude` in workspace settings or the
`.code-workspace` file:
```json
"files.exclude": {
    "**/*.meta": true
}
```

**File to update:**
- `ReactiveUIToolKit.code-workspace` — add to `settings.files.exclude`

**Priority:** Low — cosmetic annoyance.

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

## No declarative USS stylesheet loading from `.uitkx`

**Symptom:** There is no way to load a `.uss` file from within a `.uitkx`
component or from the UITKX framework. Users must manually load USS in their
bootstrap code via `rootVisualElement.styleSheets.Add(...)`.

**Context:** `className` is fully wired in the library (`PropsApplier` calls
`AddToClassList`/`RemoveFromClassList`), so USS class selectors do match on
UITKX-rendered elements. But without a way to load the stylesheet, `className`
is effectively useless for USS-based styling.

**Possible solutions:**
- An `@uss` directive in `.uitkx` files (e.g. `@uss "./MyComponent.uss"`)
- A `styleSheet` prop on root elements / `VisualElementSafe`
- Auto-discovery of co-located `.uss` files (e.g. `PlayerCard.uss` next to
  `PlayerCard.uitkx`)

**Priority:** Medium — unlocks USS pseudo-state styling (`:hover`, `:active`,
`:focus`) which inline styles cannot achieve.

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

## Add UI Toolkit Debugger shortcut to ReactiveUITK menu + HMR keybinding

**Feature:** Add a menu item under `ReactiveUITK/` that opens the Unity
UI Toolkit Debugger (`Window > UI Toolkit > Debugger`). The Unity menu
command is `Window/UI Toolkit/Debugger`.

**Additionally:** Add a keybinding in the HMR window to toggle the
UI Toolkit Debugger on/off for quick access during development.

**Files to update:**
- `Editor/FiberMenu.cs` — add `[MenuItem("ReactiveUITK/UI Toolkit Debugger")]`
  that calls `EditorApplication.ExecuteMenuItem("Window/UI Toolkit/Debugger")`
- HMR window — add a toggle button or keyboard shortcut

**Priority:** Low — quality-of-life convenience.
