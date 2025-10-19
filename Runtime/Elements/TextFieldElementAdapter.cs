using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class TextFieldElementAdapter : IElementAdapter
    {
        public VisualElement Create()
        {
            return new TextField();
        }

        public void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is TextField textFieldElement && properties != null)
            {
                ApplyValue(textFieldElement, properties);
                ApplyBoolean(textFieldElement, properties, "multiline", v => textFieldElement.multiline = v);
                ApplyBoolean(textFieldElement, properties, "password", v => SetPasswordField(textFieldElement, v));
                ApplyBoolean(textFieldElement, properties, "readOnly", v => textFieldElement.isReadOnly = v);
                ApplyInt(textFieldElement, properties, "maxLength", v => textFieldElement.maxLength = v);
                ApplyPlaceholderVariants(textFieldElement, properties);
                ApplyHidePlaceholderVariants(textFieldElement, properties);
                ApplySlotVariants(textFieldElement, properties);
            }
            PropsApplier.Apply(element, properties);
        }

        public void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is TextField textFieldElement)
            {
                if (previous == null)
                {
                    previous = new Dictionary<string, object>();
                }
                if (next == null)
                {
                    next = new Dictionary<string, object>();
                }
                DiffValue(textFieldElement, previous, next);
                DiffBoolean(textFieldElement, previous, next, "multiline", v => textFieldElement.multiline = v);
                DiffBoolean(textFieldElement, previous, next, "password", v => SetPasswordField(textFieldElement, v));
                DiffBoolean(textFieldElement, previous, next, "readOnly", v => textFieldElement.isReadOnly = v);
                DiffInt(textFieldElement, previous, next, "maxLength", v => textFieldElement.maxLength = v, textFieldElement.maxLength);
                DiffPlaceholder(textFieldElement, previous, next);
                DiffHidePlaceholder(textFieldElement, previous, next);
                DiffSlot(textFieldElement, previous, next, "label");
                DiffSlot(textFieldElement, previous, next, "input");
                DiffSlot(textFieldElement, previous, next, "textElement");
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplyValue(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            if (properties.TryGetValue("value", out var valueObject) && valueObject is string valueString)
            {
                textFieldElement.value = valueString;
            }
        }

        private static void DiffValue(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            string previousValue = previous.TryGetValue("value", out var p) && p is string ps ? ps : null;
            string nextValue = next.TryGetValue("value", out var n) && n is string ns ? ns : null;
            if (previousValue != nextValue)
            {
                textFieldElement.value = nextValue ?? string.Empty;
            }
        }

        private static void ApplyPlaceholderVariants(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            string placeholder = GetPlaceholderValue(properties);
            if (!string.IsNullOrEmpty(placeholder))
            {
                SetTextEditionPlaceholder(textFieldElement, placeholder);
            }
        }

        private static void DiffPlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            string previousPlaceholder = GetPlaceholderValue(previous);
            string nextPlaceholder = GetPlaceholderValue(next);
            if (previousPlaceholder != nextPlaceholder)
            {
                SetTextEditionPlaceholder(textFieldElement, nextPlaceholder ?? string.Empty);
            }
        }

        private static void ApplyHidePlaceholderVariants(TextField textFieldElement, IReadOnlyDictionary<string, object> properties)
        {
            bool? hide = GetHidePlaceholderValue(properties);
            if (hide.HasValue)
            {
                SetTextEditionHideOnFocus(textFieldElement, hide.Value);
            }
        }

        private static void DiffHidePlaceholder(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            bool? previousHide = GetHidePlaceholderValue(previous);
            bool? nextHide = GetHidePlaceholderValue(next);
            if (previousHide != nextHide)
            {
                SetTextEditionHideOnFocus(textFieldElement, nextHide ?? false);
            }
        }

        private static string GetPlaceholderValue(IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null)
            {
                return null;
            }
            if (properties.TryGetValue("placeholder", out var direct) && direct is string directString)
            {
                return directString;
            }
            if (properties.TryGetValue("placeholderText", out var camel) && camel is string camelString)
            {
                return camelString;
            }
            if (properties.TryGetValue("placeholder-text", out var dashed) && dashed is string dashedString)
            {
                return dashedString;
            }
            return null;
        }

        private static bool? GetHidePlaceholderValue(IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null)
            {
                return null;
            }
            if (properties.TryGetValue("hidePlaceholderOnFocus", out var camel) && camel is bool camelBool)
            {
                return camelBool;
            }
            if (properties.TryGetValue("hide-placeholder-on-focus", out var dashed) && dashed is bool dashedBool)
            {
                return dashedBool;
            }
            return null;
        }

        private static void ApplyBoolean(TextField textFieldElement, IReadOnlyDictionary<string, object> properties, string key, System.Action<bool> apply)
        {
            if (properties.TryGetValue(key, out var valueObject) && valueObject is bool boolValue)
            {
                apply(boolValue);
            }
        }

        private static void DiffBoolean(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string key, System.Action<bool> apply)
        {
            bool previousValue = previous.TryGetValue(key, out var prevObj) && prevObj is bool prevBool && prevBool;
            bool nextValue = next.TryGetValue(key, out var nextObj) && nextObj is bool nextBool && nextBool;
            if (previousValue != nextValue)
            {
                apply(nextValue);
            }
        }

        private static void ApplyInt(TextField textFieldElement, IReadOnlyDictionary<string, object> properties, string key, System.Action<int> apply)
        {
            if (properties.TryGetValue(key, out var valueObject) && valueObject is int intValue)
            {
                apply(intValue);
            }
        }

        private static void DiffInt(TextField textFieldElement, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string key, System.Action<int> apply, int fallbackValue)
        {
            int previousValue = previous.TryGetValue(key, out var prevObj) && prevObj is int prevInt ? prevInt : fallbackValue;
            int nextValue = next.TryGetValue(key, out var nextObj) && nextObj is int nextInt ? nextInt : previousValue;
            if (previousValue != nextValue)
            {
                apply(nextValue);
            }
        }

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
                    var labelInstance = labelProperty.GetValue(textFieldElement) as VisualElement;
                    if (labelInstance != null)
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
