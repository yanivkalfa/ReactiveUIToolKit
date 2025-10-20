using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TextFieldProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Value { get; set; }
        public bool? Multiline { get; set; }
        public bool? Password { get; set; }
        public bool? ReadOnly { get; set; }
        public int? MaxLength { get; set; }
        public string Placeholder { get; set; }
        public bool? HidePlaceholderOnFocus { get; set; }
        public Style Style { get; set; }

        // Nested slot props
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> TextElement { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
            if (Value != null)
            {
                dict["value"] = Value;
            }
            if (Multiline.HasValue)
            {
                dict["multiline"] = Multiline.Value;
            }
            if (Password.HasValue)
            {
                dict["password"] = Password.Value;
            }
            if (ReadOnly.HasValue)
            {
                dict["readOnly"] = ReadOnly.Value;
            }
            if (MaxLength.HasValue)
            {
                dict["maxLength"] = MaxLength.Value;
            }
            if (!string.IsNullOrEmpty(Placeholder))
            {
                dict["placeholder"] = Placeholder;
            }
            if (HidePlaceholderOnFocus.HasValue)
            {
                dict["hidePlaceholderOnFocus"] = HidePlaceholderOnFocus.Value;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (Input != null)
            {
                dict["input"] = Input;
            }
            if (TextElement != null)
            {
                dict["textElement"] = TextElement;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            return dict;
        }
    }
}
