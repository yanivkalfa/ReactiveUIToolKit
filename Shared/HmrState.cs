#if UNITY_EDITOR
namespace ReactiveUITK.Core
{
    /// <summary>
    /// Shared state flag for HMR (Hot Module Replacement).
    /// Set by the Editor-side HMR controller; read by the Fiber reconciler
    /// to enable cross-assembly delegate matching during hot reload.
    /// </summary>
    internal static class HmrState
    {
        internal static bool IsActive;
    }
}
#endif
