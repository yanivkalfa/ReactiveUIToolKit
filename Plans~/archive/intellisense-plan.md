# IntelliSense Production Plan — ReactiveUIToolKit / UITKX

**Status:** ✅ COMPLETED — archived 2026-03-19 (T-01 through T-08 shipped, T-09 N/A, T-10 moved to TECH_DEBT.md as TD-13)  
**Target IDEs:** VS Code (primary), Visual Studio 2022 (primary), JetBrains Rider (bonus)  
**Author:** Copilot (Claude Sonnet 4.6)  
**Created:** 2025  
**Mandate:** No shortcuts. No temporary workarounds. Production-grade solutions only.

---

## 1 · Context for the Next Engineer

This plan was written after a deep investigation of the UITKX language server's IntelliSense subsystem.  
The system already has **virtual document generation with bidirectional source maps** — that foundation is solid.  
The bugs are entirely in **how that foundation is used** to deliver completions.

### Architecture in one paragraph

When the user opens a `.uitkx` file the LSP server (a .NET 8 process, `UitkxLanguageServer`) is started:
- **VS Code:** TypeScript extension (`ide-extensions~/vscode/`) spawns the server via `stdio`.
- **VS2022:** MEF extension (`ide-extensions~/visual-studio/UitkxVsix/`) spawns the server via `stdio`; completions are delivered by `UitkxCompletionSource` which calls `textDocument/completion` directly over `JsonRpc`.
- **Rider:** Gradle plugin (`ide-extensions~/rider/`) — stub currently, same LSP server.

The server contains:
- `language-lib/` — parser, AST, virtual-document generator, source map (language-agnostic library)
- `lsp-server/` — LSP handlers, Roslyn host (one `AdhocWorkspace` per open `.uitkx` file)

```
.uitkx source
  │
  ├─ UitkxParser          → AST (nodes, attributes, expressions)
  ├─ VirtualDocumentGenerator → C# source file text + SourceMap (bidirectional offset table)
  └─ RoslynHost           → AdhocWorkspace + Project + Document (one per file)
                              ↕  CompletionService (CURRENTLY: Recommender — WRONG)
                           CompletionHandler (LSP handler, decides routing)
```

### What works today

- `from?.Path` member-access completion (is inside `FunctionSetup` region; `Recommender` accidentally produces scope symbols)
- Tag completions, attribute name completions (schema-based, not Roslyn)
- Go-to-definition, hover for UITKX symbols

### What is broken today

| Scenario | Problem |
|---|---|
| `StyleKeys.` inside `style={...}` | `AttributeValue` not in routing gate — falls through to schema items |
| `allowNextRef.` at wrong position | Leaky `CSharpCodeBlock` heuristic fires on markup lines |
| Pressing `.` anywhere | `ToVirtualOffset()` returns null → `s_keywordItems` popup instead of empty |
| Any `attr={expr}` position | `CursorKind.AttributeValue` not gated to Roslyn |
| Method call `(` → parameter hints | No `SignatureHelpHandler` registered |

---

## 2 · Root Cause Summary (CONFIRMED)

Five root causes have been confirmed through direct source investigation:

### RC-1 · Wrong Roslyn API (`Recommender` instead of `CompletionService`)

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs`  
**Lines:** ~106–120

`Recommender.GetRecommendedSymbolsAtPositionAsync` is a **scope dump** — it enumerates every symbol accessible at that position. It does NOT understand:
- Member access (`.` trigger) — returns entire scope, not members of the LHS
- Object initializer context
- Method argument context
- Override/implement suggestions
- Keywords in context (it returns no keywords at all)

`CompletionService.GetCompletionsAsync` is the **correct API** — it is the same engine VS Code's own C# extension and Visual Studio use. It is context-aware and returns only relevant items.

`CompletionService` is available in the already-referenced NuGet `Microsoft.CodeAnalysis.Workspaces.Common` 4.9.2. **No new dependency is needed.**

### RC-2 · `AdhocWorkspace` blocks `CompletionService`

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`  
**Line:** 381

```csharp
var ws = new AdhocWorkspace();   // ← WRONG
```

`CompletionService.GetService(document)` internally calls `document.Project.Language` and then looks up the `ICompletionService` from the workspace's `HostServices`. When `AdhocWorkspace()` is used without a host, no MEF services are loaded, and `GetService()` returns `null`.

Fix:
```csharp
var ws = new AdhocWorkspace(MefHostServices.DefaultHost);   // ← CORRECT
```

`MefHostServices.DefaultHost` is in `Microsoft.CodeAnalysis.Workspaces.Common` — already referenced.

### RC-3 · `AttributeValue` not in routing gate

**File:** `ide-extensions~/lsp-server/CompletionHandler.cs`  
**Lines:** ~138–149

