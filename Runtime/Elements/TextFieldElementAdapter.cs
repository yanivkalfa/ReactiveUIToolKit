using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class TextFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new TextField();
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (!(element is TextField textFieldElement) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<string>(properties, "value", v => textFieldElement.value = v);
            TryApplyProp<bool>(properties, "multiline", v => textFieldElement.multiline = v);
            TryApplyProp<bool>(properties, "password", v => SetPasswordField(textFieldElement, v));
            TryApplyProp<bool>(properties, "readOnly", v => textFieldElement.isReadOnly = v);
            TryApplyProp<int>(properties, "maxLength", v => textFieldElement.maxLength = v);
            ApplyPlaceholder(textFieldElement, properties);
            ApplyHidePlaceholder(textFieldElement, properties);
            ApplySlotVariants(textFieldElement, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (!(element is TextField textFieldElement))
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            TryDiffProp<string>(previous, next, "value", v => textFieldElement.value = v ?? string.Empty);
            TryDiffProp<bool>(previous, next, "multiline", v => textFieldElement.multiline = v);
            TryDiffProp<bool>(previous, next, "password", v => SetPasswordField(textFieldElement, v));
            TryDiffProp<bool>(previous, next, "readOnly", v => textFieldElement.isReadOnly = v);
            TryDiffProp<int>(previous, next, "maxLength", v => textFieldElement.maxLength = v);
            DiffPlaceholder(textFieldElement, previous, next);
            DiffHidePlaceholder(textFieldElement, previous, next);
            DiffSlot(textFieldElement, previous, next, "label");
            DiffSlot(textFieldElement, previous, next, "input");
            DiffSlot(textFieldElement, previous, next, "textElement");
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplyPlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            string resolvedPlaceholder = null;
            if (properties != null)
            {
                if (properties.TryGetValue("placeholder-text", out var placeholderRaw) && placeholderRaw is string placeholderTextValue)
                {
                    resolvedPlaceholder = placeholderTextValue;
                }
            }
            if (!string.IsNullOrEmpty(resolvedPlaceholder))
            {
                textFieldElement.textEdition.placeholder = resolvedPlaceholder;
                textFieldElement.label = string.Empty;
                textFieldElement.textEdition.hidePlaceholderOnFocus = false;
                //email.textEdition.placeholder = "you@example.com";
                //email.textEdition.hidePlaceholderOnFocus = true;
                //SetTextEditionPlaceholder(textFieldElement, resolvedPlaceholder);
            }
        }

        private static void DiffPlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            string previousResolved = null;
            if (previous != null)
            {
                if (previous.TryGetValue("placeholder-text", out var prevRaw) && prevRaw is string prevStr)
                {
                    previousResolved = prevStr;
                }
            }
            string nextResolved = null;
            if (next != null)
            {
                if (next.TryGetValue("placeholder-text", out var nextRaw) && nextRaw is string nextStr)
                {
                    nextResolved = nextStr;
                }
            }
            if (previousResolved != nextResolved)
            {
                SetTextEditionPlaceholder(textFieldElement, nextResolved ?? string.Empty);
            }
        }

        private static void ApplyHidePlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            bool? resolvedHide = null;
            if (properties != null)
            {
                if (properties.TryGetValue("hide-placeholder-on-focus", out var hideRaw) && hideRaw is bool hideBoolValue)
                {
                    resolvedHide = hideBoolValue;
                }
            }
            if (resolvedHide.HasValue)
            {
                SetTextEditionHideOnFocus(textFieldElement, resolvedHide.Value);
            }
        }

        private static void DiffHidePlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            bool? previousResolved = null;
            if (previous != null)
            {
                if (previous.TryGetValue("hide-placeholder-on-focus", out var prevHideRaw) && prevHideRaw is bool prevHideBool)
                {
                    previousResolved = prevHideBool;
                }
            }
            bool? nextResolved = null;
            if (next != null)
            {
                if (next.TryGetValue("hide-placeholder-on-focus", out var nextHideRaw) && nextHideRaw is bool nextHideBool)
                {
                    nextResolved = nextHideBool;
                }
            }
            if (previousResolved != nextResolved)
            {
                SetTextEditionHideOnFocus(textFieldElement, nextResolved ?? false);
            }
        }

        // private static string GetPlaceholderValue(IReadOnlyDictionary<string, object> properties)
        // {
        //     if (properties == null) return null;
        //     if (properties.TryGetValue("placeholder", out var direct) && direct is string directString) return directString;
        //     if (properties.TryGetValue("placeholderText", out var camel) && camel is string camelString) return camelString;
        //     if (properties.TryGetValue("placeholder-text", out var dashed) && dashed is string dashedString) return dashedString;
        //     return null;
        // }

        // private static bool? GetHidePlaceholderValue(IReadOnlyDictionary<string, object> properties)
        // {
        //     if (properties == null) return null;
        //     if (properties.TryGetValue("hidePlaceholderOnFocus", out var camel) && camel is bool camelBool) return camelBool;
        //     if (properties.TryGetValue("hide-placeholder-on-focus", out var dashed) && dashed is bool dashedBool) return dashedBool;
        //     return null;
        // }


        private static void SetPasswordField(TextField textFieldElement, bool enabled)
        {
            var propertyInfo = typeof(TextField).GetProperty("isPasswordField");
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool) && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(textFieldElement, enabled, null);
            }
        }

        private static void SetTextEditionPlaceholder(TextField textFieldElement, string placeholderString)
        {
            var editionProperty = typeof(TextField).GetProperty("textEdition");
            if (editionProperty == null)
            {
                return;
            }
            var editionInstance = editionProperty.GetValue(textFieldElement);
            if (editionInstance == null)
            {
                return;
            }
            var placeholderProperty = editionInstance.GetType().GetProperty("placeholder");
            if (placeholderProperty != null && placeholderProperty.PropertyType == typeof(string) && placeholderProperty.CanWrite)
            {
                placeholderProperty.SetValue(editionInstance, placeholderString, null);
            }
        }

        private static void SetTextEditionHideOnFocus(TextField textFieldElement, bool hide)
        {
            var editionProperty = typeof(TextField).GetProperty("textEdition");
            if (editionProperty == null)
            {
                return;
            }
            var editionInstance = editionProperty.GetValue(textFieldElement);
            if (editionInstance == null)
            {
                return;
            }
            var hideProperty = editionInstance.GetType().GetProperty("hidePlaceholderOnFocus");
            if (hideProperty != null && hideProperty.PropertyType == typeof(bool) && hideProperty.CanWrite)
            {
                hideProperty.SetValue(editionInstance, hide, null);
            }
        }

        private static void ApplySlotVariants(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            ApplySlot(textFieldElement, properties, "label");
            ApplySlot(textFieldElement, properties, "input");
            ApplySlot(textFieldElement, properties, "textElement");
        }

        private static void DiffSlot(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string slotKey)
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

        private static void ApplySlot(TextField textFieldElement, IReadOnlyDictionary<string, object> properties, string slotKey)
        {
            if (properties == null)
            {
                return;
            }
            if (!properties.TryGetValue(slotKey, out var slotObject) || slotObject is not Dictionary<string, object> slotMap)
            {
                return;
            }
            VisualElement target = ResolveSlotElement(textFieldElement, slotKey);
            if (target == null)
            {
                return;
            }
            if (slotMap.TryGetValue("style", out var styleObj) && styleObj is IDictionary<string, object> styleMap)
            {
                PropsApplier.Apply(target, new Dictionary<string, object>{{"style", styleMap}});
            }
            foreach (KeyValuePair<string, object> entry in slotMap)
            {
                if (entry.Key == "style")
                {
                    continue;
                }
                PropsApplier.Apply(target, new Dictionary<string, object>{{entry.Key, entry.Value}});
            }
        }

        private static VisualElement ResolveSlotElement(TextField textFieldElement, string slotKey)
        {
            if (slotKey == "label")
            {
                var labelProperty = typeof(TextField).GetProperty("labelElement");
                if (labelProperty != null)
                {
                    if (labelProperty.GetValue(textFieldElement) is VisualElement labelInstance)
                    {
                        return labelInstance;
                    }
                }
                return textFieldElement.Q<Label>();
            }
            if (slotKey == "input")
            {
                return textFieldElement.Q(className: "unity-text-input");
            }
            if (slotKey == "textElement")
            {
                return textFieldElement.Q<TextElement>();
            }
            return null;
        }
    }
}
