using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoundsFieldProps : BaseProps
    {
        public Bounds? Value { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not BoundsFieldProps o)
                return false;
            if (Value != o.Value)
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
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<BoundsFieldProps>.Return(this);
        }
    }
}
