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

        private FileSystemWatcher _watcher;
        private readonly Dictionary<string, int> _pendingChanges = new();
        private readonly Dictionary<string, int> _pendingUssChanges = new();
        private readonly object _lock = new object();
        private bool _disposed;

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
                // Default is 8KB which overflows easily when watching a full
                // Unity Assets/ tree (every save also touches .meta files,
                // Library/, etc.). On overflow, the OS silently drops events
                // for arbitrary files until the buffer drains — which manifests
                // as "save the .uitkx file but HMR never reacts" for whichever
                // files happened to be in the dropped batch. 64KB is the
                // documented maximum and costs only a few KB of pinned memory.
                InternalBufferSize = 64 * 1024,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += OnFileSystemEvent;
            _watcher.Created += OnFileSystemEvent;
            _watcher.Renamed += (s, e) =>
                OnFileSystemEvent(
                    s,
                    new FileSystemEventArgs(
                        WatcherChangeTypes.Changed,
                        Path.GetDirectoryName(e.FullPath),
                        Path.GetFileName(e.FullPath)
                    )
                );
            // Surface buffer overflows instead of silently losing events.
            // When this fires, every save *after* the overflow is suspect
            // until the buffer drains — log loudly so the user knows to
            // re-save (or restart HMR) rather than chasing a phantom bug.
            _watcher.Error += OnWatcherError;

            // Pump pending changes on every editor update
            EditorApplication.update += PumpPendingChanges;
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            UnityEngine.Debug.LogError(
                "[HMR] FileSystemWatcher error — events may have been dropped. "
                    + "If a recently saved .uitkx file did not hot-reload, save it "
                    + "again. Details: "
                    + (ex?.Message ?? "(no details)")
            );
        }

        public void Stop()
        {
            EditorApplication.update -= PumpPendingChanges;

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

        // Toggled via EditorPrefs key "UITKX_HMR_VerboseWatcher". When true,
        // every raw file event the OS delivers is logged. Useful when a save
        // appears to do nothing — if the path never appears in the trace, the
        // OS isn't delivering the event (FSW buffer overflow, antivirus
        // hook, network share, symlink, etc.) and the fix is upstream of HMR.
        private static bool VerboseTrace =>
            EditorPrefs.GetBool("UITKX_HMR_VerboseWatcher", false);

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            string ext = Path.GetExtension(e.FullPath);
            if (string.IsNullOrEmpty(ext))
                return;

            // Trace BEFORE any filtering so the user can see every event
            // received for the .uitkx / .uss / .cs files we care about.
            if (VerboseTrace
                && (ext.Equals(".uitkx", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".uss", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                UnityEngine.Debug.Log(
                    $"[HMR][trace] FSW {e.ChangeType} {e.FullPath}"
                );
            }

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

        // ── Main thread pump ──────────────────────────────────────────────────

        private void PumpPendingChanges()
        {
            if (_disposed)
                return;

            List<string> ready = null;
            List<string> readyUss = null;
            int now = Environment.TickCount;

            lock (_lock)
            {
                if (_pendingChanges.Count == 0 && _pendingUssChanges.Count == 0)
                    return;

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
