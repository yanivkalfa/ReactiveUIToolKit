using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class GroupBoxProps : BaseProps
    {
        public string Text { get; set; }
        public Dictionary<string, object> Label { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            if (Label != null)
            {
                map["label"] = Label;
            }
            return map;
        }
    }
}
