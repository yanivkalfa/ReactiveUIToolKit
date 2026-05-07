using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ToggleElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public Label Label;
            public VisualElement Input;
            public VisualElement Checkmark;
        }

        private static readonly ConditionalWeakTable<Toggle, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return new Toggle();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Toggle toggleElement && properties != null)
            {
                TryApplyProp<bool>(
                    properties,
                    "value",
                    value =>
                    {
                        toggleElement.value = value;
                    }
                );
                TryApplyProp<string>(
                    properties,
                    "text",
                    value =>
                    {
                        toggleElement.text = value ?? string.Empty;
                    }
                );
                ApplySlots(toggleElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Toggle toggleElement)
            {
                TryDiffProp<bool>(
                    previous,
                    next,
                    "value",
                    value =>
                    {
                        toggleElement.value = value;
                    }
                );
                TryDiffProp<string>(
                    previous,
                    next,
                    "text",
                    value =>
                    {
                        toggleElement.text = value ?? string.Empty;
                    }
                );
                if (
                    ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.EnableDiffTracing
                    && ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.CurrentTraceLevel
                        != ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.TraceLevel.None
                )
                {
                    UnityEngine.Debug.Log(
                        $"[ToggleDiff] key={(element.userData as ReactiveUITK.Core.NodeMetadata)?.Key} value={toggleElement.value}"
                    );
                }
                DiffSlot(toggleElement, previous, next, "label");
                DiffSlot(toggleElement, previous, next, "input");
                DiffSlot(toggleElement, previous, next, "checkmark");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySlots(
            Toggle toggleElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(toggleElement, properties, "label");
            ApplySlot(toggleElement, properties, "input");
            ApplySlot(toggleElement, properties, "checkmark");
        }

        private static void DiffSlot(
            Toggle toggleElement,
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
                ApplySlot(toggleElement, next, slotKey);
            }
        }

        private static void ApplySlot(
            Toggle toggleElement,
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
            VisualElement target = ResolveSlotElement(toggleElement, slotKey);
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

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Toggle toggle && props is ToggleProps tp)
            {
                if (tp.Value.HasValue)
                    toggle.value = tp.Value.Value;
                if (tp.Text != null)
                    toggle.text = tp.Text;
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.Label != null)
                {
                    var labelEl = ResolveSlotElement(toggle, "label");
                    if (labelEl != null)
                        PropsApplier.Apply(labelEl, tp.Label);
                }
                if (tp.Input != null)
                {
                    var inputEl = ResolveSlotElement(toggle, "input");
                    if (inputEl != null)
                        PropsApplier.Apply(inputEl, tp.Input);
                }
                if (tp.Checkmark != null)
                {
                    var checkmarkEl = ResolveSlotElement(toggle, "checkmark");
                    if (checkmarkEl != null)
                        PropsApplier.Apply(checkmarkEl, tp.Checkmark);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Toggle toggle && prev is ToggleProps tp && next is ToggleProps tn)
            {
                if (tp.Value != tn.Value && tn.Value.HasValue)
                    toggle.value = tn.Value.Value;
                if (tp.Text != tn.Text)
                    toggle.text = tn.Text ?? string.Empty;
                if (tp.OnChange != tn.OnChange)
                {
                    if (tn.OnChange != null)
                        PropsApplier.ApplySingle(element, tp.OnChange, "onChange", tn.OnChange);
                    else if (tp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", tp.OnChange);
                }
                if (!ReferenceEquals(tp.Label, tn.Label))
                {
                    var labelEl = ResolveSlotElement(toggle, "label");
                    if (labelEl != null)
                    {
                        if (tp.Label != null && tn.Label != null)
                            PropsApplier.ApplyDiff(labelEl, tp.Label, tn.Label);
                        else if (tn.Label != null)
                            PropsApplier.Apply(labelEl, tn.Label);
                    }
                }
                if (!ReferenceEquals(tp.Input, tn.Input))
                {
                    var inputEl = ResolveSlotElement(toggle, "input");
                    if (inputEl != null)
                    {
                        if (tp.Input != null && tn.Input != null)
                            PropsApplier.ApplyDiff(inputEl, tp.Input, tn.Input);
                        else if (tn.Input != null)
                            PropsApplier.Apply(inputEl, tn.Input);
                    }
                }
                if (!ReferenceEquals(tp.Checkmark, tn.Checkmark))
                {
                    var checkmarkEl = ResolveSlotElement(toggle, "checkmark");
                    if (checkmarkEl != null)
                    {
                        if (tp.Checkmark != null && tn.Checkmark != null)
                            PropsApplier.ApplyDiff(checkmarkEl, tp.Checkmark, tn.Checkmark);
                        else if (tn.Checkmark != null)
                            PropsApplier.Apply(checkmarkEl, tn.Checkmark);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }

        private static VisualElement ResolveSlotElement(Toggle toggleElement, string slotKey)
        {
            if (!cache.TryGetValue(toggleElement, out CachedParts parts))
            {
                parts = new CachedParts();
                cache.Add(toggleElement, parts);
            }
            if (slotKey == "label")
            {
                if (parts.Label == null)
                {
                    parts.Label = toggleElement.Q<Label>();
                }
                return parts.Label;
            }
            if (slotKey == "input")
            {
                if (parts.Input == null)
                {
                    parts.Input = toggleElement.Q(className: "unity-toggle__input");
                    if (parts.Input == null)
                    {
                        parts.Input = toggleElement.Q(className: "unity-base-field__input");
                    }
                }
                return parts.Input;
            }
            if (slotKey == "checkmark")
            {
                if (parts.Checkmark == null)
                {
                    parts.Checkmark = toggleElement.Q(className: "unity-toggle__checkmark");
                }
                return parts.Checkmark;
            }
            return null;
        }
    }
}
