using System;
using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class FoldoutProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public Action<UnityEngine.UIElements.ChangeEvent<bool>> OnChange { get; set; }
        public Dictionary<string, object> Header { get; set; }

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
            if (Header != null)
            {
                dict["header"] = Header;
            }
            return dict;
        }
    }
}
