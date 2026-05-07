using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

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

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not TreeViewProps o)
                return false;
            if (!ReferenceEquals(RootItems, o.RootItems))
                return false;
            if (FixedItemHeight != o.FixedItemHeight)
                return false;
            if (Selection != o.Selection)
                return false;
            if (SelectedIndex != o.SelectedIndex)
                return false;
            if (Row != o.Row)
                return false;
            if (!ReferenceEquals(ExpandedItemIds, o.ExpandedItemIds))
                return false;
            if (StopTrackingUserChange != o.StopTrackingUserChange)
                return false;
            if (ItemExpandedChanged != o.ItemExpandedChanged)
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            RootItems = null;
            FixedItemHeight = null;
            Selection = null;
            SelectedIndex = null;
            Row = null;
            ExpandedItemIds = null;
            StopTrackingUserChange = null;
            ItemExpandedChanged = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<TreeViewProps>.Return(this);
        }
    }
}
