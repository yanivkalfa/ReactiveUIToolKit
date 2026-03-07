using System;
using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ButtonProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public bool? Enabled { get; set; }
        public Action OnClick { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                dict["text"] = Text;
            }
            if (Enabled.HasValue)
            {
                dict["enabled"] = Enabled.Value;
            }
            if (OnClick != null)
            {
                dict["onClick"] = OnClick;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            if (Ref != null)
            {
                dict["ref"] = Ref;
            }
            return dict;
        }
    }
}
