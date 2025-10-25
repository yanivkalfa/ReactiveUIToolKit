// Samples/benchmark/BenchmarkSetup.cs
using System;
using System.Reflection;
using UnityEngine;
using ReactiveUITK.Bench;
using ReactiveUITK.Core;
using ReactiveUITK.Samples.Shared;

namespace ReactiveUITK.Benchmark
{
    [DefaultExecutionOrder(-1000)]
    public sealed class BenchmarkSetup : MonoBehaviour
    {
        private void Awake()
        {
          
            try
            {
              BenchSharedHost.SharedDemoRenderer = () => V.Func(SharedDemoPage.Render);
              Debug.Log("[BenchEditorHost] SharedDemo hook set.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BenchmarkSetup] Failed to set SharedDemo hook: " + e.Message);
            }
        }
    }
}
