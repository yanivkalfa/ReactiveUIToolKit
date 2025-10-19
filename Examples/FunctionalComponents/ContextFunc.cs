using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class ContextFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var themeColor = Hooks.UseContext<Color>("themeColor");
            if (themeColor == default)
            {
                themeColor = Color.gray;
            }
            var style = new Dictionary<string, object>{{"padding",6f},{"backgroundColor", themeColor}};
            var rootProps = new Dictionary<string, object>{{"style", style}};
            return V.VisualElement(rootProps, null, V.Text("Functional consumer uses theme context"));
        }
    }
}
