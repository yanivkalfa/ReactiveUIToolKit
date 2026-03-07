using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class RadioButtonGroupProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IList<string> Choices { get; set; }
        public string Value { get; set; }
        public int? Index { get; set; }
        public Style Style { get; set; }
        public Dictionary<string, object> ContentContainer { get; set; }
        public System.Action<UnityEngine.UIElements.ChangeEvent<int>> OnChange { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            if (!string.IsNullOrEmpty(Name))
            {
                map["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                map["className"] = ClassName;
            }
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
            if (ContentContainer != null)
            {
                map["contentContainer"] = ContentContainer;
            }
            if (Style != null)
            {
                map["style"] = Style;
            }
            if (Ref != null)
            {
                map["ref"] = Ref;
            }
            return map;
        }
    }
}
