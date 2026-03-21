using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class SliderElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public VisualElement Input;
            public VisualElement Track;
            public VisualElement DragContainer;
            public VisualElement Handle;
            public VisualElement HandleBorder;
        }

        private static readonly ConditionalWeakTable<Slider, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return new Slider();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
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
                    slider.direction =
                        ds == "vertical" ? SliderDirection.Vertical : SliderDirection.Horizontal;
                }
            }

            ApplySlots(slider, properties);

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Slider slider)
            {
                TryDiffProp<float>(previous, next, "lowValue", v => slider.lowValue = v);
                TryDiffProp<float>(previous, next, "highValue", v => slider.highValue = v);
                TryDiffProp<float>(previous, next, "value", v => slider.value = v);
                if (
                    !TryDiffProp<SliderDirection>(
                        previous,
                        next,
                        "direction",
                        v => slider.direction = v
                    )
                )
                {
                    previous ??= new Dictionary<string, object>();
                    next ??= new Dictionary<string, object>();
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

                DiffSlot(slider, previous, next, "input");
                DiffSlot(slider, previous, next, "track");
                DiffSlot(slider, previous, next, "dragContainer");
                DiffSlot(slider, previous, next, "handle");
                DiffSlot(slider, previous, next, "handleBorder");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            Slider slider,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(slider, properties, "input");
            ApplySlot(slider, properties, "track");
            ApplySlot(slider, properties, "dragContainer");
            ApplySlot(slider, properties, "handle");
            ApplySlot(slider, properties, "handleBorder");
        }

        private static void DiffSlot(
            Slider slider,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            string slotKey
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue(slotKey, out var prevSlot);
            next.TryGetValue(slotKey, out var nextSlot);
            if (!ReferenceEquals(prevSlot, nextSlot))
            {
                ApplySlot(slider, next, slotKey);
            }
        }

        private static void ApplySlot(
            Slider slider,
            IReadOnlyDictionary<string, object> properties,
            string slotKey
        )
        {
            if (properties == null)
            {
                return;
            }
            if (!properties.TryGetValue(slotKey, out var slotObject))
            {
                return;
            }
            if (slotObject is not Dictionary<string, object> slotMap)
            {
                return;
            }

            var target = ResolveSlotElement(slider, slotKey);
            if (target == null)
            {
                return;
            }

            if (
                slotMap.TryGetValue("style", out var styleObject)
                && styleObject is IDictionary<string, object> styleMap
            )
            {
                PropsApplier.Apply(
                    target,
                    new Dictionary<string, object> { { "style", styleMap } }
                );
            }

            foreach (var entry in slotMap)
            {
                if (entry.Key == "style")
                {
                    continue;
                }
                PropsApplier.Apply(
                    target,
                    new Dictionary<string, object> { { entry.Key, entry.Value } }
                );
            }
        }

        private static VisualElement ResolveSlotElement(Slider slider, string slotKey)
        {
            if (!cache.TryGetValue(slider, out var parts))
            {
                parts = new CachedParts();
                cache.Add(slider, parts);
            }

            switch (slotKey)
            {
                case "input":
                    if (parts.Input == null)
                    {
                        parts.Input = slider.Q(className: "unity-base-slider__input");
                    }
                    return parts.Input;
                case "track":
                    if (parts.Track == null)
                    {
                        parts.Track = slider.Q(className: "unity-base-slider__tracker");
                    }
                    return parts.Track;
                case "dragContainer":
                    if (parts.DragContainer == null)
                    {
                        parts.DragContainer = slider.Q(
                            className: "unity-base-slider__drag-container"
                        );
                    }
                    return parts.DragContainer;
                case "handle":
                    if (parts.Handle == null)
                    {
                        parts.Handle = slider.Q(className: "unity-base-slider__dragger");
                    }
                    return parts.Handle;
                case "handleBorder":
                    if (parts.HandleBorder == null)
                    {
                        parts.HandleBorder = slider.Q(
                            className: "unity-base-slider__dragger-border"
                        );
                    }
                    return parts.HandleBorder;
                default:
                    return null;
            }
        }
    }
}
