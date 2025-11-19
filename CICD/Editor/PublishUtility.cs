using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

                CopyDirectory(packageRoot, distRoot);

                var distGit = Path.Combine(distRoot, ".git");
                if (Directory.Exists(distGit))
                {
                    DeleteDirectory(distGit);
                }

                string cfgPath = Path.Combine(packageRoot, "config.json");
                ConfigModel cfg = null;
                if (File.Exists(cfgPath))
                {
                    try
                    {
                        cfg = JsonUtility.FromJson<ConfigModel>(File.ReadAllText(cfgPath));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Publish: failed reading config.json: " + ex.Message);
                    }
                }

                if (cfg != null && cfg.pathsToOmitFromDist != null)
                {
                    foreach (var raw in cfg.pathsToOmitFromDist)
                    {
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            continue;
                        }
                        string pattern = raw.Replace('\\', '/').Trim();
                        bool recursive = pattern.EndsWith("/**", StringComparison.Ordinal);
                        string basePath = recursive
                            ? pattern.Substring(0, pattern.Length - 3)
                            : pattern;
                        basePath = basePath.TrimEnd('/');

                        string abs = Path.Combine(
                            distRoot,
                            basePath.Replace('/', Path.DirectorySeparatorChar)
                        );

                        if (recursive)
                        {
                            if (Directory.Exists(abs))
                            {
                                DeleteDirectory(abs);
                                continue;
                            }

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

                            TryDeleteFile(abs + ".meta");
                        }
                    }
                }

                string samples = Path.Combine(distRoot, "Samples");
                string samplesTilde = Path.Combine(distRoot, "Samples~");
                if (Directory.Exists(samples))
                {
                    if (Directory.Exists(samplesTilde))
                    {
                        DeleteDirectory(samplesTilde);
                    }
                    Directory.Move(samples, samplesTilde);

                    TryMoveMeta(samples, samplesTilde);
                }

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
            try
            {
                string packageRoot = Path.Combine(Application.dataPath, "ReactiveUIToolKit");

                string rootPkg = Path.Combine(packageRoot, "package.json");
                string bumpedVersion = BumpPatchVersion(rootPkg);
                if (!string.IsNullOrEmpty(bumpedVersion))
                {
                    Debug.Log("[Publish] bumped version to " + bumpedVersion);
                    AssetDatabase.Refresh();
                }

                BuildDist();

                string distRoot = Path.Combine(packageRoot, "dist~");
                if (!Directory.Exists(distRoot))
                {
                    Debug.LogError("Publish: dist~ not found. Build step did not complete.");
                    return;
                }

                var gitTop = RunGit(
                    "rev-parse --show-toplevel",
                    packageRoot,
                    out string repoRoot,
                    out string err1
                );
                if (gitTop != 0 || string.IsNullOrWhiteSpace(repoRoot))
                {
                    Debug.LogError("Publish: Could not determine git repo root. " + err1);
                    return;
                }
                repoRoot = repoRoot.Trim();

                string tag = string.IsNullOrEmpty(bumpedVersion) ? null : ("v" + bumpedVersion);

                string branch = "dist";
                string remote = "origin";
                string worktree = Path.Combine(repoRoot, "_dist_branch");

                if (Directory.Exists(worktree))
                {
                    RunGit($"worktree remove -f \"{worktree}\"", repoRoot, out var _, out var _eRm);
                    RunGit("worktree prune", repoRoot, out var _, out var _ePrune);
                    try
                    {
                        DeleteDirectory(worktree);
                    }
                    catch { }
                }

                RunGit("fetch --tags --prune origin", repoRoot, out var _fOut, out var fErr);
                if (!string.IsNullOrEmpty(fErr))
                {
                    Debug.Log("Publish: fetch note: " + fErr);
                }

                bool remoteBranchExists =
                    RunGit(
                        $"ls-remote --heads {remote} {branch}",
                        repoRoot,
                        out var lsOut,
                        out var lsErr
                    ) == 0
                    && !string.IsNullOrWhiteSpace(lsOut);

                bool branchExists =
                    RunGit(
                        $"rev-parse --verify --quiet {branch}",
                        repoRoot,
                        out var _vOut,
                        out var _eChk
                    ) == 0;

                string commitish = remoteBranchExists
                    ? $"{remote}/{branch}"
                    : (branchExists ? branch : "HEAD");
                if (
                    RunGit(
                        $"worktree add -B {branch} \"{worktree}\" {commitish}",
                        repoRoot,
                        out var _,
                        out var e2
                    ) != 0
                )
                {
                    Debug.LogError("Publish: git worktree add failed: " + e2);
                    return;
                }

                DeleteAllExceptGit(worktree);

                CopyDirectory(distRoot, worktree);

                if (RunGit("add -A", worktree, out var _, out var e3) != 0)
                {
                    Debug.LogError("Publish: git add failed: " + e3);
                    return;
                }
                RunGit("status --porcelain", worktree, out var status, out var _);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    string msg =
                        "dist update" + (string.IsNullOrEmpty(tag) ? string.Empty : (" " + tag));
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

                if (!string.IsNullOrEmpty(tag))
                {
                    RunGit($"tag -f {tag}", worktree, out var _, out var tagErr);
                    if (!string.IsNullOrEmpty(tagErr))
                    {
                        Debug.Log("Publish: tag note: " + tagErr);
                    }
                }

                string pushArgs = branchExists
                    ? $"push {remote} {branch}"
                    : $"push -u {remote} {branch}";
                if (RunGit(pushArgs, worktree, out var _, out var e5) != 0)
                {
                    Debug.LogError("Publish: git push branch failed: " + e5);
                    return;
                }
                if (!string.IsNullOrEmpty(tag))
                {
                    RunGit($"push {remote} {tag}", worktree, out var _, out var e6);
                    if (!string.IsNullOrEmpty(e6))
                    {
                        Debug.Log("Publish: push tag note: " + e6);
                    }
                }

                RunGit($"worktree remove -f \"{worktree}\"", repoRoot, out var _, out var _e7);

                Debug.Log(
                    $"[Publish] dist pushed to '{remote}/{branch}'"
                        + (string.IsNullOrEmpty(tag) ? string.Empty : $", tag {tag}")
                );
                Debug.Log("[Install Hint] UPM Git URL: <repo-url>#dist");
            }
            catch (Exception ex)
            {
                Debug.LogError("Publish: Build and Push failed: " + ex.Message);
            }
        }

        [MenuItem(
            "Window/ReactiveUITK/Publish/Build Dist and Push to Store (stub)",
            priority = 1002
        )]
        public static void BuildDistAndPushToStore()
        {
            BuildDist();
            Debug.Log("Publish: Store upload not implemented yet. Dist built.");
        }

        [Serializable]
        private class PackageJson
        {
            public string version;
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            foreach (
                string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories)
            )
            {
                string rel = dir.Substring(sourceDir.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                var relForward = rel.Replace('\\', '/');
                if (relForward.Length == 0)
                {
                    continue;
                }
                if (relForward.Equals(".git", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (relForward.Contains("/.git/", StringComparison.Ordinal))
                {
                    continue;
                }
                Directory.CreateDirectory(Path.Combine(destDir, rel));
            }
            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = file.Substring(sourceDir.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var relForward = rel.Replace('\\', '/');
                if (relForward.StartsWith(".git/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (relForward.Contains("/.git/", StringComparison.Ordinal))
                {
                    continue;
                }
                string target = Path.Combine(destDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, overwrite: true);
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            File.SetAttributes(path, FileAttributes.Normal);
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                TryDeleteFile(file);
            }
            foreach (
                var dir in Directory
                    .GetDirectories(path, "*", SearchOption.AllDirectories)
                    .OrderByDescending(p => p.Length)
            )
            {
                TryDeleteDir(dir);
            }
            TryDeleteDir(path);
        }

        private static void TryDeleteFile(string p)
        {
            try
            {
                if (File.Exists(p))
                {
                    File.SetAttributes(p, FileAttributes.Normal);
                }
                File.Delete(p);
            }
            catch { }
        }

        private static void TryDeleteDir(string p)
        {
            try
            {
                if (Directory.Exists(p))
                {
                    Directory.Delete(p, recursive: false);
                }
            }
            catch { }
        }

        private static void DeleteAllStartingWith(string distRoot, string relBaseForward)
        {
            string normalized = relBaseForward.Trim('/');
            foreach (
                var path in Directory.GetFileSystemEntries(
                    distRoot,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                string rel = path.Substring(distRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
                if (
                    rel.StartsWith(normalized + "/", StringComparison.Ordinal)
                    || rel.Equals(normalized, StringComparison.Ordinal)
                )
                {
                    if (Directory.Exists(path))
                    {
                        DeleteDirectory(path);
                    }
                    else
                    {
                        TryDeleteFile(path);
                    }
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
                    if (File.Exists(metaTo))
                    {
                        File.Delete(metaTo);
                    }
                    File.Move(metaFrom, metaTo);
                }
            }
            catch { }
        }

        private static string BumpPatchVersion(string packageJsonPath)
        {
            try
            {
                if (!File.Exists(packageJsonPath))
                {
                    Debug.LogWarning(
                        "Publish: package.json not found for version bump at " + packageJsonPath
                    );
                    return null;
                }
                string text = File.ReadAllText(packageJsonPath, Encoding.UTF8);

                var m = Regex.Match(
                    text,
                    "\"version\"\\s*:\\s*\"([^\"]+)\"",
                    RegexOptions.Multiline
                );
                string current = m.Success ? m.Groups[1].Value : "0.0.0";
                var parts = current.Split('.');
                int major = parts.Length > 0 && int.TryParse(parts[0], out var a) ? a : 0;
                int minor = parts.Length > 1 && int.TryParse(parts[1], out var b) ? b : 0;
                int patch = parts.Length > 2 && int.TryParse(parts[2], out var c) ? c : 0;
                patch++;
                string next = $"{major}.{minor}.{patch}";

                if (m.Success)
                {
                    int valStart = m.Groups[1].Index;
                    int valLen = m.Groups[1].Length;
                    var sb = new StringBuilder(text.Length - valLen + next.Length);
                    sb.Append(text, 0, valStart);
                    sb.Append(next);
                    sb.Append(text, valStart + valLen, text.Length - (valStart + valLen));
                    text = sb.ToString();
                }
                else
                {
                    int insertAt = text.LastIndexOf('}');
                    if (insertAt < 0)
                    {
                        Debug.LogWarning(
                            "Publish: package.json appears malformed; cannot insert version."
                        );
                        return null;
                    }
                    string prefix = text.Substring(0, insertAt).TrimEnd();
                    string suffix = text.Substring(insertAt);
                    if (!prefix.EndsWith(","))
                    {
                        prefix += ",";
                    }
                    string insert = "\n  \"version\": \"" + next + "\"\n";
                    text = prefix + insert + suffix;
                }

                File.WriteAllText(packageJsonPath, text, Encoding.UTF8);

                var vm = Regex.Match(
                    text,
                    "\"version\"\\s*:\\s*\"([^\"]+)\"",
                    RegexOptions.Multiline
                );
                if (!vm.Success || vm.Groups[1].Value != next)
                {
                    Debug.LogError("Publish: version bump validation failed. Aborting.");
                    return null;
                }
                return next;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Publish: version bump failed: " + ex.Message);
                return null;
            }
        }

        private static int RunGit(
            string args,
            string workingDir,
            out string stdout,
            out string stderr
        )
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            var p = new System.Diagnostics.Process { StartInfo = psi };
            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();
            p.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    sbOut.AppendLine(e.Data);
                }
            };
            p.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    sbErr.AppendLine(e.Data);
                }
            };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            stdout = sbOut.ToString();
            stderr = sbErr.ToString();
            if (!string.IsNullOrEmpty(stdout))
            {
                Debug.Log($"[git] {args}\n{stdout}");
            }
            if (!string.IsNullOrEmpty(stderr))
            {
                Debug.Log($"[git:err] {args}\n{stderr}");
            }
            return p.ExitCode;
        }

        private static void DeleteAllExceptGit(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }
            foreach (var entry in Directory.GetFileSystemEntries(dir))
            {
                var name = Path.GetFileName(entry)
                    ?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (Directory.Exists(entry))
                {
                    DeleteDirectory(entry);
                }
                else
                {
                    TryDeleteFile(entry);
                }
            }
        }
    }
}
