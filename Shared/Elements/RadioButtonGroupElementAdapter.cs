using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class RadioButtonGroupElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public VisualElement ContentContainer;
        }

        private static readonly ConditionalWeakTable<RadioButtonGroup, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return new RadioButtonGroup();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is RadioButtonGroup groupElement && properties != null)
            {
                if (
                    properties.TryGetValue("choices", out object choicesObject)
                    && choicesObject is IEnumerable<string> choices
                )
                {
                    groupElement.choices = choices;
                }
                TryApplyProp<int>(
                    properties,
                    "value",
                    v =>
                    {
                        groupElement.value = v;
                    }
                );
                TryApplyProp<string>(
                    properties,
                    "value",
                    v =>
                    {
                        int resolved = ResolveIndex(groupElement.choices, v);
                        if (resolved >= 0)
                        {
                            groupElement.value = resolved;
                        }
                    }
                );
                TryApplyProp<int>(
                    properties,
                    "index",
                    idx =>
                    {
                        int clamped = ClampIndex(groupElement.choices, idx);
                        groupElement.value = clamped;
                    }
                );
                ApplySlots(groupElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is RadioButtonGroup groupElement)
            {
                object previousChoices = null;
                object nextChoices = null;
                if (previous != null)
                {
                    previous.TryGetValue("choices", out previousChoices);
                }
                if (next != null)
                {
                    next.TryGetValue("choices", out nextChoices);
                }
                if (
                    !ReferenceEquals(previousChoices, nextChoices)
                    && nextChoices is IEnumerable<string> choices
                )
                {
                    groupElement.choices = choices;
                }
                TryDiffProp<int>(
                    previous,
                    next,
                    "value",
                    v =>
                    {
                        groupElement.value = v;
                    }
                );
                TryDiffProp<string>(
                    previous,
                    next,
                    "value",
                    v =>
                    {
                        int resolved = ResolveIndex(groupElement.choices, v);
                        if (resolved >= 0)
                        {
                            groupElement.value = resolved;
                        }
                    }
                );
                TryDiffProp<int>(
                    previous,
                    next,
                    "index",
                    idx =>
                    {
                        int clamped = ClampIndex(groupElement.choices, idx);
                        groupElement.value = clamped;
                    }
                );
                if (
                    ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.EnableDiffTracing
                    && ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.CurrentTraceLevel
                        != ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.TraceLevel.None
                )
                {
                    UnityEngine.Debug.Log(
                        $"[RadioGroupDiff] key={(element.userData as ReactiveUITK.Core.NodeMetadata)?.Key} value={groupElement.value} choicesRef={(groupElement.choices != null ? groupElement.choices.GetHashCode() : 0)}"
                    );
                }
                DiffSlot(groupElement, previous, next, "contentContainer");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is RadioButtonGroup groupElement && props is RadioButtonGroupProps tp)
            {
                if (tp.Choices != null)
                    groupElement.choices = tp.Choices;
                if (tp.Value != null)
                {
                    int resolved = ResolveIndex(groupElement.choices, tp.Value);
                    if (resolved >= 0)
                        groupElement.value = resolved;
                }
                if (tp.Index.HasValue)
                {
                    int clamped = ClampIndex(groupElement.choices, tp.Index.Value);
                    groupElement.value = clamped;
                }
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is RadioButtonGroup groupElement
                && prev is RadioButtonGroupProps tp
                && next is RadioButtonGroupProps tn
            )
            {
                if (!ReferenceEquals(tp.Choices, tn.Choices) && tn.Choices != null)
                    groupElement.choices = tn.Choices;
                if (tp.Value != tn.Value && tn.Value != null)
                {
                    int resolved = ResolveIndex(groupElement.choices, tn.Value);
                    if (resolved >= 0)
                        groupElement.value = resolved;
                }
                if (tp.Index != tn.Index && tn.Index.HasValue)
                {
                    int clamped = ClampIndex(groupElement.choices, tn.Index.Value);
                    groupElement.value = clamped;
                }
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
            }
            base.ApplyTypedDiff(element, prev, next);
        }

        private static void ApplySlots(
            RadioButtonGroup groupElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(groupElement, properties, "contentContainer");
        }

        private static void DiffSlot(
            RadioButtonGroup groupElement,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            string slotKey
        )
        {
            object previousSlot = null;
            object nextSlot = null;
            if (previous != null)
            {
                previous.TryGetValue(slotKey, out previousSlot);
            }
            if (next != null)
            {
                next.TryGetValue(slotKey, out nextSlot);
            }
            if (!ReferenceEquals(previousSlot, nextSlot))
            {
                ApplySlot(groupElement, next, slotKey);
            }
        }

        private static void ApplySlot(
            RadioButtonGroup groupElement,
            IReadOnlyDictionary<string, object> properties,
            string slotKey
        )
        {
            if (properties == null)
            {
                return;
            }
            if (!properties.TryGetValue(slotKey, out object slotObject))
            {
                return;
            }
            if (slotObject is not Dictionary<string, object> slotMap)
            {
                return;
            }
            VisualElement target = ResolveSlotElement(groupElement, slotKey);
            if (target == null)
            {
                return;
            }
            if (
                slotMap.TryGetValue("style", out object styleObject)
                && styleObject is IDictionary<string, object> styleMap
            )
            {
                PropsApplier.Apply(
                    target,
                    new Dictionary<string, object> { { "style", styleMap } }
                );
            }
            foreach (KeyValuePair<string, object> entry in slotMap)
            {
                if (entry.Key == "style")
                {
                    continue;
                }
                PropsApplier.Apply(
                    target,
                    new Dictionary<string, object> { { entry.Key, entry.Value } }
                );
            }
        }

        private static VisualElement ResolveSlotElement(
            RadioButtonGroup groupElement,
            string slotKey
        )
        {
            if (!cache.TryGetValue(groupElement, out CachedParts parts))
            {
                parts = new CachedParts();
                cache.Add(groupElement, parts);
            }
            if (slotKey == "contentContainer")
            {
                if (parts.ContentContainer == null)
                {
                    parts.ContentContainer = groupElement.contentContainer;
                }
                return parts.ContentContainer;
            }
            return null;
        }

        private static int ResolveIndex(IEnumerable<string> choices, string value)
        {
            if (choices == null || value == null)
            {
                return -1;
            }
            int i = 0;
            foreach (string choice in choices)
            {
                if (choice == value)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        private static int ClampIndex(IEnumerable<string> choices, int idx)
        {
            if (choices == null)
            {
                return idx < 0 ? 0 : idx;
            }
            int count = 0;
            using (var e = choices.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    count++;
                }
            }
            if (count == 0)
            {
                return 0;
            }
            if (idx < 0)
            {
                return 0;
            }
            if (idx >= count)
            {
                return count - 1;
            }
            return idx;
        }
    }
}
