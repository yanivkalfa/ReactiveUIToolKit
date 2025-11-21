using System;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER
using Unity.Profiling;
#endif

namespace ReactiveUITK.Bench
{
    internal sealed class BenchMetrics
    {
        private const int MaxFrames = 60000;
        private int count;
        private double sumDt;
        private double minDt = double.MaxValue;
        private double maxDt = 0;
        private readonly double[] samples = new double[MaxFrames];

#if UNITY_2022_2_OR_NEWER
        private ProfilerRecorder gcAllocRecorder;
#endif

        public void Begin()
        {
            count = 0;
            sumDt = 0;
            minDt = double.MaxValue;
            maxDt = 0;
            Array.Clear(samples, 0, samples.Length);
#if UNITY_2022_2_OR_NEWER
            try
            {
                gcAllocRecorder = ProfilerRecorder.StartNew(
                    ProfilerCategory.Memory,
                    "GC Allocated In Frame"
                );
            }
            catch { }
#endif
        }

        public void End()
        {
#if UNITY_2022_2_OR_NEWER
            if (gcAllocRecorder.Valid)
            {
                gcAllocRecorder.Dispose();
            }
#endif
        }

        public void Sample(float deltaTime)
        {
            var dt = Math.Max(1e-6f, deltaTime);
            if (count < MaxFrames)
            {
                samples[count] = dt;
            }
            count++;
            sumDt += dt;
            if (dt < minDt)
            {
                minDt = dt;
            }
            if (dt > maxDt)
            {
                maxDt = dt;
            }
        }

        private static double Percentile(double[] arr, int n, double p)
        {
            if (n == 0)
            {
                return 0;
            }
            var idx = (int)Math.Clamp(Math.Round(p * (n - 1)), 0, n - 1);
            return arr[idx];
        }

        private double PercentileSorted(double p)
        {
            int n = Math.Min(count, samples.Length);
            Array.Sort(samples, 0, n);
            return Percentile(samples, n, p);
        }

        public string CsvHeader =>
            "name,duration,frames,avgDt,avgFPS,p95FPS,p99FPS,minFPS,maxFPS,gcAllocPerFrameBytes";

        public string ToCsvRow(string name, float durationSec)
        {
            var avgDt = sumDt / Math.Max(1, count);
            var avgFps = 1.0 / avgDt;
            var p95fps = 1.0 / PercentileSorted(0.95);
            var p99fps = 1.0 / PercentileSorted(0.99);
            long gc = 0;
#if UNITY_2022_2_OR_NEWER
            try
            {
                gc = (long)gcAllocRecorder.LastValue;
            }
            catch { }
#endif
            return $"{name},{durationSec},{count},{avgDt:F6},{avgFps:F2},{p95fps:F2},{p99fps:F2},{(1.0 / maxDt):F2},{(1.0 / minDt):F2},{gc}";
        }

        public string SummaryString()
        {
            var fps = 1.0 / (sumDt / Math.Max(1, count));
            var p95 = 1.0 / PercentileSorted(0.95);
            var p99 = 1.0 / PercentileSorted(0.99);
            long gc = 0;
#if UNITY_2022_2_OR_NEWER
            try
            {
                gc = (long)gcAllocRecorder.LastValue;
            }
            catch { }
#endif
            return $"avgFPS={fps:F1} p95FPS={p95:F1} p99FPS={p99:F1} minFPS={(1.0 / maxDt):F1} maxFPS={(1.0 / minDt):F1} GC/frame={gc} bytes";
        }
    }
}
