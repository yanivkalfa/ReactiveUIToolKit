using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Watches for .uitkx and companion .cs file changes using FileSystemWatcher.
    /// Debounces rapid saves and marshals events to the main thread.
    /// </summary>
    internal sealed class UitkxHmrFileWatcher : IDisposable
    {
        /// <summary>
        /// Fires on the main thread after debounce. Parameter is the .uitkx file path
        /// that changed (or the .uitkx associated with a changed companion .cs file).
        /// </summary>
        public event Action<string> OnUitkxChanged;

        /// <summary>
        /// Fires on the main thread after debounce when a .uss file is saved.
        /// Parameter is the absolute path to the changed .uss file.
        /// </summary>
        public event Action<string> OnUssChanged;

        /// <summary>
        /// Fires on the main thread after debounce when a .uitkx file is
        /// deleted or renamed away (FSW Deleted/Renamed old path, or the
        /// AssetPostprocessor's deleted/moved-from lists). Parameter is the
        /// absolute path of the file that no longer exists. Used by the
        /// controller to evict stale per-file state (retry queue, compiler
        /// registry, import edges). Suppressed when the file exists again by
        /// pump time — editors that save via delete-and-replace must not tear
        /// down live registrations on every save.
        /// </summary>
        public event Action<string> OnUitkxDeleted;

        private FileSystemWatcher _watcher;
        private readonly Dictionary<string, int> _pendingChanges = new();
        private readonly Dictionary<string, int> _pendingUssChanges = new();
        private readonly Dictionary<string, int> _pendingDeletions = new();
        private readonly object _lock = new object();
        private bool _disposed;

        // Raw-event trace (EditorPrefs UITKX_HMR_VerboseWatcher, set through the
        // controller). Volatile field, not a property: read on the FSW threadpool
        // thread, written on the main thread.
        internal volatile bool TraceEnabled;

        private const int DebounceMs = 50; // 50ms debounce

        public void Start(string watchRoot)
        {
            if (_watcher != null)
                Stop();

            _watcher = new FileSystemWatcher
            {
                Path = watchRoot,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += OnFileSystemEvent;
            _watcher.Created += OnFileSystemEvent;
            _watcher.Deleted += (s, e) =>
            {
                if (TraceEnabled)
                    Debug.Log($"[HMR][trace] FSW Deleted {e.FullPath}");
                EnqueueDeletion(e.FullPath);
            };
            _watcher.Renamed += (s, e) =>
            {
                if (TraceEnabled)
                    Debug.Log($"[HMR][trace] FSW Renamed {e.OldFullPath} -> {e.FullPath}");
                // A rename is a change of the NEW path plus a deletion of the OLD
                // path — without the second half, a renamed member file's previous
                // identity (registry entries, import edges) is never evicted.
                EnqueueDeletion(e.OldFullPath);
                OnFileSystemEvent(
                    s,
                    new FileSystemEventArgs(
                        WatcherChangeTypes.Changed,
                        Path.GetDirectoryName(e.FullPath),
                        Path.GetFileName(e.FullPath)
                    )
                );
            };
            // Overflow visibility: Mono's FSW drops events silently when its 8 KB
            // buffer overflows (see the AssetPostprocessor comment below). Without
            // this handler a dropped save is indistinguishable from a save that
            // never happened. Subscribing an event does not touch the fragile FSW
            // configuration (Path/filters/EnableRaisingEvents stay exactly as-is).
            _watcher.Error += (s, e) =>
            {
                Debug.LogWarning(
                    "[HMR] File watcher error — OS file events may have been lost. "
                        + "If a save produced no '[HMR] Save:' line, re-save the file. "
                        + $"({e.GetException()?.Message ?? "unknown"})"
                );
            };

            // Pump pending changes on every editor update
            EditorApplication.update += PumpPendingChanges;

            // Register a redundant event source via Unity's AssetPostprocessor.
            // Mono's FileSystemWatcher silently drops events when its 8 KB
            // internal buffer overflows under save bursts on a deep Assets/
            // tree (every save also touches .meta files, etc.). The
            // AssetPostprocessor path is main-thread, never drops, and
            // _pendingChanges dedupes by path so redundant events from both
            // sources are harmless. Touching FSW config has proven fragile on
            // some Mono versions, so we keep FSW exactly as-is and add
            // AssetPostprocessor as a parallel safety net.
            UitkxHmrAssetPostprocessor.Register(this);
        }

        public void Stop()
        {
            EditorApplication.update -= PumpPendingChanges;

            UitkxHmrAssetPostprocessor.Unregister(this);

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            lock (_lock)
            {
                _pendingChanges.Clear();
                _pendingUssChanges.Clear();
                _pendingDeletions.Clear();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Stop();
        }

        // ── FileSystemWatcher callback (threadpool thread) ────────────────────

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            if (TraceEnabled)
                Debug.Log($"[HMR][trace] FSW {e.ChangeType} {e.FullPath}");

            string ext = Path.GetExtension(e.FullPath);
            if (string.IsNullOrEmpty(ext))
                return;

            // .uss file change
            if (ext.Equals(".uss", StringComparison.OrdinalIgnoreCase))
            {
                lock (_lock)
                {
                    _pendingUssChanges[e.FullPath] = Environment.TickCount;
                }
                return;
            }

            string uitkxPath = null;

            if (ext.Equals(".uitkx", StringComparison.OrdinalIgnoreCase))
            {
                uitkxPath = e.FullPath;
            }
            else if (ext.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            {
                // Check if this is a companion .cs file (same directory has a .uitkx)
                uitkxPath = FindAssociatedUitkx(e.FullPath);
            }

            if (uitkxPath == null)
                return;

            // Use Environment.TickCount (thread-safe) instead of
            // EditorApplication.timeSinceStartup (main thread only).
            lock (_lock)
            {
                _pendingChanges[uitkxPath] = Environment.TickCount;
            }
        }

        // Push a synthetic change event from the AssetPostprocessor.
        // Routes through OnFileSystemEvent so all extension filtering,
        // companion-.cs mapping, USS handling, and dedupe behave identically
        // to the FileSystemWatcher path.
        internal void EnqueueAssetChange(string absolutePath)
        {
            if (_disposed || string.IsNullOrEmpty(absolutePath))
                return;
            OnFileSystemEvent(
                this,
                new FileSystemEventArgs(
                    WatcherChangeTypes.Changed,
                    Path.GetDirectoryName(absolutePath) ?? string.Empty,
                    Path.GetFileName(absolutePath)
                )
            );
        }

        // Push a deletion notification from the AssetPostprocessor. Routes
        // through the same debounced pending-deletion queue as the FSW
        // Deleted/Renamed events so the pump's exists-again guard applies
        // uniformly. Companion .cs deletions are intentionally ignored:
        // there's no per-.cs-path state to evict.
        internal void EnqueueAssetDeletion(string absolutePath)
        {
            if (_disposed)
                return;
            EnqueueDeletion(absolutePath);
        }

        // Shared deletion intake (FSW threadpool thread or main thread). Also
        // evicts any pending change entry for the path so a stale debounced
        // compile doesn't fire after the file is gone.
        private void EnqueueDeletion(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return;
            string ext = Path.GetExtension(absolutePath);
            if (string.IsNullOrEmpty(ext))
                return;
            if (!ext.Equals(".uitkx", StringComparison.OrdinalIgnoreCase))
                return;

            lock (_lock)
            {
                _pendingChanges.Remove(absolutePath);
                _pendingDeletions[absolutePath] = Environment.TickCount;
            }
        }

        // ── Main thread pump ──────────────────────────────────────────────────

        private void PumpPendingChanges()
        {
            if (_disposed)
                return;

            List<string> ready = null;
            List<string> readyUss = null;
            List<string> readyDeleted = null;
            int now = Environment.TickCount;

            lock (_lock)
            {
                if (
                    _pendingChanges.Count == 0
                    && _pendingUssChanges.Count == 0
                    && _pendingDeletions.Count == 0
                )
                    return;

                foreach (var kvp in _pendingDeletions)
                {
                    if (now - kvp.Value >= DebounceMs)
                    {
                        readyDeleted ??= new List<string>();
                        readyDeleted.Add(kvp.Key);
                    }
                }

                if (readyDeleted != null)
                    foreach (var path in readyDeleted)
                        _pendingDeletions.Remove(path);

                foreach (var kvp in _pendingChanges)
                {
                    if (now - kvp.Value >= DebounceMs)
                    {
                        ready ??= new List<string>();
                        ready.Add(kvp.Key);
                    }
                }

                if (ready != null)
                    foreach (var path in ready)
                        _pendingChanges.Remove(path);

                foreach (var kvp in _pendingUssChanges)
                {
                    if (now - kvp.Value >= DebounceMs)
                    {
                        readyUss ??= new List<string>();
                        readyUss.Add(kvp.Key);
                    }
                }

                if (readyUss != null)
                    foreach (var path in readyUss)
                        _pendingUssChanges.Remove(path);
            }

            // Deletions first: a rename delivers deletion(old) + change(new) in the
            // same burst, and the old identity must be evicted before the new path's
            // compile re-registers its dependents. Exists-again guard: editors that
            // save via delete-and-replace surface a transient Deleted for a file
            // that is back on disk by pump time — that is a save, not a deletion.
            // A save it is: EnqueueDeletion cancelled any pending change for the
            // path, so discarding here would eat the whole save with zero events
            // (the mid-session member-file silence class). Re-route it as a change
            // unless one is already pending or matured in this pump.
            if (readyDeleted != null)
            {
                foreach (var path in readyDeleted)
                {
                    if (!File.Exists(path))
                    {
                        OnUitkxDeleted?.Invoke(path);
                        continue;
                    }
                    bool changePending;
                    lock (_lock)
                    {
                        changePending = _pendingChanges.ContainsKey(path);
                    }
                    bool changeMatured = false;
                    if (!changePending && ready != null)
                        foreach (var r in ready)
                            if (string.Equals(r, path, StringComparison.OrdinalIgnoreCase))
                            {
                                changeMatured = true;
                                break;
                            }
                    if (!changePending && !changeMatured)
                    {
                        Debug.Log(
                            $"[HMR] Watch: delete-and-replace save detected for "
                                + $"{Path.GetFileName(path)} — treating as change."
                        );
                        OnUitkxChanged?.Invoke(path);
                    }
                }
            }

            if (ready != null)
                foreach (var path in ready)
                    OnUitkxChanged?.Invoke(path);

            if (readyUss != null)
                foreach (var path in readyUss)
                    OnUssChanged?.Invoke(path);
        }

        // ── Companion file detection ──────────────────────────────────────────

        /// <summary>
        /// Given a .cs file path, find the associated .uitkx file in the same directory.
        /// Convention: MyComponent.styles.cs → MyComponent.uitkx (same folder).
        /// </summary>
        private static string FindAssociatedUitkx(string csFilePath)
        {
            string dir = Path.GetDirectoryName(csFilePath);
            if (dir == null)
                return null;

            // Skip generated files
            string fileName = Path.GetFileName(csFilePath);
            if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                return null;

            // Check if any .uitkx file exists in the same directory
            try
            {
                var uitkxFiles = Directory.GetFiles(dir, "*.uitkx");
                if (uitkxFiles.Length == 0)
                    return null;

                // If exactly one .uitkx, it's the companion target
                if (uitkxFiles.Length == 1)
                    return uitkxFiles[0];

                // Multiple .uitkx files: try matching by prefix
                string csBase = Path.GetFileNameWithoutExtension(csFilePath);
                // Strip additional extensions: MyComponent.styles → MyComponent
                int dotIdx = csBase.IndexOf('.');
                if (dotIdx > 0)
                    csBase = csBase.Substring(0, dotIdx);

                return uitkxFiles.FirstOrDefault(f =>
                        Path.GetFileNameWithoutExtension(f)
                            .Equals(csBase, StringComparison.OrdinalIgnoreCase)
                    ) ?? uitkxFiles[0]; // fallback to first
            }
            catch
            {
                return null;
            }
        }
    }
}
