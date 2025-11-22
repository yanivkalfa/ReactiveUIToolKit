namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Global configuration for Fiber reconciler
    /// </summary>
    public static class FiberConfig
    {
        /// <summary>
        /// Enable Fiber reconciler (default: true)
        /// Set to true to use new Fiber implementation
        /// </summary>
        public static bool UseFiberReconciler { get; set; } = true;

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
