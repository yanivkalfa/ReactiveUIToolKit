using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Samples.PortalsPlayground;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.PortalsPlayground
{
    /// <summary>
    /// Runtime mount for <see cref="PortalsPlayground"/>.
    ///
    /// Demonstrates the multi-panel pattern: one component tree, one Render call,
    /// many UIDocuments acting as portal targets. The main host renders the root
    /// tree; satellite UIDocuments contribute their <c>rootVisualElement</c> as
    /// named portal slots seeded into <see cref="HostContext.Environment"/>.
    ///
    /// Setup:
    /// 1. Add this MonoBehaviour to a GameObject that also has <see cref="RootRenderer"/>.
    /// 2. Assign <see cref="mainUIDocument"/> to the UIDocument where the root tree mounts.
    /// 3. Assign <see cref="slotAUIDocument"/> and <see cref="slotBUIDocument"/> to two
    ///    other UIDocuments (e.g. World Space panels parented to scene objects).
    /// 4. Press Play. Buttons in either satellite panel update shared state and both
    ///    satellite panels re-render with the new value.
    /// </summary>
    [RequireComponent(typeof(RootRenderer))]
    public class PortalsPlaygroundBootstrap : MonoBehaviour
    {
        [Tooltip("UIDocument that hosts the root component tree.")]
        [SerializeField]
        private UIDocument mainUIDocument;

        [Tooltip("UIDocument whose rootVisualElement is exposed as portal slot 'portals:slot-a'.")]
        [SerializeField]
        private UIDocument slotAUIDocument;

        [Tooltip("UIDocument whose rootVisualElement is exposed as portal slot 'portals:slot-b'.")]
        [SerializeField]
        private UIDocument slotBUIDocument;

        private RootRenderer rootRenderer;

        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null || mainUIDocument == null || mainUIDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    "[PortalsPlaygroundBootstrap] Missing RootRenderer or mainUIDocument."
                );
                return;
            }

            // Seed satellite panel roots as named portal targets BEFORE the first Render.
            // RootRenderer.Initialize invokes the env callback against the shared HostContext;
            // useContext<VisualElement>("portals:slot-a"/"slot-b") reads them inside the tree.
            rootRenderer.Initialize(
                mainUIDocument.rootVisualElement,
                env: ctx =>
                {
                    if (slotAUIDocument != null && slotAUIDocument.rootVisualElement != null)
                        ctx.Environment["portals:slot-a"] = slotAUIDocument.rootVisualElement;

                    if (slotBUIDocument != null && slotBUIDocument.rootVisualElement != null)
                        ctx.Environment["portals:slot-b"] = slotBUIDocument.rootVisualElement;
                }
            );

            var hostProps = new VisualElementProps { PickingMode = PickingMode.Position };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(PortalsPlayground.Render)));
        }
    }
}
