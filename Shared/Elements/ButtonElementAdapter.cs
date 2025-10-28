using System.Collections.Generic;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ButtonElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<Button>();
        }

        private static bool IsInsideListView(VisualElement ve)
        {
            var p = ve.parent;
            while (p != null)
            {
                if (p is BaseVerticalCollectionView)
                    return true;
                p = p.parent;
            }
            return false;
        }

        private static void EnsureListViewFriendly(Button button)
        {
            if (button == null)
                return;
            if (!IsInsideListView(button))
                return;
            // Make button act independent inside ListView rows; rely on selectionType=None
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
                previous ??= new Dictionary<string, object>();
                next ??= new Dictionary<string, object>();
                string prevText =
                    previous.TryGetValue("text", out var p) && p is string ps ? ps : null;
                string nextText = next.TryGetValue("text", out var n) && n is string ns ? ns : null;
                if (prevText != nextText)
                {
                    button.text = nextText ?? string.Empty;
                }
                EnsureListViewFriendly(button);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
