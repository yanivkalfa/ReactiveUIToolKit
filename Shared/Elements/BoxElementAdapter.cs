using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class BoxElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new Box();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not Box box || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            ApplySlots(box, properties);
            PropsApplier.Apply(elemenst, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not Box box)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            ApplySlotsDiff(box, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(Box box, IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(box.contentContainer, ccMap);
            }
        }

        private static void ApplySlotsDiff(
            Box box,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(box.contentContainer, ccMap);
            }
        }
    }
}
