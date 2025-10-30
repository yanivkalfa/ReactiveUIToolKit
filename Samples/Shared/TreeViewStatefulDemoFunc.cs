using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class TreeViewStatefulDemoFunc
    {
        private sealed class RowData
        {
            public SharedTreeRowItem Parent;
            public SharedTreeRowItem Child;
            public bool HasChild;
        }

        // private static List<TreeViewItemData<object>> buildRoots(
        //     List<RowData> rows,
        //     missingType setRows
        // ) { }

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (rows, setRows) = Hooks.UseState(new List<RowData>());

            Func<List<TreeViewItemData<object>>> buildRoots = () =>
            {
                var combined = new List<TreeViewItemData<object>>();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int pid = 1000 + (i * 2);
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
                    combined.Add(
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
                return combined;
            };

            var rowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (i, obj) =>
                        {
                            var row = obj as SharedTreeRowItem;
                            if (row == null)
                                return V.Label(
                                    new LabelProps { Text = "<invalid row payload>" },
                                    $"row-{i}"
                                );

                            var id = !string.IsNullOrEmpty(row.Id) ? $"{row.Id}" : $"{i}";
                            var prefix = (row.IsChild == true) ? "child" : "parent";
                            var key = $"tv-{prefix}-{id}";

                            Debug.Log($"Row {i}: {JsonUtility.ToJson(row, true)}");

                            Debug.Log($"ShouldOverrideElement - {row.ShouldOverrideElement}");

                            var children = row.ShouldOverrideElement
                                ? V.Label(new LabelProps { Text = row.Text ?? "<null>" }, key)
                                : V.Func(IntroCounterFunc.Render, null, key);

                            return V.VisualElement(null, key, children);
                        }
                    ),
                rows
            );

            void AddParent()
            {
                var copy = new List<RowData>(rows);
                copy.Add(
                    new RowData
                    {
                        Parent = new SharedTreeRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "Parent",
                        },
                        HasChild = false,
                    }
                );
                setRows(copy);
            }

            void AddChild()
            {
                if (rows.Count == 0)
                    return;
                var copy = new List<RowData>(rows);
                var last = copy[copy.Count - 1];
                last.HasChild = true;
                if (last.Child == null)
                {
                    last.Child = new SharedTreeRowItem
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Text = "Child",
                        IsChild = true,
                    };
                }
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
                // Switch child to a label and update its text (matches previous behavior)
                last.Child ??= new SharedTreeRowItem { Id = System.Guid.NewGuid().ToString("N") };
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

            var tvProps = new TreeViewProps
            {
                RootItems = buildRoots(),
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Row = rowRenderer,
                Style = new Style { (MarginBottom, 30f) },
            };

            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "TreeView (Isolated Demo)",
                    ContentContainer = new Dictionary<string, object>
                    {
                        {
                            "style",
                            new Style { (PaddingLeft, 6f), (PaddingTop, 4f) }
                        },
                    },
                },
                null,
                btnRow,
                V.TreeView(tvProps)
            );
        }
    }
}
