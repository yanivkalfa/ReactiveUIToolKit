using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollViewProps : BaseProps
    {
        public string Mode { get; set; }
        public ScrollerVisibility? VerticalScrollerVisibility { get; set; }
        public ScrollerVisibility? HorizontalScrollerVisibility { get; set; }
        public Vector2? ScrollOffset { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (!string.IsNullOrEmpty(Mode))
            {
                dict["mode"] = Mode;
            }
            if (VerticalScrollerVisibility.HasValue)
            {
                dict["verticalScrollerVisibility"] = VerticalScrollerVisibility.Value;
            }
            if (HorizontalScrollerVisibility.HasValue)
            {
                dict["horizontalScrollerVisibility"] = HorizontalScrollerVisibility.Value;
            }
            if (ScrollOffset.HasValue)
            {
                dict["scrollOffset"] = ScrollOffset.Value;
            }
            return dict;
        }
    }
}
