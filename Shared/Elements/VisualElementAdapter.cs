using System.Collections.Generic;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class VisualElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<VisualElement>();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
