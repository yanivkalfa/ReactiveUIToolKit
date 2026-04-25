using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class FoldoutProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public ChangeEventHandler<bool> OnChangeCapture { get; set; }
        public Dictionary<string, object> Header { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not FoldoutProps o)
                return false;
            if (Text != o.Text)
                return false;
            if (Value != o.Value)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            if (!ReferenceEquals(Header, o.Header))
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
            if (Value.HasValue)
            {
                dict["value"] = Value.Value;
            }
            if (OnChange != null)
            {
                dict["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                dict["onChangeCapture"] = OnChangeCapture;
            }
            if (Header != null)
            {
                dict["header"] = Header;
            }
            return dict;
        }

        internal override void __ResetFields()
        {
            Text = null;
            Value = null;
            OnChange = null;
            OnChangeCapture = null;
            Header = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<FoldoutProps>.Return(this);
        }
    }
}
