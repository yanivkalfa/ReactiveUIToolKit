#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXShared;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxMediaPlaygroundDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Media Playground")]
        public static void ShowWindow()
        {
            EditorUitkxMediaPlaygroundDemoWindow window =
                GetWindow<EditorUitkxMediaPlaygroundDemoWindow>("UITKX Media Playground");
            window.minSize = new Vector2(560, 640);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                hostElement,
                V.Func(MediaPlaygroundDemoPage.Render, key: "media-playground-demo")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
