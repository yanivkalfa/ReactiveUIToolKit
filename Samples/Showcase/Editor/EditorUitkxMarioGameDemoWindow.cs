#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using Samples.MarioGame;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxMarioGameDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Mario Game")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxMarioGameDemoWindow>("Mario Game Demo");
            window.minSize = new Vector2(820, 640);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(MarioGame.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
