using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class SliderIntElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new SliderInt();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not SliderInt slider || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<int>(properties, "lowValue", v => slider.lowValue = v);
            TryApplyProp<int>(properties, "highValue", v => slider.highValue = v);
            TryApplyProp<int>(properties, "value", v => slider.value = v);
            if (properties.TryGetValue("direction", out var dirObj))
            {
                if (dirObj is SliderDirection dir)
                {
                    slider.direction = dir;
                }
                else if (dirObj is string ds)
                {
                    ds = ds.ToLowerInvariant();
                    slider.direction =
                        ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                }
            }

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is SliderInt slider)
            {
                TryDiffProp<int>(previous, next, "lowValue", v => slider.lowValue = v);
                TryDiffProp<int>(previous, next, "highValue", v => slider.highValue = v);
                TryDiffProp<int>(previous, next, "value", v => slider.value = v);
                if (
                    !TryDiffProp<SliderDirection>(
                        previous,
                        next,
                        "direction",
                        v => slider.direction = v
                    )
                )
                {
                    previous ??= s_emptyProps;
                    next ??= s_emptyProps;
                    previous.TryGetValue("direction", out var pd);
                    next.TryGetValue("direction", out var nd);
                    if (!Equals(pd, nd) && nd is string ds)
                    {
                        ds = ds.ToLowerInvariant();
                        slider.direction =
                            ds == "vertical"
                                ? SliderDirection.Vertical
                                : SliderDirection.Horizontal;
                    }
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
