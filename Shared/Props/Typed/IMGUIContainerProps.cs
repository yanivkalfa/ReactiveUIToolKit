using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class IMGUIContainerProps : BaseProps
    {
        public Action OnGUI { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (OnGUI != null)
                map["onGUI"] = OnGUI;
            return map;
        }
    }
}
