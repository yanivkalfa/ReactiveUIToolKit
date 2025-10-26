using ReactiveUITK.Elements.Pools;
using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class SliderElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<Slider>();
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (!(element is Slider slider) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<float>(properties, "lowValue", v => slider.lowValue = v);
            TryApplyProp<float>(properties, "highValue", v => slider.highValue = v);
            TryApplyProp<float>(properties, "value", v => slider.value = v);
            if (properties.TryGetValue("direction", out var dirObj))
            {
                if (dirObj is SliderDirection dir)
                {
                    slider.direction = dir;
                }
                else if (dirObj is string ds)
                {
                    ds = ds.ToLowerInvariant();
                    slider.direction = ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                }
            }

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is Slider slider)
            {
                TryDiffProp<float>(previous, next, "lowValue", v => slider.lowValue = v);
                TryDiffProp<float>(previous, next, "highValue", v => slider.highValue = v);
                TryDiffProp<float>(previous, next, "value", v => slider.value = v);
                if (!TryDiffProp<SliderDirection>(previous, next, "direction", v => slider.direction = v))
                {
                    previous ??= new Dictionary<string, object>();
                    next ??= new Dictionary<string, object>();
                    previous.TryGetValue("direction", out var pd);
                    next.TryGetValue("direction", out var nd);
                    if (!Equals(pd, nd) && nd is string ds)
                    {
                        ds = ds.ToLowerInvariant();
                        slider.direction = ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                    }
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}

