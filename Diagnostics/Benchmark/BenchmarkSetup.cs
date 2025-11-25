using System;
using System.Reflection;
using ReactiveUITK.Bench;
using ReactiveUITK.Core;
using ReactiveUITK.Samples.Shared;
using UnityEngine;

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
