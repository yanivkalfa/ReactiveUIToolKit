using UnityEngine;

namespace PrettyUi
{
    /// <summary>
    /// In-repo stub of the consumer project's <c>GameSceneLoader</c>. The real
    /// implementation additively loads a scene named <c>GameScene</c> on
    /// <c>GamePage</c> mount and unloads it on unmount. We don't ship a scene
    /// here — the goal of <c>Samples/UIs/PrettyUi</c> is repro of HMR / render
    /// bugs against the same component tree, not exercising scene loading.
    /// <para>
    /// Keep the public surface (static <c>LoadAsync</c> / <c>UnloadAsync</c>,
    /// <c>IsLoaded</c>) identical so <c>GamePage.uitkx</c>'s <c>useEffect</c>
    /// graph compiles and runs unchanged. Calls log once at <c>Debug.Log</c>
    /// then no-op.
    /// </para>
    /// </summary>
    public static class GameSceneLoader
    {
        public const string GameSceneName = "GameScene";

        public static bool IsLoaded => false;

        public static void LoadAsync()
        {
            Debug.Log("[GameSceneLoader stub] LoadAsync invoked (no-op in PrettyUi sample).");
        }

        public static void UnloadAsync()
        {
            Debug.Log("[GameSceneLoader stub] UnloadAsync invoked (no-op in PrettyUi sample).");
        }
    }
}
