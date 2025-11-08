using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine;
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
            // Tracker.Attach handles stopTrackingUserChange -> TrackUserExpansion
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
                ReapplyExpansion(tv, parts, null, properties);
            }

            if (properties.TryGetValue("rootItems", out var roots))
            {
                var scrollSnapshot = CaptureScrollSnapshot(tv);
                SetRootItems(tv, roots);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                ReapplyExpansion(tv, parts, null, properties, scrollSnapshot);
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
            // Tracker.Attach handles stopTrackingUserChange -> TrackUserExpansion
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
                var scrollSnapshot = CaptureScrollSnapshot(tv);
                SetRootItems(tv, nr);
                try
                {
                    tv.RefreshItems();
                }
                catch { }
                ReapplyExpansion(tv, parts, previous, next, scrollSnapshot);
            }
            // Apply fixedItemHeight diff regardless of root change
            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
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
                ReapplyExpansion(tv, parts, previous, next);
            }

            PropsApplier.ApplyDiff(element, previous, next);
            // Final cooperative reapply guard
            ReapplyExpansion(tv, parts, previous, next);
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

        private static void ReapplyExpansion(
            TreeView view,
            Cached parts,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            ScrollSnapshot? snapshotOverride = null
        )
        {
            if (view == null || parts == null)
                return;

            var snapshot = snapshotOverride.HasValue
                ? snapshotOverride.Value
                : CaptureScrollSnapshot(view);

            try
            {
                var ops = ReactiveUITK.Elements.TreeViewExpansionOps.Instance;
                parts.ExpansionTracker.Reapply(view, parts, previous, next, ops);
            }
            catch { }

            RestoreScrollSnapshot(view, snapshot);
        }

        private static ScrollSnapshot CaptureScrollSnapshot(TreeView view)
        {
            if (view == null)
                return default;

            try
            {
                var scroll = view.Q<ScrollView>();
                if (scroll == null)
                    return default;

                var offset = scroll.scrollOffset;
                var maxX = GetHighValue(scroll.horizontalScroller);
                var maxY = GetHighValue(scroll.verticalScroller);
                return new ScrollSnapshot(true, offset, maxX, maxY);
            }
            catch
            {
                return default;
            }
        }

        private static void RestoreScrollSnapshot(TreeView view, ScrollSnapshot snapshot)
        {
            if (view == null || !snapshot.IsValid)
                return;

            try
            {
                var scroll = view.Q<ScrollView>();
                if (scroll == null)
                    return;

                var target = new Vector2(
                    ResolveAxis(
                        snapshot.Offset.x,
                        snapshot.MaxX,
                        GetHighValue(scroll.horizontalScroller)
                    ),
                    ResolveAxis(
                        snapshot.Offset.y,
                        snapshot.MaxY,
                        GetHighValue(scroll.verticalScroller)
                    )
                );

                ApplyScrollOffset(scroll, target);
            }
            catch { }
        }

        private static void ApplyScrollOffset(ScrollView scroll, Vector2 target)
        {
            if (scroll == null)
                return;

            void Apply()
            {
                try
                {
                    scroll.scrollOffset = target;
                }
                catch { }
            }

            Apply();
            try
            {
                scroll.schedule?.Execute(Apply)?.ExecuteLater(0);
            }
            catch { }
        }

        private static float ResolveAxis(float previousValue, float previousMax, float newMax)
        {
            newMax = Math.Max(newMax, 0f);
            if (previousMax <= 0f)
                return Clamp(previousValue, 0f, newMax);

            var tolerance = Math.Max(previousMax * 0.01f, 2f);
            var distanceFromEnd = previousMax - previousValue;
            if (distanceFromEnd <= tolerance)
                return newMax;

            return Clamp(previousValue, 0f, newMax);
        }

        private static float GetHighValue(Scroller scroller)
        {
            if (scroller == null)
                return 0f;
            try
            {
                return scroller.highValue;
            }
            catch
            {
                return 0f;
            }
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private readonly struct ScrollSnapshot
        {
            public ScrollSnapshot(bool isValid, Vector2 offset, float maxX, float maxY)
            {
                IsValid = isValid;
                Offset = offset;
                MaxX = maxX;
                MaxY = maxY;
            }

            public bool IsValid { get; }
            public Vector2 Offset { get; }
            public float MaxX { get; }
            public float MaxY { get; }
        }
    }
}
