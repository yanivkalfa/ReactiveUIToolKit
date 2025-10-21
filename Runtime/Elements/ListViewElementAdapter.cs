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

            if (properties.TryGetValue("items", out var itemsObj))
            {
                if (itemsObj is IList ilist)
                {
                    listView.itemsSource = ilist;
                }
                else if (itemsObj is IEnumerable<object> enumObj)
                {
                    listView.itemsSource = new List<object>(enumObj);
                }
            }
            TryApplyProp<int>(properties, "selectedIndex", i => listView.selectedIndex = i);
            TryApplyProp<float>(properties, "fixedItemHeight", h => listView.fixedItemHeight = h);

            // VNode row rendering support: props["row"] = Func<int, object, VirtualNode>
            if (properties.TryGetValue("row", out var rowObj) && rowObj is Func<int, object, VirtualNode> rowFn)
            {
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
                    rr.Render(rowFn(i, item));
                };
                listView.unbindItem = (ve, i) =>
                {
                    (ve.userData as IVNodeHostRenderer)?.Unmount();
                };
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

            // items
            previous.TryGetValue("items", out var oldItemsObj);
            next.TryGetValue("items", out var newItemsObj);
            if (!Equals(oldItemsObj, newItemsObj))
            {
                if (newItemsObj is IList ilist)
                {
                    listView.itemsSource = ilist;
                }
                else if (newItemsObj is IEnumerable<object> enumObj)
                {
                    listView.itemsSource = new List<object>(enumObj);
                }
                else
                {
                    listView.itemsSource = null;
                }
            }

            TryDiffProp<int>(previous, next, "selectedIndex", i => listView.selectedIndex = i);
            TryDiffProp<float>(previous, next, "fixedItemHeight", h => listView.fixedItemHeight = h);

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
            if (!ReferenceEquals(prevRowObj, nextRowObj) && nextRowObj is Func<int, object, VirtualNode> rowFn)
            {
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
                    rr.Render(rowFn(i, item));
                };
                listView.unbindItem = (ve, i) =>
                {
                    (ve.userData as IVNodeHostRenderer)?.Unmount();
                };
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
