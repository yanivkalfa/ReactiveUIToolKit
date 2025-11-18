using System;
using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class FoldoutProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public bool? Value { get; set; }
        public Style Style { get; set; }
    public object Ref { get; set; }

        public Action<UnityEngine.UIElements.ChangeEvent<bool>> OnChange { get; set; }

        // Slots
        public Dictionary<string, object> ContentContainer { get; set; }
        public Dictionary<string, object> Header { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                dict["className"] = ClassName;
            if (Text != null)
                dict["text"] = Text;
            if (Value.HasValue)
                dict["value"] = Value.Value;
            if (OnChange != null)
                dict["onChange"] = OnChange;
            if (ContentContainer != null)
                dict["contentContainer"] = ContentContainer;
            if (Header != null)
                dict["header"] = Header;
            if (Style != null)
                dict["style"] = Style;
            if (Ref != null)
                dict["ref"] = Ref;
            return dict;
        }
    }
}
