# General Bugs

Ongoing bug tracker. Add new bugs at the bottom. Mark completed with `[x]`.

---

## VS2022 Extension

### [x] 1. No dim coloring for return/break/continue

**Reported:** 2026-03-14

The tagger maps `DiagnosticTag.Unnecessary` to `PredefinedErrorTypeNames.HintedSuggestion`, which shows "..." dots under the text but does NOT dim/fade it like VS Code does. To get actual dimming, we need a separate `ITagger<IClassificationTag>` that applies `ClassificationTypeNames.ExcludedCode` (or a custom faded classification) to diagnostic ranges tagged with `Unnecessary`.

**Files involved:**
- `ide-extensions~/visual-studio/UitkxVsix/UitkxDiagnosticTagger.cs` — current tagger
- `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` — sends `DiagnosticTag.Unnecessary` for UITKX0107/UITKX0110

---

### [x] 2. `{/* */}` JSX comments not colored green

**Reported:** 2026-03-14

The VS2022 custom classifier (`UitkxClassifier.cs`) only handles `//` line comments. It has no code for `{/* */}` JSX-style block comments. The LSP server sends `comment` semantic tokens for these, but the VS2022 classifier doesn't consume semantic tokens — it uses its own text-based parsing. Need to add `{/* */}` block comment parsing to the classifier.

**Files involved:**
- `ide-extensions~/visual-studio/UitkxVsix/UitkxClassifier.cs` — `TryClassifyLineComment()` only handles `//`
- `ide-extensions~/grammar/uitkx.tmLanguage.json` — has `jsx-comment` rule (VS Code uses this)
- `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs` — emits `comment` tokens for `{/* */}`

---

## Both Editors (Server-Side)

### [x] 3. Hovering over elements shows wrong props

**Reported:** 2026-03-14

When hovering over element names in markup, the tooltip sometimes shows incorrect props. The hover handler (`HoverHandler.cs`) checks the workspace index first (`_index.TryGetElementInfo(word)`), which returns props from `*Props.cs` files in the workspace. For built-in elements like `Label`, it should fall through to the schema lookup. Possible causes: workspace index contains a stale or incorrect entry, or the element name matches a different component in the workspace index before reaching the schema fallback.

**Files involved:**
- `ide-extensions~/lsp-server/HoverHandler.cs` — priority chain: workspace index → schema → attribute → directive → hooks
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — `TryGetElementInfo()` and `GetProps()`

---

### [x] 4. Inline `<Tag>` markup in C# lambda expressions causes compilation errors

**Reported:** 2026-03-14

Using `<Label text="..." />` markup inside lambda expressions in C# object initializers causes Unity compilation errors. For example:

```csharp
new TabViewProps.TabDef { Title = "Log", Content = () => <Label text="hello" /> }
```

The transpiler's `ScanForReturnMarkup()` in `UitkxParser.cs` supports `return <Tag .../>` and `var x = <Tag .../>` patterns but deliberately excludes `=>` (arrow operator) to avoid mismatching lambda arrows. This means inline markup after `=>` is not detected and gets emitted as raw text, causing `CS1525: Invalid expression term '<'`.

**Unity error location mismatch:** Unity reports errors ~15 lines above the actual change because the source generator's line mapping diverges from the original `.uitkx` source at that point.

#### Required support

All of the following should work at any nesting depth:

```csharp
// 1. Assign markup to a variable (already works via Pattern B: = <Tag)
var label = (<Label text="This is the log tab." />);

// 2. Arrow lambda with inline markup (NEW — Pattern C: => <Tag)
Content = () => (<Label text="This is the log tab." />)

// 3. Arrow lambda with pure C# variable reference (already works — no transpilation needed)
Content = () => label

// 4. Real-world usage inside object initializers at any depth
var tabViewProps = new TabViewProps
{
  SelectedIndex = tabIndex,
  Tabs = new List<TabViewProps.TabDef>
  {
    new TabViewProps.TabDef { Title = "Log", Content = () => label },
    new TabViewProps.TabDef { Title = "Counter", Content = () => (<Label text="Count" />) },
    new TabViewProps.TabDef { Title = "Mode", Content = () => V.Label(new LabelProps { Text = $"Mode: {mode}" }) },
  },
};
```

#### Implementation

Add **Pattern C** to `ScanForReturnMarkup()`: match `=>` followed by optional whitespace, then optionally `(`, then `<Letter`. The `=>` token is unambiguous in C# — it's always a lambda arrow. Same structure as Patterns A and B.

**Files involved:**
- `ide-extensions~/language-lib/Parser/UitkxParser.cs` — `ScanForReturnMarkup()` excludes `=>`, needs Pattern C
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `EmitCodeBlockContent()` splices markup
- `Samples/UITKX/Components/UitkxCounterFunc/UitkxCounterFunc.uitkx` — sample file with the issue

