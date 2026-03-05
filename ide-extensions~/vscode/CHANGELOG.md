# Changelog

## [1.0.37]
- **Fix: missing `>` diagnostic squiggle now sits on `<TagName`, not column 0**.
  The error now carries the exact source range of the opening token so the squiggle
  underlines `<Box` (or whatever the tag name is) rather than the first 1-2 characters
  of the line. Uses a dedicated `UITKX0303` ("missing tag close") diagnostic code
  distinct from the generic unexpected-token code.

## [1.0.36]
- **Fix: deleting `>` from an opening tag no longer corrupts children's syntax colors**.
  Root cause was in `UitkxParser.ParseAttributes()`. When `>` was missing, the attribute
  scanner had no exit condition for `@` or `<`, so it consumed the entire rest of the file
  as attribute content — reading `@switch`, `(mode)`, `{...}` switch body, child elements,
  all as bogus attribute tokens. The resulting mangled AST produced wrong semantic tokens
  and wrong diagnostics for the whole file.

  Fix: `ParseAttributes()` now breaks immediately when it encounters `@` or `<` at the
  top of the attribute loop. Both are unambiguous: `@` always signals a control-flow
  directive and `<` always signals a child element — neither can appear as a valid
  attribute start at this level (expressions are consumed atomically by `ReadBraceExpression`
  and string values by `ReadStringLiteral` before the outer loop sees them).

  Secondary: `attribute-value-expression` in the TM grammar now requires `=` directly
  before `{` (was bare `{`), preventing the same class of grammar-level consume-all
  bug as a resilience measure for incremental TM rendering between keystrokes.

## [1.0.33]
- **Fix blue+red squiggle race condition**: After pushing a new diagnostic,
  the server now immediately sends `workspace/semanticTokens/refresh` to VS Code.
  This forces VS Code to re-fetch semantic tokens in sync with the diagnostic
  update, eliminating the ~150ms window where stale teal coloring from a valid
  element could coexist with the new red error squiggle from an unknown element.

## [1.0.32]
- **Fix unknown-element blue text color**: Removed `entity.name.tag.uitkx` from
  the TM grammar's tag-name captures.  Now that semantic tokens own tag-name
  styling exclusively, unknown elements (whose semantic token is suppressed by
  the v1.0.31 logic) fall through to the editor's default foreground color
  rather than the TM grammar's blue `entity.name.tag` mapping.  Known elements
  remain teal via their semantic token; unknown elements show plain default text
  with only the red UITKX0105 squiggle as their indicator.

## [1.0.31]
- **Fix unknown-element double squiggle**: Unknown PascalCase elements (e.g. a
  mistyped component name) previously showed both a red diagnostic squiggle **and**
  a second teal/blue underline from the semantic token.  Now the semantic token is
  suppressed for PascalCase names that are not present in the workspace index, so
  only the red error squiggle appears.  Built-in lowercase elements and known
  components are unaffected.  When the index has not yet finished scanning the file
  is highlighted as before (safe fallback).  
  Implementation: `SemanticTokensProvider.GetTokens` accepts an optional
  `HashSet<string>? knownElements`; `SemanticTokensHandler` passes
  `_index.IsReady ? _index.KnownElements : null`.  
  Also changed `uitkxElement` `superType` from `"type"` → `"class"` in
  `package.json` to avoid additional theme-driven underlines on the type token.

## [1.0.30]
- **Fix close-tag color mismatch**: Closing tag names (e.g. `</Box>`) now receive
  the same semantic token as the opening tag, so both render identically (no more
  teal open / blue close inconsistency).  
  Implementation: `ElementNode` now stores `CloseTagLine`; the parser captures it
  before consuming `</TagName>`; `SemanticTokensProvider` emits the token via the
  new `FindCloseTagName` helper (mirrors the existing `FindOpenTagName` pattern).

## [1.0.29]
- **Fix blue element color**: Declare `semanticTokenTypes` in `package.json` so VS Code correctly resolves
  `uitkxElement`, `uitkxDirective`, `uitkxAttribute`, `uitkxExpression`, and `uitkxDirectiveName` token
  types with proper theme inheritance (`type`, `keyword`, `property`, `macro`, `namespace` supertypes).
  Without this declaration VS Code ignored the semantic tokens entirely, letting the TextMate grammar's
  `entity.name.tag` (blue in Dark+) win over the intended teal/type color.
- Remove unused `s_builtIns` set and `s_defaultLibraryMods` field from `SemanticTokensProvider`
  (leftover from v1.0.27 `defaultLibrary` modifier experiment).
- Remove `SemanticTokenModifiers.DefaultLibrary` constant; `declaration` modifier retained for future
  known-component highlighting.


- Add Marketplace icon (`images/logo.png`) and wire manifest `icon` field so extension branding renders correctly.

## [1.0.11]
- Re-publish VS Code extension after IntelliSense/LSP stabilization so Marketplace package reflects current fixed behavior.

## [1.0.10]
- Fix formatter indentation for directive blocks nested under tags (`@if/@for/@case/@code`) and align closing `}` with opening directive level.
- Add `@case` expression tokenization before `{` and classify switch-arm labels (`count`, `0`, `1`, `_`) plus `=>` operator.

## [1.0.9]
- Fix control-flow grammar for `@if/@for/@foreach/@while/@switch` so variables and operators inside parentheses are tokenized/classified.
- Fix brace-based indentation for directive blocks so `@if (...) {` and `@for (...) {` indent correctly on Enter.

## [1.0.8]
- Fix onEnterRules: use plain string regex and correct string indentAction enum values

## [1.0.7]
- Fix onEnterRules: use integer indentAction values and { pattern } regex form; add editor.autoIndent=full default

## [1.0.6]
- Replace indentationRules with explicit onEnterRules for reliable Enter-key auto-indent after open tags

## [1.0.5]
- Fix on-Enter auto-indent: remove `<>` from bracket pairs (was confusing VS Code indent engine), improve increaseIndentPattern

## [1.0.4]
- Added rollForward LatestMajor to runtimeconfig so the server runs on .NET 8, 9, or 10+

## [1.0.3]
- Bundle all dependencies with esbuild so `vscode-languageclient` is available at runtime (fixes missing completions/hover after install)

## [1.0.2]
- Added explicit `activationEvents` to fix extension not activating when installed from Marketplace

## [1.0.1]

### Changed
- LSP server now targets .NET 8 (was .NET 10) — compatible with .NET 8, 9, and 10 runtimes

## [1.0.0]

### Added
- Syntax highlighting for `.uitkx` files (directives, control flow, element tags, attributes, embedded C#)
- Tag completions triggered by `<`
- Attribute completions for all built-in elements
- Directive and control-flow completions triggered by `@`
- Hover documentation (element descriptions, attribute types)
- Language configuration: bracket matching, auto-close, folding
- Bundled LSP server (`UitkxLanguageServer.dll`)
