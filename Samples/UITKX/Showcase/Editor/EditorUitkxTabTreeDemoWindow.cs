#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxTabTreeDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Tabs + TreeView")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxTabTreeDemoWindow>("Tabs + TreeView Demo");
            window.minSize = new Vector2(600, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(TabTreeDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
