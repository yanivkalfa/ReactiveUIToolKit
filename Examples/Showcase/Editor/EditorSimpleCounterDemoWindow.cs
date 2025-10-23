using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Examples.FunctionalComponents;

namespace ReactiveUITK.Examples.Editor
{
    public sealed class EditorSimpleCounterDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Simple Counter")]
        public static void ShowWindow()
        {
            EditorSimpleCounterDemoWindow window = GetWindow<EditorSimpleCounterDemoWindow>("Simple Counter Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SimpleCounterFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
