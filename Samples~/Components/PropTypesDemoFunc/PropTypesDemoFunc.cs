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
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var (useDefaults, setUseDefaults) = Hooks.UseState(false);

            var badgeProps = new StatusBadgeFunc.Props
            {
                Label = useDefaults ? null : "All systems nominal",
                StatusColor = useDefaults ? new Color(0.3f, 0.3f, 0.3f) : Color.green,
            };

            var badge = V.Func<StatusBadgeFunc.Props>(StatusBadgeFunc.Render, badgeProps);

            string modeText = useDefaults
                ? "Missing props (null label shows fallback)"
                : "Valid props";

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { (StyleKeys.FlexGrow, 1f), (StyleKeys.Padding, 10f) },
                },
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
                        Text = useDefaults ? "Switch to valid props" : "Show default fallbacks",
                        OnClick = _ => setUseDefaults(!useDefaults),
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
                            "With typed props, the C# type system enforces prop correctness at compile time. "
                            + "Toggle above to pass null/default values and see the badge fallback.",
                        Style = new Style { (StyleKeys.WhiteSpace, "normal") },
                    }
                )
            );
        }

        private static class StatusBadgeFunc
        {
            public sealed class Props : IProps
            {
                public string Label { get; set; }
                public Color StatusColor { get; set; } = new Color(0.3f, 0.3f, 0.3f);
            }

            public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
            {
                var p = rawProps as Props;
                string label = p?.Label ?? "<missing>";
                Color color = p?.StatusColor ?? new Color(0.3f, 0.3f, 0.3f);

                return V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.Padding, 12f),
                            (StyleKeys.BorderRadius, 6f),
                            (StyleKeys.BackgroundColor, new Color(color.r, color.g, color.b, 0.2f)),
                            (StyleKeys.BorderWidth, 1f),
                            (StyleKeys.BorderColor, color),
                        },
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