---

### [x] 5. Formatting breaks when inline markup or `@()` expressions are used inside object initializers

**Reported:** 2026-03-14

Saving to format in VS Code corrupts the code when inline markup (`<Label ... />`) or `@(expr)` expressions appear inside nested object initializers / collection initializers.

**Before (correct):**
```csharp
var tabViewProps = new TabViewProps
{
  SelectedIndex = tabIndex,
  Tabs = new List<TabViewProps.TabDef>
  {
    new TabViewProps.TabDef { Title = "Log", Content = () => @(label) or label },
    new TabViewProps.TabDef { Title = "Counter", Content = () => (<Label text="This is the log tab." />) },
    new TabViewProps.TabDef { Title = "Mode", Content = () => V.Label(new LabelProps { Text = $"Mode: {mode}" }) },
  },
  Style = new Style { (StyleKeys.Height, 160f) },
};
```

**After format (broken):**
```csharp
var tabViewProps = new TabViewProps
{
  SelectedIndex = tabIndex,
  Tabs = new List<TabViewProps.TabDef>
  {
    new TabViewProps.TabDef { Title = "Log", Content = () => @(label) or label },
    new TabViewProps.TabDef { Title = "Counter", Content = () => (
  <Label text="This is the log tab." />
)
},
new TabViewProps.TabDef { Title = "Mode", Content = () => V.Label(new LabelProps { Text = $"Mode: {mode}" }) },
},
Style = new Style { (StyleKeys.Height, 160f) },
};
```

The formatter sees `<Label .../>` inside the parentheses and treats it as markup to be formatted on its own line, which destroys the surrounding C# indentation context.

**Files involved:**
- `ide-extensions~/language-lib/Formatter/AstFormatter.cs` — main formatter logic

---

### [x] 6. `@(expr)` in C# setup code causes transient CS0266/CS1662 that disappear on save

**Reported:** 2026-03-14

#### What was happening

When a user writes `@(expr)` in a C# setup-code position (e.g. a lambda body in an object initializer):

```csharp
new TabViewProps.TabDef { Title = "Log", Content = () => @(element) },
```

VS Code shows red squiggles before saving (`CS0266` + `CS1662`) which then disappear after saving. The error was inconsistent — it should either always appear or never.

#### Root cause

`@(expr)` is UITKX markup-interpolation syntax. It is only valid **inside markup blocks** (e.g. `<Box>@(element)</Box>`). In C# setup-code context the `@` character is not valid C# (CS1056). Roslyn error-recovers by ignoring `@` and reading `(element)` as a parenthesised expression, returning its type as `object`. This cascades into CS0266/CS1662 (type mismatch). These errors happen to map back to the `.uitkx` source and appear as squiggles.

The errors disappeared on save because a `didChange` (format-on-save) re-triggers T1+T2 (clean) immediately, wiping the T3 state before the next Roslyn rebuild.

#### Fix (two-part)

1. **Branch 3** in `EmitFunctionStyleSetupSegmented` (`VirtualDocumentGenerator.cs`): strips the `@` from `@(` in setup code before Roslyn sees it. This prevents the confusing CS0266/CS1662 cascade (Roslyn now sees valid `(element)` or `(object)null!`).

2. **UITKX0306 diagnostic** in `DirectiveParser.ScanAtExprInSetupCode()`: scans the raw source setup-code ranges for `@(` patterns (outside strings/comments) and emits a clear, consistent T1 error: *"@(expr) syntax is only valid inside markup blocks. Use the expression directly."*

This means VS Code shows **one clear error** that matches Unity's rejection — no confusing type errors, no inconsistency between save states.

#### Design note — valid vs. invalid forms for `TabDef.Content`

| Form | VS Code | Unity |
|---|---|---|
| `Content = () => element` | ✅ no error | ✅ works |
| `Content = () => (<Label text="..." />)` | ✅ no error | ✅ works (Pattern C transpilation) |
| `Content = () => @(element)` | ❌ UITKX0306 | ❌ Unity error — `@(expr)` is markup-only syntax |
| `Content = element` | ❌ type mismatch | ❌ type mismatch |
| `Content = @(element)` | ❌ UITKX0306 | ❌ also invalid `@` |

`@(expr)` in C# setup code is not a supported UITKX feature. Only `() => element` (variable) and `() => (<Tag .../>)` (arrow lambda with inline markup) are intended.

**Files involved:**
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` — `EmitFunctionStyleSetupSegmented()` Branch 3 strips `@`
- `ide-extensions~/language-lib/Parser/DirectiveParser.cs` — `ScanAtExprInSetupCode()` emits UITKX0306
- `ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs` — `AtExprInSetupCode` constant
