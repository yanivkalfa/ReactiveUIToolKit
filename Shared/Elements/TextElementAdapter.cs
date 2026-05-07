using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TextElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new TextElement();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TextElement te || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<string>(properties, "text", v => te.text = v ?? string.Empty);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TextElement te)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<string>(previous, next, "text", v => te.text = v ?? string.Empty);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is TextElement te && props is TextElementProps tp)
            {
                if (tp.Text != null)
                    te.text = tp.Text;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is TextElement te
                && prev is TextElementProps tp
                && next is TextElementProps tn
            )
            {
                if (tp.Text != tn.Text)
                    te.text = tn.Text ?? string.Empty;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
