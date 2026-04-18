#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using Samples.StressTest;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxStressTestDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Stress Test")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxStressTestDemoWindow>("Stress Test Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(StressTest.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
