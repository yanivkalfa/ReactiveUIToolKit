using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class SliderIntProps : BaseProps
    {
        public int? LowValue { get; set; }
        public int? HighValue { get; set; }
        public int? Value { get; set; }
        public string Direction { get; set; }

        public ChangeEventHandler<int> OnChange { get; set; }
        public ChangeEventHandler<int> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not SliderIntProps o)
                return false;
            if (LowValue != o.LowValue)
                return false;
            if (HighValue != o.HighValue)
                return false;
            if (Value != o.Value)
                return false;
            if (Direction != o.Direction)
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
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<SliderIntProps>.Return(this);
        }
    }
}
