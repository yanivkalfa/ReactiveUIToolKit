#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace ReactiveUITK.Editor.HMR
{
    // Editor-only mirror of UitkxPipeline.IsOwnedByCompilation /
    // FindOwningAsmdefAssemblyName (SourceGenerator~/UitkxPipeline.cs L281-L340).
    //
    // Walks up from a file path looking for the nearest *.asmdef and returns its
    // "name" field. When no .asmdef is found, falls back to Unity's
    // Assembly-CSharp / Assembly-CSharp-Editor convention based on whether the
    // path contains an Editor/ segment. Identical contract to the SG; covered by
    // AsmdefResolverParityTests.
    //
    // Cache is keyed by directory (not file path) to avoid re-walking once per
    // file in a folder. Invalidated when an .asmdef is created, deleted, or
    // renamed via InvalidateAll() called from the file watcher.
    internal static class AsmdefResolver
    {
        private static readonly Regex s_asmdefNameRegex = new Regex(
            @"""name""\s*:\s*""([^""]+)""",
            RegexOptions.CultureInvariant | RegexOptions.Compiled
        );

        // Directory absolute path -> resolved owner asmdef name (or fallback).
        private static readonly ConcurrentDictionary<string, string> s_dirCache =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string OwningAsmdefName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "Assembly-CSharp";

            string dir;
            try { dir = Path.GetDirectoryName(Path.GetFullPath(filePath)); }
            catch { dir = null; }

            if (string.IsNullOrEmpty(dir))
                return IsInsideEditorFolder(filePath)
                    ? "Assembly-CSharp-Editor" : "Assembly-CSharp";

            return s_dirCache.GetOrAdd(dir, d => ResolveDirectory(d, filePath));
        }

        public static void InvalidateAll() => s_dirCache.Clear();

        private static string ResolveDirectory(string startDir, string originalFilePath)
        {
            try
            {
                string dir = startDir;
                while (!string.IsNullOrEmpty(dir))
                {
                    foreach (string asmdef in Directory.GetFiles(dir, "*.asmdef"))
                    {
                        string json = File.ReadAllText(asmdef);
                        var m = s_asmdefNameRegex.Match(json);
                        if (m.Success)
                            return m.Groups[1].Value.Trim();
                    }

                    string dirName = Path.GetFileName(dir);
                    if (string.Equals(dirName, "Assets", StringComparison.OrdinalIgnoreCase))
                        break;

                    dir = Path.GetDirectoryName(dir);
                }
            }
            catch
            {
                // Never crash on filesystem errors; fall through to convention.
            }

            return IsInsideEditorFolder(originalFilePath)
                ? "Assembly-CSharp-Editor" : "Assembly-CSharp";
        }

        private static bool IsInsideEditorFolder(string filePath)
        {
            foreach (string part in (filePath ?? string.Empty).Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(part, "Editor", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
#endif
