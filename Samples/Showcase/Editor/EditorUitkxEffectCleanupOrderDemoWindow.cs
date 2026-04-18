#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxEffectCleanupOrderDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(Core-Fixes)/Effect Cleanup Order")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxEffectCleanupOrderDemoWindow>("Effect Cleanup Order");
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(EffectCleanupOrderDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
