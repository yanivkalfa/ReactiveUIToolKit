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
        private bool _previousRunInBackground;
        private int _swapCount;
        private int _errorCount;
        private string _lastComponentName;
        private float _lastSwapMs;
        private string _lastTimingBreakdown;
        private readonly List<string> _recentErrors = new();
        private bool _compilationQueued;
        private string _queuedPath;

        // Set the first time the compiler reports an infrastructure failure
        // (reflection signature mismatch, missing type/method, bad image, etc.).
        // HMR self-disables and refuses to spam the console with retries until
        // the user fixes the underlying language-library mismatch and restarts
        // HMR (or Unity).
        private bool _loggedInfrastructureFailure;

        // ── Pending retry state (auto-cascade for new-file dependencies) ────
        private readonly Dictionary<string, string> _pendingRetryPaths = new(
            StringComparer.OrdinalIgnoreCase
        );
        private bool _retryingPending;

        // ── USS → UITKX reverse dependency map ───────────────────────────────
        // Key = absolute .uss path (lower-case), Value = list of absolute .uitkx paths
        private readonly Dictionary<string, List<string>> _ussDependents = new(
            StringComparer.OrdinalIgnoreCase
        );

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

        /// <summary>
        /// When true, a detected CLR rude edit on a module type (newly-added
        /// <c>static readonly</c> field) automatically triggers a domain
        /// reload via <c>EditorUtility.RequestScriptReload</c> on the next
        /// editor frame. Default true — adding a new field is otherwise
        /// invisible until the user manually reloads. Disable only if you
        /// want full manual control (a warning is still logged either way).
        /// </summary>
        public bool AutoReloadOnRudeEdit
        {
            get => EditorPrefs.GetBool("UITKX_HMR_AutoReloadOnRudeEdit", true);
            set => EditorPrefs.SetBool("UITKX_HMR_AutoReloadOnRudeEdit", value);
        }

        /// <summary>
        /// When true, the FileSystemWatcher logs every raw .uitkx / .uss / .cs
        /// event to the Console as <c>[HMR][trace] FSW ...</c>. Use this when a
        /// save appears to do nothing in HMR — if no trace line appears for
        /// your file, the OS itself isn't delivering the event (FSW buffer
        /// overflow, antivirus hook, OneDrive/symlink path, etc.) and the
        /// problem is upstream of HMR. Off by default; high noise.
        /// </summary>
        public bool VerboseWatcherTrace
        {
            get => EditorPrefs.GetBool("UITKX_HMR_VerboseWatcher", false);
            set => EditorPrefs.SetBool("UITKX_HMR_VerboseWatcher", value);
        }

        // ── Memory tracking ─────────────────────────────────────────────────
        private long _sessionBaselineMemory;

        // ── Public properties ─────────────────────────────────────────────
        public bool Active => _active;
        public int SwapCount => _swapCount;
        public int ErrorCount => _errorCount;
        public string LastComponentName => _lastComponentName;
        public float LastSwapMs => _lastSwapMs;
        public string LastTimingBreakdown => _lastTimingBreakdown;
        public IReadOnlyList<string> RecentErrors => _recentErrors;

        /// <summary>Current process working set in bytes (close to Task Manager).</summary>
#if UNITY_EDITOR_WIN
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern System.IntPtr GetCurrentProcess();

        [System.Runtime.InteropServices.DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetProcessMemoryInfo(
            System.IntPtr hProcess,
            out PROCESS_MEMORY_COUNTERS counters,
            uint size
        );

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential
        )]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public System.UIntPtr PeakWorkingSetSize;
            public System.UIntPtr WorkingSetSize;
            public System.UIntPtr QuotaPeakPagedPoolUsage;
            public System.UIntPtr QuotaPagedPoolUsage;
            public System.UIntPtr QuotaPeakNonPagedPoolUsage;
            public System.UIntPtr QuotaNonPagedPoolUsage;
            public System.UIntPtr PagefileUsage;
            public System.UIntPtr PeakPagefileUsage;
        }

        public static long CurrentMemoryBytes
        {
            get
            {
                var counters = new PROCESS_MEMORY_COUNTERS();
                counters.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(counters);
                if (GetProcessMemoryInfo(GetCurrentProcess(), out counters, counters.cb))
                    return (long)(ulong)counters.WorkingSetSize;
                return 0;
            }
        }
