using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class IMGUIContainerProps : BaseProps
    {
        public Action OnGUI { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not IMGUIContainerProps o)
                return false;
            if (OnGUI != o.OnGUI)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (OnGUI != null)
                map["onGUI"] = OnGUI;
            return map;
        }

        internal override void __ResetFields()
        {
            OnGUI = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<IMGUIContainerProps>.Return(this);
        }
    }
}