`wantsRoslynCompletion` checks:
```csharp
ctx.Kind == CursorKind.CSharpExpression
|| ctx.Kind == CursorKind.CSharpCodeBlock
|| (inCodeBlockLine && ...)
```

`CursorKind.AttributeValue` is **never** in this gate. So when the cursor is inside `onClick={...}`, `style={...}`, `value={...}`, or any other attribute expression, the routing falls through to schema completions, which are meaningless for a C# expression.

The source map **already contains** `AttributeExpression` region entries for all `attr={expr}` spans (emitted by `VirtualDocumentGenerator`). The completion infrastructure just never asks Roslyn for those positions.

### RC-4 · Null-map fallback returns keywords instead of empty

**File:** `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs`  
**Lines:** ~90, ~97, ~104

When `ToVirtualOffset()` returns null (cursor not in any mapped C# region), the provider returns `s_keywordItems` — a list of 50 C# keywords. This causes a keyword popup anywhere the user types `.`, because `.` is a trigger character.

The correct behavior is to return `Array.Empty<CompletionItem>()` and let the `CompletionHandler` fall through to schema items or return empty.

### RC-5 · Leaky `CSharpCodeBlock` heuristic

**File:** `ide-extensions~/language-lib/IntelliSense/AstCursorContext.cs`  
**Added in v1.0.138**

The heuristic fires when:
```csharp
IsFunctionStyle && line1 >= FunctionSetupStartLine && astTagName == null
```

There is no `FunctionSetupEndLine`. So the heuristic also fires on:
- The `return (` line
- Lines inside the JSX `return (...)` block where `astTagName` happens to be null
- The closing `}` of the component
- Blank lines after the setup code

This causes spurious Roslyn calls for markup positions, and those return garbage (source map returns null for markup → RC-4 triggers → keyword popup).

---

## 3 · File Inventory

### Files that will be modified

| File | Role | Tasks |
|---|---|---|
| `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` | Workspace creation | T-01 |
| `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs` | Roslyn API call | T-02, T-03 |
| `ide-extensions~/lsp-server/CompletionHandler.cs` | LSP completion routing | T-04, T-05 |
| `ide-extensions~/language-lib/IntelliSense/AstCursorContext.cs` | Cursor position classification | T-06 |
| `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs` | Virtual doc & source map | Reference only (no changes expected) |
| `ide-extensions~/language-lib/Roslyn/SourceMap.cs` | Position mapping | Reference only (no changes expected) |
| `ide-extensions~/lsp-server/Program.cs` | Handler registration | T-07 (SignatureHelp) |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxCompletionSource.cs` | VS2022 LSP call | T-08 |

### Key supporting files (read-only reference)

| File | Why it matters |
|---|---|
| `ide-extensions~/lsp-server/UitkxLanguageServer.csproj` | NuGet deps — `Workspaces.Common` 4.9.2 already present |
| `ide-extensions~/language-lib/Parser/DirectivesInfo.cs` | `FunctionSetupStartLine` and other directive metadata |
| `ide-extensions~/language-lib/Nodes/*.cs` | AST node types |

---

## 4 · Task List

Each task is self-contained and safe to implement independently unless a dependency is noted.  
**Recommended implementation order:** T-01 → T-02 → T-03 → T-04 → T-05 → T-06 → T-07 → T-08 → T-09 → T-10

---

### T-01 · Switch `AdhocWorkspace` to use `MefHostServices` 
**Status:** NOT STARTED  
**Priority:** CRITICAL — prerequisite for all other Roslyn tasks  
**Effort:** ~5 minutes, 1 line change  
**Risk:** Low — `MefHostServices.DefaultHost` is lazy-initialized and thread-safe  

**Problem:**  
`CompletionService.GetService(document)` returns `null` when the workspace has no MEF host, because the C# completion providers are MEF-exported services. `new AdhocWorkspace()` uses a minimal no-MEF host.

**Fix:**

File: `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`

Find (around line 381 inside `UpdateWorkspace()`):
```csharp
var ws = new AdhocWorkspace();
```

Replace with:
```csharp
var ws = new AdhocWorkspace(MefHostServices.DefaultHost);
```

Add using at top of file if not already present:
```csharp
using Microsoft.CodeAnalysis.Host.Mef;
```

**Verification:**  
After T-02 is done, add a debug log: `ServerLog.Log($"CompletionService: {CompletionService.GetService(doc) != null}")`. Should log `true`.

**Notes:**  
`MefHostServices.DefaultHost` scans for all Roslyn MEF compositions at first access. This is a one-time ~50ms startup cost (happens on first workspace creation, not on every completion). It loads both `CSharpCompletionService` and `VisualBasicCompletionService` — no configuration needed.

---

### T-02 · Replace `Recommender` with `CompletionService`
**Status:** NOT STARTED  
**Priority:** CRITICAL — core fix  
**Depends on:** T-01 (MefHostServices must be active)  
**Effort:** ~30–60 minutes  
**Risk:** Medium — requires careful mapping of `CompletionItem` fields  

**Problem:**  
`Recommender.GetRecommendedSymbolsAtPositionAsync` is a scope dump; it does not produce context-aware completions. It does not understand member access, overloads, or keywords-in-context.

**Fix:**

File: `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs`

**Step 1** — Add usings:
```csharp
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Tags;
// Remove:  using Microsoft.CodeAnalysis.Recommendations;
```

**Step 2** — Determine the `CompletionTrigger` from the caller. `GetCompletionsAsync` should accept a `char?` trigger character and pass it down:

```csharp
public async Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
    string            uitkxFilePath,
    string            uitkxSource,
    ParseResult       parseResult,
    int               uitkxOffset,
    char?             triggerChar = null,      // ← ADD THIS PARAMETER
    CancellationToken ct = default)
```

**Step 3** — Replace the `Recommender` block with:

```csharp
// Build CompletionTrigger from the trigger character.
var trigger = triggerChar.HasValue
    ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
    : CompletionTrigger.Invoke;

// Get the CompletionService — requires MefHostServices.DefaultHost on the workspace.
var completionService = CompletionService.GetService(roslynDoc);
if (completionService == null)
{
    ServerLog.Log("[RoslynCompletion] CompletionService is null — workspace host missing MEF?");
    return Array.Empty<CompletionItem>();
}

// Ask Roslyn for completions at the virtual offset.
var completionList = await completionService
    .GetCompletionsAsync(roslynDoc, virtualOffset, trigger, cancellationToken: ct)
    .ConfigureAwait(false);

if (completionList == null || completionList.ItemsList.Count == 0)
    return Array.Empty<CompletionItem>();

// Map Roslyn CompletionItem → LSP CompletionItem.
var items = new List<CompletionItem>(completionList.ItemsList.Count);
foreach (var rItem in completionList.ItemsList)
{
    ct.ThrowIfCancellationRequested();
    items.Add(RoslynItemToLspItem(rItem));
}

ServerLog.Log($"[RoslynCompletion] {items.Count} items at virtual offset {virtualOffset}");
return items;
```

**Step 4** — Replace `SymbolToLspItem` with `RoslynItemToLspItem`:

```csharp
private static CompletionItem RoslynItemToLspItem(
    Microsoft.CodeAnalysis.Completion.CompletionItem rItem)
{
    return new CompletionItem
    {
        Label      = rItem.DisplayText,
        Kind       = TagsToCompletionKind(rItem.Tags),
        Detail     = string.IsNullOrEmpty(rItem.InlineDescription)
                         ? rItem.DisplayTextSuffix   // e.g. "()" for methods
                         : rItem.InlineDescription,
        InsertText = rItem.Properties.TryGetValue("InsertionText", out var ins) && !string.IsNullOrEmpty(ins)
                         ? ins
                         : rItem.DisplayText,
        SortText   = rItem.SortText,
        FilterText = rItem.FilterText,
    };
}

private static CompletionItemKind TagsToCompletionKind(
    System.Collections.Immutable.ImmutableArray<string> tags)
{
    // Roslyn uses string tags from WellKnownTags / Microsoft.CodeAnalysis.Tags.
    // The most useful ones are checked first.
    foreach (var tag in tags)
    {
        switch (tag)
        {
            case WellKnownTags.Method:       return CompletionItemKind.Method;
            case WellKnownTags.ExtensionMethod: return CompletionItemKind.Method;
            case WellKnownTags.Property:     return CompletionItemKind.Property;
            case WellKnownTags.Field:        return CompletionItemKind.Field;
            case WellKnownTags.Event:        return CompletionItemKind.Event;
            case WellKnownTags.Class:        return CompletionItemKind.Class;
            case WellKnownTags.Structure:    return CompletionItemKind.Struct;
            case WellKnownTags.Interface:    return CompletionItemKind.Interface;
            case WellKnownTags.Enum:         return CompletionItemKind.Enum;
            case WellKnownTags.EnumMember:   return CompletionItemKind.EnumMember;
            case WellKnownTags.Delegate:     return CompletionItemKind.Class;
            case WellKnownTags.Namespace:    return CompletionItemKind.Module;
            case WellKnownTags.Local:        return CompletionItemKind.Variable;
            case WellKnownTags.Parameter:    return CompletionItemKind.Variable;
            case WellKnownTags.Keyword:      return CompletionItemKind.Keyword;
            case WellKnownTags.Snippet:      return CompletionItemKind.Snippet;
            case WellKnownTags.TypeParameter: return CompletionItemKind.TypeParameter;
            case WellKnownTags.Constant:     return CompletionItemKind.Constant;
        }
    }
    return CompletionItemKind.Text;
}
```

**Step 5** — Remove `s_csharpKeywords`, `s_keywordItems`, `BuildKeywordItems()`, `SymbolToLspItem()`, `SymbolKindToCompletionKind()` — they are no longer needed. `CompletionService` returns keywords in context already.

**Step 6** — Remove the `#pragma warning CS0618` block.

**Step 7** — Update `CompletionHandler.cs` to pass the trigger char:
```csharp
var roslynList = await _roslynCompletion
    .GetCompletionsAsync(localPath, text, parseResult, offset,
        triggerChar?[0],    // ← pass the trigger char
        cancellationToken)
    .ConfigureAwait(false);
```

**Important API note on `CompletionList.ItemsList`:**  
In Roslyn 4.x, `CompletionList` exposes `ItemsList` (an `IReadOnlyList<CompletionItem>`) as the primary collection. The older `Items` property (ImmutableArray) still exists but `ItemsList` is preferred. If building against 4.9.2 and `ItemsList` does not compile, use `Items` instead (they are the same data).

**Important API note on `CompletionTrigger`:**  
`CompletionTrigger.CreateInsertionTrigger(char)` is the correct factory for character-triggered completion.  
`CompletionTrigger.Invoke` is for explicit invocation (Ctrl+Space).  
Do NOT construct `CompletionTrigger` with `new` — use the static factories.

**Verification:**  
- Type `SomeObject.` → should see members, not keywords  
- Type `new ` → should see type names  
- Press Ctrl+Space in setup code → should see scope symbols + keywords  

---

### T-03 · Fix null-map fallback → return empty not keywords
**Status:** NOT STARTED  
**Priority:** HIGH — needed to stop keyword popups at non-C# positions  
**Depends on:** None (independent)  
**Effort:** ~5 minutes  
**Risk:** Very low  

**Problem:**  
Three places in `RoslynCompletionProvider.GetCompletionsAsync` return `s_keywordItems` when the virtual document or source map lookup fails. This causes 50 keyword completions to appear at any position with a `.` trigger.

**Fix:**

File: `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs`

Replace all occurrences of:
```csharp
return s_keywordItems; // workspace not ready; fall back to keywords
```
and:
```csharp
return s_keywordItems; // cursor not in a C# region
```
and:
```csharp
return s_keywordItems;
```
(the ones that happen on null-workspace, null-map-result, and null-roslynDoc)

With:
```csharp
return Array.Empty<CompletionItem>();
```

**Exception** — the `catch` block at the end should also return empty:
```csharp
catch (Exception ex)
{
    ServerLog.Log($"[RoslynCompletion] Error: {ex.Message}");
    return Array.Empty<CompletionItem>();
}
```

**Rationale:**  
When workspace isn't ready, `CompletionHandler` already handles the `roslynList.Count == 0` case correctly: for `CSharpExpression`/`CSharpCodeBlock` kinds it returns `isIncomplete: true` (retry signal). For non-C# positions the caller should just fall to schema items or empty.

---

### T-04 · Add `AttributeValue` to Roslyn routing gate
**Status:** NOT STARTED  
**Priority:** HIGH — blocks all `attr={expr}` completions  
**Depends on:** T-02, T-03 (without these, routing to Roslyn returns keywords or garbage)  
**Effort:** ~5 minutes  
**Risk:** Low — source map will return null for non-C# attribute values  

**Problem:**  
`CursorKind.AttributeValue` is never sent to Roslyn. All `onClick={...}`, `style={...}`, `value={expr}`, etc. positions fall through to schema `AttributeValueItems()` which returns fixed enum strings — useless for a C# expression.

**Fix:**

File: `ide-extensions~/lsp-server/CompletionHandler.cs`

Find the `wantsRoslynCompletion` block:
```csharp
bool wantsRoslynCompletion =
    ctx.Kind == CursorKind.CSharpExpression
    || ctx.Kind == CursorKind.CSharpCodeBlock
    || (
        inCodeBlockLine
        && !inEmbeddedMarkupInCode
        && ctx.Kind != CursorKind.DirectiveName
        && ctx.Kind != CursorKind.ControlFlowName
        && triggerChar != "<"
    );
```

Add `AttributeValue`:
```csharp
bool wantsRoslynCompletion =
    ctx.Kind == CursorKind.CSharpExpression
    || ctx.Kind == CursorKind.CSharpCodeBlock
    || ctx.Kind == CursorKind.AttributeValue      // ← ADD THIS
    || (
        inCodeBlockLine
        && !inEmbeddedMarkupInCode
        && ctx.Kind != CursorKind.DirectiveName
        && ctx.Kind != CursorKind.ControlFlowName
        && triggerChar != "<"
    );
```

**Why this is safe:**  
- For plain-string attribute values like `class="btn"`, the cursor's uitkx offset is NOT in any mapped C# region. `ToVirtualOffset()` returns null. T-03 makes `GetCompletionsAsync` return empty → `roslynList.Count == 0` → `CompletionHandler` falls through for `AttributeValue` kinds (they're not `CSharpExpression`/`CSharpCodeBlock` so no `isIncomplete` is returned) and the normal `AttributeValueItems()` is called. ✓
- For C# attribute values like `style={StyleKeys.}`, the offset IS in an `AttributeExpression` mapped region. `ToVirtualOffset()` returns a valid virtual offset with `FunctionSetup` or `AttributeExpression` kind. `CompletionService` returns member completions. ✓

**Note about the incomplete-list return:**  
After adding `AttributeValue` to the gate, the `isIncomplete: true` early-return block that currently only applies to `CSharpExpression` and `CSharpCodeBlock` should also be extended to `AttributeValue`:

```csharp
if (ctx.Kind == CursorKind.CSharpExpression
    || ctx.Kind == CursorKind.CSharpCodeBlock
    || ctx.Kind == CursorKind.AttributeValue)   // ← ADD
{
    Log($"completion: {ctx.Kind} — Roslyn not ready, returning incomplete");
    return new CompletionList(isIncomplete: true);
}
```

---

### T-05 · Fix leaky `CSharpCodeBlock` heuristic — use source map as authority
**Status:** NOT STARTED  
**Priority:** HIGH — prevents spurious popups and wrong completions  
**Effort:** ~2–4 hours (requires understanding both `AstCursorContext` and `CompletionHandler`)  
**Risk:** Medium — changes cursor classification logic  

**Problem:**  
The `CSharpCodeBlock` cursor kind is detected by `AstCursorContext.Find` using:
```csharp
IsFunctionStyle && line1 >= FunctionSetupStartLine && astTagName == null
```
Because there is no `FunctionSetupEndLine`, this heuristic fires on the `return (` line, on lines inside the JSX markup block where the AST happens to not have a current tag, and on the closing `}` of the component body.

**Recommended solution — Source Map as authority:**  
The `SourceMap` is the ground truth for "is this cursor position inside C# code?". Its `ToVirtualOffset()` returns non-null only when the cursor is inside a mapped C# region (`FunctionSetup`, `AttributeExpression`, `InlineExpression`, or `CodeBlock`). For all markup lines, it returns null.

Rather than improving the line-heuristic in `AstCursorContext`, a cleaner design is to **make `CompletionHandler` derive `wantsRoslynCompletion` from the source map directly** for the code-block case, bypassing `AstCursorContext`'s heuristic entirely.

**Implementation approach:**

**Option A (Recommended): Source-map pre-check in `CompletionHandler`**

Add a helper that checks the source map before the `wantsRoslynCompletion` evaluation:

```csharp
// In CompletionHandler, just before the wantsRoslynCompletion computation:
bool offsetIsInCSharpRegion = false;
if (!string.IsNullOrEmpty(localPath))
{
    var virtualDoc = _roslynHost.GetVirtualDocument(localPath);  // ← needs RoslynHost injected
    if (virtualDoc != null)
        offsetIsInCSharpRegion = virtualDoc.Map.ToVirtualOffset(offset).HasValue;
}
```

Then update `wantsRoslynCompletion`:
```csharp
bool wantsRoslynCompletion =
    ctx.Kind == CursorKind.CSharpExpression
    || ctx.Kind == CursorKind.AttributeValue
    || offsetIsInCSharpRegion;   // replaces both CSharpCodeBlock heuristic AND inCodeBlockLine
```

This removes the dependency on:
- `inCodeBlockLine` (line-scan heuristic)
- `inEmbeddedMarkupInCode` 
- `CSharpCodeBlock` cursor kind for routing decisions (though `CSharpCodeBlock` can remain in the enum for other consumers like the diagnostics handler)

**Option B: Fix `AstCursorContext` heuristic by adding `FunctionSetupEndLine`**

If Option A is too big a change:

1. Find `FunctionSetupEndLine` — scan the `.uitkx` text from `FunctionSetupStartLine` forward for a line matching `^\s*return\s*[({]`. The line number just before that is the last setup line.
2. Store it in `DirectivesInfo` as `FunctionBodyReturnLine`.
3. Update `AstCursorContext.Find` to gate `CSharpCodeBlock` on `line1 < FunctionBodyReturnLine`.

The downside: this requires modifying `DirectivesInfo`, `DirectiveParser`, and `AstCursorContext` — more surface area.

**Note on `CompletionHandler` needing `RoslynHost` for Option A:**  
`CompletionHandler` already receives `RoslynHost` in its constructor — it's passed to `RoslynCompletionProvider`. Inject it as a direct field too:
```csharp
private readonly RoslynHost _roslynHost;  // add
```

**Verification:**  
- Cursor on `return (` line → no completion popup
- Cursor on markup line inside `return (...)` block → no C# popup (only tag completions if inside `<`)
- Cursor on setup code line → correct C# completions

---

### T-06 · Implement `SignatureHelpHandler` (parameter hints)
**Status:** NOT STARTED  
**Priority:** MEDIUM — quality-of-life feature  
**Depends on:** T-01 (MefHostServices needed)  
**Effort:** ~2–4 hours  
**Risk:** Low (additive, no changes to existing handlers)  

**Problem:**  
There is no `SignatureHelpHandler` registered in `Program.cs`. When the user types `(` or `,` inside a method call in a `.uitkx` expression (e.g., `GetRoute(`, `NavigateTo(route, `), VS Code shows no parameter hints.

**Implementation:**

**Step 1** — The API to use is `Microsoft.CodeAnalysis.SignatureHelp.ISignatureHelpProvider` (not the public signature). The correct public API since Roslyn 4.x is via the workspace service:

```csharp
// Trigger SignatureHelp via document method:
var signatureHelpService = document.Project.Solution.Workspace.Services
    .GetService<ISignatureHelpService>();
```

However, `ISignatureHelpService` is an internal Roslyn API. **The supported approach is to use `SignatureHelpItems`** via a reflection path, or — more robustly — use the existing `SemanticModel` to find method overloads manually.

**Recommended implementation (robust, no internal Roslyn APIs):**

Create `ide-extensions~/lsp-server/SignatureHelpHandler.cs`:

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

// Handler for textDocument/signatureHelp
public sealed class SignatureHelpHandler : ISignatureHelpHandler
{
    private readonly RoslynHost _host;

    // Register for trigger on '(' and ','
    public SignatureHelpRegistrationOptions GetRegistrationOptions(...) =>
        new() { TriggerCharacters = new Container<string>("(", ","), ... };

    public async Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken ct)
    {
        // 1. Map LSP position → uitkx offset
        // 2. Map uitkx offset → virtual offset (ToVirtualOffset)
        // 3. Get SemanticModel from RoslynDocument
        // 4. Find InvocationExpressionSyntax at that offset
        // 5. Resolve the method group symbol
        // 6. Return all overloads as SignatureInformation[]
        //    with the active parameter index from the argument list.
    }
}
```

**Step 2** — Algorithm for finding the active overload:
1. Get `SyntaxNode` at `virtualOffset` from the syntax tree.
2. Walk ancestors to find `InvocationExpressionSyntax` or `ObjectCreationExpressionSyntax`.
3. From `SemanticModel.GetSymbolInfo(invocation.Expression)`, get the `IMethodSymbol` candidates.
4. Count commas in `invocation.ArgumentList` before the cursor to determine active parameter index.
5. Build `SignatureInformation` for each overload: display string from `IMethodSymbol.ToDisplayString(FullyQualifiedFormat)`, `ParameterInformation` for each parameter.

**Step 3** — Register in `Program.cs`:
```csharp
.WithHandler<SignatureHelpHandler>()
```

**Verification:**  
Type `SomeMethod(` → parameter tooltips appear.  
Type `,` → active parameter advances.

---

### T-07 · Improve hover handler with Roslyn type info for attribute expressions
**Status:** NOT STARTED  
**Priority:** LOW — nice to have  
**Depends on:** T-01  
**Effort:** ~1–2 hours  
**Risk:** Low (additive)  

**Problem:**  
`HoverHandler.cs` exists but may not show type information for C# expressions inside `attr={expr}` attribute values since `AttributeValue` positions are not yet routed to Roslyn for hover.

**Fix:**  
Investigate `HoverHandler.cs`. Ensure it:
1. Maps cursor offset → virtual offset via `SourceMap`.
2. Calls `SemanticModel.GetSymbolInfo(node)` or `GetTypeInfo(node)` at that offset.
3. Returns the type display string.

If `HoverHandler.cs` already does source-map lookup for `CSharpExpression` positions, extend it to also look up `AttributeExpression` positions.

---

### T-08 · VS2022 — Verify trigger character passing in `UitkxCompletionSource`
**Status:** NOT STARTED  
**Priority:** MEDIUM — VS2022 completions may not use trigger kind correctly  
**Depends on:** T-02 (CompletionService must be in use)  
**Effort:** ~30 minutes (verify + possibly fix)  
**Risk:** Low  

**Problem:**  
`UitkxCompletionSource.cs` calls `textDocument/completion` with `context = new { triggerKind = 1 }` (Invoked), regardless of the actual trigger. This means the `triggerKind` and `triggerCharacter` are not passed, so `CompletionService` always uses `CompletionTrigger.Invoke` and may not produce member-access completions on `.`.

**Fix:**

File: `ide-extensions~/visual-studio/UitkxVsix/UitkxCompletionSource.cs`

In `GetCompletionContextAsync`, detect the trigger:

```csharp
int triggerKind;     // 1 = Invoked, 2 = TriggerCharacter, 3 = TriggerForIncompleteCompletions
string? triggerChar = null;

if (trigger.Reason == CompletionTriggerReason.Insertion)
{
    var insertedChar = trigger.Character;
    if (insertedChar == '.' || insertedChar == '<' || insertedChar == '@' || insertedChar == '{')
    {
        triggerKind = 2;  // TriggerCharacter
        triggerChar = insertedChar.ToString();
    }
    else
    {
        triggerKind = 1;  // Invoked (user typed a letter, not a trigger char)
        triggerChar = null;
    }
}
else
{
    triggerKind = 1;
    triggerChar = null;
}
```

Then pass `triggerChar` in the LSP request:
```csharp
context = new
{
    triggerKind,
    triggerCharacter = triggerChar
}
```

**Verification:**  
In VS2022, type `someVar.` → member completions appear (not just keywords).

---

### T-09 · Sync changes to `dist~/` directory
**Status:** NOT STARTED  
**Priority:** REQUIRED before release  
**Depends on:** All other tasks  
**Effort:** ~15 minutes  
**Risk:** Very low  

**Problem:**  
The `dist~/` directory is a copy of the workspace used for distribution. All changes to `ide-extensions~/lsp-server/` and `ide-extensions~/language-lib/` must be mirrored to `dist~/ide-extensions~/lsp-server/` and `dist~/ide-extensions~/language-lib/`.

**Fix:**  
Verify the existing sync/build script (`scripts/` directory) handles this. If there is a build task or publish script, run it. If not, manually copy modified files:

```
ide-extensions~/lsp-server/Roslyn/RoslynHost.cs
    → dist~/ide-extensions~/lsp-server/Roslyn/RoslynHost.cs

ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs
    → dist~/ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs

ide-extensions~/lsp-server/CompletionHandler.cs
    → dist~/ide-extensions~/lsp-server/CompletionHandler.cs

ide-extensions~/language-lib/IntelliSense/AstCursorContext.cs
    → dist~/ide-extensions~/language-lib/IntelliSense/AstCursorContext.cs
    (only if T-05 Option B was chosen)
```

Also rebuild the VS Code extension and the VS2022 VSIX:
- `npm run build` in `ide-extensions~/vscode/`
- `dotnet build` in `ide-extensions~/visual-studio/UitkxVsix/`

---

### T-10 · Integration test scenarios
**Status:** NOT STARTED  
**Priority:** REQUIRED  
**Depends on:** T-01 through T-05  
**Effort:** ~2 hours  

**Test scenarios — ordered from simple to complex:**

| # | Scenario | Expected | Verifies |
|---|---|---|---|
| 1 | Press Ctrl+Space on a blank line in setup code | C# scope symbols appear (locals, fields, types) | T-01, T-02 |
| 2 | Type `from.` in setup code | Members of `from`'s type appear, not keyword list | T-01, T-02 |
| 3 | Type `.` on a blank line in markup | No popup (or tag completions from `<`, not C# keywords) | T-03, T-05 |
| 4 | Type `StyleKeys.` inside `style={StyleKeys.}` | Members of `StyleKeys` type appear | T-04 |
| 5 | Type `onClick={` | C# scope completions appear (callable symbols) | T-04 |
| 6 | Type variable name prefix in `value={my` | Filtered C# completions appear | T-04 |
| 7 | Press Ctrl+Space on `return (` line | No popup (or only UITKX items) | T-05 |
| 8 | Type `(` after a method call | Signature help appears (parameter hints) | T-06 |
| 9 | Hover over expression in `style={expr}` | Type name tooltip appears | T-07 |
| 10 | VS2022: type `from.` | Same as scenario 2 but in VS2022 | T-08 |

**Test file:** Use `RouterNavigationGuardPanel.uitkx` (complex real-world file with function-style component, setup code, multiple attribute expressions, `@code` block) as primary test artifact.

---

## 5 · NuGet Dependencies

No new NuGet packages are needed.

| Package | Version | Status | Purpose |
|---|---|---|---|
| `Microsoft.CodeAnalysis.CSharp` | 4.9.2 | ✅ Already referenced | Syntax, compilation |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | 4.9.2 | ✅ Already referenced | AdhocWorkspace, formatter |
| `Microsoft.CodeAnalysis.Workspaces.Common` | 4.9.2 | ✅ Already referenced | `MefHostServices`, `CompletionService`, `WellKnownTags` |

All three packages ship both `net8.0` and `netstandard2.0` targets. The LSP server runs as a standalone process separate from Unity, so there is no conflict with Unity's internal Roslyn version.

**If upgrading to a newer Roslyn (optional, future):**  
`Microsoft.CodeAnalysis.Workspaces.Common` 5.x targets .NET 8 natively and has improvements to `CompletionService`. To upgrade, change the Version attribute in `UitkxLanguageServer.csproj` for all three packages simultaneously (they must be the same version).

---

## 6 · Rider Support (Bonus Track)

**Status:** NOT IN SCOPE FOR THIS PLAN — existing stub only  
**File:** `ide-extensions~/rider/`  

The Rider plugin receives completions from the same LSP server. Once the LSP server correctly returns context-aware completions (T-01 through T-05), Rider will automatically benefit — **no Rider-specific changes are needed** for basic completion.

For Rider-native UX (completion icons, fancy UI integration), a Rider-specific `ILookupElementProvider` can be added but that is a separate project.

---

## 7 · VS2022 Integration Architecture (for reference)

The VS2022 extension does **not** use VS Code's `vscode-languageclient` protocol adapter. It calls LSP directly:

1. `UitkxLanguageClient` (MEF `ILanguageClient`) starts the server and captures `JsonRpc`.
2. `UitkxCompletionSourceProvider` creates `UitkxCompletionSource` for `.uitkx` buffers.
3. `UitkxCompletionSource.GetCompletionContextAsync` calls `textDocument/completion` via `JsonRpc`.
4. Results are mapped: LSP `CompletionItem[]` → VS `CompletionItem[]` via `JToken` deserialization.

The `UitkxMiddleLayer` intercepts some LSP messages for VS2022-specific processing. It does NOT intercept `textDocument/completion`.

---

## 8 · Known Limitations After This Plan

| Limitation | Explanation |
|---|---|
| No `completionItem/resolve` support | All data populated upfront; no deferred resolution. Acceptable for now. |
| No `textEdit` in completion items | `InsertText` replaces the current word only. Range replacement not implemented. |
| No snippet expansions | `CompletionService` returns some snippet items; `insertTextFormat` is not set to `Snippet`. |
| Roslyn workspace = per-file `AdhocWorkspace` | No cross-file navigation, no project-wide symbol resolution. The workspace knows only about the current file's assembly references. |
| `@code` block completions not tested | `CodeBlock` regions are in the source map but test coverage is for function-style components. |

---

## 9 · Implementation Checklist

```
[ ] T-01 · AdhocWorkspace(MefHostServices.DefaultHost)         — 1 line
[ ] T-02 · CompletionService replaces Recommender              — ~50 lines + cleanup
[ ] T-03 · Null-map fallback → Array.Empty                     — 3 lines
[ ] T-04 · AttributeValue in wantsRoslynCompletion             — 2 lines
[ ] T-05 · Source-map-based routing (fix leaky heuristic)      — ~20 lines
[ ] T-06 · SignatureHelpHandler (additive)                     — new file ~150 lines
[ ] T-07 · Hover for attribute expressions (verify + extend)   — ~10 lines
[ ] T-08 · VS2022 trigger char passing                         — ~15 lines
[ ] T-09 · Sync dist~/ + rebuild VSIX + extension             — build step
[ ] T-10 · Integration tests with RouterNavigationGuardPanel   — manual
```

---

## 10 · Critical Implementation Warnings

1. **T-01 must come before T-02.** Without `MefHostServices.DefaultHost`, `CompletionService.GetService()` returns null and the CompletionService path will silently fall back to empty or throw.

2. **T-03 must come before T-04.** Without the empty-fallback fix, adding `AttributeValue` to the gate will cause keyword popups on all attribute positions whose values are plain strings.

3. **Do not remove `CursorKind.CSharpCodeBlock` from the enum.** It may be used by other handlers (diagnostics, semantic tokens). Only remove/reduce its influence on `wantsRoslynCompletion` in `CompletionHandler`.

4. **`CompletionService.GetCompletionsAsync` may return null.** Always null-check `completionList` before accessing `.ItemsList`. In Roslyn 4.9.2, `null` is returned when no providers fire, not an empty list.

5. **`MefHostServices.DefaultHost` loaded once.** Do not call `new AdhocWorkspace(MefHostServices.DefaultHost)` in a hot path. It is already within `UpdateWorkspace()` which is called only once per file open, so this is fine.

6. **Virtual offset must be inside committed source text.** If the workspace was rebuilt 300ms after the last keystroke and the cursor moved, the virtual offset might be out of range for the old document. Always call `await _host.EnsureReadyAsync(...)` before calling `CompletionService.GetCompletionsAsync`.

7. **`WellKnownTags` namespace:** The `WellKnownTags` class is in `Microsoft.CodeAnalysis.Tags` namespace inside `Microsoft.CodeAnalysis.Workspaces.Common`. Add `using Microsoft.CodeAnalysis.Tags;`.

---

*End of plan. Last updated: 2025.*
