#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorPropTypesDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Tests-(35-37-40)/PropTypes Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorPropTypesDemoWindow>("PropTypes Demo");
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(PropTypesDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
