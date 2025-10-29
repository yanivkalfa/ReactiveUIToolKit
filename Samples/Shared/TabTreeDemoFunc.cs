using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public static class TabTreeDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (tabIndex, setTabIndex) = Hooks.UseState(0);

            // Build sample tree data
            var treeData = Hooks.UseMemo(
                () =>
                {
                    var r1Children = new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(
                            11,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child 1",
                            },
                            null
                        ),
                        new TreeViewItemData<object>(
                            12,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child 2",
                            },
                            null
                        ),
                    };
                    var r2Children = new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(
                            21,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child A",
                            },
                            null
                        ),
                        new TreeViewItemData<object>(
                            22,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child B",
                            },
                            null
                        ),
                    };
                    var roots = new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(
                            1,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Root 1",
                            },
                            r1Children
                        ),
                        new TreeViewItemData<object>(
                            2,
                            new SharedRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Root 2",
                            },
                            r2Children
                        ),
                    };
                    return roots;
                },
                0
            );

            var treeRow = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (i, obj) =>
                        {
                            var it = obj as SharedRowItem;
                            return V.Label(new LabelProps { Text = it?.Text ?? "<null>" });
                        }
                    ),
                0
            );

            var treeProps = new TreeViewProps
            {
                RootItems = treeData,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Row = treeRow,
            };

            var mctvCols = Hooks.UseMemo(
                () =>
                    new List<MultiColumnTreeViewProps.ColumnDef>
                    {
                        new()
                        {
                            Title = "Name",
                            Width = 180f,
                            Cell = (i, obj) =>
                            {
                                var it = obj as SharedRowItem;
                                return V.Label(new LabelProps { Text = it?.Text ?? string.Empty });
                            },
                        },
                        new()
                        {
                            Title = "ID",
                            Width = 160f,
                            Cell = (i, obj) =>
                            {
                                var it = obj as SharedRowItem;
                                var id = it?.Id ?? string.Empty;
                                var s = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(new LabelProps { Text = s });
                            },
                        },
                    },
                treeData?.Count ?? 0
            );

            var mctvProps = new MultiColumnTreeViewProps
            {
                RootItems = treeData,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = mctvCols,
            };

            var tabViewProps = new TabViewProps
            {
                SelectedIndex = tabIndex,
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () =>
                            V.Label(new LabelProps { Text = "This is a TabView + TreeView demo." }),
                    },
                    new() { Title = "Tree", Content = () => V.TreeView(treeProps) },
                    new()
                    {
                        Title = "Tree (Columns)",
                        Content = () => V.MultiColumnTreeView(mctvProps),
                    },
                },
                // Give the TabView a visible content area
                Style = new Style { (Props.Typed.StyleKeys.Height, 240f) },
            };

            var btnRow = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (Props.Typed.StyleKeys.FlexDirection, "row"),
                            (Props.Typed.StyleKeys.MarginTop, 6f),
                        }
                    },
                },
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "Intro",
                        OnClick = () => setTabIndex(0),
                        Style = new Style { (Props.Typed.StyleKeys.Width, 80f) },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Tree",
                        OnClick = () => setTabIndex(1),
                        Style = new Style
                        {
                            (Props.Typed.StyleKeys.MarginLeft, 6f),
                            (Props.Typed.StyleKeys.Width, 80f),
                        },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Columns",
                        OnClick = () => setTabIndex(2),
                        Style = new Style
                        {
                            (Props.Typed.StyleKeys.MarginLeft, 6f),
                            (Props.Typed.StyleKeys.Width, 90f),
                        },
                    }
                )
            );

            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "Tabs + TreeView",
                    ContentContainer = new Dictionary<string, object>
                    {
                        {
                            "style",
                            new Style
                            {
                                (Props.Typed.StyleKeys.PaddingLeft, 6f),
                                (Props.Typed.StyleKeys.PaddingTop, 4f),
                            }
                        },
                    },
                },
                null,
                btnRow,
                V.TabView(tabViewProps)
            );
        }
    }
}
