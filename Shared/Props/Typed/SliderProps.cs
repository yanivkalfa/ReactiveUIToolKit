using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class SliderProps : BaseProps
    {
        public float? LowValue { get; set; }
        public float? HighValue { get; set; }
        public float? Value { get; set; }
        public string Direction { get; set; }

        // Optional slot-style props for inner parts of the slider.
        // These maps can contain "style", "className", etc., which are
        // applied directly to the corresponding UI Toolkit elements.
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Track { get; set; }
        public Dictionary<string, object> DragContainer { get; set; }
        public Dictionary<string, object> Handle { get; set; }
        public Dictionary<string, object> HandleBorder { get; set; }

        public ChangeEventHandler<float> OnChange { get; set; }
        public ChangeEventHandler<float> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not SliderProps o)
                return false;
            if (LowValue != o.LowValue)
                return false;
            if (HighValue != o.HighValue)
                return false;
            if (Value != o.Value)
                return false;
            if (Direction != o.Direction)
                return false;
            if (!ReferenceEquals(Input, o.Input))
                return false;
            if (!ReferenceEquals(Track, o.Track))
                return false;
            if (!ReferenceEquals(DragContainer, o.DragContainer))
                return false;
            if (!ReferenceEquals(Handle, o.Handle))
                return false;
            if (!ReferenceEquals(HandleBorder, o.HandleBorder))
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
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
            if (OnChangeCapture != null)
            {
                dict["onChangeCapture"] = OnChangeCapture;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            LowValue = null;
            HighValue = null;
            Value = null;
            Direction = null;
            Input = null;
            Track = null;
            DragContainer = null;
            Handle = null;
            HandleBorder = null;
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<SliderProps>.Return(this);
        }
    }
}