#else
        public static long CurrentMemoryBytes =>
            UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()
            + UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
#endif

        /// <summary>Delta from HMR session start, in MB.</summary>
        public float SessionMemoryDeltaMB =>
            _active ? (CurrentMemoryBytes - _sessionBaselineMemory) / (1024f * 1024f) : 0f;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public bool Start(out string error)
        {
            if (_active)
            {
                error = null;
                return true;
            }

            // Reuse the compiler across start/stop cycles to avoid re-creating
            // the expensive Roslyn MetadataReferences (~150-200MB) each time.
            if (_compiler == null)
            {
                _compiler = new UitkxHmrCompiler();
                if (!_compiler.TryInitialize(out error))
                {
                    _compiler.Dispose();
                    _compiler = null;
                    return false;
                }
            }
            else
            {
                // Clear per-session caches from the previous HMR session
                _compiler.Reset();
            }

            // Lock assembly reloads
            _suppressor.Lock();

            // Start watching
            string assetsPath = Path.GetFullPath(UnityEngine.Application.dataPath);
            _watcher.OnUitkxChanged += OnUitkxFileChanged;
            _watcher.OnUssChanged += OnUssFileChanged;
            _watcher.Start(assetsPath);

            // Seed the workspace-wide hook-container registry so HMR recompiles
            // can resolve cross-directory `using static <Ns>.<HookContainer>;`
            // directives without scanning the project on every recompile. This
            // mirrors what the SG's UitkxGenerator pre-scan does at compile
            // time. The seed runs on a background thread; the first recompile
            // gates briefly via TryWaitForSeed.
            HookContainerRegistry.Seed(assetsPath);

            // Build initial USS dependency map (only on first start;
            // subsequent starts reuse the map since .uitkx files haven't changed)
            if (_ussDependents.Count == 0)
                BuildUssDependencyMap(assetsPath);

            // Hook lifecycle events
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            // Set HMR state flag (read by Fiber reconciler for CanReuseFiber)
            HmrState.IsActive = true;

            // Keep Unity's update loop running when the editor loses focus so
            // FileSystemWatcher events are pumped to the main thread immediately.
            _previousRunInBackground = UnityEngine.Application.runInBackground;
            UnityEngine.Application.runInBackground = true;

            _active = true;
            _swapCount = 0;
            _errorCount = 0;
            _recentErrors.Clear();
            _pendingRetryPaths.Clear();
            _loggedInfrastructureFailure = false;
            _sessionBaselineMemory = CurrentMemoryBytes;

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

            // Restore original runInBackground setting
            UnityEngine.Application.runInBackground = _previousRunInBackground;

            // Unhook events
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            // Stop watching
            _watcher.OnUitkxChanged -= OnUitkxFileChanged;
            _watcher.OnUssChanged -= OnUssFileChanged;
            _watcher.Stop();

            // Drop the workspace-wide hook-container index; a fresh seed runs
            // on the next Start.
            HookContainerRegistry.Reset();

            _pendingRetryPaths.Clear();
            // Keep _ussDependents across start/stop cycles — it's rebuilt
            // incrementally and re-scanning all .uitkx files is expensive.

            // Unlock assembly reloads (triggers pending compilation)
            _suppressor.Unlock();

            // Reset session caches but keep the compiler alive
            // (Roslyn MetadataReferences are expensive to rebuild)
            _compiler?.Reset();

            // Keep s_instance alive — the controller persists across
            // start/stop cycles to reuse the compiler.

            Debug.Log(
                $"[HMR] Stopped — {_swapCount} swap(s), {_errorCount} error(s). "
                    + "Assembly reload unlocked."
            );
        }

        public void Dispose()
        {
            Stop();
            _compiler?.Dispose();
            _compiler = null;
            _ussDependents.Clear();

            if (s_instance == this)
                s_instance = null;
        }

        // ── File change handler ───────────────────────────────────────────────

        private void OnUitkxFileChanged(string uitkxPath)
        {
            if (!_active)
                return;

            // Keep the workspace-wide hook-container index in sync with disk so
            // a newly added or edited hook file is visible to the next recompile.
            HookContainerRegistry.Invalidate(uitkxPath);

            // If a compilation is already in progress, queue this one
            if (_compilationQueued)
            {
                _queuedPath = uitkxPath;
                return;
            }

            // If this is a companion .uitkx file (e.g. Foo.style.uitkx),
            // redirect to compile the parent component file (Foo.uitkx)
            // so the companion's module/hook members are included.
            uitkxPath = ResolveParentComponentFile(uitkxPath);

            ProcessFileChange(uitkxPath);
        }

        /// <summary>
        /// If <paramref name="uitkxPath"/> is a companion file (e.g. Foo.style.uitkx,
        /// Foo.hooks.uitkx), returns the parent component file path (Foo.uitkx).
        /// Otherwise returns the original path unchanged.
        /// </summary>
        private static string ResolveParentComponentFile(string uitkxPath)
        {
            // Companion files have double extensions: ComponentName.suffix.uitkx
            var fileName = Path.GetFileName(uitkxPath); // "Foo.style.uitkx"
            var withoutExt = Path.GetFileNameWithoutExtension(fileName); // "Foo.style"

            // If the base name still contains a dot, it's a companion
            int dotIdx = withoutExt.IndexOf('.');
            if (dotIdx > 0)
            {
                string componentName = withoutExt.Substring(0, dotIdx); // "Foo"
                string dir = Path.GetDirectoryName(uitkxPath);
                string parentPath = Path.Combine(dir, componentName + ".uitkx");
                if (File.Exists(parentPath))
                    return parentPath;
            }
            return uitkxPath;
        }

        private void ProcessFileChange(string uitkxPath)
        {
            string componentDir = Path.GetDirectoryName(uitkxPath);
            string componentBase = Path.GetFileNameWithoutExtension(uitkxPath);

            // Find companion .cs files scoped to this component only.
            // Only include files named <ComponentBase>.cs, <ComponentBase>.styles.cs, etc.
            // to avoid pulling in another component's companions from the same directory.
            string[] companionFiles = null;
            if (componentDir != null)
            {
                try
                {
                    string prefix = componentBase + ".";
                    companionFiles = Directory
                        .GetFiles(componentDir, "*.cs")
                        .Where(f =>
                        {
                            var fileName = Path.GetFileName(f);
                            return !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                                && fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                        })
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
                // Sync asset references into cache (lightweight — no SO write)
                SyncAssetCacheForHmr(uitkxPath);

                // Update USS dependency map for this file
                RegisterUssDependencies(uitkxPath);

                // Re-bind `static readonly` module fields BEFORE delegate swap
                // so the new render delegate sees the new field values on its
                // first execution. Pre-fix, edits to a `module` declaration's
                // field initializers (e.g. removing a Style entry) had no
                // effect until the user exited Play mode and forced a full
                // assembly reload. See UitkxHmrModuleStaticSwapper for design.
                ModuleStaticSwapResult moduleStaticResult = ModuleStaticSwapResult.Empty;
                try
                {
                    moduleStaticResult = UitkxHmrModuleStaticSwapper.SwapModuleStatics(
                        result.LoadedAssembly
                    );
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[HMR] Module-static re-init failed: {ex.Message}"
                    );
                }
                int reInitedFields = moduleStaticResult.Copied;
                int addedFieldsDetected = moduleStaticResult.AddedFieldsDetected;

                // Re-bind module static-method __hmr_* delegate fields. Mirrors
                // the per-fiber render-delegate swap and the hook delegate swap,
                // but operates type-wide on every module type in the HMR assembly.
                int reInitedMethods = 0;
                try
                {
                    reInitedMethods = UitkxHmrModuleMethodSwapper.SwapModuleMethods(
                        result.LoadedAssembly
                    );
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[HMR] Module-method re-init failed: {ex.Message}"
                    );
                }

                // Swap delegates (timed)
                var swapSw = Stopwatch.StartNew();
                int swapped;
                if (result.IsHookModuleFile)
                {
                    // Hook/module files: update static delegate fields + global re-render
                    string ns = null;
                    try
                    {
                        // Extract namespace from the first type in the HMR assembly
                        var firstType = result.LoadedAssembly.GetTypes().FirstOrDefault();
                        if (firstType != null)
                            ns = firstType.Namespace;
                    }
                    catch { }

                    swapped = UitkxHmrDelegateSwapper.SwapHooks(
                        result.LoadedAssembly,
                        result.HookContainerClass,
                        ns
                    );
                }
                else
                {
                    // Component files: swap the per-component __hmr_Render
                    // trampoline field. Single static-field write per
                    // component type — no per-fiber tree walk.
                    swapped = UitkxHmrComponentTrampolineSwapper.SwapAll(
                        result.LoadedAssembly,
                        result.ComponentName,
                        uitkxPath
                    );
                }
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
                            + (
                                reInitedFields > 0
                                    ? $" | Module statics re-init: {reInitedFields}"
                                    : string.Empty
                            )
                            + (
                                reInitedMethods > 0
                                    ? $" | Module methods re-init: {reInitedMethods}"
                                    : string.Empty
                            )
                    );

                // Rude-edit handling: a newly-added `static readonly` field on
                // a module type cannot be added to the project-loaded type
                // (CLR seals type metadata). The swapper has already logged a
                // once-per-session warning. If the user opted in, request a
                // domain reload to materialise the new field everywhere.
                // The request is routed through RequestDomainReloadSafe which
                // defers the reload until Edit mode if we are currently in
                // Play mode — firing RequestScriptReload during Play mode
                // produces partial reloads that leave MonoBehaviours with
                // missing script references.
                if (addedFieldsDetected > 0 && AutoReloadOnRudeEdit)
                {
                    Debug.Log(
                        "[HMR] AutoReloadOnRudeEdit enabled — scheduling domain "
                            + $"reload to materialise {addedFieldsDetected} newly-added "
                            + "module field(s) on the project-loaded type(s)."
                    );
                    RequestDomainReloadSafe(
                        $"materialise {addedFieldsDetected} newly-added module field(s)"
                    );
                }

                // Remove from pending retries if this was a re-attempt
                _pendingRetryPaths.Remove(uitkxPath);

                // A successful compilation may unblock pending failures that
                // depend on this component. Schedule a retry pass on the next frame.
                if (_pendingRetryPaths.Count > 0 && !_retryingPending)
                    EditorApplication.delayCall += RetryPendingCompilations;
            }
            else
            {
                // Infrastructure failures (reflection signature mismatch, missing
                // type/method, etc.) mean HMR plumbing itself is broken — retrying
                // would fail forever. Log once with actionable text and self-disable.
                if (result.IsInfrastructureError)
                {
                    if (!_loggedInfrastructureFailure)
                    {
                        _loggedInfrastructureFailure = true;
                        _errorCount++;
                        _recentErrors.Add(result.Error);
                        if (_recentErrors.Count > 10)
                            _recentErrors.RemoveAt(0);
                        Debug.LogError(
                            "[HMR] Infrastructure failure — the loaded language "
                                + "library is incompatible with this HMR build. HMR has "
                                + "been disabled for this session. Restart Unity (or click "
                                + "Start in the HMR window) after rebuilding the language "
                                + "library.\n\nDetails: "
                                + result.Error
                        );
                    }
                    Stop();
                    return;
                }

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

        // ── USS change handler ────────────────────────────────────────────────

        private void OnUssFileChanged(string ussPath)
        {
            if (!_active)
                return;

            // Also update the cached StyleSheet in the registry so PropsApplier
            // picks up the new version on reconcile.
            string normalized = ussPath.Replace('\\', '/');
            int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            if (assetsIdx >= 0)
            {
                string assetRelative = normalized.Substring(assetsIdx + 1); // "Assets/..."
                AssetDatabase.ImportAsset(assetRelative, ImportAssetOptions.ForceSynchronousImport);
                var sheet = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>(
                    assetRelative
                );
                if (sheet != null)
                    UitkxAssetRegistry.InjectCacheEntry(assetRelative, sheet);
            }

            // Find all .uitkx files that reference this .uss and re-trigger HMR
            if (_ussDependents.TryGetValue(ussPath, out var dependents))
            {
                foreach (string uitkxPath in dependents)
                    OnUitkxFileChanged(uitkxPath);
            }
        }

        /// <summary>
        /// Scans all .uitkx files under assetsRoot for @uss directives and builds
        /// a reverse map: absolute .uss path → list of .uitkx paths that import it.
        /// </summary>
        private void BuildUssDependencyMap(string assetsRoot)
        {
            _ussDependents.Clear();
            try
            {
                foreach (
                    string uitkxPath in Directory.EnumerateFiles(
                        assetsRoot,
                        "*.uitkx",
                        SearchOption.AllDirectories
                    )
                )
                {
                    RegisterUssDependencies(uitkxPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HMR] Failed to build USS dependency map: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads a single .uitkx file, extracts @uss directives, and registers
        /// the reverse mapping. Also called after every successful HMR compilation
        /// to keep the map up to date.
        /// </summary>
        private void RegisterUssDependencies(string uitkxPath)
        {
            try
            {
                string content = File.ReadAllText(uitkxPath);
                string uitkxDir = Path.GetDirectoryName(uitkxPath);

                foreach (Match m in s_ussDirectiveRe.Matches(content))
                {
                    string rawPath = m.Groups[1].Value;
                    // Resolve relative paths to absolute
                    string absoluteUss;
                    if (rawPath.StartsWith("./") || rawPath.StartsWith("../"))
                    {
                        absoluteUss = Path.GetFullPath(Path.Combine(uitkxDir, rawPath));
                    }
                    else
                    {
                        // Assume Assets-relative path
                        string projectRoot = Path.GetFullPath(
                            Path.Combine(UnityEngine.Application.dataPath, "..")
                        );
                        absoluteUss = Path.GetFullPath(Path.Combine(projectRoot, rawPath));
                    }

                    if (!_ussDependents.TryGetValue(absoluteUss, out var list))
                    {
                        list = new List<string>();
                        _ussDependents[absoluteUss] = list;
                    }
                    if (!list.Contains(uitkxPath))
                        list.Add(uitkxPath);
                }
            }
            catch
            { /* file may be locked or deleted */
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

        // ── Asset cache sync for HMR ──────────────────────────────────────────

        private static readonly Regex s_assetCallRe = new Regex(
            @"(?:Asset|Ast)\s*<\s*(\w+)\s*>\s*\(\s*""([^""]+)""\s*\)",
            RegexOptions.Compiled
        );

        private static readonly Regex s_ussDirectiveRe = new Regex(
            @"@uss\s+""([^""]+)""",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Lightweight HMR-safe asset cache sync: reads the .uitkx file, extracts
        /// asset references, resolves paths, and injects them directly into the
        /// static cache without writing to the SO.
        /// </summary>
        private static void SyncAssetCacheForHmr(string uitkxPath)
        {
            try
            {
                if (!File.Exists(uitkxPath))
                    return;
                string content = File.ReadAllText(uitkxPath);

                string normalized = uitkxPath.Replace('\\', '/');
                int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
                string assetDir;
                if (assetsIdx >= 0)
                {
                    string assetPath = normalized.Substring(assetsIdx + 1);
                    int lastSlash = assetPath.LastIndexOf('/');
                    assetDir = lastSlash >= 0 ? assetPath.Substring(0, lastSlash) : "Assets";
                }
                else
                {
                    assetDir = Path.GetDirectoryName(uitkxPath)?.Replace('\\', '/') ?? "";
                }

                foreach (Match m in s_ussDirectiveRe.Matches(content))
                    InjectIfResolved(assetDir, m.Groups[1].Value, "StyleSheet");

                foreach (Match m in s_assetCallRe.Matches(content))
                    InjectIfResolved(assetDir, m.Groups[2].Value, m.Groups[1].Value);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HMR] Asset cache sync failed: {ex.Message}");
            }
        }

        // ── Image extensions handled by TextureImporter ────────────────────────

        private static readonly HashSet<string> s_imageExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".tga",
            ".psd",
            ".gif",
            ".tif",
            ".tiff",
            ".exr",
            ".hdr",
        };

        private static void InjectIfResolved(string uitkxDir, string rawPath, string requestedType)
        {
            string resolved;
            if (rawPath.StartsWith("./") || rawPath.StartsWith("../"))
            {
                string combined = uitkxDir + "/" + rawPath;
                var parts = combined.Replace('\\', '/').Split('/');
                var stack = new List<string>();
                foreach (var p in parts)
                {
                    if (p == "." || p == "")
                        continue;
                    if (p == ".." && stack.Count > 0)
                        stack.RemoveAt(stack.Count - 1);
                    else if (p != "..")
                        stack.Add(p);
                }
                resolved = string.Join("/", stack);
            }
            else
            {
                resolved = rawPath;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resolved);

            // If the asset isn't in the database but exists on disk, import it first.
            // This handles files copied into the project during HMR (assemblies are
            // locked so auto-refresh is suppressed).
            if (asset == null)
            {
                string projectRoot = Application.dataPath;
                if (projectRoot.EndsWith("/Assets") || projectRoot.EndsWith("\\Assets"))
                    projectRoot = projectRoot.Substring(0, projectRoot.Length - 6);
                string diskPath = Path.Combine(
                    projectRoot,
                    resolved.Replace('/', Path.DirectorySeparatorChar)
                );

                if (File.Exists(diskPath))
                {
                    AssetDatabase.ImportAsset(resolved, ImportAssetOptions.ForceSynchronousImport);
                    asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resolved);
                }
            }

            if (asset == null)
                return;

            // ── Auto-configure importer when requested type doesn't match ─────
            string ext = Path.GetExtension(resolved);

            if (s_imageExtensions.Contains(ext))
            {
                asset = ConfigureTextureImport(resolved, asset, requestedType);
            }

            if (asset != null)
                UitkxAssetRegistry.InjectCacheEntry(resolved, asset);
        }

        /// <summary>
        /// Ensures a texture asset's importer matches the requested type.
        /// If <paramref name="requestedType"/> is <c>Sprite</c> but the file is
        /// imported as <c>Texture2D</c> (or vice-versa), reconfigures the
        /// <see cref="TextureImporter"/> and reimports synchronously.
        /// </summary>
        private static UnityEngine.Object ConfigureTextureImport(
            string assetPath,
            UnityEngine.Object currentAsset,
            string requestedType
        )
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return currentAsset;

            if (string.Equals(requestedType, "Sprite", StringComparison.Ordinal))
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                }
                // Load the Sprite sub-asset (created by sprite import)
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                return sprite != null ? sprite : currentAsset;
            }

            if (string.Equals(requestedType, "Texture2D", StringComparison.Ordinal))
            {
                if (importer.textureType == TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Default;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath) ?? currentAsset;
                }
                // Already a Texture2D-compatible import
                return currentAsset;
            }

            // Other requested types on image files — return as-is
            return currentAsset;
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

        // ── Safe domain reload (Play-mode aware) ──────────────────────────────

        // Set while a domain reload has been requested but is being held until
        // the editor exits Play mode. Idempotent: multiple rude edits in the
        // same Play session coalesce into a single deferred reload.
        private bool _pendingReloadOnEditMode;
        private string _pendingReloadReason;

        /// <summary>
        /// Routes a script-reload request through a Play-mode guard.
        /// Firing <c>RequestScriptReload</c> while the editor is in Play mode
        /// produces a partial domain reload that leaves Play-mode
        /// MonoBehaviours with broken script references (observed as
        /// "The referenced script (Unknown) on this Behaviour is missing!").
        /// If we are in Play mode (or transitioning), the request is held
        /// until <c>PlayModeStateChange.EnteredEditMode</c> fires. Otherwise
        /// it dispatches via <c>delayCall</c> so the current HMR cycle
        /// completes cleanly first.
        /// </summary>
        private void RequestDomainReloadSafe(string reason)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (_pendingReloadOnEditMode)
                    return;
                _pendingReloadOnEditMode = true;
                _pendingReloadReason = reason;
                EditorApplication.playModeStateChanged += OnPlayModeChangedForDeferredReload;
                Debug.Log("[HMR] Domain reload deferred until exit of Play mode " + $"({reason}).");
                return;
            }
            string captured = reason;
            EditorApplication.delayCall += () => SafeRequestScriptReload(captured);
        }

        private void OnPlayModeChangedForDeferredReload(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            EditorApplication.playModeStateChanged -= OnPlayModeChangedForDeferredReload;
            string reason = _pendingReloadReason;
            _pendingReloadOnEditMode = false;
            _pendingReloadReason = null;
            EditorApplication.delayCall += () => SafeRequestScriptReload(reason);
        }

        private static void SafeRequestScriptReload(string reason)
        {
            try
            {
                Debug.Log($"[HMR] Requesting domain reload ({reason}).");
                UnityEditor.EditorUtility.RequestScriptReload();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HMR] RequestScriptReload failed: {ex.Message}");
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
