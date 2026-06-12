# Phase 1 & Phase 2 — JSX-in-Expression Unification

> **Status:** Research / Design
> **Date:** 2026-05-08
> **Scope:** Source generator, HMR, IDE language-lib, formatter, samples, docs
> **Breaking changes:** None for Phase 1. Phase 2 is cosmetic-only (zero existing samples affected).
> **Owner:** TBD

---

## 0. TL;DR

uitkx supports JSX literals (`<Tag/>`) inside C# expressions in only **two of six** syntactic positions today (component preamble, directive bodies). Inside child `{expr}`, child `@(expr)`, attribute `attr={expr}` (when not a bare literal), and inside lambdas/ternaries anywhere, JSX literals are emitted **verbatim** to Roslyn and break.

Empirical proof (probed via `GeneratorTestHelper.Run`):

| Input | Emit | Result |
|---|---|---|
| `<Box>{el}</Box>` (`el` is `var`) | `(el)` | ✅ |
| `<Box>@(el)</Box>` (same) | `(el)` | ✅ (identical to `{el}`) |
| `<Box>{cond ? <A/> : null}</Box>` | `(cond ? <A/> : null)` | ❌ Roslyn |
| `<Box>@(cond ? <A/> : null)</Box>` | `(cond ? <A/> : null)` | ❌ Roslyn |
| `<Box icon={cond ? <A/> : null}/>` | `__p_0.Icon = cond ? <A/> : null;` | ❌ Roslyn |
| `<Box renderRow={item => <Label text={item}/>}/>` | `__p_0.RenderRow = item => <Label text={item}/>;` | ❌ Roslyn |
| `<Box icon={<Label/>}/>` | `__p_0.Icon = V.Label(__p_1, key: null);` | ✅ (opt-in `IsJsxInBraces` path) |

**Phase 1** wires the existing `DirectiveParser.FindBareJsxRanges` + `FindJsxBlockRanges` scanners (already proven in preamble & directive bodies) into the emit path for `CSharpExpressionValue` and `ExpressionNode`. Result: every position becomes JSX-aware uniformly, matching Babel's "JSX is recursively allowed wherever an expression is allowed" model.

**Phase 2** collapses `@(expr)` and child-`{expr}` into a single canonical form: bare `{expr}` (Option A — React parity). Statement-level directives `@if/@for/@foreach/@while/@switch` are **untouched**. Zero samples currently use `@(cond ? ...)` so migration is mechanical.

Combined footprint: ~390 LOC added, ~120 modified, 11–14 files. Zero runtime change. Zero parser-grammar change for Phase 1.

---

## 1. The six expression positions

For reference, here are every place a C# expression can appear in uitkx, with the path each one currently takes:

| # | Position | Example | Current path | JSX-aware? |
|---|---|---|---|---|
| 1 | Component preamble | `var x = (<Tag/>);` between `{` and `return (` | `FindBareJsxRanges` + `FindJsxBlockRanges` splice | ✅ |
| 2 | Directive body | `@if (...) { ... <Tag/> ... }` | Same scanners | ✅ |
| 3 | Child `@(expr)` | `<Box>@(cond ? <A/> : <B/>)</Box>` | `MarkupTokenizer.ReadParenExpressionWithOffset` → `ExpressionNode` → `EmitExpressionNode` emits `(expr)` literally | ❌ |
| 4 | Child `{expr}` | `<Box>{cond ? <A/> : <B/>}</Box>` | `MarkupTokenizer.ReadBraceExpressionWithOffset` → `ExpressionNode` → same | ❌ |
| 5 | Attribute C# `attr={expr}` | `prop={cond ? <A/> : null}` | `ParseAttributes` → `CSharpExpressionValue` → `AttrVal` → `TransformExpression` (no JSX rewrite) | ❌ |
| 6 | Attribute JSX-literal `attr={<Tag/>}` | `prop={<Foo/>}` | `IsJsxInBraces()` → `JsxExpressionValue` → `EmitJsxToString` | ✅ (entire value must be one literal) |

The asymmetry is **historical, not principled.** Each context was wired up as someone needed it.

---

## 2. Inventory

### 2.1 Parser ([ide-extensions~/language-lib/Parser](ide-extensions~/language-lib/Parser/))

- [DirectiveParser.cs](ide-extensions~/language-lib/Parser/DirectiveParser.cs) — `FindBareJsxRanges` (~line 2289) and `FindJsxBlockRanges` (~line 2150). The two scanners that disambiguate JSX from `<` operators inside arbitrary C#. **Currently called from preamble + directive-body parsing only.**
- [UitkxParser.cs](ide-extensions~/language-lib/Parser/UitkxParser.cs) — markup tree builder.
  - Line 482–497: child `{expr}` → `ExpressionNode`.
  - Line 513–526: child `@(expr)` → `ExpressionNode` (same node type).
  - Line 803–836: attribute value path. Line 804 `IsJsxInBraces()` gate; line 828 `CSharpExpressionValue` fallback.
  - Line 1371–1382: `IsJsxInBraces()` — narrow gate (first non-whitespace char must be `<` then letter or `>`).
- [MarkupTokenizer.cs](ide-extensions~/language-lib/Parser/MarkupTokenizer.cs) — line 229 `ReadBraceExpressionWithOffset`, line 258 `ReadParenExpressionWithOffset`. Both call into `ExpressionExtractor`.
- [ExpressionExtractor.cs](ide-extensions~/language-lib/Parser/ExpressionExtractor.cs) — `FindMatchingClose`. The workhorse paren/brace balancer; handles regular/verbatim/interpolated strings, char literals, line/block comments, nested delimiters. **Robust and reused everywhere.**

