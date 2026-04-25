using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnListViewProps : BaseProps
    {
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }

        public List<ColumnDef> Columns { get; set; }

        public List<SortedColumnDef> SortedColumns { get; set; }
        public object SortingMode { get; set; }
        public ColumnSortEventHandler ColumnSortingChanged { get; set; }
        public Dictionary<string, float> ColumnWidths { get; set; }
        public Dictionary<string, bool> ColumnVisibility { get; set; }
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }
        public ColumnLayoutEventHandler ColumnLayoutChanged { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not MultiColumnListViewProps o)
                return false;
            if (!ReferenceEquals(Items, o.Items))
                return false;
            if (SelectedIndex != o.SelectedIndex)
                return false;
            if (FixedItemHeight != o.FixedItemHeight)
                return false;
            if (Selection != o.Selection)
                return false;
            if (!ReferenceEquals(Columns, o.Columns))
                return false;
            if (!ReferenceEquals(SortedColumns, o.SortedColumns))
                return false;
            if (!ReferenceEquals(SortingMode, o.SortingMode))
                return false;
            if (ColumnSortingChanged != o.ColumnSortingChanged)
                return false;
            if (!ReferenceEquals(ColumnWidths, o.ColumnWidths))
                return false;
            if (!ReferenceEquals(ColumnVisibility, o.ColumnVisibility))
                return false;
            if (!ReferenceEquals(ColumnDisplayIndex, o.ColumnDisplayIndex))
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
                var dict = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Name))
                {
                    dict["name"] = Name;
                }
                if (!string.IsNullOrEmpty(Title))
                {
                    dict["title"] = Title;
                }
                if (Width.HasValue)
                {
                    dict["width"] = Width.Value;
                }
                if (MinWidth.HasValue)
                {
                    dict["minWidth"] = MinWidth.Value;
                }
                if (MaxWidth.HasValue)
                {
                    dict["maxWidth"] = MaxWidth.Value;
                }
                if (Resizable.HasValue)
                {
                    dict["resizable"] = Resizable.Value;
                }
                if (Stretchable.HasValue)
                {
                    dict["stretchable"] = Stretchable.Value;
                }
                if (Sortable.HasValue)
                {
                    dict["sortable"] = Sortable.Value;
                }
                if (Cell != null)
                {
                    dict["cell"] = Cell;
                }
                return dict;
            }
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
            if (Selection.HasValue)
            {
                dict["selectionType"] = Selection.Value;
            }
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                {
                    cols.Add(c?.ToDictionary());
                }
                dict["columns"] = cols;
            }
            if (SortedColumns != null)
            {
                var arr = new List<Dictionary<string, object>>(SortedColumns.Count);
                foreach (var s in SortedColumns)
                {
                    arr.Add(s?.ToDictionary());
                }
                dict["sortedColumns"] = arr;
            }
            if (SortingMode != null)
            {
                dict["sortingMode"] = SortingMode;
            }
            if (ColumnSortingChanged != null)
            {
                dict["columnSortingChanged"] = ColumnSortingChanged;
            }
            if (ColumnWidths != null)
            {
                dict["columnWidths"] = ColumnWidths;
            }
            if (ColumnVisibility != null)
            {
                dict["columnVisibility"] = ColumnVisibility;
            }
            if (ColumnDisplayIndex != null)
            {
                dict["columnDisplayIndex"] = ColumnDisplayIndex;
            }
            if (ColumnLayoutChanged != null)
            {
                dict["columnLayoutChanged"] = ColumnLayoutChanged;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Items = null;
            SelectedIndex = null;
            FixedItemHeight = null;
            Selection = null;
            Columns = null;
            SortedColumns = null;
            SortingMode = null;
            ColumnSortingChanged = null;
            ColumnWidths = null;
            ColumnVisibility = null;
            ColumnDisplayIndex = null;
            ColumnLayoutChanged = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<MultiColumnListViewProps>.Return(this);
        }
    }
}
