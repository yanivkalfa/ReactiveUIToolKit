# Semantic Coloring Unification Plan v2

**Goal**: Achieve consistent, type-aware coloring across VSCode and VS2022 by leveraging the LSP server's Roslyn analysis through editor-appropriate channels — semantic tokens for VSCode, custom notifications for VS2022.

**Branch**: `cleanup-before-release`  
**Status**: Phase 1 (Roslyn delegate detection) implemented and working for VSCode. Phases A+B ready for implementation.  
**Created**: 2026-04-09 (v2 — replaces original plan that assumed VS2022 supports LSP semantic tokens)

---

## Why the Original Plan Failed

The original plan (v1) assumed VS2022's LSP client supports `textDocument/semanticTokens`. **It does not.** Microsoft's own docs confirm semantic tokens are not listed in VS2022's supported LSP features. When we re-enabled semantic token registration (Phase 3 v1), VS2022's LSP framework couldn't process the tokens, causing a total whiteout.

**Key insight**: VS2022 colorizes custom LSP languages ONLY through `IClassifier` (MEF) or TextMate grammar. There is no LSP semantic token pipeline. This is a hard platform constraint, not a configuration issue.

---

## Problem Statement

Two independent coloring systems are maintained in parallel:

| | VSCode | VS2022 |
|---|---|---|
| **Source** | LSP semantic tokens (UITKX + Roslyn) | Hand-written lexer (UitkxClassifier) |
| **Type awareness** | Yes (Roslyn SemanticModel) | No (char lookahead only) |
| **Hook support** | Roslyn delegate detection (Phase 1) | None |
| **Maintenance** | Shared LSP server | Separate ~1400 LOC C# lexer |

This causes:
1. `useState<string>(...)` colored as identifier (blue) instead of function (yellow) in VS2022 — generics break `identifier(` detection
2. `setGameStarted` in `var (gameStarted, setGameStarted) = useState(...)` is blue in VS2022 — no type info to know it's a delegate
3. Custom hooks with N return values (some delegates, some values) can't be colored correctly in VS2022
4. VSCode is correct (Phase 1 delegate detection already works)

## Target Architecture (v2)

```
                    ┌─────────────────────────────────────┐
                    │         LSP Server (shared)          │
                    │                                      │
                    │  SemanticTokensProvider (UITKX AST)  │
                    │    → tags, attributes, directives    │
                    │                                      │
                    │  RoslynSemanticTokensProvider         │
                    │    → C# classifications + delegate   │
                    │      detection (TypeKind.Delegate)   │
                    │                                      │
                    │  ┌─ VSCode path ─────────────────┐   │
                    │  │ Merged → semanticTokens resp  │   │
                    │  └───────────────────────────────-┘   │
                    │                                      │
                    │  ┌─ VS2022 path ─────────────────┐   │
                    │  │ uitkx/classificationOverrides │   │
                    │  │ custom notification (push)    │   │
                    │  └───────────────────────────────-┘   │
                    └──────────┬──────────┬────────────────┘
                               │          │
                    ┌──────────▼──┐  ┌────▼─────────────────┐
                    │   VSCode    │  │      VS2022           │
                    │             │  │                        │
                    │ TextMate    │  │ UitkxClassifier        │
                    │ (baseline)  │  │ (instant baseline)     │
                    │      +      │  │        ↓               │
                    │ LSP semantic│  │ MiddleLayer intercepts  │
                    │ tokens      │  │ uitkx/classOverrides   │
                    │ (override)  │  │        ↓               │
                    │             │  │ OverrideStore (static)  │
                    │             │  │        ↓               │
                    │             │  │ Classifier applies      │
                    │             │  │ overrides atomically    │
                    │             │  │        +               │
                    │             │  │ Unreachable dimming     │
                    │             │  │ (always wins)           │
                    └─────────────┘  └────────────────────────┘
```

**Why this works**: VS2022's UitkxClassifier is the sole classification source. It already receives external data from the LSP server (diagnostics for unreachable code dimming) via the MiddleLayer → Store → Event pattern. We add a second data channel (classification overrides) using the exact same pattern.

