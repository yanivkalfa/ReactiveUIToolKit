using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ScrollerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new Scroller();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is not Scroller sc)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            TryDiffProp<float>(previous, next, "lowValue", v => sc.lowValue = v);
            TryDiffProp<float>(previous, next, "highValue", v => sc.highValue = v);
            TryDiffProp<float>(previous, next, "value", v => sc.value = v);
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}

