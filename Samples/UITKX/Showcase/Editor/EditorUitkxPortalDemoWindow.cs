#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxPortalDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Tests-(35-37-40)/Portal Event Scope")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxPortalDemoWindow>("Portal Event Scope");
            window.minSize = new Vector2(540, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(PortalEventScopeDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
