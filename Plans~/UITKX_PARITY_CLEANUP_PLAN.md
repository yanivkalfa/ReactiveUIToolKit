# .uitkx Parity Cleanup Plan (Unity side)

> **Origin.** The 2026-07-03 `.uitkx`↔`.guitkx` parity investigation
> (`ReactiveUI-Gadot/plans/UITKX_GUITKX_SYNTAX_PARITY.md` — "matrix" below) found Unity-side defects
> alongside the Godot gaps. The Godot work is planned in
> `ReactiveUI-Gadot/plans/SYNTAX_PARITY_EXECUTION_PLAN.md` and is **in scope now**; this file is the
> deferred Unity-side batch — **not currently scheduled** (user decision 2026-07-03).
>
> Path shorthands as in the matrix: `DP/UP/MT/DA/DC` = language-lib Parser/Diagnostics files, `SV/CE/PR/UD/PIPE` =
> SourceGenerator~ files. Line numbers anchored at the 0.6.4-era tree; re-locate by identifier if drifted.
> Standing rule: after language-lib/SourceGenerator~ edits, rebuild committed DLLs via
> `scripts/build-generator.ps1` and run both test suites (see CLAUDE.md).

## Explicit non-task (user decision 2026-07-03)

**Text interpolation stays as-is on Unity** (text does not stop at `{`; `{expr}` only at node start —
MT:340-353). Rationale: UI Toolkit requires text inside a text-bearing element, and typed-props made
mid-text interpolation impractical here. The **Godot side converges to Unity's semantics** instead
(their plan, T2.4). Optional courtesy (fold into U5 if desired): a low-severity IDE hint when a text
node contains `{...}` that parses as an expression — "braces in text render literally".

## Status legend: ⬜ todo · 🟨 in progress · ✅ done

### U1 — Dead diagnostic weight  · effort: small · Status: ⬜
Delete never-emitted constants/descriptors and phantom numbering so the code space is honest before the
cross-side concordance page (Godot plan T3.1) links to it:
- Constants never emitted: UITKX0005, 0012, 0101, 0102 ("MissingComponent", DC:54), 0110 (DC:101).
- The entire unused SG descriptor set UD 0300-0305 (bridged parser diags carry their own codes).
- Phantom numbers surviving only in comments: 2103; 0009/0010/0017/0022-0024 in stale headers (SV:15-19).
- UITKX0303 means different things in language-lib vs the SG descriptor — keep one, kill the other.
**Done when:** every code in DC.cs has ≥1 emission site (enforced by a reflection test that greps
emission sites per constant, or a curated allowlist with reasons).

### U2 — Severity reconciliation  · effort: small · Status: ⬜
Same mistake, different severities per surface today:
- IDE Error vs compile Warning: 0104 (dup key), 0106 (missing key), 0305 (unknown directive).
- Documented Warning but emitted Error: 0105, 0109, 0121 (DC:92-94 doc comments vs DA/CE emissions).
Pick ONE severity per code (recommend the IDE's, it's the user-facing surface), align emitters + doc
comments, add a table-driven test asserting severity per code. Coordinate numbers/severities with the
Godot renumbering (their T3.1/T3.2) so the concordance is clean.

### U3 — Diagnostics dropped at the Unity bridge  · effort: medium · Status: ⬜
- All language-lib diagnostics bridged with `Location.None` are invisible in Unity (PIPE:412; dropped at
  PIPE:176-178) — parser warnings 2106/2203 and 0150 never reach the console. Fix: synthesize a real
  `Location` from the parse span (the pipeline has the file path + offsets) instead of `Location.None`.
- Surfaced errors lose their code: `#error` text strips the UITKX prefix (PIPE:200) — keep the code in
  the message so users can search docs.
- Setup-code JSX parse errors are computed then discarded (`jsxDiags` never read, PIPE:388-392) — report them.
- 0025/0026 exist only as raw `#error` strings with no descriptor (CE:442, 2865); give them descriptors,
  and give the code-less `#error UITKX:` for `@foreach`-in-expression (CE:2692) a real number (next free
  002x, coordinate with Godot's adoption of 0026).
**Done when:** a `.uitkx` with a parser warning shows it in the Unity console with its code, and no
diagnostic path computes-then-discards.

### U4 — Internal multi-root counting mismatch  · effort: small · Status: ⬜
DA counts elements+directives+expr+text as roots for 0108 (DA:207-253); SV counts only elements
(SV:57-76) — IDE and compile can disagree on the same file, backstopped only by `#error UITKX0025`.
Extract ONE root-counting routine into language-lib, call it from both (the SG already references the
lib), pin with a shared-fixture test — this is the same "two implementations, no contract" disease the
Godot side has at full scale (matrix §5.2).

### U5 — Silent tolerances worth closing  · effort: medium · Status: ⬜
From matrix §3.5 (each: emit at the right layer + test):
- Duplicate attributes on one element — silent, last-wins (UP:753-871; CE:1362-1369) ⇒ warning.
- Expression-valued duplicate keys never dup-checked (CE:2151 literal-only) ⇒ adopt the Godot approach:
  compare normalized expression signatures, not just string literals.
- `Key=`/`key` case inconsistency: `Key=` satisfies the missing-key check but is invisible to dup-check
  (DA:758-761 vs 1030-1033) ⇒ one canonicalization helper used by both.
- camelCase `useEffect(` escapes 0018 (SV:177 matches exact casing) ⇒ case-tolerant match + fixture.
- Stray `>` in markup silently swallowed (UP:621-625) ⇒ 0300-family error.
- Missing-key check is direct-children-only ⇒ decide (document as intended, or recurse) — small design note.

### U6 — Docs & samples truthfulness  · effort: small · Status: ⬜
- Document `{__children}` (load-bearing in `Samples/Components/DirectiveSuccessDemo/SectionBox.uitkx:14`,
  zero doc hits) and the bare-boolean attribute shorthand (UP:858-867).
- Document prop spread `{...expr}` as an intentional **Godot-only** divergence on the reference page.
- Fix: stale `@()` comment (`DeepNestedSection.uitkx:48`); stale directive list in the pipeline header
  (PIPE:22); UITKX0305's message still lists always-rejected `code` as valid (UP:1617).

### U7 — Optional: single-quote attribute strings  · effort: small · Status: ⬜ (decision)
Unity accepts only `name="..."` (single quote = UITKX0300, UP:797-867); Godot and JSX accept both.
Accepting `'...'` is a superset change (nothing breaks). Take it or explicitly reject it in this file —
either way the reference page states the rule.

## Definition of done
U1–U6 landed with tests; committed generator DLLs rebuilt; both suites green
(`SourceGenerator~/Tests`, `lsp-server/Tests`); versions bumped per VERSIONING.md (all patch-grade
except U3 if new codes surface — minor); the cross-side concordance page (Godot plan T6.1) updated to
reflect any number/severity changes made here.
