using PrettyUi.App;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.UIs.PrettyUi
{
    /// <summary>
    /// Mounts the <see cref="AppRoot"/> tree from <c>Samples/UIs/PrettyUi/UI</c>.
    /// Faithful copy of an external consumer project so HMR-time bugs that only
    /// surface against this exact shape (router + module/style + relative
    /// <c>Asset&lt;T&gt;("../Resources/...")</c>) can be reproduced inside this
    /// repo without round-tripping through publish.
    /// <para>
    /// Wire-up: attach to a GameObject with a <see cref="UIDocument"/> assigned
    /// to <see cref="uiDocument"/>. The component auto-adds a
    /// <see cref="RootRenderer"/>.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(RootRenderer))]
    public class PrettyUiBootstrap : MonoBehaviour
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
                    "PrettyUiBootstrap: Missing RootRenderer or UIDocument (uiDocument.rootVisualElement is null)."
                );
                return;
            }

            rootRenderer.Initialize(uiDocument.rootVisualElement);
            var hostProps = new VisualElementProps { PickingMode = PickingMode.Ignore };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(AppRoot.Render)));
        }
    }
}
