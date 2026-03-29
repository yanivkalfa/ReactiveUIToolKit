# UITKX Language Server — Architectural Design

**Status:** Reference document — living design record.  
**Scope:** The IDE-side language server stack (`ide-extensions~/`).  
**Purpose:** Record the three-layer architecture, the current state of each layer, the dependency
graph between the four open feature areas, and the rationale for the implementation order.

---

## 1. The Three Layers

Your language stack has three independent layers that currently operate in parallel with unclear
ownership boundaries.

| # | Layer | Authority today | Runs when |
|---|-------|-----------------|-----------|
| 1 | **TextMate grammar** (`uitkx.tmLanguage.json`) | Static regex tokenization | File opens (zero latency, no LSP needed) |
| 2 | **Semantic tokens** (`SemanticTokensProvider.cs` via LSP) | AST-driven tokenization, *overrides* TmLanguage for any range it emits | After LSP handshake (~1 s) |
| 3 | **Language services** (Completion, Hover, Formatting, Diagnostics) | Position-aware features driven by `AstCursorContext` | On-demand, after LSP ready |

### What "override" means

The LSP `textDocument/semanticTokens/full` response instructs the editor to re-colour any range
the server touches.  If the server emits a token for a range that the TmLanguage grammar also
coloured, the server's colouring wins.  This is by design in the LSP specification.

### The current conflict

Both layer 1 and layer 2 colour the same ranges:

- TmLanguage colours tag names, directives, attributes, and — poorly — C# inside `@code` / `@(expr)`.
- `SemanticTokensProvider` colours tag names (`uitkxElement`), directives (`uitkxDirective`),
  attributes (`uitkxAttribute`), and expression delimiters (`uitkxExpression`).

There is **no written contract** about which layer owns which region.  Before the LSP connects,
TmLanguage is the only source.  After it connects, the editor tries to reconcile both, and the
result depends on IDE settings (`editor.bracketPairColorization.enabled`, theme token-mapping
rules, etc.).

The `embeddedLanguages` key in `package.json` declares
`"source.cs.embedded.uitkx": "csharp"` — this would allow VS Code's C# extension to activate
inside embedded C# regions — but the TmLanguage grammar **never emits** that scope name on
`@code` block content.  Therefore embedded C# IDE support (auto-indent, native completions)
never activates.

### The goal

- **TmLanguage**: fallback only — covers the "pre-connection" flash.  It must be kept in sync
  with semantic token coverage but must never fight with it.  No range that the LSP handles
  should be given a strongly-opinionated colour by the grammar.
- **Semantic tokens (layer 2)**: the **single authoritative** source for all colouring once the
  LSP is connected.  Covers all UITKX ranges *and* all C# ranges inside the file.
- **Layer 3 services**: driven by the same AST-parsing pipeline as semantic tokens; consistent
  by construction.

---

## 2. Feature Area Status

### ~~Item 1 — Embedded Roslyn (Tier-3 Diagnostics + C# Semantic Services)~~ ✅ DONE

**P9 in `UITKX_IMPLEMENTATION_PLAN.md`.** Fully implemented.

The LSP server embeds a full Roslyn compilation host (`RoslynHost.cs`) with per-file
`AdhocWorkspace` instances. Key infrastructure:

- **`VirtualDocumentGenerator`** — Turns a `ParseResult` → virtual C# string + `SourceMap`
  that maps every UITKX source range to its generated C# position.
- **`RoslynHost`** — Manages `AdhocWorkspace` + `CSharpCompilation` per open `.uitkx` file,
  updated on every `textDocument/didChange`. Discovers reference assemblies via
  `ReferenceAssemblyLocator` (Unity `Library/ScriptAssemblies` + Editor `Managed/` + BCL).
- **`ReferenceAssemblyLocator`** — Resolves Unity project assemblies + detects Unity version
  from `ProjectSettings/ProjectVersion.txt` (exposed as `DetectedVersion`).

Roslyn powers the following through SourceMap-projected spans:

| Feature | File | Status |
|---------|------|--------|
| Tier-3 semantic diagnostics (type errors, undefined symbols) | `DiagnosticsPublisher.cs` | ✅ |
| Version-compatibility diagnostics (UITKX0200) | `DiagnosticsPublisher.cs` | ✅ |
| C# completions in `@code { }` and `attr={expr}` | `CompletionHandler.cs` | ✅ |
| Hover with type info in C# regions | `HoverHandler.cs` | ✅ |
| Go to Definition (F12) across `.uitkx` ↔ `.cs` | `DefinitionHandler.cs` | ✅ |
| Rename Symbol (F2) across `.uitkx` ↔ `.cs` | `RenameHandler.cs` | ✅ |
| Find All References (Shift+F12) | `ReferencesHandler.cs` | ✅ |
| Typed prop checking via virtual document | `VirtualDocumentGenerator.cs` | ✅ |

