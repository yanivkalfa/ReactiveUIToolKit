using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SimpleCounterFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var (count, setCount) = Hooks.UseState(0);
            return V.VisualElement(null, null,
                V.Text($"Count: {count}"),
                V.Button(new ButtonProps
                {
                    Text = "+",
                    OnClick = () => setCount(count + 1)
                })
            );
        }
    }
}
