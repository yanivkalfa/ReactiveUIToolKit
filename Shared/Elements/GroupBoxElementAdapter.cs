using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class GroupBoxElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public VisualElement ContentContainer;
            public Label Label;
        }

        private static readonly ConditionalWeakTable<GroupBox, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return new GroupBox();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is GroupBox groupBoxElement && properties != null)
            {
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        groupBoxElement.text = value ?? string.Empty;
                    }
                );
                ApplySlots(groupBoxElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is GroupBox groupBoxElement)
            {
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        groupBoxElement.text = value ?? string.Empty;
                    }
                );
                DiffSlot(groupBoxElement, previous, next, "contentContainer");
                DiffSlot(groupBoxElement, previous, next, "label");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            GroupBox groupBoxElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(groupBoxElement, properties, "contentContainer");
            ApplySlot(groupBoxElement, properties, "label");
        }

        private static void DiffSlot(
            GroupBox groupBoxElement,
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
                ApplySlot(groupBoxElement, next, slotKey);
            }
        }

        private static void ApplySlot(
            GroupBox groupBoxElement,
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
            VisualElement target = ResolveSlotElement(groupBoxElement, slotKey);
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

        private static VisualElement ResolveSlotElement(GroupBox groupBoxElement, string slotKey)
        {
            if (!cache.TryGetValue(groupBoxElement, out CachedParts parts))
            {
                parts = new CachedParts();
                cache.Add(groupBoxElement, parts);
            }
            if (slotKey == "contentContainer")
            {
                if (parts.ContentContainer == null)
                {
                    parts.ContentContainer = groupBoxElement.contentContainer;
                }
                return parts.ContentContainer;
            }
            if (slotKey == "label")
            {
                if (parts.Label == null)
                {
                    parts.Label = groupBoxElement.Q<Label>();
                }
                return parts.Label;
            }
            return null;
        }

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            var gb = element as GroupBox;
            var cc = (gb != null) ? gb.contentContainer : element;
            return EnsureMount(cc);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is GroupBox gb && props is GroupBoxProps gp)
            {
                if (gp.Text != null)
                    gb.text = gp.Text;
                if (gp.Label is Dictionary<string, object> labelMap)
                {
                    var labelEl = ResolveSlotElement(gb, "label");
                    if (labelEl != null)
                        PropsApplier.Apply(labelEl, labelMap);
                }
                if (props.ContentContainer is Dictionary<string, object> ccMap)
                {
                    var ccEl = ResolveSlotElement(gb, "contentContainer");
                    if (ccEl != null)
                        PropsApplier.Apply(ccEl, ccMap);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is GroupBox gb && prev is GroupBoxProps gp && next is GroupBoxProps gn)
            {
                if (gp.Text != gn.Text)
                    gb.text = gn.Text ?? string.Empty;
                if (
                    !ReferenceEquals(gp.Label, gn.Label)
                    && gn.Label is Dictionary<string, object> labelMap
                )
                {
                    var labelEl = ResolveSlotElement(gb, "label");
                    if (labelEl != null)
                        PropsApplier.Apply(labelEl, labelMap);
                }
                if (
                    !ReferenceEquals(prev.ContentContainer, next.ContentContainer)
                    && next.ContentContainer is Dictionary<string, object> ccMap
                )
                {
                    var ccEl = ResolveSlotElement(gb, "contentContainer");
                    if (ccEl != null)
                        PropsApplier.Apply(ccEl, ccMap);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
