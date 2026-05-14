#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    // Editor-side workspace-wide index of hook containers, keyed by owning
    // asmdef. Mirrors what WorkspaceIndex.GetAllCsFiles + AsmdefResolver give
    // the LSP (and what the SG's UitkxGenerator pre-scan does at compile time).
    //
    // Existence rationale: HMR's UitkxHmrCompiler.EmitCompanionUitkxSources only
    // scans the same folder as the .uitkx being recompiled; that misses hooks
    // declared in other folders (e.g. PrettyUi's UseUiDocumentSlot.hooks.uitkx
    // under Assets/UI/Hooks consumed by .uitkx files in Assets/UI/Pages/...).
    // Without this registry the runtime trampoline would lose the corresponding
    // `using static <Ns>.<HookContainer>;` directive and HMR recompiles would
    // fail with CS0103 even though full-build SG compilations succeed.
    //
    // Lifecycle:
    //   - Seed(rootDir): kicks off a background scan via Task.Run when HMR
    //     starts. Returns immediately; first HMR recompile waits ~100 ms via
    //     TryWaitForSeed and proceeds with whatever has been indexed by then.
    //   - Invalidate(path): re-indexes a single .uitkx file when the watcher
    //     reports a change. Cheap; runs synchronously.
    //   - Reset(): clears the registry on HMR stop.
    //
    // Thread safety: a single ReaderWriterLockSlim guards the dict. Writes are
    // brief (one Add/Remove per file); reads return immutable snapshots.
    internal static class HookContainerRegistry
    {
        // Single-line @namespace directive, e.g.  @namespace Foo.Bar;
        private static readonly Regex s_nsRegex = new Regex(
            @"^\s*@namespace\s+([A-Za-z_][\w.]*)\s*;",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // Top-level `hook NameOfHook(...)` declaration.
        private static readonly Regex s_hookRegex = new Regex(
            @"^\s*hook\s+[A-Za-z_]\w*\s*\(",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // ── State ───────────────────────────────────────────────────────────

        // Per-hook-file record: FQN of its emitted static partial class.
        private struct Entry
        {
            public string Asmdef;     // e.g. "Assembly-CSharp" or "MyAsm"
            public string Fqn;        // e.g. "PrettyUi.UIHooks.UseUiDocumentSlotHooks"
        }

        // file path (full) -> Entry. One entry per hook-bearing .uitkx file.
        private static readonly Dictionary<string, Entry> s_byPath =
            new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        private static readonly ReaderWriterLockSlim s_lock =
            new ReaderWriterLockSlim();

        private static readonly ManualResetEventSlim s_seedComplete =
            new ManualResetEventSlim(initialState: false);

        private static int s_seedStarted; // 0 = not seeded, 1 = seeding/seeded

        // ── Public API ──────────────────────────────────────────────────────

        public static void Seed(string rootDir)
        {
            // Idempotent: if Seed has already been called this session and not
            // Reset, just return — the registry is either complete or in flight.
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
                    Debug.LogWarning($"[HMR] HookContainerRegistry.Seed failed: {ex.Message}");
                }
                finally
                {
                    s_seedComplete.Set();
                }
            });
        }

        // Returns true if the seed scan has finished within timeoutMs.
        public static bool TryWaitForSeed(int timeoutMs)
            => s_seedComplete.Wait(timeoutMs);

        public static void Invalidate(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            try { path = Path.GetFullPath(path); } catch { return; }

            if (!File.Exists(path))
            {
                s_lock.EnterWriteLock();
                try { s_byPath.Remove(path); }
                finally { s_lock.ExitWriteLock(); }
                return;
            }
            TryIndexFile(path);
        }

        // Returns the FQNs of every hook container belonging to the given asmdef,
        // EXCLUDING any whose source path is in 'excludePaths' (used by the HMR
        // compiler to skip companions it already emits inline so the using-static
        // is only added once).
        public static IReadOnlyList<string> GetForAsmdef(
            string asmdef, ICollection<string> excludePaths)
        {
            if (string.IsNullOrEmpty(asmdef))
                return Array.Empty<string>();

            var result = new List<string>();
            s_lock.EnterReadLock();
            try
            {
                foreach (var kv in s_byPath)
                {
                    if (!string.Equals(kv.Value.Asmdef, asmdef, StringComparison.Ordinal))
                        continue;
                    if (excludePaths != null && excludePaths.Contains(kv.Key))
                        continue;
                    result.Add(kv.Value.Fqn);
                }
            }
            finally { s_lock.ExitReadLock(); }
            return result;
        }

        public static void Reset()
        {
            s_lock.EnterWriteLock();
            try { s_byPath.Clear(); }
            finally { s_lock.ExitWriteLock(); }

            s_seedComplete.Reset();
            Interlocked.Exchange(ref s_seedStarted, 0);
            AsmdefResolver.InvalidateAll();
        }

        // ── Internals ───────────────────────────────────────────────────────

        private static void TryIndexFile(string path)
        {
            try
            {
                string full = Path.GetFullPath(path);
                string text = File.ReadAllText(full);

                // Cheap pre-filter: skip files that don't declare any hook.
                if (!s_hookRegex.IsMatch(text))
                {
                    s_lock.EnterWriteLock();
                    try { s_byPath.Remove(full); }
                    finally { s_lock.ExitWriteLock(); }
                    return;
                }

                var nsMatch = s_nsRegex.Match(text);
                if (!nsMatch.Success)
                    return; // hook files without an explicit namespace cannot be referenced cross-file

                string ns = nsMatch.Groups[1].Value;
                string container = DeriveContainerClassName(full);
                string asmdef = AsmdefResolver.OwningAsmdefName(full);

                var entry = new Entry { Asmdef = asmdef, Fqn = $"{ns}.{container}" };
                s_lock.EnterWriteLock();
                try { s_byPath[full] = entry; }
                finally { s_lock.ExitWriteLock(); }
            }
            catch
            {
                // Best-effort; never crash HMR on a malformed file.
            }
        }

        // Mirrors HmrHookEmitter.DeriveContainerClassName / HookEmitter /
        // RoslynHost.DerivePeerHookContainerClass — keep these in sync.
        private static string DeriveContainerClassName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            int dot = fileName.IndexOf('.');
            if (dot > 0)
                fileName = fileName.Substring(0, dot);
            if (fileName.Length > 0 && char.IsLower(fileName[0]))
                fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            return fileName + "Hooks";
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
