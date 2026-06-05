# Optimizations

Performance, hygiene, and waste-reduction items that are NOT correctness
bugs. Each entry must explain (a) what work is wasted, (b) why it is
wasted, (c) what the budget is today, (d) what the budget would be after
the fix, and (e) why we have not already shipped the fix.

For correctness bugs, file under `TECH_DEBT_V2.md` instead. If an item
crosses the line (e.g. a perf regression that breaks UX latency budgets),
move it to `TECH_DEBT_V2.md` and link from here.

## Entry shape

```
## <N>. <Short headline>

**Status:** Open | In progress | Shipped vX.Y.Z | Rejected
**Severity:** P0 | P1 | P2 | P3
**Category:** HMR | LSP | SG | Runtime | Editor | Build
**Surface area:** <files / subsystems touched by the wasted work>
**User-visible?:** No | Cosmetic | Latency | Memory | Other

**What is wasted.** ...

**Root cause.** ...

**Current cost.** ...

**Post-fix cost.** ...

**Candidate fix.** ...

**Why deferred.** ...

**Surfaced by.** ...
```

### Severity calibration

- **P0** - dominates a hot path; user-perceptible latency or memory
  budget exceeded. Blocks a release.
- **P1** - measurable on a profile (>5% of a known hot path) but no
  user-perceptible symptom yet. Schedule for the next release.
- **P2** - shows up only under non-default workflows or rare inputs.
  Backlog.
- **P3** - hygiene only; would not appear on a profile under any
  realistic workload. Fix opportunistically when the surrounding code
  is touched for another reason.

---

## 1. HMR dependency index over-links near-clones via leftover module tokens

**Status:** Open
**Severity:** P3
**Category:** HMR
**Surface area:** `Editor/HMR/UitkxFileDependencyIndex.cs` -
`TryIndexFile` token scan, `s_moduleReverse` reverse map, cascade
walker (`CollectTransitiveDependents`).
**User-visible?:** No. After Issue 14.1 shipped in 0.5.21, cascade-
pulled near-clones get the cheap trampoline-field swap only and skip
`FullResetComponentState`, so the prior scene-duplication symptom is
gone. The only remaining cost is one extra Roslyn compile per save in
the affected workflows.

**What is wasted.** When a user does the common copy-rename refactor
to add a sibling component (duplicate `Marker.uitkx` -> `Bob.uitkx`,
rename the `component` and `module` declarations, but forget to update
internal `Marker.Style.X` / `Marker.Padding` token references), the
dependency index registers `Bob.uitkx` as a referrer of module
`Marker`. From that point on, every save of `Marker.uitkx` cascades
into `Bob.uitkx`, pulling it through `UitkxHmrCompiler.CompileBatch`
even though `Bob.uitkx` does not actually depend on the saved edit.
Cost is one extra full Roslyn parse + Emit + Load per save per
near-clone, plus the per-fiber `NotifyMatchingFibers` walk on every
mounted `Bob` instance.

**Root cause.** `TryIndexFile` populates `ReferencedModules` by running
`s_pascalDottedRegex` (`\b(?<name>[A-Z][A-Za-z0-9_]*)\s*\.\s*[A-Za-z_]`)
over the body after `s_stripRegex` removes string literals and
comments. The strip pass does NOT remove:
- USS block bodies (`.selector { background-color: red; }`),
- JSX attribute string positions that contain inline expressions,
- asset-path argument literals.

The token scan therefore picks up every `PascalCase.Member` regardless
of whether it lives in an executable position. The reverse-map entry
gets committed in `CommitNode` (`s_moduleReverse[name].Add(filePath)`)
and the cascade walker happily follows it. The existing
`DeclaredModules.Contains(name)` filter (line ~393) only catches the
file's own freshly-declared modules, which does not help on a copy-
rename where the leftover token references the *previous* module name.

**Current cost.**
- Per affected save: ~80-300 ms wall-clock (one extra Roslyn compile
  per near-clone file pulled in; scales linearly with the number of
  near-clones in the cascade closure).
- Per cascaded near-clone fiber: one `__hmr_Render` field write +
  `NotifyMatchingFibers` walk. Microseconds; negligible against the
  Roslyn cost.
- Workspace-indexer cost: zero change. The over-linking happens at
  index time, which already ran.

**Post-fix cost.**
- Narrow fix (component-name self-skip): zero cost in steady state.
  One additional `string.Equals` per `PascalCase.Member` token at
  `TryIndexFile` time, paid once per file index (not per save).
- Full fix (USS-lexer-shaped strip): the strip regex grows. Indexer
  cost rises a few percent on the cold workspace scan; per-file
  invalidate cost is unchanged because it runs on save only.

**Candidate fix (narrow, safe).** In `TryIndexFile` after the
`DeclaredModules.Contains` filter (~line 393):

```csharp
if (!string.IsNullOrEmpty(node.ComponentName)
    && string.Equals(name, node.ComponentName, StringComparison.Ordinal))
    continue;
```

This covers the sub-case where a copy-renamed file has leftover tokens
referencing its OWN previous component name (the common case when the
user renames `component Marker -> component Bob` but forgets a
`Marker.X` in the body). It does NOT cover the sub-case where leftover
tokens reference a SIBLING module declared in another file - that
needs the larger fix below.

**Larger fix (USS-lexer-shaped).** Extend `s_stripRegex` to also drop:
- USS block bodies (`{ ... }` after a CSS-style selector context),
- JSX attribute string positions before token scanning.

This is a structural edit to a hot path; needs benchmarking against
the workspace-wide indexer cost before landing. Likely needs a small
state machine (the current strip pass is regex-only and cannot match
balanced braces correctly).

**Why deferred.**
1. Not a correctness bug after 14.1 shipped. The user-visible scene-
   duplication symptom is already fixed.
2. The narrow fix is only a partial win (does not catch sibling-module
   leftover tokens), and the full USS-lexer-shaped fix is a real
   structural edit that warrants benchmarking before landing.
3. No telemetry on how often copy-rename leftover tokens occur in the
   wild. Without that, the cost of the extra compile in affected saves
   could be acceptable (~one save in twenty during refactor flows, with
   each extra compile costing ~200ms - perceptible but not blocking).

When fixing: implement the narrow change first and ship it as P3
hygiene; revisit the larger USS-lexer pass when the strip regex is
touched for an unrelated reason (e.g. a parser-side change to USS
handling), to amortise the benchmarking effort.

**Surfaced by.** Issue 14 investigation (2026-05-19). See
`Plans~/ISSUE_14_FIX_PLAN.md` Patch 14.2 for the original analysis.

---
