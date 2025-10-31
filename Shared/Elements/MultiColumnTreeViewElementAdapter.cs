using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class MultiColumnTreeViewElementAdapter
        : StatefulElementAdapter<MultiColumnTreeView, MultiColumnTreeViewElementAdapter.Cached>
    {
        public sealed class Cached
        {
            public IList LastRoot;
            public List<(string name, string title)> ColSig;
            public List<Func<int, object, VirtualNode>> CellFns;

            // Expansion tracking
            public HashSet<int> DesiredExpanded = new();
            public Dictionary<int, bool> ExpandAllById = new();
            public bool OurHandlerAttached;
            public Delegate UserExpandedHandler;
            public bool TrackUserExpansion = true;

            // Stable renderer pool for cells: key = rowKey|c=colIndex
            public Dictionary<string, (IVNodeHostRenderer renderer, VisualElement mount)> Pool =
                new();

            // Column layout persistence (by column name)
            public Dictionary<string, float> ColumnWidths = new();
            public IElementStateTracker<MultiColumnTreeView, Cached> LayoutTracker =
                new MultiColumnLayoutTracker();
        }

        private static HostContext host;
        private static HostContext Host =>
            host ??= new HostContext(ElementRegistryProvider.GetDefaultRegistry());

        public override VisualElement Create() => new MultiColumnTreeView();

        private static void SetRootItems(MultiColumnTreeView tv, object root)
        {
            if (tv == null || root == null)
                return;
            var mi = typeof(MultiColumnTreeView)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "SetRootItems" && m.IsGenericMethodDefinition);
            if (mi != null)
            {
                try
                {
                    mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { root });
                    return;
                }
                catch { }
            }
            var any = typeof(MultiColumnTreeView)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "SetRootItems" && m.GetParameters().Length == 1);
            try
            {
                any?.Invoke(tv, new object[] { root });
            }
            catch { }
        }

        private static object GetItemForRow(MultiColumnTreeView tv, int index)
        {
            try
            {
                var mi = typeof(MultiColumnTreeView)
                    .GetMethods()
                    .FirstOrDefault(m =>
                        m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition
                    );
                if (mi != null)
                {
                    return mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { index });
                }
            }
            catch { }
            return null;
        }

        private static string DeriveRowKey(MultiColumnTreeView tv, int index, object item)
        {
            try
            {
                if (item != null)
                {
                    var t = item.GetType();
                    var f = t.GetField(
                        "Id",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.Public
                    );
                    if (f?.FieldType == typeof(string))
                    {
                        var s = f.GetValue(item) as string;
                        if (!string.IsNullOrEmpty(s))
                            return s;
                    }
                    var p = t.GetProperty(
                        "Id",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.Public
                    );
                    if (p?.PropertyType == typeof(string))
                    {
                        var s = p.GetValue(item) as string;
                        if (!string.IsNullOrEmpty(s))
                            return s;
                    }
                }
            }
            catch { }
            try
            {
                var mi = typeof(MultiColumnTreeView).GetMethod(
                    "GetIdForIndex",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int) },
                    null
                );
                var idObj = mi?.Invoke(tv, new object[] { index });
                if (idObj != null)
                    return idObj.ToString();
            }
            catch { }
            return $"row-{index}";
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not MultiColumnTreeView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = GetState(tv);
            if (properties != null)
            {
                if (properties.TryGetValue("rootItems", out var r))
                {
                    var nextList = r as IList;
                    if (!ReferenceEquals(parts.LastRoot, nextList))
                    {
                        parts.LastRoot = nextList;
                        SetRootItems(tv, nextList);
                        try
                        {
                            tv.Rebuild();
                        }
                        catch { }
                        ReapplyDesired(tv, parts);
                        parts.LayoutTracker.Reapply(tv, parts, null, properties);
                    }
                }
                TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
                if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                    tv.selectionType = st;
                TryApplyProp<int>(properties, "selectedIndex", i => tv.selectedIndex = i);

                // stopTrackingUserChange option
                if (properties.TryGetValue("stopTrackingUserChange", out var stopObj))
                {
                    parts.TrackUserExpansion = !(stopObj is bool b && b);
                }

                if (
                    properties.TryGetValue("columns", out var cols)
                    && cols is IEnumerable<Dictionary<string, object>> list
                )
                {
                    var sig = new List<(string, string)>();
                    var fns = new List<Func<int, object, VirtualNode>>();
                    foreach (var c in list)
                    {
                        c.TryGetValue("name", out var n);
                        c.TryGetValue("title", out var t);
                        c.TryGetValue("cell", out var cell);
                        sig.Add((n as string, t as string));
                        fns.Add(cell as Func<int, object, VirtualNode>);
                    }
                    bool same = parts.ColSig != null && parts.ColSig.Count == sig.Count;
                    if (same)
                    {
                        for (int i = 0; i < sig.Count; i++)
                            if (parts.ColSig[i] != sig[i])
                            {
                                same = false;
                                break;
                            }
                    }
                    parts.ColSig = sig;
                    parts.CellFns = fns;
                    if (!same)
                    {
                        RebuildColumns(tv, list, parts);
                    }
                }

                // User-provided expansion changed handler
                if (properties.TryGetValue("itemExpandedChanged", out var userHandler))
                {
                    if (!ReferenceEquals(parts.UserExpandedHandler, userHandler))
                    {
                        if (parts.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> prev)
                        {
                            try
                            {
                                tv.itemExpandedChanged -= prev;
                            }
                            catch { }
                        }
                        parts.UserExpandedHandler = userHandler as Delegate;
                        if (parts.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> nextH)
                        {
                            try
                            {
                                tv.itemExpandedChanged += nextH;
                            }
                            catch { }
                        }
                    }
                }

                EnsureOurExpansionHandler(tv, parts);

                // expandedItemIds override
                if (properties.TryGetValue("expandedItemIds", out var expObj))
                {
                    var ids = CoerceIds(expObj);
                    parts.DesiredExpanded.Clear();
                    parts.ExpandAllById.Clear();
                    if (ids != null)
                    {
                        foreach (var id in ids)
                            parts.DesiredExpanded.Add(id);
                    }
                    ReapplyDesired(tv, parts);
                }
            }
            ApplySlots(tv, properties);
            parts.LayoutTracker.Attach(tv, parts, properties);
            parts.LayoutTracker.Reapply(tv, parts, null, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not MultiColumnTreeView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = GetState(tv);

            previous.TryGetValue("rootItems", out var pr);
            next.TryGetValue("rootItems", out var nr);
            if (!ReferenceEquals(pr, nr))
            {
                parts.LastRoot = nr as IList;
                SetRootItems(tv, nr);
                try
                {
                    tv.Rebuild();
                }
                catch { }
                ReapplyDesired(tv, parts);
                parts.LayoutTracker.Reapply(tv, parts, previous, next);
                TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            }
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.selectedIndex = i);

            previous.TryGetValue("columns", out var pc);
            next.TryGetValue("columns", out var nc);
            if (!ReferenceEquals(pc, nc) && nc is IEnumerable<Dictionary<string, object>> list)
            {
                var sig = new List<(string, string)>();
                var fns = new List<Func<int, object, VirtualNode>>();
                foreach (var c in list)
                {
                    c.TryGetValue("name", out var n);
                    c.TryGetValue("title", out var t);
                    c.TryGetValue("cell", out var cell);
                    sig.Add((n as string, t as string));
                    fns.Add(cell as Func<int, object, VirtualNode>);
                }
                parts.ColSig = sig;
                parts.CellFns = fns;
                RebuildColumns(tv, list, parts);
            }
            ApplySlotsDiff(tv, previous, next);

            // stopTrackingUserChange diff
            if (next.TryGetValue("stopTrackingUserChange", out var stopObj))
                parts.TrackUserExpansion = !(stopObj is bool b && b);

            // user handler diff
            previous.TryGetValue("itemExpandedChanged", out var prevUser);
            next.TryGetValue("itemExpandedChanged", out var nextUser);
            if (!ReferenceEquals(prevUser, nextUser))
            {
                if (parts.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> prev)
                {
                    try
                    {
                        tv.itemExpandedChanged -= prev;
                    }
                    catch { }
                }
                parts.UserExpandedHandler = nextUser as Delegate;
                if (parts.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> nextH)
                {
                    try
                    {
                        tv.itemExpandedChanged += nextH;
                    }
                    catch { }
                }
            }

            EnsureOurExpansionHandler(tv, parts);

            // expandedItemIds diff
            if (next.TryGetValue("expandedItemIds", out var nextExp))
            {
                var ids = CoerceIds(nextExp);
                parts.DesiredExpanded.Clear();
                parts.ExpandAllById.Clear();
                if (ids != null)
                {
                    foreach (var id in ids)
                        parts.DesiredExpanded.Add(id);
                }
                ReapplyDesired(tv, parts);
            }

            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void RebuildColumns(
            MultiColumnTreeView tv,
            IEnumerable<Dictionary<string, object>> cols,
            Cached parts
        )
        {
            tv.columns.Clear();
            int idx = 0;
            foreach (var c in cols)
            {
                c.TryGetValue("title", out var t);
                c.TryGetValue("name", out var n);
                var col = new Column { title = t as string };
                if (n is string ns && !string.IsNullOrEmpty(ns))
                    col.name = ns;
                else if (string.IsNullOrEmpty(col.name))
                    col.name = $"col{idx}";
                if (c.TryGetValue("width", out var w))
                {
                    try
                    {
                        col.width = Convert.ToSingle(w);
                    }
                    catch { }
                }
                if (c.TryGetValue("minWidth", out var mw))
                {
                    try
                    {
                        col.minWidth = Convert.ToSingle(mw);
                    }
                    catch { }
                }
                if (c.TryGetValue("maxWidth", out var xw))
                {
                    try
                    {
                        col.maxWidth = Convert.ToSingle(xw);
                    }
                    catch { }
                }
                if (c.TryGetValue("resizable", out var rz) && rz is bool rb)
                    col.resizable = rb;
                if (c.TryGetValue("stretchable", out var st) && st is bool sb)
                    col.stretchable = sb;
                col.makeCell = () => new VisualElement();
                int captured = idx;
                col.bindCell = (ve, rowIndex) =>
                {
                    object item = GetItemForRow(tv, rowIndex);
                    var key = DeriveRowKey(tv, rowIndex, item) + "|c=" + captured;
                    if (!parts.Pool.TryGetValue(key, out var entry))
                    {
                        var mount = new VisualElement();
                        try
                        {
                            mount.pickingMode = PickingMode.Ignore;
                        }
                        catch { }
                        var rrNew = new VNodeHostRenderer(Host, mount);
                        entry = (rrNew, mount);
                        parts.Pool[key] = entry;
                    }
                    if (entry.mount.parent != ve)
                    {
                        try
                        {
                            entry.mount.RemoveFromHierarchy();
                        }
                        catch { }
                        ve.Add(entry.mount);
                    }
                    var fn =
                        parts.CellFns != null && captured < parts.CellFns.Count
                            ? parts.CellFns[captured]
                            : null;
                    if (fn != null)
                    {
                        var vnode = fn(rowIndex, item);
                        vnode = EnsureVisualElementRoot(vnode, "MultiColumnTreeViewCell");
                        entry.renderer.Render(vnode);
                    }
                };
                col.unbindCell = (ve, i) =>
                {
                    foreach (var kv in parts.Pool)
                    {
                        var mount = kv.Value.mount;
                        if (mount != null && mount.parent == ve)
                        {
                            try
                            {
                                mount.RemoveFromHierarchy();
                            }
                            catch { }
                        }
                    }
                };
                // Apply persisted width (by name) if present
                if (
                    !string.IsNullOrEmpty(col.name)
                    && parts.ColumnWidths != null
                    && parts.ColumnWidths.TryGetValue(col.name, out var savedW)
                )
                {
                    try
                    {
                        col.width = savedW;
                    }
                    catch { }
                }
                tv.columns.Add(col);
                idx++;
            }
            try
            {
                tv.Rebuild();
            }
            catch { }
        }

        private static void ApplySlots(
            MultiColumnTreeView tv,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
                PropsApplier.Apply(tv.contentContainer, ccMap);
            if (
                properties.TryGetValue("scrollView", out var sv)
                && sv is Dictionary<string, object> svMap
            )
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void ApplySlotsDiff(
            MultiColumnTreeView tv,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
                PropsApplier.Apply(tv.contentContainer, ccMap);
            previous.TryGetValue("scrollView", out var psv);
            next.TryGetValue("scrollView", out var nsv);
            if (!ReferenceEquals(psv, nsv) && nsv is Dictionary<string, object> svMap)
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void EnsureOurExpansionHandler(MultiColumnTreeView tv, Cached parts)
        {
            if (tv == null || parts == null)
                return;
            bool shouldAttach = parts.TrackUserExpansion && parts.UserExpandedHandler == null;
            if (shouldAttach && !parts.OurHandlerAttached)
            {
                Action<TreeViewExpansionChangedArgs> h = e =>
                {
                    try
                    {
                        if (e.isExpanded)
                            parts.DesiredExpanded.Add(e.id);
                        else
                            parts.DesiredExpanded.Remove(e.id);
                        parts.ExpandAllById[e.id] = e.isAppliedToAllChildren;
                    }
                    catch { }
                };
                try
                {
                    tv.itemExpandedChanged += h;
                    parts.OurHandlerAttached = true;
                }
                catch { }
            }
        }

        private static void ReapplyDesired(MultiColumnTreeView tv, Cached parts)
        {
            if (tv == null || parts == null)
                return;
            foreach (var id in parts.DesiredExpanded)
            {
                bool all = parts.ExpandAllById.TryGetValue(id, out var v) && v;
                try
                {
                    tv.ExpandItem(id, all, false);
                }
                catch { }
            }
            try
            {
                tv.RefreshItems();
            }
            catch { }
        }
    }
}
