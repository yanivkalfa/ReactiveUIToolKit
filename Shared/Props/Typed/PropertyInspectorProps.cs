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
    }

    public sealed class InspectorElementProps : BaseProps
    {
        public Object Target { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Target != null)
                map["target"] = Target;
            return map;
        }
    }
}
