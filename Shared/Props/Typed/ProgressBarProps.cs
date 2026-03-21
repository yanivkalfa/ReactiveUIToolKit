using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ProgressBarProps : BaseProps
    {
        public float? Value { get; set; }
        public string Title { get; set; }
        public Dictionary<string, object> Progress { get; set; }
        public Dictionary<string, object> TitleElement { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
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
            return map;
        }
    }
}
