# UITKX — JSX-Style Markup for ReactiveUIToolKit
## Complete Implementation Plan

---

## Guiding Principles

- Stay as close to React/JSX as possible — naming, semantics, mental model
- No shortcuts — always the correct, maintainable implementation
- Type safety at every layer — markup errors = compile errors
- Zero runtime cost — all transformation happens at compile time
- IntelliSense everywhere — `.uitkx` files feel like `.tsx` files

---

## Final Developer Experience Target

A developer writes this:

```uitkx
@namespace MyGame.UI
@using MyGame.Models
@component PlayerHUD

<Box style={Styles.Outer} key="hud-root">

    <Label text={$"Health: {props.Health}"} />

    @if (props.Health < 20) {
        <Label text="⚠ LOW HEALTH" style={Styles.Warn} />
    }

    @foreach (var item in props.Inventory) {
        <ItemSlot key={item.Id.ToString()} data={item} onPress={HandleItemPress} />
    }

    <Button text="Open Map" onClick={OpenMap} />

</Box>

@code {
    private static void HandleItemPress(Item item) { /* ... */ }
    private static void OpenMap() { /* ... */ }
}
```

And the toolchain gives them:

- ✅ Compile errors pointing at the `.uitkx` file and line — not generated code
- ✅ Full IntelliSense: element names, attribute names, attribute types
- ✅ Hover documentation pulled from C# XML docs on Props classes
- ✅ Go-to-definition on `<ItemSlot />` jumps to `ItemSlot.cs`
- ✅ Debugger breakpoints set on `.uitkx` lines work
- ✅ The generated C# is never hand-edited — it lives in Unity's temp folder

---

## Repository Structure (End State)

```
Assets/ReactiveUIToolKit/
│
├── Runtime/                          (existing)
├── Shared/                           (existing)
├── Editor/                           (existing)
├── Samples/                          (existing)
│
├── SourceGenerator/                  ← NEW standalone C# project
│   ├── ReactiveUITK.SourceGenerator.csproj
│   ├── UitkxGenerator.cs
│   ├── Parser/
│   │   ├── UitkxParser.cs
│   │   ├── DirectiveParser.cs
│   │   ├── MarkupTokenizer.cs
│   │   ├── ExpressionExtractor.cs
│   │   ├── ControlFlowParser.cs
│   │   └── Nodes/
│   │       ├── AstNode.cs
│   │       ├── ElementNode.cs
│   │       ├── TextNode.cs
│   │       ├── ExpressionNode.cs
│   │       ├── IfNode.cs
│   │       ├── ForeachNode.cs
│   │       ├── SwitchNode.cs
│   │       └── CodeBlockNode.cs
│   ├── Emitter/
│   │   ├── CSharpEmitter.cs
│   │   ├── LineMapBuilder.cs
│   │   └── PropsResolver.cs          ← Strategy B: Roslyn symbol inspection
│   ├── Diagnostics/
│   │   └── UitkxDiagnostics.cs
│   └── Tests/
│       ├── ReactiveUITK.SourceGenerator.Tests.csproj
│       ├── ParserTests.cs
│       ├── EmitterTests.cs
│       └── IntegrationTests.cs
│
├── Editor/Generators/
│   └── ReactiveUITK.SourceGenerator.dll   ← built output, labeled RoslynAnalyzer
│
└── vscode-uitkx/                     ← NEW VS Code extension
    ├── package.json
    ├── tsconfig.json
    ├── src/
    │   ├── extension.ts
    │   ├── language-server/
    │   │   ├── server.ts
    │   │   ├── completionProvider.ts
    │   │   ├── hoverProvider.ts
    │   │   ├── definitionProvider.ts
    │   │   └── diagnosticsProvider.ts
    │   └── grammar/
    │       └── uitkx.tmLanguage.json
    ├── syntaxes/
    │   └── uitkx.tmLanguage.json
    └── schemas/
        └── uitkx.schema.json
```

---

## File Format Specification

### File Extension
`.uitkx`

### Full Grammar

```
File           = Directive* Markup CodeBlock?
Directive      = '@' DirectiveName DirectiveValue NEWLINE
Markup         = Element | IfBlock | ForeachBlock | SwitchBlock | Expression | Text
Element        = '<' TagName Attribute* '/>'
               | '<' TagName Attribute* '>' Markup* '</' TagName '>'
Attribute      = AttrName '=' '{' CSharpExpr '}'
               | AttrName '=' '"' StringLiteral '"'
               | AttrName                           (boolean shorthand: present = true)
IfBlock        = '@if' '(' CSharpExpr ')' '{' Markup* '}'
                 ('@else if' '(' CSharpExpr ')' '{' Markup* '}')*
                 ('@else' '{' Markup* '}')?
ForeachBlock   = '@foreach' '(' CSharpDecl 'in' CSharpExpr ')' '{' Markup* '}'
SwitchBlock    = '@switch' '(' CSharpExpr ')' '{' SwitchCase* '}'
SwitchCase     = '@case' CSharpExpr ':' Markup* | '@default' ':' Markup*
Expression     = '@(' CSharpExpr ')'             (inline expression → renders as child VirtualNode)
CodeBlock      = '@code' '{' CSharpCode '}'
TagName        = UpperCaseName                   (function component)
               | LowerCaseName                   (built-in element)
```

### Tag Name Resolution Rules

| Tag | Resolves to |
|-----|-------------|
| `<label>` | `V.Label(new LabelProps { ... })` |
| `<button>` | `V.Button(new ButtonProps { ... })` |
| `<box>` | `V.Box(new BoxProps { ... })` |
| `<visualElement>` | `V.VisualElement(...)` |
| `<MyComponent>` | `V.Func(MyComponentRender, props)` |
| `<fragment>` | `V.Fragment(...)` |

Lowercase = built-in element mapped via PropsResolver (Strategy B).
Uppercase = function component, resolved by looking up `MyComponentRender` method in scope.

### Directives Reference

```
@namespace    MyGame.UI             Required. Namespace of the generated class.
@component    PlayerHUD             Required. Class name. Must match filename.
@using        MyGame.Models         Optional, repeatable. Adds using statement.
@props        PlayerHUDProps        Optional. Type of the props parameter.
@key          "my-default-key"      Optional. Default VirtualNode key.
```

### `props` Binding

When `@props PlayerHUDProps` is declared, the `props` variable inside markup is
strongly typed as `PlayerHUDProps` — not `Dictionary<string,object>`. The generator
emits a cast at the top of the Render method:

```csharp
var props = (PlayerHUDProps)__propsDict["props"];
// or if the whole dict IS the props:
var props = PropsHelper.Bind<PlayerHUDProps>(__rawProps);
```

(Exact binding mechanism depends on how your library passes props — confirmed in Phase 1.)

### Attribute Rules

```uitkx
<!-- String literal -- no curly braces needed for simple strings -->
<Label text="Hello World" />

<!-- C# expression -->
<Label text={playerName.ToUpper()} />

<!-- Boolean shorthand — presence = true, absence = false -->
<Button disabled />
<Button disabled={isLoading} />

<!-- Event handler -->
<Button onClick={HandleClick} />
<Button onClick={() => SetCount(count + 1)} />

<!-- Style — always an expression -->
<Label style={Styles.Warning} />

<!-- Key — always string or expression -->
<ItemSlot key={item.Id.ToString()} />

<!-- Spread (like JSX {...rest}) — phase 3 stretch goal -->
<Button {...extraProps} />
```

### Inline C# Code Blocks (`@code`)

Placed at the bottom of the file, inside the generated partial class:

```uitkx
@code {
    private static readonly Style s_warn = new Style
    {
        (StyleKeys.Color, Color.red),
    };

    private static void HandleClose() => SetOpen(false);
}
```

### Control Flow Examples

```uitkx
@if (isLoggedIn) {
    <UserPanel user={currentUser} />
} @else {
    <LoginForm />
}

@foreach (var item in items) {
    <ItemRow key={item.Id.ToString()} item={item} />
}

@switch (status) {
    @case "loading":
        <Spinner />
    @case "error":
        <ErrorBanner message={errorMessage} />
    @default:
        <ContentPanel data={data} />
}
```

---

## Phase 1 — Roslyn Source Generator Foundation
**Estimated time: 5–7 days**

### 1.1 — Create the generator project

Create `SourceGenerator/ReactiveUITK.SourceGenerator.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    <!-- Output the DLL directly into the Unity asset folder -->
    <OutputPath>..\Editor\Generators\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <!-- Pin to the version Unity's Roslyn uses (as of Unity 6) -->
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

The `.meta` file for `ReactiveUITK.SourceGenerator.dll` must contain:
```yaml
labels:
- RoslynAnalyzer
```

Without this label Unity will NOT run the generator.

### 1.2 — Register `.uitkx` files as AdditionalFiles

Create `Assets/ReactiveUIToolKit/Directory.Build.props`:

```xml
<Project>
  <ItemGroup>
    <AdditionalFiles Include="**\*.uitkx" />
  </ItemGroup>
</Project>
```

Confirm Unity picks this up by checking that `context.AdditionalTextsProvider` yields the files (add temporary `Debug.Log` in a test generator first).

### 1.3 — IIncrementalGenerator entry point (UitkxGenerator.cs)

Incremental generators (not the old ISourceGenerator) are required for performance — Unity recompiles frequently and we must not regenerate files whose source hasn't changed.

```csharp
[Generator]
public sealed class UitkxGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Grab all .uitkx additional files
        var uitkxFiles = context.AdditionalTextsProvider
            .Where(static f => f.Path.EndsWith(".uitkx",
                StringComparison.OrdinalIgnoreCase));

        // 2. Combine with compilation (needed for PropsResolver / Strategy B)
        var combined = uitkxFiles.Combine(context.CompilationProvider);

        // 3. Transform: parse + resolve + emit
        var generated = combined.Select(static (pair, ct) =>
        {
            var (file, compilation) = pair;
            var source = file.GetText(ct)?.ToString() ?? string.Empty;
            return UitkxPipeline.Run(source, file.Path, compilation, ct);
        });

        // 4. Register source output
        context.RegisterSourceOutput(generated, static (spc, result) =>
        {
            foreach (var diag in result.Diagnostics)
                spc.ReportDiagnostic(diag);

            if (result.Source is not null)
                spc.AddSource(result.HintName, result.Source);
        });
    }
}
```

`UitkxPipeline.Run` is the single entry point that chains:
`DirectiveParser → Tokenizer → MarkupParser → PropsResolver → CSharpEmitter`

### 1.4 — Build pipeline for the generator itself

Add a pre-build step (PowerShell script in `scripts/`) that builds the generator and copies the DLL to the correct location:

```powershell
# scripts/build-generator.ps1
Push-Location "$PSScriptRoot/../SourceGenerator"
dotnet build -c Release
Pop-Location
Write-Host "Generator built successfully."
```

This is run manually by developers after modifying the generator. The Unity asset pipeline picks up the new DLL automatically.

---

## Phase 2 — Parser
**Estimated time: 7–10 days**

### 2.1 — AST Node Hierarchy (Nodes/)

All nodes carry `SourceLine` and `SourceFile` for `#line` emission:

```csharp
// AstNode.cs
public abstract record AstNode(int SourceLine, string SourceFile);

// ElementNode.cs
public record ElementNode(
    string TagName,
    ImmutableArray<AttributeNode> Attributes,
    ImmutableArray<AstNode> Children,
    int SourceLine,
    string SourceFile
) : AstNode(SourceLine, SourceFile);

// AttributeNode.cs
public record AttributeNode(
    string Name,
    AttributeValue Value,   // StringLiteral | CSharpExpression | BooleanShorthand
    int SourceLine
);

// IfNode.cs
public record IfNode(
    ImmutableArray<IfBranch> Branches,  // condition + body pairs, last may be else
    int SourceLine,
    string SourceFile
) : AstNode(SourceLine, SourceFile);

// ForeachNode.cs
public record ForeachNode(
    string IteratorDeclaration,   // "var item"
    string CollectionExpression,  // "props.Inventory"
    ImmutableArray<AstNode> Body,
    int SourceLine,
    string SourceFile
) : AstNode(SourceLine, SourceFile);

// TextNode.cs
public record TextNode(string Content, int SourceLine, string SourceFile)
    : AstNode(SourceLine, SourceFile);

// ExpressionNode.cs  — @(expr) inline expression
public record ExpressionNode(string Expression, int SourceLine, string SourceFile)
    : AstNode(SourceLine, SourceFile);

// CodeBlockNode.cs
public record CodeBlockNode(string Code, int SourceLine, string SourceFile)
    : AstNode(SourceLine, SourceFile);
```

### 2.2 — Tokenizer (MarkupTokenizer.cs)

Write a hand-written recursive descent tokenizer (not a regex soup). Token types:

```csharp
public enum TokenKind
{
    OpenTag,         // <TagName
    CloseTag,        // </TagName>
    SelfCloseEnd,    // />
    OpenTagEnd,      // >
    AttrName,        // someProp
    AttrEq,          // =
    AttrStringValue, // "literal"
    AttrExprValue,   // {expr}
    Directive,       // @if, @foreach, @code, @namespace, etc.
    CSharpBlock,     // { ... } — tracks brace depth
    Text,            // raw text between elements
    Expression,      // @(expr)
    EOF,
}
```

