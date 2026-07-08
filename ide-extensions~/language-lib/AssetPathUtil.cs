using System;
using System.Collections.Generic;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// Canonical Unity asset-path resolution rule, shared by every consumer that turns a
    /// bare/relative path written in a <c>.uitkx</c> file (e.g. <c>@uss "styles.uss"</c>,
    /// <c>Asset&lt;Texture2D&gt;("icon.png")</c>) into a Unity project-relative path.
    ///
    /// The rule: a path already rooted at <c>Assets/</c> or <c>Packages/</c> is absolute and
    /// passed through unchanged; every other path (bare <c>"styles.uss"</c> or explicitly
    /// relative <c>"./styles.uss"</c> / <c>"../shared/styles.uss"</c>) is resolved relative to
    /// the directory containing the <c>.uitkx</c> file, with <c>.</c>/<c>..</c> segments
    /// collapsed.
    ///
    /// Before this existed, four independent consumers disagreed on bare-path semantics
    /// (uitkx-dir-relative vs. as-is-unresolved vs. project-root-relative) — the editor could
    /// show no error while the build emitted an unresolvable path, or HMR's USS dependency map
    /// could miss a file entirely (see FINAL_AUDIT_UITKX_FINDINGS.md, finding H-03).
    ///
    /// <c>Editor/HMR</c> cannot reference this type directly (its asmdef only references
    /// <c>ReactiveUITK.Shared</c>/<c>ReactiveUITK.Runtime</c> — the language-lib is consumed via
    /// reflection against the committed analyzer DLL, never a normal assembly reference). Its
    /// HMR-side mirror must be kept byte-for-byte identical to this algorithm; see
    /// <c>UitkxHmrController.HmrAssetPathUtil</c>.
    /// </summary>
    public static class AssetPathUtil
    {
        /// <summary>
        /// Resolves <paramref name="rawPath"/> against <paramref name="uitkxDir"/> per the rule
        /// above. <paramref name="uitkxDir"/> should be a Unity project-relative directory (e.g.
        /// <c>"Assets/UI"</c>), typically obtained from <see cref="GetAssetDir"/>.
        /// </summary>
        public static string ResolveAssetPath(string uitkxDir, string rawPath)
        {
            if (string.IsNullOrEmpty(rawPath))
                return rawPath;

            if (rawPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                rawPath.StartsWith("Packages/", StringComparison.Ordinal))
                return rawPath;

            string combined = string.IsNullOrEmpty(uitkxDir) ? rawPath : uitkxDir + "/" + rawPath;
            var parts = combined.Replace('\\', '/').Split('/');
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

        /// <summary>
        /// Extracts the Unity project-relative directory (e.g. <c>"Assets/UI"</c>) containing
        /// the file at <paramref name="filePath"/> (an absolute OS path). Returns the directory
        /// name (not ending in <c>Assets/</c>-relative-with-trailing-slash) or <c>null</c> when
        /// no <c>Assets/</c> segment is found (e.g. a test-environment temp path).
        /// </summary>
        public static string? GetAssetDir(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            string normalized = filePath.Replace('\\', '/');
            int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            if (assetsIdx < 0)
            {
                if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    int lastSlash = normalized.LastIndexOf('/');
                    return lastSlash > 0 ? normalized.Substring(0, lastSlash) : "Assets";
                }
                return null;
            }

            string assetPath = normalized.Substring(assetsIdx + 1);
            int dirSlash = assetPath.LastIndexOf('/');
            return dirSlash >= 0 ? assetPath.Substring(0, dirSlash) : "Assets";
        }

        /// <summary>
        /// Extracts the Unity project root (the folder containing <c>Assets/</c>) from an
        /// absolute file path. Returns <c>null</c> when <c>Assets/</c> is not found.
        /// </summary>
        public static string? GetProjectRoot(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            string normalized = filePath.Replace('\\', '/');
            int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            return assetsIdx >= 0 ? normalized.Substring(0, assetsIdx) : null;
        }
    }
}
