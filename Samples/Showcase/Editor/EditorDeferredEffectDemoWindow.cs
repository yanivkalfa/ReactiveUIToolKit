#if UNITY_EDITOR
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorDeferredEffectDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Legacy/Tests-(Core-Fixes)/Deferred UseEffect")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorDeferredEffectDemoWindow>("Deferred UseEffect");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(host, V.Func(DeferredEffectDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
