#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxDiabloMenuDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Diablo Menu")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxDiabloMenuDemoWindow>("Diablo Menu Demo");
            window.minSize = new Vector2(600, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            // TODO: DiabloMenuDemoFunc was removed — replace with a valid component.
            // EditorRootRendererUtility.Render(hostElement, V.Func(DiabloMenuDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
