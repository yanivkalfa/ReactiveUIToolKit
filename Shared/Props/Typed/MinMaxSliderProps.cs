using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MinMaxSliderProps : BaseProps
    {
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }
        public float? LowLimit { get; set; }
        public float? HighLimit { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not MinMaxSliderProps o)
                return false;
            if (MinValue != o.MinValue)
                return false;
            if (MaxValue != o.MaxValue)
                return false;
            if (LowLimit != o.LowLimit)
                return false;
            if (HighLimit != o.HighLimit)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (MinValue.HasValue)
                map["minValue"] = MinValue.Value;
            if (MaxValue.HasValue)
                map["maxValue"] = MaxValue.Value;
            if (LowLimit.HasValue)
                map["lowLimit"] = LowLimit.Value;
            if (HighLimit.HasValue)
                map["highLimit"] = HighLimit.Value;
            return map;
        }

        internal override void __ResetFields()
        {
            MinValue = null;
            MaxValue = null;
            LowLimit = null;
            HighLimit = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<MinMaxSliderProps>.Return(this);
        }
    }
}
