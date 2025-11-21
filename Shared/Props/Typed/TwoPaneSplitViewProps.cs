#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TwoPaneSplitViewProps
    {
        public string Orientation { get; set; } // "horizontal" | "vertical"
        public int? FixedPaneIndex { get; set; }
        public float? FixedPaneInitialDimension { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Orientation)) map["orientation"] = Orientation;
            if (FixedPaneIndex.HasValue) map["fixedPaneIndex"] = FixedPaneIndex.Value;
            if (FixedPaneInitialDimension.HasValue) map["fixedPaneInitialDimension"] = FixedPaneInitialDimension.Value;
            if (Style != null) map["style"] = Style;
            if (Ref != null) map["ref"] = Ref;
            return map;
        }
    }
}
#endif

