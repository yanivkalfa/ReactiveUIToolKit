using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class CounterFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var (count, setCount) = Hooks.UseState(0);
            var increment = Hooks.UseStableCallback(() => setCount(count + 1));
            var outerStyle = new Dictionary<string, object>{{"padding",10f}};
            var buttonStyle = new Dictionary<string, object>{{"marginTop",8f},{"width",160f},{"height",30f}};
            var outerProps = new Dictionary<string, object>{{"style", outerStyle}};
            var buttonProps = new Dictionary<string, object>{{"onClick", (System.Action)increment},{"style", buttonStyle}};
            return V.VisualElement(outerProps, null,
                V.Text($"Count: {count}"),
                V.VisualElement(buttonProps, null, V.Text("Increment"))
            );
        }
    }
}
