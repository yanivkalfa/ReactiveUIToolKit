// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// namespace ReactiveUITK.Bench
// {
//     [Serializable]
//     public class BenchEnvironment
//     {
//         public string unityVersion;
//         public string productName;
//         public string platform;
//         public string graphicsDevice;
//         public string deviceModel;
//         public string deviceName;
//         public int screenWidth;
//         public int screenHeight;
//         public int systemMemoryMB;
//         public bool isEditor;
//         public bool isDevelopmentBuild;
//     }

//     [Serializable]
//     public class BenchSample
//     {
//         public double t;           // seconds since scenario start
//         public float dt;           // frame delta
//         public float fps;          // 1/dt
//         public long monoUsed;      // Mono heap used (if available)
//         public long gcTotal;       // GC total managed (approx)
//         public int  gcCount;       // GC.CollectionCount(0)
//     }

//     [Serializable]
//     public class BenchScenarioResult
//     {
//         public int index;
//         public string name;
//         public double startedAt;         // unix epoch (ms)
//         public double endedAt;           // unix epoch (ms)
//         public List<BenchSample> samples = new List<BenchSample>();

//         // Aggregates for easy plotting without parsing whole samples
//         public float fpsAvg;
//         public float fpsP95;
//         public float fpsMin;
//         public float fpsMax;
//         public long monoUsedMax;
//         public long gcTotalMax;
//     }

//     [Serializable]
//     public class BenchRun
//     {
//         public string runId;             // timestamped
//         public string suite;             // e.g. "RUITK Bench"
//         public BenchEnvironment env;
//         public List<BenchScenarioResult> scenarios = new List<BenchScenarioResult>();
//     }

//     public static class BenchRunCollector
//     {
//         private static BenchRun _run;
//         private static BenchScenarioResult _cur;
//         private static double _scenarioStartTime;
//         private static double _lastTime;
//         private static int _lastGC;
//         private static bool _enabled;

//         // ============== Public API ==============

//         public static void BeginRun(string suiteName)
//         {
//             _enabled = true;
//             _run = new BenchRun
//             {
//                 runId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
//                 suite = suiteName,
//                 env = CaptureEnvironment()
//             };
//         }

//         public static void BeginScenario(int index, string name)
//         {
//             if (!_enabled) return;
//             EndScenario(); // close previous if any

//             _cur = new BenchScenarioResult
//             {
//                 index = index,
//                 name = name,
//                 startedAt = NowMs()
//             };
//             _run.scenarios.Add(_cur);

//             _scenarioStartTime = NowSec();
//             _lastTime = _scenarioStartTime;
//             _lastGC = GC.CollectionCount(0);
//         }

//         public static void SampleFrame()
//         {
//             if (!_enabled || _cur == null) return;

//             // time
//             double now = NowSec();
//             float dt = (float)Math.Max(1e-6, now - _lastTime);
//             _lastTime = now;

//             // stats
//             int gcCount = GC.CollectionCount(0);
//             long monoUsed = GetMonoUsedSize();
//             long gcTotal  = GetGcTotalMemory();

//             var s = new BenchSample
//             {
//                 t = now - _scenarioStartTime,
//                 dt = dt,
//                 fps = 1f / dt,
//                 monoUsed = monoUsed,
//                 gcTotal = gcTotal,
//                 gcCount = gcCount
//             };
//             _cur.samples.Add(s);
//         }

//         public static void EndScenario()
//         {
//             if (!_enabled || _cur == null) return;

//             _cur.endedAt = NowMs();

//             if (_cur.samples.Count > 0)
//             {
//                 var fps = _cur.samples.Select(x => x.fps).ToArray();
//                 Array.Sort(fps);
//                 _cur.fpsAvg = (float)_cur.samples.Average(x => x.fps);
//                 _cur.fpsP95 = fps[(int)Math.Floor(0.95 * (fps.Length - 1))];
//                 _cur.fpsMin = fps.First();
//                 _cur.fpsMax = fps.Last();
//                 _cur.monoUsedMax = _cur.samples.Max(x => x.monoUsed);
//                 _cur.gcTotalMax  = _cur.samples.Max(x => x.gcTotal);
//             }

//             _cur = null;
//         }

//         public static void EndRun(bool writeFiles = true, bool logSummary = true, string preferredName = null)
//         {
//             if (!_enabled) return;
//             EndScenario();

//             if (logSummary)
//                 LogSummaryToConsole();

//             if (writeFiles)
//                 WriteArtifacts(preferredName);

//             _enabled = false;
//             _run = null;
//             _cur = null;
//         }

//         // ============== Helpers ==============