### Item 2 — Coloring / Tokenization — IDE-agnostic ownership

The semantic token types are custom (`uitkxElement`, `uitkxDirective`, etc.) and are mapped
to theme colours via `semanticTokenScopes` in `package.json`.  This mapping is VSCode-specific.

Remaining gaps:
- C# inside `.uitkx` is coloured by Roslyn classification for known tokens, but falls back
  to TmLanguage regex heuristics for some edge cases.
- Rider and VS2022 rely solely on LSP semantic tokens — the `semanticTokenScopes` mapping
  is VS Code specific, so these IDEs may show slightly different colours.

### ~~Item 3 — Formatting~~ ✅ MOSTLY DONE

`AstFormatter.cs` is AST-based and handles the full UITKX markup structure including:
- Self-closing vs explicit close tag preservation (`CloseTagLine` check)
- Empty element same-line formatting (`<Box></Box>` stays on one line)
- Attribute wrapping (print-width, single-attribute-per-line)
- C# setup code formatting via custom line-based re-indentation

Remaining gap:
- C# inside `@code { }` blocks uses indent adjustment only, not Roslyn's formatter.

### ~~Item 4 — IntelliSense (Completions / Hover)~~ ✅ DONE

`CompletionHandler.cs` + `AstCursorContext.cs` provide:
- ✅ Directive completions in the header region
- ✅ Control-flow directive completions (`@if`, `@foreach`, etc.)
- ✅ Tag-name completions (from `WorkspaceIndex` + schema, with version annotations)
- ✅ Attribute-name completions (from workspace props + schema, version-filtered)
- ✅ Attribute-value completions (boolean, enum hints from schema, version-aware)
- ✅ Style key/value completions with version badges
- ✅ C# completions inside `@code { }` blocks via Roslyn `CompletionService`
- ✅ C# completions inside `attr={expr}` via Roslyn `CompletionService`
- ✅ Hover with type information in C# regions via Roslyn `SymbolInfo`

---

## 3. The Implementation (Completed)

The dependency graph described in the original design was implemented in full.
The Roslyn host is the engine; Items 2, 3, and 4 consume its output.

```
  language-lib: Parser layer
  ├── DirectiveParser + UitkxParser + ExpressionExtractor
  └── → ParseResult (AST, DirectiveSet, CodeBlockNode, ExpressionNode)
         │
         ▼
  VirtualDocumentGenerator.Generate(ParseResult) → virtual C# + SourceMap
         │
         ▼
  RoslynHost (AdhocWorkspace per file, CSharpCompilation, MetadataReferences)
  ├── Compilation.GetDiagnostics     → DiagnosticsPublisher (Tier-3)   ✅
  ├── ClassifiedSpans                → SemanticTokensProvider          ✅
  ├── CompletionService              → CompletionHandler (C# regions)  ✅
  ├── SemanticModel.GetSymbolInfo    → HoverHandler                    ✅
  ├── SemanticModel.GetSymbolInfo    → DefinitionHandler               ✅
  ├── SymbolFinder.FindReferences    → ReferencesHandler               ✅
  └── Renamer.RenameSymbolAsync      → RenameHandler                   ✅
```

---

## 4. Remaining Opportunities

With Item 1 (Roslyn host) complete, the remaining polish areas are:

- **C# formatting in `@code` blocks** — Roslyn's formatter could replace the current
  indent-only pass-through. Low priority since the custom indenter works well.
- **Semantic token IDE portability** — VS2022 and Rider rely on standard LSP token types.
  Custom `uitkxElement` etc. could be mapped to standard types with modifiers for
  better cross-IDE colour consistency.
- **TmLanguage cleanup** — The grammar could be simplified to a minimal pre-connection
  fallback now that semantic tokens are authoritative.

---

## 5. Key Data Flows (Quick Reference)

### textDocument/didChange → Roslyn update
```
TextSyncHandler.DidChange
  → DocumentStore.Set(uri, text)
  → DiagnosticsPublisher.Publish(uri, text)       [Tier 1+2 — today]
  → RoslynHost.UpdateAsync(uri, text)             [Tier 3 — new]
      → VirtualDocumentGenerator.Generate(parseResult)
      → workspace.TryApplyChanges(document.WithText(...))
```

### textDocument/semanticTokens/full → coloring
```
SemanticTokensHandler.Tokenize
  → SemanticTokensProvider.GetTokens(parseResult, source, knownElements)
      UITKX ranges:  AST walk (existing)
      C# ranges:     RoslynHost.GetClassifiedSpansAsync(uri, csharpRegions)
                       → SourceMap.ToUitkxSpan(classifiedSpan)
                       → emit standard LSP token type
```

### textDocument/completion → completions
```
CompletionHandler.Handle
  → AstCursorContext.Find(parseResult, text, line, col)
  → if ctx.Kind is in C# region:
      → RoslynHost.GetCompletionsAsync(uri, virtualPosition)
          → map back symbols → LSP CompletionItem list
  → else:
      existing UITKX completion logic (unchanged)
```

