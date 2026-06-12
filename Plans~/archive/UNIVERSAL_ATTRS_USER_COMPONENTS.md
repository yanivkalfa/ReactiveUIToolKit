# Plan — Strict Attribute Validation for User Components

## Status / metadata

| Field | Value |
| --- | --- |
| Author | Copilot (drafted from investigation in this conversation) |
| Date | 2026-05-08 |
| Library version at draft time | `0.5.3` (extension `1.1.15`) |
| Target version | `0.5.4` (extension `1.1.16`) |
| Branch | `cleanup_and_upgrades` (continuation) |
| Status | **Draft — pending user approval** |

This plan is **read-only research + design**. It contains zero code changes. After
approval it becomes the implementation contract.

## Trigger / motivating example

In the Pretty-UI consumer repo a parent passed `style={...}` to a user component
that did **not** declare a `style` parameter:

```jsx
component AppButton(string text, Action? onClick, bool active, bool disabled, Texture2D? iconName) { ... }

// elsewhere:
<AppButton text="New Game" active={true} onClick={...} style={Button} />
```

The current toolchain's response:

1. **Editor (LSP)**: no diagnostic — `style` is in the schema's `universalAttributes`
   list, so it is allowed on every element including user components.
2. **Source generator**: emits `__p_0.Style = Button;` into a `new AppButtonProps { ... }`
   initializer **without any name validation**, because the user-component code path
   (`EmitFuncComponent`) never calls the UITKX0109 check that the built-in path
   (`EmitElement`) runs.
3. **C# compiler**: blows up with `CS0117 'AppButtonProps' does not contain a definition for 'Style'`
   — pointing at a generated symbol the user never typed.

The runtime works the way React's runtime works: user-component props only contain
what the author declared (plus `IProps`). They do **not** inherit `BaseProps`, so they
genuinely have no `Style`/`Name`/`onClick`/etc. The toolchain just hasn't been
modelling that correctly.

## Goal

Make the editor + source generator agree with the runtime model: **user components
accept only the attributes the author declared, plus the two structural-universal
attributes (`key`, `ref`)**. Any other attribute on a user component is a
diagnostic, *not* a CS-error against generated code.

This is the React/Vue/Svelte (typed) semantic the user has explicitly chosen.

## Investigation summary (background — no action items here)

### Runtime model (verified)

- [`Shared/Props/Typed/BaseProps.cs`](../Shared/Props/Typed/BaseProps.cs) is an
  `abstract class BaseProps : IProps` carrying ~58 members (`Name`, `ClassName`,
  `Style`, `Ref`, `ContentContainer`, visibility/enabled, focus/picking, locale,
  events, `ExtraProps`).
- **Built-in elements** (`<Box>`, `<Label>`, `<ScrollView>`, `<ListView>`, …)
  generate `*Props` classes that extend `BaseProps`, so all 58 members are
  reachable on them.
- **User components** (any `component Foo(...) { ... }` block) generate a
  `FooProps : IProps` with **only the declared parameters** — no base class.
  See [`SourceGenerator~/Emitter/CSharpEmitter.cs:1281`](../SourceGenerator~/Emitter/CSharpEmitter.cs)
  (`EmitFunctionPropsClass`).
- **`key` is structural**: it lives on `VirtualNode._key` directly
  ([`Shared/Core/VNode.cs:51`](../Shared/Core/VNode.cs)) and is consumed by
  `FiberChildReconciliation.ReconcileChildrenWithKeys` for stable identity
  matching across re-renders. **It applies identically to built-ins and user
  components** because both produce `VirtualNode`s and the reconciler treats them
  uniformly. Every factory in `V.cs` (`V.Box`, `V.Label`, `V.Func`, `V.Func<T>`,
  `V.Fragment`, `V.Portal`, `V.Suspense`) already takes `string key = null` and
  writes it onto `VirtualNode._key` — never into any `*Props`.
