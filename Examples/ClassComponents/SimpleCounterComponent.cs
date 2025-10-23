using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class SimpleCounterComponent : ReactiveComponent
    {
        private int count;
        protected override VirtualNode Render()
        {
            return V.VisualElement(null, null,
                V.Text($"Count: {count}"),
                V.Button(new ButtonProps
                {
                    Text = "+",
                    OnClick = () => SetState(ref count, count + 1)
                })
            );
        }
    }
}