### 2.2 Source generator ([SourceGenerator~/Emitter](SourceGenerator~/Emitter/))

- [CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs)
  - Line 1577 `EmitExpressionNode` — emits `(ex.Expression)` raw.
  - Line 1707–1729 `AttrVal` — switches on `AttributeValue` subtypes; line 1726 `CSharpExpressionValue` → `TransformExpression(cev.Expression)`; line 1727 `JsxExpressionValue` → `EmitJsxToString`.
  - Line 1753 `EmitJsxToString` — proven JSX-tree → C# emitter; reused by Phase 1 splice.
  - Line 1756–1880 `SpliceSetupCodeMarkup` — **the existing pattern Phase 1 mirrors.** Extracts JSX ranges from a stretch of C# source, parses each, emits `V.Tag(...)`, splices back. Uses `_directives.SetupCodeMarkupRanges` + `SetupCodeBareJsxRanges` (the preamble's pre-computed ranges).
  - Line 1882–2000 `SpliceBodyCodeMarkup` — same pattern for directive bodies.
  - Line 3019 `TransformExpression` — currently does setter-lambda sugar + `Asset<T>` path resolution **only**; no JSX rewriting.
- [HookEmitter.cs](SourceGenerator~/Emitter/HookEmitter.cs) — mirrors `CSharpEmitter` for hook render bodies. Same fix needed.
- [ModuleEmitter.cs](SourceGenerator~/Emitter/ModuleEmitter.cs) — module bodies use [ModuleBodyRewriter.cs](SourceGenerator~/Emitter/ModuleBodyRewriter.cs) which is **regex-based**, not AST-driven. Phase 1 defers this; module/hook bodies don't currently support JSX in expressions either.
- [PropsResolver.cs](SourceGenerator~/Emitter/PropsResolver.cs), [TagResolution.cs](SourceGenerator~/Emitter/TagResolution.cs) — unaffected.
- [StructureValidator.cs](SourceGenerator~/Emitter/StructureValidator.cs), [HooksValidator.cs](SourceGenerator~/Emitter/HooksValidator.cs) — unaffected (validation runs on AST shape, not on emit text).

### 2.3 HMR ([Editor/HMR](Editor/HMR/))

- [HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs) — reflection-based mirror of `CSharpEmitter`. Handles `ExpressionNode`, `CSharpExpressionValue`, `JsxExpressionValue` via type-name string switches. **Phase 1 must apply the same splice logic here**, otherwise hot-reload will silently emit broken code while cold-build works.
- HMR has **no direct access to source file text** at emit time — it reflects over the AST. Therefore Phase 1 must either:
  - **(A)** cache `MarkupRanges` + `BareJsxRanges` directly on `ExpressionNode` and `CSharpExpressionValue` at parse time (so HMR can see them via reflection), **or**
  - **(B)** make HMR call the same `FindBareJsxRanges` against a re-loaded source string.
- **Recommended: option A.** Adds two `ImmutableArray<(int Start, int End, int Line)>` fields to two AST nodes; ~5 lines of parser code; decouples splice from source access.

### 2.4 IDE / language-lib

- [Formatter/AstFormatter.cs](ide-extensions~/language-lib/Formatter/AstFormatter.cs) — line 717–722 handles `CSharpExpressionValue` and `JsxExpressionValue` in attribute formatting. **Phase 1: no logical change** (formatter works on AST, not emit). **Phase 2:** add `@(expr)` → `{expr}` canonicalization in child position.
- [Diagnostics/DiagnosticsAnalyzer.cs](ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs) — line 655, 1025 inspect attribute values. AST-level; unaffected.
- [Roslyn/VirtualDocumentGenerator.cs](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs) — wraps each `ExpressionNode` and `CSharpExpressionValue` in a synthetic `static object __expr_N() => expr;` method for Roslyn analysis. Lines 776, 806–814, 899, 920–927 handle the two node types. After Phase 1 the VDG would benefit from also splicing JSX in these wrapped expressions, otherwise IDE features (hover/completion/diagnostics) on `<Label/>` inside a ternary will see raw broken text.
  - **Note:** if VDG is *not* updated, the IDE will show Roslyn errors on the JSX literals even though the SG-emitted output compiles. Two truths to reconcile. Phase 1 should update VDG to apply the same splice.
- [SemanticTokens/SemanticTokensProvider.cs](ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs) — line 270 emits semantic tokens for `ExpressionNode`. AST-level; unaffected.
- [grammar/uitkx-schema.json](ide-extensions~/grammar/uitkx-schema.json), TextMate grammar — Phase 2 only: drop `@(...)` pattern.

### 2.5 VS Code, Rider, Visual Studio extensions

- All three are thin clients over the LSP server, which delegates to language-lib. **No client-side parsing.** Both phases are server-side only.
- LSP server itself: routes requests to language-lib; no changes needed.

### 2.6 Unity-side runtime ([Runtime](Runtime/), [Shared](Shared/), [Editor](Editor/))

- Generated C# calls `V.Tag(...)` factories. Phase 1 just makes more positions correctly emit those calls.
- Audited: `EditorRenderScheduler`, `EditorRootRendererUtility`, `UitkxAssetRegistrySync`, `UITKX_GeneratorTrigger`, `UitkxCsprojPostprocessor`, `UitkxChangeWatcher`, `UitkxConsoleNavigation`, `RegistryDebug`, `FiberMenu` — none have any source-form-dependent logic.
- **Runtime impact: zero.**

### 2.7 Tests ([SourceGenerator~/Tests](SourceGenerator~/Tests/))

