using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // Minimal, opinion-free adapter: create TreeView, wire row lifecycle, apply props, refresh items.
    public sealed class TreeViewElementAdapter : BaseElementAdapter
    {
        private sealed class Cached
        {
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;

            // Stable renderer pool by row key
            public Dictionary<string, (IVNodeHostRenderer renderer, VisualElement mount)> Pool =
                new();

            // Expansion tracking/cache
            public HashSet<int> DesiredExpanded = new();
            public Dictionary<int, bool> ExpandAllById = new();
            public bool OurHandlerAttached;
            public Delegate UserExpandedHandler;
            public bool TrackUserExpansion = true; // default to tracking on
        }

        private static readonly ConditionalWeakTable<TreeView, Cached> Cache = new();
        private static HostContext sharedHost;
        private static HostContext Host =>
            sharedHost ??= new HostContext(ElementRegistryProvider.GetDefaultRegistry());

        public override VisualElement Create() => GlobalVisualElementPool.Get<TreeView>();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TreeView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = Cache.GetValue(tv, _ => new Cached());
            if (properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            if (properties.TryGetValue("rootItems", out var roots))
            {
                SetRootItems(tv, roots);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                // Reapply desired expansions after a root rebuild
                ReapplyDesired(tv, parts);
            }

            TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));

            // Option: stop tracking user-driven expansion changes
            if (properties.TryGetValue("stopTrackingUserChange", out var stopObj))
            {
                parts.TrackUserExpansion = !(stopObj is bool b && b);
            }

            // User-provided expansion changed handler (direct pass-through)
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

            // Our internal expansion tracker (only attaches if tracking enabled and no user handler is set)
            EnsureOurExpansionHandler(tv, parts);

            // ExpandedItemIds: override cache and apply immediately via ExpandItem
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

            if (
                properties.TryGetValue("row", out var rowFn)
                && rowFn is Func<int, object, VirtualNode> rf
            )
            {
                parts.RowFn = rf;
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    tv.makeItem = () => new VisualElement();
                    tv.bindItem = (ve, index) =>
                    {
                        object item = GetItemForIndex(tv, index);
                        var key = DeriveRowKey(tv, index, item) ?? $"row-{index}";
                        if (!parts.Pool.TryGetValue(key, out var entry))
                        {
                            var mount = new VisualElement();
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
                        var f = parts.RowFn;
                        if (f != null)
                        {
                            var vnode = EnsureVisualElementRoot(f(index, item), "TreeViewRow");
                            entry.renderer.Render(vnode);
                        }
                    };
                    tv.unbindItem = (ve, i) =>
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
                }
            }

            // Slots are user-driven
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

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TreeView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = Cache.GetValue(tv, _ => new Cached());

            previous.TryGetValue("rootItems", out var prevRoots);
            next.TryGetValue("rootItems", out var nextRoots);
            if (!ReferenceEquals(prevRoots, nextRoots) && nextRoots != null)
            {
                SetRootItems(tv, nextRoots);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                ReapplyDesired(tv, parts);
            }

            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

            // ExpandedItemIds diff: override cache and reapply
            previous.TryGetValue("expandedItemIds", out var prevExp);
            next.TryGetValue("expandedItemIds", out var nextExp);
            if (!ReferenceEquals(prevExp, nextExp) && nextExp != null)
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

            previous.TryGetValue("row", out var prevRow);
            next.TryGetValue("row", out var nextRow);
            if (!ReferenceEquals(prevRow, nextRow) && nextRow is Func<int, object, VirtualNode> rf)
            {
                parts.RowFn = rf;
                try
                {
                    tv.RefreshItems();
                }
                catch { }
            }

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
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void SetRootItems(TreeView tv, object rootItems)
        {
            if (tv == null || rootItems == null)
                return;
            try
            {
                var mi = typeof(TreeView)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "SetRootItems" && m.IsGenericMethodDefinition);
                if (mi != null)
                {
                    mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { rootItems });
                    return;
                }
            }
            catch { }
            try
            {
                var any = typeof(TreeView)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "SetRootItems" && m.GetParameters().Length == 1);
                any?.Invoke(tv, new object[] { rootItems });
            }
            catch { }
        }

        private static object GetItemForIndex(TreeView tv, int index)
        {
            try
            {
                var mi = typeof(TreeView)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m =>
                        m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition
                    );
                if (mi != null)
                {
                    var generic = mi.MakeGenericMethod(typeof(object));
                    return generic.Invoke(tv, new object[] { index });
                }
            }
            catch { }
            return null;
        }

        private static List<int> CoerceIds(object value)
        {
            if (value == null)
                return null;
            try
            {
                var list = new List<int>();
                if (value is IEnumerable<int> gen)
                {
                    foreach (var v in gen)
                        list.Add(v);
                    return list;
                }
                if (value is System.Collections.IEnumerable any)
                {
                    foreach (var o in any)
                    {
                        try
                        {
                            list.Add(Convert.ToInt32(o));
                        }
                        catch { }
                    }
                    return list;
                }
            }
            catch { }
            return null;
        }

        private static void EnsureOurExpansionHandler(TreeView tv, Cached parts)
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

        private static void ReapplyDesired(TreeView tv, Cached parts)
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

        private static string DeriveRowKey(TreeView tv, int index, object item)
        {
            // Prefer string Id on payload; fallback to controller id; last resort index
            try
            {
                if (item != null)
                {
                    var t = item.GetType();
                    var f = t.GetField("Id", BindingFlags.Instance | BindingFlags.Public);
                    if (f?.FieldType == typeof(string))
                    {
                        var s = f.GetValue(item) as string;
                        if (!string.IsNullOrEmpty(s))
                            return s;
                    }
                    var p = t.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
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
                var mi = typeof(TreeView).GetMethod(
                    "GetIdForIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
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

        private static List<int> TryGetExpanded(TreeView tv)
        {
            try
            {
                var mi = typeof(TreeView).GetMethod(
                    "GetExpandedIds",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null
                );
                var result = mi?.Invoke(tv, null) as System.Collections.IEnumerable;
                if (result == null)
                    return new List<int>();
                var list = new List<int>();
                foreach (var o in result)
                {
                    try
                    {
                        list.Add(Convert.ToInt32(o));
                    }
                    catch { }
                }
                return list;
            }
            catch { }
            return new List<int>();
        }

        private static void TryRestoreExpanded(TreeView tv, List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;
            try
            {
                var mi = typeof(TreeView).GetMethod(
                    "SetExpanded",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), typeof(bool) },
                    null
                );
                if (mi == null)
                    return;
                foreach (var id in ids)
                {
                    try
                    {
                        mi.Invoke(tv, new object[] { id, true });
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
