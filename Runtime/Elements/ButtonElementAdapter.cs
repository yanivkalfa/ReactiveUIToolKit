using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Props;

namespace ReactiveUITK.Elements
{
    public sealed class ButtonElementAdapter : IElementAdapter
    {
        public VisualElement Create()
        {
            return new Button();
        }

        public void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is Button button && properties != null)
            {
                if (properties.TryGetValue("text", out var textObj) && textObj is string txt)
                {
                    button.text = txt;
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is Button button)
            {
                previous ??= new Dictionary<string, object>();
                next ??= new Dictionary<string, object>();
                string prevText = previous.TryGetValue("text", out var p) && p is string ps ? ps : null;
                string nextText = next.TryGetValue("text", out var n) && n is string ns ? ns : null;
                if (prevText != nextText)
                {
                    button.text = nextText ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
