#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorEffectCleanupOrderDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Legacy/Tests-(Core-Fixes)/Effect Cleanup Order")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorEffectCleanupOrderDemoWindow>("Effect Cleanup Order");
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(EffectCleanupOrderDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
