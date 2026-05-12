# PrettyUi reproduction — open HMR / runtime bugs

Tracking bucket for bugs reproduced against the [`Samples/UIs/PrettyUi`](../Samples/UIs/PrettyUi/README.md)
in-repo mirror of `C:\Users\neta\Pretty Ui\Assets\UI`. Reported during HMR
session right after 0.4.18 shipped. **Not yet investigated, not yet fixed.**

Reproduction baseline: edit `Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx`
(or any `.style.uitkx` / `.hooks.uitkx`) while play mode + HMR are active.

---

## Issue 1 — spurious "API drift" warnings on Roslyn calls (cosmetic)

**Symptom (Unity Console, on HMR start / first compile):**

```
[HMR] MetadataReference.CreateFromFile: HMR passed 1 args but the loaded
language library declares 3 parameter(s); padded missing tail with compile-time
defaults. Update the HMR compiler to pass the new arguments explicitly.
  at UitkxHmrCompiler.InvokeWithDefaults (...:1436)
  at UitkxHmrCompiler.BuildMetadataReferences (...:857)

[HMR] CSharpSyntaxTree.ParseText: HMR passed 2 args but the loaded language
library declares 5 parameter(s); padded missing tail with compile-time
defaults. ...
  at UitkxHmrCompiler.InProcessCompile (...:923)
```

**Diagnosis (preliminary — not verified).** The drift-detection warning in
`InvokeWithDefaults` was designed for **language-library** calls (where new
optional params indicate real API drift HMR should learn about, e.g. the
`lineOffset` added in 0.4.7). It also fires for Roslyn calls where the trailing
optional params (`MetadataReferenceProperties`, `DocumentationProvider`,
`parseOptions`, `encoding`, `path`, `CancellationToken`) have stable canonical
defaults we *intentionally* don't pass.

**Fix shape (deferred).** Add a `silent` flag to `InvokeWithDefaults` for
Roslyn-targeted call sites; suppress the drift warning there but keep the
padding behavior intact. Drift warning stays loud for language-library calls.

---

## Issue 2 — genuine API drift on `UitkxParser.Parse`

**Symptom (Unity Console, on every save):**

```
[HMR] UitkxParser.Parse: HMR passed 5 args but the loaded language library
declares 6 parameter(s); padded missing tail with compile-time defaults.
Update the HMR compiler to pass the new arguments explicitly.
  at UitkxHmrCompiler.Compile (...:201)
```

**Diagnosis (preliminary — not verified).** Real drift. Language library has
gained another optional parameter on `UitkxParser.Parse` since the HMR compiler
was last synced. Need to identify the new parameter in
`Shared/Language/UitkxParser.cs` (or wherever `UitkxParser.Parse` lives now)
and pass it explicitly from both call sites in
`Editor/HMR/UitkxHmrCompiler.cs:201` and the markup-parse-func closure ~line
225.

**Note.** This warning cleanly disappears once the explicit arg is added; until
then, padding with the compile-time default is *probably* correct (depends on
what the new param does — must verify before declaring "harmless").

---

## Issue 3 — REAL FAILURE: `Compilation.Emit` discovery selects wrong overload

**Symptom (Unity Console, on every save):**

```
[HMR] In-process compile failed, trying external: [HMR] In-process Roslyn
error: [HMR] Compilation.Emit: missing required argument 'pdbStream'
(position 1). The HMR compiler is out of sync with the loaded language
library.
  at UitkxHmrCompiler.CompileSources (...:905)
```

**Diagnosis (preliminary — not verified).** `_emitToStream` discovery in
`UitkxHmrCompiler.TryLoadRoslyn` (line ~820) does:

```csharp
_emitToStream = baseCompilationType
    .GetMethods(...)
    .First(m => m.Name == "Emit"
                && m.GetParameters().Length > 0
                && m.GetParameters()[0].ParameterType == typeof(Stream));
```

`Compilation.Emit` has many overloads. `First(...)` ordering is non-deterministic
across Roslyn versions / runtimes. In the consumer's environment it picked an
overload where `pdbStream` (position 1) has **no default value** — so the
padding logic in `InvokeWithDefaults` correctly refuses to invent one and
throws.

The canonical "all-optional-tail" overload is:

```csharp
EmitResult Emit(
    Stream peStream,
    Stream pdbStream = null,
    Stream xmlDocumentationStream = null,
    Stream win32Resources = null,
    IEnumerable<ResourceDescription> manifestResources = null,
    EmitOptions options = null,
    IMethodSymbol debugEntryPoint = null,
    Stream sourceLinkStream = null,
    IEnumerable<EmbeddedText> embeddedTexts = null,
    CancellationToken cancellationToken = default)
```

**Fix shape (deferred).** Tighten discovery to pick the overload where every
parameter after position 0 is optional (`HasDefaultValue == true`). Apply the
same tightening to `_createFromFile` and `_parseText` discovery for consistency.

**Severity.** This causes the in-process compiler path to fail every time and
fall through to the external `dotnet exec` compiler — slow path, still works,
but blows up `[HMR]` log noise and adds 100s of ms per save.

---

## Issue 4 — children disappear after HMR-triggered recompile

**Symptom.** After editing `Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx`
(`module {}` body change — adjusting `BackgroundImage`, `Padding`, etc.) HMR
does its recompile cycle and the on-screen tree collapses: only the top-level
component (`AppRoot`) remains, every child component (`<Router>`, the
`<Routes>`, every `Page*` and `*Func` in the tree) vanishes.

**Reproduction status.** Reported verbally; needs a concrete repro inside the
new in-repo `Samples/UIs/PrettyUi` mount before deeper investigation.

**Diagnosis.** Not started. Candidate angles to explore later:

- Does HMR's reapply path strip the rendered children when only the
  `module` half of a paired `Foo.uitkx` / `Foo.style.uitkx` recompiles?
- Does `HmrHookEmitter` wipe / replace the previously-rendered VNode tree
  for the component instead of patching the module-scope statics?
- Does `Style.BackgroundImage` re-resolution invalidate something downstream
  in the reconciler when the `Asset<T>` registry lookup changes value
  (or fails) on the new compilation?

**Severity.** High — a successful HMR recompile of a `.style.uitkx` should
**never** drop the live render tree. This is a regression from the 0.4.16
"HMR self-disable on infra error" hardening or the 0.4.17 module/hook asset
path rewrite — needs `git bisect` against the PrettyUi repro once it's
reachable in-repo.

---

## Repro workflow

1. Open the repo in Unity (Editor uses `ReactiveUITK.Samples` asmdef which now
   transitively includes `Samples/UIs/PrettyUi/PrettyUiBootstrap.cs`).
2. Create a scene, add an empty GameObject, attach a `UIDocument` (any
   PanelSettings), then attach `PrettyUiBootstrap` and drag the `UIDocument`
   into its `Ui Document` field.
3. Enter play mode.
4. Open the HMR window (`Window → ReactiveUITK → HMR` — verify exact path);
   click Start. Issue 1, 2, 3 should fire on first save. Issue 4 should fire
   after editing `Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx`.

## Out of scope

- Investigating the bugs (deferred until repro is confirmed in-repo).
- Editing `Editor/HMR/*.cs` (no fixes until repro confirmed).
- Bumping version, changelog updates (no fixes → no entry).


---

# Comprehensive HMR coherence audit (post 0.4.18)

> **Mandate.** User asked for a thorough whole-system review of HMR vs the
> source generator (SG) so we stop fixing one bug at a time. This section
> enumerates every **silent divergence** between HMR (`Editor/HMR/*.cs`) and
> the SG (`SourceGenerator~/Emitter/*.cs`) found by reading both code paths
> side-by-side. Issues 1�4 above are repeated only by cross-reference; new
> findings start at Issue 5. Source-generated artifacts on a *cold* build are
> the ground truth; HMR must produce semantically equivalent C# from the
> same `.uitkx` input.

## Architecture recap

- **SG** is a Roslyn ISourceGenerator running in a clean compile (Editor
  domain). It has full Roslyn `Compilation`, `INamedTypeSymbol`, and a
  `PropsResolver` that walks symbols to resolve props types, ref-param props,
  peer-component metadata, etc. ~3700 lines, 1070 unit tests passing.
