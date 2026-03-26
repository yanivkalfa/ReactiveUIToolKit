namespace ReactiveUITK.EditorSupport
{
    /// <summary>
    /// Top-level ReactiveUITK editor menu items.
    /// </summary>
    public static class FiberMenu
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("ReactiveUITK/UI Toolkit Debugger", priority = 9000)]
        private static void OpenUIToolkitDebugger()
        {
            UnityEditor.EditorApplication.ExecuteMenuItem("Window/UI Toolkit/Debugger");
        }
#endif
    }
}
