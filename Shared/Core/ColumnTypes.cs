// ColumnTypes.cs
// ─────────────────────────────────────────────────────────────────────────────
// Shared data types for multi-column list/tree views.
// Moved to Core so ColumnSortEventHandler and ColumnLayoutEventHandler can be
// strongly typed without depending on ReactiveUITK.Props.Typed.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Describes the sort state of a single column in a
    /// <c>MultiColumnListView</c> or <c>MultiColumnTreeView</c>.
    /// </summary>
    public sealed class SortedColumnDef : IProps
    {
        /// <summary>The column's <c>name</c> attribute.</summary>
        public string Name { get; set; }

        /// <summary>The active sort direction, or <c>null</c> when unsorted.</summary>
        public SortDirection? Direction { get; set; }

        /// <summary>The sort priority index (0 = primary sort).</summary>
        public int? Index { get; set; }

        /// <inheritdoc/>
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

    /// <summary>
    /// Snapshot of column widths, visibility, and display order in a
    /// <c>MultiColumnListView</c> or <c>MultiColumnTreeView</c>.
    /// </summary>
    public sealed class ColumnLayoutState : IProps
    {
        /// <summary>Maps column name → pixel width.</summary>
        public Dictionary<string, float> ColumnWidths { get; set; }

        /// <summary>Maps column name → visibility flag.</summary>
        public Dictionary<string, bool> ColumnVisibility { get; set; }

        /// <summary>Maps column name → display order index.</summary>
        public Dictionary<string, int> ColumnDisplayIndex { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, object> ToDictionary() =>
            new Dictionary<string, object>();
    }
}
