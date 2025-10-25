using ReactiveUITK.Core.Config;

namespace ReactiveUITK.Core
{
    public static class BuildDefinesConfig
    {
        public static string ResolveEnvironment()
        {
            return ReactiveUITKConfig.Current.EnvironmentLabel ?? "production";
        }

        public static Reconciler.DiffTraceLevel ResolveTraceLevel()
        {
            return ReactiveUITKConfig.Current.TraceLevel;
        }

        public static bool ResolveEnableDiffTracing()
        {
            return ReactiveUITKConfig.Current.EnableDiffTracing;
        }
    }
}
