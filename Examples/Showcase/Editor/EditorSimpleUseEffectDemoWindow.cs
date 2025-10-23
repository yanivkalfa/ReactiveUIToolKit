using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Examples.FunctionalComponents;

namespace ReactiveUITK.Examples.Editor
{
    public sealed class EditorSimpleUseEffectDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Simple UseEffect")]
        public static void ShowWindow()
        {
            EditorSimpleUseEffectDemoWindow window = GetWindow<EditorSimpleUseEffectDemoWindow>("Simple UseEffect Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Mount(hostElement, V.Func(SimpleUseEffectFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
