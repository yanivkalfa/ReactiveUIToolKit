using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Test for Step 3: UseEffect is now deferred via the scheduler.
    ///
    /// UseLayoutEffect: always synchronous — runs during the commit phase.
    /// UseEffect:       now deferred  — runs in the next scheduler pump (next frame).
    ///
    /// HOW TO READ THE DISPLAY
    /// ────────────────────────
    /// "Commit #N"   increments in UseLayoutEffect  (synchronous, same frame as render).
    /// "Effect  #N"  increments in UseEffect        (deferred, fires one frame later).
    ///
    /// After the window first opens, or after clicking "Force Re-render":
    ///   • For one frame  →  Commit is ahead of Effect  (Commit=N+1, Effect=N).
    ///   • Next frame     →  Effect catches up           (both = N+1).
    ///
    /// The "Last timestamps" section captures the exact DateTime.Now at which each
    /// callback ran, making the frame-delay directly visible.
    /// </summary>
    public static class DeferredEffectDemoFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (commitCount, setCommitCount) = Hooks.UseState(0);
            var (effectCount, setEffectCount) = Hooks.UseState(0);
            var (lastCommitTime, setLastCommitTime) = Hooks.UseState("—");
            var (lastEffectTime, setLastEffectTime) = Hooks.UseState("—");
            var (trigger, setTrigger) = Hooks.UseState(0);

            // UseLayoutEffect — synchronous with the commit phase.
            Hooks.UseLayoutEffect(
                () =>
                {
                    setCommitCount.Set(n => n + 1);
                    setLastCommitTime.Set(System.DateTime.Now.ToString("HH:mm:ss.fff"));
                    return null;
                },
                new object[] { trigger }
            );

            // UseEffect — deferred to the next scheduler pump (next frame).
            Hooks.UseEffect(
                () =>
                {
                    setEffectCount.Set(n => n + 1);
                    setLastEffectTime.Set(System.DateTime.Now.ToString("HH:mm:ss.fff"));
                    return null;
                },
                new object[] { trigger }
            );

            bool inSync = commitCount == effectCount;

            var containerStyle = new Style
            {
                (StyleKeys.Padding, 14f),
                (StyleKeys.FlexDirection, "column"),
                (StyleKeys.FlexGrow, 1f),
            };

            var cardStyle = new Style
            {
                (StyleKeys.Padding, 10f),
                (StyleKeys.MarginTop, 10f),
                (StyleKeys.BackgroundColor, new Color(0.1f, 0.1f, 0.12f, 0.9f)),
                (StyleKeys.BorderRadius, 4f),
            };

            var syncColor = inSync
                ? new Color(0.3f, 0.85f, 0.45f, 1f)
                : new Color(0.95f, 0.65f, 0.1f, 1f);

            var syncLabelStyle = new Style
            {
                (StyleKeys.MarginTop, 6f),
                (StyleKeys.FontSize, 13f),
                (StyleKeys.Color, syncColor),
            };

            return V.VisualElement(
                containerStyle,
                null,
                V.Text("Deferred UseEffect — Step 3 Test"),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "UseLayoutEffect is synchronous. UseEffect is deferred (next frame via scheduler). "
                            + "Click the button and watch Commit get ahead of Effect for one frame.",
                        Style = new Style
                        {
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.Color, new Color(0.75f, 0.75f, 0.75f, 1f)),
                            (StyleKeys.WhiteSpace, "normal"),
                        },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Force Re-render (change trigger)",
                        OnClick = () => setTrigger.Set(t => t + 1),
                        Style = new Style { (StyleKeys.MarginTop, 10f), (StyleKeys.Width, 240f) },
                    }
                ),
                V.VisualElement(
                    cardStyle,
                    null,
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Commit #  (UseLayoutEffect) : {commitCount}",
                            Style = new Style
                            {
                                (StyleKeys.Color, new Color(0.4f, 0.8f, 1.0f, 1f)),
                            },
                        }
                    ),
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Effect  #  (UseEffect)      : {effectCount}",
                            Style = new Style
                            {
                                (StyleKeys.Color, new Color(0.6f, 1.0f, 0.5f, 1f)),
                                (StyleKeys.MarginTop, 4f),
                            },
                        }
                    ),
                    V.Label(
                        new LabelProps
                        {
                            Text = inSync
                                ? "✔ In sync — both match"
                                : $"⏳ Commit is ahead by {commitCount - effectCount} — effect pending next frame",
                            Style = syncLabelStyle,
                        }
                    )
                ),
                V.VisualElement(
                    cardStyle,
                    null,
                    V.Text("Last timestamps:"),
                    V.Label(
                        new LabelProps
                        {
                            Text = $"  Commit time : {lastCommitTime}",
                            Style = new Style
                            {
                                (StyleKeys.Color, new Color(0.4f, 0.8f, 1.0f, 1f)),
                                (StyleKeys.MarginTop, 4f),
                                (StyleKeys.FontSize, 11f),
                            },
                        }
                    ),
                    V.Label(
                        new LabelProps
                        {
                            Text = $"  Effect time : {lastEffectTime}",
                            Style = new Style
                            {
                                (StyleKeys.Color, new Color(0.6f, 1.0f, 0.5f, 1f)),
                                (StyleKeys.MarginTop, 2f),
                                (StyleKeys.FontSize, 11f),
                            },
                        }
                    )
                )
            );
        }
    }
}
