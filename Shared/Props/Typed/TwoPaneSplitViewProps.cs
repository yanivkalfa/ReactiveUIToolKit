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

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not TwoPaneSplitViewProps o)
                return false;
            if (Orientation != o.Orientation)
                return false;
            if (FixedPaneIndex != o.FixedPaneIndex)
                return false;
            if (FixedPaneInitialDimension != o.FixedPaneInitialDimension)
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Orientation = null;
            FixedPaneIndex = null;
            FixedPaneInitialDimension = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<TwoPaneSplitViewProps>.Return(this);
        }
    }
}
#endif
