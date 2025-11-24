using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Minimal Fiber guard demo without any router.
    /// Used to validate that UseState + events work in isolation.
    /// </summary>
    public static class GuardReproFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (enabled, setEnabled) = Hooks.UseState(false);
            var (message, setMessage) =
                Hooks.UseState("Guard disabled; all navigation allowed.");
            var (clicks, setClicks) = Hooks.UseState(0);

            Debug.Log(
                $"[GuardRepro] Render enabled={enabled} clicks={clicks} message='{message}'"
            );

            return V.VisualElement(
                new Style
                {
                    (StyleKeys.FlexDirection, "column"),
                    (StyleKeys.Padding, 10f),
                    (StyleKeys.FlexGrow, 1f),
                },
                null,
                V.Toggle(
                    new ToggleProps
                    {
                        Text = "Guard enabled",
                        Value = enabled,
                        OnChange = evt =>
                        {
                            bool next = evt.newValue;
                            Debug.Log($"[GuardRepro] Toggle OnChange value={next}");
                            setEnabled(next);
                            setMessage(
                                next
                                    ? "Guard ON (confirmation required)."
                                    : "Guard OFF (navigation free)."
                            );
                        },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = $"Test clicks: {clicks}",
                        OnClick = () =>
                        {
                            int next = clicks + 1;
                            Debug.Log($"[GuardRepro] Button OnClick clicks={next}");
                            setClicks(next);
                        },
                        Style = new Style { (StyleKeys.MarginTop, 4f) },
                    }
                ),
                V.Text($"Guard enabled: {enabled}"),
                V.Text(message)
            );
        }
    }
}

