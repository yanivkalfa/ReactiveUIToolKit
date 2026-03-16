# Changelog

## [1.0.234] - 2026-03-16
- Unreachable code graying: Roslyn CS0162 now dimmed; faster diagnostic refresh; full-line gray-out for keywords in dead code

## [1.0.234] - 2026-03-16
- Unreachable code graying: Roslyn CS0162 now dimmed; faster diagnostic refresh; full-line gray-out for keywords in dead code

## [1.0.233] - 2026-03-15
- Fix: formatter no-op when multiple top-level returns with @(...) in markup (UITKX2103 + UITKX0306 interaction)

## [1.0.230] - 2026-03-15
- Fix semicolon check skipping comments after JSX paren blocks

## [1.0.229] - 2026-03-15
- Fix diagnostics disappearing on save/reformat (carry forward T3 cache); T1 semicolon check for JSX paren blocks

## [1.0.228] - 2026-03-15
- T1 missing-semicolon detection for paren-wrapped JSX blocks in setup code; hover shows inherited base props

## [1.0.227] - 2026-03-15
- T1 missing-semicolon detection for paren-wrapped JSX blocks in setup code; hover shows inherited base props

## [1.0.222] - 2026-03-15
- Move Portal JSX back to setup code using ternary paren-wrapped pattern

## [1.0.221] - 2026-03-15
- Add Portal JSX tag support in source generator, IDE schema, and completions; migrate samples from V.Portal API to JSX markup

## [1.0.220] - 2026-03-15
- Setup JSX: enable T2 unknown-element/attribute checks, C# expression validation, tag completions, and source-map entries for markup embedded in setup code; replace V.Portal with JSX syntax in samples

## [1.0.219] - 2026-03-15
- Virtual document: change useState setter stub from `delegate T __StateUpdater__<T>(T prev)` to `delegate void __StateSetter__<T>(Func<T,T> updater)` so that Roslyn properly type-checks lambda bodies inside state updaters (e.g. `setTreeRows(prev => { ... })`) — the lambda parameter `prev` is now correctly inferred as `T`, enabling full semantic analysis; fixes missing diagnostics for all `setX(prev => ...)` patterns
- Diagnostic mapper: replace CS1660 suppression with targeted CS1503 suppression for state-setter direct-value calls (e.g. `setCount(5)`) by inspecting the error message for `Func<`; real CS1503 type-mismatch errors still surface

## [1.0.218] - 2026-03-15
- Virtual document: fix `seg2Line` calculation in `EmitMappedWithGap` straddle case — post-gap code now accounts for newlines in the removed return statement, preventing `#line` directive offset errors for code after the return block

## [1.0.217] - 2026-03-14
- Virtual document: skip `/* */` and `//` comments in setup-code JSX scanner; fixes regression where `{/* return (...) */}` JSX comment blocks caused `#line` directives to end up inside C# block comments, making them invisible to the compiler — this caused wrong squiggly-line positions and missing diagnostics entirely for code after the comment block

## [1.0.216] - 2026-03-14
- Formatter: expand single-line container JSX in paren blocks (e.g. `(<Box><Label/></Box>)`) into multi-line formatted output; previously only multi-line blocks got JSX formatting
- Virtual document: add Branch 2b for bare `= <Tag` assignment markup so Roslyn no longer shows CS1525/CS0119 errors before format-on-save wraps the parens

## [1.0.215] - 2026-03-14
- Formatter: normalize bare assignment JSX (`var x = <Tag/>` → `var x = (<Tag/>)`) to match arrow-lambda normalization; both `= <Tag` and `=> <Tag` now auto-wrap in parentheses for consistent formatting

## [1.0.214] - 2026-03-14
- Formatter: auto-split `{content` lines where `{` opens a multi-line block with content on the same line (e.g. `{new TabDef { ... }` becomes `{` + `new TabDef { ... }` on separate lines); fixes indentation when list initializer opening brace is on the same line as the first item

## [1.0.213] - 2026-03-14
- Formatter: fix mid-line `{` brace tracking in EmitCSharpLines; object initializers with JSX property values (e.g. `new TabDef { Content = () => (<jsx/>) }`) now correctly maintain brace depth for subsequent sibling properties; fixes `Style` property and closing `};` losing indentation after JSX in nested initializers

