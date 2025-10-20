using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class SimpleHelloFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var containerStyle = new Style { (Padding, 4f) };
            var rootProps = new Dictionary<string, object> { { "style", containerStyle } };
            return V.VisualElement(rootProps, null, V.Text("Hello ReactiveUITK (Function Component)"));
        }
    }
}
