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

## 2. The Four Open Feature Areas

### Item 1 — Embedded Roslyn (Tier-3 Diagnostics + C# Semantic Services)

**P9 in `UITKX_IMPLEMENTATION_PLAN.md`.**

C# code inside `.uitkx` files has no Roslyn backing today.  The LSP server has no knowledge
of types, symbols, or compilation errors inside `@code { }` or `@(expr)` regions.

- `@code { }` blocks: class-level members — type-checked by Roslyn when it compiles the
  generated `.g.cs` file, but never inspected by the LSP server.
- `@(expr)` / `attr={expr}` blocks: expression-level C# — no type checking, no completions.
- Tier-1 parse errors: handled (parser syntax).
- Tier-2 structural errors (UITKX0101–0106): handled (`DiagnosticsAnalyzer`).
- **Tier-3 semantic errors** (type mismatches, undefined symbols, nullability violations): **not handled**.

### Item 2 — Coloring / Tokenization — IDE-agnostic ownership

The semantic token types are custom (`uitkxElement`, `uitkxDirective`, etc.) and are mapped
to theme colours via `semanticTokenScopes` in `package.json`.  This mapping is VSCode-specific.

Problems today:
- The mapping relies on the IDE's theme; there is no enforced UITKX colour palette.
- C# inside `.uitkx` is coloured by TmLanguage regex heuristics — wrong on edge cases.
- The same ranges receive conflicting colours from TmLanguage and LSP depending on timing.
- Rider and VS2022 have no TmLanguage at all — the LSP semantic tokens are their only
  colouring source, and the `semanticTokenScopes` mapping does not apply there.

**Making tokenization IDE-agnostic** means: semantic tokens are the truth, and each token type
maps to a standard LSP type/modifier pair that every IDE already knows how to colour correctly.

### Item 3 — Formatting

`AstFormatter.cs` is AST-based and correct for the UITKX markup structure.

Gaps:
- C# inside `@code { }` blocks is passed through verbatim with only indent adjustment.
  Roslyn's formatter would apply full C# formatting rules.
- Attribute expression values `attr={...}` are not formatted at all.
- Edge cases in brace alignment within control-flow bodies have reported exceptions.

### Item 4 — IntelliSense (Completions / Hover)

`CompletionHandler.cs` + `AstCursorContext.cs` provide:
- ✅ Directive completions in the header region
- ✅ Control-flow directive completions (`@if`, `@foreach`, etc.)
- ✅ Tag-name completions (from `WorkspaceIndex` KnownElements)
- ✅ Attribute-name completions (from `WorkspaceIndex.GetProps`)
- ✅ Attribute-value completions (boolean, enum hints from schema)
- ❌ C# completions inside `@code { }` blocks — short-circuits to empty list
- ❌ C# completions inside `@(expr)` / `attr={expr}` — same empty list
- ❌ Hover type information inside C# regions

---

## 3. The Dependency Graph

```
                ┌──────────────────────────────────────────────────────┐
                │   language-lib: Parser layer                          │
                │   DirectiveParser + UitkxParser + ExpressionExtractor │
                │   → ParseResult (AST, DirectiveSet, CodeBlockNode,    │
                │     ExpressionNode, AttributeNode.CSharpExpressionValue)│
                │   Knows: exact byte ranges of every C# region        │
                └────────────────────────┬─────────────────────────────┘
                                         │   (always available)
              ┌──────────────────────────▼─────────────────────────────────────┐
              │  Item 2 — Complete Semantic Token Coverage (surface contract)   │
              │  SemanticTokensProvider as SOLE coloring authority              │
              │                                                                 │
              │  UITKX regions (tags, attrs, directives, control-flow):         │
              │    → Already correct via AST walk                               │
              │  C# regions (@code body, @(expr), attr={expr}):                 │
              │    → NEEDS Roslyn classification (today: regex heuristics)      │
              └────────────┬───────────────────────────┬────────────────────────┘
                           │                           │
              ┌────────────▼────────────┐   ┌──────────▼─────────────────────────┐
              │ Item 3 — Formatting     │   │ Item 4 — IntelliSense               │
              │ UITKX structure: ✅     │   │ UITKX structure: ✅                 │
              │ @code C# blocks: ❌     │   │ @code / @(expr) completions: ❌     │
              │   (pass-through only)   │   │ Hover inside C# regions: ❌         │
              │                         │   │                                     │
              │ Fix: feed Roslyn-        │   │ Fix: translate cursor → virtual doc │
              │ formatted @code back     │   │ position → ask CompletionService    │
              │ into AstFormatter output │   │                                     │
              └────────────┬────────────┘   └──────────┬──────────────────────────┘
                           │                            │
                           └──────────┬─────────────────┘
                                      │ both need C# answers
            ┌─────────────────────────▼─────────────────────────────────────────┐
            │  Item 1 — Embedded Roslyn Host                                    │
            │                                                                   │
            │  Inputs:                                                          │
            │    • VirtualDocumentGenerator.Generate(ParseResult) →            │
            │        generated C# string + SourceMap                           │
            │    • Reference assemblies discovered from workspace               │
            │                                                                   │
            │  Roslyn Workspace (AdhocWorkspace, one per open .uitkx file):    │
            │    • CSharpCompilation with MetadataReferences                   │
            │    • Document updated on every textDocument/didChange            │
            │                                                                   │
            │  Outputs (all C# regions):                                       │
            │    • ClassifiedSpans → SemanticTokensProvider (feeds Item 2)     │
            │    • CompletionService.GetCompletionsAsync → CompletionHandler   │
            │      (feeds Item 4)                                               │
            │    • Formatter.FormatAsync → AstFormatter @code replacement      │
            │      (feeds Item 3)                                               │
            │    • Compilation.GetDiagnostics → DiagnosticsPublisher Tier 3    │
            │      (this IS Item 1 / P9)                                        │
            │    • SemanticModel.GetSymbolInfo → HoverHandler (feeds Item 4)   │
            └───────────────────────────────────────────────────────────────────┘
```

