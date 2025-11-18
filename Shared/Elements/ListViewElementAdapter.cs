using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ListViewElementAdapter : BaseElementAdapter
    {
        private static HostContext sharedHostContext;

        private sealed class CachedParts : IScrollState
        {
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;
            public IList LastItems; // track previous items reference
            public Dictionary<string, (IVNodeHostRenderer renderer, VisualElement mount)> Pool =
                new();

            public bool IsScrolling { get; set; }
            public bool ScrollWired { get; set; }
            public IReadOnlyDictionary<string, object> PendingPrev { get; set; }
            public IReadOnlyDictionary<string, object> PendingNext { get; set; }
            public float ScrollX { get; set; }
            public float ScrollY { get; set; }
            public int ScrollActivityId { get; set; }
            public IElementStateTracker<ListView, CachedParts> ScrollTracker =
                new MultiColumnScrollTracker<ListView, CachedParts>(
                    new MultiColumnScrollOps<ListView>(),
                    flush: null
                );
        }

        private static readonly ConditionalWeakTable<ListView, CachedParts> cachedPartsByList =
            new();

        private static HostContext GetRowHostContext()
        {
            if (sharedHostContext == null)
            {
                var registry = ElementRegistryProvider.GetDefaultRegistry();
                sharedHostContext = new HostContext(registry);
            }
            return sharedHostContext;
        }

        public override VisualElement Create()
        {
            return new ListView();
        }

        private static IList NormalizeItems(object itemsObj)
        {
            if (itemsObj == null)
                return null;
            if (itemsObj is IList il)
                return il;
            if (itemsObj is IEnumerable en)
            {
                var list = new List<object>();
                foreach (var it in en)
                    list.Add(it);
                return list;
            }
            return null;
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (!(element is ListView listView) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
            EnsureViewDataKey(listView, properties);
            parts.ScrollTracker.Attach(listView, parts, properties);

            if (properties.TryGetValue("items", out var itemsObj))
            {
                var incoming = NormalizeItems(itemsObj);
                if (!ReferenceEquals(parts.LastItems, incoming))
                {
                    parts.LastItems = incoming;
                    listView.itemsSource = incoming as IList;
                    try
                    {
                        listView.Rebuild();
                    }
                    catch { }
                }
            }
            else if (parts.LastItems != null)
            {
                // items prop removed: clear list
                parts.LastItems = null;
                listView.itemsSource = null;
                try
                {
                    listView.Rebuild();
                }
                catch { }
            }

            TryApplyProp<int>(properties, "selectedIndex", i => listView.selectedIndex = i);
            TryApplyProp<float>(properties, "fixedItemHeight", h => listView.fixedItemHeight = h);
            if (
                properties.TryGetValue("selectionType", out var selObj)
                && selObj is SelectionType sel
            )
            {
                listView.selectionType = sel;
            }

            // Row renderer wiring
            if (
                properties.TryGetValue("row", out var rowObj)
                && rowObj is Func<int, object, VirtualNode> rowFn
            )
            {
                parts.RowFn = rowFn;
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    if (!properties.ContainsKey("selectionType"))
                    {
                        listView.selectionType = SelectionType.None;
                    }
                    listView.makeItem = () => new VisualElement();
                    listView.bindItem = (ve, i) =>
                    {
                        object item = null;
                        if (listView.itemsSource is IList il && i >= 0 && i < il.Count)
                        {
                            item = il[i];
                        }
                        var key = DeriveRowKey(listView, i, item) ?? ($"row-{i}");
                        var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
                        if (!parts.Pool.TryGetValue(key, out var entry))
                        {
                            var mount = new VisualElement();
                            try
                            {
                                mount.pickingMode = PickingMode.Ignore;
                            }
                            catch { }
                            var rrNew = new VNodeHostRenderer(GetRowHostContext(), mount);
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
                            var vnode = EnsureVisualElementRoot(f(i, item), "ListViewRow");
                            entry.renderer.Render(vnode);
                        }
                    };
                    listView.unbindItem = (ve, i) =>
                    {
                        var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
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

            // Allow overrides
            if (properties.TryGetValue("makeItem", out var mi) && mi is Func<VisualElement> make)
            {
                listView.makeItem = make;
            }
            if (
                properties.TryGetValue("bindItem", out var bi)
                && bi is Action<VisualElement, int> bind
            )
            {
                listView.bindItem = bind;
            }
            if (
                properties.TryGetValue("unbindItem", out var ubi)
                && ubi is Action<VisualElement, int> unbind
            )
            {
                listView.unbindItem = unbind;
            }

            ApplySlots(listView, properties);
            PropsApplier.Apply(element, properties);
            parts.ScrollTracker.Reapply(listView, parts, null, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (!(element is ListView listView))
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());

            previous.TryGetValue("items", out var prevItemsObj);
            next.TryGetValue("items", out var nextItemsObj);
            var prevItems = NormalizeItems(prevItemsObj);
            var nextItems = NormalizeItems(nextItemsObj);
            if (!ReferenceEquals(prevItems, nextItems))
            {
                parts.LastItems = nextItems;
                listView.itemsSource = nextItems;
                try
                {
                    listView.Rebuild();
                }
                catch { }
            }

            TryDiffProp<int>(previous, next, "selectedIndex", i => listView.selectedIndex = i);
            TryDiffProp<float>(
                previous,
                next,
                "fixedItemHeight",
                h => listView.fixedItemHeight = h
            );
            if (
                next.TryGetValue("selectionType", out var selNext)
                && selNext is SelectionType selType
            )
            {
                if (listView.selectionType != selType)
                {
                    listView.selectionType = selType;
                }
            }

            if (
                next.TryGetValue("row", out var rowNext)
                && rowNext is Func<int, object, VirtualNode> newRowFn
            )
            {
                bool changed = !ReferenceEquals(parts.RowFn, newRowFn);
                parts.RowFn = newRowFn;
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    if (!next.ContainsKey("selectionType"))
                    {
                        listView.selectionType = SelectionType.None;
                    }
                    listView.makeItem = () => new VisualElement();
                    listView.bindItem = (ve, i) =>
                    {
                        object item = null;
                        if (listView.itemsSource is IList il && i >= 0 && i < il.Count)
                        {
                            item = il[i];
                        }
                        var key = DeriveRowKey(listView, i, item) ?? ($"row-{i}");
                        var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
                        if (!parts.Pool.TryGetValue(key, out var entry))
                        {
                            var mount = new VisualElement();
                            try
                            {
                                mount.pickingMode = PickingMode.Ignore;
                            }
                            catch { }
                            var rrNew = new VNodeHostRenderer(GetRowHostContext(), mount);
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
                            var vnode = EnsureVisualElementRoot(f(i, item), "ListViewRow");
                            entry.renderer.Render(vnode);
                        }
                    };
                    listView.unbindItem = (ve, i) =>
                    {
                        var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
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
                else if (changed && parts.LastItems != null)
                {
                    // Refresh all realized items so new delegate closure applies
                    int count = parts.LastItems.Count;
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            listView.RefreshItem(i);
                        }
                        catch { }
                    }
                }
            }

            previous.TryGetValue("makeItem", out var oldMakeObj);
            next.TryGetValue("makeItem", out var newMakeObj);
            if (!ReferenceEquals(oldMakeObj, newMakeObj) && newMakeObj is Func<VisualElement> make)
            {
                listView.makeItem = make;
            }
            previous.TryGetValue("bindItem", out var oldBindObj);
            next.TryGetValue("bindItem", out var newBindObj);
            if (
                !ReferenceEquals(oldBindObj, newBindObj)
                && newBindObj is Action<VisualElement, int> bind
            )
            {
                listView.bindItem = bind;
            }
            previous.TryGetValue("unbindItem", out var oldUnbindObj);
            next.TryGetValue("unbindItem", out var newUnbindObj);
            if (
                !ReferenceEquals(oldUnbindObj, newUnbindObj)
                && newUnbindObj is Action<VisualElement, int> unbind
            )
            {
                listView.unbindItem = unbind;
            }

            ApplySlotsDiff(listView, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
            parts.ScrollTracker.Reapply(listView, parts, previous, next);
        }

        private static void ApplySlots(
            ListView listView,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(listView.contentContainer, ccMap);
            }
            if (
                properties.TryGetValue("scrollView", out var sv)
                && sv is Dictionary<string, object> svMap
            )
            {
                var scroll = listView.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void ApplySlotsDiff(
            ListView listView,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var prevCC);
            next.TryGetValue("contentContainer", out var nextCC);
            if (!ReferenceEquals(prevCC, nextCC) && nextCC is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(listView.contentContainer, ccMap);
            }
            previous.TryGetValue("scrollView", out var prevSV);
            next.TryGetValue("scrollView", out var nextSV);
            if (!ReferenceEquals(prevSV, nextSV) && nextSV is Dictionary<string, object> svMap)
            {
                var scroll = listView.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static string DeriveRowKey(ListView listView, int index, object item)
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
            return $"row-{index}";
        }

        private static void EnsureViewDataKey(
            ListView view,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (view == null)
                return;

            string desired = null;
            if (
                properties != null
                && properties.TryGetValue("viewDataKey", out var raw)
                && raw is string explicitKey
                && !string.IsNullOrEmpty(explicitKey)
            )
            {
                desired = explicitKey;
            }

            if (string.IsNullOrEmpty(desired))
            {
                if ((view.userData as NodeMetadata)?.Key is string metadataKey && !string.IsNullOrEmpty(metadataKey))
                {
                    desired = metadataKey;
                }
                else if (!string.IsNullOrEmpty(view.name))
                {
                    desired = view.name;
                }
            }

            if (string.IsNullOrEmpty(desired))
                return;

            if (string.Equals(view.viewDataKey, desired, StringComparison.Ordinal))
                return;

            view.viewDataKey = desired;
        }
    }
}
