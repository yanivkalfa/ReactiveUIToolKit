using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class BottomBarFunc
    {
        public sealed class Props : IProps
        {
            public string InputValue { get; set; }
            public System.Action<string> SetTextValue { get; set; }
        }

        private static readonly Style BarStyle = new()
        {
            (BackgroundColor, UColor.white),
            (FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (AlignItems, "center"),
            (Position, "relative"),
            (FlexGrow, 1f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BorderTopWidth, 1f),
            (BorderTopColor, new UColor(0.85f, 0.85f, 0.85f, 1f)),
        };

        private static readonly Style ButtonBaseStyle = new()
        {
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f),
        };

        private static readonly Style LeftButtonStyle = new(ButtonBaseStyle)
        {
            (BackgroundColor, new UColor(0.2f, 0.6f, 0.3f, 1f)),
        };

        private static readonly Style RightButtonStyle = new(ButtonBaseStyle)
        {
            (BackgroundColor, new UColor(0.7f, 0.2f, 0.6f, 1f)),
        };

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (leftClicks, setLeftClicks) = Hooks.UseState(0);
            var (rightClicks, setRightClicks) = Hooks.UseState(0);

            var p = rawProps as Props;
            string inputValue = p?.InputValue ?? string.Empty;
            System.Action<string> setParentText = p?.SetTextValue;

            var leftButtonProps = new ButtonProps
            {
                Style = LeftButtonStyle,
                OnClick = () =>
                {
                    setLeftClicks(leftClicks + 1);
                    setParentText?.Invoke("left");
                },
                Text = $"Bottom Left ({leftClicks})",
            };

            var rightButtonProps = new ButtonProps
            {
                Style = RightButtonStyle,
                OnClick = () =>
                {
                    setRightClicks(rightClicks + 1);
                    setParentText?.Invoke("right");
                },
                Text = $"Bottom Right ({rightClicks})",
            };

            var barProps = new Dictionary<string, object> { { "style", BarStyle } };

            return V.VisualElement(
                barProps,
                key: "bottom-bar-root",
                V.Label(
                    new LabelProps
                    {
                        Text = string.IsNullOrEmpty(inputValue)
                            ? "Type above..."
                            : ("Typed: " + inputValue),
                    },
                    key: "bottom-bar-text"
                ),
                V.Button(leftButtonProps),
                V.Button(rightButtonProps)
            );
        }
    }
}
