using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class ExceptionFlowDemoFunc
    {
        private static readonly Style CardStyle = new Style
        {
            (StyleKeys.Padding, 12f),
            (StyleKeys.MarginTop, 6f),
            (StyleKeys.BackgroundColor, new Color32(245, 245, 245, 255)),
            (StyleKeys.Color, Color.black),
            (StyleKeys.BorderRadius, 6f),
        };
        private const float SimulatedLoadSeconds = 1.2f;

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (shouldThrow, setShouldThrow) = Hooks.UseState(false);
            var (suspenseReady, setSuspenseReady) = Hooks.UseState(true);
            var scheduler = Hooks.UseContext<IScheduler>("scheduler");

            Hooks.UseEffect(
                () =>
                {
                    if (suspenseReady || scheduler == null)
                    {
                        return null;
                    }

                    bool cancelled = false;
                    float resumeAt = Time.realtimeSinceStartup + SimulatedLoadSeconds;

                    void Pump()
                    {
                        if (cancelled)
                        {
                            return;
                        }
                        if (Time.realtimeSinceStartup >= resumeAt)
                        {
                            Hooks.FlushSync(() => setSuspenseReady(true));
                        }
                        else
                        {
                            scheduler.Enqueue(Pump, IScheduler.Priority.Low);
                        }
                    }

                    scheduler.Enqueue(Pump, IScheduler.Priority.Low);

                    return () =>
                    {
                        cancelled = true;
                    };
                },
                new object[] { suspenseReady }
            );

            void ToggleError()
            {
                setShouldThrow(!shouldThrow);
            }

            void StartSimulatedLoad()
            {
                if (!suspenseReady)
                {
                    return;
                }

                setSuspenseReady(false);

                if (scheduler == null)
                {
                    Hooks.FlushSync(() => setSuspenseReady(true));
                }
            }

            var actionRow = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style { (StyleKeys.FlexDirection, "row") }
                    },
                },
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = shouldThrow ? "Clear Error" : "Trigger Error",
                        OnClick = ToggleError,
                        Style = new Style { (StyleKeys.Width, 140f), (StyleKeys.MarginRight, 8f) },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = suspenseReady ? "Simulate Async Load" : "Loading…",
                        OnClick = StartSimulatedLoad,
                        Style = new Style { (StyleKeys.Width, 180f) },
                    }
                )
            );

            var suspenseFallback = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (StyleKeys.Padding, 8f),
                            (StyleKeys.BackgroundColor, new Color32(33, 150, 243, 255)),
                            (StyleKeys.Color, Color.white),
                            (StyleKeys.BorderRadius, 4f),
                            (StyleKeys.FlexGrow, 1f),
                            (StyleKeys.MinHeight, 60f),
                            (StyleKeys.AlignItems, "center"),
                            (StyleKeys.JustifyContent, "center"),
                        }
                    },
                },
                null,
                V.Text("Suspense fallback: awaiting fake data…")
            );

            var errorFallback = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (StyleKeys.Padding, 8f),
                            (StyleKeys.BackgroundColor, new Color32(244, 67, 54, 255)),
                            (StyleKeys.Color, Color.white),
                            (StyleKeys.BorderRadius, 4f),
                            (StyleKeys.FlexGrow, 1f),
                            (StyleKeys.MinHeight, 60f),
                            (StyleKeys.AlignItems, "center"),
                            (StyleKeys.JustifyContent, "center"),
                        }
                    },
                },
                null,
                V.Text("Error fallback: boundary caught an exception.")
            );

            VirtualNode ThrowingChild() =>
                V.Func(
                    UnstableChild.Render,
                    new Dictionary<string, object> { { "shouldThrow", shouldThrow } }
                );

            return V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (StyleKeys.Padding, 14f),
                            (StyleKeys.MarginTop, 10f),
                            (StyleKeys.FlexGrow, 1f),
                        }
                    },
                },
                null,
                V.Text("Error Boundary + Suspense Control-Flow Demo"),
                actionRow,
                V.VisualElement(
                    new Dictionary<string, object> { { "style", CardStyle } },
                    null,
                    V.Text("Content area"),
                    V.ErrorBoundary(
                        new ErrorBoundaryProps
                        {
                            Fallback = errorFallback,
                            OnError = ex => Debug.LogWarning($"Boundary captured: {ex?.Message}"),
                            ResetKey = shouldThrow ? "throw" : "clear",
                        },
                        "demo-boundary",
                        V.Suspense(
                            () => suspenseReady,
                            suspenseFallback,
                            "demo-suspense",
                            ThrowingChild()
                        )
                    )
                )
            );
        }

        private static class UnstableChild
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                bool shouldThrow =
                    props != null
                    && props.TryGetValue("shouldThrow", out var raw)
                    && raw is bool flag
                    && flag;
                if (shouldThrow)
                {
                    throw new InvalidOperationException("Simulated child exception.");
                }

                return V.Text("Child content rendered successfully.");
            }
        }
    }
}
