# TextMate Grammar Audit — `uitkx.tmLanguage.json`

**Audit date:** 2026-04-05
**Files audited:**
- `ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json` (the grammar)
- `ide-extensions~/grammar/uitkx.tmLanguage.json` (copy under grammar/)
- `ide-extensions~/language-lib/Parser/UitkxParser.cs` (parser)
- `ide-extensions~/language-lib/Parser/MarkupTokenizer.cs` (lexer)
- `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs` (layer 2 tokens)
- `ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs` (layer 3 tokens)
- `ide-extensions~/grammar/uitkx-schema.json` (element/attribute schema)

---

## Architecture Context

UITKX uses a three-layer coloring system:

1. **TextMate grammar** (instant, regex-based) — the subject of this audit
2. **SemanticTokensProvider** (LSP, ~200ms) — AST-aware element/directive tokens
3. **RoslynSemanticTokensProvider** (LSP, ~300ms) — Roslyn C# classification

Layer 2 and 3 tokens override layer 1 at the same position. Layer 1 is
critical for the **initial render** before LSP connects and for any constructs
the LSP providers don't cover.

---

## SECTION 1 — Critical Issues

### 1.1 Fragment syntax `<>...</>` not highlighted (CRITICAL)

**Problem:** `tag-self-closing` and `tag-open` both begin with:
```regex
(<)([A-Za-z][A-Za-z0-9]*)
```
This requires at least one letter after `<`. The fragment `<>` won't match.

`tag-close` uses:
```regex
(</)(\\w+)(>)
```
This requires at least one word char. The fragment close `</>` won't match.

**Result:** Fragment syntax gets zero highlighting — no tag punctuation coloring,
no bracket matching, nothing. This is the only element type that's completely
invisible to the grammar.

**Parser support:** `ParseElement()` handles `<>` at L704-721 by checking
`TryConsume('>')` immediately after `<` (no tag name).

**SemanticTokensProvider:** Handles fragments via `ElementNode("")` — but only
after LSP loads. Until then, fragments are plain text.

**Fix approach:** Add dedicated fragment rules before the named-tag rules:

```json
"fragment-open": {
  "name": "meta.tag.fragment.open.uitkx",
  "match": "(<)(>)",
  "captures": {
    "1": { "name": "punctuation.definition.tag.begin.uitkx" },
    "2": { "name": "punctuation.definition.tag.end.uitkx" }
  }
},
"fragment-close": {
  "name": "meta.tag.fragment.close.uitkx",
  "match": "(</)(\\/?>)",
  "captures": {
    "1": { "name": "punctuation.definition.tag.begin.uitkx" },
    "2": { "name": "punctuation.definition.tag.end.uitkx" }
  }
}
```

These must appear **before** `tag-self-closing`/`tag-open`/`tag-close` in the
patterns array so `<>` is consumed before the named-tag rules try to match.

### 1.2 Nested generics break function-call highlighting (CRITICAL)

**Problem:** The `function-call` pattern uses:
```regex
(?=\\s*(?:<[^>]*>)?\\s*\\()
```

The `<[^>]*>` stops at the **first** `>`, so `Render<List<int>>()` matches
`<List<int` then fails because the `>` closes prematurely.

**Result:** Any function call with nested generic type arguments loses its
function-name highlighting. E.g.:
- `CreateComponent<MyProps<T>>()` — `CreateComponent` not colored as function
- `UseReducer<State, Action>()` — works (no nesting), but
  `UseReducer<Dictionary<string, int>, Action>()` — fails

**Fix approach:** Support one level of nesting:
```regex
(?=\\s*(?:<[^<>]*(?:<[^<>]*>[^<>]*)*>)?\\s*\\()
```
Or accept that deeply nested generics are a Roslyn-layer concern and
document the limitation.

---

## SECTION 2 — High Priority Issues

### 2.1 Missing verbatim string `@"..."` pattern

**Problem:** No rule matches the `@` prefix of `@"escape-free strings"`.

