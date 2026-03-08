using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SignalCounterDemoFunc
    {
        private static readonly Signal<int> CounterSignal = ReactiveUITK.Signals.SignalFactory.Get<int>(
            "demo.counter",
            0
        );

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            int count = Hooks.UseSignal(CounterSignal);
            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { (StyleKeys.Padding, 12f), (StyleKeys.MarginTop, 8f) },
                },
                null,
                V.Text("Signal Counter (shared state)"),
                V.Text($"Count: {count}"),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style { (StyleKeys.FlexDirection, "row") },
                    },
                    null,
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Increment",
                            OnClick = () => CounterSignal.Dispatch(v => v + 1),
                            Style = new Style { (StyleKeys.MarginRight, 6f) },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Decrement",
                            OnClick = () => CounterSignal.Dispatch(v => v - 1),
                            Style = new Style { (StyleKeys.MarginRight, 6f) },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Reset",
                            OnClick = () => CounterSignal.Dispatch(0),
                        }
                    )
                )
            );
        }
    }
}
