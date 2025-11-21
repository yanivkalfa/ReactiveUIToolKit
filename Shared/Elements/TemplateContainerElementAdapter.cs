using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TemplateContainerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new TemplateContainer();

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            return (element as TemplateContainer) ?? element;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is not TemplateContainer tc || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            if (properties.TryGetValue("contentContainer", out var cc) && cc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is not TemplateContainer tc)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}

