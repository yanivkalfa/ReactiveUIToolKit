using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class LabelElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Label();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Label labelElement && properties != null)
            {
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        var newVal = value ?? string.Empty;
                        labelElement.text = newVal;
                    }
                );
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Label labelElement)
            {
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        labelElement.text = value ?? string.Empty;
                    }
                );
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Label label && props is LabelProps lp)
            {
                if (lp.Text != null)
                    label.text = lp.Text;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Label label && prev is LabelProps lp && next is LabelProps ln)
            {
                if (lp.Text != ln.Text)
                    label.text = ln.Text ?? string.Empty;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
