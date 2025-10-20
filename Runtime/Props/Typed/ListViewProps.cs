using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ListViewProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public System.Func<VisualElement> MakeItem { get; set; }
        public System.Action<VisualElement, int> BindItem { get; set; }
        public System.Action<VisualElement, int> UnbindItem { get; set; }
        public Style Style { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
            if (Items != null)
            {
                dict["items"] = Items;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (FixedItemHeight.HasValue)
            {
                dict["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (MakeItem != null)
            {
                dict["makeItem"] = MakeItem;
            }
            if (BindItem != null)
            {
                dict["bindItem"] = BindItem;
            }
            if (UnbindItem != null)
            {
                dict["unbindItem"] = UnbindItem;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            return dict;
        }
    }
}
