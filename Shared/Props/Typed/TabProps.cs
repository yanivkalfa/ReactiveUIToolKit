using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabProps : global::ReactiveUITK.Core.IProps
    {
        public string Text { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Text))
            {
                d["text"] = Text;
            }
            if (Style != null)
            {
                d["style"] = Style;
            }
            if (Ref != null)
            {
                d["ref"] = Ref;
            }
            return d;
        }
    }
}
