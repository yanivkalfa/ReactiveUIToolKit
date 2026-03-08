using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ShowcaseListTabsSection
    {
        public sealed class Props : IProps
        {
            /// <summary>Called when the ListView row count changes.</summary>
            public Action<int> OnListCountChanged { get; set; }
            /// <summary>Called when the MultiColumnListView row count changes.</summary>
            public Action<int> OnMclvCountChanged { get; set; }
            /// <summary>Called when the selected tab index changes (for ValuesBar).</summary>
            public Action<int> OnListTabIndexChanged { get; set; }
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as Props;

            // ── State ────────────────────────────────────────────────────────
            var (listTabIndex, setListTabIndex) = Hooks.UseState(0);
            var (showTabs, setShowTabs) = Hooks.UseState(true);
            var (listRows, setListRows) = Hooks.UseState(new List<ListViewRowState>());
            var (mclvRows, setMclvRows) = Hooks.UseState(new List<MultiColumnListViewRowState>());
            var (mclvSortDefs, setMclvSortDefs) = Hooks.UseState<List<SortedColumnDef>>(null);
            var (mclvLayout, setMclvLayout) = Hooks.UseState<ColumnLayoutState>(null);

            // Propagate tab index to parent for ValuesBar
            Hooks.UseEffect(
                () => { p?.OnListTabIndexChanged?.Invoke(listTabIndex); return null; },
                new object[] { listTabIndex }
            );

            // ── ListView actions ─────────────────────────────────────────────
            Action listAddItem = () =>
            {
                setListRows.Set(prev =>
                {
                    var next = new List<ListViewRowState>
                    {
                        new ListViewRowState { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                    };
                    if (prev != null)
                        for (int i = 0; i < prev.Count; i++) next.Add(prev[i]);
                    return next;
                });
            };

            Action listSetTopItem = () =>
            {
                setListRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[0];
                    if (source == null) return prev;
                    var id = !string.IsNullOrEmpty(source.Id) ? source.Id : Guid.NewGuid().ToString("N");
                    var next = new List<ListViewRowState>(prev);
                    next[0] = new ListViewRowState
                    {
                        Id = id,
                        Text = $"{id} {DateTime.Now:HH:mm:ss}",
                        ShouldOverrideElement = true,
                    };
                    return next;
                });
            };

            Action listDeleteLast = () =>
            {
                setListRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<ListViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            // ── MultiColumnListView actions ──────────────────────────────────
            Action mclvAddItem = () =>
            {
                setMclvRows.Set(prev =>
                {
                    var next = new List<MultiColumnListViewRowState>
                    {
                        new MultiColumnListViewRowState
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "NEW " + DateTime.Now.ToLongTimeString(),
                        },
                    };
                    if (prev != null)
                        for (int i = 0; i < prev.Count; i++) next.Add(prev[i]);
                    return next;
                });
            };

            Action mclvSetTopItem = () =>
            {
                setMclvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var source = prev[0];
                    if (source == null) return prev;
                    var id = !string.IsNullOrEmpty(source.Id) ? source.Id : Guid.NewGuid().ToString("N");
                    var next = new List<MultiColumnListViewRowState>(prev);
                    next[0] = new MultiColumnListViewRowState
                    {
                        Id = id,
                        Text = $"{id} {DateTime.Now:HH:mm:ss}",
                        ShouldOverrideElement = true,
                    };
                    return next;
                });
            };

            Action mclvDeleteLast = () =>
            {
                setMclvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0) return prev;
                    var next = new List<MultiColumnListViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            ColumnLayoutEventHandler mclvLayoutChanged = layout =>
            {
                var clone = SharedDemoPageUtils.CloneLayout(layout);
                if (SharedDemoPageUtils.LayoutEqual(clone, mclvLayout)) return;
                setMclvLayout.Set(_ => clone);
            };

            ColumnSortEventHandler mclvSortChanged = defs =>
            {
                setMclvSortDefs(
                    defs != null ? new List<SortedColumnDef>(defs) : null
                );
            };

            // ── TabView props ────────────────────────────────────────────────
            var listTabViewProps = new TabViewProps
            {
                SelectedTabIndex = listTabIndex,
                SelectedIndexChanged = index => setListTabIndex(index),
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () => V.Func(IntroCounterFunc.Render),
                    },
                    new()
                    {
                        Title = "List",
                        Content = () => V.Func<ListViewStatefulDemoFunc.Props>(
                            ListViewStatefulDemoFunc.Render,
                            new ListViewStatefulDemoFunc.Props
                            {
                                Items = listRows,
                                AddItem = listAddItem,
                                SetTopItem = listSetTopItem,
                                DeleteLast = listDeleteLast,
                                OnCountChanged = count => p?.OnListCountChanged?.Invoke(count),
                            }),
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () => V.Func<MultiColumnListViewStatefulDemoFunc.Props>(
                            MultiColumnListViewStatefulDemoFunc.Render,
                            new MultiColumnListViewStatefulDemoFunc.Props
                            {
                                Items = mclvRows,
                                SortDefs = mclvSortDefs,
                                ColumnWidths = mclvLayout?.ColumnWidths,
                                ColumnVisibility = mclvLayout?.ColumnVisibility,
                                ColumnDisplayIndex = mclvLayout?.ColumnDisplayIndex,
                                AddItem = mclvAddItem,
                                SetTopItem = mclvSetTopItem,
                                DeleteLast = mclvDeleteLast,
                                OnSortChanged = mclvSortChanged,
                                OnLayoutChanged = mclvLayoutChanged,
                                OnCountChanged = count => p?.OnMclvCountChanged?.Invoke(count),
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
                    Text = showTabs ? "Hide List Tabs" : "Show List Tabs",
                    OnClick = _ => setShowTabs(!showTabs),
                }),
                V.Label(new LabelProps
                {
                    Text = "TabView + ListViews",
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
                    V.TabView(listTabViewProps)
                ),
                showTabs ? V.Fragment() : V.Text("List tabs hidden")
            );
        }
    }
}
