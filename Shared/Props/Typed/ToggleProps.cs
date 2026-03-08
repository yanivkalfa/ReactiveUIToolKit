using System.Collections.Generic;
using UnityEngine.UIElements;

using ReactiveUITK.Core;
namespace ReactiveUITK.Props.Typed
{
    public sealed class ToggleProps : BaseProps
    {
        public bool? Value { get; set; }
        public string Text { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public Dictionary<string, object> Checkmark { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
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
            return map;
        }
    }
}
