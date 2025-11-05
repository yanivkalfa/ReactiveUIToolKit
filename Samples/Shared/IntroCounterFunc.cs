using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class IntroCounterFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (count, setCount) = Hooks.UseState(0);

            var rowStyle = new Style { (FlexDirection, "row"), (AlignItems, "center") };

            var buttonProps = new ButtonProps
            {
                Text = "+1",
                OnClick = () => setCount.Set(count + 1),
                Style = new Style { (MarginLeft, 8f) },
            };

            return ReactiveUITK.V.VisualElement(
                new Dictionary<string, object> { { "style", rowStyle } },
                null,
                ReactiveUITK.V.Label(new LabelProps { Text = $"Count: {count}" }),
                ReactiveUITK.V.Button(buttonProps)
            );
        }
    }
}
