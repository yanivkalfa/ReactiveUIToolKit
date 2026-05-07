using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class IMGUIContainerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new IMGUIContainer();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not IMGUIContainer c || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            if (properties.TryGetValue("onGUI", out var og) && og is Action a)
            {
                c.onGUIHandler = a;
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not IMGUIContainer c)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            if (next != null && next.TryGetValue("onGUI", out var og) && og is Action a)
            {
                c.onGUIHandler = a;
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is IMGUIContainer c && props is IMGUIContainerProps ip)
            {
                if (ip.OnGUI != null)
                    c.onGUIHandler = ip.OnGUI;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is IMGUIContainer c
                && prev is IMGUIContainerProps ip
                && next is IMGUIContainerProps inext
            )
            {
                if (ip.OnGUI != inext.OnGUI)
                    c.onGUIHandler = inext.OnGUI;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
