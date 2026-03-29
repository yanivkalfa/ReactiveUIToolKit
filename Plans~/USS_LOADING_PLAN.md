# USS Stylesheet Loading — Implementation Plan

**Status:** ✅ Complete (core functionality) — polish items remain (LSP path completion, sample component)  
**Priority:** Medium (unlocks USS pseudo-state styling: `:hover`, `:active`, `:focus`)  
**Chosen approach:** Option B — Per-Component with static cache + detached-element attachment  
**Depends on:** `ASSET_REGISTRY_PLAN.md` — the registry SO and `Asset<T>()` / `Ast<T>()` helper
are the foundation. USS loading uses `UitkxAssetRegistry.Get<StyleSheet>(key)` at runtime.

### Delivery Phases

| Deliverable | Scope | Status |
|-------------|-------|--------|
| **1 — Asset Registry** (ASSET_REGISTRY_PLAN.md) | `UitkxAssetRegistry` SO + `Asset<T>()` / `Ast<T>()` + Editor sync + Source gen path resolution | ✅ Complete |
| **2 — USS Loading** (this plan) | `@uss` directive parsing + `__ussKeys` emission + PropsApplier detached-element attachment | ✅ Complete (polish remaining) |

---

## 1. Problem

`className` is fully wired (`AddToClassList`/`RemoveFromClassList` with efficient diffing)
but there is no way to load the `.uss` file that defines those classes from within a `.uitkx`
component. Users must manually call `rootVisualElement.styleSheets.Add(...)` in bootstrap
code, making `className` effectively useless for USS-based styling.

---

## 2. Key Architectural Insight — Detached Element Attachment

`FiberReconciler.CommitPlacement` applies properties **before** inserting the element into
the panel:

```
1. CreateElement()           ← element exists, DETACHED (no parent, no panel)
2. ApplyProperties()         ← className, style, events applied — STILL DETACHED
3. AppendChild/InsertBefore  ← element joins parent → OnAttachToPanel fires
```

If `element.styleSheets.Add(sheet)` happens during step 2 (inside `ApplyProperties`), the
stylesheet is present **before** the element enters the panel. Unity resolves styles only
once on attachment — **identical to how UXML `CloneTree()` works**. Zero re-resolution cost,
no dirty flags, no cascade penalty.

This eliminates all performance concerns with runtime stylesheet loading.

---

## 3. Options Evaluated

### Option A — Panel-Global (Next.js `import './style.css'` model)
All `@uss` files loaded onto panel root. Simple but no isolation between mount points.
Naming collisions. Can't scope styles per tree. Dynamic sheet changes dirty ALL elements.
**Rejected.**

### Option B — Per-Component Root Element ✅ CHOSEN
Each component with `@uss` attaches its sheet to its own root VisualElement via the
`__ussKeys` prop. Sheet loaded from a static cache (populated once from a registry SO).
Exploits the detached-element gap so attachment is free.

### Option C — Per-Mount-Point Collection (recursive from entry)
Walk component graph at mount time, collect all `@uss` refs, attach to mount root.
Breaks with portals (children render outside mount root). Dynamic/lazy components won't
have their USS until they actually render. **Rejected.**

### Option D — Hybrid (lazy per-component + ancestor promotion)
Starts as B, promotes frequently-used sheets to nearest ancestor. Sounds optimal but
**promotes sheets to ancestor = expands the dirty scope when sheets change**, making
dynamic changes worse. Complexity not justified. **Rejected.**

### Option E — Explicit `useStyleSheet` Hook
No directive. Components call a hook. Verbose, easy to forget, no static analysis.
Deviates from `@using`/`@namespace` directive convention. **Rejected.**

### Performance Ranking (steady state)

| Rank | Option | Steady-State Cost | First-Render Cost |
|:----:|--------|-------------------|-------------------|
| 1 | A: Global | Zero | Lowest |
| 2 | D: Hybrid | Zero (after promotion) | Medium |
| 3 | C: Per-Mount | Zero | Medium-High |
| 4 | **B: Per-Component** | **Near-zero (detached attachment)** | **Low** |
| 5 | E: Hook | Low (effect overhead) | Highest |

