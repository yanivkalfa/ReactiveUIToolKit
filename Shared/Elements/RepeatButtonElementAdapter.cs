using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class RepeatButtonElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new RepeatButton();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is RepeatButton repeatButtonElement && properties != null)
            {
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        repeatButtonElement.text = value ?? string.Empty;
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
            if (element is RepeatButton repeatButtonElement)
            {
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        repeatButtonElement.text = value ?? string.Empty;
                    }
                );
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is RepeatButton rb && props is RepeatButtonProps rp)
            {
                if (rp.Text != null)
                    rb.text = rp.Text;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is RepeatButton rb
                && prev is RepeatButtonProps rp
                && next is RepeatButtonProps rn
            )
            {
                if (rp.Text != rn.Text)
                    rb.text = rn.Text ?? string.Empty;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
