using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabProps
    {
        public string Text { get; set; }
        public Style Style { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Text)) d["text"] = Text;
            if (Style != null) d["style"] = Style;
            return d;
        }
    }
}

