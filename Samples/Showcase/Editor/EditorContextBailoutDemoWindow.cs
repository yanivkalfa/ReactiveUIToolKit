#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorContextBailoutDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(Core-Fixes)/Context Through Bailout")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorContextBailoutDemoWindow>("Context Through Bailout");
            window.minSize = new Vector2(520, 460);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(ContextBailoutDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
