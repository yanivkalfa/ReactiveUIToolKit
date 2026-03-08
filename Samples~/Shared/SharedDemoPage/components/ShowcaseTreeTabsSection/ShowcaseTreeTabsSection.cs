using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ShowcaseTreeTabsSection
    {
        public sealed class Props : IProps
        {
            /// <summary>Called when the TreeView row count changes.</summary>
            public Action<int> OnTreeCountChanged { get; set; }
            /// <summary>Called when the MultiColumnTreeView row count changes.</summary>
            public Action<int> OnMctvCountChanged { get; set; }
            /// <summary>Called when the selected tab index changes (for ValuesBar).</summary>
            public Action<int> OnTreeTabIndexChanged { get; set; }
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as Props;

            // ── State ────────────────────────────────────────────────────────
            var (treeTabIndex, setTreeTabIndex) = Hooks.UseState(0);
            var (showTabs, setShowTabs)         = Hooks.UseState(true);
            var (treeRows, setTreeRows)         = Hooks.UseState(new List<TreeViewRowState>());
            var (treeExpandedIds, setTreeExpandedIds) = Hooks.UseState(new List<int>());
            var (_, setTreeNextPid) = Hooks.UseState(1000);
            var (mctvRows, setMctvRows)         = Hooks.UseState(new List<MultiColumnTreeViewRowState>());
            var (mctvNextPid, setMctvNextPid)   = Hooks.UseState(2000);
            var (mctvSortDefs, setMctvSortDefs) = Hooks.UseState<List<SortedColumnDef>>(null);
            var (mctvLayout, setMctvLayout)     = Hooks.UseState<ColumnLayoutState>(null);

            // Propagate tab index to parent for ValuesBar
            Hooks.UseEffect(
                () => { p?.OnTreeTabIndexChanged?.Invoke(treeTabIndex); return null; },
                new object[] { treeTabIndex }
            );

            // ── TreeView actions ─────────────────────────────────────────────
            Action treeAddParent = () =>
            {
                int assignedPid = 0;
                setTreeNextPid.Set(prev => { assignedPid = prev; return prev + 2; });
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    var next = prev != null
                        ? new List<TreeViewRowState>(prev)
                        : new List<TreeViewRowState>();
                    next.Add(new TreeViewRowState
                    {
                        Pid = assignedPid,
                        Parent = new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                        HasChild = false,
                    });
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                    setTreeExpandedIds.Set(prev => SharedDemoPageUtils.PruneTreeExpandedIds(latestRows, prev));
            };

            Action treeAddChild = () =>
            {
                List<TreeViewRowState> latestRows = null;
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
                        Child = source.Child ?? new SharedTreeRowItem
                            { Id = Guid.NewGuid().ToString("N"), Text = "Child", IsChild = true },
                        HasChild = true,
                    };
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                    setTreeExpandedIds.Set(prev => SharedDemoPageUtils.PruneTreeExpandedIds(latestRows, prev));
            };

            Action treeSetParent = () =>
            {
                List<TreeViewRowState> latestRows = null;
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
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                    setTreeExpandedIds.Set(prev => SharedDemoPageUtils.PruneTreeExpandedIds(latestRows, prev));
            };

            Action treeSetChild = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild) return prev;
                    var childItem = source.Child ?? new SharedTreeRowItem
                        { Id = Guid.NewGuid().ToString("N"), IsChild = true };
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
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                    setTreeExpandedIds.Set(prev => SharedDemoPageUtils.PruneTreeExpandedIds(latestRows, prev));
            };

            Action treeDeleteLast = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<TreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                    setTreeExpandedIds.Set(prev => SharedDemoPageUtils.PruneTreeExpandedIds(latestRows, prev));
            };

            TreeExpansionEventHandler treeExpandedChanged = args =>
            {
                setTreeExpandedIds.Set(prev =>
                {
                    var nextSet = prev != null ? new HashSet<int>(prev) : new HashSet<int>();
                    if (args != null)
                    {
                        if (args.isExpanded) nextSet.Add(args.id);
                        else nextSet.Remove(args.id);
                    }
                    var valid = SharedDemoPageUtils.BuildTreeValidIds(treeRows);
                    if (valid.Count > 0)
                    {
                        var removals = new List<int>();
                        foreach (var id in nextSet)
                            if (!valid.Contains(id)) removals.Add(id);
                        for (int i = 0; i < removals.Count; i++) nextSet.Remove(removals[i]);
                    }
                    var nextList = new List<int>(nextSet);
                    nextList.Sort();
                    if (prev != null && prev.Count == nextList.Count)
                    {
                        var prevSet = new HashSet<int>(prev);
                        if (prevSet.SetEquals(nextSet)) return prev;
                    }
                    return nextList;
                });
            };

            // ── MultiColumnTreeView actions ──────────────────────────────────
            Action mctvAddParent = () =>
            {
                int pidBase = mctvNextPid;
                setMctvRows.Set(prev =>
                {
                    var next = prev != null
                        ? new List<MultiColumnTreeViewRowState>(prev)
                        : new List<MultiColumnTreeViewRowState>();
                    next.Add(new MultiColumnTreeViewRowState
                    {
                        Pid = pidBase,
                        Parent = new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                        HasChild = false,
                    });
                    return next;
                });
                setMctvNextPid(pidBase + 2);
            };

            Action mctvAddChild = () =>
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
                        Child = source.Child ?? new SharedTreeRowItem
                            { Id = Guid.NewGuid().ToString("N"), Text = "Child", IsChild = true },
                        HasChild = true,
                    };
                    return next;
                });
            };

            Action mctvSetParent = () =>
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
            };

            Action mctvSetChild = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild) return prev;
                    var childItem = source.Child ?? new SharedTreeRowItem
                        { Id = Guid.NewGuid().ToString("N"), IsChild = true };
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
            };

            Action mctvDeleteLast = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            ColumnLayoutEventHandler mctvLayoutChanged = layout =>
            {
                var clone = SharedDemoPageUtils.CloneLayout(layout);
                if (SharedDemoPageUtils.LayoutEqual(clone, mctvLayout)) return;
                setMctvLayout.Set(_ => clone);
            };

            ColumnSortEventHandler mctvSortChanged = defs =>
            {
                setMctvSortDefs(
                    defs != null ? new List<SortedColumnDef>(defs) : null
                );
            };

            // ── TabView props ────────────────────────────────────────────────
            var tabViewProps = new TabViewProps
            {
                SelectedTabIndex = treeTabIndex,
                SelectedIndexChanged = index => setTreeTabIndex(index),
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () => V.Func(IntroCounterFunc.Render),
                    },
                    new()
                    {
                        Title = "Tree",
                        Content = () => V.Func<TreeViewStatefulDemoFunc.Props>(
                            TreeViewStatefulDemoFunc.Render,
                            new TreeViewStatefulDemoFunc.Props
                            {
                                Rows = treeRows,
                                AddParent = treeAddParent,
                                AddChild = treeAddChild,
                                SetParent = treeSetParent,
                                SetChild = treeSetChild,
                                DeleteLast = treeDeleteLast,
                                ExpandedItemIds = treeExpandedIds,
                                OnExpandedChanged = treeExpandedChanged,
                                OnCountChanged = count => p?.OnTreeCountChanged?.Invoke(count),
                            }),
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () => V.Func<MultiColumnTreeViewStatefulDemoFunc.Props>(
                            MultiColumnTreeViewStatefulDemoFunc.Render,
                            new MultiColumnTreeViewStatefulDemoFunc.Props
                            {
                                Rows = mctvRows,
                                SortDefs = mctvSortDefs,
                                ColumnWidths = mctvLayout?.ColumnWidths,
                                ColumnVisibility = mctvLayout?.ColumnVisibility,
                                ColumnDisplayIndex = mctvLayout?.ColumnDisplayIndex,
                                AddParent = mctvAddParent,
                                AddChild = mctvAddChild,
                                SetParent = mctvSetParent,
                                SetChild = mctvSetChild,
                                DeleteLast = mctvDeleteLast,
                                OnSortChanged = mctvSortChanged,
                                OnLayoutChanged = mctvLayoutChanged,
                                OnCountChanged = count => p?.OnMctvCountChanged?.Invoke(count),
                            }),
                    },
                },
                Style = new Style { (Height, 240f) },
            };

            // ── Render ───────────────────────────────────────────────────────
            return V.VisualElement(
                null,
                null,
                V.Button(new ButtonProps
                {
                    Text = showTabs ? "Hide Tree Tabs" : "Show Tree Tabs",
                    OnClick = _ => setShowTabs(!showTabs),
                }),
                V.Label(new LabelProps
                {
                    Text = "TabView + TreeView",
                    Style = new Style
                    {
                        (FontSize, 16f),
                        (TextColor, new Color(0.1f, 0.1f, 0.1f, 1f)),
                    },
                }),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (MaxHeight, 500f),
                            (FlexGrow, 0f),
                            (StyleKeys.Display, showTabs ? "flex" : "none"),
                            (StyleKeys.FlexDirection, "column"),
                            (StyleKeys.Overflow, "visible"),
                        },
                    },
                    null,
                    V.TabView(tabViewProps)
                ),
                showTabs ? V.Fragment() : V.Text("Tree tabs hidden")
            );
        }
    }
}
