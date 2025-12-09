#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorSyntheticEventDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(35-37-40)/Synthetic Events")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorSyntheticEventDemoWindow>("Synthetic Event Demo");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(SyntheticEventDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
