#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorKeyedDiffLisDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(13-17-18-26)/Keyed Diff (LIS)")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorKeyedDiffLisDemoWindow>("Keyed Diff (LIS)");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(KeyedDiffLisDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
