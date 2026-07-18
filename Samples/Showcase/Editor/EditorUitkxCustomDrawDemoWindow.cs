#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.Components.CustomDrawDemoFunc;
using ReactiveUITK.Samples.Components.CustomDrawDemoFunc.CustomDrawDemoFunc;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    /// <summary>
    /// Editor demo for custom rendering via the <c>onGenerateVisualContent</c>
    /// prop (Unity's <c>VisualElement.generateVisualContent</c>). Shows
    /// Painter2D vector drawing, a raw mesh built with
    /// <c>MeshGenerationContext.Allocate</c>, and the <c>redrawKey</c> repaint
    /// trigger paired with a stable callback.
    ///
    /// Launch from the menu: <b>ReactiveUITK / Demos / Custom Drawing</b>.
    /// </summary>
    public sealed class EditorUitkxCustomDrawDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Custom Drawing")]
        public static void ShowWindow()
        {
            EditorUitkxCustomDrawDemoWindow window =
                GetWindow<EditorUitkxCustomDrawDemoWindow>("UITKX Custom Drawing");
            window.minSize = new Vector2(460, 560);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Render(hostElement, V.Func(CustomDrawDemoFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
#endif
