#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorContextDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Context Demo")]
        public static void Open()
        {
            var window = GetWindow<EditorContextDemoWindow>("Context Demo");
            window.minSize = new Vector2(480f, 360f);
            window.Show();
        }

        private void CreateGUI()
        {
            var host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(ContextDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
