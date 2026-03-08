using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class LabelProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Text != null)
            {
                map["text"] = Text;
            }
            return map;
        }
    }
}
