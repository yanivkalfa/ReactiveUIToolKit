# Changelog

## [1.0.80]
- **Fix: restore C# coloring in `@code` body lines (e.g. `var`, `useState`, tuples).**
  Re-added `expression-content` fallback inside `@code` grammar body so pure C#
  lines are tokenized again while embedded markup patterns remain available.

## [1.0.79]
- **Revert: roll back 1.0.77 editor-default color overrides.**
  Removed UITKX defaults for `editor.guides.bracketPairs` and
  `editor.guides.highlightActiveBracketPair` so behavior returns to the
  1.0.76 baseline. Kept only `editor.bracketPairColorization.enabled: false`.

## [1.0.78]
- **Hotfix: restore UITKX semantic token colors.**
  Reverted the UITKX default that disabled semantic highlighting, which caused
  tag/directive semantic colors to disappear. Bracket-pair colorization controls
  remain in place.

## [1.0.77]
- **Fix: consistent punctuation coloring mode for UITKX (`{}` / `()` / brackets).**
  Added UITKX language defaults to disable bracket-pair colorization overlays,
  bracket-pair guides, and semantic highlighting overlays for UITKX documents.
  This forces one grammar-driven color model so identical syntax colors the same
  across all sections/contexts.

## [1.0.76]
- **Fix: enforce one consistent color model across all markup contexts.**
  Disabled context-dependent C# semantic overlays from UITKX semantic token output
  (including hook/body C# token passes), keeping semantic coloring focused on
  UITKX markup/directive primitives so identical markup patterns color the same
  anywhere in the file.

## [1.0.75]
- **Fix: structural embedded-markup color consistency inside `@code`.**
  Semantic tokens now scan and parse embedded markup directly from `@code` source
  spans (not only parser-captured forms), emit normal markup tokens for those
  elements/attributes, and exclude those spans from C# semantic tokenization.
  This makes markup coloring significantly more consistent across contexts.

## [1.0.74]
- **Fix: unify embedded-markup punctuation colors inside `@code`.**
  Removed the global `expression-content` fallback from `@code` body grammar,
  so embedded markup uses the same scope rules as top-level markup (reducing
  `{}` / `()` / mixed-token color drift across contexts).

## [1.0.73]
- **Fix: attribute completion inserts canonical library property names.**
  Completion now normalizes dynamic workspace props to schema/library casing,
  so selecting `text` inserts `text=...` (not `Text=...`).
- **Fix: more consistent embedded-markup coloring inside `@code`.**
  Added control-flow grammar scopes inside `@code` and suppressed C# semantic
  tokenization on markup-closer lines within embedded markup runs.

## [1.0.72]
- **Fix: attribute completion in same-line nested tags.**
  Completion now prefers the local tag under the cursor, so `<Box><Label .../></Box>`
  suggests `Label` attributes (e.g. `text`) correctly.
- **Fix: better embedded-markup coloring inside `@code`.**
  C# semantic tokenization now skips markup-like lines in `@code` blocks so
  embedded UITKX markup coloring stays consistent with normal markup.

## [1.0.71]
- **Fix: restore `@code` suggestion at header boundary.**
  Header detection now includes the markup-start boundary line again, while
  keeping the non-header `@code` leak prevention in place.

## [1.0.70]
- **Fix: hard-stop `@code` outside header context.**
  Added a final completion-result safety filter so `@code` is removed whenever
  the cursor is not in the strict header zone.

## [1.0.69]
- **Hotfix: rebuild with updated completion server binary.**
  Ensures the post-header `@code` completion routing fix is compiled into the
  bundled LSP server payload.

## [1.0.68]
- **Fix: `@code` no longer leaks into post-header directive completions.**
  Completion routing now treats non-header `@` contexts as control-flow/markup
  contexts, and header-only `@code` suggestions stop once you are past the
  header/`@code` boundary.

## [1.0.67]
- **Fix: completion works again for embedded markup inside `@code`.**
  Tag and markup directive suggestions are now enabled in likely embedded-markup
  regions inside `@code` blocks, while pure C# lines remain directive-filtered.

## [1.0.66]
- **Fix: header completion now suggests only `@code`.**
  In the header zone (before markup), directive completion is now intentionally
  constrained to `@code` only, matching the current authoring workflow.

## [1.0.65]
- **Fix: `@code` appears again in the header/directive completion zone.**
  The directive/markup boundary completion routing now treats the markup-start line
  as header-capable for directive suggestions, so `@code` is suggested where expected.
- **Fix: directive suggestions stay context-bound.**
  Header zones keep top-level directives while markup/code contexts keep their own
  filtered suggestion sets.

## [1.0.39]
- **Fix: `useState` / hook hover now works inside `@code` blocks.** `ClassifyTagPosition`
  was returning `CursorContext.Empty` (word stripped) when the cursor was on plain
  code text with no `<` tag on the same line. It now returns the word so hover,
  go-to-definition, and hook docs all trigger correctly.
- **Fix: formatter no longer erases `{/* */}` JSX comments on save.** The formatter
  now detects `{/*` in the source and skips formatting entirely for that document,
  preserving all comments.
- **Fix: `{/*` auto-closes correctly.** `{/*` → `*/}` is now listed before `{` → `}`
  in `autoClosingPairs`, so VS Code picks the longer match before the single-char one.
- **Fix: Unity Console click now actually opens VS Code on Windows.** `code` is a
  `.cmd` script on Windows; the handler now uses `cmd.exe /c code --goto ...` so
  it launches correctly.
- **Fix: `useState` shorthand hook errors.** The SourceGenerator DLL in `Analyzers/`
  has been rebuilt with the `ApplyHookAliases` substitution, so `useState(0)` in
  `@code` blocks now correctly becomes `Hooks.UseState(0)` in generated C#.

## [1.0.38]
- **Feature 8.6.1: Unity Console click opens `.uitkx` in VS Code**.
  A new `[OnOpenAsset]` handler in `Editor/UitkxConsoleNavigation.cs` intercepts
  opens of `.uitkx` assets and redirects them to `code --goto path:line`, so
  clicking a Unity Console hyperlink (backed by the `#line` directives already
  emitted by the source generator) opens the exact `.uitkx` line in VS Code.
- **Feature 8.6.2a: React-style hook shorthands** — `useState(0)`, `useEffect(...)`,
  `useMemo(...)` etc. are now valid in `@code` blocks. The emitter normalises them
  to `Hooks.UseState(0)` etc. before generating C#. The rules-of-hooks validator
  (UITKX0015/0016/0018) also fires on the camelCase shorthand forms.
- **Feature 8.6.2b: Hook setter coloring** — in `var (count, setCount) = useState(0)`,
  the setter variable `setCount` is now colored with the attribute/property theme
  color, making it visually distinct from the state variable.
- **Feature 8.6.2c: Hook hover documentation** — hovering `useState`, `useEffect`,
  `Hooks.UseState` etc. shows a markdown tooltip with the hook's signature and
  a one-line description. Works for all 10 built-in hooks (both shorthand and
  fully-qualified forms).
- **Feature 8.6.4: JSX-style markup comments `{/* */}` + Ctrl+/ support**.
  - `{/* any text */}` is now recognized in markup, colored as a comment, and
    completely ignored by the parser and emitter.
  - Multi-line comments are supported.
  - `Ctrl+/` inserts `//` (ideal inside `@code` blocks).
  - `Ctrl+Shift+/` wraps the selection in `{/* */}` (ideal in markup).
  - Typing `{/*` auto-inserts `*/}`.
  - `<!-- -->` HTML comments continue to work as before.

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
