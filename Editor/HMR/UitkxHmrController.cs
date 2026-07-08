using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
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

        // ── Compile queue ────────────────────────────────────────────────────
        // Real FIFO queue (replaces the prior single-slot _queuedPath design,
        // which was effectively dead code — _compilationQueued was never set
        // to true and the single slot dropped concurrent saves). Used by the
        // dep-graph cascade (Rank 3 / Rank 4) which can produce N-file batches
        // that MUST drain in order so each consumer's cctor runs after its
        // dependency cctor produced the new value.
        //
        // Invariants:
        //   • At most one ProcessFileChange call is in flight (DrainQueue
        //     re-enters via EditorApplication.delayCall, never recurses).
        //   • Each path appears at most once in the queue at any time
        //     (deduped via _enqueued set).
        private readonly Queue<string> _compileQueue = new();
        private readonly HashSet<string> _enqueued = new(StringComparer.OrdinalIgnoreCase);
        private bool _compileInFlight;

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
            _watcher.OnUitkxDeleted += OnUitkxFileDeleted;
            _watcher.Start(assetsPath);

            // Seed the workspace-wide hook-container registry so HMR recompiles
            // can resolve cross-directory `using static <Ns>.<HookContainer>;`
            // directives without scanning the project on every recompile. This
            // mirrors what the SG's UitkxGenerator pre-scan does at compile
            // time. The seed runs on a background thread; the first recompile
            // gates briefly via TryWaitForSeed.
            HookContainerRegistry.Seed(assetsPath);

            // UITKX Fast Refresh: wire the renderer-walk callback the
            // Refresh runtime needs to dispatch PerformRefresh. Doing it
            // here (instead of [InitializeOnLoadMethod]) keeps the editor
            // load path lazy — the provider is only set when HMR actually
            // starts.
            global::ReactiveUITK.Refresh.RefreshRuntime.RegisterRootRendererProvider(
                EnumerateRootFibers);

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
            _watcher.OnUitkxDeleted -= OnUitkxFileDeleted;
            _watcher.Stop();

            // Drop the workspace-wide hook-container index; a fresh seed runs
            // on the next Start.
            HookContainerRegistry.Reset();

            _pendingRetryPaths.Clear();
            _compileQueue.Clear();
            _enqueued.Clear();
            _compileInFlight = false;
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

            // If this is a companion .uitkx file (e.g. Foo.style.uitkx),
            // redirect to compile the parent component file (Foo.uitkx)
            // so the companion's module/hook members are included.
            uitkxPath = ResolveParentComponentFile(uitkxPath);

            // ── UITKX Fast Refresh ─────────────────────────────────────────
            // No cascade walker, no transitive consumer compile. The Family
            // handle indirection means a single Register call — emitted by
            // the [ModuleInitializer] in the freshly compiled assembly —
            // reaches every consumer regardless of whether their IL was
            // recompiled in this round. See Plans~/HMR_FAST_REFRESH_PLAN.md.
            EnqueueCompile(uitkxPath);
            DrainCompileQueueIfIdle();
        }

        // ── Compile queue plumbing ────────────────────────────────────────────

        // ── UITKX Fast Refresh: root-fiber enumeration ───────────────────────
        // Supplied to RefreshRuntime.RegisterRootRendererProvider in Start().
        // Walks the two registries that hold every live FiberRenderer:
        //   • EditorRootRendererUtility.GetAllRenderers — editor-window hosts
        //   • RootRenderer.AllInstances — runtime MonoBehaviour hosts
        // Mirrors the iteration the (now deleted) component trampoline
        // swapper used. Cheap to call: O(renderers); RefreshRuntime calls
        // it once per HMR cycle.
        private static IEnumerable<FiberNode> EnumerateRootFibers()
        {
            foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
            {
                var fr = renderer?.FiberRendererInternal;
                if (fr?.Root?.Current != null)
                    yield return fr.Root.Current;
            }
            foreach (var rootRenderer in RootRenderer.AllInstances)
            {
                var vhr = rootRenderer?.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current != null)
                    yield return vhr.FiberRendererInternal.Root.Current;
            }
        }

        private void EnqueueCompile(string uitkxPath)
        {
            if (string.IsNullOrEmpty(uitkxPath))
                return;
            if (_enqueued.Add(uitkxPath))
                _compileQueue.Enqueue(uitkxPath);
        }

        private void DrainCompileQueueIfIdle()
        {
            if (_compileInFlight)
                return; // a delayCall is already pumping
            if (_compileQueue.Count == 0)
                return;

            _compileInFlight = true;
            try
            {
                // UITKX Fast Refresh — every save is a single-file compile.
                // The cascade walker and union-compile batch path were
                // removed when Family-handle indirection eliminated the
                // cross-DLL identity bug that those mechanisms existed to
                // work around (see Plans~/HMR_FAST_REFRESH_PLAN.md).
                //
                // H-02: dequeue exactly ONE item per invocation, not the whole
                // queue in a single editor tick. A shared .uss edited by many
                // components fans out to N queued compiles (see
                // OnUssFileChanged) — draining all N synchronously here froze
                // the editor for the duration of N Roslyn compiles. The
                // delayCall tail below already re-invokes this method on the
                // next tick whenever the queue is non-empty, so compiles still
                // land one per tick without blocking the frame.
                if (_compileQueue.Count > 0)
                {
                    string next = _compileQueue.Dequeue();
                    _enqueued.Remove(next);
                    ProcessFileChange(next);
                }
            }
            finally
            {
                _compileInFlight = false;
            }

            // Continue draining on the next editor tick if new saves arrived
            // while we were compiling (debounced file events route through
            // OnUitkxFileChanged → EnqueueCompile → DrainCompileQueueIfIdle).
            if (_compileQueue.Count > 0)
                EditorApplication.delayCall += DrainCompileQueueIfIdle;
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
                ApplySuccessfulCompileResult(result, uitkxPath);

                // Remove from pending retries if this was a re-attempt
                _pendingRetryPaths.Remove(uitkxPath);

                // A successful compilation may unblock pending failures that
                // depend on this component. Schedule a retry pass on the next frame.
                if (_pendingRetryPaths.Count > 0 && !_retryingPending)
                    EditorApplication.delayCall += RetryPendingCompilations;
            }
            else
            {
                HandleCompileFailure(result, uitkxPath);
            }

            // The compile queue (set by OnUitkxFileChanged + cascade walker)
            // is drained by DrainCompileQueueIfIdle on subsequent editor ticks;
            // we don't re-queue from here. The historical single-slot
            // _compilationQueued / _queuedPath fields were dead code and have
            // been replaced by the real FIFO queue declared near the top of
            // this class.
        }

        // ── Extracted swap pipeline (shared by single-file and batch flows) ──

        /// <summary>
        /// Run the post-compile swap pipeline for a single successful result:
        /// module-static + module-method re-init, then component-family
        /// PerformRefresh (or hook delegate swap for hook/module files).
        /// </summary>
        private void ApplySuccessfulCompileResult(
            HmrCompileResult result, string uitkxPath)
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
                // Hook/module files: update static delegate fields + global re-render.
                // Namespace is carried through HmrCompileResult so we don't probe via
                // GetTypes().FirstOrDefault() (which picks Roslyn's embedded
                // Microsoft.CodeAnalysis.EmbeddedAttribute due to metadata ordering).
                string ns = result.Namespace;
                if (string.IsNullOrEmpty(ns))
                {
                    try
                    {
                        var containerType = result.LoadedAssembly.GetTypes()
                            .FirstOrDefault(t =>
                                t.Name == result.HookContainerClass
                                || t.Name == result.ComponentName);
                        ns = containerType?.Namespace;
                    }
                    catch { }
                }

                // Only swap hook delegates when the file actually declared
                // hooks. A module-ONLY file has no hook container, so calling
                // SwapHooks would just log a spurious "Could not find hook
                // container" warning and no-op.
                int hookSwaps = result.HasHooks
                    ? UitkxHmrDelegateSwapper.SwapHooks(
                        result.LoadedAssembly,
                        result.HookContainerClass,
                        ns
                    )
                    : 0;

                // Module-method edits: SwapModuleMethods (above) rebound the
                // static trampoline delegates, but loaded consumers still hold
                // render output produced by the OLD method bodies. SwapHooks
                // fires the global re-render on its own success; a module-only
                // file never runs it, so fire it here. Without this, a module
                // method edit only becomes visible after an unrelated re-render
                // (a consumer state change, or the next game-loop frame).
                if (reInitedMethods > 0 && hookSwaps == 0)
                    UitkxHmrDelegateSwapper.TriggerGlobalReRender();

                swapped = hookSwaps;

                // Drain Phase 1/3 dirty + force-remount queues populated by
                // the hook's RegisterHook ModuleInitializer (just kicked by
                // ForceRunModuleInitializers). PropagateHookSignatureChanges
                // walks s_reverseEdges to fan force-remount out to every
                // consumer; without this call, hook signature changes never
                // remount their consumers and useRef / useState state from
                // before the edit lingers across a signature change. The
                // delegate-swap re-render (TriggerGlobalReRender) is what
                // picks up body-only edits; PerformRefresh is what enforces
                // the state-reset for signature edits. Both run; the second
                // call is cheap when nothing is dirty.
                int refreshed = global::ReactiveUITK.Refresh.RefreshRuntime.PerformRefresh();
                if (refreshed > swapped)
                    swapped = refreshed;

                // Surface a module-only swap in the [HMR] notification even when
                // no hook delegates or component families reported live
                // instances (the change was applied via the global re-render).
                if (reInitedMethods > 0 && swapped == 0)
                    swapped = reInitedMethods;
            }
            else
            {
                // Component files: under the Family architecture, the
                // freshly compiled assembly's [ModuleInitializer] has
                // already run -- forced deterministically by
                // ForceRunModuleInitializers right after Assembly.LoadFrom
                // (the CLR fires <Module>.cctor lazily on first member
                // access, which never happens for the synthetic companion
                // type that carries [ModuleInitializer], so we kick it
                // explicitly via RuntimeHelpers.RunModuleConstructor).
                // Register has therefore atomically updated the Family's
                // Current delegate. PerformRefresh now walks live renderer
                // trees once, schedules a re-render for every fiber whose
                // Family was just updated, and resets state for fibers
                // whose hook signature changed. Single tree walk -- not
                // per-component-type.
                swapped = global::ReactiveUITK.Refresh.RefreshRuntime.PerformRefresh();
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
        }

        /// <summary>
        /// Handle the failure branch of a per-file compile result (infrastructure
        /// detection, dependency auto-resolve, retry-queue maintenance). Extracted
        /// to keep ProcessFileChange small enough to reason about and so the
        /// batch fallback flow can route failures uniformly through here.
        /// </summary>
        private void HandleCompileFailure(HmrCompileResult result, string uitkxPath)
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

            // If the file no longer exists (renamed away or deleted while
            // a stale event was in flight), don't enqueue a retry.
            if (!File.Exists(uitkxPath))
            {
                _pendingRetryPaths.Remove(uitkxPath);
                return;
            }
            _pendingRetryPaths[uitkxPath] = result.Error;
        }

        // ── Deletion handler ──────────────────────────────────────────────────

        /// <summary>
        /// Invoked synchronously on the main thread by the watcher when a
        /// .uitkx file is deleted or renamed away. Evicts the path from the
        /// pending-retry queue so subsequent <see cref="RetryPendingCompilations"/>
        /// passes don't try to re-read a file that no longer exists. Without
        /// this, a copy-rename-edit cycle (the user creates a new component
        /// by cloning an existing one) would leave a stale entry that throws
        /// FileNotFoundException on every retry pass forever.
        /// See Plans~/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md §4.4.
        /// </summary>
        private void OnUitkxFileDeleted(string uitkxPath)
        {
            if (!_active)
                return;
            if (_pendingRetryPaths.Remove(uitkxPath))
            {
                Debug.Log(
                    $"[HMR] {Path.GetFileName(uitkxPath)} no longer exists — "
                        + "removed from retry queue."
                );
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
        // H-03: asset-path resolution previously disagreed across four consumers
        // (this method treated a bare path as project-root-relative; InjectIfResolved
        // below left it unresolved; the editor's DiagnosticsAnalyzer and the
        // SourceGenerator's CSharpEmitter both resolve it uitkx-dir-relative). The
        // canonical rule now lives once in language-lib's AssetPathUtil; Editor/HMR
        // cannot reference that assembly directly (its asmdef only references
        // ReactiveUITK.Shared/ReactiveUITK.Runtime — language-lib is consumed via
        // reflection against the committed analyzer DLL elsewhere in this feature).
        // HmrAssetPathUtil is a byte-for-byte mirror of AssetPathUtil's algorithm —
        // if you change one, change the other (see FINAL_AUDIT_UITKX_FINDINGS.md H-03).
        internal static class HmrAssetPathUtil
        {
            public static string ResolveAssetPath(string uitkxDir, string rawPath)
            {
                if (string.IsNullOrEmpty(rawPath))
                    return rawPath;

                if (rawPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                    rawPath.StartsWith("Packages/", StringComparison.Ordinal))
                    return rawPath;

                string combined = string.IsNullOrEmpty(uitkxDir) ? rawPath : uitkxDir + "/" + rawPath;
                var parts = combined.Replace('\\', '/').Split('/');
                var stack = new List<string>();
                foreach (var p in parts)
                {
                    if (p == "." || p == "") continue;
                    if (p == ".." && stack.Count > 0)
                        stack.RemoveAt(stack.Count - 1);
                    else if (p != "..")
                        stack.Add(p);
                }
                return string.Join("/", stack);
            }

            public static string GetAssetDir(string filePath)
            {
                string normalized = (filePath ?? string.Empty).Replace('\\', '/');
                int assetsIdx = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
                if (assetsIdx < 0)
                {
                    if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        int lastSlash = normalized.LastIndexOf('/');
                        return lastSlash > 0 ? normalized.Substring(0, lastSlash) : "Assets";
                    }
                    return Path.GetDirectoryName(filePath)?.Replace('\\', '/') ?? "";
                }

                string assetPath = normalized.Substring(assetsIdx + 1);
                int dirSlash = assetPath.LastIndexOf('/');
                return dirSlash >= 0 ? assetPath.Substring(0, dirSlash) : "Assets";
            }
        }

        private void RegisterUssDependencies(string uitkxPath)
        {
            try
            {
                string content = File.ReadAllText(uitkxPath);
                string uitkxDir = HmrAssetPathUtil.GetAssetDir(uitkxPath);
                string projectRoot = Path.GetFullPath(
                    Path.Combine(UnityEngine.Application.dataPath, "..")
                );

                foreach (Match m in s_ussDirectiveRe.Matches(content))
                {
                    string rawPath = m.Groups[1].Value;
                    string resolved = HmrAssetPathUtil.ResolveAssetPath(uitkxDir, rawPath);
                    string absoluteUss = Path.GetFullPath(Path.Combine(projectRoot, resolved));

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
                // H-06: this is a direct (re-entrant) ProcessFileChange call, not routed
                // through the compile queue — it technically violates the "at most one
                // ProcessFileChange in flight" invariant documented above (this method is
                // itself only ever reached from within an in-flight ProcessFileChange).
                // Safe today because auto-discovered dependencies are compiled
                // synchronously one at a time right here (no concurrent queue drain can
                // interleave); revisit if this path is ever made to fan out concurrently.
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
                string assetDir = HmrAssetPathUtil.GetAssetDir(uitkxPath);

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
            // H-03: bare paths ("styles.uss", no "./" prefix) previously passed through
            // unresolved here — see HmrAssetPathUtil's doc comment above RegisterUssDependencies.
            string resolved = HmrAssetPathUtil.ResolveAssetPath(uitkxDir, rawPath);

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
