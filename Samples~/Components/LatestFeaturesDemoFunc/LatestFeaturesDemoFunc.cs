using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class LatestFeaturesDemoFunc
    {
        private static readonly string[] DefaultItems = { "Alpha", "Bravo", "Charlie", "Delta" };

        private static List<string> CreateDefaultItems()
        {
            return new List<string>(DefaultItems);
        }

        private sealed class MetricsSnapshot
        {
            public long LastRenderMs;
            public int WorkUnits;
            public int Commits;
            public int Slices;
            public int Yields;
            public int EffectsCommitted;
        }

        public static VirtualNode LatestFeaturesDemo(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (items, setItems) = Hooks.UseState(CreateDefaultItems());
            var (pendingTask, setPendingTask) = Hooks.UseState<Task>(null);
            var (loadCount, setLoadCount) = Hooks.UseState(0);
            var (status, setStatus) = Hooks.UseState(
                "Click \"Simulate async load\" to exercise Suspense."
            );
            var (metricsSnapshot, setMetricsSnapshot) = Hooks.UseState<MetricsSnapshot>(null);

            // Capture a single Fiber metrics snapshot so we can show
            // some numbers without causing a render loop.
            Hooks.UseEffect(
                () =>
                {
                    void OnMetrics(Core.Fiber.FiberReconciler.FiberReconcilerMetrics metrics)
                    {
                        setMetricsSnapshot.Set(prev =>
                            prev
                            ?? new MetricsSnapshot
                            {
                                LastRenderMs = metrics.LastRenderMs,
                                WorkUnits = metrics.WorkUnits,
                                Commits = metrics.Commits,
                                Slices = metrics.Slices,
                                Yields = metrics.Yields,
                                EffectsCommitted = metrics.EffectsCommitted,
                            }
                        );
                    }

                    Core.Fiber.FiberReconciler.MetricsEmitted += OnMetrics;
                    return () => Core.Fiber.FiberReconciler.MetricsEmitted -= OnMetrics;
                },
                Array.Empty<object>()
            );

            // Wire async task completion back into state (items + status).
            Hooks.UseEffect(
                () =>
                {
                    Task task = pendingTask;
                    if (task == null)
                    {
                        return null;
                    }

                    bool cancelled = false;
                    SynchronizationContext syncContext = SynchronizationContext.Current;

                    task.ContinueWith(
                        t =>
                        {
                            if (cancelled)
                            {
                                return;
                            }

                            void Apply()
                            {
                                if (t.IsFaulted)
                                {
                                    setStatus("Load failed (check console).");
                                }
                                else if (t.IsCanceled)
                                {
                                    setStatus("Load cancelled.");
                                }
                                else
                                {
                                    setItems(CreateDefaultItems());
                                    setLoadCount.Set(count => count + 1);
                                    setStatus($"Data loaded ({DateTime.Now:HH:mm:ss}).");
                                }

                                setPendingTask((Task)null);
                            }

                            if (syncContext != null)
                            {
                                try
                                {
                                    syncContext.Post(_ => Apply(), null);
                                }
                                catch
                                {
                                    Apply();
                                }
                            }
                            else
                            {
                                Apply();
                            }
                        },
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default
                    );

                    return () =>
                    {
                        cancelled = true;
                    };
                },
                new object[] { pendingTask }
            );

            void ResetOrder()
            {
                setItems(CreateDefaultItems());
            }

            void ReverseOrder()
            {
                if (items.Count == 0)
                {
                    return;
                }
                var reversed = new List<string>(items);
                reversed.Reverse();
                setItems(reversed);
            }

            void RotateOrder()
            {
                if (items.Count == 0)
                {
                    return;
                }

                var rotated = new List<string>(items.Count);
                for (int i = 1; i < items.Count; i++)
                {
                    rotated.Add(items[i]);
                }
                rotated.Add(items[0]);
                setItems(rotated);
            }

            void SimulateAsyncLoad()
            {
                if (pendingTask != null && !pendingTask.IsCompleted)
                {
                    return;
                }

                setStatus("Loading sample data...");
                setItems(new List<string>());

                Task loadTask = Task.Run(async () =>
                {
                    await Task.Delay(1500);
                });
                setPendingTask(loadTask);
            }

            ButtonProps CreateButton(string text, Action onClick) =>
                new ButtonProps
                {
                    Text = text,
                    OnClick = _ => onClick?.Invoke(),
                    Style = new Style { ("minWidth", 140f) },
                };

            VirtualNode controlsRow = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { ("flexDirection", "row"), ("gap", 6f), ("marginBottom", 6f) },
                },
                "controls-row",
                V.Button(
                    CreateButton(
                        pendingTask != null && !pendingTask.IsCompleted
                            ? "Loading..."
                            : "Simulate async load",
                        SimulateAsyncLoad
                    )
                )
            );

            bool SuspenseReady() => pendingTask == null || pendingTask.IsCompletedSuccessfully;

            VirtualNode fallbackContent = V.VisualElement(
                null,
                "suspense-fallback",
                V.Label(new LabelProps { Text = "Loading sample content..." }),
                V.Label(
                    new LabelProps { Text = "This fallback disappears once the Task finishes." }
                )
            );

            string orderText =
                items.Count == 0
                    ? "No items loaded yet. Click the button above to fetch sample data."
                    : string.Join(", ", items);

            VirtualNode reorderControls = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { ("flexDirection", "row"), ("gap", 6f), ("marginBottom", 4f) },
                },
                "reorder-controls",
                V.Button(CreateButton("Reverse order", ReverseOrder)),
                V.Button(CreateButton("Rotate left", RotateOrder)),
                V.Button(CreateButton("Reset order", ResetOrder))
            );

            VirtualNode listSection = V.VisualElement(
                null,
                "lis-section",
                V.Label(
                    new LabelProps
                    {
                        Text = "Keyed diff demo (Point 13): use the controls to reorder.",
                    }
                ),
                reorderControls,
                V.Label(new LabelProps { Text = $"Current order: {orderText}" })
            );

            string metricsLine =
                metricsSnapshot == null
                    ? "Waiting for Fiber metrics..."
                    : $"Last render {metricsSnapshot.LastRenderMs} ms | work units {metricsSnapshot.WorkUnits}, commits {metricsSnapshot.Commits}, slices {metricsSnapshot.Slices}, yields {metricsSnapshot.Yields}, effects {metricsSnapshot.EffectsCommitted}.";

            VirtualNode metricsSection = V.VisualElement(
                null,
                "metrics-section",
                V.Label(new LabelProps { Text = "Diff metrics throttle (Point 12):" }),
                V.Label(new LabelProps { Text = metricsLine })
            );

            VirtualNode suspenseContent = V.VisualElement(
                null,
                "suspense-content",
                V.Label(new LabelProps { Text = $"Completed async loads: {loadCount}" }),
                metricsSection,
                listSection
            );

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { ("padding", 12f), ("flexDirection", "column"), ("gap", 8f) },
                },
                "latest-demo-root",
                V.Label(new LabelProps { Text = "ReactiveUITK Latest Changes Showcase" }),
                controlsRow,
                V.Label(new LabelProps { Text = status }),
                V.Suspense(
                    SuspenseReady,
                    pendingTask,
                    fallbackContent,
                    "latest-demo-suspense",
                    suspenseContent
                )
            );
        }
    }
}
