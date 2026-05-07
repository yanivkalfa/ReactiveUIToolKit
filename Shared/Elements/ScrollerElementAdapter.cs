using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ScrollerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new Scroller();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not Scroller sc || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<float>(properties, "lowValue", v => sc.lowValue = v);
            TryApplyProp<float>(properties, "highValue", v => sc.highValue = v);
            TryApplyProp<float>(properties, "value", v => sc.value = v);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not Scroller sc)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<float>(previous, next, "lowValue", v => sc.lowValue = v);
            TryDiffProp<float>(previous, next, "highValue", v => sc.highValue = v);
            TryDiffProp<float>(previous, next, "value", v => sc.value = v);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Scroller sc && props is ScrollerProps sp)
            {
                if (sp.LowValue.HasValue)
                    sc.lowValue = sp.LowValue.Value;
                if (sp.HighValue.HasValue)
                    sc.highValue = sp.HighValue.Value;
                if (sp.Value.HasValue)
                    sc.value = sp.Value.Value;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Scroller sc && prev is ScrollerProps sp && next is ScrollerProps sn)
            {
                if (sp.LowValue != sn.LowValue && sn.LowValue.HasValue)
                    sc.lowValue = sn.LowValue.Value;
                if (sp.HighValue != sn.HighValue && sn.HighValue.HasValue)
                    sc.highValue = sn.HighValue.Value;
                if (sp.Value != sn.Value && sn.Value.HasValue)
                    sc.value = sn.Value.Value;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
