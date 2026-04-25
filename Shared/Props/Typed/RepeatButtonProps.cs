using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RepeatButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not RepeatButtonProps o)
                return false;
            if (Text != o.Text)
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
            return map;
        }

        internal override void __ResetFields()
        {
            Text = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<RepeatButtonProps>.Return(this);
        }
    }
}
