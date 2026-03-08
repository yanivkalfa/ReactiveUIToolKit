using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class KeyedDiffLisDemoFunc
    {
        private static readonly string[] SeedItems =
        {
            "Alpha",
            "Bravo",
            "Charlie",
            "Delta",
            "Echo",
            "Foxtrot",
            "Golf",
        };

        private static readonly System.Random Random = new();

        private static List<string> CreateDefaultItems() => new List<string>(SeedItems);

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (items, setItems) = Hooks.UseState(CreateDefaultItems());
            var (operationCount, setOperationCount) = Hooks.UseState(0);

            void IncrementOps() => setOperationCount.Set(count => count + 1);

            void Reset()
            {
                setItems(CreateDefaultItems());
                setOperationCount(0);
            }

            void Reverse()
            {
                var next = new List<string>(items);
                next.Reverse();
                setItems(next);
                IncrementOps();
            }

            void RotateTailToHead()
            {
                if (items.Count == 0)
                {
                    return;
                }
                var next = new List<string>(items.Count);
                next.Add(items[items.Count - 1]);
                for (int i = 0; i < items.Count - 1; i++)
                {
                    next.Add(items[i]);
                }
                setItems(next);
                IncrementOps();
            }

            void Shuffle()
            {
                if (items.Count < 2)
                {
                    return;
                }
                var next = new List<string>(items);
                for (int i = next.Count - 1; i > 0; i--)
                {
                    int j = Random.Next(i + 1);
                    (next[i], next[j]) = (next[j], next[i]);
                }
                setItems(next);
                IncrementOps();
            }

            void InsertNew()
            {
                var next = new List<string>(items);
                string newKey = $"New-{DateTime.Now.Ticks % 1000}";
                int index = next.Count == 0 ? 0 : Random.Next(0, next.Count);
                next.Insert(index, newKey);
                setItems(next);
                IncrementOps();
            }

            void RemoveMiddle()
            {
                if (items.Count == 0)
                {
                    return;
                }
                int index = items.Count / 2;
                var next = new List<string>(items);
                next.RemoveAt(index);
                setItems(next);
                IncrementOps();
            }

            var headerStyle = new Style
            {
                (StyleKeys.FontSize, 16f),
                ("unityFontStyleAndWeight", FontStyle.Bold),
                (StyleKeys.MarginBottom, 6f),
            };

            var hintStyle = new Style
            {
                (StyleKeys.MarginBottom, 6f),
                (StyleKeys.WhiteSpace, "normal"),
            };

            VirtualNode Button(string label, Action onClick) =>
                V.Button(
                    new ButtonProps
                    {
                        Text = label,
                        OnClick = onClick,
                        Style = new Style
                        {
                            (StyleKeys.MinWidth, 135f),
                            (StyleKeys.MarginRight, 6f),
                            (StyleKeys.MarginBottom, 6f),
                        },
                    }
                );

            var controls = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexDirection, "row"),
                        (StyleKeys.FlexWrap, "wrap"),
                        (StyleKeys.MarginBottom, 8f),
                    }
                },
                null,
                Button("Reset order", Reset),
                Button("Reverse order", Reverse),
                Button("Rotate tail → head", RotateTailToHead),
                Button("Shuffle", Shuffle),
                Button("Insert new key", InsertNew),
                Button("Remove middle", RemoveMiddle)
            );

            var rows = new List<VirtualNode>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                string key = items[i];
                rows.Add(
                    V.VisualElement(
                        new VisualElementProps
                        {
                            Style = new Style
                            {
                                (StyleKeys.FlexDirection, "row"),
                                (StyleKeys.AlignItems, "center"),
                                (StyleKeys.PaddingLeft, 10f),
                                (StyleKeys.PaddingRight, 10f),
                                (StyleKeys.PaddingTop, 6f),
                                (StyleKeys.PaddingBottom, 6f),
                                (StyleKeys.MarginBottom, 4f),
                                (StyleKeys.BackgroundColor, new Color(0.17f, 0.17f, 0.2f, 0.85f)),
                                (StyleKeys.BorderRadius, 4f),
                                (StyleKeys.JustifyContent, "space-between"),
                            }
                        },
                        key,
                        V.Label(new LabelProps { Text = $"Key: {key}" }),
                        V.Label(
                            new LabelProps
                            {
                                Text = $"Index {i}",
                                Style = new Style { (StyleKeys.TextColor, Color.cyan) },
                            }
                        )
                    )
                );
            }

            return V.ScrollView(
                new ScrollViewProps
                {
                    Style = new Style { (StyleKeys.Padding, 10f), (StyleKeys.FlexGrow, 1f) },
                },
                null,
                V.Label(new LabelProps { Text = "Keyed diff / LIS test", Style = headerStyle }),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "Use the buttons to reorder, insert, and remove keyed rows. The new keyed diff keeps visual rows mounted while their order changes.",
                        Style = hintStyle,
                    }
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = $"Operations performed: {operationCount}",
                        Style = new Style { (StyleKeys.MarginBottom, 4f) },
                    }
                ),
                controls,
                V.VisualElement(
                    new VisualElementProps { Style = new Style { (StyleKeys.FlexDirection, "column") } },
                    null,
                    rows.ToArray()
                )
            );
        }
    }
}
