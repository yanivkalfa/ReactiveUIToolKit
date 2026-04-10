#if UNITY_EDITOR
namespace ReactiveUITK.Core
{
    /// <summary>
    /// Shared state flag for HMR (Hot Module Replacement).
    /// Set by the Editor-side HMR controller; read by the Fiber reconciler
    /// to enable cross-assembly delegate matching during hot reload.
    /// </summary>
    public static class HmrState
    {
        public static bool IsActive;
    }
}
#endif
