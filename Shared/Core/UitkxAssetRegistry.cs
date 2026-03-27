using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Central asset registry — maps string keys to Unity asset references.
    /// One instance lives at <c>Resources/__uitkx_registry</c>.
    /// Populated by Editor tooling (<c>UitkxAssetRegistrySync</c>).
    /// Supports any <see cref="UnityEngine.Object"/> subtype
    /// (StyleSheet, Texture2D, Sprite, Font, etc.)
    /// </summary>
    public sealed class UitkxAssetRegistry : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string key;               // e.g. "Assets/UI/avatar.png"
            public UnityEngine.Object asset;  // Texture2D, StyleSheet, Font, etc.
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        // ── Runtime cache ────────────────────────────────────────────

        private static Dictionary<string, UnityEngine.Object> s_cache;
        private static HashSet<string> s_erroredKeys;

        /// <summary>
        /// Get a typed asset by registry key. O(1) dictionary lookup.
        /// Logs an error (once per key) if the key is not found or the type mismatches.
        /// </summary>
        public static T Get<T>(string key) where T : UnityEngine.Object
        {
            if (s_cache == null) LoadCache();
            if (s_cache != null && s_cache.TryGetValue(key, out var obj))
            {
                var typed = obj as T;
                if (typed == null && s_erroredKeys.Add(key))
                    Debug.LogError($"[UITKX] Asset type mismatch for \"{key}\": " +
                        $"expected {typeof(T).Name}, found {obj.GetType().Name}.");
                return typed;
            }
            if (s_erroredKeys.Add(key))
                Debug.LogError($"[UITKX] Asset not found in registry: \"{key}\". " +
                    "Ensure the file exists and the .uitkx referencing it has been saved.");
            return null;
        }

        /// <summary>
        /// Check whether a key exists in the registry (any type).
        /// </summary>
        public static bool Contains(string key)
        {
            if (s_cache == null) LoadCache();
            return s_cache != null && s_cache.ContainsKey(key);
        }

        private static void LoadCache()
        {
            var registry = Resources.Load<UitkxAssetRegistry>("__uitkx_registry");
            s_cache = new Dictionary<string, UnityEngine.Object>(
                registry != null ? registry.entries.Length : 0);
            s_erroredKeys = new HashSet<string>();
            if (registry != null)
            {
                foreach (var e in registry.entries)
                {
                    if (e.asset != null && !string.IsNullOrEmpty(e.key))
                        s_cache[e.key] = e.asset;
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            s_cache = null;
            s_erroredKeys = null;
        }

        // ── Editor API (for UitkxAssetRegistrySync) ─────────────────

#if UNITY_EDITOR
        /// <summary>
        /// Inject an asset directly into the runtime cache without touching the SO.
        /// Used by HMR to register new assets while assemblies are locked.
        /// </summary>
        public static void InjectCacheEntry(string key, UnityEngine.Object asset)
        {
            if (s_cache == null) LoadCache();
            if (asset != null && !string.IsNullOrEmpty(key))
            {
                s_cache[key] = asset;
                // Clear any previous error for this key so it resolves on next Get<T>
                s_erroredKeys?.Remove(key);
            }
        }

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
            var list = new List<Entry>(entries);
            list.Add(new Entry { key = key, asset = asset });
            entries = list.ToArray();
        }

        public bool Remove(string key)
        {
            var list = new List<Entry>(entries);
            int removed = list.RemoveAll(e => e.key == key);
            if (removed > 0) entries = list.ToArray();
            return removed > 0;
        }

        public void ReplaceAll(Entry[] newEntries)
        {
            entries = newEntries ?? Array.Empty<Entry>();
        }

        public IReadOnlyList<Entry> Entries => entries;
#endif
    }
}
