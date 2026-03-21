using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollerProps : BaseProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }

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
    }
}
