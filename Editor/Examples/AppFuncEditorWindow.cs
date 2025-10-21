using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
// using ReactiveUITK.Examples.FunctionalComponents;

namespace ReactiveUITK.EditorExamples
{
    public sealed class AppFuncEditorWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/AppFunc Demo")] 
        public static void ShowWindow()
        {
            AppFuncEditorWindow window = GetWindow<AppFuncEditorWindow>("ReactiveUITK AppFunc");
            window.minSize = new UnityEngine.Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement host = rootVisualElement;
            host.style.flexGrow = 1f;
            EditorRootRendererUtility.Mount(host,
                V.VisualElement(new System.Collections.Generic.Dictionary<string, object>
                {
                    {"style", new ReactiveUITK.Props.Typed.Style { (ReactiveUITK.Props.Typed.StyleKeys.Padding, 8f) } }
                }, null,
                    V.Text("ReactiveUITK Editor Demo"),
                    V.Button(new ReactiveUITK.Props.Typed.ButtonProps { Text = "Click me", OnClick = () => UnityEngine.Debug.Log("Editor button clicked") })
                )
            );
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }
}
