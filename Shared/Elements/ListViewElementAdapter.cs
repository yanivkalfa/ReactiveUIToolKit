using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using ReactiveUITK.Props;
using ReactiveUITK.Core;

namespace ReactiveUITK.Elements
{
    public sealed class ListViewElementAdapter : BaseElementAdapter
    {
        private static HostContext sharedHostContext;
        private sealed class CachedParts
        {
            public VisualElement ContentContainer;
            public ScrollView ScrollView;
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;
            public List<object> Buffer;
        }
        private static readonly ConditionalWeakTable<ListView, CachedParts> cachedPartsByList = new();

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

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (!(element is ListView listView) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            // Initialize or update backing buffer
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
            if (properties.TryGetValue("items", out var itemsObj))
            {
                IList newList = itemsObj as IList;
                if (newList == null && itemsObj is IEnumerable enumerable)
                {
                    var tmp = new List<object>();
                    foreach (var it in enumerable) tmp.Add(it);
                    newList = tmp;
                }
                if (parts.Buffer == null)
                {
                    parts.Buffer = new List<object>();
                    if (newList != null)
                    {
                        int n = newList.Count;
                        parts.Buffer.Capacity = n;
                        for (int i = 0; i < n; i++) parts.Buffer.Add(newList[i]);
                    }
                    listView.itemsSource = parts.Buffer;
                }
                else if (newList != null)
                {
                    if (parts.Buffer.Count == newList.Count)
                    {
                        // Overlay differences in-place and refresh changed rows
                        for (int i = 0; i < parts.Buffer.Count; i++)
                        {
                            object cur = parts.Buffer[i];
                            object nxt = newList[i];
                            if (!ReferenceEquals(cur, nxt) && !Equals(cur, nxt))
                            {
                                parts.Buffer[i] = nxt;
                                try { listView.RefreshItem(i); } catch { }
                            }
                        }
                    }
                    else
                    {
                        // Rebuild buffer and assign
                        parts.Buffer.Clear();
                        int n = newList.Count;
                        parts.Buffer.Capacity = n;
                        for (int i = 0; i < n; i++) parts.Buffer.Add(newList[i]);
                        listView.itemsSource = parts.Buffer;
                        try { listView.Rebuild(); } catch { }
                    }
                }
                else
                {
                    parts.Buffer?.Clear();
                    listView.itemsSource = parts.Buffer;
                    try { listView.Rebuild(); } catch { }
                }
            }
            TryApplyProp<int>(properties, "selectedIndex", i => listView.selectedIndex = i);
            TryApplyProp<float>(properties, "fixedItemHeight", h => listView.fixedItemHeight = h);
            // Selection type: default None when row renderer is used (so embedded buttons work on first click)
            if (properties.TryGetValue("selectionType", out var selObj) && selObj is SelectionType sel)
            {
                listView.selectionType = sel;
            }

            // VNode row rendering support: props["row"] = Func<int, object, VirtualNode>
            if (properties.TryGetValue("row", out var rowObj) && rowObj is Func<int, object, VirtualNode> rowFn)
            {
                parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
                parts.RowFn = rowFn; // update latest function reference
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    // If selectionType not explicitly provided, disable selection for better embedded control UX
                    if (!properties.ContainsKey("selectionType"))
                    {
                        listView.selectionType = SelectionType.None;
                    }
                    listView.makeItem = () =>
                    {
                        var ve = new VisualElement();
                        ve.userData = new VNodeHostRenderer(GetRowHostContext(), ve);
                        return ve;
                    };
                    listView.bindItem = (ve, i) =>
                    {
                        var rr = ve.userData as IVNodeHostRenderer;
                        if (rr == null)
                        {
                            rr = new VNodeHostRenderer(GetRowHostContext(), ve);
                            ve.userData = rr;
                        }
                        object item = null;
                        if (listView.itemsSource is IList il && i >= 0 && i < il.Count)
                        {
                            item = il[i];
                        }
                        var f = parts.RowFn;
                        if (f != null)
                        {
                            rr.Render(f(i, item));
                        }
                    };
                    listView.unbindItem = (ve, i) =>
                    {
                        (ve.userData as IVNodeHostRenderer)?.Unmount();
                    };
                }
            }

            if (properties.TryGetValue("makeItem", out var mi) && mi is Func<VisualElement> make)
            {
                listView.makeItem = make;
            }
            if (properties.TryGetValue("bindItem", out var bi) && bi is Action<VisualElement, int> bind)
            {
                listView.bindItem = bind;
            }
            if (properties.TryGetValue("unbindItem", out var ubi) && ubi is Action<VisualElement, int> unbind)
            {
                listView.unbindItem = unbind;
            }

            ApplySlots(listView, properties);

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (!(element is ListView listView))
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();

            // items diff: always compare incoming list with backing buffer and update rows in-place
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
            next.TryGetValue("items", out var newItemsObj);
            IList newListForDiff = newItemsObj as IList;
            if (newListForDiff == null && newItemsObj is IEnumerable enumerableNext)
            {
                var tmp = new List<object>();
                foreach (var it in enumerableNext) tmp.Add(it);
                newListForDiff = tmp;
            }
            if (parts.Buffer == null)
            {
                // Initialize buffer first time if needed
                parts.Buffer = new List<object>();
                if (newListForDiff != null)
                {
                    for (int i = 0; i < newListForDiff.Count; i++) parts.Buffer.Add(newListForDiff[i]);
                }
                listView.itemsSource = parts.Buffer;
                try { listView.Rebuild(); } catch { }
            }
            else if (newListForDiff != null)
            {
                if (parts.Buffer.Count == newListForDiff.Count)
                {
                    for (int i = 0; i < parts.Buffer.Count; i++)
                    {
                        object cur = parts.Buffer[i];
                        object nxt = newListForDiff[i];
                        if (!ReferenceEquals(cur, nxt) && !Equals(cur, nxt))
                        {
                            parts.Buffer[i] = nxt;
                            try { listView.RefreshItem(i); } catch { }
                        }
                    }
                }
                else
                {
                    parts.Buffer.Clear();
                    for (int i = 0; i < newListForDiff.Count; i++) parts.Buffer.Add(newListForDiff[i]);
                    listView.itemsSource = parts.Buffer;
                    try { listView.Rebuild(); } catch { }
                }
            }