The tokenizer must handle:
- Brace depth counting for `{expr}` extraction (expressions may contain nested braces)
- C# string literals inside expressions (including verbatim strings `@"..."` and raw strings `"""..."""`)
- Nested tags of the same name (e.g., `<Box>` inside `<Box>`)
- HTML-style comments `<!-- -->` stripped before tokenizing

### 2.3 — DirectiveParser (DirectiveParser.cs)

Runs first, extracts all `@directive value` lines from the top of the file and validates required directives are present:

```csharp
public sealed class DirectiveParser
{
    public DirectiveSet Parse(string source, string filePath, 
                              DiagnosticBag diagnostics);
}

public sealed record DirectiveSet(
    string Namespace,        // required
    string ComponentName,    // required, must match filename
    string? PropsTypeName,   // optional
    string? DefaultKey,      // optional
    ImmutableArray<string> Usings
);
```

Emits `UITKX005` if `@namespace` or `@component` are missing.
Emits `UITKX006` if `@component` value doesn't match the filename (warning, not error).

### 2.4 — MarkupParser (UitkxParser.cs)

Consumes tokens from the tokenizer, produces an `ImmutableArray<AstNode>` root tree.

Key parser methods:
```csharp
private AstNode ParseNode();
private ElementNode ParseElement(Token openTag);
private IfNode ParseIf();
private ForeachNode ParseForeach();
private SwitchNode ParseSwitch();
private ExpressionNode ParseInlineExpression();
private AstNode ParseCodeBlock();
private ImmutableArray<AttributeNode> ParseAttributes();
private AttributeValue ParseAttributeValue();
```

Error recovery: on a parse error, emit the diagnostic and skip to the next `>` or `@` to continue parsing — never abort the whole file on one error.

### 2.5 — Expression Extractor (ExpressionExtractor.cs)

`{expr}` values may contain anything valid in C#. Extract by tracking:
- `{` / `}` brace depth
- String literals: `"..."`, `@"..."`, `$"..."`, `"""..."""`
- Comments: `//` and `/* */`

Returns the raw expression string verbatim — the generator never interprets it, just embeds it.

---

## Phase 3 — PropsResolver (Strategy B: Roslyn Symbol Inspection)
**Estimated time: 5–7 days**

This is the core of type safety. The resolver uses the live `Compilation` to discover all Props types and build the attribute-to-property mapping at generation time.

### 3.1 — Discovery

```csharp
public sealed class PropsResolver
{
    public PropsResolver(Compilation compilation) { ... }

    // Returns the PropsType and PropertyInfo for a given (tagName, attrName)
    public bool TryResolve(
        string tagName,        // e.g. "label"
        string attrName,       // e.g. "text"
        out ResolvedAttribute result);
}

public record ResolvedAttribute(
    string PropsTypeName,     // "LabelProps"
    string PropertyName,      // "Text"
    ITypeSymbol PropertyType, // the Roslyn type — used for type-checking expressions
    string? XmlDocSummary     // for hover in the language server
);
```

### 3.2 — PropsType Convention Discovery

The generator needs to know that `<label>` maps to `LabelProps`. Two strategies can be combined:

**Strategy B-1 — Naming convention** (primary, zero config):
Lowercase tag `label` → look for a type named `LabelProps` or `LabelElement` in the compilation.

**Strategy B-2 — Attribute annotation** (explicit override):
```csharp
[UitkxElement("label")]
public class LabelProps { ... }
```
A custom `[UitkxElement]` attribute on Props classes. The resolver collects all types decorated with this attribute.

Both strategies run; annotation wins on conflict.

### 3.3 — Property Mapping

For each resolved Props type, enumerate its public settable properties via Roslyn:

```csharp
var propsType = compilation.GetTypeByMetadataName("ReactiveUITK.Props.Typed.LabelProps");
var properties = propsType.GetMembers()
    .OfType<IPropertySymbol>()
    .Where(p => p.SetMethod != null && p.DeclaredAccessibility == Accessibility.Public);
```

Map attribute names by: lowercase the property name (`Text` → `text`, `OnClick` → `onClick`).
Store a reverse map: `("label", "text") → PropertySymbol(Text, string)`.

### 3.4 — Function Component Resolution

Uppercase tags like `<ItemSlot />` resolve to function components. The resolver looks for:

