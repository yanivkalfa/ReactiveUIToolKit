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
    }
}
