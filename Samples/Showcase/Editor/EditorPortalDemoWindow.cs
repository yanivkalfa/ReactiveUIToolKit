#if UNITY_EDITOR
using System.Collections.Generic;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.FunctionalComponents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Editor
{
    public sealed class EditorPortalDemoWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/Demos/Tests-(35-37-40)/Portal Event Scope")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorPortalDemoWindow>("Portal Event Scope");
            window.minSize = new Vector2(540, 420);
            window.Show();
        }

        private VisualElement portalTarget;
        private VisualElement contentContainer;

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1f;
            root.Clear();

            var hostContainer = new VisualElement { name = "PortalHost" };
            hostContainer.style.flexGrow = 1f;
            hostContainer.style.flexDirection = FlexDirection.Row;

            contentContainer = new VisualElement
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
            portalOuter.Add(new Label("Portal Target"));
            portalTarget = new VisualElement
            {
                name = "PortalTarget",
                style = { flexGrow = 1f, marginTop = 4f },
            };
            portalOuter.Add(portalTarget);

            hostContainer.Add(contentContainer);
            hostContainer.Add(portalOuter);
            root.Add(hostContainer);

            var props = new Dictionary<string, object> { { "portalTarget", portalTarget } };
            EditorRootRendererUtility.Render(
                contentContainer,
                V.Func(PortalEventScopeDemoFunc.Render, props)
            );
        }

        private void OnDisable()
        {
            if (contentContainer != null)
            {
                EditorRootRendererUtility.Unmount(contentContainer);
            }
            portalTarget?.Clear();
        }
    }
}
#endif
