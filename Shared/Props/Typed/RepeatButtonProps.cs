using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RepeatButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                map["text"] = Text;
            }
            return map;
        }
    }
}
