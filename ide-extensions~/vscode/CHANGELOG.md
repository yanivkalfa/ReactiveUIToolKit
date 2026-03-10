# Changelog

## [1.0.118] - 2026-03-10
- Fix diagnostic squiggle positions: attribute names now underline the name (not col 0), unclosed-tag errors land on the tag opener; suppress CS0246 false-positives from T3 Roslyn (Unity-only assembly types); workspace index now discovers function-style components from .uitkx files (fixes unknown-element warnings for peer components)

## [1.0.117] - 2026-03-10
- fix: UITKX0105 now fires for unknown elements (schema+workspace index wired to analyzer)
- fix: UITKX2101 squiggle aims at component name column instead of col 0
- fix: CS0219 suppressed (cascade false positive from broken parse)
- fix: CS0246 no longer globally suppressed — user-authored type errors now show correctly
- feat: UITKX0109 (warning) — unknown attribute on a known element

## [1.0.115] - 2026-03-09
- fix: UITKX0103 squiggle on component NAME column; unknown directive is error not warning

## [1.0.114] - 2026-03-09
- fix: validate UITKX markup inside setup-code JSX blocks; fix UITKX0103 squiggle on component name line

## [1.0.114] - 2026-03-09
- fix: validate UITKX markup inside setup-code JSX blocks; fix UITKX0103 squiggle on component name line

## [1.0.113] - 2026-03-09
- fix: validate UITKX markup inside setup-code JSX blocks; fix UITKX0103 squiggle on component name line

## [1.0.113] - 2026-03-09
- fix: correct column tracking for @else/@case/@default squiggles and unknown directive errors

## [1.0.113] - 2026-03-09
- powershell -ExecutionPolicy Bypass -File "c:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit\scripts\publish-extension.ps1" -BumpVersion 2>&1 | Select-Object -Last 30

## [1.0.112] - 2026-03-09
- Fix Roslyn diagnostic squiggle position: use exact trimmed-code start offset so errors land on the correct token

## [1.0.111] - 2026-03-09
- Fix squiggle positions: wire SourceColumn/EndColumn for all control-flow nodes (@if/@for/@foreach/@while/@switch/@break/@continue) and @(expr)

## [1.0.110] - 2026-03-09
- Revert grammar color regression - restore original tag-name and attribute-boolean-shorthand tokenization

## [1.0.109] - 2026-03-09
- Revert grammar color regression - restore original tag-name and attribute-boolean-shorthand tokenization

## [1.0.109] - 2026-03-09
- Add UITKX0108 diagnostic - error on multiple render roots at component level

## [1.0.109] - 2026-03-09
- Add UITKX0108 diagnostic: multiple render roots error

## [1.0.109] - 2026-03-09
- Skip block-body lambdas (CS1977 unsuppressable), add provideContext stub

## [1.0.108] - 2026-03-09
- Fix conditional JSX return (void->object render method), add useContext and useLayoutEffect stubs

## [1.0.107] - 2026-03-09
- Fix conditional JSX return (void->object render method), add useContext and useLayoutEffect stubs

## [1.0.106] - 2026-03-09
- Keep CS0428 method-group warning visible (orange = warning, not an error)

## [1.0.105] - 2026-03-09
- Fix useRef returning object (now Ref<T>), zero-arg lambdas cast to Action, method-group suppress CS0428

## [1.0.104] - 2026-03-09
- Fix 'object does not contain newValue': cast attribute lambdas to Action<dynamic> so e.newValue and similar event property accesses compile

## [1.0.103] - 2026-03-09
- Fix Color ambiguity: add using Color = UnityEngine.Color alias to virtual doc usings (same fix as CSharpEmitter)

## [1.0.102] - 2026-03-09
- Fix UnityEngine.UIElementsModule CS0433/CS0012: skip monolithic UnityEngine.dll forwarder from Managed/, use split module DLLs instead

## [1.0.101] - 2026-03-09
- Fix UnityEngine module DLL ambiguity (two-pass Managed scan); fix AnimationsDemoPage stray semicolons (PowerShell write)

