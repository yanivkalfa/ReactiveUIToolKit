using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using ReactiveUITK.Props;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Util;

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

            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
            if (properties.TryGetValue("items", out var itemsObj))
            {
                var incoming = BufferDiffUtil.NormalizeToList(itemsObj);
                if (BufferDiffUtil.EnsureBuffer(ref parts.Buffer, incoming))
                {
                    listView.itemsSource = parts.Buffer;
                    try { listView.Rebuild(); } catch { }
                }
                else if (incoming != null)
                {
                    bool any = BufferDiffUtil.OverlayDiff(parts.Buffer, incoming, null, i => { try { listView.RefreshItem(i); } catch { } });
                    if (!any && parts.Buffer.Count != (incoming?.Count ?? 0))
                    {
                        BufferDiffUtil.RebuildBuffer(parts.Buffer, incoming);
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

            // items diff via BufferDiffUtil
            var parts = cachedPartsByList.GetValue(listView, _ => new CachedParts());
            next.TryGetValue("items", out var newItemsObj);
            var incomingNext = BufferDiffUtil.NormalizeToList(newItemsObj);
            if (BufferDiffUtil.EnsureBuffer(ref parts.Buffer, incomingNext))
            {
                listView.itemsSource = parts.Buffer;
                try { listView.Rebuild(); } catch { }
            }
            else if (incomingNext != null)
            {
                bool any = BufferDiffUtil.OverlayDiff(parts.Buffer, incomingNext, null, i => { try { listView.RefreshItem(i); } catch { } });
                if (!any && parts.Buffer.Count != incomingNext.Count)
                {
                    BufferDiffUtil.RebuildBuffer(parts.Buffer, incomingNext);
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

            // Delegates (reference-based)
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
            // Minimal slot wiring: forward style or props for contentContainer / scrollView if provided
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
