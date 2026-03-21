using System;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ReactiveUITK.Bench
{
    public static class BenchSharedHost
    {
        private static IVNodeHostRenderer _renderer;
        private static BenchMetrics _metrics;

        private static int _scenarioIndex;
        private static float _timer;
        private static Action _currentRender;
        private static string _currentName;
        private static float _currentDuration;

        public static Func<VirtualNode> SharedDemoRenderer;

        public static int CurrentIndex => _scenarioIndex;
        public static string CurrentName => _currentName;
        public static bool IsComplete { get; private set; }
        public static event Action OnAllScenariosComplete;

        private static long _lastRenderMs;
        private static int _lastWorkUnits;

#if UNITY_EDITOR
        private static double _lastEditorNow = -1;
#endif

        public static void Init(
            IVNodeHostRenderer renderer,
            BenchOutputTarget outputTarget = BenchOutputTarget.Auto,
            BenchEnvOverrides? overrides = null
        )
        {
            _renderer = renderer;
            _metrics = new BenchMetrics();

            var title =
                outputTarget == BenchOutputTarget.Editor
                    ? "ReactiveUITK Bench (Editor)"
                    : "ReactiveUITK Bench (Runtime)";
            BenchPerSecondLogger.BeginRun(title, overrides, outputTarget);

            IsComplete = false;
            FiberReconciler.MetricsEmitted += OnReconcilerMetrics;
            _scenarioIndex = -1;
            NextScenario();
        }

        public static void Tick()
        {
            if (_currentRender == null)
            {
                return;
            }

            long gcBefore = GC.GetTotalMemory(false);
            _lastRenderMs = 0;
            _lastWorkUnits = 0;
            _currentRender.Invoke();
            long gcAfter = GC.GetTotalMemory(false);
            long renderAlloc = Math.Max(0, gcAfter - gcBefore);

#if UNITY_EDITOR
            double now = EditorApplication.timeSinceStartup;
            if (_lastEditorNow < 0)
            {
                _lastEditorNow = now;
            }
            float dt = (float)(now - _lastEditorNow);
            _lastEditorNow = now;
            if (dt <= 0f || dt > 0.5f)
            {
                dt = 1f / 60f;
            }
#else
            float dt = Time.unscaledDeltaTime;
#endif
            _metrics.Sample(dt);
            _timer += dt;

            BenchPerSecondLogger.SampleFrame(dt, renderAlloc, _lastRenderMs, _lastWorkUnits);

            if (_timer >= _currentDuration)
            {
                FlushScenario();
                NextScenario();
            }
        }

        public static void SkipScenario()
        {
            FlushScenario();
            NextScenario();
        }

        private static void FlushScenario()
        {
            if (string.IsNullOrEmpty(_currentName))
            {
                return;
            }

            try
            {
                Debug.Log($"[Bench] {_currentName} => {_metrics.SummaryString()}");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Bench] Summary failed: " + e.Message);
            }
            finally
            {
                BenchPerSecondLogger.EndScenarioAndWriteFile();
                _metrics.End();
            }
        }

        private static void NextScenario()
        {
            _timer = 0f;
#if UNITY_EDITOR
            _lastEditorNow = -1;
#endif
            _scenarioIndex++;

            if (_scenarioIndex >= BenchConfig.Default.Count)
            {
                _currentRender = null;
                _currentName = null;
                IsComplete = true;
                FiberReconciler.MetricsEmitted -= OnReconcilerMetrics;
                Debug.Log("[Bench] All scenarios done.");
                OnAllScenariosComplete?.Invoke();
                return;
            }

            var def = BenchConfig.Default[_scenarioIndex];
            _currentName = def.Name;
            _currentDuration = def.DurationSec;
            _currentRender = BenchScenarios.Build(def.Name);

            _metrics.Begin();

            BenchPerSecondLogger.BeginScenario(_scenarioIndex, _currentName, _currentDuration);

            Debug.Log($"[Bench] Start: {_currentName} ({_currentDuration:F1}s)");
        }

        public static void Render(VirtualNode vnode)
        {
            _renderer?.Render(vnode);
        }

        private static void OnReconcilerMetrics(FiberReconciler.FiberReconcilerMetrics m)
        {
            _lastRenderMs = m.LastRenderMs;
            _lastWorkUnits = m.WorkUnits;
        }
    }
}