## [1.0.212] - 2026-03-14
- Formatter: fix JSX paren-blocks in deeply nested C# (method bodies, if blocks, switch cases, lambda bodies) losing indentation context; use placeholder-based approach so EmitCSharpLines processes entire C# in one pass preserving brace tracking; JSX formatted at correct nesting depth

## [1.0.211] - 2026-03-14
- Formatter: normalize bare arrow JSX (`=> <Tag />`) to paren-wrapped form (`=> (<Tag />) `); single-line elements stay inline preserving C# indentation context; multi-line container elements formatted with proper JSX layout; fix idempotency for closing `)` with trailing content

## [1.0.210] - 2026-03-14
- Fix server DLL not refreshed during VSIX packaging; all virtual document changes now active

## [1.0.209] - 2026-03-14
- Fix stale language-lib DLL in LSP server publish output; all 1.0.208 changes now active

## [1.0.208] - 2026-03-14
- Fix `() => <Tag />` lambda markup not transpiled (Pattern C); fix CS0266 on `() => variable` by using VirtualNode placeholder; re-add UITKX0306 diagnostic for `@(expr)` in setup code; add `@(` stripping in virtual document

## [1.0.207] - 2026-03-14
- Add UITKX0306 diagnostic for @(expr) in setup code; fix transient CS0266/CS1662 errors on @() in lambdas

## [1.0.206] - 2026-03-14
- Fix hover props, lambda markup support, formatter inline self-closing elements

## [1.0.202] - 2026-03-14
- Auto re-diagnose all open documents when workspace index changes (e.g. saving a component now clears stale UITKX0109 errors in other open files)

## [1.0.201] - 2026-03-13
- Fix false unreachable dimming on multi-line return expressions (return () => { ... })

## [1.0.200] - 2026-03-13
- Dim entire scope after return in nested methods/lambdas; suppress duplicate CS0162 squiggle

## [1.0.199] - 2026-03-13
- Extend unreachable code dimming to cover entire scope after return in nested methods and lambdas

## [1.0.198] - 2026-03-13
- Downgrade UITKX2103 from Error to Warning so SG generates real code instead of #error for multi-return components

## [1.0.197] - 2026-03-13
- Fix formatter to use last top-level return in multi-return function-style components

## [1.0.196] - 2026-03-13
- Revert multi-return changes to restore correct dimming behavior

## [1.0.196] - 2026-03-13
- Revert multi-return changes to restore correct dimming behavior

## [1.0.195] - 2026-03-13
- Fix multi-return regression: strip earlier markup returns from setup code, downgrade UITKX2103 to warning, dim earlier returns

## [1.0.194] - 2026-03-13
- Fix formatter extracting wrong return statement in multi-return function-style components (use last return)

## [1.0.193] - 2026-03-13
- Fix formatter extracting wrong return statement in multi-return function-style components (use last return)

## [1.0.192] - 2026-03-13
- Fix formatting for files with return: split setup code at the return gap so the formatter preserves return position and is idempotent; skip UITKX2103 error to allow formatting files with multiple returns

## [1.0.191] - 2026-03-13
- Fix source-map gap for correct Roslyn semantic token mapping

## [1.0.190] - 2026-03-13
- Fixed formatting regression. Fixed return line coloring: C# tokens (return keyword) now emitted for mixed markup-code lines. Suppressed parser errors (UITKX2103 etc.) in unreachable regions.

## [1.0.189] - 2026-03-13
- Fixed return line dimming: return statement no longer flagged as unreachable. Suppressed Roslyn false-positive warnings in unreachable regions. Fixed multi-colored words caused by C# generic types being misinterpreted as UITKX tags.

## [1.0.188] - 2026-03-13
- Fixed function-style component coloring

## [1.0.188] - 2026-03-13
- Fixed function-style component coloring: semantic tokens now properly cover the entire component body. Fixed line-number alignment for code after return statement. Added multi-line unreachable diagnostic for code after return in function-style components.

## [1.0.187] - 2026-03-13
- Fixed unreachable code dimming: uses DiagnosticTag.Unnecessary for uniform opacity fade across all tokens. Removed uitkxUnreachable semantic token modifier. Multi-line diagnostic ranges now cover entire unreachable blocks.