## [1.0.100] - 2026-03-09
- Fix useState setter stub: use delegate T(T prev) covering both setX(value) and setX(prev=>{}) patterns; fix Managed DLL scan skipping UnityEngine module DLLs when monolithic UnityEngine.dll already present

## [1.0.99] - 2026-03-09
- Fix useState setter type (use Hooks.StateSetter<T>) so setX(prev => {...}) functional updates compile; fix reference locator walk-up and recursive managed dir scan for UnityEngine.UIElementsModule

## [1.0.98] - 2026-03-09
- Fix @for/@foreach/@while loop variables not in scope for attribute expressions inside body

## [1.0.97] - 2026-03-09
- Fix stray semicolons in AnimationsDemoPage useMemo dep args causing CS1026/CS1513 Unity compile errors

## [1.0.97] - 2026-03-09
- Fix stray semicolons in AnimationsDemoPage useMemo dep args causing CS1026/CS1513 Unity compile errors

## [1.0.96] - 2026-03-09
- Cast lambda attribute expressions to Action<object> instead of object to fix delegate inference errors

## [1.0.95] - 2026-03-09
- Suppress CS1026/CS1513/CS0411 virtual-doc noise; bump to v1.0.95

## [1.0.94] - 2026-03-09
- Suppress CS1026/CS1513 cascade errors and CS0411 lambda noise in virtual doc diagnostics (RoslynHost explicit filter + pragma)

## [1.0.94] - 2026-03-09
- Fix stray semicolons in AnimationsDemoPage useMemo dep args (CS1026/CS1513)

## [1.0.94] - 2026-03-09
- fix IsSuppressed filter

## [1.0.94] - 2026-03-09
- no-op reinstall

## [1.0.94] - 2026-03-09
- Fix CS0104 FlexDirection ambiguity, CS0411 lambda inference, skip setup JSX attribute collection

## [1.0.94] - 2026-03-09
- Fix CS0104 FlexDirection ambiguity, CS0411 lambda inference, skip setup JSX attribute collection

## [1.0.93] - 2026-03-09
- Fix (repackage): ensure rebuilt DLL is bundled — usings/hook stubs now correctly compiled into server

## [1.0.92] - 2026-03-09
- Fix: add Animation/Props.Typed namespaces + StyleKeys static using + UColor alias to virtual doc; fix CS0815 on JSX variable assignments

## [1.0.91] - 2026-03-09
- Fix: useState/useMemo/useEffect hook shorthands no longer cause CS0103/CS8130 errors in function-style components

## [1.0.90] - 2026-03-09
- Fix: JSX variable assignments in function-style setup code no longer cause cascading Roslyn errors

## [1.0.89] - 2026-03-09
- Fix tokenization, formatting (Biome/Prettier style), completions, and error squiggle columns

## [1.0.88] - 2026-03-09
- Add Roslyn-powered C# IntelliSense completions, semantic tokens, diagnostics, and formatting inside .uitkx files

