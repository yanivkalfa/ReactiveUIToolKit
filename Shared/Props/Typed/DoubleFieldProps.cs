using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class DoubleFieldProps : BaseProps
    {
        public double? Value { get; set; }
        public ChangeEventHandler<double> OnChange { get; set; }
        public ChangeEventHandler<double> OnChangeCapture { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not DoubleFieldProps o)
                return false;
            if (Value != o.Value)
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
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (OnChangeCapture != null)
                map["onChangeCapture"] = OnChangeCapture;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            OnChange = null;
            OnChangeCapture = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<DoubleFieldProps>.Return(this);
        }
    }
}
