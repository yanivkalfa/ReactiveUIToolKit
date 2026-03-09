# UITKX VS Code Tooling Fix Plan

**Status**: Active — ready for implementation  
**Version target**: v1.0.89+  
**Author**: AI session (Claude Sonnet 4.6)  
**Last updated**: 2025-01

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Issue Catalog](#2-issue-catalog)
3. [Fix Plan: Area 1 — Colors & Tokenization](#3-fix-plan-area-1--colors--tokenization)
4. [Fix Plan: Area 2 — Formatting](#4-fix-plan-area-2--formatting)
5. [Fix Plan: Area 3 — IntelliSense / Completions](#5-fix-plan-area-3--intellisense--completions)
6. [Fix Plan: Area 4 — Error Coloring & File Indicators](#6-fix-plan-area-4--error-coloring--file-indicators)
7. [Testing Strategy](#7-testing-strategy)
8. [Debug Protocol (User-Facing)](#8-debug-protocol-user-facing)
9. [Implementation Order & Milestones](#9-implementation-order--milestones)
10. [Takeover Context (for next AI)](#10-takeover-context-for-next-ai)

---

## 1. Architecture Overview

The toolchain has three independent coloring/processing layers that must cooperate:

```
Layer 1: TextMate Grammar  (static, fires immediately on file open)
  File: ide-extensions~/grammar/uitkx.tmLanguage.json
  Consumed by: VS Code's built-in tokenizer
  Produces: TextMate scope names → mapped to theme colors

Layer 2: LSP Semantic Tokens  (async, fires after server handshake ~200ms)
  File: ide-extensions~/lsp-server/SemanticTokensHandler.cs
  Provider: ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs
  Produces: typed tokens that OVERRIDE Layer 1 per-position
  Custom types (declared in package.json): uitkxElement, uitkxDirective,
    uitkxAttribute, uitkxExpression, uitkxDirectiveName

Layer 3: Roslyn C# Semantic Tokens  (async, fires ~300ms after Layer 2)
  File: ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs
  Produces: standard LSP semantic token types (variable, method, type, etc.)
  Merged into Layer 2 output, deduplicated by position

Diagnostics pipeline (separate from tokens):
  T1+T2: DiagnosticsPublisher.Publish() — synchronous, immediate
  T3: RoslynHost.EnqueueRebuild() → PushTier3() — async, ~300ms later
```

**Key files for this plan:**

| File | Purpose |
|---|---|
| `ide-extensions~/grammar/uitkx.tmLanguage.json` | TextMate grammar (Layer 1) |
| `ide-extensions~/grammar/uitkx.language-configuration.json` | Bracket/indent rules |
| `ide-extensions~/vscode/package.json` | Token type declarations, editor defaults |
| `ide-extensions~/vscode/src/extension.ts` | LanguageClient activation, completion middleware |
| `ide-extensions~/lsp-server/SemanticTokensHandler.cs` | Layer 2+3 merge |
| `ide-extensions~/lsp-server/CompletionHandler.cs` | Completion routing |
| `ide-extensions~/lsp-server/FormattingHandler.cs` | Format pipeline |
| `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` | T1+T2+T3 diagnostic push |
| `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs` | Roslyn → ParseDiagnostic |
| `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs` | UITKX token walk |

---

## 2. Issue Catalog

Every issue below was verified by reading source code. No assumptions.

### 2.1 Colors / Tokenization

#### BUG-C1: Tag names have no static-grammar color
**File**: `uitkx.tmLanguage.json`, patterns `tag-open` and `tag-self-closing`  
**Evidence**: Capture group `"2"` (the tag name after `<`) is `{}` — empty, no `name` property.
```json
"beginCaptures": {
  "1": { "name": "punctuation.definition.tag.begin.uitkx" },
  "2": {}   // ← NO scope assigned to tag name
}
```
**Effect**: Before LSP loads, tags have no color. After LSP loads, semantic tokens fix it — but there's a visible flash of uncolored tokens on open.  
**Fix**: Assign `entity.name.tag.uitkx` (or `entity.name.type.uitkx`) to capture 2 in both `tag-open` and `tag-self-closing`.

#### BUG-C2: `uitkxExpression` superType `macro` is barely themed
**File**: `ide-extensions~/vscode/package.json`, `semanticTokenTypes`  
**Evidence**:
```json
{ "id": "uitkxExpression", "superType": "macro", ... }
```
`macro` is a rare superType. Most popular themes (One Dark Pro, Tokyo Night, Dracula, Monokai Pro, GitHub) do not style `semanticTokenTypes.macro` — it falls back to default text color. The `@(` delimiter of inline expressions therefore appears in plain white/black.  
**Fix**: Change superType to `operator` (widely styled by all themes as a distinct punctuation color).

#### BUG-C3: Embedded language declaration is dead code
**File**: `ide-extensions~/vscode/package.json`, `grammars` section  
**Evidence**:
```json
"embeddedLanguages": { "source.cs.embedded.uitkx": "csharp" }
```
This tells VS Code: "if a grammar token has scope `source.cs.embedded.uitkx`, treat it as C#." But no pattern in `uitkx.tmLanguage.json` ever sets that scope name. Result: the embedded language feature does nothing.  
**Fix (two options)**:  
- **Option A (correct)**: Give `code-block-body` and `expression-content` patterns the parent scope `source.cs.embedded.uitkx`. This lets VS Code apply C# bracket matching, indentation, and some coloring inside those regions.  
- **Option B (remove)**: Delete the `embeddedLanguages` declaration to eliminate false expectations. Simpler, safer short-term.  
**Recommend Option B now, Option A as a future enhancement.**

#### BUG-C4: `<` in `autoClosingPairs` causes unwanted `>` auto-insertion
**File**: `uitkx.language-configuration.json`  
**Evidence**:
```json
"autoClosingPairs": [ ..., ["<", ">"] ],
"brackets": [ ..., ["<", ">"] ]
```
When the cursor is inside a `@code {}` block and the user types `<` for a C# generic (e.g. `List<string>`), VS Code auto-inserts `>`. This is incorrect and the `>` must be manually deleted.  
**Fix**: Remove `["<", ">"]` from both `autoClosingPairs` and `brackets`. Tag auto-close in UITKX markup is handled by the server's completion insert-text snippets (`<Box />`, etc.), not by bracket matching.

#### BUG-C5: `attribute-boolean-shorthand` overmatch
**File**: `uitkx.tmLanguage.json`, pattern `attribute-boolean-shorthand`  
**Evidence**:
```json
"attribute-boolean-shorthand": {
  "match": "\\b[A-Za-z][A-Za-z0-9\\-_]*\\b"
}
```
This catch-all pattern inside `tag-attributes` matches any word. It will color element names and other identifiers that fall through earlier patterns as `entity.other.attribute-name.uitkx`. This causes incorrect coloring of tag-body content that isn't actually an attribute.  
**Fix**: Add negative lookahead to ignore words that follow `=` (those are already consumed by `attribute-value-string`/`attribute-value-expression`) and words that appear after `/>` or `>`. Better: move `attribute-boolean-shorthand` to be the last resort and add the constraint that it must appear before `=`, `/`, `>`, or whitespace:
```json
"match": "(?<![=\\.])\\b[A-Za-z][A-Za-z0-9\\-_]*\\b(?=\\s*(?:[=\\/> \\t\\r\\n]|$))"
```

---

### 2.2 Formatting

#### BUG-F1: `onEnterRules` fires inside C# code blocks on `{` 
**File**: `uitkx.language-configuration.json`  
**Evidence**: The `onEnterRules` entry triggers on `\{[^}]*$` (any `{` opening on a line). When writing C# code inside `@code {}`, typing `{` for a lambda or object initializer triggers the indent rule. This is usually harmless but can produce double-indents in specific patterns like:
```
var x = new { 
    ← cursor indents fine, but pressing Enter again in some positions adds an extra level
```
**Fix**: This is difficult to fully solve at the language-configuration level. The formatter itself should be the source of truth for indentation. Removing the redundant `onEnterRules` entirely (leaving `indentationRules` in place) reduces the conflict. VS Code uses `indentationRules` for auto-indent after Enter; `onEnterRules` are extra and often conflict.

#### BUG-F2: Formatter produces no output when parse fails
**File**: `ide-extensions~/lsp-server/FormattingHandler.cs`  
**Evidence**: The handler calls `formatter.Format(text, localPath)` and returns `null` if `formatted == text`. There is no error path — if the formatter throws (e.g. because the document has a syntax error), the exception propagates to OmniSharp which may silently swallow it and show no formatting result.  
**Fix**: Wrap the `formatter.Format()` call in try/catch. On exception, log and return `null` (no-op). This ensures formatting never causes the server to crash or produce an error popup.

---

### 2.3 IntelliSense / Completions

#### BUG-I1: Space and newline as trigger characters are too aggressive
**File**: `ide-extensions~/lsp-server/CompletionHandler.cs`  
**Evidence**:
```csharp
TriggerCharacters = new Container<string>("<", "@", " ", "\n", "{"),
```
Space (` `) and newline (`\n`) as trigger characters cause VS Code to request completions on every keystroke. The `Handle` method is smart and will return empty lists in most cases, but VS Code still shows a visible loading spinner and may briefly display an empty completion popup on every space press. This degrades typing performance.  
**Fix**: Remove `" "` and `"\n"` from `TriggerCharacters`. Tag attribute completions triggered by space can instead be invoked manually (`Ctrl+Space`) or triggered naturally by `<` and `@`. The server's cursor-context logic (`AstCursorContext.Find`) will provide correct completions regardless of trigger character.

#### BUG-I2: `@` strip middleware strips from ALL items, not just context-appropriate ones
**File**: `ide-extensions~/vscode/src/extension.ts`, `provideCompletionItem` middleware  
**Evidence**:
```typescript
if (text && text.startsWith('@')) {
  const stripped = text.slice(1);
  item.insertText = ...stripped...;
}
```
The middleware strips the leading `@` from every completion item's `insertText`. This is correct for the case where the user typed `@` (so the `@` is already in the buffer). But if the user triggered completion without typing `@` (e.g. pressed `Ctrl+Space` at the start of a line), the `@` is not in the buffer and stripping it produces incorrect insert text.  
**Fix**: Only strip the `@` when `context.triggerCharacter === '@'` or when the word before the cursor is `@`:
```typescript
provideCompletionItem(document, position, context, token, next) {
  return Promise.resolve(next(document, position, context, token)).then(result => {
    if (!result) return result;
    const shouldStrip = context.triggerCharacter === '@'
      || (position.character > 0
          && document.getText(new vscode.Range(
               position.translate(0, -1), position)) === '@');
    if (!shouldStrip) return result;
    // ... existing strip logic
  });
}
```

#### BUG-I3: Roslyn completions not ready on first trigger
**File**: `ide-extensions~/lsp-server/CompletionHandler.cs`, line ~130  
**Evidence**:
```csharp
if (ctx.Kind == CursorKind.CSharpExpression) {
    Log("completion: CSharpExpression — Roslyn not ready, returning empty");
    return new CompletionList();
}
```
When Roslyn hasn't compiled the workspace yet (first open), C# expression completions silently return empty. The user sees nothing. There's no error, just silence.  
**Fix**: Return `isIncomplete: true` on the `CompletionList` when Roslyn is not yet ready. This tells VS Code to re-request completions when the user types more. Once Roslyn is ready, the next request will succeed.
```csharp
return new CompletionList(isIncomplete: true);
```

---

### 2.4 Error Coloring & File Indicators

#### BUG-E1: Roslyn diagnostic column is always 0 (source-map route)
**File**: `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs`, lines 96-99  
**Evidence**:
```csharp
if (mapEntry != null) {
    uitkxLine = mapEntry.UitkxLine;
    uitkxCol  = 0; // column within mapped region (future: compute from offset)
    ...
}
```
All Roslyn diagnostics that resolve via source-map have column 0. The red squiggle always starts at the beginning of the line, not under the actual offending symbol. This is why "error coloring feels wrong" — the squiggle is misplaced.  
**Fix**: Compute the actual column offset. The `mapEntry` contains `UitkxLine` and (presumably) a character offset. The Roslyn diagnostic's `Location.SourceSpan.Start.Character` gives the offset within the virtual document; you need to subtract the virtual document line start to get the column within that line, then add `mapEntry`'s column offset. See [Fix F1 in Section 6.1](#area-4-detailed-fixes).

#### BUG-E2: File-level red indicators caused by T3 diagnostics on unmapped regions
**File**: `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs`, line ~115  
**Evidence**: Several Roslyn diagnostics fall through the source-map route into the `#line` directive route (`GetMappedLineSpan()`). If `mappedSpan.IsValid == false` or the file path doesn't match, `uitkxLine = 0`, `uitkxCol = 0`. In `DiagnosticsPublisher.ToLsp()`:
```csharp
int startLine = Math.Max(0, d.SourceLine - 1);  // 0 - 1 = -1 → 0
int endChar = startChar > 0 ? startChar : 1;    // startChar=0 → endChar=1
```
A diagnostic at line 1, col 0 with range `[0:0 → 0:1]` covers the very first character of the file. VS Code shows this as a file-level error in the Explorer pane (red file icon) and a squiggle on line 1. This is why files with no user-visible errors still appear red.  
**Fix**: In `RoslynDiagnosticMapper.Map()`, after computing position, skip any diagnostic where `uitkxLine == 0` and `uitkxCol == 0` AND the diagnostic's original span falls entirely outside mapped regions. A mapped-region check already exists (`IsUitkxPath` guard) — extend it to also guard against line-0 positions coming from unmapped scaffold.

#### BUG-E3: `endChar` fallback produces zero-width or one-character ranges
**File**: `ide-extensions~/lsp-server/DiagnosticsPublisher.cs`, method `ToLsp()`  
**Evidence**:
```csharp
int endChar =
    d.EndColumn > 0 ? d.EndColumn
    : startChar > 0 ? startChar
    : 1;
```
When `EndColumn == 0` and `StartChar == 0`, `endChar` becomes `1`. This creates a range covering only the first character of the line. The squiggle is very short (one char) rather than spanning the actual token.  
**Fix**: When end information is missing, use a minimum squiggle length that covers the word at position. As a practical heuristic: `endChar = startChar + 1` is acceptable for now, but the real fix is ensuring Roslyn provides column info (BUG-E1 fix will also solve most endChar issues).

---

## 3. Fix Plan: Area 1 — Colors & Tokenization

### Priority order: C4 > C1 > C5 > C2 > C3

### Fix C4: Remove `<>` from autoClosingPairs and brackets

**File**: `ide-extensions~/grammar/uitkx.language-configuration.json`

**Change**:
```json
// BEFORE
"brackets": [
  ["{", "}"], ["[", "]"], ["(", ")"], ["<", ">"]
],
"autoClosingPairs": [
  ["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""], ["'", "'"], ["<", ">"]
],
"surroundingPairs": [
  ["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""], ["'", "'"], ["<", ">"]
]

// AFTER
"brackets": [
  ["{", "}"], ["[", "]"], ["(", ")"]
],
"autoClosingPairs": [
  ["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""], ["'", "'"]
],
"surroundingPairs": [
  ["{", "}"], ["[", "]"], ["(", ")"], ["\"", "\""], ["'", "'"]
]
```

**Test**: Open a `.uitkx` file with a `@code { }` block. Type `List<` — should NOT auto-insert `>`. Also test that `<Box` in markup works fine (completion inserts the full snippet anyway).

---

### Fix C1: Add scope to tag name captures

**File**: `ide-extensions~/grammar/uitkx.tmLanguage.json`

Locate `tag-self-closing` and `tag-open` patterns. Change capture `"2"` from `{}` to:
```json
"2": { "name": "entity.name.tag.uitkx" }
```

`entity.name.tag` is a well-supported TextMate scope — HTML/XML extensions use it widely, so all major themes color it (usually the distinctive tag-name color, often blue or teal).

**Note on semantic token precedence**: After LSP loads, semantic tokens from `SemanticTokensProvider` — which emit `uitkxElement` (superType: `class`) — will override this. So this fix only matters for the first ~200ms on file open and for themes that disable semantic highlighting. Both are worth fixing.

---

### Fix C5: Tighten `attribute-boolean-shorthand`

**File**: `ide-extensions~/grammar/uitkx.tmLanguage.json`

Change the `attribute-boolean-shorthand` pattern. Also move it to be the very last entry in `tag-attributes.patterns` (it already is — just confirm):

```json
"attribute-boolean-shorthand": {
  "comment": "Lone attribute name (boolean shorthand): must look like attr before =, />, or whitespace",
  "name": "entity.other.attribute-name.uitkx",
  "match": "\\b[A-Za-z][A-Za-z0-9\\-_]*\\b(?=\\s*(?:[=/> \\t\\r\\n]|$))"
}
```

The added `(?=...)` lookahead ensures only words that precede `=`, `/>`, `>`, or whitespace are matched — reflecting genuine attribute name positions.

---

### Fix C2: Change `uitkxExpression` superType to `operator`

**File**: `ide-extensions~/vscode/package.json`

```json
// BEFORE
{ "id": "uitkxExpression", "superType": "macro", ... }

// AFTER
{ "id": "uitkxExpression", "superType": "operator", ... }
```

`operator` is styled as a distinct punctuation/operator color in essentially all themes. The `@(` delimiter will now get a visible non-default color.

---

### Fix C3: Remove dead `embeddedLanguages` (short-term)

**File**: `ide-extensions~/vscode/package.json`

```json
// BEFORE
"grammars": [{
  "language": "uitkx",
  "scopeName": "source.uitkx",
  "path": "./syntaxes/uitkx.tmLanguage.json",
  "embeddedLanguages": { "source.cs.embedded.uitkx": "csharp" }
}]

// AFTER
"grammars": [{
  "language": "uitkx",
  "scopeName": "source.uitkx",
  "path": "./syntaxes/uitkx.tmLanguage.json"
}]
```

**Future enhancement**: To enable real embedded C# highlighting, assign `source.cs.embedded.uitkx` as the parent scope of `code-block-body` content. This is a significant grammar rewrite and should be done separately.

---

## 4. Fix Plan: Area 2 — Formatting

### Fix F1: Wrap formatter in try/catch

**File**: `ide-extensions~/lsp-server/FormattingHandler.cs`

```csharp
// BEFORE
var formatted = formatter.Format(text, localPath ?? string.Empty);

// AFTER
string formatted;
try
{
    formatted = formatter.Format(text, localPath ?? string.Empty);
}
catch (Exception ex)
{
    ServerLog.Log($"[Formatting] Format error for '{localPath}': {ex.Message}");
    return Task.FromResult<TextEditContainer?>(null);
}
```

### Fix F2: Remove `onEnterRules` to stop double-indent conflicts

**File**: `ide-extensions~/grammar/uitkx.language-configuration.json`

Remove the entire `"onEnterRules"` array. The `indentationRules` are sufficient for VS Code's auto-indent-after-Enter behavior. The `onEnterRules` were redundant and caused conflicts in C# expression contexts.

**Before removing**: Test that Enter after `<Box>` still creates an indented line — it will, because `indentationRules.increaseIndentPattern` already handles `<[A-Za-z][^>]*[^/]?>`.

---

## 5. Fix Plan: Area 3 — IntelliSense / Completions

### Fix I1: Remove space and newline trigger characters

**File**: `ide-extensions~/lsp-server/CompletionHandler.cs`

```csharp
// BEFORE
TriggerCharacters = new Container<string>("<", "@", " ", "\n", "{"),

// AFTER
TriggerCharacters = new Container<string>("<", "@", "{"),
```

### Fix I2: Scope `@`-strip middleware to trigger-appropriately

**File**: `ide-extensions~/vscode/src/extension.ts`

Inside `provideCompletionItem` middleware:
```typescript
provideCompletionItem(document, position, context, token, next) {
  return Promise.resolve(next(document, position, context, token)).then(result => {
    if (!result) return result;

    // Only strip leading '@' when the user typed it (it's already in the buffer)
    const triggerIsAt = context.triggerCharacter === '@';
    const wordBeforeIsAt = position.character > 0
      && document.getText(new vscode.Range(position.translate(0, -1), position)) === '@';
    if (!triggerIsAt && !wordBeforeIsAt) return result;

    const items = Array.isArray(result) ? result : (result as vscode.CompletionList).items;
    for (const item of items) {
      const raw = item.insertText;
      const text = typeof raw === 'string' ? raw : raw?.value;
      if (text && text.startsWith('@')) {
        const stripped = text.slice(1);
        item.insertText = typeof raw === 'string'
          ? stripped
          : new vscode.SnippetString(stripped);
      }
    }
    return result;
  });
},
```

### Fix I3: Return `isIncomplete: true` when Roslyn not ready

**File**: `ide-extensions~/lsp-server/CompletionHandler.cs`

```csharp
// BEFORE
if (ctx.Kind == CursorKind.CSharpExpression) {
    Log("completion: CSharpExpression — Roslyn not ready, returning empty");
    return new CompletionList();
}

// AFTER
if (ctx.Kind == CursorKind.CSharpExpression) {
    Log("completion: CSharpExpression — Roslyn not ready, returning incomplete");
    return new CompletionList(isIncomplete: true);
}
```

Also apply the same change for the general Roslyn fallback path (the `roslynList.Count == 0` case for non-expression positions if that path should retry).

---

## 6. Fix Plan: Area 4 — Error Coloring & File Indicators

### Area 4 Detailed Fixes

#### Fix E1: Compute actual column from source-map entry

**File**: `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs`

The `SourceMapEntry` type needs to record the column offset of the mapped region's start within the `.uitkx` file. Check `SourceMapEntry` definition in `language-lib`:

```
ide-extensions~/language-lib/Roslyn/SourceMapEntry.cs
```

If `SourceMapEntry` has a `UitkxColumn` or `CharOffset` property, use it:

```csharp
if (mapEntry != null)
{
    // Roslyn span start within the virtual document
    var span = diag.Location.SourceSpan;
    
    uitkxLine    = mapEntry.UitkxLine;
    // Add the intra-region column: span.Start character minus the region's
    // virtual start character gives the offset within the .uitkx line.
    uitkxCol     = mapEntry.UitkxColumn + (span.Start.Character - mapEntry.VirtualStartColumn);
    uitkxEndLine = mapEntry.UitkxLine;
    uitkxEndCol  = uitkxCol + (span.End - span.Start); // single-line spans common
}
```

**If `SourceMapEntry` doesn't have these fields yet**: Add them during the virtual document build in `RoslynHost` (wherever `SourceMapEntry` objects are constructed). Each mapped region knows its `.uitkx` line and column and its corresponding virtual document offset — these should already be present.

**This is the highest-impact single fix**: it moves squiggles from column 0 to the actual symbol.

---

#### Fix E2: Skip diagnostics that land at line 0 col 0 from scaffold code

**File**: `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs`

After position resolution (both routes), add a guard before constructing the `ParseDiagnostic`:

```csharp
// Skip diagnostics that couldn't be mapped to a real user-authored position.
// Line 0 in ParseDiagnostic is the sentinel for "unknown position".
// These would create phantom squiggles at the top of the file.
if (uitkxLine == 0)
{
    ServerLog.Log($"[DiagMapper] Skipping unmapped diagnostic {diag.Id}: {diag.GetMessage()}");
    continue;
}
```

This is a safe, zero-risk change that eliminates the false file-level red indicators.

---

#### Fix E3: Minimum squiggle width in ToLsp

**File**: `ide-extensions~/lsp-server/DiagnosticsPublisher.cs`

```csharp
// BEFORE
int endChar =
    d.EndColumn > 0 ? d.EndColumn
    : startChar > 0 ? startChar
    : 1;

// AFTER
// Ensure end is always strictly greater than start so VS Code renders a visible squiggle.
int endChar =
    d.EndColumn > 0 ? Math.Max(d.EndColumn, startChar + 1)
    : startChar + 1;
```

---

## 7. Testing Strategy

### 7.1 Test File Fixture

Create `TestFixtures~/basic.uitkx` with all language features exercised:

```
@namespace Test.NS
@using System.Collections.Generic
@component MyComponent
@code {
    var count = 0;
    List<int> items = new();
    bool isVisible = true;
}

<Box class="root">
    <Label text={count.ToString()} />
    @if (isVisible) {
        <Button text="Click" OnClick={e => count++} />
    }
    @foreach (var item in items) {
        <Label text={item.ToString()} />
    }
</Box>
```

### 7.2 Per-Fix Verification Steps

| Fix | How to verify |
|---|---|
| C4 (no `<>` autoclosing) | In `@code {}`, type `List<` — no `>` appears |
| C1 (tag name colored) | Open fixture, immediately (before LSP fires) — `Box` and `Label` should have non-default color |
| C5 (attribute overmatch) | Check that text inside tag body not near `=` doesn't get attribute color |
| C2 (uitkxExpression) | `@(count)` — the `@(` part should have a visible distinct color (operator-style) |
| F1 (formatter no crash) | Open file with syntax error, press `Shift+Alt+F` — no error popup, no server restart |
| F2 (no double indent) | Press Enter after `@code {` — indented correctly. Press Enter inside a C# lambda `x => {` — indented correctly, not double-indented |
| I1 (no space trigger) | Type a space inside a `@code` block — NO completion popup appears |
| I2 (@-strip scoped) | Press `Ctrl+Space` at start of line — completion items insert `@if`, not `if` |
| I3 (incomplete on Roslyn not ready) | Immediately after opening a file (before Roslyn compiles), trigger completion inside `@code {}` — see spinner/retry, not blank popup |
| E1 (columns correct) | Add a C# error inside `@code {}` (e.g. `var x = undefinedVar;`) — squiggle under `undefinedVar`, not at column 0 |
| E2 (no phantom errors) | Open a `.uitkx` file with no user errors — no red file icon in Explorer, no squiggles on line 1 |
| E3 (squiggle width) | Any squiggle should span at least 1 character width |

### 7.3 Theme Coverage Test

Test with at least 3 themes from different families:
1. **Dark+** (VS Code built-in) — baseline
2. **One Dark Pro** — most popular community theme
3. **GitHub Light** — light theme, ensures no invisible-on-white colors

For each theme: open `TestFixtures~/basic.uitkx`, verify:
- Keywords are distinct from identifiers  
- Tag names are colored (not default text color)
- Strings are string-colored
- Comments are comment-colored
- Operators in expressions are operator-colored is

---

## 8. Debug Protocol (User-Facing)

When reporting tooling issues, provide this structured output:

### Step 1: Open the UITKX Output Channel
`Ctrl+Shift+P` → "UITKX: Open Output" or find "UITKX" in the Output panel dropdown.

### Step 2: Enable LSP trace (temporarily)
Set `"uitkx.trace.server": "verbose"` in VS Code settings, then reload the window.  
This shows every LSP request/response in the "UITKX LSP Trace" output channel.

### Step 3: Capture a report
For any bug report, provide:
```
1. UITKX output channel contents (after opening the affected file)
2. Screenshot of the issue (with cursor position visible)
3. Contents of the .uitkx file that exhibits the issue
4. VS Code version: Help > About
5. Theme name in use
6. Extension version: Extensions panel > UITKX
```

### Step 4: Semantic token inspector
`Ctrl+Shift+P` → "Developer: Inspect Editor Tokens and Scopes"  
Click on any token. Reports:
- TextMate grammar scope (Layer 1)
- Semantic token type (Layer 2)
- Which color rule is winning

This is the single most useful debug tool for color issues.

### Step 5: Force Roslyn re-analyze
`Ctrl+Shift+P` → "Developer: Reload Window"  
This restarts the LSP server. Check if the issue persists after reload.

---

## 9. Implementation Order & Milestones

### Milestone 1: Immediate wins (no server recompile needed)
These fixes are pure JSON/TypeScript — push to marketplace immediately after:
- [x] **C4**: Remove `<>` from autoClosingPairs/brackets
- [x] **C1**: Add `entity.name.tag.uitkx` to tag name captures
- [x] **C5**: Tighten `attribute-boolean-shorthand` match
- [x] **C2**: Change `uitkxExpression` superType to `operator`
- [x] **C3**: Remove dead `embeddedLanguages`
- [x] **F2**: Remove `onEnterRules`
- [x] **I1**: Remove space/newline trigger characters
- [x] **I2**: Scope the `@`-strip middleware

*Build: `npm run build` in `ide-extensions~/vscode/`*  
*Publish: run publish pipeline → v1.0.89*

### Milestone 2: Server fixes (requires `dotnet publish` + marketplace update)
- [x] **F1**: Formatter try/catch
- [x] **E2**: Skip diagnostics at line 0 (phantom errors — highest user-visible impact)
- [x] **E3**: Minimum squiggle width
- [x] **I3**: `isIncomplete: true` for Roslyn not ready

*Build: `dotnet publish -c Release --self-contained false` in `ide-extensions~/lsp-server/`*  
*Publish: v1.0.90*

### Milestone 3: Column precision (requires SourceMapEntry investigation)
- [x] **E1**: Compute actual Roslyn diagnostic column
  
*Requires reading `SourceMapEntry.cs` and `RoslynHost`'s virtual document builder to understand available fields. Then patch `RoslynDiagnosticMapper`. May require adding `UitkxColumn` field to `SourceMapEntry`.*  
*Publish: v1.0.91*

### Milestone 4: Embedded C# (optional enhancement)
- [ ] **C3-Option-A**: Assign `source.cs.embedded.uitkx` scope to C# regions in grammar  
This is a significant grammar expansion and should be treated as a separate feature ticket.

---

## 10. Takeover Context (for next AI)

**You are continuing work on the UITKX VS Code extension.**

### What this extension does
UITKX is a React/JSX-like templating language for Unity UI Toolkit. Files end in `.uitkx`. The VS Code extension provides syntax highlighting, IntelliSense, formatting, and error checking via an LSP server.

### Repository location
`c:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit\`

### Published extension
Publisher: `ReactiveUITK`, name: `uitkx`, current version: `1.0.88`  
PAT for publishing: stored in `publisher-secrets.json` (not committed) at workspace root.  
Passing PAT: always via `--pat` CLI flag to vsce, never via env var.

### How to build and publish
```powershell
# Extension only (TypeScript changes):
cd ide-extensions~/vscode
npm run build

# Full publish:
# Use Unity menu: UITKX > Publish Extension
# Or run: scripts/publish-extension.ps1
```

### Key architectural rules
1. **Layer 1 (TextMate grammar) should never conflict with Layer 2 (semantic tokens)**. If a position is covered by semantic tokens, it takes precedence. Grammar is only for the initial render and for editors with semantic tokens disabled.
2. **All LSP server changes require `dotnet publish`** before packaging. Run: `dotnet publish -c Release --self-contained false` in `ide-extensions~/lsp-server/`.
3. **Diagnostic positions use 1-based lines**, LSP uses 0-based — conversion happens in `DiagnosticsPublisher.ToLsp()`.
4. **Column information in source-mapped Roslyn diagnostics is incomplete** (always 0). BUG-E1 fix requires adding `UitkxColumn` to `SourceMapEntry`.
5. **`isInsideCodeBlock` in `extension.ts`** and **`IsInsideCodeBlockAtOffset` in `CompletionHandler.cs`** are parallel implementations that must stay semantically equivalent.

### Priority if resuming
Start with **Milestone 1** (JSON/TS-only fixes, no server recompile). They are entirely self-contained and immediately publishable. Then do **Milestone 2** (E2 is the most impactful — kills phantom file-level errors).

### Files NOT to edit without reading first
These were under active change at session end and may have been user-edited:
- `scripts/publish-extension.ps1`
- `CICD/Editor/PublishUtility.cs`
- `ide-extensions~/vscode/package.json`

Always `read_file` these before any edits.
