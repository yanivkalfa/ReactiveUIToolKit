# IntelliSense Bugs — Phase 2 Plan

**Parent plan:** [`intellisense-plan.md`](intellisense-plan.md)  
**Status:** ACTIVE  
**Created:** 2026-03-10  
**Mandate:** No shortcuts. No temporary workarounds. Production-grade solutions only.

---

## Overview

Items tracked here fall into two categories:
- **Carried over** — deferred from Phase 1 implementation (T-01 → T-09 + hotfixes v1.0.140, v1.0.141)
- **New** — reported by user after v1.0.141

Shipped so far: v1.0.139 (T01-T09), v1.0.140 (block-body lambda stub emission), v1.0.141 (double-check lock bug + CompletionHandler EnsureReadyAsync gate), v1.0.142 (N-2: evt member completions in block-body lambda, N-4: JSX comment in attribute list), v1.0.144 (N-3: bare return in lambda, N-5: auto-indent, N-1: useRef completions, CO-1/CO-4: CS0246/CS0219 re-enabled, CO-3: unreachable node dimming, N-6: Roslyn format on save, CO-2: StyleKeys value completions).

---

## Carried-Over Items

### ~~CO-1~~ · Type-not-found errors are globally suppressed (CS0246) — ✅ SHIPPED v1.0.144

**Symptom:** Misspelling a type name (e.g. `Stlay` instead of `Style`) shows no red squiggle.  
**Root cause:** The virtual-document scaffold contains a blanket `#pragma warning disable CS0246` that covers all user C# regions, not just the generated wrapper scaffolding.  
**Fix:** Remove CS0246 from the global pragma list. Suppress it only on specific generated scaffold lines that are known to produce false positives (e.g. dynamic invocations, generated member stubs).  
**Effort:** Medium — need to identify every scaffold pattern that legitimately triggers CS0246 and add targeted per-line suppression.

---

### ~~CO-2~~ · StyleKeys property completions on quoted string arguments — ✅ SHIPPED v1.0.144

**Symptom:** In `(StyleKeys.FlexDirection, "column")` the string literal `"column"` should offer completions restricted to the valid values for `FlexDirection` (e.g. `"row"`, `"column"`, `"row-reverse"`, …).  
**Root cause:** No schema-to-type mapping exists. StyleKeys members are schema-defined and not C# enums, so Roslyn cannot infer the set of valid strings.  
**Fix:** Two-phase:
1. Extend the schema with a `valueSet` field per StyleKeys entry.
2. In `CompletionHandler`, detect when the cursor is inside a string literal that is the second element of a `(StyleKeys.Xxx, "…")` tuple, and substitute schema-driven string completions for Roslyn completions.  
**Effort:** Large — requires schema changes + AST cursor detection + new completion path.

---

### CO-3 · Dead code after @return / @break / @continue not dimmed — ❌ OPEN (user reports not working)

**Symptom:** Statements written after an unconditional `@return` in markup control flow are not grayed out.  
**Root cause:** UITKX has no reachability analysis pass; no equivalent to Roslyn's "unreachable code" diagnostic for the markup control-flow layer.  
**Fix:** Add a reachability walk over the AST `ControlFlowNode` list inside `VirtualDocumentGenerator` or a new `ReachabilityAnalyzer`. Emit a custom diagnostic (code `UITKX0010`, severity Hint, tag Unnecessary) for each unreachable span.  
**Effort:** Large — new analysis pass.

---

### ~~CO-4~~ · Unused variables / functions not highlighted — ✅ SHIPPED v1.0.144

**Symptom:** A declared variable that is never read (CS0219) shows no warning.  
**Root cause:** CS0219 is in the global `#pragma warning disable` list.  
**Fix:** Same approach as CO-1 — remove CS0219 from the global suppress list and validate that no scaffold line produces a spurious unused-variable warning. If any scaffold variable does, suppress it per-line.  
**Effort:** Small–Medium.

---

## New Items — v1.0.141 Testing

### N-1 · `.Current` / `.Value` — no semantic colour — ❌ OPEN (user reports still no colour after v1.0.148)

