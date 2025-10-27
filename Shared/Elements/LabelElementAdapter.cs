using System.Collections.Generic;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class LabelElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<Label>();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Label labelElement && properties != null)
            {
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        labelElement.text = value ?? string.Empty;
                    }
                );
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Label labelElement)
            {
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        labelElement.text = value ?? string.Empty;
                    }
                );
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
