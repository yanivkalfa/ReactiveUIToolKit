using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;

namespace ReactiveUITK.Examples.Editor
{
    public sealed class AppFuncEditorWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/AppFunc Demo")]
        public static void ShowWindow()
        {
            AppFuncEditorWindow window = GetWindow<AppFuncEditorWindow>("ReactiveUITK AppFunc");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Mount(hostElement, V.Func(Shared.SharedDemoPage.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
