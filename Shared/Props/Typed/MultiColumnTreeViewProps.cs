using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnTreeViewProps : BaseProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public List<ColumnDef> Columns { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public Dictionary<string, float> ColumnWidths { get; set; }
        public Dictionary<string, bool> ColumnVisibility { get; set; }
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }
        public List<SortedColumnDef> SortedColumns { get; set; }
        public object SortingMode { get; set; }
        public ColumnSortEventHandler ColumnSortingChanged { get; set; }
        public ColumnLayoutEventHandler ColumnLayoutChanged { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not MultiColumnTreeViewProps o)
                return false;
            if (!ReferenceEquals(RootItems, o.RootItems))
                return false;
            if (FixedItemHeight != o.FixedItemHeight)
                return false;
            if (Selection != o.Selection)
                return false;
            if (SelectedIndex != o.SelectedIndex)
                return false;
            if (!ReferenceEquals(Columns, o.Columns))
                return false;
            if (!ReferenceEquals(ExpandedItemIds, o.ExpandedItemIds))
                return false;
            if (StopTrackingUserChange != o.StopTrackingUserChange)
                return false;
            if (!ReferenceEquals(ColumnWidths, o.ColumnWidths))
                return false;
            if (!ReferenceEquals(ColumnVisibility, o.ColumnVisibility))
                return false;
            if (!ReferenceEquals(ColumnDisplayIndex, o.ColumnDisplayIndex))
                return false;
            if (!ReferenceEquals(SortedColumns, o.SortedColumns))
                return false;
            if (!ReferenceEquals(SortingMode, o.SortingMode))
                return false;
            if (ColumnSortingChanged != o.ColumnSortingChanged)
                return false;
            if (ColumnLayoutChanged != o.ColumnLayoutChanged)
                return false;
            return true;
        }

        public sealed class ColumnDef : global::ReactiveUITK.Core.IProps
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float? Width { get; set; }
            public float? MinWidth { get; set; }
            public float? MaxWidth { get; set; }
            public bool? Resizable { get; set; }
            public bool? Stretchable { get; set; }
            public bool? Sortable { get; set; }
            public RowRenderer Cell { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Name))
                {
                    d["name"] = Name;
                }
                if (!string.IsNullOrEmpty(Title))
                {
                    d["title"] = Title;
                }
                if (Width.HasValue)
                {
                    d["width"] = Width.Value;
                }
                if (MinWidth.HasValue)
                {
                    d["minWidth"] = MinWidth.Value;
                }
                if (MaxWidth.HasValue)
                {
                    d["maxWidth"] = MaxWidth.Value;
                }
                if (Resizable.HasValue)
                {
                    d["resizable"] = Resizable.Value;
                }
                if (Stretchable.HasValue)
                {
                    d["stretchable"] = Stretchable.Value;
                }
                if (Sortable.HasValue)
                {
                    d["sortable"] = Sortable.Value;
                }
                if (Cell != null)
                {
                    d["cell"] = Cell;
                }
                return d;
            }
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
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                {
                    cols.Add(c?.ToDictionary());
                }
                d["columns"] = cols;
            }
            if (ExpandedItemIds != null)
            {
                d["expandedItemIds"] = ExpandedItemIds;
            }
            if (StopTrackingUserChange.HasValue)
            {
                d["stopTrackingUserChange"] = StopTrackingUserChange.Value;
            }
            if (ColumnWidths != null)
            {
                d["columnWidths"] = ColumnWidths;
            }
            if (ColumnVisibility != null)
            {
                d["columnVisibility"] = ColumnVisibility;
            }
            if (ColumnDisplayIndex != null)
            {
                d["columnDisplayIndex"] = ColumnDisplayIndex;
            }
            if (SortedColumns != null)
            {
                var arr = new List<Dictionary<string, object>>(SortedColumns.Count);
                foreach (var s in SortedColumns)
                {
                    arr.Add(s?.ToDictionary());
                }
                d["sortedColumns"] = arr;
            }
            if (SortingMode != null)
            {
                d["sortingMode"] = SortingMode;
            }
            if (ColumnSortingChanged != null)
            {
                d["columnSortingChanged"] = ColumnSortingChanged;
            }
            if (ColumnLayoutChanged != null)
            {
                d["columnLayoutChanged"] = ColumnLayoutChanged;
            }
            return d;
        }

        internal override void __ResetFields()
        {
            RootItems = null;
            FixedItemHeight = null;
            Selection = null;
            SelectedIndex = null;
            Columns = null;
            ExpandedItemIds = null;
            StopTrackingUserChange = null;
            ColumnWidths = null;
            ColumnVisibility = null;
            ColumnDisplayIndex = null;
            SortedColumns = null;
            SortingMode = null;
            ColumnSortingChanged = null;
            ColumnLayoutChanged = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<MultiColumnTreeViewProps>.Return(this);
        }
    }
}