| Test file | Existing coverage | Gap |
|---|---|---|
| [EmitterTests.cs](SourceGenerator~/Tests/EmitterTests.cs) | Directive-body JSX (`@if { <A/> }`); preamble JSX; bare attribute JSX | Ternary-in-child, ternary-in-attribute, lambda-in-attribute, generic/LINQ inside child expressions |
| [FormatterSnapshotTests.cs](SourceGenerator~/Tests/FormatterSnapshotTests.cs) | 8000+ lines; arrow-lambda preamble JSX (~line 1135–1225) | Same gaps as EmitterTests; Phase 2 needs canonicalization snapshot |
| [ParserTests.cs](SourceGenerator~/Tests/ParserTests.cs) | `attr={expr}` → `CSharpExpressionValue` shape | None for the AST shape; Phase 1 doesn't change parsing |
| [HmrEmitterParityContractTests.cs](SourceGenerator~/Tests/HmrEmitterParityContractTests.cs) | Bare JSX attribute (`attr={<Foo/>}`) | Ternary-in-attribute, lambda-in-attribute parity |
| [VirtualDocumentTests.cs](SourceGenerator~/Tests/VirtualDocumentTests.cs) | Source-map round-trip on simple expressions | JSX-inside-expression mapping |
| [DiagnosticsAnalyzerTests.cs](SourceGenerator~/Tests/DiagnosticsAnalyzerTests.cs) | Various directive errors | Phase 2: `@(expr)` deprecation diagnostic |

### 2.8 Samples & docs

- **Audit confirmed: zero samples use `@(cond ? <A/> : <B/>)` or any JSX-in-expression beyond the bare-attribute case.** Every `@(...)` in `Samples/**` is a plain variable spliced (`@(childNode)`, `@(__children)`, etc.) or a function call returning `VirtualNode` (`@(KeyDot(...))`).
- ReactiveUIToolKitDocs~/ examples follow the same pattern.
- Phase 2 migration cost: **zero source files** to rewrite.

---

## 3. Phase 1 — Detailed plan

### 3.1 Goal

Every `CSharpExpressionValue` and `ExpressionNode` becomes JSX-aware. After Phase 1, all six positions in §1 work uniformly.

### 3.2 Parser changes

Two new fields on `ExpressionNode` and `CSharpExpressionValue`:

```csharp
// In Nodes/AstNode.cs

public sealed record ExpressionNode(string Expression, int SourceLine, string SourceFile) : AstNode(...)
{
    public int ExpressionOffset { get; init; } = 0;
    public int ExpressionLength { get; init; } = 0;

    /// New (Phase 1): paren-wrapped JSX block ranges within Expression, absolute source positions.
    public ImmutableArray<(int Start, int End, int Line)> MarkupRanges { get; init; }
        = ImmutableArray<(int, int, int)>.Empty;

    /// New (Phase 1): bare JSX ranges within Expression, absolute source positions.
    public ImmutableArray<(int Start, int End, int Line)> BareJsxRanges { get; init; }
        = ImmutableArray<(int, int, int)>.Empty;
}

public sealed record CSharpExpressionValue(string Expression, int ExpressionOffset = 0) : AttributeValue
{
    /// Same two new fields.
    public ImmutableArray<(int Start, int End, int Line)> MarkupRanges { get; init; } = ...;
    public ImmutableArray<(int Start, int End, int Line)> BareJsxRanges { get; init; } = ...;
}
```

Populate at parse sites:

