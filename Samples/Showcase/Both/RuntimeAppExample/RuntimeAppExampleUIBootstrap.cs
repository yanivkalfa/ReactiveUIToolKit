using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using Samples.TicTacToe;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public class RuntimeAppExampleUIBootstrap : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null)
            {
                Debug.LogError(
                    "RuntimeAppExampleUIBootstrap: RootRenderer component missing on GameObject"
                );
                return;
            }
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    "RuntimeAppExampleUIBootstrap: UIDocument not assigned or rootVisualElement null"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            // TODO: DiabloMenuDemoFunc was removed — replace with a valid component.
            var hostProps = new VisualElementProps { PickingMode = PickingMode.Ignore };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(TicTacToe.Render)));
        }
    }
}
