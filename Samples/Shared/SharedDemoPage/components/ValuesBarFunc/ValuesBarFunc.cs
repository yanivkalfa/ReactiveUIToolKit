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
        public sealed class Props : IProps
        {
            public IEnumerable<KeyValuePair<string, string>> Items { get; set; }
        }

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

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            IEnumerable<KeyValuePair<string, string>> items =
                p?.Items ?? Array.Empty<KeyValuePair<string, string>>();

            var containerProps = new Dictionary<string, object> { { "style", BarContainerStyle } };

            var itemNodes = new List<VirtualNode>();
            foreach (var kv in items)
            {
                itemNodes.Add(
                    V.Func<ValueItemFunc.Props>(
                        ValueItemFunc.Render,
                        new ValueItemFunc.Props { TypeText = kv.Key, TypeValue = kv.Value }
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
