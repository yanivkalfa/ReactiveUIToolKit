using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not TabProps o)
                return false;
            if (Text != o.Text)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
            {
                d["text"] = Text;
            }
            return d;
        }

        internal override void __ResetFields()
        {
            Text = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<TabProps>.Return(this);
        }
    }
}
