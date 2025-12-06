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
        public string Direction { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        // Optional slot-style props for inner parts of the slider.
        // These maps can contain "style", "className", etc., which are
        // applied directly to the corresponding UI Toolkit elements.
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Track { get; set; }
        public Dictionary<string, object> DragContainer { get; set; }
        public Dictionary<string, object> Handle { get; set; }
        public Dictionary<string, object> HandleBorder { get; set; }

        public Action<ChangeEvent<float>> OnChange { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
            if (LowValue.HasValue)
            {
                dict["lowValue"] = LowValue.Value;
            }
            if (HighValue.HasValue)
            {
                dict["highValue"] = HighValue.Value;
            }
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Direction))
            {
                dict["direction"] = Direction;
            }
            if (Input != null)
            {
                dict["input"] = Input;
            }
            if (Track != null)
            {
                dict["track"] = Track;
            }
            if (DragContainer != null)
            {
                dict["dragContainer"] = DragContainer;
            }
            if (Handle != null)
            {
                dict["handle"] = Handle;
            }
            if (HandleBorder != null)
            {
                dict["handleBorder"] = HandleBorder;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            if (Ref != null)
            {
                dict["ref"] = Ref;
            }
            return dict;
        }
    }
}
