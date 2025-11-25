#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Samples.Shared;
using ReactiveUITK.EditorSupport;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorAnimationsDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Animations")]
        public static void Open()
        {
            var w = GetWindow<EditorAnimationsDemoWindow>("RUITK Animations");
            w.minSize = new Vector2(420, 320);
            w.Show();
        }

        private void CreateGUI()
        {
            var host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                host,
                V.Func(AnimationsDemoPage.Render, key: "animations-demo")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
