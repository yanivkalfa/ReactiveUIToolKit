#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using Samples.TicTacToe;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxTicTacToeDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tic Tac Toe")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxTicTacToeDemoWindow>("Tic Tac Toe Demo");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(TicTacToe.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
