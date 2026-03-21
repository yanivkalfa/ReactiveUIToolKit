using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class Vector2IntFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new Vector2IntField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not Vector2IntField field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<Vector2Int>(properties, "value", v => field.value = v);
            ApplySlots(field, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not Vector2IntField field)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<Vector2Int>(previous, next, "value", v => field.value = v);
            ApplySlotsDiff(field, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            BaseField<Vector2Int> field,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("label", out var labelObj)
                && labelObj is Dictionary<string, object> labelMap
            )
            {
                if (field.labelElement != null)
                    PropsApplier.Apply(field.labelElement, labelMap);
            }
            if (
                properties.TryGetValue("visualInput", out var viObj)
                && viObj is Dictionary<string, object> viMap
            )
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, viMap);
            }
        }

        private static void ApplySlotsDiff(
            BaseField<Vector2Int> field,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            previous.TryGetValue("label", out var prevLabel);
            next.TryGetValue("label", out var nextLabel);
            if (
                !ReferenceEquals(prevLabel, nextLabel)
                && nextLabel is Dictionary<string, object> labelMap
            )
            {
                if (field.labelElement != null)
                    PropsApplier.Apply(field.labelElement, labelMap);
            }
            previous.TryGetValue("visualInput", out var prevVi);
            next.TryGetValue("visualInput", out var nextVi);
            if (!ReferenceEquals(prevVi, nextVi) && nextVi is Dictionary<string, object> viMap)
            {
                var input = field.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                    PropsApplier.Apply(input, viMap);
            }
        }
    }
}
