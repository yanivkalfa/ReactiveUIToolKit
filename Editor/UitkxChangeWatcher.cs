#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using ReactiveUITK.EditorSupport.HMR;

namespace ReactiveUITK.Editor
{
    /// <summary>
    /// Forces Unity to recompile the assembly that OWNS a changed .uitkx file, so
    /// the source generator re-runs and picks up the new content.
    ///
    /// WHY THIS IS NEEDED
    /// ──────────────────
    /// Unity only re-runs Roslyn (and therefore source generators) when a .cs file
    /// in an assembly changes. Saving a .uitkx alone does not recompile anything, so
    /// the generator never sees the updated content.
    ///
    /// HOW IT WORKS (fast, incremental — no CleanBuildCache stall)
    /// ──────────────────────────────────────────────────────────
    /// A .uitkx file is an AdditionalFile of whatever assembly its folder's .asmdef
    /// defines (e.g. Samples/*.uitkx → ReactiveUITK.Examples; .uitkx with no ancestor
    /// .asmdef → the default Assembly-CSharp). To make THAT assembly recompile we
    /// write a tiny trigger .cs into its own folder and bump a value inside it. A real
    /// script change is exactly what a manual .cs edit does: Unity recompiles just that
    /// one assembly INCREMENTALLY (the analyzer/generator stays warm), and a genuine
    /// recompile re-reads the assembly's .uitkx files fresh from disk.
    ///
    /// This replaced two earlier approaches, both wrong:
    ///   • A single trigger .cs in Assembly-CSharp (the shipped behavior) dirties only
    ///     Assembly-CSharp — never the asmdef assembly a component actually lives in —
    ///     so asmdef-owned .uitkx edits produced STALE generated output until a full
    ///     reimport / Library clear / any .cs edit. (This was the regression: commit
    ///     3f41aa8 removed CleanBuildCache, which had masked the problem by force-
    ///     recompiling EVERY assembly — at the cost of a 30-40s cold-analyzer stall on
    ///     HMR Stop.)
    ///   • Reimporting the owning .asmdef ASSET (ImportAsset ForceUpdate) does not
    ///     recompile the assembly — reimporting an asmdef is not a script change — so
    ///     it left output just as stale.
    ///
    /// Writing an actual .cs into the owning assembly is the only thing that reliably
    /// forces the incremental recompile, and it keeps the analyzer warm (no clean).
    ///
    /// While HMR is active it hot-swaps .uitkx edits directly, so we skip the cold
    /// recompile entirely — it would fight the assembly-reload lock and, batched at
    /// HMR Stop, was the original source of the 30-40s stall.
    ///
    /// Work is deferred via <see cref="EditorApplication.delayCall"/> so it runs
    /// outside the import callback, avoiding a recursive import loop.
    /// </summary>
    public sealed class UitkxChangeWatcher : AssetPostprocessor
    {
        private const string TriggerFileName = "UITKX_GeneratorTrigger.g.cs";

        // Folder (asset path) used for .uitkx that belong to the default assembly
        // (no ancestor .asmdef → Assembly-CSharp). Created on demand. Chosen inside
        // the consuming project's Assets/ so it works whether the package lives under
        // Assets/ (dev) or is UPM-installed under Packages/ (read-only).
        private const string DefaultAssemblyFolderAsset = "Assets/ReactiveUITK";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            // The distinct set of owning-assembly FOLDERS (asset paths) to dirty, plus
            // whether any changed .uitkx belongs to the default (Assembly-CSharp) one.
            var owningFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool anyDefaultAssembly = false;
            bool anyUitkx = false;

            foreach (var arr in new[] { importedAssets, deletedAssets, movedAssets, movedFromAssetPaths })
            {
                foreach (string p in arr)
                {
                    if (!p.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                        continue;
                    anyUitkx = true;
                    string folder = FindOwningAsmdefFolder(p);
                    if (folder != null)
                        owningFolders.Add(folder);
                    else
                        anyDefaultAssembly = true;
                }
            }

            if (!anyUitkx)
                return;

            // Defer out of the import callback (avoids recursive import).
            EditorApplication.delayCall += () => TriggerRecompile(owningFolders, anyDefaultAssembly);

            // Sync asset registry entries for changed .uitkx files.
            var imported = importedAssets;
            EditorApplication.delayCall += () => UitkxAssetRegistrySync.SyncChangedFiles(imported);
        }

