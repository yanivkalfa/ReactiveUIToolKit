using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonProps : BaseProps
    {
        public bool? Value { get; set; }
        public string Text { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public ChangeEventHandler<bool> OnChangeCapture { get; set; }
        public Dictionary<string, object> Label { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not RadioButtonProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (Text != o.Text)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                map["onChangeCapture"] = OnChangeCapture;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            Text = null;
            OnChange = null;
            OnChangeCapture = null;
            Label = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<RadioButtonProps>.Return(this);
        }
    }
}
