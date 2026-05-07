using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleButtonGroupProps : BaseProps
    {
        public int? Value { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToggleButtonGroupProps o)
                return false;
            if (Value != o.Value)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value.HasValue)
                map["value"] = Value.Value;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToggleButtonGroupProps>.Return(this);
        }
    }
}