- **`ref` is universal at the JSX surface, two-path at runtime**:
  - On built-ins: `ref={x}` → `propsVar.Ref = x` (BaseProps member).
  - On user components: `EmitFuncComponent` already routes it via
    `PropsResolver.TryGetRefParamPropName` to a `Hooks.MutableRef<T>` parameter,
    with three outcomes (Found / None → UITKX0020 / Ambiguous → UITKX0021).
  - Conceptually equivalent to React's `forwardRef`. Universal at JSX, opt-in at
    the user-component declaration.

### Schema state today

[`ide-extensions~/grammar/uitkx-schema.json`](../ide-extensions~/grammar/uitkx-schema.json)
declares **60 entries** under `universalAttributes`. Of those, **only 2 are
truly structural** (`key`, `ref`). The remaining **58** are `BaseProps` members
that have no meaning on a user component:

| Bucket | Count | Examples |
| --- | --- | --- |
| Structural (apply everywhere) | 2 | `key`, `ref` |
| Identity / styling (BaseProps) | 4 | `name`, `className`, `style`, `contentContainer` |
| Visibility / enabled (BaseProps) | 2 | `visible`, `enabled` |
| Tooltip / persistence | 2 | `tooltip`, `viewDataKey` |
| Focus / picking (BaseProps) | 4 | `pickingMode`, `focusable`, `tabIndex`, `delegatesFocus` |
| Locale | 1 | `languageDirection` |
| Passthrough bag (BaseProps) | 1 | `extraProps` — escape hatch for elements whose props are not yet fully modelled (e.g. `ListView`, `TableView`, …) |
| Events (BaseProps) | 44 | `onClick`, `onPointerDown`, … (+ Capture variants) |

The toolchain consumes this list through:

- [`ide-extensions~/lsp-server/DiagnosticsPublisher.cs:706`](../ide-extensions~/lsp-server/DiagnosticsPublisher.cs)
  `BuildKnownAttributes` does
  `attrs(component) ∪ schema.UniversalAttributes` for **every** workspace
  component → silently allows all 60.
- [`ide-extensions~/lsp-server/HoverHandler.cs:165`](../ide-extensions~/lsp-server/HoverHandler.cs)
  hover suggestions also use the universal list (informational only).

### Source-generator state today

- [`CSharpEmitter.cs:902`](../SourceGenerator~/Emitter/CSharpEmitter.cs)
  (`EmitElement`) calls `_resolver.GetPublicPropertyNames(res.PropsTypeName)`
  and emits UITKX0109 for unknown attributes. Built-in elements catch
  unknown attrs.
- [`CSharpEmitter.cs:1123`](../SourceGenerator~/Emitter/CSharpEmitter.cs)
  (`EmitFuncComponent`) does **no** name validation. `key`/`ref` are routed
  specially; everything else lands in `new FuncPropsType { Name = AttrVal }`
  unconditionally. Bad attrs become C# CS0117 / CS0246 against generated code.

### Diagnostic registry

- `UITKX0109 UnknownAttribute` is registered in both
  [`SourceGenerator~/Diagnostics/UitkxDiagnostics.cs:157`](../SourceGenerator~/Diagnostics/UitkxDiagnostics.cs)
  and [`ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs:91`](../ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs).
- **Severity is currently inconsistent between the two emit sites.** The SG
  descriptor emits at `Warning`; the LSP analyzer
  ([`DiagnosticsAnalyzer.cs:735`](../ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs))
  already emits at `ParseSeverity.Error`. So a user gets a red squiggle in the
  editor for the same code that produces only a yellow warning at build time.
  This is its own latent bug.
- We **reuse UITKX0109** (no new code) and **promote the SG descriptor to
  `Error`** so both emit sites agree. Existing tests assert presence/absence
  via `HasDiag(...)` — they do not pin severity, so no test rewrites are
  needed. Test method names that contain `_NoWarning` will be renamed for
  clarity but the assertions are unchanged.
- Older inline comments in `CSharpEmitter.cs` say "UITKX0002 — validate
  attribute names…" — that is a stale comment from an earlier numbering scheme.
  No `UITKX0002` exists today. Comment fix is included in the work.

### Test-coverage gap

- The only UITKX0109 tests are
  [`SourceGenerator~/Tests/DiagnosticsAnalyzerTests.cs:176`](../SourceGenerator~/Tests/DiagnosticsAnalyzerTests.cs):
  `<Label bogus="..."/>` (built-in only).
