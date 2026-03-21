using System.Collections.Generic;
using ReactiveUITK.Props;
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
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            TryDiffProp<string>(previous, next, "text", v => te.text = v ?? string.Empty);
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
