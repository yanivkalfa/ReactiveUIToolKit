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
            public long LastDiffMs;
            public int Reconciled;
            public int Skipped;
            public int Effects;
            public int PortalsBuilt;
            public int PortalsUpdated;
        }

        public static VirtualNode Render(
            Dictionary<string, object> props,
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

            Hooks.UseEffect(
                () =>
                {
                    void OnMetrics(Reconciler.ReconcilerMetrics metrics)
                    {
                        setMetricsSnapshot(
                            new MetricsSnapshot
                            {
                                LastDiffMs = metrics.LastDiffMs,
                                Reconciled = metrics.Reconciled,
                                Skipped = metrics.Skipped,
                                Effects = metrics.EffectsRan,
                                PortalsBuilt = metrics.PortalsBuilt,
                                PortalsUpdated = metrics.PortalsUpdated,
                            }
                        );
                    }

                    Reconciler.MetricsEmitted += OnMetrics;
                    return () => Reconciler.MetricsEmitted -= OnMetrics;
                },
                Array.Empty<object>()
            );

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
                    OnClick = onClick,
                    Style = new Style { ("minWidth", 140f) },
                };

            VirtualNode controlsRow = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style { ("flexDirection", "row"), ("gap", 6f), ("marginBottom", 6f) }
                    },
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
                V.Text("Loading sample content...", key: "fallback-line-1"),
                V.Text("This fallback disappears once the Task finishes.", key: "fallback-line-2")
            );

            List<VirtualNode> itemNodes = new List<VirtualNode>(items.Count);
            foreach (string item in items)
            {
                itemNodes.Add(
                    V.VisualElement(
                        null,
                        $"lis-item-{item}",
                        V.Text($"• {item}", key: $"lis-text-{item}")
                    )
                );
            }
            if (itemNodes.Count == 0)
            {
                itemNodes.Add(
                    V.Text(
                        "No items loaded yet. Click the button above to fetch sample data.",
                        key: "lis-empty"
                    )
                );
            }

            VirtualNode reorderControls = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style { ("flexDirection", "row"), ("gap", 6f), ("marginBottom", 4f) }
                    },
                },
                "reorder-controls",
                V.Button(CreateButton("Reverse order", ReverseOrder)),
                V.Button(CreateButton("Rotate left", RotateOrder)),
                V.Button(CreateButton("Reset order", ResetOrder))
            );

            VirtualNode listSection = V.VisualElement(
                null,
                "lis-section",
                V.Text(
                    "Keyed diff demo (Point 13): use the controls to reorder.",
                    key: "lis-heading"
                ),
                reorderControls,
                V.VisualElement(null, "lis-items-host", itemNodes.ToArray())
            );

            string metricsLine =
                metricsSnapshot == null
                    ? "Waiting for diff metrics..."
                    : $"Last diff {metricsSnapshot.LastDiffMs} ms | reconciled {metricsSnapshot.Reconciled}, skipped {metricsSnapshot.Skipped}, effects {metricsSnapshot.Effects}.";

            VirtualNode metricsSection = V.VisualElement(
                null,
                "metrics-section",
                V.Text("Diff metrics throttle (Point 12):", key: "metrics-heading"),
                V.Text(metricsLine, key: "metrics-line")
            );

            VirtualNode suspenseContent = V.VisualElement(
                null,
                "suspense-content",
                V.Text($"Completed async loads: {loadCount}", key: "load-count-line"),
                metricsSection,
                listSection
            );

            return V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style { ("padding", 12f), ("flexDirection", "column"), ("gap", 8f) }
                    },
                },
                "latest-demo-root",
                V.Text("ReactiveUITK Latest Changes Showcase", key: "latest-heading"),
                controlsRow,
                V.Text(status, key: "status-line"),
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
