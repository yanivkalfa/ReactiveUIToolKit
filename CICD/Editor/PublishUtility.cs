using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.CICD
{
    internal static class PublishUtility
    {
        [Serializable]
        private sealed class EnvVars
        {
            public string env;
            public string traceLevel;
            public bool diffTracing;
        }

        [Serializable]
        private sealed class ConfigModel
        {
            public EnvVars envVariables;
            public List<string> pathsToOmitFromDist;
        }

        [MenuItem("Window/ReactiveUITK/Publish/Build Dist", priority = 1000)]
        public static void BuildDist()
        {
            try
            {
                // Build dist inside the package repo root (Assets/ReactiveUIToolKit/dist)
                string packageRoot = Path.Combine(Application.dataPath, "ReactiveUIToolKit");
                string distRoot = Path.Combine(packageRoot, "dist~");

                if (!Directory.Exists(packageRoot))
                {
                    Debug.LogError("Publish: package root not found: " + packageRoot);
                    return;
                }

                if (Directory.Exists(distRoot))
                {
                    DeleteDirectory(distRoot);
                }
                Directory.CreateDirectory(distRoot);

                // Copy everything from packageRoot to dist
                CopyDirectory(packageRoot, distRoot);

                // Load config.json if present
                string cfgPath = Path.Combine(packageRoot, "config.json");
                ConfigModel cfg = null;
                if (File.Exists(cfgPath))
                {
                    try { cfg = JsonUtility.FromJson<ConfigModel>(File.ReadAllText(cfgPath)); }
                    catch (Exception ex) { Debug.LogWarning("Publish: failed reading config.json: " + ex.Message); }
                }

                // Prune per config
                if (cfg != null && cfg.pathsToOmitFromDist != null)
                {
                    foreach (var raw in cfg.pathsToOmitFromDist)
                    {
                        if (string.IsNullOrWhiteSpace(raw)) continue;
                        string pattern = raw.Replace('\\', '/').Trim();
                        bool recursive = pattern.EndsWith("/**", StringComparison.Ordinal);
                        string basePath = recursive ? pattern.Substring(0, pattern.Length - 3) : pattern;
                        basePath = basePath.TrimEnd('/');

                        // Resolve absolute under dist
                        string abs = Path.Combine(distRoot, basePath.Replace('/', Path.DirectorySeparatorChar));

                        if (recursive)
                        {
                            if (Directory.Exists(abs))
                            {
                                DeleteDirectory(abs);
                                continue;
                            }
                            // If base is a glob into subdirs, remove any item starting with base
                            DeleteAllStartingWith(distRoot, basePath);
                            continue;
                        }

                        if (Directory.Exists(abs))
                        {
                            DeleteDirectory(abs);
                            continue;
                        }
                        if (File.Exists(abs))
                        {
                            TryDeleteFile(abs);
                            // also try delete matching .meta
                            TryDeleteFile(abs + ".meta");
                        }
                    }
                }

                // Rename Samples -> Samples~ if needed
                string samples = Path.Combine(distRoot, "Samples");
                string samplesTilde = Path.Combine(distRoot, "Samples~");
                if (Directory.Exists(samples))
                {
                    if (Directory.Exists(samplesTilde)) DeleteDirectory(samplesTilde);
                    Directory.Move(samples, samplesTilde);
                    // move meta if present
                    TryMoveMeta(samples, samplesTilde);
                }

                // Sanity check
                string pkgJson = Path.Combine(distRoot, "package.json");
                if (!File.Exists(pkgJson))
                {
                    Debug.LogWarning("Publish: dist/package.json missing. Did copy fail?");
                }

                Debug.Log("[Publish] dist built at: " + distRoot);
            }
            catch (Exception ex)
            {
                Debug.LogError("Publish: Build Dist failed: " + ex);
            }
        }

        [MenuItem("Window/ReactiveUITK/Publish/Build Dist and Push", priority = 1001)]
        public static void BuildDistAndPush()
        {
            BuildDist();
            try
            {
                string packageRoot = Path.Combine(Application.dataPath, "ReactiveUIToolKit");
                string distRoot = Path.Combine(packageRoot, "dist~");
                string pkgJsonPath = Path.Combine(distRoot, "package.json");
                string tag = null;
                if (File.Exists(pkgJsonPath))
                {
                    var pkg = JsonUtility.FromJson<PackageJson>(File.ReadAllText(pkgJsonPath));
                    if (pkg != null && !string.IsNullOrEmpty(pkg.version))
                    {
                        tag = "v" + pkg.version;
                    }
                }
                string script = Path.Combine(Application.dataPath, "ReactiveUIToolKit", "CICD", "release-dist.ps1");
                if (!File.Exists(script))
                {
                    Debug.LogError("Publish: release-dist.ps1 not found at: " + script);
                    return;
                }
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{script}\" -Branch dist -Remote origin" + (string.IsNullOrEmpty(tag) ? "" : ($" -Tag {tag}")),
                    WorkingDirectory = packageRoot,
                    UseShellExecute = false,
                };
                var p = System.Diagnostics.Process.Start(psi);
                Debug.Log("Publish: invoked release-dist.ps1 (check terminal for prompts/output)");
            }
            catch (Exception ex)
            {
                Debug.LogError("Publish: Build and Push failed: " + ex.Message);
            }
        }

        [MenuItem("Window/ReactiveUITK/Publish/Build Dist and Push to Store (stub)", priority = 1002)]
        public static void BuildDistAndPushToStore()
        {
            BuildDist();
            Debug.Log("Publish: Store upload not implemented yet. Dist built.");
        }

        // ===== helpers =====

        [Serializable]
        private class PackageJson { public string version; }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            foreach (string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = dir.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(destDir, rel));
            }
            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string target = Path.Combine(destDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, overwrite: true);
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            File.SetAttributes(path, FileAttributes.Normal);
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                TryDeleteFile(file);
            }
            foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories).OrderByDescending(p => p.Length))
            {
                TryDeleteDir(dir);
            }
            TryDeleteDir(path);
        }

        private static void TryDeleteFile(string p)
        {
            try { if (File.Exists(p)) File.SetAttributes(p, FileAttributes.Normal); File.Delete(p); } catch { }
        }
        private static void TryDeleteDir(string p)
        {
            try { if (Directory.Exists(p)) Directory.Delete(p, recursive: false); } catch { }
        }

        private static void DeleteAllStartingWith(string distRoot, string relBaseForward)
        {
            string normalized = relBaseForward.Trim('/');
            foreach (var path in Directory.GetFileSystemEntries(distRoot, "*", SearchOption.AllDirectories))
            {
                string rel = path.Substring(distRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
                if (rel.StartsWith(normalized + "/", StringComparison.Ordinal) || rel.Equals(normalized, StringComparison.Ordinal))
                {
                    if (Directory.Exists(path)) DeleteDirectory(path); else TryDeleteFile(path);
                }
            }
        }

        private static void TryMoveMeta(string fromDir, string toDir)
        {
            try
            {
                string metaFrom = fromDir.TrimEnd(Path.DirectorySeparatorChar) + ".meta";
                string metaTo = toDir.TrimEnd(Path.DirectorySeparatorChar) + ".meta";
                if (File.Exists(metaFrom))
                {
                    if (File.Exists(metaTo)) File.Delete(metaTo);
                    File.Move(metaFrom, metaTo);
                }
            }
            catch { }
        }
    }
}