**Symptom:** Inside `RouterHooks.UseBlocker(…)` the variable `allowNextRef` (declared by `useRef<bool>()`) has no member completions.  
**Root cause:** Still under investigation after v1.0.141. Likely one of:
  - `EnsureReadyAsync` rebuilds the workspace but `GetVirtualDocument` in `CompletionHandler` still races between the newly-created `FileState` and the `vdoc` snapshot stored after the build.
  - OR the `useRef<T>` stub in the virtual document is not emitted correctly (returns type `global::ReactiveUITK.Ref<T>`) when `T` is inferred from context.
**Fix plan:**
1. Add `ServerLog` tracing to prove the workspace is rebuilt before `GetCompletionsAsync`.
2. Check the generated virtual document for the file and confirm `allowNextRef` has type `Ref<bool>`.
3. If type is correct, suspect a `CompletionService` positioning issue.
**Effort:** Medium.

---

### ~~N-2~~ · `evt.newValue` — no completions inside `onChange` block-body lambda — ✅ SHIPPED

**Symptom:** `onChange={evt => { evt.| }}` — after the dot, no members are offered.  
**Root cause (updated):** `EmitBlockBodyLambda` emits `dynamic evt = default!;` inside a bare block `{ … }`. However, the body may contain `return` statements, and `return` inside a plain block statement (not inside a method/lambda in the virtual doc) is invalid C# — Roslyn may reject the virtual document document entirely, so no completions are available.  
**Fix:** Change `EmitBlockBodyLambda` to wrap the body in a local function with a `dynamic` return type:
```csharp
// BEFORE — bare block, return is illegal
{
    dynamic evt = default!;
    <mapped body>
}

// AFTER — local function, return is valid
{
    dynamic __uitkx_body() {
        dynamic evt = default!;
        <mapped body>
        return default!;
    }
    _ = __uitkx_body();
}
```
This makes `return` (of any type) valid inside the body, and member completions on `evt` work because Roslyn can type-check the local function body.  
**Effort:** Small — targeted change to `EmitBlockBodyLambda`.

---

### ~~N-3~~ · `return` inside block-body lambda shows error — ✅ SHIPPED v1.0.144

**Symptom:** `if (lastBlockedTo == null) return;` inside an attribute-expression block-body lambda shows: _"An object of a type convertible to 'object' is required"_.  
**Root cause:** Same as N-2. The body is placed inside a bare `{ }` block in the virtual document, so `return` is parsed as returning from the enclosing `__uitkx_render()` method (which has a non-`bool` return type).  
**Fix (revised):** N-2's local-function wrapping is in place but the local function has `dynamic` return type. A bare `return;` (no value) in a `dynamic`-returning function is CS0126 — a hard error. Add `#pragma warning disable CS0126` / `restore` around the mapped body inside `EmitBlockBodyLambda` so bare `return;` is suppressed.  
**Effort:** Small — one additional pragma pair in `EmitBlockBodyLambda`.

---

### ~~N-4~~ · JSX comment `{/* … */}` does not comment out a component — ✅ SHIPPED

**Symptom:**
```uitkx
<VisualElement
    {/* style={new Style { … }} */}
>
```
The `{/* … */}` syntax inside a tag's attribute list is not recognized as a comment; the parser either errors or treats the braces as an expression.  
**Root cause:** The UITKX parser's attribute-list scan does not handle the `{/* comment */}` JSX comment form. It is only handled (if at all) inside element children, not between attribute tokens.  
**Fix:** Extend the parser's attribute-scanning loop to recognize `{/*` as the start of a JSX comment node, consume text until `*/}`, and skip the span without emitting any attribute.  
**Effort:** Medium — parser change + AST node + formatter skip. *(shipped)*

---

### N-5 · Enter after tag / brace / @case produces wrong indentation — ❌ OPEN (root causes tracked as FMT-1 – FMT-4)