**Why this won't flicker**: All classification comes from a single `IClassifier`. No aggregator conflict between multiple sources. `ClassificationChanged` fires once with the full buffer span — same as the unreachable code flow that's already stable.

---

## Phase 1: Roslyn Delegate Detection (DONE ✅)

**Files**: `ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs`  
**Status**: Implemented and working for VSCode. In stash `all changes`.

In `GetTokensAsync()`, after Roslyn classifies a span as `"local name"` or `"parameter name"`, queries `SemanticModel.GetTypeInfo()`. If the type is `TypeKind.Delegate`, overrides the token type from `Variable` → `Function`.

Covers ALL delegate-typed locals generically — `StateSetter<T>`, `Action`, `Func<T>`, etc.

**Result**: VSCode correctly colors hook setters as functions (yellow). VS2022 unaffected (semantic tokens stripped).

---

## Phase A: Fix Generics in UitkxClassifier (VS2022 instant improvement)

**Files**: `ide-extensions~/visual-studio/UitkxVsix/UitkxClassifier.cs`  
**Affects**: VS2022 only  
**Estimated LOC**: ~30  
**Risk**: Very low  
**Dependencies**: None

### Problem

`ClassifyExpressionSegment` and `ClassifyAll` detect function calls via:
```
parse identifier → skip whitespace → check char == '('
```

For `useState<string>(...)`:
- Parses `useState`, `i` now points to `<`
- Probes forward: `text[probe] == '('` → **FALSE** (`<` ≠ `(`)
- Classified as `_identifier` (blue) instead of `_method` (yellow)

### Solution

After the identifier parse + whitespace skip, ALSO check for `<...>(` pattern:

```csharp
if (probe < end && text[probe] == '(')
{
    AddSpan(snapshot, spans, identStart, token.Length, _method);
}
else if (probe < end && text[probe] == '<')
{
    // Try to skip generic type parameters: identifier<T>(
    int genEnd = TrySkipGenericArgs(text, probe, end);
    if (genEnd > 0)
    {
        int probe2 = genEnd;
        while (probe2 < end && char.IsWhiteSpace(text[probe2]))
            probe2++;
        if (probe2 < end && text[probe2] == '(')
            AddSpan(snapshot, spans, identStart, token.Length, _method);
        else
            AddSpan(snapshot, spans, identStart, token.Length, _identifier);
    }
    else
    {
        AddSpan(snapshot, spans, identStart, token.Length, _identifier);
    }
}
else
{
    AddSpan(snapshot, spans, identStart, token.Length, _identifier);
}
```

`TrySkipGenericArgs` — conservative bracket matching:

```csharp
private static int TrySkipGenericArgs(string text, int start, int end)
{
    if (start >= end || text[start] != '<') return -1;
    int depth = 1;
    int i = start + 1;
    while (i < end && depth > 0)
    {
        char c = text[i];
        if (c == '<') depth++;
        else if (c == '>') depth--;
        // Bail out on characters that can't appear in generic args
        else if (c == ';' || c == '{' || c == '}' || c == '=' && (i + 1 >= end || text[i + 1] != '>'))
            return -1;
        i++;
    }
    return depth == 0 ? i : -1;
}
```

### Where to Apply

Two sites use the `identifier → probe → (` pattern:
1. `ClassifyAll` method (~line 712)
2. `ClassifyExpressionSegment` method (~line 858)

Both need the same fix.

### False Positive Analysis

**Risk**: `foo < bar > (baz)` misread as generic call.
- Mitigated by bailout on `=`, `;`, `{`, `}` inside `<>`
- In UITKX C# code, bare comparison chains followed by parenthesized expressions are extremely rare
- Even if misclassified, the override from Phase B would correct it ~300ms later

### Result

After this phase:
- `useState<string>(...)` → yellow immediately in VS2022
- `useState<int[]>(...)` → yellow immediately
- `useReducer<State, Action>(...)` → yellow immediately
- No wait for Roslyn — pure lexer improvement

---

## Phase B: Custom Classification Override Notification (the core solution)

