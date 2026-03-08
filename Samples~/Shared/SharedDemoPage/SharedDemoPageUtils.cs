using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.Shared
{
    internal static class SharedDemoPageUtils
    {
        internal static Dictionary<string, T> CloneDict<T>(IReadOnlyDictionary<string, T> source)
        {
            if (source == null) return null;
            if (source.Count == 0) return new Dictionary<string, T>();
            return new Dictionary<string, T>(source);
        }

        internal static bool DictEqual<T>(
            IReadOnlyDictionary<string, T> left,
            IReadOnlyDictionary<string, T> right
        )
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            if (left.Count != right.Count) return false;
            foreach (var kv in left)
            {
                if (!right.TryGetValue(kv.Key, out var rv)) return false;
                if (!EqualityComparer<T>.Default.Equals(kv.Value, rv)) return false;
            }
            return true;
        }

        internal static ColumnLayoutState CloneLayout(ColumnLayoutState layout)
        {
            if (layout == null) return null;
            return new ColumnLayoutState
            {
                ColumnWidths = CloneDict(layout.ColumnWidths),
                ColumnVisibility = CloneDict(layout.ColumnVisibility),
                ColumnDisplayIndex = CloneDict(layout.ColumnDisplayIndex),
            };
        }

        internal static bool LayoutEqual(ColumnLayoutState a, ColumnLayoutState b)
        {
            return DictEqual(a?.ColumnWidths, b?.ColumnWidths)
                && DictEqual(a?.ColumnVisibility, b?.ColumnVisibility)
                && DictEqual(a?.ColumnDisplayIndex, b?.ColumnDisplayIndex);
        }

        internal static HashSet<int> BuildTreeValidIds(IReadOnlyList<TreeViewRowState> rows)
        {
            var set = new HashSet<int>();
            if (rows == null) return set;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                int baseId = row != null && row.Pid != 0 ? row.Pid : 1000 + (i * 2);
                if (baseId != 0) set.Add(baseId);
                if (row?.HasChild == true) set.Add(baseId + 1);
            }
            return set;
        }

        internal static List<int> PruneTreeExpandedIds(
            IReadOnlyList<TreeViewRowState> rows,
            IList<int> expanded
        )
        {
            if (expanded == null) return null;
            var valid = BuildTreeValidIds(rows);
            if (expanded.Count == 0) return expanded as List<int> ?? new List<int>();
            var nextSet = new HashSet<int>();
            bool changed = false;
            for (int i = 0; i < expanded.Count; i++)
            {
                var id = expanded[i];
                if (valid.Contains(id)) nextSet.Add(id);
                else changed = true;
            }
            if (!changed && expanded is List<int> existing && existing.Count == nextSet.Count)
                return existing;
            var nextList = new List<int>(nextSet);
            nextList.Sort();
            return nextList;
        }
    }
}
