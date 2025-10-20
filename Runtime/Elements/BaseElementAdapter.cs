using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public abstract class BaseElementAdapter : IElementAdapter
    {
        public abstract VisualElement Create();
        public virtual void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties) { }
        public virtual void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next) { }

        protected bool TryApplyProp<T>(IReadOnlyDictionary<string, object> properties, string key, Action<T> assign)
        {
            if (properties == null)
            {
                return false;
            }
            if (!properties.TryGetValue(key, out var raw))
            {
                return false;
            }
            if (raw is T typed)
            {
                assign?.Invoke(typed);
                return true;
            }
            return false;
        }

        protected bool TryDiffProp<T>(IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next, string key, Action<T> assign)
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue(key, out var prevRaw);
            next.TryGetValue(key, out var nextRaw);
            if (ReferenceEquals(prevRaw, nextRaw))
            {
                return false;
            }
            if (nextRaw is T typed)
            {
                assign?.Invoke(typed);
                return true;
            }
            return false;
        }
    }
}
