# UITKX P8.6 — Bug & Feature Checklist

> Tracked as of v1.0.50 · March 2026

---

## 🔴 Critical / Blocking

- [x] **#1 CS compilation errors** — Fixed in v1.0.41.
  Root cause: `JsxCommentNode` (v1.0.40) was being included in `EmitChildArgs` with a comma separator but emitting nothing, producing `V.Box(V.Label(...), , V.Label(...))`. Root-level `JsxCommentNode` also caused unwanted Fragment wrapping.
  Fix: `EmitChildArgs` skips comment nodes and adjusts comma logic. `BuildSource` filters them from markup node list.
  ⚠️ **Unity: reimport scripts** (Assets → Reimport All) to pick up the updated `Analyzers/ReactiveUITK.SourceGenerator.dll`.

---

## 🟠 Editor Features (Broken)

- [x] **#2 `@code` block not colored** — Fixed in v1.0.44, stabilized through v1.0.50.
  Root cause: the LSP server emitted no semantic tokens for the C# content inside `@code { }`, leaving it to the TextMate grammar alone (which produced only basic coloring). The `contentName` injection approach (v1.0.42) broke markup coloring — reverted in v1.0.43.
  Fix: `SemanticTokensProvider` scans the `@code` body line-by-line with a composite regex and emits `keyword`, `function`, `type`, `variable`, `string`, `number`, and `comment` semantic tokens for C# constructs. Follow-up fixes (v1.0.48-v1.0.50) prevent semantic-token bleed into embedded markup/comment spans and improve comment-scope grammar inside `@code`.
  *(Note: elements inside `{/* */}` do get colored — so the issue was specific to the `@code` region.)*

- [x] **#3 Setter hover missing** — Fixed in v1.0.51 (regression polished in v1.0.52).
  Fix: `HoverHandler` now resolves hook tuple setter identifiers in `@code` (both declaration and usage sites, e.g. `setCount(...)`) and returns hook-setter hover docs.

- [ ] **#4 `{/*` auto-close wraps wrong** — Deferred by product decision for this cycle.
  Current behavior accepted for now; can be revisited post-8.6.

- [x] **#5 `Ctrl+/` in markup** — Fixed in v1.0.47.
  Fix: added `uitkx.toggleBlockComment` command + keybinding override so `Ctrl+/`/`Cmd+/` is context-aware in `.uitkx` files: markup toggles `{/* */}`, non-markup code in `@code` toggles `//`. Selection range normalization wraps from first non-whitespace to end of last touched line.

- [ ] **#6 Unreachable code not grayed** — everything *after* a `return` statement (anywhere in the file) should be dimmed.
  Currently no dimming because UITKX semantic tokens suppress OmniSharp's unreachable-code decoration.

- [ ] **#7 Unity console click navigation** — clicking / double-clicking / Ctrl+clicking a `.uitkx` compile error in the Unity console does nothing.

- [ ] **#8 Missing `;` auto-fix on save/format (default ON, opt-out)** — formatter should insert missing semicolons in safe statement-ending contexts.
  Scope: run during format/save (not while typing), deterministic and cross-IDE friendly.
  Config: default enabled; users can opt out via formatter option (e.g. `insertMissingSemicolonsOnFormat: false`).

---

## ✅ Confirmed Fixed

- [x] **Parser break** on `@` / `<` in attributes — v1.0.36
- [x] **Squiggle position** on `<TagName` (was column 0) — v1.0.37
- [x] **Hover word classification** (`Empty` → `Word`) — v1.0.39
- [x] **Formatter erasing `{/* */}` comments** — v1.0.40 (`JsxCommentNode` AST)
- [x] **`@code` inner markup indentation** — working correctly

---

## Notes

- **#2 vs #6**: Coloring inside `@code` and OmniSharp dimming are related — UITKX currently emits semantic tokens across the whole file, which prevents OmniSharp from applying its own decorations. A proper fix would scope UITKX tokens to only the markup region and leave the `@code` region to OmniSharp.
- **#4 / #5**: Both require a `vscode.commands.registerTextEditorCommand` override for the comment-toggle command (`editor.action.blockComment`) scoped to the `uitkx` language.
