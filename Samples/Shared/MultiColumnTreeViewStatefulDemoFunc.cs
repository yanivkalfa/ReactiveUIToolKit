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
        private sealed class RowData
        {
            public SharedTreeRowItem Parent;
            public SharedTreeRowItem Child;
            public bool HasChild;
            public int Pid; // stable per-row id
        }

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (rows, setRows) = Hooks.UseState(new List<RowData>());
            var (nextPid, setNextPid) = Hooks.UseState(2000);
            var (sortDefs, setSortDefs) = Hooks.UseState<
                List<MultiColumnTreeViewProps.SortedColumnDef>
            >(null);

            void AddParent()
            {
                var copy = new List<RowData>(rows);
                copy.Add(
                    new RowData
                    {
                        Pid = nextPid,
                        Parent = new SharedTreeRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "Parent",
                        },
                        HasChild = false,
                    }
                );
                setRows(copy);
                setNextPid(nextPid + 2);
            }

            void AddChild()
            {
                if (rows.Count == 0)
                    return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                last.HasChild = true;
                last.Child ??= new SharedTreeRowItem
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Text = "Child",
                    IsChild = true,
                };
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void SetParentValue()
            {
                if (rows.Count == 0)
                    return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                last.Parent ??= new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                var prevParent = last.Parent.Text;
                last.Parent.Text = $"{last.Parent.Id} {DateTime.Now:HH:mm:ss}";
                last.Parent.ShouldOverrideElement = true;
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void SetChildValue()
            {
                if (rows.Count == 0)
                    return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                if (!last.HasChild)
                    return;
                last.Child ??= new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                var prevChild = last.Child.Text;
                last.Child.Text = $"{last.Child.Id} {DateTime.Now:HH:mm:ss}";
                last.Child.ShouldOverrideElement = true;
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void DeleteLast()
            {
                if (rows.Count == 0)
                    return;
                var copy = new List<RowData>(rows);
                copy.RemoveAt(copy.Count - 1);
                setRows(copy);
            }

            var btnRow = V.VisualElement(
                new Style { (StyleKeys.FlexDirection, "row"), (MarginBottom, 6f) },
                null,
                V.Button(new ButtonProps { Text = "Add Parent", OnClick = AddParent }),
                V.Button(new ButtonProps { Text = "Add Child", OnClick = AddChild }),
                V.Button(new ButtonProps { Text = "Set Parent", OnClick = SetParentValue }),
                V.Button(new ButtonProps { Text = "Set Child", OnClick = SetChildValue }),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = DeleteLast })
            );

            var rootsNow = BuildRoots(rows, sortDefs);

            // Notify parent of current displayed row count (roots + children) if requested
            try
            {
                int countValue = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    countValue += 1;
                    if (rows[i]?.HasChild == true) countValue += 1;
                }
                if (props != null && props.TryGetValue("onCountChanged", out var oc) && oc is Action<int> cb)
                {
                    Hooks.UseEffect(
                        () =>
                        {
                            try { cb(countValue); } catch { }
                            return null;
                        },
                        new object[] { countValue }
                    );
                }
            }
            catch { }

            var columns = Hooks.UseMemo(
                () =>
                    new List<MultiColumnTreeViewProps.ColumnDef>
                    {
                        new()
                        {
                            Name = "Name",
                            Title = "Name",
                            Width = 200f,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var row = obj as SharedTreeRowItem;
                                if (row == null)
                                    return V.Label(new LabelProps { Text = "<invalid>" });
                                var id = !string.IsNullOrEmpty(row.Id) ? row.Id : i.ToString();
                                var funcKey = $"mctv-row-{id}";
                                var childrenNode = row.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = row.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                return childrenNode;
                            },
                        },
                        new()
                        {
                            Name = "ID",
                            Title = "ID",
                            Width = 180f,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var row = obj as SharedTreeRowItem;
                                var id = row?.Id ?? string.Empty;
                                var s = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(new LabelProps { Text = s });
                            },
                        },
                    },
                rootsNow?.Count ?? 0
            );

            Action<List<MultiColumnTreeViewProps.SortedColumnDef>> onSort =
                defs => setSortDefs(defs);

            var propsMap = new MultiColumnTreeViewProps
            {
                RootItems = rootsNow,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
                SortingMode = ColumnSortingMode.Custom,
                ColumnSortingChanged = onSort,
                Style = new Style { (MarginBottom, 30f) },
            };

            return V.VisualElement(null, null, btnRow, V.MultiColumnTreeView(propsMap));
        }

        private static List<TreeViewItemData<object>> BuildRoots(
            List<RowData> rows,
            List<MultiColumnTreeViewProps.SortedColumnDef> sortDefs
        )
        {
            var sortedRows = ReactiveUITK.Shared.Util.SortUtils.MultiSort(
                sortDefs,
                new List<RowData>(rows),
                (r, col) =>
                    string.Equals(col, "Name", StringComparison.OrdinalIgnoreCase) ? r?.Parent?.Text
                    : string.Equals(col, "ID", StringComparison.OrdinalIgnoreCase) ? r?.Parent?.Id
                    : null
            );

            var list = new List<TreeViewItemData<object>>();
            if (sortedRows == null)
                return list;
            for (int i = 0; i < sortedRows.Count; i++)
            {
                var row = sortedRows[i];
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
