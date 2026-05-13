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

        /// <summary>
        /// Editor-side rollback hook for the per-component <c>__hmr_Render</c>
        /// trampoline. The reconciler invokes this from its render-crash
        /// catch block (inside <c>BeginWork</c>): if it returns <c>true</c>,
        /// the component's trampoline field has been reverted to the
        /// previous-known-working delegate and the reconciler retries the
        /// render once before falling through to the nearest error boundary.
        ///
        /// Wired up by the Editor at load time (see
        /// <c>UitkxHmrComponentTrampolineSwapper</c>). Null in player builds
        /// — the reconciler skips the rollback path under the same
        /// <c>#if UNITY_EDITOR</c> guard.
        /// </summary>
        public static System.Func<System.Type, bool> TryRollbackComponent;
    }
}
#endif
