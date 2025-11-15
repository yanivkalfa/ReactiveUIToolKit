using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SyntheticEventDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (log, setLog) = Hooks.UseState(
                "Click, drag, or scroll inside the panel to inspect normalized synthetic events."
            );

            void UpdateLog(string label, SyntheticEvent evt)
            {
                if (evt == null)
                {
                    setLog($"{label}: <null event>");
                    return;
                }
                string summary = $"{label}: type={evt.Type}";
                if (evt is SyntheticWheelEvent wheel)
                {
                    summary =
                        $"{label}: Δ={wheel.Delta.x:0.0},{wheel.Delta.y:0.0} pos={wheel.Position.x:0.0},{wheel.Position.y:0.0} button={wheel.Button}";
                }
                else if (evt is SyntheticPointerEvent pointer)
                {
                    summary =
                        $"{label}: pointerId={pointer.PointerId} pos={pointer.Position.x:0.0},{pointer.Position.y:0.0} button={pointer.Button} clicks={pointer.ClickCount} pressure={pointer.Pressure:0.00}";
                }
                setLog(summary);
            }

            var interactiveArea = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (StyleKeys.Height, 200f),
                            (StyleKeys.BorderRadius, 6f),
                            (StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.25f, 0.9f)),
                            (StyleKeys.JustifyContent, "center"),
                            (StyleKeys.AlignItems, "center"),
                            (StyleKeys.MarginBottom, 10f),
                        }
                    },
                    {
                        "onPointerDown",
                        (Action<SyntheticPointerEvent>)(e => UpdateLog("PointerDown", e))
                    },
                    {
                        "onPointerMove",
                        (Action<SyntheticPointerEvent>)(e => UpdateLog("PointerMove", e))
                    },
                    {
                        "onPointerUp",
                        (Action<SyntheticPointerEvent>)(e => UpdateLog("PointerUp", e))
                    },
                    { "onWheel", (Action<SyntheticWheelEvent>)(e => UpdateLog("Wheel", e)) },
                },
                key: null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Interact here (pointer & wheel events)",
                        Style = new Style { (StyleKeys.TextColor, Color.white) },
                    }
                )
            );

            return V.VisualElement(
                new Style
                {
                    (StyleKeys.Padding, 10f),
                    (StyleKeys.FlexGrow, 1f),
                },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Synthetic Event Inspector",
                        Style = new Style
                        {
                            (StyleKeys.FontSize, 16f),
                            ("unityFontStyleAndWeight", FontStyle.Bold),
                        },
                    }
                ),
                interactiveArea,
                V.Label(
                    new LabelProps
                    {
                        Text = log,
                        Style = new Style
                        {
                            (StyleKeys.WhiteSpace, "normal"),
                            (StyleKeys.BackgroundColor, new Color(0.16f, 0.16f, 0.18f, 0.9f)),
                            (StyleKeys.BorderRadius, 4f),
                            (StyleKeys.Padding, 8f),
                            (StyleKeys.MarginTop, 6f),
                        },
                    }
                )
            );
        }
    }
}
