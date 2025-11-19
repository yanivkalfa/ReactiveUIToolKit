using System;
using System.IO;
using UnityEngine;

namespace ReactiveUITK.Core.Config
{
    public sealed class ReactiveUITKConfig
    {
        [Serializable]
        private sealed class EnvVariables
        {
            public string env;
            public string traceLevel;
            public bool diffTracing;
            public bool exceptionControlFlow;
        }

        [Serializable]
        private sealed class Root
        {
            public EnvVariables envVariables;
        }

        public string EnvironmentLabel { get; private set; } = "production";
        public ReactiveUITK.Core.Reconciler.DiffTraceLevel TraceLevel { get; private set; } =
            ReactiveUITK.Core.Reconciler.DiffTraceLevel.None;
        public bool EnableDiffTracing { get; private set; } = false;
        public bool UseExceptionBoundaryFlow { get; private set; } = false;

        private static ReactiveUITKConfig instance;
        public static ReactiveUITKConfig Current
        {
            get
            {
                if (instance == null)
                {
                    instance = Load();
                }
                return instance;
            }
        }

        private static ReactiveUITKConfig Load()
        {
            var cfg = new ReactiveUITKConfig();
            try
            {
                string candidate = GetDefaultProjectConfigPath();
                if (File.Exists(candidate))
                {
                    string json = File.ReadAllText(candidate);
                    var parsed = JsonUtility.FromJson<Root>(json);
                    if (parsed != null && parsed.envVariables != null)
                    {
                        if (!string.IsNullOrEmpty(parsed.envVariables.env))
                        {
                            cfg.EnvironmentLabel = parsed.envVariables.env;
                        }
                        if (!string.IsNullOrEmpty(parsed.envVariables.traceLevel))
                        {
                            cfg.TraceLevel = ParseTraceLevel(parsed.envVariables.traceLevel);
                        }
                        cfg.EnableDiffTracing = parsed.envVariables.diffTracing;
                        cfg.UseExceptionBoundaryFlow = parsed.envVariables.exceptionControlFlow;
                    }
                }
            }
            catch { }
            return cfg;
        }

        private static string GetDefaultProjectConfigPath()
        {
            string assets = Application.dataPath;
            return Path.Combine(assets, "ReactiveUIToolKit", "config.json");
        }

        private static ReactiveUITK.Core.Reconciler.DiffTraceLevel ParseTraceLevel(string value)
        {
            switch (value)
            {
                case "Verbose":
                case "verbose":
                    return ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose;
                case "Basic":
                case "basic":
                    return ReactiveUITK.Core.Reconciler.DiffTraceLevel.Basic;
                case "None":
                case "none":
                default:
                    return ReactiveUITK.Core.Reconciler.DiffTraceLevel.None;
            }
        }
    }
}
