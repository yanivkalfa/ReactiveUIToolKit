using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class DropdownFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new DropdownField();
        }

        private static void ApplySlots(
            DropdownField df,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            if (
                properties.TryGetValue("label", out var labelObj)
                && labelObj is Dictionary<string, object> labelMap
            )
            {
                if (df.labelElement != null)
                {
                    PropsApplier.Apply(df.labelElement, labelMap);
                }
            }
            if (
                properties.TryGetValue("visualInput", out var viObj)
                && viObj is Dictionary<string, object> viMap
            )
            {
                var input = df.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                {
                    PropsApplier.Apply(input, viMap);
                }
            }
        }

        private static void ApplySlotsDiff(
            DropdownField df,
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
                if (df.labelElement != null)
                {
                    PropsApplier.Apply(df.labelElement, labelMap);
                }
            }
            previous.TryGetValue("visualInput", out var prevVi);
            next.TryGetValue("visualInput", out var nextVi);
            if (!ReferenceEquals(prevVi, nextVi) && nextVi is Dictionary<string, object> viMap)
            {
                var input = df.Q<VisualElement>(className: "unity-base-field__input");
                if (input != null)
                {
                    PropsApplier.Apply(input, viMap);
                }
            }
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (!(element is DropdownField df) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            if (properties.TryGetValue("choices", out var ch) && ch is IList<string> list)
            {
                df.choices = new List<string>(list);
            }
            else if (
                properties.TryGetValue("choices", out var ch2) && ch2 is IEnumerable<string> en
            )
            {
                df.choices = new List<string>(en);
            }

            TryApplyProp<string>(properties, "value", v => df.value = v);
            TryApplyProp<int>(
                properties,
                "selectedIndex",
                i =>
                {
                    if (df.choices != null && i >= 0 && i < df.choices.Count)
                    {
                        df.value = df.choices[i];
                    }
                }
            );

            ApplySlots(df, properties);

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is DropdownField df)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;

                previous.TryGetValue("choices", out var prevChoices);
                next.TryGetValue("choices", out var nextChoices);
                if (!ReferenceEquals(prevChoices, nextChoices))
                {
                    if (nextChoices is IList<string> list)
                    {
                        df.choices = new List<string>(list);
                    }
                    else if (nextChoices is IEnumerable<string> en)
                    {
                        df.choices = new List<string>(en);
                    }
                }

                TryDiffProp<string>(previous, next, "value", v => df.value = v);
                if (
                    TryDiffProp<int>(
                        previous,
                        next,
                        "selectedIndex",
                        i =>
                        {
                            if (df.choices != null && i >= 0 && i < df.choices.Count)
                            {
                                df.value = df.choices[i];
                            }
                        }
                    )
                ) { }

                ApplySlotsDiff(df, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is DropdownField df && props is DropdownFieldProps tp)
            {
                if (tp.Choices != null)
                    df.choices = new List<string>(tp.Choices);
                if (tp.Value != null)
                    df.value = tp.Value;
                if (
                    tp.SelectedIndex.HasValue
                    && df.choices != null
                    && tp.SelectedIndex.Value >= 0
                    && tp.SelectedIndex.Value < df.choices.Count
                )
                    df.value = df.choices[tp.SelectedIndex.Value];
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
                if (tp.Label != null && df.labelElement != null)
                    PropsApplier.Apply(df.labelElement, tp.Label);
                if (tp.VisualInput != null)
                {
                    var input = df.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                        PropsApplier.Apply(input, tp.VisualInput);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is DropdownField df
                && prev is DropdownFieldProps tp
                && next is DropdownFieldProps tn
            )
            {
                if (!ReferenceEquals(tp.Choices, tn.Choices) && tn.Choices != null)
                    df.choices = new List<string>(tn.Choices);
                if (tp.Value != tn.Value)
                    df.value = tn.Value;
                if (
                    tp.SelectedIndex != tn.SelectedIndex
                    && tn.SelectedIndex.HasValue
                    && df.choices != null
                    && tn.SelectedIndex.Value >= 0
                    && tn.SelectedIndex.Value < df.choices.Count
                )
                    df.value = df.choices[tn.SelectedIndex.Value];
                if (tp.OnChange != tn.OnChange)
                {
                    if (tn.OnChange != null)
                        PropsApplier.ApplySingle(element, tp.OnChange, "onChange", tn.OnChange);
                    else if (tp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", tp.OnChange);
                }
                if (tp.OnChangeCapture != tn.OnChangeCapture)
                {
                    if (tn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            tp.OnChangeCapture,
                            "onChangeCapture",
                            tn.OnChangeCapture
                        );
                    else if (tp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", tp.OnChangeCapture);
                }
                if (!ReferenceEquals(tp.Label, tn.Label) && df.labelElement != null)
                {
                    if (tp.Label != null && tn.Label != null)
                        PropsApplier.ApplyDiff(df.labelElement, tp.Label, tn.Label);
                    else if (tn.Label != null)
                        PropsApplier.Apply(df.labelElement, tn.Label);
                }
                if (!ReferenceEquals(tp.VisualInput, tn.VisualInput))
                {
                    var input = df.Q<VisualElement>(className: "unity-base-field__input");
                    if (input != null)
                    {
                        if (tp.VisualInput != null && tn.VisualInput != null)
                            PropsApplier.ApplyDiff(input, tp.VisualInput, tn.VisualInput);
                        else if (tn.VisualInput != null)
                            PropsApplier.Apply(input, tn.VisualInput);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