## [1.0.186] - 2026-03-13
- Fix: unreachable code dimming now covers all code after return/@break/@continue in the same scope

## [1.0.185] - 2026-03-13
- Fix: unreachable code dimming now covers all code after return/@break/@continue in the same scope — previously only @break/@continue triggered dimming for sibling nodes; @code blocks with top-level return now properly dim all subsequent siblings

## [1.0.184] - 2026-03-13
- Fix: suppress CS1977 false-positive errors on nested lambdas in block-body lambda attributes (e.g. dm.AppendAction(x, _ => ...)) where param is dynamic

## [1.0.183] - 2026-03-13
- Fix: suppress CS1660 false-positive errors on useState lambda updaters (e.g. setCount(v => v+1)) - reverted struct approach, added CS1660 to diagnostic mapper suppression list

## [1.0.182] - 2026-03-13
- Fix useState lambda errors (CS1660): replace __StateUpdater__ delegate with __StateUpdate__<T> struct mirroring real StateUpdate<T> implicit operators so both setX(value) and setX(prev => ...) compile correctly

## [1.0.181] - 2026-03-13
- Fix useState lambda errors: replace __StateUpdater__ delegate with __StateUpdate__ struct mirroring real StateUpdate<T> implicit operators so both setX(value) and setX(prev => ...) compile without CS1660

## [1.0.181] - 2026-03-13
- Add multi-line tag opening to increaseIndentPattern so cursor indents after <Tag (works at advanced autoIndent level)

## [1.0.180] - 2026-03-13
- Sync editor.tabSize with uitkx.config.json indentSize so onEnterRules indent by the correct amount

## [1.0.179] - 2026-03-13
- Cursor fix: revert self-closing tag rule to none (outdent was wrong)

## [1.0.178] - 2026-03-13
- Cursor fix: self-closing tag onEnterRule uses outdent instead of none

## [1.0.177] - 2026-03-12
- Config precedence fix and logging

## [1.0.176] - 2026-03-12
- Fix: add /> to decreaseIndentPattern so self-closing tag line auto-outdents to tag level

## [1.0.175] - 2026-03-12
- Fix: add /> to decreaseIndentPattern so self-closing tag line auto-outdents to tag level

## [1.0.174] - 2026-03-12
- Fix: add /> to decreaseIndentPattern so self-closing tag line auto-outdents to tag level

## [1.0.173] - 2026-03-12
- Fix: uitkx.config.json now takes precedence over editor.tabSize; add cursor indent rule for multi-line tag openings

## [1.0.172] - 2026-03-12
- re-publish

## [1.0.172] - 2026-03-12
- Fix: enable onEnterRules by switching autoIndent to full; cursor now stays at sibling level after self-closing and closing tags

## [1.0.171] - 2026-03-12
- Fix: tabSize setting now respected from VS Code editor; fix cursor indentation after self-closing tags, closing tags, and closing braces

## [1.0.170] - 2026-03-12
- re-publish

## [1.0.170] - 2026-03-12
- tabSize setting now respected from editor; improved cursor indentation rules

## [1.0.169] - 2026-03-12
- Fix: strip spaces after ( and before ), custom-type declarations recognized as statements for Allman brace normalization

## [1.0.168] - 2026-03-12
- Fix: collapse multiple consecutive spaces to single space, pull misindented Allman braces, tab-to-space conversion in line content

## [1.0.167] - 2026-03-12
- Fix: inner block formatting (Style entries, lambda bodies, switch-case) now normalises indentation in JSX-containing setup code

## [1.0.167] - 2026-03-12
- Fix: inner block formatting (Style entries, lambda bodies, switch-case) now normalises indentation in JSX-containing setup code

## [1.0.166] - 2026-03-12
- Fix: inner block formatting (Style entries, lambda bodies, switch-case) now normalises indentation in JSX-containing setup code

## [1.0.166] - 2026-03-12
- Fix: disable Roslyn FormatStatements in JSX-setup path to prevent save-loop oscillation

## [1.0.165] - 2026-03-12
- Fix: EmitCSharpLines now normalises mixed-indent corruption in files with embedded JSX setup code

