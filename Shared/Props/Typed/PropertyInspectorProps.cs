using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class PropertyFieldProps : BaseProps
    {
        public Object Target { get; set; }
        public string BindingPath { get; set; }
        public string Label { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not PropertyFieldProps o)
                return false;
            if (Target != o.Target)
                return false;
            if (BindingPath != o.BindingPath)
                return false;
            if (Label != o.Label)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Target != null)
                map["target"] = Target;
            if (!string.IsNullOrEmpty(BindingPath))
                map["bindingPath"] = BindingPath;
            if (!string.IsNullOrEmpty(Label))
                map["label"] = Label;
            return map;
        }

        internal override void __ResetFields()
        {
            Target = null;
            BindingPath = null;
            Label = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<PropertyFieldProps>.Return(this);
        }
    }

    public sealed class InspectorElementProps : BaseProps
    {
        public Object Target { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not InspectorElementProps o)
                return false;
            if (Target != o.Target)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Target != null)
                map["target"] = Target;
            return map;
        }

        internal override void __ResetFields()
        {
            Target = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<InspectorElementProps>.Return(this);
        }
    }
}
