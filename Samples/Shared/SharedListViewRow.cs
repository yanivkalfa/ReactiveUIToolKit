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
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            props ??= new Dictionary<string, object>();
            props.TryGetValue("item", out var itemObj);
            props.TryGetValue("index", out var indexObj);
            props.TryGetValue("onRemove", out var removeObj);
            var rowItem = itemObj as SharedRowItem;
            int index = indexObj is int i ? i : -1;
            var onRemove = removeObj as Action<SharedRowItem>;
            var (counter, setCounter) = Hooks.UseState(0);
            string display = rowItem?.Text ?? "<null>";
            string key =
                rowItem != null
                    ? ($"shared-row-{rowItem.Id}-{index}")
                    : ($"shared-row-missing-{index}");
            return V.VisualElement(
                new Style { (FlexDirection, "row"), (AlignItems, "center") },
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