### textDocument/formatting → formatted document
```
FormattingHandler.Handle
  → AstFormatter.Format(source, filePath, formatterOptions)
      markup + directive regions: AST formatter (existing)
      @code regions:
          → RoslynHost.FormatAsync(uri, codeBlockRange)
              → replace raw @code body with Roslyn-formatted text
```

---

## 6. IDE Portability (The Item 2 Promise)

Semantic tokens are an **LSP-standard protocol feature** (LSP 3.16+).  Every IDE that supports
LSP can consume them:

| IDE | Semantic tokens support | TmLanguage support |
|-----|------------------------|--------------------|
| VS Code | ✅ Full (`semanticTokenScopes` mapping in `package.json`) | ✅ |
| Rider | ✅ Via LSP plugin | ❌ |
| VS 2022 | ✅ Via LSP client | ❌ (has its own TextMate-like system) |
| Neovim + lspconfig | ✅ | Via plugin only |

Once semantic tokens are the sole authoritative source (not fighting with TmLanguage) **the same
token stream produces correct colouring in every IDE** without writing separate syntax
highlighting adapters.  The `semanticTokenTypes` declared in `package.json` use `superType`
inheritance from standard LSP types so IDEs that do not know custom type names fall back to the
standard colour gracefully.

---

## 7. File Map (Current)

```
ide-extensions~/
├── grammar/
│   └── uitkx.tmLanguage.json            Layer 1 — TmLanguage (static, fallback)
│
├── language-lib/                         Layer 2+3 logic (netstandard2.0)
│   ├── Parser/
│   │   ├── DirectiveParser.cs           Parses @directive header + function-style
│   │   ├── UitkxParser.cs               Recursive-descent markup parser
│   │   ├── ExpressionExtractor.cs       Brace/paren-balanced C# extractor
│   │   └── MarkupTokenizer.cs           Character scanner
│   ├── Nodes/
│   │   └── AstNode.cs                   AST record types (incl. CodeBlockNode, ExpressionNode)
│   ├── SemanticTokens/
│   │   ├── SemanticTokensProvider.cs    AST walker → SemanticTokenData[]
│   │   ├── SemanticTokenTypes.cs        Token type string constants + All[] legend
│   │   └── SemanticTokenData.cs
│   ├── IntelliSense/
│   │   └── AstCursorContext.cs          Cursor context classifier → CursorKind
│   ├── Formatter/
│   │   ├── AstFormatter.cs              AST-based formatter
│   │   ├── FormatterOptions.cs
│   │   └── ConfigLoader.cs
│   ├── Diagnostics/
│   │   ├── DiagnosticsAnalyzer.cs       Tier-2 structural checks (UITKX0101–0106)
│   │   └── DiagnosticCodes.cs
│   └── Lowering/
│       └── CanonicalLowering.cs
│
└── lsp-server/                           LSP server process (net8.0)
    ├── Program.cs                        DI / LanguageServer.From(...)
    ├── DocumentStore.cs                  Thread-safe text cache
    ├── WorkspaceIndex.cs                 *Props.cs scanner → element + prop info
    ├── TextSyncHandler.cs                didOpen / didChange → DocumentStore + Diagnostics
    ├── SemanticTokensHandler.cs          LSP bridge → SemanticTokensProvider
    ├── CompletionHandler.cs              LSP bridge → AstCursorContext + schema
    ├── HoverHandler.cs
    ├── FormattingHandler.cs              LSP bridge → AstFormatter
    ├── DiagnosticsPublisher.cs           Tier 1+2 → publishDiagnostics
    ├── DefinitionHandler.cs
    └── WatchedFilesHandler.cs
```

**Planned additions** (see `EMBEDDED_ROSLYN_IMPLEMENTATION_PLAN.md`):

```
ide-extensions~/
└── language-lib/
    └── Roslyn/                           NEW — no Roslyn dep (just data types + generator)
        ├── VirtualDocumentGenerator.cs  ParseResult → VirtualDocument + SourceMap
        ├── SourceMap.cs                  span translation uitkx ↔ virtualDoc
        ├── CSharpRegion.cs               enum CSharpRegionKind + region data
        └── VirtualDocument.cs            the generated C# + SourceMap + metadata

    lsp-server/
    └── Roslyn/                           NEW — net8.0 only (Roslyn dep here)
        ├── RoslynHost.cs                 AdhocWorkspace manager (one doc per open .uitkx)
        ├── ReferenceAssemblyLocator.cs   Discovers Unity + BCL + project assemblies
        └── RoslynDiagnosticMapper.cs     Roslyn Diagnostic → ParseDiagnostic (Tier 3)
```

---

*Last updated:* 2026-03-09 — initial creation.
