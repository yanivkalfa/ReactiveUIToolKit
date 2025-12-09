using ReactiveUITK.Core;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeHelloWorldDemoBootstrap : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null || uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    "RuntimeHelloWorldDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(HelloWorldFunc.Render));
        }
    }
}
