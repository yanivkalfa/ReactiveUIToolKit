# Asset Registry — Implementation Plan

**Status:** � In progress — Deliverable 1  
**Priority:** High (foundation for USS loading, image/font/material references, and future asset types)  
**Delivery:** This is **Deliverable 1**. USS Loading (USS_LOADING_PLAN.md) is **Deliverable 2** and builds on this.

### Delivery Phases

| Deliverable | Scope | Status |
|-------------|-------|--------|
| **1 — Asset Registry** (this plan) | `UitkxAssetRegistry` SO + `Asset<T>()` / `Ast<T>()` + Editor sync + Source gen path resolution + HMR cache injection + LSP UITKX0120 diagnostic | ✅ Complete |
| **2 — USS Loading** (USS_LOADING_PLAN.md) | `@uss` directive parsing + `__ussKeys` emission + PropsApplier detached-element attachment | ✅ Complete (polish remaining) |

---

## 1. Problem

UITKX components have no way to reference Unity assets (textures, sprites, fonts, USS
stylesheets, materials) by path. Users must pass pre-loaded object references from
MonoBehaviour inspector fields or manual `Resources.Load` calls.

This creates friction for:
- USS loading (`@uss` directive) — needs StyleSheet references at runtime
- Image backgrounds — `<Image texture={???} />` requires a Texture2D object
- Font customization — style `FontFamily` requires a Font object
- Shared assets — same texture used in 10 components, loaded 10 times

---

## 2. Design Goals

1. **Type-safe** — `Asset<Texture2D>("./avatar.png")` checked at compile time
2. **Zero per-frame cost** — cached dictionary lookup, no `Resources.Load` per render
3. **No file duplication** — registry holds GUID references, not copies
4. **Works in builds** — `Resources.Load` for the registry SO, asset deps followed by Unity
5. **Simple DX** — follows the `using static CssHelpers` pattern users already know
6. **Asset-type agnostic** — one registry, one API, any `UnityEngine.Object` subtype
7. **Editor auto-sync** — watcher detects asset references dynamically

---

## 3. User Experience

### 3.1 Inline Expressions — `Asset<T>("./path")`

```uitkx
@component PlayerCard
@using static ReactiveUITK.AssetHelpers
@uss "./PlayerCard.uss"

<VisualElement className="player-card">
    <Image texture={Asset<Texture2D>("./avatar.png")} />
    <Image sprite={Asset<Sprite>("./icons/rank.png")} />
    <Label style={new Style { BackgroundImage = Asset<Texture2D>("../shared/panel-bg.png") }} />
</VisualElement>
```

**How it reads:** "Load the Texture2D at `./avatar.png` relative to this file."

The C# compiler validates:
- `Asset<Texture2D>(...)` returns `Texture2D` → assignable to `ImageProps.Texture` ✅
- `Asset<Sprite>(...)` returns `Sprite` → assignable to `ImageProps.Sprite` ✅
- `Asset<AudioClip>(...)` → `Texture` prop? **Compile error** ❌ (type mismatch)

### 3.2 Directive Form — `@uss`

```uitkx
@uss "./PlayerCard.uss"
```

`@uss` is syntactic sugar. Under the hood it uses the same registry. The source generator
emits registry lookups for `@uss`, so the user doesn't write `Asset<StyleSheet>(...)` for
stylesheets — the directive handles it.

### 3.3 Auto-Injected Using

