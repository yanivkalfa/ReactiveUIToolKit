using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class ProgressBarElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public VisualElement Progress;
            public Label Title;
        }
        private static readonly ConditionalWeakTable<ProgressBar, CachedParts> cache = new();
        public override VisualElement Create()
        {
            ProgressBar progressBar = new();
            return progressBar;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is ProgressBar progressBarElement && properties != null)
            {
                TryApplyProp<float>(properties, "value", value => { progressBarElement.value = value; });
                TryApplyProp<int>(properties, "intValue", value => { progressBarElement.value = value; });
                TryApplyProp<string>(properties, "title", value => { progressBarElement.title = value ?? string.Empty; });
                ApplySlots(progressBarElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ProgressBar progressBarElement)
            {
                TryDiffProp<float>(previous, next, "value", value => { progressBarElement.value = value; });
                TryDiffProp<int>(previous, next, "intValue", value => { progressBarElement.value = value; });
                TryDiffProp<string>(previous, next, "title", value => { progressBarElement.title = value ?? string.Empty; });
                DiffSlot(progressBarElement, previous, next, "progress");
                DiffSlot(progressBarElement, previous, next, "titleElement");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(ProgressBar progressBarElement, IReadOnlyDictionary<string, object> properties)
        {
            ApplySlot(progressBarElement, properties, "progress");
            ApplySlot(progressBarElement, properties, "titleElement");
        }

        private static void DiffSlot(ProgressBar progressBarElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string slotKey)
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
                ApplySlot(progressBarElement, next, slotKey);
            }
        }

        private static void ApplySlot(ProgressBar progressBarElement, IReadOnlyDictionary<string, object> properties, string slotKey)
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
            VisualElement target = ResolveSlotElement(progressBarElement, slotKey);
            if (target == null)
            {
                return;
            }
            if (slotMap.TryGetValue("style", out object styleObject) && styleObject is IDictionary<string, object> styleMap)
            {
                PropsApplier.Apply(target, new Dictionary<string, object> { { "style", styleMap } });
            }
            foreach (KeyValuePair<string, object> entry in slotMap)
            {
                if (entry.Key == "style")
                {
                    continue;
                }
                PropsApplier.Apply(target, new Dictionary<string, object> { { entry.Key, entry.Value } });
            }
        }

        private static VisualElement ResolveSlotElement(ProgressBar progressBarElement, string slotKey)
        {
            if (!cache.TryGetValue(progressBarElement, out CachedParts parts))
            {
                parts = new CachedParts();
                cache.Add(progressBarElement, parts);
            }
            if (slotKey == "progress")
            {
                if (parts.Progress == null)
                {
                    parts.Progress = progressBarElement.Q(className: "unity-progress-bar__progress");
                }
                return parts.Progress;
            }
            if (slotKey == "titleElement")
            {
                if (parts.Title == null)
                {
                    parts.Title = progressBarElement.Q<Label>();
                }
                return parts.Title;
            }
            return null;
        }
    }
}
