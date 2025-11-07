using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnListViewProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IList Items { get; set; }
        public int? SelectedIndex { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public Style Style { get; set; }
    public object Ref { get; set; }

        // Column definitions
        public List<ColumnDef> Columns { get; set; }

        public List<SortedColumnDef> SortedColumns { get; set; }
        public object SortingMode { get; set; }
        public Delegate ColumnSortingChanged { get; set; }

        public sealed class ColumnDef
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float? Width { get; set; }
            public float? MinWidth { get; set; }
            public float? MaxWidth { get; set; }
            public bool? Resizable { get; set; }
            public bool? Stretchable { get; set; }
            public bool? Sortable { get; set; }
            public Func<int, object, ReactiveUITK.Core.VirtualNode> Cell { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var dict = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Name))
                    dict["name"] = Name;
                if (!string.IsNullOrEmpty(Title))
                    dict["title"] = Title;
                if (Width.HasValue)
                    dict["width"] = Width.Value;
                if (MinWidth.HasValue)
                    dict["minWidth"] = MinWidth.Value;
                if (MaxWidth.HasValue)
                    dict["maxWidth"] = MaxWidth.Value;
                if (Resizable.HasValue)
                    dict["resizable"] = Resizable.Value;
                if (Stretchable.HasValue)
                    dict["stretchable"] = Stretchable.Value;
                if (Sortable.HasValue)
                    dict["sortable"] = Sortable.Value;
                if (Cell != null)
                    dict["cell"] = Cell;
                return dict;
            }
        }

        public sealed class SortedColumnDef
        {
            public string Name { get; set; }
            public SortDirection? Direction { get; set; }
            public int? Index { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Name))
                    d["name"] = Name;
                if (Direction.HasValue)
                    d["direction"] = Direction.Value;
                if (Index.HasValue)
                    d["index"] = Index.Value;
                return d;
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                dict["className"] = ClassName;
            if (Items != null)
                dict["items"] = Items;
            if (SelectedIndex.HasValue)
                dict["selectedIndex"] = SelectedIndex.Value;
            if (FixedItemHeight.HasValue)
                dict["fixedItemHeight"] = FixedItemHeight.Value;
            if (Selection.HasValue)
                dict["selectionType"] = Selection.Value;
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                    cols.Add(c?.ToDictionary());
                dict["columns"] = cols;
            }
            if (SortedColumns != null)
            {
                var arr = new List<Dictionary<string, object>>(SortedColumns.Count);
                foreach (var s in SortedColumns)
                    arr.Add(s?.ToDictionary());
                dict["sortedColumns"] = arr;
            }
            if (SortingMode != null)
                dict["sortingMode"] = SortingMode;
            if (ColumnSortingChanged is Hooks.StateSetter<List<SortedColumnDef>> setter)
            {
                dict["columnSortingChanged"] = setter.ToValueAction();
            }
            else if (ColumnSortingChanged is Action<List<SortedColumnDef>> action)
            {
                dict["columnSortingChanged"] = action;
            }
            if (Style != null)
                dict["style"] = Style;
            if (Ref != null)
                dict["ref"] = Ref;
            return dict;
        }
    }
}
