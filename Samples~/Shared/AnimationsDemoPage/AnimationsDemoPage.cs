using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class AnimationsDemoPage
    {
        public static ReactiveUITK.Core.VirtualNode Render(
            IProps rawProps,
            System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.VirtualNode> children
        )
        {
            var (repeatClickCount, setRepeatClickCount) = Hooks.UseState(0);
            var (animNonce, setAnimNonce) = Hooks.UseState(0);

            var repeatButtonProps = new RepeatButtonProps
            {
                Text = $"Repeat ({repeatClickCount})",
                OnClick = () => setRepeatClickCount(repeatClickCount + 1),
            };

            var repeatPulseTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 1f,
                            To = 0.4f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        new AnimateTrack
                        {
                            Property = "translateY",
                            From = 0f,
                            To = 6f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                    },
                0
            );

            var multiTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 0f,
                            To = 1f,
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                        new AnimateTrack
                        {
                            Property = "translateY",
                            From = 12f,
                            To = 0f,
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                        new AnimateTrack
                        {
                            Property = "width",
                            From = 120f,
                            To = 180f,
                            Duration = 0.6f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        new AnimateTrack
                        {
                            Property = "height",
                            From = 32f,
                            To = 44f,
                            Duration = 0.6f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        new AnimateTrack
                        {
                            Property = "backgroundColor",
                            From = new UColor(0.75f, 0.85f, 1f, 1f),
                            To = new UColor(1f, 1f, 1f, 1f),
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                    },
                animNonce
            );

            var flashAnimTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 1f,
                            To = 0.3f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                    },
                0
            );

            return V.VisualElement(
                new VisualElementProps { Style = new Style { (StyleKeys.FlexDirection, "column"), (Padding, 12f) } },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Animations",
                        Style = new Style { (FontSize, 18f) },
                    }
                ),
                V.Animate(
                    new AnimateProps { Tracks = flashAnimTracks },
                    null,
                    V.VisualElement(
                        new VisualElementProps
                        {
                            Style = new Style
                            {
                                (Width, 160f),
                                (Height, 60f),
                                (BackgroundColor, new UColor(0.3f, 0.6f, 0.9f, 1f)),
                                (BorderRadius, 6f),
                                (JustifyContent, "center"),
                                (AlignItems, "center"),
                                (MarginTop, 6f),
                            },
                        },
                        null,
                        V.Label(
                            new LabelProps
                            {
                                Text = "Flashing Box",
                                Style = new Style { (TextColor, UColor.white) },
                            }
                        )
                    )
                ),
                V.Animate(
                    new AnimateProps { Tracks = multiTracks },
                    null,
                    V.VisualElement(
                        new VisualElementProps
                        {
                            Style = new Style
                            {
                                (Width, 200f),
                                (Height, 120f),
                                (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
                                (BorderRadius, 8f),
                                (JustifyContent, "center"),
                                (AlignItems, "center"),
                                (MarginTop, 8f),
                            },
                        },
                        null,
                        V.Label(new LabelProps { Text = "Animated Card" })
                    )
                ),
                V.Label(new LabelProps { Text = "Animated Repeat Button" }),
                V.Animate(
                    new AnimateProps { Tracks = repeatPulseTracks },
                    null,
                    V.RepeatButton(repeatButtonProps)
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Re-seed Multi-track",
                        OnClick = () => setAnimNonce(animNonce + 1),
                    }
                )
            );
        }
    }
}