1. A type named `ItemSlot` in the compilation with a `Render` static method matching the `Func<Dictionary<string,object>, IReadOnlyList<VirtualNode>, VirtualNode>` signature
2. OR a static method named `ItemSlotRender` in any accessible type

If found: emit `V.Func(ItemSlot.Render, new Dictionary<string,object>{{ ... }})`.
If not found: emit `UITKX001 — Unknown component 'ItemSlot'`.

### 3.5 — Type-Checking Attribute Expressions

Because the resolver holds `ITypeSymbol` for each property, it can validate that expressions are assignable. For `onClick={HandleClick}`, the resolver knows `OnClick` is `Action` and can check that `HandleClick` is an `Action` (or compatible lambda). This uses Roslyn's `Compilation.ClassifyCommonConversion`. Errors emit `UITKX007 — Type mismatch: expected {expected}, got {actual}`.

---

## Phase 4 — C# Emitter
**Estimated time: 5–7 days**

### 4.1 — Output Structure

The emitter produces a single `.cs` source string with this skeleton:

```csharp
// <auto-generated — do not edit — source: PlayerHUD.uitkx />
#nullable enable

#line 1 "Assets/MyGame/UI/PlayerHUD.uitkx"

namespace MyGame.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using ReactiveUITK.Core;
    using ReactiveUITK.Props.Typed;
    using MyGame.Models;                   // from @using directives

    public partial class PlayerHUD
    {
        public static VirtualNode Render(
            Dictionary<string, object> __rawProps,
            IReadOnlyList<VirtualNode> __children)
        {
            // props binding (when @props is declared)
            var props = ReactiveUITK.Core.PropsHelper.Bind<PlayerHUDProps>(__rawProps);

#line 5 "Assets/MyGame/UI/PlayerHUD.uitkx"
            return V.Box(new BoxProps { Style = Styles.Outer, Key = "hud-root" }, key: null,
#line 7 "Assets/MyGame/UI/PlayerHUD.uitkx"
                V.Label(new LabelProps { Text = $"Health: {props.Health}" }),
#line 9 "Assets/MyGame/UI/PlayerHUD.uitkx"
                props.Health < 20
                    ? V.Label(new LabelProps { Text = "⚠ LOW HEALTH", Style = Styles.Warn })
                    : null,
#line 13 "Assets/MyGame/UI/PlayerHUD.uitkx"
                props.Inventory.Select(__item_0 =>
#line 14 "Assets/MyGame/UI/PlayerHUD.uitkx"
                    V.Func(ItemSlot.Render,
                           new Dictionary<string,object>{ {"data", __item_0} },
                           key: __item_0.Id.ToString())
                ).ToArray(),
#line 17 "Assets/MyGame/UI/PlayerHUD.uitkx"
                V.Button(new ButtonProps { Text = "Open Map", OnClick = OpenMap })
            );
        }

        // @code block content pasted verbatim:
#line 22 "Assets/MyGame/UI/PlayerHUD.uitkx"
        private static readonly Style s_warn = new Style { (StyleKeys.Color, Color.red) };
        private static void HandleClose() => SetOpen(false);
    }
}
```

### 4.2 — #line Directive Strategy

Every `V.` call emission is preceded by a `#line N "path"` directive using the source line of the corresponding markup node. The `LineMapBuilder` class is responsible for inserting these.

Rules:
- One `#line` per top-level statement in the return expression
- C# expressions embedded verbatim (not on separate lines) inherit the `#line` of their containing element
- The `@code` block gets `#line` directives for every statement within it
- `// <auto-generated>` header is preceded by `#line default` then `#line hidden` to prevent stepping into boilerplate during debug

### 4.3 — Control Flow Emission

**@if → ternary chain** (2 or 3 branches) OR **immediately-invoked lambda** (4+ branches or complex bodies):

```csharp
// Simple ternary:
condition1 ? V.A() : condition2 ? V.B() : V.C()

// Complex (lambda):
((System.Func<VirtualNode>)(() => {
    if (condition1) return V.A();
    else if (condition2) return V.B();
    return V.C();
}))()
```

**@foreach → .Select().ToArray()**:
```csharp
items.Select(__item_0 => V.Func(...)).ToArray()
```
Iterator variable is renamed `__item_0`, `__item_1` etc. to avoid collision.

