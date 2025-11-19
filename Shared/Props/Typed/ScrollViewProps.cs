using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ScrollViewProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Mode { get; set; }
        public ScrollerVisibility? VerticalScrollerVisibility { get; set; }
        public ScrollerVisibility? HorizontalScrollerVisibility { get; set; }
        public Vector2? ScrollOffset { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ContentContainer { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
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
            if (ContentContainer != null)
            {
                dict["contentContainer"] = ContentContainer;
            }
            if (Style != null)
            {
                dict["style"] = Style;
            }
            if (Ref != null)
            {
                dict["ref"] = Ref;
            }
            return dict;
        }
    }
}
