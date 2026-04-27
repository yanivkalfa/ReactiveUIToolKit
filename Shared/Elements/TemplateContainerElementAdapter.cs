using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
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

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TemplateContainer tc || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TemplateContainer tc)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (
                element is TemplateContainer tc
                && props.ContentContainer is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is TemplateContainer tc)
            {
                if (
                    !ReferenceEquals(prev.ContentContainer, next.ContentContainer)
                    && next.ContentContainer is Dictionary<string, object> ccMap
                )
                    PropsApplier.Apply(tc.contentContainer, ccMap);
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
