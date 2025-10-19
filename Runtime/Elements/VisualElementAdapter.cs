using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class VisualElementAdapter : IElementAdapter
    {
        public VisualElement Create()
        {
            return new VisualElement();
        }
        public void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null)
            {
                return;
            }
            PropsApplier.Apply(element, properties);
        }
        public void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
