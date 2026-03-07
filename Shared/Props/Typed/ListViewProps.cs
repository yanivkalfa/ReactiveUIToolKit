using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ListViewProps : BaseProps
    {
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public System.Func<VisualElement> MakeItem { get; set; }
        public System.Action<VisualElement, int> BindItem { get; set; }
        public System.Action<VisualElement, int> UnbindItem { get; set; }

        public System.Func<int, object, ReactiveUITK.Core.VirtualNode> Row { get; set; }
        public SelectionType? Selection { get; set; }

        public Dictionary<string, object> ScrollView { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
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
            if (ScrollView != null)
            {
                dict["scrollView"] = ScrollView;
            }
            return dict;
        }
    }
}
