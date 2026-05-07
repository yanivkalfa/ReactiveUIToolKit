using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollerProps : BaseProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ScrollerProps o)
                return false;
            if (LowValue != o.LowValue)
                return false;
            if (HighValue != o.HighValue)
                return false;
            if (Value != o.Value)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (LowValue.HasValue)
                map["lowValue"] = LowValue.Value;
            if (HighValue.HasValue)
                map["highValue"] = HighValue.Value;
            if (Value.HasValue)
                map["value"] = Value.Value;
            return map;
        }

        internal override void __ResetFields()
        {
            LowValue = null;
            HighValue = null;
            Value = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ScrollerProps>.Return(this);
        }
    }
}