## [1.0.164] - 2026-03-12
- Fix formatter mixed-indent bug: IsStatementStarter ensures var/void/control-flow lines normalize to 2sp even when file has mixed indentation; strip 3+ extra spaces after opening '(' in Style tuples; add Section P regression tests (P01-P06)

## [1.0.163] - 2026-03-12
- Fix formatter bug: comment lines no longer suppress over-indent correction; add .csharpierignore

## [1.0.162] - 2026-03-12
- Add exhaustive formatter whitespace regression tests (Sections D-N, 420 tests total)

## [1.0.162] - 2026-03-12
- Add exhaustive formatter whitespace regression tests (Sections D-N, 420 tests total)

## [1.0.161] - 2026-03-12
- Fix: normalize block indentation in setup code (new Style/List initializers)

## [1.0.160] - 2026-03-12
- Fix: normalize block indentation in setup code (new Style/List initializers)

## [1.0.159] - 2026-03-12
- Add activate completion log to diagnose formatter registration

## [1.0.158] - 2026-03-12
- Add diagnostic logging to formatting provider

## [1.0.157] - 2026-03-12
- Fix formatting: re-add explicit DocumentFormattingEditProvider since OmniSharp dynamic registration never fires (DocumentFormattingProvider=null in all initialize responses)

## [1.0.156] - 2026-03-12
- Fix formatting: remove conflicting explicit formatting provider; rely on OmniSharp dynamic registration that was already working

## [1.0.155] - 2026-03-12
- Debug formatter: remove State.Running guard, add output-channel logging to trace format requests

## [1.0.154] - 2026-03-12
- Fix: register document formatting provider explicitly in extension.ts — OmniSharp dynamic registration was silently advertised as null to VS Code, so format-on-save never fired

## [1.0.153] - 2026-03-12
- Fix formatter: skip Roslyn for function-style component setup code to prevent 4-space indentation and structural reformatting in the live extension

## [1.0.152] - 2026-03-12
- Add comprehensive formatter snapshot tests; fix trailing whitespace and blank-line normalisation in all 54 sample files

## [1.0.151] - 2026-03-11
- Fix blank-line preservation in JSX-setup formatter: blank lines between setup statements and the  'var x = (<JSX>)' assignment were erased on save. Now counts leading newlines in the trailing partial-expression segment and re-emits them, so user-authored blank lines (e.g. between two useState calls and var component) survive Format Document.

## [1.0.150] - 2026-03-11
- Fix FMT-1: Roslyn now formats complete setup-code statements independently by splitting at the last statement terminator, eliminating indent inconsistency when preceding statements have stray whitespace. Fix FMT-2/3/4: remove increaseIndentPattern from language-configuration.json so onEnterRules are the sole indent authority on Enter (no more double-indent). Fix editor.tabSize: change default from 4 to 2 to match formatter indentSize, so Enter-key indent level matches formatted output.

