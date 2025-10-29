using System;
using System.Collections;
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
    public sealed class TreeViewElementAdapter : BaseElementAdapter
    {
        private sealed class Cached
        {
            public IList LastRoot;
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;
        }

        private static readonly ConditionalWeakTable<TreeView, Cached> cache = new();
        private static HostContext sharedHost;
        private static HostContext Host => sharedHost ??= new HostContext(ElementRegistryProvider.GetDefaultRegistry());

        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<TreeView>();
        }

        private static void SetRootItems(TreeView tv, object rootItems)
        {
            if (tv == null || rootItems == null) return;
            // Try generic SetRootItems<T>(IList<TreeViewItemData<T>>)
            var methods = typeof(TreeView).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            var mi = methods.FirstOrDefault(m => m.Name == "SetRootItems" && m.IsGenericMethodDefinition);
            if (mi != null)
            {
                try { mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { rootItems }); return; } catch { }
            }
            // Fallback to any SetRootItems with single param
            var any = methods.FirstOrDefault(m => m.Name == "SetRootItems" && m.GetParameters().Length == 1);
            try { any?.Invoke(tv, new object[] { rootItems }); } catch { }
        }

        private static object GetItemForIndex(TreeView tv, int index)
        {
            try
            {
                var mi = typeof(TreeView).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition);
                if (mi != null)
                {
                    var generic = mi.MakeGenericMethod(typeof(object));
                    return generic.Invoke(tv, new object[] { index });
                }
            }
            catch { }
            return null;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is not TreeView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = cache.GetValue(tv, _ => new Cached());
            if (properties != null)
            {
                if (properties.TryGetValue("rootItems", out var r))
                {
                    if (!ReferenceEquals(parts.LastRoot, r as IList))
                    {
                        parts.LastRoot = r as IList;
                        SetRootItems(tv, r);
                    }
                }
                TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
                if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                    tv.selectionType = st;
                TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));
                if (properties.TryGetValue("row", out var row) && row is Func<int, object, VirtualNode> rf)
                {
                    parts.RowFn = rf;
                    if (!parts.RowWired)
                    {
                        parts.RowWired = true;
                        tv.makeItem = () =>
                        {
                            var row = new VisualElement();
                            var content = new VisualElement();
                            // Make row content transparent to pointer picking so expander/caret stays responsive
                            try { content.pickingMode = PickingMode.Ignore; } catch { }
                            row.Add(content);
                            row.userData = new VNodeHostRenderer(Host, content);
                            return row;
                        };
                        tv.bindItem = (ve, index) =>
                        {
                            var rr = ve.userData as IVNodeHostRenderer;
                            if (rr == null)
                            {
                                // If userData not set (older items), bind to an inner content element to avoid intercepting pointer events
                                VisualElement content = ve.childCount > 0 ? ve.ElementAt(0) as VisualElement : null;
                                if (content == null) { content = new VisualElement(); ve.Add(content); }
                                try { content.pickingMode = PickingMode.Ignore; } catch { }
                                rr = new VNodeHostRenderer(Host, content);
                                ve.userData = rr;
                            }
                            var item = GetItemForIndex(tv, index);
                            var f = parts.RowFn;
                            if (f != null)
                            {
                                var vnode = f(index, item);
                                vnode = EnsureVisualElementRoot(vnode, "TreeViewRow");
                                rr.Render(vnode);
                            }
                        };
                        tv.unbindItem = (ve, i) => { (ve.userData as IVNodeHostRenderer)?.Unmount(); };
                    }
                }
            }
            // Slots
            if (properties != null && properties.TryGetValue("contentContainer", out var cc) && cc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(tv.contentContainer, ccMap);
            }
            if (properties != null && properties.TryGetValue("scrollView", out var sv) && sv is Dictionary<string, object> svMap)
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null) PropsApplier.Apply(scroll, svMap);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is not TreeView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = cache.GetValue(tv, _ => new Cached());

            previous.TryGetValue("rootItems", out var prevRoot);
            next.TryGetValue("rootItems", out var nextRoot);
            if (!ReferenceEquals(prevRoot, nextRoot))
            {
                parts.LastRoot = nextRoot as IList;
                SetRootItems(tv, nextRoot);
            }
            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

            previous.TryGetValue("row", out var prow);
            next.TryGetValue("row", out var nrow);
            if (!ReferenceEquals(prow, nrow) && nrow is Func<int, object, VirtualNode> rf)
            {
                parts.RowFn = rf;
                // refresh visible
                try { tv.RefreshItems(); } catch { }
            }

            // Slots diff
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
                PropsApplier.Apply(tv.contentContainer, ccMap);
            previous.TryGetValue("scrollView", out var psv);
            next.TryGetValue("scrollView", out var nsv);
            if (!ReferenceEquals(psv, nsv) && nsv is Dictionary<string, object> svMap)
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null) PropsApplier.Apply(scroll, svMap);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
