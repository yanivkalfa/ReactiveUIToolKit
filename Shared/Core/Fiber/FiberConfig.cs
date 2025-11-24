namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Global configuration for Fiber reconciler
    /// </summary>
    public static class FiberConfig
    {
        /// <summary>
        /// Enable verbose fiber logging
        /// </summary>
        public static bool EnableFiberLogging { get; set; } = true;
        /// <summary>
        /// Enable to see which reconciler is being used
        /// </summary>
        public static bool ShowReconcilerInfo { get; set; } = true;
    }
}
