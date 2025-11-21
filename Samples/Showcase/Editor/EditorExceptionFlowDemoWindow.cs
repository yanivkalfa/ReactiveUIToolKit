#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorExceptionFlowDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Exception Flow")]
        public static void ShowWindow()
        {
            EditorExceptionFlowDemoWindow window = GetWindow<EditorExceptionFlowDemoWindow>(
                "Exception Flow Demo"
            );
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ExceptionFlowDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
