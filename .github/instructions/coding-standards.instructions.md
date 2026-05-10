---
name: 'Coding standards'
description: 'ReactiveUIToolKit-internal policy on code style, comments, special characters, and how to approach fixes. Does not apply to consumer projects.'
applyTo: '**/ReactiveUIToolKit/**'
---

# Coding standards

These rules apply to **all code edits inside the ReactiveUIToolKit
package** (the four parallel emission/analysis layers — SG, HMR, IDE
virtual doc, shared parser — plus their tests, scripts, and tooling).
They do **not** apply to consumer projects that merely use the package
(e.g. `JustStayOn/Assets/UI/**`), and they do **not** apply
to Markdown documentation, Discord posts, or commit messages — those have
their own conventions.

## Style — code files only

- **No emojis** in source code, generated code, log output, diagnostic
  messages, or analyzer descriptions.
- **No special / non-ASCII characters** in identifiers, string literals
  shipped to users, or diagnostic text. The only allowed non-ASCII
  characters in human-facing text are em-dash `—` and arrow `→`, and
  only when they materially aid readability.
- **No `//` line comments unless strictly necessary.** Useful cases:
  - Explaining non-obvious intent inside a complex algorithm (a one-liner
    naming the invariant or the reason for an unusual step).
  - Citing a spec, RFC, or ticket number.
  - Marking a deliberate workaround with a `TECH_DEBT_V2` item reference.

  Bad cases (do not add):
  - Restating what the code does (`// increment counter` above `i++`).
  - Section banners (`// ---- Helpers ----`).
  - Author tags or change-log style comments.
  - Commented-out code. Delete it.
- **No XML doc comments (`///`)** unless the symbol is part of a public,
  consumer-facing API surface. Don't add them to internal helpers or
  newly modified existing code that didn't already have them.
- **Match the surrounding file's style** for indentation, brace placement,
  and naming. Don't reformat unrelated lines.

## Approach — research before editing

Before applying any non-trivial fix or feature, do a **deep-research
pass** instead of jumping to a conclusion. Read the relevant code, trace
the data flow, and explicitly answer the impact questions below in the
reply *before* making the edit. If you can't answer one, search the
codebase until you can — or surface the unknown to the user.

Mandatory questions for any change beyond a typo / one-line fix:

- **Blast radius.** What other call sites depend on this symbol /
  format / contract? Use `vscode_listCodeUsages` or `grep_search` to
  enumerate them; don't guess.
- **Parity.** Does this code have a mirror in any of the four parallel
  layers (SG / HMR / IDE virtual doc / shared parser)? If yes, the
  same change usually has to land in all of them — see the parity
  rule below.
- **HMR safety.** Will the change break HMR's reflection plumbing in
  `UitkxHmrCompiler` or its emit-time contract with `HmrCSharpEmitter`?
  Anything that adds a new shared parser entry point typically needs a
  new reflection delegate.
- **Source-map / diagnostic-mapping safety.** Does the change shift
  generated-line offsets? If yes, every cursor-context test, hover
  test, and diagnostic-mapping test that pins a specific line is at
  risk.
- **Performance.** Hot paths to be wary of: parser scanners
  (per-keystroke in the LSP), `FindBareJsxRanges`, splicers (called
  once per `{expr}` per emit), and anything inside the formatter
  (called on every save). A linear scan added inside an inner loop
  becomes quadratic for free.
- **Test coverage.** Does an existing test pin the current behaviour?
  If yes, the test either confirms the bug (rewrite the test as the
  first step) or confirms the fix (the test should pass after the edit
  with no rewrite). State which case applies.
- **Backwards compatibility.** Will existing `.uitkx` files in real
  user projects (`c:\Users\neta\Pretty Ui\Assets\UI\…`) still
  compile and render the same after the change? A diagnostic
  severity bump from Warning → Error is a breaking change.

Skip the questions only when the change is genuinely trivial (a typo,
a comment fix, a string-literal correction). Document the answers
inline in the reply, not as code comments.

## Approach — root cause, not patch

- **Identify the root cause before editing.** State it in one line in the
  reply before the fix, e.g.
  *"`SpliceExpressionMarkup` doesn't recognise `&&` as a JSX trigger."*
- **Fix at the layer where the bug lives**, not at the call site that
  surfaces it. Wrapping a caller in defensive code to avoid hitting the
  bug is a patch, not a fix.
- **Check parity layers.** This codebase has four parallel emission /
  analysis layers that must stay in sync:
  - SourceGenerator (`SourceGenerator~/Emitter/*`)
  - HMR emitter (`Editor/HMR/Hmr*Emitter.cs`)
  - IDE virtual document (`ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`)
  - Shared parser/scanner (`ide-extensions~/language-lib/Parser/*`)

  When fixing a bug in one, check the other three for the same bug and
  fix in parallel. Don't leave parity drift. The
  `HmrEmitterParityContractTests` will catch SG↔HMR drift but not the
  IDE virtual doc.
- **Don't suppress unexplained errors.** No `try { ... } catch { }`
  around code that "sometimes throws". No `#pragma warning disable`
  unless the warning is a known false positive with a comment explaining
  why.
- **Don't comment out failing tests.** Either fix the test (if the
  expectation is wrong) or fix the code. If the failure is genuinely
  out-of-scope for the current change, file it in
  [Plans~/TECH_DEBT_V2.md](../../Plans~/TECH_DEBT_V2.md) and explain
  why in the reply before deferring.
- **Workarounds require a tech-debt entry.** If a fix can't reach the
  root cause this iteration (e.g. it would require touching three
  splicers when the user only asked for one), append an item to
  `Plans~/TECH_DEBT_V2.md` describing what's deferred, why, the
  trigger to revisit, and the files involved. Reference the item
  number in the changelog entry.

## When in doubt

Ask before guessing. If the user's request is ambiguous between a quick
patch and a deeper fix, surface the trade-off in one sentence and let
them choose. Prefer the deeper fix when the quick patch would leave
parity drift between the four layers above.
