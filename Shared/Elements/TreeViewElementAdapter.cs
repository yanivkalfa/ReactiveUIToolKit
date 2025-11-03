using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // Minimal, opinion-free adapter for UI Toolkit TreeView
    public sealed class TreeViewElementAdapter
        : StatefulElementAdapter<TreeView, TreeViewElementAdapter.Cached>
    {
        public sealed class Cached : IExpansionState
        {
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;

            // Stable renderer pool by row key
            public Dictionary<string, (IVNodeHostRenderer renderer, VisualElement mount)> Pool =
                new();

            // Expansion tracking/cache
            internal ExpansionStateTracker<TreeView, Cached> ExpansionTracker =
                new ExpansionStateTracker<TreeView, Cached>();
            public HashSet<int> DesiredExpanded { get; set; } = new();
            public Dictionary<int, bool> ExpandAllById { get; set; } = new();
            public bool OurHandlerAttached { get; set; }
            public Delegate UserExpandedHandler { get; set; }
            public bool TrackUserExpansion { get; set; } = true;
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

            // Expansion wiring
            if (properties.TryGetValue("stopTrackingUserChange", out var stopObj))
                try
                {
                    var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                    parts.ExpansionTracker.Attach(tv, parts, properties, ops);
                }
                catch { }
            parts.TrackUserExpansion = !(stopObj is bool b && b);
            // User expansion handler wiring is handled by the cooperative tracker
            // Inline expansion handler removed; cooperative tracker handles subscriptions

            if (properties.TryGetValue("expandedItemIds", out var expObj))
            {
                var ids = BaseElementAdapter.CoerceIds(expObj);
                parts.DesiredExpanded.Clear();
                parts.ExpandAllById.Clear();
                if (ids != null)
                {
                    foreach (var id in ids)
                        parts.DesiredExpanded.Add(id);
                }
                try
                {
                    var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                    parts.ExpansionTracker.Reapply(tv, parts, null, properties, ops);
                }
                catch { }
            }

            if (properties.TryGetValue("rootItems", out var roots))
            {
                SetRootItems(tv, roots);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                try
                {
                    var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                    parts.ExpansionTracker.Reapply(tv, parts, null, properties, ops);
                }
                catch { }
            }

            TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));

            // Row rendering
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

            // Expansion wiring diff
            if (next.TryGetValue("stopTrackingUserChange", out var stopObj))
                parts.TrackUserExpansion = !(stopObj is bool b && b);
            // Cooperative tracker handles user expansion handler wiring
            try
            {
                var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                parts.ExpansionTracker.Attach(tv, parts, next, ops);
            }
            catch { }

            previous.TryGetValue("rootItems", out var pr);
            next.TryGetValue("rootItems", out var nr);
            if (!ReferenceEquals(pr, nr))
            {
                SetRootItems(tv, nr);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                try
                {
                    var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                    parts.ExpansionTracker.Reapply(tv, parts, previous, next, ops);
                }
                catch { }
                TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            }
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

            // expandedItemIds diff
            if (next.TryGetValue("expandedItemIds", out var nextExp))
            {
                var ids = BaseElementAdapter.CoerceIds(nextExp);
                parts.DesiredExpanded.Clear();
                parts.ExpandAllById.Clear();
                if (ids != null)
                {
                    foreach (var id in ids)
                        parts.DesiredExpanded.Add(id);
                }
                try
                {
                    var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                    parts.ExpansionTracker.Reapply(tv, parts, previous, next, ops);
                }
                catch { }
            }

            PropsApplier.ApplyDiff(element, previous, next);
            // Final cooperative reapply guard
            try
            {
                var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                parts.ExpansionTracker.Reapply(tv, parts, previous, next, ops);
            }
            catch { }
        }

        private static void SetRootItems(TreeView tv, object root)
        {
            if (tv == null)
                return;
            try
            {
                var mi = typeof(TreeView)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == "SetRootItems" && m.IsGenericMethodDefinition);
                if (mi != null)
                {
                    mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { root });
                    return;
                }
            }
            catch { }
            try
            {
                var any = typeof(TreeView)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == "SetRootItems" && m.GetParameters().Length == 1);
                any?.Invoke(tv, new object[] { root });
            }
            catch { }
        }

        private static object GetItemForIndex(TreeView tv, int index)
        {
            try
            {
                var methods = typeof(TreeView).GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                var gen = methods.FirstOrDefault(m =>
                    m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition
                );
                if (gen != null)
                {
                    try
                    {
                        return gen.MakeGenericMethod(typeof(object))
                            .Invoke(tv, new object[] { index });
                    }
                    catch { }
                }
                var nonGen = methods.FirstOrDefault(m =>
                    m.Name == "GetItemDataForIndex" && !m.IsGenericMethod
                );
                if (nonGen != null)
                {
                    try
                    {
                        return nonGen.Invoke(tv, new object[] { index });
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }

        private static string DeriveRowKey(TreeView tv, int index, object item)
        {
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

        // Inline expansion helpers removed in favor of cooperative tracker
    }
}
