# UITKX — JSX-Style Markup for ReactiveUIToolKit
## Complete Implementation Plan (v2 — Production-Ready)

> **AI HANDOFF NOTE**: This document is the single source of truth for the
> UITKX toolchain implementation. Every phase is self-contained and executable
> by any agent or developer without prior context. Read this file top-to-bottom
> before starting any phase. Mark phases `[DONE]` as they are completed and
> update the "Current Status" section.

---

## Current Status

| Phase | Name | Status |
|-------|------|--------|
| P1 | Shared Language Library | `[DONE]` |
| P2 | AST Formatter + Config | `[DONE]` |
| P3 | Structural Diagnostics Tier 1 + 2 | `[DONE]` |
| P4 | Semantic Tokens | `[DONE]` |
| P5 | IntelliSense (Completions / Hover / Go-To-Def) | `[DONE]` |
| P6 | Embedded Markup in `@code` | `[DONE]` |
| P7 | `PropsHelper.Bind<T>` + `[UitkxElement]` | `[DONE]` |
| P8 | Integration, Samples, Migration Docs | `[DONE]` |
| P8.6 | Developer Experience Improvements (IDE Polish) | `[~]` IN PROGRESS (only #9 remaining) |
| P9 | Structural Diagnostics Tier 3 (Embedded Roslyn) | `[ ]` NOT STARTED |
| P10 | Rider Plugin | `[ ]` NOT STARTED |

**Pre-existing work already done (do not re-do):**
- Checkmark TextMate grammar (`grammar/uitkx.tmLanguage.json`) -- covers all directives, elements, expressions, `@case` fix deployed in v1.0.17
- Checkmark Recursive-descent parser (`SourceGenerator~/Parser/UitkxParser.cs`) -- produces full AST
- Checkmark Source generator (`SourceGenerator~/UitkxGenerator.cs`) -- compiles `.uitkx` to C# via Roslyn
- Checkmark `#line` directives in generated output -- debugger breakpoints on `.uitkx` lines already work
- Checkmark VSCode extension scaffold (`ide-extensions~/vscode/`) -- v1.0.17 installed
- Checkmark VS2022 extension scaffold (`ide-extensions~/visual-studio/`) -- VSIX wired
- Checkmark LSP server (`ide-extensions~/lsp-server/`) -- net8.0, start/stop, basic formatting wired
- Checkmark Character-level formatter (`lsp-server/Formatter.cs`) -- works but will be replaced by AST formatter in Phase 2

---

## Guiding Principles

1. **No shortcuts** -- correct, maintainable implementation before convenience.
2. **Single source of truth** -- logic lives in the Shared Library; IDEs are thin wires to it.
3. **Graceful degradation** -- if the LSP server crashes, files are still valid C#; TextMate grammar gives syntax colouring.
4. **Zero runtime overhead** -- all `.uitkx` transformation is compile-time. No reflection, no dynamic dispatch.
5. **IDE-agnostic core** -- the `UitkxLanguage` library contains all language logic. VSCode, VS2022, and Rider each only need a thin adapter.
6. **Actionable errors** -- every diagnostic maps back to a character range in the original `.uitkx` file (never in generated C#).

---

## Final Developer Experience Target

```uitkx
@namespace MyGame.UI
@using MyGame.Models
@component PlayerHUD

<Box style={Styles.Outer} key="hud-root">

    <Label text={$"Health: {props.Health}"} />

    @if (props.Health < 20) {
        <Label text="WARNING LOW HEALTH" style={Styles.Warn} />
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

The toolchain gives the developer:

- Compile errors pointing at `.uitkx` file + line -- never at generated code
- Full IntelliSense: element names, attribute names, expected types
- Hover shows XML-doc comment for the Props property
- Go-to-definition on `<ItemSlot />` jumps to `ItemSlotProps.cs`
- Debugger breakpoints set on `.uitkx` lines work (via `#line` directives)
- Auto-format on save (configurable options -- see Phase 2)
- Semantic token highlighting in VSCode and Rider >= 2024.2
- `PropsHelper.Bind<T>()` bridges C# property binding to `.uitkx` props at runtime

---

## Repository Structure (End State)

```
Assets/ReactiveUIToolKit/
|
+-- Runtime/                                  (existing)
+-- Shared/                                   (existing)
+-- Editor/                                   (existing)
+-- Samples/                                  (existing)
|
+-- SourceGenerator~/                         (existing -- source generator project)
|   +-- Parser/
|   |   +-- UitkxParser.cs                   <- WILL BE MOVED to Language Library
|   |   +-- Nodes/
|   |   |   +-- *.cs
|   |   +-- ...
|   +-- UitkxGenerator.cs                    <- references Language Library
|
+-- ide-extensions~/
    +-- language-lib/                         <- NEW (Phase 1)
    |   +-- UitkxLanguage.csproj             (netstandard2.0, zero Roslyn)
    |   +-- Parser/                          <- parser MOVED here from SourceGenerator
    |   +-- Formatter/                       <- NEW (Phase 2)
    |   +-- Diagnostics/                     <- NEW (Phase 3)
    |   +-- SemanticTokens/                  <- NEW (Phase 4)
    |   +-- Analysis/                        <- NEW (Phase 5)
    |
    +-- lsp-server/                           (existing)
    |   +-- LspServer.csproj                 (net8.0)
    |   +-- Server.cs
    |   +-- Handlers/
    |   |   +-- FormattingHandler.cs         <- delegates to Language Library
    |   |   +-- DiagnosticsHandler.cs        <- delegates to Language Library
    |   |   +-- SemanticTokensHandler.cs     <- delegates to Language Library
    |   |   +-- CompletionHandler.cs         <- delegates to Language Library
    |   +-- Formatter.cs                     <- DELETE after Phase 2
    |
    +-- vscode/                               (existing)
    +-- visual-studio/                        (existing)
    +-- rider/                               <- NEW (Phase 10)
```

---

## `.uitkx` File Format Reference

> **Critical**: Every phase that touches file parsing must respect this spec.

### Directives (top of file, before any markup)
```
@namespace <dotted.name>
@using <dotted.name>
@component <PascalName>
@inject <Type> <name>
```

### Markup
- Elements: `<PascalCaseName attr="literal" attr2={csharp_expression} />`
- Attributes: string literals with double quotes, or `{c# expression}` with braced expressions
- Self-closing: `<Foo />` -- note the space before `>`
- Children: `<Box> ... </Box>`
- `key` is a reserved string attribute on every element

### Control Flow
```
@if (expr) { ... } @else if (expr) { ... } @else { ... }
@for (init; cond; step) { ... }
@foreach (var x in expr) { ... }
@while (expr) { ... }
@switch (expr) {
    @case value: ... @break
    @default: ...
}
```

### Code Block
```
@code {
    // arbitrary C# -- private statics, helpers, field initializers
    // Phase 6: return <Markup /> will be allowed here
}
```

### Generated C# (informational)
The source generator emits a partial class + `Render()` method. Each element
becomes a `UITK_<Name>(new <Name>Props { ... })` call. Control flow maps 1:1.
`#line N "path/to/file.uitkx"` directives are emitted at every statement
boundary so debugger and error locations always point to the `.uitkx` file.

---

## IDE Support Matrix (Web-Research Confirmed)

| Feature | VSCode | VS2022 | Rider >= 2023.2 | Rider >= 2024.2 |
|---|---|---|---|---|
| TextMate syntax colouring | YES | YES | YES (via TextMate bundles) | YES |
| LSP diagnostics (push) | YES | YES | YES | YES |
| LSP formatting | YES | YES | YES | YES |
| LSP hover | YES | YES | YES | YES |
| LSP completions | YES | YES | YES | YES |
| LSP go-to-definition | YES | YES | YES | YES |
| LSP rename | YES | YES | NO | NO |
| **LSP semantic tokens** | YES | **NO -- never** | NO | YES (2024.2.2+) |
| LSP code actions | YES | YES | YES | YES |
| LSP find references | YES | YES | YES (2024.2) | YES |
| LSP inlay hints | YES | YES | YES (2025.2) | YES |

**Source:**
- VS2022: https://learn.microsoft.com/en-us/visualstudio/extensibility/language-server-protocol
  (`textDocument/semanticTokens` absent from the supported feature table)
- Rider: https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html
  (`textDocument/semanticTokens/full` added in 2024.2.2)

**Architectural consequence:** VS2022 syntax highlighting is **permanently** driven by
the TextMate grammar. The semantic-tokens LSP method must be implemented but will
silently go unused by VS2022. No workaround is needed -- TextMate grammar already
covers all highlighting needs.

---

## `uitkx.config.json` Schema (End State)

```json
{
    "formatter": {
        "printWidth": 100,
        "indentSize": 4,
        "indentStyle": "space",
        "trailingComma": false,
        "bracketSameLine": false,
        "singleAttributePerLine": false,
        "closingBracketSameLine": true,
        "preserveBlankLines": true,
        "insertSpaceBeforeSelfClose": true,
        "maxConsecutiveBlankLines": 1
    },
    "diagnostics": {
        "missingKey": "warning",
        "unusedUsing": "warning",
        "deprecatedElement": "warning"
    }
}
```

**`formatter` field descriptions:**

| Field | Type | Default | Description |
|---|---|---|---|
| `printWidth` | int | 100 | Soft column limit. Long attribute lists wrap when a tag would exceed this. |
| `indentSize` | int | 4 | Number of spaces (or tab-width equivalent) per indent level. |
| `indentStyle` | "space" or "tab" | "space" | Whether to indent with spaces or hard tabs. |
| `trailingComma` | bool | false | Add trailing comma after last prop in multi-line prop spread (future use). |
| `bracketSameLine` | bool | false | When false, the closing `>` of an opening tag goes on its own line if attributes wrap. |
| `singleAttributePerLine` | bool | false | When true, force every attribute onto its own line even if they fit within `printWidth`. |
| `closingBracketSameLine` | bool | true | Keep `/>` on the line of the last attribute rather than on a new line. |
| `preserveBlankLines` | bool | true | Preserve single blank lines between sibling elements. |
| `insertSpaceBeforeSelfClose` | bool | true | Emit `<Foo />` not `<Foo/>`. |
| `maxConsecutiveBlankLines` | int | 1 | Collapse runs of blank lines down to this many. |

Config lookup order: tool searches from the `.uitkx` file upward through directories
until `uitkx.config.json` is found, identical to ESLint / Prettier.

---

# Phase 1 -- Shared Language Library

**Goal:** Extract the parser + AST from the source generator into a standalone
`netstandard2.0` library that both the source generator and LSP server can depend on.
This is the foundation every subsequent phase builds on.

**Why now:** Without this, each fix to the parser must be applied in two places.
The formatter (Phase 2), diagnostics (Phase 3), and semantic tokens (Phase 4) all
need to walk the same AST.

## 1.1 -- Create the project

Create `ide-extensions~/language-lib/UitkxLanguage.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>10</LangVersion>
    <RootNamespace>ReactiveUITK.Language</RootNamespace>
    <AssemblyName>ReactiveUITK.Language</AssemblyName>
  </PropertyGroup>
  <!-- Zero NuGet dependencies -- only BCL allowed -->
</Project>
```

**Strict constraints:**
- No dependency on `Microsoft.CodeAnalysis` (Roslyn) -- that belongs in the source generator only
- No dependency on OmniSharp, LSP libraries, or any IDE SDK
- Must compile under `netstandard2.0` and `net8.0` (for test projects)

## 1.2 -- Move the parser

**Actual file structure (verified):** All node types live in a single file;
there is no `ControlFlowParser.cs`. Move the following files from
`SourceGenerator~/` to `ide-extensions~/language-lib/`:

```
Parser/UitkxParser.cs
Parser/DirectiveParser.cs
Parser/MarkupTokenizer.cs
Parser/ExpressionExtractor.cs
Nodes/AstNode.cs              ← single file containing ALL node types
                                (ElementNode, TextNode, ExpressionNode,
                                 CodeBlockNode, IfNode, ForeachNode, ForNode,
                                 WhileNode, SwitchNode, SwitchCase, IfBranch,
                                 AttributeNode, AttributeValue variants)
```

Do NOT move `Parser/ParseResult.cs` — it depends on Roslyn's `Diagnostic`
type and will be replaced by the new Roslyn-free version defined in Phase 1.4.

Update `SourceGenerator~` `.csproj` to add a `<ProjectReference>` to
`UitkxLanguage.csproj` and remove the moved files from the SourceGenerator~
directory. The SourceGenerator will reference the shared types via the project
reference.

**Namespace changes required in every moved file:**
- `ReactiveUITK.SourceGenerator.Parser` → `ReactiveUITK.Language.Parser`
- `ReactiveUITK.SourceGenerator.Nodes` → `ReactiveUITK.Language.Nodes`

**Roslyn removal required in moved files:**
Both `UitkxParser.cs` and `DirectiveParser.cs` currently have
`using Microsoft.CodeAnalysis;` and use Roslyn's `Diagnostic` type to report
parse errors. When moved to the language library, replace every use of
`Diagnostic` / `DiagnosticDescriptor` / `Location` with the new
`ParseDiagnostic` record defined in Phase 1.4. The `List<Diagnostic> _diagnostics`
field in `UitkxParser` becomes `List<ParseDiagnostic>`.

**Access modifier change:**
`UitkxParser` is currently `internal sealed class`. It must be changed to
`public sealed class` when moved to the shared library so the LSP server and
test projects can call `UitkxParser.Parse()`.

In `SourceGenerator~/UitkxGenerator.cs`, update usings:
```csharp
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Nodes;
```

## 1.3 -- Source position tracking

**Current state (verified):** `AstNode` is a C# record with primary constructor:
```csharp
public abstract record AstNode(int SourceLine, string SourceFile);
```
Only a 1-based line number is tracked. Column and end-position are absent.

All subsequent phases (formatter, diagnostics, semantic tokens) require
precise character ranges. Extend the base record and all derived records to
carry:
```
SourceLine    (already exists, 1-based — KEEP THIS NAME to avoid mass rename)
SourceColumn  (0-based char offset of the opening token on that line)
EndLine       (1-based, inclusive)
EndColumn     (0-based, exclusive)
```

**Implementation approach:** Add the four fields to the `AstNode` primary
constructor. Propagate through every derived record definition in `AstNode.cs`.
Update all call-sites in `UitkxParser.cs`, `DirectiveParser.cs`, and
`MarkupTokenizer.cs` to supply the new values.

Do NOT rename `SourceLine` to `StartLine` — the emitter and many other
call-sites reference `SourceLine` by name and a rename would be a large,
risky change with no payoff. Use `SourceLine` / `SourceColumn` / `EndLine` /
`EndColumn` as the canonical names throughout the language library.

## 1.4 -- `ParseResult` and `ParseDiagnostic` (Roslyn-free)

**Current state (verified):** `ParseResult.cs` already exists in
`SourceGenerator~/Parser/` but it depends on Roslyn:
```csharp
using Microsoft.CodeAnalysis;
// ...
public sealed record ParseResult(
    DirectiveSet Directives,
    ImmutableArray<AstNode> RootNodes,
    ImmutableArray<Diagnostic> Diagnostics   // ← Roslyn type
);
```

This file stays in the SourceGenerator project (it will be updated to use the
new `ParseDiagnostic` type below). Do not move it to the language library.

**What to add to the language library** (`ide-extensions~/language-lib/ParseDiagnostic.cs`):

```csharp
namespace ReactiveUITK.Language;

/// <summary>
/// A Roslyn-free diagnostic produced by the UITKX parser or analyzer.
/// All positions use the same coordinate system as AstNode:
/// SourceLine = 1-based, SourceColumn/EndColumn = 0-based.
/// </summary>
public sealed class ParseDiagnostic
{
    public string Code             { get; init; } = "";
    public string Message          { get; init; } = "";
    public DiagnosticSeverity Severity { get; init; }
    public int SourceLine          { get; init; }   // 1-based
    public int SourceColumn        { get; init; }   // 0-based
    public int EndLine             { get; init; }   // 1-based
    public int EndColumn           { get; init; }   // 0-based exclusive
}

public enum DiagnosticSeverity { Error, Warning, Information, Hint }
```

Then update `SourceGenerator~/Parser/ParseResult.cs` to replace
`ImmutableArray<Diagnostic>` (Roslyn) with `ImmutableArray<ParseDiagnostic>`
(language library), and remove the `using Microsoft.CodeAnalysis;` import that
was only there for `Diagnostic`.

The `UitkxParser.Parse()` static entry point already returns `ParseResult` —
no signature change is needed. Only the type of the Diagnostics element changes.

## 1.5 -- Update LSP server project reference

In `ide-extensions~/lsp-server/LspServer.csproj`, add:
```xml
<ProjectReference Include="../language-lib/UitkxLanguage.csproj" />
```

## 1.6 -- Verification checklist
- [ ] `dotnet build` succeeds for `UitkxLanguage.csproj` with zero warnings and zero Roslyn references
- [ ] `dotnet build` succeeds for `LspServer.csproj` still
- [ ] `dotnet build` succeeds for `ReactiveUITK.SourceGenerator.csproj` still
- [ ] The existing VSCode formatter integration (character-level one) still works end-to-end
- [ ] Unity source generator still compiles `.uitkx` files correctly
- [ ] `UitkxLanguage.csproj` has zero NuGet dependencies (`dotnet list package` shows empty)

---

# Phase 2 -- AST Formatter + Full Config Options

**Goal:** Replace the current character-level `Formatter.cs` with an AST-based
pretty-printer that walks the parsed tree and produces canonical output. All
options from `uitkx.config.json` must be respected.

**Why this matters:** The current formatter has edge cases with nested elements,
expressions containing `>`, and multi-attribute alignment. The AST formatter
eliminates the entire class of "formatter corrupts code" bugs because it prints
from the structured representation, not from the source text.

## 2.1 -- `FormatterOptions` record

In `ide-extensions~/language-lib/Formatter/FormatterOptions.cs`:

```csharp
namespace ReactiveUITK.Language.Formatter;

public sealed record FormatterOptions
{
    public int    PrintWidth                { get; init; } = 100;
    public int    IndentSize               { get; init; } = 4;
    public bool   UseTabIndent             { get; init; } = false;
    public bool   TrailingComma            { get; init; } = false;
    public bool   BracketSameLine         { get; init; } = false;
    public bool   SingleAttributePerLine  { get; init; } = false;
    public bool   ClosingBracketSameLine  { get; init; } = true;
    public bool   PreserveBlankLines      { get; init; } = true;
    public bool   InsertSpaceBeforeSelfClose { get; init; } = true;
    public int    MaxConsecutiveBlankLines { get; init; } = 1;

    public static FormatterOptions Default { get; } = new();

    public static FormatterOptions FromJson(string json)
    {
        // Parse the "formatter" section from uitkx.config.json.
        // Use System.Text.Json (available as NuGet) or a hand-rolled reader
        // to avoid adding heavyweight dependencies.
    }
}
```

`ConfigLoader.cs` in language-lib:

```csharp
public static class ConfigLoader
{
    /// Walk directories from fileDirectory up to the root, return the first
    /// uitkx.config.json found.  Return FormatterOptions.Default if none found.
    public static FormatterOptions LoadFormatterOptions(string fileDirectory) { ... }
}
```

## 2.2 -- `AstFormatter` class

In `ide-extensions~/language-lib/Formatter/AstFormatter.cs`:

```csharp
namespace ReactiveUITK.Language.Formatter;

public sealed class AstFormatter
{
    private readonly FormatterOptions _opts;
    private readonly StringBuilder _sb = new();
    private int _indent = 0;

    public AstFormatter(FormatterOptions opts) { _opts = opts; }

    /// Parse source, format it, return the formatted text.
    /// On parse error, returns the original source unchanged.
    public string Format(string source)
    {
        var result = UitkxParser.Parse(source);
        if (result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            return source;   // do not mangle broken files
        FormatDocument(result);
        return _sb.ToString();
    }

    private void FormatDocument(ParseResult result) { ... }
    private void FormatNode(AstNode node) { ... }
    private void FormatElement(ElementNode el) { ... }
    private void FormatAttributes(ElementNode el) { ... }   // respects printWidth
    private void FormatDirective(DirectiveNode d) { ... }
    private void FormatControlFlow(AstNode node) { ... }
    private void FormatCodeBlock(CodeBlockNode block) { ... }
    private string IndentString() =>
        new string(_opts.UseTabIndent ? '\t' : ' ',
                   _opts.UseTabIndent ? _indent : _indent * _opts.IndentSize);
}
```

### Attribute line-wrapping algorithm

Given an element `<Foo a="1" b={expr} c="3">`:

1. Render all attributes on one line.
2. If total length <= `printWidth` AND `!singleAttributePerLine` -- keep on one line.
3. Otherwise: put each attribute on its own indented line.
   - If `bracketSameLine == false`: closing `>` or `/>` goes on its own line at the element indent level.
   - If `bracketSameLine == true`: closing `>` stays on the last attribute line.
   - `closingBracketSameLine` applies specifically to `/>` (self-closing).

### Blank line handling

- Collect consecutive blank lines between siblings.
- If `preserveBlankLines == true`: emit `min(count, maxConsecutiveBlankLines)` blank lines.
- If `preserveBlankLines == false`: strip all blank lines between siblings.

## 2.3 -- Wire formatter into LSP

In `lsp-server/Handlers/FormattingHandler.cs`:

```csharp
public Task<TextEdit[]> HandleAsync(DocumentFormattingParams p)
{
    var text   = _docs.GetText(p.TextDocument.Uri);
    var opts   = ConfigLoader.LoadFormatterOptions(GetDirectory(p.TextDocument.Uri));
    var fmt    = new AstFormatter(opts);
    var result = fmt.Format(text);
    return Task.FromResult(FullDocumentEdit(text, result));
}
```

Delete `lsp-server/Formatter.cs` once the new formatter passes all tests.

## 2.4 -- VS2022 formatting trigger

VS2022 calls `textDocument/formatting` on Save and on Format Document (Alt+Shift+F).
This already works via the existing VS2022 VSIX wire-up -- no new code needed.
Confirm with a manual test after deploying the new formatter.

## 2.5 -- Unit tests

Test cases required in `ide-extensions~/language-lib.Tests/FormatterTests.cs`:
- All attributes fit on one line
- Attributes wrap past `printWidth`
- `singleAttributePerLine = true`
- `bracketSameLine` both values
- `preserveBlankLines` and `maxConsecutiveBlankLines`
- Deeply nested elements: correct indent
- `@if` / `@foreach` / `@switch` blocks
- `@code` block body preserved verbatim (formatter must NOT touch C# inside `@code`)
- Malformed input -- returns original source unchanged

## 2.6 -- Verification checklist
- [ ] `dotnet test` passes all formatter tests
- [ ] Format Document in VSCode produces correct output
- [ ] Format Document in VS2022 produces correct output
- [ ] `uitkx.config.json` options all take effect
- [ ] `@code` block content is never modified by the formatter

---

# Phase 3 -- Structural Diagnostics Tier 1 + 2  `[DONE]`

**Goal:** Report parse errors (T1) and semantic structural errors (T2) as LSP
diagnostics that appear in the Problems panel of every IDE.

**Tier 1 -- Parser errors (syntax):** Unclosed tags, unrecognised directives,
mismatched `@if`/`@else`, illegal nesting. These come from `ParseResult.Diagnostics`.

**Tier 2 -- Semantic structural errors:** Unknown element names, duplicate `key`
attributes, `@component` name doesn't match filename, missing required `@namespace`.

**NOT in this phase:** Type errors on attribute expressions (that is Tier 3 / Phase 9).

## 3.1 -- Diagnostic code catalogue

`DiagnosticCodes.cs` in the language library:

```csharp
public static class DiagnosticCodes
{
    // T1 -- Parser errors
    public const string UnclosedTag         = "UITKX0001";
    public const string MismatchedTag       = "UITKX0002";
    public const string UnknownDirective    = "UITKX0003";
    public const string MalformedExpression = "UITKX0004";
    public const string MissingElseIf      = "UITKX0005";

    // T2 -- Structural
    public const string MissingNamespace    = "UITKX0101";
    public const string MissingComponent    = "UITKX0102";
    public const string FilenameMismatch    = "UITKX0103";
    public const string DuplicateKey        = "UITKX0104";
    public const string UnknownElement      = "UITKX0105";
    public const string MissingKey          = "UITKX0106";  // severity = warning
}
```

## 3.2 -- `DiagnosticsAnalyzer` class

`ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs`:

```csharp
public sealed class DiagnosticsAnalyzer
{
    /// projectElements: element names known in the project (from workspace symbols).
    /// Pass null to skip T2 element-name checks (when project info is not available).
    public IReadOnlyList<ParseDiagnostic> Analyze(
        ParseResult parseResult,
        string filePath,
        IReadOnlySet<string>? projectElements = null)
    { ... }
}
```

T2 checks:
1. `MissingNamespace` -- no `@namespace` directive anywhere in the file.
2. `MissingComponent` -- no `@component` directive anywhere in the file.
3. `FilenameMismatch` -- `@component Foo` but filename is `Bar.uitkx`.
4. `DuplicateKey` -- two sibling elements share the same literal `key="..."` value.
5. `UnknownElement` -- `<ItemSlot />` but `ItemSlotProps` not in `projectElements`.
   Only emit when `projectElements` is non-null.
6. `MissingKey` -- element inside `@foreach` body has no `key` attribute. Severity = Warning.

## 3.3 -- LSP diagnostics handler

`lsp-server/Handlers/DiagnosticsHandler.cs` triggers on:
`textDocument/didOpen`, `textDocument/didChange`, `textDocument/didSave`.

```csharp
public async Task PublishAsync(string uri, string text, ILanguageServerFacade server)
{
    var result   = UitkxParser.Parse(text);
    var analyzer = new DiagnosticsAnalyzer();
    var diags    = analyzer.Analyze(result, UriToPath(uri), _workspaceElements);

    await server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
    {
        Uri         = uri,
        Diagnostics = diags.Select(d => new Diagnostic
        {
            Range    = d.ToLspRange(),
            Severity = d.Severity.ToLsp(),
            Code     = d.Code,
            Source   = "uitkx",
            Message  = d.Message
        }).ToArray()
    });
}
```

## 3.4 -- Workspace element index

Add `WorkspaceIndex` to the LSP server:
1. On startup, scan workspace for all `*Props.cs` files.
2. Extract class names matching `(\w+)Props` -- strip `Props` suffix -- add to `HashSet<string>`.
3. On `workspace/didChangeWatchedFiles`, update incrementally.
4. Expose `IReadOnlySet<string> KnownElements`.

Pure text scanning with regex on `.cs` files -- no Roslyn required for Tier 2.

## 3.5 -- Verification checklist
- [ ] Unclosed tag -- red squiggle at the element start tag
- [ ] Missing `@namespace` -- error on line 1
- [ ] `@component Foo` in `Bar.uitkx` -- error on the `@component` line
- [ ] Duplicate `key="x"` siblings -- error on both elements key attributes
- [ ] `<ItemSlot />` with no `ItemSlotProps.cs` in workspace -- warning underline
- [ ] `@foreach` child without `key` -- yellow squiggle
- [ ] Diagnostics update within 500 ms of typing

---

# Phase 4 -- Semantic Tokens  [DONE]

**Goal:** Provide semantic token highlighting so elements, expressions, directive
keywords, and props attribute names get distinct colours controlled by the user theme.

**Scope:**
- VSCode: full support
- Rider >= 2024.2.2: `textDocument/semanticTokens/full` confirmed supported
- VS2022: NOT supported via LSP -- continues using TextMate grammar (no extra work needed)

## 4.1 -- Token type + modifier definitions

Register in the `Initialize` response capabilities:

```
Token types (custom to UITKX):
  uitkxElement        -- element open/close tag names  (Box in <Box>)
  uitkxDirective      -- @if, @foreach, @switch, etc.
  uitkxAttribute      -- attribute name
  uitkxExpression     -- { } expression delimiters
  uitkxDirectiveName  -- value text following @namespace, @component, @using

Token types (reuse LSP standard):
  variable
  keyword

Token modifiers:
  declaration         -- element whose Props class was found in the workspace
  defaultLibrary      -- built-in elements (Box, Label, Button)
```

## 4.2 -- `SemanticTokensProvider` class

`ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs`:

```csharp
public sealed class SemanticTokensProvider
{
    public SemanticTokenData[] GetTokens(ParseResult result)
    {
        var tokens = new List<SemanticTokenData>();
        foreach (var node in WalkAll(result.Nodes))
            CollectTokens(node, tokens);
        return tokens.OrderBy(t => t.Line).ThenBy(t => t.Column).ToArray();
    }
}

public readonly struct SemanticTokenData
{
    public int      Line      { get; init; }   // 0-based
    public int      Column    { get; init; }   // 0-based
    public int      Length    { get; init; }
    public string   TokenType { get; init; }
    public string[] Modifiers { get; init; }
}
```

## 4.3 -- LSP handler

In `lsp-server/Handlers/SemanticTokensHandler.cs` respond to
`textDocument/semanticTokens/full`. Encode results as the LSP integer delta
encoding per spec section 3.16.

Server advertises:
```json
"semanticTokensProvider": {
    "legend": { "tokenTypes": [...], "tokenModifiers": [...] },
    "full": true
}
```

## 4.4 -- Verification checklist
- [ ] Open `.uitkx` in VSCode -- element names get `uitkxElement` colour
- [ ] `@if` keyword gets `uitkxDirective` colour (distinct from element colour)
- [ ] Attribute names get `uitkxAttribute` colour
- [ ] `{ expression }` gets `uitkxExpression` colour
- [ ] Colour follows theme changes without restarting server
- [ ] VS2022: no change -- TextMate grammar colours unchanged

---

# Phase 5 -- IntelliSense (Completions + Hover + Go-To-Definition)  [DONE]

**Goal:** Upgrade the existing IntelliSense handlers to use the `WorkspaceIndex`
and language library rather than static schema data.

**Current state (verified):** `CompletionHandler.cs` and `HoverHandler.cs` already
exist in `lsp-server/` and are wired to the server. They currently use `UitkxSchema`
(a static JSON schema) and `DocumentStore`. This phase replaces the Schema-based
logic with dynamic `WorkspaceIndex` lookups, and adds Go-To-Definition.

## 5.1 -- Update `CompletionHandler.cs`

Replace the static `UitkxSchema`-based completion logic with `WorkspaceIndex`
lookups. Completion trigger points and item shapes:

| Cursor position | Completions provided |
|---|---|
| `<` | All known element names from WorkspaceIndex |
| `<Foo ` (in attribute position) | All props on `FooProps` class |
| `@` at line start | All directive keywords: if, foreach, for, while, switch, code, namespace, component, using, inject |
| `@component ` | Filename stem (auto-complete from filename) |

Completion item shape:
- `label`: completion text
- `kind`: Class for elements, Property for props, Keyword for directives
- `detail`: type signature (e.g. `string text`)
- `documentation`: XML doc comment for the props property

Props scanning -- extend `WorkspaceIndex` to parse `*Props.cs` for property declarations:

```csharp
private static readonly Regex PropRegex =
    new Regex(@"public\s+(?<type>[\w<>\[\],\s?]+)\s+(?<name>\w+)\s*\{", RegexOptions.Compiled);
```

Store as `Dictionary<string, List<PropInfo>>` where key = element name.

## 5.2 -- Update `HoverHandler.cs`

Replace the static schema hover with `WorkspaceIndex` lookups:

- Over element name: show props class name + first XML doc line
- Over attribute name: show `PropType propName` + XML doc comment
- Over `@component Foo`: show generated class signature preview

The file already exists and handles `textDocument/hover` — only the data
source changes from `UitkxSchema` to `WorkspaceIndex`.

## 5.3 -- Add `DefinitionHandler.cs` (NEW)

Create `lsp-server/DefinitionHandler.cs` — this handler does not yet exist:

- On element name `<ItemSlot />`: jump to `ItemSlotProps.cs`
- On component identifier in `@component`: jump to the generated `.g.cs` file
- On props attribute name: jump to the property declaration line in `*Props.cs`

Return `LocationLink` with both source range (the `.uitkx` identifier triggered on)
and target range (the line in the `.cs` file).

## 5.4 -- Verification checklist
- [ ] `<` -- list of known elements appears in autocomplete
- [ ] `<Box ` -- Box props appear (text, style, key, onClick, ...)
- [ ] Selecting a completion inserts it correctly
- [ ] Hover over `<Label />` shows Props summary
- [ ] Hover over `text=` shows `string text -- The label text`
- [ ] Ctrl+Click on `<ItemSlot />` jumps to `ItemSlotProps.cs`
- [ ] All of the above work in VSCode and VS2022

---

# Phase 6 -- Embedded Markup in `@code`  [DONE]

**Goal:** Allow `return <Markup />` inside `@code { }` blocks.

```uitkx
@code {
    private static VisualElement RenderBadge(string text) {
        return <Label text={text} style={Styles.Badge} />;
    }
}
```

This was "Stab-1" in the old plan. This phase implements the subset: `return <.../>`
inside `@code`. Not arbitrary interleaved C# + markup.

## 6.1 -- Dual-mode scanner

Modify `MarkupTokenizer` to accept a hint that it is scanning inside a `@code` block.
After extracting the raw `@code { }` string, run a secondary scan for
`return <ElementName` patterns. Each site produces a `ReturnMarkupNode`:

```csharp
public sealed class ReturnMarkupNode : AstNode
{
    public ElementNode Element           { get; init; }
    public int StartOffsetInCodeBlock    { get; init; }
}
```

## 6.2 -- Source generator update

In `CSharpEmitter.cs`, when emitting `@code`:
- Replace `return <Markup />` text with `return UITK_ElementName(new ElementNameProps { ... });`
- Emit `#line` directive for the `return` keyword pointing to `.uitkx` source.

## 6.3 -- Formatter update

`AstFormatter` must format embedded markup nodes using the same attribute-wrapping
rules as top-level markup.

## 6.4 -- Diagnostics update

`DiagnosticsAnalyzer` must scan `ReturnMarkupNode` children for T1/T2 checks.

## 6.5 -- Verification checklist
- [ ] `return <Label text="hi" />` inside `@code` compiles correctly
- [ ] Go-to-definition on the embedded element works
- [ ] Formatter wraps embedded element attributes per `printWidth`
- [ ] Diagnostics (e.g. unknown element) fire on embedded markup too
- [ ] `#line` directives in generated code point to the `.uitkx` file line

---

# Phase 7 -- `PropsHelper.Bind<T>` + `[UitkxElement]` Attribute  [DONE]

**Goal:** Runtime API bridging C# property binding to UITKX props.

## 7.1 -- `[UitkxElement]` attribute

In `Runtime/`:
```csharp
namespace ReactiveUITK;

/// Apply to a Props class to mark it as UITKX-managed.
/// The source generator emits this attribute automatically on generated Props.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UitkxElementAttribute : Attribute
{
    public string ComponentName { get; }
    public UitkxElementAttribute(string componentName) { ComponentName = componentName; }
}
```

Update `CSharpEmitter.cs` to emit `[UitkxElement("ComponentName")]` on the Props class.

## 7.2 -- `PropsHelper.Bind<TProps>`

In `Runtime/PropsHelper.cs`:

```csharp
public static class PropsHelper
{
    /// Create a binding that re-renders when the selected property changes.
    public static IDisposable Bind<TProps>(
        Expression<Func<TProps, object?>> selector,
        IObservable<object?> source)
        where TProps : class
    { ... }

    // Overload for INotifyPropertyChanged sources
    public static IDisposable Bind<TProps, TSource>(
        Expression<Func<TProps, object?>> selector,
        TSource source,
        Expression<Func<TSource, object?>> sourceProperty)
        where TProps : class
        where TSource : INotifyPropertyChanged
    { ... }
}
```

## 7.3 -- Source generator: Props class shape

Required shape:
- All props are `public` get/set properties (not fields)
- Class is `public sealed`
- Has `[UitkxElement("ComponentName")]`

## 7.4 -- Verification checklist
- [ ] `[UitkxElement("PlayerHUD")]` appears on the generated Props class
- [ ] `PropsHelper.Bind<PlayerHUDProps>(r => r.Health, healthObservable)` compiles
- [ ] Binding causes re-render when source changes
- [ ] IntelliSense on the selector lambda shows props

---

# Phase 8 -- Integration Samples + Migration Docs

**Goal:** End-to-end samples that demonstrate all features; migration guide for existing Func components.

## 8.1 -- Counter sample

Ensure `Samples/UitkxCounterFunc.uitkx` is the canonical source and the hand-written
`.cs` equivalent is in a `Legacy/` subfolder or removed.

## 8.2 -- PlayerHUD sample

Create `Samples/PlayerHUD/PlayerHUDProps.cs` and `PlayerHUD.uitkx` demonstrating:
- `@foreach` with `key` attribute
- `@if` / `@else`
- `@switch` / `@case`
- `[UitkxElement]`
- `PropsHelper.Bind<PlayerHUDProps>`

## 8.3 -- Migration guide

Create `ReactiveUIToolKitDocs~/MIGRATION_GUIDE.md` covering:
- Before (hand-written C# Func component)
- After (.uitkx file)
- Step-by-step instructions
- Common pitfalls

## 8.4 -- README update

Update `README.md` with:
- Quick start (under 10 minutes from zero to first component)
- Link to migration guide
- Link to sample project

## 8.5 -- Verification checklist
- [x] Counter sample compiles and renders correctly in Unity
- [x] PlayerHUD sample created (`PlayerHUD.uitkx` + `PlayerHUDProps.cs` + `PlayerHUD.cs`)
- [x] Migration guide created (`ReactiveUIToolKitDocs~/MIGRATION_GUIDE.md`)
- [x] README quick start added with samples table and migration guide link

---

# Phase 8.6 -- Developer Experience Improvements (IDE Polish)

**Goal:** Quality-of-life IDE features that make daily `.uitkx` editing faster
and more integrated with the Unity workflow.

> These are additive, standalone improvements. Each sub-task is independently
> shippable as a patch release. No Roslyn embedding (that is Phase 9).

---

## 8.6.1 -- Unity Console Click → `.uitkx` Navigation

**What:** When a runtime error or `Debug.Log` call includes a `.uitkx` file
reference (via the `#line` directives already emitted by `CSharpEmitter`),
clicking the hyperlink in the Unity Console should open the correct `.uitkx`
line in VS Code — not the generated `.g.cs` file.

**Why it works already:** The source generator emits `#line N "Filename.uitkx"`
directives in the generated C#. Unity's console already tracks these. The only
missing piece is an `[OnOpenAsset]` callback to intercept the open-asset event
and redirect VS Code to the `.uitkx` source.

**Implementation (≈30 lines, ~1-2 hours):**

```
Editor/UitkxConsoleNavigation.cs
```

```csharp
using UnityEditor.Callbacks;
using UnityEditor;
using System.Diagnostics;

public static class UitkxConsoleNavigation
{
    [OnOpenAsset]
    private static bool OnOpenAsset(int instanceId, int line)
    {
        var path = AssetDatabase.GetAssetPath(instanceId);
        if (!path.EndsWith(".uitkx", System.StringComparison.OrdinalIgnoreCase))
            return false;
        // Open in VS Code at the specific line
        Process.Start("code", $"--goto \"{System.IO.Path.GetFullPath(path)}:{line}\"");
        return true; // handled
    }
}
```

**Acceptance criteria:**
- [ ] Clicking a Unity Console message that points to a `.uitkx` line opens
      VS Code at that exact line.
- [ ] Clicking a `.uitkx` asset in the Project window also opens it in VS Code.
- [ ] Works on Windows and macOS (uses `code --goto`; no hard-coded paths).

---

## 8.6.2 -- Hook Shorthand Aliases + Setter Coloring + Hover

**What:** Three IDE/emitter improvements for hook declarations in `@code` blocks:

1. **Hook shorthand aliases** — write `useState(0)` / `useEffect(...)` (React
   camelCase style) instead of `Hooks.UseState(0)` / `Hooks.UseEffect(...)`.
   The emitter normalises the shorthand to the fully-qualified call before
   generating C#. Both forms continue to work with no breaking changes.

   ```csharp
   // Before (still works):
   var (count, setCount) = Hooks.UseState(0);
   var (mode, setMode)   = Hooks.UseState("normal");

   // After (now also works):
   var (count, setCount) = useState(0);
   var (mode, setMode)   = useState("normal");
   ```

2. **Setter coloring** — `var (count, setCount) = useState(0)` should color
   `setCount` with the `uitkxAttribute` semantic token (green/property), making
   it visually distinct from regular variables.
3. **Hover documentation** — hovering `useState`, `useEffect`, etc. shows a
   brief JSDoc-style tooltip explaining the hook's purpose and parameters.

**Implementation breakdown:**

### Hook shorthand (`CSharpEmitter.cs` + `HooksValidator.cs`)

In `EmitCodeBlockContent`, apply a regex substitution before writing the code
block text:

```csharp
private static readonly (Regex Pattern, string Replacement)[] s_hookAliases =
[
    (new Regex(@"\buseState\b"),    "Hooks.UseState"),
    (new Regex(@"\buseEffect\b"),   "Hooks.UseEffect"),
    (new Regex(@"\buseRef\b"),      "Hooks.UseRef"),
    (new Regex(@"\buseMemo\b"),     "Hooks.UseMemo"),
    (new Regex(@"\buseCallback\b"), "Hooks.UseCallback"),
    (new Regex(@"\buseSignal\b"),   "Hooks.UseSignal"),
];
```

Also add `"useState("`, `"useEffect("` etc. to `HooksValidator.s_hookPatterns`
so the rules-of-hooks validator fires on the shorthand form too.

> **Note:** `#line` directive offsets are unaffected because the substitution
> is length-neutral (`useState` ↔ `Hooks.UseState` differs in length, but
> `#line` directives reference `.uitkx` source positions, not generated C#
> positions — so no alignment issue).

Estimated effort: ~1–2 hours.

### Setter coloring (`SemanticTokensProvider.cs`)

The provider already walks `@code` blocks for element markup. Extend it to
also scan raw `@code` text for hook tuple destructuring:

```
Regex: var\s*\((?<state>\w+),\s*(?<setter>\w+)\)\s*=\s*use[A-Za-z]+\s*[<(]
```

- Emit `SemanticTokenTypes.Attribute` for the *setter* capture group.
- Matches both `useState(` and `Hooks.UseState(` forms.
- Pattern is additive; no parser changes needed.

Estimated effort: ~2 hours.

### Hover documentation (`HoverHandler.cs`)

Add a lookup table in the LSP server:

```csharp
private static readonly Dictionary<string, string> s_hookDocs = new()
{
    ["useState"]    = "**useState**`<T>(initialValue)`  \nShorthand for `Hooks.UseState`. "
                    + "Returns `(value, setter)`. Calling `setter(v)` schedules a re-render.",
    ["Hooks.UseState"] = "**Hooks.UseState**`<T>(initialValue)`  \n"
                    + "Returns `(value, setter)`. Calling `setter(v)` schedules a re-render.",
    ["useEffect"]   = "**useEffect**`(action, deps?)`  \nShorthand for `Hooks.UseEffect`. "
                    + "Runs `action` after each render (or only when `deps` change).",
    // ...
};
```

Trigger: cursor is on a word matching `use[A-Za-z]+` or `Hooks\.Use[A-Z]\w+`
inside `@code` text.

Estimated effort: ~3 hours.

**Acceptance criteria:**
- [ ] `var (count, setCount) = useState(0)` compiles to the same C# as
      `var (count, setCount) = Hooks.UseState(0)`.
- [ ] Rules-of-hooks diagnostics (UITKX0015 / 0016 / 0018) fire for both
      the shorthand and qualified forms.
- [ ] `setCount` (setter) is colored differently from `count` (state).
- [ ] Hovering `useState` or `Hooks.UseState` shows a markdown tooltip.

---

## 8.6.3 -- Function-Style Component Syntax (v2)

**What:** A top-level `function` keyword as an alternative to the current
`@code {}` + bare markup pattern:

```uitkx
@namespace MyGame.UI
@using MyGame.Models

function PlayerHUD(PlayerHUDProps props) {

    var healthText = $"Health: {props.Health}";

    return (
        <Box style={Styles.Outer}>
            <Label text={healthText} />
            @if (props.IsLowHealth) {
                <Label text="WARNING" style={Styles.Warn} />
            }
        </Box>
    );
}
```

**Why:** More natural for developers coming from React/TypeScript. The component
name is inferred from the `function` name, replacing the `@component` directive.
Old `@code` / `@component` syntax continues to work with no changes.

**Implementation plan:**

1. **`DirectiveParser.cs`** — No changes. `function` is not a directive.

2. **`UitkxParser.cs`** — Add a new entry-point mode: if the file contains
   `function <Name>(...)` at the top level (after whitespace / headers), parse
   it as a `FunctionComponentNode` instead of the default directive+markup tree.
   - Parse `function Name(Type params) { ... return (...); }` into a synthetic
     `@component Name` directive + `@code` block that wraps the body.
   - Reuse all existing `ParseElement`, `ScanForReturnMarkup`, etc. logic inside.

3. **`CSharpEmitter.cs`** — Detect `FunctionComponentNode` and emit the same
   C# output as the directive-based path. No template changes.

4. **`AstFormatter.cs`** — Format function-style files consistently:
   `function Name(Props) {` on one line, body indented 4 spaces, `}` alone.

5. **`SemanticTokensProvider.cs`** — Color `function` keyword (already has a
   `Keyword` token type). Color the function name as `uitkxElement`.

6. **Grammar (`uitkx.tmLanguage.json`)** — Add `function-component` pattern
   under the top-level `patterns` array. Use existing tag/expression/code-block
   rules for the body.

Estimated effort: ~1.5–2 days.

**Backward compatibility:** Files using `@code {}` / `@component` are unaffected.
The two syntaxes produce identical C# output.

**Acceptance criteria:**
- [ ] `function Foo(FooProps props) { return (<Box />); }` compiles to the same
      C# as the equivalent `@component Foo` / `@code { }` form.
- [ ] The function name is syntax-highlighted as an element name.
- [ ] Formatter round-trips function-style files without altering them (idempotent).
- [ ] Old `@component` / `@code` style files are unaffected.

---

---

## 8.6.4 -- JSX-Style Markup Comments `{/* */}` + Ctrl+/ Support

**What:** A native comment syntax for `.uitkx` markup that:
- Uses `{/* block comment */}` — the JSX idiom, already familiar to React devs.
- Wires `Ctrl+/` to insert `//` line comments (ideal inside `@code` blocks).
- Wires `Ctrl+Shift+/` to wrap selections in `{/* */}` (ideal inside markup).
- Colors both forms with the editor's comment theme color.

**Why not `<!-- -->`?**  
HTML-style comments already work in the parser, but they feel XML/alien next to
UITKX's JSX spirit and can't be nested. `{/* */}` is what every React dev
reaches for instinctively.

**Why not a single custom delimiter like `{{}}`?**  
`{/* */}` is zero-ambiguity with expression syntax (`={expr}`), universally
understood, and already handled by theme engines that know
`comment.block.uitkx`.

**Files to change:**

| # | File | Change |
|---|---|---|
| 1 | `vscode/language-configuration.json` | Add `lineComment: "//"` + change `blockComment` to `["{/*", "*/}"]` |
| 2 | `grammar/uitkx.tmLanguage.json` | Add `jsx-comment` repository rule + include it first in `patterns` |
| 3 | `language-lib/Parser/MarkupTokenizer.cs` | Add `TrySkipJsxComment(out int sl, out int sc, out int el, out int ec)` |
| 4 | `language-lib/Parser/UitkxParser.cs` | Call `TrySkipJsxComment` in `ParseContent()`, accumulate `CommentSpan` list on `ParseResult` |
| 5 | `lsp-server/SemanticTokensHandler.cs` | Emit `SemanticTokenTypes.Comment` for each `CommentSpan` |

**Implementation details:**

### 1. `language-configuration.json`

```jsonc
"comments": {
  "lineComment": "//",         // Ctrl+/  → inserts // (great for @code)
  "blockComment": ["{/*", "*/}"] // Ctrl+Shift+/ → wraps in {/* */} (great for markup)
}
```

Also add `{/*` / `*/}` to `autoClosingPairs` so typing `{/*` auto-inserts `*/}`.

### 2. Grammar — new `jsx-comment` rule

```json
"jsx-comment": {
  "name": "comment.block.uitkx",
  "begin": "\\{/\\*",
  "end":   "\\*/\\}",
  "beginCaptures": { "0": { "name": "punctuation.definition.comment.uitkx" } },
  "endCaptures":   { "0": { "name": "punctuation.definition.comment.uitkx" } }
}
```

Insert `{ "include": "#jsx-comment" }` at the **top** of the root `patterns`
array (above `#comment`) so it takes priority during the ~150 ms before LSP
tokens arrive.

### 3. `MarkupTokenizer.cs` — `TrySkipJsxComment`

```csharp
/// <summary>
/// Tries to skip a JSX-style comment <c>{/* ... */}</c>.
/// Returns <c>true</c> and the 1-based start/end positions when found.
/// </summary>
public bool TrySkipJsxComment(
    out int startLine, out int startCol,
    out int endLine,   out int endCol)
{
    startLine = startCol = endLine = endCol = 0;
    if (!IsAt("{/*")) return false;

    startLine = Line; startCol = Col;
    TryConsume("{/*");

    while (!IsEof)
    {
        if (IsAt("*/}")) { TryConsume("*/}"); break; }
        Advance();
    }
    endLine = Line; endCol = Col;
    return true;
}
```

### 4. `UitkxParser.cs` — `ParseContent()` addition

Right after the `TrySkipHtmlComment` check:

```csharp
// ── JSX-style comment {/* ... */} ────────────────────────────────────
if (_scanner.TrySkipJsxComment(
        out int cSL, out int cSC, out int cEL, out int cEC))
{
    _commentSpans.Add(new CommentSpan(cSL, cSC, cEL, cEC));
    continue;
}
```

where `_commentSpans` is a `List<CommentSpan>` field accumulated during a parse
pass and exposed on `ParseResult`.

### 5. `SemanticTokensHandler.cs`

After building the main token list, iterate `result.CommentSpans` and emit
`SemanticTokenTypes.Comment` tokens for each span. This ensures LSP coloring
overrides any stale TM tokens within ~150 ms.

**Acceptance criteria:**
- [ ] `{/* any text */}` in markup renders in the comment theme color.
- [ ] `{/* */}` can span multiple lines; all lines are colored as comments.
- [ ] `Ctrl+/` inserts `//` (line comment, useful in `@code`).
- [ ] `Ctrl+Shift+/` wraps the current selection in `{/* */}` (block comment).
- [ ] Typing `{/*` auto-inserts `*/}`.
- [ ] Content inside `{/* */}` is completely ignored by the parser/emitter — no
      semantic tokens, no diagnostics, no C# output.
- [ ] `<!-- -->` HTML comments continue to work as before (no regression).

Estimated effort: **~4–6 hours** (Medium).

---

## Phase 8.6 Status

> Consolidated here from the former `UITKX_P8_6_BUGS.md` tracker.
> Active remaining scope for Phase 8.6 is now **only #9**.
> **#4 `{/*` auto-close wraps wrong` was removed from scope by product decision**.

| Sub-task | Status |
|---|---|
| #1 CS compilation errors | `[x]` COMPLETED (v1.0.41) |
| #2 `@code` block semantic coloring | `[x]` COMPLETED (v1.0.44+) |
| #3 Hook setter hover docs | `[x]` COMPLETED (v1.0.51+) |
| #4 `{/*` auto-close wraps wrong | `[-]` REMOVED FROM SCOPE |
| #5 `Ctrl+/` in markup | `[x]` COMPLETED (v1.0.47) |
| #6 Unreachable code dimming | `[x]` COMPLETED (v1.0.53) |
| #7 Unity console click navigation | `[x]` COMPLETED |
| #8 Missing `;` auto-fix on save/format | `[x]` COMPLETED (v1.0.57) |
| #9 `@for` / `@while` support `break` / `continue` flow handling | `[ ]` NOT STARTED (ONLY REMAINING 8.6 ITEM) |

### 8.6 Remaining Scope — #9 (Information Only)

`#9` means UITKX loop bodies should support normal loop-control statements:

- `break;` exits the nearest enclosing `@for` / `@foreach` / `@while` loop body.
- `continue;` skips to the next iteration of the nearest enclosing loop.

Expected language behavior:

- `break;` / `continue;` are valid only inside loop bodies.
- Using them outside loops should produce clear diagnostics.
- Nested loop semantics should match C# (nearest loop wins).
- Generated C# should preserve equivalent control flow (no behavior drift).

---

# Phase 9 -- Structural Diagnostics Tier 3 (Embedded Roslyn)

> **This is Option A** -- highest-fidelity diagnostic tier. Done second-to-last
> because it is the most complex and the tool is already valuable without it.
> **Do not start Phase 9 until Phases 1-8 are complete and stable.**

**Goal:** Embed a Roslyn workspace inside the LSP server to provide full type-checking
on `{ c# expression }` attribute values and inside `@code` blocks.

**Why Roslyn in the LSP server only:** The language library must stay
`netstandard2.0` with zero Roslyn dependency (Unity source generator conflicts).
The LSP server is `net8.0` and can take heavyweight NuGet dependencies.

## 9.1 -- Architecture

```
LSP Server (net8.0)
  +-- UitkxLanguage.dll             (zero-Roslyn)
  +-- Microsoft.CodeAnalysis.CSharp.Workspaces  (added in this phase)
  +-- RoslynDiagnosticsProvider
        +-- AdhocWorkspace
        +-- Synthetic project with:
        |     +-- All .cs files in the Unity project
        |     +-- A synthetic .cs file = emitted output of the .uitkx file
        +-- Maps Roslyn diagnostic locations back to .uitkx positions
              via the #line directives in the synthetic .cs
```

## 9.2 -- Roslyn provider

`lsp-server/Roslyn/RoslynDiagnosticsProvider.cs`:

```csharp
public sealed class RoslynDiagnosticsProvider : IDisposable
{
    public async Task<IReadOnlyList<ParseDiagnostic>> GetDiagnosticsAsync(
        string uitkxFilePath, string uitkxSource, CancellationToken ct)
    {
        // 1. Run UITKX emitter to get synthetic C# text
        // 2. Update AdhocWorkspace synthetic document
        // 3. GetSemanticModelAsync()
        // 4. Filter diagnostics by mapped .uitkx position
        // 5. Return as ParseDiagnostic list
    }
}
```

## 9.3 -- `#line` directive mapping

`Diagnostic.Location.GetMappedLineSpan()` returns the .uitkx-mapped position directly
from `#line` directives. No separate source-map data structure needed.

## 9.4 -- Throttling

- Trigger Tier 3 on `textDocument/didSave` and on 1500ms idle after `didChange`
- Cancel previous analysis when a new one starts
- Tier 1 and Tier 2 diagnostics still update on every keystroke

## 9.5 -- Verification checklist
- [ ] `<Label text={42} />` -- error "Cannot convert int to string" on the `{42}` span
- [ ] Error disappears when corrected
- [ ] No lag on fast typing (Tier 1/2 instant; Tier 3 batched)
- [ ] Roslyn errors do not duplicate Tier 1/2 errors (dedup by range + message)
- [ ] Large project (1000+ .cs files) -- Tier 3 analysis completes < 3 seconds

---

# Phase 10 -- Rider Plugin

> **This is the very last phase.** Do not start until Phases 1-9 are complete
> and the LSP server is stable in production (VSCode + VS2022 users).

**Confirmed facts (JetBrains docs, verified 2025):**
- Rider LSP support requires IntelliJ Platform 2023.2+
- `textDocument/semanticTokens/full` added in Rider **2024.2.2**
- Plugin bundles the LSP server binary; spawns via `createCommandLine()`
- StdIO channel available since 2023.2; Socket since 2024.1
- TextMate grammars register via the `com.intellij.textmate` bundled plugin

**Target: Rider 2024.2 as minimum** (StdIO + formatting + hover + completions + semantic tokens)

## 10.1 -- Plugin project

Create `ide-extensions~/rider/`:

```
rider/
+-- build.gradle.kts
+-- settings.gradle.kts
+-- gradle.properties           (intellijPlatformVersion = 2024.2)
+-- plugin.xml
+-- src/main/kotlin/
    +-- com/reactiveuitk/uitkx/
        +-- UitkxLspServerSupportProvider.kt
        +-- UitkxLspServerDescriptor.kt
```

`build.gradle.kts`:
```kotlin
plugins {
    kotlin("jvm") version "1.9.21"
    id("org.jetbrains.intellij.platform") version "2.1.0"
}
intellijPlatform { rider("2024.2") }
```

## 10.2 -- `LspServerSupportProvider`

```kotlin
class UitkxLspServerSupportProvider : LspServerSupportProvider {
    override fun fileOpened(project: Project, file: VirtualFile,
                            serverStarter: LspServerStarter) {
        if (file.extension == "uitkx")
            serverStarter.ensureServerStarted(UitkxLspServerDescriptor(project))
    }
}

class UitkxLspServerDescriptor(project: Project)
    : ProjectWideLspServerDescriptor(project, "UITKX Language Server")
{
    override fun isSupportedFile(file: VirtualFile) = file.extension == "uitkx"

    override fun createCommandLine(): GeneralCommandLine {
        // Platform detection: win-x64, linux-x64, osx-arm64 -- pick correct binary
        val serverExe = PathManager.getPluginsPath() +
                        "/uitkx-rider/lsp-server/ReactiveUITK.LspServer"
        return GeneralCommandLine(serverExe, "--stdio")
    }
}
```

`plugin.xml`:
```xml
<depends>com.intellij.modules.lsp</depends>
<extensions defaultExtensionNs="com.intellij">
    <platform.lsp.serverSupportProvider
        implementation="com.reactiveuitk.uitkx.UitkxLspServerSupportProvider"/>
    <textmate.bundles bundle="uitkx.tmbundle"/>
</extensions>
```

## 10.3 -- TextMate grammar bundle

Convert `grammar/uitkx.tmLanguage.json` to XML plist format:

```
uitkx.tmbundle/
+-- info.plist
+-- Syntaxes/
    +-- uitkx.tmLanguage    (XML plist -- convert from JSON)
```

## 10.4 -- LSP server packaging

Bundle self-contained `net8.0` executables: `win-x64`, `linux-x64`, `osx-arm64`.
Add platform detection in `createCommandLine()`.

## 10.5 -- Release pipeline

Add `rider/` build to CI:
1. `./gradlew buildPlugin` produces `build/distributions/uitkx-rider-x.y.z.zip`
2. Publish to JetBrains Marketplace

## 10.6 -- Verification checklist
- [ ] Install plugin from local ZIP into Rider 2024.2
- [ ] Open `.uitkx` -- syntax colours appear (TextMate)
- [ ] LSP server starts (Language Services status bar widget shows green)
- [ ] Completions, hover, go-to-def all work
- [ ] Semantic token highlighting works (different colour for elements vs directives)
- [ ] Format on save works
- [ ] Diagnostics appear in Problems panel
- [ ] Confirmed working in Rider 2023.2 (minus semantic tokens -- expected)

---

## Cross-Cutting Concerns

### Error propagation rule
Parser errors (Tier 1) always take precedence. If a file fails to parse, suppress
Tier 2 and Tier 3 diagnostics to avoid cascading false positives.

### Thread safety
LSP server handles multiple requests concurrently. Language library classes
(`AstFormatter`, `DiagnosticsAnalyzer`, `SemanticTokensProvider`) must be stateless.
Instantiate a new object per request. Cache only `ParseResult` keyed by `(uri, version)`
in a thread-safe dictionary.

### `@code` content is opaque to non-Roslyn analysis
Phases 1-8 treat `@code { }` content as a raw string. Only Phase 9 looks inside it.
The language library must not attempt to parse C# from `@code` blocks -- only the
`ReturnMarkupNode` regex scan (Phase 6).

### Version numbers
- VSCode extension: bump to `1.1.0` after Phase 2 is complete
- VS2022 VSIX: bump minor version after Phase 2 is complete
- Rider plugin: version matches LSP server version on initial release

### Testing strategy
- Unit tests in `ide-extensions~/language-lib.Tests/` for every phase
- LSP integration tests via `OmniSharp.Extensions.LanguageServer.Testing`
- Do not skip tests -- they are the safety net until Rider is released

---

## Appendix A -- UITKX Directive Quick Reference

| Directive | Syntax | Generated C# |
|---|---|---|
| `@namespace` | `@namespace Foo.Bar` | `namespace Foo.Bar {` |
| `@using` | `@using Foo.Bar` | `using Foo.Bar;` |
| `@component` | `@component Foo` | `public partial class Foo` |
| `@inject` | `@inject IFoo foo` | Parameter on constructor |
| `@if` | `@if (x) { ... } @else { ... }` | `if (x) { ... } else { ... }` |
| `@for` | `@for (int i=0; i<n; i++) { ... }` | `for (int i=0; i<n; i++) { ... }` |
| `@foreach` | `@foreach (var x in xs) { ... }` | `foreach (var x in xs) { ... }` |
| `@while` | `@while (cond) { ... }` | `while (cond) { ... }` |
| `@switch` | `@switch (x) { @case v: ... @break }` | `switch (x) { case v: ... break; }` |
| `@code` | `@code { ... }` | Inserted verbatim into the class body |

---

## Appendix B -- Known Limitations

1. **VS2022 semantic tokens**: Not supported via LSP by Visual Studio 2022 (as of 2025).
   VS2022 uses TextMate grammar for all colouring. No workaround exists at the LSP level.
   Source: https://learn.microsoft.com/en-us/visualstudio/extensibility/language-server-protocol

2. **Rider < 2024.2 semantic tokens**: Rider 2023.2-2024.1 does not support
   `textDocument/semanticTokens` via LSP. TextMate grammar colours apply.
   Source: https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html

3. **Roslyn in the language library**: Adding Roslyn to `UitkxLanguage.csproj` would
   break the Unity source generator context (Unity Roslyn version conflict).
   The library must remain zero-dependency `netstandard2.0`.

4. **C# IntelliSense inside `@code`**: Full C# IntelliSense inside `@code` blocks
   requires the Roslyn embedding in Phase 9. Until then, the IDE uses its native
   C# IntelliSense for code within `@code`.

5. **`PropsHelper.Bind<T>` expression trees**: Expression trees require .NET Standard 2.1+.
   Ensure the Runtime assembly targets `.NET Standard 2.1`, or use a Roslyn-generated
   proxy approach if constrained to 2.0.
