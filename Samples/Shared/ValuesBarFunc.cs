using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class ValuesBarFunc
    {
        private static readonly Style BarContainerStyle = new()
        {
            (BackgroundColor, UColor.white),
            (BorderBottomWidth, 1f),
            (BorderBottomColor, new UColor(0.85f, 0.85f, 0.85f, 1f)),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
        };

        // Vertical scroll with row-wrapping content so items flow to new lines
        private static readonly ScrollViewProps ScrollProps = new()
        {
            Mode = "vertical",
            ContentContainer = new Dictionary<string, object>
            {
                {
                    "style",
                    new Style
                    {
                        (FlexDirection, "row"),
                        (FlexWrap, "wrap"),
                        (AlignItems, "stretch"),
                        (AlignContent, "flex-start"),
                        (FlexGrow, 1f),
                    }
                },
            },
        };

        // props: { "items": IEnumerable<KeyValuePair<string,string>> }
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            IEnumerable<KeyValuePair<string, string>> items = Array.Empty<
                KeyValuePair<string, string>
            >();
            if (
                props != null
                && props.TryGetValue("items", out var it)
                && it is IEnumerable<KeyValuePair<string, string>> list
            )
            {
                items = list;
            }

            var containerProps = new Dictionary<string, object> { { "style", BarContainerStyle } };

            var itemNodes = new List<VirtualNode>();
            foreach (var kv in items)
            {
                itemNodes.Add(
                    V.Func(
                        ValueItemFunc.Render,
                        new Dictionary<string, object>
                        {
                            { "typeText", kv.Key },
                            { "typeValue", kv.Value },
                        }
                    )
                );
            }

            return V.VisualElement(
                containerProps,
                null,
                V.ScrollView(ScrollProps, null, itemNodes.ToArray())
            );
        }
    }
}