## [1.0.149] - 2026-03-11
- Fix Enter-key indentation: remove double-indent on { lines (increaseIndentPattern no longer duplicates onEnterRules brace handling); add decreaseIndentPattern for closing tags; add @case/@default colon rule so Enter after a case clause indents to body level; keep open-tag indent and indentOutdoot rules intact

## [1.0.148] - 2026-03-11
- Fix C# formatting indentation: Roslyn now uses the configured indentSize (default 2) instead of hard-coded 4 spaces; lambda bodies and collection initializers now indent correctly. Fix JSX-in-setup: each C# segment between JSX blocks is now Roslyn-formatted. Fix N-1: property name and field name tokens mapped to variable type for consistent blue highlight of .Current/.Value

## [1.0.147] - 2026-03-11
- Fix JSX-in-setup formatting: each C# segment and JSX block now formatted independently so indentation is correct; fix code after return in function-style components not being dimmed (CS0162 now applies DiagnosticTag.Unnecessary)

## [1.0.146] - 2026-03-11
- N-1: property members now get proper semantic coloring; CO-3: unreachable code hint now visibly fades full line; N-5: auto-indent on Enter restored for JSX tags; CO-4: unused variable is now an error

## [1.0.145] - 2026-03-11
- Fix N-1 useRef<T> regression, N-3 bare return in event handlers, N-5 auto-indent, CO-1/CO-4 type/unused-var diagnostics, CO-3 unreachable hint range

## [1.0.145] - 2026-03-11
- Fix N-1 useRef<T> regression, N-3 bare return in event handlers, N-5 auto-indent, CO-1/CO-4 type/unused-var diagnostics, CO-3 unreachable hint range

## [1.0.144] - 2026-03-11
- N-3: fix bare return in block-body lambda; N-5: auto-indent after open tag; N-1: useRef<T> member completions; CO-1/CO-4: re-enable CS0246/CS0219 diagnostics; CO-3: dim unreachable nodes after @break/@continue; N-6: Roslyn-format entire component setup code on save; CO-2: StyleKeys string value completions; fix CS1977 dynamic-dispatch false positive

## [1.0.143] - 2026-03-11
- N-3 bare return in block-body lambda (CS0126 pragma), N-5 double-indent on Enter (autoIndent advanced), N-1 useRef completions (inline scaffold), CO-1 CS0246 targeted suppressions, N-6 Roslyn formatting for function-style setup code, CO-2 StyleKeys value completions, CO-3 dead-code dimming after @break/@continue (UITKX0110)

## [1.0.143] - 2026-03-11
- N-3 bare return in block-body lambda (CS0126 pragma), N-5 double-indent on Enter (autoIndent advanced), N-1 useRef completions (inline scaffold), CO-1 CS0246 targeted suppressions, N-6 Roslyn formatting for function-style setup code, CO-2 StyleKeys value completions, CO-3 dead-code dimming after @break/@continue (UITKX0110)

## [1.0.143] - 2026-03-11
- N-3 bare return in block-body lambda (CS0126 pragma), N-5 double-indent on Enter (autoIndent advanced), N-1 useRef completions (inline scaffold), CO-1 CS0246 targeted suppressions, N-6 Roslyn formatting for function-style setup code, CO-2 StyleKeys value completions, CO-3 dead-code dimming after @break/@continue (UITKX0110)

## [1.0.143] - 2026-03-11
- N-3 bare return in block-body lambda (CS0126 pragma), N-5 double-indent on Enter (autoIndent advanced), N-1 useRef completions (inline scaffold), CO-1 CS0246 targeted suppressions, N-6 Roslyn formatting for function-style setup code, CO-2 StyleKeys value completions, CO-3 dead-code dimming after @break/@continue (UITKX0110)

## [1.0.143] - 2026-03-11
- fix: revert N-6 formatter (Roslyn class-wrapper corrupted function-style setup code); fix: block-body lambda params now typed with actual UIElements event types (ChangeEvent<dynamic> for onChange, ClickEvent for onClick, etc.) so evt.newValue and other members are available; fix: onEnterRule added for unclosed open tags (pressing Enter after tag name before close-bracket now indents)

## [1.0.142] - 2026-03-10
- fix: block-body lambda handlers (onChange/onClick) now use local function wrapper — return statements and member completions (evt.newValue etc.) work correctly; fix: JSX comment {/* */} inside attribute list no longer parsed as attribute; fix: on-save formatter now Roslyn-formats function-style setup code; fix: Enter-after-tag indentation improved

## [1.0.141] - 2026-03-10
- fix: EnsureReadyAsync double-check bug — workspace now always rebuilt when source changes, fixing stale virtual doc for completions and coloring; fix: CompletionHandler now refreshes virtual doc before source-map gate check

## [1.0.140] - 2026-03-10
- fix: stale virtual doc coloring after completion; fix missing completions in block-body lambda handlers (onClick/onChange/etc.); fix missing completions after typing dot in setup code

## [1.0.139] - 2026-03-10
- skip-no-new-entry

## [1.0.139] - 2026-03-10
- Production-grade C# IntelliSense: replaced Recommender API with CompletionService for context-aware completions (member access, keywords, overloads); attribute expression completions (onClick, style, value); SignatureHelp parameter hints; Roslyn-based hover with type info; fixed keyword popup on non-C# positions

## [1.0.138] - 2026-03-10
- Fix member completions in function-style setup code: AstCursorContext now returns CSharpCodeBlock for code lines, CompletionHandler routes it to Roslyn. Add '.' as completion trigger character. Fix SourceMap.ToVirtualOffset boundary so cursor at end of expression (e.g. after typing '.') still maps into virtual doc.

## [1.0.137] - 2026-03-10
- Fix missing colors inside component declaration parameter list

## [1.0.136] - 2026-03-10
- Fix 'component' keyword color inconsistency — parametered declarations now match same color as bare declarations

## [1.0.135] - 2026-03-10
- Fix false 'Unknown element' diagnostics for recursive components (e.g. DeepNode) and cross-file references during workspace scan

## [1.0.134] - 2026-03-10
- Removed automatic semicolon insertion on format — users must write their own semicolons in .uitkx files

## [1.0.133] - 2026-03-10
- fix: formatter no longer injects spurious semicolons into object initializer expressions or ternary branches; fix: multi-line attribute expression values are now re-indented correctly on format

## [1.0.132] - 2026-03-10
- fix: formatter was dropping component parameters (e.g. string label = 'x') on save in function-style components

## [1.0.131] - 2026-03-10
- fix: formatter was stripping @ from @using directives on save in function-style components

## [1.0.130] - 2026-03-10
- fix: generic method calls like useState<T>() now colored as function calls instead of variables

## [1.0.129] - 2026-03-10
- fix: @ symbol in @if/@for/@switch etc. now always colored — uses keyword scope instead of punctuation scope which many themes don't color

## [1.0.128] - 2026-03-10
- fix: grammar coloring consistency — tag names always colored; @if/@for etc. colored inside paren expressions; control-flow body braces use full markup context

## [1.0.127] - 2026-03-10
- fix: function-style body C# coloring via grammar fallback; @code block semantic tokens now wired up; hook setter colors activated; unreachable code dimming after @break/@continue

## [1.0.126] - 2026-03-10
- fix: function-style body C# coloring via grammar fallback; @code block semantic tokens now wired up; hook setter colors activated; unreachable code dimming after @break/@continue

## [1.0.126] - 2026-03-10
- fix: closing tag no longer under-indented after Enter inside JSX tags

## [1.0.126] - 2026-03-10
- fix: closing tag no longer under-indented after Enter inside `<Tag>content</Tag>` — removed `</` from `decreaseIndentPattern` which was double-outdenting the closing tag already placed correctly by the `indentOutdent` onEnterRule

## [1.0.125] - 2026-03-10
- fix: formatter preserves @namespace/usings; fix body indent drift; fix double-indent on Enter after JSX tags

## [1.0.125] - 2026-03-10
- skip

## [1.0.125] - 2026-03-10
- fix: formatter preserves @namespace/usings in function-style components; fix body indent drift on successive saves; fix double-indent on Enter after JSX tags

## [1.0.125] - 2026-03-10
- fix: formatter re-emits `@namespace` / `using` preamble for function-style components so they are no longer stripped on save
- fix: body indentation no longer drifts forward on successive saves (firstLineStripped anchor corrected for function-style setup code)
- fix: on-enter after `<Tag>` no longer double-indents — removed JSX tag from `increaseIndentPattern` so only `onEnterRules` drives indent (was compounding with them)

## [1.0.124] - 2026-03-10
- @for/@foreach/@while/@if headers now validated by Roslyn with column-accurate squiggles

## [1.0.123] - 2026-03-10
- UITKX0109 unknown attribute on a known element is now an Error (red) instead of Warning (yellow)

## [1.0.122] - 2026-03-10
- Add Router, Route, Link, Suspense to schema as known elements with their attributes, fixing unknown-element and unknown-attribute warnings for those framework components

## [1.0.121] - 2026-03-10
- Fix nullable warnings: use #nullable enable annotations instead of #nullable enable so VisualElement? compiles without cascading CS8600/CS8603/CS8604 warnings; add all BaseProps event handlers (onClick, onPointerDown, extraProps etc.) to schema universalAttributes fixing unknown-attribute warnings on RepeatButton and all workspace elements; add text attribute to Text schema element

## [1.0.120] - 2026-03-10
- Fix CS8632: add #nullable enable to virtual doc and suppress at compile level; fix ValuesBarFunc: scan all *.cs files (not just *Props.cs) to index nested class Props components; fix BatchOnClick CS8974: suppress method-group-to-object conversion warning

## [1.0.119] - 2026-03-10
- Fix CS8632 nullable annotation warning in virtual documents; fix ValuesBarFunc (nested class Props) not indexed; parse function-style params from .uitkx components so attributes are validated correctly

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
