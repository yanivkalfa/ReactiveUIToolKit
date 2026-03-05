# UITKX P8.6 — Bug & Feature Checklist

> Tracked as of v1.0.43 · March 2026

---

## 🔴 Critical / Blocking

- [x] **#1 CS compilation errors** — Fixed in v1.0.41.
  Root cause: `JsxCommentNode` (v1.0.40) was being included in `EmitChildArgs` with a comma separator but emitting nothing, producing `V.Box(V.Label(...), , V.Label(...))`. Root-level `JsxCommentNode` also caused unwanted Fragment wrapping.
  Fix: `EmitChildArgs` skips comment nodes and adjusts comma logic. `BuildSource` filters them from markup node list.
  ⚠️ **Unity: reimport scripts** (Assets → Reimport All) to pick up the updated `Analyzers/ReactiveUITK.SourceGenerator.dll`.

---

## 🟠 Editor Features (Broken)

- [ ] **#2 `@code` block not colored** — identifiers, variables, hook calls inside `@code { }` have no semantic coloring.
  *(Note: elements inside `{/* */}` do get colored — so the issue is specific to the `@code` region.)*
  ⚠️ Attempted `contentName: "source.cs.embedded.uitkx"` in v1.0.42 — **reverted in v1.0.43**: the C# grammar injection overrides all the UITKX patterns inside `@code`, breaking markup coloring (tags, `{}`, `/` etc. all wrong colors). Needs a different approach: inject targeted token patterns (keywords, types, identifiers) as explicit rules in `code-block-body` without delegating the whole scope to C#.

- [ ] **#3 Setter hover missing** — `setCount`, `setMode`, etc. show no hover docs.
  The hover handler covers tags/attributes but not C# identifiers inside `@code`.

- [ ] **#4 `{/*` auto-close wraps wrong** — typing `{/*` closes immediately at cursor `{/* | */}`.
  Should instead wrap the selected text / nearby element.

- [ ] **#5 `Ctrl+/` in markup** — should toggle `{/* */}` around selected lines/element as a block comment.
  Currently inserts `//` (line comment) or does nothing useful in markup context.

- [ ] **#6 Unreachable code not grayed** — everything *after* a `return` statement (anywhere in the file) should be dimmed.
  Currently no dimming because UITKX semantic tokens suppress OmniSharp's unreachable-code decoration.

- [ ] **#7 Unity console click navigation** — clicking / double-clicking / Ctrl+clicking a `.uitkx` compile error in the Unity console does nothing.

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
