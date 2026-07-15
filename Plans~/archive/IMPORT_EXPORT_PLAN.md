> **ARCHIVED 2026-07-15** — Fully executed on feat/uitkx-imports (0.7.0/0.7.1): grammar, strict diagnostics, path-derived namespaces, accessibility, mixed-decl emit, codemod, LSP, HMR mirrors incl. path-qualified hook family keys. The 'EXECUTION NOT STARTED' header below is historical. Deliberate deviations: 2313 conventions are documentation-only, 2310 retired, UITKX0103 no longer emitted, singular ComponentName compat reads kept.

# UITKX Import/Export — Unity leg execution plan (leg 3 of the family campaign)

> STATUS: approved design (owner sign-off 2026-07-11, rounds 1–3 + amendments A1–A8). EXECUTION NOT STARTED.
> Branch: `feat/uitkx-imports`. One PR. This file is SELF-CONTAINED — implement without re-research.
> Family master plan: `ReactiveUI-Unreal/plans/IMPORT_EXPORT_MASTER_PLAN.md` (canonical). This doc duplicates
> every family-shared contract on purpose.

## CONTRACT FREEZE (leg 1 shipped) — 2026-07-11

> Unreal leg 1 **SHIPPED** on `feat/uetkx-imports` (21 commits, verified green: battery 55/55,
> `RUICompile -check` 0 drift, corpus + mirror gates, VS2022 vsix rebuilt). This section is the
> AS-SHIPPED family contract extracted from that branch (`UetkxResolve.cpp`, `UetkxFileScan.cpp`,
> `UetkxDriver.cpp`, `plans/TECH_DEBT.md` TD-023..025) and SUPERSEDES §3's draft table wherever they
> disagree. **This leg (leg 3) still starts only after the Godot leg (leg 2, `feat/guitkx-imports`)
> merges** — family order unchanged; the Godot leg's own gates (doom + naming merges) are satisfied
> and its restart is contract-frozen in `ReactiveUI-Gadot/plans/IMPORT_EXPORT_PLAN.md` §0.

### Frozen family diagnostic table — UITKX2300–2309 (+ reserved band)

Family block = **2300–2315**, confirmed by leg 1's shipped M0.2 audit (23xx free in all three
registries). Codes + messages are identical family-wide modulo prefix (`UETKX`→`UITKX`) and extension
(`.uetkx`→`.uitkx`). Only 2304 is a warning. Shipped emit split: 2303/2309 fire at SCAN (file-local),
2300/2301/2302/2304/2305/2307/2308 at RESOLVE, 2306 at the driver/sweep level.

| Code | Sev | Frozen message (Unity substitutions applied) |
|---|---|---|
| UITKX2300 | Error | ``unknown import specifier `{0}` — no file at {1}(.uitkx)`` (both args = the specifier) |
| UITKX2301 | Error | `` `{0}` is not exported by {1} — add `export` to its declaration `` |
| UITKX2302 | Error | `` `{0}` is not declared in {1} `` |
| UITKX2303 | Error | ``duplicate import of `{0}` (already imported from {1})`` |
| UITKX2304 | Warning | ``unused import `{0}` `` |
| UITKX2305 | Error | `` `{0}` is defined in {1} but not imported — add: import {{ {0} }} from "{2}" `` |
| UITKX2306 | Error | ``value-import cycle: {0} (hooks/modules load eagerly — break the chain or move to component refs)`` — ONE placeholder = the whole chain of filenames, `A.uitkx -> B.hooks.uitkx -> A.uitkx` |
| UITKX2307 | Error | `` `{0}` is used like a uitkx component/hook but no file exports it `` (export-table names only; ambient C# exempt, A4) |
| UITKX2308 | Error | ``import crosses a module/root boundary ({0} -> {1}) — imports are module-scoped in v1`` (Unity boundary = asmdef) |
| UITKX2309 | Error | ``import must appear in the preamble, before the first declaration`` |
| UITKX2310–2315 | — | stamped "reserved (family)" in the shipped canonical table — see the registration note below |

Renumberings/rewordings vs §3's draft (the §3 table below is already fixed in place):

- Frozen **2308 = the family cross-boundary code** (A1: "one family code"; Unity's boundary is the
  asmdef) — the draft had it at 2309.
- Frozen **2309 = import-after-first-decl** (imports are PREAMBLE ONLY) — missing from the draft.
- The draft's 2308 (engine-native specifier) is NOT a family code: in leg 1, engine-native forms
  simply fail resolution → **2300**. Mirror that: `Assets/`, `Packages/`, absolute → unresolvable →
  2300 (the resolver never maps them).
- Wordings for 2300/2301/2303/2304/2305/2306/2307 align to shipped text (family rule: identical
  modulo prefix/extension). Notable: 2301 carries the `add \`export\`` fix-it, NOT an
  "Exported names:" list; 2305 says "defined" and prints the exact import line to add; 2306 prints
  ONE placeholder holding the joined chain plus the "(hooks/modules load eagerly …)" tail.
- **2310–2314 (Unity leg-local: namespace derivation, accessibility mismatch, container merge,
  convention lint, `~/` root):** these sit inside the family-reserved band. At leg-3 start,
  REGISTER them into the canonical Unreal table (as Unity-only rows) so no sibling reuses the
  numbers; 2315 stays free. Their messages are Unity-local (no family mirror required).
- **2105 note:** leg 1 retargeted UETKX2105 → sev-1 WARN "one component per file — convention"
  (unparseable junk stays a 2101-class error). Unity's planned 2313 lint overlaps (it also covers
  hooks-outside-`.hooks` + filename mismatch). Reconcile at leg-3 start: retarget UITKX2105 to the
  family message for the one-component case and keep 2313 for the Unity-only conventions, or record
  the consolidation under 2313 as a per-repo divergence in the registry note.

### Frozen cycle semantics

