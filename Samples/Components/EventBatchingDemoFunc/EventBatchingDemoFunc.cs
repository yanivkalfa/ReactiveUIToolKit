using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class EventBatchingDemoFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (batchedValue, setBatchedValue) = Hooks.UseState(0);
            var (renderCount, setRenderCount) = Hooks.UseState(0);
            var (status, setStatus) = Hooks.UseState("Click a button to enqueue updates.");

            Hooks.UseLayoutEffect(
                () =>
                {
                    setRenderCount.Set(count => count + 1);
                    return null;
                },
                new object[] { batchedValue }
            );

            void TripleUpdate()
            {
                setBatchedValue.Set(v => v + 5);
                setBatchedValue.Set(v => v - 2);
                setBatchedValue.Set(v => v + 1);
                setStatus($"Queued 3 functional updates ({DateTime.Now:HH:mm:ss})");
            }

            void ValueThenUpdater()
            {
                setBatchedValue(10);
                setBatchedValue.Set(v => v - 3);
                setStatus($"Set direct value then updater ({DateTime.Now:HH:mm:ss})");
            }

            void RapidClicks()
            {
                for (int i = 0; i < 5; i++)
                {
                    setBatchedValue.Set(v => v + 1);
                }
                setStatus($"Issued 5 increments ({DateTime.Now:HH:mm:ss})");
            }

            var statBoxStyle = new Style
            {
                (StyleKeys.Padding, 8f),
                (StyleKeys.MarginBottom, 6f),
                (StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.23f, 0.85f)),
                (StyleKeys.BorderRadius, 4f),
            };

            VirtualNode Button(string text, Action onClick) =>
                V.Button(
                    new ButtonProps
                    {
                        Text = text,
                        OnClick = onClick,
                        Style = new Style
                        {
                            (StyleKeys.MinWidth, 180f),
                            (StyleKeys.MarginRight, 6f),
                            (StyleKeys.MarginBottom, 6f),
                        },
                    }
                );

            return V.ScrollView(
                new ScrollViewProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexGrow, 1f),
                        (StyleKeys.Padding, 12f),
                        (StyleKeys.WhiteSpace, "normal"),
                    },
                },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Synthetic event batching test",
                        Style = new Style
                        {
                            (StyleKeys.FontSize, 16f),
                            ("unityFontStyleAndWeight", FontStyle.Bold),
                            (StyleKeys.MarginBottom, 6f),
                        },
                    }
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "Each action sends several state updates during a single event handler. The scheduler wraps handlers in BeginBatch/EndBatch, so renders stay coalesced.",
                        Style = new Style { (StyleKeys.MarginBottom, 6f) },
                    }
                ),
                V.VisualElement(
                    statBoxStyle,
                    null,
                    V.Label(new LabelProps { Text = $"Value: {batchedValue}" }),
                    V.Label(new LabelProps { Text = $"Render count (tracked): {renderCount}" }),
                    V.Label(
                        new LabelProps
                        {
                            Text = status,
                            Style = new Style { (StyleKeys.MarginTop, 4f) },
                        }
                    )
                ),
                V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.FlexWrap, "wrap") },
                    null,
                    Button("Add +5, -2, +1", TripleUpdate),
                    Button("Set 10 then -3", ValueThenUpdater),
                    Button("Loop +1 five times", RapidClicks)
                )
            );
        }
    }
}
