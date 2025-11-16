using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class PortalEventScopeDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            if (props == null || !props.TryGetValue("portalTarget", out var targetObj) || targetObj is not VisualElement portalTarget)
            {
                return V.Label(new LabelProps { Text = "Portal target not supplied." });
            }

            var (mounted, setMounted) = Hooks.UseState(true);
            var (log, setLog) = Hooks.UseState("Click the portal button to verify event scoping.");

            void AppendLog(string message)
            {
                setLog.Set(prev => $"{DateTime.Now:HH:mm:ss} {message}\n" + (prev ?? string.Empty));
            }

            var portalNode = V.Portal(
                portalTarget,
                null,
                mounted
                    ? V.Button(
                        new ButtonProps
                        {
                            Text = "Portal Button (click me)",
                            OnClick = () => AppendLog("Portal button clicked"),
                            Style = new Style { (StyleKeys.MarginTop, 6f) },
                        }
                    )
                    : V.Label(new LabelProps { Text = "Portal unmounted." })
            );

            return V.VisualElement(
                new Style
                {
                    (StyleKeys.FlexGrow, 1f),
                    (StyleKeys.Padding, 10f),
                },
                null,
                V.Label(
                    new LabelProps
                    {
                        Text = "Portal event scope demo",
                        Style = new Style
                        {
                            (StyleKeys.FontSize, 16f),
                            ("unityFontStyleAndWeight", FontStyle.Bold),
                        },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = mounted ? "Unmount Portal" : "Mount Portal",
                        OnClick = () => setMounted(!mounted),
                        Style = new Style { (StyleKeys.MinWidth, 140f), (StyleKeys.MarginBottom, 6f) },
                    }
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = log,
                        Style = new Style
                        {
                            (StyleKeys.WhiteSpace, "normal"),
                            (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 0.9f)),
                            (StyleKeys.Padding, 6f),
                            (StyleKeys.BorderRadius, 4f),
                        },
                    }
                ),
                portalNode
            );
        }
    }
}
