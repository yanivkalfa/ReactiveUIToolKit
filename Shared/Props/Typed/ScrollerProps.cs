using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollerProps : global::ReactiveUITK.Core.IProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (LowValue.HasValue)
                map["lowValue"] = LowValue.Value;
            if (HighValue.HasValue)
                map["highValue"] = HighValue.Value;
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }
}
