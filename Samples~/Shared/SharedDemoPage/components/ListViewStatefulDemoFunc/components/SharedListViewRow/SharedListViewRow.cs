using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedListViewRow
    {
        public sealed class Props : IProps
        {
            public SharedRowItem Item { get; set; }
            public int Index { get; set; } = -1;
            public Action<SharedRowItem> OnRemove { get; set; }
        }

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            var rowItem = p?.Item;
            int index = p?.Index ?? -1;
            var onRemove = p?.OnRemove;
            var (counter, setCounter) = Hooks.UseState(0);
            string display = rowItem?.Text ?? "<null>";
            string key =
                rowItem != null
                    ? ($"shared-row-{rowItem.Id}-{index}")
                    : ($"shared-row-missing-{index}");
            return V.VisualElement(
                new VisualElementProps { Style = new Style { (FlexDirection, "row"), (AlignItems, "center") } },
                key: key,
                V.Button(
                    new ButtonProps
                    {
                        Text = "+",
                        OnClick = () => setCounter(counter + 1),
                        Style = new Style { (Width, 24f), (Height, 18f), (MarginRight, 6f) },
                    }
                ),
                V.Text(display),
                V.Button(
                    new ButtonProps
                    {
                        Text = " X ",
                        OnClick = () => onRemove?.Invoke(rowItem),
                        Style = new Style { (MarginLeft, 8f), (Width, 24f), (Height, 18f) },
                    }
                ),
                V.Text(" Count: " + counter, key: key + "-count")
            );
        }
    }
}
