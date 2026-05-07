using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class BoxProps : BaseProps
    {
        public override bool ShallowEquals(BaseProps other)
        {
            return base.ShallowEquals(other) && other is BoxProps;
        }

        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();

        internal override void __ResetFields() { }

        internal override void __ReturnToPool()
        {
            Pool<BoxProps>.Return(this);
        }
    }
}
