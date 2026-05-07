using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonGroupProps : BaseProps
    {
        public IList<string> Choices { get; set; }
        public string Value { get; set; }
        public int? Index { get; set; }
        public ChangeEventHandler<int> OnChange { get; set; }
        public ChangeEventHandler<int> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not RadioButtonGroupProps o)
                return false;
            if (!ReferenceEquals(Choices, o.Choices))
                return false;
            if (Value != o.Value)
                return false;
            if (Index != o.Index)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Choices != null)
            {
                map["choices"] = Choices;
            }
            if (!string.IsNullOrEmpty(Value))
            {
                map["value"] = Value;
            }
            if (Index.HasValue)
            {
                map["index"] = Index.Value;
            }
            if (OnChange != null)
            {
                map["onChange"] = OnChange;
            }
            if (OnChangeCapture != null)
            {
                map["onChangeCapture"] = OnChangeCapture;
            }
            return map;
        }

        internal override void __ResetFields()
        {
            Choices = null;
            Value = null;
            Index = null;
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<RadioButtonGroupProps>.Return(this);
        }
    }
}
