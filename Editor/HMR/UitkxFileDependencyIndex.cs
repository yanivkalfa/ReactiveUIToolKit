#if UNITY_EDITOR
#nullable enable annotations
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    // ─────────────────────────────────────────────────────────────────────────
    //  UitkxFileDependencyIndex
    //
    //  Workspace-wide, asmdef-aware dependency graph for .uitkx files. Used by
    //  HMR Ranks 3 / 4 / 5 to cascade recompiles transitively when a module
    //  value or a component prop signature changes.
    //
    //  Tracks for every indexed file:
    //    • Asmdef                  — Unity assembly that owns the file.
    //    • DeclaredNamespace       — @namespace value.
    //    • ComponentName           — single `component <Name>` declaration (or
    //                                null for module-/hook-only files).
    //    • DeclaredModules         — set of `module <Name>` declarations.
    //    • ReferencedModules       — set of short module names this file reads
    //                                via `Name.Member` token tuples.
    //    • ReferencedComponents    — set of JSX `<Tag>` names whose first
    //                                character is uppercase (function comps).
    //
    //  Reverse maps for cascade walks:
    //    • s_moduleReverse[name]    → set of files that READ that module.
    //    • s_componentReverse[name] → set of files that CONSUME that component.
    //
    //  Lifecycle mirrors HookContainerRegistry exactly so reviewers don't have
    //  to learn a new pattern. Cycle-safe transitive walker provided.
    //
    //  THIS IS A PARALLEL INDEX TO HookContainerRegistry, NOT A REPLACEMENT.
    //  HookContainerRegistry already proves the lifecycle works in production;
    //  this file extends the same pattern to a richer per-file payload.
    // ─────────────────────────────────────────────────────────────────────────
    internal static class UitkxFileDependencyIndex
    {
        // ── Regexes (kept identical in spirit to WorkspaceIndex parity rules)

        private static readonly Regex s_nsRegex = new Regex(
            @"^\s*@namespace\s+([A-Za-z_][\w.]*)\s*;",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // `module Foo {` (PascalCase). Captures the name.
        private static readonly Regex s_moduleDeclRegex = new Regex(
            @"^\s*module\s+(?<name>[A-Z][A-Za-z0-9_]*)\s*\{",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // `component Foo(`, `component Foo {`, or `component Foo<`.
        private static readonly Regex s_componentDeclRegex = new Regex(
            @"^\s*component\s+(?<name>[A-Z][A-Za-z0-9_]*)\s*(?:\(|\{|<)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // PascalCase identifier followed by `.<member>` (token-bounded). Used to
        // detect `ModuleName.Field` reads. False positives on `System.Linq` etc.
        // are filtered against the known-module set computed across the index.
        private static readonly Regex s_pascalDottedRegex = new Regex(
            @"\b(?<name>[A-Z][A-Za-z0-9_]*)\s*\.\s*[A-Za-z_]",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // JSX-style PascalCase open tag: `<Foo` or `<Foo.` (member-access tag).
        // Lowercase tags are built-in DOM elements; we don't track them.
        private static readonly Regex s_jsxOpenRegex = new Regex(
            @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // Strips // line comments, /* block */ comments, "..." and @"..." string
        // literals so token scans don't false-positive on text inside them.
        // Order matters: longer constructs first.
        private static readonly Regex s_stripRegex = new Regex(
            "@\"(?:[^\"]|\"\")*\"" +    // @"..." verbatim string
            "|\"(?:\\\\.|[^\"\\\\])*\"" + // "..." regular string (escape-aware)
            "|/\\*[\\s\\S]*?\\*/" +     // /* ... */ block comment
            "|//[^\\n]*",               // // line comment
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // ── State ───────────────────────────────────────────────────────────

        internal sealed class FileNode
        {
            public string FilePath = "";
            public string Asmdef = "";
            public string DeclaredNamespace = "";
            public string? ComponentName;
            public HashSet<string> DeclaredModules =
                new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> ReferencedModules =
                new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> ReferencedComponents =
                new HashSet<string>(StringComparer.Ordinal);
        }

        // path → node (case-insensitive on Windows; the watcher normalises paths).
        private static readonly Dictionary<string, FileNode> s_byPath =
            new Dictionary<string, FileNode>(StringComparer.OrdinalIgnoreCase);

        // module name → set of files referencing it.
        private static readonly Dictionary<string, HashSet<string>> s_moduleReverse =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        // component name → set of files consuming it via JSX.
        private static readonly Dictionary<string, HashSet<string>> s_componentReverse =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        private static readonly ReaderWriterLockSlim s_lock =
            new ReaderWriterLockSlim();

        private static readonly ManualResetEventSlim s_seedComplete =
            new ManualResetEventSlim(initialState: false);

        private static int s_seedStarted; // 0 = not started, 1 = in flight or done

        // ── Public API ──────────────────────────────────────────────────────

        public static void Seed(string rootDir)
        {
            if (Interlocked.Exchange(ref s_seedStarted, 1) == 1)
                return;

            Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(rootDir) || !Directory.Exists(rootDir))
                        return;

                    foreach (var path in Directory.EnumerateFiles(
                                 rootDir, "*.uitkx", SearchOption.AllDirectories))
                    {
                        if (IsInsideTildeFolder(path))
                            continue;
                        TryIndexFile(path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] UitkxFileDependencyIndex.Seed failed: {ex.Message}");
                }
                finally
                {
                    s_seedComplete.Set();
                }
            });
        }

        public static bool TryWaitForSeed(int timeoutMs)
            => s_seedComplete.Wait(timeoutMs);

        public static void Invalidate(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            try { path = Path.GetFullPath(path); } catch { return; }

            if (!File.Exists(path))
            {
                RemoveFile(path);
                return;
            }
            TryIndexFile(path);
        }

        public static void Reset()
        {
            s_lock.EnterWriteLock();
            try
            {
                s_byPath.Clear();
                s_moduleReverse.Clear();
                s_componentReverse.Clear();
            }
            finally { s_lock.ExitWriteLock(); }

            s_seedComplete.Reset();
            Interlocked.Exchange(ref s_seedStarted, 0);
        }

        // Returns a snapshot view of the node for the given path, or null.
        public static FileNode? TryGetNode(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            try { path = Path.GetFullPath(path); } catch { return null; }

            s_lock.EnterReadLock();
            try
            {
                return s_byPath.TryGetValue(path, out var node) ? node : null;
            }
            finally { s_lock.ExitReadLock(); }
        }

        // ── Transitive cascade walker (cycle-safe) ───────────────────────────

        // Returns the dep-graph closure of files that must be recompiled when
        // 'startPath' changes. Order: dependencies first (topological), the
        // start file LAST so its cctor runs after its dependents updated their
        // own shared static state.
        //
        // 'modulesOnly' restricts the walk to s_moduleReverse (Rank 3); set to
        // false to also follow s_componentReverse (Rank 4).
        //
        // Cross-asmdef edges are pruned: Unity recompiles other asmdefs on its
        // own next reload, and an HMR DLL is asmdef-scoped by construction.
        public static List<string> CollectTransitiveDependents(
            string startPath,
            bool includeComponents)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(startPath))
                return result;
            try { startPath = Path.GetFullPath(startPath); } catch { return result; }

            string startAsmdef;
            s_lock.EnterReadLock();
            try
            {
                if (!s_byPath.TryGetValue(startPath, out var startNode))
                {
                    result.Add(startPath);
                    return result;
                }
                startAsmdef = startNode.Asmdef;

                // BFS-style topological collection using a queue + visited set.
                // We do not need true Tarjan SCC ordering — cycle break is by
                // visited check, and ProcessFileChange tolerates per-file order
                // within the same asmdef.
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var dependents = new List<string>();
                var queue = new Queue<string>();
                queue.Enqueue(startPath);
                visited.Add(startPath);

                while (queue.Count > 0)
                {
                    string cur = queue.Dequeue();
                    if (!s_byPath.TryGetValue(cur, out var node))
                        continue;

                    // Add all modules declared in 'cur' as cascade seeds.
                    foreach (string mod in node.DeclaredModules)
                    {
                        if (!s_moduleReverse.TryGetValue(mod, out var refs))
                            continue;
                        foreach (string referrer in refs)
                            EnqueueIfSameAsmdef(referrer, queue, visited, dependents, startAsmdef);
                    }

                    if (includeComponents && !string.IsNullOrEmpty(node.ComponentName))
                    {
                        if (s_componentReverse.TryGetValue(node.ComponentName!, out var crefs))
                        {
                            foreach (string referrer in crefs)
                                EnqueueIfSameAsmdef(
                                    referrer, queue, visited, dependents, startAsmdef);
                        }
                    }
                }

                // Topology: dependents first, then the start file (cctor of
                // start runs LAST so consumers see fresh module statics).
                result.AddRange(dependents);
                result.Add(startPath);
            }
            finally { s_lock.ExitReadLock(); }

            return result;
        }

        private static void EnqueueIfSameAsmdef(
            string referrer,
            Queue<string> queue,
            HashSet<string> visited,
            List<string> dependents,
            string startAsmdef)
        {
            if (!visited.Add(referrer))
                return; // cycle break
            if (!s_byPath.TryGetValue(referrer, out var refNode))
                return;
            if (!string.Equals(refNode.Asmdef, startAsmdef, StringComparison.Ordinal))
                return; // cross-asmdef cascade pruned
            dependents.Add(referrer);
            queue.Enqueue(referrer);
        }

        // ── Internals ───────────────────────────────────────────────────────

        private static void TryIndexFile(string path)
        {
            try
            {
                string full = Path.GetFullPath(path);
                string raw = File.ReadAllText(full);
                string stripped = s_stripRegex.Replace(raw, " ");

                var node = new FileNode
                {
                    FilePath = full,
                    Asmdef = AsmdefResolver.OwningAsmdefName(full),
                };

                var nsMatch = s_nsRegex.Match(raw);
                if (nsMatch.Success)
                    node.DeclaredNamespace = nsMatch.Groups[1].Value;

                foreach (Match m in s_moduleDeclRegex.Matches(stripped))
                    node.DeclaredModules.Add(m.Groups["name"].Value);

                var compMatch = s_componentDeclRegex.Match(stripped);
                if (compMatch.Success)
                    node.ComponentName = compMatch.Groups["name"].Value;

                // Referenced modules and components are best-effort token scans
                // over the stripped source. The reverse map filters non-modules
                // at lookup time (cheap).
                foreach (Match m in s_pascalDottedRegex.Matches(stripped))
                {
                    string name = m.Groups["name"].Value;
                    // Skip self-references to the file's own module decls — they
                    // can't trigger a useful cascade.
                    if (node.DeclaredModules.Contains(name))
                        continue;
                    node.ReferencedModules.Add(name);
                }

                foreach (Match m in s_jsxOpenRegex.Matches(stripped))
                {
                    string name = m.Groups["name"].Value;
                    if (string.Equals(name, node.ComponentName, StringComparison.Ordinal))
                        continue;
                    node.ReferencedComponents.Add(name);
                }

                CommitNode(full, node);
            }
            catch
            {
                // Best-effort: never crash HMR on a malformed file.
            }
        }

        private static void CommitNode(string full, FileNode node)
        {
            s_lock.EnterWriteLock();
            try
            {
                // Evict any stale reverse entries from the prior version.
                if (s_byPath.TryGetValue(full, out var prior))
                    EvictReverseEntries(full, prior);

                s_byPath[full] = node;

                foreach (string mod in node.ReferencedModules)
                    AddReverse(s_moduleReverse, mod, full);
                foreach (string comp in node.ReferencedComponents)
                    AddReverse(s_componentReverse, comp, full);
            }
            finally { s_lock.ExitWriteLock(); }
        }

        private static void RemoveFile(string full)
        {
            s_lock.EnterWriteLock();
            try
            {
                if (!s_byPath.TryGetValue(full, out var prior))
                    return;
                EvictReverseEntries(full, prior);
                s_byPath.Remove(full);
            }
            finally { s_lock.ExitWriteLock(); }
        }

        private static void EvictReverseEntries(string full, FileNode prior)
        {
            foreach (string mod in prior.ReferencedModules)
                RemoveReverse(s_moduleReverse, mod, full);
            foreach (string comp in prior.ReferencedComponents)
                RemoveReverse(s_componentReverse, comp, full);
        }

        private static void AddReverse(
            Dictionary<string, HashSet<string>> map, string key, string path)
        {
            if (!map.TryGetValue(key, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                map[key] = set;
            }
            set.Add(path);
        }

        private static void RemoveReverse(
            Dictionary<string, HashSet<string>> map, string key, string path)
        {
            if (!map.TryGetValue(key, out var set))
                return;
            set.Remove(path);
            if (set.Count == 0)
                map.Remove(key);
        }

        private static bool IsInsideTildeFolder(string filePath)
        {
            foreach (var seg in (filePath ?? string.Empty).Split(
                         new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                if (seg.Length > 1 && seg[seg.Length - 1] == '~')
                    return true;
            }
            return false;
        }
    }
}
#endif
