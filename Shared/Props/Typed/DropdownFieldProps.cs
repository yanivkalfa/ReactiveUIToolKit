using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class DropdownFieldProps : BaseProps
    {
        public List<string> Choices { get; set; }
        public string Value { get; set; }
        public int? SelectedIndex { get; set; }

        public ChangeEventHandler<string> OnChange { get; set; }
        public ChangeEventHandler<string> OnChangeCapture { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not DropdownFieldProps o)
                return false;
            if (!ReferenceEquals(Choices, o.Choices))
                return false;
            if (Value != o.Value)
                return false;
            if (SelectedIndex != o.SelectedIndex)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(VisualInput, o.VisualInput))
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Choices != null)
            {
                dict["choices"] = Choices;
            }
            if (Value != null)
            {
                dict["value"] = Value;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                dict["onChangeCapture"] = OnChangeCapture;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (VisualInput != null)
            {
                dict["visualInput"] = VisualInput;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Choices = null;
            Value = null;
            SelectedIndex = null;
            OnChange = null;
            OnChangeCapture = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<DropdownFieldProps>.Return(this);
        }
    }
}
