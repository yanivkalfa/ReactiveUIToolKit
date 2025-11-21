using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class MinMaxSliderElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new MinMaxSlider();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not MinMaxSlider s || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<float>(properties, "minValue", v => s.minValue = v);
            TryApplyProp<float>(properties, "maxValue", v => s.maxValue = v);
            TryApplyProp<float>(properties, "lowLimit", v => s.lowLimit = v);
            TryApplyProp<float>(properties, "highLimit", v => s.highLimit = v);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not MinMaxSlider s)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            TryDiffProp<float>(previous, next, "minValue", v => s.minValue = v);
            TryDiffProp<float>(previous, next, "maxValue", v => s.maxValue = v);
            TryDiffProp<float>(previous, next, "lowLimit", v => s.lowLimit = v);
            TryDiffProp<float>(previous, next, "highLimit", v => s.highLimit = v);
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}

