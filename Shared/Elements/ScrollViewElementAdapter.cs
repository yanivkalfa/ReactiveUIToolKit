using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ScrollViewElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new ScrollView();
        }

        private static void ApplySlots(
            ScrollView sv,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var ccObj)
                && ccObj is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(sv.contentContainer, ccMap);
            }
        }

        private static void ApplySlotsDiff(
            ScrollView sv,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var prevCC);
            next.TryGetValue("contentContainer", out var nextCC);
            if (!ReferenceEquals(prevCC, nextCC) && nextCC is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(sv.contentContainer, ccMap);
            }
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (!(element is ScrollView sv) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            if (properties.TryGetValue("mode", out var modeObj))
            {
                if (modeObj is ScrollViewMode m)
                {
                    sv.mode = m;
                }
                else if (modeObj is string ms)
                {
                    ms = ms.ToLowerInvariant();
                    sv.mode = ms switch
                    {
                        "vertical" => ScrollViewMode.Vertical,
                        "horizontal" => ScrollViewMode.Horizontal,
                        _ => ScrollViewMode.Vertical,
                    };
                }
            }

            if (properties.TryGetValue("verticalScrollerVisibility", out var vsvObj))
            {
                if (vsvObj is ScrollerVisibility vis)
                {
                    sv.verticalScrollerVisibility = vis;
                }
                else if (vsvObj is string svis)
                {
                    svis = svis.ToLowerInvariant();
                    sv.verticalScrollerVisibility = svis switch
                    {
                        "auto" => ScrollerVisibility.Auto,
                        "hidden" => ScrollerVisibility.Hidden,
                        _ => ScrollerVisibility.Auto,
                    };
                }
            }

            if (properties.TryGetValue("horizontalScrollerVisibility", out var hsvObj))
            {
                if (hsvObj is ScrollerVisibility hvis)
                {
                    sv.horizontalScrollerVisibility = hvis;
                }
                else if (hsvObj is string shvis)
                {
                    shvis = shvis.ToLowerInvariant();
                    sv.horizontalScrollerVisibility = shvis switch
                    {
                        "auto" => ScrollerVisibility.Auto,
                        "hidden" => ScrollerVisibility.Hidden,
                        _ => ScrollerVisibility.Auto,
                    };
                }
            }

            if (properties.TryGetValue("scrollOffset", out var offObj) && offObj is Vector2 off)
            {
                sv.scrollOffset = off;
            }

            ApplySlots(sv, properties);

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ScrollView sv)
            {
                previous ??= new Dictionary<string, object>();
                next ??= new Dictionary<string, object>();

                TryDiffProp<ScrollViewMode>(previous, next, "mode", m => sv.mode = m);
                if (!TryDiffProp<ScrollViewMode>(previous, next, "mode", m => sv.mode = m))
                {
                    previous.TryGetValue("mode", out var pm);
                    next.TryGetValue("mode", out var nm);
                    if (!Equals(pm, nm) && nm is string ms)
                    {
                        ms = ms.ToLowerInvariant();
                        sv.mode = ms switch
                        {
                            "vertical" => ScrollViewMode.Vertical,
                            "horizontal" => ScrollViewMode.Horizontal,
                            _ => ScrollViewMode.Vertical,
                        };
                    }
                }

                TryDiffProp<ScrollerVisibility>(
                    previous,
                    next,
                    "verticalScrollerVisibility",
                    v => sv.verticalScrollerVisibility = v
                );
                TryDiffProp<ScrollerVisibility>(
                    previous,
                    next,
                    "horizontalScrollerVisibility",
                    v => sv.horizontalScrollerVisibility = v
                );
                TryDiffProp<Vector2>(previous, next, "scrollOffset", v => sv.scrollOffset = v);

                ApplySlotsDiff(sv, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            var sv = element as ScrollView;
            return sv != null ? sv.contentContainer : element;
        }
    }
}
