using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class MultiColumnTreeViewStatefulDemoFunc
    {
        public sealed class Props : IProps
        {
            public IReadOnlyList<MultiColumnTreeViewRowState> Rows { get; set; }
            public List<SortedColumnDef> SortDefs { get; set; }
            public Dictionary<string, float> ColumnWidths { get; set; }
            public Dictionary<string, bool> ColumnVisibility { get; set; }
            public Dictionary<string, int> ColumnDisplayIndex { get; set; }
            public Action AddParent { get; set; }
            public Action AddChild { get; set; }
            public Action SetParent { get; set; }
            public Action SetChild { get; set; }
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
            var rows = p?.Rows ?? Array.Empty<MultiColumnTreeViewRowState>();
            var sortDefs = p?.SortDefs;
            var columnWidths = p?.ColumnWidths;
            var columnVisibility = p?.ColumnVisibility;
            var columnDisplayIndex = p?.ColumnDisplayIndex;
            var addParent = p?.AddParent;
            var addChild = p?.AddChild;
            var setParent = p?.SetParent;
            var setChild = p?.SetChild;
            var deleteLast = p?.DeleteLast;
            var onSortChanged = p?.OnSortChanged;
            ColumnLayoutEventHandler columnLayoutChanged = p?.OnLayoutChanged;

            var rootsNow = Hooks.UseMemo(
                () => BuildRoots(rows, sortDefs),
                new object[] { rows, sortDefs }
            );

            var columns = Hooks.UseMemo(
                () =>
                {
                    var list = new List<MultiColumnTreeViewProps.ColumnDef>();
                    list.Add(
                        new MultiColumnTreeViewProps.ColumnDef
                        {
                            Name = "Name",
                            Title = "Name",
                            Width = 200f,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var row = obj as SharedTreeRowItem;
                                if (row == null)
                                {
                                    return V.Label(
                                        new LabelProps { Text = "<invalid>" },
                                        key: $"mctv-invalid-{i}"
                                    );
                                }
                                var id = !string.IsNullOrEmpty(row.Id) ? row.Id : i.ToString();
                                var funcKey = $"mctv-row-{id}";
                                var childrenNode = row.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = row.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                return V.VisualElement(
                                    null,
                                    key: $"mctv-name-wrap-{id}",
                                    childrenNode
                                );
                            },
                        }
                    );
                    list.Add(
                        new MultiColumnTreeViewProps.ColumnDef
                        {
                            Name = "ID",
                            Title = "ID",
                            Width = 180f,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var row = obj as SharedTreeRowItem;
                                var id = row?.Id ?? i.ToString();
                                var shortId = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(
                                    new LabelProps { Text = shortId },
                                    key: $"mctv-id-cell-{id}"
                                );
                            },
                        }
                    );
                    return list;
                },
                rootsNow?.Count ?? 0
            );

            try
            {
                int countValue = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row == null)
                    {
                        continue;
                    }
                    countValue += 1;
                    if (row.HasChild)
                    {
                        countValue += 1;
                    }
                }
                Hooks.UseEffect(
                    () =>
                    {
                        try
                        {
                            p?.OnCountChanged?.Invoke(countValue);
                        }
                        catch { }
                        return null;
                    },
                    new object[] { countValue }
                );
            }
            catch { }

            PointerEventHandler Safe(Action candidate) => _ => candidate?.Invoke();

            var btnRow = V.VisualElement(
                new VisualElementProps { Style = new Style { (StyleKeys.FlexDirection, "row"), (MarginBottom, 6f) } },
                null,
                V.Button(new ButtonProps { Text = "Add Parent", OnClick = Safe(addParent) }),
                V.Button(new ButtonProps { Text = "Add Child", OnClick = Safe(addChild) }),
                V.Button(new ButtonProps { Text = "Set Parent", OnClick = Safe(setParent) }),
                V.Button(new ButtonProps { Text = "Set Child", OnClick = Safe(setChild) }),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) })
            );

            var propsMap = new MultiColumnTreeViewProps
            {
                RootItems = rootsNow,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
                SortingMode = ColumnSortingMode.Custom,
                SortedColumns = sortDefs,
                ColumnSortingChanged = onSortChanged ?? (_ => { }),
                ColumnWidths = columnWidths,
                ColumnVisibility = columnVisibility,
                ColumnDisplayIndex = columnDisplayIndex,
                ColumnLayoutChanged = columnLayoutChanged,
                Style = new Style { (MarginBottom, 30f) },
            };

            return V.VisualElement(null, null, btnRow, V.MultiColumnTreeView(propsMap));
        }

        private static List<TreeViewItemData<object>> BuildRoots(
            IReadOnlyList<MultiColumnTreeViewRowState> rows,
            IReadOnlyList<SortedColumnDef> sortDefs
        )
        {
            var rowBuffer = new List<MultiColumnTreeViewRowState>();
            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i] != null)
                    {
                        rowBuffer.Add(rows[i]);
                    }
                }
            }

            var sortedRows = ReactiveUITK.Shared.Util.SortUtils.MultiSort(
                sortDefs,
                rowBuffer,
                (r, col) =>
                    string.Equals(col, "Name", StringComparison.OrdinalIgnoreCase) ? r?.Parent?.Text
                    : string.Equals(col, "ID", StringComparison.OrdinalIgnoreCase) ? r?.Parent?.Id
                    : null
            );

            var list = new List<TreeViewItemData<object>>();
            if (sortedRows == null)
            {
                return list;
            }
            for (int i = 0; i < sortedRows.Count; i++)
            {
                var row = sortedRows[i];
                if (row == null)
                {
                    continue;
                }
                int pid = row.Pid;
                List<TreeViewItemData<object>> ch = null;
                if (row.HasChild)
                {
                    ch = new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(
                            pid + 1,
                            row.Child
                                ?? new SharedTreeRowItem
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    Text = "Child",
                                    IsChild = true,
                                },
                            null
                        ),
                    };
                }
                list.Add(
                    new TreeViewItemData<object>(
                        pid,
                        row.Parent
                            ?? new SharedTreeRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Parent",
                            },
                        ch
                    )
                );
            }
            return list;
        }
    }
}
