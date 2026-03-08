using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class MultiColumnListViewStatefulDemoFunc
    {
        public sealed class Props : IProps
        {
            public IReadOnlyList<MultiColumnListViewRowState> Items { get; set; }
            public List<SortedColumnDef> SortDefs { get; set; }
            public Dictionary<string, float> ColumnWidths { get; set; }
            public Dictionary<string, bool> ColumnVisibility { get; set; }
            public Dictionary<string, int> ColumnDisplayIndex { get; set; }
            public Action AddItem { get; set; }
            public Action SetTopItem { get; set; }
            public Action DeleteLast { get; set; }
            public ColumnSortEventHandler OnSortChanged { get; set; }
            public ColumnLayoutEventHandler OnLayoutChanged { get; set; }
            public Action<int> OnCountChanged { get; set; }
        }

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            var items = p?.Items ?? Array.Empty<MultiColumnListViewRowState>();
            var sortDefs = p?.SortDefs;
            var columnWidths = p?.ColumnWidths;
            var columnVisibility = p?.ColumnVisibility;
            var columnDisplayIndex = p?.ColumnDisplayIndex;
            var addItem = p?.AddItem;
            var setTopItem = p?.SetTopItem;
            var deleteLast = p?.DeleteLast;
            var sortChanged = p?.OnSortChanged;
            ColumnLayoutEventHandler columnLayoutChanged = p?.OnLayoutChanged;

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
                                var it = obj as MultiColumnListViewRowState;
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
                                var it = obj as MultiColumnListViewRowState;
                                if (it == null)
                                {
                                    return V.Label(
                                        new LabelProps { Text = "<invalid>" },
                                        key: $"invalid-{i}"
                                    );
                                }
                                var id = it.Id ?? i.ToString();
                                var funcKey = $"mclv-row-{id}";
                                var childrenNode = it.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = it.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                return V.VisualElement(null, key: $"row-wrap-{id}", childrenNode);
                            },
                        },
                    },
                items
            );

            var displayed = Hooks.UseMemo(
                () =>
                {
                    var defsTree =
                        sortDefs == null
                            ? null
                            : new List<SortedColumnDef>(sortDefs.Count);
                    if (sortDefs != null)
                    {
                        foreach (var d in sortDefs)
                        {
                            defsTree.Add(
                                new SortedColumnDef
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
                        items ?? new List<MultiColumnListViewRowState>(),
                        (r, col) =>
                            string.Equals(col, "ID", StringComparison.OrdinalIgnoreCase) ? r?.Id
                            : string.Equals(col, "Text", StringComparison.OrdinalIgnoreCase)
                                ? r?.Text
                            : null
                    );
                },
                new object[] { items, sortDefs }
            );

            Hooks.UseEffect(
                () =>
                {
                    try
                    {
                        p?.OnCountChanged?.Invoke(items?.Count ?? 0);
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
                ColumnSortingChanged = sortChanged ?? (_ => { }),
                ColumnWidths = columnWidths,
                ColumnVisibility = columnVisibility,
                ColumnDisplayIndex = columnDisplayIndex,
                ColumnLayoutChanged = columnLayoutChanged,
                Style = new Style { (MarginBottom, 30f) },
            };

            PointerEventHandler Safe(Action candidate) => _ => candidate?.Invoke();

            var controls = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexDirection, "row"),
                        (MarginBottom, 6f),
                        (FlexShrink, 0f),
                    },
                },
                key: "controls",
                V.Button(
                    new ButtonProps { Text = "Add Parent", OnClick = Safe(addItem) },
                    key: "btn-add"
                ),
                V.Button(
                    new ButtonProps { Text = "Set Value", OnClick = Safe(setTopItem) },
                    key: "btn-set"
                ),
                V.Button(
                    new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) },
                    key: "btn-delete"
                )
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
