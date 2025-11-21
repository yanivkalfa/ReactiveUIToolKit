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
        public object Ref { get; set; }
        public string ViewDataKey { get; set; }

        public System.Func<int, object, ReactiveUITK.Core.VirtualNode> Row { get; set; }
        public SelectionType? Selection { get; set; }

        public Dictionary<string, object> ContentContainer { get; set; }
        public Dictionary<string, object> ScrollView { get; set; }

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
            if (Row != null)
            {
                dict["row"] = Row;
            }
            if (Selection.HasValue)
            {
                dict["selectionType"] = Selection.Value;
            }
            if (ContentContainer != null)
            {
                dict["contentContainer"] = ContentContainer;
            }
            if (ScrollView != null)
            {
                dict["scrollView"] = ScrollView;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            if (Ref != null)
            {
                dict["ref"] = Ref;
            }
            if (!string.IsNullOrEmpty(ViewDataKey))
            {
                dict["viewDataKey"] = ViewDataKey;
            }
            return dict;
        }
    }
}
