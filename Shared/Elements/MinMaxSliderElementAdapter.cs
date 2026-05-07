using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
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
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<float>(previous, next, "minValue", v => s.minValue = v);
            TryDiffProp<float>(previous, next, "maxValue", v => s.maxValue = v);
            TryDiffProp<float>(previous, next, "lowLimit", v => s.lowLimit = v);
            TryDiffProp<float>(previous, next, "highLimit", v => s.highLimit = v);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is MinMaxSlider s && props is MinMaxSliderProps mp)
            {
                if (mp.MinValue.HasValue)
                    s.minValue = mp.MinValue.Value;
                if (mp.MaxValue.HasValue)
                    s.maxValue = mp.MaxValue.Value;
                if (mp.LowLimit.HasValue)
                    s.lowLimit = mp.LowLimit.Value;
                if (mp.HighLimit.HasValue)
                    s.highLimit = mp.HighLimit.Value;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is MinMaxSlider s
                && prev is MinMaxSliderProps mp
                && next is MinMaxSliderProps mn
            )
            {
                if (mp.MinValue != mn.MinValue && mn.MinValue.HasValue)
                    s.minValue = mn.MinValue.Value;
                if (mp.MaxValue != mn.MaxValue && mn.MaxValue.HasValue)
                    s.maxValue = mn.MaxValue.Value;
                if (mp.LowLimit != mn.LowLimit && mn.LowLimit.HasValue)
                    s.lowLimit = mn.LowLimit.Value;
                if (mp.HighLimit != mn.HighLimit && mn.HighLimit.HasValue)
                    s.highLimit = mn.HighLimit.Value;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
