using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ObjectFieldProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public UnityEngine.Object Value { get; set; }
        public string ObjectType { get; set; }
        public bool? AllowSceneObjects { get; set; }
        public Style Style { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Value != null)
                map["value"] = Value;
            if (!string.IsNullOrEmpty(ObjectType))
                map["objectType"] = ObjectType;
            if (AllowSceneObjects.HasValue)
                map["allowSceneObjects"] = AllowSceneObjects.Value;
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
