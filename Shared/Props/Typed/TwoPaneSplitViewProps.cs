#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TwoPaneSplitViewProps : BaseProps
    {
        public string Orientation { get; set; } // "horizontal" | "vertical"
        public int? FixedPaneIndex { get; set; }
        public float? FixedPaneInitialDimension { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Orientation))
                map["orientation"] = Orientation;
            if (FixedPaneIndex.HasValue)
                map["fixedPaneIndex"] = FixedPaneIndex.Value;
            if (FixedPaneInitialDimension.HasValue)
                map["fixedPaneInitialDimension"] = FixedPaneInitialDimension.Value;
            return map;
        }
    }
}
#endif
