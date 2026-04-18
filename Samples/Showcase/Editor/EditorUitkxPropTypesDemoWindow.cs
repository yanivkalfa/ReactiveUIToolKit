#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxPropTypesDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(35-37-40)/PropTypes Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxPropTypesDemoWindow>("PropTypes Demo");
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(PropTypesDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
