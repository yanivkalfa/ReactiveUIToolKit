#if UNITY_EDITOR
using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TwoPaneSplitViewElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new TwoPaneSplitView();

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            return element; // children are added directly in order
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TwoPaneSplitView split || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<int>(properties, "fixedPaneIndex", v => split.fixedPaneIndex = v);
            TryApplyProp<float>(
                properties,
                "fixedPaneInitialDimension",
                v => split.fixedPaneInitialDimension = v
            );
            if (properties.TryGetValue("orientation", out var o) && o is string s)
            {
                split.orientation =
                    s.Equals("vertical", System.StringComparison.OrdinalIgnoreCase)
                        ? TwoPaneSplitViewOrientation.Vertical
                        : TwoPaneSplitViewOrientation.Horizontal;
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TwoPaneSplitView split)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<int>(previous, next, "fixedPaneIndex", v => split.fixedPaneIndex = v);
            TryDiffProp<float>(
                previous,
                next,
                "fixedPaneInitialDimension",
                v => split.fixedPaneInitialDimension = v
            );
            if (next.TryGetValue("orientation", out var o) && o is string s)
            {
                split.orientation =
                    s == "vertical"
                        ? TwoPaneSplitViewOrientation.Vertical
                        : TwoPaneSplitViewOrientation.Horizontal;
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
#endif