With the detached-element insight, Option B's first-render cost is identical to A/C/D
because `styleSheets.Add()` on a detached element is free — the actual cost is selector
matching at panel attachment time, which is unavoidable and shared by all approaches
(including Unity's own UXML).

---

## 4. Chosen Design — Option B Detailed

### 4.1 User Experience

```uitkx
@component PlayerCard
@uss "./PlayerCard.uss"
@uss "../shared/buttons.uss"

<VisualElement className="player-card">
    <Label className="player-name" text={name} />
    <Button className="action-btn" text="Attack" />
</VisualElement>
```

Multiple `@uss` directives allowed. Paths are relative to the `.uitkx` file.

### 4.2 End-to-End Pipeline

```
PlayerCard.uitkx             (user writes @uss "./PlayerCard.uss")
        ↓
DirectiveParser.cs            (parses @uss, stores paths in DirectiveSet.UssSheets)
        ↓
CSharpEmitter.cs              (emits static __uitkx_ussKeys array)
        ↓
UitkxChangeWatcher.cs         (detects @uss, resolves path, updates registry SO)
        ↓
Resources/__uitkx_registry    (ScriptableObject mapping path keys → StyleSheet refs)
        ↓
Generated Render()            (includes __ussKeys in root element props)
        ↓
PropsApplier.ApplySingle      (handles __ussKeys → loads from cache → element.styleSheets.Add)
        ↓
FiberReconciler               (element attaches to panel with sheet already present)
```

### 4.3 No File Duplication

USS files stay where the user placed them. The registry ScriptableObject holds Unity
serialized references (GUIDs), not copies. USS is imported and compiled by Unity **once**
at its original location. The registry is ~50 bytes per entry.

### 4.4 Registry ScriptableObject

```csharp
// Runtime/Core/UitkxStyleSheetRegistry.cs
public sealed class UitkxStyleSheetRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string key;          // "Assets/UI/PlayerCard"
        public StyleSheet sheet;    // serialized reference to original .uss
    }

    [SerializeField] private Entry[] entries;

    // Runtime: cached lookup
    private static Dictionary<string, StyleSheet> s_cache;

    public static StyleSheet Get(string key)
    {
        if (s_cache == null) LoadCache();
        s_cache.TryGetValue(key, out var sheet);
        return sheet;
    }

    private static void LoadCache()
    {
        var registry = Resources.Load<UitkxStyleSheetRegistry>("__uitkx_registry");
        s_cache = new Dictionary<string, StyleSheet>();
        if (registry != null)
            foreach (var e in registry.entries)
                s_cache[e.key] = e.sheet;
    }
}
```

### 4.5 Source Generator Output

```csharp
// <auto-generated/> PlayerCard.uitkx.g.cs
namespace MyApp.UI
{
    public partial class PlayerCard
    {
        // Static per component TYPE, not per instance
        internal static readonly string[] __uitkx_ussKeys = new[]
        {
            "Assets/UI/PlayerCard",
            "Assets/UI/shared/buttons"
        };

        public static VirtualNode Render(
            global::ReactiveUITK.Core.IProps __rawProps,
            IReadOnlyList<VirtualNode> __children)
        {
            // ... function setup ...

            return V.Create<VisualElement>(new Dictionary<string, object>
            {
                { "__ussKeys", __uitkx_ussKeys },
                { "className", "player-card" }
            },
                V.Create<Label>(new Dictionary<string, object>
                {
                    { "className", "player-name" },
                    { "text", name }
                }),
                V.Create<Button>(new Dictionary<string, object>
                {
                    { "className", "action-btn" },
                    { "text", "Attack" }
                })
            );
        }
    }
}
```

### 4.6 PropsApplier Integration

```csharp
// In PropsApplier.ApplySingle — new case:
if (propertyName == "__ussKeys")
{
    if (propertyValue is string[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            var sheet = UitkxStyleSheetRegistry.Get(keys[i]);
            if (sheet != null && !element.styleSheets.Contains(sheet))
                element.styleSheets.Add(sheet);
        }
    }
    return;
}
```

Diffing: `ApplyDiff` uses `ReferenceEquals` on props — `__uitkx_ussKeys` is a static
`readonly` array, so re-renders with the same component skip the USS check entirely.

### 4.7 UitkxChangeWatcher Extension

On `.uitkx` save, the watcher already triggers recompilation. Extend it to:

1. Parse `@uss` directives from the saved file
2. Resolve relative paths to absolute asset paths
3. Load/create `Resources/__uitkx_registry.asset`
4. Update entries (add new, remove stale, update changed paths)
5. `EditorUtility.SetDirty(registry)` + `AssetDatabase.SaveAssets()`

---

## 5. Scenario Coverage

| Scenario | How It Works |
|----------|-------------|
| **Single mount point** | Sheet added to component's root element before panel attachment. Standard flow. |
| **Multiple mount points** | Each tree's components carry their own `__ussKeys`. Isolation by default. |
| **Shared component in two trees** | Same `StyleSheet` object from cache added to elements in both trees. Unity handles this. |
| **Portals** | Portal'd element carries its `__ussKeys` prop → sheet applied before attachment to portal target. Styles travel with the component. |
| **Dynamic/lazy components** | Sheet added when component first renders, before its element attaches. Just works. |
| **Same USS in 50 instances** | Same `StyleSheet` reference from cache. `element.styleSheets.Add()` on detached element is free. No duplication. |
| **USS content changes (user edits .uss)** | Unity reimports in-place. Same `StyleSheet` object, new content. Elements update automatically. No UITKX action needed. |
| **`@uss` directive added/removed** | Source generator re-emits `__uitkx_ussKeys`. Next render: `ApplyDiff` detects new array reference → adds/removes sheets. Only dirties that component's subtree. |
| **HMR (soft re-render)** | VisualElement tree persists. Sheets already attached. If `__ussKeys` unchanged → `ReferenceEquals` skip. |
| **HMR (directive change)** | New `__uitkx_ussKeys` emitted → diff applies → old sheet removed, new sheet added. Scoped to component. |
| **Clean project / first open** | `UitkxChangeWatcher` runs on domain reload, scans all `.uitkx` files, rebuilds registry. |

---

## 6. Implementation Checklist

### Phase 1 — Runtime Foundation
```
[x] 1. Shared/Core/UitkxAssetRegistry.cs — ScriptableObject + static cache (Get<T>, Contains, LoadCache, InjectCacheEntry)
[x] 2. PropsApplier.cs — Handle __ussKeys prop (add sheets from cache via UitkxAssetRegistry.Get<StyleSheet>)
[x] 3. PropsApplier.cs — Handle __ussKeys in ApplyDiff / RemoveProp (element.styleSheets.Remove)
```

### Phase 2 — Editor Tooling
```
[x] 4. Editor/UitkxAssetRegistrySync.cs — Parse @uss directives on .uitkx save (regex: @uss\s+"([^"]+)")
[x] 5. Editor/UitkxAssetRegistrySync.cs — Resolve USS paths, update registry SO (ResolvePath → LoadAssetTyped → registry.Set)
[x] 6. Editor/UitkxAssetRegistrySync.cs — Full rescan on domain reload via [InitializeOnLoad] → FullRescan() → ReplaceAll()
[x] 7. Resources/__uitkx_registry.asset — Auto-created by GetOrCreateRegistry()
```

### Phase 3 — Directive Parsing (Language-Lib + Source Generator)
```
[x] 8. language-lib/Parser/DirectiveParser.cs — TryReadFunctionStyleUss() parses @uss "path" lines
[x] 9. language-lib/Parser/ParseResult.cs — ImmutableArray<string> UssFiles in DirectiveSet
[x] 10. SourceGenerator~/Emitter/CSharpEmitter.cs — Emits static __uitkx_ussKeys array + UITKX0022/0023 diagnostics
[x] 11. SourceGenerator~/Emitter/CSharpEmitter.cs — Injects __ussKeys into root element props dict
```

### Phase 4 — IDE Support
```
[ ] 12. uitkx-schema.json — Document @uss directive (not in schema; @uss is a preamble directive)
[ ] 13. LSP CompletionHandler — Path completion after @uss " (not implemented; users type manually)
[ ] 14. LSP DiagnosticsPublisher — Real-time warning for missing .uss (compensated by SG UITKX0022 at compile time)
[x] 15. grammar/uitkx.tmLanguage.json — Highlight @uss directive (uss-directive pattern: keyword + string path)
```

### Phase 5 — Polish
```
[ ] 16. .gitignore — Add Resources/__uitkx_registry.asset policy note
[x] 17. Documentation — Assets page (@uss section) + Styling page (USS Stylesheets section) on docs site
[ ] 18. Sample — Add USS example to Samples/ (no @uss sample components exist yet)
[x] 19. Tests — Source generator tests: UITKX0120 UssDirective_MissingFile, AssetCall_MissingFile, etc.
[ ] 20. Tests — LSP tests for @uss diagnostics and completion (no @uss-specific LSP tests)
```

---

## 7. Files to Create/Modify

| File | Action | Phase |
|------|--------|-------|
| `Runtime/Core/UitkxStyleSheetRegistry.cs` | **Create** | 1 |
| `Shared/Props/PropsApplier.cs` | Modify — add `__ussKeys` handling | 1 |
| `Editor/UitkxChangeWatcher.cs` | Modify — add `@uss` detection + registry sync | 2 |
| `ide-extensions~/language-lib/Parser/DirectiveParser.cs` | Modify — parse `@uss` | 3 |
| `ide-extensions~/language-lib/Parser/ParseResult.cs` | Modify — add `UssSheets` field | 3 |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Modify — emit `__uitkx_ussKeys` | 3 |
| `ide-extensions~/grammar/uitkx-schema.json` | Modify — document `@uss` | 4 |
| `ide-extensions~/lsp-server/CompletionHandler.cs` | Modify — path completions | 4 |
| `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` | Modify — missing USS warning | 4 |

---

## 8. Open Questions

1. **Registry commit policy** — Should `__uitkx_registry.asset` be committed to git
   (deterministic, needed for builds) or gitignored (generated artifact)?
   **Leaning commit** — it's small and required for builds without Editor.

2. **Runtime-only projects (no Editor)** — The registry must be pre-built. CI needs an
   Editor step to generate/update the registry before building the player.

3. **USS hot-reload in Play Mode** — When a `.uss` file changes during Play Mode, Unity
   reimports it and the `StyleSheet` object updates. Do attached elements pick up the change
   automatically, or do we need to re-add the sheet? **Needs testing.**

4. **Addressables users** — Some projects don't use `Resources/`. Could offer an alternative
   registry backend, but `Resources/` covers 95% of use cases. Defer to v2.
