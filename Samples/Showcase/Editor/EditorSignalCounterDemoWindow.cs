#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorSignalCounterDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Legacy/Signal Counter")]
        public static void ShowWindow()
        {
            EditorSignalCounterDemoWindow window = GetWindow<EditorSignalCounterDemoWindow>(
                "Signal Counter Demo"
            );
            window.minSize = new Vector2(360, 260);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SignalCounterDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
