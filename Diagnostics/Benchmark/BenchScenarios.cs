using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Bench
{
    internal static class BenchScenarios
    {
        private static VirtualNode Row(string text, string key = null, bool last = false)
        {
            var style = new Style
            {
                (FlexDirection, "row"),
                (PaddingLeft, 8f),
                (PaddingRight, 8f),
                (PaddingTop, 6f),
                (PaddingBottom, 6f),
                (BackgroundColor, new UColor(0.22f, 0.24f, 0.30f, 1f)),
                (MarginBottom, last ? 0f : 6f),
                (MinHeight, 50f),
            };

            var labelStyle = new Style { (TextColor, UColor.white) };

            return V.VisualElement(
                style,
                key,
                V.Label(new LabelProps { Text = text, Style = labelStyle })
            );
        }

        private static Style Column(int padding = 10) =>
            new()
            {
                (FlexDirection, "column"),
                (FlexGrow, 1f),
                (PaddingLeft, padding),
                (PaddingRight, padding),
                (PaddingTop, padding),
                (PaddingBottom, padding),
                (BackgroundColor, new UColor(0.12f, 0.12f, 0.12f, 1f)),
            };

        public static Action Build(string name) =>
            name switch
            {
                "Smoke" => Smoke(),
                "StaticScreen" => StaticScreen(),
                "PropChurn_500" => PropChurn(500),
                "ListReorder_200" => ListReorder(200),
                "MountUnmount_50x20" => MountUnmount(50, 20),
                "ErrorBoundaryTrip" => ErrorBoundaryTrip(),
                "BigListManual_3000" => BigListManual(3000),
                "SharedDemo" => SharedDemo(),
                _ => null,
            };

        public static Action Smoke()
        {
            var headerStyle = new Style { (TextColor, UColor.white) };
            var vnode = V.VisualElement(
                Column(),
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "🔥 Bench SMOKE — should be visible",
                        Style = headerStyle,
                    }
                ),
                V.VisualElement(
                    new Style { (Height, 48f), (MarginBottom, 8f), (BackgroundColor, UColor.red) },
                    "bar_r"
                ),
                V.VisualElement(
                    new Style
                    {
                        (Height, 48f),
                        (MarginBottom, 8f),
                        (BackgroundColor, UColor.green),
                    },
                    "bar_g"
                ),
                V.VisualElement(
                    new Style { (Height, 48f), (MarginBottom, 8f), (BackgroundColor, UColor.blue) },
                    "bar_b"
                ),
                V.VisualElement(
                    new Style
                    {
                        (Height, 48f),
                        (MarginBottom, 8f),
                        (BackgroundColor, UColor.yellow),
                    },
                    "bar_y"
                ),
                V.VisualElement(
                    new Style { (Height, 48f), (MarginBottom, 0f), (BackgroundColor, UColor.cyan) },
                    "bar_c"
                )
            );
            return () => BenchSharedHost.Render(vnode);
        }

        public static Action StaticScreen()
        {
            var headerStyle = new Style { (TextColor, UColor.white) };
            var vnode = V.VisualElement(
                Column(),
                null,
                V.Label(new LabelProps { Text = "Static Screen", Style = headerStyle }),
                Row("Hello 1"),
                Row("Hello 2"),
                Row("Hello 3", last: true)
            );
            return () => BenchSharedHost.Render(vnode);
        }

        public static Action PropChurn(int n)
        {
            int tick = 0;
            return () =>
            {
                tick++;
                var children = new List<VirtualNode>(n);
                for (int i = 0; i < n; i++)
                {
                    children.Add(Row($"Row {i} :: {tick % 1000}", key: $"k{i}", last: i == n - 1));
                }
                var vnode = V.VisualElement(
                    new Style
                    {
                        (FlexDirection, "column"),
                        (FlexGrow, 1f),
                        (BackgroundColor, new UColor(0.12f, 0.12f, 0.12f, 1f)),
                    },
                    null,
                    children.ToArray()
                );
                BenchSharedHost.Render(vnode);
            };
        }

        public static Action ListReorder(int n)
        {
            var order = new List<int>(n);
            for (int i = 0; i < n; i++)
            {
                order.Add(i);
            }
            float t = 0;
            return () =>
            {
                t += Time.deltaTime;
                if (t > 1f)
                {
                    t = 0;
                    int first = order[0];
                    order.RemoveAt(0);
                    order.Add(first);
                }
                var children = new List<VirtualNode>(n);
                for (int idx = 0; idx < order.Count; idx++)
                {
                    int i = order[idx];
                    children.Add(Row($"Item {i}", key: $"id{i}", last: idx == order.Count - 1));
                }
                var vnode = V.VisualElement(
                    new Style
                    {
                        (FlexDirection, "column"),
                        (FlexGrow, 1f),
                        (BackgroundColor, new UColor(0.12f, 0.12f, 0.12f, 1f)),
                    },
                    null,
                    children.ToArray()
                );
                BenchSharedHost.Render(vnode);
            };
        }

        public static Action MountUnmount(int groups, int perGroup)
        {
            int frame = 0;
            return () =>
            {
                frame++;
                var children = new List<VirtualNode>(groups * perGroup);
                for (int g = 0; g < groups; g++)
                {
                    bool show = ((frame / 30 + g) % 2) == 0;
                    for (int i = 0; i < perGroup; i++)
                    {
                        if (show)
                        {
                            children.Add(Row($"G{g} I{i}", key: $"g{g}_i{i}"));
                        }
                    }
                }
                var vnode = V.VisualElement(
                    new Style
                    {
                        (FlexDirection, "column"),
                        (FlexGrow, 1f),
                        (BackgroundColor, new UColor(0.12f, 0.12f, 0.12f, 1f)),
                    },
                    null,
                    children.ToArray()
                );
                BenchSharedHost.Render(vnode);
            };
        }

        public static Action ErrorBoundaryTrip()
        {
            int frame = 0;
            bool lastShouldThrow = false;
            string resetKey = Guid.NewGuid().ToString("N");

            return () =>
            {
                frame++;
                bool shouldThrow = (frame / 120) % 2 == 0;
                if (lastShouldThrow && !shouldThrow)
                {
                    resetKey = Guid.NewGuid().ToString("N");
                }
                lastShouldThrow = shouldThrow;

                VirtualNode guardedChild = V.Func(
                    (props, children) =>
                    {
                        if (shouldThrow)
                        {
                            throw new InvalidOperationException("Synthetic benchmark error");
                        }
                        return V.Label(
                            new LabelProps
                            {
                                Text = $"Frame {frame}: all good",
                                Style = new Style { (TextColor, UColor.green) },
                            }
                        );
                    },
                    null,
                    "bench_guarded"
                );

                VirtualNode fallback = V.Label(
                    new LabelProps
                    {
                        Text = "Fallback active",
                        Style = new Style { (TextColor, UColor.red) },
                    }
                );

                VirtualNode boundary = V.ErrorBoundary(
                    new ErrorBoundaryProps
                    {
                        Fallback = fallback,
                        OnError = ex =>
                        {
                            Debug.LogWarning("Bench ErrorBoundary captured: " + ex?.Message);
                        },
                        ResetKey = resetKey,
                    },
                    "bench_error_boundary",
                    guardedChild
                );

                var statusLabel = V.Label(
                    new LabelProps
                    {
                        Text = shouldThrow ? "Phase: throwing" : "Phase: stable",
                        Style = new Style { (TextColor, shouldThrow ? UColor.red : UColor.white) },
                    }
                );

                var vnode = V.VisualElement(Column(), null, statusLabel, boundary);

                BenchSharedHost.Render(vnode);
            };
        }

        public static Action BigListManual(int count)
        {
            int start = 0;
            return () =>
            {
                start = (start + 20) % Math.Max(1, count - 100);
                var children = new List<VirtualNode>(100);
                for (int i = 0; i < 100; i++)
                {
                    int id = start + i;
                    children.Add(Row($"Row {id}", key: $"row_{id}", last: i == 99));
                }
                var vnode = V.VisualElement(
                    new Style
                    {
                        (FlexDirection, "column"),
                        (FlexGrow, 1f),
                        (BackgroundColor, new UColor(0.12f, 0.12f, 0.12f, 1f)),
                    },
                    null,
                    children.ToArray()
                );
                BenchSharedHost.Render(vnode);
            };
        }

        public static Action SharedDemo()
        {
            return () =>
            {
                if (BenchSharedHost.SharedDemoRenderer != null)
                {
                    var vnode = BenchSharedHost.SharedDemoRenderer.Invoke();
                    BenchSharedHost.Render(vnode);
                }
                else
                {
                    var vnode = V.VisualElement(
                        Column(),
                        null,
                        V.Label(new LabelProps { Text = "SharedDemo hook not set" }),
                        V.VisualElement(
                            new Style { (Height, 40f), (BackgroundColor, UColor.magenta) },
                            "mag"
                        )
                    );
                    BenchSharedHost.Render(vnode);
                }
            };
        }
    }
}
