#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxRenderDepthGuardDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(Core-Fixes)/Render Depth Guard")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxRenderDepthGuardDemoWindow>("Render Depth Guard");
            window.minSize = new Vector2(540, 480);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RenderDepthGuardDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
