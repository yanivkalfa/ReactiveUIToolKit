using System;

namespace ReactiveUITK.Core
{
    public static class BuildDefinesConfig
    {
        // ENV_* defines control the resolved environment label
        public static string ResolveEnvironment()
        {
            string env = "production";
#if ENV_DEV
            env = "development";
#elif ENV_STAGING
            env = "staging";
#elif ENV_PROD
            env = "production";
#endif
            return env;
        }

        public static Reconciler.DiffTraceLevel ResolveTraceLevel()
        {
#if RUITK_TRACE_VERBOSE
            return Reconciler.DiffTraceLevel.Verbose;
#elif RUITK_TRACE_BASIC
            return Reconciler.DiffTraceLevel.Basic;
#elif ENV_DEV
            return Reconciler.DiffTraceLevel.Basic;
#else
            return Reconciler.DiffTraceLevel.None;
#endif
        }

        public static bool ResolveEnableDiffTracing()
        {
#if RUITK_DIFF_TRACING
            return true;
#else
            return ResolveTraceLevel() == Reconciler.DiffTraceLevel.Verbose;
#endif
        }
    }
}