**What happens:** The `@` falls through all patterns (not `@(`, not `@if`, not
`@uss`, not a directive). It becomes unstyled plain text. The `"..."` portion
IS matched by `double-quoted-string`, so the string content gets colored but
the leading `@` is wrong-colored or unstyled.

**Worse:** Inside a verbatim string, `""` is the escape for a literal quote.
The grammar's `double-quoted-string` uses `end: "\""` which will terminate
at the first `"` — so `@"path with ""quotes"" inside"` closes at the first
`""`, leaving the rest mis-highlighted.

**Fix approach:** Add before `double-quoted-string` in `expression-content`:
```json
"verbatim-string": {
  "name": "string.quoted.double.uitkx",
  "begin": "@\"",
  "end": "\"(?!\")",
  "patterns": [
    { "name": "constant.character.escape.uitkx", "match": "\"\"" }
  ]
}
```

### 2.2 Missing verbatim interpolated string `$@"..."` / `@$"..."`

**Problem:** C# allows `$@"..."` and `@$"..."` for interpolated verbatim strings.
The grammar only handles `$"..."`. The verbatim+interpolated combo is not matched.

**Fix approach:** Add before `interpolated-string`:
```json
"verbatim-interpolated-string": {
  "begin": "(?:\\$@|@\\$)\"",
  "end": "\"(?!\")",
  ...same as interpolated-string but with "" escape instead of \\.
}
```

### 2.3 Missing char literal `'x'`

**Problem:** No pattern handles single-quoted char literals. `'A'`, `'\n'`,
`'\u0041'` are all treated as plain text/operators.

**Result:** In expressions like `e.keyCode == 'A'`, the `'A'` is unstyled.

**Fix approach:**
```json
"char-literal": {
  "name": "string.quoted.single.uitkx",
  "match": "'(?:\\\\.|[^'\\\\])'"
}
```

### 2.4 Missing raw string literal `"""..."""`

**Problem:** C# 11 raw strings use `"""` delimiters. The `double-quoted-string`
has `begin: "\"(?!\"\")"` — the negative lookahead correctly avoids matching
the first `"` of `"""`, but no separate rule handles the triple-quote pattern.

**Result:** Raw strings get zero highlighting. They appear as plain text with
embedded quote characters potentially breaking other rules.

**Fix approach:**
```json
"raw-string-literal": {
  "name": "string.quoted.double.uitkx",
  "begin": "\"\"\"",
  "end": "\"\"\""
}
```
Note: must appear before `double-quoted-string` in the pattern order.

---

## SECTION 3 — Medium Priority Issues

### 3.1 Missing null-conditional operators `?.` and `?[`

**Problem:** The `cs-operator` regex:
```regex
(\\+\\+|--|\\?\\?|&&|\\|\\||[+\\-*/%&|^~!<>=]=?|\\?)
```
has `\\?\\?` for null-coalescing and `\\?` for ternary, but not `\\?\\.` for
null-conditional member access or `\\?\\[` for null-conditional indexer.

**Result:** In `list?.Count` or `dict?["key"]`, the `?.` and `?[` are partially
matched (the `?` matches as ternary operator, then `.` / `[` are unstyled).

**Fix:** Add `\\?\\.` and `\\?\\[` to the operator alternation.

### 3.2 Missing null-coalescing assignment `??=`

**Problem:** Only `??` is in the operator regex. `??=` is matched as `??` + `=`
(two separate operator tokens). Not wrong per se, but inconsistent with how
`+=`, `-=`, etc. are handled.

**Fix:** Add `\\?\\?=` before `\\?\\?` in the alternation.

### 3.3 Missing `when` keyword (pattern matching)

**Problem:** In `@switch` bodies, C# pattern matching allows:
```csharp
@case > 0 when x < 100:
```
The `when` keyword is not in the `cs-keywords` list. It gets highlighted as
a variable (`variable.other.uitkx`).

**Fix:** Add `when` to the keywords list.

### 3.4 Missing range operator `..`

**Problem:** The range operator `..` (e.g., `items[1..^1]`) is not in the
operator regex. Appears as two dots (plain text).