The source generator auto-injects `using static ReactiveUITK.AssetHelpers;` when
`Asset<T>()` calls are detected in expressions (or always — it's harmless). Users
can also add `@using static ReactiveUITK.AssetHelpers` explicitly.

---

## 4. API Design

### 4.1 The `Asset<T>` Function

```csharp
// Shared/Core/AssetHelpers.cs
namespace ReactiveUITK
{
    /// <summary>
    /// Provides the <c>Asset&lt;T&gt;("./relative-path")</c> helper for loading Unity assets
    /// by path from the UITKX asset registry. Used in .uitkx attribute expressions.
    /// </summary>
    public static class AssetHelpers
    {
        /// <summary>
        /// Load a Unity asset by its registry key (resolved asset path without extension).
        /// Returns null if the key is not found or the asset type doesn't match.
        /// </summary>
        /// <typeparam name="T">The Unity asset type (Texture2D, Sprite, Font, etc.)</typeparam>
        /// <param name="path">
        /// The asset path as registered. In .uitkx files, relative paths like "./avatar.png"
        /// are resolved by the source generator to absolute registry keys like
        /// "Assets/UI/avatar". At runtime, this parameter receives the resolved key.
        /// </param>
        public static T Asset<T>(string path) where T : UnityEngine.Object
        {
            return UitkxAssetRegistry.Get<T>(path);
        }

        /// <summary>Shorthand alias for <see cref="Asset{T}"/>.</summary>
        public static T Ast<T>(string path) where T : UnityEngine.Object
        {
            return UitkxAssetRegistry.Get<T>(path);
        }
    }
}
```

**Why a wrapper over `UitkxAssetRegistry.Get<T>` directly?**
- Shorter: `Asset<Texture2D>(...)` vs `UitkxAssetRegistry.Get<Texture2D>(...)`
- `using static` lets you skip the class name entirely
- Follows the `Blur()`, `Px()`, `Pct()` convention from CssHelpers
- Can be extended with overloads later (e.g., fallback parameter)

### 4.2 Path Resolution — Relative to Absolute

The `.uitkx` file contains a relative path: `Asset<Texture2D>("./avatar.png")`

The **source generator** resolves it at compile time:

```csharp
// User writes in PlayerCard.uitkx (located at Assets/UI/PlayerCard.uitkx):
Asset<Texture2D>("./avatar.png")

// Source generator knows _filePath = "Assets/UI/PlayerCard.uitkx"
// Resolves "./avatar.png" relative to "Assets/UI/"
// Emits:
Asset<Texture2D>("Assets/UI/avatar.png")
```

The registry key is the **full asset path with extension**. This avoids ambiguity
when two files share the same name but different extensions (e.g. `button.wav` vs
`button.png`).

**Path resolution rules:**
- `"./avatar.png"` → relative to `.uitkx` file directory, extension kept
- `"../shared/bg.png"` → parent directory traversal, extension kept
- `"Assets/UI/avatar.png"` → absolute, passed through unchanged

### 4.3 The Registry ScriptableObject

```csharp
// Shared/Core/UitkxAssetRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Central asset registry — maps string keys to Unity asset references.
    /// One instance lives at Resources/__uitkx_registry. Populated by Editor tooling.
    /// Supports any UnityEngine.Object subtype (StyleSheet, Texture2D, Sprite, Font, etc.)
    /// </summary>
    public sealed class UitkxAssetRegistry : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string key;              // "Assets/UI/avatar.png"
            public UnityEngine.Object asset; // Texture2D, StyleSheet, Font, etc.
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        // ── Runtime cache ────────────────────────────────────────────

        private static Dictionary<string, UnityEngine.Object> s_cache;

        /// <summary>
        /// Get a typed asset by registry key. O(1) dictionary lookup.
        /// Returns null if key not found or type mismatch.
        /// </summary>
        public static T Get<T>(string key) where T : UnityEngine.Object
        {
            if (s_cache == null) LoadCache();
            if (s_cache.TryGetValue(key, out var obj))
                return obj as T;
            return null;
        }

        /// <summary>
        /// Check if a key exists in the registry (any type).
        /// </summary>
        public static bool Contains(string key)
        {
            if (s_cache == null) LoadCache();
            return s_cache.ContainsKey(key);
        }

        private static void LoadCache()
        {
            var registry = Resources.Load<UitkxAssetRegistry>("__uitkx_registry");
            s_cache = new Dictionary<string, UnityEngine.Object>(
                registry != null ? registry.entries.Length : 0);
            if (registry != null)
            {
                foreach (var e in registry.entries)
                {
                    if (e.asset != null && !string.IsNullOrEmpty(e.key))
                        s_cache[e.key] = e.asset;
                }
            }
        }

        /// <summary>
        /// Reset cache (called on domain reload in Editor).
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            s_cache = null;
        }

        // ── Editor API (for UitkxChangeWatcher) ─────────────────────

        #if UNITY_EDITOR
        /// <summary>
        /// Add or update an entry. Editor-only.
        /// </summary>
        public void Set(string key, UnityEngine.Object asset)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].key == key)
                {
                    entries[i].asset = asset;
                    return;
                }
            }
            // Append
            var list = new List<Entry>(entries);
            list.Add(new Entry { key = key, asset = asset });
            entries = list.ToArray();
        }

        /// <summary>
        /// Remove an entry by key. Editor-only.
        /// </summary>
        public bool Remove(string key)
        {
            var list = new List<Entry>(entries);
            int removed = list.RemoveAll(e => e.key == key);
            if (removed > 0) entries = list.ToArray();
            return removed > 0;
        }

        /// <summary>
        /// Replace all entries (used during full rescan). Editor-only.
        /// </summary>
        public void ReplaceAll(Entry[] newEntries)
        {
            entries = newEntries ?? Array.Empty<Entry>();
        }

        /// <summary>All current keys (for stale entry detection). Editor-only.</summary>
        public IReadOnlyList<Entry> Entries => entries;
        #endif
    }
}
```

**Key design decisions:**
- `UnityEngine.Object` for the asset field — covers ALL asset types in one array
- `as T` cast at runtime — returns null on type mismatch (safe, never throws)
- `ResetCache()` via `[RuntimeInitializeOnLoadMethod]` — handles domain reload
- Editor-only `Set`/`Remove`/`ReplaceAll` — watcher uses these, stripped from builds
- No generics in serialized struct — Unity can't serialize `Entry<T>`

### 4.4 Runtime Error on Missing Asset

When `Get<T>(key)` finds no entry (or a type mismatch), it emits `Debug.LogError`
with the key name. A `HashSet<string> s_erroredKeys` deduplicates so the same key
only logs once per session (avoids log spam in `Render()` loops).

```csharp
private static readonly HashSet<string> s_erroredKeys = new();

public static T Get<T>(string key) where T : UnityEngine.Object
{
    if (s_cache == null) LoadCache();
    if (s_cache.TryGetValue(key, out var obj))
    {
        if (obj is T typed) return typed;
        if (s_erroredKeys.Add(key))
            Debug.LogError($"[UITKX] Asset type mismatch for \"{key}\": expected {typeof(T).Name}, got {obj.GetType().Name}");
        return null;
    }
    if (s_erroredKeys.Add(key))
        Debug.LogError($"[UITKX] Asset not found in registry: \"{key}\"");
    return null;
}
```

### 4.5 HMR Cache Injection

During HMR, assemblies are locked so the registry SO cannot be modified. Instead,
`UitkxHmrController.SyncAssetCacheForHmr()` parses the `.uitkx` file for `Asset<T>()`
and `@uss` references, resolves paths, loads assets via `AssetDatabase`, and injects
them into the static cache via `UitkxAssetRegistry.InjectCacheEntry(key, asset)`.

This runs **before** the delegate swap so the new `Render()` body sees updated assets.

```csharp
// UitkxAssetRegistry.cs — editor-only static method
public static void InjectCacheEntry(string key, UnityEngine.Object asset)
{
    if (s_cache == null) LoadCache();
    s_cache[key] = asset;
    s_erroredKeys.Remove(key); // clear error state if previously missing
}
```

### 4.6 LSP Diagnostic — UITKX0120 Asset Not Found

The `DiagnosticsAnalyzer.CheckAssetPaths()` method scans the source text for
`Asset<T>("path")`, `Ast<T>("path")`, and `@uss "path"` patterns. For each match,
it resolves relative paths and checks `File.Exists()`. If the file doesn't exist,
it emits a `ParseDiagnostic` with code `UITKX0120`, severity **Error** (T2), and
the message includes the resolved path.

This gives immediate red-line feedback in the IDE when an asset path is wrong.

---

## 5. Source Generator Integration

### 5.1 Detecting `Asset<T>()` Calls in Expressions

The source generator doesn't need to parse C# expressions deeply. It uses a simple
regex to find `Asset<...>("...")` patterns and extract the relative path:

```csharp
// In CSharpEmitter — path resolution pass
private static readonly Regex s_assetCallRe = new(
    @"(?:Asset|Ast)\s*<\s*\w+\s*>\s*\(\s*""([^""]+)""\s*\)",
    RegexOptions.Compiled);

private string ResolveAssetPaths(string expression, string uitkxDir)
{
    return s_assetCallRe.Replace(expression, match =>
    {
        string rawPath = match.Groups[1].Value;
        if (!rawPath.StartsWith("./") && !rawPath.StartsWith("../"))
            return match.Value; // absolute — pass through unchanged
        string resolved = ResolvePath(uitkxDir, rawPath);
        return match.Value.Replace(match.Groups[1].Value, resolved);
    });
}
```

Input: `Asset<Texture2D>("./avatar.png")`  
Output: `Asset<Texture2D>("Assets/UI/avatar.png")`

### 5.2 Auto-Injecting the Using

When the emitter detects any `Asset<` pattern in expressions, it ensures:

```csharp
using static ReactiveUITK.AssetHelpers;
```

is included in the generated file (merged with user's `@using` directives).

### 5.3 `@uss` Directive Emission

`@uss "./PlayerCard.uss"` becomes:

```csharp
internal static readonly string[] __uitkx_ussKeys = new[]
{
    "Assets/UI/PlayerCard.uss"  // resolved, extension kept
};
```

And in `Render()`:
```csharp
{ "__ussKeys", __uitkx_ussKeys }
```

The PropsApplier calls `UitkxAssetRegistry.Get<StyleSheet>(key)` — same registry,
same cache, same mechanism.

---

## 6. Editor Tooling — Registry Sync

### 6.1 UitkxChangeWatcher Extension

On `.uitkx` save, the watcher:

1. **Reads the file content**
2. **Extracts all asset references:**
   - `@uss` directive paths → `StyleSheet` type
   - `Asset<T>("path")` expressions → extract path + type name
3. **Resolves relative paths** to absolute asset paths
4. **Loads each asset** via `AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)`
5. **Updates registry SO:**
   ```csharp
   var registry = GetOrCreateRegistry();
   registry.Set("Assets/UI/avatar.png", texture2dAsset);
   registry.Set("Assets/UI/PlayerCard.uss", styleSheetAsset);
   EditorUtility.SetDirty(registry);
   AssetDatabase.SaveAssets();
   ```

### 6.2 Full Rescan on Domain Reload

`[InitializeOnLoadMethod]` triggers a full rescan:

1. Find all `.uitkx` files: `AssetDatabase.FindAssets("t:DefaultAsset", ...)` filtered by `.uitkx`
2. For each file, extract all `@uss` and `Asset<>()` references
3. Resolve all paths, load all assets
4. `registry.ReplaceAll(allEntries)` — atomic rebuild
5. Remove stale entries (assets that no longer exist, `.uitkx` files deleted)

### 6.3 Registry Creation

If `Resources/__uitkx_registry.asset` doesn't exist, the watcher auto-creates it:

```csharp
static UitkxAssetRegistry GetOrCreateRegistry()
{
    const string path = "Assets/.../Resources/__uitkx_registry.asset";
    var registry = AssetDatabase.LoadAssetAtPath<UitkxAssetRegistry>(path);
    if (registry == null)
    {
        // Ensure Resources/ folder exists
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        registry = ScriptableObject.CreateInstance<UitkxAssetRegistry>();
        AssetDatabase.CreateAsset(registry, path);
    }
    return registry;
}
```

---

## 7. Type Safety Matrix

| Context | Expression | Return Type | Compiler Check |
|---------|-----------|-------------|----------------|
| `<Image texture={...}>` | `Asset<Texture2D>("./a.png")` | `Texture2D` | ✅ `ImageProps.Texture : Texture2D` |
| `<Image sprite={...}>` | `Asset<Sprite>("./a.png")` | `Sprite` | ✅ `ImageProps.Sprite : Sprite` |
| `<Image texture={...}>` | `Asset<Sprite>("./a.png")` | `Sprite` | ❌ **Compile error** — Sprite not assignable to Texture2D |
| `style BackgroundImage` | `Asset<Texture2D>("./bg.png")` | `Texture2D` | ✅ `Style.BackgroundImage : Texture2D` |
| `style FontFamily` | `Asset<Font>("./f.ttf")` | `Font` | ✅ `Style.FontFamily : Font` |
| `@uss "./x.uss"` | (automatic) | `StyleSheet` | ✅ registry returns StyleSheet |
| Prop on user component | `Asset<MyScriptableObject>("./d.asset")` | `MyScriptableObject` | ✅ if prop type matches |

### Wrong Type at Runtime?

If the registry has a `Texture2D` at key `"Assets/UI/avatar.png"` but someone calls
`Asset<Sprite>("Assets/UI/avatar.png")`, the `as T` cast returns `null` and a
`Debug.LogError` is emitted (once per key). The LSP could warn about type mismatches
as a future enhancement.

---

## 8. Supported Asset Types

Any `UnityEngine.Object` subtype works. Common ones for UI Toolkit:

| Type | Used For | Example |
|------|----------|---------|
| `StyleSheet` | `@uss` directive, USS loading | `@uss "./card.uss"` |
| `Texture2D` | Image texture, style backgroundImage | `Asset<Texture2D>("./bg.png")` |
| `Sprite` | Image sprite, style backgroundImage | `Asset<Sprite>("./icon.png")` |
| `Font` | Style fontFamily | `Asset<Font>("./custom.ttf")` |
| `VectorImage` | SVG backgrounds (Unity 6+) | `Asset<VectorImage>("./logo.svg")` |
| `Material` | Style unityMaterial (Unity 6.3+) | `Asset<Material>("./blur.mat")` |
| `RenderTexture` | Dynamic textures | `Asset<RenderTexture>("./rt.renderTexture")` |
| `AudioClip` | UI sound effects (via C# code) | `Asset<AudioClip>("./click.wav")` |
| `ScriptableObject` | Custom data assets | `Asset<GameConfig>("./config.asset")` |

---

## 9. Scenario Coverage

| Scenario | How It Works |
|----------|-------------|
| **Same asset in 10 components** | Same registry key → same cache entry → same object reference. One load. |
| **Asset type mismatch** | `as T` returns null. C# compiler catches most mismatches at compile time. |
| **Asset deleted** | Watcher removes stale entry on rescan. Runtime: `Get<T>` returns null. |
| **Asset moved/renamed** | Unity updates GUID in registry SO automatically. Key stays the same (path-based). |
| **New .uitkx added** | Watcher triggers on save → extracts refs → adds to registry. |
| **.uitkx deleted** | Full rescan on domain reload removes orphaned entries. |
| **Build** | Unity follows dependencies from registry SO → includes only referenced assets. |
| **HMR** | Registry cache persists (static dict). `SyncAssetCacheForHmr()` injects new entries via `InjectCacheEntry()` before delegate swap. |
| **Domain reload** | `ResetCache()` via `[RuntimeInitializeOnLoadMethod]` → lazy re-init on next `Get<T>`. |
| **Multiple registries?** | No — one registry for entire project. Keys are globally unique (full asset paths). |

---

## 10. Implementation Checklist

### ═══ DELIVERABLE 1 — Asset Registry (this plan) ═══

### Phase 1 — Runtime Foundation
```
[x] 1. Shared/Core/UitkxAssetRegistry.cs — ScriptableObject + static cache + Get<T>
[x] 2. Shared/Core/AssetHelpers.cs — Asset<T>(string) + Ast<T>(string) helpers
```

### Phase 2 — Editor Tooling
```
[x] 3. Editor/UitkxAssetRegistrySync.cs — New file: registry sync logic
       - Parse Asset<T>("path") / Ast<T>("path") expressions via regex
       - Resolve relative paths to absolute
       - Load assets via AssetDatabase
       - Update/create registry SO
[x] 4. Editor/UitkxChangeWatcher.cs — Hook into registry sync on .uitkx save
[x] 5. Editor/UitkxAssetRegistrySync.cs — Full rescan on [InitializeOnLoad]
[x] 6. Auto-create Resources/__uitkx_registry.asset if missing
```

### Phase 3 — Source Generator
```
[x] 7. SourceGenerator~/Emitter/CSharpEmitter.cs — Resolve Asset<T>("./relative") paths
       (also matches Ast<T>("./relative"))
[x] 8. SourceGenerator~/Emitter/CSharpEmitter.cs — Auto-inject using static AssetHelpers
```

### Phase 4 — Runtime Error Handling + HMR Injection + LSP Diagnostic
```
[x] 9. Shared/Core/UitkxAssetRegistry.cs — Debug.LogError on missing key (per-key dedup via s_erroredKeys HashSet)
[x] 10. Shared/Core/UitkxAssetRegistry.cs — InjectCacheEntry(key, asset) static method for HMR
[x] 11. Editor/HMR/UitkxHmrController.cs — SyncAssetCacheForHmr() parses .uitkx, resolves paths, injects cache entries before delegate swap
[x] 12. ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs — UITKX0120 AssetNotFound
[x] 13. ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs — CheckAssetPaths() validates File.Exists for Asset/Ast/@uss paths
[x] 14. ide-extensions~/lsp-server/DiagnosticsPublisher.cs — Pass sourceText to Analyze()
```

### Phase 5 — Tests
```
[x] 15. SourceGenerator~/Tests/EmitterTests.cs — 7 asset path resolution tests (extensions kept)
[x] 16. SourceGenerator~/Tests/DiagnosticsAnalyzerTests.cs — 5 UITKX0120 diagnostic tests
[ ] 17. Tests — Registry SO serialization round-trip
```

### ═══ DELIVERABLE 2 — USS Loading (see USS_LOADING_PLAN.md) ═══

### Phase 6 — USS Directive + Runtime
```
[ ] 18. language-lib/Parser/DirectiveParser.cs — Parse @uss "path"
[ ] 19. language-lib/Parser/ParseResult.cs — Add UssSheets to DirectiveSet
[ ] 20. SourceGenerator~/Emitter/CSharpEmitter.cs — Emit __uitkx_ussKeys from @uss
[ ] 21. PropsApplier.cs — Handle __ussKeys (load StyleSheet from registry, add to detached element)
```

### Phase 7 — IDE Support
```
[ ] 22. LSP CompletionHandler — Path completion inside Asset<T>("...") and @uss "..."
[ ] 23. uitkx-schema.json — Document @uss directive
[ ] 24. LSP SemanticTokens — Highlight @uss and asset paths
```

### Phase 8 — Polish (Deliverable 2)
```
[ ] 25. .gitignore — Policy for Resources/__uitkx_registry.asset
[ ] 26. Documentation — Asset loading + USS loading pages in docs site
[ ] 27. Samples — Example using @uss + Asset<Texture2D> + Asset<Sprite>
[ ] 28. Tests — USS loading runtime tests
```

---

## 11. Files to Create/Modify

| File | Action | Phase |
|------|--------|-------|
| `Shared/Core/UitkxAssetRegistry.cs` | **Create** — SO + cache + Get<T> + Debug.LogError + InjectCacheEntry | 1, 4 |
| `Shared/Core/AssetHelpers.cs` | **Create** — Asset<T> + Ast<T> wrappers | 1 |
| `Editor/UitkxAssetRegistrySync.cs` | **Create** — registry sync logic | 2 |
| `Editor/UitkxChangeWatcher.cs` | Modify — hook sync | 2 |
| `Editor/HMR/UitkxHmrController.cs` | Modify — SyncAssetCacheForHmr + InjectCacheEntry calls | 4 |
| `Editor/HMR/HmrCSharpEmitter.cs` | Modify — path resolution (mirrors CSharpEmitter) | 3 |
| `SourceGenerator~/Emitter/CSharpEmitter.cs` | Modify — path resolution + using | 3 |
| `SourceGenerator~/Tests/EmitterTests.cs` | **Create** — 7 asset emitter tests | 5 |
| `SourceGenerator~/Tests/DiagnosticsAnalyzerTests.cs` | Modify — 5 UITKX0120 tests | 5 |
| `ide-extensions~/language-lib/Diagnostics/DiagnosticCodes.cs` | Modify — UITKX0120 | 4 |
| `ide-extensions~/language-lib/Diagnostics/DiagnosticsAnalyzer.cs` | Modify — CheckAssetPaths | 4 |
| `ide-extensions~/lsp-server/DiagnosticsPublisher.cs` | Modify — pass sourceText | 4 |
| `Shared/Props/PropsApplier.cs` | Modify — `__ussKeys` handling | 6 |
| `ide-extensions~/language-lib/Parser/DirectiveParser.cs` | Modify — `@uss` | 6 |
| `ide-extensions~/language-lib/Parser/ParseResult.cs` | Modify — `UssSheets` | 6 |
| `ide-extensions~/grammar/uitkx-schema.json` | Modify — `@uss` docs | 7 |
| `ide-extensions~/lsp-server/CompletionHandler.cs` | Modify — path completion | 7 |

---

## 12. Relationship to USS_LOADING_PLAN.md

The USS Loading Plan depends on this Asset Registry as its foundation:
- `@uss` directives use `UitkxAssetRegistry.Get<StyleSheet>(key)` at runtime
- USS paths are resolved by the same source generator path resolution
- The watcher sync covers both `@uss` and `Asset<T>()` references
- Phase 1 here (registry + AssetHelpers) must be completed before USS Phase 1

```
ASSET_REGISTRY_PLAN (this file)     USS_LOADING_PLAN
  Phase 1: Registry SO + AssetHelpers ──→ Phase 1: PropsApplier __ussKeys
  Phase 2: Editor sync               ──→ Phase 2: Watcher @uss detection
  Phase 3: Source gen path resolution ──→ Phase 3: @uss emission
  Phase 4: @uss directive parsing     ──→ (shared)
  Phase 5: IDE support                ──→ Phase 4: @uss IDE support
```

---

## 13. Future Extensions (v2+)

- **Addressables backend** — alternative to `Resources.Load` for the registry SO
- **Async loading** — `AssetAsync<T>("path")` returning a coroutine/task handle
- **Fallback assets** — `Asset<Texture2D>("./missing.png", fallback: defaultTex)`
- **Hot-reload assets** — detect asset reimport and invalidate cache entries
- **Asset bundles** — registry entries pointing to bundled assets
- **LSP type inference** — warn when `Asset<Sprite>` is used where `Texture2D` expected