## [1.0.87]
- **Fix: function-style components no longer require a companion `.cs` file.**
  The source generator now determines assembly ownership by walking up the
  directory tree to find the nearest `.asmdef` file (Unity's own rule), instead
  of requiring a sibling `.cs` file in the same folder. A `.uitkx` file placed
  anywhere under `Assets/` is routed to the correct assembly automatically.
  Projects with no `.asmdef` files fall back to `Assembly-CSharp` as expected.
- **Feature: optional `@namespace` directive in function-style components.**
  Function-style files now accept an optional `@namespace My.Game.UI` line
  before the `component` keyword (interleaved with `using` lines in any order).
  Priority: inline `@namespace` > companion `.cs` namespace > `ReactiveUITK.FunctionStyle`.

## [1.0.86]
- **Feature: `using` directives in function-style components.**
  Function-style files (`component Name { ... }`) now accept `using X.Y.Z;` lines
  before the `component` keyword. The declared namespaces are emitted as `using`
  statements in the generated C# output alongside the built-in framework usings.
- **Fix: nested generic type arguments in hook shorthands.**
  `useContext<Dictionary<string, Color>>()` and similar 2â€“3 level nested generics
  now correctly expand to `Hooks.UseContext<Dictionary<string, Color>>()`. The
  previous regex stopped at the first `>` making nested types silently unmatched.

## [1.0.85]
- **Fix: stabilize function-style diagnostics and reduce false broken-file errors.**
  Added defensive function-style parsing around leading comments/trivia,
  suppressed legacy missing-`@namespace`/`@component` diagnostics for
  function-style documents, and aligned generator fallback behavior with
  function-style semantics.
- **Fix: improve function-style syntax coloring coverage.**
  Added grammar scopes for function-style `component` declarations and
  `return (...)` keyword matching to improve token consistency in UITKX files.

## [1.0.84]
- **Feature: complete function-style tooling parity across LSP + formatter.**
  Applied canonical lowering in semantic-token and diagnostics pipelines,
  improved go-to-definition for setup variables in `component Name { ... }`,
  added function-style declaration tokenization, and updated formatter support
  to preserve function-style authoring (`component` + `return (...)`) with
  regression tests.

## [1.0.83]
- **Test: add canonical lowering semantic-equivalence coverage.**
  Added a normalization test that validates function-style and directive-header
  sources lower to the same semantic AST shape (including control-flow blocks),
  and updated function-style plan progress for Phase 2 setup-code hoisting.

## [1.0.82]
- **Feature: add canonical lowering stage for function-style components.**
  Introduced a dedicated lowering pass between parse and validation/emission so
  function-style setup code is normalized into a synthetic `@code` root in one
  canonical pipeline stage, with regression tests for both function-style and
  directive-header authoring forms.

## [1.0.81]
- **Feature: strengthen function-style component diagnostics.**
  Added dedicated detection for invalid function-style returns (`UITKX2102`)
  and mixed file forms (`UITKX2104`) so `component Name { ... }` contracts
  now produce deterministic errors for non-markup returns and header mixing.

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
- **Fix: `{/*` auto-closes correctly.** `{/*` â†’ `*/}` is now listed before `{` â†’ `}`
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
- **Feature 8.6.2a: React-style hook shorthands** â€” `useState(0)`, `useEffect(...)`,
  `useMemo(...)` etc. are now valid in `@code` blocks. The emitter normalises them
  to `Hooks.UseState(0)` etc. before generating C#. The rules-of-hooks validator
  (UITKX0015/0016/0018) also fires on the camelCase shorthand forms.
- **Feature 8.6.2b: Hook setter coloring** â€” in `var (count, setCount) = useState(0)`,
  the setter variable `setCount` is now colored with the attribute/property theme
  color, making it visually distinct from the state variable.
- **Feature 8.6.2c: Hook hover documentation** â€” hovering `useState`, `useEffect`,
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
  as attribute content â€” reading `@switch`, `(mode)`, `{...}` switch body, child elements,
  all as bogus attribute tokens. The resulting mangled AST produced wrong semantic tokens
  and wrong diagnostics for the whole file.

  Fix: `ParseAttributes()` now breaks immediately when it encounters `@` or `<` at the
  top of the attribute loop. Both are unambiguous: `@` always signals a control-flow
  directive and `<` always signals a child element â€” neither can appear as a valid
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
  Also changed `uitkxElement` `superType` from `"type"` â†’ `"class"` in
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
- LSP server now targets .NET 8 (was .NET 10) â€” compatible with .NET 8, 9, and 10 runtimes

## [1.0.0]

### Added
- Syntax highlighting for `.uitkx` files (directives, control flow, element tags, attributes, embedded C#)
- Tag completions triggered by `<`
- Attribute completions for all built-in elements
- Directive and control-flow completions triggered by `@`
- Hover documentation (element descriptions, attribute types)
- Language configuration: bracket matching, auto-close, folding
- Bundled LSP server (`UitkxLanguageServer.dll`)
