using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                d["text"] = Text;
            }
            return d;
        }
    }
}
