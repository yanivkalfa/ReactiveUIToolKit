#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using Samples.SnakeGame;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxSnakeGameDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Snake Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxSnakeGameDemoWindow>("Snake Game Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(SnakeGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
