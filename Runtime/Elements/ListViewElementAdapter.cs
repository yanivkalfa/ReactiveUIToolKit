using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class ListViewElementAdapter : BaseElementAdapter
    {
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

            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
