using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class SimpleTextFieldComponent : ReactiveComponent
    {
        private string text = "";
        protected override VirtualNode Render()
        {
            return V.VisualElement(null, null,
                V.TextField(new TextFieldProps
                {
                    Value = text,
                    OnChange = e => SetState(ref text, e.newValue),
                    Placeholder = "Type here..."
                }),
                V.Text($"You typed: {text}")
            );
        }
    }
}
