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
            var (sortDefs, setSortDefs) = Hooks.UseState<
                List<MultiColumnListViewProps.SortedColumnDef>
            >(null);

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
                    if (items == null || items.Count == 0)
                        return;
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
                            Name = "ID",
                            Title = "ID",
                            Width = 140f,
                            MinWidth = 100f,
                            Resizable = true,
                            Stretchable = true,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as Row;
                                var id = it?.Id ?? i.ToString();
                                var shortId = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(
                                    new LabelProps { Text = shortId },
                                    key: $"id-cell-{id}"
                                );
                            },
                        },
                        new()
                        {
                            Name = "Text",
                            Title = "Text",
                            Width = 260f,
                            MinWidth = 140f,
                            Resizable = true,
                            Stretchable = true,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as Row;
                                if (it == null)
                                    return V.Label(
                                        new LabelProps { Text = "<invalid>" },
                                        key: $"invalid-{i}"
                                    );
                                var id = it.Id ?? i.ToString();
                                var funcKey = $"mclv-row-{id}";
                                var childrenNode = it.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = it.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                // Wrap to ensure stable VisualElement root
                                return V.VisualElement(null, key: $"row-wrap-{id}", childrenNode);
                            },
                        },
                    },
                items
            );

            // Build displayed list respecting current multi-sort definitions (memo BEFORE effect)
            var displayed = Hooks.UseMemo(
                () =>
                {
                    var defsTree =
                        sortDefs == null
                            ? null
                            : new List<MultiColumnTreeViewProps.SortedColumnDef>(sortDefs.Count);
                    if (sortDefs != null)
                    {
                        foreach (var d in sortDefs)
                        {
                            defsTree.Add(
                                new MultiColumnTreeViewProps.SortedColumnDef
                                {
                                    Name = d?.Name,
                                    Direction = d?.Direction,
                                    Index = d?.Index,
                                }
                            );
                        }
                    }
                    return ReactiveUITK.Shared.Util.SortUtils.MultiSort(
                        defsTree,
                        items ?? new List<Row>(),
                        (r, col) =>
                            string.Equals(col, "ID", StringComparison.OrdinalIgnoreCase) ? r?.Id
                            : string.Equals(col, "Text", StringComparison.OrdinalIgnoreCase)
                                ? r?.Text
                            : null
                    );
                },
                new object[] { items, sortDefs }
            );

            // Notify parent of count when it changes (effect AFTER all memos)
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
                        {
                            cb(items?.Count ?? 0);
                        }
                    }
                    catch { }
                    return null;
                },
                new object[] { items?.Count ?? 0 }
            );

            var propsMap = new MultiColumnListViewProps
            {
                Items = displayed,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
                SortingMode = ColumnSortingMode.Custom,
                SortedColumns = sortDefs,
                ColumnSortingChanged = setSortDefs,
                Style = new Style { (ReactiveUITK.Props.Typed.StyleKeys.MarginBottom, 30f) },
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
                            (ReactiveUITK.Props.Typed.StyleKeys.FlexShrink, 0f),
                        }
                    },
                },
                key: "controls",
                V.Button(btnAdd, key: "btn-add"),
                V.Button(btnSetLast, key: "btn-set"),
                V.Button(btnDeleteLast, key: "btn-delete")
            );

            return V.VisualElement(
                null,
                key: "mclv-root",
                controls,
                V.MultiColumnListView(propsMap, key: "mclv-list")
            );
        }
    }
}
