using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class HelpBoxProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public string MessageType { get; set; } // "info" | "warning" | "error"
        public Style Style { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name)) dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName)) dict["className"] = ClassName;
            if (Text != null) dict["text"] = Text;
            if (!string.IsNullOrEmpty(MessageType)) dict["messageType"] = MessageType;
            if (Style != null) dict["style"] = Style;
            return dict;
        }
    }
}

