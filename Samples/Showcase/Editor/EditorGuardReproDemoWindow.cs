#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    /// <summary>
    /// Editor window that hosts the standalone GuardReproFunc demo
    /// without any router involvement.
    /// </summary>
    public sealed class EditorGuardReproDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Fiber Guard Repro (No Router)")]
        public static void ShowWindow()
        {
            EditorGuardReproDemoWindow window = GetWindow<EditorGuardReproDemoWindow>(
                "Guard Repro (No Router)"
            );
            window.minSize = new Vector2(360, 200);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(GuardReproFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif

