using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReactiveUITK.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Orchestrates the HMR lifecycle: file watching, compilation, delegate
    /// swapping, and assembly reload suppression.
    /// </summary>
    internal sealed class UitkxHmrController : IDisposable
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        private static UitkxHmrController s_instance;
        public static UitkxHmrController Instance => s_instance;
        public static bool IsActive => s_instance != null && s_instance._active;

        // ── Components ────────────────────────────────────────────────────────
        private readonly AssemblyReloadSuppressor _suppressor = new();
        private readonly UitkxHmrFileWatcher _watcher = new();
        private UitkxHmrCompiler _compiler;

        // ── State ─────────────────────────────────────────────────────────────
        private bool _active;
        private int _swapCount;
        private int _errorCount;
        private string _lastComponentName;
        private float _lastSwapMs;
        private string _lastTimingBreakdown;
        private readonly List<string> _recentErrors = new();
        private bool _compilationQueued;
        private string _queuedPath;

        // ── Pending retry state (auto-cascade for new-file dependencies) ────
        private readonly Dictionary<string, string> _pendingRetryPaths = new(
            StringComparer.OrdinalIgnoreCase
        );
        private bool _retryingPending;

        // ── Settings ──────────────────────────────────────────────────────────
        public bool AutoStopOnPlayMode
        {
            get => EditorPrefs.GetBool("UITKX_HMR_AutoStopPlay", true);
            set => EditorPrefs.SetBool("UITKX_HMR_AutoStopPlay", value);
        }
        public bool ShowNotifications
        {
            get => EditorPrefs.GetBool("UITKX_HMR_ShowNotify", true);
            set => EditorPrefs.SetBool("UITKX_HMR_ShowNotify", value);
        }

        // ── Public properties ─────────────────────────────────────────────────
        public bool Active => _active;
        public int SwapCount => _swapCount;
        public int ErrorCount => _errorCount;
        public string LastComponentName => _lastComponentName;
        public float LastSwapMs => _lastSwapMs;
        public string LastTimingBreakdown => _lastTimingBreakdown;
        public IReadOnlyList<string> RecentErrors => _recentErrors;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public bool Start(out string error)
        {
            if (_active)
            {
                error = null;
                return true;
            }

            // Initialize compiler
            _compiler = new UitkxHmrCompiler();
            if (!_compiler.TryInitialize(out error))
            {
                _compiler.Dispose();
                _compiler = null;
                return false;
            }

            // Lock assembly reloads
            _suppressor.Lock();

            // Start watching
            string assetsPath = Path.GetFullPath(UnityEngine.Application.dataPath);
            _watcher.OnUitkxChanged += OnUitkxFileChanged;
            _watcher.Start(assetsPath);

            // Hook lifecycle events
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            // Set HMR state flag (read by Fiber reconciler for CanReuseFiber)
            HmrState.IsActive = true;

            _active = true;
            _swapCount = 0;
            _errorCount = 0;
            _recentErrors.Clear();
            _pendingRetryPaths.Clear();

            s_instance = this;
            error = null;

            Debug.Log("[HMR] Started — assembly reload locked. Save a .uitkx file to hot-reload.");
            return true;
        }

        public void Stop()
        {
            if (!_active)
                return;
            _active = false;

            HmrState.IsActive = false;

            // Unhook events
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            // Stop watching
            _watcher.OnUitkxChanged -= OnUitkxFileChanged;
            _watcher.Stop();

            _pendingRetryPaths.Clear();

            // Unlock assembly reloads (triggers pending compilation)
            _suppressor.Unlock();

            // Cleanup compiler
            _compiler?.Dispose();
            _compiler = null;

            if (s_instance == this)
                s_instance = null;

            Debug.Log(
                $"[HMR] Stopped — {_swapCount} swap(s), {_errorCount} error(s). "
                    + "Assembly reload unlocked."
            );
        }

        public void Dispose() => Stop();

        // ── File change handler ───────────────────────────────────────────────

        private void OnUitkxFileChanged(string uitkxPath)
        {
            if (!_active)
                return;

            // If a compilation is already in progress, queue this one
            if (_compilationQueued)
            {
                _queuedPath = uitkxPath;
                return;
            }

            ProcessFileChange(uitkxPath);
        }

        private void ProcessFileChange(string uitkxPath)
        {
            string componentDir = Path.GetDirectoryName(uitkxPath);
            string componentBase = Path.GetFileNameWithoutExtension(uitkxPath);

            // Find companion .cs files
            string[] companionFiles = null;
            if (componentDir != null)
            {
                try
                {
                    companionFiles = Directory
                        .GetFiles(componentDir, "*.cs")
                        .Where(f => !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }
                catch
                { /* ignore IO errors */
                }
            }

            // Compile
            var result = _compiler.Compile(uitkxPath, companionFiles);

            if (result.Success)
            {
                // Swap delegates (timed)
                var swapSw = Stopwatch.StartNew();
                int swapped = UitkxHmrDelegateSwapper.SwapAll(
                    result.LoadedAssembly,
                    result.ComponentName,
                    uitkxPath
                );
                swapSw.Stop();
                result.SwapMs = swapSw.Elapsed.TotalMilliseconds;

                _swapCount++;
                _lastComponentName = result.ComponentName;
                _lastSwapMs = (float)result.TotalMs;
                _lastTimingBreakdown = result.TimingBreakdown;

                if (ShowNotifications && swapped > 0)
                    Debug.Log(
                        $"[HMR] {result.ComponentName} updated ({result.TotalMs:F0}ms, "
                            + $"{swapped} instance(s)) — {result.TimingBreakdown}"
                    );

                // Remove from pending retries if this was a re-attempt
                _pendingRetryPaths.Remove(uitkxPath);

                // A successful compilation may unblock pending failures that
                // depend on this component. Schedule a retry pass on the next frame.
                if (_pendingRetryPaths.Count > 0 && !_retryingPending)
                    EditorApplication.delayCall += RetryPendingCompilations;
            }
            else
            {
                // Try to auto-discover and compile missing dependencies.
                // CS0103 means an unresolved name — may be a .uitkx component
                // the FileWatcher missed.
                if (TryResolveMissingDependencies(result.Error, uitkxPath))
                {
                    // Dependencies were compiled — retry will happen via
                    // the cascade mechanism on the next frame.
                    return;
                }

                // During auto-retry, suppress duplicate error logging
                if (!_retryingPending || !_pendingRetryPaths.ContainsKey(uitkxPath))
                {
                    _errorCount++;
                    _recentErrors.Add(result.Error);
                    if (_recentErrors.Count > 10)
                        _recentErrors.RemoveAt(0);
                    Debug.LogWarning(result.Error);
                }
                _pendingRetryPaths[uitkxPath] = result.Error;
            }

            // Process queued file change
            if (_queuedPath != null)
            {
                string queued = _queuedPath;
                _queuedPath = null;
                // Use delayCall to avoid deep recursion
                EditorApplication.delayCall += () => ProcessFileChange(queued);
            }
        }

        // ── Missing dependency discovery ──────────────────────────────────────

        private static readonly Regex s_cs0103Re = new(
            @"error CS0103: The name '(\w+)' does not exist",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Parse CS0103 errors, search Assets/ for matching .uitkx files,
        /// compile them on-demand. Returns true if any dependencies were
        /// newly compiled (caller should retry via cascade).
        /// </summary>
        private bool TryResolveMissingDependencies(string errorText, string failedPath)
        {
            if (errorText == null || _compiler == null)
                return false;

            var matches = s_cs0103Re.Matches(errorText);
            if (matches.Count == 0)
                return false;

            bool anyCompiled = false;
            string assetsDir = Path.GetFullPath(UnityEngine.Application.dataPath);

            foreach (Match m in matches)
            {
                string missingName = m.Groups[1].Value;

                // Already compiled in this HMR session?
                if (_compiler.HmrAssemblyPaths.ContainsKey(missingName))
                    continue;

                // Search for a matching .uitkx file on disk
                string uitkxPath = FindUitkxByComponentName(assetsDir, missingName);
                if (
                    uitkxPath == null
                    || uitkxPath.Equals(failedPath, StringComparison.OrdinalIgnoreCase)
                )
                    continue;

                Debug.Log(
                    $"[HMR] Auto-discovered dependency: {missingName} → {Path.GetFileName(uitkxPath)}"
                );
                ProcessFileChange(uitkxPath);

                if (_compiler.HmrAssemblyPaths.ContainsKey(missingName))
                    anyCompiled = true;
            }

            if (anyCompiled)
            {
                // Queue the failed file for retry now that deps are available
                _pendingRetryPaths[failedPath] = errorText;
                if (!_retryingPending)
                    EditorApplication.delayCall += RetryPendingCompilations;
            }

            return anyCompiled;
        }

        /// <summary>
        /// Search Assets/ recursively for a .uitkx file whose name matches
        /// the given component name (case-insensitive).
        /// </summary>
        private static string FindUitkxByComponentName(string assetsDir, string componentName)
        {
            string target = componentName + ".uitkx";
            try
            {
                foreach (
                    var file in Directory.EnumerateFiles(
                        assetsDir,
                        "*.uitkx",
                        SearchOption.AllDirectories
                    )
                )
                {
                    if (Path.GetFileName(file).Equals(target, StringComparison.OrdinalIgnoreCase))
                        return file;
                }
            }
            catch
            { /* IO errors during scan — skip */
            }
            return null;
        }

        // ── Auto-cascade retry ────────────────────────────────────────────────

        /// <summary>
        /// Retry compilation of previously-failed files. A successful compilation
        /// of component A may have registered a new assembly that unblocks
        /// component B which references A.
        /// </summary>
        private void RetryPendingCompilations()
        {
            if (!_active || _pendingRetryPaths.Count == 0)
                return;

            _retryingPending = true;
            try
            {
                var paths = new List<string>(_pendingRetryPaths.Keys);
                foreach (var path in paths)
                {
                    if (!_active)
                        break;
                    ProcessFileChange(path);
                }
            }
            finally
            {
                _retryingPending = false;
            }
        }

        // ── Auto-stop hooks ───────────────────────────────────────────────────

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!AutoStopOnPlayMode)
                return;

            if (
                state == PlayModeStateChange.ExitingEditMode
                || state == PlayModeStateChange.ExitingPlayMode
            )
            {
                Debug.Log("[HMR] Auto-stopping due to play mode change.");
                Stop();
                UitkxHmrWindow.RepaintIfOpen();
            }
        }
    }

    /// <summary>
    /// Separate build processor that delegates to the HMR controller.
    /// Unity requires IPreprocessBuildWithReport implementors to have a
    /// parameterless constructor it can instantiate.
    /// </summary>
    internal sealed class UitkxHmrBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (UitkxHmrController.IsActive)
            {
                Debug.Log("[HMR] Auto-stopping due to build.");
                UitkxHmrController.Instance?.Stop();
            }
        }
    }
}