**Files**:
- Server: `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` (~60 LOC)
- Server: `ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs` (~15 LOC — new public method)
- Client: `ide-extensions~/visual-studio/UitkxVsix/UitkxMiddleLayer.cs` (~15 LOC)
- Client: `ide-extensions~/visual-studio/UitkxVsix/UitkxDiagnosticTagger.cs` (~40 LOC — new store class)
- Client: `ide-extensions~/visual-studio/UitkxVsix/UitkxClassifier.cs` (~50 LOC)

**Affects**: VS2022 only (VSCode already has full coloring via Phase 1)  
**Estimated LOC**: ~180 net new  
**Risk**: Low — mirrors existing diagnostic flow pattern  
**Dependencies**: Phase 1 (reuse Roslyn analysis)

### Architecture

```
┌─ RoslynHost.RebuildAsync() completes ─────────────────────────────────┐
│  1. Virtual doc rebuilt, SemanticModel ready                          │
│  2. publisher.PushTier3(diagnostics)  ← existing                     │
│  3. publisher.PushOverrides(uitkxPath) ← NEW                         │
│     └─ RoslynSemanticTokensProvider.GetDelegateOverridesAsync()       │
│        └─ Roslyn classifies → finds delegate-typed locals/params     │
│        └─ Returns list of (line, col, length, "function")            │
│     └─ _server.SendNotification("uitkx/classificationOverrides", ..) │
└───────────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (JSON-RPC notification)
┌─ VS2022 MiddleLayer ─────────────────────────────────────────────────┐
│  HandleNotificationAsync("uitkx/classificationOverrides", ...)       │
│     └─ ClassificationOverrideStore.HandleOverrides(param)            │
│        └─ Parse JSON → store overrides per URI                       │
│        └─ Fire OverridesChanged event                                │
│                              │                                       │
│                              ▼                                       │
│  UitkxClassifier.OnOverridesChanged()                                │
│     └─ Store overrides in _classificationOverrides                   │
│     └─ Invalidate cache (_cachedSnapshot = null)                     │
│     └─ Fire ClassificationChanged (full buffer span)                 │
│                              │                                       │
│                              ▼                                       │
│  UitkxClassifier.GetClassificationSpans()                            │
│     └─ Lexical pass (baseline, as today)                             │
│     └─ Apply overrides: for each override, find matching span        │
│        at (line,col,length) and replace classification type          │
│     └─ Apply unreachable dimming (unchanged)                         │
└───────────────────────────────────────────────────────────────────────┘
```

### Server Side Implementation

#### 1. New method on RoslynSemanticTokensProvider

```csharp
/// <summary>
/// Returns only spans where Roslyn's type analysis overrides the lexer's
/// classification (e.g., delegate-typed locals that should be Function).
/// Lightweight alternative to GetTokensAsync for VS2022's override channel.
/// </summary>
public async Task<(int Line, int Col, int Length, string Type)[]> GetDelegateOverridesAsync(
    Document document,
    SourceMap map,
    string uitkxSource,
    CancellationToken ct)
```

This extracts ONLY the delegate-typed locals/parameters — the spans where the classifier would assign `_identifier` but the correct classification is `_method`.

#### 2. Hook into DiagnosticsPublisher.PushTier3

After pushing T3 diagnostics, if `IsVisualStudio`, also compute and send overrides:

```csharp
// In PushTier3, after PushToClient(uri, combined):
if (CapabilityPatchStream.IsVisualStudio)
    PushClassificationOverrides(uitkxFilePath, uitkxSource);
```

`PushClassificationOverrides` calls `RoslynSemanticTokensProvider.GetDelegateOverridesAsync()` using the cached `RoslynHost.GetRoslynDocument()` + `GetVirtualDocument()`, then sends:

```csharp
_server.SendNotification("uitkx/classificationOverrides", new
{
    uri = DocumentUri.File(uitkxFilePath).ToString(),
    overrides = overrideArray
});
```

#### 3. Notification payload

```json
{
    "uri": "file:///c%3A/Users/neta/Tic-tac-toe/Assets/UI/App/Pages/Home/Home.uitkx",
    "overrides": [
        { "line": 3, "character": 25, "length": 16, "type": "function" },
        { "line": 4, "character": 25, "length": 14, "type": "function" }
    ]
}
```

Only `"function"` overrides are sent (delegate detection). Future: could extend to other override types.

### Client Side Implementation

