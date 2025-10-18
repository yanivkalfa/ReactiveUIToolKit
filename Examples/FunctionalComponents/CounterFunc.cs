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
            return V.View(new Dictionary<string, object>{{"style.padding",10f}}, null,
                V.Text($"Count: {count}"),
                V.View(new Dictionary<string, object>{{"onClick", (System.Action)increment}, {"style.marginTop",8f}, {"style.width",160f}, {"style.height",30f}}, null,
                    V.Text("Increment"))
            );
        }
    }
}
