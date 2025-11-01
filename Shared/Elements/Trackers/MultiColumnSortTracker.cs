using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnSortTracker
        : IElementStateTracker<MultiColumnTreeView, MultiColumnTreeViewElementAdapter.Cached>
    {
        public void Attach(
            MultiColumnTreeView tv,
            MultiColumnTreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> props
        )
        {
            // Read overrides from props: sortedColumns
            if (props != null && props.TryGetValue("sortedColumns", out var sortedObj))
            {
                var fromProps = CoerceSorted(sortedObj);
                if (fromProps != null)
                {
                    state.SortedColumns = fromProps;
                }
            }

            // Wire user-provided notify (optional)
            if (props != null && props.TryGetValue("columnSortingChanged", out var user))
            {
                state.UserSortNotify = user as Delegate;
            }

            // Attach internal handler once: snapshot then forward to user
            if (state.InternalSortHandler == null)
            {
                state.InternalSortHandler = () =>
                {
                    try
                    {
                        state.SortedColumns = SnapshotSorted(tv);
                    }
                    catch { }
                    try
                    {
                        if (state.UserSortNotify != null)
                        {
                            var defs = ToSortedDefs(state.SortedColumns);
                            // Preferred signature: (VisualElement, List<SortedColumnDef>)
                            if (
                                state.UserSortNotify
                                is Action<
                                    UnityEngine.UIElements.VisualElement,
                                    List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef>
                                > a2
                            )
                                a2.Invoke(tv, defs);
                            else if (
                                state.UserSortNotify
                                is Action<
                                    List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef>
                                > a
                            )
                                a.Invoke(defs);
                            else if (state.UserSortNotify is Action<object> ao)
                                ao.Invoke(defs);
                            else if (state.UserSortNotify is Action g)
                                g.Invoke();
                        }
                    }
                    catch { }
                };
                try
                {
                    tv.columnSortingChanged += state.InternalSortHandler;
                }
                catch { }
            }
        }

        public void Detach(MultiColumnTreeView tv, MultiColumnTreeViewElementAdapter.Cached state)
        {
            // No-op; event detachment not strictly necessary across adapter lifetime
        }

        public void Reapply(
            MultiColumnTreeView tv,
            MultiColumnTreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            if (tv == null || state == null)
                return;

            // Latest from props wins
            if (nextProps != null && nextProps.TryGetValue("sortedColumns", out var sortedObj))
            {
                var fromProps = CoerceSorted(sortedObj);
                if (fromProps != null)
                {
                    state.SortedColumns = fromProps;
                }
            }

            // Apply desired sort via sortColumnDescriptions
            try
            {
                var desired = state.SortedColumns ?? new List<(string, SortDirection, int)>();
                var scd = tv.sortColumnDescriptions; // SortColumnDescriptions collection
                if (scd != null)
                {
                    scd.Clear();
                    foreach (var item in desired.OrderBy(x => x.index))
                    {
                        if (string.IsNullOrEmpty(item.name))
                            continue;
                        var desc = new SortColumnDescription(item.name, item.direction);
                        scd.Add(desc);
                    }
                }
            }
            catch { }

            // Refresh internal snapshot from control in case runtime adjusted order
            try
            {
                state.SortedColumns = SnapshotSorted(tv);
            }
            catch { }
        }

        private static List<(string name, SortDirection direction, int index)> CoerceSorted(
            object obj
        )
        {
            if (obj == null)
                return null;
            var list = new List<(string, SortDirection, int)>();
            try
            {
                if (obj is IEnumerable en)
                {
                    int i = 0;
                    foreach (var it in en)
                    {
                        if (it is IDictionary<string, object> map)
                        {
                            map.TryGetValue("name", out var n);
                            map.TryGetValue("direction", out var d);
                            map.TryGetValue("index", out var idx);
                            var name = n as string;
                            SortDirection dir = SortDirection.Ascending;
                            if (d is SortDirection sd)
                                dir = sd;
                            else if (d is string ds)
                            {
                                if (
                                    string.Equals(
                                        ds,
                                        "Descending",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                )
                                    dir = SortDirection.Descending;
                            }
                            int ord = idx is int ii ? ii : i;
                            if (!string.IsNullOrEmpty(name))
                                list.Add((name, dir, ord));
                            i++;
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        private static List<(string name, SortDirection direction, int index)> SnapshotSorted(
            MultiColumnTreeView tv
        )
        {
            var list = new List<(string, SortDirection, int)>();
            if (tv == null)
                return list;
            try
            {
                var sorted = tv.sortedColumns; // IReadOnlyList<SortColumnDescription>
                if (sorted != null)
                {
                    int i = 0;
                    foreach (var s in sorted)
                    {
                        try
                        {
                            var name = s.columnName;
                            var dir = s.direction;
                            if (!string.IsNullOrEmpty(name))
                                list.Add((name, dir, i));
                        }
                        catch { }
                        i++;
                    }
                }
            }
            catch { }
            return list;
        }

        private static List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef> ToSortedDefs(
            List<(string name, SortDirection direction, int index)> src
        )
        {
            var list =
                new List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef>();
            if (src == null)
                return list;
            foreach (var (name, direction, index) in src.OrderBy(x => x.index))
            {
                list.Add(
                    new ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef
                    {
                        Name = name,
                        Direction = direction,
                        Index = index,
                    }
                );
            }
            return list;
        }
    }
}
