using ReactiveUITK.Core.Config;
using ReactiveUITK.Core.Diagnostics;

namespace ReactiveUITK.Core
{
    public static class BuildDefinesConfig
    {
        public static string ResolveEnvironment()
        {
            return ReactiveUITKConfig.Current.EnvironmentLabel ?? "production";
        }

        public static DiagnosticsConfig.TraceLevel ResolveTraceLevel()
        {
            return ReactiveUITKConfig.Current.TraceLevel;
        }

        public static bool ResolveEnableDiffTracing()
        {
            return ReactiveUITKConfig.Current.EnableDiffTracing;
        }

        public static bool ResolveExceptionBoundaryFlow()
        {
            return ReactiveUITKConfig.Current.UseExceptionBoundaryFlow;
        }
    }
}