- **HMR** is a hand-written reflection-driven transpiler living in
  `Editor/HMR/HmrCSharpEmitter.cs` (~2400 lines) plus
  `Editor/HMR/UitkxHmrCompiler.cs` (~1500 lines). It loads the IDE language
  library DLL via `Assembly.LoadFrom`, reads the AST via reflection
  (`n.GetType().Name`-based switch), and resolves props via
  `AppDomain.CurrentDomain.GetAssemblies()` reflection scans. No symbols, no
  resolver, no compilation graph � *snapshot of the live AppDomain*.
- **Consequence.** Every cold-build code path that uses `INamedTypeSymbol`
  or `_resolver` in SG must have a hand-written equivalent in HMR. When the
  SG learns a new trick, HMR needs to be updated **manually in lockstep** �
  there is no shared emitter.

## Verified parity (no action needed)

These were checked and are functionally aligned:

- **AST node-kind coverage.** `HmrCSharpEmitter.EmitNode` switch
  ([HmrCSharpEmitter.cs#L657](../Editor/HMR/HmrCSharpEmitter.cs#L657)) handles
  every AST node kind SG handles: `ElementNode`, `TextNode`, `ExpressionNode`,
  `IfNode`, `ForeachNode`, `ForNode`, `WhileNode`, `SwitchNode`, `CommentNode`.
- **Static-style hoisting (OPT-V2-2 Phase A).** HMR has
  `TryHoistStaticStyle` ([HmrCSharpEmitter.cs#L1584](../Editor/HMR/HmrCSharpEmitter.cs#L1584))
  emitting into `_hoistedStyleFields` and a class-level `// -- Hoisted static
  styles (OPT-V2-2) --` block. Matches SG.
- **Function-style props class.** HMR `EmitFunctionPropsClass`
  ([HmrCSharpEmitter.cs#L1419](../Editor/HMR/HmrCSharpEmitter.cs#L1419))
  emits the synthetic `{Name}Props : IProps` with `Equals`/`GetHashCode`
  overrides. Matches SG.
- **Pool rent for built-in typed elements.** HMR `EmitTyped`
  ([HmrCSharpEmitter.cs#L836](../Editor/HMR/HmrCSharpEmitter.cs#L836)) uses
  `BaseProps.__Rent<T>()` and `Style.__Rent()` into `_rentBuffer`, hoisted
  before the return. Matches SG ([CSharpEmitter.cs#L968](../SourceGenerator~/Emitter/CSharpEmitter.cs#L968)).
- **Props-type resolution (post-0.4.18).** HMR `FindPropsType`
  ([HmrCSharpEmitter.cs#L1063](../Editor/HMR/HmrCSharpEmitter.cs#L1063))
  mirrors SG `PropsResolver.TryGetFuncComponentPropsTypeName` fallback chain.
  Confirmed working in 0.4.18.
- **`@inject` field emission.** HMR
  ([HmrCSharpEmitter.cs#L208](../Editor/HMR/HmrCSharpEmitter.cs#L208))
  emits the `// @inject fields` block before Render. Matches SG.
- **`@uss` stylesheet keys.** HMR `_isRootElement && _ussFiles.Count > 0`
  injects `__uitkx_ussKeys`. Path resolution appears intact.
- **Setup-code JSX splice + line directives.** `SpliceSetupCodeMarkup`,
  `ApplyHookAliases`, `ResolveAssetPaths`, `#line {srcLine} "{_linePath}"`
  emission all present. Hot-edit step-through still works.
- **Fiber rollback / state preservation.** `FiberReconciler` HMR catch
  path ([FiberReconciler.cs#L457](../Shared/Core/Fiber/FiberReconciler.cs#L457))
  using `HmrPreviousRender` and `ResetComponentStateForHmrRollback`
  ([FiberReconciler.cs#L1550](../Shared/Core/Fiber/FiberReconciler.cs#L1550))
  is sound. Delegate swap (`UitkxHmrDelegateSwapper.WalkFiber`) preserves
  hooks unless `HasHookSignatureChanged` returns true. **No issues found in
  the swap pipeline.**
- **Canonical lowering.** `CanonicalLowering.LowerToRenderRoots` is now a
  no-op pass-through ([CanonicalLowering.cs#L17](../ide-extensions~/language-lib/Lowering/CanonicalLowering.cs#L17));
  HMR's reflection wrapper is wasted work but harmless.

---

## Issue 5 � `<MyFuncComponent ref={x}>` is silently dropped after HMR recompile

**Severity.** Silent miscompile. Critical. Same severity class as Issue 4.

**Symptom (no console output).** Imperative refs to function components stop
firing after the first hot reload. `myRef.Current` stays `null` for the
post-HMR generation; any code that depends on it (focus management, scroll
APIs, third-party DOM-style adapters) silently no-ops.

**Repro shape.**

```uitkx
@using ReactiveUITK.Hooks;

component App {
    var inputRef = UseRef<TextField>();
    var focus = () => inputRef.Current?.Focus();

    return (
        <VisualElement>
            <MyTextField ref={inputRef} />
            <Button OnClick={focus}>Focus</Button>
        </VisualElement>
    );
}
```

After editing `App.uitkx` (or a `.style.uitkx` companion) and triggering HMR,
clicking "Focus" silently no-ops because `inputRef.Current` is `null`.

**Root cause.**
[`HmrCSharpEmitter.EmitFuncComponent` line 985](../Editor/HMR/HmrCSharpEmitter.cs#L985):

```csharp
var filteredAttrs = FilterAttrs(attrs, "key");
filteredAttrs = FilterAttrs(filteredAttrs, "ref");   // ? drop, no re-route
```

HMR unconditionally **drops** the `ref` attribute. The SG counterpart
([`CSharpEmitter.cs#L1165`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1165))
calls `_resolver.TryGetRefParamPropName(componentSymbol, out propName)` to
locate the Props field of type `MutableRef<T>` and writes
`propsVar.{propName} = (MutableRef<T>)refExpr;` so the ref is delivered into
the synthesized props object.

**Why HMR can't easily replicate.** No `INamedTypeSymbol` available � would
need to walk reflection metadata of the resolved Props type and find the
property whose declared type is generic-`MutableRef<>`. Doable with the
existing `FindPropsType`+`Type.GetProperties()` machinery, just not done.

**Proposed fix.** Extend `FindPropsType`'s post-resolution path: after
locating the Props `Type`, scan its public instance properties for one whose
`PropertyType.IsGenericType && GetGenericTypeDefinition() ==
typeof(MutableRef<>)`. Cache the first match per `(componentTypeName)`.
Inside `EmitFuncComponent`, **before** the `FilterAttrs(... "ref")` call,
read the `ref={...}` value with `GetAttrExpr`, then if non-null and a
`MutableRef` slot was found, append `{refSlotName} = (MutableRef<...>){val}`
to the props initializer.

---

## Issue 6 � `<Component prop={<Jsx/>}>` collapses to `null` after HMR recompile

**Severity.** Silent miscompile. Critical. **This is the root cause of Issue 4
on PrettyUi**, but the symptom class is broader: every JSX-valued attribute on
every component breaks.

**Repro shape.** PrettyUi already exhibits it via React-Router-v6 form:

```uitkx
<Routes>
    <Route path="/" element={<HomePage/>} />
    <Route path="/about" element={<AboutPage/>} />
</Routes>
```

After HMR, `RouteFuncProps.Element` becomes `null`, the router renders
nothing, every page disappears. Same applies to any component-defined
`VirtualNode`-typed prop (`header={<X/>}`, `fallback={<Y/>}`, etc.).

**Root cause.**
[`HmrCSharpEmitter.AttrToExpr` line 1923�1932](../Editor/HMR/HmrCSharpEmitter.cs#L1923):

```csharp
case "JsxExpressionValue":
    // TODO: emit nested JSX expression
    return "null /* jsx attr value */";
```

A literal stub. SG does the obvious thing �
[`CSharpEmitter.cs#L1726`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1726)
calls `EmitJsxToString(jsx.Element)` to recursively emit the embedded element
expression.

**Proposed fix.** Reuse the existing emit path. Inside this case, get the
`Element` AST node off the `JsxExpressionValue` (via
`UitkxHmrCompiler.GetProp(value, "Element")`), then recurse into a sub-emitter
that captures `EmitNode(element)` into a buffered `StringBuilder` and returns
the captured string. Be careful to share `_rentBuffer` / `_hoistedStyleFields`
with the parent emitter so any rentals/hoists from inside the nested JSX still
end up in the right place.

---

## Issue 7 � `Compilation.Emit` overload discovery is non-deterministic

(See Issue 3 above for the symptom.) **Adding here for completeness:** the
identical fragility class exists in three sibling discoveries that *currently
happen* to pick the right overload but have the same `.First()` race:

- `_emitToStream` ([UitkxHmrCompiler.cs#L817](../Editor/HMR/UitkxHmrCompiler.cs#L817))
  � confirmed broken.
- `_createFromFile` ([UitkxHmrCompiler.cs#L804](../Editor/HMR/UitkxHmrCompiler.cs#L804))
  � `MetadataReference.CreateFromFile` has 3 overloads; we currently land on
  the right one by luck.
- `_parseText` ([UitkxHmrCompiler.cs#L726](../Editor/HMR/UitkxHmrCompiler.cs#L726))
  � `CSharpSyntaxTree.ParseText` has 3+ overloads; we land on
  `(string, CSharpParseOptions, ...)` by luck.
- `_compilationCreate` ([UitkxHmrCompiler.cs#L803](../Editor/HMR/UitkxHmrCompiler.cs#L803))
  � picks "first 4-arg `Create`" � Roslyn has only one of those today, but
  this is a 1-method-add-away from breakage.

**Proposed fix.** A single helper `PickAllOptionalTailOverload(name, firstParamType)`
that filters by name and `firstParamType`, then prefers the overload where every
parameter from index 1 onward has `HasDefaultValue == true`. Falls back to the
shortest signature. Apply across all five reflection cache lookups.

---

## Issue 8 � `CheckIfGenuinelyNew` cross-reference detection is over-broad

**Severity.** Silent miscompile risk for genuinely-new components whose name
collides with a pre-existing type *anywhere in any loaded assembly*.

**Symptom (latent, not reproduced).** A user creates `App.uitkx` and
references it from another `.uitkx`. If any pre-existing assembly (Unity,
third-party, this package itself) already declares a type literally named
`App` (e.g. `UnityEngine.Networking.App`, `Microsoft.SqlServer.App`, custom
sample code), HMR considers the component **not genuinely new** and skips
adding it to the cross-reference list. Dependent compilations then fail to
resolve `App.Render` and fall over.

**Root cause.**
[`UitkxHmrCompiler.CheckIfGenuinelyNew` line 581�583](../Editor/HMR/UitkxHmrCompiler.cs#L581):

```csharp
foreach (var type in asm.GetExportedTypes())
{
    if (type.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase))
        return; // exists in a pre-existing assembly � NOT new
}
```

`type.Name` is the bare unqualified name. `OrdinalIgnoreCase` makes this
worse. No namespace check, no `IComponent`/render-shape check.

**Proposed fix.** Two layered constraints:
1. Match on the component's expected fully-qualified name
   `{InferredNamespace}.{componentName}` (build the inferred namespace the
   same way the emitter does), or
2. Restrict the "exists" check to types that actually have a matching
   `static VirtualNode Render(IProps)` � the SG-emitted shape � to filter
   out unrelated types that just share a name.

Either alone fixes the bug; both together is best.

---

## Issue 9 � `EmitFuncComponent` does not pool the synthesized props object

**Severity.** Performance regression vs cold build, semantically correct.

**Site.** [`HmrCSharpEmitter.cs#L992`](../Editor/HMR/HmrCSharpEmitter.cs#L992)
emits `new {propsTypeName} { ... }` for function-component invocations. SG
([`CSharpEmitter.cs#L968`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L968))
emits `BaseProps.__Rent<{propsTypeName}>()` + per-prop assignment + return-
to-pool. After HMR, every render allocates a fresh props object ? extra GC
pressure that wasn't there before the hot reload, only on touched components.

**Proposed fix.** Mirror `EmitTyped`'s `_rentBuffer` pattern: emit
`var __p_n = BaseProps.__Rent<T>(); __p_n.X = ...;` into `_rentBuffer`, then
emit `V.Func<T>(R, __p_n, key: k, ...)` using the local. Skip pooling if
`FindPropsType` returns the bare-fallback (since pool may not exist for
external Props types).

---

## Issue 10 � duplicate-key check (UITKX0010) is missing in HMR

**Severity.** Diagnostic divergence. Cold build emits a compile-time error
on duplicate sibling `key={...}` values; after HMR recompile the same code
silently reconciles incorrectly (wrong fiber matched to wrong key).

**Root cause.** `grep "UITKX0010\|CheckDuplicateKeys"` finds zero hits in
`Editor/HMR/`. SG emits the check inside `EmitChildArgs` / per-call-site
during emit. HMR `EmitChildArgs` skips it.

**Proposed fix.** Port the SG check inline. Cheap: walk children once,
collect literal-string `key` attribute values, raise a `#error UITKX0010`
on first duplicate. Skip when keys are non-literal C# expressions.

---

## Issue 11 � `InvokeWithDefaults` drift warning is too loud (cosmetic)

(See Issue 1 above; restating the action.) The warning is fine for
language-library calls but spammy for Roslyn calls where we *intentionally*
omit canonical-default trailing args. **Proposed fix:** add a
`bool silent = false` parameter; pass `silent: true` from the five
Roslyn-targeted invoke sites in `UitkxHmrCompiler.cs`. Drift warnings still
fire loud for the language-library call sites that matter.

---

## Issue 12 � `UitkxParser.Parse` call sites underspecify args (low-noise drift)

(See Issue 2 above.) `DirectiveParser.Parse` arity is 4 (`source, filePath,
diagnosticBag, useLastReturn=true`) � already aligned in HMR.
`UitkxParser.Parse` arity is 6 (`source, filePath, directives, diagnostics,
validateSingleRoot=false, lineOffset=0`); HMR passes 5 from
[lines 175, 202, 224, 226, 397](../Editor/HMR/UitkxHmrCompiler.cs#L175)
omitting `lineOffset`. Pad-to-zero is the correct semantic default for
freestanding files (no offset). **Proposed fix:** pass explicit `0` to
silence the drift warning; no behavior change.

---

## Out of scope (still)

- Implementing any of the fixes above; user direction stands ("don't fix
  anything yet").
- Touching SG. SG is the ground truth, 1070/1070 tests passing.

---

# HMR viability assessment

**Verdict: viable with targeted fixes � no structural rework required.**

The HMR pipeline (parse ? lower ? emit C# ? in-process Roslyn compile ?
delegate swap ? fiber rollback) is structurally sound. The fiber/state
preservation layer in particular is in good shape and is the hardest part
of the system to get right. The compiler bootstrap (Roslyn-via-reflection)
is wobbly but works; the in-process path's failure (Issue 7) is masked by
the external-`csc.dll` fallback.

The serious issues are concentrated in **the C# emitter's coverage of
attribute-value kinds**:

- **Issue 6** (`JsxExpressionValue` stub) � 1 case, 5 lines to fix.
- **Issue 5** (`ref={x}` dropped on func components) � needs ~30 lines of
  reflection-walked Props prop discovery.

These two together account for the user-visible PrettyUi regression and
likely most other "things vanish after I save" reports we'll see.

**Fix order (recommended):**

1. **Issue 6** � JSX-valued attributes. Highest impact, smallest fix. 5
   lines + recurse into `EmitNode`. **Unblocks all React-Router code.**
2. **Issue 5** � `ref={x}` reflection routing. Medium impact, ~30 lines.
   Unblocks all imperative-ref code after HMR.
3. **Issue 7** � Roslyn overload discovery hardening. Medium impact, ~20
   lines + 1 helper. Removes the "every save falls through to slow
   external compile" tax.
4. **Issue 8** � `CheckIfGenuinelyNew` over-broad match. Latent, needs a
   repro to confirm severity, but the fix is small. ~10 lines.
5. **Issue 10** � UITKX0010 in HMR. Diagnostic parity. ~25 lines.
6. **Issue 11 + Issue 12** � log-noise cleanup. Cosmetic. ~10 lines total.
7. **Issue 9** � props pooling for `EmitFuncComponent`. Perf only. ~15
   lines. Schedule when convenient, not blocking.

Estimated total surface to touch: **two files**
(`Editor/HMR/HmrCSharpEmitter.cs` and `Editor/HMR/UitkxHmrCompiler.cs`),
**~150 lines net**. No structural rework. No new abstractions.

**Long-term recommendation (out of scope for this audit, future ticket):**
HMR's hand-written transpiler will keep drifting from SG every time the
emitter learns a new trick. Three options to consider when next a divergence
bites:

- (a) extract the SG emitter into a portable assembly and have HMR
  reflection-call it instead of re-implementing � biggest payoff, biggest
  effort;
- (b) generate a parity-test corpus from SG and run HMR against the same
  inputs in the SG test suite � catches divergences in CI without sharing
  code;
- (c) accept the drift and add a "HMR coverage" checklist to every PR that
  touches `SourceGenerator~/Emitter/CSharpEmitter.cs`.

Option (b) gives the best cost/value ratio and would have caught Issues
5, 6, 9, 10 automatically.


---

# Post-audit verification pass (simulated implementation)

> **Mandate.** Walk through each proposed fix as if implementing it: read the
> exact code, check assumptions, check for breakage in adjacent paths. Any
> proposed fix that doesn't survive simulation is downgraded or replaced.

## TL;DR � verification verdict per issue

| #   | Original verdict           | After verification         | Change                                  |
| --- | -------------------------- | -------------------------- | --------------------------------------- |
| 1   | Cosmetic � silent flag     | ? Confirmed viable         | none                                    |
| 2   | Real drift � append `0`    | ? Confirmed viable         | none                                    |
| 3   | Real failure � picker      | ? Confirmed viable         | none                                    |
| 4   | Symptom of #6              | ? Confirmed (alias of #6)  | downgraded to "umbrella for #5/#6"      |
| 5   | Critical � ref routing     | ? Viable, simpler than thought | impl notes refined; needs `Ref<>` too |
| 6   | Critical � JsxExpressionValue | ? Viable                | impl notes refined                      |
| 7   | Hardening � picker         | ? Viable                   | none                                    |
| 8   | Latent silent miscompile   | ?? Severity downgraded     | "rare edge case", not "common silent"   |
| 9   | Perf regression            | ? **INVALID � RETRACTED**  | SG also uses `new`, not `__Rent`        |
| 10  | Diagnostic divergence      | ?? Code id corrected        | UITKX0104 (not 0010); `Debug.LogWarning`, not `#warning` |
| 11  | Cosmetic � silent flag     | ? Confirmed viable         | none                                    |
| 12  | Drift � append `0`         | ? Confirmed viable         | none                                    |

---

## Issue 5 � ref={x} routing � VERIFIED VIABLE (impl refined)

**Verification.**

- Read [`HmrCSharpEmitter.FindPropsType` (line 1063)](../Editor/HMR/HmrCSharpEmitter.cs#L1063):
  it returns a **string** (the formatted type-name to emit), not a `Type`.
  My original proposal "scan its public instance properties" needs that
  `Type`. **Refactor required:** add a sibling `FindPropsTypeReflected(string typeName)`
  returning `Type` and the cached `RefSlotName`.
- Read [`Shared/Core/ReactiveTypes.cs:74`](../Shared/Core/ReactiveTypes.cs#L74)
  and [`Shared/Core/Hooks.cs:30`](../Shared/Core/Hooks.cs#L30): `Ref<T>` is the
  primary type today. `Hooks.MutableRef<T>` exists but is `[Obsolete]`. SG
  ([`PropsResolver.cs:166-170`](../SourceGenerator~/Emitter/PropsResolver.cs#L166))
  matches **both**. HMR fix must too: `genDef == typeof(Ref<>) || genDef == typeof(Hooks.MutableRef<>)`.
- Read [`SourceGenerator~/Emitter/CSharpEmitter.cs:1175`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1175):
  SG emits `{refPropName} = {AttrVal(refAttr.Value)}` � **no cast**. The
  user's local is already typed `Ref<X>` from `UseRef<X>()`, the compiler
  infers. HMR fix is simpler than my earlier "30 lines + cast generic-arg
  resolution"; it's ~25 lines and no cast.

**Adjacent breakage check.**

- The `FilterAttrs(filteredAttrs, "ref")` at [HmrCSharpEmitter.cs#L986](../Editor/HMR/HmrCSharpEmitter.cs#L986)
  must stay: we still want `ref` removed from the literal-prop loop after
  routing, otherwise we'd emit `Ref = ...` twice.
- The no-props branch (`else { _sb.Append("V.Func(...)"); }`) at line 1011
  needs symmetric handling: if `ref={x}` is present on a no-props component,
  SG emits `UITKX0020` (RefOnComponentWithNoRefParam). HMR could either
  silently drop (current behavior, runtime no-op) or emit a `#warning`. SG
  parity says diagnostic, but this is an authoring-error case, not a hot-path
  miscompile. **Recommend: `Debug.LogWarning` at HMR emit time, low priority.**
- `FindPropsTypeReflected` reflection scan happens **per HMR recompile per
  call site**. The existing `FindPropsType` already does this scan and
  doesn't cache. **Add a small `ConcurrentDictionary<string, (string Name, Type T, string RefSlot)>`
  cache keyed by `typeName`.** Invalidate on `Reset()`.

**Refined fix.** ~40 lines net (cache + reflection helper + EmitFuncComponent
edit). Still small. **Confirmed viable.**

---

## Issue 6 � JsxExpressionValue stub � VERIFIED VIABLE (impl refined)

**Verification.**

- Read AST type:
  [`ide-extensions~/language-lib/Nodes/AstNode.cs:93`](../ide-extensions~/language-lib/Nodes/AstNode.cs#L93):
  `public sealed record JsxExpressionValue(ElementNode? Element) : AttributeValue;`
  ? reflection access via `GetProp(value, "Element")` ? element AST node.
- Read SG's `EmitJsxToString`
  ([CSharpEmitter.cs:1735](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1735)):

  ```csharp
  if (element == null) return $"({QVNode})null";
  int startLen = _sb.Length;
  EmitElementNode(element);
  string result = _sb.ToString(startLen, _sb.Length - startLen);
  _sb.Length = startLen;
  return result;
  ```

  The `_sb`-capture pattern is HMR-compatible 1:1 (HMR also has a
  member-scoped `_sb`).
- Check `_isRootElement` ([HmrCSharpEmitter.cs:125](../Editor/HMR/HmrCSharpEmitter.cs#L125)):
  starts true, becomes false on first `EmitElement`. By the time we hit any
  `JsxExpressionValue` (which is an attribute on something that came AFTER
  the root), it's already false. **No state corruption.**
- Check `_rentBuffer` / `_hoistedStyleFields`: shared across the recursion
  is the **correct** behavior � Style.__Rent / hoisted-style statements from
  inside the nested JSX must end up in the parent's pre-return rent block,
  not get lost. Already what the proposed fix does.

**Adjacent breakage check.**

- Recursive `EmitElement` from inside `AttrToExpr` is reentrant-safe: no
  static state, no class-level mutable globals beyond the buffers we want
  shared.
- A nested element that is itself a function component will route through
  `EmitFuncComponent` ? `FindPropsType` (per call). The reflection cost
  of `FindPropsType` is per-element, not per-render. Same cost as a
  same-shape top-level call. **No new perf hot path.**

**Refined fix.** ~10 lines (replace stub with the SG-style capture). Still the
smallest fix in the audit. **Confirmed viable; highest priority.**

---

## Issue 7 � Roslyn overload picker � VERIFIED VIABLE

**Verification.**

- Re-read all 5 reflection sites in [`UitkxHmrCompiler.CacheReflectionHandles`](../Editor/HMR/UitkxHmrCompiler.cs#L513):
  - `_directiveParse` � `GetMethod("Parse", BindingFlags.Public | BindingFlags.Static)` �
    safe (only one `Parse`).
  - `_uitkxParse` � same, safe.
  - `_canonicalLower` � same, safe.
  - `_parseDiagnosticType` � type lookup, safe.
- Re-read all 5 Roslyn reflection sites in `TryLoadRoslyn`:
  - `_compilationCreate` (line 803) � `Length == 4` filter; only one Roslyn
    overload of `CSharpCompilation.Create` has 4 params today. Safe-by-luck.
  - `_createFromFile` (line 804) � `first param == typeof(string)`; canonical
    overload has all-optional tail.
  - `_emitToStream` (line 817) � `first param == typeof(Stream)`; **proven
    broken** in user's environment.
  - `_parseText` (line 726) � `first param == typeof(string)`; canonical
    overload has all-optional tail.
  - `WithNullableContextOptions`, `WithOptimizationLevel`, `WithAssemblyName`,
    `RemoveSyntaxTrees`, `AddSyntaxTrees`, `AddReferences` � all use
    name + arity-1 filters. Safe-by-luck for all but
    `WithAssemblyName` which already constrains by `paramType == string`.
- The proposed `PickAllOptionalTailOverload(name, firstParamType)` helper
  with prefer-all-default-tail / fallback-to-shortest works for the four
  fragile sites: every canonical overload of `Emit(Stream)`,
  `CreateFromFile(string)`, `ParseText(string)`, `Create(string,...)` has
  all-optional tail starting from index 1. **Safe across Roslyn 4.x.**

**Adjacent breakage check.**

- `_emitToStream` is the only currently-failing one. Fixing it might surface
  a new latent bug if other code paths assumed the old `pdbStream`-required
  overload. None do � the only caller is
  [`UitkxHmrCompiler.cs#L926`](../Editor/HMR/UitkxHmrCompiler.cs#L926) and
  it passes a single `MemoryStream peStream`.
- For `_parseText` the call site passes `(src, _parseOptions)` � works
  unchanged regardless of which overload is picked, because both candidate
  overloads accept `(string, CSharpParseOptions, ...)`.

**Confirmed viable. ~25 lines net (helper + 4 call-site updates).**

---

## Issue 8 � `CheckIfGenuinelyNew` � VIABLE BUT SEVERITY DOWNGRADED

**Verification.**

- Re-read [`UitkxHmrCompiler.CheckIfGenuinelyNew`](../Editor/HMR/UitkxHmrCompiler.cs#L570)
  AND [`BuildCrossRefs`](../Editor/HMR/UitkxHmrCompiler.cs#L1063).
- The actual purpose: when a `.uitkx` is **already cold-built into a
  project assembly**, do NOT add HMR-emitted DLL as cross-ref (that would
  cause CS0433 type-defined-in-multiple-assemblies). When it's **genuinely
  new**, DO add it so dependents can resolve it.
- The bug fires only when **both** are true:
  1. The user creates a brand-new `App.uitkx` (no cold build yet).
  2. Some unrelated assembly happens to have a public type named `App`
     (case-insensitive bare-name match).
- Realistic case: Unity's own `App` types, `System.Web.HttpApplication.App`,
  etc. Possible but rare in practice.
- The HMR-rendered live tree primarily uses delegate-swap
  (`UitkxHmrDelegateSwapper.WalkFiber`), not cross-refs, so the bug
  doesn't affect the *running* tree. It affects **dependents that need to
  resolve the new type at compile time**, which happens only when the user
  has a chain of brand-new components referencing each other.

**Adjacent breakage check.**

- Proposed FQN match (`{InferredNamespace}.{componentName}`) requires HMR
  to know the **same** inferred namespace SG uses. Already known via
  `directives.Namespace` (read at [UitkxHmrCompiler.cs:188](../Editor/HMR/UitkxHmrCompiler.cs#L188)).
  Pass it to `CheckIfGenuinelyNew(componentName, expectedNs)`.
- Proposed shape-check (look for `static VirtualNode Render(IProps)`)
  requires reflection on every type in every assembly � expensive. Skip.
- Performance: `GetExportedTypes()` already loops every type per assembly
  per check. Adding FQN comparison is O(1) extra. No regression.

**Severity downgrade.** From "Critical / Silent miscompile risk" to
"Edge-case / Latent". **Fix order moved DOWN to #6 in the priority list.**
Still ~10 lines.

---

## Issue 9 � props-object pooling � INVALID, RETRACTED

**Verification.**

- Read [`SourceGenerator~/Emitter/CSharpEmitter.cs:1148`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1148):

  ```csharp
  _sb.Append($"V.Func<{propsTypeName}>({typeName}.Render, new {propsTypeName} {{");
  ```

  **SG also uses `new`** for function-component prop instantiation �
  identical to HMR.
- Read [`SourceGenerator~/Emitter/CSharpEmitter.cs:1280`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1280):
  the synthetic props class is `public sealed class {className} : global::ReactiveUITK.Core.IProps`
  � `IProps`, not `BaseProps`.
- Read [`Shared/Props/Typed/BaseProps.cs:870`](../Shared/Props/Typed/BaseProps.cs#L870):

  ```csharp
  public static T __Rent<T>() where T : BaseProps, new() { return Pool<T>.Rent(); }
  ```

  The pool is **constrained to `BaseProps`**. Synthetic props classes do
  NOT derive from `BaseProps` and **cannot be pooled**.
- Read [`SourceGenerator~/Emitter/CSharpEmitter.cs:934`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L934)
  comment: *"ErrorBoundaryProps extends IProps (not BaseProps) � cannot be pooled"*.
  Confirms the design intent: pooling is only for typed-element props.

**Verdict.** HMR's `EmitFuncComponent` `new {PropsType} { ... }` is
**correct parity with SG**. There is no "perf regression vs cold build"
because cold build does the same thing. **Issue 9 retracted.**

---

## Issue 10 � UITKX0104 duplicate-key � VIABLE (impl refined)

**Verification.**

- The SG diagnostic id is **UITKX0104** (the original audit said `UITKX0010`
  � typo; UITKX0010 doesn't exist).
- Read [`UitkxDiagnostics.cs:179`](../SourceGenerator~/Diagnostics/UitkxDiagnostics.cs#L179):
  `defaultSeverity: DiagnosticSeverity.Warning`. **Warning, not Error.**
  Cold build does NOT fail on duplicate keys; it warns and the reconciler
  runs (with whatever wrong matches the duplicate keys produce).
- Read [`SourceGenerator~/Emitter/CSharpEmitter.cs:1631`](../SourceGenerator~/Emitter/CSharpEmitter.cs#L1631):
  `CheckDuplicateKeys` walks children, collects `StringLiteralValue` keys
  in a `HashSet<string>`, and adds a Roslyn `Diagnostic` on first duplicate.
- The original audit's proposed `#error UITKX0010` is **wrong on two
  counts**: (a) wrong code id, (b) wrong severity (would hard-fail HMR
  recompile when SG only warns). Refine to `Debug.LogWarning(...)` at HMR
  emit time, mirroring SG's per-build single-warning behavior.

**Adjacent breakage check.**

- Walking children once is O(n) per call site; called from `EmitChildArgs`
  per JSX scope. Negligible cost.
- `Debug.LogWarning` from inside `HmrCSharpEmitter` is fine � the emitter
  already has access to UnityEngine via the Editor asmdef.

**Refined fix.** ~20 lines (port `CheckDuplicateKeys` + call from each
of the existing `EmitChildArgs` call sites OR call it once at the top of
each container emit). **Confirmed viable; low priority.**

---

## Issues 1, 2, 11, 12 � fully verified, no changes

- **Issues 1 + 11** (silent flag for Roslyn drift): read
  [`InvokeWithDefaults`](../Editor/HMR/UitkxHmrCompiler.cs#L1395). Adding a
  trailing `bool silent = false` parameter is mechanical; pass `silent: true`
  from the four Roslyn-targeted invoke sites (`_createFromFile`,
  `_parseText`, `_compilationCreate` if it ever pads, `_emitToStream`).
  Drift warnings still fire for `_directiveParse`, `_uitkxParse`,
  `_canonicalLower` � the language-library calls that genuinely matter.
  ~10 lines. Confirmed viable.

- **Issues 2 + 12** (UitkxParser arity): confirmed
  [`UitkxParser.Parse`](../ide-extensions~/language-lib/Parser/UitkxParser.cs#L75)
  is `(source, filePath, directives, diagnostics, validateSingleRoot=false, lineOffset=0)`,
  6 params. HMR call sites
  ([line 202](../Editor/HMR/UitkxHmrCompiler.cs#L202) and
  [line 226](../Editor/HMR/UitkxHmrCompiler.cs#L226)) pass `false` as 5th
  arg = `validateSingleRoot=false` (correct for HMR re-parsing) and omit
  `lineOffset`. The omission is padded to default `0`, which is the correct
  semantic for freestanding files. Append explicit `, 0` to silence the
  drift warning. **No behavior change.** ~4 char-edits across 2 lines.

---

## Revised fix-order recommendation

After verification:

1. **Issue 6** � JsxExpressionValue (~10 lines) � **PrettyUi unblocker**.
2. **Issue 5** � ref={x} routing (~40 lines, includes `Type`-cache helper).
3. **Issue 7** � Roslyn overload picker (~25 lines).
4. **Issues 11 + 12** � drift-noise cleanup (~15 lines total).
5. **Issue 10** � UITKX0104 parity in HMR via Debug.LogWarning (~20 lines).
6. **Issue 8** � `CheckIfGenuinelyNew` FQN match (~10 lines).
7. ~~**Issue 9** � props pooling~~ � **RETRACTED**, no fix needed.

**Total revised surface: ~120 lines** across the same two HMR files. No
structural rework. No new abstractions. Issues 5+6 alone fix the
user-visible PrettyUi failure mode.

## Verification process notes

- Every "proposed fix" claim in the original audit was re-checked against
  actual code, not against assumed code.
- Issue 9 caught me out � I assumed SG had a different code path for
  function components than typed elements. It doesn't. **Lesson:** when
  comparing SG vs HMR, read **the same code path** in both, not the
  generally-correct-sounding one.
- Issue 10 had wrong diagnostic id (typo). **Lesson:** grep the actual
  `UitkxDiagnostics.cs` registry before quoting an id.
- Issue 8 severity claim was high-impact but the actual blast radius is
  small once you trace through `BuildCrossRefs` + `_genuinelyNewComponents`
  semantics. **Lesson:** trace the *consumers* of a flag before declaring
  the producer broken.


---

# Implementation status (v0.4.19)

All actionable issues from the audit + verification pass have been
implemented. SG-parity contract suite added as a long-term drift tripwire.
Baseline preserved: 1097 / 1135 tests passing (was 1092 / 1130 � exactly
+5 new tests added; same 38 pre-existing PrettyUi formatter snapshot
mismatches unrelated to HMR).

| # | Issue | Status | Site of fix | Pinned by |
|---|-------|--------|-------------|-----------|
| 1 | Spurious 'API drift' warnings on Roslyn calls | IMPLEMENTED | Editor/HMR/UitkxHmrCompiler.cs _silentDriftMethods set + RegisterSilentDrift for `_parseText`, `_compilationCreate`, `_createFromFile`, `_emitToStream` | manual inspection (cosmetic) |
| 2 | Genuine drift on UitkxParser.Parse | IMPLEMENTED | UitkxHmrCompiler.cs both `_uitkxParse` call sites now pass explicit `lineOffset: 0` | Sg_DistinctSiblingKeys_NoUitkx0104 (proves parser invocation works on multi-key trees) |
| 4 | Children disappear after HMR-triggered recompile | UMBRELLA � resolved by #5 + #6 | n/a | n/a |
| 5 | `<MyFuncComponent ref={x}>` silently dropped | IMPLEMENTED | Editor/HMR/HmrCSharpEmitter.cs `FindPropsTypeAndRefSlot` + `FindRefSlotName` (matches both `Ref<T>` and `Hooks.MutableRef<T>`); `EmitFuncComponent` extracts `refExpr` before attribute filter | Sg_RefOnFuncComponent_RoutesIntoPropsSlot |
| 6 | `<Component prop={<Jsx/>}>` collapses to `null` | IMPLEMENTED | HmrCSharpEmitter.cs `AttrToExpr` `JsxExpressionValue` case uses `_sb`-capture pattern (mirrors SG `EmitJsxToString`) | Sg_JsxAsAttributeValue_EmitsNestedElement |
| 7 | `Compilation.Emit` overload discovery non-deterministic | IMPLEMENTED | UitkxHmrCompiler.cs `PickAllOptionalTailOverload` helper applied to `_parseText`, `_compilationCreate`, `_createFromFile`, `_emitToStream` | manual inspection (overload picker is deterministic by construction; would require a fragile reflection harness to pin) |
| 8 | `CheckIfGenuinelyNew` cross-reference detection over-broad | IMPLEMENTED | UitkxHmrCompiler.cs `CheckIfGenuinelyNew(componentName, expectedNamespace)` � builds FQN, case-sensitive `type.FullName` match (was bare `type.Name` `OrdinalIgnoreCase`); call site passes `ns` from directives | manual inspection (would require Editor host to construct `AppDomain` peers) |
| 9 | `EmitFuncComponent` does not pool synthesized props | RETRACTED � invariant pinned | n/a | Sg_FuncComponentInvocation_UsesNewNotRent (asserts SG never emits `__Rent` for function-component props; if SG ever changes, HMR must follow) |
| 10 | UITKX0104 duplicate-key check missing in HMR | IMPLEMENTED | HmrCSharpEmitter.cs `CheckDuplicateKeys` called at top of `EmitChildArgs`; emits `Debug.LogWarning` with id `UITKX0104` (severity matches SG: Warning, not Error) | Sg_DuplicateSiblingKey_RaisesUitkx0104 + Sg_DistinctSiblingKeys_NoUitkx0104 |
| 11 | `InvokeWithDefaults` drift warning too loud | IMPLEMENTED | covered by #1 (silent flag) � language-library calls (`_directiveParse`, `_uitkxParse`, `_canonicalLower`) NOT silent so genuine drift surfaces loud | n/a |
| 12 | `UitkxParser.Parse` call sites underspecify args | IMPLEMENTED | covered by #2 � both call sites pass full positional arg list including `lineOffset: 0` | n/a |

## SG-parity contract corpus (long-term)

New test file: `SourceGenerator~/Tests/HmrEmitterParityContractTests.cs`
(5 xUnit `[Fact]` tests). Pattern mirrors the existing
`HmrFindPropsTypeContractTests` � runs SG against tiny `.uitkx` fixtures
and asserts the marker substrings / diagnostic ids HMR's hand-written
emitter must mirror.

These tests are **one-way drift tripwires**: SG is the ground truth. If SG
ever changes the emit shape (e.g. renames `V.Label` to `V.Element`, or
switches from `new GreeterProps` to `__Rent<GreeterProps>`), the test
fails and the corresponding HMR site must be reviewed for parity. The tests
cannot load HMR directly � Editor asmdef pulls in `UnityEditor` which is
not loadable from a standalone .NET test runner.

Tests:
1. `Sg_RefOnFuncComponent_RoutesIntoPropsSlot` � pins `InputRef = myRef` for Issue 5.
2. `Sg_JsxAsAttributeValue_EmitsNestedElement` � pins `Header = V.Label(` for Issue 6.
3. `Sg_DuplicateSiblingKey_RaisesUitkx0104` � pins `UITKX0104` diag id for Issue 10.
4. `Sg_DistinctSiblingKeys_NoUitkx0104` � negative-side of Issue 10 (no false positive).
5. `Sg_FuncComponentInvocation_UsesNewNotRent` � pins Issue 9 retraction.

## Out of scope (intentionally)

- Issue 3 � superseded by Issue 7 (same root cause, larger fix).
- The 38 pre-existing `FormatterSnapshotTests.RoslynIdempotency_SampleFile_IsUnchanged` failures
  are PrettyUi-sample formatter snapshot mismatches unrelated to HMR � currently
  being addressed in the user's parallel `Samples/UIs/PrettyUi/UI/AppRoot.style.uitkx` editing.


---

# Issue 13 � module `static readonly` fields not re-initialised on HMR — RESOLVED (B28)

**Discovered:** 2026-05-03 during manual validation of v0.4.19 fixes against
PrettyUi. The 12 prior issues were all for the component / hook / Roslyn-host
paths; this is the analogous gap for the `module` declaration path.

**Resolution.** See [`B28_HMR_STATIC_READONLY_FIX.md`](B28_HMR_STATIC_READONLY_FIX.md).
Root cause: Mono JIT inlines `ldsfld` reads of `initonly` static fields into
native code, so `FieldInfo.SetValue` updates the slot but pre-JIT'd callers
keep reading the cold reference. Fix: SG (and the parallel HMR emitter)
emit module-scope static fields without `readonly`, decorating them with
`[ReactiveUITK.UitkxHmrSwap]` as the discriminator the swapper uses to find
generator-managed mutable statics. The hook-cache `static readonly` field
(ref-identity immutable; only its contents are replaced) is preserved and the
swapper predicate still matches it via `IsInitOnly`. Analyzer **UITKX0210**
warns when user code writes to `[UitkxHmrSwap]`-marked fields outside the
static constructor. Test coverage: 1218/1218 pass including 20 new tests
(stripper unit tests, analyzer tests, end-to-end module strip tests).

**Symptom (Play mode, HMR enabled).**
1. Author a `module` with at least one `public static readonly` field
   (e.g. a `Style` constant in a `.style.uitkx` companion).
2. Edit the field's initializer expression � for example, delete a
   `BackgroundColor = Theme.BgPanel,` entry � and save.
3. HMR reports a successful swap (`[HMR] {Component} updated`), but the
   rendered UI still shows the *old* field value (background reappears).
4. Exiting Play mode and re-entering � which forces a full Unity assembly
   reload � picks up the new value, confirming the source on disk is correct
   and the bug lives in the in-process HMR field-binding pipeline.

**Diagnosis.**
`module {Name} { � }` is emitted (by both `ModuleEmitter.Emit` and
`HmrHookEmitter.EmitModules`) as `public partial class {Name} { �user body verbatim� }`.
The user's `public static readonly Style Root = new Style { � }` becomes a
plain CLR `static readonly` field bound by the type's cctor at first load.

When the project is cold-built, the project assembly's `{Name}` cctor runs
once and pins the field. When HMR rebuilds a *new* dynamic assembly, that
new assembly's `{Name}` cctor runs against the new initializer expression
and produces fresh values bound to the *HMR* type's fields � but
`UitkxHmrDelegateSwapper.SwapAll` and `SwapHooks` only re-bind fields
whose names start with `__hmr_*` (the synthesized delegate slots for
hooks and component renders). User-named module fields (`Root`, `BgColor`,
�) are never copied across the assembly boundary, so the project type's
`static readonly` field permanently references the cold-build instance.

**Fix shape � Option C (cross-assembly static field copy), implemented.**
After a successful HMR compile, iterate every type in the HMR assembly,
locate the project-side type with the same `FullName`, and for each
`static readonly` field on the HMR type copy `hmrField.GetValue(null)`
into the project-side `FieldInfo.SetValue` (which bypasses the readonly
runtime check � same mechanism the BCL uses for record InitOnly support).

Considered alternatives:

  - **Option A � synthesize a `__hmr_reinit_module()` method** that
    re-runs the field initializers when called. Would require parsing
    the freeform user C# inside the module body at HMR-emit time and
    rewriting it into discrete field assignments � a Roslyn rewrite
    with parity risk against `ModuleEmitter` and no advantage over
    Option C, since the HMR cctor already produces the values for free.

  - **Option B � re-run the project type's cctor.**
    `RuntimeHelpers.RunClassConstructor` is documented as no-op on
    second call (CLR enforces type-initializer-runs-once); invoking the
    cctor `MethodInfo` directly would re-fire any side-effects the
    user wrote in the module body (event subscriptions, registry pushes
    �) ? duplicate handlers, corrupt state. Option C side-steps this.

**Safety guardrails (see `UitkxHmrModuleStaticSwapper.cs` for source).**

  - Only `FieldInfo.IsInitOnly == true` (i.e. `static readonly`) fields
    are copied. **Mutable** `static` fields are preserved � they
    represent user runtime state and clobbering would be a regression.
  - `IsLiteral` (i.e. `const`) is skipped � no runtime field slot.
  - `__hmr_*` fields are skipped � owned by `UitkxHmrDelegateSwapper`.
  - Compiler-generated types (`<Module>`, `<>c__DisplayClass*`) are
    skipped.
  - Each field copy is wrapped in a per-field try/catch so one bad
    cross-assembly type-identity mismatch (rare; only happens if the user
    defines a nested type inside the module body) cannot break the rest
    of the HMR cycle.
  - Conservative `AreCompatibleFieldTypes` check requires same
    `FullName` AND same defining assembly � guards against `SetValue`
    throwing when the user changed a field's type during edit.
  - Called BEFORE delegate swap so that when the new render delegate
    first executes, the static fields it reads are already the new values.

**Implementation sites.**

  - New file `Editor/HMR/UitkxHmrModuleStaticSwapper.cs` � the swapper
    + extensive design notes / safety rationale at the top.
  - `Editor/HMR/UitkxHmrController.cs` `ProcessFileChange` � calls
    `SwapModuleStatics` immediately after compile-success and before
    delegate swap; surfaces the count in the existing
    `[HMR] {Component} updated` log line via `| Module statics re-init: N`.

**Out-of-scope (intentional follow-ups).**

  - Static **methods** in modules (e.g. `StyleExtensions.Extend(...)`)
    � would need synthesized `__hmr_*` delegate fields like hooks/
    components have. Currently changes to module static methods only
    take effect after a full assembly reload. Track separately if a
    user reports it.

  - **Newly added** `static readonly` fields the user introduces during a
    session � they exist in the HMR assembly but have no slot in the
    project type. Anything compiled fresh against the HMR assembly sees
    them; the project type does not. A full assembly reload is required
    to materialise them. Same constraint applies to *any* IDE-grade hot
    reload (.NET's `Hot Reload`, Edit and Continue) � listed for
    transparency, not as a HMR defect.

**Status:** ? IMPLEMENTED for v0.4.19.


---

# v0.4.20 follow-up plan — module HMR completeness

Investigation conducted 2026-05-03 on the three follow-ups left "out of scope"
under Issue 13. Goal: convert the rough notes into concrete production-grade
proposals before any code is written. **No code changes yet — proposals only,
awaiting user approval.**

## Research summary (what was verified)

1. **Hooks already solve the moral equivalent of (a).**
   `SourceGenerator~/Emitter/HookEmitter.cs:130–160` synthesises a
   `__hmr_<name>` delegate field, a public trampoline guarded by
   `#if UNITY_EDITOR && HmrState.IsActive`, and a private `__<name>_body`
   method. Zero overhead in player builds; one delegate-call indirection
   in HMR-active Editor builds. This is the canonical pattern to mirror.

2. **Module bodies are stored as raw C# text.**
   `ide-extensions~/language-lib/Parser/ParseResult.cs` — `ModuleDeclaration`
   exposes only `(Name, Body, DeclarationLine, BodyStartLine, BodyStartOffset,
   BodyEndOffset)`. `Body` is opaque text — the parser does NOT split fields
   from methods. To rewrite per-method we MUST parse with Roslyn at SG-emit
   time (and mirror the same parse on the HMR side).

3. **Roslyn 4.3.1 is already loaded** by both SG (`<PackageReference>` in
   `SourceGenerator~/ReactiveUITK.SourceGenerator.csproj`) and the HMR
   pipeline (reflection-loaded via `Editor/Data/DotNetSdkRoslyn/`).
   No new dependency required for any proposal below.

4. **30 % of static module methods in the samples use `ref/out/in/params`.**
   PowerShell scan: 134 static methods total; 40 use a by-ref or `params`
   parameter — almost all in `Samples/Components/DoomGame/GameLogic.uitkx`
   (`public static void Tick(ref GameState st, …)` and friends).
   `Func<>`/`Action<>` cannot express by-ref parameters. A solution that
   silently drops these would break the most performance-critical user code.
   ⇒ The fix MUST emit **custom delegate types**, not `Func<>`/`Action<>`.

5. **Mutable statics in samples are runtime caches** (e.g. `DoomTextures.
   _walls`, `_built`). Resetting them on every HMR cycle would force a full
   texture re-load — a gameplay-killing regression. The current swapper
   correctly skips them via `!FieldInfo.IsInitOnly`. This matches .NET Hot
   Reload, Edit-and-Continue, and JS HMR semantics across the industry.

---

## (a) Static methods in modules don't hot-reload — REAL bug, large fix

### Production-grade design — synthesised custom-delegate trampolines per method

For every static method declared inside a `module { … }` body, the SG
(and the HMR-side mirror in `HmrHookEmitter.EmitModules`) emits four
artefacts instead of the current verbatim body:

```csharp
// 1. Custom delegate type — handles ref/out/in/params natively (Func<> can't).
private delegate void __GameLogic_Tick_8f3a_Delegate(ref GameState st, float dt, InputCmd input);

// 2. Per-method delegate field — initialised to the body method (no startup cost).
#if UNITY_EDITOR
[EditorBrowsable(EditorBrowsableState.Never)]
internal static __GameLogic_Tick_8f3a_Delegate __hmr_Tick_8f3a = __Tick_body_8f3a;
#endif

// 3. Public trampoline — preserves the ORIGINAL signature so callers don't change.
public static void Tick(ref GameState st, float dt, InputCmd input)
{
#if UNITY_EDITOR
    if (HmrState.IsActive) { __hmr_Tick_8f3a(ref st, dt, input); return; }
#endif
    __Tick_body_8f3a(ref st, dt, input);
}

// 4. Body method — original user code, with #line mapping preserved.
private static void __Tick_body_8f3a(ref GameState st, float dt, InputCmd input)
{
#line 60 "GameLogic.uitkx"
    /* original body */
#line hidden
}
```

Where `8f3a` is a stable hash of the parameter type list — disambiguates
overloads (e.g. `Foo(int)` vs `Foo(string)`).

A new HMR swapper (analogous to `UitkxHmrModuleStaticSwapper` but for
methods) walks every `__hmr_*` field on the project type, locates the
matching `__<name>_body_<hash>` method on the HMR-compiled type, builds
a delegate via `Delegate.CreateDelegate(fieldType, hmrBodyMethod)`, and
assigns it.

### Coverage matrix

| Method shape | Works? | Mechanism |
|---|---|---|
| `static void Foo(int)` | ✅ | Custom `void(int)` delegate |
| `static T Foo<T>(T)` (generic) | ✅ | `MethodInfo` field + `ConcurrentDictionary<Type, Delegate>` cache (mirror generic-hook pattern in `HookEmitter`) |
| `static void Foo(ref T)` / `out T` / `in T` | ✅ | Custom delegate supports by-ref naturally |
| `static void Foo(params T[])` | ✅ | Custom `void(T[])` delegate; `params` is call-site sugar |
| `static int Foo(int x = 0)` | ✅ | Defaults are call-site sugar; trampoline preserves them |
| `static async Task Foo()` | ✅ | `delegate Task()` — async returns are normal delegates |
| Iterator (`yield return`) | ✅ | State machine lives in body method, opaque to delegate |
| Multiple overloads of same name | ✅ | Param-list hash disambiguator in field name |
| `unsafe` / pointer params | ✅ | Custom delegate accepts pointer types |

### Explicitly NOT supported (with mitigations)

| Shape | Why | Mitigation |
|---|---|---|
| Instance methods inside a module | Modules are static-utility containers; instance dispatch needs a `this`-bound delegate per fiber instance | SG diagnostic `UITKX0150: instance methods inside 'module {Name}' are not HMR-supported; declare static or move to a partial class .cs file` |
| Properties (auto / with bodies) | Accessor-pair rewrite has poor cost/benefit; `static readonly` already covered by Issue 13 swapper | Skip; tell users to prefer `static readonly` for HMR-able values |
| Operator overloads / conversions | CLR-mandated signatures; call-site lookup is by signature, not user-named method | Skip — full assembly reload required |
| `virtual` / `override` / `abstract` | Vtable slot, not a static method — modules don't emit these anyway | N/A |
| Methods inside nested types of a module | Unbounded recursion of the rewrite; near-zero real-world incidence | SG diagnostic `UITKX0151` |

### Cost / risk

- **Effort:** SG-side new emitter (~400 LOC) + HMR-side mirror (~300 LOC) + 8–12 SG-parity tests + manual DoomGame HMR validation. Realistic estimate: ~1 focused day.
- **Risk:** custom-delegate emission is well-trodden territory. The new piece is the deterministic param-list hash for overload disambiguation — must be stable across runs (use a fingerprint string like `_RefGameState_Single_InputCmd` rather than a randomised hash; readable in stack traces is a bonus).
- **Performance:** one delegate call per HMR-active call site in Editor only. Zero overhead in player builds (`#if UNITY_EDITOR` strips everything).
- **Compatibility:** public method signatures preserved → existing callers unchanged at source and binary level.

### Recommendation
Implement. Skip the four "not supported" shapes with clear SG diagnostics rather than silent failure.

---

## (b) Mutable static fields preserved across HMR — NOT A BUG (UX work only)

### Why no behavioural change

There is **no signal** in the source distinguishing "the user changed
`score = 0` → `score = 100` because they want a reset" from "the user
edited a comment near the field declaration". Any clobbering interpretation
would destroy live game state on every save. Every mainstream hot-reload
tool (.NET Hot Reload, EnC, JS HMR) preserves mutable state by design.

### Work to do

1. **Documentation paragraph** in the HMR README / CHANGELOG describing
   the contract: `static readonly` and `const` re-init on save; mutable
   `static` fields preserve their runtime value across HMR cycles; exit
   Play mode to reset.

2. **Optional one-time discoverability log** — when the swapper detects
   that the *initializer source text* of a mutable static field changed
   (track a per-session SHA of each module file), log once per session:

   ```
   [HMR] Note: 'DoomTextures._walls' mutable static field's initializer
         changed. Mutable statics preserve their runtime value across HMR —
         exit Play mode to reset. (Shown once per session.)
   ```

### Cost / risk
~30 min docs + ~1 hour for the optional discoverability log. Zero behavioural risk.

### Recommendation
Document the contract. Add the optional log if we agree it's helpful.
Do NOT change the swap behaviour.

---

## (c) Newly-added `static readonly` fields silently stale — REAL bug, partial fix is the only honest option

### CLR constraint

Type metadata is **sealed at load time**. You cannot add a new field to an
already-loaded type via reflection or any other documented API. .NET Hot
Reload classifies "add a field to an existing type" as a **rude edit** and
either rejects it or surfaces a balloon warning in Visual Studio. They've
spent years on this and have not solved it because it is a CLR-level
constraint, not a tooling gap.

### Practical RUITK impact

The HMR-compiled assembly DOES have the new field. Code recompiled by HMR
(the user's render delegate) sees it. But pre-existing IL in
`Assembly-CSharp.dll` (compiled at cold-build time) references the project
type — which lacks the new field — so any access from non-HMR code throws
`MissingFieldException` or, in pathological layouts, returns stale data.

### Production-grade compromise

1. **Detect** in `UitkxHmrModuleStaticSwapper.SwapModuleStatics`: for each
   project↔HMR module type pair, compute
   `(HMR static-readonly field set) − (project static-readonly field set)`.
   If non-empty, the user added new fields this session.

2. **Log once per `(typeFullName, fieldName)` per session:**

   ```
   [HMR] 'Theme' has 1 newly-added field(s) since cold build: 'NewAccentColor'.
         CLR cannot add fields to a loaded type. References from this HMR
         cycle's render delegate may work, but references from non-HMR code
         (.cs scripts, compiled Assembly-CSharp) will see the old type.
         To materialise everywhere, exit Play mode and re-enter (or trigger
         a domain reload).
   ```

3. **Optional opt-in `AutoReloadOnRudeEdit` HMR setting** (default off).
   When enabled and a rude edit is detected, prompt via
   `EditorUtility.DisplayDialog`: "New module fields detected — reload
   domain to materialise them? [Reload] [Stay in Play mode]". On Reload,
   call `EditorUtility.RequestScriptReload()`.

### Cost / risk
~50 LOC additive in the existing swapper + once-per-session dedup
`HashSet<string>` + optional dialog wired into HMR settings. ~2 hours.
Zero behavioural risk — purely additive logging unless the user opts in.

### What it doesn't fix
The underlying CLR constraint. Users still need a manual domain reload
to fully materialise new fields — but they will **know** they need to,
instead of getting silent staleness.

### Recommendation
Implement detection + once-per-session warning now. Add the opt-in
auto-reload prompt as a follow-up if requested.

---

## Priority order for v0.4.20

| # | Issue | Severity | Real fix? | Effort |
|---|---|---|---|---|
| 1 | (b) Document mutable-static contract | Not a bug | Docs only | ~30 min |
| 2 | (c) Detect + warn on rude-edit field additions | Medium | Partial (CLR limit) | ~2 hours |
| 3 | (a) Module static methods custom-delegate trampolines | High (~30 % of methods affected) | Yes — full fix | ~1 day |

After all three: HMR's contract becomes
*"the only things that don't fully hot-reload are mutable static state
(preserved on purpose) and brand-new field additions (CLR limitation,
clearly logged)"* — production-grade and predictable.

**Status:** PROPOSAL — awaiting user approval before implementation.
