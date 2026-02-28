using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Test for Step 1: Render depth guard.
    /// Renders a recursively nested tree of function components.
    /// Without the guard this would overflow the stack or loop forever.
    /// With the guard the reconciler caps at MaxRenderDepth (25), logs a clear
    /// error to the console, and returns a null child — so the UI stays alive.
    /// Expected console output when depth is pressed past 25:
    ///   [Fiber] Maximum render depth (25) exceeded in 'DeepNodeFunc' ...
    /// </summary>
    public static class RenderDepthGuardDemoFunc
    {
        private const int SafeDepth = 10;
        private const int DangerDepth = 30; // deliberately exceeds MaxRenderDepth=25

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (depth, setDepth) = Hooks.UseState(0);
            var (triggered, setTriggered) = Hooks.UseState(false);

            var containerStyle = new Style
            {
                (StyleKeys.Padding, 14f),
                (StyleKeys.FlexDirection, "column"),
                (StyleKeys.FlexGrow, 1f),
            };

            var infoStyle = new Style
            {
                (StyleKeys.Padding, 8f),
                (StyleKeys.MarginTop, 8f),
                (StyleKeys.BackgroundColor, new Color(0.1f, 0.1f, 0.1f, 0.7f)),
                (StyleKeys.BorderRadius, 4f),
            };

            var warnStyle = new Style
            {
                (StyleKeys.Padding, 8f),
                (StyleKeys.MarginTop, 6f),
                (StyleKeys.BackgroundColor, new Color(0.5f, 0.15f, 0.1f, 0.8f)),
                (StyleKeys.BorderRadius, 4f),
            };

            return V.VisualElement(
                containerStyle,
                null,
                V.Text("Render Depth Guard — Step 1 Test"),
                V.VisualElement(
                    infoStyle,
                    null,
                    V.Text(
                        "MaxRenderDepth = 25.  A tree deeper than that will be capped and an error "
                            + "will be printed to the console.  The window must NOT freeze or crash."
                    )
                ),
                V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginTop, 10f) },
                    null,
                    V.Button(
                        new ButtonProps
                        {
                            Text = $"Render Safe Tree (depth {SafeDepth})",
                            OnClick = () =>
                            {
                                setDepth(SafeDepth);
                                setTriggered(false);
                            },
                            Style = new Style { (StyleKeys.MarginRight, 8f) },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = $"Trigger Deep Tree (depth {DangerDepth} > 25) ⚠",
                            OnClick = () =>
                            {
                                setDepth(DangerDepth);
                                setTriggered(true);
                            },
                        }
                    )
                ),
                triggered
                    ? V.VisualElement(
                        warnStyle,
                        null,
                        V.Text(
                            "⚠ Deep tree triggered — check Console for '[Fiber] Maximum render depth exceeded' error."
                        )
                    )
                    : V.Fragment(),
                depth > 0
                    ? V.VisualElement(
                        new Style { (StyleKeys.MarginTop, 8f) },
                        null,
                        V.Text($"Tree root (requested depth: {depth}):"),
                        V.Func(
                            DeepNodeFunc.Render,
                            new Dictionary<string, object>
                            {
                                { "remaining", depth },
                                { "maxDisplay", depth },
                            }
                        )
                    )
                    : V.VisualElement(
                        infoStyle,
                        null,
                        V.Text("No tree rendered yet — click a button above.")
                    )
            );
        }

        // A function component that recursively nests itself.
        private static class DeepNodeFunc
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                int remaining =
                    props != null && props.TryGetValue("remaining", out var r) && r is int ri
                        ? ri
                        : 0;
                int maxDisplay =
                    props != null && props.TryGetValue("maxDisplay", out var m) && m is int mi
                        ? mi
                        : 0;

                int currentLevel = maxDisplay - remaining + 1;

                var nodeStyle = new Style
                {
                    (StyleKeys.PaddingLeft, 12f),
                    (StyleKeys.PaddingTop, 2f),
                    (StyleKeys.BorderLeftWidth, 1f),
                    (StyleKeys.BorderLeftColor, new Color(0.4f, 0.4f, 0.4f, 0.5f)),
                    (StyleKeys.MarginTop, 2f),
                };

                if (remaining <= 0)
                {
                    return V.VisualElement(
                        nodeStyle,
                        null,
                        V.Text($"Level {currentLevel}: leaf node")
                    );
                }

                return V.VisualElement(
                    nodeStyle,
                    null,
                    V.Text($"Level {currentLevel}"),
                    V.Func(
                        DeepNodeFunc.Render,
                        new Dictionary<string, object>
                        {
                            { "remaining", remaining - 1 },
                            { "maxDisplay", maxDisplay },
                        }
                    )
                );
            }
        }
    }
}
