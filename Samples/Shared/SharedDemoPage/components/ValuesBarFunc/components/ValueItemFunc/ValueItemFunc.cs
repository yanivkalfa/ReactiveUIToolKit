using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class ValueItemFunc
    {
        public sealed class Props : IProps
        {
            public string TypeText { get; set; }
            public string TypeValue { get; set; }
        }

        private static readonly Style ContainerStyle = new()
        {
            (FlexDirection, "row"),
            (AlignItems, "center"),
            (BackgroundColor, new UColor(0.98f, 0.98f, 0.98f, 1f)),
            (BorderWidth, 1f),
            (BorderColor, new UColor(0.88f, 0.88f, 0.88f, 1f)),
            (BorderRadius, 4f),
            (PaddingLeft, 6f),
            (PaddingRight, 6f),
            (PaddingTop, 3f),
            (PaddingBottom, 3f),
            (MarginRight, 8f),
            (MarginBottom, 6f),
            (FlexGrow, 1f),
            (MinWidth, 140f),
        };

        private static readonly LabelProps LabelPreset = new()
        {
            Style = new Style
            {
                (TextColor, new UColor(0.35f, 0.35f, 0.35f, 1f)),
                ("unityFontStyle", "bold"),
            },
        };

        private static readonly LabelProps ValuePreset = new()
        {
            Style = new Style { (TextColor, new UColor(0.1f, 0.1f, 0.1f, 1f)), (MarginLeft, 4f) },
        };

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            string label = p?.TypeText ?? string.Empty;
            string valueText = p?.TypeValue ?? string.Empty;

            var containerProps = new VisualElementProps { Style = ContainerStyle };

            var lblProps = new LabelProps
            {
                Text = string.IsNullOrEmpty(label) ? "" : (label + ":"),
            };

            lblProps.Style = LabelPreset.Style;
            var valProps = new LabelProps { Text = valueText, Style = ValuePreset.Style };

            return V.VisualElement(containerProps, null, V.Label(lblProps), V.Label(valProps));
        }
    }
}
