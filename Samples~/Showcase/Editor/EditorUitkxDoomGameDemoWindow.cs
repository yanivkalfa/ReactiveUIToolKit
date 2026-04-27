#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using Samples.DoomGame;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxDoomGameDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Doom Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxDoomGameDemoWindow>("Doom Game Demo");
            window.minSize = new Vector2(660, 500);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(DoomGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
