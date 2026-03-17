# UITKX Implementation Plan - Function-Style Syntax (Production-Ready)

## Objective
Ship first-class function-style component authoring:

```uitkx
component ComponentName {
		// C# setup code
		return (
				<Box />
		);
}
```

This plan targets long-term maintainability: clean parser architecture, deterministic lowering, robust diagnostics, stable tokenization, formatter support, and full IDE parity.

## Status Legend
- `[ ]` not started
- `[~]` in progress
- `[x]` completed
- `[-]` removed/superseded

## Current Status

| Track | Status | Notes |
|---|---|---|
| Syntax contract | `[x]` | Canonical `component PascalCaseName { ... return (...) ... }` contract locked |
| Parser/tokenizer refactor | `[x]` | Function-style entry parsing, return extraction, recovery, and mixed-form diagnostics implemented |
| Lowering pipeline | `[x]` | Canonical lowering hoists setup code and normalizes function-style/directive-style root shape |
| Emission + runtime compatibility | `[x]` | Lowered roots are emitted through existing generator path with compatibility tests |
| Diagnostics + formatter + IDE | `[x]` | Completion/definition/semantic tokens/formatting parity implemented for function-style |
| Test matrix + rollout | `[x]` | Parser/lowering/emitter/formatter coverage added and suite is green |

---

## Phase 0 - Design Freeze (Contract First)

- [ ] Finalize canonical declaration form: `component PascalCaseName { ... }`.
- [ ] Finalize PascalCase validation rules and diagnostic behavior.
- [ ] Finalize `return (...)` requirements:
	- [ ] Exactly one top-level return required.
	- [ ] Return value must be UITKX markup expression.
	- [ ] Unreachable/extra returns produce deterministic diagnostics.
- [ ] Decide support level for optional future syntax (params/generics/async) and explicitly mark out of scope.
- [ ] Publish syntax contract section in docs before implementation starts.

**Exit criteria:** Language contract is frozen and signed off; no parser coding before this is complete.

### Phase 0 - Acceptance Contract (Implementation-Ready)

#### 0.A Grammar shape (authoring contract)

```ebnf
FunctionComponent  ::= "component" PascalIdentifier "{" FunctionBody "}"
FunctionBody       ::= (CSharpStatement | UitkxDirectiveOrMarkup | ReturnStatement)*
ReturnStatement    ::= "return" "(" MarkupRoot ")" ";"
MarkupRoot         ::= Element | Fragment | ControlFlowBlock
PascalIdentifier   ::= [A-Z] [A-Za-z0-9]*
```

**Notes:**
- The keyword is lowercase `component`.
- `component` declaration is a top-level file form (mutually exclusive with directive-header form in v1 of this feature).
- `return (...)` is required and must return UITKX markup.
- Function-style files do not require `@namespace` in `.uitkx`; namespace is inferred from the companion partial `.cs` file (with fallback to `ReactiveUITK.FunctionStyle` when unavailable).

#### 0.B Valid examples

```uitkx
component CounterPanel {
	var (count, setCount) = useState(0);

	return (
		<Box>
			<Button text="+" onClick={() => setCount(count + 1)} />
		</Box>
	);
}
```

#### 0.C Invalid examples (must diagnose)

```uitkx
component counterPanel { }                 // invalid casing
component CounterPanel { }                 // missing return
component CounterPanel { return count; }   // invalid return kind
component CounterPanel {
	return (<Box />);
	return (<Label />);
}                                           // multiple top-level returns
```

#### 0.D Diagnostic ID allocation (proposed, reserved for this feature)

| Code | Severity | Message (summary) | Trigger |
|---|---|---|---|
| UITKX2100 | Error | `component` name must be PascalCase | non-Pascal identifier |
| UITKX2101 | Error | Missing `return (...)` in function-style component | no valid top-level return |
| UITKX2102 | Error | `return` must return UITKX markup | scalar/C# expression return |
| UITKX2103 | Error | Multiple top-level returns are not allowed | >1 top-level return |
| UITKX2104 | Error | Function-style form cannot be mixed with directive header form | file contains both forms |
| UITKX2105 | Error | Invalid top-level statement in function-style component | unsupported top-level construct |

#### 0.E Out-of-scope (explicit)

- No params/generics on `component` declaration in first production release.
- No `async`/`await` component signature semantics.
- No overload-like multiple component declarations per file.
- No mixing legacy directive-header and function-style in same file.

#### 0.F Phase 0 done-definition

- [ ] Grammar contract above accepted unchanged.
- [x] Diagnostic codes/messages approved.
- [x] At least 1 valid + 4 invalid golden examples added to tests/docs.
- [ ] Main plan + release notes reference this contract as source of truth.

