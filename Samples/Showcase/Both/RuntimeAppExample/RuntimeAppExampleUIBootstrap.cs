using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK;
using ReactiveUITK.Samples.Shared;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public class RuntimeAppExampleUIBootstrap : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        private RootRenderer rootRenderer;
        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null)
            {
                Debug.LogError("RuntimeAppExampleUIBootstrap: RootRenderer component missing on GameObject");
                return;
            }
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("RuntimeAppExampleUIBootstrap: UIDocument not assigned or rootVisualElement null");
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(SharedDemoPage.Render));
        }
    }
}
