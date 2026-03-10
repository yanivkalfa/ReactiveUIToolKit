#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxRefForwardingDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/UITKX/Ref Forwarding + useRef")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxRefForwardingDemoWindow>("Ref Forwarding Demo");
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