- [UitkxParser.cs:487–497](ide-extensions~/language-lib/Parser/UitkxParser.cs#L487) — child `{expr}`. After `ReadBraceExpressionWithOffset`, call `DirectiveParser.FindJsxBlockRanges(_source, exprOffset, exprOffset + expr.Length)` and `FindBareJsxRanges(...)`. Pass into `ExpressionNode`.
- [UitkxParser.cs:513–526](ide-extensions~/language-lib/Parser/UitkxParser.cs#L513) — child `@(expr)`. Same.
- [UitkxParser.cs:828–836](ide-extensions~/language-lib/Parser/UitkxParser.cs#L828) — attribute `CSharpExpressionValue`. Same.

### 3.3 Emitter changes

In [CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs), add a single helper that mirrors `SpliceSetupCodeMarkup`:

```csharp
private string SpliceExpressionMarkup(
    string expr,
    int exprAbsOffset,
    ImmutableArray<(int Start, int End, int Line)> markupRanges,
    ImmutableArray<(int Start, int End, int Line)> bareJsxRanges,
    int sourceLine)
{
    if (markupRanges.IsDefaultOrEmpty && bareJsxRanges.IsDefaultOrEmpty)
        return expr;

    // Merge + sort ranges by start position. Skip ranges that overlap (prefer outermost).
    // For each range:
    //   - convert absolute → expression-relative offsets
    //   - parse the JSX substring with UitkxParser.Parse(...)
    //   - emit via EmitNode into a temp StringBuilder
    //   - splice the emit result back into expr
    // Bonus for safety: if parse fails, leave the original substring (Roslyn will surface the error).
}
```

Wire into the two callers:

```csharp
private void EmitExpressionNode(ExpressionNode ex)
{
    string spliced = SpliceExpressionMarkup(
        ex.Expression, ex.ExpressionOffset,
        ex.MarkupRanges, ex.BareJsxRanges,
        ex.SourceLine);
    _sb.Append($"({spliced})");
}

private string AttrVal(AttributeValue v) => v switch
{
    StringLiteralValue slv => $"\"{EscStr(slv.Value)}\"",
    CSharpExpressionValue cev =>
        TransformExpression(SpliceExpressionMarkup(
            cev.Expression, cev.ExpressionOffset,
            cev.MarkupRanges, cev.BareJsxRanges,
            /*sourceLine*/ 0)),  // attribute line tracked elsewhere
    JsxExpressionValue jsx => EmitJsxToString(jsx.Element),
    BooleanShorthandValue => "true",
    _ => "null",
};
```

**Order matters:** splice **before** `TransformExpression`. The setter-lambda regex and asset-path resolution operate on C# source; once JSX is spliced into `V.Tag(...)` calls it's plain C# and the existing transforms run cleanly.

### 3.4 HookEmitter changes

[HookEmitter.cs](SourceGenerator~/Emitter/HookEmitter.cs) mirrors `CSharpEmitter` for hook render bodies. Apply identical splice (extract `SpliceExpressionMarkup` to a shared helper class or static method on `CSharpEmitter` that `HookEmitter` reuses).

### 3.5 HMR changes

[HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs) — duplicate the splice logic for the reflection-based emit path:

- Line 637 (`ExpressionNode` case): pull `MarkupRanges` and `BareJsxRanges` via reflection from the AST node, run the same splice, emit `(spliced)`.
- Line 2044 (`CSharpExpressionValue` case): same.

The splice helper itself can either be duplicated in HMR or hosted on a shared static class. Duplicate is simpler given HMR's reflection style.

**Critical:** add `HmrEmitterParityContractTests` cases for ternary-in-child, ternary-in-attribute, lambda-in-attribute. Without parity HMR will silently regress.

### 3.6 VirtualDocumentGenerator (IDE) changes

[VirtualDocumentGenerator.cs:776, 806–814, 899, 920–927](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs#L776) wraps each expression in a synthetic Roslyn-visible method. After Phase 1 the SG emits spliced (`V.Tag(...)`) but VDG would still wrap the raw `<Label/>`-bearing text — IDE diagnostics would diverge from compile diagnostics.

**Fix:** in VDG's expression-wrapping path, apply the same splice before injecting into the synthetic method body. Reuse the helper.

### 3.7 Source-map / `#line` directives

Phase 1 keeps the existing `#line SourceLine` semantics: errors inside a spliced JSX literal map to the **outer expression's** line, not the JSX element's line. Acceptable for v1; users can inspect the generated `.uitkx.g.cs`.

Phase 1.5 (deferred) could emit per-element `#line` markers inside the spliced output for line-precise error mapping.

### 3.8 New tests for Phase 1

Add to [EmitterTests.cs](SourceGenerator~/Tests/EmitterTests.cs):

```csharp
[Fact] public void Sg_JsxInChildExpressionTernary_Spliced()       // {cond ? <A/> : <B/>}
[Fact] public void Sg_JsxInChildExpressionAtParen_Spliced()       // @(cond ? <A/> : null)
[Fact] public void Sg_JsxInAttributeTernary_Spliced()             // attr={cond ? <A/> : null}
[Fact] public void Sg_JsxInAttributeLambda_Spliced()              // attr={item => <Label text={item}/>}
[Fact] public void Sg_JsxInChildLinqSelect_Spliced()              // {items.Select(x => <Label/>)}
[Fact] public void Sg_JsxInChildNestedTernary_Spliced()           // {a ? (b ? <A/> : <C/>) : <B/>}
[Fact] public void Sg_StringContainingTagLikeText_NotSpliced()    // {x: "<Label/>" } — must NOT splice (string literal)
[Fact] public void Sg_GenericLessThan_NotConfusedAsJsx()          // {items.Where(x => x.Age < 18)}
```

Add to [HmrEmitterParityContractTests.cs](SourceGenerator~/Tests/HmrEmitterParityContractTests.cs):

```csharp
[Fact] public void Hmr_JsxInChildExpressionTernary_MatchesSg()
[Fact] public void Hmr_JsxInAttributeTernary_MatchesSg()
[Fact] public void Hmr_JsxInAttributeLambda_MatchesSg()
```

Existing must-not-regress tests:
- Arrow-lambda preamble JSX (FormatterSnapshotTests ~line 1135–1225)
- Directive-body JSX (EmitterTests ~line 111)
- Bare attribute JSX (HmrEmitterParityContractTests ~line 111)

### 3.9 Phase 1 risk register

| Subsystem | Phase 1 risk | Mitigation |
|---|---|---|
| Parser | None — no logic changes, only adds two cached fields | Fields default to `Empty`; existing code paths see same values |
| `FindBareJsxRanges` / `FindJsxBlockRanges` correctness on arbitrary C# | Low — already trusted on preamble and directive bodies, which receive arbitrary C# (LINQ, ternaries, lambdas, generics in samples) | Add string-with-tag-like-text test, generic-less-than test |
| CSharpEmitter splice | Medium — new code path, but mirrors `SpliceSetupCodeMarkup` 1:1 | Test matrix in §3.8 |
| HookEmitter splice | Same as CSharpEmitter | Same |
| HMR parity | High — reflection-based mirror is easy to forget | Parity tests are mandatory; CI must fail loudly on divergence |
| VirtualDocumentGenerator | Medium — divergence between IDE and compile diagnostics if not updated | Update VDG in same PR; add VirtualDocumentTests case |
| Formatter | Low — runs on AST, doesn't see emit | Snapshot tests auto-detect drift |
| Runtime | None — generated C# calls same `V.*` factories | — |

### 3.10 Phase 1 footprint estimate

| Component | Files | LOC added | LOC modified |
|---|---|---|---|
| Parser (cache fields + populate) | 2 | 30 | 5 |
| CSharpEmitter | 1 | 80 | 10 |
| HookEmitter | 1 | 40 | 5 |
| HmrCSharpEmitter | 1 | 50 | 15 |
| VirtualDocumentGenerator | 1 | 30 | 10 |
| Tests | 3 | 200 | 0 |
| **Phase 1 total** | **9** | **430** | **45** |

---

## 4. Phase 2 — Detailed plan

### 4.1 Goal

Drop child-position `@(expr)` in favor of bare `{expr}`. Statement directives (`@if`, `@for`, `@foreach`, `@while`, `@switch`, `@case`) **untouched**. Attribute syntax `attr={expr}` is already brace-only — no change.

End state:

| Position | Syntax |
|---|---|
| JSX child, C# expression (with JSX freely nested after Phase 1) | `<Box>{expr}</Box>` |
| JSX attribute, C# expression (with JSX freely nested after Phase 1) | `<Box prop={expr}/>` |
| Statement-level control flow | `@if (...) { ... }`, `@foreach`, `@for`, `@while`, `@switch` |
| Component header | `component Name(...) { ... }` |

`@(...)` is gone.

### 4.2 Parser changes

[UitkxParser.cs:513–526](ide-extensions~/language-lib/Parser/UitkxParser.cs#L513) — `@` followed by `(` path. Two options:

- **Soft deprecate:** still parse, emit info-level diagnostic `UITKX_INFO_AT_PAREN_DEPRECATED`. Existing code keeps working until next format pass canonicalizes.
- **Hard remove:** stop parsing, `@(` becomes a parse error.

**Recommended: soft deprecate** + auto-canonicalize on save. Zero user friction; old `.uitkx` files keep working until they're touched, at which point the formatter rewrites.

### 4.3 Formatter changes

[AstFormatter.cs](ide-extensions~/language-lib/Formatter/AstFormatter.cs) — when emitting a child-position `ExpressionNode`, always print `{expr}` regardless of original form.

(The `ExpressionNode` doesn't currently track which form it came from. After parsing, both `@(x)` and `{x}` produce the same `ExpressionNode`. Formatter just always emits `{expr}`. Done.)

### 4.4 Grammar / TextMate

[ide-extensions~/grammar/uitkx-schema.json](ide-extensions~/grammar/uitkx-schema.json) and the TextMate grammar — drop the `@(...)` rule from JSX child syntax-highlighting patterns. Keep `@if`/`@for`/etc.

### 4.5 Samples & docs

- Run formatter over `Samples/**/*.uitkx` to canonicalize. (Audit shows zero `@(cond ? <A/> : <B/>)` patterns; only `@(varName)` and `@(fnCall(...))` patterns, all of which become `{varName}` and `{fnCall(...)}` respectively.)
- Update [ReactiveUIToolKitDocs~](ReactiveUIToolKitDocs~/) examples to use `{expr}` only.
- Update language reference (drop `@(...)` form, document one syntax: `{expr}`).

### 4.6 New tests for Phase 2

```csharp
[Fact] public void Diag_AtParenInChildPosition_EmitsDeprecationInfo()
[Fact] public void Fmt_AtParenChildExpression_NormalizedToBraceOnSave()
```

### 4.7 Phase 2 risk register

| Subsystem | Phase 2 risk | Mitigation |
|---|---|---|
| Parser (soft deprecate) | None — keeps working, just adds info diagnostic | Diagnostic severity = info |
| Formatter canonicalization | None — snapshot-driven | Add 1 snapshot |
| Grammar (TextMate) | Low — old files might highlight slightly differently if user disables formatter | Regenerate grammar; document |
| HMR / runtime / IDE features | None — `ExpressionNode` is the same regardless of source form | — |
| Samples / docs | None — zero existing usage | — |

### 4.8 Phase 2 footprint estimate

| Component | Files | LOC added | LOC modified |
|---|---|---|---|
| Parser (soft deprecate) | 1 | 5 | 2 |
| Formatter canonicalization | 1 | 5 | 5 |
| Grammar (TextMate / schema) | 1–2 | 0 | 5 |
| Tests | 2 | 30 | 0 |
| Docs | 1–2 | 10 | 30 |
| **Phase 2 total** | **6–8** | **50** | **42** |

---

## 5. Combined footprint

| | Files | LOC added | LOC modified | Risk |
|---|---|---|---|---|
| **Phase 1** | 9 | 430 | 45 | Medium-High (HMR parity, VDG sync) |
| **Phase 2** | 6–8 | 50 | 42 | Low (cosmetic) |
| **Combined** | 11–14 | 480 | 87 | Medium |

---

## 6. Why the React-uniform model is the right shape

Babel/TSC don't have this asymmetry because **JSX is part of the JS grammar.** Inside any expression, `<Tag/>` is recognized recursively. There is no "scan for `<Tag>` substrings" pass; there's one parser that knows both languages.

uitkx **could** have a real grammar where C# expressions and JSX are mutually recursive — but that means a Roslyn-aware parser or replacing the current text-scanner with a real grammar. Big project.

The pragmatic alternative is what Phase 1 does: **wire the existing scanner into the missing positions.** `FindBareJsxRanges` is already trusted on arbitrary C# (preamble + directive bodies handle ternaries, lambdas, LINQ today). Reusing it in two more positions makes the system uniform without a rewrite. The implementation is ~80 LOC of splice plumbing.

If we ever want to graduate to a real grammar, the AST shape after Phase 1 is what that grammar would produce — so Phase 1 doesn't paint into a corner.

---

## 7. Open questions

1. **Range cache vs reflective re-scan.** Phase 1 adds `MarkupRanges` + `BareJsxRanges` to `ExpressionNode` and `CSharpExpressionValue`. Alternative: HMR re-loads source and re-scans. **Recommended: cache.** ~5 lines of parser code; decouples HMR from source access.
2. **`JsxExpressionValue` retention.** Today the bare-JSX-attribute path (`attr={<Foo/>}`) takes a special `JsxExpressionValue` with a parsed `ElementNode`. After Phase 1 the same shape would also work via `CSharpExpressionValue` + splice. **Recommended: keep `JsxExpressionValue`** for the common single-literal case (slightly cleaner emit; AttrVal switch already exists). It becomes an optimization/convenience, not the only working path.
3. **`#line` precision.** Phase 1 uses outer expression's `SourceLine` for spliced JSX. Phase 1.5 could emit per-element `#line` for precise mapping. **Defer.**
4. **Module/hook bodies.** [ModuleBodyRewriter](SourceGenerator~/Emitter/ModuleBodyRewriter.cs) is regex-based, doesn't go through the AST emit path. It currently doesn't support JSX in expressions either, so deferring is consistent. **Defer to Phase 1.5.**
5. **Hard vs soft deprecation of `@(...)`.** Soft is recommended; user has no migration cost but legacy files keep working. Hard could be a flag in a major version bump.

---

## 8. Implementation order

Recommended sequence:

1. **Phase 1 prep**
   - [ ] Add `MarkupRanges` + `BareJsxRanges` to `ExpressionNode` and `CSharpExpressionValue` in [Nodes/AstNode.cs](ide-extensions~/language-lib/Nodes/AstNode.cs).
   - [ ] Populate at parse sites in [UitkxParser.cs](ide-extensions~/language-lib/Parser/UitkxParser.cs).
   - [ ] Add the 8 new emitter tests **first** (TDD). Verify they fail.

2. **Phase 1 SG**
   - [ ] Implement `SpliceExpressionMarkup` in [CSharpEmitter.cs](SourceGenerator~/Emitter/CSharpEmitter.cs).
   - [ ] Wire into `EmitExpressionNode` and `AttrVal`.
   - [ ] All 8 tests pass; full suite (1164+) green.

3. **Phase 1 hook emitter**
   - [ ] Mirror in [HookEmitter.cs](SourceGenerator~/Emitter/HookEmitter.cs).

4. **Phase 1 HMR**
   - [ ] Mirror in [HmrCSharpEmitter.cs](Editor/HMR/HmrCSharpEmitter.cs).
   - [ ] Add 3 new HMR parity tests; pass.

5. **Phase 1 VDG**
   - [ ] Update [VirtualDocumentGenerator.cs](ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs) to splice before wrapping.
   - [ ] Add VirtualDocumentTests case.

6. **Phase 1 release**
   - [ ] Bump library + extension versions.
   - [ ] Changelog entry: "Feature: JSX literals now work inside ternaries, lambdas, and arbitrary C# expressions in attribute values and JSX child positions, matching React/Babel semantics."

7. **Phase 2** (optional, after Phase 1 settles)
   - [ ] Soft-deprecate `@(...)` parser path with info diagnostic.
   - [ ] Canonicalize `@(...)` → `{...}` in formatter.
   - [ ] Drop TextMate `@(...)` rule.
   - [ ] Run formatter over samples and docs.

---

## 9. Conclusion

Both phases are realistic and unblock real ergonomic patterns. Phase 1 is the high-value piece (uniform JSX-in-expression behavior); Phase 2 is cosmetic polish. Together they bring uitkx's expression grammar in line with React/Babel without a parser rewrite. The existing `FindBareJsxRanges` scanner is the load-bearing component — already trusted in two contexts, simply applied in four more.

Phase 1 effort: ~3-4 working days.
Phase 2 effort: ~1 day.
Combined risk: medium (HMR parity is the main hazard).
Combined LOC: ~480 added, ~90 modified, 11–14 files.
Runtime impact: zero.
Breaking changes: none (Phase 2 is a soft deprecation).

---

## 10. Second-pass deep findings (2026-05-08)

This pass goes beyond the structural plan above and verifies claims empirically against the actual generator. **Methodology:** ran a probe test through `GeneratorTestHelper.Run` for each questionable pattern and dumped the emitted C# to confirm or refute predictions. Results override anything in §§1–9 where they conflict.

### 10.1 Scanner robustness — empirically tested patterns

The `FindBareJsxRanges` + `FindJsxBlockRanges` scanners (in [DirectiveParser.cs](ide-extensions~/language-lib/Parser/DirectiveParser.cs)) handle the following correctly **today** (verified by probing the preamble path, which already routes through these scanners):

| Input (in preamble) | Emitted C# | Result |
|---|---|---|
| `var el = flag ? <Label /> : null;` | `var el = flag ? V.Label(__p_0, key: null) : null;` | ✅ |
| `var el = fallback ?? <Label />;` | `var el = fallback ?? V.Label(__p_0, key: null);` | ✅ (null-coalescing handled) |
| `var first = items.Count < 1 ? "empty" : "has";` | `var first = items.Count < 1 ? "empty" : "has";` | ✅ (`<` not confused with JSX) |
| `var s = "<Label/>";` | `var s = "<Label/>";` | ✅ (string literal preserved) |
| `OfType<string>(items).Select<string, VirtualNode>(s => <Label/>)` | `...Select<string, VirtualNode>(s => V.Label(...))` | ✅ (explicit generic args + lambda + JSX all coexist) |

This is **stronger** than the original plan implied: the scanner is not a heuristic minefield. The interpolated-string skipper, generic vs operator disambiguation, and `??`-followed-by-JSX path all already work in the preamble. **Phase 1 simply applies the same already-trusted machinery in two more positions.**

Notes on the algorithm (verified by reading [DirectiveParser.cs](ide-extensions~/language-lib/Parser/DirectiveParser.cs)):

- **Operator disambiguation:** the `=`/`?`/`:` triggers each peek the previous and next char to reject `==`, `=>`, `??`, `?.`, `::`, `!=`, `<=`, `>=`. Solid.
- **String / char / interpolated string skipping:** handled by `TrySkipStringOrCharLiteral` which tracks brace depth inside `$"..."` interpolation holes — JSX inside an interpolation hole would actually get spliced too (rare but correct).
- **Comment skipping:** `//` and `/* */` are skipped before any `<` recognition.
- **Termination via `FindJsxElementEnd`** ([ReturnFinder.cs](ide-extensions~/language-lib/Parser/ReturnFinder.cs)): plain `<` / `</` / `/>` depth counter, also skips strings inside attribute values. On malformed JSX it scans to EOF; the SG then gets a parse error from `UitkxParser.Parse`, which surfaces as a `#error` diagnostic — **safe failure mode, never silent corruption**.

### 10.2 Generic args false-positive risk — measured, low

Concern raised in original plan: `<Foo>` could look like a self-closing tag `<Foo/>` and confuse `FindJsxElementEnd`. Empirical test:

```csharp
var first = OfType<string>(items).Select<string, VirtualNode>(s => <Label />);
```

The scanner's operator-position trigger only fires after `=`/`?`/`:`/`return`/`=>`/`(`. None of those precede `<string>` or `<string, VirtualNode>` — those `<` characters appear after `OfType` and `Select` (identifiers). So the scanner never even *peeks* at them. Generic args are invisible to the JSX detector, by construction.

Where the scanner *does* activate (after `=>`), it correctly identifies the lambda body's `<Label />` JSX and splices it. **Generic-arg false-positive is structurally impossible** in the current design.

### 10.3 Sample audit — exact `@(...)` inventory

Searched all `.uitkx` files. **Zero** uses of `@(cond ? ... : ...)`, `@(x ?? ...)`, `@(x => ...)`, or any non-trivial expression. Every existing usage falls into:

- **Plain identifier** (~15 occurrences): `@(childNode)`, `@(sidebar)`, `@(content)`, `@(portalNode)`, `@(__children)`, `@(catBadge)`, `@(deepStatusNode)`, `@(statusNode)`, `@(inlineNode)`
- **Function call returning `VirtualNode`** (~6): `@(KeyDot(...))`, `@(DeepPillBar(...))`, `@(PillBar(...))`, `@(DeepNestedSection(...))`

Phase 2 migration: a single formatter pass over `Samples/**/*.uitkx` rewrites all of them to `{name}` / `{fnCall(...)}`. Mechanical, no edge cases.

### 10.4 IDE divergence — VirtualDocumentGenerator must be updated in the SAME PR as Phase 1

Important refinement to §3.6: if the SG splices `<Label/>` → `V.Label(...)` but VDG keeps wrapping the raw `<Label/>` string into a synthetic Roslyn method, then:

- **Compile** (uses SG output): success.
- **IDE Roslyn-driven diagnostics** (uses VDG output): false errors on the JSX literal.

Result: file looks broken in editor but compiles cleanly. Confusing and dangerous. **VDG splicing is non-optional for Phase 1 — must ship together with SG splicing, not in a follow-up.**

Add to §8 implementation order: VDG is a hard prereq for the Phase 1 release (not "Phase 1 SG" → "Phase 1 VDG" sequenced; they ship as one atomic unit).

### 10.5 Hooks-validator — text-pattern scan is safe under Phase 1

[HooksValidator.cs:108, 120](SourceGenerator~/Emitter/HooksValidator.cs#L108) does substring search for `useState(`, `useReducer(`, etc., on `ExpressionNode.Expression` and `CSharpExpressionValue.Expression`. These are **AST-level fields**, never modified by the emitter. Phase 1 splicing happens **at emit time on a copy of the text**, not on the AST. The validator runs on the AST → it sees the original text → behavior is identical pre- and post-Phase-1. **No change required.**

This is also the reason the design is clean: AST is immutable and shared; emit constructs derived strings on demand. Phase 1 fits this pattern naturally.

### 10.6 Diagnostics inventory — none affected by Phase 1

Audit of [DiagnosticsAnalyzer.cs](ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs):

| Code | Trigger | Phase 1 impact |
|---|---|---|
| UITKX0013–0016 | Hooks in control blocks/attributes | None — AST-level pattern match |
| UITKX0018 | `useEffect` missing deps | None — AST-level |
| UITKX0024 | Control block missing return | None — directive structure unchanged |
| UITKX0025/0108 | Multiple root elements | None — markup structure unchanged |
| UITKX0104 | Duplicate sibling key | None — AST-level |
| UITKX0106 | Loop element missing key | None — AST-level |
| UITKX0109 | Unknown attribute | None — attribute resolution unchanged |
| UITKX0120/0121 | Asset path resolution errors | None — runs in `TransformExpression`, executes after splice; if JSX is inside `Asset<T>("path")` (unlikely), splice runs first (no JSX inside asset paths) |
| UITKX0150 | Module body parse failure | None — Phase 1 defers module bodies |
| UITKX2106 | Bad component param | None — unrelated path |

No new diagnostic codes needed for Phase 1. Phase 2 may add an info-level `UITKX0xxx_AT_PAREN_DEPRECATED` (informational only; samples migrate via formatter).

### 10.7 SemanticTokens behavior — degrades gracefully, can be improved later

[SemanticTokensProvider.cs:270](ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs#L270) tokenizes `ExpressionNode` as a single C#-expression run. After Phase 1, JSX literals inside the expression are spliced at emit time but the AST still stores the raw expression text. So:

- **Today:** JSX inside `{cond ? <Label/> : null}` is colored as plain C# (no JSX coloring on `<Label/>`).
- **After Phase 1 (no semantic-token change):** Same coloring as today — emit-level rewrite doesn't reach the token provider.
- **Optional Phase 1.5:** SemanticTokensProvider walks `ExpressionNode.MarkupRanges` + `BareJsxRanges` (newly cached on the node) and emits JSX-flavored tokens for those sub-spans. Modest LOC.

Acceptable to ship Phase 1 without this; the file compiles and runs correctly, only the editor coloring is plain.

### 10.8 Build / versioning — additive AST change

Adding `MarkupRanges` and `BareJsxRanges` to `ExpressionNode` and `CSharpExpressionValue` (`init` properties with `Empty` defaults):

- **Binary-compatible** — defaults are `ImmutableArray<...>.Empty`, so any code that doesn't set them still compiles.
- **Source-compatible** — record positional constructors are unchanged; only `with`-expression and direct `init` assignments differ.
- **Reflection consumers** (HMR via `GetProp(obj, "MarkupRanges")`) — get the new fields automatically.

Recommended bump: **library 0.5.1 → 0.6.0** (minor, additive feature). Extensions 1.1.13 → 1.2.0.

### 10.9 Risks confirmed and refined

| Risk | Original assessment | Verified assessment |
|---|---|---|
| Scanner false-positives on generics | Medium | **Low** — structurally impossible (scanner only triggers after specific operator chars, none of which precede generic-arg `<`) |
| Scanner missing `??`-followed-by-JSX | Flagged by subagent's first pass | **Refuted** — empirically works today (see 10.1) |
| Scanner mishandling string interpolation | Medium | **Low** — verified via reading `TrySkipStringOrCharLiteral`; brace-depth tracking inside `$"..."` is correct |
| HMR parity drift | High | **High** (unchanged) — HMR is reflection-based and easy to forget; mandatory parity tests required |
| VDG divergence | Medium | **High** — must ship in same PR as SG splice, not deferred. Without it, IDE shows phantom errors on compiling files |
| Module/hook body JSX | Medium | **Medium** (deferred — `ModuleBodyRewriter` is regex-based and out of scope for Phase 1) |
| Per-element `#line` precision | Low | **Low** (deferred to 1.5) |

### 10.10 Refinements to the implementation order (§8)

Updated sequence:

1. **AST cache fields + parser populate** — additive, no behavior change. Run full test suite; expect zero diff.
2. **Write 8+ failing tests first** (TDD). Document expected emitted C# for each.
3. **Implement `SpliceExpressionMarkup` in CSharpEmitter, wire `EmitExpressionNode` and `AttrVal`.** Tests pass.
4. **Mirror in HookEmitter.** Hook tests pass.
5. **Mirror in HmrCSharpEmitter, add 3 parity tests.** **HARD GATE — do not proceed without parity.**
6. **Update VirtualDocumentGenerator** to splice before wrapping. **HARD GATE — must ship together with SG; verify with IDE-roundtrip test.**
7. **Run full suite (1164+ tests). Run preamble JSX, directive-body JSX, attribute literal JSX regression cases.** Confirm no drift.
8. **Bump library 0.5.x → 0.6.0, extensions 1.1.13 → 1.2.0.** Changelog: "Feature: JSX literals work in arbitrary C# expression positions (ternaries, lambdas, attribute values, child expressions) — matches React/Babel."
9. **Phase 2** (separate PR, post-Phase-1):
   - Soft-deprecate `@(...)` parser path (info diagnostic).
   - Formatter canonicalizes `@(expr)` → `{expr}` in child position.
   - Drop TextMate `@(...)` rule.
   - Run formatter over samples and docs.

### 10.11 Plan revisions

Two non-trivial revisions vs. §§1–9:

1. **§3.6 (VDG) is upgraded from "secondary" to "Phase 1 hard prereq."** Without VDG splicing, the IDE shows phantom errors on files that compile cleanly. Both must ship together.
2. **The `??`-with-JSX limitation flagged in the subagent's first pass is refuted.** The scanner already handles it (verified empirically). Don't waste effort "fixing" what isn't broken.

Other §§1–9 content holds.

### 10.12 Open questions resolved

- *Does HMR have source access?* — No. Reflects over AST. AST cache (option A in §3.5) is required. **Resolved: cache.**
- *Do existing tests assert column-precise diagnostics on JSX inside expressions?* — No. Existing tests assert at line granularity for inner-expression elements; Phase 1's coarse "outer SourceLine" mapping is acceptable. **Resolved: defer per-element `#line` to 1.5.**
- *Hard or soft deprecation of `@(...)` in Phase 2?* — Soft. Zero migration friction, no user impact, formatter auto-canonicalizes. **Resolved: soft.**
- *Should `JsxExpressionValue` (the bare-attribute-JSX-literal opt-in) survive after Phase 1?* — Yes, as an emit optimization. Cleaner for the common case. The fallback `CSharpExpressionValue` + splice path handles the harder cases. **Resolved: keep both.**
- *Module/hook body JSX timing?* — Defer. ModuleBodyRewriter is a separate regex-based system; refactoring it is its own project. **Resolved: Phase 1.5+.**

### 10.13 Final risk-adjusted estimate

| | Files | LOC added | LOC modified | Risk |
|---|---|---|---|---|
| Phase 1 (incl. VDG hard prereq) | 10 | 460 | 60 | Medium |
| Phase 2 | 6–8 | 50 | 42 | Low |
| **Combined** | **12–15** | **510** | **102** | **Medium** |

Effort unchanged: Phase 1 ≈ 3–4 days; Phase 2 ≈ 1 day. Risk concentrated in HMR and VDG parity; both have clear test strategies and hard CI gates.
