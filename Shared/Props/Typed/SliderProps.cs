using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class SliderProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }
        public string Direction { get; set; } // "horizontal" | "vertical"
        public Style Style { get; set; }

        public Action<ChangeEvent<float>> OnChange { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name)) dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName)) dict["className"] = ClassName;
            if (LowValue.HasValue) dict["lowValue"] = LowValue.Value;
            if (HighValue.HasValue) dict["highValue"] = HighValue.Value;
            if (Value.HasValue) dict["value"] = Value.Value;
            if (!string.IsNullOrEmpty(Direction)) dict["direction"] = Direction;
            if (OnChange != null) dict["onChange"] = OnChange;
            if (Style != null) dict["style"] = Style;
            return dict;
        }
    }
}