**Fix:** Add `\\.\\.` to the operator alternation (before single `.` if one
is ever added).

### 3.5 attribute-boolean-shorthand over-matching

**Problem:** The pattern `\\b[A-Za-z][A-Za-z0-9\\-_]*\\b` has no negative
lookahead for `=`. It matches **any** word in attribute position, including
attribute names that are about to be assigned a value.

**Effect:** When `attribute-key` also fires on the same token, there shouldn't
be a conflict (both produce `entity.other.attribute-name`). But if ordering
is ever changed, the boolean shorthand could incorrectly classify a keyed
attribute.

**Fix:** Add negative lookahead: `(?!\\s*=)` after the word boundary.

### 3.6 cs-type-name matches too broadly

**Problem:** `\\b[A-Z][A-Za-z0-9]*\\b` matches every PascalCase word. This
includes C# keywords already in `cs-keywords` (e.g., `String`, `Int32`) and
element tag names (which are already scoped by tag rules).

**Effect:** Inside expressions, PascalCase words that aren't types get
`entity.name.type.uitkx` color. This is mostly correct (PascalCase = type
in C# convention), but overly aggressive. Roslyn layer 3 corrects this
when it loads.

**Low urgency**: This is by-design for the gap before semantic tokens load.
No fix needed unless specific false positives are reported.

---

## SECTION 4 — Low Priority Issues

### 4.1 All C# keywords use `keyword.control.uitkx`

**Problem:** TextMate convention distinguishes:
- `keyword.control` — flow control: `if`, `else`, `for`, `while`, `return`,
  `break`, `continue`, `throw`
- `keyword.other` — other: `var`, `new`, `typeof`, `nameof`, `class`, `struct`
- `keyword.operator` — keyword operators: `is`, `as`, `in`
- `constant.language` — literals: `true`, `false`, `null`

Current grammar lumps all 50+ keywords into `keyword.control.uitkx`.

**Effect:** Most themes color all keyword subcategories the same, so the visual
impact is minimal. Some advanced themes (e.g., One Dark Pro, Catppuccin)
distinguish `constant.language` from `keyword.control`.

**Fix (cosmetic):** Split into proper subcategories. Low priority since Layer 3
(Roslyn) overrides most of these.

### 4.2 Missing `sizeof` keyword

**Problem:** `sizeof` is not in the `cs-keywords` list. It's treated as a
function call (the `function-call` pattern matches `sizeof(`).

**Effect:** `sizeof(int)` highlights `sizeof` as function instead of keyword.
Minor — most themes color both purple/blue.

### 4.3 Square brackets `[...]` not highlighted

**Problem:** Array indexers `arr[0]` and attributes `[Attribute]` have no
dedicated pattern. The `[` and `]` are unstyled punctuation.

**Effect:** Purely cosmetic. Most themes don't color brackets anyway.

### 4.4 Index-from-end `^` and range `..` operators

**Problem:** `^1` (index from end) uses `^` which IS in the operator regex
(as XOR), so it gets operator color — accidentally correct. The `..` range
operator is not matched (see 3.4).

### 4.5 `@props` and `@key` directives may be overly permissive

**Problem:** These are in `directive-declaration` but may not be relevant for
function-style components (which use parameter syntax instead). Not a
highlighting error — just overly broad matching.

**No fix needed:** The directives ARE valid for directive-style (non-function)
components and the grammar shouldn't restrict based on component style.

---

## SECTION 5 — Dead Code & Stale Patterns

### 5.1 `s_codeBodyTokenRegex` in SemanticTokensProvider is dead code

**Problem:** The regex `s_codeBodyTokenRegex` (L565-575) is defined but never
invoked anywhere in the `SemanticTokensProvider`. It was likely used for
`@code` block tokenization before `@code` was deprecated.

**Location:** `SemanticTokensProvider.cs` L565-575

**Fix:** Remove the dead regex field.

### 5.2 Repo memory file outdated

**Problem:** `/memories/repo/uitkx-coloring-architecture.md` references:
- `CodeBlockNode` → `@code` as `uitkxDirective` — **no longer exists**
- `BreakNode`/`ContinueNode` → keyword as `uitkxDirective` — **no longer exist**
- `code-block` / `code-block-newline-brace` patterns — **removed from grammar**
- "Unreachable tracking: After `@break`/`@continue`" — **not implemented**
- `CollectEmbeddedMarkupTokensInCodeBlock` — **likely dead code**

**Fix:** Update the repo memory file to reflect current architecture.

---

## SECTION 6 — Semantic Token Asymmetries

### 6.1 HTML comments — TM only

`<!-- -->` is colored by TM grammar (`comment.block.uitkx`) but not by the
SemanticTokensProvider (which doesn't have an HtmlCommentNode). This works
correctly — TM handles it, no override needed.

### 6.2 Hook setters — LSP only

`setState` and similar hook setter names are colored as functions by a special
post-pass in SemanticTokensProvider (`CollectHookSetterTokens`). The TM grammar
has no awareness of hooks — it just sees `setState` as a regular function call
(which the `function-call` pattern handles if followed by `(`).

No gap — both layers produce the same visual result for `setState(...)`.

### 6.3 C# keywords in setup code — TM fallback is important

Inside control block bodies (`{ setup code; return (...); }`), the TM grammar's
`code-block-body` pattern provides C# keyword/identifier coloring. The LSP
provider does NOT colorize individual C# tokens in setup code — it relies on
Roslyn (layer 3) for that.

**Gap:** If Roslyn is slow or offline, setup code C# tokens rely entirely on
the TM grammar's `expression-content` patterns. This makes the TM grammar's
C# coverage important for the user experience during LSP startup.

---

## SECTION 7 — Cross-Platform Grammar Sync

### 7.1 Two copies of the grammar

The grammar exists in two locations:
- `ide-extensions~/vscode/syntaxes/uitkx.tmLanguage.json` — VSCode extension
- `ide-extensions~/grammar/uitkx.tmLanguage.json` — shared reference

These must be kept in sync. Any fix applied to one must be applied to the other.
Consider:
- Making one a symlink to the other
- Or adding a build step that copies grammar/ → vscode/syntaxes/

### 7.2 Rider and VS don't use the TextMate grammar

Both Rider and Visual Studio rely on LSP semantic tokens, not TextMate grammar.
Changes to the TM grammar only affect VS Code (and potentially other TextMate-
compatible editors like Sublime Text, Zed, etc.).

---

## SECTION 8 — Priority Matrix

### P0 — Critical (Broken highlighting)

| # | Issue | Section |
|---|-------|---------|
| 1 | Fragment `<>...</>` gets zero highlighting | 1.1 |
| 2 | Nested generics break function-call regex | 1.2 |

### P1 — High (Missing C# string types)

| # | Issue | Section |
|---|-------|---------|
| 3 | Verbatim string `@"..."` mis-highlighted | 2.1 |
| 4 | Verbatim interpolated `$@"..."` / `@$"..."` not handled | 2.2 |
| 5 | Char literal `'x'` unstyled | 2.3 |
| 6 | Raw string literal `"""..."""` unstyled | 2.4 |

### P2 — Medium (Missing operators & patterns)

| # | Issue | Section |
|---|-------|---------|
| 7 | Null-conditional `?.` / `?[` | 3.1 |
| 8 | Null-coalescing assignment `??=` | 3.2 |
| 9 | `when` keyword missing | 3.3 |
| 10 | Range operator `..` missing | 3.4 |
| 11 | Boolean shorthand over-match | 3.5 |

### P3 — Low (Cosmetic / cleanup)

| # | Issue | Section |
|---|-------|---------|
| 12 | Keyword subcategory scopes | 4.1 |
| 13 | `sizeof` as function not keyword | 4.2 |
| 14 | Dead `s_codeBodyTokenRegex` | 5.1 |
| 15 | Outdated repo memory | 5.2 |
| 16 | Two grammar copies need sync | 7.1 |
