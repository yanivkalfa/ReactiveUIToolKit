using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class HelpBoxProps : BaseProps
    {
        public string Text { get; set; }
        public string MessageType { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not HelpBoxProps o)
                return false;
            if (Text != o.Text)
                return false;
            if (MessageType != o.MessageType)
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Text = null;
            MessageType = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<HelpBoxProps>.Return(this);
        }
    }
}
