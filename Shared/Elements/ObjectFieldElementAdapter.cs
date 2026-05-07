using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.UIElements;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Elements
{
    public sealed class ObjectFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ObjectField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not ObjectField field || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<UnityEngine.Object>(properties, "value", v => field.value = v);
            TryApplyProp<bool>(properties, "allowSceneObjects", v => field.allowSceneObjects = v);
            if (properties.TryGetValue("objectType", out var tObj))
            {
                var t = tObj as Type ?? (tObj is string s ? Type.GetType(s) : null);
                if (t != null && typeof(UnityEngine.Object).IsAssignableFrom(t))
                {
                    field.objectType = t;
                }
            }

            ApplySlots(field, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not ObjectField field)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<UnityEngine.Object>(previous, next, "value", v => field.value = v);
            TryDiffProp<bool>(
                previous,
                next,
                "allowSceneObjects",
                v => field.allowSceneObjects = v
            );
            if (next.TryGetValue("objectType", out var tObj))
            {
                var t = tObj as Type ?? (tObj is string s ? Type.GetType(s) : null);
                if (t != null && typeof(UnityEngine.Object).IsAssignableFrom(t))
                {
                    field.objectType = t;
                }
            }
            ApplySlotsDiff(field, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            BaseField<UnityEngine.Object> field,
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
            BaseField<UnityEngine.Object> field,
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
            if (element is ObjectField field && props is ObjectFieldProps fp)
            {
                if (fp.Value != null)
                    field.value = fp.Value;
                if (fp.AllowSceneObjects.HasValue)
                    field.allowSceneObjects = fp.AllowSceneObjects.Value;
                if (!string.IsNullOrEmpty(fp.ObjectType))
                {
                    var t = Type.GetType(fp.ObjectType);
                    if (t != null && typeof(UnityEngine.Object).IsAssignableFrom(t))
                        field.objectType = t;
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
                element is ObjectField field
                && prev is ObjectFieldProps fp
                && next is ObjectFieldProps fn
            )
            {
                if (fp.Value != fn.Value && fn.Value != null)
                    field.value = fn.Value;
                if (fp.AllowSceneObjects != fn.AllowSceneObjects && fn.AllowSceneObjects.HasValue)
                    field.allowSceneObjects = fn.AllowSceneObjects.Value;
                if (fp.ObjectType != fn.ObjectType && !string.IsNullOrEmpty(fn.ObjectType))
                {
                    var t = Type.GetType(fn.ObjectType);
                    if (t != null && typeof(UnityEngine.Object).IsAssignableFrom(t))
                        field.objectType = t;
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
#endif