**@switch → switch expression** (C# 8):
```csharp
status switch {
    "loading" => V.Spinner(...),
    "error"   => V.ErrorBanner(...),
    _         => V.ContentPanel(...)
}
```

### 4.4 — Null Safety

Every child position that may be `null` (from `@if` without `@else`) is wrapped in a null-filtering helper:

```csharp
// Generated at top of file once:
private static VirtualNode[] __FilterChildren(params VirtualNode?[] children)
    => children.Where(c => c != null).Select(c => c!).ToArray();
```

So the return becomes `V.Box(..., __FilterChildren(child1, child2, mabeNull))`.
This mirrors React's behavior where `{condition && <Foo />}` renders nothing when false.

### 4.5 — Hint Name Strategy

`spc.AddSource(hintName, source)` — Unity uses the hint name as the filename in its temp compilation folder. Use:

```
PlayerHUD.uitkx.g.cs
```

Unique per file, clearly identifies provenance.

---

## Phase 5 — Diagnostics
**Estimated time: 2–3 days**

### Full Diagnostic Table

| ID | Severity | When | Example Message |
|---|---|---|---|
| UITKX001 | Error | Unknown built-in element | Unknown element `<grid>`. Did you mean `<box>`? |
| UITKX002 | Error | Unknown attribute on known element | Unknown attribute `tex` on `<label>`. Did you mean `text`? |
| UITKX003 | Error | Mismatched closing tag | Expected `</Box>` but found `</button>` |
| UITKX004 | Error | Unclosed tag | Unclosed element `<Button>` opened at line 12 |
| UITKX005 | Error | Missing required directive | Missing `@namespace` directive |
| UITKX006 | Warning | Component name ≠ filename | `@component PlayerPanel` but file is `PlayerHUD.uitkx` |
| UITKX007 | Error | Type mismatch on attribute | `onClick` expects `Action` but found `string` |
| UITKX008 | Warning | Unknown component | Unknown component `<FooBar>`. Is it in scope? |
| UITKX009 | Error | foreach missing key | Elements inside `@foreach` should have a `key` attribute |
| UITKX010 | Warning | Duplicate key in siblings | Duplicate key `"item"` among sibling elements |
| UITKX011 | Error | Expression syntax error | Brace mismatch in expression `{items.Select(i => i.Id}` |
| UITKX012 | Error | @component must be first directive | `@namespace` must appear before `@component` |

Each diagnostic is constructed as a `Diagnostic.Create(descriptor, location)` where `location` is a `Location` built from the `.uitkx` file path and line/column. Because of the `#line` directives in generated code, Roslyn automatically maps these back to the `.uitkx` source.

---

## Phase 6 — Generator Unit Tests
**Estimated time: 3–5 days**

### Test Project Setup

Create `SourceGenerator/Tests/ReactiveUITK.SourceGenerator.Tests.csproj` targeting `net8.0`, using:
- `Microsoft.CodeAnalysis.CSharp.Workspaces`
- `Microsoft.CodeAnalysis.Testing` (the official Roslyn test helpers)
- `xunit`

### Test Categories

**Parser tests** — verify AST shape for given markup:
```csharp
[Fact]
public void ParseSimpleLabel()
{
    var ast = UitkxParser.Parse("<Label text=\"Hello\" />");
    Assert.Single(ast);
    var el = Assert.IsType<ElementNode>(ast[0]);
    Assert.Equal("Label", el.TagName);
    Assert.Single(el.Attributes);
    Assert.Equal("text", el.Attributes[0].Name);
}
```

**Emitter tests** — verify generated C# string for given AST:
```csharp
[Fact]
public void EmitLabel()
{
    var source = RunGenerator("<Label text=\"Hello\" />");
    Assert.Contains("V.Label(new LabelProps { Text = \"Hello\" })", source);
}
```

**Diagnostic tests** — verify correct errors are reported:
```csharp
[Fact]
public void ErrorOnUnknownElement()
{
    var diags = RunGeneratorDiagnostics("<grid />");
    Assert.Contains(diags, d => d.Id == "UITKX001");
}
```

**Integration tests** — run the full generator against a mock Unity compilation and assert the output compiles:
```csharp
[Fact]
public void GeneratedCodeCompiles()
{
    var uitkxSource = File.ReadAllText("TestFiles/PlayerHUD.uitkx");
    var generatedCs = RunFullPipeline(uitkxSource, mockCompilation);
    var compilation = CSharpCompilation.Create(...)
        .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedCs));
    Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
}
```

---

## Phase 7 — VS Code Extension
**Estimated time: 10–14 days**

### 7.1 — Project Structure

```
vscode-uitkx/
├── package.json           ← extension manifest
├── tsconfig.json
├── esbuild.config.js      ← bundle to single .js
├── src/
│   ├── extension.ts       ← activation, registers language client
│   ├── server/
│   │   ├── server.ts      ← LSP server entry point
│   │   ├── analyzer.ts    ← parses .uitkx and calls into schema
│   │   ├── schema/
│   │   │   ├── schemaLoader.ts    ← loads generated schema JSON
│   │   │   └── schemaTypes.ts
│   │   ├── providers/
│   │   │   ├── completion.ts
│   │   │   ├── hover.ts
│   │   │   ├── definition.ts
│   │   │   └── diagnostics.ts
│   └── grammar/
│       └── uitkx.tmLanguage.json
```

### 7.2 — Schema Generation (Bridge between Generator and Extension)

The Roslyn generator, as a post-build step, writes a schema file alongside the DLL:

```
Editor/Generators/uitkx-schema.json
```

Format:
```json
{
  "version": "1.0",
  "elements": {
    "label": {
      "propsType": "ReactiveUITK.Props.Typed.LabelProps",
      "attributes": {
        "text": { "type": "string", "doc": "The displayed text content." },
        "style": { "type": "ReactiveUITK.Core.Style", "doc": "Visual style overrides." },
        "key":   { "type": "string", "doc": "Reconciler key for list stability." }
      }
    },
    "button": {
      "propsType": "ReactiveUITK.Props.Typed.ButtonProps",
      "attributes": {
        "text":    { "type": "string", "doc": "Button label text." },
        "onClick": { "type": "System.Action", "doc": "Callback when the button is clicked." },
        "style":   { "type": "ReactiveUITK.Core.Style", "doc": "Visual style overrides." },
        "disabled":{ "type": "bool", "doc": "When true, the button is non-interactive." }
      }
    }
  },
  "components": {
    "PlayerHUD": {
      "file": "Assets/MyGame/UI/PlayerHUD.cs",
      "propsType": "MyGame.Models.PlayerHUDProps",
      "doc": "Displays the player's health, inventory, and map button."
    }
  }
}
```

This schema is read by the VS Code extension's language server. When the generator re-runs (e.g., a new Props property was added), the schema regenerates, the extension's file watcher picks it up, and IntelliSense updates without restarting VS Code.

### 7.3 — Syntax Highlighting (TextMate Grammar)

`uitkx.tmLanguage.json` scopes:
- Directive keywords (`@namespace`, `@component`, `@if`, `@foreach`, `@code`) → keyword color
- Tag names → entity.name.tag
- Attribute names → entity.other.attribute-name
- `{expr}` blocks → source.cs.embedded (triggers C# highlighting inside braces)
- String literals in attributes → string
- XML comments `<!-- -->` → comment

### 7.4 — Language Server Features

**Completions:**
- At `<` → suggest all known element names (from schema)
- Inside open tag after element name → suggest attribute names not yet on this element
- After `={` → suggest nothing (C# expression, IDE handles it)
- After `@` at line start → suggest directive keywords and control flow keywords

**Hover:**
- On element name → show `propsType` and component doc string
- On attribute name → show type and XML doc summary from schema

**Go-to-Definition:**
- On `<MyComponent />` → open the `.cs` file listed in schema under `components`
- On attribute name → jump to the property declaration in the Props class (via schema file path)

**Diagnostics:**
- Mirror `UITKX001`–`UITKX012` live without requiring a compile
- Unknown elements and attributes highlighted in red as the developer types

### 7.5 — Extension Manifest (package.json, key parts)

```json
{
  "name": "reactiveuitk-uitkx",
  "displayName": "ReactiveUIToolKit UITKX",
  "contributes": {
    "languages": [{
      "id": "uitkx",
      "extensions": [".uitkx"],
      "configuration": "./language-configuration.json"
    }],
    "grammars": [{
      "language": "uitkx",
      "scopeName": "source.uitkx",
      "path": "./syntaxes/uitkx.tmLanguage.json"
    }],
    "configuration": {
      "properties": {
        "uitkx.schemaPath": {
          "type": "string",
          "description": "Path to uitkx-schema.json relative to workspace root.",
          "default": "Assets/ReactiveUIToolKit/Editor/Generators/uitkx-schema.json"
        }
      }
    }
  }
}
```

### 7.6 — Distribution

1. Build: `npm run package` → produces `reactiveuitk-uitkx-x.x.x.vsix`
2. Commit the `.vsix` to `scripts/vscode-extension/`
3. Add to `.vscode/extensions.json`:
```json
{
  "recommendations": ["reactiveuitk.reactiveuitk-uitkx"]
}
```
4. Add to `README.md`: one-liner install command:
```bash
code --install-extension scripts/vscode-extension/reactiveuitk-uitkx-x.x.x.vsix
```

---

## Phase 8 — Runtime/Library Support
**Estimated time: 2–3 days**

### 8.1 — PropsHelper.Bind<T>

The generated code calls `PropsHelper.Bind<PlayerHUDProps>(__rawProps)`. This needs to live in `Shared/Core/`:

```csharp
public static class PropsHelper
{
    public static T Bind<T>(Dictionary<string, object> rawProps) where T : new()
    {
        // Use cached reflection or code-generated binders.
        // Maps dictionary keys to strongly-typed properties.
    }
}
```

This replaces the manual `props.TryGetValue("health", out var h) && h is int hi ? hi : 0` pattern — the binding is automatic and type-safe.

### 8.2 — [UitkxElement] Attribute

Add to `Shared/Core/`:
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UitkxElementAttribute : Attribute
{
    public string TagName { get; }
    public UitkxElementAttribute(string tagName) => TagName = tagName;
}
```

Annotate all existing Props types:
```csharp
[UitkxElement("label")]
public class LabelProps { ... }

[UitkxElement("button")]
public class ButtonProps { ... }
```

---

## Phase 9 — Integration & Samples
**Estimated time: 3–4 days**

### 9.1 — Convert one existing Func component to .uitkx

Pick `EffectCleanupOrderDemoFunc` as the first conversion target (it has conditional rendering and loops). Convert it, verify:
- Build succeeds
- Generated `.g.cs` is in Unity's temp folder
- Demo window still works
- Debugger breakpoints in `.uitkx` file hit correctly

### 9.2 — Add a dedicated Samples/UITKX/ folder

```
Samples/UITKX/
├── HelloWorld.uitkx
├── HelloWorld.cs       ← partial class, empty (no @code needed)
├── ThemeDemo.uitkx
├── ThemeDemo.cs
└── ReactiveUITK.Examples.UITKX.asmdef
```

### 9.3 — Document the migration path

Add `UITKX_MIGRATION.md` explaining:
- How to convert an existing `V.`-style component to `.uitkx`
- Both styles are permanently supported — no forced migration
- When to use each (`.uitkx` for UI-heavy components, `V.` for programmatic/dynamic)

---

## Implementation Order Summary

| Phase | What | Days |
|---|---|---|
| 1 | Generator project setup + entry point + AdditionalFiles wiring | 5–7 |
| 2 | Tokenizer + Parser + AST nodes | 7–10 |
| 3 | PropsResolver (Strategy B, Roslyn symbol inspection) | 5–7 |
| 4 | C# Emitter + #line directives | 5–7 |
| 5 | Full diagnostic suite | 2–3 |
| 6 | Generator unit tests | 3–5 |
| 7 | VS Code extension (grammar + LSP + schema) | 10–14 |
| 8 | Runtime library support (PropsHelper, [UitkxElement]) | 2–3 |
| 9 | Integration, samples, migration docs | 3–4 |
| **Total** | | **~42–60 days** |

Phases 1–6 can ship as a usable (no IntelliSense) MVP.
Phase 7 is the quality-of-life layer that makes it feel like `.tsx`.
Phases 8–9 polish and prove it in production.

---

## Key Decisions Locked In

| Decision | Choice | Reason |
|---|---|---|
| Tag case convention | Uppercase = component, lowercase = built-in | Identical to JSX |
| Style attribute | Always `style={expression}` | No CSS string parsing complexity, identical to JSX |
| Props mapping | Strategy B: Roslyn symbol inspection | Self-updating, no maintenance table |
| Generator type | `IIncrementalGenerator` | Required for Unity performance |
| Control flow syntax | `@if`, `@foreach`, `@switch` | Identical to Razor/Blazor, familiar to C# devs |
| IntelliSense bridge | Schema JSON generated by Roslyn generator | Decouples generator from VS Code; no LSP-in-generator hacks |
| Extension distribution | `.vsix` committed to repo + `extensions.json` recommendation | Zero friction for new developers |
| Existing `V.` API | Kept forever, not deprecated | `.uitkx` compiles to `V.` calls; both are always valid |
