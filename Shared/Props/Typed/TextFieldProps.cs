using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TextFieldProps : BaseProps
    {
        public string Value { get; set; }
        public bool? Multiline { get; set; }
        public bool? Password { get; set; }
        public bool? ReadOnly { get; set; }
        public int? MaxLength { get; set; }
        public string Placeholder { get; set; }
        public bool? HidePlaceholderOnFocus { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> TextElement { get; set; }

        public ChangeEventHandler<string> OnChange { get; set; }
        public ChangeEventHandler<string> OnChangeCapture { get; set; }

        public string LabelText { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not TextFieldProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (Multiline != o.Multiline)
                return false;
            if (Password != o.Password)
                return false;
            if (ReadOnly != o.ReadOnly)
                return false;
            if (MaxLength != o.MaxLength)
                return false;
            if (Placeholder != o.Placeholder)
                return false;
            if (HidePlaceholderOnFocus != o.HidePlaceholderOnFocus)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(Input, o.Input))
                return false;
            if (!ReferenceEquals(TextElement, o.TextElement))
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (LabelText != o.LabelText)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
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
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                dict["onChangeCapture"] = OnChangeCapture;
            }
            if (!string.IsNullOrEmpty(LabelText))
            {
                dict["label"] = LabelText;
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
            return dict;
        }

        internal override void __ResetFields()
        {
            Value = null;
            Multiline = null;
            Password = null;
            ReadOnly = null;
            MaxLength = null;
            Placeholder = null;
            HidePlaceholderOnFocus = null;
            Label = null;
            Input = null;
            TextElement = null;
            OnChange = null;
            OnChangeCapture = null;
            LabelText = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<TextFieldProps>.Return(this);
        }
    }
}
