using System;
using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class DropdownFieldProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public List<string> Choices { get; set; }
        public string Value { get; set; }
        public int? SelectedIndex { get; set; }
        public Style Style { get; set; }

        public Action<UnityEngine.UIElements.ChangeEvent<string>> OnChange { get; set; }

        // Slots
        public Dictionary<string, object> Label { get; set; }
        public Dictionary<string, object> VisualInput { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name)) dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName)) dict["className"] = ClassName;
            if (Choices != null) dict["choices"] = Choices;
            if (Value != null) dict["value"] = Value;
            if (SelectedIndex.HasValue) dict["selectedIndex"] = SelectedIndex.Value;
            if (OnChange != null) dict["onChange"] = OnChange;
            if (Label != null) dict["label"] = Label;
            if (VisualInput != null) dict["visualInput"] = VisualInput;
            if (Style != null) dict["style"] = Style;
            return dict;
        }
    }
}
