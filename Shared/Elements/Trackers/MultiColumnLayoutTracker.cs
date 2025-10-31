using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnLayoutTracker
        : IElementStateTracker<MultiColumnTreeView, MultiColumnTreeViewElementAdapter.Cached>
    {
        private static Dictionary<string, float> CaptureCurrentWidths(MultiColumnTreeView tv)
        {
            var map = new Dictionary<string, float>();
            if (tv == null)
                return map;
            try
            {
                foreach (var col in tv.columns)
                {
                    if (col == null)
                        continue;
                    var name = string.IsNullOrEmpty(col.name) ? null : col.name;
                    if (name == null)
                        continue;
                    try
                    {
                        map[name] = col.width.value;
                    }
                    catch { }
                }
            }
            catch { }
            return map;
        }

        public void Attach(
            MultiColumnTreeView tv,
            MultiColumnTreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> props
        )
        {
            // Read overrides from props if provided
            if (props != null && props.TryGetValue("columnWidths", out var widthsObj))
            {
                if (widthsObj is IDictionary<string, object> map)
                {
                    state.ColumnWidths.Clear();
                    foreach (var kv in map)
                    {
                        try
                        {
                            state.ColumnWidths[kv.Key] = System.Convert.ToSingle(kv.Value);
                        }
                        catch { }
                    }
                }
                else if (widthsObj is IDictionary<string, float> fMap)
                {
                    state.ColumnWidths = new Dictionary<string, float>(fMap);
                }
            }

            // Seed from current UI if nothing provided
            if (state.ColumnWidths == null || state.ColumnWidths.Count == 0)
            {
                state.ColumnWidths = CaptureCurrentWidths(tv);
            }
        }

        public void Detach(MultiColumnTreeView tv, MultiColumnTreeViewElementAdapter.Cached state)
        {
            // Placeholder for cleanup if needed
        }

        public void Reapply(
            MultiColumnTreeView tv,
            MultiColumnTreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            if (tv == null || state == null)
                return;

            // Capture current widths whenever we reapply (persist user changes across rebuilds)
            try
            {
                var current = CaptureCurrentWidths(tv);
                if (current != null && current.Count > 0)
                {
                    foreach (var kv in current)
                    {
                        state.ColumnWidths[kv.Key] = kv.Value;
                    }
                }
            }
            catch { }

            if (state.ColumnWidths == null || state.ColumnWidths.Count == 0)
                return;
            foreach (var col in tv.columns)
            {
                if (col == null)
                    continue;
                var name = col.name;
                if (!string.IsNullOrEmpty(name) && state.ColumnWidths.TryGetValue(name, out var w))
                {
                    try
                    {
                        col.width = w;
                    }
                    catch { }
                }
            }
        }
    }
}
