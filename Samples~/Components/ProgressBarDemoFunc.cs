using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class ProgressBarDemoFunc
    {
        private static readonly Style ContainerStyle = new()
        {
            (FlexDirection, "column"),
            (Padding, 16f),
        };

        private static readonly Style ProgressBarStyle = new()
        {
            (BorderWidth, 2f),
            (BorderColor, new Color(0.07f, 0.9f, 0.22f, 1f)),
            (BackgroundColor, new Color(0.02f, 0.2f, 0.02f, 0.7f)),
            (BorderRadius, 6f),
            (Height, 30f),
            (JustifyContent, "center"),
            (PaddingLeft, 0f),
            (PaddingRight, 0f),
            (PaddingTop, 0f),
            (PaddingBottom, 0f),
        };

        private static readonly Style ProgressFillStyle = new()
        {
            (BackgroundColor, new Color(0.4f, 0.95f, 0.4f, 0.7f)),
            (BorderRadius, 4f),
            (MarginLeft, 2f),
            (MarginRight, 2f),
            (MarginTop, 2f),
            (MarginBottom, 2f),
        };

        private static readonly Style ProgressTitleStyle = new()
        {
            (TextColor, new Color(0.92f, 1f, 0.92f, 1f)),
            (FontSize, 13f),
        };

        private static readonly Dictionary<string, object> ProgressSlot = new()
        {
            { "style", ProgressFillStyle },
        };

        private static readonly Dictionary<string, object> TitleSlot = new()
        {
            { "style", ProgressTitleStyle },
        };

        private static readonly Style ButtonRowStyle = new()
        {
            (FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (MarginTop, 8f),
        };

        private static readonly Style ButtonStyle = new()
        {
            (FlexGrow, 1f),
            (MarginLeft, 4f),
            (MarginRight, 4f),
        };

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (progress, setProgress) = Hooks.UseState(15f);
            float Clamp(float value) => Mathf.Clamp(value, 0f, 100f);

            VirtualNode progressBar = V.ProgressBar(
                new ProgressBarProps
                {
                    Value = progress,
                    Title = $"Downloading - {progress:0}%",
                    Style = ProgressBarStyle,
                    Progress = ProgressSlot,
                    TitleElement = TitleSlot,
                }
            );

            VirtualNode buttons = V.VisualElement(
                ButtonRowStyle,
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "-10%",
                        OnClick = () => setProgress.Set(value => Clamp(value - 10f)),
                        Style = ButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "+10%",
                        OnClick = () => setProgress.Set(value => Clamp(value + 10f)),
                        Style = ButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Reset",
                        OnClick = () => setProgress.Set(15f),
                        Style = ButtonStyle,
                    }
                )
            );

            return V.VisualElement(
                ContainerStyle,
                null,
                V.Text("ProgressBar component demo"),
                progressBar,
                V.Text($"Current value: {progress:0}%"),
                buttons
            );
        }
    }
}
