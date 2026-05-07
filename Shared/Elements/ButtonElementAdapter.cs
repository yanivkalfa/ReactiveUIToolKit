using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ButtonElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Button();
        }

        private static bool IsInsideListView(VisualElement ve)
        {
            var p = ve.parent;
            while (p != null)
            {
                if (p is BaseVerticalCollectionView)
                {
                    return true;
                }
                p = p.parent;
            }
            return false;
        }

        private static void EnsureListViewFriendly(Button button)
        {
            if (button == null)
            {
                return;
            }
            if (!IsInsideListView(button))
            {
                return;
            }

            button.focusable = false;
            button.pickingMode = PickingMode.Position;
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Button button && properties != null)
            {
                if (properties.TryGetValue("text", out var textObj) && textObj is string txt)
                {
                    button.text = txt;
                }
                if (properties.TryGetValue("enabled", out var enabledObj) && enabledObj is bool en)
                {
                    button.SetEnabled(en);
                }
                EnsureListViewFriendly(button);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Button button)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;
                string prevText =
                    previous.TryGetValue("text", out var p) && p is string ps ? ps : null;
                string nextText = next.TryGetValue("text", out var n) && n is string ns ? ns : null;
                if (prevText != nextText)
                {
                    button.text = nextText ?? string.Empty;
                }
                bool prevEnabled =
                    previous.TryGetValue("enabled", out var pe) && pe is bool peb ? peb : true;
                bool nextEnabled =
                    next.TryGetValue("enabled", out var ne) && ne is bool neb ? neb : true;
                if (prevEnabled != nextEnabled)
                {
                    button.SetEnabled(nextEnabled);
                }
                EnsureListViewFriendly(button);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Button button && props is ButtonProps bp)
            {
                if (bp.Text != null)
                    button.text = bp.Text;
                EnsureListViewFriendly(button);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Button button && prev is ButtonProps bp && next is ButtonProps bn)
            {
                if (bp.Text != bn.Text)
                    button.text = bn.Text ?? string.Empty;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
