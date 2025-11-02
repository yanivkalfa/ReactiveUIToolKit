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
                    List<(string name, SortDirection direction, int index)> snap = null;
                    try
                    {
                        snap = SnapshotSorted(tv);
                    }
                    catch { }
                    snap ??= new List<(string, SortDirection, int)>();
                    var prev = state.SortedColumns ?? new List<(string, SortDirection, int)>();
                    bool changed = !SortedEqual(prev, snap);
                    state.SortedColumns = snap;
                    if (!changed)
                        return; // avoid notifying user when nothing actually changed
                    try
                    {
                        if (state.UserSortNotify != null)
                        {
                            var defs = ToSortedDefs(state.SortedColumns);
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                try
                                {
                                    DispatchUserNotify(state.UserSortNotify, tv, defs);
                                }
                                catch { }
                            };
#else
                            try
                            {
                                tv?.schedule?.Execute(() =>
                                    {
                                        try
                                        {
                                            DispatchUserNotify(state.UserSortNotify, tv, defs);
                                        }
                                        catch { }
                                    })
                                    ?.ExecuteLater(0);
                            }
                            catch
                            {
                                try
                                {
                                    DispatchUserNotify(state.UserSortNotify, tv, defs);
                                }
                                catch { }
                            }
#endif
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

            // Also refresh the user-provided notification delegate if it changed
            if (nextProps != null && nextProps.TryGetValue("columnSortingChanged", out var user))
            {
                state.UserSortNotify = user as Delegate;
            }

            // Apply desired sort via sortColumnDescriptions
            try
            {
                var desired = state.SortedColumns ?? new List<(string, SortDirection, int)>();
                var current = SnapshotSorted(tv);
                bool differs = !SortedEqual(current, desired);
                var scd = tv.sortColumnDescriptions; // SortColumnDescriptions collection
                if (differs && scd != null)
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

        private static void DispatchUserNotify(
            Delegate notify,
            UnityEngine.UIElements.VisualElement tv,
            List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef> defs
        )
        {
            if (notify == null)
                return;
            if (
                notify
                is Action<
                    UnityEngine.UIElements.VisualElement,
                    List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef>
                > a2
            )
            {
                a2.Invoke(tv, defs);
                return;
            }
            if (
                notify
                is Action<List<ReactiveUITK.Props.Typed.MultiColumnTreeViewProps.SortedColumnDef>> a
            )
            {
                a.Invoke(defs);
                return;
            }
            if (notify is Action<object> ao)
            {
                ao.Invoke(defs);
                return;
            }
            if (notify is Action g)
            {
                g.Invoke();
            }
        }

        private static bool SortedEqual(
            List<(string name, SortDirection direction, int index)> a,
            List<(string name, SortDirection direction, int index)> b
        )
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i].name, b[i].name, StringComparison.Ordinal))
                    return false;
                if (a[i].direction != b[i].direction)
                    return false;
                if (a[i].index != b[i].index)
                    return false;
            }
            return true;
        }
    }
}
