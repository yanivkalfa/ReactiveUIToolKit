using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public interface IElementAdapter
    {
        VisualElement Create();
        void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> props);
        void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> prev,
            IReadOnlyDictionary<string, object> next
        );
        VisualElement ResolveChildHost(VisualElement element);
    }
}