- **Cross-file COMPONENT cycles are LEGAL family-wide — UETKX2107 RETIRED in leg 1.** Unreal earned
  it with a two-phase DECL/BODY emit + aggregator (DECL phase = complete props structs + defaulted
  wrapper fwd-decls + hook fwd-decls + module bodies; BODY phase = impls + default-free wrapper defs
  + registrations; every generated file included twice) and a committed `CycleProof/` fixture pair
  that compiles AND renders. **C# is order-free (partials + no decl-order dependence), so Unity gets
  this naturally — zero emit work**; mirror the CycleProof pair as a test fixture to PROVE it.
- **2306 value-cycle DFS is still REQUIRED for family parity** (TDZ-parity POLICY, locked): edges =
  imported HOOKS + MODULES only (component edges exempt); DFS per asmdef in the SG pre-scan over
  fresh preamble scans; the error prints the full chain (frozen message above).

### Other shipped semantics to hold (as-built in leg 1)

- **Graph truth = declared imports in SOURCE (A2):** shipped — Unreal's aggregator now orders from
  RESOLVED preamble import edges (Kahn topo, alpha ties, cycle remainder alphabetical); sidecars
  demoted to per-machine cache/verifier only. Unity has no sidecars — already true; §8's import
  graph must stay source-scanned.
- **Single-sweep staleness fixpoint:** shipped — ONE `CompileAllRoots` call converges (pass 1
  compiles stale; if any export_hash moved, an internal pass 2 re-sweeps importers; error verdicts
  invalidate when a recorded dep export-hash moves; unresolved specifiers record the
  resolved-candidate path with hash 0). Unity analog = §8's reverse-edge invalidation of
  `_cachedCompilations`: editing B must invalidate every importer A in the SAME file event.
- **Codemod default:** export-EVERYTHING, idempotent + re-runnable, reference scan is a FRESH scan
  (tags + hook calls + module member quals) — never table/edge reuse. §11 matches; hold it.
- **`#line` project-relative:** shipped in leg 1 (VS2022 breakpoint bind on `.uetkx` source
  confirmed by owner; forward slashes, fixtures use `<Basename>` form). Unity's HMR emitters
  already emit `#line` — keep parity (§8 "already shipped, keep").
- **Privacy = compile-time scoping + exported-names-only ledgers:** leg 1 wraps non-exported decls
  in a per-file detail namespace (no registration → tree-shaken) and keys duplicate-binding ledgers
  on EXPORTED names only; runtime registries stay name-flat (accepted v1 divergence, Unreal
  TD-026). Unity analog: export→`public`/`internal` + strict diags as the real fence (§6) — same
  compile-time-only stance.

### Corpus mirror sources (from the Unreal repo, branch `feat/uetkx-imports`)

- `ide-extensions/lsp-server/test-fixtures/uetkx-scanner-cases.json` — the shared scanner corpus.
  `_tiers.familyCore = ["skipNoncodeMarkup", "findMatchingMarkup", "fileScan"]` is byte-identical
  family-wide after `UETKX|GUITKX|UITKX → TKX` code-prefix normalization; `_tiers.perLeg`
  (`skipNoncode`, `findMatching`, `fileScanLeg`) is engine lexis — NOT mirrored/hashed.
- `ide-extensions/lsp-server/test-fixtures/uetkx-formatter-cases.json` — the shared formatter corpus
  (import canonicalization, `export`, mixed-decl idempotency); mirror in shape, not hashed.
- `scripts/corpus-hash.mjs` + `plans/family-corpus.hash` — the TD-009 drift gate: adopt the same
  script + hash file here (§14), point it at the mirrored corpus, wire into CI. Pinned family hash
  as of the freeze: `657e5f4ef77cb44df693e7cfebc1112163cdc1ee2bd541b4b5e1069abb08013b`.
- `Source/RuiHostTests/ContractFixtures/` — contract fixtures to mirror in shape: `Showcase` /
  `StatusChip` / `ChipSupport` / `Counter` / `Preamble` / `Palette` (`.uetkx` + `.inl.expected`),
  `BadAttr` (+ `.diags.expected`), and `ImportError.uetkx` + `ImportError.uetkx.diags.expected`
  (pins 2300/2301/2305 message text).
- `Source/RuiHostTests/CycleProof/` — `CycleA.uetkx` ↔ `CycleB.uetkx` (+ `.uetkx.diags.json`,
  `.uetkx.inl`): the component-cycle-LEGAL proof pair (compiles + renders).
- `Source/RuiHostTests/Private/ReactiveUIUetkxResolveTest.cpp` — the resolve suite exercising every
  2300–2309 code; mirror its case list into `ImportResolutionTests.cs` (§15).

## 0. Sequencing gates (family order: Unreal → Godot → Unity)

- GATE-1: Unreal leg (`feat/uetkx-imports`) MERGED — it freezes the family diagnostic table + the canonical
  shared corpus + the TD-009 hash-manifest gate. Do not start until both are frozen there.
- GATE-2: Godot leg (`feat/guitkx-imports`) MERGED (itself gated on `feat/doom-guitkx-port`).
- GATE-3 (intra-leg): extend `SourceGenerator~/Tests/HmrEmitterParityContractTests.cs` BEFORE touching either
  emitter (locked decision 5: SG + HMR emitters change in lockstep under that contract).
- GATE-4 (intra-leg, committed-DLL ordering — adversarial A7(h)): grammar changes are INERT in Unity until
  `Analyzers/*.dll` are rebuilt+recommitted. Commit order inside the PR is fixed (§12).

## 1. Locked family decisions (verbatim scope for this leg)

1. **Strict from day one** — implicit cross-file resolution is an ERROR the moment the feature lands; the
   migration codemod runs inside the same PR so every existing file gains import lines before strictness is on.
2. **Named exports only** — no `default`, no `import *`. (`export { X } from "./y"` re-export = fast-follow.)
3. **Specifiers: relative (`./`, `../`) AND root alias `~/`, both v1.** Engine-native forms (`Assets/`,
   `Packages/`, absolute) FORBIDDEN in import specifiers. `~/` = UI source root from `uitkx.config.json`
   walk-up key `"root"`, engine default `Assets/`. Extensionless — `.uitkx` implied.
