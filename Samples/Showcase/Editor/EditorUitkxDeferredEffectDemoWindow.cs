#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxDeferredEffectDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(Core-Fixes)/Deferred UseEffect")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxDeferredEffectDemoWindow>("Deferred UseEffect");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(DeferredEffectDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
