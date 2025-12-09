#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorMainMenuRouterDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Main Menu Router")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorMainMenuRouterDemoWindow>("Main Menu Router Demo");
            window.minSize = new Vector2(640f, 420f);
            window.Show();
        }

        private void CreateGUI()
        {
            var host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(MainMenuRouterDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