- **Zero tests** cover `<UserComp unknown_attr="..."/>` in either the SG or the
  LSP analyzer. This is the regression-prevention gap the bug exploited.

## Design decisions (confirmed with user)

1. **Truly universal attributes**: `key`, `ref` only.
2. **`extraProps` is intrinsic-only.** It is a generic escape hatch for any
   built-in element where you need to set a property the typed pipeline
   doesn't model — sometimes that's an under-modelled element like
   `ListView`/`TableView`, sometimes it's a one-off advanced property on a
   well-modelled element. The mechanism is element-agnostic at the runtime
   layer ([`TypedPropsApplier.cs:131-136`](../Shared/Props/TypedPropsApplier.cs)
   iterates the dict and calls `PropsApplier.ApplySingle(element, …)` against
   the underlying `VisualElement`). User components have **no underlying
   `VisualElement`** — they return `VirtualNode`s from `Render`. So
   `extraProps` is structurally meaningless on a user component and must be
   rejected as an unknown attribute there.
3. **Reuse `UITKX0109` `UnknownAttribute`** as the diagnostic code, with
   improved message text for the user-component case. No new code.
4. **Promote the SG descriptor of UITKX0109 from `Warning` to `Error`** to
   align with the LSP analyzer and to match the runtime semantics (an unknown
   attribute on a user component will produce a CS-error or a silent runtime
   no-op — both are bugs the toolchain should refuse to compile).

## Schema redesign

We split the existing flat `universalAttributes` list into two scopes. The
schema becomes self-documenting about which attributes are structural vs.
BaseProps.

```jsonc
{
  "version": "1.1",
  "description": "UITKX element and directive schema for IDE tooling",

  // applies to every JSX call: built-ins AND user components
  "structuralAttributes": [
    { "name": "key", "type": "string", "description": "Unique reconciler key. Stabilises identity across re-renders." },
    { "name": "ref", "type": "object", "description": "Forwarded ref. Built-ins receive a VisualElement handle; user components must declare a Hooks.MutableRef<T> parameter to accept it." }
  ],

  // applies to built-in (BaseProps-derived) elements ONLY — never to user components
  "intrinsicElementAttributes": [
    { "name": "name", "type": "string", … },
    { "name": "className", … },
    { "name": "style", … },
    { "name": "contentContainer", … },
    { "name": "visible", … },
    /* …all 58 current BaseProps entries excluding key/ref… */
    { "name": "extraProps", "type": "Dictionary<string,object>",
      "description": "Escape-hatch passthrough bag for built-in elements whose props are not yet fully modelled (e.g. ListView, TableView). NOT applicable to user components." }
  ],

  "elements": { /* unchanged */ },
  "directives": { /* unchanged */ }
}
```

### Backward compatibility

The schema bump is **`1.0` → `1.1`**. The `universalAttributes` key disappears
**after** every consumer is updated. No grace period — both are owned in this
repo and shipped together. Schema version checks in
[`ide-extensions~/lsp-server/SchemaLoader.cs`](../ide-extensions~/lsp-server/SchemaLoader.cs)
will be updated.

## LSP changes — `ide-extensions~/lsp-server`

### `SchemaLoader.cs`

- Replace `public List<AttributeInfo> UniversalAttributes` with two properties:
  - `public List<AttributeInfo> StructuralAttributes`
  - `public List<AttributeInfo> IntrinsicElementAttributes`
- Update `[JsonPropertyName]` mappings to `"structuralAttributes"` /
  `"intrinsicElementAttributes"`.
- Remove `Root.UniversalAttributes` callers; replace with the two new accessors.
- Schema version assertion: bump expected version to `1.1`.

### `DiagnosticsPublisher.BuildKnownAttributes`

Replace the current single-rule build with element-class–aware logic:

```csharp
foreach (var tagName in _schema.Root.Elements.Keys)              // built-ins
{
    var attrs = _schema.GetAttributesForElement(tagName)         // already includes per-element + structural
        .Concat(_schema.Root.IntrinsicElementAttributes)         // NEW
        .Select(a => a.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    result[tagName] = attrs;
}

foreach (var tagName in _index.KnownElements)                    // user components
{
    if (result.ContainsKey(tagName)) continue;
    var attrs = _index.GetProps(tagName).Select(p => p.Name)
        .Concat(_schema.Root.StructuralAttributes.Select(a => a.Name))   // ONLY structural — never intrinsic
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    result[tagName] = attrs;
}
```

`SchemaLoader.GetAttributesForElement(tagName)` is updated to merge per-element
attrs with **structural + intrinsic** instead of the old flat universal list.

### `HoverHandler.cs`

Update universal-attribute hover suggestions: only show structural attrs on
user components; show structural ∪ intrinsic on built-ins. Cosmetic but keeps
the experience honest.

## Source-generator changes — `SourceGenerator~/Emitter`

### `CSharpEmitter.EmitFuncComponent` (lines ~1123-1252)

Add the same UITKX0109 validation that `EmitElement` performs, scoped to the
user-component prop set. Pseudocode (mirrors the existing
`EmitElement` block at lines 902-932):

```csharp
private void EmitFuncComponent(TagResolution res, ImmutableArray<AttributeNode> attrs, string keyExpr,
                               ImmutableArray<AstNode> children, ImmutableArray<string> searchNamespaces)
{
    // … existing refAttr extraction …

    // ── UITKX0109 — validate attribute names against declared user-component props. ──
    // Exempt key/ref (structural-universal) and skip ref-routing here (already handled below).
    if (res.FuncPropsTypeName != null)
    {
        var knownProps = _resolver.GetPublicPropertyNames(res.FuncPropsTypeName);
        if (knownProps.Count > 0)
        {
            foreach (var attr in attrs)
            {
                if (IsKey(attr.Name) || IsRefAttr(attr.Name)) continue;
                string mapped = ToPropName(attr.Name);
                if (!knownProps.Contains(mapped))
                {
                    string? suggestion = FindClosestMatch(mapped, knownProps);
                    string hint = suggestion != null ? $". Did you mean '{suggestion}'?" : "";
                    var loc = MakeLoc(_filePath, attr.SourceLine);
                    _diagnostics.Add(Diagnostic.Create(UitkxDiagnostics.UnknownAttribute, loc,
                        attr.Name, res.FuncTypeName, hint));
                }
            }
        }
    }
    else
    {
        // No-props user component: any non-structural attribute is unknown.
        foreach (var attr in attrs)
        {
            if (IsKey(attr.Name) || IsRefAttr(attr.Name)) continue;
            var loc = MakeLoc(_filePath, attr.SourceLine);
            _diagnostics.Add(Diagnostic.Create(UitkxDiagnostics.UnknownAttribute, loc,
                attr.Name, res.FuncTypeName,
                $". Component '{res.FuncTypeName}' declares no parameters; add one or remove the attribute."));
        }
    }

    // … existing emission, but: when emitting attribute assignments,
    // SKIP attributes flagged as unknown so we don't pile CS0117 on top of UITKX0109.
}
```

Two implementation details:

1. **Skip-on-unknown.** The bad assignment (`Foo = AttrVal`) must not be
   written into the C# output, so the C# compiler doesn't pile CS0117/CS0246
   onto the UITKX0109 we just emitted. We collect the unknown set into a
   `HashSet<string>` and skip in the emit loop.
2. **No-props user components.** When `FuncPropsTypeName == null`, the
   component declares zero parameters. Any non-structural attr is unknown.

### Stale-comment fix

[`CSharpEmitter.cs:902`](../SourceGenerator~/Emitter/CSharpEmitter.cs) currently
says `// UITKX0002 — validate attribute names against known Props properties`.
There is no UITKX0002 — it is UITKX0109. Update the comment.

### `PropsResolver`

The line 408 comment `// GetPublicPropertyNames — used by CSharpEmitter for UITKX0002 attribute validation`
gets the same correction (UITKX0109). Behavior unchanged.

## Diagnostic message updates

`UitkxDiagnostics.UnknownAttribute.messageFormat` is currently:

```
Unknown attribute '{0}' on <{1}>{2}
```

Where `{2}` is either empty or `". Did you mean 'X'?"`. We extend the
suggestion-builder so the formatted hint is more actionable on user
components — driven by SG, not by descriptor change:

| Case | `{2}` value |
| --- | --- |
| Built-in element, close match exists | `". Did you mean 'X'?"` |
| Built-in element, no close match | `""` |
| User component, close match exists | `". Did you mean 'X'?"` |
| User component, no close match | `". Available: A, B, C. Add a parameter to '<TypeName>' or remove the attribute."` |
| User component with zero params | `". '<TypeName>' declares no parameters; add one or remove the attribute."` |

(The descriptor's `messageFormat` is unchanged so the existing severity/rule
identity stays stable. Only the format-args input from SG/analyzer changes.)

## Tests

Added under `SourceGenerator~/Tests/`:

### `DiagnosticsAnalyzerTests.cs` (analyzer / language-lib path)

```csharp
[Fact] UITKX0109_UserComponent_UnknownAttr_Errors()              // <Foo bogus="x"/>; expect UITKX0109
[Fact] UITKX0109_UserComponent_KeyAlwaysAllowed()                // <Foo key="x"/>; no UITKX0109
[Fact] UITKX0109_UserComponent_RefAlwaysAllowed_NoUITKX0109()    // <Foo ref={r}/>; UITKX0020 may still fire from SG path, but NOT UITKX0109
[Fact] UITKX0109_UserComponent_StyleNotForwarded()               // regression test: <Foo style={x}/> when Foo lacks style param → UITKX0109
[Fact] UITKX0109_BuiltIn_StyleAllowed()                          // <Box style={x}/> still passes
[Fact] UITKX0109_BuiltIn_ExtraPropsAllowed()                     // <ListView extraProps={d}/> still passes
[Fact] UITKX0109_UserComponent_ExtraPropsRejected()              // <Foo extraProps={d}/> when Foo lacks extraProps param → UITKX0109
```

### Source-generator emit-time path

End-to-end SG tests (existing snapshot-style infra) for:

```csharp
[Fact] EmitFuncComponent_UnknownAttr_NoBadAssignmentInOutput()   // generated C# does NOT contain `.Style =` for the bad case
[Fact] EmitFuncComponent_UnknownAttr_EmitsUITKX0109()            // diagnostic is in the SG diag list
[Fact] EmitFuncComponent_KeyAttr_StillRoutedAsKey()              // <Foo key="k"/> → V.Func(Foo.Render, …, key: "k", …)
[Fact] EmitFuncComponent_RefAttr_StillRoutedToMutableRefParam()  // unchanged behaviour
```

### LSP integration test (existing harness, if present)

```csharp
[Fact] LSP_UserComponent_StyleAttr_Squiggles()                   // open a file with <UserComp style={x}/>; expect UITKX0109 diag from server
[Fact] LSP_BuiltIn_StyleAttr_NoSquiggle()                        // sanity
```

## Migration / breaking-change surface

This **is** a behavioural change for any consumer who currently relies on
`<UserComp style={...} onClick={...}/>` "working" (i.e. silently no-oping). In
the Pretty-UI repo there are real consumers depending on it. Migration is
straightforward and handled per consumer:

1. Either declare the attribute on the user component:
   ```jsx
   component AppButton(..., Style? style = null) { … merge style on the host … }
   ```
2. Or remove the attribute at the call site.

This matches what the runtime always did — this just makes the toolchain
honest.

We classify it as a **diagnostic upgrade**, not an API break, because:

- No public API surface changes.
- Existing well-behaved code is unaffected.
- `key` and `ref` continue to work everywhere.
- The new diagnostic is a `Warning` (existing severity for UITKX0109).
  Codebases that treat warnings as errors will see a build break — that is
  the desired catch-the-bug behaviour.

The MIGRATION_GUIDE.md will get a section "0.5.4: stricter user-component
attribute validation" listing these two recipes.

## Versioning + changelog