**Symptom:** Pressing Enter after `<VisualElement …>` (or any element) places the cursor at column 0 (`<`), with no auto-indent.  
**Root cause:** The VS Code extension's `language-configuration.json` (or equivalent indentation rule) either has no `indentationRules` / `onEnterRules` that recognize the UITKX tag structure, or the rule exists but does not fire for function-style component files.  
**Fix:**
1. Add `onEnterRules` to `language-configuration.json`:
   - Match `^(\s*)<[A-Za-z][^/]*[^/]>$` (open tag at end of line) → `indentAction: Indent`.
   - Match `^(\s*)<[A-Za-z][^/]*/>\s*$` (self-closing) → `indentAction: None`.
2. Verify the rule works with function-style `.uitkx` files.  
**Effort:** Small.

---

### N-6 · On-save formatter does not format the entire component body — ⚠️ PARTIAL (user reports semi-working)

**Symptom:** The entire component body (markup + setup code) is not reformatted when the file is saved (despite format-on-save being enabled). Confirmed: not just lambda bodies — the whole component body is skipped.  
**Root cause candidates:**
  - The UITKX formatter (`AstFormatter`) only reformats markup sections; it does not reformat setup-code / C# lambda bodies (passed through verbatim).
  - OR the formatter is not being invoked at all for function-style components.
  - OR the formatter's indentation emitter uses the node's known indent level but does not parse and re-indent the interior of a multi-line lambda argument.  
**Fix plan:**
1. Investigate `AstFormatter` to confirm which sections are reformatted.
2. For setup-code blocks (function-style components), call Roslyn's `Document.WithSyntaxRoot(Formatter.Format(…))` to produce formatted C# text, then map the formatted offsets back to .uitkx positions.  
**Note:** This requires the virtual document to be fully valid C# (no Roslyn errors) for Roslyn formatting to produce correct output. N-2/N-3 must be fixed first.  
**Effort:** Large — Roslyn formatter integration.

---

## Priority Order

| # | ID | Description | Effort | Depends on |
|---|---|---|---|---|
| # | ID | Description | Effort | Depends on | Status |
|---|---|---|---|---|---|
| 1 | ~~N-2~~ | Block-body lambda `evt.` completions | Small | — | ✅ Shipped |
| 2 | ~~N-4~~ | JSX comment inside attribute list | Medium | — | ✅ Shipped |
| 3 | ~~N-3~~ | `return;` inside block-body lambda shows error | Small | — | ✅ v1.0.144 |
| 4 | N-5 | Enter after tag / brace / @case indentation | Small | — | ❌ OPEN → FMT-1–4 |
| 5 | N-1 | `.Current`/`.Value` semantic colour | Medium | — | ❌ OPEN |
| 6 | ~~CO-1 + CO-4~~ | Re-enable CS0246 / CS0219 (type errors + unused vars) | Medium | — | ✅ v1.0.144 |
| 7 | N-6 | On-save formatter for entire component body | Large | — | ⚠️ PARTIAL |
| 8 | ~~CO-2~~ | StyleKeys string value completions | Large | — | ✅ v1.0.144 |
| 9 | CO-3 | Dead-code dimming after @return/@break | Large | — | ❌ OPEN |

---

---

## New Items — Post v1.0.149 (Formatting & Indentation)

### FMT-1 · `var (count, setCount)` / `var (mode, setMode)` wrong indentation after Format Document — ❌ OPEN

**Symptom:** In a function-style component with two `useState` calls, Format Document produces inconsistent indentation between the two lines. E.g. `var (count, setCount) = useState(0)` comes out at 2-space but `var (mode, setMode) = useState("normal")` comes out at 4-space (or vice versa).  
**Root cause:** `EmitSetupCodeWithJsx` splits the setup code into C# segments at JSX block boundaries and calls `FormatStatements` on each segment independently. The Roslyn formatter formats each segment in isolation without surrounding context, so relative indentation within a multi-statement segment may be miscalculated for tuple-deconstruction patterns.  
**Fix plan:** Investigate how `FormatStatements` wraps the segment (e.g. in a synthetic method body) and whether `var (a, b) = …` parses correctly in that wrapper. If the wrapper produces a mismatched indent, adjust the wrapper or post-process the result to normalize leading whitespace to a uniform base level.  
**Effort:** Medium.

---

### FMT-2 · Enter after `component X {` places cursor 2 columns too far — ❌ OPEN

