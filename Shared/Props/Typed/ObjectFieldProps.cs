using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ObjectFieldProps : BaseProps
    {
        public UnityEngine.Object Value { get; set; }
        public string ObjectType { get; set; }
        public bool? AllowSceneObjects { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
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
            return map;
        }
    }
}
