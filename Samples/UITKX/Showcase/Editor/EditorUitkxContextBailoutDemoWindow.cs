#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxContextBailoutDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Tests-(Core-Fixes)/Context Through Bailout")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxContextBailoutDemoWindow>("Context Through Bailout");
            window.minSize = new Vector2(520, 460);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(ContextBailoutDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
