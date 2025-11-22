using System.Collections.Generic;
using UnityEngine.UIElements;
using ReactiveUITK.Elements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Host configuration - handles platform-specific operations
    /// Uses Element Registry adapters for proper element creation and property application
    /// </summary>
    public class FiberHostConfig
    {
        private readonly ElementRegistry _registry;

        public FiberHostConfig(ElementRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// Create a VisualElement of the given type using the adapter registry
        /// </summary>
        public VisualElement CreateElement(string elementType)
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter != null)
            {
                return adapter.Create();
            }

            // Fallback to generic VisualElement
            return new VisualElement { name = elementType };
        }

        /// <summary>
        /// Apply properties to an element using the adapter
        /// </summary>
        public void ApplyProperties(
            VisualElement element,
            string elementType,
            IReadOnlyDictionary<string, object> oldProps,
            IReadOnlyDictionary<string, object> newProps)
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter != null && newProps != null)
            {
                if (oldProps != null)
                {
                    adapter.ApplyPropertiesDiff(element, oldProps, newProps);
                }
                else
                {
                    adapter.ApplyProperties(element, newProps);
                }
            }
        }

        /// <summary>
        /// Append child to parent
        /// </summary>
        public void AppendChild(VisualElement parent, VisualElement child)
        {
            parent.Add(child);
        }

        /// <summary>
        /// Insert child before reference child
        /// </summary>
        public void InsertBefore(VisualElement parent, VisualElement child, VisualElement beforeChild)
        {
            if (beforeChild == null)
            {
                parent.Add(child);
            }
            else
            {
                int index = parent.IndexOf(beforeChild);
                if (index >= 0)
                {
                    parent.Insert(index, child);
                }
                else
                {
                    parent.Add(child);
                }
            }
        }

        /// <summary>
        /// Remove child from parent
        /// </summary>
        public void RemoveChild(VisualElement parent, VisualElement child)
        {
            if (child != null && child.parent == parent)
            {
                parent.Remove(child);
            }
        }

        /// <summary>
        /// Get parent element
        /// </summary>
        public VisualElement GetParent(VisualElement element)
        {
            return element?.parent;
        }

        /// <summary>
        /// Clear all children
        /// </summary>
        public void ClearChildren(VisualElement element)
        {
            element?.Clear();
        }
    }
}
