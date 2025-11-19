using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    
    
    internal sealed class SelectionTracker : IElementStateTracker<ListView, object>
    {
        public void Attach(
            ListView element,
            object state,
            IReadOnlyDictionary<string, object> props
        )
        {
            
            
            
        }

        public void Detach(ListView element, object state)
        {
            
        }

        public void Reapply(
            ListView element,
            object state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            
        }
    }
}
