namespace ReactiveUITK.Core.Diagnostics
{
    /// <summary>
    /// Central switch for optional development-time logging across the toolkit.
    /// </summary>
    public static class InternalLogOptions
    {
        /// <summary>
        /// When true, non-essential diagnostic logs will be emitted.
        /// Defaults to false so normal usage stays quiet.
        /// </summary>
        public static bool EnableInternalLogs { get; set; } = false;
    }
}