//         private static BenchEnvironment CaptureEnvironment()
//         {
//             var env = new BenchEnvironment
//             {
//                 unityVersion = Application.unityVersion,
//                 productName = Application.productName,
//                 platform = Application.platform.ToString(),
//                 graphicsDevice = SystemInfo.graphicsDeviceName,
//                 deviceModel = SystemInfo.deviceModel,
//                 deviceName = SystemInfo.deviceName,
//                 screenWidth = Screen.width,
//                 screenHeight = Screen.height,
//                 systemMemoryMB = SystemInfo.systemMemorySize,
//                 isEditor = Application.isEditor,
// #if DEVELOPMENT_BUILD
//                 isDevelopmentBuild = true
// #else
//                 isDevelopmentBuild = false
// #endif
//             };
//             return env;
//         }

//         private static void LogSummaryToConsole()
//         {
//             if (_run == null) return;

//             Debug.Log($"[Bench] Run {_run.runId} — {_run.suite} — Env: " +
//                       $"{_run.env.platform} {( _run.env.isEditor ? "(Editor)" : "(Runtime)")}, " +
//                       $"{_run.env.unityVersion}, { _run.env.graphicsDevice }, { _run.env.screenWidth}x{_run.env.screenHeight}");

//             foreach (var s in _run.scenarios)
//             {
//                 Debug.Log($"[Bench]  #{s.index:00} {s.name}  |  fps avg={s.fpsAvg:F1} p95={s.fpsP95:F1} min={s.fpsMin:F1} max={s.fpsMax:F1}  " +
//                           $"monoUsedMax={FormatBytes(s.monoUsedMax)}  gcMax={FormatBytes(s.gcTotalMax)}  samples={s.samples.Count}");
//             }
//         }

//         private static void WriteArtifacts(string preferredName)
//         {
//             string root, folder;
// #if UNITY_EDITOR
//             root = Path.Combine(Application.dataPath, "ReactiveUIToolKit", "Samples", "benchmark", "results_editor");
// #else
//             root = Path.Combine(Application.persistentDataPath, "benchmark_results_runtime");
// #endif
//             folder = Path.Combine(root, preferredName ?? _run.runId);
//             Directory.CreateDirectory(folder);

//             // JSON (full run)
//             var jsonPath = Path.Combine(folder, "run.json");
//             var json = JsonUtility.ToJson(_run, prettyPrint: true);
//             File.WriteAllText(jsonPath, json);
// #if UNITY_EDITOR
//             AssetDatabase.Refresh();
// #endif
//             Debug.Log($"[Bench] Wrote JSON: {jsonPath}");

//             // CSV (one per scenario, easy graphing)
//             for (int i = 0; i < _run.scenarios.Count; i++)
//             {
//                 var s = _run.scenarios[i];
//                 var nameSafe = Sanitize($"{i:00}_{s.name}");
//                 var csvPath = Path.Combine(folder, $"{nameSafe}.csv");
//                 using (var sw = new StreamWriter(csvPath))
//                 {
//                     sw.WriteLine("t,dt,fps,monoUsed,gcTotal,gcCount");
//                     foreach (var sm in s.samples)
//                         sw.WriteLine($"{sm.t:F6},{sm.dt:F6},{sm.fps:F3},{sm.monoUsed},{sm.gcTotal},{sm.gcCount}");
//                 }
// #if UNITY_EDITOR
//                 AssetDatabase.Refresh();
// #endif
//                 Debug.Log($"[Bench] Wrote CSV: {csvPath}");
//             }
//         }

//         private static long GetMonoUsedSize()
//         {
// #if UNITY_2020_2_OR_NEWER
//             return UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
// #else
//             return 0;
// #endif
//         }

//         private static long GetGcTotalMemory()
//         {
//             try { return GC.GetTotalMemory(forceFullCollection: false); }
//             catch { return 0; }
//         }

//         private static string FormatBytes(long b)
//         {
//             if (b <= 0) return "0";
//             string[] units = { "B", "KB", "MB", "GB" };
//             int u = (int)Math.Min(units.Length - 1, Math.Log(b, 1024));
//             double val = b / Math.Pow(1024, u);
//             return $"{val:F1}{units[u]}";
//         }

//         private static string Sanitize(string name)
//         {
//             foreach (var c in Path.GetInvalidFileNameChars())
//                 name = name.Replace(c, '_');
//             return name;
//         }

//         private static double NowSec()
//         {
// #if UNITY_EDITOR
//             // Works in editor windows even when Time.time is not advancing
//             return EditorApplication.timeSinceStartup;
// #else
//             return Time.realtimeSinceStartupAsDouble;
// #endif
//         }

//         private static double NowMs() => (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
//     }
// }
