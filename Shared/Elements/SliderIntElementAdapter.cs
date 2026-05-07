using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
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

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is SliderInt slider && props is SliderIntProps tp)
            {
                if (tp.LowValue.HasValue)
                    slider.lowValue = tp.LowValue.Value;
                if (tp.HighValue.HasValue)
                    slider.highValue = tp.HighValue.Value;
                if (tp.Value.HasValue)
                    slider.value = tp.Value.Value;
                if (!string.IsNullOrEmpty(tp.Direction))
                {
                    var ds = tp.Direction.ToLowerInvariant();
                    slider.direction =
                        ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                }
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is SliderInt slider
                && prev is SliderIntProps tp
                && next is SliderIntProps tn
            )
            {
                if (tp.LowValue != tn.LowValue && tn.LowValue.HasValue)
                    slider.lowValue = tn.LowValue.Value;
                if (tp.HighValue != tn.HighValue && tn.HighValue.HasValue)
                    slider.highValue = tn.HighValue.Value;
                if (tp.Value != tn.Value && tn.Value.HasValue)
                    slider.value = tn.Value.Value;
                if (tp.Direction != tn.Direction && !string.IsNullOrEmpty(tn.Direction))
                {
                    var ds = tn.Direction.ToLowerInvariant();
                    slider.direction =
                        ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                }
                if (tp.OnChange != tn.OnChange)
                {
                    if (tn.OnChange != null)
                        PropsApplier.ApplySingle(element, tp.OnChange, "onChange", tn.OnChange);
                    else if (tp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", tp.OnChange);
                }
                if (tp.OnChangeCapture != tn.OnChangeCapture)
                {
                    if (tn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            tp.OnChangeCapture,
                            "onChangeCapture",
                            tn.OnChangeCapture
                        );
                    else if (tp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", tp.OnChangeCapture);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
