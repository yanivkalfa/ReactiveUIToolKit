using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class EnumFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new EnumField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not EnumField field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<Enum>(properties, "value", v => field.value = v);
            TryApplyProp<string>(
                properties,
                "enumType",
                s =>
                {
                    var t = Type.GetType(s);
                    if (t != null && t.IsEnum)
                    {
                        try
                        {
                            field.Init((Enum)Enum.ToObject(t, 0));
                        }
                        catch { }
                    }
                }
            );

            ApplySlots(field, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not EnumField field)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;

            TryDiffProp<Enum>(previous, next, "value", v => field.value = v);
            ApplySlotsDiff(field, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            BaseField<Enum> field,
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
            BaseField<Enum> field,
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

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is EnumField field && props is EnumFieldProps fp)
            {
                if (fp.Value != null)
                    field.value = fp.Value;
                if (!string.IsNullOrEmpty(fp.EnumType))
                {
                    var t = Type.GetType(fp.EnumType);
                    if (t != null && t.IsEnum)
                    {
                        try
                        {
                            field.Init((Enum)Enum.ToObject(t, 0));
                        }
                        catch { }
                    }
                }
                if (fp.Label != null && field.labelElement != null)
                    PropsApplier.Apply(field.labelElement, fp.Label);
                if (fp.VisualInput != null)
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                        PropsApplier.Apply(input, fp.VisualInput);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is EnumField field
                && prev is EnumFieldProps fp
                && next is EnumFieldProps fn
            )
            {
                if (!object.Equals(fp.Value, fn.Value) && fn.Value != null)
                    field.value = fn.Value;
                if (fp.EnumType != fn.EnumType && !string.IsNullOrEmpty(fn.EnumType))
                {
                    var t = Type.GetType(fn.EnumType);
                    if (t != null && t.IsEnum)
                    {
                        try
                        {
                            field.Init((Enum)Enum.ToObject(t, 0));
                        }
                        catch { }
                    }
                }
                if (!ReferenceEquals(fp.Label, fn.Label) && field.labelElement != null)
                {
                    if (fp.Label != null && fn.Label != null)
                        PropsApplier.ApplyDiff(field.labelElement, fp.Label, fn.Label);
                    else if (fn.Label != null)
                        PropsApplier.Apply(field.labelElement, fn.Label);
                }
                if (!ReferenceEquals(fp.VisualInput, fn.VisualInput))
                {
                    var input = field.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                    {
                        if (fp.VisualInput != null && fn.VisualInput != null)
                            PropsApplier.ApplyDiff(input, fp.VisualInput, fn.VisualInput);
                        else if (fn.VisualInput != null)
                            PropsApplier.Apply(input, fn.VisualInput);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
