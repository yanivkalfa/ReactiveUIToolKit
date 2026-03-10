#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXShared;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxAnimationsDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Animations")]
        public static void ShowWindow()
        {
            EditorUitkxAnimationsDemoWindow window = GetWindow<EditorUitkxAnimationsDemoWindow>(
                "UITKX Animations Demo"
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
                V.Func(AnimationsDemoPage.Render, key: "animations-demo")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
