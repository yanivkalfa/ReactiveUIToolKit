using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveUITK.Core.Util
{
    /// <summary>
    /// Utility for id-keyed overlay of collections used by virtualized views (e.g., TreeView/MultiColumnTreeView).
    /// Computes added/removed/updated ids and can drive targeted refresh-by-id when structure is stable.
    /// </summary>
    public static class IdKeyedDiffUtil
    {
        public sealed class IdDiffResult
        {
            public readonly List<object> AddedIds = new List<object>();
            public readonly List<object> RemovedIds = new List<object>();
            public readonly List<object> UpdatedIds = new List<object>();
            public bool OrderChanged;
            public bool StructureChanged => AddedIds.Count > 0 || RemovedIds.Count > 0 || OrderChanged;
        }

        /// <summary>
        /// Builds a mapping id->(index,item) for a sequence using provided id selector.
        /// </summary>
        private static Dictionary<object, (int index, object item)> BuildMap(IEnumerable sequence, Func<object, object> getId)
        {
            var map = new Dictionary<object, (int, object)>();
            if (sequence == null || getId == null) return map;
            int idx = 0;
            foreach (var it in sequence)
            {
                var id = getId(it);
                if (id != null && !map.ContainsKey(id))
                {
                    map.Add(id, (idx, it));
                }
                idx++;
            }
            return map;
        }

        /// <summary>
        /// Compares two id-keyed sequences and returns ids that were added/removed/updated; also flags order changes.
        /// 'equals' defaults to reference/Equals. Null-safe.
        /// </summary>
        public static IdDiffResult DiffById(
            IEnumerable current,
            IEnumerable next,
            Func<object, object> getId,
            Func<object, object, bool> equals = null)
        {
            equals ??= ((a, b) => ReferenceEquals(a, b) || Equals(a, b));
            var result = new IdDiffResult();
            var curMap = BuildMap(current, getId);
            var nxtMap = BuildMap(next, getId);

            // Added & updated
            foreach (var kv in nxtMap)
            {
                var id = kv.Key;
                var (nIdx, nItem) = kv.Value;
                if (!curMap.TryGetValue(id, out var curTuple))
                {
                    result.AddedIds.Add(id);
                }
                else
                {
                    if (!equals(curTuple.item, nItem))
                    {
                        result.UpdatedIds.Add(id);
                    }
                    if (curTuple.index != nIdx)
                    {
                        result.OrderChanged = true;
                    }
                }
            }
            // Removed
            foreach (var id in curMap.Keys)
            {
                if (!nxtMap.ContainsKey(id))
                {
                    result.RemovedIds.Add(id);
                }
            }
            return result;
        }

        /// <summary>
        /// Applies an id-keyed overlay onto a backing buffer and invokes update or rebuild callbacks.
        /// Use when your view can refresh a row by its id (via id->index lookup).
        /// </summary>
        /// <param name="buffer">Stable backing buffer (assigned to itemsSource).</param>
        /// <param name="current">Enumeration of current items (can be buffer).</param>
        /// <param name="next">Incoming items sequence.</param>
        /// <param name="getId">Id selector for items.</param>
        /// <param name="equals">Equality check; defaults to reference/Equals.</param>
        /// <param name="idToRowIndex">Resolver from id to current row index. Return -1 if not mapped.</param>
        /// <param name="onRefreshRowIndex">Called for each row index to be refreshed in-place.</param>
        /// <param name="onRebuild">Called when structure changed (added/removed/order) — caller should rebuild view & buffer.</param>
        public static void OverlayById(
            IList buffer,
            IEnumerable current,
            IEnumerable next,
            Func<object, object> getId,
            Func<object, object, bool> equals,
            Func<object, int> idToRowIndex,
            Action<int> onRefreshRowIndex,
            Action onRebuild)
        {
            if (buffer == null)
            {
                onRebuild?.Invoke();
                return;
            }
            var diff = DiffById(current, next, getId, equals);
            if (diff.StructureChanged)
            {
                onRebuild?.Invoke();
                return;
            }
            // Stable structure: update changed ids only
            var nextMap = BuildMap(next, getId);
            foreach (var id in diff.UpdatedIds)
            {
                if (!nextMap.TryGetValue(id, out var tuple)) continue;
                int row = idToRowIndex != null ? idToRowIndex(id) : tuple.index;
                if (row >= 0 && row < buffer.Count)
                {
                    buffer[row] = tuple.item;
                    onRefreshRowIndex?.Invoke(row);
                }
            }
        }
    }
}

