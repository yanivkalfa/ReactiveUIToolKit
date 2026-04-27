using System.Collections.Generic;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public interface IElementAdapter
    {
        VisualElement Create();
        void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> props);
        void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> prev,
            IReadOnlyDictionary<string, object> next
        );
        VisualElement ResolveChildHost(VisualElement element);
    }

    /// <summary>
    /// Typed props adapter — enables the typed pipeline for built-in host elements.
    /// All built-in adapters implement this via <see cref="BaseElementAdapter"/>.
    /// </summary>
    public interface ITypedElementAdapter
    {
        /// <summary>
        /// Apply all typed properties on initial placement (no previous state).
        /// </summary>
        void ApplyTypedFull(VisualElement element, BaseProps props);

        /// <summary>
        /// Apply only the changed typed properties.
        /// </summary>
        void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next);
    }
}
