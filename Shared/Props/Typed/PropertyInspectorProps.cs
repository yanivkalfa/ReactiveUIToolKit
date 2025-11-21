using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class PropertyFieldProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Object Target { get; set; }
        public string BindingPath { get; set; }
        public string Label { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }
        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name)) map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName)) map["className"] = ClassName;
            if (Target != null) map["target"] = Target;
            if (!string.IsNullOrEmpty(BindingPath)) map["bindingPath"] = BindingPath;
            if (!string.IsNullOrEmpty(Label)) map["label"] = Label;
            if (Style != null) map["style"] = Style;
            if (Ref != null) map["ref"] = Ref;
            return map;
        }
    }

    public sealed class InspectorElementProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Object Target { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }
        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name)) map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName)) map["className"] = ClassName;
            if (Target != null) map["target"] = Target;
            if (Style != null) map["style"] = Style;
            if (Ref != null) map["ref"] = Ref;
            return map;
        }
    }
}

