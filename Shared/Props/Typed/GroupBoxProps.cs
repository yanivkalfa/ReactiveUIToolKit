using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class GroupBoxProps : BaseProps
    {
        public string Text { get; set; }
        public Dictionary<string, object> Label { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not GroupBoxProps o)
                return false;
            if (Text != o.Text)
                return false;
            if (!ReferenceEquals(Label, o.Label))
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Text = null;
            Label = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<GroupBoxProps>.Return(this);
        }
    }
}
