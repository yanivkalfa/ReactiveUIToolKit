#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ReactiveUITK.Core;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.Editor
{
    /// <summary>
    /// Populates the <see cref="UitkxAssetRegistry"/> ScriptableObject by scanning
    /// <c>.uitkx</c> files for <c>Asset&lt;T&gt;("path")</c>, <c>Ast&lt;T&gt;("path")</c>
    /// and <c>@uss "path"</c> references.
    ///
    /// <list type="bullet">
    ///   <item><b>Domain reload</b> — full rescan of all .uitkx files.</item>
    ///   <item><b>On save</b> — incremental update for changed files
    ///     (called by <see cref="UitkxChangeWatcher"/>).</item>
    /// </list>
    /// </summary>
    [InitializeOnLoad]
    internal static class UitkxAssetRegistrySync
    {
        private const string RegistryFolder = "Assets/ReactiveUITK/Resources";
        private const string RegistryAssetPath = RegistryFolder + "/__uitkx_registry.asset";

        private static readonly Regex s_assetCallRe = new(
            @"(?:Asset|Ast)\s*<\s*(\w+)\s*>\s*\(\s*""([^""]+)""\s*\)",
            RegexOptions.Compiled);

        private static readonly Regex s_ussDirectiveRe = new(
            @"@uss\s+""([^""]+)""",
            RegexOptions.Compiled);

        static UitkxAssetRegistrySync()
        {
            EditorApplication.delayCall += FullRescan;
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Incremental sync: update registry entries for changed <c>.uitkx</c> files.
        /// Called by <see cref="UitkxChangeWatcher"/> on asset import.
        /// </summary>
        public static void SyncChangedFiles(string[] assetPaths)
        {
            bool anyUitkx = false;
            foreach (var p in assetPaths)
            {
                if (p.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                {
                    anyUitkx = true;
                    break;
                }
            }
            if (!anyUitkx) return;

            var registry = GetOrCreateRegistry();
            if (registry == null) return;

            bool dirty = false;
            string projectRoot = GetProjectRoot();

            foreach (var assetPath in assetPaths)
            {
                if (!assetPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                    continue;

                string absPath = Path.Combine(projectRoot, assetPath);
                if (!File.Exists(absPath)) continue;

                string content = File.ReadAllText(absPath);
                string uitkxDir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                var refs = ExtractAssetReferences(content, uitkxDir);

                foreach (var (key, resolvedAssetPath) in refs)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                        resolvedAssetPath);
                    if (asset != null)
                    {
                        registry.Set(key, asset);
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssetIfDirty(registry);
            }
        }

        /// <summary>
        /// Full atomic rebuild of the registry from all <c>.uitkx</c> files
        /// under <c>Assets/</c>.
        /// </summary>
        public static void FullRescan()
        {
            string dataPath = Application.dataPath; // …/Assets
            string[] uitkxFiles;
            try
            {
                uitkxFiles = Directory.GetFiles(dataPath, "*.uitkx", SearchOption.AllDirectories);
            }
            catch (Exception)
            {
                return; // folder not accessible (rare)
            }

            if (uitkxFiles.Length == 0)
            {
                ClearRegistryIfExists();
                return;
            }

            var allEntries = new Dictionary<string, UnityEngine.Object>();

            foreach (string absPath in uitkxFiles)
            {
                string assetPath = "Assets" + absPath
                    .Substring(dataPath.Length)
                    .Replace('\\', '/');
                string content;
                try { content = File.ReadAllText(absPath); }
                catch { continue; }

                string uitkxDir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                var refs = ExtractAssetReferences(content, uitkxDir);

                foreach (var (key, resolvedAssetPath) in refs)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                        resolvedAssetPath);
                    if (asset != null)
                        allEntries[key] = asset;
                }
            }

            if (allEntries.Count == 0)
            {
                ClearRegistryIfExists();
                return;
            }

            var registry = GetOrCreateRegistry();
            if (registry == null) return;

            var entries = new UitkxAssetRegistry.Entry[allEntries.Count];
            int i = 0;
            foreach (var kvp in allEntries)
                entries[i++] = new UitkxAssetRegistry.Entry { key = kvp.Key, asset = kvp.Value };

            registry.ReplaceAll(entries);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssetIfDirty(registry);
        }

        // ── Path parsing ─────────────────────────────────────────────

        private static List<(string key, string assetPath)> ExtractAssetReferences(
            string content, string uitkxDir)
        {
            var result = new List<(string, string)>();

            foreach (Match m in s_ussDirectiveRe.Matches(content))
            {
                string rawPath = m.Groups[1].Value;
                string resolved = ResolvePath(uitkxDir, rawPath);
                result.Add((resolved, resolved));
            }

            foreach (Match m in s_assetCallRe.Matches(content))
            {
                string rawPath = m.Groups[2].Value;
                string resolved = ResolvePath(uitkxDir, rawPath);
                result.Add((resolved, resolved));
            }

            return result;
        }

        private static string ResolvePath(string uitkxDir, string rawPath)
        {
            if (rawPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                rawPath.StartsWith("Packages/", StringComparison.Ordinal))
                return rawPath;

            string combined = uitkxDir + "/" + rawPath;
            return NormalizePath(combined);
        }

        private static string NormalizePath(string path)
        {
            var parts = path.Replace('\\', '/').Split('/');
            var stack = new List<string>();
            foreach (var p in parts)
            {
                if (p == "." || p == "") continue;
                if (p == ".." && stack.Count > 0)
                    stack.RemoveAt(stack.Count - 1);
                else if (p != "..")
                    stack.Add(p);
            }
            return string.Join("/", stack);
        }

        // ── Registry SO management ───────────────────────────────────

        private static UitkxAssetRegistry GetOrCreateRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<UitkxAssetRegistry>(RegistryAssetPath);
            if (registry != null) return registry;

            string absFolder = Path.Combine(GetProjectRoot(), RegistryFolder.Replace('/', '\\'));
            if (!Directory.Exists(absFolder))
            {
                Directory.CreateDirectory(absFolder);
                AssetDatabase.Refresh();
            }

            registry = ScriptableObject.CreateInstance<UitkxAssetRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryAssetPath);
            AssetDatabase.SaveAssets();
            return registry;
        }

        private static void ClearRegistryIfExists()
        {
            var registry = AssetDatabase.LoadAssetAtPath<UitkxAssetRegistry>(RegistryAssetPath);
            if (registry != null && registry.Entries.Count > 0)
            {
                registry.ReplaceAll(Array.Empty<UitkxAssetRegistry.Entry>());
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssetIfDirty(registry);
            }
        }

        private static string GetProjectRoot()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }
    }
}
#endif
