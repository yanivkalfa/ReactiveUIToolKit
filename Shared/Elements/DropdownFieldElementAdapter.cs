using System.Collections.Generic;
using ReactiveUITK.Props;
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
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
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
                previous ??= new Dictionary<string, object>();
                next ??= new Dictionary<string, object>();

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
    }
}
