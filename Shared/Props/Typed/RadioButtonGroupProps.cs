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
    }
}
