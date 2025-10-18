using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class AppFunc
    {
        // Functional root: returns top bar with left/right boxes
        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var topBarStyle = new Dictionary<string, object>
            {
                {"backgroundColor", Color.white},
                {"flexDirection", "row"},
                {"justifyContent", "space-between"},
                {"alignItems", "center"},
                {"paddingLeft", 12f},
                {"paddingRight", 12f},
                {"paddingTop", 8f},
                {"paddingBottom", 8f},
                {"borderBottomWidth", 1f},
                {"borderBottomColor", new Color(0.85f,0.85f,0.85f,1f)}
            };

            var leftBoxStyle = new Dictionary<string, object>
            {
                {"backgroundColor", new Color(0.2f,0.4f,0.9f,1f)},
                {"color", Color.white},
                {"paddingLeft", 10f},
                {"paddingRight", 10f},
                {"paddingTop", 6f},
                {"paddingBottom", 6f},
                {"borderRadius", 4f},
                {"fontSize", 14f}
            };

            var rightBoxStyle = new Dictionary<string, object>
            {
                {"backgroundColor", new Color(0.9f,0.3f,0.2f,1f)},
                {"color", Color.white},
                {"paddingLeft", 10f},
                {"paddingRight", 10f},
                {"paddingTop", 6f},
                {"paddingBottom", 6f},
                {"borderRadius", 4f},
                {"fontSize", 14f}
            };

            var pageStyle = new Dictionary<string, object>
            {
                {"flexDirection", "column"},
                {"flexGrow", 1f},
                {"backgroundColor", new Color(0.95f,0.95f,0.95f,1f)}
            };

            return V.VisualElement(new Dictionary<string, object>{{"style", pageStyle}}, null,
                V.VisualElement(new Dictionary<string, object>{{"style", topBarStyle}}, null,
                    V.VisualElement(new Dictionary<string, object>{{"style", leftBoxStyle}}, null,
                        V.Text("Left")
                    ),
                    V.VisualElement(new Dictionary<string, object>{{"style", rightBoxStyle}}, null,
                        V.Text("Right")
                    )
                )
            );
        }
    }

    // Wrapper class component to mount functional root via RootRenderer.Render<>()
    public sealed class AppFuncRoot : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            // Wrap function root so Reconciler builds it (BuildSubtree only processes children of root node)
            return V.VisualElement(null, null, V.Func(AppFunc.Render));
        }
    }
}
