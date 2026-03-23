# Tech Debt

## ~~Inspector "Go to Script" broken for `.uitkx`-generated classes~~ ✅ FIXED

Fixed — `TryOpenViaVisualStudioComIntegration()` now correctly verifies
process success before consuming the event.

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

## ~~LSP diagnostics not updated when companion `.cs` file changes~~ ✅ FIXED

Fixed — TextSyncHandler now registers for `**/*.cs` in addition to `**/*.uitkx`.
When a companion `.cs` file is edited, the handler sets a companion overlay and
re-publishes diagnostics for all `.uitkx` files in the same directory automatically.

---

## ~~Rename Symbol (F2) not working in `.uitkx` files~~ ✅ FIXED

Fixed — bidirectional rename between `.uitkx` ↔ `.cs` companion files works.

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

## ~~Go to Definition (F12) not working for companion symbols in `.uitkx`~~ ✅ FIXED

Fixed — Roslyn-based symbol resolution added. Works for same-file vars,
companion `.cs` symbols, and multi-line attribute values.

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
