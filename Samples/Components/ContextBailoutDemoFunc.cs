using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Test for Step 4: Context propagates through bailed-out subtrees.
    ///
    /// COMPONENT TREE
    /// ──────────────
    ///  ContextBailoutDemoFunc  (Root)
    ///   └─ MiddleLayerFunc     (bails out — its own props never change)
    ///       └─ ThemeConsumerFunc  (reads context via UseContext)
    ///
    /// THE BUG (before the fix)
    /// ────────────────────────
    /// When the Root changed the context value, MiddleLayer bailed out because its
    /// props dictionary was reference-equal. The reconciler skipped its subtree,
    /// so ThemeConsumer never received the new context value.
    ///
    /// THE FIX (Step 4, Hooks.ProvideContext + FiberReconciler)
    /// ─────────────────────────────────────────────────────────
    /// ProvideContext now walks the committed alternate tree and marks every
    /// context-consumer fiber with HasPendingStateUpdate=true, plus every
    /// intermediate fiber with SubtreeHasUpdates=true. The reconciler then
    /// re-enters the bailed-out subtree and updates only the marked consumers.
    ///
    /// HOW TO USE THE DEMO
    /// ────────────────────
    /// • Click "Toggle Theme" several times.
    /// • The color swatch must change each click.
    /// • "Consumer rendered N times" must increase each click.
    /// • "Middle layer rendered N times" should ideally NOT increase (it bailed out),
    ///   but it may if the reconciler chose to re-render it as part of propagation.
    ///   Either way the consumer seeing the new color is the key assertion.
    /// </summary>
    public static class ContextBailoutDemoFunc
    {
        // ─────────────────────────────
        //  Inner: Middle layer (stable props → bails out)
        // ─────────────────────────────
        private static readonly Dictionary<string, object> s_middleStaticProps = new Dictionary<
            string,
            object
        >
        {
            { "id", "middle" },
        };

        private static VirtualNode MiddleLayerRender(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            // UseRef: mutation during render doesn't trigger re-renders → no infinite loop.
            var renderCountRef = Hooks.UseRef(0);
            renderCountRef.Value++;
            int renderCount = renderCountRef.Value;

            var color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var style = new Style
            {
                (StyleKeys.Padding, 10f),
                (StyleKeys.MarginTop, 8f),
                (StyleKeys.BackgroundColor, color),
                (StyleKeys.BorderRadius, 4f),
            };

            return V.VisualElement(
                style,
                null,
                V.Label(
                    new LabelProps
                    {
                        Text =
                            $"Middle layer rendered {renderCount} times  (props never change → expects bailout)",
                        Style = new Style
                        {
                            (StyleKeys.Color, new Color(0.7f, 0.7f, 0.7f, 1f)),
                            (StyleKeys.FontSize, 11f),
                        },
                    }
                ),
                V.Func(ThemeConsumerRender)
            );
        }

        // ─────────────────────────────
        //  Inner: Consumer — reads context
        // ─────────────────────────────
        private static VirtualNode ThemeConsumerRender(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            // UseRef: mutation during render doesn't trigger re-renders → no infinite loop.
            var renderCountRef = Hooks.UseRef(0);
            renderCountRef.Value++;
            int renderCount = renderCountRef.Value;

            var themeColor = Hooks.UseContext<Color>("bailout-test-theme");

            var swatchStyle = new Style
            {
                (StyleKeys.Width, 60f),
                (StyleKeys.Height, 30f),
                (StyleKeys.BackgroundColor, themeColor),
                (StyleKeys.MarginTop, 6f),
                (StyleKeys.BorderRadius, 3f),
            };

            var consumerCard = new Style
            {
                (StyleKeys.Padding, 10f),
                (StyleKeys.MarginTop, 8f),
                (StyleKeys.BackgroundColor, new Color(0.08f, 0.08f, 0.1f, 1f)),
                (StyleKeys.BorderRadius, 4f),
            };

            string rgb = $"R={themeColor.r:F2}  G={themeColor.g:F2}  B={themeColor.b:F2}";

            return V.VisualElement(
                consumerCard,
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Theme Consumer  (UseContext)",
                        Style = new Style { (StyleKeys.Color, new Color(0.4f, 0.85f, 1f, 1f)) },
                    }
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = $"Consumer rendered {renderCount} times",
                        Style = new Style
                        {
                            (StyleKeys.Color, new Color(0.6f, 1.0f, 0.5f, 1f)),
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.FontSize, 11f),
                        },
                    }
                ),
                V.VisualElement(swatchStyle, null),
                V.Label(
                    new LabelProps
                    {
                        Text = rgb,
                        Style = new Style
                        {
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.FontSize, 11f),
                            (StyleKeys.Color, new Color(0.85f, 0.85f, 0.85f, 1f)),
                        },
                    }
                )
            );
        }

        // ─────────────────────────────
        //  Root
        // ─────────────────────────────
        private static readonly Color s_blue = new Color(0.2f, 0.45f, 0.9f, 1f);
        private static readonly Color s_orange = new Color(0.9f, 0.5f, 0.15f, 1f);

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (useBlue, setUseBlue) = Hooks.UseState(true);
            Color themeColor = useBlue ? s_blue : s_orange;

            Hooks.ProvideContext("bailout-test-theme", themeColor);

            var containerStyle = new Style
            {
                (StyleKeys.Padding, 14f),
                (StyleKeys.FlexDirection, "column"),
                (StyleKeys.FlexGrow, 1f),
            };

            return V.VisualElement(
                containerStyle,
                null,
                V.Text("Context Through Bailout — Step 4 Test"),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            "Root provides a context color. Middle layer's props never change so "
                            + "it bails out. Without the fix the consumer would be stuck on the "
                            + "initial color. With the fix the consumer re-renders every toggle.",
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
                        Text = $"Toggle Theme  (now: {(useBlue ? "Blue" : "Orange")})",
                        OnClick = () => setUseBlue.Set(b => !b),
                        Style = new Style { (StyleKeys.MarginTop, 10f), (StyleKeys.Width, 220f) },
                    }
                ),
                V.Func(
                    MiddleLayerRender,
                    s_middleStaticProps // static ref — never changes → triggers bailout
                )
            );
        }
    }
}
