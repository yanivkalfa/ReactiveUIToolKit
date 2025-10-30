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
            }

            TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));

            if (
                properties.TryGetValue("row", out var rowFn)
                && rowFn is Func<int, object, VirtualNode> rf
            )
            {
                parts.RowFn = rf;
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    tv.makeItem = () =>
                    {
                        var mount = new VisualElement();
                        mount.userData = new VNodeHostRenderer(Host, mount);
                        return mount;
                    };
                    tv.bindItem = (ve, index) =>
                    {
                        var rr = ve.userData as IVNodeHostRenderer;
                        if (rr == null)
                        {
                            rr = new VNodeHostRenderer(Host, ve);
                            ve.userData = rr;
                        }
                        object item = GetItemForIndex(tv, index);
                        var f = parts.RowFn;
                        if (f != null)
                        {
                            var vnode = EnsureVisualElementRoot(f(index, item), "TreeViewRow");
                            rr.Render(vnode);
                        }
                    };
                    tv.unbindItem = (ve, i) =>
                    {
                        (ve.userData as IVNodeHostRenderer)?.Unmount();
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
            }

            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

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
    }
}
