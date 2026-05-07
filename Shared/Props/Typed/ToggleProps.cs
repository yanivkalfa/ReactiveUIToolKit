using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleProps : BaseProps
    {
        public bool? Value { get; set; }
        public string Text { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Checkmark { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToggleProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (Text != o.Text)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(Input, o.Input))
                return false;
            if (!ReferenceEquals(Checkmark, o.Checkmark))
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
            if (Label != null)
            {
                map["label"] = Label;
            }
            if (Input != null)
            {
                map["input"] = Input;
            }
            if (Checkmark != null)
            {
                map["checkmark"] = Checkmark;
            }
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            Text = null;
            OnChange = null;
            Label = null;
            Input = null;
            Checkmark = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToggleProps>.Return(this);
        }
    }
}
