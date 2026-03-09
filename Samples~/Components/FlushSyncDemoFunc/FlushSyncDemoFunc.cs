using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class FlushSyncDemoFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var (batchedCount, setBatchedCount) = Hooks.UseState(0);
            var (syncCount, setSyncCount) = Hooks.UseState(0);

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (FlexDirection, "column"),
                        (PaddingLeft, 12f),
                        (PaddingRight, 12f),
                        (PaddingTop, 12f),
                        (PaddingBottom, 12f),
                        (BackgroundColor, new Color(0.11f, 0.11f, 0.11f, 1f)),
                    },
                },
                null,
                V.Label(new LabelProps { Text = "FlushSync vs batched updates" }),
                V.Label(new LabelProps { Text = $"Batched counter: {batchedCount}" }),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Increment batched",
                        OnClick = _ => setBatchedCount.Set(v => v + 1),
                    }
                ),
                V.Label(new LabelProps { Text = $"FlushSync counter: {syncCount}" }),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Increment with FlushSync",
                        OnClick = _ => Hooks.FlushSync(() => setSyncCount.Set(v => v + 1)),
                    }
                )
            );
        }
    }
}
