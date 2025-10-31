using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MultiColumnTreeViewProps
    {
        public IList RootItems { get; set; }
        public float? FixedItemHeight { get; set; }
        public SelectionType? Selection { get; set; }
        public int? SelectedIndex { get; set; }
        public List<ColumnDef> Columns { get; set; }
        public IList<int> ExpandedItemIds { get; set; }
        public bool? StopTrackingUserChange { get; set; }
        public Dictionary<string, float> ColumnWidths { get; set; }
        public Style Style { get; set; }

        public sealed class ColumnDef
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public float? Width { get; set; }
            public float? MinWidth { get; set; }
            public float? MaxWidth { get; set; }
            public bool? Resizable { get; set; }
            public bool? Stretchable { get; set; }
            public Func<int, object, ReactiveUITK.Core.VirtualNode> Cell { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Name))
                    d["name"] = Name;
                if (!string.IsNullOrEmpty(Title))
                    d["title"] = Title;
                if (Width.HasValue)
                    d["width"] = Width.Value;
                if (MinWidth.HasValue)
                    d["minWidth"] = MinWidth.Value;
                if (MaxWidth.HasValue)
                    d["maxWidth"] = MaxWidth.Value;
                if (Resizable.HasValue)
                    d["resizable"] = Resizable.Value;
                if (Stretchable.HasValue)
                    d["stretchable"] = Stretchable.Value;
                if (Cell != null)
                    d["cell"] = Cell;
                return d;
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            if (RootItems != null)
                d["rootItems"] = RootItems;
            if (FixedItemHeight.HasValue)
                d["fixedItemHeight"] = FixedItemHeight.Value;
            if (Selection.HasValue)
                d["selectionType"] = Selection.Value;
            if (SelectedIndex.HasValue)
                d["selectedIndex"] = SelectedIndex.Value;
            if (Columns != null)
            {
                var cols = new List<Dictionary<string, object>>(Columns.Count);
                foreach (var c in Columns)
                    cols.Add(c?.ToDictionary());
                d["columns"] = cols;
            }
            if (ExpandedItemIds != null)
                d["expandedItemIds"] = ExpandedItemIds;
            if (StopTrackingUserChange.HasValue)
                d["stopTrackingUserChange"] = StopTrackingUserChange.Value;
            if (ColumnWidths != null)
                d["columnWidths"] = ColumnWidths;
            if (Style != null)
                d["style"] = Style;
            return d;
        }
    }
}
