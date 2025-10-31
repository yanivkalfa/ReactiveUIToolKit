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
    public sealed class TreeViewElementAdapter
        : StatefulElementAdapter<TreeView, TreeViewElementAdapter.Cached>
    {
        public sealed class Cached
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
            public IElementStateTracker<TreeView, Cached> ExpansionTracker =
                new TreeViewExpansionTracker();
        }

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
            var parts = GetState(tv);
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
                // Reapply expansions via tracker
                parts.ExpansionTracker.Reapply(tv, parts, null, properties);
            }

            TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));

            // Delegate wiring and overrides to the tracker
            parts.ExpansionTracker.Attach(tv, parts, properties);
            parts.ExpansionTracker.Reapply(tv, parts, null, properties);

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
            var parts = GetState(tv);

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
                parts.ExpansionTracker.Reapply(tv, parts, previous, next);
            }

            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

            // Delegate diff handling to tracker
            parts.ExpansionTracker.Attach(tv, parts, next);
            parts.ExpansionTracker.Reapply(tv, parts, previous, next);

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

        // Expansion handling delegated to TreeViewExpansionTracker

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
