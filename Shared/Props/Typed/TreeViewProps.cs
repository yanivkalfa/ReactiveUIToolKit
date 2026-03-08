using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

using ReactiveUITK.Core;
namespace ReactiveUITK.Props.Typed
{
    public sealed class TreeViewProps : BaseProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public RowRenderer Row { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public TreeExpansionEventHandler ItemExpandedChanged { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
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
            return d;
        }
    }
}
