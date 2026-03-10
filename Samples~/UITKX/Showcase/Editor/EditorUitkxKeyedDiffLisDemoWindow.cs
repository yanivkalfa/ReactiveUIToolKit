#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxKeyedDiffLisDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Tests-(13-17-18-26)/Keyed Diff (LIS)")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxKeyedDiffLisDemoWindow>("Keyed Diff (LIS)");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(KeyedDiffLisDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
