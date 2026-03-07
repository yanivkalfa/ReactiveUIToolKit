using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class HelpBoxProps : BaseProps
    {
        public string Text { get; set; }
        public string MessageType { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Text != null)
            {
                dict["text"] = Text;
            }
            if (!string.IsNullOrEmpty(MessageType))
            {
                dict["messageType"] = MessageType;
            }
            return dict;
        }
    }
}