#### 1. ClassificationOverrideStore (new static class)

Mirrors `UitkxDiagnosticStore` exactly:

```csharp
internal static class ClassificationOverrideStore
{
    internal static event Action<string, List<ClassificationOverride>>? OverridesChanged;

    internal static void HandleOverrides(JToken param)
    {
        var uri = param["uri"]?.Value<string>();
        if (string.IsNullOrEmpty(uri)) return;

        var list = new List<ClassificationOverride>();
        var arr = param["overrides"] as JArray;
        if (arr != null)
        {
            foreach (var item in arr)
            {
                list.Add(new ClassificationOverride
                {
                    Line = item["line"]!.Value<int>(),
                    Character = item["character"]!.Value<int>(),
                    Length = item["length"]!.Value<int>(),
                    Type = item["type"]!.Value<string>()!,
                });
            }
        }
        OverridesChanged?.Invoke(uri!, list);
    }
}

internal struct ClassificationOverride
{
    public int Line;
    public int Character;
    public int Length;
    public string Type;
}
```

#### 2. MiddleLayer addition

In `HandleNotificationAsync`, add before the final `await sendNotification(methodParam)`:

```csharp
if (methodName == "uitkx/classificationOverrides")
{
    ClassificationOverrideStore.HandleOverrides(methodParam);
    return; // Don't forward to VS2022 — it doesn't know this method
}
```

#### 3. UitkxClassifier override integration

Subscribe to `ClassificationOverrideStore.OverridesChanged` (same pattern as diagnostics):

```csharp
// In EnsureSnapshotClassified, alongside existing DiagnosticsChanged subscription:
ClassificationOverrideStore.OverridesChanged += OnOverridesChanged;

// New field:
private List<ClassificationOverride>? _classificationOverrides;

// New handler:
private void OnOverridesChanged(string uri, List<ClassificationOverride> overrides)
{
    // URI match check (same as OnDiagnosticsChanged)
    _classificationOverrides = overrides;
    _cachedSnapshot = null; // invalidate cache
    ClassificationChanged?.Invoke(this,
        new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
}
```

Apply overrides in `GetClassificationSpans`, AFTER lexical pass, BEFORE unreachable dimming:

```csharp
// Apply classification overrides from Roslyn
var overrides = _classificationOverrides;
if (overrides != null && overrides.Count > 0)
{
    for (int s = 0; s < result.Count; s++)
    {
        var span = result[s];
        var spanStart = span.Span.Start;
        var line = snapshot.GetLineFromPosition(spanStart);
        int lineNum = line.LineNumber;
        int col = spanStart - line.Start.Position;
        int len = span.Span.Length;

        foreach (var ov in overrides)
        {
            if (ov.Line == lineNum && ov.Character == col && ov.Length == len)
            {
                var newType = MapOverrideType(ov.Type);
                if (newType != null)
                    result[s] = new ClassificationSpan(span.Span, newType);
                break;
            }
        }
    }
}
```

`MapOverrideType` maps the string type to the classifier's `IClassificationType`:
```csharp
private IClassificationType? MapOverrideType(string type) => type switch
{
    "function" => _method,
    "type" => _typeName,
    "keyword" => _keyword,
    _ => null
};
```

### Timing Analysis

```
0ms     — File opens / user edits
0ms     — UitkxClassifier fires (baseline + Phase A generics fix)
          useState<string> → yellow (Phase A), setGameStarted → blue (no type info yet)
~300ms  — Roslyn rebuild completes
~300ms  — T3 diagnostics + uitkx/classificationOverrides sent
~310ms  — MiddleLayer receives, store fires event
~310ms  — Classifier invalidates, ClassificationChanged fires
~310ms  — VS2022 re-paints: setGameStarted → yellow (override applied)
```

Net visible effect: `setGameStarted` is blue for ~300ms, then turns yellow. Same delay as unreachable code dimming (which users already accept).

### Why This Can't Fail the Way v1 Failed

