using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TreeViewProps : global::ReactiveUITK.Core.IProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public System.Func<int, object, ReactiveUITK.Core.VirtualNode> Row { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public Delegate ItemExpandedChanged { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }
        public string ViewDataKey { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            if (RootItems != null)
            {
                d["rootItems"] = RootItems;
            }
            if (FixedItemHeight.HasValue)
            {
                d["fixedItemHeight"] = FixedItemHeight.Value;
            }
            if (Selection.HasValue)
            {
                d["selectionType"] = Selection.Value;
            }
            if (SelectedIndex.HasValue)
            {
                d["selectedIndex"] = SelectedIndex.Value;
            }
            if (Row != null)
            {
                d["row"] = Row;
            }
            if (ExpandedItemIds != null)
            {
                d["expandedItemIds"] = ExpandedItemIds;
            }
            if (StopTrackingUserChange.HasValue)
            {
                d["stopTrackingUserChange"] = StopTrackingUserChange.Value;
            }
            if (ItemExpandedChanged != null)
            {
                d["itemExpandedChanged"] = ItemExpandedChanged;
            }
            if (Style != null)
            {
                d["style"] = Style;
            }
            if (Ref != null)
            {
                d["ref"] = Ref;
            }
            if (!string.IsNullOrEmpty(ViewDataKey))
            {
                d["viewDataKey"] = ViewDataKey;
            }
            return d;
        }
    }
}
