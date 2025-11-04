using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // No-op stub for future selection persistence across rebuilds.
    // Intentionally uses object for state since it's not yet wired to a specific adapter cache.
    internal sealed class SelectionTracker : IElementStateTracker<ListView, object>
    {
        public void Attach(
            ListView element,
            object state,
            IReadOnlyDictionary<string, object> props
        )
        {
            // TODO: wire selection changed event to update state
            // Placeholder: read optional overrides from props if present
            // (e.g., selectedIndex, selectedIds)
        }

        public void Detach(ListView element, object state)
        {
            // TODO: detach events when wired
        }

        public void Reapply(
            ListView element,
            object state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            // TODO: set selection from state/props; for now, no-op
        }
    }
}
