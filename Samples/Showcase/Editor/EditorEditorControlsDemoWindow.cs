#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Samples.Shared;
using ReactiveUITK.EditorSupport;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorEditorControlsDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Legacy/Editor Controls")]
        public static void Open()
        {
            var w = GetWindow<EditorEditorControlsDemoWindow>("RUITK Editor Controls");
            w.minSize = new Vector2(600, 360);
            w.Show();
        }

        private void CreateGUI()
        {
            var host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                host,
                V.Func(EditorControlsDemoPage.Render, key: "editor-controls-demo")
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
