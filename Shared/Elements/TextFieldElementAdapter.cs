using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TextFieldElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public VisualElement Label;
            public VisualElement Input;
            public TextElement TextElem;
        }

        private static readonly ConditionalWeakTable<TextField, CachedParts> cache = new();

        public override VisualElement Create()
        {
            return new TextField();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TextField textFieldElement || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<string>(
                properties,
                "value",
                v =>
                {
                    textFieldElement.value = v;
                }
            );
            TryApplyProp<bool>(
                properties,
                "multiline",
                v =>
                {
                    textFieldElement.multiline = v;
                }
            );
            TryApplyProp<bool>(
                properties,
                "password",
                v =>
                {
                    SetPasswordField(textFieldElement, v);
                }
            );
            TryApplyProp<bool>(
                properties,
                "readOnly",
                v =>
                {
                    textFieldElement.isReadOnly = v;
                }
            );
            TryApplyProp<int>(
                properties,
                "maxLength",
                v =>
                {
                    textFieldElement.maxLength = v;
                }
            );
            ApplyPlaceholder(textFieldElement, properties);
            ApplyHidePlaceholder(textFieldElement, properties);
            ApplySlots(textFieldElement, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TextField textFieldElement)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<string>(
                previous,
                next,
                "value",
                v =>
                {
                    textFieldElement.value = v ?? string.Empty;
                }
            );
            TryDiffProp<bool>(
                previous,
                next,
                "multiline",
                v =>
                {
                    textFieldElement.multiline = v;
                }
            );
            TryDiffProp<bool>(
                previous,
                next,
                "password",
                v =>
                {
                    SetPasswordField(textFieldElement, v);
                }
            );
            TryDiffProp<bool>(
                previous,
                next,
                "readOnly",
                v =>
                {
                    textFieldElement.isReadOnly = v;
                }
            );
            TryDiffProp<int>(
                previous,
                next,
                "maxLength",
                v =>
                {
                    textFieldElement.maxLength = v;
                }
            );
            DiffPlaceholder(textFieldElement, previous, next);
            DiffHidePlaceholder(textFieldElement, previous, next);
            DiffSlot(textFieldElement, previous, next, "label");
            DiffSlot(textFieldElement, previous, next, "input");
            DiffSlot(textFieldElement, previous, next, "textElement");
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is TextField tf && props is TextFieldProps tp)
            {
                if (tp.Value != null)
                    tf.value = tp.Value;
                if (tp.Multiline.HasValue)
                    tf.multiline = tp.Multiline.Value;
                if (tp.Password.HasValue)
                    SetPasswordField(tf, tp.Password.Value);
                if (tp.ReadOnly.HasValue)
                    tf.isReadOnly = tp.ReadOnly.Value;
                if (tp.MaxLength.HasValue)
                    tf.maxLength = tp.MaxLength.Value;
                if (tp.Placeholder != null)
                {
                    try
                    {
                        tf.textEdition.placeholder = tp.Placeholder;
                    }
                    catch
                    {
                        SetTextEditionPlaceholder(tf, tp.Placeholder);
                    }
                }
                if (tp.HidePlaceholderOnFocus.HasValue)
                {
                    try
                    {
                        tf.textEdition.hidePlaceholderOnFocus = tp.HidePlaceholderOnFocus.Value;
                    }
                    catch
                    {
                        SetTextEditionHideOnFocus(tf, tp.HidePlaceholderOnFocus.Value);
                    }
                }
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
                if (tp.Label is Dictionary<string, object> labelMap)
                {
                    var labelEl = ResolveSlotElement(tf, "label");
                    if (labelEl != null)
                        PropsApplier.Apply(labelEl, labelMap);
                }
                if (tp.Input is Dictionary<string, object> inputMap)
                {
                    var inputEl = ResolveSlotElement(tf, "input");
                    if (inputEl != null)
                        PropsApplier.Apply(inputEl, inputMap);
                }
                if (tp.TextElement is Dictionary<string, object> textElemMap)
                {
                    var textEl = ResolveSlotElement(tf, "textElement");
                    if (textEl != null)
                        PropsApplier.Apply(textEl, textElemMap);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is TextField tf && prev is TextFieldProps tp && next is TextFieldProps tn)
            {
                if (tp.Value != tn.Value)
                    tf.value = tn.Value ?? string.Empty;
                if (tp.Multiline != tn.Multiline && tn.Multiline.HasValue)
                    tf.multiline = tn.Multiline.Value;
                if (tp.Password != tn.Password && tn.Password.HasValue)
                    SetPasswordField(tf, tn.Password.Value);
                if (tp.ReadOnly != tn.ReadOnly && tn.ReadOnly.HasValue)
                    tf.isReadOnly = tn.ReadOnly.Value;
                if (tp.MaxLength != tn.MaxLength && tn.MaxLength.HasValue)
                    tf.maxLength = tn.MaxLength.Value;
                if (tp.Placeholder != tn.Placeholder)
                {
                    string value = tn.Placeholder ?? string.Empty;
                    try
                    {
                        tf.textEdition.placeholder = value;
                    }
                    catch
                    {
                        SetTextEditionPlaceholder(tf, value);
                    }
                }
                if (tp.HidePlaceholderOnFocus != tn.HidePlaceholderOnFocus)
                {
                    bool value = tn.HidePlaceholderOnFocus ?? false;
                    try
                    {
                        tf.textEdition.hidePlaceholderOnFocus = value;
                    }
                    catch
                    {
                        SetTextEditionHideOnFocus(tf, value);
                    }
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
                if (!ReferenceEquals(tp.Label, tn.Label))
                {
                    var labelEl = ResolveSlotElement(tf, "label");
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
                    var inputEl = ResolveSlotElement(tf, "input");
                    if (inputEl != null)
                    {
                        if (tp.Input != null && tn.Input != null)
                            PropsApplier.ApplyDiff(inputEl, tp.Input, tn.Input);
                        else if (tn.Input != null)
                            PropsApplier.Apply(inputEl, tn.Input);
                    }
                }
                if (!ReferenceEquals(tp.TextElement, tn.TextElement))
                {
                    var textEl = ResolveSlotElement(tf, "textElement");
                    if (textEl != null)
                    {
                        if (tp.TextElement != null && tn.TextElement != null)
                            PropsApplier.ApplyDiff(textEl, tp.TextElement, tn.TextElement);
                        else if (tn.TextElement != null)
                            PropsApplier.Apply(textEl, tn.TextElement);
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }

        private static void ApplyPlaceholder(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            if (
                properties.TryGetValue("placeholder", out var placeholderObj)
                && placeholderObj is string placeholder
            )
            {
                try
                {
                    textFieldElement.textEdition.placeholder = placeholder;
                }
                catch
                {
                    SetTextEditionPlaceholder(textFieldElement, placeholder);
                }
            }
        }

        private static void DiffPlaceholder(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            string previousPlaceholder = null;
            if (
                previous != null
                && previous.TryGetValue("placeholder", out var prevObj)
                && prevObj is string prevPlaceholder
            )
            {
                previousPlaceholder = prevPlaceholder;
            }
            string nextPlaceholder = null;
            if (
                next != null
                && next.TryGetValue("placeholder", out var nextObj)
                && nextObj is string nextPlaceholderStr
            )
            {
                nextPlaceholder = nextPlaceholderStr;
            }
            if (previousPlaceholder != nextPlaceholder)
            {
                string value = nextPlaceholder ?? string.Empty;
                try
                {
                    textFieldElement.textEdition.placeholder = value;
                }
                catch
                {
                    SetTextEditionPlaceholder(textFieldElement, value);
                }
            }
        }

        private static void ApplyHidePlaceholder(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            if (
                properties.TryGetValue("hidePlaceholderOnFocus", out var hideObj)
                && hideObj is bool hide
            )
            {
                try
                {
                    textFieldElement.textEdition.hidePlaceholderOnFocus = hide;
                }
                catch
                {
                    SetTextEditionHideOnFocus(textFieldElement, hide);
                }
            }
        }

        private static void DiffHidePlaceholder(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            bool? previousHide = null;
            if (
                previous != null
                && previous.TryGetValue("hidePlaceholderOnFocus", out var prevObj)
                && prevObj is bool prevHide
            )
            {
                previousHide = prevHide;
            }
            bool? nextHide = null;
            if (
                next != null
                && next.TryGetValue("hidePlaceholderOnFocus", out var nextObj)
                && nextObj is bool nextHideVal
            )
            {
                nextHide = nextHideVal;
            }
            if (previousHide != nextHide)
            {
                bool value = nextHide ?? false;
                try
                {
                    textFieldElement.textEdition.hidePlaceholderOnFocus = value;
                }
                catch
                {
                    SetTextEditionHideOnFocus(textFieldElement, value);
                }
            }
        }

        private static void SetPasswordField(TextField textFieldElement, bool enabled)
        {
            var propertyInfo = typeof(TextField).GetProperty("isPasswordField");
            if (
                propertyInfo != null
                && propertyInfo.PropertyType == typeof(bool)
                && propertyInfo.CanWrite
            )
            {
                propertyInfo.SetValue(textFieldElement, enabled, null);
            }
        }

        private static bool SetTextEditionPlaceholder(
            TextField textFieldElement,
            string placeholderString
        )
        {
            var flags =
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.FlattenHierarchy;
            object editionInstance = null;
            var editionProperty = typeof(TextField).GetProperty("textEdition", flags);
            if (editionProperty != null)
            {
                editionInstance = editionProperty.GetValue(textFieldElement);
            }
            else
            {
                var editionField = typeof(TextField).GetField("textEdition", flags);
                if (editionField != null)
                {
                    editionInstance = editionField.GetValue(textFieldElement);
                }
            }
            if (editionInstance == null)
            {
                return false;
            }
            var placeholderProperty = editionInstance.GetType().GetProperty("placeholder", flags);
            if (
                placeholderProperty != null
                && placeholderProperty.PropertyType == typeof(string)
                && placeholderProperty.CanWrite
            )
            {
                placeholderProperty.SetValue(editionInstance, placeholderString, null);
                return true;
            }
            var placeholderField = editionInstance.GetType().GetField("placeholder", flags);
            if (placeholderField != null && placeholderField.FieldType == typeof(string))
            {
                placeholderField.SetValue(editionInstance, placeholderString);
                return true;
            }
            return false;
        }

        private static bool SetTextEditionHideOnFocus(TextField textFieldElement, bool hide)
        {
            var flags =
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.FlattenHierarchy;
            object editionInstance = null;
            var editionProperty = typeof(TextField).GetProperty("textEdition", flags);
            if (editionProperty != null)
            {
                editionInstance = editionProperty.GetValue(textFieldElement);
            }
            else
            {
                var editionField = typeof(TextField).GetField("textEdition", flags);
                if (editionField != null)
                {
                    editionInstance = editionField.GetValue(textFieldElement);
                }
            }
            if (editionInstance == null)
            {
                return false;
            }
            var hideProperty = editionInstance
                .GetType()
                .GetProperty("hidePlaceholderOnFocus", flags);
            if (
                hideProperty != null
                && hideProperty.PropertyType == typeof(bool)
                && hideProperty.CanWrite
            )
            {
                hideProperty.SetValue(editionInstance, hide, null);
                return true;
            }
            var hideField = editionInstance.GetType().GetField("hidePlaceholderOnFocus", flags);
            if (hideField != null && hideField.FieldType == typeof(bool))
            {
                hideField.SetValue(editionInstance, hide);
                return true;
            }
            return false;
        }

        private static void ApplySlots(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> properties
        )
        {
            ApplySlot(textFieldElement, properties, "label");
            ApplySlot(textFieldElement, properties, "input");
            ApplySlot(textFieldElement, properties, "textElement");
        }

        private static void DiffSlot(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            string slotKey
        )
        {
            object previousSlot;
            object nextSlot;
            previous.TryGetValue(slotKey, out previousSlot);
            next.TryGetValue(slotKey, out nextSlot);
            if (!ReferenceEquals(previousSlot, nextSlot))
            {
                ApplySlot(textFieldElement, next, slotKey);
            }
        }

        private static void ApplySlot(
            TextField textFieldElement,
            IReadOnlyDictionary<string, object> properties,
            string slotKey
        )
        {
            if (properties == null)
            {
                return;
            }
            if (
                !properties.TryGetValue(slotKey, out var slotObject)
                || slotObject is not Dictionary<string, object> slotMap
            )
            {
                return;
            }
            VisualElement target = ResolveSlotElement(textFieldElement, slotKey);
            if (target == null)
            {
                return;
            }
            if (
                slotMap.TryGetValue("style", out var styleObj)
                && styleObj is IDictionary<string, object> styleMap
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

        private static VisualElement ResolveSlotElement(TextField textFieldElement, string slotKey)
        {
            if (!cache.TryGetValue(textFieldElement, out var parts))
            {
                parts = new CachedParts();
                cache.Add(textFieldElement, parts);
            }
            if (slotKey == "label")
            {
                if (parts.Label == null)
                {
                    var labelProperty = typeof(TextField).GetProperty("labelElement");
                    if (labelProperty != null)
                    {
                        if (labelProperty.GetValue(textFieldElement) is VisualElement labelInstance)
                        {
                            parts.Label = labelInstance;
                        }
                    }
                    if (parts.Label == null)
                    {
                        parts.Label = textFieldElement.Q<Label>();
                    }
                }
                return parts.Label;
            }
            if (slotKey == "input")
            {
                if (parts.Input == null)
                {
                    parts.Input = textFieldElement.Q(className: "unity-text-input");
                }
                return parts.Input;
            }
            if (slotKey == "textElement")
            {
                if (parts.TextElem == null)
                {
                    parts.TextElem = textFieldElement.Q<TextElement>();
                }
                return parts.TextElem;
            }
            return null;
        }
    }
}
