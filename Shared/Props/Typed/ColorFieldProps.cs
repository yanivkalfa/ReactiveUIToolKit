using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ColorFieldProps : BaseProps
    {
        public Color? Value { get; set; }
        public Action<ChangeEvent<Color>> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            return map;
        }
    }
}
