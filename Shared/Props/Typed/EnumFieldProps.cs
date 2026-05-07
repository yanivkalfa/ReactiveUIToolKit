using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class EnumFieldProps : BaseProps
    {
        public Enum Value { get; set; }
        public string EnumType { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not EnumFieldProps o)
                return false;
            if (!object.Equals(Value, o.Value))
                return false;
            if (EnumType != o.EnumType)
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
            if (Value != null)
                map["value"] = Value;
            if (!string.IsNullOrEmpty(EnumType))
                map["enumType"] = EnumType;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            EnumType = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<EnumFieldProps>.Return(this);
        }
    }
}
