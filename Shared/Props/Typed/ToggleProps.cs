using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public bool? Value { get; set; }
        public string Text { get; set; }
        public Style Style { get; set; }
        public System.Action<ChangeEvent<bool>> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Checkmark { get; set; }
    public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            if (!string.IsNullOrEmpty(Name))
            {
                map["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                map["className"] = ClassName;
            }
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            if (Input != null)
            {
                map["input"] = Input;
            }
            if (Checkmark != null)
            {
                map["checkmark"] = Checkmark;
            }
            if (Style != null)
            {
                map["style"] = Style;
            }
            if (Ref != null)
            {
                map["ref"] = Ref;
            }
            return map;
        }
    }
}
