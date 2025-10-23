using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;

namespace ReactiveUITK.Examples.Editor
{
    public sealed class EditorAppExampleWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Showcase All")]
        public static void ShowWindow()
        {
            EditorAppExampleWindow window = GetWindow<EditorAppExampleWindow>("ReactiveUITK Showcase Demo");
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
