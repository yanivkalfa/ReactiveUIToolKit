using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TextElementProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not TextElementProps o)
                return false;
            if (Text != o.Text)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            return map;
        }

        internal override void __ResetFields()
        {
            Text = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<TextElementProps>.Return(this);
        }
    }
}
