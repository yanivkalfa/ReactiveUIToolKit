#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorRefForwardingDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Ref Forwarding + useRef")]
        public static void ShowWindow()
        {
            EditorRefForwardingDemoWindow window = GetWindow<EditorRefForwardingDemoWindow>(
                "Ref Forwarding Demo"
            );
            window.minSize = new Vector2(520, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(RefForwardingDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
