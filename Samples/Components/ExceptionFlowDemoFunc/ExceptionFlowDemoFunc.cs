using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class ExceptionFlowDemoFunc
    {
        private sealed class UnstableChildProps : IProps
        {
            public bool ShouldThrow { get; set; }
        }

        private static readonly Style CardStyle = new Style
        {
            (StyleKeys.Padding, 12f),
            (StyleKeys.MarginTop, 6f),
            (StyleKeys.BackgroundColor, new Color32(245, 245, 245, 255)),
            (StyleKeys.Color, Color.black),
            (StyleKeys.BorderRadius, 6f),
        };

        private const float SimulatedLoadSeconds = 1.2f;

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var (shouldThrow, setShouldThrow) = Hooks.UseState(false);
            var (pendingTask, setPendingTask) = Hooks.UseState<Task>(null);

            void ToggleError()
            {
                setShouldThrow(!shouldThrow);
            }

            void StartSimulatedLoad()
            {
                if (pendingTask != null && !pendingTask.IsCompleted)
                {
                    return;
                }

                int delayMs = (int)(SimulatedLoadSeconds * 1000f);
                if (delayMs < 0)
                {
                    delayMs = 0;
                }

                Task loadTask = Task.Run(async () =>
                {
                    await Task.Delay(delayMs);
                });
                setPendingTask(loadTask);
            }

            // Clear pendingTask once it finishes so the button resets to "Simulate Async Load".
            // Without this the parent never re-renders after the task completes, leaving the
            // button stuck on "Loading…" and keeping the old pendingTask reference in state.
            Hooks.UseEffect(
                () =>
                {
                    if (pendingTask == null || pendingTask.IsCompleted)
                        return null;
                    pendingTask.ContinueWith(_ => setPendingTask((Task)null));
                    return null;
                },
                new object[] { pendingTask }
            );

            var actionRow = V.VisualElement(
                new VisualElementProps { Style = new Style { (StyleKeys.FlexDirection, "row") } },
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
                        Text =
                            pendingTask != null && !pendingTask.IsCompleted
                                ? "Loading…"
                                : "Simulate Async Load",
                        OnClick = StartSimulatedLoad,
                        Style = new Style { (StyleKeys.Width, 180f) },
                    }
                )
            );

            var suspenseFallback = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.Padding, 8f),
                        (StyleKeys.BackgroundColor, new Color32(33, 150, 243, 255)),
                        (StyleKeys.Color, Color.white),
                        (StyleKeys.BorderRadius, 4f),
                        (StyleKeys.FlexGrow, 1f),
                        (StyleKeys.MinHeight, 60f),
                        (StyleKeys.AlignItems, "center"),
                        (StyleKeys.JustifyContent, "center"),
                    },
                },
                null,
                V.Text("Suspense fallback: awaiting fake data…")
            );

            var errorFallback = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.Padding, 8f),
                        (StyleKeys.BackgroundColor, new Color32(244, 67, 54, 255)),
                        (StyleKeys.Color, Color.white),
                        (StyleKeys.BorderRadius, 4f),
                        (StyleKeys.FlexGrow, 1f),
                        (StyleKeys.MinHeight, 60f),
                        (StyleKeys.AlignItems, "center"),
                        (StyleKeys.JustifyContent, "center"),
                    },
                },
                null,
                V.Text("Error fallback: boundary caught an exception.")
            );

            VirtualNode ThrowingChild() =>
                V.Func<UnstableChildProps>(
                    UnstableChild,
                    new UnstableChildProps { ShouldThrow = shouldThrow }
                );

            bool SuspenseReady() => pendingTask == null || pendingTask.IsCompletedSuccessfully;

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.Padding, 14f),
                        (StyleKeys.MarginTop, 10f),
                        (StyleKeys.FlexGrow, 1f),
                    },
                },
                null,
                V.Text("Error Boundary + Suspense Control-Flow Demo"),
                actionRow,
                V.VisualElement(
                    new VisualElementProps { Style = CardStyle },
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
                            SuspenseReady,
                            pendingTask,
                            suspenseFallback,
                            "demo-suspense",
                            ThrowingChild()
                        )
                    )
                )
            );
        }

        private static VirtualNode UnstableChild(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as UnstableChildProps;
            bool shouldThrow = p?.ShouldThrow ?? false;
            if (shouldThrow)
            {
                throw new InvalidOperationException("Simulated child exception.");
            }

            return V.Text("Child content rendered successfully.");
        }
    }
}
