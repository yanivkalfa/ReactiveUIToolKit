using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ButtonProps o)
                return false;
            if (Text != o.Text)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                dict["text"] = Text;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Text = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ButtonProps>.Return(this);
        }
    }
}
