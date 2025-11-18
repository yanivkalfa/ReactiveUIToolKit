using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class GroupBoxProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public Style Style { get; set; }
        public Dictionary<string, object> ContentContainer { get; set; }
        public Dictionary<string, object> Label { get; set; }
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
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (ContentContainer != null)
            {
                map["contentContainer"] = ContentContainer;
            }
            if (Label != null)
            {
                map["label"] = Label;
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
