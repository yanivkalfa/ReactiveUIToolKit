#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorFlushSyncDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(35-37-40)/FlushSync Demo")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorFlushSyncDemoWindow>("FlushSync Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(FlushSyncDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
