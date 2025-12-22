using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class LabelElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Label();
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
                        var newVal = value ?? string.Empty;
                        // Debug.Log($"[LabelAdapter] Setting text to '{newVal}' on {labelElement.GetHashCode()}"); 
                        labelElement.text = newVal;
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
