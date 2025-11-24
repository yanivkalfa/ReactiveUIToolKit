#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Editor
{
    /// <summary>
    /// Editor window that hosts the RouterGuardReproFunc demo,
    /// i.e., GuardReproFunc nested under a Router.
    /// </summary>
    public sealed class EditorRouterGuardReproDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Fiber Guard Repro (Router)")]
        public static void ShowWindow()
        {
            EditorRouterGuardReproDemoWindow window =
                GetWindow<EditorRouterGuardReproDemoWindow>("Guard Repro (Router)");
            window.minSize = new Vector2(360, 220);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(
                hostElement,
                V.Func(RouterGuardReproFunc.Render)
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif

