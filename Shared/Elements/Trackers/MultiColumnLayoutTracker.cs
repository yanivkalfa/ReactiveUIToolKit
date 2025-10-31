using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnLayoutTracker
        : IElementStateTracker<MultiColumnTreeView, MultiColumnTreeViewElementAdapter.Cached>
    {
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
