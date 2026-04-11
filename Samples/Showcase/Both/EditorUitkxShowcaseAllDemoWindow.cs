#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxShowcaseAllDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Showcase All")]
        public static void ShowWindow()
        {
            EditorUitkxShowcaseAllDemoWindow window = GetWindow<EditorUitkxShowcaseAllDemoWindow>(
                "ReactiveUITK UITKX Showcase Demo"
            );
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                hostElement,
                V.Func(ShowcaseDemoPage.Render, key: "showcase-demo-page")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
