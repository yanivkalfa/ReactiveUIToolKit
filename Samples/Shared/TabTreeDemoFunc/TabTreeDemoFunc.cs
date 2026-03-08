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
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (tabIndex, setTabIndex) = Hooks.UseState(0);

            // ── TreeView state ──────────────────────────────────────────────────
            var (treeRows,        setTreeRows)        = Hooks.UseState(new List<TreeViewRowState>());
            var (treeNextPid,     setTreeNextPid)     = Hooks.UseState(2);
            var (treeExpandedIds, setTreeExpandedIds) = Hooks.UseState<List<int>>(null);

            // ── MultiColumnTreeView state ───────────────────────────────────────
            var (mctvRows,    setMctvRows)    = Hooks.UseState(new List<MultiColumnTreeViewRowState>());
            var (mctvNextPid, setMctvNextPid) = Hooks.UseState(2);

            // ── TreeView actions ────────────────────────────────────────────────
            void TreeAddParent()
            {
                var pid = treeNextPid;
                setTreeNextPid(treeNextPid + 2);
                setTreeRows.Set(prev =>
                {
                    var next = prev != null ? new List<TreeViewRowState>(prev) : new List<TreeViewRowState>();
                    next.Add(new TreeViewRowState
                    {
                        Pid = pid,
                        Parent = new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                        HasChild = false,
                    });
                    return next;
                });
            }

            void TreeAddChild()
            {
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null) return prev;
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = source.Child ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Child", IsChild = true },
                        HasChild = true,
                    };
                    return next;
                });
            }

            void TreeSetParent()
            {
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null) return prev;
                    var parentItem = source.Parent ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                    parentItem.Text = $"{parentItem.Id} {DateTime.Now:HH:mm:ss}";
                    parentItem.ShouldOverrideElement = true;
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = parentItem,
                        Child = source.Child,
                        HasChild = source.HasChild,
                    };
                    return next;
                });
            }

            void TreeSetChild()
            {
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild) return prev;
                    var childItem = source.Child ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), IsChild = true };
                    childItem.Text = $"{childItem.Id} {DateTime.Now:HH:mm:ss}";
                    childItem.ShouldOverrideElement = true;
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = childItem,
                        HasChild = true,
                    };
                    return next;
                });
            }

            void TreeDeleteLast()
            {
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<TreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            }

            void TreeExpandedChanged(TreeViewExpansionChangedArgs args)
            {
                setTreeExpandedIds.Set(prev =>
                {
                    var nextSet = prev != null ? new HashSet<int>(prev) : new HashSet<int>();
                    if (args != null)
                    {
                        if (args.isExpanded) nextSet.Add(args.id);
                        else nextSet.Remove(args.id);
                    }
                    var nextList = new List<int>(nextSet);
                    nextList.Sort();
                    // Bailout: return same reference if content unchanged → no re-render → breaks expansion loop
                    if (prev != null && prev.Count == nextList.Count)
                    {
                        var prevSet = new HashSet<int>(prev);
                        if (prevSet.SetEquals(nextSet))
                            return prev;
                    }
                    return nextList;
                });
            }

            // ── MultiColumnTreeView actions ─────────────────────────────────────
            void MctvAddParent()
            {
                var pid = mctvNextPid;
                setMctvNextPid(mctvNextPid + 2);
                setMctvRows.Set(prev =>
                {
                    var next = prev != null ? new List<MultiColumnTreeViewRowState>(prev) : new List<MultiColumnTreeViewRowState>();
                    next.Add(new MultiColumnTreeViewRowState
                    {
                        Pid = pid,
                        Parent = new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                        HasChild = false,
                    });
                    return next;
                });
            }

            void MctvAddChild()
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null) return prev;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = source.Child ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Child", IsChild = true },
                        HasChild = true,
                    };
                    return next;
                });
            }

            void MctvSetParent()
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null) return prev;
                    var parentItem = source.Parent ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                    parentItem.Text = $"{parentItem.Id} {DateTime.Now:HH:mm:ss}";
                    parentItem.ShouldOverrideElement = true;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = parentItem,
                        Child = source.Child,
                        HasChild = source.HasChild,
                    };
                    return next;
                });
            }

            void MctvSetChild()
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild) return prev;
                    var childItem = source.Child ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), IsChild = true };
                    childItem.Text = $"{childItem.Id} {DateTime.Now:HH:mm:ss}";
                    childItem.ShouldOverrideElement = true;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = childItem,
                        HasChild = true,
                    };
                    return next;
                });
            }

            void MctvDeleteLast()
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            }

            // ── Tab definitions ─────────────────────────────────────────────────
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
                    new()
                    {
                        Title = "Tree",
                        Content = () => V.Func<TreeViewStatefulDemoFunc.Props>(
                            TreeViewStatefulDemoFunc.Render,
                            new TreeViewStatefulDemoFunc.Props
                            {
                                Rows = treeRows,
                                AddParent = TreeAddParent,
                                AddChild = TreeAddChild,
                                SetParent = TreeSetParent,
                                SetChild = TreeSetChild,
                                DeleteLast = TreeDeleteLast,
                                ExpandedItemIds = treeExpandedIds,
                                OnExpandedChanged = (Action<TreeViewExpansionChangedArgs>)TreeExpandedChanged,
                            }),
                    },
                    new()
                    {
                        Title = "Tree (Columns)",
                        Content = () => V.Func<MultiColumnTreeViewStatefulDemoFunc.Props>(
                            MultiColumnTreeViewStatefulDemoFunc.Render,
                            new MultiColumnTreeViewStatefulDemoFunc.Props
                            {
                                Rows = mctvRows,
                                AddParent = MctvAddParent,
                                AddChild = MctvAddChild,
                                SetParent = MctvSetParent,
                                SetChild = MctvSetChild,
                                DeleteLast = MctvDeleteLast,
                            }),
                    },
                },
                Style = new Style { (Props.Typed.StyleKeys.Height, 240f) },
            };

            var btnRow = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (Props.Typed.StyleKeys.FlexDirection, "row"),
                        (Props.Typed.StyleKeys.MarginTop, 6f),
                    },
                },
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "Intro",
                        OnClick = () => setTabIndex(0),
                        Style = new Style { (Props.Typed.StyleKeys.Width, 80f) },
                    },
                    key: "tabs-btn-intro"
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
                    },
                    key: "tabs-btn-tree"
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
                    },
                    key: "tabs-btn-columns"
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
