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

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ScrollViewProps o)
                return false;
            if (Mode != o.Mode)
                return false;
            if (VerticalScrollerVisibility != o.VerticalScrollerVisibility)
                return false;
            if (HorizontalScrollerVisibility != o.HorizontalScrollerVisibility)
                return false;
            if (ScrollOffset != o.ScrollOffset)
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Mode = null;
            VerticalScrollerVisibility = null;
            HorizontalScrollerVisibility = null;
            ScrollOffset = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ScrollViewProps>.Return(this);
        }
    }
}
