using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ShowcaseTopBar
    {
        public sealed class Props : IProps
        {
            public string InputText { get; set; }
            public Action<ChangeEvent<string>> OnInputChange { get; set; }
            public Action OnSetText { get; set; }
            public DateTime CurrentTime { get; set; }
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as Props;
            var inputText = p?.InputText ?? string.Empty;

            return V.VisualElement(
                new VisualElementProps { Style = SharedDemoPageStyles.TopBarStyle },
                null,
                V.VisualElement(
                    new VisualElementProps { Style = SharedDemoPageStyles.LeftBoxStyle },
                    null,
                    V.Text("Left")
                ),
                V.TextField(new TextFieldProps
                {
                    Style = SharedDemoPageStyles.TextInputStyle,
                    Placeholder = "Type here...",
                    HidePlaceholderOnFocus = false,
                    Value = inputText,
                    LabelText = string.IsNullOrEmpty(inputText) ? string.Empty : "Value: " + inputText,
                    OnChange = p?.OnInputChange,
                }),
                V.Button(new ButtonProps
                {
                    Text = "Set Text",
                    OnClick = p?.OnSetText,
                    Style = new Style { (MarginLeft, 6f), (Height, 28f) },
                }),
                V.VisualElement(
                    new VisualElementProps { Style = SharedDemoPageStyles.RightBoxStyle },
                    null,
                    V.Text("Right")
                )
            );
        }
    }
}
