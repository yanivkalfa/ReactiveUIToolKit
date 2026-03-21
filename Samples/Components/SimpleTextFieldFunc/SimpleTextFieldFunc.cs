using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SimpleTextFieldFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (text, setText) = Hooks.UseState("");
            return V.VisualElement(
                null,
                null,
                V.TextField(
                    new TextFieldProps
                    {
                        Value = text,
                        OnChange = e => setText.Set(e.newValue),
                        Placeholder = "Type here...",
                    }
                ),
                V.Text($"You typed: {text}")
            );
        }
    }
}
