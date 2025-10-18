using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public interface IElementAdapter
    {
        VisualElement Create();
        // Apply full property set (initial mount)
        void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties);
        // Apply only differences between previous and next (update)
        void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next);
    }
}