4. **ESM cycle parity**: cross-file COMPONENT cycles legal (C# partials are order-independent — zero extra
   work in this leg). VALUE cycles (hook/module import cycles) = compile ERROR printing the chain
   (TDZ-parity POLICY, uniform family-wide, even though C# itself would tolerate them).
5. **Full mixed-decl files in v1** (round 3): multiple components + hooks + modules per file. Data-model +
   emitter + HMR + LSP rewrite is in scope (§5, §8).
6. **`~/` extends into asset references in v1** (round 3): `Asset<T>` strings + `@uss` — AssetPathUtil + the
   byte-for-byte HMR mirror + DiagnosticsAnalyzer (§9).
7. **`@namespace` becomes an OPTIONAL interop override** (round 3). Default namespace is derived
   deterministically from the asmdef-relative path: `ReactiveUITK.Uitkx.<AsmdefRelPath.Segments>` (§4).
   `InferFunctionStyleNamespace` (companion-.cs regex read, `DirectiveParser.cs:1067-1089`) is RETIRED from
   the import contract — a target's identity may never flip on a `.cs` edit. `@using` survives for engine
   namespaces in embedded C#.
8. **Imports are asmdef-scoped v1** (A7d): crossing an asmdef boundary = family diagnostic
   UITKX2308 (the frozen family cross-boundary code — see CONTRACT FREEZE; was 2309 in the draft).
9. **Strict mode polices markup-owned names only** (A4): names exported by `.uitkx` files. Hand-written C#
   (companion `.cs`, engine types, `using static` targets) is ambient and never flagged.
10. **Codemod exports everything existing** (A3): privacy (no `export` = `internal` + strict-invisible) is
    opt-in going forward. Zero-inbound-edge roots (screens mounted from hand C#) must keep working.
11. **HMR semantics (Fast Refresh parity)**: component-only file edit → in-place refresh, state preserved;
    hook-signature change → remount consumers; non-component export edited → propagate up the import graph to
    nearest component importers; escapes the root → full-rebuild notice.

## 2. Family grammar (SHAPE-identical; Unity casing = PascalCase-ish `useX` hooks per repo idiom)

```
import { StatusChip } from "./components/StatusChip"
import { useCounter, CounterStyles } from "~/Shared/Counter.hooks"

export component Screen(int Start = 0) { ...setup... return (<Box>...</Box>); }
export hook useCounter(int start) { ... }
export module CounterStyles { ... }
component LocalHelper { ... }        // no export = file-private (strict-invisible, internal)
```

- Imports: static, string-literal specifiers, PREAMBLE ONLY (with `@namespace`/`@using`/`@uss`), before the
  first declaration. Trivially regex-scannable (all six family implementations depend on this).
- A file = a SEQUENCE of declarations, any kinds, any order, each optionally `export`-prefixed.
- One-component-per-file / hooks-in-`.hooks`-files / filename==component become LINT tier (UITKX2313).
- Non-exported declarations are unreachable cross-file (UITKX2301 on import attempt).

## 3. Family diagnostics contract — block UITKX2300–2315

- PREREQUISITE (A8a) — code-block audit: DONE for this leg 2026-07-11. Occupied in THIS repo:
  0000–0026 (sparse), 0101–0150, 0200–0211, 0300–0306, 2100–2106, 2200–2210. Godot: +2504–2508, 3000–3006.
  Unreal: +2506–2508. **23xx is free in all three repos** → family block = **2300–2315** (confirmed by
  leg 1's shipped M0.2 audit). Re-verification is DONE: the frozen canonical table is recorded in the
  CONTRACT FREEZE section above — write `DiagnosticCodes.cs` from it, not from memory.
- Registries to update in lockstep: `ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs`,
  `DiagnosticsAnalyzer.cs`, SG `Diagnostics/`, LSP `DiagnosticsPublisher.cs`, docs Diagnostics page.
- Severity policy: codes + messages identical family-wide; severities may diverge ONLY where the table's
  per-repo column says so (e.g. single-decl lint here vs hard error in Godot).

| Code | Sev (Unity) | Message template (2300–2309 FROZEN by leg 1 — identical family-wide modulo prefix/extension) |
|---|---|---|
| UITKX2300 | Error | ``unknown import specifier `{0}` — no file at {1}(.uitkx)`` (also fires for engine-native specifiers — they never resolve) |
| UITKX2301 | Error | `` `{0}` is not exported by {1} — add `export` to its declaration `` |
| UITKX2302 | Error | `` `{0}` is not declared in {1} `` |
| UITKX2303 | Error | ``duplicate import of `{0}` (already imported from {1})`` |
| UITKX2304 | Warning | ``unused import `{0}` `` |
| UITKX2305 | Error | `` `{0}` is defined in {1} but not imported — add: import {{ {0} }} from "{2}" `` |
| UITKX2306 | Error | ``value-import cycle: {0} (hooks/modules load eagerly — break the chain or move to component refs)`` ({0} = chain `A.uitkx -> B.hooks.uitkx -> A.uitkx`) |
| UITKX2307 | Error | `` `{0}` is used like a uitkx component/hook but no file exports it `` (export-table names only; ambient C# exempt) |
| UITKX2308 | Error | ``import crosses a module/root boundary ({0} -> {1}) — imports are module-scoped in v1`` (Unity boundary = asmdef; family code) |
| UITKX2309 | Error | ``import must appear in the preamble, before the first declaration`` |
| UITKX2310 | Error | `Cannot derive a namespace for '{0}' (no owning .asmdef); add @namespace.` (Unity-local — register in the canonical table at leg-3 start) |
| UITKX2311 | Error | `Export accessibility mismatch across parts merging into '{0}'.` (Unity-local — register) |
| UITKX2312 | Error | `Hook container '{0}' merge conflict between '{1}' and '{2}' (duplicate hook / accessibility).` (Unity-local — register) |
| UITKX2313 | Warning (lint) | `Convention: {0}.` (multi-component file; hooks outside .hooks; filename mismatch) (Unity-local — register; reconcile with the frozen 2105 retarget, see CONTRACT FREEZE) |
| UITKX2314 | Error | `'~/' root is not configured or resolves outside the project ('{0}').` (Unity-local — register) |
| UITKX2315 | — | reserved (family) |

- UITKX0103 (filename mismatch, hard) is DEMOTED into 2313 lint (mixed-decl makes it meaningless);
  UITKX0120 (asset not found) keeps firing for `~/` asset paths after resolution.

## 4. Path-derived default namespaces (round-3 decision, replaces InferFunctionStyleNamespace)

- Rule: `EffectiveNamespace(file) = @namespace if present else "ReactiveUITK.Uitkx." + Join('.', Sanitize(seg) for seg in DirSegments(fileDir relative to owning-asmdef dir))`.
  File name EXCLUDED (files in one folder share a namespace). File directly beside the asmdef →
  `ReactiveUITK.Uitkx` alone. Owning asmdef found by the existing walk-up
  (`UitkxPipeline.FindOwningAsmdefAssemblyName`; Editor mirror `Editor/HMR/AsmdefResolver.cs`; LSP mirror
  `ide-extensions~/lsp-server/AsmdefResolver.cs` — all three already parity-tested by
  `AsmdefResolverParityTests.cs`; extend that test to namespace derivation). No asmdef → UITKX2310.
- Sanitization (pin EXACTLY, mirrored in C# + the LSP/TS side): per segment — keep `[A-Za-z0-9_]`, every
  other char → `_`; leading digit → prefix `_`; empty → `_`; C# keyword → prefix `_`; case preserved
  verbatim. Example: `Samples/Components/Tic Tac-Toe/Board.uitkx` (asmdef at `Samples/`) →
  `ReactiveUITK.Uitkx.Components.Tic_Tac_Toe`.
- `@namespace` = interop override ONLY (files consumed by hand-written C#; path identity makes file moves
  breaking for native consumers — markup consumers are codemod/IDE-fixed). `HasExplicitNamespace` stays.
- DELETE `InferFunctionStyleNamespace` + `s_namespaceRegex` + `FunctionStyleDefaultNamespace`
  (`DirectiveParser.cs:27-31, 1067-1089`) and the dead `"ReactiveUITK.Generated"` fallbacks
  (`HookEmitter.cs:~84`, `ModuleEmitter.cs:~82`, CSharpEmitter equivalent). HMR's
  `HookContainerRegistry.cs:184-186` "refuse without explicit @namespace" branch → replaced by derivation.
- Implement derivation ONCE in language-lib (new `ide-extensions~/language-lib/NamespaceDerivation.cs`),
  consumed by SG + LSP; HMR gets a byte-for-byte mirror (cannot reference language-lib —
  `AssetPathUtil.cs:22-26` doc) pinned by a new parity contract test.

## 5. Mixed-decl v1 — the data-model rewrite (A1, round-3 full scope)

`ide-extensions~/language-lib/Parser/ParseResult.cs` (shared source: SG csproj `ProjectReference`s
language-lib — one edit serves both):

- NEW `public sealed record ImportDeclaration(ImmutableArray<string> Names, string Specifier, int Line, int Column, ImmutableArray<int> NameColumns)`.
- NEW `public sealed record ComponentDeclaration(string Name, bool IsExported, ImmutableArray<FunctionParam> FunctionParams, string? FunctionSetupCode, int FunctionSetupStartLine, int FunctionSetupStartOffset, int MarkupStartLine, int MarkupStartIndex, int MarkupEndIndex, int DeclarationLine, int NameColumn, int ReturnEndLine, int BodyEndLine)` — i.e. every singular
  component field currently on `DirectiveSet` (`ComponentName`, `FunctionParams`, `FunctionSetupCode`,
  `MarkupStart*`, `ComponentDeclarationLine`, `ComponentNameColumn`, `FunctionReturnEndLine`,
  `FunctionBodyEndLine`, `PropsTypeName`, `DefaultKey`) moves INTO the per-decl record.
- `DirectiveSet` gains `ImmutableArray<ImportDeclaration> Imports` and
  `ImmutableArray<ComponentDeclaration> ComponentDeclarations` (PLURAL — supersedes `ComponentName`).
  Keep `ComponentName => ComponentDeclarations.FirstOrDefault()?.Name` as a computed compat property during
  the migration; delete all singular reads by PR end (grep gate: `grep -rn "\.ComponentName" SourceGenerator~ ide-extensions~ Editor` returns only the record definition).
- `HookDeclaration` / `ModuleDeclaration` gain `bool IsExported`.

`DirectiveParser.cs`:

- `s_topLevelKeywords` (`:33-46`): add `"import"`, `"export"`, `"from"`.
- Preamble loop (order-agnostic, before first decl): parse `import { A, B } from "spec"` lines into
  `DirectiveSet.Imports`. Scan-side family diags here per the frozen split: 2303 (duplicate import)
  + 2309 (import after the first decl — imports are preamble-only). Engine-native specifiers get NO
  dedicated code: they fail resolution → 2300 (frozen; the draft's parse-time 2308 is gone).
- REPLACE the first-keyword dispatch (`:202-215`) with a DECLARATION LOOP: at each top-level cursor position
  read optional `export`, then `component|hook|module`, parse that decl fully (component parse captures its
  own setup/markup span), repeat until EOF. Delete the hook/module-file "Invalid top-level statement"
  trailing-content error (`:542-556`). Anything that is not a decl start = a parse error on the
  existing invalid-top-level path (NOT 2307 — frozen 2307 is the strict no-file-exports-it diagnostic).
- LIFT the `@uss`-forbidden-in-hook/module-files ban (`:507-517`): `@uss` legal in any file; sheets attach to
  every component declared in the file (no components → 2313 lint "uss without component").

## 6. Resolution, export tables, three injection forms (A7a)

- NEW `SourceGenerator~/ImportResolver.cs` (linked into language-lib or placed there and shared): specifier →
  absolute file. Relative: against importer dir. `~/`: against config root (§9 walk-up). Extensionless →
  append `.uitkx`; missing file = 2300 (engine-native specifiers also land here — they never resolve);
  ordinal-case-sensitive compare on the project-relative path.
  Owning-asmdef equality check → 2308 (frozen family boundary code).
- Peer tables (`UitkxGenerator.cs:169-244` pre-scan): keep the assembly-wide scan (adversarial REFUTED the
  O(edges) claim — the "did you mean" text of 2305 NEEDS the full export table; restate the win as scoped
  injection + HMR invalidation). Extend `PeerComponentInfo.cs` / `PeerHookContainerInfo.cs` with
  `IsExported`, `SourceFilePath`, `DerivedNamespace`; ADD `PeerModuleInfo.cs` (none exists today — module
  edges currently resolve via ambient C# visibility only) + `TryBuildPeerModuleInfo` beside
  `TryBuildPeerComponentInfo`/`TryBuildPeerHookContainerInfo` (`UitkxGenerator.cs:181-183, 354-401`).
- Injection per import kind, in `UitkxPipeline.cs`:
  1. **component** → NO using line. Bind name→(namespace,file) into the peer-component table handed to
     `PropsResolver` — tag lowering already emits the FQN (`PropsResolver` `SourceQualifiedTypeName`).
     Unimported peer components are REMOVED from the table (that IS strictness for tags → 2305).
  2. **hook** → `using static {EffectiveNamespace}.{Container};` for the CONTAINER owning the imported hook.
     DELETE the unconditional all-containers injection (`UitkxPipeline.cs:229-254`). C# has no per-method
     static import, so the whole container is exposed to C# — per-NAME strictness stays a uitkx diagnostic
     (2305/2307 from the setup-code scan).
  3. **module** → alias `using {X} = {EffectiveNamespace}.{X};` — NEVER `using {Namespace};` (namespace-using
     resurrects implicit cross-file refs).
- Strict detector (A7: syntactic name-table match, NOT semantic): over markup tags + setup/attr expressions:
  (a) tag not builtin (`ElementRegistry` + `Shared/Core/Router/RouterTagAliases.cs`) and in the component
  export table but unimported → 2305; in no table → existing unknown-tag path. (b) `\buse[A-Z_]\w*\s*\(`
  calls minus builtin `HookRegistry` names: in hook export table but unimported → 2305; in no table AND not
  resolvable ambiently → 2307 only if the name appears in NO table (hand-written C# hooks exempt — A4).
  (c) `\b{ModuleName}\s*\.` member access for names in the module export table but unimported → 2305.
  (d) `global::`-qualified references bypass the detector — document as a legal interop escape (owner Q
  default: allowed, lint later).
- Export → accessibility: `export` = `public`, non-export = `internal` on the generated type. CS0262 rule
  (A7c): any hook-container or module part that MERGES into another generated/hand-written partial
  (module named after a component — `ModuleEmitter.cs:14-18`; containers shared across `Foo.hooks.uitkx` /
  `Foo.styles.uitkx`) emits **NO accessibility modifier**; if the contributing files disagree on exportedness
  → UITKX2311. Change `ModuleEmitter.cs:118` and `HookEmitter.cs:94` accordingly. Note in docs: `internal`
  fences asmdefs only; uitkx strict diagnostics are the real privacy fence.
- Value-cycle check: DFS over hook/module import edges per asmdef in the SG pre-scan → 2306 with chain.
  Component-only cycles: legal, no work.

### Emitted-code shape (before → after), `Samples/Components/Counter/Counter.uitkx`

Before (today):
```csharp
namespace <regex-read from companion .cs, else ReactiveUITK.FunctionStyle>
{
    using static NsA.TicTacToeHooks;    // EVERY container in the asmdef, unconditionally
    using static NsB.TodoHooks;         // ← CS0121 ambiguity class
    public partial class Counter { ... }
}
```
After (source gains `import { useCounter } from "./Counter.hooks"` + `import { AppStyles } from "~/Styles/App"` + `export component Counter`):
```csharp
namespace ReactiveUITK.Uitkx.Components.Counter                       // path-derived (or @namespace)
{
    using static ReactiveUITK.Uitkx.Components.Counter.CounterHooks;  // per-import only
    using AppStyles = ReactiveUITK.Uitkx.Styles.App.AppStyles;        // module alias, never namespace-using
    public partial class Counter { ... }                              // export→public, else internal
}
```

## 7. Mixed-decl emit + hook family keys

- `UitkxPipeline.cs`: delete the `ComponentName == null` hook/module short-circuit (`:58-66`). New flow per
  file: `HookEmitter.Emit` (if hooks) + `ModuleEmitter.Emit` (if modules) + `CSharpEmitter.Emit` ONCE PER
  `ComponentDeclaration` → concatenate into ONE `.g.cs` (extend the existing hook+module concat at `:97`).
  hintName unchanged (per file).
- Container naming stays `HookEmitter.DeriveContainerClassName(filePath)` (`:378-392`, dot-suffix strip).
  Collision rule (round-3): hooks inside `Foo.uitkx` AND `Foo.hooks.uitkx` both derive `FooHooks` → the
  parts MERGE (both emit `public static partial class` / no-modifier per §6) IFF EffectiveNamespace is
  identical AND exportedness matches AND no duplicate hook name across parts; otherwise UITKX2312.
- Multi-component files: one partial class per component; duplicate component name within one file = error
  (reuse 0104-class duplicate); filename match demoted to 2313 lint.
- **Hook family keys become path-identity qualified** (same migration as namespaces): `HookEmitter.cs:146`
  `familyKey = hook.Name` → `familyKey = $"{EffectiveNamespace}.{Container}::{hook.Name}"`. Source-derived on
  all three sides with zero file reads (namespace now never comes from a `.cs`): hook side knows ns+container
  locally; consumer side (`CSharpEmitter.ExtractCustomHookFamilyKeys`, `:735`, called at `:242`) maps each
  called hook name → its imported container via the file's `Imports`; `HmrHookEmitter` mirrors byte-for-byte.
  `RefreshRuntime` (`Shared/Core/Refresh/RefreshRuntime.cs`) unchanged — keys are opaque Ordinal strings.
- **Component Register ids qualified in the SAME migration**: simple-type-name ids (documented collision at
  `RefreshRuntime.cs:~170-172`) → `$"{EffectiveNamespace}.{ComponentName}"` in both CSharpEmitter and
  HmrCSharpEmitter. One-time invalidation of in-flight editor HMR sessions — editor-only, documented in
  changelog. File move = documented one-time HMR remount (path identity).

## 8. HMR (all mirrors enumerated — A7g; parity contract extended FIRST per GATE-3)

- NEW `Editor/HMR/UitkxImportGraph.cs`: regex pre-scan of import lines (same discipline as
  `HookContainerRegistry`) over all `.uitkx`; forward + reverse edges; updated per file event
  (`UitkxHmrFileWatcher` / `UitkxHmrAssetPostprocessor`). Source is the graph truth (A2) — no sidecars exist
  in this repo; nothing to migrate.
- `Editor/HMR/HookContainerRegistry.cs`: `s_hookRegex` `^\s*hook\s+` → `^\s*(?:export\s+)?hook\s+` (`:48-50`);
  keep `s_nsRegex`; namespace now = derivation mirror when `@namespace` absent (`:184-186` refusal branch
  deleted); registry keyed by (path, container).
- `Editor/HMR/UitkxHmrCompiler.cs`: `_cachedCompilations` (`:154`, writes `:2370`) gains reverse-edge
  invalidation — editing B invalidates every importer A's cached Compilation (today: never — the confirmed
  correctness gap). Per-asmdef metadata-ref filtering unchanged (imports are asmdef-scoped).
- `Editor/HMR/UitkxHmrController.cs`:
  - DELETE filename-convention companion redirect `ResolveParentComponentFile` (`:338, :432-449`) —
    companions become ordinary imported files compiled standalone.
  - KEEP the same-folder `<Base>*.cs` hand-written-companion scan (`:456-481`) — ambient C# is outside the
    import graph (A4).
  - REPLACE CS0103-regex dependency discovery (`:727-732, :927-990`) with graph resolution.
  - Propagation per §1.11: component edit → in-place refresh; hook-signature change → remount importers
    (family indirection already reaches consumers, `:340-346`); module/non-component export edit → walk
    reverse edges to nearest component importers and refresh them; root escape → rebuild notice.
- `Editor/HMR/HmrCSharpEmitter.cs` + `HmrHookEmitter.cs`: mirror EVERY §6/§7 emit change (injection forms,
  accessibility, concat order, family keys, Register ids, `#line` — already shipped, keep).
- `HmrAssetPathUtil` (nested in `UitkxHmrController.cs:850-893`): §9 `~/` + walk-up mirror.
- Other syntactic mirrors that must learn `import`/`export` tokens: `Editor/HMR/NewCsFileDiscovery.cs`
  (check for `.uitkx` regexes), `UitkxChangeWatcher.cs` / `UITKX_GeneratorTrigger.cs` (path triggers only —
  verify no grammar regexes), `Editor/UitkxCsprojPostprocessor.cs` (none expected — verify by grep
  `grep -rn "hook\\\\s\|component\\\\s\|module\\\\s" Editor/`).

## 9. `~/` in asset references + config walk-up (round-3 decision 6)

- `uitkx.config.json` gains key `"root"` (project-relative dir, default `"Assets"`). Walk-up =
  `ide-extensions~/language-lib/Formatter/ConfigLoader.cs` (nearest-config-WINS, NO merge — same wording as
  Unreal A5g: a formatter-only config in a subdir SHADOWS an ancestor's `"root"`; document this). Generalize
  `ConfigLoader` out of `Formatter/` (move to `language-lib/ConfigLoader.cs`, keep namespace) so the
  resolver + analyzer + formatter share one loader.
- `ide-extensions~/language-lib/AssetPathUtil.cs` `ResolveAssetPath` (`:35-56`): new branch — `rawPath`
  starting `~/` resolves against the configured root; escape above project root → UITKX2314. Existing
  `Assets/`/`Packages/`-rooted and relative behavior unchanged (asset strings still accept `Assets/` — the
  import-specifier ban does NOT apply to asset positions; document the asymmetry).
- Mirrors (byte-for-byte, each pinned by contract test): `HmrAssetPathUtil.ResolveAssetPath`
  (`UitkxHmrController.cs:852`) + its own minimal walk-up (HMR cannot reference language-lib);
  `DiagnosticsAnalyzer.ResolveAssetPath` (`DiagnosticsAnalyzer.cs:~1415`) already delegates to AssetPathUtil
  → automatic; `CSharpEmitter.cs:~3880-3911` (`Asset<T>`/`@uss` lowering + UITKX0120) already delegates →
  automatic. Codemod moves NO files, so no asset-path rewriting in v1.

## 10. LSP / IDE tooling

- `ide-extensions~/lsp-server/WorkspaceIndex.cs`: index exports (name→file→kind→EffectiveNamespace) +
  import graph; component regex (`s_uitkxComponentPattern`, `:140`) + hook/module patterns gain optional
  `export\s+` prefix; index becomes export-aware for cross-file suggestions.
- `DefinitionHandler.cs`: go-to-def on an imported name → target decl; on a specifier string → target file.
- `CompletionHandler.cs`: inside `import { … }` → target's export list; inside specifier string → path
  completion (relative + `~/`); auto-import quick-fix for 2305 (insert the import line the message names).
- `DiagnosticsPublisher.cs` + `DiagnosticsAnalyzer.cs`: surface 2300–2314 live.
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`: PARITY with §6/§7 emission — imports
  lower to the same using-static/alias lines; multi-decl files produce one virtual doc containing all
  containers/partials; length-preserving source map unaffected (imports map to hidden preamble).
- `ide-extensions~/language-lib/Formatter/AstFormatter.cs` — **CRITICAL (A7f): the formatter re-prints the
  preamble from DirectiveSet fields ONLY and silently DELETES unmodeled lines** (component path `:123-154`,
  hook/module path `:304-332`). Both paths MUST re-emit `DirectiveSet.Imports` FIRST (order: LeadingTrivia,
  imports, `@namespace`, `@using`, `@uss`), and re-emit `export ` prefixes on decls. Round-trip corpus cases
  mandatory before the codemod ships (a format pass on a codemodded file must be byte-stable).
- `ide-extensions~/language-lib/SemanticTokens/`: token types for `import`/`export`/`from` + specifier string.
- `ide-extensions~/grammar/uitkx.tmLanguage.json`: import/export/from keywords + specifier scopes (vscode
  prebuild copies it; VS2022/rider consume the same grammar).
- Version-skew tolerance (critic): new syntax is additive, so a NEW extension on a pre-import project shows
  nothing; an OLD extension red-squiggles imports → same-window release rule: publish vscode/vs2022/rider
  updates the same day the package ships (§13).

## 11. Codemod — `UitkxMigrateImports` (load-bearing: leg's first green milestone)

- Location: NEW console project `SourceGenerator~/Tools/UitkxMigrateImports/` (net8.0, references
  language-lib). Run: `dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- <repo Assets dir>`.
- Inputs: all 163 `Samples/**/*.uitkx` + any other `.uitkx` under `Assets/`, grouped by owning asmdef.
- Pass 1 — export tables (per asmdef): parse every file with the NEW parser; record EVERY top-level decl as
  exported (export-everything default, A3): name → (file, kind, container, EffectiveNamespace).
- Pass 2 — reference scan per file (A3: a NEW scan, not table reuse): (a) markup tags matching the component
  table minus builtins; (b) setup/attr code `\buse[A-Z_]\w*\s*\(` minus `HookRegistry` builtins matching the
  hook table; (c) `\b{Module}\s*\.` matching the module table; skip string literals/comments via
  `CSharpLexFacts`. Names matching >1 exporter: disambiguate replicating `BuildSearchNamespaces` order
  (`CSharpEmitter.cs:1295-1310` — own ns, then `@using` order); still ambiguous → codemod ERROR, manual fix.
- Writes, per file: (1) prepend `export ` to every top-level decl; (2) insert import lines at top of
  preamble after leading comment trivia, one line per target file, names sorted, specifier = `./`-relative
  from importer dir (never `~/` in codemod output); (3) **stamp the CURRENT effective namespace as explicit
  `@namespace`** into every file where `HasExplicitNamespace == false` (freezes today's companion-.cs-derived
  identity → zero behavioral change for existing C# consumers and existing HMR keys' namespaces; path-derived
  default applies to NEW files only).
- Idempotent + re-runnable (critic: rebase tool, not one-shot): skips decls already `export`ed, merges into
  existing import lines, deterministic ordering; running twice = no diff.
- Acceptance: after codemod — `dotnet test` both suites green; SG produces ZERO UITKX23xx over Samples;
  format every migrated file (`FormattingHandler`/AstFormatter) → byte-stable; Unity project compiles.

## 12. Analyzer-DLL rebuild + intra-PR commit sequencing (A7h) — COMMITTED-OUTPUT FLAGS

Order of commits inside `feat/uitkx-imports` (each step names committed generated output it changes):

1. Family corpus mirror + hash manifest (§14) — new committed fixtures. [FLAG: new committed corpus]
2. Parity contracts extended (`HmrEmitterParityContractTests`, AsmdefResolver→namespace parity, asset-path
   mirror contract) — red until later steps.
3. Data model + parser + formatter + resolver + emitters + strict diagnostics + HMR + LSP (the code of
   §4–§10), with strictness behind an internal flag DEFAULT OFF.
4. **Rebuild + recommit `Analyzers/ReactiveUITK.SourceGenerator.dll`, `ReactiveUITK.Language.dll` (+ .pdb,
   deps.json) via `scripts/build-generator.ps1`** — the codemod's output is unparseable by the OLD committed
   DLL; this commit MUST precede the codemod commit. [FLAG: committed analyzer DLLs]
5. Run codemod over `Samples/` (+ any stray `.uitkx`); commit migrated sources. [FLAG: mass source diff,
   163 files]
6. Flip strictness ON (delete the flag); supersede pinned tests in the SAME commit:
   `HookUsingStaticTest.cs`, `HookUsingStaticAsmdefBoundaryTest.cs`, `HookUsingStaticCrossNamespaceTest.cs`
   → replaced by `ImportScopedHookInjectionTests.cs` (+ boundary/cross-ns variants asserting the new forms).
7. **Rebuild + recommit Analyzers DLLs AGAIN** (strict-on build). [FLAG: committed analyzer DLLs]
8. Regenerate `GeneratedPreview~/Test.uitkx.g.cs`. [FLAG: committed generated preview]
9. Re-pin `SourceGenerator~/Tests/Golden/` + `FormatterSnapshotTests` snapshots. [FLAG: committed goldens]
10. Docs + changelogs + version bumps (§13).

## 13. Docs site, changelogs, versions (A8c, critic)

- `ReactiveUIToolKitDocs~/src/pages/UITKX/`: NEW `Imports/` page (grammar, strictness, `~/`, privacy,
  migration guide incl. codemod command); REWRITE `CompanionFiles/` (file-kind rules → lint-tier conventions;
  mixed-decl files); UPDATE `Config/UitkxConfigPage.tsx` (`"root"` key + shadowing rule),
  `Diagnostics/` (2300–2315 table), `Concepts/`, `GettingStarted/`, `Hooks/` (family-key note),
  `Assets/AssetsPage.tsx` (`~/` in asset strings). Build: `cd ReactiveUIToolKitDocs~ && npm run build`.
- `CHANGELOG.md`: minor entry (new syntax) — package `0.6.x → 0.7.0` in `package.json` (SemVer minor: new
  functionality; codemod keeps existing code working; the one-time HMR key invalidation is editor-only,
  document it). Use `scripts/changelog.mjs`.
- `ide-extensions~/changelog.json`: new entry; vscode/vs2022 `1.2.x → 1.3.0`, rider bump; SAME-WINDOW release
  with the package (skew rule §10). Republish `.vsix`/marketplace via existing `scripts/publish-*.ps1` flow.
- `Plans~/DISCORD_CHANGELOG.md` staging note per house process. No commits/publishes without owner ask.

## 14. Corpus mirroring prerequisite (TD-009/TD-018 — A8b)

- The mirror mechanism SHIPPED in leg 1 (TD-009: "mechanism shipped leg 1; sibling PRs pending").
  Exact source paths, the familyCore/perLeg tier partition, and the pinned family hash are in the
  CONTRACT FREEZE section at the top of this file. This leg CONSUMES it:
  - Mirror the frozen family-core corpus (`uetkx-scanner-cases.json` `_tiers.familyCore` — markup-shape
    cases, byte-identical after `UETKX→UITKX` substitution) into
    `SourceGenerator~/Tests/Corpus/ImportExport/` + per-leg Unity-cased declaration cases beside it
    (`useX` hooks — the family grammar is SHAPE-identical, not byte-identical).
  - NEW test `ImportCorpusManifestTests.cs`: SHA-256 manifest of family-core files must equal the manifest
    committed in the Unreal repo (`scripts/corpus-hash.mjs` + `plans/family-corpus.hash` — adopt the same
    normalization: `UETKX|GUITKX|UITKX → TKX`; checked-in copy of the hash; drift = red).

## 15. Test matrix + verify commands

New/updated suites (all under `SourceGenerator~/Tests/` unless noted):

| Suite | Content |
|---|---|
| `ParserTests.cs` (+corpus) | import forms, export prefixes, mixed-decl sequences, preamble ordering, 2303 + 2309 at scan, engine-native specifier → 2300 |
| NEW `ImportResolutionTests.cs` | relative/`~/` resolve, every frozen 2300–2309 (mirror `ReactiveUIUetkxResolveTest.cpp`'s case list) + 2314, value-cycle 2306 chain, component-cycle-legal pair (CycleProof mirror) |
| NEW `NamespaceDerivationTests.cs` | §4 sanitization table, @namespace override, 2310 |
| NEW `StrictModeTests.cs` | 2305 message names exact import line; 2307; ambient-C# exemption; builtin allowlists |
| `EmitterTests.cs` | three injection forms; export→public/internal; no-modifier merge (2311); concat emit; multi-component; container merge/2312 |
| NEW `ImportScopedHookInjectionTests.cs` (+Boundary/CrossNs) | supersedes the three `HookUsingStatic*` tests (same commit as strict flip) |
| `HmrEmitterParityContractTests.cs` | extended FIRST; family keys, Register ids, injections, mixed emit |
| NEW `AssetPathMirrorContractTests.cs` | AssetPathUtil vs HmrAssetPathUtil `~/` byte-parity |
| `AsmdefResolverParityTests.cs` | + namespace-derivation parity across SG/HMR/LSP |
| `FormatterTests.cs` / `FormatterSnapshotTests.cs` | imports survive round-trip; ordering; export prefixes; byte-stability post-codemod |
| `DiagnosticTests.cs` / `DiagnosticsAnalyzerTests.cs` | 2300–2314 registry + live-analyzer parity |
| `VirtualDocumentTests.cs` | imports→usings mapping; multi-decl virtual docs; source-map integrity |
| `HookRegistryTests.cs` + `Golden/HookRegistry` | re-pin if hook metadata surface changed [FLAG goldens] |
| NEW `CodemodTests.cs` | fixture tree in/out; idempotence (run twice = no diff); ambiguity error path |
| `StaleAdditionalTextTests.cs` | disk-authoritative re-read still correct with imports |
| lsp-server `Tests/` | WorkspaceIndex exports/graph, definition/completion on imports, 23xx publishing |
| NEW `ImportCorpusManifestTests.cs` | §14 hash gate |

Verify commands (run after every milestone; all must pass before PR):

```bash
dotnet test SourceGenerator~/Tests/ReactiveUITK.SourceGenerator.Tests.csproj
dotnet test ide-extensions~/lsp-server/Tests/UitkxLanguageServer.Tests.csproj
scripts/build-generator.ps1                                # then confirm Analyzers/ diff is intentional
dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- Samples   # idempotence: run twice, git diff empty
cd ide-extensions~/vscode && npm ci && npm run build
ide-extensions~/visual-studio/build-local.ps1
cd ReactiveUIToolKitDocs~ && npm run build
# Unity in-editor: open project, confirm Samples compile + HMR hot-swap on a hook edit propagates via graph
```

## 16. Risks

- Codemod is load-bearing (strict day one): zero-23xx tree over Samples is the PR's first hard milestone.
- Hook-key + Register-id migration invalidates in-flight editor HMR sessions once (editor-only; changelog).
- Formatter data-loss (A7f) is the sharpest edge: land `DirectiveSet.Imports` + AstFormatter re-emit BEFORE
  anyone formats a migrated file; round-trip test is non-negotiable.
- Old committed DLL vs new syntax: the two-rebuild sequence (§12 steps 4/7) is order-critical on fresh clones.
- `~/` walk-up shadowing (nearest-wins, no merge) can silently retarget roots in nested-config setups —
  documented in Config docs; 2314 catches escapes.
