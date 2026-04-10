# VS2022 Extension — Tech Debt

Status tracker for VS2022-specific issues found during the VS Code cleanup pass.

---

## VS-1: FilesToWatch is null — no workspace/didChangeWatchedFiles

**File:** `UitkxLanguageClient.cs` → `FilesToWatch => null`
**Impact:** Medium — the LSP server's `WatchedFilesHandler` never fires for file create/delete/rename events in VS2022. New `.uitkx` files created outside the editor won't be indexed until the server restarts.
**VS Code equivalent:** `vscode.workspace.createFileSystemWatcher('**/*.uitkx')` registered in `extension.ts`.
**Fix:** Set `FilesToWatch` to `new[] { "**/*.uitkx" }`. Needs testing — VS2022's `ILanguageClient` framework may or may not convert this to the LSP `workspace/didChangeWatchedFiles` capability correctly.

---

## VS-2: Go-To-Definition / Find-References — verify they work

**Files:** No `UitkxGoToDefinitionHandler.cs` or `UitkxFindReferencesHandler.cs` exist.
**Impact:** Unknown — the LSP server registers `DefinitionHandler` and `ReferencesHandler`, and `CapabilityPatchStream` injects `definitionProvider:true` / `referencesProvider:true`. VS2022 may auto-route F12/Shift+F12 via native LSP routing since the middleware includes these methods in `NeedsBufferSync`.
**Fix:** F5 test with Experimental Instance. If it works, update `README.md` to remove "not yet supported" note. If it doesn't, implement handlers using `InternalRpc` like completion/hover.

---

## VS-3: @ completion — verify no double-@ insertion

**Impact:** Low-Medium — VS Code has middleware that strips leading `@` from completion items when trigger char is `@`. VS2022's `UitkxCompletionSource` uses `InitializeCompletion` to walk back over `@` in the applicable span. If insertion produces `@@namespace` instead of `@namespace`, needs a fix in the completion source.
**Fix:** F5 test typing `@` and selecting a directive completion.

---

## VS-4: Format-on-save not implemented

**Impact:** Low — VS Code has format-on-save via `DocumentFormattingEditProvider`. VS2022 only supports manual Ctrl+K,Ctrl+D formatting. No format-on-save handler exists.
**Fix:** Future enhancement — implement `ITextViewCreationListener` that hooks `ITextDocument.FileActionOccurred` and triggers format on save.

---

## VS-5: Source code comments still reference old .cs companion pattern

**Files:**
- `Editor/HMR/UitkxHmrFileWatcher.cs` — lines 11, 18, 117, 188
- `Editor/HMR/UitkxHmrController.cs` — lines 266-267
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — line 109
- `SourceGenerator~/Diagnostics/UitkxDiagnostics.cs` — line 68
- `UitkxVsix/UitkxRenameHandler.cs` — line 87

**Impact:** Low — code comments only, no functional impact. HMR system may still legitimately handle `.cs` files at runtime.
**Fix:** Update comments to reflect `.uitkx` hook/module companion pattern when touching these files.

---

## ✅ FIXED: Hover BuildAdornment inline bold parsing

**File:** `UitkxQuickInfoSource.cs` → `BuildAdornment()`
**Issue:** New hover format `**(kind)** \`symbol\` : \`type\`` wasn't parsed — only full-line bold `**...**` was handled, leaving literal `**` characters in hover tooltip.
**Fix:** Added `**(` prefix detection with `)**` extraction before the existing full-line bold check.

## ✅ FIXED: Classifier missing hook/module keywords

**File:** `UitkxClassifier.cs` → `_keywords` HashSet
**Issue:** `hook` and `module` weren't in the keyword set. They're bare keywords (no `@` prefix), so they rendered as plain identifiers.
**Fix:** Added `"hook"` and `"module"` to `_keywords`. The hook name gets method coloring (followed by `(`), module name gets identifier coloring.
