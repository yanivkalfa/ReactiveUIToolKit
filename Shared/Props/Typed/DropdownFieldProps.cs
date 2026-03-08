using System;
using System.Collections.Generic;

using ReactiveUITK.Core;
namespace ReactiveUITK.Props.Typed
{
    public sealed class DropdownFieldProps : BaseProps
    {
        public List<string> Choices { get; set; }
        public string Value { get; set; }
        public int? SelectedIndex { get; set; }

        public ChangeEventHandler<string> OnChange { get; set; }

        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Choices != null)
            {
                dict["choices"] = Choices;
            }
            if (Value != null)
            {
                dict["value"] = Value;
            }
            if (SelectedIndex.HasValue)
            {
                dict["selectedIndex"] = SelectedIndex.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (Label != null)
            {
                dict["label"] = Label;
            }
            if (VisualInput != null)
            {
                dict["visualInput"] = VisualInput;
            }
            return dict;
        }
    }
}
