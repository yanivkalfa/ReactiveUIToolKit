using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class RepeatButtonElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            RepeatButton repeatButton = new();
            return repeatButton;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is RepeatButton repeatButtonElement && properties != null)
            {
                TryApplyProp<string>(properties, "text", value => { repeatButtonElement.text = value ?? string.Empty; });
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is RepeatButton repeatButtonElement)
            {
                TryDiffProp<string>(previous, next, "text", value => { repeatButtonElement.text = value ?? string.Empty; });
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}

