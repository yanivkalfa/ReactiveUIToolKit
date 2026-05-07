using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class LabelProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not LabelProps o)
                return false;
            if (Text != o.Text)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Text != null)
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
            Pool<LabelProps>.Return(this);
        }
    }
}