| v1 Failure | Why v2 Is Different |
|---|---|
| VS2022 doesn't support semantic tokens | We don't use semantic tokens. Custom notification via existing JSON-RPC pipe. |
| Semantic token registration caused whiteout | We don't register any new VS2022 capabilities. `CapabilityPatchStream` unchanged. |
| Classification aggregator conflict (two sources) | Single source: UitkxClassifier. Overrides modify its output, not compete with it. |
| Stripping classifier (Phase 4 v1) removed safety net | Classifier untouched. Overrides are additive corrections. |
| Theoretical audit missed integration bugs | Every component used here is already proven: `MiddleLayer.HandleNotificationAsync` handles `publishDiagnostics` today; `UitkxDiagnosticStore` pattern is identical; `_server.SendNotification` is the same OmniSharp API used for diagnostics. |

### What Could Go Wrong (and mitigations)

1. **Custom notification dropped**: `CanHandle()` returns `true` for all methods. `HandleNotificationAsync` already routes custom methods. Risk: negligible.

2. **Stale overrides after fast typing**: Same as stale diagnostics — replaced on next Roslyn rebuild (~300ms debounce). Acceptable.

3. **Position mismatch**: If user edits between Roslyn rebuild and override application, line/col may be off. Mitigation: overrides apply to the NEXT `GetClassificationSpans` call which uses the current snapshot — if positions don't match any span, the override is simply skipped. Worst case: one cycle of baseline-only coloring.

4. **Performance**: `GetDelegateOverridesAsync` reuses the cached `Document` and `SemanticModel` from the just-completed Roslyn rebuild. Cost: ~50-100ms on top of T3 diagnostics (~200ms). Stays within the existing debounce window.

5. **VSCode receives notification**: `CapabilityPatchStream.IsVisualStudio` gate ensures overrides are ONLY sent when the client is VS2022. Zero impact on VSCode.

---

## Phase Summary

| Phase | Files Changed | Affects | Risk | Dependency | Status |
|-------|--------------|---------|------|------------|--------|
| 1. Roslyn delegate detection | RoslynSemanticTokensProvider.cs | VSCode | Low | None | **DONE** ✅ |
| A. Fix generics in classifier | UitkxClassifier.cs | VS2022 | Very Low | None | Ready |
| B. Classification override notification | DiagnosticsPublisher.cs, RoslynSemanticTokensProvider.cs, UitkxMiddleLayer.cs, UitkxDiagnosticTagger.cs, UitkxClassifier.cs | VS2022 | Low | Phase 1 | Ready |

Phase A and B are independent — can be done in parallel.  
Phase A gives instant improvement for the most common case (generics).  
Phase B gives full type-aware improvement for ALL cases (delegates anywhere).

### What to Do with Stashed Changes

The stash `all changes` contains:
- **Phase 1 (RoslynSemanticTokensProvider.cs)**: KEEP — core analysis, used by both VSCode and VS2022 Phase B
- **[Order] attributes (UitkxClassifications.cs)**: REMOVE — no longer needed (we're not using semantic tokens in VS2022)
- **CollectHookSetterTokens removal (SemanticTokensProvider.cs)**: KEEP — superseded by Phase 1 Roslyn detection

---

## Key Files Reference

| File | Role |
|------|------|
| `ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs` | Roslyn C# classification → LSP tokens + delegate overrides |
| `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` | T1+T2+T3 diagnostics + classification override notification |
| `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` | Roslyn workspace management, calls PushTier3 after rebuild |
| `ide-extensions~/lsp-server/CapabilityPatchStream.cs` | VS2022 capability patching (strips semanticTokens, unchanged) |
| `ide-extensions~/lsp-server/SemanticTokensHandler.cs` | Merge UITKX + Roslyn tokens for VSCode |
| `ide-extensions~/language-lib/SemanticTokens/SemanticTokensProvider.cs` | UITKX AST → semantic tokens (tags, directives) |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxClassifier.cs` | VS2022 hand-written lexer + override application |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxMiddleLayer.cs` | LSP message interceptor (diagnostics + overrides) |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxDiagnosticTagger.cs` | Diagnostic store + ClassificationOverrideStore |
| `ide-extensions~/visual-studio/UitkxVsix/UitkxClassifications.cs` | VS2022 classification type + format definitions |
| `ide-extensions~/grammar/uitkx.tmLanguage.json` | TextMate grammar (VSCode baseline) |
