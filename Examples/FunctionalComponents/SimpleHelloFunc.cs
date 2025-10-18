using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class SimpleHelloFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            return V.VisualElement(new Dictionary<string, object>{{"style", new Dictionary<string, object>{{"padding",4f}}}}, null, V.Text("Hello ReactiveUITK (Function Component)"));
        }
    }
}
