using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public static class MultiColumnListViewStatefulDemoFunc
    {
        private sealed class Row
        {
            public string Id;
            public string Text;
            public bool ShouldOverrideElement;
        }

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var initial = Hooks.UseMemo(() =>
            {
                // Start with 0 rows
                return new List<Row>();
            });
            var (items, setItems) = Hooks.UseState(initial);

            // Notify parent of count when it changes
            Hooks.UseEffect(
                () =>
                {
                    try
                    {
                        if (
                            props != null
                            && props.TryGetValue("onCountChanged", out var oc)
                            && oc is Action<int> cb
                        )
                            cb(items?.Count ?? 0);
                    }
                    catch { }
                    return null;
                },
                new object[] { items?.Count ?? 0 }
            );

            var btnAdd = new ButtonProps
            {
                Text = "Add Parent",
                OnClick = () =>
                {
                    var copy = new List<Row>((items?.Count ?? 0) + 1)
                    {
                        new Row
                        {
                            Id = System.Guid.NewGuid().ToString("N"),
                            Text = "NEW " + System.DateTime.Now.ToLongTimeString(),
                        },
                    };
                    if (items != null)
                        copy.AddRange(items);
                    setItems(copy);
                },
            };
            var btnSetLast = new ButtonProps
            {
                Text = "Set Value",
                OnClick = () =>
                {
                    if (items == null || items.Count == 0) return;
                    var copy = new List<Row>(items);
                    var top = copy[0];
                    top.Text = $"{top.Id} {System.DateTime.Now:HH:mm:ss}";
                    top.ShouldOverrideElement = true;
                    copy[0] = top;
                    setItems(copy);
                },
            };
            var btnDeleteLast = new ButtonProps
            {
                Text = "Delete Last",
                OnClick = () =>
                {
                    if (items == null || items.Count == 0)
                        return;
                    var copy = new List<Row>(items);
                    copy.RemoveAt(copy.Count - 1);
                    setItems(copy);
                },
            };

            var columns = Hooks.UseMemo(
                () =>
                    new List<MultiColumnListViewProps.ColumnDef>
                    {
                        new()
                        {
                            Title = "ID",
                            Width = 140f,
                            MinWidth = 100f,
                            Resizable = true,
                            Stretchable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as Row;
                                var id = it?.Id ?? string.Empty;
                                var shortId = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(new LabelProps { Text = shortId });
                            },
                        },
                        new()
                        {
                            Title = "Text",
                            Width = 260f,
                            MinWidth = 140f,
                            Resizable = true,
                            Stretchable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as Row;
                                if (it == null)
                                    return V.Label(new LabelProps { Text = "<invalid>" });
                                var id = it.Id ?? i.ToString();
                                var funcKey = $"mclv-row-{id}";
                                var childrenNode = it.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = it.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                // Wrap to ensure stable VisualElement root
                                return V.VisualElement(null, null, childrenNode);
                            },
                        },
                    },
                items
            );

            var propsMap = new MultiColumnListViewProps
            {
                Items = items,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
            };

            var controls = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (ReactiveUITK.Props.Typed.StyleKeys.FlexDirection, "row"),
                            (ReactiveUITK.Props.Typed.StyleKeys.MarginBottom, 6f),
                        }
                    },
                },
                null,
                V.Button(btnAdd),
                V.Button(btnSetLast),
                V.Button(btnDeleteLast)
            );

            return V.VisualElement(null, null, controls, V.MultiColumnListView(propsMap));
        }
    }
}
