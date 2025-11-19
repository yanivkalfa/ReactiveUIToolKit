using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ReactiveUITK.Bench
{
    [Serializable]
    public class BenchEnv
    {
        public string unityVersion;
        public string productName;
        public string platform;
        public string graphicsDevice;
        public string deviceModel;
        public string deviceName;
        public int screenWidth;
        public int screenHeight;
        public int systemMemoryMB;
        public bool isEditor;
        public bool isDevelopmentBuild;
    }

    [Serializable]
    public class BenchScenarioHeader
    {
        public int index;
        public string name;
        public float durationSec;
        public double startedAt; 
        public double endedAt; 
    }

    [Serializable]
    public class BenchScenarioSummary
    {
        public float fpsAvg;
        public float fpsP95;
        public float fpsMin;
        public float fpsMax;
        public int samplesTotal;
        public int secondsTotal;
        public long monoUsedMax;
        public int gcCollectionsTotal;
    }

    [Serializable]
    public class BenchPerSecondBucket
    {
        public int sec; 
        public float fpsAvg;
        public float fpsMin;
        public float fpsMax;
        public int sampleCount;
        public long monoUsedMax;
        public int gcCollections; 
    }

    [Serializable]
    public class BenchScenarioFile
    {
        public string runId;
        public string suite;
        public BenchEnv env;
        public BenchScenarioHeader scenario;
        public BenchScenarioSummary scenarioSummary;
        public List<BenchPerSecondBucket> perSecond;
    }

    
    
    
    
    public static class BenchPerSecondLogger
    {
        private static string _runId;
        private static string _suite;
        private static BenchEnv _env;

        
        private static int _idx;
        private static string _name;
        private static float _durationSec;
        private static double _secStartEpoch;
        private static bool _active;

        private static List<BenchPerSecondBucket> _bins;
        private static int _lastSecondBin;
        private static int _samplesTotal;
        private static long _monoUsedMaxScenario;
        private static int _gcAtScenarioStart;

        
        private const int MaxPctlCache = 4096; 
        private static readonly float[] _pctlFps = new float[MaxPctlCache];
        private static int _pctlCount;

        
        private static float _accFpsSum;
        private static int _accFpsCount;
        private static float _accFpsMin;
        private static float _accFpsMax;
        private static long _accMonoUsedMax;
        private static int _accGcStart;

        private static BenchOutputTarget _forcedOutput = BenchOutputTarget.Auto;

        
#if UNITY_EDITOR
        private static double NowSec() => EditorApplication.timeSinceStartup;
#else
        private static double NowSec() => Time.realtimeSinceStartupAsDouble;
#endif

        private static double EpochNowSec() => (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

        public static void BeginRun(
            string suite,
            BenchEnvOverrides? overrides = null,
            BenchOutputTarget outputTarget = BenchOutputTarget.Auto
        )
        {
            _runId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _suite = suite;

            var ov = overrides ?? default;

            _env = new BenchEnv
            {
                unityVersion = Application.unityVersion,
                productName = string.IsNullOrEmpty(ov.productName)
                    ? Application.productName
                    : ov.productName,
                platform = string.IsNullOrEmpty(ov.platform)
                    ? Application.platform.ToString()
                    : ov.platform,
                graphicsDevice = string.IsNullOrEmpty(ov.graphicsDevice)
                    ? SystemInfo.graphicsDeviceName
                    : ov.graphicsDevice,
                deviceModel = string.IsNullOrEmpty(ov.deviceModel)
                    ? SystemInfo.deviceModel
                    : ov.deviceModel,
                deviceName = string.IsNullOrEmpty(ov.deviceName)
                    ? SystemInfo.deviceName
                    : ov.deviceName,
                screenWidth = ov.screenWidth ?? Screen.width,
                screenHeight = ov.screenHeight ?? Screen.height,
                systemMemoryMB = ov.systemMemoryMB ?? SystemInfo.systemMemorySize,
                isEditor = ov.isEditor ?? Application.isEditor,
#if DEVELOPMENT_BUILD
                isDevelopmentBuild = ov.isDevelopmentBuild ?? true
#else
                isDevelopmentBuild = ov.isDevelopmentBuild ?? false
#endif
            };

            _forcedOutput = outputTarget;
        }

        public static void BeginScenario(int index, string name, float durationSec)
        {
            _idx = index;
            _name = name ?? $"Scenario_{index}";
            _durationSec = durationSec;

            var seconds = Mathf.Max(1, Mathf.CeilToInt(durationSec));
            _bins = new List<BenchPerSecondBucket>(seconds);
            _lastSecondBin = -1;

            _samplesTotal = 0;
            _monoUsedMaxScenario = 0;
            _pctlCount = 0;

            _accFpsSum = 0f;
            _accFpsCount = 0;
            _accFpsMin = float.PositiveInfinity;
            _accFpsMax = float.NegativeInfinity;
            _accMonoUsedMax = 0;
            _accGcStart = GC.CollectionCount(0);

            _gcAtScenarioStart = _accGcStart;

            _secStartEpoch = EpochNowSec();
            _active = true;
        }

        public static void SampleFrame(float dt)
        {
            if (!_active)
            {
                return;
            }

            
            if (dt <= 0f)
            {
                dt = 1f / 60f;
            }
            var fps = 1f / dt;

            
            if (_pctlCount < MaxPctlCache)
            {
                _pctlFps[_pctlCount++] = fps;
            }

            
            var elapsed = NowSec() - 0.0; 
            
            
            var secBin = (int)Math.Floor(EpochNowSec() - _secStartEpoch);
            if (secBin < 0)
            {
                secBin = 0;
            }

            
            if (secBin != _lastSecondBin && _lastSecondBin >= 0)
            {
                FlushSecondBin(_lastSecondBin);
            }

            if (secBin != _lastSecondBin)
            {
                
                _accFpsSum = 0f;
                _accFpsCount = 0;
                _accFpsMin = float.PositiveInfinity;
                _accFpsMax = float.NegativeInfinity;
                _accMonoUsedMax = 0;
                _accGcStart = GC.CollectionCount(0);
                _lastSecondBin = secBin;
            }

            
            _accFpsSum += fps;
            _accFpsCount++;
            if (fps < _accFpsMin)
            {
                _accFpsMin = fps;
            }
            if (fps > _accFpsMax)
            {
                _accFpsMax = fps;
            }

#if UNITY_2020_2_OR_NEWER
            var monoUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
#else
            var monoUsed = 0L;
#endif
            if (monoUsed > _accMonoUsedMax)
            {
                _accMonoUsedMax = monoUsed;
            }
            if (monoUsed > _monoUsedMaxScenario)
            {
                _monoUsedMaxScenario = monoUsed;
            }

            _samplesTotal++;
        }

        public static void EndScenarioAndWriteFile()
        {
            if (!_active)
            {
                return;
            }

            
            if (_lastSecondBin >= 0)
            {
                FlushSecondBin(_lastSecondBin);
            }

            var endedEpoch = EpochNowSec();

            
            var scenarioFile = new BenchScenarioFile
            {
                runId = _runId,
                suite = _suite,
                env = _env,
                scenario = new BenchScenarioHeader
                {
                    index = _idx,
                    name = _name,
                    durationSec = _durationSec,
                    startedAt = _secStartEpoch,
                    endedAt = endedEpoch,
                },
                scenarioSummary = BuildScenarioSummary(),
                perSecond = _bins,
            };

            
            string folderName;
            switch (_forcedOutput)
            {
                case BenchOutputTarget.Editor:
                    folderName = "Editor";
                    break;
                case BenchOutputTarget.Runtime:
                    folderName = "Runtime";
                    break;
                default:
#if UNITY_EDITOR
                    folderName = "Editor";
#else
                    folderName = "Runtime";
#endif
                    break;
            }

            string root = Path.Combine(
                Application.dataPath,
                "ReactiveUIToolKit",
                "Diagnostics",
                "Benchmark",
                "Results",
                folderName,
                _runId
            );

            Directory.CreateDirectory(root);

            var fileName = $"{_idx:D2}_{Sanitize(_name)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var path = Path.Combine(root, fileName);

            var json = JsonUtility.ToJson(scenarioFile, prettyPrint: true);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log(
                $"[Bench] Wrote scenario JSON: {path}  (bins={_bins.Count}, samples={_samplesTotal})"
            );

            
            _active = false;
            _bins = null;
        }

        private static BenchScenarioSummary BuildScenarioSummary()
        {
            
            float fpsAvg = 0f,
                fpsMin = float.PositiveInfinity,
                fpsMax = float.NegativeInfinity;
            if (_pctlCount > 0)
            {
                
                var n = _pctlCount;
                var tmp = new float[n];
                Array.Copy(_pctlFps, tmp, n);
                Array.Sort(tmp);
                fpsMin = tmp[0];
                fpsMax = tmp[n - 1];

                double sum = 0;
                for (int i = 0; i < n; i++)
                {
                    sum += tmp[i];
                }
                fpsAvg = (float)(sum / n);

                int p95i = (int)Math.Floor(0.95 * (n - 1));
                var fpsP95 = tmp[p95i];

                return new BenchScenarioSummary
                {
                    fpsAvg = fpsAvg,
                    fpsP95 = fpsP95,
                    fpsMin = fpsMin,
                    fpsMax = fpsMax,
                    samplesTotal = _samplesTotal,
                    secondsTotal = _bins?.Count ?? 0,
                    monoUsedMax = _monoUsedMaxScenario,
                    gcCollectionsTotal = GC.CollectionCount(0) - _gcAtScenarioStart,
                };
            }

            
            return new BenchScenarioSummary
            {
                fpsAvg = 0,
                fpsP95 = 0,
                fpsMin = 0,
                fpsMax = 0,
                samplesTotal = 0,
                secondsTotal = _bins?.Count ?? 0,
                monoUsedMax = _monoUsedMaxScenario,
                gcCollectionsTotal = GC.CollectionCount(0) - _gcAtScenarioStart,
            };
        }

        private static void FlushSecondBin(int sec)
        {
            if (_accFpsCount <= 0)
            {
                
                _bins.Add(
                    new BenchPerSecondBucket
                    {
                        sec = sec,
                        fpsAvg = 0,
                        fpsMin = 0,
                        fpsMax = 0,
                        sampleCount = 0,
                        monoUsedMax = 0,
                        gcCollections = 0,
                    }
                );
                return;
            }

            var gcDelta = GC.CollectionCount(0) - _accGcStart;
            _bins.Add(
                new BenchPerSecondBucket
                {
                    sec = sec,
                    fpsAvg = _accFpsSum / _accFpsCount,
                    fpsMin = _accFpsMin,
                    fpsMax = _accFpsMax,
                    sampleCount = _accFpsCount,
                    monoUsedMax = _accMonoUsedMax,
                    gcCollections = Mathf.Max(0, gcDelta),
                }
            );
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "scenario";
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s.Replace(' ', '_');
        }
    }
}
