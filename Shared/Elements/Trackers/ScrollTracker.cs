using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // No-op stub for future scroll position persistence across rebuilds.
    internal sealed class ScrollTracker : IElementStateTracker<ScrollView, object>
    {
        public void Attach(
            ScrollView element,
            object state,
            IReadOnlyDictionary<string, object> props
        )
        {
            // TODO: listen to scroll position changes and update state
        }

        public void Detach(ScrollView element, object state)
        {
            // TODO: detach listeners when wired
        }

        public void Reapply(
            ScrollView element,
            object state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            // TODO: set scroll position from state/props; for now, no-op
        }
    }
}
