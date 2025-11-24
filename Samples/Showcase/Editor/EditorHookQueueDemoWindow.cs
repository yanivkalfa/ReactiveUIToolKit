#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorHookQueueDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(13-17-18-26)/Hook Queue Merge")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorHookQueueDemoWindow>("Hook Queue Merge Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        [MenuItem("ReactiveUITK/Demos/Tests-(35-37-40)/Linked Update Queue")]
        public static void ShowWindowModern()
        {
            ShowWindow();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(HookStateQueueDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