- Library: `0.5.3` → `0.5.4`.
- Extension: `1.1.15` → `1.1.16`.
- `CHANGELOG.md` entry under `[0.5.4]` with `Changed` (schema split,
  user-component attribute validation), `Fixed` (CS0117 cascade for unknown
  attrs on user components — now UITKX0109 with actionable message).
- `ide-extensions~/changelog.json` entry mirroring the above.
- `Plans~/DISCORD_CHANGELOG.md` rewritten for the announcement.

## Implementation order (sequenced for easy review)

The work splits cleanly into commits that compile + test green at each step.

1. **Schema split + `SchemaLoader` accessors** (no behavioural change yet —
   `BuildKnownAttributes` keeps producing the same set by combining structural
   ∪ intrinsic).
2. **LSP `BuildKnownAttributes` rewrite** — gates intrinsic attrs to built-ins.
   Tests (LSP fixtures + analyzer test) added in same commit.
3. **SG `EmitFuncComponent` UITKX0109 + skip-on-unknown** + the stale-comment
   fix in `EmitElement`. Tests added in same commit.
4. **Diagnostic message refinement** for the user-component case.
5. **Docs site update** — Reference page Migration callout, Diagnostics page
   UITKX0109 row gets a "user component" sub-bullet, search index updated.
6. **Version bump + changelogs + Discord** — final commit, tagged release.

Each commit is self-contained, builds clean, and ships green tests.

## Risks / unknowns

- **Hidden consumers of `Root.UniversalAttributes`.** A `grep` over the repo
  found one production caller (`HoverHandler`) plus the schema/parser tests.
  Step 1 of the implementation will do a follow-up grep including
  `Plans~/`, `Diagnostics/`, samples, and CICD scripts; any additional
  callers must be updated in the same commit as the schema split.
- **Schema-driven analyzer in language-lib.** Need to verify that
  `language-lib` has no second copy of the universal-attribute list. (Brief
  read of `Diagnostics/DiagnosticsAnalyzer.cs` shows it consumes
  `knownAttributes` *passed by the caller*, so the rewrite is contained to
  `DiagnosticsPublisher.BuildKnownAttributes`.) Step 1 confirms this.
- **Severity promotion to `Error`.** Decided: yes (see Design decision #4).
  The risk is downstream consumers who currently have UITKX0109 warnings in
  their code; after the bump those become build failures. That is
  intentional — the existing warnings are either (a) typos that were silent
  no-ops at runtime or (b) about to become CS-errors against generated code
  the moment the schema split lands. Surfacing them at `Error` is the
  correct outcome.
- **`ContentContainer` semantics.** Today it lives on BaseProps. For some
  built-in container elements it has runtime meaning. The schema split puts
  it in `intrinsicElementAttributes`. If any user component intentionally
  re-exposes it via a declared param, the new behaviour will validate against
  *the user's* declaration — which is the correct outcome.

## What this plan explicitly does NOT do

- Does **not** introduce auto-`style` / auto-`className` forwarding on user
  components (Option B from the earlier discussion). React doesn't do that;
  we don't either. Consumers opt-in by declaring the parameter and applying
  it themselves.
- Does **not** add a "spread rest props" feature. `extraProps` stays
  intrinsic-only as an escape hatch for under-modelled built-in elements.
- Does **not** change `key` or `ref` semantics. They are already correct.
- Does **not** change diagnostic severity (stays `Warning`).

## Open questions for review

All resolved. Plan is locked pending final go-ahead.

- **Schema version bump**: deferred (cosmetic; does not affect implementation).
  Default `1.0 → 1.1` will be used unless changed at implementation time.
- **`extraProps` placement**: confirmed `intrinsicElementAttributes`.
  Verified by reading [`TypedPropsApplier.cs:131-136`](../Shared/Props/TypedPropsApplier.cs):
  the runtime iterates the dict and calls `ApplySingle(element, …)` against a
  `VisualElement` — user components don't have one, so the property is
  structurally meaningless on them.
- **Severity**: promote SG descriptor to `Error`. Aligns with LSP analyzer
  (which already emits at `Error`); no test rewrites needed.
- **Version bump**: deferred (cosmetic). Default `0.5.4` patch will be used
  unless changed at implementation time.
