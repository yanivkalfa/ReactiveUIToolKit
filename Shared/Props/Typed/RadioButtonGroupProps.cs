using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonGroupProps : BaseProps
    {
        public IList<string> Choices { get; set; }
        public string Value { get; set; }
        public int? Index { get; set; }
        public System.Action<UnityEngine.UIElements.ChangeEvent<int>> OnChange { get; set; }

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
            return map;
        }
    }
}
