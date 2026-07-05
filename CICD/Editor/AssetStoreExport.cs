using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ReactiveUITK.CICD
{
    /// <summary>
    /// Headless Asset Store package export (Tier A of Plans~/ASSET_STORE_PUBLISHING_PLAN.md).
    ///
    /// Runs inside a SHELL project whose Assets/ReactiveUIToolKit holds the store-shaped
    /// content (the dist omit-list applied, Samples kept visible, plus CICD/ so this method
    /// exists to be called):
    ///
    ///   Unity -batchmode -nographics -quit -projectPath &lt;shell&gt;
    ///         -executeMethod ReactiveUITK.CICD.AssetStoreExport.Run
    ///         [-exportOut &lt;absolute .unitypackage path&gt;]
    ///
    /// CICD/ itself never ships: it is excluded from the collected asset list, and the
    /// guard rails below turn any packaging surprise into a red CI run instead of a store
    /// rejection two review-days later. Script compile errors abort batchmode before this
    /// method runs, so a package that does not compile on the floor Unity version can never
    /// export — that IS the validation the store reviewers apply first.
    /// </summary>
    internal static class AssetStoreExport
    {
        private const string PackageRoot = "Assets/ReactiveUIToolKit";

        public static void Run()
        {
            try
            {
                string outPath = ArgAfter("-exportOut");
                if (string.IsNullOrEmpty(outPath))
                {
                    outPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "ReactiveUIToolKit.unitypackage"
                    );
                }

                var paths = AssetDatabase
                    .FindAssets(string.Empty, new[] { PackageRoot })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Distinct()
                    .Where(p =>
                        !p.StartsWith(PackageRoot + "/CICD", StringComparison.OrdinalIgnoreCase)
                    )
                    .OrderBy(p => p, StringComparer.Ordinal)
                    .ToArray();

                if (paths.Length == 0)
                {
                    Fail("no assets found under " + PackageRoot);
                    return;
                }

                // Must-ship guard rails: a store install is broken without these.
                RequirePrefix(paths, PackageRoot + "/Runtime");
                RequirePrefix(paths, PackageRoot + "/Shared");
                RequirePrefix(paths, PackageRoot + "/Editor");
                RequirePrefix(paths, PackageRoot + "/Analyzers");

                // Must-NOT-ship guard rails.
                if (paths.Any(p => p.Contains("publisher-secrets")))
                {
                    Fail("publisher-secrets leaked into the export set");
                    return;
                }
                if (paths.Any(p => p.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)))
                {
                    Fail("a .pdb leaked into the export set (omit-list not applied?)");
                    return;
                }
                if (paths.Any(p => p.EndsWith("/CLAUDE.md", StringComparison.OrdinalIgnoreCase)))
                {
                    Fail("repo-internal files leaked into the export set (pathsToOmitFromStore not applied?)");
                    return;
                }

                string outDir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                AssetDatabase.ExportPackage(paths, outPath, ExportPackageOptions.Default);

                if (!File.Exists(outPath))
                {
                    Fail("ExportPackage produced no file at " + outPath);
                    return;
                }

                long size = new FileInfo(outPath).Length;
                Debug.Log(
                    $"[AssetStoreExport] exported {paths.Length} assets ({size / 1024} KB) -> {outPath}"
                );
                Debug.Log("[AssetStoreExport] OK");
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetStoreExport] FAILED: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void RequirePrefix(string[] paths, string prefix)
        {
            if (!paths.Any(p => p.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase)))
            {
                Fail("required content missing from export set: " + prefix);
            }
        }

        private static void Fail(string message)
        {
            Debug.LogError("[AssetStoreExport] FAILED: " + message);
            EditorApplication.Exit(1);
        }

        private static string ArgAfter(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