**Symptom:** After typing `component Counter {` and pressing Enter, the cursor lands at column 4 (two indents) instead of column 2 (one indent).  
**Root cause:** Both `increaseIndentPattern` (before v1.0.149 fix: `^.*\{\s*$`) AND the matching `onEnterRule` fired simultaneously, doubling the indent. Attempted fix in v1.0.149 (removed `{` from `increaseIndentPattern`), but user reports issue persists.  
**Fix plan:** Re-read `language-configuration.json` as it is now on disk. Verify the `increaseIndentPattern` change took effect. If correct pattern is in place, investigate whether VS Code's `brackets` array (`["{","}"]`) is applying its own indent logic independently of both rules.  
**Effort:** Small–Medium.

---

### FMT-3 · Enter after `<VisualElement>` places cursor at same column as `<` — ❌ OPEN

**Symptom:** After `<VisualElement>` (open tag on its own line), pressing Enter places the cursor at the same column as `<` instead of one indent level in.  
**Root cause:** The `onEnterRule` for open tags requires `beforeText` to match, but the rule may not be firing. `decreaseIndentPattern` was added in v1.0.149 for `</` tags but the open-tag indent rule may still be misconfigured.  
**Fix plan:** Confirm the `onEnterRule` with `beforeText: "^(?!.*/>\\s*$).*[^/]>\\s*$"` is present and has no regex errors. Test with a minimal `.uitkx` file. If VS Code ignores the rule, the `increaseIndentPattern` (which was narrowed in v1.0.149) may need to re-include open tags.  
**Effort:** Small.

---

### FMT-4 · Enter after `@case "value":` places cursor under `@` — ❌ OPEN

**Symptom:** After `@case "squared":`, pressing Enter places the cursor directly below `@` (column 0 or same column) instead of one indent level deeper for the case body.  
**Root cause:** No `onEnterRule` existed for lines ending with `:`. An attempt was made in v1.0.149 with `beforeText: "^\\s*@(?:case\\b.*|default):\\s*$"`, but user reports it is still not working.  
**Fix plan:** Verify the regex matches actual `@case "squared":` text. Note that `@default:` has no space before `:` but `@case "x":` does — confirm the pattern handles both. If the VS Code JSON regex engine has issues with `\b` or `|` in this position, simplify to `^\\s*@(case|default).*:\\s*$`.  
**Effort:** Small.

---

## Notes

- **`setLastBlockedFrom(default)`** — `default` is standard C# syntax meaning "the default value for the inferred type" (null for reference types, 0 for int, false for bool, etc.). This is correct user code; no fix needed.
- All items above are for the **VS Code** extension. Visual Studio 2022 parity is assumed to follow automatically since both share the same LSP server.

---

## New Items — Post v1.0.151

### SU-1 · `setState(prev => newValue)` callback form shows LSP type error — ❌ OPEN

**Symptom:** Usage of the functional-update / callback form of a `useState` setter:
```uitkx
var (treeRows, setTreeRows) = useState(new List<TreeViewRowState>());
// …
setTreeRows(prev => {
    var next = new List<TreeViewRowState>(prev);
    // …
    return next;
});
```
The lambda `prev => { … }` shows the error: _"Cannot convert lambda expression to type 'List\<TreeViewRowState\>'"_.

**Root cause:** The virtual-document generator emits a single overload stub for each `useState` setter, matching only the direct-value form: `Action<T>` (i.e. `setter(newValue)`). The functional-update form `setter(prev => newValue)` requires a `Func<T, T>` overload. Without that second overload, Roslyn cannot resolve the call and produces a type-mismatch error.

**Fix plan:**
1. Find where useState setter stubs are emitted in the virtual document generator (likely `VirtualDocumentGenerator.cs` or `StubEmitter.cs`).
2. For each `useState` setter stub, emit a second overload accepting `Func<T, T>`:
   ```csharp
   // existing
   void setFoo(T value) { }
   // add
   void setFoo(Func<T, T> updater) { }
   ```
3. Verify the fix against both simple `prev => prev + 1` and multi-line block-body `prev => { … return …; }` forms.

**Effort:** Small — targeted addition of one overload per setter stub.

