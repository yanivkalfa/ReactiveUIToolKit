using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class PropTypesDemoFunc
    {
        private static readonly PropTypeDefinition[] BadgePropTypes =
        {
            PropTypes.String("label", required: true),
            PropTypes.InstanceOf<Color>("statusColor", required: true),
        };

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (useInvalid, setUseInvalid) = Hooks.UseState(false);

            var badgeProps = new Dictionary<string, object>
            {
                { "label", useInvalid ? (object)123 : "All systems nominal" },
                { "statusColor", useInvalid ? (object)"red" : Color.green },
            };

            var badge = V.Func(StatusBadgeFunc.Render, badgeProps).WithPropTypes(BadgePropTypes);

            string modeText = useInvalid
                ? "Invalid props (watch console for warnings)"
                : "Valid props";

            return V.VisualElement(
                new Style { (StyleKeys.FlexGrow, 1f), (StyleKeys.Padding, 10f) },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "PropTypes demo",
                        Style = new Style
                        {
                            (StyleKeys.FontSize, 16f),
                            ("unityFontStyleAndWeight", FontStyle.Bold),
                        },
                    }
                ),
                badge,
                V.Label(
                    new LabelProps
                    {
                        Text = modeText,
                        Style = new Style { (StyleKeys.WhiteSpace, "normal") },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = useInvalid ? "Switch to valid props" : "Inject invalid props",
                        OnClick = () => setUseInvalid(!useInvalid),
                        Style = new Style
                        {
                            (StyleKeys.MinWidth, 160f),
                            (StyleKeys.MarginBottom, 6f),
                        },
                    }
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "React-style prop-type warnings will appear in the Unity Console when invalid props are supplied.",
                        Style = new Style { (StyleKeys.WhiteSpace, "normal") },
                    }
                )
            );
        }

        private static class StatusBadgeFunc
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                string label = "<missing>";
                if (props != null && props.TryGetValue("label", out var lbl) && lbl is string text)
                {
                    label = text;
                }
                Color color = new Color(0.3f, 0.3f, 0.3f);
                if (
                    props != null
                    && props.TryGetValue("statusColor", out var col)
                    && col is Color typedColor
                )
                {
                    color = typedColor;
                }

                return V.VisualElement(
                    new Style
                    {
                        (StyleKeys.Padding, 12f),
                        (StyleKeys.BorderRadius, 6f),
                        (StyleKeys.BackgroundColor, new Color(color.r, color.g, color.b, 0.2f)),
                        (StyleKeys.BorderWidth, 1f),
                        (StyleKeys.BorderColor, color),
                    },
                    null,
                    V.Label(
                        new LabelProps
                        {
                            Text = label,
                            Style = new Style
                            {
                                (StyleKeys.TextColor, color),
                                (StyleKeys.UnityTextOutlineColor, new Color(0f, 0f, 0f, 0.4f)),
                            },
                        }
                    )
                );
            }
        }
    }
}