---

## Phase 1 - Parser + Tokenizer Foundation (Real Fix)

- [ ] Refactor parser entry model to support dual top-level forms:
	- [x] existing directive-based files
	- [x] new `component Name { ... }` files
- [ ] Introduce dedicated function-style AST nodes (do not overload legacy nodes):
	- [ ] `FunctionComponentNode`
	- [ ] `FunctionReturnNode`
	- [ ] structured body statement nodes (code + markup-return boundary)
- [x] Implement robust block parsing with full brace/paren balance handling.
- [x] Implement resilient recovery for malformed function-style source.
- [x] Ensure parser span data is complete for diagnostics + IDE features.

**Exit criteria:** Parser handles valid/invalid function-style files deterministically with stable AST and recovery.

---

## Phase 2 - Canonical Lowering Layer

- [x] Add explicit lowering stage from function-style AST to existing internal render IR.
- [ ] Enforce semantic normalization in lowering:
	- [x] setup code hoisting
	- [x] return markup extraction
	- [x] control-flow normalization
- [x] Preserve source-map/line mapping metadata through lowering.
- [x] Guarantee backward compatibility: legacy syntax and function-style both produce equivalent IR for equivalent logic.

**Exit criteria:** One canonical IR path regardless of authoring style.

---

## Phase 3 - Emitter + Generator Integration

- [x] Integrate lowered function-style IR into C# emitter path.
- [x] Keep generated code deterministic (ordering, naming, helper usage).
- [x] Maintain `#line` mapping integrity from function-style source.
- [x] Verify compatibility with hooks/state patterns already supported in `@code`.
- [x] Validate Unity compile output on representative samples (simple, nested, control-flow heavy).

**Exit criteria:** Generator output compiles cleanly and mirrors runtime behavior of legacy model.

---

## Phase 4 - Diagnostics (Production Quality)

- [x] Add dedicated diagnostic codes for function-style contract violations:
	- [x] invalid component name casing
	- [x] missing/invalid return
	- [x] illegal top-level constructs
	- [x] unsupported syntax in function body
- [x] Add precise ranges/messages/fixes for all new diagnostics.
- [x] Ensure no duplicate/conflicting diagnostics across parser/lowering/emitter stages.
- [x] Ensure diagnostics remain stable in incomplete typing states.

**Exit criteria:** Diagnostics are specific, actionable, and non-flaky during editing.

---

## Phase 5 - IDE + Tooling Parity

- [x] Update completion engine for `component` declaration context.
- [x] Update hover and definition behavior for function-style symbols.
- [x] Update semantic tokenization to support new syntax nodes consistently.
- [x] Update formatter for function declaration blocks + return blocks.
- [x] Ensure color/token behavior remains consistent between legacy and function-style forms.

**Exit criteria:** VS Code + VS extension behavior is feature-parity with legacy authoring.

---

## Phase 6 - Tests + Validation Matrix

- [x] Parser tests:
	- [x] happy-path declarations
	- [x] malformed signatures
	- [x] malformed return blocks
	- [x] nested control-flow and embedded markup
- [x] Lowering tests:
	- [x] IR equivalence across legacy/function-style variants
	- [x] source-span preservation
- [x] Emitter tests:
	- [x] deterministic generated source snapshots
	- [x] compile-success verification patterns
- [x] LSP/IDE tests:
	- [x] completion/hover/token/format snapshots
	- [x] incremental edit behavior

**Exit criteria:** Green full matrix with no regressions in legacy syntax.

---

## Phase 7 - Migration + Release

- [ ] Add side-by-side migration guide (legacy -> function-style).
- [ ] Add official examples under samples/docs.
- [x] Publish changelog with compatibility guarantees and known limitations.
- [x] Execute full release flow (version bump, build, publish, local install verification).
- [x] Post-release smoke test on real sample projects.

**Exit criteria:** Production release complete with documentation and verified install path.

---

## Non-Negotiable Quality Rules

- [x] No one-off parser hacks for function-style support.
- [x] No ambiguity with existing markup grammar.
- [x] No regressions to existing directive-based syntax.
- [x] No tokenization/color drift introduced by new syntax.
- [x] Every behavior change must be covered by tests before release.

---

## Execution Order Recommendation

1. Phase 0 (freeze contract)
2. Phase 1 + 2 (parser foundation + lowering)
3. Phase 3 + 4 (emission + diagnostics)
4. Phase 5 (IDE parity)
5. Phase 6 + 7 (validation + release)

This sequence minimizes rework and avoids shipping syntax before architecture is stable.
