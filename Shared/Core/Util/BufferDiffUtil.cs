using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveUITK.Core.Util
{
    /// <summary>
    /// Utility helpers for list-backed UI virtualization: maintains a stable backing buffer,
    /// overlays per-index changes, and triggers targeted refresh callbacks for changed rows.
    /// </summary>
    public static class BufferDiffUtil
    {
        /// <summary>
        /// Normalizes an incoming items object to an IList. If items is IEnumerable, copies into a new List&lt;object&gt;.
        /// Returns null when items is null or not a collection.
        /// </summary>
        public static IList NormalizeToList(object items)
        {
            if (items == null)
            {
                return null;
            }
            if (items is IList ilist)
            {
                return ilist;
            }
            if (items is IEnumerable enumerable)
            {
                var list = new List<object>();
                foreach (var it in enumerable) list.Add(it);
                return list;
            }
            return null;
        }

        /// <summary>
        /// Ensures the backing buffer exists. When created, copies items from 'from' (if provided).
        /// Returns true when a new buffer was created.
        /// </summary>
        public static bool EnsureBuffer(ref List<object> buffer, IList from)
        {
            if (buffer != null)
            {
                return false;
            }
            buffer = new List<object>();
            if (from != null)
            {
                int n = from.Count;
                buffer.Capacity = n;
                for (int i = 0; i < n; i++) buffer.Add(from[i]);
            }
            return true;
        }

        /// <summary>
        /// Overlays per-index differences from 'next' into 'buffer' when counts match.
        /// Uses the provided equality function (or reference/Equals) to detect changes.
        /// Invokes 'onChanged(index)' for each changed index. Returns true when any changes were applied.
        /// </summary>
        public static bool OverlayDiff(
            IList buffer,
            IList next,
            Func<object, object, bool> equals = null,
            Action<int> onChanged = null)
        {
            if (buffer == null || next == null || buffer.Count != next.Count)
            {
                return false;
            }
            equals ??= ((a, b) => ReferenceEquals(a, b) || Equals(a, b));
            bool any = false;
            int count = buffer.Count;
            for (int i = 0; i < count; i++)
            {
                var cur = buffer[i];
                var nxt = next[i];
                if (!equals(cur, nxt))
                {
                    buffer[i] = nxt;
                    any = true;
                    onChanged?.Invoke(i);
                }
            }
            return any;
        }

        /// <summary>
        /// Rebuilds buffer to exactly match 'next' (clears and copies). Safe when 'next' is null.
        /// </summary>
        public static void RebuildBuffer(IList buffer, IList next)
        {
            if (buffer == null)
            {
                return;
            }
            buffer.Clear();
            if (next == null)
            {
                return;
            }
            if (buffer is List<object> list && list.Capacity < next.Count)
            {
                list.Capacity = next.Count;
            }
            int n = next.Count;
            for (int i = 0; i < n; i++) buffer.Add(next[i]);
        }
    }
}

