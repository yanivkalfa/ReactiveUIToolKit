using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public interface IElementAdapter
    {
        VisualElement Create();
        void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties);
        void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next);
    }
}
