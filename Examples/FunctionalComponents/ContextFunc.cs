using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class ContextFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var theme = Hooks.UseContext<Color>("themeColor");
            if (theme == default) theme = Color.gray;
            return V.VisualElement(new Dictionary<string, object>{{"style", new Dictionary<string, object>{{"padding",6f},{"backgroundColor",theme}}}}, null,
                V.Text("Functional consumer uses theme context"));
        }
    }
}
