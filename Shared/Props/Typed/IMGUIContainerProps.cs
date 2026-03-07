using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class IMGUIContainerProps : global::ReactiveUITK.Core.IProps
    {
        public Action OnGUI { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (OnGUI != null)
                map["onGUI"] = OnGUI;
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }
}