---

## 4. The Build Order (Why Item 1 First)

### Items 2, 3, and 4 each have a working UITKX half

All three already work correctly for UITKX-structure positions.  The gap in each is identical:
*what do you do when the cursor / span is inside a C# region?*  Today all three answer "nothing
useful."  The single infrastructure piece that answers all three at once is the Roslyn host.

### Why *not* start with Items 2, 3, or 4

- **Item 2 alone**: you can clean up the TmLanguage conflict and assert semantic-tokens-only
  ownership for UITKX ranges without Roslyn.  C# regions remain heuristic.  This is a useful
  cleanup but it does not unlock Items 3 or 4.
- **Item 3 alone**: you can fix AstFormatter edge cases without Roslyn.  C# formatting remains
  pass-through.  This is a useful fix but low leverage.
- **Item 4 alone**: you cannot meaningfully improve C# completions without Roslyn.  Any
  completions you add for C# regions without semantic knowledge will be wrong.

### The correct order

```
Step 1 — VirtualDocument + SourceMap (no Roslyn yet)
          Build the generator that turns a ParseResult → virtual C# string.
          This is pure language-lib work with no external dependencies.
          Deliverable: VirtualDocumentGenerator, SourceMap, SourceMapEntry data types.

Step 2 — Roslyn Host (RoslynHost, ReferenceAssemblyLocator)
          Embed Roslyn in the LSP server.
          Wire: TextSyncHandler events → RoslynHost.UpdateAsync → Roslyn workspace.
          Deliverable: compilation + semantic model available for every open .uitkx file.

Step 3 — Tier-3 Diagnostics
          Wire RoslynHost → DiagnosticsPublisher as new Tier 3.
          Project Roslyn diagnostic spans through SourceMap back to .uitkx positions.
          Deliverable: P9 complete — UITKX0200+ error codes for semantic errors.

Step 4 — Semantic tokens (C# regions)
          Wire RoslynHost.ClassifyAsync → SemanticTokensProvider.
          SemanticTokensProvider becomes fully authoritative (no more regex heuristics).
          Simultaneously: clean up TmLanguage conflict (Item 2 fully resolved).
          Deliverable: accurate IDE-agnostic colouring for the entire file.

Step 5 — IntelliSense (C# regions)
          Wire RoslynHost.GetCompletionsAsync → CompletionHandler for code positions.
          Wire RoslynHost.GetSymbolInfoAsync → HoverHandler.
          Deliverable: full IntelliSense inside @code and @(expr) (Item 4 fully resolved).

Step 6 — Formatting (C# regions)
          Wire RoslynHost.FormatAsync → AstFormatter for @code blocks.
          Deliverable: AstFormatter applies real C# formatting to @code content (Item 3 fully resolved).
```

### Bottom line

**Item 1 (Roslyn host) is the engine.**  Items 2, 3, and 4 are the surfaces that consume its
output.  Building the surface first (Step 1: VirtualDocument + SourceMap) is a pure refactor
with no external dependency.  After that, Item 1 (Steps 2–3) is the single blocker that, once
removed, lets Items 2, 3, and 4 be completed in rapid succession.

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
