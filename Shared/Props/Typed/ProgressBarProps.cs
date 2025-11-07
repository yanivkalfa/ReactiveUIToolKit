using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ProgressBarProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public float? Value { get; set; }
        public string Title { get; set; }
        public Style Style { get; set; }
        public Dictionary<string, object> Progress { get; set; }
        public Dictionary<string, object> TitleElement { get; set; }
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
            if (!string.IsNullOrEmpty(Title))
            {
                map["title"] = Title;
            }
            if (Progress != null)
            {
                map["progress"] = Progress;
            }
            if (TitleElement != null)
            {
                map["titleElement"] = TitleElement;
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
