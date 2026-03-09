using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Test for Step 2: Effect cleanup ordering.
    ///
    /// Two independent child components (Panel A and Panel B) each have a
    /// UseEffect with a cleanup.  When the shared "generation" counter changes,
    /// both effects run in the same commit.
    ///
    /// CORRECT order (React invariant):
    ///   cleanup-A  →  cleanup-B  →  setup-A  →  setup-B
    ///
    /// WRONG order (old bug):
    ///   cleanup-A  →  setup-A  →  cleanup-B  →  setup-B
    ///
    /// The log panel below shows the actual execution order.  All cleanups must
    /// appear before any setups within a single "generation" group.
    /// </summary>
    public static class EffectCleanupOrderDemoFunc
    {
        // Shared mutable log — UseRef-based so it survives re-renders without
        // creating new list instances (which would break ref equality).
        private sealed class LogRef
        {
            public List<string> Entries = new List<string>();
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var (generation, setGeneration) = Hooks.UseState(0);
            var logRef = Hooks.UseRef<LogRef>();

            // Initialise the ref once.
            if (logRef.Current == null)
                logRef.Current = new LogRef();

            // Snapshot log for display (freeze at render time so we display
            // whatever was accumulated up to this render).
            var displayLog = new List<string>(logRef.Current.Entries);

            var containerStyle = new Style
            {
                (StyleKeys.Padding, 14f),
                (StyleKeys.FlexDirection, "column"),
                (StyleKeys.FlexGrow, 1f),
            };

            var sectionStyle = new Style
            {
                (StyleKeys.FlexDirection, "row"),
                (StyleKeys.MarginTop, 10f),
            };

            var logBoxStyle = new Style
            {
                (StyleKeys.MarginTop, 10f),
                (StyleKeys.Padding, 8f),
                (StyleKeys.BackgroundColor, new Color(0.08f, 0.08f, 0.1f, 0.9f)),
                (StyleKeys.BorderRadius, 4f),
            };

            var logEntries = new List<VirtualNode>();
            for (int i = displayLog.Count - 1; i >= 0; i--)
            {
                string entry = displayLog[i];
                Color c = entry.Contains("cleanup")
                    ? new Color(0.9f, 0.55f, 0.2f, 1f)
                    : new Color(0.3f, 0.85f, 0.45f, 1f);
                logEntries.Add(
                    V.Label(
                        new LabelProps
                        {
                            Text = entry,
                            Style = new Style { (StyleKeys.Color, c), (StyleKeys.FontSize, 11f) },
                        }
                    )
                );
            }

            return V.VisualElement(
                new VisualElementProps { Style = containerStyle },
                null,
                V.Text("Effect Cleanup Order — Step 2 Test"),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "All CLEANUPS must appear before any SETUPS within the same generation.",
                        Style = new Style
                        {
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.Color, new Color(0.75f, 0.75f, 0.75f, 1f)),
                        },
                    }
                ),
                V.VisualElement(
                    new VisualElementProps { Style = sectionStyle },
                    null,
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Next Generation (triggers both effects)",
                            OnClick = _ =>
                            {
                                logRef.Current.Entries.Add(
                                    $"── generation {generation + 1} ──────────────"
                                );
                                setGeneration.Set(g => g + 1);
                            },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Clear Log",
                            OnClick = _ =>
                            {
                                logRef.Current.Entries.Clear();
                                setGeneration.Set(g => g); // force re-render to refresh display
                            },
                            Style = new Style { (StyleKeys.MarginLeft, 8f) },
                        }
                    )
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = $"Current generation: {generation}",
                        Style = new Style { (StyleKeys.MarginTop, 6f) },
                    }
                ),
                // Two child panels — both will run effects in the same commit.
                V.Func<EffectPanelFunc.Props>(
                    EffectPanelFunc.Render,
                    new EffectPanelFunc.Props
                    {
                        Label = "Panel A",
                        Generation = generation,
                        Log = logRef.Current,
                    }
                ),
                V.Func<EffectPanelFunc.Props>(
                    EffectPanelFunc.Render,
                    new EffectPanelFunc.Props
                    {
                        Label = "Panel B",
                        Generation = generation,
                        Log = logRef.Current,
                    }
                ),
                V.VisualElement(
                    new VisualElementProps { Style = logBoxStyle },
                    null,
                    displayLog.Count == 0
                        ? V.Text("Log is empty — click 'Next Generation' to start.")
                        : V.Fragment(null, logEntries.ToArray())
                )
            );
        }

        private static class EffectPanelFunc
        {
            public sealed class Props : IProps
            {
                public string Label { get; set; }
                public int Generation { get; set; }
                public LogRef Log { get; set; }
            }

            public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
            {
                var p = rawProps as Props;
                string label = p?.Label ?? "Panel";
                int generation = p?.Generation ?? 0;
                var log = p?.Log;

                Hooks.UseEffect(
                    () =>
                    {
                        log?.Entries.Add(
                            $"  setup   [{label}]  gen={generation}  ({System.DateTime.Now:HH:mm:ss.fff})"
                        );

                        return () =>
                        {
                            log?.Entries.Add(
                                $"  cleanup [{label}]  gen={generation}  ({System.DateTime.Now:HH:mm:ss.fff})"
                            );
                        };
                    },
                    new object[] { generation }
                );

                var panelStyle = new Style
                {
                    (StyleKeys.Padding, 6f),
                    (StyleKeys.MarginTop, 6f),
                    (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.18f, 0.8f)),
                    (StyleKeys.BorderRadius, 4f),
                };

                return V.VisualElement(
                    new VisualElementProps { Style = panelStyle },
                    null,
                    V.Text($"{label}  —  generation {generation}")
                );
            }
        }
    }
}
