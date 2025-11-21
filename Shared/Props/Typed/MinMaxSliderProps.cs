using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class MinMaxSliderProps
    {
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }
        public float? LowLimit { get; set; }
        public float? HighLimit { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (MinValue.HasValue) map["minValue"] = MinValue.Value;
            if (MaxValue.HasValue) map["maxValue"] = MaxValue.Value;
            if (LowLimit.HasValue) map["lowLimit"] = LowLimit.Value;
            if (HighLimit.HasValue) map["highLimit"] = HighLimit.Value;
            if (Style != null) map["style"] = Style;
            if (Ref != null) map["ref"] = Ref;
            return map;
        }
    }
}

