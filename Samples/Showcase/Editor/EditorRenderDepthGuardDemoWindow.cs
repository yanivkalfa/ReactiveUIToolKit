#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorRenderDepthGuardDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(Core-Fixes)/Render Depth Guard")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorRenderDepthGuardDemoWindow>("Render Depth Guard");
            window.minSize = new Vector2(540, 480);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(RenderDepthGuardDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