            TryDiffProp<int>(previous, next, "selectedIndex", i => listView.selectedIndex = i);
            TryDiffProp<float>(previous, next, "fixedItemHeight", h => listView.fixedItemHeight = h);
            if (next.TryGetValue("selectionType", out var selNext) && selNext is SelectionType selType)
            {
                if (listView.selectionType != selType)
                {
                    listView.selectionType = selType;
                }
            }

            // Delegates (compare reference)
            previous.TryGetValue("makeItem", out var oldMakeObj);
            next.TryGetValue("makeItem", out var newMakeObj);
            if (!ReferenceEquals(oldMakeObj, newMakeObj) && newMakeObj is Func<VisualElement> make)
            {
                listView.makeItem = make;
            }

            previous.TryGetValue("bindItem", out var oldBindObj);
            next.TryGetValue("bindItem", out var newBindObj);
            if (!ReferenceEquals(oldBindObj, newBindObj) && newBindObj is Action<VisualElement, int> bind)
            {
                listView.bindItem = bind;
            }

            previous.TryGetValue("unbindItem", out var oldUnbindObj);
            next.TryGetValue("unbindItem", out var newUnbindObj);
            if (!ReferenceEquals(oldUnbindObj, newUnbindObj) && newUnbindObj is Action<VisualElement, int> unbind)
            {
                listView.unbindItem = unbind;
            }

            // Row function diff (reference check)
            previous.TryGetValue("row", out var prevRowObj);
            next.TryGetValue("row", out var nextRowObj);
            if (nextRowObj is Func<int, object, VirtualNode> nextRowFn)
            {
                parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
                parts.RowFn = nextRowFn; // always update latest function reference
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
                    listView.makeItem = () =>
                    {
                        var ve = new VisualElement();
                        ve.userData = new VNodeHostRenderer(GetRowHostContext(), ve);
                        return ve;
                    };
                    listView.bindItem = (ve, i) =>
                    {
                        var rr = ve.userData as IVNodeHostRenderer;
                        if (rr == null)
                        {
                            rr = new VNodeHostRenderer(GetRowHostContext(), ve);
                            ve.userData = rr;
                        }
                        object item = null;
                        if (listView.itemsSource is IList il && i >= 0 && i < il.Count)
                        {
                            item = il[i];
                        }
                        var f = parts.RowFn;
                        if (f != null)
                        {
                            rr.Render(f(i, item));
                        }
                    };
                    listView.unbindItem = (ve, i) =>
                    {
                        (ve.userData as IVNodeHostRenderer)?.Unmount();
                    };
                }
            }

            DiffSlot(listView, previous, next, "contentContainer");
            DiffSlot(listView, previous, next, "scrollView");

            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(ListView listView, IReadOnlyDictionary<string, object> properties)
        {
            ApplySlot(listView, properties, "contentContainer");
            ApplySlot(listView, properties, "scrollView");
        }

        private static void DiffSlot(ListView listView, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string slotKey)
        {
            object previousSlotObject;
            object nextSlotObject;
            previous.TryGetValue(slotKey, out previousSlotObject);
            next.TryGetValue(slotKey, out nextSlotObject);
            if (!ReferenceEquals(previousSlotObject, nextSlotObject))
            {
                ApplySlot(listView, next, slotKey);
            }
        }

        private static void ApplySlot(ListView listView, IReadOnlyDictionary<string, object> properties, string slotKey)
        {
            if (properties == null)
            {
                return;
            }
            if (!properties.TryGetValue(slotKey, out object slotObject))
            {
                return;
            }
            if (slotObject is not Dictionary<string, object> slotMap)
            {
                return;
            }
            VisualElement target = ResolveSlotElement(listView, slotKey);
            if (target == null)
            {
                return;
            }
            if (slotMap.TryGetValue("style", out object styleObject) && styleObject is IDictionary<string, object> styleMap)
            {
                PropsApplier.Apply(target, new Dictionary<string, object> { { "style", styleMap } });
            }
            foreach (KeyValuePair<string, object> entry in slotMap)
            {
                if (entry.Key == "style")
                {
                    continue;
                }
                PropsApplier.Apply(target, new Dictionary<string, object> { { entry.Key, entry.Value } });
            }
        }

        private static VisualElement ResolveSlotElement(ListView listView, string slotKey)
        {
            if (!cachedPartsByList.TryGetValue(listView, out CachedParts parts))
            {
                parts = new CachedParts();
                cachedPartsByList.Add(listView, parts);
            }
            if (slotKey == "contentContainer")
            {
                if (parts.ContentContainer == null)
                {
                    parts.ContentContainer = listView.contentContainer;
                }
                return parts.ContentContainer;
            }
            if (slotKey == "scrollView")
            {
                if (parts.ScrollView == null)
                {
                    parts.ScrollView = listView.Q<ScrollView>();
                }
                return parts.ScrollView;
            }
            return null;
        }
    }
}
