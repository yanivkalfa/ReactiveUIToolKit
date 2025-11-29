#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorProgressBarDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Progress Bar")]
        public static void ShowWindow()
        {
            EditorProgressBarDemoWindow window = GetWindow<EditorProgressBarDemoWindow>(
                "Progress Bar Demo"
            );
            window.minSize = new Vector2(420, 240);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ProgressBarDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
