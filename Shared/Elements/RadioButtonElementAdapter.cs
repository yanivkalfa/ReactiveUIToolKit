using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class RadioButtonElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public Label Label;
        }

        private static readonly ConditionalWeakTable<RadioButton, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<RadioButton>();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is RadioButton radioButtonElement && properties != null)
            {
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        radioButtonElement.text = value ?? string.Empty;
                    }
                );
                TryApplyProp<bool>(
                    properties,
                    "value",
                    value =>
                    {
                        radioButtonElement.value = value;
                    }
                );
                ApplySlots(radioButtonElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is RadioButton radioButtonElement)
            {
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        radioButtonElement.text = value ?? string.Empty;
                    }
                );
                TryDiffProp<bool>(
                    previous,
                    next,
                    "value",
                    value =>
                    {
                        radioButtonElement.value = value;
                    }
                );
                if (
                    ReactiveUITK.Core.Reconciler.EnableDiffTracing
                    && ReactiveUITK.Core.Reconciler.TraceLevel
                        != ReactiveUITK.Core.Reconciler.DiffTraceLevel.None
                )
                {
                    UnityEngine.Debug.Log(
                        $"[RadioButtonDiff] key={(element.userData as ReactiveUITK.Core.NodeMetadata)?.Key} value={radioButtonElement.value}"
                    );
                }
                DiffSlot(radioButtonElement, previous, next, "label");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            RadioButton radioButtonElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(radioButtonElement, properties, "label");
        }

        private static void DiffSlot(
            RadioButton radioButtonElement,
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
                ApplySlot(radioButtonElement, next, slotKey);
            }
        }

        private static void ApplySlot(
            RadioButton radioButtonElement,
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
            VisualElement target = ResolveSlotElement(radioButtonElement, slotKey);
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
            RadioButton radioButtonElement,
            string slotKey
        )
        {
            if (!cache.TryGetValue(radioButtonElement, out CachedParts parts))
            {
                parts = new CachedParts();
                cache.Add(radioButtonElement, parts);
            }
            if (slotKey == "label")
            {
                if (parts.Label == null)
                {
                    parts.Label = radioButtonElement.Q<Label>();
                }
                return parts.Label;
            }
            return null;
        }
    }
}
