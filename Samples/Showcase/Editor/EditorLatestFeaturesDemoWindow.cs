#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorLatestFeaturesDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Latest Features Showcase")]
        public static void ShowWindow()
        {
            EditorLatestFeaturesDemoWindow window = GetWindow<EditorLatestFeaturesDemoWindow>(
                "Latest Features Demo"
            );
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(LatestFeaturesDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
