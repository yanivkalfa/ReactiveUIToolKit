using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RepeatButtonProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public System.Action OnClick { get; set; }
        public Style Style { get; set; }
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
            if (OnClick != null)
            {
                map["onClick"] = OnClick;
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
