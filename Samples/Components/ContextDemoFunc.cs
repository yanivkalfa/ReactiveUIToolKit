using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class ContextDemoFunc
    {
        private const string ThemeKey = "context-demo-theme";

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (usePrimaryTheme, setUsePrimaryTheme) = Hooks.UseState(true);

            Color themeColor = usePrimaryTheme
                ? new Color(0.23f, 0.65f, 0.95f, 1f)
                : new Color(0.95f, 0.4f, 0.35f, 1f);

            Hooks.ProvideContext(ThemeKey, themeColor);

            return V.VisualElement(
                new Style
                {
                    (StyleKeys.FlexDirection, "column"),
                    (StyleKeys.MarginBottom, 0f),
                    (StyleKeys.PaddingBottom, 0f),
                    (StyleKeys.Padding, 12f),
                    (StyleKeys.BackgroundColor, new Color(0.12f, 0.12f, 0.12f, 1f)),
                    (StyleKeys.FlexGrow, 1f),
                },
                null,
                V.Text("Context Demo (toggle theme to see consumers update)"),
                V.Button(
                    new ButtonProps
                    {
                        Text = usePrimaryTheme ? "Switch To Warm Theme" : "Switch To Cool Theme",
                        OnClick = () => setUsePrimaryTheme.Set(prev => !prev),
                        Style = new Style { (StyleKeys.Width, 220f) },
                    }
                ),
                V.Func(ContextConsumer.Render),
                V.Func(
                    ContextConsumer.Render,
                    new Dictionary<string, object> { { "label", "Secondary Panel" } }
                )
            );
        }

        private static class ContextConsumer
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var theme = Hooks.UseContext<Color>(ThemeKey);
                string label =
                    props != null && props.TryGetValue("label", out var raw) && raw is string text
                        ? text
                        : "Primary Panel";

                var style = new Style
                {
                    (StyleKeys.Padding, 10f),
                    (StyleKeys.BorderRadius, 6f),
                    (StyleKeys.BackgroundColor, theme),
                    (StyleKeys.Color, Color.black),
                };

                // Add extra spacing between primary and secondary panels
                if (label == "Secondary Panel")
                {
                    style[StyleKeys.MarginTop] = 10f;
                }

                return V.VisualElement(
                    style,
                    null,
                    V.Text($"{label}: rgba({theme.r:F2}, {theme.g:F2}, {theme.b:F2})")
                );
            }
        }
    }
}
