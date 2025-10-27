using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class ValueItemFunc
    {
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
            Style = new Style { (TextColor, new UColor(0.35f, 0.35f, 0.35f, 1f)), ("unityFontStyle", "bold") },
        };

        private static readonly LabelProps ValuePreset = new()
        {
            Style = new Style { (TextColor, new UColor(0.1f, 0.1f, 0.1f, 1f)), (MarginLeft, 4f) },
        };

        // props: { "typeText": string, "typeValue": object }
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            string label =
                props != null && props.TryGetValue("typeText", out var lt) && lt != null
                    ? lt.ToString()
                    : string.Empty;
            string valueText =
                props != null && props.TryGetValue("typeValue", out var tv) && tv != null
                    ? tv.ToString()
                    : string.Empty;

            var containerProps = new Dictionary<string, object> { { "style", ContainerStyle } };

            var lblProps = new LabelProps
            {
                Text = string.IsNullOrEmpty(label) ? "" : (label + ":"),
            };
            // apply style presets
            lblProps.Style = LabelPreset.Style;
            var valProps = new LabelProps { Text = valueText, Style = ValuePreset.Style };

            return V.VisualElement(containerProps, null, V.Label(lblProps), V.Label(valProps));
        }
    }
}
