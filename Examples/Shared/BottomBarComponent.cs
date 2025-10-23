using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Examples.Shared
{
    public sealed class BottomBarComponent : ReactiveComponent
    {
        private int leftClicks;
        private int rightClicks;
        private string inputValueCache;

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
            (BorderTopColor, new UColor(0.85f, 0.85f, 0.85f, 1f))
        };

        private static readonly Style ButtonBaseStyle = new()
        {
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f)
        };

        private static readonly Style LeftButtonStyle = new(ButtonBaseStyle)
        {
            (BackgroundColor, new UColor(0.2f, 0.6f, 0.3f, 1f))
        };

        private static readonly Style RightButtonStyle = new(ButtonBaseStyle)
        {
            (BackgroundColor, new UColor(0.7f, 0.2f, 0.6f, 1f))
        };

        protected override VirtualNode Render()
        {
            string inputValue = string.Empty;
            if (Props != null && Props.TryGetValue("inputValue", out var v) && v is string s)
            {
                inputValue = s;
            }
            inputValueCache = inputValue;

            System.Action<string> setParentText = null;
            if (Props != null && Props.TryGetValue("setTextValue", out var setterObj) && setterObj is System.Action<string> setter)
            {
                setParentText = setter;
            }

            var leftButtonProps = new ButtonProps
            {
                Style = LeftButtonStyle,
                OnClick = () =>
                {
                    SetState(ref leftClicks, leftClicks + 1);
                    setParentText?.Invoke("left");
                },
                Text = $"Bottom Left ({leftClicks})"
            };

            var rightButtonProps = new ButtonProps
            {
                Style = RightButtonStyle,
                OnClick = () =>
                {
                    SetState(ref rightClicks, rightClicks + 1);
                    setParentText?.Invoke("right");
                },
                Text = $"Bottom Right ({rightClicks})"
            };

            var barProps = new Dictionary<string, object> { { "style", BarStyle } };

            return V.VisualElement(barProps, null,
                V.Text(string.IsNullOrEmpty(inputValueCache) ? "Type above..." : ("Typed: " + inputValueCache)),
                V.Button(leftButtonProps),
                V.Button(rightButtonProps)
            );
        }
    }
}
