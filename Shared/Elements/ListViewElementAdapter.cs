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
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;
            public IList LastItems; // track previous items reference
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

        private static IList NormalizeItems(object itemsObj)
        {
            if (itemsObj == null) return null;
            if (itemsObj is IList il) return il;
            if (itemsObj is IEnumerable en)
            {
                var list = new List<object>();
                foreach (var it in en) list.Add(it);
                return list;
            }
            return null;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (!(element is ListView listView) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());

            if (properties.TryGetValue("items", out var itemsObj))
            {
                var incoming = NormalizeItems(itemsObj);
                if (!ReferenceEquals(parts.LastItems, incoming))
                {
                    parts.LastItems = incoming;
                    listView.itemsSource = incoming as IList;
                    try { listView.Rebuild(); } catch { }
                }
            }
            else if (parts.LastItems != null)
            {
                // items prop removed: clear list
                parts.LastItems = null;
                listView.itemsSource = null;
                try { listView.Rebuild(); } catch { }
            }

            TryApplyProp<int>(properties, "selectedIndex", i => listView.selectedIndex = i);
            TryApplyProp<float>(properties, "fixedItemHeight", h => listView.fixedItemHeight = h);
            if (properties.TryGetValue("selectionType", out var selObj) && selObj is SelectionType sel)
            {
                listView.selectionType = sel;
            }

            // Row renderer wiring
            if (properties.TryGetValue("row", out var rowObj) && rowObj is Func<int, object, VirtualNode> rowFn)
            {
                parts.RowFn = rowFn;
                if (!parts.RowWired)
                {
                    parts.RowWired = true;
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

            // Allow overrides
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
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());

            previous.TryGetValue("items", out var prevItemsObj);
            next.TryGetValue("items", out var nextItemsObj);
            var prevItems = NormalizeItems(prevItemsObj);
            var nextItems = NormalizeItems(nextItemsObj);
            if (!ReferenceEquals(prevItems, nextItems))
            {
                parts.LastItems = nextItems;
                listView.itemsSource = nextItems;
                try { listView.Rebuild(); } catch { }
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

            if (next.TryGetValue("row", out var rowNext) && rowNext is Func<int, object, VirtualNode> newRowFn)
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
                    listView.unbindItem = (ve, i) => { (ve.userData as IVNodeHostRenderer)?.Unmount(); };
                }
                else if (changed && parts.LastItems != null)
                {
                    // Refresh all realized items so new delegate closure applies
                    int count = parts.LastItems.Count;
                    for (int i = 0; i < count; i++)
                    {
                        try { listView.RefreshItem(i); } catch { }
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

            ApplySlotsDiff(listView, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(ListView listView, IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null) return;
            if (properties.TryGetValue("contentContainer", out var cc) && cc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(listView.contentContainer, ccMap);
            }
            if (properties.TryGetValue("scrollView", out var sv) && sv is Dictionary<string, object> svMap)
            {
                var scroll = listView.Q<ScrollView>();
                if (scroll != null) PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void ApplySlotsDiff(ListView listView, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
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
                if (scroll != null) PropsApplier.Apply(scroll, svMap);
            }
        }
    }
}
