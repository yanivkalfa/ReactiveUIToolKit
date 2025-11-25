using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoundsFieldProps
    {
        public Bounds? Value { get; set; }
        public Style Style { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (Label != null)
                map["label"] = Label;
            if (VisualInput != null)
                map["visualInput"] = VisualInput;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }
}
