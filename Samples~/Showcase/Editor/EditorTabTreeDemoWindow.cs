#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.Shared;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorTabTreeDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Tabs + TreeView")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorTabTreeDemoWindow>("Tabs + TreeView Demo");
            window.minSize = new Vector2(600, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(TabTreeDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
