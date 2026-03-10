#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorRouterDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Legacy/Router")]
        public static void ShowWindow()
        {
            EditorRouterDemoWindow window = GetWindow<EditorRouterDemoWindow>("Router Demo");
            window.minSize = new Vector2(500, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RouterDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
