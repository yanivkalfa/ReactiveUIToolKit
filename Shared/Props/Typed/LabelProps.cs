using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class LabelProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public Style Style { get; set; }

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
            if (Text != null)
            {
                map["text"] = Text;
            }
            if (Style != null)
            {
                map["style"] = Style;
            }
            return map;
        }
    }
}
