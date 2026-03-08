using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class IntroCounterFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (count, setCount) = Hooks.UseState(0);

            var rowStyle = new Style { (FlexDirection, "row"), (AlignItems, "center") };

            var buttonProps = new ButtonProps
            {
                Text = "+1",
                OnClick = _ => setCount(count + 1),
                Style = new Style { (MarginLeft, 8f) },
            };

            return ReactiveUITK.V.VisualElement(
                new VisualElementProps { Style = rowStyle },
                null,
                ReactiveUITK.V.Label(new LabelProps { Text = $"Count: {count}" }),
                ReactiveUITK.V.Button(buttonProps)
            );
        }
    }
}
