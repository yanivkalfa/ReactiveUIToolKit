#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorEventBatchingDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(13-17-18-26)/Event Batching")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorEventBatchingDemoWindow>("Event Batching Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(EventBatchingDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
