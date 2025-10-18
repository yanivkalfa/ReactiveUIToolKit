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
            return V.VisualElement(new Dictionary<string, object>{{"style", new Dictionary<string, object>{{"padding",10f}}}}, null,
                V.Text($"Count: {count}"),
                V.VisualElement(new Dictionary<string, object>{{"onClick", (System.Action)increment}, {"style", new Dictionary<string, object>{{"marginTop",8f},{"width",160f},{"height",30f}}}}, null,
                    V.Text("Increment"))
            );
        }
    }
}
