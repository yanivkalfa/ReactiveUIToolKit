#if UNITY_EDITOR
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.UITKX.Editor
{
    public sealed class EditorUitkxPortalDemoWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Tests-(35-37-40)/Portal Event Scope")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorUitkxPortalDemoWindow>("Portal Event Scope");
            window.minSize = new Vector2(540, 420);
            window.Show();
        }

        private VisualElement contentRegion;

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1f;
            root.Clear();

            // Build the split layout imperatively â€” the host owns the named DOM slot.
            // The component tree never receives portalTarget as a prop; it reads it
            // from HostContext.Environment via useContext(PortalContextKeys.ModalRoot),
            // exactly like how the scheduler and env flags are seeded today.
            var hostContainer = new VisualElement
            {
                name = "PortalDemoHost",
                style = { flexGrow = 1f, flexDirection = FlexDirection.Row },
            };

            contentRegion = new VisualElement
            {
                name = "ContentRegion",
                style = { flexGrow = 1f, marginRight = 6f },
            };

            var portalOuter = new VisualElement
            {
                name = "PortalTargetOuter",
                style =
                {
                    width = 220f,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                    paddingTop = 6f,
                    paddingBottom = 6f,
                    paddingLeft = 6f,
                    paddingRight = 6f,
                },
            };
            portalOuter.Add(new Label("Portal Target") { style = { marginBottom = 4f } });

            var portalTarget = new VisualElement
            {
                name = "PortalTarget",
                style = { flexGrow = 1f },
            };
            portalOuter.Add(portalTarget);

            hostContainer.Add(contentRegion);
            hostContainer.Add(portalOuter);
            root.Add(hostContainer);

            // Seed the portal slot into the HostContext before the renderer starts.
            // PortalEventScopeDemoFunc retrieves it via useContext<VisualElement>(PortalContextKeys.ModalRoot).
            EditorRootRendererUtility.Render(
                contentRegion,
                V.Func(PortalEventScopeDemoFunc.Render),
                env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = portalTarget
            );
        }

        private void OnDisable()
        {
            if (contentRegion != null)
                EditorRootRendererUtility.Unmount(contentRegion);
        }
    }
}
#endif
