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

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ObjectFieldProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (ObjectType != o.ObjectType)
                return false;
            if (AllowSceneObjects != o.AllowSceneObjects)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            if (!ReferenceEquals(VisualInput, o.VisualInput))
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Value = null;
            ObjectType = null;
            AllowSceneObjects = null;
            Label = null;
            VisualInput = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ObjectFieldProps>.Return(this);
        }
    }
}
