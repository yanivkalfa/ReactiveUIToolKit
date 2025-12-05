#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using ReactiveUITK;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorToggleButtonGroupDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Toggle Button Group")]
        public static void ShowWindow()
        {
            EditorToggleButtonGroupDemoWindow window = GetWindow<EditorToggleButtonGroupDemoWindow>(
                "Toggle Button Group Demo"
            );
            window.minSize = new Vector2(420f, 260f);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ToggleButtonGroupDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
