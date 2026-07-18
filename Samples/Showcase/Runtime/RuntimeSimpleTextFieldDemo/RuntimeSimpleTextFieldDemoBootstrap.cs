using ReactiveUITK.Core;
using ReactiveUITK.Samples.Components.SimpleTextFieldFunc;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Samples.Components.SimpleTextFieldFunc.SimpleTextFieldFunc;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
    [RequireComponent(typeof(RootRenderer))]
    public class RuntimeSimpleTextFieldDemoBootstrap : MonoBehaviour
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
                    "RuntimeSimpleTextFieldDemoBootstrap: Missing RootRenderer or UIDocument"
                );
                return;
            }
            rootRenderer.Initialize(uiDocument.rootVisualElement);
            rootRenderer.Render(V.Func(SimpleTextFieldFunc.Render));
        }
    }
}
