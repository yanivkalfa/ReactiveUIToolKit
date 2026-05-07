using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ListViewProps : BaseProps
    {
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public ItemFactory MakeItem { get; set; }
        public ItemBinder BindItem { get; set; }
        public ItemBinder UnbindItem { get; set; }

        public RowRenderer Row { get; set; }
        public SelectionType? Selection { get; set; }

        public Dictionary<string, object> ScrollView { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ListViewProps o)
                return false;
            if (!ReferenceEquals(Items, o.Items))
                return false;
            if (SelectedIndex != o.SelectedIndex)
                return false;
            if (FixedItemHeight != o.FixedItemHeight)
                return false;
            if (MakeItem != o.MakeItem)
                return false;
            if (BindItem != o.BindItem)
                return false;
            if (UnbindItem != o.UnbindItem)
                return false;
            if (Row != o.Row)
                return false;
            if (Selection != o.Selection)
                return false;
            if (!ReferenceEquals(ScrollView, o.ScrollView))
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Items = null;
            SelectedIndex = null;
            FixedItemHeight = null;
            MakeItem = null;
            BindItem = null;
            UnbindItem = null;
            Row = null;
            Selection = null;
            ScrollView = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ListViewProps>.Return(this);
        }
    }
}
