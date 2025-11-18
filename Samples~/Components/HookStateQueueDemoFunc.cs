using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class HookStateQueueDemoFunc
    {
        private static List<string> BootstrapLog() =>
            new List<string> { "Click a button to queue multiple state updates." };

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (value, setValue) = Hooks.UseState(0);
            var (flushCount, setFlushCount) = Hooks.UseState(0);
            var (logEntries, setLogEntries) = Hooks.UseState(BootstrapLog());

            Hooks.UseLayoutEffect(
                () =>
                {
                    setFlushCount.Set(count => count + 1);
                    return null;
                },
                new object[] { value }
            );

            void AppendLog(string message)
            {
                var next = new List<string>(logEntries.Count + 1)
                {
                    $"{DateTime.Now:HH:mm:ss}: {message}",
                };
                next.AddRange(logEntries);
                if (next.Count > 8)
                {
                    next.RemoveRange(8, next.Count - 8);
                }
                setLogEntries(next);
            }

            void TripleIncrement()
            {
                setValue.Set(v => v + 1);
                setValue.Set(v => v + 1);
                setValue.Set(v => v + 1);
                AppendLog("Queued +1 functional update three times");
            }

            void ResetThenBoost()
            {
                setValue(0);
                setValue.Set(v => v + 5);
                AppendLog("Reset to 0 and applied +5 in the same event");
            }

            void MultiplyAfterAdd()
            {
                setValue.Set(v => v + 1);
                setValue.Set(v => v * 2);
                AppendLog("Applied +1 then ×2 to verify preview chaining");
            }

            var badgeStyle = new Style
            {
                (StyleKeys.BackgroundColor, new Color(0.2f, 0.4f, 0.2f, 0.8f)),
                (StyleKeys.PaddingLeft, 8f),
                (StyleKeys.PaddingRight, 8f),
                (StyleKeys.PaddingTop, 2f),
                (StyleKeys.PaddingBottom, 2f),
                (StyleKeys.BorderRadius, 4f),
                (StyleKeys.MarginRight, 6f),
            };

            VirtualNode Button(string text, Action onClick) =>
                V.Button(
                    new ButtonProps
                    {
                        Text = text,
                        OnClick = onClick,
                        Style = new Style
                        {
                            (StyleKeys.MinWidth, 160f),
                            (StyleKeys.MarginRight, 6f),
                            (StyleKeys.MarginBottom, 6f),
                        },
                    }
                );

            var logNodes = new List<VirtualNode>(logEntries.Count);
            for (int i = 0; i < logEntries.Count; i++)
            {
                string entry = logEntries[i];
                logNodes.Add(
                    V.Label(
                        new LabelProps
                        {
                            Text = entry,
                            Style = new Style { (StyleKeys.MarginBottom, 2f) },
                        },
                        $"log-{i}"
                    )
                );
            }

            return V.ScrollView(
                new ScrollViewProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexGrow, 1f),
                        (StyleKeys.Padding, 10f),
                        (StyleKeys.WhiteSpace, "normal"),
                    },
                },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Hook queue merging test",
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
                            "Each button issues several setState calls back-to-back. The per-hook queue stores them until commit so the final value matches the combined functional updates.",
                        Style = new Style { (StyleKeys.MarginBottom, 8f) },
                    }
                ),
                V.VisualElement(
                    new Style
                    {
                        (StyleKeys.FlexDirection, "row"),
                        (StyleKeys.AlignItems, "center"),
                        (StyleKeys.MarginBottom, 6f),
                    },
                    null,
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Value: {value}",
                            Style = badgeStyle,
                        }
                    ),
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Flush count: {flushCount}",
                            Style = badgeStyle,
                        }
                    )
                ),
                V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.FlexWrap, "wrap") },
                    null,
                    Button("+1 three times", TripleIncrement),
                    Button("Reset then +5", ResetThenBoost),
                    Button("+1 then ×2", MultiplyAfterAdd)
                ),
                V.VisualElement(
                    new Style
                    {
                        (StyleKeys.MarginTop, 8f),
                        (StyleKeys.Padding, 8f),
                        (StyleKeys.BackgroundColor, new Color(0.14f, 0.14f, 0.14f, 0.85f)),
                        (StyleKeys.BorderRadius, 4f),
                    },
                    null,
                    V.Label(
                        new LabelProps
                        {
                            Text = "Recent operations",
                            Style = new Style
                            {
                                (StyleKeys.MarginBottom, 4f),
                                ("unityFontStyleAndWeight", FontStyle.Bold),
                            },
                        }
                    ),
                    V.VisualElement(
                        new Style { (StyleKeys.FlexDirection, "column") },
                        null,
                        logNodes.ToArray()
                    )
                )
            );
        }
    }
}
