using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Examples.FunctionalComponents;

public class ReactiveUIBootstrap : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private RootRenderer rootRenderer;
    private void Awake()
    {
        rootRenderer = GetComponent<RootRenderer>();
        if (rootRenderer == null)
        {
            Debug.LogError("ReactiveUIBootstrap: RootRenderer component missing on GameObject");
            return;
        }
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogError("ReactiveUIBootstrap: UIDocument not assigned or rootVisualElement null");
            return;
        }
        rootRenderer.Initialize(uiDocument.rootVisualElement);
        rootRenderer.Render<AppFuncRoot>();
    }
}
