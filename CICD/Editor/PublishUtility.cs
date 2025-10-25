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
                // Ensure dist does not contain VCS metadata
                var distGit = Path.Combine(distRoot, ".git");
                if (Directory.Exists(distGit)) { DeleteDirectory(distGit); }

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
                if (!Directory.Exists(distRoot))
                {
                    Debug.LogError("Publish: dist~ not found. Build step did not complete.");
                    return;
                }

                // Resolve repo root via git
                var gitTop = RunGit("rev-parse --show-toplevel", packageRoot, out string repoRoot, out string err1);
                if (gitTop != 0 || string.IsNullOrWhiteSpace(repoRoot))
                {
                    Debug.LogError("Publish: Could not determine git repo root. " + err1);
                    return;
                }
                repoRoot = repoRoot.Trim();

                // Determine tag from package.json (optional)
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

                string branch = "dist";
                string remote = "origin";
                string worktree = Path.Combine(repoRoot, "_dist_branch");

                // Ensure previous worktree is removed cleanly if it exists
                if (Directory.Exists(worktree))
                {
                    // Try to detach via git first (in case the worktree is registered)
                    RunGit($"worktree remove -f \"{worktree}\"", repoRoot, out var _, out var _eRm);
                    RunGit("worktree prune", repoRoot, out var _, out var _ePrune);
                    try { DeleteDirectory(worktree); } catch { }
                }

                // Ensure branch exists; if not, create from HEAD
                bool branchExists = RunGit($"rev-parse --verify --quiet {branch}", repoRoot, out var _, out var _eChk) == 0;

                // git worktree add -B dist <worktree> <commitish>
                string commitish = branchExists ? branch : "HEAD";
                if (RunGit($"worktree add -B {branch} \"{worktree}\" {commitish}", repoRoot, out var _, out var e2) != 0)
                {
                    Debug.LogError("Publish: git worktree add failed: " + e2);
                    return;
                }

                // Clean worktree (keep .git)
                DeleteAllExceptGit(worktree);

                // Copy dist~ into worktree
                CopyDirectory(distRoot, worktree);

                // Commit changes
                if (RunGit("add -A", worktree, out var _, out var e3) != 0)
                {
                    Debug.LogError("Publish: git add failed: " + e3);
                    return;
                }
                RunGit("status --porcelain", worktree, out var status, out var _);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    string msg = "dist update" + (string.IsNullOrEmpty(tag) ? string.Empty : (" " + tag));
                    if (RunGit($"commit -m \"{msg}\"", worktree, out var _, out var e4) != 0)
                    {
                        Debug.LogError("Publish: git commit failed: " + e4);
                        return;
                    }
                }
                else
                {
                    Debug.Log("Publish: no changes to commit on dist branch");
                }

                // Tag (optional)
                if (!string.IsNullOrEmpty(tag))
                {
                    // Create or update lightweight tag
                    RunGit($"tag -f {tag}", worktree, out var _, out var tagErr);
                    if (!string.IsNullOrEmpty(tagErr)) { Debug.Log("Publish: tag note: " + tagErr); }
                }

                // Push branch and tag
                // First push: set upstream if branch was newly created
                string pushArgs = branchExists ? $"push {remote} {branch}" : $"push -u {remote} {branch}";
                if (RunGit(pushArgs, worktree, out var _, out var e5) != 0)
                {
                    Debug.LogError("Publish: git push branch failed: " + e5);
                    return;
                }
                if (!string.IsNullOrEmpty(tag))
                {
                    RunGit($"push {remote} {tag}", worktree, out var _, out var e6);
                    if (!string.IsNullOrEmpty(e6)) { Debug.Log("Publish: push tag note: " + e6); }
                }

                // Cleanup worktree
                RunGit($"worktree remove -f \"{worktree}\"", repoRoot, out var _, out var _e7);

                Debug.Log($"[Publish] dist pushed to '{remote}/{branch}'" + (string.IsNullOrEmpty(tag) ? string.Empty : $", tag {tag}"));
                Debug.Log("[Install Hint] UPM Git URL: <repo-url>#dist");
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
                // Skip any .git directory
                var relForward = rel.Replace('\\', '/');
                if (relForward.Length == 0) continue;
                if (relForward.Equals(".git", StringComparison.OrdinalIgnoreCase)) continue;
                if (relForward.Contains("/.git/", StringComparison.Ordinal)) continue;
                Directory.CreateDirectory(Path.Combine(destDir, rel));
            }
            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var relForward = rel.Replace('\\', '/');
                if (relForward.StartsWith(".git/", StringComparison.OrdinalIgnoreCase)) continue;
                if (relForward.Contains("/.git/", StringComparison.Ordinal)) continue;
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

        private static int RunGit(string args, string workingDir, out string stdout, out string stderr)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var p = new System.Diagnostics.Process { StartInfo = psi };
            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();
            p.OutputDataReceived += (_, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
            p.ErrorDataReceived +=  (_, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            stdout = sbOut.ToString();
            stderr = sbErr.ToString();
            if (!string.IsNullOrEmpty(stdout)) Debug.Log($"[git] {args}\n{stdout}");
            if (!string.IsNullOrEmpty(stderr)) Debug.Log($"[git:err] {args}\n{stderr}");
            return p.ExitCode;
        }

        private static void DeleteAllExceptGit(string dir)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var entry in Directory.GetFileSystemEntries(dir))
            {
                var name = Path.GetFileName(entry)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase)) continue;
                if (Directory.Exists(entry)) DeleteDirectory(entry); else TryDeleteFile(entry);
            }
        }
    }
}
