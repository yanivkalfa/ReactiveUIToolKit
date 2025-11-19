using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    
    internal sealed class ScrollTracker : IElementStateTracker<ScrollView, object>
    {
        public void Attach(
            ScrollView element,
            object state,
            IReadOnlyDictionary<string, object> props
        )
        {
            
        }

        public void Detach(ScrollView element, object state)
        {
            
        }

        public void Reapply(
            ScrollView element,
            object state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            
        }
    }
}
