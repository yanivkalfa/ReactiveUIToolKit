using System;
using System.Collections.Generic;
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

            Func<List<TreeViewItemData<object>>> buildRoots = () =>
            {
                var list = new List<TreeViewItemData<object>>();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int pid = row.Pid; // stable, assigned at creation
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
            };

            void AddParent()
            {
                var copy = new List<RowData>(rows);
                copy.Add(new RowData
                {
                    Pid = nextPid,
                    Parent = new SharedTreeRowItem
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Text = "Parent",
                    },
                    HasChild = false,
                });
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

            var rootsNow = buildRoots();

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
                                        new LabelProps { Text = row.Text ?? "<null>" }
                                    )
                                    : V.Func(IntroCounterFunc.Render, null);
                                return V.VisualElement(null, funcKey, childrenNode);
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

            Action<VisualElement, List<MultiColumnTreeViewProps.SortedColumnDef>> onSort = (
                ve,
                defs
            ) =>
            {
                if (ve is not MultiColumnTreeView tv || defs == null || defs.Count == 0)
                    return;
                var first = defs[0];
                var ordered = new List<RowData>(rows);
                Comparison<RowData> cmp = null;
                if (string.Equals(first.Name, "Name", StringComparison.OrdinalIgnoreCase))
                    cmp = (a, b) =>
                        string.Compare(
                            a?.Parent?.Text ?? string.Empty,
                            b?.Parent?.Text ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        );
                else if (string.Equals(first.Name, "ID", StringComparison.OrdinalIgnoreCase))
                    cmp = (a, b) =>
                        string.Compare(
                            a?.Parent?.Id ?? string.Empty,
                            b?.Parent?.Id ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        );
                if (cmp == null)
                    return;
                ordered.Sort(cmp);
                if (first.Direction.HasValue && first.Direction.Value == SortDirection.Descending)
                    ordered.Reverse();
                // Setting state will rebuild roots via adapter; no explicit refresh needed here
                setRows(ordered);
            };

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

        private static int FindColumnIndex(MultiColumnTreeView tv, string name)
        {
            if (tv == null || string.IsNullOrEmpty(name))
                return -1;
            int idx = 0;
            foreach (var col in tv.columns)
            {
                if (
                    !string.IsNullOrEmpty(col?.name)
                    && string.Equals(col.name, name, StringComparison.Ordinal)
                )
                    return idx;
                idx++;
            }
            return -1;
        }

        private static string ExtractCellText(
            MultiColumnTreeView tv,
            VisualElement row,
            int colIndex
        )
        {
            if (tv == null || row == null)
                return string.Empty;
            try
            {
                if (row.childCount <= colIndex)
                    return string.Empty;
                var cell = row.ElementAt(colIndex);
                var lbl = cell?.Q<Label>();
                return lbl?.text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