        private static void TriggerRecompile(HashSet<string> owningFolders, bool anyDefaultAssembly)
        {
            // HMR is active: it hot-swaps .uitkx edits live. A cold recompile here
            // would fight the assembly-reload lock and, batched, was the original
            // 30-40s HMR-Stop stall. Let HMR own the update.
            if (UitkxHmrController.IsActive)
                return;

            if (anyDefaultAssembly)
                WriteTrigger(DefaultAssemblyFolderAsset, createIfMissing: true);

            foreach (string folder in owningFolders)
                WriteTrigger(folder, createIfMissing: false);

            // Incremental recompile of just the dirtied assemblies — NOT
            // CleanBuildCache (that clears Roslyn's cache and cold-restarts every
            // analyzer, the 30-40s stall). The trigger .cs above is a real script
            // change, which is all Unity needs to recompile the owning assembly and
            // re-read its .uitkx fresh.
            CompilationPipeline.RequestScriptCompilation();
        }

        /// <summary>
        /// Writes/updates the trigger .cs inside <paramref name="folderAssetPath"/> so
        /// the assembly that owns that folder recompiles. The bumped <c>Stamp</c> value
        /// guarantees a genuine content change every save.
        /// </summary>
        private static void WriteTrigger(string folderAssetPath, bool createIfMissing)
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string absDir = Path.Combine(projectRoot, folderAssetPath.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(absDir))
                {
                    if (!createIfMissing)
                        return; // read-only package or folder gone — nothing to do
                    Directory.CreateDirectory(absDir);
                }

                string absFile = Path.Combine(absDir, TriggerFileName);
                string content =
                    "// <auto-generated/> UITKX recompile trigger.\n"
                    + "// Rewritten by UitkxChangeWatcher on each .uitkx save to force Unity to\n"
                    + "// recompile THIS assembly so the source generator re-reads its .uitkx files.\n"
                    + "// Safe to delete; gitignored by default. Do not reference this type.\n"
                    + "namespace ReactiveUITK.Generated\n"
                    + "{\n"
                    + "    internal static class UitkxRecompileTrigger\n"
                    + "    {\n"
                    + $"        internal const long Stamp = {DateTime.UtcNow.Ticks}L;\n"
                    + "    }\n"
                    + "}\n";
                File.WriteAllText(absFile, content);

                // ImportAsset tells Unity this .cs changed, scheduling the recompile.
                AssetDatabase.ImportAsset(
                    folderAssetPath + "/" + TriggerFileName,
                    ImportAssetOptions.ForceUpdate
                );
            }
            catch
            {
                // Best effort — a failed trigger write must never break asset import.
            }
        }

        /// <summary>
        /// Returns the asset-path folder of the nearest ancestor that contains an
        /// .asmdef (i.e. the root folder of the assembly that owns
        /// <paramref name="uitkxAssetPath"/>), or <c>null</c> when the file belongs to
        /// the default assembly (no .asmdef up to the Assets root → Assembly-CSharp).
        /// Only Assets/-rooted paths are handled; a .uitkx inside a read-only UPM
        /// package returns null and falls back to the default-assembly trigger.
        /// </summary>
        private static string FindOwningAsmdefFolder(string uitkxAssetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string dir = Path.GetDirectoryName(uitkxAssetPath)?.Replace('\\', '/');

            while (
                !string.IsNullOrEmpty(dir)
                && (
                    dir.Equals("Assets", StringComparison.OrdinalIgnoreCase)
                    || dir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                string absDir = Path.Combine(projectRoot, dir.Replace('/', Path.DirectorySeparatorChar));
                if (Directory.Exists(absDir)
                    && Directory.GetFiles(absDir, "*.asmdef", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    return dir;
                }
                int slash = dir.LastIndexOf('/');
                dir = slash > 0 ? dir.Substring(0, slash) : null;
            }
            return null;
        }
    }
}
#endif
