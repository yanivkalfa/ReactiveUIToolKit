using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class IMGUIContainerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new IMGUIContainer();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
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
    }
}

