using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
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
        }

        public static VirtualNode Render(
            System.Collections.Generic.Dictionary<string, object> props,
            System.Collections.Generic.IReadOnlyList<VirtualNode> children
        )
        {
            var (rows, setRows) = Hooks.UseState(new List<RowData>());

            Func<List<TreeViewItemData<object>>> buildRoots = () =>
            {
                var list = new List<TreeViewItemData<object>>();
                for (int i = 0; i < rows.Count; i++)
                {
                    int pid = 2000 + (i * 2);
                    var row = rows[i];
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
                                        Id = System.Guid.NewGuid().ToString("N"),
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
                                    Id = System.Guid.NewGuid().ToString("N"),
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
                copy.Add(
                    new RowData
                    {
                        Parent = new SharedTreeRowItem
                        {
                            Id = System.Guid.NewGuid().ToString("N"),
                            Text = "Parent",
                        },
                        HasChild = false,
                    }
                );
                setRows(copy);
            }

            void AddChild()
            {
                if (rows.Count == 0) return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                last.HasChild = true;
                last.Child ??= new SharedTreeRowItem
                {
                    Id = System.Guid.NewGuid().ToString("N"),
                    Text = "Child",
                    IsChild = true,
                };
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void SetParentValue()
            {
                if (rows.Count == 0) return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                last.Parent ??= new SharedTreeRowItem { Id = System.Guid.NewGuid().ToString("N") };
                last.Parent.Text = $"{last.Parent.Id} {DateTime.Now:HH:mm:ss}";
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void SetChildValue()
            {
                if (rows.Count == 0) return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                if (!last.HasChild) return;
                last.Child ??= new SharedTreeRowItem { Id = System.Guid.NewGuid().ToString("N") };
                last.Child.Text = $"{last.Child.Id} {DateTime.Now:HH:mm:ss}";
                copy[copy.Count - 1] = last;
                setRows(copy);
            }

            void DeleteLast()
            {
                if (rows.Count == 0) return;
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
                            Title = "Name",
                            Width = 200f,
                            Cell = (i, obj) =>
                            {
                                var row = obj as SharedTreeRowItem;
                                if (row == null)
                                    return V.Label(new LabelProps { Text = "<invalid>" });
                                var id = !string.IsNullOrEmpty(row.Id) ? row.Id : i.ToString();
                                var funcKey = $"mctv-row-{id}";
                                var children = row.ShouldOverrideElement
                                    ? V.Label(new LabelProps { Text = row.Text ?? "<null>" }, funcKey)
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                return V.VisualElement(null, null, children);
                            },
                        },
                        new()
                        {
                            Title = "ID",
                            Width = 180f,
                            Cell = (i, obj) =>
                            {
                                var it = obj as SharedRowItem;
                                var id = it?.Id ?? string.Empty;
                                var s = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(new LabelProps { Text = s });
                            },
                        },
                    },
                rootsNow?.Count ?? 0
            );

            var propsMap = new MultiColumnTreeViewProps
            {
                RootItems = rootsNow,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
                Style = new Style { (MarginBottom, 30f) },
            };

            return V.VisualElement(null, null, btnRow, V.MultiColumnTreeView(propsMap));
        }
    }
}
